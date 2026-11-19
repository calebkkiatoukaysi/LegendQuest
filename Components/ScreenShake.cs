using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Components;

/// <summary>How hard the screen shakes, as the options set it.</summary>
public enum ShakeStrength
{
    /// <summary>As it's tuned.</summary>
    Full,

    /// <summary>A third as much, and no kick.</summary>
    Reduced,

    /// <summary>Not at all.</summary>
    Off,
}

/// <summary>What shook the screen, for how much trauma it adds (ScreenShake.TraumaOf).</summary>
public enum ShakeCause
{
    /// <summary>An auto attack or any light blow: none, since they come every two seconds from everyone.</summary>
    Light,

    /// <summary>A heavy melee skill landing.</summary>
    Heavy,

    /// <summary>A spell or a shot hitting one target.</summary>
    Spell,

    /// <summary>A spell that catches several (a burst, a ground marker resolving).</summary>
    AreaSpell,

    /// <summary>A blow that kills.</summary>
    Kill,

    /// <summary>The player taking a hit.</summary>
    PlayerHit,

    /// <summary>The player taking a big hit: more than a fifth of what they can have.</summary>
    PlayerBigHit,

    /// <summary>A boss's slam or its big area attack.</summary>
    BossSlam,

    /// <summary>A body hitting the floor: a knockdown or a death landing.</summary>
    Land,

    /// <summary>One of the party going down.</summary>
    Downed,
}

/// <summary>
/// The jolt when a blow lands in a fight. Its Transform goes after the camera's in SpriteBatch.Begin,
/// so the whole map shakes and pushes in a little toward the hit, then settles.
/// </summary>
/// <remarks>
/// The Black Box's ScreenShake, smaller: a room in a fight is a busy picture with a lot of hits in it,
/// so a hit is a nudge and a critical a jolt, never enough to lose your place. A hit adds "trauma"
/// (0 to 1) and time takes it away; the shake is trauma squared, so it fades out instead of stopping
/// dead. The push in is there so the shake never shows past the edge of the map. How much each kind
/// of moment adds is TraumaOf (the feel spec): nothing for auto attacks, which come from everyone
/// every two seconds, a little for spells, more for heavy skills, crits and kills. A heavy blow also
/// kicks the picture a few pixels the way it was going (Kick with a direction), which springs back in
/// about a tenth of a second. Strength is the options' setting: Reduced is a third as much and no
/// kick, Off nothing at all.
/// </remarks>
public class ScreenShake
{
    /// <summary>The furthest the picture moves either way at full trauma, in screen pixels.</summary>
    /// <remarks>Less up and down than sideways, since the screen is wider than it is tall.</remarks>
    private const float MaxShiftX = 9f;
    private const float MaxShiftY = 5f;

    /// <summary>How much bigger everything gets at full trauma. 0.02 is 2%.</summary>
    private const float MaxZoom = 0.02f;

    /// <summary>How much trauma goes away a second: the worst hit is over in a little over half a second.</summary>
    private const float Recovery = 1.8f;

    /// <summary>How fast it wobbles, in radians a second. Each axis uses a different multiple so it never goes round in a circle.</summary>
    private const float Wobble = 47f;

    /// <summary>How much a reduced shake is of a full one.</summary>
    private const float ReducedShare = 1f / 3f;

    /// <summary>A kick: how many screen pixels it goes for each unit of trauma behind it, the most it goes, and how fast it springs back (a rate a second).</summary>
    private const float KickPixels = 12f;
    private const float KickMost = 5f;
    private const float KickReturn = 22f;

    /// <summary>How much more trauma a critical adds on top of the blow's own.</summary>
    public const float CriticalTrauma = 0.2f;

    private float _trauma;
    private float _clock;
    private Vector2 _focus;
    private Vector2 _kick;

    /// <summary>The matrix to multiply after the camera's. Identity when nothing is shaking.</summary>
    public Matrix Transform { get; private set; } = Matrix.Identity;

    /// <summary>How hard it shakes, as the options have it. Full to start with.</summary>
    public ShakeStrength Strength { get; set; } = ShakeStrength.Full;

    /// <summary>How much trauma a moment adds at full strength, 0 to 1, a critical adding CriticalTrauma more.</summary>
    /// <param name="cause">What happened.</param>
    /// <param name="critical">Whether it was a critical.</param>
    public static float TraumaOf(ShakeCause cause, bool critical = false)
    {
        float trauma = cause switch
        {
            ShakeCause.Light => 0f,
            ShakeCause.Heavy => 0.25f,
            ShakeCause.Spell => 0.15f,
            ShakeCause.AreaSpell => 0.3f,
            ShakeCause.Kill => 0.4f,
            ShakeCause.PlayerHit => 0.2f,
            ShakeCause.PlayerBigHit => 0.45f,
            ShakeCause.BossSlam => 0.5f,
            ShakeCause.Land => 0.12f,
            ShakeCause.Downed => 0.35f,
            _ => 0f,
        };
        return MathF.Min(1f, trauma + (critical ? CriticalTrauma : 0f));
    }

