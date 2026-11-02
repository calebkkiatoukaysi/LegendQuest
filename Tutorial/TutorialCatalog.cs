using System;
using System.Collections.Generic;
using LegendQuest.Components;

namespace LegendQuest.Tutorial;

/// <summary>
/// Every tutorial tip, in the order HOW TO PLAY lists them, and the ids the game calls them by.
/// </summary>
/// <remarks>
/// A static table like the class catalog, checked when the game starts, so a tip with no words, two
/// tips with one id, or one too long to read at a glance stops the game there instead of turning up
/// as a wall of text mid-play. The combat and inventory tips are written now so the screens that
/// show them only have to call them. A few are only ever read in HOW TO PLAY: the conversation ones,
/// since a conversation has the whole screen and a card can't go over it, and the ones about
/// things there's no single moment for. Only the characters the fonts have: plain ASCII and the
/// middle dot.
/// </remarks>
public static class TutorialCatalog
{
    /// <summary>Walking, by the keys or the mouse. The first moment you have control.</summary>
    public const string Move = "explore-move";

    /// <summary>The mouse wheel. Only in HOW TO PLAY.</summary>
    public const string Zoom = "explore-zoom";

    /// <summary>Chests, doors, levers and signs. The first time one's in reach.</summary>
    public const string Use = "explore-use";

    /// <summary>Walking into a building. The first time you're by a door into one.</summary>
    public const string Enter = "explore-enter";

    /// <summary>M for the map. The first time you're somewhere that's on the world map.</summary>
    public const string Map = "explore-map";

    /// <summary>Waking a waypoint stone and travelling between them. The first time one's in reach.</summary>
    public const string Waypoint = "explore-waypoint";

    /// <summary>Where every control is. Once you've had a handful of tips.</summary>
    public const string Help = "explore-help";

    /// <summary>E or a click to talk. The first time someone's in reach.</summary>
    public const string Talk = "explore-talk";

    /// <summary>Going on and answering in a conversation. Only in HOW TO PLAY, since a conversation has the screen.</summary>
    public const string Choices = "talk-choices";

    /// <summary>The d20 behind a risky answer. Only in HOW TO PLAY, for the same reason.</summary>
    public const string Checks = "talk-checks";

    /// <summary>The red "!". The first time there's one.</summary>
    public const string Event = "explore-event";

    /// <summary>The gold "!" over someone not in the party. The first time there's one.</summary>
    public const string Work = "explore-work";

    /// <summary>The line under the map's name. The first time it changes.</summary>
    public const string Objective = "explore-objective";

    /// <summary>The Lantern Market's quest board. The first time you're in reach of it.</summary>
    public const string Board = "explore-board";

    /// <summary>Tab and F1 to F4. Once there are two of you and you've walked a while.</summary>
    public const string Party = "explore-party";

    /// <summary>One of the party with something to say. The first time one has.</summary>
    public const string Companion = "explore-companion";

    /// <summary>Approval. Only in HOW TO PLAY: the toasts already say when it moves.</summary>
    public const string Approval = "party-approval";

    /// <summary>Swapping with whoever's waiting at the Gilded Flagon. The first time anyone's waiting there.</summary>
    public const string Camp = "explore-camp";

    /// <summary>A fight's turns. The combat screen shows it when the first fight starts.</summary>
    public const string CombatStart = "combat-start";

    /// <summary>Moving on your turn. The combat screen shows it on your first turn.</summary>
    public const string CombatMove = "combat-move";

    /// <summary>Attacking. The combat screen shows it the first time an enemy's in reach.</summary>
    public const string CombatAttack = "combat-attack";

    /// <summary>The hotbar. The combat screen shows it the first time you've an ability to use.</summary>
    public const string CombatAbility = "combat-ability";

    /// <summary>The action and the bonus action. The combat screen shows it the first time you spend one.</summary>
    public const string CombatBonus = "combat-bonus";

    /// <summary>Ending your turn. The combat screen shows it once you've moved and acted.</summary>
    public const string CombatEndTurn = "combat-end-turn";

    /// <summary>The Soul-Link. The combat screen shows it the first time the sigil charges.</summary>
    public const string CombatSoulLink = "combat-soul-link";

    /// <summary>Being Downed and helping someone up. The combat screen shows it the first time one of the party goes down.</summary>
    public const string CombatDowned = "combat-downed";

