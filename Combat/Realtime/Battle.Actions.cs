using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>Using skills: what can be used and why not, walking into reach, paying for it, casts and telegraphs, the clip and its blows, projectiles, the auto attack, reactions and potions.</summary>
public sealed partial class Battle
{
    /// <summary>The GCD of an action skill before haste (11-combat.md §2.5).</summary>
    public const int GcdMs = 1500;

    /// <summary>The shortest haste can make the GCD.</summary>
    public const int ShortestGcdMs = 1000;

    /// <summary>The lockout the bonus skills share.</summary>
    public const int BonusLockoutMs = 500;

    /// <summary>The potion's own cooldown.</summary>
    public const int PotionCooldownMs = 30000;

    /// <summary>How long an order may wait on a cooldown or the GCD before it's dropped.</summary>
    private const int LongestWaitMs = 1600;

    /// <summary>How much further than its reach a blow still lands, for someone who stepped back as it came.</summary>
    private const float Slack = 12f;

    /// <summary>The time between the extra blows of a skill whose clip has fewer hits than it has strikes.</summary>
    private const int ExtraBlowMs = 150;

    /// <summary>How long after a skill's last blow its echo lands (Echo Strike).</summary>
    public const int EchoMs = 400;

    /// <summary>How long a telegraph has to be for the party's passives to count it a foe's big move (Somebody Allowed to Say No).</summary>
    public const int BigMoveMs = 1000;

    /// <summary>The potion, as a skill: it goes on its own cooldown and heals what the bottle says.</summary>
    internal static readonly SkillDefinition Potion = new()
    {
        Id = "potion", Name = "Potion", Kind = SkillKind.Bonus, CooldownMs = PotionCooldownMs, Range = SkillRange.Self, Target = SkillTarget.Self,
        Motion = "drink-potion", Ai = AiUse.Heal, Effects = new[] { new Effect { Kind = EffectType.Heal, On = EffectOn.Self, Id = "potion" } },
        Description = "Drink the bag's best healing potion.",
    };

    // Asking.

    /// <summary>Why a fighter can't use a skill on a target now, or null if they can (reach aside: a skill out of reach walks them into it).</summary>
    /// <param name="f">Who.</param>
    /// <param name="skill">What.</param>
    /// <param name="target">At whom, or null for the skill's own choice.</param>
    /// <param name="timing">Whether the cooldown, the GCD and being busy count (false for an order that will wait for them).</param>
    internal string Problem(Fighter f, SkillDefinition skill, Fighter target, bool timing = true)
    {
        if (Outcome != FightOutcome.Ongoing) return "The fight's over";
        if (f.IsOut) return f.IsDown ? "Down" : "Out of the fight";
        if (!CanAct(f)) return "Can't act";
        if (skill.Kind == SkillKind.Passive) return "Always on";
        if (skill != Potion && skill.Id != SkillCatalog.SoulLinkStrike && !f.BarSkills.Contains(skill)) return "Not on the bar";
        if (timing)
        {
            if (CooldownLeft(f, skill) > 0) return "Cooling down";
            if (skill.OnGcd && f.GcdUntilMs > NowMs) return "Global cooldown";
            if (skill.Kind is SkillKind.Bonus or SkillKind.Reaction && f.BonusUntilMs > NowMs && skill != Potion) return "Too soon";
        }
        if (FocusCost(f, skill) > f.Focus) return "Not enough Focus";
        if (skill.HpCost > 0 && skill.HpCost >= f.Hp) return "Not enough hit points";
        if (skill.Kind == SkillKind.Reaction && Armed(f, skill) is not null) return "Already armed";

        Fighter aimed = target ?? DefaultTarget(f, skill);
        switch (skill.Target)
        {
            case SkillTarget.Foe:
                if (aimed is null || !Hostile(f, aimed) || !aimed.IsStanding) return "No target";
                if (!Targetable(aimed, skill)) return "Can't be targeted";
                if (skill.OncePerFoe && f.UsedOnFoe.Contains(skill.Id + "|" + aimed.Id)) return "Already done to it";
                break;
            case SkillTarget.Ally:
                if (aimed is null || aimed.Side != f.Side || aimed.IsDead || aimed.HasLeft || !aimed.Present) return "No ally";
                if (aimed.IsDown && !Revives(skill)) return "They're down";
                break;
        }

        foreach (Effect effect in skill.Effects)
        {
            if (effect.Kind == EffectType.Heal && effect.PoolPerLevel > 0 && PoolLeft(f, skill, effect) <= 0) return "Nothing left";
            if (effect.Kind == EffectType.Heal && effect.Id == "potion" && BestPotion() is null) return "No potions";
            if (effect.Kind == EffectType.Summon && Summoned(f, effect.Id)) return "Already here";
        }
        return null;
    }

