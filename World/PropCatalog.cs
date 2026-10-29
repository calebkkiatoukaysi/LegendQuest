using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// Every prop a map can place, by the id map files use.
/// </summary>
/// <remarks>
/// Each one's picture is Content/Props/(id).png. Most are cut from my pictures, by
/// tools/cut_supplied.py (Questeria's, the harbour's, the furniture, and the houses it puts together
/// from their parts) or by tools/generate_world.py (the old road's trees and the barrow's urns and
/// chests). The rest are drawn to match: the barrow's sarcophagus, sconces and doors and the camp by
/// generate_world.py, the ship by generate_harbour.py, and the bed by generate_interiors.py.
/// </remarks>
public static class PropCatalog
{
    /// <summary>How far a sconce's torch light reaches. Four tiles, so a hall wants one every eight or so.</summary>
    private const float TorchReach = 64f;

    /// <summary>How far the smithy's forge lights the street outside its arch.</summary>
    private const float ForgeReach = 72f;

    /// <summary>How far a ship's lantern's light reaches: a little over two tiles, so the deck between them stays dark.</summary>
    private const float LanternReach = 40f;

    /// <summary>How far a hearth's light reaches: most of a tavern.</summary>
    private const float HearthReach = 120f;

    /// <summary>How far a candle stand's light reaches.</summary>
    private const float CandleReach = 52f;

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

        // A blue urn for the barrow, off my props sheet like the other one.
        new() { Id = "urn-blue", Texture = "Props/urn-blue", Footprint = new Point(12, 5) },

