using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;

namespace LegendQuest.Components;

/// <summary>
/// How a fight looks and sounds, as data: the colour and the sound of each kind of damage, what
/// flies for each ability, the colour a spell's light is, and which conditions are good news.
/// </summary>
/// <remarks>
/// Tables rather than switches spread through the screen, so a new kind of damage or a new spell
/// gets its look in one place, and the static constructor throws if one is missed, the first time
/// a fight starts. The colours keep to the game's bright fantasy: steel throws ember sparks, fire
/// orange, frost ice blue, radiant light gold, force violet.
/// </remarks>
public static class CombatStyle
{
    /// <summary>The thing that flies for a weapon attack made from range.</summary>
    public const string Arrow = "arrow";

    /// <summary>How thick a panel's rim is, in screen pixels, and how bright its gold.</summary>
    private const int RimThickness = 2;
    private const float RimStrength = 0.7f;

    /// <summary>Hit points on the party's side: a fresh green.</summary>
    public static readonly Color PartyHealth = new(110, 206, 104);

    /// <summary>Hit points on the foes' side: a hot red.</summary>
    public static readonly Color FoeHealth = new(226, 72, 58);

    /// <summary>What healing looks like: the warm gold-white of the motes and the numbers.</summary>
    public static readonly Color Healing = new(150, 236, 128);

    /// <summary>The light of a spell that does no damage (an inspiration, a blessing): the Soul-Bond's gold.</summary>
    public static readonly Color Blessing = new(255, 222, 128);

    /// <summary>The colour of the vines and weeds of a spell that holds someone where they stand.</summary>
    public static readonly Color Nature = new(132, 204, 92);

    /// <summary>The navy the fight's panels are filled with (the log, the cards, the frame round whoever's turn it is), see-through enough to keep the map under them.</summary>
    public static readonly Color PanelFill = new Color(12, 18, 38) * 0.86f;

    /// <summary>The gold rim round those panels, the hotbar's gold.</summary>
    public static readonly Color PanelRim = new(214, 172, 84);

    /// <summary>What a foe's lines in the log are written in: a soft red, so the eye can tell whose turn did what.</summary>
    public static readonly Color FoeLine = new(240, 164, 150);

    /// <summary>Each kind of damage: its sparks' colour, and the sound of it landing.</summary>
    private static readonly Dictionary<DamageType, (Color Colour, Sfx Sound)> Damage = new()
    {
        [DamageType.Slashing] = (new Color(255, 168, 86), Sfx.SwordHit),
        [DamageType.Piercing] = (new Color(255, 186, 110), Sfx.SwordHit),
        [DamageType.Bludgeoning] = (new Color(236, 214, 176), Sfx.Thud),
        [DamageType.Fire] = (new Color(255, 118, 40), Sfx.Fire),
        [DamageType.Cold] = (new Color(140, 212, 255), Sfx.Frost),
        [DamageType.Lightning] = (new Color(196, 220, 255), Sfx.Cast),
        [DamageType.Thunder] = (new Color(186, 186, 255), Sfx.Thud),
        [DamageType.Radiant] = (new Color(255, 226, 128), Sfx.Cast),
        [DamageType.Necrotic] = (new Color(168, 116, 214), Sfx.Cast),
        [DamageType.Poison] = (new Color(150, 222, 96), Sfx.Cast),
        [DamageType.Psychic] = (new Color(255, 132, 212), Sfx.Cast),
        [DamageType.Force] = (new Color(178, 142, 255), Sfx.Thud),
        [DamageType.Acid] = (new Color(176, 240, 96), Sfx.Cast),
    };

    /// <summary>What flies from the user to the target for each ability that throws or shoots something, by its hud.png piece. A weapon attack from range flies an Arrow; anything else here lands where it is.</summary>
    private static readonly Dictionary<AbilityId, string> Flying = new()
    {
        [AbilityId.FireBolt] = "fire-bolt",
        [AbilityId.ProduceFlame] = "fire-bolt",
        [AbilityId.RayOfFrost] = "frost-ray",
        [AbilityId.EldritchBlast] = "eldritch-blast",
        [AbilityId.MagicMissile] = "magic-missile",
        [AbilityId.ThrowAlchemistsFire] = "flask",
        [AbilityId.BlastingCharge] = "flask",
    };

    /// <summary>The conditions that are good news for whoever has them, so they pop up in gold rather than red.</summary>
    private static readonly Dictionary<ConditionId, bool> Helpful = new()
    {
        [ConditionId.Downed] = false,
        [ConditionId.Prone] = false,
        [ConditionId.Slowed] = false,
        [ConditionId.Disengaged] = true,
        [ConditionId.Dodging] = true,
        [ConditionId.Restrained] = false,
        [ConditionId.Hexed] = false,
        [ConditionId.Inspired] = true,
        [ConditionId.Stable] = true,
        [ConditionId.Helped] = true,
        [ConditionId.Mocked] = false,
    };

