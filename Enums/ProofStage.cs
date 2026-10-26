namespace LegendQuest;

/// <summary>
/// Which part of the proof run is playing. It goes through the screens in the same order a player would.
/// </summary>
public enum ProofStage
{
    /// <summary>The title, the options, the save slots and the pause menu.</summary>
    Menus,

    /// <summary>Back on the title after QUIT TO TITLE, which is the end for now.</summary>
    TitleAgain,

    /// <summary>Writing the audio log and quitting.</summary>
    Done,
}
