using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Items;

namespace LegendQuest.Characters;

/// <summary>
/// The characters' parts made in ChatGPT, cut by tools/import_parts.py into atlas pages, and the
/// recipes that put each character together from them at their base's sockets.
/// </summary>
/// <remarks>
/// <para>
/// LoadAll reads Content/Characters/Parts/parts.json as the game starts (so --validate checks it)
/// and checks everything in it: that the importer and the game agree on the sockets, the slots and
/// their order; that every page was built; that every part is on its page, on a base there is,
/// hanging from its slot's socket; and that every recipe's morph makes sense and its parts are in the
/// right slots and on one base. LoadContent cuts every part off its page, the way the tutorials cut
/// sprites off a sheet, by its rectangle, and lets the pages go.
/// </para>
/// <para>
/// A character's figure is put together the first time it's asked for (RigComposer) and kept: an
/// ImportedFigure like a whole picture's, so the rooms, the fights and the portraits draw it the
/// same way. Until their armor's been made it has no body to stand in a room with (HasBody is
/// false): the room draws them as they were, and only the portraits use the painted face. A recipe
/// whose face hasn't been made yet makes nothing, and they keep the look they had.
/// </para>
/// <para>
/// The party wear what they've equipped (Dress): each item draws the part §7.1 of the prompt book
/// gives it on their base, and a companion's starting kit draws their own pieces (§7.2). Changing
/// what someone wears makes a new outfit, and a figure is put together once per outfit.
/// </para>
/// </remarks>
public static class PartCatalog
{
    /// <summary>Where the pages and parts.json are, under Content.</summary>
    public const string Folder = "Characters/Parts";

    /// <summary>The layout of parts.json the game reads.</summary>
    public const int FileVersion = 6;

    /// <summary>A base's fists in parts.json's hands: what's held in the near hand and the far one is drawn under them.</summary>

    /// <summary>The recipes the player's figure is put together on, whichever face they pick: the one on that face's base.</summary>
    private static readonly string[] PlayerTemplates = { "you-male", "you-female" };

    /// <summary>The size of the bases' canvas the parts were cut from.</summary>
    private static readonly int[] SourceCanvas = { 1024, 1536 };

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private static PartsFile _file;
    private static GraphicsDevice _device;
    private static readonly Dictionary<string, PartImage> Images = new(StringComparer.Ordinal);

    /// <summary>A body's or an armor's pieces by its id, one image per bone; Images keeps the first, for its slot and base.</summary>
    private static readonly Dictionary<string, List<PartImage>> PieceImages = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, ImportedFigure> Built = new(StringComparer.Ordinal);

    /// <summary>What each item draws in each slot it can go in, by base: §7.1.</summary>
    private static readonly Dictionary<(string Item, EquipmentSlot Slot), Dictionary<string, string>> Gear = new();

    /// <summary>The party's own pieces for their starting kit, by recipe id, then by slot and item: §7.2. A null part draws nothing.</summary>
    private static readonly Dictionary<string, Dictionary<(EquipmentSlot Slot, string Item), string>> Own = new(StringComparer.Ordinal);

    /// <summary>The faces the creation screen offers you and the Soul-Bound, in order: the shared set's, the ten men's (face-m01 on) then the ten women's (face-f01 on), as many as are painted.</summary>
    public static IReadOnlyList<string> PlayerFaces { get; private set; } = Array.Empty<string>();

    /// <summary>The hairs the creation screen offers, in order: the hair library's styles (hair-01 on), the man's copy of each (the woman's is chosen on her base).</summary>
    public static IReadOnlyList<string> PlayerHairs { get; private set; } = Array.Empty<string>();

    /// <summary>Reads and checks parts.json in the built game. With no parts imported there's no file, and nobody's put together.</summary>
    /// <remarks>Throws on the first fault, naming it, so a bad import stops the game as it starts.</remarks>
    public static void LoadAll()
    {
        _file = null;
        PlayerFaces = Array.Empty<string>();
        PlayerHairs = Array.Empty<string>();
        Skeleton.Current = null;
        ClipLibrary.Load(null, null);
        Images.Clear();
        PieceImages.Clear();
        Built.Clear();
        foreach ((_, Standing kept) in Pictures) kept.Picture.Dispose();
        Pictures.Clear();
        foreach ((_, ImportedFigure kept) in Larges) kept.Dispose();
        Larges.Clear();
        Gear.Clear();
        Own.Clear();

        string folder = Path.Combine(AppContext.BaseDirectory, "Content", Folder);
        string path = Path.Combine(folder, "parts.json");
        if (!File.Exists(path)) return;

        PartsFile file;
        try
        {
            file = JsonSerializer.Deserialize<PartsFile>(File.ReadAllText(path), Options);
        }
        catch (JsonException unreadable)
        {
            throw new InvalidOperationException($"Content/{Folder}/parts.json isn't readable: {unreadable.Message}", unreadable);
        }
        Check(file, folder);
        _file = file;
        Skeleton.Current = new Skeleton(file.Bones, file.Draw);
        ClipLibrary.Load(Path.Combine(folder, "clips.json"), Skeleton.Current);
        foreach (PartsFile.GearEntry g in file.Gear)
            Gear[(g.Item, GearSlot(g.Slot).Value)] = g.Parts;
        foreach ((string who, PartsFile.OwnEntry[] rows) in file.Own)
            Own[who] = rows.ToDictionary(r => (GearSlot(r.Slot).Value, r.Item), r => r.Part);
        PlayerFaces = Numbered(file, "face-m").Concat(Numbered(file, "face-f")).Where(IsSetFace).ToArray();
        PlayerHairs = Numbered(file, "hair-").Where(IsLibrary).ToArray();
    }

