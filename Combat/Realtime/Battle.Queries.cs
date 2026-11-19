using System;
using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Combat.Realtime;

/// <summary>What the HUD asks a fight: a fighter's bar (slots, cooldowns, the GCD, costs, why not), threat, a boss's phases.</summary>
/// <remarks>None of these change the fight; they're safe to call every frame.</remarks>
public sealed partial class Battle
{
    /// <summary>The slot keys: 1 the auto, 2 to 4 the skills, 5 the ultimate, 6 the potion, 7 the Soul-Link.</summary>
    public const int PotionKey = 6, SoulLinkKey = 7;

    /// <summary>A fighter's bar as the hotbar shows it (11-combat.md §2.4): keys 1 to 5 their class's auto, skills and ultimate (or their form's), 6 the potion, and 7 the Soul-Link for the duo.</summary>
    /// <param name="f">Whose.</param>
    public IReadOnlyList<BarSlot> Bar(Fighter f)
    {
        var slots = new List<BarSlot>();
        Fighter target = f.Target;
        if (f.Form is not null || f.ClassId is null)
        {
            for (int i = 0; i < f.BarSkills.Count; i++) slots.Add(Slot(f, i + 1, f.BarSkills[i], 1, target));
        }
        else
        {
            ClassKit kit = SkillCatalog.Kit(f.ClassId.Value);
            int[] ranks = { 1, 1, ClassRanks.Skill2Rank, ClassRanks.Skill3Rank, ClassRanks.UltimateRank };
            IReadOnlyList<SkillDefinition> bar = kit.Bar;
            for (int i = 0; i < bar.Count; i++) slots.Add(Slot(f, i + 1, bar[i], ranks[i], target));
        }
        if (f.Kind == FighterKind.Member) slots.Add(Slot(f, PotionKey, Potion, 1, f));
        if (f.Id is Origins.You or Origins.SoulBound && f.Kind == FighterKind.Member)
        {
            string why = SoulLinkProblem(f, target);
            slots.Add(new BarSlot(SoulLinkKey, SkillCatalog.Get(SkillCatalog.SoulLinkStrike), SoulLinkActive, 1, 0, 0, 0, 0, 0, true,
                target is not null && InReach(f, SkillCatalog.Get(SkillCatalog.SoulLinkStrike), target), why is null, why, _soulCharges));
        }
        return slots;
    }

    /// <summary>One slot of a bar.</summary>
    private BarSlot Slot(Fighter f, int key, SkillDefinition skill, int unlock, Fighter target)
    {
        bool unlocked = f.Rank >= unlock && (f.BarSkills.Contains(skill) || skill == Potion);
        int left = CooldownLeft(f, skill);
        int total = skill == Potion ? PotionCooldownMs : f.CooldownTotal.TryGetValue(skill.Id, out int was) ? was : CooldownLength(f, skill);
        int gcdLeft = skill.OnGcd ? Math.Max(0, f.GcdUntilMs - NowMs) : 0;
        Fighter aimed = target is not null && (skill.Target != SkillTarget.Foe || Hostile(f, target)) ? target : DefaultTarget(f, skill);
        string why = unlocked ? Problem(f, skill, aimed) : $"Rank {unlock}";
        int cost = FocusCost(f, skill);
        return new BarSlot(key, skill, unlocked, unlock, left, total, gcdLeft, f.GcdTotalMs, cost, cost <= f.Focus, aimed is null || InReach(f, skill, aimed),
            unlocked && why is null, why, 0);
    }

    /// <summary>Why a fighter can't use a skill on a target now (cooldown, the GCD, Focus, no target...), or null if they can. A target out of reach isn't a reason: using it walks them into reach.</summary>
    /// <param name="f">Who.</param>
    /// <param name="skill">What.</param>
    /// <param name="target">At whom, or null for the skill's own choice.</param>
    public string WhyNot(Fighter f, SkillDefinition skill, Fighter target = null) => Problem(f, skill, target);

