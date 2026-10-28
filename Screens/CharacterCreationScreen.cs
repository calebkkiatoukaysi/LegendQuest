using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Components;
using LegendQuest.Rules;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// Making your character: your name, your class, and how you look, with you turning round on the
/// left as it changes. BEGIN writes the new game into the slot you picked and sets sail.
/// </summary>
/// <remarks>
/// A menu like the options, a row for every part of your look with left and right to change it,
/// and the paperdoll baking you again every time one changes (it's small, so that's instant). Your
/// face is shown twice: on the figure, and blown up in the frame above it, since the face is only a
/// few pixels on the figure. Your name gets typed into its row: Enter or a click on it starts
/// typing, and Enter or Escape stops. The scores are your class's recommended spread for now; point
/// buy comes with the character screen.
/// </remarks>
public class CharacterCreationScreen : MenuScreen
{
    private const string Heading = "WHO ARE YOU?";
    private const string Note = "LEFT AND RIGHT TO CHANGE  ·  ENTER ON NAME TO TYPE";
    private const string TypingNote = "TYPE YOUR NAME  ·  ENTER WHEN IT'S DONE";
    private const string DefaultName = "Wanderer";

    /// <summary>The longest a name can be. Long enough for most, short enough to fit on a party card.</summary>
    private const int NameLength = 12;

    /// <summary>The two panels: the one you're shown on, and the one with the rows.</summary>
    private static readonly Rectangle PreviewPanel = new(40, 30, 660, 840);
    private static readonly Rectangle OptionsPanel = new(720, 30, 840, 840);

    /// <summary>The rows, and the three buttons along the bottom.</summary>
    private static readonly Point RowSize = new(764, 42);
    private const float RowGap = 4f;
    private static readonly Point ButtonSize = new(210, 54);
    private const float ButtonGap = 20f;
    private const float RowsTop = 134f;

    /// <summary>You, full length: how much bigger than art pixels, and where your feet go.</summary>
    private const float FigureScale = 8f;
    private static readonly Vector2 FigureFeet = new(370f, 720f);

    /// <summary>Your face, blown up: the head and shoulders of the frame facing down, how big, and where.</summary>
    private static readonly Rectangle Bust = new(4, 4, 24, 24);
    private const float BustScale = 6f;
    private static readonly Point BustAt = new(96, 110);

    /// <summary>Where your name goes, beside your face, with your class under it.</summary>
    private static readonly Vector2 NameAt = new(450f, 150f);

    /// <summary>The shadow under your feet.</summary>
    private static readonly Point ShadowSize = new(150, 34);
    private static readonly Color ShadowColor = Color.Black * 0.45f;

    /// <summary>How long you face each way before turning, and how long a step takes.</summary>
    private const float TurnSeconds = 2.2f;
    private const float StepSeconds = 0.14f;

