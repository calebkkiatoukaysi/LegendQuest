using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components.Realtime;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Components;

/// <summary>
/// The character sheet, as the inventory's right-hand page: level, class and the experience to the
/// next level, their people and what their class is for, proficiency, the six scores with their
/// modifiers and saves, how they attack, and their class's kit with what each skill costs in a fight
/// and the rank that opens it.
/// </summary>
/// <remarks>
/// Every number comes from where a fight gets it (CombatStats, LevelTable, SkillCatalog), so
/// the sheet can't disagree with the dice. It's laid out again whenever it's shown someone, and an
/// skill's row can be picked with the keys or the mouse to read what it does in the box at the
/// bottom, the way a long description shouldn't crowd the list. Anything that won't fit above the
/// bottom of its area is left off rather than drawn over what's under it. The tiles go in a point
/// sampled batch, the words in a linear one after.
/// </remarks>
public class CharacterSheet
{
    /// <summary>5e's spell save DC before proficiency and the casting modifier.</summary>
    private const int SpellDcBase = 8;

    /// <summary>The room between the six score tiles, between parts of the sheet, and under a heading, in screen pixels.</summary>
    private const int TileGap = 8;
    private const int SectionGap = 16;
    private const int HeadingGap = 2;

    /// <summary>Where an attack's columns start, from the sheet's left edge: its to-hit and its damage. Each has its name over it on the heading's row.</summary>
    private const float HitColumn = 220f;
    private const float DamageColumn = 330f;

    /// <summary>What goes between the parts of a line: a people and a role, or what an ability costs and what's left.</summary>
    private const string Separator = "  ·  ";

    /// <summary>The six abilities, in the order the sheet sets them out.</summary>
    private static readonly Ability[] Abilities = Enum.GetValues<Ability>();

    private readonly StatTiles _tiles = new();

    /// <summary>The fight's icons, for each skill's row, as the hotbar shows them.</summary>
    private readonly HudIcons _icons = new();

    /// <summary>How wide the room is in front of a skill's name for its icon, in screen pixels.</summary>
    private const int IconRoom = 30;
    private readonly List<(string Text, float X, float Y, Color Colour, bool Big, bool Right)> _words = new();
    private readonly List<(Rectangle Tile, string Name, string Number, string Under)> _scores = new();
    private readonly List<(Rectangle Row, SkillDefinition Skill, bool Locked)> _abilities = new();

    private SpriteFont _font;
    private SpriteFont _detailFont;
    private Rectangle _area;
    private float _detailTop;
    private int _picked;

    /// <summary>Loads the fonts, the tiles and the skills' icons.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="device">The graphics device, for the icons' sweep.</param>
    public void LoadContent(ContentManager content, GraphicsDevice device)
    {
        _tiles.LoadContent(content);
        _icons.LoadContent(content, device);
        _font = content.Load<SpriteFont>("spectral-ui");
        _detailFont = content.Load<SpriteFont>("spectral-detail");
    }

    /// <summary>Lays the sheet out for someone, in an area of the screen. The picked ability stays picked if they have that many.</summary>
    /// <param name="member">Whose sheet.</param>
    /// <param name="area">Where it goes, in screen pixels.</param>
    public void Show(PartyMember member, Rectangle area)
    {
        _area = area;
        _words.Clear();
        _scores.Clear();
        _abilities.Clear();

        float y = area.Top;
        y = LayOutHeader(member, y);
        y = LayOutScores(member, y + SectionGap);
        y = LayOutAttacks(member, y + SectionGap);
        LayOutAbilities(member, y + SectionGap);
        _picked = Math.Clamp(_picked, 0, Math.Max(0, _abilities.Count - 1));
    }

    /// <summary>Picks the first ability, for someone else's sheet.</summary>
    public void PickFirst() => _picked = 0;

    /// <summary>Moves the pick up or down the abilities, wrapping round.</summary>
    /// <param name="step">-1 up, 1 down.</param>
    /// <returns>True if there's more than one to move between.</returns>
    public bool Step(int step)
    {
        if (_abilities.Count < 2) return false;
        _picked = (_picked + step + _abilities.Count) % _abilities.Count;
        return true;
    }

