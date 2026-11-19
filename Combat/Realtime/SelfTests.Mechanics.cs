using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// The self-test's checks of the advanced kits' own kinds of effect: each is set up in a scripted
/// fight and must do what its row says, not just fire. The every-skill check shows each skill does
/// something; these show the unusual ones do the right thing.
/// </summary>
internal static partial class SelfTests
{
    /// <summary>A long enough wait for a control effect's span, in the checks that hold foes on someone.</summary>
    private const int HoldMs = 30000;

    /// <summary>The checks, by name.</summary>
    private static readonly (string Name, Func<string> Check)[] MechanicChecks =
    {
        ("Broadside outdoors: fire on every foe", BroadsideOutdoors),
        ("Broadside indoors: Grapeshot on foes within 60 px only", BroadsideIndoors),
        ("Whistle for the Watch: a Watchman joins and fights", () => SummonFights(ClassId.Watchblade, "watchblade-whistle-for-the-watch", "watchman")),
        ("Call the Order: a knight joins and fights", () => SummonFights(ClassId.GildedKnight, "gilded-knight-call-the-order", "gilded-order-knight")),
        ("Arrest: takes a quartered foe out of a nonlethal fight", ArrestTakesOut),
        ("Pick Pocket: a fifth of the loot, once a foe", PickPocketOncePerFoe),
        ("Captain's Share: the fight's gold bonus", CaptainsShare),
        ("Throw Anything: the bag's Alchemist's Fire, used up", ThrowsFromTheBag),
        ("Lucky Stone: an ally's next attack is a natural 20", LuckyStone),
        ("Cover: the next attack on the ally comes at you", CoverTakesTheBlow),
        ("Hold: foes pick nobody behind you", HoldKeepsThemOff),
        ("Counterspell: a foe's cast fails", CounterspellStopsCast),
        ("Mirror Image: three images, struck instead of you, then gone", MirrorImageDecoys),
        ("Refract: a ranged attack on an ally hits a foe instead", RefractTurnsIt),
        ("Sanctuary: a foe that fails its save won't go at them", SanctuaryHolds),
        ("Reveal: images off, hiding over, every foe Marked", RevealDispels),
        ("Rewind: hit points back to 6 s ago", RewindRestores),
        ("Bleed the Clock: every foe Frozen or Slowed; the Frozen do nothing", BleedTheClockStops),
        ("Echo Strike: a skill happens again", EchoStrikeEchoes),
        ("Somebody Allowed to Say No: a foe's big wind-up is stopped, once", SomebodySaysNo),
        ("Staying Behind: once a fight, 1 hit point instead of 0", StayingBehind),
    };

    /// <summary>Runs every mechanics check and prints a line each.</summary>
    public static bool Mechanics()
    {
        Console.WriteLine();
        Console.WriteLine("Mechanics: the advanced kits' own kinds of effect, each set up in a scripted fight.");
        bool passed = true;
        foreach ((string name, Func<string> check) in MechanicChecks)
        {
            string problem;
            try
            {
                problem = check();
            }
            catch (Exception fault)
            {
                problem = $"threw {fault.GetType().Name}: {fault.Message}";
            }
            passed &= problem is null;
            Console.WriteLine($"  {name}: {(problem is null ? "ok" : "FAIL (" + problem + ")")}");
        }
        return passed;
    }

    // Helpers.

    /// <summary>Steps a fight for a while (or to its end), doing something before each step, and gives back everything that happened.</summary>
    private static List<BattleEvent> Run(Battle battle, int ms, Action each = null)
    {
        var seen = new List<BattleEvent>();
        int until = battle.NowMs + ms;
        while (battle.NowMs < until && battle.Outcome == FightOutcome.Ongoing)
        {
            each?.Invoke();
            seen.AddRange(battle.Step());
        }
        return seen;
    }

