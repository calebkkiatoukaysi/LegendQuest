using System;
using System.Collections.Generic;
using System.Numerics;
using LegendQuest.Characters;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// A real-time fight: everyone in it, stepped in fixed 20 ms ticks from a seed, taking commands and
/// handing back what happened each tick.
/// </summary>
/// <remarks>
/// <para>
/// 11-combat.md, and the contract's §3 API. The fight draws nothing and reads no clock: time is NowMs,
/// whole milliseconds, moved on by Step one tick at a time, and every roll comes from the fight's own
/// Pcg32, seeded once. So the game (which calls Step from Update with an accumulator) and the
/// simulator (which calls it in a loop) play exactly the same fight from the same seed and the same
/// commands. Commands are queued (Command) and applied at the start of the next tick.
/// </para>
/// <para>
/// Each tick, in this order: the queued commands; the encounter's script (waves, guests, supports,
/// a time limit); every blow and projectile due; then each fighter in turn (auras, Focus, death saves,
/// a Help channel, a cast finishing, the swing timer, their orders, their auto attack, their walk);
/// keeping people from standing in one another; the AI for whoever is free to think; and the outcome.
/// Everything walks lists in a fixed order, so nothing depends on a hash's order.
/// </para>
/// <para>
/// The partials: Battle.Join (who's in it), Battle.Actions (using skills, casts, telegraphs, blows),
/// Battle.Effects (what a blow does), Battle.Rolls (the 5e sheet: attacks, saves, damage, the stats),
/// Battle.Auras, Battle.Life (hurt, healed, downed, threat, Focus, the Soul-Link), Battle.Encounter
/// (waves, supports, phases, endings), Battle.Movement, Battle.Queries (for the HUD), and the brains
/// in Battle.Brains.
/// </para>
/// </remarks>
public sealed partial class Battle
{
    /// <summary>One tick of the rules, in milliseconds.</summary>
    public const int TickMs = 20;

    /// <summary>A turn of the old tables, in milliseconds: an aura's tick, a death save.</summary>
    public const int TurnMs = 3000;

    private readonly List<Fighter> _fighters = new();
    private readonly List<BattleEvent> _events = new();
    private readonly List<BattleCommand> _queue = new();
    private readonly List<Scheduled> _scheduled = new();
    private readonly Pcg32 _rng;
    private long _sequence;

    /// <summary>Sets a fight up: everyone in the setups joins (those with a FromSeconds later), at their places on the arena.</summary>
    /// <param name="encounter">The fight.</param>
    /// <param name="setups">Who's in it from the start: the party, its guests, the foes (EncounterRoster builds them).</param>
    /// <param name="rules">The difficulty, the flags, the leader, the bag.</param>
    /// <param name="seed">The seed every roll comes from.</param>
    /// <param name="arena">The floor; null for BattleArena.Default.</param>
    public Battle(EncounterDefinition encounter, IReadOnlyList<CombatantSetup> setups, BattleRules rules, ulong seed, BattleArena arena = null)
    {
        Encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
        Rules = rules ?? new BattleRules(Difficulty.Balanced);
        Arena = arena ?? BattleArena.Default();
        Seed = seed;
        _rng = new Pcg32(seed);

        int partyPlace = 0;
        foreach (CombatantSetup setup in setups) Join(setup, ref partyPlace);
        Leader = Rules.LeaderId is null ? null : Find(Rules.LeaderId);
        SetUpEncounter();
        foreach (Fighter fighter in _fighters) StartProcs(fighter);
    }

    /// <summary>The fight.</summary>
    public EncounterDefinition Encounter { get; }

    /// <summary>What it's played by.</summary>
    public BattleRules Rules { get; }

    /// <summary>The floor.</summary>
    public BattleArena Arena { get; }

    /// <summary>The seed it was started from.</summary>
    public ulong Seed { get; }

    /// <summary>How long it's been going, in milliseconds.</summary>
    public int NowMs { get; private set; }

    /// <summary>How it stands.</summary>
    public FightOutcome Outcome { get; private set; }

