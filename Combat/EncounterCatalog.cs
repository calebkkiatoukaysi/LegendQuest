using System;
using System.Collections.Generic;
using LegendQuest.Characters;
using LegendQuest.Dialogue;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.Items;
using LegendQuest.Rules;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Combat;

/// <summary>
/// Every fight in the story so far, by the story's encounter id (Story/09-registry.md lists them all).
/// </summary>
/// <remarks>
/// The foes in a room already are its npcs (Content/Zones), and they walk from where the room has
/// them standing to their slots: the wraiths over the Gull's Wager's rail once the bell's rung, the
/// Red Gulls on the harbourside, the wolves at the Mirewood's edge, the Thornmother in her den, the
/// Hollowed at Thornbridge's treeline, Kael on his shelf and the sappers beside him. Everyone else
/// turns up in a slot on the foes' side: the guard wave down the quarry's passage, the second raid
/// out of Thornbridge's trees, and the sapper chief up out of the hole. Slots are 11-combat.md §1's
/// numbers, F(1) to F(4) the foes' front row and B(5) to B(8) its back, lane by lane. The allies are
/// §4.1's: who stands in the party's rows as a guest, and who helps from behind them as a support.
/// Each fight is started by the conversation before it (DialogueEffect.Fight), and winning it sets
/// its flag, the one the story already reads, so the room changes, and then plays the conversation
/// after it (Then), which is the rest of the scene. None of them can be lost and carried on from: a
/// loss is the game over screen.
/// </remarks>
public static class EncounterCatalog
{
    /// <summary>The share of his hit points Kael calls the duel off at: the story's 30%.</summary>
    private const float KaelYields = 0.3f;

    /// <summary>How long the den has to be held while Sera draws the rot out of the crystal, in rounds.</summary>
    private const int CrystalRounds = 5;

    /// <summary>The companions who help before they've joined, by origin id.</summary>
    private const string Sera = "sera";
    private const string Mira = "mira";
    private const string Kael = "kael";

    /// <summary>The flag the Lodge's wardens come on: Tamsin was told the Lodge sent you (A1-C2-01).</summary>
    public const string LodgeTold = "lodge-told";

