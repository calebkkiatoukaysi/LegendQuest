using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// Act 1, Chapter 3, "Ashes at Thornbridge": the refugees shut out at the east gate and Sergeant
/// Brisk's orders, Mira Ashgrove at the healer's station, her remedies for the sick, the Hollowed
/// raid, and Mira joining.
/// </summary>
/// <remarks>
/// A1-C3-01 and A1-C3-02 are compiled from Story/04-script-prologue-act1.md into StoryPrologueAct1;
/// this class keeps their flags by name, and the scenes 04 leaves as built: the three sick refugees Mira's
/// remedies go to, and what Brisk, Hedda and her daughter say when you talk to them again. Mira
/// tells her story about Maleficio either straight away or once the remedies are handed out, so both
/// of her scripts end on the same lines (Maleficio); the last of them sounds the horns and sets
/// mira-supplies-done, which brings the Hollowed out of the treeline, and walking up to them starts
/// the raid: a few lines, then the fight (EncounterCatalog's a1-c3-02-raid, with Mira beside you and
/// the Soul-Bound's bark when the second wave comes), and winning it plays the rest.
/// </remarks>
public static class Act1Chapter3
{
    private const string SoulBound = Origins.SoulBound;

    /// <summary>All three remedies are handed out: Mira's waiting to hear.</summary>
    private const string SuppliesDelivered = "supplies-delivered";

    /// <summary>Brisk's said his piece about the raid.</summary>
    private const string BriskAshamed = "brisk-ashamed";

    /// <summary>The gate scene's over: Mira's at the healer's station.</summary>
    public const string GateDone = "a1-c3-01";

    /// <summary>The flag A1-C3-01 sets when you talk Brisk into letting the sick through the gate.</summary>
    public const string BriskPersuaded = "brisk-persuaded";

    /// <summary>The flag A1-C3-01 sets when the Soul-Bound shoves Brisk and a sword comes out.</summary>
    public const string BriskShoved = "brisk-shoved";

    /// <summary>The flag A1-C3-02 sets when you flirt with Mira. Her romance reads it.</summary>
    public const string MiraFlirt = "mira-flirt-1";

    /// <summary>You offered to help Mira first: her remedies are yours to hand out.</summary>
    public const string Supplies = "mira-supplies";

    /// <summary>The old farmer has his remedy.</summary>
    public const string SuppliesOne = "supplies-1";

    /// <summary>The feverish boy has his remedy.</summary>
    public const string SuppliesTwo = "supplies-2";

    /// <summary>The glassblower has her salve.</summary>
    public const string SuppliesThree = "supplies-3";

    /// <summary>Mira's told her story and the horns have sounded: the Hollowed are at the treeline.</summary>
    public const string SuppliesDone = "mira-supplies-done";

    /// <summary>The Hollowed raid is beaten: they leave the treeline.</summary>
    public const string RaidBeaten = "hollowed-raid-beaten";

    /// <summary>The flag A1-C3-02 sets when the Soul-Bound speaks to the camp and it cheers their name. A1-C6-04 and Act 3's coup read it.</summary>
    public const string CrowdLoved = "sb-crowd-loved";

    /// <summary>Mira's joined: she leaves the healer's station.</summary>
    public const string MiraRecruited = "mira-recruited";

    /// <summary>The chapter's over.</summary>
    public const string ChapterDone = "a1-c3";

