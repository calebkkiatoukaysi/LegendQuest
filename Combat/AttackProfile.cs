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
/// <param name="Reach">Which rows it reaches: Ranged for a ranged weapon, Reach for a reach weapon, Melee for the rest.</param>
/// <param name="Properties">The weapon's properties: Finesse and Ranged matter to the rules here.</param>
public sealed record AttackProfile(string Name, int Bonus, IReadOnlyList<(string Source, int Amount)> Modifiers, DiceExpression Damage,
    DamageType DamageType, DiceExpression Extra, DamageType ExtraType, Reach Reach, WeaponProperty Properties)
{
    /// <summary>Whether it's a ranged attack: Prone works the other way against it, and nobody makes an opportunity attack with it.</summary>
    public bool Ranged => (Properties & WeaponProperty.Ranged) != 0;

    /// <summary>Whether Sneak Attack can ride on it: a finesse or a ranged weapon.</summary>
    public bool Sneaky => (Properties & (WeaponProperty.Finesse | WeaponProperty.Ranged)) != 0;

    /// <summary>The damage it does on average, extra included, for the AI.</summary>
    public double AverageDamage => Damage.Average + (Extra.IsNone ? 0.0 : Extra.Average);

    /// <summary>The reach a weapon's properties give it.</summary>
    /// <param name="properties">The weapon's properties.</param>
    public static Reach ReachOf(WeaponProperty properties) =>
        (properties & WeaponProperty.Ranged) != 0 ? Reach.Ranged : (properties & WeaponProperty.Reach) != 0 ? Reach.Reach : Reach.Melee;
}
