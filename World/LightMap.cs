using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.World;

/// <summary>
/// The dark on a dark map: one texture of darkness with the pools of light cut out of it, laid over
/// the whole map after everything else is drawn, so anyone who walks out of the light goes dark too.
/// </summary>
/// <remarks>
/// The Black Box's lobby did it this way, and it's built once, not every frame. It's low
/// resolution and stretched over the map with smooth sampling, so the edges of the pools are soft.
/// What's new is that light stops at walls, which the lobby (one room) never needed: each texel
/// looks for a wall between itself and the light. Not from a tutorial, just a line walked across
/// the tiles. A shut door stops it too, so opening one means building it again.
/// </remarks>
public class LightMap
{
    /// <summary>How dark it gets with no light at all, 0 to 1. Not quite 1, so the walls still show a little.</summary>
    private const float Darkness = 0.94f;

    /// <summary>How many art pixels each texel of the darkness covers. Four is plenty with the smooth sampling.</summary>
    private const int Cell = 4;

    /// <summary>How fast a pool fades toward its edge. Same curve as the lobby's lamps.</summary>
    private const float Falloff = 2.2f;

    /// <summary>How much wider than tall a pool is, since we're looking down at the floor at an angle.</summary>
    private const float Squash = 1.35f;

    /// <summary>Past this many times its reach a light adds nothing worth working out.</summary>
    private const float Cutoff = 2.5f;

    /// <summary>How many rows tall a wall's face is. The face takes its light from the floor below it.</summary>
    private const int FaceRows = 2;

    /// <summary>The colour of the dark. Cold and nearly black.</summary>
    private static readonly Color ShadeColor = new(4, 3, 8);

    private readonly Tilemap _tiles;
    private readonly float _ambient;
    private readonly IReadOnlyList<(Vector2 Pool, float Reach, float Strength)> _lights;

    private Texture2D _shade;

    /// <summary>Gets ready to build the dark for a map. Nothing is built until LoadContent.</summary>
    /// <param name="tiles">The map, for its size and which tiles are solid.</param>
    /// <param name="ambient">How much light gets everywhere, 0 to 1.</param>
    /// <param name="lights">Every pool of light: its middle in world pixels, how far it reaches, how bright it is.</param>
    public LightMap(Tilemap tiles, float ambient, IReadOnlyList<(Vector2 Pool, float Reach, float Strength)> lights)
    {
        _tiles = tiles;
        _ambient = ambient;
        _lights = lights;
    }

    /// <summary>Builds the darkness texture.</summary>
    /// <param name="graphicsDevice">The device, since the texture is made in code.</param>
    public void LoadContent(GraphicsDevice graphicsDevice) => _shade = Build(graphicsDevice);

    /// <summary>Builds the darkness again, after a door opens or shuts. Not every frame: it walks a line per texel per light.</summary>
    /// <param name="graphicsDevice">The device, since the texture is made in code.</param>
    public void Rebuild(GraphicsDevice graphicsDevice)
    {
        _shade?.Dispose();
        _shade = Build(graphicsDevice);
    }

    /// <summary>Disposes the darkness texture, since the content manager didn't load it.</summary>
    public void Unload() => _shade?.Dispose();

    /// <summary>Draws the darkness over the whole map. Needs its own linear sampled batch, after the world.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space, linear-sampled.</param>
    public void Draw(SpriteBatch spriteBatch) => spriteBatch.Draw(_shade, _tiles.Bounds, Color.White);

    /// <summary>Builds the low resolution texture of how dark each part of the map is.</summary>
    private Texture2D Build(GraphicsDevice graphicsDevice)
    {
        Rectangle bounds = _tiles.Bounds;
        int width = bounds.Width / Cell, height = bounds.Height / Cell;
        var texels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var at = new Vector2((x + 0.5f) * Cell, (y + 0.5f) * Cell);
                float alpha = Darkness * (1f - MathHelper.Clamp(_ambient + LightAt(at), 0f, 1f));

                // Premultiplied, which is what SpriteBatch's alpha blend expects.
                texels[y * width + x] = ShadeColor * alpha;
            }
        }

        var shade = new Texture2D(graphicsDevice, width, height);
        shade.SetData(texels);
        return shade;
    }

    /// <summary>How much light reaches a point from every pool that can see it.</summary>
    /// <param name="at">The point, in world pixels.</param>
    private float LightAt(Vector2 at)
    {
        float light = 0f;

        foreach ((Vector2 pool, float reach, float strength) in _lights)
        {
            float d = Vector2.Distance(new Vector2(at.X, pool.Y + (at.Y - pool.Y) * Squash), pool) / reach;
            if (d > Cutoff || Blocked(at, pool)) continue;

            light += strength / (1f + d * d * d * Falloff);
        }

        return light;
    }

    /// <summary>Whether walls stand between a point and a light.</summary>
    /// <remarks>
    /// Two tries, and either getting through means it's lit: straight from the tile the point is in
    /// (so rubble on a lit floor is lit), and from where it's seen from (so a wall's face is lit by
    /// the torch beside it).
    /// </remarks>
    private bool Blocked(Vector2 at, Vector2 light)
    {
        if (!_tiles.LineBlocked(Tilemap.CellOf(at), Tilemap.CellOf(light))) return false;
        if (SeenFrom(at) is not Point from || SeenFrom(light) is not Point to) return true;
        return _tiles.LineBlocked(from, to);
    }

    /// <summary>The tile a point is seen from: its own, or for a wall's face, the floor just in front of it.</summary>
    /// <remarks>
    /// A wall's face is looked at from the south, so it's as lit as the floor right below it. My
    /// first go walked the line from the face tile itself, so every face shut its own torch out of
    /// the floor beside it and the pools came out square. FaceRows down and no further, so the
    /// top of a wall (which is further in than that) stays dark.
    /// </remarks>
    /// <returns>The tile, or null if there's no floor that close below it.</returns>
    private Point? SeenFrom(Vector2 world)
    {
        Point cell = Tilemap.CellOf(world);

        for (int down = 0; down <= FaceRows; down++)
            if (!_tiles.BlocksSight(cell.X, cell.Y + down)) return new Point(cell.X, cell.Y + down);

        return null;
    }
}
