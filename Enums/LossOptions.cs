using System;

namespace LegendQuest;

/// <summary>
/// What the game over screen offers after a lost fight, by difficulty (DifficultyRules.Losing).
/// </summary>
/// <remarks>Flags, since a difficulty can offer more than one. New ones go on the end, at the next power of two.</remarks>
[Flags]
public enum LossOptions
{
    /// <summary>Nothing beyond going back to the title.</summary>
    None = 0,

    /// <summary>Fight it again from the start.</summary>
    Retry = 1,

    /// <summary>Fight it again on Story, for this fight only.</summary>
    RetryOnStory = 2,

    /// <summary>"Win this fight", once it's been lost twice.</summary>
    WinAfterTwoLosses = 4,
}
