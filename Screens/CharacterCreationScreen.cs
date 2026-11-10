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
/// turning round on the left as it changes. It's the Prologue's first three scenes as well.
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
/// They stand on the left as the rooms will draw them, in what their class starts wearing: a walker
/// with their painted figure, put together big (PartCatalog.Large), that breathes, walks, attacks
/// and casts (F), turns round (R), and shows its colours as painted while V's held. Without a
/// painted face the paperdoll stands there instead, turning, with its own rows on the tabs. The
/// scores are the class's recommended spread for now; point buy comes with the character sheet.
/// </para>
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
    private static readonly string[] Tabs = { "WHO", "CHARACTER", "ARMOR" };

    /// <summary>The longest a name can be. Long enough for most, short enough to fit on a party card.</summary>
    private const int NameLength = 12;

    /// <summary>The two panels: the one they're shown on, and the one with the rows.</summary>
    private static readonly Rectangle PreviewPanel = new(40, 30, 660, 840);
    private static readonly Rectangle OptionsPanel = new(720, 30, 840, 840);

    /// <summary>The tabs along the top of the rows, the rows, and the three buttons along the bottom.</summary>
    private static readonly Point TabSize = new(220, 54);
    private const float TabGap = 16f;
    private const float TabsY = 150f;
    private static readonly Point RowSize = new(720, 50);
    private const float RowGap = 8f;
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

    /// <summary>How tall a standard adult's painted figure stands in the preview, in screen pixels.</summary>
    private const int PaintedHeight = 400;

    /// <summary>What the preview's doing, F stepping through them: its name under it, and how long an attack and a cast take before they go round again.</summary>
    private enum PreviewAction { Standing, Walking, Attacking, Casting }
    private const float AttackSeconds = 1.6f, WindUpSeconds = 0.4f, StrikeSeconds = 0.75f;
    private const float CastSeconds = 2.2f, CastHeldSeconds = 1.4f;

    /// <summary>The grid under the rows: how far below the last row, the gap between its items, the most across it, and its items' frames.</summary>
    private const float DetailTop = 22f;
    private const int DetailGap = 6;
    private const int MostAcross = 14;
    private const int PickedFrame = 3, HoverFrame = 2;
    private static readonly Point ChipCell = new(44, 44), FaceCell = new(64, 64), NotchCell = new(56, 22);

    /// <summary>Where the keys for the preview are written, up from the bottom of its panel.</summary>
    private const float PreviewKeysUp = 66f;

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

    /// <summary>Rows shown only sometimes, and when: the painted figure's with a painted face, the paperdoll's without.</summary>
    private readonly Dictionary<MenuEntry, Func<bool>> _shownWhen = new();

    /// <summary>The swatch each colour row has picked, drawn beside its value: null for as painted.</summary>
    private readonly Dictionary<MenuEntry, Func<Color?>> _chips = new();

    /// <summary>The grid each row shows under the rows while it's picked.</summary>
    private readonly Dictionary<MenuEntry, Detail> _details = new();

    /// <summary>The painted figure on the left, a walker like the rooms', with its feet on FigureFeet.</summary>
    private readonly WalkerSprite _preview = new() { Facing = Direction.Right };

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

        Swatch(2, "BASE", () => _member.Look.BaseTint, v => _member.Look.BaseTint = v, Swatches.Cloth, painted);
        Swatch(2, "TRIM", () => _member.Look.TrimTint, v => _member.Look.TrimTint = v, Swatches.Cloth, painted);
        Adjustable(2, "HELM", () => _member.Look.HideHelm ? 1 : 0, v => _member.Look.HideHelm = v == 1, new[] { "SHOWN", "HIDDEN" }, painted);
        Adjustable(2, "CAPE", () => _member.Look.HideCape ? 1 : 0, v => _member.Look.HideCape = v == 1, new[] { "SHOWN", "HIDDEN" }, painted);
        Adjustable(2, "HAT", () => _member.Look.Hat, v => _member.Look.Hat = v, Names(LookOptions.Hats), drawn);
        Adjustable(2, "SHIRT", () => _member.Look.Shirt, v => _member.Look.Shirt = v, LookOptions.Shirts, drawn);
        Adjustable(2, "VEST", () => _member.Look.Vest, v => _member.Look.Vest = v, LookOptions.Vests, drawn);
        Adjustable(2, "LEGS", () => _member.Look.Legs, v => _member.Look.Legs = v, Names(LookOptions.Legs), drawn);
        Adjustable(2, "LEGS COLOUR", () => _member.Look.LegsColour, v => _member.Look.LegsColour = v, LookOptions.LegsColours, drawn);
        Adjustable(2, "CLOAK", () => _member.Look.Cloak, v => _member.Look.Cloak = v, LookOptions.Cloaks, drawn);

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
        if (_nextAction.Occurred(input) || input.IsNewLeftClick && FigureBox().Contains(input.MousePosition)) NextAction();
        if (_turnRound.Occurred(input)) _preview.Facing = _preview.Facing == Direction.Right ? Direction.Left : Direction.Right;
        _comparing = input.IsKeyPressed(Keys.V);

        // The grid under the rows: the mouse on an item lights it, and a click picks it.
        _hovered = -1;
        if (ShownDetail() is Detail detail)
        {
            Rectangle[] boxes = DetailBoxes(detail);
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
    /// it has more than fit full-size between the last row and the buttons (twenty faces, or the
    /// whole hair library), the widest as wide as the rows.
    /// </summary>
    private Rectangle[] DetailBoxes(Detail detail)
    {
        float top = _rowsBottom + DetailTop;
        float room = ButtonsY - ButtonSize.Y / 2f - DetailGap * 2 - top;
        (int Lines, float Scale) best = (1, 0f);
        for (int lines = (detail.Count + MostAcross - 1) / MostAcross; lines <= detail.Count; lines++)
        {
            int count = (detail.Count + lines - 1) / lines;
            float scale = MathF.Min(1f, MathF.Min((RowSize.X - (count - 1) * DetailGap) / (float)(count * detail.Size.X),
                                                  (room - (lines - 1) * DetailGap) / (lines * detail.Size.Y)));
            if (scale > best.Scale + 0.001f) best = (lines, scale);
            if (scale >= 1f) break;
        }
        int across = (detail.Count + best.Lines - 1) / best.Lines;
        var size = new Point(Math.Max(1, (int)(detail.Size.X * best.Scale)), Math.Max(1, (int)(detail.Size.Y * best.Scale)));
        var boxes = new Rectangle[detail.Count];
        for (int i = 0; i < detail.Count; i++)
        {
            int line = i / across, column = i % across;
            int here = Math.Min(across, detail.Count - line * across);
            float width = here * size.X + (here - 1) * DetailGap;
            float left = OptionsPanel.Center.X - width / 2f;
            boxes[i] = new Rectangle((int)(left + column * (size.X + DetailGap)), (int)(top + line * (size.Y + DetailGap)), size.X, size.Y);
        }
        return boxes;
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
        if (_soulBound) _memory.Update(gameTime);

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

        if (!DrawPreview(spriteBatch, alpha))
        {
            // The shadow's the glow squashed flat, so it wants smoothing.
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            spriteBatch.Draw(_glow, new Rectangle((int)FigureFeet.X - ShadowSize.X / 2, (int)FigureFeet.Y - ShadowSize.Y / 2,
                ShadowSize.X, ShadowSize.Y), ShadowColor * alpha);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            DrawFigure(spriteBatch, alpha);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawBust(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        string heading = _soulBound ? SoulBoundHeading : YouHeading;
        Text.Draw(spriteBatch, _font, heading, OptionsPanel.Center.X, OptionsPanel.Top + FormPanel.Padding,
            Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        string shownName = Typed().Length > 0 ? Typed().ToUpperInvariant() : "?";
        Text.Draw(spriteBatch, _font, shownName, NameAt.X, NameAt.Y, Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, Summary(), NameAt.X, NameAt.Y + _font.LineSpacing + 6f,
            Palette.Bone * alpha, Palette.FormShadow * alpha, centred: true);

        // Under them on the left, since the tabs fill the line under the heading on the right.
        if (_soulBound)
            Text.Draw(spriteBatch, _detailFont, SoulBoundLine, PreviewPanel.Center.X, NameAt.Y + _font.LineSpacing + 6f + _detailFont.LineSpacing * 2f,
                Palette.Narration * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, Scores(), PreviewPanel.Center.X, FigureFeet.Y + 40f,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        if (_preview.DrawsArt)
            Text.Draw(spriteBatch, _detailFont, PreviewKeys(), PreviewPanel.Center.X, PreviewPanel.Bottom - PreviewKeysUp,
                Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, About(), OptionsPanel.Center.X, ButtonsY - ButtonSize.Y / 2f - 18f - _detailFont.LineSpacing,
            Palette.Narration * alpha, Palette.FormShadow * alpha, centred: true);

        string note = _status ?? (_typing ? TypingNote : HoveredName() ?? Note);
        Text.Draw(spriteBatch, _detailFont, note, OptionsPanel.Center.X, _noteY,
            (_status is null ? Palette.Note : Palette.Ember) * alpha, Palette.FormShadow * alpha, centred: true);
        spriteBatch.End();

        base.Draw(gameTime);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        DrawChips(spriteBatch, alpha);
        DrawDetail(spriteBatch, alpha);
        spriteBatch.End();
    }

    /// <summary>
    /// Their painted figure, if they've a painted body: the walker on the left, put together big in
    /// what they're wearing and dyed as they are (as painted while V's held), drawn at a room's scale
    /// so it moves as it will there. False if there's no painted body, for the paperdoll instead.
    /// </summary>
    private bool DrawPreview(SpriteBatch spriteBatch, float alpha)
    {
        Outfit outfit = PartCatalog.OutfitOf(_member);
        if (_comparing && outfit is not null) outfit = outfit with { Tints = default };
        if (PartCatalog.Large(outfit, PaintedHeight) is not { HasBody: true } figure)
        {
            _preview.Art = null;
            return false;
        }

        float scale = ZoneCatalog.RoomScale;
        _preview.Art = figure;
        _preview.Position = FigureFeet / scale;
        _preview.Opacity = alpha;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, transformMatrix: Matrix.CreateScale(scale, scale, 1f));
        _preview.Draw(spriteBatch, 0.5f, 1f / scale, Color.White);
        spriteBatch.End();
        return true;
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
        Rectangle[] boxes = DetailBoxes(detail);
        int current = detail.Current();
        for (int i = 0; i < boxes.Length; i++)
        {
            Rectangle box = boxes[i];
            spriteBatch.Draw(ScreenManager.BlankTexture, box, Palette.FormShadow * alpha);
            detail.DrawItem(spriteBatch, new Rectangle(box.X + ChipFrame, box.Y + ChipFrame, box.Width - ChipFrame * 2, box.Height - ChipFrame * 2), i, alpha);
            if (i == current) Frame(spriteBatch, box, PickedFrame, Palette.Torch * alpha);
            else if (i == _hovered) Frame(spriteBatch, box, HoverFrame, Palette.Bone * alpha);
        }
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
        foreach (string hair in hairs) names.Add("HAIR " + int.Parse(hair["hair-".Length..], System.Globalization.CultureInfo.InvariantCulture));
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
            ImportedFigure figure = PartCatalog.Of(PartCatalog.OutfitOf(_member));
            look.HairPart = kept;
            look.HideHelm = hidden;
            if (figure is not null) batch.Draw(figure.Portrait(box.Size), box, Color.White * alpha);
        }, index => names[index]);
    }

    private void DrawFace(SpriteBatch spriteBatch, Rectangle box, int index, float alpha)
    {
        if (index == 0)
        {
            if (_sheet is not null) spriteBatch.Draw(_sheet, box, Bust, Color.White * alpha);
            return;
        }
        if (PartCatalog.ForMember(_member.Id, PartCatalog.PlayerFaces[index - 1]) is ImportedFigure face)
            spriteBatch.Draw(face.Portrait(box.Size), box, Color.White * alpha);
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
        _page = page;
        MenuEntries.Clear();
        foreach (MenuEntry tab in _tabs) MenuEntries.Add(tab);
        float y = RowsTop;
        foreach (MenuEntry row in _pages[page])
        {
            if (_shownWhen.TryGetValue(row, out Func<bool> shown) && !shown()) continue;
            MenuEntries.Add(row);
            row.Plate.Center = new Vector2(OptionsPanel.Center.X, y + RowSize.Y / 2f);
            y += RowSize.Y + RowGap;
        }
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
        _noteY = ButtonsY + ButtonSize.Y / 2f + 14f;
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

    /// <summary>Their face on a panel of its own: the painted one they picked, at the frame's size, or the paperdoll's blown up.</summary>
    private void DrawBust(SpriteBatch spriteBatch, float alpha)
    {
        if (_sheet is null) return;

        var well = new Point((int)(Bust.Width * BustScale), (int)(Bust.Height * BustScale));
        var frame = new Rectangle(BustAt.X - 16, BustAt.Y - 16, well.X + 32, well.Y + 32);
        _panel.Draw(spriteBatch, frame, alpha);
        if (PartCatalog.Dress(_member) is ImportedFigure painted)
            spriteBatch.Draw(painted.Portrait(well), BustAt.ToVector2(), Color.White * alpha);
        else
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
