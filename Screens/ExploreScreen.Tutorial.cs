using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Components;
using LegendQuest.StateManagement;
using LegendQuest.Tutorial;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The zone screen's tutorial: noticing the moments a tip's for, the card it comes up on, and
/// putting it away.
/// </summary>
/// <remarks>
/// Each tip comes at the moment it's needed, once a game: the first time you can walk, someone's in
/// reach, a mark's up, what you're to do next changes, and so on. Most of those moments are things
/// the screen already knows every frame (who's nearest, who's marked), so they're noticed here, and
/// the few that are something you do (talking, using something, opening the zone map) tell it from where
/// they happen with TutorialDone, which also puts away a tip that's taught what it set out to.
/// Nothing's noticed while a conversation's up or the ship's coming in, and the card waits too. The
/// fighting partial shows its tips through ShowTutorial the same way. The proof run keeps tips quiet
/// (Tutorials.Quiet), and ProofTip puts one up for a shot.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long you have to have walked before the tip about walking has done its job and goes, in seconds.</summary>
    private const float MoveTaughtSeconds = 4f;

    /// <summary>How long you walk in a zone, in seconds, before the tip about switching who you control comes, so it isn't one more thing the moment you land.</summary>
    private const float PartyTipSeconds = 20f;

    /// <summary>How near a door you have to come along the floor for the tip about going in, in art pixels.</summary>
    private const float DoorTipReach = 32f;

    /// <summary>How many tips you've seen before the one that says where they all are comes.</summary>
    private const int HelpAfterTips = 5;

    /// <summary>What the quest board's drawn as, its look in the zone file. It's the only notice board there is.</summary>
    private const string QuestBoardLook = "notice-board";

    private readonly TutorialPanel _tutorialPanel = new();

    private Tutorials _tutorials;

    /// <summary>What you were to do next and who you were controlling when last looked, so a change is noticed, and how long you've walked in this zone, in seconds.</summary>
    private string _tutorialObjective;
    private int _tutorialLeader;
    private float _tutorialWalked;

    /// <summary>Whether tips are on, so the screen knows not to say the same thing a tip will.</summary>
    private bool TutorialsOn => _tutorials.On;

    /// <summary>Gets the tips ready for this game, and the card they come up on.</summary>
    private void LoadTutorial()
    {
        _tutorials ??= new Tutorials(_game, _audio.Settings);
        _tutorialPanel.LoadContent(_content, ScreenManager.BlankTexture);
        _tutorialObjective = _game.Objective;
        _tutorialLeader = _game.Leader;
    }

    /// <summary>Puts a tip in line to come up, unless it's been seen or tips are off. Safe to call every frame the moment holds.</summary>
    /// <param name="tipId">The tip, by its id in TutorialCatalog.</param>
    /// <returns>True if it was put in line.</returns>
    private bool ShowTutorial(string tipId) => _tutorials.Show(tipId);

    /// <summary>Says you've done what a tip teaches, so it goes if it's up and never comes if it isn't.</summary>
    /// <param name="tipId">The tip, by its id in TutorialCatalog.</param>
    private void TutorialDone(string tipId) => _tutorials.Complete(tipId);

    /// <summary>Says you've used something, which is the tip for that kind of thing done.</summary>
    /// <param name="thing">What was used.</param>
    private void TutorialUsed(MapInteractable thing) => TutorialDone(TutorialFor(thing));

    /// <summary>Enter, X or a click on the card puts it away, and a tip that holds everything stops you where you are until it's read.</summary>
    /// <param name="input">The input this frame.</param>
    /// <returns>True if the card took the input, so nothing else should look at it.</returns>
    private bool HandleTutorialInput(InputState input)
    {
        if (!_tutorialPanel.HandleInput(input, _tutorials)) return false;

        if (_tutorials.Current is { Blocking: true }) Halt();
        return true;
    }

    /// <summary>Whether the mouse is on the card, which is in front of everything, so the mouse there isn't pointing at the room.</summary>
    private bool PointingAtTutorial() => _tutorialPanel.Contains(_mouse);

    /// <summary>Notices the moments while you're the one playing, then runs the card: in when there's a tip and room, out when it's done or something's over the screen.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    private void UpdateTutorial(GameTime gameTime)
    {
        bool playing = IsActive && !_talking && _arriving == Arriving.None;
        if (playing && !InCombat) NoticeMoments((float)gameTime.ElapsedGameTime.TotalSeconds);
        else _tutorialLeader = _game.Leader;

        _tutorialPanel.Update(gameTime, _tutorials, playing);
    }

    /// <summary>Draws the card over everything in the room: the card and its keys point sampled, then its words.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    private void DrawTutorial(SpriteBatch spriteBatch, float alpha)
    {
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        _tutorialPanel.DrawCard(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _tutorialPanel.DrawWords(spriteBatch, alpha);
        spriteBatch.End();
    }

    /// <summary>For the proof run, since tips are quiet in it: puts a tip up next for a shot of its card.</summary>
    /// <param name="tipId">The tip, by its id in TutorialCatalog.</param>
    internal void ProofTip(string tipId) => _tutorials.ProofShow(tipId);

    /// <summary>Puts in line the tip for every moment that holds right now, in the order they'd best come, and sees off the ones whose lesson you've shown you've learned.</summary>
    /// <remarks>Called every frame you're playing. Show ignores a tip that's seen, up or waiting, so asking again costs nothing.</remarks>
    /// <param name="seconds">How long the frame was.</param>
    private void NoticeMoments(float seconds)
    {
        // Walking, the first moment you can, and done once you've walked a little.
        ShowTutorial(TutorialCatalog.Move);
        if (Leader.IsWalking) _tutorialWalked += seconds;
        if (_tutorialWalked >= MoveTaughtSeconds) TutorialDone(TutorialCatalog.Move);

        // The marks: red over something about to happen, gold over one of the party, gold over anyone else.
        foreach ((WalkerSprite walker, _, MarkerKind kind) in _marked)
        {
            if (kind == MarkerKind.Event) ShowTutorial(TutorialCatalog.Event);
            else if (walker is not null && _walkers.Contains(walker)) ShowTutorial(TutorialCatalog.Companion);
            else ShowTutorial(TutorialCatalog.Work);
        }

        // Whoever and whatever's in reach, and the doors.
        if (_nearNpc is not null) ShowTutorial(TutorialCatalog.Talk);
        if (_nearMember is not null) ShowTutorial(TutorialCatalog.Companion);
        if (_near is MapInteractable thing) ShowTutorial(TutorialFor(thing));
        if (NearDoor()) ShowTutorial(TutorialCatalog.Enter);

        // What you're to do next, the first time it changes.
        if (_game.Objective != _tutorialObjective)
        {
            _tutorialObjective = _game.Objective;
            if (!string.IsNullOrEmpty(_game.Objective)) ShowTutorial(TutorialCatalog.Objective);
        }

        // The zone map, once you're somewhere that's on the world map.
        if (_zone.World is not null) ShowTutorial(TutorialCatalog.Map);

        // Switching who you control: done the first time you do, and taught once you've walked a while with someone behind you.
        if (_game.Leader != _tutorialLeader)
        {
            _tutorialLeader = _game.Leader;
            TutorialDone(TutorialCatalog.Party);
        }
        if (_walkers.Count > 1 && _tutorials.Seen(TutorialCatalog.Move) && _tutorialWalked >= PartyTipSeconds) ShowTutorial(TutorialCatalog.Party);

        if (_campers.Count > 0) ShowTutorial(TutorialCatalog.Camp);
        if (_tutorials.SeenCount >= HelpAfterTips) ShowTutorial(TutorialCatalog.Help);
    }

    /// <summary>The tip for using something: waking a waypoint stone, reading the quest board, or opening, pulling or reading anything else.</summary>
    private static string TutorialFor(MapInteractable thing)
    {
        if (thing.Kind == InteractableKind.Waypoint) return TutorialCatalog.Waypoint;
        return thing.Look.Id == QuestBoardLook ? TutorialCatalog.Board : TutorialCatalog.Use;
    }

    /// <summary>Whether your feet are near a door that's there and opens: within DoorTipReach of its foot along the floor, and near the back of it where doors stand.</summary>
    private bool NearDoor()
    {
        Vector2 feet = Leader.Position;
        foreach (RoomDoor door in Doors())
        {
            if (door.LockedWhy is not null) continue;

            // How far outside the doorway the feet are across; nothing if they're under it.
            float across = MathF.Max(MathF.Max(door.Area.Left - feet.X, feet.X - door.Area.Right), 0f);
            if (across <= DoorTipReach && feet.Y - door.Foot.Y <= DoorTipReach) return true;
        }
        return false;
    }
}
