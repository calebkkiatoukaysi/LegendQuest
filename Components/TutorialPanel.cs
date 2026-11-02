using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.StateManagement;
using LegendQuest.Tutorial;

namespace LegendQuest.Components;

/// <summary>
/// The card a tutorial tip comes up on: its title in gold, the idea under it, the keys as key caps,
/// and ENTER · GOT IT in the corner, where the tip says it goes.
/// </summary>
/// <remarks>
/// It shows whatever Tutorials says is up, and asks it for the next one when there's room, a moment
/// after the last has gone so two don't blur into one. It's cut from the panel the forms are, at the
/// party cards' size, so it belongs with them, and a gold edge round it flares as it comes in and
/// settles, so the eye goes to it once and then leaves it alone. It eases in and out, rising into
/// place (or dropping, at the top of the screen). It doesn't pause anything: walking carries on
/// under it, and it goes by itself once there's been time to read it twice, unless the tip says it
/// holds everything until it's read. Enter, X on a pad, or a click on it puts it away. While
/// anything's over the screen it's on (a conversation, the pause menu) it fades out and its clock
/// stops, and it comes back after.
/// </remarks>
public class TutorialPanel
{
    /// <summary>How wide the card is, and how far in from its edges the words go, in screen pixels.</summary>
    private const int CardWidth = 560;
    private const int Inset = 30;

    /// <summary>The room under the title, and over the keys, in screen pixels.</summary>
    private const float TitleGap = 4f;
    private const float KeysGap = 10f;

    /// <summary>How far from the screen's edges the card keeps, and how far under the top it goes: past what you're to do next, which ends about here. Screen pixels.</summary>
    private const int Margin = 24;
    private const int UnderObjective = 100;

    /// <summary>How much of the bottom of the screen the party's cards take, from the raised one's top: PartyStrip's margin, card and raise. Screen pixels.</summary>
    private const int StripRoom = 182;

    /// <summary>The panel the card's cut from, as PartyStrip cuts its cards: the frame, its corner, and a size down from the forms.</summary>
    private const int PanelFrame = 32;
    private const int PanelCorner = 12;
    private const float PanelScale = 2f;

    /// <summary>How long the card takes to come in and to go, how far it travels as it does, and the pause before the next one.</summary>
    private const float FadeInSeconds = 0.3f;
    private const float FadeOutSeconds = 0.35f;
    private const float Travel = 12f;
    private const float GapSeconds = 0.8f;

    /// <summary>How long a card stays up before it goes by itself: a few seconds, and more for every letter, so a slow reader gets it twice.</summary>
    private const float HoldSeconds = 6f;
    private const float SecondsPerLetter = 0.07f;

    /// <summary>The gold edge round the card: how thick, how bright it settles at, and how long it takes to settle from full.</summary>
    private const int EdgeThickness = 2;
    private const float EdgeRest = 0.35f;
    private const float EdgeSettleSeconds = 1.2f;

    /// <summary>How dark the screen goes behind a tip that holds everything.</summary>
    private const float BlockingVeil = 0.45f;

    /// <summary>What's written beside the key that puts it away.</summary>
    private const string GotIt = "GOT IT";

    /// <summary>The key that puts it away, on its cap.</summary>
    private static readonly IReadOnlyList<string> GotItKeys = new[] { "ENTER" };

    /// <summary>The room between the ENTER cap and GOT IT, in screen pixels.</summary>
    private const float GotItGap = 10f;

    private readonly InputAction _gotIt = new(new[] { Buttons.X }, new[] { Keys.Enter }, true);
    private readonly KeyCaps _keys = new();

    private Texture2D _panel;
    private Texture2D _pixel;
    private SpriteFont _font;

    /// <summary>The tip on the card, its title in capitals and its words broken into rows, and the card's size.</summary>
    private TutorialTip _tip;
    private string _title;
    private List<string> _rows = new();
    private Point _size;

    /// <summary>How far in the card is, 0 to 1, whether it's on its way out, how long it's been up, and how long till the next may come.</summary>
    private float _shown;
    private bool _leaving;
    private float _age;
    private float _gap;

    /// <summary>Whether the mouse is on the card.</summary>
    private bool _hovered;

    /// <summary>Loads the panel, the font and the key caps.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="pixel">One white pixel for the edge and the veil: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, Texture2D pixel)
    {
        _panel = content.Load<Texture2D>("panel");
        _font = content.Load<SpriteFont>("spectral-detail");
        _keys.LoadContent(content);
        _pixel = pixel;
    }

