using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Attacks and saves: the d20 with everything that gives advantage or disadvantage, hits and
/// criticals, the damage and what rides on it, and the odds of all of it for the tooltip and the AI.
/// </summary>
/// <remarks>
/// 5e's from the SRD 5.1. Advantage comes from help, a restrained or prone target, a Downed one within
/// reach, and Pack Tactics; disadvantage from a dodging target, being restrained, prone, mocked or in
/// armour you aren't trained for, a ranged attack with a foe beside you, and a Warden's shield. Any
/// of each cancels out (AttackMath.Combine). A hit on someone Downed from beside them is a critical.
/// Every roll is written out in full for the log.
/// </remarks>
public sealed partial class CombatEngine
{
    /// <summary>One part of a hit's damage: its dice, its kind, and what it is when it isn't the weapon's own ("sneak attack").</summary>
    private readonly record struct DamagePart(DiceExpression Dice, DamageType Type, string Note);

    /// <summary>The Attack action: a party member's one swing, or a foe's best attack, or every one of its attacks with Multiattack.</summary>
    /// <remarks>With Multiattack, an attack whose target has fallen, or is out of its reach, goes to the weakest foe it can reach instead.</remarks>
    private void WeaponAction(Combatant user, AbilityDefinition ability, Combatant target)
    {
        if (user.Member is not null || !Has(user, AbilityId.Multiattack))
        {
            AttackProfile weapon = user.Member is not null ? user.MainHand : BestAttack(user, target, user.Cell) ?? user.MainHand;
            Strike(user, weapon, target, ability, false);
            return;
        }

        foreach (AttackProfile attack in user.Attacks)
        {
            if (Outcome != CombatOutcome.Ongoing) return;
            Combatant aim = Aimable(user, attack, target) ? target : Weakest(user, attack);
            if (aim is not null) Strike(user, attack, aim, ability, false);
        }
    }

    /// <summary>One attack with a weapon (or a foe's attack): the roll, the damage, and a trip if they can.</summary>
    /// <param name="attacker">Who.</param>
    /// <param name="weapon">With what.</param>
    /// <param name="target">At whom.</param>
    /// <param name="ability">The ability it's part of, for the events.</param>
    /// <param name="offHand">Whether it's the off hand's.</param>
    private void Strike(Combatant attacker, AttackProfile weapon, Combatant target, AbilityDefinition ability, bool offHand)
    {
        var damage = new List<DamagePart> { new(weapon.Damage, weapon.DamageType, null) };
        if (!weapon.Extra.IsNone) damage.Add(new DamagePart(weapon.Extra, weapon.ExtraType, null));

        string action = $"{attacker.Label} attacks {target.Label}{(offHand ? " off-hand" : string.Empty)}";
        bool hit = ResolveAttack(attacker, target, ability, weapon.Bonus, weapon.Modifiers, weapon.Ranged, weapon.Sneaky, damage, action, weapon.DamageType);
        if (hit && !weapon.Ranged && !target.IsOut && Has(attacker, AbilityId.Trip)) Trip(attacker, target);
    }

    /// <summary>Something from the bag thrown at a foe: a ranged attack with Dexterity alone, since a flask is nobody's weapon.</summary>
    private void Throw(Combatant user, AbilityDefinition ability, Combatant target)
    {
        ItemDefinition item = ItemCatalog.Get(ability.Item);
        int dexterity = user.Scores.Modifier(Ability.Dexterity);
        var damage = new[] { new DamagePart(item.ThrownDamage, item.ThrownDamageType, null) };
        ResolveAttack(user, target, ability, dexterity, new[] { ("DEX", dexterity) }, true, false, damage,
            $"{user.Label} throws {item.Name.ToUpperInvariant()} at {target.Label}", item.ThrownDamageType);
    }