    private static readonly EncounterDefinition[] Table =
    {
        // P-05: the tutorial. Three wraiths up over the port rail while the duo are at the bow: two in
        // reach of the first swing and one behind them, so there's the front row to learn on and a
        // back one to learn about. Odelle's crew are at the rail with boat-hooks, and it's their ship.
        new()
        {
            Id = "p-05-wraiths", Name = "Sea-wraiths", Kind = EncounterKind.Tutorial, Victory = Prologue.WraithsBeaten, Then = "p-05-wraiths-after",
            Foes = new[] { Npc("sea-wraith", "wraith-1", F(1)), Npc("sea-wraith", "wraith-2", F(2)), Npc("sea-wraith", "wraith-3", B(5)) },
            Supports = new[]
            {
                new EncounterSupport("odelle-crew", "Odelle's crew", "odelle",
                    new SupportAbility("A boat-hook: 1d6 to a wraith", SupportTarget.RandomFoe, Dice("1d6"), DamageType.Piercing)),
            },
        },

        // A1-C1-01: the three thugs and Grell, round Tomas on the docks. A brawl with cudgels, so
        // nobody dies in it: Grell's got to be on his knees after, begging, and nobody's soul-bound
        // friend is beaten to death in their first hour ashore. Grell's at the front of his Gulls, and
        // from the third round Tomas has had enough of them and gets his net out.
        new()
        {
            Id = "a1-c1-01-gulls", Name = "The Red Gulls", Victory = Act1Chapter1.GullsBeaten, Then = "a1-c1-01-gulls-after", Nonlethal = true,
            Foes = new[]
            {
                Npc("red-gull-thug", "gull-1", F(2)), Npc("red-gull-thug", "gull-2", F(3)), Npc("red-gull-thug", "gull-3", F(4)),
                Npc("bosun-grell", "grell", F(1)),
            },
            Supports = new[]
            {
                new EncounterSupport("tomas", "Old Tomas", "tomas",
                    new SupportAbility("A net over one Gull: Restrained a turn", SupportTarget.RandomFoe, DiceExpression.None, DamageType.Bludgeoning,
                        ConditionId.Restrained, 1),
                    FromRound: 3),
            },
        },

        // A1-C2-01: the pack at the Mirewood's edge. Sera's tracking the same pack from the treeline,
        // and from the third round her arrows come out of it; the last one takes the last wolf, which
        // is where the scene after picks up.
        new()
        {
            Id = "a1-c2-01-gloomfangs", Name = "Gloomfang Wolves", Victory = Act1Chapter2.GloomfangsBeaten, Then = "a1-c2-01-gloomfangs-after",
            Foes = new[] { Npc("gloomfang", "wolf-1", F(1)), Npc("gloomfang", "wolf-2", F(2)), Npc("gloomfang", "wolf-3", F(3)), Npc("gloomfang", "wolf-4", F(4)) },
            Supports = new[]
            {
                new EncounterSupport("sera-treeline", "Sera, in the trees", Sera,
                    new SupportAbility("An arrow from the trees: 1d8 to a wolf", SupportTarget.RandomFoe, Dice("1d8"), DamageType.Piercing),
                    FromRound: 3),
            },
        },

        // A1-C2-02: the Thornmother in her den, across two slots. If Tamsin was told the Lodge sent
        // you, its wardens are on the quarry lip with bows. The crystal's choice comes after, and one
        // way through it is the guard wave.
        new()
        {
            Id = "a1-c2-02-thornmother", Name = "The Thornmother", Kind = EncounterKind.MiniBoss, Music = Track.Boss,
            Victory = Act1Chapter2.ThornmotherBeaten, Then = "a1-c2-02-thornmother-after",
            Foes = new[] { Npc("thornmother", "thornmother", F(1)) },
            Supports = new[]
            {
                new EncounterSupport("lodge-wardens", "The Lodge's wardens", "tamsin",
                    new SupportAbility("Arrows from the quarry lip: 1d8 to the Thornmother", SupportTarget.Foe, Dice("1d8"), DamageType.Piercing,
                        Foe: "thornmother"),
                    When: LodgeTold),
            },
        },

        // A1-C2-02, the compromise: the two of you hold the den while Sera draws the rot out of the
        // crystal (so she sits it out, at the crystal behind you, her ring filling as the rounds go),
        // and what's left of the quarry's corrupted comes down the passage at you, a few at a time,
        // the way the story's guard wave does: the pack, then the courier's dead escort, then a
        // straggler. Last till she's done and it's won.
        new()
        {
            Id = "a1-c2-02-guard-wave", Name = "Guarding the Crystal", Victory = Act1Chapter2.CrystalGuarded, Then = "a1-c2-02-guard-wave-after",
            SitsOut = new[] { Sera }, EndsAfterRounds = CrystalRounds,
            Foes = new[] { Spawn("gloomfang", F(1)), Spawn("gloomfang", F(2)), Spawn("gloomfang", F(3)) },
            Waves = new[]
            {
                new EncounterWave(2, new[] { Spawn("hollowed-footman", F(4)) }, null),
                new EncounterWave(3, new[] { Spawn("gloomfang", F(1)) }, null),
            },
            Supports = new[] { new EncounterSupport("sera-crystal", "Sera, at the crystal", Sera, null, Progress: true) },
        },

        // A1-C3-02: the Hollowed out of the treeline at Thornbridge, more of them behind, and Mira
        // fighting beside you from the back before she's joined. The Watch turns up late, as the Watch
        // does: Brisk and two of his from the fourth round.
        new()
        {
            Id = "a1-c3-02-raid", Name = "The Raid on Thornbridge", Victory = Act1Chapter3.RaidBeaten, Then = "a1-c3-02-raid-after",
            Foes = new[]
            {
                Npc("hollowed-footman", "hollowed-1", F(1)), Npc("hollowed-footman", "hollowed-2", F(2)), Npc("hollowed-footman", "hollowed-3", F(3)),
                Npc("hollowed-hexer", "hollowed-4", B(5)),
            },
            Waves = new[]
            {
                new EncounterWave(3, new[] { Spawn("hollowed-footman", F(4)), Spawn("hollowed-hexer", B(6)), Spawn("hollowed-footman", F(1)) },
                    "Where's the Watch?! Where's the king's army?!"),
            },
            Guests = new[] { new EncounterGuest(Mira, Row.Back) },
            Supports = new[]
            {
                new EncounterSupport("thornbridge-watch", "Sergeant Brisk and the Watch", "brisk",
                    new SupportAbility("Watch crossbows: 1d6 to a raider", SupportTarget.RandomFoe, Dice("1d6"), DamageType.Piercing),
                    FromRound: 4),
            },
        },

        // A1-C4-02: Kael on his shelf. A duel: nobody's meant to die, and nobody helps. He calls it
        // off when he's hurt enough, which brings the sappers up through the floor, and the truce leads
        // straight into them.
        new()
        {
            Id = "a1-c4-02-kael", Name = "Kael", Kind = EncounterKind.MiniBoss, Victory = Act1Chapter4.KaelTruce, Then = "a1-c4-02-kael-after",
            YieldsAt = KaelYields, Nonlethal = true,
            Foes = new[] { Npc("kael-duel", "kael", F(1)) },
        },

        // A1-C4-02, after the truce: the sappers up through the floor, their chief behind them, and
        // Kael at your shoulder.
        new()
        {
            Id = "a1-c4-02-sappers", Name = "The Sappers", LootTable = "old-crown-vault", Victory = Act1Chapter4.SappersBeaten, Then = "a1-c4-02-sappers-after",
            Foes = new[]
            {
                Npc("hollowed-sapper", "sapper-1", F(1)), Npc("hollowed-sapper", "sapper-2", F(2)), Npc("hollowed-sapper", "sapper-3", F(3)),
                Spawn("sapper-chief", B(5)),
            },
            Guests = new[] { new EncounterGuest(Kael) },
        },
    };

