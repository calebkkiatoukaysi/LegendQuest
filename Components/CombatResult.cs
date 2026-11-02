using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Items;

namespace LegendQuest.Components;

/// <summary>
/// The panel at the end of a fight: VICTORY with the experience, the gold and what the fallen
/// carried, or DEFEAT, and ENTER to go on.
/// </summary>
/// <remarks>
/// It comes up over the dimmed map once the last blow's been shown, and holds until it's been read
/// (a moment at least, so the click that won the fight doesn't also put it away). The loot is shown
/// the way the bag shows it, each thing in its well with its count, and it's already in the bag by
/// the time this is up.
/// </remarks>
public class CombatResult
{
    /// <summary>How wide the panel is, and how far in from its edges everything goes, in screen pixels.</summary>
    private const int Width = 660;
    private const int Inset = 28;

    /// <summary>How long it takes to come in, and how long before a press can put it away.</summary>
    private const float FadeInSeconds = 0.4f;
    private const float ReadySeconds = 0.7f;

    /// <summary>How big the heading is drawn, against the title font's own size.</summary>
    private const float HeadingScale = 0.62f;

    /// <summary>The room between the parts of the panel, and between two pieces of loot, in screen pixels.</summary>
    private const int PartGap = 16;
    private const int LootGap = 10;

    /// <summary>How many pieces of loot fit on a row.</summary>
    private const int LootPerRow = 7;

    /// <summary>How dark the map goes behind it.</summary>
    private const float Veil = 0.5f;

    private const string ContinueLine = "ENTER  ·  CONTINUE";

    private readonly ItemIcons _icons = new();
    private readonly List<(string Text, Color Colour)> _lines = new();
    private readonly List<(ItemDefinition Item, int Count)> _loot = new();

    private SpriteFont _title;
    private SpriteFont _font;
    private Texture2D _pixel;
    private string _heading;
    private Color _headingColour;
    private float _clock;

    /// <summary>Whether it's up.</summary>
    public bool Showing { get; private set; }

    /// <summary>Whether it's been up long enough to be put away.</summary>
    public bool Ready => Showing && _clock >= ReadySeconds;

    /// <summary>Loads the fonts and the item icons.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="pixel">One white pixel for the veil and the panel: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, Texture2D pixel)
    {
        _title = content.Load<SpriteFont>("spectral-title");
        _font = content.Load<SpriteFont>("spectral-ui");
        _icons.LoadContent(content);
        _pixel = pixel;
    }

    /// <summary>Puts it up.</summary>
    /// <param name="heading">VICTORY or DEFEAT.</param>
    /// <param name="colour">The heading's colour.</param>
    /// <param name="lines">The lines under it, each with its colour: the experience, the gold.</param>
    /// <param name="loot">The items that went in the bag, by id, one entry each.</param>
    public void Show(string heading, Color colour, IEnumerable<(string Text, Color Colour)> lines, IEnumerable<string> loot)
    {
        _heading = heading;
        _headingColour = colour;
        _lines.Clear();
        _lines.AddRange(lines);
        _loot.Clear();
        foreach (string id in loot)
        {
            int at = _loot.FindIndex(each => each.Item.Id == id);
            if (at >= 0) _loot[at] = (_loot[at].Item, _loot[at].Count + 1);
            else _loot.Add((ItemCatalog.Get(id), 1));
        }
        _clock = 0f;
        Showing = true;
    }

    /// <summary>Takes it down.</summary>
    public void Hide() => Showing = false;

    /// <summary>Runs its clock.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        if (Showing) _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>Draws the veil, the panel and the loot's wells and icons. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawArt(SpriteBatch spriteBatch, float alpha)
    {
        if (!Showing) return;
        float shown = Shown(alpha);

        spriteBatch.Draw(_pixel, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight), Color.Black * (Veil * shown));
        Rectangle panel = Panel();
        CombatStyle.DrawPanel(spriteBatch, _pixel, panel, shown);

        for (int i = 0; i < _loot.Count; i++)
        {
            Rectangle slot = LootSlot(i);
            _icons.DrawWell(spriteBatch, slot, WellLook.Plain, shown);
            _icons.DrawItem(spriteBatch, slot, _loot[i].Item, shown);
        }
    }

    /// <summary>Writes the heading, the lines, the loot's counts and ENTER to go on. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, float alpha)
    {
        if (!Showing) return;
        float shown = Shown(alpha);
        Rectangle panel = Panel();

        Vector2 size = _title.MeasureString(_heading) * HeadingScale;
        var at = new Vector2(MathF.Round(panel.Center.X - size.X / 2f), panel.Top + Inset);
        spriteBatch.DrawString(_title, _heading, at + Palette.ShadowOffset, Palette.Shadow * shown, 0f, Vector2.Zero, HeadingScale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_title, _heading, at, _headingColour * shown, 0f, Vector2.Zero, HeadingScale, SpriteEffects.None, 0f);

        float y = at.Y + size.Y + PartGap;
        foreach ((string text, Color colour) in _lines)
        {
            Text.Draw(spriteBatch, _font, text, panel.Center.X, y, colour * shown, Palette.Shadow * shown, centred: true);
            y += _font.LineSpacing;
        }

        for (int i = 0; i < _loot.Count; i++)
        {
            if (_loot[i].Count < 2) continue;
            Rectangle slot = LootSlot(i);
            Text.Draw(spriteBatch, _font, "x" + _loot[i].Count, slot.Right - 4, slot.Bottom - _font.LineSpacing, Palette.Gold * shown, Palette.Shadow * shown, rightAligned: true);
        }

        Color prompt = (Ready ? Palette.Torch : Palette.Note) * shown;
        Text.Draw(spriteBatch, _font, ContinueLine, panel.Center.X, panel.Bottom - Inset - _font.LineSpacing, prompt, Palette.Shadow * shown, centred: true);
    }

    /// <summary>How far in it is, 0 to 1.</summary>
    private float Shown(float alpha) => MathHelper.Clamp(_clock / FadeInSeconds, 0f, 1f) * alpha;

    /// <summary>How many rows the loot takes.</summary>
    private int LootRows => (_loot.Count + LootPerRow - 1) / LootPerRow;

    /// <summary>Where the panel is: in the middle of the screen, as tall as what's on it.</summary>
    private Rectangle Panel()
    {
        int heading = (int)(_title.LineSpacing * HeadingScale);
        int loot = LootRows == 0 ? 0 : LootRows * (ItemIcons.Size + LootGap) + PartGap;
        int height = Inset * 2 + heading + PartGap + _lines.Count * _font.LineSpacing + PartGap + loot + _font.LineSpacing;
        return new Rectangle((LegendQuestGame.ScreenWidth - Width) / 2, (LegendQuestGame.ScreenHeight - height) / 2, Width, height);
    }

    /// <summary>Where one piece of loot's well is: in rows under the lines, each row centred.</summary>
    private Rectangle LootSlot(int index)
    {
        Rectangle panel = Panel();
        int row = index / LootPerRow, column = index % LootPerRow;
        int inRow = Math.Min(LootPerRow, _loot.Count - row * LootPerRow);
        int width = inRow * ItemIcons.Size + (inRow - 1) * LootGap;
        int top = panel.Top + Inset + (int)(_title.LineSpacing * HeadingScale) + PartGap + _lines.Count * _font.LineSpacing + PartGap;
        return new Rectangle(panel.Center.X - width / 2 + column * (ItemIcons.Size + LootGap), top + row * (ItemIcons.Size + LootGap), ItemIcons.Size, ItemIcons.Size);
    }
}
