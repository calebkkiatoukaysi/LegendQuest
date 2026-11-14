using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The d20 as a real 3D object: an icosahedron, twenty flat triangles round a ball with a number on
/// each, lit by a lamp at the top left. DiceTable throws it; this only knows what it looks like and
/// which way round it has to be to show a number.
/// </summary>
/// <remarks>
/// Nothing is loaded for it but the font its numbers are painted in. The twelve corners are the
/// icosahedron's, (0, ±1, ±φ) and its two turns, (±1, ±φ, 0) and (±φ, 0, ±1), and a face is any three
/// corners that are all an edge apart. A face has its own three corners rather than sharing them,
/// so it's lit one flat shade, the way a cut facet catches a lamp. The faces are numbered so
/// opposite ones add up to 21, the way a real d20's are: the odd numbers on one half, high and low
/// in turn, and the even ones opposite them.
/// The faces' pictures are painted once, as it loads, onto one sheet of twenty triangles, five by
/// four: ivory with a lighter rim and an ink edge (the old sprite die's ivory and ink), and the
/// number in ink across the middle, its top towards the face's first corner. 6 and 9 are underlined
/// so they can't be taken for each other.
/// </remarks>
public sealed class DieModel : IDisposable
{
    /// <summary>How far the middle of a face is from the middle of the die, for a die whose corners are 1 from it. A die lying on a face has its middle this high.</summary>
    public const float FaceDistance = 0.7947f;

    /// <summary>The golden ratio, which the icosahedron's corners are made of.</summary>
    private static readonly float Phi = (1f + MathF.Sqrt(5f)) / 2f;

    /// <summary>The faces' sheet: a cell per face, in texels, and how many cells across and down.</summary>
    private const int Cell = 160;
    private const int Columns = 5;
    private const int Rows = 4;

    /// <summary>How far a face's triangle is in from its cell's sides, how wide its ink edge is, and how wide the lighter rim inside that, in texels.</summary>
    private const float CellMargin = 4f;
    private const float EdgeWidth = 1.8f;
    private const float RimWidth = 5f;

    /// <summary>How tall a number's digits are, as a share of its face's height; and the line under a 6 or a 9: how far under the digit, how thick, and how wide as a share of the digit.</summary>
    private const float NumberHeight = 0.33f;
    private const float UnderlineGap = 5f;
    private const float UnderlineThickness = 4f;
    private const float UnderlineWidth = 0.8f;

    /// <summary>The ivory faces, and the lighter rim round each. The edges and the numbers are ink.</summary>
    private static readonly Color Ivory = new(244, 236, 222);
    private static readonly Color Rim = new(255, 252, 244);

    /// <summary>The lamp, from the top left and in front: a warm light with a shine. Then a dim, cool fill from the bottom right, and the room's own light.</summary>
    private static readonly Vector3 LampDirection = Vector3.Normalize(new Vector3(0.55f, -0.6f, -0.58f));
    private static readonly Vector3 LampColour = new(0.7f, 0.66f, 0.6f);
    private static readonly Vector3 ShineColour = new(0.42f, 0.4f, 0.36f);
    private static readonly Vector3 FillDirection = Vector3.Normalize(new Vector3(-0.5f, 0.6f, -0.6f));
    private static readonly Vector3 FillColour = new(0.14f, 0.15f, 0.2f);
    private static readonly Vector3 RoomLight = new(0.45f, 0.43f, 0.46f);
    private const float ShineSharpness = 20f;

    private readonly Matrix[] _faceUp = new Matrix[21];

    private GraphicsDevice _device;
    private VertexBuffer _vertices;
    private Texture2D _faces;
    private BasicEffect _effect;

