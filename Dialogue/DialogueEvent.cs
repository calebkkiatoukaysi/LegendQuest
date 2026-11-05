using LegendQuest.Combat;
using LegendQuest.Rules;

namespace LegendQuest.Dialogue;

/// <summary>
/// Something a conversation did, for the screen to show or play.
/// </summary>
/// <param name="Kind">What happened.</param>
/// <param name="Id">The flag, meter, item, origin, objective, "map spawn", support, enemy or guest it was about.</param>
/// <param name="Amount">How much a meter, the gold or the experience moved; for Recruited, where they went: 1 into the party, 0 to wait at camp, -1 away; for LevelUp, the level; for a FightOrder adding foes, how many.</param>
/// <param name="Check">The check that was rolled, for a Check event.</param>
public sealed record DialogueEvent(DialogueEventKind Kind, string Id, int Amount = 0, CheckResult Check = null)
{
    /// <summary>For a FightOrder event, the change to hand CombatEngine.Apply; null for any other.</summary>
    public FightOrder Order { get; init; }
}
