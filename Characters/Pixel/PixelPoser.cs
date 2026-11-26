using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
/// laid back to front (Add) and the frame is finished once: on the poser's own texture (End), or
/// on one to keep (Bake), since a clip's steps come round again (PixelFrames). Because the outline
/// goes round the posed figure rather than each piece, no line crosses an elbow or a knee, and the
/// pieces' overlap at a joint closes it.
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

    /// <summary>How many of the nine sub-cells under an art pixel have to be solid for it to be: fewer if the one under its middle is, so a bowstave or a blade's tip a pixel wide isn't broken into dashes when it turns.</summary>
    private const int Solid = 5, SolidMiddle = 3;

    /// <summary>How many sub-cells a feature of the face counts as when an art pixel picks its colour, so an eye half a pixel wide is still drawn.</summary>
    private const int FeatureWeight = 2;

    /// <summary>The bit kept with a pixel's limb that says it's a feature of the face, which tidying leaves alone; and the bits that are the limb.</summary>
    private const int Kept = 64, Limb = 63;

    /// <summary>How far a piece can have moved, or its far end swung, since the frame was drawn without it being drawn again, in art pixels.</summary>
    private const float Still = 0.12f;

    /// <summary>Added to a place on a piece before it's cut to a whole number and taken off after, so the cut rounds down either side of zero; a multiple of PixelRig.Sub.</summary>
    private const int Lift = 3072;

    /// <summary>The widest and tallest a frame can be, in art pixels: far more than anyone holding the longest spear overhead.</summary>
    private const int Most = 512;

    private readonly struct Draw
    {
        public readonly PixelPiece Piece;
        public readonly Linear2 Linear, Inverse;
        public readonly Vector2 Rest, Now;
        public readonly int Limb;
        public readonly Rectangle Box;

        public Draw(PixelPiece piece, Linear2 linear, Linear2 inverse, Vector2 rest, Vector2 now, int limb, Rectangle box)
        {
            Piece = piece;
            Linear = linear;
            Inverse = inverse;
            Rest = rest;
            Now = now;
            Limb = limb;
            Box = box;
        }
    }

    private readonly List<Draw> _draws = new(), _drawn = new();
    private PixelRig _drawnRig;
    private PixelRig _rig;
    private byte[] _index = Array.Empty<byte>(), _owner = Array.Empty<byte>(), _limb = Array.Empty<byte>();
    private Color[] _upload = Array.Empty<Color>();
    private readonly int[] _counts = new int[256];
    private readonly byte[] _taps = new byte[9];
    private readonly float[] _stepX = new float[9], _stepY = new float[9];
    private Texture2D _texture;

    /// <summary>The frame last finished on the poser's own texture, and whether it's still the last one drawn into the buffer.</summary>
    private PixelFrame _own;
    private bool _ownIsLast;

    /// <summary>The frame last finished to be kept.</summary>
    private PixelFrame _baked;

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
        _draws.Add(new Draw(piece, linear, inverse, rest, now, LimbOf(key), box));
    }

    /// <summary>Finishes the frame on the poser's own texture: draws the pieces, the lines between limbs and the outline, unless nothing has moved a pixel's worth since it last did.</summary>
    /// <param name="device">The device the texture is made on.</param>
    /// <returns>The frame, or null if there's nothing to draw.</returns>
    public PixelFrame End(GraphicsDevice device)
    {
        if (_rig is null || _draws.Count == 0) return null;
        if (_own is not null && _ownIsLast && Unmoved()) return _own;
        if (!Render(out int w, out int h, out Rectangle all)) return null;
        if (_texture is null || _texture.Width < w * 2 || _texture.Height < h)
        {
            int tw = Math.Max(_texture?.Width ?? 128, Grown(w * 2)), th = Math.Max(_texture?.Height ?? 128, Grown(h));
            _texture?.Dispose();
            _texture = new Texture2D(device, tw, th);
        }
        _texture.SetData(0, new Rectangle(0, 0, w * 2, h), _upload, 0, w * 2 * h);
        _own = new PixelFrame(_texture, new Rectangle(0, 0, w, h), new Rectangle(w, 0, w, h), new Vector2(-all.X, -all.Y));
        _ownIsLast = true;
        _baked = null;
        return _own;
    }

    /// <summary>Finishes the frame on a texture of its own, to be kept (PixelFrames).</summary>
    /// <param name="device">The device the texture is made on.</param>
    /// <returns>The frame, or null if there's nothing to draw.</returns>
    public PixelFrame Bake(GraphicsDevice device)
    {
        if (_rig is null || _draws.Count == 0) return null;
        // Most steps of a breath are the picture the step before was: the same frame, kept twice.
        if (_baked is not null && !_ownIsLast && !_baked.Texture.IsDisposed && Unmoved()) return _baked;
        if (!Render(out int w, out int h, out Rectangle all)) return null;
        _ownIsLast = false;
        var texture = new Texture2D(device, w * 2, h);
        texture.SetData(0, new Rectangle(0, 0, w * 2, h), _upload, 0, w * 2 * h);
        return _baked = new PixelFrame(texture, new Rectangle(0, 0, w, h), new Rectangle(w, 0, w, h), new Vector2(-all.X, -all.Y));
    }

    /// <summary>Draws the frame into the upload buffer: the pieces front to back, the tidying, the lines between limbs, the outline and the white inside.</summary>
    private bool Render(out int w, out int h, out Rectangle all)
    {
        _drawnRig = _rig;
        _drawn.Clear();
        _drawn.AddRange(_draws);
        all = _draws[0].Box;
        for (int i = 1; i < _draws.Count; i++) all = Rectangle.Union(all, _draws[i].Box);
        all.Inflate(Margin, Margin);
        if (all.Width > Most || all.Height > Most) all = Rectangle.Intersect(all, new Rectangle(-Most / 2, -Most + 32, Most, Most));
        w = all.Width;
        h = all.Height;
        if (w <= 0 || h <= 0) return false;
        if (_index.Length < w * h)
        {
            _index = new byte[w * h];
            _owner = new byte[w * h];
            _limb = new byte[w * h];
        }
        Array.Clear(_index, 0, w * h);
        Array.Clear(_owner, 0, w * h);

        // Front to back, so what a nearer piece covers isn't worked out for the ones behind it.
        for (int d = _draws.Count - 1; d >= 0; d--) Lay(_draws[d], d + 1, all);
        Tidy(w, h);

        if (_upload.Length < w * 2 * h) _upload = new Color[w * 2 * h];
        Paint(w, h);
        return true;
    }

    /// <summary>
    /// Whether the frame asked for is the one already drawn, to within Still of an art pixel: the same
    /// pieces in the same order, none of them moved or turned enough to change a pixel's worth. Most
    /// of a breath is, so someone standing is drawn again only a few times a second; the difference
    /// is always taken from the frame that was drawn, so a slow drift still gets there.
    /// </summary>
    private bool Unmoved()
    {
        if (_drawnRig != _rig || _drawn.Count != _draws.Count) return false;
        for (int i = 0; i < _draws.Count; i++)
        {
            Draw a = _draws[i], b = _drawn[i];
            if (a.Piece != b.Piece || a.Rest != b.Rest) return false;
            if (MathF.Abs(a.Now.X - b.Now.X) > Still || MathF.Abs(a.Now.Y - b.Now.Y) > Still) return false;
            float reach = Math.Max(a.Piece.Width, a.Piece.Height) / (float)PixelRig.Sub;
            float turned = MathF.Max(MathF.Max(MathF.Abs(a.Linear.M11 - b.Linear.M11), MathF.Abs(a.Linear.M12 - b.Linear.M12)),
                MathF.Max(MathF.Abs(a.Linear.M21 - b.Linear.M21), MathF.Abs(a.Linear.M22 - b.Linear.M22)));
            if (turned * reach > Still) return false;
        }
        return true;
    }

    /// <summary>Lets the texture go.</summary>
    public void Dispose()
    {
        _texture?.Dispose();
        _texture = null;
        _own = null;
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
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void Lay(in Draw draw, int order, Rectangle all)
    {
        PixelPiece piece = draw.Piece;
        Rectangle box = Rectangle.Intersect(draw.Box, all);
        Linear2 inv = draw.Inverse;
        byte[] cells = piece.Cells;
        int pw = piece.Width, ph = piece.Height, w = all.Width;
        bool[] near = piece.Near, flat = piece.Flat;
        int nw = piece.NearWidth, nh = near.Length / nw;
        byte[] taps = _taps;
        float[] stepX = _stepX, stepY = _stepY;
        int[] counts = _counts;
        byte[] index = _index, owner = _owner, limbs = _limb;
        byte mine = (byte)Math.Min(255, order), limb = (byte)draw.Limb;
        for (int k = 0; k < 9; k++)
        {
            int dx = k % 3 - 1, dy = k / 3 - 1;
            // Kept above zero, so a cast rounds down.
            stepX[k] = inv.M11 * dx + inv.M12 * dy + Lift;
            stepY[k] = inv.M21 * dx + inv.M22 * dy + Lift;
        }
        for (int y = box.Top; y < box.Bottom; y++)
        {
            // The art pixel's middle, taken back to where it is on the piece at rest, in sub-cells.
            float oy = y + 0.5f - draw.Now.Y, ox = box.Left + 0.5f - draw.Now.X;
            float ax = (draw.Rest.X + inv.M11 * ox + inv.M12 * oy) * PixelRig.Sub - piece.Left;
            float ay = (draw.Rest.Y + inv.M21 * ox + inv.M22 * oy) * PixelRig.Sub - piece.Top;
            float alongX = inv.M11 * PixelRig.Sub, alongY = inv.M21 * PixelRig.Sub;
            for (int x = box.Left; x < box.Right; x++, ax += alongX, ay += alongY)
            {
                // Nothing of the piece anywhere near: most of a turned piece's box.
                int bx = (int)(ax + Lift) / PixelRig.Sub - Lift / PixelRig.Sub + 1, by = (int)(ay + Lift) / PixelRig.Sub - Lift / PixelRig.Sub + 1;
                if (bx < 0 || by < 0 || bx >= nw || by >= nh || !near[by * nw + bx]) continue;

                int at = (y - all.Y) * w + x - all.X;
                if (index[at] != 0) continue;
                // Well inside a flat patch of colour, every sub-cell under the pixel is that colour.
                int mx = (int)(ax + Lift) - Lift, my = (int)(ay + Lift) - Lift;
                if (mx >= 0 && my >= 0 && mx < pw && my < ph && flat[my * pw + mx])
                {
                    index[at] = cells[my * pw + mx];
                    owner[at] = mine;
                    limbs[at] = limb;
                    continue;
                }

                int solid = 0;
                for (int k = 0; k < 9; k++)
                {
                    int sx = (int)(ax + stepX[k]) - Lift, sy = (int)(ay + stepY[k]) - Lift;
                    byte tap = sx >= 0 && sy >= 0 && sx < pw && sy < ph ? cells[sy * pw + sx] : (byte)0;
                    taps[k] = tap;
                    if (tap != 0) solid++;
                }
                if (solid < (taps[4] != 0 ? SolidMiddle : Solid)) continue;

                // The commonest colour, a feature of the face counting three times; the middle sub-cell's wins a tie.
                int best = 0, most = 0, middle = taps[4] & PixelRig.Number;
                for (int k = 0; k < 9; k++)
                {
                    int tap = taps[k] & PixelRig.Number;
                    if (tap == 0) continue;
                    int count = counts[tap] += (taps[k] & PixelRig.Feature) != 0 ? FeatureWeight : 1;
                    if (count > most || count == most && tap == middle)
                    {
                        most = count;
                        best = tap;
                    }
                }
                bool feature = false;
                for (int k = 0; k < 9; k++)
                {
                    counts[taps[k] & PixelRig.Number] = 0;
                    feature |= (taps[k] & PixelRig.Feature) != 0 && (taps[k] & PixelRig.Number) == best;
                }

                index[at] = (byte)best;
                owner[at] = mine;
                limbs[at] = (byte)(feature ? Kept | limb : limb);
            }
        }
    }

    /// <summary>
    /// Tidies the frame as a pixel artist would: a lone pixel sticking out of the figure is dropped, a
    /// one-pixel hole is filled, and a speck of a colour its neighbours don't share takes theirs (not
    /// a feature of the face: an eye is a pixel or two).
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
                // A speck in the middle of the figure (at most one of the eight round it shares its
                // colour, so it isn't part of a line) takes the colour three of the four beside it have.
                int alike = (own == up ? 1 : 0) + (own == down ? 1 : 0) + (own == left ? 1 : 0) + (own == right ? 1 : 0)
                    + (own == _index[at - w - 1] ? 1 : 0) + (own == _index[at - w + 1] ? 1 : 0) + (own == _index[at + w - 1] ? 1 : 0) + (own == _index[at + w + 1] ? 1 : 0);
                if (alike > 1) continue;
                byte most = up == down && (up == left || up == right) ? up : left == right && (left == up || left == down) ? left : (byte)0;
                if (most != 0 && most != own) _index[at] = most;
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