    /// <summary>Whether a skill heals (and so gets a Downed ally up).</summary>
    private static bool Revives(SkillDefinition skill)
    {
        foreach (Effect effect in skill.Effects)
            if (effect.Kind == EffectType.Heal) return true;
        return false;
    }

    /// <summary>Who a skill goes at when nobody's named: the target if it suits, else the nearest foe attacking you (or the nearest), the most hurt ally, or yourself.</summary>
    internal Fighter DefaultTarget(Fighter f, SkillDefinition skill)
    {
        switch (skill.Target)
        {
            case SkillTarget.Foe:
                if (f.Target is { } t && Hostile(f, t) && t.IsStanding && Targetable(t, skill)) return t;
                return NearestFoe(f, attackingMe: true) ?? NearestFoe(f, attackingMe: false);
            case SkillTarget.Ally:
                if (f.Target is { } a && a.Side == f.Side && !a.IsDead && a.Present && !a.HasLeft) return a;
                return MostHurtAlly(f) ?? f;
            case SkillTarget.Self:
                return f;
            default:
                return null;
        }
    }

    /// <summary>The nearest standing, targetable foe of someone, perhaps only among those attacking them.</summary>
    internal Fighter NearestFoe(Fighter f, bool attackingMe)
    {
        Fighter best = null;
        float bestDistance = float.MaxValue;
        foreach (Fighter other in _fighters)
        {
            if (!Hostile(f, other) || !other.IsStanding || other.Vanished) continue;
            if (attackingMe && other.Target != f) continue;
            float distance = Vector2.Distance(f.Position, other.Position);
            if (distance < bestDistance)
            {
                best = other;
                bestDistance = distance;
            }
        }
        return best;
    }

    /// <summary>The most hurt ally of someone (by share of hit points), Downed first, or null if nobody's hurt.</summary>
    internal Fighter MostHurtAlly(Fighter f)
    {
        Fighter best = null;
        double worst = 1.0;
        foreach (Fighter other in _fighters)
        {
            if (other.Side != f.Side || other.IsDead || other.HasLeft || !other.Present) continue;
            double share = other.IsDown ? -1.0 : other.HpShare;
            if (share < worst)
            {
                best = other;
                worst = share;
            }
        }
        return best;
    }

    /// <summary>Whether someone can be picked out by a skill: not vanished, and not Frozen unless the skill's an area.</summary>
    internal bool Targetable(Fighter target, SkillDefinition skill)
    {
        if (target.Vanished) return false;
        if (Has(target, ConditionId.Frozen) && skill.Area is AreaShape.Single or AreaShape.Spread or AreaShape.RandomFoes or AreaShape.Chain) return false;
        return true;
    }

    /// <summary>How long until a skill's own cooldown is up, in milliseconds (the potion's included).</summary>
    public int CooldownLeft(Fighter f, SkillDefinition skill)
    {
        if (skill == Potion) return Math.Max(0, f.PotionReadyMs - NowMs);
        return f.CooldownUntil.TryGetValue(skill.Id, out int until) ? Math.Max(0, until - NowMs) : 0;
    }

    /// <summary>What a skill costs someone now: its Focus, or nothing if their next skill is free.</summary>
    internal int FocusCost(Fighter f, SkillDefinition skill)
    {
        if (skill.Focus == 0 || f.Side == Side.Foes) return 0;
        foreach (Aura aura in f.AuraList)
            if (!aura.Gone && aura.Definition.UsedBy == AuraUse.NextSkill && HasMod(aura.Definition, Stat.FreeSkill)) return 0;
        return skill.Focus;
    }

    // Times, with haste.

    /// <summary>Someone's haste, in percent, from -50 to 50 (11-combat.md §3.3): 3 a point of their Dexterity (or their casting ability, for a caster) above 0, and whatever's on them.</summary>
    internal int Haste(Fighter f)
    {
        Ability ability = f.ClassId is ClassId id && ClassCatalog.Get(id).CastingAbility is Ability casting ? casting : Ability.Dexterity;
        int fromStat = f.Side == Side.Foes && f.Kind == FighterKind.Foe ? 0 : 3 * Math.Max(0, f.Scores.Modifier(ability));
        return Math.Clamp(fromStat + (int)Sum(f, Stat.Haste, default), -50, 50);
    }

    /// <summary>Shortens a time by someone's haste.</summary>
    private int Hasted(Fighter f, int ms) => (int)Math.Round(ms * (1.0 - Haste(f) / 100.0));

    /// <summary>Someone's auto attack's interval now.</summary>
    internal int SwingInterval(Fighter f) => Math.Max(600, Hasted(f, f.Form?.SwingMs ?? f.BaseSwingMs));

