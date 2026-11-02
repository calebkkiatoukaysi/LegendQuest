using System;
using System.Collections.Generic;

namespace LegendQuest.World;

/// <summary>
/// Every tileset, by the id map files use.
/// </summary>
/// <remarks>
/// The tile lists have to match the lists in tools/generate_world.py (and tools/cut_supplied.py for
/// Questeria), in the same order, since that's the order the tiles are on the sheet. Tilemap throws
/// when it loads a sheet that's the wrong size, which is what a mismatch looks like.
/// </remarks>
public static class TilesetCatalog
{
    /// <summary>How long each frame of the water's ripples is up, in seconds. The harbour's and the night sea's are the same.</summary>
    private const float WaterFrameSeconds = 0.35f;

    /// <summary>How long each frame of a lantern swinging on the Gull's Wager's rail is up, in seconds.</summary>
    private const float LanternFrameSeconds = 0.3f;

    /// <summary>Questeria and its harbour: the plaza's slabs, the terrace, cobbles, lawns, sand, water and the boardwalk. Everything standing on them is props.</summary>
    /// <remarks>
    /// Drawn by tools/generate_harbour.py, which has to list them in this order too. The lawns have
    /// a kerb round them, the water a quay wall and a lip of coping, and the boardwalk a beam along
    /// its front, all drawn by Tilemap wherever they meet something else. The water ripples: four
    /// frames a variant.
    /// </remarks>
    private static readonly TilesetDefinition Questeria = new()
    {
        Id = "questeria",
        Texture = "Tiles/questeria",
        Tiles = new TileDefinition[]
        {
            new() { Name = "plaza", Variants = 6 },
            new() { Name = "terrace", Variants = 2 },
            new() { Name = "cobble", Variants = 3 },
            new() { Name = "grass", Variants = 4, Edged = true, Surface = Surface.Grass },
            new() { Name = "sand", Variants = 2, Surface = Surface.Grass },
            new() { Name = "water", Variants = 3, Frames = 4, FrameSeconds = WaterFrameSeconds, Edged = true, Solid = true },
            new() { Name = "planks", Variants = 4, Edged = true },
        },
    };

    /// <summary>The Gull's Wager: the night sea all round her, her deck, her rails and her side, the hatch, her bow and stern, the bowsprit, and the lanterns swinging on her rail.</summary>
    /// <remarks>
    /// Drawn by tools/generate_harbour.py in daylight colours; the deck's map is dark, and its
    /// lanterns light it. The bow and stern are blocks of nine tiles each way that go on the detail
    /// layer, painted whole so her rails curve round them; the ground under them says where you can
    /// stand. The bowsprit is a block of three that carries on from the bow, and the hatch a block of two.
    /// </remarks>
    private static readonly TilesetDefinition Ship = new()
    {
        Id = "ship",
        Texture = "Tiles/ship",
        Tiles = new TileDefinition[]
        {
            new() { Name = "sea", Variants = 3, Frames = 4, FrameSeconds = WaterFrameSeconds, Solid = true },
            new() { Name = "deck", Variants = 3 },
            new() { Name = "rail-n", Variants = 2, Solid = true },
            new() { Name = "rail-s", Variants = 2, Solid = true },
            new() { Name = "hull", Variants = 2, Solid = true },
            new() { Name = "hatch", Block = 2, Solid = true },
            new() { Name = "bow", Block = 9 },
            new() { Name = "stern", Block = 9 },
            new() { Name = "bowsprit", Block = 3 },
            new() { Name = "lantern", Frames = 4, FrameSeconds = LanternFrameSeconds },
        },
    };

    /// <summary>Inside Questeria's buildings: wood and stone floors, the rug, and the walls.</summary>
    /// <remarks>
    /// Drawn by tools/generate_interiors.py. The back wall shows its face, two tiles of it (plaster
    /// with a beam, over panelling), and the other walls only their tops, so a room's open towards
    /// you. The rug has a border, drawn wherever it meets the floor.
    /// </remarks>
    private static readonly TilesetDefinition Interior = new()
    {
        Id = "interior",
        Texture = "Tiles/interior",
        Tiles = new TileDefinition[]
        {
            new() { Name = "floor-wood", Variants = 3 },
            new() { Name = "floor-stone", Variants = 2 },
            new() { Name = "rug", Edged = true },
            new() { Name = "wall-top", Solid = true, Opaque = true },
            new() { Name = "wall-face", Variants = 3, Solid = true, Opaque = true },
            new() { Name = "wall-base", Variants = 2, Solid = true, Opaque = true },
        },
    };

