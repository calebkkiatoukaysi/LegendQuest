using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The fight's bar along the bottom of the screen, and the hotbar on it: six primary buttons (Auto,
/// the three skills, the ultimate and the potion) on keys 1 to 6, the shared strip (Dodge, Help,
/// something to throw and the elixir) on 7, 8, 9 and 0, and END TURN at the end.
/// </summary>
/// <remarks>
/// The bar is a dark panel the full width of the screen, BarHeight tall, low enough that the front
/// row's feet stand clear of it. The buttons are hud.png's slot frames blown up twice with the
/// ability's icon in the well: plain, lit under the mouse, gold when picked and waiting for a
/// target, and dark with the icon greyed when it can't be used (the card says why). A primary button
/// with nothing in its place in the kit is an empty dark well. Each button has its key in the top
/// corner, its Focus cost in blue in the bottom corner (or what's left in the bag, in gold), and
/// while it's cooling down a veil over it with the turns left in the middle. The tutorial can point
/// at a button, and it pulses gold.
/// </remarks>
public class CombatHotbar
{
    /// <summary>What END TURN says, and its key.</summary>
    private const string EndTurnLabel = "END TURN";
    private const string EndTurnKey = "SPACE";

    /// <summary>How tall the bar along the bottom is, in screen pixels.</summary>
    public const int BarHeight = 88;

    /// <summary>How much the hud art is blown up.</summary>
    private const float Scale = 2f;

    /// <summary>A button's frame in art pixels.</summary>
    private const int Frame = 30;

    /// <summary>Where the primary buttons start across the screen, right of the frame, the budget and the sigil, in screen pixels.</summary>
    public const int Left = 580;

    /// <summary>How far down the bar the buttons sit, in screen pixels.</summary>
    private const int TopInset = 14;

    /// <summary>The room between two buttons, before the strip, and before END TURN, in screen pixels.</summary>
    private const int Gap = 4;
    private const int StripGap = 18;
    private const int EndTurnGap = 24;

    /// <summary>How far in from a button's corner its key and its numbers go, in screen pixels.</summary>
    private const int CornerInset = 5;

    /// <summary>How far down from END TURN's top its label starts, in screen pixels.</summary>
    private const int LabelDrop = 4;

    /// <summary>How bright the tutorial's edge is at the bottom of its pulse, against its top.</summary>
    private const float GuideLeast = 0.45f;

    /// <summary>How fast the tutorial's pulse goes, in radians a second, and how thick its edge is.</summary>
    private const float GuideRate = 6f;
    private const int GuideEdge = 3;

    /// <summary>How dark the veil over a button that's cooling down is.</summary>
    private const float CooldownVeil = 0.6f;

    /// <summary>What an icon's multiplied by when its button can't be used.</summary>
    private static readonly Color Greyed = new(96, 96, 108);

    private CombatArt _art;
    private Texture2D _pixel;
    private SpriteFont _font;
    private SpriteFont _numberFont;
    private float _clock;

    /// <summary>A button's size on screen.</summary>
    public static int SlotSize => (int)(Frame * Scale);

    /// <summary>The top of the bar on screen.</summary>
    public static int BarTop => LegendQuestGame.ScreenHeight - BarHeight;

    /// <summary>The top of the buttons on screen.</summary>
    public static int Top => BarTop + TopInset;

    /// <summary>Where the bar is on screen: the whole width along the bottom.</summary>
    public static Rectangle Bar => new(0, BarTop, LegendQuestGame.ScreenWidth, BarHeight);

