namespace LegendQuest;

/// <summary>
/// Every sound effect. The file is the name in kebab case under Content/Sfx, so MenuMove is
/// Sfx/menu-move.
/// </summary>
/// <remarks>Grouped by where you hear them. If you rename one, rename its file and its line in Content.mgcb too.</remarks>
public enum Sfx
{
    // Menus.

    /// <summary>Moving the cursor to another plate.</summary>
    MenuMove,

    /// <summary>Pressing a plate.</summary>
    MenuConfirm,

    /// <summary>Backing out of a menu.</summary>
    MenuBack,

    /// <summary>Changing a value with left or right (the options).</summary>
    OptionChange,

    /// <summary>Opening the pause menu.</summary>
    Pause,

    // Walking.

    /// <summary>One step on something hard. The pitch and pan change every step.</summary>
    FootstepStone,

    /// <summary>One step on grass or dirt.</summary>
    FootstepGrass,

    // Using things.

    /// <summary>A door swinging open.</summary>
    Door,

    /// <summary>Trying a door that won't open.</summary>
    DoorLocked,

    /// <summary>A chest's lid coming up.</summary>
    Chest,

    /// <summary>A lever going over.</summary>
    Lever,

    /// <summary>Picking something up: coins, for now.</summary>
    Pickup,

    /// <summary>A waypoint stone waking up the first time it's found.</summary>
    Waypoint,

    // Conversations.

    /// <summary>The d20 thrown across the panel.</summary>
    DiceRoll,

    /// <summary>A check passing.</summary>
    CheckPass,

    /// <summary>A check failing.</summary>
    CheckFail,

    /// <summary>A companion approving of what you said.</summary>
    Approve,

    /// <summary>A companion disapproving.</summary>
    Disapprove,

    // The story.

    /// <summary>One heartbeat, then two in rhythm: the Prologue's title card.</summary>
    Heartbeat,

    /// <summary>A ship's bell rung in alarm.</summary>
    ShipBell,
}
