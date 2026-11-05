namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// Act 1, Chapter 1, "Saltwind Welcome": the Red Gulls on the docks, Captain Hale, and the quest
/// board in the Lantern Market.
/// </summary>
/// <remarks>
/// A1-C1-01 and A1-C1-02 are compiled from Story/04-script-prologue-act1.md into StoryPrologueAct1;
/// this class keeps their flags by name, and Fennick's line for after. A1-C1-01 is two conversations either side of the fight with
/// the Red Gulls (EncounterCatalog's a1-c1-01-gulls): every way into it ends with Grell sending the
/// thugs at you, and winning it plays the rest. The flags change the docks as it goes: the thugs are
/// gone once they're beaten, Grell once he's dealt with, and the Watch turns up.
/// </remarks>
public static class Act1Chapter1
{

    /// <summary>The Red Gulls are beaten: the thugs leave the docks.</summary>
    public const string GullsBeaten = "red-gulls-beaten";

    /// <summary>Grell's been dealt with one way or another: he leaves the docks.</summary>
    public const string GrellGone = "grell-gone";

    /// <summary>The flag A1-C1-01 sets when the Soul-Bound kicks Grell into the harbour.</summary>
    public const string DockCruelty = "sb-dock-cruelty";

    /// <summary>The docks scene is over.</summary>
    public const string DocksDone = "a1-c1-01";

    /// <summary>The quest board's been read and the Mirewood contract taken.</summary>
    public const string BoardRead = "a1-c1-02";

    /// <summary>Fennick once the contract's taken.</summary>
    public static readonly DialogueScript FennickAfter = new()
    {
        Id = "a1-c1-02-after",
        With = "fennick",
        Needs = new[] { BoardRead },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "fennick", Line = "The Mirewood's past the city gate and down the old road. Bring back proof, and bring back yourselves." },
        },
    };
}
