using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// --simulate on the real-time engine (11-combat.md §9.2): fights encounters headless with the AI
/// driving everyone (you included, with the human handicap), on the fixed 20 ms step, each run from
/// its own seed, and reports each fight against its kind's targets.
/// </summary>
/// <remarks>
/// <para>
/// `--simulate &lt;id|all|classes|selftest|numbers|trace:id|skill:id&gt; [runs] [story|balanced|tactician]`. A
/// fight's report has its win rate, mean length in seconds, who went down most, and each member's
/// damage and healing a second and how long they held a foe's attention; then the verdict against
/// §9.2's targets (win rate, length, most downed, nothing unfinished). Tactician's verdicts are only
/// for information. `classes` puts each class in your place in every fight, and says which of its
/// skills the AI used there; `selftest` checks the engine replays a fight exactly from its seed, that
/// every skill of every class with a kit (and every foe's move) fires and does what its kind says,
/// that the advanced kits' own kinds of effect do the right thing, and that no fight stalls.
/// `trace:` prints one run of a fight, and `skill:` the self-test's scripted fight for one skill.
/// </para>
/// <para>
/// Run r of a fight is seeded Pcg32.Mix(Seed, the fight's id's hash, r), so a single run can be
/// replayed on its own. The party is the one the story would have there: you as a Blademaster, the
/// Soul-Bound as a Shadowstep, and the companions with you by then, at the story's level and rank,
/// with two healing potions in the bag.
/// </para>
/// </remarks>
public static partial class BattleSimulator
{
    /// <summary>How many runs --simulate does when it isn't told.</summary>
    public const int DefaultRuns = 200;

    /// <summary>What --simulate takes for every encounter.</summary>
    public const string Everything = "all";

    /// <summary>What --simulate takes for every class in your place.</summary>
    public const string EveryClass = "classes";

    /// <summary>What --simulate takes for the engine's own checks.</summary>
    public const string SelfTest = "selftest";

    /// <summary>What --simulate takes, before an encounter's id, to print one run's whole log (trace:a1-c3-02-raid), the run given by the runs number.</summary>
    public const string Trace = "trace:";

    /// <summary>What --simulate takes, before a skill's id, to print the self-test's scripted fight for that skill (skill:glasswright-refract).</summary>
    public const string SkillTrace = "skill:";

    /// <summary>The base seed every run's is mixed from.</summary>
    public const ulong Seed = 20261119UL;

    /// <summary>The fight time after which a fight still going counts as unfinished.</summary>
    public const int CapMs = 600_000;

    /// <summary>The healing potions in the bag at the start of every run.</summary>
    private const int StartingPotions = 2;

    /// <summary>How far under Balanced's band Tactician is expected to land.</summary>
    private const double TacticianDrop = 0.15;

    /// <summary>What every fight on Story must be won by.</summary>
    private const double StoryModeWins = 0.99;

    /// <summary>How far a class may fall under the median class's win rate.</summary>
    private const double ClassSpread = 0.05;

    /// <summary>The story's companions with you by each fight (Sera from the Mirewood on, Mira from Thornbridge's end).</summary>
    private static readonly Dictionary<string, string[]> Companions = new(StringComparer.Ordinal)
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

