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

        // The townspeople who'll stop and talk (Townspeople's conversations). Mine, not the story's,
        // but for Sergeant Brisk, who's in the Watch's helm and blue like the rest of it.
        new("netty", "Netty the Net-Mender", null,
            new CharacterLook { Skin = 3, Face = 4, Eyes = 2, Hair = 4, HairColour = 4, Shirt = 4, Vest = 1, Legs = 1, LegsColour = 2 }),
        new("maud", "Maud Tolley", null,
            new CharacterLook { Skin = 0, Face = 1, Eyes = 4, Hair = 3, HairColour = 4, Shirt = 0, Vest = 6, Legs = 1, LegsColour = 1, Cloak = 4 }),
        new("pip", "Pip the Potboy", null,
            new CharacterLook { Ancestry = Ancestry.Hearthling, Skin = 0, Face = 4, Eyes = 1, Hair = 0, HairColour = 3, Shirt = 1, Vest = 5, LegsColour = 0 }),
        new("osric", "Osric Penhallow", null,
            new CharacterLook { Skin = 1, Face = 0, Eyes = 1, Hair = 1, HairColour = 5, Beard = 2, Shirt = 1, Vest = 4, LegsColour = 5, Cloak = 6 }),
        new("brisk", "Sergeant Brisk", null,
            new CharacterLook { Skin = 4, Face = 3, Eyes = 1, Hair = 1, HairColour = 1, Beard = 2, Hat = 3, Shirt = 1, Vest = 0, LegsColour = 2, Cloak = 1 }),
        new("wren", "Wren", null,
            new CharacterLook { Ancestry = Ancestry.Hearthling, Skin = 4, Face = 4, Eyes = 3, Hair = 3, HairColour = 1, Shirt = 4, Vest = 6, LegsColour = 0, Cloak = 4 }),
        new("lark", "Lark the Busker", null,
            new CharacterLook { Ancestry = Ancestry.Sylvan, Skin = 1, Face = 2, Eyes = 5, Hair = 2, HairColour = 6, Shirt = 3, Vest = 7, LegsColour = 4, Cloak = 6 }),
        new("ilsa", "Ilsa Fenwright", null,
            new CharacterLook { Skin = 2, Face = 1, Eyes = 2, Hair = 4, HairColour = 2, Shirt = 2, Vest = 0, Legs = 1, LegsColour = 2, Cloak = 1 }),
        new("tamsin", "Ranger Tamsin", null,
            new CharacterLook { Ancestry = Ancestry.Wildkin, Lineage = 1, Skin = 3, Face = 5, Eyes = 3, Hair = 3, HairColour = 0, Shirt = 4, Vest = 2, LegsColour = 3, Cloak = 3 }),
        new("oswin", "Guardsman Oswin", null,
            new CharacterLook { Skin = 0, Face = 0, Eyes = 0, Hair = 1, HairColour = 2, Hat = 3, Shirt = 1, Vest = 0, LegsColour = 2, Cloak = 5 }),
        new("hesper", "Sister Hesper", null,
            new CharacterLook { Skin = 5, Face = 1, Eyes = 1, Hair = 2, HairColour = 4, Shirt = 1, Vest = 5, Legs = 1, LegsColour = 4, Cloak = 5 }),
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
