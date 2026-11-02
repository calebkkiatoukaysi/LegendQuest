using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// What's left of the turn, over the hotbar: the action's circle, the bonus action's triangle, the
/// reaction, and the movement bar with a tick a tile and the metres written beside it.
/// </summary>
/// <remarks>
/// Spec 6.7's budget display. Each pip is full, hollow once it's spent, or pale for what the thing
/// you're pointing at would spend, so you can see before you click that Healing Word is your bonus
/// action and not your action. The movement bar's three-sliced off hud.png: the track as long as the
/// whole turn's movement (a Dash makes it longer and the bar shrinks its tiles to fit), the fill for
/// what's left, and the part a path you're pointing at would use in pale, or the whole of what's left
/// in red if the path's longer than that.
/// </remarks>
public class BudgetBar
{
    /// <summary>How much the hud art is blown up.</summary>
    private const float Scale = 2f;

    /// <summary>How far over the hotbar the pips sit, in screen pixels.</summary>
    private const int AboveHotbar = 10;

    /// <summary>The room between two pips, and between the pips and the bar, in screen pixels.</summary>
    private const int PipGap = 6;
    private const int BarGap = 14;

    /// <summary>How long the movement bar is, in screen pixels, whatever the movement.</summary>
    private const int BarLength = 300;

    /// <summary>The movement pieces' caps and their middle on hud.png, in art pixels: three at each end, and the two columns between stretched.</summary>
    private const int Cap = 3;
    private const int Middle = 2;

    /// <summary>The room between the bar and the metres written after it, in screen pixels.</summary>
    private const int TextGap = 10;

    private CombatArt _art;
    private SpriteFont _font;

    /// <summary>Loads the font and keeps the shared art.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="art">The fight's sheets.</param>
    public void LoadContent(ContentManager content, CombatArt art)
    {
        _font = content.Load<SpriteFont>("spectral-detail");
        _art = art;
    }

    /// <summary>Which part the mouse is on.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <returns>The part, or null.</returns>
    public BudgetPart? At(Point mouse)
    {
        foreach (BudgetPart part in Enum.GetValues<BudgetPart>())
            if (Bounds(part).Contains(mouse)) return part;
        return null;
    }

    /// <summary>Where a part is on screen.</summary>
    /// <param name="part">Which.</param>
    public static Rectangle Bounds(BudgetPart part)
    {
        int pip = (int)(CombatIcons.Piece("action").Width * Scale);
        int top = CombatHotbar.Top - AboveHotbar - pip;
        int barLeft = CombatHotbar.Left + 3 * (pip + PipGap) - PipGap + BarGap;
        return part switch
        {
            BudgetPart.Movement => new Rectangle(barLeft, top + pip / 2 - (int)(CombatIcons.Piece("move-track").Height * Scale) / 2,
                BarLength, (int)(CombatIcons.Piece("move-track").Height * Scale)),
            _ => new Rectangle(CombatHotbar.Left + (int)part * (pip + PipGap), top, pip, pip),
        };
    }

