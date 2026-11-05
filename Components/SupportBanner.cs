using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// A support's banner in a fight: a ribbon with an icon and a line on it, over the support's figure
/// behind the rows ("1D6 TO A WRAITH") and at the end of the turn order ("ODELLE'S CREW  ·  END OF
/// ROUND"), and for a support holding out for something, a ring beside it filling as the rounds go.
/// </summary>
/// <remarks>
/// hud.png's support-banner piece blown up twice and three-sliced across: four art pixels at each end,
/// the swallowtail on the right, and the four between stretched to fit the line. It's lit while the
/// support will act at the end of this round, and dim before its first round, after its last, and
/// once a script has stood it down. The ring is hud.png's progress-ring, a frame for every eighth.
/// </remarks>
public class SupportBanner
{
    /// <summary>How much the hud art is blown up.</summary>
    private const float Scale = 2f;

    /// <summary>The banner piece's ends and its middle, in art pixels.</summary>
    private const int Cap = 4;
    private const int Middle = 4;

    /// <summary>How far in from the banner's left the icon sits, the room after it, and the room after the line for the swallowtail, in screen pixels.</summary>
    private const int IconInset = 8;
    private const int IconGap = 6;
    private const int TailRoom = 18;

    /// <summary>How bright a dim banner is.</summary>
    private const float Dim = 0.45f;

    /// <summary>The room between a banner and its ring, in screen pixels.</summary>
    private const int RingGap = 4;

    private CombatArt _art;
    private SpriteFont _font;

    /// <summary>How tall a banner is on screen.</summary>
    public static int Height => (int)(CombatIcons.Piece("support-banner").Height * Scale);

    /// <summary>How big the ring beside it is on screen.</summary>
    public static int RingSize => (int)(CombatIcons.Piece("progress-ring").Width * Scale);

    /// <summary>Loads the font and keeps the shared art.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="art">The fight's sheets.</param>
    public void LoadContent(ContentManager content, CombatArt art)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _art = art;
    }

    /// <summary>How wide a banner with a line on it is, on screen.</summary>
    /// <param name="text">The line.</param>
    public int Width(string text) => IconInset + CombatIcons.AbilitySize + IconGap + (int)MathF.Ceiling(_font.MeasureString(text ?? string.Empty).X) + TailRoom;

    /// <summary>Where the ring goes beside a banner: just right of it, its middle level with the banner's.</summary>
    /// <param name="banner">The banner, on screen.</param>
    public static Rectangle Ring(Rectangle banner) => new(banner.Right + RingGap, banner.Center.Y - RingSize / 2, RingSize, RingSize);

    /// <summary>Draws the ribbon, its icon and its ring. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="area">Where the banner is, on screen: Height tall and Width wide.</param>
    /// <param name="icon">The icon on the ability sheet, or null for none.</param>
    /// <param name="lit">Whether the support acts at the end of this round.</param>
    /// <param name="progress">How full its ring is, 0 to 1, or null for no ring.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawArt(SpriteBatch spriteBatch, Rectangle area, Rectangle? icon, bool lit, float? progress, float alpha)
    {
        Color colour = Color.White * ((lit ? 1f : Dim) * alpha);
        Rectangle source = CombatIcons.Piece("support-banner");
        int cap = Math.Min((int)(Cap * Scale), area.Width / 2);
        spriteBatch.Draw(_art.Hud, new Rectangle(area.X, area.Y, cap, area.Height), new Rectangle(source.X, source.Y, Cap, source.Height), colour);
        spriteBatch.Draw(_art.Hud, new Rectangle(area.X + cap, area.Y, area.Width - cap * 2, area.Height), new Rectangle(source.X + Cap, source.Y, Middle, source.Height), colour);
        spriteBatch.Draw(_art.Hud, new Rectangle(area.Right - cap, area.Y, cap, area.Height), new Rectangle(source.Right - Cap, source.Y, Cap, source.Height), colour);

        if (icon is Rectangle face)
            _art.Ability(spriteBatch, face, new Vector2(area.X + IconInset, area.Center.Y - CombatIcons.AbilitySize / 2), 1f, colour);

        if (progress is float share)
        {
            int frames = CombatIcons.Frames("progress-ring");
            int frame = Math.Clamp((int)MathF.Round(share * (frames - 1)), 0, frames - 1);
            _art.Piece(spriteBatch, "progress-ring", Ring(area).Location.ToVector2(), Scale, Color.White * alpha, frame);
        }
    }

    /// <summary>Writes the banner's line. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="area">Where the banner is, on screen.</param>
    /// <param name="text">The line.</param>
    /// <param name="lit">Whether the support acts at the end of this round.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawWords(SpriteBatch spriteBatch, Rectangle area, string text, bool lit, float alpha)
    {
        float x = area.X + IconInset + CombatIcons.AbilitySize + IconGap;
        Text.Draw(spriteBatch, _font, text, x, area.Center.Y - _font.LineSpacing / 2f, (lit ? Palette.Torch : Palette.Note) * alpha, Palette.Shadow * alpha);
    }
}
