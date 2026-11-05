using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// Walking in a side-on room: where feet can stand on its floor band, moving along it, the paths
/// the mouse and the followers take, and finding somewhere on it to stand.
/// </summary>
/// <remarks>
/// <para>
/// Feet are a point on the floor with a small box over it, as they were on the tile maps. The point
/// has to stay on the floor (inside the band, outside its ellipses), and the box out of the
/// footprints of whatever's standing in the room: the props, the shut doors, and anyone who stands
/// still. Moving goes one axis at a time, as The Black Box's lobby did: a footprint pushes the feet
/// back out right against its edge, and the edge of the floor turns a step that would leave it into
/// a slide along it, by looking a little either side for floor, so walking into a sloping back edge
/// or round a fountain carries you along it instead of stopping you dead.
/// </para>
/// <para>
/// Paths go over a NavGrid of the room in Cell-sized cells: a cell's walkable if its middle is on the
/// floor and no footprint, grown by the feet box, covers it. Growing the footprints is what keeps a
/// path's feet clear of them, so a straight line only has to be looked along, not either side of it.
/// Not from a tutorial.
/// </para>
/// </remarks>
public sealed class RoomWalker
{
    /// <summary>How big a cell of the path grid is, in world pixels: 32 canvas pixels, so a band is a few dozen cells deep.</summary>
    public const float Cell = 8f;

    /// <summary>The feet box, in world pixels: what collides. Narrower than a person, so two can pass in the band.</summary>
    public static readonly Vector2 FeetSize = new(10f, 4f);

    /// <summary>How far off a footprint the feet get left. The tutorial's rectangles count touching as colliding, so it can't be exactly 0.</summary>
    private const float CollisionGap = 0.01f;

    /// <summary>How far either side of a step that would leave the floor to look for floor to slide onto, as a share of the step, and in how many tries: a slope steeper than this stops you.</summary>
    private const float SlideReach = 1.5f;
    private const int SlideTries = 4;

    /// <summary>How far inside the floor's edge a point put back on it goes, in world pixels.</summary>
    private const float EdgeInset = 0.5f;

    /// <summary>How many cells round a point that isn't a path's to look for one that is.</summary>
    private const int AimSearch = 2;

    /// <summary>How much further a spot deeper or nearer counts as, against one along the floor, when spreading people out round someone: they line up along the room.</summary>
    private const float DepthWeight = 2f;

    /// <summary>How many tries a stroll gets to land on the floor before staying home.</summary>
    private const int StrollTries = 6;

    /// <summary>The nearest a stroll goes from home, as a share of how far it can go, so a stroll is a walk and not a shuffle.</summary>
    private const float StrollLeast = 0.3f;

    /// <summary>The furthest a stroll goes deeper or nearer, as a share of how far it can go: the floor's shallow, so strolls go along it.</summary>
    public const float StrollDepth = 0.15f;

    private readonly RoomDefinition _room;
    private readonly NavGrid _nav;
    private readonly bool[,] _floor;
    private readonly List<BoundingRectangle> _standing = new();

    /// <summary>Where the floor starts and ends across the room, in world pixels.</summary>
    private readonly float _left;
    private readonly float _right;

    /// <summary>Lays the path grid over a room's floor. Nothing's standing on it until Rebuild.</summary>
    /// <param name="room">The room.</param>
    public RoomWalker(RoomDefinition room)
    {
        _room = room;

        int width = (int)MathF.Ceiling(room.Width / Cell), height = (int)MathF.Ceiling(ZoneCatalog.RoomHeight / Cell);
        _floor = new bool[width, height];
        var grid = new NavGrid(width, height, Cell, Vector2.Zero, 0f, cell => !_floor[cell.X, cell.Y]);
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _floor[x, y] = room.OnFloor(grid.MiddleOf(new Point(x, y)));
        _nav = grid;
        _nav.Build(Array.Empty<BoundingRectangle>());

        _left = float.MaxValue;
        _right = float.MinValue;
        foreach (Vector2 point in room.Floor)
        {
            _left = MathF.Min(_left, point.X);
            _right = MathF.Max(_right, point.X);
        }
    }

    /// <summary>The feet box with the feet at a point: as wide as FeetSize, and as deep up from the point.</summary>
    /// <param name="feet">The point, in world pixels.</param>
    public static BoundingRectangle FeetAt(Vector2 feet) =>
        new(feet.X - FeetSize.X / 2f, feet.Y - FeetSize.Y, FeetSize.X, FeetSize.Y);

    /// <summary>Says what's standing in the room now, which nobody walks through and the paths go round. Cheap enough to do whenever a door opens or anyone comes or goes.</summary>
    /// <param name="standing">The footprints of everything standing still: props, shut doors, and people who don't walk about.</param>
    public void Rebuild(IEnumerable<BoundingRectangle> standing)
    {
        _standing.Clear();
        _standing.AddRange(standing);

        var grown = new List<BoundingRectangle>(_standing.Count);
        foreach (BoundingRectangle footprint in _standing)
            grown.Add(new BoundingRectangle(footprint.X - FeetSize.X / 2f, footprint.Y, footprint.Width + FeetSize.X, footprint.Height + FeetSize.Y));
        _nav.Build(grown);
    }

