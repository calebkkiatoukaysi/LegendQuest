using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Components;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The party's fallen in a fight they weren't meant to lose: LOAD LAST SAVE or QUIT TO TITLE, over
/// the map where it happened.
/// </summary>
/// <remarks>
/// A popup, so the fight stays drawn behind its veil. The pause menu's form, with the game over
/// sting. LOAD LAST SAVE loads whichever slot was written last, which is the autosave from just
/// before the fight unless you've saved since; it's dead if no slot has a game in it. Escape does
/// nothing: there's no going back to a lost fight.
/// </remarks>
public class GameOverScreen : MenuScreen
{
    private const string Heading = "THE LIGHT GOES OUT";
    private const string Note = "THE PARTY HAS FALLEN. THE STORY ISN'T OVER YET.";
    private const string LoadLabel = "LOAD LAST SAVE";
    private const string TitleLabel = "QUIT TO TITLE";

    private const int PanelWidth = 760;
    private const float EntryGap = 16f;
    private const float HeadingGap = 26f;

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.8);

    private readonly FormPanel _panel = new();
    private readonly MenuEntry _load;

    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Rectangle _panelBounds;
    private float _noteY;

    /// <summary>Builds the menu.</summary>
    public GameOverScreen()
    {
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _load = new MenuEntry(LoadLabel, Palette.Torch);
        var title = new MenuEntry(TitleLabel, Palette.Ember);
        _load.Selected += LoadLastSave;
        title.Selected += QuitToTitle;

        MenuEntries.Add(_load);
        MenuEntries.Add(title);
    }

    /// <summary>Loads the form, lays it out, deadens LOAD LAST SAVE if there's nothing to load, and plays the sting.</summary>
    public override void Activate()
    {
        base.Activate();

        _panel.LoadContent(Content, ScreenManager.GraphicsDevice);
        _font = Content.Load<SpriteFont>("spectral-ui");
        _detailFont = Content.Load<SpriteFont>("spectral-detail");
        _load.Plate.Enabled = SaveSystem.Latest() is not null;
        if (!_load.Plate.Enabled) SelectedEntry = 1;

        LayOut();
        Audio.Play(Sfx.GameOver);
    }

    /// <summary>Escape does nothing here: there's nowhere to go back to.</summary>
    protected override void OnCancel() { }

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
            Palette.Ember * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, Note, _panelBounds.Center.X, _noteY,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>For the proof run, since it can't press keys: LOAD LAST SAVE.</summary>
    internal void ProofLoadLastSave() => LoadLastSave();

    /// <summary>Loads whichever slot was written last, onto its map. If it's gone since the screen came up, LOAD LAST SAVE just goes dead.</summary>
    private void LoadLastSave()
    {
        SaveSlot latest = SaveSystem.Latest();
        if (latest is null)
        {
            _load.Plate.Enabled = false;
            return;
        }

        SaveData game = latest.Data;
        LoadingScreen.Load(ScreenManager, MapCatalog.Get(game.MapId).Name.ToUpperInvariant(), new ExploreScreen(game));
    }

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
