using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// Reads a .map file out of Content/Maps and turns it into a MapDefinition, checking everything on the way.
/// </summary>
/// <remarks>
/// It's my tilemap exercise's loader grown up: the exercise read a tileset name, a tile size, a map
/// size and a list of ids, and this reads the same things out of sections, with the tiles laid out
/// as ASCII like The Black Box's lobby so I can see the map while I edit it. Two fixes from the
/// exercise are in here. Every line gets trimmed, because the exercise kept the \r off Windows line
/// endings on the end of each one. And every row is checked against the map's size, because the
/// exercise took the id list on trust and a short one just read past the end. Anything wrong throws
/// with the file and line, and MapCatalog loads every map at startup, so I find out right away.
/// </remarks>
public static class MapLoader
{
    /// <summary>Where Content.mgcb copies the map files, relative to the game.</summary>
    private const string Folder = "Content/Maps/";

    /// <summary>An empty cell in a tile layer.</summary>
    private const char Empty = '.';

    /// <summary>Starts a comment, anywhere on a line. It can't stand for a tile, so a row can't have one in it.</summary>
    private const char Comment = ';';

    /// <summary>The spawn point every map needs. It's where the party arrives when nothing says otherwise.</summary>
    public const string StartSpawn = "start";

    private static readonly string[] Sections = { "map", "legend", "ground", "detail", "overhead", "objects" };
    private static readonly string[] RequiredSettings = { "id", "name", "tileset", "size", "dark", "music" };

    /// <summary>Reads and checks one map.</summary>
    /// <param name="id">The map, which is also its file name without the .map.</param>
    /// <returns>The map.</returns>
    /// <exception cref="InvalidOperationException">If the file is wrong in any way. The message says where.</exception>
    public static MapDefinition Load(string id)
    {
        string text;
        using (Stream stream = TitleContainer.OpenStream(Folder + id + ".map"))
        using (var reader = new StreamReader(stream))
            text = reader.ReadToEnd();

        return Parse(id, text);
    }

    /// <summary>Turns a map file's text into a map.</summary>
    /// <param name="id">The map's id, for the error messages and to check the file says the same.</param>
    /// <param name="text">Everything in the file.</param>
    private static MapDefinition Parse(string id, string text)
    {
        Dictionary<string, List<(int Line, string Text)>> sections = SplitSections(id, text);

        if (!sections.TryGetValue("map", out var mapLines)) throw Fail(id, "there's no [map] section");
        Dictionary<string, (int Line, string Value)> settings = ReadSettings(id, mapLines);

        if (settings["id"].Value != id)
            throw Fail(id, settings["id"].Line, $"the file says it's {settings["id"].Value}, but it's called {id}.map");

        if (!TilesetCatalog.TryGet(settings["tileset"].Value, out TilesetDefinition tileset))
            throw Fail(id, settings["tileset"].Line, $"there's no tileset called {settings["tileset"].Value}");

        (int width, int height) = ReadSize(id, settings["size"]);
        bool dark = ReadYesNo(id, settings["dark"]);
        float ambient = ReadAmbient(id, settings, dark);

        if (!Enum.TryParse(settings["music"].Value, out Track music) || !Enum.IsDefined(music))
            throw Fail(id, settings["music"].Line, $"there's no song called {settings["music"].Value} (the Track names are {string.Join(", ", Enum.GetNames<Track>())})");

        if (!sections.TryGetValue("legend", out var legendLines)) throw Fail(id, "there's no [legend] section");
        Dictionary<char, int> legend = ReadLegend(id, legendLines, tileset);

        if (!sections.TryGetValue("ground", out var groundLines)) throw Fail(id, "there's no [ground] section");
        int[,] ground = ReadLayer(id, "ground", groundLines, legend, width, height, emptyAllowed: false);
        int[,] detail = sections.TryGetValue("detail", out var detailLines)
            ? ReadLayer(id, "detail", detailLines, legend, width, height, emptyAllowed: true)
            : new int[width, height];
        int[,] overhead = sections.TryGetValue("overhead", out var overheadLines)
            ? ReadLayer(id, "overhead", overheadLines, legend, width, height, emptyAllowed: true)
            : new int[width, height];

        var spawns = new Dictionary<string, Vector2>();
        var props = new List<MapProp>();
        if (sections.TryGetValue("objects", out var objectLines))
            ReadObjects(id, objectLines, width, height, spawns, props);

        if (!spawns.ContainsKey(StartSpawn))
            throw Fail(id, $"there's no spawn called {StartSpawn}, and every map needs one");

        return new MapDefinition
        {
            Id = id,
            Name = settings["name"].Value,
            Tileset = tileset,
            Dark = dark,
            Ambient = ambient,
            Music = music,
            Width = width,
            Height = height,
            Ground = ground,
            Detail = detail,
            Overhead = overhead,
            Spawns = spawns,
            Props = props,
        };
    }

