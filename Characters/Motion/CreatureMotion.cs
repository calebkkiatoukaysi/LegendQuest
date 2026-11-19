using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters.Motion;

/// <summary>How a creature drawn as one picture (a sheet's frame, or a whole imported picture) is posed at a moment of a move: which column of its sheet, and how the picture is moved, stretched, turned, tinted and faded.</summary>
/// <param name="Column">The sheet's column: 0 standing, 5 wind-up, 6 strike, 7 cast, 8 hurt, 9 downed; -1 to leave it to the walk.</param>
/// <param name="Offset">How far it's moved, in world pixels: across toward the way it faces, and down.</param>
/// <param name="Scale">How much wider and taller it's drawn, about its feet.</param>
/// <param name="Lean">How far it's turned about its feet, in radians toward the way it faces (less than nothing is back).</param>
/// <param name="Tint">The colour washed over it, or transparent for none (premultiplied: a colour times its strength).</param>
/// <param name="Alpha">How much of it shows, 0 to 1.</param>
public readonly record struct CreaturePose(int Column, Vector2 Offset, Vector2 Scale, float Lean, Color Tint, float Alpha)
{
    /// <summary>Standing as it is.</summary>
    public static readonly CreaturePose Rest = new(-1, Vector2.Zero, Vector2.One, 0f, Color.Transparent, 1f);
}

/// <summary>
/// The fighting moves of the creatures that have no rig (the sea-wraith, the gloomfang and the
/// Thornmother are ten-column pixel sheets): each move made from the frames they have by squash,
/// stretch, lean, lunge, tint and tip-over, with no new art.
/// </summary>
/// <remarks>
/// <para>
/// Each move is a clip with no bones (Clip with only a length and events), so a creature's attack
/// times its blow the same way a person's does: the screen asks for "auto-claw-1" or
/// "telegraph-bite", gets back a clip with a "hit" in it, and the walker fires ClipEvent on it. Pose
/// says what the picture does at each moment:
/// </para>
/// <list type="bullet">
/// <item><b>telegraph</b>: crouched into the wind-up frame, squashed (shorter and wider), leaning back, with a red tint pulsing, held as long as the wind-up lasts.</item>
/// <item><b>an attack</b> (auto, skill, foe moves, strike): a quick coil, then stretched long toward the target and lunging into the strike frame on the blow, and easing back with a little overshoot.</item>
/// <item><b>hurt</b>: the hurt frame, squashed and knocked back, springing back to size.</item>
/// <item><b>block, dodge</b>: braced in the wind-up frame and pushed back; or a hop back.</item>
/// <item><b>knockdown, getup</b>: tipping back from the hurt frame onto the downed one ("land"), and back up.</item>
/// <item><b>die</b>: a flinch, sliding back as it tips over, landing on the downed frame ("land"), a held beat, then fading out ("fx" death-motes, when the screen lets the motes go).</item>
/// <item><b>idle and ready</b>: breathing, a slow swell.</item>
/// </list>
/// </remarks>
public static class CreatureMotion
{
    /// <summary>The sheet's columns.</summary>
    public const int Stand = 0, WindUp = 5, Strike = 6, Cast = 7, Hurt = 8, Downed = 9;

    /// <summary>What each kind of move is.</summary>
    public enum Move
    {
        /// <summary>Breathing.</summary>
        Idle,

        /// <summary>A held wind-up.</summary>
        Telegraph,

        /// <summary>A blow.</summary>
        Attack,

        /// <summary>Casting.</summary>
        Cast,

        /// <summary>A flinch.</summary>
        Hurt,

        /// <summary>Braced against a blow.</summary>
        Block,

        /// <summary>A hop back.</summary>
        Dodge,

        /// <summary>Knocked off its feet.</summary>
        Knockdown,

        /// <summary>Back up.</summary>
        GetUp,

        /// <summary>Lying down.</summary>
        Down,

        /// <summary>Death.</summary>
        Die,
    }

    /// <summary>An attack's timing: the coil, the blow landing, the end of the lunge, and the whole.</summary>
    private const float AttackCoil = 0.2f, AttackHit = 0.27f, AttackRecover = 0.4f, AttackLength = 0.62f;

    /// <summary>Death's timing: the tip over ends and it lands, the hold ends and it starts to fade, and the whole.</summary>
    private const float DieLand = 0.42f, DieFade = 0.95f, DieLength = 1.6f;

    /// <summary>Knockdown's: when it lands, and the whole.</summary>
    private const float KnockLand = 0.32f, KnockLength = 0.55f;

    /// <summary>The red of a telegraph's pulse.</summary>
    private static readonly Color Warning = new(255, 40, 30);

    private static readonly Dictionary<string, Clip> Made = new(StringComparer.Ordinal);

