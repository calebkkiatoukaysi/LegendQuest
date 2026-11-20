using System;
using System.Numerics;

namespace LegendQuest.Combat.Realtime;

/// <summary>The reaches of 11-combat.md §2.2, in world pixels between two fighters' feet.</summary>
public static class Ranges
{
    /// <summary>Swords, axes, fists, claws, bites.</summary>
    public const float Melee = 30f;

    /// <summary>Spears, polearms, whips.</summary>
    public const float Reach = 42f;

    /// <summary>Thrown things, short spells.</summary>
    public const float Short = 100f;

    /// <summary>Bows, crossbows, most spells.</summary>
    public const float Long = 220f;

    /// <summary>Anywhere in the room.</summary>
    public const float Room = 100000f;

    /// <summary>A reach's pixels.</summary>
    /// <param name="range">The reach.</param>
    public static float Of(SkillRange range) => range switch
    {
        SkillRange.Melee => Melee,
        SkillRange.Reach => Reach,
        SkillRange.Short => Short,
        SkillRange.Long => Long,
        SkillRange.Room => Room,
        SkillRange.Self => 0f,
        _ => Melee,
    };

    /// <summary>A weapon's reach: melee, reach, or long for a ranged weapon.</summary>
    /// <param name="reach">The weapon's reach, from its properties or its stat block.</param>
    public static float Of(LegendQuest.Reach reach) => reach switch
    {
        LegendQuest.Reach.Reach => Reach,
        LegendQuest.Reach.Ranged or LegendQuest.Reach.AnyRow => Long,
        _ => Melee,
    };
}

/// <summary>Walking, reach, facing, and keeping fighters from standing in one another.</summary>
public sealed partial class Battle
{
    /// <summary>
    /// How close two fighters' feet may come before they're nudged apart: an ellipse, wide across the
    /// room and shallower down it. A figure is about 30 to 40 pixels across the shoulders, so two at
    /// the same depth need most of that between them; two a lane apart in depth draw one clearly behind
    /// the other. A big creature's size widens it (its whole size across, half of it down).
    /// </summary>
    private const float PersonalAcross = 26f, PersonalDown = 20f;

    /// <summary>How much wider personal space is across between two on the same side, who never need to be in reach of each other.</summary>
    private const float AllyRoom = 6f;

    /// <summary>The room a spot round a target wants clear of everyone else: more than personal space, so a crowd takes free spots before it's pushed into them.</summary>
    private const float SpotAcross = 34f, SpotDown = 24f;

    /// <summary>How far apart in depth the lanes round a target are, before its size is added (60% of it): a little more than personal space down, so two lanes don't push each other.</summary>
    private const float Lane = 22f;

    /// <summary>
    /// What a step in depth counts for in reach, against a step across: half. On a side-on floor a blow
    /// reaches someone a lane in front or behind as easily as someone level with you a little further
    /// off, so attackers in the lanes round a target can stand nearly a full reach across from it rather
    /// than crowd in on it.
    /// </summary>
    private const float DepthReach = 0.5f;

    /// <summary>The share of a reach someone stands at round their target, so they're well inside it and a nudge doesn't put them out.</summary>
    private const float StandShare = 0.85f;

    /// <summary>The least share of their stand-off a melee fighter stands across from their target, in whichever lane: any nearer and they're over or under it.</summary>
    private const float Beside = 0.6f;

    /// <summary>What going round to a target's far side costs a spot, in the walk's pixels' worth: a little, so a second attacker flanks rather than queue behind the first.</summary>
    private const float FarSide = 10f;

    /// <summary>What standing in a foe's filling ground marker costs a spot: more than any walk or crowd, so nobody picks one.</summary>
    private const float InTheMarker = 400f;

    /// <summary>How often someone already in reach looks for a freer spot if they're crowded, and how crowded (Crowding's score) counts.</summary>
    private const int ResettleMs = 1000;
    private const float Crowded = 12f;

    /// <summary>How close to a goal counts as there.</summary>
    private const float Arrived = 1.5f;

    /// <summary>How far a skill reaches for someone: the weapon's for a weapon skill, otherwise the skill's own.</summary>
    internal float RangeOf(Fighter f, SkillDefinition skill)
    {
        if (skill.Range == SkillRange.Weapon) return Ranges.Of(f.MainAttack.Reach);
        return Ranges.Of(skill.Range);
    }

