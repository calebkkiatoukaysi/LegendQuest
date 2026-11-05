using System;
using System.Collections.Generic;
using LegendQuest.Combat;
using LegendQuest.Rules;

namespace LegendQuest.Items;

/// <summary>
/// Every item there is: the weapons, the armour, the shields and the things worn with them, the
/// potions, and what's picked up to sell.
/// </summary>
/// <remarks>
/// A static table, like The Black Box's ItemCatalog, in the order the icons are drawn in, so an
/// item's Icon is its place here. The weapons and armour are the SRD 5.1's tables, damage, ranges,
/// properties, AC, Dexterity caps, Strength and gold included (a quarterstaff's 2 sp rounds up to a
/// gold piece). The magic ones are the team's, priced inside the DMG's ranges for their rarity. The
/// static constructor checks every row against its kind and throws, so a typo shows up when the game
/// starts and not in a fight.
/// </remarks>
public static class ItemCatalog
{
    /// <summary>The potion the party starts with two of.</summary>
    public const string HealingPotion = "healing-potion";

    /// <summary>What a shield adds to AC, the SRD's +2. A magic one adds more.</summary>
    private const int ShieldAc = 2;

    /// <summary>The Focus an elixir of focus gives back in a fight.</summary>
    private const int ElixirFocus = 30;

    /// <summary>A tile's width, in metres.</summary>
    private const float Tile = 1.5f;

    /// <summary>How far a melee weapon reaches: the next tile.</summary>
    private const float Melee = Tile;

    /// <summary>How far a Reach weapon reaches: two tiles.</summary>
    private const float LongReach = 2 * Tile;

    /// <summary>How far something thrown goes at its normal range: the SRD's 20 feet.</summary>
    private const float Throw = 6f;

    /// <summary>A shortbow's or a light crossbow's normal range: the SRD's 80 feet.</summary>
    private const float ShortRange = 24f;

    /// <summary>A longbow's normal range: the SRD's 150 feet.</summary>
    private const float LongRange = 45f;

    /// <summary>The most Dexterity medium armour lets count.</summary>
    private const int MediumDex = 2;

    /// <summary>The most Dexterity heavy armour lets count: none.</summary>
    private const int HeavyDex = 0;

    /// <summary>The first character the game's fonts can draw. Anything outside them shows as a "?".</summary>
    private const char FirstPrintable = ' ';

    /// <summary>The last character the game's fonts can draw.</summary>
    private const char LastPrintable = '~';

