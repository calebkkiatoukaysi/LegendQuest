using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// Which cells of a grid laid over the world can be walked on, for the pathfinder: not solid, and
/// not covered by something standing on them.
/// </summary>
/// <remarks>
/// The grid is any size of square cell from any corner: RoomWalker lays one over a room's floor band.
/// What's solid is whatever the grid's told when it's made (every cell whose middle is off the
/// floor), and Build adds the footprints of props and people on top. A footprint only blocks a cell if it covers
/// the middle of it; walking slides round the corners of the rest, the same as it would without a
/// path.
/// </remarks>
public class NavGrid
{
    /// <summary>How far in from a cell's edge its middle starts, as a share of the cell. A footprint has to reach into the middle to block the cell.</summary>
    private const float CoreInset = 0.25f;

    /// <summary>How often along a straight line to look, in world pixels.</summary>
    private const float LineStep = 4f;

    private readonly Func<Point, bool> _solid;
    private readonly bool[,] _blocked;

    /// <summary>How many cells across.</summary>
    public int Width { get; }

    /// <summary>How many cells down.</summary>
    public int Height { get; }

    /// <summary>How big a cell is, in world pixels.</summary>
    public float CellSize { get; }

    /// <summary>Where the top left of the first cell is, in world pixels.</summary>
    public Vector2 Origin { get; }

    /// <summary>How far either side of a straight line the feet need clear to walk it, in world pixels.</summary>
    public float Clearance { get; }

    /// <summary>Sets up an empty grid. Build fills it in.</summary>
    /// <param name="width">How many cells across.</param>
    /// <param name="height">How many cells down.</param>
    /// <param name="cellSize">How big a cell is, in world pixels.</param>
    /// <param name="origin">Where the top left of the first cell is, in world pixels.</param>
    /// <param name="clearance">How far either side of a straight line the feet need clear to walk it, in world pixels. 0 looks along the line alone.</param>
    /// <param name="solid">Whether a cell can never be walked on, whatever's standing where.</param>
    public NavGrid(int width, int height, float cellSize, Vector2 origin, float clearance, Func<Point, bool> solid)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        Origin = origin;
        Clearance = clearance;
        _solid = solid;
        _blocked = new bool[width, height];
    }

    /// <summary>Works out which cells are blocked: the solid ones, and any whose middle a footprint covers.</summary>
    /// <remarks>Cheap enough to do again whenever a door opens or shuts, or anyone comes or goes.</remarks>
    /// <param name="footprints">Everything standing in the world that nobody can walk through.</param>
    public void Build(IEnumerable<BoundingRectangle> footprints)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                _blocked[x, y] = _solid(new Point(x, y));

        float inset = CellSize * CoreInset, core = CellSize - 2f * inset;
        foreach (BoundingRectangle footprint in footprints)
        {
            Point first = CellOf(new Vector2(footprint.Left, footprint.Top));
            Point last = CellOf(new Vector2(footprint.Right, footprint.Bottom));
            for (int y = Math.Max(first.Y, 0); y <= Math.Min(last.Y, Height - 1); y++)
            {
                for (int x = Math.Max(first.X, 0); x <= Math.Min(last.X, Width - 1); x++)
                {
                    var middle = new BoundingRectangle(Origin.X + x * CellSize + inset, Origin.Y + y * CellSize + inset, core, core);
                    if (footprint.CollidesWith(middle)) _blocked[x, y] = true;
                }
            }
        }
    }

    /// <summary>Whether a cell can be walked on. Off the grid can't.</summary>
    /// <param name="cell">The cell.</param>
    public bool IsWalkable(Point cell) =>
        cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height && !_blocked[cell.X, cell.Y];

    /// <summary>Whether feet could walk straight from one point to another without leaving walkable cells.</summary>
    /// <remarks>It looks along the middle of the line, and either side of it by the clearance, every few pixels.</remarks>
    /// <param name="from">One end, in world pixels.</param>
    /// <param name="to">The other end.</param>
    public bool LineClear(Vector2 from, Vector2 to)
    {
        Vector2 along = to - from;
        float length = along.Length();
        if (length < 0.001f) return IsWalkable(CellOf(from));

        along /= length;
        var side = new Vector2(-along.Y, along.X) * Clearance;

        for (float d = 0f; d <= length; d += LineStep)
        {
            Vector2 at = from + along * d;
            if (!IsWalkable(CellOf(at))) return false;
            if (Clearance > 0f && (!IsWalkable(CellOf(at + side)) || !IsWalkable(CellOf(at - side)))) return false;
        }

        return IsWalkable(CellOf(to));
    }

    /// <summary>Which cell a point is in. A point off the grid gives a cell off it.</summary>
    /// <param name="world">The point, in world pixels.</param>
    public Point CellOf(Vector2 world) =>
        new((int)MathF.Floor((world.X - Origin.X) / CellSize), (int)MathF.Floor((world.Y - Origin.Y) / CellSize));

    /// <summary>The middle of a cell, in world pixels.</summary>
    /// <param name="cell">The cell.</param>
    public Vector2 MiddleOf(Point cell) => Origin + new Vector2((cell.X + 0.5f) * CellSize, (cell.Y + 0.5f) * CellSize);
}
