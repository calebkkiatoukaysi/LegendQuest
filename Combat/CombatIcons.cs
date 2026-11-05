using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Combat;

/// <summary>
/// Where every piece of combat art is: the ability icons, the condition icons, and the named pieces
/// of the fight's screen (the hotbar's slots, the initiative portraits, the action pips, the
/// movement bar, death saves, the Soul-Link sigil, the rings and dots on the ground, and the things
/// that fly), each as a source rectangle on its sheet under Content/Combat/.
/// </summary>
/// <remarks>
/// <para>
/// tools/generate_combat_art.py draws the sheets. Before it draws a thing it reads this file and the
/// AbilityId and ConditionId enums, and stops with what's different if the names, the places or the
/// sizes here aren't its own, so the art and this index can't drift apart.
/// </para>
/// <para>
/// Everything is keyed by name, spelt the script's way ("fire-bolt"). A name is matched without its
/// hyphens and whatever its case, so an enum's own name ("FireBolt") finds the same icon: that's
/// how the combat screen gets from an AbilityId or a ConditionId to its icon, and the overloads
/// that take the enums do it for you. The static constructor checks every AbilityId and every
/// ConditionId has its icon, in the enum's order, and throws on purpose if one doesn't, so an
/// ability added to the enum without its icon shows up the first time a fight draws.
/// </para>
/// </remarks>
public static class CombatIcons
{
    /// <summary>The ability icons' sheet, as the content manager names it.</summary>
    public const string AbilitySheet = "Combat/abilities";

    /// <summary>The condition icons' sheet, as the content manager names it.</summary>
    public const string ConditionSheet = "Combat/conditions";

    /// <summary>The sheet with every other piece on it, as the content manager names it.</summary>
    public const string HudSheet = "Combat/hud";

    /// <summary>An ability icon's size, in art pixels: a tile with its icon drawn on it. ICON in the script.</summary>
    public const int AbilitySize = 24;

    /// <summary>How many ability icons there are to a row. COLUMNS in the script.</summary>
    public const int AbilityColumns = 16;

    /// <summary>A condition icon's size, in art pixels. CONDITION in the script.</summary>
    public const int ConditionSize = 12;

    /// <summary>
    /// How far in from a hotbar slot's or an initiative portrait's edge, in art pixels, the well is
    /// that the icon (24x24) or the face (24x26) goes in. FRAME_INSET in the script.
    /// </summary>
    public const int FrameInset = 3;

    /// <summary>
    /// The cell the icons that aren't abilities start from: the first of the fourth row, so the
    /// abilities can grow to that many before they meet them. EXTRAS_FROM in the script.
    /// </summary>
    private const int ExtrasFrom = 48;

    /// <summary>How wide hud.png is, in pixels. HUD_WIDTH in the script.</summary>
    private const int HudWidth = 256;

    /// <summary>How tall hud.png is, in pixels. HUD_HEIGHT in the script.</summary>
    private const int HudHeight = 100;

    /// <summary>
    /// Every ability's icon, in AbilityId's order: an ability's icon is the cell numbered (int)its
    /// id. ABILITIES in the script.
    /// </summary>
    private static readonly string[] Abilities =
    {
        "attack", "off-hand-attack", "dash", "disengage", "dodge", "help", "drink-potion",
        "throw-alchemists-fire", "soul-link-strike", "second-wind", "defence", "sneak-attack",
        "cunning-dash", "cunning-disengage", "sacred-flame", "healing-word", "cure-wounds", "fire-bolt",
        "ray-of-frost", "magic-missile", "lay-on-hands", "protection", "produce-flame", "entangle",
        "eldritch-blast", "hex", "vicious-mockery", "bardic-inspiration", "multiattack", "pack-tactics",
        "trip", "smoke-bomb", "blasting-charge", "thorn-burst", "drink-elixir",
    };

    /// <summary>
    /// The icons on the ability sheet that aren't abilities, from ExtrasFrom: the End Turn button,
    /// and Attack for someone with a bow or a crossbow in hand. EXTRAS in the script.
    /// </summary>
    private static readonly string[] Extras = { "end-turn", "ranged-attack" };

    /// <summary>Every condition's icon, in ConditionId's order. CONDITIONS in the script.</summary>
    private static readonly string[] Conditions =
    {
        "downed", "prone", "slowed", "disengaged", "dodging", "restrained", "hexed", "inspired", "stable",
        "helped", "mocked",
    };

