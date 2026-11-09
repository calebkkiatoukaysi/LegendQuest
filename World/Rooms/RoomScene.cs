using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A room as something to draw: its painted layers at their scales and parallax, its tile strip, the
/// little things moving in it, and its dark.
/// </summary>
/// <remarks>
/// <para>
/// The layers are Content/Rooms/(Paint)/sky, far, mid and fore, painted by tools/rooms/paint_all.py
/// (or, for a room it has no recipe for yet, tools/rooms/generate_room_placeholders.py). A layer is
/// always the screen's height, 900 canvas pixels, at whatever resolution it was painted at, so each
/// is drawn at the scale its height gives (RoomHeight / its height): a painted layer is 900 texels
/// tall and drawn one texel to a screen pixel; a placeholder's sky and foreground are 225 tall (4 screen
/// pixels to a texel), its far layer 450 (2) and its mid layer 300 (3). A layer wider than the widest
/// texture the graphics profile takes is written in pieces, (layer)-0, (layer)-1 and on, drawn side by
/// side. A layer with parallax p slides p times as far as the room does: the sky 0.1, the far layer
/// 0.3, the mid layer and the tile strip 1, the foreground 1.15. A flat room has no sky or far layer
/// and nothing slides: an interior, or a room painted as one picture, sky to floor, which
/// tools/rooms/cut_room.py makes its mid layer (opaque down to the floor band, where the tile strip
/// shows through) with a foreground if it has one. The mid layer's buildings stand on the floor
/// band's back edge with their doorways on their door boxes, and doors.txt beside the layers says
/// where each doorway is painted (DoorArt). If a layer of a room that isn't flat hasn't been painted,
/// the room's scenery lines are drawn from the props' own pictures instead.
/// </para>
/// <para>
/// To wire it into ExploreScreen:
/// </para>
/// <list type="number">
/// <item>On entering a room, <c>new RoomScene(room)</c> and <c>LoadContent(Game.Services, GraphicsDevice)</c>;
/// on leaving it, <c>Unload()</c>. It loads into a content manager of its own, so a room's pictures go when it does.</item>
/// <item>Lock the camera to it: <c>camera.LockTo(scene.Bounds, (int)ZoneCatalog.RoomScale)</c>.</item>
/// <item>Each frame, <c>Update(gameTime)</c>.</item>
/// <item>To draw, outside any batch: <c>DrawBack</c>, then the world batch (back to front, each prop and
/// walker at <c>Depth(feet)</c>), then <c>DrawFront</c>, then <c>DrawDark</c>, then the glows, indicators and
/// screen. Each of the three starts and ends its own batches, and takes the shake a cue's put on the
/// screen (ScreenShake.Transform) so the painting shakes with the people. In a dark room DrawDark is
/// what puts the canvas's alpha back after the sky and far layer drew, so it has to come after DrawBack.</item>
/// <item>Footsteps read <c>SurfaceAt(feet)</c>; the indicators outline <c>DoorArt(door)</c> on hover and float
/// the badge over it, while the click stays the door's own Area.</item>
/// </list>
/// </remarks>
public sealed class RoomScene
{
    /// <summary>Screen pixels to an art pixel for each layer drawn from the props' own pictures, when it hasn't been painted. The world is RoomScale screen pixels to a world pixel.</summary>
    private const float FarScale = 2f, MidScale = 3f, ForeScale = 4f;

    /// <summary>How far each layer slides for every world pixel the camera does, unless the room is flat.</summary>
    private const float SkyParallax = 0.1f, FarParallax = 0.3f, ForeParallax = 1.15f;

    /// <summary>Half the screen's width in world pixels: what something in a sliding layer is placed about, so it's where it should be when the camera's centred on it.</summary>
    private const float HalfView = LegendQuestGame.ScreenWidth / 2f / ZoneCatalog.RoomScale;

    /// <summary>How far above the floor's back edge the far layer's things stand, in world pixels, when they're drawn from the props.</summary>
    private const float FarGround = 10f;

    /// <summary>How much of a foreground prop sinks below the bottom of the screen, when it's drawn from the props.</summary>
    private const float ForeSink = 0.18f;

    /// <summary>The sky when it hasn't been painted, top to horizon: the title's blue going pink.</summary>
    private static readonly Color[] SkyBands =
    {
        new(78, 124, 210), new(112, 158, 230), new(168, 184, 240), new(226, 184, 224), new(252, 214, 206),
    };

    private readonly RoomDefinition _room;
    private readonly Tilemap _strip;
    private readonly LifeLoops _life;
    private readonly Dictionary<string, BoundingRectangle> _doorArt = new();

    /// <summary>What each unpainted layer is drawn from: a prop's picture, its first frame, and where its foot goes in the layer, in world pixels.</summary>
    private readonly Dictionary<string, List<(Texture2D Texture, Rectangle Source, Vector2 Foot)>> _pieces = new();

