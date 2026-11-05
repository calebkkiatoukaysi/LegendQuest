using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// What a support does at the end of a round: who it lands on, the damage it rolls, and the
/// condition it puts on. It always lands.
/// </summary>
/// <param name="Text">What its banner says it does.</param>
/// <param name="Target">Who it lands on.</param>
/// <param name="Damage">The damage it rolls on each of them, or None.</param>
/// <param name="Type">The kind of damage.</param>
/// <param name="Condition">A condition it puts on each of them, or null.</param>
/// <param name="Turns">How many of the bearer's turns the condition lasts.</param>
/// <param name="Foe">For SupportTarget.Foe, the foe it's aimed at, which has to be one of this fight's; null otherwise.</param>
public sealed record SupportAbility(string Text, SupportTarget Target, DiceExpression Damage, DamageType Type,
    ConditionId? Condition = null, int Turns = 0, string Foe = null);
