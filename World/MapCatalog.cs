using System;
using System.Collections.Generic;

namespace LegendQuest.World;

/// <summary>
/// Every map in the game, read and checked once at startup, and how they join up on the world map.
/// </summary>
/// <remarks>
/// Loading them all on the first frame is what turns a typo in a map file into an error I see right
/// away, instead of one a player finds when they walk onto that map. They're small text files, so
/// it costs nothing. The links between maps aren't written down anywhere of their own: they're
/// wherever an exit goes, so the world map can't disagree with the exits. A door into a building
/// isn't a road, since the building isn't on the world map; it's part of the place it's in.
/// </remarks>
public static class MapCatalog
{
    /// <summary>The map a new game starts on: the deck of the Gull's Wager, two days out from Questeria.</summary>
    public const string Opening = "gulls-wager";

    /// <summary>How wide the world map is, in the map screen's pixels. A map's world = x y has to be inside it.</summary>
    public const int WorldWidth = 1200;

    /// <summary>How tall the world map is, in the map screen's pixels.</summary>
    public const int WorldHeight = 520;

    /// <summary>Every map, by id, which is also its file name in Content/Maps. The world map lists them in this order.</summary>
    private static readonly string[] Ids =
    {
        "gulls-wager", "docks", "questeria", "road", "barrow",
        "gilded-flagon", "cottage-a", "cottage-b", "shop", "warehouse",
    };

    private static readonly Dictionary<string, MapDefinition> Loaded = new();
    private static readonly Dictionary<string, List<string>> Linked = new();

    /// <summary>Every map, in order.</summary>
    public static IEnumerable<MapDefinition> All
    {
        get
        {
            foreach (string id in Ids) yield return Loaded[id];
        }
    }

    /// <summary>Reads and checks every map, then checks every exit goes somewhere. Call it once, before anything asks for a map.</summary>
    /// <exception cref="InvalidOperationException">If any map file is wrong. The message says which and where.</exception>
    public static void LoadAll()
    {
        if (Array.IndexOf(Ids, Opening) < 0)
            throw new InvalidOperationException($"The opening map, {Opening}, isn't in the map list.");

        foreach (string id in Ids) Loaded[id] = MapLoader.Load(id);

        foreach (MapDefinition map in Loaded.Values)
        {
            var links = new List<string>();
            foreach (MapExit exit in map.Exits)
            {
                if (exit.ToMap == MapExit.Back) continue;
                if (!Loaded.TryGetValue(exit.ToMap, out MapDefinition to))
                    throw new InvalidOperationException($"Maps/{map.Id}.map: exit {exit.Id} goes to {exit.ToMap}, and there's no map called that.");
                if (!to.Spawns.ContainsKey(exit.ToSpawn))
                    throw new InvalidOperationException($"Maps/{map.Id}.map: exit {exit.Id} goes to spawn {exit.ToSpawn} on {to.Id}, which doesn't have one called that.");
                if (to.Id != map.Id && to.World is not null && !links.Contains(to.Id)) links.Add(to.Id);
            }
            Linked[map.Id] = links;
        }
    }

    /// <summary>Whether there's a map with this id. Saves get checked against this when they're read.</summary>
    /// <param name="id">The map's id.</param>
    public static bool Contains(string id) => id is not null && Loaded.ContainsKey(id);

    /// <summary>One map, by id.</summary>
    /// <param name="id">The map's id. Check it with Contains first if it came from a save.</param>
    public static MapDefinition Get(string id) => Loaded[id];

    /// <summary>The maps on the world map this one has an exit to.</summary>
    /// <param name="id">The map's id.</param>
    public static IReadOnlyList<string> Links(string id) => Linked[id];
}
