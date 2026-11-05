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
/// Drawing a fight: the room and everyone in it with the marks on the ground (where you can reach,
/// the path, the rings under whoever's turn it is and whoever you're pointing at), the dark, what's
/// flying and what's thrown about, then the screen: health over heads, the numbers and the lines
/// said, the turn order, the frame, the sigil, the budget and the hotbar, the log, the banner, the
/// card, and the panel at the end.
/// </summary>
/// <remarks>
/// The room's own order of batches (RoomScene's back, the floor, its front, its dark), with the
/// fight's in among them, so it's drawn instead of the room's Draw while a fight's on. The marks on
/// the ground fill the fight's tiles under everyone's feet; in a dark room they're drawn again in
/// light over the dark, the way the pool under the mouse is, so they can always be seen. The whole
/// room shakes with a hit, and with a scene's shake still settling; the screen's pieces don't.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>Where the marks on the ground sort: over the ground's props, under the things on walls and everyone standing. The fills, then the dots, then the rings.</summary>
    private const float GroundFill = 0.966f;
    private const float GroundDot = 0.965f;
    private const float GroundRing = 0.964f;

    /// <summary>How strong the marks are drawn again in light in a dark room.</summary>
    private const float MarksInTheDark = 0.6f;

    /// <summary>What the corner of the screen says in a fight.</summary>
    private const string CombatHint = "L  ·  LOG      ESC  ·  PAUSE";

    /// <summary>How the ground you can reach is tinted, and how bright it pulses to when the tutorial's pointing at it.</summary>
    private static readonly Color ReachTint = new Color(255, 222, 140) * 0.1f;
    private static readonly Color ReachGuided = new Color(255, 222, 140) * 0.26f;

    /// <summary>How an area's tiles are tinted, as a share of the spell's colour.</summary>
    private const float AreaTint = 0.3f;

    /// <summary>How fast the rings turn through their frames, and the pulse on guided ones, in radians a second.</summary>
    private const float RingFrameSeconds = 0.12f;
    private const float PulseRate = 6f;

    /// <summary>How wide a ring's art is, so a big creature gets a bigger one.</summary>
    private const int RingWidth = 24;

    /// <summary>The health bar over someone's head: its size and how far over the head, in screen pixels.</summary>
    private const int OverheadWidth = 36;
    private const int OverheadHeight = 5;
    private const int OverheadRise = 10;

    /// <summary>How far over the budget the line saying why something can't be done goes, in screen pixels.</summary>
    private const int RefusalRise = 64;

    private static readonly Color OverheadBack = new Color(14, 12, 20) * 0.85f;

    /// <summary>How bright the ring under someone pointed at is.</summary>
    private const float PointedRing = 0.8f;

    /// <summary>The tiles whoever's turn it is can reach, and what they were worked out for.</summary>
    private readonly HashSet<Point> _reach = new();
    private object _reachFor;

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

        // 1. The room behind everyone, then the marks on the ground and everything on the floor,
        //    sorted by how far down the band it stands, then the room in front of everyone.
        _scene.DrawBack(gameTime, spriteBatch, _camera, tint, shake);
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        foreach (PropSprite prop in AllProps()) prop.Draw(spriteBatch, prop.Layer(ZoneCatalog.RoomHeight), tint);
        foreach (TorchSprite torch in _torches) torch.Draw(spriteBatch, Layers.WallFlame);
        DrawGroundMarks(spriteBatch, clock, 1f);
        foreach (Npc npc in _npcs) npc.Walker.Draw(spriteBatch, OnFloor(npc.Walker.Position), pixel, tint);
        foreach (WalkerSprite walker in _walkers) walker.Draw(spriteBatch, OnFloor(walker.Position), pixel, tint);
        foreach (WalkerSprite walker in _spawned) walker.Draw(spriteBatch, OnFloor(walker.Position), pixel, tint);
        spriteBatch.End();
        _scene.DrawFront(spriteBatch, _camera, tint, shake);

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

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawOverheads(spriteBatch, alpha);
        _initiative.DrawArt(spriteBatch, views, current, _hoverPortrait, alpha);
        if (turn is not null) _unitFrame.DrawArt(spriteBatch, View(turn), Passives(turn), alpha);
        if (_combat.SoulLink.Active) _sigil.DrawArt(spriteBatch, DuoStanding(), DuoStrained(), _shown.Charge, alpha);
        if (HotbarOwner is Combatant owner)
        {
            _budgetBar.DrawArt(spriteBatch, owner.Budget, yours ? Spends() : null, yours ? PathMetres() : 0f, alpha);
            _hotbar.DrawArt(spriteBatch, _hotbarEntries, _hoverSlot, _hoverEndTurn, GuidingEndTurn, yours, alpha);
        }
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        DrawHeading(spriteBatch, _encounter.Name.ToUpperInvariant(), Margin, false, alpha);
        DrawHeading(spriteBatch, CombatHint, LegendQuestGame.ScreenWidth - Margin, true, alpha);
        _popups.Draw(spriteBatch, _camera.ToScreen, alpha);
        DrawBubbles(spriteBatch, PlaceBarks(), alpha);
        _initiative.DrawWords(spriteBatch, views, current, _shown.Round, alpha);
        if (turn is not null) _unitFrame.DrawWords(spriteBatch, View(turn), WhatTheyAre(turn), turn.Speed, alpha);
        if (_combat.SoulLink.Active) _sigil.DrawWords(spriteBatch, DuoStanding(), _shown.Charge, alpha);
        if (HotbarOwner is Combatant hotbar)
        {
            _budgetBar.DrawWords(spriteBatch, hotbar.Budget, yours ? PathMetres() : 0f, alpha);
            _hotbar.DrawWords(spriteBatch, _hotbarEntries, yours, alpha);
        }
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
        return new FighterView(who.Id, who.Name, walker?.Sheet, walker?.Shape ?? WalkerShape.Person, who.Side == Side.Foes,
            _shown.Hp(who), who.MaxHp, who.ArmourClass, _shown.Conditions(who), _shown.IsDown(who), _shown.HasDied(who), successes, failures);
    }

    /// <summary>Everyone on the turn order as the screen's showing them.</summary>
    private List<FighterView> OrderViews()
    {
        var views = new List<FighterView>();
        foreach (Combatant each in _shown.Order) views.Add(View(each));
        return views;
    }

    /// <summary>Whether you and the Soul-Bound are both in the fight and neither has fallen, so the bond's whole.</summary>
    private bool DuoStanding()
    {
        foreach (Combatant each in _combat.Combatants)
            if (each.IsDuo && _shown.HasDied(each)) return false;
        return true;
    }

    /// <summary>Whether one of you and the Soul-Bound is down, which strains the bond.</summary>
    private bool DuoStrained()
    {
        foreach (Combatant each in _combat.Combatants)
            if (each.IsDuo && _shown.IsDown(each)) return true;
        return false;
    }

    /// <summary>What the thing you're pointing at would spend, for the budget's pale pips: the picked ability's cost, or the planned one's.</summary>
    private ActionCost? Spends()
    {
        if (_hoverSlot >= 0) return _hotbarEntries[_hoverSlot].Ability.Cost;
        if (_plan is { Problem: null, Ability: AbilityDefinition ability }) return ability.Cost;
        return _selected?.Cost;
    }

    /// <summary>How far the planned walk would go, in metres, for the budget's bar: all of it for a walk, so the bar shows when it's too far.</summary>
    private float PathMetres() => _plan is null ? 0f : _plan.Path.Count * Measure.TileMetres;

    /// <summary>The marks on the ground: the tiles you can reach, an area's tiles, the path and where it ends, and the rings under whoever's turn it is, whoever you're pointing at, and anyone the path would let swing at you.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space, point-sampled.</param>
    /// <param name="clock">The game's clock, for the rings' frames and the pulses.</param>
    /// <param name="strength">How strong they're drawn: 1 on the ground, less when they're drawn again as light in a dark room.</param>
    private void DrawGroundMarks(SpriteBatch spriteBatch, float clock, float strength)
    {
        float pulse = 0.5f + 0.5f * MathF.Sin(clock * PulseRate);
        int frame = (int)(clock / RingFrameSeconds);
        Texture2D pixel = ScreenManager.BlankTexture;

        if (AwaitingPlayer)
        {
            Combatant me = _combat.Current;
            if (_selected is null)
            {
                Color reach = (GuidingMove ? Color.Lerp(ReachTint, ReachGuided, pulse) : ReachTint) * strength;
                foreach (Point cell in Reach(me)) Fill(spriteBatch, pixel, cell, reach);
            }

            if (_plan is { Ability: { Target: TargetKind.Area or TargetKind.Point } area } && _plan.Problem is null)
            {
                Color fill = CombatStyle.LightOf(area) * (AreaTint * strength);
                foreach (Point cell in Around(_plan.Cell, area.Radius)) Fill(spriteBatch, pixel, cell, fill);
            }

            if (_plan is not null) DrawPath(spriteBatch, _plan, strength);
            if (_plan?.Target is Combatant aimed) Ring(spriteBatch, aimed, aimed.Side != me.Side, frame, Palette.Bone * strength);
            if (GuidingAttack)
                foreach (Combatant each in _combat.Combatants)
                    if (each.Side != me.Side && !_shown.HasDied(each)) Ring(spriteBatch, each, true, frame, Palette.Gold * (pulse * strength));
            if (_plan is not null) DrawThreats(spriteBatch, me, _plan, frame, strength);
        }

        if (_shown.Turn is string id && Fighter(id) is Combatant turn)
            Ring(spriteBatch, turn, false, frame, (turn.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Gold) * strength);
        if (_hoverFighter is Combatant pointed && pointed.Id != _shown.Turn)
            Ring(spriteBatch, pointed, pointed.Side == Side.Foes, frame, Palette.Bone * (PointedRing * strength));
    }

    /// <summary>The dots of a planned walk, pale past what this turn's movement covers, and a mark where it stops (and another where it would end, if that's further).</summary>
    private void DrawPath(SpriteBatch spriteBatch, TurnPlan plan, float strength)
    {
        if (plan.Path.Count == 0 || plan.Problem is not null) return;

        Color colour = Color.White * strength;
        for (int i = 0; i < plan.Path.Count - 1; i++)
        {
            string piece = i < plan.Reachable ? "path-dot" : "path-dot-far";
            Rectangle dot = CombatIcons.Piece(piece);
            spriteBatch.Draw(_combatArt.Hud, FeetOn(plan.Path[i]) - new Vector2(dot.Width / 2f, dot.Height / 2f), dot, colour, 0f, Vector2.Zero, 1f, SpriteEffects.None, GroundDot);
        }

        bool cutShort = plan.Reachable < plan.Path.Count;
        Rectangle end = CombatIcons.Piece(cutShort ? "path-end-far" : "path-end");
        spriteBatch.Draw(_combatArt.Hud, FeetOn(plan.Path[^1]) - new Vector2(end.Width / 2f, end.Height / 2f), end, colour, 0f, Vector2.Zero, 1f, SpriteEffects.None, GroundDot);
        if (cutShort && plan.Reachable > 0)
        {
            Rectangle stop = CombatIcons.Piece("path-end");
            spriteBatch.Draw(_combatArt.Hud, FeetOn(plan.Path[plan.Reachable - 1]) - new Vector2(stop.Width / 2f, stop.Height / 2f), stop, colour, 0f, Vector2.Zero, 1f, SpriteEffects.None, GroundDot);
        }
    }

    /// <summary>An ember ring under every foe the planned walk would let swing at you.</summary>
    private void DrawThreats(SpriteBatch spriteBatch, Combatant me, TurnPlan plan, int frame, float strength)
    {
        int steps = plan.Ability is null ? Math.Min(plan.Reachable, plan.Path.Count) : plan.Path.Count;
        if (steps == 0 || plan.Problem is not null) return;

        var walked = new List<Point>();
        for (int i = 0; i < steps; i++) walked.Add(plan.Path[i]);
        foreach (Combatant threat in _combat.ThreatsAlong(me, me.Cell, walked)) Ring(spriteBatch, threat, true, frame, Palette.Ember * strength);
    }

    /// <summary>A ring under someone's feet, turning through its frames: a foe's or a friend's, as big as they stand.</summary>
    private void Ring(SpriteBatch spriteBatch, Combatant who, bool foe, int frame, Color colour)
    {
        if (FighterWalker(who.Id) is not WalkerSprite walker || walker.Opacity <= 0f) return;

        Rectangle source = CombatIcons.Piece(foe ? "target-foe" : "target-ally", frame);
        int scale = Math.Max(1, (int)MathF.Round(walker.Shape.Shadow.X / RingWidth));
        Vector2 at = walker.Position - new Vector2(source.Width * scale / 2f, source.Height * scale / 2f);
        spriteBatch.Draw(_combatArt.Hud, new Vector2(MathF.Round(at.X), MathF.Round(at.Y)), source, colour, 0f, Vector2.Zero, scale, SpriteEffects.None, GroundRing);
    }

    /// <summary>The tiles whoever's turn it is can walk to and stop on this turn, worked out again only when where they are or what's left of their movement changes.</summary>
    private HashSet<Point> Reach(Combatant me)
    {
        var key = (me.Id, me.Cell, me.Budget.MovementLeft);
        if (Equals(key, _reachFor)) return _reach;

        _reachFor = key;
        _reach.Clear();
        foreach (Point cell in _combat.Flood(me, me.Budget.MovementLeft).Keys)
            if (cell != me.Cell && _combat.At(cell, me) is null) _reach.Add(cell);
        return _reach;
    }

    /// <summary>Every tile within a distance of one, on the fight's ground.</summary>
    private IEnumerable<Point> Around(Point centre, float metres)
    {
        int tiles = Measure.Tiles(metres);
        for (int y = centre.Y - tiles; y <= centre.Y + tiles; y++)
            for (int x = centre.X - tiles; x <= centre.X + tiles; x++)
                if (x >= 0 && y >= 0 && x < _ground.Width && y < _ground.Height) yield return new Point(x, y);
    }

    /// <summary>Fills the patch of floor a tile covers, under everyone's feet.</summary>
    private void Fill(SpriteBatch spriteBatch, Texture2D pixel, Point cell, Color colour)
    {
        BoundingRectangle area = _ground.Area(cell);
        spriteBatch.Draw(pixel, new Vector2(area.X, area.Y), null, colour, 0f, Vector2.Zero, new Vector2(area.Width, area.Height), SpriteEffects.None, GroundFill);
    }

    /// <summary>The health bar over everyone's head who's up, and over anyone of yours who's Downed, their death saves.</summary>
    private void DrawOverheads(SpriteBatch spriteBatch, float alpha)
    {
        Texture2D pixel = ScreenManager.BlankTexture;
        foreach (Combatant each in _combat.Combatants)
        {
            if (_shown.HasDied(each) || FighterWalker(each.Id) is not WalkerSprite walker || walker.Opacity <= 0f || !_shown.HasRolled(each)) continue;

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
        }
    }

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

    /// <summary>Why the last thing tried couldn't be done, over the hotbar, fading.</summary>
    private void DrawRefusal(SpriteBatch spriteBatch, float alpha)
    {
        if (_refusalClock <= 0f || _refusal is null) return;
        float shown = MathHelper.Clamp(_refusalClock / (RefusalSeconds / 3f), 0f, 1f) * alpha;
        Text.Draw(spriteBatch, _detailFont, _refusal.ToUpperInvariant(), CombatHotbar.Left, CombatHotbar.Top - RefusalRise,
            Palette.Ember * shown, Palette.Shadow * shown);
    }
}