    /// <summary>Whether a part is the man's copy of one of the hair library's: hair- and two digits.</summary>
    public static bool IsLibrary(string id) => id is { Length: 7 } && id.StartsWith("hair-", StringComparison.Ordinal) && char.IsDigit(id[5]) && char.IsDigit(id[6]);

    /// <summary>
    /// The copy of a part made for a base: the part itself if it's on that base, else its pair on it
    /// (the library's hairs and extras are painted on both, the woman's copy the man's id and -f),
    /// else the part itself, to be fitted (Fit).
    /// </summary>
    /// <param name="id">The part.</param>
    /// <param name="onBase">The base it's going on.</param>
    public static string OnBase(string id, string onBase)
    {
        if (id is null || _file is null || !_file.Parts.TryGetValue(id, out PartsFile.PartEntry part) || part.Base == onBase) return id;
        string pair = id.EndsWith("-f", StringComparison.Ordinal) ? id[..^2] : id + "-f";
        return _file.Parts.TryGetValue(pair, out PartsFile.PartEntry other) && other.Base == onBase ? pair : id;
    }

    /// <summary>Whether a face is one of the shared set's: face-m or face-f and two digits.</summary>
    public static bool IsSetFace(string id) =>
        id is { Length: 8 } && id.StartsWith("face-", StringComparison.Ordinal) && (id[5] == 'm' || id[5] == 'f') && char.IsDigit(id[6]) && char.IsDigit(id[7]);

    /// <summary>Every part whose id starts so, in order of the number after it, then the rest by name.</summary>
    private static string[] Numbered(PartsFile file, string prefix) =>
        file.Parts.Keys.Where(id => id.StartsWith(prefix, StringComparison.Ordinal))
            .OrderBy(id => int.TryParse(id[prefix.Length..], out int n) ? n : int.MaxValue).ThenBy(id => id, StringComparer.Ordinal).ToArray();

    /// <summary>Cuts every part off its page, ready to put together. The pages are let go afterwards.</summary>
    /// <remarks>Throws if a page isn't the size parts.json says: one was cut again and the other wasn't.</remarks>
    /// <param name="services">The game's services, for a content manager of its own.</param>
    /// <param name="device">The device the figures' textures are made on.</param>
    public static void LoadContent(IServiceProvider services, GraphicsDevice device)
    {
        _device = device;
        Images.Clear();
        PieceImages.Clear();
        Built.Clear();
        foreach ((_, Standing kept) in Pictures) kept.Picture.Dispose();
        Pictures.Clear();
        foreach ((_, ImportedFigure kept) in Larges) kept.Dispose();
        Larges.Clear();
        if (_file is null) return;

        using var content = new ContentManager(services, "Content");
        var pages = new Dictionary<string, (Color[] Pixels, int Width)>(StringComparer.Ordinal);
        foreach ((string name, int[] size) in _file.Pages)
        {
            var page = content.Load<Texture2D>(Folder + "/" + name);
            if (page.Width != size[0] || page.Height != size[1])
                throw new InvalidOperationException($"Content/{Folder}/{name}.png is {page.Width} by {page.Height}, but parts.json says {size[0]} by {size[1]}. Run python tools/import_parts.py.");
            var pixels = new Color[page.Width * page.Height];
            page.GetData(pixels);
            pages[name] = (pixels, page.Width);
        }

        foreach ((string id, PartsFile.PartEntry entry) in _file.Parts)
        {
            Sockets.TryParse(entry.Slot, out PartSlot slot);
            Vector3 ratio = entry.Skin is { Length: 3 } s ? new Vector3(s[0], s[1], s[2]) : Vector3.One;
            Vector3?[] tones = entry.Tones?.Select(t => t is null ? (Vector3?)null : new Vector3(t[0], t[1], t[2]) / 255f).ToArray();
            PartImage Image(string key, string pageName, int[] rect, int[] maskRect, float[] pivot, string bone)
            {
                (Color[] page, int stride) = pages[pageName];
                Color[] mask = maskRect is null ? null : Cut(page, stride, maskRect);
                return new PartImage(key, slot, entry.Base, rect[2], rect[3], Cut(page, stride, rect), mask, new Vector2(pivot[0], pivot[1]), ratio, tones, bone);
            }
            if (entry.Pieces is null)
            {
                Images[id] = Image(id, entry.Page, entry.Rect, entry.MaskRect, entry.Pivot, null);
                continue;
            }
            var pieces = new List<PartImage>();
            foreach ((string bone, PartsFile.PieceEntry piece) in entry.Pieces)
                pieces.Add(Image(id + "@" + bone, piece.Page, piece.Rect, piece.MaskRect, piece.Pivot, bone));
            PieceImages[id] = pieces;
            Images[id] = pieces[0] with { Id = id, Bone = null };
        }
    }

    /// <summary>Whether a part's been made and imported.</summary>
    /// <param name="id">The part's id.</param>
    public static bool Has(string id) => id is not null && _file is not null && _file.Parts.ContainsKey(id);

    /// <summary>The figure a recipe puts together, or null if there's no such recipe or its face isn't made yet. It has a body to stand in a room with once its armor's made too.</summary>
    /// <param name="id">The recipe's id: a character's, as Story/13-art-prompts.md §7 and the game both call them.</param>
    public static ImportedFigure Figure(string id) =>
        id is not null && _file is not null && _file.Recipes.TryGetValue(id, out PartsFile.RecipeEntry recipe) ? Build(Headed(Outfit.Of(id, recipe))) : null;

    /// <summary>
    /// An outfit with something on its head: its own hair or helm if that's painted, else the hair
    /// painted with its face (hair-x for face-x), so nobody's bald while their helm's still to make.
    /// </summary>
    private static Outfit Headed(Outfit outfit) => Has(outfit.Helm) ? outfit : outfit with { Helm = Has(Outfit.HairOf(outfit.Face)) ? Outfit.HairOf(outfit.Face) : null };

