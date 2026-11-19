using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Combat.Realtime;
using LegendQuest.Items;
using LegendQuest.StateManagement;
using LegendQuest.Tutorial;

namespace LegendQuest.Screens;

/// <summary>
/// The proof run's fights, and the screens over the room it looks in on: whenever a fight's on,
/// whatever part of the run it is, this plays it; and a visit can open the inventory, a shop, the
/// help or a tip for a shot.
/// </summary>
/// <remarks>
/// <para>
/// A fight takes as long as it takes, so no part of the run's schedule could know where one ends.
/// While one's on, it gets the run to itself: the party's AI drives the leader as well as the rest,
/// in real time on the run's lockstep clock, and the run takes a shot of the fight in the thick of it
/// (the first moment from a few seconds in with several people mid-move and numbers in the air, or
/// at the latest half a minute in) and of the panel at the end. For a few fights it also films a run
/// of frames from that moment, every second frame for three seconds, into rt/, to check that each
/// blow's number and flash come on the frame it lands.
/// </para>
/// <para>
/// The tutorial at sea starts the way a player would: the pretend mouse goes over the nearest wraith
/// and clicks it, the shot shows the target picked (its ring, its frame, the card), and then the AI
/// plays on. The fights are seeded under the proof run, so every run fights the same fights; one
/// that's lost goes to the game-over screen like any other, and the run (LegendQuestGame.Proof.cs)
/// loads the save from before it and fights it again.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far into a fight its shot can be taken from, and the latest it waits for a busy moment, in frames.</summary>
    private const int ProofShotFrom = 240;
    private const int ProofShotLatest = 1800;

    /// <summary>How busy a moment has to be for the fight's shot: figures mid-move and numbers in the air, counted together (ProofBusy).</summary>
    private const int ProofShotBusy = 5;

    /// <summary>How long the panel at the end has been ready (faded in) before its shot, and how long after the shot it's put away.</summary>
    private const int ProofFightEnding = 6;
    private const int ProofFightDismiss = 12;

    /// <summary>The tutorial's opening: how long a lesson's card is up before its shot, how long after that the mouse is over the wraith before the click (and the other lessons are done), how long a lesson waits to be put away before GOT IT is pressed for it, and when the log's shot is after the lessons.</summary>
    private const int ProofLessonShot = 30;
    private const int ProofAimFrames = 20;
    private const int ProofLessonGiveUp = 90;
    private const int ProofLogShot = 420;

    /// <summary>How long the lesson that's up has been up, in frames.</summary>
    private int _proofLessonFrames;

    /// <summary>The fights a run of frames is filmed of, and how long the run is and how often it saves a frame.</summary>
    private static readonly string[] ProofFilmed = { "a1-c1-01-gulls", "a1-c2-02-thornmother" };
    private const int ProofFilmFrames = 180;
    private const int ProofFilmEvery = 2;

    /// <summary>How long a screen opened over the room for a shot gets to come in.</summary>
    private const int ProofScreenSettles = 30;

    /// <summary>The fight the run's in, by encounter id; how long it's been on; how long its end panel has been up; the frame the tutorial's click was made on; and how far into the film it is (-1 before it starts).</summary>
    private string _proofFight;
    private int _proofFightClock;
    private int _proofEndingClock;
    private int _proofClickedAt;
    private int _proofFilm;

    /// <summary>Whether the fight's shot has been taken.</summary>
    private bool _proofShot;

    /// <summary>One frame of a fight under the proof run.</summary>
    /// <returns>The shot to take this frame, or null.</returns>
    private string FightStep()
    {
        if (_encounter.Id != _proofFight)
        {
            _proofFight = _encounter.Id;
            _proofFightClock = 0;
            _proofEndingClock = 0;
            _proofClickedAt = -1;
            _proofFilm = -1;
            _proofShot = false;
            ProofAutoplay(!_encounter.Tutorial, 1f);
        }
        _proofFightClock++;

        if (ProofFightOver)
        {
            ProofLetGo();
            if (!_proofShot)
            {
                _proofShot = true;
                ProofNote($"{_proofFight} was over before it got busy, so its shot is of its end.");
                return $"fight-{_proofFight}.png";
            }

            // The panel's own clock: its shot once it's faded in and ready, then it's put away.
            if (!_result.Ready) return null;
            _proofEndingClock++;
            string ending = FightWon ? "won" : "lost";
            if (_proofEndingClock == ProofFightEnding) return $"fight-{_proofFight}-{ending}.png";
            if (_proofEndingClock >= ProofFightEnding + ProofFightDismiss) ProofContinue();
            return null;
        }

        if (_encounter.Tutorial && TutorialOpening() is string opening) return opening;

        if (_proofFilm >= 0 && _proofFilm < ProofFilmFrames)
        {
            int frame = _proofFilm++;
            return frame % ProofFilmEvery == 0 ? $"rt/{_proofFight}-{frame / ProofFilmEvery:000}.png" : null;
        }

        if (!_proofShot && _proofFightClock >= ProofShotFrom && (ProofBusy() >= ProofShotBusy || _proofFightClock >= ProofShotLatest))
        {
            _proofShot = true;
            if (Array.IndexOf(ProofFilmed, _proofFight) >= 0) _proofFilm = 0;
            return $"fight-{_proofFight}.png";
        }
        return null;
    }

    /// <summary>
    /// The tutorial's opening, the way a player meets it: its first three lessons, each card held up
    /// for its shot and then done as it says (the mouse over the nearest wraith and a click, GOT IT,
    /// 2 for the first skill), the shot of the wraith targeted, then the AI playing on; and the log's
    /// shot later.
    /// </summary>
    /// <returns>A shot to take this frame, or null.</returns>
    private string TutorialOpening()
    {
        if (_proofClickedAt < 0)
        {
            if (_tutorials.Current is { HoldsFight: true } lesson && _tutorialPanel.Showing) return ProofLesson(lesson.Id);
            _proofLessonFrames = 0;
            if (!_tutorials.Seen(TutorialCatalog.CombatAbility) || _tutorials.Current is not null) return null;

            // The three lessons are done: the AI plays on from here.
            _proofClickedAt = _proofFightClock;
            ProofLetGo();
            ProofAutoplay(true, 1f);
            return null;
        }

        int since = _proofFightClock - _proofClickedAt;
        if (since == ProofLogShot) ProofShowLog(true);
        if (since == ProofLogShot + 2) return "combat-log.png";
        if (since == ProofLogShot + 3) ProofShowLog(false);
        return null;
    }

    /// <summary>One frame of a lesson's card being up in the tutorial fight under the proof run: its shot once it's in, then what it says done, the way a player would; GOT IT if that doesn't put it away.</summary>
    /// <param name="id">The lesson, by its tip's id.</param>
    /// <returns>A shot to take this frame, or null.</returns>
    private string ProofLesson(string id)
    {
        int frame = ++_proofLessonFrames;
        if (frame == 1 && id != TutorialCatalog.CombatTarget) ProofLetGo();
        if (frame == ProofLessonShot) return $"lesson-{id[(id.IndexOf('-') + 1)..]}.png";
        if (id == TutorialCatalog.CombatAttack && frame == ProofLessonShot + 1)
        {
            if (!ProofTargeted) ProofNote($"{_proofFight}: the click on the nearest wraith didn't target it.");
            return "combat-target.png";
        }

        if (id == TutorialCatalog.CombatTarget && frame >= ProofLessonShot && ProofNearestFoe() is string foe)
        {
            ProofAimAtFighter(foe);
            if (frame == ProofLessonShot + ProofAimFrames) ProofClick();
        }
        if (id == TutorialCatalog.CombatAttack && frame == ProofLessonShot + ProofAimFrames) _tutorials.Dismiss();
        if (id == TutorialCatalog.CombatAbility && frame == ProofLessonShot + ProofAimFrames) UseSlot(_battle.Leader, 1);
        if (frame == ProofLessonShot + ProofLessonGiveUp)
        {
            ProofNote($"{_proofFight}: lesson {id} was still up after it was done; put away.");
            _tutorials.Dismiss();
        }
        return null;
    }

    /// <summary>Says something about the run's fight shots on the error stream, for a shot that had to be taken late or not at all, rather than waiting for ever.</summary>
    private static void ProofNote(string text) => Console.Error.WriteLine("proof: " + text);

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
