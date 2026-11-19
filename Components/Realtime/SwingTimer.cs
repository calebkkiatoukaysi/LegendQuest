using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The auto attack's heartbeat: a thin bar over the hotbar that fills as the swing comes round and
/// flashes when it lands, so you can time a skill between two swings.
/// </summary>
/// <remarks>
/// It's told how far round the swing is each frame; when that falls back from near full to near
/// empty the swing has landed, and the bar flashes white for a moment. Dimmed while there's no swing
/// (no target, out of reach).
/// </remarks>
public class SwingTimer
{
    /// <summary>The fill, and the flash when a swing lands, how long it lasts and how bright it is.</summary>
    public static readonly Color Fill = new(170, 186, 222);
    private const float FlashSeconds = 0.22f;
    private const float FlashStrength = 0.9f;

    /// <summary>How far a fall in progress counts as a swing landing.</summary>
    private const float WrapDrop = 0.5f;

    /// <summary>How dim the bar is with no swing coming.</summary>
    private const float Idle = 0.35f;

    private float _last = -1f;
    private float _flash;

    /// <summary>Notes how far round the swing is this frame and runs its flash.</summary>
    /// <param name="progress">How far round, 0 to 1, or less than 0 for none.</param>
    /// <param name="seconds">How long the frame was.</param>
    public void Update(float progress, float seconds)
    {
        if (progress >= 0f && _last >= 0f && _last - progress > WrapDrop) _flash = FlashSeconds;
        _last = progress;
        _flash = MathF.Max(0f, _flash - seconds);
    }

    /// <summary>Flashes it, as if a swing had just landed.</summary>
    public void Land() => _flash = FlashSeconds;

    /// <summary>Forgets the last swing it saw and its flash, for a new fight.</summary>
    public void Reset()
    {
        _last = -1f;
        _flash = 0f;
    }

    /// <summary>Draws the bar and its fill. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="area">Where, in screen pixels.</param>
    /// <param name="progress">How far round the swing is, 0 to 1, or less than 0 for none.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawArt(SpriteBatch spriteBatch, HudKit kit, Rectangle area, float progress, float alpha)
    {
        float a = progress < 0f ? alpha * Idle : alpha;
        kit.Bar(spriteBatch, area, MathF.Max(0f, progress), Fill, a);
        if (_flash > 0f)
            spriteBatch.Draw(kit.Pixel, area, Color.White * (FlashStrength * _flash / FlashSeconds * alpha));
    }
}
