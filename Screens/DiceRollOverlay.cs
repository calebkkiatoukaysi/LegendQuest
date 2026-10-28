using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Components;
using LegendQuest.Rules;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// Popup. A check on screen: the d20 with the DC over it and every modifier listed, a press to
/// roll it, the tumble, then the natural roll, the total, and SUCCESS or FAILURE.
/// </summary>
/// <remarks>
/// The rules rolled it already (AbilityCheck); this only shows it, so a check comes out the same
/// whether it's watched or not, the way the Black Box's table showed what the round engine said.
/// With advantage the second die shows beside the first and the lower one greys out.
/// </remarks>
public class DiceRollOverlay : GameScreen
{
    private const string RollHint = "SPACE OR CLICK TO ROLL";
    private const string DoneHint = "SPACE OR CLICK TO GO ON";
    private const string AdvantageNote = "ADVANTAGE";
    private const string SuccessWord = "SUCCESS";
    private const string FailureWord = "FAILURE";

    /// <summary>The panel in the middle, and where the dice sit on it.</summary>
    private static readonly Rectangle PanelBounds = new(500, 130, 600, 650);
    private static readonly Vector2 DieCentre = new(800f, 390f);

    /// <summary>The d20 sheet: how big a frame is, how many tumble frames come before the face, and how much bigger it's drawn.</summary>
    private const int DieFrame = 40;
    private const int TumbleFrames = 8;
    private const float DieScale = 4f;

    /// <summary>How far apart the two dice are with advantage, and how far the number sits under the die's middle.</summary>
    private const float PairGap = 92f;
    private const float NumberDrop = 6f;

    /// <summary>How long the tumble lasts, and how long a tumble frame is up.</summary>
    private const float TumbleSeconds = 0.9f;
    private const float TumbleFrameTime = 0.07f;

