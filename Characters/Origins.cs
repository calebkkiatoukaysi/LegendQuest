using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Characters;

/// <summary>
/// Everyone who can be in the party: you, until character creation exists, and the four companions.
/// </summary>
/// <remarks>
/// The names are placeholders, and so is everything about them; I'll rewrite them. Every score
/// spread is 5e's standard array (15, 14, 13, 12, 10, 8) put where that class wants it.
/// </remarks>
public static class Origins
{
    private static readonly Origin[] Table =
    {
        // You, until there's a character creation screen to make you with.
        new()
        {
            Id = "you", Name = "Wanderer", Class = ClassId.Fighter,
            Scores = new AbilityScores { Strength = 13, Dexterity = 15, Constitution = 14, Intelligence = 10, Wisdom = 12, Charisma = 8 },
            Look = new CharacterLook(),
        },
        new()
        {
            Id = "bren", Name = "Bren", Class = ClassId.Fighter,
            Scores = new AbilityScores { Strength = 15, Dexterity = 13, Constitution = 14, Intelligence = 8, Wisdom = 12, Charisma = 10 },
            Look = new CharacterLook { Skin = 2, Shirt = 2, Breeches = 1 },
        },
        new()
        {
            Id = "vesper", Name = "Vesper", Class = ClassId.Rogue,
            Scores = new AbilityScores { Strength = 8, Dexterity = 15, Constitution = 14, Intelligence = 12, Wisdom = 10, Charisma = 13 },
            Look = new CharacterLook { Skin = 1, Shirt = 5, Breeches = 2 },
        },
        new()
        {
            Id = "maren", Name = "Maren", Class = ClassId.Cleric,
            Scores = new AbilityScores { Strength = 13, Dexterity = 10, Constitution = 14, Intelligence = 8, Wisdom = 15, Charisma = 12 },
            Look = new CharacterLook { Skin = 3, Shirt = 1, Breeches = 3 },
        },
        new()
        {
            Id = "corvin", Name = "Corvin", Class = ClassId.Wizard,
            Scores = new AbilityScores { Strength = 8, Dexterity = 13, Constitution = 14, Intelligence = 15, Wisdom = 12, Charisma = 10 },
            Look = new CharacterLook { Skin = 4, Shirt = 4, Breeches = 1 },
        },
    };

    /// <summary>Who a new game starts with, in party order.</summary>
    /// <remarks>
    /// All four slots, so the party strip and the followers can be seen full. When conversations can
    /// recruit people, Maren comes off this and gets recruited instead, and the start is you and two.
    /// </remarks>
    private static readonly string[] Starting = { "you", "bren", "vesper", "maren" };

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
}