    /// <summary>What kind of move a clip name is, for a creature: telegraph-*, the attacks (auto-*, skill-*, foe-*, strike, attack), hurt, block, dodge, knockdown, getup, downed, die, cast and channel, or breathing for anything else.</summary>
    /// <param name="name">The clip's name.</param>
    public static Move Kind(string name)
    {
        if (name is null) return Move.Idle;
        if (name.StartsWith("telegraph", StringComparison.Ordinal) || name.StartsWith("windup", StringComparison.Ordinal)) return Move.Telegraph;
        if (name.StartsWith("auto-", StringComparison.Ordinal) || name.StartsWith("skill-", StringComparison.Ordinal) || name.StartsWith("foe-", StringComparison.Ordinal)
            || name.StartsWith("strike", StringComparison.Ordinal) || name == "attack" || name == "soul-link-strike") return Move.Attack;
        return name switch
        {
            "hurt" or "hurt-big" => Move.Hurt,
            "block" => Move.Block,
            "dodge" => Move.Dodge,
            "knockdown" => Move.Knockdown,
            "getup" or "revive" => Move.GetUp,
            "downed" => Move.Down,
            "die" => Move.Die,
            "cast" or "channel" => Move.Cast,
            _ => Move.Idle,
        };
    }

    /// <summary>The clip a creature plays for a move by name: no bones, its length and its events, made once for each name.</summary>
    /// <param name="name">The clip's name, as a person's would be.</param>
    public static Clip ClipFor(string name)
    {
        name ??= "idle";
        if (Made.TryGetValue(name, out Clip made)) return made;
        Move kind = Kind(name);
        (float length, bool loop, ClipEvent[] events) = kind switch
        {
            Move.Telegraph => (0.8f, true, new[] { new ClipEvent(0f, ClipEvent.Telegraph, null) }),
            Move.Attack => (AttackLength, false, new[] { new ClipEvent(AttackHit, ClipEvent.Hit, null), new ClipEvent(AttackRecover, ClipEvent.Recover, null) }),
            Move.Cast => (0.6f, name == "channel", new[] { new ClipEvent(0.3f, ClipEvent.Release, null) }),
            Move.Hurt => (name == "hurt-big" ? 0.45f : 0.35f, false, Array.Empty<ClipEvent>()),
            Move.Block => (0.4f, false, new[] { new ClipEvent(0.08f, ClipEvent.Hit, null) }),
            Move.Dodge => (0.4f, false, Array.Empty<ClipEvent>()),
            Move.Knockdown => (KnockLength, false, new[] { new ClipEvent(KnockLand, ClipEvent.Land, null) }),
            Move.GetUp => (0.5f, false, Array.Empty<ClipEvent>()),
            Move.Down => (1f, true, Array.Empty<ClipEvent>()),
            Move.Die => (DieLength, false, new[] { new ClipEvent(DieLand, ClipEvent.Land, null), new ClipEvent(DieFade, ClipEvent.Fx, "death-motes") }),
            _ => (2.4f, true, Array.Empty<ClipEvent>()),
        };
        made = new Clip(name, length, loop, ClipEase.Smooth, Array.Empty<ClipTrack>(), Array.Empty<string>(), 0f, Array.Empty<float>(), Array.Empty<ClipSwap>())
        {
            Events = events,
            Blend = 0f,
        };
        Made[name] = made;
        return made;
    }

