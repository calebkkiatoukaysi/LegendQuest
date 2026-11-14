using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The dice for a check, in 3D on the panel: floating over it and slowly turning till they're
/// thrown, then up, tumbling, down, bouncing and rolling to a stop with the number the rules rolled
/// on top.
/// </summary>
/// <remarks>
/// The camera's straight over the panel, far enough up that the perspective's gentle, and on the
/// panel (height 0) one unit is one canvas pixel, so a die is put where it goes on the canvas. A die
/// coming up towards the camera gets bigger, and its shadow (the glow, darkened) slides away down
/// and right from the lamp at the top left, spreading and fading.
/// The dice are drawn on a target of their own at twice the size they're shown, with a depth buffer,
/// then shrunk onto the canvas smoothed, so their edges come out clean, with an ink outline round
/// them the way the art is outlined.
/// The landing is worked out backwards. Where a die ends is the turn that puts the rolled number on
/// top (DieModel.FaceUp), and the tumble is a spin round a level axis that slows to nothing there,
/// so it always lands on the number the rules already rolled. The turn it was floating at when it
/// was thrown fades into the tumble over the first part of the throw, so there's no jump.
/// Nothing in a throw is random: how it tumbles comes from which die it is and what it rolled.
/// </remarks>
public sealed class DiceTable : IDisposable
{
    /// <summary>One die on the table: where it comes to rest on the canvas, the number it shows, and whether it's the one that counts.</summary>
    public readonly record struct Die(Vector2 Home, int Value, bool Kept);

    /// <summary>How big a die is: from its middle to a corner, in canvas pixels.</summary>
    public const float Radius = 76f;

    /// <summary>How many times bigger than they're shown the dice are drawn.</summary>
    private const int Supersample = 2;

    /// <summary>How high over the panel the camera is, and how near and far it sees, in canvas pixels.</summary>
    private const float CameraHeight = 900f;
    private const float NearPlane = 100f;
    private const float FarPlane = 2000f;

    /// <summary>Before the throw: how high a die floats, how far and how fast it bobs, and how it turns (round what, how fast, and leaning back how far).</summary>
    private const float Hover = 14f;
    private const float Bob = 4f;
    private const float BobSpeed = 2.2f;
    private static readonly Vector3 IdleAxis = Vector3.Normalize(new Vector3(0.35f, 1f, 0.25f));
    private const float IdleTurn = 0.7f;
    private const float IdleLean = 0.4f;

    /// <summary>The throw's hops, in seconds: up out of the hand and down, then smaller and smaller bounces. And how hard it falls, in canvas pixels a second a second.</summary>
    private static readonly float[] Hops = { 0.26f, 0.15f, 0.11f, 0.08f, 0.06f, 0.045f };
    private const float Gravity = 9000f;

    /// <summary>How many times a die turns over in a throw, how quickly its spin dies away, how far it wanders off its place and back, and how much of the throw the turn it was floating at takes to fade.</summary>
    private const float Turns = 1.5f;
    private const float SpinFade = 2f;
    private const float Wander = 36f;
    private const float Settle = 0.6f;

    /// <summary>How far round from straight a die lands, either way, in radians, so a pair don't look stamped.</summary>
    private const float LandingTwist = 0.1f;

    /// <summary>After landing: how solid the die that doesn't count is, and the flash of the pass or fail colour on the one that does, how bright and for how long.</summary>
    private const float DroppedAlpha = 0.4f;
    private const float FlashStrength = 0.3f;
    private const float FlashSeconds = 0.5f;

    /// <summary>The shadow: its size under a die at rest, as a share of the die's size across and down; how dark it is; where the lamp puts it from the die; and per pixel the die's up, how much further it goes, how much it spreads and how it fades.</summary>
    private static readonly Vector2 ShadowSize = new(2.2f, 1.8f);
    private const float ShadowDarkness = 0.6f;
    private static readonly Vector2 ShadowOffset = new(6f, 10f);
    private static readonly Vector2 ShadowLean = new(0.35f, 0.5f);
    private const float ShadowSpread = 1f / 250f;
    private const float ShadowFade = 1f / 90f;

    /// <summary>How far the outline goes round the dice, in canvas pixels.</summary>
    private const float Outline = 1.5f;

    private static readonly Vector2[] OutlineSteps =
    {
        new(-1f, 0f), new(1f, 0f), new(0f, -1f), new(0f, 1f),
        new(-0.7f, -0.7f), new(0.7f, -0.7f), new(-0.7f, 0.7f), new(0.7f, 0.7f),
    };

    private readonly Rectangle _area;
    private readonly float _seconds;
    private readonly Die[] _dice;
    private readonly Vector3 _flash;
    private readonly DieModel _model = new();
    private readonly Matrix _view;
    private readonly Matrix _projection;

