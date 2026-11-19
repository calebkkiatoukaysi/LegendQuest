using System;
using System.Collections.Generic;
using System.Numerics;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// The encounter's script in real time (11-combat.md §5.2, §5.4, §6.1): supports acting on their
/// timers, waves walking in at their second (sooner if the field's clear), bosses changing phase at
/// shares of their health, a time limit to last, a health share to reach, and the orders a
/// conversation can give a fight that's going (FightOrder).
/// </summary>
public sealed partial class Battle
{
    /// <summary>How long the field has to stay clear before the next wave comes early.</summary>
    public const int ClearWaitMs = 1500;

    private readonly List<SupportRun> _supports = new();
    private readonly List<WaveRun> _waves = new();
    private int _clearSinceMs = -1;
    private int _entry;

    /// <summary>Sets the encounter's script up: its supports (those whose flag is set), its waves, and its foes' moves on their first cooldowns.</summary>
    private void SetUpEncounter()
    {
        foreach (EncounterSupport support in Encounter.Supports)
        {
            if (support.When is not null && !Rules.Has(support.When)) continue;
            _supports.Add(new SupportRun(support) { NextMs = Math.Max(TickMs, support.StartsAtSeconds * 1000) });
        }
        for (int i = 0; i < Encounter.Waves.Count; i++)
            _waves.Add(new WaveRun(Encounter.Waves[i], i) { AtMs = Math.Max(TickMs, Encounter.Waves[i].ArrivesAtSeconds * 1000) });
        for (int i = 0; i < _fighters.Count; i++) _fighters[i].NextThinkMs = i * TickMs % 300;
    }

    /// <summary>Runs the encounter's script for a tick: supports, waves, phases.</summary>
    private void RunEncounter()
    {
        foreach (SupportRun run in _supports)
        {
            if (run.Off || run.Support.Ability is null || run.Support.Ability.Target == SupportTarget.None || NowMs < run.NextMs) continue;
            int stops = run.Support.StopsAtSeconds * 1000;
            if (stops > 0 && NowMs > stops)
            {
                run.Off = true;
                continue;
            }
            SupportActs(run);
            run.LastMs = NowMs;
            run.NextMs += Math.Max(1, run.Support.EverySeconds) * 1000;
        }

        bool clear = true;
        foreach (Fighter f in _fighters)
            if (f.Side == Side.Foes && f.IsStanding) clear = false;
        if (!clear) _clearSinceMs = -1;
        else if (_clearSinceMs < 0) _clearSinceMs = NowMs;

        foreach (WaveRun wave in _waves)
        {
            if (wave.Arrived) continue;
            bool early = clear && NowMs - _clearSinceMs >= ClearWaitMs;
            if (NowMs >= wave.AtMs || early) ArriveWave(wave);
            break;
        }

        int count = _fighters.Count;
        for (int i = 0; i < count; i++)
        {
            Fighter f = _fighters[i];
            if (f.Side != Side.Foes || !f.IsStanding || f.Enemy is null) continue;
            IReadOnlyList<FoePhase> phases = f.Enemy.Phases;
            while (f.Phase - 1 < phases.Count && f.HpShare < phases[f.Phase - 1].AtShare) EnterPhase(f, phases[f.Phase - 1]);
        }
    }

    /// <summary>A support behind the fight acts: its dice and its condition on whoever it's aimed at.</summary>
    private void SupportActs(SupportRun run)
    {
        SupportAbility ability = run.Support.Ability;
        var targets = new List<Fighter>();
        switch (ability.Target)
        {
            case SupportTarget.RandomFoe:
                if (Pick(Side.Foes) is Fighter random) targets.Add(random);
                break;
            case SupportTarget.Foe:
                foreach (Fighter f in _fighters)
                    if (f.Side == Side.Foes && f.IsStanding && (f.Id == ability.Foe || f.EnemyId == ability.Foe)) { targets.Add(f); break; }
                break;
            case SupportTarget.FoeRow:
                if (Pick(Side.Foes) is Fighter centre)
                    foreach (Fighter f in _fighters)
                        if (f.Side == Side.Foes && f.IsStanding && Vector2.Distance(f.Position, centre.Position) <= RowPx + f.Size) targets.Add(f);
                break;
            case SupportTarget.AllFoes:
                foreach (Fighter f in _fighters)
                    if (f.Side == Side.Foes && f.IsStanding) targets.Add(f);
                break;
            case SupportTarget.YourFrontRow:
            case SupportTarget.AllAllies:
                foreach (Fighter f in _fighters)
                    if (f.Side == Side.Party && f.IsStanding) targets.Add(f);
                break;
        }
        if (targets.Count == 0) return;

        foreach (Fighter t in targets)
        {
            int amount = ability.Damage.IsNone ? 0 : Math.Max(0, ability.Damage.Roll(_rng, run.Support.Name).Total);
            Emit(new BattleEvent
            {
                Kind = BattleEventKind.SupportActed, ActorId = run.Support.Id, TargetId = t.Id, Amount = amount, DamageType = ability.Type, Position = Arena.SupportSpot,
                DurationMs = Math.Max(1, run.Support.EverySeconds) * 1000, Text = $"{run.Support.Name}: {ability.Text}.",
            });
            if (amount > 0 && t.Side == Side.Foes) Hurt(t, amount, ability.Type, null, HitFlags.None, null, ignoreResistance: false);
            else if (amount > 0) Heal(null, t, amount, null, tick: false);
            if (ability.Condition is ConditionId condition && t.IsStanding)
                ApplyAura(t, new AuraDefinition { Id = "support-" + condition.ToString().ToLowerInvariant(), Name = condition.ToString(), Condition = condition, DurationMs = Math.Max(TickMs, ability.LastsMs) }, null, null, 0);
        }
    }

