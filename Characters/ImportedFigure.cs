using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.World;

namespace LegendQuest.Characters;

/// <summary>
/// One imported picture made ready to draw: the figure at the size it stands in a room, where its
/// feet are in that, how big it stands and the shadow under it, and its face cut out for a portrait
/// of whatever size is asked for.
/// </summary>
/// <remarks>
/// <para>
/// Made from the picture's pixels, premultiplied, at the size the file has (a standard adult
/// ImportedArt.FileHeight tall), so anything that puts a figure together itself can make one the
/// same way. The room's copy is shrunk once, here, to the size it stands on the canvas (a standard
/// adult ImportedArt.RoomHeight tall), so the room's point-sampled batch draws it a texel to a
/// canvas pixel, crisp, with the feet on a whole texel. Shrinking averages the area each new pixel
/// covers, and growing blends the nearest two, both on premultiplied colour so the edges don't
/// darken.
/// </para>
/// <para>
/// A portrait is a box round the middle of the head box, its longer side times FaceReach tall and
/// starting FaceLift of that side above it, as wide as the well it's for is shaped. Each size is
/// made the first time it's asked for and kept, so it's drawn a texel to a pixel too.
/// tools/import_character.py's preview cuts the same box.
/// </para>
/// </remarks>
public sealed class ImportedFigure
{
    /// <summary>How many world pixels a texel of Picture is: a canvas pixel in a room.</summary>
    public const float Texel = 1f / ZoneCatalog.RoomScale;

    /// <summary>How tall the portrait's box is against the head box's longer side, and how far over the head box it starts.</summary>
    private const float FaceReach = 1.4f;
    private const float FaceLift = 0.1f;

    /// <summary>The widest a portrait's box is kept for, against its height. A wider well gets transparent sides.</summary>
    private const float WidestFace = 1.5f;

    /// <summary>A person's shadow: how wide against their height, and how deep against its width (03-art-overhaul.md §2.4, 60 by 18 under a figure 196 tall).</summary>
    private const float PersonShadow = 60f / 196f;
    private const float PersonShadowDepth = 0.3f;

    /// <summary>A creature's shadow: how wide against its length, and how deep against its width.</summary>
    private const float CreatureShadow = 0.85f;
    private const float CreatureShadowDepth = 0.22f;

    private readonly GraphicsDevice _device;
    private readonly Dictionary<Point, Texture2D> _portraits = new();

    /// <summary>The part of the picture round the head that's kept for the portraits, and where it is on the picture.</summary>
    private readonly Color[] _face;
    private readonly Rectangle _faceArea;

    /// <summary>The middle of the head box across, the top of the portrait's box, and the head box's longer side, in the picture's pixels.</summary>
    private readonly float _faceMiddle;
    private readonly float _faceTop;
    private readonly float _headSide;

