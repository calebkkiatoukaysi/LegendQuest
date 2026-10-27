using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A map as something to draw, walk into and see across: its tiles cut from the tileset, drawn in
/// three layers, the walls built from the solid ones, and which cells stop light and sight.
/// </summary>
/// <remarks>
/// This is the tilemap exercise's Tilemap: cut the tileset into source rectangles in LoadContent,
/// then draw the map in a double loop. What's new is the three layers, picking a variant per cell,
/// only drawing the tiles that are on screen, the walls, and the line of sight.
/// </remarks>
public class Tilemap
{
    /// <summary>One tile, in art pixels.</summary>
    public const int TileSize = 16;

    private readonly MapDefinition _map;
    private readonly List<BoundingRectangle> _walls = new();

    /// <summary>Cells a shut door is standing in. They stop light and sight like a wall until it opens.</summary>
    private readonly HashSet<Point> _shut = new();

    private Texture2D _sheet;
    private Rectangle[] _frames;

    /// <summary>The whole map, in world pixels.</summary>
    public Rectangle Bounds => new(0, 0, _map.Width * TileSize, _map.Height * TileSize);

    /// <summary>Everything nobody can walk through: runs of solid tiles, and a wall all the way round outside the edge.</summary>
    public IReadOnlyList<BoundingRectangle> Walls => _walls;

    /// <summary>Builds the walls for a map. The tiles themselves load in LoadContent.</summary>
    /// <param name="map">The map.</param>
    public Tilemap(MapDefinition map)
    {
        _map = map;
        BuildWalls();
    }

    /// <summary>Loads the tileset's sheet and cuts it into one source rectangle per tile.</summary>
    /// <remarks>Throws if the sheet hasn't got every tile the tileset lists. That means the generator and TilesetCatalog don't match.</remarks>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        TilesetDefinition tileset = _map.Tileset;
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

