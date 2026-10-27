namespace LegendQuest.Collisions;

/// <summary>
/// An axis-aligned bounding rectangle, done the way the collision tutorial does it.
/// </summary>
/// <remarks>Feet, walls and prop footprints are all one of these. Copied from The Black Box.</remarks>
public struct BoundingRectangle
{
    /// <summary>The left edge, in world pixels.</summary>
    public float X;

    /// <summary>The top edge, in world pixels.</summary>
    public float Y;

    /// <summary>How wide it is, in world pixels.</summary>
    public float Width;

    /// <summary>How tall it is, in world pixels.</summary>
    public float Height;

    /// <summary>The left edge.</summary>
    public float Left => X;

    /// <summary>The right edge.</summary>
    public float Right => X + Width;

    /// <summary>The top edge.</summary>
    public float Top => Y;

    /// <summary>The bottom edge.</summary>
    public float Bottom => Y + Height;

    /// <summary>Makes a rectangle from its top-left corner and its size.</summary>
    /// <param name="x">The left edge, in world pixels.</param>
    /// <param name="y">The top edge, in world pixels.</param>
    /// <param name="width">How wide, in world pixels.</param>
    /// <param name="height">How tall, in world pixels.</param>
    public BoundingRectangle(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>Whether this rectangle overlaps another.</summary>
    /// <param name="other">The other rectangle.</param>
    /// <returns>True if they touch or overlap.</returns>
    public bool CollidesWith(BoundingRectangle other) => CollisionHelper.Collides(this, other);
}