    /// <summary>Runs what --simulate was asked for, prints it, and says whether everything passed.</summary>
    /// <param name="what">An encounter id, "all", "classes" or "selftest".</param>
    /// <param name="runs">How many runs of each fight.</param>
    /// <param name="difficulty">The difficulty.</param>
    public static bool Run(string what, int runs, Difficulty difficulty)
    {
        runs = Math.Max(1, runs);
        if (what == SelfTest) return RunSelfTest();
        if (what == Numbers) return RunNumbers();
        if (what == EveryClass) return RunClasses(runs, difficulty);
        if (what.StartsWith(EveryClass + ":", StringComparison.Ordinal)) return RunClasses(runs, difficulty, what.Substring(EveryClass.Length + 1).Split(','));
        if (what.StartsWith(SkillTrace, StringComparison.Ordinal) && SkillCatalog.Exists(what.Substring(SkillTrace.Length)))
            return SelfTests.TraceSkill(SkillCatalog.Get(what.Substring(SkillTrace.Length)));
        if (what.StartsWith(Trace, StringComparison.Ordinal))
        {
            // trace:<encounter>, or trace:<encounter>@<class code> to put that class in your place.
            string[] parts = what.Substring(Trace.Length).Split('@');
            ClassDefinition youAs = parts.Length > 1 ? ClassCatalog.ByCode(parts[1]) : null;
            if (EncounterCatalog.Exists(parts[0]) && (parts.Length == 1 || youAs is not null))
                return RunTrace(EncounterCatalog.Get(parts[0]), runs == DefaultRuns ? 0 : runs, difficulty, youAs?.Id);
        }

        List<EncounterDefinition> fights;
        if (what == Everything) fights = EncounterCatalog.All.ToList();
        else if (EncounterCatalog.Exists(what)) fights = new List<EncounterDefinition> { EncounterCatalog.Get(what) };
        else
        {
            Console.WriteLine($"There's no encounter called \"{what}\". There's:");
            foreach (EncounterDefinition encounter in EncounterCatalog.All) Console.WriteLine("  " + encounter.Id);
            Console.WriteLine($"  {Everything}\n  {EveryClass}\n  {SelfTest}\n  {Numbers}\n  {Trace}<encounter id>\n  {SkillTrace}<skill id>");
            return false;
        }

        Console.WriteLine($"Real-time simulator: {runs} runs a fight on {difficulty}, seed {Seed}, clips from {MotionTimings.LoadedFrom ?? "(none: fallback timings)"}.");
        var rows = new List<Verdict>();
        foreach (EncounterDefinition encounter in fights)
        {
            Tally plain = Fights(encounter, runs, difficulty, flagged: false);
            Verdict verdict = Judge(encounter, plain, difficulty);
            Detail(encounter, plain, verdict);
            rows.Add(verdict);
            if (Conditional(encounter).Count > 0)
            {
                Tally with = Fights(encounter, runs, difficulty, flagged: true);
                Verdict info = Judge(encounter, with, difficulty) with { Fight = "  with " + string.Join(", ", Conditional(encounter)), Info = true };
                Console.WriteLine($"  with {string.Join(", ", Conditional(encounter))}: won {Pct(with.WinRate)}, {with.MeanSeconds:F0} s.");
                rows.Add(info);
            }
        }
        Report(rows, difficulty);
        return rows.All(row => row.Info || row.Passed);
    }

    /// <summary>Prints one run's whole log, a line an event with its time, and how it ended.</summary>
    private static bool RunTrace(EncounterDefinition encounter, int run, Difficulty difficulty, ClassId? youAs = null)
    {
        ulong seed = RunSeed(encounter, run, false);
        List<PartyMember> party = Party(encounter, youAs);
        var bag = new Inventory(new List<ItemStack>());
        bag.Add(ItemCatalog.HealingPotion, StartingPotions);
        var battle = new Battle(encounter, BattleRoster.Build(encounter, party), new BattleRules(difficulty, null, null, bag) { HumanHandicap = true }, seed);
        Console.WriteLine($"Trace of {encounter.Id}, run {run} (seed {seed}) on {difficulty}.");
        foreach (Fighter f in battle.Fighters)
            Console.WriteLine($"  {f.Id,-22} {f.Name,-18} {f.Side,-6} hp {f.Hp}/{f.MaxHp} ac {f.BaseAc} at ({f.Position.X:F0},{f.Position.Y:F0}) rank {f.Rank} level {f.Level} bar {string.Join(" ", f.Skills.Select(s => s.Id))}");
        while (battle.Outcome == FightOutcome.Ongoing && battle.NowMs < CapMs)
            foreach (BattleEvent e in battle.Step())
                if (e.Text is not null) Console.WriteLine($"{e.AtMs / 1000.0,7:F2}  {e.Text}");
        Console.WriteLine($"{battle.Outcome} at {battle.NowMs / 1000.0:F1} s.");
        foreach (Fighter f in battle.Fighters)
            Console.WriteLine($"  {f.Name,-18} hp {f.Hp}/{f.MaxHp} dealt {f.DamageDone} healed {f.HealingDone} downs {f.Downs} uses {string.Join(", ", f.Uses.Select(u => u.Key + "×" + u.Value))}");
        return true;
    }

    // Fighting.

