using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters;

/// <summary>
/// The shadow under a figure in the pixel look: a flat, hard-edged oval in deep plum, a texel to an
/// art pixel, in place of the painted look's soft blob. One picture is kept for each size asked for.
/// </summary>
public static class PixelShadow
{
    /// <summary>The shadow's colour: the plum the game uses where black would go, see-through.</summary>
    public static readonly Color Colour = new Color(30, 14, 50) * 0.42f;

    private static readonly Dictionary<Point, Texture2D> Kept = new();

    /// <summary>An oval of a size, white, to be drawn in Colour.</summary>
    /// <param name="device">The device to make it on.</param>
    /// <param name="width">How wide, in art pixels.</param>
    /// <param name="height">How tall, in art pixels.</param>
    public static Texture2D Of(GraphicsDevice device, int width, int height)
    {
        var size = new Point(Math.Clamp(width, 2, 256), Math.Clamp(height, 1, 128));
        if (Kept.TryGetValue(size, out Texture2D oval) && !oval.IsDisposed) return oval;
        var pixels = new Color[size.X * size.Y];
        for (int y = 0; y < size.Y; y++)
        {
            for (int x = 0; x < size.X; x++)
            {
                float dx = (x + 0.5f) / size.X * 2f - 1f, dy = (y + 0.5f) / size.Y * 2f - 1f;
                if (dx * dx + dy * dy <= 1f) pixels[y * size.X + x] = Color.White;
            }
        }
        oval = new Texture2D(device, size.X, size.Y);
        oval.SetData(pixels);
        Kept[size] = oval;
        return oval;
    }
}
