using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// The AI: plays a whole turn for whoever's turn it is, foe or guest (or, under --simulate, the
/// party), by scoring everything they could do from everywhere they could get to, doing the best,
/// and going again until nothing left is worth doing.
/// </summary>
/// <remarks>
/// The Black Box's OpponentMind for a grid, with spec 6.7's priorities as utility weights, every one
/// a named constant so tuning is done here and not in the cases. A score is in points of damage: an
/// attack is worth its hit chance times its damage, plus a bonus for the chance it drops the target;
/// healing a point is worth a little more than dealing one; a path costs the damage its opportunity
/// attacks would do. It has no randomness of its own and walks everything in the same order, so the
/// same seed plays the same fight. The party's side plays by the same sums.
/// </remarks>
public static class EnemyMind
{
    // Damage, and finishing (spec 6.7: "finishing a Downed or low-HP target", "the best hit chance
    // times expected damage").

    /// <summary>What a point of expected damage is worth: the unit everything else is measured in.</summary>
    private const double DamageWeight = 1.0;

    /// <summary>What dropping a target is worth on top of its damage, times the chance this does it: one fewer of them acting.</summary>
    private const double KillWeight = 6.0;

    /// <summary>What a target's missing health is worth, times the hit chance, so the hurt get focused.</summary>
    private const double LowHealthWeight = 2.0;

    /// <summary>How much a target's own damage a round makes it worth hitting first, per point.</summary>
    private const double DangerWeight = 0.15;

    /// <summary>What a hit on a Downed party member is worth to a foe: a failed death save, two from beside them. Below most standing targets, so foes finish the fallen only when nobody's standing in reach.</summary>
    private const double FinishDownedWeight = 0.5;

    // Keeping range, and opportunity attacks (spec 6.7).

    /// <summary>
    /// What being beside a foe costs someone who fights from range (disadvantage, and a swing at them
    /// when they leave), counted against where they are now: moving in costs it, stepping away earns
    /// it, and staying put is free, so somebody already caught in melee still does something there.
    /// </summary>
    private const double KeepRangeWeight = 4.0;

    /// <summary>What a point of damage expected from opportunity attacks on the way costs. Exactly a point, so a move is taken when what it buys is worth more than what it risks.</summary>
    private const double OpportunityWeight = 1.0;

    /// <summary>How much opportunity damage on a path makes a bonus-action Disengage worth spending first.</summary>
    private const double DisengageWorth = 2.0;

    /// <summary>What every step costs, so of two equally good places the nearer wins and nobody wanders.</summary>
    private const double StepCost = 0.05;

    // Healing and getting people up.

    /// <summary>What getting a Downed ally back up is worth the first time: more than anything but finishing a fight.</summary>
    private const double ReviveWeight = 30.0;

    /// <summary>
    /// What getting someone up is worth for each time they've already gone down this fight, as a share
    /// of the time before. Someone knocked straight back down soaks an attack, which is worth a turn or
    /// two, but not every turn: the first draft got the same ally up at 1 HP forty times in a fight it
    /// never finished, while the foe that kept dropping them was never hit.
    /// </summary>
    private const double RevivedAgainShare = 0.5;

    /// <summary>What each failed death save adds to getting someone up in a fight to the death, since getting up wipes them and the third one kills.</summary>
    private const double DyingWeight = 8.0;

    /// <summary>The share of it Help's 1 HP is worth, against a heal that gives more.</summary>
    private const double HelpReviveShare = 0.8;

    /// <summary>What a point of healing is worth on someone hurt enough to need it.</summary>
    private const double HealWeight = 1.2;

    /// <summary>The share of their hit points under which someone's worth healing.</summary>
    private const double HealBelow = 0.5;

    /// <summary>The share of their hit points under which someone drinks a potion: lower, since the bag's shared.</summary>
    private const double PotionBelow = 0.35;

    // Abilities and bonus actions (spec 6.7: "using a bonus action or ability when it has one").

    /// <summary>What spending a slot, a use or an item costs, so cantrips and plain attacks come first unless the spend buys more.</summary>
    private const double SpendCost = 2.0;

    /// <summary>What setting up a turn is worth while the action's still to come (Hex before the attack, inspiration before the ally's), so it's done first.</summary>
    private const double SetupBonus = 10.0;

