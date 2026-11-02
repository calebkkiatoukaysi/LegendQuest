using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>
/// The shape of the frames on someone's sheet: how big one is, where the feet are in it, the box
/// round the figure for pointing at it, the shadow under it, and the floor it stands on.
/// </summary>
/// <remarks>
/// Every person has the one shape, The Black Box's 32x48 frame with the feet at (16, 46), so the
/// paperdoll's layers and the townsfolk's sheets all cut the same way. The creatures are bigger, and
/// each has a shape of its own in Creatures. WalkerSprite cuts and draws by whichever it's given.
/// </remarks>
/// <param name="Frame">How big one frame is, in art pixels.</param>
/// <param name="Feet">Where the feet are in a frame (the middle of the bottom of the soles), in art pixels. The frame is drawn from this point.</param>
/// <param name="Figure">The box round the figure itself, for pointing at it: how wide, and how tall up from the feet, in art pixels.</param>
/// <param name="Shadow">The shadow under the feet: how wide and how tall, in art pixels.</param>
/// <param name="Footprint">The floor nobody can walk through: how wide, and how deep up from the feet, in art pixels.</param>
public sealed record WalkerShape(Point Frame, Vector2 Feet, Vector2 Figure, Vector2 Shadow, Vector2 Footprint)
{
    /// <summary>A person: anyone off the paperdoll or a townsfolk sheet. Their footprint is the same size as the party's feet box, so nobody walks through anybody.</summary>
    public static WalkerShape Person { get; } = new(
        Frame: new Point(32, 48),
        Feet: new Vector2(16f, 46f),
        Figure: new Vector2(14f, 42f),
        Shadow: new Vector2(16f, 5f),
        Footprint: new Vector2(10f, 4f));

    /// <summary>What's wrong with it, or null if it's well formed: the frame has some size, the feet are in it, and the figure, the shadow and the footprint all have some size and fit inside it.</summary>
    public string Problem()
    {
        if (Frame.X <= 0 || Frame.Y <= 0) return $"has a frame {Frame.X} by {Frame.Y}, which is nothing";
        if (Feet.X < 0f || Feet.X > Frame.X || Feet.Y <= 0f || Feet.Y > Frame.Y)
            return $"has its feet at {Feet.X}, {Feet.Y}, which is outside its {Frame.X} by {Frame.Y} frame";
        if (Figure.X <= 0f || Figure.Y <= 0f || Figure.X > Frame.X || Figure.Y > Feet.Y)
            return $"has a figure {Figure.X} by {Figure.Y}, which doesn't fit in its frame over its feet";
        if (Shadow.X <= 0f || Shadow.Y <= 0f || Shadow.X > Frame.X)
            return $"has a shadow {Shadow.X} by {Shadow.Y}, which is nothing or wider than its frame";
        if (Footprint.X <= 0f || Footprint.Y <= 0f || Footprint.X > Frame.X)
            return $"has a footprint {Footprint.X} by {Footprint.Y}, which is nothing or wider than its frame";
        return null;
    }
}
