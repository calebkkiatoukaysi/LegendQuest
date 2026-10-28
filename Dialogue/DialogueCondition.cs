using System;
using LegendQuest.Save;
using LegendQuest.Story;

namespace LegendQuest.Dialogue;

/// <summary>
/// Something that has to be true for a line to be said or an option to be open: a flag, a meter,
/// your voice, who's in the party, or your class.
/// </summary>
/// <param name="Kind">What it looks at.</param>
/// <param name="Id">The flag, meter, voice, party member or class it's about, by name.</param>
/// <param name="Amount">The number a meter is checked against. Unused otherwise.</param>
public readonly record struct DialogueCondition(ConditionKind Kind, string Id, int Amount = 0)
{
    /// <summary>You talk in this voice.</summary>
    public static DialogueCondition Speaks(Voice voice) => new(ConditionKind.Voice, voice.ToString());

    /// <summary>You're this class.</summary>
    public static DialogueCondition IsClass(ClassId id) => new(ConditionKind.Class, id.ToString());

    /// <summary>Whether it's true in a game.</summary>
    /// <param name="game">The game.</param>
    public bool Holds(SaveData game) => Kind switch
    {
        ConditionKind.Flag => game.Flags.Contains(Id),
        ConditionKind.NotFlag => !game.Flags.Contains(Id),
        ConditionKind.MeterAtLeast => game.Meter(Id) >= Amount,
        ConditionKind.MeterBelow => game.Meter(Id) < Amount,
        ConditionKind.Voice => game.Voice.ToString() == Id,
        ConditionKind.InParty => game.InParty(Id),
        _ => game.You.Class.ToString() == Id,
    };

    /// <summary>What a greyed-out option says it needs, in capitals.</summary>
    public string Reason => Kind switch
    {
        ConditionKind.Voice or ConditionKind.Class => Id.ToUpperInvariant(),
        ConditionKind.MeterAtLeast => Id.ToUpperInvariant() + " " + Amount,
        _ => "LOCKED",
    };

    /// <summary>What's wrong with it, or null if it's well formed.</summary>
    public string Problem() => Kind switch
    {
        _ when string.IsNullOrWhiteSpace(Id) => "names nothing",
        ConditionKind.MeterAtLeast or ConditionKind.MeterBelow when !Meters.Exists(Id) => $"names a meter, {Id}, that doesn't exist",
        ConditionKind.Voice when !Enum.TryParse(Id, out Voice _) => $"names a voice, {Id}, that doesn't exist",
        ConditionKind.Class when !Enum.TryParse(Id, out ClassId _) => $"names a class, {Id}, that doesn't exist",
        _ => null,
    };
}
