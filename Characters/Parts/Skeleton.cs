using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>
/// The cut-out skeleton the figures from parts are animated on (tools/rig/skeleton.py, as
/// parts.json carries it): its bones, parents first, each turning about a joint of its base, and the
/// order their pieces are drawn in. The clips (ClipLibrary) turn and move the bones; Solve works out
/// where each one is then.
/// </summary>
public sealed class Skeleton
{
    private readonly Dictionary<string, int> _index = new(StringComparer.Ordinal);

    /// <summary>The skeleton parts.json describes.</summary>
    /// <param name="bones">Each bone as its name, its parent's (null for the root) and its joint's, parents first.</param>
    /// <param name="draw">The bones back to front, as their pieces are drawn at rest.</param>
    public Skeleton(string[][] bones, string[] draw)
    {
        Names = new string[bones.Length];
        Parents = new int[bones.Length];
        Joints = new string[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            Names[i] = bones[i][0];
            Joints[i] = bones[i][2];
            Parents[i] = bones[i][1] is string parent ? _index[parent] : -1;
            _index[Names[i]] = i;
        }
        Draw = draw;
    }

    /// <summary>The skeleton the parts in Content were cut for, once PartCatalog's read them; null before.</summary>
    public static Skeleton Current { get; internal set; }

    /// <summary>Each bone's name, parents first.</summary>
    public string[] Names { get; }

    /// <summary>Each bone's parent, by its place in Names; -1 for the root.</summary>
    public int[] Parents { get; }

    /// <summary>The joint each bone turns about, by name (the bases' Joints).</summary>
    public string[] Joints { get; }

    /// <summary>The bones back to front, as their pieces are drawn at rest.</summary>
    public string[] Draw { get; }

    /// <summary>How many bones there are.</summary>
    public int Count => Names.Length;

    /// <summary>A bone's place in Names, or -1 for one there isn't.</summary>
    /// <param name="bone">Its name.</param>
    public int IndexOf(string bone) => bone is not null && _index.TryGetValue(bone, out int i) ? i : -1;

    /// <summary>
    /// Where every bone is in a pose: each one's turn from the rest pose, all its parents' added
    /// (radians, clockwise), and where its joint has gone, on the picture its joints are measured on.
    /// A bone turns about its joint, then moves by its offset, and carries its children; its stretch
    /// (Sx, Sy) stretches its own piece and how far off its joint its children's joints are, the same
    /// as tools/rig/skeleton.py's world().
    /// </summary>
    /// <param name="pose">Each bone's turn (degrees), offset (the clips' units) and stretch, by its place in Names.</param>
    /// <param name="joints">Each bone's joint at rest, on the picture.</param>
    /// <param name="unit">How many of the picture's pixels one of the clips' units is.</param>
    /// <param name="turns">Each bone's turn, filled in.</param>
    /// <param name="at">Where each bone's joint is, filled in.</param>
    public void Solve(BonePose[] pose, Vector2[] joints, float unit, float[] turns, Vector2[] at) => Solve(pose, joints, unit, turns, at, null);

    /// <summary>
    /// Where every bone is in a pose, as Solve, and each bone's whole transform without its shift: how
    /// a step across its own piece on the picture is turned and stretched by the time it's drawn,
    /// its parents' stretches and its own included.
    /// </summary>
    /// <param name="pose">Each bone's turn (degrees), offset (the clips' units) and stretch, by its place in Names.</param>
    /// <param name="joints">Each bone's joint at rest, on the picture.</param>
    /// <param name="unit">How many of the picture's pixels one of the clips' units is.</param>
    /// <param name="turns">Each bone's turn, filled in.</param>
    /// <param name="at">Where each bone's joint is, filled in.</param>
    /// <param name="linear">Each bone's turn and stretch together, filled in; or null.</param>
    public void Solve(BonePose[] pose, Vector2[] joints, float unit, float[] turns, Vector2[] at, Linear2[] linear)
    {
        // Each bone's whole move as a linear part and a shift: a point p goes to Linear(p) + shift.
        Span<Linear2> whole = stackalloc Linear2[Count];
        Span<Vector2> shift = stackalloc Vector2[Count];
        for (int i = 0; i < Count; i++)
        {
            BonePose own = pose[i];
            float local = MathHelper.ToRadians(own.Rot);
            Vector2 joint = joints[i];
            // Its own move, about its joint: stretched, turned, then moved by its offset.
            Linear2 mine = Linear2.Turn(local) * Linear2.Stretch(own.Sx, own.Sy);
            Vector2 moved = joint + new Vector2(own.X, own.Y) * unit;
            Vector2 offset = moved - mine.Apply(joint);
            int p = Parents[i];
            if (p < 0)
            {
                turns[i] = local;
                whole[i] = mine;
                shift[i] = offset;
            }
            else
            {
                turns[i] = turns[p] + local;
                whole[i] = whole[p] * mine;
                shift[i] = whole[p].Apply(offset) + shift[p];
            }
            at[i] = whole[i].Apply(joint) + shift[i];
            if (linear is not null) linear[i] = whole[i];
        }
    }

