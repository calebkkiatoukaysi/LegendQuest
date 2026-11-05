using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// What IndicatorLayer needs to know about a room for one frame: who's in it and where, what's
/// still to be used, where the mouse and you are, and whether Alt's held.
/// </summary>
/// <remarks>
/// The room and its flags say which doors and ways out are there and which are locked; the rest is
/// what only the screen walking the room knows. Everything's in world pixels.
/// </remarks>
/// <param name="Room">The room.</param>
/// <param name="Flags">The game's flags, for which doors and ways out are there, the locked twins among them.</param>
/// <param name="ShowAll">Whether Alt is held, which shows every badge, name, label and glint at once.</param>
/// <param name="Leader">Where whoever you're controlling is standing: an end's label shows once they're near it.</param>
/// <param name="Mouse">The point in the room under the mouse.</param>
/// <param name="Plates">Everyone with a name over their head: the name, the top of their head, their marker, the badge beside the name for anyone you can buy from or train with (or null), and whether they're named. Townsfolk aren't, and their names only show with the mouse on them, or with Alt.</param>
/// <param name="Glints">Where a glint goes on each thing that's still to be used: on its picture, a little above the middle reads best.</param>
/// <param name="MarkerBehind">The marker for a room, by zone and room: what a badge or an end's chevrons show for the room through them. RoomMarkers.In, or a cache of it.</param>
public sealed record IndicatorFrame(RoomDefinition Room, ISet<string> Flags, bool ShowAll, Vector2 Leader, Vector2 Mouse,
    IReadOnlyList<(string Name, Vector2 Head, MarkerKind Marker, BadgeKind? Badge, bool Named)> Plates,
    IReadOnlyList<Vector2> Glints, Func<string, string, MarkerKind> MarkerBehind);
