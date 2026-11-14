using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Dialogue.Scripts.Generated;
using LegendQuest.Rules;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// Making your character, then the Soul-Bound: a name, a people, a class and a look, with them
/// standing big on the Gull's Wager's deck as it changes. It's the Prologue's first three scenes as well.
/// </summary>
/// <remarks>
/// It runs twice. The first time it's you, and once you're made the narrator names you and asks
/// what drives you, which is your voice. The second time it's the Soul-Bound, remembered: the pier
/// you met on is behind everything, their name starts empty and has to be typed, and their pronouns
/// are on it. Once they're made the narrator remembers them, the game is written to its slot, and
/// it sets sail. The rows are on three tabs: WHO (name, people, class), CHARACTER (the painted face
/// and its skin, hair and eyes, height and build) and ARMOR (the Base and Trim whatever they wear is
/// dyed, and whether the helm and cape show). Left and right change a row and Q and E the tab; the
/// row that's picked shows everything it can be under the rows, face thumbnails, swatches or a
/// slider, to click on.
/// <para>
/// They stand on the ship's main deck (the Prologue's own room, its sky, sea and lanterns moving
/// behind them) as the rooms will draw them, in what their class starts wearing: a walker with their
/// painted figure, put together big off the main thread (PartCatalog.LargeWhenReady) and faded in
/// over the last one when it's ready, so changing anything never stalls a frame. It breathes, walks,
/// attacks and casts (F), turns round (R), and shows its colours as painted while V's held. Without a
/// painted face the paperdoll stands there instead, turning, with its own rows on the tabs. Their
/// name is typed into the box on the card at the top left (click it, or Tab), beside their face. The
/// grids' thumbnails are put together off the main thread too (PartCatalog.ThumbnailWhenReady) and
/// show a soft placeholder till they're ready. The scores are the class's recommended spread for
/// now; point buy comes with the character sheet.
/// </para>
/// </remarks>
public class CharacterCreationScreen : MenuScreen
{
    private const string YouHeading = "WHO ARE YOU, WANDERER?";
    private const string SoulBoundHeading = "WHO WALKS BESIDE YOU?";
    private const string SoulBoundLine = "No one walks into legend alone.";
    private const string Note = "LEFT / RIGHT TO CHANGE  ·  Q / E FOR TABS  ·  TAB TO NAME";
    private const string TypingNote = "TYPE THE NAME  ·  ENTER WHEN IT'S DONE";
    private const string NeedsNameNote = "THEY NEED A NAME  ·  CLICK THE NAME BOX TO TYPE IT";
    private const string NamePlaceholder = "Click to type a name";
    private const string DefaultName = "Wanderer";

    /// <summary>The tabs, in order.</summary>
    private static readonly string[] Tabs = { "WHO", "LOOK", "EXTRAS", "ARMOR" };

    /// <summary>The longest a name can be. Long enough for most, short enough to fit on a party card.</summary>
    private const int NameLength = 12;

    /// <summary>The panel with the rows, down the right, and the card with their face and name, at the top left over the deck.</summary>
    private static readonly Rectangle OptionsPanel = new(980, 30, 590, 840);
    private static readonly Rectangle NameCard = new(40, 30, 560, 176);

    /// <summary>On the card: their face's well, the label over the name box, the box, and the line under it.</summary>
    private static readonly Rectangle PortraitWell = new(64, 56, 120, 124);
    private static readonly Vector2 NameLabelAt = new(206f, 58f);
    private static readonly Rectangle NameBox = new(204, 86, 372, 54);
    private const float SummaryGap = 12f;

    /// <summary>How far in from the name box's left the name starts, how fast its caret blinks, and the caret's width.</summary>
    private const float NameInset = 16f;
    private const float CaretSeconds = 0.53f;
    private const int CaretWidth = 3;

    /// <summary>The tabs along the top of the rows, the rows, and the three buttons along the bottom.</summary>
    private static readonly Point TabSize = new(132, 48);
    private const float TabGap = 8f;
    private const float TabsY = 140f;
    private static readonly Point RowSize = new(540, 46);
    private const float RowGap = 8f;
    private const float RowsTop = 186f;
    private static readonly Point ButtonSize = new(170, 52);
    private const float ButtonGap = 14f;
    private const float ButtonsY = 776f;

    /// <summary>When a tab's rows come in: how long they take to fade and rise, and how far they rise.</summary>
    private const float RowsInSeconds = 0.2f;
    private const float RowsRise = 12f;

    /// <summary>The paperdoll, full length: how much bigger than art pixels. And where their feet go on the deck, either way.</summary>
    private const float FigureScale = 11f;
    private static readonly Vector2 FigureFeet = new(500f, 800f);

    /// <summary>The paperdoll's face, blown up for the card: the head and shoulders of the frame facing down, and how big.</summary>
    private static readonly Rectangle Bust = new(4, 4, 24, 24);
    private const float BustScale = 4.5f;

    /// <summary>The shadow under their feet.</summary>
    private static readonly Point ShadowSize = new(230, 46);

    /// <summary>How tall a standard adult's painted figure stands on the deck, in screen pixels.</summary>
    private const int PaintedHeight = 600;

    /// <summary>How long a new figure takes to fade in over the last one.</summary>
    private const float FigureFadeSeconds = 0.18f;

    /// <summary>The deck they stand on: the Prologue's ship and its room, where on it they stand (its start), and how much it's dimmed so they stand out.</summary>
    private const string DeckZone = "gulls-wager", DeckRoom = "deck", DeckSpot = "start";
    private static readonly Color DeckVeil = new Color(10, 12, 30) * 0.28f;

    /// <summary>A grid thumbnail still being put together: a soft box that breathes.</summary>
    private const float WaitingPulse = 3f;

    /// <summary>What the preview's doing, F stepping through them: its name under it, and how long an attack and a cast take before they go round again.</summary>
    private enum PreviewAction { Standing, Walking, Attacking, Casting }
    private const float AttackSeconds = 1.6f, WindUpSeconds = 0.4f, StrikeSeconds = 0.75f;
    private const float CastSeconds = 2.2f, CastHeldSeconds = 1.4f;

    /// <summary>The grid under the rows: how far below the last row, the gap between its items, the most across it, and its items' frames.</summary>
    private const float DetailTop = 22f;
    private const int DetailGap = 6;
    private const int MostAcross = 14;
    private const int PickedFrame = 3, HoverFrame = 2;

    /// <summary>The smallest a grid's items are drawn, a share of their size; and the room either side of a window of a longer grid, for how many more there are.</summary>
    private const float LeastScale = 0.75f;
    private const int WindowArrows = 56;
    private static readonly Point ChipCell = new(44, 44), FaceCell = new(64, 64), NotchCell = new(56, 22);

