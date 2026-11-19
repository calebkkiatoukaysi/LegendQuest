using System;
using System.Collections.Generic;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using NumericsVector = System.Numerics.Vector2;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace LegendQuest.World;

/// <summary>
/// The floor a real-time fight is fought on in a room: the BattleArena the engine keeps everyone on,
/// built from the room's floor band and its fight range (Story/11-combat.md §2.1).
/// </summary>
/// <remarks>
/// <para>
/// Pure arithmetic on the room, so --validate can check every room a fight is fought in without a
/// window. The engine's floor is a rectangle, so the arena is the widest one that stays on the band
/// all the way across the fight range: from the lowest point of the band's back edge to the highest
/// of its front edge, each a little in from the edge. A room whose band slopes so far that this
/// leaves too little depth to fight in fails Check.
/// </para>
/// <para>
/// Everyone in the room starts where they stand (the screen passes their feet), so the arena's own
/// places are only for those who turn up: foes who aren't in the room stand in the encounter's slots
/// ahead of the party (round the foes who are, if any are), four lanes across the band and a second
/// line behind; guests who come from nowhere stand behind the party; waves walk in from the far end
/// of the fight range on the foes' side; and supports stand on the back edge behind the party.
/// </para>
/// </remarks>
public static class BattleFloor
{
    /// <summary>How far in from the band's edges the arena stays, in world pixels, so nobody's feet sit on the line.</summary>
    public const float Inset = 3f;

    /// <summary>The least depth of floor a fight needs, in world pixels: room for a front line and a back line to pass each other.</summary>
    public const float LeastDepth = 18f;

    /// <summary>The least width of floor a fight needs, in world pixels: the party and the foes a long reach apart with room behind each.</summary>
    public const float LeastWidth = 160f;

    /// <summary>How far ahead of the party the foes' front line stands when none of them is in the room, in world pixels.</summary>
    public const float FoesAhead = 90f;

    /// <summary>How far behind their front line the foes' second line stands, in world pixels.</summary>
    public const float SecondLine = 34f;

    /// <summary>How far behind the party a guest who comes from nowhere stands, and a support, in world pixels.</summary>
    public const float GuestsBehind = 30f;
    public const float SupportBehind = 46f;

    /// <summary>How finely the band is walked to find its edges, in world pixels.</summary>
    private const float Step = 2f;

    /// <summary>Where each lane stands down the arena's depth, from its far edge (0) to its near edge (1).</summary>
    private static readonly float[] Lanes = { 0.2f, 0.45f, 0.7f, 0.95f };

    /// <summary>The arena for a fight in a room: its floor, the foes' slots, where waves come in and where a support stands.</summary>
    /// <param name="room">The room.</param>
    /// <param name="party">Where the party stands: the leader's feet, in world pixels.</param>
    /// <param name="foes">Where the foes standing in the room stand, on average, or null if none is.</param>
    public static BattleArena For(RoomDefinition room, Vector2 party, Vector2? foes)
    {
        (float left, float right, float top, float bottom) = Band(room);
        float partyX = Math.Clamp(party.X, left, right);

        // The foes' side: where the foes in the room are, or else the side of the party with more room.
        float ahead = foes is Vector2 standing ? MathF.Sign(standing.X - partyX) : 0f;
        if (ahead == 0f) ahead = right - partyX >= partyX - left ? 1f : -1f;
        float front = foes is Vector2 at ? at.X : partyX + ahead * FoesAhead;
        front = Math.Clamp(front, left + SecondLine, right - SecondLine);

        var slots = new List<NumericsVector>();
        foreach (float depth in Lanes) slots.Add(new NumericsVector(front, Down(top, bottom, depth)));
        foreach (float depth in Lanes) slots.Add(new NumericsVector(Math.Clamp(front + ahead * SecondLine, left, right), Down(top, bottom, depth - 0.12f)));

        float behind = Math.Clamp(partyX - ahead * GuestsBehind, left, right);
        var partyStarts = new List<NumericsVector>();
        foreach (float depth in Lanes) partyStarts.Add(new NumericsVector(behind, Down(top, bottom, depth)));

        float edge = ahead > 0f ? right : left;
        var entries = new List<NumericsVector>();
        foreach (float depth in new[] { 0.3f, 0.7f, 0.5f, 0.1f, 0.9f }) entries.Add(new NumericsVector(edge, Down(top, bottom, depth)));

        float support = Math.Clamp(partyX - ahead * SupportBehind, left, right);
        return new BattleArena
        {
            Left = left, Right = right, Top = top, Bottom = bottom,
            PartyStarts = partyStarts, FoeSlots = slots, Entries = entries,
            SupportSpot = new NumericsVector(support, room.BackEdgeAt(support) + Inset),
        };
    }

    /// <summary>The widest rectangle of floor that stays on the room's band right across its fight range, a little in from the edges.</summary>
    /// <param name="room">The room.</param>
    /// <returns>Its left, right, far (top) and near (bottom) edges, in world pixels.</returns>
    public static (float Left, float Right, float Top, float Bottom) Band(RoomDefinition room)
    {
        float left = room.FightLeft + Inset, right = room.FightRight - Inset;
        float top = float.MinValue, bottom = float.MaxValue;
        for (float x = left; x <= right + Step / 2f; x += Step)
        {
            top = MathF.Max(top, room.BackEdgeAt(x));
            bottom = MathF.Min(bottom, room.FrontEdgeAt(x));
        }
        return (left, right, top + Inset, bottom - Inset);
    }

    /// <summary>
    /// Checks a room can host a real-time fight: its fight range is wide enough, and the band is deep
    /// enough across all of it. --validate runs it for every room a built fight is fought in.
    /// </summary>
    /// <param name="room">The room.</param>
    /// <exception cref="InvalidOperationException">With where and what, if it can't.</exception>
    public static void Check(RoomDefinition room)
    {
        string where = $"Zones/{room.ZoneId}.txt: room {room.Id}";
        (float left, float right, float top, float bottom) = Band(room);
        if (right - left < LeastWidth)
            throw new InvalidOperationException($"{where}: its fight range is {right - left:0} world pixels across, and a fight needs {LeastWidth:0}.");
        if (bottom - top < LeastDepth)
            throw new InvalidOperationException($"{where}: the floor band is only {bottom - top:0} world pixels deep right across the fight range ({top:0} to {bottom:0}), and a fight needs {LeastDepth:0}.");
    }

    /// <summary>A depth down the arena, from its far edge (0) to its near edge (1).</summary>
    private static float Down(float top, float bottom, float share) => top + (bottom - top) * Math.Clamp(share, 0f, 1f);
}