    /// <summary>A spell attack: proficiency and the casting modifier (and anything worn that adds to spell attacks), and the spell's conditions on a hit.</summary>
    private void SpellAttack(Combatant user, AbilityDefinition ability, Combatant target)
    {
        var damage = new[] { new DamagePart(ability.Damage, ability.DamageType, null) };
        bool ranged = ability.Range > Measure.MeleeReach;
        bool hit = ResolveAttack(user, target, ability, user.SpellAttack, SpellModifiers(user), ranged, false, damage,
            $"{user.Label} casts {ability.Name.ToUpperInvariant()} at {target.Label}", ability.DamageType);

        if (!hit || target.IsOut) return;
        foreach (ConditionId condition in ability.Conditions) AddCondition(target, condition, ability.Turns, user, ability.Id);
    }

    /// <summary>A save ability: everyone it catches saves against the DC, and takes its damage (once rolled for all of them) and its conditions if they fail.</summary>
    private void SavingThrows(Combatant user, AbilityDefinition ability, Combatant target, Point cell)
    {
        var caught = new List<Combatant>();
        if (ability.Target == TargetKind.Enemy) caught.Add(target);
        else
        {
            Point centre = ability.Target == TargetKind.Self ? user.Cell : cell;
            foreach (Combatant each in _combatants)
            {
                bool self = each == user && ability.Target == TargetKind.Self;
                if (!self && !Beaten(each) && !each.IsOut && Measure.Within(each.Cell, centre, ability.Radius)) caught.Add(each);
            }
        }

        string name = ability.Name.ToUpperInvariant();
        string on = ability.Target == TargetKind.Enemy ? $" on {target.Label}" : string.Empty;
        Emit(CombatEventKind.AbilityUsed, user, ability.Target == TargetKind.Enemy ? target : null, 0,
            $"{user.Label} {(ability.Spell ? "casts" : "uses")} {name}{on}.", ability: ability.Id);

        int dc = ability.Dc > 0 ? ability.Dc : user.SpellDc;
        RollResult rolled = ability.Damage.IsNone ? null : ability.Damage.Roll(_random, name);
        int full = rolled is null ? 0 : Math.Max(0, rolled.Total);
        string kind = ability.DamageType.ToString().ToLowerInvariant();

        foreach (Combatant each in caught)
        {
            bool saved = RollSave(each, ability.Save ?? Ability.Dexterity, dc, name, out RollResult roll, out string line);
            int damage = rolled is null ? 0 : !saved ? full : ability.SaveHalves ? full / 2 : 0;
            if (rolled is not null && (!saved || ability.SaveHalves))
                line += saved ? $" {ability.Damage} = {full} {kind}, halved to {damage}." : $" {ability.Damage} = {full} {kind}.";

            Emit(CombatEventKind.SaveRolled, user, each, roll.Total, line, roll, ability.Id);
            if (damage > 0) Damage(each, damage, ability.DamageType, user, false, ability.Id, rolled);
            if (saved || each.IsOut) continue;
            foreach (ConditionId condition in ability.Conditions) AddCondition(each, condition, ability.Turns, user, ability.Id, dc);
        }
    }

    /// <summary>Darts that never miss, each rolling its own damage; any left when the target falls fly on to the nearest foe in sight.</summary>
    private void Darts(Combatant user, AbilityDefinition ability, Combatant target)
    {
        string name = ability.Name.ToUpperInvariant();
        string kind = ability.DamageType.ToString().ToLowerInvariant();
        Emit(CombatEventKind.AbilityUsed, user, target, 0, $"{user.Label} casts {name}.", ability: ability.Id);

        Combatant aim = target;
        for (int dart = 0; dart < ability.Darts && Outcome == CombatOutcome.Ongoing; dart++)
        {
            if (aim is null || aim.IsOut || Beaten(aim)) aim = NearestInSight(user, RangeOf(user, ability));
            if (aim is null) return;

            RollResult roll = ability.Damage.Roll(_random, name);
            int amount = Math.Max(0, roll.Total);
            Emit(CombatEventKind.Hit, user, aim, amount, $"A dart strikes {aim.Label}: {ability.Damage} = {amount} {kind}.", roll, ability.Id, ability.DamageType);
            Damage(aim, amount, ability.DamageType, user, false, ability.Id, roll);
        }
    }