    /// <summary>
    /// A party member with a face they picked: their own recipe with that face, if it's on the same
    /// base, or the player's on that face's base. Null if the face isn't one there is.
    /// </summary>
    /// <param name="id">Who: you, the Soul-Bound or a companion.</param>
    /// <param name="face">The face's id.</param>
    public static ImportedFigure ForMember(string id, string face) =>
        Pick(id, face) is (string name, PartsFile.RecipeEntry recipe) ? Build(Headed(Outfit.Of(name, recipe, face) with { Helm = OwnHead(id, recipe, face) })) : null;

    /// <summary>What's on a party member's head before anything's worn there: their recipe's own piece if it's theirs (a companion's hood), else the hair painted with the face they picked.</summary>
    private static string OwnHead(string id, PartsFile.RecipeEntry recipe, string face) =>
        id is not null && _file.Recipes.ContainsKey(id) && Has(recipe.Helm) && recipe.Face == face ? recipe.Helm : Outfit.HairOf(face);

    /// <summary>
    /// A party member as they are now: the face they picked on their own recipe or the player's (as
    /// ForMember), wearing what they've equipped. Each item draws its part from §7.1 on their base, or
    /// their own piece from §7.2 if it's in their starting kit; body armour that isn't painted yet
    /// leaves their clothes on, and anything else that isn't draws nothing. Null if they've no
    /// painted face.
    /// </summary>
    /// <param name="member">Who.</param>
    public static ImportedFigure Dress(PartyMember member) => OutfitOf(member) is Outfit outfit ? Build(outfit) : null;

    /// <summary>What Dress puts a party member together from, or null if they've no painted face.</summary>
    /// <param name="member">Who.</param>
    public static Outfit OutfitOf(PartyMember member)
    {
        if (member?.Look?.Portrait is not string face || Pick(member.Id, face) is not (string name, PartsFile.RecipeEntry recipe)) return null;
        string onBase = FaceBase(face);
        Outfit outfit = Outfit.Of(name, recipe, face);
        return outfit with
        {
            Armor = Wears(member, name, EquipmentSlot.Body, onBase, outfit.Armor),
            Cape = member.Look.HideCape ? null : Wears(member, name, EquipmentSlot.Cloak, onBase, outfit.Cape),
            Helm = HeadOf(member, name, recipe, face, onBase),
            Weapon = Wears(member, name, EquipmentSlot.MainHand, onBase, null),
            Offhand = Wears(member, name, EquipmentSlot.OffHand, onBase, null),
            Tints = Tints.Of(member.Look).Over(outfit.Tints),
            Morph = new Morph(outfit.Morph.Height * member.Look.PaintedMorph.Height, outfit.Morph.Width * member.Look.PaintedMorph.Width),
        };
    }

    /// <summary>
    /// What's on a party member's head, hair and helm being one slot: with the helm hidden, their
    /// hair; else what they wear there (its part from §7.1, or their own piece for a starting item),
    /// if it's painted; else, with nothing worn there, their recipe's own piece (a companion's hood),
    /// if it's painted; else their hair. Their hair is the one they picked, or with none picked the
    /// one painted with their face; empty for none.
    /// </summary>
    private static string HeadOf(PartyMember member, string recipe, PartsFile.RecipeEntry entry, string face, string onBase)
    {
        string hair = member.Look.HairPart switch { null => Has(Outfit.HairOf(face)) ? Outfit.HairOf(face) : null, "" => null, string picked => picked };
        if (member.Look.HideHelm) return hair;
        if (member.Equipment.ContainsKey(EquipmentSlot.Head)) return Wears(member, recipe, EquipmentSlot.Head, onBase, null) ?? hair;
        return OwnHead(member.Id, entry, face) is string own && own != Outfit.HairOf(face) && Has(own) ? own : hair;
    }

    /// <summary>
    /// What one of someone's equipment slots draws: their own piece for a starting item, else the
    /// item's part on their base, else what the recipe has there (their clothes, cape or helm) if the
    /// slot's empty or the body armour isn't painted yet; else nothing.
    /// </summary>
    /// <param name="member">Who.</param>
    /// <param name="recipe">The recipe they're on.</param>
    /// <param name="slot">The equipment slot.</param>
    /// <param name="onBase">Their base.</param>
    /// <param name="otherwise">The recipe's part in that slot, or null for the hands.</param>
    private static string Wears(PartyMember member, string recipe, EquipmentSlot slot, string onBase, string otherwise)
    {
        if (!member.Equipment.TryGetValue(slot, out string item)) return otherwise;
        string fallback = slot == EquipmentSlot.Body ? otherwise : null;
        if (OwnPiece(recipe, slot, item, out string own)) return own is null ? null : Has(own) ? own : fallback;
        return Gear.TryGetValue((item, slot), out Dictionary<string, string> parts) && parts.GetValueOrDefault(onBase) is string part && Has(part) ? part : fallback;
    }

    /// <summary>Whether a recipe has its own piece for an item in a slot (§7.2), and which: a variant recipe (soulbound.champion) uses its character's.</summary>
    private static bool OwnPiece(string recipe, EquipmentSlot slot, string item, out string part)
    {
        part = null;
        string character = recipe.Split('.')[0];
        return (Own.TryGetValue(recipe, out var rows) || Own.TryGetValue(character, out rows)) && rows.TryGetValue((slot, item), out part);
    }

    /// <summary>The recipe a party member with a painted face goes on, and its id: their own if it's on that face's base, else the player's on it.</summary>
    private static (string Id, PartsFile.RecipeEntry Recipe)? Pick(string id, string face)
    {
        if (FaceBase(face) is not string onBase) return null;
        if (id is not null && _file.Recipes.TryGetValue(id, out PartsFile.RecipeEntry own) && BaseOf(_file, own) == onBase)
            return (id, own);
        foreach (string template in PlayerTemplates)
        {
            if (_file.Recipes.TryGetValue(template, out PartsFile.RecipeEntry recipe) && BaseOf(_file, recipe) == onBase)
                return (template, recipe);
        }
        return null;
    }

