namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// The Prologue, "Two Names, One Oath": the title card, who drives you, the Soul-Bound remembered,
/// the night on the Gull's Wager, the sea-wraiths, and Questeria at dawn.
/// </summary>
/// <remarks>
/// The scenes themselves are compiled from Story/04-script-prologue-act1.md into StoryPrologueAct1
/// (tools/story/compile_scripts.py); this class keeps the flags they set, by name, for the maps, the
/// fights and the code that read them. The voyage ends on the bell and the wraiths coming over the
/// rail, and the fight they start is the guided one (EncounterCatalog's p-05-wraiths): it teaches
/// moving, attacking, an ability, the Soul-Link and ending a turn. Winning it plays the dawn.
/// </remarks>
public static class Prologue
{

    /// <summary>The flag P-04 sets when you warn the Soul-Bound about crowns. A1-C6-04 reads it.</summary>
    public const string SbWarned = "sb-warned-p";

    /// <summary>The flag the end of the Prologue sets: you're coming into Questeria's harbour.</summary>
    public const string Done = "prologue-done";

    /// <summary>The flag the lookout's bell sets: the sea-wraiths are over the Gull's Wager's rail.</summary>
    public const string WraithsSighted = "wraiths-sighted";

    /// <summary>The sea-wraiths are beaten: they're gone from the deck.</summary>
    public const string WraithsBeaten = "wraiths-beaten";
}
