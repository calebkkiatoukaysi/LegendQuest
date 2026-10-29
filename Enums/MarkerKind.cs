namespace LegendQuest;

/// <summary>
/// The marker over someone with something for you, or over somewhere something's going to happen.
/// The value is the row on markers.png, counting None.
/// </summary>
/// <remarks>
/// Yellow for the people who give you work and see it through, red for the moments the story turns
/// on, the way the games I've played do it. A conversation says which it is with its Marker; a
/// trigger gets the red one with mark=yes.
/// </remarks>
public enum MarkerKind
{
    /// <summary>No marker.</summary>
    None,

    /// <summary>A gold "!": someone with work or a quest for you, or waiting to hear how it went.</summary>
    Quest,

    /// <summary>A red "!": something important happens here, or with them.</summary>
    Event,
}
