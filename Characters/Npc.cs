using System;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;
using LegendQuest.World;

namespace LegendQuest.Characters;

/// <summary>
/// Someone standing in a room who isn't in the party: their figure, their feet box, and the line
/// they're saying, if they're saying one.
/// </summary>
/// <remarks>
/// The Black Box's LobbyNpc. Most stand there, glance left and right every few seconds, and turn to
/// face you when you talk to them. The ones the room lets wander stroll off along the floor every so
/// often, mostly sideways and only a little deeper or nearer (the room's RoomWalker.Stroll picks
/// where), rest, glance round, and go somewhere else. A patrol walks from one end of its stretch to
/// the other and back, resting at each end, and turns back early if something's in its way. The
/// screen does their walking, so they bump into the same things you do. What they say goes up over
/// their head for a few seconds, unless they've a conversation for you instead. Someone with more than one line says them in turn, and
/// every so often they're ready to say one by themselves; the screen decides whether they get to.
/// A creature is one of these too, in its own shape: a wolf looks round and faces you the same way.
/// </remarks>
public class Npc
{
    /// <summary>About how long they look one way before turning.</summary>
    private const float GlanceSeconds = 3.2f;

    /// <summary>How long they keep facing you after you've spoken to them, before they go back to looking round.</summary>
    private const float AttentionSeconds = 5f;

    /// <summary>How long a line stays over their head, plus a bit for every letter, and how long it takes to fade.</summary>
    private const float LineSeconds = 2.2f;
    private const float LineSecondsPerLetter = 0.05f;
    private const float LineFadeSeconds = 0.4f;

    /// <summary>The shortest and longest they keep quiet between lines they say by themselves.</summary>
    private const float QuietestSeconds = 9f;
    private const float ChattiestSeconds = 18f;

    /// <summary>How fast the ones who wander stroll, in art pixels a second. A good bit slower than you walk.</summary>
    public const float StrollSpeed = 24f;

    /// <summary>About how long they stand about between strolls.</summary>
    private const float RestSeconds = 3f;

    /// <summary>How long they keep on towards somewhere, past how long it'd take to walk there, before giving up on it. Something's usually in the way.</summary>
    private const float GiveUpSeconds = 5f;

    /// <summary>Which ways they look: along the room, since everyone faces left or right.</summary>
    private static readonly Direction[] Glances = { Direction.Left, Direction.Right };

    private readonly Random _random;
    private readonly RoomWalker _floor;
    private float _glanceClock;
    private float _lineClock;
    private float _restClock;
    private float _tryClock;
    private float _quietClock;
    private Vector2? _heading;

    /// <summary>Which end of their stretch a patrol's walking to, or will walk to next.</summary>
    private bool _patrolRight;

    /// <summary>The line that's up, which of their lines comes next, and whether they came out with it by themselves.</summary>
    private string _line;
    private int _nextLine;
    private bool _unprompted;

    /// <summary>Where they're strolling to, or null if they're standing about.</summary>
    public Vector2? Heading => _heading;

    /// <summary>Whether they walk about, wandering or on patrol, so they're never anywhere long enough to path round.</summary>
    public bool Moves => Definition.Wander > 0f || Definition.Patrol is not null;

    /// <summary>Who they are, from the zone.</summary>
    public MapNpc Definition { get; }

    /// <summary>Their figure, in a person's shape or their creature's.</summary>
    public WalkerSprite Walker { get; }

    /// <summary>Their feet box. Nobody can walk through it.</summary>
    public BoundingRectangle Footprint => FootprintAt(Walker.Position);

    /// <summary>The line over their head, or null if they aren't saying anything.</summary>
    public string Line => _lineClock > 0f ? _line : null;

    /// <summary>How much of the line is showing, 0 to 1, so it fades out at the end.</summary>
    public float LineAlpha => MathHelper.Clamp(_lineClock / LineFadeSeconds, 0f, 1f);

    /// <summary>Whether the line over their head is one they came out with by themselves, not one you asked for.</summary>
    public bool Chattering => Line is not null && _unprompted;

    /// <summary>Whether they've kept quiet long enough to say something by themselves, if they've anything to say and aren't saying it already.</summary>
    public bool ReadyToChatter => _quietClock <= 0f && Definition.Say.Count > 0 && Line is null;

