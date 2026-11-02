using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Fights an encounter headless, hundreds of times, with EnemyMind playing both sides, and prints how
/// it went: how often the party wins, how long it takes, and what it costs each of them.
/// </summary>
/// <remarks>
/// The Black Box's RoundSimulator, for --simulate. No window and no game: just the catalogs, the
/// engine and a seeded Random, so two runs print the same table. The party is the default level-1
/// one, you as a Blademaster and the Soul-Bound as a Shadowstep with the story's companions who'd be
/// there, each in their kit at full health, with the two potions the party starts with. They fight on
/// open ground, a move apart, the way most of the story's fights start; a wave turns up on its own tile
/// pulled in to the edge of the ground. Every run is a fresh party, so a fight that follows another
/// (the guard wave after the Thornmother) is tested on its own.
/// </remarks>
public static class CombatSimulator
{
    /// <summary>How many runs --simulate does when it isn't told.</summary>
    public const int DefaultRuns = 1000;

    /// <summary>What --simulate takes for every encounter in turn.</summary>
    public const string Everything = "all";

    /// <summary>The tutorial's target: nobody should lose it.</summary>
    public const double TutorialWins = 0.98;

    /// <summary>A story fight's target: usually won, sometimes with someone down, now and then lost.</summary>
    public const double StoryWinsLeast = 0.80, StoryWinsMost = 0.92;

    /// <summary>A boss's target: a real chance of losing.</summary>
    public const double BossWinsLeast = 0.65, BossWinsMost = 0.80;

    /// <summary>A fixed seed, so two runs of the simulator print the same table.</summary>
    private const int Seed = 12345;

    /// <summary>The rounds after which a fight still going counts as unfinished, not won. Nothing in the story should get near it.</summary>
    private const int RoundCap = 30;

    /// <summary>The open ground's size in tiles: bigger than any fight needs.</summary>
    private const int GroundWidth = 24, GroundHeight = 16;

    /// <summary>The column the party lines up in.</summary>
    private const int PartyColumn = 4;

    /// <summary>How many tiles apart the two sides start: one move, about where the story's fights start once their conversations end.</summary>
    private const int StartGap = 6;

    /// <summary>The healing potions in the bag at the start of every run, the ones a new party sets out with.</summary>
    private const int StartingPotions = 2;

    /// <summary>The story's companions in the party by each fight, beside you and the Soul-Bound: Sera from the Mirewood on, and Mira from Thornbridge's end (a guest in the raid itself). Whoever an encounter sits out (Sera at the crystal) is left out of it.</summary>
    private static readonly Dictionary<string, string[]> Companions = new()
    {
        ["p-05-wraiths"] = Array.Empty<string>(),
        ["a1-c1-01-gulls"] = Array.Empty<string>(),
        ["a1-c2-01-gloomfangs"] = Array.Empty<string>(),
        ["a1-c2-02-thornmother"] = new[] { "sera" },
        ["a1-c2-02-guard-wave"] = new[] { "sera" },
        ["a1-c3-02-raid"] = new[] { "sera" },
        ["a1-c4-02-kael"] = new[] { "sera", "mira" },
        ["a1-c4-02-sappers"] = new[] { "sera", "mira" },
    };

    /// <summary>Checks every encounter has its companions here, and they're all real.</summary>
    /// <remarks>Throws on purpose, so a new encounter can't be simulated with the wrong party by accident.</remarks>
    static CombatSimulator()
    {
        foreach (EncounterDefinition encounter in EncounterCatalog.All)
        {
            if (!Companions.TryGetValue(encounter.Id, out string[] companions))
                throw new InvalidOperationException($"CombatSimulator doesn't know who's with you for encounter {encounter.Id}.");
            foreach (string id in companions)
                if (!Origins.Exists(id)) throw new InvalidOperationException($"CombatSimulator puts {id} in encounter {encounter.Id}, who isn't an origin.");
        }
    }

