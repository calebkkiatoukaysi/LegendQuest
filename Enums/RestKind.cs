namespace LegendQuest;

/// <summary>
/// The rest that gives back something spent: an ability's uses, or spell slots.
/// </summary>
/// <remarks>A long rest gives back everything a short one does too, so the order matters: None, then Short, then Long.</remarks>
public enum RestKind
{
    /// <summary>Never spent, so there's nothing to give back.</summary>
    None,

    /// <summary>A short rest, or a long one.</summary>
    Short,

    /// <summary>Only a long rest.</summary>
    Long,
}