    /// <summary>Brisk at the gate before the raid, as it went between you.</summary>
    /// <remarks>Ahead of Townspeople's Brisk in the catalogue, so it wins.</remarks>
    public static readonly DialogueScript BriskCamp = new()
    {
        Id = "a1-c3-brisk",
        With = "brisk",
        Needs = new[] { GateDone },
        Unless = new[] { RaidBeaten },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "brisk", Line = "The sick are up at the temple. Sister Hesper's got them. If the Chancellor asks, I tripped and the gate fell open.",
                Conditions = new[] { DialogueCondition.Is(BriskPersuaded) },
            },
            new()
            {
                Speaker = "brisk", Direction = "not quite looking at you", Line = "Keep your friend away from my gate, {name}. Next time it won't be a recruit with a sword out.",
                Conditions = new[] { DialogueCondition.Is(BriskShoved) },
            },
            new()
            {
                Speaker = "brisk", Line = "I don't like it either. For what that's worth. Which is nothing, out here.",
                Conditions = new[] { DialogueCondition.Not(BriskPersuaded), DialogueCondition.Not(BriskShoved) },
            },
            new() { Speaker = "brisk", Line = "The dog's gone over to the children. Traitor." },
        },
    };

    /// <summary>Brisk after the raid: what he watched from the wall, and the dog that didn't wait for orders.</summary>
    /// <remarks>Ahead of Townspeople's Brisk in the catalogue, so it wins. Once it's said, he's back to his old lines.</remarks>
    public static readonly DialogueScript BriskAfterRaid = new()
    {
        Id = "a1-c3-brisk-after",
        With = "brisk",
        Needs = new[] { RaidBeaten },
        Unless = new[] { BriskAshamed },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "brisk", Direction = "his helm under his arm", Line = "Forty of them. I counted. I stood up on that wall and I counted." },
            new() { Speaker = "brisk", Line = "The dog went over the wall at them. Didn't wait for orders. Came back with a Hollowed's boot in her teeth." },
            new()
            {
                Speaker = "brisk", Line = "'The dog's the best of us.' I used to say it for a laugh.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You had orders.", Next = "orders" },
                    new() { Text = "Next time, come out.", Next = "next-time" },
                    new()
                    {
                        Text = "Then be better than your orders, Sergeant.", Next = "better",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Honorable) }, Locked = true,
                    },
                },
            },
            new() { Id = "orders", Speaker = "brisk", Line = "Your friend's right about that word. It tastes worse every time I say it.", Next = "go" },
            new() { Id = "next-time", Speaker = "brisk", Line = "Next time I will. Orders or no.", Next = "go" },
            new() { Id = "better", Speaker = "brisk", Direction = "a long look", Line = "...Aye. I'll try that." },
            new()
            {
                Id = "go", Speaker = "brisk", Line = "Go on. Mind yourselves.",
                Effects = new[] { DialogueEffect.Set(BriskAshamed) },
            },
        },
    };

    /// <summary>Hedda, the mother at the gate, before the raid.</summary>
    public static readonly DialogueScript MotherCamp = new()
    {
        Id = "a1-c3-hedda",
        With = "refugee-mother",
        Needs = new[] { GateDone },
        Unless = new[] { RaidBeaten },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "refugee-mother", Line = "I'm Hedda. That's Lise. She's eating, for once, and she's made a friend of the sergeant's dog. Thank you." },
            new() { Speaker = "refugee-mother", Line = "We kept bees, out past the glass city. The bees went first. Then the neighbours. Then we walked." },
            new()
            {
                Speaker = "refugee-mother", Line = "They took my father through with the sick. A sister at your temple gave him a blanket with flowers on it. He cried.",
                Conditions = new[] { DialogueCondition.Is(BriskPersuaded) },
            },
            new()
            {
                Speaker = "refugee-mother", Line = "Lise thinks you're soldiers. I told her soldiers don't share their bread.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Is there anything else you need?", Next = "need" },
                    new() { Text = "Rest. We'll keep watch.", Next = "rest" },
                },
            },
            new() { Id = "need", Speaker = "refugee-mother", Direction = "a small laugh", Line = "Bread. Sleep. A door that shuts. In that order.", Next = DialogueScript.End },
            new() { Id = "rest", Speaker = "refugee-mother", Line = "Nobody's said that to me in three weeks." },
        },
    };

    /// <summary>Hedda after the raid.</summary>
    public static readonly DialogueScript MotherAfter = new()
    {
        Id = "a1-c3-hedda-after",
        With = "refugee-mother",
        Needs = new[] { RaidBeaten },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "refugee-mother", Line = "I heard the horns. I put her under a cart and lay on top of her and I didn't breathe." },
            new()
            {
                Speaker = "refugee-mother", Line = "And then {sb} stood up on that cart and said nobody was coming, so you would. Half the camp's been saying {sb_their} name since. Like a prayer.",
                Conditions = new[] { DialogueCondition.Is(CrowdLoved) },
            },
            new()
            {
                Speaker = "refugee-mother", Line = "You came. Nobody else came. I won't forget it.",
                Conditions = new[] { DialogueCondition.Not(CrowdLoved) },
            },
        },
    };

    /// <summary>Lise, Hedda's daughter, before the raid.</summary>
    public static readonly DialogueScript ChildCamp = new()
    {
        Id = "a1-c3-lise",
        With = "refugee-child",
        Needs = new[] { GateDone },
        Unless = new[] { RaidBeaten },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "refugee-child", Direction = "her mouth full", Line = "Is the big dog yours? It's not yours. It's the soldier's. It likes me better." },
        },
    };

    /// <summary>Lise after the raid.</summary>
    public static readonly DialogueScript ChildAfter = new()
    {
        Id = "a1-c3-lise-after",
        With = "refugee-child",
        Needs = new[] { RaidBeaten },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "refugee-child", Line = "The dog bit one. A grey one. On the bottom." },
            new() { Speaker = "refugee-child", Line = "When I'm big, I'm going to be {sb}.", Conditions = new[] { DialogueCondition.Is(CrowdLoved) } },
            new() { Speaker = "refugee-child", Line = "When I'm big, I'm going to have a dog.", Conditions = new[] { DialogueCondition.Not(CrowdLoved) } },
        },
    };

    /// <summary>Mira while her remedies are still being handed out.</summary>
    public static readonly DialogueScript MiraWaiting = new()
    {
        Id = "a1-c3-02-waiting",
        With = "mira",
        Needs = new[] { Supplies },
        Unless = new[] { SuppliesDelivered },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "mira", Direction = "not looking up from the cot", Line = "The farmer, the boy and the glassblower. Down by the water. Thank you." },
        },
    };

    /// <summary>The old farmer's cough remedy.</summary>
    public static readonly DialogueScript SupplyOne = new()
    {
        Id = "a1-c3-02-supplies-1",
        With = "sick-1",
        Needs = new[] { Supplies },
        Unless = new[] { SuppliesOne },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "refugee-1", Direction = "coughing", Line = "Mira sent you? Bless that girl. She'll heal the whole camp and forget to eat." },
            new() { Speaker = Speakers.Narrator, Line = "He drinks the remedy, makes a face, and drinks the rest." },
            new() { Speaker = "refugee-1", Line = "Forty years I grew apples behind the Ember Archive. Best in the east. Then the trees grew thorns, inward, right through the fruit." },
            new() { Speaker = SoulBound, Line = "What did you do?" },
            new()
            {
                Speaker = "refugee-1", Line = "Burned them. My own trees. Then I walked.",
                Effects = new[] { DialogueEffect.Set(SuppliesOne) },
            },
            LastRemedy(SuppliesTwo, SuppliesThree),
        },
    };

    /// <summary>The feverish boy's willowbark.</summary>
    public static readonly DialogueScript SupplyTwo = new()
    {
        Id = "a1-c3-02-supplies-2",
        With = "sick-2",
        Needs = new[] { Supplies },
        Unless = new[] { SuppliesTwo },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "refugee-2", Direction = "shivering under a blanket", Line = "Is that Mira's? It tastes like a tree." },
            new() { Speaker = SoulBound, Line = "It's willowbark. Drink it anyway." },
            new() { Speaker = "refugee-2", Line = "My brother went out to see what the horns were. At night. He came back the next morning, grey all over, and walked straight past me." },
            new() { Speaker = "refugee-2", Line = "Didn't know my name." },
            new() { Speaker = SoulBound, Direction = "quietly", Line = "...I'm sorry." },
            new()
            {
                Speaker = "refugee-2", Line = "If you ever see a grey one with a blue ribbon round his wrist... don't tell me. Just don't tell me.",
                Effects = new[] { DialogueEffect.Set(SuppliesTwo) },
            },
            LastRemedy(SuppliesOne, SuppliesThree),
        },
    };

    /// <summary>The glassblower's salve.</summary>
    public static readonly DialogueScript SupplyThree = new()
    {
        Id = "a1-c3-02-supplies-3",
        With = "sick-3",
        Needs = new[] { Supplies },
        Unless = new[] { SuppliesThree },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "refugee-3", Line = "Mira's salve? Oh, thank the light. Careful. Careful... There." },
            new() { Speaker = "refugee-3", Line = "I made windows for the Ember Archive. Coloured glass, the whole east wall. You could read by the light of it." },
            new() { Speaker = "refugee-3", Line = "Mira used to sit under my window and read to the children. She did all the voices." },
            new()
            {
                Speaker = "refugee-3", Line = "When the Hollowed fired the workshops, I went back in for my pipes. Stupid. You can always make more pipes.",
                Effects = new[] { DialogueEffect.Set(SuppliesThree) },
            },
            LastRemedy(SuppliesOne, SuppliesTwo),
        },
    };

    /// <summary>The old farmer once he's had his remedy.</summary>
    public static readonly DialogueScript SickOneAfter = new()
    {
        Id = "a1-c3-02-supplies-1-after",
        With = "sick-1",
        Needs = new[] { SuppliesOne },
        Nodes = new DialogueNode[] { new() { Speaker = "refugee-1", Line = "Bless that girl. And bless you, while I'm at it." } },
    };

    /// <summary>The boy once he's had his willowbark.</summary>
    public static readonly DialogueScript SickTwoAfter = new()
    {
        Id = "a1-c3-02-supplies-2-after",
        With = "sick-2",
        Needs = new[] { SuppliesTwo },
        Nodes = new DialogueNode[] { new() { Speaker = "refugee-2", Line = "Still tastes like a tree. Still drinking it." } },
    };

    /// <summary>The glassblower once she's had her salve.</summary>
    public static readonly DialogueScript SickThreeAfter = new()
    {
        Id = "a1-c3-02-supplies-3-after",
        With = "sick-3",
        Needs = new[] { SuppliesThree },
        Nodes = new DialogueNode[] { new() { Speaker = "refugee-3", Line = "I can feel my fingers again. All ten. I counted." } },
    };

    /// <summary>A1-C3-02, once the remedies are handed out: Mira sits down at last and tells you.</summary>
    public static readonly DialogueScript MiraStory = new()
    {
        Id = "a1-c3-02-story",
        With = "mira",
        Needs = new[] { SuppliesDelivered },
        Unless = new[] { SuppliesDone },
        Marker = MarkerKind.Event,
        Nodes = Join(
            new DialogueNode[]
            {
                new() { Speaker = "mira", Line = "The farmer, the boy and the glassblower. All three?" },
                new() { Speaker = SoulBound, Line = "All three. The farmer says you forget to eat." },
                new() { Speaker = "mira", Direction = "sitting down at last", Line = "The farmer's right. Sit with me a moment." },
                new() { Speaker = "mira", Line = "I said I was an archivist, before. You were kind enough not to ask before what." },
            },
            Maleficio()),
    };

    /// <summary>Mira once the horns have gone, if you talk to her instead of going.</summary>
    public static readonly DialogueScript MiraReady = new()
    {
        Id = "a1-c3-02-ready",
        With = "mira",
        Needs = new[] { SuppliesDone },
        Unless = new[] { RaidBeaten },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "mira", Direction = "gold light on her hands", Line = "Go! I'll keep them all behind me. Go!" },
        },
    };

    /// <summary>A1-C3-02's raid up to the fight: the Hollowed at the treeline, and Mira coming up beside you.</summary>
    public static readonly DialogueScript Raid = new()
    {
        Id = "a1-c3-02-raid",
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator, Sound = Sfx.WarHorn,
                Line = "The horns again, close now. Footmen come out of the treeline with rusted swords, and behind them a Hexer in rags, chanting, black light gathering in her hands.",
            },
            new()
            {
                Speaker = "mira", Direction = "stepping up beside you", Line = "I'm not hiding behind the carts. Not again.",
                Effects = new[] { DialogueEffect.Fight("a1-c3-02-raid") },
            },
        },
    };

    /// <summary>The heart of A1-C3-02, shared by both ways you hear it: Mira on Maleficio, the Soul-Bound's slip, and the horns.</summary>
    /// <remarks>
    /// Story/04's #before lines, word for word, so the errands way hears what the meeting's "Before
    /// what?" does. Starts on the line called before, which that option goes to. Its last line brings
    /// the Hollowed out.
    /// </remarks>
    private static DialogueNode[] Maleficio() => new DialogueNode[]
    {
        new() { Id = "before", Speaker = "mira", Line = "Before my teacher opened the Box. Maleficio. He wanted to end the famines in the east, and for a while he did." },
        new() { Speaker = "mira", Line = "First he needed enough power to fix the harvest. Then enough to fix the council. Then everything after that." },
        new() { Speaker = "mira", Line = "Every step made sense when he took it. That's the part I can't get past." },
        new() { Speaker = SoulBound, Line = "The council, though. Maybe he wasn't wrong about the council.", Cues = new[] { StageCue.Camera("soulbound") } },
        new() { Speaker = "mira", Expression = "angry", Line = "He burned the council. In their hall. With their children at the windows." },
        new() { Speaker = SoulBound, Expression = "worried", Direction = "hands up", Line = "I didn't mean that. I meant the arguing. I'm sorry." },
        new()
        {
            Speaker = Speakers.Narrator, Sound = Sfx.WarHorn, Line = "Horns in the treeline. Grey shapes come out of the trees toward the tents.",
            Cues = new[] { StageCue.Sfx("war-horn") },
            Effects = new[] { DialogueEffect.Set(SuppliesDone), DialogueEffect.Objective("Stop the Hollowed at the treeline before they reach the camp") },
        },
    };

    /// <summary>The line a remedy script ends on when it was the last of the three: back to Mira.</summary>
    /// <param name="other">One of the other two remedies' flags.</param>
    /// <param name="another">The other.</param>
    private static DialogueNode LastRemedy(string other, string another) => new()
    {
        Speaker = SoulBound, Line = "That's the last of them. Back to Mira.",
        Conditions = new[] { DialogueCondition.Is(other), DialogueCondition.Is(another) },
        Effects = new[] { DialogueEffect.Set(SuppliesDelivered), DialogueEffect.Objective("Go back to Mira at the healer's station") },
    };

    /// <summary>Two runs of lines as one script's: the first, then the second.</summary>
    private static DialogueNode[] Join(DialogueNode[] first, DialogueNode[] second)
    {
        var joined = new DialogueNode[first.Length + second.Length];
        first.CopyTo(joined, 0);
        second.CopyTo(joined, first.Length);
        return joined;
    }
}