    /// <summary>Orders a skill used and steps the fight for a while.</summary>
    private static List<BattleEvent> Use(Battle battle, Fighter me, string skill, Fighter target, int ms)
    {
        battle.Command(BattleCommand.Use(me.Id, skill, target?.Id));
        return Run(battle, ms);
    }

    /// <summary>The foes of a scripted fight, in their order.</summary>
    private static List<Fighter> Foes(Battle battle) => battle.Fighters.Where(f => f.Side == Side.Foes).ToList();

    /// <summary>Puts someone just in front of another, within melee reach.</summary>
    private static void Beside(Fighter who, Fighter of, float side = 0f) =>
        who.Position = of.Position + new Vector2(of.Facing * (Ranges.Melee * 0.6f + of.Size + who.Size), side);

    /// <summary>Holds a foe on someone: it's Taunted by them for 30 s.</summary>
    private static void HoldOn(Battle battle, Fighter foe, Fighter on)
    {
        battle.ApplyAura(foe, new AuraDefinition { Id = "taunted", Name = "Taunted", Condition = ConditionId.Taunted, DurationMs = HoldMs }, on, null, 0);
        foe.Target = on;
    }

    /// <summary>Whether an event is an attack's outcome on someone.</summary>
    private static bool Outcome(BattleEvent e) =>
        e.Kind is BattleEventKind.Damaged or BattleEventKind.Absorbed or BattleEventKind.Missed or BattleEventKind.Dodged or BattleEventKind.Blocked;

    /// <summary>The live aura of an id on someone, or null.</summary>
    private static Aura Live(Fighter f, string id) => f.Auras.FirstOrDefault(a => a.Id == id && !a.Gone);

    /// <summary>The scripted fight's foes with a hexer at their head, for the checks that need a foe that casts at range.</summary>
    private static EncounterDefinition HexerFight(bool nonlethal = false) => new()
    {
        Id = "selftest-hexer", Name = "Self-test", ExpectedLevel = 5, Nonlethal = nonlethal,
        Foes = new[]
        {
            new EncounterFoe("hollowed-hexer", null, Slot.Of(Side.Foes, 1)), new EncounterFoe("hollowed-footman", null, Slot.Of(Side.Foes, 2)),
            new EncounterFoe("hollowed-footman", null, Slot.Of(Side.Foes, 3)),
        },
    };

    // The checks: each returns null if it did what it says, else what went wrong.

    private static string BroadsideOutdoors()
    {
        var (battle, me, _, _) = Scripted(ClassId.Deckhand);
        List<BattleEvent> seen = Use(battle, me, "deckhand-broadside", null, 3000);
        int burnt = seen.Where(e => e.ActorId == me.Id && e.SkillId == "deckhand-broadside" && e.Kind == BattleEventKind.Damaged && e.DamageType == DamageType.Fire)
            .Select(e => e.TargetId).Distinct().Count();
        int foes = Foes(battle).Count;
        return burnt == foes ? null : $"fire on {burnt} of {foes} foes";
    }

    private static string BroadsideIndoors()
    {
        var (battle, me, _, _) = Scripted(ClassId.Deckhand, outdoors: false);
        List<Fighter> foes = Foes(battle);
        void Place()
        {
            Beside(foes[0], me);
            foes[1].Position = me.Position + new Vector2(me.Facing * 250f, -20f);
            foes[2].Position = me.Position + new Vector2(me.Facing * 250f, 20f);
        }
        Place();
        battle.Command(BattleCommand.Use(me.Id, "deckhand-broadside", null));
        List<BattleEvent> seen = Run(battle, 2500, Place);
        var hit = seen.Where(e => e.ActorId == me.Id && e.SkillId == "deckhand-broadside" && Outcome(e)).ToList();
        if (hit.Any(e => e.DamageType == DamageType.Fire)) return "fired the ship's guns indoors";
        if (!hit.Any(e => e.TargetId == foes[0].Id && e.Kind == BattleEventKind.Damaged && e.DamageType == DamageType.Piercing)) return "the near foe took no grapeshot";
        if (hit.Any(e => e.TargetId != foes[0].Id)) return "grapeshot reached a foe 250 px away";
        return null;
    }

