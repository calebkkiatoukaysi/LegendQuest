using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Items;

namespace LegendQuest.Characters;

/// <summary>
/// Bakes someone's look, and the gear they're wearing, into one sheet: every layer it calls for
/// drawn over the body in PaperdollOrder, then recoloured to their presets.
/// </summary>
/// <remarks>
/// <para>
/// Once per change, not per frame, the same rule as the palette swap: whoever bakes a sheet keeps it
/// and disposes it when they're done. WalkerSprite then animates the baked sheet like any other.
/// Every layer is the body's layout (ten frames across: standing, four steps, and the fights'
/// wind-up, strike, cast, hurt and downed; a row each way someone faces), so a pixel in one lands
/// on the same pixel of the body in all of them. There are three bodies: the lean one most peoples
/// have, the Stoneborn's stout one and the Hearthling's small one, and every layer is drawn again
/// for each, in a folder of its own. Each screen that bakes has its own Paperdoll, loaded through
/// its own content manager.
/// </para>
/// <para>
/// Gear is named the way ItemDefinition.Layer names it (armour-plate, weapon-bow and the rest).
/// Metal and leather are drawn in key shades of their own, so a gear layer can be given a tint after
/// a colon (weapon-longsword:gold, armour-leather:black), which swaps its keys for that row of
/// gear-palettes.png: one drawn sheet makes several items. A helm takes the place of the look's
/// hat, and a great helm hides the hair and ears the way a hood does.
/// </para>
/// <para>
/// Every layer is checked to be there, for every body, when the Paperdoll loads, so a sheet that's
/// missing stops the game there and then rather than the first time someone puts it on. Each is
/// only read (and its size checked) the first time a sheet's baked with it, since most looks use a
/// handful of the two hundred.
/// </para>
/// </remarks>
public class Paperdoll
{
    /// <summary>What separates a gear layer's name from the tint it's in: weapon-longsword:gold.</summary>
    private const char TintMark = ':';

    /// <summary>The gear layer a cloak draws. It isn't a sheet of its own but the cape's two halves, in the look's cloak dye (woad if the look hasn't a cloak).</summary>
    private const string Cloak = "cloak";

    /// <summary>The folder each body's sheets are in, under Content/Paperdoll. The lean body's are at the top.</summary>
    private static readonly Dictionary<Build, string> Folders = new()
    {
        [Build.Lean] = "Paperdoll/",
        [Build.Stout] = "Paperdoll/Stout/",
        [Build.Small] = "Paperdoll/Small/",
    };

    /// <summary>The sheets every body has whatever the look: the body, the cape's two halves, and every people's parts.</summary>
    private static readonly string[] Always =
    {
        "body", "cape-back", "cape-front", "ears-pointed", "ears-wolf", "ears-hawk", "ears-bear",
        "horns-curl", "horns-swept", "tail-ember-back", "tail-ember-front", "tail-wolf-back", "tail-wolf-front",
    };

    /// <summary>Every gear layer, by the name ItemDefinition.Layer uses, and the slot it goes in. Each but the cloak is a sheet in every body's folder.</summary>
    private static readonly Dictionary<string, PaperdollSlot> GearSlots = new()
    {
        ["armour-padded"] = PaperdollSlot.Armour,
        ["armour-leather"] = PaperdollSlot.Armour,
        ["armour-studded"] = PaperdollSlot.Armour,
        ["armour-hide"] = PaperdollSlot.Armour,
        ["armour-chain-shirt"] = PaperdollSlot.Armour,
        ["armour-scale"] = PaperdollSlot.Armour,
        ["armour-chain"] = PaperdollSlot.Armour,
        ["armour-plate"] = PaperdollSlot.Armour,
        ["armour-robe"] = PaperdollSlot.Armour,
        ["helm-cap"] = PaperdollSlot.Hat,
        ["helm-full"] = PaperdollSlot.Hat,
        ["helm-circlet"] = PaperdollSlot.Hat,
        ["gloves"] = PaperdollSlot.Hands,
        ["gauntlets"] = PaperdollSlot.Hands,
        ["boots-leather"] = PaperdollSlot.Boots,
        ["boots-iron"] = PaperdollSlot.Boots,
        [Cloak] = PaperdollSlot.CapeBack,
        ["shield-round"] = PaperdollSlot.Shield,
        ["shield-heater"] = PaperdollSlot.Shield,
        ["weapon-dagger"] = PaperdollSlot.Weapon,
        ["weapon-shortsword"] = PaperdollSlot.Weapon,
        ["weapon-longsword"] = PaperdollSlot.Weapon,
        ["weapon-rapier"] = PaperdollSlot.Weapon,
        ["weapon-scimitar"] = PaperdollSlot.Weapon,
        ["weapon-axe"] = PaperdollSlot.Weapon,
        ["weapon-greatsword"] = PaperdollSlot.Weapon,
        ["weapon-hammer"] = PaperdollSlot.Weapon,
        ["weapon-mace"] = PaperdollSlot.Weapon,
        ["weapon-staff"] = PaperdollSlot.Weapon,
        ["weapon-spear"] = PaperdollSlot.Weapon,
        ["weapon-bow"] = PaperdollSlot.Weapon,
        ["weapon-crossbow"] = PaperdollSlot.Weapon,
    };