    /// <summary>
    /// Rolls an attack and, on a hit, its damage: Sneak Attack and Hex on top where they apply, doubled
    /// dice on a critical, and the whole line for the log on one event.
    /// </summary>
    /// <returns>Whether it hit.</returns>
    private bool ResolveAttack(Combatant attacker, Combatant target, AbilityDefinition ability, int bonus,
        IReadOnlyList<(string Source, int Amount)> modifiers, bool ranged, bool sneaky, IReadOnlyList<DamagePart> damage, string action, DamageType kind)
    {
        Advantage advantage = AdvantageOf(attacker, target, attacker.Cell, ranged, true, out Combatant protector);
        if (protector is not null)
        {
            protector.Budget.Spend(ActionCost.Reaction);
            Emit(CombatEventKind.Log, protector, target, 0, $"{protector.Label} raises a shield over {target.Label}: disadvantage.", ability: AbilityId.Protection);
        }

        int kept = AttackMath.D20(_random, advantage, out int first, out int second);
        var rolledWith = new List<(string Source, int Amount)>(modifiers);
        int total = kept + bonus;
        bool hit = AttackMath.Hits(kept, bonus, target.ArmourClass);
        string lifted = string.Empty;

        if (!hit && kept != AttackMath.FumbleRoll && attacker.Find(ConditionId.Inspired) is ActiveCondition inspired
            && target.ArmourClass - total <= inspired.Die.Max)
        {
            int lift = inspired.Die.Roll(_random, "INSPIRATION").Total;
            rolledWith.Add(("INSPIRATION", lift));
            total += lift;
            lifted = $" {CombatText.Signed(lift)} inspiration";
            hit = total >= target.ArmourClass;
            RemoveCondition(attacker, ConditionId.Inspired);
        }

        bool critical = hit && (kept == AttackMath.CriticalRoll || target.IsDown && Measure.Within(attacker.Cell, target.Cell, Measure.MeleeReach));
        var d20 = new RollResult { Dice = Rolled(advantage, first, second), Modifiers = rolledWith, Total = total };
        string verdict = critical ? "CRITICAL HIT" : hit ? "HIT" : "MISS";
        string line = $"{action}{CombatText.AdvantageNote(advantage, first, second)}: {kept} {CombatText.Signed(bonus)}{lifted} = {total} vs AC {target.ArmourClass} - {verdict}.";

        if (!hit)
        {
            Emit(CombatEventKind.AttackRolled, attacker, target, total, line, d20, ability.Id);
            Emit(CombatEventKind.Missed, attacker, target, 0, null, null, ability.Id);
            return false;
        }

        var parts = new List<DamagePart>(damage);
        if (sneaky && SneakAttackApplies(attacker, target, advantage))
        {
            attacker.SneakTurn = _turnNumber;
            parts.Add(new DamagePart(AbilityCatalog.Get(AbilityId.SneakAttack).Damage, kind, "sneak attack"));
        }
        if (target.Find(ConditionId.Hexed) is { } hex && hex.Source == attacker.Id)
            parts.Add(new DamagePart(hex.Die, AbilityCatalog.Get(hex.From ?? AbilityId.Hex).DamageType, "hex"));

        int amount = RollDamage(parts, critical, out RollResult rolled, out string written);
        Emit(CombatEventKind.AttackRolled, attacker, target, total, $"{line} {written}.", d20, ability.Id);
        Emit(critical ? CombatEventKind.Critical : CombatEventKind.Hit, attacker, target, amount, null, null, ability.Id, kind);
        Damage(target, amount, kind, attacker, critical, ability.Id, rolled);
        return true;
    }

