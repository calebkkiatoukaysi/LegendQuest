using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// A condition on someone in a fight: which, for how long, and who put it there.
/// </summary>
/// <remarks>
/// Turns count down at the end of each of the bearer's own turns and it comes off at zero, so 1 is
/// "until the end of their next turn" when someone else puts it on them and "until the end of this
/// turn" when they put it on themselves. Dodging is the exception, and ends at the start of their
/// turn instead, as in 5e.
/// </remarks>
public sealed class ActiveCondition
{
    /// <summary>Puts a condition on.</summary>
    /// <param name="id">Which condition.</param>
    /// <param name="turns">How many of the bearer's turns it lasts, or 0 for until something ends it.</param>
    /// <param name="source">The id of whoever put it on, or null for the rules themselves.</param>
    /// <param name="from">The ability that put it on, or null.</param>
    /// <param name="dc">The DC of the save that ends it, for Restrained, or 0.</param>
    /// <param name="die">The die it carries: Hex's extra damage or Bardic Inspiration's bonus, or None.</param>
    internal ActiveCondition(ConditionId id, int turns, string source, AbilityId? from, int dc, DiceExpression die)
    {
        Id = id;
        Turns = turns;
        Source = source;
        From = from;
        Dc = dc;
        Die = die;
    }

    /// <summary>Which condition it is.</summary>
    public ConditionId Id { get; }

    /// <summary>How many more of the bearer's turns it lasts, or 0 for until something ends it.</summary>
    public int Turns { get; internal set; }

    /// <summary>The id of whoever put it on, or null for the rules themselves (being Downed).</summary>
    public string Source { get; }

    /// <summary>The ability that put it on, or null.</summary>
    public AbilityId? From { get; }

    /// <summary>The DC of the Strength save that ends it at the end of each of the bearer's turns, or 0 if none does.</summary>
    public int Dc { get; }

    /// <summary>The die it carries: Hex's extra damage, or Bardic Inspiration's bonus. None for the rest.</summary>
    public DiceExpression Die { get; }
}