    private ContentManager _content;
    private LightMap _lightMap;
    /// <summary>Each painted layer's pieces, left to right (most layers are one), or null if it hasn't been painted.</summary>
    private List<Texture2D> _sky, _far, _mid, _fore;
    private Texture2D _pixel;

    /// <summary>Whether the room has no sky and no far layer: an interior, or a street painted as one picture. Nothing in it slides.</summary>
    private bool _flat;

    /// <summary>Gets a room ready to draw. Nothing loads until LoadContent.</summary>
    /// <param name="room">The room.</param>
    public RoomScene(RoomDefinition room)
    {
        _room = room;
        _strip = new Tilemap(room.Tiles, new Point(0, (int)room.TilesTop));
        _life = new LifeLoops(room);
    }

    /// <summary>The room, in world pixels: what the camera's locked to.</summary>
    public Rectangle Bounds => new(0, 0, (int)_room.Width, (int)ZoneCatalog.RoomHeight);

    /// <summary>Loads the room's layers, tiles, doorways and the pictures its life loops use, and builds its dark.</summary>
    /// <param name="services">The game's services, for the room's own content manager.</param>
    /// <param name="device">The device, for the textures made in code.</param>
    public void LoadContent(IServiceProvider services, GraphicsDevice device)
    {
        _content = new ContentManager(services, "Content");
        _strip.LoadContent(_content);
        _pixel = new Texture2D(device, 1, 1);
        _pixel.SetData(new[] { Color.White });

        if (_room.Kind == RoomKind.Street)
        {
            _sky = Layer("sky");
            _far = Layer("far");
        }
        _flat = _sky is null && _far is null;
        _mid = Layer("mid");
        _fore = Layer("fore");
        if (_far is null && !_flat) ComposePieces("far", FarParallax, FarScale);
        if (_mid is null) ComposePieces("mid", 1f, MidScale);
        if (_fore is null && !_flat) ComposePieces("fore", Parallax(ForeParallax), ForeScale);

        ReadDoorArt();
        _life.LoadContent(_content);

        if (!_room.Dark) return;
        _lightMap = new LightMap(Bounds, _room.Ambient, Lights());
        _lightMap.LoadContent(device);
    }

    /// <summary>Lets go of everything the room loaded or made.</summary>
    public void Unload()
    {
        _lightMap?.Unload();
        _pixel?.Dispose();
        _content?.Dispose();
    }

    /// <summary>Runs the life loops.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime) => _life.Update(gameTime);

    /// <summary>Draws everything behind the people with the screen still: the sky and the far layer (unless the room is flat), the tile strip, the mid layer, and the life that's behind everyone.</summary>
    /// <param name="gameTime">The frame's timing, for the water's ripples.</param>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="camera">The camera, locked to the room.</param>
    /// <param name="tint">The room's light colour, multiplied into everything.</param>
    public void DrawBack(GameTime gameTime, SpriteBatch spriteBatch, Camera2D camera, Color tint) =>
        DrawBack(gameTime, spriteBatch, camera, tint, Matrix.Identity);

    /// <summary>Draws everything behind the people: the sky and the far layer (unless the room is flat), the tile strip, the mid layer, and the life that's behind everyone.</summary>
    /// <param name="gameTime">The frame's timing, for the water's ripples.</param>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="camera">The camera, locked to the room.</param>
    /// <param name="tint">The room's light colour, multiplied into everything.</param>
    /// <param name="shake">The shake on the screen, after the camera: ScreenShake.Transform.</param>
    public void DrawBack(GameTime gameTime, SpriteBatch spriteBatch, Camera2D camera, Color tint, Matrix shake)
    {
        Matrix world = camera.Transform * shake;
        if (!_flat)
        {
            // Out of the dark: a painted night sky already has its light.
            BlendState blend = _lightMap is null ? BlendState.AlphaBlend : LightMap.OutOfTheDark;
            spriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointClamp, transformMatrix: world);
            float skyLeft = LayerLeft(camera, SkyParallax);
            if (_sky is not null) DrawLayer(spriteBatch, _sky, skyLeft, tint);
            else DrawSkyBands(spriteBatch, skyLeft, tint);
            _life.DrawSky(spriteBatch, skyLeft, tint);
            float farLeft = LayerLeft(camera, FarParallax);
            if (_far is not null) DrawLayer(spriteBatch, _far, farLeft, tint);
            else DrawPieces(spriteBatch, "far", farLeft, FarScale, tint);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        _strip.Draw(gameTime, spriteBatch, camera.View, tint);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        if (_mid is not null) DrawLayer(spriteBatch, _mid, 0f, tint);
        else DrawPieces(spriteBatch, "mid", 0f, MidScale, tint);
        _life.DrawMid(spriteBatch, tint);
        spriteBatch.End();

        _life.DrawBehind(spriteBatch, world);
    }