    /// <summary>The barrow: stone floors, walls seen from above and from the front, and what's lying about.</summary>
    /// <remarks>
    /// Drawn by tools/generate_world.py, which has to list them in this order too. The floors are
    /// blocks of two tiles each way, so the slabs can be every size; the worn floor and the floor
    /// in a wall's shadow are the same blocks, so they carry on into the floor round them, and
    /// they need the same number of variants as the floor for that.
    /// </remarks>
    private static readonly TilesetDefinition Crypt = new()
    {
        Id = "crypt",
        Texture = "Tiles/crypt",
        Tiles = new TileDefinition[]
        {
            // Ground.
            new() { Name = "floor", Variants = 4, Block = 2 },
            new() { Name = "floor-worn", Variants = 4, Block = 2 },
            new() { Name = "floor-shadow", Variants = 4, Block = 2 },
            new() { Name = "steps" },

            // Walls: the top of one seen from above, then the two rows of its face. The niche and
            // the carving go in the upper row.
            new() { Name = "wall-top", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "wall-face", Variants = 3, Solid = true, Opaque = true },
            new() { Name = "wall-niche", Solid = true, Opaque = true },
            new() { Name = "wall-carving", Solid = true, Opaque = true },
            new() { Name = "wall-base", Variants = 2, Solid = true, Opaque = true },

            // Detail, drawn over the floor. The cobwebs go in a room's top left and top right corners.
            new() { Name = "rubble", Solid = true },
            new() { Name = "bones", Variants = 2 },
            new() { Name = "cobweb" },
            new() { Name = "cobweb-ne" },
        },
    };

    /// <summary>The old road: grass, fallen leaves, the road and the undergrowth, and what's lying about on them.</summary>
    /// <remarks>
    /// Drawn by tools/generate_world.py, which has to list them in this order too. The leaves, the
    /// road and the undergrowth have edges, which Tilemap draws wherever they meet other ground:
    /// grass reaching over the leaves, a verge along the road, and the top and face of the
    /// undergrowth. The trees, bushes, stumps and logs are props, standing on their feet, so their
    /// crowns overlap and the ones in front of you hide you.
    /// </remarks>
    private static readonly TilesetDefinition Forest = new()
    {
        Id = "forest",
        Texture = "Tiles/forest",
        Tiles = new TileDefinition[]
        {
            // Ground. The undergrowth is what's under the deep wood: you can't push through it.
            new() { Name = "grass", Variants = 4, Surface = Surface.Grass },
            new() { Name = "leaves", Variants = 4, Edged = true, Surface = Surface.Grass },
            new() { Name = "road", Variants = 3, Edged = true, Surface = Surface.Grass },
            new() { Name = "undergrowth", Variants = 2, Edged = true, Solid = true },

            // Detail, drawn over the ground. All of it stays off the edges of its tile, so a patch of
            // it has a ragged edge, apart from the ruts, which run on along the road.
            new() { Name = "moss", Variants = 2 },
            new() { Name = "tuft", Variants = 2 },
            new() { Name = "fern", Variants = 2 },
            new() { Name = "flowers", Variants = 2 },
            new() { Name = "pebbles", Variants = 2 },
            new() { Name = "roots", Variants = 2 },
            new() { Name = "ruts", Variants = 2 },
            new() { Name = "litter", Variants = 2 },
        },
    };

