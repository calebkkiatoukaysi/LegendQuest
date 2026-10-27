using Microsoft.Xna.Framework;
using LegendQuest.Characters;

namespace LegendQuest.World;

/// <summary>
/// Someone standing on a map who isn't in the party: who they are, where, how they look, and what
/// they say if you talk to them.
/// </summary>
/// <param name="Id">What the map file calls them.</param>
/// <param name="Name">What they're called on screen.</param>
/// <param name="Feet">Where their feet are, in world pixels.</param>
/// <param name="Facing">Which way they face when nobody's talking to them.</param>
/// <param name="Look">How they look.</param>
/// <param name="Say">The line they say when you talk to them. Conversations replace this when there are conversations.</param>
public sealed record MapNpc(string Id, string Name, Vector2 Feet, Direction Facing, CharacterLook Look, string Say);
