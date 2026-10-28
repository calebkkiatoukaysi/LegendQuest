using System;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;
using LegendQuest.World;

namespace LegendQuest.Characters;

/// <summary>
/// Someone standing on a map who isn't in the party: their figure, their feet box, and the line
/// they're saying, if they're saying one.
/// </summary>
/// <remarks>
/// The Black Box's LobbyNpc. Most stand there, look round every few seconds, and turn to face you
/// when you talk to them. The ones the map lets wander stroll off somewhere near where they were put
/// every so often, rest, look round, and go somewhere else; the screen does their walking, so they
/// bump into the same things you do. What they say goes up over their head for a few seconds, unless
/// they've a conversation for you instead.
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

    /// <summary>How fast the ones who wander stroll, in art pixels a second. A good bit slower than you walk.</summary>
    public const float StrollSpeed = 24f;

    /// <summary>About how long they stand about between strolls.</summary>
    private const float RestSeconds = 3f;

    /// <summary>How long they keep on towards somewhere before giving up on it. Something's usually in the way.</summary>
    private const float GiveUpSeconds = 5f;

    /// <summary>The size of their feet box, in world pixels. The same as yours, so nobody walks through them.</summary>
    private static readonly Vector2 FeetSize = new(10f, 4f);

    /// <summary>Which ways they look. Never up, since someone facing the wall looks like a bug.</summary>
    private static readonly Direction[] Glances = { Direction.Down, Direction.Left, Direction.Down, Direction.Right };

    private readonly Random _random;
    private float _glanceClock;
    private float _lineClock;
    private float _restClock;
    private float _tryClock;
    private Vector2? _heading;

    /// <summary>Where they're strolling to, or null if they're standing about.</summary>
    public Vector2? Heading => _heading;

    /// <summary>Who they are, from the map.</summary>
    public MapNpc Definition { get; }

    /// <summary>Their figure.</summary>
    public WalkerSprite Walker { get; } = new();

    /// <summary>Their feet box. Nobody can walk through it.</summary>
    public BoundingRectangle Footprint => new(
        Walker.Position.X - FeetSize.X / 2f, Walker.Position.Y - FeetSize.Y, FeetSize.X, FeetSize.Y);

    /// <summary>The line over their head, or null if they aren't saying anything.</summary>
    public string Line => _lineClock > 0f ? Definition.Say : null;

    /// <summary>How much of the line is showing, 0 to 1, so it fades out at the end.</summary>
    public float LineAlpha => MathHelper.Clamp(_lineClock / LineFadeSeconds, 0f, 1f);

    /// <summary>Puts someone where the map says.</summary>
    /// <param name="definition">Who and where, from the map.</param>
    public Npc(MapNpc definition)
    {
        Definition = definition;
        Walker.Position = definition.Feet;
        Walker.Facing = definition.Facing;

        // Seeded from the id so they act the same every time. Can't use string.GetHashCode, since
        // .NET changes it every run.
        int seed = 17;
        foreach (char c in definition.Id) seed = seed * 31 + c;
        _random = new Random(seed);
        _glanceClock = GlanceSeconds * (0.5f + (float)_random.NextDouble());
        _restClock = RestSeconds * (0.5f + (float)_random.NextDouble());
    }

    /// <summary>Looks round every so often, unless they've just been spoken to, picks somewhere to stroll to if they wander and they've rested, and runs the clock on what they're saying.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _lineClock = MathF.Max(0f, _lineClock - seconds);

        if (_heading is not null)
        {
            _tryClock -= seconds;
            if (_tryClock <= 0f) Arrive();
            Walker.Update(gameTime);
            return;
        }

        if (Definition.Wander > 0f)
        {
            _restClock -= seconds;
            if (_restClock <= 0f)
            {
                float angle = (float)(_random.NextDouble() * MathHelper.TwoPi);
                float distance = Definition.Wander * (0.3f + 0.7f * (float)_random.NextDouble());
                _heading = Definition.Feet + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
                _tryClock = GiveUpSeconds;
            }
        }

        _glanceClock -= seconds;
        if (_glanceClock <= 0f)
        {
            _glanceClock = GlanceSeconds * (0.6f + (float)_random.NextDouble() * 0.8f);
            Walker.Facing = Glances[_random.Next(Glances.Length)];
        }

        Walker.Update(gameTime);
    }

    /// <summary>Stops strolling and stands about for a while: they got where they were going, or gave up on it.</summary>
    public void Arrive()
    {
        _heading = null;
        Walker.IsWalking = false;
        _restClock = RestSeconds * (0.6f + (float)_random.NextDouble() * 0.8f);
    }

    /// <summary>Stops, turns to face whoever's talking to them, and says their line if they're to.</summary>
    /// <param name="speaker">Where the feet of whoever's talking to them are.</param>
    /// <param name="say">Whether to say their line over their head. Not when there's a conversation instead.</param>
    public void TalkTo(Vector2 speaker, bool say)
    {
        Walker.Face(speaker);
        Walker.IsWalking = false;
        _heading = null;
        _glanceClock = AttentionSeconds;
        _restClock = AttentionSeconds;
        if (say && Definition.Say is not null)
            _lineClock = LineSeconds + Definition.Say.Length * LineSecondsPerLetter + LineFadeSeconds;
    }
}