    /// <summary>Fights an encounter (or every one) this many times and prints the table.</summary>
    /// <param name="encounterId">Which, by id, or "all".</param>
    /// <param name="runs">How many times each.</param>
    public static void Run(string encounterId, int runs)
    {
        runs = Math.Max(1, runs);
        if (encounterId == Everything)
        {
            foreach (EncounterDefinition encounter in EncounterCatalog.All) Report(encounter, runs);
            return;
        }

        if (!EncounterCatalog.Exists(encounterId))
        {
            Console.WriteLine($"There's no encounter called \"{encounterId}\". There's:");
            foreach (EncounterDefinition encounter in EncounterCatalog.All) Console.WriteLine("  " + encounter.Id);
            Console.WriteLine("  " + Everything);
            return;
        }
        Report(EncounterCatalog.Get(encounterId), runs);
    }

    /// <summary>Fights one encounter this many times and prints how it went.</summary>
    private static void Report(EncounterDefinition encounter, int runs)
    {
        var random = new Random(Seed);
        int won = 0, unfinished = 0, rounds = 0;
        var stalls = new Dictionary<string, int>();
        var hp = new Dictionary<string, double>();
        var downs = new Dictionary<string, int>();
        var downed = new Dictionary<string, int>();
        var died = new Dictionary<string, int>();
        IReadOnlyList<Combatant> sides = null;

        for (int run = 0; run < runs; run++)
        {
            CombatEngine engine = Fight(encounter, random, out string stall);
            sides = engine.Combatants;
            rounds += engine.Round;
            if (engine.Outcome == CombatOutcome.Victory) won++;
            if (stall is not null)
            {
                unfinished++;
                stalls[stall] = stalls.GetValueOrDefault(stall) + 1;
            }

            foreach (Combatant each in engine.Combatants)
            {
                if (each.Side != Side.Party) continue;
                hp[each.Id] = hp.GetValueOrDefault(each.Id) + each.Hp;
                downs[each.Id] = downs.GetValueOrDefault(each.Id) + each.Downs;
                downed[each.Id] = downed.GetValueOrDefault(each.Id) + (each.Downs > 0 ? 1 : 0);
                died[each.Id] = died.GetValueOrDefault(each.Id) + (each.IsDead ? 1 : 0);
            }
        }

        double wins = won / (double)runs;
        (double least, double most) = Target(encounter);
        string verdict = wins < least ? "TOO HARD" : wins > most ? "TOO EASY" : "on target";
        string band = most >= 1.0 ? Invariant($"{least:P0} or more") : Invariant($"{least:P0} to {most:P0}");

        Console.WriteLine(Invariant($"Legend Quest  --  {encounter.Id}, \"{encounter.Name}\": {runs} runs, seed {Seed}"));
        Console.WriteLine("Foes: " + Foes(encounter));
        Console.WriteLine();
        Console.WriteLine("WON      ROUNDS   UNFINISHED   TARGET           VERDICT");
        Console.WriteLine(Invariant($"{wins,-8:P1} {rounds / (double)runs,-8:F1} {unfinished / (double)runs,-12:P1} {band,-16} {verdict}"));
        Console.WriteLine();
        Console.WriteLine("MEMBER                HP LEFT         DOWNED   DOWNS   DIED");
        foreach (Combatant each in sides.Where(each => each.Side == Side.Party))
        {
            string name = (each.Guest ? each.Name + " (guest)" : each.Name) + ", " + ClassName(each);
            string left = Invariant($"{hp[each.Id] / runs:F1} / {each.MaxHp}");
            Console.WriteLine(Invariant($"{name,-21} {left,-15} {downed[each.Id] / (double)runs,-8:P0} {downs[each.Id] / (double)runs,-7:F2} {died[each.Id] / (double)runs:P1}"));
        }
        Console.WriteLine();

        if (stalls.Count == 0) return;
        Console.WriteLine(Invariant($"UNFINISHED, AND WHY (judged on the last {StallWatch.Window} rounds before the cap)"));
        foreach ((string why, int count) in stalls.OrderByDescending(entry => entry.Value).ThenBy(entry => entry.Key, StringComparer.Ordinal))
            Console.WriteLine(Invariant($"{count,6}  {why}"));
        Console.WriteLine();
    }

