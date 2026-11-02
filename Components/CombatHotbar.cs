using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The hotbar along the bottom of the screen in a fight: a slot for each thing whoever's turn it is
/// can do, the key for each in its corner, and END TURN at the end.
/// </summary>
/// <remarks>
/// hud.png's slot frames, blown up twice, with the ability's icon in the well. A slot is plain,
/// lit under the mouse, gold when it's picked and waiting for a target, and dark with its icon greyed
/// when it can't be used (the card says why). The slots come in groups with a gap between: the
/// fighting ones, the Soul-Link, moving about, and the bag. The keys are 1 to 0, then - and =, left
/// to right. What's left of what a slot spends (spell slots, potions in the bag) is in its bottom
/// corner. The tutorial can point at a slot, and it pulses gold.
/// </remarks>
public class CombatHotbar
{
    /// <summary>The keys the slots go by, left to right.</summary>
    public static readonly IReadOnlyList<string> KeyNames = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=" };

    /// <summary>What END TURN says, and its key.</summary>
    private const string EndTurnLabel = "END TURN";
    private const string EndTurnKey = "SPACE";

    /// <summary>How much the hud art is blown up.</summary>
    private const float Scale = 2f;

    /// <summary>A slot's frame in art pixels.</summary>
    private const int Frame = 30;

    /// <summary>Where the hotbar starts across the screen, and how far its bottom is from the screen's, in screen pixels: right of the frame of whoever's turn it is and the sigil.</summary>
    public const int Left = 540;
    private const int BottomMargin = 26;

    /// <summary>The room between two slots, between two groups, and before END TURN, in screen pixels.</summary>
    private const int Gap = 4;
    private const int GroupGap = 16;
    private const int EndTurnGap = 28;

    /// <summary>How far in from a slot's corner its key and its count go, in screen pixels.</summary>
    private const int CornerInset = 5;

    /// <summary>How far down from END TURN's top its label starts, in screen pixels.</summary>
    private const int LabelDrop = 4;

    /// <summary>How bright the tutorial's edge is at the bottom of its pulse, against its top.</summary>
    private const float GuideLeast = 0.45f;

    /// <summary>How fast the tutorial's pulse goes, in radians a second, and how thick its edge is.</summary>
    private const float GuideRate = 6f;
    private const int GuideEdge = 3;

    /// <summary>What an icon's multiplied by when its slot can't be used.</summary>
    private static readonly Color Greyed = new(96, 96, 108);

    private CombatArt _art;
    private Texture2D _pixel;
    private SpriteFont _font;
    private float _clock;

    /// <summary>A slot's size on screen.</summary>
    public static int SlotSize => (int)(Frame * Scale);

    /// <summary>The top of the slots on screen.</summary>
    public static int Top => LegendQuestGame.ScreenHeight - BottomMargin - SlotSize;

    /// <summary>Loads the font, and keeps the shared art and the pixel.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="art">The fight's sheets.</param>
    /// <param name="pixel">One white pixel: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, CombatArt art, Texture2D pixel)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _art = art;
        _pixel = pixel;
    }

    /// <summary>Runs the tutorial's pulse.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime) => _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;

    /// <summary>Where each slot is on screen, a gap between groups.</summary>
    /// <param name="entries">The slots.</param>
    public static Rectangle[] Slots(IReadOnlyList<HotbarEntry> entries)
    {
        var slots = new Rectangle[entries.Count];
        int x = Left;
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0) x += entries[i].Group != entries[i - 1].Group ? GroupGap : Gap;
            slots[i] = new Rectangle(x, Top, SlotSize, SlotSize);
            x += SlotSize;
        }
        return slots;
    }

    /// <summary>Where END TURN is: after the last slot.</summary>
    /// <param name="entries">The slots.</param>
    public static Rectangle EndTurn(IReadOnlyList<HotbarEntry> entries)
    {
        Rectangle[] slots = Slots(entries);
        int x = slots.Length == 0 ? Left : slots[^1].Right + EndTurnGap;
        return new Rectangle(x, Top, SlotSize, SlotSize);
    }

    /// <summary>Which slot the mouse is on.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="entries">The slots.</param>
    /// <returns>Its place on the hotbar, or null.</returns>
    public static int? At(Point mouse, IReadOnlyList<HotbarEntry> entries)
    {
        Rectangle[] slots = Slots(entries);
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].Contains(mouse)) return i;
        return null;
    }

    /// <summary>Draws the slots, their icons and END TURN. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="entries">The slots.</param>
    /// <param name="hovered">Which slot the mouse is on, or -1.</param>
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
            bool usable = entry.Usable && enabled;
            string frame = entry.Selected ? "hotbar-selected" : !usable ? "hotbar-disabled" : i == hovered ? "hotbar-hover" : "hotbar-normal";
            _art.Piece(spriteBatch, frame, slots[i].Location.ToVector2(), Scale, Color.White * alpha);
            _art.Ability(spriteBatch, entry.Icon, new Vector2(slots[i].X + inset, slots[i].Y + inset), Scale, (usable ? Color.White : Greyed) * alpha);
            if (entry.Guided) DrawGuide(spriteBatch, slots[i], alpha);
        }

        Rectangle end = EndTurn(entries);
        _art.Piece(spriteBatch, !enabled ? "hotbar-disabled" : endTurnLit ? "hotbar-hover" : "hotbar-normal", end.Location.ToVector2(), Scale, Color.White * alpha);
        _art.Ability(spriteBatch, CombatIcons.Ability("end-turn"), new Vector2(end.X + inset, end.Y + inset), Scale, (enabled ? Color.White : Greyed) * alpha);
        if (endTurnGuided) DrawGuide(spriteBatch, end, alpha);
    }

    /// <summary>Writes each slot's key and count, and END TURN's label and key. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="entries">The slots.</param>
    /// <param name="enabled">Whether it's yours to press.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, IReadOnlyList<HotbarEntry> entries, bool enabled, float alpha)
    {
        Rectangle[] slots = Slots(entries);
        for (int i = 0; i < entries.Count; i++)
        {
            Color key = (entries[i].Usable && enabled ? Palette.Bone : Palette.Note) * alpha;
            if (i < KeyNames.Count) Text.Draw(spriteBatch, _font, KeyNames[i], slots[i].Left + CornerInset, slots[i].Top, key, Palette.Shadow * alpha);
            if (entries[i].Count is string count)
                Text.Draw(spriteBatch, _font, count, slots[i].Right - CornerInset, slots[i].Bottom - _font.LineSpacing, Palette.Gold * alpha, Palette.Shadow * alpha, rightAligned: true);
        }

        Rectangle end = EndTurn(entries);
        Color label = (enabled ? Palette.Torch : Palette.Note) * alpha;
        Text.Draw(spriteBatch, _font, EndTurnLabel, end.Right + CornerInset * 2, end.Top + LabelDrop, label, Palette.Shadow * alpha);
        Text.Draw(spriteBatch, _font, EndTurnKey, end.Right + CornerInset * 2, end.Top + LabelDrop + _font.LineSpacing, Palette.Note * alpha, Palette.Shadow * alpha);
    }

    /// <summary>Whether a point is on END TURN or its label.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="entries">The slots.</param>
    public bool OnEndTurn(Point mouse, IReadOnlyList<HotbarEntry> entries)
    {
        Rectangle end = EndTurn(entries);
        end.Width += CornerInset * 2 + (int)_font.MeasureString(EndTurnLabel).X;
        return end.Contains(mouse);
    }

    /// <summary>The tutorial's pulsing gold edge round a slot.</summary>
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
