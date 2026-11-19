using System;
using System.Collections.Generic;
using LegendQuest.Combat.Realtime;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Every class's kit and every foe's move for the real-time engine, by skill id: 10-classes.md's
/// tables as data.
/// </summary>
/// <remarks>
/// <para>
/// HOW TO ADD A CLASS. Adding a class is adding rows, nothing else:
/// </para>
/// <list type="number">
/// <item>Its ClassId and ClassDefinition are already in ClassCatalog (every class in 10-classes.md is),
/// with its Code (the start of every skill id), Focus model, threat and saves.</item>
/// <item>Write a new partial, SkillCatalog.&lt;Group&gt;.cs, with a static method that returns its seven
/// SkillDefinitions: the rank 1 auto (Kind Auto), the rank 1, 2 and 3 skills, the rank 5 ultimate
/// (Ultimate = true) and the rank 4 and 10 passives (Kind Passive). Ids are skill-ids.md's:
/// `&lt;code&gt;-&lt;kebab name&gt;`. Then add one line calling it to the Advanced partial method (see
/// SkillCatalog.Base.cs's BaseClasses for the shape): implement
/// <c>static partial void Advanced(List&lt;SkillDefinition&gt; rows)</c> in your partial and add your rows there.
/// Forms (like the bear) go in <c>static partial void AdvancedForms(List&lt;FormDefinition&gt; forms)</c>.</item>
/// <item>Convert each table line by 11-combat.md §4.2 and §4.3: A is Kind Action (on the GCD), B Bonus,
/// R Reaction (an Arm effect first, then what it does when it fires, On = Attacker for whoever set it off),
/// P Passive (Mods for always-on numbers, Procs for things it does when something happens, AllyMods for
/// what it gives allies near). CD 1 to 4 are 6000, 8000, 12000, 16000 ms; 1/fight is CooldownMs 45000 with
/// Ultimate = true (ready at the start). A turn is 3000 ms. Shapes: "1 foe, front" is Range Melee (or
/// Weapon); "1 foe" Long; "foe + neighbours" AreaShape.Neighbours; "1 row" Row; "front row of foes" Front
/// with Target None; "a column" Line; "all foes"/"all allies" AllFoes/AllAllies with Range Room and
/// Target None; "N random foes" RandomFoes with Count and Strikes N; "your row" AlliesNear.</item>
/// <item>Effects say what happens, in order, each to whoever its On says: Damage (Delivery Attack, Save
/// or Sure; Weapon for weapon dice, Plus for the tables' ⁺), Heal, TempHp, Aura (an AuraDefinition: a
/// condition, stat mods, a tick), Cleanse, Knockback, Pull, Dash, Teleport, Swap, Taunt, Summon (an
/// EnemyCatalog id), Transform (a form id), Arm, Rider (effects on the next hit), Focus, Reset (take
/// another action), Gold, Rewind, Dispel, Interrupt, Strike, Charge, Vanish. IfLanded makes an effect
/// wait on the one before it landing (a hit, or a failed save). If a skill needs something no kind
/// does, add the smallest new EffectType (or Stat, When, ProcTrigger) on the end of its enum and its
/// case in Battle.Effects.cs.</item>
/// <item>Each non-passive skill gets a clip, skill-&lt;id&gt; (contract §2.3), except a weapon auto,
/// which plays the weapon's auto-&lt;kind&gt;-n. Its blows land on that clip's hit or release events;
/// set Strikes for a skill with several. Set Projectile to the school for anything that flies.</item>
/// <item>Give the AI what it needs: Ai (what it's for), AiValue (what a buff, control or taunt is worth
/// in points of damage), AiMinTargets for an area skill.</item>
/// <item>Run `--simulate selftest`: it uses every skill of every class with a kit in a scripted fight
/// and checks each does what its kind says; then `--simulate classes`.</item>
/// </list>
/// <para>
/// The static constructor checks every row (a kit of seven at the right ranks, an id that starts with its
/// class's code, effects with what their kinds need), so a mistake shows up when the game starts. A class
/// without a kit here fights with the kit of its ClassDefinition.InterimKit's class until it has one.
/// </para>
/// </remarks>
public static partial class SkillCatalog
{
    /// <summary>The generic auto attack of a foe's stat block: its attacks, as a combo for a Multiattack.</summary>
    public const string FoeAttack = "foe-attack";

