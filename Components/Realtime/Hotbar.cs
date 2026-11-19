using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The real-time hotbar, bottom centre (11-combat.md §1, §2.4): six slots on keys 1 to 6 (the auto
/// attack, three skills, the ultimate and the potion) and the Soul-Link sigil on 7, with your cast
/// bar and the auto attack's swing over it.
/// </summary>
/// <remarks>
/// <para>
/// Each slot is hud.png's slot frame twice its size with the skill's icon in its well, its key in
/// the top corner and its Focus cost (or what's left in the bag) in the bottom one. While its own
/// cooldown runs, a dark veil sweeps round clockwise from twelve with a pale hand on its edge and
/// the seconds left in the middle; while the global cooldown runs, a lighter veil sweeps over every
/// slot on it, with no number. A slot whose target is out of reach is tinted red; one there isn't
/// Focus enough for is greyed with its cost in red; one that can't be used for another reason is
/// dimmed. Pressing one flashes it; one coming off its cooldown catches a glint across it; an armed
/// reaction glows gold, its seconds left in its corner, until it fires.
/// </para>
/// <para>
/// The sigil on 7 is the Soul-Link's: grey and dead while the bond's broken, violet while one of
/// you is down, a gold pulse while it charges, and lit gold with a glow when its two charges are in,
/// with a pip for each charge in its corner.
/// </para>
/// <para>
/// The slots are clickable: At says which the mouse is on (0 to 5, or SigilIndex for the sigil),
/// and Describe puts its card together for CombatCard.DrawAbove.
/// </para>
/// </remarks>
public class Hotbar
{
    /// <summary>How many skill slots there are, and the index At gives the sigil.</summary>
    public const int SlotCount = 6;
    public const int SigilIndex = SlotCount;

    /// <summary>How much the hud art is blown up, a slot's size on screen, and the room between two slots and before the sigil.</summary>
    private const float Scale = 2f;
    public const int SlotSize = 60;
    private const int Gap = 6;
    private const int SigilGap = 22;

    /// <summary>How far up from the bottom of the screen the slots sit, and how far the panel reaches round them.</summary>
    private const int BottomMargin = 14;
    private const int PanelReach = 10;

    /// <summary>The swing bar and the cast bar over the panel: their widths, heights and the room between.</summary>
    private const int SwingWidth = 300;
    private const int SwingHeight = 7;
    private const int CastWidth = 380;
    private const int CastHeight = 26;
    private const int OverGap = 6;

    /// <summary>How far in from a slot's corner its key and its numbers go, in screen pixels.</summary>
    private const int CornerInset = 5;

    /// <summary>The veils: the cooldown's, the global cooldown's, and their hands' brightness.</summary>
    private const float CooldownVeil = 0.64f;
    private const float CooldownHand = 0.85f;
    private const float GlobalVeil = 0.4f;
    private const float GlobalHand = 0.55f;

    /// <summary>How long a press flash and a ready glint last, in seconds, and how bright the flash is.</summary>
    private const float PressSeconds = 0.22f;
    private const float PressStrength = 0.6f;
    private const float GlintSeconds = 0.45f;

    /// <summary>How fast an armed slot's gold edge breathes, and how thick it is.</summary>
    private const float ArmedRate = 2.4f;
    private const int ArmedEdge = 3;

    /// <summary>The ready sigil's glow: how far it reaches past the slot, and how bright.</summary>
    private const float GlowReach = 0.7f;
    private const float GlowStrength = 0.8f;

    /// <summary>What an icon's multiplied by: greyed when there isn't Focus enough, dimmed when it can't be used, red when out of reach.</summary>
    private static readonly Color Greyed = new(92, 92, 104);
    private static readonly Color Dimmed = new(150, 150, 162);
    private static readonly Color OutOfReach = new(255, 120, 108);

    /// <summary>The red laid over a slot out of reach.</summary>
    private static readonly Color ReachVeil = new Color(200, 30, 24) * 0.3f;

    private readonly float[] _press = new float[SlotCount + 1];
    private readonly float[] _glint = new float[SlotCount];
    private readonly float[] _lastCooldown = new float[SlotCount];

    /// <summary>The auto attack's swing bar over the panel.</summary>
    public SwingTimer Swing { get; } = new();

    /// <summary>Where each slot is on screen: the six, then the sigil.</summary>
    public static Rectangle[] Slots()
    {
        var slots = new Rectangle[SlotCount + 1];
        int width = SlotCount * SlotSize + (SlotCount - 1) * Gap + SigilGap + SlotSize;
        int x = (LegendQuestGame.ScreenWidth - width) / 2;
        int y = LegendQuestGame.ScreenHeight - BottomMargin - SlotSize;
        for (int i = 0; i < SlotCount; i++, x += SlotSize + Gap) slots[i] = new Rectangle(x, y, SlotSize, SlotSize);
        slots[SigilIndex] = new Rectangle(x - Gap + SigilGap, y, SlotSize, SlotSize);
        return slots;
    }

    /// <summary>The panel behind the slots.</summary>
    public static Rectangle Panel
    {
        get
        {
            Rectangle[] slots = Slots();
            int left = slots[0].X - PanelReach, right = slots[SigilIndex].Right + PanelReach;
            int top = slots[0].Y - PanelReach;
            return new Rectangle(left, top, right - left, LegendQuestGame.ScreenHeight - top + 2);
        }
    }

    /// <summary>The swing bar: just over the panel, centred.</summary>
    public static Rectangle SwingArea => new((LegendQuestGame.ScreenWidth - SwingWidth) / 2, Panel.Y - OverGap - SwingHeight, SwingWidth, SwingHeight);

    /// <summary>Your cast bar: over the swing bar, centred.</summary>
    public static Rectangle CastArea => new((LegendQuestGame.ScreenWidth - CastWidth) / 2, SwingArea.Y - OverGap - CastHeight, CastWidth, CastHeight);

    /// <summary>Which slot the mouse is on: 0 to 5, SigilIndex for the sigil, or -1.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    public static int At(Point mouse)
    {
        Rectangle[] slots = Slots();
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].Contains(mouse)) return i;
        return -1;
    }

    /// <summary>Flashes a slot, as when its key's pressed or it's clicked.</summary>
    /// <param name="index">0 to 5, or SigilIndex.</param>
    public void Press(int index)
    {
        if (index >= 0 && index < _press.Length) _press[index] = PressSeconds;
    }

    /// <summary>Starts a slot's ready glint, as when its cooldown's just run out. Update does it on its own when it sees one run out.</summary>
    /// <param name="index">0 to 5.</param>
    /// <param name="through">How far through the glint to start, 0 to 1: 0 is its start; a picture of it half across wants 0.5.</param>
    public void Ready(int index, float through = 0f)
    {
        if (index >= 0 && index < _glint.Length) _glint[index] = GlintSeconds * (1f - MathHelper.Clamp(through, 0f, 1f));
    }

    /// <summary>Forgets the flashes, the glints, the cooldowns it last saw and the swing, for a new fight.</summary>
    public void Reset()
    {
        Array.Clear(_press);
        Array.Clear(_glint);
        Array.Clear(_lastCooldown);
        Swing.Reset();
    }

    /// <summary>Runs the flashes and glints, starts a glint on any slot whose cooldown ran out since the last frame, and moves the swing on.</summary>
    /// <param name="seconds">How long the frame was.</param>
    /// <param name="slots">The six slots as they are this frame.</param>
    /// <param name="swing">How far round your auto attack's swing is, 0 to 1, or less than 0.</param>
    public void Update(float seconds, IReadOnlyList<SlotView> slots, float swing)
    {
        for (int i = 0; i < _press.Length; i++) _press[i] = MathF.Max(0f, _press[i] - seconds);
        for (int i = 0; i < _glint.Length; i++) _glint[i] = MathF.Max(0f, _glint[i] - seconds);
        for (int i = 0; i < SlotCount && i < slots.Count; i++)
        {
            float left = slots[i]?.CooldownLeft ?? 0f;
            if (_lastCooldown[i] > 0f && left <= 0f) _glint[i] = GlintSeconds;
            _lastCooldown[i] = left;
        }
        Swing.Update(swing, seconds);
    }

    /// <summary>Draws the panel, the slots and their icons, sweeps, tints, flashes and glints, the sigil, your cast bar and the swing. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="slots">The six slots.</param>
    /// <param name="sigil">The Soul-Link sigil, or null to leave slot 7 empty.</param>
    /// <param name="cast">Your cast, or null.</param>
    /// <param name="swing">How far round your swing is, 0 to 1, or less than 0.</param>
    /// <param name="hovered">Which slot the mouse is on, or -1.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawArt(SpriteBatch spriteBatch, HudKit kit, IReadOnlyList<SlotView> slots, SoulLinkView sigil, CastView cast, float swing, int hovered, float alpha)
    {
        CombatStyle.DrawPanel(spriteBatch, kit.Pixel, Panel, alpha);
        Swing.DrawArt(spriteBatch, kit, SwingArea, swing, alpha);
        if (cast is not null) CastBar.DrawArt(spriteBatch, kit, CastArea, cast, false, alpha);

        Rectangle[] rects = Slots();
        for (int i = 0; i < SlotCount; i++)
            DrawSlot(spriteBatch, kit, i < slots.Count ? slots[i] : null, i, rects[i], i == hovered, alpha);
        DrawSigil(spriteBatch, kit, sigil, rects[SigilIndex], hovered == SigilIndex, alpha);
    }

    /// <summary>Writes the keys, the costs and counts, the seconds left on cooldowns and armed reactions, what unlocks an empty slot, and your cast's words. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="slots">The six slots.</param>
    /// <param name="sigil">The Soul-Link sigil, or null.</param>
    /// <param name="cast">Your cast, or null.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawWords(SpriteBatch spriteBatch, HudKit kit, IReadOnlyList<SlotView> slots, SoulLinkView sigil, CastView cast, float alpha)
    {
        if (cast is not null) CastBar.DrawWords(spriteBatch, kit, CastArea, cast, alpha);

        Rectangle[] rects = Slots();
        for (int i = 0; i < SlotCount && i < slots.Count; i++)
        {
            SlotView slot = slots[i];
            Rectangle r = rects[i];
            if (slot is null) continue;
            bool live = slot.IconId is not null && slot.Usable;
            HudKit.Outlined(spriteBatch, kit.Small, slot.Key, new Vector2(r.X + CornerInset + 1, r.Y + 2), (live ? Palette.Bone : Palette.Note) * alpha, alpha);

            if (slot.IconId is null)
            {
                // "RANK 5" as two lines, the word over the number, to fit the slot.
                if (slot.Locked is not null)
                {
                    string[] words = slot.Locked.Split(' ', 2);
                    float y = r.Center.Y - (words.Length - 1) * kit.Small.LineSpacing / 2f + 7f;
                    foreach (string word in words)
                    {
                        HudKit.Outlined(spriteBatch, kit.Small, word, new Vector2(r.Center.X, y), Palette.Note * alpha, alpha, align: 0.5f, middle: true);
                        y += kit.Small.LineSpacing - 4;
                    }
                }
                continue;
            }

            var corner = new Vector2(r.Right - CornerInset, r.Bottom - kit.Small.LineSpacing - 1);
            if (slot.FocusCost > 0)
                HudKit.Outlined(spriteBatch, kit.Small, slot.FocusCost.ToString(CultureInfo.InvariantCulture), corner,
                    (slot.Affordable ? CombatStyle.FocusText : Palette.Ember) * alpha, alpha, align: 1f);
            else if (slot.Count is not null)
                HudKit.Outlined(spriteBatch, kit.Small, slot.Count, corner, Palette.Gold * alpha, alpha, align: 1f);

            if (slot.Cooling)
                HudKit.Outlined(spriteBatch, kit.Timer, HudKit.Seconds(slot.CooldownLeft), r.Center.ToVector2() + new Vector2(0f, 1f), Color.White * alpha, alpha,
                    align: 0.5f, middle: true);
            if (slot.Armed > 0f)
                HudKit.Outlined(spriteBatch, kit.Small, HudKit.Seconds(slot.Armed), new Vector2(r.Right - CornerInset, r.Y + 2), Palette.Gold * alpha, alpha, align: 1f);
        }

        if (sigil is not null)
        {
            Rectangle r = rects[SigilIndex];
            HudKit.Outlined(spriteBatch, kit.Small, sigil.Key, new Vector2(r.X + CornerInset + 1, r.Y + 2), (sigil.Ready ? Palette.Gold : Palette.Note) * alpha, alpha);
        }
    }

    /// <summary>Puts a slot's card together: its name, its numbers, what it does, its cooldown and why it can't be used.</summary>
    /// <param name="card">The card, cleared first.</param>
    /// <param name="slot">The slot.</param>
    public static void Describe(CombatCard card, SlotView slot)
    {
        card.Clear();
        if (slot.IconId is null)
        {
            card.Line("EMPTY", Palette.Note);
            if (slot.Locked is not null) card.Line("Unlocks at " + slot.Locked.ToLowerInvariant() + ".", Palette.Bone);
            return;
        }
        card.Line(slot.Name.ToUpperInvariant(), Palette.Gold);
        card.Line(slot.Detail, CombatStyle.FocusText);
        card.Gap();
        card.Line(slot.Text, Palette.Bone);
        if (slot.Cooling || slot.Armed > 0f) card.Gap();
        if (slot.Cooling) card.Line($"Ready in {HudKit.Seconds(slot.CooldownLeft, unit: true)}", Palette.Note);
        if (slot.Armed > 0f) card.Line($"Armed: {HudKit.Seconds(slot.Armed, unit: true)} left to fire", Palette.Gold);
        if (!slot.Usable && !slot.Cooling && slot.WhyNot is not null)
        {
            card.Gap();
            card.Line(slot.WhyNot, Palette.Ember);
        }
    }

    /// <summary>Puts the sigil's card together.</summary>
    /// <param name="card">The card, cleared first.</param>
    /// <param name="sigil">The sigil.</param>
    public static void Describe(CombatCard card, SoulLinkView sigil)
    {
        card.Clear();
        card.Line("SOUL-LINK STRIKE", Palette.Gold);
        card.Line(sigil.Broken ? "The bond is broken." : $"{sigil.Charge} of {sigil.Full} charges", CombatStyle.FocusText);
        card.Gap();
        card.Line(sigil.Text, Palette.Bone);
        if (sigil.Strained) card.Line("One of you is down.", Palette.Ember);
    }

    /// <summary>One slot: its frame, its icon tinted for what's wrong, its sweeps, the red of out of reach, the press, the glint and the armed glow.</summary>
    private void DrawSlot(SpriteBatch spriteBatch, HudKit kit, SlotView slot, int index, Rectangle r, bool hovered, float alpha)
    {
        bool empty = slot?.IconId is null;
        bool armed = slot is { Armed: > 0f };
        string frame = empty ? "hotbar-disabled"
            : armed ? "hotbar-selected"
            : !slot.Usable && !slot.Cooling && !slot.Global ? "hotbar-disabled"
            : hovered || _glint[index] > 0f ? "hotbar-hover"
            : "hotbar-normal";
        float push = _press[index] > 0f || slot is { Pressed: > 0f } ? 1f : 0f;
        kit.Art.Piece(spriteBatch, frame, new Vector2(r.X, r.Y + push), Scale, Color.White * alpha);
        if (empty) return;

        int inset = (int)(Combat.CombatIcons.FrameInset * Scale);
        var well = new Rectangle(r.X + inset, r.Y + inset + (int)push, r.Width - inset * 2, r.Height - inset * 2);
        Color tint = !slot.Affordable ? Greyed : !slot.InRange ? OutOfReach : !slot.Usable && !slot.Cooling && !slot.Global ? Dimmed : Color.White;
        kit.Icons.Draw(spriteBatch, slot.IconId, well.Location.ToVector2(), Scale, tint * alpha);
        if (slot.Affordable && !slot.InRange) spriteBatch.Draw(kit.Pixel, well, ReachVeil * alpha);

        if (slot.Global) kit.Icons.DrawSweep(spriteBatch, kit.Pixel, well, slot.GcdLeft / slot.GcdTotal, GlobalVeil * alpha, GlobalHand * alpha);
        if (slot.Cooling) kit.Icons.DrawSweep(spriteBatch, kit.Pixel, well, slot.CooldownLeft / slot.CooldownTotal, CooldownVeil * alpha, CooldownHand * alpha);

        float flash = MathF.Max(_press[index] / PressSeconds, slot.Pressed);
        if (flash > 0f) spriteBatch.Draw(kit.Pixel, well, Color.White * (PressStrength * flash * alpha));

        if (_glint[index] > 0f)
        {
            float u = 1f - _glint[index] / GlintSeconds;
            int window = HudIcons.SweepSize;
            int offset = (int)MathHelper.Lerp(window * 2 - 4, 4, u);
            spriteBatch.Draw(kit.Icons.Glint, well, new Rectangle(offset, 0, window, window), Color.White * (0.9f * alpha));
        }

        if (armed) kit.Edge(spriteBatch, InflateBy(r, ArmedEdge), ArmedEdge, Palette.Gold * (kit.Pulse(ArmedRate, 0.35f) * alpha));
    }

    /// <summary>The sigil's slot: its frame, its glow when ready, the sigil in its state and a pip for each charge.</summary>
    private void DrawSigil(SpriteBatch spriteBatch, HudKit kit, SoulLinkView sigil, Rectangle r, bool hovered, float alpha)
    {
        if (sigil is null)
        {
            kit.Art.Piece(spriteBatch, "hotbar-disabled", r.Location.ToVector2(), Scale, Color.White * alpha);
            return;
        }

        if (sigil.Ready)
        {
            Rectangle glow = r;
            glow.Inflate((int)(r.Width * GlowReach), (int)(r.Height * GlowReach));
            spriteBatch.Draw(kit.Glow, glow, Palette.Gold * (GlowStrength * kit.Pulse(0.8f, 0.55f) * alpha));
        }
        float push = _press[SigilIndex] > 0f || sigil.Pressed > 0f ? 1f : 0f;
        string frame = sigil.Broken ? "hotbar-disabled" : sigil.Ready ? "hotbar-selected" : hovered ? "hotbar-hover" : "hotbar-normal";
        kit.Art.Piece(spriteBatch, frame, new Vector2(r.X, r.Y + push), Scale, Color.White * alpha);

        string state = sigil.Broken ? "soul-link-broken" : sigil.Strained ? "soul-link-violet" : sigil.Charge == 0 ? "soul-link-pulse" : "soul-link-gold";
        int inset = (int)(Combat.CombatIcons.FrameInset * Scale);
        Color tint = sigil.Ready || sigil.Broken || sigil.Strained ? Color.White : new Color(200, 190, 170);
        kit.Art.Piece(spriteBatch, state, new Vector2(r.X + inset, r.Y + inset + push), Scale, tint * alpha);

        float flash = MathF.Max(_press[SigilIndex] / PressSeconds, sigil.Pressed);
        var well = new Rectangle(r.X + inset, r.Y + inset + (int)push, r.Width - inset * 2, r.Height - inset * 2);
        if (flash > 0f) spriteBatch.Draw(kit.Pixel, well, Color.White * (PressStrength * flash * alpha));

        if (sigil.Broken) return;
        const int pip = 6;
        for (int i = 0; i < sigil.Full; i++)
        {
            var box = new Rectangle(r.Right - CornerInset - (sigil.Full - i) * (pip + 3) + 1, r.Bottom - CornerInset - pip - 1, pip, pip);
            spriteBatch.Draw(kit.Pixel, InflateBy(box, 1), HudKit.BarEdge * alpha);
            spriteBatch.Draw(kit.Pixel, box, (i < sigil.Charge ? Palette.Gold : HudKit.BarBack) * alpha);
        }
    }

    /// <summary>A box grown by a number of pixels each way.</summary>
    private static Rectangle InflateBy(Rectangle box, int by)
    {
        box.Inflate(by, by);
        return box;
    }
}
