using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// Every prop a map can place, by the id map files use.
/// </summary>
/// <remarks>
/// Each one's picture is Content/Props/(id).png: drawn by tools/generate_world.py, or for Questeria,
/// cut from my pictures by tools/cut_supplied.py.
/// </remarks>
public static class PropCatalog
{
    /// <summary>How far a sconce's torch light reaches. Four tiles, so a hall wants one every eight or so.</summary>
    private const float TorchReach = 64f;

    /// <summary>How far a campfire's light reaches. Further than a torch, and it's out in the open.</summary>
    private const float FireReach = 88f;

    private static readonly PropDefinition[] Table =
    {
        new() { Id = "barrel", Texture = "Props/barrel", Footprint = new Point(12, 6) },
        new() { Id = "crate", Texture = "Props/crate", Footprint = new Point(14, 7) },
        new() { Id = "urn", Texture = "Props/urn", Footprint = new Point(8, 4) },
        new() { Id = "sarcophagus", Texture = "Props/sarcophagus", Footprint = new Point(30, 14) },
        new() { Id = "bedroll", Texture = "Props/bedroll" },
        new() { Id = "campfire", Texture = "Props/campfire", Footprint = new Point(16, 6),
            Light = new PropLight(new Vector2(0f, -4f), new Vector2(0f, -2f), FireReach, 1f) },

        // The ones that are something you can use. Each has its shut (or up, or asleep) picture first.
        new() { Id = "chest", Texture = "Props/chest", Frames = 2, Footprint = new Point(14, 6) },
        new() { Id = "door", Texture = "Props/door", Frames = 2, Footprint = new Point(32, 6) },
        new() { Id = "gate", Texture = "Props/gate", Frames = 2, Footprint = new Point(32, 6) },
        new() { Id = "lever", Texture = "Props/lever", Frames = 2, Placement = PropPlacement.Wall },
        new() { Id = "sign", Texture = "Props/sign", Footprint = new Point(4, 3) },
        new() { Id = "waypoint", Texture = "Props/waypoint", Frames = 2, Footprint = new Point(12, 5) },

        // The flame sits on the top of the handle, 14 art pixels up from where the sconce is fixed. The
        // sconce goes on the lower row of a wall's face, so the floor starts half a tile below it.
        new()
        {
            Id = "sconce", Texture = "Props/sconce", Placement = PropPlacement.Wall,
            Light = new PropLight(new Vector2(0f, -14f), new Vector2(0f, 14f), TorchReach, 1f),
        },
    };

