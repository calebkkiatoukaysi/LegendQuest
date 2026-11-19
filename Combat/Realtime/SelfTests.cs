using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// `--simulate selftest`'s checks of the real-time engine: a fight replays exactly from its seed,
/// every skill of every class with a kit (and every foe's move) fires in a small scripted fight and
/// does what its kind says, and no built fight runs into the cap.
/// </summary>
internal static partial class SelfTests
{
    /// <summary>How long a scripted skill gets to fire and land.</summary>
    private const int SkillWaitMs = 9000;

    /// <summary>Runs of each fight for the stall check.</summary>
    private const int StallRuns = 25;

    /// <summary>The scripted fight's foes: three Hollowed footmen, at level 5.</summary>
    private static readonly EncounterDefinition Dummies = new()
    {
        Id = "selftest", Name = "Self-test", ExpectedLevel = 5,
        Foes = new[]
        {
            new EncounterFoe("hollowed-footman", null, Slot.Of(Side.Foes, 1)), new EncounterFoe("hollowed-footman", null, Slot.Of(Side.Foes, 2)),
            new EncounterFoe("hollowed-footman", null, Slot.Of(Side.Foes, 3)),
        },
    };

    /// <summary>The same fight from the same seed gives the same events, to the last field; a different seed gives a different fight.</summary>
    public static bool Determinism()
    {
        Console.WriteLine();
        Console.WriteLine("Determinism: every fight twice from the same seed, and once from another.");
        bool passed = true;
        foreach (EncounterDefinition encounter in EncounterCatalog.All)
        {
            ulong seed = BattleSimulator.RunSeed(encounter, 7, false);
            Battle a = BattleSimulator.Fight(encounter, BattleSimulator.Party(encounter), Difficulty.Balanced, false, seed, out ulong first);
            BattleSimulator.Fight(encounter, BattleSimulator.Party(encounter), Difficulty.Balanced, false, seed, out ulong second);
            BattleSimulator.Fight(encounter, BattleSimulator.Party(encounter), Difficulty.Balanced, false, seed + 1, out ulong other);
            bool same = first == second, differs = first != other;
            passed &= same && differs;
            Console.WriteLine($"  {encounter.Id,-24} {a.NowMs / 1000.0,6:F1} s  {first:x16} {(same ? "same" : "DIFFERENT")}, other seed {(differs ? "differs" : "THE SAME")}  {(same && differs ? "ok" : "FAIL")}");
        }
        return passed;
    }

    /// <summary>Every skill of every class with a kit, used in a small fight at level 5 and rank 10: it fires without an exception and does what its kind says.</summary>
    public static bool EverySkill()
    {
        Console.WriteLine();
        Console.WriteLine("Every skill: each class's seven at level 5, rank 10, against three footmen, with a hurt ally beside them.");
        bool passed = true;
        int count = 0;
        foreach (ClassId cls in SkillCatalog.KittedClasses)
        {
            ClassKit kit = SkillCatalog.Kit(cls);
            var results = new List<string>();
            foreach (SkillDefinition skill in kit.All)
            {
                count++;
                string problem;
                try
                {
                    problem = TrySkill(cls, skill, out List<string> extra);
                    results.AddRange(extra);
                }
                catch (Exception fault)
                {
                    problem = $"threw {fault.GetType().Name}: {fault.Message}";
                }
                if (problem is null) results.Add(skill.Id + " ok");
                else
                {
                    passed = false;
                    results.Add($"{skill.Id} FAIL ({problem})");
                }
            }
            Console.WriteLine($"  {ClassCatalog.Get(cls).Name,-12} {string.Join("; ", results)}");
        }
        Console.WriteLine($"  {count} skills, {(passed ? "all ok" : "some FAILED")}.");
        return passed;
    }

