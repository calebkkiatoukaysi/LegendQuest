using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Someone in a fight: a party member or a foe, where they stand, how hurt they are, what's on them,
/// and what's left of their turn.
/// </summary>
/// <remarks>
/// A party member's hit points are their PartyMember's, so what they lose in a fight they've still
/// lost after it, the same way DialogueRunner writes straight into the save. A foe's are its own.
/// Everything else here only lasts the fight. The engine changes it; the screens only read it.
/// </remarks>
public sealed class Combatant
{
    /// <summary>What a spell save DC starts from, before proficiency and the casting modifier.</summary>
    private const int SpellDcBase = 8;

    private readonly List<ActiveCondition> _conditions = new();

    /// <summary>A foe's abilities that have been used and haven't recharged, or that only work once a fight.</summary>
    private readonly HashSet<AbilityId> _spent = new();

    /// <summary>A foe's hit points. A party member's are on their PartyMember.</summary>
    private int _hp;

    /// <summary>Sets someone up from how the fight starts them.</summary>
    /// <param name="setup">Who, where, and on which side.</param>
    /// <param name="cell">The tile they actually start on (the setup's, or the nearest free one).</param>
    /// <param name="joined">How many joined before them, which breaks the last of initiative's ties.</param>
    internal Combatant(CombatantSetup setup, Point cell, int joined)
    {
        Id = setup.Id;
        Side = setup.Side;
        Member = setup.Member;
        Enemy = setup.Enemy;
        Guest = setup.Guest;
        Cell = cell;
        Facing = setup.Facing is Direction.Left or Direction.Right ? setup.Facing : Side == Side.Foes ? Direction.Left : Direction.Right;
        Joined = joined;

        if (Member is not null)
        {
            Name = Member.Name;
            Scores = Member.Scores;
            MaxHp = Member.MaxHp;
            ArmourClass = CombatStats.ArmourClassOf(Member);
            Speed = CombatStats.SpeedOf(Member);
            ArmourUntrained = CombatStats.ArmourUntrained(Member);
            Casting = CombatStats.CastingAbility(Member);
            SpellFocus = CombatStats.SpellAttackBonus(Member);

            AttackProfile main = CombatStats.Weapon(Member, EquipmentSlot.MainHand);
            AttackProfile off = CombatStats.Weapon(Member, EquipmentSlot.OffHand);
            MainHand = main ?? CombatStats.Unarmed(Scores);
            bool pair = main is not null && off is not null && Light(main) && Light(off);
            OffHand = pair ? off : null;
            Attacks = new[] { MainHand };
        }
        else
        {
            Name = Enemy.Name;
            Scores = Enemy.Scores;
            MaxHp = Math.Max(1, (int)Enemy.HitDice.Average);
            _hp = MaxHp;
            ArmourClass = Enemy.ArmourClass;
            Speed = Enemy.Speed;

            var attacks = new AttackProfile[Enemy.Attacks.Count];
            for (int i = 0; i < attacks.Length; i++) attacks[i] = CombatStats.Of(Enemy.Attacks[i]);
            Attacks = attacks;
            MainHand = attacks[0];
        }

        // The melee attack they'd swing as someone runs past, and so how far they threaten.
        AttackProfile melee = null;
        foreach (AttackProfile attack in Attacks)
            if (!attack.Ranged && (melee is null || attack.AverageDamage > melee.AverageDamage)) melee = attack;
        Melee = melee ?? CombatStats.Unarmed(Scores);
    }

    /// <summary>Who they are: a party member's origin id, or the foe's map npc id or spawned id.</summary>
    public string Id { get; }

    /// <summary>What they're called on screen.</summary>
    public string Name { get; }

    /// <summary>Which side they're on.</summary>
    public Side Side { get; }

    /// <summary>The party member they are, or null for a foe.</summary>
    public PartyMember Member { get; }

    /// <summary>The foe they are, or null for a party member.</summary>
    public EnemyDefinition Enemy { get; }

    /// <summary>Whether they're fighting on the party's side without being in it, so the AI plays them.</summary>
    public bool Guest { get; }

    /// <summary>The tile they're on.</summary>
    public Point Cell { get; set; }

    /// <summary>Which way they're facing: left or right, never up or down. Set up facing either of those, they start facing their side's way, the party right and the foes left.</summary>
    public Direction Facing { get; set; }

    /// <summary>The hit points they have now.</summary>
    public int Hp => Member?.Hp ?? _hp;

    /// <summary>The most hit points they can have.</summary>
    public int MaxHp { get; }

    /// <summary>Their armour class.</summary>
    public int ArmourClass { get; }

    /// <summary>How far they can move in a turn, in metres, before conditions.</summary>
    public float Speed { get; }

    /// <summary>Their initiative roll's total. Higher goes first.</summary>
    public int Initiative { get; internal set; }

    /// <summary>Whether they're at 0 hit points and still alive: Downed, or a foe knocked out in a fight to yield.</summary>
    public bool IsDown => !IsDead && Hp <= 0;

    /// <summary>Whether they've died.</summary>
    public bool IsDead { get; internal set; }

    /// <summary>Whether they're out of the fight for now: down or dead.</summary>
    public bool IsOut => IsDown || IsDead;

    /// <summary>The conditions on them, in the order they went on.</summary>
    public IReadOnlyList<ActiveCondition> Conditions => _conditions;

    /// <summary>What's left of their turn.</summary>
    public TurnBudget Budget { get; } = new();

