namespace LegendQuest.Combat.Realtime;

/// <summary>Handing the leader to the party's AI: for the proof run, which plays every story fight through on its own.</summary>
/// <remarks>
/// With it on, the brains think for the leader as they do for everyone else on the party's side (Think
/// skips only the fighter the player drives), and the screen stops sending the leader's commands. It
/// changes nothing else: the same seed and the same commands still play the same fight.
/// </remarks>
public sealed partial class Battle
{
    /// <summary>Whether the party's AI drives the leader too, as it does in the simulator.</summary>
    public bool LeaderAutoplay { get; set; }

    /// <summary>Whether a fighter is the player's to drive now: the leader, while autoplay is off.</summary>
    /// <param name="f">The fighter.</param>
    private bool DrivenByPlayer(Fighter f) => f == Leader && !LeaderAutoplay;
}
