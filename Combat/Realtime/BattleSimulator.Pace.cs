using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Characters;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// How a fight felt, measured while it runs: the heartbeat of your auto attack, how often you pressed a
/// skill and how often one was ready to press, how long a foe lasted once the party started on it, how
/// long a boss spent in each phase, and how much of the foes' time each member held them under control.
/// </summary>
/// <remarks>
/// 11-combat.md asks for fights that move at a steady pace: an auto about every 2 s, a skill to press
/// every 1.5 to 3 s, an ordinary foe down in about 8 to 15 s of the party's fire, and a boss that lasts
/// 75 to 180 s with every phase seen. A Pace watches one fight's events and samples it every 200 ms;
/// PaceTally adds up many. Nothing here changes the fight.
/// </remarks>
internal sealed class Pace
{
    /// <summary>How often the fight is sampled, in fight time.</summary>
    internal const int SampleMs = 200;

    /// <summary>The longest gap between two of your autos that still counts as the heartbeat (a longer one is you walking, down, or busy).</summary>
    internal const int LongestAutoGapMs = 6000;

    /// <summary>What counts as holding a foe under control: it can't act, can't move, can't reach, or fights badly because of you.</summary>
    internal static readonly HashSet<ConditionId> Control = new()
    {
        ConditionId.Stunned, ConditionId.Frozen, ConditionId.Restrained, ConditionId.Prone, ConditionId.Slowed,
        ConditionId.Charmed, ConditionId.Frightened, ConditionId.Blinded, ConditionId.Mocked,
    };

    private readonly Dictionary<string, int> _firstHurtMs = new(StringComparer.Ordinal);
    private readonly List<int> _phaseStarts = new();
    private int _lastAutoMs = -1;
    private string _bossId;

    /// <summary>The gaps between your autos that count, summed, and how many.</summary>
    public long AutoGapMs;
    public int AutoGaps;

    /// <summary>Your skill presses (not the auto, not the potion).</summary>
    public int Presses;

    /// <summary>The Focus your presses cost, summed; your Focus at the first and last samples; and the presses your bar allows a second by cooldowns and the GCD alone, with its mean Focus cost.</summary>
    public long FocusSpent;
    public int FocusFirst = -1, FocusLast;
    public double BarRate, BarCost;

    /// <summary>Samples you were standing in, and those of them with a skill (keys 2 to 5) you could press.</summary>
    public int StandingSamples;
    public int ReadySamples;

    /// <summary>Your Focus over the samples you were standing in, summed, and how many of them found it at 95 or more (nothing to spend it on).</summary>
    public long FocusSum;
    public int FocusFullSamples;

    /// <summary>Ordinary foes beaten (dead, knocked out or yielding), and the time from each one's first wound to its fall, summed.</summary>
    public int FoesBeaten;
    public long FoeLifeMs;

    /// <summary>The beaten ordinary foes' most hit points, summed; and the party's damage and the fight's length, for how long the whole party's fire takes to drop one.</summary>
    public long FoeMaxHp;
    public long PartyDamage;
    public int FightMs;

    /// <summary>Samples of standing foes, and each member's share of them they held under control.</summary>
    public long FoeSamples;
    public readonly Dictionary<string, long> ControlSamples = new(StringComparer.Ordinal);

    /// <summary>Each member's healing by their skills, potions left out (a potion is nobody's role).</summary>
    public readonly Dictionary<string, long> SkillHealing = new(StringComparer.Ordinal);

    /// <summary>A boss's time in each of its phases, when it fell (empty otherwise).</summary>
    public readonly List<int> PhaseMs = new();

