using System;
using System.Collections.Generic;
using System.Numerics;
using LegendQuest.Characters;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// Someone in a real-time fight: a party member, a guest, a foe or a summon. Where they stand, how
/// hurt they are, their Focus, their auras, what they're doing and their timers.
/// </summary>
/// <remarks>
/// The engine (Battle) changes it; the screen and the AI only read it. Hit points are the fighter's
/// own for the length of the fight: Battle.ApplyToParty writes the party's back when it's over. Every
/// time is fight time in milliseconds (Battle.NowMs); every distance is world pixels.
/// </remarks>
public sealed class Fighter
{
    /// <summary>The most Focus anyone holds.</summary>
    public const int MaxFocus = 100;

    internal readonly List<Aura> AuraList = new();
    internal readonly Dictionary<string, int> CooldownUntil = new(StringComparer.Ordinal);
    internal readonly List<SkillDefinition> BarSkills = new();
    internal readonly List<SkillDefinition> PassiveSkills = new();
    internal readonly List<Armed> ArmedList = new();
    internal readonly List<Rider> Riders = new();
    internal readonly List<ThreatEntry> ThreatTable = new();
    internal readonly HashSet<string> UsedOnFoe = new(StringComparer.Ordinal);
    internal readonly HashSet<string> ProcsSpent = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, int> ProcReadyAt = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, int> Pools = new(StringComparer.Ordinal);
    internal readonly int[] HpHistory = new int[16];
    internal readonly List<(Fighter Who, int AtMs)> RecentHits = new();
    internal readonly Dictionary<string, int> CooldownTotal = new(StringComparer.Ordinal);
    internal readonly Dictionary<string, int> CooldownOverride = new(StringComparer.Ordinal);
    internal readonly HashSet<Fighter> HurtBy = new();
    internal readonly Dictionary<Aura, bool> SanctuarySaves = new();

    /// <summary>Sets a fighter up; Battle fills in the rest.</summary>
    internal Fighter(string id, string name, Side side, FighterKind kind, int index)
    {
        Id = id;
        Name = name;
        Side = side;
        Kind = kind;
        Index = index;
    }

    // Who.

    /// <summary>Who they are: a party member's origin id, the foe's npc id or a made-up one.</summary>
    public string Id { get; }

    /// <summary>What they're called on screen.</summary>
    public string Name { get; }

    /// <summary>Which side they fight on.</summary>
    public Side Side { get; }

    /// <summary>What they are to the fight.</summary>
    public FighterKind Kind { get; }

    /// <summary>Their place in Battle.Fighters, which is also the order everything is done in each tick.</summary>
    public int Index { get; }

    /// <summary>Their class, for a party member or a companion guest; null for a stat block.</summary>
    public ClassId? ClassId { get; internal set; }

    /// <summary>Their stat block's id, for a foe, a summon or a stat-block guest; null for a party member.</summary>
    public string EnemyId => Enemy?.Id;

    /// <summary>The party member they are, or null.</summary>
    public PartyMember Member { get; internal set; }

    /// <summary>The stat block they fight from, or null.</summary>
    public EnemyDefinition Enemy { get; internal set; }

    /// <summary>Their level.</summary>
    public int Level { get; internal set; } = 1;

    /// <summary>Their rank in their class, 1 to 10 (10 for a stat block, which has every skill it has).</summary>
    public int Rank { get; internal set; } = ClassRanks.MaxRank;

    /// <summary>Their ability scores.</summary>
    public AbilityScores Scores { get; internal set; } = new();

    /// <summary>Their proficiency bonus.</summary>
    public int Proficiency { get; internal set; } = 2;

    // Where.

    /// <summary>Where their feet are, in world pixels.</summary>
    public Vector2 Position { get; internal set; }

    /// <summary>Which way they face: +1 to the right, -1 to the left.</summary>
    public int Facing { get; internal set; } = 1;

    /// <summary>How much further than their feet their body reaches, for reach and areas: a big foe's bulk.</summary>
    public float Size { get; internal set; }

    /// <summary>Where they're walking to, or null if they're standing.</summary>
    public Vector2? Goal { get; internal set; }

    /// <summary>Where they started the fight, for a foe pulled too far from home.</summary>
    public Vector2 Home { get; internal set; }

    // How hurt.

    /// <summary>Their hit points.</summary>
    public int Hp { get; internal set; }

    /// <summary>The most hit points they can have.</summary>
    public int MaxHp { get; internal set; }

