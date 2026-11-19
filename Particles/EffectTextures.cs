using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Particles;

/// <summary>The shapes the fight's effects are drawn with (EffectTextures makes each).</summary>
public enum EffectShape
{
    /// <summary>A soft round dot of light, brightest in the middle.</summary>
    Dot,

    /// <summary>A soft line, long across: a spark's streak, a ray.</summary>
    Streak,

    /// <summary>A sharp four-sided splinter: ice, glass, a thorn.</summary>
    Shard,

    /// <summary>A thin ring: a shockwave, a ripple, a bubble.</summary>
    Ring,

    /// <summary>A four-pointed flare: a glint, the white of an impact.</summary>
    Star,

    /// <summary>A crescent, its bright edge on the outside: the arc a blade cuts through the air at the moment it lands.</summary>
    Slash,

    /// <summary>A soft lumpy puff: smoke, dust, a wisp.</summary>
    Puff,

    /// <summary>A small pointed leaf.</summary>
    Leaf,
}

/// <summary>
/// The pictures the fight's impacts, bursts and projectiles are drawn with, made in code when the
/// fight's effects load, so there's no art to import or credit: every one is white (premultiplied,
/// so drawing it in a colour gives that colour) and drawn smooth and additive, the way light is.
/// </summary>
/// <remarks>
/// Each is worked out pixel by pixel from a formula: a dot is a Gaussian, a streak a Gaussian
/// stretched four to one, a shard a diamond with a hard edge, a ring a thin band of a Gaussian
/// round a circle, a star two crossed streaks and a dot, the slash a crescent between two circles
/// that's brightest along its outer edge and fades toward its tips, a puff a few overlapping dots
/// placed by a fixed seed, and a leaf a pointed oval. They're the same on every run.
/// </remarks>
public sealed class EffectTextures : IDisposable
{
    private readonly Texture2D[] _textures;

    /// <summary>Makes every shape on a device.</summary>
    public EffectTextures(GraphicsDevice device)
    {
        _textures = new Texture2D[Enum.GetValues<EffectShape>().Length];
        _textures[(int)EffectShape.Dot] = Make(device, 32, 32, (x, y) => Gauss(Len(x, y), 0.42f));
        _textures[(int)EffectShape.Streak] = Make(device, 64, 16, (x, y) => Gauss(x, 0.55f) * Gauss(y, 0.35f));
        _textures[(int)EffectShape.Shard] = Make(device, 24, 24, (x, y) => Edge(1f - (MathF.Abs(x) * 1.0f + MathF.Abs(y) * 2.2f)) * (0.75f + 0.25f * (1f - MathF.Abs(x))));
        _textures[(int)EffectShape.Ring] = Make(device, 64, 64, (x, y) => Gauss(Len(x, y) - 0.82f, 0.07f));
        _textures[(int)EffectShape.Star] = Make(device, 48, 48, (x, y) =>
            MathF.Min(1f, Gauss(x, 0.9f) * Gauss(y, 0.06f) + Gauss(y, 0.9f) * Gauss(x, 0.06f) + 0.8f * Gauss(Len(x, y), 0.22f)));
        _textures[(int)EffectShape.Slash] = Make(device, 128, 64, Crescent);
        _textures[(int)EffectShape.Puff] = Make(device, 32, 32, Puff);
        _textures[(int)EffectShape.Leaf] = Make(device, 16, 10, (x, y) => Edge(1f - (x * x + MathF.Pow(MathF.Abs(y) / (1f - 0.7f * MathF.Abs(x)), 2f))) * (0.8f + 0.2f * MathF.Sign(-y)));
    }

    /// <summary>A shape's picture.</summary>
    public Texture2D this[EffectShape shape] => _textures[(int)shape];

    /// <summary>Lets go of every picture.</summary>
    public void Dispose()
    {
        foreach (Texture2D texture in _textures) texture?.Dispose();
    }

    /// <summary>A picture of a size, each pixel's strength from a formula over -1 to 1 across and down.</summary>
    private static Texture2D Make(GraphicsDevice device, int width, int height, Func<float, float, float> strength)
    {
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width * 2f - 1f, v = (y + 0.5f) / height * 2f - 1f;
                float a = MathHelper.Clamp(strength(u, v), 0f, 1f);
                byte b = (byte)MathF.Round(a * 255f);
                pixels[y * width + x] = new Color(b, b, b, b);
            }
        }
        var texture = new Texture2D(device, width, height);
        texture.SetData(pixels);
        return texture;
    }

    private static float Len(float x, float y) => MathF.Sqrt(x * x + y * y);

    private static float Gauss(float d, float width) => MathF.Exp(-(d * d) / (2f * width * width)) * (MathF.Abs(d) < 1f ? 1f : 0f);

    /// <summary>A hard edge softened over a pixel or so.</summary>
    private static float Edge(float inside) => MathHelper.Clamp(inside * 8f, 0f, 1f);

    /// <summary>The slash: between a circle and a smaller one set below it, bright at the outer edge, fading to each tip.</summary>
    private static float Crescent(float x, float y)
    {
        // In the picture's own pixels (128 by 64): the outer circle's middle is the bottom middle, and
        // the inner one's lower still and bigger, so the band between them thins to a point each end.
        float px = x * 64f, py = (y + 1f) * 32f;
        float outer = Len(px, py - 64f), inner = Len(px, py - 96f);
        if (outer > 62f || inner < 86f) return 0f;
        float rim = MathF.Exp(-MathF.Pow((62f - outer) / 4f, 2f));
        float body = MathHelper.Clamp((inner - 86f) / 8f, 0f, 1f);
        float tips = MathF.Pow(MathHelper.Clamp(1f - MathF.Abs(px) / 62f, 0f, 1f), 0.5f);
        return (0.3f * body + 0.9f * rim) * tips;
    }

    /// <summary>A puff: a handful of soft dots round the middle, from a fixed seed.</summary>
    private static float Puff(float x, float y)
    {
        float a = 0f;
        var random = new Random(7);
        for (int i = 0; i < 6; i++)
        {
            float cx = (float)random.NextDouble() * 0.8f - 0.4f, cy = (float)random.NextDouble() * 0.8f - 0.4f;
            float r = 0.25f + (float)random.NextDouble() * 0.2f;
            a += 0.45f * Gauss(Len(x - cx, y - cy), r * 0.6f);
        }
        return MathF.Min(1f, a) * Gauss(Len(x, y), 0.6f) * 1.4f;
    }
}
