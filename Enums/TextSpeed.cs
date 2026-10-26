namespace LegendQuest;

/// <summary>
/// How fast lines of dialogue type out. Picked on the options screen.
/// </summary>
/// <remarks>Saved in settings.json by name, not number, so reordering here is safe but renaming one puts it back to Normal.</remarks>
public enum TextSpeed
{
    /// <summary>Slow enough to read along as it types.</summary>
    Slow,

    /// <summary>The default.</summary>
    Normal,

    /// <summary>For when you read faster than it types.</summary>
    Fast,

    /// <summary>The whole line at once.</summary>
    Instant,
}