    /// <summary>A wave walks in from the room's edges to its slots, with its bark.</summary>
    private void ArriveWave(WaveRun wave)
    {
        wave.Arrived = true;
        _clearSinceMs = -1;
        if (!string.IsNullOrWhiteSpace(wave.Wave.Bark)) Emit(new BattleEvent { Kind = BattleEventKind.Bark, Text = wave.Wave.Bark });
        for (int i = 0; i < wave.Wave.Foes.Count; i++)
        {
            EncounterFoe foe = wave.Wave.Foes[i];
            Vector2 from = Arena.Entries.Count > 0 ? Arena.Entries[_entry++ % Arena.Entries.Count] : new Vector2(Arena.Right, Arena.Middle.Y);
            Fighter f = Spawn(foe.Enemy, $"{foe.Enemy}-w{wave.Index + 1}-{i + 1}", Side.Foes, FighterKind.Foe, from, Arena.FoeStart(foe.Slot));
            if (NearestFoe(f, attackingMe: false) is Fighter nearest) SetTarget(f, nearest);
        }
    }

    /// <summary>A boss enters its next phase: whatever it was doing stops, it says so, and its moves, speed and cooldowns change; it may call foes in.</summary>
    private void EnterPhase(Fighter f, FoePhase phase)
    {
        f.Phase++;
        Interrupt(f, phase.Name);
        f.Action = null;
        Emit(new BattleEvent { Kind = BattleEventKind.PhaseChanged, ActorId = f.Id, Amount = f.Phase, Text = phase.Name, Flags = HitFlags.Big, Position = f.Position });
        if (!string.IsNullOrWhiteSpace(phase.Bark)) Emit(new BattleEvent { Kind = BattleEventKind.Bark, ActorId = f.Id, Text = phase.Bark });

        foreach (string removed in phase.Removes) f.FoeSkills.Remove(removed);
        foreach (string added in phase.Adds)
        {
            if (f.FoeSkills.Contains(added)) continue;
            f.FoeSkills.Add(added);
            f.CooldownUntil[added] = NowMs + 2500;
        }
        foreach (KeyValuePair<string, int> cooldown in phase.Cooldowns)
        {
            f.CooldownOverride[cooldown.Key] = cooldown.Value;
            if (f.CooldownUntil.TryGetValue(cooldown.Key, out int until) && until > NowMs + cooldown.Value) f.CooldownUntil[cooldown.Key] = NowMs + cooldown.Value;
        }
        if (phase.HastePct != 0)
            ApplyAura(f, new AuraDefinition { Id = "phase-" + f.Phase, Name = phase.Name, Beneficial = true, Mods = new[] { new StatMod(Stat.Haste, phase.HastePct) } }, f, null, 0);
        f.SwingTotalMs = SwingInterval(f);

        for (int n = 0; n < phase.SummonCount && phase.Summons is not null; n++)
        {
            Vector2 from = Arena.Entries.Count > 0 ? Arena.Entries[_entry++ % Arena.Entries.Count] : new Vector2(Arena.Right, Arena.Middle.Y);
            Vector2 to = Arena.Clamp(f.Position + new Vector2(-30f - 12f * n, (n % 2 == 0 ? 1f : -1f) * 14f));
            _summoned++;
            Fighter called = Spawn(phase.Summons, $"{phase.Summons}-p{f.Phase}-{_summoned}", Side.Foes, FighterKind.Foe, from, to);
            if (NearestFoe(called, attackingMe: false) is Fighter nearest) SetTarget(called, nearest);
        }
    }

    /// <summary>Whether a foe hasn't come yet: a wave still to walk in, or someone joining later.</summary>
    private bool WavesLeft
    {
        get
        {
            foreach (WaveRun wave in _waves)
                if (!wave.Arrived) return true;
            foreach (Fighter f in _fighters)
                if (f.Side == Side.Foes && !f.Present && !f.HasLeft) return true;
            return false;
        }
    }

