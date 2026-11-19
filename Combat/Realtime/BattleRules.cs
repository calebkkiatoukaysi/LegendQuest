using System;
using LegendQuest.Items;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// What a real-time fight is played by beyond the encounter: the difficulty, the story's flags, who
/// the player drives, the bag the potions come from, and where it's fought.
/// </summary>
/// <param name="Difficulty">How hard it is: foes' hit points, rolls and telegraphs, the party's starting Focus (DifficultyRules).</param>
/// <param name="Flag">Whether a story flag is set (allies who only come on one; soul-bond-broken), or null for none set.</param>
/// <param name="LeaderId">The fighter the player drives, by id, or null for the AI to drive everyone (the simulator).</param>
/// <param name="Bag">The party's bag, for potions, or null for none.</param>
public sealed record BattleRules(Difficulty Difficulty, Func<string, bool> Flag = null, string LeaderId = null, Inventory Bag = null)
{
    /// <summary>The flag that turns the Soul-Link off.</summary>
    public const string SoulBondBroken = "soul-bond-broken";

    /// <summary>Whether the party's AI plays as a person would: 0.4 s late, and its best choice 85% of the time (11-combat.md §9.2). The simulator turns it on.</summary>
    public bool HumanHandicap { get; init; }

    /// <summary>Whether it's fought somewhere dark (underground, or at night), for the skills that care.</summary>
    public bool Dark { get; init; }

    /// <summary>Whether it's fought outdoors, for the skills that care (Broadside).</summary>
    public bool Outdoors { get; init; } = true;

    /// <summary>Whether a story flag is set.</summary>
    /// <param name="flag">The flag.</param>
    public bool Has(string flag) => Flag is not null && Flag(flag);
}
