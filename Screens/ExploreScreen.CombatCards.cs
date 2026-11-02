using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Screens;

/// <summary>
/// What the card says in a fight about whatever the mouse is on: a hotbar slot, END TURN, a part of
/// the turn's budget, the Soul-Link, an icon in the frame, someone (and what the picked ability or a
/// click would do to them), or a walk.
/// </summary>
/// <remarks>
/// Every number on it says what it is: "72% TO HIT", "4 TO 11 SLASHING", "DC 13 DEX SAVE  ·  58%
/// THEY FAIL", "RANGE 18.0 M  ·  7.5 M AWAY". The odds are the engine's own (Estimate), from the tile
/// the ability would be used from, so a walk into range first is counted. Anything that can't be
/// done says why in ember, and the last line is what a click does, in torchlight.
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
            if (_hoverPortrait >= 0) _cardBelow = InitiativeBar.Portrait(_hoverPortrait, _shown.Order.Count, CurrentIndex(_shown.Order));
        }
        else
        {
            DescribePlan(_plan);
        }
    }

    /// <summary>A hotbar slot: its name, what it costs and reaches, what it does, its numbers, what's left of it, why it can't be used if it can't, and its key.</summary>
    private void DescribeSlot(HotbarEntry entry)
    {
        AbilityDefinition ability = entry.Ability;
        Combatant owner = HotbarOwner;

        _card.Line(ability.Name.ToUpperInvariant(), Palette.Gold);
        _card.Line(Summary(owner, ability), Palette.Note);
        _card.Gap();
        _card.Line(ability.Description, Palette.Narration);

        if (Numbers(owner, ability) is string numbers)
        {
            _card.Gap();
            _card.Line(numbers, Palette.Bone);
        }
        if (Supply(owner, ability) is string supply) _card.Line(supply, Palette.Gold);
        if (!entry.Usable && _combat.Unusable(owner, ability) is string why) _card.Line(why.ToUpperInvariant(), Palette.Ember);

        int index = _hotbarEntries.IndexOf(entry);
        string key = index < CombatHotbar.KeyNames.Count ? $"KEY {CombatHotbar.KeyNames[index]}  ·  " : string.Empty;
        _card.Gap();
        _card.Line(key + (ability.Target == TargetKind.Self ? "USES IT AT ONCE" : "THEN CLICK " + TargetWord(ability)), Palette.Torch);
    }

    /// <summary>END TURN: what it does, and who's next.</summary>
    private void DescribeEndTurn()
    {
        _card.Line("END TURN", Palette.Gold);
        _card.Line("Whatever's left of the turn is lost: the reaction comes back at the start of the next one.", Palette.Narration);
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

    /// <summary>A part of the turn's budget: what it's for, and what's left of it.</summary>
    private void DescribeBudget(BudgetPart part)
    {
        TurnBudget budget = (HotbarOwner ?? _combat.Current).Budget;
        (string name, string meaning, bool left) = part switch
        {
            BudgetPart.Action => ("ACTION", "One a turn: an attack, a spell, Dash, Disengage, Dodge, Help, or something thrown.", budget.Action),
            BudgetPart.Bonus => ("BONUS ACTION", "One a turn, for the quick things: a potion, Healing Word, Cunning Action, Hex, an off-hand attack.", budget.BonusAction),
            BudgetPart.Reaction => ("REACTION", "One between your turns: a swing at a foe who walks out of your reach without disengaging.", budget.Reaction),
            _ => ("MOVEMENT", "A tile is 1.5 m, and a diagonal step costs the same. Dash adds as much again.", budget.MovementLeft > 0f),
        };

        _card.Line(name, Palette.Gold);
        _card.Line(meaning, Palette.Narration);
        string state = part == BudgetPart.Movement
            ? $"{BudgetBar.Metres(budget.MovementLeft)} OF {BudgetBar.Metres(budget.Movement)} M LEFT"
            : left ? "STILL TO USE" : "SPENT";
        _card.Line(state, left ? Palette.Bone : Palette.Note);
    }

    /// <summary>The Soul-Link: how it fills, and how full it is.</summary>
    private void DescribeSigil()
    {
        _card.Line("SOUL-LINK", Palette.Gold);
        _card.Line("Hit the same foe as the Soul-Bound between your turns and the bond charges. Full, either of you can spend it on the Soul-Link Strike: both weapons, a sure hit, and 1d8 radiant.", Palette.Narration);
        _card.Line($"CHARGE {_shown.Charge} / {SoulLink.Full}", _shown.Charge >= SoulLink.Full ? Palette.Gold : Palette.Bone);
    }

    /// <summary>A condition on someone: what it's called, how long it lasts, and what it does.</summary>
    private void DescribeCondition(Combatant who, ConditionId condition)
    {
        _card.Line(ConditionInfo.Name(condition).ToUpperInvariant(), CombatStyle.Helps(condition) ? Palette.Gold : Palette.Ember);
        if (Lasts(who, condition) is string lasts) _card.Line(lasts, Palette.Note);
        _card.Line(CombatStyle.Explain(condition), Palette.Narration);
    }

    /// <summary>Someone: their name, what they are, their health and armour (or their death saves), what's on them, and whether it's their turn.</summary>
    private void DescribeFighter(Combatant fighter)
    {
        _card.Line(fighter.Name.ToUpperInvariant(), fighter.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Heading);
        _card.Line(WhatTheyAre(fighter), Palette.Note);

        if (_shown.HasDied(fighter)) _card.Line("FALLEN, OUT OF THE FIGHT", Palette.Ember);
        else if (_shown.IsDown(fighter))
        {
            (int successes, int failures) = _shown.DeathSaves(fighter);
            _card.Line(_encounter.Nonlethal || fighter.Side == Side.Foes ? "KNOCKED OUT" : $"DOWNED  ·  DEATH SAVES {successes} MADE, {failures} FAILED", Palette.Ember);
        }
        else _card.Line($"{_shown.Hp(fighter)} / {fighter.MaxHp} HP  ·  AC {fighter.ArmourClass}", Palette.Bone);

        var on = new List<string>();
        foreach (ConditionId condition in _shown.Conditions(fighter))
        {
            if (condition is ConditionId.Downed or ConditionId.Stable) continue;
            string lasts = Lasts(fighter, condition);
            on.Add(ConditionInfo.Name(condition).ToUpperInvariant() + (lasts is null ? string.Empty : $" ({lasts})"));
        }
        if (on.Count > 0) _card.Line(string.Join("  ·  ", on), Palette.Bone);
        if (fighter.Id == _shown.Turn) _card.Line("THEIR TURN", Palette.Gold);
    }

    /// <summary>What a click would do: the walk and what it costs, or the ability with its odds, its damage, its reach and anything it provokes; or why it can't be done.</summary>
    private void DescribePlan(TurnPlan plan)
    {
        if (plan is null) return;
        Combatant me = _combat.Current;
        _card.Gap();

        if (plan.Ability is not AbilityDefinition ability)
        {
            float metres = plan.Path.Count * Measure.TileMetres;
            if (plan.Problem is not null)
            {
                _card.Line(plan.Problem.ToUpperInvariant(), Palette.Ember);
                return;
            }
            _card.Line($"WALK  ·  {BudgetBar.Metres(metres)} M", Palette.Gold);
            if (plan.Reachable < plan.Path.Count)
                _card.Line($"ONLY {BudgetBar.Metres(plan.Reachable * Measure.TileMetres)} M OF IT THIS TURN; THE REST IS TOO FAR", Palette.Ember);
            DescribeThreats(me, plan);
            _card.Line("CLICK  ·  WALK", Palette.Torch);
            return;
        }

        _card.Line($"{ability.Name.ToUpperInvariant()}  ·  {CostWord(ability.Cost)}", Palette.Gold);
        if (plan.Problem is not null)
        {
            _card.Line(plan.Problem.ToUpperInvariant(), Palette.Ember);
            return;
        }

        Point from = plan.Path.Count > 0 ? plan.Path[^1] : me.Cell;
        if (plan.Target is Combatant target && ability.Target is TargetKind.Enemy or TargetKind.Ally)
        {
            if (ability.Kind == AbilityKind.Help && _shown.IsDown(target)) _card.Line("GETS THEM UP ON 1 HP", CombatStyle.Healing);
            else DescribeOdds(_combat.Estimate(me, ability, target, from), me, ability);
        }
        else if (ability.Target is TargetKind.Area or TargetKind.Point)
        {
            DescribeCaught(me, ability, plan.Cell, from);
        }

        Point aim = plan.Target?.Cell ?? plan.Cell;
        float range = _combat.RangeOf(me, ability);
        if (range > 0f) _card.Line($"RANGE {BudgetBar.Metres(range)} M  ·  {BudgetBar.Metres(Measure.Steps(from, aim) * Measure.TileMetres)} M AWAY", Palette.Note);
        if (plan.Path.Count > 0) _card.Line($"WALKS {BudgetBar.Metres(plan.Path.Count * Measure.TileMetres)} M INTO REACH FIRST", Palette.Note);
        DescribeThreats(me, plan);
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

    /// <summary>Who an area would catch and the odds on each foe, with a warning for every friend caught in it.</summary>
    private void DescribeCaught(Combatant me, AbilityDefinition ability, Point centre, Point from)
    {
        int caught = 0;
        foreach (Combatant each in _combat.Combatants)
        {
            if (_shown.HasDied(each) || each.IsOut || !Measure.Within(each.Cell, centre, ability.Radius)) continue;
            caught++;
            if (each.Side == me.Side)
            {
                _card.Line($"{each.Name.ToUpperInvariant()} IS IN IT TOO", Palette.Ember);
                continue;
            }
            AttackPreview odds = _combat.Estimate(me, ability, each, from);
            _card.Line($"{each.Name.ToUpperInvariant()}: {odds.Save}, {Percent(odds.Chance)} THEY FAIL", Palette.Bone);
        }
        if (caught == 0) _card.Line("NOBODY THERE", Palette.Note);
    }

    /// <summary>A warning for every foe the walk would let swing at you.</summary>
    private void DescribeThreats(Combatant me, TurnPlan plan)
    {
        int steps = Math.Min(plan.Path.Count, plan.Ability is null ? plan.Reachable : plan.Path.Count);
        if (steps == 0) return;

        var walked = new List<Point>();
        for (int i = 0; i < steps; i++) walked.Add(plan.Path[i]);
        foreach (Combatant threat in _combat.ThreatsAlong(me, me.Cell, walked))
            _card.Line($"OPPORTUNITY ATTACK FROM {threat.Name.ToUpperInvariant()}", Palette.Ember);
    }

    /// <summary>What an ability costs, who it's aimed at, how far it reaches, and what kind of spell it is, for its slot's card.</summary>
    private string Summary(Combatant owner, AbilityDefinition ability)
    {
        var parts = new List<string> { CostWord(ability.Cost), TargetWord(ability) };
        float range = _combat.RangeOf(owner, ability);
        if (range > 0f && ability.Target != TargetKind.Self) parts.Add($"{BudgetBar.Metres(range)} M");
        if (ability.Radius > 0f) parts.Add($"{BudgetBar.Metres(ability.Radius)} M ROUND");
        if (ability.Cantrip) parts.Add("CANTRIP");
        else if (ability.Spell) parts.Add($"LEVEL {ability.SpellSlotLevel} SPELL");
        if (ability.Concentration) parts.Add("CONCENTRATION");
        return string.Join("  ·  ", parts);
    }

    /// <summary>An ability's numbers for its slot's card: its dice and its bonus to hit, its save, its healing; or null for one without any.</summary>
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
                return $"{ability.Darts} DARTS OF {ability.Damage} {kind}, NEVER MISSING";
            case AbilityKind.Heal when ability.Heals.IsNone:
                return owner.Member is null ? null : $"HEALS UP TO {ResourceCatalog.Left(owner.Member, ability.Resource)} HP FROM THE POOL";
            case AbilityKind.Heal:
                int flat = (ability.HealsAddCasting ? owner.CastingModifier : 0) + (ability.HealsAddLevel ? owner.Level : 0);
                DiceExpression heals = ability.Heals with { Bonus = ability.Heals.Bonus + flat };
                return $"HEALS {heals}";
            case AbilityKind.SoulLink:
                return $"BOTH WEAPONS + {ability.Damage} RADIANT, A SURE HIT";
            case AbilityKind.Dash:
                return $"+{BudgetBar.Metres(owner.Budget.Speed)} M OF MOVEMENT";
            case AbilityKind.Condition when !ability.Damage.IsNone:
                return ability.Id == AbilityId.Hex ? $"+{ability.Damage} {kind} ON YOUR EVERY HIT" : $"A {ability.Damage} TO ADD";
            default:
                return null;
        }
    }

    /// <summary>What's left of what an ability spends, for its slot's card: slots or uses and when they come back, or how many are in the bag.</summary>
    private string Supply(Combatant owner, AbilityDefinition ability)
    {
        if (owner.Member is null) return null;
        if (ability.Item is not null) return $"IN THE BAG  ·  {_combat.Bag?.Count(ability.Item) ?? 0}";
        if (ability.Kind == AbilityKind.Potion) return $"HEALING POTIONS IN THE BAG  ·  {HealingPotions()}";
        if (ability.Resource is null) return null;

        ResourceDefinition resource = ResourceCatalog.Get(ability.Resource);
        string back = resource.Recharge == RestKind.Short ? "A SHORT REST" : "A LONG REST";
        return $"{resource.Name.ToUpperInvariant()}  ·  {ResourceCatalog.Left(owner.Member, ability.Resource)} OF {resource.Max(owner.Member)} LEFT, BACK ON {back}";
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

    /// <summary>Who or what an ability's aimed at, in capitals.</summary>
    private static string TargetWord(AbilityDefinition ability) => ability.Target switch
    {
        TargetKind.Self => "YOURSELF",
        TargetKind.Ally => ability.NotSelf ? "AN ALLY" : "YOU OR AN ALLY",
        TargetKind.Enemy => "A FOE",
        TargetKind.Point => "A TILE",
        _ => "WHERE IT LANDS",
    };

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
