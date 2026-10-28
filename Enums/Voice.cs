namespace LegendQuest;

/// <summary>
/// How you talk: picked once at the start, and some lines and options only show for one of them.
/// </summary>
/// <remarks>Saves keep it by name, so renaming one breaks saves and reordering them doesn't.</remarks>
public enum Voice
{
    /// <summary>Not picked yet. Only before the creation screen's question.</summary>
    None,

    /// <summary>Doing what's right, even when it costs.</summary>
    Honorable,

    /// <summary>Getting the job done.</summary>
    Pragmatic,

    /// <summary>A good story, and a better tavern at the end of it.</summary>
    Witty,
}
