namespace LegendQuest.World;

/// <summary>
/// A conversation that plays by itself on a map, the first time it can once you're there.
/// </summary>
/// <remarks>The deck at night plays the Prologue's this way as soon as the map's up, and a scene with when= plays the moment its flag goes up.</remarks>
/// <param name="Id">What the map file calls it. It's saved as played by this.</param>
/// <param name="Talk">The conversation, by its id in DialogueCatalog.</param>
/// <param name="Presence">When it can play.</param>
public sealed record MapScene(string Id, string Talk, MapPresence Presence);
