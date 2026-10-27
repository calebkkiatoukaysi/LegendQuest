namespace LegendQuest.World;

/// <summary>
/// Something that happened on a map, as WorldState hands it back: what kind, and the line to show for it.
/// </summary>
/// <param name="Kind">What happened. The screen picks the sound from this.</param>
/// <param name="Text">The line to show, or null if the sound says enough.</param>
public sealed record WorldEvent(WorldEventKind Kind, string Text);
