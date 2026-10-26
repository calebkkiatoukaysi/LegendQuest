using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Save;
using LegendQuest.Screens;
using LegendQuest.StateManagement;

namespace LegendQuest;

/// <summary>
/// The proof run: plays through the screens on its own, saves a picture of each one, and quits.
/// </summary>
/// <remarks>
/// Same harness as The Black Box. It goes through the game like a player would, on a scratch save
/// folder inside the shots folder, so it never touches a real slot. It's silent: every sound that
/// would have played gets written to audio.log with the frame it was asked for on. Screens with a
/// schedule of their own get their own frame count once they're up.
/// </remarks>
public partial class LegendQuestGame
{
    /// <summary>Where the shots go, or null when the game is being played.</summary>
    public string ProofDirectory { get; init; }

    // The menus by frame. Each one gets time to fade in before its shot.
    private const int ProofTitleShot = 60;
    private const int ProofOptionsOpen = 61;
    private const int ProofOptionsShot = 85;
    private const int ProofOptionsClose = 86;
    private const int ProofLoadOpen = 105;
    private const int ProofLoadShot = 130;
    private const int ProofEraseAsk = 131;
    private const int ProofEraseAskShot = 140;
    private const int ProofEraseSure = 141;
    private const int ProofErasedShot = 150;
    private const int ProofLoadClose = 151;
    private const int ProofNewOpen = 170;
    private const int ProofNewShot = 195;
    private const int ProofNewClose = 196;
    private const int ProofPauseOpen = 215;
    private const int ProofPauseShot = 235;
    private const int ProofPauseQuit = 236;

    /// <summary>The slot the proof breaks on purpose, and then erases.</summary>
    private const int ProofBrokenSlot = 1;

    /// <summary>How long the title gets to settle after coming back through the loading screen.</summary>
    private const int ProofSettle = 60;

    /// <summary>The frame the proof run is on, which is what schedules it.</summary>
    private int _proofFrame;

    /// <summary>Whether a frame has been drawn since the schedule last moved.</summary>
    /// <remarks>
    /// Saving a PNG stalls the fixed timestep, and it catches up with a burst of Updates with no
    /// Draw between. The Black Box's first draft lost shots to that burst, so the schedule only
    /// counts drawn frames.
    /// </remarks>
    private bool _proofDrawn = true;

    /// <summary>The file the next frame is to be written to, or null.</summary>
    private string _proofPending;

    /// <summary>The target the frame is being drawn into while a shot is pending.</summary>
    private RenderTarget2D _proofTarget;

    /// <summary>Which part of the run it's on, the screen whose schedule is running, and the frame that screen opened on.</summary>
    private ProofStage _proofStage = ProofStage.Menus;
    private GameScreen _proofSubject;
    private int _proofSubjectStart;

    /// <summary>Every sound the run played, by frame. Gets written out at the end.</summary>
    private readonly StringBuilder _proofAudio = new();
    private int _proofAudioSeen;

    /// <summary>Makes the run silent and blind to the real devices, and gives it a scratch save folder with one of every kind of slot in it.</summary>
    /// <remarks>The scratch folder gets wiped first, so every run starts from the same slots.</remarks>
    private void BeginProof()
    {
        _audio.Silent = true;
        SettingsStore.ReadOnly = true;
        _screenManager.IgnoreWindowFocus = true;
        _screenManager.Input.IgnoreDevices = true;

        string saves = Path.Combine(ProofDirectory, "saves");
        if (Directory.Exists(saves)) Directory.Delete(saves, recursive: true);
        SaveSystem.Folder = saves;

        SaveSystem.Write(0, new SaveData { MapId = "village", PlaytimeSeconds = 4320 });
        SaveSystem.Write(SaveSystem.QuickSlot, new SaveData { MapId = "road", PlaytimeSeconds = 2840 });
        File.WriteAllText(SaveSystem.PathFor(ProofBrokenSlot), "{ this was never a save");
    }

    /// <summary>Runs the proof schedule, one drawn frame at a time.</summary>
    private void UpdateProof()
    {
        if (ProofDirectory is null) return;
        if (!_proofDrawn) return;

        _proofDrawn = false;
        _proofFrame++;
        RecordProofAudio();

        switch (_proofStage)
        {
            case ProofStage.Menus:
                _proofPending = ProofMenus();
                break;

            case ProofStage.TitleAgain:
                _proofPending = RunSubject<TitleScreen>(_ => Frame() == ProofSettle ? "title-again.png" : null, ProofSettle, ProofStage.Done);
                break;

            default:
                File.WriteAllText(Path.Combine(ProofDirectory, "audio.log"), _proofAudio.ToString());
                Exit();
                break;
        }
    }