    /// <summary>Whether a card is up and in, so it can be put away.</summary>
    public bool Showing => _tip is not null && !_leaving && _shown > 0f;

    /// <summary>Whether a point is on the card that's up.</summary>
    /// <param name="point">Where, in screen pixels.</param>
    public bool Contains(Point point) => Showing && Card().Contains(point);

    /// <summary>Puts the card away on Enter, X or a click on it.</summary>
    /// <param name="input">The input this frame.</param>
    /// <param name="tutorials">The tips the card's showing.</param>
    /// <returns>True if it took the input: it was put away, or it's a tip that holds everything till it's read.</returns>
    public bool HandleInput(InputState input, Tutorials tutorials)
    {
        _hovered = Contains(input.MousePosition);
        if (!Showing) return false;

        if (_gotIt.Occurred(input) || (_hovered && input.IsNewLeftClick))
        {
            tutorials.Dismiss();
            return true;
        }
        return _tip.Blocking;
    }

    /// <summary>Brings the next tip in when there's room, eases the card in and out, and puts it away when its time's up.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="tutorials">The tips to show.</param>
    /// <param name="visible">Whether the screen it's on is the one being played: not under a conversation or a menu, and not mid-cutscene. While it isn't, the card fades out and waits.</param>
    public void Update(GameTime gameTime, Tutorials tutorials, bool visible)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (!tutorials.On) tutorials.Clear();
        if (_tip is not null && tutorials.Current != _tip) _leaving = true;

        if (_leaving || !visible)
        {
            _shown = MathF.Max(0f, _shown - seconds / FadeOutSeconds);
            if (!_leaving || _shown > 0f) return;

            _tip = null;
            _leaving = false;
            _gap = GapSeconds;
            return;
        }

        if (_tip is null)
        {
            _gap = MathF.Max(0f, _gap - seconds);
            if (_gap > 0f || tutorials.Next() is not TutorialTip next) return;
            Begin(next);
        }

