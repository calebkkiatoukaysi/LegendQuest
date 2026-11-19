using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// What every piece of the real-time HUD shares: its fonts, the shared art and the pixel, and the
/// few ways it draws a bar, an outlined number, a face and a time.
/// </summary>
/// <remarks>
/// Loaded once by whoever draws the fight and handed to each component, so the frames, the hotbar
/// and the nameplates all draw from the same sheets and fonts. The words go over the world as well
/// as over the panels, so the small numbers (timers, costs, stacks) are drawn with a dark outline
/// all the way round rather than the offset shadow the rest of the game uses: an outlined number
/// reads on a bright icon and on grass alike.
/// </remarks>
public class HudKit
{
    /// <summary>The fonts: the names and lines (Spectral Light 17), the bigger timers on the slots (Spectral Medium 18), and the small numbers (Spectral Medium 12).</summary>
    public const string WordsFont = "spectral-detail";
    public const string TimerFont = "spectral-timer";
    public const string SmallFont = "spectral-small";
    public const string HeadingFont = "spectral-ui";

    /// <summary>A bar's dark back and its edge.</summary>
    public static readonly Color BarBack = new(12, 10, 20);
    public static readonly Color BarEdge = new(4, 4, 8);

    /// <summary>What the bars are filled with: the party's health, a foe's, Focus, temporary hit points, and the lag that shows what was just lost.</summary>
    public static readonly Color PartyHealth = CombatStyle.PartyHealth;
    public static readonly Color FoeHealth = CombatStyle.FoeHealth;
    public static readonly Color Focus = new(78, 150, 240);
    public static readonly Color TempHp = new(168, 214, 255);
    public static readonly Color Lag = new(255, 214, 150);

    /// <summary>The edge of a good aura and a bad one.</summary>
    public static readonly Color Good = new(110, 206, 104);
    public static readonly Color Bad = new(226, 72, 58);

    /// <summary>How far round an outline goes, in screen pixels.</summary>
    private const float OutlineReach = 1f;

    /// <summary>The fight's sheets, the old ones (hud.png's frames, pips and rings).</summary>
    public CombatArt Art { get; } = new();

    /// <summary>The real-time icons, the sweep and the glint.</summary>
    public HudIcons Icons { get; } = new();

    /// <summary>One white pixel.</summary>
    public Texture2D Pixel { get; private set; }

    /// <summary>The soft round glow, for things that shine.</summary>
    public Texture2D Glow { get; private set; }

    /// <summary>How long the HUD's been running, in seconds, for its pulses and flashes; and how long the last frame was.</summary>
    public float Clock { get; private set; }
    public float Delta { get; private set; }

    /// <summary>The fonts, by what they're for.</summary>
    public SpriteFont Words { get; private set; }
    public SpriteFont Timer { get; private set; }
    public SpriteFont Small { get; private set; }
    public SpriteFont Heading { get; private set; }

    /// <summary>Loads the sheets, the index, the fonts and the glow.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="device">The graphics device.</param>
    /// <param name="pixel">One white pixel: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, GraphicsDevice device, Texture2D pixel)
    {
        Art.LoadContent(content);
        Icons.LoadContent(content, device);
        Pixel = pixel;
        Glow = content.Load<Texture2D>("glow");
        Words = content.Load<SpriteFont>(WordsFont);
        Timer = content.Load<SpriteFont>(TimerFont);
        Small = content.Load<SpriteFont>(SmallFont);
        Heading = content.Load<SpriteFont>(HeadingFont);
    }

