using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.World;

namespace LegendQuest.Combat;

/// <summary>
/// One field: foes standing in a wild room once the story's moved past them, a fight you can pick by
/// clicking one, or that picks you if they're the kind to come at you, and that's back a minute after
/// it's won or as soon as you come into the room again (Story/11-combat.md §2.1).
/// </summary>
/// <param name="Encounter">The fight, by its id in EncounterCatalog: a Random one whose foes are all npcs of this room.</param>
/// <param name="Zone">The zone the room's in.</param>
/// <param name="Room">The room.</param>
/// <param name="Aggressive">Whether the foes start the fight themselves when you come within EncounterCatalog.AggroReach of one; if not, they wait to be clicked.</param>
/// <param name="Presence">When they're there: once the story fight that cleared the way is won, and so on.</param>
/// <param name="Foes">Who stands where.</param>
public sealed record FieldGround(string Encounter, string Zone, string Room, bool Aggressive, MapPresence Presence, IReadOnlyList<FieldFoe> Foes);

/// <summary>One foe of a field: the npc the encounter's foe line names, and where the field puts them, or nowhere if the zone file has them standing in the room already.</summary>
/// <param name="Npc">Their npc id, which the encounter's foe line names.</param>
/// <param name="At">Where their feet go, in the zone files' canvas pixels (a room is 900 tall), facing left; or null for someone the zone file has put there.</param>
public sealed record FieldFoe(string Npc, Vector2? At = null);

/// <summary>
/// The field fights: the foes who keep a wild room after its story fight, so a player can fight what's
/// there, as often as they like, for experience and class points (Story/08-random-encounters.md §3
/// says what belongs where; Story/12-rooms.md §7 how they stand in a room).
/// </summary>
/// <remarks>
/// <para>
/// Each is an EncounterKind.Random fight like any other (Check holds them to the same rules), worth a
/// random fight's experience and 15 class points, with the foes' own loot. Its foes are npcs of the
/// room: ones the zone file already has (the vault's sleeping constructs), or ones this table puts
/// down, by FieldGround, where the zone files (written by tools/rooms, never by hand) have nobody.
/// ExploreScreen.Field reads the grounds: their foes' red names, a click to fight, the aggressive ones
/// coming at you, and the room filling again.
/// </para>
/// <para>
/// None is on top of the story: the Mirewood's pack once the scene with the first pack is over, what's
/// left of the raid at Thornbridge's river once the chapter's done, and the vault's constructs, which sleep until
/// someone wakes them (the banter in the vault says so). They aren't in All, so the story's order, the
/// simulator's "all" and the ranks it expects are the story's alone; Field lists them.
/// </para>
/// </remarks>
public static partial class EncounterCatalog
{
    /// <summary>How long a won field takes to fill again while you're still in its room, in seconds. Leaving and coming back fills it at once.</summary>
    public const float FieldRespawnSeconds = 60f;

    /// <summary>How near an aggressive field's foe you can come before it comes at you, in world pixels (11-combat.md §2.1).</summary>
    public const float AggroReach = 120f;

    /// <summary>The field fights, in the story's order of the rooms they're in.</summary>
    private static readonly EncounterDefinition[] FieldTable =
    {
        // The Mirewood, once the scene at its edge is over (A1-C2-01): three more of the gloomfangs that run the
        // deep wood, which come at anyone on the path.
        new()
        {
            Id = "rnd-mirewood-gloomfangs", Name = "Gloomfangs in the Deep", Kind = EncounterKind.Random, ExpectedLevel = 2,
            Foes = new[] { Npc("gloomfang", "field-wolf-1", F(1)), Npc("gloomfang", "field-wolf-2", F(2)), Npc("gloomfang", "field-wolf-3", F(3)) },
        },

        // Thornbridge's river, once the chapter's done (A1-C3), not straight after the raid: two
        // Hollowed who came out of the trees too late for it, and come at anyone on the bank.
        new()
        {
            Id = "rnd-thornbridge-hollowed", Name = "Stragglers of the Raid", Kind = EncounterKind.Random, ExpectedLevel = 2,
            Foes = new[] { Npc("hollowed-footman", "field-hollowed-1", F(1)), Npc("hollowed-footman", "field-hollowed-2", F(2)) },
        },

        // The vault's four constructs, two by the stair and two in the halls: each asleep until somebody
        // wakes it, and each a fight of its own.
        Construct(1), Construct(2), Construct(3), Construct(4),
    };

