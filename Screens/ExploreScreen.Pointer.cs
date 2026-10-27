using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Components;
using LegendQuest.Rules;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's mouse: what it's over, the tooltip and the cursor for that, the path you'd
/// take if you clicked, and walking it when you do.
/// </summary>
/// <remarks>
/// Clicking the ground walks there by A*. Clicking someone or something out of reach walks you
/// into reach first, then talks to them or uses it, the same as E would. The keys always take over
/// from a path. Hovering a party card, a person, a thing or the ground each give their own cursor.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How close to a point on a path you have to get before heading for the next one, in art pixels.</summary>
    private const float ArriveDistance = 2f;

    /// <summary>How long you can push along a path without getting anywhere before giving up on it.</summary>
    private const float PathStuckSeconds = 0.4f;

    /// <summary>How much short of something's reach to aim for, so you end up inside it and not on the edge.</summary>
    private const float ReachMargin = 2f;

    /// <summary>The dots of the path you'd walk, how far apart they are in art pixels.</summary>
    private const float PathDotSpacing = 6f;

    /// <summary>One mark on map-marks.png, in art pixels. Has to match MARK in tools/generate_ui.py.</summary>
    private const int MarkSize = 12;

    /// <summary>The soft pool of light under whatever the mouse is on: its size in art pixels, and its colour.</summary>
    private static readonly Vector2 HighlightSize = new(28f, 10f);
    private static readonly Color HighlightColor = new Color(255, 226, 170) * 0.35f;

    private readonly Tooltip _tooltip = new();
    private readonly PartyStrip _strip = new();

    private CursorSprite _cursor;
    private Texture2D _marks;

    /// <summary>The path you're walking, which point of it you're heading for, and how long you've been stuck.</summary>
    private List<Vector2> _path;
    private int _pathStep;
    private float _pathStuck;

    /// <summary>What to do when you get in reach of where you're walking to, if anything: where it is, how close is close enough, and the thing to do.</summary>
    private (Vector2 At, float Reach, Action Act)? _pending;

    /// <summary>The path you'd walk if you clicked where the mouse is, and the tiles it was worked out for, so it only gets worked out again when one of them changes.</summary>
    private List<Vector2> _preview;
    private (Point From, Point To)? _previewFor;

    /// <summary>What the tooltip says this frame (null for no tooltip), where the mouse is, and where the pool of light goes.</summary>
    private string _tipName;
    private string _tipDetail;
    private Point _mouse;
    private Vector2? _highlight;

    /// <summary>Works out what the mouse is over, sets the cursor and the tooltip for it, and does whatever a click on it does.</summary>
    /// <param name="input">The input this frame.</param>
    private void HandlePointer(InputState input)
    {
        _tipName = null;
        _tipDetail = null;
        _highlight = null;
        _mouse = input.MousePosition;
        bool click = input.IsNewLeftClick;

        if (!new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight).Contains(_mouse))
        {
            _preview = null;
            return;
        }

        // The party's cards are in front of the map, so they come first.
        if (_strip.CardAt(_mouse, _walkers.Count, _game.Leader) is int card)
        {
            _preview = null;
            DescribeMember(card);
            if (click) SelectLeader(card);
            return;
        }

        Vector2 world = _camera.ToWorld(_mouse.ToVector2());

        if (NpcAt(world) is Npc npc)
        {
            _preview = null;
            _tipName = npc.Definition.Name;
            _highlight = npc.Walker.Position;
            _cursor.Shape = CursorShape.Talk;
            if (click) Approach(npc.Walker.Position, TalkReach, () => Talk(npc));
            return;
        }

        if (MemberAt(world) is int member)
        {
            _preview = null;
            DescribeMember(member);
            _highlight = _walkers[member].Position;
            if (click) SelectLeader(member);
            return;
        }

        if (ThingAt(world) is MapInteractable thing)
        {
            _preview = null;
            _tipName = thing.Name;
            _highlight = UseSpot(thing);
            bool usable = _world.CanUse(thing);
            if (usable) _cursor.Shape = CursorShape.Use;
            if (click && usable) Approach(thing.Foot, thing.Reach, () => Use(thing));
            return;
        }

        // The ground: the path there, worked out again only when you or the mouse move to another tile.
        Point from = Tilemap.CellOf(Leader.Position), to = Tilemap.CellOf(world);
        if (_previewFor != (from, to))
        {
            _previewFor = (from, to);
            _preview = Pathfinder.Find(_nav, Leader.Position, NavGrid.Middle(to), 0f);
        }

        _cursor.Shape = _preview is null ? CursorShape.Invalid : CursorShape.Move;
        if (click && _preview is not null) StartPath(_preview, null);
    }

    /// <summary>The tooltip for someone in the party: their name, and their level, class and hit points.</summary>
    private void DescribeMember(int index)
    {
        PartyMember member = _game.Party[index];
        _tipName = member.Name;
        _tipDetail = string.Format(CultureInfo.InvariantCulture, "Level {0} {1}  ·  {2} / {3} HP",
            member.Level, ClassCatalog.Get(member.Class).Name, member.Hp, member.MaxHp);
    }

    /// <summary>Uses something now if you're close enough, or walks into reach of it and uses it when you get there.</summary>
    /// <param name="at">Where it is, in world pixels.</param>
    /// <param name="reach">How close you have to be.</param>
    /// <param name="act">What to do once you're there.</param>
    private void Approach(Vector2 at, float reach, Action act)
    {
        if (Vector2.Distance(Leader.Position, at) <= reach)
        {
            Halt();
            act();
            return;
        }

        List<Vector2> path = Pathfinder.Find(_nav, Leader.Position, at, reach - ReachMargin);
        if (path is not null) StartPath(path, (at, reach, act));
    }

    /// <summary>Sets you walking a path, and what to do at the end of it if anything.</summary>
    private void StartPath(List<Vector2> path, (Vector2 At, float Reach, Action Act)? pending)
    {
        _path = path;
        _pathStep = 0;
        _pathStuck = 0f;
        _pending = pending;
    }

    /// <summary>Forgets the path you're on, and what was waiting at the end of it.</summary>
    private void StopPath()
    {
        _path = null;
        _pending = null;
    }

    /// <summary>Walks you along your path: does what's waiting the moment you're in reach of it, and gives up if you've been stuck.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <returns>True if you're walking a path.</returns>
    private bool FollowPath(GameTime gameTime)
    {
        if (_path is null) return false;

        if (_pending is { } pending && Vector2.Distance(Leader.Position, pending.At) <= pending.Reach)
        {
            Halt();
            pending.Act();
            return false;
        }

        Vector2 before = Leader.Position;
        if (!StepAlong(Leader, _path, ref _pathStep, WalkSpeed, gameTime, null))
        {
            StopPath();
            return false;
        }

        _pathStuck = Vector2.Distance(before, Leader.Position) < StuckMovement
            ? _pathStuck + (float)gameTime.ElapsedGameTime.TotalSeconds
            : 0f;
        if (_pathStuck > PathStuckSeconds) Halt();
        return true;
    }

    /// <summary>The person the mouse is over, the one in front if two overlap, or null.</summary>
    private Npc NpcAt(Vector2 world)
    {
        Npc found = null;
        foreach (Npc npc in _npcs)
            if (Contains(npc.Walker.Figure, world) && (found is null || npc.Walker.Position.Y > found.Walker.Position.Y)) found = npc;
        return found;
    }

    /// <summary>Which of the party the mouse is over, the one in front if two overlap, or null.</summary>
    private int? MemberAt(Vector2 world)
    {
        int? found = null;
        for (int i = 0; i < _walkers.Count; i++)
        {
            if (Contains(_walkers[i].Figure, world) && (found is not int best || _walkers[i].Position.Y > _walkers[best].Position.Y))
                found = i;
        }
        return found;
    }

    /// <summary>The thing the mouse is over, by its picture, the one in front if two overlap, or null.</summary>
    private MapInteractable ThingAt(Vector2 world)
    {
        MapInteractable found = null;
        foreach ((MapInteractable thing, PropSprite sprite) in _things)
        {
            Point size = sprite.Size;
            var picture = new BoundingRectangle(sprite.Foot.X - size.X / 2f, sprite.Foot.Y - size.Y, size.X, size.Y);
            if (Contains(picture, world) && (found is null || thing.Foot.Y > found.Foot.Y)) found = thing;
        }
        return found;
    }

    /// <summary>Where you'd stand to use something: its foot, or for something on a wall, the floor below it.</summary>
    private static Vector2 UseSpot(MapInteractable thing) =>
        thing.Look.Placement == PropPlacement.Wall ? thing.Foot + new Vector2(0f, Tilemap.TileSize / 2f) : thing.Foot;

    /// <summary>Whether a point is inside a rectangle.</summary>
    private static bool Contains(BoundingRectangle box, Vector2 point) =>
        point.X >= box.Left && point.X <= box.Right && point.Y >= box.Top && point.Y <= box.Bottom;

    /// <summary>The soft pool of light under whatever the mouse is on. Goes in the additive batch.</summary>
    private void DrawHighlight(SpriteBatch spriteBatch)
    {
        if (_highlight is not Vector2 at) return;

        var area = new Rectangle((int)(at.X - HighlightSize.X / 2f), (int)(at.Y - HighlightSize.Y / 2f), (int)HighlightSize.X, (int)HighlightSize.Y);
        spriteBatch.Draw(_glow, area, HighlightColor);
    }

    /// <summary>Dots along the path you'd walk if you clicked, and a ring where it ends. In world space.</summary>
    private void DrawPathPreview(SpriteBatch spriteBatch)
    {
        if (_preview is null) return;

        Vector2 from = Leader.Position;
        float carry = PathDotSpacing;
        foreach (Vector2 to in _preview)
        {
            float length = Vector2.Distance(from, to);
            for (; carry <= length; carry += PathDotSpacing) DrawMark(spriteBatch, MapMark.Dot, Vector2.Lerp(from, to, carry / length));
            carry -= length;
            from = to;
        }
        DrawMark(spriteBatch, MapMark.Here, from);
    }

    /// <summary>Draws one mark from map-marks.png at its own size, centred on a point in the world.</summary>
    private void DrawMark(SpriteBatch spriteBatch, MapMark mark, Vector2 centre)
    {
        var source = new Rectangle((int)mark * MarkSize, 0, MarkSize, MarkSize);
        spriteBatch.Draw(_marks, new Vector2(MathF.Round(centre.X), MathF.Round(centre.Y)), source, Color.White,
            0f, new Vector2(MarkSize / 2f), 1f, SpriteEffects.None, 0f);
    }

    /// <summary>The tooltip, if the mouse is over anything that has one.</summary>
    private void DrawTooltip(SpriteBatch spriteBatch, float alpha)
    {
        if (_tipName is null) return;
        _tooltip.Draw(spriteBatch, _mouse, _tipName.ToUpperInvariant(), _tipDetail?.ToUpperInvariant(), alpha);
    }
}