    /// <summary>Questeria's props and the harbour's. The buildings block most of their width and a few
    /// tiles deep, so you can walk round them but not into them, and going behind one, its roof hides
    /// you. The gate and the flower arch have a gap down the middle to walk through. The ship sorts
    /// from its far rail rather than the water, so whoever's stood on its deck is drawn on it.</summary>
    private static readonly PropDefinition[] Questeria =
    {
        new() { Id = "palace", Texture = "Props/palace", Footprint = new Point(280, 112) },
        new() { Id = "temple", Texture = "Props/temple", Footprint = new Point(166, 100) },
        new() { Id = "guildhall", Texture = "Props/guildhall", Footprint = new Point(210, 100) },
        new() { Id = "city-gate", Texture = "Props/city-gate", Footprint = new Point(190, 34), Gap = 56 },
        new() { Id = "inn", Texture = "Props/inn", Footprint = new Point(176, 84) },
        new() { Id = "house-ivy", Texture = "Props/house-ivy", Footprint = new Point(124, 78) },
        new() { Id = "house-shop", Texture = "Props/house-shop", Footprint = new Point(162, 78) },
        new() { Id = "house-awning", Texture = "Props/house-awning", Footprint = new Point(140, 78) },
        new() { Id = "house-rose", Texture = "Props/house-rose", Footprint = new Point(132, 78) },
        new() { Id = "garden-wall", Texture = "Props/garden-wall", Footprint = new Point(84, 12) },
        new() { Id = "fountain", Texture = "Props/fountain", Footprint = new Point(118, 56) },
        new() { Id = "statue", Texture = "Props/statue", Footprint = new Point(34, 16) },
        new() { Id = "crystal-pillar", Texture = "Props/crystal-pillar", Footprint = new Point(14, 8) },
        new() { Id = "lamp", Texture = "Props/lamp", Footprint = new Point(7, 4) },
        new() { Id = "banner-pink", Texture = "Props/banner-pink", Footprint = new Point(32, 4) },
        new() { Id = "banner-blue", Texture = "Props/banner-blue", Footprint = new Point(32, 4) },
        new() { Id = "bench", Texture = "Props/bench", Footprint = new Point(42, 8) },
        new() { Id = "table", Texture = "Props/table", Footprint = new Point(42, 10) },
        new() { Id = "planter", Texture = "Props/planter", Footprint = new Point(30, 14) },
        new() { Id = "cypress-pot", Texture = "Props/cypress-pot", Footprint = new Point(16, 8) },
        new() { Id = "flower-cart", Texture = "Props/flower-cart", Footprint = new Point(46, 12) },
        new() { Id = "well-roofed", Texture = "Props/well-roofed", Footprint = new Point(42, 18) },
        new() { Id = "cask", Texture = "Props/cask", Footprint = new Point(10, 6) },
        new() { Id = "water-barrel", Texture = "Props/water-barrel", Footprint = new Point(16, 8) },
        new() { Id = "stall-blue", Texture = "Props/stall-blue", Footprint = new Point(66, 28) },
        new() { Id = "stall-pink", Texture = "Props/stall-pink", Footprint = new Point(64, 28) },
        new() { Id = "stall-teal", Texture = "Props/stall-teal", Footprint = new Point(62, 28) },
        new() { Id = "stall-cloth", Texture = "Props/stall-cloth", Footprint = new Point(64, 28) },
        new() { Id = "cherry-tree", Texture = "Props/cherry-tree", Footprint = new Point(16, 7) },
        new() { Id = "cherry-tree-small", Texture = "Props/cherry-tree-small", Footprint = new Point(12, 6) },
        new() { Id = "oak", Texture = "Props/oak", Footprint = new Point(18, 7) },
        new() { Id = "blossom-tree", Texture = "Props/blossom-tree", Footprint = new Point(16, 7) },
        new() { Id = "cypress", Texture = "Props/cypress", Footprint = new Point(9, 4) },
        new() { Id = "topiary", Texture = "Props/topiary", Footprint = new Point(26, 10) },
        new() { Id = "flower-arch", Texture = "Props/flower-arch", Footprint = new Point(48, 6), Gap = 30 },
        new() { Id = "bush-flowers", Texture = "Props/bush-flowers", Footprint = new Point(28, 10) },
        new() { Id = "bush-pink", Texture = "Props/bush-pink", Footprint = new Point(28, 10) },
        new() { Id = "rocks", Texture = "Props/rocks", Footprint = new Point(26, 10) },
        new() { Id = "flowers-pink", Texture = "Props/flowers-pink", Placement = PropPlacement.Ground },
        new() { Id = "flowers-white", Texture = "Props/flowers-white", Placement = PropPlacement.Ground },
        new() { Id = "flowers-mixed", Texture = "Props/flowers-mixed", Placement = PropPlacement.Ground },
        // The harbour.
        new() { Id = "cargo-crate", Texture = "Props/cargo-crate", Footprint = new Point(20, 10) },
        new() { Id = "cargo-barrel", Texture = "Props/cargo-barrel", Footprint = new Point(16, 8) },
        new() { Id = "sacks", Texture = "Props/sacks", Footprint = new Point(32, 12) },
        new() { Id = "rope-posts", Texture = "Props/rope-posts", Footprint = new Point(38, 6) },
        new() { Id = "bollard", Texture = "Props/bollard", Footprint = new Point(8, 5) },
        new() { Id = "lantern-post", Texture = "Props/lantern-post", Footprint = new Point(6, 4) },
        new() { Id = "signpost", Texture = "Props/signpost", Footprint = new Point(8, 4) },
        new() { Id = "ship", Texture = "Props/ship", Frames = 2, SortRise = 70 },
        new() { Id = "gangplank", Texture = "Props/gangplank", Placement = PropPlacement.Ground },
        new() { Id = "rowboat", Texture = "Props/rowboat", Placement = PropPlacement.Ground },
        // What Questeria's sign and waypoint look like: the notice board, and the crystal on its
        // plinth, dim and then lit.
        new() { Id = "notice-board", Texture = "Props/notice-board", Footprint = new Point(36, 8) },
        new() { Id = "crystal-stone", Texture = "Props/crystal-stone", Frames = 2, Footprint = new Point(14, 6) },
    };

    private static readonly Dictionary<string, PropDefinition> ById = new();

    /// <summary>Indexes the table and checks every row.</summary>
    /// <remarks>Throws on purpose, so a bad row shows up when the game starts.</remarks>
    static PropCatalog()
    {
        foreach (PropDefinition prop in Table.Concat(Questeria))
        {
            if (prop.Footprint.X < 0 || prop.Footprint.Y < 0)
                throw new InvalidOperationException($"Prop {prop.Id} has a negative footprint.");
            if (prop.Gap != 0 && (prop.Gap < 0 || prop.Gap >= prop.Footprint.X))
                throw new InvalidOperationException($"Prop {prop.Id} has a gap of {prop.Gap} in a footprint {prop.Footprint.X} wide.");
            if (prop.Frames < 1)
                throw new InvalidOperationException($"Prop {prop.Id} has {prop.Frames} pictures. It needs at least one.");
            if (prop.Light is { } light && (light.Reach <= 0f || light.Strength <= 0f))
                throw new InvalidOperationException($"Prop {prop.Id} gives off a light that reaches nowhere.");
            if (!ById.TryAdd(prop.Id, prop))
                throw new InvalidOperationException($"Two props are called {prop.Id}.");
        }
    }

    /// <summary>Looks a prop up by id.</summary>
    /// <param name="id">What map files call it.</param>
    /// <param name="prop">The prop, if there's one called that.</param>
    /// <returns>True if there is.</returns>
    public static bool TryGet(string id, out PropDefinition prop) => ById.TryGetValue(id ?? string.Empty, out prop);
}
