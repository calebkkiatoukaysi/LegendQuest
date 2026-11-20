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
/// <param name="Bone">For one bone's piece of a body or an armor, the bone; null for a part that's whole.</param>
/// <param name="HeldKind">The held item's kind from its source, or null for older parts.</param>
public sealed record PartImage(string Id, PartSlot Slot, string Base, int Width, int Height, Color[] Pixels, Color[] Mask, Vector2 Pivot, Vector3 SkinRatio, Vector3?[] Tones,
    string Bone = null, string HeldKind = null);

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
/// Puts a figure together from its parts, the way the game draws it: each part at its socket, moved
/// and scaled by the morph, coloured (Paint), and laid into the picture of the piece it moves with.
/// A body or an armor comes in its bones' pieces, each into its bone's picture; the head, the face
/// and the hair or helm into the head's; the cape into one behind everything that moves with the
/// torso; and what each hand holds into one just under that hand. Each picture turns about its
/// bone's joint (Skeleton), and the clips (ClipLibrary) move them.
/// </summary>
public static class RigComposer
{
    /// <summary>The cape's picture, behind everything, moving with the torso.</summary>
    public const string Cape = "cape";

    /// <summary>What the near hand holds, just under it, moving with it.</summary>
    public const string Weapon = "weapon";

    /// <summary>What the far hand holds, just under it, moving with it.</summary>
    public const string Offhand = "offhand";

    /// <summary>
    /// The near foot drawn side on, toes the way the figure faces: the far foot's picture (which is
    /// painted that way) at the near ankle, a little bigger for being nearer. The near foot's own is
    /// three-quarters on, toes to the camera, which is right standing and wrong running; a clip that
    /// swaps foot_near for "side" (the jog and the run) is drawn with this one instead.
    /// </summary>
    public const string SideFoot = NearFoot + ".side";

    /// <summary>How much bigger the side-on near foot is than the far foot it's made from.</summary>
    private const float SideFootScale = 1.08f;

    private const string NearFoot = "foot_near", FarFoot = "foot_far";

    /// <summary>The bone the cape moves with, and the hands that hold things.</summary>
    private const string Torso = "torso", NearHand = "hand_near", FarHand = "hand_far", HeadBone = "head";

    /// <summary>A figure put together: the whole picture (premultiplied), its size, the root, the head box, its pieces back to front, the near hand, and each bone's joint, all on the picture.</summary>
    /// <param name="Pixels">The whole figure, premultiplied, row by row.</param>
    /// <param name="Width">How wide it is.</param>
    /// <param name="Height">How tall it is.</param>
    /// <param name="Root">The root (the ground between the feet).</param>
    /// <param name="Head">The box round the head.</param>
    /// <param name="Layers">Its pieces back to front.</param>
    /// <param name="Hand">Where the near hand is.</param>
    /// <param name="Joints">Each bone's joint, by the bone's name.</param>
    public sealed record Result(Color[] Pixels, int Width, int Height, Vector2 Root, Rectangle Head, IReadOnlyList<Layer> Layers, Vector2 Hand, IReadOnlyDictionary<string, Vector2> Joints);

    /// <summary>One piece of a figure on the whole picture's canvas: which it is (a bone, or the cape or what a hand holds), the bone it moves with, its pixels, and its bone's joint.</summary>
    /// <param name="Key">Which piece: a bone's name, Cape, Weapon or Offhand.</param>
    /// <param name="Bone">The bone it moves with.</param>
    /// <param name="Pixels">Its pixels on the whole canvas, premultiplied.</param>
    /// <param name="Pivot">Its bone's joint on the canvas.</param>
    public sealed record Layer(string Key, string Bone, Color[] Pixels, Vector2 Pivot);

