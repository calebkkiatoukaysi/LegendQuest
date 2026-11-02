using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Dialogue;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.Items;

namespace LegendQuest.Combat;

/// <summary>
/// Every fight in the story so far, by STORY.md's encounter id.
/// </summary>
/// <remarks>
/// The foes on a map already are its npcs (Content/Maps): the wraiths over the Gull's Wager's rail
/// once the bell's rung, the Red Gulls on the docks, the wolves at the Mirewood's edge, the
/// Thornmother in her den, the Hollowed at Thornbridge's treeline, Kael on his shelf and the sappers
/// beside him. Everyone else turns up on a tile picked off the map: the guard wave down the quarry's
/// passage, the second raid out of Thornbridge's trees, and the sapper chief up out of the hole.
/// Each fight is started by the conversation before it (DialogueEffect.Fight), and winning it sets
/// its flag, the one the story already reads, so the map changes, and then plays the conversation
/// after it (Then), which is the rest of the scene. None of them can be lost and carried on from: a
/// loss is the game over screen.
/// </remarks>
public static class EncounterCatalog
{
    /// <summary>The share of his hit points Kael calls the duel off at: STORY.md's 30%.</summary>
    private const float KaelYields = 0.3f;

    private static readonly EncounterDefinition[] Table =
    {
        // P-05: the tutorial. Three wraiths up over the port rail while the duo are at the bow: one
        // close enough to walk up to and hit on the first turn, the other two further aft and coming,
        // so there's one at a time to learn on and to fill the Soul-Link against.
        new()
        {
            Id = "p-05-wraiths", Name = "Sea-wraiths", Tutorial = true, Victory = Prologue.WraithsBeaten, Then = "p-05-wraiths-after",
            Foes = new[] { Npc("sea-wraith", "wraith-1"), Npc("sea-wraith", "wraith-2"), Npc("sea-wraith", "wraith-3") },
        },

        // A1-C1-01: the three thugs and Grell, round Tomas on the docks. A brawl with cudgels, so
        // nobody dies in it: Grell's got to be on his knees after, begging, and nobody's soul-bound
        // friend is beaten to death in their first hour ashore.
        new()
        {
            Id = "a1-c1-01-gulls", Name = "The Red Gulls", Victory = Act1Chapter1.GullsBeaten, Then = "a1-c1-01-gulls-after", Nonlethal = true,
            Foes = new[] { Npc("red-gull-thug", "gull-1"), Npc("red-gull-thug", "gull-2"), Npc("red-gull-thug", "gull-3"), Npc("bosun-grell", "grell") },
        },

        // A1-C2-01: the pack at the Mirewood's edge. Sera's watching from the tree line, and her arrow
        // is for the one that gets up again, after.
        new()
        {
            Id = "a1-c2-01-gloomfangs", Name = "Gloomfang Wolves", Victory = Act1Chapter2.GloomfangsBeaten, Then = "a1-c2-01-gloomfangs-after",
            Foes = new[] { Npc("gloomfang", "wolf-1"), Npc("gloomfang", "wolf-2"), Npc("gloomfang", "wolf-3"), Npc("gloomfang", "wolf-4") },
        },

        // A1-C2-02: the Thornmother in her den. The crystal's choice comes after, and one way through
        // it is the guard wave.
        new()
        {
            Id = "a1-c2-02-thornmother", Name = "The Thornmother", Boss = true, Music = Track.Boss,
            Victory = Act1Chapter2.ThornmotherBeaten, Then = "a1-c2-02-thornmother-after",
            Foes = new[] { Npc("thornmother", "thornmother") },
        },

        // A1-C2-02, the compromise: the two of you hold the den while Sera draws the rot out of the
        // crystal (so she sits it out), and what's left of the quarry's corrupted comes down the
        // passage at you, a few at a time, the way STORY.md's guard wave does: the pack, then the
        // courier's dead escort, then a straggler.
        new()
        {
            Id = "a1-c2-02-guard-wave", Name = "Guarding the Crystal", Victory = Act1Chapter2.CrystalGuarded, Then = "a1-c2-02-guard-wave-after",
            SitsOut = new[] { "sera" },
            Foes = new[] { Spawn("gloomfang", 15, 9), Spawn("gloomfang", 15, 11), Spawn("gloomfang", 16, 12) },
            Waves = new[]
            {
                new EncounterWave(2, new[] { Spawn("hollowed-footman", 15, 4) }, null),
                new EncounterWave(3, new[] { Spawn("gloomfang", 12, 4) }, null),
            },
        },

        // A1-C3-02: the Hollowed out of the treeline at Thornbridge, more of them behind, and Mira
        // fighting beside you before she's joined.
        new()
        {
            Id = "a1-c3-02-raid", Name = "The Raid on Thornbridge", Victory = Act1Chapter3.RaidBeaten, Then = "a1-c3-02-raid-after",
            Foes = new[]
            {
                Npc("hollowed-footman", "hollowed-1"), Npc("hollowed-footman", "hollowed-2"), Npc("hollowed-footman", "hollowed-3"),
                Npc("hollowed-hexer", "hollowed-4"),
            },
            Waves = new[]
            {
                new EncounterWave(3, new[] { Spawn("hollowed-footman", 38, 12), Spawn("hollowed-hexer", 39, 13), Spawn("hollowed-footman", 38, 14) },
                    "Where's the Watch?! Where's the king's army?!"),
            },
            Guests = new[] { "mira" },
        },

        // A1-C4-02: Kael on his shelf. Nobody's meant to die; he calls it off when he's hurt enough,
        // which brings the sappers up through the floor, and the truce leads straight into them.
        new()
        {
            Id = "a1-c4-02-kael", Name = "Kael", Victory = Act1Chapter4.KaelTruce, Then = "a1-c4-02-kael-after", YieldsAt = KaelYields, Nonlethal = true,
            Foes = new[] { Npc("kael-duel", "kael") },
        },

        // A1-C4-02, after the truce: the sappers up through the floor, their chief behind them, and
        // Kael at your shoulder.
        new()
        {
            Id = "a1-c4-02-sappers", Name = "The Sappers", LootTable = "old-crown-vault", Victory = Act1Chapter4.SappersBeaten, Then = "a1-c4-02-sappers-after",
            Foes = new[]
            {
                Npc("hollowed-sapper", "sapper-1"), Npc("hollowed-sapper", "sapper-2"), Npc("hollowed-sapper", "sapper-3"),
                Spawn("sapper-chief", 33, 9),
            },
            Guests = new[] { "kael" },
        },
    };

