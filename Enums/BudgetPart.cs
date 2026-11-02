namespace LegendQuest;

/// <summary>
/// A part of the turn's budget over the fight's hotbar, for the card that says what it is when it's
/// pointed at.
/// </summary>
/// <remarks>In the order they're drawn, left to right: the three pips go by their number.</remarks>
public enum BudgetPart
{
    /// <summary>The action's circle.</summary>
    Action,

    /// <summary>The bonus action's triangle.</summary>
    Bonus,

    /// <summary>The reaction.</summary>
    Reaction,

    /// <summary>The movement bar.</summary>
    Movement,
}