    /// <summary>Picks the ability under a point, if there is one.</summary>
    /// <param name="point">Where, in screen pixels: the mouse.</param>
    /// <returns>True if the pick moved to another.</returns>
    public bool PickAt(Point point)
    {
        for (int i = 0; i < _abilities.Count; i++)
        {
            if (!_abilities[i].Row.Contains(point) || i == _picked) continue;
            _picked = i;
            return true;
        }
        return false;
    }

    /// <summary>Draws the score tiles. Goes in a point sampled batch, before DrawWords.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawTiles(SpriteBatch spriteBatch, float alpha)
    {
        foreach ((Rectangle tile, _, _, _) in _scores) _tiles.DrawTile(spriteBatch, tile, alpha);

        // Each skill's icon in front of its name, dimmed while its rank's still to come.
        float scale = MathF.Min(1f, (_detailFont.LineSpacing - 2f) / _icons.Size);
        foreach ((Rectangle row, SkillDefinition skill, bool locked) in _abilities)
        {
            float drop = (row.Height - _icons.Size * scale) / 2f;
            _icons.Draw(spriteBatch, skill.Id, new Vector2(row.Left, MathF.Round(row.Top + drop)), scale, (locked ? Color.Gray * 0.45f : Color.White) * alpha);
        }
    }

    /// <summary>Writes everything on the sheet, the picked ability's row in torchlight and what it does under the list. Goes in a linear sampled batch, after DrawTiles.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="alpha">How far the screen it's on has faded in, 0 to 1.</param>
    public void DrawWords(SpriteBatch spriteBatch, float alpha)
    {
        foreach ((string text, float x, float y, Color colour, bool big, bool right) in _words)
            Text.Draw(spriteBatch, big ? _font : _detailFont, text, x, y, colour * alpha, Palette.Shadow * alpha, rightAligned: right);

        foreach ((Rectangle tile, string name, string number, string under) in _scores)
            _tiles.DrawWords(spriteBatch, tile, name, number, under, Palette.Gold, alpha);

        for (int i = 0; i < _abilities.Count; i++)
        {
            (Rectangle row, SkillDefinition skill, bool locked) = _abilities[i];
            Color colour = i == _picked ? Palette.Torch : locked ? Palette.Note : Palette.Bone;
            Text.Draw(spriteBatch, _detailFont, skill.Name, row.Left + IconRoom, row.Top, colour * alpha, Palette.Shadow * alpha);
        }

        if (_picked >= _abilities.Count) return;

        float top = _detailTop;
        foreach (string row in Text.Wrap(_abilities[_picked].Skill.Description, _detailFont, _area.Width))
        {
            if (top + _detailFont.LineSpacing > _area.Bottom) break;
            Text.Draw(spriteBatch, _detailFont, row, _area.Left, top, Palette.Narration * alpha, Palette.Shadow * alpha);
            top += _detailFont.LineSpacing;
        }
    }

    /// <summary>Their level and class, then their people, what their class is for if there's room, and the experience to their next level.</summary>
    /// <returns>Where the next part starts.</returns>
    private float LayOutHeader(PartyMember member, float y)
    {
        ClassDefinition kind = ClassCatalog.Get(member.Class);
        string level = "LEVEL " + member.Level.ToString(CultureInfo.InvariantCulture) + "  " + kind.Name.ToUpperInvariant();
        Word(level, _area.Left, y, Palette.Heading, big: true);
        y += _font.LineSpacing;

        // The experience goes at the right of the line under (beside a long class's name it ran into
        // it), and the class's role before it only if there's room: a line more would push the kit's
        // last row off the sheet.
        string who = member.Look.Ancestry + Separator + kind.Role, next = ToNextLevel(member);
        if (_detailFont.MeasureString(who + Separator + next).X > _area.Width) who = member.Look.Ancestry.ToString();
        Word(who, _area.Left, y, Palette.Note);
        Word(next, _area.Right, y, Palette.Note, right: true);
        return y + _detailFont.LineSpacing;
    }

