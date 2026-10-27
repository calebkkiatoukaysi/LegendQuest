using LegendQuest.Rules;

namespace LegendQuest.Characters;

/// <summary>
/// Where someone in the party starts from: their name, class, scores and look before anything's
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

    /// <summary>Their ability scores.</summary>
    public required AbilityScores Scores { get; init; }

    /// <summary>How they look.</summary>
    public required CharacterLook Look { get; init; }

    /// <summary>A new party member from this origin, at full health.</summary>
    public PartyMember Create()
    {
        var member = new PartyMember
        {
            Id = Id,
            Name = Name,
            Class = Class,
            Scores = Scores,
            Look = new CharacterLook { Skin = Look.Skin, Shirt = Look.Shirt, Breeches = Look.Breeches },
        };
        member.Hp = member.MaxHp;
        return member;
    }
}
