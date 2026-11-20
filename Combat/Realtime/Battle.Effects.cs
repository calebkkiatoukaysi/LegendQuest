using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// What a blow does: the floor it covers (11-combat.md §4.3's shapes), who it lands on as it falls,
/// and each of its skill's effects in turn on each of them (damage, healing, auras, movement, and the
/// rest of EffectType), with riders, procs, Sneak Attack and the Soul-Link's charge.
/// </summary>
public sealed partial class Battle
{
    /// <summary>"Foe + neighbours": every foe within this of the target.</summary>
    public const float NeighbourPx = 36f;

    /// <summary>"1 row": every foe within this of the target.</summary>
    public const float RowPx = 48f;

    /// <summary>"Front row of foes": every foe within this in front of the user.</summary>
    public const float FrontPx = 60f;

    /// <summary>"A column": a line this wide from the user through the target.</summary>
    public const float LineWidthPx = 24f;

    /// <summary>"Your row": allies within this of the user.</summary>
    public const float NearPx = 60f;

    /// <summary>A ring's radius when its skill doesn't give one.</summary>
    public const float DefaultRingPx = 48f;

    /// <summary>How far a chain jumps from one foe to the next.</summary>
    public const float ChainPx = 100f;

    /// <summary>Sneak Attack's window: an ally's hit on the target this recently counts, and it comes round this often.</summary>
    public const int SneakWindowMs = 3000;

    /// <summary>Sneak Attack's d6s before levels 5 and 9 add theirs: two, as 5e has them by third level, so a rogue's cut is the hardest single blow in the early party.</summary>
    public const int SneakDice = 2;

    /// <summary>How long a rider waits for its hit when its skill doesn't say.</summary>
    private const int RiderMs = 6000;

    /// <summary>The blow a Riposte or a Strike effect makes with a weapon when the user's auto isn't one.</summary>
    private static readonly SkillDefinition FreeStrike = new()
    {
        Id = "strike", Name = "Strike", Kind = SkillKind.Auto, Weapon = true,
        Effects = new[] { new Effect { Kind = EffectType.Damage, Weapon = true, Delivery = Delivery.Attack } },
    };

    private int _summoned;
    private readonly Dictionary<string, int> _stolen = new(StringComparer.Ordinal);

    /// <summary>Whether someone has called in a stat block that's still standing with them (one Watchman a fight at a time).</summary>
    internal bool Summoned(Fighter f, string enemyId)
    {
        foreach (Fighter other in _fighters)
            if (other.Summoner == f && other.EnemyId == enemyId && other.IsStanding) return true;
        return false;
    }

    // The floor a skill covers.

    /// <summary>The area a skill covers from where its user and target stand now: its ground marker, and where its blow will land if it's fixed by a telegraph; null for a skill aimed at one fighter.</summary>
    internal BattleArea AreaFor(Fighter f, SkillDefinition skill, Fighter target)
    {
        Vector2 at = target?.Position ?? f.Position;
        float size = target?.Size ?? 0f;
        bool aroundUser = skill.Target is SkillTarget.Self or SkillTarget.None || target is null;
        switch (skill.Area)
        {
            case AreaShape.Ring:
                return BattleArea.Ring(aroundUser ? f.Position : at, (skill.AreaPx > 0f ? skill.AreaPx : DefaultRingPx) + (aroundUser ? f.Size : size));
            case AreaShape.Neighbours:
                return BattleArea.Ring(at, NeighbourPx + size);
            case AreaShape.Row:
                return BattleArea.Ring(at, (skill.AreaPx > 0f ? skill.AreaPx : RowPx) + size);
            case AreaShape.Front:
                return BattleArea.Cone(f.Position, f.Position + Toward(f, target) * 10f, (skill.AreaPx > 0f ? skill.AreaPx : FrontPx) + f.Size, 180f);
            case AreaShape.Line:
            {
                float reach = skill.AreaPx > 0f ? skill.AreaPx : Math.Min(RangeOf(f, skill), Ranges.Long);
                return BattleArea.Line(f.Position, Arena.Clamp(f.Position + Toward(f, target) * reach), LineWidthPx + f.Size);
            }
            case AreaShape.AllFoes:
            case AreaShape.AllAllies:
                return BattleArea.Room;
            case AreaShape.AlliesNear:
                return BattleArea.Ring(f.Position, NearPx);
            default:
                return null;
        }
    }

    /// <summary>The way from someone to their target, or the way they face with none.</summary>
    private static Vector2 Toward(Fighter f, Fighter target)
    {
        if (target is not null && target != f)
        {
            Vector2 way = target.Position - f.Position;
            if (way.LengthSquared() > 0.01f) return Vector2.Normalize(way);
        }
        return new Vector2(f.Facing, 0f);
    }

    /// <summary>Whether a skill's area falls on the user's own side (a heal, a buff) rather than the foes'.</summary>
    private static bool Friendly(SkillDefinition skill) =>
        skill.Target == SkillTarget.Ally || skill.Area is AreaShape.AllAllies or AreaShape.AlliesNear or AreaShape.AlliesBehind or AreaShape.Allies
        || skill.Target == SkillTarget.Self && skill.Area == AreaShape.Single;

    /// <summary>Whether someone can be caught by a blow from a side: present, standing, not vanished, and on the other side (or the same, for a friendly one).</summary>
    private static bool Catchable(Fighter who, Side side) => who.Side == side && who.IsStanding && !who.Vanished;

