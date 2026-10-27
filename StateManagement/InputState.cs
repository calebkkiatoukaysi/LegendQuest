// Adapted from the Game State Management sample from Microsoft XNA Game Studio 4.0
// Archived at https://github.com/SimonDarksideJ/GameStateManagementSample

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LegendQuest.StateManagement;

/// <summary>
/// The keyboard, mouse and gamepads, this frame and last, so a screen can tell a press from a hold.
/// </summary>
/// <remarks>
/// Pretty much the advanced input tutorial's InputState, like The Black Box's. The differences:
/// the ScreenManager updates it instead of it being a component, and the mouse is in here too.
/// The Black Box's buttons read Mouse.GetState themselves, so the proof run couldn't keep the real
/// mouse from lighting one up. Now nothing reads a device except this class.
/// </remarks>
public class InputState : IInputState
{
    /// <summary>The most gamepads MonoGame reports.</summary>
    private const int MaxInputs = 4;

    /// <summary>A mouse that's off the window with nothing held, for when the devices are being ignored.</summary>
    private static readonly MouseState NoMouse = new(-1, -1, 0,
        ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);

    /// <summary>Every gamepad this frame.</summary>
    public readonly GamePadState[] CurrentGamePadStates = new GamePadState[MaxInputs];

    /// <summary>The keyboard this frame.</summary>
    public KeyboardState CurrentKeyboardState;

    /// <summary>The mouse this frame.</summary>
    public MouseState CurrentMouseState;

    private readonly GamePadState[] _priorGamePadStates = new GamePadState[MaxInputs];
    private KeyboardState _priorKeyboardState;
    private MouseState _priorMouseState;

    /// <summary>Whether the first frame has been read yet.</summary>
    private bool _sampled;

    /// <summary>When true every device reads as untouched, and the mouse as off the window unless the proof run has put it somewhere.</summary>
    /// <remarks>The proof run turns this on, so whatever I do with the mouse and keyboard while it runs can't change what it shoots.</remarks>
    public bool IgnoreDevices { get; set; }

    /// <summary>The mouse the proof run is pretending there is, while the real devices are ignored.</summary>
    private MouseState _proofMouse = NoMouse;

    /// <summary>Reads every device and keeps the last frame for comparison.</summary>
    public void Update()
    {
        for (int i = 0; i < MaxInputs; i++)
        {
            _priorGamePadStates[i] = CurrentGamePadStates[i];
            CurrentGamePadStates[i] = IgnoreDevices ? default : GamePad.GetState((PlayerIndex)i);
        }

        _priorKeyboardState = CurrentKeyboardState;
        CurrentKeyboardState = IgnoreDevices ? default : Keyboard.GetState();

        _priorMouseState = CurrentMouseState;
        CurrentMouseState = IgnoreDevices ? _proofMouse : Mouse.GetState();

        // A key that's already held when the game starts shouldn't count as a new press.
        if (!_sampled)
        {
            _sampled = true;
            _priorKeyboardState = CurrentKeyboardState;
            _priorMouseState = CurrentMouseState;
            for (int i = 0; i < MaxInputs; i++) _priorGamePadStates[i] = CurrentGamePadStates[i];
        }
    }

    /// <summary>For the proof run, since it can't move the real mouse: where the pretend mouse is, and whether its left button's down.</summary>
    /// <remarks>It goes through the same press and release checks as the real one, so hovering and clicking get tested the way they'd happen.</remarks>
    /// <param name="position">Where, in screen pixels.</param>
    /// <param name="leftDown">Whether the left button is held.</param>
    internal void ProofMouse(Point position, bool leftDown) =>
        _proofMouse = new MouseState(position.X, position.Y, 0, leftDown ? ButtonState.Pressed : ButtonState.Released,
            ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);

    /// <inheritdoc/>
    public bool IsKeyPressed(Keys key) => CurrentKeyboardState.IsKeyDown(key);

    /// <inheritdoc/>
    public bool IsNewKeyPress(Keys key) =>
        CurrentKeyboardState.IsKeyDown(key) && _priorKeyboardState.IsKeyUp(key);

    /// <inheritdoc/>
    public bool IsButtonPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
    {
        if (controllingPlayer.HasValue)
        {
            playerIndex = controllingPlayer.Value;
            return CurrentGamePadStates[(int)playerIndex].IsButtonDown(button);
        }

        return IsButtonPressed(button, PlayerIndex.One, out playerIndex) ||
               IsButtonPressed(button, PlayerIndex.Two, out playerIndex) ||
               IsButtonPressed(button, PlayerIndex.Three, out playerIndex) ||
               IsButtonPressed(button, PlayerIndex.Four, out playerIndex);
    }

    /// <inheritdoc/>
    public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
    {
        if (controllingPlayer.HasValue)
        {
            playerIndex = controllingPlayer.Value;
            int i = (int)playerIndex;
            return CurrentGamePadStates[i].IsButtonDown(button) && _priorGamePadStates[i].IsButtonUp(button);
        }

        return IsNewButtonPress(button, PlayerIndex.One, out playerIndex) ||
               IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) ||
               IsNewButtonPress(button, PlayerIndex.Three, out playerIndex) ||
               IsNewButtonPress(button, PlayerIndex.Four, out playerIndex);
    }

    /// <summary>Whether the mouse moved since last frame. The menus only follow the mouse when it does.</summary>
    public bool MouseMoved =>
        CurrentMouseState.X != _priorMouseState.X || CurrentMouseState.Y != _priorMouseState.Y;

    /// <summary>Where the mouse is, in screen pixels.</summary>
    public Point MousePosition => CurrentMouseState.Position;

    /// <summary>Whether the left button went down this frame.</summary>
    public bool IsNewLeftClick =>
        CurrentMouseState.LeftButton == ButtonState.Pressed && _priorMouseState.LeftButton == ButtonState.Released;

    /// <summary>Whether the left button came back up this frame.</summary>
    public bool IsLeftReleased =>
        CurrentMouseState.LeftButton == ButtonState.Released && _priorMouseState.LeftButton == ButtonState.Pressed;

    /// <summary>Which way the wheel turned this frame: 1 away from you, -1 toward you, 0 if it didn't.</summary>
    /// <remarks>Just the sign, not notches. A touchpad sends little bits at a time and would never add up to a whole notch in one frame.</remarks>
    public int WheelTurn => System.Math.Sign(CurrentMouseState.ScrollWheelValue - _priorMouseState.ScrollWheelValue);
}
