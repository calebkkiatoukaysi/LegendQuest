using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>
/// One part, cut off its atlas page: its pixels premultiplied, its mask if it has one, its slot, the
/// base it was painted on, its pivot, and the colours its dye channels were painted.
/// </summary>
/// <param name="Id">Its id in parts.json.</param>
/// <param name="Slot">Its slot.</param>
/// <param name="Base">The base it was painted on.</param>
/// <param name="Width">How wide it is.</param>
/// <param name="Height">How tall it is.</param>
/// <param name="Pixels">Its pixels, premultiplied, row by row.</param>
/// <param name="Mask">Its mask, row by row, or null for none: red how much each pixel is skin, green and blue how much it's in each dye channel.</param>
/// <param name="Pivot">Its socket in its own pixels.</param>
/// <param name="SkinRatio">For a face, its skin against the base's; one for anything else.</param>
/// <param name="Tones">The colour each of its two dye channels was painted, 0 to 1, or null for one it hasn't.</param>
public sealed record PartImage(string Id, PartSlot Slot, string Base, int Width, int Height, Color[] Pixels, Color[] Mask, Vector2 Pivot, Vector3 SkinRatio, Vector3?[] Tones);

/// <summary>How one part's coloured as it's put together: its skin multiplied by a ratio, and its two dye channels dyed to swatches.</summary>
/// <param name="SkinRatio">What its skin's multiplied by, red, green and blue: one for as it is.</param>
/// <param name="First">The swatch for its first dye channel (an armor's Base, a face's hair), or null for as painted.</param>
/// <param name="Second">The swatch for its second (an armor's Trim, a face's eyes), or null for as painted.</param>
public readonly record struct Paint(Vector3 SkinRatio, Color? First = null, Color? Second = null)
{
    /// <summary>Nothing changed.</summary>
    public static readonly Paint Plain = new(Vector3.One);
}

/// <summary>
/// Puts a character together from their parts at their base's sockets, morphed, into one
/// premultiplied picture: the same as tools/import_parts.py's compose, which draws the previews.
/// </summary>
/// <remarks>
/// <para>
/// Each part is drawn back to front by its slot, with its pivot on its socket where the morph moved
/// it, at the size the morph gives its slot (Morph.ScaleOf), and a cape, helm or weapon painted on the
/// other base scaled to fit this one. Each is coloured as its Paint says: its skin multiplied to the
/// face's or the one picked, and its dye channels dyed (Dye). A part at its own size is copied as it is, so a character
/// with no morph comes out pixel for pixel as the parts were painted; one that's scaled is resampled
/// on premultiplied colour, so its edges don't darken.
/// </para>
/// <para>
/// Drawn once, when someone's figure is first asked for, and kept: a figure is drawn as one picture,
/// so the rooms' point-sampled batch keeps it crisp, and a morph is filtered properly instead of
/// stretched a pixel at a time.
/// </para>
/// </remarks>
public static class RigComposer
{
    /// <summary>A character put together: the picture, how big it is, where the root is on it, the box round the head, and the pieces it moves in.</summary>
    /// <param name="Pixels">The picture, premultiplied, row by row.</param>
    /// <param name="Width">How wide it is.</param>
    /// <param name="Height">How tall it is.</param>
    /// <param name="Root">Where the root (the ground between the feet) is on it.</param>
    /// <param name="Head">The box round the head, crown to chin, on it.</param>
    /// <param name="Layers">Each piece that has a part in it, back to front: the same picture split up, every one the picture's size so they line up.</param>
    /// <param name="Hand">Where the near hand is on it.</param>
    public sealed record Result(Color[] Pixels, int Width, int Height, Vector2 Root, Rectangle Head, IReadOnlyList<Layer> Layers, Vector2 Hand);

