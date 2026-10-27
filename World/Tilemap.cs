using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A map as something to draw and walk into: its tiles cut from the tileset, drawn in three
/// layers, and the walls built from the solid ones.
/// </summary>
/// <remarks>
/// This is the tilemap exercise's Tilemap: cut the tileset into source rectangles in LoadContent,
/// then draw the map in a double loop. What's new is the three layers, picking a variant per cell,
/// only drawing the tiles that are on screen, and the walls.
/// </remarks>
public class Tilemap
{
    /// <summary>One tile, in art pixels.</summary>
    public const int TileSize = 16;

    private readonly MapDefinition _map;
    private readonly List<BoundingRectangle> _walls = new();

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
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Rectangle view)
    {
        int x0 = Math.Max(0, view.Left / TileSize), x1 = Math.Min(_map.Width - 1, view.Right / TileSize);
        int y0 = Math.Max(0, view.Top / TileSize), y1 = Math.Min(_map.Height - 1, view.Bottom / TileSize);

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                DrawTile(spriteBatch, _map.Ground[x, y], x, y, Layers.GroundTile);
                DrawTile(spriteBatch, _map.Detail[x, y], x, y, Layers.DetailTile);
                DrawTile(spriteBatch, _map.Overhead[x, y], x, y, Layers.Overhead);
            }
        }
    }

    /// <summary>What the ground is like underfoot at a point, for the footsteps.</summary>
    /// <param name="world">The point, in world pixels.</param>
    public Surface SurfaceAt(Vector2 world)
    {
        int x = (int)MathF.Floor(world.X / TileSize), y = (int)MathF.Floor(world.Y / TileSize);
        if (x < 0 || y < 0 || x >= _map.Width || y >= _map.Height) return Surface.Stone;
        return _map.Tileset[_map.Ground[x, y]].Surface;
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

    /// <summary>Draws one cell of one layer, unless it's empty.</summary>
    private void DrawTile(SpriteBatch spriteBatch, int tile, int x, int y, float layerDepth)
    {
        if (tile == 0) return;

        Rectangle source = _frames[_map.Tileset.FrameOf(tile, Pick(x, y))];
        spriteBatch.Draw(_sheet, new Vector2(x * TileSize, y * TileSize), source, Color.White,
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