    private static string SummonFights(ClassId cls, string skill, string enemy)
    {
        var (battle, me, _, _) = Scripted(cls);
        List<BattleEvent> seen = Use(battle, me, skill, null, 9000);
        Fighter called = battle.Fighters.FirstOrDefault(f => f.EnemyId == enemy && f.Summoner == me);
        if (called is null) return "nobody came";
        if (called.Side != Side.Party || called.Kind != FighterKind.Summon) return $"came as {called.Side} {called.Kind}";
        if (!seen.Any(e => e.ActorId == called.Id && e.Kind == BattleEventKind.Damaged)) return $"{called.Name} never hurt a foe in 9 s";
        if (battle.WhyNot(me, SkillCatalog.Get(skill)) is null) return "could be used again at once";
        return null;
    }

    private static string ArrestTakesOut()
    {
        var (battle, me, _, _) = Scripted(ClassId.Watchblade, HexerFight(nonlethal: true));
        Fighter foe = Foes(battle)[1];
        Beside(foe, me);
        foe.Hp = foe.MaxHp / 5;
        battle.Command(BattleCommand.Target(me.Id, foe.Id));
        Use(battle, me, "watchblade-arrest", foe, 2500);
        return foe.HasLeft ? null : $"{foe.Name} on {foe.Hp}/{foe.MaxHp} is still in the fight";
    }

    private static string PickPocketOncePerFoe()
    {
        var (battle, me, _, _) = Scripted(ClassId.Lanerunner);
        List<Fighter> foes = Foes(battle);
        Beside(foes[0], me);
        battle.Command(BattleCommand.Target(me.Id, foes[0].Id));
        Use(battle, me, "lanerunner-pick-pocket", foes[0], 1500);
        if (!battle.Stolen.TryGetValue(foes[0].Id, out int share) || share != 20) return $"took {share}% from the first foe, not 20%";
        List<BattleEvent> again = Use(battle, me, "lanerunner-pick-pocket", foes[0], 1500);
        if (battle.Stolen[foes[0].Id] != 20 || !again.Any(e => e.Kind == BattleEventKind.Denied && e.ActorId == me.Id)) return "picked the same pocket twice";
        Beside(foes[1], me, 8f);
        Use(battle, me, "lanerunner-pick-pocket", foes[1], 1500);
        return battle.Stolen.ContainsKey(foes[1].Id) ? null : "couldn't pick a second foe's pocket";
    }

    private static string CaptainsShare()
    {
        var (battle, _, _, _) = Scripted(ClassId.Deckhand);
        return battle.GoldBonusPct == 20 ? null : $"gold bonus {battle.GoldBonusPct}%, not 20%";
    }

    private static string ThrowsFromTheBag()
    {
        var (battle, me, _, foe) = Scripted(ClassId.Wayfarer, bagExtra: "alchemists-fire");
        battle.Command(BattleCommand.Target(me.Id, foe.Id));
        List<BattleEvent> seen = Use(battle, me, "wayfarer-throw-anything", foe, 3000);
        if (battle.Rules.Bag.Count("alchemists-fire") != 0) return "the flask is still in the bag";
        if (!seen.Any(e => e.ActorId == me.Id && e.SkillId == "wayfarer-throw-anything" && Outcome(e) && (e.Kind != BattleEventKind.Damaged || e.DamageType == DamageType.Fire)))
            return "no fire landed";
        return null;
    }

