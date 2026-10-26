using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Audio;
using LegendQuest.Components;
using LegendQuest.Particles;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// The gate at night behind the title and every form on it: two torches flickering, and embers
/// going up off them. This is the sample's BackgroundScreen.
/// </summary>
/// <remarks>
/// Like The Black Box's box screen it only transitions off when it's told to leave, not when
/// something covers it, so the gate stays up behind the options and the save slots.
/// </remarks>
public class TitleBackgroundScreen : GameScreen
{
    /// <summary>How much the gate picture is blown up. It's 400x225 art pixels, so 4x fills the window.</summary>
    private const float SceneScale = 4f;

    /// <summary>Where the two flames sit on the gate picture, in art pixels. Has to match TITLE_TORCHES in tools/generate_world.py.</summary>
    private static readonly Vector2[] TorchesAt = { new(118f, 99f), new(282f, 99f) };

    /// <summary>How far the flickering light reaches round each torch, and its colour at full strength.</summary>
    /// <remarks>Kept faint. The pool is already painted into the picture, this is just the part that moves.</remarks>
    private const float GlowRadius = 190f;
    private static readonly Color GlowColor = new Color(255, 150, 70) * 0.2f;

    /// <summary>How many embers can be in the air at once, and how long between puffs off one torch (seconds).</summary>
    private const int MaxEmbers = 120;
    private const float PuffGapMin = 0.06f;
    private const float PuffGapMax = 0.3f;

    /// <summary>How long the gate takes to fade in and out.</summary>
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.8);

    private readonly TorchSprite[] _torches = new TorchSprite[TorchesAt.Length];
    private readonly float[] _puffClocks = new float[TorchesAt.Length];
    private readonly EmberParticleSystem _embers = new(MaxEmbers);

    private ContentManager _content;
    private Texture2D _scene;

    /// <summary>Puts a torch in each sconce and sets the fades.</summary>
    public TitleBackgroundScreen()
    {
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        for (int i = 0; i < TorchesAt.Length; i++)
            _torches[i] = new TorchSprite(TorchesAt[i] * SceneScale, SceneScale);
    }

    /// <summary>Loads the gate, the torches and the embers, and starts the title music.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");

        _scene = _content.Load<Texture2D>("title-scene");
        foreach (TorchSprite torch in _torches) torch.LoadContent(_content);
        _embers.LoadContent(_content);

        ScreenManager.Game.Services.GetService<AudioManager>().PlaySong(Track.Title);
    }

    /// <summary>Unloads the gate.</summary>
    public override void Unload() => _content?.Unload();

    /// <summary>Runs the fades, the torches and the embers. Never counts as covered, so it stays drawn under the menus.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Ignored on purpose (see the class remarks).</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, false);

        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = 0; i < _torches.Length; i++)
        {
            _torches[i].Update(gameTime);

            _puffClocks[i] -= elapsed;
            if (_puffClocks[i] > 0f) continue;

            _embers.Puff(_torches[i].Tip);
            _puffClocks[i] = RandomHelper.NextFloat(PuffGapMin, PuffGapMax);
        }

        _embers.Update(gameTime);
    }

    /// <summary>Draws the gate and the flames, the light over them, then the embers. Three batches.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        // 1. The gate and the flames. Point sampling keeps the pixel art crisp.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        spriteBatch.Draw(_scene, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, SceneScale, SpriteEffects.None, Layers.TitleScene);
        foreach (TorchSprite torch in _torches) torch.Draw(spriteBatch, Layers.Flame);
        spriteBatch.End();

        // 2. The flickering light, additive so it brightens the stone under it. Linear sampling keeps it soft.
        spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp);
        foreach (TorchSprite torch in _torches) torch.DrawGlow(spriteBatch, GlowRadius, GlowColor);
        spriteBatch.End();

        // 3. The embers, in front of everything.
        _embers.Draw(spriteBatch, Matrix.Identity);

        ScreenManager.FadeBackBufferToBlack(TransitionPosition);
    }
}
