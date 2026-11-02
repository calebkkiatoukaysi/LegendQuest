using Microsoft.Xna.Framework;

namespace LegendQuest.Combat;

/// <summary>
/// One foe in an encounter: what it fights as, and either the map npc who turns on you or the tile
/// it turns up on.
/// </summary>
/// <param name="Enemy">Its stat block, by EnemyCatalog id.</param>
/// <param name="Npc">The id of the npc on the map who becomes it, or null if it isn't standing there already.</param>
/// <param name="Cell">The tile it turns up on when there's no npc, or null when there is.</param>
public sealed record EncounterFoe(string Enemy, string Npc, Point? Cell);
