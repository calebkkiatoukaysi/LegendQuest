using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// A boss's wide frame across the top (11-combat.md §1, §5.2): its name and title, a pip for each
/// phase, its health the width of the frame with a tick where each phase begins, its cast bar under
/// it and its auras.
/// </summary>
/// <remarks>
/// The pips are diamonds: the phases behind it dark red, the one it's in gold and breathing, the
/// ones to come hollow. The phase's name ("ENRAGED") goes before them. The health bar has the
/// lag the other frames have, the share left as a percentage in its middle and the hit points at
/// its right. The cast bar goes under the frame, centred, as wide as a third of the screen, so a
/// boss's big wind-up is the thing to see.
/// </remarks>
public class BossFrame
{
    /// <summary>Where the frame is and how big, in screen pixels.</summary>
    public const int Width = 760;
    public const int Top = 12;
    public const int Height = 72;

    /// <summary>How far in its insides are, the health bar's height, and the room under it for the cast.</summary>
    private const int Inset = 12;
    private const int HealthHeight = 20;
    private const int CastWidth = 420;
    private const int CastHeight = 22;
    private const int CastDrop = 8;

    /// <summary>The phase pips: how big, the room between, and how fast the current one breathes.</summary>
    private const int Pip = 12;
    private const int PipGap = 8;
    private const float BreatheRate = 1.2f;

    /// <summary>How many auras to a row under the frame.</summary>
    private const int AurasPerRow = 8;

    /// <summary>The pips' colours: done, the one it's in, to come.</summary>
    private static readonly Color PipDone = new(120, 30, 36);
    private static readonly Color PipNow = new(255, 214, 110);
    private static readonly Color PipToCome = new(40, 36, 56);

    private readonly HealthLag _lag = new();

    /// <summary>Where the frame is on screen.</summary>
    public static Rectangle Bounds => new((LegendQuestGame.ScreenWidth - Width) / 2, Top, Width, Height);

    /// <summary>Where the cast bar goes: under the frame, centred.</summary>
    public static Rectangle CastArea => new((LegendQuestGame.ScreenWidth - CastWidth) / 2, Top + Height + CastDrop, CastWidth, CastHeight);

    /// <summary>The bottom of everything the frame draws under itself: the cast bar's room.</summary>
    public static int Bottom => CastArea.Bottom;

    /// <summary>Where its aura row starts: under the frame at its left.</summary>
    public static Point AurasAt => new(Bounds.X, Bounds.Bottom + CastDrop);

    /// <summary>What the boss's aura row covers on the screen, timers included, or an empty rectangle for none.</summary>
    /// <param name="boss">The boss.</param>
    public static Rectangle AuraArea(BossView boss) => AuraRow.Area(boss?.Fighter.Auras?.Count ?? 0, AurasAt, AurasPerRow, true);

    /// <summary>Which of the boss's auras the mouse is on, or null.</summary>
    /// <param name="mouse">Where the mouse is.</param>
    /// <param name="boss">The boss.</param>
    public static AuraView AuraAt(Point mouse, BossView boss)
    {
        if (boss?.Fighter.Auras is null) return null;
        int i = AuraRow.At(mouse, boss.Fighter.Auras.Count, AurasAt, AurasPerRow, true);
        return i >= 0 ? boss.Fighter.Auras[i] : null;
    }

    /// <summary>Draws the panel, the pips, the health bar with its ticks, the cast bar and the auras. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="boss">The boss, or null for none.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawArt(SpriteBatch spriteBatch, HudKit kit, BossView boss, float alpha)
    {
        if (boss is null) return;
        FighterHudView fighter = boss.Fighter;
        Rectangle bounds = Bounds;
        CombatStyle.DrawPanel(spriteBatch, kit.Pixel, bounds, alpha);
        spriteBatch.Draw(kit.Pixel, new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 4, 2), CombatStyle.FoeHealth * (0.5f * alpha));

        for (int i = 0; i < boss.Phases; i++)
        {
            Vector2 centre = PipCentre(i, boss.Phases);
            Color colour = i < boss.Phase - 1 ? PipDone : i == boss.Phase - 1 ? PipNow * kit.Pulse(BreatheRate, 0.7f) : PipToCome;
            Diamond(spriteBatch, kit, centre, Pip + 4, HudKit.BarEdge * alpha);
            Diamond(spriteBatch, kit, centre, Pip, colour * alpha);
            if (i == boss.Phase - 1) Diamond(spriteBatch, kit, centre - new Vector2(0f, 2f), 4, Color.White * (0.8f * alpha));
        }

