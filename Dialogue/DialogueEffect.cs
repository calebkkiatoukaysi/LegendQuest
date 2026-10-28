using System;
using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue;

/// <summary>
/// Something a line or an option does to the game: a flag, a meter, gold, an item, what to do
/// next, someone joining, your voice, or somewhere to go.
/// </summary>
/// <param name="Kind">What it does.</param>
/// <param name="Id">The flag, meter, item, text, origin, voice or "map spawn" it's about.</param>
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

    /// <summary>What you're to do next.</summary>
    public static DialogueEffect Objective(string text) => new(EffectKind.Objective, text);

    /// <summary>Picks your voice.</summary>
    public static DialogueEffect Speak(Voice voice) => new(EffectKind.Voice, voice.ToString());

    /// <summary>Takes the party to a map, onto one of its spawns, once the conversation's over.</summary>
    public static DialogueEffect Travel(string map, string spawn) => new(EffectKind.Travel, map + " " + spawn);

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
        _ => null,
    };
}
