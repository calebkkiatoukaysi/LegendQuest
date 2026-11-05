using System;
using System.Collections.Generic;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Attacks and saves: the d20 with everything that gives advantage or disadvantage, hits and
/// criticals, the damage and what rides on it, and the odds of all of it for the tooltip and the AI.
/// </summary>
/// <remarks>
/// 5e's from the SRD 5.1, in rows. Advantage comes from help, a target that's restrained, Marked
/// (once), Stunned or Blinded, a prone or Downed one for melee and reach, and Pack Tactics (another of
/// the pack standing in its front row, and the target in the opposing front); disadvantage from a
/// dodging target, a prone one for ranged, being restrained, prone, mocked, Poisoned, Blinded,
/// Frightened or in armour you aren't trained for, and a Warden's shield next to the target. Any of
/// each cancels out (AttackMath.Combine). A melee hit on someone Downed is a critical. Sneak Attack
/// rides on advantage, or on a target an ally's hit this round, or one that's Marked or Prone. A foe
/// adds the difficulty to its attack rolls and its DCs, and one that drains Focus takes it off whoever
/// it hits. Every roll is written out in full for the log.
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
            AttackProfile weapon = user.Member is not null ? user.MainHand : BestAttack(user, target) ?? user.MainHand;
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
        bool ranged = ReachOf(user, ability) == Reach.Ranged;
        bool hit = ResolveAttack(user, target, ability, user.SpellAttack, SpellModifiers(user), ranged, false, damage,
            $"{user.Label} casts {ability.Name.ToUpperInvariant()} at {target.Label}", ability.DamageType);

        if (!hit || target.IsOut) return;
        foreach (ConditionId condition in ability.Conditions) AddCondition(target, condition, ability.Turns, user, ability.Id);
    }

    /// <summary>A save ability: everyone it catches saves against the DC, and takes its damage (once rolled for all of them) and its conditions if they fail, and a pull or push.</summary>
    /// <param name="user">Who.</param>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom: its pick.</param>
    private void SavingThrows(Combatant user, AbilityDefinition ability, Combatant target)
    {
        bool single = ability.Shape == TargetShape.Foe;
        IReadOnlyList<Combatant> caught = Caught(user, ability, target);

        string name = ability.Name.ToUpperInvariant();
        string on = single ? $" on {target.Label}" : string.Empty;
        Emit(CombatEventKind.AbilityUsed, user, single ? target : null, 0,
            $"{user.Label} {(ability.Spell ? "casts" : "uses")} {name}{on}.", ability: ability.Id);

        int dc = DcOf(user, ability);
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
            MoveRow(user, ability, each);
        }
    }

    /// <summary>The DC of a save someone's ability sets: its own, or their spell save DC, and a foe's share of the difficulty on top.</summary>
    private static int DcOf(Combatant user, AbilityDefinition ability) => (ability.Dc > 0 ? ability.Dc : user.SpellDc) + user.RollBonus;

    /// <summary>Darts that never miss, each rolling its own damage; any left when the target falls fly on to the first foe in slot order it can still reach.</summary>
    private void Darts(Combatant user, AbilityDefinition ability, Combatant target)
    {
        string name = ability.Name.ToUpperInvariant();
        string kind = ability.DamageType.ToString().ToLowerInvariant();
        Emit(CombatEventKind.AbilityUsed, user, target, 0, $"{user.Label} casts {name}.", ability: ability.Id);

        Combatant aim = target;
        for (int dart = 0; dart < ability.Count && Outcome == CombatOutcome.Ongoing; dart++)
        {
            if (aim is null || aim.IsOut || Beaten(aim)) aim = NextAim(user, ability);
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
        bool marked = target.Has(ConditionId.Marked);
        Advantage advantage = AdvantageOf(attacker, target, ranged, true, out Combatant protector);
        if (protector is not null)
        {
            protector.Budget.Spend(ActionCost.Reaction);
            Emit(CombatEventKind.Log, protector, target, 0, $"{protector.Label} raises a shield over {target.Label}: disadvantage.", ability: AbilityId.Protection);
        }

        int kept = AttackMath.D20(_random, advantage, out int first, out int second);
        var rolledWith = new List<(string Source, int Amount)>(modifiers);
        if (attacker.RollBonus != 0)
        {
            bonus += attacker.RollBonus;
            rolledWith.Add(("DIFFICULTY", attacker.RollBonus));
        }
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

        bool critical = hit && (kept == AttackMath.CriticalRoll || target.IsDown && !ranged);
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
        if (sneaky && SneakAttackApplies(attacker, target, advantage, marked))
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

        if (attacker.Enemy is { FocusDrain: > 0 } drainer && !target.IsDead && target.Focus > 0)
        {
            int drained = -target.ChangeFocus(-drainer.FocusDrain);
            Emit(CombatEventKind.FocusChanged, attacker, target, -drained, $"{attacker.Label} drains {drained} Focus from {target.Label}.");
        }
        if (!target.IsOut) MoveRow(attacker, ability, target);
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
        bool saved = RollSave(target, trip.Save ?? Ability.Strength, DcOf(attacker, trip), trip.Name.ToUpperInvariant(), out RollResult roll, out string line);
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

    /// <summary>Everything that gives an attack advantage or disadvantage.</summary>
    /// <param name="attacker">Who.</param>
    /// <param name="target">At whom.</param>
    /// <param name="ranged">Whether it's a ranged attack, for Prone.</param>
    /// <param name="consume">Whether to use up what's spent by attacking (Helped, Mocked, the target's Mark), which only the real attack does.</param>
    /// <param name="protector">A Warden whose shield would turn it to disadvantage, or null.</param>
    private Advantage AdvantageOf(Combatant attacker, Combatant target, bool ranged, bool consume, out Combatant protector)
    {
        int advantages = 0, disadvantages = 0;

        if (attacker.Has(ConditionId.Helped)) advantages++;
        if (target.Has(ConditionId.Restrained) || target.Has(ConditionId.Marked) || target.Has(ConditionId.Stunned) || target.Has(ConditionId.Blinded)) advantages++;
        if ((target.Has(ConditionId.Prone) || target.IsDown) && !ranged) advantages++;
        if (Has(attacker, AbilityId.PackTactics) && PackInFront(attacker) && Exposed(target)) advantages++;

        if (target.Has(ConditionId.Prone) && ranged) disadvantages++;
        if (target.Has(ConditionId.Dodging) && !target.IsOut) disadvantages++;
        if (attacker.Has(ConditionId.Restrained) || attacker.Has(ConditionId.Prone) || attacker.Has(ConditionId.Mocked)
            || attacker.Has(ConditionId.Poisoned) || attacker.Has(ConditionId.Blinded) || attacker.Has(ConditionId.Frightened)) disadvantages++;
        if (attacker.ArmourUntrained) disadvantages++;

        protector = disadvantages == 0 ? Protector(attacker, target) : null;
        if (protector is not null) disadvantages++;

        if (consume)
        {
            RemoveCondition(attacker, ConditionId.Helped);
            RemoveCondition(attacker, ConditionId.Mocked);
            RemoveCondition(target, ConditionId.Marked);
        }
        return AttackMath.Combine(advantages, disadvantages);
    }

    /// <summary>Whether another of someone's side is standing in their side's effective front row, for Pack Tactics.</summary>
    private bool PackInFront(Combatant who)
    {
        foreach (Combatant each in Formation.InRow(who.Side, Formation.FrontOf(who.Side)))
            if (each != who && Standing(each)) return true;
        return false;
    }

    /// <summary>A Warden next to the target with a shield up and a reaction to spend, who'd turn an attack on it to disadvantage; or null.</summary>
    private Combatant Protector(Combatant attacker, Combatant target)
    {
        foreach (Combatant each in _combatants)
        {
            if (each == target || Hostile(each, target) || each.IsOut || !each.Budget.Reaction || !Has(each, AbilityId.Protection)) continue;
            if (each.Member?.WornIn(EquipmentSlot.OffHand) is not { Kind: ItemKind.Shield }) continue;
            if (Contains(Formation.NextTo(target), each)) return each;
        }
        return null;
    }

    /// <summary>Whether Sneak Attack rides on a hit: the attacker has it and hasn't used it this turn, and has advantage, or a target an ally's hit this round, or one that's Marked or Prone.</summary>
    /// <param name="attacker">Who.</param>
    /// <param name="target">At whom.</param>
    /// <param name="advantage">The attack's advantage.</param>
    /// <param name="marked">Whether the target was Marked before the attack used the mark up.</param>
    private bool SneakAttackApplies(Combatant attacker, Combatant target, Advantage advantage, bool marked)
    {
        if (!Has(attacker, AbilityId.SneakAttack) || attacker.SneakTurn == _turnNumber) return false;
        return advantage == Advantage.Advantage || marked || target.Has(ConditionId.Prone) || HitByAlly(attacker, target);
    }

    /// <summary>Whether someone else on the attacker's side has hit the target this round.</summary>
    private bool HitByAlly(Combatant attacker, Combatant target)
    {
        if (!_hitThisRound.TryGetValue(target.Id, out List<string> hitters)) return false;
        foreach (string id in hitters)
            if (id != attacker.Id && Find(id) is Combatant hitter && hitter.Side == attacker.Side) return true;
        return false;
    }

    /// <summary>Notes someone hitting someone else this round, for Sneak Attack.</summary>
    private void NoteHit(Combatant source, Combatant target)
    {
        if (source is null || source == target) return;
        if (!_hitThisRound.TryGetValue(target.Id, out List<string> hitters)) _hitThisRound[target.Id] = hitters = new List<string>();
        if (!hitters.Contains(source.Id)) hitters.Add(source.Id);
    }

    /// <summary>Where a dart goes once its target has fallen: the first foe in slot order the ability can still reach, or null.</summary>
    private Combatant NextAim(Combatant user, AbilityDefinition ability)
    {
        foreach (Combatant each in OnField(user.Side == Side.Party ? Side.Foes : Side.Party))
            if (Standing(each) && CanReach(user, ReachOf(user, ability), each) && !each.Has(ConditionId.Frozen)) return each;
        return null;
    }

    /// <summary>The foe's attack that does most against a target from where it stands, of the ones that reach it; null if none does.</summary>
    internal AttackProfile BestAttack(Combatant user, Combatant target)
    {
        AttackProfile best = null;
        double most = -1.0;
        foreach (AttackProfile attack in user.Attacks)
        {
            if (!CanReach(user, attack.Reach, target)) continue;
            Advantage advantage = AdvantageOf(user, target, attack.Ranged, false, out _);
            double expected = AttackMath.HitChance(attack.Bonus + user.RollBonus, target.ArmourClass, advantage) * attack.AverageDamage;
            if (expected <= most) continue;
            most = expected;
            best = attack;
        }
        return best;
    }

    /// <summary>Whether a target can be hit by an attack from where the user stands: still in the fight, on the field, not Frozen, in reach.</summary>
    private bool Aimable(Combatant user, AttackProfile attack, Combatant target) =>
        target is not null && !target.IsDead && !(target.Side == Side.Foes && Beaten(target)) && !(target.IsDown && _encounter.Nonlethal)
        && SlotOf(target) is not null && !target.Has(ConditionId.Frozen) && CanReach(user, attack.Reach, target);

    /// <summary>The standing foe with the fewest hit points that an attack can reach, for a Multiattack's follow-ups; null if none.</summary>
    private Combatant Weakest(Combatant user, AttackProfile attack)
    {
        Combatant weakest = null;
        foreach (Combatant each in _combatants)
            if (Hostile(each, user) && !each.IsOut && Aimable(user, attack, each) && (weakest is null || each.Hp < weakest.Hp)) weakest = each;
        return weakest;
    }

    /// <summary>The modifiers on a spell attack, for the dice and the log: the casting ability, proficiency, and anything worn.</summary>
    private static List<(string Source, int Amount)> SpellModifiers(Combatant user)
    {
        var modifiers = new List<(string Source, int Amount)>();
        if (user.Casting is Ability casting) modifiers.Add((CombatStats.Abbreviation(casting), user.CastingModifier));
        modifiers.Add(("PROF", LevelTable.Proficiency(user.Level)));
        if (user.SpellFocus != 0) modifiers.Add(("FOCUS", user.SpellFocus));
        return modifiers;
    }

    /// <summary>The d20s for a RollResult: one, or both with advantage or disadvantage.</summary>
    private static int[] Rolled(Advantage advantage, int first, int second) =>
        advantage == Advantage.Normal ? new[] { first } : new[] { first, second };
}