    /// <summary>
    /// Every piece of hud.png: its name, its first frame, and how many frames it has, side by side to
    /// the right. HUD in the script.
    /// </summary>
    /// <remarks>
    /// The frames have a black edge, a bevel and a trim round a well FrameInset in: draw the frame,
    /// then the icon or the face in the well. A disabled slot wants its icon drawn greyed as well.
    /// The pips come full, spent (a hollow) and preview (pale, for what the move you're pointing at
    /// would spend). The movement pieces are three-sliced, a three-pixel cap at each end and the
    /// two middle columns stretched: the track first at the bar's length, then the fill as far as
    /// there's movement, then the preview or the over piece, and a tick every tile. The death saves
    /// are 10x10 sockets. The Soul-Link sigil's five states are the story's: gold, the one-frame
    /// violet flicker, violet, broken grey, and grey with gold coming back. The target rings go under
    /// someone's feet and turn over their four frames. The things that fly point right with their
    /// middle at the middle of the frame, for turning to where they're going.
    /// </remarks>
    private static readonly (string Name, Rectangle Source, int Frames)[] Pieces =
    {
        ("hotbar-normal", new Rectangle(0, 0, 30, 30), 1),
        ("hotbar-hover", new Rectangle(30, 0, 30, 30), 1),
        ("hotbar-disabled", new Rectangle(60, 0, 30, 30), 1),
        ("hotbar-selected", new Rectangle(90, 0, 30, 30), 1),
        ("portrait-party", new Rectangle(120, 0, 30, 32), 1),
        ("portrait-foe", new Rectangle(150, 0, 30, 32), 1),
        ("portrait-current", new Rectangle(180, 0, 30, 32), 1),
        ("death-save-empty", new Rectangle(210, 0, 10, 10), 1),
        ("death-save-success", new Rectangle(220, 0, 10, 10), 1),
        ("death-save-failure", new Rectangle(230, 0, 10, 10), 1),
        ("path-dot", new Rectangle(210, 10, 6, 6), 1),
        ("path-dot-far", new Rectangle(216, 10, 6, 6), 1),
        ("soul-link-gold", new Rectangle(0, 32, 24, 24), 1),
        ("soul-link-flicker", new Rectangle(24, 32, 24, 24), 1),
        ("soul-link-violet", new Rectangle(48, 32, 24, 24), 1),
        ("soul-link-broken", new Rectangle(72, 32, 24, 24), 1),
        ("soul-link-pulse", new Rectangle(96, 32, 24, 24), 1),
        ("target-foe", new Rectangle(120, 32, 24, 12), 4),
        ("target-ally", new Rectangle(120, 44, 24, 12), 4),
        ("path-end", new Rectangle(216, 32, 16, 16), 1),
        ("path-end-far", new Rectangle(232, 32, 16, 16), 1),
        ("arrow", new Rectangle(0, 56, 16, 16), 1),
        ("fire-bolt", new Rectangle(16, 56, 16, 16), 4),
        ("frost-ray", new Rectangle(80, 56, 16, 16), 4),
        ("radiant-mote", new Rectangle(144, 56, 16, 16), 4),
        ("eldritch-blast", new Rectangle(0, 72, 16, 16), 4),
        ("magic-missile", new Rectangle(64, 72, 16, 16), 4),
        ("flask", new Rectangle(128, 72, 16, 16), 4),
        ("action", new Rectangle(0, 88, 12, 12), 1),
        ("action-spent", new Rectangle(12, 88, 12, 12), 1),
        ("action-preview", new Rectangle(24, 88, 12, 12), 1),
        ("bonus", new Rectangle(36, 88, 12, 12), 1),
        ("bonus-spent", new Rectangle(48, 88, 12, 12), 1),
        ("bonus-preview", new Rectangle(60, 88, 12, 12), 1),
        ("reaction", new Rectangle(72, 88, 12, 12), 1),
        ("reaction-spent", new Rectangle(84, 88, 12, 12), 1),
        ("reaction-preview", new Rectangle(96, 88, 12, 12), 1),
        ("move-track", new Rectangle(108, 88, 8, 8), 1),
        ("move-fill", new Rectangle(116, 88, 8, 8), 1),
        ("move-preview", new Rectangle(124, 88, 8, 8), 1),
        ("move-over", new Rectangle(132, 88, 8, 8), 1),
        ("move-tick", new Rectangle(140, 88, 2, 8), 1),
    };

