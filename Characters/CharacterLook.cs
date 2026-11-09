using System;
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

    /// <summary>Which hat, the first being none. A hood hides the hair and ears under it.</summary>
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

    /// <summary>The painted face their portraits use (a face part's id, as PartCatalog.PlayerFaces lists them), or null for the paperdoll's. The paperdoll still draws them in a room until their armor's painted.</summary>
    public string Portrait { get; set; }

    /// <summary>The painted hair (a hair part's id, as PartCatalog.PlayerHairs lists them), what's on their head when nothing's worn there or the helm's hidden: hair and helm are one slot. Null for the face's own (the hair painted with it), and empty for none at all.</summary>
    public string HairPart { get; set; }

    /// <summary>The painted figure's skin: 0 for the face's own, or a number into Swatches.Skin, from 1.</summary>
    public int SkinTint { get; set; }

    /// <summary>The painted figure's hair, eyebrows and beard: 0 as painted, or a number into Swatches.Hair, from 1.</summary>
    public int HairTint { get; set; }

    /// <summary>The painted figure's eyes: 0 as painted, or a number into Swatches.Eyes, from 1.</summary>
    public int EyeTint { get; set; }

    /// <summary>The Base of whatever they wear, its biggest material: 0 as painted, or a number into Swatches.Cloth, from 1. Whatever they put on next is dyed it too.</summary>
    public int BaseTint { get; set; }

    /// <summary>The Trim of whatever they wear, its next material: 0 as painted, or a number into Swatches.Cloth, from 1.</summary>
    public int TrimTint { get; set; }

    /// <summary>Whether the painted figure leaves off the helm they're wearing, which still counts.</summary>
    public bool HideHelm { get; set; }

    /// <summary>Whether the painted figure leaves off the cape they're wearing, which still counts.</summary>
    public bool HideCape { get; set; }

    /// <summary>How much taller or shorter the painted figure stands than its recipe, in steps of HeightPerStep, from ShortestStep to TallestStep.</summary>
    public int HeightStep { get; set; }

    /// <summary>How much broader or slighter the painted figure is than its recipe, in steps of BuildPerStep, from SlightestStep to BroadestStep.</summary>
    public int BuildStep { get; set; }

    /// <summary>The steps HeightStep and BuildStep go in, and how far each goes: two in a hundred taller, four in a hundred broader.</summary>
    public const int ShortestStep = -4, TallestStep = 4, SlightestStep = -3, BroadestStep = 6;
    public const float HeightPerStep = 0.02f, BuildPerStep = 0.04f;

    /// <summary>How much taller and broader the painted figure is than its recipe, from HeightStep and BuildStep kept in their range.</summary>
    [JsonIgnore]
    public Morph PaintedMorph => new(
        1f + HeightPerStep * Math.Clamp(HeightStep, ShortestStep, TallestStep),
        1f + BuildPerStep * Math.Clamp(BuildStep, SlightestStep, BroadestStep));

    /// <summary>A copy, so changing one look doesn't change another.</summary>
    public CharacterLook Copy() => (CharacterLook)MemberwiseClone();

    /// <summary>Every number together, for keeping one baked sheet per look. Not saved; it's everything above.</summary>
    [JsonIgnore]
    public string Key => string.Join(",", Ancestry, Lineage, Skin, Face, Eyes, Hair, HairColour, Beard, Hat, Shirt, Vest, Legs, LegsColour, Cloak);
}
