using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Characters;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.Story;

namespace LegendQuest.Screens;

/// <summary>
/// Popup. A conversation: the scene dims, whoever's speaking has their face and name on the panel,
/// the line types out, and a question's options are numbered under it.
/// </summary>
/// <remarks>
/// It only shows what DialogueRunner says; the runner has the rules. A check opens the dice over it
/// and the next line waits till they're put away. A painting can sit behind a line, and a card
/// line is big in the middle of the screen. Approval, gold, items and whoever joins come up as
/// toasts, and everything the runner did goes out through Happened as it happens, so the map under
/// it can catch up (someone leaving, someone joining). Nothing under it moves while it's up.
/// </remarks>
public class DialogueScreen : GameScreen
{
    private const string NextHint = "ENTER  ·  NEXT";
    private const string LastHint = "ENTER  ·  DONE";
    private const string ChooseHint = "1-4 OR CLICK  ·  CHOOSE";

    /// <summary>Where the panel goes: across the bottom, as tall as what's on it.</summary>
    private const int PanelLeft = 170;
    private const int PanelWidth = 1260;
    private const int PanelBottom = 870;
    private const int Inset = 30;

    /// <summary>The face: how much bigger than art pixels, and the frame round it.</summary>
    private const float PortraitScale = 5f;
    private const int PortraitFrame = 14;

    /// <summary>Where the words start, past the face, and how wide they wrap.</summary>
    private const int TextLeft = 210;
    private const int TextWidth = PanelWidth - TextLeft - Inset;

    /// <summary>An option's row: how tall, how far its words start past the number and the icon, and the icon's size on screen.</summary>
    private const int OptionRow = 36;
    private const int OptionIndent = 70;
    private const int IconSize = 24;

    /// <summary>The face on a sheet: the head and shoulders of the frame facing down.</summary>
    private static readonly Rectangle Bust = new(4, 4, 24, 24);

    /// <summary>The icons beside options, in dialogue-icons.png: 12 art pixels square, in this order.</summary>
    private const int IconCell = 12;
    private enum Icon { Die, Heart, Lock, Honorable, Pragmatic, Witty }

    /// <summary>How dark the scene goes behind the panel, and behind a card.</summary>
    private static readonly Color Veil = Color.Black * 0.38f;
    private static readonly Color CardVeil = Color.Black * 0.82f;

    /// <summary>A card's words: the short ones are drawn in the title's face, the long ones wrap in the UI face.</summary>
    private const int ShortCard = 26;
    private const int CardWidth = 1100;

    /// <summary>The bar a question on a clock drains, and its colour.</summary>
    private const int ClockHeight = 6;
    private static readonly Color ClockColor = new(222, 182, 98);

    /// <summary>How long a painting takes to fade in or out behind the lines.</summary>
    private const float PaintingFade = 0.6f;

