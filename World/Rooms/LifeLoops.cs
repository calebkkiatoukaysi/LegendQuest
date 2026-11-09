using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Collisions;
using LegendQuest.Components;
using LegendQuest.Particles;

namespace LegendQuest.World;

/// <summary>
/// The little things moving in a room (12-rooms §3.2): clouds drifting across the sky, gulls,
/// chimney smoke, flags, glints on water, falling leaves, motes, ash, and candle flames with their
/// glow, all painted to match the rooms by tools/rooms/paint_life.py into Content/Rooms/Life.
/// </summary>
/// <remarks>
/// Each of the room's life lines is a box. Clouds drift in the sky layer at its parallax; gulls,
/// flags and candles are drawn into the mid layer's batch; the particles go behind the people
/// (smoke, glints) or in front of them (motes, leaves, ash). The particle systems were tuned for the
/// title, which is in screen pixels, so they run in screen pixels here too: the boxes are scaled up
/// by RoomScale for them, and their batch scaled back down; a room repaints them with its own
/// pictures (ParticleSystem.Repaint), painted PaintedParticles times the size of the title's. Every
/// painted picture here is drawn at the size its sheet says, one texel to a screen pixel.
/// </remarks>
internal sealed class LifeLoops
{
    /// <summary>Where the painted pictures are.</summary>
    private const string Life = "Rooms/Life/";

    /// <summary>A cloud in world pixels, and how many different ones Rooms/Life/clouds has side by side.</summary>
    private const int CloudWidth = 132, CloudCells = 6;

    /// <summary>A gull's picture across, in world pixels; Rooms/Life/gull is its two wingbeats side by side.</summary>
    private const float GullWidth = 16f;
    private const int GullFrames = 2;

    /// <summary>A flag's cloth across, in world pixels, Rooms/Life/flag's frames of it waving, how long each is up, and the pole's thickness.</summary>
    private const float FlagWidth = 16f, FlagFrameTime = 0.11f, PoleWidth = 1.5f;
    private const int FlagFrames = 8;

    /// <summary>Where the pole's top is in a flag's frame, in world pixels from its top left.</summary>
    private static readonly Vector2 FlagTop = new(1.5f, 2f);

    /// <summary>How many times bigger the painted particles are than the title's.</summary>
    private const float PaintedParticles = 4f;

    /// <summary>How many clouds drift in each box, and how fast, in world pixels a second.</summary>
    private const int CloudsPerBox = 2;
    private const float CloudSpeedMin = 1.5f, CloudSpeedMax = 4f;

    /// <summary>A gull to so many world pixels of its box, how fast they glide, how far and how often they bob, and how long a wingbeat is.</summary>
    private const float GullSpacing = 90f, GullSpeedMin = 9f, GullSpeedMax = 17f, GullBob = 3f, GullBobRate = 0.8f, WingBeat = 0.22f;

    /// <summary>A flag to so many world pixels of its box.</summary>
    private const float FlagSpacing = 80f;

    /// <summary>A candle to so many world pixels of its box, and how big its flame is.</summary>
    private const float CandleSpacing = 50f, CandleScale = 0.5f;

    /// <summary>How far a candle's glow reaches, in world pixels, and its colour.</summary>
    private const float CandleGlow = 14f;
    private static readonly Color CandleGlowColor = new Color(255, 180, 100) * 0.45f;

    /// <summary>How often each kind of particle is let go from its box, in seconds.</summary>
    private static readonly Dictionary<string, float> Every = new()
    {
        ["smoke"] = 0.45f, ["shimmer"] = 0.25f, ["leaves"] = 0.7f, ["motes"] = 0.6f, ["ash"] = 0.35f,
    };

    /// <summary>How many puffs of each kind can be in the air at once, per box.</summary>
    private const int PuffsInTheAir = 4;