    /// <summary>Winning. The combat screen shows it after the first fight that's won.</summary>
    public const string CombatVictory = "combat-victory";

    /// <summary>I for the bag and C for the sheet. Shown the next time something goes in the bag after the loot tip (another chest, or buying something), unless the bag's been opened by then.</summary>
    public const string InventoryOpen = "inventory-open";

    /// <summary>Putting gear on. The inventory shows it the first time it's open.</summary>
    public const string InventoryEquip = "inventory-equip";

    /// <summary>Potions. The inventory shows it the first time it's open with one in the bag.</summary>
    public const string InventoryPotion = "inventory-potion";

    /// <summary>Loot going in the bag. The map shows it the first time anything comes out of a chest.</summary>
    public const string Loot = "loot";

    /// <summary>The most letters a title or a body can have, so every tip reads at a glance: a title fits one row of the card, and a body three.</summary>
    private const int LongestTitle = 26;
    private const int LongestBody = 120;

    /// <summary>The most letters on one key cap, so a row of them fits under a body.</summary>
    private const int LongestKey = 12;

    /// <summary>The most key caps one tip shows.</summary>
    private const int MostKeys = 6;

    /// <summary>Every id the game calls by name. Each one has to have a row, so a tip renamed in the table and not here shows up when the game starts.</summary>
    private static readonly string[] Called =
    {
        Move, Zoom, Use, Enter, Map, Waypoint, Help, Talk, Choices, Checks, Event, Work, Objective, Board,
        Party, Companion, Approval, Camp, CombatStart, CombatMove, CombatAttack, CombatAbility, CombatBonus,
        CombatEndTurn, CombatSoulLink, CombatDowned, CombatVictory, InventoryOpen, InventoryEquip, InventoryPotion, Loot,
    };

