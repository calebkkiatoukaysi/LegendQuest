using Microsoft.Xna.Framework;

namespace LegendQuest.Components;

/// <summary>
/// A support in a fight the way the screen's showing it, for its banner over its figure and its entry
/// at the end of the turn order.
/// </summary>
/// <param name="Id">Which support, by its id in the encounter.</param>
/// <param name="Name">What the turn order calls it, in capitals.</param>
/// <param name="Line">What its banner over its figure says it does, in capitals.</param>
/// <param name="Icon">Its icon on the ability sheet, or null for none.</param>
/// <param name="Lit">Whether it acts at the end of this round.</param>
/// <param name="Progress">How far it is through what it's holding out for, 0 to 1, or null if it isn't.</param>
public sealed record SupportView(string Id, string Name, string Line, Rectangle? Icon, bool Lit, float? Progress);
