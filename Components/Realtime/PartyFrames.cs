using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The party's rows under your frame (11-combat.md §1): one small row each for everyone fighting
/// beside you, with their face, name, hit points and Focus, a down marker and their death saves,
/// and their auras beside the row.
/// </summary>
/// <remarks>
/// Clicking a row targets them (for a heal), so At says which row the mouse is on. The row of
/// whoever's your target has a gold edge. Downed, the face greys, the health bar says DOWN and the
/// death saves take the Focus bar's place; out of the fight, the whole row dims. The auras beside a
/// row have no timers, to keep the rows short; pointing at one brings up its card with its time.
/// </remarks>
public class PartyFrames
{
    /// <summary>Where the rows are, how big each is and the room between them, in screen pixels.</summary>
    public const int Left = PlayerFrame.Left;
    public const int Width = 262;
    public const int RowHeight = 60;
    public const int RowGap = 6;

    /// <summary>How far in a row's insides are, and the face's frame round its well, in screen pixels.</summary>
    private const int Inset = 4;
    private const int FaceEdge = 2;

    /// <summary>How much a face is blown up.</summary>
    private const float FaceScale = 2f;

    /// <summary>The bars: the health's height, the Focus bar's, and the room between.</summary>
    private const int HealthHeight = 14;
    private const int FocusHeight = 6;
    private const int BarGap = 3;

    /// <summary>How many auras go beside a row, and the room between the row and them.</summary>
    private const int AurasBeside = 5;
    private const int AuraGap = 6;

    /// <summary>How dim the row of someone out of the fight is.</summary>
    private const float OutDim = 0.5f;

    /// <summary>The faces' frames: the party's blue, and gold for your target.</summary>
    private static readonly Color FaceFrame = new(86, 104, 164);

    /// <summary>A downed row's health bar: grey, full, with DOWN on it.</summary>
    private static readonly Color DownBar = new(64, 58, 66);

    private const string Down = "DOWN";

    private readonly HealthLag _lag = new();

    /// <summary>The top of the first row, in screen pixels: under your frame and its aura row. The screen sets it each frame from how many auras you have.</summary>
    public int Top { get; set; } = PlayerFrame.Top + FighterFrame.Height + 52;

    /// <summary>Where each row is.</summary>
    /// <param name="count">How many rows.</param>
    public Rectangle[] Rows(int count)
    {
        var rows = new Rectangle[count];
        for (int i = 0; i < count; i++) rows[i] = new Rectangle(Left, Top + i * (RowHeight + RowGap), Width, RowHeight);
        return rows;
    }

    /// <summary>Which row the mouse is on, or -1.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="count">How many rows.</param>
    public int At(Point mouse, int count)
    {
        Rectangle[] rows = Rows(count);
        for (int i = 0; i < rows.Length; i++)
            if (rows[i].Contains(mouse)) return i;
        return -1;
    }

    /// <summary>Which aura beside a row the mouse is on, or null.</summary>
    /// <param name="mouse">Where the mouse is.</param>
    /// <param name="party">The rows' people.</param>
    public AuraView AuraAt(Point mouse, IReadOnlyList<FighterHudView> party)
    {
        Rectangle[] rows = Rows(party.Count);
        for (int i = 0; i < rows.Length; i++)
        {
            IReadOnlyList<AuraView> auras = Shown(party[i].Auras);
            int a = AuraRow.At(mouse, auras.Count, AurasAt(rows[i]), AurasBeside, false);
            if (a >= 0) return auras[a];
        }
        return null;
    }

    /// <summary>What the auras beside each row cover on the screen, timers and all, for a row that has any.</summary>
    /// <param name="party">The rows' people.</param>
    public IEnumerable<Rectangle> AuraAreas(IReadOnlyList<FighterHudView> party)
    {
        Rectangle[] rows = Rows(party.Count);
        for (int i = 0; i < rows.Length; i++)
        {
            Rectangle area = AuraRow.Area(Shown(party[i].Auras).Count, AurasAt(rows[i]), AurasBeside, false);
            if (!area.IsEmpty) yield return area;
        }
    }

    /// <summary>Draws each row: its panel, face, bars or death saves, and the auras beside it. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="party">Everyone in the party but you, in the order the rows go.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawArt(SpriteBatch spriteBatch, HudKit kit, IReadOnlyList<FighterHudView> party, float alpha)
    {
        Rectangle[] rows = Rows(party.Count);
        for (int i = 0; i < party.Count; i++)
        {
            FighterHudView member = party[i];
            Rectangle row = rows[i];
            float a = alpha * (member.IsOut ? OutDim : 1f);
            CombatStyle.DrawPanel(spriteBatch, kit.Pixel, row, a);

            Rectangle face = FaceWell(row);
            Rectangle frame = face;
            frame.Inflate(FaceEdge, FaceEdge);
            spriteBatch.Draw(kit.Pixel, frame, (member.IsTarget ? Palette.Gold : FaceFrame) * a);
            spriteBatch.Draw(kit.Pixel, face, HudKit.BarBack * a);
            HudKit.Face(spriteBatch, member.Portrait, face, HudKit.FaceTint(member.IsDown, member.IsOut) * a);
            if (member.IsTarget) kit.Edge(spriteBatch, row, 2, Palette.Gold * a);

            Rectangle health = HealthBar(kit, row);
            float share = member.IsDown ? 0f : member.HealthShare;
            float lag = member.IsDown ? 0f : _lag.Step(member.Id, share, kit.Delta);
            kit.Bar(spriteBatch, health, member.IsDown ? 1f : 0f, DownBar, a);
            Rectangle inside = HudKit.Inside(health);
            kit.Fill(spriteBatch, inside, share, lag, HudKit.Lag, a * 0.85f);
            kit.Fill(spriteBatch, inside, 0f, share, HudKit.PartyHealth, a);
            if (member.TempHp > 0 && member.MaxHp > 0)
            {
                float temp = (float)member.TempHp / member.MaxHp;
                float from = MathF.Min(share, 1f - temp);
                kit.Fill(spriteBatch, inside, from, from + temp, HudKit.TempHp, a * 0.9f);
            }

            Rectangle focus = FocusBar(health);
            if (member.IsDown) DrawDeathSaves(spriteBatch, kit, member, focus, a);
            else if (member.MaxFocus > 0) kit.Bar(spriteBatch, focus, (float)member.Focus / member.MaxFocus, HudKit.Focus, a);

            AuraRow.DrawArt(spriteBatch, kit, Shown(member.Auras), AurasAt(row), AurasBeside, false, a);
        }
    }