    /// <summary>The meadows round Questeria, the old road's and Thornbridge's: grass and long grass, the footpaths and the road, ploughed fields, the stream and the ford across it, hedges, the top of the city wall, and the flowers, clover, crops and the rest lying about on them.</summary>
    /// <remarks>
    /// Drawn by tools/generate_meadow.py, which has to list them in this order too. Grass is what
    /// everything else sits in, so it's the one without edges: the long grass has short grass round
    /// it, the paths and the road a verge, the fields grass growing over them, the stream a bank and
    /// a line of foam, the ford wet gravel, a hedge its lit top and shadowed face, and the wall its
    /// parapet. The stream and the ford ripple, four frames a variant, the same as the harbour. The
    /// ford's shallow enough to wade, so a path can cross the stream without a bridge. The trees, the
    /// farm, the bridge, the gate and the camp are props.
    /// </remarks>
    private static readonly TilesetDefinition Meadow = new()
    {
        Id = "meadow",
        Texture = "Tiles/meadow",
        Tiles = new TileDefinition[]
        {
            // Ground. You can't get through the stream, a hedge or the wall, and you can't see past the wall.
            new() { Name = "grass", Variants = 6, Surface = Surface.Grass },
            new() { Name = "long-grass", Variants = 3, Edged = true, Surface = Surface.Grass },
            new() { Name = "path", Variants = 3, Edged = true, Surface = Surface.Grass },
            new() { Name = "road", Variants = 3, Edged = true, Surface = Surface.Grass },
            new() { Name = "soil", Variants = 2, Edged = true, Surface = Surface.Grass },
            new() { Name = "stream", Variants = 3, Frames = 4, FrameSeconds = WaterFrameSeconds, Edged = true, Solid = true },
            new() { Name = "ford", Variants = 2, Frames = 4, FrameSeconds = WaterFrameSeconds, Edged = true, Surface = Surface.Grass },
            new() { Name = "hedge", Variants = 2, Edged = true, Solid = true },
            new() { Name = "wall", Variants = 2, Edged = true, Solid = true, Opaque = true },

            // Detail, drawn over the ground. All of it stays off the edges of its tile, so a patch of
            // it has a ragged edge, apart from the ruts and the crops, which run on along the road and
            // the furrows. The lilies go on the stream and the stepping stones on the ford.
            new() { Name = "flowers", Variants = 4 },
            new() { Name = "clover", Variants = 2 },
            new() { Name = "tuft", Variants = 2 },
            new() { Name = "pebbles", Variants = 2 },
            new() { Name = "ruts", Variants = 2 },
            new() { Name = "reeds", Variants = 2 },
            new() { Name = "lilies", Variants = 2 },
            new() { Name = "crops", Variants = 2 },
            new() { Name = "stepping", Variants = 2 },
            new() { Name = "petals", Variants = 2 },
        },
    };

    /// <summary>How long each frame of the oil going round the tar is up, in seconds. Slower than water, since tar's thick.</summary>
    private const float TarFrameSeconds = 0.45f;

    /// <summary>The Mirewood: three grasses from lush to rotten, fallen leaves and rotten ones, gloom moss, the rangers' trail, the tar-streaked stream, the undergrowth, the rock the quarry's cut into, and what's lying about on them.</summary>
    /// <remarks>
    /// Drawn by tools/generate_mirewood.py, which has to list them in this order too. The three
    /// grasses share their light, so where one meets the next only the colour changes. The leaves,
    /// the moss, the trail, the stream, the undergrowth and the top of the rock have edges, which
    /// Tilemap draws wherever they meet other ground: grass reaching over the leaves and the moss, a
    /// verge along the trail, the stream's banks, and the lip of the rock where its face drops away.
    /// The stream and the tar lying on it move: four frames a variant, the oil going round the tar.
    /// The trees are props, the same as the old road's.
    /// </remarks>
    private static readonly TilesetDefinition Mirewood = new()
    {
        Id = "mirewood",
        Texture = "Tiles/mirewood",
        Tiles = new TileDefinition[]
        {
            // Ground, lush at the west edge and rotten by the quarry. The undergrowth and the rock
            // you can't get through.
            new() { Name = "grass", Variants = 4, Surface = Surface.Grass },
            new() { Name = "grass-wilt", Variants = 4, Surface = Surface.Grass },
            new() { Name = "grass-rot", Variants = 4, Surface = Surface.Grass },
            new() { Name = "leaves", Variants = 4, Edged = true, Surface = Surface.Grass },
            new() { Name = "leaves-blackvein", Variants = 3, Edged = true, Surface = Surface.Grass },
            new() { Name = "gloom-moss", Variants = 2, Edged = true, Surface = Surface.Grass },
            new() { Name = "trail", Variants = 3, Edged = true, Surface = Surface.Grass },
            new() { Name = "tar-stream", Variants = 3, Frames = 4, FrameSeconds = TarFrameSeconds, Edged = true, Solid = true },
            new() { Name = "undergrowth", Variants = 2, Edged = true, Solid = true },
            new() { Name = "undergrowth-rot", Variants = 2, Edged = true, Solid = true },

            // The rock: its top seen from above, then the two rows of its cut face.
            new() { Name = "cliff-top", Variants = 2, Edged = true, Solid = true, Opaque = true },
            new() { Name = "cliff-face", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "cliff-base", Variants = 2, Solid = true, Opaque = true },

            // Detail, drawn over the ground and kept off the edges of its tile. The brambles go over
            // the undergrowth, and the tar lying thick on the stream moves with it.
            new() { Name = "tuft", Variants = 2 },
            new() { Name = "flowers", Variants = 2 },
            new() { Name = "fern", Variants = 2 },
            new() { Name = "pebbles", Variants = 2 },
            new() { Name = "roots", Variants = 2 },
            new() { Name = "litter", Variants = 2 },
            new() { Name = "veins", Variants = 2 },
            new() { Name = "tuft-dead", Variants = 2 },
            new() { Name = "toadstools", Variants = 2 },
            new() { Name = "tar-spatter", Variants = 2 },
            new() { Name = "brambles", Variants = 2 },
            new() { Name = "tar-slick", Variants = 2, Frames = 4, FrameSeconds = TarFrameSeconds },
        },
    };

