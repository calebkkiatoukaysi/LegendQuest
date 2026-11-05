using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Story;

/// <summary>
/// One of the story's people: their name, and either the townsfolk sheet they're drawn from or the
/// look the paperdoll bakes for them. Someone who's only ever a voice has neither.
/// </summary>
/// <remarks>
/// Someone the story changes the look of partway through a scene (a swap cue) has their other looks
/// by name as well: the Princess with her hood up, as she is in the Flagon.
/// </remarks>
/// <param name="Id">What maps and conversations call them.</param>
/// <param name="Name">What they're called on screen.</param>
/// <param name="Sheet">The townsfolk sheet they're drawn from, or null.</param>
/// <param name="Look">The look the paperdoll bakes them from, or null.</param>
/// <param name="Alternates">Their other looks, by the name a swap cue calls each, or null for none.</param>
public sealed record CastMember(string Id, string Name, string Sheet, CharacterLook Look, IReadOnlyDictionary<string, CharacterLook> Alternates = null)
{
    /// <summary>One of their looks by the name a swap cue gives it: their own id for the look they usually have, or one of their Alternates.</summary>
    /// <param name="name">The look's name.</param>
    /// <returns>The look, or null if they've none called that (or they're drawn from a sheet and it's their own).</returns>
    public CharacterLook LookCalled(string name) =>
        name == Id ? Look : Alternates is not null && Alternates.TryGetValue(name ?? string.Empty, out CharacterLook other) ? other : null;
}