    /// <summary>Everyone who's in it or has been: the party's side and the foes, in the order they joined (which is the order everything's done in).</summary>
    public IReadOnlyList<Fighter> Fighters => _fighters;

    /// <summary>The fighter the player drives, or null when the AI drives everyone.</summary>
    public Fighter Leader { get; }

    /// <summary>The dice, for the brains (so their choices replay from the seed too).</summary>
    internal Pcg32 Random => _rng;

    /// <summary>Advances the fight one tick and returns everything that happened in it, in order.</summary>
    public IReadOnlyList<BattleEvent> Step()
    {
        if (Outcome != FightOutcome.Ongoing)
        {
            var left = new List<BattleEvent>(_events);
            _events.Clear();
            return left;
        }

        NowMs += TickMs;
        ApplyCommands();
        RunEncounter();
        RunScheduled();
        foreach (Fighter fighter in _fighters) Tick(fighter);
        Separate();
        Think();
        Sweep();
        CheckOutcome();

        var happened = new List<BattleEvent>(_events);
        _events.Clear();
        return happened;
    }

    /// <summary>Queues a command for the start of the next tick.</summary>
    /// <param name="command">The command.</param>
    public void Command(BattleCommand command)
    {
        if (command is not null) _queue.Add(command);
    }

    /// <summary>A fighter by id, or null.</summary>
    /// <param name="id">Their id.</param>
    public Fighter Find(string id)
    {
        if (id is null) return null;
        foreach (Fighter fighter in _fighters)
            if (fighter.Id == id) return fighter;
        return null;
    }

    /// <summary>Writes the party's hit points back onto their PartyMembers, for after the fight: a won fight gets the fallen up on 1, then the after-fight passives heal them.</summary>
    /// <remarks>Guests and summons are nobody's to write to. Call it once, when the fight's over (or after a concede, to keep the damage).</remarks>
    public void ApplyToParty()
    {
        foreach (Fighter fighter in _fighters)
        {
            if (fighter.Kind != FighterKind.Member || fighter.Member is null) continue;
            int hp = fighter.IsDown || fighter.IsDead ? (Outcome == FightOutcome.Won ? 1 : 0) : fighter.Hp;
            fighter.Member.Hp = Math.Clamp(hp, 0, fighter.Member.MaxHp);
        }
        if (Outcome != FightOutcome.Won) return;

        // After a won fight, the passives that heal the party (Natural Recovery's tenth, Song of Rest's level).
        foreach (Fighter healer in _fighters)
        {
            if (healer.Kind != FighterKind.Member || healer.Member is null) continue;
            foreach (SkillDefinition passive in healer.PassiveSkills)
                foreach (Proc proc in passive.Procs)
                {
                    if (proc.Trigger != ProcTrigger.AfterFight) continue;
                    foreach (Effect effect in proc.Effects)
                    {
                        if (effect.Kind != EffectType.Heal) continue;
                        foreach (Fighter member in _fighters)
                        {
                            if (member.Kind != FighterKind.Member || member.Member is not PartyMember party) continue;
                            int amount = (int)Math.Round(party.MaxHp * effect.MaxHpShare) + effect.LevelTimes * healer.Level + effect.Flat;
                            party.Hp = Math.Clamp(party.Hp + amount, 0, party.MaxHp);
                        }
                    }
                }
        }
    }

    // Events.

    /// <summary>Adds an event to this tick's.</summary>
    internal void Emit(BattleEvent happened) => _events.Add(happened with { AtMs = NowMs });

    /// <summary>Adds an event about an actor and a target to this tick's.</summary>
    internal void Emit(BattleEventKind kind, Fighter actor, Fighter target = null, string text = null, int amount = 0, string skill = null,
        HitFlags flags = HitFlags.None, DamageType type = default)
    {
        _events.Add(new BattleEvent
        {
            Kind = kind, AtMs = NowMs, ActorId = actor?.Id, TargetId = target?.Id, Text = text, Amount = amount, SkillId = skill, Flags = flags,
            DamageType = type, Position = (target ?? actor)?.Position ?? default,
        });
    }

    // Scheduling: blows, arrivals, anything at a later tick.

