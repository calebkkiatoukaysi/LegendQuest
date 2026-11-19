using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// A row of auras: each one's icon in an edge of green (good news) or red (bad), its stacks in its
/// corner, its time left under it, the share gone veiled over it, and a flash as it runs out.
/// </summary>
/// <remarks>
/// The frames draw one under themselves, the party's rows one beside each, without timers. The
/// icons are 24x24 at their own size, so an aura row is the hotbar's icons at half the size. An
/// aura that lasts (a passive, a stance) has no timer and no veil. One with under FlashUnder
/// seconds left pulses, and its timer turns gold and shows tenths.
/// </remarks>
public static class AuraRow
{
    /// <summary>How big an icon's box is, its edge included, and the room between two boxes and under a box for its timer, in screen pixels.</summary>
    public const int Box = 26;
    public const int Gap = 3;
    public const int TimerRoom = 15;

    /// <summary>The room between two boxes that have their timers under them: wider, so that two timers side by side ("2.1", "24s") read as two.</summary>
    public const int TimedGap = 9;

    /// <summary>How long before it runs out an aura flashes, in seconds, and how fast.</summary>
    public const float FlashUnder = 3f;
    private const float FlashRate = 3f;
    private const float FlashLeast = 0.35f;

    /// <summary>How dark the veil over the share of an aura that's gone is.</summary>
    private const float Veil = 0.5f;

    /// <summary>Where each aura's box goes: from a corner, left to right (or right to left), wrapping after a number a row.</summary>
    /// <param name="count">How many auras.</param>
    /// <param name="at">The row's first box's top left (or top right, right to left), in screen pixels.</param>
    /// <param name="perRow">How many to a row.</param>
    /// <param name="timers">Whether there's room under each box for its timer.</param>
    /// <param name="rightToLeft">Whether the row runs leftward from its corner.</param>
    public static Rectangle[] Layout(int count, Point at, int perRow, bool timers, bool rightToLeft = false)
    {
        var boxes = new Rectangle[count];
        int rowHeight = Box + Gap + (timers ? TimerRoom : 0);
        int step = Box + (timers ? TimedGap : Gap);
        for (int i = 0; i < count; i++)
        {
            int column = i % Math.Max(1, perRow), row = i / Math.Max(1, perRow);
            int x = rightToLeft ? at.X - Box - column * step : at.X + column * step;
            boxes[i] = new Rectangle(x, at.Y + row * rowHeight, Box, Box);
        }
        return boxes;
    }

    /// <summary>How tall a row of so many auras is, timers included.</summary>
    /// <param name="count">How many auras.</param>
    /// <param name="perRow">How many to a row.</param>
    /// <param name="timers">Whether each has its timer under it.</param>
    public static int Height(int count, int perRow, bool timers) =>
        count == 0 ? 0 : (count + perRow - 1) / perRow * (Box + Gap + (timers ? TimerRoom : 0));

    /// <summary>Which aura the mouse is on, or -1.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="count">How many auras.</param>
    /// <param name="at">The row's corner, as for Layout.</param>
    /// <param name="perRow">How many to a row.</param>
    /// <param name="timers">Whether each has its timer under it.</param>
    /// <param name="rightToLeft">Whether the row runs leftward.</param>
    public static int At(Point mouse, int count, Point at, int perRow, bool timers, bool rightToLeft = false)
    {
        Rectangle[] boxes = Layout(count, at, perRow, timers, rightToLeft);
        for (int i = 0; i < boxes.Length; i++)
            if (boxes[i].Contains(mouse)) return i;
        return -1;
    }