    /// <summary>Where each field's foes stand and when they're there.</summary>
    private static readonly FieldGround[] Grounds =
    {
        new("rnd-mirewood-gloomfangs", "mirewood", "deep", true, new MapPresence(Act1Chapter2.EdgeDone, null),
            new[] { new FieldFoe("field-wolf-1", new Vector2(1160, 726)), new FieldFoe("field-wolf-2", new Vector2(1340, 806)), new FieldFoe("field-wolf-3", new Vector2(1540, 742)) }),
        new("rnd-thornbridge-hollowed", "thornbridge", "river-edge", true, new MapPresence(Act1Chapter3.ChapterDone, null),
            new[] { new FieldFoe("field-hollowed-1", new Vector2(1120, 712)), new FieldFoe("field-hollowed-2", new Vector2(1260, 790)) }),
        new("rnd-vault-construct-1", "old-crown-vault", "entrance", false, MapPresence.Always, new[] { new FieldFoe("construct-1") }),
        new("rnd-vault-construct-2", "old-crown-vault", "entrance", false, MapPresence.Always, new[] { new FieldFoe("construct-2") }),
        new("rnd-vault-construct-3", "old-crown-vault", "halls", false, MapPresence.Always, new[] { new FieldFoe("construct-3") }),
        new("rnd-vault-construct-4", "old-crown-vault", "halls", false, MapPresence.Always, new[] { new FieldFoe("construct-4") }),
    };

    /// <summary>One of the vault's constructs, by its number in the zone file.</summary>
    private static EncounterDefinition Construct(int number) => new()
    {
        Id = $"rnd-vault-construct-{number}", Name = "A Vault Construct", Kind = EncounterKind.Random, ExpectedLevel = 3,
        Foes = new[] { Npc("vault-construct", $"construct-{number}", F(1)) },
    };

    /// <summary>Every field fight, in the order of the rooms they're in.</summary>
    public static IEnumerable<EncounterDefinition> Field => FieldTable;

    /// <summary>The fields in one room, there or not with the flags as they are.</summary>
    /// <param name="zone">The zone.</param>
    /// <param name="room">The room.</param>
    public static IEnumerable<FieldGround> GroundsIn(string zone, string room) => Grounds.Where(each => each.Zone == zone && each.Room == room);

    /// <summary>The field a foe npc of a room stands in, or null if they're no field's.</summary>
    /// <param name="zone">The zone.</param>
    /// <param name="room">The room.</param>
    /// <param name="npc">Their npc id.</param>
    public static FieldGround GroundOf(string zone, string room, string npc) =>
        Grounds.FirstOrDefault(each => each.Zone == zone && each.Room == room && each.Foes.Any(foe => foe.Npc == npc));

    /// <summary>Every room a field's fought in, for BattleFloor.Check.</summary>
    /// <remarks>Call it after ZoneCatalog.LoadAll.</remarks>
    public static IEnumerable<RoomDefinition> FieldRooms() => Grounds.Select(each => ZoneCatalog.Get(each.Zone).Room(each.Room)).Distinct();

    /// <summary>
    /// Checks every field against the zones: its fight's a Random one that's in the table, its room's
    /// there, each foe it puts down isn't someone the room has already (and stands on its floor), each
    /// it doesn't is, and its foes are exactly its fight's.
    /// </summary>
    /// <remarks>Throws on purpose. Call it after ZoneCatalog.LoadAll.</remarks>
    public static void ValidateField()
    {
        var seen = new HashSet<string>();
        foreach (FieldGround ground in Grounds)
        {
            string name = $"Field {ground.Encounter} in {ground.Zone}/{ground.Room}";
            if (!seen.Add(ground.Encounter)) throw new InvalidOperationException($"{name} is laid out twice.");
            if (!ById.TryGetValue(ground.Encounter, out EncounterDefinition fight) || fight.Kind != EncounterKind.Random || !FieldTable.Contains(fight))
                throw new InvalidOperationException($"{name} has no Random fight of that id in the field table.");
            if (fight.Waves.Count > 0 || fight.Guests.Count > 0 || fight.Supports.Count > 0 || fight.Victory is not null || fight.Then is not null)
                throw new InvalidOperationException($"{name}: a field is its foes alone, with nothing to set or say after.");

            RoomDefinition room = ZoneCatalog.Get(ground.Zone)?.Room(ground.Room)
                ?? throw new InvalidOperationException($"{name}: there's no such room.");
            var npcs = new HashSet<string>(fight.Foes.Select(foe => foe.Npc));
            if (fight.Foes.Any(foe => foe.Npc is null) || !npcs.SetEquals(ground.Foes.Select(foe => foe.Npc)))
                throw new InvalidOperationException($"{name}: its foes have to be its fight's npcs, each once.");

            foreach (FieldFoe foe in ground.Foes)
            {
                bool there = room.Npcs.Any(npc => npc.Id == foe.Npc);
                if (foe.At is null && !there) throw new InvalidOperationException($"{name}: {foe.Npc} isn't in the room's zone file, and the field doesn't put them down.");
                if (foe.At is Vector2 at)
                {
                    if (there) throw new InvalidOperationException($"{name}: {foe.Npc} is in the zone file already, and the field puts them down again.");
                    if (!room.OnFloor(at / ZoneCatalog.RoomScale)) throw new InvalidOperationException($"{name}: {foe.Npc} stands off the room's floor at {at}.");
                }
            }
        }
    }
}
