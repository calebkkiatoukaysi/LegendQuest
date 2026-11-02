using System;
using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Combat;

/// <summary>
/// Something an ability spends that only a rest gives back: a pool of spell slots, a feature's
/// uses, a paladin's healing.
/// </summary>
/// <remarks>
/// How much someone has comes from their class and level, so it's worked out each time rather than
/// saved; only what's been spent goes in the save (PartyMember.Spent, by this id).
/// </remarks>
public sealed class ResourceDefinition
{
    /// <summary>What it's called in PartyMember.Spent and on an AbilityDefinition.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>The rest that gives it all back.</summary>
    public required RestKind Recharge { get; init; }

    /// <summary>The classes that have it at all.</summary>
    public required IReadOnlyList<ClassId> Classes { get; init; }

    /// <summary>How many a class that has it gets, before PerLevel. Unused when CountFrom is set.</summary>
    public int Count { get; init; }

    /// <summary>Whether Count is per level (Lay on Hands is five hit points a level).</summary>
    public bool PerLevel { get; init; }

    /// <summary>The ability whose modifier is the count instead, never less than one (Bardic Inspiration is the Charisma modifier), or null.</summary>
    public Ability? CountFrom { get; init; }

    /// <summary>The most of it someone can have, spent or not.</summary>
    /// <param name="member">Who.</param>
    public int Max(PartyMember member)
    {
        if (!Contains(Classes, member.Class)) return 0;
        if (CountFrom is Ability ability) return Math.Max(1, member.Scores.Modifier(ability));
        return PerLevel ? Count * member.Level : Count;
    }

    /// <summary>Whether a class is in the list, without LINQ.</summary>
    private static bool Contains(IReadOnlyList<ClassId> classes, ClassId id)
    {
        foreach (ClassId each in classes)
            if (each == id) return true;
        return false;
    }
}
