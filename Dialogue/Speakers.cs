namespace LegendQuest.Dialogue;

/// <summary>
/// The speakers who aren't anyone: the narrator, and the cards.
/// </summary>
public static class Speakers
{
    /// <summary>The narrator: no face and no name, just the line.</summary>
    public const string Narrator = "narrator";

    /// <summary>A card: the line big in the middle of the screen, like a title or a fight that's over.</summary>
    public const string Card = "card";

    /// <summary>A chapter's card: the line is the chapter's name, like "Chapter 2  ·  The Rot in the Mirewood", big in the middle of the screen under the act, the way the ship coming in ends on Chapter 1's.</summary>
    public const string Chapter = "chapter";
}
