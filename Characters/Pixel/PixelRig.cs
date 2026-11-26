using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>
/// A figure converted to pixel art (PixelArt.Convert): its own small palette, and each of its pieces
/// as a grid of palette numbers, ready for PixelPoser to pose on the art-pixel grid.
/// </summary>
/// <remarks>
/// The pieces are kept three times finer than an art pixel (Sub), so a limb can turn and still be
/// drawn by majority colour: each art pixel of a pose looks at the nine sub-cells under it and takes
/// the colour most of them have. At rest that's the same as shrinking the painting by majority
/// colour; turned, it's the upscale, turn and pick filter pixel artists use, without ever blending
/// two colours.
/// </remarks>
/// <param name="Colours">The palette: number 0 is clear, the rest opaque.</param>
/// <param name="Outline">For each colour, the outline drawn outside a pixel of it: deep brown-violet mixed with a little of the colour.</param>
/// <param name="Shade">For each colour, the lighter line drawn on it along the edge of a piece in front.</param>
/// <param name="Pieces">The pieces by key: a bone's name, or RigComposer's Cape, Weapon, Offhand or SideFoot.</param>
public sealed record PixelRig(Color[] Colours, Color[] Outline, Color[] Shade, IReadOnlyDictionary<string, PixelPiece> Pieces)
{
    /// <summary>How many sub-cells across an art pixel a piece is kept at.</summary>
    public const int Sub = 3;

    /// <summary>The bit of a sub-cell that says it's a feature of the face (an eye, a brow, the mouth), which counts for more when an art pixel picks its colour; and the bits that are its palette number.</summary>
    public const byte Feature = 0x80, Number = 0x7f;
}

/// <summary>One piece of a pixel figure: a grid of palette numbers, Sub to an art pixel, and where its top left is from the figure's feet.</summary>
/// <param name="Left">Its left edge, in sub-cells right of the feet.</param>
/// <param name="Top">Its top edge, in sub-cells below the feet (so negative).</param>
/// <param name="Width">How many sub-cells wide.</param>
/// <param name="Height">How many sub-cells tall.</param>
/// <param name="Cells">Each sub-cell's palette number, row by row, 0 for clear; with PixelRig.Feature set on a feature of the face.</param>
public sealed record PixelPiece(int Left, int Top, int Width, int Height, byte[] Cells);

/// <summary>
/// Turns a painted figure into pixel art at the size it stands in a room: one palette for the whole
/// figure, each piece shrunk onto the art-pixel grid by majority colour, never by averaging.
/// </summary>
/// <remarks>
/// <para>
/// It works on the figure as RigComposer put it together, after the morph, the fit, the skin tint
/// and the dyes, not on the parts' atlases. A character's build stretches every part (most of the
/// cast are morphed, some by half as much again), and a part painted on one base is scaled to fit
/// the other; pixel art that's been stretched isn't pixel art any more, so the figure is shrunk
/// last, once, at exactly the size it's drawn. The dyes and the skin tints are the painted look's
/// own, so they can't drift, and the palette is the character's (docs/pixel-art-brief.md §1: about
/// 32 colours a character), whatever mix of armour, helm and weapon they wear.
/// </para>
/// <para>
/// The steps: (1) every colour in the figure is counted, and a palette of at most Colours picked
/// from them, no two closer than Apart in a perceptual colour space, so a painted surface's brush
/// texture falls into one flat colour rather than a speckle of near-twins; the palette is then
/// graded so shadows lean violet and highlights warm. (2) In each piece the painting's thin dark ink
/// lines are found (dark, and too narrow to hold a LineWidth square) and all but kept out of the
/// vote, since a line a fraction of an art pixel wide only survives shrinking as broken specks;
/// except on the skin of the face, where they're the eyes, brows and mouth, and are marked as
/// features so they come through as whole pixels. (3) Each sub-cell takes the palette colour most
/// of its pixels have, and is solid if at least half of it is covered. (4) Specks smaller than a
/// sub-cell's neighbours are smoothed away. The outline is not drawn here: PixelPoser draws it round
/// the posed figure, so no line crosses a joint.
/// </para>
/// </remarks>
public static class PixelArt
{
    /// <summary>The most colours a figure's palette has.</summary>
    public const int Colours = 32;

