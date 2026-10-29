namespace LegendQuest.Story;

/// <summary>
/// One line of a banter: who says it, and what, in a bubble over their head.
/// </summary>
/// <param name="Speaker">Who says it: you, the Soul-Bound, a companion by their origin id, or someone on the map by their id there (tomas, gossip-a).</param>
/// <param name="Line">What they say. May hold {name}, {sb} and the other tokens, the same as a conversation's lines.</param>
public readonly record struct BanterLine(string Speaker, string Line);
