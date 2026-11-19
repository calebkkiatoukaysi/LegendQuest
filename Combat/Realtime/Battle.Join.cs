using System;
using System.Collections.Generic;
using System.Numerics;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>Who's in a fight: fighters made from party members and from stat blocks, their numbers, their bars, and where they stand.</summary>
public sealed partial class Battle
{
    /// <summary>World pixels a second for each metre of 5e speed: 9 m walks 72 px a second, about a figure and a half.</summary>
    public const float PixelsPerMetre = 8f;

    /// <summary>How much further a big foe's body reaches than its feet.</summary>
    public const float BigSize = 14f;

    /// <summary>The Focus a foe starts on.</summary>
    public const int FoeStartFocus = 40;

    /// <summary>The Focus the Tempo model starts every fight on (11-combat.md §4.1).</summary>
    public const int TempoStartFocus = 80;

    /// <summary>Adds someone from a setup.</summary>
    private void Join(CombatantSetup setup, ref int partyPlace)
    {
        FighterKind kind = setup.Side == Side.Foes ? FighterKind.Foe : setup.Guest ? FighterKind.Guest : FighterKind.Member;
        Fighter f = setup.Member is not null
            ? FromMember(setup.Member, setup.Id, setup.Side, kind, _fighters.Count)
            : FromStatBlock(setup.Enemy, setup.Id, setup.Side, kind, _fighters.Count);

        Vector2 place;
        if (setup.At is Vector2 at) place = at;
        else if (setup.Side == Side.Foes) place = setup.Slot is Slot slot ? Arena.FoeStart(slot) : Arena.Middle;
        else place = Arena.PartyStarts.Count > 0 ? Arena.PartyStarts[partyPlace++ % Arena.PartyStarts.Count] : Arena.Middle;
        f.Position = f.Home = Arena.Clamp(place);
        f.Facing = setup.Side == Side.Party ? 1 : -1;
        _fighters.Add(f);

        if (setup.FromSeconds > 0)
        {
            f.Present = false;
            Vector2 stand = f.Position;
            At(setup.FromSeconds * 1000, () => Arrive(f, new Vector2(f.Side == Side.Party ? Arena.Left : Arena.Right, stand.Y), stand));
        }
    }

    /// <summary>Brings someone who wasn't there into the fight, walking in from a point to where they'll stand.</summary>
    internal void Arrive(Fighter f, Vector2 from, Vector2 to)
    {
        f.Present = true;
        f.Position = Arena.Clamp(from);
        f.Home = Arena.Clamp(to);
        SetGoal(f, f.Home);
        f.NextThinkMs = NowMs + 200;
        Emit(new BattleEvent { Kind = BattleEventKind.Joined, ActorId = f.Id, Position = f.Position, Text = $"{f.Name} joins the fight." });
    }

    /// <summary>A fighter from a party member (or a companion guest): their sheet, their weapon, their class's kit at their rank.</summary>
    private Fighter FromMember(PartyMember member, string id, Side side, FighterKind kind, int index)
    {
        string name = string.IsNullOrWhiteSpace(member.Name) ? Origins.NameOf(member.Id) : member.Name;
        ClassDefinition cls = ClassCatalog.Get(member.Class);
        var f = new Fighter(id, name, side, kind, index)
        {
            Member = member, ClassId = member.Class, Level = member.Level, Scores = member.Scores, Proficiency = LevelTable.Proficiency(member.Level),
            Rank = member.Rank,
        };

        f.MaxHp = member.MaxHp;
        f.Hp = kind == FighterKind.Member ? Math.Clamp(member.Hp, 1, f.MaxHp) : f.MaxHp;
        f.BaseAc = CombatStats.ArmourClassOf(member);
        f.DexAc = DexShare(member);
        foreach (ItemDefinition item in member.Worn)
            if (item.Kind == ItemKind.Shield) f.ShieldAc += item.AcBonus;

        ItemDefinition main = member.WornIn(EquipmentSlot.MainHand);
        f.MainAttack = CombatStats.Weapon(member, EquipmentSlot.MainHand) ?? CombatStats.Unarmed(member.Scores, member.Level);
        f.Attacks = new[] { f.MainAttack };
        f.WeaponKind = WeaponKinds.Of(main);
        f.BaseSwingMs = WeaponKinds.SwingOf(main);
        f.CastingAbility = cls.CastingAbility ?? (member.Scores.Modifier(Ability.Dexterity) > member.Scores.Modifier(Ability.Strength) ? Ability.Dexterity : Ability.Strength);
        f.SpellAttackExtra = CombatStats.SpellAttackBonus(member);
        f.FocusModel = cls.Focus;
        f.ThreatPct = cls.ThreatPct;
        f.WalkSpeed = Math.Max(40f, CombatStats.SpeedOf(member) * PixelsPerMetre);

        int start = DifficultyRules.StartFocus(Rules.Difficulty);
        if (f.FocusModel == FocusModel.Tempo) start = Math.Max(start, TempoStartFocus);
        f.FocusMilli = start * 1000;

        SetBar(f, SkillCatalog.Kit(member.Class));
        f.Ranged = RangeOf(f, f.Auto) > Ranges.Reach + 1f;
        foreach (SkillDefinition skill in f.BarSkills)
            if (skill.Ai == AiUse.Heal && skill.Target == SkillTarget.Ally) f.Healer = true;
        f.Defender = f.ThreatPct > 100;
        f.SwingTotalMs = SwingInterval(f);
        f.SwingLeftMs = f.SwingTotalMs / 2;
        return f;
    }

