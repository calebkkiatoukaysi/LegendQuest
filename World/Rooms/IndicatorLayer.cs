using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Collisions;
using LegendQuest.Components;

namespace LegendQuest.World;

/// <summary>
/// The marks that make a room readable (12-rooms §2): a badge and a plaque over every door, chevrons
/// at the ends of the floor and on the stairs with a label saying where they go, names over heads, a
/// glint on everything still to be used, and with Alt held, all of it at once.
/// </summary>
/// <remarks>
/// <para>A badge floats BadgeLift over its doorway: always for a shop, an inn, a trainer or a story
/// place, and for a plain door only with the mouse on it, with Alt, or when someone through it has
/// something for you. A locked door or way out is greyed, with a padlock, and says why with the mouse
/// on it. A small "!" over a badge, or beside an end's chevrons, is the marker of the room through it.
/// An end's label shows with the mouse on it or once you're within LabelReach of it, with a compass if
/// it leaves for somewhere else on the world map; a stair's only with the mouse on it. A townsperson's
/// name only shows with the mouse on them.</para>
/// <para>It's drawn in screen space over the room and its dark, so it all shows in the dark too. The
/// camera's locked at ZoneCatalog.RoomScale in a room, so a canvas pixel is a screen pixel: the sizes
/// here are both, and DoorAt and ChevronAt divide them by RoomScale to test in world pixels. The art is
/// indicators.png and markers.png, point sampled at 3x like the rest of the UI (2x for an icon in a
/// line of writing), and the writing is linear sampled over the soft smudge the lines over people's
/// heads have. So Draw lays everything out first, then draws the smudges, the art and the writing, each
/// in a batch of its own.</para>
/// </remarks>
public sealed class IndicatorLayer
{
    /// <summary>One cell of indicators.png, and where everything is on it. Have to match INDICATOR and the columns in tools/generate_ui.py.</summary>
    private const int Cell = 18;
    private const int BadgeRow = 0;
    private const int LockedBadgeRow = 1;
    private const int ChevronRow = 2;
    private const int LockedChevronRow = 3;
    private const int PadlockColumn = 13;
    private const int CompassColumn = 14;
    private const int GlintColumn = 4;
    private const int GlintFrames = 4;
    private const int PlaqueColumn = 8;
    private const int OutlineColumn = 9;

    /// <summary>The plaque's and the doorway outline's nine-slices, each in the top left of its cell. Have to match PLAQUE and PLAQUE_CORNER.</summary>
    private const int PlaqueFrame = 12;
    private const int PlaqueCorner = 4;

    /// <summary>One cell of markers.png, and how wide the "!" in it is. Have to match MARKER and BANG_WIDTH.</summary>
    private const int MarkerCell = 14;
    private const int BangWidth = 8;

    /// <summary>How far the art's blown up: as far as the rest of the UI, and less for an icon in a line of writing.</summary>
    private const float Scale = 3f;
    private const float InlineScale = 2f;

    /// <summary>A badge and its plaque: how far over the doorway the plaque's bottom floats, how tall the plaque is, how much room it leaves either side of the name, how big the badge is, and how far it sits down onto the plaque's top edge.</summary>
    private const int BadgeLift = 30;
    private const int PlaqueHeight = (int)(PlaqueFrame * Scale);
    private const int PlaqueRoom = 18;
    private const int BadgeSize = (int)(Cell * Scale);
    private const int BadgeOverlap = 6;

    /// <summary>The chevrons: how far in from an end the outer one stands and how far in from that the inner one; how far over the back of the floor a stair's lower one stands and how far over that its upper one.</summary>
    private const float EdgeInset = 40f;
    private const float EdgeSpacing = 40f;
    private const float StairLift = 36f;
    private const float StairSpacing = 34f;

    /// <summary>How the chevrons pulse toward the way out: how fast, how far behind the inner one the outer one is, how faint they get, and how far they nudge the way they point.</summary>
    private const float PulseRate = 3f;
    private const float PulseLag = 1.1f;
    private const float PulseFloor = 0.45f;
    private const float PulseNudge = 4f;

