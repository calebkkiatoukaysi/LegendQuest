using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest;

/// <summary>
/// Drawing a line of text over its shadow. Shared by every screen that writes anything.
/// </summary>
public static class Text
{
    /// <summary>Draws a line centred across the screen, over a hard offset shadow.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="font">The SpriteFont to measure and render with.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="y">Top of the line, in screen pixels.</param>
    /// <param name="color">Colour of the text.</param>
    /// <param name="shadow">Colour of the shadow behind it.</param>
    public static void DrawCentered(SpriteBatch spriteBatch, SpriteFont font, string text, float y, Color color, Color shadow) =>
        Draw(spriteBatch, font, text, LegendQuestGame.ScreenWidth / 2f, y, color, shadow, centred: true);

    /// <summary>Draws a line against a point, over a hard offset shadow.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="font">The SpriteFont to measure and render with.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="x">The left edge, the centre, or the right edge, depending on the flags.</param>
    /// <param name="y">Top of the line, in screen pixels.</param>
    /// <param name="color">Colour of the text.</param>
    /// <param name="shadow">Colour of the shadow behind it.</param>
    /// <param name="centred">True to centre the line on <paramref name="x"/>.</param>
    /// <param name="rightAligned">True to end the line at <paramref name="x"/>.</param>
    public static void Draw(SpriteBatch spriteBatch, SpriteFont font, string text, float x, float y,
        Color color, Color shadow, bool centred = false, bool rightAligned = false)
    {
        if (string.IsNullOrEmpty(text)) return;

        Vector2 size = font.MeasureString(text);
        float left = centred ? x - size.X / 2f : rightAligned ? x - size.X : x;
        var position = new Vector2(MathF.Round(left), MathF.Round(y));

        spriteBatch.DrawString(font, text, position + Palette.ShadowOffset, shadow, 0f, Vector2.Zero, 1f, SpriteEffects.None, Layers.TextShadow);
        spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, Layers.Text);
    }
}