    /// <summary>Draws what's in front of the people with the screen still: the foreground, and the life that drifts in front of everyone.</summary>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="camera">The camera, locked to the room.</param>
    /// <param name="tint">The room's light colour.</param>
    public void DrawFront(SpriteBatch spriteBatch, Camera2D camera, Color tint) => DrawFront(spriteBatch, camera, tint, Matrix.Identity);

    /// <summary>Draws what's in front of the people: the foreground, and the life that drifts in front of everyone.</summary>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="camera">The camera, locked to the room.</param>
    /// <param name="tint">The room's light colour.</param>
    /// <param name="shake">The shake on the screen, after the camera: ScreenShake.Transform.</param>
    public void DrawFront(SpriteBatch spriteBatch, Camera2D camera, Color tint, Matrix shake)
    {
        Matrix world = camera.Transform * shake;
        float left = LayerLeft(camera, Parallax(ForeParallax));
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        if (_fore is not null) DrawLayer(spriteBatch, _fore, left, tint);
        else DrawPieces(spriteBatch, "fore", left, ForeScale, tint);
        spriteBatch.End();

        _life.DrawFront(spriteBatch, world);
    }

    /// <summary>In a dark room, with the screen still, lays the dark over the floor, the mid layer, the people and the foreground (not the sky or the far layer), then the candles' glow over it. Nothing in a lit room.</summary>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="camera">The camera, locked to the room.</param>
    public void DrawDark(SpriteBatch spriteBatch, Camera2D camera) => DrawDark(spriteBatch, camera, Matrix.Identity);

