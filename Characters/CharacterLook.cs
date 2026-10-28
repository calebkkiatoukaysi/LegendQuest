using System.Text.Json.Serialization;

namespace LegendQuest.Characters;

/// <summary>
/// How someone looks: their people, skin, face, eyes, hair and beard, their hat, and the colours of
/// what they're wearing. Every one but the people is a number into one of LookOptions' lists.
/// </summary>
/// <remarks>
/// Saved as numbers, not colours, the same as The Black Box's PlayerLook, so I can tweak a preset
/// later without breaking anyone's save. A number that's since gone off the end of its list wraps
/// round. The paperdoll's armour comes on top of this later; this is the person under it.
/// </remarks>
public class CharacterLook
{
    /// <summary>Who their people are.</summary>
    public Ancestry Ancestry { get; set; }

    /// <summary>Which line of their people: an Emberkin's horns, or a Wildkin's beast. Ignored for the rest.</summary>
    public int Lineage { get; set; }

    /// <summary>Which skin.</summary>
    public int Skin { get; set; }

    /// <summary>Which face. The first is the face the body sheet's drawn with.</summary>
    public int Face { get; set; }

    /// <summary>Which eye colour.</summary>
    public int Eyes { get; set; }

    /// <summary>Which hair style.</summary>
    public int Hair { get; set; }

    /// <summary>Which hair colour. Beards come out the same colour.</summary>
    public int HairColour { get; set; }

    /// <summary>Which beard, the first being none.</summary>
    public int Beard { get; set; }

    /// <summary>Which hat, the first being none.</summary>
    public int Hat { get; set; }

    /// <summary>Which shirt dye.</summary>
    public int Shirt { get; set; }

    /// <summary>Which dye the vest over the shirt is.</summary>
    public int Vest { get; set; }

    /// <summary>Breeches or a skirt.</summary>
    public int Legs { get; set; }

    /// <summary>Which dye the breeches or the skirt are.</summary>
    public int LegsColour { get; set; }

    /// <summary>Which cloak, the first being none.</summary>
    public int Cloak { get; set; }

    /// <summary>A copy, so changing one look doesn't change another.</summary>
    public CharacterLook Copy() => (CharacterLook)MemberwiseClone();

    /// <summary>Every number together, for keeping one baked sheet per look. Not saved; it's everything above.</summary>
    [JsonIgnore]
    public string Key => string.Join(",", Ancestry, Lineage, Skin, Face, Eyes, Hair, HairColour, Beard, Hat, Shirt, Vest, Legs, LegsColour, Cloak);
}