    /// <summary>Puts someone where the zone says, on a room's floor.</summary>
    /// <param name="definition">Who and where, from the zone.</param>
    /// <param name="floor">Walking in the room they're in, which picks where they stroll to.</param>
    public Npc(MapNpc definition, RoomWalker floor)
    {
        Definition = definition;
        _floor = floor;
        Walker = new WalkerSprite
        {
            Shape = definition.Creature is null ? WalkerShape.Person : Creatures.Get(definition.Creature).Shape,
            Position = definition.Feet,
            Facing = definition.Facing,
        };

        // Seeded from the id so they act the same every time. Can't use string.GetHashCode, since
        // .NET changes it every run.
        int seed = 17;
        foreach (char c in definition.Id) seed = seed * 31 + c;
        _random = new Random(seed);
        _glanceClock = GlanceSeconds * (0.5f + (float)_random.NextDouble());
        _restClock = RestSeconds * (0.5f + (float)_random.NextDouble());
        if (definition.Patrol is (float left, float right)) _patrolRight = definition.Feet.X - left < right - definition.Feet.X;
        WaitToChatter();
    }

    /// <summary>Glances round every so often, unless they've just been spoken to, sets off for the other end of their patrol or somewhere to stroll to once they've rested, and runs the clocks on what they're saying and how long they've kept quiet.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _lineClock = MathF.Max(0f, _lineClock - seconds);
        _quietClock = MathF.Max(0f, _quietClock - seconds);

        if (_heading is not null)
        {
            _tryClock -= seconds;
            if (_tryClock <= 0f) Arrive();
            Walker.Update(gameTime);
            return;
        }

        if (Moves)
        {
            _restClock -= seconds;
            if (_restClock <= 0f) SetOff();
        }

        _glanceClock -= seconds;
        if (_glanceClock <= 0f)
        {
            _glanceClock = GlanceSeconds * (0.6f + (float)_random.NextDouble() * 0.8f);
            Walker.Facing = Glances[_random.Next(Glances.Length)];
        }

        Walker.Update(gameTime);
    }

    /// <summary>Where their feet box would be with their feet at a point: a person's is the same size as yours, so nobody walks through them, and a creature's is as broad as it stands.</summary>
    /// <param name="feet">The point, in world pixels.</param>
    public BoundingRectangle FootprintAt(Vector2 feet)
    {
        Vector2 size = Walker.Shape.Footprint;
        return new BoundingRectangle(feet.X - size.X / 2f, feet.Y - size.Y, size.X, size.Y);
    }

    /// <summary>Stops strolling and stands about for a while: they got where they were going, or gave up on it. A patrol goes back the other way next.</summary>
    public void Arrive()
    {
        if (_heading is not null && Definition.Patrol is not null) _patrolRight = !_patrolRight;
        _heading = null;
        Walker.IsWalking = false;
        _restClock = RestSeconds * (0.6f + (float)_random.NextDouble() * 0.8f);
    }

    /// <summary>Sets off: a patrol for the end of its stretch it's facing, anyone else on a stroll mostly along the floor from where they were put.</summary>
    private void SetOff()
    {
        Vector2 home = Definition.Feet;
        if (Definition.Patrol is (float left, float right))
        {
            _heading = new Vector2(_patrolRight ? right : left, home.Y);
        }
        else
        {
            _heading = _floor.Stroll(home, Definition.Wander, _random);
        }
        _tryClock = Vector2.Distance(Walker.Position, _heading.Value) / StrollSpeed + GiveUpSeconds;
    }

    /// <summary>Stops, turns to face whoever's talking to them, and says their next line if they're to.</summary>
    /// <param name="speaker">Where the feet of whoever's talking to them are.</param>
    /// <param name="say">Whether to say their line over their head. Not when there's a conversation instead.</param>
    public void TalkTo(Vector2 speaker, bool say)
    {
        Walker.Face(speaker);
        Walker.IsWalking = false;
        _heading = null;
        _glanceClock = AttentionSeconds;
        _restClock = AttentionSeconds;
        if (say) SayNext(false);
    }

    /// <summary>Says their next line by themselves, without stopping whatever they're doing.</summary>
    public void Chatter() => SayNext(true);

    /// <summary>Keeps quiet a while longer before they're ready to say something by themselves again.</summary>
    public void WaitToChatter() =>
        _quietClock = QuietestSeconds + (float)_random.NextDouble() * (ChattiestSeconds - QuietestSeconds);

    /// <summary>Puts their next line up over their head, and goes back to the first after the last.</summary>
    /// <param name="unprompted">Whether they're saying it by themselves.</param>
    private void SayNext(bool unprompted)
    {
        WaitToChatter();
        if (Definition.Say.Count == 0) return;

        _line = Definition.Say[_nextLine];
        _nextLine = (_nextLine + 1) % Definition.Say.Count;
        _unprompted = unprompted;
        _lineClock = LineSeconds + _line.Length * LineSecondsPerLetter + LineFadeSeconds;
    }
}