    /// <summary>How far apart two colours of a palette are at the least, in Oklab: nearer than this, a painted surface's texture would speckle between them.</summary>
    private const float Apart = 0.052f;

    /// <summary>How opaque a painted pixel has to be to count, out of 255.</summary>
    private const int Solid = 128;

    /// <summary>How dark a pixel is to be ink, by its luma out of 255, and the widest a run of ink can be, in the painting's pixels, and still be a line rather than a dark area.</summary>
    private const int InkLuma = 62, LineWidth = 7;

    /// <summary>How much an ink line's pixel counts toward its sub-cell being solid, and toward its colour.</summary>
    private const float LineCover = 0.7f, LineVote = 0.12f;

    /// <summary>How much of a sub-cell has to be covered for it to be solid, and how much of it has to be a feature's ink for it to be a feature.</summary>
    private const float Cover = 0.5f, FeatureCover = 0.3f;

    /// <summary>
    /// Telling a feature of the face from the hair and the line round the jaw: how far each way from
    /// a pixel that isn't skin is looked along for skin, in the painting's pixels; how many of the
    /// eight ways there has to be some; and how near the cheek's colour counts as skin, in the
    /// palette's colour space.
    /// </summary>
    private const int FeatureReach = 14, FeatureWays = 6;
    private const float SkinNear = 0.16f;

    /// <summary>The eight ways, across and down.</summary>
    private static readonly int[] WayX = { 1, 1, 0, -1, -1, -1, 0, 1 }, WayY = { 0, 1, 1, 1, 0, -1, -1, -1 };

    /// <summary>How much more a difference of hue or richness counts than one of lightness when the palette is picked: brush texture is mostly lightness, and two materials are mostly hue.</summary>
    private const float Chroma = 2f;

    /// <summary>How many times each piece's specks are smoothed away.</summary>
    private const int Smoothings = 2;

    /// <summary>The outline's colour before a pixel's own is mixed in (docs/pixel-art-brief.md §1), how much of the pixel's colour goes into it, and how far a pixel is darkened toward it for a line inside the figure.</summary>
    private static readonly Vector3 Ink = new Vector3(42, 31, 61) / 255f;
    private const float OutlineOwn = 0.2f, ShadeDeep = 0.42f;

    /// <summary>A figure's layers converted, on the art-pixel grid the room's picture of it has its feet on.</summary>
    /// <param name="layers">Each piece's key and its pixels on the whole canvas, premultiplied.</param>
    /// <param name="whole">The whole figure on the canvas, premultiplied: what shows at rest, which the palette favours.</param>
    /// <param name="width">The canvas's width.</param>
    /// <param name="height">The canvas's height.</param>
    /// <param name="fileHeight">How tall a standard adult is in the canvas's pixels.</param>
    /// <param name="feet">Where the feet are on the canvas.</param>
    /// <param name="face">The box round the head on the canvas, where ink lines on skin are features; empty for none.</param>
    public static PixelRig Convert(IReadOnlyList<(string Key, Color[] Pixels)> layers, Color[] whole, int width, int height, float fileHeight, Vector2 feet, Rectangle face)
    {
        (Point room, Vector2 roomFeet) = ImportedFigure.RoomFrame(width, height, fileHeight, feet);
        // A canvas pixel's middle, in sub-cells from the feet: an art pixel is two of the room's.
        float perX = (float)room.X / width * PixelRig.Sub / ArtSettings.ArtScale, perY = (float)room.Y / height * PixelRig.Sub / ArtSettings.ArtScale;
        float feetX = roomFeet.X * PixelRig.Sub / ArtSettings.ArtScale, feetY = roomFeet.Y * PixelRig.Sub / ArtSettings.ArtScale;
        var subX = new int[width + 1];
        var subY = new int[height + 1];
        for (int x = 0; x <= width; x++) subX[x] = (int)MathF.Floor((x + 0.5f) * perX - feetX);
        for (int y = 0; y <= height; y++) subY[y] = (int)MathF.Floor((y + 0.5f) * perY - feetY);

        var palette = new PaletteBuilder();
        palette.Count(whole, 1, 3f);
        foreach ((_, Color[] pixels) in layers) palette.Count(pixels, 2, 1f);
        palette.Build(Colours);

        var pieces = new Dictionary<string, PixelPiece>(StringComparer.Ordinal);
        foreach ((string key, Color[] pixels) in layers)
        {
            Rectangle box = Trimmed(pixels, width, height);
            if (box.IsEmpty) continue;
            bool head = key == "head" && !face.IsEmpty;
            if (Piece(pixels, width, box, subX, subY, palette, head ? face : Rectangle.Empty) is PixelPiece piece) pieces[key] = piece;
        }

        int n = palette.Colours.Length;
        var colours = new Color[n + 1];
        var outline = new Color[n + 1];
        var shade = new Color[n + 1];
        for (int i = 0; i < n; i++)
        {
            Vector3 c = palette.Colours[i];
            Vector3 line = Vector3.Lerp(Ink, c * 0.6f, OutlineOwn);
            colours[i + 1] = new Color(c);
            outline[i + 1] = new Color(line);
            shade[i + 1] = new Color(Vector3.Lerp(c, line, ShadeDeep));
        }
        return new PixelRig(colours, outline, shade, pieces);
    }

