namespace LegendQuest;

/// <summary>
/// How a class fills its Focus in a fight, on top of the 1 a second everyone gets: 11-combat.md
/// §4.1's nine models. FocusModels has each one's numbers.
/// </summary>
/// <remarks>New ones go on the end.</remarks>
public enum FocusModel
{
    /// <summary>+8 for each of your hits, +14 on a crit.</summary>
    Momentum,

    /// <summary>+5 a hit, +16 a crit, +12 when you dodge.</summary>
    Opportunist,

    /// <summary>+6 when you're hit, +10 when you block, +4 for each of your hits.</summary>
    Bulwark,

    /// <summary>+5 a second, steadily.</summary>
    Wellspring,

    /// <summary>+3 a second, +4 for every heal you land.</summary>
    Devotion,

    /// <summary>+3 a second, +2 for every tick of your damage or healing over time.</summary>
    Wild,

    /// <summary>+6 for each hit on a target carrying your curse, +2 otherwise; +1 a second.</summary>
    Pact,

    /// <summary>Starts every fight on 80; +6 for each of your hits.</summary>
    Tempo,

    /// <summary>+4 a second; some skills cost hit points as well.</summary>
    Blood,
}
