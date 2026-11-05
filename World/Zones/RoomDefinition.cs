using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// One room of a zone, as read out of its zone file: a side-on painted scene you walk along, its
/// floor band, its tile strip, its ways out, and everything placed in it.
/// </summary>
/// <remarks>
/// Data only, in world pixels: the file is in canvas pixels and ZoneLoader divides everything by
/// ZoneCatalog.RoomScale once, so walkers, props and reaches keep the numbers they had on the maps.
/// A room is RoomHeight tall and Width wide, and nothing changes it after it's loaded, except that
/// ZoneCatalog fills in which room an exit into another zone arrives in once every zone is read.
/// </remarks>
public sealed class RoomDefinition
{
    /// <summary>What the zone file calls it, unique in its zone. Saves keep it, with the zone, in Visited.</summary>
    public required string Id { get; init; }

    /// <summary>The zone it's in.</summary>
    public required string ZoneId { get; init; }

    /// <summary>What it's called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>Where its painted layers are, under Content/Rooms: zone/room unless the file says otherwise.</summary>
    public required string Paint { get; init; }

    /// <summary>Whether it's a street (with a sky and parallax) or an interior.</summary>
    public required RoomKind Kind { get; init; }

    /// <summary>How wide it is, in world pixels.</summary>
    public required float Width { get; init; }

    /// <summary>The floor band, where feet can stand: a simple polygon, its last point joined to its first.</summary>
    public required IReadOnlyList<Vector2> Floor { get; init; }

    /// <summary>The ellipses on the floor that feet can't go into.</summary>
    public required IReadOnlyList<RoomBlock> Blocks { get; init; }

    /// <summary>The left of where a fight in it stands, in world pixels.</summary>
    public required float FightLeft { get; init; }

    /// <summary>The right of where a fight in it stands.</summary>
    public required float FightRight { get; init; }

    /// <summary>Its tile strip.</summary>
    public required TileLayers Tiles { get; init; }

    /// <summary>How far down the room the tile strip starts, in world pixels. An interior's starts at the top, with the back wall.</summary>
    public required float TilesTop { get; init; }

    /// <summary>The props standing in its painted layers.</summary>
    public required IReadOnlyList<RoomScenery> Scenery { get; init; }

    /// <summary>The little things moving in it.</summary>
    public required IReadOnlyList<RoomLife> Life { get; init; }

    /// <summary>Whether it's dark, which means the darkness is laid over it with the lights cut out of it.</summary>
    public required bool Dark { get; init; }

    /// <summary>How much light gets everywhere in a dark room, 0 (none) to 1 (as if it weren't dark).</summary>
    public required float Ambient { get; init; }

    /// <summary>The colour of the light in it, as a map's tint is. White unless the file says.</summary>
    public required Color Tint { get; init; }

    /// <summary>Its states and the flag that turns each on (when protectorate = act3). Read and kept, for the acts that use them.</summary>
    public required IReadOnlyDictionary<string, string> States { get; init; }

    /// <summary>Where the party can arrive in it, by name. Spawn names are unique in the zone, so a name says which room as well.</summary>
    public required IReadOnlyDictionary<string, MapSpawn> Spawns { get; init; }

    /// <summary>Its ends and stairs that go somewhere.</summary>
    /// <remarks>Settable inside World only so ZoneCatalog can fill in the room an exit into another zone arrives in.</remarks>
    public IReadOnlyList<RoomExit> Exits { get; internal set; } = Array.Empty<RoomExit>();

    /// <summary>Its doors.</summary>
    /// <remarks>Settable inside World for the same reason as Exits.</remarks>
    public IReadOnlyList<RoomDoor> Doors { get; internal set; } = Array.Empty<RoomDoor>();

    /// <summary>Everyone standing in it who isn't in the party, and the creatures.</summary>
    public required IReadOnlyList<MapNpc> Npcs { get; init; }

    /// <summary>The strips that do something when you walk into them. Each spans the floor's whole depth.</summary>
    public required IReadOnlyList<MapTrigger> Triggers { get; init; }

