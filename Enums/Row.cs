namespace LegendQuest;

/// <summary>
/// The two rows each side of a fight stands in: the front, facing the other side across the
/// middle of the room, and the back, a step behind it.
/// </summary>
/// <remarks>A side with nobody standing in its front row fights with its back row as its front (Formation.FrontOf).</remarks>
public enum Row
{
    /// <summary>The row facing the other side, which melee reaches.</summary>
    Front,

    /// <summary>A step behind the front row and deeper in the room, out of melee's reach while the front row holds.</summary>
    Back,
}
