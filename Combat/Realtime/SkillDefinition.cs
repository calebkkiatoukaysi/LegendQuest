using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// One skill, as data: a class's auto attack, skill, ultimate, reaction or passive, or a foe's move.
/// What it costs, how often, how far it reaches, who it lands on, and what it does (its Effects, in
/// order).
/// </summary>
/// <remarks>
/// Every skill in 10-classes.md is one of these, by its id (`&lt;class code&gt;-&lt;kebab name&gt;`), in
/// SkillCatalog; so is every foe's telegraphed move. A skill is data the engine reads, never code: a
/// new class is new rows. Times are milliseconds. The conversion from the class tables' turns is
/// 11-combat.md §4.2: CD 1 to 4 are 6, 8, 12 and 16 s, once a fight is a 45 s cooldown that's ready at
/// the start, a turn is 3 s. Its clip is Motion if set; otherwise a weapon auto plays auto-&lt;kind&gt;-n and
/// anything else skill-&lt;id&gt; (contract §2.3); its blows land on that clip's hit or release events
/// (MotionTimings).
/// </remarks>
public sealed record SkillDefinition
{
    /// <summary>Its id: `&lt;class code&gt;-&lt;kebab name&gt;` for a class skill, `foe-&lt;name&gt;` for a foe's.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on the bar and in the log.</summary>
    public required string Name { get; init; }

    /// <summary>The class whose kit it's in, or null for a foe's move or a form's.</summary>
    public ClassId? Class { get; init; }

    /// <summary>The class rank it unlocks at: 1 for the auto and skill 1, 2 and 3 for the next skills, 4 and 10 for the passives, 5 for the ultimate.</summary>
    public int Rank { get; init; } = 1;

    /// <summary>What kind of skill it is: the auto attack, an action on the GCD, a bonus off it, an armed reaction, or a passive.</summary>
    public SkillKind Kind { get; init; } = SkillKind.Action;

    /// <summary>The Focus it costs.</summary>
    public int Focus { get; init; }

    /// <summary>The hit points it costs as well (the Hemomancer's), never taking the user below 1.</summary>
    public int HpCost { get; init; }

    /// <summary>Its own cooldown, before haste, or 0 for the GCD only.</summary>
    public int CooldownMs { get; init; }

    /// <summary>Whether it's the class's ultimate: once a fight in the tables, a 45 s cooldown that's ready at the start, a short wind-up everyone sees.</summary>
    public bool Ultimate { get; init; }

    /// <summary>A cast time before its clip, shown on a cast bar, during which the caster is rooted: 0 for most skills.</summary>
    public int CastMs { get; init; }

    /// <summary>A foe's (or an ultimate's) visible wind-up before its clip: the telegraph, with its ground marker, after which the area's fixed.</summary>
    public int TelegraphMs { get; init; }

    /// <summary>Whether a cast or a telegraph can be stopped: by a Stun, a knockdown, moving, a Counterspell, an interrupt, or failing the Con save a hit forces.</summary>
    public bool Interruptible { get; init; } = true;

    /// <summary>How far it reaches: the weapon's reach, melee 30, reach 42, short 100, long 220, the room, or the user alone.</summary>
    public SkillRange Range { get; init; } = SkillRange.Weapon;

    /// <summary>Who it's aimed at: a foe, an ally (the user counts), the user, or nobody (an area around the user, or the room).</summary>
    public SkillTarget Target { get; init; } = SkillTarget.Foe;

    /// <summary>Who else it catches, by 11-combat.md §4.3's shapes.</summary>
    public AreaShape Area { get; init; } = AreaShape.Single;

    /// <summary>The area's size in world pixels where its shape takes one (a Ring's radius); 0 for the shape's own.</summary>
    public float AreaPx { get; init; }

    /// <summary>How many: N random foes, N allies, the darts' spread, a chain's links.</summary>
    public int Count { get; init; }

    /// <summary>What it does, in order, to whoever each effect says.</summary>
    public IReadOnlyList<Effect> Effects { get; init; } = Array.Empty<Effect>();

    /// <summary>How many separate blows it lands, each on a hit or release event of its clip: three darts, two beams. Each blow applies every effect not marked Once.</summary>
    public int Strikes { get; init; } = 1;

    /// <summary>Whether it lands one more blow at level 5 and another at 9 (Eldritch Blast's beams).</summary>
    public bool StrikesByLevel { get; init; }

    /// <summary>Whether it's a blow of the weapon in hand: it rolls the weapon's attack, reaches as the weapon does, and as an auto plays the weapon's swing.</summary>
    public bool Weapon { get; init; }

    /// <summary>For a projectile, the school it flies as (its speed and colour); null for a blow that lands where it's struck.</summary>
    public DamageType? Projectile { get; init; }

