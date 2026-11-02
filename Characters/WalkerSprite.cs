using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Collisions;

namespace LegendQuest.Characters;

/// <summary>
/// Someone walking around a map. Cuts frames from their sheet, animates while they move, strikes
/// the fighting poses, and draws a shadow under them.
/// </summary>
/// <remarks>
/// The Black Box's WalkerSprite on the new sheet layout. A sheet is ten columns (standing, four
/// walking frames, then wind-up, strike, cast, hurt and downed) and four rows, one per direction.
/// Columns 1 and 3 are when a foot hits the ground, so that's when Stepped fires and the footsteps
/// match the animation. How big a frame is and where the feet are in it is the Shape: a person's
/// 32x48, the same as The Black Box's so sheets made for it still cut the same, unless they're a
/// creature with a bigger frame of its own. A fight plays the poses (Play), nudges the figure out
/// and back for a lunge or a recoil (Nudge), flashes it red when it's hit (Flash) and fades it in or
/// out (Opacity). A townsfolk sheet is only the first five columns, so on one of those a pose
/// stands instead, and the nudge and the flash carry the moment on their own.
/// </remarks>
public class WalkerSprite
{
    /// <summary>How many walking frames come after the standing one.</summary>
    private const int WalkFrames = 4;

    /// <summary>How many fighting poses come after the walk: wind-up, strike, cast, hurt and downed.</summary>
    private const int Poses = 5;

    /// <summary>How many rows a sheet has: one for each way someone can face.</summary>
    private const int Rows = 4;

    /// <summary>Seconds per walking frame. Four frames is one full step cycle.</summary>
    private const float StepTime = 0.14f;

    /// <summary>How dark the shadow under the feet is.</summary>
    private static readonly Color ShadowColor = new Color(0, 0, 0) * 0.5f;

    /// <summary>What a flash tints the figure toward at its height: a hot red, multiplied into whatever the map's tint is.</summary>
    private static readonly Color FlashColour = new(255, 90, 76);

    private Texture2D _shadow;
    private float _clock;
    private int _frame;

    /// <summary>The pose struck, and how much longer it's held, in seconds; a held pose has no end.</summary>
    private WalkerPose _pose;
    private float _poseLeft;
    private bool _poseHeld;

    /// <summary>The nudge playing: how far out it goes at its height, how long it's been going, and how long it takes there and back. It starts as one that's finished, so nothing moves.</summary>
    private Vector2 _nudge;
    private float _nudgeClock = 1f;
    private float _nudgeLength = 1f;

    /// <summary>The flash playing: how long it's been going, and how long it takes to die away. It starts as one that's died away.</summary>
    private float _flashClock = 1f;
    private float _flashLength = 1f;

    /// <summary>The sheet to cut frames from.</summary>
    public Texture2D Sheet { get; set; }

    /// <summary>The shape of the frames on the sheet: how big, where the feet are, the figure, the shadow and the footprint. A person's unless it says otherwise.</summary>
    public WalkerShape Shape { get; init; } = WalkerShape.Person;

    /// <summary>Where the feet are, in world pixels.</summary>
    public Vector2 Position { get; set; }

    /// <summary>Which way they're facing.</summary>
    public Direction Facing { get; set; } = Direction.Down;

    /// <summary>Whether they're walking. Standing still uses the first column.</summary>
    public bool IsWalking { get; set; }

    /// <summary>How much of them is drawn, 0 to 1: a fight fades in someone arriving and fades out someone who's died.</summary>
    public float Opacity { get; set; } = 1f;

    /// <summary>The fighting pose they've struck, or None.</summary>
    public WalkerPose Pose => _pose;

    /// <summary>Whether their sheet has the fighting poses: all ten columns, not just the townsfolk's five.</summary>
    public bool HasPoses => Sheet is not null && Sheet.Width >= (1 + WalkFrames + Poses) * Shape.Frame.X;

    /// <summary>Fires when a foot hits the ground.</summary>
    public event Action Stepped;

    /// <summary>The box round the figure, from its feet to the top of its head, for telling when the mouse is over it.</summary>
    public BoundingRectangle Figure => new(Position.X - Shape.Figure.X / 2f, Position.Y - Shape.Figure.Y, Shape.Figure.X, Shape.Figure.Y);

