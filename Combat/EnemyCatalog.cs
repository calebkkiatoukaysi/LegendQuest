using System;
using System.Collections.Generic;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;
using LegendQuest.Story;

namespace LegendQuest.Combat;

/// <summary>
/// Every foe's stat block, by id: the story's enemies so far.
/// </summary>
/// <remarks>
/// Written in the SRD 5.1's shape and built to the encounters rather than lifted from it: a level-1
/// party is fragile, and two of these fights are the duo alone, so the early ones are a step under
/// the SRD's bandits and wolves. Each one's numbers are worked out against CombatSimulator's targets
/// (README, "Combat"): what the party does to it a round against its hit points, and what it does
/// to them against theirs. Looks are Creatures where one's drawn, otherwise the Cast's. Each drops
/// its own LootTableCatalog table, by the same id.
/// </remarks>
public static class EnemyCatalog
{
    private static readonly EnemyDefinition[] Table =
    {
        // P-05, the tutorial: three of them over the rail, slow and weak, so the duo can learn on them.
        // Each takes two hits to put down (13 hit points, past almost any single level-1 hit), so you
        // and the Soul-Bound both get to hit the same one and the Soul-Link fills, twice, before the
        // last of them; and their grasp only chills (1d2), so going down while you learn takes a lot
        // of bad luck.
        new()
        {
            Id = "sea-wraith", Name = "Sea-wraith", Creature = "sea-wraith", ArmourClass = 10, HitDice = Dice("3d8"), Speed = 7.5f,
            Scores = new AbilityScores { Strength = 10, Dexterity = 10, Constitution = 10, Intelligence = 6, Wisdom = 10, Charisma = 6 },
            Attacks = new[] { new EnemyAttack("Clammy Grasp", 2, Dice("1d2"), DamageType.Cold, Reach.Melee, WeaponProperty.None) },
            LootTable = "sea-wraith", Xp = 25,
        },

        // A1-C1-01: the Red Gulls shaking Tomas down, and their bosun. Still a brawl with cudgels, but in
        // rows the Soul-Bound can step back behind you, so the thugs take a couple of hits each (2d6+2)
        // and swing a little better (+2), and Grell's hook and cutlass (+5) are what can put you down.
        new()
        {
            Id = "red-gull-thug", Name = "Red Gull Thug", Cast = "red-gull", ArmourClass = 10, HitDice = Dice("2d6+2"), Speed = 9f,
            Scores = new AbilityScores { Strength = 11, Dexterity = 11, Constitution = 11, Intelligence = 9, Wisdom = 9, Charisma = 10 },
            Attacks = new[] { new EnemyAttack("Cudgel", 2, Dice("1d4"), DamageType.Bludgeoning, Reach.Melee, WeaponProperty.None) },
            LootTable = "red-gull-thug", Xp = 25,
        },
        new()
        {
            Id = "bosun-grell", Name = "Bosun Grell", Cast = "grell", ArmourClass = 12, HitDice = Dice("3d8+3"), Speed = 9f,
            Scores = new AbilityScores { Strength = 13, Dexterity = 10, Constitution = 13, Intelligence = 10, Wisdom = 10, Charisma = 12 },
            Attacks = new[]
            {
                new EnemyAttack("Hook Hand", 5, Dice("1d4+1"), DamageType.Piercing, Reach.Melee, WeaponProperty.None),
                new EnemyAttack("Cutlass", 5, Dice("1d6+1"), DamageType.Slashing, Reach.Melee, WeaponProperty.None),
            },
            Abilities = new[] { AbilityId.Multiattack },
            LootTable = "bosun-grell", Xp = 100,
        },

        // A1-C2: the Mirewood's wolves, black smoke leaking from their eyes. A pack against a level-2
        // party: a few hits each (3d6+3) and a real bite (+4, 1d6), with Pack Tactics and Trip.
        new()
        {
            Id = "gloomfang", Name = "Gloomfang Wolf", Creature = "gloomfang", ArmourClass = 11, HitDice = Dice("3d6+3"), Speed = 12f,
            Scores = new AbilityScores { Strength = 10, Dexterity = 14, Constitution = 11, Intelligence = 3, Wisdom = 12, Charisma = 6 },
            Attacks = new[] { new EnemyAttack("Bite", 4, Dice("1d6"), DamageType.Piercing, Reach.Melee, WeaponProperty.None) },
            Abilities = new[] { AbilityId.PackTactics, AbilityId.Trip },
            LootTable = "gloomfang", Xp = 25,
        },

        // A1-C2-02's boss: the she-bear the crystal took, two attacks a turn and thorns through her hide.
        // What makes her a boss is how long she stands and what she does to you while she does: hit
        // points and a thorn-hard hide a round or two past what her attacks alone would need, and a
        // swipe that knocks you flat (Trip), so her second attack comes with advantage and you're
        // getting up instead of hitting back. None of it is more damage in one blow: in rows she bites
        // less surely than she did on the grid (+5), and lasts longer behind a harder hide.
        new()
        {
            Id = "thornmother", Name = "The Thornmother", Creature = "thornmother", ArmourClass = 15, HitDice = Dice("12d10+24"), Speed = 9f,
            Scores = new AbilityScores { Strength = 18, Dexterity = 10, Constitution = 15, Intelligence = 3, Wisdom = 12, Charisma = 7 },
            Attacks = new[]
            {
                new EnemyAttack("Bite", 5, Dice("1d8+3"), DamageType.Piercing, Reach.Melee, WeaponProperty.None),
                new EnemyAttack("Claws", 5, Dice("2d4+3"), DamageType.Slashing, Reach.Melee, WeaponProperty.None),
            },
            Abilities = new[] { AbilityId.Multiattack, AbilityId.ThornBurst, AbilityId.Trip },
            LootTable = "thornmother", Xp = 450, Big = true,
        },

        // A1-C3-02: Doomward's dead, come for Thornbridge. The footmen are the weight of the raid (and
        // the courier's escort in the guard wave): hard to put down (4d8+4) and heavy rusted blades
        // (1d10+1), so they're what the front row has to hold. The hexers shoot from the back.
        new()
        {
            Id = "hollowed-footman", Name = "Hollowed Footman", Creature = "hollowed-footman", ArmourClass = 13, HitDice = Dice("4d8+4"), Speed = 9f,
            Scores = new AbilityScores { Strength = 13, Dexterity = 10, Constitution = 12, Intelligence = 5, Wisdom = 8, Charisma = 5 },
            Attacks = new[] { new EnemyAttack("Rusted Longsword", 3, Dice("1d10+1"), DamageType.Slashing, Reach.Melee, WeaponProperty.None) },
            LootTable = "hollowed-footman", Xp = 50,
        },
        new()
        {
            Id = "hollowed-hexer", Name = "Hollowed Hexer", Creature = "hollowed-hexer", ArmourClass = 11, HitDice = Dice("3d8"), Speed = 9f,
            Scores = new AbilityScores { Strength = 9, Dexterity = 12, Constitution = 10, Intelligence = 13, Wisdom = 11, Charisma = 14 },
            Attacks = new[]
            {
                new EnemyAttack("Ashen Bolt", 3, Dice("1d8"), DamageType.Necrotic, Reach.Ranged, WeaponProperty.Ranged),
                new EnemyAttack("Dagger", 3, Dice("1d4+1"), DamageType.Piercing, Reach.Melee, WeaponProperty.Finesse),
            },
            Abilities = new[] { AbilityId.Hex },
            LootTable = "hollowed-hexer", Xp = 100, Ranged = true,
        },

        // A1-C4-02: the Hollowed who tunnelled into the vault, and the one who leads them. The sappers
        // are diggers, not soldiers (+3 with a pick), so five against four of them isn't a massacre of
        // whoever's in leather; what makes them dangerous is how much digging they've survived (4d8+4)
        // and the blasting powder they throw over the front row (AbilityCatalog's Blasting Charge).
        new()
        {
            Id = "hollowed-sapper", Name = "Hollowed Sapper", Creature = "hollowed-sapper", ArmourClass = 12, HitDice = Dice("4d8+4"), Speed = 9f,
            Scores = new AbilityScores { Strength = 14, Dexterity = 11, Constitution = 13, Intelligence = 6, Wisdom = 8, Charisma = 5 },
            Attacks = new[] { new EnemyAttack("Pick", 3, Dice("1d8+2"), DamageType.Piercing, Reach.Melee, WeaponProperty.None) },
            Abilities = new[] { AbilityId.BlastingCharge },
            LootTable = "hollowed-sapper", Xp = 50,
        },
        new()
        {
            Id = "sapper-chief", Name = "Sapper Chief", Creature = "hollowed-sapper", ArmourClass = 14, HitDice = Dice("7d8+14"), Speed = 9f,
            Scores = new AbilityScores { Strength = 16, Dexterity = 11, Constitution = 14, Intelligence = 8, Wisdom = 10, Charisma = 8 },
            Attacks = new[]
            {
                new EnemyAttack("Pick", 4, Dice("1d8+3"), DamageType.Piercing, Reach.Melee, WeaponProperty.None),
                new EnemyAttack("Lantern", 5, Dice("1d6+3"), DamageType.Fire, Reach.Melee, WeaponProperty.None),
            },
            Abilities = new[] { AbilityId.Multiattack, AbilityId.BlastingCharge },
            LootTable = "sapper-chief", Xp = 200,
        },

        // The vault's guardians: stone and gold, slow, and very hard to put down.
        new()
        {
            Id = "vault-construct", Name = "Vault Construct", Creature = "vault-construct", ArmourClass = 15, HitDice = Dice("5d10+10"), Speed = 6f,
            Scores = new AbilityScores { Strength = 17, Dexterity = 8, Constitution = 15, Intelligence = 3, Wisdom = 10, Charisma = 1 },
            Attacks = new[] { new EnemyAttack("Gilded Slam", 5, Dice("1d8+3"), DamageType.Bludgeoning, Reach.Melee, WeaponProperty.None) },
            Abilities = new[] { AbilityId.Trip },
            LootTable = "vault-construct", Xp = 200,
        },

        // A1-C4-02's duel: Kael, quick and slippery, a rapier, a dagger and a feint a turn, smoke and
        // decoys. He gives up when he's hurt enough (EncounterCatalog's YieldsAt), so his hit points
        // are only how long the duel lasts, and no body's searched. One duellist against four in rows
        // only ever reaches whoever holds the front, so he's the best blade in Act 1 (+8, a 2d8+3
        // rapier) and lasts (16d8+16) until he's cut through your shield and can reach the rest.
        new()
        {
            Id = "kael-duel", Name = "Kael", Cast = "kael", ArmourClass = 15, HitDice = Dice("16d8+16"), Speed = 10.5f,
            Scores = new AbilityScores { Strength = 10, Dexterity = 17, Constitution = 12, Intelligence = 13, Wisdom = 12, Charisma = 15 },
            Attacks = new[]
            {
                new EnemyAttack("Rapier", 8, Dice("2d8+3"), DamageType.Piercing, Reach.Melee, WeaponProperty.Finesse),
                new EnemyAttack("Off-hand Dagger", 8, Dice("1d6+3"), DamageType.Piercing, Reach.Melee, WeaponProperty.Finesse),
                new EnemyAttack("Feint and Stab", 8, Dice("1d6+3"), DamageType.Piercing, Reach.Melee, WeaponProperty.Finesse),
            },
            Abilities = new[] { AbilityId.Multiattack, AbilityId.SneakAttack, AbilityId.SmokeBomb, AbilityId.CunningDisengage },
            Xp = 200,
        },
    };

