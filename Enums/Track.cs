namespace LegendQuest;

/// <summary>
/// The songs. Each screen (or map) asks for one when it opens and AudioManager fades between them.
/// </summary>
/// <remarks>Map files name their song by these names (music = Exploring), so renaming one means fixing the maps too. The loader says which.</remarks>
public enum Track
{
    /// <summary>"Embers": the title, the options and the save slots.</summary>
    Title,

    /// <summary>"Hearthsmoke": walking about on a map that's lit.</summary>
    Exploring,

    /// <summary>"Cold Stone": walking about in the dark, underground.</summary>
    Underground,
}
