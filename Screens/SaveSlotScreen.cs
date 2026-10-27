using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Components;
using LegendQuest.Save;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// The save slots as a popup. Whoever opens it decides what happens to the slot that gets picked.
/// </summary>
/// <remarks>
/// Same as The Black Box's except it doesn't send the game anywhere itself. It just says which slot
/// was picked, so the title can use it for a new game and for loading, and the pause menu can use
/// it later for saving.
/// </remarks>
public class SaveSlotScreen : GameScreen
{
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.25);

    private readonly SaveSlotMenu _menu;

    private readonly InputAction _up = new(new[] { Buttons.DPadUp, Buttons.LeftThumbstickUp }, new[] { Keys.Up, Keys.W }, true);
    private readonly InputAction _down = new(new[] { Buttons.DPadDown, Buttons.LeftThumbstickDown }, new[] { Keys.Down, Keys.S }, true);
    private readonly InputAction _left = new(new[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft }, new[] { Keys.Left, Keys.A }, true);
    private readonly InputAction _right = new(new[] { Buttons.DPadRight, Buttons.LeftThumbstickRight }, new[] { Keys.Right, Keys.D }, true);
    private readonly InputAction _select = new(new[] { Buttons.A }, new[] { Keys.Enter, Keys.Space }, true);
    private readonly InputAction _cancel = new(new[] { Buttons.B, Buttons.Back }, new[] { Keys.Escape }, true);

    private ContentManager _content;
    private AudioManager _audio;

    /// <summary>Raised once a slot is picked and the form has started to fade, with the game in it.</summary>
    public event Action<int, SaveData> SlotChosen;

    /// <summary>Raised when the form is backed out of without picking anything.</summary>
    public event Action Closed;

    /// <summary>Hooks up what happens when a slot is picked or the form is closed.</summary>
    /// <param name="purpose">Starting a new game or loading one.</param>
    public SaveSlotScreen(SlotPurpose purpose)
    {
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _menu = new SaveSlotMenu(purpose);
        _menu.SlotChosen += (slot, game) =>
        {
            _audio.Play(Sfx.MenuConfirm);
            ExitScreen();
            SlotChosen?.Invoke(slot, game);
        };
        _menu.Closed += () =>
        {
            _audio.Play(Sfx.MenuBack);
            ExitScreen();
            Closed?.Invoke();
        };
    }

    /// <summary>Loads the form and opens it with whatever is on disk right now.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();

        _menu.LoadContent(_content, ScreenManager.GraphicsDevice);
        _menu.Open();
    }

    /// <summary>Unloads the form.</summary>
    public override void Unload() => _content?.Unload();

    /// <summary>Moves around the form with the keyboard, follows the mouse, backs out on Escape, and runs the plates.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (!_menu.IsOpen) return;

        bool moved =
            input.MouseMoved && _menu.FollowMouse(input.MousePosition) ||
            _up.Occurred(input) && _menu.Navigate(-1, 0) ||
            _down.Occurred(input) && _menu.Navigate(1, 0) ||
            _left.Occurred(input) && _menu.Navigate(0, -1) ||
            _right.Occurred(input) && _menu.Navigate(0, 1);

        if (moved) _audio.Play(Sfx.MenuMove);

        if (_select.Occurred(input)) _menu.Press();
        else if (_cancel.Occurred(input)) _menu.Close();

        _menu.Update(gameTime, input);
    }

    /// <summary>Draws the form, faded with the transition.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        _menu.Opacity = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        _menu.Draw(gameTime, spriteBatch);
        spriteBatch.End();
    }

    /// <summary>For the proof run, since it can't press keys: presses one slot's ERASE.</summary>
    /// <param name="slot">Which slot.</param>
    internal void ProofErase(int slot) => _menu.ProofErase(slot);
}