    /// <summary>Who one blow of a move lands on, worked out the moment it lands: its target, the next of its darts, a random foe, or everyone in its area (fixed where a telegraph fixed it).</summary>
    internal List<Fighter> TargetsFor(Fighter f, ActionState action, int index)
    {
        SkillDefinition skill = action.Skill;
        var list = new List<Fighter>();
        Fighter target = action.Target;
        Side side = Friendly(skill) ? f.Side : f.Side == Side.Party ? Side.Foes : Side.Party;

        switch (skill.Area)
        {
            case AreaShape.Single:
                if (skill.Target == SkillTarget.Self) list.Add(f);
                else if (skill.Target == SkillTarget.Foe)
                {
                    if (target is null || !target.IsStanding || !Targetable(target, skill)) break;
                    if (skill.Projectile is null && !(skill.Weapon && AttackOf(f, index, action.Auto).Ranged) && !action.SoulLink
                        && Vector2.Distance(f.Position, target.Position) > RangeOf(f, skill) + f.Size + target.Size + Slack)
                    {
                        Emit(new BattleEvent { Kind = BattleEventKind.Missed, ActorId = f.Id, TargetId = target.Id, SkillId = skill.Id, Flags = HitFlags.Miss, Position = target.Position, Text = $"{target.Name} is out of reach." });
                        break;
                    }
                    list.Add(target);
                }
                else if (skill.Target == SkillTarget.Ally && target is not null && !target.IsDead && !target.HasLeft && target.Present
                         && (target.IsStanding || Revives(skill))) list.Add(target);
                break;

            case AreaShape.Spread:
            {
                var darts = new List<Fighter>();
                if (target is not null && target.IsStanding && Targetable(target, skill)) darts.Add(target);
                Vector2 from = target?.Position ?? f.Position;
                foreach (Fighter other in NearestFirst(from, side))
                {
                    if (darts.Count >= skill.Count) break;
                    if (!darts.Contains(other) && Targetable(other, skill)) darts.Add(other);
                }
                if (darts.Count > 0) list.Add(darts[index % darts.Count]);
                break;
            }

            case AreaShape.RandomFoes:
            {
                var engaged = new List<Fighter>();
                foreach (Fighter other in _fighters)
                    if (Catchable(other, side) && Targetable(other, skill)) engaged.Add(other);
                if (engaged.Count > 0) list.Add(engaged[(int)_rng.Below((uint)engaged.Count)]);
                break;
            }

            case AreaShape.Chain:
            {
                if (target is null || !target.IsStanding || !Targetable(target, skill)) break;
                list.Add(target);
                Fighter last = target;
                for (int link = 0; link < skill.Count; link++)
                {
                    Fighter next = null;
                    foreach (Fighter other in NearestFirst(last.Position, side))
                    {
                        if (list.Contains(other) || !Targetable(other, skill)) continue;
                        if (Vector2.Distance(other.Position, last.Position) <= ChainPx + other.Size + last.Size) next = other;
                        break;
                    }
                    if (next is null) break;
                    list.Add(next);
                    last = next;
                }
                break;
            }

            case AreaShape.Allies:
            {
                if (target is not null && target.Side == f.Side && (target.IsStanding || target.IsDown && Revives(skill))) list.Add(target);
                foreach (Fighter other in NearestFirst(target?.Position ?? f.Position, f.Side))
                {
                    if (list.Count >= skill.Count) break;
                    if (!list.Contains(other)) list.Add(other);
                }
                break;
            }

            case AreaShape.AlliesBehind:
            {
                float mine = DistanceToSide(f.Position, f.Side == Side.Party ? Side.Foes : Side.Party);
                foreach (Fighter other in _fighters)
                    if (Catchable(other, f.Side) && (other == f || DistanceToSide(other.Position, f.Side == Side.Party ? Side.Foes : Side.Party) > mine)) list.Add(other);
                break;
            }

            default:
            {
                BattleArea area = action.Area ?? AreaFor(f, skill, target);
                if (skill.Area == AreaShape.Neighbours && target is not null && Catchable(target, side)) list.Add(target);
                foreach (Fighter other in _fighters)
                {
                    if (!Catchable(other, side) || list.Contains(other)) continue;
                    if (area is null || area.Contains(other.Position, other.Size)) list.Add(other);
                }
                break;
            }
        }
        return list;
    }

    /// <summary>The standing fighters of a side, nearest a point first (ties in joining order).</summary>
    private List<Fighter> NearestFirst(Vector2 from, Side side)
    {
        var list = new List<Fighter>();
        foreach (Fighter other in _fighters)
            if (Catchable(other, side)) list.Add(other);
        return list.OrderBy(other => Vector2.Distance(from, other.Position)).ThenBy(other => other.Index).ToList();
    }

    /// <summary>How far a point is from the nearest standing fighter of a side, or a long way if there's nobody.</summary>
    internal float DistanceToSide(Vector2 from, Side side)
    {
        float best = float.MaxValue;
        foreach (Fighter other in _fighters)
            if (Catchable(other, side)) best = MathF.Min(best, Vector2.Distance(from, other.Position));
        return best;
    }

    // Resolving a blow.

    /// <summary>Lands one blow of a move on its targets (worked out now if none are given): each of its skill's effects in turn on each target, the ones about the user or the whole side once.</summary>
    /// <param name="f">The user.</param>
    /// <param name="action">The move.</param>
    /// <param name="index">Which of its blows, from 0.</param>
    /// <param name="targets">Who it lands on, or null to work it out now (a projectile arriving at an area).</param>
    /// <param name="first">Whether this is the first (or only) part of the blow, which applies the effects that happen once a blow.</param>
    /// <param name="split">How many hits of its clip the damage is shared across, for a single blow with several.</param>
    /// <param name="scale">How strong it is: 1, or a half for an echo (Echo Strike).</param>
    private void Resolve(Fighter f, ActionState action, int index, List<Fighter> targets, bool first, int split, double scale = 1.0)
    {
        if (Outcome != FightOutcome.Ongoing || !f.Present || f.HasLeft) return;
        targets ??= TargetsFor(f, action, index);
        SkillDefinition skill = action.Skill;
        bool aimedAtOne = skill.Target == SkillTarget.Foe && !IsArea(skill.Area);
        if (targets.Count == 0)
        {
            if (aimedAtOne || !first) return;
            var empty = new BlowState { User = f, Skill = skill, Action = action, Index = index, Split = 1, Primary = action.Target, FirstTarget = true, Area = IsArea(skill.Area), Scale = scale };
            foreach (Effect effect in skill.Effects)
                if (effect.On != EffectOn.Targets && Applies(empty, effect)) ApplyEverywhere(empty, effect);
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            var blow = new BlowState
            {
                User = f, Skill = skill, Action = action, Index = index, Split = split, Primary = action.Target, Target = targets[i], TargetIndex = i,
                FirstTarget = first && i == 0, Area = IsArea(skill.Area), Landed = true, Scale = scale,
            };
            foreach (Effect effect in skill.Effects)
            {
                if (!Applies(blow, effect)) continue;
                bool landed = ApplyEverywhere(blow, effect);
                if (effect.Kind == EffectType.Damage || effect.Kind == EffectType.Aura && effect.Save is not null) blow.Landed = landed;
            }
            if (Outcome != FightOutcome.Ongoing) return;
        }
    }

