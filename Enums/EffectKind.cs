namespace LegendQuest;

/// <summary>
/// What an effect in a conversation does to the game.
/// </summary>
public enum EffectKind
{
    /// <summary>Sets a flag.</summary>
    SetFlag,

    /// <summary>Clears a flag.</summary>
    ClearFlag,

    /// <summary>Adds to a meter (or takes away, with a negative amount), kept inside its range.</summary>
    AddMeter,

    /// <summary>Sets a meter to a number.</summary>
    SetMeter,

    /// <summary>Hands over gold.</summary>
    Gold,

    /// <summary>Hands over a key item.</summary>
    GiveItem,

    /// <summary>Takes a key item away.</summary>
    TakeItem,

    /// <summary>Changes what you're to do next.</summary>
    Objective,

    /// <summary>Someone joins the party, or waits at camp if it's full.</summary>
    Recruit,

    /// <summary>Picks the voice you talk in.</summary>
    Voice,

    /// <summary>Takes the party to another map once the conversation's over: the id is "map spawn".</summary>
    Travel,
}
