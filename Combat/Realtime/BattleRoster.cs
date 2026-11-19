using System;
using System.Collections.Generic;
using LegendQuest.Characters;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// Who starts a real-time fight: the party (less whoever the encounter sits out), its guests (those
/// whose flag is set, at the level the story has them at), and the foes the encounter starts with.
/// </summary>
/// <remarks>
/// The waves, the supports and the guests who come later are the encounter's script, which Battle runs
/// itself. The screen passes where everyone stands (CombatantSetup.At) when it has the room; without it
/// the arena places them (BattleArena).
/// </remarks>
public static class BattleRoster
{
    /// <summary>The setups for a fight, in the order Battle takes them: the party, the guests, the foes.</summary>
    /// <param name="encounter">The fight.</param>
    /// <param name="party">The party, leader first.</param>
    /// <param name="flag">Whether a story flag is set, or null for none.</param>
    public static List<CombatantSetup> Build(EncounterDefinition encounter, IReadOnlyList<PartyMember> party, Func<string, bool> flag = null)
    {
        var setups = new List<CombatantSetup>();
        var here = new HashSet<string>(StringComparer.Ordinal);
        foreach (PartyMember member in party)
        {
            if (member is null || Contains(encounter.SitsOut, member.Id) || !here.Add(member.Id)) continue;
            setups.Add(CombatantSetup.ForMember(member));
        }

        int level = LevelTable.Expected(encounter);
        foreach (EncounterGuest guest in encounter.Guests)
        {
            if (guest.When is not null && (flag is null || !flag(guest.When))) continue;
            if (!here.Add(guest.Id)) continue;
            CombatantSetup setup = CombatantSetup.ForGuest(guest);
            if (setup.Member is PartyMember companion)
            {
                Leveling.SetLevel(companion, Math.Max(companion.Level, level));
                companion.Hp = companion.MaxHp;
            }
            setups.Add(setup);
        }

        setups.AddRange(FightRoster.Foes(encounter));
        return setups;
    }

    /// <summary>Whether a list of ids has one.</summary>
    private static bool Contains(IReadOnlyList<string> list, string id)
    {
        foreach (string each in list)
            if (each == id) return true;
        return false;
    }
}
