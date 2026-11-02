using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// More foes arriving partway through a fight, at the start of a round, or as soon as everyone
/// before them is beaten if that's sooner.
/// </summary>
/// <param name="Round">The round they arrive at the start of. 2 or later.</param>
/// <param name="Foes">Who arrives. Each one turns up on its tile, so none of them is a map npc.</param>
/// <param name="Bark">What the Soul-Bound shouts when they come, or null.</param>
public sealed record EncounterWave(int Round, IReadOnlyList<EncounterFoe> Foes, string Bark);
