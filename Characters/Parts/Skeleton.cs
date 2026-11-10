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
    /// A bone turns about its joint, then moves by its offset, and carries its children.
    /// </summary>
    /// <param name="pose">Each bone's turn (degrees) and offset (the clips' units), by its place in Names.</param>
    /// <param name="joints">Each bone's joint at rest, on the picture.</param>
    /// <param name="unit">How many of the picture's pixels one of the clips' units is.</param>
    /// <param name="turns">Each bone's turn, filled in.</param>
    /// <param name="at">Where each bone's joint is, filled in.</param>
    public void Solve(BonePose[] pose, Vector2[] joints, float unit, float[] turns, Vector2[] at)
    {
        // Each bone's whole move as a turn and a shift: a point p goes to Turn(turn, p) + shift.
        Span<Vector2> shift = stackalloc Vector2[Count];
        for (int i = 0; i < Count; i++)
        {
            BonePose own = pose[i];
            float local = MathHelper.ToRadians(own.Rot);
            Vector2 joint = joints[i];
            Vector2 moved = joint + new Vector2(own.X, own.Y) * unit;
            // Its own move: p to Turn(local, p - joint) + joint + offset.
            Vector2 mine = moved - Turn(local, joint);
            int p = Parents[i];
            if (p < 0)
            {
                turns[i] = local;
                shift[i] = mine;
            }
            else
            {
                turns[i] = turns[p] + local;
                shift[i] = Turn(turns[p], mine) + shift[p];
            }
            at[i] = Turn(turns[i], joint) + shift[i];
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
