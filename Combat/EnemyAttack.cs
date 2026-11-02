using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// One attack from a foe's stat block, written the way the SRD's are: "Bite. +4 to hit, reach
/// 1.5 m. 2d4+2 piercing."
/// </summary>
/// <param name="Name">What it's called in the log.</param>
/// <param name="Bonus">What it adds to the d20.</param>
/// <param name="Damage">What it does on a hit, bonus and all.</param>
/// <param name="DamageType">The kind of damage.</param>
/// <param name="Range">Its reach or range, in metres: 1.5 for most melee.</param>
/// <param name="Properties">Finesse for Sneak Attack, Ranged for the ranged rules; usually None.</param>
public sealed record EnemyAttack(string Name, int Bonus, DiceExpression Damage, DamageType DamageType, float Range, WeaponProperty Properties);
