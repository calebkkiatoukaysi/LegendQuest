using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Story;

/// <summary>
/// The world tints a scene can put over a map with a tint cue: the light changing, for the
/// Mirewood's twilight, the night, the eclipse's stages, stopped time.
/// </summary>
/// <remarks>
/// The list is 03b-world-art.md §5's, each a colour the map and everyone on it are multiplied by,
/// the same as a map's own tint, with the art's strength already in it: night is a deep blue at
/// 55%, so it's that blue taken 55% of the way from white. Twilight is the Mirewood's own tint. A
/// multiply can't take colour out, so ash is a flat grey rather than half its colour gone; fire's
/// flicker and flicker-day-night's turning from day to night hold still, at a fire's orange and at
/// night, until the painted rooms bring the light that moves.
/// </remarks>
public static class Tints
{
    private static readonly Dictionary<string, Color> Table = new()
    {
        ["day"] = Color.White,
        ["daylight"] = new Color(255, 248, 234),
        ["dawn"] = new Color(255, 214, 198),
        ["twilight"] = new Color(196, 188, 226),
        ["night"] = new Color(118, 128, 178),
        ["moonlight"] = new Color(172, 182, 214),
        ["snow"] = new Color(228, 236, 246),
        ["snow-night"] = new Color(150, 160, 198),
        ["candle"] = new Color(255, 222, 182),
        ["torches-dim"] = new Color(206, 160, 118),
        ["fire"] = new Color(255, 192, 136),
        ["cave"] = new Color(150, 170, 152),
        ["ash"] = new Color(184, 182, 178),
        ["ember-sun"] = new Color(255, 172, 132),
        ["eclipse"] = new Color(170, 170, 192),
        ["eclipse-1"] = new Color(222, 222, 230),
        ["eclipse-2"] = new Color(192, 192, 208),
        ["eclipse-3"] = new Color(158, 158, 182),
        ["eclipse-4"] = new Color(112, 112, 142),
        ["eclipse-5"] = new Color(40, 40, 56),
        ["black"] = new Color(28, 26, 34),
        ["starless"] = new Color(40, 46, 72),
        ["violet-haze"] = new Color(218, 201, 246),
        ["flicker-day-night"] = new Color(118, 128, 178),
    };

    /// <summary>What a tint cue says to take a tint off, back to the map's own.</summary>
    public const string Off = "off";

    /// <summary>Whether there's a tint called this.</summary>
    /// <param name="id">What a script calls it.</param>
    public static bool Exists(string id) => id is not null && Table.ContainsKey(id);

    /// <summary>A tint's colour.</summary>
    /// <param name="id">Its id. It has to exist.</param>
    public static Color Get(string id) =>
        Table.TryGetValue(id, out Color colour) ? colour : throw new InvalidOperationException($"There's no tint called {id}.");
}