    /// <summary>`--simulate skill:&lt;id&gt;`: the scripted fight the every-skill check uses for one class skill, printed an event a line, and what the check made of it.</summary>
    /// <returns>Whether the check passed.</returns>
    public static bool TraceSkill(SkillDefinition skill)
    {
        if (skill.Class is not ClassId cls)
        {
            Console.WriteLine($"{skill.Id} isn't a class skill: the foe moves are tried by the self-test's every-foe-move check.");
            return false;
        }
        Console.WriteLine($"Scripted fight for {skill.Id} ({ClassCatalog.Get(cls).Name}, level 5, rank 10).");
        string problem;
        try
        {
            problem = TrySkill(cls, skill, out _, (battle, e) =>
            {
                string line = e.Text ?? $"[{e.Kind}] {e.ActorId} -> {e.TargetId} {e.SkillId} {e.Amount}";
                Console.WriteLine($"{e.AtMs / 1000.0,7:F2}  {line}");
            });
        }
        catch (Exception fault)
        {
            problem = $"threw {fault.GetType().Name}: {fault.Message} {fault.StackTrace}";
        }
        Console.WriteLine(problem is null ? $"{skill.Id} ok" : $"{skill.Id} FAIL ({problem})");
        return problem is null;
    }

    /// <summary>What the check says of a skill that was used but whose effect never came about.</summary>
    private const string Fizzled = "used, but nothing it does happened";

    /// <summary>How many fights a skill gets to show what it does, when it was used and the dice kept it from landing (a rider on the next hit, missed three times running).</summary>
    private const int Tries = 3;

    /// <summary>Uses one skill in a scripted fight and checks it did what it says; null if it did, else what went wrong. A form's own skills are tried too, inside the form.</summary>
    /// <remarks>A skill used whose effect never came about is tried again on a fresh seed, up to Tries fights: the check is that it can, not that the dice let it the first time.</remarks>
    private static string TrySkill(ClassId cls, SkillDefinition skill, out List<string> extra, Action<Battle, BattleEvent> log = null)
    {
        string problem = null;
        extra = new List<string>();
        for (int attempt = 0; attempt < Tries; attempt++)
        {
            problem = TryOnce(cls, skill, extra, log, attempt);
            if (problem is null || !problem.StartsWith(Fizzled, StringComparison.Ordinal)) return problem;
            extra.Clear();
        }
        return problem;
    }

    /// <summary>One try of TrySkill, in a scripted fight seeded for the attempt.</summary>
    private static string TryOnce(ClassId cls, SkillDefinition skill, List<string> extra, Action<Battle, BattleEvent> log, int attempt)
    {
        var (battle, me, friend, foe) = Scripted(cls, attempt: attempt);

        if (skill.Kind == SkillKind.Passive)
        {
            if (!me.Passives.Contains(skill)) return "not on at rank 10";
            if (skill.Mods.Count + skill.Procs.Count + skill.AllyMods.Count == 0) return "does nothing";
            return null;
        }

        Stage(battle, me, friend, foe, skill);
        string target = skill.Target switch { SkillTarget.Foe => foe.Id, SkillTarget.Ally => friend.Id, _ => null };
        battle.Command(BattleCommand.Target(me.Id, foe.Id));
        if (skill.Kind != SkillKind.Auto) battle.Command(BattleCommand.Use(me.Id, skill.Id, target));

        string problem = Watch(battle, me, skill, log);
        if (problem is not null) return problem;

        foreach (Effect effect in skill.Effects)
        {
            if (effect.Kind != EffectType.Transform) continue;
            FormDefinition form = SkillCatalog.Form(effect.Id);
            if (me.Form != form) return "didn't take the form";
            foreach (string id in form.Skills.Prepend(form.Auto))
            {
                SkillDefinition inner = SkillCatalog.Get(id);
                Fighter near = battle.NearestFoe(me, attackingMe: false);
                if (near is null) break;
                battle.Command(BattleCommand.Target(me.Id, near.Id));
                if (inner.Kind != SkillKind.Auto) battle.Command(BattleCommand.Use(me.Id, inner.Id, near.Id));
                string bad = Watch(battle, me, inner, log);
                extra.Add(inner.Id + (bad is null ? " ok" : $" FAIL ({bad})"));
                if (bad is not null) return $"its form's {inner.Id}: {bad}";
            }
        }
        return null;
    }