    /// <summary>Its clip, if not the default (auto-&lt;kind&gt;-n for a weapon auto, skill-&lt;id&gt; for the rest).</summary>
    public string Motion { get; init; }

    /// <summary>For an armed reaction, what sets it off.</summary>
    public ReactionTrigger Trigger { get; init; }

    /// <summary>For an armed reaction, how long it waits for its trigger.</summary>
    public int ArmMs { get; init; } = 4000;

    /// <summary>Whether it can only be used once on each foe (Pick Pocket).</summary>
    public bool OncePerFoe { get; init; }

    /// <summary>Whether the auras it puts on are concentration: one at a time, and a hit can break them.</summary>
    public bool Concentration { get; init; }

    /// <summary>For a passive, what it always adds while the class has the rank for it.</summary>
    public IReadOnlyList<StatMod> Mods { get; init; } = Array.Empty<StatMod>();

    /// <summary>For a passive, what it does when something happens.</summary>
    public IReadOnlyList<Proc> Procs { get; init; } = Array.Empty<Proc>();

    /// <summary>For a passive, what it gives allies near the user (Aura of Protection's Charisma to saves).</summary>
    public IReadOnlyList<StatMod> AllyMods { get; init; } = Array.Empty<StatMod>();

    /// <summary>How near an ally has to be for AllyMods, in world pixels; 0 for the whole room.</summary>
    public float AllyRadius { get; init; }

    /// <summary>For a foe's move, its save DC (the stat block's, before the difficulty), or 0 for the user's own.</summary>
    public int Dc { get; init; }

    /// <summary>For a foe's move, its attack bonus (the stat block's, before the difficulty), or 0 for the user's main attack's.</summary>
    public int AttackBonus { get; init; }

    /// <summary>What it's for, to the AI: what it scores the skill by.</summary>
    public AiUse Ai { get; init; } = AiUse.Damage;

    /// <summary>What it's worth to the AI on top of its sums, in points of damage: what a buff, a control effect or a taunt buys.</summary>
    public double AiValue { get; init; }

    /// <summary>The fewest targets in its area the AI wants before it uses it (an area skill on one foe is a waste).</summary>
    public int AiMinTargets { get; init; } = 1;

    /// <summary>What it says it does, for the tooltip.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Whether it's on the GCD: action skills are, and the auto, bonus skills, reactions and passives aren't.</summary>
    public bool OnGcd => Kind == SkillKind.Action;

    /// <summary>Whether it's a move with a target and an effect at all (not a passive).</summary>
    public bool Active => Kind != SkillKind.Passive;

    /// <summary>How many blows it lands for someone of a level.</summary>
    /// <param name="level">Their level.</param>
    public int StrikesAt(int level) => StrikesByLevel ? Strikes + (level >= 5 ? 1 : 0) + (level >= 9 ? 1 : 0) : Strikes;

    /// <summary>The clip to play for it, for a fighter with a weapon kind, on their nth swing.</summary>
    /// <param name="weapon">Their weapon kind.</param>
    /// <param name="swing">How many swings they've made, for the auto's 1, 2, 3.</param>
    public string MotionFor(WeaponKind weapon, int swing)
    {
        if (Motion is not null) return Motion;
        if (Weapon && Kind == SkillKind.Auto) return WeaponKinds.AutoClip(weapon, swing);
        return "skill-" + Id;
    }

    /// <summary>The clip it plays by default, for --validate and the contract's list: null for a weapon auto and a passive, which have none of their own.</summary>
    public string OwnMotion => !Active ? null : Motion ?? (Weapon && Kind == SkillKind.Auto ? null : "skill-" + Id);

    /// <summary>What sort of move it is, for its clip's fallback timing.</summary>
    public MotionKind MotionKind => Kind == SkillKind.Auto ? MotionKind.Auto
        : Ultimate ? MotionKind.Ultimate
        : Projectile is not null || Range is SkillRange.Long or SkillRange.Room && !Weapon ? MotionKind.Release
        : MotionKind.Melee;
}

/// <summary>One thing a skill does, to whoever On says.</summary>
/// <remarks>
/// One record for every kind, with the fields each kind reads; the rest stay at their defaults. A
/// skill's effects apply in order on each of its blows, to that blow's targets, and a later effect can
/// ask for the earlier blow to have landed (IfLanded): Vicious Mockery's disadvantage only on a failed
/// save, Thunderwave's push only on a failed save.
/// </remarks>
public sealed record Effect
{
    /// <summary>What it does.</summary>
    public EffectType Kind { get; init; }

    /// <summary>Who it lands on.</summary>
    public EffectOn On { get; init; } = EffectOn.Targets;

    /// <summary>Whether it applies on the first blow only, not on each of a multi-blow skill's.</summary>
    public bool Once { get; init; }

    /// <summary>Whether it only applies to a target the effect before it landed on (hit, or failed its save).</summary>
    public bool IfLanded { get; init; }

