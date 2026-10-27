using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// A prop placed on a map: which one, and where its foot is.
/// </summary>
/// <param name="Prop">What it is.</param>
/// <param name="Foot">The middle of its bottom edge, in world pixels.</param>
public sealed record MapProp(PropDefinition Prop, Vector2 Foot);
