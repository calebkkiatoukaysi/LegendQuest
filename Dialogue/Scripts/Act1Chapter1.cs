using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// Act 1, Chapter 1, "Saltwind Welcome": the Red Gulls on the docks, Captain Hale, and the quest
/// board in the Lantern Market.
/// </summary>
/// <remarks>
/// STORY.md's A1-C1-01 and A1-C1-02. The fight with the Red Gulls is a card until combat exists.
/// The flags it sets change the docks as it goes: the thugs are gone once they're beaten, Grell
/// once he's dealt with, and the Watch turns up.
/// </remarks>
public static class Act1Chapter1
{
    private const string You = Origins.You;
    private const string SoulBound = Origins.SoulBound;

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

    /// <summary>A1-C1-01: the Red Gulls shaking down Old Tomas, the fight, Grell, Captain Hale, and the hook.</summary>
    public static readonly DialogueScript Docks = new()
    {
        Id = "a1-c1-01",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "red-gull", Line = "Dock tax is due, grandfather. Doubled. Harbour's dangerous these days." },
            new() { Speaker = "tomas", Line = "There's no such tax! Please, that's my whole week's catch..." },
            new()
            {
                Speaker = SoulBound, Direction = "jaw tightening", Line = "...You seeing this?",
                Options = new DialogueOption[]
                {
                    new() { Text = "Let's handle it.", Next = "fight", Effects = new[] { DialogueEffect.Add(Meters.Fame, 1) } },
                    new()
                    {
                        Text = "Try talking first.",
                        Check = new DialogueCheck
                        {
                            Label = "PERSUADE",
                            Advantage = new[] { DialogueCondition.IsClass(ClassId.Bard), DialogueCondition.Speaks(Voice.Honorable) },
                        },
                        OnSuccess = "talked", OnFailure = "not-talked",
                    },
                    new()
                    {
                        Text = "Not our business. We just got here.", Next = "charges",
                        Effects = new[] { DialogueEffect.Add(Meters.Bond, -3), DialogueEffect.Add(Meters.Ambition, 2) },
                    },
                },
            },
            new() { Id = "talked", Speaker = Speakers.Narrator, Line = "The thugs look at each other, then at the two of you, and back off." },
            new() { Speaker = Speakers.Narrator, Line = "Then their leader comes back, with friends.", Next = "fight" },
            new() { Id = "not-talked", Speaker = "red-gull", Line = "Talk's free. Tax isn't.", Next = "fight" },
            new() { Id = "charges", Speaker = SoulBound, Line = "It's exactly our business. That's Lord Harrow's men with different hats." },
            new() { Speaker = Speakers.Narrator, Line = "{sb} charges in. There's no stopping it now." },

            new()
            {
                Id = "fight", Speaker = Speakers.Card, Line = "The Red Gulls are beaten. Bosun Grell goes down last.",
                Effects = new[] { DialogueEffect.Set(GullsBeaten) },
            },
            new() { Speaker = Speakers.Narrator, Line = "Grell's on his knees. {sb} keeps a blade at his throat a beat too long." },
            new() { Speaker = "grell", Line = "Mercy! It was just coin!" },
            new()
            {
                Speaker = SoulBound, Direction = "cold, quiet", Line = "That's what they always say. 'Just coin.' 'Just the nets.' 'Just a tax.'",
                Options = new DialogueOption[]
                {
                    new() { Text = "{sb}. Enough. He's done.", Next = "enough", Effects = new[] { DialogueEffect.Add(Meters.Bond, 3) } },
                    new()
                    {
                        Text = "(Say nothing.)", Next = "kicked",
                        Effects = new[] { DialogueEffect.Add(Meters.Ambition, 3), DialogueEffect.Set(DockCruelty) },
                    },
                    new() { Text = "Let the Watch decide what he's owed.", Next = "watch" },
                },
            },
            new() { Id = "enough", Speaker = Speakers.Narrator, Line = "{sb} lowers the blade." },
            new() { Speaker = SoulBound, Direction = "breathing out, then a smile", Line = "Right. Right. Sorry. Old ghosts.", Next = "hale" },
            new()
            {
                Id = "kicked", Speaker = Speakers.Narrator, Line = "{sb} plants a boot on Grell's chest and kicks him off the pier into the harbour.",
                Effects = new[] { DialogueEffect.Set(GrellGone) }, Next = "hale",
            },
            new() { Id = "watch", Speaker = SoulBound, Line = "The Watch. Sure. Let's see if the Watch is worth anything here." },

            new() { Id = "hale", Speaker = Speakers.Narrator, Line = "Whistles. The City Watch comes down the pier at a run, led by Captain Roderic Hale.", Effects = new[] { DialogueEffect.Set(GrellGone) } },
            new() { Speaker = "hale", Line = "Red Gulls, down in one go. And not a single one of my men had to get wet. Who are you two?" },
            new() { Speaker = You, Line = "{name}. And {sb}. We just landed." },
            new() { Speaker = "hale", Line = "Then welcome to Questeria. We're stretched thin. Half my Watch got sent east to the Greywatch garrison, and every rat in the Low Ring knows it. If you're looking for work, the quest board in the Lantern Market pays honest coin." },
            new() { Speaker = SoulBound, Line = "Sent east? What's in the east?" },
            new() { Speaker = "hale", Direction = "a beat", Line = "Rumours. Above my rank. Go make some coin." },
            new()
            {
                Speaker = "tomas", Line = "Bless you both. Here, it's not much.",
                Effects = new[] { DialogueEffect.Give("tomas-hook") },
            },
            new()
            {
                Speaker = SoulBound, Direction = "walking away", Line = "Did you see his face? That's why we do this. Not the coin. That.",
                Effects = new[] { DialogueEffect.Set(DocksDone), DialogueEffect.Objective("Find the quest board in the Lantern Market, up in the city") },
            },
        },
    };

    /// <summary>A1-C1-02: Questmaster Fennick and the board, and the Mirewood contract.</summary>
    public static readonly DialogueScript LanternMarket = new()
    {
        Id = "a1-c1-02",
        With = "fennick",
        Unless = new[] { BoardRead },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "fennick", Board = true, Line = "New faces! Contracts are posted by the city, the guilds, and anyone with a coin and a problem. Monsters on the left, bandits in the middle, lost-and-found on the right." },
            new() { Speaker = Speakers.Narrator, Board = true, Line = "One posting glows among the rest." },
            new() { Speaker = "fennick", Board = true, Line = "That one's from the Rangers' Lodge. Something's wrong in the Mirewood. Animals gone mad, woodcutters missing. Pays triple." },
            new()
            {
                Speaker = SoulBound, Board = true, Line = "Triple? Sold.",
                Effects = new[] { DialogueEffect.Set(BoardRead), DialogueEffect.Objective("Take the Rangers' contract: through the city gate and down the old road to the Mirewood") },
            },
        },
    };

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