    /// <summary>Moves the clock on: once a frame, before anything's drawn.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        Delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Clock += Delta;
    }

    /// <summary>A pulse between a least and 1, `rate` times a second, for things that flash or breathe.</summary>
    /// <param name="rate">How many pulses a second.</param>
    /// <param name="least">How low it goes.</param>
    public float Pulse(float rate, float least) => MathHelper.Lerp(least, 1f, 0.5f + 0.5f * MathF.Cos(Clock * rate * MathHelper.TwoPi));

    /// <summary>A time as a timer shows it: whole seconds from three up ("12"), tenths under three ("2.5"), and minutes from a minute ("2m").</summary>
    /// <param name="seconds">The time, in seconds.</param>
    /// <param name="unit">Whether whole seconds get an "s" after them ("12s"), as the aura rows write them.</param>
    public static string Seconds(float seconds, bool unit = false)
    {
        if (seconds >= 60f) return ((int)MathF.Ceiling(seconds / 60f)).ToString(CultureInfo.InvariantCulture) + "m";
        if (seconds >= 3f) return ((int)MathF.Ceiling(seconds)).ToString(CultureInfo.InvariantCulture) + (unit ? "s" : string.Empty);
        return MathF.Max(0f, MathF.Ceiling(seconds * 10f) / 10f).ToString("0.0", CultureInfo.InvariantCulture);
    }

    /// <summary>Draws a bar: its dark back, the fill to a share, a lit top row and a dark edge.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="area">Where, in screen pixels, its edge included.</param>
    /// <param name="share">How full, 0 to 1.</param>
    /// <param name="fill">The fill's colour.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void Bar(SpriteBatch spriteBatch, Rectangle area, float share, Color fill, float alpha)
    {
        spriteBatch.Draw(Pixel, area, BarEdge * alpha);
        Rectangle inside = Inside(area);
        spriteBatch.Draw(Pixel, inside, BarBack * alpha);
        Fill(spriteBatch, inside, 0f, share, fill, alpha);
    }

    /// <summary>Fills part of a bar's inside, from one share to another, lit along its top and shaded along its bottom.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="inside">The bar's inside, in screen pixels.</param>
    /// <param name="from">Where the fill starts, 0 to 1.</param>
    /// <param name="to">Where it ends, 0 to 1.</param>
    /// <param name="fill">Its colour.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void Fill(SpriteBatch spriteBatch, Rectangle inside, float from, float to, Color fill, float alpha)
    {
        int left = inside.X + (int)MathF.Round(inside.Width * MathHelper.Clamp(from, 0f, 1f));
        int right = inside.X + (int)MathF.Round(inside.Width * MathHelper.Clamp(to, 0f, 1f));
        if (right <= left) return;
        var part = new Rectangle(left, inside.Y, right - left, inside.Height);
        spriteBatch.Draw(Pixel, part, fill * alpha);
        if (inside.Height >= 4)
        {
            spriteBatch.Draw(Pixel, new Rectangle(part.X, part.Y, part.Width, Math.Max(1, inside.Height / 4)), Color.Lerp(fill, Color.White, 0.35f) * alpha);
            spriteBatch.Draw(Pixel, new Rectangle(part.X, part.Bottom - 1, part.Width, 1), Color.Lerp(fill, Color.Black, 0.35f) * alpha);
        }
    }

    /// <summary>A bar's inside: one pixel in from its edge.</summary>
    /// <param name="area">The bar.</param>
    public static Rectangle Inside(Rectangle area) => new(area.X + 1, area.Y + 1, area.Width - 2, area.Height - 2);

    /// <summary>Draws a box's edge, `thickness` thick, inside it.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="area">The box.</param>
    /// <param name="thickness">How thick, in screen pixels.</param>
    /// <param name="colour">Its colour, faded as wanted.</param>
    public void Edge(SpriteBatch spriteBatch, Rectangle area, int thickness, Color colour)
    {
        spriteBatch.Draw(Pixel, new Rectangle(area.Left, area.Top, area.Width, thickness), colour);
        spriteBatch.Draw(Pixel, new Rectangle(area.Left, area.Bottom - thickness, area.Width, thickness), colour);
        spriteBatch.Draw(Pixel, new Rectangle(area.Left, area.Top + thickness, thickness, area.Height - thickness * 2), colour);
        spriteBatch.Draw(Pixel, new Rectangle(area.Right - thickness, area.Top + thickness, thickness, area.Height - thickness * 2), colour);
    }

    /// <summary>Writes a line with a dark outline all the way round it.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="font">The font.</param>
    /// <param name="text">The words.</param>
    /// <param name="at">The point it's placed against, in screen pixels.</param>
    /// <param name="colour">The words' colour, faded as wanted.</param>
    /// <param name="alpha">How faded in the outline is.</param>
    /// <param name="align">Where the point is on the line: 0 its left, 0.5 its middle, 1 its right; its top, unless middle is set.</param>
    /// <param name="middle">Whether the point is the line's middle up and down rather than its top.</param>
    public static void Outlined(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 at, Color colour, float alpha, float align = 0f, bool middle = false)
    {
        if (string.IsNullOrEmpty(text)) return;
        Vector2 size = font.MeasureString(text);
        var position = new Vector2(MathF.Round(at.X - size.X * align), MathF.Round(at.Y - (middle ? font.LineSpacing / 2f : 0f)));
        Color outline = Color.Black * (0.85f * alpha);
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                if (dx != 0 || dy != 0)
                    spriteBatch.DrawString(font, text, position + new Vector2(dx, dy) * OutlineReach, outline);
        spriteBatch.DrawString(font, text, position, colour);
    }

    /// <summary>Draws someone's face into a well: the bust off their sheet, or their imported picture's portrait; darker if they're down or out.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="portrait">Where their face comes from.</param>
    /// <param name="well">The well, in screen pixels.</param>
    /// <param name="tint">What to multiply it by.</param>
    public static void Face(SpriteBatch spriteBatch, PortraitView portrait, Rectangle well, Color tint)
    {
        if (portrait is null) return;
        if (portrait.Art is not null) spriteBatch.Draw(portrait.Art.Portrait(well.Size), well, tint);
        else if (portrait.Sheet is not null) spriteBatch.Draw(portrait.Sheet, well, CombatArt.FaceOf(portrait.Shape), tint);
    }

    /// <summary>How someone's face is tinted: white, grey while they're down, dark once they're out.</summary>
    /// <param name="down">Whether they're down.</param>
    /// <param name="out">Whether they're out of the fight.</param>
    public static Color FaceTint(bool down, bool @out) => @out ? new Color(70, 70, 80) : down ? new Color(140, 130, 140) : Color.White;
}
