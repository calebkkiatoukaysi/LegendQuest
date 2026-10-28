namespace LegendQuest;

/// <summary>
/// Which part of the proof run is playing. It goes through the screens in the same order a player would.
/// </summary>
public enum ProofStage
{
    /// <summary>The title, the options and the save slots.</summary>
    Menus,

    /// <summary>A new game: the Prologue's card, you made, your voice, the Soul-Bound made, the memory and the oath.</summary>
    Creation,

    /// <summary>The deck of the Gull's Wager at night: the Prologue's voyage, the sea-wraiths, and Questeria at dawn.</summary>
    Voyage,

    /// <summary>The harbour: the ship sailing in, the walk off it, Act 1's card, the Red Gulls and Captain Hale, then up to the city.</summary>
    Docks,

    /// <summary>Walking round Questeria, Fennick's board, finding its waypoint, the map, then into the Gilded Flagon.</summary>
    Questeria,

    /// <summary>Inside the Gilded Flagon: the innkeeper's errand, and out through the door.</summary>
    Flagon,

    /// <summary>Outside the Flagon's door again, then out through the city gate.</summary>
    QuesteriaOut,

    /// <summary>The old road: the trigger, the chest, the sign, the waypoint, and fast travel back to Questeria.</summary>
    Road,

    /// <summary>Back in Questeria off the waypoint, and straight back to the road by it.</summary>
    QuesteriaAgain,

    /// <summary>On the road again by its waypoint, and up the path into the barrow.</summary>
    RoadAgain,

    /// <summary>The barrow: dark, the trigger, the locked door, the lever and its door, the chest, the map, then out through the pause menu.</summary>
    Barrow,

    /// <summary>Back on the title after QUIT TO TITLE, and the slots with the autosave in them.</summary>
    TitleAgain,

    /// <summary>Writing the audio log and quitting.</summary>
    Done,
}
