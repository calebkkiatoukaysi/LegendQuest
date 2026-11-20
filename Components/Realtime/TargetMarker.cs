using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// What marks your target in the room: a soft target ellipse under its feet, red for a foe
/// and gold for an ally, and a small chevron bobbing over its nameplate.
/// </summary>
/// <remarks>
/// The ring goes under the figures (DrawRing, with the world's other ground marks, before the
/// figures are drawn), the chevron over everything (DrawChevron, with the nameplates). Both are in
/// screen space: the screen hands over where the feet and the top of the plate are.
/// </remarks>
public static class TargetMarker
{
    /// <summary>The smallest target ellipse, in screen pixels.</summary>
    private const float RingWidth = 64f;

    /// <summary>How wide the ring is against the figure, at least.</summary>
    private const float RingWiden = 1.15f;

    /// <summary>The chevron: how wide, how tall, how far over the plate, and how far and fast it bobs.</summary>
    private const int ChevronWidth = 14;
    private const int ChevronHeight = 8;
    private const int ChevronLift = 6;
    private const float BobReach = 3f;
    private const float BobRate = 1.6f;

    /// <summary>How far over the top of the plate the chevron reaches at the top of its bob, in screen pixels, for what's drawn over the room to keep clear of.</summary>
    public const int ChevronRoom = ChevronLift + ChevronHeight + (int)BobReach + 1;

    /// <summary>Draws the ring under someone's feet in a linearly sampled batch, before the figures.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linearly sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="feet">Where their feet are, in screen pixels.</param>
    /// <param name="width">How wide they are, in screen pixels.</param>
    /// <param name="foe">Whether they're a foe.</param>
    /// <param name="alpha">How faded in it is.</param>
    public static void DrawRing(SpriteBatch spriteBatch, HudKit kit, Vector2 feet, float width, bool foe, float alpha)
    {
        float wide = MathF.Max(RingWidth, width * RingWiden), high = wide * .42f;
        var at = new Vector2(MathF.Round(feet.X - wide / 2f), MathF.Round(feet.Y - high / 2f));
        Color colour = (foe ? HudKit.FoeHealth : Palette.Gold) * (alpha * kit.Pulse(.8f, .85f));
        spriteBatch.Draw(kit.TargetRing, at, null, colour, 0f, Vector2.Zero,
            new Vector2(wide / kit.TargetRing.Width, high / kit.TargetRing.Height), SpriteEffects.None, 0f);
    }

    /// <summary>Draws the chevron over the target's plate, pointing down at it. Goes in a point sampled batch, after the plates.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="over">The middle of the top of the plate, in screen pixels.</param>
    /// <param name="foe">Whether it's a foe: red, or gold for an ally.</param>
    /// <param name="alpha">How faded in it is.</param>
    public static void DrawChevron(SpriteBatch spriteBatch, HudKit kit, Vector2 over, bool foe, float alpha)
    {
        float bob = MathF.Round(BobReach * (0.5f + 0.5f * MathF.Sin(kit.Clock * BobRate * MathHelper.TwoPi)));
        int top = (int)(over.Y - ChevronLift - ChevronHeight - bob);
        int left = (int)MathF.Round(over.X - ChevronWidth / 2f);
        Color fill = (foe ? HudKit.FoeHealth : Palette.Gold) * alpha;
        Color edge = HudKit.BarEdge * alpha;
        for (int row = -1; row <= ChevronHeight; row++)
        {
            int half = ChevronWidth / 2 - row * ChevronWidth / 2 / ChevronHeight;
            if (half <= 0) half = 1;
            int width = half * 2;
            int x = left + ChevronWidth / 2 - half;
            spriteBatch.Draw(kit.Pixel, new Rectangle(x - 1, top + row, width + 2, 1), edge);
            if (row >= 0 && row < ChevronHeight)
                spriteBatch.Draw(kit.Pixel, new Rectangle(x, top + row, width, 1), row == 0 ? Color.Lerp(fill, Color.White * alpha, 0.5f) : fill);
        }
    }
}
