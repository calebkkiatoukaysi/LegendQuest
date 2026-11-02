using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The frame in the bottom left in a fight, for whoever's turn it is: their face, their name and
/// what they are, their hit points written out, their armour and speed, what's on them, and what
/// they always have (Sneak Attack, the Defence style).
/// </summary>
/// <remarks>
/// It stands in for the party's cards while a fight's on, since it's always the one whose turn it is
/// you need to read, foe or friend. The bar drains with what's shown, like the turn order's. The
/// conditions and the always-on abilities are icons; pointing at one says what it is.
/// </remarks>
public class UnitFrame
{
    /// <summary>Where the frame is and how big, in screen pixels.</summary>
    private const int Left = 24;
    private const int Width = 420;
    private const int Height = 150;
    private const int BottomMargin = 24;

    /// <summary>How much the portrait is blown up, and how far in from the frame's corner it sits.</summary>
    private const float PortraitScale = 3f;
    private const int Inset = 12;

    /// <summary>Where the words start, right of the portrait, and the health bar's size, in screen pixels.</summary>
    private const int WordsGap = 14;

    /// <summary>How far the name sits above the inset, so its letters line up with the portrait's frame, and the room round the health bar, in screen pixels.</summary>
    private const int NameLift = 2;
    private const int BarRoom = 5;
    private const int BarWidth = 120;
    private const int BarHeight = 10;

    /// <summary>How big the icons in the bottom row are drawn: the conditions, and the always-on abilities.</summary>
    private const float ConditionScale = 2f;
    private const float PassiveScale = 1f;
    private const int IconGap = 4;

    private static readonly Color BarBack = new(16, 14, 24);

    private CombatArt _art;
    private Texture2D _pixel;
    private SpriteFont _font;

    /// <summary>Where the frame is on screen.</summary>
    public static Rectangle Bounds => new(Left, LegendQuestGame.ScreenHeight - BottomMargin - Height, Width, Height);

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
        int x = WordsLeft;
        int bottom = Bounds.Bottom - Inset;
        for (int i = 0; i < icons.Length; i++)
        {
            float scale = i < conditions ? ConditionScale : PassiveScale;
            int size = (int)((i < conditions ? CombatIcons.ConditionSize : CombatIcons.AbilitySize) * scale);
            icons[i] = new Rectangle(x, bottom - size, size, size);
            x += size + IconGap;
        }
        return icons;
    }

    /// <summary>Draws the panel, the portrait, the health bar and the icons. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="fighter">Whose turn it is.</param>
    /// <param name="passives">Their always-on abilities.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawArt(SpriteBatch spriteBatch, FighterView fighter, IReadOnlyList<AbilityDefinition> passives, float alpha)
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

        Rectangle[] icons = Icons(fighter.Conditions.Count, passives.Count);
        for (int i = 0; i < fighter.Conditions.Count; i++)
            _art.Condition(spriteBatch, fighter.Conditions[i], icons[i].Location.ToVector2(), ConditionScale, alpha);
        for (int i = 0; i < passives.Count; i++)
            _art.Ability(spriteBatch, CombatIcons.Ability(passives[i].Id), icons[fighter.Conditions.Count + i].Location.ToVector2(), PassiveScale, Color.White * alpha);
    }

    /// <summary>Writes the name, what they are, their hit points, their armour and their speed. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="fighter">Whose turn it is.</param>
    /// <param name="what">What they are: "LEVEL 1 BLADEMASTER", or a foe's kind.</param>
    /// <param name="speed">How far they move in a turn, in metres.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, FighterView fighter, string what, float speed, float alpha)
    {
        Rectangle bounds = Bounds;
        float y = bounds.Top + Inset - NameLift;
        Text.Draw(spriteBatch, _font, fighter.Name.ToUpperInvariant(), WordsLeft, y, (fighter.Foe ? CombatStyle.FoeLine : Palette.Heading) * alpha, Palette.Shadow * alpha);
        Text.Draw(spriteBatch, _font, what, WordsLeft, y + _font.LineSpacing, Palette.Note * alpha, Palette.Shadow * alpha);

        Rectangle bar = HealthBar;
        Text.Draw(spriteBatch, _font, $"{fighter.Hp} / {fighter.MaxHp} HP", bar.Right + IconGap * 2, bar.Center.Y - _font.LineSpacing / 2f,
            Palette.Bone * alpha, Palette.Shadow * alpha);
        Text.Draw(spriteBatch, _font, $"AC {fighter.ArmourClass}  ·  SPEED {BudgetBar.Metres(speed)} M", WordsLeft, bar.Bottom + BarRoom,
            Palette.Bone * alpha, Palette.Shadow * alpha);
    }

    /// <summary>Where the words start across the screen: right of the portrait.</summary>
    private static int WordsLeft => Left + Inset + (int)(CombatIcons.Piece("portrait-current").Width * PortraitScale) + WordsGap;

    /// <summary>Where the health bar is: under the name and what they are.</summary>
    private Rectangle HealthBar => new(WordsLeft, Bounds.Top + Inset - NameLift + _font.LineSpacing * 2 + BarRoom, BarWidth, BarHeight);
}
