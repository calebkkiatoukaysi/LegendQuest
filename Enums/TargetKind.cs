namespace LegendQuest;

/// <summary>
/// What an ability is pointed at when it's used.
/// </summary>
public enum TargetKind
{
    /// <summary>Whoever's using it.</summary>
    Self,

    /// <summary>Someone on the user's side.</summary>
    Ally,

    /// <summary>Someone on the other side.</summary>
    Enemy,

    /// <summary>A tile, with nobody needing to be in it.</summary>
    Point,

    /// <summary>Everyone within its radius of a tile, friend or foe.</summary>
    Area,
}