    /// <summary>Whether an effect applies on this blow at all: not an Arm (which arming did), not a once-only one after the first blow, not one waiting on a blow that didn't land, and only the weapon for the Soul-Link partner's half.</summary>
    private static bool Applies(BlowState blow, Effect effect)
    {
        if (effect.Kind == EffectType.Arm) return false;
        if (effect.Once && blow.Index > 0) return false;
        if (Waits(blow, effect)) return false;
        if (blow.Action is { Partner: true } && !(effect.Kind == EffectType.Damage && effect.Weapon)) return false;
        return true;
    }

    /// <summary>Whether an effect waits on how the one before it went: IfLanded on a blow that didn't land, IfSaved on one that did.</summary>
    private static bool Waits(BlowState blow, Effect effect) => effect.IfLanded && !blow.Landed || effect.IfSaved && blow.Landed;

    /// <summary>Whether an effect suits this target and this fight: its place (outdoors or in), undead only, not the user, within its distance, a target an ally just hit.</summary>
    private bool Suits(Fighter f, Effect e, Fighter on)
    {
        if (e.Place == Place.Outdoors && !Rules.Outdoors || e.Place == Place.Indoors && Rules.Outdoors) return false;
        if (e.UndeadOnly && on.Enemy?.Undead != true) return false;
        if (e.NotSelf && on == f) return false;
        if (e.WithinPx > 0f && Vector2.Distance(f.Position, on.Position) > e.WithinPx + f.Size + on.Size) return false;
        if (e.IfHitByAlly)
        {
            bool hit = false;
            foreach ((Fighter who, int at) in on.RecentHits)
                if (who != f && who.Side == f.Side && NowMs - at <= SneakWindowMs) hit = true;
            if (!hit) return false;
        }
        return true;
    }

    /// <summary>Applies an effect to whoever its On says, and says whether it landed on the blow's target.</summary>
    private bool ApplyEverywhere(BlowState blow, Effect effect)
    {
        switch (effect.On)
        {
            case EffectOn.Targets:
                return blow.Target is not null && Apply(blow, effect, blow.Target);
            case EffectOn.Self:
                return !blow.FirstTarget || Apply(blow, effect, blow.User);
            case EffectOn.Primary:
                return !blow.FirstTarget || blow.Primary is null || Apply(blow, effect, blow.Primary);
            case EffectOn.Attacker:
                return blow.Attacker is null || Apply(blow, effect, blow.Attacker);
            case EffectOn.RandomFoe:
            {
                if (!blow.FirstTarget) return true;
                Side foes = blow.User.Side == Side.Party ? Side.Foes : Side.Party;
                Fighter picked = Pick(foes, blow.Target) ?? (blow.Target is { IsStanding: true } only && only.Side == foes ? only : null);
                return picked is null || Apply(blow, effect, picked);
            }
            default:
            {
                if (!blow.FirstTarget) return true;
                foreach (Fighter who in SideOf(blow.User, effect.On, effect.Kind == EffectType.Heal, effect.Kind == EffectType.Dispel)) Apply(blow, effect, who);
                return true;
            }
        }
    }

    /// <summary>Everyone an effect aimed at a whole side lands on: every ally standing (and the Downed, for a heal), every ally but the user, or every foe (the vanished too, for a dispel that brings them out).</summary>
    private List<Fighter> SideOf(Fighter user, EffectOn on, bool heal, bool hidden = false)
    {
        var list = new List<Fighter>();
        foreach (Fighter other in _fighters)
        {
            if (!other.Present || other.IsDead || other.HasLeft) continue;
            bool ally = other.Side == user.Side;
            switch (on)
            {
                case EffectOn.AllAllies when ally && (other.IsStanding || heal && other.IsDown):
                case EffectOn.OtherAllies when ally && other != user && (other.IsStanding || heal && other.IsDown):
                case EffectOn.AllFoes when !ally && other.IsStanding && (!other.Vanished || hidden):
                    list.Add(other);
                    break;
            }
        }
        return list;
    }

    /// <summary>Applies one effect outside a move: a reaction firing, a passive's proc, the start of the fight.</summary>
    /// <param name="user">Whose it is.</param>
    /// <param name="skill">The skill or passive it's from.</param>
    /// <param name="effect">The effect.</param>
    /// <param name="on">Who it's aimed at (the blow's target, for an effect on Targets).</param>
    /// <param name="attacker">Whoever set a reaction off, or null.</param>
    /// <param name="first">Whether the effects that happen once (on the user, on a whole side) apply.</param>
    internal void ApplyLoose(Fighter user, SkillDefinition skill, Effect effect, Fighter on, Fighter attacker, bool first = true)
    {
        if (on is null) return;
        var blow = new BlowState
        {
            User = user, Skill = skill, Primary = on, Target = on, FirstTarget = first, Attacker = attacker, Landed = true, Split = 1, Loose = true,
        };
        ApplyEverywhere(blow, effect);
    }