    /// <summary>The party the story would have for a fight: you (as a class, if one's given), the Soul-Bound, and the companions, fresh, at the story's level and rank.</summary>
    internal static List<PartyMember> Party(EncounterDefinition encounter, ClassId? youAs = null)
    {
        PartyMember you = Origins.Create(Origins.You);
        you.Name = "You";
        if (youAs is ClassId cls && cls != you.Class)
        {
            you.Class = cls;
            you.Scores = ClassCatalog.Get(cls).Recommended;
            you.Equipment.Clear();
            you.PutOn(Origins.KitFor(Origins.You, cls));
        }
        PartyMember soulBound = Origins.Create(Origins.SoulBound);
        soulBound.Name = "Soul-Bound";

        var party = new List<PartyMember> { you, soulBound };
        if (Companions.TryGetValue(encounter.Id, out string[] companions))
            foreach (string id in companions) party.Add(Origins.Create(id));

        int level = LevelTable.Expected(encounter);
        foreach (PartyMember member in party)
        {
            Leveling.SetLevel(member, level);
            member.ClassPoints[member.Class] = ClassRanks.PointsFor(ClassRanks.ExpectedRank(encounter, member.Id));
            member.Hp = member.MaxHp;
        }
        return party;
    }

    /// <summary>One fight from a seed, to its end or the cap, with the AI on both sides; the event stream's hash if asked for.</summary>
    internal static Battle Fight(EncounterDefinition encounter, IReadOnlyList<PartyMember> party, Difficulty difficulty, bool flagged, ulong seed, out ulong hash, Pace pace = null)
    {
        SortedSet<string> conditional = Conditional(encounter);
        bool Flag(string id) => flagged && conditional.Contains(id);
        var bag = new Inventory(new List<ItemStack>());
        bag.Add(ItemCatalog.HealingPotion, StartingPotions);
        var rules = new BattleRules(difficulty, Flag, null, bag) { HumanHandicap = true };
        var battle = new Battle(encounter, BattleRoster.Build(encounter, party, Flag), rules, seed);
        hash = 14695981039346656037UL;
        while (battle.Outcome == FightOutcome.Ongoing && battle.NowMs < CapMs)
        {
            IReadOnlyList<BattleEvent> events = battle.Step();
            foreach (BattleEvent happened in events) hash = Mix(hash, happened.Signature());
            pace?.Observe(battle, events);
        }
        return battle;
    }

    /// <summary>Folds a line into an FNV-1a hash.</summary>
    private static ulong Mix(ulong hash, string text)
    {
        foreach (char c in text)
        {
            hash ^= c;
            hash *= 1099511628211UL;
        }
        hash ^= '\n';
        return hash * 1099511628211UL;
    }

    /// <summary>The seed of one run of a fight.</summary>
    internal static ulong RunSeed(EncounterDefinition encounter, int run, bool flagged) =>
        Pcg32.Mix(Seed, Pcg32.Hash(encounter.Id), (ulong)run, flagged ? 1UL : 0UL);

    /// <summary>Fights one encounter this many times and adds it all up.</summary>
    private static Tally Fights(EncounterDefinition encounter, int runs, Difficulty difficulty, bool flagged, ClassId? youAs = null)
    {
        var tally = new Tally();
        for (int run = 0; run < runs; run++)
        {
            List<PartyMember> party = Party(encounter, youAs);
            var pace = new Pace();
            Battle battle = Fight(encounter, party, difficulty, flagged, RunSeed(encounter, run, flagged), out _, pace);
            pace.Finish(battle);
            tally.Add(battle, pace);
        }
        return tally;
    }

    /// <summary>The story flags an encounter's allies need: what it's fought both ways on.</summary>
    private static SortedSet<string> Conditional(EncounterDefinition encounter)
    {
        var flags = new SortedSet<string>(StringComparer.Ordinal);
        foreach (EncounterGuest guest in encounter.Guests)
            if (guest.When is not null) flags.Add(guest.When);
        foreach (EncounterSupport support in encounter.Supports)
            if (support.When is not null) flags.Add(support.When);
        return flags;
    }

    // Judging.

