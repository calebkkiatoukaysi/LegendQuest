namespace LegendQuest.World;

/// <summary>
/// One kind of tile in a tileset: what it's called, how many ways it's drawn, whether you can walk
/// through it, and what it sounds like underfoot.
/// </summary>
/// <remarks>
/// Variants are drawn one after another on the sheet, and the map picks one per cell from where the
/// cell is, so a field of grass doesn't repeat. That's how The Black Box's lobby floor worked.
/// </remarks>
public sealed class TileDefinition
{
    /// <summary>What it's called. Map files name tiles by this in their [legend].</summary>
    public required string Name { get; init; }

    /// <summary>How many versions of it are on the sheet, side by side.</summary>
    public int Variants { get; init; } = 1;

    /// <summary>Whether it stops people walking. Collision is built from this.</summary>
    public bool Solid { get; init; }

    /// <summary>What it sounds like to walk on.</summary>
    public Surface Surface { get; init; } = Surface.Stone;
}
