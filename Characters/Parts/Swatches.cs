using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>
/// The colours the painted characters can be dyed: skin, hair and eyes for a face, and Base and Trim
/// for what they wear. A look keeps a number into each list, 0 for the colour it was painted.
/// </summary>
/// <remarks>
/// Each part's mask (tools/import_parts.py) says where its skin, hair, eyes, Base and Trim are and the
/// colour each was painted; a dye maps that colour onto the swatch and shades the rest of the
/// channel as it was (RigComposer.Dye), so the cel shading and the linework stay. Skin is tinted by
/// ratio instead, as the faces' own skins already are, so it keeps every warm shadow.
/// </remarks>
public static class Swatches
{
    /// <summary>One swatch: its name on the creation screen and its colour.</summary>
    /// <param name="Name">Its name, in capitals.</param>
    /// <param name="Colour">Its colour, opaque.</param>
    public sealed record Swatch(string Name, Color Colour);

    /// <summary>Skin tones, palest to deepest, then the Stoneborn's grey and the Emberkin's ember.</summary>
    public static IReadOnlyList<Swatch> Skin { get; } = new Swatch[]
    {
        new("PORCELAIN", new Color(250, 222, 206)), new("FAIR", new Color(243, 204, 178)), new("PEACH", new Color(236, 188, 152)),
        new("GOLDEN", new Color(226, 176, 124)), new("TAN", new Color(210, 152, 104)), new("OLIVE", new Color(190, 146, 100)),
        new("BRONZE", new Color(166, 112, 74)), new("BROWN", new Color(132, 86, 58)), new("UMBER", new Color(102, 66, 46)),
        new("DEEP", new Color(78, 50, 36)), new("STONE", new Color(168, 160, 158)), new("EMBER", new Color(196, 96, 72)),
    };

    /// <summary>Hair colours, the natural ones first.</summary>
    public static IReadOnlyList<Swatch> Hair { get; } = new Swatch[]
    {
        new("BLACK", new Color(34, 30, 34)), new("DARK BROWN", new Color(68, 44, 30)), new("CHESTNUT", new Color(118, 70, 40)),
        new("AUBURN", new Color(148, 62, 36)), new("COPPER", new Color(194, 98, 46)), new("HONEY", new Color(196, 142, 72)),
        new("GOLD", new Color(226, 184, 96)), new("PLATINUM", new Color(236, 226, 200)), new("SILVER", new Color(196, 200, 210)),
        new("WHITE", new Color(244, 244, 248)), new("ROSE", new Color(228, 122, 164)), new("LAVENDER", new Color(172, 140, 222)),
        new("MIDNIGHT", new Color(42, 56, 112)), new("TEAL", new Color(40, 142, 150)), new("CRIMSON", new Color(170, 30, 46)),
    };

    /// <summary>Eye colours.</summary>
    public static IReadOnlyList<Swatch> Eyes { get; } = new Swatch[]
    {
        new("BLUE", new Color(60, 120, 210)), new("SKY", new Color(110, 180, 236)), new("GREEN", new Color(66, 150, 84)),
        new("HAZEL", new Color(140, 120, 60)), new("BROWN", new Color(110, 66, 36)), new("AMBER", new Color(214, 140, 40)),
        new("GREY", new Color(140, 150, 160)), new("VIOLET", new Color(150, 90, 210)), new("RED", new Color(196, 40, 48)),
        new("GOLD", new Color(232, 192, 70)),
    };

    /// <summary>Cloth and metal for Base and Trim.</summary>
    public static IReadOnlyList<Swatch> Cloth { get; } = new Swatch[]
    {
        new("CRIMSON", new Color(150, 28, 40)), new("SCARLET", new Color(206, 46, 44)), new("EMBER", new Color(214, 96, 40)),
        new("OCHRE", new Color(200, 146, 50)), new("GOLD", new Color(222, 176, 70)), new("OLIVE", new Color(118, 120, 52)),
        new("FOREST", new Color(40, 98, 58)), new("MOSS", new Color(96, 140, 70)), new("TEAL", new Color(30, 128, 134)),
        new("SKY", new Color(96, 160, 220)), new("WOAD", new Color(52, 86, 164)), new("NAVY", new Color(36, 52, 104)),
        new("VIOLET", new Color(110, 64, 170)), new("PLUM", new Color(110, 40, 92)), new("ROSE", new Color(214, 110, 150)),
        new("IVORY", new Color(232, 222, 196)), new("SAND", new Color(196, 170, 128)), new("BROWN", new Color(110, 72, 44)),
        new("ASH", new Color(150, 150, 156)), new("CHARCOAL", new Color(58, 58, 66)), new("SILVER", new Color(200, 204, 214)),
        new("BRASS", new Color(184, 150, 80)),
    };

    /// <summary>The colour a look's number picks from a list: null for 0, which is as painted, and the nth swatch for n, wrapping round.</summary>
    /// <param name="list">The list.</param>
    /// <param name="number">The look's number.</param>
    public static Color? Pick(IReadOnlyList<Swatch> list, int number) => number <= 0 ? null : list[(number - 1) % list.Count].Colour;

    /// <summary>A swatch's name for a look's number: AS PAINTED for 0.</summary>
    /// <param name="list">The list.</param>
    /// <param name="number">The look's number.</param>
    public static string NameOf(IReadOnlyList<Swatch> list, int number) => number <= 0 ? "AS PAINTED" : list[(number - 1) % list.Count].Name;

    /// <summary>A colour as six hex digits, or a dash for none: for an outfit's Key.</summary>
    /// <param name="colour">The colour, or null.</param>
    public static string Hex(Color? colour) =>
        colour is Color c ? c.R.ToString("x2", CultureInfo.InvariantCulture) + c.G.ToString("x2", CultureInfo.InvariantCulture) + c.B.ToString("x2", CultureInfo.InvariantCulture) : "-";
}

/// <summary>The colours a painted figure is dyed: each null to leave it as it was painted.</summary>
/// <param name="Skin">The skin, body and face.</param>
/// <param name="Hair">The hair, eyebrows and beard.</param>
/// <param name="Eyes">The eyes.</param>
/// <param name="Base">What they wear's biggest material.</param>
/// <param name="Trim">What they wear's next.</param>
public readonly record struct Tints(Color? Skin, Color? Hair, Color? Eyes, Color? Base, Color? Trim)
{
    /// <summary>The colours a look picks.</summary>
    /// <param name="look">The look.</param>
    public static Tints Of(CharacterLook look) => look is null ? default : new Tints(
        Swatches.Pick(Swatches.Skin, look.SkinTint), Swatches.Pick(Swatches.Hair, look.HairTint), Swatches.Pick(Swatches.Eyes, look.EyeTint),
        Swatches.Pick(Swatches.Cloth, look.BaseTint), Swatches.Pick(Swatches.Cloth, look.TrimTint));

    /// <summary>All five, for an outfit's Key.</summary>
    public string Key => string.Join(",", Swatches.Hex(Skin), Swatches.Hex(Hair), Swatches.Hex(Eyes), Swatches.Hex(Base), Swatches.Hex(Trim));

    /// <summary>Whether it leaves everything as painted.</summary>
    public bool None => Skin is null && Hair is null && Eyes is null && Base is null && Trim is null;
}
