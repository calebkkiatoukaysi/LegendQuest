using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.Rules;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// Making your character, then the Soul-Bound: a name, a people, a class and a look, with them
/// turning round on the left as it changes. It's the Prologue's first three scenes as well.
/// </summary>
/// <remarks>
/// It runs twice. The first time it's you, and once you're made the narrator names you and asks
/// what drives you, which is your voice. The second time it's the Soul-Bound, remembered: the pier
/// you met on is behind everything, their name starts empty and has to be typed, and their pronouns
/// are on it. Once they're made the narrator remembers them, the game is written to its slot, and
/// it sets sail. The rows are on three tabs (who, face, dress), since there are too many to fit in
/// one column; left and right change one, Q and E change the tab. The paperdoll bakes them again
/// every time a row changes (it's small, so that's instant), and the face is blown up in the frame
/// over them, since it's only a few pixels on the figure. The scores are the class's recommended
/// spread for now; point buy comes with the character sheet.
/// </remarks>
public class CharacterCreationScreen : MenuScreen
{
    private const string YouHeading = "WHO ARE YOU, WANDERER?";
    private const string SoulBoundHeading = "WHO WALKS BESIDE YOU?";
    private const string SoulBoundLine = "No one walks into legend alone.";
    private const string Note = "LEFT AND RIGHT TO CHANGE  ·  Q AND E FOR THE TABS";
    private const string TypingNote = "TYPE THE NAME  ·  ENTER WHEN IT'S DONE";
    private const string NeedsNameNote = "THEY NEED A NAME  ·  ENTER ON NAME TO TYPE IT";
    private const string DefaultName = "Wanderer";

    /// <summary>The tabs, in order.</summary>
    private static readonly string[] Tabs = { "WHO", "FACE", "DRESS" };

    /// <summary>The longest a name can be. Long enough for most, short enough to fit on a party card.</summary>
    private const int NameLength = 12;

    /// <summary>The two panels: the one they're shown on, and the one with the rows.</summary>
    private static readonly Rectangle PreviewPanel = new(40, 30, 660, 840);
    private static readonly Rectangle OptionsPanel = new(720, 30, 840, 840);

    /// <summary>The tabs along the top of the rows, the rows, and the three buttons along the bottom.</summary>
    private static readonly Point TabSize = new(220, 54);
    private const float TabGap = 16f;
    private const float TabsY = 150f;
    private static readonly Point RowSize = new(720, 56);
    private const float RowGap = 10f;
    private const float RowsTop = 210f;
    private static readonly Point ButtonSize = new(210, 56);
    private const float ButtonGap = 20f;
    private const float ButtonsY = 760f;

    /// <summary>Them, full length: how much bigger than art pixels, and where their feet go.</summary>
    private const float FigureScale = 8f;
    private static readonly Vector2 FigureFeet = new(370f, 720f);

    /// <summary>Their face, blown up: the head and shoulders of the frame facing down, how big, and where.</summary>
    private static readonly Rectangle Bust = new(4, 4, 24, 24);
    private const float BustScale = 6f;
    private static readonly Point BustAt = new(96, 110);

    /// <summary>Where their name goes, beside their face, with their class under it.</summary>
    private static readonly Vector2 NameAt = new(450f, 150f);

    /// <summary>The shadow under their feet.</summary>
    private static readonly Point ShadowSize = new(150, 34);
    private static readonly Color ShadowColor = Color.Black * 0.45f;

    /// <summary>How long they face each way before turning, and how long a step takes.</summary>
    private const float TurnSeconds = 2.2f;
    private const float StepSeconds = 0.14f;

    /// <summary>The way they turn round: down, right, up, left, so it looks like turning on the spot.</summary>
    private static readonly Direction[] Turning = { Direction.Down, Direction.Right, Direction.Up, Direction.Left };

