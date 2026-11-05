using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Components;
using LegendQuest.Save;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// The three volumes, the text speed, the tutorial's tips and full screen, on a form over whatever opened it. Left and right change one.
/// </summary>
/// <remarks>
/// You hear a volume change right away since AudioManager reads the settings every frame. The file
/// only gets written when the form closes, not on every press. Same as The Black Box, plus the
/// text speed for the dialogue, and the difficulty (Story, Balanced, Tactician), which can't be
/// changed in the middle of a fight: opened from the pause menu in one, its row says so.
/// </remarks>
public class OptionsScreen : MenuScreen
{
    private const string Heading = "OPTIONS";
    private const string Note = "LEFT AND RIGHT TO CHANGE ONE";
    private const string MasterLabel = "MASTER";
    private const string MusicLabel = "MUSIC";
    private const string SoundLabel = "SOUND";
    private const string TextSpeedLabel = "TEXT SPEED";
    private const string DifficultyLabel = "DIFFICULTY";
    private const string LockedNote = "THE DIFFICULTY CAN'T CHANGE MID-FIGHT";
    private const string TipsLabel = "TUTORIAL TIPS";
    private const string DisplayLabel = "DISPLAY";
    private const string BackLabel = "BACK";

    /// <summary>How much one press moves a volume.</summary>
    private const float VolumeStep = 0.1f;