    /// <summary>Builds the die and paints its faces.</summary>
    /// <param name="device">The graphics device.</param>
    /// <param name="numbers">The font the numbers are painted in.</param>
    public void Load(GraphicsDevice device, SpriteFont numbers)
    {
        _device = device;
        _faces = PaintFaces(device, numbers);

        Vector3[] corners = Corners();
        List<int[]> faces = Faces(corners);
        int[] numbering = Numbering(corners, faces);

        var vertices = new VertexPositionNormalTexture[faces.Count * 3];
        for (int f = 0; f < faces.Count; f++)
        {
            Vector3 apex = corners[faces[f][0]], second = corners[faces[f][1]], third = corners[faces[f][2]];
            Vector3 middle = (apex + second + third) / 3f;
            Vector3 normal = Vector3.Normalize(middle);
            Vector3 up = Vector3.Normalize(apex - middle);
            Vector3 right = Vector3.Cross(up, normal);
            int number = numbering[f];

            // Turned so this face looks at the camera (+Z) with its top up (+Y). The rows are where
            // the face's right, up and out go, so its transpose takes them back to the axes.
            _faceUp[number] = Matrix.Transpose(new Matrix(
                right.X, right.Y, right.Z, 0f,
                up.X, up.Y, up.Z, 0f,
                normal.X, normal.Y, normal.Z, 0f,
                0f, 0f, 0f, 1f));

            (Vector2 top, Vector2 left, Vector2 rightCorner) = SheetCorners(number);
            bool secondOnLeft = Vector3.Dot(second - middle, right) < 0f;
            vertices[f * 3] = new VertexPositionNormalTexture(apex, normal, top);
            vertices[f * 3 + 1] = new VertexPositionNormalTexture(second, normal, secondOnLeft ? left : rightCorner);
            vertices[f * 3 + 2] = new VertexPositionNormalTexture(third, normal, secondOnLeft ? rightCorner : left);
        }

        _vertices = new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
        _vertices.SetData(vertices);

        _effect = new BasicEffect(device)
        {
            TextureEnabled = true,
            Texture = _faces,
            LightingEnabled = true,
            PreferPerPixelLighting = true,
            AmbientLightColor = RoomLight,
            SpecularColor = Vector3.One,
            SpecularPower = ShineSharpness,
        };
        _effect.DirectionalLight0.Enabled = true;
        _effect.DirectionalLight0.Direction = LampDirection;
        _effect.DirectionalLight0.DiffuseColor = LampColour;
        _effect.DirectionalLight0.SpecularColor = ShineColour;
        _effect.DirectionalLight1.Enabled = true;
        _effect.DirectionalLight1.Direction = FillDirection;
        _effect.DirectionalLight1.DiffuseColor = FillColour;
        _effect.DirectionalLight1.SpecularColor = Vector3.Zero;
        _effect.DirectionalLight2.Enabled = false;
    }

    /// <summary>The turn that puts a number's face towards the camera, the right way up.</summary>
    /// <param name="number">1 to 20.</param>
    public Matrix FaceUp(int number) => _faceUp[number];