    private static readonly TutorialTip[] Table =
    {
        // Walking about.
        new()
        {
            Id = Move, Topic = TipTopic.Exploring, Title = "Finding your feet",
            Body = "Walk with WASD or the arrows, or click the ground to walk there. The dotted line shows the way first.",
            Keys = new[] { "W", "A", "S", "D", KeyCaps.Click },
        },
        new()
        {
            Id = Zoom, Topic = TipTopic.Exploring, Title = "A closer look",
            Body = "The mouse wheel zooms the world in and out, a whole step at a time.",
            Keys = new[] { KeyCaps.Wheel },
        },
        new()
        {
            Id = Use, Topic = TipTopic.Exploring, Title = "Chests, doors and levers",
            Body = "When something's name comes up over it, E opens, pulls or reads it. Click it and you'll walk over to do the same.",
            Keys = new[] { "E", KeyCaps.Click },
        },
        new()
        {
            Id = Enter, Topic = TipTopic.Exploring, Title = "Going inside",
            Body = "Walk into a building's door to go in, and back out through it to leave.",
        },
        new()
        {
            Id = Map, Topic = TipTopic.Exploring, Title = "The map", Anchor = TipAnchor.TopRight,
            Body = "M opens the map: all you've seen of this place, and the world beyond it. Tab turns between the two.",
            Keys = new[] { "M", "TAB" },
        },
        new()
        {
            Id = Waypoint, Topic = TipTopic.Exploring, Title = "Waypoint stones",
            Body = "Use a waypoint stone to wake it. From any stone you've woken, you can travel to any other.",
            Keys = new[] { "E" },
        },
        new()
        {
            Id = Help, Topic = TipTopic.Exploring, Title = "Every control", Anchor = TipAnchor.TopRight,
            Body = "Forgotten a key? Press Esc and pick HOW TO PLAY. Every control and every tip is there.",
            Keys = new[] { "ESC" },
        },

        // Talking and the story.
        new()
        {
            Id = Talk, Topic = TipTopic.Story, Title = "Someone to talk to",
            Body = "When a name and E · TALK come up over someone, press E to talk to them. Or click them from anywhere.",
            Keys = new[] { "E", KeyCaps.Click },
        },
        new()
        {
            Id = Choices, Topic = TipTopic.Story, Title = "Saying your piece",
            Body = "Enter, Space or a click carries a conversation on. At a question, answer with 1 to 4, or click one.",
            Keys = new[] { "ENTER", "1-4", KeyCaps.Click },
        },
        new()
        {
            Id = Checks, Topic = TipTopic.Story, Title = "The dice",
            Body = "An answer with a die by it is a risk: a d20, plus what you're good at, against a number. Space rolls.",
            Keys = new[] { "SPACE" },
        },
        new()
        {
            Id = Event, Topic = TipTopic.Story, Title = "A red mark",
            Body = "A red ! is something about to happen. Walk up to it and it starts. An arrow at the edge points to one out of sight.",
        },
        new()
        {
            Id = Work, Topic = TipTopic.Story, Title = "A gold mark",
            Body = "A gold ! is someone with work for you, or waiting to hear how it went. Talk to them.",
            Keys = new[] { "E" },
        },
        new()
        {
            Id = Objective, Topic = TipTopic.Story, Title = "What's next", Anchor = TipAnchor.TopLeft,
            Body = "The gold line under the place's name is what you're to do next. It moves on as the story does.",
        },
        new()
        {
            Id = Board, Topic = TipTopic.Story, Title = "The quest board",
            Body = "Anyone with a problem and a coin posts it here. Read the board, or ask Fennick beside it.",
            Keys = new[] { "E" },
        },

        // The party.
        new()
        {
            Id = Party, Topic = TipTopic.Party, Title = "Your party", Anchor = TipAnchor.AboveStrip,
            Body = "Tab changes who you're controlling, or F1 to F4 picks one. The rest follow. Clicking a card works too.",
            Keys = new[] { "TAB", "F1-F4", KeyCaps.Click },
        },
        new()
        {
            Id = Companion, Topic = TipTopic.Party, Title = "Something to say", Anchor = TipAnchor.AboveStrip,
            Body = "A gold ! over one of the party means they've something to say. Press E beside them, or click them.",
            Keys = new[] { "E", KeyCaps.Click },
        },
        new()
        {
            Id = Approval, Topic = TipTopic.Party, Title = "What they think",
            Body = "Your companions notice what you say and do. When one approves or disapproves, you'll hear about it.",
        },
        new()
        {
            Id = Camp, Topic = TipTopic.Party, Title = "Waiting at the Flagon",
            Body = "Four can travel together. Anyone else waits here: talk to them to swap them in for someone with you.",
            Keys = new[] { "E" },
        },

        // Fighting.
        new()
        {
            Id = CombatStart, Topic = TipTopic.Combat, Title = "A fight", Anchor = TipAnchor.Centre, Blocking = true,
            Body = "Everyone takes turns, in the order along the top. On yours, move, act, then end your turn.",
        },
        new()
        {
            Id = CombatMove, Topic = TipTopic.Combat, Title = "Moving in a fight", Anchor = TipAnchor.Right,
            Body = "Click where to go. The path shows what it costs in metres, and turns red past what you've got left.",
            Keys = new[] { KeyCaps.Click, "W", "A", "S", "D" },
        },
        new()
        {
            Id = CombatAttack, Topic = TipTopic.Combat, Title = "Attacking", Anchor = TipAnchor.Right,
            Body = "Click an enemy to attack. Point at one first to see your chance to hit and the damage you'd do.",
            Keys = new[] { KeyCaps.Click },
        },
        new()
        {
            Id = CombatAbility, Topic = TipTopic.Combat, Title = "Abilities and spells", Anchor = TipAnchor.Right,
            Body = "Your skills and spells are on the hotbar. Press 1 to 0 or click one, then a target. Right click cancels.",
            Keys = new[] { "1-0", KeyCaps.Click, KeyCaps.RightClick },
        },
        new()
        {
            Id = CombatBonus, Topic = TipTopic.Combat, Title = "Action and bonus action", Anchor = TipAnchor.Right,
            Body = "Each turn you get one action, the circle, and one bonus action, the triangle. Some things only need the bonus.",
        },
        new()
        {
            Id = CombatEndTurn, Topic = TipTopic.Combat, Title = "Ending your turn", Anchor = TipAnchor.Right,
            Body = "Done? Space or END TURN passes to whoever's next. Whatever you didn't use, you lose.",
            Keys = new[] { "SPACE" },
        },
        new()
        {
            Id = CombatSoulLink, Topic = TipTopic.Combat, Title = "The Soul-Link", Anchor = TipAnchor.Right,
            Body = "Hit the same foe as the Soul-Bound between your turns and the sigil fills. Full, the two of you strike as one.",
        },
        new()
        {
            Id = CombatDowned, Topic = TipTopic.Combat, Title = "Downed", Anchor = TipAnchor.Right,
            Body = "At 0 hit points a friend goes down, and rolls against death each turn. Help them up, or heal them.",
        },
        new()
        {
            Id = CombatVictory, Topic = TipTopic.Combat, Title = "The fight's won", Anchor = TipAnchor.Right,
            Body = "Everyone who fought shares the experience, and whatever the fallen carried is yours to take.",
        },

        // Gear.
        new()
        {
            Id = InventoryOpen, Topic = TipTopic.Gear, Title = "The bag",
            Body = "I opens the party's bag, and C the character sheet: what everyone's wearing, and how they're doing.",
            Keys = new[] { "I", "C" },
        },
        new()
        {
            Id = InventoryEquip, Topic = TipTopic.Gear, Title = "Wearing it",
            Body = "Click a piece of gear to put it on whoever you've picked. What they wear shows on them as they walk.",
            Keys = new[] { KeyCaps.Click },
        },
        new()
        {
            Id = InventoryPotion, Topic = TipTopic.Gear, Title = "Potions",
            Body = "Drink a healing potion from the bag, or in a fight as a bonus action. It's worth keeping a couple.",
        },
        new()
        {
            Id = Loot, Topic = TipTopic.Gear, Title = "Something for you",
            Body = "Whatever you find goes in the party's bag. Press I to look through it.",
            Keys = new[] { "I" },
        },
    };

