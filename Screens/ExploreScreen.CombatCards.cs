using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Screens;

/// <summary>
/// What the card says in a fight about whatever the mouse is on: a hotbar button, END TURN, a part
/// of the turn's budget, the Soul-Link, a support, an icon in the frame, someone (and what the picked
/// ability or a click would do to them), or a slot to Shift to.
/// </summary>
/// <remarks>
/// Every number on it says what it is: "72% TO HIT", "4 TO 11 SLASHING", "DC 13 DEX SAVE  ·  58% THEY
/// FAIL", "25 FOCUS  ·  COOLDOWN 1". The odds are the engine's own (Estimate). What an ability can
/// reach is said in rows ("MELEE: THEIR FRONT ROW"), and what it would catch is listed by name.
/// Anything that can't be done says why in ember, and the last line is what a click does, in
/// torchlight.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>Where the card goes this frame: above a piece of the screen, below one, or (both null) by the mouse.</summary>
    private Rectangle? _cardAbove;
    private Rectangle? _cardBelow;

    /// <summary>Fills the card for whatever the mouse is on.</summary>
    private void DescribeHover()
    {
        _card.Clear();
        _cardAbove = null;
        _cardBelow = null;

        if (_hoverSlot >= 0)
        {
            DescribeSlot(_hotbarEntries[_hoverSlot]);
            _cardAbove = CombatHotbar.Slots(_hotbarEntries)[_hoverSlot];
        }
        else if (_hoverEndTurn)
        {
            DescribeEndTurn();
            _cardAbove = CombatHotbar.EndTurn(_hotbarEntries);
        }
        else if (_hoverBudget is BudgetPart part)
        {
            DescribeBudget(part);
            _cardAbove = BudgetBar.Bounds(part);
        }
        else if (_hoverSigil)
        {
            DescribeSigil();
            _cardAbove = SoulLinkSigil.Bounds;
        }
        else if (_hoverSupport >= 0 && _hoverSupport < ActingSupports().Count)
        {
            DescribeSupport(ActingSupports()[_hoverSupport]);
            _cardBelow = _initiative.SupportBanners(_shown.Order.Count)[_hoverSupport];
        }
        else if (_hoverIcon >= 0 && Fighter(_shown.Turn) is Combatant current)
        {
            IReadOnlyList<ConditionId> conditions = _shown.Conditions(current);
            List<AbilityDefinition> passives = Passives(current);
            if (_hoverIcon < conditions.Count) DescribeCondition(current, conditions[_hoverIcon]);
            else
            {
                AbilityDefinition passive = passives[_hoverIcon - conditions.Count];
                _card.Line(passive.Name.ToUpperInvariant(), Palette.Gold);
                _card.Line("ALWAYS ON", Palette.Note);
                _card.Line(passive.Description, Palette.Narration);
            }
            _cardAbove = _unitFrame.Icons(conditions.Count, passives.Count)[_hoverIcon];
        }
        else if (_hoverFighter is Combatant fighter)
        {
            DescribeFighter(fighter);
            DescribePlan(_plan);
            if (_hoverPortrait >= 0) _cardBelow = _initiative.Portrait(_hoverPortrait, _shown.Order.Count, CurrentIndex(_shown.Order));
        }
        else
        {
            DescribePlan(_plan);
        }
    }

    /// <summary>A hotbar button: its name, what it costs and reaches, what it does, its numbers, what's left of it, why it can't be used if it can't, and its key.</summary>
    private void DescribeSlot(HotbarEntry entry)
    {
        if (entry.Ability is not AbilityDefinition ability)
        {
            _card.Line("EMPTY", Palette.Note);
            _card.Line("Nothing in the kit goes here yet.", Palette.Narration);
            return;
        }
        Combatant owner = HotbarOwner;

        _card.Line(ability.Name.ToUpperInvariant(), Palette.Gold);
        _card.Line(Summary(owner, ability), Palette.Note);
        _card.Line(ReachWord(owner, ability), Palette.Note);
        _card.Gap();
        _card.Line(ability.Description, Palette.Narration);

        if (Numbers(owner, ability) is string numbers)
        {
            _card.Gap();
            _card.Line(numbers, Palette.Bone);
        }
        if (Supply(owner, ability) is string supply) _card.Line(supply, Palette.Gold);
        if (!entry.Usable && _combat.Unusable(owner, ability) is string why) _card.Line(why.ToUpperInvariant(), Palette.Ember);

        _card.Gap();
        _card.Line($"KEY {entry.Key}  ·  " + (ability.Shape == TargetShape.Self ? "USES IT AT ONCE" : "THEN CLICK " + TargetWord(ability)), Palette.Torch);
    }

    /// <summary>END TURN: what it does, and who's next.</summary>
    private void DescribeEndTurn()
    {
        _card.Line("END TURN", Palette.Gold);
        _card.Line("Whatever's left of the turn is lost: the reaction comes back at the start of the next one, with 15 Focus.", Palette.Narration);
        IReadOnlyList<Combatant> order = _shown.Order;
        int at = CurrentIndex(order);
        for (int step = 1; at >= 0 && step < order.Count; step++)
        {
            Combatant next = order[(at + step) % order.Count];
            if (_shown.HasDied(next) || _shown.IsDown(next)) continue;
            _card.Line($"NEXT  ·  {next.Name.ToUpperInvariant()}", next.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Bone);
            break;
        }
        _card.Gap();
        _card.Line("SPACE OR END", Palette.Torch);
    }

    /// <summary>A part of the turn's budget: what it's for, and whether it's left.</summary>
    private void DescribeBudget(BudgetPart part)
    {
        TurnBudget budget = (HotbarOwner ?? _combat.Current).Budget;
        (string name, string meaning, bool left) = part switch
        {
            BudgetPart.Action => ("ACTION", "One a turn: your Auto, a skill, Dodge, Help, or something thrown.", budget.Action),
            BudgetPart.Bonus => ("BONUS ACTION", "One a turn, for the quick things: a potion, the elixir, Healing Word, Second Wind, Hex.", budget.BonusAction),
            BudgetPart.Reaction => ("REACTION", "One between your turns: a swing at a foe who Shifts out of your front row while you're in yours.", budget.Reaction),
            _ => ("SHIFT", "Once a turn and free: swap with a friend beside or behind you, or step into an empty slot in your other row. Leaving the front row in reach of a foe's front row lets one swing at you first.", budget.Shift),
        };

        _card.Line(name, Palette.Gold);
        _card.Line(meaning, Palette.Narration);
        _card.Line(left ? "STILL TO USE" : "SPENT", left ? Palette.Bone : Palette.Note);
        if (part == BudgetPart.Shift) _card.Line("CLICK A CHEVRON, OR W A S D", Palette.Torch);
    }

    /// <summary>The Soul-Link: how it fills, how full it is, and that a click strikes.</summary>
    private void DescribeSigil()
    {
        _card.Line("SOUL-LINK", Palette.Gold);
        _card.Line("Hit the same foe as the Soul-Bound between your turns and the bond charges. Full, either of you can spend it on the Soul-Link Strike: both weapons, a sure hit, and radiant light. You both have to be able to reach the foe.", Palette.Narration);
        if (!BondWhole())
        {
            _card.Line("THE BOND IS BROKEN", Palette.Ember);
            return;
        }
        _card.Line($"CHARGE {_shown.Charge} / {SoulLink.Full}", _shown.Charge >= SoulLink.Full ? Palette.Gold : Palette.Bone);
        if (_shown.Charge >= SoulLink.Full) _card.Line("CLICK  ·  SOUL-LINK STRIKE", Palette.Torch);
    }

    /// <summary>A support on the turn order: who, what it does, and when.</summary>
    private void DescribeSupport(SupportView view)
    {
        SupportState state = FindSupport(view.Id);
        _card.Line(view.Name, Palette.Gold);
        _card.Line("SUPPORT  ·  BEHIND THE ROWS, OUT OF REACH", Palette.Note);
        _card.Line(view.Line, Palette.Narration);
        if (state is null) return;

        EncounterSupport support = state.Support;
        string when = state.Off ? "STOOD DOWN"
            : view.Lit ? "ACTS AT THE END OF THIS ROUND"
            : _shown.Round < support.FromRound ? $"FROM THE END OF ROUND {support.FromRound}"
            : "DONE";
        _card.Line(when, view.Lit ? Palette.Gold : Palette.Note);
    }

    /// <summary>A condition on someone: what it's called, how long it lasts, and what it does.</summary>
    private void DescribeCondition(Combatant who, ConditionId condition)
    {
        _card.Line(ConditionInfo.Name(condition).ToUpperInvariant(), CombatStyle.Helps(condition) ? Palette.Gold : Palette.Ember);
        if (Lasts(who, condition) is string lasts) _card.Line(lasts, Palette.Note);
        _card.Line(CombatStyle.Explain(condition), Palette.Narration);
    }

    /// <summary>Someone: their name, what they are, where they stand, their health, armour and Focus (or their death saves), what's on them, their next move if you've seen it, and whether it's their turn.</summary>
    private void DescribeFighter(Combatant fighter)
    {
        _card.Line(fighter.Name.ToUpperInvariant(), fighter.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Heading);
        string where = _shown.SlotsOf(fighter).Count > 0 ? "  ·  " + WhereWord(_shown.SlotsOf(fighter)[0]) : string.Empty;
        _card.Line(WhatTheyAre(fighter) + where, Palette.Note);

        if (_shown.HasDied(fighter)) _card.Line("FALLEN, OUT OF THE FIGHT", Palette.Ember);
        else if (_shown.HasFallenBack(fighter)) _card.Line("FALLEN BACK, OUT OF THE FIGHT", Palette.Note);
        else if (_shown.IsDown(fighter))
        {
            (int successes, int failures) = _shown.DeathSaves(fighter);
            _card.Line(_encounter.Nonlethal || fighter.Side == Side.Foes ? "KNOCKED OUT" : $"DOWNED  ·  DEATH SAVES {successes} MADE, {failures} FAILED", Palette.Ember);
        }
        else _card.Line($"{_shown.Hp(fighter)} / {fighter.MaxHp} HP  ·  AC {fighter.ArmourClass}  ·  {_shown.Focus(fighter)} FOCUS", Palette.Bone);

        var on = new List<string>();
        foreach (ConditionId condition in _shown.Conditions(fighter))
        {
            if (condition is ConditionId.Downed or ConditionId.Stable) continue;
            string lasts = Lasts(fighter, condition);
            on.Add(ConditionInfo.Name(condition).ToUpperInvariant() + (lasts is null ? string.Empty : $" ({lasts})"));
        }
        if (on.Count > 0) _card.Line(string.Join("  ·  ", on), Palette.Bone);

        if (fighter.Side == Side.Foes && _intents.TryGetValue(fighter.Id, out var intent) && Seen(fighter, intent.Ability))
        {
            string at = intent.Target is null ? string.Empty : " AT " + intent.Target.Name.ToUpperInvariant();
            _card.Line($"NEXT  ·  {intent.Ability.Name.ToUpperInvariant()}{at}", CombatStyle.FoeLine);
        }
        if (fighter.Id == _shown.Turn) _card.Line("THEIR TURN", Palette.Gold);
    }

    /// <summary>What a click would do: the Shift and what it risks, or the ability with its odds and who it'd catch; or why it can't be done.</summary>
    private void DescribePlan(TurnPlan plan)
    {
        if (plan is null) return;
        Combatant me = _combat.Current;
        _card.Gap();

        if (plan.Shift is Slot to)
        {
            _card.Line("SHIFT  ·  FREE, ONCE A TURN", Palette.Gold);
            if (plan.Problem is not null)
            {
                _card.Line(plan.Problem.ToUpperInvariant(), Palette.Ember);
                return;
            }
            _card.Line("TO " + WhereWord(to), Palette.Bone);
            if (_shown.At(to) is Combatant swap && swap != me) _card.Line($"SWAPS WITH {swap.Name.ToUpperInvariant()}", Palette.Bone);
            if (_combat.Opportunist(me, to) is Combatant swinging)
                _card.Line($"{swinging.Name.ToUpperInvariant()} SWINGS AT YOU FIRST: LEAVING THE FRONT ROW", Palette.Ember);
            _card.Line("CLICK  ·  SHIFT", Palette.Torch);
            return;
        }

        if (plan.Ability is not AbilityDefinition ability) return;
        _card.Line($"{ability.Name.ToUpperInvariant()}  ·  {CostWord(ability.Cost)}", Palette.Gold);
        if (plan.Problem is not null)
        {
            _card.Line(plan.Problem.ToUpperInvariant(), Palette.Ember);
            return;
        }

        if (plan.Target is Combatant target)
        {
            IReadOnlyList<Combatant> caught = _combat.Caught(me, ability, target);
            if (ability.Kind == AbilityKind.Help && _shown.IsDown(target)) _card.Line("GETS THEM UP ON 1 HP", CombatStyle.Healing);
            else if (caught.Count <= 1) DescribeOdds(_combat.Estimate(me, ability, target), me, ability);
            else DescribeCaught(me, ability, caught);
        }
        _card.Line(ReachWord(me, ability), Palette.Note);
        _card.Line("CLICK  ·  " + ability.Name.ToUpperInvariant(), Palette.Torch);
    }

    /// <summary>The odds of an attack, a save or a heal: the chance, advantage or disadvantage, and the least and most it does.</summary>
    private void DescribeOdds(AttackPreview odds, Combatant me, AbilityDefinition ability)
    {
        string edge = odds.Advantage switch
        {
            Advantage.Advantage => "  ·  ADVANTAGE",
            Advantage.Disadvantage => "  ·  DISADVANTAGE",
            _ => string.Empty,
        };

        if (odds.Save is not null) _card.Line($"{odds.Save}  ·  {Percent(odds.Chance)} THEY FAIL{edge}", Palette.Bone);
        else if (!odds.Heals) _card.Line(odds.Chance >= 1.0 ? "A SURE HIT" : $"{Percent(odds.Chance)} TO HIT{edge}", Palette.Bone);

        if (odds.Max <= 0) return;
        string range = odds.Min == odds.Max ? $"{odds.Max}" : $"{odds.Min} TO {odds.Max}";
        _card.Line(odds.Heals ? $"HEALS {range} HP" : $"{range} {DamageWord(me, ability)}", odds.Heals ? CombatStyle.Healing : Palette.Bone);
    }

    /// <summary>Everyone a shape would catch, with the odds on each foe and a word for each friend.</summary>
    private void DescribeCaught(Combatant me, AbilityDefinition ability, IReadOnlyList<Combatant> caught)
    {
        foreach (Combatant each in caught.Distinct())
        {
            if (each.Side == me.Side)
            {
                _card.Line($"{each.Name.ToUpperInvariant()}", ability.Kind is AbilityKind.Heal ? CombatStyle.Healing : Palette.Bone);
                continue;
            }
            AttackPreview odds = _combat.Estimate(me, ability, each);
            string chance = odds.Save is not null ? $"{odds.Save}, {Percent(odds.Chance)} THEY FAIL" : odds.Chance >= 1.0 ? "A SURE HIT" : $"{Percent(odds.Chance)} TO HIT";
            _card.Line($"{each.Name.ToUpperInvariant()}: {chance}", Palette.Bone);
        }
    }

    /// <summary>What an ability costs: the part of the turn, its Focus, its cooldown, once a fight, and concentration, for its button's card.</summary>
    private static string Summary(Combatant owner, AbilityDefinition ability)
    {
        var parts = new List<string> { CostWord(ability.Cost) };
        if (ability.Focus > 0) parts.Add($"{ability.Focus} FOCUS");
        if (ability.Cooldown > 0) parts.Add($"COOLDOWN {ability.Cooldown}");
        if (ability.OncePerFight) parts.Add("ONCE A FIGHT");
        if (ability.FocusGain > 0) parts.Add($"+{ability.FocusGain} FOCUS");
        if (ability.Concentration) parts.Add("CONCENTRATION");
        if (owner?.Member is null && ability.Spell) parts.Add("SPELL");
        return string.Join("  ·  ", parts);
    }

    /// <summary>Who an ability reaches, in rows, and the shape it hits: "MELEE: THEIR FRONT ROW  ·  ONE FOE".</summary>
    private static string ReachWord(Combatant who, AbilityDefinition ability)
    {
        string shape = ShapeWord(ability);
        if (ability.Shape is TargetShape.Self or TargetShape.Ally or TargetShape.AllyNextTo or TargetShape.AllAllies or TargetShape.YourRow or TargetShape.YourBackRow)
            return shape;

        Reach reach = ability.Reach == Reach.Weapon ? who?.MainHand.Reach ?? Reach.Melee : ability.Reach;
        string rows = reach switch
        {
            Reach.Melee => "MELEE: THEIR FRONT ROW, FROM YOURS",
            Reach.Reach => "REACH: EITHER ROW FROM YOUR FRONT ROW, THEIR FRONT FROM YOUR BACK",
            Reach.Ranged => "RANGED: ANYONE",
            _ => "ANY ROW",
        };
        return rows + "  ·  " + shape;
    }

    /// <summary>The shape an ability hits, in capitals.</summary>
    private static string ShapeWord(AbilityDefinition ability) => ability.Shape switch
    {
        TargetShape.Self => "YOURSELF",
        TargetShape.Ally => ability.NotSelf ? "AN ALLY" : "YOU OR AN ALLY",
        TargetShape.AllyNextTo => "YOU OR AN ALLY NEXT TO YOU",
        TargetShape.AllAllies => "ALL YOUR SIDE",
        TargetShape.YourRow => "YOUR ROW",
        TargetShape.YourBackRow => "YOUR BACK ROW",
        TargetShape.Foe => "ONE FOE",
        TargetShape.FoeAndNeighbours => "A FOE AND THOSE BESIDE IT",
        TargetShape.FoeRow => "A WHOLE ROW OF FOES",
        TargetShape.FoeColumn => "BOTH SLOTS OF A LANE",
        TargetShape.AllFoes => "EVERY FOE",
        _ => ability.Count > 1 ? $"{ability.Count} FOES AT RANDOM" : "A FOE AT RANDOM",
    };

    /// <summary>Where a slot is, in words: "FRONT ROW, SLOT 2".</summary>
    private static string WhereWord(Slot slot) => $"{(slot.Row == Row.Front ? "FRONT" : "BACK")} ROW, SLOT {slot.Number}";

    /// <summary>An ability's numbers for its button's card: its dice and its bonus to hit, its save, its healing; or null for one without any.</summary>
    private string Numbers(Combatant owner, AbilityDefinition ability)
    {
        string kind = DamageWord(owner, ability);
        switch (ability.Kind)
        {
            case AbilityKind.Weapon:
                return Weapon(owner.MainHand);
            case AbilityKind.OffHand:
                return owner.OffHand is null ? null : Weapon(owner.OffHand);
            case AbilityKind.Thrown:
                ItemDefinition item = ItemCatalog.Get(ability.Item);
                return $"{item.ThrownDamage} {kind}  ·  {Signed(owner.Scores.Modifier(Ability.Dexterity))} TO HIT";
            case AbilityKind.SpellAttack:
                return $"{ability.Damage} {kind}  ·  {Signed(owner.SpellAttack)} TO HIT";
            case AbilityKind.Save:
                int dc = ability.Dc > 0 ? ability.Dc : owner.SpellDc;
                string save = $"DC {dc} {CombatStats.Abbreviation(ability.Save ?? Ability.Dexterity)} SAVE";
                string damage = ability.Damage.IsNone ? string.Empty : $"  ·  {ability.Damage} {kind}{(ability.SaveHalves ? ", HALF ON A SAVE" : string.Empty)}";
                return save + damage;
            case AbilityKind.AutoHit:
                return $"{ability.Count} DARTS OF {ability.Damage} {kind}, NEVER MISSING";
            case AbilityKind.Heal when ability.Heals.IsNone:
                return ability.PoolPerLevel > 0 && owner.Member is not null ? $"HEALS FROM A POOL OF {ability.PoolPerLevel * owner.Member.Level} HP A FIGHT" : null;
            case AbilityKind.Heal:
                int flat = (ability.HealsAddCasting ? owner.CastingModifier : 0) + (ability.HealsAddLevel ? owner.Level : 0);
                DiceExpression heals = ability.Heals with { Bonus = ability.Heals.Bonus + flat };
                return $"HEALS {heals}";
            case AbilityKind.SoulLink:
                return $"BOTH WEAPONS + {ability.Damage} RADIANT, A SURE HIT";
            case AbilityKind.Condition when !ability.Damage.IsNone:
                return ability.Id == AbilityId.Hex ? $"+{ability.Damage} {kind} ON YOUR EVERY HIT" : $"A {ability.Damage} TO ADD";
            default:
                return null;
        }
    }

    /// <summary>What's left in the bag of what an ability uses up, for its button's card.</summary>
    private string Supply(Combatant owner, AbilityDefinition ability)
    {
        if (owner.Member is null) return null;
        if (ability.Item is not null) return $"IN THE BAG  ·  {_combat.Bag?.Count(ability.Item) ?? 0}";
        if (ability.Kind == AbilityKind.Potion) return $"HEALING POTIONS IN THE BAG  ·  {HealingPotions()}";
        return null;
    }

    /// <summary>A weapon's dice and bonus to hit: "1d8+3 SLASHING  ·  +5 TO HIT", with any extra a magic one does.</summary>
    private static string Weapon(AttackProfile weapon)
    {
        string extra = weapon.Extra.IsNone ? string.Empty : $" + {weapon.Extra} {weapon.ExtraType.ToString().ToUpperInvariant()}";
        return $"{weapon.Damage} {weapon.DamageType.ToString().ToUpperInvariant()}{extra}  ·  {Signed(weapon.Bonus)} TO HIT";
    }

    /// <summary>The kind of damage an ability does, in capitals: the weapon's for an attack, the flask's for something thrown.</summary>
    private static string DamageWord(Combatant who, AbilityDefinition ability)
    {
        DamageType kind = ability.Kind switch
        {
            AbilityKind.Weapon => who.MainHand.DamageType,
            AbilityKind.OffHand when who.OffHand is not null => who.OffHand.DamageType,
            AbilityKind.Thrown => ItemCatalog.Get(ability.Item).ThrownDamageType,
            _ => ability.DamageType,
        };
        return kind.ToString().ToUpperInvariant();
    }

    /// <summary>What part of the turn something costs, in capitals.</summary>
    private static string CostWord(ActionCost cost) => cost switch
    {
        ActionCost.Action => "ACTION",
        ActionCost.BonusAction => "BONUS ACTION",
        ActionCost.Reaction => "REACTION",
        _ => "FREE",
    };

    /// <summary>Who an ability's aimed at, in capitals, for "THEN CLICK ...".</summary>
    private static string TargetWord(AbilityDefinition ability) => IsForFriends(ability) ? (ability.NotSelf ? "AN ALLY" : "YOU OR AN ALLY") : "A FOE";

    /// <summary>How long a condition on someone lasts, in capitals, or null for until something ends it.</summary>
    private static string Lasts(Combatant who, ConditionId condition)
    {
        foreach (ActiveCondition on in who.Conditions)
            if (on.Id == condition && on.Turns > 0) return on.Turns == 1 ? "1 TURN" : $"{on.Turns} TURNS";
        return null;
    }

    /// <summary>A chance as a whole percentage: "72%".</summary>
    private static string Percent(double chance) => ((int)Math.Round(chance * 100.0)).ToString(CultureInfo.InvariantCulture) + "%";

    /// <summary>A bonus with its sign: "+5", "-1".</summary>
    private static string Signed(int value) => value >= 0 ? "+" + value : value.ToString(CultureInfo.InvariantCulture);
}
