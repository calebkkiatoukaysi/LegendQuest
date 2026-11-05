using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The turn order across the top of the screen in a fight: a portrait for everyone, in the order
/// they go, with their health under it and what's on them, the round beside it, and after the last
/// a banner for each support ("ODELLE'S CREW  ·  END OF ROUND"), since they act when the round ends.
/// </summary>
/// <remarks>
/// Spec 6.7's initiative bar, as 11-combat §4's turn-order line. It sits along the top rather than
/// over the bar so it never covers the front row's feet and plates. It fills in a portrait at a time
/// as the initiative dice are shown.
/// The party's portraits are framed in gold, the foes' in red, and whoever's turn it is sits lower
/// in the bright frame with a pointer over them, so whose turn it is reads at a glance. A bar under
/// each drains as the damage is shown, its lost part fading after the rest. Under that go the first
/// two conditions (the card says the rest), or for someone Downed their death saves: three
/// sockets for successes and three for failures. Someone fallen is dark. Pointing at a portrait
/// lights it and the card says who it is.
/// </remarks>
public class InitiativeBar
{
    /// <summary>How much the hud art is blown up.</summary>
    private const float Scale = 2f;

    /// <summary>A portrait's frame in art pixels, and the room between two on screen.</summary>
    private const int FrameWidth = 30;
    private const int FrameHeight = 32;
    private const int Gap = 8;

    /// <summary>How far down from the top the portraits are (under the fight's name and the keys in the corner, so nothing on the line meets them), and how much further down whoever's turn it is sits, in screen pixels.</summary>
    private const int Top = 58;

    /// <summary>The room kept left of the first portrait for the round, and the least room left of that, in screen pixels.</summary>
    private const int RoundRoom = 120;
    private const int SideMargin = 24;
    private const int CurrentDrop = 8;

    /// <summary>The health bar under a portrait: how far under it, and how tall, in screen pixels.</summary>
    private const int BarGap = 4;
    private const int BarHeight = 6;

    /// <summary>How fast a bar's lost part catches up with the rest, as a share of the bar a second.</summary>
    private const float DrainRate = 0.9f;

    /// <summary>How many conditions show under a portrait before the rest are counted.</summary>
    private const int ConditionsShown = 2;

    /// <summary>How big a condition icon and a death save socket are drawn.</summary>
    private const float ConditionScale = 2f;
    private const float SocketScale = 1f;

    /// <summary>The room under the bar before the conditions or the death saves, in screen pixels.</summary>
    private const int UnderBar = 3;

    /// <summary>How many death saves of each kind a track holds.</summary>
    private const int Saves = 3;

    /// <summary>The pointer over whoever's turn it is: how wide and tall, in screen pixels.</summary>
    private const int PointerWidth = 12;
    private const int PointerHeight = 6;

    /// <summary>The light round a portrait that's pointed at, in screen pixels.</summary>
    private const int HoverEdge = 2;

    private static readonly Color BarBack = new(16, 14, 24);
    private static readonly Color BarLost = new Color(255, 236, 200) * 0.7f;

    /// <summary>The light round a portrait the mouse is on.</summary>
    private static readonly Color HoverLight = Palette.Bone * 0.8f;

    /// <summary>The room between the pointer and the top of the portrait it points at, in screen pixels.</summary>
    private const int PointerGap = 2;

    /// <summary>The room between the last portrait and the first support's banner, and between two banners, in screen pixels.</summary>
    private const int SupportGap = 10;

    /// <summary>What a support's entry says after its name.</summary>
    private const string EndOfRound = "END OF ROUND";

    /// <summary>How full each bar's lost part still is, by who it's for.</summary>
    private readonly Dictionary<string, float> _drained = new();

    /// <summary>The supports at the end of the line, as last given.</summary>
    private IReadOnlyList<SupportView> _supports = Array.Empty<SupportView>();

    private CombatArt _art;
    private Texture2D _pixel;
    private SpriteFont _font;
    private SupportBanner _banner;

