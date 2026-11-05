using System;
using LegendQuest.Characters;

namespace LegendQuest.Combat;

/// <summary>
/// Who starts a fight, where, and on which side: what the explore screen (or the simulator) hands
/// the engine.
/// </summary>
/// <param name="Id">Who they are: a party member's origin id, or the foe's map npc id or a spawned id.</param>
/// <param name="Side">Which side they're on.</param>
/// <param name="Member">The party member, or null for someone fighting from a stat block.</param>
/// <param name="Enemy">The stat block they fight from, or null for a party member.</param>
/// <param name="Guest">Whether they fight on the party's side without being in it, played by the AI.</param>
public sealed record CombatantSetup(string Id, Side Side, PartyMember Member, EnemyDefinition Enemy, bool Guest)
{
    /// <summary>The slot they start in, or null for the engine to find them one.</summary>
    public Slot? Slot { get; init; }

    /// <summary>The row they'd rather stand in when they've no slot or it's taken, or null to go by their kit: a melee auto in front, the rest at the back.</summary>
    public Row? Row { get; init; }

    /// <summary>The round they join at the start of: 1 to be there from the beginning, later for a guest who turns up.</summary>
    public int FromRound { get; init; } = 1;

    /// <summary>A party member, on the party's side.</summary>
    /// <param name="member">Who.</param>
    /// <param name="slot">The slot they start in, or null for the engine to place them by their kit.</param>
    public static CombatantSetup ForMember(PartyMember member, Slot? slot = null) =>
        new(member.Id, Side.Party, member, null, false) { Slot = slot };

    /// <summary>A foe, by the encounter's line for it, in the encounter's slot.</summary>
    /// <param name="foe">The encounter's line.</param>
    /// <param name="id">Its id: the map npc's, or one made up for it.</param>
    public static CombatantSetup ForFoe(EncounterFoe foe, string id) =>
        new(id, Side.Foes, null, EnemyCatalog.Get(foe.Enemy), false) { Slot = foe.Slot };

    /// <summary>A guest who fights alongside the party: a companion before they've joined, from their origin, or anyone else from a stat block; with the row they'd rather and the round they join.</summary>
    /// <remarks>A companion guest is a fresh copy of them at full health, since whatever happens to them before they join shouldn't follow them in.</remarks>
    /// <param name="guest">The encounter's line for them.</param>
    public static CombatantSetup ForGuest(EncounterGuest guest)
    {
        CombatantSetup setup = Origins.Exists(guest.Id) ? new(guest.Id, Side.Party, Origins.Create(guest.Id), null, true)
            : EnemyCatalog.Exists(guest.Id) ? new(guest.Id, Side.Party, null, EnemyCatalog.Get(guest.Id), true)
            : throw new ArgumentException($"Guest {guest.Id} is neither an origin nor an enemy.", nameof(guest));
        return setup with { Row = guest.Row, FromRound = guest.FromRound };
    }
}
