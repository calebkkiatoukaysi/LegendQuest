namespace LegendQuest.Story;

/// <summary>
/// One meter the story keeps: what it starts at, the range it stays in, and whose approval it is,
/// if it's a companion's.
/// </summary>
/// <param name="Id">What the scripts and the save call it.</param>
/// <param name="Start">What it is in a new game.</param>
/// <param name="Min">The lowest it goes.</param>
/// <param name="Max">The highest it goes.</param>
/// <param name="Companion">The companion whose approval it is (their origin id), for the "approves" toast, or null for one that's hidden or isn't anyone's.</param>
public sealed record MeterDefinition(string Id, int Start, int Min, int Max, string Companion);
