namespace LegendQuest.Combat;

/// <summary>
/// A support in a fight as it stands: whether it acts at the end of this round, and whether a
/// script has stood it down.
/// </summary>
/// <remarks>The engine changes it; the screens only read it, for the banner over the figure and its line in the turn order.</remarks>
public sealed class SupportState
{
    /// <summary>Brings a support into a fight, not yet acting.</summary>
    /// <param name="support">The encounter's line for it.</param>
    internal SupportState(EncounterSupport support)
    {
        Support = support;
    }

    /// <summary>Who it is and what it does.</summary>
    public EncounterSupport Support { get; }

    /// <summary>Its id, as SupportActed's actor names it.</summary>
    public string Id => Support.Id;

    /// <summary>Whether it acts at the end of this round: it has an ability, the round's from its FromRound to its UntilRound, and nothing's stood it down. Its banner's dim when it isn't.</summary>
    public bool Ready { get; internal set; }

    /// <summary>Whether a script has stood it down for the rest of the fight (support-off).</summary>
    public bool Off { get; internal set; }
}
