using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Items;
using LegendQuest.Save;
using LegendQuest.Screens;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest;

/// <summary>
/// Legend Quest. Sets up the window and the two things every screen shares (the screen manager and
/// the sound), then opens the title.
/// </summary>
/// <remarks>
/// Kept to setup only, the same as The Black Box ended up. Every screen is its own GameScreen on a
/// ScreenManager like the game state management tutorial.
/// </remarks>
public partial class LegendQuestGame : Game
{
    internal const int ScreenWidth = 1600;
    internal const int ScreenHeight = 900;

    /// <summary>What's behind everything. Not quite black, so the darkest stone still reads as stone.</summary>
    private static readonly Color VoidColor = new(8, 7, 9);

    private readonly GraphicsDeviceManager _graphics;
    private readonly ScreenManager _screenManager;
    private readonly AudioManager _audio;
    private readonly CursorSprite _cursor = new();
    private readonly ScreenCanvas _canvas;

    /// <summary>Sets up the window and adds the sound and the screen manager as components and services.</summary>
    public LegendQuestGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();
        _canvas = new ScreenCanvas(this, _graphics);
        Content.RootDirectory = "Content";
        Window.Title = "Legend Quest";

        // The game draws its own cursor, so it can change shape.
        IsMouseVisible = false;

        // The sound and the screens are components so they keep running no matter what screen is up,
        // and services so any screen can get to them (like the achievement service in the services tutorial).
        _audio = new AudioManager(this, SettingsStore.Load());
        Components.Add(_audio);
        Services.AddService(_audio);

        _screenManager = new ScreenManager(this);
        Components.Add(_screenManager);
        Services.AddService(_screenManager);

        // The cursor isn't a component, since it has to be drawn after the screens and nothing else
        // about it needs a component. It's a service so the screens can change its shape.
        Services.AddService(_cursor);

        // The options switch full screen on and off through the canvas, and the mouse gets mapped
        // from the window onto it.
        Services.AddService(_canvas);
        _screenManager.Input.MouseMap = _canvas.ToCanvas;
    }

    /// <summary>Sizes the window for the screen it's on, now the switches are in.</summary>
    /// <remarks>Not in the constructor, since ProofDirectory is set after it. The proof run gets a window whatever the settings say.</remarks>
    protected override void Initialize()
    {
        _canvas.SetUp(ProofDirectory is null && _audio.Settings.FullScreen);
        base.Initialize();
    }

    /// <summary>Reads every map and every zone, and checks every table that's read from them: the conversations, the shops and the loot, and the imported characters' pictures.</summary>
    /// <remarks>
    /// Throws on the first fault, with what and where. The game does it as it starts, and --validate
    /// does it alone, with no window, so the data can be checked in a second instead of a proof run.
    /// Nothing else reads the shops or the loot tables until a shop opens or a fight's won, so a bad
    /// row in one would otherwise only show up then; reading them checks them now. The zones are read
    /// before the conversations, which check their travels against them, and the rest of the zones'
    /// checks (talks, barks, every fight's foes together) wait for the conversations. Then who's drawn
    /// for each fight's supports, which is often someone standing in a zone, and that every room a
    /// fight's fought in has room on its floor for the fight's slots. Last, every imported picture's
    /// .json, which has to be for someone the tables know, and the character parts' parts.json.
    /// </remarks>
    public static void LoadAndCheckData()
    {
        ZoneCatalog.LoadAll();
        DialogueCatalog.ValidateAll();
        ZoneCatalog.ValidateAfterDialogue();
        EncounterCatalog.ValidateAfterZones();
        foreach (RoomDefinition room in EncounterCatalog.FightRooms()) FightStage.Check(room);
        _ = ShopCatalog.All.Count() + LootTableCatalog.All.Count();
        ImportedArt.LoadAll();
        PartCatalog.LoadAll();
    }

    /// <summary>Reads every map, makes the imported characters' pictures ready, loads all the sounds, then opens the gate and the title.</summary>
    /// <remarks>
    /// The first screens get added here instead of the constructor because the gate starts the title
    /// music when it opens. The maps are read first, because reading a save checks its map exists,
    /// and because a broken map file should stop the game here, not when somebody walks onto it.
    /// </remarks>
    protected override void LoadContent()
    {
        LoadAndCheckData();

        if (ProofDirectory is not null)
        {
            PartCatalog.Synchronous = true;
            BeginProof();
        }

        ImportedArt.LoadContent(Services, GraphicsDevice);
        PartCatalog.LoadContent(Services, GraphicsDevice);
        RegisterCrowds();
        PartCatalog.Prewarm();
        _audio.LoadContent(Content);
        _cursor.LoadContent(Content);
        _canvas.LoadContent(GraphicsDevice);

        _screenManager.AddScreen(new TitleBackgroundScreen());
        _screenManager.AddScreen(new TitleScreen());
    }

    /// <summary>
    /// Gives everyone in the rooms and the cast who has no recipe of their own a look from the shared
    /// parts (PartCatalog.Crowd), by the id the rooms ask for their picture by: so nobody is drawn as
    /// the old pixel figures once the painted parts are there. Creatures keep their own pictures.
    /// </summary>
    private static void RegisterCrowds()
    {
        foreach (Story.CastMember member in Story.Cast.All) PartCatalog.Crowd(member.Id, member.Name, member.Look?.Ancestry);
        foreach (ZoneDefinition zone in ZoneCatalog.All)
        {
            foreach (RoomDefinition room in zone.Rooms)
            {
                foreach (MapNpc npc in room.Npcs)
                {
                    if (npc.Creature is not null || npc.Cast is not null) continue;
                    PartCatalog.Crowd(npc.Look is not null ? npc.Id : npc.Sheet, npc.Name, npc.Look?.Ancestry);
                }
            }
        }
    }

    /// <summary>Moves the proof run along if there is one, then updates the components (which is the screens and the sound), then F11 for full screen.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    protected override void Update(GameTime gameTime)
    {
        StartFrameTime();
        UpdateProof();
        PartCatalog.Update();
        base.Update(gameTime);

        if (_screenManager.Input.IsNewKeyPress(Keys.F11))
        {
            _canvas.SetFullScreen(!_canvas.FullScreen);
            _audio.Settings.FullScreen = _canvas.FullScreen;
            SettingsStore.Save(_audio.Settings);
        }
    }

    /// <summary>Clears the canvas to the void, draws the screens and then the cursor on it, saves it if a shot is due, and puts it in the window.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    protected override void Draw(GameTime gameTime)
    {
        _canvas.Begin(GraphicsDevice);
        GraphicsDevice.Clear(VoidColor);

        base.Draw(gameTime);
        _cursor.Draw(gameTime, _screenManager.SpriteBatch, _screenManager.Input.MousePosition);

        EndFrameTime();
        EndProofCapture();
        _canvas.Present(GraphicsDevice);
    }
}
