using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Hit points and conditions: damage and what it does to the Downed, dying, healing and getting back
/// up, the abilities that heal or put conditions on people, concentration, and the Soul-Link's
/// charge and barks.
/// </summary>
/// <remarks>
/// A foe that's beaten (dead, given up, or knocked out in a fight to yield) leaves the field,
/// and so does anyone who dies. A Downed party member keeps their slot. A guest at 0 hit points falls
/// back instead of going down: out of the fight, with no death saves.
/// </remarks>
public sealed partial class CombatEngine
{
    /// <summary>The lowest a concentration save's DC goes: 5e's 10, or half the damage if that's more.</summary>
    private const int ConcentrationFloor = 10;

    /// <summary>Takes hit points off someone, and deals with what follows: Downed or dead at 0, a failed death save for a hit on someone already Downed, concentration, giving up, the Soul-Link, and the fight's end.</summary>
    /// <param name="target">Who's hurt.</param>
    /// <param name="amount">How much.</param>
    /// <param name="kind">The kind of damage.</param>
    /// <param name="source">Who did it, or null.</param>
    /// <param name="critical">Whether it was a critical, which costs someone Downed two death saves.</param>
    /// <param name="ability">What did it.</param>
    /// <param name="roll">The damage dice, for the screen.</param>
    private void Damage(Combatant target, int amount, DamageType kind, Combatant source, bool critical, AbilityId? ability, RollResult roll)
    {
        if (target.IsDead || target.FellBack || amount <= 0) return;
        NoteHit(source, target);

        if (target.IsDown)
        {
            if (target.Side == Side.Foes || _encounter.Nonlethal) return;
            RemoveCondition(target, ConditionId.Stable);
            target.DeathFailures += critical ? 2 : 1;
            string failed = critical ? "two failed death saves" : "a failed death save";
            Emit(CombatEventKind.Damaged, source, target, amount, $"{target.Label} is hit while down: {failed}.", roll, ability, kind);
            if (target.DeathFailures >= DeathSavesNeeded) Die(target);
            CheckOutcome();
            return;
        }

        target.SetHp(target.Hp - amount);
        Emit(CombatEventKind.Damaged, source, target, amount, null, roll, ability, kind);
        if (ability != AbilityId.SoulLinkStrike) ChargeSoulLink(source, target);

        if (target.Hp == 0) Drop(target);
        else
        {
            KeepConcentration(target, amount);
            if (target.Side == Side.Foes && Beaten(target) && _yielded.Add(target.Id))
                Emit(CombatEventKind.Log, target, null, 0, $"{target.Label} has had enough, and gives up the fight.");
        }
        if (target.Side == Side.Foes && Beaten(target)) Formation.Remove(target);

        BarkIfHurt(target);
        CheckOutcome();
    }

    /// <summary>Someone at 0 hit points: a foe dies, a party member goes Downed, and in a fight to yield anyone's knocked out, stable.</summary>
    private void Drop(Combatant who)
    {
        if (who.Guest && who.Side == Side.Party)
        {
            FallBack(who);
            return;
        }
        if (who.Side == Side.Foes && !_encounter.Nonlethal)
        {
            Die(who);
            return;
        }

        who.Downs++;
        who.DeathSuccesses = who.DeathFailures = 0;
        EndConcentration(who, false);
        AddCondition(who, ConditionId.Downed, 0, null, null);
        Emit(CombatEventKind.Downed, who, who, 0, _encounter.Nonlethal ? $"{who.Label} is knocked out." : $"{who.Label} is down!");
        if (_encounter.Nonlethal) AddCondition(who, ConditionId.Stable, 0, null, null);
    }