    // Amounts: damage, healing, temporary hit points.

    /// <summary>The dice.</summary>
    public DiceExpression Dice { get; init; }

    /// <summary>Whether the dice go up at levels 5 and 9 (the tables' ⁺).</summary>
    public bool Plus { get; init; }

    /// <summary>Whether it's the weapon's dice, with everything the weapon adds (its ability, magic, gauntlets), WeaponTimes over.</summary>
    public bool Weapon { get; init; }

    /// <summary>How many times the weapon's dice are rolled (Last Harvest's three).</summary>
    public int WeaponTimes { get; init; } = 1;

    /// <summary>An ability whose modifier is added.</summary>
    public Ability? AddAbility { get; init; }

    /// <summary>Whether the user's casting ability's modifier is added (or the better of Strength and Dexterity for a class that doesn't cast).</summary>
    public bool AddCasting { get; init; }

    /// <summary>The user's level times this is added (1 for "+ level", 3 for Wild Shape's 3 × level).</summary>
    public int LevelTimes { get; init; }

    /// <summary>A flat number added.</summary>
    public int Flat { get; init; }

    /// <summary>The kind of damage, or null for the weapon's.</summary>
    public DamageType? Type { get; init; }

    /// <summary>How it reaches its target: an attack roll (the weapon's for a weapon blow, a spell attack otherwise), a saving throw, or a sure thing.</summary>
    public Delivery Delivery { get; init; } = Delivery.Sure;

    /// <summary>The saving throw, for Delivery.Save or a condition's.</summary>
    public Ability? Save { get; init; }

    /// <summary>Whether a made save halves it (otherwise a made save stops it).</summary>
    public bool Half { get; init; }

    /// <summary>Whether Sneak Attack rides on it when its conditions are met.</summary>
    public bool Sneak { get; init; }

    /// <summary>Whether Sneak Attack rides on it whatever the conditions (Shadow Dance).</summary>
    public bool ForceSneak { get; init; }

    /// <summary>Whether undead and fiends make their save against it with disadvantage (Dawnlight).</summary>
    public bool UndeadDisadvantage { get; init; }

    /// <summary>For Heal: it heals from a pool of this many hit points a level, a fight (Lay on Hands), as much as the target is missing.</summary>
    public int PoolPerLevel { get; init; }

    /// <summary>Whether a critical does half as much again on top of 5e's doubled dice.</summary>
    public bool CritsHarder { get; init; }

    /// <summary>Whether it ignores resistance (Spire Lance).</summary>
    public bool IgnoreResistance { get; init; }

    /// <summary>The share of the damage dealt the user heals (Drain all of it, Last Harvest half).</summary>
    public double Lifesteal { get; init; }

    /// <summary>The share of the damage dealt each ally heals (Crimson Tide's quarter).</summary>
    public double PartyHealShare { get; init; }

    /// <summary>Whether a Marked target takes it twice (Rain of Arrows).</summary>
    public bool TwiceIfMarked { get; init; }

    /// <summary>The share of it targets after the first take (Chain Lightning's half), or 1.</summary>
    public double OthersShare { get; init; } = 1.0;

    /// <summary>Extra dice against undead and fiends (Divine Smite's 1d8), and with disadvantage on their save.</summary>
    public DiceExpression VsUndead { get; init; }

    // Other kinds.

    /// <summary>The aura it puts on (Kind Aura, Taunt).</summary>
    public AuraDefinition Aura { get; init; }

    /// <summary>How far, in world pixels: a knockback, a dash.</summary>
    public float Distance { get; init; }

    /// <summary>The conditions it takes off (Cleanse); empty takes off every harmful aura.</summary>
    public IReadOnlyList<ConditionId> Cleanses { get; init; } = Array.Empty<ConditionId>();

    /// <summary>Who it calls in (Summon), by EnemyCatalog id, or the form it takes (Transform), by SkillCatalog form id.</summary>
    public string Id { get; init; }

    /// <summary>How long a summon, a form or a rider lasts; 0 for the rest of the fight.</summary>
    public int DurationMs { get; init; }

    /// <summary>The effects a rider adds to the user's next hit (Divine Smite, Poisoned Edge).</summary>
    public IReadOnlyList<Effect> Rider { get; init; } = Array.Empty<Effect>();

    /// <summary>A number for the kinds that need one: Focus gained, a share of gold in percent, how many to summon.</summary>
    public int Amount { get; init; }

    /// <summary>For Heal: a share of the most hit points instead of dice (Nobody Left Behind's quarter).</summary>
    public double MaxHpShare { get; init; }

    /// <summary>For Heal: it only gets the Downed up (and heals nobody standing).</summary>
    public bool DownedOnly { get; init; }

    // Who and where it counts (the advanced classes' kits).

    /// <summary>Whether it only applies to whoever the effect before it didn't land on (a made save): Bleed the Clock's Slowed for those who save against the freeze.</summary>
    public bool IfSaved { get; init; }

