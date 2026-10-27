using Microsoft.Xna.Framework;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's part of the proof run: the village, the road, fast travel both ways, the barrow,
/// then out through the pause menu.
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
    internal const int VillageProofEnd = Settled + 926;
    internal const int RoadProofEnd = Settled + 267;
    internal const int VillageAgainProofEnd = Settled + 26;
    internal const int RoadAgainProofEnd = Settled + 316;
    internal const int BarrowProofEnd = Settled + 593;

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
            ProofStage.Village => VillageStep(frame),
            ProofStage.Road => RoadStep(frame),
            ProofStage.VillageAgain => VillageAgainStep(frame),
            ProofStage.RoadAgain => RoadAgainStep(frame),
            _ => BarrowStep(frame),
        };
    }

    /// <summary>The village: a walk with the party behind you, round behind a roof, both sides of a barrel, along a wall, both zooms, the mouse (a path, a click, the elder, a wall), switching who you control, the others catching up, the waypoint, the map, and off east.</summary>
    private string VillageStep(int frame)
    {
        switch (frame)
        {
            case Settled:
                return "village.png";

            // Up the road into the square, with the others falling in behind.
            case Settled + 1:
                _proofMove = new Vector2(0f, -1f);
                return null;

            case Settled + 40:
                return "village-walk.png";

            // Along behind the elder's house, where the thatch hangs over the ground. You should go under it.
            case Settled + 70:
                ProofPlace(21.2f, 3.6f, Direction.Right);
                _proofMove = new Vector2(1f, 0f);
                return null;

            case Settled + 125:
                return "village-behind-roof.png";

            // Just behind a barrel, then just in front of it, to check the sorting.
            case Settled + 127:
                _proofMove = null;
                ProofPlace(22.5f, 8.95f, Direction.Up);
                return null;

            case Settled + 150:
                return "village-behind-barrel.png";

            case Settled + 152:
                ProofPlace(22.5f, 10.1f, Direction.Up);
                return null;

            case Settled + 170:
                return "village-front-barrel.png";

            // Into the front of a house at an angle, so you slide along it instead of stopping.
            case Settled + 172:
                ProofPlace(12.5f, 10.5f, Direction.Up);
                _proofMove = new Vector2(0.6f, -1f);
                return null;

            case Settled + 210:
                return "village-slide.png";

            // By the well, zoomed all the way out, then all the way in.
            case Settled + 211:
                _proofMove = null;
                ProofPlace(28f, 20.5f, Direction.Down);
                _camera.ZoomBy(-1);
                return null;

            case Settled + 235:
                return "village-zoom-2.png";

            case Settled + 237:
                _camera.ZoomBy(2);
                return null;

            case Settled + 260:
                return "village-zoom-4.png";

            // South of the well, the mouse over the far side of it: the path goes round it.
            case Settled + 262:
                _camera.ZoomBy(-1);
                ProofPlace(27.5f, 21.5f, Direction.Up);
                return null;

            case Settled + 270:
                ProofAimAt(new Vector2(27.5f, 13.5f) * Tilemap.TileSize);
                return null;

            case Settled + 295:
                return "village-path.png";

            // Click, and walk it. The others follow, and the line ends up behind you.
            case Settled + 296:
                ProofClick();
                return null;

            case Settled + 340:
                return "village-click-walk.png";

            case Settled + 470:
                return "village-path-end.png";

            // The mouse on the elder, then a click: you walk up to them and they turn and speak.
            case Settled + 471:
                ProofAimAt(ProofNpc("elder") - new Vector2(0f, HeadHeight / 2f));
                return null;

            case Settled + 495:
                return "village-hover-npc.png";

            case Settled + 496:
                ProofClick();
                return null;

            case Settled + 570:
                return "village-talk.png";

            // The mouse on a wall: there's no way there.
            case Settled + 571:
                ProofAimAt(new Vector2(25.5f, 7.5f) * Tilemap.TileSize);
                return null;

            case Settled + 585:
                return "village-invalid.png";

            // Tab to the next in line; the camera eases over to them.
            case Settled + 586:
                ProofLetGo();
                SelectLeader((_game.Leader + 1) % _walkers.Count);
                return null;

            case Settled + 610:
                return "village-switch.png";

            // A click on the third card takes control of them.
            case Settled + 611:
                ScreenManager.Input.ProofMouse(ProofCard(2), false);
                return null;

            case Settled + 612:
                ScreenManager.Input.ProofMouse(ProofCard(2), true);
                return null;

            case Settled + 613:
                ScreenManager.Input.ProofMouse(ProofCard(2), false);
                return null;

            case Settled + 635:
                return "village-card.png";

            // Back to you, like F1, then off across the village on your own: the others find their way after you.
            case Settled + 636:
                ProofLetGo();
                SelectLeader(0);
                return null;

            case Settled + 641:
                ProofPlaceAlone(15.5f, 12.5f, Direction.Down);
                return null;

            case Settled + 740:
                return "village-catch-up.png";

            case Settled + 840:
                return "village-caught-up.png";

            // Beside the waypoint stone, close enough that its name and E come up over it.
            case Settled + 842:
                ProofPlace(35.75f, 13.75f, Direction.Left);
                return null;

            case Settled + 865:
                return "village-waypoint.png";

            // Using it finds it, and opens the world to travel. Nowhere else has a waypoint yet.
            case Settled + 866:
                ProofUse();
                return null;

            case Settled + 890:
                return "map-world-first.png";

            // Closed, then M for the map of the village, as much as the walk round has seen.
            case Settled + 891:
                ProofMap().ExitScreen();
                return null;

            case Settled + 900:
                OpenMap(null);
                return null;

            case Settled + 920:
                return "map-village.png";

            // Then east into the exit, which autosaves and goes to the old road.
            case Settled + 921:
                ProofMap().ExitScreen();
                return null;

            case Settled + 926:
                ProofPlace(52.5f, 17f, Direction.Right);
                _proofMove = new Vector2(1f, 0f);
                return null;

            default:
                return null;
        }
    }

    /// <summary>The old road: a walk in, the trigger before the camp, the chest, the sign, the waypoint, both sides of the map, and fast travel back to the village.</summary>
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

            // The waypoint: found, so now there are two, and the village is somewhere to go.
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
                ProofMap().ProofTravel("village");
                return null;

            default:
                return null;
        }
    }

    /// <summary>Back in the village, standing at the stone. Straight back to the road by it.</summary>
    private string VillageAgainStep(int frame)
    {
        switch (frame)
        {
            case Settled:
                return "village-arrived.png";

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