    private static readonly Color SmokeColor = new Color(200, 196, 214) * 0.35f;
    private static readonly Color MoteColor = new Color(255, 222, 160) * 0.6f;
    private static readonly Color AshColor = new Color(176, 170, 186) * 0.5f;

    private readonly float _skyWidth;
    private readonly List<(int Cell, Vector2 At, float Speed)> _clouds = new();
    private readonly List<(Vector2 At, float Speed, float Phase, float Left, float Right)> _gulls = new();
    private readonly List<(Vector2 Pole, float Height, float Phase)> _flags = new();
    private readonly List<TorchSprite> _candles = new();
    private readonly List<(string Kind, Rectangle Area, float Clock)> _emitters = new();

    private readonly MoteParticleSystem _smoke, _motes, _ash;
    private readonly SparkleParticleSystem _shimmer;
    private readonly PetalParticleSystem _leaves;

    private Texture2D _cloudSheet, _gullSheet, _flagSheet, _pole;
    private float _clock;

    /// <summary>Sets up a room's life from its life lines: where each cloud, gull, flag and candle starts, seeded on where its box is so a room's always the same.</summary>
    /// <param name="room">The room.</param>
    public LifeLoops(RoomDefinition room)
    {
        float scale = ZoneCatalog.RoomScale, screen = LegendQuestGame.ScreenWidth;
        _skyWidth = room.Kind == RoomKind.Street ? (screen + 0.1f * (room.Width * scale - screen)) / scale : room.Width;

        var counts = new Dictionary<string, int>();
        foreach (RoomLife life in room.Life)
        {
            BoundingRectangle box = life.Area;
            var random = new Random((int)(box.X * 31f + box.Y * 17f + box.Width));
            switch (life.Kind)
            {
                case "clouds":
                    for (int i = 0; i < CloudsPerBox; i++)
                        _clouds.Add((random.Next(CloudCells), new Vector2(Next(random, 0f, _skyWidth), Next(random, box.Top, box.Bottom)),
                            Next(random, CloudSpeedMin, CloudSpeedMax)));
                    break;
                case "gulls":
                    for (int i = 0; i < Math.Max(2, (int)(box.Width / GullSpacing)); i++)
                        _gulls.Add((new Vector2(Next(random, box.Left, box.Right), Next(random, box.Top, box.Bottom)),
                            Next(random, GullSpeedMin, GullSpeedMax) * (random.Next(2) == 0 ? -1f : 1f), Next(random, 0f, MathHelper.TwoPi), box.Left, box.Right));
                    break;
                case "flag":
                    for (float x = box.Left + 2f; x < box.Right; x += FlagSpacing)
                        _flags.Add((new Vector2(x, box.Bottom), box.Height, Next(random, 0f, MathHelper.TwoPi)));
                    break;
                case "candle":
                    for (float x = box.Left + CandleSpacing / 2f; x < box.Right; x += CandleSpacing)
                        _candles.Add(new TorchSprite(new Vector2(x, box.Bottom), CandleScale, Life + "candle"));
                    break;
                default:
                    var area = new Rectangle((int)(box.X * scale), (int)(box.Y * scale), (int)(box.Width * scale), (int)(box.Height * scale));
                    _emitters.Add((life.Kind, area, Next(random, 0f, Every[life.Kind])));
                    counts[life.Kind] = counts.GetValueOrDefault(life.Kind) + 1;
                    break;
            }
        }

        int Of(string kind) => counts.GetValueOrDefault(kind) * PuffsInTheAir;
        if (Of("smoke") > 0) _smoke = new MoteParticleSystem(Of("smoke"), SmokeColor);
        if (Of("motes") > 0) _motes = new MoteParticleSystem(Of("motes"), MoteColor);
        if (Of("ash") > 0) _ash = new MoteParticleSystem(Of("ash"), AshColor);
        if (Of("shimmer") > 0) _shimmer = new SparkleParticleSystem(Of("shimmer") * 2);
        if (Of("leaves") > 0) _leaves = new PetalParticleSystem(Of("leaves") * 4);
        _smoke?.Repaint(Life + "smoke", PaintedParticles);
        _motes?.Repaint(Life + "mote", PaintedParticles);
        _ash?.Repaint(Life + "ash", PaintedParticles);
        _shimmer?.Repaint(Life + "sparkle", PaintedParticles);
        _leaves?.Repaint(Life + "leaf", PaintedParticles);
    }