    /// <summary>
    /// Puts parts together into a figure on the base whose sockets, head box and joints these are:
    /// each part at its slot's socket (Sockets.Of) moved by the morph, scaled by it and by its fit,
    /// coloured by its paint, in slot order within each piece.
    /// </summary>
    /// <param name="sockets">The base's sockets, in the parts' pixels.</param>
    /// <param name="head">The base's head box: left, top, width, height.</param>
    /// <param name="joints">The base's joints, by name, in the parts' pixels.</param>
    /// <param name="skeleton">The skeleton: its bones, their joints and the order they're drawn in.</param>
    /// <param name="parts">The parts.</param>
    /// <param name="morph">How the figure's shaped.</param>
    /// <param name="paint">How each part's coloured.</param>
    /// <param name="fit">How much more each part's scaled to fit this base (PartCatalog.Fit); one if null.</param>
    public static Result Compose(IReadOnlyDictionary<string, Vector2> sockets, float[] head, IReadOnlyDictionary<string, Vector2> joints, Skeleton skeleton,
        IEnumerable<PartImage> parts, Morph morph, Func<PartImage, Paint> paint, Func<PartImage, Vector2> fit = null)
    {
        Vector2 root = sockets[Sockets.Root];
        List<PartImage> inOrder = parts.OrderBy(p => p.Slot).ToList();
        // A mark on the skin (freckles, a scar, soot) shows only where what's under it is skin, so
        // only then is each part's skin kept track of.
        bool marked = inOrder.Any(OnSkin);
        var placed = new List<(Color[] Pixels, int Width, int Height, int Left, int Top, string Key, Color[] Mask, bool OnSkin)>();
        foreach (PartImage part in inOrder)
        {
            Vector2 at = morph.Move(sockets[Sockets.Of(part.Slot)], root);
            Vector2 scale = morph.ScaleOf(part.Slot) * (fit?.Invoke(part) ?? Vector2.One);
            int width = Math.Max(1, (int)MathF.Round(part.Width * scale.X));
            int height = Math.Max(1, (int)MathF.Round(part.Height * scale.Y));
            int left = (int)MathF.Round(at.X - part.Pivot.X * width / part.Width);
            int top = (int)MathF.Round(at.Y - part.Pivot.Y * height / part.Height);
            Color[] pixels = Painted(part, paint(part));
            bool resized = width != part.Width || height != part.Height;
            if (resized) pixels = ImportedFigure.Scaled(pixels, part.Width, part.Height, width, height);
            Color[] mask = marked && !OnSkin(part) ? part.Mask : null;
            if (mask is not null && resized) mask = ImportedFigure.Scaled(mask, part.Width, part.Height, width, height);
            placed.Add((pixels, width, height, left, top, KeyOf(part), mask, OnSkin(part)));
        }
        if (placed.Count == 0) throw new ArgumentException("There are no parts to put together.", nameof(parts));

        // The canvas holds every part, and the root under the feet with a row or two to spare.
        int x0 = Math.Min(placed.Min(p => p.Left), (int)MathF.Floor(root.X) - 1), y0 = placed.Min(p => p.Top);
        int x1 = Math.Max(placed.Max(p => p.Left + p.Width), (int)MathF.Ceiling(root.X) + 1);
        int y1 = Math.Max(placed.Max(p => p.Top + p.Height), (int)MathF.Ceiling(root.Y) + 2);
        int canvasWidth = x1 - x0, canvasHeight = y1 - y0;

        // Each piece's own picture, its parts put together in slot order.
        var pieces = new Dictionary<string, Color[]>(StringComparer.Ordinal);
        var skins = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach ((Color[] pixels, int width, int height, int left, int top, string key, Color[] mask, bool onSkin) in placed)
        {
            if (!pieces.TryGetValue(key, out Color[] layer)) pieces[key] = layer = new Color[canvasWidth * canvasHeight];
            if (!marked)
            {
                Over(layer, canvasWidth, pixels, width, height, left - x0, top - y0);
                continue;
            }
            if (!skins.TryGetValue(key, out byte[] skin)) skins[key] = skin = new byte[canvasWidth * canvasHeight];
            if (onSkin) OverSkin(layer, skin, canvasWidth, pixels, width, height, left - x0, top - y0);
            else
            {
                Over(layer, canvasWidth, pixels, width, height, left - x0, top - y0);
                Covered(skin, canvasWidth, pixels, mask, width, height, left - x0, top - y0);
            }
        }
        var at0 = new Vector2(x0, y0);
        var jointAt = new Dictionary<string, Vector2>(StringComparer.Ordinal);
        for (int i = 0; i < skeleton.Count; i++)
            jointAt[skeleton.Names[i]] = morph.Move(joints[skeleton.Joints[i]], root) - at0;

        // Back to front: the cape, then the bones as they're drawn, what each hand holds just under it.
        // A shield is the other way about: it's painted face on, so it goes over the fist that holds it.
        bool shield = inOrder.Any(p => p.Slot == PartSlot.Offhand && (p.HeldKind == "shield" || IsShield(p.Id)));
        var order = new List<string> { Cape };
        foreach (string bone in skeleton.Draw)
        {
            if (bone == NearHand) order.Add(Weapon);
            if (bone == FarHand && !shield) order.Add(Offhand);
            order.Add(bone);
            if (bone == FarHand && shield) order.Add(Offhand);
        }
        var layers = new List<Layer>();
        var canvas = new Color[canvasWidth * canvasHeight];
        foreach (string key in order)
        {
            if (!pieces.TryGetValue(key, out Color[] layer)) continue;
            string bone = RidesOn(key);
            layers.Add(new Layer(key, bone, layer, jointAt[bone]));
            Over(canvas, canvasWidth, layer, canvasWidth, canvasHeight, 0, 0);
            // The near foot's other picture goes with it, in its place in the order; it isn't on the whole picture.
            if (key == NearFoot && pieces.TryGetValue(FarFoot, out Color[] far) && jointAt.ContainsKey(FarFoot))
                layers.Add(new Layer(SideFoot, bone, Moved(far, canvasWidth, canvasHeight, jointAt[FarFoot], jointAt[NearFoot], SideFootScale), jointAt[bone]));
        }

        // The head box goes where the head did: on the neck where the morph moved it, at the head's size.
        Vector2 neck = sockets[Sockets.Neck], neckAt = morph.Move(neck, root);
        float s = morph.HeadScale;
        float headLeft = neckAt.X + (head[0] - neck.X) * s - x0, headTop = neckAt.Y + (head[1] - neck.Y) * s - y0;
        var box = new Rectangle((int)MathF.Floor(headLeft), (int)MathF.Floor(headTop), (int)MathF.Ceiling(head[2] * s), (int)MathF.Ceiling(head[3] * s));
        box = Rectangle.Intersect(box, new Rectangle(0, 0, canvasWidth, canvasHeight));
        return new Result(canvas, canvasWidth, canvasHeight, root - at0, box, layers, morph.Move(sockets[Sockets.NearHand], root) - at0, jointAt);
    }

