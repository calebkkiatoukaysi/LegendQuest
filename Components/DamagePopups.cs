using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>What a number or word popping up over someone in a fight is, which says its colour, size and how it moves.</summary>
public enum PopupKind
{
    /// <summary>Damage dealt by your side: white.</summary>
    Dealt,

    /// <summary>A critical: gold, bigger, popping in from larger and bouncing once.</summary>
    Critical,

    /// <summary>Damage your side takes: red.</summary>
    Taken,

    /// <summary>Healing: green with a "+", rising slower.</summary>
    Heal,

    /// <summary>A tick of damage or healing over time: smaller, in the element's tint.</summary>
    Tick,

    /// <summary>MISS, DODGE or BLOCK: grey, smaller, no pop.</summary>
    Avoided,

    /// <summary>Any other word or number, in the colour it's given (the old screen's calls).</summary>
    Other,
}

/// <summary>
/// The numbers and words that pop up over someone in a fight: damage, healing, MISS, DODGE, BLOCK,
/// CRITICAL, a condition going on, a death save.
/// </summary>
/// <remarks>
/// Each one is pinned to a point on the map, so it stays over its owner however the camera moves,
/// and drawn on the screen at the screen's own size so it's always crisp. It pops (in big and
/// settling), rises as it eases, and fades out at the end. Numbers are in the big Spectral digits
/// of spectral-number; words in the UI font. The colours follow the convention players know: white
/// for damage dealt, gold and bigger with a bounce for a critical, red for damage taken, green with a
/// "+" for healing, grey words for what missed, and smaller numbers in the element's tint for ticks
/// over time. Several over the same person close together stack up rather than sitting on each
/// other, and alternate a little left and right, so a flurry's hits all read. Over one person at
/// most MostPerTarget are up at once (given by who they are): past that a new number is added into
/// the newest one of its kind instead, which pops again with the sum. Numbers can be turned off in
/// the options (Shown); the words still come up.
/// </remarks>
public class DamagePopups
{
    /// <summary>How long a number and a word stay up, in seconds.</summary>
    private const float NumberSeconds = 1.15f;
    private const float WordSeconds = 1.05f;

    /// <summary>How far one rises over its life, in screen pixels, and how much slower healing rises.</summary>
    private const float Rise = 48f;
    private const float HealRise = 0.7f;

    /// <summary>How long the pop takes to settle, and how much bigger it starts.</summary>
    private const float PopSeconds = 0.14f;
    private const float PopOvershoot = 0.45f;

    /// <summary>A critical's pop: how much bigger it starts, how long it takes to come down with a back ease, then its one bounce, how high in screen pixels and how long.</summary>
    private const float CriticalStart = 1.6f;
    private const float CriticalPopSeconds = 0.12f;
    private const float BounceHeight = 9f;
    private const float BounceSeconds = 0.22f;

    /// <summary>How much longer a running total stays up after another number's added into it, in seconds.</summary>
    private const float MergedSeconds = 0.8f;

    /// <summary>How far through its life one starts to fade.</summary>
    private const float FadeFrom = 0.65f;

    /// <summary>How big a number, a critical's number, a tick and a word are drawn, against their fonts' own size.</summary>
    private const float NumberScale = 0.62f;
    private const float CriticalScale = 0.9f;
    private const float TickScale = 0.5f;
    private const float WordScale = 0.78f;
    private const float AvoidedScale = 0.62f;

    /// <summary>How far up a new one goes past one over the same person that's only just come up, in screen pixels, and how close together counts.</summary>
    private const float StackGap = 30f;
    private const float StackSeconds = 0.45f;
    private const float SameSpot = 4f;

    /// <summary>How far left or right of the middle each one over someone sits, by turns, in screen pixels.</summary>
    private const float Fan = 12f;

    /// <summary>The most up over one person at once; any more are added into one already up.</summary>
    public const int MostPerTarget = 6;

