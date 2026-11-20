using System;
using System.Collections.Generic;
using System.Numerics;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>Auras (11-combat.md §3.6 and §4.4): putting them on and refreshing them, their ticks and saves, using them up, cleansing, concentration, what the conditions mean, and adding up what everything on someone changes.</summary>
public sealed partial class Battle
{
    /// <summary>How long a knockdown keeps someone on the ground.</summary>
    public const int KnockdownMs = 1500;

    /// <summary>How long temporary hit points last when nothing says.</summary>
    private const int DefaultShieldMs = 30000;

    // What's on someone.

    /// <summary>Whether a condition is on someone.</summary>
    public bool Has(Fighter f, ConditionId condition)
    {
        foreach (Aura aura in f.AuraList)
            if (!aura.Gone && aura.Definition.Condition == condition) return true;
        return false;
    }

    /// <summary>Whether someone is marked for the skills that care: the Marked condition, or an aura that counts as a mark (Hunter's Mark).</summary>
    public bool IsMarked(Fighter f)
    {
        foreach (Aura aura in f.AuraList)
            if (!aura.Gone && (aura.Definition.Condition == ConditionId.Marked || aura.Definition.CountsAsMarked)) return true;
        return false;
    }

    /// <summary>Whether someone is Restrained or Prone by a particular source's doing.</summary>
    private bool HeldBy(Fighter f, Fighter source)
    {
        foreach (Aura aura in f.AuraList)
            if (!aura.Gone && aura.Source == source && aura.Definition.Condition is ConditionId.Restrained or ConditionId.Prone) return true;
        return false;
    }

    /// <summary>The first aura with a condition on someone, or null.</summary>
    internal Aura First(Fighter f, ConditionId condition)
    {
        foreach (Aura aura in f.AuraList)
            if (!aura.Gone && aura.Definition.Condition == condition) return aura;
        return null;
    }

    /// <summary>Whether someone can act at all: not out, not Stunned or Frozen, not vanished.</summary>
    public bool CanAct(Fighter f) => f.IsStanding && !f.Vanished && !Has(f, ConditionId.Stunned) && !Has(f, ConditionId.Frozen);

    /// <summary>Whether someone can walk: they can act, nothing roots them (Restrained, Prone), and they aren't mid-move, casting, or helping someone up.</summary>
    public bool CanMove(Fighter f) => CanAct(f) && f.Action is null && f.Helping is null && !Has(f, ConditionId.Restrained) && !Has(f, ConditionId.Prone);

    /// <summary>Whether an aura definition changes a stat.</summary>
    private static bool HasMod(AuraDefinition aura, Stat stat)
    {
        foreach (StatMod mod in aura.Mods)
            if (mod.Stat == stat) return true;
        return false;
    }

    /// <summary>Whether someone is immune to a condition.</summary>
    internal bool Immune(Fighter f, ConditionId condition)
    {
        foreach ((StatMod mod, Fighter _) in Mods(f, Stat.Immune))
            if (mod.Condition == condition) return true;
        return false;
    }

    /// <summary>Every mod on someone for a stat: from their passives, their auras (once a stack), and their allies' passives that reach them; with who it comes from.</summary>
    internal IEnumerable<(StatMod Mod, Fighter From)> Mods(Fighter f, Stat stat)
    {
        foreach (SkillDefinition passive in f.PassiveSkills)
            foreach (StatMod mod in passive.Mods)
                if (mod.Stat == stat) yield return (mod, f);

        foreach (Aura aura in f.AuraList)
        {
            if (aura.Gone) continue;
            foreach (StatMod mod in aura.Definition.Mods)
                if (mod.Stat == stat)
                    for (int i = 0; i < aura.Stacks; i++) yield return (mod, aura.Source ?? f);
        }

        foreach (Fighter ally in _fighters)
        {
            if (ally.Side != f.Side || !ally.IsStanding) continue;
            foreach (SkillDefinition passive in ally.PassiveSkills)
            {
                if (passive.AllyMods.Count == 0) continue;
                if (passive.AllyRadius > 0f && Vector2.Distance(ally.Position, f.Position) > passive.AllyRadius) continue;
                foreach (StatMod mod in passive.AllyMods)
                    if (mod.Stat == stat) yield return (mod, ally);
            }
        }
    }

