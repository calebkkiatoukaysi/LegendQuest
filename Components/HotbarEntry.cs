using Microsoft.Xna.Framework;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// One slot on the fight's hotbar, as the explore screen hands it over: the ability, its icon, which
/// group it's in, whether it can be used now, whether it's picked, and the count in its corner.
/// </summary>
/// <param name="Ability">The ability.</param>
/// <param name="Icon">Its icon on the ability sheet.</param>
/// <param name="Group">Which group of slots it's in (fighting, the bond, moving, the bag): a gap goes between two groups.</param>
/// <param name="Usable">Whether it can be used right now. One that can't is greyed, and its card says why.</param>
/// <param name="Selected">Whether it's the one picked, waiting for a target.</param>
/// <param name="Count">What's left of what it spends, for its corner ("2" slots, "3" potions), or null.</param>
/// <param name="Guided">Whether the tutorial's pointing at it.</param>
public sealed record HotbarEntry(AbilityDefinition Ability, Rectangle Icon, int Group, bool Usable, bool Selected, string Count, bool Guided);
