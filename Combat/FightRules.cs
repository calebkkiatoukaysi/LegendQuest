using System;

namespace LegendQuest.Combat;

/// <summary>
/// What a fight is played by beyond the encounter itself: the difficulty, the story's flags, and
/// where the kits come from.
/// </summary>
/// <param name="Difficulty">How hard it is: the foes' hit points and rolls, and the Focus the party's side starts on (DifficultyRules).</param>
/// <param name="Flag">Whether a story flag is set, for the allies who only come when one is.</param>
/// <param name="Kits">Where everyone's kit comes from.</param>
public sealed record FightRules(Difficulty Difficulty, Func<string, bool> Flag, IKitSource Kits);
