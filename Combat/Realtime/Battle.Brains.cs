using System;
using System.Collections.Generic;
using System.Numerics;
using LegendQuest.Characters;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// The brains (11-combat.md §5.1 and §5.3): the party AI for everyone on the party's side but the
/// leader, and the foes' AI, each run only when a fighter is free to think, about every 0.3 s.
/// </summary>
/// <remarks>
/// <para>
/// The party AI is the old EnemyMind's utility scoring in real time: it scores every skill it could
/// use on every target worth thinking about, by expected damage, healing and control, weighed by what
/// the target means to the fight, less the price in Focus, and does the best one; failing that it
/// keeps its auto attack on its target. Healers heal first, defenders taunt foes hitting someone else
/// and stand in front, everyone assists the leader's target unless something's more urgent, ranged
/// fighters step away from foes in melee with them, and anyone free helps a Downed ally up when
/// nobody's healing them. Everyone steps out of a telegraph's ground marker.
/// </para>
/// <para>
/// With BattleRules.HumanHandicap (the simulator) the party side plays as a person would: each
/// decision waits 0.4 s before it's acted on, and 15% of the time it takes its second-best choice,
/// drawn from the fight's own dice so it replays from the seed.
/// </para>
/// <para>
/// Foes keep a threat table (Battle.Life) and attack the top of it, switching only when someone
/// passes 110% of their target's threat (130% for a ranged foe); a taunt overrides it. They use their
/// moves when they're ready and worth it, walk into reach, keep their range, and a foe pulled more
/// than 300 px from home that nobody's hit for 6 s walks back and heals.
/// </para>
/// </remarks>
public sealed partial class Battle
{
    /// <summary>How often a free fighter's AI thinks.</summary>
    public const int ThinkMs = 300;

    /// <summary>The human handicap's reaction time.</summary>
    public const int ReactionMs = 400;

    /// <summary>The human handicap's share of best choices, in percent.</summary>
    public const int BestChoicePct = 85;

    /// <summary>How far a foe may be pulled from home before it thinks about resetting.</summary>
    public const float LeashPx = 300f;

    /// <summary>How long nobody may have hit a foe that's too far from home before it resets.</summary>
    public const int ResetQuietMs = 6000;

    /// <summary>The least a choice has to be worth, in points of damage, to be worth a button.</summary>
    private const double WorthPressing = 2.0;

    /// <summary>What a point of Focus is worth to the AI, in points of damage.</summary>
    private const double FocusPrice = 0.08;

    /// <summary>What a percent of a foe's loot taken as gold is worth to the AI, in points of damage.</summary>
    private const double GoldWorth = 0.2;

    /// <summary>The chance the AI's attack lands, for its sums.</summary>
    private const double HitGuess = 0.65;

    /// <summary>Runs the AI of everyone free to think, in joining order.</summary>
    private void Think()
    {
        for (int i = 0; i < _fighters.Count; i++)
        {
            Fighter f = _fighters[i];
            if (!f.IsStanding || f == Leader) continue;
            if (f.Pending is BattleCommand pending)
            {
                if (NowMs < f.PendingAtMs) continue;
                f.Pending = null;
                Apply(pending);
                continue;
            }
            if (NowMs < f.NextThinkMs) continue;
            f.NextThinkMs = NowMs + ThinkMs;
            if (!CanAct(f) || f.Helping is not null) continue;
            if (f.Side == Side.Foes) ThinkFoe(f);
            else ThinkAlly(f);
            if (Outcome != FightOutcome.Ongoing) return;
        }
    }

    /// <summary>Acts on a decision: at once for a foe, or after the reaction time for the party with the human handicap.</summary>
    private void Decide(Fighter f, BattleCommand command)
    {
        if (Rules.HumanHandicap && f.Side == Side.Party)
        {
            f.Pending = command;
            f.PendingAtMs = NowMs + ReactionMs;
            return;
        }
        Apply(command);
    }

    // The party.

