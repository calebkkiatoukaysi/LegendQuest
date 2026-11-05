using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Fights an encounter headless, hundreds of times, with EnemyMind playing both sides, and prints how
/// it went against 11-combat.md §6.2's targets: how often the party wins, how long it takes, and
/// how often each of them goes down.
/// </summary>
/// <remarks>
/// The Black Box's RoundSimulator, for --simulate. No window and no game: just the catalogs, the
/// engine and a seeded Random, so two runs print the same table, and every fight starts again from
/// the seed, so it prints the same whether it's fought alone or with the rest. The party is the one
/// the story would have there: you as a Blademaster and the Soul-Bound as a Shadowstep with the
/// companions who'd be with you, at the level the story has them at by then, in the interim kits, at
/// full health, with the two potions a party sets out with. They stand in rows with the fight's
/// guests, its supports behind them, and the foes in the slots the encounter gives them. Every run is
/// a fresh party, so a fight that follows another (the guard wave after the Thornmother) is tested on
/// its own. An ally who only comes on a story flag is fought both ways: the verdict is on the fight
/// without them, the harder one, and the fight with them is printed beside it.
/// </remarks>
public static class CombatSimulator
{
    /// <summary>How many runs --simulate does when it isn't told.</summary>
    public const int DefaultRuns = 1000;

    /// <summary>What --simulate takes for every encounter in turn.</summary>
    public const string Everything = "all";

    /// <summary>The tutorial's target on Balanced: nobody should lose it.</summary>
    public const double TutorialWins = 0.99;

    /// <summary>A random fight's target on Balanced: never the hard part.</summary>
    public const double RandomWins = 0.97;

    /// <summary>A story fight's target on Balanced: hard to lose, not easy to sleepwalk.</summary>
    public const double StoryWinsLeast = 0.92, StoryWinsMost = 0.97;

    /// <summary>A mini-boss's target on Balanced: the Thornmother, Kael's duel.</summary>
    public const double MiniBossWinsLeast = 0.88, MiniBossWinsMost = 0.94;

    /// <summary>A boss's target on Balanced: a real chance of losing.</summary>
    public const double BossWinsLeast = 0.82, BossWinsMost = 0.90;

    /// <summary>The Sovereign's target on Balanced, with the vial.</summary>
    public const double SovereignWinsLeast = 0.78, SovereignWinsMost = 0.88;

    /// <summary>The most often any one of the party may go down in a tutorial, story or random fight.</summary>
    public const double DownedMost = 0.40;

    /// <summary>The most often any one of the party may go down in a mini-boss, boss or the Sovereign.</summary>
    public const double BossDownedMost = 0.70;

    /// <summary>Every fight's target on Story: all but never lost.</summary>
    public const double StoryModeWins = 0.99;

    /// <summary>About how far under Balanced's band Tactician lands, for its information-only targets.</summary>
    public const double TacticianDrop = 0.15;

    /// <summary>A fixed seed, so two runs of the simulator print the same table.</summary>
    private const int Seed = 12345;

    /// <summary>The rounds after which a fight still going counts as unfinished, not won. Nothing in the story should get near it.</summary>
    private const int RoundCap = 30;

    /// <summary>The healing potions in the bag at the start of every run, the ones a new party sets out with.</summary>
    private const int StartingPotions = 2;

    /// <summary>The story's companions in the party by each fight, beside you and the Soul-Bound: Sera from the Mirewood on, and Mira from Thornbridge's end (a guest in the raid itself). Whoever an encounter sits out (Sera at the crystal) is left out by the roster.</summary>
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

    /// <summary>Fights an encounter (or every one) this many times on a difficulty, prints each one's tables and then the report, and says whether they all passed.</summary>
    /// <param name="encounterId">Which, by id, or "all".</param>
    /// <param name="runs">How many times each.</param>
    /// <param name="difficulty">The difficulty they're fought on. Tactician's verdicts are for information, so it always passes.</param>
    /// <returns>Whether every fight met its targets; false for an encounter that isn't there.</returns>
    public static bool Run(string encounterId, int runs, Difficulty difficulty)
    {
        runs = Math.Max(1, runs);
        List<EncounterDefinition> fights;
        if (encounterId == Everything) fights = EncounterCatalog.All.ToList();
        else if (EncounterCatalog.Exists(encounterId)) fights = new List<EncounterDefinition> { EncounterCatalog.Get(encounterId) };
        else
        {
            Console.WriteLine($"There's no encounter called \"{encounterId}\". There's:");
            foreach (EncounterDefinition encounter in EncounterCatalog.All) Console.WriteLine("  " + encounter.Id);
            Console.WriteLine("  " + Everything);
            return false;
        }

        var rows = new List<Line>();
        foreach (EncounterDefinition encounter in fights)
        {
            Tally plain = Fights(encounter, runs, difficulty, false);
            Tally flagged = Conditional(encounter).Count > 0 ? Fights(encounter, runs, difficulty, true) : null;
            Line row = Judge(encounter, plain, difficulty);
            Detail(encounter, plain, flagged, row, difficulty);
            rows.Add(row);
            if (flagged is not null)
                rows.Add(Judge(encounter, flagged, difficulty) with { Fight = "  with " + string.Join(", ", Conditional(encounter)), Verdict = "(info)", Info = true });
        }

        Report(rows, runs, difficulty);
        return rows.All(row => row.Info || row.Passed);
    }

