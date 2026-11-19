using System;

namespace LegendQuest.Characters.Motion;

/// <summary>
/// The feel spec's numbers in one place (research feel.md §10, Story/11-combat.md §8): how long a
/// blow freezes the two fighters, how long the target flashes, and how far it's knocked, so every
/// screen that stages a hit stages it the same way. Frames are at 60 a second.
/// </summary>
/// <remarks>
/// The freeze follows the fighting games' shape (a base and a slope on the damage, with a cap) at
/// about half their length, since a room has several fighters and a heartbeat of auto attacks, and
/// long freezes stacking up would feel like lag: 3 frames for a light blow, up to 10 for a heavy one,
/// 3 more for a critical, at least 10 for a kill, and never more than 14.
/// </remarks>
public static class Feel
{
    /// <summary>The frames a second everything here is counted in.</summary>
    public const float FramesPerSecond = 60f;

    /// <summary>The longest any freeze is, in frames.</summary>
    public const int MostHitStop = 14;

    /// <summary>A light blow's freeze, the least, in frames.</summary>
    public const int LeastHitStop = 3;

    /// <summary>How many frames the white flash holds for a blow and for a critical, and how many it takes to fade after.</summary>
    public const int FlashFrames = 2;
    public const int CriticalFlashFrames = 4;
    public const int FlashFadeFrames = 4;

    /// <summary>How long a spell's element tint stays on the target, in frames, after its white flash.</summary>
    public const int ElementTintFrames = 6;

    /// <summary>How far a blow knocks its target, in world pixels: an auto attack, a heavy skill, a spell, and a kill's pop; a critical knocks half as far again.</summary>
    public const float LightKnock = 3f;
    public const float HeavyKnock = 10f;
    public const float SpellKnock = 5f;
    public const float KillKnock = 20f;
    public const float CriticalKnock = 1.5f;

    /// <summary>How long a knock takes to come back, in seconds.</summary>
    public const float KnockSeconds = 0.15f;

    /// <summary>The squash a heavy blow gives its target: how short it goes (0.85 of its height), and for how many frames.</summary>
    public const float HeavySquash = 0.15f;
    public const float LightSquash = 0.06f;
    public const int SquashFrames = 8;

    /// <summary>Seconds in a number of frames.</summary>
    public static float Seconds(int frames) => frames / FramesPerSecond;

    /// <summary>
    /// How many frames a blow freezes the attacker and the target: 3 plus 8 for a blow that takes
    /// all of the target's health, between 3 and 10; 3 more for a critical, to at most 10; a kill at
    /// least 10; never more than 14.
    /// </summary>
    /// <param name="damage">How much the blow did.</param>
    /// <param name="targetMost">The most health the target can have.</param>
    /// <param name="critical">Whether it was a critical.</param>
    /// <param name="kill">Whether it killed (or downed) the target.</param>
    public static int HitStopFrames(int damage, int targetMost, bool critical, bool kill)
    {
        float share = targetMost > 0 ? Math.Clamp(damage / (float)targetMost, 0f, 1f) : 0f;
        int frames = Math.Clamp((int)MathF.Round(3f + 8f * share), LeastHitStop, 10);
        if (critical) frames = Math.Min(frames + 3, 10);
        if (kill) frames = Math.Max(frames, 10) + (critical ? 2 : 0);
        return Math.Min(frames, MostHitStop);
    }

    /// <summary>How far a blow knocks its target, in world pixels.</summary>
    /// <param name="heavy">Whether it was a heavy skill rather than an auto attack.</param>
    /// <param name="critical">Whether it was a critical.</param>
    /// <param name="kill">Whether it killed.</param>
    public static float Knock(bool heavy, bool critical, bool kill)
    {
        float knock = kill ? KillKnock : heavy ? HeavyKnock : LightKnock;
        return critical && !kill ? knock * CriticalKnock : knock;
    }
}