    /// <summary>Writes each row's name, its hit points or DOWN, and its auras' stacks. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="party">Everyone in the party but you.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawWords(SpriteBatch spriteBatch, HudKit kit, IReadOnlyList<FighterHudView> party, float alpha)
    {
        Rectangle[] rows = Rows(party.Count);
        for (int i = 0; i < party.Count; i++)
        {
            FighterHudView member = party[i];
            Rectangle row = rows[i];
            float a = alpha * (member.IsOut ? OutDim : 1f);
            Rectangle health = HealthBar(kit, row);
            Text.Draw(spriteBatch, kit.Words, member.Name.ToUpperInvariant(), health.X, row.Y + 1, (member.IsTarget ? Palette.Gold : Palette.Heading) * a,
                Palette.Shadow * a, width: health.Width);
            string hp = member.IsDown ? Down : member.Hp.ToString(CultureInfo.InvariantCulture) + " / " + member.MaxHp.ToString(CultureInfo.InvariantCulture);
            HudKit.Outlined(spriteBatch, kit.Small, hp, health.Center.ToVector2(), (member.IsDown ? Palette.Ember : Color.White) * a, a, align: 0.5f, middle: true);
            AuraRow.DrawWords(spriteBatch, kit, Shown(member.Auras), AurasAt(row), AurasBeside, false, a);
        }
    }

    /// <summary>Forgets the health lags, for a new fight.</summary>
    public void Reset() => _lag.Clear();

    /// <summary>The auras that fit beside a row.</summary>
    private static IReadOnlyList<AuraView> Shown(IReadOnlyList<AuraView> auras)
    {
        if (auras is null) return Array.Empty<AuraView>();
        if (auras.Count <= AurasBeside) return auras;
        var shown = new AuraView[AurasBeside];
        for (int i = 0; i < AurasBeside; i++) shown[i] = auras[i];
        return shown;
    }

    /// <summary>Where a row's face goes: its bust, twice its size, at the row's left.</summary>
    private static Rectangle FaceWell(Rectangle row)
    {
        int w = (int)(CombatArt.Bust.Width * FaceScale), h = (int)(CombatArt.Bust.Height * FaceScale);
        return new Rectangle(row.X + Inset + FaceEdge, row.Y + (row.Height - h) / 2, w, h);
    }

    /// <summary>Where a row's health bar is: under its name, right of the face.</summary>
    private static Rectangle HealthBar(HudKit kit, Rectangle row)
    {
        Rectangle face = FaceWell(row);
        int left = face.Right + FaceEdge + 8;
        int top = row.Bottom - Inset - FocusHeight - BarGap - HealthHeight - 2;
        return new Rectangle(left, top, row.Right - Inset - 4 - left, HealthHeight);
    }

    /// <summary>Where a row's Focus bar is: under its health.</summary>
    private static Rectangle FocusBar(Rectangle health) => new(health.X, health.Bottom + BarGap, health.Width, FocusHeight);

    /// <summary>Where the auras beside a row start.</summary>
    private static Point AurasAt(Rectangle row) => new(row.Right + AuraGap, row.Y + (row.Height - AuraRow.Box) / 2);

    /// <summary>The death saves, small, where the Focus bar goes.</summary>
    private static void DrawDeathSaves(SpriteBatch spriteBatch, HudKit kit, FighterHudView member, Rectangle focus, float alpha)
    {
        const int pip = 6, gap = 3;
        int x = focus.X;
        for (int i = 0; i < 6; i++)
        {
            bool success = i < 3;
            int n = success ? i : i - 3;
            bool lit = success ? n < member.DeathSuccesses : n < member.DeathFailures;
            Color colour = !lit ? HudKit.BarBack : success ? HudKit.Good : HudKit.Bad;
            var box = new Rectangle(x, focus.Y, pip, pip);
            spriteBatch.Draw(kit.Pixel, new Rectangle(box.X - 1, box.Y - 1, box.Width + 2, box.Height + 2), HudKit.BarEdge * alpha);
            spriteBatch.Draw(kit.Pixel, box, colour * alpha);
            x += pip + gap + (i == 2 ? gap * 2 : 0);
        }
    }
}