    /// <summary>How the picture is posed at a time into a move.</summary>
    /// <param name="kind">The move.</param>
    /// <param name="time">Seconds into it.</param>
    /// <param name="length">How long the move is.</param>
    /// <param name="breath">Seconds through the creature's own breath, for breathing.</param>
    public static CreaturePose Pose(Move kind, float time, float length, float breath)
    {
        float swell = 1f + 0.018f * MathF.Sin(breath / 2.4f * MathHelper.TwoPi);
        switch (kind)
        {
            case Move.Telegraph:
            {
                float sink = Ease(MathHelper.Clamp(time / 0.15f, 0f, 1f));
                float pulse = 0.5f + 0.5f * MathF.Sin(time * MathHelper.TwoPi * 3f);
                return new CreaturePose(WindUp, new Vector2(-2f * sink, 0f), new Vector2(1f + 0.1f * sink, 1f - 0.12f * sink), -0.06f * sink,
                    Warning * ((0.1f + 0.22f * pulse) * sink), 1f);
            }
            case Move.Attack:
            {
                if (time < AttackCoil)
                {
                    float c = Ease(time / AttackCoil);
                    return new CreaturePose(WindUp, new Vector2(-3f * c, 0f), new Vector2(1f + 0.08f * c, 1f - 0.1f * c), -0.07f * c, Color.Transparent, 1f);
                }
                if (time < AttackRecover)
                {
                    // Snapping out to the full lunge by the blow, held through it.
                    float u = MathHelper.Clamp((time - AttackCoil) / (AttackHit - AttackCoil), 0f, 1f);
                    float o = 1f - (1f - u) * (1f - u);
                    return new CreaturePose(Strike, new Vector2(-3f + 13f * o, 0f), new Vector2(1f + 0.2f * o, 1f - 0.12f * o), 0.08f * o, Color.Transparent, 1f);
                }
                float r = MathHelper.Clamp((time - AttackRecover) / (length - AttackRecover), 0f, 1f);
                float back = Back(r);
                return new CreaturePose(r < 0.45f ? Strike : Stand, new Vector2(10f * (1f - back), 0f),
                    new Vector2(1f + 0.2f * (1f - back), 1f - 0.12f * (1f - back)), 0.08f * (1f - back), Color.Transparent, 1f);
            }
            case Move.Cast:
            {
                float u = Ease(MathHelper.Clamp(time / 0.2f, 0f, 1f));
                return new CreaturePose(Cast, Vector2.Zero, new Vector2(1f - 0.04f * u, 1f + 0.06f * u), -0.03f * u, Color.Transparent, 1f);
            }
            case Move.Hurt:
            {
                float u = MathHelper.Clamp(time / length, 0f, 1f);
                float squash = Wobble(u);
                return new CreaturePose(u < 0.75f ? Hurt : Stand, new Vector2(-4f * (1f - Ease(u)), 0f), new Vector2(1f + 0.14f * squash, 1f - 0.16f * squash),
                    -0.08f * (1f - Ease(u)), Color.Transparent, 1f);
            }
            case Move.Block:
            {
                float u = MathHelper.Clamp(time / length, 0f, 1f);
                float push = MathF.Sin(MathF.PI * MathF.Min(1f, u * 1.6f));
                return new CreaturePose(u < 0.8f ? WindUp : Stand, new Vector2(-3f * push, 0f), new Vector2(1f + 0.06f * push, 1f - 0.08f * push), -0.04f * push, Color.Transparent, 1f);
            }
            case Move.Dodge:
            {
                float u = MathHelper.Clamp(time / length, 0f, 1f);
                float hop = MathF.Sin(MathF.PI * u);
                return new CreaturePose(u < 0.7f ? 1 : Stand, new Vector2(-9f * hop, -4f * MathF.Sin(MathF.PI * MathF.Min(1f, u * 2f))), Vector2.One, -0.05f * hop, Color.Transparent, 1f);
            }
            case Move.Knockdown:
                return Fall(time, KnockLand, 1f);
            case Move.GetUp:
            {
                float u = MathHelper.Clamp(time / length, 0f, 1f);
                if (u < 0.35f) return new CreaturePose(Downed, Vector2.Zero, new Vector2(1f, 1f - 0.08f * Ease(u / 0.35f)), 0f, Color.Transparent, 1f);
                float up = Ease((u - 0.35f) / 0.65f);
                return new CreaturePose(u < 0.7f ? Hurt : Stand, new Vector2(-3f * (1f - up), 0f), new Vector2(1f, 0.85f + 0.15f * up), -0.25f * (1f - up), Color.Transparent, 1f);
            }
            case Move.Down:
                return new CreaturePose(Downed, Vector2.Zero, Vector2.One, 0f, Color.Transparent, 1f);
            case Move.Die:
            {
                CreaturePose fall = Fall(time, DieLand, 1f);
                float fade = MathHelper.Clamp((time - DieFade) / (length - DieFade), 0f, 1f);
                return fall with { Alpha = 1f - fade * fade };
            }
            default:
                return new CreaturePose(-1, Vector2.Zero, new Vector2(1f + (swell - 1f) * 0.4f, swell), 0f, Color.Transparent, 1f);
        }
    }

    /// <summary>Tipping back off its feet: the hurt frame turning back and sliding as it goes over, then the downed frame from the landing, with a little bounce.</summary>
    private static CreaturePose Fall(float time, float land, float alpha)
    {
        if (time < land)
        {
            float u = time / land;
            float over = u * u;
            return new CreaturePose(Hurt, new Vector2(-10f * Ease(u), 0f), new Vector2(1f + 0.05f * over, 1f - 0.12f * over), -0.55f * over, Color.Transparent, alpha);
        }
        float b = MathHelper.Clamp((time - land) / 0.18f, 0f, 1f);
        float bounce = MathF.Sin(MathF.PI * b) * (1f - b);
        return new CreaturePose(Downed, new Vector2(-10f, -2f * bounce), new Vector2(1f + 0.08f * (1f - b), 1f - 0.1f * (1f - b)), 0f, Color.Transparent, alpha);
    }

    /// <summary>Smoothstep.</summary>
    private static float Ease(float u) => u * u * (3f - 2f * u);

    /// <summary>Ease-out with overshoot (the clips' "back").</summary>
    private static float Back(float u)
    {
        const float s = 1.70158f;
        float w = u - 1f;
        return 1f + (s + 1f) * w * w * w + s * w * w;
    }

    /// <summary>A squash that hits at once and springs back past nothing and settles: 1 at the start, a small dip below 0 and back to 0.</summary>
    private static float Wobble(float u) => MathF.Exp(-5f * u) * MathF.Cos(u * MathF.PI * 2.2f);
}
