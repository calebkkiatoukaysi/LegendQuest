namespace LegendQuest;

/// <summary>
/// What an effect in a conversation does to the game.
/// </summary>
public enum EffectKind
{
    /// <summary>Sets a flag.</summary>
    SetFlag,

    /// <summary>Clears a flag.</summary>
    ClearFlag,

    /// <summary>Adds to a meter (or takes away, with a negative amount), kept inside its range.</summary>
    AddMeter,

    /// <summary>Sets a meter to a number.</summary>
    SetMeter,

    /// <summary>Hands over gold.</summary>
    Gold,

    /// <summary>Hands over a key item.</summary>
    GiveItem,

    /// <summary>Takes a key item away.</summary>
    TakeItem,

    /// <summary>Changes what you're to do next.</summary>
    Objective,

    /// <summary>Someone joins the party, or waits at camp if it's full. Someone already waiting comes along once there's room.</summary>
    Recruit,

    /// <summary>Someone waiting at camp takes a companion's place in the party, and the companion waits in their seat: the id is "waiting member", both origin ids. Only the camp's offer does it, so it has no DialogueEffect of its own.</summary>
    Swap,

    /// <summary>Picks the voice you talk in.</summary>
    Voice,

    /// <summary>Takes the party to another map once the conversation's over: the id is "map spawn".</summary>
    Travel,

    /// <summary>Opens a shop once the conversation's over: the id is the shop's, in ShopCatalog.</summary>
    Shop,

    /// <summary>Starts a fight once the conversation's over, on the map it's on: the id is the encounter's, in EncounterCatalog.</summary>
    Fight,

    /// <summary>A long rest for everyone, in the party and waiting at camp: hit points full, spell slots and every use back. The id is the kind of rest.</summary>
    Rest,
}