    private static readonly Dictionary<string, EncounterDefinition> ById = new();

    /// <summary>Indexes the table and checks every encounter.</summary>
    /// <remarks>Throws on purpose, so a fight that names a foe or a guest who isn't there, or that can't start, shows up when the game starts. Who's drawn for a support is checked once the zones are in (ValidateAfterZones).</remarks>
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

    /// <summary>Whether a fight has a foe by this name: its npc id, or its EnemyCatalog id, among the foes it starts with or any wave's.</summary>
    /// <param name="encounter">The fight.</param>
    /// <param name="foe">The name.</param>
    public static bool HasFoe(EncounterDefinition encounter, string foe)
    {
        foreach (EncounterFoe each in encounter.Foes)
            if (each.Npc == foe || each.Enemy == foe) return true;
        foreach (EncounterWave wave in encounter.Waves)
            foreach (EncounterFoe each in wave.Foes)
                if (each.Enemy == foe) return true;
        return false;
    }

    /// <summary>Every room a built fight's fought in: each room where all of a fight's foes from the map stand together, once, in the zones' order.</summary>
    /// <remarks>Call it after ZoneCatalog.LoadAll. A fight with no foes on the map is fought where the scene before it plays, and every one of those so far follows a fight in the same room (the guard wave the Thornmother's), so that room's here already.</remarks>
    public static IReadOnlyList<RoomDefinition> FightRooms()
    {
        var rooms = new List<RoomDefinition>();
        foreach (ZoneDefinition zone in ZoneCatalog.All)
        {
            foreach (RoomDefinition room in zone.Rooms)
            {
                var here = new HashSet<string>();
                foreach (MapNpc npc in room.Npcs) here.Add(npc.Id);

                bool hosts = false;
                foreach (EncounterDefinition row in Table)
                {
                    bool all = true, any = false;
                    foreach (EncounterFoe foe in row.Foes)
                    {
                        if (foe.Npc is null) continue;
                        any = true;
                        all &= here.Contains(foe.Npc);
                    }
                    hosts |= any && all;
                }
                if (hosts) rooms.Add(room);
            }
        }
        return rooms;
    }

    /// <summary>Checks that everyone drawn for a support can be: someone a zone puts down (by their id or their cast id), one of the cast, an origin, an enemy, a creature, or a townsfolk sheet.</summary>
    /// <remarks>Throws on purpose. Call it after ZoneCatalog.LoadAll, since a support's figure is often whoever's standing in the room already: Odelle on her deck, Tomas on the docks, Brisk at the gate.</remarks>
    public static void ValidateAfterZones()
    {
        var people = new HashSet<string>();
        foreach (ZoneDefinition zone in ZoneCatalog.All)
        {
            foreach (RoomDefinition room in zone.Rooms)
            {
                foreach (MapNpc npc in room.Npcs)
                {
                    people.Add(npc.Id);
                    if (npc.Cast is not null) people.Add(npc.Cast);
                }
            }
        }

        foreach (EncounterDefinition row in Table)
        {
            foreach (EncounterSupport support in row.Supports)
            {
                string figure = support.Figure;
                bool drawn = people.Contains(figure) || Cast.Get(figure) is not null || Origins.Exists(figure) || EnemyCatalog.Exists(figure)
                    || Creatures.Get(figure) is not null || Townsfolk.Contains(figure);
                if (!drawn)
                    throw new InvalidOperationException($"Encounter {row.Id}'s support {support.Id} is drawn as {figure}, who isn't anyone a zone puts down, one of the cast, an origin, an enemy, a creature or a townsfolk sheet.");
            }
        }
    }

