using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The mouse cursor, drawn by the game so it can change shape: an arrow, a ring to walk, a hand to
/// use something, a speech bubble to talk, and a crossed circle where you can't go.
/// </summary>
/// <remarks>
/// A game service like the audio manager, so any screen can set the shape. Shape goes back to the
/// arrow every frame after it's drawn, so a screen only has to say when it wants something else. It's
/// drawn last, over every screen, which also puts it in the proof's pictures.
/// </remarks>
public class CursorSprite
{
    /// <summary>One frame of cursors.png, in art pixels, and how much it's blown up. Has to match CURSOR in tools/generate_ui.py.</summary>
    private const int FrameSize = 16;
    private const float Scale = 2f;

    /// <summary>The pixel of each frame that's the point the mouse is at: the arrow's tip, and the middle of the rest. In CursorShape order.</summary>
    private static readonly Vector2[] Hotspots = { new(1, 1), new(8, 8), new(8, 8), new(8, 8), new(8, 8) };

    private Texture2D _sheet;

    /// <summary>Which cursor to draw this frame.</summary>
    public CursorShape Shape { get; set; }

    /// <summary>Loads the cursors.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content) => _sheet = content.Load<Texture2D>("cursors");

    /// <summary>Draws the cursor at the mouse, if the mouse is over the window, then goes back to the arrow for next frame.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="spriteBatch">The SpriteBatch to render with. It's begun and ended here.</param>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Point mouse)
    {
        var window = new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight);
        if (window.Contains(mouse))
        {
            int frame = (int)Shape;
            var source = new Rectangle(frame * FrameSize, 0, FrameSize, FrameSize);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            spriteBatch.Draw(_sheet, mouse.ToVector2(), source, Color.White, 0f, Hotspots[frame], Scale, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        Shape = CursorShape.Default;
    }
}