    /// <summary>Draws the die. The render state (depth, culling, blending) is the caller's.</summary>
    /// <param name="world">Where it is, which way round and how big: its corners are 1 from its middle before this.</param>
    /// <param name="view">The camera.</param>
    /// <param name="projection">The camera's lens.</param>
    /// <param name="alpha">How solid it is.</param>
    /// <param name="glow">A light of its own, added over the lamp's (black for none).</param>
    public void Draw(Matrix world, Matrix view, Matrix projection, float alpha, Vector3 glow)
    {
        _effect.World = world;
        _effect.View = view;
        _effect.Projection = projection;
        _effect.Alpha = alpha;
        _effect.EmissiveColor = glow;

        _device.SetVertexBuffer(_vertices);
        foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertices.VertexCount / 3);
        }
    }

    /// <summary>Lets go of the die's buffer, sheet and effect.</summary>
    public void Dispose()
    {
        _vertices?.Dispose();
        _faces?.Dispose();
        _effect?.Dispose();
    }

    /// <summary>The twelve corners, 1 from the middle.</summary>
    private static Vector3[] Corners()
    {
        var corners = new List<Vector3>();
        foreach (float a in new[] { -1f, 1f })
            foreach (float b in new[] { -Phi, Phi })
            {
                corners.Add(new Vector3(0f, a, b));
                corners.Add(new Vector3(a, b, 0f));
                corners.Add(new Vector3(b, 0f, a));
            }
        return corners.ConvertAll(Vector3.Normalize).ToArray();
    }

    /// <summary>The twenty faces: every three corners an edge apart, wound clockwise seen from outside, which is the way round the graphics card keeps.</summary>
    private static List<int[]> Faces(Vector3[] corners)
    {
        float edge = float.MaxValue;
        for (int i = 1; i < corners.Length; i++) edge = MathF.Min(edge, Vector3.DistanceSquared(corners[0], corners[i]));
        bool Joined(int i, int j) => Vector3.DistanceSquared(corners[i], corners[j]) < edge * 1.01f;

        var faces = new List<int[]>();
        for (int i = 0; i < corners.Length; i++)
            for (int j = i + 1; j < corners.Length; j++)
                for (int k = j + 1; k < corners.Length; k++)
                {
                    if (!Joined(i, j) || !Joined(j, k) || !Joined(i, k)) continue;
                    Vector3 facing = Vector3.Cross(corners[j] - corners[i], corners[k] - corners[i]);
                    bool outward = Vector3.Dot(facing, corners[i] + corners[j] + corners[k]) > 0f;
                    faces.Add(outward ? new[] { i, k, j } : new[] { i, j, k });
                }
        return faces;
    }

    /// <summary>The number on each face. The half that faces one way get the odd numbers, high and low in turn, and each face opposite one of them gets what makes 21.</summary>
    private static int[] Numbering(Vector3[] corners, List<int[]> faces)
    {
        // Any way that isn't square to a face splits them ten and ten.
        Vector3 way = Vector3.Normalize(new Vector3(0.3f, 0.5f, 1f));
        Vector3[] middles = faces.ConvertAll(f => corners[f[0]] + corners[f[1]] + corners[f[2]]).ToArray();
        var half = new List<int>();
        for (int f = 0; f < faces.Count; f++)
            if (Vector3.Dot(middles[f], way) > 0f) half.Add(f);
        half.Sort((a, b) => Vector3.Dot(middles[b], way).CompareTo(Vector3.Dot(middles[a], way)));

        var numbering = new int[faces.Count];
        for (int k = 0; k < half.Count; k++)
        {
            int f = half[k];
            int number = k % 2 == 0 ? k + 1 : 20 - k;
            numbering[f] = number;
            for (int g = 0; g < faces.Count; g++)
                if (Vector3.DistanceSquared(middles[g], -middles[f]) < 1e-4f) numbering[g] = 21 - number;
        }
        return numbering;
    }

    /// <summary>A face's triangle in its cell, in texels: the top corner, then the bottom left and right.</summary>
    private static (Vector2 Top, Vector2 Left, Vector2 Right) CellTriangle()
    {
        float side = Cell - CellMargin * 2f;
        float height = side * MathF.Sqrt(3f) / 2f;
        float top = (Cell - height) / 2f;
        return (new Vector2(Cell / 2f, top), new Vector2(CellMargin, top + height), new Vector2(Cell - CellMargin, top + height));
    }

    /// <summary>Where a number's cell is on the sheet, in texels.</summary>
    private static Vector2 CellCorner(int number) => new((number - 1) % Columns * Cell, (number - 1) / Columns * Cell);

    /// <summary>A number's triangle on the sheet, as texture coordinates.</summary>
    private static (Vector2 Top, Vector2 Left, Vector2 Right) SheetCorners(int number)
    {
        var size = new Vector2(Cell * Columns, Cell * Rows);
        Vector2 corner = CellCorner(number);
        (Vector2 top, Vector2 left, Vector2 right) = CellTriangle();
        return ((corner + top) / size, (corner + left) / size, (corner + right) / size);
    }

    /// <summary>The sheet of faces: each cell's triangle in ivory, rimmed and edged, then the numbers on them in ink.</summary>
    private static Texture2D PaintFaces(GraphicsDevice device, SpriteFont numbers)
    {
        int width = Cell * Columns, height = Cell * Rows;
        (Vector2 top, Vector2 left, Vector2 right) = CellTriangle();

        var plain = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var at = new Vector2(x % Cell + 0.5f, y % Cell + 0.5f);
                float inside = MathF.Min(InFrom(at, top, left, right), MathF.Min(InFrom(at, left, right, top), InFrom(at, right, top, left)));
                Color face = Color.Lerp(Rim, Ivory, MathHelper.Clamp((inside - EdgeWidth) / RimWidth, 0f, 1f));
                plain[y * width + x] = Color.Lerp(Palette.Ink, face, MathHelper.Clamp(inside - EdgeWidth + 0.5f, 0f, 1f));
            }

        RenderTargetBinding[] was = device.GetRenderTargets();
        using var ground = new Texture2D(device, width, height);
        using var pixel = new Texture2D(device, 1, 1);
        using var sheet = new RenderTarget2D(device, width, height);
        using var batch = new SpriteBatch(device);
        ground.SetData(plain);
        pixel.SetData(new[] { Color.White });

        device.SetRenderTarget(sheet);
        device.Clear(Palette.Ink);
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        batch.Draw(ground, Vector2.Zero, Color.White);
        SpriteFont.Glyph digit = numbers.GetGlyphs()['0'];
        for (int number = 1; number <= 20; number++)
            PaintNumber(batch, numbers, digit, pixel, number);
        batch.End();
        device.SetRenderTargets(was);

        // Copied off the target, so it's an ordinary texture that nothing can clear.
        var painted = new Color[width * height];
        sheet.GetData(painted);
        var faces = new Texture2D(device, width, height);
        faces.SetData(painted);
        return faces;
    }

    /// <summary>One number in the middle of its face, its digits the height NumberHeight says, and the line under it if it's a 6 or a 9.</summary>
    private static void PaintNumber(SpriteBatch batch, SpriteFont numbers, SpriteFont.Glyph digit, Texture2D pixel, int number)
    {
        (Vector2 top, Vector2 left, _) = CellTriangle();
        Vector2 middle = CellCorner(number) + new Vector2(top.X, top.Y + (left.Y - top.Y) * 2f / 3f);
        float scale = NumberHeight * (left.Y - top.Y) / digit.BoundsInTexture.Height;
        float tall = digit.BoundsInTexture.Height * scale;

        string text = number.ToString();
        float wide = numbers.MeasureString(text).X * scale;
        var at = new Vector2(middle.X - wide / 2f, middle.Y - tall / 2f - digit.Cropping.Y * scale);
        batch.DrawString(numbers, text, at, Palette.Ink, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        if (number is 6 or 9)
        {
            float line = digit.BoundsInTexture.Width * scale * UnderlineWidth;
            batch.Draw(pixel, new Vector2(middle.X - line / 2f, middle.Y + tall / 2f + UnderlineGap), null, Palette.Ink, 0f, Vector2.Zero,
                new Vector2(line, UnderlineThickness), SpriteEffects.None, 0f);
        }
    }

    /// <summary>How far a point is in from a triangle's side, towards its third corner. Less than 0 is outside.</summary>
    private static float InFrom(Vector2 point, Vector2 from, Vector2 to, Vector2 third)
    {
        Vector2 along = Vector2.Normalize(to - from);
        var across = new Vector2(-along.Y, along.X);
        if (Vector2.Dot(across, third - from) < 0f) across = -across;
        return Vector2.Dot(across, point - from);
    }
}
