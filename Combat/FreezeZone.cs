namespace LegendQuest.Combat;

/// <summary>
/// Stopped time over the field: each round one slot on a side is marked a round ahead, and whoever
/// stands there when it fires is Frozen.
/// </summary>
/// <remarks>A stub until the fights that use it, in M7 and M8. Validation turns down an encounter that has one.</remarks>
/// <param name="Side">Whose slots it falls on.</param>
/// <param name="Row">The row it keeps to, or null for either.</param>
/// <param name="Turns">How many of their turns whoever it catches is Frozen for.</param>
public sealed record FreezeZone(Side Side, Row? Row = null, int Turns = 2);