    /// <summary>What Hex is worth on a healthy target, scaled down as the target's hit points go.</summary>
    private const double HexValue = 4.0;

    /// <summary>What a bardic inspiration die is worth to an ally.</summary>
    private const double InspireValue = 3.0;

    /// <summary>What helping a standing ally's next attack is worth.</summary>
    private const double HelpValue = 1.0;

    /// <summary>What dodging is worth for each foe beside them, for someone with nothing better to do.</summary>
    private const double DodgeValue = 1.0;

    /// <summary>What a smoke bomb is worth for each foe beside them, once they're hurt below this share.</summary>
    private const double SmokeValue = 4.0;

    /// <summary>The share of their hit points under which a smoke bomb starts to look good.</summary>
    private const double SmokeBelow = 0.75;

    /// <summary>What an elixir of focus is worth to a caster with a slot spent.</summary>
    private const double ElixirValue = 3.0;

    /// <summary>What the Soul-Link Strike is worth on top of its damage, so a full bond gets spent rather than saved.</summary>
    private const double SoulLinkBonus = 4.0;

    /// <summary>What catching an ally in an area is worth, against catching a foe: it counts against, and a bit more.</summary>
    private const double FriendlyFireWeight = 1.5;

    /// <summary>What putting each condition on a foe is worth, times the chance it sticks.</summary>
    private static readonly Dictionary<ConditionId, double> RiderValues = new()
    {
        [ConditionId.Restrained] = 6.0,
        [ConditionId.Prone] = 2.0,
        [ConditionId.Mocked] = 1.5,
        [ConditionId.Slowed] = 0.5,
    };

    /// <summary>The most things one turn tries, as a backstop: a move and an action, a bonus action, an off-hand attack, and spare.</summary>
    private const int MostDecisions = 6;

    /// <summary>One thing that could be done: walk a path, then use an ability on someone or at a tile.</summary>
    /// <param name="Ability">What.</param>
    /// <param name="Target">At whom, or the user for one on themselves.</param>
    /// <param name="Aim">The tile it's aimed at: the target's, or an area's middle.</param>
    /// <param name="Path">The tiles to walk first, possibly none.</param>
    /// <param name="Risk">What the walk risks in opportunity attacks.</param>
    /// <param name="Score">What it's worth, all told.</param>
    private sealed record Option(AbilityDefinition Ability, Combatant Target, Point Aim, List<Point> Path, double Risk, double Score);

    /// <summary>Plays someone's turn: the best thing, then the next best, until nothing's worth doing; then, if they did nothing, closes in.</summary>
    /// <param name="engine">The fight.</param>
    /// <param name="who">Whose turn it is.</param>
    internal static void Play(CombatEngine engine, Combatant who)
    {
        Decide(engine, who);
        if (!Playing(engine, who) || !who.Budget.Action) return;

        CloseIn(engine, who);
        Decide(engine, who);
    }

    /// <summary>Does the best thing, again and again, while there's something worth doing.</summary>
    private static void Decide(CombatEngine engine, Combatant who)
    {
        for (int decision = 0; decision < MostDecisions && Playing(engine, who); decision++)
        {
            if (Best(engine, who) is not Option best || !Execute(engine, who, best)) return;
        }
    }

    /// <summary>Whether it's still their turn to play.</summary>
    private static bool Playing(CombatEngine engine, Combatant who) =>
        engine.Outcome == CombatOutcome.Ongoing && engine.Current == who && !who.IsOut;

    /// <summary>Walks an option's path (Disengaging first if it's dangerous and a bonus action can do it) and uses its ability.</summary>
    /// <returns>Whether it got as far as using the ability.</returns>
    private static bool Execute(CombatEngine engine, Combatant who, Option option)
    {
        if (option.Path.Count > 0)
        {
            if (option.Risk >= DisengageWorth) SlipAway(engine, who);
            engine.Walk(option.Path);
            if (!Playing(engine, who)) return false;
        }
        Combatant target = option.Ability.Target == TargetKind.Self ? who : option.Target;
        return engine.Perform(option.Ability, target, option.Aim);
    }

