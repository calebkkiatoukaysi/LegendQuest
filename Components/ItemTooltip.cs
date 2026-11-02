using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Items;

namespace LegendQuest.Components;

/// <summary>
/// The card that comes up beside an item: its name in its rarity's colour, what it is, its numbers,
/// what it says about itself, what putting it on would change (better in green, worse in red), what
/// whoever'd wear it ought to know, what it's worth, and what a click does.
/// </summary>
/// <remarks>
/// The words come from ItemReadout and GearComparison, so every screen says the same about an item.
/// Its rows are worked out once by ShowItem or ShowText and drawn until something else is shown, so a
/// screen only calls one of those when what's under the mouse or the keys changes. It sits beside the
/// slot it's about, on the right if there's room and on the left if not, so it never covers the slot
/// itself, and it's cut from the forms' panel at the party cards' size with a thin edge in the
/// item's rarity colour. The card goes in a point sampled batch and the words in a linear one after.
/// </remarks>
public class ItemTooltip
{
    /// <summary>How wide the card is, and how far in from its edges the words go, in screen pixels.</summary>
    private const int Width = 580;
    private const int Inset = 22;

    /// <summary>How tall the space between two parts of the card is, as a share of a line.</summary>
    private const float GapShare = 0.45f;

    /// <summary>How far from the slot the card sits, and how far it keeps from the screen's edges, in screen pixels.</summary>
    private const int Beside = 12;
    private const int Margin = 16;

    /// <summary>The panel the card's cut from, as PartyStrip cuts its cards: the frame, its corner, and a size down from the forms.</summary>
    private const int PanelFrame = 32;
    private const int PanelCorner = 12;
    private const float PanelScale = 2f;

    /// <summary>The edge round the card: how thick, and how bright its rarity colour is.</summary>
    private const int EdgeThickness = 2;
    private const float EdgeBrightness = 0.7f;

    /// <summary>An uncommon thing's colour and a rare one's: green and blue, like every RPG's, and like their rims in slots.png.</summary>
    private static readonly Color UncommonColour = new(142, 196, 92);
    private static readonly Color RareColour = new(110, 168, 240);

    /// <summary>A change for the better and one for the worse.</summary>
    private static readonly Color BetterColour = new(142, 196, 92);
    private static readonly Color WorseColour = new(240, 114, 108);

    /// <summary>The card's rows, top to bottom: some words and their colour, or null words for a gap.</summary>
    private readonly List<(string Text, Color Colour)> _rows = new();

    private Texture2D _panel;
    private Texture2D _pixel;
    private SpriteFont _font;
    private Color _edge;
    private int _height;

    /// <summary>Whether there's anything on the card to show.</summary>
    public bool Showing => _rows.Count > 0;

    /// <summary>The colour an item's name is in, for its rarity.</summary>
    /// <param name="rarity">The rarity.</param>
    public static Color RarityColour(Rarity rarity) => rarity switch
    {
        Rarity.Uncommon => UncommonColour,
        Rarity.Rare => RareColour,
        _ => Palette.Bone,
    };

    /// <summary>Loads the panel and the font.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="pixel">One white pixel for the edge: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, Texture2D pixel)
    {
        _panel = content.Load<Texture2D>("panel");
        _font = content.Load<SpriteFont>("spectral-detail");
        _pixel = pixel;
    }

    /// <summary>Takes everything off the card, so it doesn't show.</summary>
    public void Clear()
    {
        _rows.Clear();
        _height = 0;
    }

    /// <summary>Puts an item on the card.</summary>
    /// <param name="item">The item.</param>
    /// <param name="wearer">Who it's about, for what they ought to know before they wear it, or null for nobody.</param>
    /// <param name="compareIn">The slot it'd go in on them, to compare against what's there; null to leave the comparison off (it's already on, or it isn't worn).</param>
    /// <param name="price">What it's worth or costs, like "WORTH 15 GOLD", or null.</param>
    /// <param name="action">What a click does, like "CLICK  ·  PUT IT ON", or null.</param>
    public void ShowItem(ItemDefinition item, PartyMember wearer, EquipmentSlot? compareIn, string price, string action)
    {
        Clear();
        _edge = RarityColour(item.Rarity);

        Add(item.Name.ToUpperInvariant(), _edge);
        Add(ItemReadout.Kind(item), Palette.Note);

        IReadOnlyList<string> stats = ItemReadout.Stats(item);
        if (stats.Count > 0) AddGap();
        foreach (string line in stats) Add(line, Palette.Bone);

        AddGap();
        Add(item.Description, Palette.Narration);

        if (wearer is not null && compareIn is EquipmentSlot slot) AddComparison(item, wearer, slot);
        if (wearer is not null)
            foreach (string warning in GearComparison.Warnings(wearer, item)) Add(warning, Palette.Ember);

        if (price is not null || action is not null) AddGap();
        if (price is not null) Add(price, Palette.Gold);
        if (action is not null) Add(action, Palette.Torch);
    }

