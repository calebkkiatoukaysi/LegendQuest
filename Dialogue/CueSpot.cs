namespace LegendQuest.Dialogue;

/// <summary>
/// Where a place cue puts someone, in the script's words: at a spawn, beside someone, on, before
/// or over a prop or someone, or a free spot in the crowd.
/// </summary>
/// <remarks>Only words. Whatever plays the cue works out where that is on its own floor.</remarks>
/// <param name="Kind">Which of them.</param>
/// <param name="Name">The spawn, or whoever or whatever it's by. Null for the crowd.</param>
public readonly record struct CueSpot(CueSpotKind Kind, string Name)
{
    /// <summary>At a spawn.</summary>
    /// <param name="spawn">The spawn's name.</param>
    public static CueSpot At(string spawn) => new(CueSpotKind.Spawn, spawn);

    /// <summary>A free spot in the crowd.</summary>
    public static CueSpot Crowd => new(CueSpotKind.Crowd, null);
}

/// <summary>The kinds of place a place cue can put someone.</summary>
public enum CueSpotKind
{
    /// <summary>At a spawn, by its name.</summary>
    Spawn,

    /// <summary>Next to someone, the side there's room on.</summary>
    Beside,

    /// <summary>Right where a prop or someone stands.</summary>
    On,

    /// <summary>In front of a prop or someone.</summary>
    Before,

    /// <summary>Behind a prop or someone, over them.</summary>
    Over,

    /// <summary>A free spot among whoever's standing about, near the party.</summary>
    Crowd,
}
