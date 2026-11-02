using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Characters;
using LegendQuest.Components;
using LegendQuest.Items;
using LegendQuest.Save;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// Popup. A shop, over the map: its shelf on the left, at its prices, and the party's bag on the
/// right, at what the shop will pay. A click buys or sells one; with Shift held it sells the whole
/// stack.
/// </summary>
/// <remarks>
/// A conversation opens it (DialogueEffect.Shop) once the talking's done. The prices are the shop's
/// (ShopDefinition.Price, a markup over what a thing's worth, and Offer, half), and Trade moves the
/// gold and the goods. Whatever's sold here is on the end of the shelf until you leave, at what you
/// got for it, so a slip of the finger can be bought back without losing a copper. The tooltips
/// measure anything worn against one of the party, Q and E for which, the same as the inventory's;
/// what can't be afforded is greyed with its price in red. Only the bag can be sold from: what's
/// being worn has to come off in the inventory first, so nobody's sword is sold out of their hand.
/// </remarks>
public class ShopScreen : GameScreen
{
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.2);

    /// <summary>The panel, nearly the whole screen, split down the middle: the shelf on the left and the bag on the right.</summary>
    private static readonly Rectangle Panel = new(40, 24, 1520, 820);

    /// <summary>The line down the middle: how far in from the panel's top and bottom it stops, how thick it is, and its colour.</summary>
    private const int DividerInset = 60;
    private const int DividerWidth = 2;
    private static readonly Color DividerColour = new Color(240, 200, 104) * 0.25f;

    /// <summary>Where the line along the bottom of the screen goes: the keys, or what's just happened.</summary>
    private const float HintTop = 856f;

    /// <summary>Both grids: slots across, rows that show at once, the room for a price under each slot, and how far down the panel they start.</summary>
    private const int GridColumns = 8;
    private const int GridRows = 5;
    private const int PriceHeight = 28;
    private const int GridTop = 150;

    /// <summary>How far down the panel the line saying who the tooltips measure against goes, under the bag.</summary>
    private const int CompareTop = 690;

    /// <summary>How long a line about what's just happened stays on the bottom line, and how long it takes to fade back to the keys.</summary>
    private const float SayingSeconds = 3.5f;
    private const float SayingFade = 0.5f;

    /// <summary>A markup this close to 1 is a fair price.</summary>
    private const float FairTolerance = 0.001f;

    private const string BagHeading = "THE BAG";
    private const string FairPrices = "FAIR PRICES";
    private const string PricesLine = "PRICES AT {0}% OF WHAT THINGS ARE WORTH";
    private const string CompareLine = "MEASURED AGAINST {0}  ·  Q  E";
    private const string Hint = "CLICK  ·  BUY OR SELL      SHIFT  ·  ALL OF IT      TAB  ·  SIDES      Q  E  ·  WHO      ESC  ·  LEAVE";
    private const string BuyAction = "CLICK  ·  BUY";
    private const string SellAction = "CLICK  ·  SELL";
    private const string SellAllAction = "CLICK  ·  SELL ONE    SHIFT  ·  ALL";

    private readonly InputAction _close = new(new[] { Buttons.B, Buttons.Back }, new[] { Keys.Escape }, true);
    private readonly InputAction _switchSide = new(new[] { Buttons.Y }, new[] { Keys.Tab }, true);
    private readonly InputAction _previousMember = new(new[] { Buttons.LeftShoulder }, new[] { Keys.Q }, true);
    private readonly InputAction _nextMember = new(new[] { Buttons.RightShoulder }, new[] { Keys.E }, true);
    private readonly InputAction _up = new(new[] { Buttons.DPadUp, Buttons.LeftThumbstickUp }, new[] { Keys.Up, Keys.W }, true);
    private readonly InputAction _down = new(new[] { Buttons.DPadDown, Buttons.LeftThumbstickDown }, new[] { Keys.Down, Keys.S }, true);
    private readonly InputAction _left = new(new[] { Buttons.DPadLeft, Buttons.LeftThumbstickLeft }, new[] { Keys.Left, Keys.A }, true);
    private readonly InputAction _right = new(new[] { Buttons.DPadRight, Buttons.LeftThumbstickRight }, new[] { Keys.Right, Keys.D }, true);
    private readonly InputAction _act = new(new[] { Buttons.A }, new[] { Keys.Enter, Keys.Space }, true);
    private readonly InputAction _actAll = new(new[] { Buttons.X }, Array.Empty<Keys>(), true);

    private readonly ShopDefinition _shop;
    private readonly SaveData _game;
    private readonly ItemGrid _shelf = new(new Point(Panel.Left + Panel.Width / 4 - ItemGrid.WidthOf(GridColumns) / 2, Panel.Top + GridTop), GridColumns, GridRows, PriceHeight);
    private readonly ItemGrid _bagGrid = new(new Point(Panel.Right - Panel.Width / 4 - ItemGrid.WidthOf(GridColumns) / 2, Panel.Top + GridTop), GridColumns, GridRows, PriceHeight);
    private readonly FormPanel _panel = new();
    private readonly ItemIcons _icons = new();
    private readonly ItemTooltip _tooltip = new();

    /// <summary>What's been sold here this visit, which goes on the end of the shelf at what it fetched, and its wrapper for adding and taking.</summary>
    private readonly List<ItemStack> _soldHere = new();
    private readonly Inventory _soldBack;

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _font;
    private SpriteFont _detailFont;

    /// <summary>The bag as it's shown, sorted.</summary>
    private List<ItemStack> _bag = new();

    /// <summary>Which side the keys are on and where, whether its tooltip's up, what it was last made for, and where it goes.</summary>
    private ShopSide _side = ShopSide.Shelf;
    private int _index;
    private bool _showTip;
    private (ShopSide Side, int Index, int Member, int Version)? _tipFor;
    private Rectangle? _tipBeside;

    /// <summary>Goes up by one whenever anything's bought or sold, so a tooltip made before it is made again.</summary>
    private int _version;

    /// <summary>Which of the party the tooltips measure what's worn against.</summary>
    private int _compare;

    /// <summary>Whether anything's been bought, for whoever opened the shop.</summary>
    private bool _bought;

    /// <summary>What's just happened, for the bottom line, its colour, and how long it's been up.</summary>
    private string _saying;
    private Color _sayingColour;
    private float _sayingAge;

    /// <summary>Raised as the shop closes, with whether anything was bought.</summary>
    public event Action<bool> Closed;

    /// <summary>Opens a shop for a game.</summary>
    /// <param name="shop">The shop.</param>
    /// <param name="game">The game: its gold, its bag, and the party the tooltips measure against.</param>
    public ShopScreen(ShopDefinition shop, SaveData game)
    {
        IsPopup = true;
        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        _shop = shop;
        _game = game;
        _compare = game.Leader;
        _soldBack = new Inventory(_soldHere);
    }

    /// <summary>Loads the panel, the fonts, the icons and the tooltip, fills both sides, and rings a coin on the counter.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();

        _panel.LoadContent(_content, ScreenManager.GraphicsDevice);
        _icons.LoadContent(_content);
        _tooltip.LoadContent(_content, ScreenManager.BlankTexture);
        _shelf.LoadContent(ScreenManager.BlankTexture);
        _bagGrid.LoadContent(ScreenManager.BlankTexture);
        _font = _content.Load<SpriteFont>("spectral-ui");
        _detailFont = _content.Load<SpriteFont>("spectral-detail");

        Refill();
        _audio.Play(Sfx.Coin);
    }

    /// <summary>Unloads everything.</summary>
    public override void Unload() => _content?.Unload();

    /// <summary>Leaving, the side, who's measured against, the mouse, and the keys.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_close.Occurred(input))
        {
            _audio.Play(Sfx.MenuBack);
            ExitScreen();
            Closed?.Invoke(_bought);
            return;
        }

        if (_previousMember.Occurred(input)) Compare((_compare + _game.Party.Count - 1) % _game.Party.Count);
        if (_nextMember.Occurred(input)) Compare((_compare + 1) % _game.Party.Count);

        bool all = input.IsKeyPressed(Keys.LeftShift) || input.IsKeyPressed(Keys.RightShift);
        if (HandleMouse(input, all)) return;

        if (_switchSide.Occurred(input) && Focus(Other(_side), Grid(Other(_side)).EnterFrom(Grid(_side).CellAt(_index).Center.Y, _side == ShopSide.Shelf)))
        {
            _showTip = true;
            _audio.Play(Sfx.MenuMove);
        }

        int across = (_right.Occurred(input) ? 1 : 0) - (_left.Occurred(input) ? 1 : 0);
        int down = (_down.Occurred(input) ? 1 : 0) - (_up.Occurred(input) ? 1 : 0);
        if ((across != 0 || down != 0) && Move(across, across == 0 ? down : 0))
        {
            _showTip = true;
            _audio.Play(Sfx.MenuMove);
        }

        bool sellAll = _actAll.Occurred(input);
        if (_act.Occurred(input) || sellAll)
        {
            _showTip = true;
            Act(all || sellAll);
        }
    }

    /// <summary>Runs the bottom line's clock and puts the tooltip where it goes.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        _sayingAge += (float)gameTime.ElapsedGameTime.TotalSeconds;
        _tipBeside = PlaceTip();
    }

    /// <summary>Draws the veil, the panel and the line down it, both grids, the headings, the gold and the bottom line, then the tooltip.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        float alpha = TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _panel.DrawVeil(spriteBatch, alpha, FormPanel.FullVeil);
        _panel.Draw(spriteBatch, Panel, alpha);
        spriteBatch.Draw(ScreenManager.BlankTexture, new Rectangle(Panel.Center.X - DividerWidth / 2, Panel.Top + DividerInset, DividerWidth, Panel.Height - DividerInset * 2),
            DividerColour * alpha);
        _shelf.DrawCells(spriteBatch, _icons, _side == ShopSide.Shelf ? _index : -1, alpha);
        _bagGrid.DrawCells(spriteBatch, _icons, _side == ShopSide.Bag ? _index : -1, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        DrawHeadings(spriteBatch, alpha);
        _shelf.DrawWords(spriteBatch, _detailFont, alpha);
        _bagGrid.DrawWords(spriteBatch, _detailFont, alpha);
        DrawBottomLine(spriteBatch, alpha);
        spriteBatch.End();

        if (_tipBeside is not Rectangle beside) return;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _tooltip.DrawCard(spriteBatch, beside, alpha);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _tooltip.DrawWords(spriteBatch, beside, alpha);
        spriteBatch.End();
    }

    /// <summary>The mouse: the wheel scrolls whichever grid it's over, and pointing at or clicking a place on either side.</summary>
    /// <returns>True if it bought or sold something, which is the end of this frame's input.</returns>
    private bool HandleMouse(InputState input, bool all)
    {
        Point mouse = input.MousePosition;
        if (input.WheelTurn != 0)
        {
            if (_shelf.Bounds.Contains(mouse)) _shelf.ScrollBy(-input.WheelTurn);
            if (_bagGrid.Bounds.Contains(mouse)) _bagGrid.ScrollBy(-input.WheelTurn);
        }

        if (!input.MouseMoved && !input.IsNewLeftClick) return false;

        ShopSide side = ShopSide.Shelf;
        int index = _shelf.IndexAt(mouse);
        if (index < 0)
        {
            side = ShopSide.Bag;
            index = _bagGrid.IndexAt(mouse);
        }

        if (index < 0)
        {
            if (input.MouseMoved) _showTip = false;
            return false;
        }

        if (side != _side || index != _index) _audio.Play(Sfx.MenuMove);
        _side = side;
        _index = index;
        _showTip = true;

        if (!input.IsNewLeftClick) return false;
        Act(all);
        return true;
    }

    /// <summary>Moves the keys a step in the grid they're in, and across the middle into the other.</summary>
    /// <param name="across">-1 left, 1 right, or 0.</param>
    /// <param name="down">-1 up, 1 down, or 0. Only one of the two is ever set.</param>
    /// <returns>True if they moved.</returns>
    private bool Move(int across, int down)
    {
        ItemGrid grid = Grid(_side);
        int next = grid.Step(_index, across, down);
        if (next >= 0) return Focus(_side, next);

        bool outward = _side == ShopSide.Shelf ? across > 0 : across < 0;
        if (!outward) return false;
        return Focus(Other(_side), Grid(Other(_side)).EnterFrom(grid.CellAt(_index).Center.Y, fromLeft: across > 0));
    }

    /// <summary>Puts the keys on a place on one side, and scrolls to it.</summary>
    /// <returns>True if there was a place to put them on.</returns>
    private bool Focus(ShopSide side, int index)
    {
        ItemGrid grid = Grid(side);
        if (index < 0 || index >= grid.Count) return false;

        _side = side;
        _index = index;
        grid.Reveal(index);
        return true;
    }

    /// <summary>Changes who the tooltips measure against.</summary>
    private void Compare(int index)
    {
        _compare = index;
        _version++;
        _audio.Play(Sfx.MenuMove);
    }

    /// <summary>Buys or sells whatever the keys are on.</summary>
    /// <param name="all">For something in the bag, whether to sell all of it there, not just one.</param>
    private void Act(bool all)
    {
        if (_index >= Grid(_side).Count) return;
        if (_side == ShopSide.Shelf) Buy();
        else Sell(_bag[_index], all);
    }

    /// <summary>Buys one of what's on the shelf under the keys, if the party can afford it: from the stock at the shop's price, or back from what's been sold here at what it fetched.</summary>
    private void Buy()
    {
        bool back = _index >= _shop.Stock.Count;
        ItemDefinition item = ItemCatalog.Get(back ? _soldHere[_index - _shop.Stock.Count].Id : _shop.Stock[_index]);
        int price = back ? _shop.Offer(item) : _shop.Price(item);

        if (Trade.WhyNotBuy(_game, item, price) is string why)
        {
            Refuse(why);
            return;
        }

        Trade.Buy(_game, item.Id, price);
        if (back) _soldBack.Remove(item.Id);
        _bought = true;
        _audio.Play(Sfx.Buy);
        Say($"Bought the {item.Name} for {Gold(price)}.", Palette.Gold);
        AfterTrade();
    }

    /// <summary>Sells one of something in the bag, or all of it there.</summary>
    private void Sell(ItemStack stack, bool all)
    {
        ItemDefinition item = ItemCatalog.Get(stack.Id);
        if (Trade.WhyNotSell(_shop, item) is string why)
        {
            Refuse(why);
            return;
        }

        int count = all ? stack.Count : 1;
        int gold = Trade.Sell(_game, _shop, item.Id, count);
        _soldBack.Add(item.Id, count);
        _audio.Play(Sfx.Coin);
        Say(count == 1
            ? $"Sold the {item.Name} for {Gold(gold)}."
            : $"Sold {count.ToString(CultureInfo.InvariantCulture)} of the {item.Name} for {Gold(gold)}.", Palette.Gold);
        AfterTrade();
    }

    /// <summary>Says why a deal can't be done, with the sound for it.</summary>
    private void Refuse(string why)
    {
        _audio.Play(Sfx.MenuBack);
        Say(why, Palette.Ember);
    }

    /// <summary>Fills both sides again after a deal, and keeps the keys on something that's still there.</summary>
    private void AfterTrade()
    {
        _version++;
        Refill();

        ItemGrid grid = Grid(_side);
        if (_index < grid.Count) return;
        if (grid.Count > 0) Focus(_side, grid.Count - 1);
        else Focus(Other(_side), 0);
    }

    /// <summary>Fills the shelf (the stock, then what's been sold here) and the bag, each with its price, greying what the party can't afford.</summary>
    private void Refill()
    {
        var shelf = new List<GridEntry>(_shop.Stock.Count + _soldHere.Count);
        foreach (string id in _shop.Stock)
        {
            ItemDefinition item = ItemCatalog.Get(id);
            shelf.Add(Priced(item, 1, _shop.Price(item)));
        }
        foreach (ItemStack sold in _soldHere)
        {
            ItemDefinition item = ItemCatalog.Get(sold.Id);
            shelf.Add(Priced(item, sold.Count, _shop.Offer(item)));
        }
        _shelf.Fill(shelf);

        _bag = BagOrder.Of(_game.Inventory);
        var bag = new List<GridEntry>(_bag.Count);
        foreach (ItemStack stack in _bag)
        {
            ItemDefinition item = ItemCatalog.Get(stack.Id);
            int offer = _shop.Offer(item);
            bag.Add(new GridEntry(item, stack.Count, offer.ToString(CultureInfo.InvariantCulture), offer > 0 ? Palette.Gold : Palette.Note, false));
        }
        _bagGrid.Fill(bag);
    }

    /// <summary>A place on the shelf: its price in gold, or greyed with its price in red if the party can't afford it.</summary>
    private GridEntry Priced(ItemDefinition item, int count, int price)
    {
        bool dear = price > _game.Gold;
        return new GridEntry(item, count, price.ToString(CultureInfo.InvariantCulture), dear ? Palette.Ember : Palette.Gold, dear);
    }

    /// <summary>One side's grid.</summary>
    private ItemGrid Grid(ShopSide side) => side == ShopSide.Shelf ? _shelf : _bagGrid;

    /// <summary>The other side of the counter.</summary>
    private static ShopSide Other(ShopSide side) => side == ShopSide.Shelf ? ShopSide.Bag : ShopSide.Shelf;

    /// <summary>Some gold, the way the lines say it: "15 gold".</summary>
    private static string Gold(int amount) => amount.ToString(CultureInfo.InvariantCulture) + " gold";

    /// <summary>Puts a line on the bottom of the screen for a few seconds, in place of the keys.</summary>
    private void Say(string line, Color colour)
    {
        _saying = line;
        _sayingColour = colour;
        _sayingAge = 0f;
    }

    /// <summary>Where the tooltip goes, if there's one up, and makes it again first if what it's for has changed.</summary>
    /// <returns>The slot it's beside, or null for no tooltip.</returns>
    private Rectangle? PlaceTip()
    {
        ItemGrid grid = Grid(_side);
        if (!_showTip || _index >= grid.Count) return null;

        Rectangle slot = grid.CellAt(_index);
        if (!grid.Bounds.Contains(slot.Center)) return null;

        var tipFor = (_side, _index, _compare, _version);
        if (_tipFor != tipFor)
        {
            _tipFor = tipFor;
            MakeTip();
        }
        return slot;
    }

    /// <summary>Puts on the tooltip whatever the keys are on, with its price, measured against whoever's being compared.</summary>
    private void MakeTip()
    {
        PartyMember member = _game.Party[_compare];
        if (_side == ShopSide.Bag)
        {
            ItemStack stack = _bag[_index];
            ItemDefinition item = ItemCatalog.Get(stack.Id);
            int offer = _shop.Offer(item);
            string action = offer < 1 ? null : stack.Count > 1 ? SellAllAction : SellAction;
            _tooltip.ShowItem(item, member, item.SlotOn(member.Equipment), "SELLS FOR " + Gold(offer).ToUpperInvariant(), action);
            return;
        }

        bool back = _index >= _shop.Stock.Count;
        ItemDefinition onShelf = ItemCatalog.Get(back ? _soldHere[_index - _shop.Stock.Count].Id : _shop.Stock[_index]);
        int price = back ? _shop.Offer(onShelf) : _shop.Price(onShelf);
        string line = (back ? "BUY BACK  ·  " : "BUY  ·  ") + Gold(price).ToUpperInvariant();
        _tooltip.ShowItem(onShelf, member, onShelf.SlotOn(member.Equipment), line, price > _game.Gold ? null : BuyAction);
    }

    /// <summary>The shop's name over its shelf with how dear it is under it, and the bag's heading over the bag with the party's gold under it and who the tooltips measure against under the bag.</summary>
    private void DrawHeadings(SpriteBatch spriteBatch, float alpha)
    {
        float left = Panel.Left + Panel.Width / 4f, right = Panel.Right - Panel.Width / 4f;
        float top = Panel.Top + FormPanel.Padding, under = top + _font.LineSpacing;

        Text.Draw(spriteBatch, _font, _shop.Name.ToUpperInvariant(), left, top, Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        string prices = MathF.Abs(_shop.BuyMarkup - 1f) < FairTolerance
            ? FairPrices
            : string.Format(CultureInfo.InvariantCulture, PricesLine, MathF.Round(_shop.BuyMarkup * 100f));
        Text.Draw(spriteBatch, _detailFont, prices, left, under, Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);

        Text.Draw(spriteBatch, _font, BagHeading, right, top, Palette.Heading * alpha, Palette.FormShadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, Gold(_game.Gold).ToUpperInvariant(), right, under, Palette.Gold * alpha, Palette.FormShadow * alpha, centred: true);

        string compare = string.Format(CultureInfo.InvariantCulture, CompareLine, _game.Party[_compare].Name.ToUpperInvariant());
        Text.Draw(spriteBatch, _detailFont, compare, right, Panel.Top + CompareTop, Palette.Note * alpha, Palette.FormShadow * alpha, centred: true);
    }

    /// <summary>The line along the bottom of the screen: what's just happened, fading back to the keys.</summary>
    private void DrawBottomLine(SpriteBatch spriteBatch, float alpha)
    {
        if (_saying is not null && _sayingAge < SayingSeconds)
        {
            float shown = MathHelper.Clamp((SayingSeconds - _sayingAge) / SayingFade, 0f, 1f) * alpha;
            Text.DrawCentered(spriteBatch, _detailFont, _saying, HintTop, _sayingColour * shown, Palette.FormShadow * shown);
            return;
        }

        Text.DrawCentered(spriteBatch, _detailFont, Hint, HintTop, Palette.Note * alpha, Palette.FormShadow * alpha);
    }
}