    private static readonly Dictionary<string, TutorialTip> ById = new();

    /// <summary>Indexes the table, and checks every tip has its words, reads at a glance, and is called by an id that's here.</summary>
    /// <remarks>Throws on purpose, so a bad row shows up when the game starts.</remarks>
    static TutorialCatalog()
    {
        foreach (TutorialTip tip in Table)
        {
            if (string.IsNullOrWhiteSpace(tip.Title) || string.IsNullOrWhiteSpace(tip.Body))
                throw new InvalidOperationException($"Tutorial tip {tip.Id} has no title or no words.");
            if (tip.Title.Length > LongestTitle)
                throw new InvalidOperationException($"Tutorial tip {tip.Id}'s title is {tip.Title.Length} letters, past the {LongestTitle} that fit on its card.");
            if (tip.Body.Length > LongestBody)
                throw new InvalidOperationException($"Tutorial tip {tip.Id} is {tip.Body.Length} letters long, past the {LongestBody} that read at a glance. Split it, or say it shorter.");
            if (tip.Keys.Count > MostKeys)
                throw new InvalidOperationException($"Tutorial tip {tip.Id} shows {tip.Keys.Count} keys, past the {MostKeys} that fit under it.");
            foreach (string key in tip.Keys)
            {
                if (string.IsNullOrWhiteSpace(key) || key.Length > LongestKey)
                    throw new InvalidOperationException($"Tutorial tip {tip.Id} has a key cap \"{key}\", which is empty or longer than {LongestKey} letters.");
            }
            if (!ById.TryAdd(tip.Id, tip)) throw new InvalidOperationException($"Two tutorial tips are called {tip.Id}.");
        }

        foreach (string id in Called)
            if (!ById.ContainsKey(id)) throw new InvalidOperationException($"Tutorial tip {id} is called by the game and isn't in TutorialCatalog.");
        if (ById.Count != Called.Length)
            throw new InvalidOperationException($"TutorialCatalog has {ById.Count} tips and {Called.Length} ids to call them by; one of them has no id.");
    }

    /// <summary>Every tip, in the order HOW TO PLAY lists them.</summary>
    public static IReadOnlyList<TutorialTip> All => Table;

    /// <summary>Whether there's a tip called this.</summary>
    /// <param name="id">Its id.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One tip.</summary>
    /// <param name="id">Its id.</param>
    /// <exception cref="ArgumentException">There isn't one, which means a typo where it's called.</exception>
    public static TutorialTip Get(string id) =>
        id is not null && ById.TryGetValue(id, out TutorialTip tip) ? tip : throw new ArgumentException($"There's no tutorial tip called {id}.", nameof(id));
}
