using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Items;
using LegendQuest.Save;
using LegendQuest.Story;

namespace LegendQuest.Dialogue;

/// <summary>
/// Something a line or an option does to the game: a flag, a meter, gold, an item, what to do
/// next, someone joining, going away or coming back, your voice, somewhere to go, a shop to open,
/// or a fight to start.
/// </summary>
/// <param name="Kind">What it does.</param>
/// <param name="Id">The flag, meter, item, text, origin, voice, "map spawn", "waiting member", shop or encounter it's about.</param>
/// <param name="Amount">How much, for meters and gold.</param>
public readonly record struct DialogueEffect(EffectKind Kind, string Id, int Amount = 0)
{
    /// <summary>What has to hold, when its line is said or its option picked, for it to happen at all. Null for always.</summary>
    /// <remarks>
    /// So one effect in a list can depend on the game without a line of its own: rejoin valencia's
    /// approval only lands if you claimed her. It's checked as it comes, so an effect before it in the
    /// same list can be what makes it hold.
    /// </remarks>
    public IReadOnlyList<DialogueCondition> Conditions { get; init; }

    /// <summary>Sets a flag.</summary>
    public static DialogueEffect Set(string flag) => new(EffectKind.SetFlag, flag);

    /// <summary>Clears a flag.</summary>
    public static DialogueEffect Clear(string flag) => new(EffectKind.ClearFlag, flag);

    /// <summary>Adds to a meter, or takes from it.</summary>
    public static DialogueEffect Add(string meter, int amount) => new(EffectKind.AddMeter, meter, amount);

    /// <summary>Hands over gold.</summary>
    public static DialogueEffect Gold(int amount) => new(EffectKind.Gold, "gold", amount);

    /// <summary>Hands over a key item.</summary>
    public static DialogueEffect Give(string item) => new(EffectKind.GiveItem, item);

    /// <summary>Takes a key item back, for one that changes hands.</summary>
    public static DialogueEffect Take(string item) => new(EffectKind.TakeItem, item);

    /// <summary>Sets a meter outright, rather than adding to it.</summary>
    public static DialogueEffect SetMeter(string meter, int value) => new(EffectKind.SetMeter, meter, value);

    /// <summary>Someone joins: into the party if there's room, to wait at camp if not.</summary>
    public static DialogueEffect Recruit(string origin) => new(EffectKind.Recruit, origin);

    /// <summary>Someone waiting at camp takes a companion's place in the party, and the companion waits in their seat.</summary>
    /// <param name="waiting">Who's waiting, by origin id.</param>
    /// <param name="member">Whose place they take, by origin id. Never you or the Soul-Bound.</param>
    public static DialogueEffect Swap(string waiting, string member) => new(EffectKind.Swap, waiting + " " + member);

    /// <summary>Someone goes away for a while, out of the party or the camp, until a Rejoin brings them back.</summary>
    public static DialogueEffect Leave(string origin) => new(EffectKind.Leave, origin);

    /// <summary>Someone who went away comes back, into the party even if it's full.</summary>
    public static DialogueEffect Rejoin(string origin) => new(EffectKind.Rejoin, origin);

    /// <summary>What you're to do next.</summary>
    public static DialogueEffect Objective(string text) => new(EffectKind.Objective, text);

    /// <summary>Picks your voice.</summary>
    public static DialogueEffect Speak(Voice voice) => new(EffectKind.Voice, voice.ToString());

    /// <summary>Takes the party to a map, onto one of its spawns, once the conversation's over.</summary>
    public static DialogueEffect Travel(string map, string spawn) => new(EffectKind.Travel, map + " " + spawn);

    /// <summary>Opens a shop once the conversation's over, the way a shopkeeper's "let's see what you've got" does.</summary>
    /// <param name="shop">The shop, by its id in ShopCatalog.</param>
    public static DialogueEffect Shop(string shop) => new(EffectKind.Shop, shop);

    /// <summary>Starts a fight once the conversation's over, on the map it's on, the way a card used to say "they attack".</summary>
    /// <param name="encounter">The fight, by its id in EncounterCatalog.</param>
    public static DialogueEffect Fight(string encounter) => new(EffectKind.Fight, encounter);

    /// <summary>A long rest for everyone: the night at camp or in a bed at the inn. Nothing else gives one yet; short rests come with the camp screen.</summary>
    public static DialogueEffect Rest() => new(EffectKind.Rest, nameof(RestKind.Long));

    /// <summary>The same effect, happening only if all of these hold as well as any it had already.</summary>
    /// <param name="conditions">What has to hold.</param>
    public DialogueEffect If(params DialogueCondition[] conditions) =>
        this with { Conditions = (Conditions ?? Array.Empty<DialogueCondition>()).Concat(conditions).ToArray() };

    /// <summary>Whether it happens in a game as it stands: all of its Conditions hold, or it has none.</summary>
    /// <param name="game">The game.</param>
    public bool Applies(SaveData game)
    {
        if (Conditions is null) return true;
        foreach (DialogueCondition condition in Conditions)
            if (!condition.Holds(game)) return false;
        return true;
    }

    /// <summary>What's wrong with it, or null if it's well formed: its conditions first, then what it does.</summary>
    /// <remarks>Where a Travel goes gets checked by DialogueCatalog, which runs after the maps have all been read.</remarks>
    public string Problem()
    {
        foreach (DialogueCondition condition in Conditions ?? Array.Empty<DialogueCondition>())
            if (condition.Problem() is string problem) return $"only happens on a condition that {problem}";

        return Kind switch
        {
            _ when string.IsNullOrWhiteSpace(Id) => "names nothing",
            EffectKind.AddMeter or EffectKind.SetMeter when !Meters.Exists(Id) => $"names a meter, {Id}, that doesn't exist",
            EffectKind.GiveItem or EffectKind.TakeItem when !KeyItems.Exists(Id) => $"names an item, {Id}, that doesn't exist",
            EffectKind.Recruit when !Origins.Exists(Id) => $"recruits {Id}, who has no origin",
            EffectKind.Swap when Id.Split(' ') is not [_, _] => $"swaps '{Id}', which isn't who's waiting and whose place they take",
            EffectKind.Swap when Id.Split(' ') is [string waiting, string member] && (!Origins.Exists(waiting) || !Origins.Exists(member)) => $"swaps '{Id}', and one of them has no origin",
            EffectKind.Swap when Id.Split(' ') is [_, Origins.You or Origins.SoulBound] => $"swaps '{Id}', and the story's about the two of you",
            EffectKind.Leave when !Origins.Exists(Id) => $"sends {Id} away, who has no origin",
            EffectKind.Leave when Id == Origins.You => "sends you away, and the party's yours",
            EffectKind.Rejoin when !Origins.Exists(Id) => $"brings {Id} back, who has no origin",
            EffectKind.Voice when !Enum.TryParse(Id, out Voice voice) || voice == LegendQuest.Voice.None => $"names a voice, {Id}, that can't be picked",
            EffectKind.Travel when Id.Split(' ').Length != 2 => $"travels to '{Id}', which isn't a map and a spawn",
            EffectKind.Shop when !ShopCatalog.Exists(Id) => $"opens a shop, {Id}, that doesn't exist",
            EffectKind.Fight when !EncounterCatalog.Exists(Id) => $"starts a fight, {Id}, that isn't in EncounterCatalog",
            _ => null,
        };
    }
}
