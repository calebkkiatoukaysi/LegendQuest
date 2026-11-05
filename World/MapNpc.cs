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
/// <param name="Sheet">Which of the townsfolk's sheets they're drawn from, or null if they're one of the cast, a look or a creature.</param>
/// <param name="Cast">Which of the story's cast they are, by their id in Cast, or null for townsfolk.</param>
/// <param name="Look">The look the paperdoll bakes them from, picked from a seed so a crowd can be anyone, or null if they've a sheet or they're one of the cast.</param>
/// <param name="Creature">Which of the Creatures they are, by id, or null if they're a person.</param>
/// <param name="Say">The lines they say, in turn, when you talk to them and there's no conversation for them in DialogueCatalog, and now and then by themselves. Empty if they've nothing to say.</param>
/// <param name="Talk">The conversation talking to them starts, by its id in DialogueCatalog, while it can start, or null to look theirs up by their id.</param>
/// <param name="Wander">How far they walk about from where they're put, in world pixels. Zero to stand still.</param>
/// <param name="Presence">When they're there.</param>
/// <param name="Aboard">The prop they're standing on, by its id, if they ride on one that moves (the crew on the ship coming into the harbour), or null if they're on the ground.</param>
/// <param name="Patrol">The stretch of a room's floor they walk from end to end and back, resting at each, in world pixels along it, or null if they don't. A zone's npcs only: a map's never patrol.</param>
/// <param name="Badge">The badge beside their name (a shop's purse, the smith's anvil) for anyone you can buy from or train with, or null.</param>
public sealed record MapNpc(string Id, string Name, Vector2 Feet, Direction Facing, string Sheet, string Cast, CharacterLook Look,
    string Creature, IReadOnlyList<string> Say, string Talk, float Wander, MapPresence Presence, string Aboard = null,
    (float Left, float Right)? Patrol = null, BadgeKind? Badge = null);
