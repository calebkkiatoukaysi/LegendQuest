using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.World;

/// <summary>
/// The dark in a dark room: one texture of darkness with the pools of light cut out of it, laid over
/// the whole room after everything else is drawn, so anyone who walks out of the light goes dark too.
/// </summary>
/// <remarks>
/// The Black Box's lobby did it this way, and it's built once, not every frame. It's low
/// resolution and stretched over the room with smooth sampling, so the edges of the pools are soft.
/// A room is seen side on, so nothing in it stands between a lamp and the wall behind it: its pools
/// are round and nothing stops them. And a room's painted sky and far layer keep their own light (a
/// night sky is already dark), so DrawMasked only darkens where the canvas's alpha is up: whatever
/// drew with OutOfTheDark left it at nothing.
/// </remarks>
public class LightMap
{
    /// <summary>How dark it gets with no light at all, 0 to 1. Not quite 1, so the walls still show a little.</summary>
    private const float Darkness = 0.94f;

    /// <summary>How many art pixels each texel of the darkness covers. Four is plenty with the smooth sampling.</summary>
    private const int Cell = 4;

    /// <summary>How fast a pool fades toward its edge. Same curve as the lobby's lamps.</summary>
    private const float Falloff = 2.2f;

    /// <summary>Past this many times its reach a light adds nothing worth working out.</summary>
    private const float Cutoff = 2.5f;

    /// <summary>The colour of the dark. Cold and nearly black.</summary>
    private static readonly Color ShadeColor = new(4, 3, 8);

    /// <summary>Draws as alpha blending does but leaves the canvas's alpha at nothing, so DrawMasked leaves it lit.</summary>
    public static readonly BlendState OutOfTheDark = new()
    {
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.InverseSourceAlpha,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.Zero,
    };

    /// <summary>Multiplies the canvas's alpha by how dark each texel is, and touches nothing else.</summary>
    private static readonly BlendState FoldDark = new()
    {
        ColorWriteChannels = ColorWriteChannels.Alpha,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.SourceAlpha,
    };

    /// <summary>Lays a colour over the canvas as far as its alpha says, and puts the alpha back to full.</summary>
    private static readonly BlendState ApplyDark = new()
    {
        ColorSourceBlend = Blend.DestinationAlpha,
        ColorDestinationBlend = Blend.InverseDestinationAlpha,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.Zero,
    };

    private readonly Rectangle _bounds;
    private readonly float _ambient;
    private readonly IReadOnlyList<(Vector2 Pool, float Reach, float Strength)> _lights;

    private Texture2D _shade;
    private Texture2D _pixel;

    /// <summary>Gets ready to build the dark over a room, where nothing stops the light. Nothing is built until LoadContent.</summary>
    /// <param name="bounds">The room, in world pixels.</param>
    /// <param name="ambient">How much light gets everywhere, 0 to 1.</param>
    /// <param name="lights">Every pool of light: its middle in world pixels, how far it reaches, how bright it is.</param>
    public LightMap(Rectangle bounds, float ambient, IReadOnlyList<(Vector2 Pool, float Reach, float Strength)> lights)
    {
        _bounds = bounds;
        _ambient = ambient;
        _lights = lights;
    }

    /// <summary>Builds the darkness texture, and the white pixel DrawMasked lays its colour with.</summary>
    /// <param name="graphicsDevice">The device, since the textures are made in code.</param>
    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _shade = Build(graphicsDevice);
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    /// <summary>Disposes the textures, since the content manager didn't load them.</summary>
    public void Unload()
    {
        _shade?.Dispose();
        _pixel?.Dispose();
    }

    /// <summary>Draws the darkness over the whole place except wherever OutOfTheDark drew last, and leaves the canvas's alpha full again.</summary>
    /// <remarks>
    /// Two passes, because blending can't multiply a colour by two alphas at once: the first folds
    /// how dark each texel is into the canvas's alpha (so what OutOfTheDark left at nothing stays
    /// nothing), and the second lays the dark's colour over the canvas by that alpha. Starts and ends
    /// its own batches, after the world.
    /// </remarks>
    /// <param name="spriteBatch">The SpriteBatch to render with, not begun.</param>
    /// <param name="transform">The camera's world to screen matrix.</param>
    public void DrawMasked(SpriteBatch spriteBatch, Matrix transform)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, FoldDark, SamplerState.LinearClamp, transformMatrix: transform);
        spriteBatch.Draw(_shade, _bounds, Color.White);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, ApplyDark, SamplerState.PointClamp, transformMatrix: transform);
        spriteBatch.Draw(_pixel, _bounds, ShadeColor);
        spriteBatch.End();
    }

    /// <summary>Builds the low resolution texture of how dark each part of the room is.</summary>
    private Texture2D Build(GraphicsDevice graphicsDevice)
    {
        int width = _bounds.Width / Cell, height = _bounds.Height / Cell;
        var texels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var at = new Vector2(_bounds.X + (x + 0.5f) * Cell, _bounds.Y + (y + 0.5f) * Cell);
                float alpha = Darkness * (1f - MathHelper.Clamp(_ambient + LightAt(at), 0f, 1f));

                // Premultiplied, which is what SpriteBatch's alpha blend expects.
                texels[y * width + x] = ShadeColor * alpha;
            }
        }

        var shade = new Texture2D(graphicsDevice, width, height);
        shade.SetData(texels);
        return shade;
    }

    /// <summary>How much light reaches a point from every pool.</summary>
    /// <param name="at">The point, in world pixels.</param>
    private float LightAt(Vector2 at)
    {
        float light = 0f;

        foreach ((Vector2 pool, float reach, float strength) in _lights)
        {
            float d = Vector2.Distance(at, pool) / reach;
            if (d > Cutoff) continue;

            light += strength / (1f + d * d * d * Falloff);
        }

        return light;
    }
}
