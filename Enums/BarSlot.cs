namespace LegendQuest;

/// <summary>
/// Where an ability goes on the bar at the bottom of a fight: one of the six primary buttons, the
/// small strip beside them, or nowhere.
/// </summary>
/// <remarks>The six are Auto, three Skills, the Ultimate and the Potion, on keys 1 to 6; the strip is on 7 to 0. New ones go on the end.</remarks>
public enum BarSlot
{
    /// <summary>The first button: the class's auto attack, which gives Focus back.</summary>
    Auto,

    /// <summary>One of the three skill buttons, in kit order.</summary>
    Skill,

    /// <summary>The ultimate's button: once a fight.</summary>
    Ultimate,

    /// <summary>The potion's button.</summary>
    Potion,

    /// <summary>On the strip, for everyone: Dodge and Help.</summary>
    Shared,

    /// <summary>On the strip, using something up out of the party's bag: a flask to throw, an elixir.</summary>
    Bag,

    /// <summary>Not a button: always on (Defence, Protection), shown by the portrait.</summary>
    Passive,

    /// <summary>Not on the bar at all: reached some other way (the Soul-Link Strike, from its sigil), or in nobody's kit.</summary>
    Hidden,
}