    /// <summary>One fight to its end (or the round cap): a fresh party set up against the encounter on open ground, both sides played by EnemyMind.</summary>
    /// <param name="encounter">The fight.</param>
    /// <param name="random">The dice.</param>
    /// <param name="stall">Why it never ended, if it ran into the cap; otherwise null.</param>
    private static CombatEngine Fight(EncounterDefinition encounter, Random random, out string stall)
    {
        var setups = new List<CombatantSetup>();
        List<PartyMember> party = Party(encounter);
        int partyRows = party.Count + encounter.Guests.Count;

        for (int i = 0; i < party.Count; i++)
            setups.Add(CombatantSetup.ForMember(party[i], Row(PartyColumn, i, partyRows), Direction.Right));
        for (int i = 0; i < encounter.Guests.Count; i++)
            setups.Add(CombatantSetup.ForGuest(encounter.Guests[i], Row(PartyColumn, party.Count + i, partyRows), Direction.Right));
        for (int i = 0; i < encounter.Foes.Count; i++)
        {
            EncounterFoe foe = encounter.Foes[i];
            string id = foe.Npc ?? $"{foe.Enemy}-{i + 1}";
            setups.Add(CombatantSetup.ForFoe(foe, id, Row(PartyColumn + StartGap, i, encounter.Foes.Count), Direction.Left));
        }

        var bag = new Inventory(new List<ItemStack>());
        bag.Add(ItemCatalog.HealingPotion, StartingPotions);

        var engine = new CombatEngine(encounter, setups, new OpenGround(GroundWidth, GroundHeight), random) { Bag = bag };
        var watch = new StallWatch();
        watch.See(engine, engine.Begin());
        while (engine.Outcome == CombatOutcome.Ongoing && engine.Round <= RoundCap) watch.See(engine, engine.RunAiTurn());

        stall = engine.Outcome == CombatOutcome.Ongoing ? watch.Verdict() : null;
        return engine;
    }

    /// <summary>The default level-1 party for an encounter: you as a Blademaster, the Soul-Bound as a Shadowstep, and the companions who'd be there, all fresh from their origins, less anyone the encounter sits out.</summary>
    private static List<PartyMember> Party(EncounterDefinition encounter)
    {
        PartyMember you = Origins.Create(Origins.You);
        you.Name = "You";
        PartyMember soulBound = Origins.Create(Origins.SoulBound);
        soulBound.Name = "Soul-Bound";

        var party = new List<PartyMember> { you, soulBound };
        foreach (string id in Companions[encounter.Id]) party.Add(Origins.Create(id));
        party.RemoveAll(member => encounter.SitsOut.Contains(member.Id));
        return party;
    }

    /// <summary>A tile in a column, the rows spread one apart round the middle of the ground.</summary>
    private static Point Row(int column, int index, int count) => new(column, GroundHeight / 2 - count / 2 + index);

    /// <summary>What the party's up against, for the heading: "3 x Red Gull Thug, Bosun Grell, then 2 more".</summary>
    private static string Foes(EncounterDefinition encounter)
    {
        IEnumerable<string> names = encounter.Foes.GroupBy(foe => foe.Enemy)
            .Select(group => (group.Count() > 1 ? $"{group.Count()} x " : string.Empty) + EnemyCatalog.Get(group.Key).Name);
        int later = encounter.Waves.Sum(wave => wave.Foes.Count);
        return string.Join(", ", names) + (later > 0 ? $", then {later} more" : string.Empty);
    }

    /// <summary>The story's name for someone's class, or "stat block" for a guest fighting from one.</summary>
    private static string ClassName(Combatant who) => who.Member is not null ? ClassCatalog.Get(who.Member.Class).Name : "stat block";

    /// <summary>The win rate an encounter's tuned to, by what kind of fight it is.</summary>
    private static (double Least, double Most) Target(EncounterDefinition encounter)
    {
        if (encounter.Tutorial) return (TutorialWins, 1.0);
        return encounter.Boss ? (BossWinsLeast, BossWinsMost) : (StoryWinsLeast, StoryWinsMost);
    }

    /// <summary>Formats with the invariant culture, so the table reads the same on any machine.</summary>
    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}
