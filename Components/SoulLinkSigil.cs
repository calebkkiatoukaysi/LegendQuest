using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The Soul-Link's sigil of the two clasped hands as a scene puts it up outside a fight, on the corner
/// of the party's cards. In a fight the sigil is the hotbar's slot 7 (Components/Realtime/Hotbar).
/// </summary>
/// <remarks>
/// A scene's sigil cues put it up (Mark and DrawMark) in the state the cue says: gold for on and gold,
/// gold with its glow for glow, violet, broken grey for grey and cold, nothing for off, and the two
/// flickers for a flash.
/// </remarks>
public class SoulLinkSigil
{
    /// <summary>How much a scene's sigil is blown up, on the corner of the party's cards.</summary>
    private const float MarkScale = 3f;

    /// <summary>How bright the glowing sigil's glow is at the bottom of its breath, against its top.</summary>
    private const float GlowLeast = 0.6f;

    /// <summary>How long a flicker shows, in seconds.</summary>
    private const float FlickerSeconds = 0.09f;

    /// <summary>How long a sigil cue's flash of one frame lasts: a thirtieth of a second (03-art-overhaul.md §4.4).</summary>
    private const float OneFrameSeconds = 1f / 30f;

    /// <summary>How fast the glow breathes, in radians a second.</summary>
    private const float PulseRate = 3.2f;

    /// <summary>How far the glow reaches past the sigil, as a share of its size, and how bright it gets.</summary>
    private const float GlowReach = 0.55f;
    private const float GlowStrength = 0.55f;

    private CombatArt _art;
    private Texture2D _glow;
    private float _clock;

    /// <summary>The state a scene's sigil cue has put it in, or null if none has; and a flash a cue's put over that, and how much longer it shows.</summary>
    private SigilState? _marked;
    private SigilState _flash;
    private float _flashLeft;

    /// <summary>How big a scene's sigil is on screen, either way, in screen pixels.</summary>
    public static int Size => (int)(CombatIcons.Piece("soul-link-gold").Width * MarkScale);

    /// <summary>Whether a scene's put it up, in a state that shows or a flash.</summary>
    public bool Marked => _flashLeft > 0f || _marked is SigilState state && state != SigilState.Off;

    /// <summary>Loads the glow, and keeps the shared art.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="art">The fight's sheets.</param>
    public void LoadContent(ContentManager content, CombatArt art)
    {
        _glow = content.Load<Texture2D>("glow");
        _art = art;
    }

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

    /// <summary>Runs the glow's breath and a scene's flash.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _clock += seconds;
        _flashLeft = MathF.Max(0f, _flashLeft - seconds);
    }

    /// <summary>Draws it in the state a scene's put it in, at a point. Goes in a point sampled batch.</summary>
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
}
