namespace LegendQuest.World;

/// <summary>
/// One kind of tile in a tileset: what it's called, how many ways it's drawn, whether you can walk
/// through it or see past it, and what it sounds like underfoot.
/// </summary>
/// <remarks>
/// Variants are drawn one after another on the sheet, and the map picks one per cell from where the
/// cell is, so a field of grass doesn't repeat. That's how The Black Box's lobby floor worked. A
/// tile can also be a block of several tiles each way, drawn as one picture: then each cell shows
/// the part of the block for where it is on the map, and the variant is picked per block.
/// </remarks>
public sealed class TileDefinition
{
    /// <summary>What it's called. Map files name tiles by this in their [legend].</summary>
    public required string Name { get; init; }

    /// <summary>How many versions of it are on the sheet, side by side.</summary>
    public int Variants { get; init; } = 1;

    /// <summary>How many tiles across (and down) one picture of it is. 1 for an ordinary tile.</summary>
    /// <remarks>Questeria's ground is blocks of 3, because the paving and the grass were painted three tiles across and cutting them smaller lost the pattern.</remarks>
    public int Block { get; init; } = 1;

    /// <summary>Whether it stops people walking. Collision is built from this.</summary>
    public bool Solid { get; init; }

    /// <summary>Whether it stops light and sight: the dark map's pools and what the map screen has seen.</summary>
    /// <remarks>Walls are. A fence, a heap of rubble or a tree trunk is solid but isn't, since you can see over or round it.</remarks>
    public bool Opaque { get; init; }

    /// <summary>Whether it has edges: four more frames after its variants, for along its north, east, south and west sides wherever the ground next to it is something else.</summary>
    /// <remarks>Tilemap works out where they go, so a lawn gets its kerb and the water its quay wall wherever a map puts them, with no tiles for every corner.</remarks>
    public bool Edged { get; init; }

    /// <summary>What it sounds like to walk on.</summary>
    public Surface Surface { get; init; } = Surface.Stone;
}