    /// <summary>Draws the three layers of whatever is on screen.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space, point-sampled, back to front.</param>
    /// <param name="view">The part of the map on screen, in world pixels.</param>
    /// <param name="tint">Colour multiplied into every tile. White for the map itself; the map screen browns it.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle view, Color tint)
    {
        int x0 = Math.Max(0, view.Left / TileSize), x1 = Math.Min(_map.Width - 1, view.Right / TileSize);
        int y0 = Math.Max(0, view.Top / TileSize), y1 = Math.Min(_map.Height - 1, view.Bottom / TileSize);

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                DrawTile(spriteBatch, _map.Ground[x, y], x, y, tint, Layers.GroundTile);
                DrawTile(spriteBatch, _map.Detail[x, y], x, y, tint, Layers.DetailTile);
                DrawTile(spriteBatch, _map.Overhead[x, y], x, y, tint, Layers.Overhead);
            }
        }
    }

    /// <summary>What the ground is like underfoot at a point, for the footsteps.</summary>
    /// <param name="world">The point, in world pixels.</param>
    public Surface SurfaceAt(Vector2 world)
    {
        Point cell = CellOf(world);
        if (cell.X < 0 || cell.Y < 0 || cell.X >= _map.Width || cell.Y >= _map.Height) return Surface.Stone;
        return _map.Tileset[_map.Ground[cell.X, cell.Y]].Surface;
    }

    /// <summary>Whether a cell stops people walking: its ground or its detail is solid. Off the map counts as solid.</summary>
    /// <param name="x">The cell's column.</param>
    /// <param name="y">The cell's row.</param>
    public bool IsSolid(int x, int y)
    {
        if (x < 0 || y < 0 || x >= _map.Width || y >= _map.Height) return true;

        TilesetDefinition tiles = _map.Tileset;
        int detail = _map.Detail[x, y];
        return tiles[_map.Ground[x, y]].Solid || detail != 0 && tiles[detail].Solid;
    }

    /// <summary>Whether a cell stops light and sight: an opaque tile, or a shut door. Off the map counts.</summary>
    /// <param name="x">The cell's column.</param>
    /// <param name="y">The cell's row.</param>
    public bool BlocksSight(int x, int y)
    {
        if (x < 0 || y < 0 || x >= _map.Width || y >= _map.Height) return true;
        if (_shut.Contains(new Point(x, y))) return true;

        TilesetDefinition tiles = _map.Tileset;
        int detail = _map.Detail[x, y];
        return tiles[_map.Ground[x, y]].Opaque || detail != 0 && tiles[detail].Opaque;
    }

    /// <summary>Marks the cells a door stands in as shut (they stop light and sight) or open.</summary>
    /// <param name="cells">The cells, in tiles.</param>
    /// <param name="shut">True if the door is shut.</param>
    public void Shut(Rectangle cells, bool shut)
    {
        for (int y = cells.Top; y < cells.Bottom; y++)
        {
            for (int x = cells.Left; x < cells.Right; x++)
            {
                if (shut) _shut.Add(new Point(x, y));
                else _shut.Remove(new Point(x, y));
            }
        }
    }

    /// <summary>Whether anything that blocks sight is on the line between two cells, not counting the two ends.</summary>
    /// <remarks>Bresenham, walked across the tiles. The ends don't count so a wall you're looking at is still seen, and so is a torch on one.</remarks>
    /// <param name="from">One end, in tiles.</param>
    /// <param name="to">The other end.</param>
    public bool LineBlocked(Point from, Point to)
    {
        int x0 = from.X, y0 = from.Y, x1 = to.X, y1 = to.Y;
        int dx = Math.Abs(x1 - x0), dy = -Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;
        int x = x0, y = y0;

        while (x != x1 || y != y1)
        {
            int doubled = 2 * error;
            if (doubled >= dy) { error += dy; x += sx; }
            if (doubled <= dx) { error += dx; y += sy; }

            if ((x != x1 || y != y1) && BlocksSight(x, y)) return true;
        }

        return false;
    }

    /// <summary>The cell a point is in.</summary>
    /// <param name="world">The point, in world pixels.</param>
    public static Point CellOf(Vector2 world) =>
        new((int)MathF.Floor(world.X / TileSize), (int)MathF.Floor(world.Y / TileSize));

    /// <summary>Draws one cell of one layer, unless it's empty.</summary>
    private void DrawTile(SpriteBatch spriteBatch, int tile, int x, int y, Color tint, float layerDepth)
    {
        if (tile == 0) return;

        Rectangle source = _frames[_map.Tileset.FrameOf(tile, Pick(x, y))];
        spriteBatch.Draw(_sheet, new Vector2(x * TileSize, y * TileSize), source, tint,
            0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth);
    }

    /// <summary>A number from a cell's place, the same every time, for picking which variant of a tile it gets.</summary>
    /// <remarks>The same hash The Black Box's lobby floor used. The low bits repeat, so it's shifted off them.</remarks>
    private static int Pick(int x, int y) => (int)((uint)(x * 73856093 ^ y * 19349663) >> 7);

    /// <summary>Merges each row's runs of solid cells into one rectangle each, and walls the outside of the map in.</summary>
    /// <remarks>Same as the lobby: a few dozen rectangles instead of one per tile.</remarks>
    private void BuildWalls()
    {
        for (int y = 0; y < _map.Height; y++)
        {
            int runStart = -1;
            for (int x = 0; x <= _map.Width; x++)
            {
                bool solid = x < _map.Width && IsSolid(x, y);
                if (solid && runStart < 0) runStart = x;
                if (!solid && runStart >= 0)
                {
                    _walls.Add(new BoundingRectangle(runStart * TileSize, y * TileSize, (x - runStart) * TileSize, TileSize));
                    runStart = -1;
                }
            }
        }

        // One tile thick round the outside, so nobody walks off the edge.
        float w = _map.Width * TileSize, h = _map.Height * TileSize;
        _walls.Add(new BoundingRectangle(-TileSize, -TileSize, w + 2 * TileSize, TileSize));
        _walls.Add(new BoundingRectangle(-TileSize, h, w + 2 * TileSize, TileSize));
        _walls.Add(new BoundingRectangle(-TileSize, 0f, TileSize, h));
        _walls.Add(new BoundingRectangle(w, 0f, TileSize, h));
    }
}
