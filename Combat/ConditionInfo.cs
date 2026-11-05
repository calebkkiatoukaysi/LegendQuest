using System;
using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// What each condition's called: its name for the icon's tooltip, and the word the log uses for
/// someone who has it.
/// </summary>
/// <remarks>A table so a condition added to the enum and not here shows up when the game starts, not as a blank in the log.</remarks>
public static class ConditionInfo
{
    private static readonly Dictionary<ConditionId, (string Name, string Word)> Table = new()
    {
        [ConditionId.Downed] = ("Downed", "down"),
        [ConditionId.Prone] = ("Prone", "prone"),
        [ConditionId.Slowed] = ("Slowed", "slowed"),
        [ConditionId.Disengaged] = ("Disengaged", "disengaged"),
        [ConditionId.Dodging] = ("Dodging", "dodging"),
        [ConditionId.Restrained] = ("Restrained", "restrained"),
        [ConditionId.Hexed] = ("Hexed", "hexed"),
        [ConditionId.Inspired] = ("Inspired", "inspired"),
        [ConditionId.Stable] = ("Stable", "stable"),
        [ConditionId.Helped] = ("Helped", "helped"),
        [ConditionId.Mocked] = ("Rattled", "rattled"),
        [ConditionId.Marked] = ("Marked", "marked"),
        [ConditionId.Taunted] = ("Taunted", "taunted"),
        [ConditionId.Burning] = ("Burning", "burning"),
        [ConditionId.Bleeding] = ("Bleeding", "bleeding"),
        [ConditionId.Frozen] = ("Frozen", "frozen in time"),
        [ConditionId.Poisoned] = ("Poisoned", "poisoned"),
        [ConditionId.Stunned] = ("Stunned", "stunned"),
        [ConditionId.Blinded] = ("Blinded", "blinded"),
        [ConditionId.Frightened] = ("Frightened", "frightened"),
        [ConditionId.Charmed] = ("Charmed", "charmed"),
    };

    /// <summary>Checks every condition has its words.</summary>
    /// <remarks>Throws on purpose.</remarks>
    static ConditionInfo()
    {
        foreach (ConditionId id in Enum.GetValues<ConditionId>())
            if (!Table.ContainsKey(id)) throw new InvalidOperationException($"Condition {id} has no name in ConditionInfo.");
    }

    /// <summary>The condition's name, for its icon's tooltip.</summary>
    /// <param name="id">Which.</param>
    public static string Name(ConditionId id) => Table[id].Name;

    /// <summary>The word for someone with it, for the log: "is restrained".</summary>
    /// <param name="id">Which.</param>
    public static string Word(ConditionId id) => Table[id].Word;
}
