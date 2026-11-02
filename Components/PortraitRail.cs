using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;

namespace LegendQuest.Components;

/// <summary>
/// The party's faces down the left of the inventory, one card each, then whoever's waiting at camp
/// under a line saying so. The one being looked at is edged in gold; clicking a card picks them.
/// </summary>
/// <remarks>
/// The same cards as the party's along the bottom of the map (PartyStrip), stood on end and without
/// the names, which are on the panel beside them: the face cut off their own baked sheet, and how
/// hurt they are along the bottom. Whoever's at camp is drawn a little faded, since they're not
/// here. The cards go in a point sampled batch and the camp's line in a linear one after.
/// </remarks>
public class PortraitRail
{
    /// <summary>Where the first card is, in screen pixels.</summary>
    private const int Left = 28;
    private const int Top = 30;

    /// <summary>One card's size, and the room between one and the next.</summary>
    private const int CardWidth = 96;
    private const int CardHeight = 100;
    private const int CardGap = 8;

    /// <summary>The room over the camp's cards for the line that says they're at camp.</summary>
    private const int CampRoom = 40;

    /// <summary>The panel the cards are cut from, as PartyStrip cuts its cards.</summary>
    private const int PanelFrame = 32;
    private const int PanelCorner = 12;
    private const float PanelScale = 2f;

    /// <summary>The face on a sheet, the same as PartyStrip's: the head and shoulders of the standing frame facing down, in art pixels.</summary>
    private static readonly Rectangle Bust = new(4, 3, 24, 26);

    /// <summary>How much the face is blown up, and how far down the card it starts.</summary>
    private const float PortraitScale = 3f;
    private const int PortraitTop = 6;

    /// <summary>The hit points bar: how far in from the card's sides, how far up from its bottom, and how tall.</summary>
    private const int BarInset = 12;
    private const int BarBottom = 8;
    private const int BarHeight = 6;

    /// <summary>The edge round the one being looked at, and the fainter one round the one under the mouse.</summary>
    private const int Edge = 2;
    private const float HoverEdge = 0.5f;

    /// <summary>How faded the faces of whoever's at camp are.</summary>
    private const float CampAlpha = 0.65f;

    /// <summary>What the line over the camp's cards says.</summary>
    private const string CampLine = "AT CAMP";

    private static readonly Color BarBack = new(24, 18, 18);
    private static readonly Color BarFill = new(168, 52, 40);

    private Texture2D _panel;
    private Texture2D _pixel;
    private SpriteFont _font;

    /// <summary>Loads the panel and the font.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="pixel">One white pixel for the bars and the edges: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, Texture2D pixel)
    {
        _panel = content.Load<Texture2D>("panel");
        _font = content.Load<SpriteFont>("spectral-detail");
        _pixel = pixel;
    }

    /// <summary>Where one card is on screen.</summary>
    /// <param name="index">Their place: the party first, then the camp.</param>
    /// <param name="partyCount">How many of them are in the party, so the camp's go under its line.</param>
    public static Rectangle Card(int index, int partyCount) => new(
        Left,
        Top + index * (CardHeight + CardGap) + (index >= partyCount ? CampRoom : 0),
        CardWidth,
        CardHeight);

    /// <summary>Which card a point is on.</summary>
    /// <param name="point">Where, in screen pixels: the mouse.</param>
    /// <param name="count">How many cards there are.</param>
    /// <param name="partyCount">How many of them are the party's.</param>
    /// <returns>Their place, or -1 for none.</returns>
    public static int CardAt(Point point, int count, int partyCount)
    {
        for (int i = 0; i < count; i++)
            if (Card(i, partyCount).Contains(point)) return i;
        return -1;
    }

    /// <summary>Draws the cards, the faces, the bars and the edges. Goes in a point sampled, deferred batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="members">The party, then whoever's at camp.</param>
    /// <param name="sheets">Each one's baked sheet, in the same order.</param>
    /// <param name="partyCount">How many of them are the party's.</param>
    /// <param name="selected">The one being looked at.</param>
    /// <param name="hovered">The one under the mouse, or -1.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawCards(SpriteBatch spriteBatch, IReadOnlyList<PartyMember> members, IReadOnlyList<Texture2D> sheets, int partyCount,
        int selected, int hovered, float alpha)
    {
        for (int i = 0; i < members.Count; i++)
        {
            Rectangle card = Card(i, partyCount);
            float faded = alpha * (i >= partyCount ? CampAlpha : 1f);
            NineSlice.Draw(spriteBatch, _panel, card, Point.Zero, PanelFrame, PanelCorner, PanelScale, Color.White * alpha, Layers.PanelPlate);

            var face = new Vector2(card.Center.X - Bust.Width * PortraitScale / 2f, card.Top + PortraitTop);
            spriteBatch.Draw(sheets[i], face, Bust, Color.White * faded, 0f, Vector2.Zero, PortraitScale, SpriteEffects.None, Layers.Portrait);

            PartyMember member = members[i];
            var bar = new Rectangle(card.Left + BarInset, card.Bottom - BarBottom - BarHeight, card.Width - BarInset * 2, BarHeight);
            float health = member.MaxHp > 0 ? MathHelper.Clamp((float)member.Hp / member.MaxHp, 0f, 1f) : 0f;
            spriteBatch.Draw(_pixel, bar, BarBack * alpha);
            spriteBatch.Draw(_pixel, new Rectangle(bar.Left, bar.Top, (int)MathF.Round(bar.Width * health), bar.Height), BarFill * faded);

            if (i == selected) DrawEdge(spriteBatch, card, Palette.Gold * alpha);
            else if (i == hovered) DrawEdge(spriteBatch, card, Palette.Bone * (HoverEdge * alpha));
        }
    }

    /// <summary>Writes the line over the camp's cards, if anyone's at camp. Goes in a linear sampled batch, after DrawCards.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="count">How many cards there are.</param>
    /// <param name="partyCount">How many of them are the party's.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawWords(SpriteBatch spriteBatch, int count, int partyCount, float alpha)
    {
        if (count <= partyCount) return;

        Rectangle first = Card(partyCount, partyCount);
        Text.Draw(spriteBatch, _font, CampLine, first.Center.X, first.Top - CampRoom + (CampRoom - _font.LineSpacing) / 2f + CardGap / 2f,
            Palette.Note * alpha, Palette.Shadow * alpha, centred: true);
    }

    /// <summary>A thin line round a card.</summary>
    private void DrawEdge(SpriteBatch spriteBatch, Rectangle card, Color colour)
    {
        Rectangle edge = card;
        edge.Inflate(Edge, Edge);
        spriteBatch.Draw(_pixel, new Rectangle(edge.Left, edge.Top, edge.Width, Edge), colour);
        spriteBatch.Draw(_pixel, new Rectangle(edge.Left, edge.Bottom - Edge, edge.Width, Edge), colour);
        spriteBatch.Draw(_pixel, new Rectangle(edge.Left, edge.Top, Edge, edge.Height), colour);
        spriteBatch.Draw(_pixel, new Rectangle(edge.Right - Edge, edge.Top, Edge, edge.Height), colour);
    }
}
