using System;
using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Dialogue;

/// <summary>
/// Every conversation there is: the arrival, Valen showing you round, and your first work from the
/// innkeeper.
/// </summary>
/// <remarks>
/// Placeholders, all of it, the same as the names; I write the lines. Kept short and plain, and
/// nothing about the world's bigger questions. Valen's the tutorial: what he says is how you find
/// out what the keys do, so it's said in his voice instead of on a card. The flags are what move
/// the first task along: arrived, then the work taken, the rosewater got, and the work done.
/// </remarks>
public static class Conversations
{
    /// <summary>The flags the arrival and the first task move through, in order.</summary>
    public const string Arrived = "arrived";
    private const string WorkTaken = "work-taken";
    private const string RosewaterGot = "rosewater-got";
    private const string WorkDone = "work-done";

    /// <summary>On the boardwalk, off the ship.</summary>
    public const string ArrivalId = "arrival";

    /// <summary>What you're to do once you're off the ship.</summary>
    public const string FirstObjective = "Find the inn in Questeria, east of the plaza";

    private static readonly Conversation[] Table =
    {
        new()
        {
            Id = ArrivalId,
            Lines = new Line[]
            {
                new(Origins.Valen, "Questeria. Told you you'd see the spire before you smelled the fish."),
                new(Origins.You, "It's bigger than you said."),
                new(Origins.Valen, "Everything's bigger than I say. Whatever you left on the other shore, {name}, it stays there. This is the new start."),
                new(Origins.Valen, "First thing anyone does here is find work, and innkeepers always know who's hiring. The city's up the street, east. Walk with the keys, or point and click where you want to go."),
            },
        },
        new()
        {
            Id = "valen-city",
            Lines = new Line[]
            {
                new(Origins.Valen, "The plaza's straight ahead. The inn's past it on the east side, by the market."),
                new(Origins.Valen, "If you lose your way, M opens the map. Walk up to anyone and press E to talk."),
            },
        },
        new()
        {
            Id = "innkeeper-work",
            With = "innkeeper",
            Needs = new[] { Arrived },
            Unless = new[] { WorkTaken },
            Lines = new Line[]
            {
                new("innkeeper", "Off the morning ship, the pair of you? You've the look."),
                new(Origins.Valen, "{name}'s after work. The honest kind."),
                new("innkeeper", "Honest's all I've got. The flower seller at the market owes me a crate of rosewater, and my knees owe me a sit down. Fetch it for me?"),
                new(Origins.You, "Rosewater. Flower seller. On my way."),
            },
            Sets = new[] { WorkTaken },
            Objective = "Collect the rosewater from the flower seller at the market",
        },
        new()
        {
            Id = "innkeeper-waiting",
            With = "innkeeper",
            Needs = new[] { WorkTaken },
            Unless = new[] { RosewaterGot },
            Lines = new Line[] { new("innkeeper", "The flower seller. Bottom of the market, with the cart. I'll be here.") },
        },
        new()
        {
            Id = "flower-rosewater",
            With = "flower-seller",
            Needs = new[] { WorkTaken },
            Unless = new[] { RosewaterGot },
            Lines = new Line[]
            {
                new("flower-seller", "From the inn? Here. Carry it level, it's the good stuff."),
                new(Origins.Valen, "See? Half of work here is walking. The other half's not dropping things."),
            },
            Sets = new[] { RosewaterGot },
            Objective = "Take the rosewater back to the innkeeper",
        },
        new()
        {
            Id = "innkeeper-paid",
            With = "innkeeper",
            Needs = new[] { RosewaterGot },
            Unless = new[] { WorkDone },
            Lines = new Line[]
            {
                new("innkeeper", "That's the one. Here, for your legs. Come back tomorrow and there'll be more."),
                new(Origins.Valen, "Not bad for a first day. There's more on the notice board in the plaza, and more past the city gate if you don't mind the road."),
                new(Origins.Valen, "The crystal by the fountain's a waypoint. Touch it once, and you can come back to it from anywhere."),
            },
            Sets = new[] { WorkDone },
            Objective = "Explore Questeria, or take the old road south",
            Gold = 15,
        },
        new()
        {
            Id = "innkeeper-after",
            With = "innkeeper",
            Needs = new[] { WorkDone },
            Lines = new Line[] { new("innkeeper", "Rooms upstairs when you're tired. Stew's on the fire.") },
        },
    };

    private static readonly Dictionary<string, Conversation> ById = new();

    /// <summary>Indexes the table, and checks every line has someone to say it.</summary>
    /// <remarks>Throws on purpose, so a typo shows up when the game starts.</remarks>
    static Conversations()
    {
        foreach (Conversation conversation in Table)
        {
            if (conversation.Lines.Count == 0) throw new InvalidOperationException($"Conversation {conversation.Id} has nothing in it.");
            foreach (Line line in conversation.Lines)
                if (string.IsNullOrWhiteSpace(line.Speaker)) throw new InvalidOperationException($"Conversation {conversation.Id} has a line nobody says.");
            if (!ById.TryAdd(conversation.Id, conversation)) throw new InvalidOperationException($"Two conversations are called {conversation.Id}.");
        }
    }

    /// <summary>A conversation by id.</summary>
    /// <param name="id">Its id.</param>
    /// <returns>The conversation, or null if there's none called that.</returns>
    public static Conversation Get(string id) => ById.GetValueOrDefault(id);

    /// <summary>What someone on a map has to say with these flags set: their first conversation that can happen, or null if they've none.</summary>
    /// <param name="npcId">Who, by their id on the map.</param>
    /// <param name="flags">The flags.</param>
    public static Conversation For(string npcId, ISet<string> flags)
    {
        foreach (Conversation conversation in Table)
            if (conversation.With == npcId && conversation.CanHappen(flags)) return conversation;
        return null;
    }
}
