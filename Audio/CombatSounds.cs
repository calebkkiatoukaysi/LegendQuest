using System;
using System.Collections.Generic;

namespace LegendQuest.Audio;

/// <summary>
/// The real-time fight's sounds by key, and how each one is played: which weapon family or spell
/// school a key belongs to, how far its pitch may wander, whether it ducks the music, and how far
/// into a swing its whoosh is loudest.
/// </summary>
/// <remarks>
/// A key is a kebab string (hit-axe, cast-fire, crit). Every key has takes 1 to 3 at
/// Content/Sfx/Combat/&lt;key&gt;-&lt;take&gt;.wav, built by tools/generate_audio.py, and
/// AudioManager.PlayCombat turns through them. README "Music and sound" says when each one plays.
/// No MonoGame types here, so the screens and the engine can both ask it things.
/// </remarks>
public static class CombatSounds
{
    /// <summary>How many takes of each sound there are on disk.</summary>
    public const int Takes = 3;

    /// <summary>The folder under Content the takes are in.</summary>
    public const string Folder = "Sfx/Combat/";

    /// <summary>The weapon families, each with a swing-&lt;family&gt; and a hit-&lt;family&gt;.</summary>
    public static readonly IReadOnlyList<string> Families = new[]
    {
        "light-blade", "heavy-blade", "axe", "blunt", "polearm", "bow", "crossbow", "unarmed", "claw", "bite",
    };

    /// <summary>The spell schools, each with a cast-&lt;school&gt; and an impact-&lt;school&gt;.</summary>
    public static readonly IReadOnlyList<string> Schools = new[]
    {
        "fire", "frost", "lightning", "radiant", "necrotic", "force", "psychic", "nature", "thunder", "poison", "blood",
    };

    /// <summary>The sounds that aren't a family's or a school's.</summary>
    public static readonly IReadOnlyList<string> Others = new[]
    {
        "crit", "block", "dodge", "miss", "heal", "buff", "debuff", "aura-tick", "telegraph", "boss-phase",
        "knockdown", "body-fall", "death", "downed", "revive", "potion", "soul-link", "ui-press", "ui-denied",
        "cooldown-ready", "level-up", "victory", "defeat",
    };

    /// <summary>Every key, in the order the generator builds them.</summary>
    public static IReadOnlyList<string> Keys { get; } = BuildKeys();

    /// <summary>The keys whose pitch stays put: they're tuned to the battle music, so nudging them would put them out of tune with it.</summary>
    private static readonly HashSet<string> Tuned = new()
    {
        "crit", "heal", "buff", "cast-radiant", "impact-radiant", "cooldown-ready", "level-up", "victory", "defeat",
        "soul-link", "revive", "boss-phase", "debuff", "cast-necrotic", "impact-necrotic", "cast-nature",
    };

    /// <summary>The keys that duck the music for a moment, so they land.</summary>
    private static readonly HashSet<string> Big = new()
    {
        "crit", "boss-phase", "knockdown", "soul-link", "impact-thunder", "level-up", "death", "downed",
    };

    /// <summary>The UI's keys: very little pitch movement, since a click that changes every press sounds broken.</summary>
    private static readonly HashSet<string> Interface = new() { "ui-press", "ui-denied" };

    /// <summary>How far into each family's swing its whoosh is loudest, in milliseconds. tools/generate_audio.py's SWING_LEAD builds them that way.</summary>
    private static readonly Dictionary<string, int> Leads = new()
    {
        ["light-blade"] = 80, ["heavy-blade"] = 140, ["axe"] = 140, ["blunt"] = 160, ["polearm"] = 100,
        ["bow"] = 0, ["crossbow"] = 0, ["unarmed"] = 70, ["claw"] = 70, ["bite"] = 100,
    };

    /// <summary>The weapon family of each weapon in ItemCatalog, by its id (or the base a named weapon is made on).</summary>
    private static readonly Dictionary<string, string> WeaponFamilies = new()
    {
        ["dagger"] = "light-blade", ["shortsword"] = "light-blade", ["rapier"] = "light-blade", ["scimitar"] = "light-blade",
        ["longsword"] = "heavy-blade", ["greatsword"] = "heavy-blade",
        ["handaxe"] = "axe", ["battleaxe"] = "axe",
        ["warhammer"] = "blunt", ["mace"] = "blunt", ["quarterstaff"] = "blunt",
        ["spear"] = "polearm",
        ["shortbow"] = "bow", ["longbow"] = "bow",
        ["light-crossbow"] = "crossbow",
    };

