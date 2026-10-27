using System.Collections.Generic;
using System.Globalization;
using LegendQuest.Save;

namespace LegendQuest.World;

/// <summary>
/// The rules for the things on a map: opening chests and doors, pulling levers, reading signs,
/// finding waypoints, and walking into triggers. It reads and changes the save, and draws nothing.
/// </summary>
/// <remarks>
/// The same shape as The Black Box's RoundEngine: the screen asks it to do something and it hands
/// back what happened, and the screen turns that into a sound and a line on screen. There are no
/// dice yet, so it doesn't take a Random. It will when locked doors can be forced.
/// </remarks>
public class WorldState
{
    /// <summary>What trying a locked door says.</summary>
    private const string LockedLine = "LOCKED";

    /// <summary>What trying a door that something else works says. A hint to go and look for the something.</summary>
    private const string StuckLine = "IT WON'T MOVE BY HAND";

    /// <summary>What a chest with nothing in it says.</summary>
    private const string EmptyLine = "EMPTY";

    /// <summary>What a chest with gold in it says.</summary>
    private const string GoldLine = "{0} GOLD";

    /// <summary>What a waypoint says the first time it's used.</summary>
    private const string FoundLine = "WAYPOINT FOUND";

    private readonly SaveData _game;
    private readonly MapDefinition _map;

    /// <summary>Sets up the rules for one map of a game.</summary>
    /// <param name="game">The save, which is where everything that changes is kept.</param>
    /// <param name="map">The map you're on.</param>
    public WorldState(SaveData game, MapDefinition map)
    {
        _game = game;
        _map = map;
    }

    /// <summary>Whether a chest or door is open, a lever is pulled, or a waypoint has been found. A sign is never open.</summary>
    /// <param name="thing">The thing.</param>
    public bool IsOpen(MapInteractable thing) => thing.Kind switch
    {
        InteractableKind.Sign => false,
        InteractableKind.Lever => _game.Flags.Contains(thing.Flag),
        InteractableKind.Door when thing.Flag is not null => _game.Flags.Contains(thing.Flag),
        InteractableKind.Waypoint => _game.Waypoints.Contains(Key(_map, thing)),
        _ => _game.Opened.Contains(Key(_map, thing)),
    };

    /// <summary>Whether there's anything left to do with it. An open chest or door has nothing; everything else can be used again.</summary>
    /// <param name="thing">The thing.</param>
    public bool CanUse(MapInteractable thing) =>
        thing.Kind is not (InteractableKind.Chest or InteractableKind.Door) || !IsOpen(thing);

    /// <summary>Uses something: opens it, pulls it, reads it or wakes it.</summary>
    /// <param name="thing">The thing. Check CanUse first.</param>
    /// <returns>What happened, in order. Usually one thing; a chest with gold in it is two.</returns>
    public List<WorldEvent> Use(MapInteractable thing)
    {
        var happened = new List<WorldEvent>();

        switch (thing.Kind)
        {
            case InteractableKind.Chest:
                _game.Opened.Add(Key(_map, thing));
                happened.Add(new WorldEvent(WorldEventKind.ChestOpened, thing.Gold > 0 ? null : EmptyLine));
                if (thing.Gold > 0)
                {
                    _game.Gold += thing.Gold;
                    happened.Add(new WorldEvent(WorldEventKind.GoldTaken, string.Format(CultureInfo.InvariantCulture, GoldLine, thing.Gold)));
                }
                break;

            case InteractableKind.Door when thing.Flag is not null:
                happened.Add(new WorldEvent(WorldEventKind.DoorStuck, StuckLine));
                break;

            case InteractableKind.Door when thing.Locked:
                happened.Add(new WorldEvent(WorldEventKind.DoorLocked, LockedLine));
                break;

            case InteractableKind.Door:
                _game.Opened.Add(Key(_map, thing));
                happened.Add(new WorldEvent(WorldEventKind.DoorOpened, null));
                break;

            case InteractableKind.Lever:
                if (!_game.Flags.Remove(thing.Flag)) _game.Flags.Add(thing.Flag);
                happened.Add(new WorldEvent(WorldEventKind.LeverPulled, null));
                break;

            case InteractableKind.Sign:
                happened.Add(new WorldEvent(WorldEventKind.SignRead, thing.Text));
                break;

            default:
                happened.Add(_game.Waypoints.Add(Key(_map, thing))
                    ? new WorldEvent(WorldEventKind.WaypointFound, FoundLine)
                    : new WorldEvent(WorldEventKind.WaypointUsed, null));
                break;
        }

        return happened;
    }

    /// <summary>Walks into a trigger: marks it used if it only goes off once, and sets its flag.</summary>
    /// <param name="trigger">The trigger.</param>
    /// <returns>What it says, or null if it says nothing (or has already gone off).</returns>
    public WorldEvent Enter(MapTrigger trigger)
    {
        if (trigger.Once && !_game.Triggered.Add(_map.Id + "/" + trigger.Id)) return null;
        if (trigger.SetFlag is not null) _game.Flags.Add(trigger.SetFlag);

        return trigger.Say is null ? null : new WorldEvent(WorldEventKind.TriggerSaid, trigger.Say);
    }

    /// <summary>Whether any of a map has been seen, which is what puts it on the world map.</summary>
    /// <param name="map">The map.</param>
    public bool IsDiscovered(MapDefinition map) => _game.Seen.ContainsKey(map.Id);

    /// <summary>A map's waypoint, if it's been found.</summary>
    /// <param name="map">The map.</param>
    /// <returns>The waypoint, or null if the map hasn't got one or it hasn't been found yet.</returns>
    public MapInteractable FoundWaypoint(MapDefinition map)
    {
        foreach (MapInteractable thing in map.Interactables)
            if (thing.Kind == InteractableKind.Waypoint && _game.Waypoints.Contains(Key(map, thing))) return thing;
        return null;
    }

    /// <summary>Every map with a found waypoint, other than this one, in world map order. That's where fast travel can go.</summary>
    /// <remarks>Fast travel only starts from a waypoint. Once there's fighting, it won't start during a fight either.</remarks>
    public List<MapDefinition> Destinations()
    {
        var destinations = new List<MapDefinition>();
        foreach (MapDefinition map in MapCatalog.All)
            if (map.Id != _map.Id && FoundWaypoint(map) is not null) destinations.Add(map);
        return destinations;
    }

    /// <summary>How the save keeps track of something on a map: map/id.</summary>
    private static string Key(MapDefinition map, MapInteractable thing) => map.Id + "/" + thing.Id;
}
