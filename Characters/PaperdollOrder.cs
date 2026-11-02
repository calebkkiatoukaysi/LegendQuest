using System;
using System.Collections.Generic;

namespace LegendQuest.Characters;

/// <summary>
/// The order the paperdoll's layers go down in, bottom first, for each way someone can face.
/// </summary>
/// <remarks>
/// <para>
/// The cloak is behind someone facing you and over them walking away, and so are the tails. Each
/// comes as two sheets so that one place in the stack works for every direction: the back half
/// is what hangs behind (all of a cape facing you, none of it facing away), and the front half
/// is what shows over them (the drape on the shoulders and the clasp facing you, all of it facing
/// away). The back halves go under the body and the front halves over it.
/// </para>
/// <para>
/// The weapon's in the right hand from the front and from behind. From the side it's in the hand
/// nearer you, the one that swings in front of the body, since the two side rows are each other
/// in a mirror the way the youth was drawn. So the weapon goes on top of everything every way they
/// face. The shield is on the other arm: facing you it's on their left arm, out in front of them,
/// and goes over everything but the weapon; from the side it's on the far arm and from behind it's
/// held out in front of them, so both times it goes under everything, and only its rim shows past
/// them.
/// </para>
/// <para>
/// Boots go on under a skirt, armour over the skirt (a robe or a hauberk covers it), gloves over
/// the armour's sleeves, and a beard over the armour's chest. The hair goes over the cloak, so long
/// hair falls down the back of it, and the hat over the hair. A helm takes the hat's place.
/// </para>
/// </remarks>
public static class PaperdollOrder
{
    private static readonly Dictionary<Direction, PaperdollSlot[]> Orders = new()
    {
        [Direction.Down] = new[]
        {
            PaperdollSlot.CapeBack, PaperdollSlot.TailBack, PaperdollSlot.Body, PaperdollSlot.Boots, PaperdollSlot.Legs,
            PaperdollSlot.Armour, PaperdollSlot.Hands, PaperdollSlot.Face, PaperdollSlot.Beard, PaperdollSlot.CapeFront,
            PaperdollSlot.Hair, PaperdollSlot.Hat, PaperdollSlot.Ears, PaperdollSlot.Horns, PaperdollSlot.TailFront,
            PaperdollSlot.Shield, PaperdollSlot.Weapon,
        },
        [Direction.Left] = new[]
        {
            PaperdollSlot.Shield, PaperdollSlot.CapeBack, PaperdollSlot.TailBack, PaperdollSlot.Body, PaperdollSlot.Boots,
            PaperdollSlot.Legs, PaperdollSlot.Armour, PaperdollSlot.Hands, PaperdollSlot.Face, PaperdollSlot.Beard,
            PaperdollSlot.CapeFront, PaperdollSlot.Hair, PaperdollSlot.Hat, PaperdollSlot.Ears, PaperdollSlot.Horns,
            PaperdollSlot.TailFront, PaperdollSlot.Weapon,
        },
        [Direction.Right] = new[]
        {
            PaperdollSlot.Shield, PaperdollSlot.CapeBack, PaperdollSlot.TailBack, PaperdollSlot.Body, PaperdollSlot.Boots,
            PaperdollSlot.Legs, PaperdollSlot.Armour, PaperdollSlot.Hands, PaperdollSlot.Face, PaperdollSlot.Beard,
            PaperdollSlot.CapeFront, PaperdollSlot.Hair, PaperdollSlot.Hat, PaperdollSlot.Ears, PaperdollSlot.Horns,
            PaperdollSlot.TailFront, PaperdollSlot.Weapon,
        },
        [Direction.Up] = new[]
        {
            PaperdollSlot.Shield, PaperdollSlot.CapeBack, PaperdollSlot.TailBack, PaperdollSlot.Body, PaperdollSlot.Boots,
            PaperdollSlot.Legs, PaperdollSlot.Armour, PaperdollSlot.Hands, PaperdollSlot.Face, PaperdollSlot.Beard,
            PaperdollSlot.CapeFront, PaperdollSlot.Hair, PaperdollSlot.Hat, PaperdollSlot.Ears, PaperdollSlot.Horns,
            PaperdollSlot.TailFront, PaperdollSlot.Weapon,
        },
    };

    /// <summary>Checks the table has an order for every direction, each with every slot in it once.</summary>
    /// <remarks>Throws on purpose, so a slot left out (which would never be drawn) shows up when the game starts.</remarks>
    static PaperdollOrder()
    {
        foreach (Direction facing in Enum.GetValues<Direction>())
        {
            if (!Orders.TryGetValue(facing, out PaperdollSlot[] order))
                throw new InvalidOperationException($"PaperdollOrder has no order for facing {facing}.");

            var seen = new HashSet<PaperdollSlot>();
            foreach (PaperdollSlot slot in order)
            {
                if (!seen.Add(slot))
                    throw new InvalidOperationException($"PaperdollOrder has {slot} twice facing {facing}.");
            }
            foreach (PaperdollSlot slot in Enum.GetValues<PaperdollSlot>())
            {
                if (!seen.Contains(slot))
                    throw new InvalidOperationException($"PaperdollOrder leaves {slot} out facing {facing}.");
            }
        }
    }

    /// <summary>The order the layers go down in for one way of facing, bottom first.</summary>
    /// <param name="facing">Which way: which row of the sheet it's for.</param>
    public static IReadOnlyList<PaperdollSlot> Of(Direction facing) => Orders[facing];
}
