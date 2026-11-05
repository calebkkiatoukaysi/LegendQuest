namespace LegendQuest.Combat;

/// <summary>
/// One foe in an encounter: what it fights as, the map npc who turns on you if there's one, and the
/// slot it stands in.
/// </summary>
/// <remarks>
/// A foe that's an npc walks from where the room has it standing to its slot; one that isn't turns
/// up in its slot. A big foe (EnemyDefinition.Big) stands in its slot and the one to the right of it,
/// so its slot is its left-hand one. In a wave, a slot that's taken when the foe arrives sends it to
/// the nearest free slot in that row, then the other row, and failing both it waits for one.
/// </remarks>
/// <param name="Enemy">Its stat block, by EnemyCatalog id.</param>
/// <param name="Npc">The id of the npc on the map who becomes it, or null if it isn't standing there already.</param>
/// <param name="Slot">Where it stands: always on the foes' side. 11-combat.md §1's numbers are Slot.Of's.</param>
public sealed record EncounterFoe(string Enemy, string Npc, Slot Slot);
