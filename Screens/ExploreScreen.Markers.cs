using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Dialogue;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's markers: the gold "!" over anyone with work or a quest for you, the red one over
/// an event, and an arrow at the edge of the screen for any that are off it.
/// </summary>
/// <remarks>
/// Who's marked only changes when the flags do, so it's worked out then (after a conversation, a
/// trigger, or using something), not every frame. They're drawn after the dark and the roofs, at the
/// map's zoom so they're the size of everything else on it, and they bob a little so the eye goes
/// to them. The map screen shows them too.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>One cell of markers.png, in art pixels, and how wide the "!" in it is. Have to match MARKER and BANG_WIDTH in tools/generate_ui.py.</summary>
    private const int MarkerCell = 14;
    private const int BangWidth = 8;

    /// <summary>The columns of markers.png: the "!", then the arrow to one that's off the screen pointing right, and pointing down and right. Each kind of marker has a row.</summary>
    private const int BangColumn = 0;
    private const int ArrowColumn = 1;
    private const int DiagonalColumn = 2;

    /// <summary>How far over someone's head a "!" floats, and how high it bobs, in art pixels.</summary>
    private const float MarkerGap = 2f;
    private const float MarkerBob = 2f;

    /// <summary>How fast the markers bob, and how far out of step one is with another for every art pixel between them, so a row of them doesn't bob together.</summary>
    private const float MarkerBobRate = 2.4f;
    private const float MarkerBobStagger = 0.05f;

    /// <summary>How far over the middle of a trigger its "!" floats, in art pixels.</summary>
    private const float MarkerOverArea = 6f;

    /// <summary>Where the arrows to markers off the screen go, in screen pixels: this far in from the sides and the bottom, and this far down, under the band across the top.</summary>
    private const int ArrowInset = 40;
    private const int ArrowTop = 150;

    /// <summary>How big the arrows are drawn, and how far they nudge toward what they're pointing at, in screen pixels.</summary>
    private const float ArrowScale = 3f;
    private const float ArrowNudge = 5f;

    private Texture2D _markers;

    /// <summary>Everyone and everywhere marked: the person (null for a trigger), where they were stood or the middle of the trigger, and which marker.</summary>
    /// <remarks>The marker follows anyone who wanders. Where they were stood only sets its bob, so the bob doesn't speed up and slow down as they walk.</remarks>
    private readonly List<(Npc Npc, Vector2 Spot, MarkerKind Kind)> _marked = new();

    /// <summary>Works out who and what's marked with the flags as they are: anyone whose conversation now has a marker, and every trigger the map marks that's there and hasn't gone off.</summary>
    private void RefreshMarkers()
    {
        _marked.Clear();

        foreach (Npc npc in _npcs)
        {
            MarkerKind kind = DialogueCatalog.For(npc.Definition.Id, _game.Flags)?.Marker ?? MarkerKind.None;
            if (kind != MarkerKind.None) _marked.Add((npc, npc.Walker.Position, kind));
        }

        foreach (MapTrigger trigger in _map.Triggers)
        {
            if (!trigger.Mark || !trigger.Presence.Holds(_game.Flags) || Used(trigger)) continue;

            var middle = new Vector2(trigger.Area.X + trigger.Area.Width / 2f, trigger.Area.Y + trigger.Area.Height / 2f);
            _marked.Add((null, middle, MarkerKind.Event));
        }
    }

    /// <summary>Every marker and where it is, in world pixels, for the map screen: at someone's feet, or the middle of a trigger.</summary>
    private List<(Vector2 At, MarkerKind Kind)> MapMarkers()
    {
        var markers = new List<(Vector2 At, MarkerKind Kind)>();
        foreach ((Npc npc, Vector2 spot, MarkerKind kind) in _marked)
            markers.Add((npc?.Walker.Position ?? spot, kind));
        return markers;
    }

    /// <summary>The point a marker floats over, in world pixels: the top of someone's head, wherever they've walked to, or a little over the middle of a trigger.</summary>
    private static Vector2 MarkedAt(Npc npc, Vector2 spot) =>
        npc is not null ? npc.Walker.Position - new Vector2(0f, HeadHeight) : spot - new Vector2(0f, MarkerOverArea);

    /// <summary>Works out where each marker on the screen goes, over its head or its trigger, moved up out of the way of the prompt and the lines over people's heads.</summary>
    /// <param name="taken">What's already over people's heads, on screen. Each marker's room is added to it.</param>
    /// <param name="marks">Filled with each marker's room on screen (with space at the top for its bob), which marker, and where its bob's up to.</param>
    private void PlaceMarkers(List<Rectangle> taken, List<(Rectangle Box, MarkerKind Kind, float Phase)> marks)
    {
        int zoom = _camera.Zoom;
        foreach ((Npc npc, Vector2 spot, MarkerKind kind) in _marked)
        {
            Vector2 over = MarkedAt(npc, spot);
            Vector2 screen = _camera.ToScreen(over);
            if (!OnScreen(screen)) continue;

            int height = (int)((MarkerCell + MarkerBob) * zoom);
            var box = new Rectangle((int)(screen.X - BangWidth * zoom / 2f), (int)(screen.Y - MarkerGap * zoom) - height, BangWidth * zoom, height);
            marks.Add((MakeRoom(box, taken), kind, spot.X * MarkerBobStagger));
        }
    }

    /// <summary>Draws each marker on the screen at the map's zoom, bobbing up from the bottom of its room.</summary>
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

    /// <summary>Draws an arrow at the edge of the screen for every marker that's off it, in the marker's colour, pointing the way to it.</summary>
    /// <remarks>The arrows only ever turn a quarter at a time, with a picture of their own for the diagonals, so their pixels stay square.</remarks>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point sampled.</param>
    /// <param name="gameTime">The frame's timing, for the nudge.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    private void DrawArrows(SpriteBatch spriteBatch, GameTime gameTime, float alpha)
    {
        var edge = new Rectangle(ArrowInset, ArrowTop, LegendQuestGame.ScreenWidth - ArrowInset * 2, LegendQuestGame.ScreenHeight - ArrowTop - ArrowInset);
        Vector2 middle = edge.Center.ToVector2();
        float nudge = (0.5f - 0.5f * MathF.Cos((float)gameTime.TotalGameTime.TotalSeconds * MarkerBobRate)) * ArrowNudge;

        foreach ((Npc npc, Vector2 spot, MarkerKind kind) in _marked)
        {
            Vector2 screen = _camera.ToScreen(MarkedAt(npc, spot));
            if (OnScreen(screen)) continue;

            // Out from the middle toward it, as far as the edge. A way straight across divides by
            // nothing on the other axis, which comes out infinite and loses to the other.
            Vector2 way = screen - middle;
            float reach = MathF.Min(edge.Width / 2f / MathF.Abs(way.X), edge.Height / 2f / MathF.Abs(way.Y));
            Vector2 at = middle + way * reach + Vector2.Normalize(way) * nudge;

            // Which of eight ways it's nearest: 0 is right, and each after is an eighth of a turn clockwise.
            int eighth = (int)MathF.Round(MathF.Atan2(way.Y, way.X) / MathHelper.PiOver4) & 7;
            int column = eighth % 2 == 0 ? ArrowColumn : DiagonalColumn;
            float turn = eighth / 2 * MathHelper.PiOver2;

            spriteBatch.Draw(_markers, new Vector2(MathF.Round(at.X), MathF.Round(at.Y)), Cell(column, kind), Color.White * alpha,
                turn, new Vector2(MarkerCell / 2f), ArrowScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>One cell of markers.png: a column, in the row for a kind of marker.</summary>
    private static Rectangle Cell(int column, MarkerKind kind) =>
        new(column * MarkerCell, ((int)kind - 1) * MarkerCell, MarkerCell, MarkerCell);
}
