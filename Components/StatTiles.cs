using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// A small tile with one number on it: its name over it, the number big, and a line under it if
/// there's more to say, like a score's modifier. The inventory's HP, AC and speed are these, and so
/// are the character sheet's six scores.
/// </summary>
/// <remarks>
/// Cut from the forms' panel at the party cards' size, like the tutorial's card, so it sits on the
/// big panel as a plate of its own. The tile goes in a point sampled batch and its words in a linear
/// one after.
/// </remarks>
public class StatTiles
{
    /// <summary>The panel a tile's cut from, as PartyStrip cuts its cards: the frame, its corner, and a size down from the forms.</summary>
    private const int PanelFrame = 32;
    private const int PanelCorner = 12;
    private const float PanelScale = 2f;

    /// <summary>How far down the tile the name starts, and how far the number and the line under it sit up into the row above, in screen pixels: the fonts' line spacing has room under the letters that a tile doesn't want.</summary>
    private const float NameTop = 8f;
    private const float NumberTuck = 6f;
    private const float UnderTuck = 10f;

    private Texture2D _panel;
    private SpriteFont _font;
    private SpriteFont _detailFont;

    /// <summary>How tall a tile with a name and a number is.</summary>
    public int Height => (int)(NameTop * 2f + _detailFont.LineSpacing + _font.LineSpacing - NumberTuck);

    /// <summary>How tall a tile with a line under its number as well is.</summary>
    public int TallHeight => Height + (int)(_detailFont.LineSpacing - UnderTuck);

    /// <summary>Loads the panel and both fonts.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _panel = content.Load<Texture2D>("panel");
        _font = content.Load<SpriteFont>("spectral-ui");
        _detailFont = content.Load<SpriteFont>("spectral-detail");
    }

    /// <summary>Draws a tile. Goes in a point sampled batch, before DrawWords.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="tile">Where, in screen pixels.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawTile(SpriteBatch spriteBatch, Rectangle tile, float alpha) =>
        NineSlice.Draw(spriteBatch, _panel, tile, Point.Zero, PanelFrame, PanelCorner, PanelScale, Color.White * alpha, Layers.PanelPlate);

    /// <summary>Writes a tile's name, its number, and the line under that, centred on it. Goes in a linear sampled batch, after DrawTile.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="tile">Where, the same as DrawTile had.</param>
    /// <param name="name">What it is, small, like "AC".</param>
    /// <param name="number">The number, big.</param>
    /// <param name="under">A line under the number, or null.</param>
    /// <param name="underColour">The line's colour.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawWords(SpriteBatch spriteBatch, Rectangle tile, string name, string number, string under, Color underColour, float alpha)
    {
        float y = tile.Top + NameTop;
        Text.Draw(spriteBatch, _detailFont, name, tile.Center.X, y, Palette.Note * alpha, Palette.Shadow * alpha, centred: true);

        y += _detailFont.LineSpacing - NumberTuck;
        Text.Draw(spriteBatch, _font, number, tile.Center.X, y, Palette.Heading * alpha, Palette.Shadow * alpha, centred: true);

        if (under is null) return;
        y += _font.LineSpacing - UnderTuck;
        Text.Draw(spriteBatch, _detailFont, under, tile.Center.X, y, underColour * alpha, Palette.Shadow * alpha, centred: true);
    }
}
