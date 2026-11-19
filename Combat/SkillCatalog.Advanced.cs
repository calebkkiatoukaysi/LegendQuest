using System.Collections.Generic;
using LegendQuest.Combat.Realtime;

namespace LegendQuest.Combat;

/// <summary>
/// The sixteen advanced classes' kits and the Hourwarden's (10-classes.md §3), converted to real time
/// by 11-combat.md §4.2 and §4.3 as the base kits are: Act 1's in SkillCatalog.ActOne.cs, the rest in
/// SkillCatalog.ActTwo.cs. This file gathers them, and holds what more than one of them uses.
/// </summary>
/// <remarks>
/// The tables were written for turns and rows, and some of their lines say things only turns and
/// rows could say. Each row's Description says what it does in real time; where the reading isn't
/// the plain conversion, 10-classes.md §8 lists it. Initiative is gone, so the passives that gave it
/// (Weather Eye, Count the Ground, No Procedure) give a head start instead: Focus, or a first swing
/// at once. Gold and shop prices are data for the screens (Battle.Stolen, Battle.GoldBonusPct, the
/// PricePct stat); the fight itself only reports them.
/// </remarks>
public static partial class SkillCatalog
{
    /// <summary>The advanced classes' rows, in 10-classes.md §3's order.</summary>
    /// <param name="rows">The catalog's rows, to add them to.</param>
    static partial void Advanced(List<SkillDefinition> rows)
    {
        rows.AddRange(ActOneClasses());
        rows.AddRange(LaterClasses());
    }

    /// <summary>Knocked down for the knockdown's 1.5 s (the tables' "Prone").</summary>
    private static AuraDefinition Knockdown(string id) => Condition(id, "Knocked Down", ConditionId.Prone, Battle.KnockdownMs);

    /// <summary>A mark the next attack on its bearer uses up, with its advantage (the tables' "Marked").</summary>
    private static AuraDefinition MarkedFor(string id, int ms) =>
        new() { Id = id, Name = "Marked", Condition = ConditionId.Marked, DurationMs = ms, UsedBy = AuraUse.AttackTaken };

    /// <summary>The Hivekeeper's honey: 2 hit points every 3 s for 9 s, which no cleanse takes off.</summary>
    private static readonly AuraDefinition Honey = new()
    {
        Id = "honey-salve", Name = "Honey Salve", Beneficial = true, DurationMs = 3 * Turn, TickDice = D("2"), TickHeals = true, Uncleansable = true,
    };

    /// <summary>The control conditions a foe can put on someone, which Choose It Yourself takes off.</summary>
    private static readonly ConditionId[] ControlConditions =
    {
        ConditionId.Stunned, ConditionId.Frozen, ConditionId.Restrained, ConditionId.Prone, ConditionId.Charmed, ConditionId.Frightened, ConditionId.Blinded,
    };

    /// <summary>A passive's head start at the start of a fight: Focus for the whole party.</summary>
    private static Effect PartyFocus(int amount) => new() { Kind = EffectType.Focus, Amount = amount, On = EffectOn.AllAllies };
}
