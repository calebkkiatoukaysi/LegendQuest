using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Items;
using LegendQuest.Story;

namespace LegendQuest.World;

/// <summary>
/// Reads a zone file out of Content/Zones and turns it into a ZoneDefinition, checking everything on the way.
/// </summary>
/// <remarks>
/// The tile maps' loader grown into side-on rooms (the maps are tools/rooms/legacy now, the
/// converter's input). The format's in tools/rooms/zonefile.py, which the room tools read it with
/// and the converter writes it with, and in 12-rooms §4: a [zone] header, then a [room id] section
/// a room, each its settings and then its objects. The file's in canvas pixels, and everything it
/// says is divided by ZoneCatalog.RoomScale once, here, so the rooms come out in world pixels.
/// Every line gets trimmed, and anything wrong throws with the file and the line, as the maps'
/// loader did, and ZoneCatalog reads every zone at startup, so a mistake shows up right away.
/// </remarks>
public static class ZoneLoader
{
    /// <summary>Where Content.mgcb copies the zone files, relative to the game.</summary>
    private const string Folder = "Content/Zones/";

    /// <summary>The spawn every zone needs, where the party arrives when nothing says otherwise.</summary>
    public const string StartSpawn = "start";

    /// <summary>How tall every room is, in canvas pixels.</summary>
    private const int CanvasHeight = 900;

    /// <summary>One tile of the strip, in canvas pixels: 16 art pixels at 4x.</summary>
    private const int CanvasTile = 64;

    /// <summary>The narrowest and widest a room can be, in canvas pixels: one screen to three.</summary>
    private const int NarrowestRoom = 1600, WidestRoom = 4800;

    /// <summary>How wide a fight stands when the room doesn't say, in canvas pixels: the middle screen.</summary>
    private const float DefaultFight = 1600f;

    /// <summary>A tile strip's empty cell, which only detail rows may have.</summary>
    private const char Empty = '.';

    /// <summary>Starts a comment, outside double quotes.</summary>
    private const char Comment = ';';

    private static readonly string[] ZoneSettings = { "id", "name", "world", "music", "region", "danger" };
    private static readonly string[] RequiredZoneSettings = { "id", "name", "music" };
    private static readonly string[] RoomSettings =
        { "name", "kind", "width", "paint", "dark", "ambient", "tint", "floor", "block", "fight", "tiles", "tiletop", "legend", "row", "detail", "legacy" };
    private static readonly string[] RepeatedSettings = { "block", "legend", "row", "detail", "legacy" };
    private static readonly string[] Kinds =
        { "scenery", "life", "exit", "door", "spawn", "npc", "prop", "chest", "sign", "lever", "waypoint", "trigger", "scene", "camp", "light", "when" };
    private static readonly string[] SceneryLayers = { "far", "mid", "fore" };
    private static readonly string[] LifeKinds = { "gulls", "clouds", "smoke", "flag", "shimmer", "leaves", "motes", "ash", "candle" };

    /// <summary>Reads and checks one zone.</summary>
    /// <param name="id">The zone, which is also its file name without the .txt.</param>
    /// <returns>The zone. An exit or door into another zone doesn't know its room yet: ZoneCatalog fills that in.</returns>
    /// <exception cref="InvalidOperationException">If the file is wrong in any way. The message says where.</exception>
    public static ZoneDefinition Load(string id)
    {
        string text;
        using (Stream stream = TitleContainer.OpenStream(Folder + id + ".txt"))
        using (var reader = new StreamReader(stream))
            text = reader.ReadToEnd();

        return new ZoneReader(id).Read(text);
    }

