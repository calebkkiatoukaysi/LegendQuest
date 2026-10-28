using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Story;

namespace LegendQuest.Components;

/// <summary>
/// The Lantern Market's quest board, filling the top of the screen: a cork board in a wooden frame,
/// its three columns of contracts pinned up, and the main quest's glowing gold.
/// </summary>
/// <remarks>Drawn behind a conversation's line, the way a painting is, so Fennick can talk you through it.</remarks>
public class QuestBoardSprite
{
    /// <summary>Where the board goes, and how thick its frame is.</summary>
    private static readonly Rectangle Bounds = new(230, 40, 1140, 500);
    private const int Frame = 18;

    /// <summary>The headers along the top, in PostingColumn's order.</summary>
    private static readonly string[] Headers = { "MONSTERS", "BANDITS", "LOST AND FOUND" };

    /// <summary>A note: its size, the gap between notes, and how far down the board they start.</summary>
    private static readonly Point NoteSize = new(340, 150);
    private const int NoteGap = 14;
    private const int NotesTop = 56;
    private const int NoteInset = 14;

    /// <summary>The colours: the frame, the cork, the paper, the pin, and the main quest's glow.</summary>
    private static readonly Color Wood = new(78, 48, 36);
    private static readonly Color WoodLight = new(132, 88, 58);
    private static readonly Color Cork = new(166, 122, 82);
    private static readonly Color Paper = new(236, 226, 198);
    private static readonly Color PaperShadow = new Color(40, 24, 20) * 0.45f;
    private static readonly Color Pin = new(196, 52, 60);
    private static readonly Color Glow = new(255, 214, 110);

    /// <summary>How fast the main quest's glow pulses, in pulses a second.</summary>
    private const float PulseRate = 0.8f;

    private Texture2D _pixel;
    private SpriteFont _detailFont;
    private float _clock;

    /// <summary>Loads the fonts, and makes the pixel the board's drawn with.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="device">The device, for the pixel.</param>
    public void LoadContent(ContentManager content, GraphicsDevice device)
    {
        _detailFont = content.Load<SpriteFont>("spectral-detail");
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    /// <summary>Disposes the pixel.</summary>
    public void Unload() => _pixel?.Dispose();

    /// <summary>Runs the glow's pulse.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime) => _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;

    /// <summary>Draws the frame, the cork, the headers and every note.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled for the writing.</param>
    /// <param name="alpha">How far it's faded in.</param>
    public void Draw(SpriteBatch spriteBatch, float alpha)
    {
        Fill(spriteBatch, Bounds, Wood * alpha);
        Fill(spriteBatch, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, 4), WoodLight * alpha);
        var cork = new Rectangle(Bounds.X + Frame, Bounds.Y + Frame, Bounds.Width - Frame * 2, Bounds.Height - Frame * 2);
        Fill(spriteBatch, cork, Cork * alpha);

        int column = cork.Width / Headers.Length;
        for (int c = 0; c < Headers.Length; c++)
            Text.Draw(spriteBatch, _detailFont, Headers[c], cork.X + column * c + column / 2f, cork.Y + 18f, Palette.Ink * alpha, Color.Transparent, centred: true);

        var placed = new int[Headers.Length];
        float pulse = 0.5f + 0.5f * MathF.Sin(_clock * PulseRate * MathHelper.TwoPi);
        foreach (Posting posting in Postings.All)
        {
            int c = (int)posting.Column;
            var note = new Rectangle(cork.X + column * c + (column - NoteSize.X) / 2, cork.Y + NotesTop + placed[c] * (NoteSize.Y + NoteGap), NoteSize.X, NoteSize.Y);
            placed[c]++;

            if (posting.Main) Fill(spriteBatch, Grow(note, 6), Glow * (alpha * (0.35f + 0.45f * pulse)));
            Fill(spriteBatch, new Rectangle(note.X + 4, note.Y + 5, note.Width, note.Height), PaperShadow * alpha);
            Fill(spriteBatch, note, Paper * alpha);
            Fill(spriteBatch, new Rectangle(note.Center.X - 4, note.Y + 4, 8, 8), Pin * alpha);

            float y = note.Y + NoteInset + 6f;
            Text.Draw(spriteBatch, _detailFont, posting.Title, note.X + NoteInset, y, Palette.Ink * alpha, Color.Transparent);
            y += _detailFont.LineSpacing + 2f;
            Text.Draw(spriteBatch, _detailFont, posting.Giver, note.X + NoteInset, y, Palette.Note * alpha, Color.Transparent);
            y += _detailFont.LineSpacing + 4f;
            foreach (string row in Wrap(posting.Text, NoteSize.X - NoteInset * 2))
            {
                Text.Draw(spriteBatch, _detailFont, row, note.X + NoteInset, y, Palette.Ink * alpha, Color.Transparent);
                y += _detailFont.LineSpacing;
            }
        }
    }

    /// <summary>A plain filled rectangle.</summary>
    private void Fill(SpriteBatch spriteBatch, Rectangle area, Color colour) => spriteBatch.Draw(_pixel, area, colour);

    /// <summary>A rectangle grown by some pixels all round.</summary>
    private static Rectangle Grow(Rectangle area, int by) => new(area.X - by, area.Y - by, area.Width + by * 2, area.Height + by * 2);

    /// <summary>Breaks a line into rows that fit a width, at the spaces.</summary>
    private System.Collections.Generic.List<string> Wrap(string text, float width)
    {
        var rows = new System.Collections.Generic.List<string>();
        string row = string.Empty;
        foreach (string word in text.Split(' '))
        {
            string tried = row.Length == 0 ? word : row + " " + word;
            if (row.Length > 0 && _detailFont.MeasureString(tried).X > width)
            {
                rows.Add(row);
                row = word;
            }
            else
            {
                row = tried;
            }
        }
        if (row.Length > 0) rows.Add(row);
        return rows;
    }
}
