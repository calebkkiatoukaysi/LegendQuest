namespace LegendQuest.Collisions;

/// <summary>
/// The collision tests, in one place so every shape agrees. Same layout as the tutorial.
/// </summary>
/// <remarks>Only rectangles so far. The circle comes back from The Black Box when something round needs it.</remarks>
public static class CollisionHelper
{
    /// <summary>Whether two rectangles overlap. They miss if one is entirely to one side of the other.</summary>
    /// <param name="a">One rectangle.</param>
    /// <param name="b">The other.</param>
    /// <returns>True if they touch or overlap.</returns>
    public static bool Collides(BoundingRectangle a, BoundingRectangle b)
    {
        return !(a.Right < b.Left
            || a.Left > b.Right
            || a.Bottom < b.Top
            || a.Top > b.Bottom);
    }
}
