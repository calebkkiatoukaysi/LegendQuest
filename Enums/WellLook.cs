namespace LegendQuest;

/// <summary>
/// How the well an item sits in is drawn.
/// </summary>
/// <remarks>The order is the order of the first three cells of Content/Items/slots.png.</remarks>
public enum WellLook
{
    /// <summary>As it is.</summary>
    Plain,

    /// <summary>Under the mouse, or where the keys are: a bronze edge.</summary>
    Hover,

    /// <summary>Picked out in gold: the slot the item you're on would go into.</summary>
    Picked,
}
