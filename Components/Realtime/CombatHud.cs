using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The whole real-time HUD in one piece (11-combat.md §1, §7): your frame, the party's rows under
/// it, the target's frame, the boss's, the hotbar with your cast and swing over it, the nameplates,
/// the target's ring and marker, and the card for whatever the mouse is on, all drawn from a
/// HudView.
/// </summary>
/// <remarks>
/// <para>
/// The fight screen builds a HudView from the battle each frame and hands it over: Update once a
/// frame, then DrawRing in a point sampled batch before the figures (it's on the floor, under their
/// feet), DrawArt in a point sampled batch after them, and DrawWords in a linear sampled batch
/// last. Each piece can still be drawn on its own; this only lays them out together and keeps
/// their clocks.
/// </para>
/// <para>
/// The layout follows the view: the party's rows start under however many rows of auras you have,
/// and the target's frame drops under the boss's while there's a boss (the view leaves it out while
/// the target is the boss, whose own frame already shows it). At says what the mouse is
/// on, for clicks (a slot, the sigil, a party row) and for the card; the card is drawn by
/// DrawWords, over a slot or the sigil, or by the mouse for an aura, and what the slot under the
/// mouse would spend shows on your Focus bar.
/// </para>
/// </remarks>
public class CombatHud
{
    /// <summary>What the mouse can be on.</summary>
    public enum Part
    {
        None,
        Slot,
        Sigil,
        PartyRow,
        Aura,
    }

    /// <summary>What the mouse is on: which part, which of them (a slot 0 to 5, a party row from 0), and the aura when it's one.</summary>
    /// <param name="On">Which part.</param>
    /// <param name="Index">Which slot or row, or -1.</param>
    /// <param name="Aura">The aura, for Part.Aura.</param>
    public readonly record struct Spot(Part On, int Index, AuraView Aura)
    {
        /// <summary>Nothing.</summary>
        public static readonly Spot Nothing = new(Part.None, -1, null);
    }

    /// <summary>The room between your aura row and the party's first row, and under the boss's frame for the target's, in screen pixels.</summary>
    private const int PartyDrop = 6;
    private const int TargetDrop = 34;

    /// <summary>The shared art, fonts and clock.</summary>
    public HudKit Kit { get; } = new();

    /// <summary>The pieces, for a screen that wants one on its own (a hit's lag, a slot's flash).</summary>
    public PlayerFrame Player { get; } = new();
    public TargetFrame Target { get; } = new();
    public PartyFrames Party { get; } = new();
    public BossFrame Boss { get; } = new();
    public Hotbar Hotbar { get; } = new();
    public Nameplates Plates { get; } = new();

    /// <summary>The card for whatever the mouse is on.</summary>
    public CombatCard Card { get; } = new();

    /// <summary>Loads the sheets, the icons' index, the fonts and the card.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="device">The graphics device.</param>
    /// <param name="pixel">One white pixel: the screen manager's BlankTexture.</param>
    public void LoadContent(ContentManager content, GraphicsDevice device, Texture2D pixel)
    {
        Kit.LoadContent(content, device, pixel);
        Card.LoadContent(content, pixel);
    }

    /// <summary>Forgets the health lags and the hotbar's flashes, for a new fight.</summary>
    public void Reset()
    {
        Player.Reset();
        Target.Reset();
        Party.Reset();
        Boss.Reset();
        Hotbar.Reset();
    }

    /// <summary>Moves the clock, the hotbar's flashes and glints and the swing on, and lays the frames out for the view. Once a frame, before drawing.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="view">What the HUD shows this frame.</param>
    public void Update(GameTime gameTime, HudView view)
    {
        Kit.Update(gameTime);
        Hotbar.Update(Kit.Delta, view.Slots, view.Player?.Swing ?? -1f);
        Arrange(view);
    }

    /// <summary>What the mouse is on: a slot, the sigil, an aura on any frame or row, or a party row.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="view">What the HUD shows.</param>
    public Spot At(Point mouse, HudView view)
    {
        Arrange(view);
        int slot = Hotbar.At(mouse);
        if (slot == Hotbar.SigilIndex) return view.Sigil is null ? Spot.Nothing : new Spot(Part.Sigil, slot, null);
        if (slot >= 0) return slot < view.Slots.Count && view.Slots[slot] is not null ? new Spot(Part.Slot, slot, null) : Spot.Nothing;

        AuraView aura = Player.AuraAt(mouse, view.Player)
            ?? Target.AuraAt(mouse, view.Target)
            ?? BossFrame.AuraAt(mouse, view.Boss)
            ?? (view.Party is null ? null : Party.AuraAt(mouse, view.Party));
        if (aura is not null) return new Spot(Part.Aura, -1, aura);

        int row = view.Party is null ? -1 : Party.At(mouse, view.Party.Count);
        return row >= 0 ? new Spot(Part.PartyRow, row, null) : Spot.Nothing;
    }

