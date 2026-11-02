using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// A turn-based fight, by 5e's rules: initiative, turns and their budgets, moving on the grid,
/// attacks, spells, conditions, going down and getting up, and who wins.
/// </summary>
/// <remarks>
/// The Black Box's RoundEngine pattern, grown up: it draws nothing, takes its Random, and every
/// command hands back what it caused as CombatEvents, in order, for the screen to play out at its
/// own pace. Nothing in here waits on time, so the simulator can fight a thousand of these headless
/// and get the same answer twice from the same seed. The rules it follows are spec 6.7's and the
/// SRD 5.1's; where they differ (Help gets a Downed ally up, 6.7's way) the spec wins. The parts are
/// split by concern: turns, movement, abilities, attacks, and health.
/// </remarks>
public sealed partial class CombatEngine
{
    private readonly EncounterDefinition _encounter;
    private readonly ICombatGrid _grid;
    private readonly Random _random;
    private readonly List<Combatant> _combatants = new();

    /// <summary>Everyone who's rolled initiative, highest first, the dead included so the turn index stays put.</summary>
    private readonly List<Combatant> _order = new();

    /// <summary>The same without the dead, which is what the initiative bar shows.</summary>
    private readonly List<Combatant> _living = new();

    /// <summary>What the command being run has caused so far.</summary>
    private readonly List<CombatEvent> _events = new();

    /// <summary>The foes who've given up, so it's only said once.</summary>
    private readonly HashSet<string> _yielded = new();

    /// <summary>Where the turn is in _order, or -1 between rounds.</summary>
    private int _turn = -1;

    /// <summary>How many of the encounter's waves have arrived.</summary>
    private int _wave;

    /// <summary>Counts up on every hit and every turn's end, so the Soul-Link can tell what happened since when.</summary>
    private int _clock;

    /// <summary>Counts every turn started, so once-a-turn things (Sneak Attack) know which turn they were used on.</summary>
    private int _turnNumber;

    /// <summary>Whether Begin has been called.</summary>
    private bool _begun;

    /// <summary>Sets a fight up: everyone on their tiles, nobody's rolled yet. Begin starts it.</summary>
    /// <remarks>Anyone whose tile is blocked or taken goes on the nearest free one. A party member already at 0 hit points starts Downed and stable.</remarks>
    /// <param name="encounter">The fight.</param>
    /// <param name="combatants">Who's in it at the start, and where.</param>
    /// <param name="grid">The ground it's on.</param>
    /// <param name="random">Where every roll comes from.</param>
    public CombatEngine(EncounterDefinition encounter, IReadOnlyList<CombatantSetup> combatants, ICombatGrid grid, Random random)
    {
        _encounter = encounter;
        _grid = grid;
        _random = random;

        foreach (CombatantSetup setup in combatants)
        {
            Combatant joined = Join(setup);
            if (joined.Member is null || joined.Hp > 0) continue;
            joined.Add(new ActiveCondition(ConditionId.Downed, 0, null, null, 0, DiceExpression.None));
            joined.Add(new ActiveCondition(ConditionId.Stable, 0, null, null, 0, DiceExpression.None));
        }

        SoulLink = new SoulLink(Find(Origins.You) is { IsDuo: true } && Find(Origins.SoulBound) is { IsDuo: true });
    }

    /// <summary>The party's shared bag, for potions and alchemist's fire, or null if there's nothing to draw on.</summary>
    public Inventory Bag { get; init; }

    /// <summary>The fight being fought.</summary>
    public EncounterDefinition Encounter => _encounter;

    /// <summary>Everyone who's been in the fight, in the order they joined.</summary>
    public IReadOnlyList<Combatant> Combatants => _combatants;

    /// <summary>Everyone still in it, in initiative order: the initiative bar.</summary>
    public IReadOnlyList<Combatant> Order => _living;

    /// <summary>Whose turn it is, or null before Begin.</summary>
    public Combatant Current => _turn >= 0 && _turn < _order.Count ? _order[_turn] : null;

    /// <summary>The round, from 1.</summary>
    public int Round { get; private set; }

    /// <summary>How the fight stands.</summary>
    public CombatOutcome Outcome { get; private set; }

    /// <summary>The duo's meter.</summary>
    public SoulLink SoulLink { get; }

