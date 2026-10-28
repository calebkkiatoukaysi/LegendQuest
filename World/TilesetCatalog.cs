using System;
using System.Collections.Generic;

namespace LegendQuest.World;

/// <summary>
/// Every tileset, by the id map files use.
/// </summary>
/// <remarks>
/// The tile lists have to match the lists in tools/generate_world.py (and tools/cut_supplied.py for
/// Questeria), in the same order, since that's the order the tiles are on the sheet. Tilemap throws
/// when it loads a sheet that's the wrong size, which is what a mismatch looks like.
/// </remarks>
public static class TilesetCatalog
{
    /// <summary>Questeria and its harbour: the plaza's slabs, the terrace, cobbles, lawns, sand, water and the boardwalk. Everything standing on them is props.</summary>
    /// <remarks>
    /// Drawn by tools/generate_harbour.py, which has to list them in this order too. The lawns have
    /// a kerb round them, the water a quay wall and a lip of coping, and the boardwalk a beam along
    /// its front, all drawn by Tilemap wherever they meet something else.
    /// </remarks>
    private static readonly TilesetDefinition Questeria = new()
    {
        Id = "questeria",
        Texture = "Tiles/questeria",
        Tiles = new TileDefinition[]
        {
            new() { Name = "plaza", Variants = 3 },
            new() { Name = "terrace", Variants = 2 },
            new() { Name = "cobble", Variants = 3 },
            new() { Name = "grass", Variants = 4, Edged = true, Surface = Surface.Grass },
            new() { Name = "sand", Variants = 2, Surface = Surface.Grass },
            new() { Name = "water", Variants = 3, Edged = true, Solid = true },
            new() { Name = "planks", Variants = 3, Edged = true },
        },
    };

    /// <summary>The barrow: stone floors, walls seen from above and from the front, and what's lying about.</summary>
    private static readonly TilesetDefinition Crypt = new()
    {
        Id = "crypt",
        Texture = "Tiles/crypt",
        Tiles = new TileDefinition[]
        {
            // Ground.
            new() { Name = "floor", Variants = 4 },
            new() { Name = "floor-worn", Variants = 2 },
            new() { Name = "floor-shadow", Variants = 2 },
            new() { Name = "steps" },

            // Walls: the top of one seen from above, then the two rows of its face.
            new() { Name = "wall-top", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "wall-face", Variants = 3, Solid = true, Opaque = true },
            new() { Name = "wall-niche", Solid = true, Opaque = true },
            new() { Name = "wall-base", Variants = 2, Solid = true, Opaque = true },

            // Detail, drawn over the floor.
            new() { Name = "rubble", Solid = true },
            new() { Name = "bones", Variants = 2 },
            new() { Name = "cobweb" },
        },
    };

    /// <summary>The old road: the forest floor and the road through it, the undergrowth, and the trees.</summary>
    /// <remarks>
    /// A tree is a trunk on the detail layer with a canopy over it on the overhead layer. The canopy
    /// comes in nine pieces (four corners, four edges and the middle) so a clump of trees can be any
    /// size of rectangle.
    /// </remarks>
    private static readonly TilesetDefinition Forest = new()
    {
        Id = "forest",
        Texture = "Tiles/forest",
        Tiles = new TileDefinition[]
        {
            // Ground. The thicket is what's under the deep forest: you can't push through it.
            new() { Name = "leaves", Variants = 4, Surface = Surface.Grass },
            new() { Name = "road", Variants = 3, Surface = Surface.Grass },
            new() { Name = "thicket", Variants = 2, Solid = true },

            // Detail, drawn over the ground. Moss and grass are lumps that don't reach the edge of
            // their tile, so a patch of them has a ragged edge instead of a square one.
            new() { Name = "moss", Variants = 2 },
            new() { Name = "tuft", Variants = 2 },
            new() { Name = "fern", Variants = 2 },
            new() { Name = "rock", Solid = true },
            new() { Name = "stump", Solid = true },
            new() { Name = "log-left", Solid = true },
            new() { Name = "log-right", Solid = true },
            new() { Name = "trunk", Solid = true },
            new() { Name = "bush", Variants = 2, Solid = true },

            // Overhead: the canopy, corners and edges round the middle.
            new() { Name = "canopy-nw" },
            new() { Name = "canopy-n", Variants = 2 },
            new() { Name = "canopy-ne" },
            new() { Name = "canopy-w", Variants = 2 },
            new() { Name = "canopy", Variants = 3 },
            new() { Name = "canopy-e", Variants = 2 },
            new() { Name = "canopy-sw" },
            new() { Name = "canopy-s", Variants = 2 },
            new() { Name = "canopy-se" },
        },
    };

    private static readonly Dictionary<string, TilesetDefinition> ById = new();

    /// <summary>Indexes every tileset and checks each one.</summary>
    /// <remarks>Throws on purpose, so a broken tileset is found when the game starts and not on the map that uses it.</remarks>
    static TilesetCatalog()
    {
        foreach (TilesetDefinition tileset in new[] { Questeria, Crypt, Forest })
        {
            tileset.Validate();
            if (!ById.TryAdd(tileset.Id, tileset))
                throw new InvalidOperationException($"Two tilesets are called {tileset.Id}.");
        }
    }

    /// <summary>Looks a tileset up by id.</summary>
    /// <param name="id">What map files call it.</param>
    /// <param name="tileset">The tileset, if there's one called that.</param>
    /// <returns>True if there is.</returns>
    public static bool TryGet(string id, out TilesetDefinition tileset) => ById.TryGetValue(id ?? string.Empty, out tileset);
}