    /// <summary>The chests, signs, levers and waypoints.</summary>
    public required IReadOnlyList<MapInteractable> Things { get; init; }

    /// <summary>The props standing on its floor, or hung on its back wall.</summary>
    public required IReadOnlyList<MapProp> Props { get; init; }

    /// <summary>The pools of light that aren't a prop's.</summary>
    public required IReadOnlyList<MapLight> Lights { get; init; }

    /// <summary>Where anyone waiting for a place in the party stands, in order.</summary>
    public required IReadOnlyList<MapCamp> Camp { get; init; }

    /// <summary>The conversations that play by themselves while you're in it, once their flags say so.</summary>
    public required IReadOnlyList<MapScene> Scenes { get; init; }

    /// <summary>The back of the floor band at x: the smallest y on its edge there. A door stands on it.</summary>
    /// <param name="x">How far along, in world pixels. Off either end, the end's.</param>
    public float BackEdgeAt(float x) => EdgeAt(x, back: true);

    /// <summary>The front of the floor band at x: the largest y on its edge there.</summary>
    /// <param name="x">How far along, in world pixels. Off either end, the end's.</param>
    public float FrontEdgeAt(float x) => EdgeAt(x, back: false);

    /// <summary>Whether feet can stand here: inside the floor band and outside every block.</summary>
    /// <param name="feet">Where, in world pixels.</param>
    public bool OnFloor(Vector2 feet)
    {
        if (!Inside(Floor, feet)) return false;
        foreach (RoomBlock block in Blocks)
            if (block.Contains(feet)) return false;
        return true;
    }

    /// <summary>Whether a point is inside a polygon, by the even-odd rule. A point on an edge counts as in.</summary>
    /// <param name="polygon">The polygon, its last point joined to its first.</param>
    /// <param name="point">The point.</param>
    internal static bool Inside(IReadOnlyList<Vector2> polygon, Vector2 point)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
        {
            Vector2 a = polygon[i], b = polygon[j];
            if (OnSegment(a, b, point)) return true;
            if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < a.X + (point.Y - a.Y) * (b.X - a.X) / (b.Y - a.Y))
                inside = !inside;
        }
        return inside;
    }

    /// <summary>Whether a point lies on the segment from a to b.</summary>
    private static bool OnSegment(Vector2 a, Vector2 b, Vector2 point)
    {
        const float Slack = 0.001f;
        float cross = (b.X - a.X) * (point.Y - a.Y) - (b.Y - a.Y) * (point.X - a.X);
        return MathF.Abs(cross) <= Slack
            && point.X >= MathF.Min(a.X, b.X) - Slack && point.X <= MathF.Max(a.X, b.X) + Slack
            && point.Y >= MathF.Min(a.Y, b.Y) - Slack && point.Y <= MathF.Max(a.Y, b.Y) + Slack;
    }

    /// <summary>The nearest or furthest y where a vertical line at x meets the floor's edge.</summary>
    private float EdgeAt(float x, bool back)
    {
        float left = float.MaxValue, right = float.MinValue;
        foreach (Vector2 point in Floor)
        {
            left = MathF.Min(left, point.X);
            right = MathF.Max(right, point.X);
        }
        x = Math.Clamp(x, left, right);

        float best = back ? float.MaxValue : float.MinValue;
        for (int i = 0, j = Floor.Count - 1; i < Floor.Count; j = i++)
        {
            Vector2 a = Floor[j], b = Floor[i];
            if (x < MathF.Min(a.X, b.X) || x > MathF.Max(a.X, b.X)) continue;

            // An upright edge meets the line along its whole length; its ends are the two candidates.
            float y0, y1;
            if (a.X == b.X) (y0, y1) = (a.Y, b.Y);
            else y0 = y1 = a.Y + (x - a.X) / (b.X - a.X) * (b.Y - a.Y);
            best = back ? MathF.Min(best, MathF.Min(y0, y1)) : MathF.Max(best, MathF.Max(y0, y1));
        }
        return best;
    }
}
