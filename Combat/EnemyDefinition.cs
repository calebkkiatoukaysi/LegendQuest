using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// A foe's stat block: how it looks, how hard it is to hit and to kill, how fast it is, and what it
/// does.
/// </summary>
/// <remarks>
/// The SRD's monster stat block, cut to what the engine uses. Every one of a kind has its hit dice's
/// average, the way the SRD prints them, so a fight plays the same way twice. It looks like one of
/// the Creatures, or like one of the Cast.
/// </remarks>
public sealed class EnemyDefinition
{
    /// <summary>What encounters call it.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen and in the log.</summary>
    public required string Name { get; init; }

    /// <summary>Which of the Creatures it's drawn as, or null if it's one of the Cast.</summary>
    public string Creature { get; init; }

    /// <summary>Which of the Cast it's drawn as, or null if it's a creature.</summary>
    public string Cast { get; init; }

    /// <summary>Its armour class.</summary>
    public required int ArmourClass { get; init; }

    /// <summary>Its hit dice, Constitution included ("2d8+2"). It has their average.</summary>
    public required DiceExpression HitDice { get; init; }

    /// <summary>How far it moves in a turn, in metres.</summary>
    public required float Speed { get; init; }

    /// <summary>Its six scores: Dexterity for initiative, and every one for its saves.</summary>
    public required AbilityScores Scores { get; init; }

    /// <summary>Its attacks. The first is its main one; with Multiattack the Attack action makes them all.</summary>
    public required IReadOnlyList<EnemyAttack> Attacks { get; init; }

    /// <summary>What it can do beyond the actions everyone has, and what it always does (Pack Tactics).</summary>
    public IReadOnlyList<AbilityId> Abilities { get; init; } = Array.Empty<AbilityId>();

    /// <summary>The XP it's worth, the SRD's for its challenge.</summary>
    public int Xp { get; init; }

    /// <summary>The loot table its body's searched with, by LootTableCatalog id, or null for nothing.</summary>
    public string LootTable { get; init; }

    /// <summary>Whether it fights from range, so the AI keeps it away from the front.</summary>
    public bool Ranged { get; init; }

    /// <summary>Whether it's big enough to stand across two slots side by side (the Thornmother). Nothing can pull or push it.</summary>
    public bool Big { get; init; }

    /// <summary>The Focus each of its hits takes from whoever it hits, as well as the damage, or 0.</summary>
    public int FocusDrain { get; init; }
}