    /// <summary>What one effect does to one fighter.</summary>
    /// <returns>Whether it landed: a hit, a failed save, an aura that went on; true for the kinds that can't miss.</returns>
    private bool Apply(BlowState blow, Effect e, Fighter on)
    {
        Fighter f = blow.User;
        SkillDefinition skill = blow.Skill;
        if (on is null || !on.Present || on.HasLeft) return false;
        if (!Suits(f, e, on)) return false;
        switch (e.Kind)
        {
            case EffectType.Damage:
                return !on.IsDead && DealDamage(blow, e, on);

            case EffectType.Heal:
                return HealEffect(blow, e, on);

            case EffectType.TempHp:
            {
                if (!on.IsStanding) return false;
                int amount = Amount(f, e, crit: false);
                double pct = Sum(f, Stat.HealingPct, new ModContext { Other = on });
                if (pct != 0) amount = (int)Math.Round(amount * Math.Max(0.0, 1.0 + pct / 100.0));
                amount = (int)Math.Round(amount * blow.Scale);
                Shield(on, amount, e.Aura, f, skill);
                return amount > 0;
            }

            case EffectType.Aura:
            {
                if (on.IsDead || on.IsDown && !e.Aura.Beneficial) return false;
                if (e.Save is Ability save && Hostile(f, on))
                {
                    int dc = Dc(f, skill);
                    if (Save(on, save, dc, e.Aura.Condition, f))
                    {
                        Emit(new BattleEvent { Kind = BattleEventKind.Missed, ActorId = f.Id, TargetId = on.Id, SkillId = skill?.Id, Flags = HitFlags.Save, Position = on.Position, Text = $"{on.Name} resists {e.Aura.Name}." });
                        return false;
                    }
                }
                return ApplyAura(on, e.Aura, f, skill, Dc(f, skill)) is not null;
            }

            case EffectType.Cleanse:
                Cleanse(on, e.Cleanses);
                return true;

            case EffectType.Knockback:
            {
                if (on == f || !on.IsStanding || on.Enemy is { Big: true }) return false;
                Vector2 way = on.Position - f.Position;
                way = way.LengthSquared() < 0.01f ? new Vector2(f.Facing, 0f) : Vector2.Normalize(way);
                Shove(on, on.Position + way * e.Distance, BattleEventKind.KnockedBack, f);
                return true;
            }

            case EffectType.Pull:
            {
                if (on == f || !on.IsStanding || on.Enemy is { Big: true }) return false;
                Vector2 way = on.Position - f.Position;
                way = way.LengthSquared() < 0.01f ? new Vector2(f.Facing, 0f) : Vector2.Normalize(way);
                float keep = Ranges.Melee * 0.7f + f.Size + on.Size;
                if (Vector2.Distance(on.Position, f.Position) <= keep + 1f) return true;
                Shove(on, f.Position + way * keep, BattleEventKind.Pulled, f);
                return true;
            }

            case EffectType.Dash:
            {
                Vector2 way;
                float distance = Math.Abs(e.Distance);
                if (e.Distance >= 0f)
                {
                    Fighter near = NearestFoe(on, attackingMe: false);
                    way = near is null ? new Vector2(-on.Facing, 0f) : on.Position - near.Position;
                }
                else
                {
                    Fighter towards = blow.Target is { } t && t != on ? t : on.Target;
                    way = towards is null ? new Vector2(on.Facing, 0f) : towards.Position - on.Position;
                    if (towards is not null) distance = Math.Min(distance, Math.Max(0f, Vector2.Distance(on.Position, towards.Position) - Ranges.Melee * 0.7f - towards.Size));
                }
                way = way.LengthSquared() < 0.01f ? new Vector2(-on.Facing, 0f) : Vector2.Normalize(way);
                Shove(on, on.Position + way * distance, BattleEventKind.Moved, on);
                return true;
            }

            case EffectType.Teleport:
            {
                Fighter to = blow.Target is { } t && t != on ? t : blow.Primary;
                if (to is null || to == on || !to.IsStanding) return false;
                Shove(on, to.Position - new Vector2(to.Facing * (Ranges.Melee * 0.6f + to.Size), 0f), BattleEventKind.Moved, on);
                Face(on, to.Position);
                return true;
            }

            case EffectType.Swap:
            {
                Fighter back = null;
                float furthest = DistanceToSide(on.Position, on.Side == Side.Party ? Side.Foes : Side.Party);
                foreach (Fighter ally in _fighters)
                {
                    if (ally == on || !Catchable(ally, on.Side)) continue;
                    float away = DistanceToSide(ally.Position, on.Side == Side.Party ? Side.Foes : Side.Party);
                    if (away > furthest)
                    {
                        furthest = away;
                        back = ally;
                    }
                }
                if (back is null) return false;
                Vector2 mine = on.Position;
                Shove(on, back.Position, BattleEventKind.Moved, on);
                Shove(back, mine, BattleEventKind.Moved, on);
                return true;
            }

            case EffectType.Taunt:
            {
                if (!on.IsStanding || !Hostile(f, on)) return false;
                int ms = e.DurationMs > 0 ? e.DurationMs : TurnMs;
                ApplyAura(on, new AuraDefinition { Id = "taunted", Name = "Taunted", Condition = ConditionId.Taunted, DurationMs = ms }, f, skill, 0);
                TopThreat(on, f);
                return true;
            }

            case EffectType.Summon:
            {
                int count = Math.Max(1, e.Amount);
                for (int n = 0; n < count; n++)
                {
                    _summoned++;
                    Vector2 from = Arena.Clamp(on.Position + new Vector2(-on.Facing * 18f, (n % 2 == 0 ? 1f : -1f) * 10f * (n + 1)));
                    Fighter called = Spawn(e.Id, $"{on.Id}-{e.Id}-{_summoned}", on.Side, on.Side == Side.Party ? FighterKind.Summon : FighterKind.Foe, from, from);
                    called.Summoner = on;
                    called.LeavesAtMs = e.DurationMs > 0 ? NowMs + e.DurationMs : 0;
                    if (on.Target is { } aim && Hostile(called, aim)) SetTarget(called, aim);
                }
                return true;
            }

            case EffectType.Transform:
                Transform(on, SkillCatalog.Form(e.Id), e.DurationMs);
                return true;

            case EffectType.Rider:
                on.Riders.Add(new Rider { Skill = skill, Effects = e.Rider, UntilMs = NowMs + (e.DurationMs > 0 ? e.DurationMs : RiderMs) });
                return true;

            case EffectType.Focus:
                GainFocus(on, e.Amount);
                Emit(new BattleEvent { Kind = BattleEventKind.FocusChanged, ActorId = f.Id, TargetId = on.Id, SkillId = skill?.Id, Amount = e.Amount });
                return true;

            case EffectType.Reset:
                if (!on.IsStanding) return false;
                on.GcdUntilMs = NowMs;
                on.NextActionFree = true;
                if (on != f) Emit(new BattleEvent { Kind = BattleEventKind.Bark, ActorId = f.Id, TargetId = on.Id, SkillId = skill?.Id, Position = on.Position, Text = $"{f.Name}: \"{on.Name}, now!\"" });
                return true;

            case EffectType.Gold:
                if (!Hostile(f, on)) return false;
                _stolen[on.Id] = (_stolen.TryGetValue(on.Id, out int taken) ? taken : 0) + e.Amount;
                Emit(new BattleEvent { Kind = BattleEventKind.Stole, ActorId = f.Id, TargetId = on.Id, SkillId = skill?.Id, Amount = e.Amount, Text = $"{f.Name} lifts something from {on.Name}." });
                return true;

            case EffectType.Rewind:
            {
                if (!on.IsStanding) return false;
                int was = HpAgo(on, e.DurationMs > 0 ? e.DurationMs : TurnMs);
                if (was <= on.Hp) return true;
                int back = Math.Min(on.MaxHp, was) - on.Hp;
                on.Hp += back;
                Emit(new BattleEvent
                {
                    Kind = BattleEventKind.Healed, ActorId = f.Id, TargetId = on.Id, SkillId = skill?.Id, Amount = back, Position = on.Position,
                    Text = $"{on.Name}'s wounds wind back: {back} hit points ({skill?.Name}).",
                });
                return true;
            }

            case EffectType.Dispel:
                foreach (Aura aura in on.AuraList)
                    if (!aura.Gone && aura.Beneficial && !aura.Definition.Uncleansable) RemoveAura(on, aura, "dispelled");
                if (on.Vanished && Hostile(f, on))
                {
                    // Dragged out where it stands rather than stepping out behind someone, so the blow's next effect (Reveal's mark) finds it.
                    on.VanishUntilMs = 0;
                    Emit(new BattleEvent { Kind = BattleEventKind.Joined, ActorId = on.Id, SkillId = skill?.Id, Position = on.Position, Text = $"{on.Name} is dragged out of hiding ({skill?.Name})!" });
                }
                return true;

            case EffectType.Interrupt:
                if (on.Action is { InMotion: false } held && held.Skill.Interruptible)
                {
                    Interrupt(on, skill?.Name ?? "interrupted");
                    return true;
                }
                return false;

            case EffectType.Strike when e.ByAllies:
            {
                if (!on.IsStanding || !Hostile(f, on)) return false;
                bool any = false;
                foreach (Fighter ally in SideOf(f, EffectOn.OtherAllies, heal: false))
                {
                    if (!CanAct(ally) || ally.Action is { InMotion: true } || !InReach(ally, ally.Auto, on)) continue;
                    if (ally.Action is not null) Interrupt(ally, skill?.Name ?? "a free strike");
                    any |= StrikeFree(ally, on);
                    if (!on.IsStanding) break;
                }
                return any;
            }

            case EffectType.Strike:
                return on.IsStanding && Hostile(f, on) && StrikeFree(f, on);

            case EffectType.Charge:
            {
                if (blow.Action?.Area is { Shape: AreaKind.Line } line) Shove(on, line.To, BattleEventKind.Moved, on);
                return true;
            }

            case EffectType.Vanish:
            {
                on.VanishUntilMs = NowMs + Math.Max(TickMs, e.DurationMs);
                on.Goal = null;
                CancelHelp(on);
                Emit(new BattleEvent { Kind = BattleEventKind.Vanished, ActorId = on.Id, SkillId = skill?.Id, DurationMs = on.VanishUntilMs - NowMs, Position = on.Position, Text = $"{on.Name} vanishes in a cloud of smoke!" });
                ApplyAura(on, Ambush, on, skill, 0);
                return true;
            }

            case EffectType.TakeOut:
            {
                if (!on.IsStanding || on.Side != Side.Foes || !Hostile(f, on) || !Encounter.Nonlethal || on.HpShare > (e.MaxHpShare > 0 ? e.MaxHpShare : 0.25)) return false;
                Leave(on, $"{on.Name} is taken in hand by {f.Name} ({skill?.Name}).");
                return true;
            }

            case EffectType.Throw:
                return !on.IsDead && Hostile(f, on) && Throw(blow, e, on);

            case EffectType.OneOf:
            {
                if (e.Rider.Count == 0) return false;
                Effect picked = e.Rider[(int)_rng.Below((uint)e.Rider.Count)];
                return Apply(blow, picked with { On = EffectOn.Targets }, on);
            }

            case EffectType.Ready:
                if (!on.IsStanding) return false;
                on.SwingLeftMs = 0;
                return true;

            default:
                return true;
        }
    }