    /// <summary>Rolls every part of a hit's damage, a critical doubling the dice, and writes it out: "1d8+3 = 9 piercing, 1d6 = 4 piercing (sneak attack)".</summary>
    /// <returns>The total.</returns>
    private int RollDamage(IReadOnlyList<DamagePart> parts, bool critical, out RollResult rolled, out string written)
    {
        var dice = new List<int>();
        var modifiers = new List<(string Source, int Amount)>();
        var words = new List<string>();
        int total = 0;

        foreach (DamagePart part in parts)
        {
            if (part.Dice.IsNone) continue;
            RollResult roll = part.Dice.Roll(_random, part.Note ?? "DAMAGE", critical);
            int amount = Math.Max(0, roll.Total);
            dice.AddRange(roll.Dice);
            modifiers.AddRange(roll.Modifiers);
            total += amount;

            DiceExpression shown = critical ? part.Dice with { Count = part.Dice.Count * 2 } : part.Dice;
            string note = part.Note is null ? string.Empty : $" ({part.Note})";
            words.Add($"{shown} = {amount} {part.Type.ToString().ToLowerInvariant()}{note}");
        }

        rolled = new RollResult { Dice = dice, Modifiers = modifiers, Total = total };
        written = string.Join(", ", words);
        return total;
    }

    /// <summary>A trip: a Strength save against the attacker's Trip, or Prone.</summary>
    private void Trip(Combatant attacker, Combatant target)
    {
        if (target.Has(ConditionId.Prone)) return;
        AbilityDefinition trip = AbilityCatalog.Get(AbilityId.Trip);
        bool saved = RollSave(target, trip.Save ?? Ability.Strength, trip.Dc, trip.Name.ToUpperInvariant(), out RollResult roll, out string line);
        Emit(CombatEventKind.SaveRolled, attacker, target, roll.Total, line, roll, AbilityId.Trip);
        if (!saved) AddCondition(target, ConditionId.Prone, 0, attacker, AbilityId.Trip);
    }

    /// <summary>Rolls a saving throw: d20 + the save bonus against a DC, with advantage or disadvantage from conditions, and Bardic Inspiration if it'd turn a failure round.</summary>
    /// <param name="who">Who's saving.</param>
    /// <param name="ability">Which save.</param>
    /// <param name="dc">The DC.</param>
    /// <param name="against">What it's against, for the log: "ENTANGLE".</param>
    /// <param name="result">The dice.</param>
    /// <param name="line">The log line, without any damage.</param>
    /// <returns>Whether they made it.</returns>
    private bool RollSave(Combatant who, Ability ability, int dc, string against, out RollResult result, out string line)
    {
        int bonus = who.SaveBonus(ability);
        Advantage advantage = SaveAdvantage(who, ability);
        int kept = AttackMath.D20(_random, advantage, out int first, out int second);
        int total = kept + bonus;
        string abbreviation = CombatStats.Abbreviation(ability);
        var modifiers = new List<(string Source, int Amount)> { (abbreviation, bonus) };
        string lifted = string.Empty;

        if (total < dc && who.Find(ConditionId.Inspired) is ActiveCondition inspired && dc - total <= inspired.Die.Max)
        {
            int lift = inspired.Die.Roll(_random, "INSPIRATION").Total;
            modifiers.Add(("INSPIRATION", lift));
            total += lift;
            lifted = $" {CombatText.Signed(lift)} inspiration";
            RemoveCondition(who, ConditionId.Inspired);
        }

        bool saved = total >= dc;
        result = new RollResult { Dice = Rolled(advantage, first, second), Modifiers = modifiers, Total = total };
        line = $"{who.Label} makes a DC {dc} {abbreviation} save against {against}{CombatText.AdvantageNote(advantage, first, second)}: "
            + $"{kept} {CombatText.Signed(bonus)}{lifted} = {total} - {(saved ? "SAVES" : "FAILS")}.";
        return saved;
    }