    /// <summary>The base a face is on: its own if it's painted, else the base of a recipe that has it (they're the bare mannequin till it is); null for neither.</summary>
    private static string FaceBase(string face)
    {
        if (_file is null || face is null) return null;
        if (_file.Parts.TryGetValue(face, out PartsFile.PartEntry part)) return part.Slot == Sockets.Name(PartSlot.Face) ? part.Base : null;
        foreach (PartsFile.RecipeEntry recipe in _file.Recipes.Values)
            if (recipe.Face == face && recipe.Base is not null) return recipe.Base;
        return null;
    }

    /// <summary>A figure for the menus, much bigger than a room's: an outfit put together again, at a size where a standard adult's this tall.</summary>
    /// <param name="Picture">The picture.</param>
    /// <param name="Feet">Where the root (the ground between the feet) is on it.</param>
    /// <param name="HasBody">Whether its armor's painted, so there's a body to show; otherwise just the face is.</param>
    public sealed record Standing(Texture2D Picture, Vector2 Feet, bool HasBody);

    /// <summary>The last few Large figures asked for, by outfit and size, oldest first.</summary>
    private static readonly List<(string Key, ImportedFigure Figure)> Larges = new();

    /// <summary>How many Large figures are kept.</summary>
    private const int KeptLarges = 6;

    /// <summary>
    /// An outfit as a figure for a menu, moving in its pieces like one in a room but much bigger: a
    /// standard adult `height` pixels tall at a room's scale, put together from the parts at their own
    /// size. The last few are kept, and let go as they drop off; null if the face isn't painted.
    /// </summary>
    /// <param name="outfit">The outfit.</param>
    /// <param name="height">How tall a standard adult is in it, in pixels at a room's scale.</param>
    public static ImportedFigure Large(Outfit outfit, int height)
    {
        if (_device is null || outfit is null) return null;
        string key = outfit.Key + "@" + height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        foreach ((string k, ImportedFigure kept) in Larges)
            if (k == key) return kept;
        if (Composed(outfit) is not (RigComposer.Result result, bool hasBody, float rigPx, string onBase)) return null;

        var info = new ImportedArtInfo(outfit.Name, "standard", "person", new[] { result.Width, result.Height },
            new[] { result.Root.X, result.Root.Y }, new[] { result.Head.X, result.Head.Y, result.Head.Width, result.Head.Height }, SourceCanvas);
        var figure = new ImportedFigure(_device, info, result.Pixels, _file.StandardHeight * ImportedArt.RoomHeight / height, hasBody, result.Layers, result.Hand, result.Joints, rigPx, onBase);
        Larges.Add((key, figure));
        if (Larges.Count > KeptLarges)
        {
            Larges[0].Figure.Dispose();
            Larges.RemoveAt(0);
        }
        return figure;
    }

    /// <summary>The last few Standing pictures asked for, by outfit and size, oldest first.</summary>
    private static readonly List<(string Key, Standing Picture)> Pictures = new();

    /// <summary>How many Standing pictures are kept.</summary>
    private const int KeptPictures = 12;

    /// <summary>
    /// An outfit put together for a menu (the creation screen, the inventory), scaled so a standard
    /// adult is `height` pixels tall, from the parts at their own size rather than blown up from a
    /// room's picture. The last few are kept; null if the face isn't painted.
    /// </summary>
    /// <param name="outfit">The outfit.</param>
    /// <param name="height">How tall a standard adult is in it, in pixels.</param>
    public static Standing Picture(Outfit outfit, int height)
    {
        if (_device is null || outfit is null) return null;
        string key = outfit.Key + "@" + height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        foreach ((string k, Standing kept) in Pictures)
            if (k == key) return kept;
        if (Composed(outfit) is not (RigComposer.Result result, bool hasBody, _, _)) return null;

        float scale = height / _file.StandardHeight;
        int width = Math.Max(1, (int)MathF.Round(result.Width * scale)), tall = Math.Max(1, (int)MathF.Round(result.Height * scale));
        var picture = new Texture2D(_device, width, tall);
        picture.SetData(ImportedFigure.Scaled(result.Pixels, result.Width, result.Height, width, tall));
        var standing = new Standing(picture, result.Root * new Vector2((float)width / result.Width, (float)tall / result.Height), hasBody);
        Pictures.Add((key, standing));
        if (Pictures.Count > KeptPictures)
        {
            Pictures[0].Picture.Picture.Dispose();
            Pictures.RemoveAt(0);
        }
        return standing;
    }