    // Fighting.

    /// <summary>Fights one encounter this many times from the seed, with or without its allies who need a story flag, and adds it all up.</summary>
    private static Tally Fights(EncounterDefinition encounter, int runs, Difficulty difficulty, bool withConditional)
    {
        var random = new Random(Seed);
        var tally = new Tally { Runs = runs };
        for (int run = 0; run < runs; run++)
        {
            CombatEngine engine = Fight(encounter, random, difficulty, withConditional, out string stall, out HashSet<string> fellBack);
            tally.Add(engine, stall, fellBack);
        }
        return tally;
    }

    /// <summary>One fight to its end (or the round cap): a fresh party lined up against the encounter, both sides played by EnemyMind.</summary>
    /// <param name="encounter">The fight.</param>
    /// <param name="random">The dice.</param>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="withConditional">Whether the allies who need a story flag come.</param>
    /// <param name="stall">Why it never ended, if it ran into the cap; otherwise null.</param>
    /// <param name="fellBack">The guests who fell back.</param>
    private static CombatEngine Fight(EncounterDefinition encounter, Random random, Difficulty difficulty, bool withConditional, out string stall, out HashSet<string> fellBack)
    {
        SortedSet<string> conditional = Conditional(encounter);
        bool Flag(string id) => withConditional && conditional.Contains(id);

        var setups = new List<CombatantSetup>(FightRoster.Build(encounter, Party(encounter), Flag, NoMeter).Setups);
        setups.AddRange(FightRoster.Foes(encounter));

        var bag = new Inventory(new List<ItemStack>());
        bag.Add(ItemCatalog.HealingPotion, StartingPotions);

        var rules = new FightRules(difficulty, Flag, AbilityCatalog.Interim);
        var engine = new CombatEngine(encounter, setups, random, rules) { Bag = bag };
        var watch = new StallWatch();
        fellBack = new HashSet<string>();
        See(engine, engine.Begin(), watch, fellBack);
        while (engine.Outcome == CombatOutcome.Ongoing && engine.Round <= RoundCap) See(engine, engine.RunAiTurn(), watch, fellBack);

        stall = engine.Outcome == CombatOutcome.Ongoing ? watch.Verdict() : null;
        return engine;
    }

    /// <summary>Passes what a command caused to the stall watch, and notes any guest who fell back.</summary>
    private static void See(CombatEngine engine, IReadOnlyList<CombatEvent> events, StallWatch watch, HashSet<string> fellBack)
    {
        watch.See(engine, events);
        foreach (CombatEvent happened in events)
        {
            if (happened.Kind != CombatEventKind.FellBack) continue;
            if (happened.Actor is not null) fellBack.Add(happened.Actor);
            if (happened.Target is not null) fellBack.Add(happened.Target);
        }
    }

    /// <summary>The party the story would have for an encounter: you as a Blademaster, the Soul-Bound as a Shadowstep, and the companions who'd be there, all fresh from their origins at the level the story has them at by then.</summary>
    private static List<PartyMember> Party(EncounterDefinition encounter)
    {
        PartyMember you = Origins.Create(Origins.You);
        you.Name = "You";
        PartyMember soulBound = Origins.Create(Origins.SoulBound);
        soulBound.Name = "Soul-Bound";

        var party = new List<PartyMember> { you, soulBound };
        foreach (string id in Companions[encounter.Id]) party.Add(Origins.Create(id));

        int level = LevelTable.Expected(encounter);
        foreach (PartyMember member in party) Leveling.SetLevel(member, level);
        return party;
    }

    // What the encounter and the story bring.

    /// <summary>Every meter at 0, for the roster: the simulator's party is never more than the side holds, so nobody's affinity decides who sits out.</summary>
    private static int NoMeter(string id) => 0;