    /// <summary>The way you turn round: down, right, up, left, so it looks like turning on the spot.</summary>
    private static readonly Direction[] Turning = { Direction.Down, Direction.Right, Direction.Up, Direction.Left };

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.4);

    private readonly int _slot;
    private readonly PartyMember _you = Origins.Create(Origins.You);
    private readonly Paperdoll _paperdoll = new();
    private readonly FormPanel _panel = new();
    private readonly Random _random = new();
    private readonly StringBuilder _name = new();
    private readonly MenuEntry _nameRow;
    private readonly MenuEntry _classRow;
    private readonly List<(MenuEntry Row, Func<string> Value)> _rows = new();
    private readonly List<MenuEntry> _buttons = new();

    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Texture2D _glow;
    private Texture2D _sheet;
    private float _clock;
    private bool _typing;
    private string _status;
    private float _noteY;

    /// <summary>Builds a row for each part of you and the three buttons.</summary>
    /// <param name="slot">The slot the game's going in.</param>
    public CharacterCreationScreen(int slot)
    {
        _slot = slot;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _nameRow = AddRow("NAME", () => Typed() + (_typing ? "_" : string.Empty));
        _nameRow.Selected += StartTyping;

        _classRow = AddRow("CLASS", () => ClassCatalog.Get(_you.Class).Name.ToUpperInvariant());
        _classRow.Adjusted += step => SetClass(Step(_you.Class, step));

        Adjustable("SKIN", () => _you.Look.Skin, v => _you.Look.Skin = v, LookOptions.Skins);
        Adjustable("FACE", () => _you.Look.Face, v => _you.Look.Face = v, Names(LookOptions.Faces));
        Adjustable("EYES", () => _you.Look.Eyes, v => _you.Look.Eyes = v, LookOptions.Eyes);
        Adjustable("HAIR", () => _you.Look.Hair, v => _you.Look.Hair = v, Names(LookOptions.Hair));
        Adjustable("HAIR COLOUR", () => _you.Look.HairColour, v => _you.Look.HairColour = v, LookOptions.HairColours);
        Adjustable("BEARD", () => _you.Look.Beard, v => _you.Look.Beard = v, Names(LookOptions.Beards));
        Adjustable("SHIRT", () => _you.Look.Shirt, v => _you.Look.Shirt = v, LookOptions.Shirts);
        Adjustable("VEST", () => _you.Look.Vest, v => _you.Look.Vest = v, LookOptions.Vests);
        Adjustable("LEGS", () => _you.Look.Legs, v => _you.Look.Legs = v, Names(LookOptions.Legs));
        Adjustable("LEGS COLOUR", () => _you.Look.LegsColour, v => _you.Look.LegsColour = v, LookOptions.LegsColours);
        Adjustable("CLOAK", () => _you.Look.Cloak, v => _you.Look.Cloak = v, LookOptions.Cloaks);

        AddButton("RANDOM", Palette.Bone, Randomise);
        AddButton("BEGIN", Palette.Torch, Begin);
        AddButton("BACK", Palette.Ember, Back);

        SetClass(_you.Class);
    }

    /// <summary>Loads the paperdoll, bakes you, lays the rows out, and starts listening for typing.</summary>
    public override void Activate()
    {
        base.Activate();

        _panel.LoadContent(Content, ScreenManager.GraphicsDevice);
        _paperdoll.LoadContent(Content);
        _font = Content.Load<SpriteFont>("spectral-ui");
        _detailFont = Content.Load<SpriteFont>("spectral-detail");
        _glow = Content.Load<Texture2D>("glow");

        LayOut();
        Refresh();
        ScreenManager.Game.Window.TextInput += OnTextInput;
    }

    /// <summary>Stops listening for typing and lets go of the baked sheet.</summary>
    public override void Unload()
    {
        ScreenManager.Game.Window.TextInput -= OnTextInput;
        _sheet?.Dispose();
        base.Unload();
    }

    /// <summary>While you're typing your name the menu doesn't move (WASD are letters too); otherwise it's a menu.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_typing)
        {
            if (input.IsNewKeyPress(Keys.Enter) || input.IsNewKeyPress(Keys.Escape) || input.IsNewLeftClick)
                StopTyping();
            return;
        }

        base.HandleInput(gameTime, input);
    }

    /// <summary>Runs the clock you walk and turn by.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>Draws both panels, you, your face, the headings and the note, then the rows.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _panel.DrawVeil(spriteBatch, alpha);
        _panel.Draw(spriteBatch, PreviewPanel, alpha);
        _panel.Draw(spriteBatch, OptionsPanel, alpha);
        spriteBatch.End();

        // The shadow's the glow squashed flat, so it wants smoothing.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        spriteBatch.Draw(_glow, new Rectangle((int)FigureFeet.X - ShadowSize.X / 2, (int)FigureFeet.Y - ShadowSize.Y / 2,
            ShadowSize.X, ShadowSize.Y), ShadowColor * alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawFigure(spriteBatch, alpha);
        DrawBust(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        Text.Draw(spriteBatch, _font, Heading, OptionsPanel.Center.X, OptionsPanel.Top + FormPanel.Padding,
            Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _font, Typed().ToUpperInvariant(), NameAt.X, NameAt.Y,
            Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, Summary(), NameAt.X, NameAt.Y + _font.LineSpacing + 6f,
            Palette.Bone * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, Scores(), PreviewPanel.Center.X, FigureFeet.Y + 40f,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        string note = _status ?? (_typing ? TypingNote : Note);
        Text.Draw(spriteBatch, _detailFont, note, OptionsPanel.Center.X, _noteY,
            (_status is null ? Palette.Note : Palette.Ember) * alpha, Palette.FormShadow * alpha, centred: true);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>Escape is the same as BACK.</summary>
    protected override void OnCancel() => Back();

    /// <summary>For the proof run: types a name, the way a player would.</summary>
    /// <param name="name">The name.</param>
    internal void ProofType(string name)
    {
        _name.Clear().Append(name);
        Refresh();
    }

    /// <summary>For the proof run: a look, as if it had been picked row by row.</summary>
    /// <param name="look">How you look.</param>
    /// <param name="classId">Your class.</param>
    internal void ProofLook(CharacterLook look, ClassId classId)
    {
        _you.Look = look;
        SetClass(classId);
    }

    /// <summary>For the proof run: BEGIN.</summary>
    internal void ProofBegin() => Begin();

    /// <summary>A row with a value between arrows.</summary>
    private MenuEntry AddRow(string label, Func<string> value)
    {
        var row = new MenuEntry(string.Empty, Palette.Torch, RowSize);
        MenuEntries.Add(row);
        _rows.Add((row, () => label + "    <  " + value() + "  >"));
        return row;
    }

    /// <summary>A row that steps through one of LookOptions' lists, wrapping round.</summary>
    private void Adjustable(string label, Func<int> get, Action<int> set, IReadOnlyList<string> names)
    {
        MenuEntry row = AddRow(label, () => names[LookOptions.Wrap(get(), names.Count)]);
        row.Adjusted += step =>
        {
            set(LookOptions.Wrap(get() + step, names.Count));
            Refresh();
        };
    }

    /// <summary>One of the buttons along the bottom.</summary>
    private void AddButton(string label, Color accent, Action picked)
    {
        var button = new MenuEntry(label, accent, ButtonSize);
        button.Selected += picked;
        MenuEntries.Add(button);
        _buttons.Add(button);
    }

    /// <summary>The names off a list of options.</summary>
    private static IReadOnlyList<string> Names(IReadOnlyList<LookOptions.Option> options)
    {
        var names = new string[options.Count];
        for (int i = 0; i < names.Length; i++) names[i] = options[i].Name;
        return names;
    }

    /// <summary>The class a step along from another, wrapping round.</summary>
    private static ClassId Step(ClassId from, int step)
    {
        ClassId[] all = Enum.GetValues<ClassId>();
        return all[LookOptions.Wrap(Array.IndexOf(all, from) + step, all.Length)];
    }

    /// <summary>Your class, and its recommended scores with it.</summary>
    private void SetClass(ClassId id)
    {
        _you.Class = id;
        _you.Scores = ClassCatalog.Get(id).Recommended;
        _you.Hp = _you.MaxHp;
        Refresh();
    }

    /// <summary>RANDOM: a whole new look. Your name and class stay.</summary>
    private void Randomise()
    {
        _you.Look = LookOptions.Random(_random);
        Refresh();
    }

    /// <summary>Starts typing your name.</summary>
    private void StartTyping()
    {
        _typing = true;
        _status = null;
        Refresh();
    }

    /// <summary>Stops typing, and tidies the name up.</summary>
    private void StopTyping()
    {
        _typing = false;
        string tidy = _name.ToString().Trim();
        _name.Clear().Append(tidy);
        Refresh();
    }

    /// <summary>A letter typed while the name's being typed. Backspace takes one off.</summary>
    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        if (!_typing) return;

        if (e.Key == Keys.Back)
        {
            if (_name.Length > 0) _name.Length--;
        }
        else if (_name.Length < NameLength && (char.IsLetter(e.Character) || e.Character is ' ' or '-' or '\''))
        {
            _name.Append(e.Character);
        }
        Refresh();
    }

    /// <summary>The name as typed, or the default if nothing's been typed.</summary>
    private string Typed() => _name.Length > 0 || _typing ? _name.ToString() : DefaultName;

    /// <summary>BACK: leaves without writing anything, back to the title.</summary>
    private void Back()
    {
        Audio.Play(Sfx.MenuBack);
        ExitScreen();
    }

    /// <summary>BEGIN: puts you in a new game, writes it to the slot, and goes to the boat.</summary>
    private void Begin()
    {
        string name = Typed().Trim();
        _you.Name = name.Length > 0 ? name : DefaultName;

        SaveData game = SaveData.NewGame(_you);
        if (!SaveSystem.Write(_slot, game))
        {
            _status = "COULD NOT WRITE THE SLOT -- " + SaveSystem.LastError;
            return;
        }

        LoadingScreen.Load(ScreenManager, MapCatalog.Get(game.MapId).Name.ToUpperInvariant(), new ExploreScreen(game));
    }

    /// <summary>Bakes you again and writes every row's value onto its plate.</summary>
    private void Refresh()
    {
        foreach ((MenuEntry row, Func<string> value) in _rows) row.Text = value();

        if (_paperdoll is null || ScreenManager is null || _font is null) return;
        _sheet?.Dispose();
        _sheet = _paperdoll.Compose(ScreenManager.GraphicsDevice, _you.Look);
    }

    /// <summary>The rows down the options panel, and the buttons in a row under them.</summary>
    private void LayOut()
    {
        float y = RowsTop;
        foreach ((MenuEntry row, _) in _rows)
        {
            row.Plate.Center = new Vector2(OptionsPanel.Center.X, y + RowSize.Y / 2f);
            y += RowSize.Y + RowGap;
        }

        float width = _buttons.Count * ButtonSize.X + (_buttons.Count - 1) * ButtonGap;
        float left = OptionsPanel.Center.X - width / 2f;
        float buttonsY = y + 8f + ButtonSize.Y / 2f;
        foreach (MenuEntry button in _buttons)
        {
            button.Plate.Center = new Vector2(left + ButtonSize.X / 2f, buttonsY);
            left += ButtonSize.X + ButtonGap;
        }
        _noteY = buttonsY + ButtonSize.Y / 2f + 14f;
    }

    /// <summary>You, full length, walking on the spot and turning round.</summary>
    private void DrawFigure(SpriteBatch spriteBatch, float alpha)
    {
        if (_sheet is null) return;

        Direction facing = Turning[(int)(_clock / TurnSeconds) % Turning.Length];
        int frame = 1 + (int)(_clock / StepSeconds) % 4;
        var source = new Rectangle(frame * 32, (int)facing * 48, 32, 48);
        spriteBatch.Draw(_sheet, FigureFeet, source, Color.White * alpha, 0f, new Vector2(16f, 46f), FigureScale, SpriteEffects.None, 0f);
    }

    /// <summary>Your face, blown up, on a panel of its own.</summary>
    private void DrawBust(SpriteBatch spriteBatch, float alpha)
    {
        if (_sheet is null) return;

        var frame = new Rectangle(BustAt.X - 16, BustAt.Y - 16, (int)(Bust.Width * BustScale) + 32, (int)(Bust.Height * BustScale) + 32);
        _panel.Draw(spriteBatch, frame, alpha);
        spriteBatch.Draw(_sheet, BustAt.ToVector2(), Bust, Color.White * alpha, 0f, Vector2.Zero, BustScale, SpriteEffects.None, 0f);
    }

    /// <summary>Your class and hit points, under you.</summary>
    private string Summary() => ClassCatalog.Get(_you.Class).Name.ToUpperInvariant() + "  ·  LEVEL 1  ·  " + _you.MaxHp + " HP";

    /// <summary>Your six scores, under that.</summary>
    private string Scores()
    {
        AbilityScores s = _you.Scores;
        return $"STR {s.Strength}  DEX {s.Dexterity}  CON {s.Constitution}  INT {s.Intelligence}  WIS {s.Wisdom}  CHA {s.Charisma}";
    }
}
