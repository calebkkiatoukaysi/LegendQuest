using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// The foes a fight starts with, and the ids the fight knows them by. BattleRoster lines the rest of
/// a fight up (the party and its guests) and takes its foes from here, for the game and the simulator.
/// </summary>
/// <remarks>
/// 11-combat.md §2.1. Each foe keeps the slot its encounter gives it, which only says where it stands
/// when it isn't standing in the room already (BattleArena.FoeStart).
/// </remarks>
public static class FightRoster
{
    /// <summary>The foes a fight starts with, each in its slot, by the ids the fight knows them by.</summary>
    /// <param name="encounter">The fight.</param>
    public static IReadOnlyList<CombatantSetup> Foes(EncounterDefinition encounter)
    {
        var setups = new List<CombatantSetup>();
        for (int i = 0; i < encounter.Foes.Count; i++)
        {
            EncounterFoe foe = encounter.Foes[i];
            setups.Add(CombatantSetup.ForFoe(foe, FoeId(foe, i)));
        }
        return setups;
    }

    /// <summary>The id a fight knows one of its starting foes by: the map npc's, or for one with none, its stat block's and its place in the list from 1 (hollowed-footman-2).</summary>
    /// <param name="foe">The foe.</param>
    /// <param name="index">Its place in the encounter's Foes, from 0.</param>
    public static string FoeId(EncounterFoe foe, int index) => foe.Npc ?? $"{foe.Enemy}-{index + 1}";
}