    /// <summary>Whether a sheet has room for every frame a walker cuts from it at a shape's size: the standing and walking columns, in all four rows.</summary>
    /// <param name="sheet">The sheet.</param>
    /// <param name="shape">The shape its frames are meant to be.</param>
    public static bool Fits(Texture2D sheet, WalkerShape shape) =>
        sheet.Width >= (1 + WalkFrames) * shape.Frame.X && sheet.Height >= Rows * shape.Frame.Y;

    /// <summary>Loads the shadow. It's the glow texture squashed flat, same trick as The Black Box.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content) => _shadow = content.Load<Texture2D>("glow");

    /// <summary>Strikes a fighting pose.</summary>
    /// <param name="pose">Which pose. None goes back to standing or walking at once.</param>
    /// <param name="seconds">How long to hold it before going back, or 0 to hold it until another pose or Release (someone Downed stays down).</param>
    public void Play(WalkerPose pose, float seconds = 0f)
    {
        _pose = pose;
        _poseHeld = seconds <= 0f;
        _poseLeft = seconds;
    }

    /// <summary>Lets go of whatever pose they're in, back to standing or walking.</summary>
    public void Release() => Play(WalkerPose.None);

    /// <summary>Moves the figure out and back again, easing in and out: a lunge toward a foe, a recoil from a blow, a sidestep from one that missed.</summary>
    /// <param name="by">How far it goes at its height, in world pixels.</param>
    /// <param name="seconds">How long the whole of it takes, there and back.</param>
    public void Nudge(Vector2 by, float seconds)
    {
        _nudge = by;
        _nudgeClock = 0f;
        _nudgeLength = MathF.Max(seconds, float.Epsilon);
    }

    /// <summary>Flashes the figure red, from full and dying away, for a hit.</summary>
    /// <param name="seconds">How long it takes to die away.</param>
    public void Flash(float seconds)
    {
        _flashClock = 0f;
        _flashLength = MathF.Max(seconds, float.Epsilon);
    }

    /// <summary>Runs the walk cycle, or stands still, and runs the clocks on the pose, the nudge and the flash.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _nudgeClock = MathF.Min(_nudgeLength, _nudgeClock + seconds);
        _flashClock = MathF.Min(_flashLength, _flashClock + seconds);
        if (_pose != WalkerPose.None && !_poseHeld)
        {
            _poseLeft -= seconds;
            if (_poseLeft <= 0f) _pose = WalkerPose.None;
        }

        if (!IsWalking)
        {
            _clock = 0f;
            _frame = 0;
            return;
        }

        _clock += seconds;
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

    /// <summary>Draws the shadow and the sprite at Position, nudged, flashed and faded as they are.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="layerDepth">Where it sorts. The map works this out from how far down it the feet are.</param>
    /// <param name="pixel">How big one screen pixel is in world pixels. The feet snap to it so the sprite doesn't crawl.</param>
    /// <param name="tint">What to multiply the sprite by: the map's tint. Not the shadow, which is black whatever the light.</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth, float pixel, Color tint)
    {
        if (Sheet is null || Opacity <= 0f) return;

        // Between screen pixels, the art pixels would come out two and four screen pixels wide by turns.
        Vector2 at = Position + _nudge * MathF.Sin(MathHelper.Pi * _nudgeClock / _nudgeLength);
        var feet = new Vector2(MathF.Round(at.X / pixel) * pixel, MathF.Round(at.Y / pixel) * pixel);

        Vector2 size = Shape.Shadow;
        var shadow = new Rectangle(
            (int)MathF.Round(feet.X - size.X / 2f),
            (int)MathF.Round(feet.Y - size.Y / 2f),
            (int)size.X,
            (int)size.Y);
        spriteBatch.Draw(_shadow, shadow, null, ShadowColor * Opacity, 0f, Vector2.Zero, SpriteEffects.None, MathF.Min(1f, layerDepth + 0.0001f));

        float flash = 1f - _flashClock / _flashLength;
        Color colour = Color.Lerp(tint, new Color(tint.ToVector4() * FlashColour.ToVector4()), flash * flash) * Opacity;

        Point frame = Shape.Frame;
        int column = _pose != WalkerPose.None && HasPoses ? WalkFrames + (int)_pose : _frame;
        var source = new Rectangle(column * frame.X, (int)Facing * frame.Y, frame.X, frame.Y);
        spriteBatch.Draw(Sheet, feet, source, colour, 0f, Shape.Feet, 1f, SpriteEffects.None, layerDepth);
    }
}