    /// <summary>The name a badge has in a zone file: its own in lower case, with a hyphen between words (StairUp is stair-up).</summary>
    /// <param name="badge">The badge.</param>
    private static string BadgeName(BadgeKind badge)
    {
        string name = badge.ToString();
        var kebab = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i])) kebab.Append('-');
            kebab.Append(char.ToLowerInvariant(name[i]));
        }
        return kebab.ToString();
    }

    /// <summary>An error about one line of a zone file.</summary>
    /// <param name="file">The file, as Zones/docks.txt.</param>
    /// <param name="line">The line, counting from 1.</param>
    /// <param name="message">What's wrong.</param>
    internal static InvalidOperationException Fail(string file, int line, string message) => new($"{file} line {line}: {message}.");

    /// <summary>An error about a zone file as a whole, or a room in it.</summary>
    internal static InvalidOperationException Fail(string where, string message) => new($"{where}: {message}.");

    /// <summary>Where an exit or a door goes, as the file says it: a place and a spawn, back, or locked.</summary>
    private sealed record Target(string Place, string Spawn, bool Back, bool Locked);

    /// <summary>An exit read off its line, before the zone's rooms are all known.</summary>
    private sealed record ExitLine(int Line, string Id, ExitSide Side, float X, Target To, string Label, string Why, bool Travel, MapPresence Presence);

    /// <summary>A door read off its line, before the room's floor and the zone's rooms are known.</summary>
    private sealed record DoorLine(int Line, string Id, float X, Target To, BadgeKind Badge, string Name, string Why, Vector2 Size, MapPresence Presence);

    /// <summary>One zone file being read, line by line.</summary>
    private sealed class ZoneReader
    {
        private readonly string _id;
        private readonly string _file;
        private readonly Dictionary<string, (int Line, string Value)> _settings = new();
        private readonly List<RoomReader> _rooms = new();
        private readonly Dictionary<string, int> _ids = new();
        private readonly Dictionary<string, int> _spawns = new();
        private RoomReader _room;
        private bool _inZone;
        private int _waypointLine;

        public ZoneReader(string id)
        {
            _id = id;
            _file = $"Zones/{id}.txt";
        }

        /// <summary>Reads the whole file and builds the zone.</summary>
        public ZoneDefinition Read(string text)
        {
            if (text.Length > 0 && text[0] == '﻿') text = text[1..];
            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i];
                int comment = ZoneLine.OutsideQuotes(raw, Comment);
                string line = (comment < 0 ? raw : raw[..comment]).Trim();
                if (line.Length == 0) continue;

                if (line[0] == '[') Section(i + 1, line);
                else if (_room is not null) _room.Line(new ZoneLine(_file, i + 1, line));
                else if (_inZone) Setting(new ZoneLine(_file, i + 1, line));
                else throw Fail(_file, i + 1, "this line comes before any section");
            }

            if (!_inZone && _rooms.Count == 0) throw Fail(_file, "there's no [zone] section");
            _room?.Finish();
            if (_rooms.Count == 0) throw Fail(_file, "a zone needs at least one room");
            if (!_spawns.ContainsKey(StartSpawn)) throw Fail(_file, $"there's no spawn called {StartSpawn}, and every zone needs one");

            return Build();
        }

        /// <summary>A [zone] or [room id] line.</summary>
        private void Section(int line, string text)
        {
            if (text[^1] != ']') throw Fail(_file, line, "a section name has no closing ]");
            string[] words = text[1..^1].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            if (words.Length == 1 && words[0].ToLowerInvariant() == "zone")
            {
                if (_inZone || _rooms.Count > 0) throw Fail(_file, line, "[zone] has to come first, and once");
                _inZone = true;
                return;
            }
            if (words.Length != 2 || words[0].ToLowerInvariant() != "room")
                throw Fail(_file, line, $"there's no section called [{text[1..^1].Trim()}] (the sections are [zone] and [room <id>])");
            if (!_inZone) throw Fail(_file, line, "[zone] has to come before the rooms");

            if (_room is null) CheckZoneSettings();
            else _room.Finish();
            if (_rooms.Exists(room => room.Id == words[1])) throw Fail(_file, line, $"there are two rooms called {words[1]}");

            _room = new RoomReader(this, words[1]);
            _rooms.Add(_room);
        }

        /// <summary>A key = value line in [zone].</summary>
        private void Setting(ZoneLine line)
        {
            if (line.Body is null || line.Head.Count != 1) throw line.Fail("a [zone] line is key = value");
            if (Array.IndexOf(ZoneSettings, line.Key) < 0)
                throw line.Fail($"[zone] has no setting called {line.Key} (they're {string.Join(", ", ZoneSettings)})");
            if (line.Body.Length == 0) throw line.Fail($"{line.Key} is set to nothing");
            if (!_settings.TryAdd(line.Key, (line.Line, line.Body))) throw line.Fail($"{line.Key} is set twice");

            switch (line.Key)
            {
                case "id":
                    if (line.Body != _id) throw line.Fail($"the file says it's {line.Body}, but it's called {_id}.txt");
                    break;
                case "world":
                    float[] world = line.ReadNumbers(line.Body, "world", 2);
                    if (world[0] < 0 || world[1] < 0 || world[0] > ZoneCatalog.WorldWidth || world[1] > ZoneCatalog.WorldHeight
                        || world[0] != MathF.Floor(world[0]) || world[1] != MathF.Floor(world[1]))
                        throw line.Fail($"world has to be two whole numbers inside the world map, which is {ZoneCatalog.WorldWidth} by {ZoneCatalog.WorldHeight}");
                    break;
                case "music":
                    if (!Enum.TryParse(line.Body, out Track music) || !Enum.IsDefined(music))
                        throw line.Fail($"there's no song called {line.Body} (the Track names are {string.Join(", ", Enum.GetNames<Track>())})");
                    break;
                case "region":
                    line.ReadWord(line.Body, "region");
                    break;
                case "danger":
                    line.ReadWhole(line.Body, "danger");
                    break;
            }
        }

        /// <summary>Checks [zone] set what every zone needs, before the first room.</summary>
        private void CheckZoneSettings()
        {
            foreach (string key in RequiredZoneSettings)
                if (!_settings.ContainsKey(key)) throw Fail(_file, $"[zone] doesn't set {key}");
        }

        /// <summary>Notes an id, which has to be unique in the zone. Spawns have their own names, kept apart.</summary>
        public void Claim(ZoneLine line, string id)
        {
            line.ReadWord(id, "an id");
            if (!_ids.TryAdd(id, line.Line)) throw line.Fail($"there's already something called {id} in this zone (line {_ids[id]})");
        }

        /// <summary>Notes a spawn's name, which has to be unique in the zone.</summary>
        public void ClaimSpawn(ZoneLine line, string name)
        {
            line.ReadWord(name, "a spawn's name");
            if (!_spawns.TryAdd(name, line.Line)) throw line.Fail($"there's already a spawn called {name} in this zone (line {_spawns[name]})");
        }

        /// <summary>Notes the zone's waypoint. There can be one, since the world map picks a waypoint by its zone.</summary>
        public void ClaimWaypoint(ZoneLine line)
        {
            if (_waypointLine != 0) throw line.Fail($"there's already a waypoint in this zone (line {_waypointLine})");
            _waypointLine = line.Line;
        }

        /// <summary>The zone's file, as the errors call it.</summary>
        public string File => _file;

        /// <summary>The zone's id, for its rooms' default paint.</summary>
        public string Id => _id;

        /// <summary>Puts the zone together: its rooms, with their exits and doors pointed at a room of theirs or another zone.</summary>
        private ZoneDefinition Build()
        {
            var rooms = new List<RoomDefinition>();
            var legacy = new List<(Rectangle OldTiles, string Room)>();
            foreach (RoomReader room in _rooms)
            {
                rooms.Add(room.Definition);
                foreach (Rectangle old in room.Legacy) legacy.Add((old, room.Id));
            }

            foreach (RoomReader room in _rooms)
            {
                var exits = new List<RoomExit>();
                foreach (ExitLine exit in room.ExitLines)
                {
                    (string zone, string toRoom, string spawn) = Resolve(exit.Line, exit.To);
                    exits.Add(new RoomExit(exit.Id, exit.Side, exit.X, zone, toRoom, spawn, exit.Label, exit.Why, exit.Travel, exit.Presence));
                }
                var doors = new List<RoomDoor>();
                foreach (DoorLine door in room.DoorLines)
                {
                    (string zone, string toRoom, string spawn) = Resolve(door.Line, door.To);
                    float bottom = room.Definition.BackEdgeAt(door.X);
                    var area = new BoundingRectangle(door.X - door.Size.X / 2f, bottom - door.Size.Y, door.Size.X, door.Size.Y);
                    doors.Add(new RoomDoor(door.Id, area, door.Badge, door.Name, zone, toRoom, spawn, door.Why, door.Presence));
                }
                room.Definition.Exits = exits;
                room.Definition.Doors = doors;
            }

            if (_waypointLine != 0 && !_settings.ContainsKey("world"))
                throw Fail(_file, _waypointLine, "there's a waypoint in a zone that isn't on the world map (it needs world =)");

            Point? world = null;
            if (_settings.TryGetValue("world", out (int Line, string Value) at))
            {
                string[] xy = at.Value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                world = new Point(int.Parse(xy[0], CultureInfo.InvariantCulture), int.Parse(xy[1], CultureInfo.InvariantCulture));
            }

            return new ZoneDefinition
            {
                Id = _id,
                Name = _settings["name"].Value,
                World = world,
                Music = Enum.Parse<Track>(_settings["music"].Value),
                Region = _settings.TryGetValue("region", out (int Line, string Value) region) ? region.Value : null,
                Danger = _settings.TryGetValue("danger", out (int Line, string Value) danger) ? int.Parse(danger.Value, CultureInfo.InvariantCulture) : 0,
                Rooms = rooms,
                Legacy = legacy,
            };
        }

        /// <summary>Where a way out goes: a room of this zone (looked for first), another zone (whose room ZoneCatalog finds), back, or nowhere.</summary>
        private (string Zone, string Room, string Spawn) Resolve(int line, Target to)
        {
            if (to.Locked) return (null, null, null);
            if (to.Back) return (RoomExit.Back, null, null);

            RoomReader room = _rooms.Find(r => r.Id == to.Place);
            if (room is null) return (to.Place, null, to.Spawn);
            if (!room.Definition.Spawns.ContainsKey(to.Spawn))
                throw Fail(_file, line, $"room {to.Place} has no spawn called {to.Spawn}");
            return (_id, to.Place, to.Spawn);
        }
    }

    /// <summary>One room being read: its settings first, then its objects.</summary>
    private sealed class RoomReader
    {
        private readonly ZoneReader _zone;
        private readonly HashSet<string> _set = new();
        private readonly Dictionary<string, int> _settingLines = new();
        private bool _objects;

        private string _name;
        private RoomKind _kind = RoomKind.Street;
        private int _width;
        private string _paint;
        private bool _dark;
        private float? _ambient;
        private Color _tint = Color.White;
        private List<Vector2> _floor;
        private readonly List<RoomBlock> _blocks = new();
        private (float Left, float Right)? _fight;
        private (int Line, string Id) _tileset;
        private int? _tileTop;
        private readonly Dictionary<char, (int Line, string Name)> _legend = new();
        private readonly List<(int Line, string Row)> _rows = new();
        private readonly List<(int Line, string Row)> _details = new();
        private readonly Dictionary<string, string> _states = new();

        private readonly List<RoomScenery> _scenery = new();
        private readonly List<RoomLife> _life = new();
        private readonly Dictionary<string, MapSpawn> _spawns = new();
        private readonly List<(int Line, MapNpc Npc)> _npcs = new();
        private readonly List<MapProp> _props = new();
        private readonly List<MapInteractable> _things = new();
        private readonly List<MapTrigger> _triggers = new();
        private readonly List<MapScene> _scenes = new();
        private readonly List<MapCamp> _camp = new();
        private readonly List<MapLight> _lights = new();

        public RoomReader(ZoneReader zone, string id)
        {
            _zone = zone;
            Id = id;
        }

        public string Id { get; }
        public List<ExitLine> ExitLines { get; } = new();
        public List<DoorLine> DoorLines { get; } = new();
        public List<Rectangle> Legacy { get; } = new();
        public RoomDefinition Definition { get; private set; }

        private string Where => $"{_zone.File}: room {Id}";

        /// <summary>A line in the room's section: a setting or an object.</summary>
        public void Line(ZoneLine line)
        {
            if (Array.IndexOf(RoomSettings, line.Key) >= 0)
            {
                if (line.Body is null || line.Head.Count != 1) throw line.Fail($"{line.Key} is a room setting: {line.Key} = value");
                Setting(line);
            }
            else if (Array.IndexOf(Kinds, line.Key) >= 0) Object(line);
            else throw line.Fail($"a room has no setting or object called {line.Key}");
        }

        /// <summary>The room's width, which a position needs to be checked against, so it has to come first.</summary>
        private int NeedWidth(ZoneLine line, string what) => _width != 0 ? _width : throw line.Fail($"width has to come before {what}");

        private void CheckX(ZoneLine line, float x, string what = "x")
        {
            int width = NeedWidth(line, "anything with a position");
            if (x < 0 || x > width) throw line.Fail($"{what} {x.ToString(CultureInfo.InvariantCulture)} is outside the room, which is {width} wide");
        }

        private void CheckXY(ZoneLine line, float x, float y)
        {
            CheckX(line, x);
            if (y < 0 || y > CanvasHeight) throw line.Fail($"y {y.ToString(CultureInfo.InvariantCulture)} is outside the room, which is {CanvasHeight} tall");
        }

        /// <summary>Canvas pixels to world pixels.</summary>
        private static float W(float canvas) => canvas / ZoneCatalog.RoomScale;

        private static Vector2 W(float x, float y) => new(x / ZoneCatalog.RoomScale, y / ZoneCatalog.RoomScale);

        private Direction Facing(ZoneLine line, float x)
        {
            string text = line.Text("facing");
            if (text is null) return x < _width / 2f ? Direction.Right : Direction.Left;
            return text.ToLowerInvariant() switch
            {
                "left" => Direction.Left,
                "right" => Direction.Right,
                _ => throw line.Fail($"facing has to be left or right, not {text}"),
            };
        }

        /// <summary>One of the room's settings, which all come before its objects.</summary>
        private void Setting(ZoneLine line)
        {
            string key = line.Key, value = line.Body;
            if (_objects) throw line.Fail($"{key} is a room setting, and those come before the room's objects");
            if (Array.IndexOf(RepeatedSettings, key) < 0 && !_set.Add(key)) throw line.Fail($"{key} is set twice");
            if (value.Length == 0) throw line.Fail($"{key} is set to nothing");
            _settingLines[key] = line.Line;

            switch (key)
            {
                case "name":
                    _name = value;
                    break;
                case "kind":
                    _kind = value switch
                    {
                        "street" => RoomKind.Street,
                        "interior" => RoomKind.Interior,
                        _ => throw line.Fail($"kind has to be street or interior, not {value}"),
                    };
                    break;
                case "width":
                    int width = line.ReadWhole(value, "width");
                    if (width % CanvasTile != 0 || width < NarrowestRoom || width > WidestRoom)
                        throw line.Fail($"width has to be a multiple of {CanvasTile} from {NarrowestRoom} to {WidestRoom}, not {value}");
                    _width = width;
                    break;
                case "paint":
                    _paint = line.ReadWord(value, "paint");
                    break;
                case "dark":
                    _dark = line.ReadYesNo(value, "dark");
                    break;
                case "ambient":
                    float ambient = line.ReadNumber(value, "ambient");
                    if (ambient < 0f || ambient > 1f) throw line.Fail($"ambient has to be from 0 to 1, not {value}");
                    _ambient = ambient;
                    break;
                case "tint":
                    string[] parts = value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 3) throw line.Fail($"tint has to be three numbers from 0 to 255, red green blue, not {value}");
                    var rgb = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        rgb[i] = line.ReadWhole(parts[i], "each of tint's numbers");
                        if (rgb[i] > byte.MaxValue) throw line.Fail($"each of tint's numbers has to be from 0 to {byte.MaxValue}, not {parts[i]}");
                    }
                    _tint = new Color(rgb[0], rgb[1], rgb[2]);
                    break;
                case "floor":
                    _floor = ReadFloor(line, value);
                    break;
                case "block":
                    float[] block = line.ReadNumbers(value, "block (centre x y, radii x y)", 4);
                    CheckXY(line, block[0], block[1]);
                    if (block[2] <= 0f || block[3] <= 0f) throw line.Fail("a block's radii have to be more than 0");
                    _blocks.Add(new RoomBlock(W(block[0], block[1]), W(block[2], block[3])));
                    break;
                case "fight":
                    float[] fight = line.ReadNumbers(value, "fight (left right)", 2);
                    CheckX(line, fight[0], "fight's left");
                    CheckX(line, fight[1], "fight's right");
                    if (fight[1] <= fight[0]) throw line.Fail("fight's right has to be more than its left");
                    _fight = (fight[0], fight[1]);
                    break;
                case "tiles":
                    _tileset = (line.Line, line.ReadWord(value, "tiles"));
                    if (!TilesetCatalog.TryGet(value, out _)) throw line.Fail($"there's no tileset called {value}");
                    break;
                case "tiletop":
                    int top = line.ReadWhole(value, "tiletop");
                    if (top % CanvasTile != 0 || top >= CanvasHeight) throw line.Fail($"tiletop has to be a multiple of {CanvasTile} above {CanvasHeight}, not {value}");
                    _tileTop = top;
                    break;
                case "legend":
                    if (value.Length < 3 || value[1] != ' ') throw line.Fail("a legend line is one character, a space and a tile's name, like legend = = planks");
                    char c = value[0];
                    if (c is Empty or Comment or ZoneLine.Separator or '"' or ' ') throw line.Fail($"'{c}' can't stand for a tile ('{Empty}' is an empty cell)");
                    string tile = line.ReadWord(value[2..].Trim(), "a tile's name");
                    if (!_legend.TryAdd(c, (line.Line, tile))) throw line.Fail($"'{c}' is in the legend twice");
                    break;
                case "row":
                case "detail":
                    ReadRow(line, key, value);
                    break;
                case "legacy":
                    string[] corners = value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (corners.Length != 4) throw line.Fail($"legacy has to be x0 y0 x1 y1 in the old map's tiles, not {value}");
                    int x0 = line.ReadWhole(corners[0], "legacy"), y0 = line.ReadWhole(corners[1], "legacy");
                    int x1 = line.ReadWhole(corners[2], "legacy"), y1 = line.ReadWhole(corners[3], "legacy");
                    if (x1 <= x0 || y1 <= y0) throw line.Fail("legacy's x1 and y1 have to be more than its x0 and y0");
                    Legacy.Add(new Rectangle(x0, y0, x1 - x0, y1 - y0));
                    break;
            }
        }

        /// <summary>The floor's points, x y pairs between commas, checked to be a simple polygon in the room.</summary>
        private List<Vector2> ReadFloor(ZoneLine line, string value)
        {
            int width = NeedWidth(line, "the floor");
            var points = new List<Vector2>();
            string[] pairs = value.Split(',');
            for (int i = 0; i < pairs.Length; i++)
            {
                float[] xy = line.ReadNumbers(pairs[i].Trim(), $"floor point {i + 1}", 2);
                if (xy[0] < 0 || xy[0] > width || xy[1] < 0 || xy[1] > CanvasHeight)
                    throw line.Fail($"floor point {i + 1} ({pairs[i].Trim()}) is outside the room");
                points.Add(new Vector2(xy[0], xy[1]));
            }
            if (Polygon.Problem(points) is string problem) throw line.Fail(problem);

            for (int i = 0; i < points.Count; i++) points[i] = W(points[i].X, points[i].Y);
            return points;
        }

        /// <summary>A row of the tile strip, one character a tile, every one of them in the legend.</summary>
        private void ReadRow(ZoneLine line, string key, string value)
        {
            int cells = NeedWidth(line, $"a {key}") / CanvasTile;
            if (value.Length != cells) throw line.Fail($"a {key} is {cells} tiles in a room {_width} wide, and this one is {value.Length}");
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == Empty)
                {
                    if (key == "row") throw line.Fail($"row has an empty cell at column {i + 1}, and every ground cell needs a tile");
                    continue;
                }
                if (!_legend.ContainsKey(value[i])) throw line.Fail($"'{value[i]}' at column {i + 1} isn't in the room's legend (legend lines come first)");
            }
            (key == "row" ? _rows : _details).Add((line.Line, value));
        }

        /// <summary>One of the room's objects.</summary>
        private void Object(ZoneLine line)
        {
            _objects = true;
            string kind = line.Key;

            if (kind == "scenery")
            {
                if (line.Body is not null || line.Head.Count != 4) throw line.Fail("a scenery line is scenery <far|mid|fore> <prop> <x>, with no =");
                string layer = line.Head[1].ToLowerInvariant();
                if (Array.IndexOf(SceneryLayers, layer) < 0) throw line.Fail($"scenery's layer has to be far, mid or fore, not {line.Head[1]}");
                if (!PropCatalog.TryGet(line.Head[2], out _)) throw line.Fail($"there's no prop called {line.Head[2]}");
                float x = line.ReadNumber(line.Head[3], "scenery's x");
                CheckX(line, x);
                _scenery.Add(new RoomScenery(layer, line.Head[2], W(x)));
                return;
            }

            if (line.Body is null) throw line.Fail($"a {kind} line needs an = and what comes after it");
            string first = line.Segments[0];

            switch (kind)
            {
                case "when":
                    if (line.Head.Count != 2 || line.Segments.Count != 1) throw line.Fail("a room state is when <state> = <flag>");
                    if (!_states.TryAdd(line.ReadWord(line.Head[1], "a state"), line.ReadWord(first, "the state's flag")))
                        throw line.Fail($"the state {line.Head[1]} is set twice");
                    return;
                case "life":
                    ReadLife(line, first);
                    return;
                case "exit":
                    ReadExit(line, first);
                    return;
                case "door":
                    ReadDoor(line, first);
                    return;
            }

            // Everything else is <kind> <id> = ..., though a light's id can be left out.
            if (kind == "light" ? line.Head.Count > 2 : line.Head.Count != 2)
                throw line.Fail(kind == "light" ? "a light is light [id] = x y reach [strength]" : $"a {kind} line is {kind} <id> = ...");
            string id = line.Head.Count > 1 ? line.Head[1] : null;

            switch (kind)
            {
                case "scene":
                    line.TakeFields(0);
                    var scene = new MapScene(id, line.Required("talk", "scene"), line.Presence());
                    line.Done(kind, "talk, when, unless");
                    _zone.Claim(line, id);
                    _scenes.Add(scene);
                    return;
                case "trigger":
                    ReadTrigger(line, id, first);
                    return;
                case "light":
                    ReadLight(line, id, first);
                    return;
            }

            float[] at = line.ReadNumbers(first, $"a {kind}'s place (x y)", 2);
            CheckXY(line, at[0], at[1]);
            line.TakeFields(1);
            Vector2 feet = W(at[0], at[1]);

            switch (kind)
            {
                case "spawn":
                    var spawn = new MapSpawn(feet, Facing(line, at[0]));
                    line.Done(kind, "facing");
                    _zone.ClaimSpawn(line, id);
                    _spawns.Add(id, spawn);
                    break;
                case "camp":
                    var place = new MapCamp(id, feet, Facing(line, at[0]));
                    line.Done(kind, "facing");
                    _zone.Claim(line, id);
                    _camp.Add(place);
                    break;
                case "prop":
                    if (!PropCatalog.TryGet(id, out PropDefinition prop)) throw line.Fail($"there's no prop called {id}");
                    string propId = line.Text("id");
                    var placed = new MapProp(prop, feet, line.Presence(), propId);
                    line.Done(kind, "id, when, unless");
                    if (placed.Id is not null) _zone.Claim(line, placed.Id);
                    _props.Add(placed);
                    break;
                case "npc":
                    _npcs.Add((line.Line, ReadNpc(line, id, feet, at[0])));
                    break;
                default:
                    _things.Add(ReadThing(line, kind, id, feet));
                    break;
            }
        }

        private void ReadLife(ZoneLine line, string first)
        {
            if (line.Head.Count != 2 || line.Segments.Count != 1) throw line.Fail("a life line is life <kind> = x0 y0 x1 y1");
            string kind = line.Head[1].ToLowerInvariant();
            if (Array.IndexOf(LifeKinds, kind) < 0) throw line.Fail($"there's no life called {kind} (they're {string.Join(", ", LifeKinds)})");
            float[] box = line.ReadNumbers(first, "life's box (x0 y0 x1 y1)", 4);
            CheckXY(line, box[0], box[1]);
            CheckXY(line, box[2], box[3]);
            if (box[2] <= box[0] || box[3] <= box[1]) throw line.Fail("life's x1 and y1 have to be more than its x0 and y0");
            _life.Add(new RoomLife(kind, new BoundingRectangle(W(box[0]), W(box[1]), W(box[2] - box[0]), W(box[3] - box[1]))));
        }

        /// <summary>Where a way out goes: room spawn, zone spawn, back or locked.</summary>
        private static Target ReadTarget(ZoneLine line, string text)
        {
            string[] words = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 1 && words[0] == "back") return new Target(null, null, Back: true, Locked: false);
            if (words.Length == 1 && words[0] == "locked") return new Target(null, null, Back: false, Locked: true);
            if (words.Length != 2) throw line.Fail($"a way out goes to <room> <spawn>, <zone> <spawn>, back or locked, not {text}");
            return new Target(words[0], words[1], Back: false, Locked: false);
        }

        private void ReadExit(ZoneLine line, string first)
        {
            if (line.Head.Count is not (2 or 3)) throw line.Fail("an exit is exit west|east = ..., or exit up|down <x> = ...");
            ExitSide side = line.Head[1].ToLowerInvariant() switch
            {
                "west" => ExitSide.West,
                "east" => ExitSide.East,
                "up" => ExitSide.Up,
                "down" => ExitSide.Down,
                _ => throw line.Fail($"an exit's side has to be west, east, up or down, not {line.Head[1]}"),
            };

            float x;
            if (side is ExitSide.Up or ExitSide.Down)
            {
                if (line.Head.Count != 3) throw line.Fail($"an exit {line.Head[1]} needs an x, like exit {line.Head[1]} 1500 = ...");
                x = line.ReadNumber(line.Head[2], "the exit's x");
                CheckX(line, x);
            }
            else
            {
                if (line.Head.Count != 2) throw line.Fail($"an exit {line.Head[1]} is at the room's end, so it has no x");
                x = side == ExitSide.West ? 0f : NeedWidth(line, "an exit");
            }

            Target to = ReadTarget(line, first);
            line.TakeFields(1);
            string id = line.Text("id") ?? $"{Id}-{line.Head[1].ToLowerInvariant()}";
            var exit = new ExitLine(line.Line, id, side, W(x), to, line.Text("label"), line.Text("why"), line.Flag("travel"), line.Presence());
            line.Done("exit", "id, label, why, travel, when, unless");
            if (to.Locked && exit.Why is null) throw line.Fail("a locked exit has to say why, with why \"...\"");
            if (!to.Locked && exit.Why is not null) throw line.Fail("only a locked exit says why");
            _zone.Claim(line, id);
            ExitLines.Add(exit);
        }

        private void ReadDoor(ZoneLine line, string first)
        {
            if (line.Head.Count != 2) throw line.Fail("a door is door <x> = <target> · <badge> · \"<name>\"");
            float x = line.ReadNumber(line.Head[1], "the door's x");
            CheckX(line, x);
            Target to = ReadTarget(line, first);
            if (line.Segments.Count < 3)
                throw line.Fail("a door needs its badge and its name after where it goes: door 1180 = rusty-anvil start · inn · \"The Rusty Anvil\"");

            string word = line.Segments[1].ToLowerInvariant();
            BadgeKind? badge = null;
            foreach (BadgeKind kind in Enum.GetValues<BadgeKind>())
                if (BadgeName(kind) == word) badge = kind;
            if (badge is null)
            {
                var names = new List<string>();
                foreach (BadgeKind kind in Enum.GetValues<BadgeKind>()) names.Add(BadgeName(kind));
                throw line.Fail($"there's no badge called {word} (they're {string.Join(", ", names)})");
            }
            string name = line.Unquote(line.Segments[2], "a door's name");

            line.TakeFields(3);
            string id = line.Text("id");
            if (id is null && (to.Back || to.Locked)) throw line.Fail($"a door that goes {(to.Back ? "back" : "nowhere, locked,")} has to have an id");
            id ??= to.Place;
            float[] size = line.Numbers("size", 2);
            if (size is not null && (size[0] <= 0f || size[1] <= 0f)) throw line.Fail("a door's size has to be more than 0 each way");
            Vector2 box = size is not null ? W(size[0], size[1]) : DefaultSize(badge.Value);
            var door = new DoorLine(line.Line, id, W(x), to, badge.Value, name, line.Text("why"), box, line.Presence());
            line.Done("door", "id, why, size, when, unless");
            if (to.Locked && door.Why is null) throw line.Fail("a locked door has to say why, with why \"...\"");
            if (!to.Locked && door.Why is not null) throw line.Fail("only a locked door says why");
            _zone.Claim(line, id);
            DoorLines.Add(door);
        }

        /// <summary>How big a door's box is when its line doesn't say, by its badge, in world pixels: a gate or the palace's door 300 canvas pixels square, a stair 160 by 120, anything else 120 by 200.</summary>
        private static Vector2 DefaultSize(BadgeKind badge) => badge switch
        {
            BadgeKind.Gate or BadgeKind.Crown => W(300f, 300f),
            BadgeKind.StairUp or BadgeKind.StairDown => W(160f, 120f),
            _ => W(120f, 200f),
        };

        private void ReadTrigger(ZoneLine line, string id, string first)
        {
            float[] box = line.ReadNumbers(first, "a trigger's box (x0 y0 x1 y1)", 4);
            CheckXY(line, box[0], box[1]);
            CheckXY(line, box[2], box[3]);
            if (box[2] <= box[0] || box[3] <= box[1]) throw line.Fail("a trigger's x1 and y1 have to be more than its x0 and y0");

            line.TakeFields(1);
            var area = new BoundingRectangle(W(box[0]), W(box[1]), W(box[2] - box[0]), W(box[3] - box[1]));
            string talk = line.Text("talk"), say = line.Text("say"), set = line.Text("set"), bark = line.Text("bark");
            var trigger = new MapTrigger(id, area, line.YesNo("once", true), say, set, talk, bark, line.Flag("mark"), line.Presence());
            line.Done("trigger", "talk, say, set, bark, mark, once, when, unless");
            if (say is null && set is null && talk is null && bark is null)
                throw line.Fail("this trigger doesn't do anything (it needs talk, say, set or bark)");
            _zone.Claim(line, id);
            _triggers.Add(trigger);
        }

        private void ReadLight(ZoneLine line, string id, string first)
        {
            if (line.Segments.Count != 1) throw line.Fail("a light has no fields");
            string[] parts = first.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length is not (3 or 4)) throw line.Fail("a light is x y reach [strength]");
            float x = line.ReadNumber(parts[0], "a light's x"), y = line.ReadNumber(parts[1], "a light's y");
            float reach = line.ReadNumber(parts[2], "a light's reach");
            float strength = parts.Length == 4 ? line.ReadNumber(parts[3], "a light's strength") : 1f;
            CheckXY(line, x, y);
            if (reach <= 0f) throw line.Fail("a light's reach has to be more than 0");
            if (strength <= 0f || strength > 1f) throw line.Fail($"a light's strength has to be more than 0 and no more than 1, not {parts[3]}");
            if (id is not null) _zone.Claim(line, id);
            _lights.Add(new MapLight(W(x, y), W(reach), strength, id));
        }

        /// <summary>Someone standing in the room: one of the cast, someone off a sheet or a look, or a creature, as the tile maps had them.</summary>
        private MapNpc ReadNpc(ZoneLine line, string id, Vector2 feet, float canvasX)
        {
            string castId = line.Text("cast"), sheet = line.Text("sheet"), seed = line.Text("look"), creatureId = line.Text("creature");
            string name = line.Text("name");
            int drawn = (castId is null ? 0 : 1) + (sheet is null ? 0 : 1) + (seed is null ? 0 : 1) + (creatureId is null ? 0 : 1);
            if (drawn > 1) throw line.Fail("someone's drawn from one of cast, sheet, look or creature, not two of them");
            if (drawn == 0) throw line.Fail("someone needs cast, sheet, look or creature for how they look");

            CharacterLook look = null;
            if (castId is not null)
            {
                CastMember cast = Cast.Get(castId) ?? throw line.Fail($"there's nobody in the cast called {castId}");
                if (cast.Look is null && cast.Sheet is null) throw line.Fail($"{castId} is only ever a voice, and can't stand in a room");
                name ??= cast.Name;
            }
            else if (creatureId is not null)
            {
                Creature creature = Creatures.Get(creatureId)
                    ?? throw line.Fail($"there's no creature called {creatureId} (the creatures are {string.Join(", ", Creatures.Ids)})");
                name ??= creature.Name;
            }
            else if (seed is not null)
            {
                int picked = line.ReadWhole(seed, "look");
                if (name is null) throw line.Fail("someone drawn from look needs name too, since there's no telling who they are");
                look = LookOptions.Random(new Random(picked));
            }
            else
            {
                if (name is null) throw line.Fail("someone off a sheet needs name too");
                if (!Townsfolk.Contains(sheet)) throw line.Fail($"there's no sheet called {sheet} (the townsfolk are {string.Join(", ", Townsfolk.All)})");
            }

            Direction facing = Facing(line, canvasX);
            float wander = line.Number("wander", 0f);
            float[] patrol = line.Numbers("patrol", 2);
            string badgeText = line.Text("badge");
            string aboard = line.Text("aboard");
            string say = line.Text("say");
            string talk = line.Text("talk");
            MapPresence presence = line.Presence();
            line.Done("npc", "cast, sheet, look, creature, name, facing, wander, patrol, badge, aboard, say, talk, when, unless");

            (float Left, float Right)? walks = null;
            if (patrol is not null)
            {
                CheckX(line, patrol[0], "patrol's left");
                CheckX(line, patrol[1], "patrol's right");
                if (patrol[1] <= patrol[0]) throw line.Fail("patrol's right has to be more than its left");
                walks = (W(patrol[0]), W(patrol[1]));
            }
            if ((wander > 0f ? 1 : 0) + (patrol is null ? 0 : 1) + (aboard is null ? 0 : 1) > 1)
                throw line.Fail("wander, patrol and aboard exclude each other");

            BadgeKind? badge = null;
            if (badgeText is not null)
            {
                foreach (BadgeKind kind in Enum.GetValues<BadgeKind>())
                    if (BadgeName(kind) == badgeText.ToLowerInvariant()) badge = kind;
                if (badge is null) throw line.Fail($"there's no badge called {badgeText}");
            }

            var lines = new List<string>();
            if (say is not null)
            {
                foreach (string part in say.Split('|'))
                {
                    if (part.Trim().Length == 0) throw line.Fail("say has an empty line in it (two |s together, or one at an end)");
                    lines.Add(part.Trim());
                }
            }

            _zone.Claim(line, id);
            return new MapNpc(id, name, feet, facing, sheet, castId, look, creatureId, lines, talk, W(wander), presence, aboard, walks, badge);
        }

        /// <summary>A chest, sign, lever or waypoint, which looks like the prop of its kind unless look says otherwise.</summary>
        private MapInteractable ReadThing(ZoneLine line, string kindName, string id, Vector2 feet)
        {
            var kind = Enum.Parse<InteractableKind>(kindName, ignoreCase: true);
            string lookId = line.Text("look") ?? kindName;
            if (!PropCatalog.TryGet(lookId, out PropDefinition look)) throw line.Fail($"there's no prop called {lookId} for it to look like");
            int pictures = kind == InteractableKind.Sign ? 1 : 2;
            if (look.Frames != pictures)
                throw line.Fail($"the {lookId} prop has {look.Frames} {(look.Frames == 1 ? "picture" : "pictures")}, and a {kindName} needs {pictures}");

            string name = line.Text("name");
            int gold = 0;
            var items = new List<string>();
            string flag = null, text = null;
            string allowed;
            switch (kind)
            {
                case InteractableKind.Chest:
                    gold = line.Text("gold") is string goldText ? line.ReadWhole(goldText, "gold") : 0;
                    if (line.Text("items") is string itemText)
                    {
                        foreach (string part in itemText.Split(','))
                        {
                            string item = part.Trim();
                            if (item.Length == 0) throw line.Fail("items has nothing between two of its commas");
                            if (!ItemCatalog.Exists(item)) throw line.Fail($"there's no item called {item}");
                            items.Add(item);
                        }
                    }
                    allowed = "look, name, gold, items, when, unless";
                    break;
                case InteractableKind.Sign:
                    text = line.Required("text", kindName);
                    allowed = "look, name, text, when, unless";
                    break;
                case InteractableKind.Lever:
                    flag = line.Required("flag", kindName);
                    allowed = "look, name, flag, when, unless";
                    break;
                default:
                    _zone.ClaimWaypoint(line);
                    allowed = "look, name, when, unless";
                    break;
            }
            MapPresence presence = line.Presence();
            line.Done(kindName, allowed);
            _zone.Claim(line, id);

            return new MapInteractable
            {
                Kind = kind,
                Id = id,
                Foot = feet,
                Look = look,
                Presence = presence,
                Name = name ?? kind.ToString(),
                Gold = gold,
                Items = items,
                Flag = flag,
                Text = text,
            };
        }

        /// <summary>Checks the room has everything it needs, and builds it. Its exits and doors come once the zone's rooms are all read.</summary>
        public void Finish()
        {
            if (_width == 0) throw Fail(Where, "it doesn't set width");
            if (_name is null) throw Fail(Where, "it doesn't set name");
            if (_floor is null) throw Fail(Where, "it has no floor");
            if (_dark && _ambient is null) throw Fail(Where, "it's dark, so it has to set ambient (0 to 1)");
            if (!_dark && _ambient is not null) throw Fail(_zone.File, _settingLines["ambient"], "ambient only means something in a dark room");
            if (_tileset.Id is null || _tileTop is null || _rows.Count == 0)
                throw Fail(Where, "it has no tile strip (it needs tiles, tiletop, legend and rows)");
            if (_spawns.Count == 0) throw Fail(Where, "it has no spawn, and every room needs one");

            int rows = (int)MathF.Ceiling((CanvasHeight - _tileTop.Value) / (float)CanvasTile);
            if (_rows.Count != rows)
                throw Fail(_zone.File, _rows[^1].Line, $"a strip from tiletop {_tileTop} has {rows} rows, and this one has {_rows.Count}");
            if (_details.Count > 0 && _details.Count != _rows.Count)
                throw Fail(_zone.File, _details[^1].Line, $"there are {_details.Count} detail rows and {_rows.Count} rows; they have to match");

            TilesetCatalog.TryGet(_tileset.Id, out TilesetDefinition tileset);
            var ids = new Dictionary<char, int>();
            foreach ((char c, (int line, string tile)) in _legend)
            {
                if (!tileset.TryGetId(tile, out int tileId)) throw Fail(_zone.File, line, $"the {tileset.Id} tileset has no tile called {tile}");
                ids[c] = tileId;
            }

            int cells = _width / CanvasTile;
            var ground = new int[cells, rows];
            var detail = new int[cells, rows];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cells; x++)
                {
                    ground[x, y] = ids[_rows[y].Row[x]];
                    if (_details.Count > 0 && _details[y].Row[x] != Empty) detail[x, y] = ids[_details[y].Row[x]];
                }
            }

            foreach ((int line, MapNpc npc) in _npcs)
            {
                if (npc.Aboard is not null && !_props.Exists(prop => prop.Id == npc.Aboard || prop.Prop.Id == npc.Aboard))
                    throw Fail(_zone.File, line, $"{npc.Id} is aboard {npc.Aboard}, but there's no {npc.Aboard} in this room");
            }

            float middle = _width / 2f, half = MathF.Min(DefaultFight, _width) / 2f;
            (float left, float right) = _fight ?? (middle - half, middle + half);
            var npcs = new List<MapNpc>();
            foreach ((_, MapNpc npc) in _npcs) npcs.Add(npc);

            Definition = new RoomDefinition
            {
                Id = Id,
                ZoneId = _zone.Id,
                Name = _name,
                Paint = _paint ?? $"{_zone.Id}/{Id}",
                Kind = _kind,
                Width = W(_width),
                Floor = _floor,
                Blocks = _blocks,
                FightLeft = W(left),
                FightRight = W(right),
                Tiles = new TileLayers(tileset, ground, detail),
                TilesTop = W(_tileTop.Value),
                Scenery = _scenery,
                Life = _life,
                Dark = _dark,
                Ambient = _ambient ?? 1f,
                Tint = _tint,
                States = _states,
                Spawns = _spawns,
                Npcs = npcs,
                Triggers = _triggers,
                Things = _things,
                Props = _props,
                Lights = _lights,
                Camp = _camp,
                Scenes = _scenes,
            };
        }
    }

    /// <summary>Checks a floor is a polygon the walking can trust: three points or more, with area, none repeated, and no two edges crossing.</summary>
    private static class Polygon
    {
        /// <summary>What's wrong with the polygon, or null if it's a simple one.</summary>
        public static string Problem(List<Vector2> points)
        {
            int n = points.Count;
            if (n < 3) return "a floor needs at least three points";
            for (int i = 0; i < n; i++)
            {
                if (points[i] == points[(i + 1) % n])
                    return $"point {i + 1} is the same as the next (the floor closes itself; don't repeat the first point)";
            }

            float area = 0f;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = points[i], b = points[(i + 1) % n];
                area += a.X * b.Y - b.X * a.Y;
            }
            if (MathF.Abs(area) < 2f) return "the floor has no area";

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (j == i + 1 || (j + 1) % n == i) continue;
                    if (Cross(points[i], points[(i + 1) % n], points[j], points[(j + 1) % n]))
                        return $"the floor's edges {i + 1} and {j + 1} cross, so it isn't a simple polygon";
                }
            }
            return null;
        }

        /// <summary>Which side of a to b the point c is: above 0 to the left, below 0 to the right, 0 on the line.</summary>
        private static float Turn(Vector2 a, Vector2 b, Vector2 c) => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

        /// <summary>Whether c lies on the segment a to b, given it's on the line through them.</summary>
        private static bool Within(Vector2 a, Vector2 b, Vector2 c) =>
            c.X >= MathF.Min(a.X, b.X) && c.X <= MathF.Max(a.X, b.X) && c.Y >= MathF.Min(a.Y, b.Y) && c.Y <= MathF.Max(a.Y, b.Y);

        /// <summary>Whether the segments a to b and c to d touch or cross.</summary>
        private static bool Cross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float t1 = Turn(a, b, c), t2 = Turn(a, b, d), t3 = Turn(c, d, a), t4 = Turn(c, d, b);
            if (t1 * t2 < 0f && t3 * t4 < 0f) return true;
            return t1 == 0f && Within(a, b, c) || t2 == 0f && Within(a, b, d) || t3 == 0f && Within(c, d, a) || t4 == 0f && Within(c, d, b);
        }
    }
}
