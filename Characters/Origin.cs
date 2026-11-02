using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Characters;

/// <summary>
/// Where someone in the party starts from: their name, class, look and kit before anything's
/// happened to them.
/// </summary>
public sealed class Origin
{
    /// <summary>Who they are. Saves keep party members by this.</summary>
    public required string Id { get; init; }

    /// <summary>What they're called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>Their class.</summary>
    public required ClassId Class { get; init; }

    /// <summary>How they look.</summary>
    public required CharacterLook Look { get; init; }

    /// <summary>What they set out in when it isn't their class's starting kit, by item id, or null for their class's. A companion's own weapons.</summary>
    public IReadOnlyList<string> Kit { get; init; }

    /// <summary>What someone from this origin sets out in as a given class: their own kit if it's their own class, and that class's starting kit if it isn't.</summary>
    /// <param name="classId">The class, which the creation screen can change from the origin's.</param>
    public IReadOnlyList<string> KitFor(ClassId classId) => Kit is not null && classId == Class ? Kit : ClassCatalog.Get(classId).StartingKit;

    /// <summary>A new party member from this origin, with their class's recommended scores, dressed in their kit, at full health.</summary>
    public PartyMember Create()
    {
        var member = new PartyMember
        {
            Id = Id,
            Name = Name,
            Class = Class,
            Scores = ClassCatalog.Get(Class).Recommended,
            Look = Look.Copy(),
        };
        member.PutOn(KitFor(Class));
        member.Hp = member.MaxHp;
        return member;
    }
}
