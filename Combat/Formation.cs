using System;
using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// Who stands where in a fight: two sides, a front row and a back row each, slots in lanes, and a
/// big foe across two of them.
/// </summary>
/// <remarks>
/// Standing means not out, not beaten and not yielded:
/// a Downed party member keeps their slot but screens nobody, and the dead, the yielded and guests
/// who've fallen back are taken off. The engine moves people; the screens and EnemyMind only ask.
/// Everything that lists people lists them in slot order (lane by lane, front before back), so the
/// same fight always reads the same way.
/// </remarks>
public sealed class Formation
{
    /// <summary>Who's in each slot. A big foe is in two.</summary>
    private readonly Dictionary<Slot, Combatant> _at = new();

    /// <summary>Whether someone counts as standing, which the engine knows (a foe that's given up doesn't).</summary>
    private readonly Func<Combatant, bool> _standing;

    /// <summary>How many looks (Try) are open, so a look inside a look still puts everything back.</summary>
    private int _looks;

    /// <summary>An empty field, for the engine to fill.</summary>
    /// <param name="standing">Whether someone counts as standing.</param>
    internal Formation(Func<Combatant, bool> standing)
    {
        _standing = standing;
    }

    /// <summary>Whoever's in a slot, or null. A big foe answers for both of its slots.</summary>
    /// <param name="slot">Which slot.</param>
    public Combatant At(Slot slot) => _at.TryGetValue(slot, out Combatant who) ? who : null;

    /// <summary>The slots someone's in: one, or two side by side for a big foe, left first; none once they're off the field.</summary>
    /// <param name="who">Who.</param>
    public IReadOnlyList<Slot> SlotsOf(Combatant who)
    {
        var slots = new List<Slot>(2);
        if (who is null) return slots;
        foreach (Slot slot in AllSlots(who.Side))
            if (At(slot) == who) slots.Add(slot);
        return slots;
    }

    /// <summary>Everyone in one of a side's rows, left to right, a big foe once.</summary>
    /// <param name="side">Which side.</param>
    /// <param name="row">Which row.</param>
    public IEnumerable<Combatant> InRow(Side side, Row row)
    {
        var seen = new List<Combatant>();
        for (int lane = 0; lane < Slot.LanesOf(side); lane++)
            if (At(new Slot(side, row, lane)) is Combatant who && !seen.Contains(who)) seen.Add(who);
        return seen;
    }

    /// <summary>A side's effective front row: Front, or Back when nobody standing is in front.</summary>
    /// <param name="side">Which side.</param>
    public Row FrontOf(Side side)
    {
        foreach (Combatant who in InRow(side, Row.Front))
            if (_standing(who)) return Row.Front;
        return Row.Back;
    }

    /// <summary>Whether someone's in their side's effective front row.</summary>
    /// <param name="who">Who.</param>
    public bool InFront(Combatant who)
    {
        IReadOnlyList<Slot> slots = SlotsOf(who);
        return slots.Count > 0 && slots[0].Row == FrontOf(who.Side);
    }

    /// <summary>Whoever's beside someone in their row: the lanes either side of them, or of both their slots for a big foe.</summary>
    /// <param name="who">Who.</param>
    public IEnumerable<Combatant> Neighbours(Combatant who)
    {
        var beside = new List<Combatant>();
        IReadOnlyList<Slot> slots = SlotsOf(who);
        if (slots.Count == 0) return beside;

        Slot left = slots[0], right = slots[^1];
        AddOnce(beside, At(left with { Lane = left.Lane - 1 }), who);
        AddOnce(beside, At(right with { Lane = right.Lane + 1 }), who);
        return beside;
    }

    /// <summary>Whoever's next to someone: their Neighbours, and whoever's in the same lane in the other row.</summary>
    /// <param name="who">Who.</param>
    public IEnumerable<Combatant> NextTo(Combatant who)
    {
        var near = new List<Combatant>(Neighbours(who));
        foreach (Slot slot in SlotsOf(who))
            AddOnce(near, At(slot with { Row = Other(slot.Row) }), who);
        return near;
    }

    /// <summary>Whoever's in one of a side's lanes, front then back.</summary>
    /// <param name="side">Which side.</param>
    /// <param name="lane">Which lane, from 0 at the left.</param>
    public IEnumerable<Combatant> Column(Side side, int lane)
    {
        var column = new List<Combatant>(2);
        AddOnce(column, At(new Slot(side, Row.Front, lane)), null);
        AddOnce(column, At(new Slot(side, Row.Back, lane)), null);
        return column;
    }

    /// <summary>A side's empty slots, the front row first, left to right.</summary>
    /// <param name="side">Which side.</param>
    public IReadOnlyList<Slot> Free(Side side)
    {
        var free = new List<Slot>();
        foreach (Slot slot in AllSlots(side))
            if (At(slot) is null) free.Add(slot);
        return free;
    }

    /// <summary>The other row: Back for Front, Front for Back.</summary>
    /// <param name="row">A row.</param>
    public static Row Other(Row row) => row == Row.Front ? Row.Back : Row.Front;

