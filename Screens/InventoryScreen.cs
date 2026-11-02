using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Items;
using LegendQuest.Rules;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.Story;
using LegendQuest.Tutorial;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// Popup. The inventory and the character sheets, over the map: the party's faces down the left,
/// whoever's picked big in the middle, turning on the spot in what they're wearing with their nine
/// slots round them, and on the right either the party's bag, its gold and the key items, or their
/// character sheet.
/// </summary>
/// <remarks>
/// A click (or Enter) on something in the bag puts it on whoever's picked, in the slot it goes in,
/// and whatever was there goes back in the bag; a click on a slot takes off what's in it, and on an
/// empty one finds something in the bag for it. A potion's drunk the same way. The rules are the
/// Wardrobe's, and the words in the tooltip are ItemReadout's and GearComparison's. They're baked
/// again off the paperdoll the moment anything changes, so it shows on them at once, and Changed
/// tells the map under this, so the party walking about wears it too. Whoever's waiting at camp can
/// be looked at but not changed: their kit's at the Flagon with them. The keys move round the slots,
/// the bag and the key items by where things are on screen, Q and E change who's picked, Tab
/// changes the page, and I or C on its own page closes it, the way they opened it.
/// </remarks>
public class InventoryScreen : GameScreen
{
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.2);

    /// <summary>The two panels: whoever's picked, and the page with the bag or the sheet on it.</summary>
    private static readonly Rectangle MemberPanel = new(140, 24, 620, 820);
    private static readonly Rectangle PagePanel = new(780, 24, 790, 820);

    /// <summary>Where the line along the bottom of the screen goes: the keys, or what's just happened.</summary>
    private const float HintTop = 856f;

    /// <summary>Where their name goes, down from the top of their panel. The rest of the panel is measured from its top too.</summary>
    private const int NameTop = FormPanel.Padding;

    /// <summary>The slots round them: how far in from the panel's sides the two columns are, how far down the first row is, the room between rows, and how far down the feet's slot under them is.</summary>
    private const int SlotInset = 44;
    private const int SlotsTop = 166;
    private const int SlotGap = 22;
    private const int FeetSlotTop = 548;

    /// <summary>The slots down the left of them and down the right, top to bottom, with the feet's slot under them. The main hand's on the left, the side it's on when they face you.</summary>
    private static readonly EquipmentSlot[] LeftColumn = { EquipmentSlot.Head, EquipmentSlot.Cloak, EquipmentSlot.Body, EquipmentSlot.MainHand };
    private static readonly EquipmentSlot[] RightColumn = { EquipmentSlot.Amulet, EquipmentSlot.Ring, EquipmentSlot.Hands, EquipmentSlot.OffHand };

    /// <summary>Them, full length: how much bigger than art pixels, how far down their panel their feet are, where that is (in the middle, between the slots), and the shadow under them.</summary>
    private const float FigureScale = 7f;
    private const float FeetTop = 518f;
    private static readonly Vector2 FigureFeet = new(MemberPanel.Center.X, MemberPanel.Top + FeetTop);
    private static readonly Point ShadowSize = new(140, 30);
    private static readonly Color ShadowColor = Color.Black * 0.45f;

    /// <summary>A paperdoll frame and the point in it that's their feet, in art pixels. Has to match the paperdoll's sheets (spec 6.5).</summary>
    private const int FrameWidth = 32;
    private const int FrameHeight = 48;
    private static readonly Vector2 FeetAnchor = new(16f, 46f);

    /// <summary>The paperdoll's columns: the walk's four, and the swing's wind-up and strike.</summary>
    private const int FirstStep = 1;
    private const int Steps = 4;
    private const int WindUpColumn = 5;
    private const int StrikeColumn = 6;

    /// <summary>How long they face each way before turning, and how long a step takes.</summary>
    private const float TurnSeconds = 2.2f;
    private const float StepSeconds = 0.16f;

    /// <summary>The way they turn round: down, right, up, left, so it looks like turning on the spot.</summary>
    private static readonly Direction[] Turning = { Direction.Down, Direction.Right, Direction.Up, Direction.Left };

    /// <summary>When something changes on them: how long the swing a new weapon gets lasts (wind-up for the first half, strike for the second), and the gold glow behind them.</summary>
    private const float SwingSeconds = 0.5f;
    private const float GlowSeconds = 0.8f;
    private static readonly Point GlowSize = new(300, 380);
    private static readonly Color GlowColour = new Color(240, 200, 104) * 0.4f;

    /// <summary>The tiles under them, HP, AC and speed: how far down their panel, how many, and the room between.</summary>
    private const int TilesTop = 636;
    private const int TileCount = 3;
    private const int TileGap = 10;

    /// <summary>The bag: slots across, rows that show at once, and how far down the page it starts. The character sheet starts there too.</summary>
    private const int GridColumns = 9;
    private const int GridRows = 5;
    private const int GridTop = 106;

    /// <summary>The gold and the key items under the bag: how far down the page the row with the gold is, and the key items' two columns.</summary>
    private const int GoldTop = 500;
    private const int KeyColumns = 2;
    private const int KeyWidth = 340;

    /// <summary>The room between the two page tabs.</summary>
    private const float TabGap = 90f;

    /// <summary>How long a line about what's just happened stays on the bottom line, and how long it takes to fade back to the keys.</summary>
    private const float SayingSeconds = 3.5f;
    private const float SayingFade = 0.5f;

    private const string BagTab = "THE BAG";
    private const string SheetTab = "CHARACTER";
    private const string KeyItemsHeading = "KEY ITEMS";
    private const string NoKeyItems = "Nothing yet.";
    private const string BagHint = "ENTER  ·  USE      Q  E  ·  WHO      TAB  ·  SHEET      I  ·  CLOSE";
    private const string SheetHint = "UP  DOWN  ·  ABILITIES      Q  E  ·  WHO      TAB  ·  BAG      C  ·  CLOSE";
    private const string PutOn = "CLICK  ·  PUT IT ON";
    private const string Drink = "CLICK  ·  DRINK IT";
    private const string TakeOff = "CLICK  ·  TAKE IT OFF";
    private const string Find = "CLICK  ·  FIND SOMETHING FOR IT";
    private const string NothingWorn = "Nothing worn here.";
    private const string NothingFits = "Nothing in the bag goes there.";

    private readonly InputAction _close = new(new[] { Buttons.B, Buttons.Back }, new[] { Keys.Escape }, true);
    private readonly InputAction _bagKey = new(Array.Empty<Buttons>(), new[] { Keys.I }, true);
    private readonly InputAction _sheetKey = new(Array.Empty<Buttons>(), new[] { Keys.C }, true);
    private readonly InputAction _switchPage = new(new[] { Buttons.Y }, new[] { Keys.Tab }, true);
    private readonly InputAction _previousMember = new(new[] { Buttons.LeftShoulder }, new[] { Keys.Q }, true);
    private readonly InputAction _nextMember = new(new[] { Buttons.RightShoulder }, new[] { Keys.E }, true);
    private readonly InputAction _up = new(new[] { Buttons.DPadUp, Buttons.LeftThumbstickUp }, new[] { Keys.Up, Keys.W }, true);
    private readonly InputAction _down = new(new[] { Buttons.DPadDown, Buttons.LeftThumbstickDown }, new[] { Keys.Down, Keys.S }, true);
    private readonly InputAction _left = new(new[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft }, new[] { Keys.Left, Keys.A }, true);
    private readonly InputAction _right = new(new[] { Buttons.DPadRight, Buttons.LeftThumbstickRight }, new[] { Keys.Right, Keys.D }, true);
    private readonly InputAction _act = new(new[] { Buttons.A }, new[] { Keys.Enter, Keys.Space }, true);
    private readonly InputAction[] _pickMember =
    {
        new(Array.Empty<Buttons>(), new[] { Keys.F1 }, true),
        new(Array.Empty<Buttons>(), new[] { Keys.F2 }, true),
        new(Array.Empty<Buttons>(), new[] { Keys.F3 }, true),
        new(Array.Empty<Buttons>(), new[] { Keys.F4 }, true),
    };

    private readonly SaveData _game;
    private readonly List<PartyMember> _members = new();
    private readonly int _partyCount;
    private readonly List<Texture2D> _sheets = new();
    private readonly Dictionary<EquipmentSlot, Rectangle> _slots = new();
    private readonly List<Rectangle> _tiles = new();
    private readonly ItemGrid _grid = new(new Point(PagePanel.Center.X - ItemGrid.WidthOf(GridColumns) / 2, PagePanel.Top + GridTop), GridColumns, GridRows, 0);
    private readonly FormPanel _panel = new();
    private readonly Paperdoll _paperdoll = new();
    private readonly ItemIcons _icons = new();
    private readonly ItemTooltip _tooltip = new();
    private readonly PortraitRail _rail = new();
    private readonly StatTiles _statTiles = new();
    private readonly CharacterSheet _sheet = new();
    private readonly TutorialPanel _tutorialPanel = new();
    private readonly Random _random = new();

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Texture2D _glow;
    private Tutorials _tutorials;

    /// <summary>The bag as it's shown, sorted, and the key items' ids.</summary>
    private List<ItemStack> _bag = new();
    private List<string> _keyItems = new();

    private InventoryPage _page;
    private int _selected;
    private int _hoveredCard = -1;
    private Rectangle _bagTab;
    private Rectangle _sheetTab;

    /// <summary>What the keys are on: a slot, or a place in the bag or the key items. Whether its tooltip's up, what the tooltip was last made for (so it's only made again when that changes), and where it goes, or null for nowhere.</summary>
    private InventorySpot _spot = InventorySpot.Bag;
    private EquipmentSlot _slot = EquipmentSlot.MainHand;
    private int _index;
    private bool _showTip;
    private (InventorySpot Spot, EquipmentSlot Slot, int Index, int Member, int Version)? _tipFor;
    private Rectangle? _tipBeside;

    /// <summary>Goes up by one whenever anything changes, so a tooltip made before it is made again.</summary>
    private int _version;

    /// <summary>The clock they turn by, and what's left of the swing and the glow when something changes on them.</summary>
    private float _turnClock;
    private float _swingClock;
    private float _glowClock;

    /// <summary>What's just happened, for the bottom line, its colour, and how long it's been up.</summary>
    private string _saying;
    private Color _sayingColour;
    private float _sayingAge;

    /// <summary>Raised whenever what anyone's wearing, or their hit points, change, so the map under this can dress them again.</summary>
    public event Action Changed;

    /// <summary>Gets the inventory ready for a game, on whoever you're controlling.</summary>
    /// <param name="game">The game: its party, camp, bag, gold and key items.</param>
    /// <param name="page">The page to open on: the bag for I, the sheet for C.</param>
    public InventoryScreen(SaveData game, InventoryPage page)
    {
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _game = game;
        _page = page;
        _members.AddRange(game.Party);
        _members.AddRange(game.Camp);
        _partyCount = game.Party.Count;
        _selected = game.Leader;
    }

    /// <summary>Whoever's picked.</summary>
    private PartyMember Member => _members[_selected];

    /// <summary>Whether whoever's picked is waiting at camp, so their kit can be looked at but not changed.</summary>
    private bool AtCamp => _selected >= _partyCount;

    /// <summary>Loads everything, bakes everyone, lays it out, and puts up the tips for putting things on and drinking.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();
        Texture2D pixel = ScreenManager.BlankTexture;

        _panel.LoadContent(_content, ScreenManager.GraphicsDevice);
        _paperdoll.LoadContent(_content);
        _icons.LoadContent(_content);
        _tooltip.LoadContent(_content, pixel);
        _rail.LoadContent(_content, pixel);
        _statTiles.LoadContent(_content);
        _sheet.LoadContent(_content);
        _tutorialPanel.LoadContent(_content, pixel);
        _grid.LoadContent(pixel);
        _font = _content.Load<SpriteFont>("spectral-ui");
        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        _glow = _content.Load<Texture2D>("glow");

        foreach (PartyMember member in _members) _sheets.Add(Bake(member));
        LayOut();
        Refill();
        _sheet.Show(Member, SheetArea());

        _tutorials = new Tutorials(_game, _audio.Settings);
        _tutorials.Complete(TutorialCatalog.InventoryOpen);
        _tutorials.Show(TutorialCatalog.InventoryEquip);
        if (_game.Bag.Count(ItemCatalog.HealingPotion) > 0) _tutorials.Show(TutorialCatalog.InventoryPotion);

        _audio.Play(Sfx.MenuConfirm);
    }

    /// <summary>Lets go of the baked sheets and unloads everything else.</summary>
    public override void Unload()
    {
        foreach (Texture2D sheet in _sheets) sheet.Dispose();
        _content?.Unload();
    }

    /// <summary>A tip's card first, then closing, the pages, who's picked, the mouse and the keys.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_tutorialPanel.HandleInput(input, _tutorials)) return;

        if (_close.Occurred(input) || _bagKey.Occurred(input) && _page == InventoryPage.Bag || _sheetKey.Occurred(input) && _page == InventoryPage.Character)
        {
            _audio.Play(Sfx.MenuBack);
            ExitScreen();
            return;
        }

        if (_bagKey.Occurred(input) || _sheetKey.Occurred(input) || _switchPage.Occurred(input))
        {
            ShowPage(_page == InventoryPage.Bag ? InventoryPage.Character : InventoryPage.Bag);
            return;
        }

        if (_previousMember.Occurred(input)) Pick((_selected + _members.Count - 1) % _members.Count);
        if (_nextMember.Occurred(input)) Pick((_selected + 1) % _members.Count);
        for (int i = 0; i < _pickMember.Length && i < _partyCount; i++)
            if (_pickMember[i].Occurred(input)) Pick(i);

        if (HandleMouse(input)) return;
        HandleKeys(input);
    }

    /// <summary>Turns them, runs the swing, the glow and the bottom line's clock, puts the tooltip where it goes, and runs the tips' card.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _turnClock += seconds;
        _swingClock = MathF.Max(0f, _swingClock - seconds);
        _glowClock = MathF.Max(0f, _glowClock - seconds);
        _sayingAge += seconds;
        _tipBeside = PlaceTip();
        _tutorialPanel.Update(gameTime, _tutorials, IsActive);
    }

    /// <summary>Draws the veil and the panels, the faces, the slots and the page, them with their shadow and any glow, all the words, then the tooltip and a tip's card over everything.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _panel.DrawVeil(spriteBatch, alpha, FormPanel.FullVeil);
        _panel.Draw(spriteBatch, MemberPanel, alpha);
        _panel.Draw(spriteBatch, PagePanel, alpha);
        _rail.DrawCards(spriteBatch, _members, _sheets, _partyCount, _selected, _hoveredCard, alpha);
        DrawSlots(spriteBatch, alpha);
        foreach (Rectangle tile in _tiles) _statTiles.DrawTile(spriteBatch, tile, alpha);
        if (_page == InventoryPage.Bag) _grid.DrawCells(spriteBatch, _icons, _spot == InventorySpot.Bag ? _index : -1, alpha);
        else _sheet.DrawTiles(spriteBatch, alpha);
        spriteBatch.End();

        // The shadow's the glow squashed flat, and the glow behind them when something changes is the
        // same texture added on, so both want smoothing.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        spriteBatch.Draw(_glow, new Rectangle((int)FigureFeet.X - ShadowSize.X / 2, (int)FigureFeet.Y - ShadowSize.Y / 2, ShadowSize.X, ShadowSize.Y),
            ShadowColor * alpha);
        spriteBatch.End();

        if (_glowClock > 0f)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp);
            var glow = new Rectangle((int)FigureFeet.X - GlowSize.X / 2, (int)FigureFeet.Y - GlowSize.Y + ShadowSize.Y, GlowSize.X, GlowSize.Y);
            spriteBatch.Draw(_glow, glow, GlowColour * (alpha * _glowClock / GlowSeconds));
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawFigure(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        DrawHeader(spriteBatch, alpha);
        DrawTileWords(spriteBatch, alpha);
        DrawTab(spriteBatch, BagTab, _bagTab, _page == InventoryPage.Bag, alpha);
        DrawTab(spriteBatch, SheetTab, _sheetTab, _page == InventoryPage.Character, alpha);
        if (_page == InventoryPage.Bag)
        {
            _grid.DrawWords(spriteBatch, _detailFont, alpha);
            DrawGoldAndKeys(spriteBatch, alpha);
        }
        else
        {
            _sheet.DrawWords(spriteBatch, alpha);
        }
        _rail.DrawWords(spriteBatch, _members.Count, _partyCount, alpha);
        DrawBottomLine(spriteBatch, alpha);
        spriteBatch.End();

        if (_tipBeside is Rectangle beside)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _tooltip.DrawCard(spriteBatch, beside, alpha);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            _tooltip.DrawWords(spriteBatch, beside, alpha);
            spriteBatch.End();
        }

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        _tutorialPanel.DrawCard(spriteBatch, alpha);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _tutorialPanel.DrawWords(spriteBatch, alpha);
        spriteBatch.End();
    }

    /// <summary>The mouse: who's picked on the faces, the page on its tabs, the wheel on the bag, the abilities on the sheet, and pointing at and clicking a slot, a place in the bag or a key item.</summary>
    /// <returns>True if it did something that's the end of this frame's input.</returns>
    private bool HandleMouse(InputState input)
    {
        Point mouse = input.MousePosition;
        _hoveredCard = PortraitRail.CardAt(mouse, _members.Count, _partyCount);

        if (input.IsNewLeftClick && _hoveredCard >= 0)
        {
            Pick(_hoveredCard);
            return true;
        }

        if (input.IsNewLeftClick && (_bagTab.Contains(mouse) && _page != InventoryPage.Bag || _sheetTab.Contains(mouse) && _page != InventoryPage.Character))
        {
            ShowPage(_page == InventoryPage.Bag ? InventoryPage.Character : InventoryPage.Bag);
            return true;
        }

        if (_page == InventoryPage.Bag && input.WheelTurn != 0 && _grid.Bounds.Contains(mouse)) _grid.ScrollBy(-input.WheelTurn);
        if (_page == InventoryPage.Character && input.MouseMoved && _sheet.PickAt(mouse)) _audio.Play(Sfx.MenuMove);

        if (!input.MouseMoved && !input.IsNewLeftClick) return false;

        if (SpotAt(mouse) is not { } found)
        {
            if (input.MouseMoved) _showTip = false;
            return false;
        }

        if (found.Spot != _spot || found.Slot != _slot || found.Index != _index) _audio.Play(Sfx.MenuMove);
        _spot = found.Spot;
        _slot = found.Slot;
        _index = found.Index;
        _showTip = true;

        if (!input.IsNewLeftClick) return false;
        Act();
        return true;
    }

    /// <summary>The keys: up and down the abilities on the sheet; round the slots, the bag and the key items on the bag's page, and Enter to use what they're on.</summary>
    private void HandleKeys(InputState input)
    {
        int across = (_right.Occurred(input) ? 1 : 0) - (_left.Occurred(input) ? 1 : 0);
        int down = (_down.Occurred(input) ? 1 : 0) - (_up.Occurred(input) ? 1 : 0);

        if (_page == InventoryPage.Character)
        {
            if (down != 0 && _sheet.Step(down)) _audio.Play(Sfx.MenuMove);
            return;
        }

        if ((across != 0 || down != 0) && Move(across, across == 0 ? down : 0))
        {
            _showTip = true;
            _audio.Play(Sfx.MenuMove);
        }

        if (!_act.Occurred(input)) return;
        _showTip = true;
        Act();
    }

    /// <summary>Moves the keys one step, by where things are on screen.</summary>
    /// <param name="across">-1 left, 1 right, or 0.</param>
    /// <param name="down">-1 up, 1 down, or 0. Only one of the two is ever set.</param>
    /// <returns>True if the keys moved.</returns>
    private bool Move(int across, int down) => _spot switch
    {
        InventorySpot.Slot => MoveFromSlot(across, down),
        InventorySpot.Bag => MoveFromBag(across, down),
        _ => MoveFromKey(across, down),
    };

    /// <summary>Round the slots: up and down a column, across between the columns, down from the hands to the feet, and right from the right-hand column into the bag.</summary>
    private bool MoveFromSlot(int across, int down)
    {
        if (_slot == EquipmentSlot.Feet)
        {
            if (down < 0 || across < 0) return FocusSlot(EquipmentSlot.MainHand);
            return across > 0 && FocusSlot(EquipmentSlot.OffHand);
        }

        bool onLeft = Array.IndexOf(LeftColumn, _slot) >= 0;
        EquipmentSlot[] column = onLeft ? LeftColumn : RightColumn;
        int row = Array.IndexOf(column, _slot);

        if (down != 0)
        {
            int next = row + down;
            if (next >= column.Length) return FocusSlot(EquipmentSlot.Feet);
            return next >= 0 && FocusSlot(column[next]);
        }

        if (onLeft) return across > 0 && FocusSlot(RightColumn[row]);
        if (across < 0) return FocusSlot(LeftColumn[row]);
        return FocusBag(_grid.EnterFrom(_slots[_slot].Center.Y, fromLeft: true));
    }

    /// <summary>Round the bag: a step in the grid, out of its left edge to the slot level with it, and down off its bottom to the key items.</summary>
    private bool MoveFromBag(int across, int down)
    {
        int next = _grid.Step(_index, across, down);
        if (next >= 0) return FocusBag(next);
        if (across < 0) return FocusSlot(SlotBeside(_grid.CellAt(_index).Center.Y));
        if (down > 0 && KeysShown() > 0) return FocusKey(_index % GridColumns < GridColumns / 2 ? 0 : Math.Min(1, KeysShown() - 1));
        return false;
    }

    /// <summary>Round the key items: across their two columns and down them, up off the top into the bag, and left off them to the slots.</summary>
    private bool MoveFromKey(int across, int down)
    {
        int column = _index % KeyColumns;
        if (across < 0) return column == 0 ? FocusSlot(SlotBeside(KeyRect(_index).Center.Y)) : FocusKey(_index - 1);
        if (across > 0) return column < KeyColumns - 1 && FocusKey(_index + 1);

        int next = _index + down * KeyColumns;
        if (next < 0) return FocusBag(_grid.Count - 1);
        return FocusKey(next);
    }

    /// <summary>Puts the keys on a slot.</summary>
    /// <returns>True, so a move can end on it.</returns>
    private bool FocusSlot(EquipmentSlot slot)
    {
        _spot = InventorySpot.Slot;
        _slot = slot;
        return true;
    }

    /// <summary>Puts the keys on a place in the bag, and scrolls to it.</summary>
    /// <returns>True if there was a place to put them on.</returns>
    private bool FocusBag(int index)
    {
        if (index < 0 || index >= _grid.Count) return false;
        _spot = InventorySpot.Bag;
        _index = index;
        _grid.Reveal(index);
        return true;
    }

    /// <summary>Puts the keys on a key item.</summary>
    /// <returns>True if there was one to put them on.</returns>
    private bool FocusKey(int index)
    {
        if (index < 0 || index >= KeysShown()) return false;
        _spot = InventorySpot.KeyItem;
        _index = index;
        return true;
    }

    /// <summary>The slot in the right-hand column nearest level with a point, for coming out of the bag or the key items to the left.</summary>
    private EquipmentSlot SlotBeside(float y)
    {
        EquipmentSlot nearest = RightColumn[0];
        foreach (EquipmentSlot slot in RightColumn)
            if (MathF.Abs(_slots[slot].Center.Y - y) < MathF.Abs(_slots[nearest].Center.Y - y)) nearest = slot;
        return nearest;
    }

    /// <summary>What a click or Enter does on whatever the keys are on: takes off what's in a slot (or finds something for an empty one), or puts on or drinks something from the bag. A key item's only there to be read.</summary>
    private void Act()
    {
        if (_spot == InventorySpot.Slot) ActOnSlot();
        else if (_spot == InventorySpot.Bag && _index < _bag.Count) Use(ItemCatalog.Get(_bag[_index].Id));
    }

    /// <summary>Takes off what's in the slot the keys are on, or finds something in the bag that goes in it if it's empty.</summary>
    private void ActOnSlot()
    {
        PartyMember member = Member;
        if (member.WornIn(_slot) is not ItemDefinition worn)
        {
            FindFor(_slot);
            return;
        }

        if (AtCamp)
        {
            Refuse(AtCampLine(member));
            return;
        }

        Wardrobe.TakeOff(member, _game.Bag, _slot);
        _audio.Play(Sfx.Equip);
        Say($"{member.Name} takes off the {worn.Name}.", Palette.Gold);
        AfterChange(member, swing: false);
    }

    /// <summary>Puts the keys on the first thing in the bag that goes in a slot, on the bag's page, or says there's nothing.</summary>
    private void FindFor(EquipmentSlot slot)
    {
        int index = _bag.FindIndex(stack => ItemCatalog.Get(stack.Id).Fits(slot));
        if (index < 0)
        {
            Refuse(NothingFits);
            return;
        }

        if (_page != InventoryPage.Bag) ShowPage(InventoryPage.Bag);
        FocusBag(index);
        _showTip = true;
        _audio.Play(Sfx.MenuMove);
    }

    /// <summary>Puts something from the bag on whoever's picked, or has them drink it, if they can.</summary>
    private void Use(ItemDefinition item)
    {
        PartyMember member = Member;
        if (AtCamp)
        {
            Refuse(AtCampLine(member));
            return;
        }

        if (Wardrobe.WhyNot(member, item) is string why)
        {
            Refuse(why);
            return;
        }

        if (item.Slot is not null) Wear(member, item);
        else Quaff(member, item);
    }

    /// <summary>Puts something on them, and says so, or says they aren't trained with it if they aren't.</summary>
    private void Wear(PartyMember member, ItemDefinition item)
    {
        Wardrobe.Wear(member, _game.Bag, item.Id);
        _audio.Play(Sfx.Equip);
        _tutorials.Complete(TutorialCatalog.InventoryEquip);

        bool held = item.Kind is ItemKind.Weapon or ItemKind.Shield;
        if (ClassCatalog.Get(member.Class).Proficient(item))
            Say(held ? $"{member.Name} takes up the {item.Name}." : $"{member.Name} puts on the {item.Name}.", Palette.Gold);
        else
            Say($"{member.Name} isn't trained with the {item.Name}.", Palette.Ember);
        AfterChange(member, swing: item.Kind == ItemKind.Weapon);
    }

    /// <summary>They drink something, and it says what it did.</summary>
    private void Quaff(PartyMember member, ItemDefinition item)
    {
        int amount = Wardrobe.Drink(member, _game.Bag, item, _random);
        _audio.Play(Sfx.Potion);
        _tutorials.Complete(TutorialCatalog.InventoryPotion);

        string count = amount.ToString(CultureInfo.InvariantCulture);
        Say(item.Heals.IsNone
            ? $"{member.Name} drinks the {item.Name}: {count} spell slot{(amount == 1 ? string.Empty : "s")} back."
            : $"{member.Name} drinks the {item.Name}: +{count} HP.", Palette.Gold);
        AfterChange(member, swing: false);
    }

    /// <summary>Says why something can't be done, with the sound for it.</summary>
    private void Refuse(string why)
    {
        _audio.Play(Sfx.MenuBack);
        Say(why, Palette.Ember);
    }

    /// <summary>What the bottom line says when someone at camp is asked to change.</summary>
    private static string AtCampLine(PartyMember member) => $"{member.Name} is waiting at {MapCatalog.Get(MapCatalog.Camp).Name}, kit and all.";

    /// <summary>Catches everything up after something on someone changed: their sheet is baked again, the bag and the character sheet are filled again, they turn to face you, and the map's told.</summary>
    /// <param name="member">Who changed.</param>
    /// <param name="swing">Whether they've taken up a new weapon, which they swing once.</param>
    private void AfterChange(PartyMember member, bool swing)
    {
        _version++;
        int index = _members.IndexOf(member);
        _sheets[index].Dispose();
        _sheets[index] = Bake(member);

        Refill();
        _sheet.Show(Member, SheetArea());
        _turnClock = 0f;
        _glowClock = GlowSeconds;
        _swingClock = swing ? SwingSeconds : 0f;
        Changed?.Invoke();
    }

    /// <summary>Picks someone to look at: their sheet, their slots, and them turning to face you.</summary>
    private void Pick(int index)
    {
        if (index == _selected) return;

        _selected = index;
        _version++;
        _turnClock = 0f;
        _swingClock = 0f;
        _sheet.PickFirst();
        _sheet.Show(Member, SheetArea());
        _audio.Play(Sfx.MenuMove);
    }

    /// <summary>Turns to a page. The tooltip goes until something's pointed at on it.</summary>
    private void ShowPage(InventoryPage page)
    {
        _page = page;
        _showTip = false;
        if (page == InventoryPage.Character) _sheet.Show(Member, SheetArea());
        _audio.Play(Sfx.OptionChange);
    }

    /// <summary>Fills the bag's grid and the key items from the save again, and keeps the keys on something that's still there.</summary>
    private void Refill()
    {
        _bag = BagOrder.Of(_game.Inventory);
        var entries = new List<GridEntry>(_bag.Count);
        foreach (ItemStack stack in _bag) entries.Add(new GridEntry(ItemCatalog.Get(stack.Id), stack.Count, null, Color.Transparent, false));
        _grid.Fill(entries);

        _keyItems = _game.Items.FindAll(KeyItems.Exists);

        if (_spot == InventorySpot.Bag && _index >= _bag.Count)
        {
            if (_bag.Count > 0) FocusBag(_bag.Count - 1);
            else FocusSlot(EquipmentSlot.MainHand);
        }
        if (_spot == InventorySpot.KeyItem && _index >= KeysShown()) FocusSlot(EquipmentSlot.MainHand);
    }

    /// <summary>Puts a line on the bottom of the screen for a few seconds, in place of the keys.</summary>
    private void Say(string line, Color colour)
    {
        _saying = line;
        _sayingColour = colour;
        _sayingAge = 0f;
    }

    /// <summary>Bakes someone off the paperdoll in what they're wearing.</summary>
    /// <returns>A new sheet, which this screen disposes.</returns>
    private Texture2D Bake(PartyMember member) => _paperdoll.Compose(ScreenManager.GraphicsDevice, member.Look, Paperdoll.GearOf(member));

    /// <summary>What the keys or the mouse are on, if there's a tooltip for it: the slot, the place in the bag or the key item, on screen. The tooltip's made again first if what it's for has changed.</summary>
    /// <returns>Where it is, or null for no tooltip.</returns>
    private Rectangle? PlaceTip()
    {
        if (!_showTip) return null;

        Rectangle? beside = _spot switch
        {
            InventorySpot.Slot => _slots[_slot],
            InventorySpot.Bag when _page == InventoryPage.Bag && _index < _bag.Count => _grid.CellAt(_index),
            InventorySpot.KeyItem when _page == InventoryPage.Bag && _index < KeysShown() => KeyRect(_index),
            _ => null,
        };
        if (beside is not Rectangle rectangle || !OnScreen(rectangle)) return null;

        var tipFor = (_spot, _slot, _index, _selected, _version);
        if (_tipFor != tipFor)
        {
            _tipFor = tipFor;
            MakeTip();
        }
        return rectangle;
    }

    /// <summary>Whether a place's slot is where it can be seen: a slot or a key item always is, and a place in the bag when it isn't scrolled away.</summary>
    private bool OnScreen(Rectangle slot) => _spot != InventorySpot.Bag || _grid.Bounds.Contains(slot.Center);

    /// <summary>Puts on the tooltip whatever the keys are on: what's worn in a slot, an empty slot, something in the bag measured against what whoever's picked is wearing, or a key item.</summary>
    private void MakeTip()
    {
        PartyMember member = Member;
        switch (_spot)
        {
            case InventorySpot.Slot when member.WornIn(_slot) is ItemDefinition worn:
                _tooltip.ShowItem(worn, member, null, Worth(worn), AtCamp ? null : TakeOff);
                break;

            case InventorySpot.Slot:
                _tooltip.ShowText(ItemReadout.SlotName(_slot).ToUpperInvariant(), NothingWorn, AtCamp ? null : Find);
                break;

            case InventorySpot.Bag:
                ItemDefinition item = ItemCatalog.Get(_bag[_index].Id);
                string action = AtCamp ? null : item.Slot is not null ? PutOn : Wardrobe.WhyNot(member, item) is null ? Drink : null;
                _tooltip.ShowItem(item, member, item.SlotOn(member.Equipment), Worth(item), action);
                break;

            default:
                KeyItem key = KeyItems.Get(_keyItems[_index]);
                _tooltip.ShowText(key.Name.ToUpperInvariant(), key.Description, null);
                break;
        }
    }

    /// <summary>What an item's worth, as the tooltip says it.</summary>
    private static string Worth(ItemDefinition item) => "WORTH " + item.Value.ToString(CultureInfo.InvariantCulture) + " GOLD";

    /// <summary>What's under a point: a slot, a place in the bag, or a key item.</summary>
    /// <returns>What and which, or null for nothing.</returns>
    private (InventorySpot Spot, EquipmentSlot Slot, int Index)? SpotAt(Point point)
    {
        foreach ((EquipmentSlot slot, Rectangle rectangle) in _slots)
            if (rectangle.Contains(point)) return (InventorySpot.Slot, slot, _index);

        if (_page != InventoryPage.Bag) return null;

        int index = _grid.IndexAt(point);
        if (index >= 0) return (InventorySpot.Bag, _slot, index);

        for (int i = 0; i < KeysShown(); i++)
            if (KeyRect(i).Contains(point)) return (InventorySpot.KeyItem, _slot, i);
        return null;
    }

    /// <summary>Where a key item's name is on the page.</summary>
    private Rectangle KeyRect(int index)
    {
        Rectangle grid = _grid.Bounds;
        int top = PagePanel.Top + GoldTop + _font.LineSpacing;
        return new Rectangle(grid.Left + index % KeyColumns * KeyWidth, top + index / KeyColumns * _detailFont.LineSpacing, KeyWidth, _detailFont.LineSpacing);
    }

    /// <summary>How many of the key items fit on the page, two to a row, above its bottom edge.</summary>
    private int KeysShown()
    {
        int rows = (PagePanel.Bottom - FormPanel.Padding - KeyRect(0).Top) / _detailFont.LineSpacing;
        return Math.Min(_keyItems.Count, rows * KeyColumns);
    }

    /// <summary>Where the character sheet goes on its page: under the tabs, as wide as the bag.</summary>
    private Rectangle SheetArea()
    {
        Rectangle grid = _grid.Bounds;
        return new Rectangle(grid.Left, grid.Top, grid.Width, PagePanel.Bottom - FormPanel.Padding - grid.Top);
    }

    /// <summary>Works out where the slots, the tiles under them and the page's tabs go.</summary>
    private void LayOut()
    {
        int left = MemberPanel.Left + SlotInset, right = MemberPanel.Right - SlotInset - ItemIcons.Size;
        for (int row = 0; row < LeftColumn.Length; row++)
        {
            int top = MemberPanel.Top + SlotsTop + row * (ItemIcons.Size + SlotGap);
            _slots[LeftColumn[row]] = new Rectangle(left, top, ItemIcons.Size, ItemIcons.Size);
            _slots[RightColumn[row]] = new Rectangle(right, top, ItemIcons.Size, ItemIcons.Size);
        }
        _slots[EquipmentSlot.Feet] = new Rectangle((int)FigureFeet.X - ItemIcons.Size / 2, MemberPanel.Top + FeetSlotTop, ItemIcons.Size, ItemIcons.Size);

        int width = (MemberPanel.Width - FormPanel.Padding * 2 - TileGap * (TileCount - 1)) / TileCount;
        for (int i = 0; i < TileCount; i++)
            _tiles.Add(new Rectangle(MemberPanel.Left + FormPanel.Padding + i * (width + TileGap), MemberPanel.Top + TilesTop, width, _statTiles.Height));

        int tabsTop = PagePanel.Top + FormPanel.Padding;
        Vector2 bag = _font.MeasureString(BagTab), sheet = _font.MeasureString(SheetTab);
        float tabsLeft = PagePanel.Center.X - (bag.X + TabGap + sheet.X) / 2f;
        _bagTab = new Rectangle((int)tabsLeft, tabsTop, (int)bag.X, _font.LineSpacing);
        _sheetTab = new Rectangle((int)(tabsLeft + bag.X + TabGap), tabsTop, (int)sheet.X, _font.LineSpacing);
    }

    /// <summary>Draws the slots round them: each well (lit where the keys are, gold where what they're on in the bag would go), then what's worn there, or the outline of what goes there if nothing is.</summary>
    private void DrawSlots(SpriteBatch spriteBatch, float alpha)
    {
        PartyMember member = Member;
        EquipmentSlot? target = _spot == InventorySpot.Bag && _page == InventoryPage.Bag && _index < _bag.Count
            ? ItemCatalog.Get(_bag[_index].Id).SlotOn(member.Equipment)
            : null;

        foreach ((EquipmentSlot slot, Rectangle rectangle) in _slots)
        {
            WellLook look = _spot == InventorySpot.Slot && _slot == slot ? WellLook.Hover : target == slot ? WellLook.Picked : WellLook.Plain;
            _icons.DrawWell(spriteBatch, rectangle, look, alpha);
            if (member.WornIn(slot) is ItemDefinition worn) _icons.DrawItem(spriteBatch, rectangle, worn, alpha);
            else _icons.DrawOutline(spriteBatch, rectangle, slot, alpha);
        }
    }

    /// <summary>Them, full length: turning on the spot and walking, or facing you and swinging a weapon they've just taken up.</summary>
    private void DrawFigure(SpriteBatch spriteBatch, float alpha)
    {
        Direction facing = Turning[(int)(_turnClock / TurnSeconds) % Turning.Length];
        int column = FirstStep + (int)(_turnClock / StepSeconds) % Steps;
        if (_swingClock > 0f)
        {
            facing = Direction.Down;
            column = _swingClock > SwingSeconds / 2f ? WindUpColumn : StrikeColumn;
        }

        var source = new Rectangle(column * FrameWidth, (int)facing * FrameHeight, FrameWidth, FrameHeight);
        spriteBatch.Draw(_sheets[_selected], FigureFeet, source, Color.White * alpha, 0f, FeetAnchor, FigureScale, SpriteEffects.None, 0f);
    }

    /// <summary>Their name over them, their people, class and level under it, and where they are if they're at camp.</summary>
    private void DrawHeader(SpriteBatch spriteBatch, float alpha)
    {
        PartyMember member = Member;
        float y = MemberPanel.Top + NameTop;
        Text.Draw(spriteBatch, _font, member.Name.ToUpperInvariant(), MemberPanel.Center.X, y, Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);

        y += _font.LineSpacing;
        string summary = member.Look.Ancestry.ToString().ToUpperInvariant() + "  ·  " + ClassCatalog.Get(member.Class).Name.ToUpperInvariant()
            + "  ·  LEVEL " + member.Level.ToString(CultureInfo.InvariantCulture);
        Text.Draw(spriteBatch, _detailFont, summary, MemberPanel.Center.X, y, Palette.Bone * alpha, Palette.FormShadow * alpha, centred: true);

        if (!AtCamp) return;
        y += _detailFont.LineSpacing;
        Text.Draw(spriteBatch, _detailFont, "WAITING AT " + MapCatalog.Get(MapCatalog.Camp).Name.ToUpperInvariant(), MemberPanel.Center.X, y,
            Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
    }

    /// <summary>Their hit points, AC and speed on the tiles under them.</summary>
    private void DrawTileWords(SpriteBatch spriteBatch, float alpha)
    {
        PartyMember member = Member;
        string hp = member.Hp.ToString(CultureInfo.InvariantCulture) + " / " + member.MaxHp.ToString(CultureInfo.InvariantCulture);
        _statTiles.DrawWords(spriteBatch, _tiles[0], "HP", hp, null, Palette.Note, alpha);
        _statTiles.DrawWords(spriteBatch, _tiles[1], "AC", CombatStats.ArmourClassOf(member).ToString(CultureInfo.InvariantCulture), null, Palette.Note, alpha);
        _statTiles.DrawWords(spriteBatch, _tiles[2], "SPEED", ItemReadout.Metres(CombatStats.SpeedOf(member)) + " M", null, Palette.Note, alpha);
    }

    /// <summary>One page's tab name: bright if it's the page that's up, dim if it isn't.</summary>
    private void DrawTab(SpriteBatch spriteBatch, string name, Rectangle tab, bool up, float alpha) =>
        Text.Draw(spriteBatch, _font, name, tab.Center.X, tab.Top, (up ? Palette.Heading : Palette.Note) * alpha, Palette.FormShadow * alpha, centred: true);

    /// <summary>The party's gold, and the key items under their heading, two to a row, the one the keys are on in torchlight.</summary>
    private void DrawGoldAndKeys(SpriteBatch spriteBatch, float alpha)
    {
        Rectangle grid = _grid.Bounds;
        float top = PagePanel.Top + GoldTop;
        Text.Draw(spriteBatch, _font, _game.Gold.ToString(CultureInfo.InvariantCulture) + " GOLD", grid.Right, top, Palette.Gold * alpha, Palette.FormShadow * alpha,
            rightAligned: true);
        Text.Draw(spriteBatch, _detailFont, KeyItemsHeading, grid.Left, top + (_font.LineSpacing - _detailFont.LineSpacing) / 2f,
            Palette.Heading * alpha, Palette.FormShadow * alpha);

        if (_keyItems.Count == 0)
        {
            Rectangle first = KeyRect(0);
            Text.Draw(spriteBatch, _detailFont, NoKeyItems, first.Left, first.Top, Palette.Note * alpha, Palette.FormShadow * alpha);
            return;
        }

        for (int i = 0; i < KeysShown(); i++)
        {
            Rectangle key = KeyRect(i);
            bool focused = _spot == InventorySpot.KeyItem && _index == i;
            Text.Draw(spriteBatch, _detailFont, KeyItems.Get(_keyItems[i]).Name, key.Left, key.Top, (focused ? Palette.Torch : Palette.Gold) * alpha,
                Palette.FormShadow * alpha);
        }
    }

    /// <summary>The line along the bottom of the screen: what's just happened, fading back to the keys for this page.</summary>
    private void DrawBottomLine(SpriteBatch spriteBatch, float alpha)
    {
        if (_saying is not null && _sayingAge < SayingSeconds)
        {
            float shown = MathHelper.Clamp((SayingSeconds - _sayingAge) / SayingFade, 0f, 1f) * alpha;
            Text.DrawCentered(spriteBatch, _detailFont, _saying, HintTop, _sayingColour * shown, Palette.FormShadow * shown);
            return;
        }

        Text.DrawCentered(spriteBatch, _detailFont, _page == InventoryPage.Bag ? BagHint : SheetHint, HintTop, Palette.Note * alpha, Palette.FormShadow * alpha);
    }
}
