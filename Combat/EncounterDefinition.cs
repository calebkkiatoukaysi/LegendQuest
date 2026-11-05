using System;
using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// One fight from the story: who's in it and where they stand, who helps, what plays, how it ends,
/// and the flag it sets when it's won.
/// </summary>
/// <remarks>
/// The ids are the story's scene ids, so the conversation that starts a fight names it, and the fight
/// can slot in where its card was. The foes that are on the map already are named by their npc ids;
/// anyone else turns up in their slot. What a fight's worth is LevelTable.FightXp's, from its Kind and
/// the level the story has the party at for it.
/// </remarks>
public sealed class EncounterDefinition
{
    /// <summary>What the story calls it.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen (the simulator's table, the toast).</summary>
    public required string Name { get; init; }

    /// <summary>The foes there when it starts, each in its slot.</summary>
    public required IReadOnlyList<EncounterFoe> Foes { get; init; }

    /// <summary>What sort of fight it is: the win rate the simulator holds it to, and what it's worth.</summary>
    public EncounterKind Kind { get; init; } = EncounterKind.Story;

    /// <summary>The song that plays.</summary>
    public Track Music { get; init; } = Track.Battle;

    /// <summary>Whether it's a boss fight of any size: a mini-boss, a boss or the Sovereign. Its start card's in ember.</summary>
    public bool Boss => Kind is EncounterKind.MiniBoss or EncounterKind.Boss or EncounterKind.Sovereign;

    /// <summary>Whether it's the tutorial fight: the tips play, and it's tuned so nobody loses it.</summary>
    public bool Tutorial => Kind == EncounterKind.Tutorial;

    /// <summary>A loot table the fight's won with on top of what each foe drops (the vault's own treasure), by LootTableCatalog id, or null.</summary>
    public string LootTable { get; init; }

    /// <summary>The flag set when it's won, or null if the conversation after it does that.</summary>
    public string Victory { get; init; }

    /// <summary>The conversation played straight after it's won, by its id in DialogueCatalog, or null for none.</summary>
    public string Then { get; init; }

    /// <summary>For a fight the story lets you lose, the conversation played instead of game over, by its id in DialogueCatalog; null means losing it is game over.</summary>
    public string Lost { get; init; }

    /// <summary>The foes who arrive later, in round order.</summary>
    public IReadOnlyList<EncounterWave> Waves { get; init; } = Array.Empty<EncounterWave>();

    /// <summary>Who fights in a slot on the party's side without being in the party: a companion before they've joined, or someone from a stat block.</summary>
    public IReadOnlyList<EncounterGuest> Guests { get; init; } = Array.Empty<EncounterGuest>();

    /// <summary>Who helps from behind the rows, at the end of every round, and can't be hit.</summary>
    public IReadOnlyList<EncounterSupport> Supports { get; init; } = Array.Empty<EncounterSupport>();

    /// <summary>
    /// Party members who don't fight this one, by origin id: Sera, busy drawing the rot out of the
    /// crystal while the two of you hold the den. FightRoster leaves them out (they stay where they
    /// are on the map, out of it), and so does the simulator. Never you or the Soul-Bound.
    /// </summary>
    public IReadOnlyList<string> SitsOut { get; init; } = Array.Empty<string>();

    /// <summary>The share of its hit points a foe gives up at, ending its part in the fight, or 0 for a fight to the end.</summary>
    public float YieldsAt { get; init; }

    /// <summary>Whether nobody dies: anyone at 0 hit points is knocked out and stable instead, and foes leave the knocked-out alone.</summary>
    public bool Nonlethal { get; init; }

    /// <summary>For a fight you only have to last: getting to the end of this round with anyone standing wins it. 0 for a fight that's won by beating the foes.</summary>
    public int EndsAfterRounds { get; init; }

    /// <summary>The share of their hit points the fight's won at, the way a fight against something too big to kill ends: a fifth, for the Hungering. EndsAtFoe's, or with none, all the foes' together. 0 for none.</summary>
    public float EndsAtHealthShare { get; init; }

    /// <summary>The foe EndsAtHealthShare watches, by its npc id or, for one with no npc, its EnemyCatalog id; null for all of them together. Only with an EndsAtHealthShare.</summary>
    public string EndsAtFoe { get; init; }

    /// <summary>The level the party's tuned to be at for it, or 0 to take it from the level table by the id's act and chapter (LevelTable.Expected).</summary>
    public int ExpectedLevel { get; init; }

    /// <summary>Slots that time stops over, a round ahead. A stub until the fights that use it (M8): validation turns down an encounter with any.</summary>
    public IReadOnlyList<FreezeZone> FreezeZones { get; init; } = Array.Empty<FreezeZone>();

    /// <summary>For a fight across several rooms, the rooms it goes through in order, by id. A stub until the fights that use it (M8): validation turns down an encounter with any.</summary>
    public IReadOnlyList<string> Rooms { get; init; } = Array.Empty<string>();
}