    /// <summary>Steps a scripted fight until the skill has fired and something its effects say has happened, or the wait's up.</summary>
    private static string Watch(Battle battle, Fighter me, SkillDefinition skill, Action<Battle, BattleEvent> log = null)
    {
        bool used = skill.Kind == SkillKind.Auto, done = false;
        string denied = null;
        int until = battle.NowMs + SkillWaitMs;
        while (battle.NowMs < until && battle.Outcome == FightOutcome.Ongoing && !done)
        {
            foreach (BattleEvent e in battle.Step())
            {
                log?.Invoke(battle, e);
                if (e.ActorId == me.Id && e.Kind == BattleEventKind.Denied && e.SkillId == skill.Id) denied = e.Text;
                if (e.ActorId == me.Id && e.SkillId == skill.Id && e.Kind is BattleEventKind.SkillUsed) used = true;
                if (used && Shows(battle, skill, skill.Effects, e, me)) done = true;
            }
            if (used && skill.Effects.Any(x => x.Kind == EffectType.Reset && x.On == EffectOn.Self) && me.NextActionFree) done = true;
        }
        if (denied is not null && !used) return "denied: " + denied;
        if (!used) return "never used";
        return done ? null : Fizzled;
    }

    /// <summary>
    /// Sets the scripted fight up for a skill that needs more than the plain start: a foe in front of
    /// you for a skill whose area is around you, and for Rewind an ally who was whole a few seconds
    /// ago and has just been badly hurt.
    /// </summary>
    private static void Stage(Battle battle, Fighter me, Fighter friend, Fighter foe, SkillDefinition skill)
    {
        if (skill.Range == SkillRange.Self && skill.Area is AreaShape.Front or AreaShape.Ring)
        {
            foe.Position = me.Position + new Vector2(me.Facing * (Ranges.Melee * 0.6f + me.Size + foe.Size), 0f);
            foe.Target = me;
        }
        if (skill.Effects.Any(x => x.Kind == EffectType.Rewind))
        {
            for (int at = battle.NowMs; battle.NowMs < at + 7000;)
            {
                friend.Hp = friend.MaxHp;
                battle.Step();
            }
            friend.Hp = Math.Max(1, friend.MaxHp / 4);
        }
    }

    /// <summary>
    /// Whether an event shows one of a skill's effects doing what its kind says: the skill's own hit,
    /// heal, aura or Focus (by its id), a shove of someone, a taunt, the summoned joining, a theft, a
    /// reset ally, an armed reaction; for a random pick (Rummage), any of the picks.
    /// </summary>
    private static bool Shows(Battle battle, SkillDefinition skill, IReadOnlyList<Effect> effects, BattleEvent e, Fighter me)
    {
        if (skill.Kind == SkillKind.Reaction) return e.ActorId == me.Id && e.Kind == BattleEventKind.Armed && e.SkillId == skill.Id;
        if (e.Kind == BattleEventKind.Joined)
            return effects.Any(x => x.Kind == EffectType.Summon) && battle.Find(e.ActorId) is { Summoner: { } caller } && caller == me;
        if (e.ActorId != me.Id) return false;
        bool own = e.SkillId == skill.Id;
        foreach (Effect effect in effects)
        {
            bool shows = effect.Kind switch
            {
                EffectType.Damage or EffectType.Throw => own && e.Kind is BattleEventKind.Damaged or BattleEventKind.Absorbed or BattleEventKind.Missed or BattleEventKind.Dodged or BattleEventKind.Blocked,
                EffectType.Strike => e.Kind is BattleEventKind.Damaged or BattleEventKind.Absorbed or BattleEventKind.Missed or BattleEventKind.Dodged or BattleEventKind.Blocked,
                EffectType.Heal or EffectType.Rewind => own && e.Kind == BattleEventKind.Healed,
                EffectType.TempHp or EffectType.Aura => own && (e.Kind is BattleEventKind.AuraApplied or BattleEventKind.AuraRefreshed || e.Kind == BattleEventKind.Missed && e.Flags.HasFlag(HitFlags.Save)),
                EffectType.Taunt => e.Kind == BattleEventKind.Taunted,
                EffectType.Transform => e.Kind == BattleEventKind.Transformed,
                EffectType.Rider => own && e.Kind == BattleEventKind.ReactionFired,
                EffectType.Teleport or EffectType.Dash or EffectType.Swap or EffectType.Charge => e.Kind == BattleEventKind.Moved && e.TargetId is not null,
                EffectType.Pull => e.Kind == BattleEventKind.Pulled,
                EffectType.Knockback => e.Kind == BattleEventKind.KnockedBack,
                EffectType.Focus => own && e.Kind == BattleEventKind.FocusChanged,
                EffectType.Gold => own && e.Kind == BattleEventKind.Stole,
                EffectType.Vanish => e.Kind == BattleEventKind.Vanished,
                EffectType.Reset => own && e.Kind == BattleEventKind.Bark,
                EffectType.OneOf => Shows(battle, skill, effect.Rider, e, me),
                _ => false,
            };
            if (shows) return true;
        }
        return false;
    }

