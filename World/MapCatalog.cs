using System;
using System.Collections.Generic;

namespace LegendQuest.World;

/// <summary>
/// Every map in the game, read and checked once at startup.
/// </summary>
/// <remarks>
/// Loading them all on the first frame is what turns a typo in a map file into an error I see right
/// away, instead of one a player finds when they walk onto that map. They're small text files, so
/// it costs nothing. The world map's positions and links go in here once there's more than one way
/// between maps.
/// </remarks>
public static class MapCatalog
{
    /// <summary>The map a new game starts on.</summary>
    public const string Opening = "village";

    /// <summary>Every map, by id, which is also its file name in Content/Maps.</summary>
    private static readonly string[] Ids = { "village", "barrow" };

    private static readonly Dictionary<string, MapDefinition> Loaded = new();

    /// <summary>Reads and checks every map. Call it once, before anything asks for a map.</summary>
    /// <exception cref="InvalidOperationException">If any map file is wrong. The message says which and where.</exception>
    public static void LoadAll()
    {
        if (Array.IndexOf(Ids, Opening) < 0)
            throw new InvalidOperationException($"The opening map, {Opening}, isn't in the map list.");

        foreach (string id in Ids) Loaded[id] = MapLoader.Load(id);
    }

    /// <summary>Whether there's a map with this id. Saves get checked against this when they're read.</summary>
    /// <param name="id">The map's id.</param>
    public static bool Contains(string id) => id is not null && Loaded.ContainsKey(id);

    /// <summary>One map, by id.</summary>
    /// <param name="id">The map's id. Check it with Contains first if it came from a save.</param>
    public static MapDefinition Get(string id) => Loaded[id];
}