    /// <summary>Spends a bonus action on disengaging (Cunning Action, a smoke bomb), if they have one to spare.</summary>
    private static void SlipAway(CombatEngine engine, Combatant who)
    {
        foreach (AbilityDefinition ability in engine.Abilities(who))
        {
            bool disengages = ability.Cost == ActionCost.BonusAction && ability.Conditions.Contains(ConditionId.Disengaged);
            if (!disengages || engine.Unusable(who, ability) is not null) continue;
            engine.Perform(ability, who, who.Cell);
            return;
        }
    }

    /// <summary>Scores everything they could do, from everywhere they could get to, and picks the best worth doing.</summary>
    /// <returns>The best option, or null if nothing's worth more than nothing.</returns>
    private static Option Best(CombatEngine engine, Combatant who)
    {
        Dictionary<Point, Point> came = engine.Flood(who, who.Budget.MovementLeft);
        Dictionary<Point, double> risk = Risks(engine, who, came);
        bool skirmisher = Skirmisher(engine, who);
        double caught = skirmisher && engine.HostileBeside(who, who.Cell) ? KeepRangeWeight : 0.0;

        var ends = new List<Point>();
        var cost = new Dictionary<Point, double>();
        double floor = 0.0;
        foreach (Point cell in came.Keys)
        {
            if (cell != who.Cell && engine.At(cell, who) is not null) continue;
            ends.Add(cell);
            double keep = (skirmisher && engine.HostileBeside(who, cell) ? KeepRangeWeight : 0.0) - caught;
            cost[cell] = risk[cell] * OpportunityWeight + keep + StepCost * Measure.Steps(who.Cell, cell);
            floor = Math.Min(floor, cost[cell]);
        }

        var places = new Dictionary<(float, bool, Point), Point?>();
        Option best = null;

        void Consider(AbilityDefinition ability, Combatant target, Point aim, Point from, double value)
        {
            double score = value - cost[from];
            if (score <= 0.0 || best is not null && score <= best.Score) return;
            List<Point> path = from == who.Cell ? new List<Point>() : CombatEngine.PathIn(came, from);
            best = new Option(ability, target, aim, path, risk[from], score);
        }

        Point? PlaceFor(AbilityDefinition ability, Point aim)
        {
            float range = engine.RangeOf(who, ability);
            var key = (range, ability.NeedsSight, aim);
            if (!places.TryGetValue(key, out Point? place))
            {
                place = Nearest(engine, who, ability, aim, ends, cost, floor);
                places[key] = place;
            }
            return place;
        }

        foreach (AbilityDefinition ability in engine.Abilities(who))
        {
            if (engine.Unusable(who, ability) is not null) continue;
            double spend = ability.Resource is not null || ability.Item is not null || ability.RechargeOn > 0 ? SpendCost : 0.0;

            switch (ability.Target)
            {
                case TargetKind.Self when ability.Radius > 0f:
                    foreach (Point end in ends) Consider(ability, who, end, end, Burst(engine, who, ability, end, end) - spend);
                    break;

                case TargetKind.Self:
                    Consider(ability, who, who.Cell, who.Cell, Selfish(engine, who, ability) - spend);
                    break;

                case TargetKind.Enemy:
                case TargetKind.Ally:
                    foreach (Combatant target in engine.Combatants)
                    {
                        if (engine.TargetFault(who, ability, target, target.Cell) is not null) continue;
                        if (PlaceFor(ability, target.Cell) is not Point from) continue;
                        Consider(ability, target, target.Cell, from, Value(engine, who, ability, target, from) - spend);
                    }
                    break;

                case TargetKind.Area:
                    foreach (Combatant target in engine.Combatants)
                    {
                        if (!CombatEngine.Hostile(who, target) || engine.Beaten(target)) continue;
                        if (PlaceFor(ability, target.Cell) is not Point from) continue;
                        Consider(ability, target, target.Cell, from, Burst(engine, who, ability, target.Cell, from) - spend);
                    }
                    break;
            }
        }
        return best;
    }

