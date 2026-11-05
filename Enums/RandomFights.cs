namespace LegendQuest;

/// <summary>
/// How random fights on the road go, by difficulty (DifficultyRules.Random). A stub until random
/// fights come in with M10.
/// </summary>
/// <remarks>New ones go on the end.</remarks>
public enum RandomFights
{
    /// <summary>They can be turned off in the options, and are by default.</summary>
    Optional,

    /// <summary>They happen.</summary>
    On,

    /// <summary>They happen, and ambushes are twice as likely.</summary>
    MoreAmbushes,
}
