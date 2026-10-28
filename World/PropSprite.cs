using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// One prop on the map: its picture, where it stands, and the floor under it nobody can walk through.
/// </summary>
/// <remarks>The Black Box's LobbyProp, reading what it is from a PropDefinition instead of being told in code.</remarks>
public class PropSprite
{
    private Texture2D _texture;

    /// <summary>What kind of prop it is.</summary>
    public PropDefinition Definition { get; }

    /// <summary>The middle of its bottom edge, in world pixels.</summary>
    public Vector2 Foot { get; }

    /// <summary>The floor it takes up: none for things you can walk through, one box for most, and two for an arch, one each side of the way through.</summary>
    public IReadOnlyList<BoundingRectangle> Footprints { get; }

    /// <summary>How far it's been moved off its foot, in world pixels. Only the arrival moves anything: the ship, sailing in.</summary>
    public Vector2 Offset { get; set; }

    /// <summary>Whether it's not drawn: the gangplank, till the ship's tied up.</summary>
    public bool Hidden { get; set; }

    /// <summary>Which of its pictures to draw: 0 is shut (or up, or asleep), 1 is open.</summary>
    public int Frame { get; set; }

    /// <summary>How big one of its pictures is, in art pixels. Only known once it's loaded.</summary>
    public Point Size => new(_texture.Width / Definition.Frames, _texture.Height);

    /// <summary>Puts a prop where the map says.</summary>
    /// <param name="placed">The prop and its foot, from the map.</param>
    public PropSprite(MapProp placed)
    {
        Definition = placed.Prop;
        Foot = placed.Foot;

        Point size = Definition.Footprint;
        float left = Foot.X - size.X / 2f, top = Foot.Y - size.Y;
        var footprints = new List<BoundingRectangle>();

        if (size != Point.Zero && Definition.Gap == 0)
        {
            footprints.Add(new BoundingRectangle(left, top, size.X, size.Y));
        }
        else if (size != Point.Zero)
        {
            // An arch: a box each side of the way through.
            float side = (size.X - Definition.Gap) / 2f;
            footprints.Add(new BoundingRectangle(left, top, side, size.Y));
            footprints.Add(new BoundingRectangle(Foot.X + Definition.Gap / 2f, top, side, size.Y));
        }

        Footprints = footprints;
    }

    /// <summary>Loads the picture.</summary>
    /// <remarks>Throws if the picture doesn't split evenly into its frames. That means the generator and PropCatalog don't match.</remarks>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>(Definition.Texture);
        if (_texture.Width % Definition.Frames != 0)
            throw new InvalidOperationException($"{Definition.Texture}.png is {_texture.Width} wide, which doesn't split into {Definition.Frames} pictures. Run tools/generate_world.py, or tools/cut_supplied.py for Questeria's.");
    }

    /// <summary>Where it sorts on its map: behind everyone if it's on a wall, under everyone if it's lying on the ground, otherwise by how far down the map its foot is.</summary>
    /// <param name="mapHeight">How tall the map is, in world pixels.</param>
    public float Layer(float mapHeight) => Definition.Placement switch
    {
        PropPlacement.Wall => Layers.WorldWall,
        PropPlacement.Ground => Layers.GroundProp,
        _ => Layers.OnFloor(Foot.Y + Offset.Y - Definition.SortRise, mapHeight),
    };

    /// <summary>Draws the prop, standing on its foot.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="layerDepth">Where it sorts.</param>
    /// <param name="tint">What to multiply it by: white on the map itself, the paper colour on the map screen.</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth, Color tint)
    {
        if (Hidden) return;

        Point size = Size;
        var source = new Rectangle(Frame * size.X, 0, size.X, size.Y);
        var origin = new Vector2(size.X / 2, size.Y);
        Vector2 at = Foot + Offset;
        spriteBatch.Draw(_texture, new Vector2(MathF.Round(at.X), MathF.Round(at.Y)), source, tint,
            0f, origin, 1f, SpriteEffects.None, layerDepth);
    }
}
