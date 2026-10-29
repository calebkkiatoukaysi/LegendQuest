using System.Collections.Generic;
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
/// <param name="Sheet">Which of the townsfolk's sheets they're drawn from, or null if they're one of the cast or a look.</param>
/// <param name="Cast">Which of the story's cast they are, by their id in Cast, or null for townsfolk.</param>
/// <param name="Look">The look the paperdoll bakes them from, picked from a seed so a crowd can be anyone, or null if they've a sheet or they're one of the cast.</param>
/// <param name="Say">The lines they say, in turn, when you talk to them and there's no conversation for them in DialogueCatalog, and now and then by themselves. Empty if they've nothing to say.</param>
/// <param name="Wander">How far they walk about from where they're put, in world pixels. Zero to stand still.</param>
/// <param name="Presence">When they're there.</param>
public sealed record MapNpc(string Id, string Name, Vector2 Feet, Direction Facing, string Sheet, string Cast, CharacterLook Look,
    IReadOnlyList<string> Say, float Wander, MapPresence Presence);
