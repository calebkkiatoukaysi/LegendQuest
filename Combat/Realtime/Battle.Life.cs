using System;
using System.Collections.Generic;
using System.Numerics;
using LegendQuest.Characters;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// Life in a fight: hurting (resistance, temporary hit points, concentration, a cast's Con save),
/// dropping (Downed, death saves, falling, a guest falling back, a foe dying or yielding), healing
/// and getting up, Help, threat, Focus, forms, smoke, summons, and the Soul-Link (11-combat.md §5.3).
/// </summary>
public sealed partial class Battle
{
    /// <summary>How long Help's channel takes to get a Downed ally up.</summary>
    public const int HelpMs = 2000;

    /// <summary>The window in which both of the duo have to hit the same foe for the Soul-Link to charge, and the least time between charges.</summary>
    public const int SoulLinkWindowMs = 3000;

    /// <summary>The charges the Soul-Link Strike needs.</summary>
    public const int SoulLinkCharges = 2;

    /// <summary>How long a full Soul-Link is left for the player to spend (key 7), when they lead one of the duo, before the Soul-Bound's AI spends it.</summary>
    public const int SoulLinkLeftToYouMs = 6000;

    /// <summary>How long a hit on someone is remembered, for Sneak Attack.</summary>
    private const int RememberHitsMs = 3000;

    private readonly Dictionary<string, int> _duoHits = new(StringComparer.Ordinal);
    private int _soulCharges;
    private int _soulNextChargeMs;

    /// <summary>When the Soul-Link last filled.</summary>
    private int _soulFullAtMs;

    // Hurting.

