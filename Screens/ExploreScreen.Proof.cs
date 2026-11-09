using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Dialogue;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The zone screen's part of the proof run: the voyage, the arrival at the harbour and the Red Gulls,
/// Questeria and the innkeeper's work, the road, fast travel both ways, the barrow, then out through
/// the pause menu. Act 1's later chapters are ExploreScreen.ProofChapters, and the beats and their
/// helpers ExploreScreen.ProofBeats.
/// </summary>
/// <remarks>
/// Every visit to a zone is its own stage, and each is a queue of beats. Movement goes in through the
/// same code the keyboard uses (ReadMovement), using things and talking through the same code the E
/// key does, Alt through the same flag it sets, and the mouse is a pretend one in InputState that the
/// screen reads like the real one, so the shots actually test the floor, the paths, the triggers, the
/// doors, the ways out, the room changes and the rules. I only teleport between scenes to keep the run
/// short, like the lobby's schedule in The Black Box. The ship coming in keeps its timed shots, on
/// the arrival's own clock.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How many frames a zone gets to fade in before the first beat.</summary>
    private const int Settled = 50;

    /// <summary>How often a conversation gets a press while the proof run's reading it, and how long a line's held for its shot.</summary>
    private const int ProofReadEvery = 6;
    private const int ProofHoldFrames = 20;

    /// <summary>The choices still to make at this visit's questions, the clock the presses go by, the shots taken, the shot a line's being held for, and how long it's been held.</summary>
    private readonly Queue<int> _proofChoices = new();
    private readonly HashSet<string> _proofShots = new();
    private int _proofTalkClock;
    private string _proofHolding;
    private int _proofHoldClock;

    /// <summary>A direction to walk in instead of reading the keyboard, while the proof run is driving.</summary>
    private Vector2? _proofMove;

    /// <summary>A point in the room the pretend mouse stays over, whatever the camera does, and whether its button is down this frame.</summary>
    private Vector2? _proofAim;
    private bool _proofPress;

    /// <summary>Sets up whatever this frame needs, and returns a file name if this frame should be saved.</summary>
    /// <param name="frame">Which drawn frame this is, counted from the zone's first.</param>
    /// <param name="stage">Which part of the run it is, which says which visit to this zone it is.</param>
    /// <returns>The file the frame should be written to, or null.</returns>
    internal string ProofStep(int frame, ProofStage stage)
    {
        // A click is the button down for one frame and up the next.
        if (_proofAim is Vector2 aim)
        {
            Vector2 screen = _camera.ToScreen(aim);
            ScreenManager.Input.ProofMouse(new Point((int)screen.X, (int)screen.Y), _proofPress);
        }
        _proofPress = false;

        // A fight has the run to itself while it's on, whatever part of the run it is.
        if (ProofInFight) return FightStep();

        return stage switch
        {
            ProofStage.Voyage => Beats(frame, VoyageBeats),
            ProofStage.Docks => Beats(frame, DocksBeats),
            ProofStage.Questeria => Beats(frame, QuesteriaBeats),
            ProofStage.Flagon => Beats(frame, FlagonBeats),
            ProofStage.QuesteriaOut => Beats(frame, () => Seq(Shot("questeria-outside.png"), Out("road"))),
            ProofStage.Road => Beats(frame, RoadBeats),
            ProofStage.QuesteriaAgain => Beats(frame, () => Seq(Shot("questeria-arrived.png"), TravelFrom("crystal", "road"))),
            ProofStage.Barrow => Beats(frame, BarrowBeats),
            _ => ChapterStep(frame, stage),
        };
    }

    /// <summary>The deck at night, with the red "!" at the bow: a walk up to it along the deck, then the Prologue's voyage read through (answering MAYBE THIS ONE'S DIFFERENT, then I'LL BE RIGHT THERE), the sea-wraiths, the fight, and Questeria at dawn, until the voyage ends at the harbour.</summary>
    private IEnumerable<ProofBeat> VoyageBeats() => Seq(
        Shot("voyage.png"),
        // The party struck in each fighting pose, held, for the pieces' own motion on the deck.
        Do(() => ProofPose(WalkerPose.WindUp)), Wait(ProofPoseFrames), Shot("rig-windup.png"),
        Do(() => ProofPose(WalkerPose.Strike)), Wait(ProofPoseFrames), Shot("rig-strike.png"),
        Do(() => ProofPose(WalkerPose.Cast)), Wait(ProofPoseFrames), Shot("rig-cast.png"),
        Do(() => ProofPose(WalkerPose.Hurt)), Wait(ProofPoseFrames), Shot("rig-hurt.png"),
        Do(() => ProofPose(WalkerPose.None)), Wait(ProofPoseFrames),
        Over(() => new InventoryScreen(_game, InventoryPage.Bag), "voyage-inventory.png"),
        Answer(1, 0),
        Walk(-ProofToTheBow),
        ReadUntil(() => false, (Asked, "voyage-choice.png"), ("lookout", "voyage-wraiths.png"), ("odelle", "voyage-dawn.png")));

    /// <summary>How long a held pose settles before its shot, in frames.</summary>
    private const int ProofPoseFrames = 24;

    /// <summary>Strikes a pose with everyone in the party and holds it; None lets it go.</summary>
    private void ProofPose(WalkerPose pose)
    {
        foreach (WalkerSprite walker in _walkers)
        {
            if (pose == WalkerPose.None) walker.Release();
            else walker.Play(pose);
        }
    }

    /// <summary>How far along the deck from the start the bow's "!" is, at most, in art pixels: the walk stops when the voyage starts.</summary>
    private const float ProofToTheBow = 160f;

    /// <summary>
    /// The harbour: the ship sailing in with the two of you on deck, the sail coming in, tied up, the
    /// plank going out, the walk ashore and Act 1's card, each on the arrival's own clock; then the red
    /// "!" over the Red Gulls and a walk east into them (talked down, with the dice, and Grell let
    /// off), the fight, Captain Hale, and on through the fish market and up into the city.
    /// </summary>
    private IEnumerable<ProofBeat> DocksBeats()
    {
        var beats = new List<object>();
        if (_arriving != Arriving.None)
        {
            beats.Add(ArrivalShot(Arriving.Sailing, 3.3f, "arrival-sailing.png"));
            beats.Add(ArrivalShot(Arriving.Sailing, 5f, "arrival-furling.png"));
            beats.Add(ArrivalShot(Arriving.Docked, 0.67f, "arrival-docked.png"));
            beats.Add(ArrivalShot(Arriving.Docked, 1.33f, "arrival-plank.png"));
            beats.Add(ArrivalShot(Arriving.Disembarking, 0.77f, "arrival-gangplank.png"));
            beats.Add(ArrivalShot(Arriving.Disembarking, 3f, "arrival-ashore.png"));
            beats.Add(ArrivalShot(Arriving.Card, 1.7f, "arrival-card.png"));
        }

        return Seq(
            Seq(beats.ToArray()),
            WaitFor(() => _arriving == Arriving.None),
            Answer(1, 0),
            Wait(30),
            Shot("docks-event.png"),
            Walk(ProofIntoTheGulls),
            ReadUntil(() => _game.Flags.Contains(Act1Chapter1.DocksDone), (Rolled, "docks-check.png"), ("red-gull", "docks-gulls.png"), ("hale", "docks-hale.png")),
            Wait(40),
            Shot("docks.png"),
            Out("city"));
    }

    /// <summary>How far east the walk into the Red Gulls goes at most, in art pixels: the conversation stops it once it's in their trigger.</summary>
    private const float ProofIntoTheGulls = 120f;

    /// <summary>A shot of the ship coming in, once the arrival's part has run so long, or as soon as it's past that part.</summary>
    /// <param name="part">The part of the arrival.</param>
    /// <param name="seconds">How far into it, on its own clock.</param>
    /// <param name="shot">The shot.</param>
    private ProofBeat ArrivalShot(Arriving part, float seconds, string shot)
    {
        bool past() => _arriving == Arriving.None || _arriving > part || _arriving == part && _arrivalClock >= seconds;
        return () => past() ? new ProofTick(true, shot) : new ProofTick(false);
    }

    /// <summary>
    /// Questeria, in off the harbour, room by room. The gate plaza: the Soul-Bound in the street, and
    /// the label at its east end. The festival square: the townsfolk about it a few seconds apart, the
    /// path round the fountain and a click to walk it, the elder, a point above the floor with no way
    /// there, Tab and a card, the mouse on the city gate, Alt held, the stair up to the Sunspire Steps
    /// and back down, and the waypoint crystal. The Lantern Market: the camera along it, Fennick and
    /// the quest board, you off on your own, the Flagon's badge with its "!", the zone map and a jump
    /// back to the square from it, then in through the Gilded Flagon's door.
    /// </summary>
    private IEnumerable<ProofBeat> QuesteriaBeats() => Seq(
        Shot("questeria.png"),
        Walk(ProofIntoTheSoulBound),
        Read(("soulbound", "questeria-soulbound.png")),
        GoToSpawn("gate-plaza-east"),
        Wait(20),
        Shot("questeria-edge-label.png"),
        Out("gate-plaza-east"),

        // The square, then the same view a few seconds later: the ones who wander have moved.
        GoToSpawn("start"),
        Wait(30),
        Shot("questeria-plaza.png"),
        Wait(210),
        Shot("questeria-plaza-later.png"),

        // West of the fountain, the mouse on the far side of it: the path goes round. Click, and walk it.
        Do(() => ProofPlace(Fountain() - new Vector2(FountainRadii().X + ProofRoundTheFountain, 0f), Direction.Right)),
        Do(() => ProofAimAt(Fountain() + new Vector2(FountainRadii().X + ProofRoundTheFountain, 0f))),
        Wait(25),
        Shot("questeria-path.png"),
        Do(ProofClick),
        Wait(44),
        Shot("questeria-click-walk.png"),
        Wait(95),
        Shot("questeria-path-end.png"),

        // A few steps from the elder, the mouse on them, then a click: you walk up and they speak.
        Do(ProofLetGo),
        Do(() => ProofPlace(ProofNpc("elder").Walker.Position + ProofOffTheElder, Direction.Right)),
        Hover("elder"),
        Wait(24),
        Shot("questeria-hover-npc.png"),
        Do(ProofClick),
        Wait(74),
        Shot("questeria-talk.png"),

        // The mouse up over the painting, where there's no door: there's no way there.
        Do(() => ProofAimAt(NoWayFrom(Leader.Position))),
        Wait(15),
        Shot("questeria-no-way.png"),

        // Tab to the Soul-Bound; the camera eases over. Then a click on your card takes you back.
        Do(ProofLetGo),
        Do(() => SelectLeader((_game.Leader + 1) % _walkers.Count)),
        Wait(24),
        Shot("questeria-switch.png"),
        ClickCard(0),
        Wait(22),
        Shot("questeria-card.png"),

        // Near the city gate, the mouse on it: its outline, its plaque and the door cursor. Then Alt.
        Do(() => ProofPlace(FindDoor("road").Foot + ProofOffTheGate, Direction.Right)),
        Hover("road"),
        Wait(20),
        Shot("questeria-door-hover.png"),
        Do(ProofLetGo),
        ShowAll(true),
        Wait(20),
        Shot("questeria-alt.png"),
        ShowAll(false),

        // The stair up: its chevrons and label under the mouse, then a click walks up it, and back down.
        Do(() => ProofPlace(new Vector2(FindExit("festival-square-up").X + ProofOffTheStair, Leader.Position.Y), Direction.Left)),
        Hover("festival-square-up"),
        Wait(20),
        Shot("questeria-stairs.png"),
        Do(ProofClick),
        WaitRoom("sunspire-steps"),
        Do(ProofLetGo),
        Wait(20),
        Out("sunspire-steps-down"),
        WaitRoom("festival-square"),

        // Beside the waypoint crystal, close enough that its name and E come up over it. Using it
        // finds it, and opens the zone map's world to travel; closed again.
        NextTo("crystal"),
        Wait(23),
        Shot("questeria-waypoint.png"),
        UseThing("crystal"),
        WaitFor(() => ProofMap() is not null),
        Wait(ProofScreenComesIn),
        Shot("map-world-first.png"),
        CloseMap(),
        Wait(12),
        Out("festival-square-east"),

        // Along the market, the camera after you; Fennick at the board, his lines, the Mirewood's contract.
        Do(() => ProofPlace(ProofMarketFrom, Direction.Right)),
        Walk(ProofMarketWalk),
        Shot("questeria-market-scroll.png"),
        TalkTo("fennick"),
        Read(("fennick", "questeria-board.png")),
        Shot("questeria-task.png"),

        // Off on your own along the market: the Soul-Bound finds their way after you.
        Do(() => ProofPlaceAlone(Leader.Position + ProofAlone, Direction.Right)),
        Wait(98),
        Shot("questeria-catch-up.png"),
        Wait(100),
        Shot("questeria-caught-up.png"),

        // The Flagon's tankard, with the innkeeper's "!" over it; the zone map, and a jump to the square.
        Do(() => ProofPlace(FindDoor("flagon").Foot + ProofOffTheFlagon, Direction.Left)),
        Wait(20),
        Shot("questeria-badges.png"),
        ZoneMap(),
        Shot("zone-map-questeria.png"),
        Jump("festival-square"),
        Wait(20),
        Shot("zone-map-jump.png"),

        // Then back to the market, and in through the Gilded Flagon's door.
        Out("flagon"));

    /// <summary>The Questeria visit's distances, in art pixels: how far east the walk into the Soul-Bound's trigger goes at most, how far past the fountain's ends the path round it starts and ends, where the walk along the market starts and how far it goes, and how far you go off on your own.</summary>
    private const float ProofIntoTheSoulBound = 80f;
    private const float ProofRoundTheFountain = 16f;
    private const float ProofOffTheStair = 50f;
    private const float ProofMarketWalk = 150f;
    private static readonly Vector2 ProofMarketFrom = new(300f, 192f);
    private static readonly Vector2 ProofAlone = new(220f, 0f);

    /// <summary>Where you stand for the mouse on the elder, the city gate and the Flagon's badge, from the elder's feet and the doors' feet, in art pixels.</summary>
    private static readonly Vector2 ProofOffTheElder = new(-40f, 6f);
    private static readonly Vector2 ProofOffTheGate = new(-60f, 40f);
    private static readonly Vector2 ProofOffTheFlagon = new(60f, 42f);

    /// <summary>Where the pretend mouse goes for there being no way: up over the painting, well to one side of you, where there's no door, way out, person or thing.</summary>
    private Vector2 NoWayFrom(Vector2 you)
    {
        float x = MathF.Max(ProofNoWayIn, you.X - ProofNoWayAside);
        return new Vector2(x, _room.BackEdgeAt(x) - ProofNoWayUp);
    }

    /// <summary>How far in from the end, to one side of you and up over the floor's back edge the no-way point is, in art pixels.</summary>
    private const float ProofNoWayIn = 60f;
    private const float ProofNoWayAside = 140f;
    private const float ProofNoWayUp = 60f;

    /// <summary>The middle of the festival square's fountain, and how far it reaches across and in depth: the room's one ellipse.</summary>
    private Vector2 Fountain() => _room.Blocks[0].Centre;
    private Vector2 FountainRadii() => _room.Blocks[0].Radii;

    /// <summary>Inside the Gilded Flagon: the tavern, the keeper's errand taken, and back out through the door.</summary>
    private IEnumerable<ProofBeat> FlagonBeats() => Seq(
        Shot("flagon.png"),
        TalkTo("innkeeper"),
        Read(("innkeeper", "flagon-innkeeper.png")),
        Out("door"));

    /// <summary>The old road: a walk in, the trigger in the farmland, then at the fork the chest, the sign, the zone map, the waypoint and the world, and fast travel back to Questeria.</summary>
    private IEnumerable<ProofBeat> RoadBeats() => Seq(
        Shot("road.png"),
        Walk(ProofRoadWalk),
        Shot("road-walk.png"),
        Into("watched"),
        Wait(49),
        Shot("road-trigger.png"),
        NextTo("camp"),
        Wait(24),
        Shot("road-camp.png"),
        UseThing("camp"),
        Wait(19),
        Shot("road-chest.png"),
        UseThing("fork"),
        Wait(40),
        Shot("road-sign.png"),
        ZoneMap(),
        Shot("map-road.png"),
        CloseMap(),
        Wait(12),
        UseThing("stone"),
        WaitFor(() => ProofMap() is not null),
        Wait(ProofScreenComesIn),
        Shot("map-world-travel.png"),
        Do(() => ProofMap().ProofTravel("questeria")));

    /// <summary>How far the walk in along the road goes, in art pixels: a little under a second of it.</summary>
    private const float ProofRoadWalk = 37f;

    /// <summary>The barrow: the steps in from the daylight, the dark further in, the cold, the hall, the locked tomb door, the stone door that won't move, the lever and the door it works, the side room's chest, the zone map, a torch, and paused, then out to the title.</summary>
    private IEnumerable<ProofBeat> BarrowBeats() => Seq(
        Shot("barrow.png"),
        Walk(ProofIntoTheDark),
        Shot("barrow-dark.png"),
        Into("cold"),
        Wait(30),
        Out("steps-east"),
        Wait(30),
        Shot("barrow-hall.png"),
        AtDoor("tomb"),
        Wait(18),
        Shot("barrow-door.png"),
        Out("tomb"),
        Wait(19),
        Shot("barrow-locked.png"),
        Out("side-shut"),
        Wait(18),
        Shot("barrow-gate-shut.png"),
        UseThing("side-lever"),
        Wait(20),
        Shot("barrow-lever.png"),

        // Back at the stone door, open now, once the line from trying it has gone. Then through it to the chest.
        AtDoor("side"),
        Wait(40),
        Shot("barrow-gate-open.png"),
        Out("side"),
        Wait(20),
        UseThing("side-chest"),
        Wait(20),
        Shot("barrow-chest.png"),
        ZoneMap(),
        Shot("map-barrow.png"),
        CloseMap(),
        Wait(12),

        // Right under a torch, then paused, and QUIT TO TITLE.
        Do(UnderATorch),
        Wait(22),
        Shot("barrow-torch.png"),
        Do(() => ScreenManager.AddScreen(new PauseMenuScreen())),
        Wait(20),
        Shot("explore-paused.png"),
        Do(() =>
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                if (screen is PauseMenuScreen pause) pause.ProofQuitToTitle();
        }));

    /// <summary>How far along the barrow's steps the walk into the dark goes, in art pixels.</summary>
    private const float ProofIntoTheDark = 110f;

    /// <summary>Stands you on the floor right under the first torch in the room.</summary>
    private void UnderATorch()
    {
        PropSprite torch = _props.Find(prop => prop.Definition.Light is not null);
        ProofPlace(_walk.NearestOnFloor(new Vector2(torch.Foot.X, _room.BackEdgeAt(torch.Foot.X) + ProofUnderTheTorch)), Direction.Right);
    }

    /// <summary>How far onto the floor from its back edge you stand under a torch, in art pixels.</summary>
    private const float ProofUnderTheTorch = 10f;

    /// <summary>A visit's beats in order, from single beats and runs of them.</summary>
    /// <param name="parts">Each a ProofBeat, or a run of them.</param>
    private static IEnumerable<ProofBeat> Seq(params object[] parts)
    {
        foreach (object part in parts)
        {
            if (part is ProofBeat beat) yield return beat;
            else foreach (ProofBeat each in (IEnumerable<ProofBeat>)part) yield return each;
        }
    }

    /// <summary>Waits till a room of the zone's in and faded in, without asking for it: for a way out you've set walking into.</summary>
    private ProofBeat WaitRoom(string room) => WaitFor(() => _room.Id == room && _roomChange is null && _roomFade <= 0f);

    /// <summary>Clicks one of the party's cards with the pretend mouse: on it, down, and up.</summary>
    private IEnumerable<ProofBeat> ClickCard(int index)
    {
        yield return Do(() => ScreenManager.Input.ProofMouse(ProofCard(index), false));
        yield return Do(() => ScreenManager.Input.ProofMouse(ProofCard(index), true));
        yield return Do(() => ScreenManager.Input.ProofMouse(ProofCard(index), false));
    }

    /// <summary>The choices to make, in order, at the next questions in this visit's conversations.</summary>
    private void ProofChoices(params int[] choices)
    {
        _proofChoices.Clear();
        foreach (int choice in choices) _proofChoices.Enqueue(choice);
    }

    /// <summary>Reads on whatever conversation's up, a press every few frames: the rest of a line, the next line, the next of the choices at a question, and the dice rolled and put away.</summary>
    private void ProofTalk()
    {
        if (_proofHolding is not null || ++_proofTalkClock % ProofReadEvery != 0) return;

        if (ProofDice() is DiceRollOverlay dice)
        {
            dice.ProofPress();
            return;
        }

        if (ProofDialogue() is not DialogueScreen talk) return;
        if (talk.ProofAsking) talk.ProofChoose(_proofChoices.Count > 0 ? _proofChoices.Dequeue() : 0);
        else talk.ProofAdvance();
    }

    /// <summary>Whether a shot hasn't been taken yet this run, marking it taken.</summary>
    private bool ProofOnce(string shot) => _proofShots.Add(shot);

    /// <summary>For a shot of a line: finishes typing it the first time it's seen, and says to take the shot a few frames later, once.</summary>
    private bool ProofHold(DialogueScreen talk, string shot)
    {
        if (_proofHolding != shot)
        {
            if (_proofShots.Contains(shot)) return false;
            _proofHolding = shot;
            _proofHoldClock = 0;
            talk.ProofFinishLine();
            return false;
        }

        if (++_proofHoldClock < ProofHoldFrames) return false;
        _proofHolding = null;
        return ProofOnce(shot);
    }

    /// <summary>The conversation that's up over this screen, or null.</summary>
    private DialogueScreen ProofDialogue()
    {
        foreach (GameScreen screen in ScreenManager.GetScreens())
            if (screen is DialogueScreen talk && !talk.IsExiting) return talk;
        return null;
    }

    /// <summary>The dice that are up over this screen, or null.</summary>
    private DiceRollOverlay ProofDice()
    {
        foreach (GameScreen screen in ScreenManager.GetScreens())
            if (screen is DiceRollOverlay dice && !dice.IsExiting) return dice;
        return null;
    }

    /// <summary>Somewhere the pretend mouse can be that's off the window, so nothing's under it.</summary>
    private static readonly Point OffScreen = new(-1, -1);

    /// <summary>Uses whatever's in reach, the same as pressing E. Does nothing if nothing is, which the next shot shows.</summary>
    private void ProofUse() => UseNearest();

    /// <summary>Keeps the pretend mouse over a point in the room from the next frame on.</summary>
    /// <param name="world">The point, in world pixels.</param>
    private void ProofAimAt(Vector2 world) => _proofAim = world;

    /// <summary>Clicks where the pretend mouse is aimed: the button goes down next frame and comes up the one after.</summary>
    private void ProofClick() => _proofPress = true;

    /// <summary>Takes the pretend mouse off the window.</summary>
    private void ProofLetGo()
    {
        _proofAim = null;
        ScreenManager.Input.ProofMouse(OffScreen, false);
    }

    /// <summary>Someone in the room, by their id in the zone file (or their origin id, waiting at camp).</summary>
    /// <exception cref="InvalidOperationException">If they aren't here, which means a beat names someone the room hasn't got, or who isn't there with the flags as they are.</exception>
    private Npc ProofNpc(string id) => _npcs.Find(npc => npc.Definition.Id == id) ?? throw Missing("person", id);

    /// <summary>The middle of one of the party's cards, on screen.</summary>
    private Point ProofCard(int index) => _strip.ProofCard(index, _game.Leader);

    /// <summary>The zone map that's up over this one, or null.</summary>
    private ZoneMapScreen ProofMap()
    {
        foreach (GameScreen screen in ScreenManager.GetScreens())
            if (screen is ZoneMapScreen map && !map.IsExiting) return map;
        return null;
    }

    /// <summary>Puts you somewhere in the room, facing a way, with the camera straight on you and the others behind you.</summary>
    /// <param name="feet">Where, in world pixels.</param>
    /// <param name="facing">Which way to face.</param>
    private void ProofPlace(Vector2 feet, Direction facing)
    {
        ProofPlaceAlone(feet, facing);
        GatherBehind(Followers());
        SeedTrail();
    }

    /// <summary>Puts just you somewhere in the room, and leaves the others where they are to find their own way to you.</summary>
    /// <param name="feet">Where, in world pixels.</param>
    /// <param name="facing">Which way to face.</param>
    private void ProofPlaceAlone(Vector2 feet, Direction facing)
    {
        Halt();
        Leader.Position = feet;
        Leader.Facing = facing;
        _camera.SnapTo(Leader.Position);
    }
}
