using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The zone screen's markers and indicators: the gold "!" over anyone with work or a quest for you,
/// the red one over an event, an arrow at the side of the screen for any that are off it, and the
/// room's badges, chevrons, name plates and glints, which IndicatorLayer draws.
/// </summary>
/// <remarks>
/// Who's marked only changes when the flags or the party do, so it's worked out then (after a
/// conversation, a trigger, using something, or changing who you control), not every frame, and so
/// is the "!" each room through a door or a way out has. One of the party with something new to say
/// to you is marked like anyone else, but never whoever you're controlling. They're drawn after the
/// dark, at the room's zoom so they're the size of everything else in it, and they bob a little so
/// the eye goes to them. A room is the screen's height, so a marker off the screen is only ever off
/// one side or the other, and its arrow points along the room. Anyone with a name plate over their
/// head has their marker, their prompt and what they say lifted clear of it.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>One cell of markers.png, in art pixels, and how wide the "!" in it is. Have to match MARKER and BANG_WIDTH in tools/generate_ui.py.</summary>
    private const int MarkerCell = 14;
    private const int BangWidth = 8;

    /// <summary>The columns of markers.png: the "!", then the arrow to one that's off the screen, pointing right. Each kind of marker has a row.</summary>
    private const int BangColumn = 0;
    private const int ArrowColumn = 1;

    /// <summary>How far over someone's head a "!" floats, and how high it bobs, in art pixels.</summary>
    private const float MarkerGap = 2f;
    private const float MarkerBob = 2f;

    /// <summary>How fast the markers bob, and how far out of step one is with another for every art pixel between them, so a row of them doesn't bob together.</summary>
    private const float MarkerBobRate = 2.4f;
    private const float MarkerBobStagger = 0.05f;

    /// <summary>How far over the middle of a trigger its "!" floats, in art pixels.</summary>
    private const float MarkerOverArea = 6f;

    /// <summary>Where the arrows to markers off the screen go, in screen pixels: this far in from the sides, and between this far down (under the band across the top) and this far up from the bottom.</summary>
    private const int ArrowInset = 40;
    private const int ArrowTop = 150;

    /// <summary>How big the arrows are drawn, and how far they nudge toward what they're pointing at, in screen pixels.</summary>
    private const float ArrowScale = 3f;
    private const float ArrowNudge = 5f;

    /// <summary>Where a glint goes on a thing's picture, as a share of its height up from its foot: a little above the middle.</summary>
    private const float GlintRise = 0.6f;

    private readonly IndicatorLayer _indicators = new();

    private Texture2D _markers;

    /// <summary>Everyone and everywhere marked: the figure it floats over (null for a trigger), where they were stood or the middle of the trigger, and which marker.</summary>
    /// <remarks>The marker follows anyone who wanders, and the party as they walk. Where they were stood only sets its bob, so the bob doesn't speed up and slow down as they go.</remarks>
    private readonly List<(WalkerSprite Walker, Vector2 Spot, MarkerKind Kind)> _marked = new();

    /// <summary>The "!" of each room a door or a way out leads into, by zone and room, as far as it's been asked for since the flags last changed.</summary>
    private readonly Dictionary<(string Zone, string Room), MarkerKind> _behind = new();

    /// <summary>The figures with a name plate over their head this frame.</summary>
    private readonly HashSet<WalkerSprite> _plated = new();

    /// <summary>Works out who and what's marked with the flags as they are: anyone in the room whose conversation now has a marker, the same for the party but you, and every trigger the room marks that's there and hasn't gone off. Forgets what the rooms through the doors had.</summary>
    private void RefreshMarkers()
    {
        _marked.Clear();
        _behind.Clear();

        foreach (Npc npc in _npcs)
        {
            MarkerKind kind = ScriptFor(npc)?.Marker ?? MarkerKind.None;
            if (kind != MarkerKind.None) _marked.Add((npc.Walker, npc.Walker.Position, kind));
        }

        for (int i = 0; i < _walkers.Count; i++)
        {
            if (i == _game.Leader) continue;

            MarkerKind kind = ScriptFor(_game.Party[i])?.Marker ?? MarkerKind.None;
            if (kind != MarkerKind.None) _marked.Add((_walkers[i], _walkers[i].Position, kind));
        }

        foreach (MapTrigger trigger in _room.Triggers)
        {
            if (!trigger.Mark || !trigger.Presence.Holds(_game.Flags) || Used(trigger)) continue;

            var middle = new Vector2(trigger.Area.X + trigger.Area.Width / 2f, trigger.Area.Y + trigger.Area.Height / 2f);
            _marked.Add((null, middle, MarkerKind.Event));
        }
    }

    /// <summary>The "!" a room through a door or a way out has, worked out the first time it's asked for after the flags change.</summary>
    /// <param name="zone">The zone it's in.</param>
    /// <param name="room">The room.</param>
    private MarkerKind MarkerBehind(string zone, string room)
    {
        if (!_behind.TryGetValue((zone, room), out MarkerKind kind)) _behind[(zone, room)] = kind = RoomMarkers.In(zone, room, _game);
        return kind;
    }

    /// <summary>What IndicatorLayer draws this frame: the room, the plates over everyone in it, the glints on what's still to be used, where you and the mouse are, and whether Alt's held. Notes who has a plate.</summary>
    private IndicatorFrame Indicators()
    {
        _plated.Clear();
        var plates = new List<(string Name, Vector2 Head, MarkerKind Marker, BadgeKind? Badge, bool Named)>();
        foreach (Npc npc in _npcs)
        {
            bool named = npc.Definition.Cast is not null || npc.Definition.Talk is not null || npc.Definition.Badge is not null || _campers.Contains(npc);
            MarkerKind marker = MarkerKind.None;
            foreach ((WalkerSprite walker, _, MarkerKind kind) in _marked)
                if (walker == npc.Walker) marker = kind;
            plates.Add((npc.Definition.Name, HeadOf(npc.Walker), marker, npc.Definition.Badge, named));
            if (named || _showAll || NpcAt(_mouseWorld) == npc) _plated.Add(npc.Walker);
        }

        var glints = new List<Vector2>();
        foreach ((MapInteractable thing, PropSprite sprite) in _things)
            if (_world.CanUse(thing)) glints.Add(thing.Foot - new Vector2(0f, sprite.Size.Y * GlintRise));

        return new IndicatorFrame(_room, _game.Flags, _showAll, Leader.Position, _mouseWorld, plates, glints, MarkerBehind);
    }

    /// <summary>Where the top of someone's head is on the screen, lifted clear of their name plate if they've one up.</summary>
    private Vector2 HeadOnScreen(WalkerSprite walker) =>
        _camera.ToScreen(HeadOf(walker)) - new Vector2(0f, _plated.Contains(walker) ? IndicatorLayer.PlateRoom : 0f);

    /// <summary>Where a marker floats over, on the screen: the top of someone's head, wherever they've walked to and clear of their plate, or a little over the middle of a trigger.</summary>
    private Vector2 MarkedAt(WalkerSprite walker, Vector2 spot) =>
        walker is not null ? HeadOnScreen(walker) : _camera.ToScreen(spot - new Vector2(0f, MarkerOverArea));

    /// <summary>Works out where each marker on the screen goes, over its head or its trigger, moved up out of the way of the prompt and the lines over people's heads.</summary>
    /// <param name="taken">What's already over people's heads, on screen. Each marker's room is added to it.</param>
    /// <param name="marks">Filled with each marker's room on screen (with space at the top for its bob), which marker, and where its bob's up to.</param>
    private void PlaceMarkers(List<Rectangle> taken, List<(Rectangle Box, MarkerKind Kind, float Phase)> marks)
    {
        int zoom = _camera.Zoom;
        foreach ((WalkerSprite walker, Vector2 spot, MarkerKind kind) in _marked)
        {
            Vector2 screen = MarkedAt(walker, spot);
            if (!OnScreen(screen)) continue;

            int height = (int)((MarkerCell + MarkerBob) * zoom);
            var box = new Rectangle((int)(screen.X - BangWidth * zoom / 2f), (int)(screen.Y - MarkerGap * zoom) - height, BangWidth * zoom, height);
            marks.Add((MakeRoom(box, taken), kind, spot.X * MarkerBobStagger));
        }
    }

    /// <summary>Draws each marker on the screen at the room's zoom, bobbing up from the bottom of its room.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point sampled.</param>
    /// <param name="marks">Where each goes, from LayOutOverheads.</param>
    /// <param name="gameTime">The frame's timing, for the bob.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    private void DrawMarkers(SpriteBatch spriteBatch, List<(Rectangle Box, MarkerKind Kind, float Phase)> marks, GameTime gameTime, float alpha)
    {
        float time = (float)gameTime.TotalGameTime.TotalSeconds;
        int zoom = _camera.Zoom;
        foreach ((Rectangle box, MarkerKind kind, float phase) in marks)
        {
            float bob = MathF.Round((0.5f - 0.5f * MathF.Cos(time * MarkerBobRate + phase)) * MarkerBob * zoom);
            spriteBatch.Draw(_markers, new Vector2(box.Center.X, box.Bottom - bob), Cell(BangColumn, kind), Color.White * alpha,
                0f, new Vector2(MarkerCell / 2f, MarkerCell), zoom, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Draws an arrow at the side of the screen for every marker that's off it, in the marker's colour, pointing along the room to it at its height.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point sampled.</param>
    /// <param name="gameTime">The frame's timing, for the nudge.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    private void DrawArrows(SpriteBatch spriteBatch, GameTime gameTime, float alpha)
    {
        float nudge = (0.5f - 0.5f * MathF.Cos((float)gameTime.TotalGameTime.TotalSeconds * MarkerBobRate)) * ArrowNudge;

        foreach ((WalkerSprite walker, Vector2 spot, MarkerKind kind) in _marked)
        {
            Vector2 screen = MarkedAt(walker, spot);
            if (OnScreen(screen)) continue;

            bool left = screen.X < LegendQuestGame.ScreenWidth / 2f;
            float x = left ? ArrowInset - nudge : LegendQuestGame.ScreenWidth - ArrowInset + nudge;
            float y = Math.Clamp(screen.Y, ArrowTop, LegendQuestGame.ScreenHeight - ArrowInset);
            spriteBatch.Draw(_markers, new Vector2(MathF.Round(x), MathF.Round(y)), Cell(ArrowColumn, kind), Color.White * alpha,
                0f, new Vector2(MarkerCell / 2f), ArrowScale, left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
    }

    /// <summary>One cell of markers.png: a column, in the row for a kind of marker.</summary>
    private static Rectangle Cell(int column, MarkerKind kind) =>
        new(column * MarkerCell, ((int)kind - 1) * MarkerCell, MarkerCell, MarkerCell);
}
