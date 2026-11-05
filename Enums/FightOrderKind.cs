namespace LegendQuest;

/// <summary>
/// What a script can change about a fight that's already going, from a conversation played in
/// the middle of it.
/// </summary>
/// <remarks>New ones go on the end.</remarks>
public enum FightOrderKind
{
    /// <summary>A support stops acting (support-off): the wall falls, the battlemages are spent.</summary>
    SupportOff,

    /// <summary>More foes join (fight-add).</summary>
    AddFoes,

    /// <summary>A guest joins the party's side (fight-guest).</summary>
    AddGuest,
}
