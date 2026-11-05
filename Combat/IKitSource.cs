using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// Where the engine gets everyone's kit from: what's on their bar, and what adds to the Focus they
/// gain each turn.
/// </summary>
/// <remarks>
/// A seam so the class kits can change under the engine without it noticing. AbilityCatalog.Interim
/// is the stand-in kits the rows are tuned on; the class kits from 10-classes.md come in through
/// here as SkillCatalog. FightRules carries it in.
/// </remarks>
public interface IKitSource
{
    /// <summary>Everything someone has in this fight, in bar order: their auto attack, their skills and what's always on, then the actions everyone shares.</summary>
    /// <param name="who">Who.</param>
    IReadOnlyList<AbilityId> Kit(Combatant who);

    /// <summary>The Focus someone gains at the start of each of their turns on top of everyone's, from what they've learnt (Arcane Recovery), or 0.</summary>
    /// <param name="who">Who.</param>
    int FocusPerTurnBonus(Combatant who);
}
