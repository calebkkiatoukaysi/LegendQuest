using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Combat;

/// <summary>
/// Distances on a fight's grid, in tiles and in metres.
/// </summary>
/// <remarks>
/// A tile is 1.5 m, 5e's five-foot square, and a diagonal step costs the same as a straight one (5e's
/// simple rule for a grid), so the distance between two tiles is the larger of the two differences. Ranges in the catalogs are in metres and get turned into
/// tiles here, so a 1.5 m melee reach is the eight tiles round you.
/// </remarks>
public static class Measure
{
    /// <summary>How many metres one tile is.</summary>
    public const float TileMetres = 1.5f;

    /// <summary>A melee weapon's reach: the tiles touching yours, diagonals included.</summary>
    public const float MeleeReach = TileMetres;

    /// <summary>How many steps apart two tiles are, a diagonal counting as one.</summary>
    /// <param name="a">One tile.</param>
    /// <param name="b">The other.</param>
    public static int Steps(Point a, Point b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

    /// <summary>How many whole tiles a distance in metres reaches.</summary>
    /// <remarks>Rounded down, with a hair of slack so 1.5 m is one tile and not a rounding error short of it.</remarks>
    /// <param name="metres">The distance.</param>
    public static int Tiles(float metres) => (int)MathF.Floor(metres / TileMetres + 0.001f);

    /// <summary>Whether one tile is within a distance of another.</summary>
    /// <param name="from">Where from.</param>
    /// <param name="to">Where to.</param>
    /// <param name="metres">How far is close enough.</param>
    public static bool Within(Point from, Point to, float metres) => Steps(from, to) <= Tiles(metres);
}
