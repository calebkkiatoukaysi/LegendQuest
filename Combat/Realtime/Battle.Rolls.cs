using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// The 5e sheet underneath (11-combat.md §3): attack rolls with advantage and disadvantage, reading a
/// miss as a dodge or a block, crits from the stats, saving throws, DCs and attack bonuses, and dice
/// that go up with level.
/// </summary>
public sealed partial class Battle
{
    /// <summary>The most crit chance anyone can have, in percent.</summary>
    public const int MostCrit = 40;

    /// <summary>The crit chance a natural 20 gives, in percent.</summary>
    public const int NaturalCrit = 5;

    /// <summary>What a point of an attack's ability modifier above 0 adds to its crit chance, in percent.</summary>
    public const int CritPerPoint = 2;

    /// <summary>Rolls an attack: armed reactions first (Cover, Refract, Shield), then the d20 with everything that gives advantage or disadvantage, then what the roll says (§3.2): a crit, a hit, a dodge, a block or a miss. Says so.</summary>
    /// <param name="a">The attacker.</param>
    /// <param name="t">The target, which a reaction may change.</param>
    /// <param name="bonus">The attack's own bonus (the weapon's, or the spell attack's).</param>
    /// <param name="kind">Melee, ranged or a spell.</param>
    /// <param name="skill">The skill, for the log and for ClassSkill mods.</param>
    /// <param name="critAbility">The ability the crit chance comes from.</param>
    internal AttackResult AttackRoll(Fighter a, Fighter t, int bonus, BlowKind kind, SkillDefinition skill, Ability critAbility)
    {
        // Reactions that move the attack or brace for it.
        if (a.Side != t.Side)
        {
            var (coverer, cover) = TakeArmed(t.Side, ReactionTrigger.AttackOnAlly, over: t);
            if (cover is not null && coverer != t)
            {
                Emit(new BattleEvent { Kind = BattleEventKind.Bark, ActorId = coverer.Id, TargetId = t.Id, Text = $"{coverer.Name} takes the blow meant for {t.Name}." });
                t = coverer;
            }
            else if (CoveredBy(t) is Fighter guard)
            {
                Emit(new BattleEvent { Kind = BattleEventKind.Bark, ActorId = guard.Id, TargetId = t.Id, Text = $"{guard.Name} takes the blow meant for {t.Name}." });
                t = guard;
            }
            // Refract only goes off if there's another foe to turn the attack onto; with the attacker
            // alone (a duel, a lone construct) it stays armed rather than firing for nothing.
            if (kind != BlowKind.Melee && _fighters.Exists(other => other.Side == a.Side && other.IsStanding && other != a && !other.Vanished))
            {
                var (refractor, refract) = TakeArmed(t.Side, ReactionTrigger.RangedOnAlly);
                if (refract is not null && Pick(a.Side, a) is Fighter turned)
                {
                    Emit(new BattleEvent { Kind = BattleEventKind.Bark, ActorId = refractor.Id, TargetId = turned.Id, Text = $"{refractor.Name} turns it aside, onto {turned.Name}." });
                    t = turned;
                }
            }
            var (shielder, shield) = TakeArmed(t.Side, ReactionTrigger.AttackOnYou, only: t);
            if (shield is not null) FireReaction(shielder, shield, a);
        }

        var result = new AttackResult { Target = t };
        if (!t.IsStanding && !t.IsDown || t.Vanished)
        {
            result.Flags = HitFlags.Miss;
            return result;
        }

        // An ally under Sanctuary heals when they're attacked, if its source has the passive for it.
        if (a.Side != t.Side) SanctuaryHeals(t);

        // A decoy (Mirror Image's images, Rooftops' feint) takes its share of the attacks meant for whoever carries it.
        foreach (Aura aura in t.AuraList)
        {
            if (aura.Gone || aura.Definition.Decoy <= 0 || aura.Definition.UsedBy != AuraUse.AttackTaken || aura.Definition.Condition is not null) continue;
            if (_rng.Percent(aura.Definition.Decoy))
            {
                aura.Charges--;
                if (aura.Charges <= 0) RemoveAura(t, aura, "the last image is gone");
                result.Flags = HitFlags.Miss;
                Emit(new BattleEvent { Kind = BattleEventKind.Missed, ActorId = a.Id, TargetId = t.Id, SkillId = skill?.Id, Flags = HitFlags.Miss, Position = t.Position, Text = $"{a.Name} strikes at a decoy of {t.Name} ({aura.Name})." });
                return result;
            }
            break;
        }

        var context = new ModContext { Other = t, Melee = kind == BlowKind.Melee, Ranged = kind == BlowKind.Ranged, Spell = kind == BlowKind.Spell, ClassSkill = skill?.Class is not null };
        var against = new ModContext { Other = a, Melee = kind == BlowKind.Melee, Ranged = kind == BlowKind.Ranged, Spell = kind == BlowKind.Spell };
        Advantage advantage = AdvantageOf(a, t, kind, context, against);

        int kept = AttackMath.D20(_rng, advantage, out int first, out int second);
        if (Any(a, Stat.Natural20, context))
        {
            kept = AttackMath.CriticalRoll;
            first = kept;
            second = 0;
        }
        int rank = skill?.Class is not null && a.Rank >= ClassRanks.ToHitRank ? 1 : 0;
        int total = kept + bonus + (int)Sum(a, Stat.ToHit, context) + RollMods(a, Stat.ToHitDice, context) + rank
            + (int)Sum(t, Stat.AttackersToHit, against) + PerAlly(a);
        int ac = t.BaseAc + (int)Sum(t, Stat.Ac, against);
        bool sure = Any(a, Stat.SureCrit, context);

        bool hit = kept == AttackMath.CriticalRoll || kept != AttackMath.FumbleRoll && total >= ac;
        bool crit = hit && (kept == AttackMath.CriticalRoll || sure || _rng.Percent(CritChance(a, critAbility, context) - NaturalCrit));
        if (t.IsDown && kind == BlowKind.Melee && hit) crit = true;

        UseUp(a, AuraUse.NextAttack);
        UseUp(t, AuraUse.AttackTaken, aura => aura.Definition.Condition is not null);

        string dice = second == 0 ? $"{first}" : $"{first}/{second}";
        string line = $"{a.Name}'s {skill?.Name ?? "attack"} at {t.Name}: {dice} + {total - kept} = {total} vs AC {ac}";
        result.Roll = kept;
        result.Total = total;
        if (hit)
        {
            result.Hit = true;
            result.Crit = crit;
            result.Flags = crit ? HitFlags.Crit : HitFlags.None;
            Emit(new BattleEvent { Kind = BattleEventKind.Hit, ActorId = a.Id, TargetId = t.Id, SkillId = skill?.Id, Flags = result.Flags, Amount = total, Position = t.Position, Text = line + (crit ? " - CRITICAL." : " - HIT.") });
            return result;
        }

        BattleEventKind miss;
        if (kept != AttackMath.FumbleRoll && t.DexAc > 0 && total >= ac - t.DexAc)
        {
            miss = BattleEventKind.Dodged;
            result.Flags = HitFlags.Dodge;
            GainFocus(t, FocusModels.Get(t.FocusModel).PerDodge);
            foreach (SkillDefinition passive in t.PassiveSkills)
                foreach (Proc proc in passive.Procs)
                    if (proc.Trigger == ProcTrigger.OnDodge && ProcReady(t, passive, proc))
                        foreach (Effect effect in proc.Effects) ApplyLoose(t, passive, effect, effect.On == EffectOn.Attacker ? a : t, a);
        }
        else if (kept != AttackMath.FumbleRoll && t.ShieldAc > 0 && total >= ac - t.ShieldAc)
        {
            miss = BattleEventKind.Blocked;
            result.Flags = HitFlags.Block;
            GainFocus(t, FocusModels.Get(t.FocusModel).PerBlock);
        }
        else
        {
            miss = BattleEventKind.Missed;
            result.Flags = HitFlags.Miss;
        }
        string word = miss == BattleEventKind.Dodged ? "DODGED" : miss == BattleEventKind.Blocked ? "BLOCKED" : "MISS";
        Emit(new BattleEvent { Kind = miss, ActorId = a.Id, TargetId = t.Id, SkillId = skill?.Id, Flags = result.Flags, Amount = total, Position = t.Position, Text = $"{line} - {word}." });

        if (kind == BlowKind.Melee && t.IsStanding)
        {
            var (riposter, riposte) = TakeArmed(t.Side, ReactionTrigger.MeleeMissOnYou, only: t);
            if (riposte is not null) FireReaction(riposter, riposte, a);
        }
        return result;
    }

