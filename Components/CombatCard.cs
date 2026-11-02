using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The card that comes up in a fight beside whatever the mouse is on: an ability on the hotbar (what
/// it costs, how far it reaches, what it does, what's left of it, and why it can't be used if it
/// can't), someone on the map (their health, their armour, what's on them, and what the ability
/// you've picked would do to them), or a piece of the screen.
/// </summary>
/// <remarks>
/// Built a row at a time each frame by whoever's showing it, then drawn: a navy panel with the gold
/// rim, as wide as its widest row up to a limit, rows wrapping past that. It goes above the thing
/// it's about when that's on the hotbar, and below and to the right of the mouse on the map, flipped
/// to stay on the screen. Every number on it says what it is, so nothing has to be guessed.
/// </remarks>
public class CombatCard
{
    /// <summary>The widest the card gets, and how far in from its edges the words go, in screen pixels.</summary>
    private const int MostWidth = 470;
    private const int Inset = 14;

    /// <summary>How tall a gap between two parts of the card is, as a share of a line.</summary>
    private const float GapShare = 0.4f;

    /// <summary>How far the card sits from the thing it's about, and from the mouse, and how far it keeps from the screen's edges, in screen pixels.</summary>
    private const int Beside = 10;
    private static readonly Point FromMouse = new(26, 28);
    private const int Margin = 12;

    /// <summary>The card's rows, top to bottom: some words and their colour, or null words for a gap.</summary>
    private readonly List<(string Text, Color Colour)> _rows = new();

    private SpriteFont _font;
    private Texture2D _pixel;
    private float _width;
    private float _height;

    /// <summary>Whether there's anything on the card.</summary>
    public bool Showing => _rows.Count > 0;

    /// <summary>Loads the font.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="pixel">One white pixel for the panel: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, Texture2D pixel)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _pixel = pixel;
    }

    /// <summary>Takes everything off the card.</summary>
    public void Clear()
    {
        _rows.Clear();
        _width = 0f;
        _height = 0f;
    }

    /// <summary>Adds words to the card, wrapped to fit.</summary>
    /// <param name="text">The words.</param>
    /// <param name="colour">Their colour.</param>
    public void Line(string text, Color colour)
    {
        if (string.IsNullOrEmpty(text)) return;
        foreach (string row in Text.Wrap(text, _font, MostWidth - Inset * 2))
        {
            _rows.Add((row, colour));
            _width = MathF.Max(_width, _font.MeasureString(row).X);
            _height += _font.LineSpacing;
        }
    }

    /// <summary>Adds a gap between two parts of the card, unless it's at the top or there's one already.</summary>
    public void Gap()
    {
        if (_rows.Count == 0 || _rows[^1].Text is null) return;
        _rows.Add((null, Color.Transparent));
        _height += _font.LineSpacing * GapShare;
    }

    /// <summary>Draws the card above a thing on the screen, centred on it and kept on the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="anchor">The thing it's about, in screen pixels.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawAbove(SpriteBatch spriteBatch, Rectangle anchor, float alpha)
    {
        Point size = Size();
        Draw(spriteBatch, new Point(anchor.Center.X - size.X / 2, anchor.Top - Beside - size.Y), alpha);
    }

    /// <summary>Draws the card below a thing on the screen, centred on it and kept on the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="anchor">The thing it's about, in screen pixels.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawBelow(SpriteBatch spriteBatch, Rectangle anchor, float alpha)
    {
        Point size = Size();
        Draw(spriteBatch, new Point(anchor.Center.X - size.X / 2, anchor.Bottom + Beside), alpha);
    }

    /// <summary>Draws the card by the mouse: below and to the right, or flipped to stay on the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawByMouse(SpriteBatch spriteBatch, Point mouse, float alpha)
    {
        Point size = Size();
        int x = mouse.X + FromMouse.X, y = mouse.Y + FromMouse.Y;
        if (x + size.X > LegendQuestGame.ScreenWidth - Margin) x = mouse.X - FromMouse.X - size.X;
        if (y + size.Y > LegendQuestGame.ScreenHeight - Margin) y = mouse.Y - FromMouse.Y - size.Y;
        Draw(spriteBatch, new Point(x, y), alpha);
    }

    /// <summary>How big the card is, its insets included.</summary>
    private Point Size() => new((int)MathF.Ceiling(_width) + Inset * 2, (int)MathF.Ceiling(_height) + Inset * 2);

    /// <summary>Draws the panel and the rows with its top left at a point, moved onto the screen if it'd go off.</summary>
    private void Draw(SpriteBatch spriteBatch, Point at, float alpha)
    {
        if (!Showing) return;

        Point size = Size();
        int x = Math.Clamp(at.X, Margin, LegendQuestGame.ScreenWidth - Margin - size.X);
        int y = Math.Clamp(at.Y, Margin, LegendQuestGame.ScreenHeight - Margin - size.Y);
        CombatStyle.DrawPanel(spriteBatch, _pixel, new Rectangle(x, y, size.X, size.Y), alpha);

        float row = y + Inset;
        foreach ((string text, Color colour) in _rows)
        {
            if (text is null)
            {
                row += _font.LineSpacing * GapShare;
                continue;
            }
            Text.Draw(spriteBatch, _font, text, x + Inset, row, colour * alpha, Palette.Shadow * alpha);
            row += _font.LineSpacing;
        }
    }
}