    /// <summary>How near an end you have to be for its label to show without the mouse on it: 300 canvas pixels, in world pixels.</summary>
    private const float LabelReach = 300f / ZoneCatalog.RoomScale;

    /// <summary>What counts as pointing at a way out: for an end, how far in from it and how far over the back of the floor (which takes in its label); for a stair, how far either side of its foot, and how far over and under the back of the floor.</summary>
    private const float EdgeHitWidth = 160f;
    private const float EdgeHitAbove = 100f;
    private const float StairHitHalf = 80f;
    private const float StairHitAbove = 130f;
    private const float StairHitBelow = 48f;

    /// <summary>The writing: how far in from the end of the room an end's label stops, the gap between a label and what it's over, how wide a locked way's reason gets before it wraps, the gap between an icon and its words, and the gap under a name plate.</summary>
    private const float LabelInset = 16f;
    private const float LabelGap = 10f;
    private const float WhyWidth = 340f;
    private const float IconGap = 6f;
    private const float PlateGap = 6f;

    /// <summary>How much room a name plate takes over a head, in screen pixels. Anything else that goes over a head, the markers and the lines people say, goes this much higher to clear it.</summary>
    public const int PlateRoom = 48;

    /// <summary>The figure under the top of someone's head that the mouse has to be on to show a townsperson's name, in world pixels: how far either side, and how far down.</summary>
    private static readonly Vector2 FigureReach = new(8f, 44f);

    /// <summary>How a glint twinkles: once every so many seconds, each of its frames this long, out of step with the next by this much for every world pixel along, and the frame it holds while Alt is held.</summary>
    private const float GlintCycle = 3f;
    private const float GlintFrameTime = 0.11f;
    private const float GlintStagger = 0.37f;
    private const int GlintFull = 2;

    /// <summary>How the small "!"s bob, as the markers over heads do: how high, in art pixels, how fast, and how far out of step one is with the next for every world pixel between them.</summary>
    private const float BobHeight = 2f;
    private const float BobRate = 2.4f;
    private const float BobStagger = 0.05f;

    /// <summary>What an end's label says when it opens the world map, when it goes back out the way you came in, and when it's locked and the zone file gives it no label.</summary>
    private const string TravelLabel = "Travel";
    private const string OutLabel = "Out";
    private const string LockedLabel = "No way through";

    /// <summary>The name plates' blue, for anyone you can buy from or train with, and red, for anyone with an event.</summary>
    private static readonly Color PlateBlue = new(156, 198, 248);
    private static readonly Color PlateRed = new(246, 134, 124);

    /// <summary>The light the outline round a doorway under the mouse is drawn in.</summary>
    private static readonly Color OutlineLight = new(255, 226, 170);

    /// <summary>The smudge under the writing, and how far it spreads past it.</summary>
    private static readonly Color Backing = Color.Black * 0.6f;
    private static readonly Vector2 BackingSpread = new(14f, 6f);

    /// <summary>This frame's layout: the smudges, the nine-sliced plaques and outlines, the rest of the art, and the writing.</summary>
    private readonly List<Rectangle> _backings = new();
    private readonly List<(Rectangle Destination, int Column, int Row, Color Tint)> _slices = new();
    private readonly List<(Texture2D Sheet, Rectangle Source, Vector2 At, float Scale, Color Tint)> _sprites = new();
    private readonly List<(string Text, Vector2 At, Color Colour)> _lines = new();

    private Texture2D _sheet;
    private Texture2D _markers;
    private Texture2D _glow;
    private SpriteFont _font;

