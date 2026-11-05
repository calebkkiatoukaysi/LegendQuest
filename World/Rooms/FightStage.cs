using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;
using LegendQuest.Combat;

namespace LegendQuest.World;

/// <summary>
/// Where a fight's slots stand on a room's floor: your side's six on the left, the foes' eight on
/// the right, the front rows facing each other across the middle (Story/11-combat.md §1, 12-rooms §5).
/// </summary>
/// <remarks>
/// <para>
/// Pure arithmetic on the room's floor band, so it can be checked without a window. The lanes are
/// LaneGap apart and the two front rows FrontGap apart across the centre; a back row sits half a lane
/// further out than its front row, so the faces behind show between the ones in front. Feet stand
/// FrontDepth of the way down the band (front) or BackDepth (back), the band measured where they
/// stand, so a sloping floor slopes the rows with it. Lane 0 is the leftmost on both sides, which puts
/// your lane 2 and the foes' lane 0 nearest the middle.
/// </para>
/// <para>
/// The centre is where the screen asks for (midway between the leader and the foes standing in the
/// room), clamped so the whole stage is inside the room's fight range if it fits there, and moved
/// along to the nearest place where every slot is on the floor if a fountain or a block is in the way.
/// Supports stand on the band's back edge behind your side, between the back row's lanes so nobody
/// stands in front of them. Check runs the same sums for every centre a fight could ask for.
/// </para>
/// </remarks>
public sealed class FightStage
{
    /// <summary>How far apart two lanes are, in world pixels.</summary>
    public const float LaneGap = 34f;

    /// <summary>How far apart the two front rows are across the middle, in world pixels.</summary>
    public const float FrontGap = 56f;

    /// <summary>How much further out than its front row a back row stands: half a lane.</summary>
    public const float BackStagger = LaneGap / 2f;

    /// <summary>How far down the floor band feet stand, from its back edge (0) to its front edge (1): the front row, then the back.</summary>
    public const float FrontDepth = 0.75f;
    public const float BackDepth = 0.30f;

    /// <summary>A slot plate's size on the floor, in world pixels: the hud's slot-plate piece at one art pixel to the world pixel.</summary>
    public const float PlateWidth = 28f;
    public const float PlateHeight = 10f;

    /// <summary>How far onto the floor from its back edge a support stands, in world pixels.</summary>
    public const float SupportInset = 3f;

    /// <summary>How far the centre may be moved along, in world pixels, to keep every slot off a block; and the step it's searched in.</summary>
    private const float MostSlide = 160f;
    private const float SlideStep = 2f;

    /// <summary>How finely Check walks the centres a fight could ask for, in world pixels.</summary>
    private const float CheckStep = 4f;

    /// <summary>How many supports Check makes sure fit behind your side.</summary>
    private const int MostSupportsChecked = 3;

    private readonly RoomDefinition _room;

    /// <summary>Lays the slots out on a room's floor about a centre.</summary>
    /// <param name="room">The room the fight's in.</param>
    /// <param name="centre">Where across the room the screen wants the middle, in world pixels: it's clamped and moved along as the remarks say.</param>
    public FightStage(RoomDefinition room, float centre)
    {
        _room = room;
        float x = Settle(room, centre);
        Centre = new Vector2(x, (room.BackEdgeAt(x) + room.FrontEdgeAt(x)) / 2f);
    }

    /// <summary>How far the stage reaches left of its centre: your side's back row at lane 0.</summary>
    public static float ReachLeft => FrontGap / 2f + (Slot.PartyLanes - 1) * LaneGap + BackStagger;

    /// <summary>How far the stage reaches right of its centre: the foes' back row at their last lane.</summary>
    public static float ReachRight => FrontGap / 2f + (Slot.FoeLanes - 1) * LaneGap + BackStagger;

    /// <summary>The middle of the stage: across, between the front rows, and down, halfway down the band. The camera's held on it.</summary>
    public Vector2 Centre { get; }