    /// <summary>One piece of a character: its parts on a picture the whole figure's size, and the socket it turns about on it.</summary>
    /// <param name="Bone">Which piece.</param>
    /// <param name="Pixels">Its parts, premultiplied, on the whole picture's canvas.</param>
    /// <param name="Pivot">Its socket on that canvas, where the morph moved it.</param>
    public sealed record Layer(FigureBone Bone, Color[] Pixels, Vector2 Pivot);

    /// <summary>Puts the parts together.</summary>
    /// <param name="sockets">The base's sockets, by name, on the scaled canvas.</param>
    /// <param name="head">The base's head box on the scaled canvas: left, top, width, height.</param>
    /// <param name="parts">The parts, in any order. They're drawn by slot.</param>
    /// <param name="morph">How much taller and broader than the base.</param>
    /// <param name="paint">How each part's coloured.</param>
    /// <param name="fit">How much more to scale a part painted on the other base, so it fits this one (PartCatalog's Fit); null for none.</param>
    /// <param name="fists">The base's fists, the near hand's piece's and the far's, as masks on the body's canvas: what a hand holds is drawn under its fist (LiftFist). Null to draw it over.</param>
    public static Result Compose(IReadOnlyDictionary<string, Vector2> sockets, float[] head, IEnumerable<PartImage> parts, Morph morph, Func<PartImage, Paint> paint, Func<PartImage, Vector2> fit = null,
        IReadOnlyDictionary<FigureBone, PartImage> fists = null)
    {
        Vector2 root = sockets[Sockets.Root];
        var placed = new List<(Color[] Pixels, int Width, int Height, int Left, int Top, FigureBone Bone)>();
        foreach (PartImage part in parts.OrderBy(p => p.Slot))
        {
            Vector2 at = morph.Move(sockets[Sockets.Of(part.Slot)], root);
            Vector2 scale = morph.ScaleOf(part.Slot) * (fit?.Invoke(part) ?? Vector2.One);
            int width = Math.Max(1, (int)MathF.Round(part.Width * scale.X));
            int height = Math.Max(1, (int)MathF.Round(part.Height * scale.Y));
            int left = (int)MathF.Round(at.X - part.Pivot.X * width / part.Width);
            int top = (int)MathF.Round(at.Y - part.Pivot.Y * height / part.Height);
            Color[] pixels = Painted(part, paint(part));
            if (width != part.Width || height != part.Height) pixels = ImportedFigure.Scaled(pixels, part.Width, part.Height, width, height);
            placed.Add((pixels, width, height, left, top, Sockets.BoneOf(part.Slot)));
        }
        if (placed.Count == 0) throw new ArgumentException("There are no parts to put together.", nameof(parts));

        int x0 = placed.Min(p => p.Left), y0 = placed.Min(p => p.Top);
        int x1 = placed.Max(p => p.Left + p.Width), y1 = placed.Max(p => p.Top + p.Height);
        int canvasWidth = x1 - x0, canvasHeight = y1 - y0;

        // Each piece's own picture, its parts put together in slot order, and where it turns.
        var pieces = new Dictionary<FigureBone, Color[]>();
        foreach ((Color[] pixels, int width, int height, int left, int top, FigureBone bone) in placed)
        {
            if (!pieces.TryGetValue(bone, out Color[] layer)) pieces[bone] = layer = new Color[canvasWidth * canvasHeight];
            Over(layer, canvasWidth, pixels, width, height, left - x0, top - y0);
        }
        var pivots = Enum.GetValues<FigureBone>().ToDictionary(bone => bone, bone => morph.Move(sockets[Sockets.OfBone(bone)], root) - new Vector2(x0, y0));
        if (fists is not null && pieces.TryGetValue(FigureBone.Body, out Color[] body))
        {
            foreach ((FigureBone bone, PartImage fist) in fists)
            {
                if (pieces.TryGetValue(bone, out Color[] held) && LiftFist(body, held, canvasWidth, canvasHeight, fist, sockets, root, morph, x0, y0) is Vector2 wrist)
                    pivots[bone] = wrist;
            }
        }

        var layers = new List<Layer>();
        var canvas = new Color[canvasWidth * canvasHeight];
        foreach (FigureBone bone in Enum.GetValues<FigureBone>())
        {
            if (!pieces.TryGetValue(bone, out Color[] layer)) continue;
            layers.Add(new Layer(bone, layer, pivots[bone]));
            Over(canvas, canvasWidth, layer, canvasWidth, canvasHeight, 0, 0);
        }

        // The head box goes where the head did: on the neck where the morph moved it, at the head's size.
        Vector2 neck = sockets[Sockets.Neck], neckAt = morph.Move(neck, root);
        float s = morph.HeadScale;
        float headLeft = neckAt.X + (head[0] - neck.X) * s - x0, headTop = neckAt.Y + (head[1] - neck.Y) * s - y0;
        var box = new Rectangle((int)MathF.Floor(headLeft), (int)MathF.Floor(headTop), (int)MathF.Ceiling(head[2] * s), (int)MathF.Ceiling(head[3] * s));
        box = Rectangle.Intersect(box, new Rectangle(0, 0, canvasWidth, canvasHeight));
        return new Result(canvas, canvasWidth, canvasHeight, root - new Vector2(x0, y0), box, layers, morph.Move(sockets[Sockets.NearHand], root) - new Vector2(x0, y0));
    }

