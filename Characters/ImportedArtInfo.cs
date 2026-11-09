using System;

namespace LegendQuest.Characters;

/// <summary>
/// What tools/import_character.py wrote beside one imported picture, in its .json: how big the
/// picture is, where the feet are in it, the box round the head for the portraits, the build and
/// the kind it was cut as, and how big the picture from ChatGPT was before it was cut.
/// </summary>
/// <remarks>
/// Every number is in the picture's own pixels, where a standard adult is ImportedArt.FileHeight
/// tall from the top of the head to the feet. The feet are the middle of the soles, so the picture
/// is drawn from there. Read once, as the game starts, and checked then.
/// </remarks>
/// <param name="Id">Whose picture it is, which is the file's name as well.</param>
/// <param name="Build">The build it was scaled as: small, stout, standard, lean or big.</param>
/// <param name="Kind">Whether it's a person or a creature.</param>
/// <param name="Size">How wide and tall the picture is.</param>
/// <param name="Feet">Where the feet are: across, and down to the bottom of the soles.</param>
/// <param name="Head">The box round the head, from the top of it to the neck: left, top, width, height.</param>
/// <param name="Source">How wide and tall the picture from ChatGPT was.</param>
public sealed record ImportedArtInfo(string Id, string Build, string Kind, int[] Size, float[] Feet, int[] Head, int[] Source)
{
    /// <summary>The builds a picture can be cut as.</summary>
    private static readonly string[] Builds = { "small", "stout", "standard", "lean", "big" };

    /// <summary>Whether it's a creature, whose shadow goes by its length rather than its height.</summary>
    public bool Creature => Kind == "creature";

    /// <summary>What's wrong with it, or null if it's well formed: a build and a kind there are, a size, feet inside the picture, a head box with some size inside it too, and the size of the picture it came from.</summary>
    public string Problem()
    {
        if (Array.IndexOf(Builds, Build) < 0) return $"has the build {Build}, which isn't one of {string.Join(", ", Builds)}";
        if (Kind is not ("person" or "creature")) return $"is a {Kind}, not a person or a creature";
        if (Size is not { Length: 2 } || Size[0] <= 0 || Size[1] <= 0) return "has no size";
        if (Feet is not { Length: 2 } || Feet[0] < 0f || Feet[0] > Size[0] || Feet[1] <= 0f || Feet[1] > Size[1])
            return "has its feet outside the picture";
        if (Head is not { Length: 4 } || Head[2] <= 0 || Head[3] <= 0 || Head[0] < 0 || Head[1] < 0 || Head[0] + Head[2] > Size[0] || Head[1] + Head[3] > Size[1])
            return "has a head box that's nothing or isn't inside the picture";
        if (Source is not { Length: 2 } || Source[0] <= 0 || Source[1] <= 0) return "doesn't say how big the picture it came from was";
        return null;
    }
}
