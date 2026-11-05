using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Combat;
using LegendQuest.Dialogue;
using LegendQuest.Story;

namespace LegendQuest.World;

/// <summary>
/// Every zone in the game, read and checked once at startup, and how they join up on the world map.
/// </summary>
/// <remarks>
/// Every zone's read on the first frame, as the tile maps' catalog read every map, so a mistake in
/// one stops the game with the file and the line, and then the zones are checked against each
/// other: every exit and door into another zone finds the room its spawn is in, the camp has
/// somewhere to wait, and everyone and everything stands on its floor. What needs the conversations
/// (that every talk and bark is one, and that every fight's foes stand together) is checked once
/// those are, by ValidateAfterDialogue. tools/rooms/check_rooms.py checks all of this too, and
/// walks every room besides, so a zone the converter writes is known good before the game reads it.
/// The roads on the world map are wherever an exit or a door goes to another zone that's on it.
/// </remarks>
public static class ZoneCatalog
{
    /// <summary>The zone a new game starts in: the deck of the Gull's Wager, two days out from Questeria.</summary>
    public const string Opening = "gulls-wager";

    /// <summary>The zone the camp's in: anyone recruited with the party full waits in the Gilded Flagon's taproom, at its camp seats.</summary>
    public const string Camp = "gilded-flagon";

    /// <summary>How many canvas pixels a zone file has to a world pixel. ZoneLoader divides by it once, so walkers, props and reaches keep the numbers they had on the maps.</summary>
    public const float RoomScale = 4f;

    /// <summary>How tall every room is, in world pixels: 900 canvas pixels.</summary>
    public const float RoomHeight = 225f;

    /// <summary>How wide the world map is, in the map screen's pixels. A zone's world = x y has to be inside it.</summary>
    public const int WorldWidth = 1200;

    /// <summary>How tall the world map is, in the map screen's pixels.</summary>
    public const int WorldHeight = 520;

    /// <summary>Every zone, by id, which is also its file name in Content/Zones. They're the maps' ids, in the maps' order.</summary>
    private static readonly string[] Ids =
    {
        "gulls-wager", "docks", "questeria", "road", "barrow", "thornbridge", "mirewood", "mirewood-quarry",
        "gilded-flagon", "cottage-a", "cottage-b", "shop", "warehouse", "bakery", "forge", "chapel",
        "sunspire-keep", "old-crown-vault", "rusty-anvil",
    };

    /// <summary>How far apart two doors' middles have to be, past half their widths each, so their badges don't overlap: 40 canvas pixels.</summary>
    private const float BadgeGap = 10f;

    /// <summary>The narrowest a badge is, in world pixels: a door narrower than this still has a badge this wide over it.</summary>
    private const float BadgeWidth = 40f;

    private static readonly Dictionary<string, ZoneDefinition> Loaded = new();
    private static readonly Dictionary<string, List<string>> Linked = new();

    /// <summary>Every zone, in order.</summary>
    public static IEnumerable<ZoneDefinition> All
    {
        get
        {
            foreach (string id in Ids) yield return Loaded[id];
        }
    }

    /// <summary>Reads and checks every zone, then checks them against each other. Call it once, before anything asks for a zone, and before DialogueCatalog.ValidateAll, which checks travels against it.</summary>
    /// <exception cref="InvalidOperationException">If any zone file is wrong. The message says which and where.</exception>
    public static void LoadAll()
    {
        if (Array.IndexOf(Ids, Opening) < 0) throw new InvalidOperationException($"The opening zone, {Opening}, isn't in the zone list.");
        if (Array.IndexOf(Ids, Camp) < 0) throw new InvalidOperationException($"The camp's zone, {Camp}, isn't in the zone list.");

        foreach (string id in Ids) Loaded[id] = ZoneLoader.Load(id);

        // Recruiting into a full party says they'll wait here, so there has to be somewhere to wait.
        bool seats = false;
        foreach (RoomDefinition room in Loaded[Camp].Rooms) seats |= room.Camp.Count > 0;
        if (!seats) throw new InvalidOperationException($"Zones/{Camp}.txt: the camp's in this zone, and it has no camp seats for anyone waiting to stand at.");

        foreach (ZoneDefinition zone in Loaded.Values)
        {
            CheckLegacy(zone);
            foreach (RoomDefinition room in zone.Rooms)
            {
                ResolveWaysOut(zone, room);
                Check(zone, room);
            }
        }

        foreach (ZoneDefinition zone in Loaded.Values)
        {
            var links = new List<string>();
            foreach (RoomDefinition room in zone.Rooms)
            {
                foreach (RoomExit exit in room.Exits) Link(zone, exit.ToZone, links);
                foreach (RoomDoor door in room.Doors) Link(zone, door.ToZone, links);
            }
            Linked[zone.Id] = links;
        }
    }

