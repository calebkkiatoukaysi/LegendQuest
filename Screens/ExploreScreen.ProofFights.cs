using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Combat;
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
/// the foes', faster than real time, and the run takes a shot of the opening (everyone in their
/// slots, the supports behind them, the turn order) and of the end. The tutorial at sea is the
/// exception: its first turn is played by hand, the way a player would meet it: a back-row slot
/// pointed at (its chevron and the ring under the foe who'd swing), Auto picked with the front row
/// it reaches lit, a wraith aimed at (its odds), the attack, the log, and the crew's boat-hook at the
/// end of the round. The zone's dice are seeded under the proof run, so every run
/// fights the same fights; one that's lost goes to the game-over screen like any other, and the
/// run (LegendQuestGame.Proof.cs) loads the save from before it and fights it again.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How fast fights play under the proof run, as a multiple of real time.</summary>
    private const float ProofFightSpeed = 3f;

    /// <summary>How many frames in a row a fight has to have been settled (everyone in their slot, the dice rolled, the first round begun) before its opening shot; and the latest the shot waits for that, after which it's taken anyway and the run says so.</summary>
    private const int ProofFightSettled = 12;
    private const int ProofFightOpeningLatest = 600;

    /// <summary>How long the panel at the end has been ready (faded in) before its shot, and how long after the shot it's put away.</summary>
    private const int ProofFightEnding = 6;
    private const int ProofFightDismiss = 12;

    /// <summary>How long each of the tutorial's hand-played moments is held before its shot.</summary>
    private const int ProofMomentFrames = 24;

    /// <summary>How long into the support's strike at the end of the tutorial's first round its shot is taken, so the blow's landing. The fight plays at its own speed until then.</summary>
    private const int ProofSupportFrames = 16;

    /// <summary>The longest the tutorial waits for the end of its first round before giving up on that shot.</summary>
    private const int ProofSupportLatest = 1200;

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

    /// <summary>Whether the fight's opening shot's been taken, how many frames in a row it's been settled for, and whether a support's strike has been shown since the tutorial started waiting for one.</summary>
    private bool _proofOpened;
    private int _proofSettled;
    private bool _proofSupportShown;

    /// <summary>The last of the tutorial's hand-played steps, which is done.</summary>
    private const int ProofTutorialDone = 12;

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
            _proofOpened = false;
            _proofSettled = 0;
            _proofSupportShown = false;
            ProofAutoplay(!_encounter.Tutorial, ProofFightSpeed);
        }
        _proofFightClock++;

        if (ProofFightOver)
        {
            if (!_proofOpened)
            {
                _proofOpened = true;
                ProofNote($"{_proofFight} was over before it settled, so its opening shot is of its end.");
                return $"fight-{_proofFight}.png";
            }
            if (_encounter.Tutorial && _proofTutorialStep < ProofTutorialDone)
            {
                ProofNote($"{_proofFight} ended at the tutorial's step {_proofTutorialStep}, so the shots after it weren't taken.");
                _proofTutorialStep = ProofTutorialDone;
            }

            // The panel's own clock: its shot once it's faded in and ready, then it's put away.
            if (!_result.Ready) return null;
            _proofEndingClock++;
            string ending = _combat.Outcome == CombatOutcome.Defeat ? "lost" : "won";
            if (_proofEndingClock == ProofFightEnding) return $"fight-{_proofFight}-{ending}.png";
            if (_proofEndingClock >= ProofFightEnding + ProofFightDismiss) ProofContinue();
            return null;
        }

        if (!_proofOpened)
        {
            _proofSettled = FightSettled ? _proofSettled + 1 : 0;
            if (_proofSettled < ProofFightSettled && _proofFightClock < ProofFightOpeningLatest) return null;
            if (_proofSettled < ProofFightSettled) ProofNote($"{_proofFight} hadn't settled after {ProofFightOpeningLatest} frames; its opening shot is taken anyway.");
            _proofOpened = true;
            return $"fight-{_proofFight}.png";
        }
        return _encounter.Tutorial ? TutorialTurnStep() : null;
    }

    /// <summary>Whether a fight's settled for its opening shot: everyone's walked to their slot, nobody's still fading in, and the first round has begun, so the turn order's filled in.</summary>
    private bool FightSettled => _holdLeft <= 0f && _glides.Count == 0 && _fades.Count == 0 && _shown.Round > 0;

    /// <summary>Says something about the run's fight shots on the error stream, for a shot that had to be taken late or not at all, rather than waiting for ever.</summary>
    private static void ProofNote(string text) => Console.Error.WriteLine("proof: " + text);

    /// <summary>
    /// The tutorial's first turn, by hand (11-combat's proof, M4 plan §9): a back-row slot pointed
    /// at, its chevron lit and the ember ring under any foe who'd swing for it; Auto picked with the
    /// mouse off everyone, so the front row it reaches is lit; the nearest wraith pointed at (its
    /// odds); the click; the log; and then, with EnemyMind playing on, the crew's boat-hook at the end
    /// of the first round.
    /// </summary>
    private string TutorialTurnStep()
    {
        int since = _proofFightClock - _proofTutorialMark;
        switch (_proofTutorialStep)
        {
            case 0 when ProofYourTurn:
                ProofAimAtSlot(ProofShiftSlot());
                return Moment();

            case 1 when since == ProofMomentFrames:
                _proofTutorialStep++;
                return "combat-shift.png";

            case 2:
                ProofPressAbility(AutoOf(_combat.Current).Id);
                ProofAimAt(new Vector2(_stage.Centre.X, _room.BackEdgeAt(_stage.Centre.X) + 1f));
                return Moment();

            case 3 when since == ProofMomentFrames:
                _proofTutorialStep++;
                return "combat-reach.png";

            case 4:
                ProofAimAtFighter(ProofTutorialTarget);
                return Moment();

            case 5 when since == ProofMomentFrames:
                _proofTutorialStep++;
                return "combat-target.png";

            case 6:
                ProofClick();
                return Moment();

            case 7 when since == ProofMomentFrames * 2:
                ProofShowLog(true);
                _proofTutorialStep++;
                return "combat-attack.png";

            case 8 when since == ProofMomentFrames * 3:
                _proofTutorialStep++;
                return "combat-log.png";

            case 9:
                ProofShowLog(false);
                ProofLetGo();
                ProofAutoplay(true, 1f);
                _proofSupportShown = false;
                return Moment();

            case 10 when _proofSupportShown:
                return Moment();

            case 10 when since >= ProofSupportLatest:
                ProofNote($"{_proofFight}: no support acted within {ProofSupportLatest} frames, so combat-support.png wasn't taken.");
                ProofAutoplay(true, ProofFightSpeed);
                _proofTutorialStep = ProofTutorialDone;
                return null;

            case 11 when since == ProofSupportFrames:
                ProofAutoplay(true, ProofFightSpeed);
                _proofTutorialStep = ProofTutorialDone;
                return "combat-support.png";

            default:
                return null;
        }
    }

    /// <summary>The slot the tutorial's Shift shot points at: a back-row slot you can Shift to, one a foe would swing at you for if there is one, or the back row's middle.</summary>
    private Slot ProofShiftSlot()
    {
        Combatant me = _combat.Current;
        Slot? fallback = null;
        foreach (Slot slot in _combat.Shifts(me))
        {
            if (slot.Row != Row.Back) continue;
            if (_combat.Opportunist(me, slot) is not null) return slot;
            fallback ??= slot;
        }
        return fallback ?? new Slot(Side.Party, Row.Back, 1);
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
