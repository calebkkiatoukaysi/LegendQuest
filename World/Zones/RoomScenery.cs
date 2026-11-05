namespace LegendQuest.World;

/// <summary>
/// A prop standing in one of a room's painted layers instead of on its floor: the buildings along
/// the back, the palace far off, a lamp post in front. Nothing walks into it.
/// </summary>
/// <remarks>It's what the placeholder painter builds a room's layers from, and what the room draws from the props' own pictures while a layer isn't painted yet.</remarks>
/// <param name="Layer">far, mid or fore.</param>
/// <param name="Prop">Which prop, by its id in PropCatalog.</param>
/// <param name="X">Where its foot is along the room, in world pixels.</param>
public sealed record RoomScenery(string Layer, string Prop, float X);
