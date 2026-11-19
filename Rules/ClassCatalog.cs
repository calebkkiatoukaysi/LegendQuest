using System;
using System.Collections.Generic;
using LegendQuest.Items;

namespace LegendQuest.Rules;

/// <summary>
/// Every class, by ClassId.
/// </summary>
/// <remarks>
/// A static table keyed by the enum, like The Black Box's ItemCatalog. The armour, shields, martial
/// weapons and casting abilities are 5e's, from the SRD 5.1. Hit points aren't: they come from level
/// and Constitution, with each class's modifier on top, 10-classes.md §1.4's (+20% for the Warden,
/// +10% for the Blademaster, -10% for the Arcanist, nothing for the rest). One simplification:
/// every class here is trained with every simple weapon, where the SRD gives wizards and druids a
/// shorter list, so Sera's shortbow is hers to use. The Hexblade fights with a pact blade, so it's
/// trained with the shortsword in its kit. The names are the story's own; the kits are the team's,
/// each class's weapon, shield and armour plus what everyone sets out in.
///
/// The eight base classes come first, then 10-classes.md §3's sixteen advanced classes and the
/// Hourwarden, each trained in what its teacher fights with, out of the items there are (a cutlass is
/// a scimitar, a wand or a censer a quarterstaff, until they're items of their own). Each has its
/// Focus model (11-combat.md §4.1), its threat (the three defenders make double) and its two saves; an
/// advanced class's two are the ones its role leans on.
/// </remarks>
public static class ClassCatalog
{
    /// <summary>What everyone sets out in, whatever their class: a cloak against the weather and boots for the road.</summary>
    /// <remarks>Before Table, since static fields are set in the order they're written and Table's kits read this.</remarks>
    private static readonly string[] Basics = { "travelling-cloak", "leather-boots" };

    /// <summary>The most a class can change hit points by, either way, in percent.</summary>
    private const int MostHpModifier = 50;

    /// <summary>The threat a defender makes, in percent: double (11-combat.md §5.1).</summary>
    private const int DefenderThreat = 200;

    /// <summary>The threat a class that fights from the shadows makes, in percent: the Shadowstep's, so a foe it cuts doesn't always know where the cut came from.</summary>
    private const int ShadowThreat = 60;

    /// <summary>The Lanerunner's: a street fighter who's gone before a foe turns, if not quite a shadow.</summary>
    private const int SlipperyThreat = 85;

