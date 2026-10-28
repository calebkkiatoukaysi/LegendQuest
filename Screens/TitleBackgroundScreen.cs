using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Audio;
using LegendQuest.Particles;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// Questeria across the lake at twilight, behind the title and every form on it. The layers sway
/// against each other, clouds drift, two islands float (one with a waterfall falling off it), a
/// banner each side of the terrace waves, a tower's crystal pulses, petals come off the trees and
/// motes of light rise off the water. This is the sample's BackgroundScreen.
/// </summary>
/// <remarks>
/// The layers are like the parallax tutorial's, except nothing's scrolling: the whole view sways
/// slowly from side to side, and each layer moves by its own share of that, the sky barely and the
/// terrace the most, so it reads as near and far. Every picture is cut from the ones I made with
/// ChatGPT by tools/cut_supplied.py, to art pixels drawn at 4x. Like The Black Box's box screen it
/// only transitions off when it's told to leave, not when something covers it, so the scene stays
/// up behind the options and the save slots.
/// </remarks>
public class TitleBackgroundScreen : GameScreen
{
    /// <summary>How much the art is blown up. Every picture on the title is the same, so all its pixels are the same size.</summary>
    private const float Scale = 4f;

    /// <summary>The layers' canvas, in art pixels, and how much of it is off the top of the window. Has to match TITLE_W and TITLE_H in tools/cut_supplied.py.</summary>
    /// <remarks>It's 16 wider than the window so it can sway 8 either way. The top 6 rows go so the castle's spires just fit.</remarks>
    private const int CanvasWidth = 416;
    private const int CanvasHeight = 277;
    private const int HiddenTop = 6;

    /// <summary>How far the nearest layer sways either way, in art pixels, and how long one sway takes, in seconds.</summary>
    private const float SwayReach = 8f;
    private const float SwaySeconds = 26f;

    /// <summary>How much of the sway each layer takes: the sky the least, the terrace all of it.</summary>
    private const float SkyDepth = 0.12f;
    private const float FarDepth = 0.35f;
    private const float CityDepth = 0.55f;
    private const float NearDepth = 1f;

    /// <summary>The clouds' cells on clouds.png. Has to match CLOUD_W and CLOUD_H in tools/cut_supplied.py.</summary>
    private static readonly Point CloudCell = new(132, 60);

    /// <summary>The clouds that drift across: which cell, how far down the canvas, and how fast (art pixels a second, leftwards).</summary>
    private static readonly (int Cell, float Y, float Speed)[] Clouds =
    {
        (0, 4f, 3f), (4, 40f, 4.5f), (2, 58f, 2.2f), (5, 18f, 2.8f),
    };

    /// <summary>Where the floating island floats: the top left of it, on the canvas. Under the title on the left, over the mountains.</summary>
    private static readonly Vector2 IslandAt = new(114f, 60f);

    /// <summary>How far the islands bob up and down, in art pixels, and how long one bob takes.</summary>
    private const float BobReach = 1.5f;
    private const float BobSeconds = 7f;

    /// <summary>The three loops: their frames' sizes, how many there are, seconds a frame, and where they go on the canvas (the top left of a frame). Has to match TITLE_LOOPS.</summary>
    private static readonly Point WaterfallFrame = new(34, 69);
    private const int WaterfallFrames = 6;
    private const float WaterfallFrameTime = 0.1f;
    private static readonly Vector2 WaterfallAt = new(232f, 50f);

    private static readonly Point SpireFrame = new(30, 74);
    private const int SpireFrames = 8;
    private const float SpireFrameTime = 0.26f;
    private static readonly Vector2 SpireAt = new(54f, 82f);

    private static readonly Point BannerFrame = new(34, 66);
    private const int BannerFrames = 8;
    private const float BannerFrameTime = 0.12f;
    private static readonly Vector2[] BannersAt = { new(109f, 150f), new(267f, 150f) };

    /// <summary>The twinkling stars: how many, the part of the canvas they're in, what scatters them there (so they're always in the same places), how fast they twinkle, their colour, and how big they get (mote.png is 8 pixels across).</summary>
    private const int Stars = 18;
    private static readonly Rectangle StarField = new(104, 8, 200, 64);
    private const int StarSeed = 580;
    private const float TwinkleRate = 1.6f;
    private static readonly Color StarColor = new(220, 226, 255);
    private const float StarScale = 0.9f;

    /// <summary>Where the petals come off (the two blossom trees) and the motes rise from (the lake), on the canvas.</summary>
    private static readonly Rectangle[] Blossom = { new(0, 6, 124, 80), new(326, 6, 90, 80) };
    private static readonly Rectangle Lake = new(70, 168, 280, 44);