    /// <summary>Their temporary hit points: every shield aura's, added up.</summary>
    public int TempHp
    {
        get
        {
            int total = 0;
            foreach (Aura aura in AuraList)
                if (!aura.Gone && aura.Definition.Shield) total += aura.Amount;
            return total;
        }
    }

    /// <summary>Their Focus, 0 to 100.</summary>
    public int Focus => FocusMilli / 1000;

    /// <summary>Their Focus in thousandths, so a second's regeneration can be paid a tick at a time.</summary>
    internal int FocusMilli { get; set; }

    /// <summary>The auras on them, in the order they went on.</summary>
    public IReadOnlyList<Aura> Auras => AuraList;

    /// <summary>Whether they're at 0 and on the ground: a Downed party member (or a knocked-out one in a brawl).</summary>
    public bool IsDown { get; internal set; }

    /// <summary>Whether they're dead (a foe) or have fallen for the rest of the fight (a party member who failed three death saves).</summary>
    public bool IsDead { get; internal set; }

    /// <summary>Whether they've left the fight standing: a guest who fell back, a foe who yielded or was knocked out in a brawl, a summon whose time was up.</summary>
    public bool HasLeft { get; internal set; }

    /// <summary>Whether they're out of it for now: down, dead or gone.</summary>
    public bool IsOut => IsDown || IsDead || HasLeft || !Present;

    /// <summary>Whether they're standing and in it.</summary>
    public bool IsStanding => !IsOut;

    /// <summary>Whether they've arrived: false for a wave or a guest that hasn't come yet.</summary>
    public bool Present { get; internal set; } = true;

    /// <summary>A Downed party member's death saves made.</summary>
    public int DeathSuccesses { get; internal set; }

    /// <summary>A Downed party member's death saves failed.</summary>
    public int DeathFailures { get; internal set; }

    /// <summary>Whether a Downed party member is stable: no more death saves, still down.</summary>
    public bool Stable { get; internal set; }

    /// <summary>How many times they've been Downed this fight.</summary>
    public int Downs { get; internal set; }

    // What they're doing.

    /// <summary>Who they're fighting (or healing), or null.</summary>
    public Fighter Target { get; internal set; }

    /// <summary>Whether their auto attack is on.</summary>
    public bool AutoAttackOn { get; internal set; }

    /// <summary>How long until their auto attack comes round; 0 when it's ready and waiting for them to be free and in reach.</summary>
    public int SwingLeftMs { get; internal set; }

    /// <summary>Their auto attack's whole interval, after haste.</summary>
    public int SwingTotalMs { get; internal set; }

    /// <summary>What they're casting or winding up, for the cast bar and the telegraph, or null.</summary>
    public CastView Casting => Action is { } a && (a.CastUntilMs > 0 || a.TelegraphUntilMs > 0) && !a.InMotion
        ? new CastView(a.Skill.Id, a.Skill.Name, a.Target?.Id, a.CastStartMs, a.CastUntilMs > 0 ? a.CastUntilMs : a.TelegraphUntilMs, a.Skill.Interruptible, a.Area, a.TelegraphUntilMs > 0)
        : null;

    /// <summary>The clip they're playing for their current move, or null.</summary>
    public string CurrentMotion { get; internal set; }

    /// <summary>When CurrentMotion started, in fight time.</summary>
    public int MotionStartMs { get; internal set; }

    /// <summary>How they fight, for their auto attack's clips and sounds.</summary>
    public WeaponKind WeaponKind { get; internal set; }

    /// <summary>The form they've taken (Wild Shape), or null.</summary>
    public FormDefinition Form { get; internal set; }

    /// <summary>When their form ends, in fight time.</summary>
    internal int FormUntilMs { get; set; }

    /// <summary>Whether they've vanished from sight (a smoke bomb): nobody can target them.</summary>
    public bool Vanished => VanishUntilMs > 0;

    /// <summary>When they step out of the smoke, or 0.</summary>
    internal int VanishUntilMs { get; set; }

    /// <summary>The boss phase they're in, 1 for the first (or for anyone without phases).</summary>
    public int Phase { get; internal set; } = 1;

    /// <summary>When the GCD ends for them, in fight time.</summary>
    public int GcdUntilMs { get; internal set; }

    /// <summary>How long their last GCD was, for the sweep.</summary>
    public int GcdTotalMs { get; internal set; }

    /// <summary>When the bonus skills' short lockout ends.</summary>
    internal int BonusUntilMs { get; set; }

    /// <summary>When the potion is ready again.</summary>
    public int PotionReadyMs { get; internal set; }

