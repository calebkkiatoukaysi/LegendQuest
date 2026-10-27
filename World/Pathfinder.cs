using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// A* over a NavGrid: the shortest way from one point to another, or to anywhere close enough to it.
/// </summary>
/// <remarks>
/// Not from a tutorial; it's the usual A*. Eight directions, but never diagonally past the corner
/// of a blocked tile, since the feet would catch on it. A straight step costs 1 and a diagonal √2,
/// so paths come out the way someone would walk them, and the guess at the rest of the way is the
/// same measure with nothing in the way (octile distance), which never guesses too much, so the
/// path is the shortest. The open list is a BinaryHeap. The tile path then gets pulled straight
/// wherever the feet could walk a straight line, so you don't zigzag across an open square.
/// </remarks>
public static class Pathfinder
{
    /// <summary>The cost of a diagonal step.</summary>
    private static readonly float Diagonal = MathF.Sqrt(2f);

    /// <summary>The eight steps from a tile: the four straight ones first, then the diagonals.</summary>
    private static readonly Point[] Steps =
    {
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        new(1, 1), new(-1, 1), new(1, -1), new(-1, -1),
    };

    /// <summary>Finds a way from one point to another, or to within reach of it.</summary>
    /// <param name="grid">Where can be walked.</param>
    /// <param name="from">Where the feet are now, in world pixels.</param>
    /// <param name="to">Where to go, in world pixels.</param>
    /// <param name="reach">How close is close enough, in world pixels. 0 means the tile it's in, which has to be walkable.</param>
    /// <returns>The points to walk through, in order, not counting where you are; or null if there's no way there.</returns>
    public static List<Vector2> Find(NavGrid grid, Vector2 from, Vector2 to, float reach)
    {
        Point start = Tilemap.CellOf(from), target = Tilemap.CellOf(to);
        if (reach <= 0f && !grid.IsWalkable(target)) return null;

        bool Arrived(Point cell) => reach <= 0f ? cell == target : Vector2.Distance(NavGrid.Middle(cell), to) <= reach;
        float Guess(Point cell)
        {
            int dx = Math.Abs(cell.X - target.X), dy = Math.Abs(cell.Y - target.Y);
            float octile = Math.Max(dx, dy) + (Diagonal - 1f) * Math.Min(dx, dy);
            return MathF.Max(0f, octile - reach / Tilemap.TileSize);
        }

        int width = grid.Width, height = grid.Height;
        var cost = new float[width * height];
        var came = new int[width * height];
        var closed = new bool[width * height];
        Array.Fill(cost, float.MaxValue);

        var open = new BinaryHeap<Point>();
        cost[start.Y * width + start.X] = 0f;
        came[start.Y * width + start.X] = -1;
        open.Push(start, Guess(start));

        while (open.Count > 0)
        {
            Point cell = open.Pop();
            int here = cell.Y * width + cell.X;
            if (closed[here]) continue;
            closed[here] = true;

            if (Arrived(cell)) return Smooth(grid, from, Walk(came, here, width));

            for (int i = 0; i < Steps.Length; i++)
            {
                Point step = Steps[i];
                var next = new Point(cell.X + step.X, cell.Y + step.Y);
                if (!grid.IsWalkable(next)) continue;

                // No cutting a corner: a diagonal needs both of the straight tiles beside it clear.
                bool diagonal = step.X != 0 && step.Y != 0;
                if (diagonal && (!grid.IsWalkable(new Point(cell.X + step.X, cell.Y)) || !grid.IsWalkable(new Point(cell.X, cell.Y + step.Y))))
                    continue;

                int there = next.Y * width + next.X;
                float through = cost[here] + (diagonal ? Diagonal : 1f);
                if (closed[there] || through >= cost[there]) continue;

                cost[there] = through;
                came[there] = here;
                open.Push(next, through + Guess(next));
            }
        }

        return null;
    }

    /// <summary>Walks back from the end along where each tile was reached from, and turns it round.</summary>
    private static List<Point> Walk(int[] came, int end, int width)
    {
        var cells = new List<Point>();
        for (int i = end; i >= 0; i = came[i]) cells.Add(new Point(i % width, i / width));
        cells.Reverse();
        return cells;
    }

    /// <summary>Pulls a tile path straight: from each point, on to the furthest one ahead that the feet could walk straight to.</summary>
    private static List<Vector2> Smooth(NavGrid grid, Vector2 from, List<Point> cells)
    {
        var points = new List<Vector2>();
        Vector2 at = from;
        int i = 0;

        while (i < cells.Count - 1)
        {
            int furthest = i + 1;
            for (int j = cells.Count - 1; j > i + 1; j--)
            {
                if (grid.LineClear(at, NavGrid.Middle(cells[j])))
                {
                    furthest = j;
                    break;
                }
            }

            at = NavGrid.Middle(cells[furthest]);
            points.Add(at);
            i = furthest;
        }

        // Already standing in the end tile, so just go to the middle of it.
        if (points.Count == 0) points.Add(NavGrid.Middle(cells[^1]));
        return points;
    }
}