    /// <summary>The cheapest tile they could get to from which an ability reaches a tile, the one they're on if nowhere's cheaper.</summary>
    /// <param name="floor">The cheapest any tile they could get to costs, so the search can stop at once when it's where they are.</param>
    private static Point? Nearest(CombatEngine engine, Combatant who, AbilityDefinition ability, Point aim, List<Point> ends, Dictionary<Point, double> cost, double floor)
    {
        if (cost[who.Cell] <= floor && engine.ReachFrom(who, ability, who.Cell, aim) is null) return who.Cell;

        Point? nearest = null;
        double cheapest = double.MaxValue;
        float range = engine.RangeOf(who, ability);
        foreach (Point end in ends)
        {
            if (!Measure.Within(end, aim, range) || cost[end] >= cheapest) continue;
            if (ability.NeedsSight && !engine.CanSee(end, aim)) continue;
            nearest = end;
            cheapest = cost[end];
        }
        return nearest;
    }

    /// <summary>What walking to each tile they could reach risks in opportunity attacks, built out along the flood so each tile adds its last step to its parent's.</summary>
    private static Dictionary<Point, double> Risks(CombatEngine engine, Combatant who, Dictionary<Point, Point> came)
    {
        var risk = new Dictionary<Point, double>();
        bool free = who.Has(ConditionId.Disengaged);

        double At(Point cell)
        {
            if (risk.TryGetValue(cell, out double known)) return known;
            Point parent = came[cell];
            double step = 0.0;
            if (parent != cell && !free)
            {
                foreach (Combatant threat in engine.ThreatsAlong(who, parent, new[] { cell }))
                    step += AttackMath.HitChance(threat.Melee.Bonus, who.ArmourClass, Advantage.Normal) * threat.Melee.AverageDamage;
            }
            double total = parent == cell ? 0.0 : At(parent) + step;
            risk[cell] = total;
            return total;
        }

        foreach (Point cell in came.Keys) At(cell);
        return risk;
    }

    /// <summary>What using an ability on someone is worth from a tile.</summary>
    private static double Value(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant target, Point from)
    {
        switch (ability.Kind)
        {
            case AbilityKind.Heal:
                AttackPreview heal = engine.Estimate(who, ability, target, from);
                return Mending(engine, target, (heal.Min + heal.Max) / 2.0, HealBelow);

            case AbilityKind.Help:
                if (target.IsDown) return Reviving(engine, target, HelpReviveShare);
                return target.Has(ConditionId.Helped) ? 0.0 : HelpValue;

            case AbilityKind.Condition:
                return Bestowing(engine, who, ability, target);

            default:
                return Harm(engine, who, ability, target, from);
        }
    }

    /// <summary>What hurting someone is worth: the damage, the chance of dropping them, how hurt and how dangerous they are, and anything it puts on them.</summary>
    private static double Harm(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant target, Point from)
    {
        AttackPreview odds = engine.Estimate(who, ability, target, from);
        if (target.IsDown) return odds.Chance * FinishDownedWeight;

        double average = (odds.Min + odds.Max) / 2.0;
        double value = odds.Chance * Math.Min(average, target.Hp) * DamageWeight
            + odds.Chance * DropChance(odds, target.Hp) * KillWeight
            + odds.Chance * (1.0 - target.Hp / (double)target.MaxHp) * LowHealthWeight
            + odds.Chance * target.MainHand.AverageDamage * DangerWeight;

        foreach (ConditionId condition in ability.Conditions)
            value += odds.Chance * RiderValues.GetValueOrDefault(condition);
        return ability.Kind == AbilityKind.SoulLink ? value + SoulLinkBonus : value;
    }

    /// <summary>What an area or a burst is worth from a tile: what it does to every foe it catches, less what it does to every ally.</summary>
    private static double Burst(CombatEngine engine, Combatant who, AbilityDefinition ability, Point centre, Point from)
    {
        double value = 0.0;
        double full = ability.Damage.IsNone ? 0.0 : ability.Damage.Average;
        foreach (Combatant caught in engine.Combatants)
        {
            bool self = caught == who && ability.Target == TargetKind.Self;
            Point middle = ability.Target == TargetKind.Self ? from : centre;
            if (self || caught.IsOut || engine.Beaten(caught) || !Measure.Within(caught.Cell == who.Cell ? from : caught.Cell, middle, ability.Radius)) continue;

            double fails = engine.Estimate(who, ability, caught, from).Chance;
            double damage = fails * full + (ability.SaveHalves ? (1.0 - fails) * full / 2.0 : 0.0);
            double each = Math.Min(damage, caught.Hp);
            foreach (ConditionId condition in ability.Conditions) each += fails * RiderValues.GetValueOrDefault(condition);

            value += CombatEngine.Hostile(who, caught) ? each : -each * FriendlyFireWeight;
        }
        return value;
    }

