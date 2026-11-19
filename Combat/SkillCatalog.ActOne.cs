using System.Collections.Generic;
using LegendQuest.Combat.Realtime;

namespace LegendQuest.Combat;

/// <summary>Act 1's ten advanced classes' kits (10-classes.md §3.1), converted to real time by 11-combat.md §4.2 and §4.3.</summary>
/// <remarks>
/// As SkillCatalog.Base.cs: each line of a table is one row, in the table's order, "until your next
/// turn" is 3 s, "N turns" N × 3 s, "the back row" a 40 px knockback, "the front row" the foes within
/// 60 px in front, "any row" long reach. Where a line needs more than that, the row's Description
/// says what it does now.
/// </remarks>
public static partial class SkillCatalog
{
    /// <summary>Act 1's advanced classes' rows: the Watchblade to the Wayfarer.</summary>
    private static IEnumerable<SkillDefinition> ActOneClasses()
    {
        // Watchblade (Captain Hale): defender and controller, Momentum, double threat.
        yield return new()
        {
            Id = "watchblade-cudgel-blow", Name = "Cudgel Blow", Class = ClassId.Watchblade, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit, Put(Knockdown("cudgelled"), ifLanded: true) with { IfHitByAlly = true } },
            Description = "Weapon dice + Str; a hit on a foe an ally struck in the last 3 s knocks it down.",
        };
        yield return new()
        {
            Id = "watchblade-hold-the-line", Name = "Hold the Line", Class = ClassId.Watchblade, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3,
            Range = SkillRange.Self, Target = SkillTarget.None, Area = AreaShape.Front, Ai = AiUse.Taunt, AiValue = 4,
            Effects = new[]
            {
                Put(Buff("hold-the-line", "Hold the Line", Turn, new StatMod(Stat.Ac, 2)), EffectOn.Self),
                new Effect { Kind = EffectType.Taunt, DurationMs = Turn },
            },
            Description = "Every foe within 60 px in front of you must attack you for 3 s; +2 AC for 3 s.",
        };
        yield return new()
        {
            Id = "watchblade-arrest", Name = "Arrest", Class = ClassId.Watchblade, Rank = 2, Focus = 25, CooldownMs = Cd3, Range = SkillRange.Melee,
            Ai = AiUse.Control, AiValue = 6,
            Effects = new[]
            {
                Put(Condition("arrested", "Restrained", ConditionId.Restrained, 2 * Turn, Ability.Strength), save: Ability.Strength),
                new Effect { Kind = EffectType.TakeOut, MaxHpShare = 0.25 },
            },
            Description = "Str save or Restrained 6 s (a Str save every 3 s ends it). In a brawl or a nonlethal fight, a foe at a quarter health or less is taken out of it.",
        };
        yield return new()
        {
            Id = "watchblade-whistle-for-the-watch", Name = "Whistle for the Watch", Class = ClassId.Watchblade, Rank = 3, Kind = SkillKind.Bonus, Focus = 30,
            CooldownMs = OncePerFight, Range = SkillRange.Self, Target = SkillTarget.Self, Ai = AiUse.Summon, AiValue = 14,
            Effects = new[] { new Effect { Kind = EffectType.Summon, Id = "watchman", On = EffectOn.Self } },
            Description = "A Watchman runs in and fights on your side for the rest of the fight (one at a time).",
        };
        yield return new()
        {
            Id = "watchblade-twenty-years-on-the-beat", Name = "Twenty Years on the Beat", Class = ClassId.Watchblade, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Save, 2) }, Description = "+2 to saves (and advantage on Insight in conversations).",
        };
        yield return new()
        {
            Id = "watchblade-shield-wall", Name = "Shield Wall", Class = ClassId.Watchblade, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllAllies, Ai = AiUse.Buff, AiValue = 6,
            Effects = new[] { Put(Buff("shield-wall", "Shield Wall", 2 * Turn, new StatMod(Stat.DamageTakenPct, -50))) },
            Description = "Everyone on your side takes half damage for 6 s.",
        };
        yield return new()
        {
            Id = "watchblade-keeper-of-the-peace", Name = "Keeper of the Peace", Class = ClassId.Watchblade, Rank = 10, Kind = SkillKind.Passive,
            AllyMods = new[] { new StatMod(Stat.DamageDice, Dice: D("1d6"), When: When.VsHeldBySource) },
            Description = "Foes you've Restrained or knocked down take +1d6 from your side's hits.",
        };

        // Netcaster (Old Tomas): support and control from the back, Wild.
        yield return new()
        {
            Id = "netcaster-gaff", Name = "Gaff", Class = ClassId.Netcaster, Kind = SkillKind.Auto, Range = SkillRange.Reach,
            Effects = new[] { SpellHit("1d6", DamageType.Piercing, plus: true) }, Description = "1d6⁺ piercing, at a reach of 42 px.",
        };
        yield return new()
        {
            Id = "netcaster-cast-the-net", Name = "Cast the Net", Class = ClassId.Netcaster, Focus = 25, CooldownMs = Cd3, Range = SkillRange.Long, Area = AreaShape.Row,
            Ai = AiUse.Control, AiValue = 5,
            Effects = new[] { Put(Condition("netted", "Netted", ConditionId.Restrained, 2 * Turn), save: Ability.Dexterity) },
            Description = "Every foe within 48 px of the target: Dex save or Restrained 6 s.",
        };
        yield return new()
        {
            Id = "netcaster-mend", Name = "Mend", Class = ClassId.Netcaster, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd1, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Ai = AiUse.Heal,
            Effects = new[] { Heal("1d6", plus: true), new Effect { Kind = EffectType.Cleanse, Cleanses = new[] { ConditionId.Restrained } } },
            Description = "Heal 1d6⁺ + Wis, and end Restrained. Gets a Downed ally up.",
        };
        yield return new()
        {
            Id = "netcaster-haul-in", Name = "Haul In", Class = ClassId.Netcaster, Rank = 3, Focus = 25, CooldownMs = Cd2, Range = SkillRange.Long,
            Ai = AiUse.Control, AiValue = 5,
            Effects = new[] { new Effect { Kind = EffectType.Pull }, Put(Knockdown("hauled-in")) },
            Description = "Haul a foe up to 220 px away to your reach, and knock it down.",
        };
        yield return new()
        {
            Id = "netcaster-weather-eye", Name = "Weather Eye", Class = ClassId.Netcaster, Rank = 4, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.FightStart, new[] { PartyFocus(10) }) },
            Description = "The party starts every fight with 10 more Focus (it was +2 initiative).",
        };
        yield return new()
        {
            Id = "netcaster-high-tide", Name = "High Tide", Class = ClassId.Netcaster, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, AiMinTargets = 2,
            Effects = new[]
            {
                SureHit("3d8", DamageType.Cold),
                Put(Knockdown("high-tide-down"), save: Ability.Strength),
                Put(new AuraDefinition
                {
                    Id = "high-tide", Name = "High Tide", Beneficial = true, DurationMs = 3 * Turn, UsedBy = AuraUse.NextAttack, Mods = new[] { new StatMod(Stat.Advantage) },
                }, EffectOn.AllAllies) with { Once = true },
            },
            Description = "Every foe: 3d8 cold, and Str save or knocked down. The party's next attacks in 9 s have advantage.",
        };
        yield return new()
        {
            Id = "netcaster-knots-hold", Name = "Knots Hold", Class = ClassId.Netcaster, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.RestrainedMs, Turn) }, Description = "Restrained you put on lasts 3 s longer.",
        };

        // Deckhand (Captain Odelle): skirmisher, Momentum.
        yield return new()
        {
            Id = "deckhand-cutlass", Name = "Cutlass", Class = ClassId.Deckhand, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit }, Description = "Weapon dice + Dex.",
        };
        yield return new()
        {
            Id = "deckhand-belaying-pin", Name = "Belaying Pin", Class = ClassId.Deckhand, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3, Range = SkillRange.Melee,
            Ai = AiUse.Control, AiValue = 7,
            Effects = new[] { Put(Condition("pinned", "Stunned", ConditionId.Stunned, Turn), save: Ability.Constitution) },
            Description = "Con save or Stunned 3 s.",
        };
        yield return new()
        {
            Id = "deckhand-rum-ration", Name = "Rum Ration", Class = ClassId.Deckhand, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3,
            Range = SkillRange.Short, Target = SkillTarget.Ally, Ai = AiUse.Heal,
            Effects = new[]
            {
                Heal("2d4", plus: true, casting: false),
                Put(new AuraDefinition
                {
                    Id = "rum-ration", Name = "Rum Ration", Beneficial = true, DurationMs = 5 * Turn, UsedBy = AuraUse.NextAttack,
                    Mods = new[] { new StatMod(Stat.ToHitDice, Dice: D("1d4")) },
                }),
            },
            Description = "Heal 2d4⁺, and +1d4 to their next attack in 15 s. Gets a Downed ally up.",
        };
        yield return new()
        {
            Id = "deckhand-swing-across", Name = "Swing Across", Class = ClassId.Deckhand, Rank = 3, Focus = 30, CooldownMs = Cd2, Weapon = true, Range = SkillRange.Long,
            Effects = new[] { new Effect { Kind = EffectType.Teleport, On = EffectOn.Self, Once = true }, WeaponHit with { Dice = D("1d6") } },
            Description = "Swing on a line to a foe up to 220 px away and land behind it: weapon dice + 1d6.",
        };
        yield return new()
        {
            Id = "deckhand-sea-legs", Name = "Sea Legs", Class = ClassId.Deckhand, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Immune, Condition: ConditionId.Prone), new StatMod(Stat.Ac, 1) }, Description = "Can't be knocked down; +1 AC.",
        };
        yield return new()
        {
            Id = "deckhand-broadside", Name = "Broadside", Class = ClassId.Deckhand, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, AiMinTargets = 2,
            Effects = new[]
            {
                SaveHit("4d10", DamageType.Fire, Ability.Dexterity, true) with { Place = Place.Outdoors },
                SureHit("3d8", DamageType.Piercing) with { Place = Place.Indoors, WithinPx = Battle.FrontPx },
            },
            Description = "Outdoors, the Gull's Wager fires from off-screen: every foe takes 4d10 fire, Dex save for half. Indoors it's Grapeshot: 3d8 to every foe within 60 px of you.",
        };
        yield return new()
        {
            Id = "deckhand-captains-share", Name = "Captain's Share", Class = ClassId.Deckhand, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.GoldPct, 20) }, Description = "+20% gold from fights.",
        };

        // Lodge Ranger (Ranger Tamsin): ranged damage with a hawk, Opportunist.
        yield return new()
        {
            Id = "lodge-ranger-shot", Name = "Shot", Class = ClassId.LodgeRanger, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit }, Description = "Weapon dice + Dex.",
        };
        yield return new()
        {
            Id = "lodge-ranger-hunters-mark", Name = "Hunter's Mark", Class = ClassId.LodgeRanger, Kind = SkillKind.Bonus, Focus = 20, Range = SkillRange.Long,
            Concentration = true, Ai = AiUse.Control, AiValue = 6,
            Effects = new[]
            {
                Put(Debuff("hunters-mark", "Hunter's Mark", 10 * Turn, new StatMod(Stat.TakenFromSource, Dice: D("1d6"))) with { CountsAsMarked = true }),
            },
            Description = "Marked for 30 s: your hits on it do +1d6 (concentration).",
        };
        yield return new()
        {
            Id = "lodge-ranger-hawk-strike", Name = "Hawk Strike", Class = ClassId.LodgeRanger, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd2,
            Range = SkillRange.Long,
            Effects = new[] { SpellHit("1d6", DamageType.Piercing, plus: true), new Effect { Kind = EffectType.Interrupt, IfLanded = true } },
            Description = "Your hawk stoops: 1d6⁺ piercing, and a hit stops what the foe is winding up.",
        };
        yield return new()
        {
            Id = "lodge-ranger-volley", Name = "Volley", Class = ClassId.LodgeRanger, Rank = 3, Focus = 30, CooldownMs = Cd3, Weapon = true, Range = SkillRange.Long,
            Area = AreaShape.Row, AiMinTargets = 2,
            Effects = new[] { new Effect { Kind = EffectType.Damage, Weapon = true, Delivery = Delivery.Save, Save = Ability.Dexterity, Half = true } },
            Description = "Weapon dice to every foe within 48 px of the target, Dex save for half.",
        };
        yield return new()
        {
            Id = "lodge-ranger-count-the-ground", Name = "Count the Ground", Class = ClassId.LodgeRanger, Rank = 4, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.FightStart, new[] { new Effect { Kind = EffectType.Ready, On = EffectOn.AllAllies } }) },
            Description = "Nobody catches the party unready: everyone's first swing of a fight comes at once.",
        };
        yield return new()
        {
            Id = "lodge-ranger-rain-of-arrows", Name = "Rain of Arrows", Class = ClassId.LodgeRanger, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllFoes, Projectile = DamageType.Piercing,
            Effects = new[] { SaveHit("4d8", DamageType.Piercing, Ability.Dexterity, true) with { TwiceIfMarked = true } },
            Description = "Every foe: 4d8 piercing, Dex save for half; Marked foes take it twice.",
        };
        yield return new()
        {
            Id = "lodge-ranger-old-growth", Name = "Old Growth", Class = ClassId.LodgeRanger, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.ToHit, 2, When: When.VsMarked) }, Description = "+2 to hit Marked foes.",
        };

        // Guild Battlemage (Archmage Thessaly): battlefield caster, Wellspring.
        yield return new()
        {
            Id = "battlemage-force-bolt", Name = "Force Bolt", Class = ClassId.Battlemage, Kind = SkillKind.Auto, Range = SkillRange.Long, Projectile = DamageType.Force,
            Effects = new[] { SpellHit("1d8", DamageType.Force, plus: true), new Effect { Kind = EffectType.Knockback, Distance = 40f, IfLanded = true, WithinPx = Ranges.Melee + 10f } },
            Description = "1d8⁺ force; a foe in melee with you is pushed 40 px back.",
        };
        yield return new()
        {
            Id = "battlemage-ward", Name = "Ward", Class = ClassId.Battlemage, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd2, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Ai = AiUse.Buff,
            Effects = new[] { new Effect { Kind = EffectType.TempHp, Dice = D("1d10"), AddCasting = true, LevelTimes = 1, Aura = TempHpShield("ward", "Ward", ShieldMs) } },
            Description = "Temporary hit points 1d10 + Int + level.",
        };
        yield return new()
        {
            Id = "battlemage-counterspell", Name = "Counterspell", Class = ClassId.Battlemage, Rank = 2, Kind = SkillKind.Reaction, Focus = 30, CooldownMs = Cd3,
            Range = SkillRange.Self, Target = SkillTarget.Self, Trigger = ReactionTrigger.FoeCast, Ai = AiUse.Defensive, AiValue = 8,
            Effects = new[] { Arm, new Effect { Kind = EffectType.Interrupt, On = EffectOn.Attacker } },
            Description = "For 4 s, the next cast or wind-up a foe starts within 220 px fails; armed while one's under way, it stops that one.",
        };
        yield return new()
        {
            Id = "battlemage-chain-lightning", Name = "Chain Lightning", Class = ClassId.Battlemage, Rank = 3, Focus = 35, CooldownMs = Cd3, Range = SkillRange.Long,
            Area = AreaShape.Chain, Count = 2,
            Effects = new[] { SpellHit("3d8", DamageType.Lightning) with { OthersShare = 0.5 } },
            Description = "3d8 lightning to the foe, then half to each of 2 more, each the nearest to the last within 100 px.",
        };
        yield return new()
        {
            Id = "battlemage-spire-training", Name = "Spire Training", Class = ClassId.Battlemage, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.KeepConcentration) }, Description = "Damage can't break your concentration.",
        };
        yield return new()
        {
            Id = "battlemage-spire-lance", Name = "Spire Lance", Class = ClassId.Battlemage, Rank = 5, Focus = 70, CooldownMs = OncePerFight, Ultimate = true, CastMs = 1000,
            Range = SkillRange.Long, Area = AreaShape.Line,
            Effects = new[] { SureHit("10d6", DamageType.Force) with { IgnoreResistance = true } },
            Description = "After a 1 s cast, a lance down a line from you through the target: 10d6 force to every foe in it; resistance doesn't count.",
        };
        yield return new()
        {
            Id = "battlemage-archmages-patience", Name = "Archmage's Patience", Class = ClassId.Battlemage, Rank = 10, Kind = SkillKind.Passive,
            Procs = new[]
            {
                new Proc(ProcTrigger.FightStart, new[]
                {
                    Put(new AuraDefinition
                    {
                        Id = "archmages-patience", Name = "Archmage's Patience", Beneficial = true, UsedBy = AuraUse.NextSkill, Mods = new[] { new StatMod(Stat.FreeSkill) },
                    }, EffectOn.Self),
                }),
            },
            Description = "Your first skill each fight costs no Focus.",
        };

        // Ashwalker (Brenna): bruiser who fights in ash, Momentum.
        yield return new()
        {
            Id = "ashwalker-ember-edge", Name = "Ember Edge", Class = ClassId.Ashwalker, Kind = SkillKind.Auto, Weapon = true,
            Effects = new[] { WeaponHit, SureHit("1d4", DamageType.Fire) with { IfLanded = true } }, Description = "Weapon dice + Str, and 1d4 fire on a hit.",
        };
        yield return new()
        {
            Id = "ashwalker-ash-cloud", Name = "Ash Cloud", Class = ClassId.Ashwalker, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd3, Range = SkillRange.Self,
            Target = SkillTarget.None, Area = AreaShape.Front, Ai = AiUse.Control, AiValue = 4,
            Effects = new[] { Put(Condition("ash-cloud", "Blinded", ConditionId.Blinded, Turn), save: Ability.Constitution) },
            Description = "Every foe within 60 px in front of you: Con save or Blinded 3 s.",
        };
        yield return new()
        {
            Id = "ashwalker-endure", Name = "Endure", Class = ClassId.Ashwalker, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3, Range = SkillRange.Self,
            Target = SkillTarget.Self, Ai = AiUse.Defensive, AiValue = 6,
            Effects = new[] { Put(Buff("endure", "Endure", Turn, new StatMod(Stat.Resist)), EffectOn.Self) },
            Description = "Resistance to all damage for 3 s.",
        };
        yield return new()
        {
            Id = "ashwalker-burn-the-fields", Name = "Burn the Fields", Class = ClassId.Ashwalker, Rank = 3, Focus = 30, CooldownMs = Cd3, Range = SkillRange.Short,
            Area = AreaShape.Row,
            Effects = new[]
            {
                SureHit("2d6", DamageType.Fire),
                Put(Condition("burn-the-fields", "Burning", ConditionId.Burning, 3 * Turn) with { TickDice = D("1d6"), TickType = DamageType.Fire }),
            },
            Description = "Every foe within 48 px of the target: 2d6 fire now, and 1d6 every 3 s for 9 s.",
        };
        yield return new()
        {
            Id = "ashwalker-doomward-born", Name = "Doomward Born", Class = ClassId.Ashwalker, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Resist, Type: DamageType.Fire), new StatMod(Stat.SaveAdvantage, Type: DamageType.Necrotic) },
            Description = "Resistance to fire; advantage on saves against necrotic.",
        };
        yield return new()
        {
            Id = "ashwalker-last-harvest", Name = "Last Harvest", Class = ClassId.Ashwalker, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Weapon = true, Range = SkillRange.Melee, Area = AreaShape.Neighbours,
            Effects = new[] { WeaponHit with { WeaponTimes = 3, Lifesteal = 0.5 }, SureHit("3d6", DamageType.Fire) with { IfLanded = true, Lifesteal = 0.5 } },
            Description = "The foe and every foe within 36 px of it: weapon dice × 3 + 3d6 fire; heal half the damage you do.",
        };
        yield return new()
        {
            Id = "ashwalker-nothing-left-to-lose", Name = "Nothing Left to Lose", Class = ClassId.Ashwalker, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.ToHit, 2, When: When.BelowHalf), new StatMod(Stat.DamageDice, Dice: D("1d6"), When: When.BelowHalf) },
            Description = "Below half health, +2 to hit and +1d6 damage.",
        };

        // Ironvein Bulwark (Brannock): the best defender, Bulwark, double threat.
        yield return new()
        {
            Id = "bulwark-shield-bash", Name = "Shield Bash", Class = ClassId.Bulwark, Kind = SkillKind.Auto, Range = SkillRange.Melee,
            Effects = new[]
            {
                SpellHit("1d6", DamageType.Bludgeoning, plus: true) with { AddAbility = Ability.Strength },
                new Effect { Kind = EffectType.Knockback, Distance = 40f, IfLanded = true },
            },
            Description = "1d6⁺ + Str bludgeoning, and a hit knocks the foe 40 px back.",
        };
        yield return new()
        {
            Id = "bulwark-cover", Name = "Cover", Class = ClassId.Bulwark, Kind = SkillKind.Reaction, Focus = 15, CooldownMs = Cd1, Range = SkillRange.Long,
            Target = SkillTarget.Ally, Trigger = ReactionTrigger.AttackOnAlly, Ai = AiUse.Defensive, AiValue = 5,
            Effects = new[] { Arm }, Description = "For 4 s, the next attack on an ally comes at you instead.",
        };
        yield return new()
        {
            Id = "bulwark-hold", Name = "Hold", Class = ClassId.Bulwark, Rank = 2, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd3, Range = SkillRange.Self,
            Target = SkillTarget.None, Area = AreaShape.AlliesBehind, Ai = AiUse.Defensive, AiValue = 3,
            Effects = new[] { Put(Buff("held-back", "Hold", Turn, new StatMod(Stat.Untargetable))) with { NotSelf = true } },
            Description = "For 3 s, foes can't pick out anyone of yours further from them than you: they turn to you.",
        };
        yield return new()
        {
            Id = "bulwark-stone-skin", Name = "Stone Skin", Class = ClassId.Bulwark, Rank = 3, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd4, Range = SkillRange.Self,
            Target = SkillTarget.Self, Ai = AiUse.Defensive, AiValue = 7,
            Effects = new[]
            {
                Put(Buff("stone-skin", "Stone Skin", 2 * Turn, new StatMod(Stat.Resist, Type: DamageType.Slashing), new StatMod(Stat.Resist, Type: DamageType.Piercing),
                    new StatMod(Stat.Resist, Type: DamageType.Bludgeoning)), EffectOn.Self),
            },
            Description = "Resistance to slashing, piercing and bludgeoning for 6 s.",
        };
        yield return new()
        {
            Id = "bulwark-forty-one-names", Name = "Forty-One Names", Class = ClassId.Bulwark, Rank = 4, Kind = SkillKind.Passive,
            Procs = new[]
            {
                new Proc(ProcTrigger.OnAllyDropped, new[]
                {
                    Put(new AuraDefinition { Id = "forty-one-names", Name = "Forty-One Names", Beneficial = true, Stacks = true, MaxStacks = 3, Mods = new[] { new StatMod(Stat.Ac, 1) } },
                        EffectOn.Self),
                }),
            },
            Description = "+1 AC for each ally who has gone Downed this fight, up to +3.",
        };
        yield return new()
        {
            Id = "bulwark-nobody-left-behind", Name = "Nobody Left Behind", Class = ClassId.Bulwark, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllAllies, Ai = AiUse.Heal,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Heal, MaxHpShare = 0.25, DownedOnly = true, On = EffectOn.AllAllies, Once = true },
                Put(Buff("covered", "Covered", Turn, new StatMod(Stat.Covered)), EffectOn.OtherAllies) with { Once = true },
            },
            Description = "Every Downed ally gets up on a quarter of their hit points; for 3 s every attack on an ally comes at you instead.",
        };
        yield return new()
        {
            Id = "bulwark-staying-behind", Name = "Staying Behind", Class = ClassId.Bulwark, Rank = 10, Kind = SkillKind.Passive,
            Procs = new[] { new Proc(ProcTrigger.OnDropped, new[] { new Effect { Kind = EffectType.StayUp } }, OncePerFight: true) },
            Description = "Once a fight, drop to 1 hit point instead of 0.",
        };

        // Ember Mender (Mira): healer who cauterises, Devotion.
        yield return new()
        {
            Id = "ember-mender-cinder", Name = "Cinder", Class = ClassId.EmberMender, Kind = SkillKind.Auto, Range = SkillRange.Long, Projectile = DamageType.Fire,
            Effects = new[] { SpellHit("1d8", DamageType.Fire, plus: true) }, Description = "1d8⁺ fire.",
        };
        yield return new()
        {
            Id = "ember-mender-cauterise", Name = "Cauterise", Class = ClassId.EmberMender, Focus = 25, CooldownMs = Cd1, Range = SkillRange.Short, Target = SkillTarget.Ally,
            Ai = AiUse.Heal,
            Effects = new[] { Heal("2d6", plus: true), new Effect { Kind = EffectType.Cleanse, Cleanses = new[] { ConditionId.Bleeding, ConditionId.Poisoned } } },
            Description = "Heal 2d6⁺ + Int; ends Bleeding and Poisoned. Gets a Downed ally up.",
        };
        yield return new()
        {
            Id = "ember-mender-read-the-wound", Name = "Read the Wound", Class = ClassId.EmberMender, Rank = 2, Kind = SkillKind.Bonus, Focus = 15, CooldownMs = Cd2,
            Range = SkillRange.Long, Ai = AiUse.Control, AiValue = 4,
            Effects = new[] { Put(Debuff("read-the-wound", "Read", Turn, new StatMod(Stat.AttackersToHit, 2))) },
            Description = "Read a foe (its frame shows its resistances and what it's winding up): attacks on it have +2 to hit for 3 s.",
        };
        yield return new()
        {
            Id = "ember-mender-warm-hands", Name = "Warm Hands", Class = ClassId.EmberMender, Rank = 3, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd3,
            Range = SkillRange.Self, Target = SkillTarget.None, Area = AreaShape.AlliesNear, Ai = AiUse.Heal,
            Effects = new[] { Heal("1d8") }, Description = "Every ally within 60 px of you, you included: heal 1d8 + Int.",
        };
        yield return new()
        {
            Id = "ember-mender-index-of-remedies", Name = "Index of Remedies", Class = ClassId.EmberMender, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.PotionPct, 50) }, Description = "Potions you drink heal 50% more.",
        };
        yield return new()
        {
            Id = "ember-mender-rekindle", Name = "Rekindle", Class = ClassId.EmberMender, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllAllies, Ai = AiUse.Heal,
            Effects = new[]
            {
                Heal("4d8", casting: false) with { On = EffectOn.AllAllies, Once = true },
                Put(Buff("rekindled", "Rekindled", 2 * Turn, new StatMod(Stat.ToHit, 2)), EffectOn.AllAllies) with { Once = true },
            },
            Description = "Every ally: heal 4d8 (the Downed get up), and +2 to hit for 6 s.",
        };
        yield return new()
        {
            Id = "ember-mender-notes-in-the-margin", Name = "Notes in the Margin", Class = ClassId.EmberMender, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.HealingPct, 100, When: When.TargetBelowQuarter) }, Description = "Your heals on someone below a quarter of their hit points are doubled.",
        };

        // Lanerunner (Kael): thief with twin blades, Opportunist.
        yield return new()
        {
            Id = "lanerunner-twin-cuts", Name = "Twin Cuts", Class = ClassId.Lanerunner, Kind = SkillKind.Auto, Weapon = true, Strikes = 2,
            Effects = new[] { WeaponHit with { Sneak = true } },
            Description = "Two hits of weapon dice + Dex; Sneak Attack (as the Shadowstep's) on one of them.",
        };
        yield return new()
        {
            Id = "lanerunner-pick-pocket", Name = "Pick Pocket", Class = ClassId.Lanerunner, Kind = SkillKind.Bonus, Focus = 15, Range = SkillRange.Melee, OncePerFoe = true,
            Ai = AiUse.Utility, AiValue = 3,
            Effects = new[] { new Effect { Kind = EffectType.Gold, Amount = 20 } },
            Description = "Once a foe: lift a fifth of its loot's gold (or a potion it carries), paid out when the fight's won.",
        };
        yield return new()
        {
            Id = "lanerunner-rooftops", Name = "Rooftops", Class = ClassId.Lanerunner, Rank = 2, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd2, Range = SkillRange.Self,
            Target = SkillTarget.Self, Ai = AiUse.Defensive, AiValue = 4,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Dash, On = EffectOn.Self, Distance = 60f },
                Put(new AuraDefinition { Id = "rooftops", Name = "Rooftops", Beneficial = true, DurationMs = 2 * Turn, UsedBy = AuraUse.AttackTaken, Decoy = 100 }, EffectOn.Self),
            },
            Description = "Dash 60 px clear; the next attack on you in 6 s misses.",
        };
        yield return new()
        {
            Id = "lanerunner-low-blow", Name = "Low Blow", Class = ClassId.Lanerunner, Rank = 3, Focus = 25, CooldownMs = Cd2, Weapon = true, Range = SkillRange.Short,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Teleport, On = EffectOn.Self, Once = true },
                WeaponHit with { Dice = D("2d6") },
                Put(Debuff("low-blow", "Winded", Turn, new StatMod(Stat.Haste, -30)), save: Ability.Constitution, ifLanded: true),
            },
            Description = "Step behind a foe up to 100 px away: weapon dice + 2d6; Con save or it's winded (30% slower) for 3 s.",
        };
        yield return new()
        {
            Id = "lanerunner-lane-rules", Name = "Lane Rules", Class = ClassId.Lanerunner, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Advantage, When: When.VsAttackingOthers) }, Description = "Advantage on attacks against a foe that's attacking one of your allies.",
        };
        yield return new()
        {
            Id = "lanerunner-everyones-pocket", Name = "Everyone's Pocket", Class = ClassId.Lanerunner, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Weapon = true, Range = SkillRange.Room, Strikes = 2,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Gold, Amount = 20, On = EffectOn.AllFoes, Once = true },
                new Effect { Kind = EffectType.Teleport, On = EffectOn.Self, Once = true },
                WeaponHit with { Sneak = true },
            },
            Description = "Pick every foe's pocket at once, then step behind the one you aim at for a Twin Cuts.",
        };
        yield return new()
        {
            Id = "lanerunner-name-on-the-door", Name = "Name on the Door", Class = ClassId.Lanerunner, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.RollsPerAlly, 1) }, Description = "+1 to every attack and save for each ally still standing.",
        };

        // Wayfarer (Pedlar Mags): jack of all trades, Tempo.
        yield return new()
        {
            Id = "wayfarer-walking-stick", Name = "Walking Stick", Class = ClassId.Wayfarer, Kind = SkillKind.Auto, Range = SkillRange.Melee,
            Effects = new[] { SpellHit("1d6", DamageType.Bludgeoning, plus: true) with { AddCasting = true } }, Description = "1d6⁺ + Str or Dex bludgeoning.",
        };
        yield return new()
        {
            Id = "wayfarer-throw-anything", Name = "Throw Anything", Class = ClassId.Wayfarer, Focus = 15, CooldownMs = Cd1, Range = SkillRange.Short,
            Projectile = DamageType.Bludgeoning,
            Effects = new[] { new Effect { Kind = EffectType.Throw, Dice = D("1d6"), Type = DamageType.Bludgeoning } },
            Description = "Throw the bag's best thing for throwing (an Alchemist's Fire) at double strength, used up; with nothing, whatever's to hand: 2d6 bludgeoning.",
        };
        yield return new()
        {
            Id = "wayfarer-duchess-kicks", Name = "Duchess Kicks", Class = ClassId.Wayfarer, Rank = 2, Kind = SkillKind.Bonus, Focus = 25, CooldownMs = Cd3,
            Range = SkillRange.Melee,
            Effects = new[] { SureHit("2d6", DamageType.Bludgeoning), new Effect { Kind = EffectType.Knockback, Distance = 40f } },
            Description = "2d6 bludgeoning, and knocked 40 px back. (She's off-screen, but she's very much involved.)",
        };
        yield return new()
        {
            Id = "wayfarer-rummage", Name = "Rummage", Class = ClassId.Wayfarer, Rank = 3, Kind = SkillKind.Bonus, Focus = 20, CooldownMs = Cd3, Range = SkillRange.Self,
            Target = SkillTarget.Self, Ai = AiUse.Utility, AiValue = 4,
            Effects = new[]
            {
                new Effect
                {
                    Kind = EffectType.OneOf, On = EffectOn.Self,
                    Rider = new[]
                    {
                        Heal("2d4+2", casting: false),
                        new Effect { Kind = EffectType.Focus, Amount = 30 },
                        Put(Buff("rummaged-tonic", "Fizzing Tonic", 3 * Turn, new StatMod(Stat.Haste, 20))),
                    },
                },
            },
            Description = "Find a random potion at the bottom of the pack and drink it: a healing potion (2d4 + 2), an elixir of focus (30 Focus), or a fizzing tonic (20% haste for 9 s).",
        };
        yield return new()
        {
            Id = "wayfarer-haggle", Name = "Haggle", Class = ClassId.Wayfarer, Rank = 4, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.PricePct, -20) }, Description = "Shops are a fifth cheaper.",
        };
        yield return new()
        {
            Id = "wayfarer-lucky-stone", Name = "Lucky Stone", Class = ClassId.Wayfarer, Rank = 5, Focus = 60, CooldownMs = OncePerFight, Ultimate = true,
            Range = SkillRange.Room, Target = SkillTarget.None, Area = AreaShape.AllAllies, Ai = AiUse.Buff, AiValue = 6,
            Effects = new[]
            {
                Put(new AuraDefinition
                {
                    Id = "lucky-stone", Name = "Lucky Stone", Beneficial = true, DurationMs = 10 * Turn, UsedBy = AuraUse.NextRoll, Mods = new[] { new StatMod(Stat.Natural20) },
                }),
            },
            Description = "Each ally's next attack roll or save in 30 s is a natural 20.",
        };
        yield return new()
        {
            Id = "wayfarer-seen-it-all", Name = "Seen It All", Class = ClassId.Wayfarer, Rank = 10, Kind = SkillKind.Passive,
            Mods = new[] { new StatMod(Stat.Save, 2) }, Description = "+2 to every save.",
        };
    }
}
