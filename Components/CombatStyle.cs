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

    /// <summary>A panel's rim, in screen pixels from its edge in: the plum outline, the marble, the gold line.</summary>
    private const int OutlineThickness = 2;
    private const int MarbleThickness = 2;
    private const int GoldThickness = 1;

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

    /// <summary>The enamel the fight's panels are filled with (the log, the cards, the frames), the forms' periwinkle, see-through enough to keep the map under them.</summary>
    public static readonly Color PanelFill = new Color(78, 72, 160) * 0.9f;

    /// <summary>The gold line inside those panels' marble rim, the hotbar's gold.</summary>
    public static readonly Color PanelRim = Palette.Trim;

    /// <summary>The marble rim's lit top and left, and its shaded bottom and right.</summary>
    private static readonly Color MarbleLit = new(255, 251, 240);
    private static readonly Color MarbleShade = new(216, 196, 182);

    /// <summary>What a foe's lines in the log are written in: a soft red, so the eye can tell whose turn did what.</summary>
    public static readonly Color FoeLine = new(240, 164, 150);

    /// <summary>What Focus is written in (a skill's cost, the Focus left): the Focus bar's cool blue, light enough to read on the enamel.</summary>
    public static readonly Color FocusText = new(150, 200, 255);

    /// <summary>The ember the opportunist's ring under a foe is drawn in, and the tint over a slot a skill would catch.</summary>
    public static readonly Color Threat = new(255, 132, 64);

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
        [ConditionId.Marked] = false,
        [ConditionId.Taunted] = false,
        [ConditionId.Burning] = false,
        [ConditionId.Bleeding] = false,
        [ConditionId.Frozen] = false,
        [ConditionId.Poisoned] = false,
        [ConditionId.Stunned] = false,
        [ConditionId.Blinded] = false,
        [ConditionId.Frightened] = false,
        [ConditionId.Charmed] = false,
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
        [ConditionId.Marked] = "The next attack on them has advantage.",
        [ConditionId.Taunted] = "Their attacks have to go at whoever taunted them, when they can.",
        [ConditionId.Burning] = "On fire: damage at the start of each of their turns.",
        [ConditionId.Bleeding] = "Damage at the start of each of their turns.",
        [ConditionId.Frozen] = "Stopped in time: they skip their turns, and only an area can reach them.",
        [ConditionId.Poisoned] = "Damage at the start of each of their turns, and disadvantage on their attacks.",
        [ConditionId.Stunned] = "They skip their turns, and attacks on them have advantage.",
        [ConditionId.Blinded] = "Disadvantage on their attacks, and attacks on them have advantage.",
        [ConditionId.Frightened] = "Disadvantage on their attacks.",
        [ConditionId.Charmed] = "They can't attack whoever charmed them.",
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
    }

    /// <summary>The colour of a kind of damage: its sparks, its motes and its number's glow.</summary>
    /// <param name="kind">The kind of damage.</param>
    public static Color DamageColour(DamageType kind) => Damage[kind].Colour;

    /// <summary>The sound of a kind of damage landing.</summary>
    /// <param name="kind">The kind of damage.</param>
    public static Sfx DamageSound(DamageType kind) => Damage[kind].Sound;

    /// <summary>What a condition does, in a line.</summary>
    /// <param name="condition">The condition.</param>
    public static string Explain(ConditionId condition) => Meaning[condition];

    /// <summary>Whether a condition's good news for whoever has it.</summary>
    /// <param name="condition">The condition.</param>
    public static bool Helps(ConditionId condition) => Helpful[condition];

    /// <summary>Draws one of the fight's panels: the enamel fill in a rim like the forms' panel, a plum outline with its corners rounded off, cream marble, and a gold line.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, deferred.</param>
    /// <param name="pixel">One white pixel: the screen manager's BlankTexture.</param>
    /// <param name="area">Where, in screen pixels.</param>
    /// <param name="alpha">How faded in it is.</param>
    public static void DrawPanel(SpriteBatch spriteBatch, Texture2D pixel, Rectangle area, float alpha)
    {
        int o = OutlineThickness, m = MarbleThickness, g = GoldThickness;
        Ring(spriteBatch, pixel, area, o, Palette.Plum * alpha, Palette.Plum * alpha, o);
        Rectangle marble = Deflate(area, o);
        Ring(spriteBatch, pixel, marble, m, MarbleLit * alpha, MarbleShade * alpha, 0);
        // The outline's rounded corners step in one more pixel over the marble.
        foreach (Point corner in new[] { new Point(marble.Left, marble.Top), new Point(marble.Right - 1, marble.Top), new Point(marble.Left, marble.Bottom - 1), new Point(marble.Right - 1, marble.Bottom - 1) })
            spriteBatch.Draw(pixel, new Rectangle(corner.X, corner.Y, 1, 1), Palette.Plum * alpha);
        Rectangle gold = Deflate(marble, m);
        Ring(spriteBatch, pixel, gold, g, PanelRim * alpha, Color.Lerp(PanelRim, new Color(150, 90, 40), 0.45f) * alpha, 0);
        spriteBatch.Draw(pixel, Deflate(gold, g), PanelFill * alpha);
    }

    /// <summary>A rectangle brought in by the same amount on every side.</summary>
    private static Rectangle Deflate(Rectangle area, int by) => new(area.X + by, area.Y + by, area.Width - by * 2, area.Height - by * 2);

    /// <summary>Draws a ring just inside a rectangle: its top and left in one colour, its bottom and right in another, the corners of its outer edge cut back by a few pixels.</summary>
    private static void Ring(SpriteBatch spriteBatch, Texture2D pixel, Rectangle area, int thickness, Color lit, Color shade, int cut)
    {
        int side = Math.Max(cut, thickness);
        spriteBatch.Draw(pixel, new Rectangle(area.Left + cut, area.Top, area.Width - cut * 2, thickness), lit);
        spriteBatch.Draw(pixel, new Rectangle(area.Left, area.Top + side, thickness, area.Height - side * 2), lit);
        spriteBatch.Draw(pixel, new Rectangle(area.Left + cut, area.Bottom - thickness, area.Width - cut * 2, thickness), shade);
        spriteBatch.Draw(pixel, new Rectangle(area.Right - thickness, area.Top + side, thickness, area.Height - side * 2), shade);
    }
}