    /// <summary>Whether a mod counts in a moment.</summary>
    /// <param name="mod">The mod.</param>
    /// <param name="f">Who carries it.</param>
    /// <param name="context">The moment.</param>
    /// <param name="from">Whose it is (the passive's owner, the aura's source), for the mods that care.</param>
    internal bool Counts(StatMod mod, Fighter f, in ModContext context, Fighter from = null)
    {
        if (mod.Type is DamageType type && context.Type is DamageType asked && type != asked && mod.Stat is Stat.Resist or Stat.Vulnerable) return false;
        return mod.When switch
        {
            When.Always => true,
            When.Melee => context.Melee,
            When.Ranged => context.Ranged,
            When.Spell => context.Spell,
            When.VsMarked => context.Other is not null && IsMarked(context.Other),
            When.BelowHalf => f.HpShare < 0.5,
            When.VsUndead => context.Other?.Enemy?.Undead == true,
            When.Dark => Rules.Dark,
            When.Daylight => !Rules.Dark && Rules.Outdoors,
            When.TargetBelowQuarter => context.Other is not null && context.Other.HpShare < 0.25,
            When.VsHeld => context.Other is not null && (Has(context.Other, ConditionId.Restrained) || Has(context.Other, ConditionId.Prone)),
            When.ClassSkill => context.ClassSkill,
            When.VsHeldBySource => context.Other is not null && HeldBy(context.Other, from ?? f),
            When.VsAttackingOthers => context.Other?.Target is Fighter victim && victim != f && victim.Side == f.Side && Hostile(context.Other, f),
            When.OnOthers => context.Other is not null && context.Other != f,
            _ => false,
        };
    }

    /// <summary>What every mod for a stat adds up to on someone in a moment (an Of ability's modifier, the mod's source's, counted in).</summary>
    internal double Sum(Fighter f, Stat stat, in ModContext context)
    {
        double total = 0;
        foreach ((StatMod mod, Fighter from) in Mods(f, stat))
        {
            if (!Counts(mod, f, context, from)) continue;
            total += mod.Amount;
            if (mod.Of is Ability ability) total += from.Scores.Modifier(ability);
        }
        return total;
    }

    /// <summary>Whether any mod for a stat counts on someone in a moment.</summary>
    internal bool Any(Fighter f, Stat stat, in ModContext context)
    {
        foreach ((StatMod mod, Fighter from) in Mods(f, stat))
            if (Counts(mod, f, context, from)) return true;
        return false;
    }

    /// <summary>Rolls every die a stat's mods add (Bless's 1d4) on someone in a moment.</summary>
    internal int RollMods(Fighter f, Stat stat, in ModContext context)
    {
        int total = 0;
        foreach ((StatMod mod, Fighter from) in Mods(f, stat))
            if (Counts(mod, f, context, from) && !mod.Dice.IsNone) total += mod.Dice.Roll(_rng, "aura").Total;
        return total;
    }

    // Putting them on.

