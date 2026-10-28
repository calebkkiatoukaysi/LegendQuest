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
/// in one lands on the same pixel of the body in all of them. There are three bodies: the lean one
/// most peoples have, the Stoneborn's stout one and the Hearthling's small one, and every layer is
/// drawn again for each, in a folder of its own. Each screen that bakes has its own Paperdoll,
/// loaded through its own content manager, since the sheets are small.
/// </remarks>
public class Paperdoll
{
    /// <summary>The order the layers go down in, bottom first.</summary>
    /// <remarks>
    /// PaperdollOrder. The cape and the tails come in two sheets because they're behind someone
    /// facing you and over them walking away: the back half is what hangs behind every way they
    /// face, and the front half is the drape over the shoulders from the front and all of it from
    /// behind. With that split, one order works for every direction. The hair's after the cape so
    /// long hair falls over it, the hat after the hair, and ears and horns after the hat so they
    /// stick out of it.
    /// </remarks>
    private static readonly Slot[] PaperdollOrder =
    {
        Slot.CapeBack, Slot.TailBack, Slot.Body, Slot.Legs, Slot.Face, Slot.Beard, Slot.CapeFront,
        Slot.Hair, Slot.Hat, Slot.Ears, Slot.Horns, Slot.TailFront,
    };

    /// <summary>The folder each body's sheets are in, under Content/Paperdoll. The lean body's are at the top.</summary>
    private static readonly Dictionary<Build, string> Folders = new()
    {
        [Build.Lean] = "Paperdoll/",
        [Build.Stout] = "Paperdoll/Stout/",
        [Build.Small] = "Paperdoll/Small/",
    };

    /// <summary>The sheets every body has whatever the look: the body, the cape's two halves, and every people's parts.</summary>
    private static readonly string[] AlwaysLoaded =
    {
        "body", "cape-back", "cape-front", "ears-pointed", "ears-wolf", "ears-hawk", "ears-bear",
        "horns-curl", "horns-swept", "tail-ember-back", "tail-ember-front", "tail-wolf-back", "tail-wolf-front",
    };

    private readonly CharacterPalettes _palettes = new();
    private readonly Dictionary<string, Color[]> _layers = new();
    private int _width;
    private int _height;

    /// <summary>The slots in PaperdollOrder.</summary>
    private enum Slot { CapeBack, TailBack, Body, Legs, Face, Beard, CapeFront, Hair, Hat, Ears, Horns, TailFront }

    /// <summary>The bodies.</summary>
    private enum Build { Lean, Stout, Small }

    /// <summary>Reads every layer's pixels, for every body, and the palettes.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _palettes.LoadContent(content);

        foreach (string folder in Folders.Values)
        {
            foreach (string name in AlwaysLoaded) Read(content, folder + name);
            foreach (IReadOnlyList<LookOptions.Option> list in new[] { LookOptions.Faces, LookOptions.Hair, LookOptions.Beards, LookOptions.Legs, LookOptions.Hats })
                foreach (LookOptions.Option option in list)
                    if (option.Sheet is not null) Read(content, folder + option.Sheet);
        }
    }

    /// <summary>Bakes a look into a sheet of its own.</summary>
    /// <param name="device">The device to make the texture on.</param>
    /// <param name="look">How they look.</param>
    /// <returns>A new texture. Whoever calls this has to dispose it.</returns>
    public Texture2D Compose(GraphicsDevice device, CharacterLook look)
    {
        string folder = Folders[BuildOf(look.Ancestry)];
        var pixels = new Color[_width * _height];
        foreach (Slot slot in PaperdollOrder)
        {
            if (Sheet(slot, look) is not string name) continue;

            Color[] layer = _layers[folder + name];
            for (int i = 0; i < pixels.Length; i++)
                if (layer[i].A != 0) pixels[i] = layer[i];
        }

        _palettes.Recolour(pixels, look);

        var sheet = new Texture2D(device, _width, _height);
        sheet.SetData(pixels);
        return sheet;
    }

    /// <summary>Which body a people has.</summary>
    private static Build BuildOf(Ancestry ancestry) => ancestry switch
    {
        Ancestry.Stoneborn => Build.Stout,
        Ancestry.Hearthling => Build.Small,
        _ => Build.Lean,
    };

    /// <summary>Which sheet goes in a slot for a look, or null if nothing does.</summary>
    private static string Sheet(Slot slot, CharacterLook look) => slot switch
    {
        Slot.Body => "body",
        Slot.CapeBack => look.Cloak > 0 ? "cape-back" : null,
        Slot.CapeFront => look.Cloak > 0 ? "cape-front" : null,
        Slot.Legs => Pick(LookOptions.Legs, look.Legs),
        Slot.Face => Pick(LookOptions.Faces, look.Face),
        Slot.Beard => Pick(LookOptions.Beards, look.Beard),
        Slot.Hat => Pick(LookOptions.Hats, look.Hat),
        Slot.Ears => Ears(look),
        Slot.Horns => look.Ancestry == Ancestry.Emberkin ? Line(look) == 0 ? "horns-curl" : "horns-swept" : null,
        Slot.TailBack => Tail(look) is string tail ? tail + "-back" : null,
        Slot.TailFront => Tail(look) is string tail ? tail + "-front" : null,
        _ => Pick(LookOptions.Hair, look.Hair),
    };

    /// <summary>A people's ears, if theirs aren't hidden under their hair: the Sylvan's points and the Wildkin's beast's.</summary>
    private static string Ears(CharacterLook look) => look.Ancestry switch
    {
        Ancestry.Sylvan => "ears-pointed",
        Ancestry.Wildkin => Line(look) switch { 0 => "ears-wolf", 1 => "ears-hawk", _ => "ears-bear" },
        _ => null,
    };

    /// <summary>A people's tail, if they've one: the Emberkin's, and the Wildkin wolf line's.</summary>
    private static string Tail(CharacterLook look) => look.Ancestry switch
    {
        Ancestry.Emberkin => "tail-ember",
        Ancestry.Wildkin when Line(look) == 0 => "tail-wolf",
        _ => null,
    };

    /// <summary>Which of their people's lines someone is, wrapped round.</summary>
    private static int Line(CharacterLook look) => LookOptions.Wrap(look.Lineage, LookOptions.Lineages(look.Ancestry).Count);

    /// <summary>The sheet for a number into a list, wrapped round.</summary>
    private static string Pick(IReadOnlyList<LookOptions.Option> options, int index) =>
        options[LookOptions.Wrap(index, options.Count)].Sheet;

    /// <summary>Loads one layer and keeps its pixels. They're all the body's size.</summary>
    private void Read(ContentManager content, string name)
    {
        Texture2D texture = content.Load<Texture2D>(name);
        _width = texture.Width;
        _height = texture.Height;

        var pixels = new Color[_width * _height];
        texture.GetData(pixels);
        _layers[name] = pixels;
    }
}
