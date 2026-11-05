namespace LegendQuest;

/// <summary>
/// How hard the fights are, set in the options and changeable any time outside a fight.
/// DifficultyRules has what each one changes.
/// </summary>
/// <remarks>11-combat.md §6.1. New ones go on the end.</remarks>
public enum Difficulty
{
    /// <summary>For the story: weaker foes, more Focus, a full heal after a story fight, and a way past a fight lost twice.</summary>
    Story,

    /// <summary>The fights as tuned. The default.</summary>
    Balanced,

    /// <summary>Tougher foes who hit more often, less Focus, and no healing between fights.</summary>
    Tactician,
}