    /// <summary>Runs something at a moment: at the first tick on or after it, after the tick's commands and script, in the order scheduled.</summary>
    /// <param name="atMs">When.</param>
    /// <param name="what">What.</param>
    internal void At(int atMs, Action what)
    {
        var item = new Scheduled(Math.Max(atMs, NowMs), _sequence++, what);
        int i = _scheduled.Count;
        while (i > 0 && Compare(_scheduled[i - 1], item) > 0) i--;
        _scheduled.Insert(i, item);
    }

    /// <summary>Runs everything due by now, in time and then scheduling order; what that schedules for now runs too.</summary>
    private void RunScheduled()
    {
        while (_scheduled.Count > 0 && _scheduled[0].AtMs <= NowMs)
        {
            Scheduled next = _scheduled[0];
            _scheduled.RemoveAt(0);
            next.What();
            if (Outcome != FightOutcome.Ongoing) return;
        }
    }

    /// <summary>Orders scheduled things: by time, then by when they were scheduled.</summary>
    private static int Compare(Scheduled a, Scheduled b) => a.AtMs != b.AtMs ? a.AtMs.CompareTo(b.AtMs) : a.Sequence.CompareTo(b.Sequence);

    /// <summary>Something waiting for its moment.</summary>
    private readonly record struct Scheduled(int AtMs, long Sequence, Action What);

    // The outcome.

    /// <summary>Clears away whatever came off this tick: auras that are gone.</summary>
    private void Sweep()
    {
        foreach (Fighter fighter in _fighters)
            fighter.AuraList.RemoveAll(aura => aura.Gone);
    }

    /// <summary>Gives the fight up.</summary>
    private void Concede()
    {
        if (Outcome != FightOutcome.Ongoing) return;
        Outcome = FightOutcome.Conceded;
        Emit(BattleEventKind.FightLost, null, text: "The fight is given up.");
    }

    /// <summary>Wins or loses it, if that's how it stands: every foe beaten with no wave to come (or a time limit lasted, or a health share reached), or every party member down.</summary>
    private void CheckOutcome()
    {
        if (Outcome != FightOutcome.Ongoing) return;

        bool members = false, standing = false;
        foreach (Fighter fighter in _fighters)
        {
            if (fighter.Side != Side.Party || fighter.Kind != FighterKind.Member || !fighter.Present) continue;
            members = true;
            standing |= fighter.IsStanding;
        }
        if (!members)
        {
            foreach (Fighter fighter in _fighters)
                standing |= fighter.Side == Side.Party && fighter.IsStanding;
        }
        if (!standing)
        {
            Outcome = FightOutcome.Lost;
            Emit(BattleEventKind.FightLost, null, text: "The party has fallen.");
            return;
        }

        if (SurvivedLongEnough() || HealthShareReached() || AllFoesBeaten())
        {
            Outcome = FightOutcome.Won;
            foreach (Fighter fighter in _fighters)
            {
                if (fighter.Side != Side.Party || !(fighter.IsDown || fighter.IsDead) || fighter.Kind != FighterKind.Member) continue;
                fighter.IsDown = false;
                fighter.IsDead = false;
                fighter.Hp = 1;
            }
            Emit(BattleEventKind.FightWon, null, text: $"{Encounter.Name}: won.");
        }
    }

    /// <summary>Whether every foe who's come is beaten and nobody's left to come.</summary>
    private bool AllFoesBeaten()
    {
        if (WavesLeft) return false;
        foreach (Fighter fighter in _fighters)
            if (fighter.Side == Side.Foes && fighter.IsStanding) return false;
        return true;
    }

    // One fighter's tick.

    /// <summary>Everything that happens to one fighter in a tick.</summary>
    private void Tick(Fighter f)
    {
        if (!f.Present || f.IsDead || f.HasLeft) return;

        TickAuras(f);
        if (f.IsDown)
        {
            TickDowned(f);
            return;
        }

        RecordHp(f);
        GainFocusOverTime(f);
        TickForm(f);
        TickVanish(f);
        TickSummon(f);
        if (!f.IsStanding) return;

        TickHelp(f);
        TickCast(f);
        TickSwing(f);
        TickOrder(f);
        TickAuto(f);
        Resettle(f);
        TickArmed(f);
        Walk(f);
        TickThreatHeld(f);
    }

