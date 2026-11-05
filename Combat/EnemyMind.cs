using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendQuest.Combat;

/// <summary>
/// The AI: plays a whole turn for whoever's turn it is, foe or guest (or, under --simulate, the
/// party), by scoring everything they could do from every slot a Shift could take them to, doing the
/// best, and going again until nothing left is worth doing.
/// </summary>
/// <remarks>
/// The Black Box's OpponentMind, for rows, with spec 6.7's priorities as utility weights, every one a
/// named constant so tuning is done here and not in the cases. A score is in points of damage: an
/// attack is worth its hit chance times its damage, plus a bonus for the chance it drops the target;
/// healing a point is worth a little more than dealing one; a Shift costs what its opportunity
/// attack would do, and earns whatever it does for where their side stands: archers and casters
/// behind a front row, and the side as little open to the foes' melee as it can be, counted by the
/// same sums the foes attack by. Each Shift is tried on with Formation.Try and put back, so a look
/// never moves anyone. It has no randomness of its own and walks everything in the same order, so
/// the same seed plays the same fight. The party's side plays by the same sums.
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

    /// <summary>What a hit on a Downed party member is worth to a foe: a failed death save, two for a melee critical. Below most standing targets, so foes finish the fallen only when nobody's standing in reach.</summary>
    private const double FinishDownedWeight = 0.5;

    // Keeping range, and opportunity attacks (spec 6.7).

    /// <summary>
    /// What standing behind a front row is worth to someone who fights from range (no melee can reach
    /// them there, and no swing when they'd leave), counted for their whole side against where it
    /// stands now: a Shift that puts one of them in front costs it, one that gets them out or screens
    /// them earns it, and staying put is free, so somebody already caught in front still does
    /// something there.
    /// </summary>
    private const double KeepRangeWeight = 4.0;

    /// <summary>What a point of damage expected from the opportunity attack a Shift draws costs. Exactly a point, so a Shift is taken when what it buys is worth more than what it risks.</summary>
    private const double OpportunityWeight = 1.0;

    /// <summary>
    /// What a point their side is open to in melee costs: for each foe who can swing at them, what
    /// its best swing at whoever it can reach is worth to it, by the same sums it attacks by (the
    /// damage, the chance it drops them, how hurt they are). Exactly a point, so the fragile step back
    /// behind the sturdy (a rogue who can Backstab from there, a hurt friend behind a full shield)
    /// when it saves more than it costs them to do there. Two points, not one, since a round in front
    /// is seldom only one: getting out again costs a turn and a swing, and a trip or a net keeps you.
    /// </summary>
    private const double ExposureWeight = 2.0;

    /// <summary>How much opportunity damage on a Shift makes a bonus action that slips away (Cunning Step, a smoke bomb) worth spending first.</summary>
    private const double DisengageWorth = 2.0;

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

    /// <summary>The share of their hit points under which someone drinks a potion, or is handed one: lower, since the bag's shared.</summary>
    private const double PotionBelow = 0.35;

    // Skills and bonus actions (spec 6.7: "using a bonus action or ability when it has one").

    /// <summary>What a skill with a price costs on top of what it does (Focus, a cooldown, once a fight, a recharge, something from the bag), so autos and plain attacks come first unless the spend buys more.</summary>
    private const double SpendCost = 2.0;

    /// <summary>What setting up a turn is worth while the action's still to come (Hex before the attack, inspiration before the ally's), so it's done first.</summary>
    private const double SetupBonus = 10.0;

    /// <summary>What Hex is worth on a healthy target, scaled down as the target's hit points go.</summary>
    private const double HexValue = 4.0;

    /// <summary>What a bardic inspiration die is worth to an ally.</summary>
    private const double InspireValue = 3.0;

    /// <summary>What helping a standing ally's next attack is worth.</summary>
    private const double HelpValue = 1.0;

    /// <summary>What dodging is worth for each foe that can reach them in melee, for someone with nothing better to do.</summary>
    private const double DodgeValue = 1.0;

    /// <summary>What a smoke bomb is worth for each foe that can reach them in melee, once they're hurt below this share.</summary>
    private const double SmokeValue = 4.0;

    /// <summary>The share of their hit points under which a smoke bomb starts to look good.</summary>
    private const double SmokeBelow = 0.75;

    /// <summary>What an elixir of focus is worth to someone short of the Focus for their dearest skill.</summary>
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

    /// <summary>The most things one turn tries, as a backstop: a Shift, an action, a bonus action, a Shift a skill gave back, and spare.</summary>
    private const int MostDecisions = 6;

    /// <summary>One thing that could be done: perhaps a Shift first, then perhaps an ability on someone.</summary>
    /// <param name="Ability">What, or null for a Shift on its own.</param>
    /// <param name="Target">At whom: the user for one on themselves; null for a Shift on its own.</param>
    /// <param name="Shift">The slot to Shift to, or null to stay: first, unless the ability's on themselves and the Shift's dangerous.</param>
    /// <param name="Risk">What the Shift risks in an opportunity attack: its damage, and the chance it drops them.</param>
    /// <param name="Score">What it's worth, all told.</param>
    private sealed record Option(AbilityDefinition Ability, Combatant Target, Slot? Shift, double Risk, double Score);

    /// <summary>Plays someone's turn: the best thing, then the next best, until nothing's worth doing. A Shift left after the action is weighed with the rest, on where it'd leave their side.</summary>
    /// <param name="engine">The fight.</param>
    /// <param name="who">Whose turn it is.</param>
    internal static void Play(CombatEngine engine, Combatant who)
    {
        for (int decision = 0; decision < MostDecisions && Playing(engine, who); decision++)
        {
            if (Best(engine, who, true) is not Option best || !Execute(engine, who, best)) return;
        }
    }

    /// <summary>What someone would do with their turn if it came now, without doing it: the intent icon over a foe.</summary>
    /// <remarks>
    /// On their own turn it's the pick Play would make first, a Shift weighed in. Otherwise it's from
    /// where they stand, with their skills as they'd be at the start of their next turn (its Focus
    /// gained, a turn off every cooldown), since their Shift and actions come back only then. Nothing
    /// moves and nothing's rolled, so asking changes nothing in the fight.
    /// </remarks>
    /// <param name="engine">The fight.</param>
    /// <param name="who">Who.</param>
    /// <returns>The ability and its target, or null if nothing's worth doing (or the best is a Shift and nothing more).</returns>
    internal static (AbilityDefinition Ability, Combatant Target)? Predict(CombatEngine engine, Combatant who)
    {
        if (engine.Outcome != CombatOutcome.Ongoing || who.IsOut || engine.Beaten(who)) return null;
        Option best = Best(engine, who, engine.Current == who);
        return best?.Ability is null ? null : (best.Ability, best.Target);
    }

    /// <summary>Whether it's still their turn to play.</summary>
    private static bool Playing(CombatEngine engine, Combatant who) =>
        engine.Outcome == CombatOutcome.Ongoing && engine.Current == who && !who.IsOut;

    /// <summary>Shifts if the option says to (slipping away first if the Shift's dangerous and a bonus action can do it) and uses its ability: the ability first when it's on themselves and the Shift's dangerous, so a potion's drunk before running past the foe, not after.</summary>
    /// <returns>Whether it all went through, so there's more to decide.</returns>
    private static bool Execute(CombatEngine engine, Combatant who, Option option)
    {
        if (option.Shift is not Slot to) return option.Ability is null || engine.Perform(option.Ability, option.Target);

        bool first = option.Ability is not null && option.Target == who && option.Risk > 0.0;
        if (first && (!engine.Perform(option.Ability, who) || !Playing(engine, who))) return false;
        if (option.Risk >= DisengageWorth && option.Ability?.Cost != ActionCost.BonusAction) SlipAway(engine, who);
        if (!engine.DoShift(who, to) || !Playing(engine, who)) return false;
        return first || option.Ability is null || engine.Perform(option.Ability, option.Target);
    }

    /// <summary>Spends a bonus action on slipping away before a dangerous Shift (Cunning Step, a smoke bomb), if they have one to spare.</summary>
    private static void SlipAway(CombatEngine engine, Combatant who)
    {
        foreach (AbilityDefinition ability in engine.Abilities(who))
        {
            bool slips = ability.Cost == ActionCost.BonusAction && (ability.FreeShift || ability.Conditions.Contains(ConditionId.Disengaged));
            if (!slips || engine.Unusable(who, ability) is not null || engine.WhyNot(who, ability, who) is not null) continue;
            engine.Perform(ability, who);
            return;
        }
    }

    /// <summary>Whether they've a bonus action to slip away with before a dangerous Shift, ready now.</summary>
    private static bool CanSlip(CombatEngine engine, Combatant who)
    {
        foreach (AbilityDefinition ability in engine.Abilities(who))
        {
            bool slips = ability.Cost == ActionCost.BonusAction && (ability.FreeShift || ability.Conditions.Contains(ConditionId.Disengaged));
            if (slips && engine.Unusable(who, ability) is null) return true;
        }
        return false;
    }

    /// <summary>Scores everything they could do, from where they stand and from every slot they could Shift to, and picks the best worth doing.</summary>
    /// <param name="engine">The fight.</param>
    /// <param name="who">Who.</param>
    /// <param name="live">Whether it's their turn now, so their budget and their Shift count; otherwise it's a look ahead from where they stand.</param>
    /// <returns>The best option, or null if nothing's worth more than nothing.</returns>
    private static Option Best(CombatEngine engine, Combatant who, bool live)
    {
        var skirmishers = new Dictionary<Combatant, bool>();
        foreach (Combatant each in engine.Combatants)
            if (each.Side == who.Side) skirmishers[each] = Skirmisher(engine, each);
        List<(Combatant Foe, AbilityDefinition Auto, Reach Reach)> swingers = Swingers(engine, who.Side);
        bool slips = live && CanSlip(engine, who);

        double here = Position(engine, who.Side, skirmishers, swingers);
        Option best = null;

        void Consider(AbilityDefinition ability, Combatant target, Slot? shift, double risk, double score)
        {
            if (score <= 0.0 || best is not null && score <= best.Score) return;
            best = new Option(ability, target, shift, risk, score);
        }

        // A dangerous Shift costs what it risks, or, when a bonus action can slip away first and the
        // option doesn't need the bonus action itself, what slipping away costs.
        double Hazard(AbilityDefinition ability, double risk) =>
            risk >= DisengageWorth && slips && ability?.Cost != ActionCost.BonusAction ? SpendCost : risk * OpportunityWeight;

        void ScoreAll(Slot? shift, double risk)
        {
            double moved = Position(engine, who.Side, skirmishers, swingers) - here;
            if (shift is not null) Consider(null, null, shift, risk, moved - Hazard(null, risk));

            foreach (AbilityDefinition ability in engine.Abilities(who))
            {
                if (ability.Kind == AbilityKind.Passive || !Ready(engine, who, ability, live)) continue;
                double spend = Spends(ability) ? SpendCost : 0.0;
                double cost = Hazard(ability, risk) - moved;
                foreach (Combatant target in Aims(engine, who, ability, live))
                    Consider(ability, target, shift, risk, Value(engine, who, ability, target) - spend - cost);
            }
        }

        ScoreAll(null, 0.0);
        if (!live || !who.Budget.Shift) return best;

        foreach (Slot to in engine.Shifts(who))
        {
            double risk = Danger(engine, who, to);
            using (engine.Formation.Try(who, to)) ScoreAll(to, risk);
        }
        return best;
    }

    /// <summary>What a Shift risks in an opportunity attack, by the sums a swing's worth: its expected damage, and the chance it drops them times KillWeight.</summary>
    private static double Danger(CombatEngine engine, Combatant who, Slot to)
    {
        double risk = engine.Risk(who, to);
        if (risk <= 0.0 || engine.Opportunist(who, to) is not Combatant threat || threat.Melee.AverageDamage <= 0.0) return risk;
        double chance = risk / threat.Melee.AverageDamage;
        return risk + chance * DropChance(threat.Melee.Damage.Min, threat.Melee.Damage.Max, who.Hp) * KillWeight;
    }

    /// <summary>Whether an ability could be used: now, on their turn, or (for a look ahead) at the start of their next one.</summary>
    /// <remarks>
    /// The look ahead goes by the engine's own prices (a stat block pays no Focus, and has its Focus
    /// skills once a fight instead), counts the Focus their next turn brings and a turn off the
    /// cooldown, and skips the turn and budget checks, since those come back with it. Something still
    /// recharging counts as not ready, though the roll at the start of the turn may bring it back.
    /// </remarks>
    private static bool Ready(CombatEngine engine, Combatant who, AbilityDefinition ability, bool live)
    {
        if (live) return engine.Unusable(who, ability) is null;
        if (CombatEngine.OnceAFight(who, ability) && who.UsedThisFight.Contains(ability.Id)) return false;
        if (engine.CooldownLeft(who, ability) > 1) return false;
        if (CombatEngine.FocusCost(who, ability) > Math.Min(Combatant.MaxFocus, who.Focus + CombatEngine.FocusPerTurn)) return false;
        if (ability.RechargeOn > 0 && who.IsSpent(ability.Id)) return false;
        if (ability.Kind == AbilityKind.SoulLink && !engine.SoulLink.IsFull) return false;
        return ability.Item is null || engine.Bag is not null && engine.Bag.Count(ability.Item) > 0;
    }

    /// <summary>Who an ability could be pointed at, from where they stand: the user for one on themselves, otherwise the engine's Targets that it would let them pick now, the beaten and the dead aside.</summary>
    /// <remarks>A shape that picks at random is only scored once, on the first of them, since who it's pointed at changes nothing.</remarks>
    private static IEnumerable<Combatant> Aims(CombatEngine engine, Combatant who, AbilityDefinition ability, bool live)
    {
        if (ability.Shape == TargetShape.Self)
        {
            if (!live || engine.WhyNot(who, ability, who) is null) yield return who;
            yield break;
        }

        foreach (Combatant target in engine.Targets(who, ability))
        {
            if (target.IsDead || CombatEngine.Hostile(who, target) && engine.Beaten(target) && target.Side == Side.Foes) continue;
            if (live && engine.WhyNot(who, ability, target) is not null) continue;
            yield return target;
            if (ability.Shape == TargetShape.RandomFoes) yield break;
        }
    }

    /// <summary>Whether using an ability spends something worth keeping: Focus, a cooldown, its once a fight, a recharge, or something from the bag.</summary>
    private static bool Spends(AbilityDefinition ability) =>
        ability.Focus > 0 || ability.Cooldown > 0 || ability.OncePerFight || ability.RechargeOn > 0 || ability.Item is not null;

    /// <summary>What using an ability pointed at someone is worth, over everyone its shape catches.</summary>
    private static double Value(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant target)
    {
        switch (ability.Kind)
        {
            case AbilityKind.Heal:
            case AbilityKind.Potion:
            case AbilityKind.Help:
                double mends = 0.0;
                foreach (Combatant each in Caught(engine, who, ability, target))
                    if (!CombatEngine.Hostile(who, each)) mends += Mends(engine, who, ability, each);
                return mends;

            case AbilityKind.Condition:
                return target == who ? Selfish(engine, who, ability) : Bestowing(engine, who, ability, target);

            case AbilityKind.Passive:
                return 0.0;

            default:
                return Harming(engine, who, ability, target);
        }
    }

    /// <summary>Everyone an ability pointed at someone catches: just them for a shape that picks one, otherwise the engine's Caught.</summary>
    private static IReadOnlyList<Combatant> Caught(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant target) =>
        ability.Shape is TargetShape.Self or TargetShape.Ally or TargetShape.AllyNextTo or TargetShape.Foe
            ? new[] { target }
            : engine.Caught(who, ability, target);

    /// <summary>What an ability that hurts is worth: what it does to every foe it catches, less what it does to every ally. Random picks are worth Count picks of the average foe it could pick.</summary>
    private static double Harming(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant target)
    {
        if (ability.Shape == TargetShape.RandomFoes)
        {
            double sum = 0.0;
            int pool = 0;
            foreach (Combatant each in engine.Targets(who, ability))
            {
                if (!CombatEngine.Hostile(who, each) || each.IsDead || engine.Beaten(each) && each.Side == Side.Foes) continue;
                sum += Harm(engine, who, ability, each);
                pool++;
            }
            return pool == 0 ? 0.0 : Math.Max(1, ability.Count) * sum / pool;
        }

        double value = 0.0;
        foreach (Combatant each in Caught(engine, who, ability, target))
        {
            if (each == who) continue;
            if (CombatEngine.Hostile(who, each)) value += Harm(engine, who, ability, each);
            else value -= Harm(engine, who, ability, each) * FriendlyFireWeight;
        }
        return value;
    }

    /// <summary>What hurting one person is worth: the damage, the chance of dropping them, how hurt and how dangerous they are, and anything it puts on them.</summary>
    /// <remarks>A save that halves counts the half on a made save, as the old area sums did; anything else counts its average on a hit.</remarks>
    private static double Harm(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant target)
    {
        AttackPreview odds = engine.Estimate(who, ability, target);
        if (target.IsDown) return odds.Chance * FinishDownedWeight;

        double damage;
        if (ability.Kind == AbilityKind.Save)
        {
            double full = ability.Damage.IsNone ? 0.0 : Math.Max(0.0, ability.Damage.Average);
            damage = odds.Chance * Math.Min(full, target.Hp) + (ability.SaveHalves ? (1.0 - odds.Chance) * Math.Min(full / 2.0, target.Hp) : 0.0);
        }
        else damage = odds.Chance * Math.Min((odds.Min + odds.Max) / 2.0, target.Hp);

        double value = damage * DamageWeight
            + odds.Chance * DropChance(odds, target.Hp) * KillWeight
            + odds.Chance * (1.0 - target.Hp / (double)target.MaxHp) * LowHealthWeight
            + odds.Chance * target.MainHand.AverageDamage * DangerWeight;

        foreach (ConditionId condition in ability.Conditions)
            value += odds.Chance * RiderValues.GetValueOrDefault(condition);
        return ability.Kind == AbilityKind.SoulLink ? value + SoulLinkBonus : value;
    }

    /// <summary>What healing, a potion or Help is worth to one ally: getting them up, the points they're missing if they're hurt enough to need them, focus for a skill they can't afford, or Help's advantage.</summary>
    private static double Mends(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant ally)
    {
        if (ally.IsDead) return 0.0;
        if (ability.Kind == AbilityKind.Help)
        {
            if (ally.IsDown) return Reviving(engine, ally, HelpReviveShare);
            return ally.Has(ConditionId.Helped) ? 0.0 : HelpValue;
        }

        AttackPreview heal = engine.Estimate(who, ability, ally);
        if (ability.Kind == AbilityKind.Potion && heal.Max <= 0) return ShortOfFocus(engine, ally) ? ElixirValue : 0.0;
        return Mending(engine, ally, (heal.Min + heal.Max) / 2.0, ability.Kind == AbilityKind.Potion ? PotionBelow : HealBelow);
    }

    /// <summary>Whether someone hasn't the Focus for the dearest skill they have.</summary>
    private static bool ShortOfFocus(CombatEngine engine, Combatant who)
    {
        foreach (AbilityDefinition ability in engine.Abilities(who))
            if (ability.Focus > who.Focus) return true;
        return false;
    }

    /// <summary>What putting a condition on someone else without hurting them is worth: Hex on a foe, inspiration on an ally, or, on foes, what each condition's worth times the chance it sticks.</summary>
    private static double Bestowing(CombatEngine engine, Combatant who, AbilityDefinition ability, Combatant target)
    {
        double setup = who.Budget.Action ? SetupBonus : 0.0;
        if (ability.Conditions.Contains(ConditionId.Hexed))
        {
            bool hexing = who.Concentrating == AbilityId.Hex && !engine.HexCanMove(who);
            return hexing ? 0.0 : HexValue * target.Hp / target.MaxHp + setup;
        }
        if (ability.Conditions.Contains(ConditionId.Inspired)) return target.IsOut ? 0.0 : InspireValue;

        double value = 0.0;
        foreach (Combatant each in Caught(engine, who, ability, target))
        {
            if (!CombatEngine.Hostile(who, each) || each.IsOut) continue;
            double sticks = engine.Estimate(who, ability, each).Chance;
            foreach (ConditionId condition in ability.Conditions) value += sticks * RiderValues.GetValueOrDefault(condition);
        }
        return value;
    }

    /// <summary>What something on themselves is worth: a dodge, a smoke bomb or Cunning Step with foes that can reach them in melee.</summary>
    private static double Selfish(CombatEngine engine, Combatant who, AbilityDefinition ability)
    {
        double threats = MeleeThreats(engine, who);
        bool dodges = ability.Conditions.Contains(ConditionId.Dodging);
        if (!dodges || threats == 0) return 0.0;
        bool smoke = ability.Conditions.Contains(ConditionId.Disengaged);
        if (!smoke) return DodgeValue * threats;
        return who.Hp < who.MaxHp * SmokeBelow ? SmokeValue * threats : 0.0;
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
    private static double DropChance(AttackPreview odds, int hp) => DropChance(odds.Min, odds.Max, hp);

    /// <summary>The chance damage between least and most drops someone with this many hit points, counting it as even across the range.</summary>
    private static double DropChance(int least, int most, int hp)
    {
        if (most < hp) return 0.0;
        if (least >= hp) return 1.0;
        return (most - hp + 1) / (double)(most - least + 1);
    }

    /// <summary>
    /// How many foes are likely to come at someone in melee, the rows' "foes beside them": those still
    /// fighting in the other side's front row, shared out over everyone standing in this one's, if
    /// this someone's in it. Every foe in front can reach every one of them, but each swings at one.
    /// </summary>
    private static double MeleeThreats(CombatEngine engine, Combatant who)
    {
        if (!engine.Formation.InFront(who)) return 0.0;
        Side other = who.Side == Side.Party ? Side.Foes : Side.Party;
        int foes = 0, screen = 0;
        foreach (Combatant each in engine.Formation.InRow(other, engine.Formation.FrontOf(other)))
            if (!each.IsOut && !engine.Beaten(each)) foes++;
        foreach (Combatant each in engine.Formation.InRow(who.Side, engine.Formation.FrontOf(who.Side)))
            if (!each.IsOut && !engine.Beaten(each)) screen++;
        return foes / (double)Math.Max(1, screen);
    }

    /// <summary>What where a side stands is worth: KeepRangeWeight for everyone standing who fights from range and has a front row between them and the foes, less what the foes' melee could do to it in a round.</summary>
    private static double Position(CombatEngine engine, Side side, Dictionary<Combatant, bool> skirmishers,
        List<(Combatant Foe, AbilityDefinition Auto, Reach Reach)> swingers)
    {
        double worth = 0.0;
        foreach ((Combatant each, bool skirmisher) in skirmishers)
        {
            if (!skirmisher || each.Side != side || !OnField(engine, each)) continue;
            if (!engine.Formation.InFront(each)) worth += KeepRangeWeight;
        }
        return worth - ExposureWeight * Exposure(engine, side, swingers);
    }

    /// <summary>The foes of a side who fight in melee or with reach, with their auto attack and its reach: where the side stands changes who they can get at.</summary>
    private static List<(Combatant Foe, AbilityDefinition Auto, Reach Reach)> Swingers(CombatEngine engine, Side side)
    {
        var swingers = new List<(Combatant, AbilityDefinition, Reach)>();
        foreach (Combatant foe in engine.Combatants)
        {
            if (foe.Side == side || !OnField(engine, foe)) continue;
            foreach (AbilityDefinition ability in engine.Abilities(foe))
            {
                if (ability.Bar != BarSlot.Auto || ability.Kind == AbilityKind.Passive) continue;
                Reach reach = engine.ReachOf(foe, ability);
                if (reach is Reach.Melee or Reach.Reach) swingers.Add((foe, ability, reach));
                break;
            }
        }
        return swingers;
    }

    /// <summary>What a side's open to in melee in a round: for each foe who can swing at any of them, what its auto attack would cost them on the one of those it can reach that it would pick, by its own sums.</summary>
    private static double Exposure(CombatEngine engine, Side side, List<(Combatant Foe, AbilityDefinition Auto, Reach Reach)> swingers)
    {
        double total = 0.0;
        foreach ((Combatant foe, AbilityDefinition auto, Reach reach) in swingers)
        {
            Combatant picked = null;
            double best = 0.0;
            foreach (Combatant each in engine.Combatants)
            {
                if (each.Side != side || !OnField(engine, each) || !engine.CanReach(foe, reach, each)) continue;
                double worth = Harm(engine, foe, auto, each);
                if (picked is not null && worth <= best) continue;
                picked = each;
                best = worth;
            }
            if (picked is not null) total += Exposed(engine, foe, auto, picked);
        }
        return total;
    }

    /// <summary>
    /// What one foe's swing at someone costs their side: what it's worth to the foe, and for one of
    /// the party proper, who'd lie there Downed until someone spent a turn on them, the chance it drops
    /// them times what getting them back up is worth.
    /// </summary>
    private static double Exposed(CombatEngine engine, Combatant foe, AbilityDefinition auto, Combatant who)
    {
        double harm = Harm(engine, foe, auto, who);
        if (who.Member is null || who.Guest) return harm;
        AttackPreview odds = engine.Estimate(foe, auto, who);
        return harm + odds.Chance * DropChance(odds, who.Hp) * Reviving(engine, who, 1.0);
    }

    /// <summary>Whether someone's standing in a slot: still fighting, and on the field.</summary>
    private static bool OnField(CombatEngine engine, Combatant who) =>
        !who.IsOut && !engine.Beaten(who) && engine.Formation.SlotsOf(who).Count > 0;

    /// <summary>Whether someone fights from range and wants a front row between them and the foes: a ranged foe, or anyone whose auto attack is ranged or reaches any row.</summary>
    private static bool Skirmisher(CombatEngine engine, Combatant who)
    {
        if (who.Enemy is not null) return who.Enemy.Ranged;
        foreach (AbilityDefinition ability in engine.Abilities(who))
        {
            if (ability.Bar != BarSlot.Auto || ability.Kind == AbilityKind.Passive) continue;
            bool ranged = ability.Reach switch
            {
                Reach.Ranged or Reach.AnyRow => true,
                Reach.Weapon => who.MainHand.Ranged,
                _ => false,
            };
            if (ranged) return true;
        }
        return false;
    }
}
