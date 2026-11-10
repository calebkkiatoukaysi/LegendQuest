using System.Collections.Generic;
using System.Globalization;

namespace LegendQuest.Characters;

/// <summary>
/// Everything one figure is put together from: the face, the part in each slot, and the morph. A
/// recipe makes one (Of), and what someone in the party is wearing changes it (PartCatalog.Dress).
/// </summary>
/// <remarks>
/// Two outfits with the same parts and morph make the same picture, so PartCatalog keeps one figure
/// per Key and shares it: everyone in the same kit looks the same until their faces differ.
/// </remarks>
/// <param name="Name">Whose it is, for the picture's id and for anything that goes wrong putting it together: a recipe id.</param>
/// <param name="Face">The face part, which decides the base.</param>
/// <param name="Armor">The armor part, or null for none.</param>
/// <param name="Cape">The cape part, or null for none.</param>
/// <param name="Helm">What's on the head, a hair or a helm (the one slot for both), or null for nothing.</param>
/// <param name="Weapon">The part in the near hand, or null for nothing.</param>
/// <param name="Offhand">The part in the far hand, or null for nothing.</param>
/// <param name="Morph">How much taller and broader than the base.</param>
/// <param name="Tints">The colours it's dyed.</param>
public sealed record Outfit(string Name, string Face, string Armor, string Cape, string Helm, string Weapon, string Offhand, Morph Morph, Tints Tints = default)
{
    /// <summary>Its facial parts and marks, in the order they're drawn within their slots: a beard, earrings, a scar, pointed ears, horns.</summary>
    public IReadOnlyList<string> Extras { get; init; } = System.Array.Empty<string>();

    /// <summary>A recipe's outfit, with another face if one's given.</summary>
    /// <param name="name">The recipe's id.</param>
    /// <param name="recipe">The recipe.</param>
    /// <param name="face">The face to put on it instead of its own, or null for its own.</param>
    public static Outfit Of(string name, PartsFile.RecipeEntry recipe, string face = null) =>
        new(name, face ?? recipe.Face, recipe.Armor, recipe.Cape, recipe.Helm, recipe.Weapon, null, new Morph(recipe.Morph[0], recipe.Morph[1]), Tints.Named(recipe.Tints)) { Base = recipe.Base, Extras = recipe.Extras ?? System.Array.Empty<string>() };

    /// <summary>The base its recipe is on, made yet or not: till its face is painted, it's that base's bare mannequin.</summary>
    public string Base { get; init; }

    /// <summary>The hair that goes with a face: the one split from it (hair-x for face-x), which the importer makes from a face painted with its hair.</summary>
    /// <param name="face">The face's id.</param>
    public static string HairOf(string face) => face is not null && face.StartsWith("face-", System.StringComparison.Ordinal) ? "hair-" + face["face-".Length..] : null;

    /// <summary>The part an outfit has in a slot: the face, the armor, the cape, the helm or a hand's.</summary>
    /// <param name="slot">The slot.</param>
    /// <returns>The part's id, or null for none (and for the body and the bald head, which are the base's).</returns>
    public string In(PartSlot slot) => slot switch
    {
        PartSlot.Face => Face,
        PartSlot.Armor => Armor,
        PartSlot.Cape => Cape,
        PartSlot.Helm => Helm,
        PartSlot.Weapon => Weapon,
        PartSlot.Offhand => Offhand,
        _ => null,
    };

    /// <summary>Everything that changes the picture, together: the parts, the morph and the dyes. Not the name.</summary>
    public string Key => string.Join("|", Base, Face, Armor, Cape, Helm, Weapon, Offhand, string.Join("+", Extras),
        Morph.Height.ToString("R", CultureInfo.InvariantCulture), Morph.Width.ToString("R", CultureInfo.InvariantCulture), Tints.Key);

    /// <summary>The part slot an equipment slot's item fills: body armour the armor, a cloak the cape, a head item the helm, and the hands' theirs. Null for gloves, boots, amulets and rings, which draw nothing.</summary>
    /// <param name="slot">The equipment slot.</param>
    public static PartSlot? SlotFor(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Body => PartSlot.Armor,
        EquipmentSlot.Cloak => PartSlot.Cape,
        EquipmentSlot.Head => PartSlot.Helm,
        EquipmentSlot.MainHand => PartSlot.Weapon,
        EquipmentSlot.OffHand => PartSlot.Offhand,
        _ => null,
    };
}