    /// <summary>Whether an attack has advantage, disadvantage, or neither (5e's rule: any of each cancels out).</summary>
    private Advantage AdvantageOf(Fighter a, Fighter t, BlowKind kind, in ModContext context, in ModContext against)
    {
        int advantages = 0, disadvantages = 0;
        if (Any(a, Stat.Advantage, context)) advantages++;
        if (Any(t, Stat.AttackersAdvantage, against)) advantages++;
        if (Has(t, ConditionId.Restrained) || Has(t, ConditionId.Stunned) || Has(t, ConditionId.Frozen) || Has(t, ConditionId.Blinded)) advantages++;
        if (Has(t, ConditionId.Marked)) advantages++;
        if (Has(t, ConditionId.Prone) || t.IsDown) { if (kind == BlowKind.Melee) advantages++; else disadvantages++; }
        if (PackTactics(a, t)) advantages++;

        if (Any(a, Stat.Disadvantage, context)) disadvantages++;
        if (Any(t, Stat.AttackersDisadvantage, against)) disadvantages++;
        if (Has(a, ConditionId.Blinded) || Has(a, ConditionId.Poisoned) || Has(a, ConditionId.Frightened) || Has(a, ConditionId.Restrained) || Has(a, ConditionId.Prone)) disadvantages++;
        if (kind != BlowKind.Spell && a.Member is not null && CombatStats.ArmourUntrained(a.Member)) disadvantages++;
        return AttackMath.Combine(advantages, disadvantages);
    }

