using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// A row of keys drawn as key caps, with the letters on them, and the mouse drawn as a mouse with
/// the button that's meant lit. The tutorial's cards and HOW TO PLAY show their keys with it.
/// </summary>
/// <remarks>
/// keycaps.png is one cap, nine-sliced so it's as wide as what's written on it, then three mice:
/// the left button lit, the right, and the wheel. A key's name in a tip is what's written on its
/// cap ("E", "TAB", "F1-F4"); Click, RightClick and Wheel are the mouse. The caps are art, so they
/// go in a point sampled batch, and the letters in a linear one after, the same split as the party's
/// cards and their names. The letters are ink with no shadow, the way the d20's number is, since a
/// shadow on ivory only smudges it.
/// </remarks>
public class KeyCaps
{
    /// <summary>The key name that's a click of the mouse's left button, drawn as the mouse with it lit.</summary>
    public const string Click = "CLICK";

    /// <summary>The key name that's a click of the right button.</summary>
    public const string RightClick = "RIGHT CLICK";

    /// <summary>The key name that's the mouse wheel.</summary>
    public const string Wheel = "WHEEL";

    /// <summary>One frame of keycaps.png, in art pixels, and the cap's nine-slice corner. Have to match KEYCAP and KEYCAP_CORNER in tools/generate_ui.py.</summary>
    private const int FrameSize = 14;
    private const int CornerSize = 5;

    /// <summary>How much the art is blown up, the same as the buttons and the world.</summary>
    private const float Scale = 3f;

    /// <summary>The rows of a cap's face, top and bottom, in art pixels: the letters go in the middle of these, not on the lip under them.</summary>
    private const int FaceTop = 1;
    private const int FaceBottom = 9;

    /// <summary>The room either side of the letters on a cap, and between one cap and the next, in screen pixels.</summary>
    private const float LetterRoom = 12f;
    private const float Gap = 8f;

    /// <summary>The mice's frames on keycaps.png, after the cap's.</summary>
    private static readonly Dictionary<string, int> MouseFrames = new()
    {
        [Click] = 1,
        [RightClick] = 2,
        [Wheel] = 3,
    };

    private Texture2D _sheet;
    private SpriteFont _font;

    /// <summary>How tall a cap is on screen.</summary>
    public static float Height => FrameSize * Scale;

    /// <summary>Loads the caps and the font written on them.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _sheet = content.Load<Texture2D>("keycaps");
        _font = content.Load<SpriteFont>("spectral-detail");
    }

    /// <summary>How wide a row of caps is on screen, gaps and all.</summary>
    /// <param name="keys">The keys, by what's written on them.</param>
    /// <returns>The width, or 0 for no keys.</returns>
    public float Measure(IReadOnlyList<string> keys)
    {
        float width = 0f;
        for (int i = 0; i < keys.Count; i++) width += Width(keys[i]) + (i > 0 ? Gap : 0f);
        return width;
    }

    /// <summary>Draws the caps and the mice. Goes in a point sampled batch, before DrawLetters.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, back to front.</param>
    /// <param name="keys">The keys, by what's written on them.</param>
    /// <param name="at">The top left of the row, in screen pixels.</param>
    /// <param name="alpha">How far it's faded in, 0 to 1.</param>
    public void DrawCaps(SpriteBatch spriteBatch, IReadOnlyList<string> keys, Vector2 at, float alpha)
    {
        float x = MathF.Round(at.X);
        foreach (string key in keys)
        {
            float width = Width(key);
            if (MouseFrames.TryGetValue(key, out int frame))
            {
                var source = new Rectangle(frame * FrameSize, 0, FrameSize, FrameSize);
                spriteBatch.Draw(_sheet, new Vector2(x, MathF.Round(at.Y)), source, Color.White * alpha,
                    0f, Vector2.Zero, Scale, SpriteEffects.None, Layers.ButtonPlate);
            }
            else
            {
                var cap = new Rectangle((int)x, (int)MathF.Round(at.Y), (int)width, (int)Height);
                NineSlice.Draw(spriteBatch, _sheet, cap, Point.Zero, FrameSize, CornerSize, Scale, Color.White * alpha, Layers.ButtonPlate);
            }
            x += width + Gap;
        }
    }

    /// <summary>Writes each key's name on its cap. Goes in a linear sampled batch, after DrawCaps.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, back to front.</param>
    /// <param name="keys">The keys, by what's written on them.</param>
    /// <param name="at">The top left of the row, the same as DrawCaps had.</param>
    /// <param name="alpha">How far it's faded in, 0 to 1.</param>
    public void DrawLetters(SpriteBatch spriteBatch, IReadOnlyList<string> keys, Vector2 at, float alpha)
    {
        float x = MathF.Round(at.X);
        float middle = MathF.Round(at.Y) + (FaceTop + FaceBottom + 1) * Scale / 2f;
        foreach (string key in keys)
        {
            float width = Width(key);
            if (!MouseFrames.ContainsKey(key))
            {
                Vector2 size = _font.MeasureString(key);
                var position = new Vector2(MathF.Round(x + (width - size.X) / 2f), MathF.Round(middle - size.Y / 2f));
                spriteBatch.DrawString(_font, key, position, Palette.Ink * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, Layers.ButtonLabel);
            }
            x += width + Gap;
        }
    }

    /// <summary>How wide one key is on screen: a mouse is its frame, and a cap is its letters with room either side, never narrower than its two corners.</summary>
    private float Width(string key)
    {
        if (MouseFrames.ContainsKey(key)) return FrameSize * Scale;
        return MathF.Max(MathF.Round(_font.MeasureString(key).X + LetterRoom * 2f), CornerSize * 2 * Scale);
    }
}
