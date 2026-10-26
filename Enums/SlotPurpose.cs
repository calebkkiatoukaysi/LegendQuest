namespace LegendQuest;

/// <summary>
/// Why the save slots are open, which decides which slots can be picked.
/// </summary>
public enum SlotPurpose
{
    /// <summary>Picking where a new game will save. Only the numbered slots, and a full one asks first.</summary>
    NewGame,

    /// <summary>Picking a game to carry on. Any slot with a game in it, the quicksave and the autosave included.</summary>
    Load,
}