    /// <summary>Advantage or disadvantage on a save: dodging helps a Dexterity save, being restrained hurts one, and armour you aren't trained for hurts Strength and Dexterity.</summary>
    private static Advantage SaveAdvantage(Combatant who, Ability ability)
    {
        int advantages = ability == Ability.Dexterity && who.Has(ConditionId.Dodging) ? 1 : 0;
        int disadvantages = ability == Ability.Dexterity && who.Has(ConditionId.Restrained) ? 1 : 0;
        if (who.ArmourUntrained && ability is Ability.Strength or Ability.Dexterity) disadvantages++;
        return AttackMath.Combine(advantages, disadvantages);
    }

    /// <summary>Everything that gives an attack advantage or disadvantage, from where the attacker would be standing.</summary>
    /// <param name="attacker">Who.</param>
    /// <param name="target">At whom.</param>
    /// <param name="from">The tile they'd attack from.</param>
    /// <param name="ranged">Whether it's a ranged attack.</param>
    /// <param name="consume">Whether to use up what's spent by attacking (Helped, Mocked), which only the real attack does.</param>
    /// <param name="protector">A Warden whose shield would turn it to disadvantage, or null.</param>
    private Advantage AdvantageOf(Combatant attacker, Combatant target, Point from, bool ranged, bool consume, out Combatant protector)
    {
        bool close = Measure.Within(from, target.Cell, Measure.MeleeReach);
        int advantages = 0, disadvantages = 0;

        if (attacker.Has(ConditionId.Helped)) advantages++;
        if (target.Has(ConditionId.Restrained)) advantages++;
        if ((target.Has(ConditionId.Prone) || target.IsDown) && close) advantages++;
        if (Has(attacker, AbilityId.PackTactics) && AllyBeside(attacker, target)) advantages++;

        if (target.Has(ConditionId.Prone) && !close) disadvantages++;
        if (target.Has(ConditionId.Dodging) && !target.IsOut) disadvantages++;
        if (attacker.Has(ConditionId.Restrained) || attacker.Has(ConditionId.Prone) || attacker.Has(ConditionId.Mocked)) disadvantages++;
        if (attacker.ArmourUntrained) disadvantages++;
        if (ranged && HostileBeside(attacker, from)) disadvantages++;

        protector = disadvantages == 0 ? Protector(attacker, target) : null;
        if (protector is not null) disadvantages++;

        if (consume)
        {
            RemoveCondition(attacker, ConditionId.Helped);
            RemoveCondition(attacker, ConditionId.Mocked);
        }
        return AttackMath.Combine(advantages, disadvantages);
    }

    /// <summary>A Warden beside the target with a shield up, a reaction to spend and the attacker in sight, who'd turn the attack to disadvantage; or null.</summary>
    private Combatant Protector(Combatant attacker, Combatant target)
    {
        foreach (Combatant each in _combatants)
        {
            if (each == target || Hostile(each, target) || each.IsOut || !each.Budget.Reaction || !Has(each, AbilityId.Protection)) continue;
            if (each.Member?.WornIn(EquipmentSlot.OffHand) is not { Kind: ItemKind.Shield }) continue;
            if (Measure.Within(each.Cell, target.Cell, Measure.MeleeReach) && CanSee(each.Cell, attacker.Cell)) return each;
        }
        return null;
    }

    /// <summary>Whether Sneak Attack rides on a hit: the attacker has it, hasn't used it this turn, and has advantage, or an ally beside the target and no disadvantage.</summary>
    private bool SneakAttackApplies(Combatant attacker, Combatant target, Advantage advantage) =>
        Has(attacker, AbilityId.SneakAttack) && attacker.SneakTurn != _turnNumber
        && (advantage == Advantage.Advantage || advantage == Advantage.Normal && AllyBeside(attacker, target));

