using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters;

/// <summary>
/// Draws one pose of a pixel figure (PixelRig) as a sprite frame on the art-pixel grid: each piece
/// turned and stretched as its bone has it, every art pixel the colour most of the nine sub-cells
/// under it have; then a lighter line where one limb crosses another, and one dark outline round
/// the whole figure.
/// </summary>
/// <remarks>
/// <para>
/// A walker keeps one of these and asks for a new frame only when its pose steps (twelve times a
/// second), so a frame is a real sprite frame: it's drawn a texel to an art pixel, never turned or
/// stretched on the screen, and so can't shimmer or show a pixel that isn't square. The pieces are
/// laid back to front (Add) and the frame is finished once (End): because the outline goes round
/// the posed figure rather than each piece, no line crosses an elbow or a knee, and the pieces'
/// overlap at a joint closes it.
/// </para>
/// <para>
/// The frame's texture is the picture on the left and, beside it, the figure's inside in white for
/// the hit flash, so a flash brightens the figure inside its outline rather than whiting it out.
/// </para>
/// </remarks>
public sealed class PixelPoser : IDisposable
{
    /// <summary>The clear art pixels kept round a frame, for the outline.</summary>
    private const int Margin = 1;

    /// <summary>How many of the nine sub-cells under an art pixel have to be solid for it to be.</summary>
    private const int Solid = 4;

    /// <summary>How many sub-cells a feature of the face counts as when an art pixel picks its colour, so an eye half a pixel wide is still drawn.</summary>
    private const int FeatureWeight = 2;

    /// <summary>The bit kept with a pixel's limb that says it's a feature of the face, which tidying leaves alone; and the bits that are the limb.</summary>
    private const int Kept = 64, Limb = 63;

    /// <summary>The widest and tallest a frame can be, in art pixels: far more than anyone holding the longest spear overhead.</summary>
    private const int Most = 512;

    private readonly struct Draw
    {
        public readonly PixelPiece Piece;
        public readonly Linear2 Inverse;
        public readonly Vector2 Rest, Now;
        public readonly int Limb;
        public readonly Rectangle Box;

        public Draw(PixelPiece piece, Linear2 inverse, Vector2 rest, Vector2 now, int limb, Rectangle box)
        {
            Piece = piece;
            Inverse = inverse;
            Rest = rest;
            Now = now;
            Limb = limb;
            Box = box;
        }
    }

    private readonly List<Draw> _draws = new();
    private PixelRig _rig;
    private byte[] _index = Array.Empty<byte>(), _owner = Array.Empty<byte>(), _limb = Array.Empty<byte>();
    private Color[] _upload = Array.Empty<Color>();
    private readonly int[] _counts = new int[256];
    private Texture2D _texture;

    /// <summary>The frame's texture: the picture in Source and its white inside in Flash. Null before the first End.</summary>
    public Texture2D Texture => _texture;

    /// <summary>Where the picture is on the texture.</summary>
    public Rectangle Source { get; private set; }

    /// <summary>Where the figure's inside, in white, is on the texture.</summary>
    public Rectangle Flash { get; private set; }

    /// <summary>Where the feet are on the picture, in art pixels from Source's top left.</summary>
    public Vector2 Feet { get; private set; }

    /// <summary>Starts a frame of a figure.</summary>
    /// <param name="rig">The figure.</param>
    public void Begin(PixelRig rig)
    {
        _rig = rig;
        _draws.Clear();
    }

    /// <summary>
    /// Lays a piece on the frame, over those laid before. A point of the piece at rest, `rest` art
    /// pixels from the feet, is drawn `now` from them; the rest of the piece is turned and stretched
    /// about it by `linear`.
    /// </summary>
    /// <param name="key">The piece's key.</param>
    /// <param name="linear">How the piece is turned and stretched.</param>
    /// <param name="rest">The point it's turned about, where it is at rest, in art pixels from the feet.</param>
    /// <param name="now">Where that point is drawn, in art pixels from the feet.</param>
    public void Add(string key, Linear2 linear, Vector2 rest, Vector2 now)
    {
        if (_rig is null || !_rig.Pieces.TryGetValue(key, out PixelPiece piece)) return;
        float det = linear.M11 * linear.M22 - linear.M12 * linear.M21;
        if (MathF.Abs(det) < 1e-4f) return;
        var inverse = new Linear2(linear.M22 / det, -linear.M12 / det, -linear.M21 / det, linear.M11 / det);

        // The box the piece lands in: its four corners, moved.
        float l = (float)piece.Left / PixelRig.Sub, t = (float)piece.Top / PixelRig.Sub;
        float r = l + (float)piece.Width / PixelRig.Sub, b = t + (float)piece.Height / PixelRig.Sub;
        float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
        Span<Vector2> corners = stackalloc Vector2[] { new(l, t), new(r, t), new(l, b), new(r, b) };
        foreach (Vector2 corner in corners)
        {
            Vector2 p = now + linear.Apply(corner - rest);
            x0 = MathF.Min(x0, p.X);
            y0 = MathF.Min(y0, p.Y);
            x1 = MathF.Max(x1, p.X);
            y1 = MathF.Max(y1, p.Y);
        }
        if (!(x1 - x0 < Most) || !(y1 - y0 < Most)) return;
        var box = new Rectangle((int)MathF.Floor(x0) - 1, (int)MathF.Floor(y0) - 1, (int)MathF.Ceiling(x1 - MathF.Floor(x0)) + 2, (int)MathF.Ceiling(y1 - MathF.Floor(y0)) + 2);
        _draws.Add(new Draw(piece, inverse, rest, now, LimbOf(key), box));
    }