    private static readonly Dictionary<string, EnemyDefinition> ById = new();

    /// <summary>Indexes the table and checks every stat block.</summary>
    /// <remarks>Throws on purpose, so a stat block that can't fight, or that names a look or an ability that isn't there, shows up when the game starts.</remarks>
    static EnemyCatalog()
    {
        foreach (EnemyDefinition row in Table)
        {
            Check(row);
            if (!ById.TryAdd(row.Id, row)) throw new InvalidOperationException($"Two enemies are called {row.Id}.");
        }
    }

    /// <summary>Every stat block, in the table's order.</summary>
    public static IEnumerable<EnemyDefinition> All => Table;

    /// <summary>Whether there's an enemy with this id.</summary>
    /// <param name="id">Which.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One stat block.</summary>
    /// <param name="id">Which. It has to exist.</param>
    public static EnemyDefinition Get(string id) => ById[id];

    /// <summary>Shorthand for the table.</summary>
    private static DiceExpression Dice(string text) => DiceExpression.Parse(text);

    /// <summary>Checks one stat block, and throws with what's wrong if it can't fight.</summary>
    private static void Check(EnemyDefinition row)
    {
        string name = $"Enemy {row.Id}";
        if (string.IsNullOrWhiteSpace(row.Name)) throw new InvalidOperationException($"{name} has no name.");
        if ((row.Creature is null) == (row.Cast is null)) throw new InvalidOperationException($"{name} needs a creature or a cast member to look like, and only one.");
        if (row.Creature is not null && Creatures.Get(row.Creature) is null) throw new InvalidOperationException($"{name} looks like creature {row.Creature}, which isn't in Creatures.");
        if (row.Cast is not null && Cast.Get(row.Cast) is null) throw new InvalidOperationException($"{name} looks like {row.Cast}, who isn't in the Cast.");
        if (row.ArmourClass < 1 || row.HitDice.Average < 1 || row.Speed <= 0f || row.Xp < 0)
            throw new InvalidOperationException($"{name} needs an AC, hit dice and speed above nothing, and no negative XP.");
        if (row.Attacks.Count == 0) throw new InvalidOperationException($"{name} has no attacks.");
        if (row.LootTable is not null && !LootTableCatalog.Exists(row.LootTable))
            throw new InvalidOperationException($"{name} drops loot table {row.LootTable}, which isn't in LootTableCatalog.");

        bool reaches = false;
        foreach (EnemyAttack attack in row.Attacks)
        {
            if (string.IsNullOrWhiteSpace(attack.Name) || attack.Damage.IsNone || attack.Reach == Reach.Weapon)
                throw new InvalidOperationException($"{name} has an attack with no name, damage or reach of its own.");
            reaches |= attack.Reach is Reach.Ranged or Reach.AnyRow;
        }
        if (row.Ranged && !reaches) throw new InvalidOperationException($"{name} fights from range with nothing that reaches past melee.");

        foreach (AbilityId ability in row.Abilities)
            if (AbilityCatalog.IsCommon(ability)) throw new InvalidOperationException($"{name} lists {ability}, which every foe has already.");
    }
}