    /// <summary>Makes a figure ready to draw from its picture.</summary>
    /// <param name="device">The device to make the textures on.</param>
    /// <param name="info">Its size, feet, head box and kind.</param>
    /// <param name="pixels">The picture, premultiplied, info.Size big, row by row.</param>
    /// <param name="fileHeight">How tall a standard adult is in the picture's pixels: ImportedArt.FileHeight for an imported picture, the parts' own for one put together from them.</param>
    /// <param name="hasBody">Whether it has a body to stand in a room with. One put together from parts before its armor's made has only a face to give the portraits.</param>
    /// <param name="onBase">For a figure from parts, the base it's built on (base-male, base-female), so it plays the clips worked out for its own proportions.</param>
    public ImportedFigure(GraphicsDevice device, ImportedArtInfo info, Color[] pixels, float fileHeight = ImportedArt.FileHeight, bool hasBody = true,
        IReadOnlyList<RigComposer.Layer> layers = null, Vector2? hand = null, IReadOnlyDictionary<string, Vector2> joints = null, float rigPx = 0f,
        string onBase = null)
    {
        _device = device;
        HasBody = hasBody;
        Base = onBase;
        int width = info.Size[0], height = info.Size[1];

        float shrink = ImportedArt.RoomHeight / fileHeight;
        int roomWidth = Math.Max(1, (int)MathF.Round(width * shrink)), roomHeight = Math.Max(1, (int)MathF.Round(height * shrink));
        Picture = new Texture2D(device, roomWidth, roomHeight);
        Picture.SetData(Scaled(pixels, width, height, roomWidth, roomHeight));
        Feet = new Vector2(MathF.Round(info.Feet[0] * roomWidth / width), MathF.Round(info.Feet[1] * roomHeight / height));
        Size = new Vector2(roomWidth, Feet.Y) * Texel;
        float shadow = info.Creature ? Size.X * CreatureShadow : Size.Y * PersonShadow;
        Shadow = new Vector2(shadow, shadow * (info.Creature ? CreatureShadowDepth : PersonShadowDepth));

        _headSide = Math.Max(info.Head[2], info.Head[3]);
        _faceMiddle = info.Head[0] + info.Head[2] / 2f;
        _faceTop = info.Head[1] - _headSide * FaceLift;
        float tall = _headSide * FaceReach, wide = tall * WidestFace;
        var kept = new Rectangle((int)MathF.Floor(_faceMiddle - wide / 2f), (int)MathF.Floor(_faceTop), (int)MathF.Ceiling(wide) + 1, (int)MathF.Ceiling(tall) + 1);
        _faceArea = Rectangle.Intersect(kept, new Rectangle(0, 0, width, height));
        _face = new Color[_faceArea.Width * _faceArea.Height];
        for (int y = 0; y < _faceArea.Height; y++)
            Array.Copy(pixels, (_faceArea.Y + y) * width + _faceArea.X, _face, y * _faceArea.Width, _faceArea.Width);

        // Each piece is shrunk on the whole picture's canvas, as the picture is, so they line up to the
        // pixel, then trimmed to what's on it.
        var rig = new List<Piece>();
        if (layers is not null)
        {
            var across = new Vector2((float)roomWidth / width, (float)roomHeight / height);
            foreach (RigComposer.Layer layer in layers)
            {
                Color[] small = Scaled(layer.Pixels, width, height, roomWidth, roomHeight);
                Rectangle box = Trimmed(small, roomWidth, roomHeight);
                if (box.IsEmpty) continue;
                var cut = new Color[box.Width * box.Height];
                for (int y = 0; y < box.Height; y++) Array.Copy(small, (box.Y + y) * roomWidth + box.X, cut, y * box.Width, box.Width);
                var texture = new Texture2D(device, box.Width, box.Height);
                texture.SetData(cut);
                rig.Add(new Piece(layer.Key, layer.Bone, texture, new Vector2(box.X, box.Y), layer.Pivot * across));
            }
            if (joints is not null)
            {
                var placed = new Dictionary<string, Vector2>(StringComparer.Ordinal);
                foreach ((string bone, Vector2 joint) in joints) placed[bone] = joint * across;
                Joints = placed;
                RigPx = rigPx * across.Y;
            }
        }
        Rig = rig;
        if (hand is Vector2 h) Hand = h * new Vector2((float)roomWidth / width, (float)roomHeight / height);
    }

    /// <summary>Where the near hand is on the picture, for a figure from parts (a cast glows there); null for a whole picture.</summary>
    public Vector2? Hand { get; }

    /// <summary>One piece of a figure at the rooms' size: which it is and the bone it moves with; its picture, trimmed; where its top left is on the whole picture; and its bone's joint, on the whole picture.</summary>
    /// <param name="Key">Which piece: a bone's name, or RigComposer's Cape, Weapon or Offhand.</param>
    /// <param name="Bone">The bone it moves with.</param>
    /// <param name="Picture">Its picture.</param>
    /// <param name="Offset">Where its top left is on the whole picture.</param>
    /// <param name="Pivot">Its bone's joint, on the whole picture: what it turns about.</param>
    public sealed record Piece(string Key, string Bone, Texture2D Picture, Vector2 Offset, Vector2 Pivot);

    /// <summary>For a figure from parts, every bone's joint on the whole picture, by the bone's name; null for a whole picture.</summary>
    public IReadOnlyDictionary<string, Vector2> Joints { get; }

    /// <summary>For a figure from parts, how many of the picture's pixels one of the clips' units is.</summary>
    public float RigPx { get; }

    /// <summary>For a figure from parts, the base it's built on (base-male, base-female); null for a whole picture.</summary>
    public string Base { get; }

    /// <summary>The pieces it moves in, back to front, for a figure put together from parts; none for a whole picture, which moves as one.</summary>
    public IReadOnlyList<Piece> Rig { get; }

    /// <summary>Lets go of its pictures: the whole one, its pieces' and its portraits. Only for one made for a while, like a menu's (PartCatalog.Large); the rooms' are kept for the game.</summary>
    public void Dispose()
    {
        Picture.Dispose();
        foreach (Piece piece in Rig) piece.Picture.Dispose();
        foreach (Texture2D portrait in _portraits.Values) portrait.Dispose();
        _portraits.Clear();
    }

