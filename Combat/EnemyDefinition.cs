using System;
using System.Collections.Generic;
using LegendQuest.Combat.Realtime;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// A foe's stat block: how it looks, how hard it is to hit and to kill, how fast it is, and what it
/// does.
/// </summary>
/// <remarks>
/// The SRD's monster stat block, cut to what the engine uses. Every one of a kind has its hit dice's
/// average, the way the SRD prints them, so a fight plays the same way twice. It looks like one of
/// the Creatures, or like one of the Cast.
/// </remarks>
public sealed class EnemyDefinition
{
    /// <summary>What encounters call it.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen and in the log.</summary>
    public required string Name { get; init; }

    /// <summary>Which of the Creatures it's drawn as, or null if it's one of the Cast.</summary>
    public string Creature { get; init; }

    /// <summary>Which of the Cast it's drawn as, or null if it's a creature.</summary>
    public string Cast { get; init; }

    /// <summary>Its armour class.</summary>
    public required int ArmourClass { get; init; }

    /// <summary>Its hit dice, Constitution included ("2d8+2"). It has their average.</summary>
    public required DiceExpression HitDice { get; init; }

    /// <summary>How far it moves in a turn, in metres.</summary>
    public required float Speed { get; init; }

    /// <summary>Its six scores: Dexterity for initiative, and every one for its saves.</summary>
    public required AbilityScores Scores { get; init; }

    /// <summary>Its attacks. The first is its main one; with Multiattack the Attack action makes them all.</summary>
    public required IReadOnlyList<EnemyAttack> Attacks { get; init; }

    /// <summary>What it can do beyond the actions everyone has, and what it always does (Pack Tactics).</summary>
    public IReadOnlyList<AbilityId> Abilities { get; init; } = Array.Empty<AbilityId>();

    /// <summary>The XP it's worth, the SRD's for its challenge.</summary>
    public int Xp { get; init; }

    /// <summary>The loot table its body's searched with, by LootTableCatalog id, or null for nothing.</summary>
    public string LootTable { get; init; }

    /// <summary>Whether it fights from range, so the AI keeps it away from the front.</summary>
    public bool Ranged { get; init; }

    /// <summary>Whether it's big enough to stand across two slots side by side (the Thornmother). Nothing can pull or push it.</summary>
    public bool Big { get; init; }

    /// <summary>The Focus each of its hits takes from whoever it hits, as well as the damage, or 0.</summary>
    public int FocusDrain { get; init; }

    // Real time (11-combat.md §5.1). The turn-based engine reads none of these.

    /// <summary>Its auto attack's swing timer, or 0 for its size's: 2.5 s for most, 3.0 s for a big one (QuickSwingMs for quick beasts is set here).</summary>
    public int AttackMs { get; init; }

    /// <summary>How many of its attacks a Multiattack swing strikes, in order, in a combo; 0 for all of them.</summary>
    public int Combo { get; init; }

    /// <summary>Its telegraphed moves from the start of the fight, by SkillCatalog id (foe-trip, foe-blasting-charge).</summary>
    public IReadOnlyList<string> Skills { get; init; } = Array.Empty<string>();

    /// <summary>A move its auto attack makes instead of a plain attack (the hexer's Ashen Bolt, with its cast), by SkillCatalog id, or null.</summary>
    public string AutoSkill { get; init; }

    /// <summary>A boss's phases after its first, in order, each at a share of its hit points (11-combat.md §5.2).</summary>
    public IReadOnlyList<FoePhase> Phases { get; init; } = Array.Empty<FoePhase>();

    /// <summary>How it holds what it fights with, for its clips; null for a creature's claws and teeth (Unarmed), or what its first attack's name suggests.</summary>
    public WeaponKind? Weapon { get; init; }

    /// <summary>Whether it's undead or a fiend, for the skills that do more to them (Divine Smite, Dawnlight).</summary>
    public bool Undead { get; init; }

    /// <summary>Whether it's a beast or a swarm, for the skills that care (the Hivekeeper's They Know Me).</summary>
    public bool Beast { get; init; }

    /// <summary>The swing timer of a quick beast: wolves and wraiths.</summary>
    public const int QuickSwingMs = 2000;

    /// <summary>The swing timer of most foes.</summary>
    public const int SwingMs = 2500;

    /// <summary>The swing timer of a big one.</summary>
    public const int BigSwingMs = 3000;

    /// <summary>Its swing timer: AttackMs, or its size's.</summary>
    public int SwingTimeMs => AttackMs > 0 ? AttackMs : Big ? BigSwingMs : SwingMs;

    /// <summary>How many times its hit dice's average its hit points are in real time, where a party lands several blows for every one of a turn: DefaultHpScale unless set.</summary>
    /// <remarks>11-combat.md §9.2's first lever for a fight's length. The turn-based engine doesn't read it.</remarks>
    public float HpScale { get; init; }

    /// <summary>The real-time hit point scale of a stat block that doesn't set its own.</summary>
    public const float DefaultHpScale = 4.5f;

    /// <summary>Its real-time hit point scale: HpScale, or the default.</summary>
    public float RealtimeHpScale => HpScale > 0f ? HpScale : DefaultHpScale;

    /// <summary>The share of what its blows roll that they do in real time, where it swings two or three times for every one of a turn while the party's hit points stay as they were: DefaultDamageScale unless set.</summary>
    /// <remarks>11-combat.md §9.2's damage lever. The turn-based engine doesn't read it.</remarks>
    public float DamageScale { get; init; }

    /// <summary>The real-time damage scale of a stat block that doesn't set its own.</summary>
    public const float DefaultDamageScale = 0.5f;

    /// <summary>Its real-time damage scale: DamageScale, or the default.</summary>
    public float RealtimeDamageScale => DamageScale > 0f ? DamageScale : DefaultDamageScale;
}
