using System;
using LegendQuest.Characters;
using LegendQuest.Save;
using LegendQuest.Story;

namespace LegendQuest.Dialogue;

/// <summary>
/// Something that has to be true for a line to be said or an option to be open: a flag, a meter or
/// two, your voice, who's in the party, your class, the act, what's in the bag, or any one of
/// several of those.
/// </summary>
/// <param name="Kind">What it looks at.</param>
/// <param name="Id">The flag, meter, voice, party member, class or key item it's about, by name. For a comparison, the meter that has to be over the other.</param>
/// <param name="Amount">The number a meter or the act is checked against, or for a comparison how far over the other meter it has to be: 1 for over, 0 for at or over. Unused otherwise.</param>
public readonly record struct DialogueCondition(ConditionKind Kind, string Id, int Amount = 0)
{
    /// <summary>For a comparison, the meter Id is checked against. Null otherwise.</summary>
    public string Against { get; init; }

    /// <summary>For AnyOf, the conditions it needs one of. Null otherwise.</summary>
    public DialogueCondition[] Children { get; init; }

    /// <summary>A flag is set.</summary>
    public static DialogueCondition Is(string flag) => new(ConditionKind.Flag, flag);

    /// <summary>A flag isn't set.</summary>
    public static DialogueCondition Not(string flag) => new(ConditionKind.NotFlag, flag);

    /// <summary>A meter is at this or over it.</summary>
    public static DialogueCondition AtLeast(string meter, int amount) => new(ConditionKind.MeterAtLeast, meter, amount);

    /// <summary>A meter is under this.</summary>
    public static DialogueCondition Below(string meter, int amount) => new(ConditionKind.MeterBelow, meter, amount);

    /// <summary>Someone's in the party (not waiting at camp, and not away).</summary>
    public static DialogueCondition With(string origin) => new(ConditionKind.InParty, origin);

    /// <summary>Someone isn't in the party: waiting at camp, away, or not recruited at all.</summary>
    public static DialogueCondition Without(string origin) => new(ConditionKind.NotInParty, origin);

    /// <summary>You talk in this voice.</summary>
    public static DialogueCondition Speaks(Voice voice) => new(ConditionKind.Voice, voice.ToString());

    /// <summary>You're this class.</summary>
    public static DialogueCondition IsClass(ClassId id) => new(ConditionKind.Class, id.ToString());

    /// <summary>The story's in this act, as the act meter has it: the act's card sets it.</summary>
    public static DialogueCondition InAct(int act) => new(ConditionKind.Act, Meters.Act, act);

    /// <summary>A key item's in the bag.</summary>
    public static DialogueCondition Has(string item) => new(ConditionKind.HasItem, item);

    /// <summary>A key item isn't in the bag.</summary>
    public static DialogueCondition Lacks(string item) => new(ConditionKind.LacksItem, item);

    /// <summary>One meter is over another ("a>b") or at it or over it ("a>=b"), the way Kael's loyalty to the Soul-Bound is weighed against his to you.</summary>
    /// <remarks>Read once, here. Anything that isn't one meter, the sign and another meter is kept whole as the Id with nothing to compare against, so Problem names it when the game starts.</remarks>
    /// <param name="comparison">Two meters' ids with &gt; or &gt;= between them, and no spaces.</param>
    public static DialogueCondition Compare(string comparison)
    {
        int sign = comparison?.IndexOf('>') ?? -1;
        if (sign < 0) return new(ConditionKind.MeterCompare, comparison);

        bool orEqual = sign + 1 < comparison.Length && comparison[sign + 1] == '=';
        string right = comparison[(sign + (orEqual ? 2 : 1))..];
        return new(ConditionKind.MeterCompare, comparison[..sign], orEqual ? 0 : 1) { Against = right };
    }

    /// <summary>Any one of these is true. An option locked behind it says what the first one needs.</summary>
    public static DialogueCondition AnyOf(params DialogueCondition[] conditions) => new(ConditionKind.AnyOf, null) { Children = conditions };

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
        ConditionKind.Class => game.You.Class.ToString() == Id,
        ConditionKind.NotInParty => !game.InParty(Id),
        ConditionKind.MeterCompare => game.Meter(Id) - game.Meter(Against) >= Amount,
        ConditionKind.Act => game.Meter(Meters.Act) == Amount,
        ConditionKind.HasItem => game.Items.Contains(Id),
        ConditionKind.LacksItem => !game.Items.Contains(Id),
        ConditionKind.AnyOf => AnyHolds(game),
        _ => throw new InvalidOperationException($"A condition can't look at {Kind}."),
    };

    /// <summary>What a greyed-out option says it needs, in capitals.</summary>
    /// <remarks>A comparison only says it's locked: the meters it weighs are the hidden ones.</remarks>
    public string Reason => Kind switch
    {
        ConditionKind.Voice or ConditionKind.Class => Id.ToUpperInvariant(),
        ConditionKind.MeterAtLeast => Id.ToUpperInvariant() + " " + Amount,
        ConditionKind.Act => "ACT " + Amount,
        ConditionKind.HasItem when KeyItems.Exists(Id) => KeyItems.Get(Id).Name.ToUpperInvariant(),
        ConditionKind.AnyOf when Children is { Length: > 0 } => Children[0].Reason,
        _ => "LOCKED",
    };

    /// <summary>What's wrong with it, or null if it's well formed.</summary>
    /// <remarks>AnyOf has no Id of its own, so it's looked at before anything else is.</remarks>
    public string Problem() => Kind switch
    {
        ConditionKind.AnyOf => ChildProblem(),
        _ when string.IsNullOrWhiteSpace(Id) => "names nothing",
        ConditionKind.MeterAtLeast or ConditionKind.MeterBelow when !Meters.Exists(Id) => $"names a meter, {Id}, that doesn't exist",
        ConditionKind.Voice when !Enum.TryParse(Id, out Voice _) => $"names a voice, {Id}, that doesn't exist",
        ConditionKind.Class when !Enum.TryParse(Id, out ClassId _) => $"names a class, {Id}, that doesn't exist",
        ConditionKind.InParty or ConditionKind.NotInParty when !Origins.Exists(Id) => $"names {Id}, who has no origin",
        ConditionKind.MeterCompare when Against is null => $"compares {Id}, which isn't one meter > or >= another",
        ConditionKind.MeterCompare when !Meters.Exists(Id) => $"compares a meter, {Id}, that doesn't exist",
        ConditionKind.MeterCompare when !Meters.Exists(Against) => $"compares a meter, {Against}, that doesn't exist",
        ConditionKind.Act when Amount < Meters.Get(Meters.Act).Min || Amount > Meters.Get(Meters.Act).Max => $"names act {Amount}, which there isn't",
        ConditionKind.HasItem or ConditionKind.LacksItem when !KeyItems.Exists(Id) => $"names an item, {Id}, that doesn't exist",
        _ => null,
    };

    /// <summary>Whether any of an AnyOf's conditions is true.</summary>
    private bool AnyHolds(SaveData game)
    {
        foreach (DialogueCondition child in Children)
            if (child.Holds(game)) return true;
        return false;
    }

    /// <summary>What's wrong with an AnyOf: that it has nothing to choose from, or the first fault in what it does have.</summary>
    private string ChildProblem()
    {
        if (Children is null || Children.Length == 0) return "is any of nothing";
        foreach (DialogueCondition child in Children)
            if (child.Problem() is string problem) return $"is any of several, one of which {problem}";
        return null;
    }
}