    /// <summary>Pack Tactics: a stat block that has it, with another of its side standing in melee with the same target.</summary>
    private bool PackTactics(Fighter a, Fighter t)
    {
        if (a.Enemy is null || !a.Enemy.Abilities.Contains(AbilityId.PackTactics)) return false;
        foreach (Fighter other in _fighters)
        {
            if (other == a || other.Side != a.Side || !other.IsStanding) continue;
            if (Reaching(other.Position, t.Position) <= Ranges.Melee + other.Size + t.Size) return true;
        }
        return false;
    }

    /// <summary>Someone's crit chance with an ability, in percent: 5 for the natural 20, 2 a point of the modifier above 0, and whatever's on them, to 40 at most.</summary>
    internal int CritChance(Fighter f, Ability ability, in ModContext context) =>
        Math.Min(MostCrit, NaturalCrit + CritPerPoint * Math.Max(0, f.Scores.Modifier(ability)) + (int)Sum(f, Stat.Crit, context));

    /// <summary>The ability a weapon attack's crit chance comes from: Dexterity for a ranged weapon, the better for a finesse one, Strength otherwise.</summary>
    internal static Ability WeaponAbility(Fighter f, AttackProfile attack)
    {
        int strength = f.Scores.Modifier(Ability.Strength), dexterity = f.Scores.Modifier(Ability.Dexterity);
        if (attack.Ranged) return Ability.Dexterity;
        if ((attack.Properties & WeaponProperty.Finesse) != 0 && dexterity > strength) return Ability.Dexterity;
        return f.Enemy is not null && dexterity > strength ? Ability.Dexterity : Ability.Strength;
    }

