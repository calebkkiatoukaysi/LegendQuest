using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Characters;

/// <summary>
/// Everyone who can be in the party: you and the Soul-Bound as the creation screen starts them,
/// and the companions the story recruits.
/// </summary>
/// <remarks>
/// You and the Soul-Bound are made on the creation screen, so theirs are only where it starts:
/// the Soul-Bound has no name until you give them one. The companions are the story's: their
/// peoples, classes and looks come from the story, and every score spread is 5e's standard array
/// put where their class wants it (ClassCatalog's recommended one). Their kits are the story's
/// too, where it gives them weapons of their own: Sera's bow, Kael's twin daggers, Brannock's
/// battered round shield. Mira sets out in her class's.
/// </remarks>
public static class Origins
{
    /// <summary>Your id. You're always first in a new party.</summary>
    public const string You = "you";

    /// <summary>The Soul-Bound's id. The arrival, the conversations and the tokens find them by it.</summary>
    public const string SoulBound = "soulbound";

    /// <summary>The most people in the party at once. Anyone else recruited waits at camp.</summary>
    public const int PartySize = 4;

    /// <summary>The most the party can ever hold: one over PartySize, for the stretch of the story when someone who went away comes back to a full party. Recruiting still stops at PartySize.</summary>
    public const int MostInParty = PartySize + 1;

    private static readonly Origin[] Table =
    {
        // You and the Soul-Bound, before the creation screen's been at them.
        new() { Id = You, Name = "Wanderer", Class = ClassId.Fighter, Look = new CharacterLook { Portrait = "face-player-1" } },
        new() { Id = SoulBound, Name = string.Empty, Class = ClassId.Rogue, Look = new CharacterLook { Hair = 2, HairColour = 1, Vest = 3, Portrait = "face-player-3" } },

        // A Sylvan Wildcaller, the Mirewood's warden: a braid, green and brown leathers, a short cape.
        new()
        {
            Id = "sera", Name = "Sera", Class = ClassId.Druid,
            Look = new CharacterLook { Ancestry = Ancestry.Sylvan, Skin = 1, Face = 3, Eyes = 2, Hair = 4, HairColour = 2, Shirt = 4, Vest = 2, LegsColour = 3, Cloak = 5 },
            Kit = ClassCatalog.Kit("shortbow", "leather-armour"),
        },
        // An Emberkin Shadowstep from the Low Ring: swept horns, an ember tail, a red sash, a dark cloak.
        new()
        {
            Id = "kael", Name = "Kael", Class = ClassId.Rogue,
            Look = new CharacterLook { Ancestry = Ancestry.Emberkin, Lineage = 1, Skin = 3, Face = 0, Eyes = 3, Hair = 1, HairColour = 1, Shirt = 4, Vest = 3, LegsColour = 1, Cloak = 4 },
            Kit = ClassCatalog.Kit("dagger", "dagger", "leather-armour"),
        },
        // A Human Lightbearer from Doomward: faded scholar's robes, her hair tied back.
        new()
        {
            Id = "mira", Name = "Mira", Class = ClassId.Cleric,
            Look = new CharacterLook { Skin = 0, Face = 1, Eyes = 1, Hair = 3, HairColour = 3, Shirt = 0, Vest = 4, Legs = 1, LegsColour = 4 },
        },
        // A Stoneborn Warden, the Royal Guard's disgraced captain: a full braided beard, battered plate.
        new()
        {
            Id = "brannock", Name = "Brannock", Class = ClassId.Paladin,
            Look = new CharacterLook { Ancestry = Ancestry.Stoneborn, Skin = 2, Face = 3, Eyes = 1, Hair = 1, HairColour = 3, Beard = 4, Shirt = 4, Vest = 6, LegsColour = 1 },
            Kit = ClassCatalog.Kit("longsword", "wooden-shield", "chain-mail"),
        },
    };

    private static readonly Dictionary<string, Origin> ById = new();

    /// <summary>Indexes the table, and checks every companion's own kit is one their class can set out in.</summary>
    /// <remarks>Throws on purpose, so a typo shows up when the game starts.</remarks>
    static Origins()
    {
        foreach (Origin origin in Table)
        {
            if (origin.Kit is not null && ClassCatalog.Get(origin.Class).KitProblem(origin.Kit) is string problem)
                throw new InvalidOperationException($"Origin {origin.Id}'s kit {problem}.");
            if (!ById.TryAdd(origin.Id, origin)) throw new InvalidOperationException($"Two origins are called {origin.Id}.");
        }
    }

    /// <summary>Whether there's an origin with this id.</summary>
    /// <param name="id">Who.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>Someone fresh from their origin, the way they'd join, with their class's recommended scores.</summary>
    /// <param name="id">Who. They have to exist.</param>
    public static PartyMember Create(string id) => ById[id].Create();

    /// <summary>What someone sets out in as a given class: a companion's own kit, or the class's starting kit.</summary>
    /// <remarks>For dressing someone who's already been made: an old save's party, or you when the creation screen changes your class.</remarks>
    /// <param name="id">Who, by origin id. Anyone without an origin gets the class's kit.</param>
    /// <param name="classId">Their class.</param>
    public static IReadOnlyList<string> KitFor(string id, ClassId classId) =>
        ById.TryGetValue(id ?? string.Empty, out Origin origin) ? origin.KitFor(classId) : ClassCatalog.Get(classId).StartingKit;

    /// <summary>What someone's called before they've joined, for the lines that mention them.</summary>
    /// <param name="id">Who.</param>
    public static string NameOf(string id) => ById.TryGetValue(id, out Origin origin) ? origin.Name : id;
}
