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
/// Looks are the story's, as near as the paperdoll gets: the Red Gulls' bandanas, the Watch's helms,
/// Odelle's tricorn.
/// </remarks>
public static class Cast
{
    /// <summary>Hedda's look, which both her ids share: the map's and the as-built lines' refugee-mother, and the story's hedda.</summary>
    private static readonly CharacterLook HeddaLook = new() { Skin = 3, Face = 1, Eyes = 1, Hair = 3, HairColour = 0, Shirt = 4, Vest = 6, Legs = 1, LegsColour = 0, Cloak = 4 };

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

        // Kettle Cove, remembered (P-03): Corra, the Soul-Bound's mother, the net-mender, as she was the
        // day of the squall. A cove-blue shawl, and a skirt the brown of oiled nets.
        new("corra", "Corra", null,
            new CharacterLook { Skin = 3, Face = 1, Eyes = 4, Hair = 3, HairColour = 0, Shirt = 4, Vest = 5, Legs = 1, LegsColour = 0, Cloak = 1 }),

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

        // The companions before they join, standing on a map or speaking from camp: the same looks
        // as their Origins, so nobody changes clothes when they're recruited.
        new("sera", "Sera", null,
            new CharacterLook { Ancestry = Ancestry.Sylvan, Skin = 1, Face = 3, Eyes = 2, Hair = 4, HairColour = 2, Shirt = 4, Vest = 2, LegsColour = 3, Cloak = 5 }),
        new("mira", "Mira", null,
            new CharacterLook { Skin = 0, Face = 1, Eyes = 1, Hair = 3, HairColour = 3, Shirt = 0, Vest = 4, Legs = 1, LegsColour = 4 }),
        new("kael", "Kael", null,
            new CharacterLook { Ancestry = Ancestry.Emberkin, Lineage = 1, Skin = 3, Face = 0, Eyes = 3, Hair = 1, HairColour = 1, Shirt = 4, Vest = 3, LegsColour = 1, Cloak = 4 }),
        new("brannock", "Brannock", null,
            new CharacterLook { Ancestry = Ancestry.Stoneborn, Skin = 2, Face = 3, Eyes = 1, Hair = 1, HairColour = 3, Beard = 4, Shirt = 4, Vest = 6, LegsColour = 1 }),

        // Thornbridge (Act1Chapter3): Hedda and her daughter Lise, and the three sick Mira's remedies
        // go to: an old orchard farmer, a boy with a fever, and a glassblower with burned hands. Mine,
        // written round the story's refugee mother. Sacking, charcoal and oat: everything they own has
        // walked three weeks. Hedda is refugee-mother on the map and in the as-built lines, and hedda
        // in the story's scripts, in the one look.
        new("refugee-mother", "Hedda", null, HeddaLook),
        new("hedda", "Hedda", null, HeddaLook),
        new("refugee-child", "Lise", "child", null),
        new("refugee-1", "Old Aurek", null,
            new CharacterLook { Skin = 3, Face = 3, Eyes = 4, Hair = 5, HairColour = 4, Beard = 3, Shirt = 4, Vest = 6, LegsColour = 0, Cloak = 4 }),
        new("refugee-2", "Teodor", null,
            new CharacterLook { Skin = 4, Face = 0, Eyes = 1, Hair = 0, HairColour = 1, Beard = 1, Shirt = 0, Vest = 5, LegsColour = 1 }),
        new("refugee-3", "Yselle the Glassblower", null,
            new CharacterLook { Ancestry = Ancestry.Stoneborn, Skin = 2, Face = 4, Eyes = 3, Hair = 4, HairColour = 3, Shirt = 3, Vest = 1, Legs = 1, LegsColour = 4 }),

        // The crown (Act1Chapter4): the Princess hooded in the Flagon and in royal blue at court, the
        // King in navy and gold with the small crown, the Chancellor in plum silk, and the palace's
        // messenger and guards in the same navy and gold. The hood, crown and circlet are hats 5, 4
        // and 6.
        new("elara-hooded", "Princess Elara", null,
            new CharacterLook { Skin = 0, Face = 3, Eyes = 2, Hair = 4, HairColour = 2, Hat = 5, Shirt = 4, Vest = 6, LegsColour = 0, Cloak = 4 }),
        new("elara", "Princess Elara", null,
            new CharacterLook { Skin = 0, Face = 3, Eyes = 2, Hair = 4, HairColour = 2, Hat = 6, Shirt = 1, Vest = 0, Legs = 1, LegsColour = 2, Cloak = 1 }),
        new("leon", "King Leon", null,
            new CharacterLook { Skin = 0, Face = 1, Eyes = 4, Hair = 2, HairColour = 4, Beard = 4, Hat = 4, Shirt = 1, Vest = 0, LegsColour = 2, Cloak = 5 }),
        new("crane", "Lord Chancellor Crane", null,
            new CharacterLook { Skin = 1, Face = 2, Eyes = 4, Hair = 2, HairColour = 1, Beard = 2, Shirt = 1, Vest = 4, LegsColour = 1, Cloak = 6 }),
        new("royal-messenger", "Royal Messenger", null,
            new CharacterLook { Skin = 1, Face = 4, Eyes = 1, Hair = 0, HairColour = 3, Shirt = 1, Vest = 0, LegsColour = 2, Cloak = 5 }),
        new("royal-guard", "Royal Guard", null,
            new CharacterLook { Skin = 2, Face = 3, Eyes = 1, Hair = 1, HairColour = 0, Beard = 1, Hat = 3, Shirt = 1, Vest = 0, LegsColour = 2, Cloak = 5 }),

        // The Rusty Anvil's barkeep: a Wildkin of the bear line, who's seen every fight on the docks.
        new("anvil-barkeep", "Hessa the Barkeep", null,
            new CharacterLook { Ancestry = Ancestry.Wildkin, Lineage = 2, Skin = 2, Face = 5, Eyes = 1, Hair = 3, HairColour = 1, Shirt = 4, Vest = 6, LegsColour = 1 }),
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