    /// <summary>For Heal: it heals only those standing (Rally's heal, beside its own getting up of the Downed).</summary>
    public bool StandingOnly { get; init; }

    /// <summary>Whether it only lands on a target an ally of the user hit in the last 3 s ("a foe already hit this round": Cudgel Blow's knockdown).</summary>
    public bool IfHitByAlly { get; init; }

    /// <summary>Whether it only lands on undead and fiends (Sunburst's blinding, Gilded Dawn's scorch).</summary>
    public bool UndeadOnly { get; init; }

    /// <summary>Whether it leaves the user out of an area that would catch them (Hold keeps the rest of the party out of reach, not the Bulwark).</summary>
    public bool NotSelf { get; init; }

    /// <summary>Only on targets within this of the user, in world pixels (feet to feet, sizes added); 0 for anywhere. Grapeshot's front row, Force Bolt's push on a foe in melee.</summary>
    public float WithinPx { get; init; }

    /// <summary>Where it happens at all: anywhere, only outdoors, or only indoors (Broadside and its Grapeshot), by BattleRules.Outdoors.</summary>
    public Place Place { get; init; }

    /// <summary>For Strike: every other ally standing within reach of the target strikes it, rather than the user (My Father's Sword).</summary>
    public bool ByAllies { get; init; }
}

/// <summary>Where an effect happens (Effect.Place).</summary>
public enum Place
{
    /// <summary>Wherever the fight is.</summary>
    Anywhere,

    /// <summary>Only in a fight outdoors.</summary>
    Outdoors,

    /// <summary>Only in a fight indoors.</summary>
    Indoors,
}

/// <summary>A buff, a debuff, a condition, damage or healing over time, a shield: anything that sits on someone for a while.</summary>
/// <remarks>
/// 11-combat.md §4.4. The same aura from the same source refreshes; Stacks adds a stack (to MaxStacks)
/// and refreshes; from another source it's a second aura. One that ticks does so every TickMs: its
/// damage or healing (which can't crit, be dodged or blocked, and counts as a hit for the Focus models
/// that count ticks), or a save to end it.
/// </remarks>
public sealed record AuraDefinition
{
    /// <summary>Its id, for the icon and the refresh rule.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on the frame.</summary>
    public required string Name { get; init; }

    /// <summary>Whether it helps whoever it's on (a buff) rather than hurting them.</summary>
    public bool Beneficial { get; init; }

    /// <summary>How long it lasts; 0 until something takes it off (used up, cleansed, the fight's end).</summary>
    public int DurationMs { get; init; }

    /// <summary>The 5e condition it is, which carries the condition's rules (Restrained, Stunned, Prone, Poisoned...), or null.</summary>
    public ConditionId? Condition { get; init; }

    /// <summary>What it changes while it's on, per stack.</summary>
    public IReadOnlyList<StatMod> Mods { get; init; } = Array.Empty<StatMod>();

    /// <summary>Whether putting it on again adds a stack.</summary>
    public bool Stacks { get; init; }

    /// <summary>The most stacks it holds.</summary>
    public int MaxStacks { get; init; } = 5;

    /// <summary>How often it ticks.</summary>
    public int TickMs { get; init; } = 3000;

    /// <summary>What each tick does in damage (or healing, with TickHeals), per stack; None for no tick.</summary>
    public DiceExpression TickDice { get; init; }

    /// <summary>Whether the tick's dice go up at levels 5 and 9.</summary>
    public bool TickPlus { get; init; }

    /// <summary>The kind of the tick's damage.</summary>
    public DamageType TickType { get; init; }

    /// <summary>Whether the tick heals rather than hurts.</summary>
    public bool TickHeals { get; init; }

    /// <summary>A save made on each tick to end it (Restrained's Str save), at its source's DC, or null.</summary>
    public Ability? SaveEnds { get; init; }

    /// <summary>What uses it up: its bearer's next attack roll, save, the next attack on them, their next hit.</summary>
    public AuraUse UsedBy { get; init; }

    /// <summary>How many uses it has before it's gone (Mirror Image's three images), for UsedBy; 1 otherwise.</summary>
    public int Charges { get; init; } = 1;

    /// <summary>Whether it's a shield of temporary hit points: its Amount is taken off hits first.</summary>
    public bool Shield { get; init; }

    /// <summary>Whether cleansing can't take it off (Thirty Summers' healing over time).</summary>
    public bool Uncleansable { get; init; }

    /// <summary>For a decoy (UsedBy AttackTaken, no condition): the chance in percent that an attack on the bearer goes at a decoy and misses, using a charge (Mirror Image's 50, Rooftops' 100); 0 if it isn't one.</summary>
    public int Decoy { get; init; }