    /// <summary>Every slot on both sides: your 1 to 6, then the foes' 1 to 8.</summary>
    public static IEnumerable<Slot> AllSlots
    {
        get
        {
            foreach (Side side in new[] { Side.Party, Side.Foes })
                for (int number = 1; number <= 2 * Slot.LanesOf(side); number++) yield return Slot.Of(side, number);
        }
    }

    /// <summary>Where feet stand on a slot.</summary>
    /// <param name="slot">The slot.</param>
    public Vector2 FeetAt(Slot slot) => FeetAt(_room, Centre.X, slot);

    /// <summary>Where someone stands: on their slot, or between their two for someone big.</summary>
    /// <param name="formation">The fight's formation.</param>
    /// <param name="who">Who.</param>
    /// <returns>Where their feet go, or null if they've no slot (fallen back, or gone).</returns>
    public Vector2? FeetOf(Formation formation, Combatant who)
    {
        IReadOnlyList<Slot> slots = formation.SlotsOf(who);
        if (slots.Count == 0) return null;
        Vector2 sum = Vector2.Zero;
        foreach (Slot slot in slots) sum += FeetAt(slot);
        return sum / slots.Count;
    }

    /// <summary>The plate on the floor at a slot: PlateWidth by PlateHeight, its middle on the feet.</summary>
    /// <param name="slot">The slot.</param>
    public BoundingRectangle Plate(Slot slot)
    {
        Vector2 feet = FeetAt(slot);
        return new BoundingRectangle(feet.X - PlateWidth / 2f, feet.Y - PlateHeight / 2f, PlateWidth, PlateHeight);
    }

    /// <summary>Which slot a point on the floor is at: the nearest one whose feet are within half a lane across and halfway to the other row down.</summary>
    /// <param name="world">The point, in world pixels.</param>
    /// <returns>The slot, or null if the point's not near one.</returns>
    public Slot? SlotAt(Vector2 world)
    {
        Slot? best = null;
        float nearest = float.MaxValue;
        foreach (Slot slot in AllSlots)
        {
            Vector2 feet = FeetAt(slot);
            float band = _room.FrontEdgeAt(feet.X) - _room.BackEdgeAt(feet.X);
            float across = MathF.Abs(world.X - feet.X) / (LaneGap / 2f);
            float down = MathF.Abs(world.Y - feet.Y) / MathF.Max(1f, band * (FrontDepth - BackDepth) / 2f);
            if (across > 1f || down > 1f) continue;

            float distance = across * across + down * down;
            if (distance >= nearest) continue;
            nearest = distance;
            best = slot;
        }
        return best;
    }

    /// <summary>Where a support stands: on the band's back edge behind your side, between the back row's lanes, the supports spread about your middle lane.</summary>
    /// <param name="index">Which support, from 0.</param>
    /// <param name="count">How many there are.</param>
    public Vector2 SupportStand(int index, int count) => SupportStand(_room, Centre.X, index, count);

    /// <summary>
    /// Checks a room can host a fight: for every centre a fight could ask for, the stage settles
    /// somewhere every slot and the first few supports' places are on the floor, and the band's deep
    /// enough that the two rows' plates don't meet. --validate runs it for every room a built fight is in.
    /// </summary>
    /// <param name="room">The room.</param>
    /// <exception cref="InvalidOperationException">With where and what, if it can't.</exception>
    public static void Check(RoomDefinition room)
    {
        string where = $"Zones/{room.ZoneId}.txt: room {room.Id}";
        if (ReachLeft + ReachRight > room.Width)
            throw new InvalidOperationException($"{where} is {room.Width:0} world pixels across, and a fight's slots need {ReachLeft + ReachRight:0}.");

        (float lo, float hi) = Bounds(room);
        for (float wanted = lo; wanted <= hi + CheckStep / 2f; wanted += CheckStep)
        {
            float centre = Settle(room, wanted);
            foreach (Slot slot in AllSlots)
            {
                Vector2 feet = FeetAt(room, centre, slot);
                if (!room.OnFloor(feet))
                    throw new InvalidOperationException($"{where}: with the fight's middle at x {centre:0}, slot {slot.Side} {slot.Number} at ({feet.X:0}, {feet.Y:0}) is off the floor, and there's nowhere within {MostSlide:0} world pixels it isn't.");

                float band = room.FrontEdgeAt(feet.X) - room.BackEdgeAt(feet.X);
                if (band * (FrontDepth - BackDepth) < PlateHeight)
                    throw new InvalidOperationException($"{where}: the floor band at x {feet.X:0} is {band:0} world pixels deep, too shallow for the front and back rows' plates to keep apart.");
            }

            for (int count = 1; count <= MostSupportsChecked; count++)
                for (int i = 0; i < count; i++)
                {
                    Vector2 stand = SupportStand(room, centre, i, count);
                    if (!room.OnFloor(stand))
                        throw new InvalidOperationException($"{where}: with the fight's middle at x {centre:0}, a support's place at ({stand.X:0}, {stand.Y:0}) is off the floor.");
                }
        }
    }