    /// <summary>One piece shrunk onto the sub-cell grid: each sub-cell the palette colour most of its pixels have, or clear.</summary>
    private static PixelPiece Piece(Color[] pixels, int stride, Rectangle box, int[] subX, int[] subY, PaletteBuilder palette, Rectangle face)
    {
        int bw = box.Width, bh = box.Height;
        var index = new byte[bw * bh];
        var ink = new bool[bw * bh];
        for (int y = 0; y < bh; y++)
        {
            for (int x = 0; x < bw; x++)
            {
                Color c = pixels[(box.Y + y) * stride + box.X + x];
                if (c.A < Solid) continue;
                int r = c.R * 255 / c.A, g = c.G * 255 / c.A, b = c.B * 255 / c.A;
                index[y * bw + x] = (byte)(palette.Nearest(r, g, b) + 1);
                ink[y * bw + x] = (r * 77 + g * 150 + b * 29) >> 8 < InkLuma;
            }
        }
        bool[] line = Lines(ink, bw, bh);
        bool[] feature = face.IsEmpty ? null : Features(index, ink, bw, bh, new Rectangle(face.X - box.X, face.Y - box.Y, face.Width, face.Height), palette);

        int left = subX[box.X], top = subY[box.Y];
        int cw = subX[box.Right - 1] - left + 1, ch = subY[box.Bottom - 1] - top + 1;
        var cells = new byte[cw * ch];
        Span<float> votes = stackalloc float[Colours + 1];
        Span<float> inked = stackalloc float[Colours + 1];
        bool any = false;
        int y0 = box.Y;
        for (int cy = 0; cy < ch; cy++)
        {
            int y1 = y0;
            while (y1 < box.Bottom && subY[y1] == top + cy) y1++;
            int x0 = box.X;
            for (int cx = 0; cx < cw; cx++)
            {
                int x1 = x0;
                while (x1 < box.Right && subX[x1] == left + cx) x1++;
                float cover = 0f, featured = 0f;
                votes.Clear();
                inked.Clear();
                for (int y = y0; y < y1; y++)
                {
                    for (int x = x0; x < x1; x++)
                    {
                        int at = (y - box.Y) * bw + x - box.X;
                        int i = index[at];
                        if (i == 0) continue;
                        float a = pixels[y * stride + x].A / 255f;
                        if (feature is not null && feature[at])
                        {
                            cover += a;
                            featured += a;
                            inked[i] += a;
                        }
                        else if (line[at])
                        {
                            cover += a * LineCover;
                            votes[i] += a * LineVote;
                            inked[i] += a * LineVote;
                        }
                        else
                        {
                            cover += a;
                            votes[i] += a;
                        }
                    }
                }
                // How many of the canvas's pixels the sub-cell covers, on or off the piece's box.
                int area = Span(subX, x0, left + cx) * Span(subY, y0, top + cy);
                x0 = x1;
                if (cover < Cover * area || cover <= 0f) continue;
                int best = 0;
                bool isFeature = featured >= FeatureCover * area;
                if (!isFeature)
                    for (int i = 1; i <= Colours; i++)
                        if (votes[i] > votes[best]) best = i;
                if (best == 0)
                    for (int i = 1; i <= Colours; i++)
                        if (inked[i] > inked[best]) best = i;
                cells[cy * cw + cx] = (byte)(isFeature ? best | PixelRig.Feature : best);
                any |= best != 0;
            }
            y0 = y1;
        }
        if (!any) return null;
        for (int i = 0; i < Smoothings; i++) cells = Smoothed(cells, cw, ch);
        return new PixelPiece(left, top, cw, ch, cells);
    }

