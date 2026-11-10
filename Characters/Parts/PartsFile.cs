using System.Collections.Generic;

namespace LegendQuest.Characters;

/// <summary>
/// Content/Characters/Parts/parts.json as tools/import_parts.py writes it: the scale the parts were
/// cut at, the sockets and their slots, the skeleton, the atlas pages, each base, every part and
/// every recipe.
/// </summary>
/// <remarks>Read once as the game starts, by PartCatalog.LoadAll, which checks every number in it.</remarks>
/// <param name="Version">Which layout the file has. PartCatalog reads FileVersion.</param>
/// <param name="Scale">How big the parts are against the bases' 1024 by 1536 canvas.</param>
/// <param name="StandardHeight">How tall a standard adult stands in the parts' pixels, crown to soles.</param>
/// <param name="Canvas">The bases' canvas at Scale: wide and tall.</param>
/// <param name="Sockets">Every socket, as the importer knows them.</param>
/// <param name="SlotSockets">The socket each slot's parts hang from, as the importer knows it.</param>
/// <param name="Order">The slots back to front, as the importer knows them.</param>
/// <param name="Bones">The skeleton's bones, parents first, each as its name, its parent's (null for the root) and the joint it turns about (tools/rig/skeleton.py).</param>
/// <param name="Draw">The bones' pieces back to front, as they're drawn at rest.</param>
/// <param name="Pages">Every atlas page by name (Content/Characters/Parts/(name).png), and its size.</param>
/// <param name="Bases">Every base by id.</param>
/// <param name="Parts">Every part by id.</param>
/// <param name="Recipes">Every character's recipe by id, from Story/13-art-prompts.md §7.</param>
/// <param name="Gear">What each item draws on each base, from §7.1.</param>
/// <param name="Own">The party's own pieces for their starting kit, by recipe id, from §7.2.</param>
public sealed record PartsFile(
    int Version,
    float Scale,
    float StandardHeight,
    int[] Canvas,
    string[] Sockets,
    Dictionary<string, string> SlotSockets,
    string[] Order,
    string[][] Bones,
    string[] Draw,
    Dictionary<string, int[]> Pages,
    Dictionary<string, PartsFile.BaseEntry> Bases,
    Dictionary<string, PartsFile.PartEntry> Parts,
    Dictionary<string, PartsFile.RecipeEntry> Recipes,
    PartsFile.GearEntry[] Gear,
    Dictionary<string, PartsFile.OwnEntry[]> Own)
{
    /// <summary>A base: its sockets, head box and joints in the parts' pixels on the scaled canvas, and the ids of its body and its bald head.</summary>
    /// <param name="Sockets">Every socket, across and down.</param>
    /// <param name="Head">The box round the head, crown to chin: left, top, width, height. The portraits crop round it.</param>
    /// <param name="Body">The id of its body from the jaw down.</param>
    /// <param name="Bald">The id of its bald head.</param>
    /// <param name="Skin">The colour of its skin on the cheeks, 0 to 255: what a skin picked for a painted figure is against.</param>
    /// <param name="Joints">Every joint of the skeleton, across and down: the middle of each painted ball.</param>
    /// <param name="RigPx">How many of the parts' pixels one of the clips' units is (they're written for a figure 161.5 units tall).</param>
    public sealed record BaseEntry(Dictionary<string, float[]> Sockets, float[] Head, string Body, string Bald, float[] Skin, Dictionary<string, float[]> Joints, float RigPx);

    /// <summary>An item and what it draws in one equipment slot: a part on each base, or null for nothing there.</summary>
    /// <param name="Item">The item's id in ItemCatalog.</param>
    /// <param name="Slot">The equipment slot: body, cloak, head, mainhand or offhand.</param>
    /// <param name="Parts">The part it draws, by base id. Null on a base where it draws nothing.</param>
    public sealed record GearEntry(string Item, string Slot, Dictionary<string, string> Parts);

    /// <summary>One of a character's starting items, and the piece of their own it's drawn as.</summary>
    /// <param name="Slot">The equipment slot it's worn in.</param>
    /// <param name="Item">The item's id in ItemCatalog.</param>
    /// <param name="Part">The part it's drawn as, or null for nothing: another piece holds it.</param>
    public sealed record OwnEntry(string Slot, string Item, string Part);

    /// <summary>A part: its slot, its base, the socket it hangs from, where it is in the atlas (whole, or in its bones' pieces), its pivot, its mask, and for a face its skin.</summary>
    /// <param name="Slot">Its slot, by name.</param>
    /// <param name="Base">The base it was painted on.</param>
    /// <param name="Socket">The socket it hangs from.</param>
    /// <param name="Page">The atlas page it's on; null for a part in pieces.</param>
    /// <param name="Rect">Where it is on the page: left, top, width, height; null for a part in pieces.</param>
    /// <param name="Pivot">Its socket in its own pixels: across and down from its top left.</param>
    /// <param name="MaskRect">Where its mask is on the page, the same size as Rect, or null for none: red its skin, green and blue its two dye channels (an armor's Base and Trim, a face's hair and eyes).</param>
    /// <param name="Skin">For a face, its skin against the base's, red, green and blue; null otherwise.</param>
    /// <param name="Tones">The colour each dye channel was painted, 0 to 255, or null for a channel it hasn't; null for a part with no dye channels.</param>
    /// <param name="Pieces">A body's or an armor's pieces by bone, each where it is in the atlas: null for a part that's whole.</param>
    public sealed record PartEntry(string Slot, string Base, string Socket, string Page, int[] Rect, float[] Pivot, int[] MaskRect, float[] Skin, float[][] Tones,
        Dictionary<string, PieceEntry> Pieces);

    /// <summary>One bone's piece of a part: where it is in the atlas, its part's socket in its own pixels, and its mask.</summary>
    /// <param name="Page">The atlas page it's on.</param>
    /// <param name="Rect">Where it is on the page: left, top, width, height.</param>
    /// <param name="Pivot">Its part's socket in its own pixels: across and down from its top left.</param>
    /// <param name="MaskRect">Where its mask is on the page, the same size as Rect, or null for none.</param>
    public sealed record PieceEntry(string Page, int[] Rect, float[] Pivot, int[] MaskRect);

    /// <summary>A recipe: the part in each slot (null for none) and the morph.</summary>
    /// <param name="Face">The face.</param>
    /// <param name="Armor">The armor.</param>
    /// <param name="Cape">The cape.</param>
    /// <param name="Helm">What's on their head, a hair or a helm (one slot), or null for nothing.</param>
    /// <param name="Weapon">The weapon.</param>
    /// <param name="Morph">Height, then width.</param>
    /// <param name="Base">The base its face and armor are painted on, made yet or not: till its face is, someone is that base's bare mannequin.</param>
    /// <param name="Tints">What its face and body are recoloured to, by swatch name (Swatches): "skin", "hair" and "eyes", any of them; the faces are shared, so this is what tells two who wear one apart.</param>
    /// <param name="Extras">Its facial parts and marks (a beard, earrings, a scar, pointed ears, horns), any number; null or empty for none.</param>
    public sealed record RecipeEntry(string Face, string Armor, string Cape, string Helm, string Weapon, float[] Morph, string Base,
        Dictionary<string, string> Tints = null, string[] Extras = null);
}