    /// <summary>A foe who's an npc in the room already, and walks to its slot.</summary>
    private static EncounterFoe Npc(string enemy, string npc, Slot slot) => new(enemy, npc, slot);

    /// <summary>A foe who turns up in its slot.</summary>
    private static EncounterFoe Spawn(string enemy, Slot slot) => new(enemy, null, slot);

    /// <summary>A slot in the foes' front row, 1 to 4 from the left.</summary>
    private static Slot F(int number) =>
        number is >= 1 and <= Slot.FoeLanes ? Slot.Of(Side.Foes, number) : throw new ArgumentOutOfRangeException(nameof(number), number, "The foes' front row is 1 to 4.");

    /// <summary>A slot in the foes' back row, 5 to 8 from the left.</summary>
    private static Slot B(int number) =>
        number is > Slot.FoeLanes and <= 2 * Slot.FoeLanes ? Slot.Of(Side.Foes, number) : throw new ArgumentOutOfRangeException(nameof(number), number, "The foes' back row is 5 to 8.");

    /// <summary>A support's dice.</summary>
    private static DiceExpression Dice(string text) => DiceExpression.Parse(text);

    /// <summary>Checks one encounter, and throws with what's wrong if it can't be fought.</summary>
    private static void Check(EncounterDefinition row)
    {
        string name = $"Encounter {row.Id}";
        if (string.IsNullOrWhiteSpace(row.Name)) throw new InvalidOperationException($"{name} has no name.");
        if (row.Foes.Count == 0) throw new InvalidOperationException($"{name} has nobody to fight.");
        if (!Enum.IsDefined(row.Kind)) throw new InvalidOperationException($"{name} is a kind of fight that isn't an EncounterKind.");
        if (!Enum.IsDefined(row.Music)) throw new InvalidOperationException($"{name} plays a song that isn't a Track.");
        if (row.LootTable is not null && !LootTableCatalog.Exists(row.LootTable))
            throw new InvalidOperationException($"{name} is won with loot table {row.LootTable}, which isn't in LootTableCatalog.");
        if (row.YieldsAt is < 0f or >= 1f) throw new InvalidOperationException($"{name} has foes yielding at {row.YieldsAt:P0} of their hit points.");
        if (row.Then is not null && DialogueCatalog.Get(row.Then) is null) throw new InvalidOperationException($"{name} goes on to conversation {row.Then}, which isn't in DialogueCatalog.");
        if (row.Lost is not null && DialogueCatalog.Get(row.Lost) is null) throw new InvalidOperationException($"{name} is lost into conversation {row.Lost}, which isn't in DialogueCatalog.");

        if (row.FreezeZones.Count > 0) throw new InvalidOperationException($"{name} has freeze zones, which come with M7/M8.");
        if (row.Rooms.Count > 0) throw new InvalidOperationException($"{name} goes across rooms, which comes with M7/M8.");
        foreach (EncounterWave wave in row.Waves)
            if (wave.Interlude is not null) throw new InvalidOperationException($"{name}'s wave on round {wave.Round} has an interlude, {wave.Interlude}, which comes with M7/M8.");

        CheckEnding(name, row);
        CheckFoes(name, row);
        CheckGuests(name, row);
        CheckSupports(name, row);

        foreach (string member in row.SitsOut)
        {
            if (!Origins.Exists(member)) throw new InvalidOperationException($"{name} sits out {member}, who isn't an origin.");
            if (member is Origins.You or Origins.SoulBound) throw new InvalidOperationException($"{name} sits out {member}, and the two of you never sit a fight out.");
        }
    }

