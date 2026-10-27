using System.Collections.Generic;

namespace LegendQuest.World;

/// <summary>
/// A min-heap: things go in with a priority, and the lowest comes out first. A*'s open list.
/// </summary>
/// <remarks>
/// A binary heap in a list, the textbook way: each entry's children are at 2i + 1 and 2i + 2, and an
/// entry is never bigger than its children. Pushing and popping both take one walk up or down the
/// tree. Not from a course tutorial.
/// </remarks>
/// <typeparam name="T">What's being kept.</typeparam>
public class BinaryHeap<T>
{
    private readonly List<(T Item, float Priority)> _entries = new();

    /// <summary>How many things are in it.</summary>
    public int Count => _entries.Count;

    /// <summary>Puts something in.</summary>
    /// <param name="item">The thing.</param>
    /// <param name="priority">Its priority. Lower comes out first.</param>
    public void Push(T item, float priority)
    {
        _entries.Add((item, priority));

        // Up the tree until its parent is no bigger than it.
        int i = _entries.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_entries[parent].Priority <= _entries[i].Priority) break;
            (_entries[parent], _entries[i]) = (_entries[i], _entries[parent]);
            i = parent;
        }
    }

    /// <summary>Takes out the thing with the lowest priority.</summary>
    /// <returns>The thing. Check Count first.</returns>
    public T Pop()
    {
        T top = _entries[0].Item;

        int last = _entries.Count - 1;
        _entries[0] = _entries[last];
        _entries.RemoveAt(last);

        // Down the tree, swapping with the smaller child, until neither child is smaller.
        int i = 0;
        while (true)
        {
            int left = 2 * i + 1, right = left + 1, smallest = i;
            if (left < _entries.Count && _entries[left].Priority < _entries[smallest].Priority) smallest = left;
            if (right < _entries.Count && _entries[right].Priority < _entries[smallest].Priority) smallest = right;
            if (smallest == i) break;
            (_entries[smallest], _entries[i]) = (_entries[i], _entries[smallest]);
            i = smallest;
        }

        return top;
    }
}