    /// <summary>The story flags an encounter's allies need to come: what the fight's fought both ways on.</summary>
    private static SortedSet<string> Conditional(EncounterDefinition encounter)
    {
        var flags = new SortedSet<string>(StringComparer.Ordinal);
        foreach (EncounterGuest guest in encounter.Guests)
            if (guest.When is not null) flags.Add(guest.When);
        foreach (EncounterSupport support in encounter.Supports)
            if (support.When is not null) flags.Add(support.When);
        return flags;
    }

    /// <summary>Who helps in a fight, for the tables: its guests, then its supports with a "+", and a "?" on any who need a story flag.</summary>
    private static string Allies(EncounterDefinition encounter)
    {
        var allies = new List<string>();
        foreach (EncounterGuest guest in encounter.Guests) allies.Add(guest.Id + (guest.When is null ? string.Empty : "?"));
        foreach (EncounterSupport support in encounter.Supports) allies.Add("+" + support.Id + (support.When is null ? string.Empty : "?"));
        return allies.Count == 0 ? "-" : string.Join(" ", allies);
    }

    // Judging.

    /// <summary>A fight's win-rate target, least and most, by its kind and the difficulty; most is 1 for a target with no ceiling.</summary>
    private static (double Least, double Most) Target(EncounterKind kind, Difficulty difficulty)
    {
        if (difficulty == Difficulty.Story) return (StoryModeWins, 1.0);
        (double least, double most) = kind switch
        {
            EncounterKind.Tutorial => (TutorialWins, 1.0),
            EncounterKind.Random => (RandomWins, 1.0),
            EncounterKind.MiniBoss => (MiniBossWinsLeast, MiniBossWinsMost),
            EncounterKind.Boss => (BossWinsLeast, BossWinsMost),
            EncounterKind.Sovereign => (SovereignWinsLeast, SovereignWinsMost),
            _ => (StoryWinsLeast, StoryWinsMost),
        };
        return difficulty == Difficulty.Tactician ? (least - TacticianDrop, most >= 1.0 ? 1.0 : most - TacticianDrop) : (least, most);
    }

    /// <summary>The most often any one of the party may go down in a fight of a kind.</summary>
    private static double DownedCap(EncounterKind kind) =>
        kind is EncounterKind.MiniBoss or EncounterKind.Boss or EncounterKind.Sovereign ? BossDownedMost : DownedMost;

    /// <summary>A fight's line in the report, with its verdict: PASS, or FAIL and why.</summary>
    /// <remarks>On Balanced the win rate has to be in its band, nobody in the party may go down more often than the cap, and nothing may run into the round cap. On Story it's 99% or more and nothing unfinished. Tactician's is information only.</remarks>
    private static Line Judge(EncounterDefinition encounter, Tally tally, Difficulty difficulty)
    {
        EncounterKind kind = encounter.Kind;
        (double least, double most) = Target(kind, difficulty);
        double wins = tally.Won / (double)tally.Runs;
        double cap = DownedCap(kind);
        MemberTally downed = tally.MostDowned();
        double downedRate = downed is null ? 0.0 : downed.Downed / (double)tally.Runs;
        double unfinished = tally.Unfinished / (double)tally.Runs;

        var faults = new List<string>();
        if (wins < least) faults.Add("too hard");
        if (wins > most) faults.Add("too easy");
        if (difficulty == Difficulty.Balanced && downedRate > cap) faults.Add("downed");
        if (tally.Unfinished > 0) faults.Add("unfinished");

        bool info = difficulty == Difficulty.Tactician;
        string verdict = info ? faults.Count == 0 ? "(info) on target" : $"(info) {string.Join(", ", faults)}"
            : faults.Count == 0 ? "PASS" : $"FAIL: {string.Join(", ", faults)}";

        return new Line(
            encounter.Id, kind.ToString(), LevelTable.Expected(encounter).ToString(CultureInfo.InvariantCulture), Allies(encounter), Percent(wins), Band(least, most),
            Invariant($"{tally.Rounds / (double)tally.Runs:F1}"), downed is null ? "nobody" : $"{downed.Name} {Percent(downedRate)}",
            difficulty == Difficulty.Balanced ? Percent(cap, 0) : "-", Percent(unfinished), verdict, faults.Count == 0, info);
    }

    // Printing.

