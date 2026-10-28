using LegendQuest.Rules;

namespace LegendQuest.Dialogue;

/// <summary>
/// Something a conversation did, for the screen to show or play.
/// </summary>
/// <param name="Kind">What happened.</param>
/// <param name="Id">The flag, meter, item, origin, objective or "map spawn" it was about.</param>
/// <param name="Amount">How much a meter or the gold moved, or 1 if a recruit joined the party and 0 if they went to camp.</param>
/// <param name="Check">The check that was rolled, for a Check event.</param>
public sealed record DialogueEvent(DialogueEventKind Kind, string Id, int Amount = 0, CheckResult Check = null);
