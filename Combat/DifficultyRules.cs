using System;

namespace LegendQuest.Combat;

/// <summary>
/// What each difficulty changes: the foes' hit points and rolls, the Focus the party's side starts
/// on, the healing after a story fight, what losing offers, and random fights.
/// </summary>
/// <remarks>
/// 11-combat.md §6.1's table. The hit points and rolls are only ever the foes', never a guest's. The
/// difficulty's a setting (Settings.Mode), not part of a game. How many times a fight's been lost,
/// and whether it's being retried on Story for this once, are the screens' to keep while the game's
/// running (RetryState); Offered turns them into what the game over screen shows.
/// </remarks>
public static class DifficultyRules
{
    /// <summary>How many times a fight has to be lost on Story before "Win this fight" is offered.</summary>
    public const int LossesBeforeWin = 2;

    /// <summary>What a foe's hit points are multiplied by.</summary>
    /// <param name="difficulty">The difficulty.</param>
    public static float FoeHp(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Story => 0.65f,
        Difficulty.Tactician => 1.25f,
        _ => 1f,
    };

    /// <summary>What's added to a foe's attack bonuses, its abilities' DCs and its spell save DC.</summary>
    /// <param name="difficulty">The difficulty.</param>
    public static int FoeRolls(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Story => -2,
        Difficulty.Tactician => 1,
        _ => 0,
    };

    /// <summary>The Focus everyone on the party's side starts a fight on, guests included.</summary>
    /// <param name="difficulty">The difficulty.</param>
    public static int StartFocus(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Story => 60,
        Difficulty.Tactician => 30,
        _ => 40,
    };

    /// <summary>The hit points a party member gets back after a story fight's won (any fight but a random one): all that's missing on Story, half of it rounded up on Balanced, none on Tactician.</summary>
    /// <param name="difficulty">The difficulty.</param>
    /// <param name="hp">Their hit points now.</param>
    /// <param name="max">Their most.</param>
    public static int HealAfterStoryFight(Difficulty difficulty, int hp, int max)
    {
        int missing = Math.Max(0, max - hp);
        return difficulty switch
        {
            Difficulty.Story => missing,
            Difficulty.Tactician => 0,
            _ => (missing + 1) / 2,
        };
    }

    /// <summary>What a difficulty offers after a fight's lost, everything it ever does: Story retries, and wins the fight for you after LossesBeforeWin losses; Balanced retries, or retries on Story for this fight only; Tactician only retries.</summary>
    /// <param name="difficulty">The difficulty.</param>
    public static LossOptions Losing(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Story => LossOptions.Retry | LossOptions.WinAfterTwoLosses,
        Difficulty.Tactician => LossOptions.Retry,
        _ => LossOptions.Retry | LossOptions.RetryOnStory,
    };

    /// <summary>What the game over screen offers after this loss: Losing's, with "Win this fight" only once the fight's been lost LossesBeforeWin times.</summary>
    /// <remarks>A fight being retried on Story for this once is lost on Story, so pass Story for it: it then offers what Story does.</remarks>
    /// <param name="difficulty">The difficulty the fight was lost on.</param>
    /// <param name="losses">How many times this fight's been lost in a row, this one included.</param>
    public static LossOptions Offered(Difficulty difficulty, int losses)
    {
        LossOptions options = Losing(difficulty);
        return losses >= LossesBeforeWin ? options : options & ~LossOptions.WinAfterTwoLosses;
    }

    /// <summary>The difficulty a fight's played on: Story if it's being retried on Story for this once, or else the setting.</summary>
    /// <param name="setting">The difficulty in the settings.</param>
    /// <param name="retryOnStory">Whether the player picked "retry on Story" after losing it.</param>
    public static Difficulty ForFight(Difficulty setting, bool retryOnStory) => retryOnStory ? Difficulty.Story : setting;

    /// <summary>How random fights go. A stub until they come in with M10.</summary>
    /// <param name="difficulty">The difficulty.</param>
    public static RandomFights Random(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Story => RandomFights.Optional,
        Difficulty.Tactician => RandomFights.MoreAmbushes,
        _ => RandomFights.On,
    };
}
