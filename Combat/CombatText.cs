namespace LegendQuest.Combat;

/// <summary>
/// The bits of the combat log's lines that every roll shares: a modifier with its sign, and which
/// two d20s were rolled with advantage or disadvantage.
/// </summary>
/// <remarks>Plain ASCII on purpose: the fonts only have the printable ASCII characters, so it's " - " and not a dash.</remarks>
internal static class CombatText
{
    /// <summary>A modifier the way the log adds it on: "+ 5", "- 1".</summary>
    /// <param name="value">The modifier.</param>
    public static string Signed(int value) => value < 0 ? $"- {-value}" : $"+ {value}";

    /// <summary>What goes after the attacker when two d20s were rolled: " with advantage (14, 6)", or nothing for one die.</summary>
    /// <param name="advantage">How it was rolled.</param>
    /// <param name="first">The first d20.</param>
    /// <param name="second">The second.</param>
    public static string AdvantageNote(Advantage advantage, int first, int second) => advantage switch
    {
        Advantage.Advantage => $" with advantage ({first}, {second})",
        Advantage.Disadvantage => $" with disadvantage ({first}, {second})",
        _ => string.Empty,
    };
}
