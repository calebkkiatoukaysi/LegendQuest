using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// One ability: what it's called, what it costs, who it's pointed at and who it catches, which rows
/// it reaches, where it goes on the bar, and what it rolls and does.
/// </summary>
/// <remarks>
/// Data, like an ItemDefinition. Kind says how the engine resolves it and the rest fills that in, so
/// most abilities need no code of their own. Shape and Reach are 11-combat.md §2's; Focus, cooldowns
/// and once a fight are §3.2's.
/// </remarks>
public sealed class AbilityDefinition
{
    /// <summary>Which ability this is.</summary>
    public required AbilityId Id { get; init; }

    /// <summary>What it's called on the bar and in the log.</summary>
    public required string Name { get; init; }

    /// <summary>What it does, for its tooltip: a sentence or two.</summary>
    public required string Description { get; init; }

    /// <summary>How it works when it's used.</summary>
    public required AbilityKind Kind { get; init; }

    /// <summary>What part of the turn it spends.</summary>
    public ActionCost Cost { get; init; }

    /// <summary>Whether an ally target has to be somebody other than the user.</summary>
    public bool NotSelf { get; init; }

    /// <summary>
    /// The damage it does, or the die it adds: Hex's extra necrotic and Bardic Inspiration's d6 are here
    /// too. None for no damage.
    /// </summary>
    public DiceExpression Damage { get; init; } = DiceExpression.None;

    /// <summary>The kind of damage it does.</summary>
    public DamageType DamageType { get; init; }

    /// <summary>For a save, which ability the target saves with, or null if there's no save.</summary>
    public Ability? Save { get; init; }

    /// <summary>A foe's ability's own save DC, or 0 to use the user's spell save DC.</summary>
    public int Dc { get; init; }

    /// <summary>Whether a target who makes the save still takes half the damage, rather than none.</summary>
    public bool SaveHalves { get; init; }

    /// <summary>The conditions it puts on its target: on a failed save for a Save, on a hit for an attack, always for a Condition.</summary>
    public IReadOnlyList<ConditionId> Conditions { get; init; } = Array.Empty<ConditionId>();

    /// <summary>How many of the bearer's turns its conditions last, counted at the end of each, or 0 for until something ends them.</summary>
    public int Turns { get; init; }

    /// <summary>Whether it needs the caster's concentration: taking damage can break it, and casting another ends it.</summary>
    public bool Concentration { get; init; }

    /// <summary>The hit points it gives back, before what's added to them. None for no healing (and for Lay on Hands, which heals what its pool allows).</summary>
    public DiceExpression Heals { get; init; } = DiceExpression.None;

    /// <summary>Whether the user's casting modifier is added to Heals.</summary>
    public bool HealsAddCasting { get; init; }

    /// <summary>Whether the user's level is added to Heals.</summary>
    public bool HealsAddLevel { get; init; }

    /// <summary>Whether it's a spell: it rolls with the casting ability, and armour you aren't trained in stops it.</summary>
    public bool Spell { get; init; }

    /// <summary>The item in the party's bag it uses up, by its ItemCatalog id, or null.</summary>
    public string Item { get; init; }

    /// <summary>For a foe, the d6 roll at the start of its turn that gives it back once it's been used (5 is "recharge 5-6"), or 0 if it doesn't need one.</summary>
    public int RechargeOn { get; init; }

    /// <summary>Who it catches once it's pointed: one person, or a pattern of slots round them (CombatEngine.Caught).</summary>
    public TargetShape Shape { get; init; }

    /// <summary>Which rows it can get at, and from which. Weapon takes it from the weapon in hand.</summary>
    public Reach Reach { get; init; }

    /// <summary>How many: RandomFoes' picks, or darts that each roll Damage on their own.</summary>
    public int Count { get; init; }

    /// <summary>The Focus it costs, or 0.</summary>
    public int Focus { get; init; }

    /// <summary>How many of the user's next turns it can't be used on once it has been, or 0.</summary>
    public int Cooldown { get; init; }

    /// <summary>Whether it works once a fight: every ultimate, and the foes' abilities that used to spend something.</summary>
    public bool OncePerFight { get; init; }

    /// <summary>The Focus using it gives back: an auto attack's 10, or 0.</summary>
    public int FocusGain { get; init; }

    /// <summary>Where it goes on the bar, which lays the bar out.</summary>
    public BarSlot Bar { get; init; }

    /// <summary>For a pool (Lay on Hands), the hit points it holds a fight for each of the user's levels, or 0 for no pool.</summary>
    public int PoolPerLevel { get; init; }

    /// <summary>Whether it moves its target between its rows when it lands.</summary>
    public RowMove RowMove { get; init; }

    /// <summary>Whether it comes with a free Shift that no opportunity attack follows (Cunning Step).</summary>
    public bool FreeShift { get; init; }
}
