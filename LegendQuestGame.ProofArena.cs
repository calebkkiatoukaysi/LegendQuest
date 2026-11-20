using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.Items;
using LegendQuest.Save;
using LegendQuest.Screens;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest;

/// <summary>
/// The proof run's fight stages, each asked for by name and run on its own: Arena (story fights from
/// the gulls to the sappers, and the fields), Boss (the Thornmother through her three phases) and
/// Classes (seven classes in your place in the raid on Thornbridge, each pressing its skills).
/// </summary>
/// <remarks>
/// <para>
/// <c>--proof &lt;dir&gt; Arena</c> (or Boss, or Classes) starts at that stage rather than the menus.
/// Each bout is a fresh game made the way the story has it at that fight: you, the Soul-Bound and the
/// companions who've joined by then, at LevelTable's level and the class rank the story's fights
/// before it give (ClassRanks.ExpectedRank, as the simulator has it), in the room the fight's fought
/// in, standing where the story leaves you, with the story's flags up to it. The save is made as the
/// fight starts, so the room starts it as it loads, the way loading a save made mid-fight does; a
/// field bout instead walks at an aggressive field until it comes at the party, or points at a
/// sleeping construct and clicks it. The party's AI fights for everyone (ExploreScreen.ProofArena
/// says what each bout films).
/// </para>
/// <para>
/// The shots go in arena/, boss/, classes/ and field/, the frame runs in seq/. A bout that's lost is
/// fought again on the next seed, at most ArenaTries times, with a line on the error stream each
/// time; one that runs past ArenaLongest frames is given up on with a line, so the run never hangs.
/// </para>
/// </remarks>
public partial class LegendQuestGame
{
    /// <summary>An environment variable that, when it's set, keeps only the bouts whose shots' paths have it in them ("field/", "sappers"), to look at a few again without the rest.</summary>
    private const string ArenaOnly = "LQ_ARENA_ONLY";

    /// <summary>How many times a bout's fought before the run gives up on it, and the longest it runs, in drawn frames (three minutes).</summary>
    private const int ArenaTries = 3, ArenaLongest = 10800;

    /// <summary>One bout: the fight, the room and where you stand in it (canvas pixels, as the zone files have it), the fight whose party it borrows, what it films, and you as another class at a rank, if so.</summary>
    private sealed record ArenaBout(string Fight, string Zone, string Room, Vector2 Feet, string PartyAs, ArenaPlan Plan, string[] Flags,
        ClassId? YouAs = null, int Rank = 0);

    /// <summary>The bouts of the stage that's running, which one it's on, its screen, how many times it's been fought, and its clock.</summary>
    private List<ArenaBout> _bouts;
    private int _bout = -1;
    private ExploreScreen _arenaScreen;
    private int _arenaTries;
    private int _arenaFrames;

    /// <summary>Starts the run at Arena, Boss or Classes when that's the part asked for.</summary>
    private void BeginArenaProof()
    {
        if (ProofLast is ProofStage.Arena or ProofStage.Boss or ProofStage.Classes) _proofStage = ProofLast;
    }

    /// <summary>One frame of a fight stage: the next bout put up when the last is done, a lost one fought again, and the bout's step.</summary>
    private string ProofArena()
    {
        _bouts ??= Bouts(_proofStage).Where(each => Environment.GetEnvironmentVariable(ArenaOnly) is not string only || each.Plan.Prefix.Contains(only)).ToList();

        if (Find<GameOverScreen>() is not null)
        {
            Console.Error.WriteLine($"proof: {_bouts[_bout].Plan.Prefix} ({_bouts[_bout].Fight}) was lost on seed {ExploreScreen.ProofSeed}.");
            if (++_arenaTries < ArenaTries) StartBout();
            else
            {
                ClearScreens();
                NextBout($"lost {ArenaTries} times; going on without it");
            }
            return null;
        }

        if (_arenaScreen is null)
        {
            if (++_bout >= _bouts.Count)
            {
                ClearScreens();
                _proofStage = ProofStage.Done;
                return null;
            }
            _arenaTries = 0;
            StartBout();
            return null;
        }

        if (!_arenaScreen.IsActive && _arenaFrames == 0) return null;
        string shot = _arenaScreen.ArenaStep(_bouts[_bout].Plan);
        if (_arenaScreen.ArenaDone) NextBout(null);
        else if (++_arenaFrames > ArenaLongest) NextBout($"ran past {ArenaLongest} frames; going on without the rest of it");
        return shot;
    }

