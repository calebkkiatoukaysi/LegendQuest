using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// Act 1, Chapter 4, "The Sunheart": the royal messenger, Princess Elara in the Flagon's back booth,
/// Kael in the Old Crown Vault, the audience with King Leon, Chancellor Crane in the great hall, and
/// Brannock Ironvein at the Rusty Anvil.
/// </summary>
/// <remarks>
/// A1-C4-01 to A1-C4-05 are compiled from Story/04-script-prologue-act1.md into StoryPrologueAct1;
/// this class keeps their flags by name, and the scenes 04 leaves as built: the messenger who finds you after
/// Thornbridge (A1-C4-00), and what the King, the Princess, the Chancellor, the royal guards and the
/// Anvil's barkeep say when you talk to them again. The vault is three conversations round two
/// fights: Kael, then his duel (EncounterCatalog's a1-c4-02-kael, which he gives up at 30%); the
/// sappers breaking in and the truce, then the sappers with Kael beside you; and the Sunheart handed
/// over, the coin, and Kael joining. The party's full by now, so Kael and Brannock go to wait at the
/// Gilded Flagon when they join. Mira is always in the party in the booth and the vault (nobody's
/// waiting yet to swap with her); at court she may be waiting at the Flagon, and she comes up to
/// speak anyway.
/// </remarks>
public static class Act1Chapter4
{
    private const string SoulBound = Origins.SoulBound;

    /// <summary>The messenger's found you and pointed you at the Flagon.</summary>
    public const string MessengerMet = "a1-c4-00";

    /// <summary>Elara's asked for the Sunheart: the vault door in the terrace wall opens.</summary>
    public const string SummonsDone = "a1-c4-01";

    /// <summary>The Hollowed sappers have broken into the vault and Kael's called a truce: the sappers appear.</summary>
    public const string KaelTruce = "kael-truce";

    /// <summary>The sappers are beaten: they leave the vault.</summary>
    public const string SappersBeaten = "sappers-beaten";

    /// <summary>The flag A1-C4-02 sets when you flirt with Kael. His romance reads it.</summary>
    public const string KaelFlirt = "kael-flirt-1";

    /// <summary>The flag A1-C4-02 sets when Kael describes the buyer who paid in Doomward coin. A1-C7-03 reads it.</summary>
    public const string SpySeed = "spy-seed-1";

    /// <summary>Kael's joined: he leaves the relic shelf.</summary>
    public const string KaelRecruited = "kael-recruited";

    /// <summary>The vault's done and the Sunheart's yours to carry: the palace door opens.</summary>
    public const string VaultDone = "a1-c4-02";

    /// <summary>The flag A1-C4-03 sets when you back the Soul-Bound in front of the King.</summary>
    public const string BackedSbCourt = "mc-backed-sb-court";

    /// <summary>The audience is over: Crane waits in the great hall, and the Rusty Anvil's door opens.</summary>
    public const string AudienceDone = "a1-c4-03";

    /// <summary>The flag A1-C4-04 sets: Crane has the Soul-Bound's ear. A1-C7-03 reads it.</summary>
    public const string CraneContact = "crane-contact";

    /// <summary>Crane's said his piece in the great hall: he leaves it.</summary>
    public const string CraneDone = "a1-c4-04";

    /// <summary>The flag A1-C4-05 sets when Brannock hears the Soul-Bound talk like a warlord. Act 3's tracker reads it.</summary>
    public const string BrannockWary = "brannock-wary-sb";

    /// <summary>Brannock's joined: he leaves the Anvil's corner table.</summary>
    public const string BrannockRecruited = "brannock-recruited";

    /// <summary>The chapter's over: the caravan for Greywatch leaves at dawn.</summary>
    public const string ChapterDone = "a1-c4";

