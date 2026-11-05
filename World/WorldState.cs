using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LegendQuest.Items;
using LegendQuest.Save;

namespace LegendQuest.World;

/// <summary>
/// The rules for the things in a zone: opening chests and doors, pulling levers, reading signs,
/// finding waypoints, and walking into triggers. It reads and changes the save, and draws nothing.
/// </summary>
/// <remarks>
/// The same shape as The Black Box's RoundEngine: the screen asks it to do something and it hands
/// back what happened, and the screen turns that into a sound and a line on screen. There are no
/// dice yet, so it doesn't take a Random. It will when locked doors can be forced. The save keeps
/// what's been used as zone/id, and a zone has its old map's id, so the keys are what they were.
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

    /// <summary>How a chest's items line names something there's more than one of: the name, then how many.</summary>
    private const string CountedLine = "{0} x{1}";

    /// <summary>What goes between the things a chest's items line names.</summary>
    private const string ItemSeparator = ", ";

    /// <summary>What a waypoint says the first time it's used.</summary>
    private const string FoundLine = "WAYPOINT FOUND";

    private readonly SaveData _game;
    private readonly string _zone;

    /// <summary>Sets up the rules for one zone of a game.</summary>
    /// <param name="game">The save, which is where everything that changes is kept.</param>
    /// <param name="zone">The zone you're in, by its id.</param>
    public WorldState(SaveData game, string zone)
    {
        _game = game;
        _zone = zone;
    }

    /// <summary>Whether a chest or door is open, a lever is pulled, or a waypoint has been found. A sign is never open.</summary>
    /// <param name="thing">The thing.</param>
    public bool IsOpen(MapInteractable thing) => thing.Kind switch
    {
        InteractableKind.Sign => false,
        InteractableKind.Lever => _game.Flags.Contains(thing.Flag),
        InteractableKind.Door when thing.Flag is not null => _game.Flags.Contains(thing.Flag),
        InteractableKind.Waypoint => _game.Waypoints.Contains(Key(thing)),
        _ => _game.Opened.Contains(Key(thing)),
    };

    /// <summary>Whether there's anything left to do with it. An open chest or door has nothing; everything else can be used again.</summary>
    /// <param name="thing">The thing.</param>
    public bool CanUse(MapInteractable thing) =>
        thing.Kind is not (InteractableKind.Chest or InteractableKind.Door) || !IsOpen(thing);

    /// <summary>Uses something: opens it, pulls it, reads it or wakes it.</summary>
    /// <param name="thing">The thing. Check CanUse first.</param>
    /// <returns>What happened, in order. Usually one thing; a chest is the lid, then the gold, then the items, for whichever it has.</returns>
    public List<WorldEvent> Use(MapInteractable thing)
    {
        var happened = new List<WorldEvent>();

        switch (thing.Kind)
        {
            case InteractableKind.Chest:
                _game.Opened.Add(Key(thing));
                happened.Add(new WorldEvent(WorldEventKind.ChestOpened, thing.Gold > 0 || thing.Items.Count > 0 ? null : EmptyLine));
                if (thing.Gold > 0)
                {
                    _game.Gold += thing.Gold;
                    happened.Add(new WorldEvent(WorldEventKind.GoldTaken, string.Format(CultureInfo.InvariantCulture, GoldLine, thing.Gold)));
                }
                if (thing.Items.Count > 0)
                {
                    Inventory bag = _game.Bag;
                    foreach (string item in thing.Items) bag.Add(item);
                    happened.Add(new WorldEvent(WorldEventKind.ItemsTaken, ItemsLine(thing.Items)));
                }
                break;

            case InteractableKind.Door when thing.Flag is not null:
                happened.Add(new WorldEvent(WorldEventKind.DoorStuck, StuckLine));
                break;

            case InteractableKind.Door when thing.Locked:
                happened.Add(new WorldEvent(WorldEventKind.DoorLocked, LockedLine));
                break;

            case InteractableKind.Door:
                _game.Opened.Add(Key(thing));
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
                happened.Add(_game.Waypoints.Add(Key(thing))
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
        if (trigger.Once && !_game.Triggered.Add(_zone + "/" + trigger.Id)) return null;
        if (trigger.SetFlag is not null) _game.Flags.Add(trigger.SetFlag);

        if (trigger.Talk is not null) return new WorldEvent(WorldEventKind.TriggerTalked, trigger.Talk);
        return trigger.Say is null ? null : new WorldEvent(WorldEventKind.TriggerSaid, trigger.Say);
    }

    /// <summary>What a chest with items in it says: each one's name, in the order they're listed, with how many there are when it's more than one.</summary>
    /// <param name="items">The items, one entry for each.</param>
    private static string ItemsLine(IReadOnlyList<string> items) =>
        string.Join(ItemSeparator, items.GroupBy(id => id).Select(same => same.Count() == 1
            ? ItemCatalog.Get(same.Key).Name
            : string.Format(CultureInfo.InvariantCulture, CountedLine, ItemCatalog.Get(same.Key).Name, same.Count())));

    /// <summary>How the save keeps track of something in this zone: zone/id.</summary>
    private string Key(MapInteractable thing) => _zone + "/" + thing.Id;
}
