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
    /// <summary>How close two fighters' feet may come before they're nudged apart.</summary>
    private const float Personal = 13f;

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
        return Vector2.Distance(f.Position, target.Position) <= RangeOf(f, skill) + f.Size + target.Size;
    }

    /// <summary>Walks someone towards a target until they're within a reach of it, to a spot on their own side of it, spread a little so a crowd doesn't stack.</summary>
    internal void Approach(Fighter f, Fighter target, float reach)
    {
        if (target is null || target == f) return;
        Vector2 from = target.Position;
        Vector2 away = f.Position - from;
        if (away.LengthSquared() < 0.01f) away = new Vector2(-target.Facing, 0f);
        away = Vector2.Normalize(away);
        float keep = MathF.Max(6f, MathF.Min(reach, 220f) * 0.8f + target.Size);
        Vector2 spot = from + away * keep;
        int crowd = 0;
        foreach (Fighter other in _fighters)
            if (other != f && other.IsStanding && other.Side == f.Side && Vector2.Distance(other.Position, spot) < Personal) crowd++;
        if (crowd > 0) spot += new Vector2(0f, (f.Index % 2 == 0 ? 1f : -1f) * Personal * crowd);
        SetGoal(f, Arena.Clamp(spot));
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
        else f.Position += to / distance * step;
        if (MathF.Abs(to.X) > 0.5f && f.Action is null) f.Facing = to.X > 0 ? 1 : -1;
    }

    /// <summary>Turns someone to face a point.</summary>
    internal static void Face(Fighter f, Vector2 point)
    {
        float dx = point.X - f.Position.X;
        if (MathF.Abs(dx) > 0.5f) f.Facing = dx > 0 ? 1 : -1;
    }

    /// <summary>Nudges apart anyone standing in someone else, half each, in a fixed order.</summary>
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
                float need = Personal + (a.Size + b.Size) * 0.6f;
                Vector2 apart = b.Position - a.Position;
                float distance = apart.Length();
                if (distance >= need) continue;
                Vector2 unit = distance < 0.01f ? new Vector2(0f, j % 2 == 0 ? 1f : -1f) : apart / distance;
                float push = (need - distance) / 2f;
                bool aFixed = !CanMove(a) || a.Size > b.Size;
                bool bFixed = !CanMove(b) || b.Size > a.Size;
                if (aFixed && bFixed) continue;
                if (aFixed) b.Position = Arena.Clamp(b.Position + unit * push * 2f);
                else if (bFixed) a.Position = Arena.Clamp(a.Position - unit * push * 2f);
                else
                {
                    a.Position = Arena.Clamp(a.Position - unit * push);
                    b.Position = Arena.Clamp(b.Position + unit * push);
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