    /// <summary>Letters a second at each text speed. Instant shows the whole line.</summary>
    private static readonly Dictionary<TextSpeed, float> LettersPerSecond = new()
    {
        [TextSpeed.Slow] = 28f,
        [TextSpeed.Normal] = 55f,
        [TextSpeed.Fast] = 110f,
        [TextSpeed.Instant] = float.PositiveInfinity,
    };

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.2);

    private readonly InputAction _next = new(new[] { Buttons.A }, new[] { Keys.Enter, Keys.Space, Keys.E }, true);
    private readonly InputAction _up = new(new[] { Buttons.DPadUp }, new[] { Keys.Up, Keys.W }, true);
    private readonly InputAction _down = new(new[] { Buttons.DPadDown }, new[] { Keys.Down, Keys.S }, true);
    private static readonly Keys[] Numbers = { Keys.D1, Keys.D2, Keys.D3, Keys.D4 };

    private readonly DialogueRunner _runner;
    private readonly SaveData _game;
    private readonly Func<string, (string Name, Texture2D Sheet)> _speaker;
    private readonly FormPanel _panel = new();
    private readonly PaintingSprite _paintings = new();
    private readonly QuestBoardSprite _board = new();
    private readonly Toast _toast = new();

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _font;
    private SpriteFont _detailFont;
    private SpriteFont _titleFont;
    private Texture2D _icons;
    private List<string> _wrapped = new();
    private IReadOnlyList<ShownOption> _options = new ShownOption[0];
    private DialogueNode _shownNode;
    private float _shown;
    private int _selected;
    private float _clock;
    private bool _waiting;
    private string _painting;
    private string _lastPainting;
    private float _paintingAlpha;
    private string _travel;

    /// <summary>Raised once the conversation's over, as the screen goes, with where it sends the party ("map spawn"), or null.</summary>
    public event Action<string> Finished;

    /// <summary>Raised for everything the conversation does, as it does it.</summary>
    public event Action<DialogueEvent> Happened;

    /// <summary>Gets a conversation ready to play against a game.</summary>
    /// <param name="script">What gets said.</param>
    /// <param name="game">The game it changes.</param>
    /// <param name="speaker">Who a speaker id is: their name and the sheet their face comes off.</param>
    /// <param name="random">Where the dice come from.</param>
    public DialogueScreen(DialogueScript script, SaveData game, Func<string, (string Name, Texture2D Sheet)> speaker, Random random)
    {
        _game = game;
        _speaker = speaker;
        _runner = new DialogueRunner(script, game, random);

        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;
    }

    /// <summary>Loads the panel, the fonts, the icons and the paintings, and starts the first line.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();
        _panel.LoadContent(_content, ScreenManager.GraphicsDevice);
        _paintings.LoadContent(_content);
        _board.LoadContent(_content, ScreenManager.GraphicsDevice);
        _toast.LoadContent(_content);
        _font = _content.Load<SpriteFont>("spectral-ui");
        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        _titleFont = _content.Load<SpriteFont>("spectral-title");
        _icons = _content.Load<Texture2D>("dialogue-icons");

        Settle();
        _paintingAlpha = _painting is null ? 0f : 1f;
    }

    /// <summary>Unloads everything, and the board's pixel.</summary>
    public override void Unload()
    {
        _board.Unload();
        _content?.Unload();
    }

    /// <summary>Shows the rest of a line, goes on, or picks an option: by key, by number, or by the mouse.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_waiting || _runner.IsOver) return;

        bool typing = _shown < Length();
        bool pressed = _next.Occurred(input) || input.IsNewLeftClick && _options.Count == 0;

        if (typing)
        {
            if (pressed || input.IsNewLeftClick) _shown = Length();
            return;
        }

        if (_options.Count == 0)
        {
            if (pressed) Continue();
            return;
        }

        if (_up.Occurred(input)) Move(-1);
        if (_down.Occurred(input)) Move(1);

        for (int i = 0; i < Numbers.Length && i < _options.Count; i++)
        {
            if (!input.IsNewKeyPress(Numbers[i])) continue;
            Choose(i);
            return;
        }

        int hovered = OptionAt(input.MousePosition);
        if (hovered >= 0 && input.MouseMoved && hovered != _selected)
        {
            _selected = hovered;
            _audio.Play(Sfx.MenuMove);
        }
        if (input.IsNewLeftClick && hovered >= 0) Choose(hovered);
        else if (_next.Occurred(input)) Choose(_selected);
    }

    /// <summary>Types the line out, runs a question's clock, fades the painting, and runs the toasts.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _paintings.Update(gameTime);
        _board.Update(gameTime);
        _toast.Update(gameTime);

        float target = _painting is null ? 0f : 1f;
        _paintingAlpha = MathHelper.Clamp(_paintingAlpha + Math.Sign(target - _paintingAlpha) * seconds / PaintingFade, 0f, 1f);
        if (_painting is not null) _lastPainting = _painting;

        if (_waiting || _runner.IsOver) return;
        _shown += LettersPerSecond[_audio.Settings.Typing] * seconds;

        DialogueNode node = _runner.Current;
        if (node.Seconds <= 0f || _options.Count == 0 || _shown < Length()) return;

        _clock += seconds;
        if (_clock < node.Seconds) return;

        _runner.TimeOut();
        Settle();
    }

    /// <summary>Draws the painting or the veil, then the card or the panel with the face, the name, the line and the options, then the toasts.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;
        DialogueNode node = _shownNode;
        if (node is null) return;
        bool card = node.Speaker == Speakers.Card;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        var screen = new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight);
        if (_lastPainting is not null && _paintingAlpha > 0f)
        {
            spriteBatch.Draw(ScreenManager.BlankTexture, screen, Color.Black * (alpha * _paintingAlpha));
            _paintings.Draw(spriteBatch, _lastPainting, alpha * _paintingAlpha);
        }
        spriteBatch.Draw(ScreenManager.BlankTexture, screen, (card && _painting is null ? CardVeil : Veil) * alpha);
        spriteBatch.End();

        if (node.Board)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            _board.Draw(spriteBatch, alpha);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        // While the dice are up the panel waits behind them with nothing on it, so it isn't drawn at all.
        Rectangle panel = PanelBounds(node);
        if (!card && !_waiting)
        {
            _panel.Draw(spriteBatch, panel, alpha);
            DrawPortrait(spriteBatch, node, panel, alpha);
            DrawIcons(spriteBatch, panel, alpha);
            DrawClock(spriteBatch, node, panel, alpha);
        }
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        if (card) DrawCard(spriteBatch, alpha);
        else if (!_waiting) DrawWords(spriteBatch, node, panel, alpha);
        _toast.Draw(spriteBatch, alpha);
        spriteBatch.End();
    }

    /// <summary>For the proof run: the same as pressing Enter, which shows the rest of a line, goes on, or picks the option that's selected.</summary>
    internal void ProofAdvance()
    {
        if (_waiting || _runner.IsOver) return;
        if (_shown < Length()) _shown = Length();
        else if (_options.Count == 0) Continue();
        else Choose(_selected);
    }

    /// <summary>For the proof run: whether it's waiting on an option to be picked, with the question typed out.</summary>
    internal bool ProofAsking => !_waiting && !_runner.IsOver && _options.Count > 0 && _shown >= Length();

    /// <summary>For the proof run: the line it's on, so a shot can wait for the right one.</summary>
    internal DialogueNode ProofLine => _runner.Current;

    /// <summary>For the proof run: finishes typing the line, and nothing else, so a shot shows all of it.</summary>
    internal void ProofFinishLine() => _shown = Math.Max(_shown, Length());

    /// <summary>For the proof run: picks an option by its place on the panel, from 0, once the line's typed.</summary>
    internal void ProofChoose(int index)
    {
        if (_waiting || _runner.IsOver) return;
        _shown = Length();
        if (index < _options.Count) Choose(index);
    }

    /// <summary>Goes on past a line that isn't a question.</summary>
    private void Continue()
    {
        _audio.Play(Sfx.MenuMove);
        _runner.Continue();
        Settle();
    }

    /// <summary>Picks an option, if it's open.</summary>
    private void Choose(int index)
    {
        if (index < 0 || index >= _options.Count) return;
        if (!_options[index].Open)
        {
            _audio.Play(Sfx.MenuBack);
            return;
        }

        _audio.Play(Sfx.MenuConfirm);
        _runner.Choose(_options[index]);
        Settle();
    }

    /// <summary>Moves the selection between the options, wrapping round.</summary>
    private void Move(int step)
    {
        _selected = (_selected + step + _options.Count) % _options.Count;
        _audio.Play(Sfx.MenuMove);
    }

    /// <summary>Catches up with the runner: plays out what it did, then shows its line, or closes if it's over.</summary>
    /// <remarks>A check puts the dice up first, and the line waits behind them.</remarks>
    private void Settle()
    {
        foreach (DialogueEvent happened in _runner.Drain()) Cue(happened);

        if (_runner.IsOver)
        {
            _painting = null;
            if (!_waiting) Close();
            return;
        }

        DialogueNode node = _runner.Current;
        _shownNode = node;
        _options = _runner.Options();
        _selected = FirstOpen();
        _shown = 0f;
        _clock = 0f;
        _painting = node.Painting;
        if (node.Sound is Sfx sound) _audio.Play(sound);
        _wrapped = Wrap(TextTokens.Fill(node.Line, _game), _font, node.Speaker == Speakers.Card ? CardWidth : TextWidth);
        if (node.Speaker == Speakers.Card) _shown = Length();
    }

    /// <summary>Turns one thing the runner did into a toast, a sound, or the dice, and passes it on.</summary>
    private void Cue(DialogueEvent happened)
    {
        switch (happened.Kind)
        {
            case DialogueEventKind.Check:
                _waiting = true;
                var dice = new DiceRollOverlay(happened.Check);
                dice.Closed += () =>
                {
                    _waiting = false;
                    if (_runner.IsOver) Close();
                };
                ScreenManager.AddScreen(dice);
                break;

            case DialogueEventKind.Meter:
                if (happened.Amount == 0) break;
                MeterDefinition meter = Meters.Get(happened.Id);
                if (meter.Companion is null || !_game.InParty(meter.Companion)) break;
                bool approves = happened.Amount > 0;
                _toast.Show(_game.Member(meter.Companion).Name + (approves ? " approves" : " disapproves"));
                _audio.Play(approves ? Sfx.Approve : Sfx.Disapprove);
                break;

            case DialogueEventKind.Gold:
                _toast.Show(happened.Amount + " gold");
                _audio.Play(Sfx.Pickup);
                break;

            case DialogueEventKind.Item:
                if (happened.Amount == 0) break;
                _toast.Show("Received  ·  " + KeyItems.Get(happened.Id).Name);
                _audio.Play(Sfx.Pickup);
                break;

            case DialogueEventKind.Recruited:
                string name = happened.Amount == 1 ? _game.Member(happened.Id).Name : Origins.NameOf(happened.Id);
                _toast.Show(name + (happened.Amount == 1 ? " joins the party" : " waits at camp"));
                _audio.Play(Sfx.Waypoint);
                break;

            case DialogueEventKind.Travel:
                _travel = happened.Id;
                break;
        }
        Happened?.Invoke(happened);
    }

    /// <summary>Closes the screen and says where the party's to go.</summary>
    private void Close()
    {
        if (IsExiting) return;
        ExitScreen();
        Finished?.Invoke(_travel);
    }

    /// <summary>The first option that can be picked, or 0.</summary>
    private int FirstOpen()
    {
        for (int i = 0; i < _options.Count; i++)
            if (_options[i].Open) return i;
        return 0;
    }

    /// <summary>The panel: as tall as the name, the line and the options need, from the bottom of the screen up.</summary>
    private Rectangle PanelBounds(DialogueNode node)
    {
        int text = Math.Max(3, _wrapped.Count) * _font.LineSpacing;
        int options = _options.Count == 0 ? 0 : 12 + _options.Count * OptionRow;
        int height = Inset * 2 + _detailFont.LineSpacing + 8 + text + options + _detailFont.LineSpacing;
        int portrait = (int)(Bust.Height * PortraitScale) + Inset * 2;
        if (node.Speaker != Speakers.Narrator) height = Math.Max(height, portrait);
        return new Rectangle(PanelLeft, PanelBottom - height, PanelWidth, height);
    }

    /// <summary>The speaker's face, in its frame, on the left of the panel.</summary>
    private void DrawPortrait(SpriteBatch spriteBatch, DialogueNode node, Rectangle panel, float alpha)
    {
        if (node.Speaker == Speakers.Narrator) return;

        (_, Texture2D sheet) = _speaker(node.Speaker);
        int size = (int)(Bust.Width * PortraitScale);
        var face = new Rectangle(panel.Left + Inset, panel.Top + Inset, size, size);
        _panel.Draw(spriteBatch, new Rectangle(face.X - PortraitFrame, face.Y - PortraitFrame, size + PortraitFrame * 2, size + PortraitFrame * 2), alpha);
        if (sheet is not null) spriteBatch.Draw(sheet, face, Bust, Color.White * alpha);
    }

    /// <summary>The icon beside each option that has one: a die for a check, a heart for a flirt, the voice for a voice's, a lock for the rest that need something.</summary>
    private void DrawIcons(SpriteBatch spriteBatch, Rectangle panel, float alpha)
    {
        if (_shown < Length()) return;
        for (int i = 0; i < _options.Count; i++)
        {
            if (IconFor(_options[i]) is not Icon icon) continue;
            var at = new Rectangle(panel.Left + TextLeft + 34, OptionTop(panel) + i * OptionRow + (OptionRow - IconSize) / 2, IconSize, IconSize);
            var source = new Rectangle((int)icon * IconCell, 0, IconCell, IconCell);
            spriteBatch.Draw(_icons, at, source, Color.White * (_options[i].Open ? alpha : alpha * 0.5f));
        }
    }

    /// <summary>A question on a clock: the bar draining along the top of the panel.</summary>
    private void DrawClock(SpriteBatch spriteBatch, DialogueNode node, Rectangle panel, float alpha)
    {
        if (node.Seconds <= 0f || _options.Count == 0 || _shown < Length()) return;
        float left = MathHelper.Clamp(1f - _clock / node.Seconds, 0f, 1f);
        var bar = new Rectangle(panel.Left + Inset, panel.Top - ClockHeight - 6, (int)((panel.Width - Inset * 2) * left), ClockHeight);
        spriteBatch.Draw(ScreenManager.BlankTexture, bar, ClockColor * alpha);
    }

    /// <summary>The speaker's name and how they say it, the line as far as it's typed, the options once it's done, and the hint.</summary>
    private void DrawWords(SpriteBatch spriteBatch, DialogueNode node, Rectangle panel, float alpha)
    {
        bool narrator = node.Speaker == Speakers.Narrator;
        float left = panel.Left + (narrator ? Inset * 2 : TextLeft), top = panel.Top + Inset - 4f;

        if (!narrator)
        {
            string name = TextTokens.Fill(_speaker(node.Speaker).Name, _game).ToUpperInvariant();
            Text.Draw(spriteBatch, _detailFont, name, left, top, Palette.Torch * alpha, Palette.FormShadow * alpha);
            if (node.Direction is not null)
            {
                float after = left + _detailFont.MeasureString(name).X + 18f;
                Text.Draw(spriteBatch, _detailFont, node.Direction, after, top, Palette.Note * alpha, Palette.FormShadow * alpha);
            }
        }

        int letters = (int)_shown;
        float y = top + _detailFont.LineSpacing + 8f;
        Color words = narrator ? Palette.Narration : Palette.Bone;
        foreach (string row in _wrapped)
        {
            string part = letters >= row.Length ? row : row[..Math.Max(0, letters)];
            Text.Draw(spriteBatch, _font, part, left, y, words * alpha, Palette.FormShadow * alpha);
            letters -= row.Length + 1;
            y += _font.LineSpacing;
        }

        bool typed = _shown >= Length();
        if (typed)
        {
            for (int i = 0; i < _options.Count; i++)
            {
                ShownOption option = _options[i];
                float rowY = OptionTop(panel) + i * OptionRow + (OptionRow - _detailFont.LineSpacing) / 2f;
                Color colour = !option.Open ? Palette.Note : i == _selected ? Palette.Torch : Palette.Bone;
                string text = (i + 1) + ".";
                Text.Draw(spriteBatch, _detailFont, text, panel.Left + TextLeft, rowY, colour * alpha, Palette.FormShadow * alpha);
                string said = TextTokens.Fill(option.Option.Text, _game);
                if (!option.Open) said += "   [" + option.Reason + "]";
                else if (option.Option.Check is DialogueCheck check) said = "[" + check.Label + " DC " + check.Dc + "]  " + said;
                Text.Draw(spriteBatch, _detailFont, said, panel.Left + TextLeft + OptionIndent, rowY, colour * alpha, Palette.FormShadow * alpha);
            }
        }

        string hint = !typed ? string.Empty : _options.Count > 0 ? ChooseHint : NextIsLast() ? LastHint : NextHint;
        Text.Draw(spriteBatch, _detailFont, hint, panel.Right - Inset, panel.Bottom - Inset - _detailFont.LineSpacing + 6f,
            Palette.Note * alpha, Palette.FormShadow * alpha, rightAligned: true);
    }

    /// <summary>A card: the line big in the middle of the screen.</summary>
    private void DrawCard(SpriteBatch spriteBatch, float alpha)
    {
        string text = TextTokens.Fill(_shownNode.Line, _game);
        if (text.Length <= ShortCard)
        {
            float y = (LegendQuestGame.ScreenHeight - _titleFont.LineSpacing) / 2f;
            Text.DrawCentered(spriteBatch, _titleFont, text, y, Palette.Bone * alpha, Palette.Shadow * alpha);
        }
        else
        {
            float y = (LegendQuestGame.ScreenHeight - _wrapped.Count * _font.LineSpacing) / 2f;
            foreach (string row in _wrapped)
            {
                Text.DrawCentered(spriteBatch, _font, row, y, Palette.Bone * alpha, Palette.Shadow * alpha);
                y += _font.LineSpacing;
            }
        }
        Text.DrawCentered(spriteBatch, _detailFont, NextIsLast() ? LastHint : NextHint, LegendQuestGame.ScreenHeight - 60f,
            Palette.Note * alpha, Palette.Shadow * alpha);
    }

    /// <summary>Where the options start on the panel: under the line.</summary>
    private int OptionTop(Rectangle panel) =>
        panel.Top + Inset - 4 + _detailFont.LineSpacing + 8 + Math.Max(1, _wrapped.Count) * _font.LineSpacing + 12;

    /// <summary>Which option the mouse is over, or -1.</summary>
    private int OptionAt(Point mouse)
    {
        if (_shownNode is null || _options.Count == 0) return -1;
        Rectangle panel = PanelBounds(_shownNode);
        int top = OptionTop(panel);
        for (int i = 0; i < _options.Count; i++)
            if (new Rectangle(panel.Left + TextLeft, top + i * OptionRow, TextWidth, OptionRow).Contains(mouse)) return i;
        return -1;
    }

    /// <summary>The icon an option gets, or null for none.</summary>
    private static Icon? IconFor(ShownOption shown)
    {
        DialogueOption option = shown.Option;
        if (option.Check is not null) return Icon.Die;
        if (option.Flirt) return Icon.Heart;
        foreach (DialogueCondition condition in option.Conditions)
        {
            if (condition.Kind != ConditionKind.Voice) continue;
            return Enum.Parse<Voice>(condition.Id) switch
            {
                Voice.Honorable => Icon.Honorable,
                Voice.Pragmatic => Icon.Pragmatic,
                _ => Icon.Witty,
            };
        }
        return option.Conditions.Count > 0 ? Icon.Lock : null;
    }

    /// <summary>Whether going on from this line ends the conversation, so the hint can say DONE.</summary>
    private bool NextIsLast()
    {
        DialogueNode node = _runner.Current;
        if (node is null) return true;
        if (node.Next == DialogueScript.End) return true;
        return node.Next is null && _runner.Script.Nodes[^1] == node;
    }

    /// <summary>How many letters the whole line is, with the gaps where it wraps.</summary>
    private int Length()
    {
        int length = -1;
        foreach (string row in _wrapped) length += row.Length + 1;
        return Math.Max(0, length);
    }

    /// <summary>Breaks a line into rows that fit a width, at the spaces.</summary>
    private static List<string> Wrap(string text, SpriteFont font, float width)
    {
        var rows = new List<string>();
        var row = new StringBuilder();
        foreach (string word in text.Split(' '))
        {
            string tried = row.Length == 0 ? word : row + " " + word;
            if (row.Length > 0 && font.MeasureString(tried).X > width)
            {
                rows.Add(row.ToString());
                row.Clear().Append(word);
            }
            else
            {
                row.Clear().Append(tried);
            }
        }
        if (row.Length > 0) rows.Add(row.ToString());
        return rows;
    }
}
