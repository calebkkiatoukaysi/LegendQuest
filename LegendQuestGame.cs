using Microsoft.Xna.Framework;
using LegendQuest.Audio;
using LegendQuest.Components;
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

    /// <summary>Sets up the window and adds the sound and the screen manager as components and services.</summary>
    public LegendQuestGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();
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
    }

    /// <summary>Reads every map, loads all the sounds, then opens the gate and the title.</summary>
    /// <remarks>
    /// The first screens get added here instead of the constructor because the gate starts the title
    /// music when it opens. The maps are read first, because reading a save checks its map exists,
    /// and because a broken map file should stop the game here, not when somebody walks onto it.
    /// </remarks>
    protected override void LoadContent()
    {
        MapCatalog.LoadAll();

        if (ProofDirectory is not null) BeginProof();

        _audio.LoadContent(Content);
        _cursor.LoadContent(Content);

        _screenManager.AddScreen(new TitleBackgroundScreen());
        _screenManager.AddScreen(new TitleScreen());
    }

    /// <summary>Moves the proof run along if there is one, then updates the components (which is the screens and the sound).</summary>
    /// <param name="gameTime">The frame's timing.</param>
    protected override void Update(GameTime gameTime)
    {
        UpdateProof();
        base.Update(gameTime);
    }

    /// <summary>Clears to the void and draws the screens and then the cursor, into the proof's render target when a shot is due.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    protected override void Draw(GameTime gameTime)
    {
        BeginProofCapture();
        GraphicsDevice.Clear(VoidColor);

        base.Draw(gameTime);
        _cursor.Draw(gameTime, _screenManager.SpriteBatch, _screenManager.Input.MousePosition);

        EndProofCapture();
    }
}