    /// <summary>Checks what needs the conversations: every talk is one and every bark a banter, and every fight's foes stand in one room, inside where its fights stand. Call it after DialogueCatalog.ValidateAll.</summary>
    /// <exception cref="InvalidOperationException">On the first that isn't.</exception>
    public static void ValidateAfterDialogue()
    {
        var rooms = new Dictionary<string, List<RoomDefinition>>();
        foreach (ZoneDefinition zone in All)
        {
            foreach (RoomDefinition room in zone.Rooms)
            {
                string where = $"Zones/{zone.Id}.txt: room {room.Id}";
                foreach (MapNpc npc in room.Npcs)
                {
                    if (npc.Talk is not null && DialogueCatalog.Get(npc.Talk) is null) throw ZoneLoader.Fail(where, $"{npc.Id} talks with {npc.Talk}, and there's no conversation called that");
                    if (!rooms.TryGetValue(npc.Id, out List<RoomDefinition> inRooms)) rooms[npc.Id] = inRooms = new List<RoomDefinition>();
                    inRooms.Add(room);
                }
                foreach (MapTrigger trigger in room.Triggers)
                {
                    if (trigger.Talk is not null && DialogueCatalog.Get(trigger.Talk) is null) throw ZoneLoader.Fail(where, $"trigger {trigger.Id} starts {trigger.Talk}, and there's no conversation called that");
                    if (trigger.Bark is not null && !Banters.Exists(trigger.Bark)) throw ZoneLoader.Fail(where, $"trigger {trigger.Id} plays {trigger.Bark}, and there's no banter called that");
                }
                foreach (MapScene scene in room.Scenes)
                    if (DialogueCatalog.Get(scene.Talk) is null) throw ZoneLoader.Fail(where, $"scene {scene.Id} plays {scene.Talk}, and there's no conversation called that");
            }
        }

        foreach (EncounterDefinition encounter in EncounterCatalog.All)
        {
            var foes = new List<string>();
            foreach (EncounterFoe foe in encounter.Foes)
                if (foe.Npc is not null) foes.Add(foe.Npc);
            if (foes.Count == 0) continue;

            // A fight is fought where its foes are, so they all have to be in one room: the one room
            // every foe's id is in (an id can be in more than one, if someone's in two zones).
            List<RoomDefinition> together = null;
            foreach (string foe in foes)
            {
                if (!rooms.TryGetValue(foe, out List<RoomDefinition> inRooms))
                    throw new InvalidOperationException($"Encounter {encounter.Id}: its foe {foe} isn't standing in any room.");
                together = together is null ? new List<RoomDefinition>(inRooms) : together.FindAll(inRooms.Contains);
            }
            if (together.Count == 0) throw new InvalidOperationException($"Encounter {encounter.Id}: its foes ({string.Join(", ", foes)}) aren't all in one room.");

            RoomDefinition room = together[0];
            foreach (MapNpc npc in room.Npcs)
            {
                if (foes.Contains(npc.Id) && (npc.Feet.X < room.FightLeft || npc.Feet.X > room.FightRight))
                    throw ZoneLoader.Fail($"Zones/{room.ZoneId}.txt: room {room.Id}", $"{npc.Id}, one of {encounter.Id}'s foes, stands outside the room's fight range");
            }
        }
    }

    /// <summary>Whether there's a zone with this id. Saves get checked against this when they're read.</summary>
    /// <param name="zone">The zone's id.</param>
    public static bool Contains(string zone) => zone is not null && Loaded.ContainsKey(zone);

    /// <summary>One zone, by id.</summary>
    /// <param name="zone">The zone's id. Check it with Contains first if it came from a save.</param>
    public static ZoneDefinition Get(string zone) => Loaded[zone];

    /// <summary>The zones on the world map this one has an exit or a door to: its roads.</summary>
    /// <param name="zone">The zone's id.</param>
    public static IReadOnlyList<string> Links(string zone) => Linked[zone];

    /// <summary>Finds a spawn: the room of the zone it's in, and where it is.</summary>
    /// <param name="zone">The zone's id.</param>
    /// <param name="spawn">The spawn's name, unique in its zone.</param>
    /// <param name="room">The room it's in, or null.</param>
    /// <param name="at">The spawn, or null.</param>
    /// <returns>Whether the zone exists and has a spawn by that name.</returns>
    public static bool TryResolve(string zone, string spawn, out RoomDefinition room, out MapSpawn at)
    {
        room = Contains(zone) ? Loaded[zone].RoomOfSpawn(spawn) : null;
        at = room?.Spawns[spawn];
        return room is not null;
    }

