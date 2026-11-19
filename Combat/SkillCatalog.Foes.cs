using System.Collections.Generic;
using LegendQuest.Combat.Realtime;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>The foes' telegraphed moves (11-combat.md §5.1, §5.2 and §6.1), by id, which EnemyCatalog's stat blocks list.</summary>
/// <remarks>
/// A foe's move costs it nothing but its cooldown: a 5e "recharge 5-6" is 14 s, "recharge 6" 18 s.
/// Each has a telegraph (TelegraphMs): its wind-up clip held, a cast bar over its head, and for an
/// area a ground marker that fills and lands the moment it's full; 0.5 s or more for a normal move,
/// 1.0 to 1.5 s for a heavy one. A rigged foe plays foe-&lt;enemy id&gt;-&lt;move&gt; (the id without its
/// "foe-"), a creature its own sheet's frames. DCs and attack bonuses are the stat block's, before the
/// difficulty's change.
/// </remarks>
public static partial class SkillCatalog
{
    /// <summary>A 5e "recharge 5-6" in real time.</summary>
    public const int Recharge5 = 14000;

    /// <summary>A 5e "recharge 6" in real time.</summary>
    public const int Recharge6 = 18000;

    /// <summary>The foes' moves.</summary>
    private static IEnumerable<SkillDefinition> FoeMoves()
    {
        // The gloomfangs' and the Thornmother's (and the vault construct's) trip: a lunge at the legs.
        yield return new()
        {
            Id = "foe-trip", Name = "Trip", CooldownMs = 12000, TelegraphMs = 600, Range = SkillRange.Melee, Dc = 11, Ai = AiUse.Control,
            Effects = new[] { Put(Condition("tripped", "Knocked Down", ConditionId.Prone, Battle.KnockdownMs), save: Ability.Strength) },
            Description = "A lunge at the legs: Str save or knocked down.",
        };

        // Bosun Grell's hook: he hauls whoever's hanging back in to him.
        yield return new()
        {
            Id = "foe-hook-pull", Name = "Hook Pull", CooldownMs = 12000, TelegraphMs = 1000, Range = SkillRange.Short, AttackBonus = 5,
            Effects = new[] { new Effect { Kind = EffectType.Pull }, SpellHit("1d4+1", DamageType.Piercing) },
            Description = "The hook hand flies out: pulled to him, and 1d4 + 1 piercing.",
        };

        // The Thornmother's second phase: the floor around her cracks and thorns burst from her hide.
        yield return new()
        {
            Id = "foe-thorn-burst", Name = "Thorn Burst", CooldownMs = Recharge5, TelegraphMs = 1800, Interruptible = false, Range = SkillRange.Self,
            Target = SkillTarget.None, Area = AreaShape.Ring, AreaPx = 62f, Dc = 12,
            Effects = new[] { SaveHit("2d6", DamageType.Piercing, Ability.Dexterity, true) },
            Description = "A ring of thorns around her fills for 1.8 s: 2d6 piercing, Dex save for half. Step out of it.",
        };

        // Her third: she marks a line across the room and charges along it.
        yield return new()
        {
            Id = "foe-uprooting-charge", Name = "Uprooting Charge", CooldownMs = Recharge5, TelegraphMs = 1500, Interruptible = false, Range = SkillRange.Long,
            Area = AreaShape.Line, AreaPx = 200f, Dc = 13,
            Effects = new[]
            {
                new Effect { Kind = EffectType.Charge, On = EffectOn.Self, Once = true },
                SaveHit("2d6+4", DamageType.Bludgeoning, Ability.Dexterity, true),
                Put(Condition("uprooted", "Knocked Down", ConditionId.Prone, Battle.KnockdownMs), ifLanded: true),
            },
            Description = "A line across the room, marked for 1.5 s, then she charges along it: 2d6 + 4 and knocked down, Dex save for half and to keep your feet.",
        };

        // The sappers' lit charge, lobbed onto the floor where the target stands.
        yield return new()
        {
            Id = "foe-blasting-charge", Name = "Blasting Charge", CooldownMs = Recharge6, TelegraphMs = 2000, Interruptible = false, Range = SkillRange.Long,
            Area = AreaShape.Ring, AreaPx = Battle.NeighbourPx, Dc = 12, Projectile = DamageType.Fire,
            Effects = new[] { SaveHit("2d6+2", DamageType.Fire, Ability.Dexterity, true) },
            Description = "A charge lands and its marker fills for 2 s: 2d6 + 2 fire to everyone in it, Dex save for half.",
        };

        // The hexer's auto: a bolt of ash with a cast an interrupt (or a solid hit) can stop.
        yield return new()
        {
            Id = "foe-ashen-bolt", Name = "Ashen Bolt", Kind = SkillKind.Auto, CastMs = 1000, Range = SkillRange.Long, AttackBonus = 3, Projectile = DamageType.Necrotic,
            Effects = new[] { SpellHit("1d8", DamageType.Necrotic) },
            Description = "A 1 s cast, then 1d8 necrotic.",
        };

        // The hexer's curse, once a fight.
        yield return new()
        {
            Id = "foe-hex", Name = "Hex", Kind = SkillKind.Bonus, CooldownMs = OncePerFight, Range = SkillRange.Long, Concentration = true, Ai = AiUse.Control,
            Effects = new[] { Put(Debuff("foe-hexed", "Hexed", 10 * Turn, new StatMod(Stat.TakenFromSource, Dice: D("1d6"), Type: DamageType.Necrotic)) with { Condition = ConditionId.Hexed }) },
            Description = "Its hits on you do +1d6 necrotic for 30 s.",
        };

        // Kael's duel: the feint that makes his next blow a critical.
        yield return new()
        {
            Id = "foe-feint", Name = "Feint", CooldownMs = 10000, TelegraphMs = 500, Range = SkillRange.Melee, Ai = AiUse.Buff,
            Effects = new[]
            {
                Put(new AuraDefinition { Id = "feinting", Name = "Feint", Beneficial = true, DurationMs = 2 * Turn, UsedBy = AuraUse.NextAttack, Mods = new[] { new StatMod(Stat.SureCrit) } }, EffectOn.Self),
            },
            Description = "A feint, telegraphed for 0.5 s: his next hit is a critical.",
        };

        // His second phase: smoke, and out of it behind you.
        yield return new()
        {
            Id = "foe-smoke-bomb", Name = "Smoke Bomb", CooldownMs = Recharge5, Range = SkillRange.Self, Target = SkillTarget.Self,
            Effects = new[] { new Effect { Kind = EffectType.Vanish, On = EffectOn.Self, DurationMs = 2000 } },
            Description = "He vanishes in smoke for 2 s and steps out behind his target with a sure Sneak Attack.",
        };

        // And his step back out of a crowd.
        yield return new()
        {
            Id = "foe-cunning-disengage", Name = "Cunning Disengage", Kind = SkillKind.Bonus, CooldownMs = 12000, Range = SkillRange.Self, Target = SkillTarget.Self,
            Effects = new[] { new Effect { Kind = EffectType.Dash, On = EffectOn.Self, Distance = 60f } },
            Description = "With two or more on him, he slips 60 px clear.",
        };

        // The Hollowed footman's heavy blow.
        yield return new()
        {
            Id = "foe-overhead-chop", Name = "Overhead Chop", CooldownMs = Recharge5, TelegraphMs = 1000, Range = SkillRange.Weapon, Weapon = true,
            Effects = new[] { WeaponHit, SureHit("1d6", DamageType.Slashing) with { IfLanded = true } },
            Description = "A two-handed chop, wound up for 1 s: its blade's damage and 1d6 more.",
        };
    }
}