    /// <summary>One think of a fighter on the party's side.</summary>
    private void ThinkAlly(Fighter f)
    {
        if (Danger(f) is BattleArea danger)
        {
            if (f.Goal is not Vector2 goal || danger.Contains(goal, f.Size)) Decide(f, BattleCommand.MoveTo(f.Id, Arena.Clamp(danger.WayOut(f.Position, 14f + f.Size))));
            return;
        }
        if (f.Action is not null || f.Order is not null) return;
        if ((f.Kind != FighterKind.Member || Problem(f, Potion, f) is not null) && Retreat(f)) return;

        Fighter target = PickTarget(f);
        var options = new List<(double Score, BattleCommand Command)>();

        if (f.Kind == FighterKind.Member && f.HpShare < 0.4 && Problem(f, Potion, f) is null)
            options.Add((20 + (1 - f.HpShare) * 40, BattleCommand.Potion(f.Id)));

        if (SoulLinkProblem(f, target) is null) options.Add((25, BattleCommand.SoulLink(f.Id, target.Id)));

        int saving = Saving(f, target);
        foreach (SkillDefinition skill in f.BarSkills)
        {
            if (skill.Kind == SkillKind.Auto) continue;
            if (saving > 0 && Spends(f, skill, saving)) continue;
            foreach (Fighter candidate in Candidates(f, skill, target))
            {
                if (Problem(f, skill, candidate) is not null) continue;
                double score = Value(f, skill, candidate);
                if (score >= WorthPressing) options.Add((score, BattleCommand.Use(f.Id, skill.Id, candidate?.Id)));
            }
        }

        if (HelpCandidate(f) is Fighter downed)
        {
            double score = f.Defender ? 10 : 18;
            score -= Vector2.Distance(f.Position, downed.Position) * 0.03;
            options.Add((score, BattleCommand.Help(f.Id, downed.Id)));
        }

        if (options.Count > 0)
        {
            options.Sort((a, b) => b.Score.CompareTo(a.Score));
            var choice = options[0];
            if (Rules.HumanHandicap && options.Count > 1 && !_rng.Percent(BestChoicePct)) choice = options[1];
            Decide(f, choice.Command);
            return;
        }

        if (target is null) return;
        if (f.Target != target || !f.Chase || !f.AutoAttackOn)
        {
            Decide(f, BattleCommand.Target(f.Id, target.Id));
            return;
        }
        if (f.Ranged && NowMs >= f.KiteReadyMs)
        {
            foreach (Fighter foe in _fighters)
            {
                if (!Hostile(f, foe) || !foe.IsStanding || foe.Target != f || foe.Ranged) continue;
                if (Vector2.Distance(foe.Position, f.Position) > Ranges.Melee + foe.Size + 10f) continue;
                Vector2 away = f.Position - foe.Position;
                away = away.LengthSquared() < 0.01f ? new Vector2(-1f, 0f) : Vector2.Normalize(away);
                f.KiteReadyMs = NowMs + 4000;
                Decide(f, BattleCommand.MoveTo(f.Id, Arena.Clamp(f.Position + away * 80f)));
                return;
            }
        }
    }

    /// <summary>Backs a hurt fighter who isn't a defender out of a melee foe's reach, behind the sturdiest ally between them and the foes (or just away), now and then; says whether it did.</summary>
    private bool Retreat(Fighter f)
    {
        if (f.Defender || f.HpShare >= 0.45 || NowMs < f.KiteReadyMs) return false;
        Fighter attacker = null;
        foreach (Fighter foe in _fighters)
        {
            if (!Hostile(f, foe) || !foe.IsStanding || foe.Target != f || foe.Ranged) continue;
            if (Vector2.Distance(foe.Position, f.Position) <= Ranges.Melee + foe.Size + f.Size + 12f) attacker = foe;
        }
        if (attacker is null) return false;

        Vector2 away = f.Position - attacker.Position;
        away = away.LengthSquared() < 0.01f ? new Vector2(f.Side == Side.Party ? -1f : 1f, 0f) : Vector2.Normalize(away);
        Fighter cover = null;
        float coverDistance = 0f;
        foreach (Fighter ally in _fighters)
        {
            if (ally == f || ally.Side != f.Side || !ally.IsStanding || !(ally.Defender || ally.HpShare > 0.6)) continue;
            float distance = Vector2.Distance(ally.Position, attacker.Position);
            if (distance > coverDistance && distance < 160f)
            {
                coverDistance = distance;
                cover = ally;
            }
        }
        Vector2 to = cover is not null ? cover.Position + away * 30f : f.Position + away * 80f;
        f.KiteReadyMs = NowMs + 5000;
        Decide(f, BattleCommand.MoveTo(f.Id, Arena.Clamp(to)));
        return true;
    }

    /// <summary>
    /// The Focus a fighter on the party's side is saving for their ultimate, as a player would: its
    /// cost while it's off its cooldown, more than they have, and worth pressing now if they had it
    /// (enough foes for an area, someone Downed for Nobody Left Behind...); 0 when they're saving for
    /// nothing.
    /// </summary>
    private int Saving(Fighter f, Fighter target)
    {
        foreach (SkillDefinition skill in f.BarSkills)
        {
            if (!skill.Ultimate || CooldownLeft(f, skill) > 0) continue;
            int cost = FocusCost(f, skill);
            if (cost <= f.Focus) return 0;
            double best = 0;
            foreach (Fighter candidate in Candidates(f, skill, target)) best = Math.Max(best, Value(f, skill, candidate));
            return best >= WorthPressing ? cost : 0;
        }
        return 0;
    }

