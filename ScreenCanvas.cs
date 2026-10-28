using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest;

/// <summary>
/// The canvas every screen draws on, always ScreenWidth by ScreenHeight, and how it gets onto a
/// window that's whatever size the screen it's on wants.
/// </summary>
/// <remarks>
/// The game was drawn straight onto a 1600x900 window, which is about two thirds of a laptop screen
/// at 200% scaling, so everything came out small. Now the screens draw onto this, and it's blown up
/// to fill the window, keeping its shape (black bars if the window's a different shape). A whole
/// number of times bigger is just point sampled. In between, it's blown up point sampled to the next
/// whole number first and then shrunk the rest of the way smoothly, so a pixel never comes out two
/// screen pixels wide next to one three wide, and the edges only blur by a pixel. The mouse gets
/// mapped back onto the canvas, so no screen knows the window isn't 1600x900.
/// </remarks>
public class ScreenCanvas
{
    /// <summary>How much of the screen a new window takes, at most, each way. The rest is for the title bar and the taskbar.</summary>
    private const float WindowShare = 0.88f;

    private readonly Game _game;
    private readonly GraphicsDeviceManager _graphics;
    private RenderTarget2D _canvas;
    private RenderTarget2D _sharpened;
    private SpriteBatch _spriteBatch;
    private bool _resizing;

    /// <summary>Where the canvas lands in the window, in window pixels.</summary>
    public Rectangle Shown { get; private set; } = new(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight);

    /// <summary>The canvas itself, for the proof run to save.</summary>
    public RenderTarget2D Target => _canvas;

    /// <summary>Whether the game's taking up the whole screen.</summary>
    public bool FullScreen => _graphics.IsFullScreen;

    /// <summary>Remembers the game and its graphics.</summary>
    /// <param name="game">The game.</param>
    /// <param name="graphics">Its graphics device manager.</param>
    public ScreenCanvas(Game game, GraphicsDeviceManager graphics)
    {
        _game = game;
        _graphics = graphics;
    }

    /// <summary>Sizes the window for the screen it's on, full screen or not, and lets it be dragged to any size.</summary>
    /// <param name="fullScreen">Whether to start full screen.</param>
    public void SetUp(bool fullScreen)
    {
        _game.Window.AllowUserResizing = true;
        _game.Window.ClientSizeChanged += Resized;

        // Borderless, so switching doesn't change the screen's own mode and blink everything.
        _graphics.HardwareModeSwitch = false;
        SetFullScreen(fullScreen);
    }

    /// <summary>Makes the canvas and the batch that blows it up.</summary>
    /// <param name="device">The graphics device.</param>
    public void LoadContent(GraphicsDevice device)
    {
        _canvas = new RenderTarget2D(device, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight);
        _spriteBatch = new SpriteBatch(device);
    }

    /// <summary>Goes full screen, or back to a window as big as fits.</summary>
    /// <param name="on">Which.</param>
    public void SetFullScreen(bool on)
    {
        DisplayMode screen = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        Point size = on ? new Point(screen.Width, screen.Height) : Fitted(screen);

        _resizing = true;
        _graphics.IsFullScreen = on;
        _graphics.PreferredBackBufferWidth = size.X;
        _graphics.PreferredBackBufferHeight = size.Y;
        _graphics.ApplyChanges();
        _resizing = false;

        Place();
    }

    /// <summary>Points everything that's drawn at the canvas. The first thing every frame.</summary>
    /// <param name="device">The graphics device.</param>
    public void Begin(GraphicsDevice device) => device.SetRenderTarget(_canvas);

    /// <summary>Blows the canvas up onto the window. The last thing every frame.</summary>
    /// <param name="device">The graphics device.</param>
    public void Present(GraphicsDevice device)
    {
        Rectangle shown = Shown;
        float scale = shown.Width / (float)LegendQuestGame.ScreenWidth;
        int whole = (int)MathF.Ceiling(scale - 0.001f);
        Texture2D picture = _canvas;

        // Not a whole number of times bigger: blow it up to the next whole number first.
        if (scale > 1f && MathF.Abs(scale - MathF.Round(scale)) > 0.001f)
        {
            Point big = new(LegendQuestGame.ScreenWidth * whole, LegendQuestGame.ScreenHeight * whole);
            if (_sharpened is null || _sharpened.Width != big.X)
            {
                _sharpened?.Dispose();
                _sharpened = new RenderTarget2D(device, big.X, big.Y);
            }
            device.SetRenderTarget(_sharpened);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_canvas, new Rectangle(0, 0, big.X, big.Y), Color.White);
            _spriteBatch.End();
            picture = _sharpened;
        }

        device.SetRenderTarget(null);
        device.Clear(Color.Black);
        bool exact = MathF.Abs(scale - MathF.Round(scale)) <= 0.001f && scale >= 1f;
        _spriteBatch.Begin(samplerState: exact ? SamplerState.PointClamp : SamplerState.LinearClamp);
        _spriteBatch.Draw(picture, shown, Color.White);
        _spriteBatch.End();
    }

    /// <summary>A window position on the canvas.</summary>
    /// <param name="window">Where, in window pixels.</param>
    /// <returns>Where that is on the canvas, in canvas pixels.</returns>
    public Point ToCanvas(Point window)
    {
        Rectangle shown = Shown;
        return new Point(
            (int)MathF.Floor((window.X - shown.X) * (float)LegendQuestGame.ScreenWidth / shown.Width),
            (int)MathF.Floor((window.Y - shown.Y) * (float)LegendQuestGame.ScreenHeight / shown.Height));
    }

    /// <summary>The biggest window with the canvas's shape that fits on a screen with room to spare.</summary>
    private static Point Fitted(DisplayMode screen)
    {
        float scale = MathF.Min(screen.Width * WindowShare / LegendQuestGame.ScreenWidth,
            screen.Height * WindowShare / LegendQuestGame.ScreenHeight);
        scale = MathF.Max(scale, 0.5f);
        return new Point((int)(LegendQuestGame.ScreenWidth * scale), (int)(LegendQuestGame.ScreenHeight * scale));
    }

    /// <summary>When the window's been dragged to a new size: makes the back buffer match it.</summary>
    private void Resized(object sender, EventArgs e)
    {
        if (_resizing || _graphics.IsFullScreen) return;

        Rectangle client = _game.Window.ClientBounds;
        if (client.Width <= 0 || client.Height <= 0) return;

        _resizing = true;
        _graphics.PreferredBackBufferWidth = client.Width;
        _graphics.PreferredBackBufferHeight = client.Height;
        _graphics.ApplyChanges();
        _resizing = false;

        Place();
    }

    /// <summary>Works out where the canvas goes in the window: as big as fits, in the middle.</summary>
    private void Place()
    {
        int width = _graphics.PreferredBackBufferWidth, height = _graphics.PreferredBackBufferHeight;
        float scale = MathF.Min(width / (float)LegendQuestGame.ScreenWidth, height / (float)LegendQuestGame.ScreenHeight);
        int w = (int)MathF.Round(LegendQuestGame.ScreenWidth * scale), h = (int)MathF.Round(LegendQuestGame.ScreenHeight * scale);
        Shown = new Rectangle((width - w) / 2, (height - h) / 2, w, h);
    }
}
