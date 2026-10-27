using System;
using System.Globalization;
using LegendQuest.World;

namespace LegendQuest.Save;

/// <summary>
/// One slot as the save slots screen sees it: which slot, what is in it, and the line to print on it.
/// </summary>
/// <remarks>The screen never touches the disk. It asks SaveSystem for these and draws them.</remarks>
public class SaveSlot
{
    /// <summary>Which slot this is, from 0 to SaveSystem.SlotCount - 1. The last two are the quicksave and the autosave.</summary>
    public int Index { get; init; }

    /// <summary>What the slot turned out to be.</summary>
    public SaveSlotState State { get; init; }

    /// <summary>The game in the slot, or null unless State is Occupied.</summary>
    public SaveData Data { get; init; }

    /// <summary>Whether there is a game in here to load or erase.</summary>
    public bool HasGame => State == SaveSlotState.Occupied;

    /// <summary>The slot's name: a numeral for the ones you pick yourself, or what fills it for the other two.</summary>
    public string Name => Index switch
    {
        SaveSystem.QuickSlot => "QUICKSAVE",
        SaveSystem.AutoSlot => "AUTOSAVE",
        _ => "SLOT " + Numeral(Index + 1),
    };

    /// <summary>The line printed under the slot's name: what is in it, or why there is nothing to load.</summary>
    public string Summary => State switch
    {
        SaveSlotState.Occupied =>
            string.Format(CultureInfo.InvariantCulture, "{0}  ·  {1}  ·  {2}",
                MapCatalog.Get(Data.MapId).Name.ToUpperInvariant(), Clock(Data.Playtime),
                Data.SavedUtc.ToLocalTime().ToString("d MMM yyyy", CultureInfo.InvariantCulture).ToUpperInvariant()),

        // Said plainly. A corrupt slot is the one place to drop the atmosphere and just tell the player.
        SaveSlotState.Unreadable => "UNREADABLE  ·  ERASE TO REUSE",

        _ => "EMPTY",
    };

    /// <summary>Formats a playtime as hours and minutes, which is all a slot has room for.</summary>
    /// <param name="time">The time to format.</param>
    private static string Clock(TimeSpan time) =>
        string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}", (int)time.TotalHours, time.Minutes);

    /// <summary>Roman numerals for 1 to 3, which is as far as the numbered slots go.</summary>
    /// <param name="value">The number to spell.</param>
    private static string Numeral(int value) => value switch
    {
        1 => "I",
        2 => "II",
        3 => "III",
        _ => value.ToString(CultureInfo.InvariantCulture),
    };
}