    /// <summary>Takes hit points off someone: resistance and vulnerability, what they take more or less of, temporary hit points first; then threat, Focus, concentration, a cast's Con save, a Hellish Rebuke; and dropping them at 0.</summary>
    /// <param name="t">Who's hurt.</param>
    /// <param name="amount">How much, before anything of theirs.</param>
    /// <param name="type">The kind of damage.</param>
    /// <param name="source">Who did it, or null (a support, the room).</param>
    /// <param name="flags">What sort of blow it was.</param>
    /// <param name="skill">The skill, for the log.</param>
    /// <param name="ignoreResistance">Whether resistance counts for nothing.</param>
    /// <param name="area">Whether it's area damage.</param>
    /// <returns>What it took off, temporary hit points included.</returns>
    internal int Hurt(Fighter t, int amount, DamageType type, Fighter source, HitFlags flags, SkillDefinition skill, bool ignoreResistance, bool area = false)
    {
        if (!t.Present || t.IsDead || t.HasLeft || amount <= 0) return 0;
        bool tick = flags.HasFlag(HitFlags.Tick);
        if (t.IsDown)
        {
            if (source is not null && Hostile(source, t) && !tick) FailDeathSave(t, flags.HasFlag(HitFlags.Crit) ? 2 : 1);
            return 0;
        }

        var context = new ModContext { Other = source, Type = type, Area = area };
        if (!ignoreResistance)
        {
            if (Any(t, Stat.Resist, context)) amount /= 2;
            if (Any(t, Stat.Vulnerable, context)) amount *= 2;
        }
        double pct = Sum(t, Stat.DamageTakenPct, context) + (area ? Sum(t, Stat.AreaTakenPct, context) : 0);
        if (pct != 0) amount = (int)Math.Round(amount * Math.Max(0.0, 1.0 + pct / 100.0));
        if (amount <= 0) return 0;

        int absorbed = 0;
        foreach (Aura aura in t.AuraList)
        {
            if (aura.Gone || !aura.Definition.Shield || aura.Amount <= 0) continue;
            int take = Math.Min(aura.Amount, amount - absorbed);
            aura.Amount -= take;
            absorbed += take;
            if (aura.Amount <= 0) RemoveAura(t, aura, "broken");
            if (absorbed >= amount) break;
        }
        if (absorbed > 0)
            Emit(new BattleEvent { Kind = BattleEventKind.Absorbed, ActorId = source?.Id, TargetId = t.Id, SkillId = skill?.Id, Amount = absorbed, DamageType = type, Flags = flags, Position = t.Position });

        int rest = amount - absorbed;
        if (rest >= t.Hp && StaysUp(t)) rest = t.Hp - 1;
        bool drops = rest >= t.Hp;
        t.Hp = Math.Max(0, t.Hp - rest);
        t.LastHurtMs = NowMs;
        t.DamageTaken += amount;

        if (source is not null)
        {
            source.DamageDone += amount;
            if (source.Side == Side.Party) t.LastHitByPartyMs = NowMs;
            else t.LastHitByFoesMs = NowMs;
            t.RecentHits.RemoveAll(hit => NowMs - hit.AtMs > RememberHitsMs);
            t.RecentHits.Add((source, NowMs));
            t.HurtBy.Add(source);
            if (t.Side == Side.Foes && source.Side == Side.Party) AddThreat(t, source, amount);
        }
        if (!tick) GainFocus(t, FocusModels.Get(t.FocusModel).WhenHit);

        if (drops) flags |= HitFlags.Kill;
        if (flags.HasFlag(HitFlags.Crit) && amount * 4 >= t.MaxHp) flags |= HitFlags.Big;
        if (rest > 0 || absorbed == 0)
            Emit(new BattleEvent
            {
                Kind = BattleEventKind.Damaged, ActorId = source?.Id, TargetId = t.Id, SkillId = skill?.Id, Amount = rest, DamageType = type, Flags = flags, Position = t.Position,
                Text = $"{t.Name} takes {rest} {type.ToString().ToLowerInvariant()}{(absorbed > 0 ? $" ({absorbed} absorbed)" : string.Empty)}.",
            });

        if (drops)
        {
            Drop(t, source);
            return amount;
        }

        // Concentration: a Con save when hurt, as 5e has it, but at most one a turn (3 s), since a fighter
        // in real time is hit two or three times as often as once a round, and a concentration spell held
        // for a round or two at the table would otherwise last seconds.
        if (t.Concentrating is not null && !Any(t, Stat.KeepConcentration, context) && NowMs - t.LastConcentrationSaveMs >= TurnMs)
        {
            t.LastConcentrationSaveMs = NowMs;
            if (!Save(t, Ability.Constitution, Math.Max(10, amount / 2), null, source)) EndConcentration(t);
        }
        if (!tick && t.Action is { InMotion: false, CastUntilMs: > 0 } cast && cast.Skill.Interruptible
            && !Save(t, Ability.Constitution, Math.Max(10, amount / 2), null, source)) Interrupt(t, "hit");
        if (t.Side == Side.Foes && Encounter.YieldsAt > 0f && t.HpShare <= Encounter.YieldsAt) Leave(t, $"{t.Name} yields!");
        if (!tick && source is not null && Hostile(source, t) && t.IsStanding)
        {
            var (rebuker, rebuke) = TakeArmed(t.Side, ReactionTrigger.HitOnYou, only: t);
            if (rebuke is not null) FireReaction(rebuker, rebuke, source);
        }
        return amount;
    }

    /// <summary>Whether a passive keeps someone on 1 hit point instead of 0 (once a fight, or on its cooldown), and says so: their own (Staying Behind), or an ally's that keeps the party up (Everyone Who Knocks).</summary>
    private bool StaysUp(Fighter t)
    {
        if (t.Side != Side.Party || t.Kind == FighterKind.Summon) return StaysUpBy(t, t, ProcTrigger.OnDropped);
        if (StaysUpBy(t, t, ProcTrigger.OnDropped)) return true;
        foreach (Fighter ally in _fighters)
            if (ally.Side == t.Side && (ally == t || ally.IsStanding) && StaysUpBy(ally, t, ProcTrigger.OnAllyDropped)) return true;
        return false;
    }

