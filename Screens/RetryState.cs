using System.Collections.Generic;
using LegendQuest.Combat;

namespace LegendQuest.Screens;

/// <summary>
/// What's remembered about fights lost this session: how many times in a row each has been lost, and
/// which the player chose to fight once more on Story.
/// </summary>
/// <remarks>
/// 11-combat §6: retrying a fight starts it fresh and costs nothing, Balanced offers to fight it
/// again on Story, and Story offers to win it outright once it's been lost
/// DifficultyRules.LossesBeforeWin times. None of that belongs in the save (a retry loads the
/// autosave from before the fight, which would forget it), so it lives here, in memory, for as long
/// as the game's running. The choice of Story is for the next go only: starting the fight uses it
/// up. Winning a fight forgets its losses.
/// </remarks>
public static class RetryState
{
    private static readonly Dictionary<string, int> Losses = new();
    private static readonly HashSet<string> OnStory = new();

    /// <summary>How many times in a row a fight's been lost this session.</summary>
    /// <param name="fight">The encounter's id.</param>
    public static int LossesIn(string fight) => Losses.GetValueOrDefault(fight ?? string.Empty);

    /// <summary>Counts a loss.</summary>
    /// <param name="fight">The encounter's id.</param>
    public static void Lost(string fight) => Losses[fight ?? string.Empty] = LossesIn(fight) + 1;

    /// <summary>Forgets a fight that's been won: its losses, and that it was to be fought on Story.</summary>
    /// <param name="fight">The encounter's id.</param>
    public static void Won(string fight)
    {
        Losses.Remove(fight ?? string.Empty);
        OnStory.Remove(fight ?? string.Empty);
    }

    /// <summary>Says a fight's next go is to be fought on Story, whatever the setting.</summary>
    /// <param name="fight">The encounter's id.</param>
    public static void PlayOnStory(string fight) => OnStory.Add(fight ?? string.Empty);

    /// <summary>The difficulty a fight starting now is fought on (DifficultyRules.ForFight): Story if the player chose that for this go, which uses the choice up, or else the setting.</summary>
    /// <param name="fight">The encounter's id.</param>
    /// <param name="setting">The difficulty in the options.</param>
    public static Difficulty Start(string fight, Difficulty setting) => DifficultyRules.ForFight(setting, OnStory.Remove(fight ?? string.Empty));

    /// <summary>What the game-over screen offers for a fight just lost (DifficultyRules.Offered), its loss already counted.</summary>
    /// <param name="fight">The encounter's id.</param>
    /// <param name="difficulty">The difficulty it was fought on.</param>
    public static LossOptions Options(string fight, Difficulty difficulty) => DifficultyRules.Offered(difficulty, LossesIn(fight));
}
