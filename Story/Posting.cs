namespace LegendQuest.Story;

/// <summary>One contract on the board.</summary>
/// <param name="Column">Which column it's in.</param>
/// <param name="Title">What it's called.</param>
/// <param name="Giver">Who posted it.</param>
/// <param name="Text">What it says, in a line.</param>
/// <param name="Main">Whether it's the main quest's, which glows.</param>
public sealed record Posting(PostingColumn Column, string Title, string Giver, string Text, bool Main);