    /// <summary>In a dark room, lays the dark over the floor, the mid layer, the people and the foreground (not the sky or the far layer), then the candles' glow over it. Nothing in a lit room.</summary>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="camera">The camera, locked to the room.</param>
    /// <param name="shake">The shake on the screen, after the camera: ScreenShake.Transform.</param>
    public void DrawDark(SpriteBatch spriteBatch, Camera2D camera, Matrix shake)
    {
        if (_lightMap is null) return;

        Matrix world = camera.Transform * shake;
        _lightMap.DrawMasked(spriteBatch, world);
        spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp, transformMatrix: world);
        _life.DrawLight(spriteBatch);
        spriteBatch.End();
    }

    /// <summary>What the ground is like underfoot, for the footsteps: the tile strip's tile there.</summary>
    /// <param name="feet">Where, in world pixels.</param>
    public Surface SurfaceAt(Vector2 feet) => _strip.SurfaceAt(feet);

    /// <summary>The layer depth for something standing with its feet here: further down the floor is further in front.</summary>
    /// <param name="feet">Where its feet are, in world pixels.</param>
    public float Depth(Vector2 feet) => Layers.OnFloor(feet.Y, ZoneCatalog.RoomHeight);

    /// <summary>Where a door's doorway is painted, in world pixels, for its hover outline and badge; its own box if nothing's painted for it.</summary>
    /// <param name="door">One of the room's doors.</param>
    public BoundingRectangle DoorArt(RoomDoor door) => _doorArt.TryGetValue(door.Id, out BoundingRectangle art) ? art : door.Area;

    /// <summary>A layer's parallax in this room: none in a flat one.</summary>
    private float Parallax(float outdoors) => _flat ? 1f : outdoors;

    /// <summary>Where a layer's left edge goes in the world for where the camera is, on a whole screen pixel so it doesn't shimmer.</summary>
    private static float LayerLeft(Camera2D camera, float parallax) =>
        MathF.Round((1f - parallax) * camera.Left * camera.Zoom) / camera.Zoom;

    /// <summary>Draws a painted layer with its left edge at a world x: each piece at the scale its height gives (the layer is the room's height), the next starting where it ends.</summary>
    private static void DrawLayer(SpriteBatch spriteBatch, List<Texture2D> pieces, float left, Color tint)
    {
        float x = left;
        foreach (Texture2D piece in pieces)
        {
            float scale = ZoneCatalog.RoomHeight / piece.Height;
            spriteBatch.Draw(piece, new Vector2(x, 0f), null, tint, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            x += piece.Width * scale;
        }
    }

    /// <summary>Loads one of the room's painted layers: the one picture, or its pieces (name-0, name-1 ...) if it was too wide for one; null if it hasn't been painted.</summary>
    private List<Texture2D> Layer(string name)
    {
        Texture2D whole = TryLoad($"Rooms/{_room.Paint}/{name}");
        if (whole is not null) return new List<Texture2D> { whole };

        var pieces = new List<Texture2D>();
        for (Texture2D piece; (piece = TryLoad($"Rooms/{_room.Paint}/{name}-{pieces.Count}")) is not null;) pieces.Add(piece);
        return pieces.Count > 0 ? pieces : null;
    }

    /// <summary>Loads a picture from the room's content manager, or null if there isn't one.</summary>
    private Texture2D TryLoad(string asset)
    {
        try
        {
            return _content.Load<Texture2D>(asset);
        }
        catch (ContentLoadException)
        {
            return null;
        }
    }

    /// <summary>Gets an unpainted layer's scenery lines ready to draw from the props' own pictures: each prop where the painter would have put it.</summary>
    private void ComposePieces(string layer, float parallax, float scale)
    {
        var pieces = new List<(Texture2D, Rectangle, Vector2)>();
        float back = float.MaxValue;
        foreach (Vector2 point in _room.Floor) back = MathF.Min(back, point.Y);

        foreach (RoomScenery line in _room.Scenery)
        {
            if (line.Layer != layer || !PropCatalog.TryGet(line.Prop, out PropDefinition prop)) continue;

            Texture2D texture = _content.Load<Texture2D>(prop.Texture);
            var source = new Rectangle(0, 0, texture.Width / prop.Frames, texture.Height);
            float x = HalfView + parallax * (line.X - HalfView);
            float y = layer switch
            {
                "far" => back - FarGround,
                "mid" => _room.BackEdgeAt(line.X),
                _ => ZoneCatalog.RoomHeight + source.Height * scale / ZoneCatalog.RoomScale * ForeSink,
            };
            pieces.Add((texture, source, new Vector2(x, y)));
        }
        _pieces[layer] = pieces;
    }

    /// <summary>Draws an unpainted layer's props, each standing on its foot.</summary>
    private void DrawPieces(SpriteBatch spriteBatch, string layer, float left, float scale, Color tint)
    {
        if (!_pieces.TryGetValue(layer, out var pieces)) return;

        foreach ((Texture2D texture, Rectangle source, Vector2 foot) in pieces)
        {
            var origin = new Vector2(source.Width / 2, source.Height);
            spriteBatch.Draw(texture, new Vector2(left + foot.X, foot.Y), source, tint, 0f, origin,
                scale / ZoneCatalog.RoomScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>The sky when it hasn't been painted: the title's colours in bands down to the floor.</summary>
    private void DrawSkyBands(SpriteBatch spriteBatch, float left, Color tint)
    {
        float band = ZoneCatalog.RoomHeight / SkyBands.Length;
        for (int i = 0; i < SkyBands.Length; i++)
        {
            var area = new Rectangle((int)left, (int)(i * band), (int)_room.Width, (int)MathF.Ceiling(band));
            spriteBatch.Draw(_pixel, area, new Color(SkyBands[i].ToVector4() * tint.ToVector4()));
        }
    }

    /// <summary>Reads doors.txt, where the painter says each door's doorway is: id x y width height, in canvas pixels.</summary>
    /// <exception cref="InvalidOperationException">If a line of it is wrong. A room with no doors.txt just has no doorways painted.</exception>
    private void ReadDoorArt()
    {
        string path = $"{_content.RootDirectory}/Rooms/{_room.Paint}/doors.txt";
        string[] lines;
        try
        {
            using Stream stream = TitleContainer.OpenStream(path);
            using var reader = new StreamReader(stream);
            lines = reader.ReadToEnd().Split('\n');
        }
        catch (Exception missing) when (missing is FileNotFoundException or DirectoryNotFoundException)
        {
            return;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line[0] == ';') continue;

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var numbers = new float[4];
            if (parts.Length != 5 || !TryNumbers(parts, numbers))
                throw new InvalidOperationException($"{path} line {i + 1}: a doorway is id x y width height, not \"{line}\".");

            float scale = ZoneCatalog.RoomScale;
            _doorArt[parts[0]] = new BoundingRectangle(numbers[0] / scale, numbers[1] / scale, numbers[2] / scale, numbers[3] / scale);
        }
    }

    /// <summary>Reads the four numbers after a doorway's id.</summary>
    private static bool TryNumbers(string[] parts, float[] numbers)
    {
        for (int i = 0; i < numbers.Length; i++)
            if (!float.TryParse(parts[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out numbers[i])) return false;
        return true;
    }

    /// <summary>Every pool of light in the room: the props that give off light, and its own lights.</summary>
    private List<(Vector2 Pool, float Reach, float Strength)> Lights()
    {
        var lights = new List<(Vector2 Pool, float Reach, float Strength)>();
        foreach (MapProp placed in _room.Props)
            if (placed.Prop.Light is { } light) lights.Add((placed.Foot + light.Pool, light.Reach, light.Strength));
        foreach (MapLight light in _room.Lights) lights.Add((light.Pool, light.Reach, light.Strength));
        return lights;
    }
}
