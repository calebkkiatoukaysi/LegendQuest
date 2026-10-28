namespace LegendQuest;

/// <summary>
/// What a condition on a line or an option in a conversation looks at.
/// </summary>
public enum ConditionKind
{
    /// <summary>A flag is set.</summary>
    Flag,

    /// <summary>A flag isn't set.</summary>
    NotFlag,

    /// <summary>A meter is at or over a number.</summary>
    MeterAtLeast,

    /// <summary>A meter is under a number.</summary>
    MeterBelow,

    /// <summary>You talk in a particular voice.</summary>
    Voice,

    /// <summary>Someone's in the party.</summary>
    InParty,

    /// <summary>You're a particular class.</summary>
    Class,
}
