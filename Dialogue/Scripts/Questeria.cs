using LegendQuest.Characters;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// The capital between the story's scenes: the Soul-Bound showing you the way in, and the Gilded
/// Flagon's keeper and the rosewater, a first errand.
/// </summary>
/// <remarks>
/// The rosewater errand was the tutorial's when Valen ran it; it's an errand now, with the
/// Soul-Bound in his place. The tutorial's lines are the Soul-Bound's, said as they'd say them.
/// </remarks>
public static class Questeria
{
    private const string You = Origins.You;
    private const string SoulBound = Origins.SoulBound;

    private const string WorkTaken = "rosewater-taken";
    private const string RosewaterGot = "rosewater-got";
    private const string WorkDone = "rosewater-done";

    /// <summary>Coming up from the harbour into the city.</summary>
    public static readonly DialogueScript SoulBoundCity = new()
    {
        Id = "soulbound-city",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = SoulBound, Line = "The Lantern Market's past the fountain, on the east side. Board's there, Hale said." },
            new() { Speaker = SoulBound, Line = "If we get lost, M for the map. And walk up to anyone and press E. People here talk, if you let them." },
        },
    };

    /// <summary>The keeper of the Gilded Flagon, with an errand.</summary>
    public static readonly DialogueScript InnkeeperWork = new()
    {
        Id = "innkeeper-work",
        With = "innkeeper",
        Needs = new[] { Act1Chapter1.DocksDone },
        Unless = new[] { WorkTaken },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "innkeeper", Line = "Off the morning ship, the pair of you? You've the look." },
            new() { Speaker = SoulBound, Line = "{name}'s after work. The honest kind. So am I." },
            new() { Speaker = "innkeeper", Line = "Honest's all I've got. The flower seller at the market owes me a crate of rosewater, and my knees owe me a sit down. Fetch it for me?" },
            new()
            {
                Speaker = You, Line = "Rosewater. Flower seller. On my way.",
                Effects = new[] { DialogueEffect.Set(WorkTaken) },
            },
        },
    };

    /// <summary>The keeper, waiting on the rosewater.</summary>
    public static readonly DialogueScript InnkeeperWaiting = new()
    {
        Id = "innkeeper-waiting",
        With = "innkeeper",
        Needs = new[] { WorkTaken },
        Unless = new[] { RosewaterGot },
        Nodes = new DialogueNode[] { new() { Speaker = "innkeeper", Line = "The flower seller. Bottom of the market, with the cart. I'll be here." } },
    };

    /// <summary>The flower seller hands it over.</summary>
    public static readonly DialogueScript Rosewater = new()
    {
        Id = "flower-rosewater",
        With = "flower-seller",
        Needs = new[] { WorkTaken },
        Unless = new[] { RosewaterGot },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "flower-seller", Line = "From the Flagon? Here. Carry it level, it's the good stuff." },
            new()
            {
                Speaker = SoulBound, Line = "Half of work here's walking. The other half's not dropping things.",
                Effects = new[] { DialogueEffect.Set(RosewaterGot) },
            },
        },
    };

    /// <summary>The keeper pays.</summary>
    public static readonly DialogueScript InnkeeperPaid = new()
    {
        Id = "innkeeper-paid",
        With = "innkeeper",
        Needs = new[] { RosewaterGot },
        Unless = new[] { WorkDone },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "innkeeper", Line = "That's the one. Here, for your legs. Come back tomorrow and there'll be more." },
            new()
            {
                Speaker = SoulBound, Line = "The crystal by the fountain's a waypoint, by the way. Touch it once and you can come back to it from anywhere.",
                Effects = new[] { DialogueEffect.Set(WorkDone), DialogueEffect.Gold(15) },
            },
        },
    };

    /// <summary>The keeper once you've been paid.</summary>
    public static readonly DialogueScript InnkeeperAfter = new()
    {
        Id = "innkeeper-after",
        With = "innkeeper",
        Needs = new[] { WorkDone },
        Nodes = new DialogueNode[] { new() { Speaker = "innkeeper", Line = "Rooms upstairs when you're tired. Stew's on the fire." } },
    };
}
