using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;
using LegendQuest.Story;

namespace LegendQuest.Characters;

/// <summary>
/// The characters' pictures made in ChatGPT and cut by tools/import_character.py: every
/// Content/Characters/Art/(id).png there is, with its .json, ready to draw in place of the
/// paperdoll or the sheet for whoever has that id.
/// </summary>
/// <remarks>
/// <para>
/// LoadAll reads and checks every .json the build put beside the game, as the game starts and with
/// the rest of the data, so --validate checks them too: its id has to be someone's in the game's
/// tables (an origin, a companion with an approval meter, one of the Cast, an enemy, a creature or a
/// townsfolk sheet) or a variant of one (id.variant), its numbers have to make sense, and its
/// picture has to have been built. LoadContent then makes every one of them ready to draw
/// (ImportedFigure), and Get hands them out. Whoever hasn't a picture keeps the look they had, so
/// the game works whatever's been imported so far.
/// </para>
/// <para>
/// Some ids have no picture of their own and draw with someone else's: SharedPictures. They're the
/// entries Story/13-art-prompts.md says have no separate prompt, the foes who are one of the Cast in
/// a fight. tools/import_character.py reads the table to refuse them and to say who else a picture
/// draws, so each pair stays on a line of its own.
/// </para>
/// </remarks>
public static class ImportedArt
{
    /// <summary>Where the pictures and their .json are, under Content.</summary>
    public const string Folder = "Characters/Art";

    /// <summary>How tall a standard adult is in the files, from the top of the head to the feet, in their pixels. tools/import_character.py cuts every picture to this, times its build's or its size's share.</summary>
    public const int FileHeight = 512;

    /// <summary>How tall a standard adult stands in a room, in canvas pixels (03-art-overhaul.md §2.4).</summary>
    public const int RoomHeight = 196;

    /// <summary>The ids drawn with someone else's picture, and whose: the enemies Story/13-art-prompts.md §3.7.1 gives the same picture as one of the Cast.</summary>
    private static readonly Dictionary<string, string> SharedPictures = new()
    {
        ["red-gull-thug"] = "red-gull",
        ["bosun-grell"] = "grell",
    };

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Every picture there is, by id, as LoadAll read them, and each made ready to draw by LoadContent.</summary>
    private static readonly SortedDictionary<string, ImportedArtInfo> Infos = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, ImportedFigure> Figures = new();

    /// <summary>Reads and checks every imported picture's .json in the built game, and the shared pictures against the game's data.</summary>
    /// <remarks>Throws on the first fault, with the file, so a bad import stops the game as it starts rather than the first time someone's drawn.</remarks>
    public static void LoadAll()
    {
        foreach ((string id, string picture) in SharedPictures)
        {
            if (!EnemyCatalog.Exists(id) || !Knows(picture))
                throw new InvalidOperationException($"ImportedArt draws {id} with {picture}'s picture, but {id} isn't an enemy or {picture} isn't anyone.");
        }

        Infos.Clear();
        string folder = Path.Combine(AppContext.BaseDirectory, "Content", Folder);
        if (!Directory.Exists(folder)) return;

        foreach (string path in Directory.GetFiles(folder, "*.json"))
        {
            string id = Path.GetFileNameWithoutExtension(path);
            string where = $"Content/{Folder}/{id}.json";
            ImportedArtInfo info;
            try
            {
                info = JsonSerializer.Deserialize<ImportedArtInfo>(File.ReadAllText(path), Options);
            }
            catch (JsonException unreadable)
            {
                throw new InvalidOperationException($"{where} isn't readable: {unreadable.Message}", unreadable);
            }

            string whose = id.Split('.')[0];
            if (info is null || info.Id != id)
                throw new InvalidOperationException($"{where} says it's {info?.Id ?? "nobody"}'s picture, not {id}'s.");
            if (!Knows(whose))
                throw new InvalidOperationException($"{where} is for {id}, who isn't anyone in the game: an origin, a companion, one of the Cast, an enemy, a creature or a townsfolk sheet, or a variant of one.");
            if (SharedPictures.TryGetValue(whose, out string shared))
                throw new InvalidOperationException($"{where} is for {whose}, who's drawn with {shared}'s picture. Import it as {shared} instead.");
            if (info.Problem() is string problem)
                throw new InvalidOperationException($"{where} {problem}. Import it again with tools/import_character.py.");
            if (!File.Exists(Path.ChangeExtension(path, ".xnb")))
                throw new InvalidOperationException($"{where} has no picture built beside it: Content/{Folder}/{id}.png isn't in Content.mgcb.");
            Infos[id] = info;
        }
    }

    /// <summary>Makes every picture LoadAll found ready to draw, each read through a content manager of its own that lets the file's picture go once it's been shrunk.</summary>
    /// <remarks>Throws if a picture isn't the size its .json says: one was cut again and the other wasn't.</remarks>
    /// <param name="services">The game's services, for the content manager.</param>
    /// <param name="device">The device to make the figures' textures on.</param>
    public static void LoadContent(IServiceProvider services, GraphicsDevice device)
    {
        Figures.Clear();
        using var content = new ContentManager(services, "Content");
        foreach ((string id, ImportedArtInfo info) in Infos)
        {
            var picture = content.Load<Texture2D>(Folder + "/" + id);
            if (picture.Width != info.Size[0] || picture.Height != info.Size[1])
                throw new InvalidOperationException($"Content/{Folder}/{id}.png is {picture.Width} by {picture.Height}, but its .json says {info.Size[0]} by {info.Size[1]}. Import it again with tools/import_character.py.");

            var pixels = new Color[picture.Width * picture.Height];
            picture.GetData(pixels);
            Figures[id] = new ImportedFigure(device, info, pixels);
        }
    }

    /// <summary>The picture someone's drawn as: their own, or for an id in SharedPictures the one it shares; failing that, the one their recipe puts together from the parts made so far (PartCatalog); or null if there's none and they keep the look they have.</summary>
    /// <param name="id">Who: an origin, cast, enemy, creature or townsfolk sheet id, or the name of one of the Cast's looks. Null is nobody.</param>
    public static ImportedFigure Get(string id)
    {
        if (id is null) return null;
        if (Figures.TryGetValue(id, out ImportedFigure figure)) return figure;
        if (SharedPictures.TryGetValue(id, out string shared)) return Figures.GetValueOrDefault(shared) ?? PartCatalog.Figure(shared);
        return PartCatalog.Figure(id);
    }

    /// <summary>A party member's picture: put together with the painted face they picked on the creation screen if they picked one, or else whatever Get gives for them.</summary>
    /// <param name="member">Who.</param>
    public static ImportedFigure For(PartyMember member) => PartCatalog.Dress(member) ?? Get(member.Id);

    /// <summary>Whether an id is someone's in the game's tables.</summary>
    private static bool Knows(string id) =>
        Origins.Exists(id) || Meters.IsCompanion(id) || Cast.Get(id) is not null || EnemyCatalog.Exists(id) || Creatures.Get(id) is not null || Townsfolk.Contains(id);
}