    /// <summary>Points every exit and door into another zone at the room its spawn is in, and checks there's such a spawn.</summary>
    private static void ResolveWaysOut(ZoneDefinition zone, RoomDefinition room)
    {
        string where = $"Zones/{zone.Id}.txt: room {room.Id}";

        string Arrive(string id, string toZone, string toSpawn)
        {
            if (!Loaded.ContainsKey(toZone)) throw ZoneLoader.Fail(where, $"{id} goes to {toZone}, which isn't a room here or a zone");
            return Loaded[toZone].RoomOfSpawn(toSpawn)?.Id ?? throw ZoneLoader.Fail(where, $"{id} goes to {toZone} {toSpawn}, and {toZone} has no spawn called {toSpawn}");
        }

        var exits = new List<RoomExit>();
        foreach (RoomExit exit in room.Exits)
            exits.Add(Elsewhere(exit.ToZone, exit.ToRoom) ? exit with { ToRoom = Arrive(exit.Id, exit.ToZone, exit.ToSpawn) } : exit);
        room.Exits = exits;

        var doors = new List<RoomDoor>();
        foreach (RoomDoor door in room.Doors)
            doors.Add(Elsewhere(door.ToZone, door.ToRoom) ? door with { ToRoom = Arrive(door.Id, door.ToZone, door.ToSpawn) } : door);
        room.Doors = doors;
    }

    /// <summary>Whether a way out goes to another zone, whose room isn't known yet: not back, not locked, and not a room of its own zone.</summary>
    private static bool Elsewhere(string toZone, string toRoom) => toZone is not null && toZone != RoomExit.Back && toRoom is null;

    /// <summary>Checks a room's doors fit it without their badges overlapping, everyone and everything in it stands on its floor, and nothing stands in an edge exit.</summary>
    private static void Check(ZoneDefinition zone, RoomDefinition room)
    {
        string where = $"Zones/{zone.Id}.txt: room {room.Id}";

        foreach (RoomDoor door in room.Doors)
        {
            if (door.Area.Left < 0f || door.Area.Right > room.Width || door.Area.Top < 0f)
                throw ZoneLoader.Fail(where, $"door {door.Id}'s box doesn't fit in the room");
        }
        var doors = new List<RoomDoor>(room.Doors);
        doors.Sort((a, b) => a.Foot.X.CompareTo(b.Foot.X));
        for (int i = 1; i < doors.Count; i++)
        {
            RoomDoor a = doors[i - 1], b = doors[i];

            // A locked twin stands where its door does, and only one of the two is ever there.
            if (a.Foot.X == b.Foot.X) continue;
            float need = (MathF.Max(a.Area.Width, BadgeWidth) + MathF.Max(b.Area.Width, BadgeWidth)) / 2f + BadgeGap;
            if (b.Foot.X - a.Foot.X < need) throw ZoneLoader.Fail(where, $"doors {a.Id} and {b.Id} are so close their badges overlap");
        }

        bool west = false, east = false;
        foreach (RoomExit exit in room.Exits)
        {
            west |= exit.Side == ExitSide.West;
            east |= exit.Side == ExitSide.East;
        }

        void Stands(string what, Vector2 feet)
        {
            if (!room.OnFloor(feet)) throw ZoneLoader.Fail(where, $"{what} isn't standing on the floor");
            if (west && feet.X < RoomExit.EdgeReach || east && feet.X > room.Width - RoomExit.EdgeReach)
                throw ZoneLoader.Fail(where, $"{what} is standing in an edge exit; move it in from the end");
        }

        foreach ((string name, MapSpawn spawn) in room.Spawns) Stands($"spawn {name}", spawn.Feet);
        foreach (MapNpc npc in room.Npcs)
            if (npc.Aboard is null) Stands(npc.Id, npc.Feet);
        foreach (MapCamp place in room.Camp) Stands($"camp seat {place.Id}", place.Feet);
        foreach (MapInteractable thing in room.Things)
        {
            Stands(thing.Id, thing.Foot);

            // Fast travel puts you beside the stone, not in front of it, so there has to be floor there.
            if (thing.Kind == InteractableKind.Waypoint && !room.OnFloor(thing.Beside))
                throw ZoneLoader.Fail(where, $"waypoint {thing.Id} has no floor beside it, which is where fast travel puts you");
        }
    }

    /// <summary>Checks no two of a zone's legacy rectangles overlap, since an old save finds its room by the one its feet were in.</summary>
    private static void CheckLegacy(ZoneDefinition zone)
    {
        for (int i = 0; i < zone.Legacy.Count; i++)
        {
            for (int j = i + 1; j < zone.Legacy.Count; j++)
            {
                if (zone.Legacy[i].OldTiles.Intersects(zone.Legacy[j].OldTiles))
                    throw ZoneLoader.Fail($"Zones/{zone.Id}.txt", $"rooms {zone.Legacy[i].Room} and {zone.Legacy[j].Room} have legacy rectangles that overlap, so an old save couldn't tell which it's in");
            }
        }
    }

    /// <summary>Adds a road to another zone on the world map, if a way out goes to one and it's on the map.</summary>
    private static void Link(ZoneDefinition zone, string toZone, List<string> links)
    {
        if (toZone is null || toZone == RoomExit.Back || toZone == zone.Id || links.Contains(toZone)) return;
        if (Loaded[toZone].World is not null) links.Add(toZone);
    }
}
