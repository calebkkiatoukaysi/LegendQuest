using System;

namespace LegendQuest.Combat;

/// <summary>
/// What's left of someone's turn: the movement bar, the action circle, the bonus action triangle,
/// and the reaction.
/// </summary>
/// <remarks>
/// 5e's action economy. Everything comes back at the start of the owner's own turn, the reaction
/// included, so a reaction spent on someone else's turn is gone until then. Only the engine spends it.
/// </remarks>
public sealed class TurnBudget
{
    /// <summary>A hair of slack when comparing metres, so 1.5 m left pays for a 1.5 m step.</summary>
    private const float Slack = 0.001f;

    /// <summary>All the movement this turn, a Dash included.</summary>
    public float Movement { get; private set; }

    /// <summary>The movement still to spend, in metres.</summary>
    public float MovementLeft { get; private set; }

    /// <summary>Whether the action's still there.</summary>
    public bool Action { get; private set; }

    /// <summary>Whether the bonus action's still there.</summary>
    public bool BonusAction { get; private set; }

    /// <summary>Whether the reaction's still there. Everyone has theirs from the start of the fight.</summary>
    public bool Reaction { get; private set; } = true;

    /// <summary>The movement the turn started with, before any Dash: what a Dash adds.</summary>
    internal float Speed { get; private set; }

    /// <summary>Whether the Attack action's been taken this turn, which the off-hand attack needs first.</summary>
    internal bool Attacked { get; set; }

    /// <summary>Gives everything back at the start of the owner's turn.</summary>
    /// <param name="movement">How far they can move this turn, in metres.</param>
    internal void Start(float movement)
    {
        Speed = Movement = MovementLeft = MathF.Max(0f, movement);
        Action = BonusAction = Reaction = true;
        Attacked = false;
    }

    /// <summary>Whether there's still enough to pay for something.</summary>
    /// <param name="cost">What it costs.</param>
    internal bool Has(ActionCost cost) => cost switch
    {
        ActionCost.Action => Action,
        ActionCost.BonusAction => BonusAction,
        ActionCost.Reaction => Reaction,
        _ => true,
    };

    /// <summary>Pays for something.</summary>
    /// <param name="cost">What it costs.</param>
    internal void Spend(ActionCost cost)
    {
        switch (cost)
        {
            case ActionCost.Action: Action = false; break;
            case ActionCost.BonusAction: BonusAction = false; break;
            case ActionCost.Reaction: Reaction = false; break;
        }
    }

    /// <summary>Whether there's enough movement left for a distance.</summary>
    /// <param name="metres">How far.</param>
    internal bool CanWalk(float metres) => MovementLeft + Slack >= metres;

    /// <summary>Spends movement.</summary>
    /// <param name="metres">How far they went.</param>
    internal void Walk(float metres) => MovementLeft = MathF.Max(0f, MovementLeft - metres);

    /// <summary>A Dash: as much movement again as the turn started with.</summary>
    internal void Dash()
    {
        Movement += Speed;
        MovementLeft += Speed;
    }
}
