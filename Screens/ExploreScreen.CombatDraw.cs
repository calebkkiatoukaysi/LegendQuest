using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// Drawing a fight: the room and everyone in it with the marks on the floor (a plate at every slot on
/// your turn, the ones a skill reaches lit, the ones it would catch filled, a chevron over each you
/// can Shift to, and the rings under whoever's turn it is, whoever you're pointing at, and the foe
/// who'd swing at you for Shifting), the dark, what's flying and what's thrown about, then the
/// screen: health and the next move over heads, the supports' banners, the numbers and the lines
/// said, the turn order, the bar with the frame, the budget, the sigil and the hotbar on it, the log,
/// the banner, the card, and the panel at the end.
/// </summary>
/// <remarks>
/// The room's own order of batches (RoomScene's back, the floor, its front, its dark), with the
/// fight's in among them, so it's drawn instead of the room's Draw while a fight's on. The plates lie
/// on the floor under everyone's feet; in a dark room they're drawn again in light over the dark, the
/// way the pool under the mouse is, so they can always be seen. The room's foreground is drawn faint
/// while a fight's on, so a tree in front never hides a foe. The chevrons float in front of
/// everyone, over an empty plate or over the head of a friend you'd swap with. The whole room shakes
/// with a hit, and with a scene's shake still settling; the screen's pieces don't.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>Where the marks on the floor sort: over the ground's props, under the things on walls and everyone standing. The plates, then the rings. The chevrons go in front of everyone.</summary>
    private const float GroundFill = 0.966f;
    private const float GroundRing = 0.964f;
    private const float ChevronDepth = 0.02f;

    /// <summary>How strong the marks are drawn again in light in a dark room.</summary>
    private const float MarksInTheDark = 0.6f;

    /// <summary>How much of the room's foreground (the trees and posts in front of everyone) is drawn while a fight's on, so it never hides anyone in the rows.</summary>
    private const float ForegroundInFight = 0.3f;

    /// <summary>Whether a prop stands in front of anyone in the fight and covers them, so it's drawn faint the way the room's foreground is: a tree on the band mustn't hide a foe you have to aim at.</summary>
    private bool HidesAFighter(PropSprite prop)
    {
        Point size = prop.Size;
        Vector2 at = prop.Foot + prop.Offset;
        var area = new BoundingRectangle(at.X - size.X / 2f, at.Y - size.Y, size.X, size.Y);
        foreach (WalkerSprite walker in _fighters.Values)
            if (prop.Foot.Y > walker.Position.Y && area.CollidesWith(walker.Figure)) return true;
        return false;
    }

    /// <summary>What the corner of the screen says in a fight.</summary>
    private const string CombatHint = "L  ·  LOG      ESC  ·  PAUSE";

    /// <summary>The slot plate's frames: plain, a foe you can reach, an ally you can reach, caught, and one to Shift to.</summary>
    private const int PlatePlain = 0, PlateFoe = 1, PlateAlly = 2, PlateCaught = 3, PlateShift = 4;

    /// <summary>How bright a plain plate is, and a plate on a turn that isn't yours (just the ones under the turn and the mouse).</summary>
    private const float PlainPlate = 0.7f;

    /// <summary>How far over a plate, or over the head of whoever stands on it, a chevron floats, how far it bobs, and how fast, in world pixels and radians a second.</summary>
    private const float ChevronRise = 3f;
    private const float ChevronBob = 2f;
    private const float ChevronRate = 5f;

    /// <summary>How fast the rings turn through their frames, and the pulse on guided ones, in radians a second.</summary>
    private const float RingFrameSeconds = 0.12f;
    private const float PulseRate = 6f;

    /// <summary>How wide a ring's art is, so a big creature gets a bigger one.</summary>
    private const int RingWidth = 24;

    /// <summary>The health bar over someone's head: its size and how far over the head, in screen pixels.</summary>
    private const int OverheadWidth = 36;
    private const int OverheadHeight = 5;
    private const int OverheadRise = 10;

    /// <summary>The room between a foe's health bar and the icon of what it'll do next, in screen pixels.</summary>
    private const int IntentGap = 4;

    /// <summary>How far over the bar the line saying why something can't be done goes, in screen pixels.</summary>
    private const int RefusalRise = 36;

    private static readonly Color OverheadBack = new Color(14, 12, 20) * 0.85f;

    /// <summary>How bright the ring under someone pointed at is.</summary>
    private const float PointedRing = 0.8f;

    /// <summary>What each foe's going to do next, as last worked out, and what it was worked out for.</summary>
    private readonly Dictionary<string, (AbilityDefinition Ability, Combatant Target)> _intents = new();
    private object _intentsFor;

    /// <summary>How many events the screen's shown this fight, so the intents are worked out again only when something's happened.</summary>
    private int _eventsShown;

    /// <summary>Draws the fight, in place of the room's own Draw.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    private void DrawCombat(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        Matrix shake = _cueShake.Transform * _effects.Shake;
        Matrix world = _camera.Transform * shake;
        float pixel = _camera.Pixel;
        float alpha = TransitionAlpha;
        float clock = (float)gameTime.TotalGameTime.TotalSeconds;
        Color tint = WorldTint;

        // 1. The room behind everyone, then the marks on the floor and everything on it, sorted by
        //    how far down the band it stands, then the room in front of everyone.
        _scene.DrawBack(gameTime, spriteBatch, _camera, tint, shake);
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        foreach (PropSprite prop in AllProps()) prop.Draw(spriteBatch, prop.Layer(ZoneCatalog.RoomHeight), HidesAFighter(prop) ? tint * ForegroundInFight : tint);
        foreach (TorchSprite torch in _torches) torch.Draw(spriteBatch, Layers.WallFlame);
        DrawGroundMarks(spriteBatch, clock, 1f);
        foreach (Npc npc in _npcs) npc.Walker.Draw(spriteBatch, OnFloor(npc.Walker.Position), pixel, tint);
        foreach (WalkerSprite walker in _walkers) walker.Draw(spriteBatch, OnFloor(walker.Position), pixel, tint);
        foreach (WalkerSprite walker in _spawned) walker.Draw(spriteBatch, OnFloor(walker.Position), pixel, tint);
        spriteBatch.End();
        _scene.DrawFront(spriteBatch, _camera, tint * ForegroundInFight, shake);

        // 2. The dark, in a dark room, and the torches and the marks again in light over it.
        _scene.DrawDark(spriteBatch, _camera, shake);
        if (_room.Dark)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp, transformMatrix: world);
            foreach (TorchSprite torch in _torches) torch.DrawGlow(spriteBatch, GlowRadius, GlowColor);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.PointClamp, transformMatrix: world);
            DrawGroundMarks(spriteBatch, clock, MarksInTheDark);
            spriteBatch.End();
        }

        // 3. What's thrown about, as light, and what's flying.
        _effects.DrawParticles(spriteBatch, world);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        _effects.DrawFlying(spriteBatch);
        spriteBatch.End();

        // 4. The screen: the band across the top, then the art, then the words.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        spriteBatch.Draw(_band, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, _band.Height), Color.White * alpha);
        spriteBatch.End();

        IReadOnlyList<Combatant> order = _shown.Order;
        var views = new List<FighterView>(order.Count);
        foreach (Combatant each in order) views.Add(View(each));
        int current = CurrentIndex(order);
        Combatant turn = _shown.Turn is null ? null : Fighter(_shown.Turn);
        bool yours = AwaitingPlayer;
        List<SupportView> supports = SupportViews();
        _initiative.SetSupports(ActingSupports());
        int spend = yours ? FocusSpend() : 0;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawOverheads(spriteBatch, alpha);
        DrawSupportBannersArt(spriteBatch, supports, alpha);
        _initiative.DrawArt(spriteBatch, views, current, _hoverPortrait, alpha);
        _hotbar.DrawBar(spriteBatch, alpha);
        if (turn is not null) _unitFrame.DrawArt(spriteBatch, View(turn), Passives(turn), spend, alpha);
        if (_combat.SoulLink.Active) _sigil.DrawArt(spriteBatch, BondWhole(), DuoStrained(), _shown.Charge, alpha);
        if (HotbarOwner is Combatant owner)
        {
            _budgetBar.DrawArt(spriteBatch, owner.Budget, yours ? Spends() : null, yours && _plan?.Shift is not null, alpha);
            _hotbar.DrawArt(spriteBatch, _hotbarEntries, _hoverSlot, _hoverEndTurn, GuidingEndTurn, yours, alpha);
        }
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        DrawHeading(spriteBatch, _encounter.Name.ToUpperInvariant(), Margin, false, alpha);
        DrawHeading(spriteBatch, CombatHint, LegendQuestGame.ScreenWidth - Margin, true, alpha);
        _popups.Draw(spriteBatch, _camera.ToScreen, alpha);
        DrawBubbles(spriteBatch, PlaceBarks(), alpha);
        DrawSupportBannersWords(spriteBatch, supports, alpha);
        _initiative.DrawWords(spriteBatch, views, current, _shown.Round, alpha);
        if (turn is not null) _unitFrame.DrawWords(spriteBatch, View(turn), WhatTheyAre(turn), spend, alpha);
        if (_combat.SoulLink.Active) _sigil.DrawWords(spriteBatch, BondWhole(), _shown.Charge, alpha);
        if (HotbarOwner is not null) _hotbar.DrawWords(spriteBatch, _hotbarEntries, yours, alpha);
        _log.Draw(spriteBatch, alpha);
        DrawRefusal(spriteBatch, alpha);
        _banner.Draw(spriteBatch, alpha);
        if (_cardAbove is Rectangle above) _card.DrawAbove(spriteBatch, above, alpha);
        else if (_cardBelow is Rectangle below) _card.DrawBelow(spriteBatch, below, alpha);
        else _card.DrawByMouse(spriteBatch, _mouse, alpha);
        spriteBatch.End();

        // 5. The panel at the end, the toasts, and a tip's card over all of it.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _result.DrawArt(spriteBatch, alpha);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _result.DrawWords(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp,
            transformMatrix: Matrix.CreateTranslation(0f, ToastDrop, 0f));
        _toast.Draw(spriteBatch, alpha);
        spriteBatch.End();

        DrawTutorial(spriteBatch, alpha);
        ScreenManager.FadeBackBufferToBlack(TransitionPosition);
    }

    /// <summary>Someone in the fight as the screen's showing them, for the turn order, the frame and the bars.</summary>
    private FighterView View(Combatant who)
    {
        WalkerSprite walker = FighterWalker(who.Id);
        (int successes, int failures) = _shown.DeathSaves(who);
        return new FighterView(who.Id, who.Name, walker?.Sheet, walker?.Shape ?? WalkerShape.Person, walker?.Art, who.Side == Side.Foes,
            _shown.Hp(who), who.MaxHp, who.ArmourClass, _shown.Focus(who), _shown.Conditions(who), _shown.IsDown(who), _shown.HasDied(who), successes, failures);
    }

    /// <summary>Everyone on the turn order as the screen's showing them.</summary>
    private List<FighterView> OrderViews()
    {
        var views = new List<FighterView>();
        foreach (Combatant each in _shown.Order) views.Add(View(each));
        return views;
    }

    /// <summary>Whether the bond's whole: you and the Soul-Bound both in the fight, neither fallen, and the bond not broken in the story.</summary>
    private bool BondWhole()
    {
        if (_game.Flags.Contains(SoulBondBroken)) return false;
        foreach (Combatant each in _combat.Combatants)
            if (each.IsDuo && _shown.HasDied(each)) return false;
        return true;
    }

    /// <summary>The story's flag for the bond being broken, which turns the sigil grey (11-combat §3.5).</summary>
    private const string SoulBondBroken = "soul-bond-broken";

    /// <summary>Whether one of you and the Soul-Bound is down, which strains the bond.</summary>
    private bool DuoStrained()
    {
        foreach (Combatant each in _combat.Combatants)
            if (each.IsDuo && _shown.IsDown(each)) return true;
        return false;
    }

    /// <summary>The ability being looked at: the hotbar button under the mouse, the one picked, or the one a click would use.</summary>
    private AbilityDefinition Showing() =>
        _hoverSlot >= 0 && _hoverSlot < _hotbarEntries.Count && _hotbarEntries[_hoverSlot].Ability is AbilityDefinition hovered ? hovered
        : _selected ?? _plan?.Ability;

    /// <summary>What the ability being looked at would spend, for the budget's pale pips.</summary>
    private ActionCost? Spends() => Showing()?.Cost;

    /// <summary>The Focus the ability being looked at would spend, for the frame's Focus bar.</summary>
    private int FocusSpend() => Showing()?.Focus ?? 0;

    /// <summary>
    /// The marks on the floor. On your turn: a plate at every slot, the ones the ability being looked
    /// at can reach lit (red for a foe, gold for a friend), the ones it would catch on whoever the
    /// mouse is on filled, and with no ability in view, a chevron over every slot you can Shift to and
    /// the ember ring under the foe who'd swing at you for the one the mouse is on. Always: the ring
    /// under whoever's turn it is, and under whoever the mouse is on.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space, point-sampled.</param>
    /// <param name="clock">The game's clock, for the rings' frames, the chevrons' bob and the pulses.</param>
    /// <param name="strength">How strong they're drawn: 1 on the floor, less when they're drawn again as light in a dark room.</param>
    private void DrawGroundMarks(SpriteBatch spriteBatch, float clock, float strength)
    {
        float pulse = 0.5f + 0.5f * MathF.Sin(clock * PulseRate);
        int frame = (int)(clock / RingFrameSeconds);

        if (AwaitingPlayer)
        {
            Combatant me = _combat.Current;
            var frames = new Dictionary<Slot, int>();
            foreach (Slot slot in FightStage.AllSlots) frames[slot] = PlatePlain;

            AbilityDefinition showing = Showing();
            if (showing is not null)
            {
                foreach (Combatant target in _combat.Targets(me, showing))
                    foreach (Slot slot in _shown.SlotsOf(target)) frames[slot] = target.Side == me.Side ? PlateAlly : PlateFoe;
                if (_plan is { Ability: not null, Target: Combatant aimed, Problem: null })
                    foreach (Combatant caught in _combat.Caught(me, _plan.Ability, aimed))
                        foreach (Slot slot in _shown.SlotsOf(caught)) frames[slot] = PlateCaught;
            }
            else
            {
                foreach (Slot slot in _combat.Shifts(me)) frames[slot] = PlateShift;
            }

            foreach ((Slot slot, int plate) in frames)
            {
                float bright = plate == PlatePlain ? PlainPlate : plate == PlateFoe && GuidingAttack ? MathHelper.Lerp(PlainPlate, 1f, pulse) : 1f;
                Plate(spriteBatch, slot, plate, Color.White * (bright * strength));
            }

            if (showing is null)
            {
                foreach (Slot slot in _combat.Shifts(me))
                {
                    bool lit = _plan?.Shift == slot;
                    float bright = GuidingMove && !lit ? MathHelper.Lerp(PlainPlate, 1f, pulse) : 1f;
                    Chevron(spriteBatch, slot, lit, clock, Color.White * (bright * strength));
                }
                if (_plan is { Shift: Slot to, Problem: null } && _combat.Opportunist(me, to) is Combatant swinging)
                    Ring(spriteBatch, swinging, true, frame, CombatStyle.Threat * strength);
            }

            if (_plan?.Target is Combatant pointedAt) Ring(spriteBatch, pointedAt, pointedAt.Side != me.Side, frame, Palette.Bone * strength);
        }

        if (_shown.Turn is string id && Fighter(id) is Combatant turn)
            Ring(spriteBatch, turn, false, frame, (turn.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Gold) * strength);
        if (_hoverFighter is Combatant pointed && pointed.Id != _shown.Turn)
            Ring(spriteBatch, pointed, pointed.Side == Side.Foes, frame, Palette.Bone * (PointedRing * strength));
    }

    /// <summary>A slot's plate on the floor, in one of its frames.</summary>
    private void Plate(SpriteBatch spriteBatch, Slot slot, int frame, Color colour)
    {
        BoundingRectangle plate = _stage.Plate(slot);
        Rectangle source = CombatIcons.Piece("slot-plate", frame);
        spriteBatch.Draw(_combatArt.Hud, new Vector2(MathF.Round(plate.X), MathF.Round(plate.Y)), source, colour, 0f, Vector2.Zero, 1f, SpriteEffects.None, GroundFill);
    }

    /// <summary>The chevron over a slot you can Shift to, bobbing: over its plate, or over the head of the friend you'd swap with.</summary>
    private void Chevron(SpriteBatch spriteBatch, Slot slot, bool lit, float clock, Color colour)
    {
        Rectangle source = CombatIcons.Piece("shift-chevron", lit ? 1 : 0);
        float bob = ChevronBob * (0.5f + 0.5f * MathF.Sin(clock * ChevronRate));
        float bottom = _shown.At(slot) is Combatant standing && FighterWalker(standing.Id) is WalkerSprite walker && walker.Opacity > 0f
            ? walker.Figure.Top - ChevronRise
            : _stage.Plate(slot).Y - ChevronRise;
        var at = new Vector2(_stage.FeetAt(slot).X - source.Width / 2f, bottom - source.Height - bob);
        spriteBatch.Draw(_combatArt.Hud, new Vector2(MathF.Round(at.X), MathF.Round(at.Y)), source, colour, 0f, Vector2.Zero, 1f, SpriteEffects.None, ChevronDepth);
    }

    /// <summary>A ring under someone's feet, turning through its frames: a foe's or a friend's, as big as they stand.</summary>
    private void Ring(SpriteBatch spriteBatch, Combatant who, bool foe, int frame, Color colour)
    {
        if (FighterWalker(who.Id) is not WalkerSprite walker || walker.Opacity <= 0f) return;

        Rectangle source = CombatIcons.Piece(foe ? "target-foe" : "target-ally", frame);
        int scale = Math.Max(1, (int)MathF.Round(walker.ShadowSize.X / RingWidth));
        Vector2 at = walker.Position - new Vector2(source.Width * scale / 2f, source.Height * scale / 2f);
        spriteBatch.Draw(_combatArt.Hud, new Vector2(MathF.Round(at.X), MathF.Round(at.Y)), source, colour, 0f, Vector2.Zero, scale, SpriteEffects.None, GroundRing);
    }

    /// <summary>The health bar over everyone's head who's up, and the icon of a foe's next move once you've seen it make it; over anyone of yours who's Downed, their death saves.</summary>
    private void DrawOverheads(SpriteBatch spriteBatch, float alpha)
    {
        Texture2D pixel = ScreenManager.BlankTexture;
        RefreshIntents();
        foreach (Combatant each in _combat.Combatants)
        {
            if (_shown.HasDied(each) || _shown.HasFallenBack(each) || FighterWalker(each.Id) is not WalkerSprite walker || walker.Opacity <= 0f || !_shown.HasRolled(each)) continue;

            Vector2 head = HeadOnScreen(walker);
            float shown = walker.Opacity * alpha;
            if (_shown.IsDown(each))
            {
                if (each.Side == Side.Foes || _encounter.Nonlethal) continue;
                (int successes, int failures) = _shown.DeathSaves(each);
                Rectangle socket = CombatIcons.Piece("death-save-empty");
                float x = head.X - socket.Width * 3f;
                for (int i = 0; i < 3; i++)
                {
                    _combatArt.Piece(spriteBatch, i < successes ? "death-save-success" : "death-save-empty", new Vector2(MathF.Round(x + i * socket.Width), MathF.Round(head.Y - OverheadRise)), 1f, Color.White * shown);
                    _combatArt.Piece(spriteBatch, i < failures ? "death-save-failure" : "death-save-empty", new Vector2(MathF.Round(x + (3 + i) * socket.Width), MathF.Round(head.Y - OverheadRise)), 1f, Color.White * shown);
                }
                continue;
            }

            var bar = new Rectangle((int)(head.X - OverheadWidth / 2f), (int)(head.Y - OverheadRise), OverheadWidth, OverheadHeight);
            float health = each.MaxHp > 0 ? MathHelper.Clamp((float)_shown.Hp(each) / each.MaxHp, 0f, 1f) : 0f;
            spriteBatch.Draw(pixel, bar, OverheadBack * shown);
            spriteBatch.Draw(pixel, new Rectangle(bar.X, bar.Y, (int)MathF.Round(bar.Width * health), bar.Height),
                (each.Side == Side.Foes ? CombatStyle.FoeHealth : CombatStyle.PartyHealth) * shown);

            if (each.Side == Side.Foes && _intents.TryGetValue(each.Id, out var intent) && Seen(each, intent.Ability))
            {
                var icon = new Vector2(bar.Right + IntentGap, bar.Center.Y - CombatIcons.AbilitySize / 2f);
                _combatArt.Ability(spriteBatch, CombatIcons.Ability(intent.Ability.Id), new Vector2(MathF.Round(icon.X), MathF.Round(icon.Y)), 1f, Color.White * shown);
            }
        }
    }

    /// <summary>Works out again what every foe's going to do next, when something's happened since it was last worked out and nothing's playing.</summary>
    private void RefreshIntents()
    {
        if (_beat is not null || _beats.Count > 0) return;
        var key = (_eventsShown, _shown.Turn, _shown.Round);
        if (Equals(key, _intentsFor)) return;
        _intentsFor = key;

        _intents.Clear();
        foreach (Combatant each in _combat.Combatants)
        {
            if (each.Side != Side.Foes || each.IsOut || _shown.HasDied(each)) continue;
            if (_combat.Intent(each) is (AbilityDefinition ability, Combatant target)) _intents[each.Id] = (ability, target);
        }
    }

    /// <summary>Whether you've seen a foe of this kind make a move this session, so its intent's shown.</summary>
    private bool Seen(Combatant foe, AbilityDefinition ability) => ability is not null && _seenMoves.Contains(MoveKey(foe, ability.Id));

    /// <summary>What _seenMoves knows a foe's move by: its kind and the ability.</summary>
    private static string MoveKey(Combatant foe, AbilityId ability) => $"{foe.Enemy?.Id ?? foe.Id}/{ability}";

    /// <summary>Where the lines said mid-fight go over the speakers' heads, each moved up out of the way of the last.</summary>
    private List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)> PlaceBarks()
    {
        var bubbles = new List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)>();
        var taken = new List<Rectangle>();
        foreach ((WalkerSprite walker, string text, float clock, float length) in _barks)
        {
            Vector2 head = HeadOnScreen(walker);
            if (!OnScreen(head)) continue;

            List<string> rows = Text.Wrap(text, _detailFont, BubbleWidth);
            float width = 0f;
            foreach (string row in rows) width = MathF.Max(width, _detailFont.MeasureString(row).X);
            float height = rows.Count * _detailFont.LineSpacing;
            var box = new Rectangle((int)(head.X - width / 2f - LineSpread.X), (int)(head.Y - OverheadRise - PromptGap - height - LineSpread.Y * 2f),
                (int)(width + LineSpread.X * 2f), (int)(height + LineSpread.Y * 2f));
            box.X = Math.Clamp(box.X, (int)Margin, LegendQuestGame.ScreenWidth - (int)Margin - box.Width);

            float shown = MathHelper.Clamp(MathF.Min(clock / BanterFadeInSeconds, (length - clock) / BanterFadeOutSeconds), 0f, 1f);
            bubbles.Add((MakeRoom(box, taken), rows, Palette.Torch, shown));
        }
        return bubbles;
    }

    /// <summary>Why the last thing tried couldn't be done, over the bar, fading.</summary>
    private void DrawRefusal(SpriteBatch spriteBatch, float alpha)
    {
        if (_refusalClock <= 0f || _refusal is null) return;
        float shown = MathHelper.Clamp(_refusalClock / (RefusalSeconds / 3f), 0f, 1f) * alpha;
        Text.Draw(spriteBatch, _detailFont, _refusal.ToUpperInvariant(), CombatHotbar.Left, CombatHotbar.BarTop - RefusalRise,
            Palette.Ember * shown, Palette.Shadow * shown);
    }
}
