using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// What the player's frame and the target's share: a navy panel with the gold rim, a face in a
/// portrait frame at one end, the name and what they are, the health bar with its lag and its
/// numbers, and the aura row under the panel.
/// </summary>
/// <remarks>
/// The player's (PlayerFrame) has the face on the left and the Focus bar under the health; the
/// target's (TargetFrame) has it on the right, the armour class by the name and the cast bar under
/// the health. Downed, the health bar greys and says DOWN, and the three death saves come up in
/// place of the bar under it. Temporary hit points sit on the end of the health as a pale shield,
/// and their number after the health's.
/// </remarks>
public abstract class FighterFrame
{
    /// <summary>How tall the panel is, how far in its insides are, and the room between its parts, in screen pixels.</summary>
    public const int Height = 100;
    protected const int Inset = 11;
    protected const int Spacing = 4;

    /// <summary>How much the portrait frame is blown up.</summary>
    private const float PortraitScale = 2f;

    /// <summary>How far down the panel the name and the line under it start, in screen pixels.</summary>
    private const int NameDrop = 2;
    private const int LineDrop = 34;

    /// <summary>The health bar's height and the bar under it, in screen pixels.</summary>
    protected const int HealthHeight = 18;
    protected const int UnderHeight = 12;

    /// <summary>How far under the panel the aura row starts, and how many auras to a row.</summary>
    private const int AuraDrop = 6;
    protected const int AurasPerRow = 10;

    /// <summary>What a downed bar says.</summary>
    private const string Down = "DOWN";

    /// <summary>The death saves' sockets: how big, and the room between.</summary>
    private const int SaveGap = 3;

    /// <summary>A downed fighter's health bar: grey, full, with DOWN on it.</summary>
    private static readonly Color DownBar = new(64, 58, 66);

    private readonly HealthLag _lag = new();

    /// <summary>Where the panel is on screen.</summary>
    public abstract Rectangle Bounds { get; }

    /// <summary>Whether the face is at the right end rather than the left.</summary>
    protected abstract bool FaceRight { get; }

    /// <summary>The portrait frame's piece on hud.png.</summary>
    protected abstract string PortraitPiece(FighterHudView fighter);

    /// <summary>Where the portrait frame is.</summary>
    protected Rectangle PortraitFrame
    {
        get
        {
            Rectangle piece = CombatIcons.Piece("portrait-current");
            int w = (int)(piece.Width * PortraitScale), h = (int)(piece.Height * PortraitScale);
            Rectangle b = Bounds;
            return new Rectangle(FaceRight ? b.Right - Inset - w : b.X + Inset, b.Y + (b.Height - h) / 2, w, h);
        }
    }

    /// <summary>Where the words and bars go: the panel past the portrait.</summary>
    protected Rectangle Body
    {
        get
        {
            Rectangle b = Bounds, p = PortraitFrame;
            int left = FaceRight ? b.X + Inset : p.Right + Inset;
            int right = FaceRight ? p.X - Inset : b.Right - Inset;
            return new Rectangle(left, b.Y + NameDrop, right - left, b.Height - NameDrop - Inset);
        }
    }

    /// <summary>Where the health bar is: over the bar under it, at the panel's foot.</summary>
    /// <param name="kit">The HUD's shared art.</param>
    protected Rectangle HealthBar(HudKit kit)
    {
        Rectangle body = Body;
        return new Rectangle(body.X, Bounds.Bottom - Inset - UnderHeight - Spacing - HealthHeight, body.Width, HealthHeight);
    }

    /// <summary>The top of the line under the name.</summary>
    protected int LineTop => Bounds.Y + LineDrop;

    /// <summary>Where the bar under the health is: the Focus, the cast, or the death saves.</summary>
    /// <param name="kit">The HUD's shared art.</param>
    protected Rectangle UnderBar(HudKit kit)
    {
        Rectangle health = HealthBar(kit);
        return new Rectangle(health.X, health.Bottom + Spacing, health.Width, UnderHeight);
    }

    /// <summary>Where the aura row starts: under the panel, at the face's end.</summary>
    public Point AurasAt => new(FaceRight ? Bounds.Right : Bounds.X, Bounds.Bottom + AuraDrop);

    /// <summary>The bottom of the aura row under the frame, timers included: where whatever goes under it can start.</summary>
    /// <param name="fighter">Whose frame.</param>
    public int AurasBottom(FighterHudView fighter) => AurasAt.Y + AuraRow.Height(fighter?.Auras?.Count ?? 0, AurasPerRow, true);

    /// <summary>Which of someone's auras the mouse is on, or null.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <param name="fighter">Whose frame.</param>
    public AuraView AuraAt(Point mouse, FighterHudView fighter)
    {
        if (fighter?.Auras is null) return null;
        int i = AuraRow.At(mouse, fighter.Auras.Count, AurasAt, AurasPerRow, true, FaceRight);
        return i >= 0 ? fighter.Auras[i] : null;
    }

    /// <summary>Forgets the health lags, for a new fight.</summary>
    public void Reset() => _lag.Clear();

    /// <summary>Sets someone's health lag outright, for a picture of a hit just taken.</summary>
    /// <param name="id">Who.</param>
    /// <param name="was">Their health before the hit, 0 to 1.</param>
    /// <param name="share">Their health now.</param>
    public void ShowHit(string id, float was, float share) => _lag.Set(id, was, share);

    /// <summary>Draws the panel, the face, the bars and the aura row. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="fighter">Whose frame, or null for none.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawArt(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        if (fighter is null) return;
        CombatStyle.DrawPanel(spriteBatch, kit.Pixel, Bounds, alpha);