    /// <summary>Draws the boxes: each edge, icon and veil, flashing as it runs out. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="auras">The auras, in the order they're shown.</param>
    /// <param name="at">The row's corner.</param>
    /// <param name="perRow">How many to a row.</param>
    /// <param name="timers">Whether each has its timer under it.</param>
    /// <param name="alpha">How faded in it is.</param>
    /// <param name="rightToLeft">Whether the row runs leftward.</param>
    public static void DrawArt(SpriteBatch spriteBatch, HudKit kit, IReadOnlyList<AuraView> auras, Point at, int perRow, bool timers, float alpha, bool rightToLeft = false)
    {
        if (auras is null) return;
        Rectangle[] boxes = Layout(auras.Count, at, perRow, timers, rightToLeft);
        for (int i = 0; i < auras.Count; i++)
        {
            AuraView aura = auras[i];
            Rectangle box = boxes[i];
            float a = alpha * Flash(kit, aura);
            spriteBatch.Draw(kit.Pixel, box, (aura.Beneficial ? HudKit.Good : HudKit.Bad) * a);
            var well = new Rectangle(box.X + 1, box.Y + 1, box.Width - 2, box.Height - 2);
            kit.Icons.Draw(spriteBatch, aura.IconId, well.Location.ToVector2(), 1f, Color.White * a,
                aura.Beneficial ? HudIcons.BuffFallback : HudIcons.DebuffFallback);
            if (!aura.Lasting) kit.Icons.DrawSweep(spriteBatch, kit.Pixel, well, 1f - aura.Share, Veil * a, 0f);
        }
    }

    /// <summary>Writes each aura's stacks in its corner and its time left under it. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="auras">The auras.</param>
    /// <param name="at">The row's corner.</param>
    /// <param name="perRow">How many to a row.</param>
    /// <param name="timers">Whether each has its timer under it.</param>
    /// <param name="alpha">How faded in it is.</param>
    /// <param name="rightToLeft">Whether the row runs leftward.</param>
    public static void DrawWords(SpriteBatch spriteBatch, HudKit kit, IReadOnlyList<AuraView> auras, Point at, int perRow, bool timers, float alpha, bool rightToLeft = false)
    {
        if (auras is null) return;
        Rectangle[] boxes = Layout(auras.Count, at, perRow, timers, rightToLeft);
        for (int i = 0; i < auras.Count; i++)
        {
            AuraView aura = auras[i];
            Rectangle box = boxes[i];
            if (aura.Stacks > 1)
                HudKit.Outlined(spriteBatch, kit.Small, aura.Stacks.ToString(CultureInfo.InvariantCulture),
                    new Vector2(box.Right + 1, box.Bottom - kit.Small.LineSpacing + 3), Color.White * alpha, alpha, align: 1f);
            if (timers && !aura.Lasting)
            {
                Color colour = aura.Remaining < FlashUnder ? Palette.Gold : Palette.Bone;
                HudKit.Outlined(spriteBatch, kit.Small, HudKit.Seconds(aura.Remaining, unit: true),
                    new Vector2(box.Center.X, box.Bottom - 1), colour * alpha, alpha, align: 0.5f);
            }
        }
    }

    /// <summary>Puts an aura's card together: its name, what it does, its stacks and time left, and who put it there.</summary>
    /// <param name="card">The card, cleared first.</param>
    /// <param name="aura">The aura.</param>
    public static void Describe(CombatCard card, AuraView aura)
    {
        card.Clear();
        card.Line(aura.Name.ToUpperInvariant(), aura.Beneficial ? HudKit.Good : CombatStyle.FoeLine);
        card.Line(aura.Text, Palette.Bone);
        card.Gap();
        string left = aura.Lasting ? "Lasts while it's on" : $"{HudKit.Seconds(aura.Remaining, unit: true)} left of {HudKit.Seconds(aura.Total, unit: true)}";
        if (aura.Stacks > 1) left += $" · {aura.Stacks} stacks";
        card.Line(left, Palette.Note);
        if (aura.Source is not null) card.Line("From " + aura.Source, Palette.Note);
    }

    /// <summary>How bright an aura is this frame: full, or pulsing as it runs out.</summary>
    private static float Flash(HudKit kit, AuraView aura) =>
        !aura.Lasting && aura.Remaining < FlashUnder ? kit.Pulse(FlashRate, FlashLeast) : 1f;
}
