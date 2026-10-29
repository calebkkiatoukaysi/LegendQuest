using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Story;

/// <summary>
/// The paintings behind the story's big moments: each one's layers, back to front, how much each
/// sways, and what the game moves over them.
/// </summary>
/// <remarks>
/// Layered like the title: every layer's the title's canvas, 416x277 art pixels at 4x, and the
/// whole picture sways slowly with each layer moving its own share, so it reads as near and far.
/// Each layer is Content/Paintings/(id)-(layer).png, built by tools/generate_paintings.py out of
/// my title pictures, and the places here are on that canvas, so they have to match it: where the
/// lantern post's arm ends, where the headland's top is flat, where the lake is. The conversation's
/// panel covers the bottom of the screen, so what matters in each is above row 176.
/// </remarks>
public static class Paintings
{
    private static readonly Dictionary<string, Painting> Table = new()
    {
        // P-03: the pier in the storm where you met, the boat over, a child running for the end of
        // the pier. Rain, lightning out to sea on the right, and the lantern swinging on its arm.
        ["pier-memory"] = new()
        {
            Layers = new[] { ("sky", 0.05f), ("sea", 0.35f), ("pier", 1f) },
            Rain = 150,
            Lightning = new Rectangle(250, 26, 140, 92),
            Lantern = new Vector2(226, 97),
            LanternLayer = "pier",
        },

        // P-03: the two of you at sixteen on the headland, under a red moon, your hands clasped
        // and the Soul-Bond's mark glowing where they meet. The moon glints on the lake below.
        ["red-moon"] = new()
        {
            Layers = new[] { ("sky", 0.05f), ("moon", 0.1f), ("shore", 0.35f), ("headland", 1f) },
            Figures = new[] { new Vector2(198, 156), new Vector2(216, 156) },
            FiguresLayer = "headland",
            Backlight = new Vector2(182, 88),
            Rim = new Color(250, 96, 80),
            Shadow = new Color(16, 6, 16),
            Sigil = new Vector2(207, 145),
            Water = new Rectangle(130, 154, 48, 40),
            WaterLayer = "shore",
            Glints = 6,
            GlintColour = new Color(255, 150, 120),
        },

        // P-05: Questeria on its hill at dawn, from the ship's rail, the sun just up behind the
        // mountains and the lake glinting under the bridge.
        ["questeria-dawn"] = new()
        {
            Layers = new[] { ("sky", 0.05f), ("clouds", 0.15f), ("gulls", 0.3f), ("city", 0.45f), ("rail", 1f) },
            Water = new Rectangle(16, 218, 240, 30),
            WaterLayer = "city",
            Glints = 12,
            GlintColour = new Color(255, 236, 190),
        },
    };

    /// <summary>Checks the table.</summary>
    /// <remarks>Throws on purpose, so a mistake shows up when the game starts: something put on a layer the painting doesn't have, figures that aren't the two of you, or a mark with nobody to glow between.</remarks>
    static Paintings()
    {
        foreach ((string id, Painting painting) in Table)
        {
            Check(id, painting, painting.Glints > 0, painting.WaterLayer, "glints");
            Check(id, painting, painting.Figures.Count > 0, painting.FiguresLayer, "the two of you");
            Check(id, painting, painting.Lantern is not null, painting.LanternLayer, "a lantern");
            if (painting.Figures.Count is not (0 or 2)) throw new InvalidOperationException($"Painting {id} has {painting.Figures.Count} figures; it's the two of you or nobody.");
            if (painting.Sigil is not null && painting.Figures.Count == 0) throw new InvalidOperationException($"Painting {id} has the Soul-Bond's mark but nobody for it to glow between.");
        }
    }

    /// <summary>Whether there's a painting called this.</summary>
    /// <param name="id">What a script calls it.</param>
    public static bool Exists(string id) => id is not null && Table.ContainsKey(id);

    /// <summary>A painting.</summary>
    /// <param name="id">Its id. It has to exist.</param>
    public static Painting Get(string id) =>
        Table.TryGetValue(id, out var painting) ? painting : throw new InvalidOperationException($"There's no painting called {id}.");

    /// <summary>Every painting's id, for loading them all.</summary>
    public static IEnumerable<string> All => Table.Keys;

    /// <summary>Throws if a painting puts something on a layer it doesn't have.</summary>
    private static void Check(string id, Painting painting, bool used, string layer, string what)
    {
        if (!used) return;
        foreach ((string name, _) in painting.Layers)
            if (name == layer) return;
        throw new InvalidOperationException($"Painting {id} puts {what} on {layer ?? "no layer"}, which isn't one of its layers.");
    }
}
