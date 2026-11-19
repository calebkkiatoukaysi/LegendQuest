using System;
using System.Numerics;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// One thing that happened in a fight, at a moment: what the screen plays as a cue (a clip, a number,
/// a sound, a ground marker) and the log prints as a line.
/// </summary>
/// <remarks>
/// Contract §3. Every event is stamped with the tick it happened on (AtMs), and a blow's Hit and
/// Damaged are stamped with the tick it lands on, so the screen shows the number on the frame the blow
/// lands. ActionStarted carries the clip and when its blows will land (ImpactMs, the first, and
/// DurationMs, the clip's length). Fields that don't apply to a kind are left at their defaults.
/// </remarks>
public sealed record BattleEvent
{
    /// <summary>What happened.</summary>
    public BattleEventKind Kind { get; init; }

    /// <summary>When, in milliseconds since the fight began.</summary>
    public int AtMs { get; init; }

    /// <summary>Who did it, by fighter id, or null.</summary>
    public string ActorId { get; init; }

    /// <summary>Who it was done to, by fighter id, or null.</summary>
    public string TargetId { get; init; }

    /// <summary>The skill, by id, or null.</summary>
    public string SkillId { get; init; }

    /// <summary>The clip the actor plays for it, or null.</summary>
    public string Motion { get; init; }

    /// <summary>The number: damage, healing, Focus, a death save's roll, a phase's number.</summary>
    public int Amount { get; init; }

    /// <summary>What sort of blow it was.</summary>
    public HitFlags Flags { get; init; }

    /// <summary>The kind of damage, for colours and sounds.</summary>
    public DamageType DamageType { get; init; }

    /// <summary>Where: a ground marker's centre, where someone moved to, where a projectile was let go.</summary>
    public Vector2 Position { get; init; }

    /// <summary>A ground marker or an area's shape, for a telegraph or a blast, or null.</summary>
    public BattleArea Area { get; init; }

    /// <summary>How long it lasts or takes: a telegraph's wind-up, an aura's duration, a clip's length.</summary>
    public int DurationMs { get; init; }

    /// <summary>When its blow lands (ActionStarted), or when a projectile arrives (ProjectileLaunched), in fight time.</summary>
    public int ImpactMs { get; init; }

    /// <summary>The aura it's about, by aura id, or null.</summary>
    public string AuraId { get; init; }

    /// <summary>The line for the combat log.</summary>
    public string Text { get; init; }

    /// <summary>A compact, stable description of the event for the determinism hash: everything but the log line.</summary>
    public string Signature() => FormattableString.Invariant(
        $"{(int)Kind}|{AtMs}|{ActorId}|{TargetId}|{SkillId}|{Motion}|{Amount}|{(int)Flags}|{(int)DamageType}|{Position.X:F2},{Position.Y:F2}|{DurationMs}|{ImpactMs}|{AuraId}");
}

/// <summary>What a BattleEvent is about.</summary>
/// <remarks>Contract §3's kinds. New ones go on the end.</remarks>
public enum BattleEventKind
{
    /// <summary>A fighter starts a move: its clip begins (Motion), and ImpactMs says when its first blow lands.</summary>
    ActionStarted,

    /// <summary>A foe winds up a big move: its area (a ground marker) and how long until it lands.</summary>
    Telegraph,

    /// <summary>A cast with a cast time begins: its bar fills for DurationMs.</summary>
    CastStarted,

    /// <summary>A cast or a telegraph was stopped before it finished.</summary>
    CastInterrupted,

    /// <summary>Something thrown, shot or cast leaves the hand, from Position, arriving at ImpactMs.</summary>
    ProjectileLaunched,

    /// <summary>An attack hits (Flags say if it's a critical).</summary>
    Hit,

    /// <summary>An attack misses outright.</summary>
    Missed,

    /// <summary>An attack misses because of the target's quickness.</summary>
    Dodged,

    /// <summary>An attack misses because of the target's shield.</summary>
    Blocked,

    /// <summary>Hit points lost: Amount of DamageType.</summary>
    Damaged,

    /// <summary>Hit points gained.</summary>
    Healed,

    /// <summary>Damage taken by temporary hit points instead.</summary>
    Absorbed,

    /// <summary>An aura goes on.</summary>
    AuraApplied,

    /// <summary>An aura already on is put on again: its time starts over, or it gains a stack.</summary>
    AuraRefreshed,

    /// <summary>An aura comes off: it ran out, was cleansed, or was used up.</summary>
    AuraRemoved,

    /// <summary>An aura's tick: damage or healing over time, or a save to end it.</summary>
    AuraTicked,

    /// <summary>Someone walked: from where they were to Position.</summary>
    Moved,

