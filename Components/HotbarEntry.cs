using Microsoft.Xna.Framework;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// One button on the fight's hotbar, as the explore screen hands it over: the ability (or none, for
/// a primary button whose place in the kit is empty), its icon, its key, whether it can be used now,
/// whether it's picked, and the numbers on it.
/// </summary>
/// <param name="Ability">The ability, or null for an empty primary button.</param>
/// <param name="Icon">Its icon on the ability sheet, or empty for none.</param>
/// <param name="Key">The key it goes by, as written in its corner: 1 to 6 for the primary buttons, 7, 8, 9 and 0 for the strip.</param>
/// <param name="Primary">Whether it's one of the six primary buttons (Auto, the three skills, the ultimate and the potion); the rest are the shared strip.</param>
/// <param name="Usable">Whether it can be used right now. One that can't is greyed, and its card says why.</param>
/// <param name="Selected">Whether it's the one picked, waiting for a target.</param>
/// <param name="Focus">The Focus it costs, or 0.</param>
/// <param name="Cooldown">How many more of your turns until it's ready again, or 0 if it is.</param>
/// <param name="Count">What's left of what it uses, for its corner ("3" potions in the bag), or null.</param>
/// <param name="Guided">Whether the tutorial's pointing at it.</param>
public sealed record HotbarEntry(AbilityDefinition Ability, Rectangle Icon, string Key, bool Primary, bool Usable, bool Selected,
    int Focus, int Cooldown, string Count, bool Guided);
