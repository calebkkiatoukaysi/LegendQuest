using System.Text.Json.Serialization;

namespace LegendQuest.Characters;

/// <summary>
/// How someone looks, as far as the body sheet goes: their skin, and the colours of their shirt and
/// breeches. Each is a preset from character-palettes.png.
/// </summary>
/// <remarks>
/// Saved as preset numbers, not colours, the same as The Black Box's PlayerLook, so I can tweak a
/// preset later without breaking anyone's save. Hair, eyes and the clothes over the smallclothes
/// come with the paperdoll.
/// </remarks>
public class CharacterLook
{
    /// <summary>Which skin preset.</summary>
    public int Skin { get; set; }

    /// <summary>Which shirt dye.</summary>
    public int Shirt { get; set; }

    /// <summary>Which breeches dye.</summary>
    public int Breeches { get; set; }

    /// <summary>The three numbers together, for keeping one recoloured sheet per look. Not saved; it's the three above.</summary>
    [JsonIgnore]
    public (int Skin, int Shirt, int Breeches) Key => (Skin, Shirt, Breeches);
}