    /// <summary>Finishes the frame: draws the pieces, the lines between limbs and the outline, and puts it on the texture.</summary>
    /// <param name="device">The device the texture is made on.</param>
    /// <returns>Whether there's anything to draw.</returns>
    public bool End(GraphicsDevice device)
    {
        if (_rig is null || _draws.Count == 0) return false;
        Rectangle all = _draws[0].Box;
        for (int i = 1; i < _draws.Count; i++) all = Rectangle.Union(all, _draws[i].Box);
        all.Inflate(Margin, Margin);
        if (all.Width > Most || all.Height > Most) all = Rectangle.Intersect(all, new Rectangle(-Most / 2, -Most + 32, Most, Most));
        int w = all.Width, h = all.Height;
        if (_index.Length < w * h)
        {
            _index = new byte[w * h];
            _owner = new byte[w * h];
            _limb = new byte[w * h];
        }
        Array.Clear(_index, 0, w * h);
        Array.Clear(_owner, 0, w * h);

        for (int d = 0; d < _draws.Count; d++) Lay(_draws[d], d + 1, all);
        Tidy(w, h);

        if (_upload.Length < w * 2 * h) _upload = new Color[w * 2 * h];
        Paint(w, h);

        if (_texture is null || _texture.Width < w * 2 || _texture.Height < h)
        {
            int tw = Math.Max(_texture?.Width ?? 128, Grown(w * 2)), th = Math.Max(_texture?.Height ?? 128, Grown(h));
            _texture?.Dispose();
            _texture = new Texture2D(device, tw, th);
        }
        _texture.SetData(0, new Rectangle(0, 0, w * 2, h), _upload, 0, w * 2 * h);
        Source = new Rectangle(0, 0, w, h);
        Flash = new Rectangle(w, 0, w, h);
        Feet = new Vector2(-all.X, -all.Y);
        return true;
    }

    /// <summary>Lets the texture go.</summary>
    public void Dispose()
    {
        _texture?.Dispose();
        _texture = null;
    }

    /// <summary>A size rounded up to the next 64, so a texture isn't made again for every pixel a frame grows.</summary>
    private static int Grown(int size) => (size + 63) / 64 * 64;

    /// <summary>
    /// The limb a piece is part of. Pieces of one limb are joined (an upper arm, its forearm and its
    /// hand), and no line is drawn between them; a line is drawn where one limb crosses another.
    /// </summary>
    private static int LimbOf(string key) => key switch
    {
        "arm_near_up" or "arm_near_low" or "hand_near" => 1,
        "arm_far_up" or "arm_far_low" or "hand_far" => 2,
        "leg_near_up" or "leg_near_low" or "foot_near" or RigComposer.SideFoot => 3,
        "leg_far_up" or "leg_far_low" or "foot_far" => 4,
        "torso" or "skirt" or "hips" or "spine" or "chest" => 5,
        "head" => 6,
        RigComposer.Cape => 7,
        RigComposer.Weapon => 8,
        RigComposer.Offhand => 9,
        _ => 10,
    };

