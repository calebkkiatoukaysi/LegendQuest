namespace LegendQuest;

/// <summary>
/// How a d20 gets rolled: once, or twice keeping the higher or the lower.
/// </summary>
/// <remarks>
/// 5e's, from the SRD 5.1. However many reasons there are for each, any advantage and any
/// disadvantage cancel out to a straight roll; AttackMath.Combine works that out.
/// </remarks>
public enum Advantage
{
    /// <summary>One d20.</summary>
    Normal,

    /// <summary>Two d20s, keeping the higher.</summary>
    Advantage,

    /// <summary>Two d20s, keeping the lower.</summary>
    Disadvantage,
}