    /// <summary>Someone dies: off the initiative bar, every condition gone, and whatever they were concentrating on with them.</summary>
    private void Die(Combatant who)
    {
        who.SetHp(0);
        who.IsDead = true;
        EndConcentration(who, false);
        who.ClearConditions();
        Emit(CombatEventKind.Died, who, who, 0, who.Side == Side.Party ? $"{who.Label} has fallen, out of the fight." : $"{who.Label} falls.");
        Formation.Remove(who);
        RefreshOrder();
    }

    /// <summary>A guest at 0 hit points falls back: out of the fight and off the field, with no death saves, and none the worse for the story.</summary>
    private void FallBack(Combatant who)
    {
        who.FellBack = true;
        EndConcentration(who, false);
        Emit(CombatEventKind.FellBack, who, who, 0, $"{who.Label} falls back, out of the fight.");
        Formation.Remove(who);
        RefreshOrder();
    }

    /// <summary>Gives someone hit points back, which gets them up if they were Downed.</summary>
    private void Heal(Combatant target, int amount, Combatant healer)
    {
        if (target.IsDead || amount <= 0) return;

        int before = target.Hp;
        target.SetHp(before + amount);
        if (before == 0) GetUp(target, healer);
        Emit(CombatEventKind.Healed, healer, target, target.Hp - before);
    }

    /// <summary>Puts someone Downed back on a few hit points: a 20 on a death save, Help, or the fight being won.</summary>
    private void Revive(Combatant who, int hp, Combatant helper)
    {
        who.SetHp(hp);
        GetUp(who, helper);
        Emit(CombatEventKind.Healed, helper, who, who.Hp);
    }

    /// <summary>Takes Downed and Stable off someone who's got hit points again, and wipes their death saves.</summary>
    private void GetUp(Combatant who, Combatant helper)
    {
        RemoveCondition(who, ConditionId.Downed);
        RemoveCondition(who, ConditionId.Stable);
        who.DeathSuccesses = who.DeathFailures = 0;
        Emit(CombatEventKind.Revived, helper ?? who, who, who.Hp, $"{who.Label} is back on their feet.");
    }

    /// <summary>A healing ability: its dice and what's added to them, or a pool's worth (Lay on Hands, as much as is needed and the pool has).</summary>
    private void Mend(Combatant user, AbilityDefinition ability, Combatant target)
    {
        string name = ability.Name.ToUpperInvariant();
        string on = target == user ? string.Empty : $" on {target.Label}";

        if (ability.Heals.IsNone)
        {
            int need = target.MaxHp - target.Hp;
            int pool = user.Pool.GetValueOrDefault(ability.Id);
            int amount = Math.Min(pool, need);
            user.Pool[ability.Id] = pool - amount;
            Emit(CombatEventKind.AbilityUsed, user, target, amount, $"{user.Label} uses {name}{on}: {amount} HP.", ability: ability.Id);
            Heal(target, amount, user);
            return;
        }

        int flat = (ability.HealsAddCasting ? user.CastingModifier : 0) + (ability.HealsAddLevel ? user.Level : 0);
        DiceExpression heals = ability.Heals with { Bonus = ability.Heals.Bonus + flat };
        RollResult roll = heals.Roll(_random, name);
        int healed = Math.Max(0, roll.Total);
        Emit(CombatEventKind.AbilityUsed, user, target, healed, $"{user.Label} {(ability.Spell ? "casts" : "uses")} {name}{on}: {heals} = {healed} HP.", roll, ability.Id);
        Heal(target, healed, user);
    }