    /// <summary>Someone's GCD now: 1.5 s, shortened by haste to 1.0 s at the least.</summary>
    internal int GcdLength(Fighter f) => Math.Max(ShortestGcdMs, Hasted(f, GcdMs));

    /// <summary>A skill's cooldown for someone now: its own (or what a boss's phase made it), shortened by haste and anything that speeds cooldowns.</summary>
    internal int CooldownLength(Fighter f, SkillDefinition skill)
    {
        int own = f.CooldownOverride.TryGetValue(skill.Id, out int changed) ? changed : skill.CooldownMs;
        double rate = 1.0 - Math.Clamp(Sum(f, Stat.CooldownRate, default), 0, 50) / 100.0;
        return (int)Math.Round(Hasted(f, own) * rate);
    }

    // Commands into orders.

    /// <summary>A Use command: the skill on someone, now if it can be, else walking into reach (or waiting out a short cooldown) first.</summary>
    private void Use(Fighter f, string skillId, Fighter target)
    {
        SkillDefinition skill = null;
        foreach (SkillDefinition s in f.BarSkills)
            if (s.Id == skillId) skill = s;
        if (skillId == Potion.Id) skill = Potion;
        if (skill is null)
        {
            Deny(f, skillId, "Not on the bar");
            return;
        }

        if (skill.Kind == SkillKind.Auto)
        {
            f.AutoAttackOn = !f.AutoAttackOn || target is not null;
            if (target is not null) SetTarget(f, target);
            return;
        }

        target ??= DefaultTarget(f, skill);
        string why = Problem(f, skill, target, timing: false);
        if (why is not null)
        {
            Deny(f, skill.Id, why);
            return;
        }
        if (skill.Target == SkillTarget.Foe && target is not null && f.Target != target) SetTarget(f, target);

        f.Order = new Order { Kind = CommandKind.Use, Skill = skill, Target = target, Point = new Vector2(NowMs, 0) };
        TickOrder(f);
    }

    /// <summary>Says a command couldn't be done.</summary>
    private void Deny(Fighter f, string skill, string why) =>
        Emit(new BattleEvent { Kind = BattleEventKind.Denied, ActorId = f.Id, SkillId = skill, Text = why });

    /// <summary>Carries an order on: walks into reach, waits for the GCD or a cooldown, then uses the skill; or walks to a Downed ally and starts helping.</summary>
    private void TickOrder(Fighter f)
    {
        if (f.Order is not Order order) return;
        if (order.Kind == CommandKind.Help)
        {
            TickHelpOrder(f, order);
            return;
        }
        if (order.Kind != CommandKind.Use) return;

        SkillDefinition skill = order.Skill;
        Fighter target = order.Target;
        if (target is not null && (target.IsDead || target.HasLeft || skill.Target == SkillTarget.Foe && !target.IsStanding))
        {
            f.Order = null;
            return;
        }

        string why = Problem(f, skill, target, timing: false);
        if (why is not null)
        {
            Deny(f, skill.Id, why);
            f.Order = null;
            return;
        }

        if (!InReach(f, skill, target))
        {
            Approach(f, target, RangeOf(f, skill));
            return;
        }

        if (f.Action is not null || !CanAct(f)) return;
        bool waiting = CooldownLeft(f, skill) > 0 || skill.OnGcd && f.GcdUntilMs > NowMs || skill.Kind is SkillKind.Bonus or SkillKind.Reaction && f.BonusUntilMs > NowMs;
        if (waiting)
        {
            if (NowMs - (int)order.Point.X > LongestWaitMs && CooldownLeft(f, skill) > LongestWaitMs) f.Order = null;
            return;
        }

        f.Order = null;
        if (f.Goal is not null && skill.Range != SkillRange.Self) f.Goal = null;
        StartAction(f, skill, target, auto: false);
    }

    // Starting a move.

