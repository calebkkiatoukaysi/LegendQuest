using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LegendQuest.Characters;
using LegendQuest.World;

namespace LegendQuest.Save;

/// <summary>
/// Everything one save slot remembers about a game.
/// </summary>
/// <remarks>
/// Plain data only, no sprites or textures, so it serialises and I can hand-edit a save while
/// testing. It grows with the game: the party, the inventory and the quests each get their field
/// when the system that owns them exists. A new field with a default doesn't need a version bump.
/// Things on a map (a chest, a trigger, a waypoint) are kept as "map/id", so two maps can each have
/// a chest called chest.
/// </remarks>
public class SaveData
{
    /// <summary>Which shape of save file this is. See SaveSystem.CurrentVersion.</summary>
    public int Version { get; set; } = SaveSystem.CurrentVersion;

    /// <summary>When the game was first started, in UTC.</summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>When the slot was last written, in UTC. This is the date the slot shows.</summary>
    public DateTime SavedUtc { get; set; }

    /// <summary>How long has been spent in this game, in seconds.</summary>
    /// <remarks>A number, not a TimeSpan, so the file stays easy to edit by hand. The game uses Playtime.</remarks>
    public double PlaytimeSeconds { get; set; }

    /// <summary>The id of the map the party is on.</summary>
    public string MapId { get; set; } = string.Empty;

    /// <summary>Everyone in the party, in order, each with where they're standing on the map.</summary>
    public List<PartyMember> Party { get; set; } = new();

    /// <summary>Which of the party you're controlling, by their place in Party.</summary>
    public int Leader { get; set; }

    /// <summary>The party's gold.</summary>
    public int Gold { get; set; }

    /// <summary>Every flag that's set. Levers, triggers and (later) conversations and quests set them, and doors read them.</summary>
    public HashSet<string> Flags { get; set; } = new();

    /// <summary>The chests and doors that have been opened, as map/id.</summary>
    public HashSet<string> Opened { get; set; } = new();

    /// <summary>The triggers that only go off once and have, as map/id.</summary>
    public HashSet<string> Triggered { get; set; } = new();

    /// <summary>The waypoints that have been found, as map/id. Fast travel goes between these.</summary>
    public HashSet<string> Waypoints { get; set; } = new();

    /// <summary>What's been seen of each map that's been visited: one bit per tile, row by row, by map id.</summary>
    /// <remarks>Packed, so a map is a few hundred bytes. JSON writes a byte array as base64, which keeps the file short.</remarks>
    public Dictionary<string, byte[]> Seen { get; set; } = new();

    /// <summary>Fields in the file that this build has no property for.</summary>
    /// <remarks>
    /// Without this System.Text.Json drops unknown fields on read, so a field I remove later would
    /// be gone before Upgrade could move it somewhere. The Black Box needed this for its version 2.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extra { get; set; }

    /// <summary>PlaytimeSeconds, in the form the rest of the game wants it.</summary>
    [JsonIgnore]
    public TimeSpan Playtime => TimeSpan.FromSeconds(PlaytimeSeconds);

    /// <summary>Whoever you're controlling.</summary>
    [JsonIgnore]
    public PartyMember LeaderMember => Party[Leader];

    /// <summary>The save a new game starts from: the starting party at the start of the opening map, with no time played.</summary>
    /// <remarks>Only the leader gets put on the start. The map puts the rest round them when it loads.</remarks>
    public static SaveData NewGame()
    {
        MapSpawn start = MapCatalog.Get(MapCatalog.Opening).Spawns[MapLoader.StartSpawn];
        var game = new SaveData { MapId = MapCatalog.Opening, Party = Origins.StartingParty() };
        game.LeaderMember.Feet = start.Feet;
        game.LeaderMember.Facing = start.Facing;
        return game;
    }
}