    /// <summary>A fighter from a stat block: a foe, a stat-block guest, a summon. Foes take the difficulty's hit points and rolls; guests and summons don't.</summary>
    internal Fighter FromStatBlock(EnemyDefinition enemy, string id, Side side, FighterKind kind, int index)
    {
        var f = new Fighter(id, enemy.Name, side, kind, index)
        {
            Enemy = enemy, Scores = enemy.Scores, Level = Math.Max(1, LevelTable.Expected(Encounter)), Proficiency = 2,
        };
        f.Proficiency = LevelTable.Proficiency(f.Level);

        bool foe = side == Side.Foes;
        int rolls = foe ? DifficultyRules.FoeRolls(Rules.Difficulty) + enemy.RealtimeHitShift : 0;
        f.MaxHp = Math.Max(1, (int)Math.Round(enemy.HitDice.Average * enemy.RealtimeHpScale * (foe ? DifficultyRules.FoeHp(Rules.Difficulty) : 1f), MidpointRounding.AwayFromZero));
        f.Hp = f.MaxHp;
        f.BaseAc = enemy.ArmourClass;
        f.DexAc = Math.Clamp(enemy.Scores.Modifier(Ability.Dexterity), 0, Math.Max(0, enemy.ArmourClass - 10));

        var attacks = new List<AttackProfile>();
        foreach (EnemyAttack attack in enemy.Attacks)
        {
            AttackProfile profile = CombatStats.Of(attack);
            attacks.Add(profile with { Bonus = profile.Bonus + rolls });
        }
        f.Attacks = attacks;
        f.MainAttack = attacks[0];
        if (enemy.Ranged)
            foreach (AttackProfile attack in attacks)
                if (attack.Ranged) { f.MainAttack = attack; break; }

        f.WeaponKind = enemy.Weapon ?? (enemy.Creature is not null ? WeaponKind.Unarmed : WeaponKind.OneHand);
        f.BaseSwingMs = enemy.SwingTimeMs;
        f.Size = enemy.Big ? BigSize : 0f;
        f.Auto = SkillCatalog.Get(enemy.AutoSkill ?? SkillCatalog.FoeAttack);
        f.BarSkills.Add(f.Auto);
        foreach (string skill in enemy.Skills)
        {
            f.FoeSkills.Add(skill);
            f.CooldownUntil[skill] = NowMs + FirstUseMs(SkillCatalog.Get(skill));
        }
        f.Ranged = enemy.Ranged;
        f.FocusModel = FocusModel.Momentum;
        f.FocusMilli = FoeStartFocus * 1000;
        f.WalkSpeed = Math.Max(30f, enemy.Speed * PixelsPerMetre);
        f.CastingAbility = Ability.Charisma;
        f.SwingTotalMs = SwingInterval(f);
        f.SwingLeftMs = f.SwingTotalMs / 2 + (int)_rng.Below(500);
        return f;
    }

    /// <summary>How long a foe waits before its move's first use: a third of its cooldown (at most 6 s), and up to a second more so a pack doesn't move as one.</summary>
    private int FirstUseMs(SkillDefinition skill) => Math.Min(6000, skill.CooldownMs / 3) + (int)_rng.Below(1000);

    /// <summary>A stat block called into the fight partway (a wave, a boss's whelps, a summon), standing at a point and walking to another.</summary>
    internal Fighter Spawn(string enemyId, string id, Side side, FighterKind kind, Vector2 from, Vector2 to)
    {
        Fighter f = FromStatBlock(EnemyCatalog.Get(enemyId), id, side, kind, _fighters.Count);
        f.Facing = side == Side.Party ? 1 : -1;
        _fighters.Add(f);
        Arrive(f, from, to);
        return f;
    }

    /// <summary>Puts a class kit's skills on a fighter's bar as far as their rank goes: the auto and skill 1, skill 2 from rank 2, skill 3 from 3, the ultimate from 5; the passives from 4 and 10.</summary>
    internal static void SetBar(Fighter f, ClassKit kit)
    {
        f.BarSkills.Clear();
        f.PassiveSkills.Clear();
        f.Auto = kit.Auto;
        f.BarSkills.Add(kit.Auto);
        f.BarSkills.Add(kit.Skill1);
        if (f.Rank >= ClassRanks.Skill2Rank) f.BarSkills.Add(kit.Skill2);
        if (f.Rank >= ClassRanks.Skill3Rank) f.BarSkills.Add(kit.Skill3);
        if (f.Rank >= ClassRanks.UltimateRank) f.BarSkills.Add(kit.Ultimate);
        if (f.Rank >= ClassRanks.PassiveRank) f.PassiveSkills.Add(kit.Passive1);
        if (f.Rank >= ClassRanks.LastPassiveRank) f.PassiveSkills.Add(kit.Passive2);
    }

    /// <summary>How much of someone's AC their Dexterity gives: all of it with no armour or light armour, up to the armour's MaxDex otherwise, never below 0.</summary>
    private static int DexShare(PartyMember member)
    {
        int dexterity = member.Scores.Modifier(Ability.Dexterity);
        ItemDefinition body = member.WornIn(EquipmentSlot.Body);
        int share = body is null || body.Kind != ItemKind.Armour ? dexterity : body.MaxDex switch
        {
            null => dexterity,
            int most => Math.Min(dexterity, most),
        };
        return Math.Max(0, share);
    }

    /// <summary>Runs every fighter's fight-start procs (a first skill that costs nothing).</summary>
    private void StartProcs(Fighter f)
    {
        foreach (SkillDefinition passive in f.PassiveSkills)
            foreach (Proc proc in passive.Procs)
                if (proc.Trigger == ProcTrigger.FightStart)
                    foreach (Effect effect in proc.Effects) ApplyLoose(f, passive, effect, f, null);
    }
}