    private static string LuckyStone()
    {
        var (battle, me, friend, _) = Scripted(ClassId.Wayfarer);
        battle.Command(BattleCommand.Use(me.Id, "wayfarer-lucky-stone", null));
        bool lucky = false;
        int until = battle.NowMs + 9000;
        while (battle.NowMs < until)
            foreach (BattleEvent e in battle.Step())
            {
                if (e.Kind == BattleEventKind.AuraApplied && e.AuraId == "lucky-stone" && e.TargetId == friend.Id) lucky = true;
                if (lucky && e.ActorId == friend.Id && Outcome(e) && e.TargetId != friend.Id)
                    return e.Flags.HasFlag(HitFlags.Crit) ? null : $"the friend's next attack after the stone wasn't a natural 20 ({e.Text})";
            }
        return lucky ? "the friend never attacked" : "the stone never reached the friend";
    }

    private static string CoverTakesTheBlow()
    {
        var (battle, me, friend, foe) = Scripted(ClassId.Bulwark);
        Use(battle, me, "bulwark-cover", friend, 300);
        Beside(foe, friend);
        battle.StartAction(foe, foe.Auto, friend, true);
        List<BattleEvent> seen = Run(battle, 2500);
        BattleEvent first = seen.FirstOrDefault(e => e.ActorId == foe.Id && Outcome(e));
        if (first is null) return "the foe never attacked";
        return first.TargetId == me.Id ? null : $"the blow went at {first.TargetId}";
    }

    private static string HoldKeepsThemOff()
    {
        var (battle, me, friend, _) = Scripted(ClassId.Bulwark);
        List<Fighter> foes = Foes(battle);
        Vector2 front = me.Position + new Vector2(me.Facing * 40f, 0f);
        foreach (Fighter foe in foes) foe.Position = front + new Vector2(me.Facing * 20f, (foes.IndexOf(foe) - 1) * 12f);
        friend.Position = me.Position - new Vector2(me.Facing * 60f, 0f);
        battle.Command(BattleCommand.Use(me.Id, "bulwark-hold", null));
        Run(battle, 900, () => friend.Position = me.Position - new Vector2(me.Facing * 60f, 0f));
        if (Live(friend, "held-back") is null) return "the ally behind you isn't held back";
        List<BattleEvent> seen = Run(battle, 2200, () => friend.Position = me.Position - new Vector2(me.Facing * 60f, 0f));
        return seen.Any(e => e.Kind == BattleEventKind.ActionStarted && e.TargetId == friend.Id && foes.Any(f => f.Id == e.ActorId))
            ? "a foe went at the ally behind you" : null;
    }

    private static string CounterspellStopsCast()
    {
        var (battle, me, _, _) = Scripted(ClassId.Battlemage, HexerFight());
        Fighter hexer = Foes(battle)[0];
        Use(battle, me, "battlemage-counterspell", null, 300);
        hexer.Position = me.Position + new Vector2(me.Facing * 120f, 0f);
        battle.StartAction(hexer, hexer.Auto, me, true);
        List<BattleEvent> seen = Run(battle, 2500);
        return seen.Any(e => e.Kind == BattleEventKind.CastInterrupted && e.ActorId == hexer.Id) ? null : "the hexer's cast went on";
    }

    private static string MirrorImageDecoys()
    {
        var (battle, me, _, _) = Scripted(ClassId.Glasswright);
        Use(battle, me, "glasswright-mirror-image", null, 600);
        Aura images = Live(me, "mirror-image");
        if (images is null || images.Charges != 3) return "no three images";
        foreach (Fighter foe in Foes(battle))
        {
            Beside(foe, me, (Foes(battle).IndexOf(foe) - 1) * 10f);
            HoldOn(battle, foe, me);
        }
        List<BattleEvent> seen = Run(battle, 20000, () => me.Hp = me.MaxHp);
        int decoys = seen.Count(e => e.TargetId == me.Id && e.Kind == BattleEventKind.Missed && e.Text?.Contains("decoy") == true);
        int real = seen.Count(e => e.TargetId == me.Id && e.Kind is BattleEventKind.Damaged or BattleEventKind.Dodged or BattleEventKind.Blocked);
        if (decoys != 3) return $"{decoys} images struck, not 3";
        if (!images.Gone) return "the images are still up after three were struck";
        return real > 0 ? null : "no attack ever got through";
    }

