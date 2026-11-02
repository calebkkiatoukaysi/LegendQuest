using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// A grid of item slots, like the bag or a shop's shelf: each place an icon in its well, how many
/// there are in the corner, and a line under it if the grid's given room for one (a price). The rows
/// past what fits scroll, with a bar beside them to say so.
/// </summary>
/// <remarks>
/// It only lays out and draws what the screen fills it with; the screen decides what's in it and
/// what a click does. A place is an index into what it was filled with, counted along the rows from
/// the top left, and the screen keeps the one the keys are on: Step moves it, Reveal scrolls to it,
/// and EnterFrom picks where the keys land coming in from beside the grid. The cells go in a point
/// sampled batch and the words in a linear one after, the same split as everywhere else.
/// </remarks>
public class ItemGrid
{
    /// <summary>The room between one slot and the next, in screen pixels.</summary>
    private const int Gap = 4;

    /// <summary>The scroll bar: its width, and how far right of the slots it is, in screen pixels.</summary>
    private const int BarWidth = 4;
    private const int BarGap = 8;

    /// <summary>How far in from a slot's right edge a count ends, and how far up from its bottom it starts, in screen pixels.</summary>
    private const float CountInset = 6f;
    private const float CountRise = 30f;

    /// <summary>How far under a slot its line goes, in screen pixels.</summary>
    private const float LabelDrop = 2f;

    /// <summary>How faded a greyed-out place's icon is.</summary>
    private const float DimAlpha = 0.4f;

    /// <summary>The bar's track and the part that says where you are: the well's dark and the slots' bronze.</summary>
    private static readonly Color BarTrack = new(14, 16, 36);
    private static readonly Color BarThumb = new(206, 162, 82);

    private readonly List<GridEntry> _entries = new();
    private readonly Point _origin;
    private readonly int _columns;
    private readonly int _rows;
    private readonly int _labelHeight;

    private Texture2D _pixel;

    /// <summary>Sets a grid out.</summary>
    /// <param name="origin">The top left of its first slot, in screen pixels.</param>
    /// <param name="columns">Slots across.</param>
    /// <param name="rows">Rows that show at once. More scroll.</param>
    /// <param name="labelHeight">The room under each slot for its line, in screen pixels, or 0 for none.</param>
    public ItemGrid(Point origin, int columns, int rows, int labelHeight)
    {
        _origin = origin;
        _columns = columns;
        _rows = rows;
        _labelHeight = labelHeight;
    }

    /// <summary>How many places are filled.</summary>
    public int Count => _entries.Count;

    /// <summary>The first row that shows.</summary>
    public int Scroll { get; private set; }

    /// <summary>The slots that show, from the top left of the first to the bottom of the last row's lines, in screen pixels.</summary>
    public Rectangle Bounds => new(_origin.X, _origin.Y, WidthOf(_columns), _rows * RowPitch - Gap);

    /// <summary>How far down one row is from the last, lines and all.</summary>
    private int RowPitch => ItemIcons.Size + _labelHeight + Gap;

    /// <summary>How many rows there are altogether, never fewer than show.</summary>
    private int TotalRows => Math.Max(_rows, (_entries.Count + _columns - 1) / _columns);

    /// <summary>How wide a grid with some columns is, so a screen can centre one before it's made.</summary>
    /// <param name="columns">Slots across.</param>
    /// <returns>The width, in screen pixels.</returns>
    public static int WidthOf(int columns) => columns * ItemIcons.Size + (columns - 1) * Gap;

    /// <summary>Takes the pixel the scroll bar's drawn with.</summary>
    /// <param name="pixel">One white pixel: the screen manager's BlankTexture.</param>
    public void LoadContent(Texture2D pixel) => _pixel = pixel;

    /// <summary>Fills the grid, in place of what was in it, and keeps the scroll where it was if there's still that much to scroll.</summary>
    /// <param name="entries">What goes in it, in order.</param>
    public void Fill(IEnumerable<GridEntry> entries)
    {
        _entries.Clear();
        _entries.AddRange(entries);
        Scroll = Math.Clamp(Scroll, 0, TotalRows - _rows);
    }

    /// <summary>Where a place's slot is on screen as it's scrolled now. It can be outside Bounds when it's scrolled out of sight.</summary>
    /// <param name="index">The place, from 0.</param>
    public Rectangle CellAt(int index)
    {
        int column = index % _columns, row = index / _columns - Scroll;
        return new Rectangle(_origin.X + column * (ItemIcons.Size + Gap), _origin.Y + row * RowPitch, ItemIcons.Size, ItemIcons.Size);
    }

    /// <summary>Which filled place a point is on, if it shows.</summary>
    /// <param name="point">Where, in screen pixels: the mouse.</param>
    /// <returns>The place, or -1 for none.</returns>
    public int IndexAt(Point point)
    {
        if (!Bounds.Contains(point)) return -1;
        for (int i = Scroll * _columns; i < _entries.Count && i < (Scroll + _rows) * _columns; i++)
            if (CellAt(i).Contains(point)) return i;
        return -1;
    }

