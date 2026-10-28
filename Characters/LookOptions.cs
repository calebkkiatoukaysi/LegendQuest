using System;
using System.Collections.Generic;

namespace LegendQuest.Characters;

/// <summary>
/// Everything a look can be, by name, in the order CharacterLook numbers them.
/// </summary>
/// <remarks>
/// The colours come off character-palettes.png and the faces, hair, beards, skirt and cape off the
/// sheets in Content/Paperdoll, all made by tools/generate_paperdoll.py. A colour list here is as
/// long as its block of rows in the palette PNG (CharacterPalettes checks), and a list of layers
/// names its sheets the way the script saves them. The first of every colour list is the youth as
/// he was drawn.
/// </remarks>
public static class LookOptions
{
    /// <summary>The skins.</summary>
    public static IReadOnlyList<string> Skins { get; } = new[] { "FAIR", "PORCELAIN", "GOLDEN", "TAN", "BROWN", "DEEP" };

    /// <summary>The faces. The first is the body sheet's own; each after it is a sheet of its own, face-(name).</summary>
    public static IReadOnlyList<Option> Faces { get; } = new Option[]
    {
        new("BRIGHT", null), new("GENTLE", "face-gentle"), new("LASHED", "face-lashed"),
        new("STERN", "face-stern"), new("FRECKLED", "face-freckled"), new("SCARRED", "face-scarred"),
    };

    /// <summary>The eye colours.</summary>
    public static IReadOnlyList<string> Eyes { get; } = new[] { "BLUE", "BROWN", "GREEN", "AMBER", "GREY", "VIOLET" };

    /// <summary>The hair styles, each a sheet of its own.</summary>
    public static IReadOnlyList<Option> Hair { get; } = new Option[]
    {
        new("TOUSLED", "hair-tousled"), new("CROPPED", "hair-cropped"), new("LONG", "hair-long"),
        new("TIED BACK", "hair-tail"), new("BRAIDED", "hair-braid"),
    };

    /// <summary>The hair colours.</summary>
    public static IReadOnlyList<string> HairColours { get; } = new[]
    {
        "CHESTNUT", "BLACK", "GOLD", "COPPER", "SILVER", "MIDNIGHT", "ROSE", "LAVENDER",
    };

    /// <summary>The beards. The first is none.</summary>
    public static IReadOnlyList<Option> Beards { get; } = new Option[]
    {
        new("NONE", null), new("STUBBLE", "beard-stubble"), new("MOUSTACHE", "beard-moustache"),
        new("BEARD", "beard-beard"), new("FULL BEARD", "beard-full"),
    };

    /// <summary>The shirt dyes.</summary>
    public static IReadOnlyList<string> Shirts { get; } = new[] { "CREAM", "WHITE", "SKY", "BLUSH", "OAT" };

    /// <summary>The vest dyes.</summary>
    public static IReadOnlyList<string> Vests { get; } = new[]
    {
        "WOAD", "TEAL", "FOREST", "CRIMSON", "PLUM", "OCHRE", "CHARCOAL", "ROSE",
    };

    /// <summary>Breeches, which are the body sheet's own, or the skirt over them.</summary>
    public static IReadOnlyList<Option> Legs { get; } = new Option[] { new("BREECHES", null), new("SKIRT", "skirt") };

    /// <summary>The dyes for the breeches or the skirt.</summary>
    public static IReadOnlyList<string> LegsColours { get; } = new[] { "BARK", "CHARCOAL", "NAVY", "OLIVE", "SAND", "WINE" };

    /// <summary>The cloaks. The first is none, and each after it is that cloak dye on the cape.</summary>
    public static IReadOnlyList<string> Cloaks { get; } = new[]
    {
        "NONE", "WOAD", "CRIMSON", "TEAL", "CHARCOAL", "OCHRE", "LAVENDER",
    };

    /// <summary>Which of a list a number means, wrapped round so a number off the end still means something.</summary>
    /// <param name="index">The number.</param>
    /// <param name="count">How long the list is.</param>
    public static int Wrap(int index, int count) => (index % count + count) % count;

    /// <summary>A look picked at random, for the creation screen's RANDOM.</summary>
    /// <param name="random">Where the numbers come from.</param>
    public static CharacterLook Random(Random random) => new()
    {
        Skin = random.Next(Skins.Count),
        Face = random.Next(Faces.Count),
        Eyes = random.Next(Eyes.Count),
        Hair = random.Next(Hair.Count),
        HairColour = random.Next(HairColours.Count),
        // Most people don't have a beard.
        Beard = random.Next(3) == 0 ? random.Next(1, Beards.Count) : 0,
        Shirt = random.Next(Shirts.Count),
        Vest = random.Next(Vests.Count),
        Legs = random.Next(Legs.Count),
        LegsColour = random.Next(LegsColours.Count),
        Cloak = random.Next(2) == 0 ? 0 : random.Next(1, Cloaks.Count),
    };

    /// <summary>One choice that comes with a sheet of its own (or doesn't, for the first of some): what the screen calls it and what the sheet's called in Content/Paperdoll.</summary>
    /// <param name="Name">What the screen calls it.</param>
    /// <param name="Sheet">Its sheet, or null for none.</param>
    public sealed record Option(string Name, string Sheet);
}
