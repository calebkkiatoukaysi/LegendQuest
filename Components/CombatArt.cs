using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The fight's three sheets (the ability icons, the condition icons and hud.png), loaded once and
/// checked against CombatIcons, with the ways the fight's screen draws from them: a named piece, an
/// icon, a face cut off someone's sheet into a portrait's well.
/// </summary>
/// <remarks>
/// Everything the hotbar, the turn order and the frame of whoever's turn it is share, so they draw
/// the same art the same way. All of it is pixel art, blown up by whole steps, so it goes in a point
/// sampled batch.
/// </remarks>
public class CombatArt
{
    /// <summary>A person's face on their sheet: the head and shoulders of the standing frame facing you, the same crop the party's cards use. It's the size of a portrait's well.</summary>
    public static readonly Rectangle Bust = new(4, 3, 24, 26);

    /// <summary>How much darker someone's face is drawn when they're down, and when they've fallen.</summary>
    private static readonly Color DownTint = new(150, 150, 160);
    private static readonly Color FallenTint = new(80, 80, 90);

    /// <summary>The ability icons' sheet.</summary>
    public Texture2D Abilities { get; private set; }

    /// <summary>The condition icons' sheet.</summary>
    public Texture2D Conditions { get; private set; }

    /// <summary>hud.png: the frames, the pips, the bars, the sigil, the rings and what flies.</summary>
    public Texture2D Hud { get; private set; }

    /// <summary>Loads the three sheets and checks each is the size CombatIcons expects.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        Abilities = Load(content, CombatIcons.AbilitySheet);
        Conditions = Load(content, CombatIcons.ConditionSheet);
        Hud = Load(content, CombatIcons.HudSheet);
    }

    /// <summary>Draws a frame of a piece of hud.png, its top left at a point.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled, deferred.</param>
    /// <param name="piece">The piece, by name.</param>
    /// <param name="at">Its top left, in screen pixels.</param>
    /// <param name="scale">How much it's blown up.</param>
    /// <param name="colour">What to multiply it by.</param>
    /// <param name="frame">Which frame, for a piece with more than one.</param>
    public void Piece(SpriteBatch spriteBatch, string piece, Vector2 at, float scale, Color colour, int frame = 0) =>
        spriteBatch.Draw(Hud, at, CombatIcons.Piece(piece, frame), colour, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    /// <summary>Draws an ability's icon.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled, deferred.</param>
    /// <param name="source">The icon on the ability sheet (CombatIcons.Ability).</param>
    /// <param name="at">Its top left, in screen pixels.</param>
    /// <param name="scale">How much it's blown up.</param>
    /// <param name="colour">What to multiply it by: white, or grey for one that can't be used.</param>
    public void Ability(SpriteBatch spriteBatch, Rectangle source, Vector2 at, float scale, Color colour) =>
        spriteBatch.Draw(Abilities, at, source, colour, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    /// <summary>Draws a condition's icon.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled, deferred.</param>
    /// <param name="condition">Which condition.</param>
    /// <param name="at">Its top left, in screen pixels.</param>
    /// <param name="scale">How much it's blown up.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void Condition(SpriteBatch spriteBatch, ConditionId condition, Vector2 at, float scale, float alpha) =>
        spriteBatch.Draw(Conditions, at, CombatIcons.Condition(condition), Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    /// <summary>Draws someone's face into a portrait's well, greyed if they're down and darker still if they've fallen.</summary>
    /// <remarks>A person's is the bust off their standing frame, a pixel for a pixel. A creature's is the top of its figure, as wide as it is, squeezed into the well.</remarks>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled, deferred.</param>
    /// <param name="fighter">Who.</param>
    /// <param name="well">The well, in screen pixels.</param>
    /// <param name="alpha">How faded in it is.</param>
    public static void Face(SpriteBatch spriteBatch, FighterView fighter, Rectangle well, float alpha)
    {
        if (fighter.Sheet is null) return;

        Color tint = (fighter.Fallen ? FallenTint : fighter.Down ? DownTint : Color.White) * alpha;
        spriteBatch.Draw(fighter.Sheet, well, FaceOf(fighter.Shape), tint);
    }

    /// <summary>Where someone's face is on their sheet.</summary>
    /// <param name="shape">The shape of their sheet's frames.</param>
    public static Rectangle FaceOf(WalkerShape shape)
    {
        if (shape.Frame == WalkerShape.Person.Frame) return Bust;

        int width = (int)shape.Figure.X;
        int height = Math.Min((int)shape.Figure.Y, width * Bust.Height / Bust.Width);
        int left = (int)(shape.Feet.X - shape.Figure.X / 2f);
        int top = (int)(shape.Feet.Y - shape.Figure.Y);
        return new Rectangle(left, top, width, height);
    }

    /// <summary>Loads one sheet and checks its size.</summary>
    private static Texture2D Load(ContentManager content, string asset)
    {
        var sheet = content.Load<Texture2D>(asset);
        CombatIcons.CheckSheet(asset, sheet);
        return sheet;
    }
}
