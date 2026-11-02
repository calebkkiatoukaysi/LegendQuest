using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// One way someone can attack, worked out once: a party member's weapon with their modifiers in it,
/// or one of a foe's attacks.
/// </summary>
/// <param name="Name">What it's called: the weapon's name, or the attack's.</param>
/// <param name="Bonus">Everything added to the d20.</param>
/// <param name="Modifiers">The same, one by one, with where each comes from, for the dice and the log.</param>
/// <param name="Damage">The damage on a hit, flat bonuses and all.</param>
/// <param name="DamageType">The kind of damage it does.</param>
/// <param name="Extra">Extra damage a magic weapon does on top (the ember dagger's fire), or None.</param>
/// <param name="ExtraType">The kind of the extra damage.</param>
/// <param name="Range">How far it reaches, in metres.</param>
/// <param name="Properties">The weapon's properties: Finesse and Ranged matter to the rules here.</param>
public sealed record AttackProfile(string Name, int Bonus, IReadOnlyList<(string Source, int Amount)> Modifiers, DiceExpression Damage,
    DamageType DamageType, DiceExpression Extra, DamageType ExtraType, float Range, WeaponProperty Properties)
{
    /// <summary>Whether it's a ranged attack: disadvantage with a foe beside you, and no opportunity attacks with it.</summary>
    public bool Ranged => (Properties & WeaponProperty.Ranged) != 0;

    /// <summary>Whether Sneak Attack can ride on it: a finesse or a ranged weapon.</summary>
    public bool Sneaky => (Properties & (WeaponProperty.Finesse | WeaponProperty.Ranged)) != 0;

    /// <summary>The damage it does on average, extra included, for the AI.</summary>
    public double AverageDamage => Damage.Average + (Extra.IsNone ? 0.0 : Extra.Average);
}
