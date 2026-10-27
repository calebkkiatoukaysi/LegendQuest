using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// The light a prop gives off, like a torch in its sconce: a flame, and the pool of light it throws.
/// </summary>
/// <param name="Flame">Where the bottom of the flame is, from the prop's foot, in art pixels.</param>
/// <param name="Pool">Where the middle of the pool of light is, from the prop's foot. For a torch on a wall that's out on the floor in front of the wall, not on the flame.</param>
/// <param name="Reach">How far the light gets before it's mostly gone, in art pixels.</param>
/// <param name="Strength">How bright it is in the middle of the pool, 0 to 1. Two lights next to each other add up.</param>
public sealed record PropLight(Vector2 Flame, Vector2 Pool, float Reach, float Strength);
