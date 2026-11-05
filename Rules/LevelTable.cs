using System;
using System.Text.RegularExpressions;
using LegendQuest.Combat;

namespace LegendQuest.Rules;

/// <summary>
/// The levels: the one the story has the party at in each chapter, the experience to the next, what
/// a fight's worth, and the proficiency bonus that comes with a level.
/// </summary>
/// <remarks>
/// 11-combat.md §6.3. Levels run from 1 to MaxLevel by the end of Act 3. Experience counts up to the
/// next level and starts again from what's left over, so a member's Xp is always how far they are
/// towards it. A story fight is worth about a third of a level at the level it's tuned for and a
/// random one a tenth, less for a party that's ahead of the story and more for one behind it, so
/// wandering about doesn't break the story's fights and skipping random ones doesn't either.
/// Leveling hands the experience out.
/// </remarks>
public static class LevelTable
{
    /// <summary>The highest level there is: Act 3's last chapter.</summary>
    public const int MaxLevel = 12;

    /// <summary>The experience to the next level, a level: ToNext is this times the level.</summary>
    public const int XpPerLevel = 300;

    /// <summary>What a fight the story brings is worth a level it's tuned for: a third of a level's experience.</summary>
    public const int StoryFightXp = 100;

    /// <summary>What a random fight is worth a level it's tuned for: a tenth of a level's experience.</summary>
    public const int RandomFightXp = 30;

    /// <summary>What a fight's worth changes by, in quarters, for each level a member is under (more) or over (less) the level it's tuned for.</summary>
    private const int QuartersPerLevel = 1;

    /// <summary>A fight id's act and chapter: p- for the Prologue, aN-cM- for act N chapter M.</summary>
    private static readonly Regex Chapter = new(@"^(?:(?<prologue>p)-|a(?<act>[1-3])-c(?<chapter>\d+)-)", RegexOptions.CultureInvariant);

    /// <summary>The level the story has the party at in each chapter, by act from 1 and chapter from 1: §6.3's table, a range taken at its lower end.</summary>
    private static readonly int[][] ByChapter =
    {
        new[] { 1, 2, 2, 3, 4, 5, 5, 6 },
        new[] { 6, 7, 7, 8, 8, 9 },
        new[] { 9, 10, 10, 11, 12 },
    };

    /// <summary>The level the Prologue's at.</summary>
    private const int PrologueLevel = 1;

    /// <summary>The level the story has the party at in a chapter.</summary>
    /// <param name="act">The act, 1 to 3; 0 for the Prologue.</param>
    /// <param name="chapter">The chapter in it, from 1; ignored for the Prologue.</param>
    /// <returns>The level, or 0 if the story hasn't that chapter.</returns>
    public static int ForChapter(int act, int chapter)
    {
        if (act == 0) return PrologueLevel;
        if (act < 1 || act > ByChapter.Length || chapter < 1 || chapter > ByChapter[act - 1].Length) return 0;
        return ByChapter[act - 1][chapter - 1];
    }

    /// <summary>The level the party's tuned to be at for a fight: its ExpectedLevel if it has one, or else the level for the act and chapter its id starts with.</summary>
    /// <param name="encounter">The fight.</param>
    /// <exception cref="InvalidOperationException">Its id doesn't start with a chapter the table has, and it has no ExpectedLevel.</exception>
    public static int Expected(EncounterDefinition encounter)
    {
        if (encounter.ExpectedLevel > 0) return encounter.ExpectedLevel;
        return FromId(encounter.Id)
            ?? throw new InvalidOperationException($"Encounter {encounter.Id}'s id doesn't start with a chapter the level table has (p- or aN-cM-), so it needs an ExpectedLevel.");
    }

    /// <summary>The level for the act and chapter an id starts with, or null if it doesn't start with one the table has.</summary>
    /// <param name="id">A fight's id.</param>
    public static int? FromId(string id)
    {
        Match match = Chapter.Match(id ?? string.Empty);
        if (!match.Success) return null;
        if (match.Groups["prologue"].Success) return PrologueLevel;

        int level = ForChapter(int.Parse(match.Groups["act"].Value), int.Parse(match.Groups["chapter"].Value));
        return level > 0 ? level : null;
    }

    /// <summary>The experience from one level to the next.</summary>
    /// <param name="level">The level they're at.</param>
    public static int ToNext(int level) => XpPerLevel * Math.Max(1, level);

    /// <summary>What a fight's worth to one member: StoryFightXp (RandomFightXp for a random one) times the level it's tuned for, a quarter less for each level they're over it (never below nothing), a quarter more for each they're under.</summary>
    /// <param name="encounter">The fight that was won.</param>
    /// <param name="memberLevel">The level they were at when it was.</param>
    public static int FightXp(EncounterDefinition encounter, int memberLevel)
    {
        int expected = Expected(encounter);
        int each = encounter.Kind == EncounterKind.Random ? RandomFightXp : StoryFightXp;
        int quarters = Math.Max(0, 4 + QuartersPerLevel * (expected - memberLevel));
        return each * expected * quarters / 4;
    }

    /// <summary>What proficiency adds to an attack, a save DC or a check at a level: 2 at levels 1 to 4, and one more each four levels after.</summary>
    /// <param name="level">The level.</param>
    public static int Proficiency(int level) => 2 + (Math.Clamp(level, 1, MaxLevel) - 1) / 4;
}