    /// <summary>Each ability icon's cell on its sheet, extras included, by name without its hyphens.</summary>
    private static readonly Dictionary<string, int> AbilityCells = new();

    /// <summary>Each condition icon's cell on its sheet, by name without its hyphens.</summary>
    private static readonly Dictionary<string, int> ConditionCells = new();

    /// <summary>Each piece's first frame and how many frames it has, by name without its hyphens.</summary>
    private static readonly Dictionary<string, (Rectangle Source, int Frames)> PieceSources = new();

    /// <summary>Indexes the tables, and checks them against the enums and against each other.</summary>
    /// <remarks>Throws on purpose, so an ability or a condition without its icon, or two pieces drawn over each other, shows up the first time a fight draws.</remarks>
    static CombatIcons()
    {
        CheckOrder(Enum.GetNames<AbilityId>(), Abilities, nameof(AbilityId));
        CheckOrder(Enum.GetNames<ConditionId>(), Conditions, nameof(ConditionId));

        if (Abilities.Length > ExtrasFrom)
            throw new InvalidOperationException($"There are {Abilities.Length} ability icons, which runs into the extras at cell {ExtrasFrom}. Move EXTRAS_FROM in tools/generate_combat_art.py and ExtrasFrom here down a row.");

        for (int i = 0; i < Abilities.Length; i++) Add(AbilityCells, Abilities[i], i, "ability icon");
        for (int i = 0; i < Extras.Length; i++) Add(AbilityCells, Extras[i], ExtrasFrom + i, "ability icon");
        for (int i = 0; i < Conditions.Length; i++) Add(ConditionCells, Conditions[i], i, "condition icon");

        var hud = new Rectangle(0, 0, HudWidth, HudHeight);
        for (int i = 0; i < Pieces.Length; i++)
        {
            (string name, Rectangle source, int frames) = Pieces[i];
            if (frames < 1)
                throw new InvalidOperationException($"The hud piece {name} has {frames} frames.");
            Rectangle strip = Strip(source, frames);
            if (!hud.Contains(strip))
                throw new InvalidOperationException($"The hud piece {name} ({strip}) runs off hud.png, which is {HudWidth}x{HudHeight}.");
            for (int j = 0; j < i; j++)
            {
                if (strip.Intersects(Strip(Pieces[j].Source, Pieces[j].Frames)))
                    throw new InvalidOperationException($"The hud pieces {Pieces[j].Name} and {name} overlap.");
            }
            if (!PieceSources.TryAdd(Key(name), (source, frames)))
                throw new InvalidOperationException($"Two hud pieces are called {name}.");
        }
    }

    /// <summary>An ability's icon.</summary>
    /// <param name="id">Which ability.</param>
    public static Rectangle Ability(AbilityId id) => AbilityCell((int)id);

    /// <summary>An icon on the ability sheet by name: an ability's, or one of the extras ("end-turn", "ranged-attack").</summary>
    /// <remarks>Throws if there's no icon called that; TryAbility doesn't.</remarks>
    /// <param name="name">The name, hyphenated or as the enum spells it.</param>
    public static Rectangle Ability(string name) =>
        TryAbility(name, out Rectangle source) ? source : throw new KeyNotFoundException($"There's no ability icon called {name}.");

    /// <summary>An icon on the ability sheet by name, if there's one called that.</summary>
    /// <param name="name">The name, hyphenated or as the enum spells it.</param>
    /// <param name="source">Where it is on the sheet, or empty if there's none.</param>
    public static bool TryAbility(string name, out Rectangle source)
    {
        bool found = AbilityCells.TryGetValue(Key(name), out int cell);
        source = found ? AbilityCell(cell) : Rectangle.Empty;
        return found;
    }

    /// <summary>A condition's icon.</summary>
    /// <param name="id">Which condition.</param>
    public static Rectangle Condition(ConditionId id) => ConditionCell((int)id);

    /// <summary>A condition's icon by name.</summary>
    /// <remarks>Throws if there's no icon called that.</remarks>
    /// <param name="name">The name, hyphenated or as the enum spells it.</param>
    public static Rectangle Condition(string name) =>
        ConditionCells.TryGetValue(Key(name), out int cell) ? ConditionCell(cell) : throw new KeyNotFoundException($"There's no condition icon called {name}.");