    /// <summary>Starts a skill: pays for it, starts its cooldown and the GCD, then its cast, its telegraph or its clip.</summary>
    internal void StartAction(Fighter f, SkillDefinition skill, Fighter target, bool auto)
    {
        if (target is not null && target != f) Face(f, target.Position);

        bool echo = false;
        if (!auto)
        {
            Pay(f, skill, target);
            f.Uses[skill.Id] = f.Uses.TryGetValue(skill.Id, out int used) ? used + 1 : 1;
            echo = UseProcs(f, skill);
        }
        else
        {
            f.SwingTotalMs = SwingInterval(f);
            f.SwingLeftMs = f.SwingTotalMs;
            f.Uses[skill.Id] = f.Uses.TryGetValue(skill.Id, out int swung) ? swung + 1 : 1;
        }

        if (skill.Kind == SkillKind.Reaction)
        {
            ArmReaction(f, skill, target);
            return;
        }

        var action = new ActionState { Skill = skill, Target = target, Auto = auto, Strikes = Math.Max(1, skill.StrikesAt(f.Level)), Serial = (int)_sequence, Echo = echo };
        action.Aim = target?.Position ?? f.Position;
        f.Action = action;
        f.Goal = null;

        if (skill.CastMs > 0)
        {
            action.CastStartMs = NowMs;
            action.CastUntilMs = NowMs + Math.Max(300, Hasted(f, skill.CastMs));
            action.Area = AreaFor(f, skill, target);
            Emit(new BattleEvent
            {
                Kind = BattleEventKind.CastStarted, ActorId = f.Id, TargetId = target?.Id, SkillId = skill.Id, DurationMs = action.CastUntilMs - NowMs,
                ImpactMs = action.CastUntilMs, Area = action.Area, Position = f.Position, Text = $"{f.Name} begins to cast {skill.Name}.",
            });
            FoeCastReactions(f, action);
            return;
        }

        if (skill.TelegraphMs > 0)
        {
            int wind = skill.TelegraphMs;
            if (f.Side == Side.Foes && Rules.Difficulty == Difficulty.Story) wind = wind * 3 / 2;
            action.CastStartMs = NowMs;
            action.TelegraphUntilMs = NowMs + wind;
            action.Area = AreaFor(f, skill, target);
            int lands = action.TelegraphUntilMs + FirstMomentMs(f, skill) + (skill.Projectile is DamageType flies && action.Area is { Shape: not AreaKind.Room }
                ? (int)(Vector2.Distance(f.Position, action.Area.Center) / ProjectileSpeed(flies) * 1000f) : 0);
            Emit(new BattleEvent
            {
                Kind = BattleEventKind.Telegraph, ActorId = f.Id, TargetId = target?.Id, SkillId = skill.Id, DurationMs = lands - NowMs, ImpactMs = lands,
                Area = action.Area, Position = action.Area?.Center ?? f.Position, Motion = "telegraph-" + WeaponKinds.Clip(f.WeaponKind),
                Text = $"{f.Name} winds up {skill.Name}!",
            });
            FoeCastReactions(f, action);
            if (f.Side == Side.Foes && skill.TelegraphMs >= BigMoveMs && f.Action == action) FoeBigMove(f);
            return;
        }

        BeginMotion(f, action);
    }

