using System;
using System.Collections.Generic;
using LegendQuest.Combat;
using LegendQuest.Rules;

namespace LegendQuest.Characters;

/// <summary>
/// Levelling up: experience going to someone, the levels it takes them up, and setting a level
/// outright.
/// </summary>
/// <remarks>
/// LevelTable has the numbers. A level's new hit points come with it, the way 5e's do: the most they
/// can have goes up, and so do the hit points they have by as much, so a hurt member stays as hurt.
/// Plain rules with nothing drawn: the screens show the lines it hands back.
/// </remarks>
public static class Leveling
{
    /// <summary>Gives someone experience, and takes them up as many levels as it reaches, never past LevelTable.MaxLevel.</summary>
    /// <remarks>At the highest level it still counts up, with nowhere to go.</remarks>
    /// <param name="member">Who.</param>
    /// <param name="xp">How much. Nothing happens for 0 or less.</param>
    /// <returns>A line for each level they've gone up, in order, like "Mira reaches level 2.": none if they haven't.</returns>
    public static IReadOnlyList<string> Award(PartyMember member, int xp)
    {
        var lines = new List<string>();
        if (xp <= 0) return lines;

        member.Xp += xp;
        while (member.Level < LevelTable.MaxLevel && member.Xp >= LevelTable.ToNext(member.Level))
        {
            member.Xp -= LevelTable.ToNext(member.Level);
            Raise(member, member.Level + 1);
            lines.Add($"{member.Name} reaches level {member.Level}.");
        }
        return lines;
    }

    /// <summary>Gives everyone the experience a won fight is worth to them (LevelTable.FightXp, by their own level), and takes them up the levels it reaches.</summary>
    /// <remarks>The screen gives it to the whole party and whoever's waiting at camp, sat out or not, so nobody falls behind the levels the story's fights are tuned for.</remarks>
    /// <param name="encounter">The fight that was won.</param>
    /// <param name="members">Who it goes to.</param>
    /// <returns>A line for each level anyone's gone up, in order.</returns>
    public static IReadOnlyList<string> AwardFight(EncounterDefinition encounter, IEnumerable<PartyMember> members)
    {
        var lines = new List<string>();
        foreach (PartyMember member in members) lines.AddRange(Award(member, LevelTable.FightXp(encounter, member.Level)));
        return lines;
    }

    /// <summary>Puts someone at a level outright, with no experience towards the next: a companion joining at the party's level, an old save brought up to date, the simulator's party.</summary>
    /// <remarks>Their hit points go up or down by as much as their most does, and stay inside it.</remarks>
    /// <param name="member">Who.</param>
    /// <param name="level">The level, kept from 1 to LevelTable.MaxLevel.</param>
    public static void SetLevel(PartyMember member, int level)
    {
        Raise(member, Math.Clamp(level, 1, LevelTable.MaxLevel));
        member.Xp = 0;
    }

    /// <summary>Changes someone's level, and their hit points by as much as their most changes.</summary>
    private static void Raise(PartyMember member, int level)
    {
        int before = member.MaxHp;
        member.Level = level;
        member.Hp = Math.Clamp(member.Hp + member.MaxHp - before, 0, member.MaxHp);
    }
}