    private const int PanelWidth = 760;
    private static readonly Point EntrySize = new(560, 64);
    private const float EntryGap = 10f;
    private const float HeadingGap = 30f;

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.25);

    private readonly FormPanel _panel = new();
    private readonly MenuEntry _master;
    private readonly MenuEntry _music;
    private readonly MenuEntry _sound;
    private readonly MenuEntry _textSpeed;
    private readonly MenuEntry _difficulty;
    private readonly MenuEntry _tips;
    private readonly MenuEntry _display;

    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Rectangle _panelBounds;
    private float _noteY;

    /// <summary>Whether the difficulty's held where it is, because a fight's on.</summary>
    private readonly bool _lockDifficulty;

    /// <summary>Builds the seven adjustable entries and BACK.</summary>
    /// <param name="lockDifficulty">Whether a fight's on, so the difficulty can't be changed.</param>
    public OptionsScreen(bool lockDifficulty = false)
    {
        _lockDifficulty = lockDifficulty;
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _master = new MenuEntry(string.Empty, Palette.Torch, EntrySize);
        _music = new MenuEntry(string.Empty, Palette.Torch, EntrySize);
        _sound = new MenuEntry(string.Empty, Palette.Torch, EntrySize);
        _textSpeed = new MenuEntry(string.Empty, Palette.Torch, EntrySize);
        _difficulty = new MenuEntry(string.Empty, Palette.Torch, EntrySize);
        _tips = new MenuEntry(string.Empty, Palette.Torch, EntrySize);
        _display = new MenuEntry(string.Empty, Palette.Torch, EntrySize);
        var back = new MenuEntry(BackLabel, Palette.Bone);

        _master.Adjusted += step => ChangeVolume(v => Audio.Settings.MasterVolume = v, Audio.Settings.MasterVolume, step);
        _music.Adjusted += step => ChangeVolume(v => Audio.Settings.MusicVolume = v, Audio.Settings.MusicVolume, step);
        _sound.Adjusted += step => ChangeVolume(v => Audio.Settings.SfxVolume = v, Audio.Settings.SfxVolume, step);
        _textSpeed.Adjusted += ChangeTextSpeed;
        _difficulty.Adjusted += ChangeDifficulty;
        _tips.Adjusted += _ => ToggleTips();
        _display.Adjusted += _ => ToggleFullScreen();
        back.Selected += Close;

        MenuEntries.Add(_master);
        MenuEntries.Add(_music);
        MenuEntries.Add(_sound);
        MenuEntries.Add(_textSpeed);
        MenuEntries.Add(_difficulty);
        MenuEntries.Add(_tips);
        MenuEntries.Add(_display);
        MenuEntries.Add(back);
    }

    /// <summary>Loads the form and lays it out in the middle of the screen.</summary>
    public override void Activate()
    {
        base.Activate();

        _panel.LoadContent(Content, ScreenManager.GraphicsDevice);
        _font = Content.Load<SpriteFont>("spectral-ui");
        _detailFont = Content.Load<SpriteFont>("spectral-detail");
        _difficulty.Plate.Enabled = !_lockDifficulty;

        Relabel();
        LayOut();
    }

    /// <summary>Escape is the same as BACK.</summary>
    protected override void OnCancel()
    {
        Audio.Play(Sfx.MenuBack);
        Close();
    }

    /// <summary>Draws the veil, the panel, the heading and the note, then the entries.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        _panel.DrawVeil(spriteBatch, alpha);
        _panel.Draw(spriteBatch, _panelBounds, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        Text.Draw(spriteBatch, _font, Heading, _panelBounds.Center.X, _panelBounds.Top + FormPanel.Padding,
            Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, _lockDifficulty ? LockedNote : Note, _panelBounds.Center.X, _noteY,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>Moves one volume by a step and puts the new number on its plate.</summary>
    /// <param name="set">Writes the new value back to the settings.</param>
    /// <param name="value">The value now.</param>
    /// <param name="step">-1 or 1.</param>
    private void ChangeVolume(Action<float> set, float value, int step)
    {
        // Rounded to the step so it always lands on a clean number (no 69.999%).
        float next = MathF.Round((value + step * VolumeStep) / VolumeStep) * VolumeStep;
        set(Math.Clamp(next, 0f, 1f));
        Relabel();
    }

    /// <summary>Steps the text speed along, wrapping round at either end.</summary>
    /// <param name="step">-1 or 1.</param>
    private void ChangeTextSpeed(int step)
    {
        TextSpeed[] speeds = Enum.GetValues<TextSpeed>();
        int next = (Array.IndexOf(speeds, Audio.Settings.Typing) + step + speeds.Length) % speeds.Length;
        Audio.Settings.TextSpeed = speeds[next].ToString();
        Relabel();
    }

    /// <summary>Steps the difficulty along, wrapping round at either end; nothing while a fight's on.</summary>
    /// <param name="step">-1 or 1.</param>
    private void ChangeDifficulty(int step)
    {
        if (_lockDifficulty) return;

        Difficulty[] modes = Enum.GetValues<Difficulty>();
        int next = (Array.IndexOf(modes, Audio.Settings.Mode) + step + modes.Length) % modes.Length;
        Audio.Settings.Mode = modes[next];
        Relabel();
    }

    /// <summary>Switches the tutorial's tips on or off. A tip that's up goes as soon as you're back on the map.</summary>
    private void ToggleTips()
    {
        Audio.Settings.TutorialTips = !Audio.Settings.TutorialTips;
        Relabel();
    }

    /// <summary>Switches between full screen and a window, straight away.</summary>
    private void ToggleFullScreen()
    {
        ScreenCanvas canvas = ScreenManager.Game.Services.GetService<ScreenCanvas>();
        canvas.SetFullScreen(!canvas.FullScreen);
        Audio.Settings.FullScreen = canvas.FullScreen;
        Relabel();
    }

    /// <summary>Writes each value onto its plate.</summary>
    private void Relabel()
    {
        Settings settings = Audio.Settings;
        _master.Text = Row(MasterLabel, Percent(settings.MasterVolume));
        _music.Text = Row(MusicLabel, Percent(settings.MusicVolume));
        _sound.Text = Row(SoundLabel, Percent(settings.SfxVolume));
        _textSpeed.Text = Row(TextSpeedLabel, settings.Typing.ToString().ToUpperInvariant());
        string mode = settings.Mode.ToString().ToUpperInvariant();
        _difficulty.Text = _lockDifficulty ? DifficultyLabel + "   " + mode : Row(DifficultyLabel, mode);
        _tips.Text = Row(TipsLabel, settings.TutorialTips ? "ON" : "OFF");
        _display.Text = Row(DisplayLabel, settings.FullScreen ? "FULL SCREEN" : "WINDOW");
    }

    /// <summary>A plate's label: the name, then the value between the arrows.</summary>
    private static string Row(string name, string value) => name + "   <  " + value + "  >";

    /// <summary>A volume as a whole percent.</summary>
    private static string Percent(float value) =>
        ((int)MathF.Round(value * 100f)).ToString(CultureInfo.InvariantCulture) + "%";

    /// <summary>Saves the settings and closes the form.</summary>
    private void Close()
    {
        SettingsStore.Save(Audio.Settings);
        ExitScreen();
    }

    /// <summary>Sizes the panel to fit the heading, the entries and the note, and centres it.</summary>
    private void LayOut()
    {
        float entries = -EntryGap;
        foreach (MenuEntry entry in MenuEntries) entries += entry.Plate.Size.Y + EntryGap;

        int height = (int)MathF.Round(FormPanel.Padding + _font.LineSpacing + HeadingGap
            + entries + HeadingGap + _detailFont.LineSpacing + FormPanel.Padding);

        _panelBounds = new Rectangle(
            (LegendQuestGame.ScreenWidth - PanelWidth) / 2,
            (LegendQuestGame.ScreenHeight - height) / 2,
            PanelWidth,
            height);

        float top = _panelBounds.Top + FormPanel.Padding + _font.LineSpacing + HeadingGap;
        LayOutColumn(new Vector2(_panelBounds.Center.X, top + entries / 2f), EntryGap);

        _noteY = top + entries + HeadingGap;
    }
}
