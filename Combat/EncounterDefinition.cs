using System;
using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// One fight from the story: who's in it, what plays, what it's worth, and the flag it sets when
/// it's won.
/// </summary>
/// <remarks>
/// The ids are the story's scene ids, so the conversation that starts a fight names it, and the fight
/// can slot in where its card was. The foes that are on the map already are named by their npc ids;
/// anyone else turns up on a tile.
/// </remarks>
public sealed class EncounterDefinition
{
    /// <summary>What the story calls it.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen (the simulator's table, the toast).</summary>
    public required string Name { get; init; }

    /// <summary>The foes there when it starts.</summary>
    public required IReadOnlyList<EncounterFoe> Foes { get; init; }

    /// <summary>The song that plays.</summary>
    public Track Music { get; init; } = Track.Battle;

    /// <summary>Whether it's a boss fight: its own song, and the boss's tuning target.</summary>
    public bool Boss { get; init; }

    /// <summary>Whether it's the tutorial fight: the tips play, and it's tuned so nobody loses it.</summary>
    public bool Tutorial { get; init; }

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

    /// <summary>Who fights alongside the party without being in it: origin ids (a companion before they've joined) or EnemyCatalog ids.</summary>
    public IReadOnlyList<string> Guests { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Party members who don't fight this one, by origin id: Sera, busy drawing the rot out of the
    /// crystal while the two of you hold the den. Whoever builds the fight's CombatantSetups leaves
    /// them out (they stay where they are on the map, out of it), and the simulator leaves them out
    /// of its party.
    /// </summary>
    public IReadOnlyList<string> SitsOut { get; init; } = Array.Empty<string>();

    /// <summary>The share of its hit points a foe gives up at, ending its part in the fight, or 0 for a fight to the end.</summary>
    public float YieldsAt { get; init; }

    /// <summary>Whether nobody dies: anyone at 0 hit points is knocked out and stable instead, and foes leave the knocked-out alone.</summary>
    public bool Nonlethal { get; init; }

    /// <summary>The XP for the whole fight, shared between the party: every foe's, waves included, as in 5e.</summary>
    public int Xp
    {
        get
        {
            int xp = 0;
            foreach (EncounterFoe foe in Foes) xp += EnemyCatalog.Get(foe.Enemy).Xp;
            foreach (EncounterWave wave in Waves)
                foreach (EncounterFoe foe in wave.Foes) xp += EnemyCatalog.Get(foe.Enemy).Xp;
            return xp;
        }
    }
}
