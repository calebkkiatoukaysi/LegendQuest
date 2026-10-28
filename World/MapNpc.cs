using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// Someone standing on a map who isn't in the party: who they are, where, how they look, and what
/// they say if you talk to them.
/// </summary>
/// <param name="Id">What the map file calls them.</param>
/// <param name="Name">What they're called on screen.</param>
/// <param name="Feet">Where their feet are, in world pixels.</param>
/// <param name="Facing">Which way they face when nobody's talking to them.</param>
/// <param name="Sheet">Which of the townsfolk's sheets they're drawn from.</param>
/// <param name="Say">The line they say when you talk to them, when there's no conversation for them in Conversations.</param>
/// <param name="Wander">How far they walk about from where they're put, in world pixels. Zero to stand still.</param>
public sealed record MapNpc(string Id, string Name, Vector2 Feet, Direction Facing, string Sheet, string Say, float Wander);
