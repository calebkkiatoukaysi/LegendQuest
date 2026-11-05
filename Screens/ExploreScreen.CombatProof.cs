using Microsoft.Xna.Framework;
using LegendQuest.Combat;

namespace LegendQuest.Screens;

/// <summary>
/// The fight's part of the proof run: starting a fight, having EnemyMind play the party so every
/// story fight can be fought through, pointing at someone or a tile, picking an ability, and the
/// moments a shot's worth taking.
/// </summary>
/// <remarks>
/// Everything goes through the same code the player's does: aiming puts the proof run's pretend mouse
/// (ProofStep moves it every frame) over someone's figure or a tile, so the plan, the rings, the path
/// and the card all come up as they would; picking an ability is pressing its key; ProofClick, in
/// ExploreScreen.Proof, clicks. With autoplay on, the panel at the end goes by itself after a moment.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>Whether a fight's on.</summary>
    internal bool ProofInFight => InCombat;

    /// <summary>Whether the fight's waiting on your turn: everything's shown and it's one of the party's go.</summary>
    internal bool ProofYourTurn => AwaitingPlayer;

    /// <summary>Whether the panel at the end is up.</summary>
    internal bool ProofFightOver => InCombat && _result.Showing;

    /// <summary>Has EnemyMind play the party's turns as well as the foes', so a fight plays itself through; and runs the fight faster than real time while it does.</summary>
    /// <param name="on">Whether to.</param>
    /// <param name="speed">How many times faster than real time the fight runs.</param>
    internal void ProofAutoplay(bool on = true, float speed = 1f)
    {
        _proofAutoplay = on;
        _combatSpeed = MathHelper.Max(speed, 1f);
    }

    /// <summary>Puts the pretend mouse over someone in the fight: the middle of their figure.</summary>
    /// <param name="id">Their id in the fight: an origin id, a room npc's id, or a spawned foe's ("sea-wraith-1").</param>
    internal void ProofAimAtFighter(string id)
    {
        if (FighterWalker(id) is { } walker) ProofAimAt(walker.Position - new Vector2(0f, walker.Shape.Figure.Y / 2f));
    }

    /// <summary>Puts the pretend mouse over a tile.</summary>
    /// <param name="cell">The tile.</param>
    internal void ProofAimAtCell(Point cell) => ProofAimAt(FeetOn(cell));

    /// <summary>Presses the hotbar key for an ability, if whoever's turn it is has it: one used on yourself happens at once, anything else waits for a target.</summary>
    /// <param name="ability">The ability.</param>
    internal void ProofPressAbility(AbilityId ability)
    {
        if (!AwaitingPlayer) return;
        BuildHotbar();
        int slot = _hotbarEntries.FindIndex(entry => entry.Ability.Id == ability);
        if (slot >= 0) PressSlot(slot);
    }

    /// <summary>Shows or hides the log, as L would.</summary>
    /// <param name="shown">Whether it shows.</param>
    internal void ProofShowLog(bool shown) => _log.Shown = shown;

    /// <summary>Puts the panel at the end away, as Enter would, once it's been up long enough.</summary>
    internal void ProofContinue()
    {
        if (_result.Ready) DismissResult();
    }
}