    /// <summary>Puts a few words on the card, for something that isn't an item: an empty slot, a key item.</summary>
    /// <param name="title">What it is, in capitals.</param>
    /// <param name="line">A line about it, or null.</param>
    /// <param name="action">What a click does, or null.</param>
    public void ShowText(string title, string line, string action)
    {
        Clear();
        _edge = Palette.Note;

        Add(title, Palette.Bone);
        if (line is not null) Add(line, Palette.Narration);
        if (action is not null) Add(action, Palette.Torch);
    }

    /// <summary>Draws the card and its edge beside a slot. Goes in a point sampled batch, before DrawWords.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled.</param>
    /// <param name="slot">The slot it's about, in screen pixels.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawCard(SpriteBatch spriteBatch, Rectangle slot, float alpha)
    {
        if (!Showing) return;

        Rectangle card = Place(slot);
        NineSlice.Draw(spriteBatch, _panel, card, Point.Zero, PanelFrame, PanelCorner, PanelScale, Color.White * alpha, Layers.PanelPlate);

        Color edge = _edge * (EdgeBrightness * alpha);
        Rectangle line = card;
        line.Inflate(EdgeThickness, EdgeThickness);
        spriteBatch.Draw(_pixel, new Rectangle(line.Left, line.Top, line.Width, EdgeThickness), edge);
        spriteBatch.Draw(_pixel, new Rectangle(line.Left, line.Bottom - EdgeThickness, line.Width, EdgeThickness), edge);
        spriteBatch.Draw(_pixel, new Rectangle(line.Left, line.Top, EdgeThickness, line.Height), edge);
        spriteBatch.Draw(_pixel, new Rectangle(line.Right - EdgeThickness, line.Top, EdgeThickness, line.Height), edge);
    }

    /// <summary>Writes the card's rows. Goes in a linear sampled batch, after DrawCard.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled.</param>
    /// <param name="slot">The slot it's about, the same as DrawCard had.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawWords(SpriteBatch spriteBatch, Rectangle slot, float alpha)
    {
        if (!Showing) return;

        Rectangle card = Place(slot);
        float y = card.Top + Inset;
        foreach ((string text, Color colour) in _rows)
        {
            if (text is null)
            {
                y += _font.LineSpacing * GapShare;
                continue;
            }

            Text.Draw(spriteBatch, _font, text, card.Left + Inset, y, colour * alpha, Palette.Shadow * alpha);
            y += _font.LineSpacing;
        }
    }

    /// <summary>What putting it on would change against what's in that slot now, each change green or red.</summary>
    private void AddComparison(ItemDefinition item, PartyMember wearer, EquipmentSlot slot)
    {
        ItemDefinition worn = wearer.WornIn(slot);
        AddGap();
        Add(worn is null
            ? $"On {wearer.Name}, where nothing's worn"
            : $"Against {wearer.Name}'s {worn.Name}", Palette.Note);

        IReadOnlyList<GearChange> changes = GearComparison.Compare(wearer, item, slot);
        if (changes.Count == 0) Add("No change", Palette.Note);
        foreach (GearChange change in changes) Add(change.ToString(), change.Better ? BetterColour : WorseColour);
    }

    /// <summary>Adds words to the card, broken into rows that fit, and makes the card tall enough for them.</summary>
    private void Add(string text, Color colour)
    {
        foreach (string row in Text.Wrap(text, _font, Width - Inset * 2))
        {
            _rows.Add((row, colour));
            _height += _font.LineSpacing;
        }
    }

    /// <summary>Adds a gap between two parts of the card, unless it's at the top.</summary>
    private void AddGap()
    {
        if (_rows.Count == 0) return;
        _rows.Add((null, Color.Transparent));
        _height += (int)MathF.Ceiling(_font.LineSpacing * GapShare);
    }

    /// <summary>Where the card goes for a slot: beside it on the right if there's room and on the left if not, level with its top, and kept on the screen.</summary>
    private Rectangle Place(Rectangle slot)
    {
        int height = _height + Inset * 2;
        int x = slot.Right + Beside;
        if (x + Width > LegendQuestGame.ScreenWidth - Margin) x = slot.Left - Beside - Width;
        int y = Math.Clamp(slot.Top, Margin, Math.Max(Margin, LegendQuestGame.ScreenHeight - Margin - height));
        return new Rectangle(x, y, Width, height);
    }
}