    /// <summary>Whether a skill reaches its target from where someone stands.</summary>
    public bool InReach(Fighter f, SkillDefinition skill, Fighter target)
    {
        if (skill.Target is SkillTarget.Self or SkillTarget.None || target is null || target == f || skill.Range == SkillRange.Room) return true;
        return Reaching(f.Position, target.Position) <= RangeOf(f, skill) + f.Size + target.Size;
    }

    /// <summary>How far apart two points are for reach: across the room in full, down it at DepthReach.</summary>
    internal static float Reaching(Vector2 a, Vector2 b)
    {
        float across = b.X - a.X, down = (b.Y - a.Y) * DepthReach;
        return MathF.Sqrt(across * across + down * down);
    }

    /// <summary>Walks someone towards a target until they're within a reach of it, to a free spot round it (SpotBy).</summary>
    internal void Approach(Fighter f, Fighter target, float reach)
    {
        if (target is null || target == f) return;
        SetGoal(f, SpotBy(f, target, reach));
    }

    /// <summary>
    /// Where someone should stand to reach a target, the way a side-on fight spreads out: in melee, on
    /// either side of the target in staggered lanes of depth (straight across from it, then a lane in
    /// front and one behind, a little nearer across so they're still in reach, which counts depth at
    /// half: Reaching); from range, back on their own side at about four fifths of their reach, in lanes
    /// and at three distances. Each spot is scored by the walk to it, a small price for going round to
    /// the target's far side, how far it eats into the room of anyone else standing there or already
    /// walking somewhere (Crowding), and a great price for standing in a foe's filling ground marker,
    /// and the cheapest wins. The spot they're already making for gets a little off, so they don't
    /// dither between two, and with <paramref name="here"/> where they stand is a spot too, free to stay
    /// at. So the first attacker stands straight across from its target, the second across from it on
    /// the far side, and the rest in the lanes in front and behind. It's all arithmetic on where
    /// everyone is, so it replays from the seed. A big target's spots stand further out and its lanes
    /// further apart.
    /// </summary>
    private Vector2 SpotBy(Fighter f, Fighter target, float reach, bool here = false)
    {
        Vector2 from = target.Position;
        float across = f.Position.X - from.X;
        int ownSide = MathF.Abs(across) > 0.5f ? MathF.Sign(across) : (f.Side == Side.Party ? -1 : 1);
        float lane = Lane + target.Size * 0.6f;
        float stand = StandOff(f, target, reach, out bool melee);
        float beside = melee ? stand * Beside : 0f;

        Vector2 best = from + new Vector2(ownSide * stand, 0f);
        float bestCost = float.MaxValue;
        if (here)
        {
            best = f.Position;
            bestCost = Crowding(f, target, f.Position, beside) + (DangerAt(f, f.Position) is not null ? InTheMarker : 0f);
        }
        void Consider(Vector2 spot, bool farSide)
        {
            Vector2 kept = Arena.Clamp(spot);
            float cost = Vector2.Distance(f.Position, kept) * 0.35f + Vector2.Distance(spot, kept) * 3f + (farSide ? FarSide : 0f);
            if (f.Goal is Vector2 goal && Vector2.Distance(goal, kept) < 6f) cost -= 8f;
            cost += Crowding(f, target, kept, beside);
            if (DangerAt(f, kept) is not null) cost += InTheMarker;
            if (cost < bestCost)
            {
                bestCost = cost;
                best = kept;
            }
        }

        if (melee)
        {
            foreach (int side in new[] { ownSide, -ownSide })
                foreach (float down in new[] { 0f, lane, -lane })
                {
                    float reached = down * DepthReach;
                    float along = MathF.Sqrt(MathF.Max(0f, stand * stand - reached * reached));
                    if (along < beside) continue;
                    Consider(from + new Vector2(side * along, down), side != ownSide);
                }
        }
        else
        {
            float own = Math.Clamp(f.Position.Y - from.Y, -stand * 0.5f, stand * 0.5f);
            foreach (float share in new[] { 1f, 0.85f, 1.12f })
                foreach (float down in new[] { own, own + lane, own - lane, own + lane * 2f, own - lane * 2f })
                {
                    float distance = stand * share, reached = down * DepthReach;
                    float along = MathF.Sqrt(MathF.Max(0f, distance * distance - reached * reached));
                    Consider(from + new Vector2(ownSide * along, down), false);
                }
        }
        return best;
    }

