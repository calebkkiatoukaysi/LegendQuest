using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The nameplates over everyone's heads in a fight (11-combat.md §1): their name, a thin health bar,
/// and over a foe winding up a big move, the move's icon and a warning bar filling to when it lands.
/// </summary>
/// <remarks>
/// Drawn in screen space from where the screen says each head is, so they don't scale with the
/// world. A foe's bar is red and its name a soft red (a brighter red for one that can be clicked to
/// start its fight); the party's are green and bone. The target's plate is brighter, bracketed in
/// gold, and the others are a little faded, so the eye finds it. A downed fighter's bar is grey and
/// says DOWN. While Alt's held the screen hands each its hit points to write in the bar. The
/// telegraph's icon sits over the name in an ember edge that pulses faster as the bar fills.
/// </remarks>
public class Nameplates
{
    /// <summary>The health bar's size, and the target's, in screen pixels.</summary>
    private const int BarWidth = 64;
    private const int BarHeight = 6;
    private const int TargetBarWidth = 76;
    private const int TargetBarHeight = 8;

    /// <summary>How far over the head the bar sits, the room between the parts, and the telegraph's bar.</summary>
    private const int Lift = 10;
    private const int Spacing = 2;
    private const int WarnHeight = 5;

    /// <summary>How faded the plates that aren't the target's are.</summary>
    private const float Others = 0.8f;

    /// <summary>The telegraph's edge, and how fast it pulses empty and full.</summary>
    private static readonly Color Warning = new(255, 140, 50);
    private const float PulseSlow = 2f;
    private const float PulseFast = 7f;

    /// <summary>The names' colours: a foe's, one that starts its fight when clicked, and the party's.</summary>
    private static readonly Color FoeName = CombatStyle.FoeLine;
    private static readonly Color HostileName = new(255, 110, 96);
    private static readonly Color DownBar = new(110, 104, 112);

    private const string Down = "DOWN";

    /// <summary>How far in from the screen's edges a plate stays, in screen pixels.</summary>
    private const int Edge = 4;

    /// <summary>Where across the screen someone's plate is centred: over their head, but kept far enough in that the name and the bar stay on the screen.</summary>
    private static float Across(HudKit kit, NameplateView plate)
    {
        float half = MathF.Max(TargetBarWidth, kit.Small.MeasureString(plate.Name ?? string.Empty).X) / 2f + Edge;
        return Math.Clamp(plate.Head.X, half, MathF.Max(half, LegendQuestGame.ScreenWidth - half));
    }

    /// <summary>Where someone's health bar is.</summary>
    private static Rectangle Bar(HudKit kit, NameplateView plate)
    {
        int w = plate.IsTarget ? TargetBarWidth : BarWidth, h = plate.IsTarget ? TargetBarHeight : BarHeight;
        return new Rectangle((int)MathF.Round(Across(kit, plate) - w / 2f), (int)MathF.Round(plate.Head.Y - Lift - h), w, h);
    }

    /// <summary>Where the top of someone's name is.</summary>
    private static float NameTop(HudKit kit, NameplateView plate) => Bar(kit, plate).Y - Spacing - kit.Small.LineSpacing + 2;

    /// <summary>Where the telegraph's icon is: over the name.</summary>
    private static Rectangle WarnIcon(HudKit kit, NameplateView plate)
    {
        int size = kit.Icons.Size + 2;
        float top = NameTop(kit, plate) - Spacing - WarnHeight - Spacing - size;
        return new Rectangle((int)MathF.Round(Across(kit, plate) - size / 2f), (int)MathF.Round(top), size, size);
    }

    /// <summary>Where the telegraph's bar is: under its icon, over the name.</summary>
    private static Rectangle WarnBar(HudKit kit, NameplateView plate)
    {
        Rectangle icon = WarnIcon(kit, plate);
        return new Rectangle((int)MathF.Round(Across(kit, plate) - BarWidth / 2f), icon.Bottom + Spacing, BarWidth, WarnHeight);
    }

    /// <summary>The top of everything someone's plate draws, for the target's marker to sit over.</summary>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="plate">The plate.</param>
    public static float Top(HudKit kit, NameplateView plate) => plate.Telegraph is not null ? WarnIcon(kit, plate).Y : NameTop(kit, plate);