    private static string RefractTurnsIt()
    {
        var (battle, me, friend, _) = Scripted(ClassId.Glasswright, HexerFight());
        Fighter hexer = Foes(battle)[0];
        // The ally is held still so they don't run at the hexer and stop its cast; the first hold is shaken off by your Choose It Yourself, so it's put on twice.
        var still = new AuraDefinition { Id = "held-still", Name = "Stunned", Condition = ConditionId.Stunned, DurationMs = 6000 };
        battle.ApplyAura(friend, still, hexer, null, 0);
        battle.ApplyAura(friend, still, hexer, null, 0);
        Use(battle, me, "glasswright-refract", null, 300);
        hexer.Position = friend.Position + new Vector2(friend.Facing * 120f, 0f);
        battle.StartAction(hexer, hexer.Auto, friend, true);
        List<BattleEvent> seen = Run(battle, 3000);
        if (!seen.Any(e => e.ActorId == me.Id && e.Kind == BattleEventKind.Bark && e.Text?.Contains("turns it aside") == true)) return "nothing was turned";
        BattleEvent landed = seen.FirstOrDefault(e => e.ActorId == hexer.Id && Outcome(e));
        if (landed is null) return "the turned bolt never landed";
        return battle.Find(landed.TargetId)?.Side == Side.Foes ? null : $"the bolt still hit {landed.TargetId}";
    }

    private static string SanctuaryHolds()
    {
        var (battle, me, friend, _) = Scripted(ClassId.Lamplighter);
        List<Fighter> foes = Foes(battle);
        foreach (Fighter foe in foes)
        {
            Beside(foe, friend, (foes.IndexOf(foe) - 1) * 10f);
            foe.Target = friend;
        }
        Use(battle, me, "lamplighter-sanctuary", friend, 500);
        Aura sanctuary = Live(friend, "sanctuary");
        if (sanctuary is null) return "no Sanctuary on the ally";
        int ends = sanctuary.EndsAtMs;
        var starts = new List<BattleEvent>();
        while (battle.NowMs < ends - 100) starts.AddRange(battle.Step().Where(e => e.Kind == BattleEventKind.ActionStarted && e.TargetId == friend.Id));
        int rolled = foes.Count(f => f.SanctuarySaves.ContainsKey(sanctuary));
        if (rolled == 0) return "no foe ever had to save";
        foreach (Fighter foe in foes)
            if (foe.SanctuarySaves.TryGetValue(sanctuary, out bool made) && !made && starts.Any(e => e.ActorId == foe.Id))
                return $"{foe.Id} failed its save and went at them anyway";
        return null;
    }

    private static string RevealDispels()
    {
        var (battle, me, _, _) = Scripted(ClassId.Lamplighter);
        List<Fighter> foes = Foes(battle);
        AuraDefinition images = SkillCatalog.Get("glasswright-mirror-image").Effects[0].Aura;
        battle.ApplyAura(foes[0], images, foes[0], null, 0);
        foes[1].VanishUntilMs = battle.NowMs + HoldMs;
        List<BattleEvent> seen = Use(battle, me, "lamplighter-reveal", null, 1500);
        if (Live(foes[0], "mirror-image") is not null) return "the foe's images are still up";
        if (foes[1].Vanished) return "the hidden foe is still hidden";
        int marked = seen.Where(e => e.Kind == BattleEventKind.AuraApplied && e.AuraId == "revealed").Select(e => e.TargetId).Distinct().Count();
        return marked == foes.Count ? null : $"{marked} of {foes.Count} foes Marked";
    }

