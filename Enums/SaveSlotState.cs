namespace LegendQuest;

/// <summary>What a slot on disk turned out to be.</summary>
public enum SaveSlotState
{
    /// <summary>Nothing has ever been written here.</summary>
    Empty,

    /// <summary>A game was read out of it.</summary>
    Occupied,

    /// <summary>There is a file, but it could not be read. SaveSystem.LastError says why.</summary>
    Unreadable,
}