    private static readonly ItemDefinition[] Table =
    {
        // Weapons: the simple and martial ones out of the SRD's table, then the magic ones.
        new()
        {
            Id = "dagger", Name = "Dagger", Kind = ItemKind.Weapon, Icon = 0, Layer = "weapon-dagger", Value = 2,
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d4"), DamageType = DamageType.Piercing,
            Properties = WeaponProperty.Finesse | WeaponProperty.Light | WeaponProperty.Thrown, Range = Melee, ThrowRange = Throw,
            Description = "A short, sharp blade that fits a boot, a sleeve or a palm. Quick in the hand, and true when it's thrown.",
        },
        new()
        {
            Id = "shortsword", Name = "Shortsword", Kind = ItemKind.Weapon, Icon = 1, Layer = "weapon-shortsword", Value = 10,
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("1d6"), DamageType = DamageType.Piercing,
            Properties = WeaponProperty.Finesse | WeaponProperty.Light, Range = Melee,
            Description = "A straight blade a forearm long, light enough to swing all day. The Watch carries them, and so do the people the Watch is after.",
        },
        new()
        {
            Id = "longsword", Name = "Longsword", Kind = ItemKind.Weapon, Icon = 2, Layer = "weapon-longsword", Value = 15,
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("1d8"), VersatileDamage = DiceExpression.Parse("1d10"),
            DamageType = DamageType.Slashing, Properties = WeaponProperty.Versatile, Range = Melee,
            Description = "A knight's blade with a long grip, for one hand or two. Every hero in every song Lark knows carries one.",
        },
        new()
        {
            Id = "rapier", Name = "Rapier", Kind = ItemKind.Weapon, Icon = 3, Layer = "weapon-rapier", Value = 25,
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Piercing,
            Properties = WeaponProperty.Finesse, Range = Melee,
            Description = "A slender duelling blade from the Sunspire's fencing yards. It wins on speed, not weight.",
        },
        new()
        {
            Id = "scimitar", Name = "Scimitar", Kind = ItemKind.Weapon, Icon = 4, Layer = "weapon-scimitar", Value = 25,
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("1d6"), DamageType = DamageType.Slashing,
            Properties = WeaponProperty.Finesse | WeaponProperty.Light, Range = Melee,
            Description = "A curved blade from the southern isles, wicked on the draw. Sailors swear by it, and at it.",
        },
        new()
        {
            Id = "handaxe", Name = "Handaxe", Kind = ItemKind.Weapon, Icon = 5, Layer = "weapon-axe", Value = 5,
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d6"), DamageType = DamageType.Slashing,
            Properties = WeaponProperty.Light | WeaponProperty.Thrown, Range = Melee, ThrowRange = Throw,
            Description = "A woodcutter's hatchet, balanced for the hand. Good for kindling; better thrown.",
        },
        new()
        {
            Id = "battleaxe", Name = "Battleaxe", Kind = ItemKind.Weapon, Icon = 6, Layer = "weapon-axe", Value = 10,
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("1d8"), VersatileDamage = DiceExpression.Parse("1d10"),
            DamageType = DamageType.Slashing, Properties = WeaponProperty.Versatile, Range = Melee,
            Description = "A broad, bearded axe head on an ash haft. One hand or two, it settles arguments.",
        },
        new()
        {
            Id = "greatsword", Name = "Greatsword", Kind = ItemKind.Weapon, Icon = 7, Layer = "weapon-greatsword", Value = 50,
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("2d6"), DamageType = DamageType.Slashing,
            Properties = WeaponProperty.Heavy | WeaponProperty.TwoHanded, Range = Melee,
            Description = "A blade as tall as its wielder that wants both hands and all of your back. Whatever it hits stays hit.",
        },
        new()
        {
            Id = "warhammer", Name = "Warhammer", Kind = ItemKind.Weapon, Icon = 8, Layer = "weapon-hammer", Value = 15,
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("1d8"), VersatileDamage = DiceExpression.Parse("1d10"),
            DamageType = DamageType.Bludgeoning, Properties = WeaponProperty.Versatile, Range = Melee,
            Description = "A square iron head on a steel-bound haft, made for the dents armour can't shrug off.",
        },
        new()
        {
            Id = "mace", Name = "Mace", Kind = ItemKind.Weapon, Icon = 9, Layer = "weapon-mace", Value = 5,
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d6"), DamageType = DamageType.Bludgeoning,
            Range = Melee,
            Description = "A flanged iron head on a short handle. Lightbearers favour it: it doesn't care what it's hitting.",
        },
        new()
        {
            Id = "quarterstaff", Name = "Quarterstaff", Kind = ItemKind.Weapon, Icon = 10, Layer = "weapon-staff", Value = 1,
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d6"), VersatileDamage = DiceExpression.Parse("1d8"),
            DamageType = DamageType.Bludgeoning, Properties = WeaponProperty.Versatile, Range = Melee,
            Description = "Seasoned oak, shod with iron at both ends. A walking stick, until it isn't.",
        },
        new()
        {
            Id = "spear", Name = "Spear", Kind = ItemKind.Weapon, Icon = 11, Layer = "weapon-spear", Value = 1,
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d6"), VersatileDamage = DiceExpression.Parse("1d8"),
            DamageType = DamageType.Piercing, Properties = WeaponProperty.Thrown | WeaponProperty.Versatile, Range = Melee, ThrowRange = Throw,
            Description = "An ash shaft with a leaf-shaped head. It keeps trouble at arm's length, or further if you throw it.",
        },
        new()
        {
            Id = "shortbow", Name = "Shortbow", Kind = ItemKind.Weapon, Icon = 12, Layer = "weapon-bow", Value = 25,
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d6"), DamageType = DamageType.Piercing,
            Properties = WeaponProperty.Ranged | WeaponProperty.TwoHanded, Range = ShortRange,
            Description = "A curved yew bow, short enough to draw among the trees. A Mirewood ranger's first friend.",
        },
        new()
        {
            Id = "longbow", Name = "Longbow", Kind = ItemKind.Weapon, Icon = 13, Layer = "weapon-bow", Value = 50,
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Piercing,
            Properties = WeaponProperty.Ranged | WeaponProperty.Heavy | WeaponProperty.TwoHanded, Range = LongRange,
            Description = "A tall yew bow with a heavy draw. It reaches across a field and through a gambeson.",
        },
        new()
        {
            Id = "light-crossbow", Name = "Light Crossbow", Kind = ItemKind.Weapon, Icon = 14, Layer = "weapon-crossbow", Value = 25,
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Piercing,
            Properties = WeaponProperty.Ranged | WeaponProperty.Loading | WeaponProperty.TwoHanded, Range = ShortRange,
            Description = "A stock, a steel bow and a crank. Slow to load, easy to aim, and it doesn't miss by much.",
        },
        new()
        {
            Id = "ember-dagger", Name = "Ember Dagger", Kind = ItemKind.Weapon, Icon = 15, Layer = "weapon-dagger", Value = 250,
            Rarity = Rarity.Uncommon, Base = "dagger",
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d4"), DamageType = DamageType.Piercing,
            Properties = WeaponProperty.Finesse | WeaponProperty.Light | WeaponProperty.Thrown, Range = Melee, ThrowRange = Throw,
            AttackBonus = 1, DamageBonus = 1, ExtraDamage = DiceExpression.Parse("1d4"), ExtraDamageType = DamageType.Fire,
            Description = "An Emberkin blade that never quite cools: its edge glows like a coal in the dark. It burns what it cuts.",
        },
        new()
        {
            Id = "thornwood-bow", Name = "Thornwood Bow", Kind = ItemKind.Weapon, Icon = 16, Layer = "weapon-bow", Value = 300,
            Rarity = Rarity.Uncommon, Base = "shortbow",
            Category = WeaponCategory.Simple, Damage = DiceExpression.Parse("1d6"), DamageType = DamageType.Piercing,
            Properties = WeaponProperty.Ranged | WeaponProperty.TwoHanded, Range = ShortRange,
            AttackBonus = 1, DamageBonus = 1,
            Description = "Cut from the briar that grew through the Thornmother's den, with the rot burned out of it. Its string hums when you nock an arrow.",
        },
        new()
        {
            Id = "sunsteel-blade", Name = "Sunsteel Blade", Kind = ItemKind.Weapon, Icon = 17, Layer = "weapon-longsword", Value = 600,
            Rarity = Rarity.Rare, Base = "longsword",
            Category = WeaponCategory.Martial, Damage = DiceExpression.Parse("1d8"), VersatileDamage = DiceExpression.Parse("1d10"),
            DamageType = DamageType.Slashing, Properties = WeaponProperty.Versatile, Range = Melee,
            AttackBonus = 1, DamageBonus = 1, ExtraDamage = DiceExpression.Parse("1d4"), ExtraDamageType = DamageType.Radiant,
            Description = "A longsword from the Old Crown's armoury, its steel folded with captured sunlight. The darker the room, the brighter it shines.",
        },

        // Body armour, lightest to heaviest, then the robes. Clothes are AC 10 like nothing at all, and anyone can wear them.
        new()
        {
            Id = "travelling-clothes", Name = "Travelling Clothes", Kind = ItemKind.Armour, Icon = 18, Value = 2,
            Armour = ArmourCategory.None, BaseAc = ArmourClass.Unarmoured,
            Description = "Wool, linen and a good belt: what anyone wears on the road. They keep off the weather and nothing else.",
        },
        new()
        {
            Id = "padded-armour", Name = "Padded Armour", Kind = ItemKind.Armour, Icon = 19, Layer = "armour-padded", Value = 5,
            Armour = ArmourCategory.Light, BaseAc = 11, StealthDisadvantage = true,
            Description = "Quilted layers of cloth and wadding. It stops a knife, and it rustles with every step.",
        },
        new()
        {
            Id = "leather-armour", Name = "Leather Armour", Kind = ItemKind.Armour, Icon = 20, Layer = "armour-leather", Value = 10,
            Armour = ArmourCategory.Light, BaseAc = 11,
            Description = "A breastplate and shoulders of boiled leather, oiled supple. Quiet, light, and a great deal better than a shirt.",
        },
        new()
        {
            Id = "studded-leather", Name = "Studded Leather", Kind = ItemKind.Armour, Icon = 21, Layer = "armour-studded", Value = 45,
            Armour = ArmourCategory.Light, BaseAc = 12,
            Description = "Tough leather set with close rows of iron rivets. The best a rogue can wear and still go unheard.",
        },
        new()
        {
            Id = "hide-armour", Name = "Hide Armour", Kind = ItemKind.Armour, Icon = 22, Layer = "armour-hide", Value = 10,
            Armour = ArmourCategory.Medium, BaseAc = 12, MaxDex = MediumDex,
            Description = "Thick furs and hides stitched one over another. Warm in the Mirewood, and heavier than it looks.",
        },
        new()
        {
            Id = "chain-shirt", Name = "Chain Shirt", Kind = ItemKind.Armour, Icon = 23, Layer = "armour-chain-shirt", Value = 50,
            Armour = ArmourCategory.Medium, BaseAc = 13, MaxDex = MediumDex,
            Description = "A shirt of interlocking rings, worn under a tunic. It turns a blade and hides under your clothes.",
        },
        new()
        {
            Id = "scale-mail", Name = "Scale Mail", Kind = ItemKind.Armour, Icon = 24, Layer = "armour-scale", Value = 50,
            Armour = ArmourCategory.Medium, BaseAc = 14, MaxDex = MediumDex, StealthDisadvantage = true,
            Description = "A coat of overlapping steel scales on leather, down past the knee. It glitters, and it clanks.",
        },
        new()
        {
            Id = "chain-mail", Name = "Chain Mail", Kind = ItemKind.Armour, Icon = 25, Layer = "armour-chain", Value = 75,
            Armour = ArmourCategory.Heavy, BaseAc = 16, MaxDex = HeavyDex, StrengthNeeded = 13, StealthDisadvantage = true,
            Description = "Steel rings from shoulder to knee over a quilted jack. Without the strength for it, you'll trudge.",
        },
        new()
        {
            Id = "plate-armour", Name = "Plate Armour", Kind = ItemKind.Armour, Icon = 26, Layer = "armour-plate", Value = 1500,
            Armour = ArmourCategory.Heavy, BaseAc = 18, MaxDex = HeavyDex, StrengthNeeded = 15, StealthDisadvantage = true,
            Description = "Shaped steel plates from collar to boot, buckled over mail. A knight's armour, at a knight's price.",
        },
        new()
        {
            Id = "mage-robes", Name = "Mage Robes", Kind = ItemKind.Armour, Icon = 27, Layer = "armour-robe", Value = 8,
            Armour = ArmourCategory.None, BaseAc = ArmourClass.Unarmoured,
            Description = "Deep blue robes with silver thread at the hems, cut in the Mages Guild's fashion. Room to move, room to cast.",
        },

        // Shields.
        new()
        {
            Id = "wooden-shield", Name = "Wooden Shield", Kind = ItemKind.Shield, Icon = 28, Layer = "shield-round", Value = 10,
            AcBonus = ShieldAc,
            Description = "A round shield of banded oak with an iron boss. The battered ones have the best stories.",
        },
        new()
        {
            Id = "steel-shield", Name = "Steel Shield", Kind = ItemKind.Shield, Icon = 29, Layer = "shield-heater", Value = 20,
            AcBonus = ShieldAc,
            Description = "A heater shield of hammered steel, enamelled Questeria blue. It rings like a bell when it's struck.",
        },
        new()
        {
            Id = "warden-shield", Name = "Warden's Shield", Kind = ItemKind.Shield, Icon = 30, Layer = "shield-heater", Value = 300,
            Rarity = Rarity.Uncommon, AcBonus = ShieldAc + 1,
            Description = "A Royal Warden's shield, blessed in the Sunspire's chapel. Blows slide off it as if it knew they were coming.",
        },

        // Heads.
        new()
        {
            Id = "leather-cap", Name = "Leather Cap", Kind = ItemKind.Head, Icon = 31, Layer = "helm-cap", Value = 2,
            Description = "A padded leather cap. It keeps off the rain and the worst of a low branch.",
        },
        new()
        {
            Id = "iron-helm", Name = "Iron Helm", Kind = ItemKind.Head, Icon = 32, Layer = "helm-full", Value = 12,
            Description = "A full iron helm with a hinged visor. It hides your hair and most of your face, which some people prefer.",
        },
        new()
        {
            Id = "circlet-of-focus", Name = "Circlet of Focus", Kind = ItemKind.Head, Icon = 33, Layer = "helm-circlet", Value = 250,
            Rarity = Rarity.Uncommon, SpellAttackBonus = 1,
            Description = "A thin silver circlet set with a blue stone that hums while a spell takes shape. Spells find their mark a little more often.",
        },

        // Hands.
        new()
        {
            Id = "leather-gloves", Name = "Leather Gloves", Kind = ItemKind.Hands, Icon = 34, Layer = "gloves", Value = 2,
            Description = "Soft doeskin gloves, good for reins, ropes and cold mornings.",
        },
        new()
        {
            Id = "gauntlets-of-might", Name = "Gauntlets of Might", Kind = ItemKind.Hands, Icon = 35, Layer = "gauntlets", Value = 250,
            Rarity = Rarity.Uncommon, DamageBonus = 1,
            Description = "Iron gauntlets with Stoneborn runes worked into the knuckles. Every weapon blow you land hits a little harder.",
        },

        // Feet.
        new()
        {
            Id = "leather-boots", Name = "Leather Boots", Kind = ItemKind.Feet, Icon = 36, Layer = "boots-leather", Value = 2,
            Description = "Stout leather boots, well broken in. Every adventure starts in a pair.",
        },
        new()
        {
            Id = "iron-boots", Name = "Iron Boots", Kind = ItemKind.Feet, Icon = 37, Layer = "boots-iron", Value = 10,
            Description = "Boots plated with iron at the shin and toe. Loud on stone, and very hard to stamp on.",
        },
        new()
        {
            Id = "boots-of-striding", Name = "Boots of Striding", Kind = ItemKind.Feet, Icon = 38, Layer = "boots-leather", Value = 250,
            Rarity = Rarity.Uncommon, SpeedBonus = Tile,
            Description = "Soft boots with a silver wing stitched at each heel. Your stride lengthens without you trying.",
        },

        // Cloaks.
        new()
        {
            Id = "travelling-cloak", Name = "Travelling Cloak", Kind = ItemKind.Cloak, Icon = 39, Layer = "cloak", Value = 1,
            Description = "A hooded wool cloak, waxed against the rain. It doubles as a blanket.",
        },
        new()
        {
            Id = "cloak-of-protection", Name = "Cloak of Protection", Kind = ItemKind.Cloak, Icon = 40, Layer = "cloak", Value = 350,
            Rarity = Rarity.Uncommon, AcBonus = 1,
            Description = "A grey-green cloak that stirs when there's no wind. Blows that should land somehow don't.",
        },

        // An amulet and a ring, which are stats and no picture.
        new()
        {
            Id = "amulet-of-vigour", Name = "Amulet of Vigour", Kind = ItemKind.Amulet, Icon = 41, Value = 300,
            Rarity = Rarity.Uncommon, MaxHpBonus = 5,
            Description = "A red stone on a gold chain, warm against the skin. You feel steadier with it on, and harder to put down.",
        },
        new()
        {
            Id = "ring-of-protection", Name = "Ring of Protection", Kind = ItemKind.Ring, Icon = 42, Value = 500,
            Rarity = Rarity.Rare, AcBonus = 1,
            Description = "A plain silver band with a ward cut inside it. A faint glimmer turns the edge of every blow.",
        },

        // Consumables: drinking one is a bonus action, throwing one an action.
        new()
        {
            Id = HealingPotion, Name = "Healing Potion", Kind = ItemKind.Consumable, Icon = 43, Value = 50, Stacks = true,
            Heals = DiceExpression.Parse("2d4+2"),
            Description = "A small bottle of red tonic with a glow in it. It tastes of cherries and closes wounds.",
        },
        new()
        {
            Id = "greater-healing-potion", Name = "Greater Healing Potion", Kind = ItemKind.Consumable, Icon = 44, Value = 150, Stacks = true,
            Rarity = Rarity.Uncommon, Heals = DiceExpression.Parse("4d4+4"),
            Description = "A bigger, brighter draught, gold flecks turning in the red. It mends what a lesser potion can't.",
        },
        new()
        {
            Id = "alchemists-fire", Name = "Alchemist's Fire", Kind = ItemKind.Consumable, Icon = 45, Value = 50, Stacks = true,
            ThrownDamage = DiceExpression.Parse("1d4"), ThrownDamageType = DamageType.Fire, ThrowRange = Throw,
            Description = "A clay flask of sticky liquid that bursts into flame when it breaks. Throw it; don't drop it.",
        },
        new()
        {
            Id = "elixir-of-focus", Name = "Elixir of Focus", Kind = ItemKind.Consumable, Icon = 46, Value = 120, Stacks = true,
            Rarity = Rarity.Uncommon, FocusRestored = ElixirFocus,
            Description = "A blue elixir that fizzes on the tongue and clears the head. Drunk in a fight, it gives back 30 Focus.",
        },

        // Materials: the story's loot, for selling.
        new()
        {
            Id = "wolf-pelt", Name = "Wolf Pelt", Kind = ItemKind.Material, Icon = 47, Value = 4, Stacks = true,
            Description = "A thick grey pelt from a Mirewood wolf, the black smoke long gone out of it. Traders pay for good fur.",
        },
        new()
        {
            Id = "black-crystal-shard", Name = "Black Crystal Shard", Kind = ItemKind.Material, Icon = 48, Value = 25, Stacks = true,
            Description = "A splinter of the black crystal that grows wherever the land's gone wrong. It's cold, it hums, and somebody always wants to buy it.",
        },
    };

