using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// What's left of the turn, on the bar between the frame and the hotbar: the action's circle and the
/// bonus action's triangle over the reaction's diamond and the Shift's two arrows.
/// </summary>
/// <remarks>
/// Spec 6.7's budget display, in rows. Each pip is full, hollow once it's spent, or pale for what the
/// thing you're pointing at would spend, so you can see before you click that Healing Word is your
/// bonus action and not your action, or that the plate you're pointing at would use your Shift. The
/// Focus a skill costs shows on the frame's Focus bar instead.
/// </remarks>
public class BudgetBar
{
    /// <summary>How much the hud art is blown up.</summary>
    private const float Scale = 2f;

    /// <summary>Where the block of pips starts across the screen: right of the frame, in screen pixels.</summary>
    private const int Left = 428;

    /// <summary>The room between two pips, in screen pixels.</summary>
    private const int PipGap = 6;

    private CombatArt _art;

    /// <summary>Keeps the shared art.</summary>
    /// <param name="content">The content manager (unused: the pips have no words).</param>
    /// <param name="art">The fight's sheets.</param>
    public void LoadContent(ContentManager content, CombatArt art) => _art = art;

    /// <summary>Which part the mouse is on.</summary>
    /// <param name="mouse">Where the mouse is, in screen pixels.</param>
    /// <returns>The part, or null.</returns>
    public BudgetPart? At(Point mouse)
    {
        foreach (BudgetPart part in Enum.GetValues<BudgetPart>())
            if (Bounds(part).Contains(mouse)) return part;
        return null;
    }

    /// <summary>Where a part is on screen: two pips on top, two under, the block's middle level with the hotbar's.</summary>
    /// <param name="part">Which.</param>
    public static Rectangle Bounds(BudgetPart part)
    {
        int pip = (int)(CombatIcons.Piece("action").Width * Scale);
        int top = CombatHotbar.Top + (CombatHotbar.SlotSize - pip * 2 - PipGap) / 2;
        int column = (int)part % 2, row = (int)part / 2;
        return new Rectangle(Left + column * (pip + PipGap), top + row * (pip + PipGap), pip, pip);
    }

    /// <summary>Draws the pips. Goes in a point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, point-sampled, deferred.</param>
    /// <param name="budget">What's left of the turn.</param>
    /// <param name="spends">What the ability you're pointing at would spend, or null.</param>
    /// <param name="shifting">Whether what you're pointing at would use the Shift.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    public void DrawArt(SpriteBatch spriteBatch, TurnBudget budget, ActionCost? spends, bool shifting, float alpha)
    {
        DrawPip(spriteBatch, BudgetPart.Action, "action", budget.Action, spends == ActionCost.Action, alpha);
        DrawPip(spriteBatch, BudgetPart.Bonus, "bonus", budget.BonusAction, spends == ActionCost.BonusAction, alpha);
        DrawPip(spriteBatch, BudgetPart.Reaction, "reaction", budget.Reaction, spends == ActionCost.Reaction, alpha);
        DrawPip(spriteBatch, BudgetPart.Shift, "shift", budget.Shift, shifting, alpha);
    }

    /// <summary>One pip: full, spent, or pale for what's being pointed at.</summary>
    private void DrawPip(SpriteBatch spriteBatch, BudgetPart part, string piece, bool left, bool previewed, float alpha)
    {
        string state = !left ? piece + "-spent" : previewed ? piece + "-preview" : piece;
        _art.Piece(spriteBatch, state, Bounds(part).Location.ToVector2(), Scale, Color.White * alpha);
    }
}
