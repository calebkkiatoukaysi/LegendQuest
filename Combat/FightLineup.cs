using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Combat;

/// <summary>
/// The party's side of a fight as FightRoster lines it up: who fights and in which slot, and who
/// sits it out.
/// </summary>
/// <param name="Setups">The party members who fight, each in a slot, then the guests who come: those there from the start in a slot, and any due later with none, for the engine to place when they join.</param>
/// <param name="SittingOut">The party members who don't fight it, in party order: whoever the fight sits out, then anyone there wasn't a slot for. The start card says who.</param>
public sealed record FightLineup(IReadOnlyList<CombatantSetup> Setups, IReadOnlyList<PartyMember> SittingOut);
