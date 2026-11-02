namespace LegendQuest.Combat;

/// <summary>
/// What the tooltip shows when an ability's pointed at someone: the chance it lands, what it does,
/// and the save if there is one.
/// </summary>
/// <param name="Chance">From 0 to 1: the chance an attack hits, or that the target fails its save. 1 for anything that can't miss.</param>
/// <param name="Min">The least it does: damage, or healing when Heals.</param>
/// <param name="Max">The most it does, a critical aside.</param>
/// <param name="Save">The save, as the tooltip writes it ("DC 13 DEX SAVE"), or null.</param>
/// <param name="Advantage">Whether the roll has advantage or disadvantage from where everyone's standing.</param>
/// <param name="Heals">Whether Min and Max are healing rather than damage.</param>
public sealed record AttackPreview(double Chance, int Min, int Max, string Save, Advantage Advantage, bool Heals);
