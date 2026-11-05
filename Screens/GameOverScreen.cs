using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Components;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The party's fallen in a fight they weren't meant to lose: what the difficulty offers (RETRY THE
/// FIGHT, RETRY ON STORY, WIN THIS FIGHT) and QUIT TO TITLE, over the room where it happened.
/// </summary>
/// <remarks>
/// A popup, so the fight stays drawn behind its veil. The pause menu's form, with the game over
/// sting. RETRY THE FIGHT loads whichever slot was written last, which is the autosave from just
/// before the fight unless you've saved since, and a save made as a fight starts is that fight again,
/// fresh, with the same allies; it's dead if no slot has a game in it. RETRY ON STORY does the same
/// with the fight marked to be fought on Story (RetryState). WIN THIS FIGHT, which Story offers once
/// a fight's been lost twice, hands the fight to the party and goes back to it for the win. Without
/// any options (outside a fight) it's LOAD LAST SAVE. Escape does nothing: there's no going back to a
/// lost fight.
/// </remarks>
public class GameOverScreen : MenuScreen
{
    private const string Heading = "THE LIGHT GOES OUT";
    private const string Note = "THE PARTY HAS FALLEN. THE STORY ISN'T OVER YET.";
    private const string LoadLabel = "LOAD LAST SAVE";
    private const string RetryLabel = "RETRY THE FIGHT";
    private const string StoryLabel = "RETRY ON STORY";
    private const string WinLabel = "WIN THIS FIGHT";
    private const string TitleLabel = "QUIT TO TITLE";

    private const int PanelWidth = 760;
    private const float EntryGap = 16f;
    private const float HeadingGap = 26f;

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.8);

    private readonly FormPanel _panel = new();
    private readonly MenuEntry _load;
    private readonly MenuEntry _story;

    /// <summary>The fight that was lost, by its encounter id, or null outside one.</summary>
    private readonly string _fight;

    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Rectangle _panelBounds;
    private float _noteY;

    /// <summary>Builds the menu.</summary>
    /// <param name="fight">The fight that was lost, by its encounter id, or null.</param>
    /// <param name="options">What the difficulty offers (RetryState.Options); None for just LOAD LAST SAVE.</param>
    /// <param name="win">What WIN THIS FIGHT does once the screen's gone (the fight conceded to the party), or null.</param>
    public GameOverScreen(string fight = null, LossOptions options = LossOptions.None, Action win = null)
    {
        _fight = fight;
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _load = new MenuEntry(options.HasFlag(LossOptions.Retry) ? RetryLabel : LoadLabel, Palette.Torch);
        _load.Selected += LoadLastSave;
        MenuEntries.Add(_load);

        if (options.HasFlag(LossOptions.RetryOnStory))
        {
            _story = new MenuEntry(StoryLabel, Palette.Torch);
            _story.Selected += RetryOnStory;
            MenuEntries.Add(_story);
        }

        if (options.HasFlag(LossOptions.WinAfterTwoLosses) && win is not null)
        {
            var winning = new MenuEntry(WinLabel, Palette.Gold);
            winning.Selected += () =>
            {
                ExitScreen();
                win();
            };
            MenuEntries.Add(winning);
        }

        var title = new MenuEntry(TitleLabel, Palette.Ember);
        title.Selected += QuitToTitle;
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
        if (_story is not null) _story.Plate.Enabled = _load.Plate.Enabled;
        if (!_load.Plate.Enabled) SelectedEntry = MenuEntries.Count - 1;

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

    /// <summary>For the proof run, since it can't press keys: RETRY THE FIGHT (or LOAD LAST SAVE).</summary>
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
        LoadingScreen.Load(ScreenManager, ZoneCatalog.Get(game.ZoneId).Name.ToUpperInvariant(), new ExploreScreen(game));
    }

    /// <summary>Marks the fight to be fought on Story, then loads the save from before it.</summary>
    private void RetryOnStory()
    {
        if (SaveSystem.Latest() is null) return;
        RetryState.PlayOnStory(_fight);
        LoadLastSave();
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