    /// <summary>A foe's threat table, highest first.</summary>
    /// <param name="foe">The foe.</param>
    public IReadOnlyList<ThreatEntry> Threat(Fighter foe) => foe.Threat;

    /// <summary>A boss's phases, for its frame's pips: the one it's in, how many it has, and the name of the one it's in.</summary>
    /// <param name="boss">The boss.</param>
    public PhaseView Phase(Fighter boss)
    {
        IReadOnlyList<FoePhase> phases = boss.Enemy?.Phases ?? Array.Empty<FoePhase>();
        string name = boss.Phase >= 2 && boss.Phase - 2 < phases.Count ? phases[boss.Phase - 2].Name : null;
        var shares = new List<double> { 1.0 };
        foreach (FoePhase phase in phases) shares.Add(phase.AtShare);
        return new PhaseView(boss.Phase, phases.Count + 1, name, shares);
    }

    /// <summary>Whether a foe gets the wide boss frame: it has phases, or it's the one foe of a boss fight.</summary>
    /// <param name="f">The foe.</param>
    public bool IsBoss(Fighter f) => f.Side == Side.Foes && (f.Enemy?.Phases.Count > 0 || Encounter.Boss && f.Kind == FighterKind.Foe && f.Enemy?.Big == true);

    /// <summary>The time a fighter has left on an aura, for its timer.</summary>
    /// <param name="aura">The aura.</param>
    public int Remaining(Aura aura) => aura.RemainingMs(NowMs);

    /// <summary>How much more gold the fight's loot gives, in percent, from the party's passives (Captain's Share), for the result panel.</summary>
    public int GoldBonusPct
    {
        get
        {
            int total = 0;
            foreach (Fighter f in _fighters)
                if (f.Side == Side.Party && f.Kind == FighterKind.Member) total += (int)Sum(f, Stat.GoldPct, default);
            return total;
        }
    }

    /// <summary>The gold the party took from foes in the fight (Pick Pocket, Everyone's Pocket), as shares of each foe's loot in percent, by foe id, for the result panel: the Stole events, summed.</summary>
    public IReadOnlyDictionary<string, int> Stolen => _stolen;
}

/// <summary>One slot of the hotbar.</summary>
/// <param name="Key">Its key, 1 to 7.</param>
/// <param name="Skill">The skill in it.</param>
/// <param name="Unlocked">Whether the class's rank has opened it.</param>
/// <param name="UnlockRank">The rank it opens at.</param>
/// <param name="CooldownLeftMs">Its own cooldown left.</param>
/// <param name="CooldownTotalMs">Its cooldown's whole length, for the sweep.</param>
/// <param name="GcdLeftMs">The GCD left over it (0 for a skill off the GCD).</param>
/// <param name="GcdTotalMs">The last GCD's length.</param>
/// <param name="FocusCost">What it costs now.</param>
/// <param name="Affordable">Whether they have the Focus.</param>
/// <param name="InReach">Whether its target is in reach (a red tint if not).</param>
/// <param name="Usable">Whether pressing it now would use it (or walk into reach to).</param>
/// <param name="WhyNot">Why not, or null.</param>
/// <param name="Charges">The Soul-Link's charges, for slot 7.</param>
public sealed record BarSlot(int Key, SkillDefinition Skill, bool Unlocked, int UnlockRank, int CooldownLeftMs, int CooldownTotalMs, int GcdLeftMs, int GcdTotalMs,
    int FocusCost, bool Affordable, bool InReach, bool Usable, string WhyNot, int Charges);

/// <summary>A boss's phases as its frame shows them.</summary>
/// <param name="Current">The phase it's in, from 1.</param>
/// <param name="Count">How many it has.</param>
/// <param name="Name">The name of the one it's in, or null for the first.</param>
/// <param name="Shares">The share of its hit points each phase begins at, the first (1.0) included: where the pips go on its bar.</param>
public sealed record PhaseView(int Current, int Count, string Name, IReadOnlyList<double> Shares);
