using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The frame at the bar's left end in a fight, for whoever's turn it is: their face, their name,
/// what they are and their armour, their hit points, their Focus, what's on them, and what they
/// always have (Sneak Attack, the Defence style).
/// </summary>
/// <remarks>
/// It stands in for the party's cards while a fight's on, since it's always the one whose turn it is
/// you need to read, foe or friend. It rises a little above the bar, over the empty floor left of
/// your back row. The health bar drains with what's shown, like the turn order's. The Focus bar is
/// three-sliced off hud.png with a tick every ten: what's left in blue, and what the skill you're
/// pointing at would spend pale at its end, or the whole of it red if there isn't enough. The
/// conditions and the always-on abilities are icons; pointing at one says what it is.
/// </remarks>
public class UnitFrame
{
    /// <summary>Where the frame is and how big, in screen pixels: it rises Rise above the bar.</summary>
    private const int Left = 12;
    private const int Width = 400;
    private const int Rise = 52;

    /// <summary>How much the portrait is blown up, and how far in from the frame's corner it sits.</summary>
    private const float PortraitScale = 2f;
    private const int Inset = 12;

    /// <summary>The room between the portrait and the words, in screen pixels.</summary>
    private const int WordsGap = 12;

    /// <summary>How far down the frame the name starts, in screen pixels.</summary>
    private const int NameDrop = 4;

    /// <summary>The bars: how long, the health bar's height, the Focus bar's scale, and how far the second is under the first, in screen pixels.</summary>
    private const int BarWidth = 150;
    private const int HealthHeight = 10;
    private const float FocusScale = 2f;
    private const int BarSpacing = 12;

    /// <summary>The Focus pieces' caps and their middle on hud.png, in art pixels: three at each end, and the two columns between stretched.</summary>
    private const int Cap = 3;
    private const int Middle = 2;

    /// <summary>How much Focus a tick on the bar marks.</summary>
    private const int FocusTick = 10;

    /// <summary>The room between a bar and the number after it, in screen pixels.</summary>
    private const int NumberGap = 8;

    /// <summary>How big the icons in the bottom row are drawn: the conditions, and the always-on abilities.</summary>
    private const float ConditionScale = 2f;
    private const float PassiveScale = 1f;
    private const int IconGap = 4;

    private static readonly Color BarBack = new(16, 14, 24);

    private CombatArt _art;
    private Texture2D _pixel;
    private SpriteFont _font;

    /// <summary>Where the frame is on screen.</summary>
    public static Rectangle Bounds => new(Left, CombatHotbar.BarTop - Rise, Width, CombatHotbar.BarHeight + Rise);

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

    /// <summary>Where each icon in the bottom row is: the conditions, then the always-on abilities.</summary>
    /// <param name="conditions">How many conditions are on them.</param>
    /// <param name="passives">How many always-on abilities they have.</param>
    public Rectangle[] Icons(int conditions, int passives)
    {
        var icons = new Rectangle[conditions + passives];
        int x = Left + Inset;
        int bottom = Bounds.Bottom - Inset / 2;
        for (int i = 0; i < icons.Length; i++)
        {
            float scale = i < conditions ? ConditionScale : PassiveScale;
            int size = (int)((i < conditions ? CombatIcons.ConditionSize : CombatIcons.AbilitySize) * scale);
            icons[i] = new Rectangle(x, bottom - size, size, size);
            x += size + IconGap;
        }
        return icons;
    }

    /// <summary>Draws the panel, the portrait, the health and Focus bars and the icons. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="fighter">Whose turn it is.</param>
    /// <param name="passives">Their always-on abilities.</param>
    /// <param name="spend">The Focus the skill being pointed at would spend, or 0.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawArt(SpriteBatch spriteBatch, FighterView fighter, IReadOnlyList<AbilityDefinition> passives, int spend, float alpha)
    {
        Rectangle bounds = Bounds;
        CombatStyle.DrawPanel(spriteBatch, _pixel, bounds, alpha);

        var frame = new Vector2(bounds.X + Inset, bounds.Y + Inset);
        _art.Piece(spriteBatch, "portrait-current", frame, PortraitScale, Color.White * alpha);
        int inset = (int)(CombatIcons.FrameInset * PortraitScale);
        var well = new Rectangle((int)frame.X + inset, (int)frame.Y + inset, (int)(CombatArt.Bust.Width * PortraitScale), (int)(CombatArt.Bust.Height * PortraitScale));
        CombatArt.Face(spriteBatch, fighter, well, alpha);

        Rectangle bar = HealthBar;
        spriteBatch.Draw(_pixel, bar, BarBack * alpha);
        float health = fighter.MaxHp > 0 ? MathHelper.Clamp((float)fighter.Hp / fighter.MaxHp, 0f, 1f) : 0f;
        Color fill = fighter.Foe ? CombatStyle.FoeHealth : CombatStyle.PartyHealth;
        spriteBatch.Draw(_pixel, new Rectangle(bar.X, bar.Y, (int)MathF.Round(bar.Width * health), bar.Height), fill * alpha);

        DrawFocus(spriteBatch, fighter.Focus, spend, alpha);

        Rectangle[] icons = Icons(fighter.Conditions.Count, passives.Count);
        for (int i = 0; i < fighter.Conditions.Count; i++)
            _art.Condition(spriteBatch, fighter.Conditions[i], icons[i].Location.ToVector2(), ConditionScale, alpha);
        for (int i = 0; i < passives.Count; i++)
            _art.Ability(spriteBatch, CombatIcons.Ability(passives[i].Id), icons[fighter.Conditions.Count + i].Location.ToVector2(), PassiveScale, Color.White * alpha);
    }