    /// <summary>Whether using a skill would spend Focus saved for an ultimate: a Focus skill other than a heal, a defence or a taunt, that would leave less than the saving.</summary>
    private bool Spends(Fighter f, SkillDefinition skill, int saving)
    {
        if (skill.Ultimate || skill.Ai is AiUse.Heal or AiUse.Defensive or AiUse.Taunt) return false;
        int cost = FocusCost(f, skill);
        return cost > 0 && f.Focus - cost < saving;
    }

    /// <summary>The ground marker of a foe's telegraph or cast that someone's standing in, or null.</summary>
    private BattleArea Danger(Fighter f)
    {
        foreach (Fighter foe in _fighters)
        {
            if (!Hostile(f, foe) || foe.Action is not ActionState action || action.Area is not BattleArea area || area.Shape == AreaKind.Room) continue;
            if (action.TelegraphUntilMs <= 0 && action.CastUntilMs <= 0) continue;
            if (area.Contains(f.Position, f.Size + 4f)) return area;
        }
        return null;
    }

    /// <summary>The foe someone on the party's side should be fighting: a defender takes a foe hitting someone else; everyone else assists the leader's target, keeps their own, or takes whoever's hitting them or nearest.</summary>
    private Fighter PickTarget(Fighter f)
    {
        bool Good(Fighter t) => t is not null && Hostile(f, t) && t.IsStanding && !t.Vanished && !Has(t, ConditionId.Frozen) && !t.Resetting;

        if (f.Defender)
        {
            Fighter loose = null;
            float best = float.MaxValue;
            foreach (Fighter foe in _fighters)
            {
                if (!Good(foe) || foe.Target is not Fighter victim || victim == f || victim.Defender) continue;
                float distance = Vector2.Distance(f.Position, foe.Position);
                if (distance < best)
                {
                    best = distance;
                    loose = foe;
                }
            }
            if (loose is not null) return loose;
        }

        Fighter anchor = Leader ?? Find(Origins.You);
        if (anchor is not null && anchor != f && anchor.IsStanding && Good(anchor.Target)) return anchor.Target;
        if (Good(f.Target)) return f.Target;
        Fighter attacker = NearestFoe(f, attackingMe: true);
        if (Good(attacker)) return attacker;
        Fighter nearest = NearestFoe(f, attackingMe: false);
        return Good(nearest) ? nearest : null;
    }

    /// <summary>Who a skill's worth thinking about using on: its target if it's one at a foe (and any foe casting), each ally for a heal or a buff, or the user.</summary>
    private IEnumerable<Fighter> Candidates(Fighter f, SkillDefinition skill, Fighter target)
    {
        switch (skill.Target)
        {
            case SkillTarget.Foe:
                if (target is not null) yield return target;
                foreach (Fighter foe in _fighters)
                    if (foe != target && Hostile(f, foe) && foe.IsStanding && foe.Casting is not null && !foe.Vanished) yield return foe;
                break;
            case SkillTarget.Ally:
                foreach (Fighter ally in _fighters)
                    if (ally.Side == f.Side && ally.Present && !ally.IsDead && !ally.HasLeft) yield return ally;
                break;
            case SkillTarget.Self:
                yield return f;
                break;
            default:
                yield return target;
                break;
        }
    }

    /// <summary>A Downed ally nobody's helping, healing or already on their way to, or null.</summary>
    private Fighter HelpCandidate(Fighter f)
    {
        Fighter best = null;
        float nearest = float.MaxValue;
        foreach (Fighter downed in _fighters)
        {
            if (downed.Side != f.Side || !downed.IsDown) continue;
            bool covered = false;
            foreach (Fighter other in _fighters)
                if (other != f && (other.Helping == downed || other.Order is { Kind: CommandKind.Help } order && order.Target == downed || other.Pending is { Kind: CommandKind.Help } pending && pending.TargetId == downed.Id))
                    covered = true;
            if (covered) continue;
            float distance = Vector2.Distance(f.Position, downed.Position);
            if (distance < nearest)
            {
                nearest = distance;
                best = downed;
            }
        }
        return best;
    }

