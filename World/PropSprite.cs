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
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content) => _texture = content.Load<Texture2D>(Definition.Texture);

    /// <summary>Draws the prop, standing on its foot.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space. Point-sampled.</param>
    /// <param name="layerDepth">Where it sorts.</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth)
    {
        var origin = new Vector2(_texture.Width / 2, _texture.Height);
        spriteBatch.Draw(_texture, new Vector2(MathF.Round(Foot.X), MathF.Round(Foot.Y)), null, Color.White,
            0f, origin, 1f, SpriteEffects.None, layerDepth);
    }
}
