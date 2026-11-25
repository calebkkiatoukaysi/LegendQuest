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
/// <para>
/// Each one is pinned to a point on the map, so it stays over its owner however the camera moves,
/// and follows its owner if they move (when the screen says where they are), so it isn't left
/// behind over whoever steps in where they were; it's drawn on the screen at the screen's own size
/// so it's always crisp. It pops (in big and settling), rises as it eases, and fades out at the
/// end. Numbers are in the big Spectral digits of spectral-number; words in the UI font. The
/// colours follow the convention players know: white for damage dealt, gold and bigger with a
/// bounce for a critical, red for damage taken, green with a "+" for healing, grey words for what
/// missed, and smaller numbers in the element's tint for ticks over time. Several over one person
/// alternate a little left and right. Over one person at most MostPerTarget are up at once (given
/// by who they are): past that a new number is added into the newest one of its kind instead, which
/// pops again with the sum. Numbers can be turned off in the options (Shown); the words still come
/// up.
/// </para>
/// <para>
/// None is drawn over another. Each holds a box on the screen as big as it's drawn (the pop and a
/// critical's bounce in it, with room round it), and is drawn inside that box. A new one takes the
/// nearest free place to the top of its owner's nameplate (or head): up first, then aside, never
/// below where it starts, and not nearer anyone else's nameplate than its own; further off only if
/// there's no room near at all (a banner's across it). Once up, one keeps its place, rising with
/// the rest; the older ones are seen to first each frame, so a new one never pushes an old one
/// about. If what the screen says to keep off (the HUD's frames, the nameplates, the banner, the
/// barks) or the screen's edge comes over it, it's nudged clear a few pixels a frame. With no room
/// near, one that's only just come up is moved at once (or, a number with nowhere at all, added
/// into a running total of the same kind over the same owner, which pops again with the sum); one
/// that's been up long enough to be read, or a word with nowhere, fades out quickly where it is, as
/// moved far it would read as another. One not yet up that has no room waits for some, and so does
/// a callout (a word popped up with Word: a skill's name, INTERRUPTED, TAUNTED) while Holding (a
/// banner's across the middle of the screen); either is dropped if it's waited too long to mean
/// anything. A pop grows only into room nobody else holds; with none it's drawn at its settled
/// size.
/// </para>
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

    /// <summary>How big a number, a critical's number, a tick, a callout and a word for what missed are drawn, against their fonts' own size.</summary>
    private const float NumberScale = 0.62f;
    private const float CriticalScale = 0.9f;
    private const float TickScale = 0.5f;
    private const float WordScale = 0.58f;
    private const float AvoidedScale = 0.56f;

    /// <summary>How far left or right of the middle each one over someone sits, by turns, in screen pixels.</summary>
    private const float Fan = 12f;

    /// <summary>How far over the top of its owner's nameplate the foot of one's letters starts, in screen pixels.</summary>
    private const float OverPlate = 6f;

    /// <summary>How far one may be moved up from where it would rise to, to make room, and aside each way, and the steps tried up and aside, in screen pixels. Down it may go only as far as it's risen, so never under where it starts.</summary>
    private const int MostClimb = 96;
    private const int MostAside = 96;
    private const int Step = 4;
    private const int AsideStep = 8;

    /// <summary>How far one may go up and aside when there's no room within the usual reach (a banner's across it), before a number's added into a total or one waits, in screen pixels.</summary>
    private const int WideClimb = 240;
    private const int WideAside = 192;

    /// <summary>What a pixel of moving aside and of moving down costs against a pixel of moving up, so one goes up before it goes aside, and aside before it goes down; and what a pixel of moving from where it was last frame costs, so one that's up stays put.</summary>
    private const float AsideCost = 2f;
    private const float DownCost = 4f;
    private const float StayCost = 5f;

    /// <summary>The room kept clear either side of each one (wide, so two side by side never read as one number) and over and under it, and between one and the screen's edge, in screen pixels.</summary>
    private const int RoomAcross = 8;
    private const int RoomUp = 3;
    private const int Edge = 4;

    /// <summary>How fast one that's up is nudged clear of what comes over it, in screen pixels a second, the least and most it's nudged in a frame, and the step of the nudges tried.</summary>
    private const float NudgeSpeed = 960f;
    private const int LeastNudge = 2;
    private const int MostNudge = 20;
    private const int NudgeStep = 2;

    /// <summary>How long one that has to give way takes to fade out where it is, in seconds; and how long one's been up before it's taken to have been read, so it gives way rather than being moved far (moved, it would read as another).</summary>
    private const float YieldSeconds = 0.25f;
    private const float ReadSeconds = 0.35f;

    /// <summary>The longest one waits to come up (a callout for a banner to go, or any for room) before it's dropped, in seconds.</summary>
    private const float MostWait = 2.5f;

    /// <summary>The most up over one person at once; any more are added into one already up.</summary>
    public const int MostPerTarget = 6;

    /// <summary>How far down and right the shadow sits, at a number's size.</summary>
    private static readonly Vector2 ShadowOffset = new(3f, 3f);

    private static readonly Color ShadowColour = Palette.LabelShadow;

    /// <summary>The colours: damage dealt, a critical, damage taken, healing, and a word for what missed.</summary>
    public static readonly Color DealtColour = new(250, 248, 240);
    public static readonly Color CriticalColour = new(255, 204, 64);
    public static readonly Color TakenColour = new(240, 64, 52);
    public static readonly Color HealColour = new(96, 228, 110);
    public static readonly Color AvoidedColour = new(176, 176, 184);

    private readonly List<Popup> _popups = new();
    private readonly Dictionary<string, int> _fans = new();

    /// <summary>The popups in the order they pick their places, and those placed so far this frame with the boxes they hold; kept to save making them every frame.</summary>
    private readonly List<Popup> _order = new();
    private readonly List<Popup> _placed = new();
    private readonly List<Rectangle> _taken = new();

    /// <summary>The moves tried for a new one (or one with no room near), cheapest first, out to the wide reach: one list trying its right before its left, one its left first. Made once, as they're the same every time.</summary>
    private static readonly Point[] RightFirst = Moves(true);
    private static readonly Point[] LeftFirst = Moves(false);

    /// <summary>How long since they were last drawn, in seconds, for how far one's nudged.</summary>
    private float _sinceDrawn;

    /// <summary>How many have come up, for telling apart two that came up on the same frame.</summary>
    private int _serial;

    private SpriteFont _numbers;
    private SpriteFont _words;

    /// <summary>The two fonts' glyphs, for how far down the line each letter's ink goes; kept, as asking the font makes a copy.</summary>
    private Dictionary<char, SpriteFont.Glyph> _numberGlyphs;
    private Dictionary<char, SpriteFont.Glyph> _wordGlyphs;

    /// <summary>Whether numbers come up (the options' damage numbers); words always do.</summary>
    public bool Shown { get; set; } = true;

    /// <summary>Whether callouts wait: set while a banner's across the middle of the screen (a fight's start, a boss's phase), so a word doesn't come up behind it.</summary>
    public bool Holding { get; set; }

    /// <summary>One popup: whose it is (for the most at once) and whose nameplate it goes over, when it came up, where it's pinned, what it says, its kind and colour, whether it's in the number font, its size, how long it's been up and since it last popped, how long it lasts and how long it rises, how far across it's fanned, the amount a number carries, whether it's a callout (held while Holding) and how long it's waited to come up, whether it's up, how far it's been moved to make room and the box it held last frame, whether it's been let near someone else's nameplate (there was nowhere else), how long it's been fading out if it's had to give way, how far over its point it started (kept, so a nameplate growing a chevron or a telegraph nudges it rather than lifting it at once), and how far its point is from where its owner stands, to follow them.</summary>
    private sealed class Popup
    {
        public string Owner;
        public string Over;
        public int Serial;
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
        public float Across;
        public int Amount;
        public string Sign;
        public bool Callout;
        public float Waited;
        public bool Placed;
        public Point Moved;
        public Rectangle Box;
        public bool Astray;
        public float Yielding = -1f;
        public float? Lift;
        public Vector2? Follow;
    }

    /// <summary>Loads the two fonts.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _numbers = content.Load<SpriteFont>("spectral-number");
        _words = content.Load<SpriteFont>("spectral-ui");
        _numberGlyphs = _numbers.GetGlyphs();
        _wordGlyphs = _words.GetGlyphs();
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
        Add(null, null, anchor, heal ? "+" : "-", amount, PopupKind.Other, colour, true, critical ? CriticalScale : NumberScale, NumberSeconds, false);
    }

    /// <summary>Pops up a callout, a word or two: a skill's name, INTERRUPTED, TAUNTED, DOWN!. It waits while Holding.</summary>
    /// <param name="anchor">The point it's over, in world pixels.</param>
    /// <param name="text">What it says, in capitals.</param>
    /// <param name="colour">Its colour.</param>
    /// <param name="over">Whose nameplate it goes over (their id in the fight), or null to go over the point alone.</param>
    public void Word(Vector2 anchor, string text, Color colour, string over = null) =>
        Add(null, over, anchor, text, 0, PopupKind.Other, colour, false, WordScale, WordSeconds, true);

    /// <summary>How wide and tall a word popped up with Word is when it starts, in screen pixels, for keeping it on the screen and off the HUD.</summary>
    /// <param name="text">What it says.</param>
    public Vector2 WordSize(string text) => _words is null ? Vector2.Zero : _words.MeasureString(text) * WordScale;

    /// <summary>Pops up damage over someone: white for damage your side deals, red for damage it takes, a critical gold (or a deeper red), bigger and bouncing.</summary>
    /// <param name="owner">Who it's over (their id in the fight), for the most at once and their nameplate; null for no limit.</param>
    /// <param name="anchor">The point it's over, in world pixels: the top of their head.</param>
    /// <param name="amount">How much.</param>
    /// <param name="critical">Whether it was a critical.</param>
    /// <param name="taken">Whether it's damage to your side.</param>
    public void Damage(string owner, Vector2 anchor, int amount, bool critical, bool taken)
    {
        if (!Shown) return;
        PopupKind kind = critical ? PopupKind.Critical : taken ? PopupKind.Taken : PopupKind.Dealt;
        Color colour = taken ? critical ? new Color(255, 96, 60) : TakenColour : critical ? CriticalColour : DealtColour;
        Add(owner, owner, anchor, "", amount, kind, colour, true, critical ? CriticalScale : NumberScale, NumberSeconds, false);
    }

    /// <summary>Pops up healing over someone: green with a "+", rising slower.</summary>
    /// <param name="owner">Who it's over.</param>
    /// <param name="anchor">The point it's over, in world pixels.</param>
    /// <param name="amount">How much.</param>
    public void Heal(string owner, Vector2 anchor, int amount)
    {
        if (!Shown) return;
        Add(owner, owner, anchor, "+", amount, PopupKind.Heal, HealColour, true, NumberScale, NumberSeconds, false);
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
        Add(owner, owner, anchor, heal ? "+" : "", amount, PopupKind.Tick, heal ? HealColour : tint, true, TickScale, NumberSeconds * 0.9f, false);
    }

    /// <summary>Pops up MISS, DODGE or BLOCK over someone, in grey, small, without a pop.</summary>
    /// <param name="owner">Who it's over.</param>
    /// <param name="anchor">The point it's over, in world pixels.</param>
    /// <param name="word">The word, in capitals.</param>
    public void Avoided(string owner, Vector2 anchor, string word) =>
        Add(owner, owner, anchor, word, 0, PopupKind.Avoided, AvoidedColour, false, AvoidedScale, WordSeconds, false);

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

    /// <summary>Runs every popup's clock and lets the finished ones go; one that isn't up yet (a callout held while Holding, or one with no room) keeps waiting, its clock still, and goes if it's waited too long.</summary>
    /// <param name="seconds">Seconds since the last update.</param>
    public void Update(float seconds)
    {
        _sinceDrawn += seconds;
        for (int i = _popups.Count - 1; i >= 0; i--)
        {
            Popup popup = _popups[i];
            if (!popup.Placed)
            {
                popup.Waited += seconds;
                if (popup.Waited >= MostWait) _popups.RemoveAt(i);
                continue;
            }
            popup.Clock += seconds;
            popup.PopClock += seconds;
            if (popup.Yielding >= 0f) popup.Yielding += seconds;
            if (popup.Clock >= popup.Life || popup.Yielding >= YieldSeconds) _popups.RemoveAt(i);
        }
    }

    /// <summary>Takes every popup down, when the fight's over.</summary>
    public void Clear()
    {
        _popups.Clear();
        _fans.Clear();
    }

    /// <summary>Draws every popup over its point, rising and fading, each in a place of its own.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="toScreen">Where a point on the map is on the screen: the camera's ToScreen.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    /// <param name="keepOff">What none may be drawn on, in screen pixels (the HUD's frames, the nameplates, a banner, the barks), or null.</param>
    /// <param name="plates">Everyone's nameplate on the screen, by their id, or null: one's own start over theirs, and none settles nearer someone else's than its own.</param>
    /// <param name="whereIs">Where someone stands on the map now, by their id, or null if they're gone; or null for every one to stay where it was pinned.</param>
    public void Draw(SpriteBatch spriteBatch, Func<Vector2, Vector2> toScreen, float alpha, IReadOnlyList<Rectangle> keepOff = null, IReadOnlyList<(string Id, Rectangle Box)> plates = null,
        Func<string, Vector2?> whereIs = null)
    {
        int nudge = Math.Clamp((int)MathF.Ceiling(NudgeSpeed * _sinceDrawn / NudgeStep) * NudgeStep, LeastNudge, MostNudge);
        _sinceDrawn = 0f;

        // The ones already up are seen to first, oldest first, so each keeps its place and a new one
        // never pushes one that's been read; then the new ones, in the order they came.
        _order.Clear();
        foreach (Popup popup in _popups)
            if (popup.Placed || !popup.Callout || !Holding) _order.Add(popup);
        _order.Sort((a, b) => a.Placed != b.Placed ? b.Placed.CompareTo(a.Placed) : a.Clock != b.Clock ? b.Clock.CompareTo(a.Clock) : a.Serial.CompareTo(b.Serial));
        _placed.Clear();
        _taken.Clear();
        var screen = new Rectangle(Edge, Edge, LegendQuestGame.ScreenWidth - Edge * 2, LegendQuestGame.ScreenHeight - Edge * 2);

        for (int i = 0; i < _order.Count; i++)
        {
            Popup popup = _order[i];
            if (popup.Over is not null && whereIs?.Invoke(popup.Over) is Vector2 stands)
            {
                popup.Follow ??= popup.Anchor - stands;
                popup.Anchor = stands + popup.Follow.Value;
            }
            SpriteFont font = popup.Number ? _numbers : _words;
            (float inkTop, float inkFoot) = Ink(popup.Number ? _numberGlyphs : _wordGlyphs, font, popup.Text);
            var size = new Vector2(font.MeasureString(popup.Text).X, inkFoot - inkTop) * popup.Scale;
            Vector2 anchor = toScreen(popup.Anchor);
            float start = anchor.Y;
            if (popup.Lift is float lift) start -= lift;
            else if (PlateOf(plates, popup.Over) is Rectangle own) start = MathF.Min(start, own.Top - OverPlate);

            // It rises over the life it came up with, so a total that's added to stays where it is.
            float risen = MathF.Min(1f, popup.Clock / popup.RiseLife);
            float eased = 1f - (1f - risen) * (1f - risen);
            float climbed = Rise * eased * (popup.Kind == PopupKind.Heal ? HealRise : 1f);
            float foot = start - climbed; // where the foot of its letters is
            float centre = anchor.X + popup.Across;

            // Its box is as big as it'll be drawn from now on: the pop that's left and the bounce to come.
            Rectangle full = BoxAt(centre, foot, size * MathF.Max(1f, Pop(popup)), HopToCome(popup));
            Rectangle settled = BoxAt(centre, foot, size, 0f);
            var place = new Placing
            {
                Screen = screen, KeepOff = keepOff, Plates = plates, Over = popup.Over, Home = new Vector2(anchor.X, start),
                Drop = (int)climbed, After = i + 1, Astray = popup.Astray,
            };

            Point? move = null;
            bool popped = true, searched = false;
            if (popup.Placed && popup.Yielding < 0f)
            {
                // It stays put if it can, else it's nudged; its pop grows only into room nobody holds.
                // Someone walking up doesn't move it, nor does being out past the usual reach: it's been read where it is.
                move = Nudge(full, popup.Moved, nudge, place with { Astray = true, Wide = true });
                if (move is null && full != settled)
                {
                    popped = false;
                    move = Nudge(settled, popup.Moved, nudge, place with { Astray = true, Wide = true });
                }
                searched = move is null;
                bool read = popup.Clock >= ReadSeconds;
                if (!read)
                {
                    move ??= Search(settled, popup.Across >= 0f, place) ?? Search(settled, popup.Across >= 0f, place with { Astray = true })
                        ?? Search(settled, popup.Across >= 0f, place with { Astray = true, Wide = true });
                    if (move is null && MergeInto(popup)) continue;
                }
                if (move is null) popup.Yielding = 0f;
            }
            else if (!popup.Placed)
            {
                searched = true;
                move = Search(full, popup.Across >= 0f, place);
                if (move is null && MergeInto(popup)) continue;
                move ??= Search(full, popup.Across >= 0f, place with { Astray = true }) ?? Search(full, popup.Across >= 0f, place with { Astray = true, Wide = true });
                if (move is null) continue;
            }

            // One giving way stays still in the box it held last frame, which the others have kept
            // off, neither rising nor following, and fades out there.
            Rectangle box = move is Point to ? Offset(popped ? full : settled, to) : popup.Box;
            if (move is Point moved)
            {
                popup.Moved = moved;
                if (searched) popup.Astray = Near(box, place);
            }
            popup.Placed = true;
            popup.Lift ??= anchor.Y - start;
            popup.Box = box;
            _taken.Add(box);
            _placed.Add(popup);

            bool giving = popup.Yielding >= 0f;
            float t = popup.Clock / popup.Life;
            float shown = (t < FadeFrom ? 1f : 1f - (t - FadeFrom) / (1f - FadeFrom)) * alpha;
            if (giving) shown *= MathF.Max(0f, 1f - popup.Yielding / YieldSeconds);
            float scale = popup.Scale * (popped && !giving ? Pop(popup) : 1f);
            float hop = popped && !giving && popup.Kind == PopupKind.Critical ? Bounce(popup.PopClock - CriticalPopSeconds) : 0f;
            float wide = font.MeasureString(popup.Text).X * scale;
            var at = giving
                ? new Vector2(MathF.Round(box.Center.X - wide / 2f), box.Bottom - RoomUp - MathF.Round(inkFoot * scale))
                : new Vector2(MathF.Round(centre + popup.Moved.X - wide / 2f), MathF.Round(foot + popup.Moved.Y - hop - inkFoot * scale));

            spriteBatch.DrawString(font, popup.Text, at + ShadowOffset * popup.Scale, ShadowColour * shown, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, popup.Text, at, popup.Colour * shown, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }

    /// <summary>What a popup's place has to keep to: the screen, what's kept off, everyone's nameplates, whose its own is and where it starts, how far down it may go (as far as it's risen), where in the order the ones still to be placed begin, whether it may go near someone else's nameplate, and whether it may go out to the wide reach.</summary>
    private readonly record struct Placing
    {
        public Rectangle Screen { get; init; }
        public IReadOnlyList<Rectangle> KeepOff { get; init; }
        public IReadOnlyList<(string Id, Rectangle Box)> Plates { get; init; }
        public string Over { get; init; }
        public Vector2 Home { get; init; }
        public int Drop { get; init; }
        public int After { get; init; }
        public bool Astray { get; init; }
        public bool Wide { get; init; }
    }

    /// <summary>The box one holds with the foot of its letters at a point: as wide as it's set and as tall as its ink, the bounce to come over it, and the room kept round it (enough for the shadow).</summary>
    private static Rectangle BoxAt(float centre, float foot, Vector2 size, float hop) =>
        new((int)MathF.Floor(centre - size.X / 2f) - RoomAcross, (int)MathF.Floor(foot - size.Y - hop) - RoomUp,
            (int)MathF.Ceiling(size.X) + RoomAcross * 2, (int)MathF.Ceiling(size.Y + hop) + RoomUp * 2);

    /// <summary>How far down a line of a font the ink of some text starts and ends, at the font's own size: the box is held round the ink rather than the whole line, which has room over and under the letters.</summary>
    private static (float Top, float Foot) Ink(Dictionary<char, SpriteFont.Glyph> glyphs, SpriteFont font, string text)
    {
        float top = float.MaxValue, foot = float.MinValue;
        if (glyphs is not null)
            foreach (char letter in text)
                if (glyphs.TryGetValue(letter, out SpriteFont.Glyph glyph) && glyph.BoundsInTexture.Height > 0)
                {
                    top = MathF.Min(top, glyph.Cropping.Y);
                    foot = MathF.Max(foot, glyph.Cropping.Y + glyph.BoundsInTexture.Height);
                }
        return top <= foot ? (top, foot) : (0f, font.LineSpacing);
    }

    /// <summary>A box moved.</summary>
    private static Rectangle Offset(Rectangle box, Point move) => new(box.X + move.X, box.Y + move.Y, box.Width, box.Height);

    /// <summary>
    /// Where one that's up goes this frame: where it was if that's still free, or else the cheapest
    /// free place within a nudge of it, counting what it costs to move from where it was; null if
    /// there's none that near.
    /// </summary>
    private Point? Nudge(Rectangle own, Point last, int reach, in Placing place)
    {
        if (Fits(own, last, place)) return last;
        Point? best = null;
        float least = float.MaxValue;
        for (int dy = -reach; dy <= reach; dy += NudgeStep)
            for (int dx = -reach; dx <= reach; dx += NudgeStep)
            {
                var move = new Point(last.X + dx, last.Y + dy);
                float cost = Cost(move.X, move.Y) + StayCost * (Math.Abs(dx) + Math.Abs(dy));
                if (cost >= least || !Fits(own, move, place)) continue;
                least = cost;
                best = move;
            }
        return best;
    }

    /// <summary>The cheapest free place for one from where it would be: up first, then aside (its fan's side first), and down only as far as it's risen; null if there's none within reach (the usual or the wide).</summary>
    private Point? Search(Rectangle own, bool rightFirst, in Placing place)
    {
        foreach (Point move in rightFirst ? RightFirst : LeftFirst)
            if (Fits(own, move, place)) return move;
        return null;
    }

    /// <summary>Whether one's box moved so is free: in reach (the usual or the wide), on the screen, off what's kept off, off what's been placed this frame and what the ones still to be placed held last frame, and (unless it may) no nearer anyone else's nameplate than its own.</summary>
    private bool Fits(Rectangle own, Point move, in Placing place)
    {
        if (move.Y > place.Drop || move.Y < -(place.Wide ? WideClimb : MostClimb) || Math.Abs(move.X) > (place.Wide ? WideAside : MostAside)) return false;
        Rectangle box = Offset(own, move);
        if (!place.Screen.Contains(box) || Hits(box, _taken) || place.KeepOff is not null && Hits(box, place.KeepOff)) return false;
        for (int j = place.After; j < _order.Count; j++)
            if (_order[j].Placed && _order[j].Box.Intersects(box)) return false;
        return place.Astray || !Near(box, place);
    }

    /// <summary>Whether a box sits nearer someone else's nameplate than where its own one starts, so it would be read as theirs.</summary>
    private static bool Near(Rectangle box, in Placing place)
    {
        if (place.Plates is null) return false;
        var foot = new Vector2(box.Center.X, box.Bottom);
        float mine = Vector2.DistanceSquared(foot, place.Home);
        foreach ((string id, Rectangle plate) in place.Plates)
            if (id != place.Over && Vector2.DistanceSquared(foot, new Vector2(plate.Center.X, plate.Top)) < mine) return true;
        return false;
    }

    /// <summary>Someone's nameplate on the screen, or null if they've none.</summary>
    private static Rectangle? PlateOf(IReadOnlyList<(string Id, Rectangle Box)> plates, string id)
    {
        if (plates is null || id is null) return null;
        foreach ((string each, Rectangle box) in plates)
            if (each == id) return box;
        return null;
    }

    /// <summary>
    /// Adds a number with no room into a running total placed already this frame over the same owner,
    /// of the same kind, colour and sign (the newest such), which pops again with the sum, and takes it
    /// away. False if it isn't such a number or there's no such total.
    /// </summary>
    private bool MergeInto(Popup popup)
    {
        if (!popup.Number || popup.Kind == PopupKind.Critical || popup.Owner is null) return false;
        Popup into = null;
        foreach (Popup other in _placed)
            if (other.Owner == popup.Owner && other.Number && other.Kind == popup.Kind && other.Colour == popup.Colour && other.Sign == popup.Sign
                && other.Yielding < 0f && (into is null || other.Clock < into.Clock)) into = other;
        if (into is null) return false;
        into.Amount += popup.Amount;
        into.Text = into.Sign + into.Amount;
        into.PopClock = 0f;
        into.Life = MathF.Max(into.Life, into.Clock + MergedSeconds);
        _popups.Remove(popup);
        return true;
    }

    /// <summary>What a move costs against where one would be: a pixel up 1, aside AsideCost, down DownCost.</summary>
    private static float Cost(int dx, int dy) => Math.Abs(dx) * AsideCost + (dy < 0 ? -dy : dy * DownCost);

    /// <summary>Every move a new one may make out to the wide reach, cheapest first, one side tried before the other where two cost the same.</summary>
    private static Point[] Moves(bool rightFirst)
    {
        var moves = new List<(float Cost, int Index, Point Move)>();
        for (int across = 0; across <= WideAside; across += AsideStep)
            for (int side = 0; side < (across == 0 ? 1 : 2); side++)
            {
                int dx = side == 0 == rightFirst ? across : -across;
                for (int dy = -WideClimb; dy <= (int)Rise; dy += Step) moves.Add((Cost(dx, dy), moves.Count, new Point(dx, dy)));
            }
        moves.Sort((a, b) => a.Cost != b.Cost ? a.Cost.CompareTo(b.Cost) : a.Index.CompareTo(b.Index));
        var sorted = new Point[moves.Count];
        for (int i = 0; i < sorted.Length; i++) sorted[i] = moves[i].Move;
        return sorted;
    }

    /// <summary>Whether a box overlaps any of some others.</summary>
    private static bool Hits(Rectangle box, IReadOnlyList<Rectangle> others)
    {
        for (int i = 0; i < others.Count; i++)
            if (others[i].Intersects(box)) return true;
        return false;
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

    /// <summary>The highest a critical's bounce has still to lift it, in screen pixels: all of it until the top, then where it is on the way down.</summary>
    private static float HopToCome(Popup popup)
    {
        if (popup.Kind != PopupKind.Critical) return 0f;
        float clock = popup.PopClock - CriticalPopSeconds;
        return clock < BounceSeconds / 2f ? BounceHeight : Bounce(clock);
    }

    /// <summary>
    /// Adds one, fanned left or right by turns over its owner (Draw finds it a place of its own); or, if
    /// its owner has the most up already, adds a number into the newest of its kind over them (or the
    /// newest number at all), which pops again with the sum where it is and stays up a little longer.
    /// A critical is never added into another nor anything into it: it keeps its own gold, and the
    /// oldest over them that isn't one makes room for it, as for a word.
    /// </summary>
    private void Add(string owner, string over, Vector2 anchor, string sign, int amount, PopupKind kind, Color colour, bool number, float scale, float life, bool callout)
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
            Owner = owner, Over = over, Serial = _serial++, Anchor = anchor, Text = text, Kind = kind, Colour = colour, Number = number, Scale = scale,
            Life = life, RiseLife = life, Across = across, Amount = amount, Sign = number ? sign : "", Callout = callout,
        });
    }
}
