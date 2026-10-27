namespace LegendQuest;

/// <summary>
/// Which part of the proof run is playing. It goes through the screens in the same order a player would.
/// </summary>
public enum ProofStage
{
    /// <summary>The title, the options and the save slots.</summary>
    Menus,

    /// <summary>Walking round the village, finding its waypoint, the map, then out to the east.</summary>
    Village,

    /// <summary>The old road: the trigger, the chest, the sign, the waypoint, and fast travel back to the village.</summary>
    Road,

    /// <summary>Back in the village off the waypoint, and straight back to the road by it.</summary>
    VillageAgain,

    /// <summary>On the road again by its waypoint, and up the path into the barrow.</summary>
    RoadAgain,

    /// <summary>The barrow: dark, the trigger, the locked door, the lever and its door, the chest, the map, then out through the pause menu.</summary>
    Barrow,

    /// <summary>Back on the title after QUIT TO TITLE, and the slots with the autosave in them.</summary>
    TitleAgain,

    /// <summary>Writing the audio log and quitting.</summary>
    Done,
}
