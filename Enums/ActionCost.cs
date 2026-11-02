namespace LegendQuest;

/// <summary>
/// What part of a turn an ability spends: the circle, the triangle, the reaction, or nothing.
/// </summary>
public enum ActionCost
{
    /// <summary>The turn's one action.</summary>
    Action,

    /// <summary>The turn's one bonus action.</summary>
    BonusAction,

    /// <summary>The one reaction a round, taken on somebody else's turn as often as your own.</summary>
    Reaction,

    /// <summary>Costs nothing: always on, or part of something else.</summary>
    Free,
}
