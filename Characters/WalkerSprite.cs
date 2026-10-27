using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Collisions;

namespace LegendQuest.Characters;

/// <summary>
/// Someone walking around a map. Cuts frames from their sheet, animates while they move, and draws
/// a shadow under them.
/// </summary>
/// <remarks>
/// The Black Box's WalkerSprite on the new sheet layout. A sheet is ten columns (standing, four
/// walking frames, then wind-up, strike, cast, hurt and downed for later) and four rows, one per
/// direction. Columns 1 and 3 are when a foot hits the ground, so that's when Stepped fires and the
/// footsteps match the animation.
/// </remarks>
public class WalkerSprite
{
    /// <summary>How wide one frame of a sheet is, in art pixels. The same as The Black Box's, so sheets made for it still cut the same.</summary>
    private const int FrameWidth = 32;

    /// <summary>How tall one frame is, in art pixels.</summary>
    private const int FrameHeight = 48;

    /// <summary>How many walking frames come after the standing one.</summary>
    private const int WalkFrames = 4;

    /// <summary>Where the feet are in a frame (middle, bottom of the soles). The sprite is drawn from this point.</summary>
    private static readonly Vector2 FeetInFrame = new(16f, 46f);

    /// <summary>Seconds per walking frame. Four frames is one full step cycle.</summary>
    private const float StepTime = 0.14f;

    /// <summary>The box round the figure itself, for pointing at it: how wide and how tall, in art pixels, up from the feet.</summary>
    private const float FigureWidth = 14f;
    private const float FigureHeight = 42f;

    /// <summary>The shadow under the feet: width, height (art pixels) and how dark.</summary>
    private const float ShadowWidth = 16f;
    private const float ShadowHeight = 5f;
    private static readonly Color ShadowColor = new Color(0, 0, 0) * 0.5f;

    private Texture2D _shadow;
    private float _clock;
    private int _frame;

    /// <summary>The sheet to cut frames from.</summary>
    public Texture2D Sheet { get; set; }

    /// <summary>Where the feet are, in world pixels.</summary>
    public Vector2 Position { get; set; }

    /// <summary>Which way they're facing.</summary>
    public Direction Facing { get; set; } = Direction.Down;

    /// <summary>Whether they're walking. Standing still uses the first column.</summary>
    public bool IsWalking { get; set; }

    /// <summary>Fires when a foot hits the ground.</summary>
    public event Action Stepped;

    /// <summary>The box round the figure, from its feet to the top of its head, for telling when the mouse is over it.</summary>
    public BoundingRectangle Figure => new(Position.X - FigureWidth / 2f, Position.Y - FigureHeight, FigureWidth, FigureHeight);

    /// <summary>Loads the shadow. It's the glow texture squashed flat, same trick as The Black Box.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content) => _shadow = content.Load<Texture2D>("glow");

    /// <summary>Runs the walk cycle, or stands still.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        if (!IsWalking)
        {
            _clock = 0f;
            _frame = 0;
            return;
        }

        _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
        int frame = 1 + (int)(_clock / StepTime) % WalkFrames;

        // The first step happens right away, so even a quick tap makes a footstep.
        if (frame != _frame && (frame == 1 || frame == 3)) Stepped?.Invoke();
        _frame = frame;
    }

    /// <summary>Turns to face a point, whichever of the four ways is closest.</summary>
    /// <param name="target">The point, in world pixels.</param>
    public void Face(Vector2 target)
    {
        Vector2 to = target - Position;
        if (to == Vector2.Zero) return;

        Facing = MathF.Abs(to.X) > MathF.Abs(to.Y)
            ? to.X < 0f ? Direction.Left : Direction.Right
            : to.Y < 0f ? Direction.Up : Direction.Down;
    }

    /// <summary>Draws the shadow and the sprite at Position.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="layerDepth">Where it sorts. The map works this out from how far down it the feet are.</param>
    /// <param name="pixel">How big one screen pixel is in world pixels. The feet snap to it so the sprite doesn't crawl.</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth, float pixel)
    {
        if (Sheet is null) return;

        // Between screen pixels, the art pixels would come out two and four screen pixels wide by turns.
        var feet = new Vector2(MathF.Round(Position.X / pixel) * pixel, MathF.Round(Position.Y / pixel) * pixel);

        var shadow = new Rectangle(
            (int)MathF.Round(feet.X - ShadowWidth / 2f),
            (int)MathF.Round(feet.Y - ShadowHeight / 2f),
            (int)ShadowWidth,
            (int)ShadowHeight);
        spriteBatch.Draw(_shadow, shadow, null, ShadowColor, 0f, Vector2.Zero, SpriteEffects.None, MathF.Min(1f, layerDepth + 0.0001f));

        var source = new Rectangle(_frame * FrameWidth, (int)Facing * FrameHeight, FrameWidth, FrameHeight);
        spriteBatch.Draw(Sheet, feet, source, Color.White, 0f, FeetInFrame, 1f, SpriteEffects.None, layerDepth);
    }
}