    /// <summary>Whether one fighter's passive with a StayUp keeps someone up, for a trigger; marks it gone off and says so.</summary>
    private bool StaysUpBy(Fighter owner, Fighter t, ProcTrigger trigger)
    {
        foreach (SkillDefinition passive in owner.PassiveSkills)
            foreach (Proc proc in passive.Procs)
            {
                if (proc.Trigger != trigger || !HasStayUp(proc) || !ProcReady(owner, passive, proc)) continue;
                Emit(new BattleEvent
                {
                    Kind = BattleEventKind.Bark, ActorId = owner.Id, TargetId = t.Id, SkillId = passive.Id,
                    Text = owner == t ? $"{t.Name} refuses to fall ({passive.Name})." : $"{owner.Name} won't let {t.Name} fall ({passive.Name}).",
                });
                foreach (Effect effect in proc.Effects)
                    if (effect.Kind != EffectType.StayUp) ApplyLoose(owner, passive, effect, t, null);
                return true;
            }
        return false;
    }

    /// <summary>Whether a proc keeps someone on 1 hit point.</summary>
    private static bool HasStayUp(Proc proc)
    {
        foreach (Effect effect in proc.Effects)
            if (effect.Kind == EffectType.StayUp) return true;
        return false;
    }

    /// <summary>Someone at 0: a party member is Downed (stable, in a brawl), a guest falls back, a summon's gone, a foe dies (or is knocked out, in a brawl).</summary>
    private void Drop(Fighter t, Fighter source)
    {
        t.Hp = 0;
        Interrupt(t, "dropped");
        t.Action = null;
        t.Order = null;
        t.Pending = null;
        t.Goal = null;
        t.Chase = false;
        t.VanishUntilMs = 0;
        t.ArmedList.Clear();
        t.Riders.Clear();
        CancelHelp(t);
        EndConcentration(t);
        if (t.Form is not null) EndForm(t);

        if (t.Side == Side.Foes)
        {
            if (Encounter.Nonlethal)
            {
                Leave(t, $"{t.Name} is knocked out.");
                return;
            }
            t.IsDead = true;
            t.CurrentMotion = "die";
            t.MotionStartMs = NowMs;
            Emit(new BattleEvent { Kind = BattleEventKind.Died, ActorId = source?.Id, TargetId = t.Id, Motion = "die", Flags = HitFlags.Kill, Position = t.Position, Text = $"{t.Name} is beaten." });
            return;
        }

        if (t.Kind == FighterKind.Guest)
        {
            Leave(t, $"{t.Name} falls back, out of the fight.");
            return;
        }
        if (t.Kind == FighterKind.Summon)
        {
            t.IsDead = true;
            Emit(new BattleEvent { Kind = BattleEventKind.Died, ActorId = source?.Id, TargetId = t.Id, Motion = "die", Position = t.Position, Text = $"{t.Name} is gone." });
            return;
        }

        t.IsDown = true;
        t.Downs++;
        t.DeathSuccesses = 0;
        t.DeathFailures = 0;
        t.Stable = Encounter.Nonlethal;
        t.NextDeathSaveMs = NowMs + TurnMs;
        t.CurrentMotion = "downed";
        t.MotionStartMs = NowMs;
        Emit(new BattleEvent { Kind = BattleEventKind.Downed, ActorId = source?.Id, TargetId = t.Id, Motion = "downed", Position = t.Position, Text = $"{t.Name} is down!" });

        foreach (Fighter ally in _fighters)
        {
            if (ally == t || ally.Side != t.Side || !ally.IsStanding) continue;
            foreach (SkillDefinition passive in ally.PassiveSkills)
                foreach (Proc proc in passive.Procs)
                    if (proc.Trigger == ProcTrigger.OnAllyDropped && !HasStayUp(proc) && ProcReady(ally, passive, proc))
                        foreach (Effect effect in proc.Effects) ApplyLoose(ally, passive, effect, t, null);
        }
    }

