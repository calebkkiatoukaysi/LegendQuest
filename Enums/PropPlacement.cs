namespace LegendQuest;

/// <summary>
/// Whether a prop stands on the floor (and sorts with the people), hangs on a wall (behind
/// everyone), or lies flat on the ground (under everyone).
/// </summary>
public enum PropPlacement
{
    /// <summary>On the floor. Sorted by how far down the map its foot is.</summary>
    Floor,

    /// <summary>On a wall, like a torch in its sconce. Always behind anyone on the floor.</summary>
    Wall,

    /// <summary>Lying on the ground, like a patch of flowers. Under anyone standing on it, and nothing stops you walking over it.</summary>
    Ground,
}
