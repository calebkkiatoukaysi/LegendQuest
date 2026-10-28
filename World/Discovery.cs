using System;
using Microsoft.Xna.Framework;
using LegendQuest.Save;

namespace LegendQuest.World;

/// <summary>
/// Which tiles of one map have been in sight, for the map screen. One bit per tile, kept in the save.
/// </summary>
/// <remarks>
/// The bits are the save's own array, so there's nothing to copy back before saving. Sight is a
/// circle round your feet that walls and shut doors cut off, using the same line as the dark map's
/// light. Not from a tutorial. It only looks again when you step into another tile. A map with fog
/// = no in its file is seen all over from the start.
/// </remarks>
public class Discovery
{
    /// <summary>How far you can see, in tiles. A bit less than a screen at the normal zoom, so the map fills in as you walk.</summary>
    private const int SightRadius = 9;

    private readonly Tilemap _tiles;
    private readonly int _width;
    private readonly int _height;
    private readonly byte[] _bits;

    /// <summary>The tile it last looked from, so it doesn't look again until you move to another.</summary>
    private Point? _lookedFrom;

    /// <summary>Picks up what the save has already seen of a map, or starts it fresh.</summary>
    /// <remarks>A map that's changed size since the save was made starts fresh too, rather than showing the wrong tiles.</remarks>
    /// <param name="map">The map.</param>
    /// <param name="tiles">The map's tiles, for what blocks sight.</param>
    /// <param name="game">The save. Having an entry in it is what makes the map discovered on the world map.</param>
    public Discovery(MapDefinition map, Tilemap tiles, SaveData game)
    {
        _tiles = tiles;
        _width = map.Width;
        _height = map.Height;

        int length = (_width * _height + 7) / 8;
        if (!game.Seen.TryGetValue(map.Id, out _bits) || _bits?.Length != length)
        {
            _bits = new byte[length];
            game.Seen[map.Id] = _bits;
        }

        if (!map.Fog) Array.Fill(_bits, byte.MaxValue);
    }

    /// <summary>Whether a tile has ever been in sight.</summary>
    /// <param name="x">The tile's column.</param>
    /// <param name="y">The tile's row.</param>
    public bool IsSeen(int x, int y)
    {
        int i = y * _width + x;
        return (_bits[i >> 3] & 1 << (i & 7)) != 0;
    }

    /// <summary>Marks everything in sight of a point as seen.</summary>
    /// <param name="feet">Where you're standing, in world pixels.</param>
    public void LookAround(Vector2 feet)
    {
        Point from = Tilemap.CellOf(feet);
        if (from == _lookedFrom) return;
        _lookedFrom = from;

        for (int y = from.Y - SightRadius; y <= from.Y + SightRadius; y++)
        {
            for (int x = from.X - SightRadius; x <= from.X + SightRadius; x++)
            {
                if (x < 0 || y < 0 || x >= _width || y >= _height) continue;

                int dx = x - from.X, dy = y - from.Y;
                if (dx * dx + dy * dy > SightRadius * SightRadius) continue;
                if (IsSeen(x, y) || _tiles.LineBlocked(from, new Point(x, y))) continue;

                int i = y * _width + x;
                _bits[i >> 3] |= (byte)(1 << (i & 7));
            }
        }
    }

    /// <summary>Forgets where it last looked from, so the next LookAround looks again even from the same tile.</summary>
    /// <remarks>For when a door opens and there's suddenly more to see.</remarks>
    public void LookAgain() => _lookedFrom = null;
}