    /// <summary>One fight's own tables: what it was, how it went with and without its flagged allies, how each of the party fared, and why any run never finished.</summary>
    private static void Detail(EncounterDefinition encounter, Tally plain, Tally flagged, Line row, Difficulty difficulty)
    {
        Console.WriteLine(Invariant($"Legend Quest  --  {encounter.Id}, \"{encounter.Name}\": {plain.Runs} runs, seed {Seed}, {difficulty}"));
        Console.WriteLine($"{row.Kind}, level {row.Level}. Foes: {Foes(encounter)}. Allies: {row.Allies}.");
        Console.WriteLine();

        var lines = new List<string[]> { new[] { string.Empty, "WON", "ROUNDS", "UNFINISHED", "TARGET", "MOST DOWNED", "VERDICT" } };
        lines.Add(new[] { flagged is null ? "as built" : "without", row.Won, row.Rounds, row.Unfinished, row.Target, row.MostDowned, row.Verdict });
        if (flagged is not null)
        {
            Line extra = Judge(encounter, flagged, difficulty);
            lines.Add(new[] { "with " + string.Join(", ", Conditional(encounter)), extra.Won, extra.Rounds, extra.Unfinished, extra.Target, extra.MostDowned, "(info)" });
        }
        Table(lines);
        Console.WriteLine();

        var members = new List<string[]> { new[] { "MEMBER", "HP LEFT", "DOWNED", "DOWNS", "DIED" } };
        foreach (MemberTally each in plain.Members)
        {
            string name = (each.Guest ? each.Name + " (guest)" : each.Name) + ", " + each.Class;
            string downed = each.Guest ? Percent(each.FellBack / (double)plain.Runs, 0) + " fell back" : Percent(each.Downed / (double)plain.Runs, 0);
            members.Add(new[]
            {
                name, Invariant($"{each.Hp / plain.Runs:F1} / {each.MaxHp}"), downed,
                Invariant($"{each.Downs / (double)plain.Runs:F2}"), Percent(each.Died / (double)plain.Runs),
            });
        }
        Table(members);
        Console.WriteLine();

        if (plain.Stalls.Count == 0) return;
        Console.WriteLine(Invariant($"UNFINISHED, AND WHY (judged on the last {StallWatch.Window} rounds before the cap)"));
        foreach ((string why, int count) in plain.Stalls.OrderByDescending(entry => entry.Value).ThenBy(entry => entry.Key, StringComparer.Ordinal))
            Console.WriteLine(Invariant($"{count,6}  {why}"));
        Console.WriteLine();
    }

    /// <summary>The report: every fight's line against its targets, and how many passed.</summary>
    private static void Report(List<Line> rows, int runs, Difficulty difficulty)
    {
        Console.WriteLine(Invariant($"REPORT  --  {difficulty}, {runs} runs each, seed {Seed}"));
        var lines = new List<string[]> { new[] { "FIGHT", "KIND", "LV", "ALLIES", "WON", "TARGET", "ROUNDS", "MOST DOWNED", "CAP", "UNFINISHED", "VERDICT" } };
        foreach (Line row in rows)
            lines.Add(new[] { row.Fight, row.Kind, row.Level, row.Allies, row.Won, row.Target, row.Rounds, row.MostDowned, row.Cap, row.Unfinished, row.Verdict });
        Table(lines);
        Console.WriteLine();

        List<Line> judged = rows.Where(row => !row.Info).ToList();
        if (difficulty == Difficulty.Tactician)
        {
            Console.WriteLine($"Tactician is for information: targets about {Percent(TacticianDrop, 0)} under Balanced's.");
            return;
        }
        int passed = judged.Count(row => row.Passed);
        string failed = passed == judged.Count ? string.Empty : ": FAIL " + string.Join(", ", judged.Where(row => !row.Passed).Select(row => row.Fight));
        Console.WriteLine(Invariant($"{passed}/{judged.Count} PASS on {difficulty}{failed}"));
    }