    /// <summary>How far down and right the shadow sits, at a number's size.</summary>
    private static readonly Vector2 ShadowOffset = new(3f, 3f);

    private static readonly Color ShadowColour = Color.Black * 0.75f;

    /// <summary>The colours: damage dealt, a critical, damage taken, healing, and a word for what missed.</summary>
    public static readonly Color DealtColour = new(250, 248, 240);
    public static readonly Color CriticalColour = new(255, 204, 64);
    public static readonly Color TakenColour = new(240, 64, 52);
    public static readonly Color HealColour = new(96, 228, 110);
    public static readonly Color AvoidedColour = new(176, 176, 184);

    private readonly List<Popup> _popups = new();
    private readonly Dictionary<string, int> _fans = new();

    private SpriteFont _numbers;
    private SpriteFont _words;

    /// <summary>Whether numbers come up (the options' damage numbers); words always do.</summary>
    public bool Shown { get; set; } = true;

    /// <summary>One popup: whose it is, where it's pinned, what it says, its kind and colour, whether it's in the number font, its size, how long it's been up and since it last popped, how long it lasts and how long it rises, how far up it's stacked and across it's fanned, and the amount a number carries.</summary>
    private sealed class Popup
    {
        public string Owner;
        public Vector2 Anchor;
        public string Text;
        public PopupKind Kind;
        public Color Colour;
        public bool Number;
        public float Scale;
        public float Clock;
        public float PopClock;
        public float Life;
        public float RiseLife;
        public float Lift;
        public float Across;
        public int Amount;
        public string Sign;
    }

