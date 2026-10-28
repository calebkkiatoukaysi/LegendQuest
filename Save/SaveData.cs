using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LegendQuest.Characters;
using LegendQuest.Story;
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

    /// <summary>Where the last way off a map was: the map, where you stood, and which way you faced. A building's way out goes back here.</summary>
    public string BackMap { get; set; }

    /// <summary>How far across BackMap you were, in world pixels.</summary>
    public float BackX { get; set; }

    /// <summary>How far down BackMap you were, in world pixels.</summary>
    public float BackY { get; set; }

    /// <summary>Everyone in the party, in order, each with where they're standing on the map.</summary>
    public List<PartyMember> Party { get; set; } = new();

    /// <summary>Which of the party you're controlling, by their place in Party.</summary>
    public int Leader { get; set; }

    /// <summary>Whoever's been recruited with the party already full, waiting at camp.</summary>
    public List<PartyMember> Camp { get; set; } = new();

    /// <summary>The party's gold.</summary>
    public int Gold { get; set; }

    /// <summary>The key items the story's handed over, by id in KeyItems.</summary>
    public List<string> Items { get; set; } = new();

    /// <summary>What you're to do next, as the corner of the screen says it, or null for nothing.</summary>
    /// <remarks>A line for now. The journal and its quests take over from this when they exist.</remarks>
    public string Objective { get; set; }

    /// <summary>How you talk, picked at the start.</summary>
    public Voice Voice { get; set; }

    /// <summary>Whoever you're courting, by origin id, or null.</summary>
    public string Romance { get; set; }

    /// <summary>The story's meters that have moved off where they start, by id in Meters. One that's missing is at its start.</summary>
    public Dictionary<string, int> Meters { get; set; } = new();

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

    /// <summary>You, whoever's being controlled. Always in the party, and first in a new game.</summary>
    [JsonIgnore]
    public PartyMember You => Member(Origins.You) ?? Party[0];

    /// <summary>Someone in the party by their id, or null if they aren't in it.</summary>
    /// <param name="id">Their origin id.</param>
    public PartyMember Member(string id)
    {
        foreach (PartyMember member in Party)
            if (member.Id == id) return member;
        return null;
    }

    /// <summary>Whether someone's in the party.</summary>
    /// <param name="id">Their origin id.</param>
    public bool InParty(string id) => Member(id) is not null;

    /// <summary>A meter's value.</summary>
    /// <param name="id">The meter, by its id in Story.Meters.</param>
    public int Meter(string id) =>
        Meters.TryGetValue(id, out int value) ? value : Story.Meters.Get(id).Start;

    /// <summary>Sets a meter, kept inside its range.</summary>
    /// <param name="id">The meter.</param>
    /// <param name="value">What to set it to.</param>
    public void SetMeter(string id, int value)
    {
        MeterDefinition meter = Story.Meters.Get(id);
        Meters[id] = Math.Clamp(value, meter.Min, meter.Max);
    }

    /// <summary>The save a new game starts from: you and the Soul-Bound at the start of the opening map, with no time played.</summary>
    /// <remarks>Only the leader gets put on the start. The map puts the rest round them when it loads.</remarks>
    /// <param name="you">You, as the creation screen made you.</param>
    /// <param name="soulBound">The Soul-Bound, as the second pass of it made them.</param>
    public static SaveData NewGame(PartyMember you, PartyMember soulBound)
    {
        MapSpawn start = MapCatalog.Get(MapCatalog.Opening).Spawns[MapLoader.StartSpawn];
        var game = new SaveData { MapId = MapCatalog.Opening, Party = new List<PartyMember> { you, soulBound } };
        game.LeaderMember.Feet = start.Feet;
        game.LeaderMember.Facing = start.Facing;
        return game;
    }
}
