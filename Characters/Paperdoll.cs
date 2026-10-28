using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters;

/// <summary>
/// Bakes someone's look into one sheet: every layer it calls for drawn over the body in order, then
/// recoloured to their presets.
/// </summary>
/// <remarks>
/// Once per change, not per frame, the same rule as the palette swap: whoever bakes a sheet keeps it
/// and disposes it when they're done. WalkerSprite then animates the baked sheet like any other.
/// Every layer is the body's layout (five frames across, a row each way someone faces), so a pixel
/// in one lands on the same pixel of the body in all of them. Each screen that bakes has its own
/// Paperdoll, loaded through its own content manager, since the sheets are small.
/// </remarks>
public class Paperdoll
{
    /// <summary>The order the layers go down in, bottom first.</summary>
    /// <remarks>
    /// PaperdollOrder. The cape comes in two sheets because it's behind someone facing you and over
    /// them walking away: cape-back is what hangs behind them every way they face, and cape-front is
    /// the drape over the shoulders from the front and the whole cape from behind. With that split,
    /// one order works for every direction. The hair goes last, so long hair falls over the cape.
    /// </remarks>
    private static readonly Slot[] PaperdollOrder =
    {
        Slot.CapeBack, Slot.Body, Slot.Legs, Slot.Face, Slot.Beard, Slot.CapeFront, Slot.Hair,
    };

    private readonly CharacterPalettes _palettes = new();
    private readonly Dictionary<string, Color[]> _layers = new();
    private int _width;
    private int _height;

    /// <summary>The slots in PaperdollOrder.</summary>
    private enum Slot { CapeBack, Body, Legs, Face, Beard, CapeFront, Hair }

    /// <summary>Reads every layer's pixels and the palettes.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _palettes.LoadContent(content);

        Read(content, "body");
        Read(content, "cape-back");
        Read(content, "cape-front");
        foreach (IReadOnlyList<LookOptions.Option> list in new[] { LookOptions.Faces, LookOptions.Hair, LookOptions.Beards, LookOptions.Legs })
            foreach (LookOptions.Option option in list)
                if (option.Sheet is not null) Read(content, option.Sheet);
    }

    /// <summary>Bakes a look into a sheet of its own.</summary>
    /// <param name="device">The device to make the texture on.</param>
    /// <param name="look">How they look.</param>
    /// <returns>A new texture. Whoever calls this has to dispose it.</returns>
    public Texture2D Compose(GraphicsDevice device, CharacterLook look)
    {
        var pixels = new Color[_width * _height];
        foreach (Slot slot in PaperdollOrder)
        {
            if (Sheet(slot, look) is not string name) continue;

            Color[] layer = _layers[name];
            for (int i = 0; i < pixels.Length; i++)
                if (layer[i].A != 0) pixels[i] = layer[i];
        }

        _palettes.Recolour(pixels, look);

        var sheet = new Texture2D(device, _width, _height);
        sheet.SetData(pixels);
        return sheet;
    }

    /// <summary>Which sheet goes in a slot for a look, or null if nothing does.</summary>
    private static string Sheet(Slot slot, CharacterLook look) => slot switch
    {
        Slot.Body => "body",
        Slot.CapeBack => look.Cloak > 0 ? "cape-back" : null,
        Slot.CapeFront => look.Cloak > 0 ? "cape-front" : null,
        Slot.Legs => Pick(LookOptions.Legs, look.Legs),
        Slot.Face => Pick(LookOptions.Faces, look.Face),
        Slot.Beard => Pick(LookOptions.Beards, look.Beard),
        _ => Pick(LookOptions.Hair, look.Hair),
    };

    /// <summary>The sheet for a number into a list, wrapped round.</summary>
    private static string Pick(IReadOnlyList<LookOptions.Option> options, int index) =>
        options[LookOptions.Wrap(index, options.Count)].Sheet;

    /// <summary>Loads one layer and keeps its pixels. They're all the body's size.</summary>
    private void Read(ContentManager content, string name)
    {
        Texture2D texture = content.Load<Texture2D>("Paperdoll/" + name);
        _width = texture.Width;
        _height = texture.Height;

        var pixels = new Color[_width * _height];
        texture.GetData(pixels);
        _layers[name] = pixels;
    }
}