    /// <summary>A scripted fight: the class at level 5 and rank 10 (as "you", driven by nobody), a hurt Soul-Bound beside them, three footmen; everyone with full Focus.</summary>
    /// <param name="cls">Your class.</param>
    /// <param name="encounter">The foes, if not the three footmen.</param>
    /// <param name="outdoors">Whether it's fought outdoors.</param>
    /// <param name="aiDrivesYou">Whether the party's AI plays you too, rather than nobody.</param>
    /// <param name="bagExtra">An item to put in the bag beside the potions, or null.</param>
    /// <param name="attempt">Which try this is, for a fresh seed: 0 the first.</param>
    private static (Battle Battle, Fighter Me, Fighter Friend, Fighter Foe) Scripted(ClassId cls, EncounterDefinition encounter = null, bool outdoors = true,
        bool aiDrivesYou = false, string bagExtra = null, int attempt = 0)
    {
        encounter ??= Dummies;
        PartyMember tester = Origins.Create(Origins.You);
        tester.Name = "Tester";
        tester.Class = cls;
        tester.Scores = ClassCatalog.Get(cls).Recommended;
        tester.Equipment.Clear();
        tester.PutOn(Origins.KitFor(Origins.You, cls));
        PartyMember friend = Origins.Create(Origins.SoulBound);
        friend.Name = "Friend";
        foreach (PartyMember member in new[] { tester, friend })
        {
            Leveling.SetLevel(member, 5);
            member.ClassPoints[member.Class] = ClassRanks.PointsFor(ClassRanks.MaxRank);
            member.Hp = member.MaxHp;
        }

        var bag = new Inventory(new List<ItemStack>());
        bag.Add(ItemCatalog.HealingPotion, 3);
        if (bagExtra is not null) bag.Add(bagExtra);
        var rules = new BattleRules(Difficulty.Balanced, null, aiDrivesYou ? null : Origins.You, bag) { Outdoors = outdoors };
        var battle = new Battle(encounter, BattleRoster.Build(encounter, new[] { tester, friend }), rules, 99UL + (ulong)cls + 1000UL * (ulong)attempt);
        Fighter me = battle.Find(Origins.You), mate = battle.Find(Origins.SoulBound);
        me.FocusMilli = Fighter.MaxFocus * 1000;
        me.Hp = me.MaxHp * 3 / 5;
        mate.Hp = mate.MaxHp / 2;
        Fighter foe = battle.NearestFoe(me, attackingMe: false);
        return (battle, me, mate, foe);
    }

