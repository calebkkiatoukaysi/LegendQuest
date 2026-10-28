namespace LegendQuest.Story;

/// <summary>
/// Something the story hands you that matters: what it's called, and what it is.
/// </summary>
/// <param name="Id">What the scripts and the save call it.</param>
/// <param name="Name">What it's called on screen.</param>
/// <param name="Description">One line about it, for the toast and, later, the inventory.</param>
public sealed record KeyItem(string Id, string Name, string Description);