    /// <summary>Checks how a fight ends and the level it's tuned for: rounds and a health share that make sense, a foe of the fight's if the share watches one, and a level the table can find.</summary>
    private static void CheckEnding(string name, EncounterDefinition row)
    {
        if (row.EndsAfterRounds < 0) throw new InvalidOperationException($"{name} ends after {row.EndsAfterRounds} rounds.");
        if (row.EndsAtHealthShare is < 0f or >= 1f) throw new InvalidOperationException($"{name} ends at {row.EndsAtHealthShare:P0} of a foe's hit points.");
        if (row.EndsAtFoe is not null && row.EndsAtHealthShare <= 0f)
            throw new InvalidOperationException($"{name} ends on {row.EndsAtFoe}, with no EndsAtHealthShare to end at.");
        if (row.EndsAtFoe is not null && !HasFoe(row, row.EndsAtFoe))
            throw new InvalidOperationException($"{name} ends at a share of {row.EndsAtFoe}'s hit points, who isn't one of its foes.");

        if (row.ExpectedLevel is < 0 or > LevelTable.MaxLevel)
            throw new InvalidOperationException($"{name} expects the party at level {row.ExpectedLevel}, past the 1 to {LevelTable.MaxLevel} there are.");
        LevelTable.Expected(row);
    }

    /// <summary>Checks the foes: each one's stat block, npc or not, and slot on the foes' side; nobody's slot overlapping another's, a big foe's two included; and the waves, from round 2 in order, with nobody from the map in them.</summary>
    /// <remarks>A wave's slots are only checked against each other, not the foes already there: someone may well be standing in one when it comes, and then the newcomer finds the nearest free one.</remarks>
    private static void CheckFoes(string name, EncounterDefinition row)
    {
        var npcs = new HashSet<string>();
        foreach (EncounterFoe foe in row.Foes)
        {
            CheckFoe(name, foe);
            if (foe.Npc is not null && !npcs.Add(foe.Npc)) throw new InvalidOperationException($"{name} has npc {foe.Npc} in it twice.");
        }
        CheckSlots(name, row.Foes);

        int round = 1;
        foreach (EncounterWave wave in row.Waves)
        {
            if (wave.Round <= round) throw new InvalidOperationException($"{name} has a wave on round {wave.Round}: waves come from round 2, in order.");
            if (wave.Foes.Count == 0) throw new InvalidOperationException($"{name} has an empty wave on round {wave.Round}.");
            foreach (EncounterFoe foe in wave.Foes)
            {
                CheckFoe(name, foe);
                if (foe.Npc is not null) throw new InvalidOperationException($"{name}'s wave on round {wave.Round} names npc {foe.Npc}: a wave's foes turn up in their slots.");
            }
            CheckSlots($"{name}'s wave on round {wave.Round}", wave.Foes);
            round = wave.Round;
        }
    }

    /// <summary>Checks one foe line: its stat block exists, and its slot's one of the foes' eight.</summary>
    private static void CheckFoe(string name, EncounterFoe foe)
    {
        if (!EnemyCatalog.Exists(foe.Enemy)) throw new InvalidOperationException($"{name} has a {foe.Enemy}, which isn't in EnemyCatalog.");
        if (foe.Slot.Side != Side.Foes) throw new InvalidOperationException($"{name}'s {foe.Enemy} stands on the party's side.");
        if (!Enum.IsDefined(foe.Slot.Row) || foe.Slot.Lane < 0 || foe.Slot.Lane >= Slot.FoeLanes)
            throw new InvalidOperationException($"{name}'s {foe.Enemy} stands in a slot the foes haven't got (row {foe.Slot.Row}, lane {foe.Slot.Lane}).");
    }

    /// <summary>Checks a set of foes who stand together don't overlap, and that a big one's second slot is there.</summary>
    private static void CheckSlots(string name, IReadOnlyList<EncounterFoe> foes)
    {
        var taken = new Dictionary<Slot, string>();
        foreach (EncounterFoe foe in foes)
        {
            bool big = EnemyCatalog.Get(foe.Enemy).Big;
            if (big && foe.Slot.Lane + 1 >= Slot.FoeLanes)
                throw new InvalidOperationException($"{name}'s {foe.Enemy} is big and stands in slot {foe.Slot.Number}, the end of its row, with no slot beside it to take.");

            for (int lane = foe.Slot.Lane; lane <= foe.Slot.Lane + (big ? 1 : 0); lane++)
            {
                var slot = foe.Slot with { Lane = lane };
                if (!taken.TryAdd(slot, foe.Npc ?? foe.Enemy))
                    throw new InvalidOperationException($"{name} has {foe.Npc ?? foe.Enemy} and {taken[slot]} both in slot {slot.Number}.");
            }
        }
    }

