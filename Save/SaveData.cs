using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LegendQuest.World;

namespace LegendQuest.Save;

/// <summary>
/// Everything one save slot remembers about a game.
/// </summary>
/// <remarks>
/// Plain data only, no sprites or textures, so it serialises and I can hand-edit a save while
/// testing. It's small for now and grows with the game: the party, the inventory, the flags and the
/// quests each get their field when the system that owns them exists. A new field with a default
/// doesn't need a version bump.
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

    /// <summary>The save a new game starts from: on the opening map, with no time played.</summary>
    public static SaveData NewGame() => new() { MapId = MapCatalog.Opening };
}