    /// <summary>Throw Anything: the bag's best thing for throwing (its thrown damage, rolled twice, and used up), or whatever's to hand (the effect's own dice, rolled twice), on an attack roll.</summary>
    private bool Throw(BlowState blow, Effect e, Fighter on)
    {
        ItemDefinition best = null;
        if (Rules.Bag is Inventory bag)
            foreach (ItemStack stack in bag.Stacks)
            {
                if (stack.Count <= 0 || !ItemCatalog.Exists(stack.Id)) continue;
                ItemDefinition item = ItemCatalog.Get(stack.Id);
                if (item.Kind == ItemKind.Consumable && !item.ThrownDamage.IsNone && (best is null || item.ThrownDamage.Average > best.ThrownDamage.Average)) best = item;
            }
        DiceExpression dice = best?.ThrownDamage ?? e.Dice;
        if (dice.IsNone) return false;
        if (best is not null)
        {
            Rules.Bag.Remove(best.Id);
            Emit(new BattleEvent { Kind = BattleEventKind.Bark, ActorId = blow.User.Id, TargetId = on.Id, SkillId = blow.Skill?.Id, Text = $"{blow.User.Name} throws the {best.Name}!" });
        }
        var thrown = new Effect
        {
            Kind = EffectType.Damage, Dice = dice with { Count = dice.Count * 2, Bonus = dice.Bonus * 2 }, Type = best?.ThrownDamageType ?? e.Type,
            Delivery = Delivery.Attack,
        };
        return DealDamage(blow, thrown, on);
    }