    /// <summary>Checks the guests: each an origin or an enemy, neither of the two of you, only once, in a row, from round 1 on.</summary>
    private static void CheckGuests(string name, EncounterDefinition row)
    {
        var guests = new HashSet<string>();
        foreach (EncounterGuest guest in row.Guests)
        {
            if (guest is null) throw new InvalidOperationException($"{name} has a guest that's nobody.");
            if (!Origins.Exists(guest.Id) && !EnemyCatalog.Exists(guest.Id)) throw new InvalidOperationException($"{name}'s guest {guest.Id} is neither an origin nor an enemy.");
            if (guest.Id is Origins.You or Origins.SoulBound) throw new InvalidOperationException($"{name} has {guest.Id} as a guest, who's always in the party.");
            if (!guests.Add(guest.Id)) throw new InvalidOperationException($"{name} has {guest.Id} as a guest twice.");
            if (!Enum.IsDefined(guest.Row)) throw new InvalidOperationException($"{name}'s guest {guest.Id} would rather a row that isn't one.");
            if (guest.FromRound < 1) throw new InvalidOperationException($"{name}'s guest {guest.Id} joins on round {guest.FromRound}.");
            if (guest.When is not null && string.IsNullOrWhiteSpace(guest.When)) throw new InvalidOperationException($"{name}'s guest {guest.Id} comes on a blank flag.");
        }
    }

    /// <summary>Checks the supports: ids of their own, a name, rounds that make sense, an ability that does something to someone who's there, and a fight that ends after so many rounds for a progress ring to count.</summary>
    private static void CheckSupports(string name, EncounterDefinition row)
    {
        var ids = new HashSet<string>();
        foreach (EncounterSupport support in row.Supports)
        {
            string who = $"{name}'s support {support?.Id}";
            if (support is null || string.IsNullOrWhiteSpace(support.Id)) throw new InvalidOperationException($"{name} has a support with no id.");
            if (!ids.Add(support.Id)) throw new InvalidOperationException($"{name} has two supports called {support.Id}.");
            if (string.IsNullOrWhiteSpace(support.Name)) throw new InvalidOperationException($"{who} has no name for the turn order.");
            if (string.IsNullOrWhiteSpace(support.Figure)) throw new InvalidOperationException($"{who} has nobody drawn for it.");
            if (support.FromRound < 1) throw new InvalidOperationException($"{who} acts from round {support.FromRound}.");
            if (support.UntilRound != 0 && support.UntilRound < support.FromRound)
                throw new InvalidOperationException($"{who} acts from round {support.FromRound} until round {support.UntilRound}.");
            if (support.When is not null && string.IsNullOrWhiteSpace(support.When)) throw new InvalidOperationException($"{who} comes on a blank flag.");
            if (support.Progress && row.EndsAfterRounds == 0) throw new InvalidOperationException($"{who} shows a progress ring, and the fight doesn't end after so many rounds.");
            if (support.Ability is SupportAbility ability) CheckAbility(who, row, ability);
        }
    }

    /// <summary>Checks what a support does: words for its banner, someone to land on, and damage or a condition to land with.</summary>
    private static void CheckAbility(string who, EncounterDefinition row, SupportAbility ability)
    {
        if (string.IsNullOrWhiteSpace(ability.Text)) throw new InvalidOperationException($"{who} has no words for its banner.");
        if (!Enum.IsDefined(ability.Target) || ability.Target == SupportTarget.None)
            throw new InvalidOperationException($"{who}'s ability lands on nobody; a support that does nothing has no ability.");
        if (!Enum.IsDefined(ability.Type)) throw new InvalidOperationException($"{who} does a kind of damage that isn't a DamageType.");
        if (ability.Damage.IsNone && ability.Condition is null) throw new InvalidOperationException($"{who}'s ability does neither damage nor a condition.");
        if (ability.Condition is ConditionId condition && (!Enum.IsDefined(condition) || ability.Turns < 1))
            throw new InvalidOperationException($"{who} puts on {condition} for {ability.Turns} turns.");
        if (ability.Condition is null && ability.Turns != 0) throw new InvalidOperationException($"{who} lasts {ability.Turns} turns with no condition to last.");
        if ((ability.Target == SupportTarget.Foe) != (ability.Foe is not null))
            throw new InvalidOperationException($"{who} needs a Foe if and only if it's aimed at one foe.");
        if (ability.Foe is not null && !HasFoe(row, ability.Foe)) throw new InvalidOperationException($"{who} is aimed at {ability.Foe}, who isn't one of the fight's foes.");
    }
}