    /// <summary>Where the keys for the preview and their scores are written, under them, and the soft dark behind them so they read on the deck.</summary>
    private const float ScoresY = 832f, PreviewKeysY = 862f;
    private static readonly Rectangle UnderThem = new(210, 828, 580, 64);
    private const int UnderThemEnds = 40;

    /// <summary>A colour row's swatch: its size, how far in from the row's right end, and its frame.</summary>
    private static readonly Point ChipSize = new(30, 30);
    private const float ChipInset = 70f;
    private const int ChipFrame = 2;
    private static readonly Color ShadowColor = Color.Black * 0.45f;

    /// <summary>How long they face each way before turning, and how long a step takes.</summary>
    private const float TurnSeconds = 2.2f;
    private const float StepSeconds = 0.14f;

    /// <summary>The way they turn round: down, right, up, left, so it looks like turning on the spot.</summary>
    private static readonly Direction[] Turning = { Direction.Down, Direction.Right, Direction.Up, Direction.Left };

    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.4);

    private readonly InputAction _previousTab = new(new[] { Buttons.LeftShoulder }, new[] { Keys.Q }, true);
    private readonly InputAction _nextTab = new(new[] { Buttons.RightShoulder }, new[] { Keys.E }, true);

    private readonly int _slot;
    private readonly SaveData _game;
    private readonly bool _soulBound;
    private readonly PartyMember _member;
    private readonly Paperdoll _paperdoll = new();
    private readonly FormPanel _panel = new();
    private readonly Camera2D _camera = new(new Point(LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight));
    private RoomScene _deck;
    private Color _deckTint = Color.White;
    private readonly Random _random = new();
    private readonly StringBuilder _name = new();
    private readonly List<(MenuEntry Row, Func<string> Value)> _rows = new();
    private readonly List<List<MenuEntry>> _pages = new() { new(), new(), new(), new() };

    /// <summary>Rows shown only sometimes, and when: the painted figure's with a painted face, the paperdoll's without.</summary>
    private readonly Dictionary<MenuEntry, Func<bool>> _shownWhen = new();

    /// <summary>The swatch each colour row has picked, drawn beside its value: null for as painted.</summary>
    private readonly Dictionary<MenuEntry, Func<Color?>> _chips = new();

    /// <summary>The grid each row shows under the rows while it's picked.</summary>
    private readonly Dictionary<MenuEntry, Detail> _details = new();

    /// <summary>The painted figure on the deck, a walker like the rooms', with its feet on FigureFeet.</summary>
    private readonly WalkerSprite _preview = new() { Facing = Direction.Right };

    /// <summary>The figure being shown, the one it's fading in over (or null), and how far it's faded in.</summary>
    private ImportedFigure _shown, _leaving;
    private float _fade = 1f;

    /// <summary>Where each row on the tab ends up, and how long since the tab's rows came in.</summary>
    private readonly Dictionary<MenuEntry, Vector2> _rowAt = new();
    private float _rowsClock = RowsInSeconds;

    /// <summary>Whether the mouse is on the name box.</summary>
    private bool _onNameBox;

    /// <summary>F: the preview's next action. R: turning it round.</summary>
    private readonly InputAction _nextAction = new(new[] { Buttons.Y }, new[] { Keys.F }, true);
    private readonly InputAction _turnRound = new(new[] { Buttons.X }, new[] { Keys.R }, true);

    private PreviewAction _action;
    private float _actionClock;
    private bool _comparing;

    /// <summary>The row whose grid is showing (the last one picked on this tab that has one), and the item the mouse is on in it, or -1.</summary>
    private MenuEntry _detailRow;
    private int _hovered = -1;

    /// <summary>Where the last row on the tab ends, which the grid goes under.</summary>
    private float _rowsBottom;

    /// <summary>
    /// What a row shows under the rows while it's picked: a grid of the Count things it can be, each
    /// Size and drawn by DrawItem (its box, its number, the alpha), Current framed, and any clicked
    /// picked (Pick); Name is an item's name, for the note while the mouse is on it.
    /// </summary>
    private sealed record Detail(int Count, Func<int> Current, Action<int> Pick, Point Size, Action<SpriteBatch, Rectangle, int, float> DrawItem, Func<int, string> Name);
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

        var card = new DialogueScreen(StoryPrologueAct1.P_01, game, NoFace, new Random());
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

        // CHARACTER: the face, and its skin, hair and eyes. ARMOR: the Base and Trim whatever they wear
        // is dyed, and whether the helm and the cape show. With a painted face they're the painted
        // figure's swatches; with the drawn one, the paperdoll's own rows.
        Func<bool> painted = () => Painted, drawn = () => !Painted;
        if (PartCatalog.PlayerFaces.Count > 0)
        {
            MenuEntry portrait = Adjustable(1, "PORTRAIT", () => PortraitIndex(), v => _member.Look.Portrait = v == 0 ? null : PartCatalog.PlayerFaces[v - 1], PortraitNames());
            portrait.Adjusted += _ => ShowPage(_page, keep: portrait);
            IReadOnlyList<string> portraits = PortraitNames();
            _details[portrait] = new Detail(portraits.Count, PortraitIndex, v =>
            {
                _member.Look.Portrait = v == 0 ? null : PartCatalog.PlayerFaces[v - 1];
                Refresh();
                ShowPage(_page, keep: portrait);
            }, FaceCell, DrawFace, v => portraits[v]);
        }
        HairRow(painted);
        Swatch(1, "SKIN", () => _member.Look.SkinTint, v => _member.Look.SkinTint = v, Swatches.Skin, painted);
        Swatch(1, "HAIR DYE", () => _member.Look.HairTint, v => _member.Look.HairTint = v, Swatches.Hair, painted);
        Swatch(1, "EYES", () => _member.Look.EyeTint, v => _member.Look.EyeTint = v, Swatches.Eyes, painted);
        Slider(1, "HEIGHT", () => _member.Look.HeightStep, v => _member.Look.HeightStep = v,
            CharacterLook.ShortestStep, CharacterLook.TallestStep, CharacterLook.HeightPerStep, painted);
        Slider(1, "BUILD", () => _member.Look.BuildStep, v => _member.Look.BuildStep = v,
            CharacterLook.SlightestStep, CharacterLook.BroadestStep, CharacterLook.BuildPerStep, painted);
        Adjustable(1, "SKIN", () => _member.Look.Skin, v => _member.Look.Skin = v, LookOptions.Skins, drawn);
        Adjustable(1, "FACE", () => _member.Look.Face, v => _member.Look.Face = v, Names(LookOptions.Faces), drawn);
        Adjustable(1, "EYES", () => _member.Look.Eyes, v => _member.Look.Eyes = v, LookOptions.Eyes, drawn);
        Adjustable(1, "HAIR", () => _member.Look.Hair, v => _member.Look.Hair = v, Names(LookOptions.Hair), drawn);
        Adjustable(1, "HAIR COLOUR", () => _member.Look.HairColour, v => _member.Look.HairColour = v, LookOptions.HairColours, drawn);
        Adjustable(1, "BEARD", () => _member.Look.Beard, v => _member.Look.Beard = v, Names(LookOptions.Beards), drawn);

        // FEATURES: what's worn on or about the head, with a painted face: a beard (the man's), earrings,
        // something on the face, marks, a people's ears and horns.
        foreach ((string kind, string label, _, _, _, bool manOnly) in PartCatalog.ExtraKinds)
            ExtraRow(kind, label, manOnly, painted);

        Swatch(3, "BASE", () => _member.Look.BaseTint, v => _member.Look.BaseTint = v, Swatches.Cloth, painted);
        Swatch(3, "TRIM", () => _member.Look.TrimTint, v => _member.Look.TrimTint = v, Swatches.Cloth, painted);
        Adjustable(3, "HELM", () => _member.Look.HideHelm ? 1 : 0, v => _member.Look.HideHelm = v == 1, new[] { "SHOWN", "HIDDEN" }, painted);
        Adjustable(3, "CAPE", () => _member.Look.HideCape ? 1 : 0, v => _member.Look.HideCape = v == 1, new[] { "SHOWN", "HIDDEN" }, painted);
        Adjustable(3, "HAT", () => _member.Look.Hat, v => _member.Look.Hat = v, Names(LookOptions.Hats), drawn);
        Adjustable(3, "SHIRT", () => _member.Look.Shirt, v => _member.Look.Shirt = v, LookOptions.Shirts, drawn);
        Adjustable(3, "VEST", () => _member.Look.Vest, v => _member.Look.Vest = v, LookOptions.Vests, drawn);
        Adjustable(3, "LEGS", () => _member.Look.Legs, v => _member.Look.Legs = v, Names(LookOptions.Legs), drawn);
        Adjustable(3, "LEGS COLOUR", () => _member.Look.LegsColour, v => _member.Look.LegsColour = v, LookOptions.LegsColours, drawn);
        Adjustable(3, "CLOAK", () => _member.Look.Cloak, v => _member.Look.Cloak = v, LookOptions.Cloaks, drawn);

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
        LoadDeck();
        _font = Content.Load<SpriteFont>("spectral-ui");
        _detailFont = Content.Load<SpriteFont>("spectral-detail");
        _glow = Content.Load<Texture2D>("glow");
        _preview.LoadContent(Content);

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
        _deck?.Unload();
        base.Unload();
    }

    /// <summary>The ship's deck behind them: its room, the camera on it so where they stand is under FigureFeet, and its light.</summary>
    private void LoadDeck()
    {
        RoomDefinition room = ZoneCatalog.Get(DeckZone).Room(DeckRoom);
        _deck = new RoomScene(room);
        _deck.LoadContent(ScreenManager.Game.Services, ScreenManager.GraphicsDevice);
        _deckTint = room.Tint;
        _camera.LockTo(_deck.Bounds, (int)ZoneCatalog.RoomScale);
        Vector2 spot = room.Spawns.TryGetValue(DeckSpot, out MapSpawn spawn) ? spawn.Feet : new Vector2(_deck.Bounds.Center.X, ZoneCatalog.RoomHeight * 0.8f);
        float half = LegendQuestGame.ScreenWidth / 2f;
        _camera.SnapTo(new Vector2(spot.X + (half - FigureFeet.X) / ZoneCatalog.RoomScale, ZoneCatalog.RoomHeight / 2f));
    }

    /// <summary>While a name's being typed the menu doesn't move (WASD are letters too); otherwise Q and E change the tab, and the rest is a menu.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_busy) return;

        _onNameBox = NameBox.Contains(input.MousePosition);
        if (_typing)
        {
            if (input.IsNewKeyPress(Keys.Enter) || input.IsNewKeyPress(Keys.Escape) || input.IsNewKeyPress(Keys.Tab)
                || input.IsNewLeftClick && !_onNameBox)
                StopTyping();
            return;
        }
        if (input.IsNewKeyPress(Keys.Tab) || input.IsNewLeftClick && _onNameBox)
        {
            StartTyping();
            Audio?.Play(Sfx.MenuMove);
            return;
        }

        if (_previousTab.Occurred(input)) ShowPage(LookOptions.Wrap(_page - 1, Tabs.Length));
        else if (_nextTab.Occurred(input)) ShowPage(LookOptions.Wrap(_page + 1, Tabs.Length));
        if (_nextAction.Occurred(input) || input.IsNewLeftClick && FigureBox().Contains(input.MousePosition)) NextAction();
        if (_turnRound.Occurred(input)) _preview.Facing = _preview.Facing == Direction.Right ? Direction.Left : Direction.Right;
        _comparing = input.IsKeyPressed(Keys.V);

        // The grid under the rows: the mouse on an item lights it, and a click picks it.
        _hovered = -1;
        if (ShownDetail() is Detail detail)
        {
            Rectangle[] boxes = DetailBoxes(detail).Boxes;
            for (int i = 0; i < boxes.Length; i++)
                if (boxes[i].Contains(input.MousePosition)) _hovered = i;
            if (_hovered >= 0 && input.IsNewLeftClick && _hovered != detail.Current())
            {
                detail.Pick(_hovered);
                Refresh();
                Audio?.Play(Sfx.MenuMove);
            }
        }

        base.HandleInput(gameTime, input);
        if (SelectedEntry < MenuEntries.Count && _details.ContainsKey(MenuEntries[SelectedEntry])) _detailRow = MenuEntries[SelectedEntry];
    }

    /// <summary>F, or a click on them: the preview walks, then attacks, then casts, then stands again.</summary>
    private void NextAction()
    {
        _action = (PreviewAction)(((int)_action + 1) % Enum.GetValues<PreviewAction>().Length);
        _actionClock = 0f;
        Audio?.Play(Sfx.MenuMove);
    }

    /// <summary>Where the preview stands, to click on.</summary>
    private static Rectangle FigureBox() =>
        new((int)FigureFeet.X - PaintedHeight / 3, (int)FigureFeet.Y - PaintedHeight, PaintedHeight * 2 / 3, PaintedHeight);

    /// <summary>The grid showing under the rows, or null: the one for the row last picked on this tab, if it's there.</summary>
    private Detail ShownDetail() =>
        _detailRow is not null && MenuEntries.Contains(_detailRow) && _details.TryGetValue(_detailRow, out Detail detail) ? detail : null;

    /// <summary>
    /// Where each of a grid's items goes, centred under the rows, as even as they'll go: on as many
    /// lines as let its items be biggest, no more than MostAcross full-size on a line, and shrunk when
    /// it has more than fit full-size between the last row and the buttons, the widest as wide as the
    /// rows. It's never shrunk below LeastScale: a grid with more than fit at that size (fifty
    /// hairstyles) shows a window of as many as do, round the one picked, and the rest are empty
    /// boxes (Rectangle.Empty), not drawn. Returns the boxes, and how many are hidden before the
    /// window and after it.
    /// </summary>
    private (Rectangle[] Boxes, int Before, int After) DetailBoxes(Detail detail)
    {
        float top = _rowsBottom + DetailTop;
        float room = ButtonsY - ButtonSize.Y / 2f - DetailGap * 2 - top;
        (int Lines, float Scale) Layout(int count)
        {
            (int Lines, float Scale) best = (1, 0f);
            for (int lines = (count + MostAcross - 1) / MostAcross; lines <= count; lines++)
            {
                int across = (count + lines - 1) / lines;
                float scale = MathF.Min(1f, MathF.Min((RowSize.X - (across - 1) * DetailGap) / (float)(across * detail.Size.X),
                                                      (room - (lines - 1) * DetailGap) / (lines * detail.Size.Y)));
                if (scale > best.Scale + 0.001f) best = (lines, scale);
                if (scale >= 1f) break;
            }
            return best;
        }

        // As many as fit at LeastScale, if not all of them: a window round the one picked.
        int shown = detail.Count, first = 0;
        (int Lines, float Scale) layout = Layout(shown);
        if (layout.Scale < LeastScale)
        {
            int lines = Math.Max(1, (int)((room + DetailGap) / (detail.Size.Y * LeastScale + DetailGap)));
            int across = Math.Max(1, (int)((RowSize.X - WindowArrows * 2 + DetailGap) / (detail.Size.X * LeastScale + DetailGap)));
            shown = Math.Min(detail.Count, lines * across);
            first = Math.Clamp(detail.Current() - shown / 2, 0, detail.Count - shown);
            layout = (lines, LeastScale);
        }
        int perLine = (shown + layout.Lines - 1) / layout.Lines;
        var size = new Point(Math.Max(1, (int)(detail.Size.X * layout.Scale)), Math.Max(1, (int)(detail.Size.Y * layout.Scale)));
        var boxes = new Rectangle[detail.Count];
        for (int k = 0; k < shown; k++)
        {
            int line = k / perLine, column = k % perLine;
            int here = Math.Min(perLine, shown - line * perLine);
            float width = here * size.X + (here - 1) * DetailGap;
            float left = OptionsPanel.Center.X - width / 2f;
            boxes[first + k] = new Rectangle((int)(left + column * (size.X + DetailGap)), (int)(top + line * (size.Y + DetailGap)), size.X, size.Y);
        }
        return (boxes, first, detail.Count - first - shown);
    }

    /// <summary>Runs the clock they walk and turn by, and the pier's sway behind the Soul-Bound.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _clock += seconds;
        _deck?.Update(gameTime);
        UpdateFigure(seconds);
        UpdateRows(seconds);

        _actionClock += seconds;
        _preview.IsWalking = _action == PreviewAction.Walking;
        WalkerPose pose = _action switch
        {
            PreviewAction.Attacking => (_actionClock % AttackSeconds) switch
            {
                < WindUpSeconds => WalkerPose.WindUp,
                < StrikeSeconds => WalkerPose.Strike,
                _ => WalkerPose.None,
            },
            PreviewAction.Casting => _actionClock % CastSeconds < CastHeldSeconds ? WalkerPose.Cast : WalkerPose.None,
            _ => WalkerPose.None,
        };
        if (pose != _preview.Pose)
        {
            if (pose == WalkerPose.None) _preview.Release();
            else _preview.Play(pose);
        }
        _preview.Update(gameTime);
    }

    /// <summary>
    /// The figure to show: the one for how they look now if it's been put together, faded in over the
    /// last; till it is, the last one stays.
    /// </summary>
    private void UpdateFigure(float seconds)
    {
        Outfit outfit = PartCatalog.OutfitOf(_member);
        if (_comparing && outfit is not null) outfit = outfit with { Tints = default };
        if (PartCatalog.LargeWhenReady(outfit, PaintedHeight) is ImportedFigure ready && ready != _shown)
        {
            _leaving = _shown;
            _shown = ready;
            _fade = _leaving is null ? 1f : 0f;
        }
        _fade = MathF.Min(1f, _fade + seconds / FigureFadeSeconds);
        if (_fade >= 1f) _leaving = null;
        _preview.Art = _shown is { HasBody: true } ? _shown : null;
    }

    /// <summary>The tab's rows rising into place and fading in, eased.</summary>
    private void UpdateRows(float seconds)
    {
        _rowsClock = MathF.Min(RowsInSeconds, _rowsClock + seconds);
        float rise = (1f - Eased(_rowsClock / RowsInSeconds)) * RowsRise;
        foreach ((MenuEntry row, Vector2 at) in _rowAt) row.Plate.Center = at + new Vector2(0f, rise);
    }

    /// <summary>A smooth ease out, 0 to 1.</summary>
    private static float Eased(float t)
    {
        t = MathHelper.Clamp(t, 0f, 1f);
        return 1f - (1f - t) * (1f - t) * (1f - t);
    }

    /// <summary>Whether they're shown as their painted figure (a painted face that makes one), not the paperdoll.</summary>
    private bool ShowsPainted => Painted && !PartCatalog.CantMake(PartCatalog.OutfitOf(_member), PaintedHeight);

    /// <summary>Draws the deck behind them, the card, them, the panel, the headings and the note, then the rows.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        // 1. The ship's deck, its sky and sea and lanterns moving, dimmed a little so they stand out.
        if (_deck is not null)
        {
            Color tint = Color.Lerp(Color.Black, _deckTint, alpha);
            _deck.DrawBack(gameTime, spriteBatch, _camera, tint);
            _deck.DrawFront(spriteBatch, _camera, tint);
        }
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        spriteBatch.Draw(ScreenManager.BlankTexture, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight), DeckVeil * alpha);
        spriteBatch.End();

        // 2. Them, standing on the deck, the shadow under them (the glow squashed flat, so smoothed).
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        spriteBatch.Draw(_glow, new Rectangle((int)FigureFeet.X - ShadowSize.X / 2, (int)FigureFeet.Y - ShadowSize.Y / 2,
            ShadowSize.X, ShadowSize.Y), ShadowColor * alpha);
        spriteBatch.End();
        if (ShowsPainted) DrawPreview(spriteBatch, alpha);
        else
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawFigure(spriteBatch, alpha);
            spriteBatch.End();
        }

        // 3. The card, with their face and the name box, and the panel down the right.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _panel.Draw(spriteBatch, NameCard, alpha);
        _panel.Draw(spriteBatch, OptionsPanel, alpha);
        DrawBust(spriteBatch, alpha);
        DrawNameBox(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        string heading = _soulBound ? SoulBoundHeading : YouHeading;
        Text.Draw(spriteBatch, _font, heading, OptionsPanel.Center.X, OptionsPanel.Top + FormPanel.Padding,
            Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true, width: OptionsPanel.Width - FormPanel.Padding * 2f);
        DrawName(spriteBatch, alpha);
        Text.Draw(spriteBatch, _detailFont, "NAME", NameLabelAt.X, NameLabelAt.Y, Palette.Note * alpha, Palette.FormShadow * alpha);
        Text.Draw(spriteBatch, _detailFont, Summary(), NameBox.X + 2f, NameBox.Bottom + SummaryGap,
            Palette.Bone * alpha, Palette.FormShadow * alpha, width: NameBox.Width);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        Text.DrawBacking(spriteBatch, _glow, UnderThem, UnderThemEnds, Palette.Ink * (0.7f * alpha));
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);

        // Over the deck between the card and the panel: the Soul-Bound's line. Under them, their scores and the preview's keys.
        if (_soulBound)
            Text.Draw(spriteBatch, _detailFont, SoulBoundLine, (NameCard.Right + OptionsPanel.Left) / 2f, NameCard.Top + 8f,
                Palette.Narration * alpha, Palette.FormShadow * alpha, centred: true, width: OptionsPanel.Left - NameCard.Right - 24f);
        Text.Draw(spriteBatch, _detailFont, Scores(), FigureFeet.X, ScoresY,
            Palette.Bone * alpha, Palette.FormShadow * alpha, centred: true);
        if (_preview.DrawsArt)
            Text.Draw(spriteBatch, _detailFont, PreviewKeys(), FigureFeet.X, PreviewKeysY,
                Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, About(), OptionsPanel.Center.X, ButtonsY - ButtonSize.Y / 2f - 18f - _detailFont.LineSpacing,
            Palette.Narration * alpha, Palette.FormShadow * alpha, centred: true, width: RowSize.X);

        string note = _status ?? (_typing ? TypingNote : HoveredName() ?? Note);
        Text.Draw(spriteBatch, _detailFont, note, OptionsPanel.Center.X, _noteY,
            (_status is null ? Palette.Note : Palette.Ember) * alpha, Palette.FormShadow * alpha, centred: true, width: RowSize.X);
        spriteBatch.End();

        // The rows, a tab's coming in faded as they rise into place; the tabs and buttons as they are.
        float rowsIn = Eased(_rowsClock / RowsInSeconds);
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        foreach (MenuEntry entry in MenuEntries)
        {
            entry.Plate.Opacity = alpha * (_rowAt.ContainsKey(entry) ? rowsIn : 1f);
            entry.Plate.Draw(gameTime, spriteBatch);
        }
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        DrawChips(spriteBatch, alpha);
        DrawDetail(spriteBatch, alpha);
        spriteBatch.End();
    }

    /// <summary>
    /// Their painted figure on the deck: the walker, put together big in what they're wearing and dyed
    /// as they are (as painted while V's held), drawn at a room's scale so it moves as it will there;
    /// a new one fading in over the last. Nothing till the first is put together.
    /// </summary>
    private void DrawPreview(SpriteBatch spriteBatch, float alpha)
    {
        if (_shown is not { HasBody: true }) return;

        float scale = ZoneCatalog.RoomScale;
        _preview.Position = FigureFeet / scale;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, transformMatrix: Matrix.CreateScale(scale, scale, 1f));
        if (_leaving is { HasBody: true } leaving)
        {
            _preview.Art = leaving;
            _preview.Opacity = alpha * (1f - _fade);
            _preview.Draw(spriteBatch, 0.5f, 1f / scale, Color.White);
        }
        _preview.Art = _shown;
        _preview.Opacity = alpha * (_leaving is null ? 1f : _fade);
        _preview.Draw(spriteBatch, 0.5f, 1f / scale, Color.White);
        spriteBatch.End();
    }

    /// <summary>The name box: a dark well, framed in gold while it's being typed in or the mouse is on it.</summary>
    private void DrawNameBox(SpriteBatch spriteBatch, float alpha)
    {
        spriteBatch.Draw(ScreenManager.BlankTexture, NameBox, Palette.Ink * (0.92f * alpha));
        Color frame = _typing ? Palette.Gold : _onNameBox ? Palette.Bone : Palette.Note * 0.7f;
        Frame(spriteBatch, NameBox, _typing ? 3 : 2, frame * alpha);
    }

    /// <summary>The name in its box, a blinking caret after it while it's being typed; the placeholder in it while it's empty.</summary>
    private void DrawName(SpriteBatch spriteBatch, float alpha)
    {
        string name = Typed();
        float y = NameBox.Center.Y - _font.LineSpacing / 2f;
        float x = NameBox.X + NameInset;
        if (name.Length == 0 && !_typing)
        {
            Text.Draw(spriteBatch, _detailFont, NamePlaceholder, x, NameBox.Center.Y - _detailFont.LineSpacing / 2f,
                Palette.Note * alpha, Color.Transparent);
            return;
        }
        Text.Draw(spriteBatch, _font, name, x, y, Palette.Heading * alpha, Palette.FormShadow * alpha);
        if (_typing && (int)(_clock / CaretSeconds) % 2 == 0)
        {
            float width = _font.MeasureString(name).X;
            spriteBatch.Draw(ScreenManager.BlankTexture, new Rectangle((int)(x + width + 3f), NameBox.Y + 11, CaretWidth, NameBox.Height - 22), Palette.Gold * alpha);
        }
    }

    /// <summary>The preview's keys under it: what F does next, turning, and comparing.</summary>
    private string PreviewKeys() =>
        "F  " + _action.ToString().ToUpperInvariant() + "   ·   R  TURN   ·   V  " + (_comparing ? "AS PAINTED" : "COMPARE");

    /// <summary>The name of the item the mouse is on in the grid, for the note; null if it's on none.</summary>
    private string HoveredName() => _hovered >= 0 && ShownDetail() is Detail detail ? "CLICK  ·  " + detail.Name(_hovered) : null;

    /// <summary>The grid under the rows: each item, the one picked framed bright and the one the mouse is on framed pale.</summary>
    private void DrawDetail(SpriteBatch spriteBatch, float alpha)
    {
        if (ShownDetail() is not Detail detail) return;
        (Rectangle[] boxes, int before, int after) = DetailBoxes(detail);
        int current = detail.Current();
        Rectangle shown = Rectangle.Empty;
        for (int i = 0; i < boxes.Length; i++)
        {
            Rectangle box = boxes[i];
            if (box.IsEmpty) continue;
            shown = shown.IsEmpty ? box : Rectangle.Union(shown, box);
            spriteBatch.Draw(ScreenManager.BlankTexture, box, Palette.FormShadow * alpha);
            detail.DrawItem(spriteBatch, new Rectangle(box.X + ChipFrame, box.Y + ChipFrame, box.Width - ChipFrame * 2, box.Height - ChipFrame * 2), i, alpha);
            if (i == current) Frame(spriteBatch, box, PickedFrame, Palette.Torch * alpha);
            else if (i == _hovered) Frame(spriteBatch, box, HoverFrame, Palette.Bone * alpha);
        }
        // A window of a longer grid: how many more there are either side of it.
        float middle = shown.Center.Y - _detailFont.LineSpacing / 2f;
        if (before > 0)
            Text.Draw(spriteBatch, _detailFont, "< " + before, shown.Left - WindowArrows / 2f, middle,
                Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        if (after > 0)
            Text.Draw(spriteBatch, _detailFont, after + " >", shown.Right + WindowArrows / 2f, middle,
                Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
    }

    /// <summary>A frame round a box, drawn inside it.</summary>
    private void Frame(SpriteBatch spriteBatch, Rectangle box, int thickness, Color colour)
    {
        Texture2D blank = ScreenManager.BlankTexture;
        spriteBatch.Draw(blank, new Rectangle(box.X, box.Y, box.Width, thickness), colour);
        spriteBatch.Draw(blank, new Rectangle(box.X, box.Bottom - thickness, box.Width, thickness), colour);
        spriteBatch.Draw(blank, new Rectangle(box.X, box.Y, thickness, box.Height), colour);
        spriteBatch.Draw(blank, new Rectangle(box.Right - thickness, box.Y, thickness, box.Height), colour);
    }

    /// <summary>A face in the PORTRAIT grid: the paperdoll's for DRAWN, then each painted face as it is.</summary>
    /// <summary>
    /// The HAIR row: what's on their head when nothing's worn there, hair and helm being one slot.
    /// Their face's own hair (the one painted with it), none, or any of the painted player hairs;
    /// its grid shows them wearing each.
    /// </summary>
    private void HairRow(Func<bool> painted)
    {
        IReadOnlyList<string> hairs = PartCatalog.PlayerHairs;
        var names = new List<string> { "OWN HAIR", "BALD" };
        foreach (string hair in hairs)
            names.Add(PartCatalog.NameOf(hair) ?? "HAIR " + int.Parse(hair["hair-".Length..], System.Globalization.CultureInfo.InvariantCulture));
        int Get() => _member.Look.HairPart switch { null => 0, "" => 1, string hair => 2 + Math.Max(0, IndexOf(hairs, hair)) };
        void Set(int v) => _member.Look.HairPart = v == 0 ? null : v == 1 ? string.Empty : hairs[v - 2];
        MenuEntry row = Adjustable(1, "HAIR", Get, Set, names, painted);
        _details[row] = new Detail(names.Count, Get, Set, FaceCell, (batch, box, index, alpha) =>
        {
            CharacterLook look = _member.Look;
            string kept = look.HairPart;
            look.HairPart = index == 0 ? null : index == 1 ? string.Empty : hairs[index - 2];
            bool hidden = look.HideHelm;
            look.HideHelm = true;
            Outfit outfit = PartCatalog.OutfitOf(_member);
            look.HairPart = kept;
            look.HideHelm = hidden;
            DrawThumbnail(batch, outfit, box, alpha);
        }, index => names[index]);
    }

    /// <summary>
    /// A FEATURES row: one kind of extra (PartCatalog.ExtraKinds), none or any painted one of it, by
    /// its card's name; its grid shows them wearing each. A man's-only kind (a beard) shows only with
    /// a man's face.
    /// </summary>
    private void ExtraRow(string kind, string label, bool manOnly, Func<bool> painted)
    {
        IReadOnlyList<string> extras = PartCatalog.PlayerExtras(kind);
        if (extras.Count == 0) return;
        var names = new List<string> { "NONE" };
        foreach (string id in extras) names.Add(PartCatalog.NameOf(id) ?? id.ToUpperInvariant());
        int Get() => _member.Look.Extras?.GetValueOrDefault(kind) is { Length: > 0 } id ? 1 + Math.Max(0, IndexOf(extras, id)) : 0;
        void Set(int v) => (_member.Look.Extras ??= new Dictionary<string, string>())[kind] = v == 0 ? string.Empty : extras[v - 1];
        Func<bool> shown = manOnly ? () => painted() && OnMan : painted;
        MenuEntry row = Adjustable(2, label, Get, Set, names, shown);
        _details[row] = new Detail(names.Count, Get, Set, FaceCell, (batch, box, index, alpha) =>
        {
            CharacterLook look = _member.Look;
            Dictionary<string, string> kept = look.Extras;
            look.Extras = kept is null ? new Dictionary<string, string>() : new Dictionary<string, string>(kept);
            look.Extras[kind] = index == 0 ? string.Empty : extras[index - 1];
            Outfit outfit = PartCatalog.OutfitOf(_member);
            look.Extras = kept;
            DrawThumbnail(batch, outfit, box, alpha);
        }, index => names[index]);
    }

    /// <summary>Whether their painted face is one of the men's (a beard is the man's).</summary>
    private bool OnMan => _member.Look.Portrait is string face && face.StartsWith("face-m", StringComparison.Ordinal);

    private void DrawFace(SpriteBatch spriteBatch, Rectangle box, int index, float alpha)
    {
        if (index == 0)
        {
            if (_sheet is not null) spriteBatch.Draw(_sheet, box, Bust, Color.White * alpha);
            return;
        }
        DrawThumbnail(spriteBatch, PartCatalog.OutfitForFace(_member.Id, PartCatalog.PlayerFaces[index - 1]), box, alpha);
    }

    /// <summary>A grid's thumbnail of an outfit if it's been put together; till then a soft box that breathes.</summary>
    private void DrawThumbnail(SpriteBatch spriteBatch, Outfit outfit, Rectangle box, float alpha)
    {
        if (PartCatalog.ThumbnailWhenReady(outfit, box.Size) is Texture2D thumbnail)
        {
            spriteBatch.Draw(thumbnail, box, Color.White * alpha);
            return;
        }
        float breath = 0.12f + 0.08f * MathF.Sin(_clock * WaitingPulse);
        spriteBatch.Draw(ScreenManager.BlankTexture, box, Palette.Bone * (breath * alpha));
    }

    /// <summary>A swatch in a colour grid: its colour, or for AS PAINTED an empty one struck through.</summary>
    private void DrawSwatch(SpriteBatch spriteBatch, Rectangle box, Color? colour, float alpha)
    {
        if (colour is Color c)
        {
            spriteBatch.Draw(ScreenManager.BlankTexture, box, c * alpha);
            return;
        }
        spriteBatch.Draw(ScreenManager.BlankTexture, box, Palette.Note * (0.25f * alpha));
        float across = MathF.Sqrt(box.Width * box.Width + box.Height * box.Height);
        spriteBatch.Draw(ScreenManager.BlankTexture, box.Center.ToVector2(), null, Palette.Ember * alpha, -MathF.Atan2(box.Height, box.Width),
            new Vector2(0.5f, 0.5f), new Vector2(across - 6f, 3f), SpriteEffects.None, 0f);
    }

    /// <summary>Each colour row's swatch at its right end, framed: the colour picked, or an empty frame for as painted.</summary>
    private void DrawChips(SpriteBatch spriteBatch, float alpha)
    {
        foreach (MenuEntry row in MenuEntries)
        {
            if (!_chips.TryGetValue(row, out Func<Color?> chip)) continue;
            var box = new Rectangle((int)(row.Plate.Center.X + RowSize.X / 2f - ChipInset), (int)(row.Plate.Center.Y - ChipSize.Y / 2f), ChipSize.X, ChipSize.Y);
            spriteBatch.Draw(ScreenManager.BlankTexture, box, Palette.FormShadow * alpha);
            var inside = new Rectangle(box.X + ChipFrame, box.Y + ChipFrame, box.Width - ChipFrame * 2, box.Height - ChipFrame * 2);
            spriteBatch.Draw(ScreenManager.BlankTexture, inside, (chip() is Color colour ? colour : Palette.Note * 0.35f) * alpha);
        }
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
        ShowPage(_page);
    }

    /// <summary>For the proof run: the Soul-Bound's pronouns, as if they'd been picked on their row.</summary>
    internal void ProofPronouns(Pronouns pronouns)
    {
        _member.Pronouns = pronouns;
        Refresh();
    }

    /// <summary>For the proof run: one of the tabs.</summary>
    internal void ProofPage(int page) => ShowPage(page);

    /// <summary>For the proof run: a height and a build, as if stepped to on their rows, and the tab's row picked whose grid shows.</summary>
    /// <param name="height">The height's step.</param>
    /// <param name="build">The build's step.</param>
    /// <param name="row">Which row of the tab, from the top.</param>
    internal void ProofShape(int height, int build, int row)
    {
        _member.Look.HeightStep = height;
        _member.Look.BuildStep = build;
        Refresh();
        SelectedEntry = _tabs.Count + row;
        if (_details.ContainsKey(MenuEntries[SelectedEntry])) _detailRow = MenuEntries[SelectedEntry];
    }

    /// <summary>For the proof run: the preview walking (1), attacking (2) or casting (3), as if F had been pressed till it was.</summary>
    internal void ProofPreview(int action)
    {
        _action = (PreviewAction)action;
        _actionClock = 0f;
    }

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

    /// <summary>A row that steps through a list, wrapping round, shown always or only when `shown` says.</summary>
    private MenuEntry Adjustable(int page, string label, Func<int> get, Action<int> set, IReadOnlyList<string> names, Func<bool> shown = null)
    {
        MenuEntry row = AddRow(page, label, () => names[LookOptions.Wrap(get(), names.Count)]);
        row.Adjusted += step =>
        {
            set(LookOptions.Wrap(get() + step, names.Count));
            Refresh();
        };
        if (shown is not null) _shownWhen[row] = shown;
        return row;
    }

    /// <summary>A row of swatches: AS PAINTED, then each in the list, with the one picked drawn beside its name.</summary>
    private void Swatch(int page, string label, Func<int> get, Action<int> set, IReadOnlyList<Swatches.Swatch> list, Func<bool> shown)
    {
        var names = new string[list.Count + 1];
        for (int i = 0; i < names.Length; i++) names[i] = Swatches.NameOf(list, i);
        MenuEntry row = Adjustable(page, label, get, set, names, shown);
        _chips[row] = () => Swatches.Pick(list, get());
        _details[row] = new Detail(names.Length, get, set, ChipCell,
            (batch, box, index, alpha) => DrawSwatch(batch, box, Swatches.Pick(list, index), alpha), index => names[index]);
    }

    /// <summary>
    /// A row that steps a number between two ends, not wrapping round, written as a share of the
    /// recipe's (100% in the middle); its grid is a notch for each step, lit up to the one it's on.
    /// </summary>
    private void Slider(int page, string label, Func<int> get, Action<int> set, int least, int most, float perStep, Func<bool> shown)
    {
        string Name(int step) => ((int)MathF.Round((1f + perStep * step) * 100f)).ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";
        MenuEntry row = AddRow(page, label, () => Name(Math.Clamp(get(), least, most)));
        row.Adjusted += step =>
        {
            set(Math.Clamp(get() + step, least, most));
            Refresh();
        };
        _shownWhen[row] = shown;
        _details[row] = new Detail(most - least + 1, () => Math.Clamp(get(), least, most) - least, v => set(least + v), NotchCell,
            (batch, box, index, alpha) =>
            {
                int on = Math.Clamp(get(), least, most) - least, middle = -least;
                bool lit = index >= Math.Min(on, middle) && index <= Math.Max(on, middle);
                batch.Draw(ScreenManager.BlankTexture, box, (lit ? Palette.Torch * 0.75f : Palette.Note * 0.2f) * alpha);
                if (index == middle) batch.Draw(ScreenManager.BlankTexture, new Rectangle(box.Center.X - 1, box.Y, 2, box.Height), Palette.Bone * alpha);
            }, index => Name(least + index));
    }

    /// <summary>One of the buttons along the bottom.</summary>
    private void AddButton(string label, Color accent, Action picked)
    {
        var button = new MenuEntry(label, accent, ButtonSize);
        button.Selected += picked;
        _buttons.Add(button);
    }

    /// <summary>
    /// Shows one tab's rows that are shown now, laid down the panel, with the tabs above them and the
    /// buttons below. The other tabs' rows, and this one's that aren't shown, are left out.
    /// </summary>
    /// <param name="page">The tab.</param>
    /// <param name="keep">A row to keep selected, when its change shows or hides others; otherwise the first row is.</param>
    private void ShowPage(int page, MenuEntry keep = null)
    {
        if (page != _page || _rowAt.Count == 0) _rowsClock = 0f;
        _page = page;
        MenuEntries.Clear();
        _rowAt.Clear();
        foreach (MenuEntry tab in _tabs) MenuEntries.Add(tab);
        float y = RowsTop;
        foreach (MenuEntry row in _pages[page])
        {
            if (_shownWhen.TryGetValue(row, out Func<bool> shown) && !shown()) continue;
            MenuEntries.Add(row);
            _rowAt[row] = new Vector2(OptionsPanel.Center.X, y + RowSize.Y / 2f);
            y += RowSize.Y + RowGap;
        }
        UpdateRows(0f);
        _rowsBottom = y - RowGap;
        foreach (MenuEntry button in _buttons) MenuEntries.Add(button);

        for (int i = 0; i < _tabs.Count; i++) _tabs[i].Plate.Accent = i == page ? Palette.Torch : Palette.Bone;
        int kept = keep is null ? -1 : MenuEntries.IndexOf(keep);
        SelectedEntry = kept >= 0 ? kept : _tabs.Count;

        // The grid under the rows starts as the picked row's, or the tab's first that has one.
        _detailRow = null;
        foreach (MenuEntry entry in MenuEntries)
        {
            if (!_details.ContainsKey(entry)) continue;
            if (_detailRow is null || entry == keep) _detailRow = entry;
        }
    }

    /// <summary>Whether they've a painted face, so the painted figure's rows are the ones shown.</summary>
    private bool Painted => _member.Look.Portrait is string face && PartCatalog.Has(face);

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
        _member.PutOn(Origins.KitFor(_member.Id, id));
        _member.Hp = _member.MaxHp;
        Refresh();
    }

    /// <summary>RANDOM: a whole new look, a painted face among it if there are any, about half its colours as painted and the rest from the swatches. The name and class stay.</summary>
    private void Randomise()
    {
        CharacterLook look = LookOptions.Random(_random);
        IReadOnlyList<string> faces = PartCatalog.PlayerFaces;
        if (faces.Count > 0) look.Portrait = faces[_random.Next(faces.Count)];
        int Maybe(int count) => _random.Next(2) == 0 ? 0 : _random.Next(1, count + 1);
        IReadOnlyList<string> hairs = PartCatalog.PlayerHairs;
        if (hairs.Count > 0) look.HairPart = hairs[_random.Next(hairs.Count)];
        // A few extras, now and then: a people's ears and horns are theirs to pick, not chance's.
        look.Extras = new Dictionary<string, string>();
        bool man = look.Portrait is string face && face.StartsWith("face-m", StringComparison.Ordinal);
        foreach ((string kind, _, _, _, _, bool manOnly) in PartCatalog.ExtraKinds)
        {
            IReadOnlyList<string> extras = PartCatalog.PlayerExtras(kind);
            double chance = kind switch { "beard" => 0.45, "earrings" => 0.3, "face" => 0.12, "piercings" => 0.08, "marks" => 0.15, _ => 0.0 };
            look.Extras[kind] = extras.Count > 0 && (man || !manOnly) && _random.NextDouble() < chance ? extras[_random.Next(extras.Count)] : string.Empty;
        }
        look.HairTint = Maybe(Swatches.Hair.Count);
        look.EyeTint = Maybe(Swatches.Eyes.Count);
        look.BaseTint = Maybe(Swatches.Cloth.Count);
        look.TrimTint = Maybe(Swatches.Cloth.Count);
        look.HeightStep = _random.Next(CharacterLook.ShortestStep, CharacterLook.TallestStep + 1);
        look.BuildStep = _random.Next(CharacterLook.SlightestStep, CharacterLook.BroadestStep / 2 + 1);
        _member.Look = look;
        Refresh();
        ShowPage(_page, keep: MenuEntries.Count > 0 ? MenuEntries[Math.Min(SelectedEntry, MenuEntries.Count - 1)] : null);
    }

    /// <summary>Where their painted face is on the PORTRAIT row: 0 for the paperdoll's, then each of PartCatalog.PlayerFaces.</summary>
    private int PortraitIndex()
    {
        int index = _member.Look.Portrait is string face ? IndexOf(PartCatalog.PlayerFaces, face) : -1;
        return index + 1;
    }

    /// <summary>The PORTRAIT row's names: the paperdoll's face, then each painted one by its number.</summary>
    private static IReadOnlyList<string> PortraitNames()
    {
        var names = new List<string> { "DRAWN" };
        foreach (string face in PartCatalog.PlayerFaces)
            names.Add((face[5] == 'm' ? "MAN " : "WOMAN ") + int.Parse(face[6..], System.Globalization.CultureInfo.InvariantCulture));
        return names;
    }

    private static int IndexOf(IReadOnlyList<string> list, string value)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] == value) return i;
        return -1;
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

        DialogueScript scene = _soulBound ? StoryPrologueAct1.P_03 : StoryPrologueAct1.P_02;
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

        LoadingScreen.Load(ScreenManager, ZoneCatalog.Get(_game.ZoneId).Name.ToUpperInvariant(), new ExploreScreen(_game));
    }

    /// <summary>Who speaks before the ship: the narrator, with no face, and the cast by name alone (Corra on the pier), since there's no walker to take a face off yet.</summary>
    private static (string Name, Texture2D Sheet, ImportedFigure Art) NoFace(string id) => (Cast.Get(id)?.Name ?? string.Empty, null, null);

    /// <summary>Bakes them again and writes every row's value onto its plate.</summary>
    private void Refresh()
    {
        foreach ((MenuEntry row, Func<string> value) in _rows) row.Text = value();

        if (ScreenManager is null || _font is null) return;
        _sheet?.Dispose();
        _sheet = _paperdoll.Compose(ScreenManager.GraphicsDevice, _member.Look, Paperdoll.GearOf(_member));
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

        width = _buttons.Count * ButtonSize.X + (_buttons.Count - 1) * ButtonGap;
        left = OptionsPanel.Center.X - width / 2f;
        foreach (MenuEntry button in _buttons)
        {
            button.Plate.Center = new Vector2(left + ButtonSize.X / 2f, ButtonsY);
            left += ButtonSize.X + ButtonGap;
        }
        _noteY = ButtonsY + ButtonSize.Y / 2f + 10f;
    }

    /// <summary>The paperdoll, full length, walking on the spot and turning round: for someone with no painted body (DrawPreview draws one who has).</summary>
    private void DrawFigure(SpriteBatch spriteBatch, float alpha)
    {
        if (_sheet is null) return;

        Direction facing = Turning[(int)(_clock / TurnSeconds) % Turning.Length];
        int frame = 1 + (int)(_clock / StepSeconds) % 4;
        var source = new Rectangle(frame * 32, (int)facing * 48, 32, 48);
        spriteBatch.Draw(_sheet, FigureFeet, source, Color.White * alpha, 0f, new Vector2(16f, 46f), FigureScale, SpriteEffects.None, 0f);
    }

    /// <summary>Their face in its well on the card: the painted one as their figure has it, or the paperdoll's blown up.</summary>
    private void DrawBust(SpriteBatch spriteBatch, float alpha)
    {
        spriteBatch.Draw(ScreenManager.BlankTexture, PortraitWell, Palette.Ink * (0.92f * alpha));
        Frame(spriteBatch, PortraitWell, 2, Palette.Gold * (0.8f * alpha));
        var inside = new Rectangle(PortraitWell.X + 4, PortraitWell.Y + 4, PortraitWell.Width - 8, PortraitWell.Height - 8);
        if (ShowsPainted)
        {
            if (_shown is not null) spriteBatch.Draw(_shown.Portrait(inside.Size), inside, Color.White * alpha);
            return;
        }
        if (_sheet is null) return;
        var size = new Vector2(Bust.Width * BustScale, Bust.Height * BustScale);
        spriteBatch.Draw(_sheet, inside.Center.ToVector2() - size / 2f, Bust, Color.White * alpha, 0f, Vector2.Zero, BustScale, SpriteEffects.None, 0f);
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