    /// <summary>What someone stepping out of smoke has: advantage on their next attack, which also lets Sneak Attack in.</summary>
    private static readonly AuraDefinition Ambush = new()
    {
        Id = "ambush", Name = "Ambush", Beneficial = true, DurationMs = 6000, UsedBy = AuraUse.NextAttack, Mods = new[] { new StatMod(Stat.Advantage) },
    };

    // Damage.

    /// <summary>A damage effect on one target: the attack roll or the save, the dice (doubled on a crit, up at levels 5 and 9), what the user and the target carry, Sneak Attack; then the hurt, Focus, lifesteal, the Soul-Link, riders and procs.</summary>
    private bool DealDamage(BlowState blow, Effect e, Fighter t)
    {
        Fighter f = blow.User;
        SkillDefinition skill = blow.Skill;
        bool weapon = e.Weapon;
        AttackProfile attack = weapon ? AttackOf(f, blow.Index, blow.Action?.Auto ?? skill.Kind == SkillKind.Auto) : null;
        BlowKind kind = weapon ? attack.Ranged ? BlowKind.Ranged : BlowKind.Melee
            : skill.Projectile is null && skill.Range is SkillRange.Melee or SkillRange.Reach ? BlowKind.Melee : BlowKind.Spell;
        DamageType type = e.Type ?? attack?.DamageType ?? f.MainAttack?.DamageType ?? DamageType.Bludgeoning;

        bool sneakReady = weapon && (e.ForceSneak || (e.Sneak || f.Enemy?.Abilities.Contains(AbilityId.SneakAttack) == true)
            && NowMs - f.LastSneakMs >= SneakWindowMs && SneakApplies(f, t));
        bool marked = IsMarked(t);

        bool crit = false, half = false;
        HitFlags flags = HitFlags.None;
        switch (e.Delivery)
        {
            case Delivery.Attack:
            {
                int bonus = weapon ? attack.Bonus : SpellAttack(f, skill);
                Ability critAbility = weapon ? WeaponAbility(f, attack) : f.CastingAbility;
                AttackResult result = AttackRoll(f, t, bonus, kind, skill, critAbility);
                if (result.Target != t)
                {
                    t = result.Target;
                    marked = IsMarked(t);
                }
                if (!result.Hit) return false;
                crit = result.Crit;
                break;
            }
            case Delivery.Save:
            {
                var against = new ModContext { Other = f, Area = blow.Area };
                bool evasion = blow.Area && Any(t, Stat.Evasion, against);
                bool made = Save(t, e.Save ?? Ability.Dexterity, Dc(f, skill), null, f, e.UndeadDisadvantage && t.Enemy?.Undead == true, type);
                if (made && (!e.Half || evasion))
                {
                    Emit(new BattleEvent { Kind = BattleEventKind.Missed, ActorId = f.Id, TargetId = t.Id, SkillId = skill?.Id, Flags = HitFlags.Save, Position = t.Position, Text = $"{t.Name} saves against {skill?.Name}." });
                    return false;
                }
                half = made || evasion;
                if (made) flags |= HitFlags.Save;
                break;
            }
        }

        var context = new ModContext
        {
            Other = t, Melee = kind == BlowKind.Melee, Ranged = kind == BlowKind.Ranged, Spell = kind == BlowKind.Spell, Area = blow.Area, Type = type,
            ClassSkill = skill?.Class is not null,
        };

        int amount = Amount(f, e, crit, attack, t);
        if (!blow.ModsUsed && !blow.Loose)
        {
            blow.ModsUsed = true;
            amount += (int)Sum(f, Stat.Damage, context);
            amount += RollMods(f, Stat.DamageDice, context) + (crit ? RollMods(f, Stat.DamageDice, context) : 0);
            amount += TakenExtra(t, f, context, crit);
            if (sneakReady)
            {
                amount += Scaled(new DiceExpression(SneakDice, 6, 0), f.Level).Roll(_rng, "Sneak Attack", crit).Total;
                f.LastSneakMs = NowMs;
            }
        }
        if (skill?.Class is ClassId owner && owner == f.ClassId && f.Rank >= ClassRanks.PowerRank) amount += amount / 10;
        if (crit && e.CritsHarder) amount += amount / 2;
        if (e.TwiceIfMarked && marked) amount *= 2;
        if (blow.TargetIndex > 0 && blow.Skill.Area == AreaShape.Chain) amount = (int)Math.Round(amount * e.OthersShare);
        if (f.Enemy is not null) amount = (int)Math.Round(amount * f.Enemy.RealtimeDamageScale, MidpointRounding.AwayFromZero);
        if (blow.Scale != 1.0) amount = (int)Math.Round(amount * blow.Scale);
        if (half) amount /= 2;
        amount = Math.Max(e.Delivery == Delivery.Save ? 0 : 1, amount);

        if (crit) flags |= HitFlags.Crit;
        if (blow.Action?.Auto == true) flags |= HitFlags.Auto;
        if (skill?.Ultimate == true || skill?.TelegraphMs >= 1000) flags |= HitFlags.Big;
        if (blow.Area) flags |= HitFlags.Area;

        int dealt;
        if (blow.Split > 1 && blow.Action is not null)
        {
            IReadOnlyList<int> hits = MotionTimings.Of(blow.Action.Motion, skill.MotionKind).HitsMs;
            int share = amount / blow.Split;
            dealt = Hurt(t, amount - share * (blow.Split - 1), type, f, flags, skill, e.IgnoreResistance, blow.Area);
            Fighter victim = t;
            for (int i = 1; i < blow.Split && i < hits.Count; i++)
                At(blow.Action.MotionStartMs + hits[i], () => Hurt(victim, share, type, f, flags, skill, e.IgnoreResistance, blow.Area));
        }
        else dealt = Hurt(t, amount, type, f, flags, skill, e.IgnoreResistance, blow.Area);

        bool landed = !half || !flags.HasFlag(HitFlags.Save);
        if (landed && f.Side != t.Side)
        {
            FocusRules rules = FocusModels.Get(f.FocusModel);
            bool cursed = HasCurseFrom(t, f);
            GainFocus(f, crit ? Math.Max(rules.PerCrit, cursed ? rules.PerCursedHit : 0) : cursed ? rules.PerCursedHit : rules.PerHit);
            NoteDuoHit(f, t);
        }
        if (e.Lifesteal > 0 && dealt > 0) Heal(f, f, (int)Math.Round(dealt * e.Lifesteal), skill, tick: false);
        if (e.PartyHealShare > 0 && dealt > 0)
            foreach (Fighter ally in SideOf(f, EffectOn.AllAllies, heal: false)) Heal(f, ally, (int)Math.Round(dealt * e.PartyHealShare), skill, tick: false);

        if (!blow.Loose && dealt > 0)
        {
            Ride(f, t);
            HitProcs(f, t, context);
        }
        return !flags.HasFlag(HitFlags.Save);
    }