    /// <summary>What using a skill on someone is worth to the AI now, in points of damage: what it does, by its effects, less its Focus.</summary>
    private double Value(Fighter f, SkillDefinition skill, Fighter t)
    {
        var caught = Caught(f, skill, t);
        int foes = 0, allies = 0;
        foreach (Fighter c in caught)
            if (Hostile(f, c)) foes++;
            else allies++;
        if (IsArea(skill.Area) && !Friendly(skill) && foes < skill.AiMinTargets) return 0;

        double value = 0;
        foreach (Effect e in skill.Effects)
        {
            switch (e.Kind)
            {
                case EffectType.Damage:
                {
                    int count = e.On switch { EffectOn.AllFoes => CountSide(f, false), EffectOn.Attacker => 0, _ => foes };
                    double each = Average(f, e) * (e.Delivery == Delivery.Sure ? 1.0 : HitGuess) * Math.Max(1, skill.StrikesAt(f.Level));
                    if (skill.Area == AreaShape.Spread) each /= Math.Max(1, skill.Strikes);
                    value += each * count;
                    if (t is not null && Hostile(f, t) && t.Casting is { Interruptible: true }) value += 2;
                    break;
                }
                case EffectType.Heal:
                {
                    var on = e.On == EffectOn.AllAllies ? SideOf(f, EffectOn.AllAllies, heal: true) : e.On == EffectOn.Self ? new List<Fighter> { f } : caught;
                    foreach (Fighter ally in on)
                    {
                        if (Hostile(f, ally) || e.DownedOnly && !ally.IsDown || e.StandingOnly && ally.IsDown) continue;
                        double amount = e.PoolPerLevel > 0 ? PoolLeft(f, skill, e) : HealAverage(f, e, ally);
                        double coming = IncomingHeal(ally, f);
                        double missing = ally.IsDown ? (coming > 0 ? 0 : ally.MaxHp) : Math.Max(0, ally.MaxHp - ally.Hp - coming);
                        if (ally.IsDown && coming > 0) continue;
                        if (ally.IsDown) value += 40;
                        else if (missing < amount * 0.6 && ally.HpShare > 0.5) continue;
                        value += Math.Min(amount, missing) * (ally.HpShare < 0.35 ? 2.0 : 1.2) * (f.Healer ? 1.5 : 1.0);
                    }
                    break;
                }
                case EffectType.TempHp:
                {
                    var on = e.On == EffectOn.Self ? new List<Fighter> { f } : caught;
                    foreach (Fighter ally in on)
                    {
                        if (Hostile(f, ally) || e.Aura is not null && HasAura(ally, e.Aura.Id)) continue;
                        double amount = HealAverage(f, e, ally);
                        value += amount * (Attacked(ally) ? 0.9 : 0.4);
                    }
                    break;
                }
                case EffectType.Aura:
                {
                    var on = e.On == EffectOn.Self ? new List<Fighter> { f } : e.On == EffectOn.AllAllies ? SideOf(f, EffectOn.AllAllies, false)
                        : e.On == EffectOn.AllFoes ? SideOf(f, EffectOn.AllFoes, false) : caught;
                    int fresh = 0;
                    foreach (Fighter who in on)
                        if (!HasAura(who, e.Aura.Id, e.Aura.Beneficial ? null : f)) fresh++;
                    if (fresh == 0) return 0;
                    double worth = skill.AiValue > 0 ? skill.AiValue : e.Aura.TickDice.IsNone ? 2 : e.Aura.TickDice.Average * Math.Max(1, e.Aura.DurationMs / Math.Max(1, e.Aura.TickMs));
                    if (skill.Ai == AiUse.Defensive && !Attacked(skill.Target == SkillTarget.Ally && t is not null ? t : f)) worth *= 0.3;
                    value += worth * fresh;
                    break;
                }
                case EffectType.Taunt:
                {
                    int loose = 0;
                    foreach (Fighter foe in e.On == EffectOn.AllFoes ? SideOf(f, EffectOn.AllFoes, false) : caught)
                        if (Hostile(f, foe) && foe.Target != f) loose++;
                    if (loose == 0) return 0;
                    value += Math.Max(1, skill.AiValue) * loose * (f.Defender ? 1.0 : 0.3);
                    break;
                }
                case EffectType.Transform:
                    if (f.Form is not null || NearestFoe(f, false) is null) return 0;
                    value += skill.AiValue;
                    break;
                case EffectType.Summon:
                    value += skill.AiValue;
                    break;
                case EffectType.Rider when skill.Area == AreaShape.AllAllies:
                {
                    // Given in Love: worth its AiValue for each ally standing with a foe in reach and no rider waiting.
                    foreach (Fighter ally in SideOf(f, EffectOn.AllAllies, heal: false))
                        if (ally.Riders.Count == 0 && ally.Target is { } its && Hostile(ally, its) && its.IsStanding) value += skill.AiValue;
                    break;
                }
                case EffectType.Rider:
                    if (f.Riders.Count > 0 || f.Target is not { } aim || !Hostile(f, aim) || !InReach(f, f.Auto, aim)) return 0;
                    value += skill.AiValue;
                    break;
                case EffectType.Reset:
                {
                    Fighter who = skill.Target == SkillTarget.Ally ? t : f;
                    if (who is null || skill.Target == SkillTarget.Ally && (who == f || who.Kind != FighterKind.Member || !who.IsStanding)) return 0;
                    if (who.GcdUntilMs <= NowMs + 300) return 0;
                    value += skill.AiValue;
                    break;
                }
                case EffectType.Arm:
                {
                    Fighter guarded = skill.Trigger == ReactionTrigger.AttackOnAlly ? t : f;
                    if (skill.Trigger == ReactionTrigger.AttackOnAlly && (t is null || t == f || t.Defender)) return 0;
                    int threats = 0;
                    foreach (Fighter foe in _fighters)
                    {
                        if (!Hostile(f, foe) || !foe.IsStanding) continue;
                        bool counts = skill.Trigger switch
                        {
                            ReactionTrigger.FoeCast => foe.Casting is { Interruptible: true },
                            ReactionTrigger.RangedOnAlly => (foe.Ranged || foe.Casting is not null) && foe.Target is { } victim && !Hostile(f, victim),
                            _ => foe.Target == guarded,
                        };
                        if (counts) threats++;
                    }
                    if (threats == 0) return 0;
                    value += skill.AiValue * Math.Min(2, threats) * (skill.Trigger == ReactionTrigger.MeleeMissOnYou ? 1.0 : 1.2);
                    break;
                }
                case EffectType.Cleanse:
                {
                    int bad = 0;
                    foreach (Fighter who in caught)
                        foreach (Aura aura in who.AuraList)
                            if (!aura.Gone && !aura.Beneficial && aura.Definition.Condition is ConditionId c && (e.Cleanses.Count == 0 || Contains(e.Cleanses, c))) bad++;
                    value += bad * 4;
                    break;
                }
                case EffectType.Focus:
                    value += e.Amount * 0.15;
                    break;
                case EffectType.Throw:
                    value += Average(f, e) * 2 * HitGuess * Math.Max(1, foes);
                    break;
                case EffectType.Rewind:
                    if (t is null || !t.IsStanding || Hostile(f, t)) return 0;
                    value += Math.Max(0, Math.Min(t.MaxHp, HpAgo(t, e.DurationMs > 0 ? e.DurationMs : TurnMs)) - t.Hp) * (t.HpShare < 0.35 ? 2.0 : 1.2);
                    break;
                case EffectType.TakeOut:
                    if (t is not null && Encounter.Nonlethal && t.HpShare <= (e.MaxHpShare > 0 ? e.MaxHpShare : 0.25)) value += 30;
                    break;
                case EffectType.Strike when e.ByAllies:
                    if (t is null) break;
                    foreach (Fighter ally in SideOf(f, EffectOn.OtherAllies, heal: false))
                        if (CanAct(ally) && InReach(ally, ally.Auto, t)) value += (ally.MainAttack.Damage.Average + 2) * HitGuess;
                    break;
                case EffectType.OneOf:
                    value += skill.AiValue;
                    break;
                case EffectType.Gold:
                {
                    // Gold is worth a little to the party: a fifth of a foe's loot about 4 points, once a foe.
                    var on = e.On == EffectOn.AllFoes ? SideOf(f, EffectOn.AllFoes, false) : t is null ? new List<Fighter>() : new List<Fighter> { t };
                    foreach (Fighter foe in on)
                        if (Hostile(f, foe) && !_stolen.ContainsKey(foe.Id)) value += e.Amount * GoldWorth;
                    break;
                }
                case EffectType.Dash:
                    if (skill.Ai == AiUse.Defensive && !Attacked(f)) return 0;
                    break;
            }
        }

        if (skill.Ai is AiUse.Utility or AiUse.Buff or AiUse.Control && value == 0) value += skill.AiValue;
        if (skill.Ultimate && value < 10) value *= 0.5;
        value -= FocusCost(f, skill) * FocusPrice;
        if (skill.HpCost > 0) value -= skill.HpCost * (f.HpShare < 0.5 ? 1.5 : 0.5);
        if (t is not null && t != f && !InReach(f, skill, t)) value -= Vector2.Distance(f.Position, t.Position) * 0.01;
        return value;
    }