    /// <summary>Keeps a second-by-second record of their hit points, for Rewind.</summary>
    private void RecordHp(Fighter f)
    {
        if (NowMs % 1000 != 0) return;
        int slot = NowMs / 1000 % f.HpHistory.Length;
        f.HpHistory[slot] = f.Hp;
    }

    /// <summary>Their hit points a number of seconds ago (as near as the record has it), or now if it doesn't go back that far.</summary>
    internal int HpAgo(Fighter f, int ms)
    {
        int seconds = Math.Clamp(ms / 1000, 1, f.HpHistory.Length - 1);
        if (NowMs / 1000 < seconds) return f.Hp;
        return f.HpHistory[(NowMs / 1000 - seconds) % f.HpHistory.Length];
    }

    /// <summary>Counts the time a fighter spends as the top of some foe's table, for the simulator's "threat held".</summary>
    private void TickThreatHeld(Fighter f)
    {
        if (f.Side != Side.Party) return;
        foreach (Fighter foe in _fighters)
        {
            if (foe.Side != Side.Foes || !foe.IsStanding || foe.Target != f) continue;
            f.ThreatHeldMs += TickMs;
            return;
        }
    }

    // Commands.

    /// <summary>Applies every queued command, in the order they came.</summary>
    private void ApplyCommands()
    {
        if (_queue.Count == 0) return;
        var commands = new List<BattleCommand>(_queue);
        _queue.Clear();
        foreach (BattleCommand command in commands) Apply(command);
    }

    /// <summary>Applies one command.</summary>
    private void Apply(BattleCommand command)
    {
        if (command.Kind == CommandKind.Concede)
        {
            Concede();
            return;
        }

        Fighter actor = Find(command.ActorId);
        if (actor is null || !actor.Present || actor.IsDead || actor.HasLeft) return;
        Fighter target = Find(command.TargetId);

        switch (command.Kind)
        {
            case CommandKind.Target:
                SetTarget(actor, target);
                break;
            case CommandKind.MoveTo:
                if (actor.IsDown) return;
                actor.Order = null;
                actor.Chase = false;
                CancelHelp(actor);
                if (actor.Action is { InMotion: false } casting && casting.Skill.Interruptible && !casting.Auto) Interrupt(actor, "moved");
                SetGoal(actor, Arena.Clamp(command.Point));
                break;
            case CommandKind.Use:
                Use(actor, command.SkillId, target);
                break;
            case CommandKind.Potion:
                DrinkPotion(actor);
                break;
            case CommandKind.SoulLink:
                SoulLinkStrike(actor, target ?? actor.Target);
                break;
            case CommandKind.AutoAttack:
                actor.AutoAttackOn = command.On && actor.Target is { } t && Hostile(actor, t);
                break;
            case CommandKind.Help:
                if (target is null || !target.IsDown || target.Side != actor.Side || actor.IsDown) return;
                actor.Order = new Order { Kind = CommandKind.Help, Target = target };
                break;
            case CommandKind.Stop:
                actor.Order = null;
                actor.Goal = null;
                actor.Chase = false;
                CancelHelp(actor);
                if (actor.Action is { InMotion: false } held && !held.Auto) Interrupt(actor, "stopped");
                break;
        }
    }

    /// <summary>Points someone at a target: a foe turns their auto attack on, an ally (for heals) leaves it as it was; null clears it and stops the auto attack.</summary>
    internal void SetTarget(Fighter actor, Fighter target)
    {
        if (target is not null && (!target.Present || target.IsDead || target.HasLeft)) return;
        actor.Target = target;
        actor.Chase = target is not null && Hostile(actor, target);
        if (target is null) actor.AutoAttackOn = false;
        else if (Hostile(actor, target)) actor.AutoAttackOn = true;
    }

    /// <summary>Whether two fighters are on opposite sides.</summary>
    internal static bool Hostile(Fighter a, Fighter b) => a is not null && b is not null && a.Side != b.Side;
}
