using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// More foes arriving partway through a fight, at the start of a round, or as soon as everyone
/// before them is beaten if that's sooner.
/// </summary>
/// <param name="Round">The round they arrive at the start of. 2 or later.</param>
/// <param name="Foes">Who arrives, each in its slot. None of them is a map npc.</param>
/// <param name="Bark">What the Soul-Bound shouts when they come, or null.</param>
/// <param name="Interlude">
/// A conversation played as they come, before the round goes on, by its DialogueCatalog id, or null.
/// Its fight-add, fight-guest and support-off change the fight. A stub until the fights that use it
/// (M7): validation turns down an encounter with one.
/// </param>
public sealed record EncounterWave(int Round, IReadOnlyList<EncounterFoe> Foes, string Bark, string Interlude = null)
{
    /// <summary>In real time, the second they walk in at (sooner if the field's clear); without one, the start of their Round at 6 s a round.</summary>
    public int AtSeconds { get; init; } = -1;

    /// <summary>The second they walk in at in real time: AtSeconds, or (Round - 1) × 6.</summary>
    public int ArrivesAtSeconds => AtSeconds >= 0 ? AtSeconds : (Round - 1) * EncounterDefinition.SecondsPerRound;
}