    /// <summary>
    /// An outfit's parts put together at their own size: the picture, and whether it has a body
    /// (its armor's painted). Null if its face isn't: a bald stranger in their clothes would stand in
    /// for them in the rooms and the portraits, so they keep the look they had till then.
    /// </summary>
    private static (RigComposer.Result Result, bool HasBody, float RigPx, string Base)? Composed(Outfit outfit)
    {
        PartImage face = Part(outfit.Face), armor = Part(outfit.Armor);
        // Till their face is painted, someone is the bare mannequin of their base.
        string baseId = face?.Base ?? armor?.Base ?? outfit.Base;
        if (baseId is null || Skeleton.Current is null || !_file.Bases.TryGetValue(baseId, out PartsFile.BaseEntry b)) return null;
        // Everyone's head is the mannequin's, the face on it. The face and the armor are the body's own,
        // so they're on its base (Check makes sure). A cape, a hair or helm (one slot) or a weapon can
        // be on either: it hangs from its socket here and is scaled to fit (Fit).
        var parts = new List<PartImage>(Drawn(Images[b.Body])) { Images[b.Bald] };
        if (face is not null) parts.Add(face);
        foreach (PartImage worn in new[] { armor, Part(outfit.Cape), Part(OnBase(outfit.Helm, baseId)), Part(outfit.Weapon), Part(outfit.Offhand) })
            if (worn is not null && (worn.Base == baseId || worn.Slot != PartSlot.Armor)) parts.AddRange(Drawn(worn));
        // The beard, the earrings, the scars over the face; the ears and horns over the hair.
        foreach (string extra in outfit.Extras)
            if (Part(OnBase(extra, baseId)) is PartImage worn && worn.Slot is PartSlot.Facial or PartSlot.Marks) parts.Add(worn);

        var sockets = b.Sockets.ToDictionary(s => s.Key, s => new Vector2(s.Value[0], s.Value[1]), StringComparer.Ordinal);
        var joints = b.Joints.ToDictionary(j => j.Key, j => new Vector2(j.Value[0], j.Value[1]), StringComparer.Ordinal);
        RigComposer.Result result = RigComposer.Compose(sockets, b.Head, joints, Skeleton.Current, parts, outfit.Morph, PaintFor(outfit.Tints, face, b), part => Fit(part, baseId));
        return (result, face is null || armor is not null && armor.Base == baseId, b.RigPx, baseId);
    }

    /// <summary>What a part is drawn as: its bones' pieces for a body or an armor, else itself.</summary>
    private static IEnumerable<PartImage> Drawn(PartImage part) => PieceImages.TryGetValue(part.Id, out List<PartImage> pieces) ? pieces : new[] { part };

    /// <summary>An outfit put together, kept by its Key.</summary>
    private static ImportedFigure Build(Outfit outfit)
    {
        if (_device is null) return null;
        string key = outfit.Key;
        if (Built.TryGetValue(key, out ImportedFigure built)) return built;

        if (Composed(outfit) is not (RigComposer.Result result, bool hasBody, float rigPx, string onBase))
        {
            Built[key] = null;
            return null;
        }
        var info = new ImportedArtInfo(outfit.Name, "standard", "person", new[] { result.Width, result.Height },
            new[] { result.Root.X, result.Root.Y }, new[] { result.Head.X, result.Head.Y, result.Head.Width, result.Head.Height }, SourceCanvas);
        if (info.Problem() is string problem)
            throw new InvalidOperationException($"Putting {outfit.Name} together with {outfit.Face} made a picture that {problem}.");

        built = new ImportedFigure(_device, info, result.Pixels, _file.StandardHeight, hasBody, result.Layers, result.Hand, result.Joints, rigPx, onBase);
        Built[key] = built;
        return built;
    }

    /// <summary>
    /// How each part of a figure is coloured: the body and the armor's skin to the face's (or to the
    /// skin picked, against the base's), the face's to the skin picked against its own; a face's hair
    /// and eyes dyed, and what's worn dyed its Base and Trim.
    /// </summary>
    private static Func<PartImage, Paint> PaintFor(Tints tints, PartImage face, PartsFile.BaseEntry b)
    {
        var baseSkin = new Vector3(b.Skin[0], b.Skin[1], b.Skin[2]) / 255f;
        Vector3 Against(Vector3 own) => tints.Skin is Color skin ? Vector3.Clamp(skin.ToVector3() / own, new Vector3(0.2f), new Vector3(4f)) : Vector3.One;
        Vector3 body = tints.Skin is null ? face?.SkinRatio ?? Vector3.One : Against(baseSkin);
        Vector3 faceSkin = face is null ? Vector3.One : Against(baseSkin * face.SkinRatio);
        return part => part.Slot switch
        {
            PartSlot.Body or PartSlot.Head => new Paint(body),
            PartSlot.Helm when part.Id.StartsWith("hair-", StringComparison.Ordinal) => new Paint(Vector3.One, tints.Hair),
            PartSlot.Armor => new Paint(body, tints.Base, tints.Trim),
            PartSlot.Cape or PartSlot.Helm => new Paint(Vector3.One, tints.Base, tints.Trim),
            PartSlot.Face => new Paint(faceSkin, tints.Hair, tints.Eyes),
            // A beard and furred ears are the hair's colour; a scar, freckles or a Sylvan's ear the skin's; metal and horn as painted.
            PartSlot.Facial or PartSlot.Marks => new Paint(faceSkin, tints.Hair),
            _ => Paint.Plain,
        };
    }

    /// <summary>The equipment slot parts.json names (body, cloak, head, mainhand, offhand), or null for one that draws nothing or isn't a slot.</summary>
    private static EquipmentSlot? GearSlot(string name) =>
        Enum.GetValues<EquipmentSlot>().Where(s => Outfit.SlotFor(s) is not null && s.ToString().ToLowerInvariant() == name).Select(s => (EquipmentSlot?)s).FirstOrDefault();

    /// <summary>
    /// How much to scale a part painted on one base so it fits another, measured off the two bases'
    /// sockets by what its slot covers: a helm by the heads' heights; a cape by the backs' widths (back
    /// to chest) and drops (back to root); a weapon by the figures' heights (crown to root). One for a
    /// part on the base it's put on.
    /// </summary>
    /// <param name="part">The part.</param>
    /// <param name="onto">The base it's going on.</param>
    public static Vector2 Fit(PartImage part, string onto)
    {
        if (part.Base == onto || _file is null) return Vector2.One;
        PartsFile.BaseEntry from = _file.Bases[part.Base], to = _file.Bases[onto];
        float Across(PartsFile.BaseEntry b, string a, string c) => b.Sockets[c][0] - b.Sockets[a][0];
        float Down(PartsFile.BaseEntry b, string a, string c) => b.Sockets[c][1] - b.Sockets[a][1];
        return part.Slot switch
        {
            PartSlot.Head or PartSlot.Face or PartSlot.Facial or PartSlot.Helm or PartSlot.Marks => new Vector2(to.Head[3] / from.Head[3]),
            PartSlot.Cape => new Vector2(Across(to, Sockets.Back, Sockets.Chest) / Across(from, Sockets.Back, Sockets.Chest),
                Down(to, Sockets.Back, Sockets.Root) / Down(from, Sockets.Back, Sockets.Root)),
            _ => new Vector2(Down(to, Sockets.Crown, Sockets.Root) / Down(from, Sockets.Crown, Sockets.Root)),
        };
    }