    /// <summary>What an effect's numbers come to before the target's and the moment's: its dice (up at 5 and 9 with ⁺, doubled on a crit), the weapon's, the abilities and the level it adds, and more against undead.</summary>
    private int Amount(Fighter f, Effect e, bool crit, AttackProfile attack = null, Fighter t = null)
    {
        int amount = 0;
        if (e.Weapon)
        {
            attack ??= f.MainAttack;
            for (int i = 0; i < Math.Max(1, e.WeaponTimes); i++)
            {
                amount += attack.Damage.Roll(_rng, attack.Name, crit).Total;
                if (!attack.Extra.IsNone) amount += attack.Extra.Roll(_rng, attack.Name, crit).Total;
            }
        }
        if (!e.Dice.IsNone) amount += (e.Plus ? Scaled(e.Dice, f.Level) : e.Dice).Roll(_rng, "skill", crit).Total;
        if (e.AddAbility is Ability ability) amount += f.Scores.Modifier(ability);
        if (e.AddCasting) amount += f.Scores.Modifier(f.CastingAbility);
        amount += e.LevelTimes * f.Level + e.Flat;
        if (!e.VsUndead.IsNone && t?.Enemy?.Undead == true) amount += e.VsUndead.Roll(_rng, "undead", crit).Total;
        return amount;
    }

    /// <summary>The extra a target takes from the hits of the one who cursed it (Hex) or from everyone's.</summary>
    private int TakenExtra(Fighter t, Fighter from, in ModContext context, bool crit)
    {
        int total = 0;
        foreach ((StatMod mod, Fighter source) in Mods(t, Stat.TakenFromSource))
            if (source == from && !mod.Dice.IsNone) total += mod.Dice.Roll(_rng, "curse", crit).Total + mod.Amount;
        foreach ((StatMod mod, Fighter _) in Mods(t, Stat.TakenFromAll))
            total += (mod.Dice.IsNone ? 0 : mod.Dice.Roll(_rng, "mark", crit).Total) + mod.Amount;
        return total;
    }

    /// <summary>Whether a target carries a curse of the user's (Hexed, or any harmful aura of theirs that's a TakenFromSource), for Pact.</summary>
    private static bool HasCurseFrom(Fighter t, Fighter user)
    {
        foreach (Aura aura in t.AuraList)
        {
            if (aura.Gone || aura.Source != user || aura.Beneficial) continue;
            if (aura.Definition.Condition == ConditionId.Hexed) return true;
            foreach (StatMod mod in aura.Definition.Mods)
                if (mod.Stat == Stat.TakenFromSource) return true;
        }
        return false;
    }

    /// <summary>Whether Sneak Attack's conditions are met: an ally of the attacker hit the target in the last 3 s, or it's Marked, Prone or down, or the attacker has advantage from something they carry (stepping out of smoke).</summary>
    private bool SneakApplies(Fighter a, Fighter t)
    {
        foreach ((Fighter who, int at) in t.RecentHits)
            if (who != a && who.Side == a.Side && NowMs - at <= SneakWindowMs) return true;
        if (IsMarked(t) || Has(t, ConditionId.Prone) || t.IsDown) return true;
        return Any(a, Stat.Advantage, new ModContext { Other = t, Melee = true });
    }

