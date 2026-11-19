using System;
using System.Numerics;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// A piece of the floor, in world pixels: a ring around a point, a line across the room, a cone in
/// front of someone, or the whole room. Telegraphs show it as a ground marker; a blow lands on
/// whoever's in it.
/// </summary>
/// <param name="Shape">What shape it is.</param>
/// <param name="Center">A ring's or a cone's centre (a cone's apex); a line's start.</param>
/// <param name="Radius">A ring's or a cone's reach.</param>
/// <param name="To">A line's end, or the direction a cone faces (a point it faces towards).</param>
/// <param name="Width">A line's width, or a cone's whole angle in degrees.</param>
public sealed record BattleArea(AreaKind Shape, Vector2 Center, float Radius, Vector2 To = default, float Width = 0f)
{
    /// <summary>The whole room.</summary>
    public static BattleArea Room { get; } = new(AreaKind.Room, Vector2.Zero, 0f);

    /// <summary>A ring of a radius around a point.</summary>
    public static BattleArea Ring(Vector2 center, float radius) => new(AreaKind.Ring, center, radius);

    /// <summary>A line from one point to another, so wide.</summary>
    public static BattleArea Line(Vector2 from, Vector2 to, float width) => new(AreaKind.Line, from, 0f, to, width);

    /// <summary>A cone from a point towards another, out to a radius, with a whole angle in degrees.</summary>
    public static BattleArea Cone(Vector2 apex, Vector2 towards, float radius, float degrees) => new(AreaKind.Cone, apex, radius, towards, degrees);

    /// <summary>Whether a point (someone's feet) is in it, allowing for how big they are.</summary>
    /// <param name="point">The point.</param>
    /// <param name="size">How far past the edge still counts: a big foe's bulk.</param>
    public bool Contains(Vector2 point, float size = 0f)
    {
        switch (Shape)
        {
            case AreaKind.Room:
                return true;
            case AreaKind.Ring:
                return Vector2.Distance(point, Center) <= Radius + size;
            case AreaKind.Line:
            {
                Vector2 along = To - Center;
                float length = along.Length();
                if (length < 0.001f) return Vector2.Distance(point, Center) <= Width / 2f + size;
                Vector2 unit = along / length;
                float t = Vector2.Dot(point - Center, unit);
                if (t < -size || t > length + size) return false;
                float off = MathF.Abs(Vector2.Dot(point - Center, new Vector2(-unit.Y, unit.X)));
                return off <= Width / 2f + size;
            }
            case AreaKind.Cone:
            {
                Vector2 offset = point - Center;
                float distance = offset.Length();
                if (distance > Radius + size) return false;
                if (distance < 0.001f) return true;
                Vector2 facing = To - Center;
                if (facing.LengthSquared() < 0.0001f) return true;
                float cos = Vector2.Dot(Vector2.Normalize(offset), Vector2.Normalize(facing));
                return cos >= MathF.Cos(Width / 2f * MathF.PI / 180f) - size / MathF.Max(distance, 1f);
            }
            default:
                return false;
        }
    }

    /// <summary>The nearest point outside it from somewhere inside, a step past its edge, for someone getting out of a marker.</summary>
    /// <param name="from">Where they stand.</param>
    /// <param name="margin">How far past the edge to go.</param>
    public Vector2 WayOut(Vector2 from, float margin)
    {
        switch (Shape)
        {
            case AreaKind.Ring:
            {
                Vector2 away = from - Center;
                if (away.LengthSquared() < 0.01f) away = new Vector2(-1f, 0.3f);
                return Center + Vector2.Normalize(away) * (Radius + margin);
            }
            case AreaKind.Line:
            {
                Vector2 along = To - Center;
                if (along.LengthSquared() < 0.01f) along = Vector2.UnitX;
                Vector2 unit = Vector2.Normalize(along);
                var normal = new Vector2(-unit.Y, unit.X);
                float side = Vector2.Dot(from - Center, normal);
                float sign = side >= 0f ? 1f : -1f;
                return from + normal * sign * (Width / 2f + margin - MathF.Abs(side));
            }
            case AreaKind.Cone:
            {
                Vector2 away = from - Center;
                if (away.LengthSquared() < 0.01f) away = Center - To;
                return Center + Vector2.Normalize(away) * (Radius + margin);
            }
            default:
                return from;
        }
    }
}

/// <summary>What shape a BattleArea is.</summary>
public enum AreaKind
{
    /// <summary>A ring around a point.</summary>
    Ring,

    /// <summary>A line across the floor.</summary>
    Line,

    /// <summary>A cone in front of someone.</summary>
    Cone,

    /// <summary>The whole room.</summary>
    Room,
}
