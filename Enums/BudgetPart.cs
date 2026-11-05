namespace LegendQuest;

/// <summary>
/// A part of the turn's budget over the fight's hotbar, for the card that says what it is when it's
/// pointed at.
/// </summary>
/// <remarks>In the order they're drawn: the action and the bonus action on top, the reaction and the Shift under them.</remarks>
public enum BudgetPart
{
    /// <summary>The action's circle.</summary>
    Action,

    /// <summary>The bonus action's triangle.</summary>
    Bonus,

    /// <summary>The reaction.</summary>
    Reaction,

    /// <summary>The Shift's two arrows: once a turn, free, a step to another slot.</summary>
    Shift,
}
