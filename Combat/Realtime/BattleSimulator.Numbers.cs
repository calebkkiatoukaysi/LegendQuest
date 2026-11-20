using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// `--simulate numbers`: every class's real-time numbers, read straight out of the catalog and
/// printed as Markdown for `10-classes.md` §8 (its Focus model, the auto's swing, and each skill's
/// kind, rank, Focus, cooldown, cast or wind-up, when its blows land, reach, area and what it does).
/// </summary>
/// <remarks>
/// Nothing here is typed twice: change a row in SkillCatalog and run this again, and the table in the
/// story file follows. When the blows land is read from the clips --simulate loaded (MotionTimings), so
/// it's the anim stream's clip times once they're in, and the fallbacks before.
/// </remarks>
public static partial class BattleSimulator
{
    /// <summary>What --simulate takes for the catalog's numbers as Markdown.</summary>
    public const string Numbers = "numbers";

    /// <summary>Prints every class's table.</summary>
    private static bool RunNumbers()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine($"<!-- Written by `--simulate numbers` from SkillCatalog; clip times from {(MotionTimings.LoadedFrom is null ? "the fallbacks" : "clips.json")}. -->");
        Console.WriteLine();
        Console.WriteLine("| Focus model | Fills Focus (everyone also gets 1 a second) |");
        Console.WriteLine("| --- | --- |");
        foreach (FocusModel model in Enum.GetValues<FocusModel>())
            Console.WriteLine($"| {model} | {FocusText(FocusModels.Get(model))} |");

        foreach (ClassDefinition cls in ClassCatalog.All.Where(c => SkillCatalog.HasKit(c.Id)))
        {
            ClassKit kit = SkillCatalog.Kit(cls.Id);
            ItemDefinition weapon = cls.StartingKit.Where(ItemCatalog.Exists).Select(ItemCatalog.Get)
                .FirstOrDefault(item => item.Kind == ItemKind.Weapon && item.Slot == EquipmentSlot.MainHand);
            WeaponKind kind = WeaponKinds.Of(weapon);
            Console.WriteLine();
            Console.WriteLine($"#### {cls.Name} (`{cls.Code}`)");
            Console.WriteLine();
            Console.WriteLine($"{cls.Focus}: {FocusText(FocusModels.Get(cls.Focus))}{(cls.Focus == FocusModel.Tempo ? ", and starts every fight on 80" : "")}. "
                + $"The auto swings every {Seconds(WeaponKinds.SwingOf(weapon))} with {(weapon is null ? "bare hands" : "a " + weapon.Name.ToLowerInvariant())} ({WeaponKinds.Clip(kind)}), before haste."
                + (cls.ThreatPct != 100 ? $" Threat ×{cls.ThreatPct / 100.0:0.#}." : ""));
            Console.WriteLine();
            Console.WriteLine("| Skill | Kind | Rank | Focus | Cooldown | Cast / wind-up | Blows land | Reach | Area | Does |");
            Console.WriteLine("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (SkillDefinition skill in kit.All) Console.WriteLine(Row(skill, kind));
        }
        return true;
    }

    /// <summary>One skill's row.</summary>
    private static string Row(SkillDefinition skill, WeaponKind kind)
    {
        if (!skill.Active) return $"| {skill.Name} | passive | {skill.Rank} | | | | | | | {skill.Description} |";
        string kindText = skill.Kind switch
        {
            SkillKind.Auto => "auto",
            SkillKind.Bonus => "B",
            SkillKind.Reaction => "R",
            _ => "A",
        } + (skill.Ultimate ? ", ultimate" : "");
        string cooldown = skill.CooldownMs > 0 ? Seconds(skill.CooldownMs) : skill.OnGcd ? "GCD" : "";
        string windup = skill.CastMs > 0 ? "cast " + Seconds(skill.CastMs) : skill.TelegraphMs > 0 ? Seconds(skill.TelegraphMs) : "";
        string clip = skill.MotionFor(kind, 0);
        MotionTiming timing = MotionTimings.Of(clip, skill.MotionKind);
        IReadOnlyList<int> moments = timing.Moments;
        string blows = moments.Count == 0 ? "" : string.Join(", ", moments.Take(4).Select(ms => (ms / 1000.0).ToString("0.00", CultureInfo.InvariantCulture))) + " s"
            + (skill.StrikesAt(1) > 1 ? $" ({skill.StrikesAt(1)} blows)" : "");
        string reach = skill.Range switch
        {
            SkillRange.Weapon => "weapon",
            SkillRange.Melee => "30",
            SkillRange.Reach => "42",
            SkillRange.Short => "100",
            SkillRange.Long => "220",
            SkillRange.Room => "room",
            _ => "self",
        };
        string area = skill.Area switch
        {
            AreaShape.Single => "",
            _ => skill.Area + (skill.AreaPx > 0 ? $" {skill.AreaPx:0} px" : "") + (skill.Count > 0 ? $" ×{skill.Count}" : ""),
        };
        return $"| {skill.Name} | {kindText} | {skill.Rank} | {(skill.Focus > 0 ? skill.Focus.ToString(CultureInfo.InvariantCulture) : "")}{(skill.HpCost > 0 ? $" + {skill.HpCost} HP" : "")} | {cooldown} | {windup} | {blows} | {reach} | {area} | {Does(skill.Effects)} |";
    }

