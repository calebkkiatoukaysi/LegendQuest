using System;
using System.Collections.Generic;
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
    /// A figure put together from parts moves each piece on top of the whole figure's lean (radians,
    /// toward the way it faces): the head nods with the breath and against the stride; what's in the
    /// near hand sways with the breath, swings with the stride, and goes back for a wind-up, through
    /// for a strike and up for a cast; the far hand braces against a blow; the cape sways on its own
    /// time, trails a walk and flies out from a lunge.
    /// </summary>
    private const float HeadNod = 0.012f, HeadCounter = 0.02f;
    private const float HandSway = 0.025f, HandSwing = 0.07f, FarSwing = 0.04f;
    private const float CapeSway = 0.025f, CapeSeconds = 2.9f, CapeTrail = 0.12f, CapeFlutter = 0.04f;

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

    /// <summary>A figure from parts: its head's nod, its near hand's swing, its far hand's brace, its cape's kick and its cast glow, each eased toward the pose's.</summary>
    private float _nod;
    private float _swing;
    private float _brace;
    private float _kick;
    private float _glow;

    /// <summary>How far through its sway the cape is, in seconds.</summary>
    private float _cape = _made * BreathSpacing % 1f * CapeSeconds;

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
        Settle(seconds);

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

        var at = new Vector2(feet.X, feet.Y - MathF.Round((bob + rest) / pixel) * pixel);
        var scale = new Vector2(ImportedFigure.Texel, ImportedFigure.Texel * stretch);
        if (body.Rig.Count > 0)
        {
            DrawPieces(spriteBatch, body, at, rotation, scale, stride, colour, layerDepth, fall);
            return;
        }
        var origin = new Vector2(left ? body.Picture.Width - body.Feet.X : body.Feet.X, body.Feet.Y);
        spriteBatch.Draw(body.Picture, at, null, colour, rotation, origin, scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
    }

    /// <summary>
    /// A figure from parts, piece by piece: each piece's socket goes where the whole figure's lean,
    /// stretch and mirroring carry it, and the piece turns about it by its own motion (Turn) on top.
    /// Then the cast glow in the near hand.
    /// </summary>
    private void DrawPieces(SpriteBatch spriteBatch, ImportedFigure body, Vector2 at, float rotation, Vector2 scale, float stride, Color colour, float layerDepth, float fall)
    {
        bool left = Facing == Direction.Left;
        float sign = left ? -1f : 1f, cos = MathF.Cos(rotation), sin = MathF.Sin(rotation);
        Vector2 Place(Vector2 onPicture)
        {
            Vector2 d = (onPicture - body.Feet) * new Vector2(sign, 1f) * scale;
            return at + new Vector2(d.X * cos - d.Y * sin, d.X * sin + d.Y * cos);
        }

        IReadOnlyList<ImportedFigure.Piece> pieces = body.Rig;
        for (int i = 0; i < pieces.Count; i++)
        {
            ImportedFigure.Piece piece = pieces[i];
            Vector2 origin = piece.Pivot - piece.Offset;
            if (left) origin.X = piece.Picture.Width - origin.X;
            float turn = sign * Turn(piece.Bone, stride) * (1f - fall);
            float depth = MathF.Min(1f, layerDepth + (pieces.Count - 1 - i) * PieceStep);
            spriteBatch.Draw(piece.Picture, Place(piece.Pivot), null, colour, rotation + turn, origin, scale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, depth);
        }

        if (_glow <= 0.01f || body.Hand is not Vector2 near) return;
        float size = body.Size.Y * CastGlowSize * (0.85f + 0.15f * MathF.Sin(_breath * 9f));
        spriteBatch.Draw(_shadow, Place(near), null, CastGlow * (_glow * 0.75f * Opacity), 0f, new Vector2(_shadow.Width, _shadow.Height) / 2f,
            size / _shadow.Width, SpriteEffects.None, MathF.Max(0f, layerDepth - PieceStep));
    }

    /// <summary>How far a piece turns about its socket now, toward the way the figure faces: its pose's, eased, and its breath, stride or sway.</summary>
    private float Turn(FigureBone bone, float stride)
    {
        float breath = _breath / BreathSeconds * MathHelper.TwoPi;
        float walking = IsWalking ? 1f : 0f;
        return bone switch
        {
            FigureBone.Head => _nod + HeadNod * MathF.Sin(breath) - walking * HeadCounter * MathF.Sin(stride),
            FigureBone.Near => _swing + HandSway * MathF.Sin(breath + 1f) + walking * HandSwing * MathF.Sin(stride),
            FigureBone.Far => _brace - walking * FarSwing * MathF.Sin(stride),
            FigureBone.Back => _kick + CapeSway * MathF.Sin(_cape / CapeSeconds * MathHelper.TwoPi)
                + walking * (CapeTrail + CapeFlutter * MathF.Sin(stride * 2f)),
            _ => 0f,
        };
    }

    /// <summary>Breathes, and eases an imported picture's lean and stretch toward its pose's, and down to lying when it's Downed or back up when it isn't.</summary>
    /// <param name="seconds">How long since the last update.</param>
    private void Settle(float seconds)
    {
        _breath = (_breath + seconds) % BreathSeconds;
        _cape = (_cape + seconds) % CapeSeconds;
        float toward = 1f - MathF.Exp(-PoseRate * seconds);
        _lean += (LeanOf(_pose) - _lean) * toward;
        (float nod, float swing, float brace, float kick) = PieceTurns(_pose);
        _nod += (nod - _nod) * toward;
        _swing += (swing - _swing) * toward;
        _brace += (brace - _brace) * toward;
        _kick += (kick - _kick) * toward;
        _glow += ((_pose == WalkerPose.Cast ? 1f : 0f) - _glow) * toward;
        _stretch += ((_pose == WalkerPose.Cast ? CastStretch : 0f) - _stretch) * toward;
        _lying += ((_pose == WalkerPose.Downed ? 1f : 0f) - _lying) * (1f - MathF.Exp(-FallRate * seconds));
    }

    /// <summary>How far a figure from parts turns its head, its near hand, its far hand and its cape for a pose, in radians toward the way it faces.</summary>
    private static (float Nod, float Swing, float Brace, float Kick) PieceTurns(WalkerPose pose) => pose switch
    {
        WalkerPose.WindUp => (-0.04f, -0.75f, 0.05f, -0.05f),
        WalkerPose.Strike => (0.07f, 0.55f, -0.1f, 0.12f),
        WalkerPose.Cast => (-0.12f, -0.45f, 0f, -0.04f),
        WalkerPose.Hurt => (-0.16f, -0.15f, 0.18f, -0.06f),
        _ => (0f, 0f, 0f, 0f),
    };

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