    /// <summary>The Soul-Link Strike, the duo's (11-combat.md §5.3).</summary>
    public const string SoulLinkStrike = "soul-link-strike";

    /// <summary>A turn, in milliseconds (11-combat.md §3.5).</summary>
    public const int Turn = 3000;

    /// <summary>The tables' CD 1 to 4, and once a fight, in milliseconds (11-combat.md §4.2).</summary>
    public const int Cd1 = 3000, Cd2 = 6000, Cd3 = 9000, Cd4 = 12000, OncePerFight = 45000;

    private static readonly Dictionary<string, SkillDefinition> ById = new(StringComparer.Ordinal);
    private static readonly Dictionary<ClassId, ClassKit> Kits = new();
    private static readonly Dictionary<string, FormDefinition> FormsById = new(StringComparer.Ordinal);
    private static readonly List<SkillDefinition> Rows = new();

    /// <summary>Gathers every partial's rows, indexes them and checks them.</summary>
    /// <remarks>Throws on purpose, so a row that can't work shows up when the game starts.</remarks>
    static SkillCatalog()
    {
        Rows.AddRange(BaseClasses());
        Rows.AddRange(FoeMoves());
        Rows.AddRange(Shared());
        Advanced(Rows);

        var forms = new List<FormDefinition>(BaseForms());
        AdvancedForms(forms);
        foreach (FormDefinition form in forms)
            if (!FormsById.TryAdd(form.Id, form)) throw new InvalidOperationException($"Two forms are called {form.Id}.");

        foreach (SkillDefinition row in Rows)
        {
            Check(row);
            if (!ById.TryAdd(row.Id, row)) throw new InvalidOperationException($"Two skills are called {row.Id}.");
        }

        foreach (ClassDefinition kind in ClassCatalog.All)
            if (KitFrom(kind) is ClassKit kit) Kits[kind.Id] = kit;

        foreach (FormDefinition form in FormsById.Values)
        {
            if (!ById.ContainsKey(form.Auto)) throw new InvalidOperationException($"Form {form.Id}'s auto {form.Auto} isn't a skill.");
            foreach (string skill in form.Skills)
                if (!ById.ContainsKey(skill)) throw new InvalidOperationException($"Form {form.Id}'s skill {skill} isn't a skill.");
        }

        foreach (EnemyDefinition enemy in EnemyCatalog.All) CheckFoe(enemy);
    }

    /// <summary>Checks a stat block's real-time moves are all here: its auto, its moves, and what its phases add.</summary>
    /// <remarks>Here rather than in EnemyCatalog, which this catalog reads (for summons) as it starts.</remarks>
    private static void CheckFoe(EnemyDefinition enemy)
    {
        string name = $"Enemy {enemy.Id}";
        if (enemy.AutoSkill is not null && (!ById.TryGetValue(enemy.AutoSkill, out SkillDefinition auto) || auto.Kind != SkillKind.Auto))
            throw new InvalidOperationException($"{name}'s auto attack {enemy.AutoSkill} isn't an auto in SkillCatalog.");
        var moves = new List<string>(enemy.Skills);
        double share = 1.0;
        foreach (FoePhase phase in enemy.Phases)
        {
            if (phase.AtShare <= 0 || phase.AtShare >= share) throw new InvalidOperationException($"{name}'s phase {phase.Name} begins at {phase.AtShare:P0}: phases go down from 100% in order.");
            share = phase.AtShare;
            moves.AddRange(phase.Adds);
            moves.AddRange(phase.Removes);
            moves.AddRange(phase.Cooldowns.Keys);
            if (phase.Summons is not null && (!EnemyCatalog.Exists(phase.Summons) || phase.SummonCount < 1))
                throw new InvalidOperationException($"{name}'s phase {phase.Name} calls in {phase.Summons}, who isn't in EnemyCatalog (or nobody).");
        }
        foreach (string move in moves)
            if (!ById.TryGetValue(move, out SkillDefinition skill) || skill.Class is not null || skill.Kind is SkillKind.Auto or SkillKind.Passive or SkillKind.Reaction)
                throw new InvalidOperationException($"{name} uses {move}, which isn't a foe's move in SkillCatalog.");
    }

