using System;
using System.Collections.Generic;

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
}