    /// <summary>The ally they're helping up, while they channel it.</summary>
    public Fighter Helping { get; internal set; }

    /// <summary>When the help channel finishes.</summary>
    internal int HelpUntilMs { get; set; }

    /// <summary>The move in progress, from its cast or wind-up to its last blow.</summary>
    internal ActionState Action { get; set; }

    /// <summary>What they've been told to do and haven't done yet: walk somewhere, use a skill once in reach, help someone up.</summary>
    internal Order Order { get; set; }

    /// <summary>Whether the next action skill they use starts no cooldown (Action Surge, Command).</summary>
    internal bool NextActionFree { get; set; }

    /// <summary>The concentration skill they're keeping up, or null.</summary>
    internal SkillDefinition Concentrating { get; set; }

    /// <summary>How many auto-attack swings they've made, for the clip's 1, 2, 3.</summary>
    internal int Swings { get; set; }

    /// <summary>When damage last made them save to keep their concentration: at most one such save a turn.</summary>
    internal int LastConcentrationSaveMs { get; set; } = -100000;

    /// <summary>The last moment anyone hit them, for a foe's reset.</summary>
    internal int LastHurtMs { get; set; }

    /// <summary>The last moment they were hit by an ally of whoever's asking, for Sneak Attack: by side.</summary>
    internal int LastHitByPartyMs { get; set; } = -100000;

    /// <summary>The last moment a foe hit them.</summary>
    internal int LastHitByFoesMs { get; set; } = -100000;

    /// <summary>The last moment their Sneak Attack landed, for its once every 3 s.</summary>
    internal int LastSneakMs { get; set; } = -100000;

    /// <summary>When the next death save is rolled.</summary>
    internal int NextDeathSaveMs { get; set; }

    /// <summary>When a Prone fighter gets up.</summary>
    internal int GetUpMs { get; set; }

    /// <summary>The next moment their AI thinks.</summary>
    internal int NextThinkMs { get; set; }

    /// <summary>A decision their AI has made and will act on after its reaction delay.</summary>
    internal BattleCommand Pending { get; set; }

    /// <summary>When Pending is acted on.</summary>
    internal int PendingAtMs { get; set; }

    /// <summary>Whether they walk after their target to keep it in reach (set by targeting a foe, cleared by walking somewhere).</summary>
    internal bool Chase { get; set; }

    /// <summary>The next moment their AI may step away from a foe in melee, so a ranged fighter doesn't spend the fight running.</summary>
    internal int KiteReadyMs { get; set; }

    /// <summary>A foe walking home to reset: it ignores everyone until it's there.</summary>
    internal bool Resetting { get; set; }

    /// <summary>The bar they had before they took on a form, to put back when it ends.</summary>
    internal List<SkillDefinition> SavedBar { get; set; }

    /// <summary>A summon's end, in fight time, or 0.</summary>
    internal int LeavesAtMs { get; set; }

    /// <summary>Who called a summon in.</summary>
    internal Fighter Summoner { get; set; }

    /// <summary>The time their Focus last ticked for the per-turn bonuses (rank 7, Arcane Recovery).</summary>
    internal int NextTurnMs { get; set; } = 3000;

    /// <summary>The stat block's skills, by id, that a phase can add to and take from.</summary>
    internal List<string> FoeSkills { get; } = new();

    // Their numbers, worked out once as they join.

    /// <summary>Their AC with everything on (before auras).</summary>
    public int BaseAc { get; internal set; }

    /// <summary>How much of their AC their Dexterity gives, for dodges.</summary>
    internal int DexAc { get; set; }

    /// <summary>How much of their AC their shield gives, for blocks.</summary>
    internal int ShieldAc { get; set; }

    /// <summary>Their main attack: the weapon in hand, or a stat block's first attack.</summary>
    internal AttackProfile MainAttack { get; set; }

    /// <summary>A stat block's attacks, in order: a Multiattack's combo.</summary>
    internal IReadOnlyList<AttackProfile> Attacks { get; set; } = Array.Empty<AttackProfile>();

    /// <summary>The ability their spells and skills use.</summary>
    internal Ability CastingAbility { get; set; } = Ability.Strength;

    /// <summary>What they add to a spell attack, before proficiency.</summary>
    internal int SpellAttackExtra { get; set; }

    /// <summary>The swing interval of their auto before haste.</summary>
    internal int BaseSwingMs { get; set; } = WeaponKinds.SwingMs;

    /// <summary>The Focus model they fill by.</summary>
    public FocusModel FocusModel { get; internal set; }