    /// <summary>Every clip the real-time engine asks for that clips.json should have (contract §2.3): each class skill's own, each rigged foe's moves (a foe that holds a weapon is a rigged figure; claws and teeth are a creature's sheet), the autos and telegraphs of every weapon kind, and the reactions; for --validate's warnings.</summary>
    public static IEnumerable<string> Clips()
    {
        foreach (SkillDefinition row in Rows)
            if (row.Class is not null && row.OwnMotion is string own) yield return own;
        foreach (WeaponKind kind in Enum.GetValues<WeaponKind>())
        {
            for (int swing = 0; swing < 3; swing++) yield return WeaponKinds.AutoClip(kind, swing);
            yield return "telegraph-" + WeaponKinds.Clip(kind);
        }
        foreach (EnemyDefinition enemy in EnemyCatalog.All)
        {
            if (enemy.Weapon is null or WeaponKind.Unarmed) continue;
            var moves = new List<string>(enemy.Skills);
            if (enemy.AutoSkill is not null) moves.Add(enemy.AutoSkill);
            foreach (FoePhase phase in enemy.Phases) moves.AddRange(phase.Adds);
            foreach (string move in moves)
                if (Get(move).Motion is null) yield return "foe-" + enemy.Id + "-" + move.Substring("foe-".Length);
        }
        foreach (string reaction in new[] { "block", "dodge", "hurt", "hurt-big", "knockdown", "getup", "downed", "die", "revive", "ready", "cast", "channel", "drink-potion", "soul-link-strike" })
            yield return reaction;
    }

    /// <summary>The advanced classes' rows, added by their own partials (see the guide above).</summary>
    /// <param name="rows">The list to add them to.</param>
    static partial void Advanced(List<SkillDefinition> rows);

    /// <summary>The advanced classes' forms, added by their own partials.</summary>
    /// <param name="forms">The list to add them to.</param>
    static partial void AdvancedForms(List<FormDefinition> forms);

    /// <summary>Every skill, in the order they were written.</summary>
    public static IReadOnlyList<SkillDefinition> All => Rows;

    /// <summary>Whether there's a skill with this id.</summary>
    /// <param name="id">Which.</param>
    public static bool Exists(string id) => id is not null && ById.ContainsKey(id);

    /// <summary>One skill.</summary>
    /// <param name="id">Which. It has to exist.</param>
    public static SkillDefinition Get(string id) =>
        id is not null && ById.TryGetValue(id, out SkillDefinition row) ? row : throw new KeyNotFoundException($"There's no skill called {id}.");

    /// <summary>A form, by id.</summary>
    /// <param name="id">Which. It has to exist.</param>
    public static FormDefinition Form(string id) => FormsById[id];

    /// <summary>Whether a class has its own kit here yet.</summary>
    /// <param name="id">The class.</param>
    public static bool HasKit(ClassId id) => Kits.ContainsKey(id);

    /// <summary>A class's kit: its own, or (for a class whose rows aren't written yet) its InterimKit class's.</summary>
    /// <param name="id">The class.</param>
    public static ClassKit Kit(ClassId id) => Kits.TryGetValue(id, out ClassKit kit) ? kit : Kits[ClassCatalog.Get(id).InterimKit];

