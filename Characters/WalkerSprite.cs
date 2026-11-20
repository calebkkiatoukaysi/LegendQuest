using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Characters.Motion;
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
/// <para>
/// The real-time fight drives them by moves instead (PlayMove): any clip by name ("auto-two-hand-2",
/// "skill-arcanist-fire-bolt", "hurt", "die"), started again if it's the one playing, firing
/// ClipEvent as playback crosses each of its events ("hit", "release", "land"), so the screen lands
/// the blow on the frame the clip does; then back to "ready" in a fight (InFight) or "idle" out of
/// one. A creature drawn from a sheet plays the same names as squash, stretch, lunge, tint and tip-over
/// on its frames (Motion.CreatureMotion). On top of the clip, each hit is staged on the figure itself
/// (Characters/Motion): a hit-stop that freezes just this figure for a few frames, the victim
/// shaking (Freeze); a white flash in its own shape (FlashWhite, FlashTint); a squash (Squash) and a
/// knock (Knockback); a smear and a fading ribbon behind the weapon through a clip's trail windows
/// (DrawTrail); and a skirt and a cape that swing after the body on springs. Hair is painted into the
/// head piece, so it moves with the head and has no spring of its own.
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
    /// its move (for the weapon in hand), and a cutscene can play any clip by name (PlayClip). In a
    /// fight, standing is the ready stance.
    /// </summary>
    private const string IdleClip = "idle", ReadyClip = "ready", JogClip = "jog", StrollClip = "walk";

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

    /// <summary>
    /// The springs on what hangs (research feel.md §9): the skirt quick and well damped (3 Hz, 0.55),
    /// a cape slower and swingier (2 Hz, 0.4); how far each trails for every world pixel a second the
    /// hips go, in degrees; and the furthest either swings off the body, in degrees.
    /// </summary>
    private const float SkirtHertz = 3f, SkirtDamping = 0.55f, CapeHertz = 2f, CapeDamping = 0.4f;
    private const float SkirtDrag = 0.12f, CapeDrag = 0.18f, MostSwing = 30f;

    /// <summary>A smear: how much longer the weapon is drawn along its blade at the fastest of a trail window, and the turn a second (degrees) that counts as fastest.</summary>
    private const float SmearMost = 0.45f, SmearFull = 1400f;

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
    private Linear2[] _linear;

    /// <summary>The move the fight asked for (PlayMove), whether it's to start again next update, whether it holds its last pose when it ends, and how fast it plays.</summary>
    private Clip _move;
    private bool _moveRestart, _moveHold;
    private float _moveSpeed = 1f;

    /// <summary>A figure that doesn't play clips: how far into its move it is, and which kind of move that is to CreatureMotion.</summary>
    private float _moveTime;
    private CreatureMotion.Move _moveKind;

    /// <summary>The hit-stop on this figure alone.</summary>
    private HitStop _hitStop;

    /// <summary>The white flash: how long it holds at full, how long it then fades over, and how long it's been going; and the element's tint over it, its colour, length and clock.</summary>
    private float _whiteHold, _whiteFade, _whiteClock = 1f;
    private Color _tintColour;
    private float _tintLength = 1f, _tintClock = 1f;

    /// <summary>The squash: how much (shorter and wider at its height), how long, and how far through.</summary>
    private float _squashAmount, _squashLength = 1f, _squashClock = 1f;

    /// <summary>The knock: how far, how long it takes, how far through, and whether it comes back or stays (a death's).</summary>
    private Vector2 _knock;
    private float _knockLength = 1f, _knockClock = 1f;
    private bool _knockStays;

    /// <summary>How long a fall's knock takes to come back under the feet as they get up, in seconds.</summary>
    private const float GetUpSeconds = 0.4f;

    /// <summary>The springs on the skirt and the cape, how far each is swung off the body now (degrees), and where the hips were last update across the way the figure faces, for how fast they're going.</summary>
    private readonly Spring _skirt = new(SkirtHertz, SkirtDamping), _cape = new(CapeHertz, CapeDamping);
    private float _skirtSwing, _capeSwing, _lastHips;
    private bool _hipsKnown;

    /// <summary>The ribbon and ghosts behind the weapon, and room to work out the skeleton between frames for it; and how hard the weapon's smeared now.</summary>
    private readonly WeaponTrail _trail = new();
    private BonePose[] _subPose;
    private float[] _subTurns;
    private Vector2[] _subAt;
    private Linear2[] _subLinear;
    private float _smear;

    /// <summary>Where and how the figure was last placed on the map, for finding a bone on it: its feet, which way it faces, and the squash.</summary>
    private Vector2 _placedFeet;
    private float _placedSign = 1f;
    private Linear2 _placedSquash = Linear2.Identity;

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

    /// <summary>Whether anyone hit flashes white (FlashWhite, FlashTint): the options' hit flashes. On to start with.</summary>
    public static bool Flashes { get; set; } = true;

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

    /// <summary>Fires as playback crosses each event of the clip playing ("hit", "release", "land", "telegraph", "recover", "fx", "step"), once each time round a loop; never while the figure's frozen in a hit-stop, since its clip isn't moving.</summary>
    public event Action<ClipEvent> ClipEvent;

    /// <summary>
    /// The kind of weapon they swing (one-hand, two-hand, polearm, dagger, unarmed, hook, staff, bow,
    /// crossbow), as the engine works it out from what's in their hand; null to guess from the figure
    /// (one-hand if it holds anything, unarmed if not).
    /// </summary>
    public string WeaponKind { get; set; }

    /// <summary>The kind of weapon they swing: WeaponKind, or the guess from the figure.</summary>
    public string Weapon => WeaponKind ?? (Body is ImportedFigure body && HoldsWeapon(body) ? "one-hand" : "unarmed");

    /// <summary>Whether they're in a fight: standing, they hold the ready stance (the "ready" clip, breathing) rather than the town's idle.</summary>
    public bool InFight { get; set; }

    /// <summary>The move the fight asked for that's playing, or null.</summary>
    public Clip Move => _move;

    /// <summary>How far into the move playing they are, in seconds of the clip.</summary>
    public float MoveTime => PlaysClips ? (_player?.Clip == _move ? _player.Time : 0f) : _moveTime;

    /// <summary>How fast the move playing goes against its own time: the fight nudges it a little faster for a moment when a hit-stop has left it behind the blow it's timed to.</summary>
    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = MathF.Max(0.05f, value);
    }

    /// <summary>Whether they're frozen in a hit-stop now.</summary>
    public bool Frozen => _hitStop.Active;

    /// <summary>The colour of the ribbon behind their weapon (not premultiplied): a warm white unless it's lit with something.</summary>
    public Color TrailColour
    {
        get => _trail.Colour;
        set => _trail.Colour = value;
    }

    /// <summary>Which way the weapon's tip was last going, in world pixels a sample, or zero if it isn't trailing: for turning sparks along the swing.</summary>
    public Vector2 SwingDirection
    {
        get
        {
            IReadOnlyList<WeaponTrail.Sample> all = _trail.All;
            return all.Count < 2 ? Vector2.Zero : all[^1].Tip - all[^2].Tip;
        }
    }

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
        _move = null;
        _pose = pose;
        _poseHeld = seconds <= 0f;
        _poseLeft = seconds;
    }

    /// <summary>Lets go of whatever pose or move they're in, back to standing or walking, on their own feet: a death's knock that stayed out is gone.</summary>
    public void Release()
    {
        Play(WalkerPose.None);
        _knockStays = false;
        _knockClock = _knockLength;
    }

    /// <summary>Brings a knock that stayed out (a fall) back under the feet over a moment, for someone getting up.</summary>
    private void RecoverKnock()
    {
        if (!_knockStays) return;
        _knockStays = false;
        // Out and held is the curve's first fifth; the rest of it eases back.
        _knockLength = GetUpSeconds / 0.8f;
        _knockClock = _knockLength * 0.2f;
    }

    /// <summary>Whether they're a figure from parts that plays clips.</summary>
    public bool PlaysClips => Body is { Joints: not null } && Skeleton.Current is not null;

    /// <summary>Plays a clip by name (ClipLibrary) until another pose or Release: one that doesn't loop holds its last pose.</summary>
    /// <param name="name">The clip's name, or another name for it.</param>
    /// <returns>How long it takes, in seconds; 0 if they don't play clips or there's no such clip.</returns>
    public float PlayClip(string name)
    {
        if (!PlaysClips || !ClipLibrary.TryGet(name, Body.Base, out Clip clip)) return 0f;
        _pose = WalkerPose.None;
        _move = null;
        _gesture = clip;
        return clip.Length;
    }

    /// <summary>
    /// Plays a fighting move by its clip's name: an auto attack ("auto-polearm-2", ClipLibrary.AutoName),
    /// a skill ("skill-warden-shield-bash"), a reaction ("hurt", "block", "knockdown", "die"), a
    /// telegraph's held coil. It blends in over the clip's own Blend, fires ClipEvent as it crosses each
    /// event, and when a clip that doesn't loop ends they go back to standing (the ready stance in a
    /// fight), unless it's to hold its last pose (a death, a knockdown until the get-up). A creature
    /// drawn from a sheet plays the same names as procedural motion on its frames, with the same
    /// events. A clip the library hasn't got falls back to the nearest it has (an auto attack to the
    /// weapon's strike), so the fight never stands still for a missing clip.
    /// </summary>
    /// <param name="name">The clip's name.</param>
    /// <param name="restart">Whether to start it from the beginning if it's the one playing (a second blow of the same swing).</param>
    /// <param name="speed">How fast it plays against its own time (haste).</param>
    /// <param name="hold">Whether to hold its last pose when it ends, until the next move or Release.</param>
    /// <returns>The clip playing (its length and its events' times, divided by speed, are when things happen), or null for a figure with neither clips nor a sheet.</returns>
    public Clip PlayMove(string name, bool restart = true, float speed = 1f, bool hold = false)
    {
        Clip clip = PlaysClips ? ClipLibrary.Get(name, Body.Base) ?? Fallback(name) : Sheet is not null || Body is not null ? CreatureMotion.ClipFor(name) : null;
        if (name is "getup" or "revive" or "stand") RecoverKnock();
        if (clip is null) return null;
        _gesture = null;
        _pose = WalkerPose.None;
        if (clip != _move || restart)
        {
            _moveRestart = true;
            _moveTime = 0f;
            _moveKind = CreatureMotion.Kind(name);
        }
        _move = clip;
        _moveHold = hold;
        _moveSpeed = MathF.Max(0.05f, speed);
        return clip;
    }

    /// <summary>Stops the move playing, back to standing (or the ready stance).</summary>
    public void StopMove() => _move = null;

    /// <summary>The clip played for a move the library hasn't got: an auto attack or a skill as the weapon's strike, a telegraph as its wind-up, a knockdown as a fall, a get-up as standing, a victory as the one-handed one.</summary>
    private Clip Fallback(string name)
    {
        string weapon = Weapon;
        Clip Strike() => ClipLibrary.ForMove("strike", weapon) ?? ClipLibrary.Get("strike-one-hand");
        if (name.StartsWith("auto-", StringComparison.Ordinal) || name.StartsWith("skill-", StringComparison.Ordinal) || name.StartsWith("foe-", StringComparison.Ordinal))
            return Strike();
        if (name.StartsWith("telegraph", StringComparison.Ordinal)) return ClipLibrary.ForMove("windup", weapon);
        if (name.StartsWith("victory", StringComparison.Ordinal)) return ClipLibrary.Get("victory-one-hand");
        return name switch
        {
            "knockdown" => ClipLibrary.Get("fall") ?? ClipLibrary.Get("downed"),
            "getup" => ClipLibrary.Get("stand") ?? ClipLibrary.Get("revive"),
            "ready" => ClipLibrary.Get(IdleClip, Body.Base),
            _ => null,
        };
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

    /// <summary>Freezes this figure alone for a hit-stop: its clip, nudges, squash, knock and springs stand still for a number of frames while the world goes on. The victim shakes a pixel or two across (up and down if it's in the air), tapering; the attacker creeps on.</summary>
    /// <param name="frames">How many frames, at 60 a second (Feel.HitStopFrames).</param>
    /// <param name="role">Which side of the blow it's on.</param>
    /// <param name="shake">How far the victim shakes each way at first, in world pixels.</param>
    /// <param name="airborne">Whether it's in the air, so it shakes up and down.</param>
    public void Freeze(int frames, HitStopRole role, float shake = 1.5f, bool airborne = false) =>
        _hitStop.Start(frames, role, shake, airborne ? Vector2.UnitY : Vector2.UnitX);

    /// <summary>Flashes the figure white in its own shape: full for a number of frames, then fading. Nothing if the options have flashes off.</summary>
    /// <param name="frames">How many frames at full (Feel.FlashFrames, CriticalFlashFrames).</param>
    /// <param name="fadeFrames">How many frames it fades over after.</param>
    public void FlashWhite(int frames = Feel.FlashFrames, int fadeFrames = Feel.FlashFadeFrames)
    {
        if (!Flashes) return;
        _whiteHold = Feel.Seconds(frames);
        _whiteFade = Feel.Seconds(Math.Max(0, fadeFrames));
        _whiteClock = 0f;
        Prepare();
    }

    /// <summary>Washes the figure in a colour (an element's, a heal's green) for a number of frames, fading: after a spell's white flash.</summary>
    /// <param name="colour">The colour.</param>
    /// <param name="frames">How many frames it takes to fade.</param>
    public void FlashTint(Color colour, int frames = Feel.ElementTintFrames)
    {
        if (!Flashes) return;
        _tintColour = colour;
        _tintLength = Feel.Seconds(Math.Max(1, frames));
        _tintClock = 0f;
        Prepare();
    }

    /// <summary>Squashes the figure about its feet, keeping its volume: shorter by the amount and wider to match at once, springing back over a number of frames. A negative amount stretches it taller.</summary>
    /// <param name="amount">How much shorter at first (0.15 is 85% of its height).</param>
    /// <param name="frames">How many frames it takes to spring back.</param>
    public void Squash(float amount, int frames = Feel.SquashFrames)
    {
        _squashAmount = MathHelper.Clamp(amount, -0.5f, 0.5f);
        _squashLength = Feel.Seconds(Math.Max(1, frames));
        _squashClock = 0f;
    }

    /// <summary>Knocks the figure along the floor: out fast, then back (a blow), or out and staying there (a kill's pop, a death's slide).</summary>
    /// <param name="by">How far, in world pixels (Feel.Knock).</param>
    /// <param name="seconds">How long the whole of it takes.</param>
    /// <param name="stays">Whether it stays out rather than coming back.</param>
    public void Knockback(Vector2 by, float seconds = Feel.KnockSeconds, bool stays = false)
    {
        _knock = by;
        _knockLength = MathF.Max(seconds, float.Epsilon);
        _knockClock = 0f;
        _knockStays = stays;
    }

    /// <summary>
    /// Takes a blow, staged as the feel spec has it: both fighters frozen (Feel.HitStopFrames), this
    /// one shaking; a white flash for 2 frames (4 on a critical), then the element's tint if it's a
    /// spell; a squash; and a knock away from the attacker. The flinch clip is the screen's to play.
    /// </summary>
    /// <param name="attacker">Who struck, frozen too (creeping); or null for a blow from nowhere (a trap, a tick).</param>
    /// <param name="damage">How much it did.</param>
    /// <param name="most">The most health this figure's fighter can have.</param>
    /// <param name="critical">Whether it was a critical.</param>
    /// <param name="kill">Whether it killed or downed.</param>
    /// <param name="heavy">Whether it was a heavy skill rather than an auto attack.</param>
    /// <param name="element">The spell's colour, to wash the figure in after the white; null for a weapon.</param>
    /// <returns>How many frames the hit-stop is.</returns>
    public int TakeHit(WalkerSprite attacker, int damage, int most, bool critical, bool kill, bool heavy, Color? element = null)
    {
        int frames = Feel.HitStopFrames(damage, most, critical, kill);
        Freeze(frames, HitStopRole.Victim, heavy || critical ? 2f : 1.25f);
        attacker?.Freeze(frames, HitStopRole.Attacker);
        FlashWhite(critical || kill ? Feel.CriticalFlashFrames : Feel.FlashFrames);
        if (element is Color tint) FlashTint(tint);
        Squash(heavy || critical || kill ? Feel.HeavySquash : Feel.LightSquash);
        float away = attacker is null ? (Facing == Direction.Left ? 1f : -1f) : MathF.Sign(Position.X - attacker.Position.X);
        if (away == 0f) away = Facing == Direction.Left ? 1f : -1f;
        Knockback(new Vector2(away * Feel.Knock(heavy, critical, kill), 0f), kill ? 0.3f : Feel.KnockSeconds, kill);
        return frames;
    }

    /// <summary>Makes the white copies of the figure's pictures now, so the first flash doesn't wait on them.</summary>
    private void Prepare()
    {
        if (Body is ImportedFigure body)
        {
            foreach (ImportedFigure.Piece piece in body.Rig) Silhouettes.Of(piece.Picture);
            if (body.Rig.Count == 0) Silhouettes.Of(body.Picture);
        }
        else Silhouettes.Of(Sheet);
    }

    /// <summary>Runs the walk cycle, or stands still, runs the clocks on the pose, the nudge and the flash, and eases an imported picture toward its pose. A hit-stop holds everything of the figure's own but the flash and its walk.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime) => Update((float)gameTime.ElapsedGameTime.TotalSeconds);

    /// <summary>Runs the walk cycle, or stands still, runs the clocks on the pose, the nudge and the flash, and eases an imported picture toward its pose.</summary>
    /// <param name="seconds">Seconds since the last update.</param>
    public void Update(float seconds)
    {
        // The figure's own clock: frozen (or creeping) through a hit-stop.
        float own = _hitStop.Scale(seconds);
        _nudgeClock = MathF.Min(_nudgeLength, _nudgeClock + own);
        _flashClock = MathF.Min(_flashLength, _flashClock + seconds);
        _whiteClock = MathF.Min(_whiteHold + _whiteFade + 1f, _whiteClock + seconds);
        _tintClock = MathF.Min(_tintLength, _tintClock + seconds);
        _squashClock = MathF.Min(_squashLength, _squashClock + own);
        _knockClock = MathF.Min(_knockLength, _knockClock + own);
        if (_pose != WalkerPose.None && !_poseHeld)
        {
            _poseLeft -= own;
            if (_poseLeft <= 0f) _pose = WalkerPose.None;
        }
        Measure(seconds);
        Settle(own);
        Animate(own);
        Swing(own);
        _trail.Update(own);

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

    /// <summary>Where the figure is drawn off where it stands: the nudge, the knock and the hit-stop's shake, in world pixels.</summary>
    private Vector2 Displacement
    {
        get
        {
            Vector2 knock = Vector2.Zero;
            if (_knockClock < _knockLength || _knockStays)
            {
                float u = MathHelper.Clamp(_knockClock / _knockLength, 0f, 1f);
                // Out in the first fifth with an ease-out, then (unless it stays) back smoothly.
                float o = u < 0.2f ? 1f - (1f - u / 0.2f) * (1f - u / 0.2f) : _knockStays ? 1f : 1f - SmoothStep((u - 0.2f) / 0.8f);
                knock = _knock * o;
            }
            return _nudge * MathF.Sin(MathHelper.Pi * _nudgeClock / _nudgeLength) + knock + _hitStop.Offset;
        }
    }

    /// <summary>The squash now, about the feet: how much wider and how much taller.</summary>
    private Vector2 SquashNow
    {
        get
        {
            if (_squashClock >= _squashLength || _squashAmount == 0f) return Vector2.One;
            float u = _squashClock / _squashLength;
            // Springing back: a little past where it was, then settled.
            float s = _squashAmount * MathF.Exp(-3.5f * u) * MathF.Cos(u * MathF.PI * 1.5f);
            float tall = 1f - s;
            return new Vector2(1f / MathF.Max(0.2f, tall), tall);
        }
    }

    /// <summary>How strong the flash over the figure is now: the white's, and the tint's colour at its strength (premultiplied, both together).</summary>
    private Color Overlay
    {
        get
        {
            float white = 0f;
            if (_whiteClock < _whiteHold) white = 1f;
            else if (_whiteFade > 0f && _whiteClock < _whiteHold + _whiteFade) white = 1f - (_whiteClock - _whiteHold) / _whiteFade;
            float tint = _tintClock < _tintLength ? 0.7f * (1f - _tintClock / _tintLength) : 0f;
            if (white <= 0f && tint <= 0f) return Color.Transparent;
            white *= 0.65f;
            Vector4 c = Vector4.One * white + _tintColour.ToVector4() * tint * (1f - white);
            return new Color(Vector4.Min(c, Vector4.One));
        }
    }

    /// <summary>Draws the shadow and the sprite at Position, nudged, flashed and faded as they are: a frame off the sheet, or the imported picture.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="layerDepth">Where it sorts. The map works this out from how far down it the feet are.</param>
    /// <param name="pixel">How big one screen pixel is in world pixels. The feet snap to it so the sprite doesn't crawl.</param>
    /// <param name="tint">What to multiply the sprite by: the map's tint. Not the shadow, which is black whatever the light.</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth, float pixel, Color tint)
    {
        if (Sheet is null && Body is null || Opacity <= 0f) return;

        // A figure that doesn't play clips is posed by its move, if it has one.
        CreaturePose posed = PlaysClips ? CreaturePose.Rest : CreaturePoseNow();
        float sign = Facing == Direction.Left ? -1f : 1f;

        // Between screen pixels, the art pixels would come out two and four screen pixels wide by turns.
        Vector2 at = Position + Displacement + new Vector2(posed.Offset.X * sign, posed.Offset.Y);
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
        float opacity = Opacity * posed.Alpha;
        spriteBatch.Draw(_shadow, shadow, null, ShadowColor * opacity, 0f, Vector2.Zero, SpriteEffects.None, MathF.Min(1f, layerDepth + 0.0001f));

        float flash = 1f - _flashClock / _flashLength;
        Color colour = Color.Lerp(tint, new Color(tint.ToVector4() * FlashColour.ToVector4()), flash * flash) * opacity;
        Color overlay = Overlay;
        if (posed.Tint.A > 0) overlay = new Color(Vector4.Min(overlay.ToVector4() + posed.Tint.ToVector4(), Vector4.One));
        overlay *= opacity;
        if (Body is not null)
        {
            DrawArt(spriteBatch, feet, pixel, colour, overlay, layerDepth, fall, posed);
            return;
        }

        Point frame = Shape.Frame;
        int column = posed.Column >= 0 && HasPoses ? posed.Column : _pose != WalkerPose.None && HasPoses ? WalkFrames + (int)_pose : _frame;
        var source = new Rectangle(column * frame.X, (int)Facing * frame.Y, frame.X, frame.Y);
        Vector2 squash = SquashNow * posed.Scale;
        float lean = posed.Lean * sign;
        spriteBatch.Draw(Sheet, feet, source, colour, lean, Shape.Feet, squash, SpriteEffects.None, layerDepth);
        if (overlay.A > 0 && Silhouettes.Of(Sheet) is Texture2D white)
            spriteBatch.Draw(white, feet, source, overlay, lean, Shape.Feet, squash, SpriteEffects.None, MathF.Max(0f, layerDepth - PieceStep / 2f));
    }

    /// <summary>A figure that doesn't play clips, posed by its move now (CreatureMotion), or breathing if it's standing still with none.</summary>
    private CreaturePose CreaturePoseNow()
    {
        if (_move is not null) return CreatureMotion.Pose(_moveKind, _moveTime, _move.Length, _breath);
        if (_pose == WalkerPose.None && !IsWalking) return CreatureMotion.Pose(CreatureMotion.Move.Idle, 0f, 1f, _breath);
        return CreaturePose.Rest;
    }

    /// <summary>Draws the imported picture at the feet: mirrored facing left, breathing, bobbing and leaning into the walk, leaning into its pose, and lying back along the floor as it falls.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="feet">Where the feet are, snapped to the screen's pixels.</param>
    /// <param name="pixel">How big one screen pixel is in world pixels, for snapping the bob and the lift.</param>
    /// <param name="colour">The tint, the flash and the fade together.</param>
    /// <param name="overlay">The white flash and any tint, premultiplied, faded with the figure.</param>
    /// <param name="layerDepth">Where it sorts.</param>
    /// <param name="fall">How far it's lying down, eased: 0 standing, 1 flat.</param>
    /// <param name="posed">A move's pose, for a picture that doesn't play clips.</param>
    private void DrawArt(SpriteBatch spriteBatch, Vector2 feet, float pixel, Color colour, Color overlay, float layerDepth, float fall, CreaturePose posed)
    {
        bool left = Facing == Direction.Left;
        float stride = _clock / (StepTime * WalkFrames) * MathHelper.TwoPi;
        float bob = IsWalking ? WalkBob * MathF.Abs(MathF.Sin(stride)) : 0f;
        float lean = _lean + posed.Lean + (IsWalking ? WalkLean + WalkSway * MathF.Sin(stride) : 0f);
        float stretch = MathHelper.Lerp(1f + _stretch + Breath * MathF.Sin(_breath / BreathSeconds * MathHelper.TwoPi), 1f, fall);
        float rotation = (left ? -1f : 1f) * MathHelper.Lerp(lean, -MathHelper.PiOver2, fall);
        ImportedFigure body = Body;
        float rest = fall * MathF.Min(body.Feet.X, body.Picture.Height * LyingRest) * ImportedFigure.Texel;

        if (PlaysClips && body.Rig.Count > 0)
        {
            DrawRigged(spriteBatch, body, feet, new Vector2(ImportedFigure.Texel), colour, overlay, layerDepth);
            return;
        }
        var at = new Vector2(feet.X, feet.Y - MathF.Round((bob + rest) / pixel) * pixel);
        Vector2 scale = new Vector2(ImportedFigure.Texel, ImportedFigure.Texel * stretch) * SquashNow * posed.Scale;
        var origin = new Vector2(left ? body.Picture.Width - body.Feet.X : body.Feet.X, body.Feet.Y);
        SpriteEffects flip = left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        spriteBatch.Draw(body.Picture, at, null, colour, rotation, origin, scale, flip, layerDepth);
        if (overlay.A > 0 && Silhouettes.Of(body.Picture) is Texture2D white)
            spriteBatch.Draw(white, at, null, overlay, rotation, origin, scale, flip, MathF.Max(0f, layerDepth - PieceStep / 2f));
    }

    /// <summary>
    /// A figure from parts, piece by piece, as its clip has it now: the skeleton worked out
    /// (Skeleton.Solve), each piece drawn turned and stretched with its bone about where its joint has
    /// gone, mirrored facing left, back to front, with what the clip brings in front of the chest after
    /// the head; the whole squashed about the feet; the skirt and cape swung by their springs; the
    /// weapon smeared and its ghosts behind it through a trail window; the flash over each piece in
    /// its own shape. Then the cast glow in the near hand.
    /// </summary>
    private void DrawRigged(SpriteBatch spriteBatch, ImportedFigure body, Vector2 at, Vector2 scale, Color colour, Color overlay, float layerDepth)
    {
        Skeleton skeleton = Skeleton.Current;
        if (_player is null || _joints is null || _joints.Length != skeleton.Count) Animate(0f);
        bool left = Facing == Direction.Left;
        float sign = left ? -1f : 1f;
        FillJoints(body, skeleton);
        skeleton.Solve(_player.Pose, _joints, body.RigPx, _turns, _at, _linear);
        Vector2 squash = SquashNow;
        Linear2 global = Linear2.Stretch(squash.X, squash.Y);
        bool squashed = squash != Vector2.One;
        _placedFeet = at;
        _placedSign = sign;
        _placedSquash = global;
        Vector2 Place(Vector2 onPicture) => at + global.Apply(onPicture - body.Feet) * new Vector2(sign, 1f) * scale;

        // Back to front, what the clip brings in front of the chest moved to just after the head.
        _order.Clear();
        IReadOnlyList<string> front = _player.Clip?.Front ?? Array.Empty<string>();
        bool Behind(ImportedFigure.Piece p) => p.Key == RigComposer.Weapon || _player.Clip?.BehindHead(p.Bone, _player.Time) == true ||
            (_player.Clip is Clip c && Contains(c.BeforeHead, p.Key));
        int afterHead = -1;
        // Running, the near foot is its side-on picture (the clip swaps it); else its own.
        bool sideFoot = _player.Clip?.PartOf("foot_near", _player.Time) == "side" && HasPiece(body, RigComposer.SideFoot);
        foreach (ImportedFigure.Piece piece in body.Rig)
        {
            if (piece.Key == (sideFoot ? "foot_near" : RigComposer.SideFoot)) continue;
            if (Behind(piece) || (front.Count > 0 && Contains(front, piece.Bone))) continue;
            _order.Add(piece);
            if (piece.Key == "head") afterHead = _order.Count;
        }
        if (front.Count > 0)
        {
            int at0 = afterHead < 0 ? _order.Count : afterHead;
            foreach (ImportedFigure.Piece piece in body.Rig)
                if (piece.Key != (sideFoot ? "foot_near" : RigComposer.SideFoot) && !Behind(piece) && Contains(front, piece.Bone)) _order.Insert(at0++, piece);
        }
        int beforeHead = _order.FindIndex(p => p.Key == "head");
        if (beforeHead < 0) beforeHead = _order.Count;
        foreach (ImportedFigure.Piece piece in body.Rig)
            if (piece.Key != (sideFoot ? "foot_near" : RigComposer.SideFoot) && Behind(piece)) _order.Insert(beforeHead++, piece);

        SpriteEffects flip = left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        for (int k = 0; k < _order.Count; k++)
        {
            ImportedFigure.Piece piece = _order[k];
            int i = skeleton.IndexOf(piece.Bone);
            if (i < 0) continue;
            Vector2 origin = piece.Pivot - piece.Offset;
            Vector2 where = _at[i];
            Linear2 linear = _linear[i];
            bool plain = !squashed && !ScaledAbove(skeleton, i);
            if (piece.Key == "skirt" && _skirtSwing != 0f)
            {
                linear *= Linear2.Turn(MathHelper.ToRadians(_skirtSwing));
                plain = false;
            }
            else if (piece.Key == RigComposer.Cape && _capeSwing != 0f)
            {
                // The cape hangs from the top of its own picture, the middle across, and swings about there.
                Vector2 hang = piece.Offset + new Vector2(piece.Picture.Width / 2f, 0f);
                where = _at[i] + _linear[i].Apply(hang - piece.Pivot);
                origin = hang - piece.Offset;
                linear *= Linear2.Turn(MathHelper.ToRadians(_capeSwing));
                plain = false;
            }
            else if (piece.Key == RigComposer.Weapon && _smear > 0f)
            {
                Vector2 axis = WeaponShape.Of(piece.Picture, origin).Axis;
                float s = _smear;
                linear *= new Linear2(1f + s * axis.X * axis.X, s * axis.X * axis.Y, s * axis.X * axis.Y, 1f + s * axis.Y * axis.Y);
                plain = false;
            }

            float turn;
            Vector2 stretch;
            if (plain)
            {
                BonePose own = _player.Pose[i];
                turn = _turns[i];
                stretch = new Vector2(own.Sx, own.Sy);
            }
            else (global * linear).Decompose(out turn, out stretch);

            if (left) origin.X = piece.Picture.Width - origin.X;
            float depth = MathF.Min(1f, layerDepth + (_order.Count - 1 - k) * PieceStep);
            Vector2 placed = Place(where);
            spriteBatch.Draw(piece.Picture, placed, null, colour, sign * turn, origin, scale * stretch, flip, depth);
            if (overlay.A > 0 && Silhouettes.Of(piece.Picture) is Texture2D white)
                spriteBatch.Draw(white, placed, null, overlay, sign * turn, origin, scale * stretch, flip, MathF.Max(0f, depth - PieceStep / 2f));

            // The weapon's ghosts, older and fainter, just behind it.
            if (piece.Key == RigComposer.Weapon && _trail.Live)
            {
                IReadOnlyList<WeaponTrail.Sample> all = _trail.All;
                int drawn = 0;
                for (int g = all.Count - 2; g >= 0 && drawn < WeaponTrail.Ghosts; g -= 2, drawn++)
                {
                    WeaponTrail.Sample ghost = all[g];
                    float strength = WeaponTrail.GhostAt(drawn) * (1f - ghost.Age / WeaponTrail.Life);
                    if (strength <= 0.01f) continue;
                    spriteBatch.Draw(piece.Picture, ghost.At, null, colour * strength, ghost.Turn, origin, ghost.Scale, flip, MathF.Min(1f, depth + PieceStep / 2f));
                }
            }
        }

        if (_glow <= 0.01f) return;
        int hand = skeleton.IndexOf("hand_near");
        if (hand < 0) return;
        float size = body.Size.Y * CastGlowSize * (0.85f + 0.15f * MathF.Sin(_breath * 9f));
        spriteBatch.Draw(_shadow, Place(_at[hand]), null, CastGlow * (_glow * 0.75f * Opacity), 0f, new Vector2(_shadow.Width, _shadow.Height) / 2f,
            size / _shadow.Width, SpriteEffects.None, MathF.Max(0f, layerDepth - PieceStep));
    }

    /// <summary>Draws the ribbon the weapon's leaving behind it, if it's leaving one. Between Begin and End of a ribbon batch with the camera's matrix, after the room's batch.</summary>
    /// <param name="batch">The ribbon batch.</param>
    public void DrawTrail(RibbonBatch batch)
    {
        if (_trail.Live && Opacity > 0f) _trail.Draw(batch, Opacity);
    }

    /// <summary>Where a bone's joint was last drawn, in world pixels (hand_near for a shot let go, head for a number); for a figure that doesn't play clips, or a bone it hasn't, a point up its middle as high as the bone would be.</summary>
    /// <param name="bone">The bone's name.</param>
    public Vector2 BonePosition(string bone)
    {
        Skeleton skeleton = Skeleton.Current;
        int i = skeleton?.IndexOf(bone) ?? -1;
        if (!PlaysClips || i < 0 || _at is null || Body is not ImportedFigure body)
        {
            float share = bone switch
            {
                "head" => 0.85f,
                "hand_near" or "hand_far" => 0.5f,
                "root" or "foot_near" or "foot_far" => 0f,
                _ => 0.55f,
            };
            return Position + Displacement - new Vector2(0f, Size.Y * share);
        }
        return _placedFeet + _placedSquash.Apply(_at[i] - body.Feet) * new Vector2(_placedSign, 1f) * ImportedFigure.Texel;
    }

    /// <summary>Where the weapon's tip was last drawn, in world pixels; the near hand if they hold nothing.</summary>
    public Vector2 WeaponTip
    {
        get
        {
            if (!PlaysClips || Body is not ImportedFigure body || _at is null) return BonePosition("hand_near");
            foreach (ImportedFigure.Piece piece in body.Rig)
            {
                if (piece.Key != RigComposer.Weapon) continue;
                int i = Skeleton.Current.IndexOf(piece.Bone);
                WeaponShape shape = WeaponShape.Of(piece.Picture, piece.Pivot - piece.Offset);
                Vector2 onPicture = _at[i] + _linear[i].Apply(shape.Tip + piece.Offset - piece.Pivot);
                return _placedFeet + _placedSquash.Apply(onPicture - body.Feet) * new Vector2(_placedSign, 1f) * ImportedFigure.Texel;
            }
            return BonePosition("hand_near");
        }
    }

    /// <summary>Whether any bone above one in the skeleton is stretched, so its piece is drawn by its whole transform rather than its own turn and stretch.</summary>
    private bool ScaledAbove(Skeleton skeleton, int bone)
    {
        for (int p = skeleton.Parents[bone]; p >= 0; p = skeleton.Parents[p])
            if (_player.Pose[p].Sx != 1f || _player.Pose[p].Sy != 1f) return true;
        return false;
    }

    private void FillJoints(ImportedFigure body, Skeleton skeleton)
    {
        for (int i = 0; i < skeleton.Count; i++) _joints[i] = body.Joints.TryGetValue(skeleton.Names[i], out Vector2 joint) ? joint : body.Feet;
    }

    private static bool HasPiece(ImportedFigure body, string key)
    {
        foreach (ImportedFigure.Piece piece in body.Rig)
            if (piece.Key == key) return true;
        return false;
    }

    private static bool Contains(IReadOnlyList<string> names, string name)
    {
        for (int i = 0; i < names.Count; i++)
            if (names[i] == name) return true;
        return false;
    }

    /// <summary>
    /// For a figure from parts, picks the clip for what they're doing and moves it on: a cutscene's
    /// clip while it holds; else the fight's move; else their fighting pose's move, for the weapon in
    /// hand; else jogging or strolling, at the pace they're going; else standing (the ready stance in a
    /// fight). Fires Stepped as the clip's feet land and ClipEvent as it crosses each event, and
    /// samples the weapon's ribbon through a trail window. A figure that doesn't play clips just runs
    /// its move's clock and fires its events.
    /// </summary>
    /// <param name="seconds">How long since the last update, on the figure's own clock.</param>
    private void Animate(float seconds)
    {
        if (!PlaysClips)
        {
            AnimateMove(seconds);
            return;
        }
        Skeleton skeleton = Skeleton.Current;
        if (_player is null || _joints is null || _joints.Length != skeleton.Count)
        {
            _player = new ClipPlayer(skeleton);
            _turns = new float[skeleton.Count];
            _linear = new Linear2[skeleton.Count];
            _joints = new Vector2[skeleton.Count];
            _at = new Vector2[skeleton.Count];
            _subPose = new BonePose[skeleton.Count];
            _subTurns = new float[skeleton.Count];
            _subAt = new Vector2[skeleton.Count];
            _subLinear = new Linear2[skeleton.Count];
        }
        ImportedFigure body = Body;

        // A move that's played out goes, unless it's to hold.
        if (_move is { Loop: false } && !_moveHold && !_moveRestart && _player.Clip == _move && _player.Finished) _move = null;

        Clip clip = _gesture ?? _move ?? (_pose != WalkerPose.None ? PoseClip(_pose) : null);
        if (clip is null && IsWalking)
        {
            if (_groundSpeed >= JogFrom) _strolling = false;
            else if (_groundSpeed > InPlace && _groundSpeed < StrollUnder) _strolling = true;
            clip = ClipLibrary.Get(_strolling ? StrollClip : JogClip, body.Base);
        }
        clip ??= (InFight ? ClipLibrary.Get(ReadyClip, body.Base) : null) ?? ClipLibrary.Get(IdleClip, body.Base);

        // A clip that goes somewhere plays at the pace they're really going, so the feet stay put.
        float pace = 1f;
        if (clip is { Speed: > 0f } && _groundSpeed > InPlace && body.RigPx > 0f)
            pace = Math.Clamp(_groundSpeed / (clip.Speed * body.RigPx * ImportedFigure.Texel), MinPace, MaxPace);
        if (clip == _move) pace = _moveSpeed;

        bool restart = clip == _move && _moveRestart;
        _moveRestart = false;
        float from = _player.Time;
        if (_player.Play(clip, pace, restart)) from = -1e-4f;
        _player.Update(seconds);
        float now = _player.Time;

        // Each event crossed since the last update, and the ribbon through a trail window.
        if (clip is not null && (seconds > 0f || from < 0f))
        {
            if (ClipEvent is not null) clip.Crossed(from, now, ev => ClipEvent?.Invoke(ev));
            SampleTrail(body, skeleton, clip, from, now, seconds);
        }

        // Each foot that's landed since the last update.
        _clipSteps = clip is not null && clip.Steps.Count > 0;
        if (!_clipSteps || seconds <= 0f) return;
        foreach (float step in clip.Steps)
            if (now >= from ? step > from && step <= now : step > from || step <= now)
                Stepped?.Invoke();
    }

    /// <summary>A figure that doesn't play clips: runs its move's clock, fires the move's events, and lets a move that's played out go (unless it holds).</summary>
    private void AnimateMove(float seconds)
    {
        if (_move is null) return;
        float from = _moveRestart ? -1e-4f : _moveTime;
        _moveRestart = false;
        float to = _moveTime + seconds * _moveSpeed;
        if (_move.Loop) to %= _move.Length;
        else to = MathF.Min(to, _move.Length);
        _moveTime = to;
        Clip move = _move;
        if (ClipEvent is not null && (seconds > 0f || from < 0f)) move.Crossed(from, to, ev => ClipEvent?.Invoke(ev));
        if (!move.Loop && !_moveHold && _move == move && _moveTime >= move.Length) _move = null;
    }

    /// <summary>
    /// Through a clip's trail window, samples the weapon a few times between the last update and this
    /// one (the clip's pose worked out at each, the skeleton solved) and adds each to the ribbon, its
    /// age how long ago in the frame it was; and works out how hard to smear the weapon from how fast
    /// the near hand is turning.
    /// </summary>
    private void SampleTrail(ImportedFigure body, Skeleton skeleton, Clip clip, float from, float now, float seconds)
    {
        _smear = 0f;
        if (clip.Trails.Count == 0 || now < from) return;
        ImportedFigure.Piece weapon = null;
        foreach (ImportedFigure.Piece piece in body.Rig)
            if (piece.Key == RigComposer.Weapon) weapon = piece;
        int hand = weapon is null ? -1 : skeleton.IndexOf(weapon.Bone);
        if (hand < 0) return;

        FillJoints(body, skeleton);
        Vector2 origin = weapon.Pivot - weapon.Offset;
        WeaponShape shape = WeaponShape.Of(weapon.Picture, origin);
        float sign = Facing == Direction.Left ? -1f : 1f;
        Vector2 feet = Position + Displacement;
        Vector2 squash = SquashNow;
        Linear2 global = Linear2.Stretch(squash.X, squash.Y);
        var mirror = new Vector2(sign, 1f);
        Vector2 Place(Vector2 onPicture) => feet + global.Apply(onPicture - body.Feet) * mirror * ImportedFigure.Texel;

        float start = MathF.Max(0f, from);
        float lastTurn = float.NaN;
        for (int j = 1; j <= WeaponTrail.Samples; j++)
        {
            float t = start + (now - start) * j / WeaponTrail.Samples;
            if (!clip.Trailing(t)) continue;
            _player.PoseAt(t, _subPose);
            skeleton.Solve(_subPose, _joints, body.RigPx, _subTurns, _subAt, _subLinear);
            Linear2 linear = _subLinear[hand];
            Vector2 bladeBase = Place(_subAt[hand] + linear.Apply(shape.Base + weapon.Offset - weapon.Pivot));
            Vector2 tip = Place(_subAt[hand] + linear.Apply(shape.Tip + weapon.Offset - weapon.Pivot));
            (global * linear).Decompose(out float turn, out Vector2 stretch);
            float age = seconds * (WeaponTrail.Samples - j) / WeaponTrail.Samples;
            _trail.Add(bladeBase, tip, Place(_subAt[hand]), sign * turn, stretch * ImportedFigure.Texel, age);
            if (!float.IsNaN(lastTurn) && seconds > 0f)
            {
                float rate = MathF.Abs(MathHelper.ToDegrees(_subTurns[hand] - lastTurn)) * WeaponTrail.Samples / seconds;
                _smear = MathF.Max(_smear, SmearMost * MathHelper.Clamp(rate / SmearFull, 0f, 1f));
            }
            lastTurn = _subTurns[hand];
        }
    }

    /// <summary>
    /// Moves the springs on what hangs: the skirt chases the angle the clip gives it and the cape the
    /// torso's, each also dragged back by how fast the hips are going across, and each piece is
    /// swung by however far its spring lags. Nothing moves on a frozen figure.
    /// </summary>
    /// <param name="seconds">How long since the last update, on the figure's own clock.</param>
    private void Swing(float seconds)
    {
        if (!PlaysClips || _player is null || Body is not ImportedFigure body) return;
        bool skirt = HasPiece(body, "skirt"), cape = HasPiece(body, RigComposer.Cape);
        if (!skirt && !cape) return;
        Skeleton skeleton = Skeleton.Current;
        FillJoints(body, skeleton);
        skeleton.Solve(_player.Pose, _joints, body.RigPx, _turns, _at, _linear);

        float sign = Facing == Direction.Left ? -1f : 1f;
        int hipsBone = skeleton.IndexOf("hips");
        float hips = sign * (Position.X + Displacement.X) + (hipsBone >= 0 ? _at[hipsBone].X * ImportedFigure.Texel : 0f);
        float speed = _hipsKnown && seconds > 0f ? (hips - _lastHips) / seconds : 0f;
        if (seconds > 0f || !_hipsKnown)
        {
            _lastHips = hips;
            _hipsKnown = true;
        }
        speed = MathHelper.Clamp(speed, -200f, 200f);

        int skirtBone = skeleton.IndexOf("skirt"), torso = skeleton.IndexOf("torso");
        if (skirt && skirtBone >= 0)
        {
            float clip = MathHelper.ToDegrees(_turns[skirtBone]);
            _skirtSwing = MathHelper.Clamp(_skirt.Update(seconds, clip + SkirtDrag * speed) - clip, -MostSwing, MostSwing);
        }
        if (cape && torso >= 0)
        {
            float body2 = MathHelper.ToDegrees(_turns[torso]);
            _capeSwing = MathHelper.Clamp(_cape.Update(seconds, body2 + CapeDrag * speed) - body2, -MostSwing, MostSwing);
        }
    }

    /// <summary>The clip a fighting pose plays: the wind-up and the strike for the weapon in hand (WeaponKind, or one-handed if they hold one, unarmed if not), the cast, the flinch, or lying down.</summary>
    private Clip PoseClip(WalkerPose pose)
    {
        string weapon = Weapon;
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

    private static float SmoothStep(float u)
    {
        u = MathHelper.Clamp(u, 0f, 1f);
        return u * u * (3f - 2f * u);
    }
}
