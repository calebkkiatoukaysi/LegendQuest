using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// A prop placed on a map: which one, and where its foot is.
/// </summary>
/// <param name="Prop">What it is.</param>
/// <param name="Foot">The middle of its bottom edge, in world pixels.</param>
/// <param name="Presence">When it's there.</param>
/// <param name="Id">What a zone file calls this one, for a cue to show or hide it, or null. A map's props are named only by what they are.</param>
public sealed record MapProp(PropDefinition Prop, Vector2 Foot, MapPresence Presence, string Id = null);
