using Microsoft.Xna.Framework;
using LegendQuest.Items;

namespace LegendQuest.Components;

/// <summary>
/// One place in an ItemGrid: the item, how many, and the line under it if the grid has room for one.
/// </summary>
/// <param name="Item">What's there.</param>
/// <param name="Count">How many. Over one, it's written in the corner.</param>
/// <param name="Label">The line under it, like a price, or null.</param>
/// <param name="LabelColour">The line's colour.</param>
/// <param name="Dim">Whether it's greyed out: something the party can't have, like what it can't afford.</param>
public readonly record struct GridEntry(ItemDefinition Item, int Count, string Label, Color LabelColour, bool Dim);