    /// <summary>Pays for a skill: Focus (or a free-skill aura), hit points, its cooldown, the GCD or the bonus lockout; says so for the bar's sweeps.</summary>
    private void Pay(Fighter f, SkillDefinition skill, Fighter target)
    {
        int focus = FocusCost(f, skill);
        if (focus == 0 && skill.Focus > 0) UseUp(f, AuraUse.NextSkill);
        if (focus > 0) f.FocusMilli = Math.Max(0, f.FocusMilli - focus * 1000);
        if (skill.HpCost > 0) f.Hp = Math.Max(1, f.Hp - skill.HpCost);

        int cooldown = CooldownLength(f, skill);
        bool free = f.NextActionFree && skill.OnGcd;
        if (free) f.NextActionFree = false;
        if (skill == Potion) f.PotionReadyMs = NowMs + cooldown;
        else if (cooldown > 0 && !free)
        {
            f.CooldownUntil[skill.Id] = NowMs + cooldown;
            f.CooldownTotal[skill.Id] = cooldown;
        }

        if (skill.OnGcd)
        {
            f.GcdTotalMs = GcdLength(f);
            f.GcdUntilMs = NowMs + f.GcdTotalMs;
        }
        else if (skill.Kind is SkillKind.Bonus or SkillKind.Reaction && skill != Potion) f.BonusUntilMs = NowMs + BonusLockoutMs;

        if (skill.OncePerFoe && target is not null) f.UsedOnFoe.Add(skill.Id + "|" + target.Id);
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.SkillUsed, ActorId = f.Id, TargetId = target?.Id, SkillId = skill.Id, Amount = focus, DurationMs = free ? 0 : cooldown,
            Text = $"{f.Name} uses {skill.Name}.",
        });
    }

    /// <summary>Finishes a cast or a telegraph whose time is up, and starts its clip.</summary>
    private void TickCast(Fighter f)
    {
        if (f.Action is not ActionState action || action.InMotion) return;
        int until = action.CastUntilMs > 0 ? action.CastUntilMs : action.TelegraphUntilMs;
        if (until > 0 && NowMs >= until) BeginMotion(f, action);
    }

    /// <summary>Starts a move's clip and schedules its blows at the clip's hit and release events (or the fallback times).</summary>
    private void BeginMotion(Fighter f, ActionState action)
    {
        SkillDefinition skill = action.Skill;
        action.InMotion = true;
        action.Motion = MotionOf(f, skill, action.Auto, action.SoulLink);
        action.MotionStartMs = NowMs;
        f.CurrentMotion = action.Motion;
        f.MotionStartMs = NowMs;
        if (action.Auto) f.Swings++;

        MotionTiming timing = MotionTimings.Of(action.Motion, skill.MotionKind);
        IReadOnlyList<int> moments = timing.Moments;
        if (moments.Count == 0) moments = MotionTimings.Fallback(action.Motion, skill.MotionKind).Moments;
        int strikes = action.Strikes;
        int combo = action.Auto && f.Enemy is { } enemy && enemy.Abilities.Contains(AbilityId.Multiattack) ? ComboSize(f) : 1;
        strikes = Math.Max(strikes, combo);
        action.Strikes = strikes;

        action.BlowsAt.Clear();
        for (int i = 0; i < strikes; i++)
            action.BlowsAt.Add(NowMs + (i < moments.Count ? moments[i] : moments[moments.Count - 1] + (i - moments.Count + 1) * ExtraBlowMs));

        Emit(new BattleEvent
        {
            Kind = BattleEventKind.ActionStarted, ActorId = f.Id, TargetId = action.Target?.Id, SkillId = skill.Id, Motion = action.Motion,
            ImpactMs = action.BlowsAt[0], DurationMs = timing.LengthMs, Position = f.Position, Area = action.Area,
            Flags = action.Auto ? HitFlags.Auto : skill.Ultimate ? HitFlags.Big : HitFlags.None,
            Text = action.Auto ? null : $"{f.Name}: {skill.Name}.",
        });

        int split = strikes == 1 && timing.HitsMs.Count > 1 ? timing.HitsMs.Count : 1;
        for (int i = 0; i < strikes; i++)
        {
            int blow = i;
            At(action.BlowsAt[i], () => Blow(f, action, blow, split));
        }
    }

    /// <summary>How many of a stat block's attacks its Multiattack swing strikes.</summary>
    private static int ComboSize(Fighter f) => f.Enemy.Combo > 0 ? Math.Min(f.Enemy.Combo, f.Attacks.Count) : f.Attacks.Count;

    /// <summary>The clip a fighter plays for a skill: a weapon auto's auto-&lt;kind&gt;-n, a foe's move's foe-&lt;enemy&gt;-&lt;move&gt;, otherwise the skill's.</summary>
    internal static string MotionOf(Fighter f, SkillDefinition skill, bool auto, bool soulLink = false)
    {
        if (soulLink) return "soul-link-strike";
        WeaponKind weapon = f.Form?.Weapon ?? f.WeaponKind;
        if (skill.Id == SkillCatalog.FoeAttack && f.Enemy is { } enemy && enemy.Abilities.Contains(AbilityId.Multiattack)
            && MotionTimings.Exists("foe-" + enemy.Id + "-multiattack"))
            return "foe-" + enemy.Id + "-multiattack";
        if (skill.Id == SkillCatalog.FoeAttack || auto && skill.Weapon) return WeaponKinds.AutoClip(weapon, f.Swings);
        if (skill.Motion is null && skill.Id.StartsWith("foe-", StringComparison.Ordinal) && f.Enemy is not null)
            return "foe-" + f.Enemy.Id + "-" + skill.Id.Substring(4);
        return skill.MotionFor(weapon, f.Swings);
    }

    /// <summary>How long after a move's clip starts its first blow lands or leaves the hand, for a telegraph's marker to fill until.</summary>
    private static int FirstMomentMs(Fighter f, SkillDefinition skill)
    {
        string motion = MotionOf(f, skill, skill.Kind == SkillKind.Auto);
        IReadOnlyList<int> moments = MotionTimings.Of(motion, skill.MotionKind).Moments;
        return moments.Count > 0 ? moments[0] : MotionTimings.Fallback(motion, skill.MotionKind).Moments[0];
    }

    /// <summary>One blow of a move: it lands now, or something leaves the hand and lands when it arrives.</summary>
    private void Blow(Fighter f, ActionState action, int index, int split)
    {
        if (f.Action != action || !f.IsStanding) return;
        action.NextBlow = index + 1;
        bool last = index == action.Strikes - 1;
        if (last) f.Action = null;

        SkillDefinition skill = action.Skill;
        if (last && action.Echo) At(NowMs + EchoMs, () => EchoOf(f, action));
        List<Fighter> targets = TargetsFor(f, action, index);
        DamageType? projectile = skill.Projectile ?? (skill.Weapon && AttackOf(f, index, action.Auto).Ranged ? DamageType.Piercing : null);

        if (projectile is DamageType school)
        {
            bool area = IsArea(skill.Area);
            if (area || targets.Count == 0)
            {
                Vector2 to = action.Area is { Shape: not AreaKind.Room } fixedArea ? fixedArea.Center : targets.Count > 0 ? Centre(targets, f.Position) : action.Target?.Position ?? f.Position;
                int arrive = Launch(f, skill, school, to, action.Target);
                At(arrive, () => Resolve(f, action, index, null, true, 1));
            }
            else
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    Fighter target = targets[i];
                    bool first = i == 0;
                    int arrive = Launch(f, skill, school, target.Position, target);
                    At(arrive, () => Resolve(f, action, index, new List<Fighter> { target }, first, 1));
                }
            }
            return;
        }

        Resolve(f, action, index, targets, true, split);
    }

    /// <summary>A skill's echo (Echo Strike): its first blow again, at half strength, on whoever it would catch now.</summary>
    private void EchoOf(Fighter f, ActionState action)
    {
        if (!f.IsStanding || Outcome != FightOutcome.Ongoing) return;
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.ReactionFired, ActorId = f.Id, TargetId = action.Target?.Id, SkillId = action.Skill.Id, Position = action.Target?.Position ?? f.Position,
            Text = $"{action.Skill.Name} happens again, an echo of itself.",
        });
        Resolve(f, action, 0, null, true, 1, 0.5);
    }

    /// <summary>Lets something fly from a fighter to a point, and says when it gets there.</summary>
    private int Launch(Fighter f, SkillDefinition skill, DamageType school, Vector2 to, Fighter target)
    {
        float distance = Vector2.Distance(f.Position, to);
        int arrive = NowMs + Math.Max(TickMs, (int)(distance / ProjectileSpeed(school) * 1000f));
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.ProjectileLaunched, ActorId = f.Id, TargetId = target?.Id, SkillId = skill.Id, Position = f.Position, DamageType = school,
            ImpactMs = arrive, Area = BattleArea.Line(f.Position, to, 0f), DurationMs = arrive - NowMs,
        });
        return arrive;
    }

    /// <summary>How fast a school's projectiles fly, in world pixels a second.</summary>
    internal static float ProjectileSpeed(DamageType school) => school switch
    {
        DamageType.Fire => 360f,
        DamageType.Force => 440f,
        DamageType.Radiant => 480f,
        DamageType.Necrotic => 300f,
        DamageType.Lightning => 900f,
        DamageType.Cold => 380f,
        DamageType.Piercing => 620f,
        DamageType.Thunder or DamageType.Psychic => 520f,
        _ => 400f,
    };

    /// <summary>Whether a shape catches more than the one it's aimed at.</summary>
    internal static bool IsArea(AreaShape shape) => shape is not (AreaShape.Single or AreaShape.Spread or AreaShape.RandomFoes or AreaShape.Chain);

    /// <summary>The middle of some fighters, or a fallback point if there are none.</summary>
    private static Vector2 Centre(List<Fighter> fighters, Vector2 fallback)
    {
        if (fighters.Count == 0) return fallback;
        Vector2 sum = Vector2.Zero;
        foreach (Fighter f in fighters) sum += f.Position;
        return sum / fighters.Count;
    }

    /// <summary>Stops a cast, a telegraph or a move that hasn't landed yet. Its costs stay paid.</summary>
    internal void Interrupt(Fighter f, string why)
    {
        if (f.Action is not ActionState action) return;
        bool landed = action.InMotion && action.NextBlow > 0;
        f.Action = null;
        if (landed) return;
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.CastInterrupted, ActorId = f.Id, SkillId = action.Skill.Id, Position = f.Position,
            Text = $"{f.Name}'s {action.Skill.Name} is stopped ({why}).",
        });
    }

    // The auto attack.

    /// <summary>Runs the swing timer down while they can act.</summary>
    private void TickSwing(Fighter f)
    {
        if (!CanAct(f) || f.Helping is not null) return;
        if (f.SwingLeftMs > 0) f.SwingLeftMs = Math.Max(0, f.SwingLeftMs - TickMs);
    }

    /// <summary>Swings when the timer's round, they're free, and their target's in reach.</summary>
    private void TickAuto(Fighter f)
    {
        if (!f.AutoAttackOn || f.SwingLeftMs > 0 || f.Action is not null || !CanAct(f) || f.Helping is not null) return;
        Fighter target = f.Target;
        if (target is null || !Hostile(f, target) || !target.IsStanding || !Targetable(target, f.Auto))
        {
            if (f == Leader) Retarget(f);
            return;
        }
        if (!InReach(f, f.Auto, target))
        {
            if (f.Chase && f.Order is null && CanMove(f)) Approach(f, target, RangeOf(f, f.Auto));
            return;
        }
        if (f.Auto.CastMs > 0 && f.Goal is not null) return;
        StartAction(f, f.Auto, target, auto: true);
    }

    /// <summary>Points the leader at a new foe when theirs is gone: the nearest attacking them, else the nearest (11-combat.md §2.3).</summary>
    private void Retarget(Fighter f)
    {
        Fighter next = NearestFoe(f, attackingMe: true) ?? NearestFoe(f, attackingMe: false);
        if (next != f.Target) f.Target = next;
        if (next is null) f.AutoAttackOn = false;
    }

    /// <summary>The attack a blow of a weapon move uses: the main hand, or a stat block's attack for this blow of its combo.</summary>
    internal static AttackProfile AttackOf(Fighter f, int blow, bool auto)
    {
        if (f.Enemy is not null && auto && f.Attacks.Count > 1 && f.Enemy.Abilities.Contains(AbilityId.Multiattack))
            return f.Attacks[blow % ComboSize(f)];
        return f.MainAttack;
    }

    // Reactions.

    /// <summary>Arms a reaction: it waits for its trigger for ArmMs (over an ally, for Cover).</summary>
    private void ArmReaction(Fighter f, SkillDefinition skill, Fighter target)
    {
        var armed = new Armed { Skill = skill, Over = target ?? f, UntilMs = NowMs + skill.ArmMs };
        f.ArmedList.Add(armed);
        string motion = MotionOf(f, skill, false);
        f.CurrentMotion = motion;
        f.MotionStartMs = NowMs;
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.ActionStarted, ActorId = f.Id, TargetId = armed.Over.Id, SkillId = skill.Id, Motion = motion, ImpactMs = NowMs,
            DurationMs = MotionTimings.Of(motion, MotionKind.Reaction).LengthMs, Position = f.Position,
        });
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.Armed, ActorId = f.Id, TargetId = armed.Over.Id, SkillId = skill.Id, DurationMs = skill.ArmMs,
            Text = $"{f.Name} readies {skill.Name}.",
        });

        // A counter armed while a foe is already winding something up answers that at once.
        if (skill.Trigger != ReactionTrigger.FoeCast) return;
        foreach (Fighter caster in _fighters)
        {
            if (!Hostile(f, caster) || !caster.IsStanding || caster.Action is not { InMotion: false } held || !held.Skill.Interruptible) continue;
            if (held.CastUntilMs <= 0 && held.TelegraphUntilMs <= 0) continue;
            if (Vector2.Distance(f.Position, caster.Position) > Ranges.Long + f.Size + caster.Size) continue;
            var (who, fired) = TakeArmed(f.Side, ReactionTrigger.FoeCast, only: f);
            if (fired is null) return;
            FireReaction(who, fired, caster);
            if (caster.Action == held) Interrupt(caster, fired.Skill.Name);
            return;
        }
    }

    /// <summary>A foe's big move starting: the first of the party's passives that answers one (Somebody Allowed to Say No, once a fight) stops it.</summary>
    private void FoeBigMove(Fighter foe)
    {
        foreach (Fighter f in _fighters)
        {
            if (!Hostile(f, foe) || !f.IsStanding) continue;
            foreach (SkillDefinition passive in f.PassiveSkills)
                foreach (Proc proc in passive.Procs)
                {
                    if (proc.Trigger != ProcTrigger.OnFoeBigMove || !ProcReady(f, passive, proc)) continue;
                    Emit(new BattleEvent { Kind = BattleEventKind.ReactionFired, ActorId = f.Id, TargetId = foe.Id, SkillId = passive.Id, Text = $"{f.Name}: \"No.\" ({passive.Name})" });
                    foreach (Effect effect in proc.Effects)
                    {
                        if (effect.Kind == EffectType.Interrupt) Interrupt(foe, passive.Name);
                        else ApplyLoose(f, passive, effect, effect.On == EffectOn.Self ? f : foe, foe);
                    }
                    return;
                }
        }
    }

    /// <summary>The armed reaction of a skill, or null.</summary>
    private Armed Armed(Fighter f, SkillDefinition skill)
    {
        foreach (Armed armed in f.ArmedList)
            if (armed.Skill == skill && armed.UntilMs > NowMs) return armed;
        return null;
    }

    /// <summary>Lets armed reactions that have waited too long go.</summary>
    private void TickArmed(Fighter f) => f.ArmedList.RemoveAll(armed => armed.UntilMs <= NowMs);

    /// <summary>The first armed reaction anyone on a side has for a trigger (over someone, for Cover), which comes off as it fires; or null.</summary>
    internal (Fighter Who, Armed Armed) TakeArmed(Side side, ReactionTrigger trigger, Fighter over = null, Fighter only = null)
    {
        foreach (Fighter f in _fighters)
        {
            if (f.Side != side || !f.IsStanding || only is not null && f != only) continue;
            for (int i = 0; i < f.ArmedList.Count; i++)
            {
                Armed armed = f.ArmedList[i];
                if (armed.Skill.Trigger != trigger || armed.UntilMs <= NowMs) continue;
                if (over is not null && armed.Over != over) continue;
                f.ArmedList.RemoveAt(i);
                Emit(new BattleEvent
                {
                    Kind = BattleEventKind.ReactionFired, ActorId = f.Id, TargetId = over?.Id, SkillId = armed.Skill.Id, Text = $"{f.Name}'s {armed.Skill.Name}!",
                });
                return (f, armed);
            }
        }
        return (null, null);
    }

    /// <summary>Fires what an armed reaction does when it goes off, at whoever set it off.</summary>
    internal void FireReaction(Fighter who, Armed armed, Fighter attacker)
    {
        bool first = true;
        foreach (Effect effect in armed.Skill.Effects)
        {
            if (effect.Kind == EffectType.Arm) continue;
            Fighter on = effect.On switch
            {
                EffectOn.Attacker => attacker,
                EffectOn.Self => who,
                _ => armed.Over ?? who,
            };
            if (on is null || on.IsDead || on.HasLeft) continue;
            ApplyLoose(who, armed.Skill, effect, on, attacker, first);
            first = false;
        }
    }

    /// <summary>Lets a foe's armed Counterspell stop a cast or telegraph that's just begun, within its range.</summary>
    private void FoeCastReactions(Fighter caster, ActionState action)
    {
        Side other = caster.Side == Side.Party ? Side.Foes : Side.Party;
        foreach (Fighter f in _fighters)
        {
            if (f.Side != other || !f.IsStanding) continue;
            foreach (Armed armed in f.ArmedList)
            {
                if (armed.Skill.Trigger != ReactionTrigger.FoeCast || armed.UntilMs <= NowMs) continue;
                if (Vector2.Distance(f.Position, caster.Position) > Ranges.Long + f.Size + caster.Size) continue;
                var (who, fired) = TakeArmed(other, ReactionTrigger.FoeCast, only: f);
                if (fired is null) continue;
                FireReaction(who, fired, caster);
                if (caster.Action == action) Interrupt(caster, fired.Skill.Name);
                return;
            }
        }
    }

    // Potions.

    /// <summary>Drinks the bag's best healing potion, if there's one, it's ready, and they're hurt.</summary>
    private void DrinkPotion(Fighter f)
    {
        string why = Problem(f, Potion, f);
        if (why is null && f.Hp >= f.MaxHp) why = "Not hurt";
        if (why is not null)
        {
            Deny(f, Potion.Id, why);
            return;
        }
        if (f.Action is not null)
        {
            f.Order = new Order { Kind = CommandKind.Use, Skill = Potion, Target = f, Point = new Vector2(NowMs, 0) };
            return;
        }
        StartAction(f, Potion, f, auto: false);
    }

    /// <summary>The best healing potion in the bag (the one that heals most), or null.</summary>
    internal ItemDefinition BestPotion()
    {
        if (Rules.Bag is not Inventory bag) return null;
        ItemDefinition best = null;
        foreach (ItemStack stack in bag.Stacks)
        {
            if (stack.Count <= 0 || !ItemCatalog.Exists(stack.Id)) continue;
            ItemDefinition item = ItemCatalog.Get(stack.Id);
            if (item.Kind != ItemKind.Consumable || item.Heals.IsNone) continue;
            if (best is null || item.Heals.Average > best.Heals.Average) best = item;
        }
        return best;
    }

    // Procs on a skill being used.

    /// <summary>Runs a fighter's OnSkillUsed procs, and says whether one of them echoes the skill (Echo Strike: not a reaction's arming).</summary>
    private bool UseProcs(Fighter f, SkillDefinition skill)
    {
        bool echo = false;
        foreach (SkillDefinition passive in f.PassiveSkills)
            foreach (Proc proc in passive.Procs)
            {
                if (proc.Trigger != ProcTrigger.OnSkillUsed) continue;
                bool echoes = false;
                foreach (Effect effect in proc.Effects) echoes |= effect.Kind == EffectType.Echo;
                if (echoes && (skill.Kind == SkillKind.Reaction || skill == Potion)) continue;
                if (!ProcReady(f, passive, proc)) continue;
                foreach (Effect effect in proc.Effects)
                {
                    if (effect.Kind == EffectType.Echo) echo = true;
                    else ApplyLoose(f, passive, effect, f.Target ?? f, null);
                }
            }
        return echo;
    }
}
