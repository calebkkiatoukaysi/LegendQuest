using System.Collections.Generic;
using LegendQuest.Combat.Realtime;

namespace LegendQuest.Combat;

/// <summary>The eight base classes' kits (10-classes.md §2), converted to real time by 11-combat.md §4.2 and §4.3.</summary>
/// <remarks>
/// Each line of a table is one row here, in the table's order. Where the table says something only
/// turns could say, the row says what it is in seconds: "until your next turn" is 3 s, "3 turns" 9 s,
/// "once a turn" an internal cooldown of 3 s, "Shift for free" a 60 px dash, "the back row" a 40 px
/// knockback, "pulled to the front" a pull to melee reach. Riposte is a B in the table but arms like a
/// reaction (11-combat.md §4.2 names it with them). Durations of temporary hit points, which 5e lets
/// last, are 30 s here so nothing lasts past a long fight.
/// </remarks>
public static partial class SkillCatalog
{
    /// <summary>How long a shield of temporary hit points lasts.</summary>
    private const int ShieldMs = 30000;

    /// <summary>The base classes' rows.</summary>
    private static IEnumerable<SkillDefinition> BaseClasses()
    {
        // Swordhand (fighter): front-line damage, Momentum.
        yield return new()
        {
            Id = "fighter-strike", Name = "Strike", Class = ClassId.Fighter, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit }, Description = "Weapon dice + Str.",
        };
        yield return new()
        {
            Id = "fighter-cleave", Name = "Cleave", Class = ClassId.Fighter, Focus = 20, CooldownMs = Cd1, Weapon = true, Range = SkillRange.Melee,
            Area = AreaShape.Neighbours, Effects = new[] { WeaponHit }, Description = "Weapon dice to the foe and every foe beside it.",
        };
        yield return new()
        {
            Id = "fighter-second-wind", Name = "Second Wind", Class = ClassId.Fighter, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = 16000,
            Range = SkillRange.Self, Target = SkillTarget.Self, Ai = AiUse.Heal,
            Effects = new[] { Heal("1d10", casting: false, levelTimes: 1) with { On = EffectOn.Self } }, Description = "Heal 1d10 + level.",
        };
        yield return new()
        {
            Id = "fighter-riposte", Name = "Riposte", Class = ClassId.Fighter, Rank = 3, Kind = SkillKind.Reaction, Focus = 20, CooldownMs = Cd3,
            Range = SkillRange.Self, Target = SkillTarget.Self, Trigger = ReactionTrigger.MeleeMissOnYou, Ai = AiUse.Defensive, AiValue = 4,
            Effects = new[] { Arm, new Effect { Kind = EffectType.Strike, On = EffectOn.Attacker } },
            Description = "For 4 s, the next melee attack that misses you is answered with a Strike.",
        };
        yield return new()
        {
            Id = "fighter-fighting-style", Name = "Fighting Style", Class = ClassId.Fighter, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.ToHit, 2, When: When.Melee) }, Description = "+2 to hit with melee.",
        };
        yield return new()
        {
            Id = "fighter-action-surge", Name = "Action Surge", Class = ClassId.Fighter, Rank = 5, Kind = SkillKind.Bonus, Focus = 60, CooldownMs = OncePerFight,
            Ultimate = true, Range = SkillRange.Self, Target = SkillTarget.Self, Ai = AiUse.Utility, AiValue = 14,
            Effects = new[] { new Effect { Kind = EffectType.Reset, On = EffectOn.Self } },
            Description = "Take another action: the GCD resets, and the next action skill you use starts no cooldown.",
        };
        yield return new()
        {
            Id = "fighter-indomitable", Name = "Indomitable", Class = ClassId.Fighter, Rank = 10, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.OnFailedSave, new[] { new Effect { Kind = EffectType.Reroll } }, OncePerFight: true) },
            Description = "Once a fight, reroll a failed save.",
        };

        // Shadowstep (rogue): single-target damage, Opportunist.
        yield return new()
        {
            Id = "rogue-strike", Name = "Strike", Class = ClassId.Rogue, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit with { Sneak = true } },
            Description = "Weapon dice + Dex. Sneak Attack: +2d6⁺ if an ally hit the target in the last 3 s, or it's Marked or Prone; once every 3 s.",
        };
        yield return new()
        {
            Id = "rogue-backstab", Name = "Backstab", Class = ClassId.Rogue, Focus = 15, CooldownMs = Cd1, Weapon = true, Range = SkillRange.Short,
            Effects = new[] { new Effect { Kind = EffectType.Teleport, On = EffectOn.Self, Once = true }, WeaponHit with { Sneak = true, WeaponTimes = 2 } },
            Description = "Step behind a foe up to 100 px away and strike: weapon dice twice + Dex, with Sneak Attack.",
        };
        yield return new()
        {
            Id = "rogue-smoke-step", Name = "Smoke Step", Class = ClassId.Rogue, Rank = 2, Kind = SkillKind.Bonus, Focus = 15, CooldownMs = Cd3,
            Range = SkillRange.Self, Target = SkillTarget.Self, Ai = AiUse.Defensive, AiValue = 4,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Dash, On = EffectOn.Self, Distance = 60f },
                Put(Buff("smoke-step", "Smoke Step", 2 * Turn, new StatMod(Stat.AttackersDisadvantage)), EffectOn.Self),
            },
            Description = "Dash 60 px clear; attacks on you have disadvantage for 6 s.",
        };
        yield return new()
        {
            Id = "rogue-poisoned-edge", Name = "Poisoned Edge", Class = ClassId.Rogue, Rank = 3, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3,
            Range = SkillRange.Self, Target = SkillTarget.Self, AiValue = 6,
            Effects = new[]
            {
                new Effect
                {
                    Kind = EffectType.Rider, On = EffectOn.Self, DurationMs = 3 * Turn,
                    Rider = new[] { Put(Condition("poisoned-edge", "Poisoned", ConditionId.Poisoned, 3 * Turn) with { TickDice = D("1d4"), TickType = DamageType.Poison }) },
                },
            },
            Description = "Your next hit in 9 s Poisons: 1d4 poison every 3 s for 9 s, and disadvantage on its attacks.",
        };
        yield return new()
        {
            Id = "rogue-cunning", Name = "Cunning", Class = ClassId.Rogue, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.SpeedPct, 25), new StatMod(Stat.Haste, 5) }, Description = "Move a quarter faster, and 5% haste.",
        };
        yield return new()
        {
            Id = "rogue-shadow-dance", Name = "Shadow Dance", Class = ClassId.Rogue, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Weapon = true, Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.RandomFoes, Count = 3, Strikes = 3,
            Effects = new[] { new Effect { Kind = EffectType.Teleport, On = EffectOn.Self }, WeaponHit with { ForceSneak = true } },
            Description = "A Backstab on each of 3 random foes (they can repeat), each with Sneak Attack.",
        };
        yield return new()
        {
            Id = "rogue-evasion", Name = "Evasion", Class = ClassId.Rogue, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Evasion) }, Description = "Area skills do half to you, and none on a made save.",
        };

        // Lightbearer (cleric): healer, Devotion.
        yield return new()
        {
            Id = "cleric-sacred-flame", Name = "Sacred Flame", Class = ClassId.Cleric, Kind = SkillKind.Auto, Range = SkillRange.Long,
            Effects = new[] { SaveHit("1d8", DamageType.Radiant, Ability.Dexterity, false, plus: true) }, Description = "Dex save or 1d8⁺ radiant.",
        };
        yield return new()
        {
            Id = "cleric-cure-wounds", Name = "Cure Wounds", Class = ClassId.Cleric, Focus = 30, CooldownMs = Cd2, Range = SkillRange.Short,
            Target = SkillTarget.Ally, Ai = AiUse.Heal, Effects = new[] { Heal("1d8", plus: true) }, Description = "Heal 1d8⁺ + Wis. Gets a Downed ally up.",
        };
        yield return new()
        {
            Id = "cleric-bless", Name = "Bless", Class = ClassId.Cleric, Rank = 2, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd4, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Area = AreaShape.Allies, Count = 3, Ai = AiUse.Buff, AiValue = 2.5,
            Effects = new[] { Put(Buff("blessed", "Blessed", 3 * Turn, new StatMod(Stat.ToHitDice, Dice: D("1d4")), new StatMod(Stat.SaveDice, Dice: D("1d4")))) },
            Description = "Three allies: +1d4 to attacks and saves for 9 s.",
        };
        yield return new()
        {
            Id = "cleric-guiding-bolt", Name = "Guiding Bolt", Class = ClassId.Cleric, Rank = 3, Focus = 30, CooldownMs = Cd2, Range = SkillRange.Long,
            Projectile = DamageType.Radiant,
            Effects = new[]
            {
                SpellHit("4d6", DamageType.Radiant),
                Put(new AuraDefinition { Id = "guided", Name = "Marked", Condition = ConditionId.Marked, DurationMs = 2 * Turn, UsedBy = AuraUse.AttackTaken }, ifLanded: true),
            },
            Description = "4d6 radiant; the next attack on it has advantage.",
        };
        yield return new()
        {
            Id = "cleric-disciple-of-life", Name = "Disciple of Life", Class = ClassId.Cleric, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.HealingFlat, 3) }, Description = "Your heals heal 3 more.",
        };
        yield return new()
        {
            Id = "cleric-dawnlight", Name = "Dawnlight", Class = ClassId.Cleric, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true, CastMs = 1000,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes,
            Effects = new[]
            {
                SaveHit("3d10", DamageType.Radiant, Ability.Constitution, true) with { UndeadDisadvantage = true },
                Heal("2d6", casting: false) with { On = EffectOn.AllAllies, Once = true },
            },
            Description = "Foes: 3d10 radiant, Con save for half (undead save with disadvantage). Allies: heal 2d6.",
        };
        yield return new()
        {
            Id = "cleric-blessed-healer", Name = "Blessed Healer", Class = ClassId.Cleric, Rank = 10, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.OnHealOther, new[] { Heal("1d6", casting: false) with { On = EffectOn.Self } }) },
            Description = "When you heal someone else, you heal 1d6.",
        };

        // Arcanist (wizard): area damage, Wellspring.
        yield return new()
        {
            Id = "wizard-fire-bolt", Name = "Fire Bolt", Class = ClassId.Wizard, Kind = SkillKind.Auto, Range = SkillRange.Long, Projectile = DamageType.Fire,
            Effects = new[] { SpellHit("1d10", DamageType.Fire, plus: true) }, Description = "1d10⁺ fire.",
        };
        yield return new()
        {
            Id = "wizard-magic-missile", Name = "Magic Missile", Class = ClassId.Wizard, Focus = 20, CooldownMs = Cd1, Range = SkillRange.Long,
            Area = AreaShape.Spread, Count = 3, Strikes = 3, Projectile = DamageType.Force,
            Effects = new[] { SureHit("1d4+2", DamageType.Force) }, Description = "Three darts of 1d4 + 2 force, shared among up to 3 foes; they always hit.",
        };
        yield return new()
        {
            Id = "wizard-burning-hands", Name = "Burning Hands", Class = ClassId.Wizard, Rank = 2, Focus = 20, CooldownMs = Cd2, Range = SkillRange.Short,
            Area = AreaShape.Row, Effects = new[] { SaveHit("3d6", DamageType.Fire, Ability.Dexterity, true) },
            Description = "3d6 fire to every foe within 48 px of the target, Dex save for half.",
        };
        yield return new()
        {
            Id = "wizard-shield", Name = "Shield", Class = ClassId.Wizard, Rank = 3, Kind = SkillKind.Reaction, Focus = 20, CooldownMs = Cd2,
            Range = SkillRange.Self, Target = SkillTarget.Self, Trigger = ReactionTrigger.AttackOnYou, Ai = AiUse.Defensive, AiValue = 3,
            Effects = new[] { Arm, Put(Buff("shielded", "Shield", Turn, new StatMod(Stat.Ac, 5)), EffectOn.Self) },
            Description = "For 4 s, the next attack on you meets +5 AC, which lasts 3 s.",
        };
        yield return new()
        {
            Id = "wizard-arcane-recovery", Name = "Arcane Recovery", Class = ClassId.Wizard, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.FocusPerTurn, 10) }, Description = "+10 Focus every 3 s.",
        };
        yield return new()
        {
            Id = "wizard-fireball", Name = "Fireball", Class = ClassId.Wizard, Rank = 5, Focus = 70, CooldownMs = OncePerFight, Ultimate = true, CastMs = 1200,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, Projectile = DamageType.Fire, AiMinTargets = 2,
            Effects = new[] { SaveHit("8d6", DamageType.Fire, Ability.Dexterity, true) }, Description = "8d6 fire to every foe, Dex save for half.",
        };
        yield return new()
        {
            Id = "wizard-empowered", Name = "Empowered", Class = ClassId.Wizard, Rank = 10, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.OnHit, new[] { new Effect { Kind = EffectType.Damage, AddAbility = Ability.Intelligence } }, CooldownMs: Turn, When: When.Spell) },
            Description = "Once every 3 s, add Int to a spell's damage.",
        };

        // Warden (paladin): tank and burst, Bulwark, double threat.
        yield return new()
        {
            Id = "paladin-strike", Name = "Strike", Class = ClassId.Paladin, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit }, Description = "Weapon dice + Str.",
        };
        yield return new()
        {
            Id = "paladin-divine-smite", Name = "Divine Smite", Class = ClassId.Paladin, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd1,
            Range = SkillRange.Self, Target = SkillTarget.Self, AiValue = 10,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Rider, On = EffectOn.Self, DurationMs = 2 * Turn, Rider = new[] { SureHit("2d8", DamageType.Radiant) with { VsUndead = D("1d8") } } },
            },
            Description = "Your next hit in 6 s does +2d8 radiant (+1d8 more against undead and fiends).",
        };
        yield return new()
        {
            Id = "paladin-lay-on-hands", Name = "Lay on Hands", Class = ClassId.Paladin, Rank = 2, Focus = 20, Range = SkillRange.Short, Target = SkillTarget.Ally,
            Ai = AiUse.Heal,
            Effects = new[] { new Effect { Kind = EffectType.Heal, PoolPerLevel = 5 }, new Effect { Kind = EffectType.Cleanse, Cleanses = new[] { ConditionId.Poisoned } } },
            Description = "Heal from a pool of 5 × level a fight, as much as they're missing. Cures Poison.",
        };
        yield return new()
        {
            Id = "paladin-raise-shield", Name = "Raise Shield", Class = ClassId.Paladin, Rank = 3, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3,
            Range = SkillRange.Self, Target = SkillTarget.Self, Ai = AiUse.Taunt, AiValue = 4,
            Effects = new[]
            {
                Put(Buff("shield-raised", "Shield Raised", Turn, new StatMod(Stat.Ac, 2)), EffectOn.Self),
                new Effect { Kind = EffectType.Taunt, On = EffectOn.AllFoes, DurationMs = Turn },
            },
            Description = "+2 AC for 3 s, and every foe's attacks must come at you for 3 s.",
        };
        yield return new()
        {
            Id = "paladin-aura-of-protection", Name = "Aura of Protection", Class = ClassId.Paladin, Rank = 4, Kind = SkillKind.Passive,
            AllyMods = new[] { new StatMod(Stat.Save, Of: Ability.Charisma) }, AllyRadius = 60f, Description = "Allies within 60 px add your Cha to saves.",
        };
        yield return new()
        {
            Id = "paladin-unbroken-oath", Name = "Unbroken Oath", Class = ClassId.Paladin, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllAllies, Ai = AiUse.Buff, AiValue = 3,
            Effects = new[]
            {
                new Effect { Kind = EffectType.TempHp, Dice = D("2d10"), LevelTimes = 1, Aura = TempHpShield("oath-shield", "Unbroken Oath", ShieldMs) },
                Put(Buff("oathbound", "Oathbound", 3 * Turn,
                    new StatMod(Stat.Immune, Condition: ConditionId.Frightened), new StatMod(Stat.Immune, Condition: ConditionId.Charmed))),
            },
            Description = "Every ally: temporary hit points 2d10 + level; immune to Frightened and Charmed for 9 s.",
        };
        yield return new()
        {
            Id = "paladin-radiant-strikes", Name = "Radiant Strikes", Class = ClassId.Paladin, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.DamageDice, Dice: D("1d8"), Type: DamageType.Radiant, When: When.Melee) }, Description = "Melee hits do +1d8 radiant.",
        };

        // Wildcaller (druid): control and healing, Wild.
        yield return new()
        {
            Id = "druid-thorn-whip", Name = "Thorn Whip", Class = ClassId.Druid, Kind = SkillKind.Auto, Range = SkillRange.Reach,
            Effects = new[] { SpellHit("1d6", DamageType.Piercing, plus: true), new Effect { Kind = EffectType.Pull, IfLanded = true } },
            Description = "1d6⁺ piercing, and pulled to your reach.",
        };
        yield return new()
        {
            Id = "druid-entangle", Name = "Entangle", Class = ClassId.Druid, Focus = 30, CooldownMs = Cd3, Range = SkillRange.Long, Area = AreaShape.Row,
            Ai = AiUse.Control, AiValue = 5,
            Effects = new[] { Put(Condition("entangled", "Restrained", ConditionId.Restrained, 2 * Turn, Ability.Strength), save: Ability.Strength) },
            Description = "Every foe within 48 px of the target: Str save or Restrained 6 s (a Str save every 3 s ends it).",
        };
        yield return new()
        {
            Id = "druid-healing-word", Name = "Healing Word", Class = ClassId.Druid, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd2,
            Range = SkillRange.Long, Target = SkillTarget.Ally, Ai = AiUse.Heal, Effects = new[] { Heal("1d4", plus: true) },
            Description = "Heal 1d4⁺ + Wis. Gets a Downed ally up.",
        };
        yield return new()
        {
            Id = "druid-moonbeam", Name = "Moonbeam", Class = ClassId.Druid, Rank = 3, Focus = 30, CooldownMs = Cd3, Range = SkillRange.Long, Area = AreaShape.Line,
            Concentration = true,
            Effects = new[]
            {
                SureHit("2d10", DamageType.Radiant),
                Put(new AuraDefinition { Id = "moonbeam", Name = "Moonbeam", DurationMs = 3 * Turn, TickDice = D("2d10"), TickType = DamageType.Radiant }),
            },
            Description = "A column from you through the target: 2d10 radiant now and every 3 s for 9 s (concentration).",
        };
        yield return new()
        {
            Id = "druid-natural-recovery", Name = "Natural Recovery", Class = ClassId.Druid, Rank = 4, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.AfterFight, new[] { new Effect { Kind = EffectType.Heal, MaxHpShare = 0.1, On = EffectOn.AllAllies } }) },
            Description = "After a fight, the party heals 10%.",
        };
        yield return new()
        {
            Id = "druid-wild-shape-bear", Name = "Wild Shape: Bear", Class = ClassId.Druid, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Self, Target = SkillTarget.Self, Ai = AiUse.Defensive, AiValue = 14,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Transform, On = EffectOn.Self, Id = "bear", DurationMs = 4 * Turn },
                new Effect { Kind = EffectType.TempHp, On = EffectOn.Self, LevelTimes = 3, Flat = 10, Aura = TempHpShield("bear-hide", "Bear's Hide", 4 * Turn) },
            },
            Description = "A bear for 12 s: temporary hit points 3 × level + 10, Claws 2d6⁺, Maul hits a foe and its neighbours.",
        };
        yield return new()
        {
            Id = "druid-natures-ward", Name = "Nature's Ward", Class = ClassId.Druid, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Immune, Condition: ConditionId.Poisoned) }, Description = "Immune to Poisoned.",
        };

        // Hexblade (warlock): ranged damage and curses, Pact.
        yield return new()
        {
            Id = "warlock-eldritch-blast", Name = "Eldritch Blast", Class = ClassId.Warlock, Kind = SkillKind.Auto, Range = SkillRange.Long,
            Projectile = DamageType.Force, StrikesByLevel = true, Effects = new[] { SpellHit("1d10", DamageType.Force) },
            Description = "1d10 force; a second beam at level 5, a third at 9.",
        };
        yield return new()
        {
            Id = "warlock-hex", Name = "Hex", Class = ClassId.Warlock, Kind = SkillKind.Bonus, Focus = 15, Range = SkillRange.Long, Concentration = true,
            Ai = AiUse.Control, AiValue = 6,
            Effects = new[] { Put(Debuff("hexed", "Hexed", 10 * Turn, new StatMod(Stat.TakenFromSource, Dice: D("1d8"), Type: DamageType.Necrotic)) with { Condition = ConditionId.Hexed }) },
            Description = "Your hits on it do +1d8 necrotic for 30 s (concentration).",
        };
        yield return new()
        {
            Id = "warlock-hellish-rebuke", Name = "Hellish Rebuke", Class = ClassId.Warlock, Rank = 2, Kind = SkillKind.Reaction, Focus = 20, CooldownMs = Cd2,
            Range = SkillRange.Self, Target = SkillTarget.Self, Trigger = ReactionTrigger.HitOnYou, Ai = AiUse.Defensive, AiValue = 6,
            Effects = new[] { Arm, SaveHit("2d10", DamageType.Fire, Ability.Dexterity, true) with { On = EffectOn.Attacker } },
            Description = "For 4 s, whoever next hits you takes 2d10 fire, Dex save for half.",
        };
        yield return new()
        {
            Id = "warlock-armour-of-shadows", Name = "Armour of Shadows", Class = ClassId.Warlock, Rank = 3, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd4,
            Range = SkillRange.Self, Target = SkillTarget.Self, Ai = AiUse.Defensive,
            Effects = new[]
            {
                new Effect { Kind = EffectType.TempHp, On = EffectOn.Self, Dice = D("1d8"), LevelTimes = 1, AddAbility = Ability.Charisma, Aura = TempHpShield("armour-of-shadows", "Armour of Shadows", ShieldMs) },
            },
            Description = "Temporary hit points 1d8 + level + Cha.",
        };
        yield return new()
        {
            Id = "warlock-agonising-blast", Name = "Agonising Blast", Class = ClassId.Warlock, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Damage, When: When.Spell, Of: Ability.Charisma) }, Description = "Add Cha to your spells' damage.",
        };
        yield return new()
        {
            Id = "warlock-void-maw", Name = "Void Maw", Class = ClassId.Warlock, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, AiMinTargets = 2,
            Effects = new[] { Put(Condition("void-maw", "Void Maw", ConditionId.Blinded, 2 * Turn) with { TickDice = D("2d6"), TickType = DamageType.Cold }) },
            Description = "Every foe: Blinded 6 s, and 2d6 cold every 3 s of it.",
        };
        yield return new()
        {
            Id = "warlock-devils-sight", Name = "Devil's Sight", Class = ClassId.Warlock, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Immune, Condition: ConditionId.Blinded) }, Description = "Ignore Blinded and darkness.",
        };

        // Skald (bard): support, Tempo.
        yield return new()
        {
            Id = "bard-vicious-mockery", Name = "Vicious Mockery", Class = ClassId.Bard, Kind = SkillKind.Auto, Range = SkillRange.Long,
            Effects = new[]
            {
                SaveHit("1d4", DamageType.Psychic, Ability.Wisdom, false, plus: true),
                Put(new AuraDefinition { Id = "mocked", Name = "Mocked", DurationMs = 2 * Turn, UsedBy = AuraUse.NextAttack, Mods = new[] { new StatMod(Stat.Disadvantage) } }, ifLanded: true),
            },
            Description = "Wis save or 1d4⁺ psychic and disadvantage on its next attack.",
        };
        yield return new()
        {
            Id = "bard-inspiration", Name = "Inspiration", Class = ClassId.Bard, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd1, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Ai = AiUse.Buff, AiValue = 2.5,
            Effects = new[]
            {
                Put(new AuraDefinition
                {
                    Id = "inspired", Name = "Inspired", Beneficial = true, DurationMs = 5 * Turn, UsedBy = AuraUse.NextRoll, Condition = ConditionId.Inspired,
                    Mods = new[] { new StatMod(Stat.ToHitDice, Dice: D("1d6")), new StatMod(Stat.SaveDice, Dice: D("1d6")) },
                }),
            },
            Description = "+1d6 to their next attack or save in 15 s.",
        };
        yield return new()
        {
            Id = "bard-healing-word", Name = "Healing Word", Class = ClassId.Bard, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd2,
            Range = SkillRange.Long, Target = SkillTarget.Ally, Ai = AiUse.Heal, Effects = new[] { Heal("1d4", plus: true) },
            Description = "Heal 1d4⁺ + Cha. Gets a Downed ally up.",
        };
        yield return new()
        {
            Id = "bard-thunderwave", Name = "Thunderwave", Class = ClassId.Bard, Rank = 3, Focus = 30, CooldownMs = Cd2, Range = SkillRange.Self,
            Target = SkillTarget.None, Area = AreaShape.Front,
            Effects = new[]
            {
                SaveHit("2d8", DamageType.Thunder, Ability.Constitution, true),
                new Effect { Kind = EffectType.Knockback, Distance = 40f, IfLanded = true },
            },
            Description = "Every foe within 60 px in front of you: 2d8 thunder, Con save for half; a failed save knocks them 40 px back.",
        };
        yield return new()
        {
            Id = "bard-song-of-rest", Name = "Song of Rest", Class = ClassId.Bard, Rank = 4, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.AfterFight, new[] { new Effect { Kind = EffectType.Heal, LevelTimes = 1, On = EffectOn.AllAllies } }) },
            Description = "After a fight, everyone heals your level.",
        };
        yield return new()
        {
            Id = "bard-battle-hymn", Name = "Battle Hymn", Class = ClassId.Bard, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllAllies, Ai = AiUse.Buff, AiValue = 5,
            Effects = new[] { Put(Buff("battle-hymn", "Battle Hymn", 3 * Turn, new StatMod(Stat.ToHitDice, Dice: D("1d6")), new StatMod(Stat.DamageDice, Dice: D("1d6")))) },
            Description = "Every ally: +1d6 to hit and damage for 9 s.",
        };
        yield return new()
        {
            Id = "bard-countercharm", Name = "Countercharm", Class = ClassId.Bard, Rank = 10, Kind = SkillKind.Passive,
            AllyMods = new[] { new StatMod(Stat.SaveAdvantage, Condition: ConditionId.Frightened), new StatMod(Stat.SaveAdvantage, Condition: ConditionId.Charmed) },
            Description = "Allies have advantage on saves against Frightened and Charmed.",
        };

        // The bear's own skills, for Wild Shape's bar. Not the druid's kit: no class.
        yield return new()
        {
            Id = "druid-bear-claws", Name = "Claws", Kind = SkillKind.Auto, Range = SkillRange.Melee, Motion = "auto-unarmed-1",
            Effects = new[] { SpellHit("2d6", DamageType.Slashing, plus: true) }, Description = "2d6⁺ slashing.",
        };
        yield return new()
        {
            Id = "druid-bear-maul", Name = "Maul", Focus = 0, CooldownMs = Cd1, Range = SkillRange.Melee, Area = AreaShape.Neighbours, Motion = "auto-unarmed-3",
            Effects = new[] { SpellHit("2d6", DamageType.Bludgeoning, plus: true) }, Description = "2d6⁺ bludgeoning to the foe and every foe beside it.",
        };
    }

    /// <summary>The base classes' forms.</summary>
    private static IEnumerable<FormDefinition> BaseForms()
    {
        yield return new FormDefinition("bear", "Bear", "druid-bear-claws", new[] { "druid-bear-maul" }, WeaponKind.Unarmed, WeaponKinds.SwingMs);
    }

    /// <summary>What everyone shares: a foe's plain attack, and the duo's Soul-Link Strike.</summary>
    private static IEnumerable<SkillDefinition> Shared()
    {
        yield return new()
        {
            Id = FoeAttack, Name = "Attack", Kind = SkillKind.Auto, Weapon = true, Effects = new[] { WeaponHit },
            Description = "Its attacks, in a combo on the one swing if it has Multiattack.",
        };
        yield return new()
        {
            Id = SoulLinkStrike, Name = "Soul-Link Strike", Kind = SkillKind.Action, Range = SkillRange.Long, Motion = "soul-link-strike",
            Effects = new[] { new Effect { Kind = EffectType.Damage, Weapon = true, Delivery = Delivery.Sure }, SureHit("1d8", DamageType.Radiant) },
            Description = "You and the Soul-Bound strike together: both weapons' damage and 1d8 radiant, a sure hit.",
        };
    }
}