    /// <summary>Prints rows of cells in columns as wide as their widest cell, two spaces apart.</summary>
    private static void Table(List<string[]> lines)
    {
        int columns = lines.Max(line => line.Length);
        var widths = new int[columns];
        foreach (string[] line in lines)
            for (int i = 0; i < line.Length; i++) widths[i] = Math.Max(widths[i], line[i].Length);

        foreach (string[] line in lines)
        {
            var text = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                text.Append(i == line.Length - 1 ? line[i] : line[i].PadRight(widths[i] + 2));
            }
            Console.WriteLine(text.ToString().TrimEnd());
        }
    }

    /// <summary>What the party's up against, for the heading: "3 x Red Gull Thug, Bosun Grell, then 2 more".</summary>
    private static string Foes(EncounterDefinition encounter)
    {
        IEnumerable<string> names = encounter.Foes.GroupBy(foe => foe.Enemy)
            .Select(group => (group.Count() > 1 ? $"{group.Count()} x " : string.Empty) + EnemyCatalog.Get(group.Key).Name);
        int later = encounter.Waves.Sum(wave => wave.Foes.Count);
        return string.Join(", ", names) + (later > 0 ? $", then {later} more" : string.Empty);
    }

    /// <summary>A target band: "92-97%", or "99%+" with no ceiling.</summary>
    private static string Band(double least, double most) =>
        most >= 1.0 ? Percent(least, 0) + "+" : Invariant($"{least * 100:F0}-{most * 100:F0}%");

    /// <summary>A share as a percentage, with no space before the sign whatever the machine's culture.</summary>
    private static string Percent(double share, int decimals = 1) =>
        (share * 100).ToString("F" + decimals, CultureInfo.InvariantCulture) + "%";

    /// <summary>Formats with the invariant culture, so the table reads the same on any machine.</summary>
    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

    /// <summary>One line of the report.</summary>
    private sealed record Line(string Fight, string Kind, string Level, string Allies, string Won, string Target, string Rounds, string MostDowned,
        string Cap, string Unfinished, string Verdict, bool Passed, bool Info = false);

    /// <summary>One fight's runs, added up.</summary>
    private sealed class Tally
    {
        private readonly Dictionary<string, MemberTally> _members = new();

        /// <summary>How many runs.</summary>
        public int Runs { get; init; }

        /// <summary>How many were won.</summary>
        public int Won { get; private set; }

        /// <summary>How many ran into the round cap.</summary>
        public int Unfinished { get; private set; }

        /// <summary>The rounds every run took, added up.</summary>
        public int Rounds { get; private set; }

        /// <summary>Why the unfinished ones never ended, and how often.</summary>
        public Dictionary<string, int> Stalls { get; } = new();

        /// <summary>Everyone who fought on the party's side, in the order they first joined.</summary>
        public List<MemberTally> Members { get; } = new();

        /// <summary>Adds one run.</summary>
        public void Add(CombatEngine engine, string stall, HashSet<string> fellBack)
        {
            Rounds += engine.Round;
            if (engine.Outcome == CombatOutcome.Victory) Won++;
            if (stall is not null)
            {
                Unfinished++;
                Stalls[stall] = Stalls.GetValueOrDefault(stall) + 1;
            }

            foreach (Combatant each in engine.Combatants)
            {
                if (each.Side != Side.Party) continue;
                if (!_members.TryGetValue(each.Id, out MemberTally member))
                {
                    member = new MemberTally(each.Name, ClassName(each), each.MaxHp, each.Guest);
                    _members[each.Id] = member;
                    Members.Add(member);
                }
                member.Hp += each.Hp;
                member.Downs += each.Downs;
                member.Downed += each.Downs > 0 ? 1 : 0;
                member.Died += each.IsDead ? 1 : 0;
                member.FellBack += fellBack.Contains(each.Id) ? 1 : 0;
            }
        }

        /// <summary>Whoever in the party proper (not a guest) went down in the most runs, or null if nobody did.</summary>
        public MemberTally MostDowned()
        {
            MemberTally most = null;
            foreach (MemberTally each in Members)
                if (!each.Guest && each.Downed > 0 && (most is null || each.Downed > most.Downed)) most = each;
            return most;
        }

        /// <summary>The story's name for someone's class, or "stat block" for a guest fighting from one.</summary>
        private static string ClassName(Combatant who) => who.Member is not null ? ClassCatalog.Get(who.Member.Class).Name : "stat block";
    }

    /// <summary>One of the party's side, over a fight's runs.</summary>
    /// <param name="name">What they're called.</param>
    /// <param name="className">Their class's name, or "stat block".</param>
    /// <param name="maxHp">Their most hit points.</param>
    /// <param name="guest">Whether they're a guest.</param>
    private sealed class MemberTally(string name, string className, int maxHp, bool guest)
    {
        /// <summary>What they're called.</summary>
        public string Name { get; } = name;

        /// <summary>Their class's name, or "stat block".</summary>
        public string Class { get; } = className;

        /// <summary>Their most hit points.</summary>
        public int MaxHp { get; } = maxHp;

        /// <summary>Whether they're a guest.</summary>
        public bool Guest { get; } = guest;

        /// <summary>Their hit points at the end of every run, added up.</summary>
        public double Hp { get; set; }

        /// <summary>The times they went down, over every run.</summary>
        public int Downs { get; set; }

        /// <summary>The runs they went down in at least once.</summary>
        public int Downed { get; set; }

        /// <summary>The runs they died in.</summary>
        public int Died { get; set; }

        /// <summary>The runs a guest fell back in.</summary>
        public int FellBack { get; set; }
    }
}
