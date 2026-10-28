using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// One kind of prop: its picture, how much floor it blocks, whether it's on the floor or a wall, and
/// any light it gives off. Chests, doors and levers are props too, with a picture for each way they can be.
/// </summary>
/// <remarks>
/// A prop stands on its foot (the middle of its bottom edge), the same as the people do, so they sort
/// against each other the same way. The footprint is measured up from the foot, like the lobby props
/// in The Black Box.
/// </remarks>
public sealed class PropDefinition
{
    /// <summary>What map files call it.</summary>
    public required string Id { get; init; }

    /// <summary>Its picture, by content name.</summary>
    public required string Texture { get; init; }

    /// <summary>How many pictures are side by side in it. Two for anything that opens, the shut one first.</summary>
    public int Frames { get; init; } = 1;

    /// <summary>How much floor it blocks, width and depth in art pixels, measured up from its foot. Zero to walk through it.</summary>
    public Point Footprint { get; init; }

    /// <summary>How wide a gap there is down the middle of the footprint, in art pixels, to walk through an arch. Zero for none.</summary>
    public int Gap { get; init; }

    /// <summary>Floor, wall or ground.</summary>
    public PropPlacement Placement { get; init; } = PropPlacement.Floor;

    /// <summary>How far up from its foot it sorts against people, in art pixels. Zero for most; the ship sorts from its far rail, so whoever's on its deck is drawn on it.</summary>
    public int SortRise { get; init; }

    /// <summary>The light it gives off, or null if it doesn't.</summary>
    public PropLight Light { get; init; }
}
