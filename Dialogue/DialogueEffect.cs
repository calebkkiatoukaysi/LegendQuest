using System;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Items;
using LegendQuest.Story;

namespace LegendQuest.Dialogue;

/// <summary>
/// Something a line or an option does to the game: a flag, a meter, gold, an item, what to do
/// next, someone joining, your voice, somewhere to go, a shop to open, or a fight to start.
/// </summary>
/// <param name="Kind">What it does.</param>
/// <param name="Id">The flag, meter, item, text, origin, voice, "map spawn", shop or encounter it's about.</param>
/// <param name="Amount">How much, for meters and gold.</param>
public readonly record struct DialogueEffect(EffectKind Kind, string Id, int Amount = 0)
{
    /// <summary>Sets a flag.</summary>
    public static DialogueEffect Set(string flag) => new(EffectKind.SetFlag, flag);

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

    /// <summary>What's wrong with it, or null if it's well formed.</summary>
    /// <remarks>Where a Travel goes gets checked by DialogueCatalog, which runs after the maps have all been read.</remarks>
    public string Problem() => Kind switch
    {
        _ when string.IsNullOrWhiteSpace(Id) => "names nothing",
        EffectKind.AddMeter or EffectKind.SetMeter when !Meters.Exists(Id) => $"names a meter, {Id}, that doesn't exist",
        EffectKind.GiveItem or EffectKind.TakeItem when !KeyItems.Exists(Id) => $"names an item, {Id}, that doesn't exist",
        EffectKind.Recruit when !Origins.Exists(Id) => $"recruits {Id}, who has no origin",
        EffectKind.Voice when !Enum.TryParse(Id, out Voice voice) || voice == LegendQuest.Voice.None => $"names a voice, {Id}, that can't be picked",
        EffectKind.Travel when Id.Split(' ').Length != 2 => $"travels to '{Id}', which isn't a map and a spawn",
        EffectKind.Shop when !ShopCatalog.Exists(Id) => $"opens a shop, {Id}, that doesn't exist",
        EffectKind.Fight when !EncounterCatalog.Exists(Id) => $"starts a fight, {Id}, that isn't in EncounterCatalog",
        _ => null,
    };
}
