using System;
using System.Collections.Generic;

namespace LegendQuest.World;

/// <summary>
/// One tileset: the sheet it's cut from, and every tile on it in order.
/// </summary>
/// <remarks>
/// A tile's id is its place in Tiles counting from 1, same as the tilemap exercise (0 is an empty
/// cell). The sheet has to have the tiles in this order, which tools/generate_world.py (or
/// tools/cut_supplied.py, for Questeria) makes sure of by working from the same list. Tilemap
/// checks the sheet is the right size when it loads.
/// </remarks>
public sealed class TilesetDefinition
{
    /// <summary>What map files call it.</summary>
    public required string Id { get; init; }

    /// <summary>The sheet, by content name.</summary>
    public required string Texture { get; init; }

    /// <summary>How many texels across a tile is on its sheet: 16 for a set drawn a pixel at a time, 64 for one painted by tools/rooms/paint_tiles.py. A tile is Tilemap.TileSize world pixels either way, so a painted one is one texel to a screen pixel.</summary>
    public int TileTexels { get; init; } = 16;

    /// <summary>Every tile, in sheet order.</summary>
    public required IReadOnlyList<TileDefinition> Tiles { get; init; }

    /// <summary>Tile ids by name, and where each tile's first frame is on the sheet. Built by Validate.</summary>
    private Dictionary<string, int> _ids;
    private int[] _firstFrames;

    /// <summary>How many frames the sheet should have: every frame of every variant of every tile, every tile of every block.</summary>
    public int FrameCount { get; private set; }

    /// <summary>One tile, by id.</summary>
    /// <param name="id">From 1. 0 is an empty cell and has no tile.</param>
    public TileDefinition this[int id] => Tiles[id - 1];

    /// <summary>Looks a tile up by name.</summary>
    /// <param name="name">The tile's name.</param>
    /// <param name="id">Its id, from 1, if there's a tile called that.</param>
    /// <returns>True if there is.</returns>
    public bool TryGetId(string name, out int id) => _ids.TryGetValue(name, out id);

    /// <summary>Which frame of the sheet a tile shows in one cell of a map, at a moment.</summary>
    /// <remarks>
    /// The variant comes from where the cell is (or where its block is), so it's the same every time.
    /// The hash is the one The Black Box's lobby floor used; its low bits repeat, so it's shifted off them.
    /// A tile that moves counts its frames on from the clock, starting from a frame the same hash
    /// picks, so the cells next to each other aren't in step.
    /// </remarks>
    /// <param name="id">The tile, from 1.</param>
    /// <param name="x">The cell's column.</param>
    /// <param name="y">The cell's row.</param>
    /// <param name="seconds">How long the game's been running, for a tile that moves.</param>
    public int FrameOf(int id, int x, int y, float seconds)
    {
        TileDefinition tile = Tiles[id - 1];
        int block = tile.Block;
        int pick = (int)((uint)(x / block * 73856093 ^ y / block * 19349663) >> 7);
        int variant = (int)((uint)pick % (uint)tile.Variants);
        int step = tile.Frames == 1 ? 0 : ((int)(seconds / tile.FrameSeconds) + pick / tile.Variants) % tile.Frames;
        return _firstFrames[id - 1] + (variant * tile.Frames + step) * block * block + y % block * block + x % block;
    }

    /// <summary>Which frame of the sheet is one of a tile's edges.</summary>
    /// <param name="id">The tile, from 1. It has to be Edged.</param>
    /// <param name="side">0 north, 1 east, 2 south, 3 west.</param>
    public int EdgeFrame(int id, int side)
    {
        TileDefinition tile = Tiles[id - 1];
        return _firstFrames[id - 1] + tile.Variants * tile.Frames * tile.Block * tile.Block + side;
    }

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
            if (tile.Block < 1)
                throw new InvalidOperationException($"Tileset {Id}: {tile.Name} is a block of {tile.Block}. It has to be at least 1.");
            if (tile.Frames < 1)
                throw new InvalidOperationException($"Tileset {Id}: {tile.Name} has {tile.Frames} frames. It needs at least one.");
            if (tile.Frames > 1 && tile.FrameSeconds <= 0f)
                throw new InvalidOperationException($"Tileset {Id}: {tile.Name} moves, so it needs FrameSeconds, how long each frame is up.");
            if (!ids.TryAdd(tile.Name, i + 1))
                throw new InvalidOperationException($"Tileset {Id}: two tiles are called {tile.Name}.");

            firsts[i] = frame;
            frame += tile.Variants * tile.Frames * tile.Block * tile.Block + (tile.Edged ? 4 : 0);
        }

        _ids = ids;
        _firstFrames = firsts;
        FrameCount = frame;
    }
}
