using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Particles;

/// <summary>
/// Random numbers for the particles. The tutorial's helper, cut down to what the particles use.
/// </summary>
/// <remarks>It has its own Random so the particles never take a number the rules would have used. The rules get a seeded one of their own.</remarks>
public static class RandomHelper
{
    private static readonly Random Generator = new();

    /// <summary>A whole number from min up to but not including max.</summary>
    public static int Next(int minValue, int maxValue) => Generator.Next(minValue, maxValue);

    /// <summary>A float between min and max.</summary>
    public static float NextFloat(float minValue, float maxValue) =>
        minValue + (float)Generator.NextDouble() * (maxValue - minValue);

    /// <summary>A point somewhere inside a rectangle.</summary>
    public static Vector2 RandomPosition(Rectangle bounds) => new(
        NextFloat(bounds.Left, bounds.Right),
        NextFloat(bounds.Top, bounds.Bottom));
}
