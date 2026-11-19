using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters.Motion;

/// <summary>
/// Strips of light drawn as triangles, additively: the ribbon a weapon leaves behind it, a bolt's
/// streak, a beam. Begin with the camera's matrix, add quads and strips in world pixels, End.
/// </summary>
/// <remarks>
/// SpriteBatch only draws rectangles, and a ribbon is a band that bends with the swing, so these go
/// through DrawUserPrimitives with a BasicEffect that colours each corner. Colours are premultiplied
/// (a colour times its strength), drawn with Blends.PremultipliedAdditive, so a ribbon lights what's
/// under it and two crossing get brighter, the way the sparks do. Everything's gathered between Begin
/// and End and drawn in one call, so a room full of swings is one draw. Something that should darken
/// as well as light, like a ground marker on a bright floor, is drawn with BlendState.AlphaBlend
/// instead (Begin's blend), its colours still premultiplied.
/// </remarks>
public sealed class RibbonBatch : IDisposable
{
    private readonly GraphicsDevice _device;
    private readonly BasicEffect _effect;
    private VertexPositionColor[] _vertices = new VertexPositionColor[1536];
    private int _count;
    private bool _begun;
    private BlendState _blend;

    /// <summary>A batch for a device.</summary>
    public RibbonBatch(GraphicsDevice device)
    {
        _device = device;
        _effect = new BasicEffect(device) { VertexColorEnabled = true, TextureEnabled = false, LightingEnabled = false };
    }

    /// <summary>Starts gathering, in world pixels through a matrix (the camera's, the shake included).</summary>
    /// <param name="world">The matrix from world pixels to the screen.</param>
    /// <param name="blend">How it's laid over what's there: added (Blends.PremultipliedAdditive) unless it says.</param>
    public void Begin(Matrix world, BlendState blend = null)
    {
        _blend = blend ?? Blends.PremultipliedAdditive;
        Viewport view = _device.Viewport;
        _effect.World = world;
        _effect.View = Matrix.Identity;
        _effect.Projection = Matrix.CreateOrthographicOffCenter(0f, view.Width, view.Height, 0f, 0f, 1f);
        _count = 0;
        _begun = true;
    }

    /// <summary>Adds a four-cornered piece: a to b along one edge, d to c along the other, each corner its own colour (premultiplied).</summary>
    public void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ca, Color cb, Color cc, Color cd)
    {
        if (!_begun) return;
        Room(6);
        Put(a, ca); Put(b, cb); Put(c, cc);
        Put(a, ca); Put(c, cc); Put(d, cd);
    }

    /// <summary>Adds a triangle, each corner its own colour (premultiplied).</summary>
    public void Triangle(Vector2 a, Vector2 b, Vector2 c, Color ca, Color cb, Color cc)
    {
        if (!_begun) return;
        Room(3);
        Put(a, ca); Put(b, cb); Put(c, cc);
    }

    /// <summary>Adds a straight band from one point to another, a width across, bright down its middle and fading to nothing at its edges.</summary>
    /// <param name="from">Where it starts, in world pixels.</param>
    /// <param name="to">Where it ends.</param>
    /// <param name="width">How wide, in world pixels.</param>
    /// <param name="start">Its colour at the start (premultiplied).</param>
    /// <param name="end">Its colour at the end.</param>
    public void Band(Vector2 from, Vector2 to, float width, Color start, Color end)
    {
        Vector2 along = to - from;
        if (along.LengthSquared() < 1e-6f) return;
        Vector2 across = Vector2.Normalize(new Vector2(-along.Y, along.X)) * (width / 2f);
        Quad(from + across, to + across, to, from, Color.Transparent, Color.Transparent, end, start);
        Quad(from, to, to - across, from - across, start, end, Color.Transparent, Color.Transparent);
    }

    /// <summary>Adds a disc as a fan of triangles: bright in the middle and fading to its rim, or a ring if it has a hole.</summary>
    /// <param name="centre">Its middle, in world pixels.</param>
    /// <param name="radii">How wide and deep it is (a ring on the floor is flattened).</param>
    /// <param name="inner">The colour at the middle, or at the hole's edge (premultiplied).</param>
    /// <param name="outer">The colour at the rim.</param>
    /// <param name="hole">How much of it is hole, 0 to 1.</param>
    /// <param name="segments">How many pieces round.</param>
    /// <param name="from">Where round it starts, in radians (for a part of one).</param>
    /// <param name="sweep">How far round it goes, in radians.</param>
    public void Disc(Vector2 centre, Vector2 radii, Color inner, Color outer, float hole = 0f, int segments = 24, float from = 0f, float sweep = MathHelper.TwoPi)
    {
        for (int i = 0; i < segments; i++)
        {
            float a0 = from + sweep * i / segments, a1 = from + sweep * (i + 1) / segments;
            var d0 = new Vector2(MathF.Cos(a0), MathF.Sin(a0));
            var d1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1));
            Vector2 o0 = centre + d0 * radii, o1 = centre + d1 * radii;
            if (hole <= 0f) Triangle(centre, o0, o1, inner, outer, outer);
            else Quad(centre + d0 * radii * hole, o0, o1, centre + d1 * radii * hole, inner, outer, outer, inner);
        }
    }

    /// <summary>Draws everything gathered since Begin, additively or as Begin said.</summary>
    public void End()
    {
        _begun = false;
        if (_count < 3) return;
        _device.BlendState = _blend;
        _device.DepthStencilState = DepthStencilState.None;
        _device.RasterizerState = RasterizerState.CullNone;
        foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _count / 3);
        }
        _count = 0;
    }

    /// <summary>Lets go of the effect.</summary>
    public void Dispose() => _effect.Dispose();

    private void Room(int more)
    {
        if (_count + more <= _vertices.Length) return;
        Array.Resize(ref _vertices, Math.Max(_vertices.Length * 2, _count + more));
    }

    private void Put(Vector2 p, Color c) => _vertices[_count++] = new VertexPositionColor(new Vector3(p, 0f), c);
}
