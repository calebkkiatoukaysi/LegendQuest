namespace LegendQuest.Combat;

/// <summary>
/// Someone who fights in a slot on the party's side without being in the party: a companion before
/// they've joined, or anyone from a stat block. The AI plays them, and at 0 hit points they fall
/// back out of the fight, unhurt for the story.
/// </summary>
/// <param name="Id">Who: an origin id, or an EnemyCatalog id.</param>
/// <param name="Row">The row they'd rather stand in.</param>
/// <param name="FromRound">The round they join at the start of: 1 to be there from the beginning.</param>
/// <param name="When">A story flag they only come if it's set, or null for always.</param>
public sealed record EncounterGuest(string Id, Row Row = Row.Front, int FromRound = 1, string When = null)
{
    /// <summary>The guest's id, for code from before guests were more than an id.</summary>
    /// <remarks>M4 transition only, so the grid's screen and simulator build until they read Id. Nothing new should lean on it.</remarks>
    /// <param name="guest">The guest.</param>
    public static implicit operator string(EncounterGuest guest) => guest?.Id;
}