    /// <summary>Writes the name, what they are and their armour, their hit points and their Focus. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="fighter">Whose turn it is.</param>
    /// <param name="what">What they are: "LEVEL 1 BLADEMASTER", or a foe's kind.</param>
    /// <param name="spend">The Focus the skill being pointed at would spend, or 0.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, FighterView fighter, string what, int spend, float alpha)
    {
        Rectangle bounds = Bounds;
        float y = bounds.Top + NameDrop;
        Text.Draw(spriteBatch, _font, fighter.Name.ToUpperInvariant(), WordsLeft, y, (fighter.Foe ? CombatStyle.FoeLine : Palette.Heading) * alpha, Palette.Shadow * alpha);
        Text.Draw(spriteBatch, _font, $"{what}  ·  AC {fighter.ArmourClass}", WordsLeft, y + _font.LineSpacing, Palette.Note * alpha, Palette.Shadow * alpha);

        Rectangle health = HealthBar;
        Text.Draw(spriteBatch, _font, $"{fighter.Hp} / {fighter.MaxHp} HP", health.Right + NumberGap, health.Center.Y - _font.LineSpacing / 2f,
            Palette.Bone * alpha, Palette.Shadow * alpha);

        Rectangle focus = FocusBar;
        string focusText = fighter.Focus.ToString(CultureInfo.InvariantCulture) + " FOCUS";
        Color focusColour = spend > fighter.Focus ? Palette.Ember : CombatStyle.FocusText;
        Text.Draw(spriteBatch, _font, focusText, focus.Right + NumberGap, focus.Center.Y - _font.LineSpacing / 2f, focusColour * alpha, Palette.Shadow * alpha);
    }

    /// <summary>Where the words start across the screen: right of the portrait.</summary>
    private static int WordsLeft => Left + Inset + (int)(CombatIcons.Piece("portrait-current").Width * PortraitScale) + WordsGap;

    /// <summary>Where the health bar is: under the name and what they are.</summary>
    private Rectangle HealthBar => new(WordsLeft, Bounds.Top + NameDrop + _font.LineSpacing * 2 + BarSpacing / 2, BarWidth, HealthHeight);

    /// <summary>Where the Focus bar is: under the health bar.</summary>
    private Rectangle FocusBar
    {
        get
        {
            Rectangle health = HealthBar;
            int height = (int)(CombatIcons.Piece("focus-track").Height * FocusScale);
            return new Rectangle(health.X, health.Bottom + BarSpacing, BarWidth, height);
        }
    }

    /// <summary>The Focus bar: the track, the fill, what a skill would spend pale at the fill's end (or the fill red if there isn't enough), and a tick every ten.</summary>
    private void DrawFocus(SpriteBatch spriteBatch, int focus, int spend, float alpha)
    {
        Rectangle bar = FocusBar;
        int left = (int)MathF.Round(bar.Width * MathHelper.Clamp((float)focus / Combatant.MaxFocus, 0f, 1f));
        int used = (int)MathF.Round(bar.Width * MathHelper.Clamp((float)Math.Min(spend, focus) / Combatant.MaxFocus, 0f, 1f));

        ThreeSlice(spriteBatch, "focus-track", bar, alpha);
        if (spend > focus)
        {
            ThreeSlice(spriteBatch, "focus-over", new Rectangle(bar.X, bar.Y, left, bar.Height), alpha);
        }
        else
        {
            ThreeSlice(spriteBatch, "focus-fill", new Rectangle(bar.X, bar.Y, left - used, bar.Height), alpha);
            ThreeSlice(spriteBatch, "focus-preview", new Rectangle(bar.X + left - used, bar.Y, used, bar.Height), alpha);
        }

        Rectangle tick = CombatIcons.Piece("focus-tick");
        for (int i = FocusTick; i < Combatant.MaxFocus; i += FocusTick)
        {
            float x = bar.X + bar.Width * ((float)i / Combatant.MaxFocus) - tick.Width * FocusScale / 2f;
            _art.Piece(spriteBatch, "focus-tick", new Vector2(MathF.Round(x), bar.Y), FocusScale, Color.White * alpha);
        }
    }

    /// <summary>One of the Focus pieces stretched to a length: its two caps at their size and its middle stretched between.</summary>
    private void ThreeSlice(SpriteBatch spriteBatch, string piece, Rectangle area, float alpha)
    {
        if (area.Width <= 0) return;

        Rectangle source = CombatIcons.Piece(piece);
        int cap = Math.Min((int)(Cap * FocusScale), area.Width / 2);
        Color colour = Color.White * alpha;
        spriteBatch.Draw(_art.Hud, new Rectangle(area.X, area.Y, cap, area.Height), new Rectangle(source.X, source.Y, Cap, source.Height), colour);
        spriteBatch.Draw(_art.Hud, new Rectangle(area.X + cap, area.Y, area.Width - cap * 2, area.Height), new Rectangle(source.X + Cap, source.Y, Middle, source.Height), colour);
        spriteBatch.Draw(_art.Hud, new Rectangle(area.Right - cap, area.Y, cap, area.Height), new Rectangle(source.Right - Cap, source.Y, Cap, source.Height), colour);
    }
}
