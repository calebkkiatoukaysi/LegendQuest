using System;
using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Story;

/// <summary>
/// The story's people who aren't in the party: what they're called, and how they look, either
/// baked off the paperdoll like the party or a townsfolk sheet of their own.
/// </summary>
/// <remarks>
/// A map puts one of them down with npc ... cast=(id), and a conversation's line from that id
/// gets their name and face whether they're on the map or not (the lookout up the mast never is).
/// Looks are STORY.md's, as near as the paperdoll gets: the Red Gulls' bandanas, the Watch's helms,
/// Odelle's tricorn.
/// </remarks>
public static class Cast
{
    private static readonly CastMember[] Table =
    {
        new("lookout", "Lookout", null, null),
        new("odelle", "Captain Odelle Marsh", null,
            new CharacterLook { Skin = 4, Face = 3, Eyes = 1, Hair = 2, HairColour = 1, Hat = 2, Shirt = 1, Vest = 3, LegsColour = 1, Cloak = 4 }),
        new("tomas", "Old Tomas", "old-man", null),
        new("red-gull", "Red Gull Thug", null,
            new CharacterLook { Skin = 2, Face = 3, Hair = 1, HairColour = 1, Beard = 1, Hat = 1, Shirt = 4, Vest = 3, LegsColour = 1 }),
        new("grell", "Bosun Grell", null,
            new CharacterLook { Ancestry = Ancestry.Stoneborn, Skin = 3, Face = 3, Hair = 5, HairColour = 3, Beard = 4, Shirt = 4, Vest = 6, LegsColour = 1 }),
        new("hale", "Captain Hale", null,
            new CharacterLook { Skin = 0, Face = 3, Eyes = 4, Hair = 1, HairColour = 4, Beard = 1, Shirt = 1, Vest = 0, LegsColour = 2, Cloak = 1 }),
        new("watch", "City Watch", null,
            new CharacterLook { Skin = 2, Hair = 1, HairColour = 0, Hat = 3, Shirt = 1, Vest = 0, LegsColour = 2, Cloak = 1 }),
        new("fennick", "Questmaster Fennick", null,
            new CharacterLook { Skin = 1, Face = 1, Eyes = 4, Hair = 1, HairColour = 4, Beard = 2, Shirt = 4, Vest = 5, LegsColour = 4 }),
    };

    private static readonly Dictionary<string, CastMember> ById = new();

    /// <summary>Indexes the table.</summary>
    /// <remarks>Throws on purpose, so a typo shows up when the game starts. A member with a sheet has to have a townsfolk sheet by that name.</remarks>
    static Cast()
    {
        foreach (CastMember member in Table)
        {
            if (member.Sheet is not null && !Townsfolk.Contains(member.Sheet))
                throw new InvalidOperationException($"Cast member {member.Id} is drawn from {member.Sheet}, which isn't a townsfolk sheet.");
            if (!ById.TryAdd(member.Id, member)) throw new InvalidOperationException($"Two cast members are called {member.Id}.");
        }
    }

    /// <summary>One of the cast, or null if there's nobody by that id.</summary>
    /// <param name="id">Who.</param>
    public static CastMember Get(string id) => ById.GetValueOrDefault(id ?? string.Empty);
}
