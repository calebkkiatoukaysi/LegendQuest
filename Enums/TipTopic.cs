namespace LegendQuest;

/// <summary>
/// What a tutorial tip is about. HOW TO PLAY gives each its own page, in this order.
/// </summary>
/// <remarks>Saved nowhere, so reordering is safe. HelpScreen names each page.</remarks>
public enum TipTopic
{
    /// <summary>Walking about, using things, going in, the map and the waypoints.</summary>
    Exploring,

    /// <summary>Talking, choices, the dice, the markers, what you're to do next and the quest board.</summary>
    Story,

    /// <summary>Who you're controlling, the companions, and whoever's waiting at camp.</summary>
    Party,

    /// <summary>Turns, moving, attacking, abilities, the Soul-Link and being Downed.</summary>
    Combat,

    /// <summary>The bag, wearing things, potions and loot.</summary>
    Gear,
}
