using System;
using Microsoft.Xna.Framework;
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
/// <param name="Cell">The tile they start on. If it's blocked or taken, the engine puts them on the nearest free one.</param>
/// <param name="Facing">Which way they face.</param>
/// <param name="Guest">Whether they fight on the party's side without being in it, played by the AI.</param>
public sealed record CombatantSetup(string Id, Side Side, PartyMember Member, EnemyDefinition Enemy, Point Cell, Direction Facing, bool Guest)
{
    /// <summary>A party member, on the party's side.</summary>
    /// <param name="member">Who.</param>
    /// <param name="cell">Where they start.</param>
    /// <param name="facing">Which way they face.</param>
    public static CombatantSetup ForMember(PartyMember member, Point cell, Direction facing) =>
        new(member.Id, Side.Party, member, null, cell, facing, false);

    /// <summary>A foe, by the encounter's line for it.</summary>
    /// <param name="foe">The encounter's line.</param>
    /// <param name="id">Its id: the map npc's, or one made up for it.</param>
    /// <param name="cell">Where it starts.</param>
    /// <param name="facing">Which way it faces.</param>
    public static CombatantSetup ForFoe(EncounterFoe foe, string id, Point cell, Direction facing) =>
        new(id, Side.Foes, null, EnemyCatalog.Get(foe.Enemy), cell, facing, false);

    /// <summary>A guest who fights alongside the party: a companion before they've joined, from their origin, or anyone else from a stat block.</summary>
    /// <remarks>A companion guest is a fresh copy of them at full health, since whatever happens to them before they join shouldn't follow them in.</remarks>
    /// <param name="id">An origin id, or an EnemyCatalog id.</param>
    /// <param name="cell">Where they start.</param>
    /// <param name="facing">Which way they face.</param>
    public static CombatantSetup ForGuest(string id, Point cell, Direction facing)
    {
        if (Origins.Exists(id)) return new(id, Side.Party, Origins.Create(id), null, cell, facing, true);
        if (EnemyCatalog.Exists(id)) return new(id, Side.Party, null, EnemyCatalog.Get(id), cell, facing, true);
        throw new ArgumentException($"Guest {id} is neither an origin nor an enemy.", nameof(id));
    }
}
