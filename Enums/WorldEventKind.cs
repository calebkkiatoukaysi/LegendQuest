namespace LegendQuest;

/// <summary>
/// Something that happened out on a map, as WorldState reports it. The map screen picks the sound
/// from this, the same way The Black Box's table did from the lines of a round.
/// </summary>
public enum WorldEventKind
{
    /// <summary>A chest's lid came up. If there was nothing in it, the line says so.</summary>
    ChestOpened,

    /// <summary>Gold out of a chest. The line says how much.</summary>
    GoldTaken,

    /// <summary>A door swung open.</summary>
    DoorOpened,

    /// <summary>A door that's locked was tried.</summary>
    DoorLocked,

    /// <summary>A door that something else works was tried by hand.</summary>
    DoorStuck,

    /// <summary>A lever went over, one way or the other.</summary>
    LeverPulled,

    /// <summary>A sign was read. The line is what it says.</summary>
    SignRead,

    /// <summary>A waypoint was used for the first time.</summary>
    WaypointFound,

    /// <summary>A waypoint that's already been found was used, which opens the map to travel from it.</summary>
    WaypointUsed,

    /// <summary>A trigger went off with something to say.</summary>
    TriggerSaid,

    /// <summary>A trigger went off that starts a conversation. The text is the conversation's id.</summary>
    TriggerTalked,
}
