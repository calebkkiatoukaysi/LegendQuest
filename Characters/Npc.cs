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
/// The Black Box's LobbyNpc. They don't walk about: they stand there, look round every few seconds,
/// and turn to face you when you talk to them. What they say goes up over their head for a few
/// seconds; conversations take over from that when there are conversations.
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

    /// <summary>The size of their feet box, in world pixels. The same as yours, so nobody walks through them.</summary>
    private static readonly Vector2 FeetSize = new(10f, 4f);

    /// <summary>Which ways they look. Never up, since someone facing the wall looks like a bug.</summary>
    private static readonly Direction[] Glances = { Direction.Down, Direction.Left, Direction.Down, Direction.Right };

    private readonly Random _random;
    private float _glanceClock;
    private float _lineClock;

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
    }

    /// <summary>Looks round every so often, unless they've just been spoken to, and runs the clock on what they're saying.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _lineClock = MathF.Max(0f, _lineClock - seconds);

        _glanceClock -= seconds;
        if (_glanceClock <= 0f)
        {
            _glanceClock = GlanceSeconds * (0.6f + (float)_random.NextDouble() * 0.8f);
            Walker.Facing = Glances[_random.Next(Glances.Length)];
        }

        Walker.Update(gameTime);
    }

    /// <summary>Turns to face whoever's talking to them and says their line.</summary>
    /// <param name="speaker">Where the feet of whoever's talking to them are.</param>
    public void TalkTo(Vector2 speaker)
    {
        Walker.Face(speaker);
        _glanceClock = AttentionSeconds;
        if (Definition.Say is not null)
            _lineClock = LineSeconds + Definition.Say.Length * LineSecondsPerLetter + LineFadeSeconds;
    }
}