    /// <summary>Whether feet could stand at a point: on the floor, and clear of everything standing.</summary>
    /// <param name="feet">The point, in world pixels.</param>
    public bool Fits(Vector2 feet) => _room.OnFloor(feet) && Clear(feet, null);

    /// <summary>Moves feet a step, across and then in depth, pushed back out of anything they walk into and slid along the edge of the floor.</summary>
    /// <remarks>Feet that are off the floor to start with (put there by a scene) move freely until they're back on it, so nobody's ever stuck.</remarks>
    /// <param name="from">Where the feet are, in world pixels.</param>
    /// <param name="step">How far to move them, in world pixels.</param>
    /// <param name="avoid">One more box they can't walk into, or null. Followers keep out of your feet with it.</param>
    /// <returns>Where they end up.</returns>
    public Vector2 Move(Vector2 from, Vector2 step, BoundingRectangle? avoid)
    {
        Vector2 at = from;
        if (step.X != 0f) at = Step(at, new Vector2(step.X, 0f), avoid);
        if (step.Y != 0f) at = Step(at, new Vector2(0f, step.Y), avoid);
        return at;
    }

    /// <summary>Finds a way along the floor round everything standing, to a point or to within reach of it.</summary>
    /// <param name="from">Where the feet are now, in world pixels.</param>
    /// <param name="to">Where to go.</param>
    /// <param name="reach">How close is close enough, in world pixels. 0 means the point itself, or as near it as a path gets if the feet don't fit there.</param>
    /// <returns>The points to walk through, in order, not counting where you are; or null if there's no way there.</returns>
    public List<Vector2> FindPath(Vector2 from, Vector2 to, float reach)
    {
        if (reach > 0f) return Pathfinder.Find(_nav, from, to, reach);
        if (PathCellNear(to) is not Vector2 aim) return null;

        List<Vector2> path = Pathfinder.Find(_nav, from, aim, 0f);
        if (path is not null && Fits(to)) path[^1] = to;
        return path;
    }

    /// <summary>The nearest point to one that's on the floor: the point itself if it is, else straight up or down onto the band, else the middle of the nearest cell of floor.</summary>
    /// <param name="point">The point, in world pixels.</param>
    public Vector2 NearestOnFloor(Vector2 point)
    {
        if (_room.OnFloor(point)) return point;

        float x = Math.Clamp(point.X, _left + EdgeInset, _right - EdgeInset);
        float back = _room.BackEdgeAt(x) + EdgeInset, front = _room.FrontEdgeAt(x) - EdgeInset;
        var onto = new Vector2(x, MathF.Max(back, MathF.Min(point.Y, front)));
        if (_room.OnFloor(onto)) return onto;

        // In a fountain, or round a bend in the band.
        Vector2 best = onto;
        float nearest = float.MaxValue;
        for (int y = 0; y < _nav.Height; y++)
        {
            for (int column = 0; column < _nav.Width; column++)
            {
                if (!_floor[column, y]) continue;
                Vector2 middle = _nav.MiddleOf(new Point(column, y));
                float distance = Vector2.DistanceSquared(middle, point);
                if (distance >= nearest) continue;
                nearest = distance;
                best = middle;
            }
        }
        return best;
    }

    /// <summary>Spots for people to stand round someone: on the floor, clear of everything, a spacing from them and from each other, nearest first and behind them before in front, along the room before deeper or nearer.</summary>
    /// <param name="at">Where they are, in world pixels.</param>
    /// <param name="spacing">How far apart, in world pixels.</param>
    /// <param name="count">How many spots.</param>
    /// <param name="facing">Which way they face, so the spots go behind them. Only left and right count.</param>
    /// <param name="taken">Spots already spoken for, which the new ones keep a spacing from too, or null.</param>
    /// <returns>Count spots; any there's no room for are on the point itself.</returns>
    public List<Vector2> SpotsNear(Vector2 at, float spacing, int count, Direction facing, IEnumerable<Vector2> taken = null)
    {
        float ahead = facing switch { Direction.Left => -1f, Direction.Right => 1f, _ => 0f };
        float Nearness(Vector2 spot)
        {
            Vector2 to = spot - at;
            return MathF.Abs(to.X) + MathF.Abs(to.Y) * DepthWeight + (to.X * ahead > 0f ? spacing * 2f : 0f);
        }

        // Every free cell near enough along the room that they'd all fit in a line either side.
        float across = spacing * (count + 2);
        Point first = _nav.CellOf(new Vector2(at.X - across, 0f)), last = _nav.CellOf(new Vector2(at.X + across, 0f));
        var found = new List<Vector2>();
        for (int y = 0; y < _nav.Height; y++)
        {
            for (int x = Math.Max(first.X, 0); x <= Math.Min(last.X, _nav.Width - 1); x++)
            {
                var cell = new Point(x, y);
                if (_nav.IsWalkable(cell)) found.Add(_nav.MiddleOf(cell));
            }
        }
        found.Sort((a, b) => Nearness(a).CompareTo(Nearness(b)));

        var kept = new List<Vector2> { at };
        if (taken is not null) kept.AddRange(taken);
        var spots = new List<Vector2>(count);
        for (int i = 0; i < count; i++)
        {
            Vector2 spot = at;
            foreach (Vector2 middle in found)
            {
                if (!kept.TrueForAll(other => Vector2.Distance(other, middle) >= spacing)) continue;
                spot = middle;
                break;
            }
            kept.Add(spot);
            spots.Add(spot);
        }
        return spots;
    }