        Rectangle frame = PortraitFrame;
        kit.Art.Piece(spriteBatch, PortraitPiece(fighter), frame.Location.ToVector2(), PortraitScale, Color.White * alpha);
        int inset = (int)(CombatIcons.FrameInset * PortraitScale);
        var well = new Rectangle(frame.X + inset, frame.Y + inset, (int)(CombatArt.Bust.Width * PortraitScale), (int)(CombatArt.Bust.Height * PortraitScale));
        HudKit.Face(spriteBatch, fighter.Portrait, well, HudKit.FaceTint(fighter.IsDown, fighter.IsOut) * alpha);

        DrawHealth(spriteBatch, kit, fighter, alpha);
        if (fighter.IsDown) DrawDeathSaves(spriteBatch, kit, fighter, alpha);
        else DrawUnder(spriteBatch, kit, fighter, alpha);

        AuraRow.DrawArt(spriteBatch, kit, fighter.Auras, AurasAt, AurasPerRow, true, alpha, FaceRight);
    }

    /// <summary>Writes the name, what they are, the numbers on the bars and the auras' timers. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled.</param>
    /// <param name="kit">The HUD's shared art.</param>
    /// <param name="fighter">Whose frame, or null for none.</param>
    /// <param name="alpha">How faded in it is.</param>
    public void DrawWords(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        if (fighter is null) return;
        Rectangle body = Body;
        Color name = fighter.Foe ? CombatStyle.FoeLine : Palette.Heading;
        Text.Draw(spriteBatch, kit.Words, fighter.Name.ToUpperInvariant(), body.X, body.Y, name * alpha, Palette.Shadow * alpha, width: body.Width);
        if (fighter.Line is not null)
            Text.Draw(spriteBatch, kit.Small, fighter.Line.ToUpperInvariant(), body.X, LineTop, Palette.Note * alpha, Palette.Shadow * alpha,
                width: body.Width - AsideWidth(kit, fighter));
        DrawAside(spriteBatch, kit, fighter, alpha);

        Rectangle health = HealthBar(kit);
        if (fighter.IsDown)
        {
            HudKit.Outlined(spriteBatch, kit.Small, Down, health.Center.ToVector2(), Palette.Ember * alpha, alpha, align: 0.5f, middle: true);
        }
        else
        {
            string hp = fighter.Hp.ToString("N0", CultureInfo.InvariantCulture) + " / " + fighter.MaxHp.ToString("N0", CultureInfo.InvariantCulture);
            if (fighter.TempHp > 0) hp += "  +" + fighter.TempHp.ToString(CultureInfo.InvariantCulture);
            HudKit.Outlined(spriteBatch, kit.Small, hp, health.Center.ToVector2(), Color.White * alpha, alpha, align: 0.5f, middle: true);
        }
        if (!fighter.IsDown) DrawUnderWords(spriteBatch, kit, fighter, alpha);

        AuraRow.DrawWords(spriteBatch, kit, fighter.Auras, AurasAt, AurasPerRow, true, alpha, FaceRight);
    }

    /// <summary>Draws the bar under the health: the player's Focus, the target's cast.</summary>
    protected abstract void DrawUnder(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha);

    /// <summary>Writes the numbers on the bar under the health.</summary>
    protected abstract void DrawUnderWords(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha);

    /// <summary>Writes anything at the far end of the name's row (the target's armour class).</summary>
    protected virtual void DrawAside(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha) { }

    /// <summary>How wide whatever's at the far end of the name's row is, so the line under the name keeps clear of it.</summary>
    protected virtual float AsideWidth(HudKit kit, FighterHudView fighter) => 0f;

    /// <summary>The health bar: its lag, its fill, temporary hit points over its end, grey when down.</summary>
    private void DrawHealth(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        Rectangle bar = HealthBar(kit);
        float share = fighter.IsDown ? 0f : fighter.HealthShare;
        float lag = fighter.IsDown ? 0f : _lag.Step(fighter.Id, share, kit.Delta);
        kit.Bar(spriteBatch, bar, fighter.IsDown ? 1f : 0f, DownBar, alpha);
        Rectangle inside = HudKit.Inside(bar);
        kit.Fill(spriteBatch, inside, share, lag, HudKit.Lag, alpha * 0.85f);
        kit.Fill(spriteBatch, inside, 0f, share, fighter.Foe ? HudKit.FoeHealth : HudKit.PartyHealth, alpha);
        if (fighter.TempHp > 0 && fighter.MaxHp > 0)
        {
            float temp = (float)fighter.TempHp / fighter.MaxHp;
            float from = MathF.Min(share, 1f - temp);
            kit.Fill(spriteBatch, inside, from, from + temp, HudKit.TempHp, alpha * 0.9f);
        }
    }

    /// <summary>The three death saves made and failed, as hud.png's sockets, where the bar under the health goes.</summary>
    private void DrawDeathSaves(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        Rectangle under = UnderBar(kit);
        Rectangle socket = CombatIcons.Piece("death-save-empty");
        int x = under.X;
        for (int i = 0; i < 3; i++, x += socket.Width + SaveGap)
            kit.Art.Piece(spriteBatch, i < fighter.DeathSuccesses ? "death-save-success" : "death-save-empty", new Vector2(x, under.Y + 1), 1f, Color.White * alpha);
        x += socket.Width;
        for (int i = 0; i < 3; i++, x += socket.Width + SaveGap)
            kit.Art.Piece(spriteBatch, i < fighter.DeathFailures ? "death-save-failure" : "death-save-empty", new Vector2(x, under.Y + 1), 1f, Color.White * alpha);
    }
}
