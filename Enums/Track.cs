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

    /// <summary>"Salt and Lanterns": on board the Gull's Wager, at sea.</summary>
    Sea,

    /// <summary>"Meadowlark": out in the sunny green fields round Questeria, on the Old Road.</summary>
    Meadow,

    /// <summary>"Sunspire": the royal keep, all gold light and old ceremony.</summary>
    Court,

    /// <summary>"Steel": a fight. Strings and drums driving under the horns' call.</summary>
    Battle,

    /// <summary>"The Hunger": a boss fight. Steel's call bent out of shape over the black crystal's hum.</summary>
    Boss,
}