    private static readonly Dictionary<string, EncounterDefinition> ById = new();

    /// <summary>Indexes the table and checks every encounter.</summary>
    /// <remarks>Throws on purpose, so a fight that names a foe or a guest who isn't there, or that can't start, shows up when the game starts.</remarks>
    static EncounterCatalog()
    {
        foreach (EncounterDefinition row in Table)
        {
            Check(row);
            if (!ById.TryAdd(row.Id, row)) throw new InvalidOperationException($"Two encounters are called {row.Id}.");
        }
    }

    /// <summary>Every encounter, in the story's order.</summary>
    public static IEnumerable<EncounterDefinition> All => Table;

    /// <summary>Whether there's an encounter with this id.</summary>
    /// <param name="id">Which.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One encounter.</summary>
    /// <param name="id">Which. It has to exist.</param>
    public static EncounterDefinition Get(string id) => ById[id];

    /// <summary>A foe who's an npc on the map already.</summary>
    private static EncounterFoe Npc(string enemy, string npc) => new(enemy, npc, null);

    /// <summary>A foe who turns up on a tile.</summary>
    private static EncounterFoe Spawn(string enemy, int x, int y) => new(enemy, null, new Point(x, y));

    /// <summary>Checks one encounter, and throws with what's wrong if it can't be fought.</summary>
    private static void Check(EncounterDefinition row)
    {
        string name = $"Encounter {row.Id}";
        if (string.IsNullOrWhiteSpace(row.Name)) throw new InvalidOperationException($"{name} has no name.");
        if (row.Foes.Count == 0) throw new InvalidOperationException($"{name} has nobody to fight.");
        if (!Enum.IsDefined(row.Music)) throw new InvalidOperationException($"{name} plays a song that isn't a Track.");
        if (row.Boss && row.Tutorial) throw new InvalidOperationException($"{name} can't be the tutorial and a boss.");
        if (row.LootTable is not null && !LootTableCatalog.Exists(row.LootTable))
            throw new InvalidOperationException($"{name} is won with loot table {row.LootTable}, which isn't in LootTableCatalog.");
        if (row.YieldsAt is < 0f or >= 1f) throw new InvalidOperationException($"{name} has foes yielding at {row.YieldsAt:P0} of their hit points.");
        if (row.Then is not null && DialogueCatalog.Get(row.Then) is null) throw new InvalidOperationException($"{name} goes on to conversation {row.Then}, which isn't in DialogueCatalog.");
        if (row.Lost is not null && DialogueCatalog.Get(row.Lost) is null) throw new InvalidOperationException($"{name} is lost into conversation {row.Lost}, which isn't in DialogueCatalog.");

        var npcs = new HashSet<string>();
        foreach (EncounterFoe foe in row.Foes)
        {
            CheckFoe(name, foe);
            if (foe.Npc is not null && !npcs.Add(foe.Npc)) throw new InvalidOperationException($"{name} has npc {foe.Npc} in it twice.");
        }

        int round = 1;
        foreach (EncounterWave wave in row.Waves)
        {
            if (wave.Round <= round) throw new InvalidOperationException($"{name} has a wave on round {wave.Round}: waves come from round 2, in order.");
            if (wave.Foes.Count == 0) throw new InvalidOperationException($"{name} has an empty wave on round {wave.Round}.");
            foreach (EncounterFoe foe in wave.Foes)
            {
                CheckFoe(name, foe);
                if (foe.Npc is not null) throw new InvalidOperationException($"{name}'s wave on round {wave.Round} names npc {foe.Npc}: a wave's foes turn up on tiles.");
            }
            round = wave.Round;
        }

        foreach (string guest in row.Guests)
            if (!Origins.Exists(guest) && !EnemyCatalog.Exists(guest)) throw new InvalidOperationException($"{name}'s guest {guest} is neither an origin nor an enemy.");

        foreach (string member in row.SitsOut)
            if (!Origins.Exists(member)) throw new InvalidOperationException($"{name} sits out {member}, who isn't an origin.");
    }

    /// <summary>Checks one foe line: its stat block exists, and it's an npc or a tile, not both or neither.</summary>
    private static void CheckFoe(string name, EncounterFoe foe)
    {
        if (!EnemyCatalog.Exists(foe.Enemy)) throw new InvalidOperationException($"{name} has a {foe.Enemy}, which isn't in EnemyCatalog.");
        if ((foe.Npc is null) == (foe.Cell is null)) throw new InvalidOperationException($"{name}'s {foe.Enemy} needs an npc or a tile, and only one.");
    }
}
