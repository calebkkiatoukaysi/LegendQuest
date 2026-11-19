using Microsoft.Xna.Framework;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;

namespace LegendQuest.Screens;

/// <summary>
/// The fight's part of the proof run: handing the leader to the party's AI so every story fight can
/// be fought through, pointing at someone, showing the log, putting the panel away, and what's on the
/// screen for choosing a shot.
/// </summary>
/// <remarks>
/// Everything goes through the same code the player's does: pointing puts the proof run's pretend
/// mouse (ProofStep moves it every frame) over someone's figure, and ProofClick, in
/// ExploreScreen.Proof, clicks it, so a target is picked the way a player picks one. With autoplay on,
/// the leader is the brains' (Battle.LeaderAutoplay), and the panel at the end goes by itself after a
/// while if the run hasn't put it away.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>Whether a fight's on.</summary>
    internal bool ProofInFight => InCombat;

    /// <summary>Whether the panel at the end is up.</summary>
    internal bool ProofFightOver => InCombat && _result.Showing;

    /// <summary>Has the party's AI drive the leader as well as everyone else, so a fight plays itself through, at a speed against real time (1 for the run's shots).</summary>
    /// <param name="on">Whether to.</param>
    /// <param name="speed">How many times faster than real time the fight runs.</param>
    internal void ProofAutoplay(bool on = true, float speed = 1f)
    {
        _proofAutoplay = on;
        _combatSpeed = MathHelper.Max(speed, 1f);
        if (_battle is not null) _battle.LeaderAutoplay = on;
    }

    /// <summary>Puts the pretend mouse over someone in the fight: the middle of their figure.</summary>
    /// <param name="id">Their id in the fight: an origin id, a room npc's id, or a spawned foe's ("sea-wraith-1").</param>
    internal void ProofAimAtFighter(string id)
    {
        if (FighterWalker(id) is { } walker) ProofAimAt(walker.Position - new Vector2(0f, walker.Size.Y / 2f));
    }

    /// <summary>The nearest foe to the leader still standing, by id, or null.</summary>
    internal string ProofNearestFoe() => _battle?.Leader is Fighter leader ? NearestFoe(leader)?.Id : null;

    /// <summary>Whether the leader has someone targeted.</summary>
    internal bool ProofTargeted => _battle?.Leader?.Target is not null;

    /// <summary>Shows or hides the log, as L would.</summary>
    /// <param name="shown">Whether it shows.</param>
    internal void ProofShowLog(bool shown) => _log.Shown = shown;

    /// <summary>Puts the panel at the end away, as Enter would, once it's been up long enough.</summary>
    internal void ProofContinue()
    {
        if (_result.Ready) DismissResult();
    }

    /// <summary>How busy the fight looks this frame, for picking its shot: one for each figure mid-move, and one for each number in the air.</summary>
    private int ProofBusy()
    {
        int busy = 0;
        foreach (Fighter fighter in _battle.Fighters)
            if (fighter.Present && !fighter.IsOut && _fighters.TryGetValue(fighter.Id, out var walker) && walker.Move is { IsAction: true }) busy++;
        foreach (Fighter fighter in _battle.Fighters) busy += _popups.CountOver(fighter.Id);
        return busy;
    }
}
