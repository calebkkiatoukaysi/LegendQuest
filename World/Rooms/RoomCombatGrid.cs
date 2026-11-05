using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A room's floor as a fight's grid: columns across the room's fight range, and its floor band
/// split into rows from the back to the front.
/// </summary>
/// <remarks>
/// <para>
/// M3's interim ground, so fights can be fought in the rooms before M4 lines them up in rows
/// (Story/11-combat.md §1), when M4's Formation replaces it. The columns are ColumnWidth wide, as
/// many as it takes to cover the fight range, centred on it. Each column's band runs from the
/// floor's back edge to its front edge at the column's middle and is split into Rows rows, so a
/// sloping floor slopes its cells with it. Feet stand at the middle of a cell. The foes who turn up
/// (EncounterCatalog's Spawn cells) do so on its right third, the side M4 gives them.
/// </para>
/// <para>
/// A cell can't be stood on if its middle is off the floor (outside the band, or in one of its
/// ellipses), or if a footprint covers the middle of it, NavGrid's rule: the props, the shut doors,
/// and whoever's standing about who isn't in the fight. Where the fighters stand is the engine's
/// business, not the grid's. Nothing on a floor stops a line of sight (a fountain doesn't), but off
/// the ground does. It's laid when the fight starts, so the screen can find everyone's cells, and
/// built once it's known who's fighting, since nothing in the room moves but them while it's on.
/// </para>
/// </remarks>
public sealed class RoomCombatGrid : IFightGround
{
    /// <summary>How wide a column is, in world pixels: 96 canvas pixels.</summary>
    public const float ColumnWidth = 24f;

    /// <summary>How many rows the floor band is split into.</summary>
    public const int Rows = 4;

    /// <summary>How far in from a cell's edges its middle starts, as a share of the cell, as NavGrid has it. A footprint has to reach into the middle to block the cell.</summary>
    private const float CoreInset = 0.25f;

    private readonly RoomDefinition _room;

    /// <summary>Where the first column's left edge is, in world pixels.</summary>
    private readonly float _left;

    /// <summary>Each column's band: where its back row starts, and how deep each row is, in world pixels.</summary>
    private readonly float[] _back;
    private readonly float[] _rowDepth;

    private readonly bool[,] _blocked;

    /// <summary>Lays the grid over a room's fight range. Nothing's solid until Build.</summary>
    /// <param name="room">The room the fight is in.</param>
    public RoomCombatGrid(RoomDefinition room)
    {
        _room = room;
        float range = room.FightRight - room.FightLeft;
        Width = Math.Max(1, (int)MathF.Ceiling(range / ColumnWidth));
        _left = room.FightLeft + (range - Width * ColumnWidth) / 2f;

        _back = new float[Width];
        _rowDepth = new float[Width];
        for (int x = 0; x < Width; x++)
        {
            float middle = _left + (x + 0.5f) * ColumnWidth;
            _back[x] = room.BackEdgeAt(middle);
            _rowDepth[x] = (room.FrontEdgeAt(middle) - _back[x]) / Rows;
        }

        _blocked = new bool[Width, Rows];
    }

    /// <summary>Works out which cells can't be stood on: those off the floor, and any whose middle a footprint covers.</summary>
    /// <param name="footprints">Everything standing in the room that isn't in the fight and can't be walked through.</param>
    public void Build(IEnumerable<BoundingRectangle> footprints)
    {
        for (int y = 0; y < Rows; y++)
            for (int x = 0; x < Width; x++)
                _blocked[x, y] = !_room.OnFloor(FeetOn(new Point(x, y)));

        foreach (BoundingRectangle footprint in footprints)
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    BoundingRectangle area = Area(new Point(x, y));
                    float insetX = area.Width * CoreInset, insetY = area.Height * CoreInset;
                    var middle = new BoundingRectangle(area.X + insetX, area.Y + insetY, area.Width - 2f * insetX, area.Height - 2f * insetY);
                    if (footprint.CollidesWith(middle)) _blocked[x, y] = true;
                }
            }
        }
    }

    /// <summary>How many columns across.</summary>
    public int Width { get; }

    /// <summary>How many rows down: Rows.</summary>
    public int Height => Rows;

    /// <summary>Whether a cell can be stood on: on the ground, its middle on the floor, and no footprint over it.</summary>
    /// <param name="cell">The cell.</param>
    public bool Walkable(Point cell) => OnGround(cell) && !_blocked[cell.X, cell.Y];

    /// <summary>Whether a cell stops a line of sight: only off the ground.</summary>
    /// <param name="cell">The cell.</param>
    public bool BlocksSight(Point cell) => !OnGround(cell);

    /// <summary>Which cell a point is in, by its column and then the rows of that column's band. A point off the ground gives a cell off it.</summary>
    /// <param name="world">The point, in world pixels.</param>
    public Point CellOf(Vector2 world)
    {
        int x = (int)MathF.Floor((world.X - _left) / ColumnWidth);
        (float back, float rowDepth) = Band(x);
        return new Point(x, (int)MathF.Floor((world.Y - back) / rowDepth));
    }

    /// <summary>Where someone's feet go to stand on a cell: its middle, on the band.</summary>
    /// <param name="cell">The cell.</param>
    public Vector2 FeetOn(Point cell)
    {
        BoundingRectangle area = Area(cell);
        return new Vector2(area.X + area.Width / 2f, area.Y + area.Height / 2f);
    }

    /// <summary>The patch of floor a cell covers: a column wide and a row of its band deep, in world pixels.</summary>
    /// <param name="cell">The cell. One off the ground is laid out as the nearest column's band goes on.</param>
    public BoundingRectangle Area(Point cell)
    {
        (float back, float rowDepth) = Band(cell.X);
        return new BoundingRectangle(_left + cell.X * ColumnWidth, back + cell.Y * rowDepth, ColumnWidth, rowDepth);
    }

    /// <summary>Whether a cell is on the grid at all.</summary>
    private bool OnGround(Point cell) => cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Rows;

    /// <summary>A column's band, or the nearest column's for one off either end.</summary>
    private (float Back, float RowDepth) Band(int column)
    {
        int x = Math.Clamp(column, 0, Width - 1);
        return (_back[x], _rowDepth[x]);
    }
}
