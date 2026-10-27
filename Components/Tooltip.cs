using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The name of whatever the mouse is over, and a line under it, next to the cursor.
/// </summary>
/// <remarks>
/// It goes below and to the right of the cursor, and flips to the other side near the edge of the
/// screen so it's never cut off. On the same soft dark smudge as the toasts, from Text.DrawBacking.
/// In a fight the line under the name will be the hit chance and the damage.
/// </remarks>
public class Tooltip
{
    /// <summary>How far from the cursor it goes, in screen pixels.</summary>
    private static readonly Vector2 Offset = new(28f, 30f);

    /// <summary>How far the smudge reaches past the text, across and up and down.</summary>
    private static readonly Vector2 Spread = new(14f, 8f);

    private static readonly Color BackingColor = Color.Black * 0.8f;

    private SpriteFont _font;
    private Texture2D _glow;

    /// <summary>Loads the font and the glow the smudge is made from.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _glow = content.Load<Texture2D>("glow");
    }

    /// <summary>Draws a tooltip by the cursor.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, back to front, linear-sampled.</param>
    /// <param name="mouse">Where the cursor is, in screen pixels.</param>
    /// <param name="name">What it is.</param>
    /// <param name="detail">A line under the name, or null.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void Draw(SpriteBatch spriteBatch, Point mouse, string name, string detail, float alpha)
    {
        Vector2 nameSize = _font.MeasureString(name);
        Vector2 detailSize = detail is null ? Vector2.Zero : _font.MeasureString(detail);
        float width = MathF.Max(nameSize.X, detailSize.X);
        float height = _font.LineSpacing * (detail is null ? 1 : 2);

        float x = mouse.X + Offset.X, y = mouse.Y + Offset.Y;
        if (x + width + Spread.X * 2f > LegendQuestGame.ScreenWidth) x = mouse.X - Offset.X - width;
        if (y + height + Spread.Y * 2f > LegendQuestGame.ScreenHeight) y = mouse.Y - Offset.Y - height;

        var area = new Rectangle((int)x, (int)(y - Spread.Y), (int)width, (int)(height + Spread.Y * 2f));
        Text.DrawBacking(spriteBatch, _glow, area, (int)Spread.X, BackingColor * alpha);

        Text.Draw(spriteBatch, _font, name, x, y, Palette.Bone * alpha, Palette.Shadow * alpha);
        if (detail is not null)
            Text.Draw(spriteBatch, _font, detail, x, y + _font.LineSpacing, Palette.Note * alpha, Palette.Shadow * alpha);
    }
}
