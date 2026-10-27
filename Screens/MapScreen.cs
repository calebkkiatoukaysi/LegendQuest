using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Components;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map, as a popup over the map you're on. One side is that map, as much of it as you've seen;
/// the other is the world, with every place you've been and the roads between them. Opened from a
/// waypoint, the world side is where you pick somewhere to travel to.
/// </summary>
/// <remarks>
/// The local map is the real tiles drawn at one screen pixel per art pixel and browned, with the dark
/// laid over what hasn't been seen. The dark is one texel per tile stretched over it with smooth
/// sampling, the same trick as the dark on a dark map, so the edge of what you've seen is soft.
/// </remarks>
public class MapScreen : GameScreen
{
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.2);

    /// <summary>The panel's size, in screen pixels. Big enough for the village at one to one, with room round it.</summary>
    private const int PanelWidth = 1380;
    private const int PanelHeight = 820;

    /// <summary>The space between the two tab names at the top, and between them and the map under them.</summary>
    private const float TabGap = 90f;
    private const float AreaGap = 18f;

    /// <summary>One mark on map-marks.png, in art pixels, and how much they're blown up. Has to match MARK in tools/generate_ui.py.</summary>
    private const int MarkSize = 12;
    private const float MarkScale = 3f;

    /// <summary>Which mark is which on map-marks.png, left to right.</summary>
    private const int PlaceMark = 0;
    private const int UnknownMark = 1;
    private const int HereMark = 2;
    private const int WaypointMark = 3;
    private const int SelectedMark = 4;
    private const int YouMark = 5;
    private const int DotMark = 6;

    /// <summary>The world map's roads: how far apart the dots are, and how far short of each place they stop, in screen pixels.</summary>
    private const float DotSpacing = 14f;
    private const float RoadClear = 24f;

    /// <summary>How close the mouse has to be to a place on the world map to pick it.</summary>
    private const float PickReach = 30f;

    /// <summary>Where a place's name goes under it, and where its waypoint mark sits over it (out of the way of the roads, which come in from the sides).</summary>
    private const float NameDrop = 22f;
    private static readonly Vector2 WaypointOver = new(0f, -34f);

    /// <summary>Colour the local map's tiles are multiplied by, so it looks drawn on old paper more than like the map itself.</summary>
    private static readonly Color MapTint = new(210, 192, 160);

    /// <summary>What covers the parts of the map that haven't been seen. The colour of the inside of the panel, so they just aren't there.</summary>
    private static readonly Color Unseen = new(17, 15, 15);

    private const string WorldName = "THE WORLD";
    private const string HereHint = "TAB  ·  THE WORLD      M  ·  CLOSE";
    private const string WorldHint = "TAB  ·  THIS MAP      M  ·  CLOSE";
    private const string TravelHint = "ARROWS  ·  CHOOSE      ENTER  ·  TRAVEL TO {0}      ESC  ·  STAY";
    private const string NowhereHint = "THERE'S NO OTHER WAYPOINT FOUND YET      ESC  ·  CLOSE";

    private readonly InputAction _close = new(new[] { Buttons.B, Buttons.Y, Buttons.Back }, new[] { Keys.Escape, Keys.M }, true);
    private readonly InputAction _switch = new(new[] { Buttons.LeftShoulder, Buttons.RightShoulder }, new[] { Keys.Tab }, true);
    private readonly InputAction _previous = new(new[] { Buttons.DPadLeft, Buttons.DPadUp, Buttons.LeftThumbstickLeft }, new[] { Keys.Left, Keys.Up, Keys.A, Keys.W }, true);
    private readonly InputAction _next = new(new[] { Buttons.DPadRight, Buttons.DPadDown, Buttons.LeftThumbstickRight }, new[] { Keys.Right, Keys.Down, Keys.D, Keys.S }, true);
    private readonly InputAction _travel = new(new[] { Buttons.A }, new[] { Keys.Enter, Keys.Space, Keys.E }, true);

    private readonly MapDefinition _map;
    private readonly Tilemap _tiles;
    private readonly Discovery _seen;
    private readonly WorldState _world;
    private readonly Vector2 _you;
    private readonly bool _travelling;
    private readonly List<MapDefinition> _destinations;
    private readonly FormPanel _panel = new();

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Texture2D _marks;
    private Texture2D _unseen;

    private Rectangle _panelBounds;
    private Rectangle _area;
    private Rectangle _hereTab;
    private Rectangle _worldTab;
    private MapView _view;
    private int _selected;

    /// <summary>Raised when a place is picked to travel to. Whoever opened the map does the travelling.</summary>
    public event Action<MapDefinition> Travel;

    /// <summary>Opens the map on the map you're on, or on the world if you're travelling.</summary>
    /// <param name="map">The map you're on.</param>
    /// <param name="tiles">Its tiles, loaded, to draw the local map with.</param>
    /// <param name="seen">What's been seen of it.</param>
    /// <param name="world">The rules, for which places are discovered and which waypoints found.</param>
    /// <param name="you">Where your feet are, in world pixels.</param>
    /// <param name="travelling">True if this was opened from a waypoint, so you can pick somewhere to go.</param>
    public MapScreen(MapDefinition map, Tilemap tiles, Discovery seen, WorldState world, Vector2 you, bool travelling)
    {
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _map = map;
        _tiles = tiles;
        _seen = seen;
        _world = world;
        _you = you;
        _travelling = travelling;
        _destinations = travelling ? world.Destinations() : new List<MapDefinition>();
        _view = travelling ? MapView.World : MapView.Here;
    }

    /// <summary>Loads the panel, the fonts and the marks, lays it all out, and builds the dark over what hasn't been seen.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();

        _panel.LoadContent(_content, ScreenManager.GraphicsDevice);
        _font = _content.Load<SpriteFont>("spectral-ui");
        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        _marks = _content.Load<Texture2D>("map-marks");

        LayOut();
        _unseen = BuildUnseen();
        _audio.Play(Sfx.MenuConfirm);
    }

    /// <summary>Unloads everything, and disposes the dark, since the content manager didn't make it.</summary>
    public override void Unload()
    {
        _unseen?.Dispose();
        _content?.Unload();
    }

    /// <summary>Closing, switching sides, and on the world side, picking somewhere and going there.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_close.Occurred(input))
        {
            _audio.Play(Sfx.MenuBack);
            ExitScreen();
            return;
        }

        Rectangle otherTab = _view == MapView.Here ? _worldTab : _hereTab;
        if (_switch.Occurred(input) || input.IsNewLeftClick && otherTab.Contains(input.MousePosition))
        {
            _view = _view == MapView.Here ? MapView.World : MapView.Here;
            _audio.Play(Sfx.OptionChange);
            return;
        }

        if (_view != MapView.World || _destinations.Count == 0) return;

        int was = _selected;
        int? pointedAt = DestinationAt(input.MousePosition);
        if (input.MouseMoved && pointedAt is int hovered) _selected = hovered;
        if (_previous.Occurred(input)) _selected = (_selected + _destinations.Count - 1) % _destinations.Count;
        if (_next.Occurred(input)) _selected = (_selected + 1) % _destinations.Count;
        if (_selected != was) _audio.Play(Sfx.MenuMove);

        if (_travel.Occurred(input) || input.IsNewLeftClick && pointedAt == _selected) Go();
    }

    /// <summary>Draws the veil and the panel, then whichever side is up, then the tabs and the hint.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        _panel.DrawVeil(spriteBatch, alpha);
        _panel.Draw(spriteBatch, _panelBounds, alpha);
        spriteBatch.End();

        if (_view == MapView.Here) DrawHere(gameTime, spriteBatch, alpha);
        else DrawWorld(spriteBatch, alpha);

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        DrawTab(spriteBatch, _map.Name.ToUpperInvariant(), _hereTab, _view == MapView.Here, alpha);
        DrawTab(spriteBatch, WorldName, _worldTab, _view == MapView.World, alpha);
        Text.Draw(spriteBatch, _detailFont, HintLine(), _panelBounds.Center.X, _panelBounds.Bottom - FormPanel.Padding - _detailFont.LineSpacing,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        spriteBatch.End();
    }

    /// <summary>For the proof run, since it can't press keys: shows one side or the other.</summary>
    /// <param name="view">Which side.</param>
    internal void ProofShow(MapView view) => _view = view;

    /// <summary>For the proof run: picks a place by its id and travels there.</summary>
    /// <param name="mapId">The map to go to. It has to be one of the places you can go.</param>
    internal void ProofTravel(string mapId)
    {
        _selected = _destinations.FindIndex(map => map.Id == mapId);
        Go();
    }

    /// <summary>Closes the map and says where to go.</summary>
    private void Go()
    {
        _audio.Play(Sfx.MenuConfirm);
        ExitScreen();
        Travel?.Invoke(_destinations[_selected]);
    }

    /// <summary>The line along the bottom: what the keys do on this side.</summary>
    private string HintLine()
    {
        if (_view == MapView.Here) return HereHint;
        if (!_travelling) return WorldHint;
        if (_destinations.Count == 0) return NowhereHint;
        return string.Format(CultureInfo.InvariantCulture, TravelHint, _destinations[_selected].Name.ToUpperInvariant());
    }

    /// <summary>Draws the map you're on: its tiles, the dark over what hasn't been seen, where the exits go, and you.</summary>
    private void DrawHere(GameTime gameTime, SpriteBatch spriteBatch, float alpha)
    {
        Rectangle drawn = HereBounds(out float scale);
        Matrix place = Matrix.CreateScale(scale, scale, 1f) * Matrix.CreateTranslation(drawn.X, drawn.Y, 0f);

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: place);
        _tiles.Draw(gameTime, spriteBatch, _tiles.Bounds, MapTint * alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        spriteBatch.Draw(_unseen, drawn, Color.White * alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        if (_world.FoundWaypoint(_map) is MapInteractable waypoint)
            DrawMark(spriteBatch, WaypointMark, drawn.Location.ToVector2() + waypoint.Foot * scale, alpha);
        DrawMark(spriteBatch, YouMark, drawn.Location.ToVector2() + _you * scale, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        foreach (MapExit exit in _map.Exits)
        {
            if (!AnySeen(exit)) continue;

            string name = MapCatalog.Get(exit.ToMap).Name.ToUpperInvariant();
            Vector2 size = _detailFont.MeasureString(name);
            var middle = new Vector2(exit.Area.X + exit.Area.Width / 2f, exit.Area.Y + exit.Area.Height / 2f) * scale + drawn.Location.ToVector2();

            // Kept inside the map, since most exits are right on its edge.
            float x = Math.Clamp(middle.X, drawn.Left + size.X / 2f, drawn.Right - size.X / 2f);
            float y = Math.Clamp(middle.Y - size.Y / 2f, drawn.Top, drawn.Bottom - size.Y);
            Text.Draw(spriteBatch, _detailFont, name, x, y, Palette.Bone * alpha, Palette.FormShadow * alpha, centred: true);
        }
        spriteBatch.End();
    }

    /// <summary>Draws the world: a mark for every place you've been, the roads between them, the ones you know of but haven't been to, and the waypoints.</summary>
    private void DrawWorld(SpriteBatch spriteBatch, float alpha)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        foreach (MapDefinition map in MapCatalog.All)
        {
            if (!_world.IsDiscovered(map)) continue;

            // Each road once: from a place to anywhere it leads that isn't discovered, or that comes later in the list.
            foreach (string id in MapCatalog.Links(map.Id))
            {
                MapDefinition other = MapCatalog.Get(id);
                if (!_world.IsDiscovered(other) || string.CompareOrdinal(map.Id, other.Id) < 0)
                    DrawRoad(spriteBatch, PlaceOf(map), PlaceOf(other), alpha);
                if (!_world.IsDiscovered(other)) DrawMark(spriteBatch, UnknownMark, PlaceOf(other), alpha);
            }

            DrawMark(spriteBatch, PlaceMark, PlaceOf(map), alpha);
            if (map == _map) DrawMark(spriteBatch, HereMark, PlaceOf(map), alpha);
            if (_world.FoundWaypoint(map) is not null) DrawMark(spriteBatch, WaypointMark, PlaceOf(map) + WaypointOver, alpha);
        }
        if (_destinations.Count > 0) DrawMark(spriteBatch, SelectedMark, PlaceOf(_destinations[_selected]), alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        foreach (MapDefinition map in MapCatalog.All)
        {
            if (!_world.IsDiscovered(map)) continue;

            bool picked = _destinations.Count > 0 && _destinations[_selected] == map;
            Color colour = picked ? Palette.Torch : map == _map ? Palette.Heading : Palette.Bone;
            Vector2 at = PlaceOf(map);
            Text.Draw(spriteBatch, _detailFont, map.Name.ToUpperInvariant(), at.X, at.Y + NameDrop,
                colour * alpha, Palette.FormShadow * alpha, centred: true);
        }
        spriteBatch.End();
    }

    /// <summary>Draws one tab name: bright if it's the side that's up, dim if it isn't.</summary>
    private void DrawTab(SpriteBatch spriteBatch, string name, Rectangle tab, bool up, float alpha) =>
        Text.Draw(spriteBatch, _font, name, tab.Center.X, tab.Top, (up ? Palette.Heading : Palette.Note) * alpha,
            Palette.FormShadow * alpha, centred: true);

    /// <summary>A dotted road between two places, stopping short of each.</summary>
    private void DrawRoad(SpriteBatch spriteBatch, Vector2 from, Vector2 to, float alpha)
    {
        float length = Vector2.Distance(from, to);
        int dots = (int)((length - RoadClear * 2f) / DotSpacing);
        Vector2 step = (to - from) / length;

        for (int i = 0; i <= dots; i++)
            DrawMark(spriteBatch, DotMark, from + step * (RoadClear + i * DotSpacing), alpha);
    }

    /// <summary>Draws one mark from map-marks.png, centred on a point.</summary>
    private void DrawMark(SpriteBatch spriteBatch, int mark, Vector2 centre, float alpha)
    {
        var source = new Rectangle(mark * MarkSize, 0, MarkSize, MarkSize);
        var at = new Vector2(MathF.Round(centre.X), MathF.Round(centre.Y));
        spriteBatch.Draw(_marks, at, source, Color.White * alpha, 0f, new Vector2(MarkSize / 2f), MarkScale, SpriteEffects.None, 0f);
    }

    /// <summary>Where a map is on the world side, in screen pixels.</summary>
    private Vector2 PlaceOf(MapDefinition map) => new(
        _area.Center.X - MapCatalog.WorldWidth / 2f + map.World.X,
        _area.Center.Y - MapCatalog.WorldHeight / 2f + map.World.Y);

    /// <summary>Which of the places you can go the mouse is over, or null.</summary>
    private int? DestinationAt(Point mouse)
    {
        for (int i = 0; i < _destinations.Count; i++)
            if (Vector2.Distance(mouse.ToVector2(), PlaceOf(_destinations[i])) <= PickReach) return i;
        return null;
    }

    /// <summary>Where the local map goes on screen: centred in the panel, one screen pixel per art pixel, or halved until it fits.</summary>
    /// <param name="scale">Screen pixels per art pixel.</param>
    private Rectangle HereBounds(out float scale)
    {
        Rectangle map = _tiles.Bounds;
        scale = 1f;
        while (map.Width * scale > _area.Width || map.Height * scale > _area.Height) scale /= 2f;

        int width = (int)(map.Width * scale), height = (int)(map.Height * scale);
        return new Rectangle(_area.Center.X - width / 2, _area.Center.Y - height / 2, width, height);
    }

    /// <summary>Whether any tile of an exit has been seen.</summary>
    private bool AnySeen(MapExit exit)
    {
        int left = (int)(exit.Area.Left / Tilemap.TileSize), right = (int)(exit.Area.Right / Tilemap.TileSize);
        int top = (int)(exit.Area.Top / Tilemap.TileSize), bottom = (int)(exit.Area.Bottom / Tilemap.TileSize);

        for (int y = top; y < bottom; y++)
            for (int x = left; x < right; x++)
                if (_seen.IsSeen(x, y)) return true;
        return false;
    }

    /// <summary>Builds the dark that goes over what hasn't been seen: one texel per tile, clear where it's been seen.</summary>
    private Texture2D BuildUnseen()
    {
        Rectangle map = _tiles.Bounds;
        int width = map.Width / Tilemap.TileSize, height = map.Height / Tilemap.TileSize;
        var texels = new Color[width * height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                texels[y * width + x] = _seen.IsSeen(x, y) ? Color.Transparent : Unseen;

        var unseen = new Texture2D(ScreenManager.GraphicsDevice, width, height);
        unseen.SetData(texels);
        return unseen;
    }

    /// <summary>Centres the panel, and works out where the tabs, the map and the hint go inside it.</summary>
    private void LayOut()
    {
        _panelBounds = new Rectangle(
            (LegendQuestGame.ScreenWidth - PanelWidth) / 2,
            (LegendQuestGame.ScreenHeight - PanelHeight) / 2,
            PanelWidth,
            PanelHeight);

        int tabsTop = _panelBounds.Top + FormPanel.Padding;
        Vector2 here = _font.MeasureString(_map.Name.ToUpperInvariant());
        Vector2 world = _font.MeasureString(WorldName);
        float left = _panelBounds.Center.X - (here.X + TabGap + world.X) / 2f;

        _hereTab = new Rectangle((int)left, tabsTop, (int)here.X, _font.LineSpacing);
        _worldTab = new Rectangle((int)(left + here.X + TabGap), tabsTop, (int)world.X, _font.LineSpacing);

        int areaTop = (int)(tabsTop + _font.LineSpacing + AreaGap);
        int areaBottom = (int)(_panelBounds.Bottom - FormPanel.Padding - _detailFont.LineSpacing - AreaGap);
        _area = new Rectangle(_panelBounds.Left + FormPanel.Padding, areaTop, PanelWidth - FormPanel.Padding * 2, areaBottom - areaTop);
    }
}