    /// <summary>Drinks something from the bag, or hands it to an ally next to them: its healing, and the Focus it gives back.</summary>
    /// <param name="user">Who's drinking it, or handing it over.</param>
    /// <param name="ability">What.</param>
    /// <param name="drinker">Who drinks it: the user, or an ally next to them.</param>
    private void Drink(Combatant user, AbilityDefinition ability, Combatant drinker)
    {
        drinker ??= user;
        ItemDefinition item = ability.Item is not null ? ItemCatalog.Get(ability.Item) : PickPotion(drinker);
        if (ability.Item is null) Bag.Remove(item.Id);

        string text = drinker == user ? $"{user.Label} drinks {item.Name.ToUpperInvariant()}" : $"{user.Label} hands {drinker.Label} {item.Name.ToUpperInvariant()}";
        RollResult roll = null;
        int healed = 0;
        if (!item.Heals.IsNone)
        {
            roll = item.Heals.Roll(_random, item.Name);
            healed = Math.Max(0, roll.Total);
            text += $": {item.Heals} = {healed} HP";
        }

        if (item.FocusRestored > 0) text += $"{(roll is null ? ":" : ",")} {item.FocusRestored} Focus";
        Emit(CombatEventKind.AbilityUsed, user, drinker, healed, text + ".", roll, ability.Id);
        Heal(drinker, healed, user);
        if (item.FocusRestored > 0) GainFocus(drinker, item.FocusRestored);
    }

    /// <summary>An ability that puts its conditions on someone without a roll: Dodge, Disengage, Hex, Bardic Inspiration, a smoke bomb.</summary>
    private void Bestow(Combatant user, AbilityDefinition ability, Combatant target)
    {
        Combatant bearer = target;
        string name = ability.Name.ToUpperInvariant();
        string text = bearer == user ? $"{user.Label} uses {name}." : $"{user.Label} {(ability.Spell ? "casts" : "uses")} {name} on {bearer.Label}.";
        Emit(CombatEventKind.AbilityUsed, user, bearer, 0, text, ability: ability.Id);

        foreach (ConditionId condition in ability.Conditions)
            AddCondition(bearer, condition, condition == ConditionId.Dodging ? 0 : ability.Turns, user, ability.Id, 0, ability.Damage);
    }

    /// <summary>Help: a Downed ally back up on 1 HP, or advantage on a standing one's next attack.</summary>
    private void Help(Combatant user, AbilityDefinition ability, Combatant target)
    {
        if (target.IsDown)
        {
            Emit(CombatEventKind.AbilityUsed, user, target, 0, $"{user.Label} hauls {target.Label} back up.", ability: ability.Id);
            Revive(target, 1, user);
            return;
        }

        Emit(CombatEventKind.AbilityUsed, user, target, 0, $"{user.Label} helps {target.Label}.", ability: ability.Id);
        foreach (ConditionId condition in ability.Conditions) AddCondition(target, condition, ability.Turns, user, ability.Id);
    }

    /// <summary>The Soul-Link Strike: the meter spent, both of the duo's weapons and the bond's radiant light on a sure hit, and the Soul-Bound's shout the first time.</summary>
    private void SoulLinkStrike(Combatant user, AbilityDefinition ability, Combatant target)
    {
        Combatant partner = Partner(user);
        SoulLink.Spend();

        if (!SoulLink.Struck && Find(Origins.SoulBound) is { IsDuo: true, IsOut: false } soulBound)
        {
            SoulLink.Struck = true;
            Emit(CombatEventKind.Bark, soulBound, target, 0, SoulLink.StrikeBark);
        }

        var parts = new[]
        {
            new DamagePart(user.MainHand.Damage, user.MainHand.DamageType, user.MainHand.Name.ToLowerInvariant()),
            new DamagePart(partner.MainHand.Damage, partner.MainHand.DamageType, partner.MainHand.Name.ToLowerInvariant()),
            new DamagePart(ability.Damage, ability.DamageType, "the bond"),
        };
        int amount = RollDamage(parts, false, out RollResult rolled, out string written);
        Emit(CombatEventKind.SoulLinkStrike, user, target, amount, $"{user.Label} and {partner.Label} strike {target.Label} as one: {written}.", rolled, ability.Id, ability.DamageType);
        Emit(CombatEventKind.Hit, user, target, amount, null, null, ability.Id, ability.DamageType);
        Damage(target, amount, user.MainHand.DamageType, user, false, ability.Id, rolled);
    }