    /// <summary>Takes in one tick's events, and samples the fight when a sample is due.</summary>
    public void Observe(Battle battle, IReadOnlyList<BattleEvent> events)
    {
        Fighter you = battle.Leader ?? battle.Fighters.FirstOrDefault(f => f.Id == Origins.You && f.Kind == FighterKind.Member);
        foreach (BattleEvent e in events)
        {
            switch (e.Kind)
            {
                case BattleEventKind.ActionStarted when you is not null && e.ActorId == you.Id && (e.Flags & HitFlags.Auto) != 0:
                    if (_lastAutoMs >= 0 && e.AtMs - _lastAutoMs <= LongestAutoGapMs)
                    {
                        AutoGapMs += e.AtMs - _lastAutoMs;
                        AutoGaps++;
                    }
                    _lastAutoMs = e.AtMs;
                    break;
                case BattleEventKind.SkillUsed when you is not null && e.ActorId == you.Id && e.SkillId != you.Auto?.Id && e.SkillId != "potion":
                    Presses++;
                    FocusSpent += e.Amount;
                    break;
                case BattleEventKind.Healed when e.ActorId is not null && e.SkillId != "potion":
                    SkillHealing[e.ActorId] = (SkillHealing.TryGetValue(e.ActorId, out long healed) ? healed : 0) + e.Amount;
                    break;
                case BattleEventKind.Damaged when e.TargetId is not null && !_firstHurtMs.ContainsKey(e.TargetId):
                    _firstHurtMs[e.TargetId] = e.AtMs;
                    break;
                case BattleEventKind.Died or BattleEventKind.FellBack:
                    Fighter dead = battle.Fighters.FirstOrDefault(f => f.Id == e.TargetId);
                    if (dead is null || dead.Side != Side.Foes || dead.Kind != FighterKind.Foe) break;
                    if (battle.IsBoss(dead))
                    {
                        if (dead.Id == _bossId) EndPhases(e.AtMs);
                    }
                    else if (_firstHurtMs.TryGetValue(dead.Id, out int from))
                    {
                        FoesBeaten++;
                        FoeLifeMs += e.AtMs - from;
                        FoeMaxHp += dead.MaxHp;
                    }
                    break;
                case BattleEventKind.PhaseChanged:
                    if (_bossId is null) Begin(e.ActorId, 0);
                    if (e.ActorId == _bossId) _phaseStarts.Add(e.AtMs);
                    break;
            }
        }

        if (battle.NowMs % SampleMs != 0) return;
        if (_bossId is null)
        {
            Fighter boss = battle.Fighters.FirstOrDefault(f => f.Side == Side.Foes && f.Enemy?.Phases.Count > 0);
            if (boss is not null) Begin(boss.Id, 0);
        }
        if (you is not null && you.IsStanding)
        {
            StandingSamples++;
            if (FocusFirst < 0) Bar(you);
            FocusLast = you.Focus;
            FocusSum += you.Focus;
            if (you.Focus >= 95) FocusFullSamples++;
            foreach (BarSlot slot in battle.Bar(you))
                if (slot.Key is >= 2 and <= 5 && slot.Usable)
                {
                    ReadySamples++;
                    break;
                }
        }
        var holding = new HashSet<string>(StringComparer.Ordinal);
        foreach (Fighter foe in battle.Fighters)
        {
            if (foe.Side != Side.Foes || !foe.IsStanding) continue;
            FoeSamples++;
            holding.Clear();
            foreach (Aura aura in foe.Auras)
                if (aura.SourceId is not null && aura.Definition.Condition is ConditionId condition && Control.Contains(condition)) holding.Add(aura.SourceId);
            foreach (string id in holding) ControlSamples[id] = (ControlSamples.TryGetValue(id, out long held) ? held : 0) + 1;
        }
    }

    /// <summary>What your bar allows: each active skill but the auto and the ultimate pressed as often as its cooldown (or the GCD) lets it, and what that costs on average.</summary>
    private void Bar(Fighter you)
    {
        FocusFirst = you.Focus;
        double cost = 0;
        foreach (SkillDefinition skill in you.Skills)
        {
            if (skill == you.Auto || skill.Ultimate || !skill.Active) continue;
            double every = Math.Max(skill.CooldownMs, skill.OnGcd ? 1500 : 500) / 1000.0;
            BarRate += 1 / every;
            cost += skill.Focus / every;
        }
        BarCost = BarRate > 0 ? cost / BarRate : 0;
    }

    /// <summary>Takes the fight's end: the party's damage and the length.</summary>
    public void Finish(Battle battle)
    {
        FightMs = battle.NowMs;
        PartyDamage = battle.Fighters.Where(f => f.Side == Side.Party).Sum(f => (long)f.DamageDone);
    }

    /// <summary>Starts timing a boss's phases.</summary>
    private void Begin(string bossId, int atMs)
    {
        _bossId = bossId;
        _phaseStarts.Clear();
        _phaseStarts.Add(atMs);
    }

    /// <summary>The boss fell: how long each phase lasted, from the first wound in the first.</summary>
    private void EndPhases(int atMs)
    {
        if (_firstHurtMs.TryGetValue(_bossId, out int firstWound) && _phaseStarts.Count > 0) _phaseStarts[0] = Math.Max(_phaseStarts[0], firstWound);
        PhaseMs.Clear();
        for (int i = 0; i < _phaseStarts.Count; i++) PhaseMs.Add((i + 1 < _phaseStarts.Count ? _phaseStarts[i + 1] : atMs) - _phaseStarts[i]);
    }
}

/// <summary>Many fights' Paces, added up.</summary>
internal sealed class PaceTally
{
    /// <summary>The least time a boss must spend in a phase for it to count as seen.</summary>
    internal const int PhaseSeenMs = 8000;

    private long _autoGapMs, _foeLifeMs, _foeSamples, _foeMaxHp, _partyDamage, _fightMs;
    private int _autoGaps, _presses, _standing, _ready, _foesBeaten, _bossFalls, _allSeen, _focusFull, _fights;
    private long _focusSum, _focusGained;
    private double _barRate, _barCost;
    private readonly List<long> _phaseMs = new();
    private readonly Dictionary<string, long> _control = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _healing = new(StringComparer.Ordinal);

