using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Save;
using LegendQuest.StateManagement;

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

    /// <summary>What the loading screen says when a game is picked, until there's a map to go to.</summary>
    /// <remarks>Goes away with the first map. Picking a slot just comes back to the title for now.</remarks>
    private const string NothingYetLine = "THERE IS NOTHING PAST THE GATE YET.";

    private const float TitleY = 48f;
    private const float HintY = 858f;

    /// <summary>Where the row of buttons sits (on the flagstones under the gate) and the gap between them.</summary>
    private const float ButtonRowY = 806f;
    private const float ButtonGap = 30f;

    /// <summary>How long the title takes to fade in and out.</summary>
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.5);

    // The title gets a tight shadow the colour of old blood instead of black. The serif is light,
    // and anything wider ghosts around the strokes.
    private static readonly Color TitleColor = new(238, 228, 214);
    private static readonly Color TitleShadow = new(92, 22, 14);
    private static readonly Color HintColor = new(150, 138, 128);

    private readonly MenuEntry _continue;

    private SpriteFont _titleFont;
    private SpriteFont _detailFont;

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
        _continue.Selected += Begin;
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

        LayOutRow(new Vector2(LegendQuestGame.ScreenWidth / 2f, ButtonRowY), ButtonGap);
        RefreshContinue();

        // If there's a game to carry on, that's the likeliest thing to want.
        if (_continue.Plate.Enabled) SelectedEntry = MenuEntries.IndexOf(_continue);
    }

    /// <summary>Escape on the title closes the game.</summary>
    protected override void OnCancel() => ScreenManager.Game.Exit();

    /// <summary>Draws the title and the hint over the gate, then the row of buttons.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        Text.DrawCentered(spriteBatch, _titleFont, Title, TitleY, TitleColor * alpha, TitleShadow * alpha);
        Text.DrawCentered(spriteBatch, _detailFont, Hint, HintY, HintColor * alpha, Palette.Shadow * alpha);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>Opens the save slots over the title.</summary>
    /// <param name="purpose">Whether a new game is going in a slot or one is coming out.</param>
    private void OpenSlots(SlotPurpose purpose)
    {
        var slots = new SaveSlotScreen(purpose);
        slots.SlotChosen += (_, _) => Begin();

        // Something may have been erased while it was open.
        slots.Closed += RefreshContinue;

        ScreenManager.AddScreen(slots);
    }

    /// <summary>Starts or carries on a game.</summary>
    /// <remarks>There are no maps yet, so for now it goes through the loading screen and back to the title.</remarks>
    private void Begin() =>
        LoadingScreen.Load(ScreenManager, NothingYetLine, new TitleBackgroundScreen(), new TitleScreen());

    /// <summary>Lights CONTINUE if any slot has a game in it, and kills it if none do.</summary>
    private void RefreshContinue() => _continue.Plate.Enabled = SaveSystem.Latest() is not null;
}