    /// <summary>
    /// Everything someone's plate draws on the screen: the target's chevron over it, the telegraph,
    /// the name, the bar, and DOWN or the hit points under it; for the numbers and words that pop
    /// up over the room to keep off.
    /// </summary>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="plate">The plate.</param>
    public static Rectangle Bounds(HudKit kit, NameplateView plate)
    {
        Rectangle bar = Bar(kit, plate);
        float wide = MathF.Max(TargetBarWidth, kit.Small.MeasureString(plate.Name ?? string.Empty).X);
        string inBar = plate.IsDown ? Down : plate.Numbers;
        if (inBar is not null) wide = MathF.Max(wide, kit.Small.MeasureString(inBar).X);
        float top = Top(kit, plate) - (plate.IsTarget ? TargetMarker.ChevronRoom : 0);
        float bottom = inBar is null ? bar.Bottom : bar.Bottom + 1 + kit.Small.LineSpacing;
        float centre = Across(kit, plate);
        return new Rectangle((int)MathF.Floor(centre - wide / 2f), (int)MathF.Floor(top), (int)MathF.Ceiling(wide), (int)MathF.Ceiling(bottom - top));
    }

    /// <summary>Draws the bars, the target's brackets and the telegraphs. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="plates">Everyone's plate, back to front, so the nearer are over the farther.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawArt(SpriteBatch spriteBatch, HudKit kit, IReadOnlyList<NameplateView> plates, float alpha)
    {
        foreach (NameplateView plate in plates)
        {
            float a = alpha * (plate.IsTarget ? 1f : Others);
            Rectangle bar = Bar(kit, plate);
            Color fill = plate.IsDown ? DownBar : plate.Foe ? HudKit.FoeHealth : HudKit.PartyHealth;
            kit.Bar(spriteBatch, bar, plate.IsDown ? 1f : plate.HealthShare, fill, a);
            if (plate.IsTarget) DrawBrackets(spriteBatch, kit, bar, a);

            if (plate.Telegraph is TelegraphView warn)
            {
                float pulse = kit.Pulse(MathHelper.Lerp(PulseSlow, PulseFast, warn.Progress), 0.4f);
                Rectangle icon = WarnIcon(kit, plate);
                spriteBatch.Draw(kit.Pixel, icon, Warning * (pulse * alpha));
                kit.Icons.Draw(spriteBatch, warn.IconId ?? "telegraph", new Vector2(icon.X + 1, icon.Y + 1), 1f, Color.White * alpha, "telegraph");
                kit.Bar(spriteBatch, WarnBar(kit, plate), warn.Progress, Warning, alpha);
            }
        }
    }

    /// <summary>Writes the names, DOWN, and the hit points while Alt's held. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="plates">Everyone's plate, back to front.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawWords(SpriteBatch spriteBatch, HudKit kit, IReadOnlyList<NameplateView> plates, float alpha)
    {
        foreach (NameplateView plate in plates)
        {
            float a = alpha * (plate.IsTarget ? 1f : Others);
            Color name = plate.Foe ? (plate.Hostile ? HostileName : FoeName) : Palette.Bone;
            if (plate.IsTarget) name = Color.Lerp(name, Color.White, 0.35f);
            HudKit.Outlined(spriteBatch, kit.Small, plate.Name, new Vector2(Across(kit, plate), NameTop(kit, plate)), name * a, a, align: 0.5f);

            Rectangle bar = Bar(kit, plate);
            string inBar = plate.IsDown ? Down : plate.Numbers;
            if (inBar is not null)
                HudKit.Outlined(spriteBatch, kit.Small, inBar, new Vector2(bar.Center.X, bar.Bottom + 1), (plate.IsDown ? Palette.Ember : Color.White) * a, a, align: 0.5f);
        }
    }

    /// <summary>The gold corners round the target's bar.</summary>
    private static void DrawBrackets(SpriteBatch spriteBatch, HudKit kit, Rectangle bar, float alpha)
    {
        const int reach = 3, arm = 5;
        Color gold = Palette.Gold * alpha;
        int left = bar.X - reach, right = bar.Right + reach - 1, top = bar.Y - reach, bottom = bar.Bottom + reach - 1;
        foreach ((int x, int y, int dx, int dy) in new[] { (left, top, 1, 1), (right, top, -1, 1), (left, bottom, 1, -1), (right, bottom, -1, -1) })
        {
            spriteBatch.Draw(kit.Pixel, new Rectangle(dx > 0 ? x : x - arm + 1, y, arm, 1), gold);
            spriteBatch.Draw(kit.Pixel, new Rectangle(x, dy > 0 ? y : y - arm + 1, 1, arm), gold);
        }
    }
}