    /// <summary>Every class with a kit of its own, in the enum's order.</summary>
    public static IEnumerable<ClassId> KittedClasses
    {
        get
        {
            foreach (ClassDefinition kind in ClassCatalog.All)
                if (Kits.ContainsKey(kind.Id)) yield return kind.Id;
        }
    }

    /// <summary>A class's seven skills in their slots, from its rows by rank and kind, or null if it has none.</summary>
    private static ClassKit KitFrom(ClassDefinition kind)
    {
        var mine = new List<SkillDefinition>();
        foreach (SkillDefinition row in Rows)
            if (row.Class == kind.Id) mine.Add(row);
        if (mine.Count == 0) return null;

        string name = $"Class {kind.Id}'s kit";
        if (mine.Count != 7) throw new InvalidOperationException($"{name} has {mine.Count} skills, not 7.");
        SkillDefinition auto = One(mine, name, row => row.Kind == SkillKind.Auto, "an auto attack at rank 1");
        SkillDefinition first = One(mine, name, row => row.Rank == 1 && row.Kind != SkillKind.Auto, "a rank 1 skill");
        SkillDefinition second = One(mine, name, row => row.Rank == ClassRanks.Skill2Rank, "a rank 2 skill");
        SkillDefinition third = One(mine, name, row => row.Rank == ClassRanks.Skill3Rank, "a rank 3 skill");
        SkillDefinition ultimate = One(mine, name, row => row.Rank == ClassRanks.UltimateRank, "a rank 5 ultimate");
        SkillDefinition passive = One(mine, name, row => row.Rank == ClassRanks.PassiveRank, "a rank 4 passive");
        SkillDefinition last = One(mine, name, row => row.Rank == ClassRanks.LastPassiveRank, "a rank 10 passive");
        if (auto.Rank != 1 || !ultimate.Ultimate || passive.Kind != SkillKind.Passive || last.Kind != SkillKind.Passive)
            throw new InvalidOperationException($"{name} needs its auto at rank 1, an ultimate at 5 and passives at 4 and 10.");
        return new ClassKit(auto, first, second, third, ultimate, passive, last);
    }

    /// <summary>The one row of a kit that's what's asked for, or an error.</summary>
    private static SkillDefinition One(List<SkillDefinition> rows, string name, Func<SkillDefinition, bool> what, string words)
    {
        SkillDefinition found = null;
        foreach (SkillDefinition row in rows)
        {
            if (!what(row)) continue;
            if (found is not null) throw new InvalidOperationException($"{name} has more than one {words}.");
            found = row;
        }
        return found ?? throw new InvalidOperationException($"{name} has no {words}.");
    }

    /// <summary>Checks one row, and throws with what's wrong if it can't work.</summary>
    private static void Check(SkillDefinition row)
    {
        string name = $"Skill {row.Id}";
        if (string.IsNullOrWhiteSpace(row.Id) || string.IsNullOrWhiteSpace(row.Name)) throw new InvalidOperationException($"{name} needs an id and a name.");
        if (row.Class is ClassId owner && !row.Id.StartsWith(ClassCatalog.Get(owner).Code + "-", StringComparison.Ordinal))
            throw new InvalidOperationException($"{name} is the {owner}'s, and doesn't start with {ClassCatalog.Get(owner).Code}-.");
        if (row.Focus is < 0 or > Fighter.MaxFocus || row.CooldownMs < 0 || row.CastMs < 0 || row.TelegraphMs < 0 || row.HpCost < 0)
            throw new InvalidOperationException($"{name} has a cost or a time below nothing (or Focus past 100).");
        if (row.Ultimate && row.CooldownMs < OncePerFight) throw new InvalidOperationException($"{name} is an ultimate on a cooldown under {OncePerFight} ms.");
        if (row.Kind == SkillKind.Reaction != (row.Trigger != ReactionTrigger.None)) throw new InvalidOperationException($"{name} needs a trigger if and only if it's a reaction.");
        if (row.Kind == SkillKind.Passive && row.Effects.Count > 0) throw new InvalidOperationException($"{name} is a passive with effects; a passive has Mods and Procs.");
        if (row.Active && row.Effects.Count == 0) throw new InvalidOperationException($"{name} does nothing.");
        if (row.Strikes < 1) throw new InvalidOperationException($"{name} lands no blows.");
        if (row.Area is AreaShape.RandomFoes or AreaShape.Spread or AreaShape.Allies or AreaShape.Chain && row.Count < 1)
            throw new InvalidOperationException($"{name}'s {row.Area} needs a Count.");
        if (row.Kind == SkillKind.Reaction && (row.Effects.Count == 0 || row.Effects[0].Kind != EffectType.Arm))
            throw new InvalidOperationException($"{name} is a reaction whose first effect isn't Arm.");

        foreach (Effect effect in row.Effects) CheckEffect(name, row, effect);
        foreach (Proc proc in row.Procs)
            foreach (Effect effect in proc.Effects) CheckEffect(name + "'s proc", row, effect);
    }

