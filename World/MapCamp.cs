using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// One place at a map's camp, where someone waiting for a place in the party stands: a seat at a
/// table in the Gilded Flagon, say.
/// </summary>
/// <remarks>Whoever's first in SaveData.Camp stands at the map's first, the next at its second, and so on.</remarks>
/// <param name="Id">What the map file calls it.</param>
/// <param name="Feet">Where whoever's waiting there stands, in world pixels.</param>
/// <param name="Facing">Which way they face when nobody's talking to them.</param>
public sealed record MapCamp(string Id, Vector2 Feet, Direction Facing);