    /// <summary>How the losing die of an advantage roll is drawn.</summary>
    private static readonly Color Dropped = Color.White * 0.35f;

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.2);

    private readonly InputAction _press = new(new[] { Buttons.A }, new[] { Keys.Space, Keys.Enter, Keys.E }, true);
    private readonly CheckResult _result;
    private readonly FormPanel _panel = new();

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Texture2D _dice;
    private float _clock;
    private bool _rolled;
    private bool _landed;

    /// <summary>Raised as it closes, once the result's been seen.</summary>
    public event Action Closed;

    /// <summary>Gets a check ready to show.</summary>
    /// <param name="result">The check, already rolled.</param>
    public DiceRollOverlay(CheckResult result)
    {
        _result = result;
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;
    }

    /// <summary>Loads the panel, the fonts and the dice.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();
        _panel.LoadContent(_content, ScreenManager.GraphicsDevice);
        _font = _content.Load<SpriteFont>("spectral-ui");
        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        _dice = _content.Load<Texture2D>("dice");
    }

    /// <summary>Unloads the panel and the dice.</summary>
    public override void Unload() => _content?.Unload();

    /// <summary>A press rolls, and once it's landed, a press goes on.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_press.Occurred(input) || input.IsNewLeftClick) Press();
    }

    /// <summary>Runs the tumble, and lands it.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        if (!_rolled || _landed) return;

        _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_clock < TumbleSeconds) return;

        _landed = true;
        _audio.Play(_result.Success ? Sfx.CheckPass : Sfx.CheckFail);
    }

    /// <summary>Draws the veil, the panel, the label and the DC, the dice, the modifiers, and once it's landed, the total and the verdict.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _panel.DrawVeil(spriteBatch, alpha);
        _panel.Draw(spriteBatch, PanelBounds, alpha);
        DrawDice(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        float y = PanelBounds.Top + FormPanel.Padding;
        Text.Draw(spriteBatch, _font, _result.Label, PanelBounds.Center.X, y, Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        y += _font.LineSpacing;
        Text.Draw(spriteBatch, _detailFont, "DC " + _result.Dc, PanelBounds.Center.X, y, Palette.Torch * alpha, Palette.FormShadow * alpha, centred: true);

        if (_landed)
        {
            foreach (DieAt die in Dice())
                Text.Draw(spriteBatch, _font, die.Value.ToString(), die.Centre.X, die.Centre.Y - _font.LineSpacing / 2f + NumberDrop,
                    Palette.Ink * (die.Kept ? alpha : alpha * 0.5f), Color.Transparent, centred: true);
        }

        y = DieCentre.Y + DieFrame * DieScale / 2f + 16f;
        foreach ((string source, int amount) in _result.Modifiers)
        {
            Text.Draw(spriteBatch, _detailFont, source + "  " + Signed(amount), PanelBounds.Center.X, y, Palette.Bone * alpha, Palette.FormShadow * alpha, centred: true);
            y += _detailFont.LineSpacing;
        }
        if (_result.Advantage)
        {
            Text.Draw(spriteBatch, _detailFont, AdvantageNote, PanelBounds.Center.X, y, Palette.Gold * alpha, Palette.FormShadow * alpha, centred: true);
            y += _detailFont.LineSpacing;
        }

        if (_landed)
        {
            y += 6f;
            Text.Draw(spriteBatch, _font, "TOTAL " + _result.Total, PanelBounds.Center.X, y, Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
            y += _font.LineSpacing;
            Text.Draw(spriteBatch, _font, _result.Success ? SuccessWord : FailureWord, PanelBounds.Center.X, y,
                (_result.Success ? Palette.Gold : Palette.Ember) * alpha, Palette.FormShadow * alpha, centred: true);
        }

        string hint = _landed ? DoneHint : _rolled ? string.Empty : RollHint;
        Text.Draw(spriteBatch, _detailFont, hint, PanelBounds.Center.X, PanelBounds.Bottom - FormPanel.Padding - _detailFont.LineSpacing,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        spriteBatch.End();
    }

    /// <summary>For the proof run: whether the dice have landed, so a shot can show them.</summary>
    internal bool ProofLanded => _landed;

    /// <summary>For the proof run: the same as a press.</summary>
    internal void ProofPress() => Press();

    /// <summary>Rolls, or goes on once it's landed. A press while it tumbles does nothing.</summary>
    private void Press()
    {
        if (IsExiting) return;

        if (!_rolled)
        {
            _rolled = true;
            _audio.Play(Sfx.DiceRoll);
            return;
        }

        if (!_landed) return;
        ExitScreen();
        Closed?.Invoke();
    }

    /// <summary>The dice: tumbling, or landed on their faces.</summary>
    private void DrawDice(SpriteBatch spriteBatch, float alpha)
    {
        int tumble = (int)(_clock / TumbleFrameTime) % TumbleFrames;
        foreach (DieAt die in Dice())
        {
            int frame = _landed ? TumbleFrames : _rolled ? (tumble + die.Index * 3) % TumbleFrames : 0;
            var source = new Rectangle(frame * DieFrame, 0, DieFrame, DieFrame);
            Color tint = !_landed || die.Kept ? Color.White : Dropped;
            spriteBatch.Draw(_dice, die.Centre, source, tint * alpha, 0f, new Vector2(DieFrame / 2f), DieScale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Where each die is, what it rolled, and whether it's the one that counts.</summary>
    private DieAt[] Dice()
    {
        if (_result.Rolls.Count == 1) return new[] { new DieAt(0, DieCentre, _result.Rolls[0], true) };

        // The kept die is the first one that rolled the kept number.
        bool firstKept = _result.Rolls[0] == _result.Kept;
        return new[]
        {
            new DieAt(0, DieCentre - new Vector2(PairGap, 0f), _result.Rolls[0], firstKept),
            new DieAt(1, DieCentre + new Vector2(PairGap, 0f), _result.Rolls[1], !firstKept),
        };
    }

    /// <summary>A number with its sign, the way modifiers are written: +2, -1, +0.</summary>
    private static string Signed(int amount) => amount < 0 ? amount.ToString() : "+" + amount;

    /// <summary>One die on the panel.</summary>
    private readonly record struct DieAt(int Index, Vector2 Centre, int Value, bool Kept);
}
