using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Components;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// Escape while playing: RESUME, OPTIONS, or QUIT TO TITLE. It's a popup so the paused screen stays
/// drawn underneath.
/// </summary>
/// <remarks>
/// The screen underneath stops itself while this has focus (the IsActive check in its Update),
/// the same as The Black Box. SAVE and LOAD go on it once there's a game to save.
/// </remarks>
public class PauseMenuScreen : MenuScreen
{
    private const string Heading = "PAUSED";
    private const string Note = "THE ROAD WILL WAIT.";
    private const string ResumeLabel = "RESUME";
    private const string OptionsLabel = "OPTIONS";
    private const string TitleLabel = "QUIT TO TITLE";

    private const int PanelWidth = 700;
    private const float EntryGap = 16f;
    private const float HeadingGap = 26f;

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.2);

    private readonly FormPanel _panel = new();

    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Rectangle _panelBounds;
    private float _noteY;

    /// <summary>Builds the menu.</summary>
    public PauseMenuScreen()
    {
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        // Torchlight keeps playing, ember is the way out.
        var resume = new MenuEntry(ResumeLabel, Palette.Torch);
        var options = new MenuEntry(OptionsLabel, Palette.Bone);
        var title = new MenuEntry(TitleLabel, Palette.Ember);

        resume.Selected += ExitScreen;
        options.Selected += () => ScreenManager.AddScreen(new OptionsScreen());
        title.Selected += QuitToTitle;

        MenuEntries.Add(resume);
        MenuEntries.Add(options);
        MenuEntries.Add(title);
    }

    /// <summary>Loads the form, lays it out and plays the pause sound.</summary>
    public override void Activate()
    {
        base.Activate();

        _panel.LoadContent(Content, ScreenManager.GraphicsDevice);
        _font = Content.Load<SpriteFont>("spectral-ui");
        _detailFont = Content.Load<SpriteFont>("spectral-detail");

        LayOut();
        Audio.Play(Sfx.Pause);
    }

    /// <summary>Pressing Escape again is the same as RESUME.</summary>
    protected override void OnCancel()
    {
        Audio.Play(Sfx.MenuBack);
        ExitScreen();
    }

    /// <summary>Draws the veil, the panel and its lines, then the entries.</summary>
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
        Text.Draw(spriteBatch, _detailFont, Note, _panelBounds.Center.X, _noteY,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>For the proof run, since it can't press keys: QUIT TO TITLE.</summary>
    internal void ProofQuitToTitle() => QuitToTitle();

    /// <summary>Goes back to the title, through the loading screen so everything under this fades out first.</summary>
    private void QuitToTitle() =>
        LoadingScreen.Load(ScreenManager, null, new TitleBackgroundScreen(), new TitleScreen());

    /// <summary>Sizes the panel to fit everything and centres it.</summary>
    private void LayOut()
    {
        float entries = -EntryGap;
        foreach (MenuEntry entry in MenuEntries) entries += entry.Plate.Size.Y + EntryGap;

        int height = (int)MathF.Round(FormPanel.Padding + _font.LineSpacing + HeadingGap / 2f
            + _detailFont.LineSpacing + HeadingGap + entries + FormPanel.Padding);

        _panelBounds = new Rectangle(
            (LegendQuestGame.ScreenWidth - PanelWidth) / 2,
            (LegendQuestGame.ScreenHeight - height) / 2,
            PanelWidth,
            height);

        _noteY = _panelBounds.Top + FormPanel.Padding + _font.LineSpacing + HeadingGap / 2f;

        float top = _noteY + _detailFont.LineSpacing + HeadingGap;
        LayOutColumn(new Vector2(_panelBounds.Center.X, top + entries / 2f), EntryGap);
    }
}
