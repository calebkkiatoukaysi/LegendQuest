using System;
using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// One of a boss's phases after its first: the share of its hit points it begins at, and what
/// changes (11-combat.md §5.2).
/// </summary>
/// <remarks>
/// A phase change interrupts whatever the boss was doing, puts the phase's name across the screen,
/// and can bark, add and take away moves, make it faster, call foes in, and change how often its
/// moves come round.
/// </remarks>
/// <param name="AtShare">The share of its hit points it begins below: 0.66 for the Thornmother's second.</param>
/// <param name="Name">What the screen calls it ("Thorns", "Enraged").</param>
public sealed record FoePhase(double AtShare, string Name)
{
    /// <summary>What it shouts as it begins, or null.</summary>
    public string Bark { get; init; }

    /// <summary>Moves it gains, by SkillCatalog id.</summary>
    public IReadOnlyList<string> Adds { get; init; } = Array.Empty<string>();

    /// <summary>Moves it loses, by SkillCatalog id.</summary>
    public IReadOnlyList<string> Removes { get; init; } = Array.Empty<string>();

    /// <summary>Haste it gains for the rest of the fight, in percent: the Thornmother's enrage is 25.</summary>
    public int HastePct { get; init; }

    /// <summary>Who it calls in, by EnemyCatalog id, or null.</summary>
    public string Summons { get; init; }

    /// <summary>How many it calls in.</summary>
    public int SummonCount { get; init; }

    /// <summary>Moves whose cooldown changes, by SkillCatalog id, with the new cooldown in milliseconds (Thorn Burst every 10 s).</summary>
    public IReadOnlyDictionary<string, int> Cooldowns { get; init; } = new Dictionary<string, int>();
}