    /// <summary>A point turned about the origin, clockwise (down the screen), by an angle in radians.</summary>
    public static Vector2 Turn(float angle, Vector2 p)
    {
        float c = MathF.Cos(angle), s = MathF.Sin(angle);
        return new Vector2(c * p.X - s * p.Y, s * p.X + c * p.Y);
    }
}

/// <summary>One bone in a pose: its turn from the rest pose (degrees, clockwise), its offset (the clips' units) and its own piece's stretch.</summary>
/// <param name="Rot">Its turn, in degrees.</param>
/// <param name="X">Its offset across.</param>
/// <param name="Y">Its offset down.</param>
/// <param name="Sx">How much wider its own piece is drawn.</param>
/// <param name="Sy">How much taller its own piece is drawn.</param>
public readonly record struct BonePose(float Rot, float X, float Y, float Sx, float Sy)
{
    /// <summary>The rest pose's: no turn, no offset, no stretch.</summary>
    public static readonly BonePose Rest = new(0f, 0f, 0f, 1f, 1f);

    /// <summary>Partway from one pose to another.</summary>
    public static BonePose Lerp(BonePose a, BonePose b, float t) =>
        new(MathHelper.Lerp(a.Rot, b.Rot, t), MathHelper.Lerp(a.X, b.X, t), MathHelper.Lerp(a.Y, b.Y, t), MathHelper.Lerp(a.Sx, b.Sx, t), MathHelper.Lerp(a.Sy, b.Sy, t));
}

/// <summary>A 2D linear map (a turn, a stretch, or any mix of them): where a step across goes (the first column) and where a step down goes (the second).</summary>
/// <param name="M11">Across, from a step across.</param>
/// <param name="M12">Across, from a step down.</param>
/// <param name="M21">Down, from a step across.</param>
/// <param name="M22">Down, from a step down.</param>
public readonly record struct Linear2(float M11, float M12, float M21, float M22)
{
    /// <summary>The one that changes nothing.</summary>
    public static readonly Linear2 Identity = new(1f, 0f, 0f, 1f);

    /// <summary>A turn clockwise (down the screen), in radians.</summary>
    public static Linear2 Turn(float angle)
    {
        float c = MathF.Cos(angle), s = MathF.Sin(angle);
        return new Linear2(c, -s, s, c);
    }

    /// <summary>A stretch along the axes.</summary>
    public static Linear2 Stretch(float x, float y) => new(x, 0f, 0f, y);

    /// <summary>One map after another: a * b is b, then a.</summary>
    public static Linear2 operator *(Linear2 a, Linear2 b) => new(
        a.M11 * b.M11 + a.M12 * b.M21, a.M11 * b.M12 + a.M12 * b.M22,
        a.M21 * b.M11 + a.M22 * b.M21, a.M21 * b.M12 + a.M22 * b.M22);

    /// <summary>Where it takes a point.</summary>
    public Vector2 Apply(Vector2 p) => new(M11 * p.X + M12 * p.Y, M21 * p.X + M22 * p.Y);

    /// <summary>
    /// As near as a sprite can draw it: a turn (radians, clockwise) and a stretch along the sprite's
    /// own axes, the turn taken from where a step across goes. A map that shears (a stretched parent
    /// with a child turned in it) loses the shear, which at the stretches the clips use is less than
    /// a pixel.
    /// </summary>
    /// <param name="angle">The turn.</param>
    /// <param name="scale">The stretch across and down; down is negative for a map that mirrors.</param>
    public void Decompose(out float angle, out Vector2 scale)
    {
        float across = MathF.Sqrt(M11 * M11 + M21 * M21);
        if (across < 1e-6f)
        {
            angle = 0f;
            scale = new Vector2(0f, M22);
            return;
        }
        angle = MathF.Atan2(M21, M11);
        scale = new Vector2(across, (M11 * M22 - M12 * M21) / across);
    }
}