    /// <summary>Loads the clouds, gulls, flags, the candles' flames and the particles' pictures.</summary>
    /// <param name="content">The room's content manager.</param>
    public void LoadContent(ContentManager content)
    {
        if (_clouds.Count > 0) _cloudSheet = content.Load<Texture2D>(Life + "clouds");
        if (_gulls.Count > 0) _gullSheet = content.Load<Texture2D>(Life + "gull");
        if (_flags.Count > 0)
        {
            _flagSheet = content.Load<Texture2D>(Life + "flag");
            _pole = content.Load<Texture2D>(Life + "flag-pole");
        }
        foreach (TorchSprite candle in _candles) candle.LoadContent(content);
        _smoke?.LoadContent(content);
        _motes?.LoadContent(content);
        _ash?.LoadContent(content);
        _shimmer?.LoadContent(content);
        _leaves?.LoadContent(content);
    }

    /// <summary>Moves everything on: the clouds drift, the gulls glide and wrap round their box, the candles flicker, and each box lets its particles go when it's due.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _clock += dt;

        for (int i = 0; i < _clouds.Count; i++)
        {
            var cloud = _clouds[i];
            cloud.At.X -= cloud.Speed * dt;
            if (cloud.At.X < -CloudWidth) cloud.At.X += _skyWidth + CloudWidth;
            _clouds[i] = cloud;
        }
        for (int i = 0; i < _gulls.Count; i++)
        {
            var gull = _gulls[i];
            gull.At.X += gull.Speed * dt;
            if (gull.At.X > gull.Right) gull.At.X = gull.Left;
            if (gull.At.X < gull.Left) gull.At.X = gull.Right;
            _gulls[i] = gull;
        }
        foreach (TorchSprite candle in _candles) candle.Update(gameTime);

