using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Story;

namespace LegendQuest.Combat;

/// <summary>
/// Who stands where when a fight starts: the party's six slots, the guests who come, who sits it
/// out, and the foes in their slots. The explore screen and the simulator both line up from here.
/// </summary>
/// <remarks>
/// 11-combat.md §4. The party's side has six slots, three to a row. Whoever the fight's SitsOut names
/// sits out first; then, if the party and the guests who come are more than six, the companion with
/// the lowest affinity sits out, and the next lowest, until they fit. You and the Soul-Bound never sit
/// out. Anyone whose auto attack is melee stands in the front row and everyone else at the back, a
/// guest in the row they'd rather; when a row's full, they go in the other. Each row fills from its
/// middle lane, then the lane nearer the foes, then the far one, in party order, then the guests.
/// </remarks>
public static class FightRoster
{
    /// <summary>How many slots the party's side has.</summary>
    public const int PartySlots = 2 * Slot.PartyLanes;

    /// <summary>The order a row's lanes fill in: the middle, the one nearer the foes (the party faces right), the far one.</summary>
    private static readonly int[] LaneOrder = { 1, 2, 0 };

    /// <summary>Lines up the party's side of a fight.</summary>
    /// <param name="encounter">The fight.</param>
    /// <param name="party">The party, in order, you first.</param>
    /// <param name="flag">Whether a story flag is set, for a guest who only comes on one.</param>
    /// <param name="meter">A meter's value, for who sits out: a companion's is affinity-(their id). A companion with no such meter counts as 0.</param>
    public static FightLineup Build(EncounterDefinition encounter, IReadOnlyList<PartyMember> party, Func<string, bool> flag, Func<string, int> meter)
    {
        var fighting = new List<PartyMember>();
        var sittingOut = new List<PartyMember>();
        foreach (PartyMember member in party)
        {
            bool sits = encounter.SitsOut.Contains(member.Id) && member.Id is not (Origins.You or Origins.SoulBound);
            (sits ? sittingOut : fighting).Add(member);
        }

        var guests = new List<EncounterGuest>();
        foreach (EncounterGuest guest in encounter.Guests)
            if ((guest.When is null || flag(guest.When)) && !party.Any(member => member.Id == guest.Id)) guests.Add(guest);

        int over = fighting.Count + guests.Count - PartySlots;
        if (over > 0)
        {
            List<PartyMember> leastKeen = fighting
                .Where(member => member.Id is not (Origins.You or Origins.SoulBound))
                .OrderBy(member => Affinity(member, meter))
                .ThenByDescending(fighting.IndexOf)
                .Take(over)
                .ToList();
            fighting.RemoveAll(leastKeen.Contains);
            sittingOut.AddRange(leastKeen);
            sittingOut.Sort((a, b) => IndexIn(party, a).CompareTo(IndexIn(party, b)));
            int still = fighting.Count + guests.Count - PartySlots;
            if (still > 0) guests.RemoveRange(guests.Count - still, still);
        }

        var taken = new HashSet<Slot>();
        var setups = new List<CombatantSetup>();
        foreach (PartyMember member in fighting)
            setups.Add(CombatantSetup.ForMember(member, Place(taken, MeleeAuto(member) ? Row.Front : Row.Back)));
        foreach (EncounterGuest guest in guests)
        {
            CombatantSetup setup = CombatantSetup.ForGuest(guest);
            setups.Add(guest.FromRound <= 1 ? setup with { Slot = Place(taken, guest.Row) } : setup);
        }
        return new FightLineup(setups, sittingOut);
    }

    /// <summary>The foes a fight starts with, each in its slot, by the ids the fight knows them by.</summary>
    /// <param name="encounter">The fight.</param>
    public static IReadOnlyList<CombatantSetup> Foes(EncounterDefinition encounter)
    {
        var setups = new List<CombatantSetup>();
        for (int i = 0; i < encounter.Foes.Count; i++)
        {
            EncounterFoe foe = encounter.Foes[i];
            setups.Add(CombatantSetup.ForFoe(foe, FoeId(foe, i)));
        }
        return setups;
    }

    /// <summary>The id a fight knows one of its starting foes by: the map npc's, or for one with none, its stat block's and its place in the list from 1 (hollowed-footman-2).</summary>
    /// <param name="foe">The foe.</param>
    /// <param name="index">Its place in the encounter's Foes, from 0.</param>
    public static string FoeId(EncounterFoe foe, int index) => foe.Npc ?? $"{foe.Enemy}-{index + 1}";

    /// <summary>Whether someone's auto attack is melee, which puts them in the front row.</summary>
    /// <remarks>
    /// Their auto is the first thing in their class's kit that's on the bar's auto button and does
    /// damage (a caster's cantrip), or else Attack, with the weapon in their main hand. A reach that's
    /// the weapon's is the weapon's own: ranged or reach if it has the property, melee if not (and
    /// with nothing in the hand); a spell's is ranged.
    /// </remarks>
    /// <param name="member">Who.</param>
    public static bool MeleeAuto(PartyMember member)
    {
        AbilityDefinition auto = AbilityCatalog.Get(AbilityId.Attack);
        foreach (AbilityId id in AbilityCatalog.Kit(member.Class))
        {
            AbilityDefinition ability = AbilityCatalog.Get(id);
            if (ability.Bar != BarSlot.Auto || ability.Damage.IsNone) continue;
            auto = ability;
            break;
        }

        Reach reach = auto.Reach;
        if (reach == Reach.Weapon) reach = auto.Kind == AbilityKind.Weapon ? WeaponReach(member.WornIn(EquipmentSlot.MainHand)) : Reach.Ranged;
        return reach == Reach.Melee;
    }

    /// <summary>The reach a weapon gives an attack: ranged or reach by its properties, melee for any other weapon and for a bare fist.</summary>
    private static Reach WeaponReach(ItemDefinition weapon) =>
        weapon is null ? Reach.Melee
        : weapon.Has(WeaponProperty.Ranged) ? Reach.Ranged
        : weapon.Has(WeaponProperty.Reach) ? Reach.Reach
        : Reach.Melee;

    /// <summary>A companion's affinity, or 0 if they haven't a meter for it.</summary>
    private static int Affinity(PartyMember member, Func<string, int> meter)
    {
        string id = "affinity-" + member.Id;
        return Meters.Exists(id) ? meter(id) : 0;
    }

    /// <summary>Where someone is in the party, by reference.</summary>
    private static int IndexIn(IReadOnlyList<PartyMember> party, PartyMember member)
    {
        for (int i = 0; i < party.Count; i++)
            if (ReferenceEquals(party[i], member)) return i;
        return party.Count;
    }

    /// <summary>The first free slot on the party's side in the row someone would rather, by LaneOrder, or in the other row if that one's full; null if both are.</summary>
    private static Slot? Place(HashSet<Slot> taken, Row preferred)
    {
        foreach (Row row in new[] { preferred, preferred == Row.Front ? Row.Back : Row.Front })
        {
            foreach (int lane in LaneOrder)
            {
                var slot = new Slot(Side.Party, row, lane);
                if (taken.Add(slot)) return slot;
            }
        }
        return null;
    }
}