        // The old road's wood: trees and undergrowth tools/generate_world.py cuts off my trees sheet,
        // some of them a deeper green for the back of the wood, and the logs and mushrooms it draws to
        // match. A tree only blocks its trunk, so you walk under its crown. The tree down across the
        // road lies from the near verge to the far one and blocks all of it. Ferns, long grass and
        // mushrooms you walk through.
        new() { Id = "oak-deep", Texture = "Props/oak-deep", Footprint = new Point(18, 7) },
        new() { Id = "oak-light", Texture = "Props/oak-light", Footprint = new Point(18, 7) },
        new() { Id = "oak-pale", Texture = "Props/oak-pale", Footprint = new Point(18, 7) },
        new() { Id = "oak-amber", Texture = "Props/oak-amber", Footprint = new Point(18, 7) },
        new() { Id = "fir", Texture = "Props/fir", Footprint = new Point(9, 4) },
        new() { Id = "fir-deep", Texture = "Props/fir-deep", Footprint = new Point(9, 4) },
        new() { Id = "stump", Texture = "Props/stump", Footprint = new Point(30, 10) },
        new() { Id = "snag", Texture = "Props/snag", Footprint = new Point(12, 6) },
        new() { Id = "dead-tree", Texture = "Props/dead-tree", Footprint = new Point(12, 6) },
        new() { Id = "bush", Texture = "Props/bush", Footprint = new Point(26, 10) },
        new() { Id = "bush-deep", Texture = "Props/bush-deep", Footprint = new Point(30, 10) },
        new() { Id = "bush-small", Texture = "Props/bush-small", Footprint = new Point(16, 7) },
        new() { Id = "fern", Texture = "Props/fern" },
        new() { Id = "grass-tall", Texture = "Props/grass-tall" },
        new() { Id = "rocks-mossy", Texture = "Props/rocks-mossy", Footprint = new Point(28, 10) },
        new() { Id = "log", Texture = "Props/log", Footprint = new Point(42, 10) },
        new() { Id = "fallen-tree", Texture = "Props/fallen-tree", Footprint = new Point(44, 72) },
        new() { Id = "mushrooms", Texture = "Props/mushrooms" },
        new() { Id = "mushrooms-pale", Texture = "Props/mushrooms-pale" },
    };

    /// <summary>Questeria's props and the harbour's. The buildings block most of their width and a few
    /// tiles deep, so you can walk round them but not into them, and going behind one, its roof hides
    /// you. The gate and the flower arch have a gap down the middle to walk through. The ship sorts
    /// from its far rail rather than the water, so whoever's stood on its deck is drawn on it, and the
    /// gangplank from its top end, so whoever's walking down it is drawn on it too.</summary>
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
        // The buildings put together from the parts on my houses sheet. Each one's foot is under the
        // middle of its walls, whatever hangs off the side; where its door is, the middle of the
        // bottom of it, is said from the foot in art pixels, for the map's entrance.
        // Door 16 right of the foot.
        new() { Id = "townhouse", Texture = "Props/townhouse", Footprint = new Point(58, 84) },
        // Door 2 right of the foot.
        new() { Id = "cottage", Texture = "Props/cottage", Footprint = new Point(80, 62) },
        // Door 15 right of the foot, the turret on the left.
        new() { Id = "turret-house", Texture = "Props/turret-house", Footprint = new Point(78, 84) },
        // Door 24 right of the foot, the awning on the left and the sign off the left corner.
        new() { Id = "bakery", Texture = "Props/bakery", Footprint = new Point(86, 80) },
        // Door on the foot.
        new() { Id = "chapel", Texture = "Props/chapel", Footprint = new Point(130, 70) },
        // The arch 8 left of the foot, with the fire in it 4 up. The sign stands off the right corner.
        new()
        {
            Id = "forge", Texture = "Props/forge", Footprint = new Point(62, 70),
            Light = new PropLight(new Vector2(-8f, -4f), new Vector2(-8f, 14f), ForgeReach, 1f),
        },
        // Door 3 left of the foot, the tower on the left and the banner on its stand off the right.
        new() { Id = "watch-post", Texture = "Props/watch-post", Footprint = new Point(94, 78) },
        // Door 3 left of the foot, and an open doorway at the right end.
        new() { Id = "warehouse", Texture = "Props/warehouse", Footprint = new Point(114, 80) },
        // Door 45 right of the foot, under the front gable; the sign hangs off the left corner.
        new() { Id = "gilded-flagon", Texture = "Props/gilded-flagon", Footprint = new Point(138, 82) },
        // Door 19 right of the foot; the sign stands off the right corner.
        new() { Id = "rusty-anvil", Texture = "Props/rusty-anvil", Footprint = new Point(90, 82) },
        // Door 1 right of the foot, under the awning; the sign hangs off the left corner.
        new() { Id = "fishmonger", Texture = "Props/fishmonger", Footprint = new Point(58, 56) },
        // The Flagon's sign on its own post, for the street. Its foot is the post.
        new() { Id = "gilded-flagon-sign", Texture = "Props/gilded-flagon-sign", Footprint = new Point(6, 4) },
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
        // More for the docks. The bell hangs off its gallows, so the whole width's blocked under it.
        new() { Id = "harbour-bell", Texture = "Props/harbour-bell", Footprint = new Point(22, 6) },
        new() { Id = "fish-stall", Texture = "Props/fish-stall", Footprint = new Point(54, 26) },
        new() { Id = "net-rack", Texture = "Props/net-rack", Footprint = new Point(38, 4) },
        new() { Id = "washing-line", Texture = "Props/washing-line", Footprint = new Point(38, 4) },
        new() { Id = "crate-open", Texture = "Props/crate-open", Footprint = new Point(18, 9) },
        new() { Id = "crate-apples", Texture = "Props/crate-apples", Footprint = new Point(16, 8) },
        new() { Id = "crate-onions", Texture = "Props/crate-onions", Footprint = new Point(14, 7) },
        new() { Id = "crate-stack", Texture = "Props/crate-stack", Footprint = new Point(26, 12) },
        new() { Id = "barrel-water", Texture = "Props/barrel-water", Footprint = new Point(17, 8) },
        new() { Id = "barrel-salt", Texture = "Props/barrel-salt", Footprint = new Point(15, 7) },
        new() { Id = "sack-pile", Texture = "Props/sack-pile", Footprint = new Point(26, 12) },
        new() { Id = "water-pump", Texture = "Props/water-pump", Footprint = new Point(18, 6) },
        new() { Id = "trough", Texture = "Props/trough", Footprint = new Point(32, 12) },
        new() { Id = "washtub", Texture = "Props/washtub", Footprint = new Point(21, 9) },
        new() { Id = "well-flowers", Texture = "Props/well-flowers", Footprint = new Point(48, 20) },
        // More for the streets. The lamps with an arm have their post to one side, so they block
        // under the lantern too. None of the lamps gives off a light: a prop's light comes with a
        // torch's flame, which is bigger than a lantern's glass, the same as the lamps that were here.
        new() { Id = "quest-board", Texture = "Props/quest-board", Footprint = new Point(40, 8) },
        new() { Id = "signpost-arrows", Texture = "Props/signpost-arrows", Footprint = new Point(8, 4) },
        new() { Id = "lamp-arm", Texture = "Props/lamp-arm", Footprint = new Point(32, 4) },
        new() { Id = "lamp-double", Texture = "Props/lamp-double", Footprint = new Point(24, 4) },
        new() { Id = "lantern-small", Texture = "Props/lantern-small", Footprint = new Point(10, 5) },
        new() { Id = "planter-stone", Texture = "Props/planter-stone", Footprint = new Point(21, 10) },
        new() { Id = "planter-long", Texture = "Props/planter-long", Footprint = new Point(36, 12) },
        new() { Id = "planter-pink", Texture = "Props/planter-pink", Footprint = new Point(32, 12) },
        new() { Id = "flower-box", Texture = "Props/flower-box", Footprint = new Point(15, 8) },
        new() { Id = "barrel-flowers", Texture = "Props/barrel-flowers", Footprint = new Point(18, 8) },
        new() { Id = "bench-iron", Texture = "Props/bench-iron", Footprint = new Point(37, 8) },
        new() { Id = "bench-plain", Texture = "Props/bench-plain", Footprint = new Point(37, 8) },
        new() { Id = "handcart", Texture = "Props/handcart", Footprint = new Point(29, 10) },
        new() { Id = "fence-iron", Texture = "Props/fence-iron", Footprint = new Point(68, 5) },
        new() { Id = "chalkboard", Texture = "Props/chalkboard", Footprint = new Point(15, 5) },
        new() { Id = "sewer-grate", Texture = "Props/sewer-grate", Placement = PropPlacement.Ground },
        new() { Id = "ship", Texture = "Props/ship", Frames = 7, SortRise = 70 },
        new() { Id = "gangplank", Texture = "Props/gangplank", Frames = 6, SortRise = 48 },
        // The Gull's Wager's deck. The lanterns light it; their glass is 26 art pixels up their post.
        new() { Id = "mast", Texture = "Props/mast", Footprint = new Point(14, 6) },
        new()
        {
            Id = "lantern-hanging", Texture = "Props/lantern-hanging", Footprint = new Point(4, 3),
            Light = new PropLight(new Vector2(3f, -24f), new Vector2(0f, 6f), LanternReach, 1f),
        },
        new() { Id = "ship-wheel", Texture = "Props/ship-wheel", Footprint = new Point(14, 6) },
        new() { Id = "rope-coil", Texture = "Props/rope-coil", Placement = PropPlacement.Ground },
        // Inside: the Gilded Flagon, the cottages, the shops and the warehouses, all off my props
        // sheet but the bed. The hearth and the candle stands light the rooms, which are dim. The
        // hearth's fire is low in its oven, 20 up; the candles are at the top of their stand.
        new() { Id = "bar-counter", Texture = "Props/bar-counter", Footprint = new Point(100, 12) },
        new() { Id = "round-table", Texture = "Props/round-table", Footprint = new Point(22, 9) },
        new() { Id = "stool", Texture = "Props/stool", Footprint = new Point(8, 4) },
        new()
        {
            Id = "hearth", Texture = "Props/hearth", Footprint = new Point(40, 10),
            Light = new PropLight(new Vector2(1f, -20f), new Vector2(0f, 14f), HearthReach, 1f),
        },
        new() { Id = "keg-stack", Texture = "Props/keg-stack", Footprint = new Point(34, 12) },
        new() { Id = "bed", Texture = "Props/bed", Footprint = new Point(30, 34) },
        new() { Id = "wardrobe", Texture = "Props/wardrobe", Footprint = new Point(26, 10) },
        new() { Id = "bookshelf", Texture = "Props/bookshelf", Footprint = new Point(40, 10) },
        new() { Id = "shop-counter", Texture = "Props/shop-counter", Footprint = new Point(56, 12) },
        new()
        {
            Id = "candle-stand", Texture = "Props/candle-stand", Footprint = new Point(6, 4),
            Light = new PropLight(new Vector2(0f, -30f), new Vector2(0f, 4f), CandleReach, 0.8f),
        },
        new() { Id = "chair", Texture = "Props/chair", Footprint = new Point(18, 8) },
        new() { Id = "armchair", Texture = "Props/armchair", Footprint = new Point(22, 10) },
        new() { Id = "desk", Texture = "Props/desk", Footprint = new Point(43, 14) },
        new() { Id = "side-table", Texture = "Props/side-table", Footprint = new Point(24, 10) },
        new() { Id = "book-crate", Texture = "Props/book-crate", Footprint = new Point(19, 8) },
        new() { Id = "potted-plant", Texture = "Props/potted-plant", Footprint = new Point(12, 6) },
        new() { Id = "utensil-rack", Texture = "Props/utensil-rack", Footprint = new Point(29, 10) },
        new() { Id = "tool-rack", Texture = "Props/tool-rack", Footprint = new Point(26, 8) },
        new() { Id = "anvil", Texture = "Props/anvil", Footprint = new Point(27, 12) },
        new() { Id = "tapestry", Texture = "Props/tapestry", Placement = PropPlacement.Wall },
        new() { Id = "rowboat", Texture = "Props/rowboat", Placement = PropPlacement.Ground },
        // What Questeria's sign and waypoint look like: the notice board, and the crystal on its
        // plinth, dim and then lit.
        new() { Id = "notice-board", Texture = "Props/notice-board", Footprint = new Point(36, 8) },
        new() { Id = "crystal-stone", Texture = "Props/crystal-stone", Frames = 2, Footprint = new Point(14, 6) },
    };

    /// <summary>The roof colours a house can have. The one it's drawn in is its plain id; each of the
    /// others is (id)-(colour), recoloured by tools/cut_supplied.py.</summary>
    private static readonly string[] RoofColours = { "blue", "pink", "teal", "plum" };

    /// <summary>The houses that come in the other roof colours too, and the colour each is drawn in.
    /// The shop's roof is half pink and half blue, so it comes in all four.</summary>
    private static readonly (string Id, string Drawn)[] Reroofed =
    {
        ("house-ivy", "blue"), ("house-awning", "blue"), ("house-rose", "pink"), ("house-shop", null),
        ("townhouse", "blue"), ("cottage", "blue"), ("turret-house", "blue"), ("bakery", "blue"),
        ("chapel", "blue"), ("forge", "blue"), ("warehouse", "blue"), ("gilded-flagon", "blue"),
        ("rusty-anvil", "blue"), ("fishmonger", "blue"),
    };

    private static readonly Dictionary<string, PropDefinition> ById = new();

    /// <summary>Indexes the table and checks every row.</summary>
    /// <remarks>Throws on purpose, so a bad row shows up when the game starts.</remarks>
    static PropCatalog()
    {
        foreach (PropDefinition prop in Table.Concat(Questeria).Concat(OtherRoofs()))
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

    /// <summary>Every house that comes in other roof colours, in each of them: the same house, its
    /// footprint and any light it has, under the other roof.</summary>
    /// <returns>A prop for each house in each roof colour it isn't drawn in.</returns>
    private static IEnumerable<PropDefinition> OtherRoofs()
    {
        foreach ((string id, string drawn) in Reroofed)
        {
            PropDefinition house = Questeria.Single(prop => prop.Id == id);
            foreach (string colour in RoofColours.Where(other => other != drawn))
            {
                yield return new PropDefinition
                {
                    Id = $"{id}-{colour}", Texture = $"{house.Texture}-{colour}",
                    Footprint = house.Footprint, Gap = house.Gap, Light = house.Light,
                };
            }
        }
    }

    /// <summary>Looks a prop up by id.</summary>
    /// <param name="id">What map files call it.</param>
    /// <param name="prop">The prop, if there's one called that.</param>
    /// <returns>True if there is.</returns>
    public static bool TryGet(string id, out PropDefinition prop) => ById.TryGetValue(id ?? string.Empty, out prop);
}