    /// <summary>A premultiplied picture on the canvas drawn again about another point and bigger or smaller: what was at `from` is at `to`, blended from the nearest four.</summary>
    private static Color[] Moved(Color[] pixels, int width, int height, Vector2 from, Vector2 to, float scale)
    {
        var moved = new Color[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            float sy = from.Y + (y - to.Y) / scale;
            int y0 = (int)MathF.Floor(sy);
            if (y0 < 0 || y0 >= height - 1) continue;
            float fy = sy - y0;
            for (int x = 0; x < width; x++)
            {
                float sx = from.X + (x - to.X) / scale;
                int x0 = (int)MathF.Floor(sx);
                if (x0 < 0 || x0 >= width - 1) continue;
                float fx = sx - x0;
                Color a = pixels[y0 * width + x0], b = pixels[y0 * width + x0 + 1], c = pixels[(y0 + 1) * width + x0], d = pixels[(y0 + 1) * width + x0 + 1];
                if ((a.A | b.A | c.A | d.A) == 0) continue;
                float wa = (1f - fx) * (1f - fy), wb = fx * (1f - fy), wc = (1f - fx) * fy, wd = fx * fy;
                moved[y * width + x] = new Color(
                    (int)MathF.Round(a.R * wa + b.R * wb + c.R * wc + d.R * wd), (int)MathF.Round(a.G * wa + b.G * wb + c.G * wc + d.G * wd),
                    (int)MathF.Round(a.B * wa + b.B * wb + c.B * wc + d.B * wd), (int)MathF.Round(a.A * wa + b.A * wb + c.A * wc + d.A * wd));
            }
        }
        return moved;
    }

