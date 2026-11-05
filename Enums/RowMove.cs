namespace LegendQuest;

/// <summary>
/// Whether an ability moves its target between its rows when it lands.
/// </summary>
/// <remarks>
/// The target goes to the nearest free slot in the other row, nearest lane first; if that row is
/// full, it swaps with whoever's in its lane. Big foes can't be moved. New ones go on the end.
/// </remarks>
public enum RowMove
{
    /// <summary>Leaves the target where it is.</summary>
    None,

    /// <summary>Drags a back-row target into its front row (Thorn Whip).</summary>
    Pull,

    /// <summary>Knocks a front-row target into its back row (Force Bolt).</summary>
    Push,
}