    /// <summary>Someone leaves the fight standing: a guest falling back, a foe yielding or knocked out in a brawl, a summon whose time is up.</summary>
    private void Leave(Fighter t, string text)
    {
        if (t.HasLeft) return;
        Interrupt(t, "left");
        t.Action = null;
        t.Goal = null;
        t.HasLeft = true;
        EndConcentration(t);
        Emit(new BattleEvent { Kind = BattleEventKind.FellBack, ActorId = t.Id, TargetId = t.Id, Position = t.Position, Text = text });
    }

    /// <summary>A Downed party member's death saves, one every 3 s: a 20 gets them up on 1, three successes make them stable, three failures and they've fallen for the rest of the fight.</summary>
    private void TickDowned(Fighter f)
    {
        if (f.Stable || Encounter.Nonlethal || NowMs < f.NextDeathSaveMs) return;
        f.NextDeathSaveMs += TurnMs;
        int roll = 1 + (int)_rng.Below(20);
        bool success = roll >= 10;
        Emit(new BattleEvent { Kind = BattleEventKind.DeathSave, ActorId = f.Id, TargetId = f.Id, Amount = roll, Flags = success ? HitFlags.Save : HitFlags.None, Text = $"{f.Name}'s death save: {roll}." });
        if (roll == 20)
        {
            Revive(f, 1, null);
            return;
        }
        if (success) f.DeathSuccesses++;
        else f.DeathFailures += roll == 1 ? 2 : 1;
        if (f.DeathSuccesses >= 3) f.Stable = true;
        if (f.DeathFailures >= 3) Fall(f);
    }

    /// <summary>A foe's blow on someone Downed: a failed death save (two for a critical), and no longer stable.</summary>
    private void FailDeathSave(Fighter t, int failures)
    {
        if (Encounter.Nonlethal) return;
        t.Stable = false;
        t.DeathFailures += failures;
        Emit(new BattleEvent { Kind = BattleEventKind.DeathSave, ActorId = t.Id, TargetId = t.Id, Amount = 0, Text = $"{t.Name} is struck while down." });
        if (t.DeathFailures >= 3) Fall(t);
    }

    /// <summary>A party member who failed three death saves has fallen: out for the rest of the fight (they get up on 1 if it's won).</summary>
    private void Fall(Fighter t)
    {
        t.IsDown = false;
        t.IsDead = true;
        Emit(new BattleEvent { Kind = BattleEventKind.Died, TargetId = t.Id, Motion = "die", Position = t.Position, Text = $"{t.Name} has fallen." });
    }

    /// <summary>Gets a Downed party member up, on some hit points.</summary>
    internal void Revive(Fighter t, int hp, Fighter by)
    {
        if (!t.IsDown) return;
        t.IsDown = false;
        t.Stable = false;
        t.DeathSuccesses = 0;
        t.DeathFailures = 0;
        t.Hp = Math.Clamp(hp, 1, t.MaxHp);
        t.CurrentMotion = "revive";
        t.MotionStartMs = NowMs;
        t.NextThinkMs = NowMs + 500;
        t.SwingLeftMs = Math.Max(t.SwingLeftMs, SwingInterval(t) / 2);
        Emit(new BattleEvent { Kind = BattleEventKind.Revived, ActorId = by?.Id, TargetId = t.Id, Motion = "revive", Amount = t.Hp, Position = t.Position, Text = $"{t.Name} is back on their feet." });
    }

    // Healing.

