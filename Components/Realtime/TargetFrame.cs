using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The target's frame, top centre (11-combat.md §1): its face at the right, its name, what it is
/// and its armour class, its hit points, its cast bar while it winds something up, and its auras
/// under it.
/// </summary>
/// <remarks>
/// The armour class sits at the end of the line under the name. A foe's is framed in red and its health is red; an ally targeted for a heal gets the party's
/// frame and green. With a boss up, its wide frame takes the top centre and the screen moves this
/// one under it with Top, for an add or an ally targeted instead.
/// </remarks>
public class TargetFrame : FighterFrame
{
    /// <summary>How wide the frame is, and where its top is unless moved, in screen pixels.</summary>
    public const int Width = 440;
    public const int DefaultTop = 16;

    /// <summary>The top of the frame, in screen pixels: under the boss's frame when there's one.</summary>
    public int Top { get; set; } = DefaultTop;

    /// <inheritdoc />
    public override Rectangle Bounds => new((LegendQuestGame.ScreenWidth - Width) / 2, Top, Width, Height);

    /// <inheritdoc />
    protected override bool FaceRight => true;

    /// <inheritdoc />
    protected override string PortraitPiece(FighterHudView fighter) => fighter.Foe ? "portrait-foe" : "portrait-party";

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        if (fighter.Casting is not null) CastBar.DrawArt(spriteBatch, kit, CastArea(kit), fighter.Casting, fighter.Foe, alpha);
    }

    /// <inheritdoc />
    protected override void DrawUnderWords(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        if (fighter.Casting is not null) CastBar.DrawWords(spriteBatch, kit, CastArea(kit), fighter.Casting, alpha);
    }

    /// <inheritdoc />
    protected override void DrawAside(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        if (fighter.ArmourClass <= 0) return;
        Rectangle body = Body;
        Text.Draw(spriteBatch, kit.Small, ArmourClass(fighter), body.Right, LineTop, Palette.Bone * alpha, Palette.Shadow * alpha, rightAligned: true);
    }

    /// <inheritdoc />
    protected override float AsideWidth(HudKit kit, FighterHudView fighter) =>
        fighter.ArmourClass > 0 ? kit.Small.MeasureString(ArmourClass(fighter)).X + 12f : 0f;

    /// <summary>Where the cast bar is: under the health, with room at its end for the lock.</summary>
    private Rectangle CastArea(HudKit kit)
    {
        Rectangle under = UnderBar(kit);
        return new Rectangle(under.X, under.Y, under.Width - 14, under.Height + 4);
    }

    /// <summary>What the armour class says.</summary>
    private static string ArmourClass(FighterHudView fighter) => "AC " + fighter.ArmourClass.ToString(CultureInfo.InvariantCulture);
}