    /// <summary>Moves on to the next bout, with a line on the error stream about this one if there's something to say.</summary>
    private void NextBout(string why)
    {
        if (why is not null) Console.Error.WriteLine($"proof: {_bouts[_bout].Plan.Prefix} ({_bouts[_bout].Fight}) {why}.");
        _arenaScreen = null;
    }

    /// <summary>Takes every screen off.</summary>
    private void ClearScreens()
    {
        foreach (GameScreen screen in _screenManager.GetScreens()) _screenManager.RemoveScreen(screen);
    }

    /// <summary>Puts the bout up: every screen off, a fresh game made for it, and its room on, on the bout's seed.</summary>
    private void StartBout()
    {
        ClearScreens();
        ArenaBout bout = _bouts[_bout];
        ExploreScreen.ProofSeed = ProofCombatSeed + _arenaTries;
        _arenaScreen = new ExploreScreen(ArenaGame(bout));
        _arenaFrames = 0;
        _screenManager.AddScreen(_arenaScreen);
    }

    /// <summary>A fresh game for a bout: the party the story has by then, at its level and rank, in the bout's room with the story's flags, the bag's two potions, and the fight under way if the save starts it.</summary>
    private static SaveData ArenaGame(ArenaBout bout)
    {
        List<PartyMember> party = BattleSimulator.Party(EncounterCatalog.Get(bout.PartyAs ?? bout.Fight), bout.YouAs);
        PartyMember you = party.First(each => each.Id == Origins.You);
        PartyMember soulBound = party.First(each => each.Id == Origins.SoulBound);
        you.Name = "Caleb";
        you.Look = new CharacterLook
        {
            Skin = 2, Face = 2, Eyes = 2, Hair = 2, HairColour = 3, Shirt = 1, Vest = 3, Legs = 1, LegsColour = 5, Cloak = 2, Portrait = "face-m03",
            BaseTint = 12, TrimTint = 5, HairPart = "hair-17", Extras = new Dictionary<string, string> { ["beard"] = "beard-01" },
        };
        soulBound.Name = "Wren";
        soulBound.Look = new CharacterLook
        {
            Ancestry = Ancestry.Sylvan, Skin = 1, Face = 2, Eyes = 4, Hair = 4, HairColour = 4, Shirt = 2, Vest = 1, LegsColour = 2, Cloak = 3, Portrait = "face-f05",
            HairTint = 12, EyeTint = 8, BaseTint = 7, TrimTint = 17,
        };
        if (bout.Rank > 0) you.ClassPoints[you.Class] = ClassRanks.PointsFor(bout.Rank);

        var game = new SaveData
        {
            ZoneId = bout.Zone, RoomId = bout.Room, Party = party,
            Fight = bout.Plan.Start == ArenaStart.Saved ? bout.Fight : null,
        };
        game.LeaderMember.Feet = bout.Feet / ZoneCatalog.RoomScale;
        game.LeaderMember.Facing = Direction.Right;
        game.Visited.Add(bout.Zone + "/" + bout.Room);
        game.Flags.UnionWith(new[] { Prologue.Done, ExploreScreen.Arrived });
        game.Flags.UnionWith(bout.Flags);
        game.Bag.Add(ItemCatalog.HealingPotion, SaveData.StartingPotions);
        return game;
    }

