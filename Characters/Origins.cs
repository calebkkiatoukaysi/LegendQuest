using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Characters;

/// <summary>
/// Everyone who can be in the party: you as you are before the creation screen changes you, Valen,
/// and the four companions.
/// </summary>
/// <remarks>
/// The names are placeholders, and so is everything about them; I'll rewrite them. Every score
/// spread is 5e's standard array (15, 14, 13, 12, 10, 8) put where that class wants it. Valen came
/// over on the boat with you and shows you round, so he's with you from the start.
/// </remarks>
public static class Origins
{
    private static readonly Origin[] Table =
    {
        // You, as the creation screen first shows you: the youth just as he was drawn.
        new()
        {
            Id = You, Name = "Wanderer", Class = ClassId.Fighter,
            Scores = new AbilityScores { Strength = 13, Dexterity = 15, Constitution = 14, Intelligence = 10, Wisdom = 12, Charisma = 8 },
            Look = new CharacterLook(),
        },
        // Black hair tied back, stubble, a charcoal vest and a woad cloak, so he's never mistaken
        // for you when you're stood side by side.
        new()
        {
            Id = Valen, Name = "Valen", Class = ClassId.Fighter,
            Scores = new AbilityScores { Strength = 15, Dexterity = 13, Constitution = 14, Intelligence = 10, Wisdom = 12, Charisma = 8 },
            Look = new CharacterLook { Skin = 3, Face = 3, Eyes = 3, Hair = 3, HairColour = 1, Beard = 1, Shirt = 4, Vest = 6, LegsColour = 2, Cloak = 1 },
        },
        new()
        {
            Id = "bren", Name = "Bren", Class = ClassId.Fighter,
            Scores = new AbilityScores { Strength = 15, Dexterity = 13, Constitution = 14, Intelligence = 8, Wisdom = 12, Charisma = 10 },
            Look = new CharacterLook { Skin = 2, Hair = 1, HairColour = 3, Beard = 3, Vest = 3, LegsColour = 1 },
        },
        new()
        {
            Id = "vesper", Name = "Vesper", Class = ClassId.Rogue,
            Scores = new AbilityScores { Strength = 8, Dexterity = 15, Constitution = 14, Intelligence = 12, Wisdom = 10, Charisma = 13 },
            Look = new CharacterLook { Skin = 1, Face = 2, Hair = 2, HairColour = 5, Vest = 4, Legs = 1, LegsColour = 1, Cloak = 4 },
        },
        new()
        {
            Id = "maren", Name = "Maren", Class = ClassId.Cleric,
            Scores = new AbilityScores { Strength = 13, Dexterity = 10, Constitution = 14, Intelligence = 8, Wisdom = 15, Charisma = 12 },
            Look = new CharacterLook { Skin = 4, Face = 1, Hair = 4, HairColour = 4, Shirt = 1, Vest = 1, Legs = 1, LegsColour = 4 },
        },
        new()
        {
            Id = "corvin", Name = "Corvin", Class = ClassId.Wizard,
            Scores = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 15, Wisdom = 12, Charisma = 10 },
            Look = new CharacterLook { Skin = 1, Face = 4, Hair = 1, HairColour = 6, Beard = 2, Vest = 4, Cloak = 6 },
        },
    };

    /// <summary>Who a new game starts with, in party order: you and Valen, off the boat together. The rest get recruited.</summary>
    private static readonly string[] Starting = { You, Valen };

    /// <summary>Your id. You're always first in a new party.</summary>
    public const string You = "you";

    /// <summary>Valen's id. The arrival and the tutorial look for him by it.</summary>
    public const string Valen = "valen";

    private static readonly Dictionary<string, Origin> ById = new();

    /// <summary>The most people in the party at once. Anyone else waits at camp.</summary>
    public const int PartySize = 4;

    /// <summary>Indexes the table and checks the starting party is in it.</summary>
    /// <remarks>Throws on purpose, so a typo shows up when the game starts.</remarks>
    static Origins()
    {
        foreach (Origin origin in Table)
            if (!ById.TryAdd(origin.Id, origin)) throw new InvalidOperationException($"Two origins are called {origin.Id}.");

        if (Starting.Length > PartySize) throw new InvalidOperationException($"The starting party has {Starting.Length} people; the most there can be is {PartySize}.");
        foreach (string id in Starting)
            if (!ById.ContainsKey(id)) throw new InvalidOperationException($"The starting party has {id} in it, and there's no origin called that.");
    }

    /// <summary>A fresh party, the way a new game starts.</summary>
    public static List<PartyMember> StartingParty()
    {
        var party = new List<PartyMember>();
        foreach (string id in Starting) party.Add(ById[id].Create());
        return party;
    }

    /// <summary>Someone fresh from their origin, the way they'd join.</summary>
    /// <param name="id">Who.</param>
    public static PartyMember Create(string id) => ById[id].Create();
}