    /// <summary>Puts a condition on someone and says so. The event's amount is the condition, as a number.</summary>
    private void AddCondition(Combatant target, ConditionId id, int turns, Combatant source, AbilityId? ability, int dc = 0, DiceExpression die = default)
    {
        target.Add(new ActiveCondition(id, turns, source?.Id, ability, dc, die));
        string text = id is ConditionId.Downed or ConditionId.Stable ? null : $"{target.Label} is {ConditionInfo.Word(id)}.";
        Emit(CombatEventKind.ConditionAdded, source ?? target, target, (int)id, text, ability: ability);
    }

    /// <summary>Takes a condition off someone, if it's on, and says so. The event's amount is the condition, as a number.</summary>
    private void RemoveCondition(Combatant who, ConditionId id)
    {
        if (!who.Remove(id)) return;
        string text = id is ConditionId.Downed or ConditionId.Stable ? null : $"{who.Label} is no longer {ConditionInfo.Word(id)}.";
        Emit(CombatEventKind.ConditionRemoved, who, who, (int)id, text);
    }

    /// <summary>Starts concentrating on a spell, which ends whatever they were concentrating on before.</summary>
    private void Concentrate(Combatant who, AbilityId spell)
    {
        EndConcentration(who, false);
        who.Concentrating = spell;
    }

    /// <summary>Ends someone's concentration, and takes off everything their spell put on anyone.</summary>
    /// <param name="who">Who.</param>
    /// <param name="broken">Whether damage broke it, which the log says.</param>
    private void EndConcentration(Combatant who, bool broken)
    {
        if (who.Concentrating is not AbilityId spell) return;
        who.Concentrating = null;
        if (broken) Emit(CombatEventKind.Log, who, null, 0, $"{who.Label} loses concentration on {AbilityName(spell)}.", ability: spell);

        foreach (Combatant each in _combatants)
            foreach (ActiveCondition condition in each.Conditions.ToArray())
                if (condition.Source == who.Id && condition.From == spell) RemoveCondition(each, condition.Id);
    }

    /// <summary>A Constitution save to keep concentrating after taking damage: DC 10, or half the damage if that's more.</summary>
    private void KeepConcentration(Combatant who, int damage)
    {
        if (who.Concentrating is not AbilityId spell) return;
        int dc = Math.Max(ConcentrationFloor, damage / 2);
        bool kept = RollSave(who, Ability.Constitution, dc, "CONCENTRATION", out RollResult roll, out string line);
        Emit(CombatEventKind.SaveRolled, who, who, roll.Total, line, roll, spell);
        if (!kept) EndConcentration(who, true);
    }

    /// <summary>Notes one of the duo damaging a foe for the Soul-Link, and says so if it charged.</summary>
    private void ChargeSoulLink(Combatant source, Combatant target)
    {
        if (source is not { IsDuo: true } || target.Side != Side.Foes) return;
        if (!SoulLink.NoteHit(source.Id, target.Id, ++_clock, _turnNumber)) return;

        string text = SoulLink.IsFull ? "The Soul-Link is full: SOUL-LINK STRIKE is ready!" : "The Soul-Link stirs.";
        Emit(CombatEventKind.SoulLinkCharged, source, target, SoulLink.Charge, text);
    }

    /// <summary>The Soul-Bound's line the first time you're badly hurt in a fight, if they're up to say it.</summary>
    private void BarkIfHurt(Combatant hurt)
    {
        if (hurt.Id != Origins.You || !hurt.IsDuo || SoulLink.Hurt || hurt.Hp > hurt.MaxHp * SoulLink.LowHealth) return;
        if (Find(Origins.SoulBound) is not { IsDuo: true, IsOut: false } soulBound) return;

        SoulLink.Hurt = true;
        Emit(CombatEventKind.Bark, soulBound, hurt, 0, SoulLink.LowHealthBark);
    }
}
