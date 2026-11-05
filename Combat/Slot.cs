using System;

namespace LegendQuest.Combat;

/// <summary>
/// One place to stand in a fight: a side, a row, and a lane across it.
/// </summary>
/// <remarks>
/// Lanes count from 0 at the left. The numbers are 11-combat.md §1's: the party's front row is 1 to
/// 3 and its back row 4 to 6, the foes' front 1 to 4 and back 5 to 8, so a lane's back slot is a
/// row's width on from its front one (your 1 is in front of your 4, a foe's 1 in front of its 5).
/// </remarks>
/// <param name="Side">Whose side it's on.</param>
/// <param name="Row">Which row.</param>
/// <param name="Lane">Which lane, from 0 at the left.</param>
public readonly record struct Slot(Side Side, Row Row, int Lane)
{
    /// <summary>How many lanes the party's side has: three to a row, six slots, five of the party and a guest.</summary>
    public const int PartyLanes = 3;

    /// <summary>How many lanes the foes' side has: four to a row, eight slots.</summary>
    public const int FoeLanes = 4;

    /// <summary>Its number the way the story writes it, from 1: the front row left to right, then the back.</summary>
    public int Number => Lane + 1 + (Row == Row.Back ? LanesOf(Side) : 0);

    /// <summary>How many lanes a side has.</summary>
    /// <param name="side">Which side.</param>
    public static int LanesOf(Side side) => side == Side.Party ? PartyLanes : FoeLanes;

    /// <summary>The slot with a number on a side, the way the story writes it.</summary>
    /// <remarks>Throws if the side hasn't a slot with that number.</remarks>
    /// <param name="side">Which side.</param>
    /// <param name="number">From 1: 1 to 6 for the party, 1 to 8 for the foes.</param>
    public static Slot Of(Side side, int number)
    {
        int lanes = LanesOf(side);
        if (number < 1 || number > 2 * lanes)
            throw new ArgumentOutOfRangeException(nameof(number), number, $"The {side} side's slots are numbered 1 to {2 * lanes}.");
        return new Slot(side, number > lanes ? Row.Back : Row.Front, (number - 1) % lanes);
    }
}