    /// <summary>The Old Quarry: cut stone floors, the floor the black's got into, broken ground full of roots, tar welling up, the walls, and what's lying about.</summary>
    /// <remarks>
    /// Drawn by tools/generate_mirewood.py, which has to list them in this order too. Laid out the
    /// barrow's way: the floors are blocks of two tiles each way, and the floor in a wall's shadow
    /// and the black-veined floor are the same blocks, so they carry on into the floor round them
    /// and need its number of variants. The walls show their top from above and two rows of face,
    /// the roots and the crystal going in the upper row. The roots and the tar have edges; the tar
    /// moves, like the Mirewood's.
    /// </remarks>
    private static readonly TilesetDefinition Quarry = new()
    {
        Id = "quarry",
        Texture = "Tiles/quarry",
        Tiles = new TileDefinition[]
        {
            // Ground.
            new() { Name = "floor", Variants = 4, Block = 2 },
            new() { Name = "floor-shadow", Variants = 4, Block = 2 },
            new() { Name = "black-vein-floor", Variants = 4, Block = 2 },
            new() { Name = "root-floor", Variants = 2, Edged = true, Surface = Surface.Grass },
            new() { Name = "tar-seep", Variants = 2, Frames = 4, FrameSeconds = TarFrameSeconds, Edged = true, Solid = true },
            new() { Name = "ramp", Surface = Surface.Grass },

            // Walls: the top of one seen from above, then the two rows of its face.
            new() { Name = "wall-top", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "wall-face", Variants = 3, Solid = true, Opaque = true },
            new() { Name = "wall-roots", Solid = true, Opaque = true },
            new() { Name = "wall-crystal", Solid = true, Opaque = true },
            new() { Name = "wall-base", Variants = 2, Solid = true, Opaque = true },

            // Detail, drawn over the floor.
            new() { Name = "rubble", Solid = true },
            new() { Name = "gravel", Variants = 2 },
            new() { Name = "roots", Variants = 2 },
            new() { Name = "shards", Variants = 2 },
            new() { Name = "fungus", Variants = 2 },
            new() { Name = "tools" },
        },
    };

    /// <summary>Sunspire Keep: polished marble, marble with gold in its joints, the royal carpet, the dais and its steps, and walls three tiles tall with tall windows in them.</summary>
    /// <remarks>
    /// Drawn by tools/generate_keep.py, which has to list them in this order too. The marble's
    /// squares meet across every tile, so its variants mix with no seams. The carpet has a crimson
    /// and gold border and the dais a gold rim (and a riser along its front), drawn wherever they
    /// meet other ground. A wall's face is three rows: the cornice, the courses, and the blue dado.
    /// A window is two tiles of its own, its arch in the cornice's row and its glass and sill in the
    /// row under it, so a map puts each one exactly where it wants it.
    /// </remarks>
    private static readonly TilesetDefinition Keep = new()
    {
        Id = "keep",
        Texture = "Tiles/keep",
        Tiles = new TileDefinition[]
        {
            // Floors.
            new() { Name = "marble", Variants = 3 },
            new() { Name = "marble-gold", Variants = 2 },
            new() { Name = "carpet", Variants = 2, Edged = true },
            new() { Name = "dais", Variants = 2, Edged = true },
            new() { Name = "steps" },

            // Walls: the top of one seen from above, then the three rows of its face, and the window.
            new() { Name = "wall-top", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "wall-high", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "wall-face", Variants = 3, Solid = true, Opaque = true },
            new() { Name = "wall-base", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "window-high", Solid = true, Opaque = true },
            new() { Name = "window", Solid = true, Opaque = true },
        },
    };