    /// <summary>
    /// A part coloured as its paint says, kept premultiplied: each pixel's skin multiplied towards the
    /// ratio by how much of it is skin, then dyed in each channel by how much of it is in that channel.
    /// The part itself if nothing changes.
    /// </summary>
    private static Color[] Painted(PartImage part, Paint paint)
    {
        bool skin = paint.SkinRatio != Vector3.One;
        Vector3? first = paint.First is Color f && part.Tones is { Length: 2 } && part.Tones[0] is not null ? f.ToVector3() : null;
        Vector3? second = paint.Second is Color g && part.Tones is { Length: 2 } && part.Tones[1] is not null ? g.ToVector3() : null;
        if (part.Mask is null || !skin && first is null && second is null) return part.Pixels;

        var painted = new Color[part.Pixels.Length];
        for (int i = 0; i < painted.Length; i++)
        {
            Color c = part.Pixels[i], m = part.Mask[i];
            if (c.A == 0 || m.R == 0 && (first is null || m.G == 0) && (second is null || m.B == 0))
            {
                painted[i] = c;
                continue;
            }
            float a = c.A / 255f;
            var v = new Vector3(c.R, c.G, c.B) / (255f * a);
            if (skin && m.R > 0) v *= Vector3.One + m.R / 255f * (paint.SkinRatio - Vector3.One);
            if (first is Vector3 one && m.G > 0) v = Vector3.Lerp(v, Dye(v, part.Tones[0].Value, one), m.G / 255f);
            if (second is Vector3 two && m.B > 0) v = Vector3.Lerp(v, Dye(v, part.Tones[1].Value, two), m.B / 255f);
            v = Vector3.Clamp(v, Vector3.Zero, Vector3.One) * a;
            painted[i] = new Color((int)MathF.Round(v.X * 255f), (int)MathF.Round(v.Y * 255f), (int)MathF.Round(v.Z * 255f), c.A);
        }
        return painted;
    }

    /// <summary>
    /// A colour dyed to a swatch: one as bright as the colour its channel was painted becomes the
    /// swatch, a darker one the swatch darkened by as much, and a lighter one the swatch lightened
    /// towards white by as much, so the cel shading and the lines come through. tools/import_parts.py's
    /// dye does the same for its previews.
    /// </summary>
    /// <param name="colour">The colour, straight, 0 to 1.</param>
    /// <param name="painted">The colour its channel was painted.</param>
    /// <param name="swatch">The swatch.</param>
    public static Vector3 Dye(Vector3 colour, Vector3 painted, Vector3 swatch)
    {
        float y = Luma(colour), reference = MathF.Max(Luma(painted), 0.02f);
        if (y <= reference) return swatch * (y / reference);
        return swatch + (Vector3.One - swatch) * ((y - reference) / MathF.Max(1e-3f, 1f - reference));
    }

