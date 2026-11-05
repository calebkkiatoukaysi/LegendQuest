using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The Soul-Link's meter in a fight: the sigil of the two clasped hands on the bar beside the hotbar,
/// with a pip for each charge under it. Clicking it strikes.
/// </summary>
/// <remarks>
/// The story's five states of the sigil, read as the meter: grey with a gold pulse while the bond's
/// empty and waiting, gold once it's charged, gold with a glow behind it and READY when it's full,
/// the one-frame violet flicker as a charge lands, violet while one of the two of you is down, and
/// broken grey when the two of you aren't both in the fight or the bond's broken. Pointing at it says
/// how it fills, and clicking it picks the Soul-Link Strike when it's full. A
/// scene's sigil cues put it up outside a fight too (Mark and DrawMark), on the corner of the party's
/// cards, in the state the cue says: gold for on and gold, gold with its glow for glow, violet,
/// broken grey for grey and cold, nothing for off, and the two flickers for a flash.
/// </remarks>
public class SoulLinkSigil
{
    /// <summary>How much the sigil is blown up, and its pips.</summary>
    private const float Scale = 2f;

    /// <summary>How much a scene's sigil (DrawMark) is blown up, on the corner of the party's cards.</summary>
    private const float MarkScale = 3f;
    private const float PipScale = 1f;

    /// <summary>Where it is: on the bar between the budget and the hotbar, its middle (and its pips') level with the hotbar's, in screen pixels.</summary>
    private const int Left = 496;
    private const int PipRoom = 18;

    /// <summary>The room between two pips, in screen pixels.</summary>
    private const int PipGap = 6;

    /// <summary>How far under the sigil its pips go, in screen pixels.</summary>
    private const int PipDrop = 4;

    /// <summary>How bright the full sigil's glow is at the bottom of its breath, against its top.</summary>
    private const float GlowLeast = 0.6f;

    /// <summary>How long the violet flicker shows when a charge lands, in seconds.</summary>
    private const float FlickerSeconds = 0.09f;

    /// <summary>How long a sigil cue's flash of one frame lasts: a thirtieth of a second (03-art-overhaul.md §4.4).</summary>
    private const float OneFrameSeconds = 1f / 30f;

    /// <summary>How fast the empty sigil's gold pulse and the full one's glow breathe, in radians a second.</summary>
    private const float PulseRate = 3.2f;

    /// <summary>How far the full sigil's glow reaches past it, as a share of its size, and how bright it gets.</summary>
    private const float GlowReach = 0.55f;
    private const float GlowStrength = 0.55f;

    private const string Ready = "READY";

    private CombatArt _art;
    private SpriteFont _font;
    private Texture2D _glow;
    private float _clock;
    private float _flicker;

    /// <summary>The state a scene's sigil cue has put it in outside a fight, or null if none has; and a flash a cue's put over that, and how much longer it shows.</summary>
    private SigilState? _marked;
    private SigilState _flash;
    private float _flashLeft;

    /// <summary>How big a scene's sigil is on screen, either way, in screen pixels.</summary>
    public static int Size => (int)(CombatIcons.Piece("soul-link-gold").Width * MarkScale);

    /// <summary>How big the fight's sigil on the bar is, either way, in screen pixels.</summary>
    private static int BarSize => (int)(CombatIcons.Piece("soul-link-gold").Width * Scale);

    /// <summary>Where the sigil is on screen.</summary>
    public static Rectangle Bounds => new(Left, CombatHotbar.Top + (CombatHotbar.SlotSize - BarSize - PipRoom) / 2, BarSize, BarSize);

    /// <summary>Whether a scene's put it up outside a fight, in a state that shows or a flash.</summary>
    public bool Marked => _flashLeft > 0f || _marked is SigilState state && state != SigilState.Off;