    /// <summary>Loads the art, the glow the smudge is made of, and the font.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _sheet = content.Load<Texture2D>("indicators");
        _markers = content.Load<Texture2D>("markers");
        _glow = content.Load<Texture2D>("glow");
        _font = content.Load<SpriteFont>("spectral-detail");
    }

    /// <summary>Draws everything for a room. Begins and ends its own batches, in screen space, so call it between batches.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="camera">The room's camera, for where everything is on screen.</param>
    /// <param name="frame">The room, and what's in it this frame.</param>
    /// <param name="gameTime">The frame's timing, for the pulse, the glints and the bob.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void Draw(SpriteBatch spriteBatch, Camera2D camera, IndicatorFrame frame, GameTime gameTime, float alpha)
    {
        _backings.Clear();
        _slices.Clear();
        _sprites.Clear();
        _lines.Clear();
        float time = (float)gameTime.TotalGameTime.TotalSeconds;

        RoomExit pointedExit = ChevronAt(frame.Room, frame.Flags, frame.Mouse);
        RoomDoor pointedDoor = DoorAt(frame.Room, frame.Flags, frame.Mouse);
        foreach (Vector2 at in frame.Glints) LayOutGlint(camera, at, frame.ShowAll, time);
        foreach (RoomExit exit in frame.Room.Exits)
            if (exit.Presence.Holds(frame.Flags)) LayOutExit(camera, frame, exit, exit == pointedExit, time);
        foreach (RoomDoor door in frame.Room.Doors)
            if (door.Presence.Holds(frame.Flags)) LayOutDoor(camera, frame, door, door == pointedDoor, time);
        foreach ((string Name, Vector2 Head, MarkerKind Marker, BadgeKind? Badge, bool Named) plate in frame.Plates)
            LayOutPlate(camera, frame, plate);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        foreach (Rectangle area in _backings) Text.DrawBacking(spriteBatch, _glow, area, (int)BackingSpread.X, Backing * alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        foreach ((Rectangle destination, int column, int row, Color tint) in _slices)
            NineSlice.Draw(spriteBatch, _sheet, destination, new Point(column * Cell, row * Cell), PlaqueFrame, PlaqueCorner, Scale, tint * alpha, 0f);
        foreach ((Texture2D sheet, Rectangle source, Vector2 at, float scale, Color tint) in _sprites)
            spriteBatch.Draw(sheet, at, source, tint * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        foreach ((string text, Vector2 at, Color colour) in _lines)
            Text.Draw(spriteBatch, _font, text, at.X, at.Y, colour * alpha, Palette.Shadow * alpha);
        spriteBatch.End();
    }

    /// <summary>The door the mouse is on: its doorway, or its badge and plaque and the gap down to the doorway, so the mouse can go from one to the other. Only doors that are there count, locked ones included. Needs LoadContent first, since the plaque's as wide as its name.</summary>
    /// <param name="room">The room.</param>
    /// <param name="flags">The game's flags, for which doors are there.</param>
    /// <param name="world">The point, in world pixels.</param>
    /// <returns>The door, or null.</returns>
    public RoomDoor DoorAt(RoomDefinition room, ISet<string> flags, Vector2 world)
    {
        const float toWorld = 1f / ZoneCatalog.RoomScale;
        foreach (RoomDoor door in room.Doors)
        {
            if (!door.Presence.Holds(flags)) continue;
            if (Inside(door.Area, world)) return door;

            (Rectangle plaque, Rectangle badge) = BadgeLayout(door);
            float x = world.X - (door.Area.X + door.Area.Width / 2f), y = world.Y - door.Area.Top;
            if (x >= Math.Min(plaque.Left, badge.Left) * toWorld && x <= Math.Max(plaque.Right, badge.Right) * toWorld
                && y >= badge.Top * toWorld && y <= 0f)
                return door;
        }
        return null;
    }

    /// <summary>The way out whose chevrons the mouse is on: the stretch of floor at an end, and over it where the label goes, or round a stair. Only ways that are there count, locked ones included.</summary>
    /// <param name="room">The room.</param>
    /// <param name="flags">The game's flags, for which ways out are there.</param>
    /// <param name="world">The point, in world pixels.</param>
    /// <returns>The way out, or null.</returns>
    public RoomExit ChevronAt(RoomDefinition room, ISet<string> flags, Vector2 world)
    {
        const float toWorld = 1f / ZoneCatalog.RoomScale;
        foreach (RoomExit exit in room.Exits)
        {
            if (!exit.Presence.Holds(flags)) continue;

            if (exit.Side is ExitSide.Up or ExitSide.Down)
            {
                float back = room.BackEdgeAt(exit.X);
                if (MathF.Abs(world.X - exit.X) <= StairHitHalf * toWorld
                    && world.Y >= back - StairHitAbove * toWorld && world.Y <= back + StairHitBelow * toWorld)
                    return exit;
            }
            else if (MathF.Abs(world.X - exit.X) <= EdgeHitWidth * toWorld
                && world.Y >= room.BackEdgeAt(world.X) - EdgeHitAbove * toWorld && world.Y <= room.FrontEdgeAt(world.X))
            {
                return exit;
            }
        }
        return null;
    }

    /// <summary>Lays out a door's badge and plaque, its outline if the mouse is on it, and its reason if it's locked and the mouse is on it.</summary>
    private void LayOutDoor(Camera2D camera, IndicatorFrame frame, RoomDoor door, bool pointed, float time)
    {
        bool locked = door.LockedWhy is not null;
        MarkerKind behind = Leads(door.ToZone) ? frame.MarkerBehind(door.ToZone, door.ToRoom) : MarkerKind.None;

        if (pointed)
        {
            Vector2 from = camera.ToScreen(new Vector2(door.Area.Left, door.Area.Top));
            Vector2 to = camera.ToScreen(new Vector2(door.Area.Right, door.Area.Bottom));
            var outline = new Rectangle((int)MathF.Round(from.X), (int)MathF.Round(from.Y), (int)MathF.Round(to.X - from.X), (int)MathF.Round(to.Y - from.Y));
            _slices.Add((outline, OutlineColumn, ChevronRow, OutlineLight * (0.75f + 0.25f * Wave(time, 0f))));
        }

        if (door.Badge == BadgeKind.Door && !pointed && !frame.ShowAll && behind == MarkerKind.None) return;

        Vector2 top = camera.ToScreen(new Vector2(door.Area.X + door.Area.Width / 2f, door.Area.Top));
        var at = new Point((int)MathF.Round(top.X), (int)MathF.Round(top.Y));
        (Rectangle plaque, Rectangle badge) = BadgeLayout(door);
        plaque.Offset(at);
        badge.Offset(at);

        _slices.Add((plaque, PlaqueColumn, locked ? LockedChevronRow : ChevronRow, Color.White));
        _sprites.Add((_sheet, CellOf((int)door.Badge, locked ? LockedBadgeRow : BadgeRow), badge.Location.ToVector2(), Scale, Color.White));
        if (locked) _sprites.Add((_sheet, CellOf(PadlockColumn, BadgeRow), badge.Location.ToVector2(), Scale, Color.White));
        if (behind != MarkerKind.None)
            AddBang(behind, new Vector2(badge.Right - BadgeSize / 4f, badge.Top + MarkerCell * InlineScale / 2f), Bob(time, door.Area.X));

        string name = door.Name.ToUpperInvariant();
        float width = _font.MeasureString(name).X;
        _lines.Add((name, new Vector2(MathF.Round(plaque.Center.X - width / 2f), MathF.Round(plaque.Center.Y - _font.LineSpacing / 2f)),
            locked ? Palette.Note : Palette.Bone));

        if (locked && pointed) AddWhy(door.LockedWhy, badge.Center.X, badge.Top - LabelGap);
    }

    /// <summary>Lays out a way out's chevrons, pulsing toward it (or greyed and still, with a padlock, if it's locked), the "!" of the room through it, and its label and reason when they show.</summary>
    private void LayOutExit(Camera2D camera, IndicatorFrame frame, RoomExit exit, bool pointed, float time)
    {
        RoomDefinition room = frame.Room;
        bool stair = exit.Side is ExitSide.Up or ExitSide.Down;
        bool locked = exit.LockedWhy is not null;
        const float half = Cell * Scale / 2f;

        // Where the two chevrons stand on screen, the inner one first so the pulse runs out toward
        // the way out, which way that is, and the x the label lines up with.
        Vector2 inner, outer, way;
        float anchor;
        if (stair)
        {
            Vector2 foot = camera.ToScreen(new Vector2(exit.X, room.BackEdgeAt(exit.X)));
            Vector2 lower = foot - new Vector2(0f, StairLift), upper = lower - new Vector2(0f, StairSpacing);
            bool up = exit.Side == ExitSide.Up;
            (inner, outer) = up ? (lower, upper) : (upper, lower);
            way = up ? -Vector2.UnitY : Vector2.UnitY;
            anchor = foot.X;
        }
        else
        {
            float inward = exit.Side == ExitSide.East ? -1f : 1f;
            float sample = exit.X + inward * (EdgeInset + EdgeSpacing / 2f) / ZoneCatalog.RoomScale;
            Vector2 edge = camera.ToScreen(new Vector2(exit.X, (room.BackEdgeAt(sample) + room.FrontEdgeAt(sample)) / 2f));
            outer = edge + new Vector2(inward * EdgeInset, 0f);
            inner = outer + new Vector2(inward * EdgeSpacing, 0f);
            way = new Vector2(-inward, 0f);
            anchor = edge.X + inward * LabelInset;
        }

        Rectangle source = CellOf(ChevronColumn(exit.Side), locked ? LockedChevronRow : ChevronRow);
        if (locked)
        {
            AddCell(source, inner, Color.White);
            AddCell(source, outer, Color.White);
            AddCell(CellOf(PadlockColumn, BadgeRow), inner, Color.White);
        }
        else
        {
            Vector2 nudge = way * MathF.Round(PulseNudge * Wave(time, 0f));
            AddCell(source, inner + nudge, Color.White * (PulseFloor + (1f - PulseFloor) * Wave(time, 0f)));
            AddCell(source, outer + nudge, Color.White * (PulseFloor + (1f - PulseFloor) * Wave(time, PulseLag)));
        }

        MarkerKind behind = Leads(exit.ToZone) ? frame.MarkerBehind(exit.ToZone, exit.ToRoom) : MarkerKind.None;
        if (behind != MarkerKind.None)
        {
            // Beside the chevrons, on the side away from the way out: to the right of a stair's.
            Vector2 middle = (inner + outer) / 2f;
            float aside = half + BangWidth * InlineScale;
            Vector2 beside = stair ? middle + new Vector2(aside, 0f) : inner - way * aside;
            AddBang(behind, beside + new Vector2(0f, MarkerCell * InlineScale / 2f), Bob(time, exit.X));
        }

        bool near = !stair && MathF.Abs(frame.Leader.X - exit.X) <= LabelReach;
        if (!pointed && !near && !frame.ShowAll) return;

        // The label over the chevrons, kept in from the end of the room, with the compass in front of
        // it for a way to somewhere else on the world map, and a locked one's reason over that.
        string label = LabelOf(room, exit).ToUpperInvariant();
        bool compass = !locked && (exit.Travel || exit.ToZone != room.ZoneId && Leads(exit.ToZone) && ZoneCatalog.Get(exit.ToZone).World is not null);
        float icon = compass ? Cell * InlineScale + IconGap : 0f;
        float width = icon + _font.MeasureString(label).X;
        float height = compass ? Cell * InlineScale : _font.LineSpacing;
        float left = stair ? anchor - width / 2f : exit.Side == ExitSide.East ? anchor - width : anchor;
        float top = MathF.Min(inner.Y, outer.Y) - half - LabelGap - height;
        AddLine(label, left, top, width, height, icon, locked ? Palette.Note : Palette.Bone);
        if (compass) _sprites.Add((_sheet, CellOf(CompassColumn, BadgeRow), new Vector2(MathF.Round(left), MathF.Round(top)), InlineScale, Color.White));

        if (locked && pointed) AddWhy(exit.LockedWhy, left + width / 2f, top - BackingSpread.Y - LabelGap);
    }

    /// <summary>Lays out one glint: a frame of its twinkle if it's twinkling now, or its fullest frame while Alt is held.</summary>
    private void LayOutGlint(Camera2D camera, Vector2 at, bool showAll, float time)
    {
        int frame = GlintFull;
        if (!showAll)
        {
            frame = (int)((time + at.X * GlintStagger) % GlintCycle / GlintFrameTime);
            if (frame >= GlintFrames) return;
        }

        // The star's middle is the pixel just up and left of its cell's middle.
        Vector2 screen = camera.ToScreen(at) - new Vector2(Cell / 2 - 0.5f) * Scale;
        _sprites.Add((_sheet, CellOf(GlintColumn + frame, ChevronRow), new Vector2(MathF.Round(screen.X), MathF.Round(screen.Y)), Scale, Color.White));
    }

    /// <summary>Lays out a name over a head, if it shows: in front of it the "!" if they've a marker and the badge if you can buy from or train with them, and the name in the colour that goes with them.</summary>
    private void LayOutPlate(Camera2D camera, IndicatorFrame frame, (string Name, Vector2 Head, MarkerKind Marker, BadgeKind? Badge, bool Named) plate)
    {
        Vector2 mouse = frame.Mouse;
        bool pointed = MathF.Abs(mouse.X - plate.Head.X) <= FigureReach.X && mouse.Y >= plate.Head.Y && mouse.Y <= plate.Head.Y + FigureReach.Y;
        if (!plate.Named && !pointed && !frame.ShowAll) return;

        string name = plate.Name.ToUpperInvariant();
        Color colour = plate.Marker switch
        {
            MarkerKind.Quest => Palette.Gold,
            MarkerKind.Event => PlateRed,
            _ => plate.Badge is null ? Palette.Bone : PlateBlue,
        };

        float bang = plate.Marker != MarkerKind.None ? BangWidth * InlineScale + IconGap : 0f;
        float badge = plate.Badge is not null ? Cell * InlineScale + IconGap : 0f;
        float width = bang + badge + _font.MeasureString(name).X;
        float height = plate.Badge is not null || plate.Marker != MarkerKind.None ? Cell * InlineScale : _font.LineSpacing;

        Vector2 head = camera.ToScreen(plate.Head);
        float left = head.X - width / 2f, top = head.Y - PlateGap - BackingSpread.Y - height;
        AddLine(name, left, top, width, height, bang + badge, colour);
        if (plate.Marker != MarkerKind.None)
            AddBang(plate.Marker, new Vector2(MathF.Round(left) + BangWidth * InlineScale / 2f, MathF.Round(top) + height / 2f + MarkerCell * InlineScale / 2f), 0f);
        if (plate.Badge is BadgeKind kind)
            _sprites.Add((_sheet, CellOf((int)kind, BadgeRow), new Vector2(MathF.Round(left + bang), MathF.Round(top)), InlineScale, Color.White));
    }

    /// <summary>Adds a line of writing on its smudge, its words after room left for icons in front of them, set in the middle of the height.</summary>
    private void AddLine(string text, float left, float top, float width, float height, float icons, Color colour)
    {
        left = MathF.Round(left);
        top = MathF.Round(top);
        _backings.Add(new Rectangle((int)left, (int)(top - BackingSpread.Y), (int)width, (int)(height + BackingSpread.Y * 2f)));
        _lines.Add((text, new Vector2(left + icons, MathF.Round(top + (height - _font.LineSpacing) / 2f)), colour));
    }

    /// <summary>Adds why something's locked, wrapped and centred over a point, its last row ending there, kept on the screen.</summary>
    private void AddWhy(string why, float centre, float bottom)
    {
        List<string> rows = Text.Wrap(why, _font, WhyWidth);
        for (int i = 0; i < rows.Count; i++)
        {
            float width = _font.MeasureString(rows[i]).X;
            float left = Math.Clamp(centre - width / 2f, LabelInset, LegendQuestGame.ScreenWidth - LabelInset - width);
            float top = bottom - (rows.Count - i) * _font.LineSpacing;
            AddLine(rows[i], left, top, width, _font.LineSpacing, 0f, Palette.Narration);
        }
    }

    /// <summary>Adds one cell of indicators.png at 3x, centred on a point on screen.</summary>
    private void AddCell(Rectangle source, Vector2 centre, Color tint)
    {
        Vector2 at = centre - new Vector2(Cell * Scale / 2f);
        _sprites.Add((_sheet, source, new Vector2(MathF.Round(at.X), MathF.Round(at.Y)), Scale, tint));
    }

    /// <summary>Adds a small "!" from markers.png standing on a point, raised by its bob.</summary>
    private void AddBang(MarkerKind kind, Vector2 foot, float bob)
    {
        var at = new Vector2(MathF.Round(foot.X - MarkerCell * InlineScale / 2f), MathF.Round(foot.Y - MarkerCell * InlineScale - bob));
        _sprites.Add((_markers, new Rectangle(0, ((int)kind - 1) * MarkerCell, MarkerCell, MarkerCell), at, InlineScale, Color.White));
    }

    /// <summary>Where a door's plaque and badge go, in canvas pixels from the middle of the top of its doorway: the plaque as wide as the name with room either side, its bottom BadgeLift over the doorway, and the badge sitting on its top edge.</summary>
    private (Rectangle Plaque, Rectangle Badge) BadgeLayout(RoomDoor door)
    {
        int width = (int)MathF.Round(_font.MeasureString(door.Name.ToUpperInvariant()).X) + PlaqueRoom * 2;
        var plaque = new Rectangle(-width / 2, -BadgeLift - PlaqueHeight, width, PlaqueHeight);
        var badge = new Rectangle(-BadgeSize / 2, plaque.Top + BadgeOverlap - BadgeSize, BadgeSize, BadgeSize);
        return (plaque, badge);
    }

    /// <summary>What an end's or a stair's label says: why not, if it's locked and has no label; Travel, if it opens the world map; the zone file's label; or the name of where it goes.</summary>
    private static string LabelOf(RoomDefinition room, RoomExit exit)
    {
        if (exit.LockedWhy is not null) return exit.Label ?? LockedLabel;
        if (exit.Travel) return TravelLabel;
        if (exit.Label is not null) return exit.Label;
        if (exit.ToZone == RoomExit.Back) return OutLabel;
        return exit.ToZone == room.ZoneId ? ZoneCatalog.Get(room.ZoneId).Room(exit.ToRoom).Name : ZoneCatalog.Get(exit.ToZone).Name;
    }

    /// <summary>Whether a way out goes somewhere with a room to ask the marker of: not locked, and not back out the way you came.</summary>
    private static bool Leads(string toZone) => toZone is not null && toZone != RoomExit.Back;

    /// <summary>The column of indicators.png a way out's chevrons are in.</summary>
    private static int ChevronColumn(ExitSide side) => side switch
    {
        ExitSide.East => 0,
        ExitSide.West => 1,
        ExitSide.Up => 2,
        _ => 3,
    };

    /// <summary>One cell of indicators.png.</summary>
    private static Rectangle CellOf(int column, int row) => new(column * Cell, row * Cell, Cell, Cell);

    /// <summary>The chevrons' pulse, 0 to 1, lagging by some of a turn.</summary>
    private static float Wave(float time, float lag) => 0.5f - 0.5f * MathF.Cos(time * PulseRate - lag);

    /// <summary>How far up a small "!" has bobbed, in screen pixels, out of step with the next by where it is.</summary>
    private static float Bob(float time, float x) =>
        MathF.Round((0.5f - 0.5f * MathF.Cos(time * BobRate + x * BobStagger)) * BobHeight * InlineScale);

    /// <summary>Whether a point is inside a rectangle, edges included.</summary>
    private static bool Inside(BoundingRectangle box, Vector2 point) =>
        point.X >= box.Left && point.X <= box.Right && point.Y >= box.Top && point.Y <= box.Bottom;
}