    /// <summary>Draws one piece into the frame: each art pixel in its box takes the colour most of the nine sub-cells under it have.</summary>
    private void Lay(in Draw draw, int order, Rectangle all)
    {
        PixelPiece piece = draw.Piece;
        Rectangle box = Rectangle.Intersect(draw.Box, all);
        Linear2 inv = draw.Inverse;
        byte[] cells = piece.Cells;
        int pw = piece.Width, ph = piece.Height, w = all.Width;
        Span<byte> taps = stackalloc byte[9];
        for (int y = box.Top; y < box.Bottom; y++)
        {
            for (int x = box.Left; x < box.Right; x++)
            {
                // The art pixel's middle, taken back to where it is on the piece at rest, in sub-cells.
                Vector2 o = new Vector2(x + 0.5f, y + 0.5f) - draw.Now;
                float ax = (draw.Rest.X + inv.M11 * o.X + inv.M12 * o.Y) * PixelRig.Sub - piece.Left;
                float ay = (draw.Rest.Y + inv.M21 * o.X + inv.M22 * o.Y) * PixelRig.Sub - piece.Top;
                if (ax < -2f || ay < -2f || ax > pw + 2f || ay > ph + 2f) continue;

                int solid = 0;
                for (int k = 0; k < 9; k++)
                {
                    int dx = k % 3 - 1, dy = k / 3 - 1;
                    int sx = (int)MathF.Floor(ax + inv.M11 * dx + inv.M12 * dy), sy = (int)MathF.Floor(ay + inv.M21 * dx + inv.M22 * dy);
                    byte tap = sx >= 0 && sy >= 0 && sx < pw && sy < ph ? cells[sy * pw + sx] : (byte)0;
                    taps[k] = tap;
                    if (tap != 0) solid++;
                }
                if (solid < Solid) continue;

                // The commonest colour, a feature of the face counting three times; the middle sub-cell's wins a tie.
                int best = 0, most = 0, middle = taps[4] & PixelRig.Number;
                for (int k = 0; k < 9; k++)
                {
                    int tap = taps[k] & PixelRig.Number;
                    if (tap == 0) continue;
                    int count = _counts[tap] += (taps[k] & PixelRig.Feature) != 0 ? FeatureWeight : 1;
                    if (count > most || count == most && tap == middle)
                    {
                        most = count;
                        best = tap;
                    }
                }
                bool feature = false;
                for (int k = 0; k < 9; k++)
                {
                    _counts[taps[k] & PixelRig.Number] = 0;
                    feature |= (taps[k] & PixelRig.Feature) != 0 && (taps[k] & PixelRig.Number) == best;
                }

                int at = (y - all.Y) * w + x - all.X;
                _index[at] = (byte)best;
                _owner[at] = (byte)Math.Min(255, order);
                _limb[at] = (byte)(feature ? Kept | draw.Limb : draw.Limb);
            }
        }
    }

    /// <summary>
    /// Tidies the frame as a pixel artist would: a lone pixel sticking out of the figure is dropped, a
    /// one-pixel hole is filled, and a pixel whose colour none of its neighbours share takes theirs
    /// (not a feature of the face: an eye is a single pixel).
    /// </summary>
    private void Tidy(int w, int h)
    {
        for (int y = 1; y < h - 1; y++)
        {
            for (int x = 1; x < w - 1; x++)
            {
                int at = y * w + x;
                byte up = _index[at - w], down = _index[at + w], left = _index[at - 1], right = _index[at + 1];
                byte own = _index[at];
                int around = (up != 0 ? 1 : 0) + (down != 0 ? 1 : 0) + (left != 0 ? 1 : 0) + (right != 0 ? 1 : 0);
                if (own == 0)
                {
                    if (around < 4) continue;
                    // A hole: the colour beside it, and the piece's that colour is of.
                    _index[at] = left;
                    _owner[at] = _owner[at - 1];
                    _limb[at] = _limb[at - 1];
                    continue;
                }
                if (around == 0)
                {
                    _index[at] = 0;
                    _owner[at] = 0;
                    continue;
                }
                if ((_limb[at] & Kept) != 0 || around < 4) continue;
                if (own == up || own == down || own == left || own == right) continue;
                if (own == _index[at - w - 1] || own == _index[at - w + 1] || own == _index[at + w - 1] || own == _index[at + w + 1]) continue;
                // An orphan in the middle of the figure: the colour most of the four beside it have.
                byte most = up == down || up == left || up == right ? up : down == left || down == right ? down : left == right ? left : (byte)0;
                if (most != 0) _index[at] = most;
            }
        }
    }

    /// <summary>Colours the frame: each pixel its palette colour, darker along the edge of another limb in front of it; the outline in the clear pixels beside the figure; and the white inside for the flash.</summary>
    private void Paint(int w, int h)
    {
        Color[] colours = _rig.Colours, outline = _rig.Outline, shade = _rig.Shade;
        int stride = w * 2;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int at = y * w + x;
                byte own = _index[at];
                Color colour, flash = Color.Transparent;
                if (own != 0)
                {
                    colour = Crossed(at, x, y, w, h) ? shade[own] : colours[own];
                    flash = Color.White;
                }
                else
                {
                    // The outline: beside the figure, the colour of the pixel under it first, so a lit top edge keeps a lighter line.
                    byte beside = y + 1 < h && _index[at + w] != 0 ? _index[at + w]
                        : y > 0 && _index[at - w] != 0 ? _index[at - w]
                        : x > 0 && _index[at - 1] != 0 ? _index[at - 1]
                        : x + 1 < w && _index[at + 1] != 0 ? _index[at + 1] : (byte)0;
                    colour = beside != 0 ? outline[beside] : Color.Transparent;
                }
                _upload[y * stride + x] = colour;
                _upload[y * stride + w + x] = flash;
            }
        }
    }

    /// <summary>Whether a pixel is beside one of another limb that's in front of it: where the line between two limbs goes, on the one behind.</summary>
    private bool Crossed(int at, int x, int y, int w, int h)
    {
        int order = _owner[at], limb = _limb[at] & Limb;
        return x > 0 && Over(at - 1, order, limb) || x + 1 < w && Over(at + 1, order, limb)
            || y > 0 && Over(at - w, order, limb) || y + 1 < h && Over(at + w, order, limb);
    }

    private bool Over(int at, int order, int limb) => _index[at] != 0 && _owner[at] > order && (_limb[at] & Limb) != limb;
}