    /// <summary>The title, options over it, the slots both ways (with an erase), then the pause menu out to the title.</summary>
    private string ProofMenus()
    {
        switch (_proofFrame)
        {
            case ProofTitleShot:
                return "title.png";

            case ProofOptionsOpen:
                _screenManager.AddScreen(new OptionsScreen());
                return null;

            case ProofOptionsShot:
                return "options.png";

            case ProofLoadOpen:
                _screenManager.AddScreen(new SaveSlotScreen(SlotPurpose.Load));
                return null;

            case ProofLoadShot:
                return "slots-load.png";

            // Erase the broken slot the way a player would: ERASE, then SURE?.
            case ProofEraseAsk:
            case ProofEraseSure:
                Find<SaveSlotScreen>()?.ProofErase(ProofBrokenSlot);
                return null;

            case ProofEraseAskShot:
                return "slots-erase-asking.png";

            case ProofErasedShot:
                return "slots-erased.png";

            case ProofNewOpen:
                _screenManager.AddScreen(new SaveSlotScreen(SlotPurpose.NewGame));
                return null;

            case ProofNewShot:
                return "slots-new.png";

            case ProofOptionsClose:
            case ProofLoadClose:
            case ProofNewClose:
                foreach (GameScreen screen in _screenManager.GetScreens())
                    if (screen is OptionsScreen or SaveSlotScreen) screen.ExitScreen();
                return null;

            // There's nothing to pause yet, so the pause menu goes over the title. QUIT TO TITLE takes
            // the long way round, through the loading screen.
            case ProofPauseOpen:
                _screenManager.AddScreen(new PauseMenuScreen());
                return null;

            case ProofPauseShot:
                return "pause.png";

            case ProofPauseQuit:
                Find<PauseMenuScreen>()?.ProofQuitToTitle();
                _proofStage = ProofStage.TitleAgain;
                return null;

            default:
                return null;
        }
    }

    /// <summary>The first screen of type T on the stack that isn't on its way out, or null.</summary>
    private T Find<T>() where T : GameScreen
    {
        foreach (GameScreen screen in _screenManager.GetScreens())
            if (screen is T found && !found.IsExiting) return found;
        return null;
    }

    /// <summary>Waits for a screen of type T to open, then runs one step of its schedule each frame until it's done.</summary>
    /// <typeparam name="T">The screen to wait for.</typeparam>
    /// <param name="step">One frame of its schedule. Returns the file name to save, or null.</param>
    /// <param name="end">The last frame of its schedule.</param>
    /// <param name="next">Which part comes next.</param>
    private string RunSubject<T>(System.Func<T, string> step, int end, ProofStage next) where T : GameScreen
    {
        if (_proofSubject is not T subject)
        {
            T found = Find<T>();
            if (found is not null)
            {
                _proofSubject = found;
                _proofSubjectStart = _proofFrame;
            }
            return null;
        }

        if (Frame() > end)
        {
            _proofStage = next;
            _proofSubject = null;
            return null;
        }

        return step(subject);
    }

    /// <summary>How many frames the current screen has been open.</summary>
    private int Frame() => _proofFrame - _proofSubjectStart;

    /// <summary>Copies any new sounds from the audio manager's log, along with the frame they played on.</summary>
    private void RecordProofAudio()
    {
        for (; _proofAudioSeen < _audio.Log.Count; _proofAudioSeen++)
            _proofAudio.Append(_proofFrame).Append(' ').AppendLine(_audio.Log[_proofAudioSeen]);
    }

    /// <summary>Points the frame at a render target if a shot is pending.</summary>
    private void BeginProofCapture()
    {
        if (_proofPending is null) return;

        _proofTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight);
        GraphicsDevice.SetRenderTarget(_proofTarget);
    }

    /// <summary>Writes the frame out if one was pending, and puts the window back.</summary>
    private void EndProofCapture()
    {
        _proofDrawn = true;
        if (_proofTarget is null) return;

        GraphicsDevice.SetRenderTarget(null);

        Directory.CreateDirectory(ProofDirectory);
        using (FileStream file = File.Create(Path.Combine(ProofDirectory, _proofPending)))
            _proofTarget.SaveAsPng(file, ScreenWidth, ScreenHeight);

        _proofTarget.Dispose();
        _proofTarget = null;
        _proofPending = null;
    }
}