    /// <summary>Rolls a saving throw: d20, the save's bonus, what's on them (Bless's die, Aura of Protection), with advantage against a condition they're warded from; a failed one can be rerolled by a passive.</summary>
    /// <param name="t">Who saves.</param>
    /// <param name="ability">Which save.</param>
    /// <param name="dc">Against what.</param>
    /// <param name="against">The condition it's against, for the mods that ward from one.</param>
    /// <param name="source">Who's making them save, for the log.</param>
    /// <param name="disadvantage">Whether it's made with disadvantage (an undead against Dawnlight).</param>
    /// <param name="type">The damage it's against, for the mods that ward from one (Doomward Born against necrotic), or null.</param>
    /// <returns>Whether they made it.</returns>
    internal bool Save(Fighter t, Ability ability, int dc, ConditionId? against, Fighter source, bool disadvantage = false, DamageType? type = null)
    {
        int bonus = t.Member is not null ? CombatStats.SaveBonus(t.Member, ability) : t.Scores.Modifier(ability);
        var context = new ModContext { Other = source, Type = type };
        bonus += (int)Sum(t, Stat.Save, context) + RollMods(t, Stat.SaveDice, context) + PerAlly(t);
        bool lucky = Any(t, Stat.Natural20, context);
        UseUp(t, AuraUse.NextRoll);

        int advantages = 0, disadvantages = disadvantage ? 1 : 0;
        foreach ((StatMod mod, Fighter _) in Mods(t, Stat.SaveAdvantage))
        {
            if (mod.Type is DamageType warded) { if (warded == type) advantages++; }
            else if (mod.Condition is null || mod.Condition == against) advantages++;
        }
        if (ability == Ability.Dexterity && Has(t, ConditionId.Restrained)) disadvantages++;
        Advantage advantage = AttackMath.Combine(advantages, disadvantages);

        int roll = lucky ? AttackMath.CriticalRoll : AttackMath.D20(_rng, advantage, out _, out _);
        bool made = roll + bonus >= dc;
        if (!made && Reroll(t)) made = AttackMath.D20(_rng, advantage, out _, out _) + bonus >= dc;
        return made;
    }

    /// <summary>Whether a passive lets someone reroll a failed save now (Indomitable, once a fight).</summary>
    private bool Reroll(Fighter t)
    {
        foreach (SkillDefinition passive in t.PassiveSkills)
            foreach (Proc proc in passive.Procs)
            {
                if (proc.Trigger != ProcTrigger.OnFailedSave || !ProcReady(t, passive, proc)) continue;
                foreach (Effect effect in proc.Effects)
                {
                    if (effect.Kind != EffectType.Reroll) continue;
                    Emit(new BattleEvent { Kind = BattleEventKind.Bark, ActorId = t.Id, SkillId = passive.Id, Text = $"{t.Name} shrugs it off ({passive.Name})." });
                    return true;
                }
            }
        return false;
    }

    /// <summary>Whether a passive's proc can go off now, and marks it as gone off if so (its cooldown, its once a fight).</summary>
    internal bool ProcReady(Fighter f, SkillDefinition passive, Proc proc)
    {
        string key = passive.Id + "#" + proc.Trigger;
        if (proc.OncePerFight && f.ProcsSpent.Contains(key)) return false;
        if (f.ProcReadyAt.TryGetValue(key, out int ready) && ready > NowMs) return false;
        if (proc.ChancePct < 100 && !_rng.Percent(proc.ChancePct)) return false;
        if (proc.OncePerFight) f.ProcsSpent.Add(key);
        if (proc.CooldownMs > 0) f.ProcReadyAt[key] = NowMs + proc.CooldownMs;
        return true;
    }

    /// <summary>The save DC of someone's skill: a foe's move's own (with the difficulty's change), or 8 + proficiency + their casting ability, and what's on them, and a point from rank 8.</summary>
    internal int Dc(Fighter f, SkillDefinition skill)
    {
        if (skill is not null && skill.Dc > 0) return skill.Dc + (f.Side == Side.Foes ? DifficultyRules.FoeRolls(Rules.Difficulty) : 0);
        int rank = skill?.Class is not null && f.Rank >= ClassRanks.DcRank ? 1 : 0;
        return 8 + f.Proficiency + f.Scores.Modifier(f.CastingAbility) + (int)Sum(f, Stat.Dc, default) + rank;
    }