    private static readonly Dictionary<string, ItemDefinition> ById = new();

    /// <summary>Indexes the table and checks every row.</summary>
    /// <remarks>Throws on purpose, so a bad row shows up when the game starts.</remarks>
    static ItemCatalog()
    {
        for (int i = 0; i < Table.Length; i++)
        {
            ItemDefinition item = Table[i];
            if (string.IsNullOrWhiteSpace(item.Id)) throw new InvalidOperationException($"Item {i} in the table has no id.");
            if (Problem(item, i) is string problem) throw new InvalidOperationException($"Item {item.Id} {problem}.");
            if (!ById.TryAdd(item.Id, item)) throw new InvalidOperationException($"Two items are called {item.Id}.");
        }

        // After they're all in, since a magic item can come before the plain one it's a finer version of.
        foreach (ItemDefinition item in Table)
        {
            if (item.Base is not null && (!ById.TryGetValue(item.Base, out ItemDefinition plain)
                || plain.Kind != item.Kind || plain.Category != item.Category || plain.Base is not null))
                throw new InvalidOperationException($"Item {item.Id} is a finer {item.Base}, which isn't a plain weapon of the same category.");
        }
    }

    /// <summary>Every item, in the table's order, which is the icons' order.</summary>
    public static IEnumerable<ItemDefinition> All
    {
        get
        {
            foreach (ItemDefinition item in Table) yield return item;
        }
    }

