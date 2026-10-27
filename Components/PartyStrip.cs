using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;

namespace LegendQuest.Components;

/// <summary>
/// The party along the bottom left: a card for each of them with their face, their name and how
/// hurt they are. The one you're controlling is raised and edged in torchlight, and clicking a card
/// takes control of them.
/// </summary>
/// <remarks>
/// The face is cut off their own sheet (the head and shoulders of the frame where they stand facing
/// you) and blown up, so it always matches how they look walking about, recolouring and all, and the
/// paperdoll will be on it for free. Conditions go under the bar once there are any.
/// </remarks>
public class PartyStrip
{
    /// <summary>How far the strip is from the left and bottom of the screen, in screen pixels.</summary>
    private const int Margin = 24;

    /// <summary>One card's size, the space between cards, and how far up the one you're controlling sits.</summary>
    private const int CardWidth = 150;
    private const int CardHeight = 150;
    private const int CardGap = 10;
    private const int LeaderRaise = 8;

    /// <summary>The panel the cards are cut from, and how much it's blown up. A size down from the forms, so the corners don't crowd a card.</summary>
    private const int PanelFrame = 32;
    private const int PanelCorner = 12;
    private const float PanelScale = 2f;

    /// <summary>The part of a sheet that's the face: the head and shoulders of the standing frame facing down, in art pixels.</summary>
    private static readonly Rectangle Bust = new(4, 3, 24, 26);

    /// <summary>How much the face is blown up, and how far down the card it starts.</summary>
    private const float PortraitScale = 3f;
    private const int PortraitTop = 14;

    /// <summary>The hit points bar: how far in from the card's sides, how far up from its bottom, and how tall.</summary>
    private const int BarInset = 16;
    private const int BarBottom = 16;
    private const int BarHeight = 8;

    /// <summary>The edge round the card of whoever you're controlling, in screen pixels.</summary>
    private const int LeaderEdge = 2;

    private static readonly Color BarBack = new(24, 18, 18);
    private static readonly Color BarFill = new(168, 52, 40);

    private Texture2D _panel;
    private Texture2D _pixel;
    private SpriteFont _font;

    /// <summary>Loads the panel, the font, and the pixel the bars are drawn with.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="graphicsDevice">The device, for the pixel.</param>
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _panel = content.Load<Texture2D>("panel");
        _font = content.Load<SpriteFont>("spectral-detail");

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    /// <summary>Disposes the pixel, since the content manager didn't make it.</summary>
    public void Unload() => _pixel?.Dispose();

    /// <summary>Draws the cards, the faces and the bars. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, back to front.</param>
    /// <param name="party">Everyone in the party, in order.</param>
    /// <param name="sheets">Each one's recoloured sheet, in the same order.</param>
    /// <param name="leader">Which one you're controlling.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawCards(SpriteBatch spriteBatch, IReadOnlyList<PartyMember> party, IReadOnlyList<Texture2D> sheets, int leader, float alpha)
    {
        for (int i = 0; i < party.Count; i++)
        {
            Rectangle card = Card(i, leader);
            NineSlice.Draw(spriteBatch, _panel, card, Point.Zero, PanelFrame, PanelCorner, PanelScale, Color.White * alpha, Layers.PanelPlate);

            if (i == leader)
            {
                Rectangle edge = card;
                edge.Inflate(LeaderEdge, LeaderEdge);
                Fill(spriteBatch, new Rectangle(edge.Left, edge.Top, edge.Width, LeaderEdge), Palette.Torch * alpha, Layers.Selection);
                Fill(spriteBatch, new Rectangle(edge.Left, edge.Bottom - LeaderEdge, edge.Width, LeaderEdge), Palette.Torch * alpha, Layers.Selection);
                Fill(spriteBatch, new Rectangle(edge.Left, edge.Top, LeaderEdge, edge.Height), Palette.Torch * alpha, Layers.Selection);
                Fill(spriteBatch, new Rectangle(edge.Right - LeaderEdge, edge.Top, LeaderEdge, edge.Height), Palette.Torch * alpha, Layers.Selection);
            }

            var face = new Vector2(card.Center.X - Bust.Width * PortraitScale / 2f, card.Top + PortraitTop);
            spriteBatch.Draw(sheets[i], face, Bust, Color.White * alpha, 0f, Vector2.Zero, PortraitScale, SpriteEffects.None, Layers.Portrait);

            PartyMember member = party[i];
            var bar = new Rectangle(card.Left + BarInset, card.Bottom - BarBottom - BarHeight, card.Width - BarInset * 2, BarHeight);
            Fill(spriteBatch, bar, BarBack * alpha, Layers.BarBack);
            float health = member.MaxHp > 0 ? MathHelper.Clamp((float)member.Hp / member.MaxHp, 0f, 1f) : 0f;
            Fill(spriteBatch, new Rectangle(bar.Left, bar.Top, (int)MathF.Round(bar.Width * health), bar.Height), BarFill * alpha, Layers.BarFill);
        }
    }

    /// <summary>Writes everyone's name on their card. Goes in a linear sampled batch, after DrawCards.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, back to front.</param>
    /// <param name="party">Everyone in the party, in order.</param>
    /// <param name="leader">Which one you're controlling.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawNames(SpriteBatch spriteBatch, IReadOnlyList<PartyMember> party, int leader, float alpha)
    {
        for (int i = 0; i < party.Count; i++)
        {
            Rectangle card = Card(i, leader);
            float y = card.Top + PortraitTop + Bust.Height * PortraitScale + 2f;
            Color colour = i == leader ? Palette.Heading : Palette.Note;
            Text.Draw(spriteBatch, _font, party[i].Name.ToUpperInvariant(), card.Center.X, y, colour * alpha, Palette.Shadow * alpha, centred: true);
        }
    }

    /// <summary>Which card the mouse is over.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="count">How many people are in the party.</param>
    /// <param name="leader">Which one you're controlling, since their card sits higher.</param>
    /// <returns>Their place in the party, or null if it isn't over one.</returns>
    public int? CardAt(Point mouse, int count, int leader)
    {
        for (int i = 0; i < count; i++)
            if (Card(i, leader).Contains(mouse)) return i;
        return null;
    }

    /// <summary>For the proof run, so it knows where to put the pretend mouse: the middle of one card.</summary>
    /// <param name="index">Their place in the party.</param>
    /// <param name="leader">Which one you're controlling.</param>
    internal Point ProofCard(int index, int leader) => Card(index, leader).Center;

    /// <summary>Where one card is on screen.</summary>
    private static Rectangle Card(int i, int leader) => new(
        Margin + i * (CardWidth + CardGap),
        LegendQuestGame.ScreenHeight - Margin - CardHeight - (i == leader ? LeaderRaise : 0),
        CardWidth,
        CardHeight);

    /// <summary>Fills a rectangle with a flat colour.</summary>
    private void Fill(SpriteBatch spriteBatch, Rectangle area, Color color, float layer) =>
        spriteBatch.Draw(_pixel, area, null, color, 0f, Vector2.Zero, SpriteEffects.None, layer);
}
