using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters.Motion;

/// <summary>
/// A white copy of a picture, its shape kept and every pixel white, for the flash of a figure being
/// hit: drawn over the figure's own pieces, in white or tinted, at the flash's strength.
/// </summary>
/// <remarks>
/// Made the first time a picture's asked for (one GetData and one SetData, a piece or a sheet a
/// time, a few kilobytes each) and kept as long as the picture is: they're keyed weakly, so a
/// figure's white copies go when its pieces do. Everything drawn here is premultiplied, so a white
/// pixel of alpha a is (a, a, a, a), and multiplying it by a colour in SpriteBatch gives that colour
/// in the picture's shape. Cheaper than a shader, and it works in the room's own point-sampled,
/// sorted batch, so the flash sorts with the figure.
/// </remarks>
public static class Silhouettes
{
    private static readonly ConditionalWeakTable<Texture2D, Texture2D> White = new();

    /// <summary>The white copy of a picture, made now if it hasn't been.</summary>
    /// <param name="picture">The picture.</param>
    public static Texture2D Of(Texture2D picture)
    {
        if (picture is null || picture.Format != SurfaceFormat.Color) return null;
        if (White.TryGetValue(picture, out Texture2D white)) return white;
        var pixels = new Color[picture.Width * picture.Height];
        picture.GetData(pixels);
        for (int i = 0; i < pixels.Length; i++)
        {
            byte a = pixels[i].A;
            pixels[i] = new Color(a, a, a, a);
        }
        white = new Texture2D(picture.GraphicsDevice, picture.Width, picture.Height);
        white.SetData(pixels);
        White.AddOrUpdate(picture, white);
        return white;
    }
}
