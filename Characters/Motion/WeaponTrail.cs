using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters.Motion;

/// <summary>
/// The fading ribbon a weapon leaves behind it through the fastest frames of a swing (a clip's trail
/// windows): the blade's base and tip, sampled a few times a frame, joined into a band that fades by
/// age and toward the hilt, and the weapon's own ghosts at the last few of those poses.
/// </summary>
/// <remarks>
/// A swing turns the blade through a hundred degrees or more in two or three frames, so samples taken
/// once a frame would leave a ribbon of straight gaps; the figure samples its clip at a few points
/// between frames (Samples a frame) and adds each. A sample lives Life seconds, about seven frames.
/// The band is a crescent along the outer half of the blade: nothing at its inner edge, under half
/// strength a quarter of the way short of the tip and full at the tip, so it reads as the arc the edge
/// cut rather than a sheet.
/// The ghosts are the weapon's own picture drawn again at older poses, fainter each, the way an
/// animator draws a smear; they're drawn with the figure, in its batch.
/// </remarks>
public sealed class WeaponTrail
{
    /// <summary>How long a sample lasts, in seconds.</summary>
    public const float Life = 0.12f;

    /// <summary>How many samples a frame a swing is cut into.</summary>
    public const int Samples = 4;

    /// <summary>How many ghosts of the weapon are drawn, and how strong the first is.</summary>
    public const int Ghosts = 3;
    private const float GhostStrength = 0.4f;

    /// <summary>How strong the ribbon is at its inner edge, and along its edge line (EdgeAt of the way out to the tip), against the tip; and how strong it is at its strongest.</summary>
    private const float BaseStrength = 0f;
    private const float EdgeStrength = 0.45f;
    private const float EdgeAt = 0.75f;
    private const float Strength = 0.85f;

    /// <summary>The most samples kept.</summary>
    private const int Most = 48;

    private readonly List<Sample> _samples = new();

    /// <summary>One sample of the weapon: its base and tip and where the piece was drawn, in world pixels, its turn and stretch, and how old it is.</summary>
    public struct Sample
    {
        public Vector2 Base;
        public Vector2 Tip;
        public Vector2 At;
        public float Turn;
        public Vector2 Scale;
        public float Age;
    }

    /// <summary>The colour of the ribbon (not premultiplied): a warm white unless the weapon's lit with something.</summary>
    public Color Colour { get; set; } = new(255, 244, 222);

    /// <summary>Whether there's anything to draw.</summary>
    public bool Live => _samples.Count > 0;

    /// <summary>The samples, oldest first.</summary>
    public IReadOnlyList<Sample> All => _samples;

    /// <summary>Adds a sample, already some seconds old (a sub-step earlier in the frame).</summary>
    public void Add(Vector2 bladeBase, Vector2 tip, Vector2 at, float turn, Vector2 scale, float age)
    {
        if (_samples.Count >= Most) _samples.RemoveAt(0);
        _samples.Add(new Sample { Base = bladeBase, Tip = tip, At = at, Turn = turn, Scale = scale, Age = age });
    }

    /// <summary>Ages every sample and lets the old ones go.</summary>
    /// <param name="seconds">How long since the last update: the figure's own, so a hit-stop holds it.</param>
    public void Update(float seconds)
    {
        for (int i = _samples.Count - 1; i >= 0; i--)
        {
            Sample s = _samples[i];
            s.Age += seconds;
            if (s.Age >= Life) _samples.RemoveAt(i);
            else _samples[i] = s;
        }
    }

    /// <summary>Lets every sample go.</summary>
    public void Clear() => _samples.Clear();

    /// <summary>Draws the ribbon: a band between each sample and the next, fading by age and in toward the hilt, brightest along the tip's path.</summary>
    /// <param name="batch">The ribbon batch, begun.</param>
    /// <param name="strength">How strong, 0 to 1 (the figure's opacity).</param>
    public void Draw(RibbonBatch batch, float strength)
    {
        for (int i = 1; i < _samples.Count; i++)
        {
            Sample a = _samples[i - 1], b = _samples[i];
            float fa = Fade(a.Age) * strength * Strength, fb = Fade(b.Age) * strength * Strength;
            Vector2 ea = Vector2.Lerp(a.Base, a.Tip, EdgeAt), eb = Vector2.Lerp(b.Base, b.Tip, EdgeAt);
            batch.Quad(a.Base, ea, eb, b.Base, Colour * (fa * BaseStrength), Colour * (fa * EdgeStrength), Colour * (fb * EdgeStrength), Colour * (fb * BaseStrength));
            batch.Quad(ea, a.Tip, b.Tip, eb, Colour * (fa * EdgeStrength), Colour * fa, Colour * fb, Colour * (fb * EdgeStrength));
        }
    }