    /// <summary>The gear that covers the whole head, so the hair and ears under it aren't drawn: the great helm. It covers the face as well, but it's drawn over that, so there's nothing to leave out.</summary>
    private static readonly HashSet<string> HidesHair = new() { "helm-full" };

    /// <summary>The tints metal can be given, in gear-palettes.png's order after metal's keys.</summary>
    private static readonly string[] MetalTints = { "iron", "blackened", "bronze", "gold" };

    /// <summary>The tints leather can be given, in gear-palettes.png's order after leather's keys, which come after metal's block.</summary>
    private static readonly string[] LeatherTints = { "dark", "black", "tan", "red" };

    /// <summary>The ways someone can face, which are the sheet's rows, top to bottom.</summary>
    private static readonly Direction[] Directions = Enum.GetValues<Direction>();

    private readonly CharacterPalettes _palettes = new();
    private readonly Dictionary<string, Color[]> _layers = new();
    private readonly Dictionary<string, Dictionary<Color, Color>> _tints = new();
    private ContentManager _content;
    private int _width;
    private int _height;

    /// <summary>The bodies.</summary>
    private enum Build { Lean, Stout, Small }

    /// <summary>Checks the gear tables against each other and against every item that draws something.</summary>
    /// <remarks>Throws on purpose, so an item that names a layer the paperdoll hasn't got shows up when the game starts, not the first time someone puts it on.</remarks>
    static Paperdoll()
    {
        foreach (string helm in HidesHair)
        {
            if (!GearSlots.TryGetValue(helm, out PaperdollSlot slot) || slot != PaperdollSlot.Hat)
                throw new InvalidOperationException($"{helm} hides the hair, but it isn't a gear layer that goes in the Hat slot.");
        }
        if (MetalTints.Concat(LeatherTints).Distinct().Count() != MetalTints.Length + LeatherTints.Length)
            throw new InvalidOperationException("Two of the paperdoll's tints have the same name.");
        foreach (ItemDefinition item in ItemCatalog.All)
        {
            if (item.Layer is not null && !GearSlots.ContainsKey(NameOf(item.Layer)))
                throw new InvalidOperationException($"Item {item.Id} draws {item.Layer}, which isn't one of the paperdoll's gear layers.");
        }
    }

    /// <summary>Loads the palettes, the gear tints and the lean body, whose size every layer has to be, and checks every other layer's there for every body.</summary>
    /// <remarks>Throws if a layer's missing: it means tools/generate_paperdoll.py hasn't been run since a layer was added.</remarks>
    /// <param name="content">The content manager to load with. Layers are loaded through it later, as they're needed.</param>
    public void LoadContent(ContentManager content)
    {
        _content = content;
        _palettes.LoadContent(content);
        LoadTints(content);

        Texture2D body = content.Load<Texture2D>(Folders[Build.Lean] + "body");
        _width = body.Width;
        _height = body.Height;

        foreach (string folder in Folders.Values)
        {
            foreach (string name in Always) Check(folder + name);
            foreach (IReadOnlyList<LookOptions.Option> list in new[] { LookOptions.Faces, LookOptions.Hair, LookOptions.Beards, LookOptions.Legs, LookOptions.Hats })
                foreach (LookOptions.Option option in list)
                    if (option.Sheet is not null) Check(folder + option.Sheet);
            foreach (string name in GearSlots.Keys)
                if (name != Cloak) Check(folder + name);
        }
    }

