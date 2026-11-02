using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.World;

namespace LegendQuest.Combat;

/// <summary>
/// Moving on the grid: paths and what they cost, who they'd provoke, and walking them one tile at a
/// time with the opportunity attacks on the way.
/// </summary>
/// <remarks>
/// A tile a step, diagonals included (Measure). Walls and corners block the way the explore screen's
/// Pathfinder does, so nobody cuts past the corner of a wall. Foes block you; allies you can walk
/// through but not stop on, as in 5e.
/// </remarks>
public sealed partial class CombatEngine
{
    /// <summary>What the search adds to a diagonal step, so of two paths the same length it picks the one with fewer of them and nobody zigzags.</summary>
    private const float DiagonalTieBreak = 0.001f;

    /// <summary>The eight steps from a tile: the four straight ones first, then the diagonals.</summary>
    private static readonly Point[] Steps =
    {
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        new(1, 1), new(-1, 1), new(1, -1), new(-1, -1),
    };

    /// <summary>The shortest way for whoever's turn it is to a tile, however far, so the part past their movement can be shown.</summary>
    /// <param name="cell">Where to.</param>
    /// <returns>The tiles to step through, not counting where they are, ending on the tile; empty if there's no way, or nobody to move.</returns>
    public IReadOnlyList<Point> PathTo(Point cell)
    {
        if (Current is not Combatant mover || Outcome != CombatOutcome.Ongoing) return Array.Empty<Point>();
        return (IReadOnlyList<Point>)FindPath(mover, mover.Cell, cell) ?? Array.Empty<Point>();
    }

    /// <summary>What a path costs in metres.</summary>
    /// <param name="path">The tiles, as PathTo gives them.</param>
    public float Cost(IReadOnlyList<Point> path) => path.Count * Measure.TileMetres;

    /// <summary>Walks whoever's turn it is along a path, as far as their movement goes and they can stop, taking opportunity attacks as they leave each foe's reach.</summary>
    /// <param name="path">The tiles, as PathTo gives them.</param>
    /// <returns>Every step, and every attack on the way.</returns>
    public IReadOnlyList<CombatEvent> Move(IReadOnlyList<Point> path)
    {
        Walk(path);
        return Flush();
    }

    /// <summary>The walk itself, leaving the events for the caller to hand back.</summary>
    /// <remarks>
    /// It works out first how far along it can get (its movement, the rules about each step, and a
    /// free tile to stop on), then goes. If an opportunity attack drops them on the way, they stop where
    /// they fell; if that's an ally's tile, they're moved off it to the nearest free one.
    /// </remarks>
    internal void Walk(IReadOnlyList<Point> path)
    {
        if (Current is not Combatant mover || Outcome != CombatOutcome.Ongoing || mover.IsOut || path is null) return;

        int last = -1;
        float metres = 0f;
        Point at = mover.Cell;
        for (int i = 0; i < path.Count; i++)
        {
            Point next = path[i];
            metres += Measure.TileMetres;
            if (Measure.Steps(at, next) != 1 || !StepAllowed(mover, at, next) || !mover.Budget.CanWalk(metres)) break;
            if (At(next, mover) is null) last = i;
            at = next;
        }

        at = mover.Cell;
        for (int i = 0; i <= last; i++)
        {
            Point next = path[i];
            if (!mover.Has(ConditionId.Disengaged))
            {
                foreach (Combatant threat in Opportunists(mover, at, next).ToArray())
                {
                    if (mover.IsOut || Outcome != CombatOutcome.Ongoing) break;
                    OpportunityAttack(threat, mover);
                }
            }
            if (mover.IsOut || Outcome != CombatOutcome.Ongoing) break;

            mover.Budget.Walk(Measure.TileMetres);
            mover.Facing = FacingOf(at, next, mover.Facing);
            mover.Cell = next;
            _events.Add(new CombatEvent(CombatEventKind.Moved, mover.Id, null, 0, null, null, at, next, null, null));
            at = next;
        }

        if (At(mover.Cell, mover) is not null) mover.Cell = NearestFree(mover.Cell);
    }

    /// <summary>The shortest path for someone from one tile to another: A*, a step a tile, through allies but not foes, never stopping on anybody.</summary>
    /// <returns>The tiles after the start, or null if there's no way.</returns>
    internal List<Point> FindPath(Combatant mover, Point from, Point to)
    {
        if (from == to || !_grid.Walkable(to) || At(to, mover) is not null) return null;

        int width = _grid.Width, height = _grid.Height;
        var cost = new float[width * height];
        var came = new int[width * height];
        var closed = new bool[width * height];
        Array.Fill(cost, float.MaxValue);

        var open = new BinaryHeap<Point>();
        cost[from.Y * width + from.X] = 0f;
        came[from.Y * width + from.X] = -1;
        open.Push(from, Measure.Steps(from, to));

        while (open.Count > 0)
        {
            Point cell = open.Pop();
            int here = cell.Y * width + cell.X;
            if (closed[here]) continue;
            closed[here] = true;

            if (cell == to) return Trace(came, here, width);

            foreach (Point step in Steps)
            {
                var next = new Point(cell.X + step.X, cell.Y + step.Y);
                if (!StepAllowed(mover, cell, next)) continue;

                int there = next.Y * width + next.X;
                float through = cost[here] + 1f + (step.X != 0 && step.Y != 0 ? DiagonalTieBreak : 0f);
                if (closed[there] || through >= cost[there]) continue;

                cost[there] = through;
                came[there] = here;
                open.Push(next, through + Measure.Steps(next, to));
            }
        }
        return null;
    }