    /// <summary>What each condition does, in a line, for its icon's card.</summary>
    private static readonly Dictionary<ConditionId, string> Meaning = new()
    {
        [ConditionId.Downed] = "At 0 hit points: out of the fight, rolling a death save each turn until helped up or healed.",
        [ConditionId.Prone] = "On the ground: melee attacks on them have advantage, ranged ones disadvantage, and theirs have disadvantage. Getting up costs half their movement.",
        [ConditionId.Slowed] = "3 m less movement on their next turn.",
        [ConditionId.Disengaged] = "Moving doesn't give anyone an opportunity attack for the rest of this turn.",
        [ConditionId.Dodging] = "Attacks on them have disadvantage, and their Dexterity saves advantage, until their next turn.",
        [ConditionId.Restrained] = "Held fast: no movement, attacks on them have advantage and theirs disadvantage. A Strength save at the end of each turn breaks free.",
        [ConditionId.Hexed] = "Cursed: whoever hexed them does 1d6 necrotic more on every hit.",
        [ConditionId.Inspired] = "Holding a bardic inspiration die, added to the first attack or save that would fail by little enough.",
        [ConditionId.Stable] = "Downed but no longer dying: no more death saves, still out until healed.",
        [ConditionId.Helped] = "Advantage on their next attack.",
        [ConditionId.Mocked] = "Rattled: disadvantage on their next attack.",
    };

    /// <summary>Checks every kind of damage and every condition has its look, and that everything that flies is on hud.png.</summary>
    /// <remarks>Throws on purpose.</remarks>
    static CombatStyle()
    {
        foreach (DamageType kind in Enum.GetValues<DamageType>())
            if (!Damage.ContainsKey(kind)) throw new InvalidOperationException($"Damage type {kind} has no colour or sound in CombatStyle.");
        foreach (ConditionId condition in Enum.GetValues<ConditionId>())
        {
            if (!Helpful.ContainsKey(condition)) throw new InvalidOperationException($"Condition {condition} isn't marked good news or bad in CombatStyle.");
            if (!Meaning.ContainsKey(condition)) throw new InvalidOperationException($"Condition {condition} has no line saying what it does in CombatStyle.");
        }

        CombatIcons.Frames(Arrow);
        foreach (string piece in Flying.Values) CombatIcons.Frames(piece);
    }

    /// <summary>The colour of a kind of damage: its sparks, its motes and its number's glow.</summary>
    /// <param name="kind">The kind of damage.</param>
    public static Color DamageColour(DamageType kind) => Damage[kind].Colour;

    /// <summary>The sound of a kind of damage landing.</summary>
    /// <param name="kind">The kind of damage.</param>
    public static Sfx DamageSound(DamageType kind) => Damage[kind].Sound;

    /// <summary>The hud.png piece that flies for an ability, or null if nothing does.</summary>
    /// <param name="ability">The ability.</param>
    public static string Projectile(AbilityId ability) => Flying.GetValueOrDefault(ability);

    /// <summary>What a condition does, in a line.</summary>
    /// <param name="condition">The condition.</param>
    public static string Explain(ConditionId condition) => Meaning[condition];

    /// <summary>Whether a condition's good news for whoever has it.</summary>
    /// <param name="condition">The condition.</param>
    public static bool Helps(ConditionId condition) => Helpful[condition];

    /// <summary>Draws one of the fight's panels: the navy fill and a thin gold rim round it.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, deferred.</param>
    /// <param name="pixel">One white pixel: the screen manager's BlankTexture.</param>
    /// <param name="area">Where, in screen pixels.</param>
    /// <param name="alpha">How faded in it is.</param>
    public static void DrawPanel(SpriteBatch spriteBatch, Texture2D pixel, Rectangle area, float alpha)
    {
        spriteBatch.Draw(pixel, area, PanelFill * alpha);
        Color rim = PanelRim * (RimStrength * alpha);
        spriteBatch.Draw(pixel, new Rectangle(area.Left, area.Top, area.Width, RimThickness), rim);
        spriteBatch.Draw(pixel, new Rectangle(area.Left, area.Bottom - RimThickness, area.Width, RimThickness), rim);
        spriteBatch.Draw(pixel, new Rectangle(area.Left, area.Top, RimThickness, area.Height), rim);
        spriteBatch.Draw(pixel, new Rectangle(area.Right - RimThickness, area.Top, RimThickness, area.Height), rim);
    }

    /// <summary>The colour of the light a spell or a gift makes round its user and its target.</summary>
    /// <param name="ability">The ability.</param>
    public static Color LightOf(AbilityDefinition ability)
    {
        if (ability.Kind is AbilityKind.Heal or AbilityKind.Potion) return Healing;
        if (ability.Conditions.Contains(ConditionId.Restrained)) return Nature;
        if (ability.Damage.IsNone || ability.Kind == AbilityKind.Condition && ability.Id != AbilityId.Hex) return Blessing;
        return DamageColour(ability.DamageType);
    }
}
