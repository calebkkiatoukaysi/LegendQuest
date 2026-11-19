using System;
using System.Collections.Generic;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The pale stretch at the end of a health bar that shows what was just lost: it holds a moment
/// after a hit, then drains down to the health, so a big hit reads as big.
/// </summary>
/// <remarks>Kept per person, by id, by each component that draws a health bar, and asked once a frame for each.</remarks>
public class HealthLag
{
    /// <summary>How long the lag holds after a hit before it drains, and how fast it drains, a share of the bar a second.</summary>
    private const float HoldSeconds = 0.4f;
    private const float DrainRate = 0.7f;

    /// <summary>Each person's lag: where it ends, how much longer it holds, and their health last frame.</summary>
    private readonly Dictionary<string, (float Shown, float Hold, float Last)> _lags = new(StringComparer.Ordinal);

    /// <summary>Where someone's lag is this frame: never under their health, holding after each hit and then catching up.</summary>
    /// <param name="id">Who.</param>
    /// <param name="share">Their health now, 0 to 1.</param>
    /// <param name="seconds">How long the frame was.</param>
    /// <returns>The lag's end, 0 to 1.</returns>
    public float Step(string id, float share, float seconds)
    {
        string key = id ?? string.Empty;
        if (!_lags.TryGetValue(key, out (float Shown, float Hold, float Last) lag) || share >= lag.Shown)
        {
            _lags[key] = (share, 0f, share);
            return share;
        }

        if (share < lag.Last) lag.Hold = HoldSeconds;
        else if (lag.Hold > 0f) lag.Hold -= seconds;
        else lag.Shown = MathF.Max(share, lag.Shown - DrainRate * seconds);
        lag.Last = share;
        _lags[key] = lag;
        return lag.Shown;
    }

    /// <summary>Sets someone's lag outright, for a picture of a hit just taken.</summary>
    /// <param name="id">Who.</param>
    /// <param name="shown">Where the lag ends, 0 to 1.</param>
    /// <param name="share">Their health now, 0 to 1.</param>
    public void Set(string id, float shown, float share) => _lags[id ?? string.Empty] = (shown, HoldSeconds * 100f, share);

    /// <summary>Forgets everyone, for a new fight.</summary>
    public void Clear() => _lags.Clear();
}