    /// <summary>Somewhere to stroll to from home: mostly along the room, a little deeper or nearer, and on the floor clear of everything.</summary>
    /// <param name="home">Where they were put, in world pixels.</param>
    /// <param name="range">How far a stroll can go, in world pixels.</param>
    /// <param name="random">The dice, theirs so they stroll the same every time.</param>
    /// <returns>Where to go, or home if StrollTries found nowhere.</returns>
    public Vector2 Stroll(Vector2 home, float range, Random random)
    {
        for (int i = 0; i < StrollTries; i++)
        {
            float across = range * (StrollLeast + (1f - StrollLeast) * (float)random.NextDouble()) * (random.Next(2) == 0 ? -1f : 1f);
            float deeper = range * StrollDepth * ((float)random.NextDouble() * 2f - 1f);
            Vector2 to = home + new Vector2(across, deeper);
            if (Fits(to)) return to;
        }
        return home;
    }

    /// <summary>Moves feet a step along one axis: out of anything they walk into, and along the edge of the floor if the step would leave it.</summary>
    private Vector2 Step(Vector2 from, Vector2 step, BoundingRectangle? avoid)
    {
        Vector2 to = PushOut(from + step, step, avoid);
        if (!_room.OnFloor(from) || _room.OnFloor(to)) return to;

        Vector2 side = step.X != 0f ? Vector2.UnitY : Vector2.UnitX;
        float reach = step.Length() * SlideReach;
        for (int i = 1; i <= SlideTries; i++)
        {
            Vector2 by = side * (reach * i / SlideTries);
            if (_room.OnFloor(to + by) && Clear(to + by, avoid)) return to + by;
            if (_room.OnFloor(to - by) && Clear(to - by, avoid)) return to - by;
        }
        return from;
    }

    /// <summary>Pushes the feet back out of anything they've walked into along a step, right up against its edge.</summary>
    private Vector2 PushOut(Vector2 at, Vector2 step, BoundingRectangle? avoid)
    {
        foreach (BoundingRectangle solid in Solids(avoid))
        {
            if (!FeetAt(at).CollidesWith(solid)) continue;

            if (step.X > 0f) at.X = solid.Left - FeetSize.X / 2f - CollisionGap;
            else if (step.X < 0f) at.X = solid.Right + FeetSize.X / 2f + CollisionGap;
            else if (step.Y > 0f) at.Y = solid.Top - CollisionGap;
            else at.Y = solid.Bottom + FeetSize.Y + CollisionGap;
        }
        return at;
    }

    /// <summary>Whether the feet box at a point is clear of everything standing, and of one more box if there is one.</summary>
    private bool Clear(Vector2 feet, BoundingRectangle? avoid)
    {
        BoundingRectangle box = FeetAt(feet);
        foreach (BoundingRectangle solid in Solids(avoid))
            if (box.CollidesWith(solid)) return false;
        return true;
    }

    /// <summary>Everything standing, and one more box if there is one.</summary>
    private IEnumerable<BoundingRectangle> Solids(BoundingRectangle? avoid)
    {
        foreach (BoundingRectangle footprint in _standing) yield return footprint;
        if (avoid is BoundingRectangle extra) yield return extra;
    }

    /// <summary>The middle of the path cell a point's in, or of the nearest one to it within AimSearch cells; null if there's none.</summary>
    private Vector2? PathCellNear(Vector2 point)
    {
        Point centre = _nav.CellOf(point);
        Vector2? best = null;
        float nearest = float.MaxValue;
        for (int dy = -AimSearch; dy <= AimSearch; dy++)
        {
            for (int dx = -AimSearch; dx <= AimSearch; dx++)
            {
                var cell = new Point(centre.X + dx, centre.Y + dy);
                if (!_nav.IsWalkable(cell)) continue;

                Vector2 middle = _nav.MiddleOf(cell);
                float distance = Vector2.DistanceSquared(middle, point);
                if (distance >= nearest) continue;
                nearest = distance;
                best = middle;
            }
        }
        return best;
    }
}
