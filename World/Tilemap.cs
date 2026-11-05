using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.World;

/// <summary>
/// A room's tile strip as something to draw and walk on: its tiles cut from the tileset and drawn in
/// two layers, the ground under the floor band (and an interior's back wall) and what lies on it.
/// </summary>
/// <remarks>
/// This is the tilemap exercise's Tilemap: cut the tileset into source rectangles in LoadContent,
/// then draw the tiles in a double loop. What's new is the two layers, picking a variant per cell
/// (and the part of a block), the edges where one kind of ground meets another, the animated water,
/// and only drawing the tiles that are on screen. The strip is drawn at an origin partway down its
/// room, and has no walls: the room's floor band says where feet go.
/// </remarks>
public class Tilemap
{
    /// <summary>One tile, in art pixels.</summary>
    public const int TileSize = 16;

    private readonly TilesetDefinition _tileset;
    private readonly int[,] _ground;
    private readonly int[,] _detail;
    private readonly int _width;
    private readonly int _height;

    /// <summary>Where the top left of the first tile is, in world pixels: how far down its room the strip starts.</summary>
    private readonly Point _origin;

    private Texture2D _sheet;
    private Rectangle[] _frames;

    /// <summary>Gets a room's tile strip ready to draw, its top left at an origin. The tiles load in LoadContent.</summary>
    /// <param name="strip">The strip's tiles.</param>
    /// <param name="origin">Where its top left goes, in world pixels: (0, the room's TilesTop).</param>
    public Tilemap(TileLayers strip, Point origin)
    {
        _tileset = strip.Tileset;
        _ground = strip.Ground;
        _detail = strip.Detail;
        _width = strip.Ground.GetLength(0);
        _height = strip.Ground.GetLength(1);
        _origin = origin;
    }

    /// <summary>Loads the tileset's sheet and cuts it into one source rectangle per tile.</summary>
    /// <remarks>Throws if the sheet hasn't got every tile the tileset lists. That means the generator and TilesetCatalog don't match.</remarks>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        TilesetDefinition tileset = _tileset;
        _sheet = content.Load<Texture2D>(tileset.Texture);

        int columns = _sheet.Width / TileSize;
        int rows = _sheet.Height / TileSize;
        if (columns * rows < tileset.FrameCount)
            throw new InvalidOperationException($"{tileset.Texture}.png has room for {columns * rows} tiles; the {tileset.Id} tileset needs {tileset.FrameCount}. Run tools/generate_world.py.");

        _frames = new Rectangle[columns * rows];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
                _frames[y * columns + x] = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);
        }
    }

    /// <summary>Draws both layers of whatever is on screen, the tiles that move on the frame the clock says.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space, point-sampled, back to front.</param>
    /// <param name="view">The part of the world on screen, in world pixels.</param>
    /// <param name="tint">Colour multiplied into every tile: the room's tint.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle view, Color tint)
    {
        Point first = CellOf(new Vector2(view.Left - _origin.X, view.Top - _origin.Y));
        Point last = CellOf(new Vector2(view.Right - _origin.X, view.Bottom - _origin.Y));
        int x0 = Math.Max(0, first.X), x1 = Math.Min(_width - 1, last.X);
        int y0 = Math.Max(0, first.Y), y1 = Math.Min(_height - 1, last.Y);
        float seconds = (float)gameTime.TotalGameTime.TotalSeconds;

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                DrawTile(spriteBatch, _ground[x, y], x, y, seconds, tint, Layers.GroundTile);
                DrawEdges(spriteBatch, x, y, tint);
                DrawTile(spriteBatch, _detail[x, y], x, y, seconds, tint, Layers.DetailTile);
            }
        }
    }

    /// <summary>What the ground is like underfoot at a point, for the footsteps.</summary>
    /// <param name="world">The point, in world pixels.</param>
    public Surface SurfaceAt(Vector2 world)
    {
        Point cell = CellOf(world - _origin.ToVector2());
        if (cell.X < 0 || cell.Y < 0 || cell.X >= _width || cell.Y >= _height) return Surface.Stone;
        return _tileset[_ground[cell.X, cell.Y]].Surface;
    }

    /// <summary>The cell a point is in, measured from the strip's origin.</summary>
    /// <param name="local">The point, in world pixels from the strip's top left.</param>
    private static Point CellOf(Vector2 local) =>
        new((int)MathF.Floor(local.X / TileSize), (int)MathF.Floor(local.Y / TileSize));

    /// <summary>Draws one cell of one layer, unless it's empty.</summary>
    private void DrawTile(SpriteBatch spriteBatch, int tile, int x, int y, float seconds, Color tint, float layerDepth)
    {
        if (tile == 0) return;

        Rectangle source = _frames[_tileset.FrameOf(tile, x, y, seconds)];
        spriteBatch.Draw(_sheet, CellCorner(x, y), source, tint, 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
    }

    /// <summary>Draws a ground tile's edges along whichever of its sides meet ground of another kind. Not along the edge of the strip.</summary>
    private void DrawEdges(SpriteBatch spriteBatch, int x, int y, Color tint)
    {
        int tile = _ground[x, y];
        if (tile == 0 || !_tileset[tile].Edged) return;

        for (int side = 0; side < 4; side++)
        {
            int nx = x + Across[side], ny = y + Down[side];
            if (nx < 0 || ny < 0 || nx >= _width || ny >= _height) continue;

            int next = _ground[nx, ny];
            if (next == 0 || next == tile) continue;

            Rectangle source = _frames[_tileset.EdgeFrame(tile, side)];
            spriteBatch.Draw(_sheet, CellCorner(x, y), source, tint, 0f, Vector2.Zero, 1f, SpriteEffects.None, Layers.GroundEdge);
        }
    }

    /// <summary>Where a cell's top left is in the world.</summary>
    private Vector2 CellCorner(int x, int y) => new(_origin.X + x * TileSize, _origin.Y + y * TileSize);

    /// <summary>Which way each side is: north, east, south, west.</summary>
    private static readonly int[] Across = { 0, 1, 0, -1 };
    private static readonly int[] Down = { -1, 0, 1, 0 };
}
