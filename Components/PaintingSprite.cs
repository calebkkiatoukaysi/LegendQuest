using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Story;

namespace LegendQuest.Components;

/// <summary>
/// One of the story's paintings, filling the screen, its layers swaying against each other.
/// </summary>
/// <remarks>
/// The title background's sway, for any painting: the whole view drifts from side to side on a slow
/// sine and each layer moves by its depth's share of that, like the parallax tutorial's layers
/// except nothing scrolls. A layer is a little wider than the screen so the sway never shows an edge.
/// </remarks>
public class PaintingSprite
{
    /// <summary>How much bigger an art pixel is on screen. The same as the title's.</summary>
    private const float Scale = 4f;

    /// <summary>The canvas every layer is drawn on, in art pixels. The same as the title's.</summary>
    private const int CanvasWidth = 416;
    private const int CanvasHeight = 277;

    /// <summary>How far the nearest layer sways either way, in art pixels, and how long a sway takes there and back.</summary>
    private const float SwayReach = 6f;
    private const float SwaySeconds = 22f;

    private readonly Dictionary<string, List<(Texture2D Texture, float Depth)>> _loaded = new();
    private float _clock;

    /// <summary>Loads every painting's layers.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        foreach (string id in Paintings.All)
        {
            var layers = new List<(Texture2D, float)>();
            foreach ((string layer, float depth) in Paintings.Layers(id))
                layers.Add((content.Load<Texture2D>("Paintings/" + id + "-" + layer), depth));
            _loaded[id] = layers;
        }
    }

    /// <summary>Runs the sway.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime) => _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;

    /// <summary>Draws a painting over the whole screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="id">Which painting.</param>
    /// <param name="alpha">How far it's faded in.</param>
    public void Draw(SpriteBatch spriteBatch, string id, float alpha)
    {
        if (!_loaded.TryGetValue(id, out var layers)) return;

        float sway = MathF.Sin(_clock / SwaySeconds * MathHelper.TwoPi) * SwayReach;
        var origin = new Vector2(CanvasWidth / 2f, CanvasHeight / 2f);
        var middle = new Vector2(LegendQuestGame.ScreenWidth / 2f, LegendQuestGame.ScreenHeight / 2f);
        foreach ((Texture2D texture, float depth) in layers)
        {
            // Whole art pixels, so the layers don't shimmer as they move.
            float x = MathF.Round(sway * depth) * Scale;
            spriteBatch.Draw(texture, middle + new Vector2(x, 0f), null, Color.White * alpha, 0f, origin, Scale, SpriteEffects.None, 0f);
        }
    }
}