    /// <summary>Death saves made while Downed, for the pips.</summary>
    public int DeathSuccesses { get; internal set; }

    /// <summary>Death saves failed while Downed, for the pips.</summary>
    public int DeathFailures { get; internal set; }

    /// <summary>The spell they're concentrating on, or null.</summary>
    public AbilityId? Concentrating { get; internal set; }

    /// <summary>How many times they've been Downed this fight.</summary>
    public int Downs { get; internal set; }

    /// <summary>Whether they're you or the Soul-Bound, who share the Soul-Link.</summary>
    public bool IsDuo => Member is not null && !Guest && (Id == Origins.You || Id == Origins.SoulBound);

    /// <summary>Their name the way the log writes it: in capitals.</summary>
    internal string Label => Name.ToUpperInvariant();

    /// <summary>How many joined the fight before them: the last of initiative's tie-breaks.</summary>
    internal int Joined { get; }

    /// <summary>Their six scores.</summary>
    internal AbilityScores Scores { get; }

    /// <summary>The ability they cast with, or null.</summary>
    internal Ability? Casting { get; }

    /// <summary>Whether they're in armour they aren't trained for: disadvantage on attacks and Strength and Dexterity saves, and no spells.</summary>
    internal bool ArmourUntrained { get; }

    /// <summary>How they attack with the main hand: a party member's weapon (or fist), a foe's first attack.</summary>
    internal AttackProfile MainHand { get; }

    /// <summary>How they attack with the off hand, if both hands hold light weapons; otherwise null.</summary>
    internal AttackProfile OffHand { get; }

    /// <summary>Every attack they have: a party member's main hand, or all of a foe's.</summary>
    internal IReadOnlyList<AttackProfile> Attacks { get; }

    /// <summary>Their best melee attack, for opportunity attacks: a fist if there's nothing else.</summary>
    internal AttackProfile Melee { get; }

    /// <summary>The number of the turn they last used Sneak Attack on, since it's once a turn.</summary>
    internal int SneakTurn { get; set; } = -1;

    /// <summary>Their level: a party member's, or 1 for a foe.</summary>
    internal int Level => Member?.Level ?? 1;

    /// <summary>Their casting modifier, or 0 if they don't cast.</summary>
    internal int CastingModifier => Casting is Ability ability ? Scores.Modifier(ability) : 0;

    /// <summary>Their spell save DC: 8 + proficiency + the casting modifier.</summary>
    internal int SpellDc => SpellDcBase + AttackMath.ProficiencyBonus + CastingModifier;

    /// <summary>What anything they're wearing adds to their spell attacks.</summary>
    internal int SpellFocus { get; }

    /// <summary>Their spell attack bonus: proficiency + the casting modifier + anything worn.</summary>
    internal int SpellAttack => AttackMath.ProficiencyBonus + CastingModifier + SpellFocus;

    /// <summary>What they add to a saving throw: the modifier, and proficiency for a party member's class saves.</summary>
    /// <param name="ability">Which save.</param>
    internal int SaveBonus(Ability ability) => Member is not null ? CombatStats.SaveBonus(Member, ability) : Scores.Modifier(ability);

    /// <summary>Whether a condition's on them.</summary>
    /// <param name="id">Which.</param>
    internal bool Has(ConditionId id) => Find(id) is not null;

    /// <summary>A condition on them, or null.</summary>
    /// <param name="id">Which.</param>
    internal ActiveCondition Find(ConditionId id)
    {
        foreach (ActiveCondition condition in _conditions)
            if (condition.Id == id) return condition;
        return null;
    }

    /// <summary>Puts a condition on, replacing the same one if it's there.</summary>
    /// <param name="condition">The condition.</param>
    internal void Add(ActiveCondition condition)
    {
        Remove(condition.Id);
        _conditions.Add(condition);
    }

    /// <summary>Takes a condition off.</summary>
    /// <param name="id">Which.</param>
    /// <returns>Whether it was on.</returns>
    internal bool Remove(ConditionId id) => _conditions.RemoveAll(each => each.Id == id) > 0;

    /// <summary>Takes every condition off at once, when they die.</summary>
    internal void ClearConditions() => _conditions.Clear();

    /// <summary>Sets their hit points, kept between 0 and their most.</summary>
    /// <param name="hp">The new hit points.</param>
    internal void SetHp(int hp)
    {
        hp = Math.Clamp(hp, 0, MaxHp);
        if (Member is not null) Member.Hp = hp;
        else _hp = hp;
    }

    /// <summary>Whether a foe's ability is spent: used and not recharged, or once a fight and used.</summary>
    /// <param name="id">Which.</param>
    internal bool IsSpent(AbilityId id) => _spent.Contains(id);

    /// <summary>Marks a foe's ability spent, or recharged.</summary>
    /// <param name="id">Which.</param>
    /// <param name="spent">Spent, or back.</param>
    internal void MarkSpent(AbilityId id, bool spent)
    {
        if (spent) _spent.Add(id);
        else _spent.Remove(id);
    }

    /// <summary>A foe's abilities that are spent, for the recharge rolls at the start of its turn.</summary>
    internal IEnumerable<AbilityId> Spent => _spent;

    /// <summary>Whether a weapon's light, so it can pair with another for an off-hand attack.</summary>
    private static bool Light(AttackProfile weapon) => (weapon.Properties & WeaponProperty.Light) != 0;
}