        _shown = MathF.Min(1f, _shown + seconds / FadeInSeconds);
        _age += seconds;
        if (!_tip.Blocking && _age >= HoldSeconds + _tip.Body.Length * SecondsPerLetter) tutorials.Dismiss();
    }

    /// <summary>Draws the veil behind a tip that holds everything, the card, its gold edge and its key caps. Goes in a point sampled batch, before DrawWords.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, back to front.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawCard(SpriteBatch spriteBatch, float alpha)
    {
        if (_tip is null || _shown <= 0f) return;

        float shown = Eased() * alpha;
        Rectangle card = Card();

        if (_tip.Blocking)
        {
            spriteBatch.Draw(_pixel, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight), null,
                Palette.Veil * (BlockingVeil * shown), 0f, Vector2.Zero, SpriteEffects.None, Layers.Veil);
        }

        NineSlice.Draw(spriteBatch, _panel, card, Point.Zero, PanelFrame, PanelCorner, PanelScale, Color.White * shown, Layers.PanelPlate);
        DrawEdge(spriteBatch, card, Palette.Gold * (EdgeBrightness() * shown));

        _keys.DrawCaps(spriteBatch, _tip.Keys, KeysAt(card), shown);
        _keys.DrawCaps(spriteBatch, GotItKeys, GotItAt(card), shown);
    }

    /// <summary>Writes the title, the words, the letters on the keys and GOT IT. Goes in a linear sampled batch, after DrawCard.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, back to front.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawWords(SpriteBatch spriteBatch, float alpha)
    {
        if (_tip is null || _shown <= 0f) return;

        float shown = Eased() * alpha;
        Rectangle card = Card();
        float x = card.Left + Inset;
        float y = card.Top + Inset;

        Text.Draw(spriteBatch, _font, _title, x, y, Palette.Gold * shown, Palette.Shadow * shown);
        y += _font.LineSpacing + TitleGap;
        foreach (string row in _rows)
        {
            Text.Draw(spriteBatch, _font, row, x, y, Palette.Bone * shown, Palette.Shadow * shown);
            y += _font.LineSpacing;
        }

        _keys.DrawLetters(spriteBatch, _tip.Keys, KeysAt(card), shown);
        Vector2 gotIt = GotItAt(card);
        _keys.DrawLetters(spriteBatch, GotItKeys, gotIt, shown);
        Text.Draw(spriteBatch, _font, GotIt, gotIt.X + _keys.Measure(GotItKeys) + GotItGap, gotIt.Y + (KeyCaps.Height - _font.LineSpacing) / 2f,
            Palette.Note * shown, Palette.Shadow * shown);
    }

    /// <summary>Puts a tip on the card: its title in capitals, its words broken to fit, and the card sized round them.</summary>
    private void Begin(TutorialTip tip)
    {
        _tip = tip;
        _title = tip.Title.ToUpperInvariant();
        _rows = Text.Wrap(tip.Body, _font, CardWidth - Inset * 2);
        _shown = 0f;
        _age = 0f;

        float height = Inset + _font.LineSpacing + TitleGap + _rows.Count * _font.LineSpacing + KeysGap + KeyCaps.Height + Inset;
        _size = new Point(CardWidth, (int)MathF.Ceiling(height));
    }

    /// <summary>Where the card is on screen, as far along its way in or out as it's got.</summary>
    private Rectangle Card()
    {
        int right = LegendQuestGame.ScreenWidth - Margin - _size.X;
        Point at = _tip.Anchor switch
        {
            TipAnchor.TopLeft => new Point(Margin, UnderObjective),
            TipAnchor.TopRight => new Point(right, UnderObjective),
            TipAnchor.AboveStrip => new Point(Margin, LegendQuestGame.ScreenHeight - StripRoom - Margin / 2 - _size.Y),
            TipAnchor.Right => new Point(right, (LegendQuestGame.ScreenHeight - _size.Y) / 2),
            TipAnchor.Centre => new Point((LegendQuestGame.ScreenWidth - _size.X) / 2, (LegendQuestGame.ScreenHeight - _size.Y) / 2),
            _ => new Point(right, LegendQuestGame.ScreenHeight - Margin - _size.Y),
        };

        // Into place from below, or from above for the cards along the top.
        bool fromAbove = _tip.Anchor is TipAnchor.TopLeft or TipAnchor.TopRight;
        float away = (1f - Eased()) * Travel * (fromAbove ? -1f : 1f);
        return new Rectangle(at.X, at.Y + (int)MathF.Round(away), _size.X, _size.Y);
    }

    /// <summary>Where the row of the tip's own keys starts: along the bottom of the card, on the left.</summary>
    private static Vector2 KeysAt(Rectangle card) => new(card.Left + Inset, card.Bottom - Inset - KeyCaps.Height);

    /// <summary>Where ENTER · GOT IT starts, so it ends at the card's right edge on the same row as the keys.</summary>
    private Vector2 GotItAt(Rectangle card)
    {
        float width = _keys.Measure(GotItKeys) + GotItGap + _font.MeasureString(GotIt).X;
        return new Vector2(card.Right - Inset - width, card.Bottom - Inset - KeyCaps.Height);
    }

    /// <summary>How far in the card is, eased so it slows into place instead of stopping dead.</summary>
    private float Eased() => _shown * _shown * (3f - 2f * _shown);

    /// <summary>How bright the gold edge is: full as the card comes in, settling over a second or so, and full again while the mouse is on it.</summary>
    private float EdgeBrightness()
    {
        if (_hovered) return 1f;
        float settled = MathHelper.Clamp(_age / EdgeSettleSeconds, 0f, 1f);
        return MathHelper.Lerp(1f, EdgeRest, settled);
    }

    /// <summary>A thin line round the card.</summary>
    private void DrawEdge(SpriteBatch spriteBatch, Rectangle card, Color colour)
    {
        Rectangle edge = card;
        edge.Inflate(EdgeThickness, EdgeThickness);
        Fill(spriteBatch, new Rectangle(edge.Left, edge.Top, edge.Width, EdgeThickness), colour);
        Fill(spriteBatch, new Rectangle(edge.Left, edge.Bottom - EdgeThickness, edge.Width, EdgeThickness), colour);
        Fill(spriteBatch, new Rectangle(edge.Left, edge.Top, EdgeThickness, edge.Height), colour);
        Fill(spriteBatch, new Rectangle(edge.Right - EdgeThickness, edge.Top, EdgeThickness, edge.Height), colour);
    }

    /// <summary>Fills a rectangle with a flat colour, at the edge's layer.</summary>
    private void Fill(SpriteBatch spriteBatch, Rectangle area, Color colour) =>
        spriteBatch.Draw(_pixel, area, null, colour, 0f, Vector2.Zero, SpriteEffects.None, Layers.Selection);
}