    /// <summary>Fires the riders waiting on someone's next hit (Divine Smite, Poisoned Edge), at the one they hit; they're used up.</summary>
    private void Ride(Fighter f, Fighter t)
    {
        if (f.Riders.Count == 0) return;
        var riders = new List<Rider>(f.Riders);
        f.Riders.Clear();
        foreach (Rider rider in riders)
        {
            if (rider.UntilMs < NowMs) continue;
            Emit(new BattleEvent { Kind = BattleEventKind.ReactionFired, ActorId = f.Id, TargetId = t.Id, SkillId = rider.Skill?.Id, Text = $"{f.Name}'s {rider.Skill?.Name}!" });
            var blow = new BlowState { User = f, Skill = rider.Skill, Primary = t, Target = t, FirstTarget = true, Landed = true, Split = 1, Loose = true };
            foreach (Effect effect in rider.Effects)
            {
                if (Waits(blow, effect)) continue;
                bool landed = ApplyEverywhere(blow, effect);
                if (effect.Kind == EffectType.Damage || effect.Kind == EffectType.Aura && effect.Save is not null) blow.Landed = landed;
            }
        }
    }

    /// <summary>Runs the user's on-hit passives (Empowered) on the one they hit.</summary>
    private void HitProcs(Fighter f, Fighter t, in ModContext context)
    {
        foreach (SkillDefinition passive in f.PassiveSkills)
            foreach (Proc proc in passive.Procs)
            {
                if (proc.Trigger != ProcTrigger.OnHit) continue;
                if (proc.When != When.Always && !Counts(new StatMod(Stat.Damage, When: proc.When), f, context)) continue;
                if (!ProcReady(f, passive, proc)) continue;
                foreach (Effect effect in proc.Effects) ApplyLoose(f, passive, effect, t, null);
            }
    }

    /// <summary>A free weapon blow from someone at a foe, landing now (Riposte's answer, a Strike effect).</summary>
    private bool StrikeFree(Fighter f, Fighter t)
    {
        SkillDefinition strike = f.Auto is { Weapon: true } auto ? auto : FreeStrike;
        string motion = WeaponKinds.AutoClip(f.Form?.Weapon ?? f.WeaponKind, f.Swings++);
        f.CurrentMotion = motion;
        f.MotionStartMs = NowMs;
        Face(f, t.Position);
        Emit(new BattleEvent { Kind = BattleEventKind.ActionStarted, ActorId = f.Id, TargetId = t.Id, SkillId = strike.Id, Motion = motion, ImpactMs = NowMs, Position = f.Position, DurationMs = MotionTimings.Of(motion, MotionKind.Auto).LengthMs });
        var blow = new BlowState { User = f, Skill = strike, Primary = t, Target = t, FirstTarget = true, Landed = true, Split = 1 };
        return DealDamage(blow, new Effect { Kind = EffectType.Damage, Weapon = true, Delivery = Delivery.Attack }, t);
    }

    // Healing and forms.

    /// <summary>A heal effect on one ally: a potion, a pool's share (Lay on Hands), a share of their hit points, or dice and what the healer adds; it gets the Downed up.</summary>
    private bool HealEffect(BlowState blow, Effect e, Fighter on)
    {
        Fighter f = blow.User;
        SkillDefinition skill = blow.Skill;
        if (on.IsDead || on.HasLeft || !on.Present) return false;
        if (e.DownedOnly && !on.IsDown) return false;
        if (e.StandingOnly && on.IsDown) return false;
        if (Hostile(f, on)) return false;

        int amount;
        if (e.Id == "potion")
        {
            ItemDefinition potion = BestPotion();
            if (potion is null) return false;
            Rules.Bag.Remove(potion.Id);
            amount = potion.Heals.Roll(_rng, potion.Name).Total;
            amount += (int)Math.Round(amount * Sum(on, Stat.PotionPct, default) / 100.0);
            Emit(new BattleEvent { Kind = BattleEventKind.PotionDrunk, ActorId = on.Id, SkillId = Potion.Id, Text = $"{on.Name} drinks a {potion.Name}." });
        }
        else if (e.PoolPerLevel > 0)
        {
            int left = PoolLeft(f, skill, e);
            int missing = on.IsDown ? on.MaxHp : on.MaxHp - on.Hp;
            amount = Math.Min(left, missing);
            if (amount <= 0) return false;
            f.Pools[skill.Id] = left - amount;
        }
        else
        {
            amount = Amount(f, e, crit: false);
            if (e.MaxHpShare > 0) amount += (int)Math.Round(on.MaxHp * e.MaxHpShare);
        }
        if (blow.Scale != 1.0) amount = (int)Math.Round(amount * blow.Scale);
        Heal(f, on, amount, skill, tick: false);
        return true;
    }

    /// <summary>Turns someone into a form for a while: its auto and skills replace their bar until it ends.</summary>
    private void Transform(Fighter f, FormDefinition form, int ms)
    {
        if (f.Form is null) f.SavedBar = new List<SkillDefinition>(f.BarSkills);
        f.Form = form;
        f.FormUntilMs = NowMs + Math.Max(TurnMs, ms);
        f.BarSkills.Clear();
        f.Auto = SkillCatalog.Get(form.Auto);
        f.BarSkills.Add(f.Auto);
        foreach (string skill in form.Skills) f.BarSkills.Add(SkillCatalog.Get(skill));
        f.SwingTotalMs = SwingInterval(f);
        f.SwingLeftMs = Math.Min(f.SwingLeftMs, f.SwingTotalMs);
        Emit(new BattleEvent { Kind = BattleEventKind.Transformed, ActorId = f.Id, SkillId = form.Id, DurationMs = f.FormUntilMs - NowMs, Position = f.Position, Text = $"{f.Name} becomes a {form.Name.ToLowerInvariant()}!" });
    }
}

/// <summary>One blow landing, as its effects see it: whose, which, on whom, and how it's gone so far.</summary>
internal sealed class BlowState
{
    public Fighter User;
    public SkillDefinition Skill;
    public ActionState Action;
    public int Index;
    public int Split = 1;
    public Fighter Primary;
    public Fighter Target;
    public int TargetIndex;
    public bool FirstTarget;
    public bool Area;
    public bool Landed;
    public bool ModsUsed;
    public Fighter Attacker;
    public bool Loose;
    public double Scale = 1.0;
}
