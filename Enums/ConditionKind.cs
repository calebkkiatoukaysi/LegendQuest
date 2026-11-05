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

    /// <summary>Someone isn't in the party: at camp, away, or never recruited.</summary>
    NotInParty,

    /// <summary>One meter is over another, or at it or over it.</summary>
    MeterCompare,

    /// <summary>The story's in a particular act.</summary>
    Act,

    /// <summary>A key item's in the bag.</summary>
    HasItem,

    /// <summary>A key item isn't in the bag.</summary>
    LacksItem,

    /// <summary>At least one of several other conditions is true.</summary>
    AnyOf,
}