    /// <summary>Adds a hit's worth of shake, pushing in toward where it landed.</summary>
    /// <remarks>The focus is kept far enough in from the edges that the push in always covers the shift (MaxShift / MaxZoom from each side).</remarks>
    /// <param name="amount">How hard, from 0 to 1, at full strength. It adds up, up to 1.</param>
    /// <param name="focus">Where the hit was, in screen pixels.</param>
    public void Kick(float amount, Vector2 focus) => Kick(amount, focus, Vector2.Zero);

    /// <summary>Adds a hit's worth of shake, pushing in toward where it landed, and kicks the picture a few pixels the way the blow was going.</summary>
    /// <param name="amount">How hard, from 0 to 1, at full strength.</param>
    /// <param name="focus">Where the hit was, in screen pixels.</param>
    /// <param name="direction">Which way the blow was going (from the attacker to the target), any length; zero for no kick.</param>
    public void Kick(float amount, Vector2 focus, Vector2 direction)
    {
        float share = Strength switch
        {
            ShakeStrength.Off => 0f,
            ShakeStrength.Reduced => ReducedShare,
            _ => 1f,
        };
        if (share <= 0f || amount <= 0f) return;
        _trauma = MathHelper.Clamp(_trauma + amount * share, 0f, 1f);

        const float insetX = MaxShiftX / MaxZoom, insetY = MaxShiftY / MaxZoom;
        _focus = new Vector2(
            MathHelper.Clamp(focus.X, insetX, LegendQuestGame.ScreenWidth - insetX),
            MathHelper.Clamp(focus.Y, insetY, LegendQuestGame.ScreenHeight - insetY));

        if (Strength == ShakeStrength.Full && direction != Vector2.Zero)
        {
            _kick += Vector2.Normalize(direction) * amount * KickPixels;
            if (_kick.Length() > KickMost) _kick = Vector2.Normalize(_kick) * KickMost;
        }
    }

    /// <summary>Adds the shake for a kind of moment (TraumaOf), with its kick if it has a direction.</summary>
    /// <param name="cause">What happened.</param>
    /// <param name="critical">Whether it was a critical.</param>
    /// <param name="focus">Where, in screen pixels.</param>
    /// <param name="direction">Which way the blow went; zero for none.</param>
    public void Kick(ShakeCause cause, bool critical, Vector2 focus, Vector2 direction = default) =>
        Kick(TraumaOf(cause, critical), focus, direction);

    /// <summary>Stops it at once.</summary>
    public void Clear()
    {
        _trauma = 0f;
        _kick = Vector2.Zero;
        Transform = Matrix.Identity;
    }

    /// <summary>Lets the shake die down and works out this frame's matrix.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime) => Update((float)gameTime.ElapsedGameTime.TotalSeconds);

    /// <summary>Lets the shake die down and works out this frame's matrix.</summary>
    /// <param name="elapsed">Seconds since the last update.</param>
    public void Update(float elapsed)
    {
        _clock += elapsed;
        _trauma = MathF.Max(0f, _trauma - Recovery * elapsed);
        _kick *= MathF.Exp(-KickReturn * elapsed);
        if (_kick.LengthSquared() < 0.04f) _kick = Vector2.Zero;

        if (_trauma <= 0f && _kick == Vector2.Zero)
        {
            Transform = Matrix.Identity;
            return;
        }

        float shake = _trauma * _trauma;
        float x = MathF.Round(MaxShiftX * shake * MathF.Sin(_clock * Wobble) * MathF.Cos(_clock * Wobble * 0.31f) + _kick.X);
        float y = MathF.Round(MaxShiftY * shake * MathF.Sin(_clock * Wobble * 1.37f + 1.3f) + _kick.Y);
        float zoom = 1f + MaxZoom * shake;

        // Scale around the focus, then shift: the same as zooming a camera in on a point. The shift is
        // whole pixels so the tiles don't shimmer while it settles.
        Transform = Matrix.CreateTranslation(-_focus.X, -_focus.Y, 0f)
            * Matrix.CreateScale(zoom, zoom, 1f)
            * Matrix.CreateTranslation(_focus.X + x, _focus.Y + y, 0f);
    }
}