    /// <summary>The healing already on its way to someone: allies' heal skills decided on, ordered or in motion at them, at their averages.</summary>
    private double IncomingHeal(Fighter on, Fighter not)
    {
        double total = 0;
        foreach (Fighter ally in _fighters)
        {
            if (ally == not || ally.Side != on.Side || !ally.IsStanding) continue;
            SkillDefinition skill = null;
            if (ally.Pending is { Kind: CommandKind.Use } pending && pending.TargetId == on.Id) skill = SkillOf(ally, pending.SkillId);
            else if (ally.Order is { Kind: CommandKind.Use } order && order.Target == on) skill = order.Skill;
            else if (ally.Action is { Auto: false } action && action.Target == on) skill = action.Skill;
            if (skill is null) continue;
            foreach (Effect effect in skill.Effects)
                if (effect.Kind == EffectType.Heal && effect.On == EffectOn.Targets) total += effect.PoolPerLevel > 0 ? on.MaxHp : HealAverage(ally, effect, on);
        }
        return total;
    }

    /// <summary>A skill on someone's bar by id, or null.</summary>
    private static SkillDefinition SkillOf(Fighter f, string id)
    {
        foreach (SkillDefinition skill in f.BarSkills)
            if (skill.Id == id) return skill;
        return null;
    }