    /// <summary>Checks one effect has what its kind needs.</summary>
    private static void CheckEffect(string name, SkillDefinition row, Effect effect)
    {
        bool amount = !effect.Dice.IsNone || effect.Weapon || effect.Flat != 0 || effect.LevelTimes != 0 || effect.AddAbility is not null || effect.AddCasting;
        switch (effect.Kind)
        {
            case EffectType.Damage when !amount:
                throw new InvalidOperationException($"{name} has damage with no dice.");
            case EffectType.Damage when effect.Delivery == Delivery.Save && effect.Save is null:
                throw new InvalidOperationException($"{name} has damage on a save with no save.");
            case EffectType.Heal when !amount && effect.MaxHpShare <= 0 && effect.PoolPerLevel <= 0:
            case EffectType.TempHp when !amount:
                throw new InvalidOperationException($"{name} has a {effect.Kind} with no amount.");
            case EffectType.Aura when effect.Aura is null:
                throw new InvalidOperationException($"{name} puts on no aura.");
            case EffectType.Summon or EffectType.Transform when string.IsNullOrWhiteSpace(effect.Id):
                throw new InvalidOperationException($"{name} calls in or turns into nothing.");
            case EffectType.Summon when !EnemyCatalog.Exists(effect.Id):
                throw new InvalidOperationException($"{name} calls in {effect.Id}, who isn't in EnemyCatalog.");
            case EffectType.Transform when !FormsById.ContainsKey(effect.Id):
                throw new InvalidOperationException($"{name} turns into {effect.Id}, which isn't a form.");
            case EffectType.Rider when effect.Rider.Count == 0:
                throw new InvalidOperationException($"{name} has a rider with nothing on it.");
        }
        if (effect.Aura is AuraDefinition aura && (string.IsNullOrWhiteSpace(aura.Id) || aura.DurationMs < 0 || aura.TickMs <= 0 || aura.MaxStacks < 1))
            throw new InvalidOperationException($"{name}'s aura {aura.Id} needs an id, a duration, a tick and stacks that make sense.");
        foreach (Effect rider in effect.Rider) CheckEffect(name + "'s rider", row, rider);
    }

    // Shorthand for the partials' tables.

    /// <summary>Dice from their text.</summary>
    private static DiceExpression D(string text) => DiceExpression.Parse(text);

    /// <summary>A weapon blow: the weapon's dice and everything it adds, on an attack roll.</summary>
    private static Effect WeaponHit => new() { Kind = EffectType.Damage, Weapon = true, Delivery = Delivery.Attack };

    /// <summary>A spell attack's damage.</summary>
    private static Effect SpellHit(string dice, DamageType type, bool plus = false) =>
        new() { Kind = EffectType.Damage, Dice = D(dice), Type = type, Plus = plus, Delivery = Delivery.Attack };

