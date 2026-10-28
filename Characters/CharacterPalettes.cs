using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters;

/// <summary>
/// The colour presets from character-palettes.png, and the palette swap that recolours a baked
/// sheet to someone's look.
/// </summary>
/// <remarks>
/// The Black Box's CharacterPalettes, with more parts to swap. generate_paperdoll.py cuts each part
/// of the youth down to a few key shades and writes every preset to the palette PNG, one row each,
/// the keys first in each block, so I read the colours from there instead of copying them in here
/// and the two can't get out of step. The blocks are in the order the script writes them, and each
/// is as long as its list in LookOptions.
/// </remarks>
public class CharacterPalettes
{
    /// <summary>The blocks of rows, top to bottom, and how many presets each one has.</summary>
    private static readonly int[] Blocks =
    {
        LookOptions.Skins.Count,
        LookOptions.HairColours.Count,
        LookOptions.Eyes.Count,
        LookOptions.Shirts.Count,
        LookOptions.Vests.Count,
        LookOptions.LegsColours.Count,
        // The cloak block has no row for "none".
        LookOptions.Cloaks.Count - 1,
    };

    private Color[][][] _blocks;

    /// <summary>Reads every preset out of character-palettes.png.</summary>
    /// <remarks>Throws if the PNG doesn't have as many rows as there are presets. That means the script and LookOptions don't match, and I'd rather find out right away.</remarks>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        Texture2D palettes = content.Load<Texture2D>("character-palettes");

        int rows = 0;
        foreach (int count in Blocks) rows += count;
        if (palettes.Height != rows)
            throw new InvalidOperationException($"character-palettes.png has {palettes.Height} rows; LookOptions needs {rows}. Run tools/generate_paperdoll.py.");

        var pixels = new Color[palettes.Width * palettes.Height];
        palettes.GetData(pixels);

        int row = 0;
        _blocks = new Color[Blocks.Length][][];
        for (int b = 0; b < Blocks.Length; b++) _blocks[b] = ReadBlock(pixels, palettes.Width, ref row, Blocks[b]);
    }

    /// <summary>Recolours a baked sheet's pixels to a look, in place.</summary>
    /// <remarks>
    /// Every key shade gets swapped for the same shade of the picked preset. It only runs when a look
    /// is baked, not every frame, and a sheet is small so it's instant. (No shader needed, just
    /// GetData and SetData like the sprite tutorials.)
    /// </remarks>
    /// <param name="pixels">The sheet, in its key colours.</param>
    /// <param name="look">The presets to put on it. Out of range ones wrap round.</param>
    public void Recolour(Color[] pixels, CharacterLook look)
    {
        int[] picked =
        {
            look.Skin, look.HairColour, look.Eyes, look.Shirt, look.Vest, look.LegsColour,
            // No cloak means the cape isn't drawn, so its keys never show; any preset will do.
            Math.Max(0, look.Cloak - 1),
        };

        var swap = new Dictionary<Color, Color>();
        for (int b = 0; b < Blocks.Length; b++)
        {
            Color[] key = _blocks[b][0], chosen = _blocks[b][LookOptions.Wrap(picked[b], Blocks[b])];
            for (int i = 0; i < key.Length && i < chosen.Length; i++) swap[key[i]] = chosen[i];
        }

        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].A == byte.MaxValue && swap.TryGetValue(pixels[i], out Color swapped))
                pixels[i] = swapped;
        }
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
