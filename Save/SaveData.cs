using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Save;

/// <summary>
/// Everything one save slot remembers about a game.
/// </summary>
/// <remarks>
/// Plain data only, no sprites or textures, so it serialises and I can hand-edit a save while
/// testing. It grows with the game: the party, the bag and the quests each get their field when
/// the system that owns them exists. A new field with a default doesn't need a version bump.
/// Things on a map (a chest, a trigger, a waypoint) are kept as "map/id", so two maps can each have
/// a chest called chest.
/// </remarks>
public class SaveData
{
    /// <summary>How many healing potions a party starts with in its bag.</summary>
    public const int StartingPotions = 2;

    /// <summary>Which shape of save file this is. See SaveSystem.CurrentVersion.</summary>
    public int Version { get; set; } = SaveSystem.CurrentVersion;

    /// <summary>When the game was first started, in UTC.</summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>When the slot was last written, in UTC. This is the date the slot shows.</summary>
    public DateTime SavedUtc { get; set; }

    /// <summary>How long has been spent in this game, in seconds.</summary>
    /// <remarks>A number, not a TimeSpan, so the file stays easy to edit by hand. The game uses Playtime.</remarks>
    public double PlaytimeSeconds { get; set; }

    /// <summary>The zone the party is in. A zone has its old map's id.</summary>
    public string ZoneId { get; set; } = string.Empty;

    /// <summary>The room of ZoneId the party is in. The party's feet are where they stand in it.</summary>
    public string RoomId { get; set; }

    /// <summary>Where the last way out of a zone was: a building's way out goes back here. Null if there's been none.</summary>
    public string BackZone { get; set; }

    /// <summary>The room of BackZone the way out was in.</summary>
    public string BackRoom { get; set; }

    /// <summary>Where in BackRoom to come back out, across, in world pixels: in front of the door, or in from the end.</summary>
    public float BackX { get; set; }

    /// <summary>Where in BackRoom to come back out, down, in world pixels.</summary>
    public float BackY { get; set; }

    /// <summary>Every room that's been walked into, as zone/room. The zone map lets you jump to any of them.</summary>
    public HashSet<string> Visited { get; set; } = new();

    /// <summary>Everyone in the party, in order, each with where they're standing in the room.</summary>
    public List<PartyMember> Party { get; set; } = new();

    /// <summary>Which of the party you're controlling, by their place in Party.</summary>
    public int Leader { get; set; }

    /// <summary>Whoever's been recruited with the party already full, waiting at camp.</summary>
    public List<PartyMember> Camp { get; set; } = new();

    /// <summary>Whoever's gone away for a while (a script's Leave), till a Rejoin brings them back.</summary>
    /// <remarks>Not in the party and not at camp, so nobody can pick them, talk to them, swap them in or take them into a fight. They keep their kit, hit points and the rest while they're gone.</remarks>
    public List<PartyMember> Away { get; set; } = new();

    /// <summary>The party's gold.</summary>
    public int Gold { get; set; }

    /// <summary>The key items the story's handed over, by id in KeyItems.</summary>
    public List<string> Items { get; set; } = new();

    /// <summary>The party's shared bag: everything that isn't a key item and isn't being worn. Bag wraps it.</summary>
    public List<ItemStack> Inventory { get; set; } = new();

    /// <summary>The fight that was starting when this was saved, by its encounter id, or null if there wasn't one.</summary>
    /// <remarks>
    /// Every story fight autosaves as it starts, after whatever started it (a trigger, a conversation)
    /// has been used up, so loading that save couldn't start the fight again on its own. This says
    /// which fight it was, and loading the save starts it again: LOAD LAST SAVE after a loss is the
    /// fight again, not the map with the fight gone.
    /// </remarks>
    public string Fight { get; set; }

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

    /// <summary>The tutorial tips that have been shown, by id, so each is only shown once.</summary>
    public HashSet<string> Tips { get; set; } = new();

    /// <summary>The chests and doors that have been opened, as map/id.</summary>
    public HashSet<string> Opened { get; set; } = new();

    /// <summary>The triggers that only go off once and have, as map/id.</summary>
    public HashSet<string> Triggered { get; set; } = new();

    /// <summary>The waypoints that have been found, as map/id. Fast travel goes between these.</summary>
    public HashSet<string> Waypoints { get; set; } = new();

    /// <summary>What everyone's holding in their sockets, as scenes' hold and drop cues left it: by who (an origin id, or someone's id on a map), then by socket (hand_front, neck, belt_front), the item's id.</summary>
    /// <remarks>
    /// Legacy characters draw none of it. It's kept so that whatever draws characters with their
    /// items in their hands later finds them where the story put them, mid-game as much as from the
    /// start: the herring on your neck from the voyage, Tomas's hook on your belt.
    /// </remarks>
    public Dictionary<string, Dictionary<string, string>> Sockets { get; set; } = new();

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

    /// <summary>The party's bag, for adding to it, taking from it and counting what's in it. It works on Inventory itself.</summary>
    [JsonIgnore]
    public Inventory Bag => new(Inventory);

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

    /// <summary>Whether someone's in the party. Not if they're waiting at camp, or away.</summary>
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

    /// <summary>The save a new game starts from: you and the Soul-Bound at the start of the opening zone, with no time played and StartingPotions in the bag.</summary>
    /// <remarks>Only the leader gets put on the start. The room puts the rest behind them when it loads.</remarks>
    /// <param name="you">You, as the creation screen made you.</param>
    /// <param name="soulBound">The Soul-Bound, as the second pass of it made them.</param>
    public static SaveData NewGame(PartyMember you, PartyMember soulBound)
    {
        ZoneCatalog.TryResolve(ZoneCatalog.Opening, ZoneLoader.StartSpawn, out RoomDefinition room, out MapSpawn start);
        var game = new SaveData { ZoneId = ZoneCatalog.Opening, RoomId = room.Id, Party = new List<PartyMember> { you, soulBound } };
        game.LeaderMember.Feet = start.Feet;
        game.LeaderMember.Facing = start.Facing;
        game.Visited.Add(ZoneCatalog.Opening + "/" + room.Id);

        game.Bag.Add(ItemCatalog.HealingPotion, StartingPotions);
        return game;
    }
}
