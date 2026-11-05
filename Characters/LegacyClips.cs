using System;
using System.Collections.Generic;

namespace LegendQuest.Characters;

/// <summary>
/// What a scene's anim cues come to on a Legacy sheet: the clip names the scripts use, by the
/// fighting pose that's nearest, or standing for the ones that get someone back up.
/// </summary>
/// <remarks>
/// A Legacy sheet has five poses past the walk (wind-up, strike, cast, hurt, downed), and the
/// cutscene clips in 03-art-overhaul.md §4.2 and §4.3 are dozens. Only the clips one of the five
/// actually shows are here: the combat clips themselves, a class's own wind-up and strike
/// (windup-two-hand, strike-dagger), and the cutscene clips that are one of them in all but name
/// (a pommel strike is a strike, a heal is a cast, a fall is downed). The rest (sit, kneel, bow,
/// flip-coin and the like) show nothing, which is what Legacy does with them. Slump isn't here,
/// though the rig plays it as a fall: Brannock slumped over his table would be lying on the floor
/// beside it. Idle, stand, rise and revive let go of whatever pose someone's in.
/// </remarks>
public static class LegacyClips
{
    /// <summary>How long someone strikes a pose before their next cue goes, in seconds: long enough to read it.</summary>
    public const float Seconds = 0.5f;

    /// <summary>The prefixes a class's own wind-up or strike has in front of its weapon class, as windup-two-hand.</summary>
    private const string WindUpPrefix = "windup-";
    private const string StrikePrefix = "strike-";

    private static readonly Dictionary<string, WalkerPose> Table = new()
    {
        ["windup"] = WalkerPose.WindUp,
        ["draw"] = WalkerPose.WindUp,
        ["strike"] = WalkerPose.Strike,
        ["pommel-strike"] = WalkerPose.Strike,
        ["loose"] = WalkerPose.Strike,
        ["blade-at-throat"] = WalkerPose.Strike,
        ["cast"] = WalkerPose.Cast,
        ["channel"] = WalkerPose.Cast,
        ["heal"] = WalkerPose.Cast,
        ["drain"] = WalkerPose.Cast,
        ["chant"] = WalkerPose.Cast,
        ["address-crowd"] = WalkerPose.Cast,
        ["hurt"] = WalkerPose.Hurt,
        ["stagger"] = WalkerPose.Hurt,
        ["burn"] = WalkerPose.Hurt,
        ["catch"] = WalkerPose.Hurt,
        ["downed"] = WalkerPose.Downed,
        ["die"] = WalkerPose.Downed,
        ["fall"] = WalkerPose.Downed,
        ["slip"] = WalkerPose.Downed,
        ["ash"] = WalkerPose.Downed,
        ["consumed"] = WalkerPose.Downed,
        ["idle"] = WalkerPose.None,
        ["stand"] = WalkerPose.None,
        ["rise"] = WalkerPose.None,
        ["revive"] = WalkerPose.None,
    };

    /// <summary>The pose a clip comes to, if it comes to one.</summary>
    /// <param name="clip">The clip's name, as the cue has it.</param>
    /// <param name="pose">The pose, or None for standing (a clip that gets someone up).</param>
    /// <returns>False for a clip Legacy shows nothing for.</returns>
    public static bool TryGet(string clip, out WalkerPose pose)
    {
        if (clip is null)
        {
            pose = WalkerPose.None;
            return false;
        }
        if (Table.TryGetValue(clip, out pose)) return true;

        pose = clip.StartsWith(WindUpPrefix, StringComparison.Ordinal) ? WalkerPose.WindUp : clip.StartsWith(StrikePrefix, StringComparison.Ordinal) ? WalkerPose.Strike : WalkerPose.None;
        return pose != WalkerPose.None;
    }
}
