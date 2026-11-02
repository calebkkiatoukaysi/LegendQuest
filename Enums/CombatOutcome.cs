namespace LegendQuest;

/// <summary>
/// How a fight stands.
/// </summary>
public enum CombatOutcome
{
    /// <summary>Still going.</summary>
    Ongoing,

    /// <summary>Every foe is dead, beaten, or has given up, and no more are coming.</summary>
    Victory,

    /// <summary>Everyone on the party's side is down or dead.</summary>
    Defeat,
}
