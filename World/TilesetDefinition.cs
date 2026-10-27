using System;
using System.Collections.Generic;

namespace LegendQuest.World;

/// <summary>
/// One tileset: the sheet it's cut from, and every tile on it in order.
/// </summary>
/// <remarks>
/// A tile's id is its place in Tiles counting from 1, same as the tilemap exercise (0 is an empty
/// cell). The sheet has to have the tiles in this order, which tools/generate_world.py makes sure of
/// by working from the same list. Tilemap checks the sheet is the right size when it loads.
/// </remarks>
public sealed class TilesetDefinition
{
    /// <summary>What map files call it.</summary>
    public required string Id { get; init; }

    /// <summary>The sheet, by content name.</summary>
    public required string Texture { get; init; }

    /// <summary>Every tile, in sheet order.</summary>
    public required IReadOnlyList<TileDefinition> Tiles { get; init; }

    /// <summary>Tile ids by name, and where each tile's first frame is on the sheet. Built by Validate.</summary>
    private Dictionary<string, int> _ids;
    private int[] _firstFrames;

    /// <summary>How many frames the sheet should have: every variant of every tile.</summary>
    public int FrameCount { get; private set; }

    /// <summary>One tile, by id.</summary>
    /// <param name="id">From 1. 0 is an empty cell and has no tile.</param>
    public TileDefinition this[int id] => Tiles[id - 1];

    /// <summary>Looks a tile up by name.</summary>
    /// <param name="name">The tile's name.</param>
    /// <param name="id">Its id, from 1, if there's a tile called that.</param>
    /// <returns>True if there is.</returns>
    public bool TryGetId(string name, out int id) => _ids.TryGetValue(name, out id);

    /// <summary>Which frame of the sheet one variant of a tile is.</summary>
    /// <param name="id">The tile, from 1.</param>
    /// <param name="pick">Any number. It wraps round the tile's variants.</param>
    public int FrameOf(int id, int pick) => _firstFrames[id - 1] + (int)((uint)pick % (uint)Tiles[id - 1].Variants);

    /// <summary>Checks the tileset and indexes it.</summary>
    /// <remarks>Throws on purpose, at startup. A tile with no name or no variants is a typo in the catalogue.</remarks>
    public void Validate()
    {
        var ids = new Dictionary<string, int>();
        var firsts = new int[Tiles.Count];
        int frame = 0;

        for (int i = 0; i < Tiles.Count; i++)
        {
            TileDefinition tile = Tiles[i];

            if (string.IsNullOrWhiteSpace(tile.Name))
                throw new InvalidOperationException($"Tileset {Id}: tile {i + 1} has no name.");
            if (tile.Variants < 1)
                throw new InvalidOperationException($"Tileset {Id}: {tile.Name} has {tile.Variants} variants. It needs at least one.");
            if (!ids.TryAdd(tile.Name, i + 1))
                throw new InvalidOperationException($"Tileset {Id}: two tiles are called {tile.Name}.");

            firsts[i] = frame;
            frame += tile.Variants;
        }

        _ids = ids;
        _firstFrames = firsts;
        FrameCount = frame;
    }
}
