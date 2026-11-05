using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Components;
using LegendQuest.StateManagement;
using LegendQuest.Tutorial;

namespace LegendQuest.Screens;

/// <summary>
/// HOW TO PLAY, from the pause menu: every control on the keyboard, the mouse and a pad, then every
/// tutorial tip, a page or two to a topic, with their keys as key caps.
/// </summary>
/// <remarks>
/// The tips are the manual. Every one in TutorialCatalog is here under its topic, written once, so
/// a tip you put away too fast, or one you never saw because tips were off, can always be found
/// again, and the two can't say different things. The pages are laid out when the screen opens:
/// each topic's tips flow down the left column and then the right, onto another page if they need
/// one. The tabs along the top jump to a topic's first page; left and right (or Q and E, or the
/// shoulders) turn a page at a time, round from the last to the first. It's a popup over the pause
/// menu, like the options.
/// </remarks>
public class HelpScreen : MenuScreen
{
    private const string Heading = "HOW TO PLAY";
    private const string Note = "LEFT AND RIGHT TURN THE PAGE";
    private const string BackLabel = "BACK";

    /// <summary>Which page this is, of how many, in the bottom right corner.</summary>
    private const string PageCounter = "{0} / {1}";

    /// <summary>The first tab, the controls. The tips' topics have a tab each after it.</summary>
    private const string ControlsTab = "KEYS";

    /// <summary>The headings over the controls' second and third columns.</summary>
    private const string KeyboardHeading = "KEYBOARD AND MOUSE";
    private const string PadHeading = "GAMEPAD";

    /// <summary>The panel, nearly the whole screen: how far in from the sides and the top, in screen pixels.</summary>
    private static readonly Point PanelInset = new(48, 16);

    /// <summary>The tabs: their size, the room between them, and the room under the heading over them.</summary>
    private static readonly Point TabSize = new(230, 54);
    private const float TabGap = 10f;
    private const float HeadingGap = 10f;

    /// <summary>The room between the tabs and the page, and between the page and BACK.</summary>
    private const float PageGap = 20f;

    /// <summary>The two columns of tips: the room between them, and between one tip and the next.</summary>
    private const float ColumnGap = 48f;
    private const float TipGap = 12f;

    /// <summary>The room between a tip's title and its keys when they share a row.</summary>
    private const float KeysBeside = 20f;

    /// <summary>Where the controls' three columns start, from the page's left edge, in screen pixels.</summary>
    private const float KeysColumn = 440f;
    private const float PadColumn = 880f;

