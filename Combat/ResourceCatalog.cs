using System;
using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Combat;

/// <summary>
/// Every resource an ability can spend, by id, and the bookkeeping of what's been spent since the
/// last rest.
/// </summary>
/// <remarks>
/// The ids are what PartyMember.Spent is keyed by, so they're names that go in a save: renaming one
/// gives everybody theirs back. The counts are the SRD 5.1's at level 1: two level-1 slots for the
/// full casters, one pact slot that comes back on a short rest, Second Wind once a short rest, five
/// hit points of Lay on Hands a level, and a Charisma modifier's worth of Bardic Inspiration.
/// </remarks>
public static class ResourceCatalog
{
    /// <summary>Level-1 spell slots: clerics, wizards, druids and bards.</summary>
    public const string SpellSlot = "spell-slot-1";

    /// <summary>The warlock's pact slot.</summary>
    public const string PactSlot = "pact-slot";

    /// <summary>The fighter's Second Wind.</summary>
    public const string SecondWind = "second-wind";

    /// <summary>The paladin's Lay on Hands pool, in hit points.</summary>
    public const string LayOnHands = "lay-on-hands";

    /// <summary>The bard's Bardic Inspiration dice.</summary>
    public const string BardicInspiration = "bardic-inspiration";

    /// <summary>The resources that are spell slots, in the order an elixir gives them back.</summary>
    private static readonly string[] Slots = { SpellSlot, PactSlot };

    private static readonly ResourceDefinition[] Table =
    {
        new()
        {
            Id = SpellSlot, Name = "Spell slots", Recharge = RestKind.Long, Count = 2,
            Classes = new[] { ClassId.Cleric, ClassId.Wizard, ClassId.Druid, ClassId.Bard },
        },
        new() { Id = PactSlot, Name = "Pact slot", Recharge = RestKind.Short, Count = 1, Classes = new[] { ClassId.Warlock } },
        new() { Id = SecondWind, Name = "Second Wind", Recharge = RestKind.Short, Count = 1, Classes = new[] { ClassId.Fighter } },
        new() { Id = LayOnHands, Name = "Lay on Hands", Recharge = RestKind.Long, Count = 5, PerLevel = true, Classes = new[] { ClassId.Paladin } },
        new()
        {
            Id = BardicInspiration, Name = "Bardic Inspiration", Recharge = RestKind.Long, CountFrom = Ability.Charisma,
            Classes = new[] { ClassId.Bard },
        },
    };

    private static readonly Dictionary<string, ResourceDefinition> ById = new();

    /// <summary>Indexes the table and checks every row.</summary>
    /// <remarks>Throws on purpose, so a bad row shows up when the game starts.</remarks>
    static ResourceCatalog()
    {
        foreach (ResourceDefinition row in Table)
        {
            if (string.IsNullOrWhiteSpace(row.Name)) throw new InvalidOperationException($"Resource {row.Id} has no name.");
            if (row.Recharge == RestKind.None) throw new InvalidOperationException($"Resource {row.Id} never comes back: give it a rest that recharges it.");
            if (row.Classes.Count == 0) throw new InvalidOperationException($"Resource {row.Id} belongs to no class.");
            if (row.CountFrom is null && row.Count < 1) throw new InvalidOperationException($"Resource {row.Id} has {row.Count} to spend.");
            if (!ById.TryAdd(row.Id, row)) throw new InvalidOperationException($"Two resources are called {row.Id}.");
        }
    }

    /// <summary>Whether there's a resource with this id.</summary>
    /// <param name="id">Which.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One resource.</summary>
    /// <param name="id">Which. It has to exist.</param>
    public static ResourceDefinition Get(string id) => ById[id];

    /// <summary>How much of a resource someone has left: their most, less what they've spent since it last came back.</summary>
    /// <param name="member">Who.</param>
    /// <param name="id">Which resource.</param>
    public static int Left(PartyMember member, string id) =>
        Math.Max(0, Get(id).Max(member) - member.Spent.GetValueOrDefault(id));

    /// <summary>Notes some of a resource spent.</summary>
    /// <param name="member">Who spent it.</param>
    /// <param name="id">Which resource.</param>
    /// <param name="amount">How much.</param>
    public static void Spend(PartyMember member, string id, int amount) =>
        member.Spent[id] = member.Spent.GetValueOrDefault(id) + amount;

    /// <summary>Gives back spent spell slots, the way an elixir of focus does: level-1 slots first, then a pact slot.</summary>
    /// <param name="member">Who drank it.</param>
    /// <param name="slots">How many to give back.</param>
    /// <returns>How many came back, which is fewer if fewer were spent.</returns>
    public static int Restore(PartyMember member, int slots)
    {
        int restored = 0;
        foreach (string id in Slots)
        {
            int back = Math.Min(slots - restored, member.Spent.GetValueOrDefault(id));
            if (back <= 0) continue;
            member.Spent[id] -= back;
            restored += back;
        }
        return restored;
    }

    /// <summary>How many spent spell slots there are that an elixir could give back.</summary>
    /// <param name="member">Who.</param>
    public static int SlotsSpent(PartyMember member) => member.Spent.GetValueOrDefault(SpellSlot) + member.Spent.GetValueOrDefault(PactSlot);

    /// <summary>Gives back everything a rest recharges: a short rest the short-rest resources, a long rest all of them.</summary>
    /// <remarks>An id the save has that isn't here any more is dropped on any rest, since nothing can spend it.</remarks>
    /// <param name="member">Who rested.</param>
    /// <param name="rest">Which rest.</param>
    public static void Recharge(PartyMember member, RestKind rest)
    {
        if (rest == RestKind.None) return;

        var back = new List<string>();
        foreach (string id in member.Spent.Keys)
            if (!Exists(id) || Get(id).Recharge <= rest) back.Add(id);
        foreach (string id in back) member.Spent.Remove(id);
    }
}