    /// <summary>Loads the two fonts.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _numbers = content.Load<SpriteFont>("spectral-number");
        _words = content.Load<SpriteFont>("spectral-ui");
    }

    /// <summary>Pops up a number: damage with a minus, healing with a plus. The old screen's call, in the colour it gives.</summary>
    /// <param name="anchor">The point it's over, in world pixels: the top of someone's head.</param>
    /// <param name="amount">How much.</param>
    /// <param name="heal">Whether it's healing.</param>
    /// <param name="critical">Whether it's a critical's, drawn bigger.</param>
    /// <param name="colour">Its colour.</param>
    public void Number(Vector2 anchor, int amount, bool heal, bool critical, Color colour)
    {
        if (!Shown) return;
        Add(null, anchor, heal ? "+" : "-", amount, PopupKind.Other, colour, true, critical ? CriticalScale : NumberScale, NumberSeconds);
    }

    /// <summary>Pops up a word or two: MISS, CRITICAL, SLOWED.</summary>
    /// <param name="anchor">The point it's over, in world pixels.</param>
    /// <param name="text">What it says, in capitals.</param>
    /// <param name="colour">Its colour.</param>
    public void Word(Vector2 anchor, string text, Color colour) => Add(null, anchor, text, 0, PopupKind.Other, colour, false, WordScale, WordSeconds);

    /// <summary>Pops up damage over someone: white for damage your side deals, red for damage it takes, a critical gold (or a deeper red), bigger and bouncing.</summary>
    /// <param name="owner">Who it's over (their id in the fight), for the most at once; null for no limit.</param>
    /// <param name="anchor">The point it's over, in world pixels: the top of their head.</param>
    /// <param name="amount">How much.</param>
    /// <param name="critical">Whether it was a critical.</param>
    /// <param name="taken">Whether it's damage to your side.</param>
    public void Damage(string owner, Vector2 anchor, int amount, bool critical, bool taken)
    {
        if (!Shown) return;
        PopupKind kind = critical ? PopupKind.Critical : taken ? PopupKind.Taken : PopupKind.Dealt;
        Color colour = taken ? critical ? new Color(255, 96, 60) : TakenColour : critical ? CriticalColour : DealtColour;
        Add(owner, anchor, "", amount, kind, colour, true, critical ? CriticalScale : NumberScale, NumberSeconds);
    }

    /// <summary>Pops up healing over someone: green with a "+", rising slower.</summary>
    /// <param name="owner">Who it's over.</param>
    /// <param name="anchor">The point it's over, in world pixels.</param>
    /// <param name="amount">How much.</param>
    public void Heal(string owner, Vector2 anchor, int amount)
    {
        if (!Shown) return;
        Add(owner, anchor, "+", amount, PopupKind.Heal, HealColour, true, NumberScale, NumberSeconds);
    }

    /// <summary>Pops up a tick of damage or healing over time: smaller, in the element's tint (or green for healing).</summary>
    /// <param name="owner">Who it's over.</param>
    /// <param name="anchor">The point it's over, in world pixels.</param>
    /// <param name="amount">How much.</param>
    /// <param name="tint">The element's colour (CombatEffects.ColourOf).</param>
    /// <param name="heal">Whether it's healing.</param>
    public void Tick(string owner, Vector2 anchor, int amount, Color tint, bool heal = false)
    {
        if (!Shown) return;
        Add(owner, anchor, heal ? "+" : "", amount, PopupKind.Tick, heal ? HealColour : tint, true, TickScale, NumberSeconds * 0.9f);
    }

    /// <summary>Pops up MISS, DODGE or BLOCK over someone, in grey, small, without a pop.</summary>
    /// <param name="owner">Who it's over.</param>
    /// <param name="anchor">The point it's over, in world pixels.</param>
    /// <param name="word">The word, in capitals.</param>
    public void Avoided(string owner, Vector2 anchor, string word) => Add(owner, anchor, word, 0, PopupKind.Avoided, AvoidedColour, false, AvoidedScale, WordSeconds);

    /// <summary>How many are up over someone now.</summary>
    /// <param name="owner">Who.</param>
    public int CountOver(string owner)
    {
        int count = 0;
        foreach (Popup popup in _popups)
            if (popup.Owner == owner) count++;
        return count;
    }

    /// <summary>Runs every popup's clock and lets the finished ones go.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime) => Update((float)gameTime.ElapsedGameTime.TotalSeconds);

    /// <summary>Runs every popup's clock and lets the finished ones go.</summary>
    /// <param name="seconds">Seconds since the last update.</param>
    public void Update(float seconds)
    {
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            _popups[i].Clock += seconds;
            _popups[i].PopClock += seconds;
            if (_popups[i].Clock >= _popups[i].Life) _popups.RemoveAt(i);
        }
    }

    /// <summary>Takes every popup down, when the fight's over.</summary>
    public void Clear()
    {
        _popups.Clear();
        _fans.Clear();
    }

    /// <summary>Draws every popup over its point, rising and fading.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="toScreen">Where a point on the map is on the screen: the camera's ToScreen.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void Draw(SpriteBatch spriteBatch, Func<Vector2, Vector2> toScreen, float alpha)
    {
        foreach (Popup popup in _popups)
        {
            float t = popup.Clock / popup.Life;
            // It rises over the life it came up with, so a total that's added to stays where it is.
            float risen = MathF.Min(1f, popup.Clock / popup.RiseLife);
            float eased = 1f - (1f - risen) * (1f - risen);
            float pop = Pop(popup);
            float hop = popup.Kind == PopupKind.Critical ? Bounce(popup.PopClock - CriticalPopSeconds) : 0f;
            float rise = Rise * eased * (popup.Kind == PopupKind.Heal ? HealRise : 1f);
            float shown = (t < FadeFrom ? 1f : 1f - (t - FadeFrom) / (1f - FadeFrom)) * alpha;

            SpriteFont font = popup.Number ? _numbers : _words;
            float scale = popup.Scale * pop;
            Vector2 size = font.MeasureString(popup.Text) * scale;
            Vector2 at = toScreen(popup.Anchor) - new Vector2(size.X / 2f - popup.Across, size.Y + popup.Lift + rise + hop);
            at = new Vector2(MathF.Round(at.X), MathF.Round(at.Y));

            spriteBatch.DrawString(font, popup.Text, at + ShadowOffset * popup.Scale, ShadowColour * shown, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, popup.Text, at, popup.Colour * shown, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>How much bigger than its size one is drawn now: a quick pop that settles; a critical's starts bigger and comes down with a back ease; a word for a miss doesn't pop.</summary>
    private static float Pop(Popup popup)
    {
        switch (popup.Kind)
        {
            case PopupKind.Avoided:
                return 1f;
            case PopupKind.Critical:
            {
                if (popup.PopClock >= CriticalPopSeconds) return 1f;
                float u = popup.PopClock / CriticalPopSeconds, w = u - 1f;
                const float s = 1.70158f;
                float f = 1f + (s + 1f) * w * w * w + s * w * w;
                return CriticalStart + (1f - CriticalStart) * f;
            }
            default:
                return popup.PopClock < PopSeconds ? 1f + PopOvershoot * (1f - popup.PopClock / PopSeconds) : 1f;
        }
    }

    /// <summary>A critical's one bounce after its pop: up and back down, in screen pixels.</summary>
    private static float Bounce(float clock) =>
        clock <= 0f || clock >= BounceSeconds ? 0f : BounceHeight * MathF.Sin(MathF.PI * clock / BounceSeconds);

    /// <summary>
    /// Adds one, stacked over any at the same spot that's only just come up and fanned left or right
    /// by turns; or, if its owner has the most up already, adds a number into the newest of its kind
    /// over them (or the newest number at all), which pops again with the sum where it is and stays up
    /// a little longer. A critical is never added into another nor anything into it: it keeps its own
    /// gold, and the oldest over them that isn't one makes room for it, as for a word.
    /// </summary>
    private void Add(string owner, Vector2 anchor, string sign, int amount, PopupKind kind, Color colour, bool number, float scale, float life)
    {
        if (owner is not null && CountOver(owner) >= MostPerTarget)
        {
            Popup into = null;
            if (number && kind != PopupKind.Critical)
            {
                for (int i = _popups.Count - 1; i >= 0 && into is null; i--)
                    if (_popups[i].Owner == owner && _popups[i].Number && _popups[i].Kind == kind) into = _popups[i];
                for (int i = _popups.Count - 1; i >= 0 && into is null; i--)
                    if (_popups[i].Owner == owner && _popups[i].Number && _popups[i].Kind != PopupKind.Critical) into = _popups[i];
            }
            if (into is not null)
            {
                into.Amount += amount;
                into.Text = into.Sign + into.Amount;
                into.PopClock = 0f;
                into.Life = MathF.Max(into.Life, into.Clock + MergedSeconds);
                return;
            }
            // Nothing to add it into: the oldest over them makes room, a critical last.
            int oldest = -1;
            for (int i = 0; i < _popups.Count && oldest < 0; i++)
                if (_popups[i].Owner == owner && _popups[i].Kind != PopupKind.Critical) oldest = i;
            for (int i = 0; i < _popups.Count && oldest < 0; i++)
                if (_popups[i].Owner == owner) oldest = i;
            if (oldest >= 0) _popups.RemoveAt(oldest);
        }

        float lift = 0f;
        foreach (Popup other in _popups)
            if (other.Clock < StackSeconds && Vector2.Distance(other.Anchor, anchor) <= SameSpot) lift = MathF.Max(lift, other.Lift + StackGap);

        float across = 0f;
        if (owner is not null)
        {
            int turn = _fans.GetValueOrDefault(owner);
            _fans[owner] = turn + 1;
            across = turn % 2 == 0 ? Fan * (turn % 4 == 0 ? -0.5f : 0.5f) : Fan * (turn % 4 == 1 ? 1f : -1f);
        }

        string text = number ? sign + amount : sign;
        _popups.Add(new Popup
        {
            Owner = owner, Anchor = anchor, Text = text, Kind = kind, Colour = colour, Number = number, Scale = scale, Life = life, RiseLife = life, Lift = lift,
            Across = across, Amount = amount, Sign = number ? sign : "",
        });
    }
}