        Rectangle bar = HealthBar;
        float share = fighter.HealthShare;
        float lag = _lag.Step(fighter.Id, share, kit.Delta);
        kit.Bar(spriteBatch, bar, 0f, Color.Transparent, alpha);
        Rectangle inside = HudKit.Inside(bar);
        kit.Fill(spriteBatch, inside, share, lag, HudKit.Lag, alpha * 0.85f);
        kit.Fill(spriteBatch, inside, 0f, share, HudKit.FoeHealth, alpha);
        if (boss.Thresholds is not null)
            foreach (float threshold in boss.Thresholds)
            {
                int x = inside.X + (int)MathF.Round(inside.Width * threshold);
                spriteBatch.Draw(kit.Pixel, new Rectangle(x - 1, bar.Y - 3, 3, bar.Height + 6), HudKit.BarEdge * alpha);
                spriteBatch.Draw(kit.Pixel, new Rectangle(x, bar.Y - 2, 1, bar.Height + 4), Palette.Gold * alpha);
            }

        if (fighter.Casting is not null) CastBar.DrawArt(spriteBatch, kit, CastArea, fighter.Casting, true, alpha);
        AuraRow.DrawArt(spriteBatch, kit, fighter.Auras, AurasAt, AurasPerRow, true, alpha);
    }

    /// <summary>Writes the name, the title, the phase, the health's share and hit points, the cast and the auras' timers. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="boss">The boss, or null for none.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawWords(SpriteBatch spriteBatch, HudKit kit, BossView boss, float alpha)
    {
        if (boss is null) return;
        FighterHudView fighter = boss.Fighter;
        Rectangle bounds = Bounds;
        string name = fighter.Name.ToUpperInvariant();
        float nameY = bounds.Y + 2;
        Text.Draw(spriteBatch, kit.Words, name, bounds.X + Inset, nameY, Palette.Gold * alpha, Palette.Shadow * alpha);
        Vector2 first = PipCentre(0, boss.Phases);
        if (boss.Title is not null)
        {
            float after = bounds.X + Inset + kit.Words.MeasureString(name).X + 14;
            Text.Draw(spriteBatch, kit.Small, boss.Title.ToUpperInvariant(), after, first.Y - kit.Small.LineSpacing / 2f, Palette.Note * alpha, Palette.Shadow * alpha);
        }
        if (boss.PhaseName is not null)
            Text.Draw(spriteBatch, kit.Small, boss.PhaseName, first.X - Pip - 8, first.Y - kit.Small.LineSpacing / 2f, CombatStyle.FoeLine * alpha, Palette.Shadow * alpha,
                rightAligned: true);

        Rectangle bar = HealthBar;
        string percent = ((int)MathF.Ceiling(fighter.HealthShare * 100f)).ToString(CultureInfo.InvariantCulture) + "%";
        HudKit.Outlined(spriteBatch, kit.Small, percent, bar.Center.ToVector2(), Color.White * alpha, alpha, align: 0.5f, middle: true);
        string hp = fighter.Hp.ToString("N0", CultureInfo.InvariantCulture) + " / " + fighter.MaxHp.ToString("N0", CultureInfo.InvariantCulture);
        HudKit.Outlined(spriteBatch, kit.Small, hp, new Vector2(bar.Right - 6, bar.Center.Y), Palette.Bone * alpha, alpha, align: 1f, middle: true);

        if (fighter.Casting is not null) CastBar.DrawWords(spriteBatch, kit, CastArea, fighter.Casting, alpha);
        AuraRow.DrawWords(spriteBatch, kit, fighter.Auras, AurasAt, AurasPerRow, true, alpha);
    }

    /// <summary>Forgets the health lag, for a new fight.</summary>
    public void Reset() => _lag.Clear();

    /// <summary>Sets the health lag outright, for a picture of a hit just taken.</summary>
    /// <param name="id">The boss's id.</param>
    /// <param name="was">Its health before the hit, 0 to 1.</param>
    /// <param name="share">Its health now.</param>
    public void ShowHit(string id, float was, float share) => _lag.Set(id, was, share);

    /// <summary>Where the health bar is: the frame's width, along its bottom.</summary>
    private static Rectangle HealthBar
    {
        get
        {
            Rectangle b = Bounds;
            return new Rectangle(b.X + Inset, b.Bottom - Inset - HealthHeight, b.Width - Inset * 2, HealthHeight);
        }
    }

    /// <summary>Where a phase's pip is: in a row at the right of the name's line.</summary>
    private static Vector2 PipCentre(int index, int count)
    {
        Rectangle b = Bounds;
        float right = b.Right - Inset - Pip / 2f - 2f;
        return new Vector2(right - (count - 1 - index) * (Pip + PipGap), b.Y + Inset + Pip / 2f + 2f);
    }

    /// <summary>A diamond: a square turned a quarter round, `size` across its points.</summary>
    private static void Diamond(SpriteBatch spriteBatch, HudKit kit, Vector2 centre, int size, Color colour)
    {
        float side = size / MathF.Sqrt(2f);
        spriteBatch.Draw(kit.Pixel, centre, null, colour, MathHelper.PiOver4, new Vector2(0.5f, 0.5f), side, SpriteEffects.None, 0f);
    }
}
