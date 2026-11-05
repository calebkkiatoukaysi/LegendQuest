namespace LegendQuest;

/// <summary>
/// Who an ability catches once it's pointed: one person, or a pattern of the slots round them.
/// CombatEngine.Caught works it out.
/// </summary>
/// <remarks>11-combat.md §2.3's shapes. "Next to" is Formation.NextTo: beside them in their row, or the same lane's other row. New ones go on the end.</remarks>
public enum TargetShape
{
    /// <summary>Whoever's using it.</summary>
    Self,

    /// <summary>One of the user's side, anywhere.</summary>
    Ally,

    /// <summary>One of the user's side next to them, or themselves: a potion handed over.</summary>
    AllyNextTo,

    /// <summary>Everyone standing on the user's side.</summary>
    AllAllies,

    /// <summary>Everyone on the user's side in the user's own row.</summary>
    YourRow,

    /// <summary>Everyone in the user's side's back row.</summary>
    YourBackRow,

    /// <summary>One foe, in a row the ability's reach can get at.</summary>
    Foe,

    /// <summary>One foe and whoever's beside it in its row. A big foe's neighbours are the lanes either side of both its slots.</summary>
    FoeAndNeighbours,

    /// <summary>Everyone in one of the other side's rows.</summary>
    FoeRow,

    /// <summary>The front and back slot of one of the other side's lanes.</summary>
    FoeColumn,

    /// <summary>Everyone standing on the other side.</summary>
    AllFoes,

    /// <summary>The ability's Count picks among the foes, at random; the same foe can be picked more than once.</summary>
    RandomFoes,
}