    /// <summary>Draws the pips and the bar. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="budget">What's left of the turn.</param>
    /// <param name="spends">What the thing you're pointing at would spend, or null.</param>
    /// <param name="path">How far, in metres, the path you're pointing at would walk, or 0.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawArt(SpriteBatch spriteBatch, TurnBudget budget, ActionCost? spends, float path, float alpha)
    {
        DrawPip(spriteBatch, BudgetPart.Action, "action", budget.Action, spends == ActionCost.Action, alpha);
        DrawPip(spriteBatch, BudgetPart.Bonus, "bonus", budget.BonusAction, spends == ActionCost.BonusAction, alpha);
        DrawPip(spriteBatch, BudgetPart.Reaction, "reaction", budget.Reaction, spends == ActionCost.Reaction, alpha);

        Rectangle bar = Bounds(BudgetPart.Movement);
        float whole = MathF.Max(budget.Movement, Measure.TileMetres);
        int left = (int)MathF.Round(bar.Width * MathHelper.Clamp(budget.MovementLeft / whole, 0f, 1f));
        int used = (int)MathF.Round(bar.Width * MathHelper.Clamp(MathF.Min(path, budget.MovementLeft) / whole, 0f, 1f));

        ThreeSlice(spriteBatch, "move-track", bar, alpha);
        if (path > budget.MovementLeft + float.Epsilon)
        {
            ThreeSlice(spriteBatch, "move-over", new Rectangle(bar.X, bar.Y, left, bar.Height), alpha);
        }
        else
        {
            ThreeSlice(spriteBatch, "move-fill", new Rectangle(bar.X, bar.Y, left - used, bar.Height), alpha);
            ThreeSlice(spriteBatch, "move-preview", new Rectangle(bar.X + left - used, bar.Y, used, bar.Height), alpha);
        }

        Rectangle tick = CombatIcons.Piece("move-tick");
        int tiles = Measure.Tiles(budget.Movement);
        for (int i = 1; i < tiles; i++)
        {
            float x = bar.X + bar.Width * (i * Measure.TileMetres / whole) - tick.Width * Scale / 2f;
            _art.Piece(spriteBatch, "move-tick", new Vector2(MathF.Round(x), bar.Y), Scale, Color.White * alpha);
        }
    }

    /// <summary>Writes the metres left after the bar, and what a path would cost. Goes in a linear sampled batch after DrawArt.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled, deferred.</param>
    /// <param name="budget">What's left of the turn.</param>
    /// <param name="path">How far, in metres, the path you're pointing at would walk, or 0.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawWords(SpriteBatch spriteBatch, TurnBudget budget, float path, float alpha)
    {
        Rectangle bar = Bounds(BudgetPart.Movement);
        string text = $"{Metres(budget.MovementLeft)} / {Metres(budget.Movement)} M";
        float y = bar.Center.Y - _font.LineSpacing / 2f;
        Text.Draw(spriteBatch, _font, text, bar.Right + TextGap, y, Palette.Bone * alpha, Palette.Shadow * alpha);

        if (path <= 0f) return;
        bool over = path > budget.MovementLeft + float.Epsilon;
        string cost = over ? $"  ·  PATH {Metres(path)} M, TOO FAR" : $"  ·  PATH {Metres(path)} M";
        float x = bar.Right + TextGap + _font.MeasureString(text).X;
        Text.Draw(spriteBatch, _font, cost, x, y, (over ? Palette.Ember : Palette.Gold) * alpha, Palette.Shadow * alpha);
    }

    /// <summary>A distance in metres the way the bar writes it: one decimal place.</summary>
    /// <param name="metres">The distance.</param>
    public static string Metres(float metres) => metres.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>One pip: full, spent, or pale for what's being pointed at.</summary>
    private void DrawPip(SpriteBatch spriteBatch, BudgetPart part, string piece, bool left, bool previewed, float alpha)
    {
        string state = !left ? piece + "-spent" : previewed ? piece + "-preview" : piece;
        _art.Piece(spriteBatch, state, Bounds(part).Location.ToVector2(), Scale, Color.White * alpha);
    }

    /// <summary>One of the movement pieces stretched to a length: its two caps at their size and its middle stretched between.</summary>
    private void ThreeSlice(SpriteBatch spriteBatch, string piece, Rectangle area, float alpha)
    {
        if (area.Width <= 0) return;

        Rectangle source = CombatIcons.Piece(piece);
        int cap = Math.Min((int)(Cap * Scale), area.Width / 2);
        Color colour = Color.White * alpha;
        spriteBatch.Draw(_art.Hud, new Rectangle(area.X, area.Y, cap, area.Height), new Rectangle(source.X, source.Y, Cap, source.Height), colour);
        spriteBatch.Draw(_art.Hud, new Rectangle(area.X + cap, area.Y, area.Width - cap * 2, area.Height), new Rectangle(source.X + Cap, source.Y, Middle, source.Height), colour);
        spriteBatch.Draw(_art.Hud, new Rectangle(area.Right - cap, area.Y, cap, area.Height), new Rectangle(source.Right - Cap, source.Y, Cap, source.Height), colour);
    }
}
