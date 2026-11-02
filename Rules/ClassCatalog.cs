using System;
using System.Collections.Generic;
using LegendQuest.Items;

namespace LegendQuest.Rules;

/// <summary>
/// Every class, by ClassId.
/// </summary>
/// <remarks>
/// A static table keyed by the enum, like The Black Box's ItemCatalog. The hit dice, armour,
/// shields, martial weapons and casting abilities are 5e's, from the SRD 5.1: a d10 for a fighter
/// or a paladin, a d8 for a rogue, a cleric, a druid, a warlock or a bard, a d6 for a wizard. One
/// simplification: every class here is trained with every simple weapon, where the SRD gives
/// wizards and druids a shorter list, so Sera's shortbow is hers to use. The Hexblade fights with a
/// pact blade, so it's trained with the shortsword in its kit. The names are the story's own; the
/// kits are the team's, each class's weapon, shield and armour plus what everyone sets out in.
/// </remarks>
public static class ClassCatalog
{
    /// <summary>What everyone sets out in, whatever their class: a cloak against the weather and boots for the road.</summary>
    /// <remarks>Before Table, since static fields are set in the order they're written and Table's kits read this.</remarks>
    private static readonly string[] Basics = { "travelling-cloak", "leather-boots" };

    private static readonly ClassDefinition[] Table =
    {
        new()
        {
            Id = ClassId.Fighter, Name = "Blademaster", Role = "Frontline weapon specialist.", HitDie = 10,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 13, Constitution = 14, Intelligence = 8, Wisdom = 12, Charisma = 10 },
            ArmourUpTo = ArmourCategory.Heavy, Shields = true, MartialWeapons = true,
            StartingKit = Kit("longsword", "wooden-shield", "chain-mail"),
        },
        new()
        {
            Id = ClassId.Rogue, Name = "Shadowstep", Role = "Stealth, crits and lockpicking.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 15, Constitution = 14, Intelligence = 12, Wisdom = 10, Charisma = 13 },
            ArmourUpTo = ArmourCategory.Light, WeaponsAlso = new[] { "longsword", "rapier", "shortsword" },
            StartingKit = Kit("shortsword", "dagger", "leather-armour"),
        },
        new()
        {
            Id = ClassId.Cleric, Name = "Lightbearer", Role = "Support and holy damage.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 13, Dexterity = 10, Constitution = 14, Intelligence = 8, Wisdom = 15, Charisma = 12 },
            ArmourUpTo = ArmourCategory.Medium, Shields = true, CastingAbility = Ability.Wisdom,
            StartingKit = Kit("mace", "wooden-shield", "scale-mail"),
        },
        new()
        {
            Id = ClassId.Wizard, Name = "Arcanist", Role = "Studied spellcaster, burst damage.", HitDie = 6,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 15, Wisdom = 12, Charisma = 10 },
            ArmourUpTo = ArmourCategory.None, CastingAbility = Ability.Intelligence,
            StartingKit = Kit("quarterstaff", "mage-robes"),
        },
        new()
        {
            Id = ClassId.Paladin, Name = "Warden", Role = "Oath-bound tank and healer.", HitDie = 10,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 10, Constitution = 13, Intelligence = 8, Wisdom = 12, Charisma = 14 },
            ArmourUpTo = ArmourCategory.Heavy, Shields = true, MartialWeapons = true, CastingAbility = Ability.Charisma,
            StartingKit = Kit("longsword", "steel-shield", "chain-mail"),
        },
        new()
        {
            Id = ClassId.Druid, Name = "Wildcaller", Role = "Nature magic, a beast companion.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 10, Dexterity = 13, Constitution = 14, Intelligence = 12, Wisdom = 15, Charisma = 8 },
            ArmourUpTo = ArmourCategory.Medium, Shields = true, WeaponsAlso = new[] { "scimitar" }, CastingAbility = Ability.Wisdom,
            StartingKit = Kit("quarterstaff", "hide-armour", "wooden-shield"),
        },
        new()
        {
            Id = ClassId.Warlock, Name = "Hexblade", Role = "Pact magic that drains life.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 10, Wisdom = 12, Charisma = 15 },
            ArmourUpTo = ArmourCategory.Light, WeaponsAlso = new[] { "shortsword" }, CastingAbility = Ability.Charisma,
            StartingKit = Kit("shortsword", "leather-armour"),
        },
        new()
        {
            Id = ClassId.Bard, Name = "Skald", Role = "Buffs, debuffs and persuasion.", HitDie = 8,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 14, Constitution = 13, Intelligence = 10, Wisdom = 12, Charisma = 15 },
            ArmourUpTo = ArmourCategory.Light, WeaponsAlso = new[] { "longsword", "rapier", "shortsword" }, CastingAbility = Ability.Charisma,
            StartingKit = Kit("rapier", "leather-armour"),
        },
    };

    private static readonly Dictionary<ClassId, ClassDefinition> ById = new();

    /// <summary>Indexes the table, and checks every class has exactly one row and a kit it can wear.</summary>
    /// <remarks>Throws on purpose, so a class added to the enum and not here, or a kit with a typo in it, shows up when the game starts.</remarks>
    static ClassCatalog()
    {
        foreach (ClassDefinition row in Table)
        {
            if (row.HitDie < 1) throw new InvalidOperationException($"Class {row.Id} has a d{row.HitDie} for a hit die.");
            if (row.Speed < 1) throw new InvalidOperationException($"Class {row.Id} moves {row.Speed} m a turn.");

            foreach (string weapon in row.WeaponsAlso)
            {
                if (!ItemCatalog.Exists(weapon) || ItemCatalog.Get(weapon).Kind != ItemKind.Weapon || ItemCatalog.Get(weapon).Category != WeaponCategory.Martial)
                    throw new InvalidOperationException($"Class {row.Id} is trained with {weapon} as well, which isn't a martial weapon.");
            }

            if (row.KitProblem(row.StartingKit) is string problem) throw new InvalidOperationException($"Class {row.Id}'s kit {problem}.");
            if (!ById.TryAdd(row.Id, row)) throw new InvalidOperationException($"Class {row.Id} is in the table twice.");
        }

        foreach (ClassId id in Enum.GetValues<ClassId>())
            if (!ById.ContainsKey(id)) throw new InvalidOperationException($"Class {id} isn't in ClassCatalog.");
    }

    /// <summary>One class.</summary>
    /// <param name="id">Which.</param>
    public static ClassDefinition Get(ClassId id) => ById[id];

    /// <summary>A kit: these items, then what everyone sets out in.</summary>
    /// <param name="own">The item ids that are this kit's own, in the order they're put on.</param>
    public static IReadOnlyList<string> Kit(params string[] own)
    {
        var kit = new List<string>(own);
        kit.AddRange(Basics);
        return kit;
    }
}
