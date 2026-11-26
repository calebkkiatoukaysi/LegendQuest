using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters;

/// <summary>One sprite frame of a pixel figure, kept: its texture, where the picture and its white inside (for the hit flash) are on it, and where the feet are on the picture.</summary>
public sealed class PixelFrame
{
    /// <summary>Makes a frame of a texture.</summary>
    /// <param name="texture">The texture.</param>
    /// <param name="source">Where the picture is on it.</param>
    /// <param name="flash">Where the figure's inside, in white, is on it.</param>
    /// <param name="feet">Where the feet are, in art pixels from the picture's top left.</param>
    public PixelFrame(Texture2D texture, Rectangle source, Rectangle flash, Vector2 feet)
    {
        Texture = texture;
        Source = source;
        Flash = flash;
        Feet = feet;
    }

    /// <summary>The texture.</summary>
    public Texture2D Texture { get; }

    /// <summary>Where the picture is on the texture.</summary>
    public Rectangle Source { get; }

    /// <summary>Where the figure's inside, in white, is on the texture.</summary>
    public Rectangle Flash { get; }

    /// <summary>Where the feet are on the picture, in art pixels from Source's top left.</summary>
    public Vector2 Feet { get; }

    /// <summary>The last drawn frame of the game it was drawn in (PixelFrames.Now), so one still showing isn't let go.</summary>
    public long Used { get; set; }
}

/// <summary>
/// The pixel figures' sprite frames, kept as they're drawn: a figure's pose at a step of a clip is
/// the same picture every time the clip comes round (a breath, a stride, a swing), and the same for
/// everyone who shares the figure, so it's posed once (PixelPoser) and drawn from here after.
/// </summary>
/// <remarks>
/// A frame is kept by the figure, the clip, the step into it, and how far the skirt and the cape
/// have swung (in steps of SwingStep degrees). Only a pose with nothing passing in it is kept: one
/// that's still blending from the clip before, squashed by a hit or smearing a blade is drawn on
/// the walker's own texture and not kept. A step that's the same picture as the one before it
/// (most of a breath) is kept as that frame again, not a new one. When more than Most are kept, the ones not drawn for
/// longest go, never one drawn in the last few frames.
/// </remarks>
public static class PixelFrames
{
    /// <summary>How many degrees of swing make a different frame.</summary>
    public const float SwingStep = 3f;

    /// <summary>The step that stands for a clip's last pose, held.</summary>
    public const int Held = int.MaxValue;

    /// <summary>How many frames are kept before the oldest go, and how many drawn frames of the game a frame has to have gone unused to go.</summary>
    private const int Most = 1500, Idle = 30;

    /// <summary>What a frame is kept by.</summary>
    /// <param name="Rig">The figure.</param>
    /// <param name="Clip">The clip, or null for the rest pose.</param>
    /// <param name="Step">The step into the clip, ArtSettings.PoseRate to a second; Held for its last pose.</param>
    /// <param name="Skirt">The skirt's swing, in SwingSteps.</param>
    /// <param name="Cape">The cape's swing, in SwingSteps.</param>
    public readonly record struct Key(PixelRig Rig, Clip Clip, int Step, int Skirt, int Cape);

    private static readonly Dictionary<Key, PixelFrame> Kept = new();

    /// <summary>The drawn frame of the game this is, counted by Tick.</summary>
    public static long Now { get; private set; }

    /// <summary>Counts a drawn frame of the game. Once a frame, from the game's Draw.</summary>
    public static void Tick() => Now++;

    /// <summary>The frame kept for a key, if there is one.</summary>
    public static bool TryGet(Key key, out PixelFrame frame)
    {
        if (!Kept.TryGetValue(key, out frame)) return false;
        // One picture can be kept for several steps, and went with the first of them to go.
        if (!frame.Texture.IsDisposed) return true;
        Kept.Remove(key);
        frame = null;
        return false;
    }

    /// <summary>Keeps a frame, letting the ones unused for longest go if there are too many.</summary>
    public static void Add(Key key, PixelFrame frame)
    {
        if (Kept.Count >= Most) Trim();
        frame.Used = Now;
        Kept[key] = frame;
    }

    /// <summary>Lets every frame go: when the figures are made again.</summary>
    public static void Clear()
    {
        foreach (PixelFrame frame in Kept.Values) frame.Texture.Dispose();
        Kept.Clear();
    }

    /// <summary>Lets go the quarter of the frames unused for longest, leaving any drawn in the last Idle frames of the game.</summary>
    private static void Trim()
    {
        var old = new List<(long Used, Key Key)>();
        foreach ((Key key, PixelFrame frame) in Kept)
            if (Now - frame.Used > Idle) old.Add((frame.Used, key));
        old.Sort((a, b) => a.Used.CompareTo(b.Used));
        for (int i = 0; i < Math.Min(old.Count, Most / 4); i++)
        {
            Kept.Remove(old[i].Key, out PixelFrame frame);
            if (!frame.Texture.IsDisposed) frame.Texture.Dispose();
        }
    }
}
