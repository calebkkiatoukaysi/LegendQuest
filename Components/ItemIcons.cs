using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Items;

namespace LegendQuest.Components;

/// <summary>
/// The item pictures and the slots they sit in: Content/Items/icons.png's icons, and slots.png's
/// wells, the faint outline an empty equipment slot shows of what goes in it, and the rims an
/// uncommon or a rare thing gets.
/// </summary>
/// <remarks>
/// Both sheets are 24 by 24 art pixel cells, blown up three times like the rest of the UI. A slot is
/// drawn in layers, in this order: the well (plain, lit, or picked out in gold), then the outline if
/// it's an empty equipment slot or the rim if what's in it is magic, then the icon. tools/generate_items.py
/// draws both sheets, and its SLOTS has to match the cells here. Goes in a point sampled, deferred
/// batch, so the layers land in the order they're drawn.
/// </remarks>
public class ItemIcons
{
    /// <summary>One cell of either sheet, in art pixels. Has to match ICON in tools/generate_items.py.</summary>
    private const int Cell = 24;

    /// <summary>Icons to a row of icons.png. Has to match COLUMNS in tools/generate_items.py.</summary>
    private const int IconColumns = 16;

    /// <summary>How much the art is blown up: the same as the buttons and the world.</summary>
    private const int Zoom = 3;

    /// <summary>Where the empty slots' outlines start in slots.png: after the three wells, one for each EquipmentSlot in its order.</summary>
    private const int FirstOutline = 3;

    /// <summary>A slot's size on screen, in screen pixels.</summary>
    public const int Size = Cell * Zoom;

    /// <summary>Where the rims start in slots.png, after the outlines: uncommon, then rare.</summary>
    private static readonly int FirstRim = FirstOutline + Enum.GetValues<EquipmentSlot>().Length;

    private Texture2D _icons;
    private Texture2D _slots;

    /// <summary>Loads both sheets.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _icons = content.Load<Texture2D>("Items/icons");
        _slots = content.Load<Texture2D>("Items/slots");
    }

    /// <summary>Draws the well a slot is, sunk into the panel.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled, deferred.</param>
    /// <param name="slot">Where, in screen pixels: Size square.</param>
    /// <param name="look">Plain, lit, or picked out in gold.</param>
    /// <param name="alpha">How far it's faded in, 0 to 1.</param>
    public void DrawWell(SpriteBatch spriteBatch, Rectangle slot, WellLook look, float alpha) =>
        DrawCell(spriteBatch, _slots, (int)look, 0, slot, alpha);

    /// <summary>Draws the faint outline of what goes in an empty equipment slot, over its well.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled, deferred.</param>
    /// <param name="slot">Where, in screen pixels.</param>
    /// <param name="equipment">Which slot it is.</param>
    /// <param name="alpha">How far it's faded in, 0 to 1.</param>
    public void DrawOutline(SpriteBatch spriteBatch, Rectangle slot, EquipmentSlot equipment, float alpha) =>
        DrawCell(spriteBatch, _slots, FirstOutline + (int)equipment, 0, slot, alpha);

    /// <summary>Draws an item over its well: the rim for its rarity if it's magic, then its icon.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled, deferred.</param>
    /// <param name="slot">Where, in screen pixels.</param>
    /// <param name="item">The item.</param>
    /// <param name="alpha">How far it's faded in, 0 to 1. Lower to grey out something that can't be had.</param>
    public void DrawItem(SpriteBatch spriteBatch, Rectangle slot, ItemDefinition item, float alpha)
    {
        if (item.Rarity != Rarity.Common) DrawCell(spriteBatch, _slots, FirstRim + (int)item.Rarity - 1, 0, slot, alpha);
        DrawCell(spriteBatch, _icons, item.Icon % IconColumns, item.Icon / IconColumns, slot, alpha);
    }

    /// <summary>Draws one cell of a sheet over a slot.</summary>
    private static void DrawCell(SpriteBatch spriteBatch, Texture2D sheet, int column, int row, Rectangle slot, float alpha) =>
        spriteBatch.Draw(sheet, slot, new Rectangle(column * Cell, row * Cell, Cell, Cell), Color.White * alpha);
}
