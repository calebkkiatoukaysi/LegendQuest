using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// A pool of light on a dark map with nothing drawn for it, like daylight coming down a stair.
/// </summary>
/// <remarks>A torch is a prop with a light on it instead. This is for light that comes from somewhere you can't see.</remarks>
/// <param name="Pool">The middle of the pool, in world pixels.</param>
/// <param name="Reach">How far it gets before it's mostly gone, in world pixels.</param>
/// <param name="Strength">How bright it is in the middle, 0 to 1.</param>
public sealed record MapLight(Vector2 Pool, float Reach, float Strength);