    /// <summary>Whether it marks its bearer for the skills that care about Marked (Hunter's Mark's +1d6, Rain of Arrows' twice) without the Marked condition's advantage.</summary>
    public bool CountsAsMarked { get; init; }
}

/// <summary>One thing a passive or an aura changes about someone.</summary>
/// <param name="Stat">What it changes.</param>
/// <param name="Amount">By how much (points, or percent for the Pct stats and Haste and Crit).</param>
/// <param name="Dice">Dice it adds, for the dice stats.</param>
/// <param name="Type">The damage type it's about: what's resisted, the kind of the extra dice; null for all.</param>
/// <param name="Condition">The condition it's about, for Immune.</param>
/// <param name="When">When it counts.</param>
/// <param name="Of">An ability whose modifier (the source's) is added to Amount: Aura of Protection's Charisma.</param>
public readonly record struct StatMod(Stat Stat, int Amount = 0, DiceExpression Dice = default, DamageType? Type = null, ConditionId? Condition = null,
    When When = When.Always, Ability? Of = null);

/// <summary>What a passive does when something happens.</summary>
/// <param name="Trigger">What sets it off.</param>
/// <param name="Effects">What it does: on the target of the hit for OnHit, on the user otherwise.</param>
/// <param name="CooldownMs">How long before it can go off again (Empowered's once a turn is 3 s).</param>
/// <param name="OncePerFight">Whether it only goes off once a fight.</param>
/// <param name="When">When it counts.</param>
/// <param name="ChancePct">The chance it goes off when its trigger happens, in percent (Echo Strike's one in five).</param>
public sealed record Proc(ProcTrigger Trigger, IReadOnlyList<Effect> Effects, int CooldownMs = 0, bool OncePerFight = false, When When = When.Always,
    int ChancePct = 100);

/// <summary>A form a skill turns someone into for a while (Wild Shape), with its own bar.</summary>
/// <param name="Id">The form's id.</param>
/// <param name="Name">What it's called.</param>
/// <param name="Auto">Its auto attack, by skill id.</param>
/// <param name="Skills">Its skills in bar order after the auto, by skill id.</param>
/// <param name="Weapon">How it fights, for its auto's clips.</param>
/// <param name="SwingMs">Its swing timer.</param>
public sealed record FormDefinition(string Id, string Name, string Auto, IReadOnlyList<string> Skills, WeaponKind Weapon, int SwingMs);

/// <summary>What kind of skill a skill is.</summary>
public enum SkillKind
{
    /// <summary>The auto attack, on its own swing timer.</summary>
    Auto,

    /// <summary>An action (A): on the GCD.</summary>
    Action,

    /// <summary>A bonus action (B): off the GCD, sharing a short lockout with the other bonus skills.</summary>
    Bonus,

    /// <summary>A reaction (R): pressed to arm it, it fires on its trigger.</summary>
    Reaction,

    /// <summary>A passive (P): always on.</summary>
    Passive,
}

/// <summary>How far a skill reaches (11-combat.md §2.2).</summary>
public enum SkillRange
{
    /// <summary>The weapon's reach: melee, reach, or long for a ranged weapon.</summary>
    Weapon,

    /// <summary>30 px between the feet.</summary>
    Melee,

    /// <summary>42 px: spears, whips.</summary>
    Reach,

    /// <summary>100 px: thrown things, short spells.</summary>
    Short,

    /// <summary>220 px: bows, most spells.</summary>
    Long,

    /// <summary>Anywhere in the room.</summary>
    Room,

    /// <summary>The user only.</summary>
    Self,
}

/// <summary>Who a skill's aimed at.</summary>
public enum SkillTarget
{
    /// <summary>A foe.</summary>
    Foe,

    /// <summary>An ally, the user included.</summary>
    Ally,

    /// <summary>The user.</summary>
    Self,

    /// <summary>Nobody in particular: an area around the user, or the room.</summary>
    None,
}

/// <summary>Who else a skill catches (11-combat.md §4.3).</summary>
public enum AreaShape
{
    /// <summary>The target alone.</summary>
    Single,

    /// <summary>Everyone on the target's side within AreaPx of the target.</summary>
    Ring,

    /// <summary>The target and every foe within 36 px of it.</summary>
    Neighbours,

    /// <summary>Every foe within 48 px of the target (a row).</summary>
    Row,

    /// <summary>Every foe within 60 px in front of the user (the front row of foes).</summary>
    Front,

    /// <summary>A line from the user through the target, 24 px wide, to the skill's range (a column).</summary>
    Line,

    /// <summary>Every foe in the fight.</summary>
    AllFoes,

    /// <summary>Every ally in the fight.</summary>
    AllAllies,

    /// <summary>Count picks among the engaged foes, one a blow, and they can repeat.</summary>
    RandomFoes,

    /// <summary>The target and up to Count - 1 other foes nearest it, a blow each in turn (the darts).</summary>
    Spread,