    /// <summary>How many petals and motes can be about at once, and how long between one lot and the next (seconds).</summary>
    private const int MaxPetals = 60;
    private const int MaxSparkles = 50;
    private const float PetalGap = 0.35f;
    private const float SparkleGap = 0.22f;

    /// <summary>How long the scene takes to fade in and out.</summary>
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.8);

    private readonly PetalParticleSystem _petals = new(MaxPetals);
    private readonly SparkleParticleSystem _sparkles = new(MaxSparkles);
    private readonly Vector2[] _stars = new Vector2[Stars];
    private readonly float[] _starPhases = new float[Stars];
    private readonly float[] _cloudX = new float[Clouds.Length];

    private ContentManager _content;
    private Texture2D _sky;
    private Texture2D _city;
    private Texture2D _foreground;
    private Texture2D _clouds;
    private Texture2D _island;
    private Texture2D _waterfall;
    private Texture2D _spire;
    private Texture2D _banner;
    private Texture2D _mote;

    private float _clock;
    private float _petalClock;
    private float _sparkleClock;

    /// <summary>Scatters the stars and the clouds, and sets the fades.</summary>
    /// <remarks>Seeded, so the stars are in the same places every time.</remarks>
    public TitleBackgroundScreen()
    {
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        var random = new Random(StarSeed);
        for (int i = 0; i < Stars; i++)
        {
            _stars[i] = new Vector2(random.Next(StarField.Left, StarField.Right), random.Next(StarField.Top, StarField.Bottom));
            _starPhases[i] = (float)random.NextDouble() * MathHelper.TwoPi;
        }
        for (int i = 0; i < Clouds.Length; i++) _cloudX[i] = i * CanvasWidth / (float)Clouds.Length;
    }

    /// <summary>Loads every layer and loop, the petals and the motes, and starts the title music.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");

        _sky = _content.Load<Texture2D>("Title/sky");
        _city = _content.Load<Texture2D>("Title/city");
        _foreground = _content.Load<Texture2D>("Title/foreground");
        _clouds = _content.Load<Texture2D>("Title/clouds");
        _island = _content.Load<Texture2D>("Title/island");
        _waterfall = _content.Load<Texture2D>("Title/waterfall");
        _spire = _content.Load<Texture2D>("Title/spire");
        _banner = _content.Load<Texture2D>("Title/banner");
        _mote = _content.Load<Texture2D>("mote");
        _petals.LoadContent(_content);
        _sparkles.LoadContent(_content);

        ScreenManager.Game.Services.GetService<AudioManager>().PlaySong(Track.Title);
    }

    /// <summary>Unloads everything.</summary>
    public override void Unload() => _content?.Unload();

    /// <summary>Runs the clock, the clouds, the petals and the motes. Never counts as covered, so it stays drawn under the menus.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Ignored on purpose (see the class remarks).</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, false);

        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _clock += elapsed;

        // Off the left edge, a cloud comes back in on the right.
        for (int i = 0; i < Clouds.Length; i++)
        {
            _cloudX[i] -= Clouds[i].Speed * elapsed;
            if (_cloudX[i] < -CloudCell.X) _cloudX[i] += CanvasWidth + CloudCell.X;
        }

        _petalClock -= elapsed;
        if (_petalClock <= 0f)
        {
            _petals.Fall(OnScreen(Blossom[RandomHelper.Next(0, Blossom.Length)], NearDepth));
            _petalClock = PetalGap;
        }

        _sparkleClock -= elapsed;
        if (_sparkleClock <= 0f)
        {
            _sparkles.Rise(OnScreen(Lake, CityDepth));
            _sparkleClock = SparkleGap;
        }

        _petals.Update(gameTime);
        _sparkles.Update(gameTime);
    }

    /// <summary>Draws the scene back to front, the stars over the sky, then the petals and the motes.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        // 1. The scene, in the order it's drawn (deferred, not sorted): the sky, what's in it, the
        //    city, the tower, the terrace and the banners on it. Point sampling keeps the pixels square.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawLayer(spriteBatch, _sky, SkyDepth);
        for (int i = 0; i < Clouds.Length; i++)
        {
            var cell = new Rectangle(Clouds[i].Cell * CloudCell.X, 0, CloudCell.X, CloudCell.Y);
            DrawArt(spriteBatch, _clouds, cell, new Vector2(_cloudX[i], Clouds[i].Y), SkyDepth);
        }
        // The two islands bob out of step, so they don't look tied together.
        DrawArt(spriteBatch, _island, _island.Bounds, IslandAt + new Vector2(0f, Bob(0f)), FarDepth);
        DrawArt(spriteBatch, _waterfall, Frame(WaterfallFrame, WaterfallFrames, WaterfallFrameTime, 0),
            WaterfallAt + new Vector2(0f, Bob(0.4f)), FarDepth);
        DrawLayer(spriteBatch, _city, CityDepth);
        DrawArt(spriteBatch, _spire, Frame(SpireFrame, SpireFrames, SpireFrameTime, 0), SpireAt, CityDepth);
        DrawLayer(spriteBatch, _foreground, NearDepth);
        for (int i = 0; i < BannersAt.Length; i++)
            DrawArt(spriteBatch, _banner, Frame(BannerFrame, BannerFrames, BannerFrameTime, i * BannerFrames / 2), BannersAt[i], NearDepth);
        spriteBatch.End();

        // 2. The stars twinkling, additive and soft, where the sky's open.
        spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp);
        for (int i = 0; i < Stars; i++)
        {
            float twinkle = 0.5f + 0.5f * MathF.Sin(_clock * TwinkleRate + _starPhases[i]);
            Vector2 at = ToScreen(_stars[i], SkyDepth);
            spriteBatch.Draw(_mote, at, null, StarColor * (twinkle * twinkle * twinkle), 0f,
                new Vector2(_mote.Width / 2f), StarScale * (0.5f + 0.5f * twinkle), SpriteEffects.None, 0f);
        }
        spriteBatch.End();

        // 3. The petals in front of everything, and the motes of light over that.
        _petals.Draw(spriteBatch, Matrix.Identity);
        _sparkles.Draw(spriteBatch, Matrix.Identity);

        ScreenManager.FadeBackBufferToBlack(TransitionPosition);
    }

    /// <summary>How far up or down an island is right now.</summary>
    /// <param name="phase">How far through its bob it starts, 0 to 1.</param>
    private float Bob(float phase) => MathF.Sin((_clock / BobSeconds + phase) * MathHelper.TwoPi) * BobReach;

    /// <summary>Draws a whole layer of the canvas, swayed by its share.</summary>
    private void DrawLayer(SpriteBatch spriteBatch, Texture2D layer, float depth) =>
        DrawArt(spriteBatch, layer, new Rectangle(0, 0, CanvasWidth, CanvasHeight), Vector2.Zero, depth);

    /// <summary>Draws part of a picture with its top left at a point on the canvas, swayed by its layer's share.</summary>
    private void DrawArt(SpriteBatch spriteBatch, Texture2D texture, Rectangle source, Vector2 canvas, float depth)
    {
        Vector2 at = ToScreen(canvas, depth);
        spriteBatch.Draw(texture, new Vector2(MathF.Round(at.X), MathF.Round(at.Y)), source, Color.White,
            0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
    }

    /// <summary>Where a point on the canvas is on the screen right now, for a layer that takes `depth` of the sway.</summary>
    /// <remarks>The canvas's middle is the window's middle, and the top HiddenTop rows are off the top.</remarks>
    private Vector2 ToScreen(Vector2 canvas, float depth)
    {
        float sway = MathF.Sin(_clock / SwaySeconds * MathHelper.TwoPi) * SwayReach * depth;
        float left = (LegendQuestGame.ScreenWidth / Scale - CanvasWidth) / 2f + sway;
        return new Vector2((canvas.X + left) * Scale, (canvas.Y - HiddenTop) * Scale);
    }

    /// <summary>A part of the canvas on the screen right now, for spawning particles in.</summary>
    private Rectangle OnScreen(Rectangle canvas, float depth)
    {
        Vector2 at = ToScreen(canvas.Location.ToVector2(), depth);
        return new Rectangle((int)at.X, (int)at.Y, (int)(canvas.Width * Scale), (int)(canvas.Height * Scale));
    }

    /// <summary>Which frame of a loop is showing, as a source rectangle.</summary>
    /// <param name="size">One frame's size.</param>
    /// <param name="frames">How many frames along the strip.</param>
    /// <param name="frameTime">Seconds a frame.</param>
    /// <param name="offset">How many frames ahead of the clock it is, so two of the same thing aren't in step.</param>
    private Rectangle Frame(Point size, int frames, float frameTime, int offset)
    {
        int frame = ((int)(_clock / frameTime) + offset) % frames;
        return new Rectangle(frame * size.X, 0, size.X, size.Y);
    }
}
