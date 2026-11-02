using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The numbers and words that pop up over someone in a fight: damage, healing, MISS, CRITICAL, a
/// condition going on, a death save.
/// </summary>
/// <remarks>
/// Each one is pinned to a point on the map, so it stays over its owner however the camera moves,
/// and drawn on the screen at the screen's own size so it's always crisp. It pops (in big and
/// settling), rises as it eases, and fades out at the end. Numbers are in the big Spectral digits
/// of spectral-number; words in the UI font. Several over the same person close together stack up
/// rather than sitting on each other, so a Multiattack's two hits both read.
/// </remarks>
public class DamagePopups
{
    /// <summary>How long a number and a word stay up, in seconds.</summary>
    private const float NumberSeconds = 1.15f;
    private const float WordSeconds = 1.05f;

    /// <summary>How far one rises over its life, in screen pixels.</summary>
    private const float Rise = 48f;

    /// <summary>How long the pop takes to settle, and how much bigger it starts.</summary>
    private const float PopSeconds = 0.14f;
    private const float PopOvershoot = 0.45f;

    /// <summary>How far through its life one starts to fade.</summary>
    private const float FadeFrom = 0.65f;

    /// <summary>How big a number, a critical's number and a word are drawn, against their fonts' own size.</summary>
    private const float NumberScale = 0.62f;
    private const float CriticalScale = 0.9f;
    private const float WordScale = 0.78f;

    /// <summary>How far up a new one goes past one over the same person that's only just come up, in screen pixels, and how close together counts.</summary>
    private const float StackGap = 30f;
    private const float StackSeconds = 0.45f;
    private const float SameSpot = 4f;

    /// <summary>How far down and right the shadow sits, at a number's size.</summary>
    private static readonly Vector2 ShadowOffset = new(3f, 3f);

    private static readonly Color ShadowColour = Color.Black * 0.75f;

    private readonly List<Popup> _popups = new();

    private SpriteFont _numbers;
    private SpriteFont _words;

    /// <summary>One popup: where it's pinned, what it says, its colour, whether it's in the number font, its size, how long it's been up and how long it lasts, and how far up it's been stacked.</summary>
    private sealed class Popup
    {
        /// <summary>The point on the map it's pinned over, in world pixels.</summary>
        public Vector2 Anchor;

        /// <summary>What it says.</summary>
        public string Text;

        /// <summary>Its colour.</summary>
        public Color Colour;

        /// <summary>Whether it's in the number font rather than the words'.</summary>
        public bool Number;

        /// <summary>How big it's drawn, against its font's own size.</summary>
        public float Scale;

        /// <summary>How long it's been up.</summary>
        public float Clock;

        /// <summary>How long it lasts.</summary>
        public float Life;

        /// <summary>How far up it's stacked over the others at the same spot, in screen pixels.</summary>
        public float Lift;
    }

    /// <summary>Loads the two fonts.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _numbers = content.Load<SpriteFont>("spectral-number");
        _words = content.Load<SpriteFont>("spectral-ui");
    }

    /// <summary>Pops up a number: damage with a minus, healing with a plus.</summary>
    /// <param name="anchor">The point it's over, in world pixels: the top of someone's head.</param>
    /// <param name="amount">How much.</param>
    /// <param name="heal">Whether it's healing.</param>
    /// <param name="critical">Whether it's a critical's, drawn bigger.</param>
    /// <param name="colour">Its colour.</param>
    public void Number(Vector2 anchor, int amount, bool heal, bool critical, Color colour) =>
        Add(anchor, (heal ? "+" : "-") + amount, colour, true, critical ? CriticalScale : NumberScale, NumberSeconds);

    /// <summary>Pops up a word or two: MISS, CRITICAL, SLOWED.</summary>
    /// <param name="anchor">The point it's over, in world pixels.</param>
    /// <param name="text">What it says, in capitals.</param>
    /// <param name="colour">Its colour.</param>
    public void Word(Vector2 anchor, string text, Color colour) => Add(anchor, text, colour, false, WordScale, WordSeconds);

    /// <summary>Runs every popup's clock and lets the finished ones go.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            _popups[i].Clock += seconds;
            if (_popups[i].Clock >= _popups[i].Life) _popups.RemoveAt(i);
        }
    }

    /// <summary>Takes every popup down, when the fight's over.</summary>
    public void Clear() => _popups.Clear();

    /// <summary>Draws every popup over its point, rising and fading.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="toScreen">Where a point on the map is on the screen: the camera's ToScreen.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void Draw(SpriteBatch spriteBatch, Func<Vector2, Vector2> toScreen, float alpha)
    {
        foreach (Popup popup in _popups)
        {
            float t = popup.Clock / popup.Life;
            float eased = 1f - (1f - t) * (1f - t);
            float pop = popup.Clock < PopSeconds ? 1f + PopOvershoot * (1f - popup.Clock / PopSeconds) : 1f;
            float shown = (t < FadeFrom ? 1f : 1f - (t - FadeFrom) / (1f - FadeFrom)) * alpha;

            SpriteFont font = popup.Number ? _numbers : _words;
            float scale = popup.Scale * pop;
            Vector2 size = font.MeasureString(popup.Text) * scale;
            Vector2 at = toScreen(popup.Anchor) - new Vector2(size.X / 2f, size.Y + popup.Lift + Rise * eased);
            at = new Vector2(MathF.Round(at.X), MathF.Round(at.Y));

            spriteBatch.DrawString(font, popup.Text, at + ShadowOffset * popup.Scale, ShadowColour * shown, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, popup.Text, at, popup.Colour * shown, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Adds one, stacked over any at the same spot that's only just come up.</summary>
    private void Add(Vector2 anchor, string text, Color colour, bool number, float scale, float life)
    {
        float lift = 0f;
        foreach (Popup other in _popups)
            if (other.Clock < StackSeconds && Vector2.Distance(other.Anchor, anchor) <= SameSpot) lift = MathF.Max(lift, other.Lift + StackGap);

        _popups.Add(new Popup { Anchor = anchor, Text = text, Colour = colour, Number = number, Scale = scale, Life = life, Lift = lift });
    }
}