    /// <summary>
    /// Puts someone on the field: in the slot they want if it's free, or the nearest free one to it in
    /// the row they'd rather, or failing that the other row. A big foe needs two free slots side by side.
    /// </summary>
    /// <remarks>With no slot wanted, the nearest is counted from the lane nearest the middle of the room: a side's right-hand lane for the party, its left-hand one for the foes.</remarks>
    /// <param name="who">Who.</param>
    /// <param name="wanted">The slot they'd like, or null. One on the other side, or off the end of a row, counts as none.</param>
    /// <param name="preferred">The row to look in first when the wanted slot's taken or there isn't one.</param>
    /// <returns>Whether they're on it; false means there's no room on their side.</returns>
    internal bool Place(Combatant who, Slot? wanted, Row preferred)
    {
        Remove(who);
        int lanes = Slot.LanesOf(who.Side);
        int width = who.Big ? 2 : 1;
        Slot? asked = wanted is Slot slot && slot.Side == who.Side && slot.Lane >= 0 && slot.Lane + width <= lanes ? slot : null;

        Row first = asked?.Row ?? preferred;
        int from = asked?.Lane ?? (who.Side == Side.Party ? lanes - width : 0);
        foreach (Row row in new[] { first, Other(first) })
        {
            for (int step = 0; step < lanes; step++)
            {
                foreach (int lane in new[] { from - step, from + step })
                {
                    if (lane < 0 || lane + width > lanes || step == 0 && lane != from) continue;
                    if (!Fits(who.Side, row, lane, width)) continue;
                    for (int i = 0; i < width; i++) _at[new Slot(who.Side, row, lane + i)] = who;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>Takes someone off the field: dead, yielded, beaten, or a guest fallen back.</summary>
    /// <param name="who">Who.</param>
    internal void Remove(Combatant who)
    {
        foreach (Slot slot in SlotsOf(who)) _at.Remove(slot);
    }

    /// <summary>Moves someone to a slot on their side, swapping with whoever's in it, who goes to the slot they left.</summary>
    /// <remarks>Throws for a big one, or a swap with a big one: neither can be moved.</remarks>
    /// <param name="who">Who.</param>
    /// <param name="to">Where to.</param>
    internal void MoveTo(Combatant who, Slot to)
    {
        IReadOnlyList<Slot> slots = SlotsOf(who);
        if (slots.Count != 1 || to.Side != who.Side) throw new InvalidOperationException($"{who.Name} can't be moved to {to}.");
        Slot from = slots[0];
        if (from == to) return;

        Combatant there = At(to);
        if (there is not null && there.Big) throw new InvalidOperationException($"{who.Name} can't swap with {there.Name}, who's too big to move.");

        _at.Remove(from);
        if (there is not null) _at[from] = there;
        _at[to] = who;
    }

    /// <summary>Moves someone the way MoveTo would, for a look: EnemyMind trying a Shift on before it picks.</summary>
    /// <remarks>
    /// Disposing it puts everyone back. It checks first that nothing else moved while the look was
    /// open, and after that everyone is where they were, and throws if either isn't so, so a look can't
    /// leak into the fight. Looks can nest, as long as the inner one is disposed first.
    /// </remarks>
    /// <param name="who">Who.</param>
    /// <param name="to">Where to.</param>
    /// <returns>What to dispose to put it back.</returns>
    internal IDisposable Try(Combatant who, Slot to)
    {
        var before = new Dictionary<Slot, Combatant>(_at);
        MoveTo(who, to);
        _looks++;
        return new Look(this, before, new Dictionary<Slot, Combatant>(_at), _looks);
    }

    /// <summary>Whether a big enough run of slots in a row is free.</summary>
    private bool Fits(Side side, Row row, int lane, int width)
    {
        for (int i = 0; i < width; i++)
            if (At(new Slot(side, row, lane + i)) is not null) return false;
        return true;
    }

    /// <summary>Every slot on a side, the front row first, left to right.</summary>
    internal static IEnumerable<Slot> AllSlots(Side side)
    {
        foreach (Row row in new[] { Row.Front, Row.Back })
            for (int lane = 0; lane < Slot.LanesOf(side); lane++)
                yield return new Slot(side, row, lane);
    }

    /// <summary>Adds someone to a list unless they're nobody, already in it, or the one being asked about.</summary>
    private static void AddOnce(List<Combatant> list, Combatant who, Combatant except)
    {
        if (who is not null && who != except && !list.Contains(who)) list.Add(who);
    }

    /// <summary>Whether two fields have everyone in the same slots.</summary>
    private static bool Same(Dictionary<Slot, Combatant> a, Dictionary<Slot, Combatant> b)
    {
        if (a.Count != b.Count) return false;
        foreach ((Slot slot, Combatant who) in a)
            if (!b.TryGetValue(slot, out Combatant other) || other != who) return false;
        return true;
    }

    /// <summary>An open look, which puts the field back when it's disposed.</summary>
    private sealed class Look : IDisposable
    {
        private readonly Formation _formation;
        private readonly Dictionary<Slot, Combatant> _before;
        private readonly Dictionary<Slot, Combatant> _during;
        private readonly int _depth;
        private bool _done;

        /// <summary>Opens a look.</summary>
        /// <param name="formation">The field.</param>
        /// <param name="before">Who was where before it.</param>
        /// <param name="during">Who's where with it on.</param>
        /// <param name="depth">How many looks are open, this one included.</param>
        public Look(Formation formation, Dictionary<Slot, Combatant> before, Dictionary<Slot, Combatant> during, int depth)
        {
            _formation = formation;
            _before = before;
            _during = during;
            _depth = depth;
        }

        /// <summary>Puts everyone back, and checks the look didn't leak.</summary>
        public void Dispose()
        {
            if (_done) throw new InvalidOperationException("Formation.Try: a look was put back twice.");
            _done = true;
            if (_formation._looks != _depth) throw new InvalidOperationException("Formation.Try: looks were put back out of order.");
            if (!Same(_formation._at, _during)) throw new InvalidOperationException("Formation.Try: someone moved while a look was open.");

            _formation._at.Clear();
            foreach ((Slot slot, Combatant who) in _before) _formation._at[slot] = who;
            _formation._looks--;
            if (!Same(_formation._at, _before)) throw new InvalidOperationException("Formation.Try: the field didn't go back the way it was.");
        }
    }
}