    /// <summary>Loads the fonts, and keeps the shared art and the pixel.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="art">The fight's sheets.</param>
    /// <param name="pixel">One white pixel: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, CombatArt art, Texture2D pixel)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _numberFont = content.Load<SpriteFont>("spectral-ui");
        _art = art;
        _pixel = pixel;
    }

    /// <summary>Runs the tutorial's pulse.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime) => _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;

    /// <summary>Where each button is on screen: the primary ones, then a gap, then the strip.</summary>
    /// <param name="entries">The buttons, primary first.</param>
    public static Rectangle[] Slots(IReadOnlyList<HotbarEntry> entries)
    {
        var slots = new Rectangle[entries.Count];
        int x = Left;
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0) x += entries[i].Primary != entries[i - 1].Primary ? StripGap : Gap;
            slots[i] = new Rectangle(x, Top, SlotSize, SlotSize);
            x += SlotSize;
        }
        return slots;
    }

    /// <summary>Where END TURN is: after the last button.</summary>
    /// <param name="entries">The buttons.</param>
    public static Rectangle EndTurn(IReadOnlyList<HotbarEntry> entries)
    {
        Rectangle[] slots = Slots(entries);
        int x = slots.Length == 0 ? Left : slots[^1].Right + EndTurnGap;
        return new Rectangle(x, Top, SlotSize, SlotSize);
    }

    /// <summary>Which button the mouse is on.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="entries">The buttons.</param>
    /// <returns>Its place on the hotbar, or null.</returns>
    public static int? At(Point mouse, IReadOnlyList<HotbarEntry> entries)
    {
        Rectangle[] slots = Slots(entries);
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].Contains(mouse)) return i;
        return null;
    }

    /// <summary>Draws the bar's panel, the full width along the bottom. Goes first, under everything else on it.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, deferred.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawBar(SpriteBatch spriteBatch, float alpha) => CombatStyle.DrawPanel(spriteBatch, _pixel, Bar, alpha);

    /// <summary>Draws the buttons, their icons and END TURN. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="entries">The buttons.</param>
    /// <param name="hovered">Which button the mouse is on, or -1.</param>
    /// <param name="endTurnLit">Whether END TURN is under the mouse.</param>
    /// <param name="endTurnGuided">Whether the tutorial's pointing at END TURN.</param>
    /// <param name="enabled">Whether it's yours to press: on anyone else's turn the whole bar is dark.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawArt(SpriteBatch spriteBatch, IReadOnlyList<HotbarEntry> entries, int hovered, bool endTurnLit, bool endTurnGuided, bool enabled, float alpha)
    {
        Rectangle[] slots = Slots(entries);
        int inset = (int)(CombatIcons.FrameInset * Scale);
        for (int i = 0; i < entries.Count; i++)
        {
            HotbarEntry entry = entries[i];
            bool usable = entry.Ability is not null && entry.Usable && enabled;
            string frame = entry.Selected ? "hotbar-selected" : !usable ? "hotbar-disabled" : i == hovered ? "hotbar-hover" : "hotbar-normal";
            _art.Piece(spriteBatch, frame, slots[i].Location.ToVector2(), Scale, Color.White * alpha);
            if (entry.Ability is null) continue;

            var well = new Rectangle(slots[i].X + inset, slots[i].Y + inset, slots[i].Width - inset * 2, slots[i].Height - inset * 2);
            _art.Ability(spriteBatch, entry.Icon, well.Location.ToVector2(), Scale, (usable ? Color.White : Greyed) * alpha);
            if (entry.Cooldown > 0) spriteBatch.Draw(_pixel, well, Color.Black * (CooldownVeil * alpha));
            if (entry.Guided) DrawGuide(spriteBatch, slots[i], alpha);
        }

        Rectangle end = EndTurn(entries);
        _art.Piece(spriteBatch, !enabled ? "hotbar-disabled" : endTurnLit ? "hotbar-hover" : "hotbar-normal", end.Location.ToVector2(), Scale, Color.White * alpha);
        _art.Ability(spriteBatch, CombatIcons.Ability("end-turn"), new Vector2(end.X + inset, end.Y + inset), Scale, (enabled ? Color.White : Greyed) * alpha);
        if (endTurnGuided) DrawGuide(spriteBatch, end, alpha);
    }

    /// <summary>Writes each button's key, its Focus cost or its count, the turns left on a cooldown, and END TURN's label and key. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="entries">The buttons.</param>
    /// <param name="enabled">Whether it's yours to press.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, IReadOnlyList<HotbarEntry> entries, bool enabled, float alpha)
    {
        Rectangle[] slots = Slots(entries);
        for (int i = 0; i < entries.Count; i++)
        {
            HotbarEntry entry = entries[i];
            Rectangle slot = slots[i];
            bool usable = entry.Ability is not null && entry.Usable && enabled;
            Text.Draw(spriteBatch, _font, entry.Key, slot.Left + CornerInset, slot.Top, (usable ? Palette.Bone : Palette.Note) * alpha, Palette.Shadow * alpha);
            if (entry.Ability is null) continue;

            float bottom = slot.Bottom - _font.LineSpacing;
            if (entry.Focus > 0)
                Text.Draw(spriteBatch, _font, entry.Focus.ToString(System.Globalization.CultureInfo.InvariantCulture), slot.Right - CornerInset, bottom,
                    CombatStyle.FocusText * alpha, Palette.Shadow * alpha, rightAligned: true);
            else if (entry.Count is string count)
                Text.Draw(spriteBatch, _font, count, slot.Right - CornerInset, bottom, Palette.Gold * alpha, Palette.Shadow * alpha, rightAligned: true);

            if (entry.Cooldown > 0)
            {
                string turns = entry.Cooldown.ToString(System.Globalization.CultureInfo.InvariantCulture);
                Vector2 size = _numberFont.MeasureString(turns);
                Text.Draw(spriteBatch, _numberFont, turns, slot.Center.X - size.X / 2f, slot.Center.Y - size.Y / 2f, Palette.Bone * alpha, Palette.Shadow * alpha);
            }
        }

        Rectangle end = EndTurn(entries);
        Color label = (enabled ? Palette.Torch : Palette.Note) * alpha;
        Text.Draw(spriteBatch, _font, EndTurnLabel, end.Right + CornerInset * 2, end.Top + LabelDrop, label, Palette.Shadow * alpha);
        Text.Draw(spriteBatch, _font, EndTurnKey, end.Right + CornerInset * 2, end.Top + LabelDrop + _font.LineSpacing, Palette.Note * alpha, Palette.Shadow * alpha);
    }

    /// <summary>Whether a point is on END TURN or its label.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="entries">The buttons.</param>
    public bool OnEndTurn(Point mouse, IReadOnlyList<HotbarEntry> entries)
    {
        Rectangle end = EndTurn(entries);
        end.Width += CornerInset * 2 + (int)_font.MeasureString(EndTurnLabel).X;
        return end.Contains(mouse);
    }

    /// <summary>The tutorial's pulsing gold edge round a button.</summary>
    private void DrawGuide(SpriteBatch spriteBatch, Rectangle slot, float alpha)
    {
        float pulse = 0.5f + 0.5f * MathF.Sin(_clock * GuideRate);
        Color edge = Palette.Gold * (MathHelper.Lerp(GuideLeast, 1f, pulse) * alpha);
        Rectangle box = slot;
        box.Inflate(GuideEdge, GuideEdge);
        spriteBatch.Draw(_pixel, new Rectangle(box.Left, box.Top, box.Width, GuideEdge), edge);
        spriteBatch.Draw(_pixel, new Rectangle(box.Left, box.Bottom - GuideEdge, box.Width, GuideEdge), edge);
        spriteBatch.Draw(_pixel, new Rectangle(box.Left, box.Top, GuideEdge, box.Height), edge);
        spriteBatch.Draw(_pixel, new Rectangle(box.Right - GuideEdge, box.Top, GuideEdge, box.Height), edge);
    }
}
