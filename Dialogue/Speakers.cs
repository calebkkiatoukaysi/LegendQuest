using System;

namespace LegendQuest.Dialogue;

/// <summary>
/// The speakers who aren't anyone: the narrator, and the cards. And how a line is written when it's
/// someone heard and not seen.
/// </summary>
public static class Speakers
{
    /// <summary>The narrator: no face and no name, just the line.</summary>
    public const string Narrator = "narrator";

    /// <summary>A card: the line big in the middle of the screen, like a title or a fight that's over.</summary>
    public const string Card = "card";

    /// <summary>A chapter's card: the line is the chapter's name, like "Chapter 2  ·  The Rot in the Mirewood", big in the middle of the screen under the act, the way the ship coming in ends on Chapter 1's.</summary>
    public const string Chapter = "chapter";

    /// <summary>What a voice's speaker id starts with, before whose voice it is: voice:soulbound is the Soul-Bound heard in your head. Their face, tinted, and no name.</summary>
    public const string VoicePrefix = "voice:";

    /// <summary>Whether an id is one of the speakers who aren't anyone: the narrator, a card or a chapter's card. A voice isn't: it's someone's.</summary>
    /// <param name="id">The speaker id.</param>
    public static bool Exists(string id) => id is Narrator or Card or Chapter;

    /// <summary>Whose voice a speaker is, for one written VoicePrefix and an id.</summary>
    /// <param name="speaker">The speaker id.</param>
    /// <returns>The id after the prefix, or null for a speaker who isn't a voice.</returns>
    public static string VoiceOf(string speaker) =>
        speaker is not null && speaker.StartsWith(VoicePrefix, StringComparison.Ordinal) ? speaker[VoicePrefix.Length..] : null;
}