    /// <summary>A kind of fight's targets on Balanced (11-combat.md §9.2).</summary>
    private static (double Least, double Most, int ShortestS, int LongestS, double Downed) Targets(EncounterKind kind) => kind switch
    {
        EncounterKind.Tutorial => (0.99, 1.0, 20, 50, 0.40),
        EncounterKind.Random => (0.97, 1.0, 15, 45, 0.40),
        EncounterKind.MiniBoss => (0.88, 0.94, 45, 120, 0.70),
        EncounterKind.Boss => (0.82, 0.90, 75, 180, 0.70),
        EncounterKind.Sovereign => (0.78, 0.88, 120, 240, 0.70),
        _ => (0.92, 0.97, 25, 75, 0.40),
    };

    /// <summary>A fight's verdict against its targets on a difficulty.</summary>
    private static Verdict Judge(EncounterDefinition encounter, Tally tally, Difficulty difficulty)
    {
        var (least, most, shortest, longest, downed) = Targets(encounter.Kind);
        if (difficulty == Difficulty.Story)
        {
            least = StoryModeWins;
            most = 1.0;
        }
        else if (difficulty == Difficulty.Tactician)
        {
            least = Math.Max(0, least - TacticianDrop);
            most = Math.Max(0, most - TacticianDrop);
        }

        var problems = new List<string>();
        if (tally.WinRate < least - 1e-9) problems.Add("too hard");
        if (tally.WinRate > most + 1e-9) problems.Add("too easy");
        if (difficulty == Difficulty.Balanced && tally.Won > 0)
        {
            if (tally.MeanSeconds < shortest) problems.Add("too short");
            if (tally.MeanSeconds > longest) problems.Add("too long");
        }
        if (difficulty != Difficulty.Story && tally.MostDownedShare > downed + 1e-9) problems.Add("too many downed");
        if (tally.Unfinished > 0) problems.Add($"{tally.Unfinished} unfinished");

        string band = most >= 1.0 ? $"{Pct(least)}+" : $"{Pct(least)}-{Pct(most)}";
        return new Verdict(encounter.Id, encounter.Kind.ToString(), LevelTable.Expected(encounter), tally.WinRate, band, tally.MeanSeconds, $"{shortest}-{longest}",
            tally.MostDowned, tally.MostDownedShare, tally.Unfinished, problems.Count == 0 ? "PASS" : string.Join(", ", problems),
            problems.Count == 0, difficulty == Difficulty.Tactician, tally.Pace);
    }

    /// <summary>Prints one fight's detail: the header, the result, and each member's numbers.</summary>
    private static void Detail(EncounterDefinition encounter, Tally tally, Verdict verdict)
    {
        Console.WriteLine();
        Console.WriteLine($"== {encounter.Id}: {encounter.Name} ({encounter.Kind}, level {LevelTable.Expected(encounter)}) ==");
        Console.WriteLine($"   won {Pct(tally.WinRate)} (target {verdict.Band}), mean {tally.MeanSeconds:F1} s (target {verdict.Length} s), unfinished {tally.Unfinished}, most downed {verdict.MostDowned} {Pct(verdict.MostDownedShare)}: {verdict.Result}");
        Console.WriteLine($"   {"MEMBER",-30}{"DOWNED",8}{"DMG/S",8}{"HEAL/S",8}{"THREAT",8}{"CTRL",7}{"HP LEFT",9}");
        foreach (MemberTally member in tally.Members.Values.OrderBy(m => m.Order))
            Console.WriteLine($"   {member.Name,-30}{Pct(member.DownedShare(tally.Runs)),8}{member.DamagePerSecond,8:F1}{member.HealingPerSecond,8:F1}{Pct(member.ThreatShare),8}{Pct(tally.Pace.ControlShare(member.Id)),7}{member.HpLeft(tally.Runs),9:F0}%");
        PaceTally pace = tally.Pace;
        string boss = pace.HadBoss
            ? $"; the boss lasts {pace.BossLasts:F0} s from its first wound ({string.Join(" + ", pace.PhaseSeconds.Select(s => s.ToString("F0", CultureInfo.InvariantCulture)))} s a phase), every phase seen in {Pct(pace.AllPhasesSeen)} of its falls"
            : string.Empty;
        Console.WriteLine($"   pace: your auto every {pace.AutoEvery:F1} s, a skill to press every {pace.AvailableEvery:F1} s and one pressed every {pace.PressEvery:F1} s (one ready {Pct(pace.ReadyShare)} of the time); a foe lasts {pace.FoeLasts:F1} s from its first wound, {pace.FoeUnderFire:F1} s of the whole party's fire{boss}.");
    }

