using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Components;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The zone map, as a popup over the room you're in (12-rooms §2.3). One side is the zone's rooms as
/// thumbnails, laid out the way they join, each with its badges and its "!", and you on yours: a room
/// you've walked into can be jumped to from there. The other side is the world, with every place
/// you've been and the roads between them. Opened from a waypoint, the world side is where you pick
/// somewhere to travel to.
/// </summary>
/// <remarks>
/// <para>The rooms are placed by how they join: east and west along a row, up and down a row apart, and a
/// door into another room of the zone a row down, since through a door is in or behind. One whose
/// place is taken goes on along its row, and a room nothing joins starts a row of its own, so every room
/// in the zone file shows. The thumbnails are Content/Rooms/&lt;paint&gt;/thumb.png, at 2x if they all fit,
/// 1x if not, or as small as it takes; a room without one is a plain plate. A room you haven't been in
/// is shown dark, without its badges, but with its "!" if it has one, since that's what you'd go there
/// for. Jumping is the screen that opened it's to do, through JumpTo, and it says with CanJump whether
/// you can: not in a fight or a scene.</para>
/// <para>The world side is the old map screen's, with zones for maps: a zone's on it once any of its
/// rooms has been walked into, and its waypoint once any of its rooms' has been found. Inside a
/// building, which isn't on the world map, where you are is the zone you came in from.</para>
/// </remarks>
public class ZoneMapScreen : GameScreen
{
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.2);

    /// <summary>The panel's size, in screen pixels, the same as the map screen's.</summary>
    private const int PanelWidth = 1380;
    private const int PanelHeight = 860;

    /// <summary>The space between the two tab names at the top, and between them and what's under them.</summary>
    private const float TabGap = 90f;
    private const float AreaGap = 18f;

    /// <summary>How big a thumbnail is, in its own pixels. Has to match the thumbnails tools/rooms/generate_room_placeholders.py writes; one that isn't is stretched to it.</summary>
    private static readonly Point ThumbSize = new(200, 113);

    /// <summary>The biggest a thumbnail's drawn, and the room between two, across and down (under the rows of writing under each), in screen pixels. Narrow enough across for a zone of three rooms in a row, like the docks, to fit at 2x.</summary>
    private const float ThumbMaxScale = 2f;
    private static readonly Point RoomGap = new(40, 22);

    /// <summary>How thick the edge round a thumbnail is, in screen pixels.</summary>
    private const int FrameWidth = 3;

    /// <summary>The gap between a thumbnail and the writing under it, and between its badges and its bottom edge.</summary>
    private const float NameGap = 6f;
    private const float BadgeInset = 2f;

    /// <summary>One mark on map-marks.png, in art pixels, and how much they're blown up. Has to match MARK in tools/generate_ui.py.</summary>
    private const int MarkSize = 12;
    private const float MarkScale = 3f;

    /// <summary>One cell of markers.png, and how wide the "!" in it is, in art pixels; and how much a room's "!" is blown up. Have to match MARKER and BANG_WIDTH.</summary>
    private const int MarkerCell = 14;
    private const int BangWidth = 8;
    private const float MarkerScale = 2f;

    /// <summary>One cell of indicators.png, and the rows and column of it used here. Have to match INDICATOR and the columns in tools/generate_ui.py.</summary>
    private const int BadgeCell = 18;
    private const int BadgeRow = 0;
    private const int LockedBadgeRow = 1;
    private const int PadlockColumn = 13;

    /// <summary>How a room's "!" bobs: how high, in art pixels, and how fast.</summary>
    private const float BobHeight = 2f;
    private const float BobRate = 2.4f;

    /// <summary>The world map's roads: how far apart the dots are, and how far short of each place they stop, in screen pixels. The same dots join the rooms.</summary>
    private const float DotSpacing = 14f;
    private const float RoadClear = 24f;

    /// <summary>How close the mouse has to be to a place on the world map to pick it.</summary>
    private const float PickReach = 30f;

    /// <summary>Where a place's name goes under it, and where its waypoint mark sits over it.</summary>
    private const float NameDrop = 22f;
    private static readonly Vector2 WaypointOver = new(0f, -34f);

    /// <summary>What a room you haven't been in is multiplied by, and the plate a room without a thumbnail is.</summary>
    private static readonly Color Unvisited = new(58, 58, 74);
    private static readonly Color Blank = new(28, 32, 60);

    /// <summary>The edge round a thumbnail: yours, the one picked, and the rest.</summary>
    private static readonly Color OtherEdge = Color.Black * 0.8f;

    private const string WorldName = "THE WORLD";

    /// <summary>What a way back out the way you came in says under a room, when there's nowhere it's known to go.</summary>
    private const string OutLabel = "OUT";

    /// <summary>How the ways out of the zone are written under the room they leave from: off the west end, and off the east end.</summary>
    private const string WestLeaving = "< {0}";
    private const string EastLeaving = "{0} >";

    private const string JumpHint = "ARROWS  ·  CHOOSE      ENTER  ·  GO TO {0}      TAB  ·  THE WORLD      M  ·  CLOSE";
    private const string HereHint = "YOU'RE HERE      TAB  ·  THE WORLD      M  ·  CLOSE";
    private const string UnvisitedHint = "YOU HAVEN'T BEEN THERE YET      TAB  ·  THE WORLD      M  ·  CLOSE";
    private const string BusyHint = "NOT IN THE MIDDLE OF THIS      TAB  ·  THE WORLD      M  ·  CLOSE";
    private const string WorldHint = "TAB  ·  {0}      M  ·  CLOSE";
    private const string TravelHint = "ARROWS  ·  CHOOSE      ENTER  ·  TRAVEL TO {0}      ESC  ·  STAY";
    private const string NowhereHint = "THERE'S NO OTHER WAYPOINT FOUND YET      ESC  ·  CLOSE";

    private readonly InputAction _close = new(new[] { Buttons.B, Buttons.Y, Buttons.Back }, new[] { Keys.Escape, Keys.M }, true);
    private readonly InputAction _switch = new(new[] { Buttons.LeftShoulder, Buttons.RightShoulder }, new[] { Keys.Tab }, true);
    private readonly InputAction _previous = new(new[] { Buttons.DPadLeft, Buttons.DPadUp, Buttons.LeftThumbstickLeft }, new[] { Keys.Left, Keys.Up, Keys.A, Keys.W }, true);
    private readonly InputAction _next = new(new[] { Buttons.DPadRight, Buttons.DPadDown, Buttons.LeftThumbstickRight }, new[] { Keys.Right, Keys.Down, Keys.D, Keys.S }, true);
    private readonly InputAction _go = new(new[] { Buttons.A }, new[] { Keys.Enter, Keys.Space, Keys.E }, true);

    private readonly SaveData _game;
    private readonly ZoneDefinition _zone;
    private readonly RoomDefinition _here;
    private readonly bool _travelling;
    private readonly List<(RoomDefinition Room, Point Cell)> _rooms;
    private readonly List<(int From, int To)> _joins = new();
    private readonly List<ZoneDefinition> _destinations = new();
    private readonly ZoneDefinition _place;
    private readonly FormPanel _panel = new();

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Texture2D _marks;
    private Texture2D _markers;
    private Texture2D _indicators;
    private Texture2D _pixel;
    private Texture2D[] _thumbs;

    private Rectangle _panelBounds;
    private Rectangle _area;
    private Rectangle _hereTab;
    private Rectangle _worldTab;
    private Rectangle[] _thumbBounds;
    private float _thumbScale;
    private MapView _view;
    private int _selectedRoom;
    private int _selected;

    /// <summary>Raised when a room is picked to jump to, with its id in this zone. Whoever opened the map does the jumping.</summary>
    public event Action<string> JumpTo;

    /// <summary>Raised when a place is picked to travel to, with its zone's id. Whoever opened the map does the travelling, to its waypoint.</summary>
    public event Action<string> TravelTo;

    /// <summary>Whether a room can be jumped to: not in a fight, or while a scene's playing. Yes unless whoever opens it says not.</summary>
    public bool CanJump { get; init; } = true;

    /// <summary>Opens the zone map on the zone's rooms, or on the world if you're travelling.</summary>
    /// <param name="game">The save, for the rooms you've been in, the waypoints you've found and the markers.</param>
    /// <param name="zone">The zone you're in.</param>
    /// <param name="room">The room of it you're in.</param>
    /// <param name="travelling">True if this was opened from a waypoint, so you can pick somewhere to go.</param>
    public ZoneMapScreen(SaveData game, string zone, string room, bool travelling)
    {
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _game = game;
        _zone = ZoneCatalog.Get(zone);
        _here = _zone.Room(room);
        _travelling = travelling;
        _rooms = Arrange(_zone);
        _selectedRoom = _rooms.FindIndex(placed => placed.Room == _here);
        _view = travelling ? MapView.World : MapView.Here;

        // Each join once, from the room earlier in the list.
        for (int i = 0; i < _rooms.Count; i++)
        {
            foreach ((string to, _) in JoinsOf(_zone, _rooms[i].Room))
            {
                int j = _rooms.FindIndex(placed => placed.Room.Id == to);
                if (!_joins.Contains((Math.Min(i, j), Math.Max(i, j)))) _joins.Add((Math.Min(i, j), Math.Max(i, j)));
            }
        }

        if (travelling)
        {
            foreach (ZoneDefinition other in ZoneCatalog.All)
                if (other != _zone && other.World is not null && HasWaypoint(other)) _destinations.Add(other);
        }

        ZoneDefinition back = game.BackZone is string id && ZoneCatalog.Contains(id) ? ZoneCatalog.Get(id) : null;
        _place = _zone.World is not null ? _zone : back?.World is not null ? back : null;
    }

    /// <summary>Loads the panel, the fonts, the marks and every room's thumbnail, and lays it all out.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();

        _panel.LoadContent(_content, ScreenManager.GraphicsDevice);
        _font = _content.Load<SpriteFont>("spectral-ui");
        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        _marks = _content.Load<Texture2D>("map-marks");
        _markers = _content.Load<Texture2D>("markers");
        _indicators = _content.Load<Texture2D>("indicators");
        _thumbs = _rooms.ConvertAll(placed => Thumbnail(placed.Room)).ToArray();

        _pixel = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        LayOut();
        _audio.Play(Sfx.MenuConfirm);
    }

    /// <summary>Unloads everything, and disposes the pixel, since the content manager didn't make it.</summary>
    public override void Unload()
    {
        _pixel?.Dispose();
        _content?.Unload();
    }

    /// <summary>Closing, switching sides, picking a room and jumping to it, and on the world side, picking somewhere and going there.</summary>
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

        if (_view == MapView.Here) HandleRooms(input);
        else HandleWorld(input);
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

        if (_view == MapView.Here) DrawRooms(gameTime, spriteBatch, alpha);
        else DrawWorld(spriteBatch, alpha);

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        DrawTab(spriteBatch, _zone.Name.ToUpperInvariant(), _hereTab, _view == MapView.Here, alpha);
        DrawTab(spriteBatch, WorldName, _worldTab, _view == MapView.World, alpha);
        Text.Draw(spriteBatch, _detailFont, HintLine(), _panelBounds.Center.X, _panelBounds.Bottom - FormPanel.Padding - _detailFont.LineSpacing,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        spriteBatch.End();
    }

    /// <summary>For the proof run, since it can't press keys: jumps to a room.</summary>
    /// <param name="room">The room's id in this zone. It has to be one you've been in.</param>
    internal void ProofJump(string room)
    {
        _selectedRoom = _rooms.FindIndex(placed => placed.Room.Id == room);
        Jump();
    }

    /// <summary>For the proof run: picks a place by its zone's id and travels there.</summary>
    /// <param name="zone">The zone to go to. It has to be one of the places you can go.</param>
    internal void ProofTravel(string zone)
    {
        _selected = _destinations.FindIndex(place => place.Id == zone);
        Go();
    }

    /// <summary>For the proof run: shows the world side.</summary>
    internal void ProofWorldTab() => _view = MapView.World;

    /// <summary>Picking a room, by the mouse or the arrows, and jumping to it with a click or Enter if you can.</summary>
    private void HandleRooms(InputState input)
    {
        int was = _selectedRoom;
        int? pointedAt = RoomAt(input.MousePosition);
        if (input.MouseMoved && pointedAt is int hovered) _selectedRoom = hovered;
        if (_previous.Occurred(input)) _selectedRoom = (_selectedRoom + _rooms.Count - 1) % _rooms.Count;
        if (_next.Occurred(input)) _selectedRoom = (_selectedRoom + 1) % _rooms.Count;
        if (_selectedRoom != was) _audio.Play(Sfx.MenuMove);

        if ((_go.Occurred(input) || input.IsNewLeftClick && pointedAt == _selectedRoom) && CanGoTo(_selectedRoom)) Jump();
    }

    /// <summary>Picking a place to travel to, by the mouse or the arrows, and going there, when it was opened from a waypoint.</summary>
    private void HandleWorld(InputState input)
    {
        if (_destinations.Count == 0) return;

        int was = _selected;
        int? pointedAt = DestinationAt(input.MousePosition);
        if (input.MouseMoved && pointedAt is int hovered) _selected = hovered;
        if (_previous.Occurred(input)) _selected = (_selected + _destinations.Count - 1) % _destinations.Count;
        if (_next.Occurred(input)) _selected = (_selected + 1) % _destinations.Count;
        if (_selected != was) _audio.Play(Sfx.MenuMove);

        if (_go.Occurred(input) || input.IsNewLeftClick && pointedAt == _selected) Go();
    }

    /// <summary>Closes the map and says which room to jump to.</summary>
    private void Jump()
    {
        _audio.Play(Sfx.MenuConfirm);
        ExitScreen();
        JumpTo?.Invoke(_rooms[_selectedRoom].Room.Id);
    }

    /// <summary>Closes the map and says where to travel to.</summary>
    private void Go()
    {
        _audio.Play(Sfx.MenuConfirm);
        ExitScreen();
        TravelTo?.Invoke(_destinations[_selected].Id);
    }

    /// <summary>The line along the bottom: what the keys do on this side, and for the room that's picked, whether you can go there.</summary>
    private string HintLine()
    {
        if (_view == MapView.World)
        {
            if (!_travelling) return string.Format(CultureInfo.InvariantCulture, WorldHint, _zone.Name.ToUpperInvariant());
            if (_destinations.Count == 0) return NowhereHint;
            return string.Format(CultureInfo.InvariantCulture, TravelHint, _destinations[_selected].Name.ToUpperInvariant());
        }

        RoomDefinition picked = _rooms[_selectedRoom].Room;
        if (picked == _here) return HereHint;
        if (!Visited(picked)) return UnvisitedHint;
        if (!CanJump) return BusyHint;
        return string.Format(CultureInfo.InvariantCulture, JumpHint, picked.Name.ToUpperInvariant());
    }

    /// <summary>Draws the zone's rooms: the thumbnails, the dots joining them, the edges round them, their badges and "!"s, you, and their names and the ways out of the zone under them.</summary>
    private void DrawRooms(GameTime gameTime, SpriteBatch spriteBatch, float alpha)
    {
        float time = (float)gameTime.TotalGameTime.TotalSeconds;
        bool whole = _thumbScale == MathF.Floor(_thumbScale);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, whole ? SamplerState.PointClamp : SamplerState.LinearClamp);
        for (int i = 0; i < _rooms.Count; i++)
        {
            Color tint = Visited(_rooms[i].Room) ? Color.White : Unvisited;
            if (_thumbs[i] is Texture2D thumb) spriteBatch.Draw(thumb, _thumbBounds[i], tint * alpha);
            else spriteBatch.Draw(_pixel, _thumbBounds[i], Blank * alpha);
        }
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        foreach ((int from, int to) in _joins) DrawJoin(spriteBatch, from, to, alpha);
        for (int i = 0; i < _rooms.Count; i++)
        {
            RoomDefinition room = _rooms[i].Room;
            Rectangle thumb = _thumbBounds[i];
            Color edge = room == _here ? Palette.Torch : i == _selectedRoom ? Palette.Bone : OtherEdge;
            DrawFrame(spriteBatch, thumb, edge * alpha);

            if (Visited(room)) DrawBadges(spriteBatch, room, thumb, alpha);
            MarkerKind marker = RoomMarkers.In(_zone.Id, room.Id, _game);
            if (marker != MarkerKind.None)
            {
                float bob = MathF.Round((0.5f - 0.5f * MathF.Cos(time * BobRate + i)) * BobHeight * MarkerScale);
                var foot = new Vector2(thumb.Right - BangWidth * MarkerScale, thumb.Top + MarkerCell * MarkerScale + FrameWidth - bob);
                DrawBang(spriteBatch, marker, foot, MarkerScale, alpha);
            }
            if (room == _here) DrawMark(spriteBatch, MapMark.You, thumb.Center.ToVector2(), alpha);
        }
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        for (int i = 0; i < _rooms.Count; i++)
        {
            RoomDefinition room = _rooms[i].Room;
            Rectangle thumb = _thumbBounds[i];
            Color colour = i == _selectedRoom ? Palette.Torch : room == _here ? Palette.Heading : Visited(room) ? Palette.Bone : Palette.Note;
            float y = thumb.Bottom + FrameWidth + NameGap;
            Text.Draw(spriteBatch, _detailFont, room.Name.ToUpperInvariant(), thumb.Center.X, y, colour * alpha, Palette.FormShadow * alpha, centred: true);

            foreach (RoomExit exit in room.Exits)
            {
                if (exit.LockedWhy is not null || exit.ToZone == _zone.Id || exit.Side is ExitSide.Up or ExitSide.Down || !exit.Presence.Holds(_game.Flags)) continue;

                bool west = exit.Side == ExitSide.West;
                string text = string.Format(CultureInfo.InvariantCulture, west ? WestLeaving : EastLeaving, LeavingTo(exit).ToUpperInvariant());
                Text.Draw(spriteBatch, _detailFont, text, west ? thumb.Left : thumb.Right, y + _detailFont.LineSpacing,
                    Palette.Note * alpha, Palette.FormShadow * alpha, rightAligned: !west);
            }
        }
        spriteBatch.End();
    }

    /// <summary>Draws a room's badges along the bottom of its thumbnail, each where its door is along the room: the plain doors only when there's a "!" through them, the locked greyed with a padlock, and the "!" of the room through each.</summary>
    private void DrawBadges(SpriteBatch spriteBatch, RoomDefinition room, Rectangle thumb, float alpha)
    {
        float scale = MathF.Max(1f, MathF.Floor(_thumbScale));
        float size = BadgeCell * scale;
        foreach (RoomDoor door in room.Doors)
        {
            if (!door.Presence.Holds(_game.Flags)) continue;

            bool locked = door.LockedWhy is not null;
            bool leads = door.ToZone is not null && door.ToZone != RoomExit.Back;
            MarkerKind behind = leads ? RoomMarkers.In(door.ToZone, door.ToRoom, _game) : MarkerKind.None;
            if (door.Badge == BadgeKind.Door && behind == MarkerKind.None) continue;

            var at = new Vector2(MathF.Round(thumb.Left + door.Foot.X / room.Width * thumb.Width - size / 2f), MathF.Round(thumb.Bottom - BadgeInset - size));
            var source = new Rectangle((int)door.Badge * BadgeCell, (locked ? LockedBadgeRow : BadgeRow) * BadgeCell, BadgeCell, BadgeCell);
            spriteBatch.Draw(_indicators, at, source, Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            if (locked)
            {
                var padlock = new Rectangle(PadlockColumn * BadgeCell, BadgeRow * BadgeCell, BadgeCell, BadgeCell);
                spriteBatch.Draw(_indicators, at, padlock, Color.White * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            if (behind != MarkerKind.None) DrawBang(spriteBatch, behind, at + new Vector2(size * 0.8f, size * 0.3f), scale, alpha);
        }
    }

    /// <summary>Draws the dots between two rooms that join, from middle to middle, leaving out any that would sit on a room or the writing under it.</summary>
    private void DrawJoin(SpriteBatch spriteBatch, int from, int to, float alpha)
    {
        Vector2 start = _thumbBounds[from].Center.ToVector2(), end = _thumbBounds[to].Center.ToVector2();
        float length = Vector2.Distance(start, end);
        Vector2 step = (end - start) / length;

        for (float along = DotSpacing / 2f; along < length; along += DotSpacing)
        {
            Vector2 dot = start + step * along;
            bool covered = false;
            for (int i = 0; i < _rooms.Count && !covered; i++) covered = Footprint(i).Contains(dot);
            if (!covered) DrawMark(spriteBatch, MapMark.Dot, dot, alpha);
        }
    }

    /// <summary>Draws the world: a mark for every place you've been, the roads between them, the ones you know of but haven't been to, the waypoints, and where you are.</summary>
    private void DrawWorld(SpriteBatch spriteBatch, float alpha)
    {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        foreach (ZoneDefinition zone in ZoneCatalog.All)
        {
            if (!Discovered(zone) || zone.World is null) continue;

            // Each road once: from a place to anywhere it leads that isn't discovered, or that comes later in the list.
            foreach (string id in ZoneCatalog.Links(zone.Id))
            {
                ZoneDefinition other = ZoneCatalog.Get(id);
                if (!Discovered(other) || string.CompareOrdinal(zone.Id, other.Id) < 0)
                    DrawRoad(spriteBatch, PlaceOf(zone), PlaceOf(other), alpha);
                if (!Discovered(other)) DrawMark(spriteBatch, MapMark.Unknown, PlaceOf(other), alpha);
            }

            DrawMark(spriteBatch, MapMark.Place, PlaceOf(zone), alpha);
            if (zone == _place) DrawMark(spriteBatch, MapMark.Here, PlaceOf(zone), alpha);
            if (HasWaypoint(zone)) DrawMark(spriteBatch, MapMark.Waypoint, PlaceOf(zone) + WaypointOver, alpha);
        }
        if (_destinations.Count > 0) DrawMark(spriteBatch, MapMark.Selected, PlaceOf(_destinations[_selected]), alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        foreach (ZoneDefinition zone in ZoneCatalog.All)
        {
            if (!Discovered(zone) || zone.World is null) continue;

            bool picked = _destinations.Count > 0 && _destinations[_selected] == zone;
            Color colour = picked ? Palette.Torch : zone == _place ? Palette.Heading : Palette.Bone;
            Vector2 at = PlaceOf(zone);
            Text.Draw(spriteBatch, _detailFont, zone.Name.ToUpperInvariant(), at.X, at.Y + NameDrop,
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
            DrawMark(spriteBatch, MapMark.Dot, from + step * (RoadClear + i * DotSpacing), alpha);
    }

    /// <summary>Draws one mark from map-marks.png, centred on a point.</summary>
    private void DrawMark(SpriteBatch spriteBatch, MapMark mark, Vector2 centre, float alpha)
    {
        var source = new Rectangle((int)mark * MarkSize, 0, MarkSize, MarkSize);
        var at = new Vector2(MathF.Round(centre.X), MathF.Round(centre.Y));
        spriteBatch.Draw(_marks, at, source, Color.White * alpha, 0f, new Vector2(MarkSize / 2f), MarkScale, SpriteEffects.None, 0f);
    }

    /// <summary>Draws one "!" from markers.png standing on a point.</summary>
    private void DrawBang(SpriteBatch spriteBatch, MarkerKind kind, Vector2 foot, float scale, float alpha)
    {
        var source = new Rectangle(0, ((int)kind - 1) * MarkerCell, MarkerCell, MarkerCell);
        var at = new Vector2(MathF.Round(foot.X), MathF.Round(foot.Y));
        spriteBatch.Draw(_markers, at, source, Color.White * alpha, 0f, new Vector2(MarkerCell / 2f, MarkerCell), scale, SpriteEffects.None, 0f);
    }

    /// <summary>Draws the edge round a thumbnail, just outside it.</summary>
    private void DrawFrame(SpriteBatch spriteBatch, Rectangle thumb, Color colour)
    {
        Rectangle outer = thumb;
        outer.Inflate(FrameWidth, FrameWidth);
        spriteBatch.Draw(_pixel, new Rectangle(outer.Left, outer.Top, outer.Width, FrameWidth), colour);
        spriteBatch.Draw(_pixel, new Rectangle(outer.Left, thumb.Bottom, outer.Width, FrameWidth), colour);
        spriteBatch.Draw(_pixel, new Rectangle(outer.Left, thumb.Top, FrameWidth, thumb.Height), colour);
        spriteBatch.Draw(_pixel, new Rectangle(thumb.Right, thumb.Top, FrameWidth, thumb.Height), colour);
    }

    /// <summary>Where a room is on screen with its edge and the two rows of writing under it, which the dots between rooms keep off.</summary>
    private Rectangle Footprint(int index)
    {
        Rectangle thumb = _thumbBounds[index];
        thumb.Inflate(FrameWidth * 2, FrameWidth * 2);
        thumb.Height += (int)(NameGap + _detailFont.LineSpacing * 2);
        return thumb;
    }

    /// <summary>Which room's thumbnail the mouse is over, or null.</summary>
    private int? RoomAt(Point mouse)
    {
        for (int i = 0; i < _thumbBounds.Length; i++)
            if (_thumbBounds[i].Contains(mouse)) return i;
        return null;
    }

    /// <summary>Which of the places you can go the mouse is over, or null.</summary>
    private int? DestinationAt(Point mouse)
    {
        for (int i = 0; i < _destinations.Count; i++)
            if (Vector2.Distance(mouse.ToVector2(), PlaceOf(_destinations[i])) <= PickReach) return i;
        return null;
    }

    /// <summary>Where a zone is on the world side, in screen pixels. Only for zones that are on it.</summary>
    private Vector2 PlaceOf(ZoneDefinition zone) => new(
        _area.Center.X - ZoneCatalog.WorldWidth / 2f + zone.World.Value.X,
        _area.Center.Y - ZoneCatalog.WorldHeight / 2f + zone.World.Value.Y);

    /// <summary>Whether you can jump to a room: one you've been in that isn't this one, when jumping's allowed.</summary>
    private bool CanGoTo(int index) => CanJump && _rooms[index].Room != _here && Visited(_rooms[index].Room);

    /// <summary>Whether you've been in a room of this zone. The one you're in counts, whatever the save says.</summary>
    private bool Visited(RoomDefinition room) => room == _here || _game.Visited.Contains(_zone.Id + "/" + room.Id);

    /// <summary>Whether any room of a zone has been walked into, which is what puts it on the world map.</summary>
    private bool Discovered(ZoneDefinition zone)
    {
        foreach (RoomDefinition room in zone.Rooms)
            if (_game.Visited.Contains(zone.Id + "/" + room.Id)) return true;
        return false;
    }

    /// <summary>Whether a zone's waypoint has been found, in whichever of its rooms it's in.</summary>
    private bool HasWaypoint(ZoneDefinition zone)
    {
        foreach (RoomDefinition room in zone.Rooms)
            foreach (MapInteractable thing in room.Things)
                if (thing.Kind == InteractableKind.Waypoint && _game.Waypoints.Contains(zone.Id + "/" + thing.Id)) return true;
        return false;
    }

    /// <summary>Where a way out of the zone goes, for the line under its room: the zone it leads to, or back where you came in from.</summary>
    private string LeavingTo(RoomExit exit)
    {
        if (exit.ToZone != RoomExit.Back) return ZoneCatalog.Get(exit.ToZone).Name;
        return _game.BackZone is string back && ZoneCatalog.Contains(back) ? ZoneCatalog.Get(back).Name : OutLabel;
    }

    /// <summary>A room's thumbnail, or null if it hasn't got one.</summary>
    private Texture2D Thumbnail(RoomDefinition room)
    {
        try
        {
            return _content.Load<Texture2D>($"Rooms/{room.Paint}/thumb");
        }
        catch (ContentLoadException)
        {
            return null;
        }
    }

    /// <summary>Centres the panel, works out where the tabs, the area and the hint go inside it, and fits the rooms into the area.</summary>
    private void LayOut()
    {
        _panelBounds = new Rectangle(
            (LegendQuestGame.ScreenWidth - PanelWidth) / 2,
            (LegendQuestGame.ScreenHeight - PanelHeight) / 2,
            PanelWidth,
            PanelHeight);

        int tabsTop = _panelBounds.Top + FormPanel.Padding;
        Vector2 here = _font.MeasureString(_zone.Name.ToUpperInvariant());
        Vector2 world = _font.MeasureString(WorldName);
        float left = _panelBounds.Center.X - (here.X + TabGap + world.X) / 2f;

        _hereTab = new Rectangle((int)left, tabsTop, (int)here.X, _font.LineSpacing);
        _worldTab = new Rectangle((int)(left + here.X + TabGap), tabsTop, (int)world.X, _font.LineSpacing);

        int areaTop = (int)(tabsTop + _font.LineSpacing + AreaGap);
        int areaBottom = (int)(_panelBounds.Bottom - FormPanel.Padding - _detailFont.LineSpacing - AreaGap);
        _area = new Rectangle(_panelBounds.Left + FormPanel.Padding, areaTop, PanelWidth - FormPanel.Padding * 2, areaBottom - areaTop);

        LayOutRooms();
    }

    /// <summary>Works out how big the thumbnails go, the biggest whole scale up to ThumbMaxScale that fits the area or as small as it takes, and where each goes, the grid centred in the area.</summary>
    private void LayOutRooms()
    {
        int columns = 0, rows = 0;
        foreach ((_, Point cell) in _rooms)
        {
            columns = Math.Max(columns, cell.X + 1);
            rows = Math.Max(rows, cell.Y + 1);
        }

        float writing = FrameWidth + NameGap + _detailFont.LineSpacing * 2f;
        float spareWidth = _area.Width - (columns - 1) * RoomGap.X - columns * FrameWidth * 2f;
        float spareHeight = _area.Height - (rows - 1) * RoomGap.Y - rows * (writing + FrameWidth);
        float fit = MathF.Min(spareWidth / (columns * ThumbSize.X), spareHeight / (rows * ThumbSize.Y));
        _thumbScale = fit >= 1f ? MathF.Min(MathF.Floor(fit), ThumbMaxScale) : fit;

        int width = (int)(ThumbSize.X * _thumbScale), height = (int)(ThumbSize.Y * _thumbScale);
        int stepX = width + FrameWidth * 2 + RoomGap.X, stepY = (int)(height + FrameWidth + writing + RoomGap.Y);
        int gridWidth = columns * stepX - RoomGap.X, gridHeight = rows * stepY - RoomGap.Y;
        int originX = _area.Center.X - gridWidth / 2 + FrameWidth, originY = _area.Center.Y - gridHeight / 2 + FrameWidth;

        _thumbBounds = new Rectangle[_rooms.Count];
        for (int i = 0; i < _rooms.Count; i++)
        {
            Point cell = _rooms[i].Cell;
            _thumbBounds[i] = new Rectangle(originX + cell.X * stepX, originY + cell.Y * stepY, width, height);
        }
    }

    /// <summary>Places every room of a zone on a grid by how they join, starting from the first, and lists them along the rows, top to bottom.</summary>
    private static List<(RoomDefinition Room, Point Cell)> Arrange(ZoneDefinition zone)
    {
        var cells = new Dictionary<RoomDefinition, Point>();
        var taken = new HashSet<Point>();
        var queue = new Queue<RoomDefinition>();

        void Put(RoomDefinition room, Point cell)
        {
            cells[room] = cell;
            taken.Add(cell);
            queue.Enqueue(room);
        }

        // The first free cell along a row: on the way it was going for an end, or either side in turn for anything else.
        Point Free(Point wanted, int along)
        {
            for (int i = 0; ; i++)
            {
                int offset = along != 0 ? along * i : (i + 1) / 2 * (i % 2 == 1 ? 1 : -1);
                var cell = new Point(wanted.X + offset, wanted.Y);
                if (!taken.Contains(cell)) return cell;
            }
        }

        foreach (RoomDefinition start in zone.Rooms)
        {
            if (cells.ContainsKey(start)) continue;

            // The first room starts the grid, and any nothing joins to starts a row under everything so far.
            int row = 0;
            foreach (Point cell in taken) row = Math.Max(row, cell.Y + 1);
            Put(start, new Point(0, row));

            while (queue.Count > 0)
            {
                RoomDefinition room = queue.Dequeue();
                foreach ((string to, Point step) in JoinsOf(zone, room))
                {
                    RoomDefinition next = zone.Room(to);
                    if (cells.ContainsKey(next)) continue;

                    Point from = cells[room];
                    Put(next, Free(new Point(from.X + step.X, from.Y + step.Y), step.Y == 0 ? step.X : 0));
                }
            }
        }

        // The grid starts at 0, 0, whichever way the rooms went from the first.
        int left = int.MaxValue, top = int.MaxValue;
        foreach (Point cell in cells.Values)
        {
            left = Math.Min(left, cell.X);
            top = Math.Min(top, cell.Y);
        }

        var placed = new List<(RoomDefinition Room, Point Cell)>();
        foreach ((RoomDefinition room, Point cell) in cells) placed.Add((room, new Point(cell.X - left, cell.Y - top)));
        placed.Sort((a, b) => a.Cell.Y != b.Cell.Y ? a.Cell.Y.CompareTo(b.Cell.Y) : a.Cell.X.CompareTo(b.Cell.X));
        return placed;
    }

    /// <summary>Where a room's ways out and doors go to in its own zone, and which way each is on the grid: an end's along the row, a stair's a row up or down, and a door's a row down.</summary>
    private static IEnumerable<(string To, Point Step)> JoinsOf(ZoneDefinition zone, RoomDefinition room)
    {
        foreach (RoomExit exit in room.Exits)
        {
            if (exit.ToZone != zone.Id) continue;
            yield return (exit.ToRoom, exit.Side switch
            {
                ExitSide.West => new Point(-1, 0),
                ExitSide.East => new Point(1, 0),
                ExitSide.Up => new Point(0, -1),
                _ => new Point(0, 1),
            });
        }
        foreach (RoomDoor door in room.Doors)
            if (door.ToZone == zone.Id) yield return (door.ToRoom, new Point(0, 1));
    }
}