    /// <summary>How bright a colour is, 0 to 1, as the dyes shade by it.</summary>
    public static float Luma(Vector3 colour) => 0.299f * colour.X + 0.587f * colour.Y + 0.114f * colour.Z;

    /// <summary>
    /// Holds what a hand holds the way a 2D game character does, under the fist: the fist comes off the
    /// body's piece and goes over the hand's, on top of what it holds, plainer than it's painted, so the
    /// hand reads as closed round a grip rather than clenched shut (PlainFist). It's put where the body
    /// is, and fades into the arm at the wrist. Returns the wrist, the top of the fist across its
    /// middle, which the hand's piece turns about so a swing keeps the hand on the arm; null if the
    /// fist's off the canvas.
    /// </summary>
    private static Vector2? LiftFist(Color[] body, Color[] held, int canvasWidth, int canvasHeight, PartImage fist,
        IReadOnlyDictionary<string, Vector2> sockets, Vector2 root, Morph morph, int x0, int y0)
    {
        Vector2 at = morph.Move(sockets[Sockets.Root], root);
        Vector2 scale = morph.ScaleOf(PartSlot.Body);
        int width = Math.Max(1, (int)MathF.Round(fist.Width * scale.X));
        int height = Math.Max(1, (int)MathF.Round(fist.Height * scale.Y));
        int left = (int)MathF.Round(at.X - fist.Pivot.X * width / fist.Width) - x0;
        int top = (int)MathF.Round(at.Y - fist.Pivot.Y * height / fist.Height) - y0;
        if (left < 0 || top < 0 || left + width > canvasWidth || top + height > canvasHeight) return null;
        Color[] mask = width == fist.Width && height == fist.Height ? fist.Pixels : ImportedFigure.Scaled(fist.Pixels, fist.Width, fist.Height, width, height);

        var under = new Color[width * height];
        for (int y = 0; y < height; y++)
            Array.Copy(body, (top + y) * canvasWidth + left, under, y * width, width);
        Color[] lifted = PlainFist(under, mask, width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int a = mask[y * width + x].A;
                if (a == 0) continue;
                int i = (top + y) * canvasWidth + left + x;
                body[i] = Scaled(body[i], 255 - a);
            }
        }
        Over(held, canvasWidth, lifted, width, height, left, top);
        return new Vector2(left + width / 2f, top);
    }

    /// <summary>The shade the base's plain fist is kept at in its picture's colour, as a share of this: tools/import_parts.py's FIST_UNIT.</summary>
    private const float FistUnit = 200f;

    /// <summary>How far in from the fist's edge its own outline is kept, and how far round each pixel inside it the plain fist looks to smooth its lines away.</summary>
    private const int FistEdge = 2;
    private const int FistSmooth = 2;

    /// <summary>
    /// The fist the body and the armor draw (premultiplied, under the mask), made plain: its own
    /// outline and silhouette kept, and inside it every colour the median of those round it, which
    /// takes away its thin lines (the knuckles, the creases) and keeps its materials as they are (the
    /// skin, a glove), then shaded with the base's plain fist (the mask's grey, a share of FistUnit: a
    /// hard shadow, a thin light, the fingers parted by lines). Faded in by the mask's alpha.
    /// </summary>
    private static Color[] PlainFist(Color[] under, Color[] mask, int width, int height)
    {
        int n = width * height;
        var inFist = new bool[n];
        var colour = new Vector3[n];
        for (int k = 0; k < n; k++)
        {
            Color c = under[k];
            if (mask[k].A == 0 || c.A < 128) continue;
            inFist[k] = true;
            colour[k] = new Vector3(c.R, c.G, c.B) * (255f / c.A);
        }

        // Its outline: what's within FistEdge of where the fist ends.
        var edge = new bool[n];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                edge[y * width + x] = inFist[y * width + x] && Near(inFist, width, height, x, y, FistEdge, inside: false);

        var plain = new Color[n];
        var reds = new List<float>();
        var greens = new List<float>();
        var blues = new List<float>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int k = y * width + x;
                if (!inFist[k]) continue;
                Color m = mask[k];
                Vector3 shown = colour[k];
                if (!edge[k])
                {
                    reds.Clear();
                    greens.Clear();
                    blues.Clear();
                    for (int ny = Math.Max(0, y - FistSmooth); ny <= Math.Min(height - 1, y + FistSmooth); ny++)
                    {
                        for (int nx = Math.Max(0, x - FistSmooth); nx <= Math.Min(width - 1, x + FistSmooth); nx++)
                        {
                            int j = ny * width + nx;
                            if (!inFist[j] || edge[j]) continue;
                            reds.Add(colour[j].X);
                            greens.Add(colour[j].Y);
                            blues.Add(colour[j].Z);
                        }
                    }
                    var smooth = new Vector3(Median(reds), Median(greens), Median(blues));
                    float share = m.R * 255f / m.A / FistUnit;
                    shown = Vector3.Clamp(smooth * share, Vector3.Zero, new Vector3(255f));
                }
                float alpha = under[k].A / 255f * (m.A / 255f);
                Vector3 premultiplied = shown * alpha;
                plain[k] = new Color((int)MathF.Round(premultiplied.X), (int)MathF.Round(premultiplied.Y), (int)MathF.Round(premultiplied.Z), (int)MathF.Round(alpha * 255f));
            }
        }
        return plain;
    }

    /// <summary>Whether a pixel has, within reach, one that's in the set (inside) or one that isn't (or the picture's edge).</summary>
    private static bool Near(bool[] set, int width, int height, int x, int y, int reach, bool inside)
    {
        for (int dy = -reach; dy <= reach; dy++)
        {
            for (int dx = -reach; dx <= reach; dx++)
            {
                int ny = y + dy, nx = x + dx;
                bool there = ny >= 0 && nx >= 0 && ny < height && nx < width && set[ny * width + nx];
                if (there == inside) return true;
            }
        }
        return false;
    }

    /// <summary>The middle of some values (sorting them).</summary>
    private static float Median(List<float> values)
    {
        values.Sort();
        return values.Count == 0 ? 0f : values[values.Count / 2];
    }

    /// <summary>A premultiplied colour at a share of itself, 0 to 255.</summary>
    private static Color Scaled(Color c, int share) =>
        new((c.R * share + 127) / 255, (c.G * share + 127) / 255, (c.B * share + 127) / 255, (c.A * share + 127) / 255);

    /// <summary>Lays a premultiplied picture over a premultiplied canvas: what's under shows through by what the picture leaves.</summary>
    private static void Over(Color[] canvas, int canvasWidth, Color[] pixels, int width, int height, int left, int top)
    {
        for (int y = 0; y < height; y++)
        {
            int row = (top + y) * canvasWidth + left;
            for (int x = 0; x < width; x++)
            {
                Color s = pixels[y * width + x];
                if (s.A == 0) continue;
                if (s.A == 255)
                {
                    canvas[row + x] = s;
                    continue;
                }
                Color d = canvas[row + x];
                int keep = 255 - s.A;
                canvas[row + x] = new Color(
                    Math.Min(255, s.R + (d.R * keep + 127) / 255),
                    Math.Min(255, s.G + (d.G * keep + 127) / 255),
                    Math.Min(255, s.B + (d.B * keep + 127) / 255),
                    Math.Min(255, s.A + (d.A * keep + 127) / 255));
            }
        }
    }
}