    /// <summary>What 11-combat.md asks of a fight's pace: an auto about every 2 s, a skill to press every 1.5 to 3 s, an ordinary foe down in 8 to 15 s of the party's fire, a boss as long as its kind's band (75 to 180 s for a boss) with every phase seen.</summary>
    private static string PaceVerdict(PaceTally pace, EncounterKind kind)
    {
        var (_, _, shortest, longest, _) = Targets(kind);
        var notes = new List<string>();
        if (pace.AutoEvery is > 0 and (< 1.6 or > 2.7)) notes.Add("autos off 2 s");
        if (pace.AvailableEvery is > 3.0 or (> 0 and < 1.5)) notes.Add(pace.AvailableEvery > 3.0 ? "few presses" : "presses crowd");
        if (pace.FoeUnderFire > 0 && (pace.FoeUnderFire < 8 || pace.FoeUnderFire > 15)) notes.Add(pace.FoeUnderFire < 8 ? "foes fall fast" : "foes last long");
        if (pace.HadBoss && (pace.BossLasts < shortest || pace.BossLasts > longest)) notes.Add(pace.BossLasts < shortest ? "boss short" : "boss long");
        if (pace.HadBoss && pace.AllPhasesSeen < 0.95) notes.Add("phases skipped");
        return notes.Count == 0 ? "ok" : string.Join(", ", notes);
    }

    /// <summary>Prints the report: a line a fight, then how many passed.</summary>
    private static void Report(List<Verdict> rows, Difficulty difficulty)
    {
        Console.WriteLine();
        Console.WriteLine("REPORT");
        Console.WriteLine($"{"FIGHT",-26}{"KIND",-10}{"LV",4}{"WON",7}{"TARGET",10}{"SECS",7}{"TARGET",9}  {"MOST DOWNED",-34}{"UNFIN",6}  VERDICT");
        foreach (Verdict row in rows)
            Console.WriteLine($"{row.Fight,-26}{row.Kind,-10}{row.Level,4}{Pct(row.WinRate),7}{row.Band,10}{row.Seconds,7:F0}{row.Length,9}  {(row.MostDowned + " " + Pct(row.MostDownedShare)),-34}{row.Unfinished,6}  {(row.Info ? "(info) " : "")}{row.Result}");
        int judged = rows.Count(row => !row.Info);
        int passed = rows.Count(row => !row.Info && row.Passed);
        Console.WriteLine($"{passed}/{judged} PASS on {difficulty}{(difficulty == Difficulty.Tactician ? " (information only)" : "")}");

        // The pace (information: 11-combat.md's feel, not §9.2's targets).
        Console.WriteLine();
        Console.WriteLine("PACE (your auto, how often your bar lets you press a skill and how often you did, a skill ready, your mean Focus and how often it's full, an ordinary foe's life from its first wound and under the whole party's fire, the boss's life and phases)");
        Console.WriteLine($"{"FIGHT",-26}{"AUTO S",8}{"AVAIL S",9}{"PRESS S",9}{"READY",7}{"FOCUS",7}{"FULL",6}{"FOE S",7}{"FIRE S",7}{"BOSS S",8}  {"PHASES S",-16}{"SEEN",6}  PACE");
        foreach (Verdict row in rows.Where(r => !r.Fight.StartsWith("  ", StringComparison.Ordinal)))
        {
            PaceTally pace = row.Pace;
            string phases = pace.HadBoss ? string.Join("+", pace.PhaseSeconds.Select(s => s.ToString("F0", CultureInfo.InvariantCulture))) : "-";
            Console.WriteLine($"{row.Fight,-26}{pace.AutoEvery,8:F1}{pace.AvailableEvery,9:F1}{pace.PressEvery,9:F1}{Pct(pace.ReadyShare),7}{pace.MeanFocus,7:F0}{Pct(pace.FocusFullShare),6}{pace.FoeLasts,7:F1}{pace.FoeUnderFire,7:F1}{(pace.HadBoss ? pace.BossLasts.ToString("F0", CultureInfo.InvariantCulture) : "-"),8}  {phases,-16}{(pace.HadBoss ? Pct(pace.AllPhasesSeen) : "-"),6}  {PaceVerdict(pace, row.EncounterKind)}");
        }
    }

