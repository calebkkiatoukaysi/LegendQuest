using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The title, with NEW GAME, CONTINUE, LOAD, OPTIONS and EXIT along the bottom. This is the sample's MainMenuScreen.
/// </summary>
/// <remarks>
/// CONTINUE picks up whichever slot was written last, and is dead when there isn't one. Escape on
/// the title closes the game, same as The Black Box.
/// </remarks>
public class TitleScreen : MenuScreen
{
    private const string Title = "Legend Quest";
    private const string NewGameLabel = "NEW GAME";
    private const string ContinueLabel = "CONTINUE";
    private const string LoadLabel = "LOAD";
    private const string OptionsLabel = "OPTIONS";
    private const string ExitLabel = "EXIT";
    private const string Hint = "ARROWS  ·  ENTER  ·  ESC TO LEAVE";

    private const float TitleY = 48f;
    private const float HintY = 858f;

    /// <summary>Where the row of buttons sits (on the terrace, in front of the balustrade) and the gap between them.</summary>
    private const float ButtonRowY = 806f;
    private const float ButtonGap = 30f;

    /// <summary>How long the title takes to fade in and out.</summary>
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.5);

    // The title's the warm gold of the lamps on the terrace, with a tight shadow the blue of the night
    // over the castle, on a soft dark of its own so the spire and the clouds behind don't eat it. The
    // serif is light, and a wider shadow ghosts around the strokes.
    private static readonly Color TitleColor = new(255, 238, 196);
    private static readonly Color TitleShadow = new(26, 22, 64);
    private static readonly Color TitleBacking = new Color(14, 12, 40) * 0.55f;
    private static readonly Vector2 TitleSpread = new(80f, 4f);

    /// <summary>The hint along the bottom, and the soft dark it sits on so it reads on the pale terrace: how dark, and how far it reaches past the line.</summary>
    private static readonly Color HintColor = new(236, 224, 214);
    private static readonly Color HintBacking = Color.Black * 0.6f;
    private static readonly Vector2 HintSpread = new(30f, 8f);

    private readonly MenuEntry _continue;

    private SpriteFont _titleFont;
    private SpriteFont _detailFont;
    private Texture2D _glow;

    /// <summary>Builds the five entries.</summary>
    public TitleScreen()
    {
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        // Torchlight is the way in, ember is the way out, and bone is for the ones that don't commit you to anything.
        var newGame = new MenuEntry(NewGameLabel, Palette.Torch);
        _continue = new MenuEntry(ContinueLabel, Palette.Torch);
        var load = new MenuEntry(LoadLabel, Palette.Bone);
        var options = new MenuEntry(OptionsLabel, Palette.Bone);
        var exit = new MenuEntry(ExitLabel, Palette.Ember);

        newGame.Selected += () => OpenSlots(SlotPurpose.NewGame);
        _continue.Selected += Continue;
        load.Selected += () => OpenSlots(SlotPurpose.Load);
        options.Selected += () => ScreenManager.AddScreen(new OptionsScreen());
        exit.Selected += () => ScreenManager.Game.Exit();

        MenuEntries.Add(newGame);
        MenuEntries.Add(_continue);
        MenuEntries.Add(load);
        MenuEntries.Add(options);
        MenuEntries.Add(exit);
    }

    /// <summary>Loads the fonts and entries, centres the row, and lights CONTINUE if there's anything to continue.</summary>
    public override void Activate()
    {
        base.Activate();

        _titleFont = Content.Load<SpriteFont>("spectral-title");
        _detailFont = Content.Load<SpriteFont>("spectral-detail");
        _glow = Content.Load<Texture2D>("glow");

        LayOutRow(new Vector2(LegendQuestGame.ScreenWidth / 2f, ButtonRowY), ButtonGap);
        RefreshContinue();

        // If there's a game to carry on, that's the likeliest thing to want.
        if (_continue.Plate.Enabled) SelectedEntry = MenuEntries.IndexOf(_continue);
    }

    /// <summary>Escape on the title closes the game.</summary>
    protected override void OnCancel() => ScreenManager.Game.Exit();

    /// <summary>Draws the title over the sky and the hint on the terrace, then the row of buttons.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        Vector2 title = _titleFont.MeasureString(Title);
        var titleArea = new Rectangle((int)((LegendQuestGame.ScreenWidth - title.X) / 2f), (int)(TitleY + TitleSpread.Y),
            (int)title.X, (int)(title.Y - TitleSpread.Y * 2f));
        Text.DrawBacking(spriteBatch, _glow, titleArea, (int)TitleSpread.X, TitleBacking * alpha);
        Text.DrawCentered(spriteBatch, _titleFont, Title, TitleY, TitleColor * alpha, TitleShadow * alpha);
        Vector2 hint = _detailFont.MeasureString(Hint);
        var backing = new Rectangle((int)((LegendQuestGame.ScreenWidth - hint.X) / 2f), (int)(HintY - HintSpread.Y),
            (int)hint.X, (int)(hint.Y + HintSpread.Y * 2f));
        Text.DrawBacking(spriteBatch, _glow, backing, (int)HintSpread.X, HintBacking * alpha);
        Text.DrawCentered(spriteBatch, _detailFont, Hint, HintY, HintColor * alpha, Palette.Shadow * alpha);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>Opens the save slots over the title.</summary>
    /// <remarks>A slot picked for a new game comes back with no game in it yet; the creation screen makes one.</remarks>
    /// <param name="purpose">Whether a new game is going in a slot or one is coming out.</param>
    private void OpenSlots(SlotPurpose purpose)
    {
        var slots = new SaveSlotScreen(purpose);
        slots.SlotChosen += (slot, game) =>
        {
            if (game is null) CharacterCreationScreen.Begin(ScreenManager, slot);
            else Begin(game);
        };

        // Something may have been erased while it was open.
        slots.Closed += RefreshContinue;

        ScreenManager.AddScreen(slots);
    }

    /// <summary>CONTINUE: loads whichever slot was written last.</summary>
    /// <remarks>Read again here rather than remembered, in case the file changed since the title came up. If it's gone, CONTINUE just goes dead.</remarks>
    private void Continue()
    {
        SaveSlot latest = SaveSystem.Latest();
        if (latest is null)
        {
            RefreshContinue();
            return;
        }

        Begin(latest.Data);
    }

    /// <summary>Goes to the map the game is on, with its name on the black in between.</summary>
    /// <param name="game">The game, new or loaded.</param>
    private void Begin(SaveData game) =>
        LoadingScreen.Load(ScreenManager, MapCatalog.Get(game.MapId).Name.ToUpperInvariant(), new ExploreScreen(game));

    /// <summary>Lights CONTINUE if any slot has a game in it, and kills it if none do.</summary>
    private void RefreshContinue() => _continue.Plate.Enabled = SaveSystem.Latest() is not null;
}
