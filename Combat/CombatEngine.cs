using System;
using System.Collections.Generic;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// A turn-based fight in rows, by 5e's rules: initiative, turns and their budgets, where everyone
/// stands, attacks, spells, conditions, Focus, going down and getting up, allies, and who wins.
/// </summary>
/// <remarks>
/// The Black Box's RoundEngine pattern, grown up: it draws nothing, takes its Random, and every
/// command hands back what it caused as CombatEvents, in order, for the screen to play out at its
/// own pace. Nothing in here waits on time, so the simulator can fight a thousand of these headless
/// and get the same answer twice from the same seed. The rules it follows are 11-combat.md's, spec
/// 6.7's and the SRD 5.1's; where they differ (Help gets a Downed ally up, 6.7's way) the spec wins.
/// The parts are split by concern: turns, the formation, abilities, attacks, health, odds and allies.
/// </remarks>
public sealed partial class CombatEngine
{
    /// <summary>The Focus a foe starts a fight on, whatever the difficulty.</summary>
    internal const int FoeStartFocus = 40;

    /// <summary>The story flag that turns the Soul-Link off.</summary>
    private const string BondBroken = "soul-bond-broken";

    private readonly EncounterDefinition _encounter;
    private readonly Random _random;

    /// <summary>The difficulty, the flags and the kits.</summary>
    private readonly FightRules _rules;

    private readonly List<Combatant> _combatants = new();

    /// <summary>Guests and others due in a later round, waiting for it.</summary>
    private readonly List<CombatantSetup> _waiting = new();

    /// <summary>Foes who turned up to a full side, waiting for a slot to come free.</summary>
    private readonly List<CombatantSetup> _queue = new();

    /// <summary>For each foe or ally hit this round, by id, who hit them, for Sneak Attack's "hit by an ally this round".</summary>
    private readonly Dictionary<string, List<string>> _hitThisRound = new();

    /// <summary>Everyone who's rolled initiative, highest first, the dead included so the turn index stays put.</summary>
    private readonly List<Combatant> _order = new();

    /// <summary>The same without the dead, which is what the initiative bar shows.</summary>
    private readonly List<Combatant> _living = new();

    /// <summary>What the command being run has caused so far.</summary>
    private readonly List<CombatEvent> _events = new();

    /// <summary>The foes who've given up, so it's only said once.</summary>
    private readonly HashSet<string> _yielded = new();

    /// <summary>Where the turn is in _order, or -1 between rounds.</summary>
    private int _turn = -1;

    /// <summary>How many of the encounter's waves have arrived.</summary>
    private int _wave;

    /// <summary>Counts up on every hit and every turn's end, so the Soul-Link can tell what happened since when.</summary>
    private int _clock;

    /// <summary>Counts every turn started, so once-a-turn things (Sneak Attack) know which turn they were used on.</summary>
    private int _turnNumber;

    /// <summary>Whether Begin has been called.</summary>
    private bool _begun;

    /// <summary>Sets a fight up: everyone in their slots, nobody's rolled yet. Begin starts it.</summary>
    /// <remarks>
    /// Anyone whose slot is taken, or who hasn't one, goes in the nearest free one in the row they'd
    /// rather; a foe with no room left waits for some, and anyone due in a later round waits for it.
    /// The difficulty goes on the foes here (their hit points and rolls) and on everyone's starting
    /// Focus. A party member already at 0 hit points starts Downed and stable. The supports whose
    /// story flag holds come; the rest stay away for the whole fight.
    /// </remarks>
    /// <param name="encounter">The fight.</param>
    /// <param name="combatants">Who's in it, and where.</param>
    /// <param name="random">Where every roll comes from.</param>
    /// <param name="rules">The difficulty, the story's flags, and where the kits come from.</param>
    public CombatEngine(EncounterDefinition encounter, IReadOnlyList<CombatantSetup> combatants, Random random, FightRules rules)
    {
        _encounter = encounter;
        _random = random;
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        Formation = new Formation(Standing);

        foreach (CombatantSetup setup in combatants)
        {
            if (setup.FromRound > 1)
            {
                _waiting.Add(setup);
                continue;
            }
            if (JoinRows(setup) is not Combatant joined || joined.Member is null || joined.Hp > 0) continue;
            joined.Add(new ActiveCondition(ConditionId.Downed, 0, null, null, 0, DiceExpression.None));
            joined.Add(new ActiveCondition(ConditionId.Stable, 0, null, null, 0, DiceExpression.None));
        }

        bool duo = Find(Origins.You) is { IsDuo: true } && Find(Origins.SoulBound) is { IsDuo: true };
        SoulLink = new SoulLink(duo && !Flag(BondBroken));
        foreach (EncounterSupport support in _encounter.Supports)
            if (support.When is null || Flag(support.When)) _supports.Add(new SupportState(support));
    }

    /// <summary>Who stands where.</summary>
    public Formation Formation { get; }

    /// <summary>The party's shared bag, for potions and alchemist's fire, or null if there's nothing to draw on.</summary>
    public Inventory Bag { get; init; }

    /// <summary>The fight being fought.</summary>
    public EncounterDefinition Encounter => _encounter;

    /// <summary>Everyone who's been in the fight, in the order they joined.</summary>
    public IReadOnlyList<Combatant> Combatants => _combatants;

    /// <summary>Everyone still in it, in initiative order: the initiative bar.</summary>
    public IReadOnlyList<Combatant> Order => _living;

