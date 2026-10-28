using System;
using System.Collections.Generic;

namespace LegendQuest.Story;

/// <summary>
/// The paintings behind the story's big moments: each one's layers, back to front, and how much
/// each sways.
/// </summary>
/// <remarks>
/// Layered like the title: every layer's the title's canvas, 416x277 art pixels at 4x, and the
/// whole picture sways slowly with each layer moving its own share, so it reads as near and far.
/// Each layer is Content/Paintings/(id)-(layer).png, drawn by tools/generate_paintings.py.
/// </remarks>
public static class Paintings
{
    private static readonly Dictionary<string, IReadOnlyList<(string Layer, float Depth)>> Table = new()
    {
        // P-03: the pier in the storm where you met, the boat over, the two of you on the boards.
        ["pier-memory"] = new[] { ("sky", 0.1f), ("sea", 0.4f), ("pier", 1f) },

        // P-03: two teenagers cutting their palms under a red moon.
        ["red-moon"] = new[] { ("sky", 0.1f), ("moon", 0.2f), ("figures", 1f) },

        // P-05: Questeria rising out of the sea at dawn, from the ship's rail.
        ["questeria-dawn"] = new[] { ("sky", 0.1f), ("city", 0.45f), ("sea", 0.7f), ("rail", 1f) },
    };

    /// <summary>Whether there's a painting called this.</summary>
    /// <param name="id">What a script calls it.</param>
    public static bool Exists(string id) => id is not null && Table.ContainsKey(id);

    /// <summary>A painting's layers, back to front.</summary>
    /// <param name="id">Its id. It has to exist.</param>
    public static IReadOnlyList<(string Layer, float Depth)> Layers(string id) =>
        Table.TryGetValue(id, out var layers) ? layers : throw new InvalidOperationException($"There's no painting called {id}.");

    /// <summary>Every painting's id, for loading them all.</summary>
    public static IEnumerable<string> All => Table.Keys;
}
