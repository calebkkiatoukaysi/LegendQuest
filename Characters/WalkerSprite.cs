using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
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
/// stands instead, and the nudge and the flash carry the moment on their own. In the side-on rooms
/// everyone faces left or right only (SideOnly): the sheets keep their four rows, and the down and
/// up ones just never get asked for.
/// <para>
/// Someone with an imported picture (Art, from ImportedArt) is drawn as that instead of a frame off
/// the sheet, mirrored to face left, and moved by transforms where the sheet has frames: a slow
/// breath, a bob and a lean into each step when walking, a lean forward to strike, back to wind up,
/// cast or take a blow, and lying down when Downed. The same Play, Nudge, Flash and Opacity drive
/// it, so a fight or a scene doesn't know which it's drawing. The box for pointing at them, the
/// shadow and the height over their head are the picture's own (Figure, ShadowSize and Size).
/// </para>
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

    /// <summary>An imported picture's breath: how much taller it stands at the top of one, and how long one takes, in seconds.</summary>
    private const float Breath = 0.012f;
    private const float BreathSeconds = 3.4f;

    /// <summary>
    /// A figure put together from parts plays the clips (ClipLibrary) on its skeleton: standing
    /// breathes, moving jogs (the party's pace) or, slower, walks (a stroll), each fighting pose plays
    /// its move (for the weapon in hand), and a cutscene can play any clip by name (PlayClip).
    /// </summary>
    private const string IdleClip = "idle", JogClip = "jog", StrollClip = "walk";

    /// <summary>
    /// How fast someone from parts is going decides how they move: from JogFrom world pixels a second
    /// they jog, and under StrollUnder they walk (between, whichever they were doing). A clip that
    /// moves the figure along plays as much faster or slower as they're really going than the pace its
    /// planted feet were worked out for, so the feet don't slide, within MinPace and MaxPace; walking
    /// on the spot (InPlace or slower: the character screen) it plays at its own pace. The speed is
    /// eased over SpeedSettle seconds, and a jump of more than MaxGroundSpeed (a door, a cut) isn't
    /// walking.
    /// </summary>
    private const float JogFrom = 40f, StrollUnder = 34f, InPlace = 3f, MinPace = 0.6f, MaxPace = 1.7f;
    private const float SpeedSettle = 0.1f, MaxGroundSpeed = 300f;

    /// <summary>How much deeper each piece behind the front one sorts, so a figure's pieces stay in order between two people at the same depth.</summary>
    private const float PieceStep = 1e-6f;

    /// <summary>The glow in the near hand while casting: its colour, and its size against the figure's height.</summary>
    private static readonly Color CastGlow = new(255, 226, 150);
    private const float CastGlowSize = 0.28f;

    /// <summary>How far apart in a breath each walker starts from the last one made, as a share of a breath, so a crowd doesn't breathe together.</summary>
    private const float BreathSpacing = 0.618034f;

    /// <summary>An imported picture walking: how far it bobs up between footfalls, in world pixels, and how far it leans into the walk and sways from step to step, in radians.</summary>
    private const float WalkBob = 1.25f;
    private const float WalkLean = 0.035f;
    private const float WalkSway = 0.018f;

    /// <summary>How far an imported picture leans for each fighting pose, in radians toward the way it faces (less than nothing is back), and how much taller it stands to cast.</summary>
    private const float WindUpLean = -0.07f;
    private const float StrikeLean = 0.16f;
    private const float CastLean = -0.04f;
    private const float HurtLean = -0.13f;
    private const float CastStretch = 0.03f;

    /// <summary>How quickly an imported picture eases into a pose, and down to lying or back up, as a rate a second.</summary>
    private const float PoseRate = 22f;
    private const float FallRate = 8f;

    /// <summary>A lying picture: how much of its height is the thickness of its back, which it's lifted by so it lies on the floor rather than through it, and how wide its shadow spreads against its height.</summary>
    private const float LyingRest = 0.12f;
    private const float LyingShadow = 0.9f;

    /// <summary>How many walkers there have been, for where in a breath the next one starts.</summary>
    private static int _made;

    private Texture2D _shadow;
    private float _clock;
    private int _frame;
    private Direction _facing = SideOnly ? Direction.Right : Direction.Down;

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

    /// <summary>An imported picture's lean (radians toward the way it faces) and stretch for its pose, and how far it's lying down (0 standing, 1 flat), each eased toward the pose's.</summary>
    private float _lean;
    private float _stretch;
    private float _lying;

    /// <summary>A figure from parts: its cast glow, eased toward the pose's.</summary>
    private float _glow;

    /// <summary>A figure from parts: the clip it's playing, the clip a cutscene asked for (held until another pose or Release), and its skeleton worked out.</summary>
    private ClipPlayer _player;
    private Clip _gesture;
    private float[] _turns;

    /// <summary>How fast they're going over the ground, eased, in world pixels a second; where they were last update; and whether they last moved at a stroll.</summary>
    private float _groundSpeed;
    private Vector2 _lastPosition;
    private bool _measured, _strolling;

    /// <summary>Whether the clip playing says when the feet land (Stepped fires then rather than off the sheet's frames).</summary>
    private bool _clipSteps;
    private Vector2[] _joints, _at;
    private readonly List<ImportedFigure.Piece> _order = new();

    /// <summary>How far through a breath an imported picture is, in seconds.</summary>
    private float _breath = _made++ * BreathSpacing % 1f * BreathSeconds;

    /// <summary>The sheet to cut frames from.</summary>
    public Texture2D Sheet { get; set; }

    /// <summary>The imported picture their portraits use, and that they're drawn as instead of the sheet if it has a body; or null for the sheet.</summary>
    public ImportedFigure Art { get; set; }

    /// <summary>Whether they're drawn as their picture rather than off the sheet: they've one, and it has a body. One put together before their armor's made has only a face.</summary>
    public bool DrawsArt => Body is not null;

    /// <summary>The picture they're drawn as, or null if they're drawn off the sheet.</summary>
    private ImportedFigure Body => Art is { HasBody: true } ? Art : null;

    /// <summary>The shape of the frames on the sheet: how big, where the feet are, the figure, the shadow and the footprint. A person's unless it says otherwise.</summary>
    public WalkerShape Shape { get; init; } = WalkerShape.Person;

    /// <summary>Where the feet are, in world pixels.</summary>
    public Vector2 Position { get; set; }

    /// <summary>Whether everyone faces only left or right, as they do in the side-on rooms: setting Up or Down does nothing, and Face goes by how far across the point is alone.</summary>
    /// <remarks>On for every walker, since there are only rooms to walk in. The sheets keep their up and down rows for whatever comes after.</remarks>
    public static bool SideOnly { get; set; } = true;

    /// <summary>Which way they're facing. With SideOnly on, Up and Down are ignored and they keep facing the way they were.</summary>
    public Direction Facing
    {
        get => _facing;
        set
        {
            if (SideOnly && value is Direction.Up or Direction.Down) return;
            _facing = value;
        }
    }

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

    /// <summary>How big the figure stands: how wide, and how tall up from the feet, in world pixels. An imported picture's own size, or the shape's figure.</summary>
    public Vector2 Size => Body?.Size ?? Shape.Figure;

    /// <summary>How wide and deep the shadow under the feet is, in world pixels: an imported picture's own, or the shape's.</summary>
    public Vector2 ShadowSize => Body?.Shadow ?? Shape.Shadow;

    /// <summary>The box round the figure, from its feet to the top of its head, for telling when the mouse is over it. An imported picture's is the picture, which sits off the feet by however much of it is behind them.</summary>
    public BoundingRectangle Figure
    {
        get
        {
            Vector2 size = Size;
            float left = Position.X - size.X / 2f;
            if (Body is not null)
            {
                float behind = Body.Feet.X * ImportedFigure.Texel;
                left = Facing == Direction.Left ? Position.X + behind - size.X : Position.X - behind;
            }
            return new BoundingRectangle(left, Position.Y - size.Y, size.X, size.Y);
        }
    }

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
        _gesture = null;
        _pose = pose;
        _poseHeld = seconds <= 0f;
        _poseLeft = seconds;
    }

    /// <summary>Lets go of whatever pose they're in, back to standing or walking.</summary>
    public void Release() => Play(WalkerPose.None);

    /// <summary>Whether they're a figure from parts that plays clips.</summary>
    public bool PlaysClips => Body is { Joints: not null } && Skeleton.Current is not null;

    /// <summary>Plays a clip by name (ClipLibrary) until another pose or Release: one that doesn't loop holds its last pose.</summary>
    /// <param name="name">The clip's name, or another name for it.</param>
    /// <returns>How long it takes, in seconds; 0 if they don't play clips or there's no such clip.</returns>
    public float PlayClip(string name)
    {
        if (!PlaysClips || !ClipLibrary.TryGet(name, Body.Base, out Clip clip)) return 0f;
        _pose = WalkerPose.None;
        _gesture = clip;
        return clip.Length;
    }

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

    /// <summary>Runs the walk cycle, or stands still, runs the clocks on the pose, the nudge and the flash, and eases an imported picture toward its pose.</summary>
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
        Measure(seconds);
        Settle(seconds);
        Animate(seconds);

        if (!IsWalking)
        {
            _clock = 0f;
            _frame = 0;
            return;
        }

        _clock += seconds;
        int frame = 1 + (int)(_clock / StepTime) % WalkFrames;

        // The first step happens right away, so even a quick tap makes a footstep. A figure from parts
        // steps when its clip's feet land instead (Animate).
        if (frame != _frame && (frame == 1 || frame == 3) && !_clipSteps) Stepped?.Invoke();
        _frame = frame;
    }

    /// <summary>Eases the speed they're going over the ground toward how far they've moved since the last update.</summary>
    /// <param name="seconds">How long since the last update.</param>
    private void Measure(float seconds)
    {
        if (seconds <= 0f) return;
        if (_measured)
        {
            float moved = Vector2.Distance(Position, _lastPosition) / seconds;
            if (moved > MaxGroundSpeed) moved = _groundSpeed;
            _groundSpeed += (moved - _groundSpeed) * (1f - MathF.Exp(-seconds / SpeedSettle));
        }
        _lastPosition = Position;
        _measured = true;
    }

    /// <summary>Turns to face a point: whichever of the four ways is closest, or with SideOnly on, left or right by which side of them it's on.</summary>
    /// <param name="target">The point, in world pixels. Straight above or below them, with SideOnly on, they don't turn.</param>
    public void Face(Vector2 target)
    {
        Vector2 to = target - Position;
        if (SideOnly)
        {
            if (to.X != 0f) Facing = to.X < 0f ? Direction.Left : Direction.Right;
            return;
        }
        if (to == Vector2.Zero) return;

        Facing = MathF.Abs(to.X) > MathF.Abs(to.Y)
            ? to.X < 0f ? Direction.Left : Direction.Right
            : to.Y < 0f ? Direction.Up : Direction.Down;
    }

    /// <summary>Draws the shadow and the sprite at Position, nudged, flashed and faded as they are: a frame off the sheet, or the imported picture.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="layerDepth">Where it sorts. The map works this out from how far down it the feet are.</param>
    /// <param name="pixel">How big one screen pixel is in world pixels. The feet snap to it so the sprite doesn't crawl.</param>
    /// <param name="tint">What to multiply the sprite by: the map's tint. Not the shadow, which is black whatever the light.</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth, float pixel, Color tint)
    {
        if (Sheet is null && Body is null || Opacity <= 0f) return;

        // Between screen pixels, the art pixels would come out two and four screen pixels wide by turns.
        Vector2 at = Position + _nudge * MathF.Sin(MathHelper.Pi * _nudgeClock / _nudgeLength);
        var feet = new Vector2(MathF.Round(at.X / pixel) * pixel, MathF.Round(at.Y / pixel) * pixel);

        // A picture lying down spreads its shadow back along the floor under it.
        float fall = _lying * _lying * (3f - 2f * _lying);
        Vector2 size = ShadowSize;
        float back = 0f;
        if (Body is not null)
        {
            size.X = MathHelper.Lerp(size.X, Size.Y * LyingShadow, fall);
            back = fall * Size.Y / 2f * (Facing == Direction.Left ? 1f : -1f);
        }
        var shadow = new Rectangle(
            (int)MathF.Round(feet.X + back - size.X / 2f),
            (int)MathF.Round(feet.Y - size.Y / 2f),
            (int)size.X,
            (int)size.Y);
        spriteBatch.Draw(_shadow, shadow, null, ShadowColor * Opacity, 0f, Vector2.Zero, SpriteEffects.None, MathF.Min(1f, layerDepth + 0.0001f));

        float flash = 1f - _flashClock / _flashLength;
        Color colour = Color.Lerp(tint, new Color(tint.ToVector4() * FlashColour.ToVector4()), flash * flash) * Opacity;
        if (Body is not null)
        {
            DrawArt(spriteBatch, feet, pixel, colour, layerDepth, fall);
            return;
        }

        Point frame = Shape.Frame;
        int column = _pose != WalkerPose.None && HasPoses ? WalkFrames + (int)_pose : _frame;
        var source = new Rectangle(column * frame.X, (int)Facing * frame.Y, frame.X, frame.Y);
        spriteBatch.Draw(Sheet, feet, source, colour, 0f, Shape.Feet, 1f, SpriteEffects.None, layerDepth);
    }

    /// <summary>Draws the imported picture at the feet: mirrored facing left, breathing, bobbing and leaning into the walk, leaning into its pose, and lying back along the floor as it falls.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="feet">Where the feet are, snapped to the screen's pixels.</param>
    /// <param name="pixel">How big one screen pixel is in world pixels, for snapping the bob and the lift.</param>
    /// <param name="colour">The tint, the flash and the fade together.</param>
    /// <param name="layerDepth">Where it sorts.</param>
    /// <param name="fall">How far it's lying down, eased: 0 standing, 1 flat.</param>
    private void DrawArt(SpriteBatch spriteBatch, Vector2 feet, float pixel, Color colour, float layerDepth, float fall)
    {
        bool left = Facing == Direction.Left;
        float stride = _clock / (StepTime * WalkFrames) * MathHelper.TwoPi;
        float bob = IsWalking ? WalkBob * MathF.Abs(MathF.Sin(stride)) : 0f;
        float lean = _lean + (IsWalking ? WalkLean + WalkSway * MathF.Sin(stride) : 0f);
        float stretch = MathHelper.Lerp(1f + _stretch + Breath * MathF.Sin(_breath / BreathSeconds * MathHelper.TwoPi), 1f, fall);
        float rotation = (left ? -1f : 1f) * MathHelper.Lerp(lean, -MathHelper.PiOver2, fall);
        ImportedFigure body = Body;
        float rest = fall * MathF.Min(body.Feet.X, body.Picture.Height * LyingRest) * ImportedFigure.Texel;

        if (PlaysClips && body.Rig.Count > 0)
        {
            DrawRigged(spriteBatch, body, feet, new Vector2(ImportedFigure.Texel), colour, layerDepth);
            return;
        }
        var at = new Vector2(feet.X, feet.Y - MathF.Round((bob + rest) / pixel) * pixel);
        var scale = new Vector2(ImportedFigure.Texel, ImportedFigure.Texel * stretch);
        var origin = new Vector2(left ? body.Picture.Width - body.Feet.X : body.Feet.X, body.Feet.Y);
        spriteBatch.Draw(body.Picture, at, null, colour, rotation, origin, scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
    }

    /// <summary>
    /// A figure from parts, piece by piece, as its clip has it now: the skeleton worked out
    /// (Skeleton.Solve), each piece drawn turned with its bone about where its joint has gone,
    /// mirrored facing left, back to front, with what the clip brings in front of the chest after the
    /// head. Then the cast glow in the near hand.
    /// </summary>
    private void DrawRigged(SpriteBatch spriteBatch, ImportedFigure body, Vector2 at, Vector2 scale, Color colour, float layerDepth)
    {
        Skeleton skeleton = Skeleton.Current;
        if (_player is null || _joints is null || _joints.Length != skeleton.Count) Animate(0f);
        bool left = Facing == Direction.Left;
        float sign = left ? -1f : 1f;
        for (int i = 0; i < skeleton.Count; i++) _joints[i] = body.Joints.TryGetValue(skeleton.Names[i], out Vector2 joint) ? joint : body.Feet;
        skeleton.Solve(_player.Pose, _joints, body.RigPx, _turns, _at);
        Vector2 Place(Vector2 onPicture) => at + (onPicture - body.Feet) * new Vector2(sign, 1f) * scale;

        // Back to front, what the clip brings in front of the chest moved to just after the head.
        _order.Clear();
        IReadOnlyList<string> front = _player.Clip?.Front ?? Array.Empty<string>();
        int afterHead = -1;
        foreach (ImportedFigure.Piece piece in body.Rig)
        {
            if (front.Count > 0 && Contains(front, piece.Bone)) continue;
            _order.Add(piece);
            if (piece.Key == "head") afterHead = _order.Count;
        }
        if (front.Count > 0)
        {
            int at0 = afterHead < 0 ? _order.Count : afterHead;
            foreach (ImportedFigure.Piece piece in body.Rig)
                if (Contains(front, piece.Bone)) _order.Insert(at0++, piece);
        }

        for (int k = 0; k < _order.Count; k++)
        {
            ImportedFigure.Piece piece = _order[k];
            int i = skeleton.IndexOf(piece.Bone);
            if (i < 0) continue;
            Vector2 origin = piece.Pivot - piece.Offset;
            if (left) origin.X = piece.Picture.Width - origin.X;
            BonePose own = _player.Pose[i];
            float depth = MathF.Min(1f, layerDepth + (_order.Count - 1 - k) * PieceStep);
            spriteBatch.Draw(piece.Picture, Place(_at[i]), null, colour, sign * _turns[i], origin, scale * new Vector2(own.Sx, own.Sy),
                left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, depth);
        }

        if (_glow <= 0.01f) return;
        int hand = skeleton.IndexOf("hand_near");
        if (hand < 0) return;
        float size = body.Size.Y * CastGlowSize * (0.85f + 0.15f * MathF.Sin(_breath * 9f));
        spriteBatch.Draw(_shadow, Place(_at[hand]), null, CastGlow * (_glow * 0.75f * Opacity), 0f, new Vector2(_shadow.Width, _shadow.Height) / 2f,
            size / _shadow.Width, SpriteEffects.None, MathF.Max(0f, layerDepth - PieceStep));
    }

    private static bool Contains(IReadOnlyList<string> names, string name)
    {
        for (int i = 0; i < names.Count; i++)
            if (names[i] == name) return true;
        return false;
    }

    /// <summary>
    /// For a figure from parts, picks the clip for what they're doing and moves it on: a cutscene's
    /// clip while it holds; else their fighting pose's move, for the weapon in hand; else jogging or
    /// strolling, at the pace they're going; else standing. Fires Stepped as the clip's feet land.
    /// </summary>
    /// <param name="seconds">How long since the last update.</param>
    private void Animate(float seconds)
    {
        if (!PlaysClips) return;
        Skeleton skeleton = Skeleton.Current;
        if (_player is null || _joints is null || _joints.Length != skeleton.Count)
        {
            _player = new ClipPlayer(skeleton);
            _turns = new float[skeleton.Count];
            _joints = new Vector2[skeleton.Count];
            _at = new Vector2[skeleton.Count];
        }
        ImportedFigure body = Body;
        Clip clip = _gesture ?? (_pose != WalkerPose.None ? PoseClip(_pose) : null);
        if (clip is null && IsWalking)
        {
            if (_groundSpeed >= JogFrom) _strolling = false;
            else if (_groundSpeed > InPlace && _groundSpeed < StrollUnder) _strolling = true;
            clip = ClipLibrary.Get(_strolling ? StrollClip : JogClip, body.Base);
        }
        clip ??= ClipLibrary.Get(IdleClip, body.Base);

        // A clip that goes somewhere plays at the pace they're really going, so the feet stay put.
        float pace = 1f;
        if (clip is { Speed: > 0f } && _groundSpeed > InPlace && body.RigPx > 0f)
            pace = Math.Clamp(_groundSpeed / (clip.Speed * body.RigPx * ImportedFigure.Texel), MinPace, MaxPace);

        Clip before = _player.Clip;
        float from = _player.Time;
        _player.Play(clip, pace);
        if (_player.Clip != before) from = -1e-4f;
        _player.Update(seconds);

        // Each foot that's landed since the last update.
        _clipSteps = clip is not null && clip.Steps.Count > 0;
        if (!_clipSteps || seconds <= 0f) return;
        float now = _player.Time;
        foreach (float step in clip.Steps)
            if (now >= from ? step > from && step <= now : step > from || step <= now)
                Stepped?.Invoke();
    }

    /// <summary>The clip a fighting pose plays: the wind-up and the strike for the weapon in hand (one-handed if they hold one, unarmed if not), the cast, the flinch, or lying down.</summary>
    private Clip PoseClip(WalkerPose pose)
    {
        string weapon = Body is ImportedFigure body && HoldsWeapon(body) ? "one-hand" : "unarmed";
        return pose switch
        {
            WalkerPose.WindUp => ClipLibrary.ForMove("windup", weapon),
            WalkerPose.Strike => ClipLibrary.ForMove("strike", weapon),
            WalkerPose.Cast => ClipLibrary.ForMove("cast", null) ?? ClipLibrary.Get("cast"),
            WalkerPose.Hurt => ClipLibrary.Get("hurt"),
            WalkerPose.Downed => ClipLibrary.Get("downed"),
            _ => null,
        };
    }

    private static bool HoldsWeapon(ImportedFigure body)
    {
        foreach (ImportedFigure.Piece piece in body.Rig)
            if (piece.Key == RigComposer.Weapon) return true;
        return false;
    }

    /// <summary>Breathes, and eases an imported picture's lean and stretch toward its pose's, and down to lying when it's Downed or back up when it isn't.</summary>
    /// <param name="seconds">How long since the last update.</param>
    private void Settle(float seconds)
    {
        _breath = (_breath + seconds) % BreathSeconds;
        float toward = 1f - MathF.Exp(-PoseRate * seconds);
        _lean += (LeanOf(_pose) - _lean) * toward;
        _glow += ((_pose == WalkerPose.Cast ? 1f : 0f) - _glow) * toward;
        _stretch += ((_pose == WalkerPose.Cast ? CastStretch : 0f) - _stretch) * toward;
        _lying += ((_pose == WalkerPose.Downed ? 1f : 0f) - _lying) * (1f - MathF.Exp(-FallRate * seconds));
    }

    /// <summary>How far an imported picture leans for a pose, in radians toward the way it faces.</summary>
    private static float LeanOf(WalkerPose pose) => pose switch
    {
        WalkerPose.WindUp => WindUpLean,
        WalkerPose.Strike => StrikeLean,
        WalkerPose.Cast => CastLean,
        WalkerPose.Hurt => HurtLean,
        _ => 0f,
    };
}
