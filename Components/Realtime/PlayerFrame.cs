using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// Your frame, top left (11-combat.md §1): your face, your name and class, your hit points and your
/// Focus, and your auras under it with their time left.
/// </summary>
/// <remarks>
/// The Focus bar is the fight's cool blue with a tick every ten; what the slot under the mouse
/// would spend shows pale at its end, or the whole of it red when there isn't enough. Your
/// passives are auras that last, so they sit in the row with no timer.
/// </remarks>
public class PlayerFrame : FighterFrame
{
    /// <summary>Where the frame is and how wide, in screen pixels.</summary>
    public const int Left = 16;
    public const int Top = 16;
    public const int Width = 372;

    /// <summary>How much Focus a tick on the bar marks, and the room kept at the bar's right for its number.</summary>
    private const int FocusTick = 10;
    private const int NumberRoom = 34;

    /// <summary>What the slot under the mouse would spend, shown on the Focus bar, or 0.</summary>
    public int Spend { get; set; }

    /// <inheritdoc />
    public override Rectangle Bounds => new(Left, Top, Width, Height);

    /// <inheritdoc />
    protected override bool FaceRight => false;

    /// <inheritdoc />
    protected override string PortraitPiece(FighterHudView fighter) => "portrait-current";

    /// <summary>Where the Focus bar is: the bar under the health, short of the room for its number.</summary>
    private Rectangle FocusBar(HudKit kit)
    {
        Rectangle under = UnderBar(kit);
        return new Rectangle(under.X, under.Y, under.Width - NumberRoom, under.Height);
    }

    /// <inheritdoc />
    protected override void DrawUnder(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        if (fighter.MaxFocus <= 0) return;
        Rectangle bar = FocusBar(kit);
        float have = MathHelper.Clamp((float)fighter.Focus / fighter.MaxFocus, 0f, 1f);
        float spend = MathHelper.Clamp((float)Spend / fighter.MaxFocus, 0f, 1f);
        kit.Bar(spriteBatch, bar, 0f, Color.Transparent, alpha);
        Rectangle inside = HudKit.Inside(bar);
        if (Spend > fighter.Focus)
        {
            kit.Fill(spriteBatch, inside, 0f, have, HudKit.Bad, alpha);
        }
        else
        {
            kit.Fill(spriteBatch, inside, 0f, have - spend, HudKit.Focus, alpha);
            kit.Fill(spriteBatch, inside, have - spend, have, Color.Lerp(HudKit.Focus, Color.White, 0.6f), alpha);
        }
        for (int i = FocusTick; i < fighter.MaxFocus; i += FocusTick)
        {
            int x = inside.X + (int)System.MathF.Round(inside.Width * (float)i / fighter.MaxFocus);
            spriteBatch.Draw(kit.Pixel, new Rectangle(x, inside.Y, 1, inside.Height), HudKit.BarEdge * (0.55f * alpha));
        }
    }

    /// <inheritdoc />
    protected override void DrawUnderWords(SpriteBatch spriteBatch, HudKit kit, FighterHudView fighter, float alpha)
    {
        if (fighter.MaxFocus <= 0) return;
        Rectangle bar = FocusBar(kit);
        Color colour = Spend > fighter.Focus ? Palette.Ember : CombatStyle.FocusText;
        HudKit.Outlined(spriteBatch, kit.Small, fighter.Focus.ToString(CultureInfo.InvariantCulture), new Vector2(UnderBar(kit).Right, bar.Center.Y),
            colour * alpha, alpha, align: 1f, middle: true);
    }
}
