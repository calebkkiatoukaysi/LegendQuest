using System;
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

    /// <summary>The floor it takes up, or null for things you can walk through or that are on a wall.</summary>
    public BoundingRectangle? Footprint { get; }

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
        if (size != Point.Zero)
            Footprint = new BoundingRectangle(Foot.X - size.X / 2f, Foot.Y - size.Y, size.X, size.Y);
    }

    /// <summary>Loads the picture.</summary>
    /// <remarks>Throws if the picture doesn't split evenly into its frames. That means the generator and PropCatalog don't match.</remarks>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>(Definition.Texture);
        if (_texture.Width % Definition.Frames != 0)
            throw new InvalidOperationException($"{Definition.Texture}.png is {_texture.Width} wide, which doesn't split into {Definition.Frames} pictures. Run tools/generate_world.py.");
    }

    /// <summary>Draws the prop, standing on its foot.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="layerDepth">Where it sorts.</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth)
    {
        Point size = Size;
        var source = new Rectangle(Frame * size.X, 0, size.X, size.Y);
        var origin = new Vector2(size.X / 2, size.Y);
        spriteBatch.Draw(_texture, new Vector2(MathF.Round(Foot.X), MathF.Round(Foot.Y)), source, Color.White,
            0f, origin, 1f, SpriteEffects.None, layerDepth);
    }
}
