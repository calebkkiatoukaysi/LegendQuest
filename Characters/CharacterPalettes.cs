using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters;

/// <summary>
/// The skin and clothing presets from character-palettes.png, and the palette swap that recolours a
/// sheet to someone's look.
/// </summary>
/// <remarks>
/// The Black Box's CharacterPalettes. generate_characters.py paints the body sheet in the first
/// preset of each block (the keys) and writes every preset to the palette PNG, one row each, so I
/// read the colours from there instead of copying them in here, and the two can't get out of step.
/// </remarks>
public class CharacterPalettes
{
    /// <summary>How many skin presets there are: the first block of rows. Has to match SKINS in tools/generate_characters.py.</summary>
    public const int SkinPresets = 5;

    /// <summary>How many shirt dyes there are: the second block. Has to match SHIRTS.</summary>
    public const int ShirtPresets = 6;

    /// <summary>How many breeches dyes there are: the third block. Has to match BREECHES_DYES.</summary>
    public const int BreechesPresets = 4;

    private Color[][] _skin;
    private Color[][] _shirt;
    private Color[][] _breeches;

    /// <summary>Reads every preset out of character-palettes.png.</summary>
    /// <remarks>Throws if the PNG doesn't have as many rows as there are presets. That means the script and this file don't match, and I'd rather find out right away.</remarks>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        Texture2D palettes = content.Load<Texture2D>("character-palettes");

        int rows = SkinPresets + ShirtPresets + BreechesPresets;
        if (palettes.Height != rows)
            throw new InvalidOperationException($"character-palettes.png has {palettes.Height} rows; the presets need {rows}. Run tools/generate_characters.py.");

        var pixels = new Color[palettes.Width * palettes.Height];
        palettes.GetData(pixels);

        int row = 0;
        _skin = ReadBlock(pixels, palettes.Width, ref row, SkinPresets);
        _shirt = ReadBlock(pixels, palettes.Width, ref row, ShirtPresets);
        _breeches = ReadBlock(pixels, palettes.Width, ref row, BreechesPresets);
    }

    /// <summary>Makes a recoloured copy of a sheet for a look.</summary>
    /// <remarks>
    /// Every key colour gets swapped for the same shade of the picked preset. It only runs when a
    /// map loads or someone's look changes, not every frame, and a sheet is small so it's instant.
    /// (No shader needed, just GetData and SetData like the sprite tutorials.)
    /// </remarks>
    /// <param name="device">The device to make the new texture on.</param>
    /// <param name="sheet">The sheet in its key colours.</param>
    /// <param name="look">The presets to put on it. Out of range ones wrap round.</param>
    /// <returns>A new texture. Whoever calls this has to dispose it.</returns>
    public Texture2D Recolour(GraphicsDevice device, Texture2D sheet, CharacterLook look)
    {
        var swap = new Dictionary<Color, Color>();
        AddSwap(swap, _skin[0], _skin[Wrap(look.Skin, SkinPresets)]);
        AddSwap(swap, _shirt[0], _shirt[Wrap(look.Shirt, ShirtPresets)]);
        AddSwap(swap, _breeches[0], _breeches[Wrap(look.Breeches, BreechesPresets)]);

        var pixels = new Color[sheet.Width * sheet.Height];
        sheet.GetData(pixels);

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A == byte.MaxValue && swap.TryGetValue(pixels[i], out Color swapped))
                pixels[i] = swapped;
        }

        var recoloured = new Texture2D(device, sheet.Width, sheet.Height);
        recoloured.SetData(pixels);
        return recoloured;
    }

    /// <summary>Wraps a preset number round, so a save with one that's since gone still gets a colour.</summary>
    private static int Wrap(int index, int count) => (index % count + count) % count;

    /// <summary>Maps each key shade to the same shade of the picked preset.</summary>
    private static void AddSwap(Dictionary<Color, Color> swap, IReadOnlyList<Color> key, IReadOnlyList<Color> chosen)
    {
        for (int i = 0; i < key.Count && i < chosen.Count; i++) swap[key[i]] = chosen[i];
    }

    /// <summary>Reads one block of rows. A transparent pixel ends a ramp early.</summary>
    private static Color[][] ReadBlock(Color[] pixels, int width, ref int row, int count)
    {
        var block = new Color[count][];
        for (int i = 0; i < count; i++, row++)
        {
            var ramp = new List<Color>(width);
            for (int x = 0; x < width; x++)
            {
                Color c = pixels[row * width + x];
                if (c.A == 0) break;
                ramp.Add(c);
            }
            block[i] = ramp.ToArray();
        }
        return block;
    }
}