    /// <summary>Allies within 60 px of the user (your row).</summary>
    AlliesNear,

    /// <summary>Allies further from the foes than the user (your back row).</summary>
    AlliesBehind,

    /// <summary>The target and the Count - 1 allies nearest them.</summary>
    Allies,

    /// <summary>The target, then Count more foes, each the nearest to the last.</summary>
    Chain,
}

/// <summary>How a damage effect reaches its target.</summary>
public enum Delivery
{
    /// <summary>It can't miss.</summary>
    Sure,

    /// <summary>An attack roll against AC: the weapon's for a weapon blow, a spell attack otherwise.</summary>
    Attack,

    /// <summary>A saving throw against the user's DC.</summary>
    Save,
}

/// <summary>What an effect does.</summary>
/// <remarks>New ones go on the end; each new one needs its case in Battle.Apply.</remarks>
public enum EffectType
{
    /// <summary>Hurts.</summary>
    Damage,

    /// <summary>Heals, and gets a Downed ally up.</summary>
    Heal,

    /// <summary>Temporary hit points: a shield aura.</summary>
    TempHp,

    /// <summary>Puts an aura on.</summary>
    Aura,

    /// <summary>Takes conditions off.</summary>
    Cleanse,

    /// <summary>Knocks them Distance away from the user.</summary>
    Knockback,

    /// <summary>Pulls them to the user's melee reach.</summary>
    Pull,

    /// <summary>The user dashes Distance away from the nearest foe (or towards the target, with a negative Distance).</summary>
    Dash,

    /// <summary>The user steps to just behind the target.</summary>
    Teleport,

    /// <summary>The user swaps places with the ally furthest back.</summary>
    Swap,

    /// <summary>Taunts them: they must attack the user (the Taunted aura, and the top of their threat).</summary>
    Taunt,

    /// <summary>Calls in Amount (at least one) of a stat block (Id) on the user's side.</summary>
    Summon,

    /// <summary>Turns the user into a form (Id) for DurationMs.</summary>
    Transform,

    /// <summary>Arms the skill's reaction.</summary>
    Arm,

    /// <summary>Adds Rider's effects to the user's next hit.</summary>
    Rider,

    /// <summary>Gives Focus: Amount.</summary>
    Focus,

    /// <summary>Take another action: the GCD resets, and the next action skill used starts no cooldown.</summary>
    Reset,

    /// <summary>Takes Amount percent of the target's loot as gold (data for the screen's result panel).</summary>
    Gold,

    /// <summary>Their hit points go back to what they were DurationMs ago, if that's more.</summary>
    Rewind,

    /// <summary>Takes beneficial auras off foes (Reveal's end of Mirror Images and invisibility).</summary>
    Dispel,

    /// <summary>Stops their cast or telegraph.</summary>
    Interrupt,

    /// <summary>A free weapon blow from the user at the target (Riposte's Strike, Crownsworn's every ally a Strike).</summary>
    Strike,

    /// <summary>Rerolls a failed save (a passive's proc).</summary>
    Reroll,

    /// <summary>Drops to 1 hit point instead of 0 (a passive's proc).</summary>
    StayUp,

    /// <summary>The user charges along the skill's line to its end (the Uprooting Charge).</summary>
    Charge,

    /// <summary>The user vanishes for DurationMs, then steps out behind their target (Smoke Bomb).</summary>
    Vanish,

    /// <summary>In a brawl or a nonlethal fight, takes a foe at or under MaxHpShare of its hit points out of the fight (Arrest).</summary>
    TakeOut,

    /// <summary>Throws the bag's best thing for throwing (Alchemist's Fire) at double strength, using it up; or, with nothing in the bag, whatever's to hand: Dice of Type, doubled (Throw Anything).</summary>
    Throw,

    /// <summary>One of Rider's effects, picked at random, on whoever On says (Rummage's random potion).</summary>
    OneOf,

    /// <summary>Their auto attack swings the moment it can: the swing timer runs out now (the head start of Count the Ground and No Procedure).</summary>
    Ready,

    /// <summary>A passive's proc on a skill being used: the skill happens again, at half strength, just after (Echo Strike).</summary>
    Echo,
}

/// <summary>Who an effect lands on.</summary>
public enum EffectOn
{
    /// <summary>The blow's targets: the skill's area as it fell.</summary>
    Targets,

    /// <summary>The user.</summary>
    Self,

    /// <summary>The one the skill was aimed at, whatever the area.</summary>
    Primary,

    /// <summary>Every ally standing.</summary>
    AllAllies,

    /// <summary>Every foe in the fight.</summary>
    AllFoes,

    /// <summary>Whoever set a reaction off.</summary>
    Attacker,

    /// <summary>Every ally but the user.</summary>
    OtherAllies,

    /// <summary>A random foe standing, other than the blow's target if there's anyone else (Prism Ray's second ray).</summary>
    RandomFoe,
}