    /// <summary>Splits the file into its sections, dropping comments and blank lines. Every line is trimmed.</summary>
    private static Dictionary<string, List<(int Line, string Text)>> SplitSections(string id, string text)
    {
        var sections = new Dictionary<string, List<(int, string)>>();
        List<(int, string)> current = null;
        string[] lines = text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            int comment = line.IndexOf(Comment);
            if (comment >= 0) line = line[..comment];

            // Trim gets the \r off a Windows line ending, which is the bug the exercise had.
            line = line.Trim();
            if (line.Length == 0) continue;

            if (line[0] == '[')
            {
                if (line[^1] != ']') throw Fail(id, i + 1, "a section name has no closing ]");

                string name = line[1..^1].Trim().ToLowerInvariant();
                if (Array.IndexOf(Sections, name) < 0)
                    throw Fail(id, i + 1, $"there's no [{name}] section in a map (the sections are {string.Join(", ", Sections)})");
                if (sections.ContainsKey(name)) throw Fail(id, i + 1, $"[{name}] comes twice");

                current = new List<(int, string)>();
                sections[name] = current;
                continue;
            }

            if (current is null) throw Fail(id, i + 1, "this line comes before any section");
            current.Add((i + 1, line));
        }

        return sections;
    }

    /// <summary>Reads the key = value lines in [map], and checks the ones every map needs are there.</summary>
    private static Dictionary<string, (int Line, string Value)> ReadSettings(string id, List<(int Line, string Text)> lines)
    {
        var settings = new Dictionary<string, (int, string)>();

        foreach ((int line, string text) in lines)
        {
            int equals = text.IndexOf('=');
            if (equals < 0) throw Fail(id, line, "a [map] line has to be key = value");

            string key = text[..equals].Trim().ToLowerInvariant();
            string value = text[(equals + 1)..].Trim();

            if (Array.IndexOf(RequiredSettings, key) < 0 && key != "ambient")
                throw Fail(id, line, $"[map] has no setting called {key}");
            if (!settings.TryAdd(key, (line, value))) throw Fail(id, line, $"{key} is set twice");
        }

        foreach (string key in RequiredSettings)
        {
            if (!settings.ContainsKey(key)) throw Fail(id, $"[map] doesn't set {key}");
        }

        return settings;
    }

    /// <summary>Reads size = width height.</summary>
    private static (int Width, int Height) ReadSize(string id, (int Line, string Value) setting)
    {
        string[] parts = setting.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int width)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int height)
            || width < 1 || height < 1)
        {
            throw Fail(id, setting.Line, $"size has to be the width and height in tiles, like 40 30, not {setting.Value}");
        }

        return (width, height);
    }

    /// <summary>Reads a yes or no.</summary>
    private static bool ReadYesNo(string id, (int Line, string Value) setting) => setting.Value.ToLowerInvariant() switch
    {
        "yes" or "true" => true,
        "no" or "false" => false,
        _ => throw Fail(id, setting.Line, $"that has to be yes or no, not {setting.Value}"),
    };

    /// <summary>Reads ambient, which only a dark map has.</summary>
    /// <remarks>On a lit map there's no darkness for it to be the level of, so it's a mistake to set it there.</remarks>
    private static float ReadAmbient(string id, Dictionary<string, (int Line, string Value)> settings, bool dark)
    {
        bool set = settings.TryGetValue("ambient", out (int Line, string Value) ambient);

        if (!dark)
        {
            if (set) throw Fail(id, ambient.Line, "ambient only means something on a dark map");
            return 1f;
        }

        if (!set) throw Fail(id, "a dark map has to set ambient, how much light gets everywhere (0 to 1)");
        if (!float.TryParse(ambient.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float level) || level < 0f || level > 1f)
            throw Fail(id, ambient.Line, $"ambient has to be a number from 0 to 1, not {ambient.Value}");

        return level;
    }

    /// <summary>Reads the [legend]: one character per tile, by the tile's name in the tileset.</summary>
    private static Dictionary<char, int> ReadLegend(string id, List<(int Line, string Text)> lines, TilesetDefinition tileset)
    {
        var legend = new Dictionary<char, int>();

        foreach ((int line, string text) in lines)
        {
            int equals = text.IndexOf('=', 1);
            string key = equals < 0 ? text : text[..equals].Trim();
            if (equals < 0 || key.Length != 1)
                throw Fail(id, line, "a [legend] line has to be one character = a tile's name");

            // A ; never gets this far, since it's a comment.
            char c = key[0];
            if (c is Empty or '[')
                throw Fail(id, line, $"'{c}' can't stand for a tile ('{Empty}' is an empty cell, '[' starts a section)");

            string name = text[(equals + 1)..].Trim();
            if (!tileset.TryGetId(name, out int tile))
                throw Fail(id, line, $"the {tileset.Id} tileset has no tile called {name}");
            if (!legend.TryAdd(c, tile)) throw Fail(id, line, $"'{c}' is in the legend twice");
        }

        return legend;
    }

    /// <summary>Reads one layer of tiles, checking every row is the map's width and there are as many rows as it's tall.</summary>
    /// <remarks>Indexed [x, y], like The Black Box's lobby.</remarks>
    private static int[,] ReadLayer(string id, string name, List<(int Line, string Text)> lines, Dictionary<char, int> legend,
        int width, int height, bool emptyAllowed)
    {
        if (lines.Count != height)
            throw Fail(id, $"[{name}] has {lines.Count} rows; the map is {height} tall (see size in [map])");

        var layer = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            (int line, string row) = lines[y];
            if (row.Length != width)
                throw Fail(id, line, $"[{name}] row {y + 1} is {row.Length} tiles; the map is {width} wide (see size in [map])");

            for (int x = 0; x < width; x++)
            {
                char c = row[x];
                if (c == Empty)
                {
                    if (!emptyAllowed) throw Fail(id, line, $"[{name}] has an empty cell at column {x + 1}, and every ground cell needs a tile");
                    continue;
                }

                if (!legend.TryGetValue(c, out int tile))
                    throw Fail(id, line, $"'{c}' at column {x + 1} isn't in the [legend]");
                layer[x, y] = tile;
            }
        }

        return layer;
    }

    /// <summary>Reads the [objects]: one per line, kind id x y, with x and y in tiles.</summary>
    /// <remarks>An object at x, y stands with its feet on the middle of that tile. Halves are fine, for anything that sits between two.</remarks>
    private static void ReadObjects(string id, List<(int Line, string Text)> lines, int width, int height,
        Dictionary<string, Vector2> spawns, List<MapProp> props)
    {
        foreach ((int line, string text) in lines)
        {
            string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) throw Fail(id, line, "an object line has to be kind id x y");

            string kind = parts[0].ToLowerInvariant();
            string name = parts[1];

            if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                throw Fail(id, line, $"{parts[2]} {parts[3]} isn't a place on the map (x and y are in tiles)");
            if (x < 0f || y < 0f || x >= width || y >= height)
                throw Fail(id, line, $"{x} {y} is off the map, which is {width} by {height}");

            if (parts.Length > 4) throw Fail(id, line, $"a {kind} has nothing to set, but this line sets {parts[4]}");

            var at = new Vector2((x + 0.5f) * Tilemap.TileSize, (y + 0.5f) * Tilemap.TileSize);

            switch (kind)
            {
                case "spawn":
                    if (!spawns.TryAdd(name, at)) throw Fail(id, line, $"there are two spawns called {name}");
                    break;

                case "prop":
                    if (!PropCatalog.TryGet(name, out PropDefinition prop)) throw Fail(id, line, $"there's no prop called {name}");
                    props.Add(new MapProp(prop, at));
                    break;

                default:
                    throw Fail(id, line, $"there's no kind of object called {kind} (so far there's spawn and prop)");
            }
        }
    }

    /// <summary>An error about one line of a map file.</summary>
    private static InvalidOperationException Fail(string id, int line, string message) =>
        new($"Maps/{id}.map line {line}: {message}.");

    /// <summary>An error about a map file as a whole.</summary>
    private static InvalidOperationException Fail(string id, string message) =>
        new($"Maps/{id}.map: {message}.");
}