    /// <summary>Adds one fight's Pace.</summary>
    public void Add(Pace pace)
    {
        _autoGapMs += pace.AutoGapMs;
        _autoGaps += pace.AutoGaps;
        _presses += pace.Presses;
        _standing += pace.StandingSamples;
        _ready += pace.ReadySamples;
        _focusSum += pace.FocusSum;
        if (pace.FocusFirst >= 0)
        {
            _fights++;
            _focusGained += pace.FocusSpent + pace.FocusLast - pace.FocusFirst;
            _barRate += pace.BarRate;
            _barCost += pace.BarCost;
        }
        _focusFull += pace.FocusFullSamples;
        _foesBeaten += pace.FoesBeaten;
        _foeLifeMs += pace.FoeLifeMs;
        _foeMaxHp += pace.FoeMaxHp;
        _partyDamage += pace.PartyDamage;
        _fightMs += pace.FightMs;
        _foeSamples += pace.FoeSamples;
        foreach ((string id, long held) in pace.ControlSamples) _control[id] = (_control.TryGetValue(id, out long sum) ? sum : 0) + held;
        foreach ((string id, long healed) in pace.SkillHealing) _healing[id] = (_healing.TryGetValue(id, out long sum) ? sum : 0) + healed;
        if (pace.PhaseMs.Count > 0)
        {
            _bossFalls++;
            if (pace.PhaseMs.All(ms => ms >= PhaseSeenMs)) _allSeen++;
            for (int i = 0; i < pace.PhaseMs.Count; i++)
            {
                if (_phaseMs.Count <= i) _phaseMs.Add(0);
                _phaseMs[i] += pace.PhaseMs[i];
            }
        }
    }

    /// <summary>The mean time between your autos, in seconds (0 if none).</summary>
    public double AutoEvery => _autoGaps == 0 ? 0 : _autoGapMs / 1000.0 / _autoGaps;

    /// <summary>The mean time between your skill presses while standing, in seconds (0 if none).</summary>
    public double PressEvery => _presses == 0 ? 0 : _standing * (Pace.SampleMs / 1000.0) / _presses;

    /// <summary>The share of your standing time with a skill ready to press.</summary>
    public double ReadyShare => _standing == 0 ? 0 : _ready / (double)_standing;

    /// <summary>
    /// How often your bar lets you press a skill, in seconds: its skills (not the auto or the ultimate)
    /// each as often as its cooldown or the GCD allows, or as often as the Focus you earned pays for their
    /// mean cost, whichever is slower. 0 with nothing on the bar but the auto.
    /// </summary>
    public double AvailableEvery
    {
        get
        {
            if (_fights == 0 || _barRate <= 0) return 0;
            double byCooldown = _fights / _barRate;
            double income = _focusGained / (_standing * (Pace.SampleMs / 1000.0));
            double byFocus = income <= 0 ? double.PositiveInfinity : _barCost / _fights / income;
            return Math.Max(byCooldown, byFocus);
        }
    }

    /// <summary>Your mean Focus while standing.</summary>
    public double MeanFocus => _standing == 0 ? 0 : _focusSum / (double)_standing;

    /// <summary>The share of your standing time with your Focus at 95 or more.</summary>
    public double FocusFullShare => _standing == 0 ? 0 : _focusFull / (double)_standing;

    /// <summary>The mean life of an ordinary foe from its first wound, in seconds (0 if none fell).</summary>
    public double FoeLasts => _foesBeaten == 0 ? 0 : _foeLifeMs / 1000.0 / _foesBeaten;

    /// <summary>How long the whole party's fire takes to drop an ordinary foe, in seconds: its most hit points over the party's damage a second (0 if none fell).</summary>
    public double FoeUnderFire => _foesBeaten == 0 || _partyDamage == 0 ? 0 : _foeMaxHp / (double)_foesBeaten / (_partyDamage * 1000.0 / _fightMs);

    /// <summary>Whether a boss fell in any of these fights.</summary>
    public bool HadBoss => _bossFalls > 0;

    /// <summary>A boss's mean seconds in each phase, over the fights it fell in.</summary>
    public IEnumerable<double> PhaseSeconds => _phaseMs.Select(ms => _bossFalls == 0 ? 0 : ms / 1000.0 / _bossFalls);

    /// <summary>The mean seconds from a boss's first wound to its fall.</summary>
    public double BossLasts => PhaseSeconds.Sum();

    /// <summary>The share of the boss's falls in which it spent at least PhaseSeenMs in every phase.</summary>
    public double AllPhasesSeen => _bossFalls == 0 ? 0 : _allSeen / (double)_bossFalls;

    /// <summary>A member's healing by their skills over all these fights (potions left out).</summary>
    public long SkillHealing(string id) => _healing.TryGetValue(id, out long healed) ? healed : 0;

    /// <summary>The share of the standing foes' time a member held one under their control.</summary>
    public double ControlShare(string id) => _foeSamples == 0 ? 0 : (_control.TryGetValue(id, out long held) ? held : 0) / (double)_foeSamples;
}
