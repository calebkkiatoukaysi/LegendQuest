using Microsoft.Xna.Framework;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's part of the proof run: walking round the village, then the barrow, then out
/// through the pause menu.
/// </summary>
/// <remarks>
/// Movement goes in through the same code the keyboard uses (ReadMovement), so the shots actually
/// test the collision, the sorting, the camera and the footsteps. I only teleport between scenes
/// to keep the run short, like the lobby's schedule in The Black Box.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How many frames a map gets to fade in before the first shot.</summary>
    private const int Settled = 50;

    /// <summary>The last frame of each map's schedule. The village sends you to the barrow on its last one, the barrow quits to the title on its.</summary>
    internal const int VillageProofEnd = Settled + 264;
    internal const int BarrowProofEnd = Settled + 208;

    /// <summary>A direction to walk in instead of reading the keyboard, while the proof run is driving.</summary>
    private Vector2? _proofMove;

    /// <summary>Sets up whatever this frame needs, and returns a file name if this frame should be saved.</summary>
    /// <param name="frame">Which drawn frame this is, counted from the map's first.</param>
    /// <returns>The file the frame should be written to, or null.</returns>
    internal string ProofStep(int frame) => _map.Id == "barrow" ? BarrowStep(frame) : VillageStep(frame);

    /// <summary>The village: a walk up the road, round behind a roof, both sides of a barrel, along a wall, and both zooms.</summary>
    private string VillageStep(int frame)
    {
        switch (frame)
        {
            case Settled:
                return "village.png";

            // Up the road into the square.
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

            // Then off to the barrow. There are no exits yet, so this is the only way there.
            case Settled + 264:
                _camera.ZoomBy(-1);
                LoadingScreen.Load(ScreenManager, MapCatalog.Get("barrow").Name.ToUpperInvariant(),
                    new ExploreScreen(new SaveData { MapId = "barrow" }));
                return null;

            default:
                return null;
        }
    }

    /// <summary>The barrow: the way in, the dark passage, the hall, right under a torch, and paused.</summary>
    private string BarrowStep(int frame)
    {
        switch (frame)
        {
            case Settled:
                return "barrow.png";

            // Up out of the entrance into the passage, which has no torch.
            case Settled + 1:
                _proofMove = new Vector2(0f, -1f);
                return null;

            case Settled + 75:
                return "barrow-dark.png";

            case Settled + 160:
                return "barrow-hall.png";

            case Settled + 161:
                _proofMove = null;
                ProofPlace(11.5f, 15.2f, Direction.Up);
                return null;

            case Settled + 185:
                return "barrow-torch.png";

            case Settled + 187:
                ScreenManager.AddScreen(new PauseMenuScreen());
                return null;

            case Settled + 207:
                return "explore-paused.png";

            case Settled + 208:
                foreach (GameScreen screen in ScreenManager.GetScreens())
                    if (screen is PauseMenuScreen pause) pause.ProofQuitToTitle();
                return null;

            default:
                return null;
        }
    }

    /// <summary>Puts you somewhere, facing a way, with the camera straight on you.</summary>
    /// <param name="x">How far across, in tiles.</param>
    /// <param name="y">How far down, in tiles.</param>
    /// <param name="facing">Which way to face.</param>
    private void ProofPlace(float x, float y, Direction facing)
    {
        _walker.Position = new Vector2(x, y) * Tilemap.TileSize;
        _walker.Facing = facing;
        _camera.SnapTo(_walker.Position);
    }
}
