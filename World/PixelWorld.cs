using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.World;

/// <summary>
/// The pixel look's world: a small picture, ArtSettings.ArtWidth by ArtHeight art pixels, that a
/// room and everyone in it are drawn onto and that's then blown up a whole number of times onto
/// the screen with hard edges, so every art pixel is a clean square whatever turned or stretched
/// to make it (docs/pixel-art-brief.md §1). The HUD, the writing and the name plates are drawn on
/// the screen afterwards, at its own resolution.
/// </summary>
/// <remarks>
/// A screen draws its world between Begin and End with every batch's matrix ended by Lens (or, for
/// a batch that's in screen pixels, by Screen), which shrinks the screen's pixels to art pixels.
/// The camera has to be on whole art pixels too: Camera2D.Grain.
/// </remarks>
public sealed class PixelWorld : IDisposable
{
    private RenderTarget2D _target;
    private RenderTargetBinding[] _screen;

    /// <summary>What a batch in screen pixels is ended by to land on the art pixels.</summary>
    public static Matrix Screen { get; } = Matrix.CreateScale(1f / ArtSettings.ArtScale, 1f / ArtSettings.ArtScale, 1f);

    /// <summary>What goes after the camera's matrix in place of the shake: the shake moved to a whole art pixel, then the screen's pixels shrunk to art pixels.</summary>
    /// <param name="shake">The shake on the screen, in screen pixels.</param>
    public static Matrix Lens(Matrix shake)
    {
        Vector3 by = shake.Translation;
        shake.Translation = new Vector3(MathF.Round(by.X / ArtSettings.ArtScale) * ArtSettings.ArtScale, MathF.Round(by.Y / ArtSettings.ArtScale) * ArtSettings.ArtScale, by.Z);
        return shake * Screen;
    }

    /// <summary>Points what's drawn at the art-pixel picture, cleared.</summary>
    /// <param name="device">The graphics device.</param>
    public void Begin(GraphicsDevice device)
    {
        _target ??= new RenderTarget2D(device, ArtSettings.ArtWidth, ArtSettings.ArtHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        _screen = device.GetRenderTargets();
        device.SetRenderTarget(_target);
        device.Clear(Color.Transparent);
    }

    /// <summary>Goes back to the screen and blows the art-pixel picture up onto it, point sampled.</summary>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    public void End(SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.SetRenderTargets(_screen);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        spriteBatch.Draw(_target, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight), Color.White);
        spriteBatch.End();
    }

    /// <summary>Lets the picture go.</summary>
    public void Dispose()
    {
        _target?.Dispose();
        _target = null;
    }
}