/// <summary>What sets an armed reaction off (11-combat.md §4.2).</summary>
public enum ReactionTrigger
{
    /// <summary>Nothing: not a reaction.</summary>
    None,

    /// <summary>A melee attack that misses the user (Riposte).</summary>
    MeleeMissOnYou,

    /// <summary>An attack on the user, before it's rolled (Shield).</summary>
    AttackOnYou,

    /// <summary>A hit on the user, after it lands (Hellish Rebuke).</summary>
    HitOnYou,

    /// <summary>A foe starting a cast or a telegraph within the skill's range (Counterspell).</summary>
    FoeCast,

    /// <summary>An attack on the ally it was armed over: the user takes it instead (Cover).</summary>
    AttackOnAlly,

    /// <summary>A ranged attack or spell at an ally: it goes at a random foe instead (Refract).</summary>
    RangedOnAlly,
}

/// <summary>What changes (StatMod.Stat).</summary>
public enum Stat
{
    /// <summary>Armour class.</summary>
    Ac,

    /// <summary>To hit, in points.</summary>
    ToHit,

    /// <summary>To hit, in dice (Bless's 1d4, Battle Hymn's 1d6).</summary>
    ToHitDice,

    /// <summary>Damage on each hit, in points.</summary>
    Damage,

    /// <summary>Damage on each hit, in dice of a type (Radiant Strikes' 1d8 radiant).</summary>
    DamageDice,

    /// <summary>Haste, in percent (Slowed is -50).</summary>
    Haste,

    /// <summary>Critical chance, in percent.</summary>
    Crit,

    /// <summary>Damage taken, in percent (Shield Wall's -50).</summary>
    DamageTakenPct,

    /// <summary>Resistance to a damage type (Type), or all.</summary>
    Resist,

    /// <summary>Vulnerability to a damage type.</summary>
    Vulnerable,

    /// <summary>Immunity to a condition (Condition).</summary>
    Immune,

    /// <summary>Saving throws, in points.</summary>
    Save,

    /// <summary>Saving throws, in dice.</summary>
    SaveDice,

    /// <summary>Movement speed, in percent.</summary>
    SpeedPct,

    /// <summary>Healing done, in percent.</summary>
    HealingPct,

    /// <summary>Healing done, in points per heal.</summary>
    HealingFlat,

    /// <summary>Focus gained every 3 s.</summary>
    FocusPerTurn,

    /// <summary>Save DCs, in points.</summary>
    Dc,

    /// <summary>Attacks on the bearer have advantage.</summary>
    AttackersAdvantage,

    /// <summary>Attacks on the bearer have disadvantage.</summary>
    AttackersDisadvantage,

    /// <summary>The bearer's attacks have advantage.</summary>
    Advantage,

    /// <summary>The bearer's attacks have disadvantage.</summary>
    Disadvantage,

    /// <summary>The bearer takes Dice more from the aura's source's hits (Hex, Hunter's Mark).</summary>
    TakenFromSource,

    /// <summary>The bearer takes Dice more from everyone's hits.</summary>
    TakenFromAll,

    /// <summary>Threat generated, in percent.</summary>
    Threat,

    /// <summary>Area damage taken, in percent; with Amount -100, none on a made save (Evasion).</summary>
    AreaTakenPct,

    /// <summary>Area damage halved, and none on a made save (Evasion).</summary>
    Evasion,

    /// <summary>The bearer's saves have advantage against a condition (Condition), or all.</summary>
    SaveAdvantage,

    /// <summary>The bearer's next skill costs no Focus.</summary>
    FreeSkill,

    /// <summary>Cooldowns run faster, in percent (separate from haste, so it doesn't touch the swing).</summary>
    CooldownRate,

    /// <summary>Potions heal more, in percent.</summary>
    PotionPct,

    /// <summary>The bearer's next hit is a critical (Feint).</summary>
    SureCrit,

    /// <summary>Restrained that the bearer puts on lasts Amount milliseconds longer (Knots Hold).</summary>
    RestrainedMs,

    /// <summary>Damage can't break the bearer's concentration (Spire Training).</summary>
    KeepConcentration,

    /// <summary>Attacks on the bearer get Amount to hit (Read the Wound's +2).</summary>
    AttackersToHit,

    /// <summary>Amount to every attack roll and save for each ally of the bearer's still standing (Name on the Door).</summary>
    RollsPerAlly,

    /// <summary>The bearer's d20 comes up 20 (Lucky Stone, used up by their next roll).</summary>
    Natural20,

    /// <summary>Healing the bearer takes, in percent (Gilded Dawn's -100: can't heal).</summary>
    HealingTakenPct,

    /// <summary>A foe has to make a Wisdom save against the source's DC to pick the bearer as its target (Sanctuary).</summary>
    Sanctuary,