    /// <summary>A share as a whole percentage.</summary>
    private static string Pct(double share) => (share * 100).ToString("F0", CultureInfo.InvariantCulture) + "%";

    // Classes.

    /// <summary>`--simulate classes`: each class in your place in every built fight; its win rate, length, damage and healing a second, threat held and control; whether it's within 5 points of the median class either way; and whether it leads its party at something.</summary>
    /// <remarks>
    /// 11-combat.md §9.2: no class is useless. Each must win its share of the fights within 5 points of
    /// the median class's, and lead its role at something: in at least half the fights it's the best
    /// of everyone on its side at one of damage a second, healing a second (by its skills: a potion is
    /// nobody's role), holding the foes' attention, or holding them under control (Pace.Control).
    /// </remarks>
    /// <param name="runs">The runs a fight, of which each class gets a quarter.</param>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="only">The class codes to run (classes:fighter,rogue), or null for every class; the median is then of these alone.</param>
    private static bool RunClasses(int runs, Difficulty difficulty, IReadOnlyCollection<string> only = null)
    {
        runs = Math.Max(1, runs / 4);
        Console.WriteLine($"Every class in your place: {runs} runs a fight on {difficulty}. A class without its own kit yet fights with its base class's (interim).");
        var lines = new List<ClassLine>();
        var usage = new List<(string Name, int TopRank, Dictionary<string, long> Used, List<string> Unused)>();
        foreach (ClassDefinition cls in ClassCatalog.All.Where(c => only is null || only.Contains(c.Code)))
        {
            var line = new ClassLine { Id = cls.Id, Name = cls.Name, Own = SkillCatalog.HasKit(cls.Id) };
            int topRank = 0;
            var used = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (EncounterDefinition encounter in EncounterCatalog.All)
            {
                Tally tally = Fights(encounter, runs, difficulty, flagged: false, cls.Id);
                MemberTally you = tally.Members.TryGetValue(Origins.You, out MemberTally found) ? found : null;
                line.Fights++;
                line.Win += tally.WinRate;
                line.ByFight.Add(tally.WinRate);
                line.Seconds += tally.MeanSeconds;
                if (you is null) continue;
                double control = tally.Pace.ControlShare(you.Id);
                line.Dps += you.DamagePerSecond;
                line.Hps += you.HealingPerSecond;
                line.Threat += you.ThreatShare;
                line.Control += control;
                line.PressEvery += tally.Pace.PressEvery;
                line.Available += tally.Pace.AvailableEvery;
                line.Ready += tally.Pace.ReadyShare;
                line.Focus += tally.Pace.MeanFocus;
                List<MemberTally> others = tally.Members.Values.Where(m => m.Id != you.Id).ToList();
                if (others.Count == 0 || you.DamagePerSecond >= others.Max(m => m.DamagePerSecond)) line.LeadsDamage++;
                long healed = tally.Pace.SkillHealing(you.Id);
                if (healed > 0 && (others.Count == 0 || healed >= others.Max(m => tally.Pace.SkillHealing(m.Id)))) line.LeadsHealing++;
                if (you.ThreatShare > 0 && (others.Count == 0 || you.ThreatShare >= others.Max(m => m.ThreatShare))) line.LeadsThreat++;
                if (control > 0 && (others.Count == 0 || control >= others.Max(m => tally.Pace.ControlShare(m.Id)))) line.LeadsControl++;
                topRank = Math.Max(topRank, you.TopRank);
                foreach ((string id, long count) in you.Uses) used[id] = (used.TryGetValue(id, out long sum) ? sum : 0) + count;
            }
            usage.Add((cls.Name, topRank, used, line.Own
                ? SkillCatalog.Kit(cls.Id).All.Where(skill => skill.Kind != SkillKind.Passive && skill.Rank <= topRank && !used.ContainsKey(skill.Id)).Select(skill => skill.Id).ToList()
                : new List<string>()));
            Console.Error.WriteLine($"  {cls.Name}: done.");
            lines.Add(line);
        }

        List<double> own = lines.Where(l => l.Own).Select(l => l.MeanWin).OrderBy(w => w).ToList();
        double median = own.Count == 0 ? 0 : own.Count % 2 == 1 ? own[own.Count / 2] : (own[own.Count / 2 - 1] + own[own.Count / 2]) / 2;

        Console.WriteLine();
        Console.WriteLine($"{"CLASS",-20}{"KIT",-9}{"WON",7}{"SECS",7}{"DMG/S",8}{"HEAL/S",8}{"THREAT",8}{"CTRL",7}{"AVAIL S",8}{"PRESS S",8}{"READY",7}{"FOCUS",6}  {"LEADS ITS PARTY IN (FIGHTS)",-40}VERDICT");
        bool passed = true;
        foreach (ClassLine line in lines)
        {
            var notes = new List<string>();
            if (line.MeanWin < median - ClassSpread - 1e-9) notes.Add($"over {Pct(ClassSpread)} under the median ({Pct(median)})");
            if (line.MeanWin > median + ClassSpread + 1e-9) notes.Add($"over {Pct(ClassSpread)} over the median ({Pct(median)})");
            int half = (line.Fights + 1) / 2;
            if (Math.Max(Math.Max(line.LeadsDamage, line.LeadsHealing), Math.Max(line.LeadsThreat, line.LeadsControl)) < half) notes.Add("leads nothing");
            var leads = new List<string>();
            if (line.LeadsDamage > 0) leads.Add($"damage {line.LeadsDamage}");
            if (line.LeadsHealing > 0) leads.Add($"healing {line.LeadsHealing}");
            if (line.LeadsThreat > 0) leads.Add($"threat {line.LeadsThreat}");
            if (line.LeadsControl > 0) leads.Add($"control {line.LeadsControl}");
            bool ok = notes.Count == 0;
            if (line.Own) passed &= ok;
            Console.WriteLine($"{line.Name,-20}{(line.Own ? "own" : "interim"),-9}{Pct(line.MeanWin),7}{line.Seconds / line.Fights,7:F0}{line.Dps / line.Fights,8:F1}{line.Hps / line.Fights,8:F1}{Pct(line.Threat / line.Fights),8}{Pct(line.Control / line.Fights),7}{line.Available / line.Fights,8:F1}{line.PressEvery / line.Fights,8:F1}{Pct(line.Ready / line.Fights),7}{line.Focus / line.Fights,6:F0}  {string.Join(", ", leads),-40}{(line.Own ? "" : "(info) ")}{(ok ? "PASS" : string.Join(", ", notes))}");
        }
        Console.WriteLine();
        Console.WriteLine($"{"WON BY FIGHT",-20}{string.Join("", EncounterCatalog.All.Select(e => $"{e.Id.Split('-').Last(),12}"))}");
        foreach (ClassLine line in lines) Console.WriteLine($"{line.Name,-20}{string.Join("", line.ByFight.Select(w => $"{Pct(w),12}"))}");
        Console.WriteLine($"Median class (own kits): {Pct(median)}. {(passed ? "Every class with a kit is within 5 points of it, and leads its party at something in half the fights or more." : "Some class is more than 5 points from it, or leads nothing.")}");

        // What the AI did with each kit in your place: every skill it used, and any it had and never used.
        Console.WriteLine();
        Console.WriteLine("What you used, as each class (uses over every run; the highest rank reached in these fights; any skill open at it that was never used):");
        foreach (var (name, topRank, used, unused) in usage)
        {
            Console.WriteLine($"  {name,-18} rank {topRank,2}: {string.Join(", ", used.OrderByDescending(u => u.Value).Select(u => u.Key + " " + u.Value))}");
            if (unused.Count > 0) Console.WriteLine($"  {"",-18}          never used: {string.Join(", ", unused)}");
        }
        return passed;
    }

