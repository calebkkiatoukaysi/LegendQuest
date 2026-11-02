using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Combat;

/// <summary>
/// What a click would do on your turn, worked out before you click: walk a path, use an ability on
/// someone or at a tile (walking into reach first if that's what it takes), or why it can't be done.
/// </summary>
/// <remarks>
/// The explore screen works one out for whatever the mouse is on, draws it (the path, the target's
/// ring, the card with the odds) and carries it out on the click, so what's shown is exactly what
/// happens.
/// </remarks>
/// <param name="Ability">What would be used, or null for a walk.</param>
/// <param name="Target">At whom, or null.</param>
/// <param name="Cell">The tile it's aimed at: the target's, or the ground's.</param>
/// <param name="Path">The tiles to walk first, not counting where they stand; empty for none.</param>
/// <param name="Reachable">How many of the path's tiles this turn's movement covers, ending somewhere free.</param>
/// <param name="Problem">Why it can't be done, in a few words, or null if it can.</param>
public sealed record TurnPlan(AbilityDefinition Ability, Combatant Target, Point Cell, IReadOnlyList<Point> Path, int Reachable, string Problem);