    /// <summary>Bakes a look, and what they're wearing, into a sheet of its own.</summary>
    /// <param name="device">The device to make the texture on.</param>
    /// <param name="look">How they look.</param>
    /// <param name="gear">The gear layers they're wearing, each with a tint after a colon if it has one (GearOf makes this from what a party member has on), or null for none. One a slot at most.</param>
    /// <returns>A new texture. Whoever calls this has to dispose it.</returns>
    public Texture2D Compose(GraphicsDevice device, CharacterLook look, IReadOnlyList<string> gear = null)
    {
        string folder = Folders[BuildOf(look.Ancestry)];
        Dictionary<PaperdollSlot, string> worn = Wear(gear);
        var pixels = new Color[_width * _height];

        // Each row of frames is one way of facing, and goes down in that way's order.
        int band = _width * (_height / Directions.Length);
        foreach (Direction facing in Directions)
        {
            int start = (int)facing * band;
            foreach (PaperdollSlot slot in PaperdollOrder.Of(facing))
            {
                if (Sheet(slot, look, worn) is not string layer) continue;

                Color[] source = Pixels(folder + NameOf(layer));
                Dictionary<Color, Color> tint = TintOf(layer) is string name ? _tints[name] : null;
                for (int i = start; i < start + band; i++)
                {
                    Color colour = source[i];
                    if (colour.A == 0) continue;
                    pixels[i] = tint is not null && tint.TryGetValue(colour, out Color tinted) ? tinted : colour;
                }
            }
        }

        _palettes.Recolour(pixels, look);

        var sheet = new Texture2D(device, _width, _height);
        sheet.SetData(pixels);
        return sheet;
    }

    /// <summary>A key for one baked sheet: the look's, and the gear, in an order that doesn't depend on the order it's listed in. Two people with the same key can share a sheet.</summary>
    /// <param name="look">How they look.</param>
    /// <param name="gear">The gear layers they're wearing, or null for none.</param>
    public static string Key(CharacterLook look, IReadOnlyList<string> gear) =>
        gear is null || gear.Count == 0
            ? look.Key
            : look.Key + "|" + string.Join(",", gear.OrderBy(layer => layer, StringComparer.Ordinal));

    /// <summary>The gear layers a party member's wearing, for Compose: every worn item's Layer, but a weapon in the off hand, which isn't drawn because the paperdoll's weapons are all in the weapon hand.</summary>
    /// <param name="member">Who.</param>
    public static IReadOnlyList<string> GearOf(PartyMember member)
    {
        var gear = new List<string>();
        foreach ((EquipmentSlot slot, string id) in member.Equipment)
        {
            ItemDefinition item = ItemCatalog.Get(id);
            if (item.Layer is null || slot == EquipmentSlot.OffHand && item.Kind == ItemKind.Weapon) continue;
            gear.Add(item.Layer);
        }
        return gear;
    }

    /// <summary>Which body a people has.</summary>
    private static Build BuildOf(Ancestry ancestry) => ancestry switch
    {
        Ancestry.Stoneborn => Build.Stout,
        Ancestry.Hearthling => Build.Small,
        _ => Build.Lean,
    };

    /// <summary>Which layer goes in a slot for a look and what they're wearing (with its tint, if it has one), or null if nothing does.</summary>
    private static string Sheet(PaperdollSlot slot, CharacterLook look, IReadOnlyDictionary<PaperdollSlot, string> worn) => slot switch
    {
        PaperdollSlot.Body => "body",
        PaperdollSlot.CapeBack => Cloaked(look, worn) ? "cape-back" : null,
        PaperdollSlot.CapeFront => Cloaked(look, worn) ? "cape-front" : null,
        PaperdollSlot.Legs => Pick(LookOptions.Legs, look.Legs),
        PaperdollSlot.Face => Pick(LookOptions.Faces, look.Face),
        PaperdollSlot.Beard => Pick(LookOptions.Beards, look.Beard),
        PaperdollSlot.Hair => HairHidden(look, worn) ? null : Pick(LookOptions.Hair, look.Hair),
        PaperdollSlot.Hat => worn.GetValueOrDefault(PaperdollSlot.Hat) ?? Pick(LookOptions.Hats, look.Hat),
        PaperdollSlot.Ears => HairHidden(look, worn) ? null : Ears(look),
        PaperdollSlot.Horns => look.Ancestry == Ancestry.Emberkin ? Line(look) == 0 ? "horns-curl" : "horns-swept" : null,
        PaperdollSlot.TailBack => Tail(look) is string tail ? tail + "-back" : null,
        PaperdollSlot.TailFront => Tail(look) is string tail ? tail + "-front" : null,
        // The rest are gear's alone: boots, armour, gloves, the shield and the weapon.
        _ => worn.GetValueOrDefault(slot),
    };

    /// <summary>Whether they've a cape on: their look's, or a cloak they're wearing.</summary>
    private static bool Cloaked(CharacterLook look, IReadOnlyDictionary<PaperdollSlot, string> worn) =>
        look.Cloak > 0 || worn.ContainsKey(PaperdollSlot.CapeBack);