    /// <summary>When an ally under the bearer's Sanctuary is attacked, they heal Dice (Bees Guard the Honey).</summary>
    SanctuaryHeal,

    /// <summary>Foes can't pick the bearer as their target, and turn to someone else (Hold).</summary>
    Untargetable,

    /// <summary>Attacks on the bearer go at the source instead while the source stands (Nobody Left Behind's Cover for everyone).</summary>
    Covered,

    /// <summary>The bearer's healing over time ticks on the Downed too, Amount times over (Thirty Summers).</summary>
    HotOnDowned,

    /// <summary>Beasts don't pick the bearer as their target unless the bearer has hurt them (They Know Me).</summary>
    BeastsIgnore,

    /// <summary>Gold from fights, in percent (Captain's Share), for the result panel (Battle.GoldBonusPct).</summary>
    GoldPct,

    /// <summary>Shop prices, in percent (Haggle's -20), for the shop to read off the passive; the fight ignores it.</summary>
    PricePct,
}

/// <summary>When a StatMod counts.</summary>
public enum When
{
    /// <summary>Always.</summary>
    Always,

    /// <summary>On melee attacks.</summary>
    Melee,

    /// <summary>On ranged attacks.</summary>
    Ranged,

    /// <summary>On spells (anything not a weapon blow).</summary>
    Spell,

    /// <summary>Against a Marked target.</summary>
    VsMarked,

    /// <summary>While the bearer is below half their hit points.</summary>
    BelowHalf,

    /// <summary>Against undead and fiends.</summary>
    VsUndead,

    /// <summary>In a dark place (underground, or at night).</summary>
    Dark,

    /// <summary>In daylight.</summary>
    Daylight,

    /// <summary>On a target below a quarter of their hit points.</summary>
    TargetBelowQuarter,

    /// <summary>Against a target Restrained or Prone.</summary>
    VsHeld,

    /// <summary>On the bearer's own class skills (rank 6's +1 to hit).</summary>
    ClassSkill,

    /// <summary>Against a target Restrained or Prone by the mod's own source (Keeper of the Peace).</summary>
    VsHeldBySource,

    /// <summary>Against a foe whose target is one of the bearer's allies, not the bearer (Lane Rules).</summary>
    VsAttackingOthers,

    /// <summary>On someone other than the bearer (Sorry Somewhere Useful's heals and shields on others).</summary>
    OnOthers,
}

/// <summary>What sets a passive's proc off.</summary>
public enum ProcTrigger
{
    /// <summary>A hit the bearer lands.</summary>
    OnHit,

    /// <summary>A heal the bearer gives someone else.</summary>
    OnHealOther,

    /// <summary>A save the bearer fails.</summary>
    OnFailedSave,

    /// <summary>The bearer dropping to 0.</summary>
    OnDropped,

    /// <summary>An ally dropping to 0.</summary>
    OnAllyDropped,

    /// <summary>The fight starting.</summary>
    FightStart,

    /// <summary>The fight being won (for the screen to apply: Natural Recovery, Song of Rest).</summary>
    AfterFight,

    /// <summary>The bearer dodging an attack.</summary>
    OnDodge,

    /// <summary>A skill the bearer uses (Echo Strike).</summary>
    OnSkillUsed,

    /// <summary>A foe putting a control condition (Stunned, Frozen, Restrained, Prone, Charmed, Frightened, Blinded) on one of the bearer's allies (Choose It Yourself).</summary>
    OnAllyControlled,

    /// <summary>A foe winding up a big move (a telegraph of 1 s or more): the effects go at the foe (Somebody Allowed to Say No).</summary>
    OnFoeBigMove,
}

/// <summary>What uses an aura up.</summary>
public enum AuraUse
{
    /// <summary>Nothing: it runs its time.</summary>
    None,

    /// <summary>The bearer's next attack roll (Mocked, Feinting).</summary>
    NextAttack,

    /// <summary>The bearer's next attack roll or save (Inspired).</summary>
    NextRoll,

    /// <summary>The next attack on the bearer (Marked, Shield's ward, Mirror Image's images).</summary>
    AttackTaken,

    /// <summary>The next skill the bearer uses (Archmage's Patience).</summary>
    NextSkill,
}

/// <summary>What a skill is for, to the AI.</summary>
public enum AiUse
{
    /// <summary>Damage.</summary>
    Damage,

    /// <summary>Healing.</summary>
    Heal,

    /// <summary>A buff on allies.</summary>
    Buff,

    /// <summary>A debuff or control on foes.</summary>
    Control,

    /// <summary>Taking the foes' attention.</summary>
    Taunt,

    /// <summary>Keeping the user alive (a shield, Second Wind, a dodge).</summary>
    Defensive,

    /// <summary>Calling someone in.</summary>
    Summon,

    /// <summary>Anything else; the AI leaves it alone unless AiValue says otherwise.</summary>
    Utility,
}
