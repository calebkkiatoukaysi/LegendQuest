namespace LegendQuest;

/// <summary>
/// Which of the other side's rows an attack can get at, and from which of your own.
/// </summary>
/// <remarks>
/// "The opposing front" is always the other side's effective front row: their back row, when nobody
/// is standing in front of it. New ones go on the end.
/// </remarks>
public enum Reach
{
    /// <summary>Whatever the weapon in hand has: Reach for a reach weapon, Ranged for a ranged one, Melee for the rest.</summary>
    Weapon,

    /// <summary>Only from your side's effective front row, and only at the opposing front.</summary>
    Melee,

    /// <summary>A spear's or a whip's: from your front row at either of their rows, from your back row at the opposing front only.</summary>
    Reach,

    /// <summary>A bow's or a spell's: from anywhere, at anyone.</summary>
    Ranged,

    /// <summary>A skill that says so (Backstab): from anywhere, at anyone.</summary>
    AnyRow,
}