    private static string RewindRestores()
    {
        var (battle, me, friend, _) = Scripted(ClassId.Hourwarden);
        Stage(battle, me, friend, null, SkillCatalog.Get("hourwarden-rewind"));
        int before = friend.Hp;
        List<BattleEvent> seen = Use(battle, me, "hourwarden-rewind", friend, 600);
        int back = seen.Where(e => e.Kind == BattleEventKind.Healed && e.SkillId == "hourwarden-rewind" && e.TargetId == friend.Id).Sum(e => e.Amount);
        return before + back >= friend.MaxHp * 9 / 10 ? null : $"from {before} back only {back} of {friend.MaxHp}";
    }

    private static string BleedTheClockStops()
    {
        var (battle, me, _, _) = Scripted(ClassId.Hourwarden);
        Use(battle, me, "hourwarden-bleed-the-clock", null, 1200);
        List<Fighter> foes = Foes(battle);
        List<Fighter> frozen = foes.Where(f => Live(f, "clock-stopped") is not null).ToList();
        int slowed = foes.Count(f => Live(f, "clock-slowed") is not null);
        if (frozen.Count + slowed != foes.Count) return $"{frozen.Count} frozen and {slowed} slowed of {foes.Count}";
        List<BattleEvent> seen = Run(battle, 2000);
        Fighter acted = frozen.FirstOrDefault(f => seen.Any(e => e.ActorId == f.Id && e.Kind == BattleEventKind.ActionStarted));
        return acted is null ? null : $"{acted.Id} acted while Frozen";
    }

    private static string SomebodySaysNo()
    {
        var (battle, me, _, foe) = Scripted(ClassId.Crownsworn);
        SkillDefinition chop = SkillCatalog.Get("foe-overhead-chop");
        Beside(foe, me);
        battle.StartAction(foe, chop, me, false);
        List<BattleEvent> seen = Run(battle, 600);
        if (!seen.Any(e => e.SkillId == "crownsworn-somebody-allowed-to-say-no" && e.Kind == BattleEventKind.ReactionFired)) return "nobody said no";
        if (!seen.Any(e => e.Kind == BattleEventKind.CastInterrupted && e.ActorId == foe.Id)) return "the chop went on";
        Fighter other = Foes(battle).First(f => f != foe);
        Beside(other, me, 10f);
        battle.StartAction(other, chop, me, false);
        seen = Run(battle, 600);
        return seen.Any(e => e.SkillId == "crownsworn-somebody-allowed-to-say-no") ? "said no twice in a fight" : null;
    }

    private static string StayingBehind()
    {
        var (battle, me, _, _) = Scripted(ClassId.Bulwark);
        foreach (Fighter foe in Foes(battle))
        {
            Beside(foe, me, (Foes(battle).IndexOf(foe) - 1) * 10f);
            HoldOn(battle, foe, me);
        }
        me.Hp = 1;
        int until = battle.NowMs + 15000;
        bool kept = false;
        while (battle.NowMs < until && !me.IsDown)
            foreach (BattleEvent e in battle.Step())
                if (e.Kind == BattleEventKind.Damaged && e.TargetId == me.Id && !kept)
                {
                    if (me.IsDown) return "went down on the first blow";
                    if (me.Hp != 1) return $"on {me.Hp} hit points after the blow, not 1";
                    kept = true;
                }
        if (!kept) return "never hit";
        return me.IsDown ? null : "never went down, though it's once a fight";
    }

    private static string EchoStrikeEchoes()
    {
        var (battle, me, _, _) = Scripted(ClassId.Hourwarden, aiDrivesYou: true);
        List<BattleEvent> seen = Run(battle, 45000, () => me.Hp = Math.Max(me.Hp, me.MaxHp / 2));
        int used = seen.Count(e => e.ActorId == me.Id && e.Kind == BattleEventKind.SkillUsed);
        int echoes = seen.Count(e => e.ActorId == me.Id && e.Text?.Contains("echo of itself") == true);
        return echoes > 0 ? null : $"no echo in {used} skills used";
    }
}
