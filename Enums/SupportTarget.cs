namespace LegendQuest;

/// <summary>
/// Who a support's ability lands on at the end of a round.
/// </summary>
/// <remarks>New ones go on the end.</remarks>
public enum SupportTarget
{
    /// <summary>Nobody: a figure there for the story's sake, with no ability (Sera at the crystal).</summary>
    None,

    /// <summary>One standing foe, picked at random.</summary>
    RandomFoe,

    /// <summary>The foe the ability names, while it stands.</summary>
    Foe,

    /// <summary>Everyone in one of the foes' rows.</summary>
    FoeRow,

    /// <summary>Every standing foe.</summary>
    AllFoes,

    /// <summary>Everyone standing in the party's front row.</summary>
    YourFrontRow,

    /// <summary>Everyone standing on the party's side.</summary>
    AllAllies,
}
