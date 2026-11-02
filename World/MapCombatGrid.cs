using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;
using LegendQuest.Combat;

namespace LegendQuest.World;

/// <summary>
/// The map a fight's on, as the fight sees it: which tiles can be stood on and which stop a line of
/// sight.
/// </summary>
/// <remarks>
/// The ICombatGrid the explore screen hands CombatEngine. It's a NavGrid of its own, built from the
/// same things the pathfinder's is (the solid tiles, the props, the shut doors) and whoever's
/// standing about who isn't in the fight, but not from anyone who is: where the fighters stand is
/// the engine's business, and it moves them. Sight is the map's own: an opaque tile or a shut door
/// blocks it, a crate doesn't. Built once when the fight starts, since nothing on the map moves but
/// the fighters while it's on.
/// </remarks>
public sealed class MapCombatGrid : ICombatGrid
{
    private readonly NavGrid _nav;
    private readonly Tilemap _tiles;

    /// <summary>Builds the ground for a fight.</summary>
    /// <param name="tiles">The map.</param>
    /// <param name="footprints">Everything standing on it that isn't in the fight and can't be walked through.</param>
    public MapCombatGrid(Tilemap tiles, IEnumerable<BoundingRectangle> footprints)
    {
        _tiles = tiles;
        _nav = new NavGrid(tiles);
        _nav.Build(footprints);
    }

    /// <summary>How many tiles across.</summary>
    public int Width => _nav.Width;

    /// <summary>How many tiles down.</summary>
    public int Height => _nav.Height;

    /// <summary>Whether a tile can be stood on: not solid, not under a prop or a shut door, nobody standing about on it, and on the map.</summary>
    /// <param name="cell">The tile.</param>
    public bool Walkable(Point cell) => _nav.IsWalkable(cell);

    /// <summary>Whether a tile stops a line of sight: an opaque tile or a shut door. Off the map does.</summary>
    /// <param name="cell">The tile.</param>
    public bool BlocksSight(Point cell) => _tiles.BlocksSight(cell.X, cell.Y);
}
