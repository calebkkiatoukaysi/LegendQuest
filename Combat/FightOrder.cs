namespace LegendQuest.Combat;

/// <summary>
/// A change to a fight that's going, from a conversation played in the middle of it: one for each
/// support-off, fight-add and fight-guest, which the screen hands to CombatEngine.Apply once the
/// conversation's over.
/// </summary>
/// <param name="Kind">What changes.</param>
/// <param name="Id">The support's id for SupportOff, the EnemyCatalog id for AddFoes, the guest's id for AddGuest.</param>
/// <param name="Count">How many foes, for AddFoes; ignored otherwise.</param>
public sealed record FightOrder(FightOrderKind Kind, string Id, int Count);