    /// <summary>What a skill's effects do, in a few words each.</summary>
    private static string Does(IReadOnlyList<Effect> effects)
    {
        var parts = new List<string>();
        foreach (Effect e in effects)
        {
            string text = e.Kind switch
            {
                EffectType.Damage => Amount(e) + (e.Type is DamageType type ? " " + type.ToString().ToLowerInvariant() : ""),
                EffectType.Heal => "heal " + Amount(e),
                EffectType.TempHp => "shield " + Amount(e),
                EffectType.Aura when e.Aura is not null => AuraText(e.Aura),
                EffectType.Knockback or EffectType.Pull or EffectType.Dash or EffectType.Teleport => e.Kind.ToString().ToLowerInvariant() + (e.Distance > 0 ? $" {e.Distance:0} px" : ""),
                EffectType.Focus => $"{(e.Amount >= 0 ? "+" : "")}{e.Amount} Focus",
                EffectType.Arm => null,
                _ => e.Kind.ToString().ToLowerInvariant(),
            };
            if (text is null) continue;
            if (e.Delivery == Delivery.Save && e.Save is Ability save) text += $" ({save.ToString()[..3]} save{(e.Half ? ", half" : "")})";
            if (e.On is EffectOn.Self && e.Kind is not (EffectType.Dash or EffectType.Teleport or EffectType.Heal)) text += " on you";
            if (e.Once && effects.Count > 1 && e.Kind == EffectType.Damage) text += " once";
            parts.Add(text);
        }
        return string.Join("; ", parts.Distinct());
    }

    /// <summary>An effect's amount: the weapon's dice or its own, and what's added.</summary>
    private static string Amount(Effect e)
    {
        string text = e.Weapon ? (e.WeaponTimes > 1 ? $"weapon ×{e.WeaponTimes}" : "weapon") : e.Dice.IsNone ? "" : e.Dice + (e.Plus ? "⁺" : "");
        if (e.AddCasting) text += " + casting";
        if (e.AddAbility is Ability ability) text += " + " + ability.ToString()[..3];
        if (e.LevelTimes > 0) text += (text.Length > 0 ? " + " : "") + (e.LevelTimes == 1 ? "level" : $"{e.LevelTimes} × level");
        if (e.PoolPerLevel > 0) text += $" from a pool of {e.PoolPerLevel} × level";
        if (e.Flat != 0 && text.Trim().Length == 0) text += e.Flat.ToString(CultureInfo.InvariantCulture);
        else if (e.Flat != 0) text += (e.Flat > 0 ? " + " : " - ") + Math.Abs(e.Flat);
        if (e.Amount > 0 && text.Length == 0) text = e.Amount.ToString(CultureInfo.InvariantCulture);
        if (e.MaxHpShare > 0) text += $"{(text.Length > 0 ? " + " : "")}{e.MaxHpShare:P0} of most HP";
        return text.Trim();
    }

    /// <summary>An aura in a few words: its name, how long, and what it ticks.</summary>
    private static string AuraText(AuraDefinition aura)
    {
        string text = aura.Name + (aura.DurationMs > 0 ? " " + Seconds(aura.DurationMs) : "");
        if (!aura.TickDice.IsNone) text += $" ({(aura.TickHeals ? "heal " : "")}{aura.TickDice}{(aura.TickPlus ? "⁺" : "")} every {Seconds(aura.TickMs)})";
        if (aura.Stacks) text += $", stacks to {aura.MaxStacks}";
        if (aura.Charges > 1) text += $", {aura.Charges} uses";
        return text;
    }

    /// <summary>A Focus model's rates in words.</summary>
    private static string FocusText(FocusRules rules)
    {
        var parts = new List<string>();
        if (rules.PerSecond > 0) parts.Add($"+{rules.PerSecond} a second");
        if (rules.PerHit > 0) parts.Add($"+{rules.PerHit} a hit");
        if (rules.PerCrit > 0 && rules.PerCrit != rules.PerHit) parts.Add($"+{rules.PerCrit} a crit");
        if (rules.PerCursedHit > 0) parts.Add($"+{rules.PerCursedHit} a hit on your cursed target");
        if (rules.PerDodge > 0) parts.Add($"+{rules.PerDodge} a dodge");
        if (rules.WhenHit > 0) parts.Add($"+{rules.WhenHit} when you're hit");
        if (rules.PerBlock > 0) parts.Add($"+{rules.PerBlock} a block");
        if (rules.PerHeal > 0) parts.Add($"+{rules.PerHeal} a heal you land");
        if (rules.PerTick > 0) parts.Add($"+{rules.PerTick} a tick of your damage or healing over time");
        return string.Join(", ", parts);
    }

    /// <summary>Milliseconds as seconds: "8 s", "1.5 s".</summary>
    private static string Seconds(int ms) => (ms / 1000.0).ToString("0.##", CultureInfo.InvariantCulture) + " s";
}