    /// <summary>One frame of a piece of hud.png.</summary>
    /// <remarks>Throws if there's no piece called that.</remarks>
    /// <param name="name">The piece, as named in the table ("hotbar-hover", "target-foe").</param>
    /// <param name="frame">Which frame, counting from 0. It wraps, so a running count animates it.</param>
    public static Rectangle Piece(string name, int frame = 0)
    {
        if (!PieceSources.TryGetValue(Key(name), out (Rectangle Source, int Frames) piece))
            throw new KeyNotFoundException($"There's no hud piece called {name}.");
        int wrapped = ((frame % piece.Frames) + piece.Frames) % piece.Frames;
        return new Rectangle(piece.Source.X + wrapped * piece.Source.Width, piece.Source.Y, piece.Source.Width, piece.Source.Height);
    }

    /// <summary>How many frames a piece of hud.png has.</summary>
    /// <remarks>Throws if there's no piece called that.</remarks>
    /// <param name="name">The piece, as named in the table.</param>
    public static int Frames(string name) =>
        PieceSources.TryGetValue(Key(name), out (Rectangle Source, int Frames) piece) ? piece.Frames : throw new KeyNotFoundException($"There's no hud piece called {name}.");

    /// <summary>Checks a loaded sheet is the size these tables say it is.</summary>
    /// <remarks>Throws if it isn't. That means the sheet was drawn by an older or newer script than this file, and I'd rather find out when it loads than draw the wrong icons.</remarks>
    /// <param name="asset">Which sheet: AbilitySheet, ConditionSheet or HudSheet.</param>
    /// <param name="sheet">The sheet as loaded.</param>
    public static void CheckSheet(string asset, Texture2D sheet)
    {
        int abilityRows = (ExtrasFrom + Extras.Length + AbilityColumns - 1) / AbilityColumns;
        Point expected = asset switch
        {
            AbilitySheet => new Point(AbilityColumns * AbilitySize, abilityRows * AbilitySize),
            ConditionSheet => new Point(Conditions.Length * ConditionSize, ConditionSize),
            HudSheet => new Point(HudWidth, HudHeight),
            _ => throw new ArgumentException($"{asset} isn't one of the combat sheets.", nameof(asset)),
        };
        if (sheet.Width != expected.X || sheet.Height != expected.Y)
            throw new InvalidOperationException($"{asset}.png is {sheet.Width}x{sheet.Height}; CombatIcons expects {expected.X}x{expected.Y}. Run tools/generate_combat_art.py.");
    }

    /// <summary>Checks a table of names is in an enum's order, one for one.</summary>
    private static void CheckOrder(string[] enumNames, string[] names, string enumName)
    {
        for (int i = 0; i < Math.Max(enumNames.Length, names.Length); i++)
        {
            string wanted = i < enumNames.Length ? enumNames[i] : null;
            string have = i < names.Length ? names[i] : null;
            if (wanted == null)
                throw new InvalidOperationException($"CombatIcons has an icon called {have} at {i}, past the end of {enumName}.");
            if (have == null || Key(wanted) != Key(have))
                throw new InvalidOperationException($"{enumName}.{wanted} is number {i}, but CombatIcons has {have ?? "nothing"} there. Draw its icon in tools/generate_combat_art.py and put its name here in the same place.");
        }
    }

    /// <summary>Indexes one name, and throws if it's there already.</summary>
    private static void Add(Dictionary<string, int> cells, string name, int cell, string what)
    {
        if (!cells.TryAdd(Key(name), cell)) throw new InvalidOperationException($"Two {what}s are called {name}.");
    }

    /// <summary>A name as the tables are keyed: no hyphens, lower case.</summary>
    private static string Key(string name) => (name ?? string.Empty).Replace("-", string.Empty).ToLowerInvariant();

    /// <summary>A cell on the ability sheet.</summary>
    private static Rectangle AbilityCell(int cell) =>
        new(cell % AbilityColumns * AbilitySize, cell / AbilityColumns * AbilitySize, AbilitySize, AbilitySize);

    /// <summary>A cell on the condition sheet.</summary>
    private static Rectangle ConditionCell(int cell) => new(cell * ConditionSize, 0, ConditionSize, ConditionSize);

    /// <summary>The whole strip of a piece's frames, for checking it fits and doesn't overlap another.</summary>
    private static Rectangle Strip(Rectangle first, int frames) => new(first.X, first.Y, first.Width * frames, first.Height);
}