    /// <summary>How far from a target someone stands to reach it: a little inside their reach, past both their bodies in melee; and whether it's melee (a reach weapon's or less).</summary>
    private static float StandOff(Fighter f, Fighter target, float reach, out bool melee)
    {
        melee = reach <= Ranges.Reach + 1f;
        return MathF.Max(6f, MathF.Min(reach, 220f) * (melee ? StandShare : 0.8f)) + target.Size + (melee ? f.Size : 0f);
    }

    /// <summary>
    /// How crowded a spot is: how far it eats into the room (SpotAcross by SpotDown, wider for big
    /// creatures) of everyone standing but the one choosing it and their target, each where they're
    /// walking to or else where they stand, 0 when it's clear and up to 60 for each one it's right on;
    /// and, in melee, up to 40 more for standing nearer than <paramref name="beside"/> across from the
    /// target, which is over or under it rather than beside it, where the two draw as one.
    /// </summary>
    private float Crowding(Fighter f, Fighter target, Vector2 spot, float beside)
    {
        float cost = 0f;
        float off = MathF.Abs(spot.X - target.Position.X);
        if (off < beside) cost += 40f * (1f - off / beside);
        foreach (Fighter other in _fighters)
        {
            if (other == f || other == target || !other.IsStanding) continue;
            Vector2 at = other.Goal ?? other.Position;
            float sizes = f.Size + other.Size;
            float x = (spot.X - at.X) / (SpotAcross + sizes), y = (spot.Y - at.Y) / (SpotDown + sizes * 0.5f);
            float inside = x * x + y * y;
            if (inside < 1f) cost += 60f * (1f - inside);
        }
        return cost;
    }

    /// <summary>Now and then, moves someone who's in reach of their target but crowded to a freer spot round it, if there's one worth the walk (SpotBy, staying where they are being one of the choices).</summary>
    private void Resettle(Fighter f)
    {
        if (NowMs < f.ResettleAtMs) return;
        f.ResettleAtMs = NowMs + ResettleMs;
        Fighter target = f.Target;
        if (!f.AutoAttackOn || !f.Chase || f.Order is not null || f.Goal is not null || !CanMove(f)) return;
        if (target is null || !Hostile(f, target) || !target.IsStanding || !InReach(f, f.Auto, target) || Danger(f) is not null) return;
        float reach = RangeOf(f, f.Auto);
        float stand = StandOff(f, target, reach, out bool melee);
        if (Crowding(f, target, f.Position, melee ? stand * Beside : 0f) < Crowded) return;
        Vector2 spot = SpotBy(f, target, reach, here: true);
        if (Vector2.Distance(spot, f.Position) > 3f) SetGoal(f, spot);
    }

    /// <summary>Sets where someone's walking, and says so when it's somewhere new.</summary>
    internal void SetGoal(Fighter f, Vector2 goal)
    {
        goal = Arena.Clamp(goal);
        if (f.Goal is Vector2 old && Vector2.Distance(old, goal) < 4f) return;
        if (Vector2.Distance(f.Position, goal) < Arrived)
        {
            f.Goal = null;
            return;
        }
        f.Goal = goal;
        float speed = SpeedOf(f);
        Emit(new BattleEvent
        {
            Kind = BattleEventKind.Moved, ActorId = f.Id, Position = goal,
            DurationMs = speed <= 0f ? 0 : (int)(Vector2.Distance(f.Position, goal) / speed * 1000f),
        });
    }

    /// <summary>How fast someone walks now, in world pixels a second: their speed, a quarter more or less for what's on them, half if Slowed.</summary>
    internal float SpeedOf(Fighter f)
    {
        double pct = Sum(f, Stat.SpeedPct, default);
        float speed = f.WalkSpeed * (float)Math.Max(0.2, 1.0 + pct / 100.0);
        if (Has(f, ConditionId.Slowed)) speed *= 0.5f;
        return speed;
    }

    /// <summary>Takes a step towards their goal, if they can move.</summary>
    private void Walk(Fighter f)
    {
        if (f.Goal is not Vector2 goal) return;
        if (!CanMove(f)) return;
        Vector2 to = goal - f.Position;
        float distance = to.Length();
        float step = SpeedOf(f) * TickMs / 1000f;
        if (distance <= step || distance < Arrived)
        {
            f.Position = goal;
            f.Goal = null;
        }
        else f.Position = Arena.Clamp(f.Position + Steer(f, to / distance, distance) * step);
        if (MathF.Abs(to.X) > 0.5f && f.Action is null) f.Facing = to.X > 0 ? 1 : -1;
    }

