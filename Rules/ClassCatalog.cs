using System;
using System.Collections.Generic;

namespace LegendQuest.Rules;

/// <summary>
/// Every class, by ClassId.
/// </summary>
/// <remarks>
/// A static table keyed by the enum, like The Black Box's ItemCatalog. The hit dice are 5e's, from
/// the SRD 5.1: a d10 for a fighter, a d8 for a rogue or a cleric, a d6 for a wizard.
/// </remarks>
public static class ClassCatalog
{
    private static readonly ClassDefinition[] Table =
    {
        new() { Id = ClassId.Fighter, Name = "Fighter", HitDie = 10 },
        new() { Id = ClassId.Rogue, Name = "Rogue", HitDie = 8 },
        new() { Id = ClassId.Cleric, Name = "Cleric", HitDie = 8 },
        new() { Id = ClassId.Wizard, Name = "Wizard", HitDie = 6 },
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