    /// <summary>An outfit put together as a figure (kept, like a recipe's): for a menu that shows someone with one thing changed, a hair to try.</summary>
    /// <param name="outfit">The outfit.</param>
    public static ImportedFigure Of(Outfit outfit) => outfit is null ? null : Build(outfit);

    /// <summary>A part that's been cut, or null for none or one not made yet.</summary>
    private static PartImage Part(string id) => id is not null && Images.TryGetValue(id, out PartImage part) ? part : null;

    /// <summary>The base a recipe's on: its face's, or its armor's, or null if neither's made yet.</summary>
    private static string BaseOf(PartsFile f, PartsFile.RecipeEntry recipe)
    {
        if (recipe.Base is not null) return recipe.Base;
        foreach (string id in new[] { recipe.Face, recipe.Armor })
            if (id is not null && f.Parts.TryGetValue(id, out PartsFile.PartEntry part)) return part.Base;
        return null;
    }

    /// <summary>A rectangle of a page's pixels, row by row.</summary>
    private static Color[] Cut(Color[] page, int stride, int[] rect)
    {
        var pixels = new Color[rect[2] * rect[3]];
        for (int y = 0; y < rect[3]; y++)
            Array.Copy(page, (rect[1] + y) * stride + rect[0], pixels, y * rect[2], rect[2]);
        return pixels;
    }

