using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Dialogue;
using LegendQuest.Story;

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
    private static readonly string[] OptionalSettings = { "world", "ambient", "fog" };

    /// <summary>Every kind of object, for the error message when a line names one that isn't.</summary>
    private static readonly string[] Kinds = { "spawn", "prop", "npc", "light", "exit", "entrance", "trigger", "scene", "chest", "door", "lever", "sign", "waypoint" };

    /// <summary>The doorway an entrance makes, in art pixels: wide enough to walk into, and so shallow you have to be walking into the door to go through.</summary>
    private const float DoorwayWidth = 18f;
    private const float DoorwayDepth = 4f;

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

        (int width, int height) = ReadPair(id, settings["size"], "size", "the width and height in tiles, like 40 30");
        if (width < 1 || height < 1) throw Fail(id, settings["size"].Line, "a map has to be at least one tile each way");

        Point? world = null;
        if (settings.TryGetValue("world", out (int Line, string Value) worldSetting))
        {
            (int worldX, int worldY) = ReadPair(id, worldSetting, "world", "where it is on the world map, like 300 200");
            if (worldX > MapCatalog.WorldWidth || worldY > MapCatalog.WorldHeight)
                throw Fail(id, worldSetting.Line, $"world has to be inside the world map, which is {MapCatalog.WorldWidth} by {MapCatalog.WorldHeight}");
            world = new Point(worldX, worldY);
        }

        bool dark = ReadYesNo(id, settings["dark"]);
        float ambient = ReadAmbient(id, settings, dark);
        bool fog = !settings.TryGetValue("fog", out (int Line, string Value) fogSetting) || ReadYesNo(id, fogSetting);

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

        List<MapObjectLine> objects = sections.TryGetValue("objects", out var objectLines)
            ? ReadObjects(id, objectLines, width, height)
            : new List<MapObjectLine>();

        // Solid the same way Tilemap works it out, for checking where things stand.
        bool Solid(int x, int y) => x < 0 || y < 0 || x >= width || y >= height
            || tileset[ground[x, y]].Solid || detail[x, y] != 0 && tileset[detail[x, y]].Solid;

        Dictionary<string, MapSpawn> spawns = ReadSpawns(id, objects);
        List<MapExit> exits = ReadAreas(objects, "exit", width, height, ReadExit);
        exits.AddRange(ReadEntrances(objects));
        List<MapTrigger> triggers = ReadAreas(objects, "trigger", width, height, ReadTrigger);
        List<MapInteractable> interactables = ReadInteractables(objects, Solid);

        foreach ((string name, MapSpawn spawn) in spawns)
        {
            foreach (MapExit exit in exits)
                if (Inside(spawn.Feet, exit.Area)) throw Fail(id, $"spawn {name} is inside exit {exit.Id}; move it a tile or so in from the way out");
        }

        // Fast travel picks a waypoint by where its map is on the world map.
        if (world is null && interactables.Exists(thing => thing.Kind == InteractableKind.Waypoint))
            throw Fail(id, "there's a waypoint on a map that isn't on the world map (it needs world =)");

        return new MapDefinition
        {
            Id = id,
            Name = settings["name"].Value,
            World = world,
            Tileset = tileset,
            Dark = dark,
            Ambient = ambient,
            Fog = fog,
            Music = music,
            Width = width,
            Height = height,
            Ground = ground,
            Detail = detail,
            Overhead = overhead,
            Spawns = spawns,
            Props = ReadProps(objects),
            Lights = ReadLights(objects),
            Exits = exits,
            Triggers = triggers,
            Interactables = interactables,
            Npcs = ReadNpcs(objects, Solid),
            Scenes = ReadScenes(objects),
        };
    }

    /// <summary>The scenes: scene id x y talk=, and when= and unless=. Where it is doesn't matter; it plays wherever you are.</summary>
    private static List<MapScene> ReadScenes(List<MapObjectLine> objects)
    {
        var scenes = new List<MapScene>();

        foreach (MapObjectLine o in Of(objects, "scene"))
        {
            string talk = o.Required("talk");
            if (DialogueCatalog.Get(talk) is null) throw o.Fail($"there's no conversation called {talk}");
            scenes.Add(new MapScene(o.Id, talk, o.Presence()));
            o.Done("talk, when, unless");
        }

        return scenes;
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

            if (Array.IndexOf(RequiredSettings, key) < 0 && Array.IndexOf(OptionalSettings, key) < 0)
                throw Fail(id, line, $"[map] has no setting called {key}");
            if (!settings.TryAdd(key, (line, value))) throw Fail(id, line, $"{key} is set twice");
        }

        foreach (string key in RequiredSettings)
        {
            if (!settings.ContainsKey(key)) throw Fail(id, $"[map] doesn't set {key}");
        }

        return settings;
    }

    /// <summary>Reads a setting that's two whole numbers, like size = 40 30.</summary>
    /// <param name="id">The map, for the error message.</param>
    /// <param name="setting">The setting and its line.</param>
    /// <param name="key">What it's called, for the error message.</param>
    /// <param name="meaning">What the two numbers are, for the error message.</param>
    private static (int A, int B) ReadPair(string id, (int Line, string Value) setting, string key, string meaning)
    {
        string[] parts = setting.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int a)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int b))
        {
            throw Fail(id, setting.Line, $"{key} has to be {meaning}, not {setting.Value}");
        }

        return (a, b);
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

    /// <summary>Splits up every line of [objects], checking each is a kind there is, on the map, and the only one with its id.</summary>
    /// <remarks>
    /// Spawns have their own names and props are named after what they are, so those two don't count.
    /// Everything else is kept track of by its id, in the save, so two with the same one would share
    /// whether they'd been opened or used.
    /// </remarks>
    private static List<MapObjectLine> ReadObjects(string id, List<(int Line, string Text)> lines, int width, int height)
    {
        var objects = new List<MapObjectLine>();
        var ids = new HashSet<string>();

        foreach ((int line, string text) in lines)
        {
            var o = new MapObjectLine(id, line, text);

            if (Array.IndexOf(Kinds, o.Kind) < 0)
                throw o.Fail($"there's no kind of object called {o.Kind} (the kinds are {string.Join(", ", Kinds)})");
            if (o.X < 0f || o.Y < 0f || o.X >= width || o.Y >= height)
                throw o.Fail($"{o.X} {o.Y} is off the map, which is {width} by {height}");
            if (o.Kind is not ("spawn" or "prop") && !ids.Add(o.Id))
                throw o.Fail($"there are two objects called {o.Id}");

            objects.Add(o);
        }

        return objects;
    }

    /// <summary>The spawns, by name: spawn name x y, and which way to face on arriving (facing=, down if it isn't set).</summary>
    private static Dictionary<string, MapSpawn> ReadSpawns(string id, List<MapObjectLine> objects)
    {
        var spawns = new Dictionary<string, MapSpawn>();

        foreach (MapObjectLine o in Of(objects, "spawn"))
        {
            var spawn = new MapSpawn(FeetOn(o), o.Facing("facing", Direction.Down));
            o.Done("facing");
            if (!spawns.TryAdd(o.Id, spawn)) throw o.Fail($"there are two spawns called {o.Id}");
        }

        if (!spawns.ContainsKey(StartSpawn))
            throw Fail(id, $"there's no spawn called {StartSpawn}, and every map needs one");

        return spawns;
    }

    /// <summary>The props: prop which x y.</summary>
    private static List<MapProp> ReadProps(List<MapObjectLine> objects)
    {
        var props = new List<MapProp>();

        foreach (MapObjectLine o in Of(objects, "prop"))
        {
            if (!PropCatalog.TryGet(o.Id, out PropDefinition prop)) throw o.Fail($"there's no prop called {o.Id}");
            MapPresence presence = o.Presence();
            o.Done("when, unless");
            props.Add(new MapProp(prop, FeetOn(o), presence));
        }

        return props;
    }

    /// <summary>The people: npc id x y, then sheet= and name= for townsfolk or cast= for the story's people, and facing=, say=, wander=, when= and unless=.</summary>
    /// <param name="objects">Every object on the map.</param>
    /// <param name="solid">Whether a tile is solid, for checking they're standing somewhere they could.</param>
    private static List<MapNpc> ReadNpcs(List<MapObjectLine> objects, Func<int, int, bool> solid)
    {
        var npcs = new List<MapNpc>();

        foreach (MapObjectLine o in Of(objects, "npc"))
        {
            string castId = o.Text("cast");
            string sheet = o.Text("sheet");
            string name = o.Text("name");
            if (castId is not null)
            {
                CastMember cast = Cast.Get(castId) ?? throw o.Fail($"there's nobody in the cast called {castId}");
                if (cast.Look is null && cast.Sheet is null) throw o.Fail($"{castId} is only ever a voice, and can't stand on a map");
                name ??= cast.Name;
            }
            else
            {
                if (sheet is null || name is null) throw o.Fail("someone who isn't in the cast needs sheet= and name=");
                if (!Townsfolk.Contains(sheet))
                    throw o.Fail($"there's no sheet called {sheet} (the townsfolk are {string.Join(", ", Townsfolk.All)})");
            }

            float wander = o.Number("wander", 0f);
            if (wander < 0f) throw o.Fail("wander can't be less than nothing");

            var npc = new MapNpc(o.Id, name, FeetOn(o), o.Facing("facing", Direction.Down), castId is null ? sheet : null, castId,
                o.Text("say"), wander * Tilemap.TileSize, o.Presence());
            o.Done("name, sheet, cast, facing, say, wander, when, unless");

            Point cell = Tilemap.CellOf(npc.Feet);
            if (solid(cell.X, cell.Y)) throw o.Fail($"{o.Id} is standing on a solid tile");
            npcs.Add(npc);
        }

        return npcs;
    }

    /// <summary>The lights that aren't a prop's: light id x y reach=tiles, and strength= (1 if it isn't set).</summary>
    private static List<MapLight> ReadLights(List<MapObjectLine> objects)
    {
        var lights = new List<MapLight>();

        foreach (MapObjectLine o in Of(objects, "light"))
        {
            float reach = o.Number("reach", 0f);
            if (reach <= 0f) throw o.Fail("a light has to set reach, how far it gets in tiles");
            float strength = o.Number("strength", 1f);
            if (strength <= 0f || strength > 1f) throw o.Fail($"strength has to be more than 0 and no more than 1, not {strength}");
            o.Done("reach, strength");

            lights.Add(new MapLight(FeetOn(o), reach * Tilemap.TileSize, strength));
        }

        return lights;
    }

    /// <summary>Reads every object of a kind that's a rectangle: x y is its top left tile, and w= and h= are its size in tiles (1 if they aren't set).</summary>
    /// <param name="objects">Every object on the map.</param>
    /// <param name="kind">Exits or triggers.</param>
    /// <param name="width">The map's width, which the rectangle has to fit inside.</param>
    /// <param name="height">The map's height.</param>
    /// <param name="read">Reads the rest of one, given its rectangle in world pixels.</param>
    private static List<T> ReadAreas<T>(List<MapObjectLine> objects, string kind, int width, int height,
        Func<MapObjectLine, BoundingRectangle, T> read)
    {
        var areas = new List<T>();

        foreach (MapObjectLine o in Of(objects, kind))
        {
            if (o.X != MathF.Floor(o.X) || o.Y != MathF.Floor(o.Y))
                throw o.Fail($"x and y are the top left tile of the {kind}, so they're whole numbers");

            int w = o.Count("w", 1), h = o.Count("h", 1);
            if (o.X + w > width || o.Y + h > height) throw o.Fail($"it's {w} by {h}, which goes off the edge of the map");

            const int tile = Tilemap.TileSize;
            areas.Add(read(o, new BoundingRectangle(o.X * tile, o.Y * tile, w * tile, h * tile)));
        }

        return areas;
    }

    /// <summary>The rest of an exit: to= the map it goes to, and at= the spawn there (start if it isn't set).</summary>
    /// <remarks>Whether that map and that spawn exist gets checked once every map is read, in MapCatalog.</remarks>
    private static MapExit ReadExit(MapObjectLine o, BoundingRectangle area)
    {
        var exit = new MapExit(o.Id, area, o.Required("to"), o.Text("at") ?? StartSpawn, o.Presence());
        o.Done("w, h, to, at, when, unless");
        if (exit.ToMap == MapExit.Back && o.Text("at") is not null) throw o.Fail("an exit back out of a building goes back where you came in; it can't say at=");
        return exit;
    }

    /// <summary>The ways into buildings: entrance id x y to= at= name=, where x y is the middle of the bottom of the door, in tiles. Walking into the door goes through it.</summary>
    private static List<MapExit> ReadEntrances(List<MapObjectLine> objects)
    {
        var entrances = new List<MapExit>();

        foreach (MapObjectLine o in Of(objects, "entrance"))
        {
            var door = new Vector2(o.X * Tilemap.TileSize, o.Y * Tilemap.TileSize);
            var area = new BoundingRectangle(door.X - DoorwayWidth / 2f, door.Y - 1f, DoorwayWidth, DoorwayDepth);
            entrances.Add(new MapExit(o.Id, area, o.Required("to"), o.Text("at") ?? StartSpawn, o.Presence(), o.Required("name")));
            o.Done("to, at, name, when, unless");
        }

        return entrances;
    }

    /// <summary>The rest of a trigger: once= (yes if it isn't set), say= a line to show, set= a flag to set, talk= a conversation to start. It has to do at least one of them.</summary>
    private static MapTrigger ReadTrigger(MapObjectLine o, BoundingRectangle area)
    {
        var trigger = new MapTrigger(o.Id, area, o.YesNo("once", true), o.Text("say"), o.Text("set"), o.Text("talk"), o.Presence());
        o.Done("w, h, once, say, set, talk, when, unless");

        if (trigger.Say is null && trigger.SetFlag is null && trigger.Talk is null)
            throw o.Fail("this trigger doesn't do anything (it needs say=, set= or talk=)");
        if (trigger.Talk is not null && DialogueCatalog.Get(trigger.Talk) is null)
            throw o.Fail($"there's no conversation called {trigger.Talk}");
        return trigger;
    }

    /// <summary>The chests, doors, levers, signs and waypoints.</summary>
    /// <remarks>
    /// Each kind looks like the prop with its name (a chest looks like the chest prop) unless look=
    /// says otherwise, and is called by its kind on screen unless name= says otherwise.
    /// </remarks>
    /// <param name="objects">Every object on the map.</param>
    /// <param name="solid">Whether a tile is solid, for checking there's room beside a waypoint.</param>
    private static List<MapInteractable> ReadInteractables(List<MapObjectLine> objects, Func<int, int, bool> solid)
    {
        var interactables = new List<MapInteractable>();
        MapInteractable waypoint = null;

        foreach (MapObjectLine o in objects)
        {
            if (!Enum.TryParse(o.Kind, ignoreCase: true, out InteractableKind kind)) continue;

            string lookId = o.Text("look") ?? o.Kind;
            if (!PropCatalog.TryGet(lookId, out PropDefinition look)) throw o.Fail($"there's no prop called {lookId} for it to look like");

            int pictures = kind == InteractableKind.Sign ? 1 : 2;
            if (look.Frames != pictures)
                throw o.Fail($"the {lookId} prop has {look.Frames} {(look.Frames == 1 ? "picture" : "pictures")}, and a {o.Kind} needs {pictures}");

            var thing = new MapInteractable
            {
                Kind = kind,
                Id = o.Id,
                Foot = FeetOn(o),
                Look = look,
                Presence = o.Presence(),
                Name = o.Text("name") ?? kind.ToString(),
                Gold = kind == InteractableKind.Chest ? o.Count("gold", 0) : 0,
                Locked = kind == InteractableKind.Door && o.YesNo("locked", false),
                Flag = kind is InteractableKind.Door or InteractableKind.Lever ? o.Text("flag") : null,
                Text = kind == InteractableKind.Sign ? o.Required("text") : null,
            };

            o.Done(kind switch
            {
                InteractableKind.Chest => "look, name, gold, when, unless",
                InteractableKind.Door => "look, name, locked, flag, when, unless",
                InteractableKind.Lever => "look, name, flag, when, unless",
                InteractableKind.Sign => "look, name, text, when, unless",
                _ => "look, name, when, unless",
            });

            if (kind == InteractableKind.Lever && thing.Flag is null) throw o.Fail("a lever has to set flag, the flag it pulls");
            if (thing.Locked && thing.Flag is not null) throw o.Fail("a door can be locked or worked by a flag, not both");

            if (kind == InteractableKind.Waypoint)
            {
                // The world map picks a waypoint by its map, so there can only be one on each.
                if (waypoint is not null) throw o.Fail($"there's already a waypoint on this map, {waypoint.Id}");
                waypoint = thing;

                Point beside = Tilemap.CellOf(thing.Beside);
                if (solid(beside.X, beside.Y))
                    throw o.Fail("the tile to its right is solid, and that's where fast travel puts you");
            }

            interactables.Add(thing);
        }

        return interactables;
    }

    /// <summary>Every object of one kind.</summary>
    private static IEnumerable<MapObjectLine> Of(List<MapObjectLine> objects, string kind)
    {
        foreach (MapObjectLine o in objects)
            if (o.Kind == kind) yield return o;
    }

    /// <summary>Where something at x, y in tiles stands: with its feet on the middle of that tile.</summary>
    /// <remarks>Halves are fine, for anything that sits between two tiles.</remarks>
    private static Vector2 FeetOn(MapObjectLine o) =>
        new((o.X + 0.5f) * Tilemap.TileSize, (o.Y + 0.5f) * Tilemap.TileSize);

    /// <summary>Whether a point is inside a rectangle.</summary>
    private static bool Inside(Vector2 point, BoundingRectangle area) =>
        point.X >= area.Left && point.X < area.Right && point.Y >= area.Top && point.Y < area.Bottom;

    /// <summary>An error about one line of a map file.</summary>
    /// <param name="id">The map.</param>
    /// <param name="line">The line, counting from 1.</param>
    /// <param name="message">What's wrong.</param>
    internal static InvalidOperationException Fail(string id, int line, string message) =>
        new($"Maps/{id}.map line {line}: {message}.");

    /// <summary>An error about a map file as a whole.</summary>
    private static InvalidOperationException Fail(string id, string message) =>
        new($"Maps/{id}.map: {message}.");
}