    /// <summary>Loads the font and the glow, and keeps the shared art.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="art">The fight's sheets.</param>
    public void LoadContent(ContentManager content, CombatArt art)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _glow = content.Load<Texture2D>("glow");
        _art = art;
    }

    /// <summary>Flickers violet for a frame or two: a charge has landed.</summary>
    public void Flicker() => _flicker = FlickerSeconds;

    /// <summary>Puts it in a state a scene's sigil cue says, or flashes it into one: the two flickers are always a flash, and any state is for one frame if the cue says so.</summary>
    /// <param name="state">The state.</param>
    /// <param name="oneFrame">Whether it's only for a frame, after which it's back to how it was.</param>
    public void Mark(SigilState state, bool oneFrame)
    {
        if (!oneFrame && state is not (SigilState.FlickerGold or SigilState.FlickerViolet))
        {
            _marked = state;
            return;
        }
        _flash = state;
        _flashLeft = oneFrame ? OneFrameSeconds : FlickerSeconds;
    }

    /// <summary>Takes down whatever a scene put it up as.</summary>
    public void Unmark()
    {
        _marked = null;
        _flashLeft = 0f;
    }

    /// <summary>Runs the pulse, the flicker and a scene's flash.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _clock += seconds;
        _flicker = MathF.Max(0f, _flicker - seconds);
        _flashLeft = MathF.Max(0f, _flashLeft - seconds);
    }

    /// <summary>Draws it in the state a scene's put it in, with no pips, at a point. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="at">Its top left, in screen pixels.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawMark(SpriteBatch spriteBatch, Point at, float alpha)
    {
        SigilState? state = _flashLeft > 0f ? _flash : _marked;
        string piece = state switch
        {
            SigilState.On or SigilState.Gold or SigilState.Glow or SigilState.FlickerGold => "soul-link-gold",
            SigilState.Violet => "soul-link-violet",
            SigilState.Grey or SigilState.Cold => "soul-link-broken",
            SigilState.FlickerViolet => "soul-link-flicker",
            _ => null,
        };
        if (piece is null) return;

        var bounds = new Rectangle(at.X, at.Y, Size, Size);
        if (state == SigilState.Glow)
        {
            float breathe = 0.5f + 0.5f * MathF.Sin(_clock * PulseRate);
            Rectangle glow = bounds;
            glow.Inflate((int)(bounds.Width * GlowReach), (int)(bounds.Height * GlowReach));
            spriteBatch.Draw(_glow, glow, Palette.Gold * (GlowStrength * MathHelper.Lerp(GlowLeast, 1f, breathe) * alpha));
        }
        _art.Piece(spriteBatch, piece, bounds.Location.ToVector2(), MarkScale, Color.White * alpha);
    }

    /// <summary>Draws the sigil in its state and its pips. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="active">Whether the two of you are both in the fight.</param>
    /// <param name="strained">Whether one of you is down.</param>
    /// <param name="charge">Its charge as shown.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawArt(SpriteBatch spriteBatch, bool active, bool strained, int charge, float alpha)
    {
        Rectangle bounds = Bounds;
        float breathe = 0.5f + 0.5f * MathF.Sin(_clock * PulseRate);
        bool full = charge >= SoulLink.Full;

        if (active && full && !strained)
        {
            Rectangle glow = bounds;
            glow.Inflate((int)(bounds.Width * GlowReach), (int)(bounds.Height * GlowReach));
            spriteBatch.Draw(_glow, glow, Palette.Gold * (GlowStrength * MathHelper.Lerp(GlowLeast, 1f, breathe) * alpha));
        }

        string state = !active ? "soul-link-broken"
            : _flicker > 0f ? "soul-link-flicker"
            : strained ? "soul-link-violet"
            : charge == 0 ? "soul-link-pulse"
            : "soul-link-gold";
        _art.Piece(spriteBatch, state, bounds.Location.ToVector2(), Scale, Color.White * alpha);

        if (!active) return;
        Rectangle pip = CombatIcons.Piece("action");
        float width = SoulLink.Full * pip.Width * PipScale + (SoulLink.Full - 1) * PipGap;
        float x = bounds.Center.X - width / 2f;
        for (int i = 0; i < SoulLink.Full; i++)
            _art.Piece(spriteBatch, i < charge ? "action" : "action-spent", new Vector2(MathF.Round(x + i * (pip.Width * PipScale + PipGap)), bounds.Bottom + PipDrop), PipScale, Color.White * alpha);
    }

    /// <summary>Writes READY over a full sigil, out over the floor above the bar. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="active">Whether the two of you are both in the fight.</param>
    /// <param name="charge">Its charge as shown.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, bool active, int charge, float alpha)
    {
        Rectangle bounds = Bounds;
        if (!active || charge < SoulLink.Full) return;
        // Only when it's full, so the floor above the bar is left alone the rest of the time.
        Text.Draw(spriteBatch, _font, Ready, bounds.Center.X, CombatHotbar.BarTop - _font.LineSpacing, Palette.Gold * alpha, Palette.Shadow * alpha, centred: true);
    }
}