    /// <summary>One class's numbers in your place, summed over the fights.</summary>
    private sealed class ClassLine
    {
        public ClassId Id;
        public string Name;
        public bool Own;
        public int Fights;
        public double Win, Seconds, Dps, Hps, Threat, Control, PressEvery, Ready, Focus, Available;
        public readonly List<double> ByFight = new();
        public int LeadsDamage, LeadsHealing, LeadsThreat, LeadsControl;

        public double MeanWin => Fights == 0 ? 0 : Win / Fights;
    }

    // The self-test.

    /// <summary>`--simulate selftest`: determinism, every skill and foe move firing, and no fight stalling.</summary>
    private static bool RunSelfTest()
    {
        Console.WriteLine($"Self-test of the real-time engine. Clips from {MotionTimings.LoadedFrom ?? "(none: fallback timings)"}.");
        bool passed = true;
        passed &= SelfTests.Determinism();
        passed &= SelfTests.EverySkill();
        passed &= SelfTests.Mechanics();
        passed &= SelfTests.EveryFoeMove();
        passed &= SelfTests.NoStalls();
        Console.WriteLine(passed ? "SELFTEST PASS" : "SELFTEST FAIL");
        return passed;
    }

    /// <summary>One fight's verdict, for the report.</summary>
    private sealed record Verdict(string Fight, string Kind, int Level, double WinRate, string Band, double Seconds, string Length, string MostDowned,
        double MostDownedShare, int Unfinished, string Result, bool Passed, bool Info, PaceTally Pace)
    {
        /// <summary>The kind of fight, for the pace's boss band.</summary>
        public EncounterKind EncounterKind => Enum.Parse<EncounterKind>(Kind);
    }