    /// <summary>Every foe's move, forced in a small fight against the party: it fires and lands (or is saved against, dodged or blocked).</summary>
    public static bool EveryFoeMove()
    {
        Console.WriteLine();
        Console.WriteLine("Every foe move: each stat block's moves (its phases' too), forced on the nearest of the party.");
        bool passed = true;
        var results = new List<string>();
        foreach (EnemyDefinition enemy in EnemyCatalog.All)
        {
            var moves = new List<string>(enemy.Skills);
            foreach (FoePhase phase in enemy.Phases) moves.AddRange(phase.Adds);
            if (enemy.AutoSkill is not null) moves.Add(enemy.AutoSkill);
            foreach (string id in moves.Distinct())
            {
                string problem;
                try
                {
                    problem = TryFoeMove(enemy, SkillCatalog.Get(id));
                }
                catch (Exception fault)
                {
                    problem = $"threw {fault.GetType().Name}: {fault.Message}";
                }
                passed &= problem is null;
                results.Add($"{enemy.Id}/{id} {(problem is null ? "ok" : "FAIL (" + problem + ")")}");
            }
        }
        foreach (string line in results) Console.WriteLine("  " + line);
        return passed;
    }

    /// <summary>Forces one foe move and checks it landed.</summary>
    private static string TryFoeMove(EnemyDefinition enemy, SkillDefinition skill)
    {
        var encounter = new EncounterDefinition { Id = "selftest-" + enemy.Id, Name = enemy.Name, ExpectedLevel = 3, Foes = new[] { new EncounterFoe(enemy.Id, null, Slot.Of(Side.Foes, 1)) } };
        var (battle, me, _, _) = Scripted(ClassId.Fighter, encounter);
        Fighter foe = battle.Fighters.First(f => f.Side == Side.Foes);
        foe.Position = me.Position + new Vector2(skill.Range is SkillRange.Short or SkillRange.Long ? 70f : 24f + foe.Size, 0f);
        foe.Target = me;
        battle.StartAction(foe, skill, skill.Target == SkillTarget.Foe ? me : skill.Target == SkillTarget.Self ? foe : me, skill.Kind == SkillKind.Auto);

        int until = battle.NowMs + SkillWaitMs;
        while (battle.NowMs < until && battle.Outcome == FightOutcome.Ongoing)
            foreach (BattleEvent e in battle.Step())
                if (e.ActorId == foe.Id && e.Kind is BattleEventKind.Damaged or BattleEventKind.Absorbed or BattleEventKind.Missed or BattleEventKind.Dodged
                        or BattleEventKind.Blocked or BattleEventKind.AuraApplied or BattleEventKind.Pulled or BattleEventKind.Vanished or BattleEventKind.Moved
                    && (e.SkillId == skill.Id || e.Kind is BattleEventKind.Vanished or BattleEventKind.Moved or BattleEventKind.Pulled))
                    return null;
        return "nothing landed";
    }

    /// <summary>Every built fight, many times: none runs into the cap.</summary>
    public static bool NoStalls()
    {
        Console.WriteLine();
        Console.WriteLine($"No stalls: every fight {StallRuns} times on each difficulty.");
        bool passed = true;
        foreach (EncounterDefinition encounter in EncounterCatalog.All)
        {
            var line = new List<string>();
            foreach (Difficulty difficulty in Enum.GetValues<Difficulty>())
            {
                int unfinished = 0, longest = 0;
                for (int run = 0; run < StallRuns; run++)
                {
                    Battle battle = BattleSimulator.Fight(encounter, BattleSimulator.Party(encounter), difficulty, false, BattleSimulator.RunSeed(encounter, 1000 + run, false), out _);
                    if (battle.Outcome == FightOutcome.Ongoing) unfinished++;
                    longest = Math.Max(longest, battle.NowMs);
                }
                passed &= unfinished == 0;
                line.Add($"{difficulty} {(unfinished == 0 ? "ok" : unfinished + " STALLED")} (longest {longest / 1000.0:F0} s)");
            }
            Console.WriteLine($"  {encounter.Id,-24} {string.Join(", ", line)}");
        }
        return passed;
    }
}
