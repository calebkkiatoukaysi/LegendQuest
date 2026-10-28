using LegendQuest.Characters;

namespace LegendQuest.Story;

/// <summary>
/// One of the story's people: their name, and either the townsfolk sheet they're drawn from or the
/// look the paperdoll bakes for them. Someone who's only ever a voice has neither.
/// </summary>
/// <param name="Id">What maps and conversations call them.</param>
/// <param name="Name">What they're called on screen.</param>
/// <param name="Sheet">The townsfolk sheet they're drawn from, or null.</param>
/// <param name="Look">The look the paperdoll bakes them from, or null.</param>
public sealed record CastMember(string Id, string Name, string Sheet, CharacterLook Look);