    /// <summary>Whether anything solid is in the way of a straight line between two tiles.</summary>
    /// <remarks>Bresenham's line from the middle of one to the middle of the other, not counting the two ends, so someone standing in a doorway can be seen.</remarks>
    /// <param name="from">One tile.</param>
    /// <param name="to">The other.</param>
    public bool CanSee(Point from, Point to)
    {
        int x = from.X, y = from.Y;
        int dx = Math.Abs(to.X - x), dy = -Math.Abs(to.Y - y);
        int stepX = x < to.X ? 1 : -1, stepY = y < to.Y ? 1 : -1;
        int error = dx + dy;

        while (x != to.X || y != to.Y)
        {
            int twice = 2 * error;
            if (twice >= dy)
            {
                error += dy;
                x += stepX;
            }
            if (twice <= dx)
            {
                error += dx;
                y += stepY;
            }
            if ((x != to.X || y != to.Y) && _grid.BlocksSight(new Point(x, y))) return false;
        }
        return true;
    }

    /// <summary>Someone in the fight by id, or null.</summary>
    /// <param name="id">Who.</param>
    internal Combatant Find(string id)
    {
        foreach (Combatant each in _combatants)
            if (each.Id == id) return each;
        return null;
    }

    /// <summary>Whether two people are on opposite sides.</summary>
    internal static bool Hostile(Combatant a, Combatant b) => a.Side != b.Side;

    /// <summary>Whether a foe's beaten: down, dead, or hurt past the point where it gives up.</summary>
    /// <param name="who">Who. Party members are never beaten this way; they're down or they aren't.</param>
    internal bool Beaten(Combatant who) =>
        who.IsOut || who.Side == Side.Foes && _encounter.YieldsAt > 0f && who.Hp <= _encounter.YieldsAt * who.MaxHp;

    /// <summary>Whoever's standing (or lying) on a tile, the dead aside, or null.</summary>
    /// <param name="cell">The tile.</param>
    /// <param name="except">Someone to ignore, usually whoever's asking.</param>
    internal Combatant At(Point cell, Combatant except = null)
    {
        foreach (Combatant each in _combatants)
            if (each != except && !each.IsDead && each.Cell == cell) return each;
        return null;
    }

    /// <summary>The other half of the duo, or null.</summary>
    /// <param name="who">You or the Soul-Bound.</param>
    internal Combatant Partner(Combatant who) =>
        who.IsDuo ? Find(who.Id == Origins.You ? Origins.SoulBound : Origins.You) : null;

    /// <summary>Puts someone in the fight on the nearest free tile to the one they're given.</summary>
    private Combatant Join(CombatantSetup setup)
    {
        var joined = new Combatant(setup, NearestFree(setup.Cell), _combatants.Count);
        _combatants.Add(joined);
        return joined;
    }

    /// <summary>The nearest tile to one that can be stood on and has nobody on it, kept on the grid.</summary>
    /// <remarks>A breadth-first search out from it, so a foe spawning where a crate is ends up beside the crate.</remarks>
    private Point NearestFree(Point cell)
    {
        var start = new Point(Math.Clamp(cell.X, 0, _grid.Width - 1), Math.Clamp(cell.Y, 0, _grid.Height - 1));
        var seen = new HashSet<Point> { start };
        var queue = new Queue<Point>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Point at = queue.Dequeue();
            if (_grid.Walkable(at) && At(at) is null) return at;

            foreach (Point step in Steps)
            {
                var next = new Point(at.X + step.X, at.Y + step.Y);
                if (next.X >= 0 && next.Y >= 0 && next.X < _grid.Width && next.Y < _grid.Height && seen.Add(next)) queue.Enqueue(next);
            }
        }
        return start;
    }

    /// <summary>Rebuilds the initiative bar without the dead.</summary>
    private void RefreshOrder()
    {
        _living.Clear();
        foreach (Combatant each in _order)
            if (!each.IsDead) _living.Add(each);
    }

    /// <summary>Notes something that happened, for the command's caller.</summary>
    private void Emit(CombatEventKind kind, Combatant actor, Combatant target, int amount = 0, string text = null, RollResult roll = null,
        AbilityId? ability = null, DamageType? damageType = null)
    {
        Point from = actor?.Cell ?? target?.Cell ?? Point.Zero;
        Point to = target?.Cell ?? from;
        _events.Add(new CombatEvent(kind, actor?.Id, target?.Id, amount, text, roll, from, to, ability, damageType));
    }

    /// <summary>Hands back everything the command caused, and starts the list again.</summary>
    private IReadOnlyList<CombatEvent> Flush()
    {
        CombatEvent[] happened = _events.ToArray();
        _events.Clear();
        return happened;
    }
}
