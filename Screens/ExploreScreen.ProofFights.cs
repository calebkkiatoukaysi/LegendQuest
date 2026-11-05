using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Items;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// The proof run's fights, and the screens over the room it looks in on: whenever a fight's on,
/// whatever part of the run it is, this plays it; and a visit can open the inventory, a shop, the
/// help or a tip for a shot.
/// </summary>
/// <remarks>
/// A fight takes as long as its dice say, so no part of the run's schedule could know where one
/// ends. While one's on, it gets the run to itself: EnemyMind plays the party's turns as well as
/// the foes', faster than real time, and the run takes a shot of the opening (the turn banner and
/// the initiative bar) and of the end. The tutorial at sea is the exception: its first turn is
/// played by hand, the movement preview past the budget, the target's tooltip and the attack, the
/// way a player would meet them. The zone's dice are seeded under the proof run, so every run
/// fights the same fights; one that's lost goes to the game-over screen like any other, and the
/// run (LegendQuestGame.Proof.cs) loads the save from before it and fights it again.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How fast fights play under the proof run, as a multiple of real time.</summary>
    private const float ProofFightSpeed = 3f;

    /// <summary>How long after a fight starts its opening shot's taken: once the banner and the initiative bar are in.</summary>
    private const int ProofFightOpening = 70;

    /// <summary>How long the panel at the end is up before its shot, so it's faded in.</summary>
    private const int ProofFightEnding = 30;

    /// <summary>How far ahead of you (towards the foes, on the right) the tutorial's movement preview reaches, in tiles: well past a turn's walk, so the part beyond it shows. It stops at the right end of the fight's ground.</summary>
    private const int ProofPreviewReach = 9;

    /// <summary>How long each of the tutorial's hand-played moments is held before its shot.</summary>
    private const int ProofMomentFrames = 24;

    /// <summary>The sea-wraith the tutorial's first attack goes for: the one that comes up over the rail nearest the bow.</summary>
    private const string ProofTutorialTarget = "wraith-1";

    /// <summary>How long a screen opened over the room for a shot gets to come in.</summary>
    private const int ProofScreenSettles = 30;

    /// <summary>The fight the run's in, by encounter id; how long it's been on; how long its end panel has been up; and how far the tutorial's hand-played turn has got.</summary>
    private string _proofFight;
    private int _proofFightClock;
    private int _proofEndingClock;
    private int _proofTutorialStep;
    private int _proofTutorialMark;

    /// <summary>One frame of a fight under the proof run.</summary>
    /// <returns>The shot to take this frame, or null.</returns>
    private string FightStep()
    {
        if (_encounter.Id != _proofFight)
        {
            _proofFight = _encounter.Id;
            _proofFightClock = 0;
            _proofEndingClock = 0;
            _proofTutorialStep = 0;
            ProofAutoplay(!_encounter.Tutorial, ProofFightSpeed);
        }
        _proofFightClock++;

        if (ProofFightOver)
        {
            string ending = _combat.Outcome == CombatOutcome.Defeat ? "lost" : "won";
            if (++_proofEndingClock == ProofFightEnding) return $"fight-{_proofFight}-{ending}.png";
            if (_proofEndingClock > ProofFightEnding) ProofContinue();
            return null;
        }

        if (_proofFightClock == ProofFightOpening) return $"fight-{_proofFight}.png";
        return _encounter.Tutorial && _proofFightClock > ProofFightOpening ? TutorialTurnStep() : null;
    }

    /// <summary>
    /// The tutorial's first turn, by hand: a tile far off pointed at (the path, and the part of it
    /// past a turn's walk in the other colour), then Attack picked and the nearest wraith pointed at
    /// (its hit chance and damage), then clicked, with the log open after. Then EnemyMind plays on.
    /// </summary>
    private string TutorialTurnStep()
    {
        int since = _proofFightClock - _proofTutorialMark;
        switch (_proofTutorialStep)
        {
            case 0 when ProofYourTurn:
                Point here = _ground.CellOf(Leader.Position);
                ProofAimAtCell(new Point(Math.Min(here.X + ProofPreviewReach, _ground.Width - 1), here.Y));
                return Moment();

            case 1 when since == ProofMomentFrames:
                _proofTutorialStep++;
                return "combat-move.png";

            case 2:
                ProofPressAbility(AbilityId.Attack);
                ProofAimAtFighter(ProofTutorialTarget);
                return Moment();

            case 3 when since == ProofMomentFrames:
                _proofTutorialStep++;
                return "combat-target.png";

            case 4:
                ProofClick();
                return Moment();

            case 5 when since == ProofMomentFrames * 2:
                ProofShowLog(true);
                _proofTutorialStep++;
                return "combat-attack.png";

            case 6 when since == ProofMomentFrames * 3:
                _proofTutorialStep++;
                return "combat-log.png";

            case 7:
                ProofShowLog(false);
                ProofLetGo();
                ProofAutoplay(true, ProofFightSpeed);
                _proofTutorialStep++;
                return null;

            default:
                return null;
        }
    }

    /// <summary>Starts the clock on one of the tutorial's moments, and moves on to waiting for it.</summary>
    private string Moment()
    {
        _proofTutorialMark = _proofFightClock;
        _proofTutorialStep++;
        return null;
    }

    /// <summary>Opens a screen over the room for a while, takes its shot, and puts it away.</summary>
    /// <param name="open">Makes the screen.</param>
    /// <param name="shot">The shot.</param>
    /// <param name="also">Anything to do to it before the shot, or null.</param>
    private IEnumerable<ProofBeat> Over(Func<GameScreen> open, string shot, Action<GameScreen> also = null)
    {
        GameScreen screen = null;
        yield return Do(() =>
        {
            screen = open();
            ScreenManager.AddScreen(screen);
        });
        yield return Wait(ProofScreenSettles);
        if (also is not null)
        {
            yield return Do(() => also(screen));
            yield return Wait(ProofScreenSettles);
        }
        yield return Shot(shot);
        yield return Do(() => screen.ExitScreen());
        yield return Wait(ProofScreenSettles);
    }

    /// <summary>The screens a player opens over the room, each looked in on once in the Flagon: the bag, the character sheet, the forge's shop, the help's first two pages, and a tip.</summary>
    private IEnumerable<ProofBeat> LookIns()
    {
        foreach (ProofBeat beat in Over(() => new InventoryScreen(_game, InventoryPage.Bag), "inventory.png")) yield return beat;
        foreach (ProofBeat beat in Over(() => new InventoryScreen(_game, InventoryPage.Character), "character-sheet.png")) yield return beat;
        foreach (ProofBeat beat in Over(() => new ShopScreen(ShopCatalog.Get(ProofShop), _game), "shop.png")) yield return beat;
        foreach (ProofBeat beat in Over(() => new HelpScreen(), "help.png")) yield return beat;
        foreach (ProofBeat beat in Over(() => new HelpScreen(), "help-combat.png", screen => ((HelpScreen)screen).ProofPage(ProofHelpCombatPage))) yield return beat;
        yield return Do(() => ProofTip(ProofTipShown));
        yield return Wait(ProofScreenSettles);
        yield return Shot("tutorial-tip.png");
        yield return Do(() => _tutorials.Dismiss());
    }

    /// <summary>The shop the run looks in on: the forge, for its weapons and armour.</summary>
    private const string ProofShop = "forge";

    /// <summary>The help's page about fighting, counted from its first.</summary>
    private const int ProofHelpCombatPage = 4;

    /// <summary>The tip the run puts up for its shot: the one about the party's conversations.</summary>
    private const string ProofTipShown = "explore-companion";
}