    private readonly Vector3[] _axes;
    private readonly float[] _spins;
    private readonly Vector2[] _wander;
    private readonly float[] _rhythm;
    private readonly Matrix[] _landing;
    private readonly Quaternion[] _unwind;
    private readonly float[] _startLift;

    private RenderTarget2D _target;
    private Texture2D _glow;
    private float _clock;
    private float _thrown = -1f;

    /// <summary>Sets the dice out.</summary>
    /// <param name="area">The part of the canvas the dice can be anywhere in, throw and all.</param>
    /// <param name="seconds">How long a throw takes from the press to the dice at rest.</param>
    /// <param name="dice">The dice: one, or two with advantage.</param>
    /// <param name="flash">The colour the die that counts flashes as it lands: the pass or fail colour.</param>
    public DiceTable(Rectangle area, float seconds, IReadOnlyList<Die> dice, Color flash)
    {
        _area = area;
        _seconds = seconds;
        _dice = new Die[dice.Count];
        for (int i = 0; i < dice.Count; i++) _dice[i] = dice[i];
        _flash = flash.ToVector3() * FlashStrength;

        _view = Matrix.CreateLookAt(new Vector3(0f, 0f, CameraHeight), Vector3.Zero, Vector3.Up);
        float near = NearPlane / CameraHeight;
        _projection = Matrix.CreatePerspectiveOffCenter(-area.Width / 2f * near, area.Width / 2f * near,
            -area.Height / 2f * near, area.Height / 2f * near, NearPlane, FarPlane);

        int count = _dice.Length;
        _axes = new Vector3[count];
        _spins = new float[count];
        _wander = new Vector2[count];
        _rhythm = new float[count];
        _landing = new Matrix[count];
        _unwind = new Quaternion[count];
        _startLift = new float[count];
        for (int i = 0; i < count; i++)
        {
            // A level axis (tipped a little), a different way for each die and each number.
            float way = 0.9f + i * 2.4f + _dice[i].Value * 0.37f;
            _axes[i] = Vector3.Normalize(new Vector3(MathF.Cos(way), MathF.Sin(way), 0.25f));
            _spins[i] = (Turns + i * 0.15f) * MathHelper.TwoPi;

            // Rolling over that axis carries it this way across the panel (up the canvas is down in y).
            Vector3 rolls = Vector3.Cross(Vector3.UnitZ, _axes[i]);
            _wander[i] = Vector2.Normalize(new Vector2(rolls.X, -rolls.Y)) * Wander;
            _rhythm[i] = 1f - i * 0.1f;
        }
    }

    /// <summary>Whether the throw's over and the dice are at rest.</summary>
    public bool Landed => _thrown >= _seconds;

    /// <summary>How long since the dice were thrown, in seconds, or less than 0 if they haven't been.</summary>
    public float Thrown => _thrown;

    /// <summary>Builds the die, and makes the target the dice are drawn on.</summary>
    /// <param name="content">Where the glow for the shadows and the numbers' font come from.</param>
    /// <param name="device">The graphics device.</param>
    public void LoadContent(ContentManager content, GraphicsDevice device)
    {
        _glow = content.Load<Texture2D>("glow");
        _model.Load(device, content.Load<SpriteFont>("spectral-number"));
        _target = new RenderTarget2D(device, _area.Width * Supersample, _area.Height * Supersample, false,
            SurfaceFormat.Color, DepthFormat.Depth24);

        for (int i = 0; i < _dice.Length; i++)
            _landing[i] = _model.FaceUp(_dice[i].Value) * Matrix.CreateRotationZ(i % 2 == 0 ? -LandingTwist : LandingTwist);
    }

    /// <summary>Throws them. Does nothing once they've been thrown.</summary>
    public void Throw()
    {
        if (_thrown >= 0f) return;

        for (int i = 0; i < _dice.Length; i++)
        {
            // What has to be undone first so the tumble starts at the turn it was floating at.
            Matrix tumble = _landing[i] * Matrix.CreateFromAxisAngle(_axes[i], _spins[i]);
            _unwind[i] = Quaternion.CreateFromRotationMatrix(Floating(i) * Matrix.Transpose(tumble));
            _startLift[i] = FloatingLift(i);
        }
        _thrown = 0f;
    }

    /// <summary>Moves the clocks on.</summary>
    /// <param name="seconds">How long the frame was.</param>
    public void Update(float seconds)
    {
        _clock += seconds;
        if (_thrown >= 0f) _thrown += seconds;
    }

    /// <summary>Draws the dice in 3D on their own target. Has to be done outside a sprite batch, before the canvas is drawn on; it puts the canvas back after.</summary>
    /// <param name="device">The graphics device.</param>
    public void Render(GraphicsDevice device)
    {
        RenderTargetBinding[] canvas = device.GetRenderTargets();
        device.SetRenderTarget(_target);
        device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1f, 0);
        device.BlendState = BlendState.AlphaBlend;
        device.DepthStencilState = DepthStencilState.Default;
        device.RasterizerState = RasterizerState.CullCounterClockwise;
        device.SamplerStates[0] = SamplerState.LinearClamp;

