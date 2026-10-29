using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Story;

namespace LegendQuest.Components;

/// <summary>
/// One of the story's paintings, filling the screen, its layers swaying against each other, and
/// whatever moves over it: rain, lightning, glints on the water, a lantern swinging, and the two of
/// you as silhouettes with the Soul-Bond's mark glowing between you.
/// </summary>
/// <remarks>
/// The title background's sway, for any painting: the whole view drifts from side to side on a slow
/// sine and each layer moves by its depth's share of that, like the parallax tutorial's layers
/// except nothing scrolls. A layer is a little wider than the screen so the sway never shows an
/// edge. What moves over a painting is in Story/Paintings.cs, and it's all worked out from the
/// clock the same way every time (where each raindrop is, when the lightning strikes, where a glint
/// turns up), so nothing's random and nothing has to be kept per drop. The figures are the two of
/// you off your own baked sheets, standing facing away, turned into silhouettes once when they're
/// handed over: dark all through, with a hint of your own light and dark, and an edge of the light
/// behind you that's brightest where it faces it.
/// </remarks>
public class PaintingSprite
{
    /// <summary>How much bigger an art pixel is on screen. The same as the title's.</summary>
    private const float Scale = 4f;

    /// <summary>The canvas every layer is drawn on, in art pixels. The same as the title's.</summary>
    private const int CanvasWidth = 416;
    private const int CanvasHeight = 277;

    /// <summary>How far the nearest layer sways either way, in art pixels, and how long a sway takes there and back.</summary>
    private const float SwayReach = 6f;
    private const float SwaySeconds = 22f;

    /// <summary>weather.png: a white cell the flash is stretched from, then a near raindrop and a far one. Has to match WEATHER_CELL in tools/generate_paintings.py.</summary>
    private static readonly Point WeatherCell = new(8, 16);
    private static readonly Rectangle FlashCell = new(0, 0, 1, 1);
    private const int NearDrop = 1;
    private const int FarDrop = 2;

    /// <summary>The rain: one drop in this many is a near one; how fast near and far ones fall (art pixels a second, give or take RainSpread of that); how far a drop goes sideways for each pixel down (the slant they're drawn at); how strong each kind is; and the sway it takes, all of it, since it's in front of everything.</summary>
    private const int NearEvery = 3;
    private const float NearSpeed = 300f;
    private const float FarSpeed = 190f;
    private const float RainSpread = 0.3f;
    private const float RainSlant = 0.25f;
    private const float NearRain = 0.8f;
    private const float FarRain = 0.45f;
    private const float RainDepth = 1f;
    private const int RainSeed = 61;

    /// <summary>bolts.png: the bolts side by side, each coming down from the middle of its top edge. Has to match BOLT_CELL.</summary>
    private static readonly Point BoltCell = new(44, 92);
    private const int BoltFrames = 2;

    /// <summary>When the lightning strikes: the first soon after the painting comes up, then between the shortest and longest gap after the last.</summary>
    private const float FirstStrike = 1.5f;
    private const float ShortestGap = 5f;
    private const float LongestGap = 11f;
    private const int StrikeSeed = 41;

    /// <summary>A strike's flash, seconds after it: full till FlashHold, dimmed to DipStrength till FlashDip, back to SecondStrength till FlashSecond, then fading out over FlashFade. The bolt's there while it's full and while it's back.</summary>
    private const float FlashHold = 0.06f;
    private const float FlashDip = 0.11f;
    private const float FlashSecond = 0.18f;
    private const float FlashFade = 0.6f;
    private const float DipStrength = 0.3f;
    private const float SecondStrength = 0.75f;

    /// <summary>How the flash lights the sky behind everything else, and how much it lights everything in front.</summary>
    private static readonly Color SkyFlash = new(196, 210, 255);
    private const float SkyFlashStrength = 0.5f;
    private const float FrontFlashStrength = 0.2f;

    /// <summary>glint.png: a speck, a plus and a little star. Has to match GLINT_CELL. Each glint turns up somewhere new every GlintSeconds and is lit for the first GlintLit of that, growing and going again.</summary>
    private const int GlintCell = 5;
    private const int GlintFrames = 3;
    private const float GlintSeconds = 1.8f;
    private const float GlintLit = 0.4f;
    private const int GlintSeed = 23;

    /// <summary>sigil.png: the Soul-Bond's glow, then its mark, each a square centred where it goes. Has to match SIGIL_CELL. They pulse once every SigilSeconds, the glow from SigilLow to full and the mark from MarkLow.</summary>
    private const int SigilCell = 24;
    private const float SigilSeconds = 2.4f;
    private const float SigilLow = 0.35f;
    private const float MarkLow = 0.7f;

