using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest;

/// <summary>
/// Blend states the stock ones don't cover.
/// </summary>
public static class Blends
{
    /// <summary>Additive blending for premultiplied-alpha content: torch light, embers, anything that glows.</summary>
    /// <remarks>
    /// Same one The Black Box's eye glow used. The stock BlendState.Additive multiplies by alpha
    /// again on the way in, which the content pipeline already did, so glows came out dim. Forcing
    /// the source factor to One fixes it.
    /// </remarks>
    public static readonly BlendState PremultipliedAdditive = new()
    {
        ColorSourceBlend = Blend.One,
        AlphaSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaDestinationBlend = Blend.One,
    };
}