    /// <summary>Who a skill would catch if it were used on someone now, for the AI's sums.</summary>
    private List<Fighter> Caught(Fighter f, SkillDefinition skill, Fighter t)
    {
        var list = new List<Fighter>();
        switch (skill.Area)
        {
            case AreaShape.Single:
                if (skill.Target == SkillTarget.Self) list.Add(f);
                else if (t is not null) list.Add(t);
                break;
            case AreaShape.Spread:
            case AreaShape.RandomFoes:
            case AreaShape.Chain:
                if (t is not null) list.Add(t);
                else if (NearestFoe(f, false) is Fighter near) list.Add(near);
                if (skill.Area == AreaShape.Chain)
                    foreach (Fighter other in NearestFirst(t?.Position ?? f.Position, f.Side == Side.Party ? Side.Foes : Side.Party))
                        if (list.Count <= skill.Count && !list.Contains(other) && Vector2.Distance(other.Position, (t ?? f).Position) <= ChainPx * 2) list.Add(other);
                break;
            default:
            {
                Side side = Friendly(skill) ? f.Side : f.Side == Side.Party ? Side.Foes : Side.Party;
                if (skill.Area == AreaShape.Allies)
                {
                    if (t is not null) list.Add(t);
                    foreach (Fighter other in NearestFirst(t?.Position ?? f.Position, f.Side))
                        if (list.Count < skill.Count && !list.Contains(other)) list.Add(other);
                    break;
                }
                BattleArea area = AreaFor(f, skill, t);
                foreach (Fighter other in _fighters)
                    if (Catchable(other, side) && (area is null || area.Contains(other.Position, other.Size))) list.Add(other);
                break;
            }
        }
        return list;
    }

    /// <summary>How many of a side are standing: the user's allies, or their foes.</summary>
    private int CountSide(Fighter f, bool allies)
    {
        int count = 0;
        foreach (Fighter other in _fighters)
            if (other.IsStanding && (other.Side == f.Side) == allies) count++;
        return count;
    }

    /// <summary>A damage effect's average, for the AI.</summary>
    private static double Average(Fighter f, Effect e)
    {
        double total = 0;
        if (e.Weapon) total += (f.MainAttack.Damage.Average + (f.MainAttack.Extra.IsNone ? 0 : f.MainAttack.Extra.Average)) * Math.Max(1, e.WeaponTimes);
        if (!e.Dice.IsNone) total += (e.Plus ? Scaled(e.Dice, f.Level) : e.Dice).Average;
        if (e.AddAbility is Ability a) total += f.Scores.Modifier(a);
        if (e.AddCasting) total += f.Scores.Modifier(f.CastingAbility);
        total += e.LevelTimes * f.Level + e.Flat;
        return total;
    }

    /// <summary>A heal's (or temporary hit points') average, for the AI.</summary>
    private double HealAverage(Fighter f, Effect e, Fighter on)
    {
        if (e.Id == "potion") return BestPotion()?.Heals.Average ?? 0;
        double total = Average(f, e);
        if (e.MaxHpShare > 0) total += on.MaxHp * e.MaxHpShare;
        return total + Sum(f, Stat.HealingFlat, default);
    }

    /// <summary>Whether an aura of an id is on someone (from a source, if one's given).</summary>
    private static bool HasAura(Fighter who, string id, Fighter from = null)
    {
        foreach (Aura aura in who.AuraList)
            if (!aura.Gone && aura.Id == id && (from is null || aura.Source == from)) return true;
        return false;
    }

