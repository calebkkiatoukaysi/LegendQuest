namespace LegendQuest;

/// <summary>
/// What sort of place a room is, which decides how it's drawn. Zone files say which with kind =.
/// </summary>
public enum RoomKind
{
    /// <summary>Outdoors, or as good as: a sky, a far layer and parallax behind the buildings.</summary>
    Street,

    /// <summary>Indoors: no sky, no far layer, no parallax, and the tile strip is the back wall as well as the floor.</summary>
    Interior,
}
