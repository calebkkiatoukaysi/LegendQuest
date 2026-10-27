using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// Which tiles of a map can be walked on, for the pathfinder: not solid, and not covered by
/// something standing on them.
/// </summary>
/// <remarks>
/// Built from the same things the collision is: the solid tiles, and the footprints of props, shut
/// doors and people. A footprint only blocks a tile if it covers the middle of it; walking slides
/// round the corners of the rest, the same as it would without a path.
/// </remarks>
public class NavGrid
{
    /// <summary>How far in from a tile's edge its middle starts, in world pixels. A footprint has to reach into the middle to block the tile.</summary>
    private const float CoreInset = 4f;

    /// <summary>How far either side of a straight line the feet need clear to walk it, and how often along it to look.</summary>
    private const float HalfWidth = 5f;
    private const float LineStep = 4f;

    private readonly Tilemap _tiles;
    private readonly bool[,] _blocked;

    /// <summary>How many tiles across.</summary>
    public int Width { get; }

    /// <summary>How many tiles down.</summary>
    public int Height { get; }

    /// <summary>Sets up an empty grid the size of a map. Build fills it in.</summary>
    /// <param name="tiles">The map.</param>
    public NavGrid(Tilemap tiles)
    {
        _tiles = tiles;
        Width = tiles.Bounds.Width / Tilemap.TileSize;
        Height = tiles.Bounds.Height / Tilemap.TileSize;
        _blocked = new bool[Width, Height];
    }

    /// <summary>Works out which tiles are blocked: the solid ones, and any whose middle a footprint covers.</summary>
    /// <remarks>Cheap enough to do again whenever a door opens or shuts.</remarks>
    /// <param name="footprints">Everything standing on the map that nobody can walk through.</param>
    public void Build(IEnumerable<BoundingRectangle> footprints)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                _blocked[x, y] = _tiles.IsSolid(x, y);

        const int tile = Tilemap.TileSize;
        foreach (BoundingRectangle footprint in footprints)
        {
            for (int y = (int)(footprint.Top / tile); y <= (int)(footprint.Bottom / tile); y++)
            {
                for (int x = (int)(footprint.Left / tile); x <= (int)(footprint.Right / tile); x++)
                {
                    if (x < 0 || y < 0 || x >= Width || y >= Height) continue;

                    var core = new BoundingRectangle(x * tile + CoreInset, y * tile + CoreInset, tile - 2 * CoreInset, tile - 2 * CoreInset);
                    if (footprint.CollidesWith(core)) _blocked[x, y] = true;
                }
            }
        }
    }

    /// <summary>Whether a tile can be walked on. Off the map can't.</summary>
    /// <param name="cell">The tile.</param>
    public bool IsWalkable(Point cell) =>
        cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height && !_blocked[cell.X, cell.Y];

    /// <summary>Whether feet could walk straight from one point to another without leaving walkable tiles.</summary>
    /// <remarks>It looks along the middle of the line and either side of it, the width of the feet, every few pixels.</remarks>
    /// <param name="from">One end, in world pixels.</param>
    /// <param name="to">The other end.</param>
    public bool LineClear(Vector2 from, Vector2 to)
    {
        Vector2 along = to - from;
        float length = along.Length();
        if (length < 0.001f) return IsWalkable(Tilemap.CellOf(from));

        along /= length;
        var side = new Vector2(-along.Y, along.X) * HalfWidth;

        for (float d = 0f; d <= length; d += LineStep)
        {
            Vector2 at = from + along * d;
            if (!IsWalkable(Tilemap.CellOf(at)) || !IsWalkable(Tilemap.CellOf(at + side)) || !IsWalkable(Tilemap.CellOf(at - side)))
                return false;
        }

        return IsWalkable(Tilemap.CellOf(to));
    }

    /// <summary>The middle of a tile, in world pixels.</summary>
    /// <param name="cell">The tile.</param>
    public static Vector2 Middle(Point cell) => new((cell.X + 0.5f) * Tilemap.TileSize, (cell.Y + 0.5f) * Tilemap.TileSize);
}
