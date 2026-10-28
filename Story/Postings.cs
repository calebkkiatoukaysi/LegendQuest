using System.Collections.Generic;

namespace LegendQuest.Story;

/// <summary>
/// The contracts on the Lantern Market's quest board.
/// </summary>
/// <remarks>
/// STORY.md's A1-C1-02: the main quest's posting, from the Rangers' Lodge, glows among the side
/// quests, which are hooks for now (Rats in the Flagon, the Missing Ledger, the Tollroad Bandits and
/// the Bellringer's Widow). Placeholders, like every line.
/// </remarks>
public static class Postings
{
    /// <summary>Every posting, top to bottom within its column.</summary>
    public static IReadOnlyList<Posting> All { get; } = new Posting[]
    {
        new(PostingColumn.Monsters, "Something in the Mirewood", "The Rangers' Lodge", "Animals gone mad, woodcutters missing. Pays triple.", true),
        new(PostingColumn.Monsters, "Rats in the Flagon", "The Gilded Flagon", "Giant rats in the cellar. Strange black eyes.", false),
        new(PostingColumn.Bandits, "The Tollroad Bandits", "The City Watch", "Highwaymen on the Mirewood road.", false),
        new(PostingColumn.LostAndFound, "The Missing Ledger", "A merchant", "Stolen. Last seen in the Low Ring.", false),
        new(PostingColumn.LostAndFound, "The Bellringer's Widow", "The bellringer's widow", "A wedding ring, lost in the harbour shallows.", false),
    };
}
