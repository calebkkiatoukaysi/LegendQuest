using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// The Soul-Link's meter in a fight: the sigil of the two clasped hands beside the hotbar, with a pip
/// for each charge under it.
/// </summary>
/// <remarks>
/// STORY.md's five states of the sigil, read as the meter: grey with a gold pulse while the bond's
/// empty and waiting, gold once it's charged, gold with a glow behind it and READY when it's full,
/// the one-frame violet flicker as a charge lands, violet while one of the two of you is down, and
/// broken grey when the two of you aren't both in the fight. Pointing at it says how it fills.
/// </remarks>
public class SoulLinkSigil
{
    /// <summary>How much the sigil is blown up, and its pips.</summary>
    private const float Scale = 3f;
    private const float PipScale = 1f;

    /// <summary>Where it is: right of the frame of whoever's turn it is, its bottom level with the hotbar's, in screen pixels.</summary>
    private const int Left = 458;
    private const int PipRoom = 18;

    /// <summary>The room between two pips, in screen pixels.</summary>
    private const int PipGap = 6;

    /// <summary>How far under the sigil its pips go, in screen pixels.</summary>
    private const int PipDrop = 4;

    /// <summary>How bright the full sigil's glow is at the bottom of its breath, against its top.</summary>
    private const float GlowLeast = 0.6f;

    /// <summary>How long the violet flicker shows when a charge lands, in seconds.</summary>
    private const float FlickerSeconds = 0.09f;

    /// <summary>How fast the empty sigil's gold pulse and the full one's glow breathe, in radians a second.</summary>
    private const float PulseRate = 3.2f;

    /// <summary>How far the full sigil's glow reaches past it, as a share of its size, and how bright it gets.</summary>
    private const float GlowReach = 0.55f;
    private const float GlowStrength = 0.55f;

    private const string Label = "SOUL-LINK";
    private const string Ready = "READY";

    private CombatArt _art;
    private SpriteFont _font;
    private Texture2D _glow;
    private float _clock;
    private float _flicker;

    /// <summary>Where the sigil is on screen.</summary>
    public static Rectangle Bounds
    {
        get
        {
            int size = (int)(CombatIcons.Piece("soul-link-gold").Width * Scale);
            return new Rectangle(Left, CombatHotbar.Top + CombatHotbar.SlotSize - size - PipRoom, size, size);
        }
    }

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

    /// <summary>Runs the pulse and the flicker.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _clock += seconds;
        _flicker = MathF.Max(0f, _flicker - seconds);
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

    /// <summary>Writes READY over a full sigil. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="active">Whether the two of you are both in the fight.</param>
    /// <param name="charge">Its charge as shown.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, bool active, int charge, float alpha)
    {
        Rectangle bounds = Bounds;
        bool ready = active && charge >= SoulLink.Full;
        // From the sigil's left edge rather than its middle, so the word runs out over the empty
        // room above the hotbar and not back over the frame of whoever's turn it is.
        Text.Draw(spriteBatch, _font, ready ? Ready : Label, bounds.Left, bounds.Top - _font.LineSpacing,
            (ready ? Palette.Gold : Palette.Note) * alpha, Palette.Shadow * alpha);
    }
}