    /// <summary>
    /// The way someone walks towards their goal, bent down or up the room round anyone standing in the
    /// way (whose personal space the straight line runs through, within the next 40 pixels and short of
    /// the goal), so going round to a target's far side walks round the target rather than into it. They
    /// go round on the side they're already off the one in the way, or towards the goal's depth, or
    /// towards the middle of the floor.
    /// </summary>
    private Vector2 Steer(Fighter f, Vector2 heading, float distance)
    {
        if (distance < 8f) return heading;
        Vector2 goal = f.Goal ?? f.Position;
        float bend = 0f;
        foreach (Fighter other in _fighters)
        {
            if (other == f || !other.IsStanding) continue;
            Vector2 apart = other.Position - f.Position;
            float ahead = Vector2.Dot(apart, heading);
            if (ahead <= 0f || ahead > MathF.Min(40f, distance - 4f)) continue;
            Vector2 off = f.Position + heading * ahead - other.Position;
            float sizes = f.Size + other.Size;
            float x = off.X / (PersonalAcross + sizes), y = off.Y / (PersonalDown + sizes * 0.5f);
            float inside = x * x + y * y;
            if (inside >= 1f) continue;
            float way = MathF.Abs(off.Y) > 0.5f ? MathF.Sign(off.Y)
                : MathF.Abs(goal.Y - other.Position.Y) > 0.5f ? MathF.Sign(goal.Y - other.Position.Y)
                : other.Position.Y <= Arena.Middle.Y ? 1f : -1f;
            bend += way * (1f - inside);
        }
        if (bend == 0f) return heading;
        return Vector2.Normalize(heading + new Vector2(0f, Math.Clamp(bend, -1f, 1f) * 1.2f));
    }

    /// <summary>Turns someone to face a point.</summary>
    internal static void Face(Fighter f, Vector2 point)
    {
        float dx = point.X - f.Position.X;
        if (MathF.Abs(dx) > 0.5f) f.Facing = dx > 0 ? 1 : -1;
    }

    /// <summary>
    /// Nudges apart anyone standing in someone else's personal space (the ellipse PersonalAcross by
    /// PersonalDown, wider for a big creature), half each, in a fixed order, straight out along the line
    /// between them to the ellipse's edge: two side by side are pushed across the room, two one behind
    /// the other only down it, which is the less it takes.
    /// </summary>
    private void Separate()
    {
        for (int i = 0; i < _fighters.Count; i++)
        {
            Fighter a = _fighters[i];
            if (!a.IsStanding) continue;
            for (int j = i + 1; j < _fighters.Count; j++)
            {
                Fighter b = _fighters[j];
                if (!b.IsStanding) continue;
                float sizes = a.Size + b.Size;
                float wide = PersonalAcross + (a.Side == b.Side ? AllyRoom : 0f) + sizes, deep = PersonalDown + sizes * 0.5f;
                Vector2 apart = b.Position - a.Position;
                float x = apart.X / wide, y = apart.Y / deep;
                float inside = MathF.Sqrt(x * x + y * y);
                if (inside >= 1f) continue;
                Vector2 push = inside < 0.01f
                    ? new Vector2(0f, (j % 2 == 0 ? 1f : -1f) * deep / 2f)
                    : apart * ((1f / inside - 1f) / 2f);
                bool aFixed = !CanMove(a) || a.Size > b.Size;
                bool bFixed = !CanMove(b) || b.Size > a.Size;
                if (aFixed && bFixed) continue;
                if (aFixed) b.Position = Arena.Clamp(b.Position + push * 2f);
                else if (bFixed) a.Position = Arena.Clamp(a.Position - push * 2f);
                else
                {
                    a.Position = Arena.Clamp(a.Position - push);
                    b.Position = Arena.Clamp(b.Position + push);
                }
            }
        }
    }

    /// <summary>Moves someone to a point at once (a knockback, a pull, a dash), kept on the floor, and says how.</summary>
    internal void Shove(Fighter f, Vector2 to, BattleEventKind kind, Fighter by)
    {
        f.Position = Arena.Clamp(to);
        f.Goal = null;
        Emit(new BattleEvent { Kind = kind, ActorId = by?.Id, TargetId = f.Id, Position = f.Position });
        if (kind != BattleEventKind.Moved && f.Action is { InMotion: false }) Interrupt(f, kind == BattleEventKind.Pulled ? "pulled" : "knocked back");
    }
}