        for (int i = 0; i < _emitters.Count; i++)
        {
            var emitter = _emitters[i];
            emitter.Clock -= dt;
            if (emitter.Clock <= 0f)
            {
                emitter.Clock += Every[emitter.Kind];
                Release(emitter.Kind, emitter.Area);
            }
            _emitters[i] = emitter;
        }
        _smoke?.Update(gameTime);
        _motes?.Update(gameTime);
        _ash?.Update(gameTime);
        _shimmer?.Update(gameTime);
        _leaves?.Update(gameTime);
    }

    /// <summary>Draws the drifting clouds into the sky's batch, against the sky layer's left edge.</summary>
    /// <param name="spriteBatch">The sky's batch, begun in world space.</param>
    /// <param name="skyLeft">Where the sky layer's left edge is in the world.</param>
    /// <param name="tint">The room's light colour.</param>
    public void DrawSky(SpriteBatch spriteBatch, float skyLeft, Color tint)
    {
        if (_cloudSheet is null) return;
        int width = _cloudSheet.Width / CloudCells;
        float scale = (float)CloudWidth / width;
        foreach ((int cell, Vector2 at, _) in _clouds)
            spriteBatch.Draw(_cloudSheet, new Vector2(skyLeft + at.X, at.Y), new Rectangle(cell * width, 0, width, _cloudSheet.Height), tint,
                0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    /// <summary>Draws the gulls, the flags and the candles' flames into the mid layer's batch.</summary>
    /// <param name="spriteBatch">The mid layer's batch, begun in world space.</param>
    /// <param name="tint">The room's light colour.</param>
    public void DrawMid(SpriteBatch spriteBatch, Color tint)
    {
        if (_gullSheet is not null)
        {
            int width = _gullSheet.Width / GullFrames;
            float scale = GullWidth / width;
            var origin = new Vector2(width / 2f, _gullSheet.Height / 2f);
            foreach ((Vector2 at, float speed, float phase, _, _) in _gulls)
            {
                int frame = (int)((_clock + phase) / WingBeat) % GullFrames;
                var middle = new Vector2(at.X, at.Y + MathF.Sin(_clock * GullBobRate * MathHelper.TwoPi + phase) * GullBob);
                spriteBatch.Draw(_gullSheet, middle, new Rectangle(frame * width, 0, width, _gullSheet.Height), tint, 0f, origin, scale,
                    speed < 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
        }

        if (_flagSheet is not null)
        {
            int width = _flagSheet.Width / FlagFrames;
            float scale = FlagWidth / width;
            foreach ((Vector2 pole, float height, float phase) in _flags)
            {
                var top = new Vector2(pole.X, pole.Y - height);
                spriteBatch.Draw(_pole, new Vector2(pole.X - PoleWidth / 2f, top.Y), null, tint, 0f, Vector2.Zero,
                    new Vector2(PoleWidth / _pole.Width, height / _pole.Height), SpriteEffects.None, 0f);
                int frame = (int)(_clock / FlagFrameTime + phase / MathHelper.TwoPi * FlagFrames) % FlagFrames;
                spriteBatch.Draw(_flagSheet, top, new Rectangle(frame * width, 0, width, _flagSheet.Height), tint, 0f, FlagTop / scale, scale,
                    SpriteEffects.None, 0f);
            }
        }

        foreach (TorchSprite candle in _candles) candle.Draw(spriteBatch, 0f);
    }

    /// <summary>Draws the particles that are behind the people: smoke and the glints on water.</summary>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="world">The camera's world to screen matrix.</param>
    public void DrawBehind(SpriteBatch spriteBatch, Matrix world)
    {
        Matrix screen = ScreenSpace(world);
        _smoke?.Draw(spriteBatch, screen);
        _shimmer?.Draw(spriteBatch, screen);
    }

    /// <summary>Draws the particles that drift in front of everyone: motes, leaves and ash.</summary>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="world">The camera's world to screen matrix.</param>
    public void DrawFront(SpriteBatch spriteBatch, Matrix world)
    {
        Matrix screen = ScreenSpace(world);
        _motes?.Draw(spriteBatch, screen);
        _leaves?.Draw(spriteBatch, screen);
        _ash?.Draw(spriteBatch, screen);
    }

    /// <summary>Draws the candles' glow, additive, over the dark.</summary>
    /// <param name="spriteBatch">An additive batch, begun in world space.</param>
    public void DrawLight(SpriteBatch spriteBatch)
    {
        foreach (TorchSprite candle in _candles) candle.DrawGlow(spriteBatch, CandleGlow, CandleGlowColor);
    }

    /// <summary>Lets a box's particles go: smoke from its bottom edge, leaves and ash from its top, the rest anywhere in it.</summary>
    private void Release(string kind, Rectangle area)
    {
        var top = new Rectangle(area.X, area.Y, area.Width, Math.Min(area.Height, 16));
        var bottom = new Rectangle(area.X, area.Bottom - Math.Min(area.Height, 16), area.Width, Math.Min(area.Height, 16));
        switch (kind)
        {
            case "smoke": _smoke.Release(bottom, falling: false); break;
            case "motes": _motes.Release(area, falling: false); break;
            case "ash": _ash.Release(top, falling: true); break;
            case "shimmer": _shimmer.Rise(area); break;
            case "leaves": _leaves.Fall(top); break;
        }
    }

    /// <summary>The batch matrix the particles draw with: screen pixels scaled back down into the world, then the camera.</summary>
    private static Matrix ScreenSpace(Matrix world) => Matrix.CreateScale(1f / ZoneCatalog.RoomScale) * world;

    private static float Next(Random random, float min, float max) => min + (float)random.NextDouble() * (max - min);
}