    /// <summary>Someone was knocked back to Position.</summary>
    KnockedBack,

    /// <summary>Someone was pulled to Position.</summary>
    Pulled,

    /// <summary>Someone was knocked flat (Prone).</summary>
    KnockedDown,

    /// <summary>Someone got up off the ground.</summary>
    GotUp,

    /// <summary>A party member at 0 hit points is on the ground.</summary>
    Downed,

    /// <summary>A Downed party member's death save: Amount is the roll; Flags.Save if it succeeded.</summary>
    DeathSave,

    /// <summary>A Downed party member is back up.</summary>
    Revived,

    /// <summary>Someone died (a foe) or fell for the rest of the fight (a party member who failed three saves).</summary>
    Died,

    /// <summary>A guest at 0 hit points, or a foe who yields, leaves the fight unhurt for the story.</summary>
    FellBack,

    /// <summary>Someone joins the fight: a wave, a summon, a guest.</summary>
    Joined,

    /// <summary>A support behind the fight acts.</summary>
    SupportActed,

    /// <summary>A boss enters a new phase: Amount is its number, Text its name.</summary>
    PhaseChanged,

    /// <summary>Someone shouts something: Text.</summary>
    Bark,

    /// <summary>The Soul-Link gains a charge: Amount is how many it has.</summary>
    SoulLinkCharged,

    /// <summary>The duo's Soul-Link Strike.</summary>
    SoulLinkStrike,

    /// <summary>A foe turns on someone new (its threat table's top changed).</summary>
    ThreatTargetChanged,

    /// <summary>The fight is won.</summary>
    FightWon,

    /// <summary>The fight is lost (or conceded).</summary>
    FightLost,

    /// <summary>A skill was used: its cooldown, the GCD, its Focus cost (Amount). The screen's bar sweeps start here.</summary>
    SkillUsed,

    /// <summary>A reaction was armed (Riposte, Shield, Cover...) and waits for its trigger for DurationMs.</summary>
    Armed,

    /// <summary>An armed reaction fired.</summary>
    ReactionFired,

    /// <summary>Focus gained or spent outside a skill's cost: Amount, signed.</summary>
    FocusChanged,

    /// <summary>Someone was taunted: they must attack ActorId for DurationMs.</summary>
    Taunted,

    /// <summary>A command couldn't be done: Text says why, for the bar's red flash.</summary>
    Denied,

    /// <summary>Someone took on a form (Wild Shape), or came out of it.</summary>
    Transformed,

    /// <summary>Someone starts helping a Downed ally up: a channel of DurationMs.</summary>
    HelpStarted,

    /// <summary>A potion was drunk.</summary>
    PotionDrunk,

    /// <summary>Someone vanished from sight (a smoke bomb) for DurationMs.</summary>
    Vanished,

    /// <summary>Gold or a potion lifted from a foe (Pick Pocket): Amount, Text says what.</summary>
    Stole,
}

/// <summary>What sort of blow an event was.</summary>
[Flags]
public enum HitFlags
{
    /// <summary>Nothing special.</summary>
    None = 0,

    /// <summary>A critical: gold and bigger.</summary>
    Crit = 1,

    /// <summary>Dodged.</summary>
    Dodge = 2,

    /// <summary>Blocked.</summary>
    Block = 4,

    /// <summary>Missed.</summary>
    Miss = 8,

    /// <summary>A save was made (half damage, or nothing).</summary>
    Save = 16,

    /// <summary>A tick of something over time: smaller and tinted.</summary>
    Tick = 32,

    /// <summary>It killed or dropped them.</summary>
    Kill = 64,

    /// <summary>A big blow: an ultimate, a boss's heavy skill, a crit over a quarter of their health. The screen shakes.</summary>
    Big = 128,

    /// <summary>An auto attack's blow.</summary>
    Auto = 256,

    /// <summary>Area damage.</summary>
    Area = 512,
}

/// <summary>How a fight stands.</summary>
public enum FightOutcome
{
    /// <summary>Still going.</summary>
    Ongoing,

    /// <summary>Won.</summary>
    Won,

    /// <summary>Lost: the whole party is down.</summary>
    Lost,

    /// <summary>Given up.</summary>
    Conceded,
}

/// <summary>Who a fighter is to the fight.</summary>
public enum FighterKind
{
    /// <summary>A party member.</summary>
    Member,

    /// <summary>Someone fighting on the party's side without being in it.</summary>
    Guest,

    /// <summary>A foe.</summary>
    Foe,

    /// <summary>Someone a skill called in (Whistle for the Watch, a boss's whelps).</summary>
    Summon,
}