    /// <summary>The Old Crown Vault, under the Sunspire: pale polished floors with gold let into them, the traps, the walls with their gilded cornice, and the earth the Hollowed sappers have dug through.</summary>
    /// <remarks>
    /// Drawn by tools/generate_vault.py, which has to list them in this order too. The floors are
    /// blocks of two tiles each way, like the barrow's, and the floor in a wall's shadow and the
    /// cracked floor are the same blocks, so they need the same number of variants. The sun crest
    /// is a block of two, so it goes on an even column and an even row. The inlay has a band of
    /// gold round it, the pit its far wall and its lips, and the earth the broken ends of the slabs
    /// round it, all drawn by Tilemap wherever they meet other ground.
    /// </remarks>
    private static readonly TilesetDefinition Vault = new()
    {
        Id = "vault",
        Texture = "Tiles/vault",
        Tiles = new TileDefinition[]
        {
            // Ground. The pit stops you walking but not seeing, like rubble does.
            new() { Name = "floor", Variants = 4, Block = 2 },
            new() { Name = "floor-shadow", Variants = 4, Block = 2 },
            new() { Name = "floor-cracked", Variants = 4, Block = 2 },
            new() { Name = "inlay", Variants = 3, Edged = true },
            new() { Name = "sun-crest", Block = 2 },
            new() { Name = "pressure-plate", Variants = 2 },
            new() { Name = "pit", Edged = true, Solid = true },
            new() { Name = "earth", Variants = 3, Edged = true, Surface = Surface.Grass },
            new() { Name = "tunnel", Variants = 3, Surface = Surface.Grass },
            new() { Name = "steps" },

            // Walls: the top of one seen from above, then the two rows of its face, with the crest
            // and the niche in the upper row; and the banks of earth where a tunnel's come down.
            new() { Name = "wall-top", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "wall-face", Variants = 3, Solid = true, Opaque = true },
            new() { Name = "wall-crest", Solid = true, Opaque = true },
            new() { Name = "wall-niche", Solid = true, Opaque = true },
            new() { Name = "wall-base", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "earth-top", Variants = 2, Solid = true, Opaque = true },
            new() { Name = "earth-face", Variants = 2, Solid = true, Opaque = true },

            // Detail, drawn over the floor. The cobwebs go in a room's top left and top right corners.
            new() { Name = "coins", Variants = 3 },
            new() { Name = "rubble", Variants = 2, Solid = true },
            new() { Name = "debris", Variants = 2 },
            new() { Name = "darts", Variants = 2 },
            new() { Name = "bones" },
            new() { Name = "cobweb" },
            new() { Name = "cobweb-ne" },
        },
    };

    private static readonly Dictionary<string, TilesetDefinition> ById = new();

    /// <summary>Indexes every tileset and checks each one.</summary>
    /// <remarks>Throws on purpose, so a broken tileset is found when the game starts and not on the map that uses it.</remarks>
    static TilesetCatalog()
    {
        foreach (TilesetDefinition tileset in new[] { Questeria, Ship, Interior, Crypt, Forest, Meadow, Mirewood, Quarry, Keep, Vault })
        {
            tileset.Validate();
            if (!ById.TryAdd(tileset.Id, tileset))
                throw new InvalidOperationException($"Two tilesets are called {tileset.Id}.");
        }
    }

    /// <summary>Looks a tileset up by id.</summary>
    /// <param name="id">What map files call it.</param>
    /// <param name="tileset">The tileset, if there's one called that.</param>
    /// <returns>True if there is.</returns>
    public static bool TryGet(string id, out TilesetDefinition tileset) => ById.TryGetValue(id ?? string.Empty, out tileset);
}