    private static readonly ClassDefinition[] Table =
    {
        new()
        {
            Id = ClassId.Fighter, Code = "fighter", Name = "Blademaster", Role = "Frontline weapon specialist.", HpModifier = 10,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 13, Constitution = 14, Intelligence = 8, Wisdom = 12, Charisma = 10 },
            ArmourUpTo = ArmourCategory.Heavy, Shields = true, MartialWeapons = true,
            StartingKit = Kit("longsword", "wooden-shield", "chain-mail"),
            Focus = FocusModel.Momentum, Saves = new[] { Ability.Strength, Ability.Constitution }, InterimKit = ClassId.Fighter,
        },
        new()
        {
            Id = ClassId.Rogue, Code = "rogue", Name = "Shadowstep", Role = "Stealth, crits and lockpicking.",
            Recommended = new AbilityScores { Strength = 8, Dexterity = 15, Constitution = 14, Intelligence = 12, Wisdom = 10, Charisma = 13 },
            ArmourUpTo = ArmourCategory.Light, WeaponsAlso = new[] { "longsword", "rapier", "shortsword" },
            StartingKit = Kit("shortsword", "dagger", "leather-armour"),
            Focus = FocusModel.Opportunist, ThreatPct = ShadowThreat, Saves = new[] { Ability.Dexterity, Ability.Intelligence }, InterimKit = ClassId.Rogue,
        },
        new()
        {
            Id = ClassId.Cleric, Code = "cleric", Name = "Lightbearer", Role = "Support and holy damage.",
            Recommended = new AbilityScores { Strength = 13, Dexterity = 10, Constitution = 14, Intelligence = 8, Wisdom = 15, Charisma = 12 },
            ArmourUpTo = ArmourCategory.Medium, Shields = true, CastingAbility = Ability.Wisdom,
            StartingKit = Kit("mace", "wooden-shield", "scale-mail"),
            Focus = FocusModel.Devotion, Saves = new[] { Ability.Wisdom, Ability.Charisma }, InterimKit = ClassId.Cleric,
        },
        new()
        {
            Id = ClassId.Wizard, Code = "wizard", Name = "Arcanist", Role = "Studied spellcaster, burst damage.",
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 15, Wisdom = 12, Charisma = 10 },
            ArmourUpTo = ArmourCategory.None, CastingAbility = Ability.Intelligence,
            StartingKit = Kit("quarterstaff", "mage-robes"),
            Focus = FocusModel.Wellspring, Saves = new[] { Ability.Intelligence, Ability.Wisdom }, InterimKit = ClassId.Wizard,
        },
        new()
        {
            Id = ClassId.Paladin, Code = "paladin", Name = "Warden", Role = "Oath-bound tank and healer.", HpModifier = 20,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 10, Constitution = 13, Intelligence = 8, Wisdom = 12, Charisma = 14 },
            ArmourUpTo = ArmourCategory.Heavy, Shields = true, MartialWeapons = true, CastingAbility = Ability.Charisma,
            StartingKit = Kit("longsword", "steel-shield", "chain-mail"),
            Focus = FocusModel.Bulwark, ThreatPct = DefenderThreat, Saves = new[] { Ability.Wisdom, Ability.Charisma }, InterimKit = ClassId.Paladin,
        },
        new()
        {
            Id = ClassId.Druid, Code = "druid", Name = "Wildcaller", Role = "Nature magic, a beast companion.",
            Recommended = new AbilityScores { Strength = 10, Dexterity = 13, Constitution = 14, Intelligence = 12, Wisdom = 15, Charisma = 8 },
            ArmourUpTo = ArmourCategory.Medium, Shields = true, WeaponsAlso = new[] { "scimitar" }, CastingAbility = Ability.Wisdom,
            StartingKit = Kit("quarterstaff", "hide-armour", "wooden-shield"),
            Focus = FocusModel.Wild, Saves = new[] { Ability.Intelligence, Ability.Wisdom }, InterimKit = ClassId.Druid,
        },
        new()
        {
            Id = ClassId.Warlock, Code = "warlock", Name = "Hexblade", Role = "Pact magic that drains life.",
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 10, Wisdom = 12, Charisma = 15 },
            ArmourUpTo = ArmourCategory.Light, WeaponsAlso = new[] { "shortsword" }, CastingAbility = Ability.Charisma,
            StartingKit = Kit("shortsword", "leather-armour"),
            Focus = FocusModel.Pact, Saves = new[] { Ability.Wisdom, Ability.Charisma }, InterimKit = ClassId.Warlock,
        },
        new()
        {
            Id = ClassId.Bard, Code = "bard", Name = "Skald", Role = "Buffs, debuffs and persuasion.",
            Recommended = new AbilityScores { Strength = 8, Dexterity = 14, Constitution = 13, Intelligence = 10, Wisdom = 12, Charisma = 15 },
            ArmourUpTo = ArmourCategory.Light, WeaponsAlso = new[] { "longsword", "rapier", "shortsword" }, CastingAbility = Ability.Charisma,
            StartingKit = Kit("rapier", "leather-armour"),
            Focus = FocusModel.Tempo, Saves = new[] { Ability.Dexterity, Ability.Charisma }, InterimKit = ClassId.Bard,
        },

        // The advanced classes (10-classes.md §3), in the table's order: Act 1's, Act 2's, Act 3's, then the Hourwarden.
        new()
        {
            Id = ClassId.Watchblade, Code = "watchblade", Name = "Watchblade", Role = "Defender and controller.", HpModifier = 20, Advanced = true,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 13, Constitution = 14, Intelligence = 8, Wisdom = 12, Charisma = 10 },
            ArmourUpTo = ArmourCategory.Medium, Shields = true, WeaponsAlso = new[] { "longsword", "shortsword" },
            StartingKit = Kit("longsword", "wooden-shield", "scale-mail"),
            Focus = FocusModel.Momentum, ThreatPct = DefenderThreat, Saves = new[] { Ability.Strength, Ability.Constitution }, InterimKit = ClassId.Fighter,
        },
        new()
        {
            Id = ClassId.Netcaster, Code = "netcaster", Name = "Netcaster", Role = "Support and control from the back.", Advanced = true,
            Recommended = new AbilityScores { Strength = 10, Dexterity = 14, Constitution = 13, Intelligence = 8, Wisdom = 15, Charisma = 12 },
            ArmourUpTo = ArmourCategory.Light, CastingAbility = Ability.Wisdom,
            StartingKit = Kit("spear", "leather-armour"),
            Focus = FocusModel.Wild, Saves = new[] { Ability.Dexterity, Ability.Wisdom }, InterimKit = ClassId.Druid,
        },
        new()
        {
            Id = ClassId.Deckhand, Code = "deckhand", Name = "Deckhand", Role = "Skirmisher with a cutlass.", HpModifier = 10, Advanced = true,
            Recommended = new AbilityScores { Strength = 10, Dexterity = 15, Constitution = 14, Intelligence = 8, Wisdom = 12, Charisma = 13 },
            ArmourUpTo = ArmourCategory.Light, WeaponsAlso = new[] { "scimitar", "shortsword" },
            StartingKit = Kit("scimitar", "leather-armour"),
            Focus = FocusModel.Momentum, Saves = new[] { Ability.Dexterity, Ability.Charisma }, InterimKit = ClassId.Rogue,
        },
        new()
        {
            Id = ClassId.LodgeRanger, Code = "lodge-ranger", Name = "Lodge Ranger", Role = "Ranged damage with a hawk.", Advanced = true,
            Recommended = new AbilityScores { Strength = 10, Dexterity = 15, Constitution = 13, Intelligence = 8, Wisdom = 14, Charisma = 12 },
            ArmourUpTo = ArmourCategory.Medium, WeaponsAlso = new[] { "longbow", "shortsword" }, CastingAbility = Ability.Wisdom,
            StartingKit = Kit("longbow", "leather-armour"),
            Focus = FocusModel.Opportunist, Saves = new[] { Ability.Strength, Ability.Dexterity }, InterimKit = ClassId.Rogue,
        },
        new()
        {
            Id = ClassId.Battlemage, Code = "battlemage", Name = "Guild Battlemage", Role = "Battlefield caster.", HpModifier = -10, Advanced = true,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 15, Wisdom = 12, Charisma = 10 },
            ArmourUpTo = ArmourCategory.Light, CastingAbility = Ability.Intelligence,
            StartingKit = Kit("quarterstaff", "leather-armour"),
            Focus = FocusModel.Wellspring, Saves = new[] { Ability.Intelligence, Ability.Wisdom }, InterimKit = ClassId.Wizard,
        },
        new()
        {
            Id = ClassId.Ashwalker, Code = "ashwalker", Name = "Ashwalker", Role = "Bruiser who fights in ash.", HpModifier = 10, Advanced = true,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 12, Constitution = 14, Intelligence = 8, Wisdom = 13, Charisma = 10 },
            ArmourUpTo = ArmourCategory.Medium, WeaponsAlso = new[] { "greatsword", "battleaxe" },
            StartingKit = Kit("greatsword", "hide-armour"),
            Focus = FocusModel.Momentum, Saves = new[] { Ability.Strength, Ability.Constitution }, InterimKit = ClassId.Fighter,
        },
        new()
        {
            Id = ClassId.Bulwark, Code = "bulwark", Name = "Ironvein Bulwark", Role = "The best defender in the game.", HpModifier = 20, Advanced = true,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 10, Constitution = 14, Intelligence = 8, Wisdom = 13, Charisma = 12 },
            ArmourUpTo = ArmourCategory.Heavy, Shields = true, WeaponsAlso = new[] { "warhammer", "battleaxe" },
            StartingKit = Kit("warhammer", "steel-shield", "chain-mail"),
            Focus = FocusModel.Bulwark, ThreatPct = DefenderThreat, Saves = new[] { Ability.Strength, Ability.Constitution }, InterimKit = ClassId.Fighter,
        },
        new()
        {
            Id = ClassId.EmberMender, Code = "ember-mender", Name = "Ember Mender", Role = "Healer who cauterises.", Advanced = true,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 12, Constitution = 14, Intelligence = 15, Wisdom = 13, Charisma = 10 },
            ArmourUpTo = ArmourCategory.Light, CastingAbility = Ability.Intelligence,
            StartingKit = Kit("quarterstaff", "leather-armour"),
            Focus = FocusModel.Devotion, Saves = new[] { Ability.Intelligence, Ability.Wisdom }, InterimKit = ClassId.Cleric,
        },
        new()
        {
            Id = ClassId.Lanerunner, Code = "lanerunner", Name = "Lanerunner", Role = "Thief with twin blades.", Advanced = true,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 15, Constitution = 13, Intelligence = 12, Wisdom = 10, Charisma = 14 },
            ArmourUpTo = ArmourCategory.Light, WeaponsAlso = new[] { "shortsword" },
            StartingKit = Kit("dagger", "dagger", "leather-armour"),
            Focus = FocusModel.Opportunist, ThreatPct = SlipperyThreat, Saves = new[] { Ability.Dexterity, Ability.Intelligence }, InterimKit = ClassId.Rogue,
        },
        new()
        {
            Id = ClassId.Wayfarer, Code = "wayfarer", Name = "Wayfarer", Role = "Jack of all trades.", HpModifier = 10, Advanced = true,
            Recommended = new AbilityScores { Strength = 13, Dexterity = 15, Constitution = 14, Intelligence = 10, Wisdom = 12, Charisma = 8 },
            ArmourUpTo = ArmourCategory.Light,
            StartingKit = Kit("quarterstaff", "leather-armour"),
            Focus = FocusModel.Tempo, Saves = new[] { Ability.Dexterity, Ability.Constitution }, InterimKit = ClassId.Fighter,
        },
        new()
        {
            Id = ClassId.GildedKnight, Code = "gilded-knight", Name = "Gilded Knight", Role = "Sun paladin.", HpModifier = 10, Advanced = true,
            Recommended = new AbilityScores { Strength = 15, Dexterity = 10, Constitution = 14, Intelligence = 8, Wisdom = 12, Charisma = 13 },
            ArmourUpTo = ArmourCategory.Heavy, Shields = true, WeaponsAlso = new[] { "longsword" }, CastingAbility = Ability.Charisma,
            StartingKit = Kit("longsword", "steel-shield", "chain-mail"),
            Focus = FocusModel.Bulwark, Saves = new[] { Ability.Wisdom, Ability.Charisma }, InterimKit = ClassId.Paladin,
        },
        new()
        {
            Id = ClassId.Hemomancer, Code = "hemomancer", Name = "Hemomancer", Role = "Blood mage, paid in hit points.", HpModifier = -10, Advanced = true,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 12, Wisdom = 10, Charisma = 15 },
            ArmourUpTo = ArmourCategory.Light, CastingAbility = Ability.Charisma,
            StartingKit = Kit("dagger", "leather-armour"),
            Focus = FocusModel.Blood, Saves = new[] { Ability.Constitution, Ability.Charisma }, InterimKit = ClassId.Warlock,
        },
        new()
        {
            Id = ClassId.Hivekeeper, Code = "hivekeeper", Name = "Hivekeeper", Role = "Healer with a swarm.", Advanced = true,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 12, Constitution = 14, Intelligence = 10, Wisdom = 15, Charisma = 13 },
            ArmourUpTo = ArmourCategory.Light, CastingAbility = Ability.Wisdom,
            StartingKit = Kit("quarterstaff", "leather-armour"),
            Focus = FocusModel.Devotion, Saves = new[] { Ability.Constitution, Ability.Wisdom }, InterimKit = ClassId.Cleric,
        },
        new()
        {
            Id = ClassId.Glasswright, Code = "glasswright", Name = "Glasswright", Role = "Illusionist of light.", HpModifier = -10, Advanced = true,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 14, Constitution = 13, Intelligence = 15, Wisdom = 10, Charisma = 12 },
            ArmourUpTo = ArmourCategory.None, CastingAbility = Ability.Intelligence,
            StartingKit = Kit("quarterstaff", "mage-robes"),
            Focus = FocusModel.Wellspring, Saves = new[] { Ability.Intelligence, Ability.Charisma }, InterimKit = ClassId.Wizard,
        },
        new()
        {
            Id = ClassId.Lamplighter, Code = "lamplighter", Name = "Lamplighter", Role = "Protector and healer.", Advanced = true,
            Recommended = new AbilityScores { Strength = 13, Dexterity = 10, Constitution = 14, Intelligence = 8, Wisdom = 15, Charisma = 12 },
            ArmourUpTo = ArmourCategory.Medium, CastingAbility = Ability.Wisdom,
            StartingKit = Kit("mace", "scale-mail"),
            Focus = FocusModel.Devotion, Saves = new[] { Ability.Wisdom, Ability.Charisma }, InterimKit = ClassId.Cleric,
        },
        new()
        {
            Id = ClassId.Crownsworn, Code = "crownsworn", Name = "Crownsworn", Role = "Commander and duellist.", HpModifier = 10, Advanced = true,
            Recommended = new AbilityScores { Strength = 10, Dexterity = 15, Constitution = 13, Intelligence = 12, Wisdom = 8, Charisma = 14 },
            ArmourUpTo = ArmourCategory.Medium, WeaponsAlso = new[] { "longsword", "rapier" },
            StartingKit = Kit("rapier", "scale-mail"),
            Focus = FocusModel.Momentum, Saves = new[] { Ability.Dexterity, Ability.Charisma }, InterimKit = ClassId.Fighter,
        },
        new()
        {
            Id = ClassId.Hourwarden, Code = "hourwarden", Name = "Hourwarden", Role = "Time mage.", Advanced = true,
            Recommended = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 15, Wisdom = 12, Charisma = 10 },
            ArmourUpTo = ArmourCategory.None, CastingAbility = Ability.Intelligence,
            StartingKit = Kit("quarterstaff", "mage-robes"),
            Focus = FocusModel.Wellspring, Saves = new[] { Ability.Intelligence, Ability.Wisdom }, InterimKit = ClassId.Wizard,
        },
    };

    private static readonly Dictionary<ClassId, ClassDefinition> ById = new();
    private static readonly Dictionary<string, ClassDefinition> ByCodeId = new(StringComparer.Ordinal);

    /// <summary>Indexes the table, and checks every class has exactly one row and a kit it can wear.</summary>
    /// <remarks>Throws on purpose, so a class added to the enum and not here, or a kit with a typo in it, shows up when the game starts.</remarks>
    static ClassCatalog()
    {
        foreach (ClassDefinition row in Table)
        {
            if (row.HpModifier is < -MostHpModifier or > MostHpModifier) throw new InvalidOperationException($"Class {row.Id} changes hit points by {row.HpModifier}%.");
            if (row.Speed < 1) throw new InvalidOperationException($"Class {row.Id} moves {row.Speed} m a turn.");
            if (string.IsNullOrWhiteSpace(row.Code) || !ByCodeId.TryAdd(row.Code, row)) throw new InvalidOperationException($"Class {row.Id} has no code id, or one another class has.");
            if (row.Saves.Count != 2 || row.Saves[0] == row.Saves[1]) throw new InvalidOperationException($"Class {row.Id} needs two different saving throws.");
            if (row.ThreatPct < 1) throw new InvalidOperationException($"Class {row.Id} makes no threat.");
            if (row.Advanced == ((int)row.Id < BaseClasses)) throw new InvalidOperationException($"Class {row.Id} is advanced if and only if it comes after the eight base classes.");
            if ((int)row.InterimKit >= BaseClasses || !row.Advanced && row.InterimKit != row.Id)
                throw new InvalidOperationException($"Class {row.Id} borrows the interim kit of {row.InterimKit}, which isn't a base class (or isn't its own).");

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

    /// <summary>How many base classes there are: the first of the enum, the ones a character can start as.</summary>
    public const int BaseClasses = 8;

    /// <summary>Every class, base classes first, in the enum's order.</summary>
    public static IReadOnlyList<ClassDefinition> All => Table;

    /// <summary>One class.</summary>
    /// <param name="id">Which.</param>
    public static ClassDefinition Get(ClassId id) => ById[id];

    /// <summary>A class by its code id (fighter, lodge-ranger), or null.</summary>
    /// <param name="code">The code id.</param>
    public static ClassDefinition ByCode(string code) => code is not null && ByCodeId.TryGetValue(code, out ClassDefinition row) ? row : null;

    /// <summary>A kit: these items, then what everyone sets out in.</summary>
    /// <param name="own">The item ids that are this kit's own, in the order they're put on.</param>
    public static IReadOnlyList<string> Kit(params string[] own)
    {
        var kit = new List<string>(own);
        kit.AddRange(Basics);
        return kit;
    }
}
