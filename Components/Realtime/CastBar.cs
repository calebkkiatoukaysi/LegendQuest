using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// A cast bar: what someone's winding up, filling as it comes, with its icon at its left end, its
/// name in it and the time left at its right.
/// </summary>
/// <remarks>
/// The leader's fills gold, over the hotbar. A foe's fills ember orange when it can be stopped,
/// and steel grey with a small lock at its end when it can't, so the eye knows at once whether
/// an interrupt is worth it. A stopped cast shows full and red with INTERRUPTED for as long as the
/// screen keeps it up. A bright line marks the fill's leading edge.
/// </remarks>
public static class CastBar
{
    /// <summary>The fills: the party's, a foe's that can be stopped, one that can't, and one stopped.</summary>
    public static readonly Color PartyFill = new(240, 196, 90);
    public static readonly Color FoeFill = new(246, 132, 52);
    public static readonly Color LockedFill = new(150, 152, 170);
    public static readonly Color StoppedFill = new(196, 46, 46);

    /// <summary>What a stopped cast says.</summary>
    private const string Stopped = "INTERRUPTED";

    /// <summary>How far in from the bar's ends its words go, in screen pixels.</summary>
    private const int TextInset = 6;

    /// <summary>How tall a cast bar has to be for its icon at its left end: an icon at its own size and an edge.</summary>
    public const int IconRoom = 26;

    /// <summary>Whether a cast's icon goes at the bar's left end: it has one, and there's room.</summary>
    /// <param name="cast">The cast.</param>
    /// <param name="area">The whole cast bar.</param>
    public static bool HasIcon(CastView cast, Rectangle area) => cast.IconId is not null && area.Height >= IconRoom;

    /// <summary>The bar itself, past the icon at its left end (the bar's height square).</summary>
    /// <param name="area">The whole cast bar, icon included.</param>
    /// <param name="icon">Whether there's an icon.</param>
    public static Rectangle Bar(Rectangle area, bool icon) =>
        icon ? new Rectangle(area.X + area.Height + 2, area.Y, area.Width - area.Height - 2, area.Height) : area;

    /// <summary>Draws the bar, its fill and its icon. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="area">Where, icon included, in screen pixels. Its height is the icon's size.</param>
    /// <param name="cast">The cast.</param>
    /// <param name="foe">Whether it's a foe's.</param>
    /// <param name="alpha">How faded in it is.</param>
    public static void DrawArt(SpriteBatch spriteBatch, HudKit kit, Rectangle area, CastView cast, bool foe, float alpha)
    {
        if (cast is null) return;
        bool icon = HasIcon(cast, area);
        if (icon)
        {
            var box = new Rectangle(area.X, area.Y, area.Height, area.Height);
            spriteBatch.Draw(kit.Pixel, box, HudKit.BarEdge * alpha);
            var at = new Vector2(box.X + (box.Width - kit.Icons.Size) / 2, box.Y + (box.Height - kit.Icons.Size) / 2);
            kit.Icons.Draw(spriteBatch, cast.IconId, at, 1f, Color.White * alpha, "telegraph");
        }

        Rectangle bar = Bar(area, icon);
        Color fill = cast.Interrupted ? StoppedFill : !foe ? PartyFill : cast.Interruptible ? FoeFill : LockedFill;
        float progress = cast.Interrupted ? 1f : MathHelper.Clamp(cast.Progress, 0f, 1f);
        kit.Bar(spriteBatch, bar, progress, fill, alpha);

        Rectangle inside = HudKit.Inside(bar);
        if (!cast.Interrupted && progress > 0f && progress < 1f)
        {
            int edge = inside.X + (int)MathF.Round(inside.Width * progress);
            spriteBatch.Draw(kit.Pixel, new Rectangle(edge - 1, inside.Y, 2, inside.Height), Color.White * (0.9f * alpha));
        }
        if (foe && !cast.Interruptible && !cast.Interrupted) DrawLock(spriteBatch, kit, new Point(bar.Right + 3, bar.Center.Y), alpha);
    }

    /// <summary>Writes the cast's name in the bar and its time left at the right. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="area">Where, icon included.</param>
    /// <param name="cast">The cast.</param>
    /// <param name="alpha">How faded in it is.</param>
    public static void DrawWords(SpriteBatch spriteBatch, HudKit kit, Rectangle area, CastView cast, float alpha)
    {
        if (cast is null) return;
        Rectangle bar = Bar(area, HasIcon(cast, area));
        float middle = bar.Center.Y;
        string name = cast.Interrupted ? Stopped : cast.Name.ToUpperInvariant();
        HudKit.Outlined(spriteBatch, kit.Small, name, new Vector2(bar.X + TextInset, middle), Color.White * alpha, alpha, middle: true);
        if (!cast.Interrupted && cast.Left > 0f)
            HudKit.Outlined(spriteBatch, kit.Small, HudKit.Seconds(cast.Left), new Vector2(bar.Right - TextInset, middle), Palette.Bone * alpha, alpha, align: 1f, middle: true);
    }

    /// <summary>A small padlock, its left middle at a point: a cast nothing can stop.</summary>
    private static void DrawLock(SpriteBatch spriteBatch, HudKit kit, Point at, float alpha)
    {
        Color steel = new Color(214, 218, 236) * alpha, dark = HudKit.BarEdge * alpha;
        spriteBatch.Draw(kit.Pixel, new Rectangle(at.X - 1, at.Y - 7, 12, 14), dark);
        spriteBatch.Draw(kit.Pixel, new Rectangle(at.X, at.Y - 1, 10, 7), steel);
        spriteBatch.Draw(kit.Pixel, new Rectangle(at.X + 2, at.Y - 6, 2, 5), steel);
        spriteBatch.Draw(kit.Pixel, new Rectangle(at.X + 6, at.Y - 6, 2, 5), steel);
        spriteBatch.Draw(kit.Pixel, new Rectangle(at.X + 2, at.Y - 6, 6, 2), steel);
        spriteBatch.Draw(kit.Pixel, new Rectangle(at.X + 4, at.Y + 1, 2, 3), dark);
    }
}