    /// <summary>How much the pier behind the Soul-Bound's pass is dimmed, so it reads as a memory.</summary>
    private static readonly Color MemoryVeil = new Color(16, 12, 36) * 0.55f;

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.4);

    private readonly InputAction _previousTab = new(new[] { Buttons.LeftShoulder }, new[] { Keys.Q }, true);
    private readonly InputAction _nextTab = new(new[] { Buttons.RightShoulder }, new[] { Keys.E }, true);

    private readonly int _slot;
    private readonly SaveData _game;
    private readonly bool _soulBound;
    private readonly PartyMember _member;
    private readonly Paperdoll _paperdoll = new();
    private readonly PaintingSprite _memory = new();
    private readonly FormPanel _panel = new();
    private readonly Random _random = new();
    private readonly StringBuilder _name = new();
    private readonly List<(MenuEntry Row, Func<string> Value)> _rows = new();
    private readonly List<List<MenuEntry>> _pages = new() { new(), new(), new() };
    private readonly List<MenuEntry> _tabs = new();
    private readonly List<MenuEntry> _buttons = new();
    private readonly MenuEntry _peopleRow;
    private readonly MenuEntry _classRow;

    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Texture2D _glow;
    private Texture2D _sheet;
    private float _clock;
    private int _page;
    private bool _typing;
    private bool _busy;
    private string _status;
    private float _noteY;

    /// <summary>Starts a new game in a slot: the Prologue's first card, then the creation screen for you.</summary>
    /// <param name="screens">The screen manager.</param>
    /// <param name="slot">The slot the game's going in.</param>
    public static void Begin(ScreenManager screens, int slot)
    {
        SaveData game = SaveData.NewGame(Origins.Create(Origins.You), Origins.Create(Origins.SoulBound));
        screens.Game.Services.GetService<Audio.AudioManager>().Play(Sfx.Heartbeat);

        var card = new DialogueScreen(Prologue.TitleCard, game, NoFace, new Random());
        card.Finished += _ => screens.AddScreen(new CharacterCreationScreen(slot, game, soulBound: false));
        screens.AddScreen(card);
    }

    /// <summary>Builds a row for each part of them, the tabs and the three buttons.</summary>
    /// <param name="slot">The slot the game's going in.</param>
    /// <param name="game">The new game, which this pass changes.</param>
    /// <param name="soulBound">Whether it's the Soul-Bound being made, not you.</param>
    public CharacterCreationScreen(int slot, SaveData game, bool soulBound)
    {
        _slot = slot;
        _game = game;
        _soulBound = soulBound;
        _member = soulBound ? game.Member(Origins.SoulBound) : game.You;
        _name.Append(_member.Name);
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        MenuEntry name = AddRow(0, "NAME", () => Typed() + (_typing ? "_" : string.Empty));
        name.Selected += StartTyping;
        if (soulBound)
            Adjustable(0, "PRONOUNS", () => (int)_member.Pronouns, v => _member.Pronouns = (Pronouns)v, new[] { "THEY / THEM", "HE / HIM", "SHE / HER" });
        _peopleRow = AddRow(0, "PEOPLE", () => _member.Look.Ancestry.ToString().ToUpperInvariant());
        _peopleRow.Adjusted += step =>
        {
            Ancestry[] all = Enum.GetValues<Ancestry>();
            _member.Look.Ancestry = all[LookOptions.Wrap(Array.IndexOf(all, _member.Look.Ancestry) + step, all.Length)];
            _member.Look.Lineage = 0;
            Refresh();
        };
        MenuEntry lineage = AddRow(0, "LINE", () => Lineages()[LookOptions.Wrap(_member.Look.Lineage, Lineages().Count)]);
        lineage.Adjusted += step =>
        {
            _member.Look.Lineage = LookOptions.Wrap(_member.Look.Lineage + step, Lineages().Count);
            Refresh();
        };
        _classRow = AddRow(0, "CLASS", () => ClassCatalog.Get(_member.Class).Name.ToUpperInvariant());
        _classRow.Adjusted += step => SetClass(Step(_member.Class, step));

        Adjustable(1, "SKIN", () => _member.Look.Skin, v => _member.Look.Skin = v, LookOptions.Skins);
        Adjustable(1, "FACE", () => _member.Look.Face, v => _member.Look.Face = v, Names(LookOptions.Faces));
        Adjustable(1, "EYES", () => _member.Look.Eyes, v => _member.Look.Eyes = v, LookOptions.Eyes);
        Adjustable(1, "HAIR", () => _member.Look.Hair, v => _member.Look.Hair = v, Names(LookOptions.Hair));
        Adjustable(1, "HAIR COLOUR", () => _member.Look.HairColour, v => _member.Look.HairColour = v, LookOptions.HairColours);
        Adjustable(1, "BEARD", () => _member.Look.Beard, v => _member.Look.Beard = v, Names(LookOptions.Beards));

        Adjustable(2, "HAT", () => _member.Look.Hat, v => _member.Look.Hat = v, Names(LookOptions.Hats));
        Adjustable(2, "SHIRT", () => _member.Look.Shirt, v => _member.Look.Shirt = v, LookOptions.Shirts);
        Adjustable(2, "VEST", () => _member.Look.Vest, v => _member.Look.Vest = v, LookOptions.Vests);
        Adjustable(2, "LEGS", () => _member.Look.Legs, v => _member.Look.Legs = v, Names(LookOptions.Legs));
        Adjustable(2, "LEGS COLOUR", () => _member.Look.LegsColour, v => _member.Look.LegsColour = v, LookOptions.LegsColours);
        Adjustable(2, "CLOAK", () => _member.Look.Cloak, v => _member.Look.Cloak = v, LookOptions.Cloaks);

        for (int i = 0; i < Tabs.Length; i++)
        {
            int page = i;
            var tab = new MenuEntry(Tabs[i], Palette.Bone, TabSize);
            tab.Selected += () => ShowPage(page);
            _tabs.Add(tab);
        }
        AddButton("RANDOM", Palette.Bone, Randomise);
        AddButton("BEGIN", Palette.Torch, Confirm);
        AddButton("BACK", Palette.Ember, Back);

        // Every entry goes in now, so MenuScreen loads them all; ShowPage keeps just one tab's.
        foreach (MenuEntry tab in _tabs) MenuEntries.Add(tab);
        foreach (List<MenuEntry> page in _pages)
            foreach (MenuEntry row in page) MenuEntries.Add(row);
        foreach (MenuEntry button in _buttons) MenuEntries.Add(button);

        SetClass(_member.Class);
    }

    /// <summary>Loads the paperdoll, bakes them, lays the rows out, and starts listening for typing.</summary>
    public override void Activate()
    {
        base.Activate();

        _panel.LoadContent(Content, ScreenManager.GraphicsDevice);
        _paperdoll.LoadContent(Content);
        if (_soulBound) _memory.LoadContent(Content);
        _font = Content.Load<SpriteFont>("spectral-ui");
        _detailFont = Content.Load<SpriteFont>("spectral-detail");
        _glow = Content.Load<Texture2D>("glow");

        LayOut();
        ShowPage(0);
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

    /// <summary>While a name's being typed the menu doesn't move (WASD are letters too); otherwise Q and E change the tab, and the rest is a menu.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_busy) return;

        if (_typing)
        {
            if (input.IsNewKeyPress(Keys.Enter) || input.IsNewKeyPress(Keys.Escape) || input.IsNewLeftClick)
                StopTyping();
            return;
        }

        if (_previousTab.Occurred(input)) ShowPage(LookOptions.Wrap(_page - 1, Tabs.Length));
        else if (_nextTab.Occurred(input)) ShowPage(LookOptions.Wrap(_page + 1, Tabs.Length));
        base.HandleInput(gameTime, input);
    }

    /// <summary>Runs the clock they walk and turn by, and the pier's sway behind the Soul-Bound.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_soulBound) _memory.Update(gameTime);
    }

    /// <summary>Draws the memory behind the Soul-Bound, both panels, them, their face, the headings and the note, then the rows.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        if (_soulBound)
        {
            _memory.Draw(spriteBatch, "pier-memory", alpha);
            spriteBatch.Draw(ScreenManager.BlankTexture, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight), MemoryVeil * alpha);
        }
        else
        {
            _panel.DrawVeil(spriteBatch, alpha);
        }
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
        string heading = _soulBound ? SoulBoundHeading : YouHeading;
        Text.Draw(spriteBatch, _font, heading, OptionsPanel.Center.X, OptionsPanel.Top + FormPanel.Padding,
            Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        if (_soulBound)
            Text.Draw(spriteBatch, _detailFont, SoulBoundLine, OptionsPanel.Center.X, OptionsPanel.Top + FormPanel.Padding + _font.LineSpacing,
                Palette.Narration * alpha, Palette.FormShadow * alpha, centred: true);

        string shownName = Typed().Length > 0 ? Typed().ToUpperInvariant() : "?";
        Text.Draw(spriteBatch, _font, shownName, NameAt.X, NameAt.Y, Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, Summary(), NameAt.X, NameAt.Y + _font.LineSpacing + 6f,
            Palette.Bone * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, Scores(), PreviewPanel.Center.X, FigureFeet.Y + 40f,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, About(), OptionsPanel.Center.X, ButtonsY - ButtonSize.Y / 2f - 18f - _detailFont.LineSpacing,
            Palette.Narration * alpha, Palette.FormShadow * alpha, centred: true);

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

    /// <summary>For the proof run: a look and a class, as if they'd been picked row by row.</summary>
    /// <param name="look">How they look.</param>
    /// <param name="classId">Their class.</param>
    internal void ProofLook(CharacterLook look, ClassId classId)
    {
        _member.Look = look;
        SetClass(classId);
    }

    /// <summary>For the proof run: the Soul-Bound's pronouns, as if they'd been picked on their row.</summary>
    internal void ProofPronouns(Pronouns pronouns)
    {
        _member.Pronouns = pronouns;
        Refresh();
    }

    /// <summary>For the proof run: one of the tabs.</summary>
    internal void ProofPage(int page) => ShowPage(page);

    /// <summary>For the proof run: BEGIN.</summary>
    internal void ProofBegin() => Confirm();

    /// <summary>A row with a value between arrows, on a tab.</summary>
    private MenuEntry AddRow(int page, string label, Func<string> value)
    {
        var row = new MenuEntry(string.Empty, Palette.Torch, RowSize);
        _pages[page].Add(row);
        _rows.Add((row, () => label + "    <  " + value() + "  >"));
        return row;
    }

    /// <summary>A row that steps through a list, wrapping round.</summary>
    private void Adjustable(int page, string label, Func<int> get, Action<int> set, IReadOnlyList<string> names)
    {
        MenuEntry row = AddRow(page, label, () => names[LookOptions.Wrap(get(), names.Count)]);
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
        _buttons.Add(button);
    }

    /// <summary>Shows one tab's rows, with the tabs above them and the buttons below. The other tabs' rows are left out.</summary>
    private void ShowPage(int page)
    {
        _page = page;
        MenuEntries.Clear();
        foreach (MenuEntry tab in _tabs) MenuEntries.Add(tab);
        foreach (MenuEntry row in _pages[page]) MenuEntries.Add(row);
        foreach (MenuEntry button in _buttons) MenuEntries.Add(button);

        for (int i = 0; i < _tabs.Count; i++) _tabs[i].Plate.Accent = i == page ? Palette.Torch : Palette.Bone;
        SelectedEntry = _tabs.Count;
    }

    /// <summary>The names off a list of options.</summary>
    private static IReadOnlyList<string> Names(IReadOnlyList<LookOptions.Option> options)
    {
        var names = new string[options.Count];
        for (int i = 0; i < names.Length; i++) names[i] = options[i].Name;
        return names;
    }

    /// <summary>The lines their people comes in.</summary>
    private IReadOnlyList<string> Lineages() => LookOptions.Lineages(_member.Look.Ancestry);

    /// <summary>The class a step along from another, wrapping round.</summary>
    private static ClassId Step(ClassId from, int step)
    {
        ClassId[] all = Enum.GetValues<ClassId>();
        return all[LookOptions.Wrap(Array.IndexOf(all, from) + step, all.Length)];
    }

    /// <summary>Their class, and its recommended scores with it.</summary>
    private void SetClass(ClassId id)
    {
        _member.Class = id;
        _member.Scores = ClassCatalog.Get(id).Recommended;
        _member.Hp = _member.MaxHp;
        Refresh();
    }

    /// <summary>RANDOM: a whole new look. The name and class stay.</summary>
    private void Randomise()
    {
        _member.Look = LookOptions.Random(_random);
        Refresh();
    }

    /// <summary>Starts typing the name.</summary>
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

    /// <summary>The name as typed. You start as the default if nothing's been typed; the Soul-Bound starts with nothing.</summary>
    private string Typed() => _name.Length > 0 || _typing || _soulBound ? _name.ToString() : DefaultName;

    /// <summary>BACK: from you, back to the title without writing anything; from the Soul-Bound, back to you.</summary>
    private void Back()
    {
        Audio.Play(Sfx.MenuBack);
        ExitScreen();
        if (_soulBound) ScreenManager.AddScreen(new CharacterCreationScreen(_slot, _game, soulBound: false));
    }

    /// <summary>BEGIN: names them, then the narrator's scene for this pass, then the next pass or the voyage.</summary>
    private void Confirm()
    {
        string name = Typed().Trim();
        if (_soulBound && name.Length == 0)
        {
            _status = NeedsNameNote;
            Audio.Play(Sfx.MenuBack);
            return;
        }
        _member.Name = name.Length > 0 ? name : DefaultName;
        _busy = true;

        DialogueScript scene = _soulBound ? Prologue.Remembered : Prologue.YourVoice;
        var talk = new DialogueScreen(scene, _game, NoFace, _random);
        talk.Finished += _ =>
        {
            _busy = false;
            if (_soulBound)
            {
                SetSail();
                return;
            }
            ExitScreen();
            ScreenManager.AddScreen(new CharacterCreationScreen(_slot, _game, soulBound: true));
        };
        ScreenManager.AddScreen(talk);
    }

    /// <summary>Writes the new game to its slot and goes to the ship. If the slot can't be written, it says so and stays.</summary>
    private void SetSail()
    {
        if (!SaveSystem.Write(_slot, _game))
        {
            _status = "COULD NOT WRITE THE SLOT -- " + SaveSystem.LastError;
            return;
        }

        LoadingScreen.Load(ScreenManager, MapCatalog.Get(_game.MapId).Name.ToUpperInvariant(), new ExploreScreen(_game));
    }

    /// <summary>The narrator has no face, and nobody else talks before the ship.</summary>
    private static (string Name, Texture2D Sheet) NoFace(string id) => (string.Empty, null);

    /// <summary>Bakes them again and writes every row's value onto its plate.</summary>
    private void Refresh()
    {
        foreach ((MenuEntry row, Func<string> value) in _rows) row.Text = value();

        if (ScreenManager is null || _font is null) return;
        _sheet?.Dispose();
        _sheet = _paperdoll.Compose(ScreenManager.GraphicsDevice, _member.Look);
    }

    /// <summary>The tabs under the heading, each tab's rows down the panel, and the buttons along the bottom.</summary>
    private void LayOut()
    {
        float width = _tabs.Count * TabSize.X + (_tabs.Count - 1) * TabGap;
        float left = OptionsPanel.Center.X - width / 2f;
        foreach (MenuEntry tab in _tabs)
        {
            tab.Plate.Center = new Vector2(left + TabSize.X / 2f, TabsY);
            left += TabSize.X + TabGap;
        }

        foreach (List<MenuEntry> page in _pages)
        {
            float y = RowsTop;
            foreach (MenuEntry row in page)
            {
                row.Plate.Center = new Vector2(OptionsPanel.Center.X, y + RowSize.Y / 2f);
                y += RowSize.Y + RowGap;
            }
        }

        width = _buttons.Count * ButtonSize.X + (_buttons.Count - 1) * ButtonGap;
        left = OptionsPanel.Center.X - width / 2f;
        foreach (MenuEntry button in _buttons)
        {
            button.Plate.Center = new Vector2(left + ButtonSize.X / 2f, ButtonsY);
            left += ButtonSize.X + ButtonGap;
        }
        _noteY = ButtonsY + ButtonSize.Y / 2f + 14f;
    }

    /// <summary>Them, full length, walking on the spot and turning round.</summary>
    private void DrawFigure(SpriteBatch spriteBatch, float alpha)
    {
        if (_sheet is null) return;

        Direction facing = Turning[(int)(_clock / TurnSeconds) % Turning.Length];
        int frame = 1 + (int)(_clock / StepSeconds) % 4;
        var source = new Rectangle(frame * 32, (int)facing * 48, 32, 48);
        spriteBatch.Draw(_sheet, FigureFeet, source, Color.White * alpha, 0f, new Vector2(16f, 46f), FigureScale, SpriteEffects.None, 0f);
    }

    /// <summary>Their face, blown up, on a panel of its own.</summary>
    private void DrawBust(SpriteBatch spriteBatch, float alpha)
    {
        if (_sheet is null) return;

        var frame = new Rectangle(BustAt.X - 16, BustAt.Y - 16, (int)(Bust.Width * BustScale) + 32, (int)(Bust.Height * BustScale) + 32);
        _panel.Draw(spriteBatch, frame, alpha);
        spriteBatch.Draw(_sheet, BustAt.ToVector2(), Bust, Color.White * alpha, 0f, Vector2.Zero, BustScale, SpriteEffects.None, 0f);
    }

    /// <summary>Their people, class and hit points, under their name.</summary>
    private string Summary() =>
        _member.Look.Ancestry.ToString().ToUpperInvariant() + "  ·  " + ClassCatalog.Get(_member.Class).Name.ToUpperInvariant() + "  ·  " + _member.MaxHp + " HP";

    /// <summary>A line about whichever row's selected, if it's the people or the class.</summary>
    private string About()
    {
        MenuEntry selected = SelectedEntry < MenuEntries.Count ? MenuEntries[SelectedEntry] : null;
        if (selected == _peopleRow) return LookOptions.Peoples[(int)_member.Look.Ancestry];
        if (selected == _classRow) return ClassCatalog.Get(_member.Class).Role;
        return string.Empty;
    }

    /// <summary>Their six scores, under them.</summary>
    private string Scores()
    {
        AbilityScores s = _member.Scores;
        return $"STR {s.Strength}  DEX {s.Dexterity}  CON {s.Constitution}  INT {s.Intelligence}  WIS {s.Wisdom}  CHA {s.Charisma}";
    }
}
