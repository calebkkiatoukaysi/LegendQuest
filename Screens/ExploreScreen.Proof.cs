using Microsoft.Xna.Framework;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's part of the proof run: the arrival at the harbour, Questeria and the
/// innkeeper's work, the road, fast travel both ways, the barrow, then out through the pause menu.
/// </summary>
/// <remarks>
/// Movement goes in through the same code the keyboard uses (ReadMovement), using things goes
/// through the same code the E key does, and the mouse is a pretend one in InputState that the
/// screen reads like the real one, so the shots actually test the collision, the paths, the
/// triggers, the exits and the rules. I only teleport between scenes to keep the run short, like the lobby's
/// schedule in The Black Box. Each visit to a map is its own stage, so each gets its own schedule.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How many frames a map gets to fade in before the first shot.</summary>
    private const int Settled = 50;

    /// <summary>The last frame of each visit's schedule. Most of them end walking into an exit or travelling, and the next stage picks up on the next map.</summary>
    internal const int DocksProofEnd = ProofArrivalRead + ProofReadFrames + 302;
    internal const int QuesteriaProofEnd = Settled + 1466;
    internal const int RoadProofEnd = Settled + 267;
    internal const int QuesteriaAgainProofEnd = Settled + 26;
    internal const int RoadAgainProofEnd = Settled + 316;
    internal const int BarrowProofEnd = Settled + 593;

    /// <summary>When the arrival's conversation is up and gets read, and how: a line every few frames for a while.</summary>
    private const int ProofArrivalRead = 840;
    private const int ProofReadEvery = 6;
    private const int ProofReadFrames = 80;

    /// <summary>When each of Questeria's conversations gets read, after Settled: Valen's, the innkeeper's work, the rosewater, and the pay.</summary>
    private static readonly int[] ProofReads = { 72, 880, 1006, 1090 };

    /// <summary>A direction to walk in instead of reading the keyboard, while the proof run is driving.</summary>
    private Vector2? _proofMove;

    /// <summary>A point on the map the pretend mouse stays over, whatever the camera does, and whether its button is down this frame.</summary>
    private Vector2? _proofAim;
    private bool _proofPress;

    /// <summary>Which map this is, so the proof run can tell one map's screen from the last one's.</summary>
    internal string MapId => _map.Id;

    /// <summary>Sets up whatever this frame needs, and returns a file name if this frame should be saved.</summary>
    /// <param name="frame">Which drawn frame this is, counted from the map's first.</param>
    /// <param name="stage">Which part of the run it is, which says which visit to this map it is.</param>
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

        return stage switch
        {
            ProofStage.Docks => DocksStep(frame),
            ProofStage.Questeria => QuesteriaStep(frame),
            ProofStage.Road => RoadStep(frame),
            ProofStage.QuesteriaAgain => QuesteriaAgainStep(frame),
            ProofStage.RoadAgain => RoadAgainStep(frame),
            _ => BarrowStep(frame),
        };
    }

    /// <summary>The harbour: the ship sailing in, the gangplank, the walk off, Valen talking, the card, then up the street to the city.</summary>
    /// <remarks>Nothing's driven till the arrival's over; it plays itself. The conversation gets read a line every few frames, like someone pressing Enter.</remarks>
    private string DocksStep(int frame)
    {
        if (frame is >= ProofArrivalRead and < ProofArrivalRead + ProofReadFrames && frame % ProofReadEvery == 0)
        {
            ProofRead();
            return null;
        }

        switch (frame)
        {
            case 200:
                return "arrival-sailing.png";

            case 520:
                return "arrival-docked.png";

            case 640:
                return "arrival-gangplank.png";

            case ProofArrivalRead - 10:
                return "arrival-valen.png";

            case ProofArrivalRead + ProofReadFrames + 80:
                return "arrival-card.png";

            case ProofArrivalRead + ProofReadFrames + 300:
                return "docks.png";

            // Then up the quay street and off the east side, into the city.
            case ProofArrivalRead + ProofReadFrames + 302:
                ProofPlace(52f, 7f, Direction.Right);
                _proofMove = new Vector2(1f, 0f);
                return null;

            default:
                return null;
        }
    }

    /// <summary>Questeria, in off the harbour: Valen in the street, the townsfolk about the plaza, behind a roof, both zooms, the path round a statue and a click to walk it, the elder, the canal, Tab and a card, the innkeeper's work start to finish, you off on your own, the waypoint, the map, then out through the city gate.</summary>
    private string QuesteriaStep(int frame)
    {
        foreach (int read in ProofReads)
        {
            if (frame >= Settled + read && frame < Settled + read + ProofReadFrames && frame % ProofReadEvery == 0)
            {
                ProofRead();
                return null;
            }
        }

        switch (frame)
        {
            case Settled:
                return "questeria.png";

            // East up the street into Valen's trigger: he talks.
            case Settled + 1:
                _proofMove = new Vector2(1f, 0f);
                return null;

            case Settled + 45:
                _proofMove = null;
                return null;

            case Settled + 70:
                return "questeria-valen.png";

            // The plaza, then the same view a few seconds later: the ones who wander have moved.
            case Settled + 160:
                ProofPlace(42f, 33f, Direction.Up);
                return null;

            case Settled + 190:
                return "questeria-plaza.png";

            case Settled + 400:
                return "questeria-plaza-later.png";

            // Along behind the ivy house. Its roof should hide you.
            case Settled + 402:
                ProofPlace(2.5f, 24.5f, Direction.Right);
                _proofMove = new Vector2(1f, 0f);
                return null;

            case Settled + 455:
                return "questeria-behind-roof.png";

            // In front of the fountain, zoomed all the way out, then all the way in.
            case Settled + 457:
                _proofMove = null;
                ProofPlace(42f, 32.5f, Direction.Down);
                _camera.ZoomBy(-1);
                return null;

            case Settled + 480:
                return "questeria-zoom-2.png";

            case Settled + 482:
                _camera.ZoomBy(2);
                return null;

            case Settled + 505:
                return "questeria-zoom-4.png";

            // South of the west statue, the mouse over the far side of it: the path goes round it.
            case Settled + 507:
                _camera.ZoomBy(-1);
                ProofPlace(32f, 25f, Direction.Up);
                return null;

            case Settled + 515:
                ProofAimAt(new Vector2(32f, 20.5f) * Tilemap.TileSize);
                return null;

            case Settled + 540:
                return "questeria-path.png";

            // Click, and walk it. Valen follows.
            case Settled + 541:
                ProofClick();
                return null;

            case Settled + 585:
                return "questeria-click-walk.png";

            case Settled + 680:
                return "questeria-path-end.png";

            // A few steps from the elder, the mouse on them, then a click: you walk up and they speak.
            case Settled + 681:
                ProofLetGo();
                ProofPlace(36f, 33.5f, Direction.Up);
                ProofAimAt(ProofNpc("elder") - new Vector2(0f, HeadHeight / 2f));
                return null;

            case Settled + 705:
                return "questeria-hover-npc.png";

            case Settled + 706:
                ProofClick();
                return null;

            case Settled + 780:
                return "questeria-talk.png";

            // The mouse on the canal: there's no way there.
            case Settled + 781:
                ProofAimAt(new Vector2(20f, 39f) * Tilemap.TileSize);
                return null;

            case Settled + 795:
                return "questeria-invalid.png";

            // Tab to Valen; the camera eases over to him. Then a click on your card takes you back.
            case Settled + 796:
                ProofLetGo();
                SelectLeader((_game.Leader + 1) % _walkers.Count);
                return null;

            case Settled + 820:
                return "questeria-switch.png";

            case Settled + 821:
                ScreenManager.Input.ProofMouse(ProofCard(0), false);
                return null;

            case Settled + 822:
                ScreenManager.Input.ProofMouse(ProofCard(0), true);
                return null;

            case Settled + 823:
                ScreenManager.Input.ProofMouse(ProofCard(0), false);
                return null;

            case Settled + 845:
                return "questeria-card.png";

            // The innkeeper: work.
            case Settled + 846:
                ProofLetGo();
                ProofPlace(64f, 33.6f, Direction.Up);
                return null;

            case Settled + 848:
                ProofUse();
                return null;

            case Settled + 875:
                return "questeria-innkeeper.png";

            case Settled + 975:
                return "questeria-task.png";

            // The flower seller hands over the rosewater.
            case Settled + 976:
                ProofPlace(75.4f, 33f, Direction.Right);
                return null;

            case Settled + 978:
                ProofUse();
                return null;

            case Settled + 1005:
                return "questeria-flowers.png";

            // Back to the innkeeper, who pays.
            case Settled + 1060:
                ProofPlace(64f, 33.6f, Direction.Up);
                return null;

            case Settled + 1062:
                ProofUse();
                return null;

            case Settled + 1180:
                return "questeria-paid.png";

            // Off on your own across the plaza: Valen finds his way after you.
            case Settled + 1182:
                ProofPlaceAlone(50f, 24.5f, Direction.Down);
                return null;

            case Settled + 1280:
                return "questeria-catch-up.png";

            case Settled + 1380:
                return "questeria-caught-up.png";

            // Beside the waypoint crystal, close enough that its name and E come up over it.
            case Settled + 1382:
                ProofPlace(35.4f, 26.3f, Direction.Left);
                return null;

            case Settled + 1405:
                return "questeria-waypoint.png";

            // Using it finds it, and opens the world to travel.
            case Settled + 1406:
                ProofUse();
                return null;

            case Settled + 1430:
                return "map-world-first.png";

            // Closed, then M for the map of Questeria, all of it, since the capital has no fog.
            case Settled + 1431:
                ProofMap().ExitScreen();
                return null;

            case Settled + 1440:
                OpenMap(null);
                return null;

            case Settled + 1460:
                return "map-questeria.png";

            // Then south through the city gate into the exit, which autosaves and goes to the old road.
            case Settled + 1461:
                ProofMap().ExitScreen();
                return null;

            case Settled + 1466:
                ProofPlace(42f, 55.5f, Direction.Down);
                _proofMove = new Vector2(0f, 1f);
                return null;

            default:
                return null;
        }
    }

    /// <summary>The old road: a walk in, the trigger before the camp, the chest, the sign, the waypoint, both sides of the map, and fast travel back to Questeria.</summary>
    private string RoadStep(int frame)
    {
        switch (frame)
        {
            case Settled:
                return "road.png";

            case Settled + 1:
                _proofMove = new Vector2(1f, 0f);
                return null;

            case Settled + 40:
                return "road-walk.png";

            // On along the road into the trigger before the camp.
            case Settled + 41:
                ProofPlace(15f, 16.5f, Direction.Right);
                return null;

            case Settled + 90:
                return "road-trigger.png";

            // The chest in the camp, by the fire.
            case Settled + 91:
                _proofMove = null;
                ProofPlace(29.125f, 22.625f, Direction.Right);
                return null;

            case Settled + 115:
                return "road-camp.png";

            case Settled + 116:
                ProofUse();
                return null;

            case Settled + 135:
                return "road-chest.png";

            // The sign at the fork.
            case Settled + 137:
                ProofPlace(37f, 13.75f, Direction.Right);
                return null;

            case Settled + 160:
                ProofUse();
                return null;

            case Settled + 200:
                return "road-sign.png";

            // The waypoint: found, so now there are two, and Questeria is somewhere to go.
            case Settled + 202:
                ProofPlace(45.75f, 11.75f, Direction.Left);
                return null;

            case Settled + 215:
                ProofUse();
                return null;

            case Settled + 240:
                return "map-world-travel.png";

            case Settled + 241:
                ProofMap().ProofShow(MapView.Here);
                return null;

            case Settled + 265:
                return "map-road.png";

            case Settled + 267:
                ProofMap().ProofTravel("questeria");
                return null;

            default:
                return null;
        }
    }

    /// <summary>Back in Questeria, standing at the crystal. Straight back to the road by it.</summary>
    private string QuesteriaAgainStep(int frame)
    {
        switch (frame)
        {
            case Settled:
                return "questeria-arrived.png";

            case Settled + 2:
                ProofUse();
                return null;

            case Settled + 26:
                ProofMap().ProofTravel("road");
                return null;

            default:
                return null;
        }
    }

    /// <summary>On the road at its stone, into the trigger at the blocked end twice (it goes off every time), then up the path north into the exit to the barrow.</summary>
    private string RoadAgainStep(int frame)
    {
        switch (frame)
        {
            case Settled:
                return "road-arrived.png";

            // Into the trigger, back out, and in again once its line has gone, so the shot shows it going off a second time.
            case Settled + 1:
                ProofPlace(51.5f, 16.5f, Direction.Right);
                _proofMove = new Vector2(1f, 0f);
                return null;

            case Settled + 30:
                _proofMove = null;
                ProofPlace(51.5f, 16.5f, Direction.Right);
                return null;

            case Settled + 250:
                _proofMove = new Vector2(1f, 0f);
                return null;

            case Settled + 295:
                return "road-blocked.png";

            case Settled + 296:
                ProofPlace(40.5f, 5f, Direction.Up);
                _proofMove = new Vector2(0f, -1f);
                return null;

            case Settled + 316:
                return "road-path.png";

            default:
                return null;
        }
    }

    /// <summary>The barrow: the way in, the dark passage, the hall, the locked door, the lever and the door it works, the chest, the map, a torch, and paused.</summary>
    private string BarrowStep(int frame)
    {
        switch (frame)
        {
            case Settled:
                return "barrow.png";

            // Up out of the entrance into the passage, which has no torch, and on into the hall's trigger.
            case Settled + 1:
                _proofMove = new Vector2(0f, -1f);
                return null;

            case Settled + 75:
                return "barrow-dark.png";

            case Settled + 150:
                return "barrow-hall.png";

            // The tomb door: locked.
            case Settled + 151:
                _proofMove = null;
                ProofPlace(21.2f, 14.7f, Direction.Left);
                return null;

            case Settled + 170:
                return "barrow-door.png";

            case Settled + 171:
                ProofUse();
                return null;

            case Settled + 190:
                return "barrow-locked.png";

            // The stone door at the west end: it won't open by hand.
            case Settled + 192:
                ProofPlace(6.2f, 14.7f, Direction.Left);
                return null;

            case Settled + 212:
                ProofUse();
                return null;

            case Settled + 230:
                return "barrow-gate-shut.png";

            // The lever at the east end, which opens it.
            case Settled + 232:
                ProofPlace(29.5f, 14.4f, Direction.Right);
                return null;

            case Settled + 250:
                ProofUse();
                return null;

            case Settled + 270:
                return "barrow-lever.png";

            // Back at the stone door, open now, with the side room's light coming through it, once the
            // line from trying it has gone. Then through it to the chest.
            case Settled + 272:
                ProofPlace(5f, 15.2f, Direction.Up);
                return null;

            case Settled + 410:
                return "barrow-gate-open.png";

            case Settled + 412:
                _proofMove = new Vector2(0f, -1f);
                return null;

            case Settled + 472:
                _proofMove = null;
                ProofPlace(6.25f, 8.75f, Direction.Left);
                return null;

            case Settled + 495:
                ProofUse();
                return null;

            case Settled + 515:
                return "barrow-chest.png";

            // The map of the barrow, as much as that's seen.
            case Settled + 517:
                OpenMap(null);
                return null;

            case Settled + 545:
                return "map-barrow.png";

            case Settled + 546:
                ProofMap().ExitScreen();
                return null;

            // Right under a torch.
            case Settled + 548:
                ProofPlace(8.5f, 15f, Direction.Up);
                return null;

            case Settled + 570:
                return "barrow-torch.png";

            case Settled + 572:
                ScreenManager.AddScreen(new PauseMenuScreen());
                return null;

            case Settled + 592:
                return "explore-paused.png";

            case Settled + 593:
                foreach (GameScreen screen in ScreenManager.GetScreens())
                    if (screen is PauseMenuScreen pause) pause.ProofQuitToTitle();
                return null;

            default:
                return null;
        }
    }

    /// <summary>Somewhere the pretend mouse can be that's off the window, so nothing's under it.</summary>
    private static readonly Point OffScreen = new(-1, -1);

    /// <summary>Uses whatever's in reach, the same as pressing E. Does nothing if nothing is, which the next shot shows.</summary>
    private void ProofUse() => UseNearest();

    /// <summary>Keeps the pretend mouse over a point on the map from the next frame on.</summary>
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

    /// <summary>Where someone's feet are, by their id in the map file.</summary>
    private Vector2 ProofNpc(string id) => _npcs.Find(npc => npc.Definition.Id == id).Walker.Position;

    /// <summary>The middle of one of the party's cards, on screen.</summary>
    private Point ProofCard(int index) => _strip.ProofCard(index, _game.Leader);

    /// <summary>Reads the conversation that's up on: the rest of its line, or the next. Does nothing if there isn't one.</summary>
    private void ProofRead()
    {
        foreach (GameScreen screen in ScreenManager.GetScreens())
            if (screen is DialogueScreen talk && !talk.IsExiting) talk.ProofAdvance();
    }

    /// <summary>The map screen that's up over this one.</summary>
    private MapScreen ProofMap()
    {
        foreach (GameScreen screen in ScreenManager.GetScreens())
            if (screen is MapScreen map && !map.IsExiting) return map;
        return null;
    }

    /// <summary>Puts you somewhere, facing a way, with the camera straight on you.</summary>
    /// <param name="x">How far across, in tiles.</param>
    /// <param name="y">How far down, in tiles.</param>
    /// <param name="facing">Which way to face.</param>
    private void ProofPlace(float x, float y, Direction facing)
    {
        ProofPlaceAlone(x, y, facing);
        GatherBehind(Followers());
        SeedTrail();
    }

    /// <summary>Puts just you somewhere, and leaves the others where they are to find their own way to you.</summary>
    /// <param name="x">How far across, in tiles.</param>
    /// <param name="y">How far down, in tiles.</param>
    /// <param name="facing">Which way to face.</param>
    private void ProofPlaceAlone(float x, float y, Direction facing)
    {
        Halt();
        Leader.Position = new Vector2(x, y) * Tilemap.TileSize;
        Leader.Facing = facing;
        _camera.SnapTo(Leader.Position);
    }
}
