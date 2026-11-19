using System;
using System.Collections.Generic;
using System.Numerics;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// The floor a fight is fought on: the band everyone can stand on, in world pixels, where each side
/// starts, and where later waves walk in from.
/// </summary>
/// <remarks>
/// The screen builds one from the room (its floor band and where everyone stands when the fight
/// starts); the simulator uses Default, a 400-pixel room like the game's. Foes start where their
/// slot puts them unless their setup says where they stand (CombatantSetup.At): the front row
/// nearer the party, the back row behind it, lanes across the band's depth.
/// </remarks>
public sealed class BattleArena
{
    /// <summary>The floor's left edge.</summary>
    public float Left { get; init; }

    /// <summary>The floor's right edge.</summary>
    public float Right { get; init; }

    /// <summary>The floor's far edge (the top of the band on screen).</summary>
    public float Top { get; init; }

    /// <summary>The floor's near edge (the bottom of the band on screen).</summary>
    public float Bottom { get; init; }

    /// <summary>Where the party's side starts, in setup order: whoever has no position of their own takes the next.</summary>
    public IReadOnlyList<Vector2> PartyStarts { get; init; } = Array.Empty<Vector2>();

    /// <summary>Where each of the foes' eight slots stands, by slot number 1 to 8 (index 0 is slot 1).</summary>
    public IReadOnlyList<Vector2> FoeSlots { get; init; } = Array.Empty<Vector2>();

    /// <summary>Where waves walk in from, in turn.</summary>
    public IReadOnlyList<Vector2> Entries { get; init; } = Array.Empty<Vector2>();

    /// <summary>Where a support stands behind the party, for its arrows' and nets' paths.</summary>
    public Vector2 SupportSpot { get; init; }

    /// <summary>The floor's middle.</summary>
    public Vector2 Middle => new((Left + Right) / 2f, (Top + Bottom) / 2f);

    /// <summary>A point kept on the floor.</summary>
    /// <param name="point">The point.</param>
    public Vector2 Clamp(Vector2 point) => new(Math.Clamp(point.X, Left, Right), Math.Clamp(point.Y, Top, Bottom));

    /// <summary>Whether a point is on the floor.</summary>
    /// <param name="point">The point.</param>
    public bool Holds(Vector2 point) => point.X >= Left && point.X <= Right && point.Y >= Top && point.Y <= Bottom;

    /// <summary>Where a foe in a slot starts.</summary>
    /// <param name="slot">The slot.</param>
    public Vector2 FoeStart(Slot slot)
    {
        int number = slot.Number;
        return number >= 1 && number <= FoeSlots.Count ? FoeSlots[number - 1] : Middle;
    }

    /// <summary>A room like the game's: 400 world pixels wide, its floor band from 165 to 215 down, the party on the left and the foes on the right.</summary>
    /// <param name="width">How wide the room is.</param>
    public static BattleArena Default(float width = 400f)
    {
        const float top = 165f, bottom = 215f;
        float party = width * 0.32f, foes = width * 0.62f;
        float[] lanes = { 172f, 184f, 196f, 208f };
        var partyStarts = new List<Vector2>
        {
            new(party, 184f), new(party, 200f), new(party - 34f, 176f), new(party - 34f, 192f), new(party - 34f, 208f), new(party - 64f, 186f),
        };
        var slots = new List<Vector2>();
        foreach (float y in lanes) slots.Add(new Vector2(foes, y));
        foreach (float y in lanes) slots.Add(new Vector2(foes + 40f, y));
        var entries = new List<Vector2> { new(width - 12f, 180f), new(width - 12f, 200f), new(width - 12f, 190f), new(width - 12f, 172f), new(width - 12f, 208f) };
        return new BattleArena
        {
            Left = 12f, Right = width - 12f, Top = top, Bottom = bottom,
            PartyStarts = partyStarts, FoeSlots = slots, Entries = entries, SupportSpot = new Vector2(24f, 178f),
        };
    }
}