    /// <summary>Scrolls by some rows, as far as there's anything to scroll to.</summary>
    /// <param name="rows">How many: down if it's more than 0, up if it's less.</param>
    public void ScrollBy(int rows) => Scroll = Math.Clamp(Scroll + rows, 0, TotalRows - _rows);

    /// <summary>Scrolls just far enough that a place shows.</summary>
    /// <param name="index">The place.</param>
    public void Reveal(int index)
    {
        int row = index / _columns;
        if (row < Scroll) Scroll = row;
        else if (row >= Scroll + _rows) Scroll = row - _rows + 1;
    }

    /// <summary>The place one step from another: across a column, down a row, or both.</summary>
    /// <remarks>Down onto a short last row lands on the last place, so the keys never fall into an empty slot.</remarks>
    /// <param name="index">Where from.</param>
    /// <param name="across">-1 left, 1 right, 0 neither.</param>
    /// <param name="down">-1 up, 1 down, 0 neither.</param>
    /// <returns>The place, or -1 if the step goes off the grid's edge (or off the end of what's in it), so the screen can take the keys somewhere else.</returns>
    public int Step(int index, int across, int down)
    {
        int column = index % _columns + across, row = index / _columns + down;
        if (column < 0 || column >= _columns || row < 0) return -1;

        int next = row * _columns + column;
        if (next < _entries.Count) return next;
        return down > 0 && row * _columns < _entries.Count ? _entries.Count - 1 : -1;
    }

    /// <summary>Where the keys land coming into the grid from beside it: the row that shows nearest to where they came from, at the edge they came in by.</summary>
    /// <param name="y">How far down the screen they came from, in screen pixels.</param>
    /// <param name="fromLeft">True if they came in from the left, so they land in the first column; false for the last.</param>
    /// <returns>The place, or -1 if the grid's empty.</returns>
    public int EnterFrom(float y, bool fromLeft)
    {
        if (_entries.Count == 0) return -1;

        int row = Scroll + Math.Clamp((int)((y - _origin.Y) / RowPitch), 0, _rows - 1);
        int first = row * _columns;
        if (first >= _entries.Count) return _entries.Count - 1;
        return fromLeft ? first : Math.Min(first + _columns - 1, _entries.Count - 1);
    }

    /// <summary>Draws every slot that shows (empty ones too), each filled one's item, and the scroll bar if there's more than shows. Goes in a point sampled, deferred batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="icons">The icons and wells.</param>
    /// <param name="focused">The place the keys or the mouse are on, lit; -1 for none.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawCells(SpriteBatch spriteBatch, ItemIcons icons, int focused, float alpha)
    {
        for (int i = Scroll * _columns; i < (Scroll + _rows) * _columns; i++)
        {
            Rectangle cell = CellAt(i);
            icons.DrawWell(spriteBatch, cell, i == focused && i < _entries.Count ? WellLook.Hover : WellLook.Plain, alpha);
            if (i < _entries.Count) icons.DrawItem(spriteBatch, cell, _entries[i].Item, alpha * (_entries[i].Dim ? DimAlpha : 1f));
        }

        if (TotalRows <= _rows) return;

        Rectangle bounds = Bounds;
        var track = new Rectangle(bounds.Right + BarGap, bounds.Top, BarWidth, bounds.Height);
        int thumbTop = track.Top + track.Height * Scroll / TotalRows;
        int thumbHeight = track.Height * _rows / TotalRows;
        spriteBatch.Draw(_pixel, track, BarTrack * alpha);
        spriteBatch.Draw(_pixel, new Rectangle(track.X, thumbTop, BarWidth, thumbHeight), BarThumb * alpha);
    }

    /// <summary>Writes how many there are in the corner of each slot that has more than one, and each line under its slot. Goes in a linear sampled batch, after DrawCells.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="font">The font to write with: the detail font.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawWords(SpriteBatch spriteBatch, SpriteFont font, float alpha)
    {
        for (int i = Scroll * _columns; i < _entries.Count && i < (Scroll + _rows) * _columns; i++)
        {
            GridEntry entry = _entries[i];
            Rectangle cell = CellAt(i);
            if (entry.Count > 1)
                Text.Draw(spriteBatch, font, entry.Count.ToString(CultureInfo.InvariantCulture), cell.Right - CountInset, cell.Bottom - CountRise,
                    Palette.Bone * alpha, Palette.Shadow * alpha, rightAligned: true);
            if (_labelHeight > 0 && entry.Label is not null)
                Text.Draw(spriteBatch, font, entry.Label, cell.Center.X, cell.Bottom + LabelDrop, entry.LabelColour * alpha, Palette.Shadow * alpha, centred: true);
        }
    }
}
