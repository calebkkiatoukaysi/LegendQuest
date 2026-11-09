using System.Collections.Generic;

namespace LegendQuest.Characters;

/// <summary>
/// Content/Characters/Parts/parts.json as tools/import_parts.py writes it: the scale the parts were
/// cut at, the sockets and their slots, the atlas pages, each base, every part and every recipe.
/// </summary>
/// <remarks>Read once as the game starts, by PartCatalog.LoadAll, which checks every number in it.</remarks>
/// <param name="Version">Which layout the file has. PartCatalog reads FileVersion.</param>
/// <param name="Scale">How big the parts are against the bases' 1024 by 1536 canvas.</param>
/// <param name="StandardHeight">How tall a standard adult stands in the parts' pixels, crown to soles.</param>
/// <param name="Canvas">The bases' canvas at Scale: wide and tall.</param>
/// <param name="Sockets">Every socket, as the importer knows them.</param>
/// <param name="SlotSockets">The socket each slot's parts hang from, as the importer knows it.</param>
/// <param name="Order">The slots back to front, as the importer knows them.</param>
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
    Dictionary<string, int[]> Pages,
    Dictionary<string, PartsFile.BaseEntry> Bases,
    Dictionary<string, PartsFile.PartEntry> Parts,
    Dictionary<string, PartsFile.RecipeEntry> Recipes,
    PartsFile.GearEntry[] Gear,
    Dictionary<string, PartsFile.OwnEntry[]> Own)
{
    /// <summary>A base: its sockets and head box in the parts' pixels on the scaled canvas, and the ids of its body, its bald head and its fists.</summary>
    /// <param name="Sockets">Every socket, across and down.</param>
    /// <param name="Head">The box round the head, crown to chin: left, top, width, height. The portraits crop round it.</param>
    /// <param name="Body">The id of its body from the jaw down.</param>
    /// <param name="Bald">The id of its bald head.</param>
    /// <param name="Skin">The colour of its skin on the cheeks, 0 to 255: what a skin picked for a painted figure is against.</param>
    /// <param name="Hands">The ids of its two fists, "near" and "far": masks on the body's canvas, each fist exactly. What a figure holds is drawn under the fist that holds it.</param>
    public sealed record BaseEntry(Dictionary<string, float[]> Sockets, float[] Head, string Body, string Bald, float[] Skin, Dictionary<string, string> Hands);

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

    /// <summary>A part: its slot, its base, the socket it hangs from, where it is in the atlas, its pivot, its mask, and for a face its skin.</summary>
    /// <param name="Slot">Its slot, by name.</param>
    /// <param name="Base">The base it was painted on.</param>
    /// <param name="Socket">The socket it hangs from.</param>
    /// <param name="Page">The atlas page it's on.</param>
    /// <param name="Rect">Where it is on the page: left, top, width, height.</param>
    /// <param name="Pivot">Its socket in its own pixels: across and down from its top left.</param>
    /// <param name="MaskRect">Where its mask is on the page, the same size as Rect, or null for none: red its skin, green and blue its two dye channels (an armor's Base and Trim, a face's hair and eyes).</param>
    /// <param name="Skin">For a face, its skin against the base's, red, green and blue; null otherwise.</param>
    /// <param name="Tones">The colour each dye channel was painted, 0 to 255, or null for a channel it hasn't; null for a part with no dye channels.</param>
    public sealed record PartEntry(string Slot, string Base, string Socket, string Page, int[] Rect, float[] Pivot, int[] MaskRect, float[] Skin, float[][] Tones);

    /// <summary>A recipe: the part in each slot (null for none) and the morph.</summary>
    /// <param name="Face">The face.</param>
    /// <param name="Armor">The armor.</param>
    /// <param name="Cape">The cape.</param>
    /// <param name="Helm">What's on their head, a hair or a helm (one slot), or null for nothing.</param>
    /// <param name="Weapon">The weapon.</param>
    /// <param name="Morph">Height, then width.</param>
    public sealed record RecipeEntry(string Face, string Armor, string Cape, string Helm, string Weapon, float[] Morph);
}