    /// <summary>How far they are from their next level, like "480 XP TO LEVEL 3", or that they're at the highest.</summary>
    private static string ToNextLevel(PartyMember member)
    {
        if (member.Level >= LevelTable.MaxLevel) return "HIGHEST LEVEL";
        int left = Math.Max(0, LevelTable.ToNext(member.Level) - member.Xp);
        return left.ToString(CultureInfo.InvariantCulture) + " XP TO LEVEL " + (member.Level + 1).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>A tile for each score with its modifier, and each save's bonus under its tile, in gold for the class's own saves.</summary>
    /// <returns>Where the next part starts.</returns>
    private float LayOutScores(PartyMember member, float y)
    {
        int width = (_area.Width - TileGap * (Abilities.Length - 1)) / Abilities.Length;
        for (int i = 0; i < Abilities.Length; i++)
        {
            Ability ability = Abilities[i];
            var tile = new Rectangle(_area.Left + i * (width + TileGap), (int)y, width, _tiles.TallHeight);
            _scores.Add((tile, CombatStats.Abbreviation(ability), member.Scores[ability].ToString(CultureInfo.InvariantCulture),
                ItemReadout.Signed(member.Scores.Modifier(ability))));

            int save = CombatStats.SaveBonus(member, ability);
            bool trained = save != member.Scores.Modifier(ability);
            Word("SAVE " + ItemReadout.Signed(save), tile.Center.X, tile.Bottom, trained ? Palette.Gold : Palette.Note, centred: true);
        }
        return y + _tiles.TallHeight + _detailFont.LineSpacing;
    }

    /// <summary>How they attack: their proficiency bonus, the main hand (or a fist), the off hand if there's a weapon in it, and their spells if they cast; and a warning if their armour's one they aren't trained in.</summary>
    /// <returns>Where the next part starts.</returns>
    private float LayOutAttacks(PartyMember member, float y)
    {
        Word("TO HIT", _area.Left + HitColumn, y, Palette.Note);
        Word("DAMAGE", _area.Left + DamageColumn, y, Palette.Note);
        int proficiency = LevelTable.Proficiency(member.Level);
        Word("PROFICIENCY " + ItemReadout.Signed(proficiency), _area.Right, y, Palette.Gold, right: true);
        y = Heading("ATTACKS", y);
        y = Attack(CombatStats.Weapon(member, EquipmentSlot.MainHand) ?? CombatStats.Unarmed(member.Scores, member.Level), y);
        if (CombatStats.Weapon(member, EquipmentSlot.OffHand) is AttackProfile offHand) y = Attack(offHand, y);

        if (CombatStats.CastingAbility(member) is Ability casting)
        {
            int modifier = member.Scores.Modifier(casting);
            Word("Spells (" + CombatStats.Abbreviation(casting) + ")", _area.Left, y, Palette.Bone);
            Word(ItemReadout.Signed(proficiency + modifier + CombatStats.SpellAttackBonus(member)), _area.Left + HitColumn, y, Palette.Gold);
            Word("Save DC " + (SpellDcBase + proficiency + modifier).ToString(CultureInfo.InvariantCulture), _area.Left + DamageColumn, y, Palette.Bone);
            y += _detailFont.LineSpacing;
        }

        if (CombatStats.ArmourUntrained(member))
        {
            Word("Untrained armour: disadvantage on attacks, no spells.", _area.Left, y, Palette.Ember);
            y += _detailFont.LineSpacing;
        }
        return y;
    }

    /// <summary>One attack's row: what it's with, its to-hit, and its damage.</summary>
    /// <returns>Where the next row starts.</returns>
    private float Attack(AttackProfile attack, float y)
    {
        string damage = attack.Damage + " " + attack.DamageType.ToString().ToLowerInvariant();
        if (!attack.Extra.IsNone) damage += " + " + attack.Extra + " " + attack.ExtraType.ToString().ToLowerInvariant();

        Word(attack.Name, _area.Left, y, Palette.Bone);
        Word(ItemReadout.Signed(attack.Bonus), _area.Left + HitColumn, y, Palette.Gold);
        Word(damage, _area.Left + DamageColumn, y, Palette.Bone);
        return y + _detailFont.LineSpacing;
    }

    /// <summary>The rank that opens each of a kit's skills, in ClassKit.All's order: the auto attack and the first skill, the second and third, the ultimate, and the two passives.</summary>
    private static readonly int[] OpensAt =
    {
        1, 1, ClassRanks.Skill2Rank, ClassRanks.Skill3Rank, ClassRanks.UltimateRank, ClassRanks.PassiveRank, ClassRanks.LastPassiveRank,
    };

    /// <summary>Their class's kit (SkillCatalog), a row each with its icon and what it costs in a fight, the ones their rank hasn't opened yet dimmed with the rank that does, and the room under them for the picked one's description. The heading has their rank in the class and its class points, and how many more the next rank wants.</summary>
    private void LayOutAbilities(PartyMember member, float y)
    {
        int points = member.PointsIn(member.Class);
        string next = member.Rank >= ClassRanks.MaxRank ? "HIGHEST RANK"
            : $"{ClassRanks.PointsFor(member.Rank + 1) - points} CP TO RANK {member.Rank + 1}";
        Word(next, _area.Right, y, Palette.Note, right: true);
        y = Heading($"SKILLS  ·  RANK {member.Rank}  ·  {points} CP", y);
        IReadOnlyList<SkillDefinition> kit = SkillCatalog.Kit(member.Class).All;
        for (int i = 0; i < kit.Count; i++)
        {
            SkillDefinition skill = kit[i];
            if (skill is null) continue;
            if (y + _detailFont.LineSpacing > _area.Bottom) break;
            int opens = i < OpensAt.Length ? OpensAt[i] : 1;
            bool locked = member.Rank < opens;
            _abilities.Add((new Rectangle(_area.Left, (int)y, _area.Width, _detailFont.LineSpacing), skill, locked));
            Word(locked ? $"RANK {opens}" : Tag(skill), _area.Right, y, Palette.Note, right: true);
            y += _detailFont.LineSpacing;
        }
        _detailTop = y + HeadingGap * 2;
    }

    /// <summary>What a skill costs to use in a fight, like "BONUS  ·  20 FOCUS  ·  12 S", or "PASSIVE": its kind, its Focus and its cooldown, short enough to clear the skill's name and icon.</summary>
    private static string Tag(SkillDefinition skill)
    {
        var parts = new List<string>
        {
            skill.Kind switch
            {
                SkillKind.Auto => "AUTO ATTACK",
                SkillKind.Bonus => "BONUS",
                SkillKind.Reaction => "REACTION",
                SkillKind.Passive => "PASSIVE",
                _ => skill.Ultimate ? "ULTIMATE" : "ACTION",
            },
        };
        if (skill.Focus > 0) parts.Add(skill.Focus.ToString(CultureInfo.InvariantCulture) + " FOCUS");
        if (skill.CooldownMs > 0) parts.Add((skill.CooldownMs / 1000f).ToString("0.#", CultureInfo.InvariantCulture) + " S");
        return string.Join(Separator, parts);
    }

    /// <summary>A part's heading.</summary>
    /// <returns>Where the part's first row goes.</returns>
    private float Heading(string text, float y)
    {
        Word(text, _area.Left, y, Palette.Heading);
        return y + _detailFont.LineSpacing + HeadingGap;
    }

    /// <summary>Puts some words on the sheet, if they fit above the bottom of its area.</summary>
    private void Word(string text, float x, float y, Color colour, bool big = false, bool right = false, bool centred = false)
    {
        SpriteFont font = big ? _font : _detailFont;
        if (y + font.LineSpacing > _area.Bottom) return;

        float left = centred ? x - font.MeasureString(text).X / 2f : x;
        _words.Add((text, left, y, colour, big, right));
    }
}
