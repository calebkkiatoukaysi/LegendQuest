using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// An ellipse on a room's floor that feet can't go into: a fountain, a well, a hole.
/// </summary>
/// <param name="Centre">Its middle, in world pixels.</param>
/// <param name="Radii">How far it reaches across and in depth from its middle, in world pixels.</param>
public sealed record RoomBlock(Vector2 Centre, Vector2 Radii)
{
    /// <summary>Whether a point's inside it.</summary>
    /// <param name="point">The point, in world pixels.</param>
    public bool Contains(Vector2 point)
    {
        float x = (point.X - Centre.X) / Radii.X, y = (point.Y - Centre.Y) / Radii.Y;
        return x * x + y * y < 1f;
    }
}