    /// <summary>Where feet stand on a slot, for a stage about a centre.</summary>
    private static Vector2 FeetAt(RoomDefinition room, float centre, Slot slot)
    {
        float x = XOf(centre, slot);
        float back = room.BackEdgeAt(x), front = room.FrontEdgeAt(x);
        return new Vector2(x, back + (front - back) * (slot.Row == Row.Front ? FrontDepth : BackDepth));
    }

    /// <summary>How far across a slot stands, for a stage about a centre.</summary>
    private static float XOf(float centre, Slot slot)
    {
        float stagger = slot.Row == Row.Back ? BackStagger : 0f;
        return slot.Side == Side.Party
            ? centre - FrontGap / 2f - (Slot.PartyLanes - 1 - slot.Lane) * LaneGap - stagger
            : centre + FrontGap / 2f + slot.Lane * LaneGap + stagger;
    }

    /// <summary>Where a support stands, for a stage about a centre: lanes counted out from the middle, spread about the second.</summary>
    private static Vector2 SupportStand(RoomDefinition room, float centre, int index, int count)
    {
        float lane = 1f - (count - 1) / 2f + index;
        float x = centre - FrontGap / 2f - lane * LaneGap;
        return new Vector2(x, room.BackEdgeAt(x) + SupportInset);
    }

    /// <summary>The least and most a centre can be: the whole stage inside the fight range if it fits there, or else inside the room.</summary>
    private static (float Lo, float Hi) Bounds(RoomDefinition room)
    {
        float lo = room.FightLeft + ReachLeft, hi = room.FightRight - ReachRight;
        if (lo <= hi) return (lo, hi);

        float middle = Math.Clamp((room.FightLeft + room.FightRight) / 2f, ReachLeft, room.Width - ReachRight);
        return (middle, middle);
    }

    /// <summary>Where the centre ends up for where it's wanted: clamped, then moved along to the nearest place every slot's on the floor, if one's in reach.</summary>
    private static float Settle(RoomDefinition room, float wanted)
    {
        (float lo, float hi) = Bounds(room);
        float centre = Math.Clamp(wanted, lo, hi);
        if (Fits(room, centre)) return centre;

        float least = MathF.Max(ReachLeft, MathF.Min(lo, centre - MostSlide));
        float most = MathF.Min(room.Width - ReachRight, MathF.Max(hi, centre + MostSlide));
        for (float slide = SlideStep; slide <= MostSlide; slide += SlideStep)
        {
            if (centre - slide >= least && Fits(room, centre - slide)) return centre - slide;
            if (centre + slide <= most && Fits(room, centre + slide)) return centre + slide;
        }
        return centre;
    }

    /// <summary>Whether every slot of a stage about a centre is on the floor.</summary>
    private static bool Fits(RoomDefinition room, float centre)
    {
        foreach (Slot slot in AllSlots)
            if (!room.OnFloor(FeetAt(room, centre, slot))) return false;
        return true;
    }
}
