using System.Collections.Generic;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// The numbers of each Focus model (11-combat.md §4.1): how much a second, a hit, a crit, a dodge, a
/// block, being hit, a heal and a tick of something over time are worth to it.
/// </summary>
/// <remarks>
/// Everyone also gets BaseRegen a second on top. A hit is one that lands (an attack that hits, or a
/// save failed against a skill's damage); a crit pays PerCrit instead of PerHit. Pact pays PerCursedHit
/// for a hit on a target carrying the user's Hexed (or any curse of theirs), PerHit otherwise.
/// </remarks>
public static class FocusModels
{
    /// <summary>The Focus everyone gets a second in a fight, before their model's.</summary>
    public const int BaseRegen = 1;

    private static readonly Dictionary<FocusModel, FocusRules> Table = new()
    {
        [FocusModel.Momentum] = new FocusRules { PerHit = 8, PerCrit = 14 },
        [FocusModel.Opportunist] = new FocusRules { PerHit = 5, PerCrit = 16, PerDodge = 12 },
        [FocusModel.Bulwark] = new FocusRules { WhenHit = 6, PerBlock = 10, PerHit = 4, PerCrit = 4 },
        [FocusModel.Wellspring] = new FocusRules { PerSecond = 5 },
        [FocusModel.Devotion] = new FocusRules { PerSecond = 3, PerHeal = 4 },
        [FocusModel.Wild] = new FocusRules { PerSecond = 3, PerTick = 2 },
        [FocusModel.Pact] = new FocusRules { PerSecond = 1, PerHit = 2, PerCrit = 2, PerCursedHit = 6 },
        [FocusModel.Tempo] = new FocusRules { PerHit = 6, PerCrit = 6 },
        [FocusModel.Blood] = new FocusRules { PerSecond = 4 },
    };

    /// <summary>A model's numbers.</summary>
    /// <param name="model">The model.</param>
    public static FocusRules Get(FocusModel model) => Table[model];
}

/// <summary>One Focus model's numbers.</summary>
public sealed record FocusRules
{
    /// <summary>A second, on top of the base 1.</summary>
    public int PerSecond { get; init; }

    /// <summary>For each of your hits.</summary>
    public int PerHit { get; init; }

    /// <summary>For each of your crits, instead of PerHit.</summary>
    public int PerCrit { get; init; }

    /// <summary>When you dodge.</summary>
    public int PerDodge { get; init; }

    /// <summary>When you're hit.</summary>
    public int WhenHit { get; init; }

    /// <summary>When you block.</summary>
    public int PerBlock { get; init; }

    /// <summary>For every heal you land.</summary>
    public int PerHeal { get; init; }

    /// <summary>For every tick of your damage or healing over time.</summary>
    public int PerTick { get; init; }

    /// <summary>For each hit on a target carrying your curse (Pact).</summary>
    public int PerCursedHit { get; init; }
}