    /// <summary>The box round what isn't clear in a picture; empty if it's all clear.</summary>
    private static Rectangle Trimmed(Color[] pixels, int width, int height)
    {
        int x0 = width, y0 = height, x1 = -1, y1 = -1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].A == 0) continue;
                if (x < x0) x0 = x;
                if (x > x1) x1 = x;
                if (y < y0) y0 = y;
                y1 = y;
            }
        }
        return x1 < 0 ? Rectangle.Empty : new Rectangle(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
    }

    /// <summary>The figure as it stands in a room, a texel to a canvas pixel, facing right.</summary>
    public Texture2D Picture { get; }

    /// <summary>Whether it has a body to stand in a room with. Without one, only the portraits use it, and the room draws whoever it is as they were.</summary>
    public bool HasBody { get; }

    /// <summary>Where the feet are in Picture, on a whole texel: across, and down to the bottom of the soles.</summary>
    public Vector2 Feet { get; }

    /// <summary>How big the figure stands: how wide, and how tall up from the feet, in world pixels.</summary>
    public Vector2 Size { get; }

    /// <summary>How wide and deep the shadow under the feet is, in world pixels.</summary>
    public Vector2 Shadow { get; }

    /// <summary>The face for a portrait's well of a size: the box round the head, shaped like the well, at the well's size. Made the first time and kept.</summary>
    /// <param name="size">The well's size, in pixels.</param>
    public Texture2D Portrait(Point size)
    {
        if (_portraits.TryGetValue(size, out Texture2D portrait)) return portrait;

        float tall = _headSide * FaceReach, wide = tall * size.X / size.Y;
        var box = new Rectangle((int)MathF.Round(_faceMiddle - wide / 2f), (int)MathF.Round(_faceTop), Math.Max(1, (int)MathF.Round(wide)), Math.Max(1, (int)MathF.Round(tall)));
        var crop = new Color[box.Width * box.Height];
        for (int y = 0; y < box.Height; y++)
        {
            for (int x = 0; x < box.Width; x++)
            {
                int px = box.X + x - _faceArea.X, py = box.Y + y - _faceArea.Y;
                if (px >= 0 && py >= 0 && px < _faceArea.Width && py < _faceArea.Height) crop[y * box.Width + x] = _face[py * _faceArea.Width + px];
            }
        }

        portrait = new Texture2D(_device, size.X, size.Y);
        portrait.SetData(Scaled(crop, box.Width, box.Height, size.X, size.Y));
        _portraits[size] = portrait;
        return portrait;
    }

    /// <summary>A premultiplied picture at a new size, across and then down: each new pixel the average of the area it covers where it's shrinking, or a blend of the nearest two where it's growing.</summary>
    internal static Color[] Scaled(Color[] pixels, int width, int height, int toWidth, int toHeight)
    {
        (int First, float[] Weights)[] across = Taps(width, toWidth), down = Taps(height, toHeight);

        var wide = new float[height * toWidth * 4];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < toWidth; x++)
            {
                (int first, float[] weights) = across[x];
                int at = (y * toWidth + x) * 4;
                for (int k = 0; k < weights.Length; k++)
                {
                    Color c = pixels[y * width + first + k];
                    float w = weights[k];
                    wide[at] += c.R * w;
                    wide[at + 1] += c.G * w;
                    wide[at + 2] += c.B * w;
                    wide[at + 3] += c.A * w;
                }
            }
        }

        var scaled = new Color[toWidth * toHeight];
        for (int y = 0; y < toHeight; y++)
        {
            (int first, float[] weights) = down[y];
            for (int x = 0; x < toWidth; x++)
            {
                float r = 0f, g = 0f, b = 0f, a = 0f;
                for (int k = 0; k < weights.Length; k++)
                {
                    int from = ((first + k) * toWidth + x) * 4;
                    float w = weights[k];
                    r += wide[from] * w;
                    g += wide[from + 1] * w;
                    b += wide[from + 2] * w;
                    a += wide[from + 3] * w;
                }
                int alpha = Math.Clamp((int)MathF.Round(a), 0, 255);
                scaled[y * toWidth + x] = new Color(
                    Math.Min(alpha, Math.Clamp((int)MathF.Round(r), 0, 255)),
                    Math.Min(alpha, Math.Clamp((int)MathF.Round(g), 0, 255)),
                    Math.Min(alpha, Math.Clamp((int)MathF.Round(b), 0, 255)),
                    alpha);
            }
        }
        return scaled;
    }

    /// <summary>Which pixels each new one takes from a row (or a column) going from one length to another, starting where, and how much of each.</summary>
    private static (int First, float[] Weights)[] Taps(int from, int to)
    {
        var taps = new (int First, float[] Weights)[to];
        double step = (double)from / to;
        for (int i = 0; i < to; i++)
        {
            if (step >= 1.0)
            {
                double start = i * step, end = start + step;
                int first = (int)Math.Floor(start), last = Math.Min(from - 1, (int)Math.Ceiling(end) - 1);
                var weights = new float[last - first + 1];
                for (int k = first; k <= last; k++) weights[k - first] = (float)(Math.Max(0.0, Math.Min(end, k + 1) - Math.Max(start, k)) / step);
                taps[i] = (first, weights);
                continue;
            }

            double centre = (i + 0.5) * step - 0.5;
            int left = (int)Math.Floor(centre);
            float t = (float)(centre - left);
            taps[i] = left < 0 ? (0, new[] { 1f })
                : left >= from - 1 ? (from - 1, new[] { 1f })
                : (left, new[] { 1f - t, t });
        }
        return taps;
    }
}
