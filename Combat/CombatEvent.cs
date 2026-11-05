using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// One thing that happened in a fight, for the screen to animate, sound and log as it shows it.
/// </summary>
/// <remarks>
/// The Black Box's round lines, with more on them. The engine has already done it all by the time
/// the screen sees one, so the screen can take as long as it likes over each. Ids rather than
/// Combatants, so an event still means something after the fight.
/// </remarks>
/// <param name="Kind">What happened.</param>
/// <param name="Actor">The id of whoever did it (a support's id for SupportActed), or null.</param>
/// <param name="Target">The id of whoever it happened to, or null.</param>
/// <param name="Amount">Its number: damage, healing, a total, a round; what it is depends on Kind.</param>
/// <param name="Text">The whole line for the combat log, every roll written out, or null when the line before already said it (a Hit after its AttackRolled).</param>
/// <param name="Roll">The dice behind it, for the dice on screen, or null.</param>
/// <param name="From">Where it came from: a Shift's start, the actor's slot. Null for nowhere in particular.</param>
/// <param name="To">Where it went: a Shift's end, the target's slot. Null for nowhere in particular.</param>
/// <param name="Ability">The ability it came from, or null.</param>
/// <param name="Type">The kind of damage, for the right sparks and sound, or null.</param>
public sealed record CombatEvent(CombatEventKind Kind, string Actor, string Target, int Amount, string Text, RollResult Roll,
    Slot? From, Slot? To, AbilityId? Ability, DamageType? Type);