    /// <summary>
    /// A piece's sub-cells with its specks gone: each solid sub-cell that isn't a feature takes the
    /// colour most of the nine round it have, if more of them have that than its own. A band two
    /// sub-cells wide (a trim, a belt) keeps its colour; a fleck of brush texture doesn't.
    /// </summary>
    private static byte[] Smoothed(byte[] cells, int width, int height)
    {
        var smoothed = (byte[])cells.Clone();
        Span<int> counts = stackalloc int[Colours + 1];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte own = cells[y * width + x];
                if (own == 0 || (own & PixelRig.Feature) != 0) continue;
                counts.Clear();
                int best = own;
                for (int dy = -1; dy <= 1; dy++)
                {
                    int ny = y + dy;
                    if (ny < 0 || ny >= height) continue;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx;
                        if (nx < 0 || nx >= width) continue;
                        int n = cells[ny * width + nx] & PixelRig.Number;
                        if (n == 0) continue;
                        if (++counts[n] > counts[best]) best = n;
                    }
                }
                if (counts[best] > counts[own]) smoothed[y * width + x] = (byte)best;
            }
        }
        return smoothed;
    }

    /// <summary>
    /// Which of the head's pixels are features of the face (the eyes, the brows, the mouth) rather
    /// than hair, a helm or the line round the jaw: those that aren't skin and have skin nearly all
    /// round them. Skin is the commonest colour in the middle of the face and the palette's colours
    /// near it.
    /// </summary>
    private static bool[] Features(byte[] index, bool[] ink, int width, int height, Rectangle face, PaletteBuilder palette)
    {
        // The cheek: the commonest colour that isn't ink in the lower middle of the head's box.
        Span<int> counts = stackalloc int[Colours + 1];
        int cheek = 0;
        for (int y = Math.Max(0, face.Y + face.Height * 45 / 100); y < Math.Min(height, face.Y + face.Height * 85 / 100); y++)
        {
            for (int x = Math.Max(0, face.X + face.Width / 4); x < Math.Min(width, face.X + face.Width * 85 / 100); x++)
            {
                int i = index[y * width + x];
                if (i == 0 || ink[y * width + x]) continue;
                if (++counts[i] > counts[cheek]) cheek = i;
            }
        }
        var features = new bool[index.Length];
        if (cheek == 0) return features;
        Span<bool> skin = stackalloc bool[Colours + 1];
        for (int i = 1; i <= palette.Colours.Length; i++) skin[i] = palette.Distance(i - 1, cheek - 1) < SkinNear;

        // From each pixel that isn't skin, a look each of eight ways for skin before the figure's
        // edge. An eye, a brow or a mouth has skin nearly all round it; the line round the jaw has
        // nothing beyond it, and the hair has skin on one side at most.
        for (int y = Math.Max(0, face.Y); y < Math.Min(height, face.Bottom); y++)
        {
            for (int x = Math.Max(0, face.X); x < Math.Min(width, face.Right); x++)
            {
                int own = index[y * width + x];
                if (own == 0 || skin[own]) continue;
                int ways = 0;
                for (int way = 0; way < 8 && ways + (8 - way) >= FeatureWays; way++)
                {
                    int dx = WayX[way], dy = WayY[way];
                    for (int t = 1; t <= FeatureReach; t++)
                    {
                        int px = x + dx * t, py = y + dy * t;
                        int i = px < 0 || py < 0 || px >= width || py >= height ? 0 : index[py * width + px];
                        if (i == 0) break;
                        if (!skin[i]) continue;
                        ways++;
                        break;
                    }
                }
                features[y * width + x] = ways >= FeatureWays;
            }
        }
        return features;
    }

    /// <summary>How many of the canvas's columns (or rows) fall in a sub-cell, counted from the first of them inside a box out to the sub-cell's own edges.</summary>
    private static int Span(int[] sub, int first, int cell)
    {
        int a = Math.Min(first, sub.Length - 1), b = a;
        while (a > 0 && sub[a - 1] == cell) a--;
        while (b < sub.Length && sub[b] == cell) b++;
        return Math.Max(1, b - a);
    }

    /// <summary>
    /// Which ink pixels are lines: those a LineWidth square of ink can't be fitted over. The ink is
    /// worn away by half a square and grown back; what doesn't come back was too thin to be an area.
    /// </summary>
    private static bool[] Lines(bool[] ink, int width, int height)
    {
        int reach = LineWidth / 2;
        bool[] worn = Box(Box(ink, width, height, reach, true, true), width, height, reach, false, true);
        bool[] grown = Box(Box(worn, width, height, reach, true, false), width, height, reach, false, false);
        var lines = new bool[ink.Length];
        for (int i = 0; i < ink.Length; i++) lines[i] = ink[i] && !grown[i];
        return lines;
    }

    /// <summary>A run of 2 * reach + 1 along the rows or the columns: true where all of it is true (wearing away) or any of it is (growing).</summary>
    private static bool[] Box(bool[] from, int width, int height, int reach, bool across, bool all)
    {
        var to = new bool[from.Length];
        int lines = across ? height : width, length = across ? width : height, step = across ? 1 : width, lineStep = across ? width : 1;
        for (int l = 0; l < lines; l++)
        {
            int start = l * lineStep, count = 0;
            // The count of true in the window, slid along; off the picture counts as false.
            for (int i = 0; i < Math.Min(reach, length); i++)
                if (from[start + i * step]) count++;
            for (int i = 0; i < length; i++)
            {
                if (i + reach < length && from[start + (i + reach) * step]) count++;
                if (i - reach - 1 >= 0 && from[start + (i - reach - 1) * step]) count--;
                to[start + i * step] = all ? count == 2 * reach + 1 : count > 0;
            }
        }
        return to;
    }

    /// <summary>The box round what's solid in a picture; empty if nothing is.</summary>
    private static Rectangle Trimmed(Color[] pixels, int width, int height)
    {
        int x0 = width, y0 = height, x1 = -1, y1 = -1;
        for (int y = 0; y < height; y++)
        {
            int row = y * width;
            for (int x = 0; x < width; x++)
            {
                if (pixels[row + x].A < Solid) continue;
                if (x < x0) x0 = x;
                if (x > x1) x1 = x;
                if (y < y0) y0 = y;
                y1 = y;
            }
        }
        return x1 < 0 ? Rectangle.Empty : new Rectangle(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
    }

    /// <summary>
    /// Picks a figure's palette: its colours counted in coarse bins, the bins clustered in a
    /// perceptual colour space (Oklab) from the most different ones out, and each cluster's colour
    /// graded. The same picture always gives the same palette.
    /// </summary>
    private sealed class PaletteBuilder
    {
        /// <summary>Five bits a channel: 32768 bins.</summary>
        private const int Bits = 5, Bins = 1 << (3 * Bits);

        /// <summary>How many times the clusters are settled.</summary>
        private const int Rounds = 8;

        /// <summary>How little of the figure a colour's bin can be, against all of it, and still start a cluster of its own: less is a stray speck.</summary>
        private const float Speck = 0.00008f;

        private readonly float[] _weight = new float[Bins];
        private readonly float[] _red = new float[Bins], _green = new float[Bins], _blue = new float[Bins];
        private readonly short[] _nearest = new short[Bins];
        private Vector3[] _labs;

        /// <summary>The palette, straight colours 0 to 1.</summary>
        public Vector3[] Colours { get; private set; }

        /// <summary>How far apart two of the palette's colours are, in Oklab, before grading.</summary>
        public float Distance(int a, int b) => Vector3.Distance(_labs[a], _labs[b]);

        /// <summary>Counts a premultiplied picture's solid pixels, every `stride`th of them, each worth `weight`.</summary>
        public void Count(Color[] pixels, int stride, float weight)
        {
            for (int i = 0; i < pixels.Length; i += stride)
            {
                Color c = pixels[i];
                if (c.A < Solid) continue;
                int r = c.R * 255 / c.A, g = c.G * 255 / c.A, b = c.B * 255 / c.A;
                int bin = Bin(r, g, b);
                _weight[bin] += weight;
                _red[bin] += r * weight;
                _green[bin] += g * weight;
                _blue[bin] += b * weight;
            }
        }

        private static int Bin(int r, int g, int b) => (r >> (8 - Bits)) << (2 * Bits) | (g >> (8 - Bits)) << Bits | b >> (8 - Bits);

        /// <summary>Clusters what's been counted into at most `count` colours, no two nearer than Apart.</summary>
        public void Build(int count)
        {
            var bins = new List<int>();
            float all = 0f;
            for (int i = 0; i < Bins; i++)
            {
                if (_weight[i] <= 0f) continue;
                bins.Add(i);
                all += _weight[i];
            }
            var lab = new Vector3[bins.Count];
            var rgb = new Vector3[bins.Count];
            var weight = new float[bins.Count];
            int heaviest = 0;
            for (int k = 0; k < bins.Count; k++)
            {
                int i = bins[k];
                weight[k] = _weight[i];
                rgb[k] = new Vector3(_red[i], _green[i], _blue[i]) / (_weight[i] * 255f);
                lab[k] = Oklab(rgb[k]);
                if (weight[k] > weight[heaviest]) heaviest = k;
            }
            count = Math.Min(count, bins.Count);
            if (count == 0)
            {
                Colours = Array.Empty<Vector3>();
                _labs = Array.Empty<Vector3>();
                return;
            }

            // The first centre is the commonest colour; each next is the colour furthest from all so
            // far (among those common enough not to be a stray speck), till none is further than
            // Apart from a centre: so a small bright trim gets a colour of its own, and a surface's
            // brush texture doesn't get several.
            var centres = new List<Vector3> { lab[heaviest] };
            var far = new float[bins.Count];
            for (int k = 0; k < bins.Count; k++) far[k] = Vector3.DistanceSquared(lab[k], lab[heaviest]);
            float speck = all * Speck;
            while (centres.Count < count)
            {
                int pick = -1;
                float best = Apart * Apart;
                for (int k = 0; k < bins.Count; k++)
                {
                    if (far[k] <= best || weight[k] < speck) continue;
                    best = far[k];
                    pick = k;
                }
                if (pick < 0) break;
                centres.Add(lab[pick]);
                for (int k = 0; k < bins.Count; k++) far[k] = MathF.Min(far[k], Vector3.DistanceSquared(lab[k], lab[pick]));
            }

            var sums = new Vector3[centres.Count];
            var colour = new Vector3[centres.Count];
            var total = new float[centres.Count];
            for (int round = 0; round < Rounds; round++)
            {
                Array.Clear(sums);
                Array.Clear(colour);
                Array.Clear(total);
                for (int k = 0; k < bins.Count; k++)
                {
                    int o = Closest(centres, lab[k]);
                    sums[o] += lab[k] * weight[k];
                    colour[o] += rgb[k] * weight[k];
                    total[o] += weight[k];
                }
                for (int c = 0; c < centres.Count; c++)
                    if (total[c] > 0f) centres[c] = sums[c] / total[c];
            }

            var kept = new List<Vector3>();
            var labs = new List<Vector3>();
            for (int c = 0; c < centres.Count; c++)
            {
                if (total[c] <= 0f) continue;
                kept.Add(Graded(colour[c] / total[c]));
                labs.Add(centres[c]);
            }
            Colours = kept.ToArray();
            _labs = labs.ToArray();
            Array.Fill(_nearest, (short)-1);
        }

        /// <summary>The palette colour nearest a colour, 0 to 255 a channel: its number from 0.</summary>
        public int Nearest(int r, int g, int b)
        {
            int bin = Bin(r, g, b);
            int found = _nearest[bin];
            if (found >= 0) return found;
            // A bin that was counted is looked up by its own mean colour; one that wasn't, by the colour itself.
            Vector3 c = _weight[bin] > 0f
                ? new Vector3(_red[bin], _green[bin], _blue[bin]) / (_weight[bin] * 255f)
                : new Vector3(r, g, b) / 255f;
            Vector3 lab = Oklab(c);
            int best = 0;
            float least = float.MaxValue;
            for (int i = 0; i < _labs.Length; i++)
            {
                float d = Vector3.DistanceSquared(lab, _labs[i]);
                if (d < least)
                {
                    least = d;
                    best = i;
                }
            }
            _nearest[bin] = (short)best;
            return best;
        }

        private static int Closest(List<Vector3> centres, Vector3 lab)
        {
            int best = 0;
            float least = float.MaxValue;
            for (int i = 0; i < centres.Count; i++)
            {
                float d = Vector3.DistanceSquared(lab, centres[i]);
                if (d < least)
                {
                    least = d;
                    best = i;
                }
            }
            return best;
        }
    }

    /// <summary>
    /// A palette colour graded the way pixel art's ramps run (docs/pixel-art-brief.md §1): a little
    /// more contrast and colour, the shadows leaning toward violet-blue and the highlights toward
    /// warm gold, by fixed amounts.
    /// </summary>
    private static Vector3 Graded(Vector3 c)
    {
        float luma = RigComposer.Luma(c);
        // More colour, then more contrast about the middle.
        c = Vector3.Lerp(new Vector3(luma), c, 1.12f);
        c = (c - new Vector3(0.5f)) * 1.06f + new Vector3(0.5f);
        float shadow = MathHelper.Clamp((0.42f - luma) / 0.42f, 0f, 1f), light = MathHelper.Clamp((luma - 0.62f) / 0.38f, 0f, 1f);
        c *= Vector3.Lerp(Vector3.One, new Vector3(0.9f, 0.88f, 1.14f), shadow * 0.55f);
        c *= Vector3.Lerp(Vector3.One, new Vector3(1.05f, 1.02f, 0.9f), light * 0.6f);
        return Vector3.Clamp(c, Vector3.Zero, Vector3.One);
    }

    /// <summary>A colour, 0 to 1 a channel, in Oklab (how light, and how green to red and blue to yellow, with equal steps looking equal), its hue and richness counted Chroma times over.</summary>
    private static Vector3 Oklab(Vector3 c)
    {
        static float Linear(float v) => v <= 0.04045f ? v / 12.92f : MathF.Pow((v + 0.055f) / 1.055f, 2.4f);
        float r = Linear(c.X), g = Linear(c.Y), b = Linear(c.Z);
        float l = MathF.Cbrt(0.4122214708f * r + 0.5363325363f * g + 0.0514459929f * b);
        float m = MathF.Cbrt(0.2119034982f * r + 0.6806995451f * g + 0.1073969566f * b);
        float s = MathF.Cbrt(0.0883024619f * r + 0.2817188376f * g + 0.6299787005f * b);
        return new Vector3(
            0.2104542553f * l + 0.7936177850f * m - 0.0040720468f * s,
            Chroma * (1.9779984951f * l - 2.4285922050f * m + 0.4505937099f * s),
            Chroma * (0.0259040371f * l + 0.7827717662f * m - 0.8086757660f * s));
    }
}