    /// <summary>Everywhere someone could walk to this turn, and the way there: a breadth-first flood out to their movement, for the AI.</summary>
    /// <param name="mover">Who.</param>
    /// <param name="metres">How far they can go.</param>
    /// <returns>For each tile reached, the tile it was reached from. Their own tile maps to itself. Tiles with someone on are in it, to walk through, but can't be stopped on.</returns>
    internal Dictionary<Point, Point> Flood(Combatant mover, float metres)
    {
        var came = new Dictionary<Point, Point> { [mover.Cell] = mover.Cell };
        var frontier = new List<Point> { mover.Cell };

        for (int steps = Measure.Tiles(metres); steps > 0 && frontier.Count > 0; steps--)
        {
            var next = new List<Point>();
            foreach (Point cell in frontier)
            {
                foreach (Point step in Steps)
                {
                    var to = new Point(cell.X + step.X, cell.Y + step.Y);
                    if (came.ContainsKey(to) || !StepAllowed(mover, cell, to)) continue;
                    came[to] = cell;
                    next.Add(to);
                }
            }
            frontier = next;
        }
        return came;
    }

    /// <summary>The path to a tile out of a Flood.</summary>
    /// <returns>The tiles after the start, in order.</returns>
    internal static List<Point> PathIn(Dictionary<Point, Point> came, Point to)
    {
        var path = new List<Point>();
        for (Point at = to; came[at] != at; at = came[at]) path.Add(at);
        path.Reverse();
        return path;
    }

    /// <summary>Everyone who'd take a swing at someone walking a path from a tile, each once, in the order they'd swing.</summary>
    internal List<Combatant> ThreatsAlong(Combatant mover, Point start, IReadOnlyList<Point> path)
    {
        var threats = new List<Combatant>();
        if (mover.Has(ConditionId.Disengaged)) return threats;

        Point at = start;
        foreach (Point next in path)
        {
            foreach (Combatant threat in Opportunists(mover, at, next))
                if (!threats.Contains(threat)) threats.Add(threat);
            at = next;
        }
        return threats;
    }

    /// <summary>Whether a step can be taken: onto the grid and something that can be stood on, not past the corner of a wall, and not into a foe.</summary>
    private bool StepAllowed(Combatant mover, Point from, Point to)
    {
        if (!_grid.Walkable(to)) return false;
        if (from.X != to.X && from.Y != to.Y && (!_grid.Walkable(new Point(to.X, from.Y)) || !_grid.Walkable(new Point(from.X, to.Y)))) return false;
        return At(to, mover) is not Combatant there || !Hostile(there, mover);
    }

    /// <summary>The foes whose reach a step leaves, who are up and have their reaction.</summary>
    private IEnumerable<Combatant> Opportunists(Combatant mover, Point from, Point to)
    {
        foreach (Combatant each in _combatants)
        {
            if (!Hostile(each, mover) || Beaten(each) || !each.Budget.Reaction) continue;
            if (Measure.Within(each.Cell, from, each.Melee.Range) && !Measure.Within(each.Cell, to, each.Melee.Range)) yield return each;
        }
    }

    /// <summary>An opportunity attack: the reaction spent, and one swing with their best melee attack.</summary>
    private void OpportunityAttack(Combatant attacker, Combatant mover)
    {
        attacker.Budget.Spend(ActionCost.Reaction);
        attacker.Facing = FacingOf(attacker.Cell, mover.Cell, attacker.Facing);
        Emit(CombatEventKind.OpportunityAttack, attacker, mover, 0, $"{attacker.Label} lashes out as {mover.Label} pulls away.", ability: AbilityId.Attack);
        Strike(attacker, attacker.Melee, mover, AbilityCatalog.Get(AbilityId.Attack), false);
    }

    /// <summary>Which way someone faces looking from one tile to another: sideways when it's at least as far across as up or down.</summary>
    /// <param name="from">Where they are.</param>
    /// <param name="to">What they're looking at.</param>
    /// <param name="otherwise">Which way they face if it's the same tile.</param>
    internal static Direction FacingOf(Point from, Point to, Direction otherwise)
    {
        int dx = to.X - from.X, dy = to.Y - from.Y;
        if (dx == 0 && dy == 0) return otherwise;
        if (Math.Abs(dx) >= Math.Abs(dy)) return dx > 0 ? Direction.Right : Direction.Left;
        return dy > 0 ? Direction.Down : Direction.Up;
    }

    /// <summary>Walks a path back from its end, and turns it round, leaving off the start.</summary>
    private static List<Point> Trace(int[] came, int end, int width)
    {
        var cells = new List<Point>();
        for (int i = end; came[i] >= 0; i = came[i]) cells.Add(new Point(i % width, i / width));
        cells.Reverse();
        return cells;
    }
}
