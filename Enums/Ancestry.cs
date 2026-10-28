namespace LegendQuest;

/// <summary>
/// Who someone's people are. It changes how they look, and later what they're good at.
/// </summary>
/// <remarks>Saves keep it by name, so renaming one breaks saves and reordering them doesn't.</remarks>
public enum Ancestry
{
    /// <summary>From every corner of Fable.</summary>
    Human,

    /// <summary>Long-lived forest folk with pointed ears, who remember old wars.</summary>
    Sylvan,

    /// <summary>Mountain clans, short and broad, master smiths.</summary>
    Stoneborn,

    /// <summary>Horned, and descended from a pact with a fire spirit. Often mistrusted.</summary>
    Emberkin,

    /// <summary>Clans with a beast's ears and tail: the wolf, hawk and bear lines.</summary>
    Wildkin,

    /// <summary>Small, lucky, and clever with tools.</summary>
    Hearthling,
}
