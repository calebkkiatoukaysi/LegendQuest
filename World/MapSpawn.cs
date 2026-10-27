using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// Somewhere on a map the party can arrive: where the feet go, and which way to face.
/// </summary>
/// <param name="Feet">Where the feet go, in world pixels.</param>
/// <param name="Facing">Which way to face on arriving. Into the map, usually.</param>
public sealed record MapSpawn(Vector2 Feet, Direction Facing);
