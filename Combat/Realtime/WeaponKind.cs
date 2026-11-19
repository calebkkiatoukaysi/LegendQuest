using LegendQuest.Characters;
using LegendQuest.Items;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// How a fighter holds what they fight with, which picks their auto attack's clips
/// (auto-&lt;kind&gt;-1, -2, -3), its swing timer, and the sound family of its blows.
/// </summary>
/// <remarks>Contract §2.3 and §4. The clip names are the kebab-case of these (WeaponKinds.Clip).</remarks>
public enum WeaponKind
{
    /// <summary>A sword, an axe, a mace in one hand.</summary>
    OneHand,

    /// <summary>A greatsword, or anything heavy or two-handed that isn't ranged.</summary>
    TwoHand,

    /// <summary>A spear or a reach weapon.</summary>
    Polearm,

    /// <summary>A dagger.</summary>
    Dagger,

    /// <summary>Fists, claws and bites.</summary>
    Unarmed,

    /// <summary>A hook hand or a gaff.</summary>
    Hook,

    /// <summary>A staff.</summary>
    Staff,

    /// <summary>A bow.</summary>
    Bow,

    /// <summary>A crossbow.</summary>
    Crossbow,
}

/// <summary>Working out a weapon kind, and its names and timers.</summary>
public static class WeaponKinds
{
    /// <summary>The swing timer of most weapons: 2.0 s (11-combat.md §2.5).</summary>
    public const int SwingMs = 2000;

    /// <summary>Light and finesse weapons: 1.8 s.</summary>
    public const int LightSwingMs = 1800;

    /// <summary>Heavy and two-handed weapons: 2.4 s.</summary>
    public const int HeavySwingMs = 2400;

    /// <summary>Crossbows: 2.6 s.</summary>
    public const int CrossbowSwingMs = 2600;

    /// <summary>Contract §4: the kind from what's in the main hand.</summary>
    /// <param name="item">The main-hand item, or null for nothing.</param>
    public static WeaponKind Of(ItemDefinition item)
    {
        if (item is null || item.Kind != ItemKind.Weapon) return WeaponKind.Unarmed;
        if (item.Has(WeaponProperty.Ranged)) return item.Has(WeaponProperty.Loading) ? WeaponKind.Crossbow : WeaponKind.Bow;
        string layer = item.Layer ?? string.Empty;
        if (layer.Contains("staff")) return WeaponKind.Staff;
        if (item.Has(WeaponProperty.Reach) || layer.Contains("spear")) return WeaponKind.Polearm;
        if (item.Has(WeaponProperty.TwoHanded) || item.Has(WeaponProperty.Heavy)) return WeaponKind.TwoHand;
        if (layer.Contains("dagger")) return WeaponKind.Dagger;
        if (layer.Contains("hook")) return WeaponKind.Hook;
        return WeaponKind.OneHand;
    }

    /// <summary>A party member's kind, from their main hand.</summary>
    /// <param name="member">Who.</param>
    public static WeaponKind Of(PartyMember member) => Of(member.WornIn(EquipmentSlot.MainHand));

    /// <summary>The swing timer a weapon gives, before haste: light and finesse 1.8 s, heavy and two-handed 2.4 s, crossbows 2.6 s, the rest 2.0 s.</summary>
    /// <param name="item">The main-hand weapon, or null.</param>
    public static int SwingOf(ItemDefinition item)
    {
        if (item is null || item.Kind != ItemKind.Weapon) return SwingMs;
        if (item.Has(WeaponProperty.Loading)) return CrossbowSwingMs;
        if (item.Has(WeaponProperty.Heavy) || item.Has(WeaponProperty.TwoHanded) && !item.Has(WeaponProperty.Ranged)) return HeavySwingMs;
        if (item.Has(WeaponProperty.Light) || item.Has(WeaponProperty.Finesse)) return LightSwingMs;
        return SwingMs;
    }

    /// <summary>The kind as clip names write it: one-hand, two-hand, polearm, dagger, unarmed, hook, staff, bow, crossbow.</summary>
    /// <param name="kind">The kind.</param>
    public static string Clip(WeaponKind kind) => kind switch
    {
        WeaponKind.OneHand => "one-hand",
        WeaponKind.TwoHand => "two-hand",
        WeaponKind.Polearm => "polearm",
        WeaponKind.Dagger => "dagger",
        WeaponKind.Unarmed => "unarmed",
        WeaponKind.Hook => "hook",
        WeaponKind.Staff => "staff",
        WeaponKind.Bow => "bow",
        _ => "crossbow",
    };

    /// <summary>The auto attack's clip for the nth swing: auto-&lt;kind&gt;-1, -2, -3, then round again.</summary>
    /// <param name="kind">The kind.</param>
    /// <param name="swing">How many swings came before this one.</param>
    public static string AutoClip(WeaponKind kind, int swing) => $"auto-{Clip(kind)}-{swing % 3 + 1}";

    /// <summary>Whether the kind shoots rather than swings, so its auto attack's blow is a release.</summary>
    /// <param name="kind">The kind.</param>
    public static bool Shoots(WeaponKind kind) => kind is WeaponKind.Bow or WeaponKind.Crossbow;
}