    /// <summary>Whose turn it is, or null before Begin.</summary>
    public Combatant Current => _turn >= 0 && _turn < _order.Count ? _order[_turn] : null;

    /// <summary>The round, from 1.</summary>
    public int Round { get; private set; }

    /// <summary>How the fight stands.</summary>
    public CombatOutcome Outcome { get; private set; }

    /// <summary>The duo's meter.</summary>
    public SoulLink SoulLink { get; }

    /// <summary>Someone in the fight by id, or null.</summary>
    /// <param name="id">Who.</param>
    internal Combatant Find(string id)
    {
        foreach (Combatant each in _combatants)
            if (each.Id == id) return each;
        return null;
    }

    /// <summary>Whether two people are on opposite sides.</summary>
    internal static bool Hostile(Combatant a, Combatant b) => a.Side != b.Side;

    /// <summary>Whether a foe's beaten: down, dead, or hurt past the point where it gives up.</summary>
    /// <param name="who">Who. Party members are never beaten this way; they're down or they aren't.</param>
    internal bool Beaten(Combatant who) =>
        who.IsOut || who.Side == Side.Foes && _encounter.YieldsAt > 0f && who.Hp <= _encounter.YieldsAt * who.MaxHp;

    /// <summary>The other half of the duo, or null.</summary>
    /// <param name="who">You or the Soul-Bound.</param>
    internal Combatant Partner(Combatant who) =>
        who.IsDuo ? Find(who.Id == Origins.You ? Origins.SoulBound : Origins.You) : null;

    /// <summary>Whether someone's standing: not out, not beaten, not given up.</summary>
    internal bool Standing(Combatant who) => !Beaten(who);

    /// <summary>Whether a story flag is set, by the fight's rules.</summary>
    private bool Flag(string flag) => _rules.Flag?.Invoke(flag) ?? false;

    /// <summary>The first slot someone's in, or null if they're off the field.</summary>
    private Slot? SlotOf(Combatant who)
    {
        if (who is null) return null;
        IReadOnlyList<Slot> slots = Formation.SlotsOf(who);
        return slots.Count > 0 ? slots[0] : null;
    }

    /// <summary>Puts someone in the fight: on the field, with the difficulty, their Focus and their pools. A foe with no room left waits in the queue instead.</summary>
    /// <remarks>Throws if the party's side is full: FightRoster sits people out so it never is.</remarks>
    /// <returns>Who joined, or null if they're waiting for room.</returns>
    private Combatant JoinRows(CombatantSetup setup)
    {
        var joined = new Combatant(setup, _combatants.Count);
        joined.Kit = _rules.Kits.Kit(joined);
        Row preferred = setup.Slot?.Row ?? setup.Row ?? KitRow(joined);
        if (!Formation.Place(joined, setup.Slot, preferred))
        {
            if (joined.Side == Side.Party) throw new InvalidOperationException($"There's no room for {joined.Name} on the party's side: it has {2 * Slot.PartyLanes} slots.");
            _queue.Add(setup);
            return null;
        }

        if (joined.Side == Side.Foes)
        {
            joined.ScaleHp(DifficultyRules.FoeHp(_rules.Difficulty));
            joined.RollBonus = DifficultyRules.FoeRolls(_rules.Difficulty);
            joined.ChangeFocus(FoeStartFocus);
        }
        else joined.ChangeFocus(DifficultyRules.StartFocus(_rules.Difficulty));

        foreach (AbilityId id in joined.Kit)
        {
            int perLevel = AbilityCatalog.Get(id).PoolPerLevel;
            if (perLevel > 0) joined.Pool[id] = perLevel * joined.Level;
        }

        _combatants.Add(joined);
        return joined;
    }

    /// <summary>The row someone stands in when nothing says which: a stat block's by whether it fights from range, a party member's by their auto attack (melee or reach in front, the rest at the back).</summary>
    private Row KitRow(Combatant who)
    {
        if (who.Member is null) return who.Enemy.Ranged ? Row.Back : Row.Front;
        foreach (AbilityId id in who.Kit)
        {
            AbilityDefinition ability = AbilityCatalog.Get(id);
            if (ability.Bar == BarSlot.Auto) return ReachOf(who, ability) is Reach.Melee or Reach.Reach ? Row.Front : Row.Back;
        }
        return Row.Front;
    }

    /// <summary>Rebuilds the initiative bar without the dead and the guests who've fallen back.</summary>
    private void RefreshOrder()
    {
        _living.Clear();
        foreach (Combatant each in _order)
            if (!each.IsDead && !each.FellBack) _living.Add(each);
    }

    /// <summary>Notes something that happened, for the command's caller: from the actor's slot to the target's.</summary>
    private void Emit(CombatEventKind kind, Combatant actor, Combatant target, int amount = 0, string text = null, RollResult roll = null,
        AbilityId? ability = null, DamageType? damageType = null) =>
        _events.Add(new CombatEvent(kind, actor?.Id, target?.Id, amount, text, roll, SlotOf(actor ?? target), SlotOf(target ?? actor), ability, damageType));

    /// <summary>Hands back everything the command caused, and starts the list again.</summary>
    private IReadOnlyList<CombatEvent> Flush()
    {
        CombatEvent[] happened = _events.ToArray();
        _events.Clear();
        return happened;
    }
}
