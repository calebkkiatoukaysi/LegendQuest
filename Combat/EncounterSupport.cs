using System;

namespace LegendQuest.Combat;

/// <summary>
/// Someone who helps from behind the rows without standing in them: archers on a wall, a crew with
/// boat-hooks. Nobody can hit them, and they use one fixed ability at the end of every round.
/// </summary>
/// <param name="Id">What the fight and the scripts call it (support-off).</param>
/// <param name="Name">What the turn order calls it.</param>
/// <param name="Figure">Who's drawn for it behind the party, by id: a room npc, one of the Cast, an origin, an enemy or a look.</param>
/// <param name="Ability">What it does at the end of each round, or null for nothing: a figure the story needs there.</param>
/// <param name="FromRound">The first round it acts at the end of.</param>
/// <param name="UntilRound">The last round it acts at the end of, or 0 for every round on.</param>
/// <param name="When">A story flag it only comes if it's set, or null for always.</param>
/// <param name="Progress">Whether its banner shows, as a ring, how far the fight is through its EndsAfterRounds.</param>
public sealed record EncounterSupport(string Id, string Name, string Figure, SupportAbility Ability,
    int FromRound = 1, int UntilRound = 0, string When = null, bool Progress = false)
{
    /// <summary>In real time, the second it first acts at; without one, (FromRound - 1) × 6, or 6 for a support there from the first round (11-combat.md §5.4).</summary>
    public int FromSeconds { get; init; } = -1;

    /// <summary>In real time, how often it acts, in seconds: every 6 unless it says otherwise.</summary>
    public int EverySeconds { get; init; } = EncounterDefinition.SecondsPerRound;

    /// <summary>The second it first acts at in real time.</summary>
    public int StartsAtSeconds => FromSeconds >= 0 ? FromSeconds : Math.Max(1, FromRound - 1) * EncounterDefinition.SecondsPerRound;

    /// <summary>The last second it acts at in real time, or 0 for every one on.</summary>
    public int StopsAtSeconds => UntilRound == 0 ? 0 : UntilRound * EncounterDefinition.SecondsPerRound;
}