    /// <summary>What putting a condition on someone else is worth: Hex on a foe, inspiration on an ally.</summary>
    private static double Bestowing(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant target)
    {
        double setup = who.Budget.Action ? SetupBonus : 0.0;
        if (ability.Conditions.Contains(ConditionId.Hexed))
        {
            bool hexing = who.Concentrating == AbilityId.Hex && !engine.HexCanMove(who);
            return hexing ? 0.0 : HexValue * target.Hp / target.MaxHp + setup;
        }
        if (ability.Conditions.Contains(ConditionId.Inspired)) return target.IsOut ? 0.0 : InspireValue;
        return 0.0;
    }

    /// <summary>What something on themselves is worth: a breather or a potion when hurt, a dodge or a smoke bomb with foes beside them.</summary>
    private static double Selfish(CombatEngine engine, Combatant who, AbilityDefinition ability)
    {
        switch (ability.Kind)
        {
            case AbilityKind.Heal:
                AttackPreview heal = engine.Estimate(who, ability, who, who.Cell);
                return Mending(engine, who, (heal.Min + heal.Max) / 2.0, HealBelow);

            case AbilityKind.Potion:
                AttackPreview potion = engine.Estimate(who, ability, who, who.Cell);
                if (potion.Max > 0) return Mending(engine, who, (potion.Min + potion.Max) / 2.0, PotionBelow);
                return who.Casting is not null ? ElixirValue : 0.0;

            case AbilityKind.Condition:
                int beside = FoesBeside(engine, who);
                bool dodges = ability.Conditions.Contains(ConditionId.Dodging);
                if (!dodges || beside == 0) return 0.0;
                bool smoke = ability.Conditions.Contains(ConditionId.Disengaged);
                if (!smoke) return DodgeValue * beside;
                return who.Hp < who.MaxHp * SmokeBelow ? SmokeValue * beside : 0.0;

            default:
                return 0.0;
        }
    }

    /// <summary>What healing someone is worth: getting them up if they're Downed, otherwise the points that make up what they're missing, if they're hurt enough to need it.</summary>
    private static double Mending(CombatEngine engine, Combatant target, double heals, double below)
    {
        if (target.IsDown) return Reviving(engine, target, 1.0);
        if (target.Hp >= target.MaxHp * below) return 0.0;
        return HealWeight * Math.Min(heals, target.MaxHp - target.Hp);
    }

    /// <summary>What getting a Downed ally up is worth: less each time they've gone down already this fight, and more for every failed death save if they're dying.</summary>
    /// <param name="engine">The fight.</param>
    /// <param name="ally">Who's Downed.</param>
    /// <param name="share">How much of a full heal's worth this is: Help's 1 HP is less.</param>
    private static double Reviving(CombatEngine engine, Combatant ally, double share)
    {
        double again = Math.Pow(RevivedAgainShare, Math.Max(0, ally.Downs - 1));
        bool dying = !engine.Encounter.Nonlethal && !ally.Has(ConditionId.Stable);
        return ReviveWeight * share * again + (dying ? ally.DeathFailures * DyingWeight : 0.0);
    }

    /// <summary>The chance damage between Min and Max drops someone with this many hit points, counting it as even across the range.</summary>
    private static double DropChance(AttackPreview odds, int hp)
    {
        if (odds.Max < hp) return 0.0;
        if (odds.Min >= hp) return 1.0;
        return (odds.Max - hp + 1) / (double)(odds.Max - odds.Min + 1);
    }

    /// <summary>How many foes still fighting are beside them.</summary>
    private static int FoesBeside(CombatEngine engine, Combatant who)
    {
        int count = 0;
        foreach (Combatant each in engine.Combatants)
            if (CombatEngine.Hostile(each, who) && !engine.Beaten(each) && Measure.Within(each.Cell, who.Cell, Measure.MeleeReach)) count++;
        return count;
    }

    /// <summary>Whether they fight from range and want to stay out of reach: a ranged foe, or a party member with a bow or a ranged cantrip.</summary>
    private static bool Skirmisher(CombatEngine engine, Combatant who)
    {
        if (who.Enemy is not null) return who.Enemy.Ranged;
        if (who.MainHand.Ranged) return true;
        foreach (AbilityDefinition ability in engine.Abilities(who))
            if (ability.Cantrip && ability.Target == TargetKind.Enemy && ability.Range > Measure.MeleeReach) return true;
        return false;
    }

