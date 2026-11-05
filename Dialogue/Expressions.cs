using System.Collections.Generic;

namespace LegendQuest.Dialogue;

/// <summary>
/// The twelve faces a line can be said with, as the scripts write them in brackets after the
/// speaker: SB (sad).
/// </summary>
/// <remarks>
/// The art spec's twelve (03-art-overhaul.md §5.2), and no others, so a typo in a script shows up
/// when the game starts instead of as a face that never changes. Worried is for someone else and
/// afraid is for yourself, so they're kept apart. A line with none is neutral. Legacy portraits have
/// the one face, so they don't show it; the rig's draw each.
/// </remarks>
public static class Expressions
{
    /// <summary>The face a line has when it doesn't say: eyes open, brows level, mouth closed.</summary>
    public const string Neutral = "neutral";

    /// <summary>A plain smile.</summary>
    public const string Smile = "smile";

    /// <summary>Brows up and a grin, the eyes half shut.</summary>
    public const string Grin = "grin";

    /// <summary>Eyes shut and the mouth open, the head bobbing with it.</summary>
    public const string Laugh = "laugh";

    /// <summary>One brow up and the eyes narrowed.</summary>
    public const string Smirk = "smirk";

    /// <summary>Eyes down and a frown.</summary>
    public const string Sad = "sad";

    /// <summary>Brows knit and the mouth flat: worried for someone else.</summary>
    public const string Worried = "worried";

    /// <summary>Eyes wide and the mouth open: afraid for yourself.</summary>
    public const string Afraid = "afraid";

    /// <summary>Eyes narrowed, brows knit, teeth gritted.</summary>
    public const string Angry = "angry";

    /// <summary>Eyes narrowed and nothing else moving, not even a blink: the Soul-Bound of Act 3.</summary>
    public const string Cold = "cold";

    /// <summary>Eyes half shut and the mouth flat.</summary>
    public const string Tired = "tired";

    /// <summary>Eyes wide, brows up, mouth open.</summary>
    public const string Shock = "shock";

    private static readonly HashSet<string> All = new()
    {
        Neutral, Smile, Grin, Laugh, Smirk, Sad, Worried, Afraid, Angry, Cold, Tired, Shock,
    };

    /// <summary>Whether a line can be said with this face.</summary>
    /// <param name="id">What the script calls it.</param>
    public static bool Exists(string id) => All.Contains(id ?? string.Empty);
}