    /// <summary>Which piece a part goes into: its bone for a body's or an armor's piece; the head for what's on the head; the cape's, or the held things' own.</summary>
    private static string KeyOf(PartImage part) => part.Bone ?? part.Slot switch
    {
        PartSlot.Cape => Cape,
        PartSlot.Weapon => Weapon,
        PartSlot.Offhand => Offhand,
        PartSlot.Head or PartSlot.Face or PartSlot.Facial or PartSlot.Helm or PartSlot.Marks => HeadBone,
        _ => Torso,
    };

    /// <summary>The bone a piece moves with.</summary>
    public static string RidesOn(string key) => key switch
    {
        Cape => Torso,
        Weapon => NearHand,
        Offhand => FarHand,
        SideFoot => NearFoot,
        _ => key,
    };

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

    /// <summary>Whether what the far hand holds is a shield, by its id (as tools/weapon_alone.py tells one): its face is towards us, so the fist is behind it.</summary>
    public static bool IsShield(string id) => id is not null && (id.Contains("shield", StringComparison.Ordinal) || id.Contains("buckler", StringComparison.Ordinal));

    /// <summary>The ids of the marks on the skin begin with this: freckles, scars, soot, a tattoo.</summary>
    private const string SkinMark = "skin-";

    /// <summary>Whether a part is a mark on the skin, which shows only where there's skin under it: not over an eye, a brow, a beard or the line round the face.</summary>
    private static bool OnSkin(PartImage part) => part.Slot == PartSlot.Facial && part.Id.StartsWith(SkinMark, StringComparison.Ordinal);

    /// <summary>How much of each pixel of a piece is skin once a part's laid on it: the part's own (its mask's red) where the part is, by how solid it is there.</summary>
    private static void Covered(byte[] skin, int canvasWidth, Color[] pixels, Color[] mask, int width, int height, int left, int top)
    {
        for (int y = 0; y < height; y++)
        {
            int row = (top + y) * canvasWidth + left;
            for (int x = 0; x < width; x++)
            {
                int a = pixels[y * width + x].A;
                if (a == 0) continue;
                int own = mask is null ? 0 : mask[y * width + x].R;
                skin[row + x] = (byte)((skin[row + x] * (255 - a) + own * a + 127) / 255);
            }
        }
    }

    /// <summary>A premultiplied picture laid over a piece only as far as the piece is skin there.</summary>
    private static void OverSkin(Color[] canvas, byte[] skin, int canvasWidth, Color[] pixels, int width, int height, int left, int top)
    {
        for (int y = 0; y < height; y++)
        {
            int row = (top + y) * canvasWidth + left;
            for (int x = 0; x < width; x++)
            {
                Color s = pixels[y * width + x];
                int k = skin[row + x];
                if (s.A == 0 || k == 0) continue;
                int r = (s.R * k + 127) / 255, g = (s.G * k + 127) / 255, b = (s.B * k + 127) / 255, a = (s.A * k + 127) / 255;
                Color d = canvas[row + x];
                int keep = 255 - a;
                canvas[row + x] = new Color(
                    Math.Min(255, r + (d.R * keep + 127) / 255),
                    Math.Min(255, g + (d.G * keep + 127) / 255),
                    Math.Min(255, b + (d.B * keep + 127) / 255),
                    Math.Min(255, a + (d.A * keep + 127) / 255));
            }
        }
    }

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