    /// <summary>Damage on a failed save (half on a made one if half).</summary>
    private static Effect SaveHit(string dice, DamageType type, Ability save, bool half, bool plus = false) =>
        new() { Kind = EffectType.Damage, Dice = D(dice), Type = type, Plus = plus, Delivery = Delivery.Save, Save = save, Half = half };

    /// <summary>Damage that can't miss.</summary>
    private static Effect SureHit(string dice, DamageType type, bool plus = false) =>
        new() { Kind = EffectType.Damage, Dice = D(dice), Type = type, Plus = plus, Delivery = Delivery.Sure };

    /// <summary>A heal of dice, plus the casting ability (if casting) and the level times levelTimes.</summary>
    private static Effect Heal(string dice, bool plus = false, bool casting = true, int levelTimes = 0) =>
        new() { Kind = EffectType.Heal, Dice = D(dice), Plus = plus, AddCasting = casting, LevelTimes = levelTimes };

    /// <summary>Puts an aura on whoever On says, on a failed save if there's a save.</summary>
    private static Effect Put(AuraDefinition aura, EffectOn on = EffectOn.Targets, Ability? save = null, bool ifLanded = false) =>
        new() { Kind = EffectType.Aura, Aura = aura, On = on, Save = save, IfLanded = ifLanded };

    /// <summary>Arms the skill's reaction.</summary>
    private static Effect Arm => new() { Kind = EffectType.Arm, On = EffectOn.Self, Once = true };

    /// <summary>An aura that's a 5e condition, for so long, with a save each tick to end it if saveEnds.</summary>
    private static AuraDefinition Condition(string id, string name, ConditionId condition, int ms, Ability? saveEnds = null) =>
        new() { Id = id, Name = name, Condition = condition, DurationMs = ms, SaveEnds = saveEnds };

    /// <summary>A buff of stat mods for so long.</summary>
    private static AuraDefinition Buff(string id, string name, int ms, params StatMod[] mods) =>
        new() { Id = id, Name = name, Beneficial = true, DurationMs = ms, Mods = mods };

    /// <summary>A debuff of stat mods for so long.</summary>
    private static AuraDefinition Debuff(string id, string name, int ms, params StatMod[] mods) =>
        new() { Id = id, Name = name, DurationMs = ms, Mods = mods };

    /// <summary>A shield of temporary hit points (the amount comes from the TempHp effect), lasting so long.</summary>
    internal static AuraDefinition TempHpShield(string id, string name, int ms) =>
        new() { Id = id, Name = name, Beneficial = true, DurationMs = ms, Shield = true };
}

/// <summary>A class's seven skills, in their slots.</summary>
/// <param name="Auto">Slot 1: the auto attack (rank 1).</param>
/// <param name="Skill1">Slot 2 (rank 1).</param>
/// <param name="Skill2">Slot 3 (rank 2).</param>
/// <param name="Skill3">Slot 4 (rank 3).</param>
/// <param name="Ultimate">Slot 5 (rank 5).</param>
/// <param name="Passive1">The rank 4 passive.</param>
/// <param name="Passive2">The rank 10 passive.</param>
public sealed record ClassKit(SkillDefinition Auto, SkillDefinition Skill1, SkillDefinition Skill2, SkillDefinition Skill3,
    SkillDefinition Ultimate, SkillDefinition Passive1, SkillDefinition Passive2)
{
    /// <summary>The bar's five class slots in order: the auto, the three skills, the ultimate.</summary>
    public IReadOnlyList<SkillDefinition> Bar => new[] { Auto, Skill1, Skill2, Skill3, Ultimate };

    /// <summary>The two passives.</summary>
    public IReadOnlyList<SkillDefinition> Passives => new[] { Passive1, Passive2 };

    /// <summary>All seven.</summary>
    public IReadOnlyList<SkillDefinition> All => new[] { Auto, Skill1, Skill2, Skill3, Ultimate, Passive1, Passive2 };
}
