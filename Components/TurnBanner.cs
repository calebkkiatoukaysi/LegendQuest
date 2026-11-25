using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The banner across the middle of the screen when a fight starts and when a turn changes hands:
/// YOUR TURN, or whose it is, with a line under it.
/// </summary>
/// <remarks>
/// A dark band on the soft smudge with a thin gold rule above and below, in gold for one of yours and
/// ember for a foe's. It slides in, holds long enough to read, and fades, without stopping anything:
/// the fight carries on under it. A new one cuts short the one that's up.
/// </remarks>
public class TurnBanner
{
    /// <summary>How long it takes to come in, how long it holds, and how long it takes to go, in seconds.</summary>
    private const float InSeconds = 0.18f;
    private const float HoldSeconds = 0.75f;
    private const float OutSeconds = 0.3f;

    /// <summary>How far it slides in from, in screen pixels.</summary>
    private const float Slide = 36f;

    /// <summary>Where the middle of the banner is, down the screen, in screen pixels: over the fight, under the turn order.</summary>
    private const float MiddleY = 360f;

    /// <summary>How big the title is drawn, against the UI font's own size.</summary>
    private const float TitleScale = 1.35f;

    /// <summary>How far the smudge reaches past the words, and how far the rules are from them, in screen pixels.</summary>
    private static readonly Vector2 Spread = new(120f, 18f);
    private const float RuleGap = 10f;
    private const int RuleThickness = 2;

    /// <summary>How long a rule is, against the words.</summary>
    private const float RuleShare = 1.2f;

    private static readonly Color BackingColour = new Color(48, 42, 108) * 0.88f;

    /// <summary>How bright the gold rules are, against the heading.</summary>
    private const float RuleStrength = 0.8f;

    private SpriteFont _title;
    private SpriteFont _detail;
    private Texture2D _glow;
    private Texture2D _pixel;

    private string _heading;
    private string _line;
    private Color _colour;
    private float _clock = float.MaxValue;

    /// <summary>Loads the fonts and the glow the smudge is made from.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="pixel">One white pixel for the rules: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, Texture2D pixel)
    {
        _title = content.Load<SpriteFont>("spectral-ui");
        _detail = content.Load<SpriteFont>("spectral-detail");
        _glow = content.Load<Texture2D>("glow");
        _pixel = pixel;
    }

    /// <summary>Puts a banner up, in place of whatever's up.</summary>
    /// <param name="heading">The big words, in capitals.</param>
    /// <param name="line">The line under them, or null.</param>
    /// <param name="colour">The heading's and the rules' colour.</param>
    public void Show(string heading, string line, Color colour)
    {
        _heading = heading;
        _line = line;
        _colour = colour;
        _clock = 0f;
    }

    /// <summary>How much longer the banner that's up stays, in seconds, or 0 if none is.</summary>
    public float Left => _heading is null || _clock == float.MaxValue ? 0f : MathF.Max(0f, InSeconds + HoldSeconds + OutSeconds - _clock);

    /// <summary>
    /// What the banner that's up covers on the screen, its smudge and rules and the whole of its
    /// slide in, or null if none is; for the numbers and words over the fight to keep off.
    /// </summary>
    public Rectangle? Area
    {
        get
        {
            if (Left <= 0f) return null;
            (float width, float height) = Measure();
            int reach = (int)MathF.Max(width * RuleShare / 2f, width / 2f + Spread.X / 2f);
            float centre = LegendQuestGame.ScreenWidth / 2f;
            int top = (int)(MiddleY - height / 2f - Spread.Y), bottom = (int)MathF.Ceiling(MiddleY + height / 2f + Spread.Y);
            return new Rectangle((int)(centre - Slide) - reach, top, reach * 2 + (int)Slide, bottom - top);
        }
    }

    /// <summary>How wide and tall the heading and the line under it are together, in screen pixels.</summary>
    private (float Width, float Height) Measure()
    {
        Vector2 headingSize = _title.MeasureString(_heading) * TitleScale;
        Vector2 lineSize = _line is null ? Vector2.Zero : _detail.MeasureString(_line);
        return (MathF.Max(headingSize.X, lineSize.X), headingSize.Y + lineSize.Y);
    }

    /// <summary>How much of the banner is showing now, 0 to 1, as it comes in and goes; 0 if none is up.</summary>
    public float Showing
    {
        get
        {
            float length = InSeconds + HoldSeconds + OutSeconds;
            if (_heading is null || _clock >= length) return 0f;
            return MathF.Min(MathHelper.Clamp(_clock / InSeconds, 0f, 1f), MathHelper.Clamp((length - _clock) / OutSeconds, 0f, 1f));
        }
    }

    /// <summary>Takes the banner down at once.</summary>
    public void Clear() => _clock = float.MaxValue;

    /// <summary>Runs the banner's clock.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        if (_clock < float.MaxValue) _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>Draws the banner if one's up.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void Draw(SpriteBatch spriteBatch, float alpha)
    {
        float length = InSeconds + HoldSeconds + OutSeconds;
        if (_heading is null || _clock >= length) return;

        float coming = MathHelper.Clamp(_clock / InSeconds, 0f, 1f);
        float going = MathHelper.Clamp((length - _clock) / OutSeconds, 0f, 1f);
        float shown = MathF.Min(coming, going) * alpha;
        float x = LegendQuestGame.ScreenWidth / 2f - Slide * (1f - coming) * (1f - coming);

        Vector2 headingSize = _title.MeasureString(_heading) * TitleScale;
        Vector2 lineSize = _line is null ? Vector2.Zero : _detail.MeasureString(_line);
        float width = MathF.Max(headingSize.X, lineSize.X);
        float height = headingSize.Y + lineSize.Y;
        float top = MiddleY - height / 2f;

        var area = new Rectangle((int)(x - width / 2f), (int)(top - Spread.Y), (int)width, (int)(height + Spread.Y * 2f));
        Text.DrawBacking(spriteBatch, _glow, area, (int)Spread.X, BackingColour * shown);

        int rule = (int)(width * RuleShare);
        Color ruleColour = _colour * (RuleStrength * shown);
        spriteBatch.Draw(_pixel, new Rectangle((int)(x - rule / 2f), (int)(top - RuleGap), rule, RuleThickness), ruleColour);
        spriteBatch.Draw(_pixel, new Rectangle((int)(x - rule / 2f), (int)(top + height + RuleGap), rule, RuleThickness), ruleColour);

        var headingAt = new Vector2(MathF.Round(x - headingSize.X / 2f), MathF.Round(top));
        spriteBatch.DrawString(_title, _heading, headingAt + Palette.ShadowOffset, Palette.Shadow * shown, 0f, Vector2.Zero, TitleScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_title, _heading, headingAt, _colour * shown, 0f, Vector2.Zero, TitleScale, SpriteEffects.None, 0f);

        if (_line is not null)
            Text.Draw(spriteBatch, _detail, _line, x, top + headingSize.Y, Palette.Bone * shown, Palette.Shadow * shown, centred: true);
    }
}
