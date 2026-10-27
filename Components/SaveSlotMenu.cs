using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Save;
using LegendQuest.StateManagement;

namespace LegendQuest.Components;

/// <summary>
/// The save slots form: every slot, what is in it, and an ERASE button beside each one that has
/// something to erase.
/// </summary>
/// <remarks>
/// Adapted from The Black Box's. All the file stuff is in SaveSystem, this just draws what it gets
/// back. What can be picked depends on why it was opened: a new game only goes in a numbered slot,
/// and a full one asks before it gets started over. SaveSlotScreen puts it on the screen stack and
/// handles the keyboard for it.
/// </remarks>
public class SaveSlotMenu
{
    private const string NewGameHeading = "START A NEW GAME";
    private const string LoadHeading = "LOAD A GAME";
    private const string BackLabel = "BACK";
    private const string EraseLabel = "ERASE";

    /// <summary>What the erase button says once it is waiting for the second click.</summary>
    private const string ConfirmLabel = "SURE?";

    /// <summary>What a full slot says while it waits for the second click that starts a new game over it.</summary>
    private const string ReplaceNote = "PICK IT AGAIN TO START OVER IT";

    private const int PanelWidth = 940;
    private const int SlotWidth = 620;
    private const int EraseWidth = 170;

    /// <summary>How tall a slot's plate is.</summary>
    /// <remarks>Spectral's lines are a lot taller than its letters, so at 84 the name sat on the top edge and the summary on the bottom one.</remarks>
    private const int SlotHeight = 100;

    /// <summary>Gap between a slot and its erase button.</summary>
    private const int EraseGap = 16;

    /// <summary>Gap between one slot row and the next. Tighter than The Black Box's, since there are five rows now.</summary>
    private const int RowGap = 10;

    /// <summary>Gap under the heading, and over the back button.</summary>
    private const int HeadingGap = 22;

    /// <summary>Gap between the back button and the line that reports what went wrong.</summary>
    private const int StatusGap = 12;

    /// <summary>The keyboard row for the back button, under the slots.</summary>
    private const int BackRow = SaveSystem.SlotCount;

    private readonly FormPanel _panel = new();
    private readonly SlotPurpose _purpose;

    private SpriteFont _font;
    private SpriteFont _detailFont;

    private SaveSlot[] _slots;
    private ButtonSprite[] _slotButtons;
    private ButtonSprite[] _eraseButtons;
    private ButtonSprite _backButton;

    private Rectangle _panelBounds;

    /// <summary>Top of the line errors are reported on, worked out in LayOut.</summary>
    private float _statusY;

    /// <summary>Which slot's erase button is waiting for its second click, or -1 for none.</summary>
    private int _confirmingErase = -1;

    /// <summary>Which full slot is waiting for its second click to have a new game started over it, or -1 for none.</summary>
    private int _confirmingReplace = -1;

    /// <summary>What went wrong with the last thing the player asked for, or null.</summary>
    private string _status;

    /// <summary>Where the keyboard is: the row (a slot or the back button), and the column (the slot or its erase button).</summary>
    private int _focusRow;
    private int _focusColumn;

    /// <summary>Whether the form is on screen and taking input.</summary>
    public bool IsOpen { get; private set; }

    /// <summary>How far the form has faded in, 0 to 1.</summary>
    public float Opacity { get; set; } = 1f;

    /// <summary>Raised when the player commits to a slot, with the game in it (a brand new one, already written, for a new game).</summary>
    public event Action<int, SaveData> SlotChosen;

    /// <summary>Raised when the player backs out without choosing anything.</summary>
    public event Action Closed;

    /// <summary>Creates the form for one purpose.</summary>
    /// <param name="purpose">Starting a new game or loading one.</param>
    public SaveSlotMenu(SlotPurpose purpose)
    {
        _purpose = purpose;
    }

    /// <summary>Loads the panel, the fonts, and every control on the form.</summary>
    /// <param name="content">The ContentManager to load with.</param>
    /// <param name="graphicsDevice">The device, for the one pixel the veil is made of.</param>
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _panel.LoadContent(content, graphicsDevice);
        _font = content.Load<SpriteFont>("spectral-ui");
        _detailFont = content.Load<SpriteFont>("spectral-detail");

        _slotButtons = new ButtonSprite[SaveSystem.SlotCount];
        _eraseButtons = new ButtonSprite[SaveSystem.SlotCount];

