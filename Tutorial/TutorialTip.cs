using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LegendQuest.Tutorial;

/// <summary>
/// One tip: a single idea, the keys that do it, and where its card goes. TutorialCatalog has every one.
/// </summary>
/// <remarks>
/// One idea a tip, said in a sentence or two, the way the games I like teach: at the moment you need
/// it and never again. The keys are drawn as key caps rather than written into the sentence, so
/// the eye finds them first. Every tip is in HOW TO PLAY too, under its topic, so one that's been
/// put away can always be found again.
/// </remarks>
public sealed class TutorialTip
{
    /// <summary>What the game calls it. The save keeps the ones you've seen by this.</summary>
    public required string Id { get; init; }

    /// <summary>The few words over it, in capitals on screen.</summary>
    public required string Title { get; init; }

    /// <summary>The idea itself, a sentence or two in the game's voice.</summary>
    public required string Body { get; init; }

    /// <summary>The page of HOW TO PLAY it's on.</summary>
    public required TipTopic Topic { get; init; }

    /// <summary>The keys it teaches, each drawn as a key cap (KeyCaps.Click and the rest are the mouse). None for a tip about something you see rather than press.</summary>
    public IReadOnlyList<string> Keys { get; init; } = Array.Empty<string>();

    /// <summary>Where its card goes.</summary>
    public TipAnchor Anchor { get; init; } = TipAnchor.BottomRight;

    /// <summary>Whether it holds everything but the pause menu until it's put away. For the few that have to be read before you go on; the rest let you walk while they're up.</summary>
    public bool Blocking { get; init; }

    /// <summary>Whether it holds a fight while it's up, and stays until it's put away or what it teaches is done, though the keys and the mouse still work: a lesson of the fight's (Story/11-combat.md §7.2), read with the fight stopped and done by doing it.</summary>
    public bool HoldsFight { get; init; }

    /// <summary>What a slot in the body ("{focus}") says when the game has nothing more particular to put there: in HOW TO PLAY, say. Each slot in the body needs one.</summary>
    public IReadOnlyDictionary<string, string> Fill { get; init; } = new Dictionary<string, string>();

    /// <summary>The body with its slots filled: from what the game has to say now (your class's way of filling Focus), or else the tip's own Fill.</summary>
    /// <param name="filling">What the game has for each slot, by name, or null.</param>
    public string Words(IReadOnlyDictionary<string, string> filling = null) =>
        Slot.Replace(Body, match =>
            filling is not null && filling.TryGetValue(match.Groups[1].Value, out string said) ? said
            : Fill.TryGetValue(match.Groups[1].Value, out string plain) ? plain : match.Value);

    /// <summary>A slot in a body: a name in braces.</summary>
    private static readonly Regex Slot = new(@"\{([a-z-]+)\}");
}