    /// <summary>Loads the font, and keeps the shared art, the pixel the bars are drawn with, and the banner the supports' entries are drawn with.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="art">The fight's sheets.</param>
    /// <param name="pixel">One white pixel: the screen manager's BlankTexture.</param>
    /// <param name="banner">The supports' banner, loaded.</param>
    public void LoadContent(ContentManager content, CombatArt art, Texture2D pixel, SupportBanner banner)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _art = art;
        _pixel = pixel;
        _banner = banner;
    }

    /// <summary>Says which supports go at the end of the line. Call before At, Portrait or the draws, each frame.</summary>
    /// <param name="supports">The supports, in the order they act.</param>
    public void SetSupports(IReadOnlyList<SupportView> supports) => _supports = supports ?? Array.Empty<SupportView>();

    /// <summary>Lets every bar's lost part catch up with what's left.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="order">Everyone on the bar.</param>
    public void Update(GameTime gameTime, IReadOnlyList<FighterView> order)
    {
        float step = DrainRate * (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (FighterView fighter in order)
        {
            float health = Health(fighter);
            float drained = _drained.GetValueOrDefault(fighter.Id, health);
            _drained[fighter.Id] = drained > health ? MathF.Max(health, drained - step) : health;
        }
    }

    /// <summary>Forgets every bar and every support, for a new fight.</summary>
    public void Clear()
    {
        _drained.Clear();
        _supports = Array.Empty<SupportView>();
    }

    /// <summary>Which portrait the mouse is on.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="count">How many are on the bar.</param>
    /// <param name="current">Which is whoever's turn it is, or -1.</param>
    /// <returns>Its place on the bar, or null.</returns>
    public int? At(Point mouse, int count, int current)
    {
        for (int i = 0; i < count; i++)
            if (Portrait(i, count, current).Contains(mouse)) return i;
        return null;
    }

    /// <summary>Where a portrait is on screen: the portraits and the supports' banners after them centred across the top.</summary>
    /// <param name="index">Its place on the bar.</param>
    /// <param name="count">How many are on the bar.</param>
    /// <param name="current">Which is whoever's turn it is, or -1.</param>
    public Rectangle Portrait(int index, int count, int current)
    {
        int width = (int)(FrameWidth * Scale), height = (int)(FrameHeight * Scale);
        int left = Start(count) + index * (width + Gap);
        return new Rectangle(left, Top + (index == current ? CurrentDrop : 0), width, height);
    }

    /// <summary>Where each support's banner is on screen: after the last portrait, level with the portraits' middles.</summary>
    /// <param name="count">How many portraits are on the bar.</param>
    public Rectangle[] SupportBanners(int count)
    {
        int width = (int)(FrameWidth * Scale), height = (int)(FrameHeight * Scale);
        var banners = new Rectangle[_supports.Count];
        int x = Start(count) + count * (width + Gap) - Gap + SupportGap;
        for (int i = 0; i < banners.Length; i++)
        {
            int bannerWidth = _banner.Width(Entry(_supports[i]));
            banners[i] = new Rectangle(x, Top + height / 2 - SupportBanner.Height / 2, bannerWidth, SupportBanner.Height);
            x += bannerWidth + SupportGap;
        }
        return banners;
    }

    /// <summary>Which support's banner the mouse is on.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="count">How many portraits are on the bar.</param>
    /// <returns>Its place among the supports, or null.</returns>
    public int? SupportAt(Point mouse, int count)
    {
        Rectangle[] banners = SupportBanners(count);
        for (int i = 0; i < banners.Length; i++)
            if (banners[i].Contains(mouse)) return i;
        return null;
    }

    /// <summary>What a support's entry on the line says.</summary>
    private static string Entry(SupportView support) => $"{support.Name}  ·  {EndOfRound}";

    /// <summary>Where the first portrait starts across the screen, so the portraits and the banners after them are centred.</summary>
    private int Start(int count)
    {
        int width = (int)(FrameWidth * Scale);
        int whole = RoundRoom + count * width + Math.Max(0, count - 1) * Gap;
        foreach (SupportView support in _supports) whole += SupportGap + _banner.Width(Entry(support));
        return Math.Max(SideMargin, (LegendQuestGame.ScreenWidth - whole) / 2) + RoundRoom;
    }

    /// <summary>Draws the portraits, their bars, their conditions and the pointer. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="order">Everyone on the bar, in turn order.</param>
    /// <param name="current">Which is whoever's turn it is, or -1.</param>
    /// <param name="hovered">Which the mouse is on, or -1.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawArt(SpriteBatch spriteBatch, IReadOnlyList<FighterView> order, int current, int hovered, float alpha)
    {
        for (int i = 0; i < order.Count; i++)
        {
            FighterView fighter = order[i];
            Rectangle frame = Portrait(i, order.Count, current);

            if (i == hovered)
            {
                Rectangle edge = frame;
                edge.Inflate(HoverEdge, HoverEdge);
                spriteBatch.Draw(_pixel, edge, HoverLight * alpha);
            }

            string piece = i == current ? "portrait-current" : fighter.Foe ? "portrait-foe" : "portrait-party";
            _art.Piece(spriteBatch, piece, frame.Location.ToVector2(), Scale, Color.White * alpha);

            int inset = (int)(CombatIcons.FrameInset * Scale);
            var well = new Rectangle(frame.X + inset, frame.Y + inset, (int)(CombatArt.Bust.Width * Scale), (int)(CombatArt.Bust.Height * Scale));
            CombatArt.Face(spriteBatch, fighter, well, alpha);

            var bar = new Rectangle(frame.X, frame.Bottom + BarGap, frame.Width, BarHeight);
            if (!fighter.Fallen) DrawBar(spriteBatch, fighter, bar, alpha);

            Vector2 under = new(frame.X, bar.Bottom + UnderBar);
            if (fighter.Down) DrawDeathSaves(spriteBatch, fighter, under, alpha);
            else DrawConditions(spriteBatch, fighter, under, alpha);

            if (i == current)
            {
                int middle = frame.Center.X;
                for (int row = 0; row < PointerHeight; row++)
                    spriteBatch.Draw(_pixel, new Rectangle(middle - PointerWidth / 2 + row, frame.Top - PointerHeight - PointerGap + row, PointerWidth - row * 2, 1), Palette.Gold * alpha);
            }
        }

        Rectangle[] banners = SupportBanners(order.Count);
        for (int i = 0; i < banners.Length; i++)
            _banner.DrawArt(spriteBatch, banners[i], _supports[i].Icon, _supports[i].Lit, null, alpha);
    }

    /// <summary>Writes the round to the left of the portraits, and the count of any conditions past the first two. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="order">Everyone on the bar, in turn order.</param>
    /// <param name="current">Which is whoever's turn it is, or -1.</param>
    /// <param name="round">The round, or 0 before the first.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, IReadOnlyList<FighterView> order, int current, int round, float alpha)
    {
        if (order.Count == 0) return;

        Rectangle first = Portrait(0, order.Count, current);
        if (round > 0)
        {
            string text = "ROUND " + round;
            Text.Draw(spriteBatch, _font, text, first.Left - Gap * 2, first.Top + (first.Height - _font.LineSpacing) / 2f,
                Palette.Gold * alpha, Palette.Shadow * alpha, rightAligned: true);
        }

        for (int i = 0; i < order.Count; i++)
        {
            int extra = order[i].Conditions.Count - ConditionsShown;
            if (order[i].Down || extra <= 0) continue;

            Rectangle frame = Portrait(i, order.Count, current);
            float x = frame.X + ConditionsShown * CombatIcons.ConditionSize * ConditionScale + 2f;
            float y = frame.Bottom + BarGap + BarHeight + UnderBar;
            Text.Draw(spriteBatch, _font, "+" + extra, x, y, Palette.Bone * alpha, Palette.Shadow * alpha);
        }

        Rectangle[] banners = SupportBanners(order.Count);
        for (int i = 0; i < banners.Length; i++)
            _banner.DrawWords(spriteBatch, banners[i], Entry(_supports[i]), _supports[i].Lit, alpha);
    }

    /// <summary>Someone's share of their hit points, 0 to 1.</summary>
    private static float Health(FighterView fighter) => fighter.MaxHp > 0 ? MathHelper.Clamp((float)fighter.Hp / fighter.MaxHp, 0f, 1f) : 0f;

    /// <summary>A health bar: the dark back, the part just lost fading out of it, and what's left in green or red.</summary>
    private void DrawBar(SpriteBatch spriteBatch, FighterView fighter, Rectangle bar, float alpha)
    {
        float health = Health(fighter);
        float drained = _drained.GetValueOrDefault(fighter.Id, health);
        spriteBatch.Draw(_pixel, bar, BarBack * alpha);
        spriteBatch.Draw(_pixel, new Rectangle(bar.X, bar.Y, (int)MathF.Round(bar.Width * drained), bar.Height), BarLost * alpha);
        Color fill = fighter.Foe ? CombatStyle.FoeHealth : CombatStyle.PartyHealth;
        spriteBatch.Draw(_pixel, new Rectangle(bar.X, bar.Y, (int)MathF.Round(bar.Width * health), bar.Height), fill * alpha);
    }

    /// <summary>The first two conditions on someone, side by side.</summary>
    private void DrawConditions(SpriteBatch spriteBatch, FighterView fighter, Vector2 at, float alpha)
    {
        for (int i = 0; i < Math.Min(ConditionsShown, fighter.Conditions.Count); i++)
            _art.Condition(spriteBatch, fighter.Conditions[i], at + new Vector2(i * CombatIcons.ConditionSize * ConditionScale, 0f), ConditionScale, alpha);
    }

    /// <summary>Someone Downed's death saves: three sockets for successes, then three for failures.</summary>
    private void DrawDeathSaves(SpriteBatch spriteBatch, FighterView fighter, Vector2 at, float alpha)
    {
        Rectangle socket = CombatIcons.Piece("death-save-empty");
        float step = socket.Width * SocketScale;
        for (int i = 0; i < Saves; i++)
        {
            _art.Piece(spriteBatch, i < fighter.DeathSuccesses ? "death-save-success" : "death-save-empty", at + new Vector2(i * step, 0f), SocketScale, Color.White * alpha);
            _art.Piece(spriteBatch, i < fighter.DeathFailures ? "death-save-failure" : "death-save-empty", at + new Vector2((Saves + i) * step, 0f), SocketScale, Color.White * alpha);
        }
    }
}
