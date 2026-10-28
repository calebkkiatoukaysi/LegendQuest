using System;
using System.Collections.Generic;

namespace LegendQuest.Rules;

/// <summary>
/// Every class, by ClassId.
/// </summary>
/// <remarks>
/// A static table keyed by the enum, like The Black Box's ItemCatalog. The hit dice are 5e's, from
/// the SRD 5.1: a d10 for a fighter or a paladin, a d8 for a rogue, a cleric, a druid, a warlock or
/// a bard, a d6 for a wizard. The names are the story's own.
/// </remarks>
public static class ClassCatalog
{
    private static readonly ClassDefinition[] Table =
    {
        new()
        {
            Id = ClassId.Fighter, Name = "Blademaster", Role = "Frontline weapon specialist.", HitDie = 10,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 13, Constitution = 14, Intelligence = 8, Wisdom = 12, Charisma = 10 },
        },
        new()
        {
            Id = ClassId.Rogue, Name = "Shadowstep", Role = "Stealth, crits and lockpicking.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 15, Constitution = 14, Intelligence = 12, Wisdom = 10, Charisma = 13 },
        },
        new()
        {
            Id = ClassId.Cleric, Name = "Lightbearer", Role = "Support and holy damage.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 13, Dexterity = 10, Constitution = 14, Intelligence = 8, Wisdom = 15, Charisma = 12 },
        },
        new()
        {
            Id = ClassId.Wizard, Name = "Arcanist", Role = "Studied spellcaster, burst damage.", HitDie = 6,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 15, Wisdom = 12, Charisma = 10 },
        },
        new()
        {
            Id = ClassId.Paladin, Name = "Warden", Role = "Oath-bound tank and healer.", HitDie = 10,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 10, Constitution = 13, Intelligence = 8, Wisdom = 12, Charisma = 14 },
        },
        new()
        {
            Id = ClassId.Druid, Name = "Wildcaller", Role = "Nature magic, a beast companion.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 10, Dexterity = 13, Constitution = 14, Intelligence = 12, Wisdom = 15, Charisma = 8 },
        },
        new()
        {
            Id = ClassId.Warlock, Name = "Hexblade", Role = "Pact magic that drains life.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 10, Wisdom = 12, Charisma = 15 },
        },
        new()
        {
            Id = ClassId.Bard, Name = "Skald", Role = "Buffs, debuffs and persuasion.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 14, Constitution = 13, Intelligence = 10, Wisdom = 12, Charisma = 15 },
        },
    };

    private static readonly Dictionary<ClassId, ClassDefinition> ById = new();

    /// <summary>Indexes the table, and checks every class has exactly one row.</summary>
    /// <remarks>Throws on purpose, so a class added to the enum and not here shows up when the game starts.</remarks>
    static ClassCatalog()
    {
        foreach (ClassDefinition row in Table)
        {
            if (row.HitDie < 1) throw new InvalidOperationException($"Class {row.Id} has a d{row.HitDie} for a hit die.");
            if (!ById.TryAdd(row.Id, row)) throw new InvalidOperationException($"Class {row.Id} is in the table twice.");
        }

        foreach (ClassId id in Enum.GetValues<ClassId>())
            if (!ById.ContainsKey(id)) throw new InvalidOperationException($"Class {id} isn't in ClassCatalog.");
    }

    /// <summary>One class.</summary>
    /// <param name="id">Which.</param>
    public static ClassDefinition Get(ClassId id) => ById[id];
}
