using System.Collections.Generic;

namespace LegendQuest.Characters;

/// <summary>
/// The townsfolk's sheets, by the id a map's npc sheet= uses.
/// </summary>
/// <remarks>
/// Each one is Content/People/(id).png, cut from my pictures by tools/cut_supplied.py, which lists
/// the same ids. They're drawn already dressed, so unlike the party there's nothing to recolour,
/// and since they only stand and walk, a sheet stops after the walking columns.
/// </remarks>
public static class Townsfolk
{
    /// <summary>Every sheet there is.</summary>
    public static IReadOnlyList<string> All { get; } = new[]
    {
        "elder", "trader", "baker", "smith", "flower-seller", "traveller", "seamstress", "courier",
        "pilgrim", "weaver", "farmhand", "old-man", "maid", "porter", "old-woman", "child", "youth",
    };

    /// <summary>Whether there's a sheet with this id.</summary>
    /// <param name="id">What the map file says.</param>
    public static bool Contains(string id)
    {
        foreach (string sheet in All)
            if (sheet == id) return true;
        return false;
    }

    /// <summary>What to load a sheet as.</summary>
    /// <param name="id">The sheet.</param>
    public static string Texture(string id) => "People/" + id;
}