    /// <summary>lantern.png: the lantern swung left to swung right, each frame hanging from the middle of its top edge. Has to match LANTERN_CELL and LANTERN_SWING. A swing there and back takes LanternSeconds.</summary>
    private static readonly Point LanternCell = new(16, 26);
    private const int LanternFrames = 5;
    private const float LanternSeconds = 2.8f;

    /// <summary>On a walking sheet, the frame standing facing away (column 0, row 3), and where its feet are in it.</summary>
    private static readonly Rectangle StandingAway = new(0, 144, 32, 48);
    private static readonly Vector2 Feet = new(16, 46);

    /// <summary>The silhouettes: how much of a figure's own light and dark shows through (a hint of hair and cloak), how squarely an edge has to face the light to catch it, and the least it catches when it does.</summary>
    private const float Detail = 0.18f;
    private const float RimFrom = 0.25f;
    private const float RimLeast = 0.35f;

    /// <summary>The eight pixels round one, for finding a silhouette's edges.</summary>
    private static readonly Point[] Around = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1), new(1, 1), new(-1, 1), new(1, -1), new(-1, -1) };

    private readonly Dictionary<string, List<(string Name, Texture2D Texture, float Depth)>> _loaded = new();
    private readonly Dictionary<string, Texture2D[]> _figures = new();
    private Texture2D _weather;
    private Texture2D _bolts;
    private Texture2D _glint;
    private Texture2D _sigil;
    private Texture2D _lantern;
    private float _clock;
    private float _lastStrike;
    private float _nextStrike = FirstStrike;
    private int _strikes;

    /// <summary>Loads every painting's layers, and the pictures that move over them.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        foreach (string id in Paintings.All)
        {
            var layers = new List<(string, Texture2D, float)>();
            foreach ((string layer, float depth) in Paintings.Get(id).Layers)
                layers.Add((layer, content.Load<Texture2D>("Paintings/" + id + "-" + layer), depth));
            _loaded[id] = layers;
        }
        _weather = content.Load<Texture2D>("Paintings/weather");
        _bolts = content.Load<Texture2D>("Paintings/bolts");
        _glint = content.Load<Texture2D>("Paintings/glint");
        _sigil = content.Load<Texture2D>("Paintings/sigil");
        _lantern = content.Load<Texture2D>("Paintings/lantern");
    }

    /// <summary>Makes your and the Soul-Bound's silhouettes for every painting the two of you are in, from your baked sheets.</summary>
    /// <remarks>The silhouettes are kept till Unload; the sheets aren't, so whoever baked them can let them go.</remarks>
    /// <param name="you">Your baked sheet.</param>
    /// <param name="soulBound">The Soul-Bound's baked sheet.</param>
    public void SetFigures(Texture2D you, Texture2D soulBound)
    {
        Unload();
        Texture2D[] sheets = { you, soulBound };
        foreach (string id in Paintings.All)
        {
            Painting painting = Paintings.Get(id);
            if (painting.Figures.Count == 0) continue;

            var figures = new Texture2D[sheets.Length];
            for (int i = 0; i < sheets.Length; i++) figures[i] = Silhouette(sheets[i], painting, painting.Figures[i]);
            _figures[id] = figures;
        }
    }

    /// <summary>Lets go of the silhouettes. The layers belong to the content manager.</summary>
    public void Unload()
    {
        foreach (Texture2D[] figures in _figures.Values)
            foreach (Texture2D figure in figures) figure.Dispose();
        _figures.Clear();
    }

    /// <summary>Runs the clock everything moves by, and the lightning's strikes.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;
        while (_clock >= _nextStrike)
        {
            _lastStrike = _nextStrike;
            _strikes++;
            _nextStrike += ShortestGap + Hash(_strikes, 0, StrikeSeed) * (LongestGap - ShortestGap);
        }
    }

    /// <summary>Draws a painting over the whole screen: its layers back to front with what's on each, then the rain and the flash in front of it all.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="id">Which painting.</param>
    /// <param name="alpha">How far it's faded in.</param>
    public void Draw(SpriteBatch spriteBatch, string id, float alpha)
    {
        if (!_loaded.TryGetValue(id, out var layers)) return;

        Painting painting = Paintings.Get(id);
        bool lightning = !painting.Lightning.IsEmpty;
        for (int i = 0; i < layers.Count; i++)
        {
            (string name, Texture2D texture, float depth) = layers[i];
            DrawArt(spriteBatch, texture, texture.Bounds, Vector2.Zero, depth, Color.White * alpha);
            if (i == 0 && lightning) DrawLightning(spriteBatch, painting, depth, alpha);
            if (name == painting.WaterLayer) DrawGlints(spriteBatch, painting, depth, alpha);
            if (name == painting.FiguresLayer) DrawFigures(spriteBatch, id, painting, depth, alpha);
            if (name == painting.LanternLayer) DrawLantern(spriteBatch, painting, depth, alpha);
        }

        DrawRain(spriteBatch, painting, lightning, alpha);
        if (lightning) DrawFlash(spriteBatch, Color.White * (Flash() * FrontFlashStrength * alpha));
    }

    /// <summary>The bolt, while it's there, and the sky lit up behind everything in front of it.</summary>
    private void DrawLightning(SpriteBatch spriteBatch, Painting painting, float depth, float alpha)
    {
        if (_strikes == 0) return;

        float since = _clock - _lastStrike;
        if (since < FlashHold || since >= FlashDip && since < FlashSecond)
        {
            Rectangle zone = painting.Lightning;
            var top = new Vector2(zone.Left + Hash(_strikes, 1, StrikeSeed) * zone.Width - BoltCell.X / 2f, zone.Top);
            var source = new Rectangle(_strikes % BoltFrames * BoltCell.X, 0, BoltCell.X, BoltCell.Y);
            DrawArt(spriteBatch, _bolts, source, top, depth, Color.White * alpha);
        }
        DrawFlash(spriteBatch, SkyFlash * (Flash() * SkyFlashStrength * alpha));
    }

    /// <summary>How bright the last strike's flash is right now, 0 to 1.</summary>
    private float Flash()
    {
        if (_strikes == 0) return 0f;

        float since = _clock - _lastStrike;
        if (since < FlashHold) return 1f;
        if (since < FlashDip) return DipStrength;
        if (since < FlashSecond) return SecondStrength;
        return SecondStrength * MathF.Max(0f, 1f - (since - FlashSecond) / FlashFade);
    }

    /// <summary>The whole screen washed with a colour, for the flash.</summary>
    private void DrawFlash(SpriteBatch spriteBatch, Color colour) =>
        spriteBatch.Draw(_weather, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight), FlashCell, colour);

    /// <summary>The rain: each drop falling down its own slanted line and coming round again at the top, a little brighter in a flash.</summary>
    private void DrawRain(SpriteBatch spriteBatch, Painting painting, bool lightning, float alpha)
    {
        float travel = CanvasHeight + WeatherCell.Y;
        float flash = lightning ? Flash() : 0f;
        for (int i = 0; i < painting.Rain; i++)
        {
            bool near = i % NearEvery == 0;
            float speed = (near ? NearSpeed : FarSpeed) * (1f - RainSpread / 2f + RainSpread * Hash(i, 0, RainSeed));
            float y = (Hash(i, 1, RainSeed) * travel + speed * _clock) % travel - WeatherCell.Y;
            float x = Hash(i, 2, RainSeed) * (CanvasWidth + RainSlant * travel) - RainSlant * y;
            var source = new Rectangle((near ? NearDrop : FarDrop) * WeatherCell.X, 0, WeatherCell.X, WeatherCell.Y);
            float strength = MathF.Min(1f, (near ? NearRain : FarRain) * (1f + flash));
            DrawArt(spriteBatch, _weather, source, new Vector2(x, y), RainDepth, Color.White * (strength * alpha));
        }
    }

    /// <summary>The glints on the water, each turning up somewhere new, growing to a star and going.</summary>
    private void DrawGlints(SpriteBatch spriteBatch, Painting painting, float depth, float alpha)
    {
        Rectangle water = painting.Water;
        for (int i = 0; i < painting.Glints; i++)
        {
            float cycle = _clock / GlintSeconds + Hash(i, 0, GlintSeed);
            int round = (int)cycle;
            float lit = (cycle - round) / GlintLit;
            if (lit >= 1f) continue;

            float strength = MathF.Sin(lit * MathF.PI);
            int frame = Math.Min(GlintFrames - 1, (int)(strength * GlintFrames));
            var at = new Vector2(water.Left + Hash(i, round, GlintSeed + 1) * water.Width - GlintCell / 2,
                water.Top + Hash(i, round, GlintSeed + 2) * water.Height - GlintCell / 2);
            DrawArt(spriteBatch, _glint, new Rectangle(frame * GlintCell, 0, GlintCell, GlintCell), at, depth, painting.GlintColour * (strength * alpha));
        }
    }

    /// <summary>The two of you, if you've been handed over, and the Soul-Bond's mark pulsing where your hands meet.</summary>
    private void DrawFigures(SpriteBatch spriteBatch, string id, Painting painting, float depth, float alpha)
    {
        if (!_figures.TryGetValue(id, out Texture2D[] figures)) return;

        for (int i = 0; i < figures.Length; i++)
            DrawArt(spriteBatch, figures[i], figures[i].Bounds, painting.Figures[i] - Feet, depth, Color.White * alpha);

        if (painting.Sigil is not Vector2 sigil) return;
        float pulse = 0.5f + 0.5f * MathF.Sin(_clock / SigilSeconds * MathHelper.TwoPi);
        Vector2 corner = sigil - new Vector2(SigilCell / 2f);
        DrawArt(spriteBatch, _sigil, new Rectangle(0, 0, SigilCell, SigilCell), corner, depth, Color.White * ((SigilLow + (1f - SigilLow) * pulse) * alpha));
        DrawArt(spriteBatch, _sigil, new Rectangle(SigilCell, 0, SigilCell, SigilCell), corner, depth, Color.White * ((MarkLow + (1f - MarkLow) * pulse) * alpha));
    }

    /// <summary>The lantern, swinging on its hook.</summary>
    private void DrawLantern(SpriteBatch spriteBatch, Painting painting, float depth, float alpha)
    {
        if (painting.Lantern is not Vector2 hook) return;

        float swing = MathF.Sin(_clock / LanternSeconds * MathHelper.TwoPi);
        int frame = (int)MathF.Round((swing + 1f) / 2f * (LanternFrames - 1));
        var source = new Rectangle(frame * LanternCell.X, 0, LanternCell.X, LanternCell.Y);
        DrawArt(spriteBatch, _lantern, source, hook - new Vector2(LanternCell.X / 2, 0f), depth, Color.White * alpha);
    }

    /// <summary>Draws part of a picture with its top left at a point on the canvas, on a layer that takes `depth` of the sway.</summary>
    private void DrawArt(SpriteBatch spriteBatch, Texture2D texture, Rectangle source, Vector2 canvas, float depth, Color colour) =>
        spriteBatch.Draw(texture, ToScreen(canvas, depth), source, colour, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

    /// <summary>Where a point on the canvas is on screen right now, for a layer that takes `depth` of the sway.</summary>
    /// <remarks>Whole art pixels, so nothing shimmers as it moves. The canvas's middle is the screen's middle.</remarks>
    private Vector2 ToScreen(Vector2 canvas, float depth)
    {
        float sway = MathF.Round(MathF.Sin(_clock / SwaySeconds * MathHelper.TwoPi) * SwayReach * depth);
        return new Vector2(
            LegendQuestGame.ScreenWidth / 2f + (MathF.Round(canvas.X) - CanvasWidth / 2f + sway) * Scale,
            LegendQuestGame.ScreenHeight / 2f + (MathF.Round(canvas.Y) - CanvasHeight / 2f) * Scale);
    }

    /// <summary>One of you as a silhouette: the frame standing facing away, filled with the painting's shadow (a hint of your own light and dark left in it), and every edge that faces the light behind you lit with its rim, the more squarely the brighter.</summary>
    private static Texture2D Silhouette(Texture2D sheet, Painting painting, Vector2 feet)
    {
        int width = StandingAway.Width, height = StandingAway.Height;
        var pixels = new Color[width * height];
        sheet.GetData(0, StandingAway, pixels, 0, pixels.Length);

        var shaded = new Color[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[y * width + x];
                if (pixel.A == 0) continue;

                float light = (pixel.R * 0.3f + pixel.G * 0.59f + pixel.B * 0.11f) / 255f;
                Color shade = Color.Lerp(painting.Shadow, painting.Rim, light * Detail);

                Vector2 toLight = painting.Backlight - (feet + new Vector2(x, y) - Feet);
                if (toLight != Vector2.Zero) toLight.Normalize();
                float facing = 0f;
                foreach (Point step in Around)
                {
                    int nx = x + step.X, ny = y + step.Y;
                    bool open = nx < 0 || ny < 0 || nx >= width || ny >= height || pixels[ny * width + nx].A == 0;
                    if (open) facing = MathF.Max(facing, Vector2.Dot(Vector2.Normalize(step.ToVector2()), toLight));
                }
                if (facing > RimFrom)
                    shade = Color.Lerp(shade, painting.Rim, RimLeast + (1f - RimLeast) * (facing - RimFrom) / (1f - RimFrom));
                shaded[y * width + x] = shade;
            }
        }

        var silhouette = new Texture2D(sheet.GraphicsDevice, width, height);
        silhouette.SetData(shaded);
        return silhouette;
    }

    /// <summary>A repeatable 0..1 number for three whole numbers. The same as hash01 in tools/pixels.py.</summary>
    private static float Hash(int a, int b, int seed)
    {
        unchecked
        {
            uint h = (uint)(a * 374761393 + b * 668265263 + seed * 2147483647);
            h = (h ^ (h >> 13)) * 1274126177u;
            return ((h ^ (h >> 16)) & 0xFFFF) / 65535f;
        }
    }
}