        for (int i = 0; i < SaveSystem.SlotCount; i++)
        {
            int slot = i;

            _slotButtons[i] = new ButtonSprite(string.Empty, Vector2.Zero, Palette.Bone, new Point(SlotWidth, SlotHeight));
            _slotButtons[i].Clicked += () => Choose(slot);

            _eraseButtons[i] = new ButtonSprite(EraseLabel, Vector2.Zero, Palette.Ember, new Point(EraseWidth, SlotHeight));
            _eraseButtons[i].Clicked += () => Erase(slot);

            _slotButtons[i].LoadContent(content);
            _eraseButtons[i].LoadContent(content);
        }

        _backButton = new ButtonSprite(BackLabel, Vector2.Zero, Palette.Bone);
        _backButton.Clicked += Close;
        _backButton.LoadContent(content);

        LayOut();
    }

    /// <summary>Opens the form, reading what is on disk right now.</summary>
    /// <remarks>Read here and not once at startup, so it shows the slots as they are after an erase.</remarks>
    public void Open()
    {
        Refresh();

        _confirmingErase = -1;
        _confirmingReplace = -1;
        _status = null;
        IsOpen = true;

        // Start on the first row there's anything to do with.
        _focusRow = 0;
        _focusColumn = 0;
        SettleFocus();

        // Reset every control so the click that opened this doesn't land on a slot.
        foreach (ButtonSprite button in _slotButtons) button.Reset();
        foreach (ButtonSprite button in _eraseButtons) button.Reset();
        _backButton.Reset();
    }

    /// <summary>Closes the form and tells whoever opened it.</summary>
    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;
        Closed?.Invoke();
    }

    /// <summary>Updates every control on the form.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame, for the mouse.</param>
    public void Update(GameTime gameTime, InputState input)
    {
        if (!IsOpen) return;

        for (int i = 0; i < _slotButtons.Length; i++)
        {
            _slotButtons[i].Focused = _focusRow == i && _focusColumn == 0;
            _eraseButtons[i].Focused = _focusRow == i && _focusColumn == 1;

            _slotButtons[i].Update(gameTime, input);
            if (_eraseButtons[i].Enabled) _eraseButtons[i].Update(gameTime, input);
        }

        _backButton.Focused = _focusRow == BackRow;
        _backButton.Update(gameTime, input);
    }

    /// <summary>Moves the keyboard selection. Up and down go through the slots and BACK, left and right switch between a slot and its erase button.</summary>
    /// <remarks>Rows with nothing to press on them get skipped, so the keyboard never lands on a dead plate.</remarks>
    /// <param name="rows">-1, 0 or 1.</param>
    /// <param name="columns">-1, 0 or 1.</param>
    /// <returns>True if the place moved.</returns>
    public bool Navigate(int rows, int columns)
    {
        int row = _focusRow;
        for (int steps = 0; rows != 0 && steps <= BackRow; steps++)
        {
            int next = Math.Clamp(row + rows, 0, BackRow);
            if (next == row) break;
            row = next;
            if (RowIsLive(row)) break;
        }

        // Nothing live that way, so stay where it was.
        if (!RowIsLive(row)) row = _focusRow;

        int column = row == BackRow ? 0 : Math.Clamp(_focusColumn + columns, 0, 1);
        if (row < BackRow && column == 1 && !_eraseButtons[row].Enabled) column = 0;
        if (row < BackRow && column == 0 && !_slotButtons[row].Enabled) column = _eraseButtons[row].Enabled ? 1 : 0;

        return FocusOn(row, column);
    }

    /// <summary>Moves the keyboard selection to whatever the mouse is over, so they never light up different plates.</summary>
    /// <param name="mouse">Where the mouse is.</param>
    /// <returns>True if the place moved.</returns>
    public bool FollowMouse(Point mouse)
    {
        for (int i = 0; i < _slotButtons.Length; i++)
        {
            if (_slotButtons[i].Enabled && _slotButtons[i].Bounds.Contains(mouse)) return FocusOn(i, 0);
            if (_eraseButtons[i].Enabled && _eraseButtons[i].Bounds.Contains(mouse)) return FocusOn(i, 1);
        }

        return _backButton.Bounds.Contains(mouse) && FocusOn(BackRow, 0);
    }

    /// <summary>Presses whatever the keyboard is on, same as clicking it.</summary>
    public void Press()
    {
        if (_focusRow == BackRow) _backButton.Activate();
        else if (_focusColumn == 0) _slotButtons[_focusRow].Activate();
        else _eraseButtons[_focusRow].Activate();
    }

    /// <summary>For the proof run, since it can't press keys: puts the keyboard on a slot's ERASE and presses it.</summary>
    /// <param name="slot">Which slot's erase button.</param>
    internal void ProofErase(int slot)
    {
        FocusOn(slot, 1);
        Press();
    }

    /// <summary>Draws the veil, the panel and everything on it. Call it in its own batch.</summary>
    /// <remarks>It still draws when closed so it can fade out after you pick a slot. The screen decides when it's open.</remarks>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="spriteBatch">The SpriteBatch to render with. Point-sampled.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _panel.DrawVeil(spriteBatch, Opacity);
        _panel.Draw(spriteBatch, _panelBounds, Opacity);

        string heading = _purpose == SlotPurpose.NewGame ? NewGameHeading : LoadHeading;
        Text.Draw(spriteBatch, _font, heading, _panelBounds.Center.X, _panelBounds.Top + FormPanel.Padding,
            Palette.Heading * Opacity, Palette.FormShadow * Opacity, centred: true);

        for (int i = 0; i < _slotButtons.Length; i++)
        {
            _slotButtons[i].Opacity = Opacity;
            _eraseButtons[i].Opacity = Opacity;

            _slotButtons[i].Draw(gameTime, spriteBatch);
            if (_eraseButtons[i].Enabled) _eraseButtons[i].Draw(gameTime, spriteBatch);
        }

        _backButton.Opacity = Opacity;
        _backButton.Draw(gameTime, spriteBatch);

        // The status row is always kept in the layout, so an error doesn't shove the form around.
        if (_status is not null)
        {
            Text.Draw(spriteBatch, _detailFont, _status, _panelBounds.Center.X, _statusY,
                Palette.Ember * Opacity, Palette.FormShadow * Opacity, centred: true);
        }
    }

    /// <summary>Whether a slot can be picked for what the form was opened for.</summary>
    /// <param name="slot">The slot as it was read.</param>
    private bool CanPick(SaveSlot slot) => _purpose switch
    {
        SlotPurpose.NewGame => slot.Index < SaveSystem.NumberedSlots && slot.State != SaveSlotState.Unreadable,
        _ => slot.HasGame,
    };

    /// <summary>Whether a row has anything on it the keyboard can press.</summary>
    /// <param name="row">A slot, or the back button's row.</param>
    private bool RowIsLive(int row) => row == BackRow || _slotButtons[row].Enabled || _eraseButtons[row].Enabled;

    /// <summary>Re-reads the slots and puts what they say onto the plates.</summary>
    private void Refresh()
    {
        _slots = SaveSystem.ReadAll();

        for (int i = 0; i < _slots.Length; i++)
        {
            SaveSlot slot = _slots[i];

            _slotButtons[i].Label = slot.Name;
            _slotButtons[i].Sublabel = slot.Summary;

            // Torchlight for a game to pick up, bone for an empty slot, and a dead plate for one that can't be picked.
            _slotButtons[i].Accent = slot.HasGame ? Palette.Torch : Palette.Bone;
            _slotButtons[i].Enabled = CanPick(slot);

            // An unreadable slot keeps its erase button. That's the only way to get the slot back.
            _eraseButtons[i].Enabled = slot.State != SaveSlotState.Empty;
            _eraseButtons[i].Label = EraseLabel;
            _eraseButtons[i].Reset();
        }
    }

    /// <summary>Commits to a slot. A full one asks first when a new game is going in it.</summary>
    /// <param name="slot">Which slot was clicked.</param>
    private void Choose(int slot)
    {
        bool confirmed = _confirmingReplace == slot;
        CancelConfirm();

        SaveSlot picked = _slots[slot];

        if (_purpose == SlotPurpose.Load)
        {
            IsOpen = false;
            SlotChosen?.Invoke(slot, picked.Data);
            return;
        }

        if (picked.HasGame && !confirmed)
        {
            _confirmingReplace = slot;
            _slotButtons[slot].Sublabel = ReplaceNote;
            _status = null;
            return;
        }

        // A new game is written straight away, so if the slot can't be written the form can say so.
        SaveData fresh = SaveData.NewGame();
        if (!SaveSystem.Write(slot, fresh))
        {
            _status = "COULD NOT WRITE " + picked.Name + " -- " + SaveSystem.LastError;
            return;
        }

        IsOpen = false;
        SlotChosen?.Invoke(slot, fresh);
    }

    /// <summary>Erases a slot, on the second click.</summary>
    /// <remarks>The button turns into the "SURE?" question itself. Cheaper than a whole confirm form, and just as hard to do by accident.</remarks>
    /// <param name="slot">Which slot's erase button was clicked.</param>
    private void Erase(int slot)
    {
        bool confirmed = _confirmingErase == slot;
        CancelConfirm();

        if (!confirmed)
        {
            _confirmingErase = slot;
            _eraseButtons[slot].Label = ConfirmLabel;
            _status = null;
            return;
        }

        if (!SaveSystem.Delete(slot))
        {
            _status = "COULD NOT ERASE " + _slots[slot].Name + " -- " + SaveSystem.LastError;
            return;
        }

        _status = null;
        Refresh();

        // The erase button is gone now that the slot is empty, so move the keyboard off it.
        SettleFocus();
    }

    /// <summary>Puts the keyboard on something it can press: the same row if there's anything left on it, otherwise the next live row down.</summary>
    /// <remarks>BACK is always live, so going down always finds something.</remarks>
    private void SettleFocus()
    {
        if (RowIsLive(_focusRow)) Navigate(0, 0);
        else Navigate(1, 0);
    }

    /// <summary>Takes back any half-confirmed erase or start-over, and puts their own words back.</summary>
    private void CancelConfirm()
    {
        if (_confirmingErase >= 0) _eraseButtons[_confirmingErase].Label = EraseLabel;
        if (_confirmingReplace >= 0) _slotButtons[_confirmingReplace].Sublabel = _slots[_confirmingReplace].Summary;

        _confirmingErase = -1;
        _confirmingReplace = -1;
    }

    /// <summary>Moves the keyboard selection to one plate.</summary>
    /// <param name="row">A slot, or the back button's row.</param>
    /// <param name="column">0 for the slot, 1 for its erase button.</param>
    /// <returns>True if the place moved.</returns>
    private bool FocusOn(int row, int column)
    {
        if (_focusRow == row && _focusColumn == column) return false;

        _focusRow = row;
        _focusColumn = column;
        return true;
    }

    /// <summary>Sizes the panel around its contents and places every control on it.</summary>
    /// <remarks>Has to run after LoadContent, because the back button sizes itself to its label.</remarks>
    private void LayOut()
    {
        int rows = _slotButtons.Length;
        int rowsHeight = rows * SlotHeight + (rows - 1) * RowGap;

        int headingHeight = _font.LineSpacing;
        int statusHeight = _detailFont.LineSpacing;

        int panelHeight = FormPanel.Padding + headingHeight + HeadingGap
            + rowsHeight + HeadingGap + _backButton.Size.Y + StatusGap + statusHeight + FormPanel.Padding;

        _panelBounds = new Rectangle(
            (LegendQuestGame.ScreenWidth - PanelWidth) / 2,
            (LegendQuestGame.ScreenHeight - panelHeight) / 2,
            PanelWidth,
            panelHeight);

        // Each row is a slot plus its erase button, centred as a pair so the slots line up either way.
        int rowWidth = SlotWidth + EraseGap + EraseWidth;
        int left = _panelBounds.Center.X - rowWidth / 2;
        int top = _panelBounds.Top + FormPanel.Padding + headingHeight + HeadingGap;

        for (int i = 0; i < rows; i++)
        {
            float centreY = top + i * (SlotHeight + RowGap) + SlotHeight / 2f;

            _slotButtons[i].Center = new Vector2(left + SlotWidth / 2f, centreY);
            _eraseButtons[i].Center = new Vector2(left + SlotWidth + EraseGap + EraseWidth / 2f, centreY);
        }

        float backTop = top + rowsHeight + HeadingGap;
        _backButton.Center = new Vector2(_panelBounds.Center.X, backTop + _backButton.Size.Y / 2f);

        _statusY = backTop + _backButton.Size.Y + StatusGap;
    }
}
