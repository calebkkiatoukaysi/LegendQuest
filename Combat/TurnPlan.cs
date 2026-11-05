namespace LegendQuest.Combat;

/// <summary>
/// What a click would do on your turn, worked out before you click: Shift to a slot, or use an
/// ability on someone, or why it can't be done.
/// </summary>
/// <remarks>
/// The explore screen works one out for whatever the mouse is on, draws it (the plates it lights,
/// the chevron, the rings, the card with the odds) and carries it out on the click, so what's shown
/// is exactly what happens.
/// </remarks>
/// <param name="Ability">What would be used, or null for a Shift.</param>
/// <param name="Target">At whom, or null.</param>
/// <param name="Shift">The slot a Shift would go to, or null for an ability.</param>
/// <param name="Problem">Why it can't be done, in a few words, or null if it can.</param>
public sealed record TurnPlan(AbilityDefinition Ability, Combatant Target, Slot? Shift, string Problem);