    /// <summary>Whether what's on their head covers their hair and ears: a helm they're wearing that does, or if they've no helm on, their look's hood.</summary>
    private static bool HairHidden(CharacterLook look, IReadOnlyDictionary<PaperdollSlot, string> worn) =>
        worn.TryGetValue(PaperdollSlot.Hat, out string helm)
            ? HidesHair.Contains(NameOf(helm))
            : LookOptions.Hats[LookOptions.Wrap(look.Hat, LookOptions.Hats.Count)].HidesHair;

    /// <summary>A people's ears: the Sylvan's points and the Wildkin's beast's.</summary>
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

    /// <summary>A gear layer's name without its tint.</summary>
    private static string NameOf(string layer) => layer.Split(TintMark)[0];

    /// <summary>The tint a gear layer's given after its colon, or null if it hasn't one.</summary>
    private static string TintOf(string layer)
    {
        int mark = layer.IndexOf(TintMark);
        return mark < 0 ? null : layer[(mark + 1)..];
    }

    /// <summary>What a list of gear layers puts in each slot, checked.</summary>
    /// <remarks>Throws if a layer or a tint isn't one there is, if the cloak's given a tint (it's the look's cloak dye), or if two layers want one slot: that's a bug in whoever made the list.</remarks>
    private Dictionary<PaperdollSlot, string> Wear(IReadOnlyList<string> gear)
    {
        var worn = new Dictionary<PaperdollSlot, string>();
        if (gear is null) return worn;

        foreach (string layer in gear)
        {
            string name = NameOf(layer);
            if (!GearSlots.TryGetValue(name, out PaperdollSlot slot))
                throw new ArgumentException($"There's no paperdoll gear layer called {name}.", nameof(gear));
            if (TintOf(layer) is string tint && (name == Cloak || !_tints.ContainsKey(tint)))
                throw new ArgumentException($"{layer} can't be tinted {tint}: the tints are {string.Join(", ", MetalTints.Concat(LeatherTints))}, and a cloak takes the look's dye.", nameof(gear));
            if (!worn.TryAdd(slot, layer))
                throw new ArgumentException($"{worn[slot]} and {layer} both go in the {slot} slot.", nameof(gear));
        }
        return worn;
    }

    /// <summary>Reads gear-palettes.png: metal's keys and each of its tints, a row each, then leather's.</summary>
    /// <remarks>Throws if it hasn't a row for every tint: the script and the names here don't match.</remarks>
    private void LoadTints(ContentManager content)
    {
        Texture2D palettes = content.Load<Texture2D>("gear-palettes");
        int rows = 2 + MetalTints.Length + LeatherTints.Length;
        if (palettes.Height != rows)
            throw new InvalidOperationException($"gear-palettes.png has {palettes.Height} rows; Paperdoll needs {rows}. Run tools/generate_paperdoll.py.");

        var pixels = new Color[palettes.Width * palettes.Height];
        palettes.GetData(pixels);

        int row = 0;
        foreach (string[] tints in new[] { MetalTints, LeatherTints })
        {
            int keys = row++;
            foreach (string tint in tints)
            {
                var swap = new Dictionary<Color, Color>();
                for (int x = 0; x < palettes.Width; x++) swap[pixels[keys * palettes.Width + x]] = pixels[row * palettes.Width + x];
                _tints[tint] = swap;
                row++;
            }
        }
    }

    /// <summary>Checks a layer's built, by opening its file the way the content manager would and closing it again, without loading it.</summary>
    /// <remarks>Throws if it isn't there.</remarks>
    private void Check(string name)
    {
        try
        {
            TitleContainer.OpenStream(Path.Combine(_content.RootDirectory, name + ".xnb")).Dispose();
        }
        catch (IOException missing)
        {
            throw new InvalidOperationException($"Content/{name}.png isn't there: every paperdoll layer has to be drawn for every body. Run tools/generate_paperdoll.py.", missing);
        }
    }

    /// <summary>A layer's pixels, loaded and read the first time they're wanted and kept after that.</summary>
    /// <remarks>Throws if the layer isn't the body's size: it was drawn for a different layout, and tools/generate_paperdoll.py hasn't been run since.</remarks>
    private Color[] Pixels(string name)
    {
        if (!_layers.TryGetValue(name, out Color[] pixels))
        {
            Texture2D texture = _content.Load<Texture2D>(name);
            if (texture.Width != _width || texture.Height != _height)
                throw new InvalidOperationException($"Content/{name}.png is {texture.Width} by {texture.Height}; every paperdoll layer has to be the body's {_width} by {_height}. Run tools/generate_paperdoll.py.");

            pixels = new Color[_width * _height];
            texture.GetData(pixels);
            _layers[name] = pixels;
        }
        return pixels;
    }
}