    /// <summary>Puts an aura on someone: refreshes it (or adds a stack) if the same source's is on them already. A condition they're immune to doesn't go on.</summary>
    /// <returns>The aura on them, or null if it didn't go on.</returns>
    internal Aura ApplyAura(Fighter t, AuraDefinition definition, Fighter source, SkillDefinition skill, int dc, int amount = 0)
    {
        if (t.IsDead || t.HasLeft || !t.Present) return null;
        if (definition.Condition is ConditionId condition && Immune(t, condition))
        {
            Emit(new BattleEvent { Kind = BattleEventKind.AuraRemoved, ActorId = source?.Id, TargetId = t.Id, AuraId = definition.Id, Text = $"{t.Name} is immune to {definition.Name}." });
            return null;
        }
        if (definition.Condition == ConditionId.Prone && t.Enemy is { Big: true }) return null;
        if (definition.Condition == ConditionId.Restrained && definition.DurationMs > 0 && source is not null)
        {
            int longer = (int)Sum(source, Stat.RestrainedMs, default);
            if (longer > 0) definition = definition with { DurationMs = definition.DurationMs + longer };
        }

        foreach (Aura existing in t.AuraList)
        {
            if (existing.Gone || existing.Id != definition.Id || existing.Source != source) continue;
            if (definition.Stacks) existing.Stacks = Math.Min(definition.MaxStacks, existing.Stacks + 1);
            existing.StartMs = NowMs;
            existing.EndsAtMs = definition.DurationMs > 0 ? NowMs + definition.DurationMs : 0;
            existing.Charges = definition.Charges;
            if (definition.Shield) existing.Amount = Math.Max(existing.Amount, amount);
            Emit(new BattleEvent
            {
                Kind = BattleEventKind.AuraRefreshed, ActorId = source?.Id, TargetId = t.Id, AuraId = definition.Id, SkillId = skill?.Id, Amount = existing.Stacks,
                DurationMs = definition.DurationMs,
            });
            return existing;
        }

        var aura = new Aura(definition, source, skill, NowMs, dc)
        {
            EndsAtMs = definition.DurationMs > 0 ? NowMs + definition.DurationMs : 0,
            NextTickMs = NowMs + definition.TickMs,
            Amount = amount,
        };
        if (skill?.Concentration == true && source is not null)
        {
            // One concentration at a time (§3.6): another skill ends the last, and so does the same one cast
            // again on someone new; a cast that catches several at once (this tick) keeps them all.
            if (source.Concentrating is not null && source.Concentrating != skill) EndConcentration(source);
            else if (source.Concentrating == skill)
                foreach (Fighter other in _fighters)
                    foreach (Aura held in other.AuraList)
                        if (!held.Gone && held.Concentration && held.Source == source && held.Skill == skill && held.StartMs < NowMs) RemoveAura(other, held, "concentration moved");
            source.Concentrating = skill;
            aura.Concentration = true;
        }
        t.AuraList.Add(aura);
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.AuraApplied, ActorId = source?.Id, TargetId = t.Id, AuraId = definition.Id, SkillId = skill?.Id, Amount = definition.Shield ? amount : 1,
            DurationMs = definition.DurationMs, Text = $"{t.Name}: {definition.Name}.",
        });
        OnCondition(t, aura, source);
        if (!definition.Beneficial && definition.Condition is ConditionId control && Controls(control) && source is not null && Hostile(source, t)) AllyControlled(t, aura, source);
        return aura;
    }

    /// <summary>Whether a condition takes someone's control from them, for the passives that answer it (Choose It Yourself).</summary>
    private static bool Controls(ConditionId condition) => condition is ConditionId.Stunned or ConditionId.Frozen or ConditionId.Restrained or ConditionId.Prone
        or ConditionId.Charmed or ConditionId.Frightened or ConditionId.Blinded;

    /// <summary>Runs the OnAllyControlled procs of someone's side (Choose It Yourself, once a fight) on a control condition a foe has just put on them.</summary>
    private void AllyControlled(Fighter t, Aura aura, Fighter source)
    {
        foreach (Fighter ally in _fighters)
        {
            if (ally.Side != t.Side || !ally.IsStanding) continue;
            foreach (SkillDefinition passive in ally.PassiveSkills)
                foreach (Proc proc in passive.Procs)
                {
                    if (proc.Trigger != ProcTrigger.OnAllyControlled || aura.Gone || !ProcReady(ally, passive, proc)) continue;
                    Emit(new BattleEvent { Kind = BattleEventKind.ReactionFired, ActorId = ally.Id, TargetId = t.Id, SkillId = passive.Id, Text = $"{ally.Name} won't have it ({passive.Name})." });
                    foreach (Effect effect in proc.Effects) ApplyLoose(ally, passive, effect, t, source);
                    return;
                }
        }
    }

    /// <summary>What a condition does the moment it goes on: a Stun or a freeze stops a cast, a knockdown puts them on the ground, a taunt turns them, a root stops a walk.</summary>
    private void OnCondition(Fighter t, Aura aura, Fighter source)
    {
        switch (aura.Definition.Condition)
        {
            case ConditionId.Stunned:
            case ConditionId.Frozen:
                Interrupt(t, aura.Name);
                t.Goal = null;
                CancelHelp(t);
                break;
            case ConditionId.Prone:
                Interrupt(t, "knocked down");
                t.Goal = null;
                CancelHelp(t);
                t.CurrentMotion = "knockdown";
                t.MotionStartMs = NowMs;
                Emit(new BattleEvent { Kind = BattleEventKind.KnockedDown, ActorId = source?.Id, TargetId = t.Id, Motion = "knockdown", Position = t.Position, DurationMs = aura.TotalMs });
                break;
            case ConditionId.Restrained:
                t.Goal = null;
                break;
            case ConditionId.Taunted when source is not null:
                if (t.Target != source && Hostile(t, source))
                {
                    t.Target = source;
                    Emit(new BattleEvent { Kind = BattleEventKind.ThreatTargetChanged, ActorId = t.Id, TargetId = source.Id });
                }
                Emit(new BattleEvent { Kind = BattleEventKind.Taunted, ActorId = source.Id, TargetId = t.Id, DurationMs = aura.TotalMs, Text = $"{t.Name} is taunted by {source.Name}." });
                break;
        }
    }

    /// <summary>Takes an aura off, saying why; a knockdown's ending is getting up, and a concentration aura's source stops concentrating if it was their last.</summary>
    internal void RemoveAura(Fighter t, Aura aura, string why)
    {
        if (aura.Gone) return;
        aura.Gone = true;
        Emit(new BattleEvent { Kind = BattleEventKind.AuraRemoved, ActorId = aura.SourceId, TargetId = t.Id, AuraId = aura.Id, Text = why is null ? null : $"{t.Name}: {aura.Name} ends ({why})." });
        if (aura.Definition.Condition == ConditionId.Prone && t.IsStanding)
        {
            t.CurrentMotion = "getup";
            t.MotionStartMs = NowMs;
            Emit(new BattleEvent { Kind = BattleEventKind.GotUp, ActorId = t.Id, Motion = "getup", Position = t.Position });
        }
    }

    /// <summary>Runs someone's auras: the ones whose time is up come off, and the ones that tick do (damage, healing, or a save to end it).</summary>
    private void TickAuras(Fighter t)
    {
        for (int i = 0; i < t.AuraList.Count; i++)
        {
            Aura aura = t.AuraList[i];
            if (aura.Gone) continue;
            AuraDefinition definition = aura.Definition;
            bool ticks = !definition.TickDice.IsNone || definition.SaveEnds is not null;
            if (ticks && NowMs >= aura.NextTickMs && !t.IsDead)
            {
                aura.NextTickMs += definition.TickMs;
                TickAura(t, aura);
                if (aura.Gone) continue;
            }
            if (aura.EndsAtMs > 0 && NowMs >= aura.EndsAtMs) RemoveAura(t, aura, null);
        }
    }

    /// <summary>One tick of an aura: its damage or healing (per stack, which can't crit, be dodged or blocked, and counts for Wild Focus), or its save.</summary>
    private void TickAura(Fighter t, Aura aura)
    {
        AuraDefinition definition = aura.Definition;
        int onDowned = definition.TickHeals && t.IsDown && aura.Source is Fighter healer ? (int)Sum(healer, Stat.HotOnDowned, default) : 0;
        if (!definition.TickDice.IsNone && (!t.IsDown || onDowned > 0))
        {
            int level = aura.Source?.Level ?? 1;
            DiceExpression dice = definition.TickPlus ? Scaled(definition.TickDice, level) : definition.TickDice;
            int amount = 0;
            for (int i = 0; i < aura.Stacks; i++) amount += Math.Max(0, dice.Roll(_rng, definition.Name).Total);
            if (onDowned > 0) amount *= onDowned;
            Emit(new BattleEvent { Kind = BattleEventKind.AuraTicked, ActorId = aura.SourceId, TargetId = t.Id, AuraId = aura.Id, Amount = amount, Flags = HitFlags.Tick });
            if (definition.TickHeals) Heal(aura.Source ?? t, t, amount, aura.Skill, tick: true);
            else Hurt(t, amount, definition.TickType, aura.Source, HitFlags.Tick, aura.Skill, ignoreResistance: false);
            if (aura.Source is Fighter source && source.IsStanding) GainFocus(source, FocusModels.Get(source.FocusModel).PerTick);
        }

        if (definition.SaveEnds is Ability save && !aura.Gone && t.IsStanding)
        {
            bool made = Save(t, save, aura.Dc, definition.Condition, aura.Source);
            Emit(new BattleEvent { Kind = BattleEventKind.AuraTicked, ActorId = aura.SourceId, TargetId = t.Id, AuraId = aura.Id, Flags = made ? HitFlags.Save : HitFlags.None });
            if (made) RemoveAura(t, aura, "saved");
        }
    }

    /// <summary>Uses up whatever on someone goes on a kind of use (Inspired on an attack roll, Marked on the next attack at them, a free skill on the next skill): a charge, and the aura when they're gone.</summary>
    internal void UseUp(Fighter t, AuraUse use, Func<Aura, bool> only = null)
    {
        foreach (Aura aura in t.AuraList)
        {
            if (aura.Gone) continue;
            AuraUse by = aura.Definition.UsedBy;
            bool matches = by == use || use == AuraUse.NextAttack && by == AuraUse.NextRoll;
            if (!matches || only is not null && !only(aura)) continue;
            aura.Charges--;
            if (aura.Charges <= 0) RemoveAura(t, aura, "used");
        }
    }

    /// <summary>Takes harmful auras off someone: those with the named conditions, or all of them that can be cleansed.</summary>
    internal void Cleanse(Fighter t, IReadOnlyList<ConditionId> conditions)
    {
        foreach (Aura aura in t.AuraList)
        {
            if (aura.Gone || aura.Beneficial || aura.Definition.Uncleansable) continue;
            bool named = conditions.Count == 0 || aura.Definition.Condition is ConditionId c && Contains(conditions, c);
            if (named) RemoveAura(t, aura, "cleansed");
        }
    }

    /// <summary>Whether a list has a condition.</summary>
    private static bool Contains(IReadOnlyList<ConditionId> list, ConditionId id)
    {
        foreach (ConditionId each in list)
            if (each == id) return true;
        return false;
    }

    /// <summary>Ends someone's concentration: every aura it holds up comes off.</summary>
    internal void EndConcentration(Fighter source)
    {
        if (source.Concentrating is null) return;
        SkillDefinition skill = source.Concentrating;
        source.Concentrating = null;
        foreach (Fighter f in _fighters)
            foreach (Aura aura in f.AuraList)
                if (!aura.Gone && aura.Concentration && aura.Source == source && aura.Skill == skill) RemoveAura(f, aura, "concentration broken");
    }

    /// <summary>Puts temporary hit points on someone, as a shield aura; a bigger shield of the same kind replaces a smaller one.</summary>
    internal void Shield(Fighter t, int amount, AuraDefinition definition, Fighter source, SkillDefinition skill)
    {
        if (amount <= 0) return;
        definition ??= SkillCatalog.TempHpShield("temp-hp", "Temporary Hit Points", DefaultShieldMs);
        ApplyAura(t, definition, source, skill, 0, amount);
    }
}

/// <summary>The moment a stat is asked about: against whom, by what kind of blow, of what damage.</summary>
internal readonly struct ModContext
{
    /// <summary>The other fighter: the target of an attack, or the attacker.</summary>
    public Fighter Other { get; init; }

    /// <summary>Whether it's a melee blow.</summary>
    public bool Melee { get; init; }

    /// <summary>Whether it's a ranged blow.</summary>
    public bool Ranged { get; init; }

    /// <summary>Whether it's a spell (anything not a weapon blow).</summary>
    public bool Spell { get; init; }

    /// <summary>Whether it's an area.</summary>
    public bool Area { get; init; }

    /// <summary>The damage type, or null.</summary>
    public DamageType? Type { get; init; }

    /// <summary>Whether it's one of the user's own class skills.</summary>
    public bool ClassSkill { get; init; }
}