    /// <summary>The least and most room between two rows of controls, in screen pixels; it's whatever shares out what the page has.</summary>
    private const float LeastControlGap = 2f;
    private const float MostControlGap = 12f;

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.2);

    /// <summary>Each topic's tab, in TipTopic's order.</summary>
    private static readonly Dictionary<TipTopic, string> TopicTabs = new()
    {
        [TipTopic.Exploring] = "WORLD",
        [TipTopic.Story] = "STORY",
        [TipTopic.Party] = "PARTY",
        [TipTopic.Combat] = "FIGHTS",
        [TipTopic.Gear] = "GEAR",
    };

    /// <summary>Every control: what it does, its keys and buttons as caps, and the pad's way of doing it (empty when a pad has none).</summary>
    /// <remarks>The fighting and the bag have their keys on their own tips, on their own pages.</remarks>
    private static readonly (string What, string[] Keys, string Pad)[] Controls =
    {
        ("Walk", new[] { "W", "A", "S", "D", "ARROWS", KeyCaps.Click }, "Left stick or d-pad"),
        ("Talk, or use something", new[] { "E", "SPACE", "ENTER", KeyCaps.Click }, "A"),
        ("Change who you control", new[] { "TAB", "F1-F4", KeyCaps.Click }, "Right shoulder"),
        ("The map, and the world", new[] { "M", "TAB" }, "Y, then the shoulders"),
        ("Every badge, name and way out", new[] { "ALT" }, string.Empty),
        ("Put a tip away", new[] { "ENTER", KeyCaps.Click }, "X"),
        ("Go on, or roll the dice", new[] { "ENTER", "SPACE", KeyCaps.Click }, "A"),
        ("Answer a question", new[] { "1-4", KeyCaps.Click }, "D-pad, then A"),
        ("Pause, or go back", new[] { "ESC" }, "Start, or B"),
        ("Full screen", new[] { "F11" }, string.Empty),
    };

    private readonly InputAction _previousPage = new(
        new[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft, Buttons.LeftShoulder },
        new[] { Keys.Left, Keys.A, Keys.Q }, true);
    private readonly InputAction _nextPage = new(
        new[] { Buttons.DPadRight, Buttons.LeftThumbstickRight, Buttons.RightShoulder },
        new[] { Keys.Right, Keys.D, Keys.E }, true);

    private readonly FormPanel _panel = new();
    private readonly KeyCaps _keys = new();
    private readonly List<ButtonSprite> _tabs = new();

    /// <summary>Each page, laid out once: the tab it's under, the words on it (where each goes and its colour), and its rows of key caps.</summary>
    private readonly List<int> _pageTabs = new();
    private readonly List<List<(string Text, Vector2 At, Color Colour)>> _pageWords = new();
    private readonly List<List<(IReadOnlyList<string> Keys, Vector2 At)>> _pageCaps = new();

    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Rectangle _panelBounds;
    private Rectangle _page;
    private float _footY;
    private int _shown;

    /// <summary>Checks every topic has a tab.</summary>
    /// <remarks>Throws on purpose, so a topic added to TipTopic and not here shows up when the game starts instead of its tips going missing.</remarks>
    static HelpScreen()
    {
        foreach (TipTopic topic in Enum.GetValues<TipTopic>())
            if (!TopicTabs.ContainsKey(topic)) throw new InvalidOperationException($"Tip topic {topic} has no tab on HelpScreen.");
    }

    /// <summary>Builds BACK and the tabs.</summary>
    public HelpScreen()
    {
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        var back = new MenuEntry(BackLabel, Palette.Bone);
        back.Selected += ExitScreen;
        MenuEntries.Add(back);

        AddTab(ControlsTab);
        foreach (TipTopic topic in Enum.GetValues<TipTopic>()) AddTab(TopicTabs[topic]);
    }

    /// <summary>Loads the form, the fonts, the key caps and the tabs, and lays out every page.</summary>
    public override void Activate()
    {
        base.Activate();

        _panel.LoadContent(Content, ScreenManager.GraphicsDevice);
        _keys.LoadContent(Content);
        _font = Content.Load<SpriteFont>("spectral-ui");
        _detailFont = Content.Load<SpriteFont>("spectral-detail");
        foreach (ButtonSprite tab in _tabs) tab.LoadContent(Content);

        LayOut();
        LayOutControls();
        foreach (TipTopic topic in Enum.GetValues<TipTopic>()) LayOutTopic(topic);
        Show(0);
    }

    /// <summary>BACK and Escape, then turning the page and the tabs.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        base.HandleInput(gameTime, input);
        if (IsExiting) return;

        if (_previousPage.Occurred(input)) Turn(-1);
        else if (_nextPage.Occurred(input)) Turn(1);

        foreach (ButtonSprite tab in _tabs) tab.Update(gameTime, input);
    }

    /// <summary>Draws the veil, the panel, the tabs and the page's key caps, then the words, then BACK.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        _panel.DrawVeil(spriteBatch, alpha);
        _panel.Draw(spriteBatch, _panelBounds, alpha);
        foreach (ButtonSprite tab in _tabs)
        {
            tab.Opacity = alpha;
            tab.Draw(gameTime, spriteBatch);
        }
        foreach ((IReadOnlyList<string> keys, Vector2 at) in _pageCaps[_shown]) _keys.DrawCaps(spriteBatch, keys, at, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        Text.Draw(spriteBatch, _font, Heading, _panelBounds.Center.X, _panelBounds.Top + FormPanel.Padding,
            Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        foreach ((string text, Vector2 at, Color colour) in _pageWords[_shown])
            Text.Draw(spriteBatch, _detailFont, text, at.X, at.Y, colour * alpha, Palette.FormShadow * alpha);
        foreach ((IReadOnlyList<string> keys, Vector2 at) in _pageCaps[_shown]) _keys.DrawLetters(spriteBatch, keys, at, alpha);

        Text.Draw(spriteBatch, _detailFont, Note, _page.Left, _footY, Palette.Note * alpha, Palette.FormShadow * alpha);
        string counter = string.Format(CultureInfo.InvariantCulture, PageCounter, _shown + 1, _pageTabs.Count);
        Text.Draw(spriteBatch, _detailFont, counter, _page.Right, _footY, Palette.Note * alpha, Palette.FormShadow * alpha, rightAligned: true);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>For the proof run, since it can't press keys: one of the pages, from 0, round past the last.</summary>
    internal void ProofPage(int page) => Show(page % _pageTabs.Count);

    /// <summary>Adds a tab, which jumps to the first page under it when it's clicked.</summary>
    private void AddTab(string label)
    {
        int index = _tabs.Count;
        var tab = new ButtonSprite(label, Vector2.Zero, Palette.Bone, TabSize);
        tab.Clicked += () =>
        {
            int first = _pageTabs.IndexOf(index);
            if (first < 0 || first == _shown) return;

            Audio.Play(Sfx.MenuMove);
            Show(first);
        };
        _tabs.Add(tab);
    }

    /// <summary>Turns a page forward or back, round from the last to the first.</summary>
    /// <param name="step">-1 or 1.</param>
    private void Turn(int step)
    {
        Audio.Play(Sfx.MenuMove);
        Show((_shown + step + _pageTabs.Count) % _pageTabs.Count);
    }

    /// <summary>Shows a page, and lights its tab in torchlight.</summary>
    private void Show(int page)
    {
        _shown = page;
        for (int i = 0; i < _tabs.Count; i++)
        {
            _tabs[i].Accent = i == _pageTabs[page] ? Palette.Torch : Palette.Bone;
            _tabs[i].Focused = i == _pageTabs[page];
        }
    }

    /// <summary>Places the panel, the tabs under the heading, BACK at the bottom, and the page between them.</summary>
    private void LayOut()
    {
        _panelBounds = new Rectangle(PanelInset.X, PanelInset.Y,
            LegendQuestGame.ScreenWidth - PanelInset.X * 2, LegendQuestGame.ScreenHeight - PanelInset.Y * 2);

        float tabsTop = _panelBounds.Top + FormPanel.Padding + _font.LineSpacing + HeadingGap;
        float row = _tabs.Count * TabSize.X + (_tabs.Count - 1) * TabGap;
        for (int i = 0; i < _tabs.Count; i++)
            _tabs[i].Center = new Vector2(_panelBounds.Center.X - row / 2f + i * (TabSize.X + TabGap) + TabSize.X / 2f, tabsTop + TabSize.Y / 2f);

        ButtonSprite back = MenuEntries[0].Plate;
        float backTop = _panelBounds.Bottom - FormPanel.Padding - back.Size.Y;
        LayOutColumn(new Vector2(_panelBounds.Center.X, backTop + back.Size.Y / 2f), 0f);
        _footY = backTop + (back.Size.Y - _detailFont.LineSpacing) / 2f;

        int top = (int)(tabsTop + TabSize.Y + PageGap);
        _page = new Rectangle(_panelBounds.Left + FormPanel.Padding, top,
            _panelBounds.Width - FormPanel.Padding * 2, (int)(backTop - PageGap) - top);
    }

    /// <summary>The controls' page: what each does down the left, its keys as caps, and the pad's way, under their headings, the rows shared out down the page.</summary>
    private void LayOutControls()
    {
        var words = new List<(string Text, Vector2 At, Color Colour)>();
        var caps = new List<(IReadOnlyList<string> Keys, Vector2 At)>();

        float keysX = _page.Left + KeysColumn;
        float padX = _page.Left + PadColumn;
        words.Add((KeyboardHeading, new Vector2(keysX, _page.Top), Palette.Note));
        words.Add((PadHeading, new Vector2(padX, _page.Top), Palette.Note));

        float y = _page.Top + _detailFont.LineSpacing + TipGap;
        float spare = _page.Bottom - y - Controls.Length * KeyCaps.Height;
        float gap = MathHelper.Clamp(spare / Controls.Length, LeastControlGap, MostControlGap);
        foreach ((string what, string[] keys, string pad) in Controls)
        {
            float textY = y + (KeyCaps.Height - _detailFont.LineSpacing) / 2f;
            words.Add((what.ToUpperInvariant(), new Vector2(_page.Left, textY), Palette.Bone));
            caps.Add((keys, new Vector2(keysX, y)));
            if (pad.Length > 0) words.Add((pad.ToUpperInvariant(), new Vector2(padX, textY), Palette.Note));
            y += KeyCaps.Height + gap;
        }

        AddPage(0, words, caps);
    }

    /// <summary>A topic's pages: each tip's title in gold with its keys beside it (or under it, if they won't fit), and its words under that, down the left column, then the right, then onto another page.</summary>
    private void LayOutTopic(TipTopic topic)
    {
        int tab = _tabs.FindIndex(each => each.Label == TopicTabs[topic]);
        float width = (_page.Width - ColumnGap) / 2f;
        float rowHeight = MathF.Max(_detailFont.LineSpacing, KeyCaps.Height);

        var words = new List<(string Text, Vector2 At, Color Colour)>();
        var caps = new List<(IReadOnlyList<string> Keys, Vector2 At)>();
        int column = 0;
        float y = _page.Top;

        foreach (TutorialTip tip in TutorialCatalog.All)
        {
            if (tip.Topic != topic) continue;

            string title = tip.Title.ToUpperInvariant();
            List<string> rows = Text.Wrap(tip.Body, _detailFont, width);
            float keys = _keys.Measure(tip.Keys);
            bool beside = _detailFont.MeasureString(title).X + KeysBeside + keys <= width;
            float height = rowHeight + rows.Count * _detailFont.LineSpacing + (beside || keys == 0f ? 0f : KeyCaps.Height) + TipGap;

            // Off the bottom of this column: the next one, or a new page once both are full.
            if (y + height > _page.Bottom && y > _page.Top)
            {
                y = _page.Top;
                if (++column == 2)
                {
                    AddPage(tab, words, caps);
                    words = new List<(string Text, Vector2 At, Color Colour)>();
                    caps = new List<(IReadOnlyList<string> Keys, Vector2 At)>();
                    column = 0;
                }
            }

            float x = _page.Left + column * (width + ColumnGap);
            words.Add((title, new Vector2(x, y + (rowHeight - _detailFont.LineSpacing) / 2f), Palette.Gold));
            if (beside && keys > 0f) caps.Add((tip.Keys, new Vector2(x + width - keys, y + (rowHeight - KeyCaps.Height) / 2f)));
            y += rowHeight;

            foreach (string line in rows)
            {
                words.Add((line, new Vector2(x, y), Palette.Bone));
                y += _detailFont.LineSpacing;
            }

            if (!beside && keys > 0f)
            {
                caps.Add((tip.Keys, new Vector2(x, y)));
                y += KeyCaps.Height;
            }
            y += TipGap;
        }

        if (words.Count > 0) AddPage(tab, words, caps);
    }

    /// <summary>Adds a laid out page under a tab.</summary>
    private void AddPage(int tab, List<(string Text, Vector2 At, Color Colour)> words, List<(IReadOnlyList<string> Keys, Vector2 At)> caps)
    {
        _pageTabs.Add(tab);
        _pageWords.Add(words);
        _pageCaps.Add(caps);
    }
}