    /// <summary>Nothing in reach was worth doing, so they close in on the nearest foe still fighting they can find a way to (or, for a foe with nobody standing, the nearest Downed): a Dash first if it's too far to walk, and stopping as soon as they're in range if they fight from range. Not with a foe beside them already, since walking off would only provoke.</summary>
    private static void CloseIn(CombatEngine engine, Combatant who)
    {
        if (FoesBeside(engine, who) > 0) return;

        float reach = Measure.MeleeReach;
        foreach (AbilityDefinition ability in engine.Abilities(who))
        {
            bool harms = ability.Kind is AbilityKind.Weapon or AbilityKind.SpellAttack or AbilityKind.Save or AbilityKind.AutoHit;
            if (harms && ability.Target == TargetKind.Enemy && engine.Unusable(who, ability) is null) reach = MathF.Max(reach, engine.RangeOf(who, ability));
        }

        foreach (Combatant quarry in Quarries(engine, who))
        {
            List<Point> path = Approach(engine, who, quarry, reach);
            if (path is null || path.Count == 0) continue;

            if (!who.Budget.CanWalk(engine.Cost(path))) Dash(engine, who);
            engine.Walk(path);
            return;
        }
    }

    /// <summary>Who to close in on, nearest first: the foes still standing, then, if nobody is and the fight isn't one to yield, the Downed.</summary>
    /// <remarks>A list rather than the one, since the nearest can be walled in by its own side, and the next one along is still worth walking to.</remarks>
    private static List<Combatant> Quarries(CombatEngine engine, Combatant who)
    {
        var standing = new List<Combatant>();
        var fallen = new List<Combatant>();
        foreach (Combatant each in engine.Combatants)
        {
            if (!CombatEngine.Hostile(each, who) || each.IsDead || engine.Beaten(each) && each.Side == Side.Foes) continue;
            if (!each.IsDown) standing.Add(each);
            else if (!engine.Encounter.Nonlethal) fallen.Add(each);
        }

        int Nearer(Combatant one, Combatant other)
        {
            int order = Measure.Steps(who.Cell, one.Cell).CompareTo(Measure.Steps(who.Cell, other.Cell));
            return order != 0 ? order : one.Joined.CompareTo(other.Joined);
        }

        standing.Sort(Nearer);
        if (standing.Count > 0) return standing;
        fallen.Sort(Nearer);
        return fallen;
    }

    /// <summary>The way to a quarry: to a free tile beside them, cut short at the first tile in range and sight for someone who fights from further off.</summary>
    private static List<Point> Approach(CombatEngine engine, Combatant who, Combatant quarry, float reach)
    {
        List<Point> best = null;
        foreach (Point step in Around(quarry.Cell))
        {
            List<Point> path = engine.FindPath(who, who.Cell, step);
            if (path is not null && (best is null || path.Count < best.Count)) best = path;
        }
        if (best is null) return null;

        for (int i = 0; i < best.Count; i++)
        {
            bool inRange = Measure.Within(best[i], quarry.Cell, reach) && engine.CanSee(best[i], quarry.Cell);
            if (inRange && engine.At(best[i], who) is null) return best.GetRange(0, i + 1);
        }
        return best;
    }

    /// <summary>The eight tiles round one.</summary>
    private static IEnumerable<Point> Around(Point cell)
    {
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                if (dx != 0 || dy != 0) yield return new Point(cell.X + dx, cell.Y + dy);
    }

    /// <summary>Dashes, with a bonus action if they have one (Cunning Action) so the action's kept, otherwise with the action.</summary>
    private static void Dash(CombatEngine engine, Combatant who)
    {
        AbilityDefinition chosen = null;
        foreach (AbilityDefinition ability in engine.Abilities(who))
        {
            if (ability.Kind != AbilityKind.Dash || engine.Unusable(who, ability) is not null) continue;
            if (chosen is null || ability.Cost == ActionCost.BonusAction) chosen = ability;
        }
        if (chosen is not null) engine.Perform(chosen, who, who.Cell);
    }
}