    /// <summary>Heals someone: what the healer adds (a heal's flat and percentage, rank 9's tenth), and gets the Downed up at once; then threat (half a point a point, shared by the foes engaged), Devotion's Focus and a heal-another proc.</summary>
    /// <param name="healer">Who heals, or null.</param>
    /// <param name="t">Who's healed.</param>
    /// <param name="amount">How much, before the healer's mods.</param>
    /// <param name="skill">The skill, or null.</param>
    /// <param name="tick">Whether it's a tick of healing over time (no mods, no Devotion Focus).</param>
    /// <returns>The hit points that landed.</returns>
    internal int Heal(Fighter healer, Fighter t, int amount, SkillDefinition skill, bool tick)
    {
        if (!t.Present || t.IsDead || t.HasLeft) return 0;
        if (healer is not null && !tick && skill != Potion)
        {
            var context = new ModContext { Other = t };
            amount += (int)Sum(healer, Stat.HealingFlat, context);
            double pct = Sum(healer, Stat.HealingPct, context);
            if (skill?.Class is ClassId owner && owner == healer.ClassId && healer.Rank >= ClassRanks.PowerRank) pct += 10;
            if (pct != 0) amount = (int)Math.Round(amount * Math.Max(0.0, 1.0 + pct / 100.0));
        }
        double taken = Sum(t, Stat.HealingTakenPct, default);
        if (taken != 0) amount = (int)Math.Round(amount * Math.Max(0.0, 1.0 + taken / 100.0));
        if (amount <= 0) return 0;

        int landed;
        if (t.IsDown)
        {
            landed = Math.Min(amount, t.MaxHp);
            Revive(t, landed, healer);
        }
        else
        {
            landed = Math.Min(amount, t.MaxHp - t.Hp);
            t.Hp += landed;
        }
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.Healed, ActorId = healer?.Id, TargetId = t.Id, SkillId = skill?.Id, Amount = landed, Flags = tick ? HitFlags.Tick : HitFlags.None,
            Position = t.Position, Text = landed > 0 ? $"{t.Name} heals {landed}." : null,
        });

        if (healer is null) return landed;
        healer.HealingDone += landed;
        if (landed > 0 && healer.Side == Side.Party) HealingThreat(healer, landed);
        if (!tick && landed > 0 && healer.IsStanding) GainFocus(healer, FocusModels.Get(healer.FocusModel).PerHeal);
        if (!tick && healer != t && healer.IsStanding)
        {
            foreach (SkillDefinition passive in healer.PassiveSkills)
                foreach (Proc proc in passive.Procs)
                    if (proc.Trigger == ProcTrigger.OnHealOther && ProcReady(healer, passive, proc))
                        foreach (Effect effect in proc.Effects) ApplyLoose(healer, passive, effect, healer, null);
        }
        return landed;
    }

    /// <summary>What's left of a skill's healing pool this fight (Lay on Hands: so many hit points a level).</summary>
    internal int PoolLeft(Fighter f, SkillDefinition skill, Effect effect) =>
        f.Pools.TryGetValue(skill.Id, out int left) ? left : effect.PoolPerLevel * Math.Max(1, f.Level);

    // Help.

    /// <summary>Walks to a Downed ally and, once beside them and free, starts Help's channel.</summary>
    private void TickHelpOrder(Fighter f, Order order)
    {
        Fighter t = order.Target;
        if (t is null || !t.IsDown)
        {
            f.Order = null;
            return;
        }
        if (Vector2.Distance(f.Position, t.Position) > Ranges.Melee + 4f)
        {
            Approach(f, t, Ranges.Melee * 0.6f);
            return;
        }
        if (f.Action is not null || !CanAct(f)) return;
        f.Order = null;
        f.Goal = null;
        f.Helping = t;
        f.HelpUntilMs = NowMs + HelpMs;
        f.CurrentMotion = "channel";
        f.MotionStartMs = NowMs;
        Face(f, t.Position);
        Emit(new BattleEvent { Kind = BattleEventKind.HelpStarted, ActorId = f.Id, TargetId = t.Id, SkillId = "help", Motion = "channel", DurationMs = HelpMs, Position = t.Position, Text = $"{f.Name} helps {t.Name} up." });
    }

    /// <summary>Carries a Help channel on: broken if either moves apart or the helper can't act, and when it's done the ally's up on 1.</summary>
    private void TickHelp(Fighter f)
    {
        if (f.Helping is not Fighter t) return;
        if (!t.IsDown || !CanAct(f) || Vector2.Distance(f.Position, t.Position) > Ranges.Melee + 12f)
        {
            CancelHelp(f);
            return;
        }
        if (NowMs < f.HelpUntilMs) return;
        f.Helping = null;
        f.CurrentMotion = "ready";
        Revive(t, 1, f);
    }

    /// <summary>Stops a Help channel.</summary>
    internal void CancelHelp(Fighter f)
    {
        if (f.Helping is not Fighter t) return;
        f.Helping = null;
        if (t.IsDown) Emit(new BattleEvent { Kind = BattleEventKind.CastInterrupted, ActorId = f.Id, TargetId = t.Id, SkillId = "help", Text = $"{f.Name} stops helping {t.Name}." });
    }

    // Threat.

    /// <summary>Adds to a foe's threat table: so much threat for someone, by their class's threat (double for the defenders) and what they carry.</summary>
    internal void AddThreat(Fighter foe, Fighter who, double amount)
    {
        if (amount <= 0 || who.Side == foe.Side) return;
        double value = amount * who.ThreatPct / 100.0 * Math.Max(0.0, 1.0 + Sum(who, Stat.Threat, default) / 100.0);
        ThreatEntry entry = null;
        int at = -1;
        for (int i = 0; i < foe.ThreatTable.Count; i++)
        {
            if (foe.ThreatTable[i].Who != who) continue;
            entry = foe.ThreatTable[i];
            at = i;
            break;
        }
        if (entry is null)
        {
            entry = new ThreatEntry(who);
            foe.ThreatTable.Add(entry);
            at = foe.ThreatTable.Count - 1;
        }
        entry.Value += value;
        while (at > 0 && foe.ThreatTable[at - 1].Value < entry.Value)
        {
            foe.ThreatTable[at] = foe.ThreatTable[at - 1];
            foe.ThreatTable[at - 1] = entry;
            at--;
        }
    }

    /// <summary>Puts someone at the top of a foe's threat table, a tenth over whoever was (a taunt).</summary>
    private void TopThreat(Fighter foe, Fighter who)
    {
        double top = foe.ThreatTable.Count > 0 ? foe.ThreatTable[0].Value : 0;
        double mine = 0;
        foreach (ThreatEntry entry in foe.ThreatTable)
            if (entry.Who == who) mine = entry.Value;
        double need = top * 1.1 + 10 - mine;
        if (need > 0) AddThreat(foe, who, need * 100.0 / Math.Max(1, who.ThreatPct) / Math.Max(0.01, 1.0 + Sum(who, Stat.Threat, default) / 100.0));
    }

    /// <summary>Healing's threat: half a point a point, shared among the foes engaged in the fight.</summary>
    private void HealingThreat(Fighter healer, int amount)
    {
        int engaged = 0;
        foreach (Fighter foe in _fighters)
            if (foe.Side == Side.Foes && foe.IsStanding && foe.Target is not null) engaged++;
        if (engaged == 0) return;
        foreach (Fighter foe in _fighters)
            if (foe.Side == Side.Foes && foe.IsStanding && foe.Target is not null) AddThreat(foe, healer, amount * 0.5 / engaged);
    }

    // Focus.

    /// <summary>Gives (or takes) Focus, kept between 0 and 100.</summary>
    internal void GainFocus(Fighter f, int amount)
    {
        if (amount == 0) return;
        f.FocusMilli = Math.Clamp(f.FocusMilli + amount * 1000, 0, Fighter.MaxFocus * 1000);
    }

    /// <summary>Focus over time: the 1 a second everyone gets and their model's own, paid a tick at a time; and every 3 s, what they gain a turn (Arcane Recovery, rank 7's 5).</summary>
    private void GainFocusOverTime(Fighter f)
    {
        FocusRules rules = FocusModels.Get(f.FocusModel);
        f.FocusMilli = Math.Clamp(f.FocusMilli + (FocusModels.BaseRegen + rules.PerSecond) * TickMs, 0, Fighter.MaxFocus * 1000);
        if (NowMs < f.NextTurnMs) return;
        f.NextTurnMs += TurnMs;
        int perTurn = (int)Sum(f, Stat.FocusPerTurn, default) + (f.Member is not null && f.Rank >= ClassRanks.FocusRank ? 5 : 0);
        GainFocus(f, perTurn);
    }

    // Forms, smoke and summons.

    /// <summary>Ends a form when its time's up.</summary>
    private void TickForm(Fighter f)
    {
        if (f.Form is not null && NowMs >= f.FormUntilMs) EndForm(f);
    }

    /// <summary>Puts someone back as they were before their form.</summary>
    private void EndForm(Fighter f)
    {
        FormDefinition form = f.Form;
        f.Form = null;
        f.BarSkills.Clear();
        if (f.SavedBar is not null) f.BarSkills.AddRange(f.SavedBar);
        f.SavedBar = null;
        f.Auto = f.BarSkills.Count > 0 ? f.BarSkills[0] : f.Auto;
        f.SwingTotalMs = SwingInterval(f);
        Emit(new BattleEvent { Kind = BattleEventKind.Transformed, ActorId = f.Id, SkillId = form?.Id, Position = f.Position, Text = $"{f.Name} is themselves again." });
    }

    /// <summary>Brings someone out of their smoke when its time's up, behind their target, ready to strike.</summary>
    private void TickVanish(Fighter f)
    {
        if (f.VanishUntilMs <= 0 || NowMs < f.VanishUntilMs) return;
        f.VanishUntilMs = 0;
        Fighter target = f.Target is { IsStanding: true } t && Hostile(f, t) ? t : NearestFoe(f, attackingMe: false);
        if (target is not null)
        {
            Shove(f, target.Position - new Vector2(target.Facing * (Ranges.Melee * 0.6f + target.Size + f.Size), 0f), BattleEventKind.Moved, f);
            Face(f, target.Position);
            SetTarget(f, target);
            f.SwingLeftMs = 0;
        }
        Emit(new BattleEvent { Kind = BattleEventKind.Joined, ActorId = f.Id, TargetId = target?.Id, Position = f.Position, Text = $"{f.Name} steps out of the smoke{(target is null ? "" : " behind " + target.Name)}!" });
    }

    /// <summary>Sends a summon away when its time's up.</summary>
    private void TickSummon(Fighter f)
    {
        if (f.LeavesAtMs > 0 && NowMs >= f.LeavesAtMs) Leave(f, $"{f.Name} goes.");
    }

    // The Soul-Link.

    /// <summary>Whether the Soul-Link is in this fight: you and the Soul-Bound both in it, and the bond not broken.</summary>
    public bool SoulLinkActive
    {
        get
        {
            if (Rules.Has(BattleRules.SoulBondBroken)) return false;
            Fighter you = Find(Origins.You), bound = Find(Origins.SoulBound);
            return you is { Kind: FighterKind.Member } && bound is { Kind: FighterKind.Member };
        }
    }

    /// <summary>The Soul-Link's charges, 0 to 2: at two, either of the duo can spend it on the Soul-Link Strike.</summary>
    public int SoulLinkCharge => _soulCharges;

    /// <summary>The other half of the duo, or null.</summary>
    internal Fighter PartnerOf(Fighter f) =>
        f.Id == Origins.You ? Find(Origins.SoulBound) : f.Id == Origins.SoulBound ? Find(Origins.You) : null;

    /// <summary>Notes a hit by one of the duo on a foe: if the other hit the same foe within 3 s, the Soul-Link charges (once every 3 s at most).</summary>
    private void NoteDuoHit(Fighter hitter, Fighter foe)
    {
        if (hitter.Kind != FighterKind.Member || foe.Side != Side.Foes || hitter.Id is not (Origins.You or Origins.SoulBound) || !SoulLinkActive) return;
        _duoHits[hitter.Id + "|" + foe.Id] = NowMs;
        string partner = hitter.Id == Origins.You ? Origins.SoulBound : Origins.You;
        if (_soulCharges >= SoulLinkCharges || NowMs < _soulNextChargeMs) return;
        if (!_duoHits.TryGetValue(partner + "|" + foe.Id, out int at) || NowMs - at > SoulLinkWindowMs) return;
        _soulCharges++;
        _soulNextChargeMs = NowMs + SoulLinkWindowMs;
        if (_soulCharges >= SoulLinkCharges) _soulFullAtMs = NowMs;
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.SoulLinkCharged, ActorId = hitter.Id, TargetId = foe.Id, Amount = _soulCharges,
            Text = _soulCharges >= SoulLinkCharges ? "The Soul-Link is full!" : "The Soul-Link stirs.",
        });
    }

    /// <summary>Why one of the duo can't spend the Soul-Link on a foe now, or null if they can.</summary>
    public string SoulLinkProblem(Fighter f, Fighter target)
    {
        if (!SoulLinkActive) return "The Soul-Link is quiet";
        if (f is null || f.Id is not (Origins.You or Origins.SoulBound)) return "Only the two of you";
        if (_soulCharges < SoulLinkCharges) return "Not charged";
        if (!CanAct(f)) return "Can't act";
        if (f.Action is { InMotion: true }) return "Busy";
        if (target is null || !Hostile(f, target) || !target.IsStanding || !Targetable(target, SkillCatalog.Get(SkillCatalog.SoulLinkStrike))) return "No target";
        return null;
    }

    /// <summary>
    /// Whether a fighter's AI leaves a full Soul-Link alone for now: it's the player's to spend when
    /// they lead one of the duo and can act, for SoulLinkLeftToYouMs after it fills, so the 7 on the
    /// bar stays lit long enough to press. Down, stunned or slow to it, the Soul-Bound spends it.
    /// </summary>
    private bool LeavesSoulLink(Fighter f)
    {
        if (Leader is not Fighter leader || f == leader || !DrivenByPlayer(leader)) return false;
        if (leader.Id is not (Origins.You or Origins.SoulBound) || !leader.IsStanding || !CanAct(leader)) return false;
        return NowMs - _soulFullAtMs < SoulLinkLeftToYouMs;
    }

    /// <summary>The Soul-Link Strike: both of the duo strike the foe together (whichever of them can), with the duo's own clip.</summary>
    private void SoulLinkStrike(Fighter actor, Fighter target)
    {
        string why = SoulLinkProblem(actor, target);
        if (why is not null)
        {
            Deny(actor, SkillCatalog.SoulLinkStrike, why);
            return;
        }
        _soulCharges = 0;
        SkillDefinition skill = SkillCatalog.Get(SkillCatalog.SoulLinkStrike);
        Fighter partner = PartnerOf(actor);
        Emit(new BattleEvent { Kind = BattleEventKind.SoulLinkStrike, ActorId = actor.Id, TargetId = target.Id, SkillId = skill.Id, Motion = "soul-link-strike", Position = target.Position, Text = "Soul-Link Strike!" });
        if (partner?.Pending is { Kind: CommandKind.SoulLink }) partner.Pending = null;
        StartSoulLink(actor, skill, target, partner: false);
        if (partner is not null && CanAct(partner) && partner.Action is not { InMotion: true }) StartSoulLink(partner, skill, target, partner: true);
    }

    /// <summary>Starts one of the duo's half of the Soul-Link Strike.</summary>
    private void StartSoulLink(Fighter f, SkillDefinition skill, Fighter target, bool partner)
    {
        if (f.Action is not null) Interrupt(f, "the Soul-Link");
        CancelHelp(f);
        Face(f, target.Position);
        f.Uses[skill.Id] = f.Uses.TryGetValue(skill.Id, out int used) ? used + 1 : 1;
        if (!partner)
        {
            f.GcdTotalMs = GcdLength(f);
            f.GcdUntilMs = NowMs + f.GcdTotalMs;
        }
        var action = new ActionState { Skill = skill, Target = target, SoulLink = true, Partner = partner, Strikes = 1, Serial = (int)_sequence, Aim = target.Position };
        f.Action = action;
        f.Goal = null;
        BeginMotion(f, action);
    }
}