    /// <summary>Whether a fight you only have to last has been lasted.</summary>
    private bool SurvivedLongEnough() => Encounter.SurviveSeconds > 0 && NowMs >= Encounter.SurviveSeconds * 1000;

    /// <summary>Whether a fight that ends at a share of a foe's hit points (or all of theirs together) is there.</summary>
    private bool HealthShareReached()
    {
        if (Encounter.EndsAtHealthShare <= 0f) return false;
        long hp = 0, most = 0;
        foreach (Fighter f in _fighters)
        {
            if (f.Side != Side.Foes || !f.Present) continue;
            if (Encounter.EndsAtFoe is not null && f.Id != Encounter.EndsAtFoe && f.EnemyId != Encounter.EndsAtFoe) continue;
            hp += f.IsStanding ? f.Hp : 0;
            most += f.MaxHp;
        }
        return most > 0 && hp <= most * Encounter.EndsAtHealthShare;
    }

    /// <summary>How far through a fight you only have to last it is, 0 to 1 (a progress ring), or 0 for one that isn't that kind.</summary>
    public double SurviveShare => Encounter.SurviveSeconds > 0 ? Math.Min(1.0, NowMs / (Encounter.SurviveSeconds * 1000.0)) : 0.0;

    /// <summary>The supports behind the fight, for their banners and timer rings.</summary>
    public IReadOnlyList<SupportView> Supports
    {
        get
        {
            var list = new List<SupportView>();
            foreach (SupportRun run in _supports)
                list.Add(new SupportView(run.Support.Id, run.Support.Name, run.Support.Figure, run.Off, run.NextMs, Math.Max(1, run.Support.EverySeconds) * 1000,
                    run.LastMs, run.Support.Progress));
            return list;
        }
    }

    /// <summary>Applies a conversation's order to the fight that's going: a support stands down, foes are added, a guest joins.</summary>
    /// <param name="order">The order.</param>
    public void Apply(FightOrder order)
    {
        switch (order.Kind)
        {
            case FightOrderKind.SupportOff:
                foreach (SupportRun run in _supports)
                    if (run.Support.Id == order.Id) run.Off = true;
                break;
            case FightOrderKind.AddFoes:
                for (int n = 0; n < Math.Max(1, order.Count); n++)
                {
                    Vector2 from = Arena.Entries.Count > 0 ? Arena.Entries[_entry++ % Arena.Entries.Count] : new Vector2(Arena.Right, Arena.Middle.Y);
                    _summoned++;
                    Fighter f = Spawn(order.Id, $"{order.Id}-o{_summoned}", Side.Foes, FighterKind.Foe, from, Arena.Clamp(from - new Vector2(60f, 0f)));
                    if (NearestFoe(f, attackingMe: false) is Fighter nearest) SetTarget(f, nearest);
                }
                break;
            case FightOrderKind.AddGuest:
                if (EnemyCatalog.Exists(order.Id))
                {
                    _summoned++;
                    Vector2 from = new(Arena.Left, Arena.Middle.Y);
                    Spawn(order.Id, $"{order.Id}-g{_summoned}", Side.Party, FighterKind.Guest, from, Arena.Clamp(from + new Vector2(60f, 0f)));
                }
                break;
        }
    }

    /// <summary>A support's state through the fight.</summary>
    private sealed class SupportRun
    {
        public SupportRun(EncounterSupport support) => Support = support;

        public EncounterSupport Support { get; }

        public int NextMs { get; set; }

        public int LastMs { get; set; } = -1;

        public bool Off { get; set; }
    }

    /// <summary>A wave's state through the fight.</summary>
    private sealed class WaveRun
    {
        public WaveRun(EncounterWave wave, int index)
        {
            Wave = wave;
            Index = index;
        }

        public EncounterWave Wave { get; }

        public int Index { get; }

        public int AtMs { get; set; }

        public bool Arrived { get; set; }
    }
}

/// <summary>A support behind the fight, as its banner shows it.</summary>
/// <param name="Id">Its id.</param>
/// <param name="Name">Its name.</param>
/// <param name="Figure">Who's drawn for it.</param>
/// <param name="Off">Whether it's stood down.</param>
/// <param name="NextMs">When it next acts, in fight time.</param>
/// <param name="EveryMs">How often it acts.</param>
/// <param name="LastMs">When it last acted, or -1.</param>
/// <param name="Progress">Whether its ring shows the fight's progress to its time limit (Battle.SurviveShare) instead.</param>
public sealed record SupportView(string Id, string Name, string Figure, bool Off, int NextMs, int EveryMs, int LastMs, bool Progress);
