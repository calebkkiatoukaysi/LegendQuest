using System;
using System.Collections.Generic;

namespace LegendQuest.Items;

/// <summary>
/// The party's bag: adding things to it, taking them out, and counting them.
/// </summary>
/// <remarks>
/// A wrapper round the save's list (SaveData.Inventory), not a copy, so whatever it does is in the
/// save already. Things that stack go on one ItemStack; anything else gets a place each, in the
/// order it came in. The key items the story hands over aren't in here: they stay in SaveData.Items.
/// </remarks>
public sealed class Inventory
{
    private readonly List<ItemStack> _stacks;

    /// <summary>Wraps a list of stacks.</summary>
    /// <param name="stacks">The list, usually SaveData.Inventory. It's changed in place.</param>
    public Inventory(List<ItemStack> stacks)
    {
        _stacks = stacks ?? throw new ArgumentNullException(nameof(stacks));
    }

    /// <summary>Every place in the bag, in order.</summary>
    public IReadOnlyList<ItemStack> Stacks => _stacks;

    /// <summary>Puts something in the bag.</summary>
    /// <param name="id">The item. It has to exist.</param>
    /// <param name="count">How many, from 1 up.</param>
    public void Add(string id, int count = 1)
    {
        if (!ItemCatalog.Exists(id)) throw new ArgumentException($"There's no item called {id}.", nameof(id));
        if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), count, "Add at least one.");

        if (!ItemCatalog.Get(id).Stacks)
        {
            for (int i = 0; i < count; i++) _stacks.Add(new ItemStack { Id = id, Count = 1 });
            return;
        }

        ItemStack stack = _stacks.Find(each => each.Id == id);
        if (stack is null) _stacks.Add(new ItemStack { Id = id, Count = count });
        else stack.Count += count;
    }

    /// <summary>Takes something out of the bag, if there's enough of it, the most recently added first.</summary>
    /// <param name="id">The item.</param>
    /// <param name="count">How many, from 1 up.</param>
    /// <returns>True if they were taken; false, and nothing taken, if there weren't that many.</returns>
    public bool Remove(string id, int count = 1)
    {
        if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), count, "Remove at least one.");
        if (Count(id) < count) return false;

        for (int i = _stacks.Count - 1; i >= 0 && count > 0; i--)
        {
            ItemStack stack = _stacks[i];
            if (stack.Id != id) continue;

            int taken = Math.Min(stack.Count, count);
            stack.Count -= taken;
            count -= taken;
            if (stack.Count == 0) _stacks.RemoveAt(i);
        }

        return true;
    }

    /// <summary>How many of something are in the bag.</summary>
    /// <param name="id">The item.</param>
    public int Count(string id)
    {
        int total = 0;
        foreach (ItemStack stack in _stacks)
            if (stack.Id == id) total += stack.Count;
        return total;
    }
}