    /// <summary>Someone's attack bonus with a spell (or any skill's attack that isn't a weapon blow): proficiency and their casting ability, and what they wear; a foe's move's own.</summary>
    internal int SpellAttack(Fighter f, SkillDefinition skill)
    {
        if (f.Enemy is not null)
            return skill is not null && skill.AttackBonus > 0 ? skill.AttackBonus + (f.Side == Side.Foes ? DifficultyRules.FoeRolls(Rules.Difficulty) + (f.Enemy?.RealtimeHitShift ?? 0) : 0) : f.MainAttack.Bonus;
        return f.Proficiency + f.Scores.Modifier(f.CastingAbility) + f.SpellAttackExtra;
    }

    /// <summary>Dice that go up at levels 5 and 9: one more die at each (10-classes.md §1.5).</summary>
    internal static DiceExpression Scaled(DiceExpression dice, int level) =>
        dice.Count == 0 ? dice : dice with { Count = dice.Count + (level >= 5 ? 1 : 0) + (level >= 9 ? 1 : 0) };

    /// <summary>What someone's rolls gain from the allies still standing beside them (Name on the Door): the mod's amount for each.</summary>
    private int PerAlly(Fighter f)
    {
        int each = (int)Sum(f, Stat.RollsPerAlly, default);
        if (each == 0) return 0;
        int standing = 0;
        foreach (Fighter ally in _fighters)
            if (ally != f && ally.Side == f.Side && ally.IsStanding) standing++;
        return each * standing;
    }

    /// <summary>Whoever is covering someone (Nobody Left Behind) and standing to take their blows, or null.</summary>
    private Fighter CoveredBy(Fighter t)
    {
        foreach (Aura aura in t.AuraList)
        {
            if (aura.Gone || aura.Source is not Fighter guard || guard == t || !guard.IsStanding || !CanAct(guard)) continue;
            foreach (StatMod mod in aura.Definition.Mods)
                if (mod.Stat == Stat.Covered) return guard;
        }
        return null;
    }

    /// <summary>Heals someone under Sanctuary who's being attacked, by what its source's passive says (Bees Guard the Honey).</summary>
    private void SanctuaryHeals(Fighter t)
    {
        foreach (Aura aura in t.AuraList)
        {
            if (aura.Gone || aura.Source is not Fighter keeper || !keeper.IsStanding || !HasMod(aura.Definition, Stat.Sanctuary)) continue;
            int amount = 0;
            foreach ((StatMod mod, Fighter from) in Mods(keeper, Stat.SanctuaryHeal))
                if (from == keeper && !mod.Dice.IsNone) amount += mod.Dice.Roll(_rng, "sanctuary").Total;
            if (amount > 0) Heal(keeper, t, amount, aura.Skill, tick: true);
            return;
        }
    }

    /// <summary>A random standing fighter of a side (but not one to leave out), or null.</summary>
    internal Fighter Pick(Side side, Fighter not = null)
    {
        var standing = new List<Fighter>();
        foreach (Fighter f in _fighters)
            if (f.Side == side && f.IsStanding && f != not && !f.Vanished) standing.Add(f);
        return standing.Count == 0 ? null : standing[(int)_rng.Below((uint)standing.Count)];
    }
}

/// <summary>How a blow reaches its target, for advantage and the mods that care.</summary>
internal enum BlowKind
{
    /// <summary>A melee blow: a weapon's, or any skill's at melee or reach range that doesn't fly (a Gaff, a Shield Bash, a Thorn Whip).</summary>
    Melee,

    /// <summary>A ranged weapon's shot, or a thrown thing.</summary>
    Ranged,

    /// <summary>A spell, or any other skill's attack that isn't a weapon's: one that flies, or reaches further than a spear.</summary>
    Spell,
}

/// <summary>What an attack roll came to.</summary>
internal sealed class AttackResult
{
    /// <summary>Who it was at in the end (a reaction can change it).</summary>
    public Fighter Target;

    /// <summary>Whether it hit.</summary>
    public bool Hit;

    /// <summary>Whether it's a critical.</summary>
    public bool Crit;

    /// <summary>What the roll said.</summary>
    public HitFlags Flags;

    /// <summary>The d20 that counted.</summary>
    public int Roll;

    /// <summary>The total against AC.</summary>
    public int Total;
}
