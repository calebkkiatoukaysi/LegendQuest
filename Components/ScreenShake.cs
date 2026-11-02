using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Components;

/// <summary>
/// The jolt when a blow lands in a fight. Its Transform goes after the camera's in SpriteBatch.Begin,
/// so the whole map shakes and pushes in a little toward the hit, then settles.
/// </summary>
/// <remarks>
/// The Black Box's ScreenShake, smaller: a map is a busy picture and a turn has a lot of hits in it,
/// so a hit is a nudge and a critical a jolt, never enough to lose your place. A hit adds "trauma"
/// (0 to 1) and time takes it away; the shake is trauma squared, so it fades out instead of stopping
/// dead. The push in is there so the shake never shows past the edge of the map.
/// </remarks>
public class ScreenShake
{
    /// <summary>The furthest the picture moves either way at full trauma, in screen pixels.</summary>
    /// <remarks>Less up and down than sideways, since the screen is wider than it is tall.</remarks>
    private const float MaxShiftX = 9f;
    private const float MaxShiftY = 5f;

    /// <summary>How much bigger everything gets at full trauma. 0.02 is 2%.</summary>
    private const float MaxZoom = 0.02f;

    /// <summary>How much trauma goes away a second: the worst hit is over in a little over half a second.</summary>
    private const float Recovery = 1.8f;

    /// <summary>How fast it wobbles, in radians a second. Each axis uses a different multiple so it never goes round in a circle.</summary>
    private const float Wobble = 47f;

    private float _trauma;
    private float _clock;
    private Vector2 _focus;

    /// <summary>The matrix to multiply after the camera's. Identity when nothing is shaking.</summary>
    public Matrix Transform { get; private set; } = Matrix.Identity;

    /// <summary>Adds a hit's worth of shake, pushing in toward where it landed.</summary>
    /// <remarks>The focus is kept far enough in from the edges that the push in always covers the shift (MaxShift / MaxZoom from each side).</remarks>
    /// <param name="amount">How hard, from 0 to 1. It adds up, up to 1.</param>
    /// <param name="focus">Where the hit was, in screen pixels.</param>
    public void Kick(float amount, Vector2 focus)
    {
        _trauma = MathHelper.Clamp(_trauma + amount, 0f, 1f);

        const float insetX = MaxShiftX / MaxZoom, insetY = MaxShiftY / MaxZoom;
        _focus = new Vector2(
            MathHelper.Clamp(focus.X, insetX, LegendQuestGame.ScreenWidth - insetX),
            MathHelper.Clamp(focus.Y, insetY, LegendQuestGame.ScreenHeight - insetY));
    }

    /// <summary>Lets the shake die down and works out this frame's matrix.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _clock += elapsed;
        _trauma = MathF.Max(0f, _trauma - Recovery * elapsed);

        if (_trauma <= 0f)
        {
            Transform = Matrix.Identity;
            return;
        }

        float shake = _trauma * _trauma;
        float x = MathF.Round(MaxShiftX * shake * MathF.Sin(_clock * Wobble) * MathF.Cos(_clock * Wobble * 0.31f));
        float y = MathF.Round(MaxShiftY * shake * MathF.Sin(_clock * Wobble * 1.37f + 1.3f));
        float zoom = 1f + MaxZoom * shake;

        // Scale around the focus, then shift: the same as zooming a camera in on a point. The shift is
        // whole pixels so the tiles don't shimmer while it settles.
        Transform = Matrix.CreateTranslation(-_focus.X, -_focus.Y, 0f)
            * Matrix.CreateScale(zoom, zoom, 1f)
            * Matrix.CreateTranslation(_focus.X + x, _focus.Y + y, 0f);
    }
}
