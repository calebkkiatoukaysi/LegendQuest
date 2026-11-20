using System;
using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Combat;

/// <summary>
/// Class ranks: the class points (CP) each rank takes, what a fight's worth, the rank each companion
/// joins at, and what a rank unlocks.
/// </summary>
/// <remarks>
/// 10-classes.md §1.3 and §6, 11-combat.md §4.5. The equipped class earns CP from every fight it's
/// equipped for; classes not equipped earn nothing. Each person keeps their own points per class
/// (PartyMember.ClassPoints), and someone who has never had points written for their own class is at
/// the rank they joined at, so a companion out of Origins needs nothing set.
/// </remarks>
public static class ClassRanks
{
    /// <summary>The highest rank.</summary>
    public const int MaxRank = 10;

    /// <summary>What a random fight's worth.</summary>
    public const int RandomFight = 15;

    /// <summary>What a story fight's worth.</summary>
    public const int StoryFight = 35;

    /// <summary>What a boss is worth: a mini-boss, a boss or the Sovereign.</summary>
    public const int BossFight = 80;

    /// <summary>What a class trial is worth.</summary>
    public const int Trial = 60;

    /// <summary>The rank a skill slot opens at, by slot: the auto and skill 1 at 1, skill 2 at 2, skill 3 at 3, the ultimate at 5.</summary>
    public const int Skill2Rank = 2, Skill3Rank = 3, PassiveRank = 4, UltimateRank = 5, ToHitRank = 6, FocusRank = 7, DcRank = 8, PowerRank = 9, LastPassiveRank = 10;

    /// <summary>The CP total each rank starts at, rank 1 first (10-classes.md §1.3).</summary>
    private static readonly int[] RankStarts = { 0, 40, 120, 240, 400, 600, 850, 1150, 1500, 2000 };

    /// <summary>The rank each companion joins at in their own class (10-classes.md §6); you and the Soul-Bound start at 1.</summary>
    private static readonly Dictionary<string, int> Joining = new(StringComparer.Ordinal)
    {
        ["sera"] = 3,
        ["mira"] = 3,
        ["kael"] = 3,
        ["brannock"] = 4,
        ["valencia"] = 4,
        ["elara"] = 5,
    };

    /// <summary>The rank a CP total makes.</summary>
    /// <param name="points">The class points.</param>
    public static int RankOf(int points)
    {
        int rank = 1;
        for (int i = 1; i < RankStarts.Length; i++)
            if (points >= RankStarts[i]) rank = i + 1;
        return rank;
    }

    /// <summary>The CP total a rank starts at.</summary>
    /// <param name="rank">The rank, 1 to 10.</param>
    public static int PointsFor(int rank) => RankStarts[Math.Clamp(rank, 1, MaxRank) - 1];

    /// <summary>The CP a fight of a kind is worth.</summary>
    /// <param name="kind">The kind of fight.</param>
    public static int Award(EncounterKind kind) => kind switch
    {
        EncounterKind.Random => RandomFight,
        EncounterKind.MiniBoss or EncounterKind.Boss or EncounterKind.Sovereign => BossFight,
        _ => StoryFight,
    };

    /// <summary>The rank someone joins the party at in the class they join with.</summary>
    /// <param name="originId">Their origin id.</param>
    public static int JoiningRank(string originId) => originId is not null && Joining.TryGetValue(originId, out int rank) ? rank : 1;

    /// <summary>Adds a fight's CP to the equipped class of everyone who fought it.</summary>
    /// <param name="kind">The kind of fight that was won.</param>
    /// <param name="members">Who fought it.</param>
    /// <returns>A line for each rank gained ("SERA: Wildcaller rank 4").</returns>
    public static IReadOnlyList<string> AwardFight(EncounterKind kind, IEnumerable<PartyMember> members)
    {
        var lines = new List<string>();
        foreach (PartyMember member in members)
        {
            int before = member.Rank;
            member.AddClassPoints(Award(kind));
            if (member.Rank > before) lines.Add($"{member.Name.ToUpperInvariant()}: {Rules.ClassCatalog.Get(member.Class).Name} rank {member.Rank}");
        }
        return lines;
    }

    /// <summary>The rank the story would have a class at by an encounter, for the simulator: the CP of every fight before it in the story's order (and a random fight beside each), on top of the rank they joined at.</summary>
    /// <param name="encounter">The fight.</param>
    /// <param name="originId">Whose class.</param>
    public static int ExpectedRank(EncounterDefinition encounter, string originId)
    {
        int points = PointsFor(JoiningRank(originId));
        foreach (EncounterDefinition before in EncounterCatalog.All)
        {
            if (before.Id == encounter.Id) break;
            points += Award(before.Kind) + RandomFight;
        }
        return RankOf(points);
    }

    /// <summary>The CP the story would have given someone's own class by a game's flags, for a save from before ranks: the rank they joined at, and for every story fight won (its Victory flag up) what it's worth and a random fight beside it, as ExpectedRank counts them.</summary>
    /// <param name="originId">Whose class.</param>
    /// <param name="flags">The game's flags.</param>
    public static int PointsFromStory(string originId, ISet<string> flags)
    {
        int points = PointsFor(JoiningRank(originId));
        foreach (EncounterDefinition fight in EncounterCatalog.All)
            if (fight.Victory is string won && flags.Contains(won)) points += Award(fight.Kind) + RandomFight;
        return points;
    }
}