    /// <summary>The bouts of a stage, in order.</summary>
    private static List<ArenaBout> Bouts(ProofStage stage) => stage switch
    {
        ProofStage.Arena => new List<ArenaBout>
        {
            // The story's fights, each in its own room with the party it has by then; the gulls' melee filmed
            // from Grell's first Hook Pull, to see how a crowd stands round its targets, and the raid's Hex.
            new("a1-c1-01-gulls", "docks", "harbourside", new Vector2(700, 770), null, new ArenaPlan { Prefix = "arena/gulls", FilmMove = "foe-hook-pull" }, Array.Empty<string>()),
            new("a1-c2-01-gloomfangs", "mirewood", "edge", new Vector2(1400, 760), null, new ArenaPlan { Prefix = "arena/gloomfangs" },
                new[] { Act1Chapter1.GullsBeaten }),
            new("a1-c3-02-raid", "thornbridge", "river-edge", new Vector2(620, 740), null,
                new ArenaPlan { Prefix = "arena/raid", FilmFoeSkills = new[] { "foe-hex" } },
                new[] { Act1Chapter1.GullsBeaten, Act1Chapter2.GloomfangsBeaten, Act1Chapter2.ThornmotherBeaten, Act1Chapter3.SuppliesDone }),
            // Kael's duel on his shelf, his Smoke Bomb (from his second phase) and Cunning Disengage filmed.
            new("a1-c4-02-kael", "old-crown-vault", "strongroom", new Vector2(2200, 740), null,
                new ArenaPlan { Prefix = "arena/kael", FilmFoeSkills = new[] { "foe-smoke-bomb", "foe-cunning-disengage" } },
                new[] { Act1Chapter1.GullsBeaten, Act1Chapter2.GloomfangsBeaten, Act1Chapter2.ThornmotherBeaten, Act1Chapter3.RaidBeaten }),
            new("a1-c4-02-sappers", "old-crown-vault", "strongroom", new Vector2(2200, 740), null, new ArenaPlan { Prefix = "arena/sappers" },
                new[] { Act1Chapter1.GullsBeaten, Act1Chapter2.GloomfangsBeaten, Act1Chapter2.ThornmotherBeaten, Act1Chapter3.RaidBeaten, Act1Chapter4.KaelTruce }),

            // The fields: the Mirewood's pack coming at the party on the path, back a minute after
            // it's beaten; and a sleeping construct in the vault, pointed at and clicked.
            new("rnd-mirewood-gloomfangs", "mirewood", "deep", new Vector2(520, 770), "a1-c2-02-thornmother",
                new ArenaPlan { Prefix = "field/wolves", Start = ArenaStart.Walk, WaitForRespawn = true },
                new[] { Act1Chapter1.GullsBeaten, Act1Chapter2.GloomfangsBeaten, Act1Chapter2.EdgeDone }),
            new("rnd-vault-construct-1", "old-crown-vault", "entrance", new Vector2(1500, 730), "a1-c4-02-sappers",
                new ArenaPlan { Prefix = "field/construct", Start = ArenaStart.Click, ClickAt = "construct-1", SheetShot = true },
                new[] { Act1Chapter1.GullsBeaten, Act1Chapter2.GloomfangsBeaten, Act1Chapter3.RaidBeaten }),
        },

        // The Thornmother in her den, with the Lodge's wardens on the lip (Tamsin was told), through all
        // three phases, her Thorn Burst filmed from its first telegraph.
        ProofStage.Boss => new List<ArenaBout>
        {
            new("a1-c2-02-thornmother", "mirewood-quarry", "floor", new Vector2(1700, 760), null,
                new ArenaPlan { Prefix = "boss/thornmother", FilmMove = "foe-thorn-burst" },
                new[] { Act1Chapter1.GullsBeaten, Act1Chapter2.GloomfangsBeaten, Act1Chapter2.LetterRead, EncounterCatalog.LodgeTold }),
        },

        // Seven classes in your place in the raid on Thornbridge (the longest of Act 1's fights, with the
        // most to hit and Mira and the Watch beside you), at rank 5 so the ultimate's on the bar, each
        // pressing its four skills as soon as each can go; the first two classes' ultimates filmed, and
        // the skills whose moves were made their own (a guard, a breath, a shield rapped), and again on a
        // go with nobody stood over the leader if the first had somebody.
        ProofStage.Classes => new[] { ClassId.Fighter, ClassId.Wizard, ClassId.Cleric, ClassId.Paladin, ClassId.Hemomancer, ClassId.LodgeRanger, ClassId.Warlock }
            .Select((cls, i) => new ArenaBout("a1-c3-02-raid", "thornbridge", "river-edge", new Vector2(620, 740), null,
                new ArenaPlan
                {
                    Prefix = "classes/" + Rules.ClassCatalog.Get(cls).Code, PressSkills = true, FilmUltimate = i < 2 || cls == ClassId.Paladin,
                    FilmSkills = cls switch
                    {
                        ClassId.Fighter => new[] { "fighter-riposte", "fighter-second-wind" },
                        ClassId.Paladin => new[] { "paladin-raise-shield", "paladin-unbroken-oath" },
                        ClassId.Warlock => new[] { "warlock-armour-of-shadows", "warlock-hex" },
                        _ => Array.Empty<string>(),
                    },
                },
                new[] { Act1Chapter1.GullsBeaten, Act1Chapter2.GloomfangsBeaten, Act1Chapter2.ThornmotherBeaten, Act1Chapter3.SuppliesDone }, cls, 5))
            .ToList(),

        _ => new List<ArenaBout>(),
    };
}