    /// <summary>Whether a foe is attacking someone.</summary>
    private bool Attacked(Fighter who)
    {
        foreach (Fighter foe in _fighters)
            if (Hostile(who, foe) && foe.IsStanding && foe.Target == who) return true;
        return false;
    }

    // The foes.

    /// <summary>One think of a foe (or anyone on the foes' side).</summary>
    private void ThinkFoe(Fighter f)
    {
        if (f.Resetting)
        {
            if (Vector2.Distance(f.Position, f.Home) <= 4f)
            {
                f.Resetting = false;
                f.Hp = f.MaxHp;
                f.ThreatTable.Clear();
                Emit(new BattleEvent { Kind = BattleEventKind.Healed, ActorId = f.Id, TargetId = f.Id, Amount = f.MaxHp, Position = f.Position, Text = $"{f.Name} backs off and recovers." });
            }
            else if (f.Goal is null) SetGoal(f, f.Home);
            return;
        }
        if (f.Kind == FighterKind.Foe && Vector2.Distance(f.Position, f.Home) > LeashPx && NowMs - f.LastHurtMs > ResetQuietMs)
        {
            f.Resetting = true;
            f.Target = null;
            f.AutoAttackOn = false;
            f.Chase = false;
            if (f.Action is { InMotion: false }) Interrupt(f, "reset");
            SetGoal(f, f.Home);
            return;
        }
        if (f.Action is not null) return;

        Fighter target = FoeTarget(f);
        if (target is null)
        {
            f.AutoAttackOn = false;
            return;
        }
        if (target != f.Target)
        {
            bool had = f.Target is not null;
            SetTarget(f, target);
            if (had) Emit(new BattleEvent { Kind = BattleEventKind.ThreatTargetChanged, ActorId = f.Id, TargetId = target.Id });
        }
        else if (!f.Chase || !f.AutoAttackOn) SetTarget(f, target);

        foreach (string id in f.FoeSkills)
        {
            SkillDefinition skill = SkillCatalog.Get(id);
            if (CooldownLeft(f, skill) > 0 || skill.OnGcd && f.GcdUntilMs > NowMs) continue;
            Fighter on = FoeSkillTarget(f, skill, target);
            if (on is null && skill.Target is SkillTarget.Foe or SkillTarget.Ally) continue;
            if (!FoeSkillWorth(f, skill, on)) continue;
            if (on is not null && !InReach(f, skill, on)) continue;
            StartAction(f, skill, on, auto: false);
            return;
        }

        if (f.Ranged && NowMs >= f.KiteReadyMs)
        {
            foreach (Fighter enemy in _fighters)
            {
                if (!Hostile(f, enemy) || !enemy.IsStanding || enemy.Ranged) continue;
                if (Vector2.Distance(enemy.Position, f.Position) > Ranges.Melee + enemy.Size + f.Size + 6f) continue;
                Vector2 away = f.Position - enemy.Position;
                away = away.LengthSquared() < 0.01f ? new Vector2(1f, 0f) : Vector2.Normalize(away);
                f.KiteReadyMs = NowMs + 3000;
                f.Chase = false;
                SetGoal(f, Arena.Clamp(f.Position + away * 50f));
                At(NowMs + 1200, () => { if (f.IsStanding && f.Target is not null) f.Chase = true; });
                return;
            }
        }
    }

    /// <summary>Who a foe means to hit: its taunter while it's taunted; otherwise the top of its threat table it can get at, switching from its current target only when someone passes 110% of that target's threat (130% for a ranged foe); with nobody on its table, whoever's nearest. Whoever it may not pick (Hold, Sanctuary, They Know Me) it passes over.</summary>
    private Fighter FoeTarget(Fighter f)
    {
        bool Good(Fighter t) => t is not null && Hostile(f, t) && t.IsStanding && !t.Vanished && MayPick(f, t);

        if (First(f, ConditionId.Taunted) is Aura taunt && taunt.Source is { } taunter && Hostile(f, taunter) && taunter.IsStanding && !taunter.Vanished) return taunt.Source;

        Fighter top = null;
        double topValue = 0, currentValue = 0;
        foreach (ThreatEntry entry in f.ThreatTable)
        {
            if (!Good(entry.Who)) continue;
            if (top is null)
            {
                top = entry.Who;
                topValue = entry.Value;
            }
            if (entry.Who == f.Target) currentValue = entry.Value;
        }
        Fighter current = Good(f.Target) ? f.Target : null;
        if (current is not null && top is not null && current != top)
            return topValue > currentValue * (f.Ranged ? 1.3 : 1.1) ? top : current;
        if (top is not null) return top;
        if (current is not null) return current;
        Fighter nearest = null;
        float best = float.MaxValue;
        foreach (Fighter other in _fighters)
        {
            if (!Good(other)) continue;
            float distance = Vector2.Distance(f.Position, other.Position);
            if (distance < best)
            {
                best = distance;
                nearest = other;
            }
        }
        return nearest;
    }

