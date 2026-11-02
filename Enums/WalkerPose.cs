namespace LegendQuest;

/// <summary>
/// The fighting poses a walker can strike: the paperdoll's and the creatures' action columns.
/// </summary>
/// <remarks>
/// In the sheet's column order after the walk, so each one's column is the number of walking frames
/// plus its own number: wind-up is column 5 and downed column 9. None is walking or standing as usual.
/// </remarks>
public enum WalkerPose
{
    /// <summary>No pose: standing, or walking.</summary>
    None,

    /// <summary>Drawing back before a blow: column 5.</summary>
    WindUp,

    /// <summary>The blow landing, lunging forward: column 6.</summary>
    Strike,

    /// <summary>Lifting and opening the hands for a spell: column 7.</summary>
    Cast,

    /// <summary>Recoiling from a hit: column 8.</summary>
    Hurt,

    /// <summary>Lying on the ground, feet at the anchor: column 9.</summary>
    Downed,
}
