using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The fight's log: every roll written out in full, a line as each thing happens, on a panel up the
/// left side over the frame of whoever's turn it is.
/// </summary>
/// <remarks>
/// Spec 6.7's log: "SERA attacks GLOOMFANG: 14 + 5 = 19 vs AC 13 - HIT. 1d8+3 = 9 slashing." A
/// line goes in when the screen shows what it's about, not when the engine worked it out, so the
/// log never gets ahead of the fight. Long lines wrap; each new one fades in. L shows and hides it,
/// and the mouse wheel over it scrolls back through the fight. Hidden, it's a small tab that says
/// how to bring it back.
/// </remarks>
public class CombatLog
{
    /// <summary>How many rows it keeps, all told, before the oldest go.</summary>
    private const int MostRows = 120;

    /// <summary>How many rows show at once.</summary>
    private const int ShownRows = 9;

    /// <summary>Where the panel is and how wide, in screen pixels: up the left side, its bottom just over the frame of whoever's turn it is.</summary>
    private const int Left = 24;
    private const int Width = 660;
    private const int Bottom = 708;

    /// <summary>How far in from the panel's edges the words go, and the room the heading takes, in screen pixels.</summary>
    private const int Inset = 12;
    private const int HeadingRoom = 6;

    /// <summary>How long a new row takes to fade in.</summary>
    private const float FadeInSeconds = 0.25f;

    /// <summary>What the heading says shown, and what the tab says hidden.</summary>
    private const string Heading = "COMBAT LOG";
    private const string HideHint = "L  ·  HIDE      WHEEL  ·  SCROLL";
    private const string ShowHint = "L  ·  COMBAT LOG";

    /// <summary>The rows, oldest first: the words, their colour, and how long they've been up.</summary>
    private readonly List<(string Text, Color Colour, float Age)> _rows = new();

    private SpriteFont _font;
    private Texture2D _pixel;

    /// <summary>How many rows back from the newest the view is scrolled.</summary>
    private int _scroll;

    /// <summary>Whether the panel's showing, or only the tab.</summary>
    public bool Shown { get; set; }

    /// <summary>Loads the font.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="pixel">One white pixel for the panel: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, Texture2D pixel)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _pixel = pixel;
    }

    /// <summary>Where the panel (or, hidden, the tab) is on screen, for telling when the mouse is over it.</summary>
    public Rectangle Bounds
    {
        get
        {
            int height = Shown ? Inset * 2 + HeadingRoom + _font.LineSpacing * (ShownRows + 1) : Inset * 2 + _font.LineSpacing;
            int width = Shown ? Width : (int)_font.MeasureString(ShowHint).X + Inset * 2;
            return new Rectangle(Left, Bottom - height, width, height);
        }
    }

    /// <summary>Writes a line in, wrapped to the panel, and scrolls back to the newest.</summary>
    /// <param name="line">The line.</param>
    /// <param name="colour">Its colour.</param>
    public void Add(string line, Color colour)
    {
        if (string.IsNullOrEmpty(line)) return;
        foreach (string row in Text.Wrap(line, _font, Width - Inset * 2)) _rows.Add((row, colour, 0f));
        if (_rows.Count > MostRows) _rows.RemoveRange(0, _rows.Count - MostRows);
        _scroll = 0;
    }

    /// <summary>Starts a fight's log afresh.</summary>
    public void Clear()
    {
        _rows.Clear();
        _scroll = 0;
    }

    /// <summary>Scrolls back or forward through the rows.</summary>
    /// <param name="steps">How many rows: positive goes back to older ones.</param>
    public void Scroll(int steps) => _scroll = Math.Clamp(_scroll + steps, 0, Math.Max(0, _rows.Count - ShownRows));

    /// <summary>Runs each new row's fade.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        for (int i = Math.Max(0, _rows.Count - ShownRows); i < _rows.Count; i++)
        {
            (string text, Color colour, float age) = _rows[i];
            if (age < FadeInSeconds) _rows[i] = (text, colour, age + seconds);
        }
    }

    /// <summary>Draws the panel and its rows, or the tab if it's hidden.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void Draw(SpriteBatch spriteBatch, float alpha)
    {
        Rectangle bounds = Bounds;
        CombatStyle.DrawPanel(spriteBatch, _pixel, bounds, alpha);

        if (!Shown)
        {
            Text.Draw(spriteBatch, _font, ShowHint, bounds.Left + Inset, bounds.Top + Inset, Palette.Note * alpha, Palette.Shadow * alpha);
            return;
        }

        Text.Draw(spriteBatch, _font, Heading, bounds.Left + Inset, bounds.Top + Inset, Palette.Gold * alpha, Palette.Shadow * alpha);
        Text.Draw(spriteBatch, _font, HideHint, bounds.Right - Inset, bounds.Top + Inset, Palette.Note * alpha, Palette.Shadow * alpha, rightAligned: true);

        int last = _rows.Count - _scroll;
        int first = Math.Max(0, last - ShownRows);
        float y = bounds.Top + Inset + HeadingRoom + _font.LineSpacing;
        for (int i = first; i < last; i++)
        {
            (string text, Color colour, float age) = _rows[i];
            float shown = MathHelper.Clamp(age / FadeInSeconds, 0f, 1f) * alpha;
            Text.Draw(spriteBatch, _font, text, bounds.Left + Inset, y, colour * shown, Palette.Shadow * shown);
            y += _font.LineSpacing;
        }
    }
}