    /// <summary>Whether there's an item called this.</summary>
    /// <param name="id">What a table, a map or a save calls it.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One item.</summary>
    /// <param name="id">Its id. It has to exist.</param>
    public static ItemDefinition Get(string id) => ById[id];

    /// <summary>What's wrong with a row, or null if nothing is.</summary>
    /// <remarks>Every kind has to have what it needs and nothing another kind uses, so a setting on the wrong row can't sit there doing nothing.</remarks>
    /// <param name="item">The row.</param>
    /// <param name="index">Where it is in the table, which has to be its Icon.</param>
    private static string Problem(ItemDefinition item, int index)
    {
        if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Description)) return "has no name or no description";
        if (!Printable(item.Name) || !Printable(item.Description)) return "has a character in its name or description that the fonts can't draw";
        if (item.Icon != index) return $"has icon {item.Icon}, but it's number {index} in the table and the icons are drawn in the table's order";
        if (item.Value < 1) return "is worth nothing";

        bool usedUp = item.Kind is ItemKind.Consumable or ItemKind.Material;
        bool pictured = !usedUp && item.Kind is not (ItemKind.Amulet or ItemKind.Ring);
        if (item.Stacks != usedUp) return usedUp ? "doesn't stack, and consumables and materials do" : "stacks, and only consumables and materials do";
        if (item.Layer is not null && !pictured) return "draws a paperdoll layer, but nothing's drawn for its kind";
        if (item.Slot is null && (item.AcBonus != 0 || item.SpeedBonus != 0f || item.MaxHpBonus != 0 || item.SpellAttackBonus != 0 || item.DamageBonus != 0))
            return "has bonuses for wearing it, and it isn't worn";
        if (item.Kind == ItemKind.Shield && item.AcBonus < ShieldAc) return $"is a shield that adds less than {ShieldAc} to AC";
        if (item.Rarity == Rarity.Common && Magical(item)) return "is magic, so it can't be Common";

        return (item.Kind == ItemKind.Weapon ? WeaponProblem(item) : NotWeaponProblem(item))
            ?? (item.Kind == ItemKind.Armour ? ArmourProblem(item) : NotArmourProblem(item))
            ?? (item.Kind == ItemKind.Consumable ? ConsumableProblem(item) : NotConsumableProblem(item));
    }

    /// <summary>What's wrong with a weapon's settings, or null.</summary>
    private static string WeaponProblem(ItemDefinition item)
    {
        if (item.Damage.Count == 0) return "is a weapon with no damage dice";
        if (item.Has(WeaponProperty.Versatile) == item.VersatileDamage.IsNone) return "has to have VersatileDamage if and only if it's Versatile";
        if (item.Has(WeaponProperty.Thrown) != item.ThrowRange > 0f) return "has to have a ThrowRange if and only if it's Thrown";
        if (item.Has(WeaponProperty.TwoHanded) && item.Has(WeaponProperty.Light)) return "is two-handed and light at once";

        bool rightRange = item.Has(WeaponProperty.Ranged)
            ? item.Range > LongReach
            : item.Range == (item.Has(WeaponProperty.Reach) ? LongReach : Melee);
        return rightRange ? null : $"has a range of {item.Range} m, which is wrong for its kind of weapon";
    }

    /// <summary>What's wrong with something that isn't a weapon having weapon settings, or null.</summary>
    private static string NotWeaponProblem(ItemDefinition item) =>
        !item.Damage.IsNone || !item.VersatileDamage.IsNone || !item.ExtraDamage.IsNone || item.Properties != WeaponProperty.None
        || item.Range != 0f || item.AttackBonus != 0 || item.Base is not null
            ? "has weapon settings, and it isn't a weapon"
            : null;

    /// <summary>What's wrong with body armour's settings, or null.</summary>
    private static string ArmourProblem(ItemDefinition item)
    {
        if (item.BaseAc < ArmourClass.Unarmoured) return $"is armour with a base AC under {ArmourClass.Unarmoured}";
        if (item.Armour == ArmourCategory.None && item.BaseAc != ArmourClass.Unarmoured) return $"is clothes, which are AC {ArmourClass.Unarmoured}";

        int? dexterity = item.Armour switch
        {
            ArmourCategory.Medium => MediumDex,
            ArmourCategory.Heavy => HeavyDex,
            _ => null,
        };
        if (item.MaxDex != dexterity) return $"lets the wrong amount of Dexterity through for {item.Armour} armour";
        return item.StrengthNeeded > 0 && item.Armour != ArmourCategory.Heavy ? "needs Strength, and only heavy armour does" : null;
    }

    /// <summary>What's wrong with something that isn't body armour having armour settings, or null.</summary>
    private static string NotArmourProblem(ItemDefinition item) =>
        item.Armour != ArmourCategory.None || item.BaseAc != 0 || item.MaxDex is not null || item.StrengthNeeded != 0 || item.StealthDisadvantage
            ? "has body armour settings, and it isn't body armour"
            : null;

    /// <summary>What's wrong with a consumable's settings, or null. It has to do exactly one thing.</summary>
    private static string ConsumableProblem(ItemDefinition item)
    {
        int effects = (item.Heals.IsNone ? 0 : 1) + (item.ThrownDamage.IsNone ? 0 : 1) + (item.FocusRestored > 0 ? 1 : 0);
        if (item.FocusRestored is < 0 or > Combatant.MaxFocus) return $"gives back {item.FocusRestored} Focus, past the 0 to {Combatant.MaxFocus} there is";
        if (effects != 1) return $"is a consumable that does {effects} things, and it has to do one";
        return item.ThrownDamage.IsNone == item.ThrowRange > 0f ? "has to have a ThrowRange if and only if it's thrown" : null;
    }

    /// <summary>What's wrong with something that isn't a consumable having consumable settings, or null.</summary>
    private static string NotConsumableProblem(ItemDefinition item) =>
        !item.Heals.IsNone || !item.ThrownDamage.IsNone || item.FocusRestored != 0 || item.Kind != ItemKind.Weapon && item.ThrowRange != 0f
            ? "does what a consumable does, and it isn't one"
            : null;

    /// <summary>Whether an item is magic: it adds to something beyond what its plain kind does.</summary>
    private static bool Magical(ItemDefinition item) =>
        item.AttackBonus > 0 || item.DamageBonus > 0 || !item.ExtraDamage.IsNone || item.SpeedBonus > 0f || item.MaxHpBonus > 0
        || item.SpellAttackBonus > 0 || item.FocusRestored > 0 || item.AcBonus > (item.Kind == ItemKind.Shield ? ShieldAc : 0);

    /// <summary>Whether every character of some text is one the fonts can draw.</summary>
    private static bool Printable(string text)
    {
        foreach (char c in text)
            if (c < FirstPrintable || c > LastPrintable) return false;
        return true;
    }
}