    /// <summary>How strong a sample of an age is: falling away as the square.</summary>
    private static float Fade(float age)
    {
        float f = 1f - MathHelper.Clamp(age / Life, 0f, 1f);
        return f * f;
    }

    /// <summary>The ghost strengths, newest first: each fainter.</summary>
    public static float GhostAt(int i) => GhostStrength * MathF.Pow(0.55f, i);
}

/// <summary>
/// Where a weapon piece's blade is, on its own picture: the grip (its pivot, where the fist closes)
/// and the furthest drawn pixel from it (the tip), and the point halfway along that's taken as the
/// start of the blade, for the ribbon's base. Worked out once a piece and kept.
/// </summary>
public sealed class WeaponShape
{
    private static readonly ConditionalWeakTable<Texture2D, WeaponShape> Shapes = new();

    /// <summary>How far from the grip toward the tip the ribbon's base is.</summary>
    private const float BladeStart = 0.5f;

    /// <summary>The grip, on the piece's picture.</summary>
    public Vector2 Grip { get; private init; }

    /// <summary>The tip, on the piece's picture.</summary>
    public Vector2 Tip { get; private init; }

    /// <summary>The ribbon's base, on the piece's picture.</summary>
    public Vector2 Base => Vector2.Lerp(Grip, Tip, BladeStart);

    /// <summary>Which way the blade runs on the picture, from the grip to the tip, as a unit.</summary>
    public Vector2 Axis => Tip == Grip ? Vector2.UnitX : Vector2.Normalize(Tip - Grip);

    /// <summary>How far off the grip the other end has to be, against the first, for the piece to count as held in the middle; and how far out from the grip, against its length, what's drawn toward each end is counted from.</summary>
    private const float TwoEnded = 0.75f, EndShare = 0.3f;

    /// <summary>A weapon piece's shape, worked out the first time.</summary>
    /// <param name="picture">The piece's picture.</param>
    /// <param name="grip">Its pivot on that picture (where it turns about).</param>
    public static WeaponShape Of(Texture2D picture, Vector2 grip)
    {
        if (Shapes.TryGetValue(picture, out WeaponShape shape)) return shape;
        Vector2 tip = grip;
        if (picture.Format == SurfaceFormat.Color)
        {
            var pixels = new Color[picture.Width * picture.Height];
            picture.GetData(pixels);
            tip = Furthest(pixels, picture.Width, picture.Height, grip, Vector2.Zero);

            // Held in the middle (a spear, a staff), the far end of the haft can be as far off as the
            // head: then the tip is the end with more drawn out toward it, which is the head.
            Vector2 other = Furthest(pixels, picture.Width, picture.Height, grip, tip - grip);
            float reach = Vector2.Distance(tip, grip);
            if (other != grip && Vector2.Distance(other, grip) > TwoEnded * reach
                && Mass(pixels, picture.Width, picture.Height, grip, other, reach * EndShare) > Mass(pixels, picture.Width, picture.Height, grip, tip, reach * EndShare))
                tip = other;
        }
        shape = new WeaponShape { Grip = grip, Tip = tip };
        Shapes.AddOrUpdate(picture, shape);
        return shape;
    }

    /// <summary>The drawn pixel furthest from the grip, among those the other way from a direction (any, for none).</summary>
    private static Vector2 Furthest(Color[] pixels, int width, int height, Vector2 grip, Vector2 awayFrom)
    {
        Vector2 best = grip;
        float far = -1f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].A < 64) continue;
                var p = new Vector2(x + 0.5f, y + 0.5f);
                if (awayFrom != Vector2.Zero && Vector2.Dot(p - grip, awayFrom) >= 0f) continue;
                float d = Vector2.DistanceSquared(p, grip);
                if (d <= far) continue;
                far = d;
                best = p;
            }
        }
        return best;
    }

    /// <summary>How many drawn pixels are out toward an end: more than a distance from the grip along the way to it.</summary>
    private static int Mass(Color[] pixels, int width, int height, Vector2 grip, Vector2 end, float beyond)
    {
        Vector2 way = end - grip;
        if (way == Vector2.Zero) return 0;
        way.Normalize();
        int count = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (pixels[y * width + x].A >= 64 && Vector2.Dot(new Vector2(x + 0.5f, y + 0.5f) - grip, way) > beyond) count++;
        return count;
    }
}