    /// <summary>Whether a foe may pick someone as its target: not someone held out of reach (Hold), not someone under Sanctuary it has failed its Wisdom save against (one save an aura), and, for a beast, not a beekeeper who hasn't hurt it (They Know Me).</summary>
    private bool MayPick(Fighter foe, Fighter t)
    {
        if (foe.Side != Side.Foes) return true;
        if (Any(t, Stat.Untargetable, new ModContext { Other = foe })) return false;
        if (foe.Enemy is { Beast: true } && Any(t, Stat.BeastsIgnore, new ModContext { Other = foe }) && !foe.HurtBy.Contains(t)) return false;
        foreach (Aura aura in t.AuraList)
        {
            if (aura.Gone || !HasMod(aura.Definition, Stat.Sanctuary) || aura.Source is not Fighter keeper) continue;
            if (!foe.SanctuarySaves.TryGetValue(aura, out bool made))
            {
                made = Save(foe, Ability.Wisdom, Dc(keeper, aura.Skill), null, keeper);
                foe.SanctuarySaves[aura] = made;
                if (!made) Emit(new BattleEvent { Kind = BattleEventKind.Bark, ActorId = keeper.Id, TargetId = foe.Id, SkillId = aura.Skill?.Id, Text = $"{foe.Name} can't bring itself to strike {t.Name} ({aura.Name})." });
            }
            if (!made) return false;
        }
        return true;
    }

    /// <summary>Who a foe's move goes at: its target, or for a pull, whoever of its foes is furthest out of melee within the move's reach (its target if nobody is).</summary>
    private Fighter FoeSkillTarget(Fighter f, SkillDefinition skill, Fighter target)
    {
        switch (skill.Target)
        {
            case SkillTarget.Self:
                return f;
            case SkillTarget.None:
                return target;
            case SkillTarget.Ally:
                return MostHurtAlly(f) ?? f;
        }
        if (!Pulls(skill)) return target;
        Fighter best = null;
        float furthest = Ranges.Melee + 10f;
        foreach (Fighter other in _fighters)
        {
            if (!Hostile(f, other) || !other.IsStanding || other.Vanished || !InReach(f, skill, other)) continue;
            float distance = Vector2.Distance(f.Position, other.Position);
            if (distance > furthest)
            {
                furthest = distance;
                best = other;
            }
        }
        return best ?? target;
    }

    /// <summary>Whether a move pulls its target in.</summary>
    private static bool Pulls(SkillDefinition skill)
    {
        foreach (Effect effect in skill.Effects)
            if (effect.Kind == EffectType.Pull) return true;
        return false;
    }

    /// <summary>Whether a foe's ready move is worth using now: an area move needs someone in it, a curse a target without it, a vanish or a disengage someone in melee, a feint its target in reach.</summary>
    private bool FoeSkillWorth(Fighter f, SkillDefinition skill, Fighter on)
    {
        if (IsArea(skill.Area))
        {
            BattleArea area = AreaFor(f, skill, on);
            int caught = 0;
            foreach (Fighter other in _fighters)
                if (Hostile(f, other) && other.IsStanding && (area is null || area.Contains(other.Position, other.Size))) caught++;
            return caught >= Math.Max(1, skill.AiMinTargets);
        }
        foreach (Effect effect in skill.Effects)
        {
            switch (effect.Kind)
            {
                case EffectType.Aura when effect.On == EffectOn.Targets && on is not null && HasAura(on, effect.Aura.Id, f):
                    return false;
                case EffectType.Aura when effect.On == EffectOn.Self && HasAura(f, effect.Aura.Id):
                    return false;
                case EffectType.Vanish:
                case EffectType.Dash when effect.Distance > 0:
                {
                    int close = 0;
                    foreach (Fighter other in _fighters)
                        if (Hostile(f, other) && other.IsStanding && Vector2.Distance(other.Position, f.Position) <= Ranges.Melee + f.Size + other.Size + 6f) close++;
                    if (close < (effect.Kind == EffectType.Dash ? 2 : 1)) return false;
                    break;
                }
            }
        }
        if (skill.Range == SkillRange.Melee && on is not null && on != f && !InReach(f, skill, on)) return false;
        return true;
    }
}
