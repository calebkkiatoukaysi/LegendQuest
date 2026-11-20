using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Characters.Motion;
using LegendQuest.Collisions;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components;
using LegendQuest.Components.Realtime;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// Drawing a fight: the room and everyone in it, with the markers on the floor and the ring under
/// your target, the dark, the weapons' trails, what's flying and what's thrown about, then the
/// screen: the nameplates, the frames and the hotbar (CombatHud), the numbers, the lines said, the
/// supports' banners, the log, the banner, the card, and the panel at the end.
/// </summary>
/// <remarks>
/// The room's own order of batches (RoomScene's back, the floor, its front, its dark) with the
/// fight's in among them (Characters/WalkerSprite's draw order for blows: the ground markers under
/// everyone, the figures, then their trails, then the particles), so it's drawn instead of the
/// room's Draw while a fight's on. The room's foreground is drawn faint while a fight's on, and so is
/// a prop that stands in front of a fighter, so a tree never hides a foe. The whole room shakes with
/// a blow, and with a scene's shake still settling; the HUD doesn't.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How much of the room's foreground (the trees and posts in front of everyone) is drawn while a fight's on.</summary>
    private const float ForegroundInFight = 0.3f;

    /// <summary>How strong the ring under your target is.</summary>
    private const float RingStrength = 0.9f;

    /// <summary>What the top right corner of the screen says in a fight: short, so it stays clear of the target's and the boss's frames in the middle.</summary>
    private const string CombatHint = "L  ·  LOG      ESC  ·  PAUSE";

    /// <summary>How far over the hotbar the line saying why something can't be done goes, in screen pixels.</summary>
    private const int RefusalRise = 16;

    /// <summary>The widest a line said mid-fight runs in the barks' lane before it wraps, and the room between two lines there, in screen pixels.</summary>
    private const float BarkLaneWidth = 640f;
    private const int BarkLaneGap = 4;

    /// <summary>How faint the supports' banners go while the banner's across the middle, which they'd otherwise show through.</summary>
    private const float SupportsUnderBanner = 0.2f;

    /// <summary>What the numbers and words over the room keep off this frame, and everyone's nameplate by their id; kept to save making them every frame.</summary>
    private readonly List<Rectangle> _keepOff = new();
    private readonly List<(string Id, Rectangle Box)> _plates = new();

    /// <summary>Whether a prop stands in front of anyone in the fight and covers them, so it's drawn faint the way the room's foreground is.</summary>
    private bool HidesAFighter(PropSprite prop)
    {
        Point size = prop.Size;
        Vector2 at = prop.Foot + prop.Offset;
        var area = new BoundingRectangle(at.X - size.X / 2f, at.Y - size.Y, size.X, size.Y);
        foreach (WalkerSprite walker in _fighters.Values)
            if (prop.Foot.Y > walker.Position.Y && area.CollidesWith(walker.Figure)) return true;
        return false;
    }

    /// <summary>Draws the fight, in place of the room's own Draw.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    private void DrawCombat(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        Matrix shake = _cueShake.Transform * _effects.Shake;
        Matrix world = _camera.Transform * shake;
        float pixel = _camera.Pixel;
        float alpha = TransitionAlpha;
        Color tint = WorldTint;
        HudView view = _hudView ?? BuildHudView();
        Point? mouse = _proofAutoplay && _proofAim is null ? null : _mouse;

        // 1. The room behind everyone, the markers on the floor and the ring under your target, then
        //    everything standing on the floor sorted by how far down the band it stands, then the
        //    room in front of everyone, faint.
        _scene.DrawBack(gameTime, spriteBatch, _camera, tint, shake);
        _effects.DrawGround(world);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _hud.DrawRing(spriteBatch, view, alpha * RingStrength);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        foreach (PropSprite prop in AllProps()) prop.Draw(spriteBatch, prop.Layer(ZoneCatalog.RoomHeight), HidesAFighter(prop) ? tint * ForegroundInFight : tint);
        foreach (TorchSprite torch in _torches) torch.Draw(spriteBatch, Layers.WallFlame);
        foreach (Npc npc in _npcs) npc.Walker.Draw(spriteBatch, OnFloor(npc.Walker.Position), pixel, tint);
        foreach (WalkerSprite walker in _walkers) walker.Draw(spriteBatch, OnFloor(walker.Position), pixel, tint);
        foreach (WalkerSprite walker in _spawned) walker.Draw(spriteBatch, OnFloor(walker.Position), pixel, tint);
        spriteBatch.End();
        _scene.DrawFront(spriteBatch, _camera, tint * ForegroundInFight, shake);

        // 2. The dark, in a dark room, and the torches in light over it.
        _scene.DrawDark(spriteBatch, _camera, shake);
        if (_room.Dark)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp, transformMatrix: world);
            foreach (TorchSprite torch in _torches) torch.DrawGlow(spriteBatch, GlowRadius, GlowColor);
            spriteBatch.End();
        }

        // 3. The weapons' trails, what's thrown about as light, and what's flying.
        if (_effects.Ribbons is RibbonBatch ribbons)
        {
            ribbons.Begin(world);
            foreach (WalkerSprite walker in _fighters.Values) walker.DrawTrail(ribbons);
            ribbons.End();
        }
        _effects.DrawParticles(spriteBatch, world);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        _effects.DrawFlying(spriteBatch);
        spriteBatch.End();

        // 4. The screen: the band across the top, then the HUD's art, the supports' banners, the
        //    words, the numbers (kept off everything else) and the lines said.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        spriteBatch.Draw(_band, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, _band.Height), Color.White * alpha);
        spriteBatch.End();

        List<SupportBannerView> supports = SupportBanners();
        float supportAlpha = alpha * MathHelper.Lerp(1f, SupportsUnderBanner, _banner.Showing);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawSupportBannersArt(spriteBatch, supports, supportAlpha);
        _hud.DrawArt(spriteBatch, view, mouse, alpha);
        DrawLessonPulse(spriteBatch, alpha);
        spriteBatch.End();

        List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)> barks = PlaceBarks();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _popups.Draw(spriteBatch, _camera.ToScreen, alpha, KeepOff(view, supports, barks), _plates,
            id => _fighters.TryGetValue(id, out WalkerSprite figure) ? figure.Position : null);
        DrawSupportBannersWords(spriteBatch, supports, supportAlpha);
        _hud.DrawWords(spriteBatch, view, mouse, alpha);
        DrawBubbles(spriteBatch, barks, alpha);
        DrawHeading(spriteBatch, CombatHint, LegendQuestGame.ScreenWidth - Margin, true, alpha);
        _log.Draw(spriteBatch, alpha);
        DrawRefusal(spriteBatch, alpha);
        _banner.Draw(spriteBatch, alpha);
        DrawFighterCard(spriteBatch, alpha);
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

    /// <summary>
    /// Where the lines said mid-fight go: a lane of their own across the middle of the screen, under
    /// the HUD's frames at the top (the boss's or the target's, with their cast bars and auras), each
    /// under the one before, oldest at the top. Over the speaker's head a line sat on their nameplate
    /// and their numbers, and a boss's on its own; every line says who's speaking, so it needn't be
    /// over them. One still waiting for the banner to go isn't placed.
    /// </summary>
    private List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)> PlaceBarks()
    {
        var bubbles = new List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)>();
        int top = HudClear;
        foreach ((WalkerSprite _, string text, float clock, float length) in _barks)
        {
            if (clock < 0f) continue;
            List<string> rows = Text.Wrap(text, _detailFont, BarkLaneWidth);
            float width = 0f;
            foreach (string row in rows) width = MathF.Max(width, _detailFont.MeasureString(row).X);
            float height = rows.Count * _detailFont.LineSpacing;
            var box = new Rectangle((int)(LegendQuestGame.ScreenWidth / 2f - width / 2f - LineSpread.X), top,
                (int)(width + LineSpread.X * 2f), (int)(height + LineSpread.Y * 2f));
            box = ClearOfHud(box);

            float shown = MathHelper.Clamp(MathF.Min(clock / BanterFadeInSeconds, (length - clock) / BanterFadeOutSeconds), 0f, 1f);
            bubbles.Add((box, rows, Palette.Torch, shown));
            top = box.Bottom + BarkLaneGap;
        }
        return bubbles;
    }

    /// <summary>
    /// What the numbers and words over the room keep off, on the screen: the HUD's frames and aura
    /// rows, everyone's nameplate, the supports' banners, the barks, the banner while it's up, the log,
    /// the hint in the corner and the line saying why something can't be done. Fills in _plates too.
    /// </summary>
    private List<Rectangle> KeepOff(HudView view, IReadOnlyList<SupportBannerView> supports, List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)> barks)
    {
        _keepOff.Clear();
        _plates.Clear();
        _keepOff.AddRange(_hud.Covered(view, true));
        if (view.Plates is not null)
            foreach (NameplateView plate in view.Plates)
            {
                Rectangle bounds = Nameplates.Bounds(_hud.Kit, plate);
                _keepOff.Add(bounds);
                _plates.Add((plate.Id, bounds));
            }
        foreach (SupportBannerView support in supports)
            if (_supportFigures.TryGetValue(support.Id, out WalkerSprite walker) && walker.Opacity > 0f) _keepOff.Add(SupportBannerOver(walker, support));
        foreach ((Rectangle box, _, _, _) in barks) _keepOff.Add(box);
        if (_banner.Area is Rectangle banner) _keepOff.Add(banner);
        _keepOff.Add(_log.Bounds);

        Vector2 hint = _detailFont.MeasureString(CombatHint);
        _keepOff.Add(new Rectangle((int)(LegendQuestGame.ScreenWidth - Margin - hint.X), (int)(HeadingY - HeadingSpread.Y), (int)hint.X, (int)(hint.Y + HeadingSpread.Y * 2f)));
        if (_refusalClock > 0f && _refusal is not null)
        {
            Vector2 refusal = _detailFont.MeasureString(_refusal.ToUpperInvariant());
            Rectangle over = Hotbar.CastArea;
            _keepOff.Add(new Rectangle((int)(over.Center.X - refusal.X / 2f), over.Y - RefusalRise - _detailFont.LineSpacing, (int)refusal.X, _detailFont.LineSpacing));
        }
        return _keepOff;
    }

    /// <summary>Why the last thing tried couldn't be done, over the hotbar, fading.</summary>
    private void DrawRefusal(SpriteBatch spriteBatch, float alpha)
    {
        if (_refusalClock <= 0f || _refusal is null) return;
        float shown = MathHelper.Clamp(_refusalClock / (RefusalSeconds / 3f), 0f, 1f) * alpha;
        Rectangle over = Hotbar.CastArea;
        Text.Draw(spriteBatch, _detailFont, _refusal.ToUpperInvariant(), over.Center.X, over.Y - RefusalRise - _detailFont.LineSpacing,
            Palette.Ember * shown, Palette.Shadow * shown, centred: true);
    }
}