    /// <summary>Checks everything in parts.json, throwing on the first fault.</summary>
    private static void Check(PartsFile f, string folder)
    {
        const string where = "Content/" + Folder + "/parts.json";
        const string fix = " Run python tools/import_parts.py.";
        if (f is null) throw new InvalidOperationException(where + " is empty.");
        if (f.Version != FileVersion) throw new InvalidOperationException($"{where} is version {f.Version}, but the game reads version {FileVersion}.{fix}");
        if (!(f.Scale > 0f) || !(f.StandardHeight > 0f) || f.Canvas is not { Length: 2 } || f.Canvas[0] <= 0 || f.Canvas[1] <= 0)
            throw new InvalidOperationException($"{where} has no scale, standard height or canvas.{fix}");

        if (f.Sockets is null || !f.Sockets.SequenceEqual(Sockets.All))
            throw new InvalidOperationException($"{where} has the sockets {Join(f.Sockets)}, but the game has {Join(Sockets.All)}: tools/import_parts.py and Characters/Parts/Sockets.cs disagree.");
        PartSlot[] slots = Enum.GetValues<PartSlot>();
        if (f.Order is null || !f.Order.SequenceEqual(slots.Select(Sockets.Name)))
            throw new InvalidOperationException($"{where} draws the slots in the order {Join(f.Order)}, but the game draws {Join(slots.Select(Sockets.Name))}: tools/import_parts.py and Characters/Parts/Sockets.cs disagree.");
        foreach (PartSlot slot in slots)
        {
            if (f.SlotSockets is null || !f.SlotSockets.TryGetValue(Sockets.Name(slot), out string socket) || socket != Sockets.Of(slot))
                throw new InvalidOperationException($"{where} hangs the {Sockets.Name(slot)} from another socket than the game's {Sockets.Of(slot)}: tools/import_parts.py and Characters/Parts/Sockets.cs disagree.");
        }

        if (f.Pages is null || f.Bases is null || f.Parts is null || f.Recipes is null)
            throw new InvalidOperationException($"{where} is missing its pages, bases, parts or recipes.{fix}");
        if (f.Bones is not { Length: > 0 } || f.Bones.Any(b => b is not { Length: 3 } || b[0] is null || b[2] is null) || f.Draw is null)
            throw new InvalidOperationException($"{where} has no skeleton: its bones and the order they're drawn in.{fix}");
        var boneNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (string[] b in f.Bones)
        {
            if (b[1] is not null && !boneNames.Contains(b[1])) throw new InvalidOperationException($"{where} has the bone {b[0]} before its parent {b[1]}.{fix}");
            boneNames.Add(b[0]);
        }
        if (f.Draw.Any(b => !boneNames.Contains(b))) throw new InvalidOperationException($"{where} draws a bone it hasn't.{fix}");
        foreach ((string name, int[] size) in f.Pages)
        {
            if (size is not { Length: 2 } || size[0] <= 0 || size[1] <= 0)
                throw new InvalidOperationException($"{where} gives the page {name} no size.{fix}");
            if (!File.Exists(Path.Combine(folder, name + ".xnb")))
                throw new InvalidOperationException($"{where} has the page {name}, but it wasn't built: Content/{Folder}/{name}.png isn't in Content.mgcb.{fix}");
        }

        foreach ((string id, PartsFile.BaseEntry b) in f.Bases)
        {
            foreach (string socket in Sockets.All)
            {
                if (b.Sockets is null || !b.Sockets.TryGetValue(socket, out float[] at) || at is not { Length: 2 } || !Inside(at[0], f.Canvas[0]) || !Inside(at[1], f.Canvas[1] + 1))
                    throw new InvalidOperationException($"{where} has no {socket} socket on {id}, or it's off the canvas.{fix}");
            }
            if (b.Head is not { Length: 4 } || !(b.Head[2] > 0f) || !(b.Head[3] > 0f))
                throw new InvalidOperationException($"{where} has no head box on {id}.{fix}");
            if (b.Skin is not { Length: 3 } || b.Skin.Any(v => !(v >= 1f && v <= 255f)))
                throw new InvalidOperationException($"{where} has no skin tone on {id}.{fix}");
            if (!IsPart(f, b.Body, PartSlot.Body, id) || !IsPart(f, b.Bald, PartSlot.Head, id))
                throw new InvalidOperationException($"{where} gives {id} a body ({b.Body}) or a bald head ({b.Bald}) that isn't its own.{fix}");
            foreach (string[] bone in f.Bones)
            {
                if (b.Joints is null || !b.Joints.TryGetValue(bone[2], out float[] joint) || joint is not { Length: 2 } || !float.IsFinite(joint[0]) || !float.IsFinite(joint[1]))
                    throw new InvalidOperationException($"{where} has no {bone[2]} joint on {id}, which its {bone[0]} turns about.{fix}");
            }
            if (!(b.RigPx > 0f)) throw new InvalidOperationException($"{where} gives {id} no size for the clips' units.{fix}");
        }

        foreach ((string id, PartsFile.PartEntry p) in f.Parts)
        {
            string what = $"{where}'s {id}";
            if (!Sockets.TryParse(p.Slot, out PartSlot slot)) throw new InvalidOperationException($"{what} is in the slot {p.Slot}, which there isn't.{fix}");
            if (p.Base is null || !f.Bases.ContainsKey(p.Base)) throw new InvalidOperationException($"{what} is on the base {p.Base}, which there isn't.{fix}");
            if (p.Socket != Sockets.Of(slot)) throw new InvalidOperationException($"{what} hangs from {p.Socket}, but a {p.Slot} hangs from {Sockets.Of(slot)}.{fix}");
            void Placed(string piece, string pageName, int[] rect, int[] maskRect, float[] pivot)
            {
                if (pageName is null || !f.Pages.TryGetValue(pageName, out int[] page)) throw new InvalidOperationException($"{piece} is on the page {pageName}, which there isn't.{fix}");
                if (!OnPage(rect, page)) throw new InvalidOperationException($"{piece} isn't inside its page {pageName}.{fix}");
                if (maskRect is not null && (!OnPage(maskRect, page) || maskRect[2] != rect[2] || maskRect[3] != rect[3]))
                    throw new InvalidOperationException($"{piece}'s mask isn't inside its page, or isn't its size.{fix}");
                if (pivot is not { Length: 2 } || !float.IsFinite(pivot[0]) || !float.IsFinite(pivot[1]))
                    throw new InvalidOperationException($"{piece} has no pivot.{fix}");
            }
            if (p.Pieces is null) Placed(what, p.Page, p.Rect, p.MaskRect, p.Pivot);
            else
            {
                if (slot is not (PartSlot.Body or PartSlot.Armor) || p.Pieces.Count == 0)
                    throw new InvalidOperationException($"{what} is in pieces, which only a body or an armor is, and has to have some.{fix}");
                foreach ((string bone, PartsFile.PieceEntry piece) in p.Pieces)
                {
                    if (!boneNames.Contains(bone)) throw new InvalidOperationException($"{what} has a piece for {bone}, which isn't a bone.{fix}");
                    Placed($"{what}'s {bone}", piece.Page, piece.Rect, piece.MaskRect, piece.Pivot);
                    if (p.Tones is not null && piece.MaskRect is null) throw new InvalidOperationException($"{what}'s {bone} has no mask for its dyes.{fix}");
                }
            }
            bool dyed = slot is PartSlot.Face or PartSlot.Facial or PartSlot.Armor or PartSlot.Cape or PartSlot.Helm or PartSlot.Marks;
            if (p.Tones is not null && (!dyed || p.MaskRect is null && p.Pieces is null || p.Tones.Length != 2
                || p.Tones.Any(t => t is not null && (t.Length != 3 || t.Any(v => !(v >= 0f && v <= 255f))))))
                throw new InvalidOperationException($"{what} has dye tones it can't have: only a face, a facial part, armor, cape, helm (a hair among them) or mark with a mask has two, each a colour or none.{fix}");
            bool face = slot == PartSlot.Face;
            if (face != (p.Skin is not null) || face && (p.Skin.Length != 3 || p.Skin.Any(v => !(v >= 0.2f && v <= 2f))))
                throw new InvalidOperationException($"{what} {(face ? "has no skin against its base's, or one out of reach" : "has a skin of its own, which only a face has")}.{fix}");
        }

        foreach ((string id, PartsFile.RecipeEntry r) in f.Recipes)
        {
            string what = $"{where}'s recipe {id}";
            if (r.Morph is not { Length: 2 } || r.Morph.Any(v => !(v >= Morph.Least && v <= Morph.Most)))
                throw new InvalidOperationException($"{what} has a morph out of reach: each way it's {Morph.Least} to {Morph.Most}. Fix it in Story/13-art-prompts.md §7.");
            foreach ((string channel, string name) in r.Tints ?? new Dictionary<string, string>())
            {
                IReadOnlyList<Swatches.Swatch> list = channel switch { "skin" => Swatches.Skin, "hair" => Swatches.Hair, "eyes" => Swatches.Eyes, _ => null };
                if (list is null || Swatches.Named(list, name) is null)
                    throw new InvalidOperationException($"{what} colours its {channel} {name}, which isn't one of the game's swatches. Fix it in Story/13-art-prompts.md §7.");
            }
            foreach (string extra in r.Extras ?? Array.Empty<string>())
            {
                if (extra is null || f.Parts.TryGetValue(extra, out PartsFile.PartEntry entry) && entry.Slot != Sockets.Name(PartSlot.Facial) && entry.Slot != Sockets.Name(PartSlot.Marks))
                    throw new InvalidOperationException($"{what} wears {extra ?? "nothing"} as an extra, but that isn't a facial part or a mark. Fix it in Story/13-art-prompts.md §7.");
            }
            foreach ((string part, PartSlot slot) in new[] { (r.Face, PartSlot.Face), (r.Armor, PartSlot.Armor), (r.Cape, PartSlot.Cape), (r.Helm, PartSlot.Helm), (r.Weapon, PartSlot.Weapon) })
            {
                if (part is not null && f.Parts.TryGetValue(part, out PartsFile.PartEntry entry) && entry.Slot != Sockets.Name(slot))
                    throw new InvalidOperationException($"{what} wears {part} as its {Sockets.Name(slot)}, but that's a {entry.Slot}. Fix it in Story/13-art-prompts.md §7.");
            }
            // The face and the armor are the body: they have to be painted on the same base. A cape, a
            // helm or a weapon hangs from its socket on either, scaled to fit (Fit).
            if (r.Face is not null && r.Armor is not null && f.Parts.TryGetValue(r.Face, out PartsFile.PartEntry faceEntry)
                && f.Parts.TryGetValue(r.Armor, out PartsFile.PartEntry armorEntry) && faceEntry.Base != armorEntry.Base)
                throw new InvalidOperationException($"{what} puts {r.Face}, painted on {faceEntry.Base}, on {r.Armor}, painted on {armorEntry.Base}: a face and its armor have to be on one base. Give it an armor made on {faceEntry.Base} in Story/13-art-prompts.md §7.");
        }

        if (f.Gear is null || f.Own is null)
            throw new InvalidOperationException($"{where} has no gear: what the items draw, from Story/13-art-prompts.md §7.1 and §7.2.{fix}");
        var seen = new HashSet<(string, EquipmentSlot)>();
        foreach (PartsFile.GearEntry g in f.Gear)
        {
            string what = $"{where}'s gear for {g?.Item}";
            EquipmentSlot slot = Worn(g?.Item, g?.Slot, what, "§7.1");
            if (!seen.Add((g.Item, slot))) throw new InvalidOperationException($"{what} in the {g.Slot} is there twice. Fix it in Story/13-art-prompts.md §7.1.");
            if (g.Parts is null || !f.Bases.Keys.All(g.Parts.ContainsKey) || g.Parts.Keys.Any(k => !f.Bases.ContainsKey(k)))
                throw new InvalidOperationException($"{what} doesn't say what it draws on each base, and only those.{fix}");
            foreach ((string onBase, string part) in g.Parts) Piece(f, part, slot, onBase, what, "§7.1");
        }
        foreach ((string who, PartsFile.OwnEntry[] rows) in f.Own)
        {
            if (!f.Recipes.TryGetValue(who, out PartsFile.RecipeEntry recipe) || rows is null)
                throw new InvalidOperationException($"{where} gives {who} their own gear, but there's no recipe {who}. Fix it in Story/13-art-prompts.md §7.2.");
            var mine = new HashSet<(EquipmentSlot, string)>();
            foreach (PartsFile.OwnEntry row in rows)
            {
                string what = $"{where}'s own gear for {who}'s {row?.Item}";
                EquipmentSlot slot = Worn(row?.Item, row?.Slot, what, "§7.2");
                if (!mine.Add((slot, row.Item))) throw new InvalidOperationException($"{what} in the {row.Slot} is there twice. Fix it in Story/13-art-prompts.md §7.2.");
                Piece(f, row.Part, slot, BaseOf(f, recipe), what, "§7.2");
            }
        }
    }

    /// <summary>The slot a gear row puts its item in, having checked the item's one there is and goes there.</summary>
    private static EquipmentSlot Worn(string item, string slotName, string what, string section)
    {
        if (item is null || !ItemCatalog.Exists(item))
            throw new InvalidOperationException($"{what} isn't an item in Items/ItemCatalog.cs. Fix it in Story/13-art-prompts.md {section}.");
        if (GearSlot(slotName) is not EquipmentSlot slot || !ItemCatalog.Get(item).Fits(slot))
            throw new InvalidOperationException($"{what} is in the slot {slotName ?? "none"}, where it can't be worn, or that draws nothing. Fix it in Story/13-art-prompts.md {section}.");
        return slot;
    }

    /// <summary>Checks a part a gear row draws, if it's been made: it's in the slot the equipment slot fills, and an armor's on the base it's listed for.</summary>
    private static void Piece(PartsFile f, string part, EquipmentSlot slot, string onBase, string what, string section)
    {
        if (part is null || !f.Parts.TryGetValue(part, out PartsFile.PartEntry entry)) return;
        PartSlot fills = Outfit.SlotFor(slot).Value;
        if (entry.Slot != Sockets.Name(fills))
            throw new InvalidOperationException($"{what} draws {part}, a {entry.Slot}, where the {slot} needs a {Sockets.Name(fills)}. Fix it in Story/13-art-prompts.md {section}.");
        if (fills == PartSlot.Armor && onBase is not null && entry.Base != onBase)
            throw new InvalidOperationException($"{what} draws {part}, painted on {entry.Base}, on {onBase}: an armor has to be on its wearer's base. Fix it in Story/13-art-prompts.md {section}.");
    }

    private static bool IsPart(PartsFile f, string id, PartSlot slot, string onBase) =>
        id is not null && f.Parts.TryGetValue(id, out PartsFile.PartEntry p) && p.Slot == Sockets.Name(slot) && p.Base == onBase;

    private static bool OnPage(int[] rect, int[] page) =>
        rect is { Length: 4 } && rect[0] >= 0 && rect[1] >= 0 && rect[2] > 0 && rect[3] > 0 && rect[0] + rect[2] <= page[0] && rect[1] + rect[3] <= page[1];

    private static bool Inside(float value, float size) => float.IsFinite(value) && value >= 0f && value <= size;

    private static string Join(IEnumerable<string> names) => names is null ? "none" : string.Join(", ", names);
}
