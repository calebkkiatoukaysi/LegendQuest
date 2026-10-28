using System.IO;
using System.Text;
using LegendQuest.Characters;
using LegendQuest.Rules;
using LegendQuest.Save;
using LegendQuest.Screens;
using LegendQuest.StateManagement;
using LegendQuest.World;

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
    private const int ProofNewGame = 215;

    /// <summary>The frame the new game's run from the slots to the ship ends on, counted from when it starts.</summary>
    private const int ProofNewGameEnd = 442;

    /// <summary>The slot the proof breaks on purpose, and then erases.</summary>
    private const int ProofBrokenSlot = 1;

    /// <summary>How long the title gets to settle after coming back through the loading screen, and when the slots open over it after.</summary>
    private const int ProofSettle = 60;
    private const int ProofSlotsAgainOpen = ProofSettle + 1;
    private const int ProofSlotsAgainShot = ProofSettle + 26;

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

        SaveSystem.Write(0, new SaveData { MapId = "questeria", PlaytimeSeconds = 4320 });
        SaveSystem.Write(SaveSystem.QuickSlot, new SaveData { MapId = "barrow", PlaytimeSeconds = 2840 });
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

            case ProofStage.Creation:
                _proofPending = ProofNewGameFlow();
                break;

            case ProofStage.Voyage:
                _proofPending = RunMap("gulls-wager", ExploreScreen.VoyageProofEnd, ProofStage.Docks);
                break;

            case ProofStage.Docks:
                _proofPending = RunMap("docks", ExploreScreen.DocksProofEnd, ProofStage.Questeria);
                break;

            case ProofStage.Questeria:
                _proofPending = RunMap("questeria", ExploreScreen.QuesteriaProofEnd, ProofStage.Flagon);
                break;

            case ProofStage.Flagon:
                _proofPending = RunMap("gilded-flagon", ExploreScreen.FlagonProofEnd, ProofStage.QuesteriaOut);
                break;

            case ProofStage.QuesteriaOut:
                _proofPending = RunMap("questeria", ExploreScreen.QuesteriaOutProofEnd, ProofStage.Road);
                break;

            case ProofStage.Road:
                _proofPending = RunMap("road", ExploreScreen.RoadProofEnd, ProofStage.QuesteriaAgain);
                break;

            case ProofStage.QuesteriaAgain:
                _proofPending = RunMap("questeria", ExploreScreen.QuesteriaAgainProofEnd, ProofStage.RoadAgain);
                break;

            case ProofStage.RoadAgain:
                _proofPending = RunMap("road", ExploreScreen.RoadAgainProofEnd, ProofStage.Barrow);
                break;

            case ProofStage.Barrow:
                _proofPending = RunMap("barrow", ExploreScreen.BarrowProofEnd, ProofStage.TitleAgain);
                break;

            case ProofStage.TitleAgain:
                _proofPending = RunSubject<TitleScreen>(_ => ProofTitleAgain(), ProofSlotsAgainShot, ProofStage.Done, null);
                break;

            default:
                File.WriteAllText(Path.Combine(ProofDirectory, "audio.log"), _proofAudio.ToString());
                Exit();
                break;
        }
    }

    /// <summary>The title, options over it, the slots both ways (with an erase), then a new game.</summary>
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

            // A new game in the first slot, the way the slots hand one to the creation screen.
            case ProofNewGame:
                CharacterCreationScreen.Begin(_screenManager, 0);
                _proofStage = ProofStage.Creation;
                _proofSubjectStart = _proofFrame;
                return null;

            default:
                return null;
        }
    }

    /// <summary>A new game from the slots to the ship: the Prologue's card, you made (on every tab), what drives you, the Soul-Bound made from the memory, what you remember, and the red moon.</summary>
    /// <remarks>Every step goes through the screens' own proof hooks, so it's the same code a player's keys and clicks reach.</remarks>
    private string ProofNewGameFlow()
    {
        int frame = Frame();
        DialogueScreen talk = Find<DialogueScreen>();
        CharacterCreationScreen creation = Find<CharacterCreationScreen>();

        switch (frame)
        {
            case 40:
                return "prologue-card.png";

            case 41:
                talk?.ProofAdvance();
                return null;

            case 90:
                return "create.png";

            case 91:
                creation?.ProofType("Caleb");
                creation?.ProofLook(new CharacterLook { Skin = 2, Face = 2, Eyes = 2, Hair = 2, HairColour = 3, Shirt = 1, Vest = 3, Legs = 1, LegsColour = 5, Cloak = 2 }, ClassId.Rogue);
                return null;

            case 110:
                return "create-who.png";

            case 111:
                creation?.ProofPage(1);
                return null;

            case 130:
                return "create-face.png";

            case 131:
                creation?.ProofPage(2);
                return null;

            case 150:
                return "create-dress.png";

            case 151:
                creation?.ProofBegin();
                return null;

            // The narrator names you, then asks what drives you: witty.
            case 190:
                talk?.ProofAdvance();
                return null;

            case 191:
                talk?.ProofAdvance();
                return null;

            case 230:
                return "prologue-voice.png";

            case 231:
                talk?.ProofChoose(2);
                return null;

            // The Soul-Bound, remembered: nameless at first, then Wren, a Sylvan, she/her.
            case 290:
                return "create-soulbound.png";

            case 291:
                creation?.ProofType("Wren");
                creation?.ProofPronouns(Pronouns.She);
                creation?.ProofLook(new CharacterLook { Ancestry = Ancestry.Sylvan, Skin = 1, Face = 2, Eyes = 4, Hair = 4, HairColour = 4, Shirt = 2, Vest = 1, LegsColour = 2, Cloak = 3 }, ClassId.Druid);
                return null;

            case 320:
                return "create-soulbound-named.png";

            case 321:
                creation?.ProofBegin();
                return null;

            // The pier, what you remember (that she always wanted more), and the red moon.
            case 360:
                talk?.ProofAdvance();
                return null;

            case 361:
                talk?.ProofAdvance();
                return null;

            case 400:
                return "prologue-memory.png";

            case 401:
                talk?.ProofChoose(1);
                return null;

            case 430:
                talk?.ProofFinishLine();
                return null;

            case 440:
                return "prologue-red-moon.png";

            case 441:
                talk?.ProofAdvance();
                return null;

            case ProofNewGameEnd:
                _proofStage = ProofStage.Voyage;
                _proofSubject = null;
                return null;

            default:
                return null;
        }
    }

    /// <summary>Back on the title: a shot of it, then the load slots, which should have the autosave from the barrow in them.</summary>
    private string ProofTitleAgain()
    {
        switch (Frame())
        {
            case ProofSettle:
                return "title-again.png";

            case ProofSlotsAgainOpen:
                _screenManager.AddScreen(new SaveSlotScreen(SlotPurpose.Load));
                return null;

            case ProofSlotsAgainShot:
                return "slots-autosave.png";

            default:
                return null;
        }
    }

    /// <summary>Runs one visit to a map: waits for that map's screen, then its schedule for this stage.</summary>
    /// <param name="mapId">The map the visit is on.</param>
    /// <param name="end">The last frame of the visit's schedule.</param>
    /// <param name="next">Which part comes next.</param>
    private string RunMap(string mapId, int end, ProofStage next)
    {
        ProofStage stage = _proofStage;
        return RunSubject<ExploreScreen>(s => s.ProofStep(Frame(), stage), end, next, s => s.MapId == mapId);
    }

    /// <summary>The first screen of type T on the stack that isn't on its way out (and passes the test, if there is one), or null.</summary>
    private T Find<T>(System.Func<T, bool> test = null) where T : GameScreen
    {
        foreach (GameScreen screen in _screenManager.GetScreens())
            if (screen is T found && !found.IsExiting && (test is null || test(found))) return found;
        return null;
    }

    /// <summary>Waits for a screen of type T to open, then runs one step of its schedule each frame until it's done.</summary>
    /// <typeparam name="T">The screen to wait for.</typeparam>
    /// <param name="step">One frame of its schedule. Returns the file name to save, or null.</param>
    /// <param name="end">The last frame of its schedule.</param>
    /// <param name="next">Which part comes next.</param>
    /// <param name="test">Which screen of type T it has to be, or null for any. The maps use it, since the last map's screen is still fading out when the next one's stage starts.</param>
    private string RunSubject<T>(System.Func<T, string> step, int end, ProofStage next, System.Func<T, bool> test) where T : GameScreen
    {
        if (_proofSubject is not T subject)
        {
            T found = Find(test);
            if (found is not null)
            {
                _proofSubject = found;
                _proofSubjectStart = _proofFrame;
            }
            return null;
        }

        if (Frame() > end || subject.IsExiting)
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

    /// <summary>Writes the canvas out if a shot was pending. It's the frame at the canvas's own size, whatever size the window is.</summary>
    private void EndProofCapture()
    {
        _proofDrawn = true;
        if (_proofPending is null) return;

        GraphicsDevice.SetRenderTarget(null);

        Directory.CreateDirectory(ProofDirectory);
        using (FileStream file = File.Create(Path.Combine(ProofDirectory, _proofPending)))
            _canvas.Target.SaveAsPng(file, ScreenWidth, ScreenHeight);

        _proofPending = null;
    }
}