        for (int i = 0; i < _dice.Length; i++)
        {
            (Vector2 at, float lift, Matrix turn) = Pose(i);
            var middle = new Vector3(at.X - _area.Center.X, _area.Center.Y - at.Y, DieModel.FaceDistance * Radius + lift);
            Matrix world = Matrix.CreateScale(Radius) * turn * Matrix.CreateTranslation(middle);

            bool dropped = Landed && !_dice[i].Kept;
            Vector3 glow = Landed && _dice[i].Kept ? _flash * MathF.Max(0f, 1f - (_thrown - _seconds) / FlashSeconds) : Vector3.Zero;
            _model.Draw(world, _view, _projection, dropped ? DroppedAlpha : 1f, glow);
        }

        device.SetRenderTargets(canvas);
    }

    /// <summary>Puts the dice on the canvas: their shadows, the outline, then the dice. Goes in a smoothed (linear) sprite batch.</summary>
    /// <param name="spriteBatch">The batch, begun.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void Draw(SpriteBatch spriteBatch, float alpha)
    {
        var glowMiddle = new Vector2(_glow.Width / 2f, _glow.Height / 2f);
        for (int i = 0; i < _dice.Length; i++)
        {
            (Vector2 at, float lift, _) = Pose(i);
            float dark = ShadowDarkness / (1f + lift * ShadowFade) * (Landed && !_dice[i].Kept ? DroppedAlpha : 1f);
            Vector2 size = ShadowSize * Radius * (1f + lift * ShadowSpread);
            spriteBatch.Draw(_glow, at + ShadowOffset + ShadowLean * lift, null, Color.Black * (dark * alpha), 0f, glowMiddle,
                size / new Vector2(_glow.Width, _glow.Height), SpriteEffects.None, 0f);
        }

        var corner = new Vector2(_area.X, _area.Y);
        const float shrink = 1f / Supersample;
        foreach (Vector2 step in OutlineSteps)
            spriteBatch.Draw(_target, corner + step * Outline, null, Palette.Ink * alpha, 0f, Vector2.Zero, shrink, SpriteEffects.None, 0f);
        spriteBatch.Draw(_target, corner, null, Color.White * alpha, 0f, Vector2.Zero, shrink, SpriteEffects.None, 0f);
    }

    /// <summary>Lets go of the die and the target.</summary>
    public void Dispose()
    {
        _model.Dispose();
        _target?.Dispose();
    }

    /// <summary>Where a die is on the canvas, how far it's up off the panel from resting, and which way round it is.</summary>
    private (Vector2 At, float Lift, Matrix Turn) Pose(int i)
    {
        if (_thrown < 0f) return (_dice[i].Home, FloatingLift(i), Floating(i));

        float u = MathHelper.Clamp(_thrown / _seconds, 0f, 1f);
        Matrix spin = Matrix.CreateFromAxisAngle(_axes[i], _spins[i] * MathF.Pow(1f - u, SpinFade));
        float settled = MathHelper.SmoothStep(0f, 1f, MathF.Min(1f, u / Settle));
        Matrix unwind = Matrix.CreateFromQuaternion(Quaternion.Slerp(_unwind[i], Quaternion.Identity, settled));

        Vector2 at = _dice[i].Home + _wander[i] * (MathF.Sin(MathF.PI * u) * (1f - u));
        return (at, Hop(_thrown / _rhythm[i], _startLift[i]), unwind * _landing[i] * spin);
    }

    /// <summary>How high a die is off resting partway through its hops: up from where it floated, then down, and smaller hops after.</summary>
    /// <param name="time">Seconds into the throw.</param>
    /// <param name="start">How high it was floating when it was thrown.</param>
    private static float Hop(float time, float start)
    {
        foreach (float hop in Hops)
        {
            if (time < hop)
            {
                float up = (Gravity * hop * hop / 2f - start) / hop;
                return MathF.Max(0f, start + up * time - Gravity * time * time / 2f);
            }
            time -= hop;
            start = 0f;
        }
        return 0f;
    }

    /// <summary>How a die's turned while it floats waiting: the 20 up, leaning back, turning slowly over.</summary>
    private Matrix Floating(int i) =>
        _model.FaceUp(20) * Matrix.CreateRotationX(-IdleLean) * Matrix.CreateFromAxisAngle(IdleAxis, IdleTurn * _clock + i * 1.7f);

    /// <summary>How high a die floats while it waits, bobbing.</summary>
    private float FloatingLift(int i) => Hover + Bob * MathF.Sin(BobSpeed * _clock + i * 1.3f);
}
