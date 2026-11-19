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

    /// <summary>How far over someone's head a line they say goes, in screen pixels, clear of their nameplate.</summary>
    private const float BarkOverPlate = 30f;

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
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
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
        //    words, the numbers and the lines said.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        spriteBatch.Draw(_band, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, _band.Height), Color.White * alpha);
        spriteBatch.End();

        List<SupportBannerView> supports = SupportBanners();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawSupportBannersArt(spriteBatch, supports, alpha);
        _hud.DrawArt(spriteBatch, view, mouse, alpha);
        DrawLessonPulse(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _popups.Draw(spriteBatch, _camera.ToScreen, alpha);
        DrawSupportBannersWords(spriteBatch, supports, alpha);
        _hud.DrawWords(spriteBatch, view, mouse, alpha);
        DrawBubbles(spriteBatch, PlaceBarks(), alpha);
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

    /// <summary>Where the lines said mid-fight go over the speakers' heads, each moved up out of the way of the last.</summary>
    private List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)> PlaceBarks()
    {
        var bubbles = new List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)>();
        var taken = new List<Rectangle>();
        foreach ((WalkerSprite walker, string text, float clock, float length) in _barks)
        {
            Vector2 head = HeadOnScreen(walker) - new Vector2(0f, BarkOverPlate);
            if (PlateTop(walker) is float plate) head.Y = MathF.Min(head.Y, plate - PromptGap);
            if (!OnScreen(head)) continue;

            List<string> rows = Text.Wrap(text, _detailFont, BubbleWidth);
            float width = 0f;
            foreach (string row in rows) width = MathF.Max(width, _detailFont.MeasureString(row).X);
            float height = rows.Count * _detailFont.LineSpacing;
            var box = new Rectangle((int)(head.X - width / 2f - LineSpread.X), (int)(head.Y - PromptGap - height - LineSpread.Y * 2f),
                (int)(width + LineSpread.X * 2f), (int)(height + LineSpread.Y * 2f));
            box.X = Math.Clamp(box.X, (int)Margin, LegendQuestGame.ScreenWidth - (int)Margin - box.Width);

            float shown = MathHelper.Clamp(MathF.Min(clock / BanterFadeInSeconds, (length - clock) / BanterFadeOutSeconds), 0f, 1f);
            bubbles.Add((MakeRoom(box, taken), rows, Palette.Torch, shown));
        }
        return bubbles;
    }

    /// <summary>The top of the HUD's nameplate over someone in the fight, telegraph and all, on screen; or null if they've none. A big creature's plate stands well over HeadOnScreen, and a line said under it covered it.</summary>
    private float? PlateTop(WalkerSprite walker)
    {
        if (_hudView?.Plates is null) return null;
        foreach ((string id, WalkerSprite figure) in _fighters)
        {
            if (figure != walker) continue;
            foreach (NameplateView plate in _hudView.Plates)
                if (plate.Id == id) return Nameplates.Top(_hud.Kit, plate);
        }
        return null;
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