    /// <summary>Draws the ring under the target's feet. Goes in a point sampled batch in screen space, before the figures.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="view">What the HUD shows.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawRing(SpriteBatch spriteBatch, HudView view, float alpha)
    {
        NameplateView target = TargetPlate(view);
        if (target is not null) TargetMarker.DrawRing(spriteBatch, Kit, target.Feet, target.Width, target.Foe, alpha);
    }

    /// <summary>Draws the nameplates, the marker over the target, the frames and the hotbar. Goes in a point sampled batch, after the figures.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="view">What the HUD shows.</param>
    /// <param name="mouse">Where the mouse is, for the slot it lights and what it would spend, or null.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawArt(SpriteBatch spriteBatch, HudView view, Point? mouse, float alpha)
    {
        Spot spot = mouse is { } m ? At(m, view) : Spot.Nothing;
        Player.Spend = spot.On == Part.Slot ? view.Slots[spot.Index].FocusCost : 0;

        if (view.Plates is not null)
        {
            Plates.DrawArt(spriteBatch, Kit, view.Plates, alpha);
            NameplateView target = TargetPlate(view);
            if (target is not null) TargetMarker.DrawChevron(spriteBatch, Kit, new Vector2(target.Head.X, Nameplates.Top(Kit, target)), target.Foe, alpha);
        }
        Player.DrawArt(spriteBatch, Kit, view.Player, alpha);
        if (view.Party is not null) Party.DrawArt(spriteBatch, Kit, view.Party, alpha);
        Boss.DrawArt(spriteBatch, Kit, view.Boss, alpha);
        Target.DrawArt(spriteBatch, Kit, view.Target, alpha);
        int hovered = spot.On is Part.Slot or Part.Sigil ? spot.Index : -1;
        Hotbar.DrawArt(spriteBatch, Kit, view.Slots, view.Sigil, view.Player?.Casting, view.Player?.Swing ?? -1f, hovered, alpha);
    }

    /// <summary>Writes the names, numbers and timers, and draws the card for whatever the mouse is on. Goes in a linear sampled batch, after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="view">What the HUD shows.</param>
    /// <param name="mouse">Where the mouse is, for the card, or null for none.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawWords(SpriteBatch spriteBatch, HudView view, Point? mouse, float alpha)
    {
        if (view.Plates is not null) Plates.DrawWords(spriteBatch, Kit, view.Plates, alpha);
        Player.DrawWords(spriteBatch, Kit, view.Player, alpha);
        if (view.Party is not null) Party.DrawWords(spriteBatch, Kit, view.Party, alpha);
        Boss.DrawWords(spriteBatch, Kit, view.Boss, alpha);
        Target.DrawWords(spriteBatch, Kit, view.Target, alpha);
        Hotbar.DrawWords(spriteBatch, Kit, view.Slots, view.Sigil, view.Player?.Casting, alpha);

        if (mouse is not { } m) return;
        Spot spot = At(m, view);
        switch (spot.On)
        {
            case Part.Slot:
                Hotbar.Describe(Card, view.Slots[spot.Index]);
                Card.DrawAbove(spriteBatch, Hotbar.Slots()[spot.Index], alpha);
                break;
            case Part.Sigil:
                Hotbar.Describe(Card, view.Sigil);
                Card.DrawAbove(spriteBatch, Hotbar.Slots()[Hotbar.SigilIndex], alpha);
                break;
            case Part.Aura:
                AuraRow.Describe(Card, spot.Aura);
                Card.DrawByMouse(spriteBatch, m, alpha);
                break;
        }
    }

    /// <summary>Lays the frames out for the view: the party under your auras, the target under the boss.</summary>
    private void Arrange(HudView view)
    {
        Party.Top = Player.AurasBottom(view.Player) + PartyDrop;
        Target.Top = view.Boss is null ? TargetFrame.DefaultTop : BossFrame.Bottom + TargetDrop;
    }

    /// <summary>The pieces of the screen the HUD covers for a view, for what's drawn over the room (a support's banner) to keep clear of: your frame with its auras, the party's rows, the boss's frame and cast bar, the target's frame, and the hotbar with the swing and cast strips over it.</summary>
    /// <param name="view">What the HUD shows.</param>
    public IEnumerable<Rectangle> Covered(HudView view)
    {
        Arrange(view);
        if (view.Player is not null)
        {
            Rectangle you = Player.Bounds;
            yield return new Rectangle(you.X, you.Y, you.Width, System.Math.Max(you.Height, Player.AurasBottom(view.Player) - you.Y));
        }
        if (view.Party is not null)
            foreach (Rectangle row in Party.Rows(view.Party.Count)) yield return row;
        if (view.Boss is not null)
        {
            yield return BossFrame.Bounds;
            yield return BossFrame.CastArea;
        }
        if (view.Target is not null) yield return Target.Bounds;
        yield return Hotbar.Panel;
        yield return Hotbar.SwingArea;
        yield return Hotbar.CastArea;
    }

    /// <summary>The target's nameplate, or null.</summary>
    private static NameplateView TargetPlate(HudView view)
    {
        if (view.Plates is null) return null;
        foreach (NameplateView plate in view.Plates)
            if (plate.IsTarget) return plate;
        return null;
    }
}
