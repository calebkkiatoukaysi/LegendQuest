namespace LegendQuest.Combat;

/// <summary>
/// What's left of someone's turn: the Shift, the action circle, the bonus action triangle, and the
/// reaction.
/// </summary>
/// <remarks>
/// 5e's action economy, with BG3's free Shift in place of movement. Everything comes back at the
/// start of the owner's own turn, the reaction included, so a reaction spent on someone else's turn
/// is gone until then. Only the engine spends it.
/// </remarks>
public sealed class TurnBudget
{
    /// <summary>Whether the turn's free Shift is still there.</summary>
    public bool Shift { get; private set; }

    /// <summary>Whether the action's still there.</summary>
    public bool Action { get; private set; }

    /// <summary>Whether the bonus action's still there.</summary>
    public bool BonusAction { get; private set; }

    /// <summary>Whether the reaction's still there. Everyone has theirs from the start of the fight.</summary>
    public bool Reaction { get; private set; } = true;

    /// <summary>Whether the Attack action's been taken this turn, which the off-hand attack needs first.</summary>
    internal bool Attacked { get; set; }

    /// <summary>Actions this turn on top of the one (Action Surge), each spent before the action itself.</summary>
    internal int ExtraActions { get; set; }

    /// <summary>Gives everything back at the start of the owner's turn.</summary>
    internal void Start()
    {
        Shift = Action = BonusAction = Reaction = true;
        Attacked = false;
        ExtraActions = 0;
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
            case ActionCost.Action when ExtraActions > 0: ExtraActions--; break;
            case ActionCost.Action: Action = false; break;
            case ActionCost.BonusAction: BonusAction = false; break;
            case ActionCost.Reaction: Reaction = false; break;
        }
    }

    /// <summary>Spends the turn's Shift: on a Shift, or on getting up from Prone.</summary>
    internal void SpendShift() => Shift = false;

    /// <summary>Gives the turn's Shift back, for a skill that comes with a free one (Cunning Step).</summary>
    internal void GiveShift() => Shift = true;
}
