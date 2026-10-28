using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// Popup. A conversation, a line at a time: the scene dims, whoever's speaking has their face and
/// name on the panel, and the line types out at the speed in the options.
/// </summary>
/// <remarks>
/// Enter, Space, E or a click shows the rest of a line that's still typing, or goes on to the next
/// one. After the last, the screen closes and tells whoever opened it, which is where the
/// conversation's flags and gold get handed out. Nothing under it moves while it's up, the same as
/// the pause menu.
/// </remarks>
public class DialogueScreen : GameScreen
{
    private const string Hint = "ENTER  ·  NEXT";
    private const string LastHint = "ENTER  ·  DONE";

    /// <summary>The panel along the bottom, and where things go on it.</summary>
    private static readonly Rectangle PanelBounds = new(170, 610, 1260, 260);
    private const int Inset = 34;
    private const float PortraitScale = 5f;
    private const int PortraitFrame = 14;
    private const int TextLeft = 230;

    /// <summary>The face on a sheet: the head and shoulders of the frame facing down, the same as the creation screen.</summary>
    private static readonly Rectangle Bust = new(4, 4, 24, 24);

    /// <summary>How dark the scene goes behind the panel.</summary>
    private static readonly Color Veil = Color.Black * 0.38f;

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

    private readonly Conversation _conversation;
    private readonly Func<string, (string Name, Texture2D Sheet)> _speaker;
    private readonly string _yourName;
    private readonly FormPanel _panel = new();

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _font;
    private SpriteFont _detailFont;
    private int _line;
    private float _shown;
    private List<string> _wrapped;

    /// <summary>Raised once the last line's been read, as the screen goes.</summary>
    public event Action Finished;

    /// <summary>Gets a conversation ready to play.</summary>
    /// <param name="conversation">What gets said.</param>
    /// <param name="speaker">Who a speaker id is: their name and the sheet their face comes off.</param>
    /// <param name="yourName">Your name, for {name}.</param>
    public DialogueScreen(Conversation conversation, Func<string, (string Name, Texture2D Sheet)> speaker, string yourName)
    {
        _conversation = conversation;
        _speaker = speaker;
        _yourName = yourName;

        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;
    }

    /// <summary>Loads the panel and the fonts and starts the first line.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();
        _panel.LoadContent(_content, ScreenManager.GraphicsDevice);
        _font = _content.Load<SpriteFont>("spectral-ui");
        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        Start(0);
    }

    /// <summary>Unloads the panel.</summary>
    public override void Unload() => _content?.Unload();

    /// <summary>Shows the rest of the line, or goes on.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_next.Occurred(input) || input.IsNewLeftClick) Advance();
    }

    /// <summary>Types the line out.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        _shown += LettersPerSecond[_audio.Settings.Typing] * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>Draws the veil, the panel, the face, the name, as much of the line as has typed, and the hint.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;
        Line line = _conversation.Lines[_line];
        (string name, Texture2D sheet) = _speaker(line.Speaker);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        spriteBatch.Draw(ScreenManager.BlankTexture, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight), Veil * alpha);
        _panel.Draw(spriteBatch, PanelBounds, alpha);
        int size = (int)(Bust.Width * PortraitScale);
        var portrait = new Rectangle(PanelBounds.Left + Inset, PanelBounds.Top + Inset, size, size);
        _panel.Draw(spriteBatch, new Rectangle(portrait.X - PortraitFrame, portrait.Y - PortraitFrame,
            size + PortraitFrame * 2, size + PortraitFrame * 2), alpha);
        if (sheet is not null) spriteBatch.Draw(sheet, portrait, Bust, Color.White * alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        float left = PanelBounds.Left + TextLeft, top = PanelBounds.Top + Inset - 4f;
        Text.Draw(spriteBatch, _detailFont, name.ToUpperInvariant(), left, top, Palette.Torch * alpha, Palette.FormShadow * alpha);

        int letters = (int)_shown;
        float y = top + _detailFont.LineSpacing + 8f;
        foreach (string row in _wrapped)
        {
            string part = letters >= row.Length ? row : row[..Math.Max(0, letters)];
            Text.Draw(spriteBatch, _font, part, left, y, Palette.Bone * alpha, Palette.FormShadow * alpha);
            letters -= row.Length + 1;
            y += _font.LineSpacing;
        }

        string hint = _line == _conversation.Lines.Count - 1 ? LastHint : Hint;
        Text.Draw(spriteBatch, _detailFont, hint, PanelBounds.Right - Inset, PanelBounds.Bottom - Inset - _detailFont.LineSpacing,
            Palette.Note * alpha, Palette.FormShadow * alpha, rightAligned: true);
        spriteBatch.End();
    }

    /// <summary>For the proof run: the same as pressing Enter.</summary>
    internal void ProofAdvance() => Advance();

    /// <summary>Shows the rest of a line that's still typing, or goes on to the next, or closes after the last.</summary>
    private void Advance()
    {
        if (IsExiting) return;

        if (_shown < Length())
        {
            _shown = Length();
            return;
        }

        if (_line + 1 < _conversation.Lines.Count)
        {
            _audio.Play(Sfx.MenuMove);
            Start(_line + 1);
            return;
        }

        _audio.Play(Sfx.MenuConfirm);
        ExitScreen();
        Finished?.Invoke();
    }

    /// <summary>Starts typing one line, wrapped to the panel.</summary>
    private void Start(int index)
    {
        _line = index;
        _shown = 0f;
        string text = _conversation.Lines[index].Text.Replace("{name}", _yourName);
        _wrapped = Wrap(text, PanelBounds.Width - TextLeft - Inset);
    }

    /// <summary>How many letters the whole line is, with the gaps where it wraps.</summary>
    private int Length()
    {
        int length = -1;
        foreach (string row in _wrapped) length += row.Length + 1;
        return Math.Max(0, length);
    }

    /// <summary>Breaks a line into rows that fit a width, at the spaces.</summary>
    private List<string> Wrap(string text, float width)
    {
        var rows = new List<string>();
        var row = new StringBuilder();
        foreach (string word in text.Split(' '))
        {
            string tried = row.Length == 0 ? word : row + " " + word;
            if (row.Length > 0 && _font.MeasureString(tried).X > width)
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
