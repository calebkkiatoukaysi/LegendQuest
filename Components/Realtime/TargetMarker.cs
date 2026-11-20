using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// What marks your target in the room: hud.png's target ring turning under its feet, red for a foe
/// and gold for an ally, and a small chevron bobbing over its nameplate.
/// </summary>
/// <remarks>
/// The ring goes under the figures (DrawRing, with the world's other ground marks, before the
/// figures are drawn), the chevron over everything (DrawChevron, with the nameplates). Both are in
/// screen space: the screen hands over where the feet and the top of the plate are.
/// </remarks>
public static class TargetMarker
{
    /// <summary>How much the ring is blown up (a room's own scale, so it sits on the floor like the art round it), and how fast it turns, frames a second.</summary>
    private const float RingScale = 4f;
    private const float RingRate = 6f;

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

    /// <summary>Draws the ring under someone's feet. Goes in a point sampled batch, before the figures.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="feet">Where their feet are, in screen pixels.</param>
    /// <param name="width">How wide they are, in screen pixels.</param>
    /// <param name="foe">Whether they're a foe.</param>
    /// <param name="alpha">How faded in it is.</param>
    public static void DrawRing(SpriteBatch spriteBatch, HudKit kit, Vector2 feet, float width, bool foe, float alpha)
    {
        string piece = foe ? "target-foe" : "target-ally";
        Rectangle source = Combat.CombatIcons.Piece(piece);
        float scale = MathF.Max(RingScale, MathF.Ceiling(width * RingWiden / source.Width));
        int frame = (int)(kit.Clock * RingRate);
        var at = new Vector2(MathF.Round(feet.X - source.Width * scale / 2f), MathF.Round(feet.Y - source.Height * scale / 2f));
        kit.Art.Piece(spriteBatch, piece, at, scale, Color.White * alpha, frame);
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