    /// <summary>The family for each of the engine's weapon kinds (Story/11-combat.md), for a fighter with no item to go by.</summary>
    private static readonly Dictionary<string, string> KindFamilies = new()
    {
        ["one-hand"] = "light-blade", ["two-hand"] = "heavy-blade", ["polearm"] = "polearm", ["dagger"] = "light-blade",
        ["unarmed"] = "unarmed", ["hook"] = "light-blade", ["staff"] = "blunt", ["bow"] = "bow", ["crossbow"] = "crossbow",
    };

    /// <summary>Whether a key is one of the combat sounds.</summary>
    /// <param name="key">The key.</param>
    public static bool IsKey(string key) => key is not null && KeySet.Contains(key);

    private static readonly HashSet<string> KeySet = new(BuildKeys());

    /// <summary>How far either way the pitch of a key is nudged each time it plays (1 is an octave).</summary>
    /// <remarks>Blows and swings wander most, 0.08 (about a semitone), the rest 0.05, the bar's clicks 0.02, and anything tuned to the music not at all.</remarks>
    /// <param name="key">The key.</param>
    public static float PitchSpread(string key)
    {
        if (Tuned.Contains(key)) return 0f;
        if (Interface.Contains(key)) return 0.02f;
        if (key.StartsWith("swing-", StringComparison.Ordinal) || key.StartsWith("hit-", StringComparison.Ordinal)) return 0.08f;
        return 0.05f;
    }

    /// <summary>Whether a key ducks the music when it plays.</summary>
    /// <param name="key">The key.</param>
    public static bool Ducks(string key) => Big.Contains(key);

    /// <summary>How long before the blow lands to start a family's swing, so its whoosh peaks on the impact. 0 for bows and crossbows, whose swing is the shot, played on release.</summary>
    /// <param name="family">One of Families.</param>
    public static int SwingLeadMs(string family) => family is not null && Leads.TryGetValue(family, out int ms) ? ms : 0;

    /// <summary>The family of a weapon, from its item id or the base it's made on (ItemDefinition.Base ?? Id). Null gives unarmed.</summary>
    /// <param name="weapon">The weapon's id or base, or null for bare hands.</param>
    public static string FamilyOfWeapon(string weapon) =>
        weapon is not null && WeaponFamilies.TryGetValue(weapon, out string family) ? family : "unarmed";

    /// <summary>The family for one of the engine's weapon kinds (one-hand, two-hand, polearm, dagger, unarmed, hook, staff, bow, crossbow).</summary>
    /// <param name="kind">The weapon kind.</param>
    public static string FamilyOfKind(string kind) =>
        kind is not null && KindFamilies.TryGetValue(kind, out string family) ? family : "unarmed";

    /// <summary>The spell school a damage type sounds like, or null for the three weapon types (their sound comes from the weapon).</summary>
    /// <remarks>Cold sounds as frost and acid as poison; there's no separate acid sound.</remarks>
    /// <param name="type">The damage type.</param>
    public static string SchoolOf(DamageType type) => type switch
    {
        DamageType.Fire => "fire",
        DamageType.Cold => "frost",
        DamageType.Lightning => "lightning",
        DamageType.Thunder => "thunder",
        DamageType.Radiant => "radiant",
        DamageType.Necrotic => "necrotic",
        DamageType.Poison or DamageType.Acid => "poison",
        DamageType.Psychic => "psychic",
        DamageType.Force => "force",
        _ => null,
    };

    /// <summary>The swing key for a family.</summary>
    /// <param name="family">One of Families.</param>
    public static string Swing(string family) => "swing-" + family;

    /// <summary>The hit key for a family.</summary>
    /// <param name="family">One of Families.</param>
    public static string Hit(string family) => "hit-" + family;

    /// <summary>The cast key for a school.</summary>
    /// <param name="school">One of Schools.</param>
    public static string Cast(string school) => "cast-" + school;

    /// <summary>The impact key for a school.</summary>
    /// <param name="school">One of Schools.</param>
    public static string Impact(string school) => "impact-" + school;

    private static List<string> BuildKeys()
    {
        var keys = new List<string>();
        foreach (string family in Families) keys.Add(Swing(family));
        foreach (string family in Families) keys.Add(Hit(family));
        keys.AddRange(new[] { "crit", "block", "dodge", "miss" });
        foreach (string school in Schools)
        {
            keys.Add(Cast(school));
            keys.Add(Impact(school));
        }
        foreach (string other in Others)
            if (!keys.Contains(other)) keys.Add(other);
        return keys;
    }
}
