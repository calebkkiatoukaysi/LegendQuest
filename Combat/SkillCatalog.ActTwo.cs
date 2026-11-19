using System.Collections.Generic;
using LegendQuest.Combat.Realtime;

namespace LegendQuest.Combat;

/// <summary>Act 2's five advanced classes, Act 3's Crownsworn and the Hourwarden after it (10-classes.md §3.2 to §3.4), converted to real time by 11-combat.md §4.2 and §4.3.</summary>
/// <remarks>As SkillCatalog.ActOne.cs. The Hourwarden's kit is provisional, as its table says.</remarks>
public static partial class SkillCatalog
{
    /// <summary>The later advanced classes' rows: the Gilded Knight to the Hourwarden.</summary>
    private static IEnumerable<SkillDefinition> LaterClasses()
    {
        // Gilded Knight (Sir Corwin): sun paladin, Bulwark.
        yield return new()
        {
            Id = "gilded-knight-morning-strike", Name = "Morning Strike", Class = ClassId.GildedKnight, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit, SureHit("1d4", DamageType.Radiant) with { IfLanded = true } }, Description = "Weapon dice + Str, and 1d4 radiant on a hit.",
        };
        yield return new()
        {
            Id = "gilded-knight-sunburst", Name = "Sunburst", Class = ClassId.GildedKnight, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd2, Range = SkillRange.Long,
            Projectile = DamageType.Radiant,
            Effects = new[]
            {
                SpellHit("2d8", DamageType.Radiant, plus: true),
                Put(Condition("sunburst", "Blinded", ConditionId.Blinded, Turn), ifLanded: true) with { UndeadOnly = true },
            },
            Description = "2d8⁺ radiant; undead and vampires it hits are Blinded 3 s.",
        };
        yield return new()
        {
            Id = "gilded-knight-vigil", Name = "Vigil", Class = ClassId.GildedKnight, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3, Range = SkillRange.Short,
            Target = SkillTarget.Ally, Ai = AiUse.Defensive, AiValue = 5,
            Effects = new[] { Put(Buff("vigil", "Vigil", Turn, new StatMod(Stat.Ac, 2))) },
            Description = "An ally gains your shield's +2 AC for 3 s.",
        };
        yield return new()
        {
            Id = "gilded-knight-call-the-order", Name = "Call the Order", Class = ClassId.GildedKnight, Rank = 3, Kind = SkillKind.Bonus, Focus = 35, CooldownMs = OncePerFight,
            Range = SkillRange.Self, Target = SkillTarget.Self, Ai = AiUse.Summon, AiValue = 16,
            Effects = new[] { new Effect { Kind = EffectType.Summon, Id = "gilded-order-knight", On = EffectOn.Self } },
            Description = "A knight of the Order joins your side for the rest of the fight (one at a time).",
        };
        yield return new()
        {
            Id = "gilded-knight-where-the-dark-gathers", Name = "Where the Dark Gathers", Class = ClassId.GildedKnight, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.ToHit, 2, When: When.VsUndead), new StatMod(Stat.DamageDice, Dice: D("1d6"), Type: DamageType.Radiant, When: When.VsUndead) },
            Description = "+2 to hit and +1d6 against undead and vampires.",
        };
        yield return new()
        {
            Id = "gilded-knight-gilded-dawn", Name = "Gilded Dawn", Class = ClassId.GildedKnight, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, AiMinTargets = 2,
            Effects = new[]
            {
                SaveHit("6d8", DamageType.Radiant, Ability.Constitution, true) with { UndeadDisadvantage = true },
                Put(Debuff("sunscorched", "Sunscorched", 2 * Turn, new StatMod(Stat.HealingTakenPct, -100))) with { UndeadOnly = true },
            },
            Description = "Every foe: 6d8 radiant, Con save for half; undead and vampires save with disadvantage and can't heal for 6 s.",
        };
        yield return new()
        {
            Id = "gilded-knight-sorry-somewhere-useful", Name = "Sorry Somewhere Useful", Class = ClassId.GildedKnight, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.HealingPct, 25, When: When.OnOthers) }, Description = "Your heals and shields on others are 25% stronger.",
        };

        // Hemomancer (Valencia): blood mage, Blood.
        yield return new()
        {
            Id = "hemomancer-blood-lash", Name = "Blood Lash", Class = ClassId.Hemomancer, Kind = SkillKind.Auto, Range = SkillRange.Long, Projectile = DamageType.Necrotic,
            Effects = new[]
            {
                SpellHit("1d10", DamageType.Necrotic, plus: true),
                new Effect { Kind = EffectType.Heal, Flat = 2, On = EffectOn.Self, IfLanded = true },
            },
            Description = "1d10⁺ necrotic; a hit heals you 2.",
        };
        yield return new()
        {
            Id = "hemomancer-drain", Name = "Drain", Class = ClassId.Hemomancer, Focus = 25, CooldownMs = Cd2, Range = SkillRange.Long, Projectile = DamageType.Necrotic,
            Effects = new[] { SpellHit("2d8", DamageType.Necrotic, plus: true) with { Lifesteal = 1.0 } }, Description = "2d8⁺ necrotic; heal what you deal.",
        };
        yield return new()
        {
            Id = "hemomancer-blood-shield", Name = "Blood Shield", Class = ClassId.Hemomancer, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, HpCost = 5, CooldownMs = Cd3,
            Range = SkillRange.Long, Target = SkillTarget.Ally, Ai = AiUse.Buff,
            Effects = new[] { new Effect { Kind = EffectType.TempHp, Dice = D("2d8"), LevelTimes = 1, Aura = TempHpShield("blood-shield", "Blood Shield", ShieldMs) } },
            Description = "Pay 5 hit points: an ally gets temporary hit points 2d8 + level.",
        };
        yield return new()
        {
            Id = "hemomancer-given-freely", Name = "Given Freely", Class = ClassId.Hemomancer, Rank = 3, Kind = SkillKind.Bonus, HpCost = 10, CooldownMs = Cd3,
            Range = SkillRange.Short, Target = SkillTarget.Ally, Ai = AiUse.Heal,
            Effects = new[] { Heal("2d10") }, Description = "Pay 10 hit points (never below 1): heal an ally 2d10 + Cha. Gets a Downed ally up.",
        };
        yield return new()
        {
            Id = "hemomancer-the-old-enemy", Name = "The Old Enemy", Class = ClassId.Hemomancer, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.ToHit, -2, When: When.Daylight), new StatMod(Stat.ToHit, 2, When: When.Dark) },
            Description = "In daylight, -2 to hit; at night or underground, +2.",
        };
        yield return new()
        {
            Id = "hemomancer-crimson-tide", Name = "Crimson Tide", Class = ClassId.Hemomancer, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, AiMinTargets = 2,
            Effects = new[] { SaveHit("4d10", DamageType.Necrotic, Ability.Constitution, true) with { PartyHealShare = 0.25 } },
            Description = "Every foe: 4d10 necrotic, Con save for half; every ally heals a quarter of all it deals.",
        };
        yield return new()
        {
            Id = "hemomancer-forty-years-of-manners", Name = "Forty Years of Manners", Class = ClassId.Hemomancer, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Immune, Condition: ConditionId.Charmed), new StatMod(Stat.Immune, Condition: ConditionId.Frightened) },
            Description = "Charmed and Frightened have no effect on you.",
        };

        // Hivekeeper (Sister Orla): healer with a swarm, Devotion.
        yield return new()
        {
            Id = "hivekeeper-swarm", Name = "Swarm", Class = ClassId.Hivekeeper, Kind = SkillKind.Auto, Range = SkillRange.Long,
            Effects = new[]
            {
                SpellHit("1d6", DamageType.Piercing, plus: true),
                Put(new AuraDefinition { Id = "swarming", Name = "Swarming", DurationMs = Turn, TickDice = D("1"), TickType = DamageType.Piercing }, ifLanded: true),
            },
            Description = "1d6⁺ piercing, and 1 more 3 s later.",
        };
        yield return new()
        {
            Id = "hivekeeper-honey-salve", Name = "Honey Salve", Class = ClassId.Hivekeeper, Focus = 20, CooldownMs = Cd1, Range = SkillRange.Short, Target = SkillTarget.Ally,
            Ai = AiUse.Heal,
            Effects = new[] { Heal("1d8", plus: true), Put(Honey) }, Description = "Heal 1d8⁺ + Wis, and 2 more every 3 s for 9 s. Gets a Downed ally up.",
        };
        yield return new()
        {
            Id = "hivekeeper-smoke-pot", Name = "Smoke Pot", Class = ClassId.Hivekeeper, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3, Range = SkillRange.Long,
            Area = AreaShape.Row, Ai = AiUse.Control, AiValue = 4,
            Effects = new[] { Put(Debuff("smoked", "Smoked", Turn, new StatMod(Stat.Disadvantage))) },
            Description = "Every foe within 48 px of the target: disadvantage on attacks for 3 s.",
        };
        yield return new()
        {
            Id = "hivekeeper-angry-hive", Name = "Angry Hive", Class = ClassId.Hivekeeper, Rank = 3, Focus = 30, CooldownMs = Cd3, Range = SkillRange.Long, Area = AreaShape.Row,
            Effects = new[]
            {
                SureHit("2d6", DamageType.Piercing, plus: true),
                Put(Condition("hive-stung", "Poisoned", ConditionId.Poisoned, 2 * Turn), save: Ability.Constitution),
            },
            Description = "Every foe within 48 px of the target: 2d6⁺ piercing, and Con save or Poisoned 6 s.",
        };
        yield return new()
        {
            Id = "hivekeeper-thirty-summers", Name = "Thirty Summers", Class = ClassId.Hivekeeper, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.HotOnDowned, 2) }, Description = "Your healing over time can't be cleansed, and ticks twice as much on the Downed (and gets them up).",
        };
        yield return new()
        {
            Id = "hivekeeper-the-whole-hive", Name = "The Whole Hive", Class = ClassId.Hivekeeper, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, AiMinTargets = 2,
            Effects = new[]
            {
                SureHit("3d8", DamageType.Piercing),
                Heal("1d8", plus: true) with { On = EffectOn.AllAllies, Once = true },
                Put(Honey, EffectOn.AllAllies) with { Once = true },
            },
            Description = "Every foe: 3d8 piercing. Every ally: a Honey Salve.",
        };
        yield return new()
        {
            Id = "hivekeeper-they-know-me", Name = "They Know Me", Class = ClassId.Hivekeeper, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.BeastsIgnore) }, Description = "Beasts and swarms won't pick you out unless you've hurt them.",
        };

        // Glasswright (Lady Isadora's journal): illusionist of light, Wellspring.
        yield return new()
        {
            Id = "glasswright-prism-ray", Name = "Prism Ray", Class = ClassId.Glasswright, Kind = SkillKind.Auto, Range = SkillRange.Long, Projectile = DamageType.Radiant,
            Effects = new[] { SpellHit("1d8", DamageType.Radiant, plus: true), SpellHit("1d4", DamageType.Radiant, plus: true) with { On = EffectOn.RandomFoe } },
            Description = "1d8⁺ radiant, and a second ray at half (1d4⁺) at another foe, at random.",
        };
        yield return new()
        {
            Id = "glasswright-mirror-image", Name = "Mirror Image", Class = ClassId.Glasswright, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd4, Range = SkillRange.Self,
            Target = SkillTarget.Self, Ai = AiUse.Defensive, AiValue = 6,
            Effects = new[]
            {
                Put(new AuraDefinition { Id = "mirror-image", Name = "Mirror Image", Beneficial = true, UsedBy = AuraUse.AttackTaken, Charges = 3, Decoy = 50 }, EffectOn.Self),
            },
            Description = "Three images: each attack on you has an even chance of striking one instead (and missing), until all three are gone.",
        };
        yield return new()
        {
            Id = "glasswright-refract", Name = "Refract", Class = ClassId.Glasswright, Rank = 2, Kind = SkillKind.Reaction, Focus = 20, CooldownMs = Cd2, Range = SkillRange.Self,
            Target = SkillTarget.Self, Trigger = ReactionTrigger.RangedOnAlly, Ai = AiUse.Defensive, AiValue = 5,
            Effects = new[] { Arm }, Description = "For 4 s, the next ranged attack or spell at you or an ally goes at a random foe instead.",
        };
        yield return new()
        {
            Id = "glasswright-glasshouse", Name = "Glasshouse", Class = ClassId.Glasswright, Rank = 3, Focus = 30, CooldownMs = Cd3, Range = SkillRange.Long, Area = AreaShape.Row,
            Effects = new[]
            {
                Put(Condition("glasshouse", "Blinded", ConditionId.Blinded, Turn), save: Ability.Dexterity),
                SureHit("2d6", DamageType.Radiant),
            },
            Description = "Every foe within 48 px of the target: Dex save or Blinded 3 s; 2d6 radiant either way.",
        };
        yield return new()
        {
            Id = "glasswright-night-flowers", Name = "Night-Flowers", Class = ClassId.Glasswright, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Damage, 2, When: When.Dark) }, Description = "Your hits do +2 in dark places.",
        };
        yield return new()
        {
            Id = "glasswright-given-in-love", Name = "Given in Love", Class = ClassId.Glasswright, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllAllies, Ai = AiUse.Buff, AiValue = 8,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Rider, DurationMs = 5 * Turn, Rider = new[] { SureHit("4d6", DamageType.Radiant) with { Lifesteal = 0.5 } } },
            },
            Description = "Every ally's next hit in 15 s does +4d6 radiant, and heals them half of it.",
        };
        yield return new()
        {
            Id = "glasswright-choose-it-yourself", Name = "Choose It Yourself", Class = ClassId.Glasswright, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Immune, Condition: ConditionId.Charmed) },
            Procs = new[]
            {
                new Proc(ProcTrigger.OnAllyControlled, new[] { new Effect { Kind = EffectType.Cleanse, Cleanses = ControlConditions } }, OncePerFight: true),
            },
            Description = "Immune to Charmed; once a fight, the first control a foe puts on an ally (a stun, a freeze, a hold, a knockdown...) is shaken off at once.",
        };

        // Lamplighter (Mother Anselle): protector and healer, Devotion.
        yield return new()
        {
            Id = "lamplighter-lantern-swing", Name = "Lantern Swing", Class = ClassId.Lamplighter, Kind = SkillKind.Auto, Range = SkillRange.Melee,
            Effects = new[] { SpellHit("1d6", DamageType.Radiant, plus: true) with { AddCasting = true } }, Description = "1d6⁺ + Wis radiant.",
        };
        yield return new()
        {
            Id = "lamplighter-sanctuary", Name = "Sanctuary", Class = ClassId.Lamplighter, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Ai = AiUse.Defensive, AiValue = 6,
            Effects = new[] { Put(Buff("sanctuary", "Sanctuary", 2 * Turn, new StatMod(Stat.Sanctuary))) },
            Description = "For 6 s, a foe must pass a Wis save to pick them as its target.",
        };
        yield return new()
        {
            Id = "lamplighter-vespers", Name = "Vespers", Class = ClassId.Lamplighter, Rank = 2, Focus = 25, CooldownMs = Cd2, Range = SkillRange.Self, Target = SkillTarget.None,
            Area = AreaShape.AlliesBehind, Ai = AiUse.Heal,
            Effects = new[] { Heal("1d8", plus: true) }, Description = "You and every ally further from the foes than you: heal 1d8⁺ + Wis.",
        };
        yield return new()
        {
            Id = "lamplighter-reveal", Name = "Reveal", Class = ClassId.Lamplighter, Rank = 3, Kind = SkillKind.Bonus, Focus = 15, CooldownMs = Cd2, Range = SkillRange.Room,
            Target = SkillTarget.None, Area = AreaShape.AllFoes, Ai = AiUse.Control, AiValue = 3,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Dispel, On = EffectOn.AllFoes, Once = true },
                Put(MarkedFor("revealed", 2 * Turn), EffectOn.AllFoes) with { Once = true },
            },
            Description = "Every foe: out of hiding, its decoys and wards gone, and the next attack on it in 6 s has advantage.",
        };
        yield return new()
        {
            Id = "lamplighter-everyone-who-knocks", Name = "Everyone Who Knocks", Class = ClassId.Lamplighter, Rank = 4, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.OnAllyDropped, new[] { new Effect { Kind = EffectType.StayUp } }, OncePerFight: true) },
            Description = "The first time anyone of yours would be Downed each fight, they stay on 1 hit point instead.",
        };
        yield return new()
        {
            Id = "lamplighter-the-bell", Name = "The Bell", Class = ClassId.Lamplighter, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllAllies, Ai = AiUse.Heal,
            Effects = new[]
            {
                Heal("3d8", casting: false) with { On = EffectOn.AllAllies, Once = true },
                new Effect { Kind = EffectType.Cleanse, On = EffectOn.AllAllies, Once = true },
            },
            Description = "Every ally: heal 3d8 (the Downed get up), and every condition on them ends.",
        };
        yield return new()
        {
            Id = "lamplighter-bees-guard-the-honey", Name = "Bees Guard the Honey", Class = ClassId.Lamplighter, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.SanctuaryHeal, Dice: D("1d6")) }, Description = "Allies under your Sanctuary heal 1d6 whenever they're attacked.",
        };

        // Crownsworn (Elara): commander and duellist, Momentum.
        yield return new()
        {
            Id = "crownsworn-riposte-strike", Name = "Riposte Strike", Class = ClassId.Crownsworn, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit, Put(Buff("on-guard", "On Guard", Turn, new StatMod(Stat.Ac, 1)), EffectOn.Self) },
            Description = "Weapon dice + Dex; +1 AC for 3 s.",
        };
        yield return new()
        {
            Id = "crownsworn-command", Name = "Command", Class = ClassId.Crownsworn, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd2, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Ai = AiUse.Utility, AiValue = 6,
            Effects = new[] { new Effect { Kind = EffectType.Reset } },
            Description = "An ally acts again: their GCD ends now, and the next action skill they use starts no cooldown.",
        };
        yield return new()
        {
            Id = "crownsworn-told-rudely", Name = "Told Rudely", Class = ClassId.Crownsworn, Rank = 2, Kind = SkillKind.Bonus, Focus = 15, CooldownMs = Cd3,
            Range = SkillRange.Long, Ai = AiUse.Taunt, AiValue = 4,
            Effects = new[] { new Effect { Kind = EffectType.Taunt, DurationMs = Turn }, Put(MarkedFor("told-rudely", Turn)) },
            Description = "A foe is Marked and Taunted (it must attack you) for 3 s.",
        };
        yield return new()
        {
            Id = "crownsworn-rally", Name = "Rally", Class = ClassId.Crownsworn, Rank = 3, Focus = 30, CooldownMs = Cd3, Range = SkillRange.Room, Target = SkillTarget.None,
            Area = AreaShape.AllAllies, Ai = AiUse.Heal,
            Effects = new[]
            {
                Heal("1d8", plus: true, casting: false) with { On = EffectOn.AllAllies, Once = true, StandingOnly = true },
                new Effect { Kind = EffectType.Heal, Flat = 1, DownedOnly = true, On = EffectOn.AllAllies, Once = true },
            },
            Description = "Every ally standing heals 1d8⁺; every Downed ally gets up on 1.",
        };
        yield return new()
        {
            Id = "crownsworn-no-procedure", Name = "No Procedure", Class = ClassId.Crownsworn, Rank = 4, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.FightStart, new[] { PartyFocus(10), new Effect { Kind = EffectType.Ready, On = EffectOn.Self } }) },
            Description = "The party starts every fight with 10 more Focus, and your first swing comes at once (it was initiative).",
        };
        yield return new()
        {
            Id = "crownsworn-my-fathers-sword", Name = "My Father's Sword", Class = ClassId.Crownsworn, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Weapon = true, Range = SkillRange.Melee,
            Effects = new[]
            {
                WeaponHit with { WeaponTimes = 4 },
                SureHit("4d8", DamageType.Radiant) with { IfLanded = true },
                new Effect { Kind = EffectType.Strike, ByAllies = true },
            },
            Description = "Weapon dice × 4 + 4d8 radiant; every ally within reach of the foe gets a free Strike on it.",
        };
        yield return new()
        {
            Id = "crownsworn-somebody-allowed-to-say-no", Name = "Somebody Allowed to Say No", Class = ClassId.Crownsworn, Rank = 10, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.OnFoeBigMove, new[] { new Effect { Kind = EffectType.Interrupt } }, OncePerFight: true) },
            Description = "Once a fight, the first big move a foe winds up (a wind-up of 1 s or more) is stopped before it lands.",
        };

        // Hourwarden (Archmage Thessaly, after Act 3): time mage, Wellspring. Provisional.
        yield return new()
        {
            Id = "hourwarden-second-hand", Name = "Second Hand", Class = ClassId.Hourwarden, Kind = SkillKind.Auto, Range = SkillRange.Long, Projectile = DamageType.Force,
            Effects = new[] { SpellHit("1d8", DamageType.Force, plus: true) }, Description = "1d8⁺ force from the staff's head, the swing a beat behind itself.",
        };
        yield return new()
        {
            Id = "hourwarden-rewind", Name = "Rewind", Class = ClassId.Hourwarden, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd3, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Ai = AiUse.Heal,
            Effects = new[] { new Effect { Kind = EffectType.Rewind, DurationMs = 2 * Turn } },
            Description = "An ally's hit points go back to what they were 6 s ago, if that's more.",
        };
        yield return new()
        {
            Id = "hourwarden-haste", Name = "Haste", Class = ClassId.Hourwarden, Rank = 2, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd3, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Ai = AiUse.Buff, AiValue = 4,
            Effects = new[] { Put(Buff("hasted", "Haste", 3 * Turn, new StatMod(Stat.Haste, 30))) }, Description = "An ally's haste +30% for 9 s.",
        };
        yield return new()
        {
            Id = "hourwarden-stop", Name = "Stop", Class = ClassId.Hourwarden, Rank = 3, Focus = 30, CooldownMs = Cd3, Range = SkillRange.Long, Ai = AiUse.Control, AiValue = 6,
            Effects = new[] { Put(Condition("stopped", "Stopped", ConditionId.Frozen, Turn), save: Ability.Wisdom) },
            Description = "Wis save or Frozen 3 s (it can't act, and only areas reach it).",
        };
        yield return new()
        {
            Id = "hourwarden-echo-strike", Name = "Echo Strike", Class = ClassId.Hourwarden, Rank = 4, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.OnSkillUsed, new[] { new Effect { Kind = EffectType.Echo } }, ChancePct: 20) },
            Description = "A skill you use has a one-in-five chance to happen again at half strength, 0.4 s after.",
        };
        yield return new()
        {
            Id = "hourwarden-bleed-the-clock", Name = "Bleed the Clock", Class = ClassId.Hourwarden, Rank = 5, Focus = 70, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, Ai = AiUse.Control, AiValue = 10, AiMinTargets = 2,
            Effects = new[]
            {
                Put(Condition("clock-stopped", "Stopped", ConditionId.Frozen, 4000), save: Ability.Wisdom),
                Put(Condition("clock-slowed", "Slowed", ConditionId.Slowed, 2 * Turn) with { Mods = new[] { new StatMod(Stat.Haste, -50) } }) with { IfSaved = true },
            },
            Description = "Every foe: Wis save or Frozen 4 s; those who save are Slowed (half speed and haste) for 6 s.",
        };
        yield return new()
        {
            Id = "hourwarden-stolen-minute", Name = "Stolen Minute", Class = ClassId.Hourwarden, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.CooldownRate, 10) }, Description = "Your cooldowns run 10% faster.",
        };
    }
}