    /// <summary>A1-C4-00: a messenger in royal livery finds you in the street and sends you to the Flagon's back booth.</summary>
    public static readonly DialogueScript Messenger = new()
    {
        Id = "a1-c4-00",
        With = "messenger",
        Needs = new[] { Act1Chapter3.ChapterDone },
        Unless = new[] { MessengerMet, SummonsDone },
        Marker = MarkerKind.Event,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Chapter, Line = "Chapter 4  ·  The Sunheart" },
            new()
            {
                Speaker = Speakers.Narrator,
                Line = "A young man in royal blue and gold is working his way along the street, checking every face against a note in his glove. He gets to yours, and his whole face lights up.",
            },
            new() { Speaker = "royal-messenger", Line = "You'll be the ones from Thornbridge. Oh, thank goodness. I've been up and down this street four times." },
            new()
            {
                Speaker = "royal-messenger", Direction = "beaming", Line = "They're singing {sb}'s name outside the east gate, did you know? Badly. But loudly.",
                Conditions = new[] { DialogueCondition.Is(Act1Chapter3.CrowdLoved) },
            },
            new() { Speaker = "royal-messenger", Line = "Somebody would like a word. Not here. The Gilded Flagon, the back booth. Come as you are, and come quietly." },
            new() { Speaker = SoulBound, Line = "Somebody?" },
            new()
            {
                Speaker = "royal-messenger", Line = "Somebody who pays my wages, and doesn't like waiting. I'd hurry. I'd hurry a lot.",
                Effects = new[] { DialogueEffect.Set(MessengerMet), DialogueEffect.Objective("Meet whoever's waiting in the Gilded Flagon's back booth") },
            },
        },
    };

    /// <summary>The messenger while you've still not been to the booth.</summary>
    public static readonly DialogueScript MessengerWaiting = new()
    {
        Id = "a1-c4-00-after",
        With = "messenger",
        Needs = new[] { MessengerMet },
        Unless = new[] { SummonsDone },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "royal-messenger", Line = "The Flagon. The back booth. Please hurry. If you're late, it's me that's in trouble." },
        },
    };

    /// <summary>King Leon once the audience is over.</summary>
    public static readonly DialogueScript LeonAfter = new()
    {
        Id = "a1-c4-leon",
        With = "leon",
        Needs = new[] { AudienceDone },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "leon", Direction = "kindly, and tired", Line = "Dawn, at the east gate. Bring Aldric's report back yourselves. I'd like to see your faces when you do." },
        },
    };

    /// <summary>Princess Elara once the audience is over, with the Sunheart in her hands.</summary>
    public static readonly DialogueScript ElaraAfter = new()
    {
        Id = "a1-c4-elara",
        With = "elara",
        Needs = new[] { AudienceDone },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "elara", Direction = "turning the Sunheart in her hands", Line = "Back in the wall by tonight, and nobody the wiser. I owe you. I hate owing people. Go to Greywatch and let me pay it back." },
        },
    };

    /// <summary>The Lord Chancellor back in the throne room once he's had his word with the Soul-Bound.</summary>
    public static readonly DialogueScript CraneAfter = new()
    {
        Id = "a1-c4-crane",
        With = "crane-court",
        Needs = new[] { CraneDone },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "crane", Direction = "smiling", Line = "Off to Greywatch? How thrilling. Do write. I so rarely get letters from anyone interesting." },
        },
    };

    /// <summary>The royal guard on the Chancellor's side of the throne.</summary>
    public static readonly DialogueScript KeepGuard = new()
    {
        Id = "a1-c4-guard-2",
        With = "guard-2",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "royal-guard", Line = "Twelve years I've stood at this door. Never once seen His Majesty bow his head to anyone." },
        },
    };

    /// <summary>The royal guard on the Princess's side of the throne: her own, who never knows where she is.</summary>
    public static readonly DialogueScript ElaraGuard = new()
    {
        Id = "a1-c4-guard-1",
        With = "guard-1",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "royal-guard", Line = "I'm Her Highness's guard. Officially. She was at the Flagon twice this week, and I found out from the cook." },
        },
    };

    /// <summary>The Rusty Anvil's barkeep, before Brannock's gone.</summary>
    public static readonly DialogueScript Barkeep = new()
    {
        Id = "a1-c4-barkeep",
        With = "barkeep",
        Unless = new[] { BrannockRecruited },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "anvil-barkeep", Line = "After the big one? Corner table. Don't wake him sudden. He wakes swinging." },
        },
    };

    /// <summary>The barkeep once Brannock's gone.</summary>
    public static readonly DialogueScript BarkeepAfter = new()
    {
        Id = "a1-c4-barkeep-after",
        With = "barkeep",
        Needs = new[] { BrannockRecruited },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "anvil-barkeep", Line = "Eleven months he sat in that corner. Feels wrong, seeing it empty. Feels good, too." },
        },
    };
}
