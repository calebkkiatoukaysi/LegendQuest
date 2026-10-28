namespace LegendQuest;

/// <summary>
/// Something a conversation did that the screen shows or plays: what DialogueRunner hands back.
/// </summary>
public enum DialogueEventKind
{
    /// <summary>A flag was set or cleared. Nothing to show; the world catches up when the conversation's over.</summary>
    Flag,

    /// <summary>A meter moved. A companion's approval gets a toast if they're in the party.</summary>
    Meter,

    /// <summary>Gold was handed over.</summary>
    Gold,

    /// <summary>A key item was handed over or taken.</summary>
    Item,

    /// <summary>What you're to do next changed.</summary>
    Objective,

    /// <summary>Someone joined, or went to wait at camp.</summary>
    Recruited,

    /// <summary>A check was rolled. The result is on the event.</summary>
    Check,

    /// <summary>The party's to go to another map once the conversation's over.</summary>
    Travel,
}
