using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// One place in the world, as read out of its zone file: a chain of side-on rooms, what it's
/// called, where it is on the world map, and what plays there.
/// </summary>
/// <remarks>
/// A zone has the id its old tile map had, and keeps every id the map's objects had, so a save's
/// zone/id keys (what's been opened, triggered and found) mean what they meant. Legacy says which
/// of the old map's tiles each room came from, which is how an old save finds its room.
/// </remarks>
public sealed class ZoneDefinition
{
    /// <summary>What saves and other zones call it. Has to match the file name.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>Where it is on the world map, in the map screen's pixels, or null if it isn't on it: inside a building, or out at sea.</summary>
    public required Point? World { get; init; }

    /// <summary>The song that plays in it.</summary>
    public required Track Music { get; init; }

    /// <summary>Which region its random encounters come from (08 §3), or null for none.</summary>
    public required string Region { get; init; }

    /// <summary>How much each room walked into adds to the danger meter. Towns and insides are 0, and never roll.</summary>
    public required int Danger { get; init; }

    /// <summary>Its rooms, left to right as the file has them.</summary>
    public required IReadOnlyList<RoomDefinition> Rooms { get; init; }

    /// <summary>Which of the old map's tiles each room came from: a rectangle in old tiles, and the room.</summary>
    public required IReadOnlyList<(Rectangle OldTiles, string Room)> Legacy { get; init; }

    /// <summary>One of its rooms, by id.</summary>
    /// <param name="id">The room's id.</param>
    /// <returns>The room, or null if it hasn't one called that.</returns>
    public RoomDefinition Room(string id)
    {
        foreach (RoomDefinition room in Rooms)
            if (room.Id == id) return room;
        return null;
    }

    /// <summary>The room a spawn's in.</summary>
    /// <param name="spawn">The spawn's name.</param>
    /// <returns>The room, or null if none of its rooms has a spawn called that.</returns>
    public RoomDefinition RoomOfSpawn(string spawn)
    {
        foreach (RoomDefinition room in Rooms)
            if (spawn is not null && room.Spawns.ContainsKey(spawn)) return room;
        return null;
    }
}