    /// <summary>Whether someone on the attacker's side, up and not the attacker, is beside the target.</summary>
    private bool AllyBeside(Combatant attacker, Combatant target)
    {
        foreach (Combatant each in _combatants)
            if (each != attacker && each.Side == attacker.Side && !each.IsOut && Measure.Within(each.Cell, target.Cell, Measure.MeleeReach)) return true;
        return false;
    }

    /// <summary>Whether a foe of someone's, still fighting, is beside a tile.</summary>
    internal bool HostileBeside(Combatant who, Point cell)
    {
        foreach (Combatant each in _combatants)
            if (Hostile(each, who) && !Beaten(each) && Measure.Within(each.Cell, cell, Measure.MeleeReach)) return true;
        return false;
    }

    /// <summary>The foe's attack that does most against a target from a tile, of the ones that reach it; null if none does.</summary>
    internal AttackProfile BestAttack(Combatant user, Combatant target, Point from)
    {
        AttackProfile best = null;
        double most = -1.0;
        foreach (AttackProfile attack in user.Attacks)
        {
            if (!Measure.Within(from, target.Cell, attack.Range)) continue;
            Advantage advantage = AdvantageOf(user, target, from, attack.Ranged, false, out _);
            double expected = AttackMath.HitChance(attack.Bonus, target.ArmourClass, advantage) * attack.AverageDamage;
            if (expected <= most) continue;
            most = expected;
            best = attack;
        }
        return best;
    }

    /// <summary>Whether a target can be hit by an attack from where the user stands: still in the fight, in reach, in sight.</summary>
    private bool Aimable(Combatant user, AttackProfile attack, Combatant target) =>
        target is not null && !target.IsDead && !(target.Side == Side.Foes && Beaten(target)) && !(target.IsDown && _encounter.Nonlethal)
        && Measure.Within(user.Cell, target.Cell, attack.Range) && CanSee(user.Cell, target.Cell);

    /// <summary>The standing foe with the fewest hit points that an attack can reach, for a Multiattack's follow-ups; null if none.</summary>
    private Combatant Weakest(Combatant user, AttackProfile attack)
    {
        Combatant weakest = null;
        foreach (Combatant each in _combatants)
            if (Hostile(each, user) && !each.IsOut && Aimable(user, attack, each) && (weakest is null || each.Hp < weakest.Hp)) weakest = each;
        return weakest;
    }

    /// <summary>The nearest foe still fighting that someone can see within a range; null if none.</summary>
    private Combatant NearestInSight(Combatant user, float range)
    {
        Combatant nearest = null;
        foreach (Combatant each in _combatants)
        {
            if (!Hostile(each, user) || each.IsOut || Beaten(each) || !Measure.Within(user.Cell, each.Cell, range) || !CanSee(user.Cell, each.Cell)) continue;
            if (nearest is null || Measure.Steps(user.Cell, each.Cell) < Measure.Steps(user.Cell, nearest.Cell)) nearest = each;
        }
        return nearest;
    }

    /// <summary>The modifiers on a spell attack, for the dice and the log: the casting ability, proficiency, and anything worn.</summary>
    private static List<(string Source, int Amount)> SpellModifiers(Combatant user)
    {
        var modifiers = new List<(string Source, int Amount)>();
        if (user.Casting is Ability casting) modifiers.Add((CombatStats.Abbreviation(casting), user.CastingModifier));
        modifiers.Add(("PROF", AttackMath.ProficiencyBonus));
        if (user.SpellFocus != 0) modifiers.Add(("FOCUS", user.SpellFocus));
        return modifiers;
    }

    /// <summary>The d20s for a RollResult: one, or both with advantage or disadvantage.</summary>
    private static int[] Rolled(Advantage advantage, int first, int second) =>
        advantage == Advantage.Normal ? new[] { first } : new[] { first, second };
}
