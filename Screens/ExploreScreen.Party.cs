using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's party: who you're controlling, everyone else following along behind, switching
/// between them, and where they all stand when you arrive.
/// </summary>
/// <remarks>
/// The followers walk your trail: wherever your feet have been, a point every few pixels, and each
/// of them aims for the spot their own distance back along it. That keeps them in single file
/// through doors and round corners without any of them having to think. If one falls too far behind
/// or gets stuck on something, they find a path to their spot with the same A* the mouse uses, then
/// go back to the trail. None of them ever stands in your feet. Not from a tutorial.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far apart the party walks along your trail, in art pixels. Under that they overlapped so much they looked like one person with too many heads.</summary>
    private const float Spacing = 22f;

    /// <summary>How far you have to move before your trail gets another point.</summary>
    private const float TrailStep = 3f;

    /// <summary>How many points the trail keeps. The last follower needs about twenty; the rest is room to spare.</summary>
    private const int TrailLength = 120;

    /// <summary>How close to their spot a follower has to be to stop.</summary>
    private const float FollowSlack = 2f;

    /// <summary>How close a follower gets to their spot before they slow down, so they don't overshoot it and jitter.</summary>
    private const float FollowEase = 8f;

    /// <summary>Further than this from their spot, a follower finds a path instead of walking straight at it.</summary>
    private const float CatchUpDistance = 72f;

    /// <summary>How much faster than walking a follower goes when they've fallen behind.</summary>
    private const float CatchUpSpeed = 1.35f;

    /// <summary>How long a follower can push without getting anywhere before they find a way round what's in the way.</summary>
    private const float StuckSeconds = 0.5f;

    /// <summary>How little a follower has to move in a frame to count as not getting anywhere, in art pixels.</summary>
    private const float StuckMovement = 0.05f;

    /// <summary>How much room followers leave round your feet, in art pixels.</summary>
    private const float LeaderRoom = 3f;

    /// <summary>How far from you a follower can be when a map loads and still keep where they were standing.</summary>
    private const float GatherDistance = 96f;

    /// <summary>How many tiles round you to look for somewhere to put the followers when you arrive.</summary>
    private const int GatherSearch = 40;

    private readonly InputAction _nextLeader = new(new[] { Buttons.RightShoulder }, new[] { Keys.Tab }, true);
    private readonly InputAction[] _pickLeader =
    {
        new(Array.Empty<Buttons>(), new[] { Keys.F1 }, true),
        new(Array.Empty<Buttons>(), new[] { Keys.F2 }, true),
        new(Array.Empty<Buttons>(), new[] { Keys.F3 }, true),
        new(Array.Empty<Buttons>(), new[] { Keys.F4 }, true),
    };

    /// <summary>Each party member's figure, in party order.</summary>
    private readonly List<WalkerSprite> _walkers = new();

    /// <summary>Where your feet have been, oldest first.</summary>
    private readonly List<Vector2> _trail = new();

    /// <summary>For each follower who's catching up, the path they're on and how far along it they are. Anyone not in here is walking the trail.</summary>
    private readonly Dictionary<WalkerSprite, (List<Vector2> Path, int Step)> _catchingUp = new();

    /// <summary>How long each follower has been pushing without getting anywhere.</summary>
    private readonly Dictionary<WalkerSprite, float> _stuck = new();

    /// <summary>Whoever you're controlling.</summary>
    private WalkerSprite Leader => _walkers[_game.Leader];

    /// <summary>Puts you where the save says (or on the map's start, if that's no good), the rest of the party round you, and notes any exit or trigger you're already standing in.</summary>
    /// <remarks>
    /// An old save, or one I've edited by hand, can say somewhere off the map or inside a wall. That
    /// isn't worth an error, so you start on the start instead. A follower keeps their own spot if
    /// it's free and near you, which is what loading a save wants; after an exit they're all on your
    /// spot, so they get put round you. Arriving in a trigger doesn't set it off; you have to walk
    /// out and back in.
    /// </remarks>
    private void Arrive()
    {
        PartyMember you = _game.LeaderMember;
        MapSpawn start = _map.Spawns[MapLoader.StartSpawn];
        bool fits = Fits(you.Feet);
        Leader.Position = fits ? you.Feet : start.Feet;
        Leader.Facing = fits ? you.Facing : start.Facing;

        var homeless = new List<WalkerSprite>();
        BoundingRectangle room = RoomAroundLeader();
        for (int i = 0; i < _walkers.Count; i++)
        {
            if (i == _game.Leader) continue;

            PartyMember member = _game.Party[i];
            WalkerSprite walker = _walkers[i];
            walker.Facing = member.Facing;

            bool keep = Fits(member.Feet) && Vector2.Distance(member.Feet, Leader.Position) <= GatherDistance
                && !FeetAt(member.Feet).CollidesWith(room);
            if (keep) walker.Position = member.Feet;
            else homeless.Add(walker);
        }
        GatherBehind(homeless);
        SeedTrail();

        BoundingRectangle feet = FeetAt(Leader.Position);
        foreach (MapExit exit in _map.Exits)
            if (feet.CollidesWith(exit.Area)) _inside.Add(exit.Id);
        foreach (MapTrigger trigger in _map.Triggers)
            if (feet.CollidesWith(trigger.Area)) _inside.Add(trigger.Id);
    }

    /// <summary>Whether feet could stand at a point: on the map and not inside anything solid.</summary>
    private bool Fits(Vector2 feet) => _tilemap.Bounds.Contains(feet) && !Blocked(FeetAt(feet));

    /// <summary>Puts followers on free tiles round you, behind you if there's room, so they don't arrive in front of you.</summary>
    /// <param name="followers">Who needs putting somewhere.</param>
    private void GatherBehind(List<WalkerSprite> followers)
    {
        if (followers.Count == 0) return;

        Point home = Tilemap.CellOf(Leader.Position);
        Vector2 ahead = Ahead(Leader.Facing);

        // The walkable tiles nearest you, out to GatherSearch of them, by how far they'd be to walk to.
        var found = new List<Point>();
        var seen = new HashSet<Point> { home };
        var queue = new Queue<Point>();
        queue.Enqueue(home);
        while (queue.Count > 0 && found.Count < GatherSearch)
        {
            Point cell = queue.Dequeue();
            if (cell != home) found.Add(cell);

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    var next = new Point(cell.X + dx, cell.Y + dy);
                    if (seen.Add(next) && _nav.IsWalkable(next)) queue.Enqueue(next);
                }
            }
        }

        // Nearest first, and a tile in front of you counts as further than it is.
        found.Sort((a, b) => Nearness(a).CompareTo(Nearness(b)));
        float Nearness(Point cell)
        {
            Vector2 to = NavGrid.Middle(cell) - Leader.Position;
            return to.Length() + (Vector2.Dot(to, ahead) > 0f ? Tilemap.TileSize : 0f);
        }

        // Then a party's spacing apart, from you and from each other, or they arrive in a heap.
        var taken = new List<Vector2> { Leader.Position };
        foreach (WalkerSprite follower in followers)
        {
            Vector2 spot = Leader.Position;
            foreach (Point cell in found)
            {
                Vector2 middle = NavGrid.Middle(cell);
                if (taken.TrueForAll(other => Vector2.Distance(other, middle) >= Spacing))
                {
                    spot = middle;
                    break;
                }
            }

            taken.Add(spot);
            follower.Position = spot;
            follower.IsWalking = false;
        }
    }

    /// <summary>Starts your trail again through where the followers are standing, last one first, so nobody has to move until you do.</summary>
    private void SeedTrail()
    {
        _trail.Clear();
        _catchingUp.Clear();
        _stuck.Clear();

        List<WalkerSprite> followers = Followers();
        for (int i = followers.Count - 1; i >= 0; i--) _trail.Add(followers[i].Position);
        _trail.Add(Leader.Position);
    }

    /// <summary>Everyone but you, in party order.</summary>
    private List<WalkerSprite> Followers()
    {
        var followers = new List<WalkerSprite>();
        for (int i = 0; i < _walkers.Count; i++)
            if (i != _game.Leader) followers.Add(_walkers[i]);
        return followers;
    }

    /// <summary>Adds to your trail if you've moved, then moves each follower toward their spot on it, or along a path if they're catching up.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    private void MoveFollowers(GameTime gameTime)
    {
        if (Vector2.Distance(Leader.Position, _trail[^1]) >= TrailStep)
        {
            _trail.Add(Leader.Position);
            if (_trail.Count > TrailLength) _trail.RemoveAt(0);
        }

        BoundingRectangle room = RoomAroundLeader();
        List<WalkerSprite> followers = Followers();
        for (int k = 0; k < followers.Count; k++)
        {
            WalkerSprite walker = followers[k];
            Vector2 spot = TrailPoint((k + 1) * Spacing);

            if (_catchingUp.TryGetValue(walker, out (List<Vector2> Path, int Step) catching))
            {
                bool going = StepAlong(walker, catching.Path, ref catching.Step, WalkSpeed * CatchUpSpeed, gameTime, room);
                if (!going || Vector2.Distance(walker.Position, spot) < Spacing) _catchingUp.Remove(walker);
                else _catchingUp[walker] = catching;
                continue;
            }

            float distance = Vector2.Distance(walker.Position, spot);
            bool atYou = FeetAt(walker.Position).CollidesWith(Grown(room, 1f));
            if (distance <= FollowSlack || atYou)
            {
                walker.IsWalking = false;
                _stuck[walker] = 0f;
                continue;
            }

            if (distance > CatchUpDistance)
            {
                CatchUp(walker, spot);
                continue;
            }

            Vector2 before = walker.Position;
            float speed = WalkSpeed * (distance > Spacing ? CatchUpSpeed : 1f);
            Walk(walker, (spot - walker.Position) / distance * MathF.Min(1f, distance / FollowEase), speed, gameTime, room);

            float stuck = Vector2.Distance(before, walker.Position) < StuckMovement
                ? _stuck.GetValueOrDefault(walker) + (float)gameTime.ElapsedGameTime.TotalSeconds
                : 0f;
            _stuck[walker] = stuck;
            if (stuck > StuckSeconds) CatchUp(walker, spot);
        }
    }

    /// <summary>Sets a follower finding their way to their spot. If there's no way at all, they're put next to you instead, rather than left behind.</summary>
    private void CatchUp(WalkerSprite walker, Vector2 spot)
    {
        _stuck[walker] = 0f;
        List<Vector2> path = Pathfinder.Find(_nav, walker.Position, spot, Spacing / 2f);
        if (path is null) GatherBehind(new List<WalkerSprite> { walker });
        else _catchingUp[walker] = (path, 0);
    }

    /// <summary>The point a distance back along your trail from where you are now.</summary>
    /// <param name="distance">How far back, in art pixels.</param>
    private Vector2 TrailPoint(float distance)
    {
        Vector2 from = Leader.Position;
        for (int i = _trail.Count - 1; i >= 0; i--)
        {
            float length = Vector2.Distance(from, _trail[i]);
            if (length >= distance) return Vector2.Lerp(from, _trail[i], distance / length);
            distance -= length;
            from = _trail[i];
        }
        return from;
    }

    /// <summary>Walks someone along a path, one point after the next.</summary>
    /// <param name="walker">Who.</param>
    /// <param name="path">The points to go through.</param>
    /// <param name="step">Which point they're heading for. Moves on as they reach each one.</param>
    /// <param name="speed">How fast, in art pixels a second.</param>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="avoid">One more box they can't walk into, or null.</param>
    /// <returns>True while there's still path to walk.</returns>
    private bool StepAlong(WalkerSprite walker, List<Vector2> path, ref int step, float speed, GameTime gameTime, BoundingRectangle? avoid)
    {
        while (step < path.Count && Vector2.Distance(walker.Position, path[step]) <= ArriveDistance) step++;
        if (step >= path.Count)
        {
            walker.IsWalking = false;
            return false;
        }

        // Full speed to every point but the last, and ease into that one.
        Vector2 to = path[step] - walker.Position;
        float distance = to.Length();
        float push = step == path.Count - 1 ? MathF.Min(1f, distance / FollowEase) : 1f;
        Walk(walker, to / distance * push, speed, gameTime, avoid);
        return true;
    }

    /// <summary>Changes who you're controlling. The camera eases over to them, and the old one falls in behind.</summary>
    /// <param name="index">Their place in the party.</param>
    private void SelectLeader(int index)
    {
        if (index == _game.Leader || index < 0 || index >= _walkers.Count) return;

        Halt();
        _game.Leader = index;
        SeedTrail();
        _audio.Play(Sfx.MenuMove);
    }

    /// <summary>Tab (or the right shoulder) for the next in line, F1 to F4 for one in particular.</summary>
    /// <returns>True if it changed who you're controlling.</returns>
    private bool HandleSwitching(InputState input)
    {
        int pick = _game.Leader;
        if (_nextLeader.Occurred(input)) pick = (_game.Leader + 1) % _walkers.Count;
        for (int i = 0; i < _pickLeader.Length && i < _walkers.Count; i++)
            if (_pickLeader[i].Occurred(input)) pick = i;

        if (pick == _game.Leader) return false;
        SelectLeader(pick);
        return true;
    }

    /// <summary>Stops you where you are: no path, nothing waiting to be used at the end of one.</summary>
    private void Halt()
    {
        StopPath();
        Leader.IsWalking = false;
    }

    /// <summary>The room round your feet that followers keep out of.</summary>
    private BoundingRectangle RoomAroundLeader() => Grown(FeetAt(Leader.Position), LeaderRoom);

    /// <summary>A rectangle grown by the same amount on every side.</summary>
    private static BoundingRectangle Grown(BoundingRectangle box, float by) =>
        new(box.X - by, box.Y - by, box.Width + by * 2f, box.Height + by * 2f);

    /// <summary>Which way is in front, for a facing.</summary>
    private static Vector2 Ahead(Direction facing) => facing switch
    {
        Direction.Up => -Vector2.UnitY,
        Direction.Left => -Vector2.UnitX,
        Direction.Right => Vector2.UnitX,
        _ => Vector2.UnitY,
    };

    /// <summary>Everyone's sheet, in party order, for the strip's faces.</summary>
    private List<Texture2D> PartySheets()
    {
        var sheets = new List<Texture2D>(_walkers.Count);
        foreach (WalkerSprite walker in _walkers) sheets.Add(walker.Sheet);
        return sheets;
    }
}
