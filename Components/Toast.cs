using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// A short line that comes up near the top of the screen for a few seconds: what was in a chest,
/// what a sign says, that a door's locked.
/// </summary>
/// <remarks>
/// One at a time, in the order they came in, and a new one cuts short the one that's up, so a run of
/// them doesn't make the last one wait. It sits on the soft dark smudge from Text.DrawBacking, so
/// it reads over bright grass as well as the dark.
/// </remarks>
public class Toast
{
    /// <summary>How long a line takes to fade in, and to fade out.</summary>
    private const float FadeInSeconds = 0.2f;
    private const float FadeOutSeconds = 0.5f;

    /// <summary>How long a line stays up, plus a bit more for every letter so a long one can be read.</summary>
    private const float HoldSeconds = 1.6f;
    private const float SecondsPerLetter = 0.04f;

    /// <summary>Where the top of the line is, in screen pixels. Under the map's name.</summary>
    private const float Top = 84f;

    /// <summary>How far the smudge reaches past the line, across and up and down, in screen pixels.</summary>
    private static readonly Vector2 Spread = new(70f, 26f);

    private static readonly Color BackingColor = Color.Black * 0.8f;

    private readonly Queue<string> _waiting = new();

    private SpriteFont _font;
    private Texture2D _glow;
    private string _line;
    private float _clock;

    /// <summary>Loads the font and the glow the smudge is made from.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _font = content.Load<SpriteFont>("spectral-ui");
        _glow = content.Load<Texture2D>("glow");
    }

    /// <summary>Puts a line up, after whatever's up already.</summary>
    /// <param name="line">The line. It goes up in capitals like everything else on screen, so map files can write it normally.</param>
    public void Show(string line)
    {
        _waiting.Enqueue(line.ToUpperInvariant());

        // Start the one that's up fading now, if it isn't already.
        if (_line is not null) _clock = MathF.Max(_clock, Length(_line) - FadeOutSeconds);
    }

    /// <summary>Runs the clock on the line that's up, and brings up the next when it's done.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        if (_line is null)
        {
            if (_waiting.Count == 0) return;
            _line = _waiting.Dequeue();
            _clock = 0f;
        }

        _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_clock >= Length(_line)) _line = null;
    }

    /// <summary>Draws the line that's up, if there is one, centred across the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, back to front, linear-sampled.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void Draw(SpriteBatch spriteBatch, float alpha)
    {
        if (_line is null) return;

        float shown = MathF.Min(1f, MathF.Min(_clock / FadeInSeconds, (Length(_line) - _clock) / FadeOutSeconds)) * alpha;
        Vector2 size = _font.MeasureString(_line);

        var area = new Rectangle((int)(LegendQuestGame.ScreenWidth / 2f - size.X / 2f), (int)(Top - Spread.Y),
            (int)size.X, (int)(size.Y + Spread.Y * 2f));
        Text.DrawBacking(spriteBatch, _glow, area, (int)Spread.X, BackingColor * shown);

        Text.DrawCentered(spriteBatch, _font, _line, Top, Palette.Heading * shown, Palette.Shadow * shown);
    }

    /// <summary>How long a line stays up altogether, fades included.</summary>
    private static float Length(string line) => FadeInSeconds + HoldSeconds + line.Length * SecondsPerLetter + FadeOutSeconds;
}
