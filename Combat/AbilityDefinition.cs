using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// One ability: what it's called, what it costs, what it's pointed at, how far it reaches, and what
/// it rolls and does.
/// </summary>
/// <remarks>
/// Data, like an ItemDefinition. Kind says how the engine resolves it and the rest fills that in, so
/// most abilities need no code of their own. Ranges are metres (a tile is 1.5 m).
/// </remarks>
public sealed class AbilityDefinition
{
    /// <summary>Which ability this is.</summary>
    public required AbilityId Id { get; init; }

    /// <summary>What it's called on the hotbar and in the log.</summary>
    public required string Name { get; init; }

    /// <summary>What it does, for its tooltip: a sentence or two.</summary>
    public required string Description { get; init; }

    /// <summary>How it works when it's used.</summary>
    public required AbilityKind Kind { get; init; }

    /// <summary>What part of the turn it spends.</summary>
    public ActionCost Cost { get; init; }

    /// <summary>What it's pointed at.</summary>
    public TargetKind Target { get; init; }

    /// <summary>How far it reaches, in metres. Zero for a weapon means the weapon's own reach or range, and for something thrown, the item's.</summary>
    public float Range { get; init; }

    /// <summary>For an area, how far from the tile it's centred on it reaches, in metres. On a Self ability it's a burst round the user that catches everyone else that close.</summary>
    public float Radius { get; init; }

    /// <summary>Whether the user has to be able to see the target (or the tile) to use it.</summary>
    public bool NeedsSight { get; init; }

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

    /// <summary>For AutoHit, how many darts, each one rolling Damage on its own.</summary>
    public int Darts { get; init; }

    /// <summary>The hit points it gives back, before what's added to them. None for no healing (and for Lay on Hands, which heals what its pool allows).</summary>
    public DiceExpression Heals { get; init; } = DiceExpression.None;

    /// <summary>Whether the user's casting modifier is added to Heals.</summary>
    public bool HealsAddCasting { get; init; }

    /// <summary>Whether the user's level is added to Heals.</summary>
    public bool HealsAddLevel { get; init; }

    /// <summary>Whether it's a spell: it rolls with the casting ability, and armour you aren't trained in stops it.</summary>
    public bool Spell { get; init; }

    /// <summary>The level of slot a spell spends: 0 for a cantrip, or for anything that isn't a spell.</summary>
    public int SpellSlotLevel { get; init; }

    /// <summary>The resource it spends, by its ResourceCatalog id, or null if it's free.</summary>
    public string Resource { get; init; }

    /// <summary>How much of Resource one use spends.</summary>
    public int Spends { get; init; } = 1;

    /// <summary>The item in the party's bag it uses up, by its ItemCatalog id, or null.</summary>
    public string Item { get; init; }

    /// <summary>For a foe, the d6 roll at the start of its turn that gives it back once it's been used (5 is "recharge 5-6"), or 0 if it doesn't need one.</summary>
    public int RechargeOn { get; init; }

    /// <summary>The rest that gives its uses back, from its resource. None for a free one.</summary>
    public RestKind Recharge => Resource is null ? RestKind.None : ResourceCatalog.Get(Resource).Recharge;

    /// <summary>Whether it's a cantrip: a spell that costs no slot.</summary>
    public bool Cantrip => Spell && SpellSlotLevel == 0;
}