    /// <summary>The threat they make, in percent.</summary>
    internal int ThreatPct { get; set; } = 100;

    /// <summary>How fast they walk, in world pixels a second, before auras.</summary>
    internal float WalkSpeed { get; set; } = 72f;

    /// <summary>Whether they fight from range: archers and casters keep their distance.</summary>
    public bool Ranged { get; internal set; }

    /// <summary>Whether they're one of the party's healers, to the AI.</summary>
    internal bool Healer { get; set; }

    /// <summary>Whether they're a defender, to the AI: they taunt and stand in front.</summary>
    internal bool Defender { get; set; }

    // What they've done, for the simulator.

    /// <summary>Damage they've dealt this fight.</summary>
    public int DamageDone { get; internal set; }

    /// <summary>Healing they've done this fight (to others and themselves, what landed).</summary>
    public int HealingDone { get; internal set; }

    /// <summary>Damage they've taken this fight.</summary>
    public int DamageTaken { get; internal set; }

    /// <summary>How long they've been the top of some foe's threat, in milliseconds.</summary>
    public int ThreatHeldMs { get; internal set; }

    /// <summary>Every skill they've used this fight, by id, and how often.</summary>
    public Dictionary<string, int> Uses { get; } = new(StringComparer.Ordinal);

    /// <summary>Their bar's skills in order: the auto first, then the skills and the ultimate they have the rank for (or a form's).</summary>
    public IReadOnlyList<SkillDefinition> Skills => BarSkills;

    /// <summary>Their passives they have the rank for.</summary>
    public IReadOnlyList<SkillDefinition> Passives => PassiveSkills;

    /// <summary>Their auto attack.</summary>
    public SkillDefinition Auto { get; internal set; }

    /// <summary>A foe's threat table, highest first: who it means to hit, and how much they've earned it.</summary>
    public IReadOnlyList<ThreatEntry> Threat => ThreatTable;

    /// <summary>The share of their hit points left, 0 to 1.</summary>
    public double HpShare => MaxHp <= 0 ? 0 : Hp / (double)MaxHp;

    /// <inheritdoc/>
    public override string ToString() => $"{Name} ({Id})";
}

/// <summary>What a fighter is casting or winding up, as the cast bar and the ground marker show it.</summary>
/// <param name="SkillId">The skill.</param>
/// <param name="Name">Its name, for the bar.</param>
/// <param name="TargetId">At whom, or null.</param>
/// <param name="StartMs">When it started.</param>
/// <param name="ImpactMs">When the cast or the wind-up finishes and the move begins.</param>
/// <param name="Interruptible">Whether it can be stopped.</param>
/// <param name="Area">Its ground marker, or null.</param>
/// <param name="Telegraph">Whether it's a foe's telegraph rather than a cast.</param>
public sealed record CastView(string SkillId, string Name, string TargetId, int StartMs, int ImpactMs, bool Interruptible, BattleArea Area, bool Telegraph);

/// <summary>One line of a foe's threat table.</summary>
public sealed class ThreatEntry
{
    /// <summary>Puts someone on the table.</summary>
    internal ThreatEntry(Fighter who) => Who = who;

    /// <summary>Who.</summary>
    public Fighter Who { get; }

    /// <summary>How much threat they've made on this foe.</summary>
    public double Value { get; internal set; }
}

/// <summary>A move in progress, from its cast or wind-up to its last blow.</summary>
internal sealed class ActionState
{
    public SkillDefinition Skill;
    public Fighter Target;
    public Vector2 Aim;
    public BattleArea Area;
    public bool Auto;
    public bool SoulLink;
    public int CastStartMs;
    public int CastUntilMs;
    public int TelegraphUntilMs;
    public bool InMotion;
    public string Motion;
    public int MotionStartMs;
    public List<int> BlowsAt = new();
    public int NextBlow;
    public int Strikes;
    public bool Partner;
    public int Serial;
    public bool Echo;
}

/// <summary>A reaction a fighter has armed, waiting for its trigger.</summary>
internal sealed class Armed
{
    public SkillDefinition Skill;
    public Fighter Over;
    public int UntilMs;
}

/// <summary>Effects waiting to ride on a fighter's next hit.</summary>
internal sealed class Rider
{
    public SkillDefinition Skill;
    public IReadOnlyList<Effect> Effects;
    public int UntilMs;
}

/// <summary>What a fighter has been told to do.</summary>
internal sealed class Order
{
    public CommandKind Kind;
    public SkillDefinition Skill;
    public Fighter Target;
    public Vector2 Point;
}