    /// <summary>Many runs of one fight, added up.</summary>
    private sealed class Tally
    {
        public int Runs;
        public int Won;
        public int Unfinished;
        public long WonMs;
        public readonly Dictionary<string, MemberTally> Members = new(StringComparer.Ordinal);
        public readonly PaceTally Pace = new();

        public double WinRate => Runs == 0 ? 0 : Won / (double)Runs;

        public double MeanSeconds => Won == 0 ? 0 : WonMs / 1000.0 / Won;

        public string MostDowned => Members.Values.Where(m => m.Member).OrderByDescending(m => m.DownedRuns).ThenBy(m => m.Order).FirstOrDefault()?.Name ?? "-";

        public double MostDownedShare => Runs == 0 ? 0 : Members.Values.Where(m => m.Member).Select(m => m.DownedRuns).DefaultIfEmpty(0).Max() / (double)Runs;

        public void Add(Battle battle, Pace pace)
        {
            Runs++;
            Pace.Add(pace);
            if (battle.Outcome == FightOutcome.Won)
            {
                Won++;
                WonMs += battle.NowMs;
            }
            else if (battle.Outcome == FightOutcome.Ongoing) Unfinished++;

            int order = 0;
            foreach (Fighter f in battle.Fighters)
            {
                if (f.Side != Side.Party || f.Kind == FighterKind.Summon) continue;
                if (!Members.TryGetValue(f.Id, out MemberTally member))
                {
                    member = new MemberTally { Id = f.Id, Name = $"{f.Name} ({(f.ClassId is ClassId c ? ClassCatalog.Get(c).Name : f.EnemyId)}{(f.Kind == FighterKind.Guest ? ", guest" : "")})", Order = order, Member = f.Kind == FighterKind.Member };
                    Members[f.Id] = member;
                }
                order++;
                if (f.Downs > 0 || f.IsDead || f.HasLeft) member.DownedRuns++;
                member.Damage += f.DamageDone;
                member.Healing += f.HealingDone;
                member.ThreatMs += f.ThreatHeldMs;
                member.FightMs += battle.NowMs;
                member.HpShare += f.IsStanding ? f.HpShare : 0;
                member.TopRank = Math.Max(member.TopRank, f.Rank);
                foreach ((string id, int count) in f.Uses) member.Uses[id] = (member.Uses.TryGetValue(id, out long sum) ? sum : 0) + count;
            }
        }
    }

    /// <summary>One member's numbers over many runs.</summary>
    private sealed class MemberTally
    {
        public string Id;
        public string Name;
        public int Order;
        public bool Member;
        public int DownedRuns;
        public long Damage;
        public long Healing;
        public long ThreatMs;
        public long FightMs;
        public double HpShare;
        public int TopRank;
        public readonly Dictionary<string, long> Uses = new(StringComparer.Ordinal);

        public double DownedShare(int runs) => runs == 0 ? 0 : DownedRuns / (double)runs;

        public double DamagePerSecond => FightMs == 0 ? 0 : Damage * 1000.0 / FightMs;

        public double HealingPerSecond => FightMs == 0 ? 0 : Healing * 1000.0 / FightMs;

        public double ThreatShare => FightMs == 0 ? 0 : ThreatMs / (double)FightMs;

        public double HpLeft(int runs) => runs == 0 ? 0 : HpShare * 100 / runs;
    }
}
