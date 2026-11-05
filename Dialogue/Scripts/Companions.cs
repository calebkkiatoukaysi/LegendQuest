using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// The companions' own conversations for Act 1, Chapters 2 to 4: what the Soul-Bound, Sera, Mira,
/// Kael and Brannock say when you walk up to them between the story's scenes.
/// </summary>
/// <remarks>
/// Mine, not the story's, written to COMPANIONS.md: each companion's wound, voice and approval table
/// are there, and every approval number here comes from those tables. Talking to a follower plays
/// the first of their conversations in DialogueCatalog that can start, so each person's play in
/// order: each needs a flag from the story and stops once it's set its own. They're written in the
/// past tense about the scenes before them, so one that's waited a chapter still reads true, and
/// where it matters they check what you chose (the crystal, the crowd, the court) and say something
/// different. Each companion's personal quest is hooked here with a flag the later chapters read.
/// The story's own flags are named here too, as the scripts for Chapters 2 to 4 set them, so these
/// and the banters can read them.
/// </remarks>
public static class Companions
{
    private const string You = Origins.You;
    private const string SoulBound = Origins.SoulBound;
    private const string Sera = "sera";
    private const string Mira = "mira";
    private const string Kael = "kael";
    private const string Brannock = "brannock";

    // Each companion's approval, and Kael's hidden loyalty to the Soul-Bound, by id in Meters.
    private const string SeraApproval = "affinity-sera";
    private const string MiraApproval = "affinity-mira";
    private const string KaelApproval = "affinity-kael";
    private const string KaelLoyalty = "affinity-kael-sb";
    private const string BrannockApproval = "affinity-brannock";

    /// <summary>The approval a companion needs before their second flirt is offered: a little past where they start, so it follows something you did beyond the first.</summary>
    private const int RomanceSeed = 25;

    // The story's flags, as Chapters 2 to 4 set them.

    /// <summary>Sera's joined (A1-C2-01).</summary>
    public const string SeraRecruited = "sera-recruited";

    /// <summary>You flirted with Sera when you met her (A1-C2-01).</summary>
    public const string SeraFlirt = "sera-flirt-1";

    /// <summary>The black crystal in the quarry's been purified, shattered or guarded while Sera purified it (A1-C2-02).</summary>
    public const string CrystalDealtWith = "crystal-dealt-with";

    /// <summary>The crystal was purified, by Sera alone or with you guarding her (A1-C2-02).</summary>
    public const string CrystalPurified = "crystal-purified";

    /// <summary>The crystal was shattered, and the black ran into the stream (A1-C2-02).</summary>
    public const string CrystalShattered = "crystal-shattered";

    /// <summary>You and the Soul-Bound held the quarry while Sera purified the crystal (A1-C2-02).</summary>
    public const string CrystalGuarded = "crystal-guarded";

    /// <summary>The night camp in the Mirewood is over (A1-C2-03).</summary>
    public const string MirewoodCamp = "a1-c2-03";

    /// <summary>Fennick's paid for the Mirewood and sent you to the east gate (A1-C2-04).</summary>
    public const string FennickPaid = "a1-c2-04";

    /// <summary>The scene at Questeria's east gate is over (A1-C3-01).</summary>
    public const string EastGate = "a1-c3-01";

    /// <summary>The Soul-Bound shoved Sergeant Brisk at the east gate (A1-C3-01).</summary>
    public const string BriskShoved = "brisk-shoved";

    /// <summary>The Hollowed raid on Thornbridge is beaten (A1-C3-02).</summary>
    public const string RaidBeaten = "hollowed-raid-beaten";

    /// <summary>You let the Soul-Bound speak to the refugees, and they cheered {sb}'s name (A1-C3-02).</summary>
    public const string CrowdLoved = "sb-crowd-loved";

    /// <summary>Mira's joined (A1-C3-02).</summary>
    public const string MiraRecruited = "mira-recruited";

    /// <summary>You flirted with Mira when you met her (A1-C3-02).</summary>
    public const string MiraFlirt = "mira-flirt-1";

    /// <summary>Chapter 3 is over: Thornbridge is held and Mira's with you.</summary>
    public const string Thornbridge = "a1-c3";

    /// <summary>Princess Elara's asked you to find the Sunheart (A1-C4-01).</summary>
    public const string Summoned = "a1-c4-01";

    /// <summary>Kael's joined (A1-C4-02).</summary>
    public const string KaelRecruited = "kael-recruited";

    /// <summary>You flirted with Kael in the vault (A1-C4-02).</summary>
    public const string KaelFlirt = "kael-flirt-1";

    /// <summary>The Old Crown Vault is done and the Sunheart's back (A1-C4-02).</summary>
    public const string VaultDone = "a1-c4-02";

    /// <summary>The audience with King Leon is over (A1-C4-03).</summary>
    public const string CourtDone = "a1-c4-03";

    /// <summary>You backed the Soul-Bound in front of the king (A1-C4-03).</summary>
    public const string BackedAtCourt = "mc-backed-sb-court";

    /// <summary>Chancellor Crane caught the Soul-Bound in the hall (A1-C4-04).</summary>
    public const string CraneContact = "crane-contact";

    /// <summary>The scene with Crane is over (A1-C4-04).</summary>
    public const string CraneDone = "a1-c4-04";

    /// <summary>Brannock's joined (A1-C4-05).</summary>
    public const string BrannockRecruited = "brannock-recruited";

    /// <summary>Brannock heard the Soul-Bound talk like a warlord, and said so (A1-C4-05).</summary>
    public const string BrannockWary = "brannock-wary-sb";

    /// <summary>Chapter 4 is over: Brannock's with you and the caravan leaves at dawn.</summary>
    public const string Chapter4Done = "a1-c4";

    // Mine: each conversation's own flag, so it only plays once.

    /// <summary>The Soul-Bound's talked about the courier's letter and the skiff.</summary>
    public const string SbTalk1 = "sb-talk-1-done";

    /// <summary>The Soul-Bound's talked about Thornbridge.</summary>
    public const string SbTalk2 = "sb-talk-2-done";

    /// <summary>The Soul-Bound's talked about the court, Crane and Lord Harrow.</summary>
    public const string SbTalk3 = "sb-talk-3-done";

    /// <summary>Sera's talked about her reach and the count.</summary>
    public const string SeraTalk1 = "sera-talk-1-done";

    /// <summary>Sera's talked about the crystal and the last silverbark.</summary>
    public const string SeraTalk2 = "sera-talk-2-done";

    /// <summary>Sera's told you what she saw in the Soul-Bound at Thornbridge.</summary>
    public const string SeraTalk3 = "sera-talk-3-done";

    /// <summary>Mira's talked about her father and the Ember Index.</summary>
    public const string MiraTalk1 = "mira-talk-1-done";

    /// <summary>Mira's talked about the black-iron coin.</summary>
    public const string MiraTalk2 = "mira-talk-2-done";

    /// <summary>Mira's talked about the court.</summary>
    public const string MiraTalk3 = "mira-talk-3-done";

    /// <summary>Kael's talked about why he took the job.</summary>
    public const string KaelTalk1 = "kael-talk-1-done";

    /// <summary>Kael's talked about knights, thieves and his mother.</summary>
    public const string KaelTalk2 = "kael-talk-2-done";

    /// <summary>Brannock's talked about Greywatch and the letters.</summary>
    public const string BrannockTalk1 = "brannock-talk-1-done";

    // Mine: the personal quests' hooks, the second flirts, and choices later chapters read.

    /// <summary>The Heron's Due: the Soul-Bound saw Lord Harrow at court.</summary>
    public const string SbQuestHook = "sb-quest-hook";

    /// <summary>You told the Soul-Bound the cove could have stood up together. Act 3's dream-visit can recall it.</summary>
    public const string SbSharedCatch = "sb-shared-catch";

    /// <summary>The Soul-Bound asked you to say whenever they sound like Maleficio, and you promised.</summary>
    public const string SbTellMe = "sb-tell-me";

    /// <summary>The Last Silverbark: Sera's told you about the Hollins and the one tree left.</summary>
    public const string SeraQuestHook = "sera-quest-hook";

    /// <summary>You guessed, with a check, that someone kind once let Sera down.</summary>
    public const string SeraGuessed = "sera-surveyors-guessed";

    /// <summary>The second flirt with Sera.</summary>
    public const string SeraFlirt2 = "sera-flirt-2";

    /// <summary>The Ember Index: Mira's told you about her father and the half of the Index she left behind.</summary>
    public const string MiraQuestHook = "mira-quest-hook";

    /// <summary>The second flirt with Mira.</summary>
    public const string MiraFlirt2 = "mira-flirt-2";

    /// <summary>You told Mira that Crane courted the Soul-Bound. A1-C7-03 can let her suspect him.</summary>
    public const string MiraHeardCrane = "mira-heard-crane";

    /// <summary>A Name on a Door: Kael's told you about Mother Gannet and Tallow Lane.</summary>
    public const string KaelQuestHook = "kael-quest-hook";

    /// <summary>The second flirt with Kael.</summary>
    public const string KaelFlirt2 = "kael-flirt-2";

    /// <summary>Forty Letters: Brannock's told you about the letters he never sent, and Hester at Kestrel Ford.</summary>
    public const string BrannockQuestHook = "brannock-quest-hook";

    // What the crystal, the crowd and the court came to, for the lines that differ by them. These
    // come before the scripts, which read them as they're built.

    private static readonly DialogueCondition[] Guarded = { DialogueCondition.Is(CrystalGuarded) };
    private static readonly DialogueCondition[] PurifiedAlone = { DialogueCondition.Is(CrystalPurified), DialogueCondition.Not(CrystalGuarded) };
    private static readonly DialogueCondition[] Shattered = { DialogueCondition.Is(CrystalShattered) };
    private static readonly DialogueCondition[] NotShattered = { DialogueCondition.Not(CrystalShattered) };
    private static readonly DialogueCondition[] Loved = { DialogueCondition.Is(CrowdLoved) };
    private static readonly DialogueCondition[] NotLoved = { DialogueCondition.Not(CrowdLoved) };
    private static readonly DialogueCondition[] Backed = { DialogueCondition.Is(BackedAtCourt) };
    private static readonly DialogueCondition[] NotBacked = { DialogueCondition.Not(BackedAtCourt) };
    private static readonly DialogueCondition[] BeforeCourt = { DialogueCondition.Not(CourtDone) };
    private static readonly DialogueCondition[] AfterCourt = { DialogueCondition.Is(CourtDone) };

    /// <summary>The Soul-Bound after the night camp in the Mirewood: the courier's letter, the crystal, Sera watching, and the skiff in the hungry winter.</summary>
    public static readonly DialogueScript SoulBoundMirewood = new()
    {
        Id = "sb-talk-1",
        With = SoulBound,
        Needs = new[] { MirewoodCamp },
        Unless = new[] { SbTalk1 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = SoulBound, Direction = "quietly", Line = "That courier's letter. 'Let the rot soften Fable before we arrive.' I can't get it out of my head." },
            new()
            {
                Speaker = SoulBound, Line = "Somewhere east of here there's a man who looked at a map of this kingdom and picked which bits to rot first.",
                Options = new DialogueOption[]
                {
                    new() { Text = "And we found his rot first. That counts.", Next = "counts", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "Armies are a job for kings and generals. Not us.", Next = "generals", Effects = new[] { DialogueEffect.Add(Meters.Ambition, 2) } },
                    new()
                    {
                        Text = "Can the end of the world wait until after breakfast?", Next = "breakfast",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                        Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) },
                    },
                },
            },
            new() { Id = "counts", Speaker = SoulBound, Line = "It counts. It isn't enough, but it counts.", Next = "crystal" },
            new() { Id = "generals", Speaker = SoulBound, Line = "Kings and generals. Right. Let's hope one of them's awake.", Next = "crystal" },
            new() { Id = "breakfast", Speaker = SoulBound, Direction = "a laugh, despite everything", Line = "It can wait until after yours. Mine's long gone." },

            // The crystal, however it went.
            new() { Id = "crystal", Speaker = SoulBound, Conditions = Guarded, Line = "And the quarry. You and me in the mouth of it, Sera at the crystal, every rotten thing in the wood coming at us." },
            new() { Speaker = SoulBound, Conditions = Guarded, Line = "That was good. That was us. I'd hold a door with you anywhere." },
            new() { Speaker = SoulBound, Conditions = Shattered, Line = "And you backed me on the crystal. Smash it, done, home. Sera's still sore about her stream, mind." },
            new() { Speaker = SoulBound, Conditions = Shattered, Line = "She'll come round. Fast was right. I'm nearly sure fast was right." },
            new()
            {
                Speaker = SoulBound, Conditions = PurifiedAlone, Direction = "a little stiff",
                Line = "And you sided with Sera on the crystal. It's fine. Her forest, her call. I walked off to cool down. Sorry about that.",
                Options = new DialogueOption[]
                {
                    new() { Text = "It was the right call.", Next = "right-call" },
                    new() { Text = "I should have backed you.", Next = "backed", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "You walked off on your own. Don't do that.", Next = "alone", Effects = new[] { DialogueEffect.Add(Meters.Bond, 3) } },
                },
            },
            new() { Id = "right-call", Speaker = SoulBound, Conditions = PurifiedAlone, Line = "Probably. I just hate that right is so slow.", Next = "purified-end" },
            new() { Id = "backed", Speaker = SoulBound, Conditions = PurifiedAlone, Line = "No. You should do what you think's right. That's half of why I keep you around.", Next = "purified-end" },
            new() { Id = "alone", Speaker = SoulBound, Conditions = PurifiedAlone, Direction = "after a moment", Line = "...No. You're right. That's not us. Never leave the other behind, and all that." },
            new() { Id = "purified-end", Speaker = SoulBound, Conditions = PurifiedAlone, Line = "Anyway. The forest gets to heal. That's worth two days of me sulking." },

            // Sera, watching from the dark.
            new()
            {
                Speaker = SoulBound, Line = "Your ranger watches me, you know. Like I'm a wolf that hasn't made its mind up yet.",
                Options = new DialogueOption[]
                {
                    new() { Text = "She watches everyone. It's her job.", Next = "job" },
                    new() { Text = "She doesn't know you yet. Give her time.", Next = "time", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "Maybe she sees something.", Next = "sees" },
                },
            },
            new() { Id = "job", Speaker = SoulBound, Line = "Mm. She doesn't watch you like that.", Next = "skiff" },
            new() { Id = "time", Speaker = SoulBound, Line = "Time. Fine. I'll be very charming at her. She'll hate it.", Next = "skiff" },
            new() { Id = "sees", Speaker = SoulBound, Direction = "a beat", Line = "Like what?" },
            new() { Speaker = You, Line = "I don't know. That's what bothers me." },
            new() { Speaker = SoulBound, Direction = "quietly", Line = "...You'd tell me. Wouldn't you? If you ever saw it too." },

            // The skiff, the winter after the nets.
            new() { Id = "skiff", Speaker = SoulBound, Line = "Remember the skiff? Old Bram's, the winter after they took the nets?" },
            new() { Speaker = You, Line = "I remember the cold." },
            new() { Speaker = SoulBound, Line = "I remember the herring. A whole sack, out of Harrow's water. We knocked on every door in the cove before it got light." },
            new() { Speaker = SoulBound, Line = "And at noon the reeve stood in the square and asked who'd fished the lord's water. And I stood up." },
            new() { Speaker = You, Line = "And they took your mother's ring for the fine." },
            new() { Speaker = SoulBound, Line = "She never said one word to me about it. Not one. That was worse than shouting." },
            new()
            {
                Speaker = SoulBound, Direction = "to the dark",
                Line = "That's what I learned that winter. Doing right when you're small just gets the people you love punished. You have to be big first.",
                Options = new DialogueOption[]
                {
                    new()
                    {
                        Text = "Or the whole cove stands up together, and they can't fine everyone.", Next = "together",
                        Effects = new[] { DialogueEffect.Add(Meters.Bond, 3), DialogueEffect.Set(SbSharedCatch) },
                    },
                    new()
                    {
                        Text = "Then we get big.", Next = "big",
                        Effects = new[] { DialogueEffect.Add(Meters.Ambition, 3), DialogueEffect.Add(Meters.Bond, 2) },
                    },
                    new() { Text = "You stood up so I didn't have to. I never thanked you.", Next = "thanks", Effects = new[] { DialogueEffect.Add(Meters.Bond, 3) } },
                },
            },
            new() { Id = "together", Speaker = SoulBound, Direction = "a long pause", Line = "...That's a nice idea. It isn't what happened." },
            new() { Speaker = SoulBound, Line = "I'll keep it, though. In case.", Next = "home" },
            new() { Id = "big", Speaker = SoulBound, Direction = "grinning", Line = "Then we get big. Big enough that nobody takes anybody's ring ever again.", Next = "home" },
            new() { Id = "thanks", Speaker = SoulBound, Line = "You don't thank people for that. You just do it back for them, one day." },
            new() { Speaker = SoulBound, Line = "Soul-Bound. Never leave the other behind. It goes both ways, or it's just a nice scar." },
            new()
            {
                Id = "home", Speaker = SoulBound, Conditions = new[] { DialogueCondition.Not(FennickPaid) },
                Line = "Come on. Fennick owes us triple, and I want to watch him count it out.",
                Effects = new[] { DialogueEffect.Set(SbTalk1) },
            },
            new()
            {
                Speaker = SoulBound, Conditions = new[] { DialogueCondition.Is(FennickPaid) },
                Line = "Come on. Somebody out there's waiting for someone to turn up. Might as well be us.",
                Effects = new[] { DialogueEffect.Set(SbTalk1) },
            },
        },
    };

    /// <summary>The Soul-Bound after Thornbridge: what they said to Mira, the crowd or your hand on their shoulder, Brisk, and Mira's warning.</summary>
    public static readonly DialogueScript SoulBoundThornbridge = new()
    {
        Id = "sb-talk-2",
        With = SoulBound,
        Needs = new[] { Thornbridge },
        Unless = new[] { SbTalk2 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = SoulBound, Direction = "not quite looking at you", Line = "Can I ask you something? And you answer it straight. No jokes." },
            new() { Speaker = You, Line = "Straight." },
            new() { Speaker = SoulBound, Line = "What I said to Mira. About her teacher, and the council. You heard it." },
            new()
            {
                Speaker = SoulBound,
                Line = "I didn't mean the burning. I'd never. I meant the arguing. Old men arguing in a hall while children starve. You know that's what I meant.",
                Options = new DialogueOption[]
                {
                    new() { Text = "I know what you meant. You're nothing like him.", Next = "nothing", Effects = new[] { DialogueEffect.Add(Meters.Bond, 3) } },
                    new() { Text = "I know what you meant. For a second it still sounded like him.", Next = "sounded", Effects = new[] { DialogueEffect.Add(Meters.Bond, 3) } },
                    new() { Text = "Mira's lost everything. Say sorry to her, not to me.", Next = "sorry", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                },
            },
            new() { Id = "nothing", Speaker = SoulBound, Direction = "breathing out", Line = "Thank you. I needed somebody to say it.", Next = "crowd" },
            new() { Id = "sounded", Speaker = SoulBound, Direction = "a long pause", Line = "...Yeah. It did, didn't it." },
            new()
            {
                Speaker = SoulBound, Line = "Then tell me when it does. Every time I sound like him. Promise?",
                Options = new DialogueOption[]
                {
                    new() { Text = "I promise.", Next = "promised", Effects = new[] { DialogueEffect.Set(SbTellMe), DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "You won't need me to.", Next = "wont" },
                },
            },
            new() { Id = "promised", Speaker = SoulBound, Line = "Good. Then I can stop being scared of it. You can be scared of it for me.", Next = "crowd" },
            new() { Id = "wont", Speaker = SoulBound, Line = "Ha. Let's hope.", Next = "crowd" },
            new() { Id = "sorry", Speaker = SoulBound, Line = "I did. Sort of. Hands up, 'I didn't mean that.' That isn't sorry, is it." },
            new() { Speaker = SoulBound, Line = "I'll do it properly. Before she has to look at me across another fire." },

            // The crowd that cheered, or your hand on their shoulder.
            new() { Id = "crowd", Speaker = SoulBound, Conditions = Loved, Direction = "quieter", Line = "And then the crowd. Shouting my name, {name}. My name. People who'd walked three weeks to get there." },
            new() { Speaker = SoulBound, Conditions = Loved, Line = "I've never felt anything like it. Warm all the way through. Like the first fire after a wet night at sea." },
            new()
            {
                Speaker = SoulBound, Conditions = Loved, Line = "Is that bad? Tell me that's not bad.",
                Options = new DialogueOption[]
                {
                    new() { Text = "It's not bad. You gave them hope.", Next = "hope", Effects = new[] { DialogueEffect.Add(Meters.Ambition, 2) } },
                    new() { Text = "It's not bad. Just don't start needing it.", Next = "needing", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "It's a dangerous kind of warm.", Next = "dangerous", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                },
            },
            new() { Id = "hope", Speaker = SoulBound, Conditions = Loved, Line = "Hope. Yes. That's what it was. That's a good word for it.", Next = "crowd-end" },
            new() { Id = "needing", Speaker = SoulBound, Conditions = Loved, Direction = "laughing, not quite", Line = "Don't start needing it. Right. Easy.", Next = "crowd-end" },
            new() { Id = "dangerous", Speaker = SoulBound, Conditions = Loved, Line = "Says the one who's never been cold. ...Sorry. You have. You were right there with me." },
            new() { Id = "crowd-end", Speaker = SoulBound, Conditions = Loved, Line = "Anyway. They needed somebody, and somebody was us." },
            new() { Speaker = SoulBound, Conditions = NotLoved, Line = "And at the end, you put your hand on my shoulder, and I came back. I didn't know how far gone I was until I felt it." },
            new()
            {
                Speaker = SoulBound, Conditions = NotLoved, Line = "Don't stop doing that. Even when I shout at you. Especially then.",
                Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) },
            },

            // The Watch on the wall.
            new() { Speaker = SoulBound, Line = "Brisk, though. Up on the wall. He isn't even a bad man. That's what gets me. He's just the shape of the job." },
            new() { Speaker = SoulBound, Conditions = new[] { DialogueCondition.Is(BriskShoved) }, Line = "I shoved him, at the gate. A sergeant doing what he was told. I'd do it again, and I hate that I would." },
            new() { Speaker = SoulBound, Line = "Somebody higher up made the job that shape. I'd like to meet them. I'd like to ask them a few things." },

            // Mira's word in your ear.
            new()
            {
                Speaker = SoulBound, Line = "Mira had a word with you after. Privately. I saw her face. She asked you to keep an eye on me, didn't she?",
                Options = new DialogueOption[]
                {
                    new() { Text = "She did. She's worried about you.", Next = "worried", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "She asked about the road east.", Next = "lied", Effects = new[] { DialogueEffect.Add(Meters.Bond, -2) } },
                    new() { Text = "That's between me and Mira.", Next = "between" },
                },
            },
            new() { Id = "worried", Speaker = SoulBound, Line = "She's worried about him. She looks at me and sees him. ...Honestly? I'd keep an eye on me too.", Next = "inn" },
            new() { Id = "lied", Speaker = SoulBound, Direction = "a small smile", Line = "Course she did. You're a terrible liar, you know. Always were. It's one of your best things.", Next = "inn" },
            new() { Id = "between", Speaker = SoulBound, Line = "Fair. People are allowed secrets. Even from me." },
            new()
            {
                Id = "inn", Speaker = SoulBound, Conditions = new[] { DialogueCondition.Not(Summoned) },
                Line = "Right. Enough feelings. Let's go and sit in Mira's inn, the Gilded Whatsit, and be found.",
                Effects = new[] { DialogueEffect.Set(SbTalk2) },
            },
            new()
            {
                Speaker = SoulBound, Conditions = new[] { DialogueCondition.Is(Summoned) },
                Line = "Right. Enough feelings. A princess needs us. A princess, {name}. Two kids from Kettle Cove.",
                Effects = new[] { DialogueEffect.Set(SbTalk2) },
            },
        },
    };

    /// <summary>The Soul-Bound after the court and Crane: knighthood, the king's caution, Crane's "not yet", and Lord Harrow clapping in the gallery.</summary>
    public static readonly DialogueScript SoulBoundCourt = new()
    {
        Id = "sb-talk-3",
        With = SoulBound,
        Needs = new[] { CraneDone },
        Unless = new[] { SbTalk3 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = SoulBound, Direction = "still glowing",
                Line = "Knights. He said knights, {name}. Kneel and rise. Two kids who used to gut fish for a copper a bucket.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You've earned it.", Next = "earned", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "When we get back from Greywatch. If we get back.", Next = "details" },
                    new() { Text = "Sir {sb}. It suits you.", Next = "suits", Effects = new[] { DialogueEffect.Add(Meters.Ambition, 2) } },
                },
            },
            new() { Id = "earned", Speaker = SoulBound, Line = "We've earned it. Don't you dare make it just me.", Next = "court" },
            new() { Id = "details", Speaker = SoulBound, Line = "Details. Lovely, important, miserable details. Let me have one evening.", Next = "court" },
            new() { Id = "suits", Speaker = SoulBound, Direction = "laughing", Line = "Say it again. No, don't. I'll get used to it." },

            // How you stood in the throne room.
            new() { Id = "court", Speaker = SoulBound, Conditions = Backed, Line = "And you stood up with me. In front of the whole court. 'Waiting costs lives.' I won't forget that. Not ever." },
            new()
            {
                Speaker = SoulBound, Conditions = NotBacked, Direction = "a little stiff", Line = "In there, with the king. For a second I was out on my own.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You spoke out of turn to a king. I didn't know what to do.", Next = "froze", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "Somebody had to keep us out of a cell.", Next = "cell" },
                },
            },
            new() { Id = "froze", Speaker = SoulBound, Conditions = NotBacked, Line = "Neither did I. My mouth just went. Next time, kick me.", Next = "court-end" },
            new() { Id = "cell", Speaker = SoulBound, Conditions = NotBacked, Line = "Fair. I'd be terrible in a cell. All that waiting." },
            new() { Id = "court-end", Speaker = SoulBound, Conditions = NotBacked, Line = "It's fine. We're fine. We're knights. Nearly." },

            // The king who thanks, and weighs.
            new() { Speaker = SoulBound, Line = "He's a good man. I mean it. He thanked us. He bowed his head to us, in front of everyone." },
            new()
            {
                Speaker = SoulBound, Line = "And then, same breath, he won't empty his villages on one scholar's word. Both things. Same man. How does that work?",
                Options = new DialogueOption[]
                {
                    new() { Text = "He fought the last war. He's careful because he remembers it.", Next = "careful" },
                    new() { Text = "Good men can be slow.", Next = "slow" },
                    new() { Text = "Kings answer to everyone. That's why they're slow.", Next = "answer", Effects = new[] { DialogueEffect.Add(Meters.Ambition, 2) } },
                },
            },
            new() { Id = "careful", Speaker = SoulBound, Line = "I know. I know. I just want careful to be faster.", Next = "crane" },
            new() { Id = "slow", Speaker = SoulBound, Line = "Funny. That's nearly what Crane said.", Next = "crane" },
            new() { Id = "answer", Speaker = SoulBound, Line = "Then maybe they shouldn't have to answer to quite so many." },
            new() { Speaker = SoulBound, Direction = "catching it", Line = "That came out wrong. You know what I mean." },

            // Crane in the hall.
            new() { Id = "crane", Speaker = SoulBound, Line = "Crane caught me in the hall after. Very charming. Rings on every finger. Smells like a flower stall." },
            new()
            {
                Speaker = SoulBound, Direction = "a small smile", Line = "He said wars need different men. And he called me Sir {sb}. 'Not yet,' he said. 'Not yet.'",
                Options = new DialogueOption[]
                {
                    new() { Text = "He called Mira's warning 'dramatic'. Don't trust him.", Next = "snake", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "What did he want from you?", Next = "want" },
                    new() { Text = "He's right. Wars need different people.", Next = "right", Effects = new[] { DialogueEffect.Add(Meters.Ambition, 3) } },
                },
            },
            new() { Id = "snake", Speaker = SoulBound, Line = "Oh, I don't. He's a snake in silk. A snake who thinks I'm going places, though. That's a new kind of snake for me.", Next = "harrow" },
            new() { Id = "want", Speaker = SoulBound, Line = "To be liked. Everybody at court wants to be liked by somebody who's about to matter." },
            new() { Speaker = SoulBound, Direction = "hearing it", Line = "Listen to me. 'About to matter.' One speech and a promise, and I'm talking like them.", Next = "harrow" },
            new() { Id = "right", Speaker = SoulBound, Direction = "after a moment", Line = "...Yes. Yes, they do." },

            // The heron in the gallery.
            new() { Id = "harrow", Speaker = SoulBound, Direction = "the smile going", Line = "Something else. In the gallery, when the king said thank you. A man clapping, in a green coat. A heron on the breast." },
            new() { Speaker = You, Line = "...Harrow's heron." },
            new() { Speaker = SoulBound, Line = "Lord Harrow. In the king's own hall, clapping for us. He looked straight at me and didn't know me." },
            new() { Speaker = SoulBound, Line = "Why would he? I was one of the kids on the landing. One of the ones he didn't look at." },
            new()
            {
                Speaker = SoulBound, Line = "I want to know what he's doing in Questeria, {name}. And I want to be there when he finds out who I am.",
                Options = new DialogueOption[]
                {
                    new()
                    {
                        Text = "We'll find out. Together, and carefully.", Next = "carefully",
                        Effects = new[] { DialogueEffect.Add(Meters.Bond, 3), DialogueEffect.Set(SbQuestHook) },
                    },
                    new()
                    {
                        Text = "Let it go. He isn't worth it.", Next = "let-go",
                        Effects = new[] { DialogueEffect.Add(Meters.Bond, -2), DialogueEffect.Set(SbQuestHook) },
                    },
                    new()
                    {
                        Text = "Then let's make him remember.", Next = "remember",
                        Effects = new[] { DialogueEffect.Add(Meters.Ambition, 3), DialogueEffect.Add(Meters.Bond, 2), DialogueEffect.Set(SbQuestHook) },
                    },
                },
            },
            new() { Id = "carefully", Speaker = SoulBound, Line = "Together. Carefully. I can do carefully. Watch me be so careful.", Next = "go" },
            new() { Id = "let-go", Speaker = SoulBound, Line = "He took the nets. He let my mother kneel in the mud and talked about the weather. He's worth exactly one look in the face.", Next = "go" },
            new() { Id = "remember", Speaker = SoulBound, Direction = "quietly", Line = "Oh, he'll remember. The landing, the nets, and every name in the cove." },
            new()
            {
                Id = "go", Speaker = SoulBound, Conditions = new[] { DialogueCondition.Not(Chapter4Done) },
                Line = "Come on. A tavern on the docks, a drunk called Brannock, and a pass full of monsters. Knighthood waits for nobody.",
                Effects = new[] { DialogueEffect.Set(SbTalk3) },
            },
            new()
            {
                Speaker = SoulBound, Conditions = new[] { DialogueCondition.Is(Chapter4Done) },
                Line = "Come on. Dawn, the east gate, a caravan, and a pass full of monsters. Knighthood waits for nobody.",
                Effects = new[] { DialogueEffect.Set(SbTalk3) },
            },
        },
    };

    /// <summary>Sera once she's joined: her reach, the count going wrong, and what she thinks of people.</summary>
    public static readonly DialogueScript SeraJoined = new()
    {
        Id = "sera-talk-1",
        With = Sera,
        Needs = new[] { SeraRecruited },
        Unless = new[] { SeraTalk1 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Sera, Direction = "not turning round", Line = "You want something. People who want something stand like that.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Just to talk. We're travelling together now.", Next = "talk", Effects = new[] { DialogueEffect.Add(SeraApproval, 2) } },
                    new() { Text = "That shot, on the last wolf. I've never seen anything like it.", Next = "shot" },
                    new()
                    {
                        Text = "How much does a warden get paid?", Next = "paid",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Pragmatic) }, Locked = true,
                    },
                },
            },
            new() { Id = "talk", Speaker = Sera, Line = "Talk, then. Quietly. The wood listens.", Next = "reach" },
            new() { Id = "shot", Speaker = Sera, Line = "Then you haven't watched enough people who need to eat.", Next = "reach" },
            new() { Id = "paid", Speaker = Sera, Line = "Arrows and apologies. The Lodge is generous with both." },
            new() { Id = "reach", Speaker = Sera, Line = "Sera Vantis. Warden of the Mirewood's western reach. Four leagues of trees, two streams, and one Lodge that forgets I exist." },
            new()
            {
                Speaker = Sera, Line = "Ask, if you're going to. I'll answer one.",
                Options = new DialogueOption[]
                {
                    new() { Text = "How long have you been warden?", Next = "how-long" },
                    new() { Text = "Do you work alone?", Next = "alone" },
                    new() { Text = "What's wrong with the wolves?", Next = "wolves" },
                },
            },
            new() { Id = "how-long", Speaker = Sera, Line = "Since the winter my mother gave it up. Long enough to know every badger by name. They don't know mine. That's the arrangement.", Next = "count" },
            new() { Id = "alone", Speaker = Sera, Line = "There's Burr. A kestrel. He's off somewhere being better company than people.", Next = "count" },
            new() { Id = "wolves", Speaker = Sera, Line = "Nothing's wrong with the wolves. Something's wrong with their water. Wolves are just honest about it." },
            new() { Id = "count", Speaker = Sera, Line = "Every warden keeps a count. Old trees, dens, springs, setts. I keep mine in my head. I could walk it blind." },
            new() { Speaker = Sera, Line = "Three weeks ago the count started going wrong. Badgers dead in their setts. A stag standing in the stream, just standing, with black running out of its eyes." },
            new() { Speaker = Sera, Conditions = new[] { DialogueCondition.Not(CrystalDealtWith) }, Line = "Whatever's down in that quarry, I want to see it before anyone swings anything at it." },
            new() { Speaker = Sera, Conditions = new[] { DialogueCondition.Is(CrystalDealtWith) }, Line = "The quarry's quiet now. The count will take years to come right. I've got years." },
            new()
            {
                Speaker = Sera, Line = "Anything else? You've the look of someone with one more.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You don't like people much, do you?", Next = "people" },
                    new()
                    {
                        Text = "Somebody was kind to you once, and it went wrong.",
                        Check = new DialogueCheck { Label = "INSIGHT", Ability = Ability.Wisdom, Advantage = new[] { DialogueCondition.IsClass(ClassId.Druid) } },
                        OnSuccess = "guessed", OnFailure = "not-guessed",
                    },
                    new() { Text = "No. That's all.", Next = "enough" },
                },
            },
            new() { Id = "people", Speaker = Sera, Line = "I like people fine. At a distance. With the wind blowing towards me." },
            new()
            {
                Speaker = Sera, Line = "Animals want what they want, and they show you. A wolf doesn't smile at you first.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Not everyone who smiles wants something.", Next = "smiles" },
                    new()
                    {
                        Text = "Then I'll try smiling second.", Next = "second",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                        Effects = new[] { DialogueEffect.Add(SeraApproval, 2) },
                    },
                    new() { Text = "That's a lonely way to live.", Next = "lonely" },
                },
            },
            new() { Id = "smiles", Speaker = Sera, Line = "No. But you don't find out which until after.", Next = "close" },
            new() { Id = "second", Speaker = Sera, Direction = "a beat late", Line = "...That was nearly funny. Don't let it go to your head.", Next = "close" },
            new() { Id = "lonely", Speaker = Sera, Line = "It's a quiet way to live. People always mix those two up.", Next = "close" },
            new()
            {
                Id = "guessed", Speaker = Sera, Direction = "a long silence", Line = "...Some surveyors. A long time ago. Honey cakes and very good manners.",
                Effects = new[] { DialogueEffect.Set(SeraGuessed) },
            },
            new() { Speaker = Sera, Line = "I'll tell you the rest when I know you better. Maybe.", Effects = new[] { DialogueEffect.Add(SeraApproval, 3) }, Next = "close" },
            new() { Id = "not-guessed", Speaker = Sera, Line = "Lots of people are kind to me. I shoot some of them.", Next = "close" },
            new() { Id = "enough", Speaker = Sera, Line = "Good. I'd run out of answers." },
            new()
            {
                Id = "close", Speaker = Sera, Conditions = new[] { DialogueCondition.Is(SeraFlirt) },
                Line = "And you. 'Do you do this often.' I've thought of some better answers since. Don't ask me for them.",
            },
            new() { Speaker = Sera, Line = "Stay where I can see you. Both of you. Loud people walk into things.", Effects = new[] { DialogueEffect.Set(SeraTalk1) } },
        },
    };

    /// <summary>Sera after the crystal and the night camp: what you chose, the Hollins, and the last silverbark. The hook for her personal quest.</summary>
    public static readonly DialogueScript SeraCrystal = new()
    {
        Id = "sera-talk-2",
        With = Sera,
        Needs = new[] { MirewoodCamp },
        Unless = new[] { SeraTalk2 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            // The crystal, however it went.
            new() { Speaker = Sera, Conditions = Guarded, Line = "You kept them off me at the quarry. Every rotten thing in the wood, and not one got past you to the crystal." },
            new() { Speaker = Sera, Conditions = Guarded, Line = "I've stood alone at a lot of things. That wasn't alone. I noticed.", Effects = new[] { DialogueEffect.Add(SeraApproval, 2) } },
            new() { Speaker = Sera, Conditions = PurifiedAlone, Line = "You let me do it slowly. Two days, and nobody came down the steps to hurry me." },
            new() { Speaker = Sera, Conditions = PurifiedAlone, Line = "{sb} walked off. You stayed. That cost you something with {sb_them}. I saw it cost you.", Effects = new[] { DialogueEffect.Add(SeraApproval, 2) } },
            new() { Speaker = Sera, Conditions = Shattered, Direction = "flatly", Line = "You broke it. The black ran into the stream, the way I said it would." },
            new() { Speaker = Sera, Conditions = Shattered, Line = "Three otters dead below the falls. A heron. The woodcutters are boiling their water twice." },
            new()
            {
                Speaker = Sera, Conditions = Shattered, Line = "You chose fast. I understand fast. I don't have to like it.",
                Options = new DialogueOption[]
                {
                    new() { Text = "People were dying while we waited. I'd choose it again.", Next = "again" },
                    new() { Text = "I was wrong. How do we fix the stream?", Next = "fix", Effects = new[] { DialogueEffect.Add(SeraApproval, 3) } },
                    new() { Text = "{sb} was right. You'd have let it fester.", Next = "fester", Effects = new[] { DialogueEffect.Add(SeraApproval, -3) } },
                },
            },
            new() { Id = "again", Speaker = Sera, Conditions = Shattered, Line = "Then we disagree, and you said so to my face. I can work with that.", Next = "stream-end" },
            new() { Id = "fix", Speaker = Sera, Conditions = Shattered, Line = "...That's a better question than most people ever ask. Slowly. Upstream first. I'll show you.", Next = "stream-end" },
            new() { Id = "fester", Speaker = Sera, Conditions = Shattered, Line = "{sb} is always very sure. I notice that about {sb_them}." },
            new() { Id = "stream-end", Speaker = Sera, Conditions = Shattered, Line = "Never mind. It's done. Done things are only good for learning from." },

            // The Hollins.
            new() { Speaker = Sera, Line = "There's something I haven't told the Lodge. Or anyone." },
            new() { Speaker = Sera, Conditions = new[] { DialogueCondition.Is(SeraGuessed) }, Line = "You guessed something, back in the wood. Surveyors. Here's the rest of it." },
            new() { Speaker = Sera, Line = "East of the quarry, past the black, there's a tree. A silverbark. The last one in the Mirewood." },
            new() { Speaker = Sera, Line = "There used to be a whole stand of them. The Hollins. Four hundred trees, older than your city." },
            new() { Speaker = You, Line = "Used to be?" },
            new() { Speaker = Sera, Line = "In the last war the crown needed warships. The king's surveyors came. Polite men. They brought my mother honey cakes." },
            new() { Speaker = Sera, Line = "I showed them the way in. I was young. They were kind to me, and I was proud to be asked." },
            new() { Speaker = Sera, Line = "A month later the Hollins was stumps. One tree left, because it was too young to be worth the axe." },
            new()
            {
                Speaker = Sera, Direction = "very evenly", Line = "So now you know why I don't like kind strangers.",
                Options = new DialogueOption[]
                {
                    new() { Text = "It wasn't your fault. You were young.", Next = "young", Effects = new[] { DialogueEffect.Add(SeraApproval, 2) } },
                    new() { Text = "So you count people the way you count trees.", Next = "counted", Effects = new[] { DialogueEffect.Add(SeraApproval, 3) } },
                    new() { Text = "Will you show me the tree?", Next = "show", Effects = new[] { DialogueEffect.Add(SeraApproval, 3) } },
                },
            },
            new() { Id = "young", Speaker = Sera, Line = "Young is when you learn what people are. I learned.", Next = "tree" },
            new() { Id = "counted", Speaker = Sera, Direction = "a long look", Line = "...Yes. That's it exactly. Nobody's ever said it back to me.", Next = "tree" },
            new() { Id = "show", Speaker = Sera, Line = "Not yet. But I'll remember you asked." },
            new()
            {
                Id = "tree", Speaker = Sera, Conditions = Shattered,
                Line = "The stream you broke the crystal into runs right past its roots. I need to go and look. Soon.",
                Effects = new[] { DialogueEffect.Set(SeraQuestHook) },
            },
            new()
            {
                Speaker = Sera, Conditions = NotShattered,
                Line = "The black never reached it. It's safe, for now. For now is all a tree that old ever gets.",
                Effects = new[] { DialogueEffect.Set(SeraQuestHook) },
            },
            new()
            {
                Speaker = Sera, Line = "Don't tell {sb}.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Why not?", Next = "why-not" },
                    new() { Text = "I'll keep it. Your tree, your secret.", Next = "keep", Effects = new[] { DialogueEffect.Add(SeraApproval, 2) } },
                    new()
                    {
                        Text = "When this is over, I'd like to see it. With you.", Next = "flirt", Flirt = true,
                        Conditions = new[] { DialogueCondition.Is(SeraFlirt), DialogueCondition.AtLeast(SeraApproval, RomanceSeed) },
                        Effects = new[] { DialogueEffect.Set(SeraFlirt2), DialogueEffect.Add(SeraApproval, 3) },
                    },
                },
            },
            new() { Id = "why-not", Speaker = Sera, Line = "Not because I think {sb} would hurt it. Because {sb} would want to help. Fast.", Next = "for-a-week" },
            new() { Id = "keep", Speaker = Sera, Line = "Good. Not because I think {sb} would hurt it. Because {sb} would want to help. Fast.", Next = "for-a-week" },
            new() { Id = "flirt", Speaker = Sera, Direction = "not quite looking at you", Line = "...Ask me again when it's over. I might say yes." },
            new() { Speaker = Sera, Line = "And not a word to {sb}. {sb} would want to help. Fast." },
            new() { Id = "for-a-week", Speaker = Sera, Line = "That's enough talking for a week.", Effects = new[] { DialogueEffect.Set(SeraTalk2) } },
        },
    };

    /// <summary>Sera after Thornbridge: what she saw in the Soul-Bound there, and that she doesn't want to be right.</summary>
    public static readonly DialogueScript SeraThornbridge = new()
    {
        Id = "sera-talk-3",
        With = Sera,
        Needs = new[] { Thornbridge },
        Unless = new[] { SeraTalk3 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Sera, Direction = "low", Line = "Thornbridge. I want to tell you what I saw there. You won't like it.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Tell me.", Next = "saw" },
                    new() { Text = "Is this about {sb}?", Next = "about" },
                    new() { Text = "Then don't.", Next = "dont" },
                },
            },
            new() { Id = "about", Speaker = Sera, Line = "Yes.", Next = "saw" },
            new() { Id = "dont", Speaker = Sera, Line = "I will anyway. That's what you get for keeping me around." },
            new() { Id = "saw", Speaker = Sera, Line = "I watched the people more than the fighting. The Hollowed are easy. They don't hide what they want." },
            new() { Speaker = Sera, Line = "When {sb} said that to Mira, about the council, everyone flinched. {sb} flinched last." },
            new() { Speaker = Sera, Line = "Like a wolf that's bitten something and only then tastes it." },
            new() { Speaker = Sera, Conditions = Loved, Line = "And when they shouted {sb_their} name, {sb} closed {sb_their} eyes. Like stepping into a warm stream." },
            new() { Speaker = Sera, Conditions = Loved, Line = "I've watched hungry things eat. That's what it looked like." },
            new() { Speaker = Sera, Conditions = NotLoved, Line = "And your hand on {sb_their} shoulder was the only thing in that whole camp {sb} listened to." },
            new() { Speaker = Sera, Conditions = NotLoved, Line = "I don't think you know how much weight that one hand is carrying." },
            new()
            {
                Speaker = Sera, Line = "I'm not saying {sb} is bad. I'm saying {sb} is hungry. Those aren't the same thing. Yet.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You don't know {sb} like I do.", Next = "know", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "I've seen it too.", Next = "seen", Effects = new[] { DialogueEffect.Add(SeraApproval, 3) } },
                    new()
                    {
                        Text = "You're watching my friend like prey. Stop it.", Next = "prey",
                        Effects = new[] { DialogueEffect.Add(SeraApproval, -3), DialogueEffect.Add(Meters.Bond, 2) },
                    },
                },
            },
            new() { Id = "know", Speaker = Sera, Line = "No. That's why I can see {sb_them}. You're standing too close to see anything but the face.", Next = "right" },
            new() { Id = "seen", Speaker = Sera, Line = "Then we've both seen it. That's a start. That's more than most people ever get.", Next = "right" },
            new() { Id = "prey", Speaker = Sera, Line = "I watch everything that might bite. I watch you, too. I'd stop if I knew how." },
            new() { Id = "right", Speaker = Sera, Line = "I don't want to be right about this. I've been right about people before. It never once made me happy." },
            new()
            {
                Speaker = Sera, Conditions = new[] { DialogueCondition.Is(SeraFlirt2) }, Direction = "not looking at you",
                Line = "And I do watch you. It's a different kind of watching. Don't let it go to your head.",
            },
            new() { Speaker = Sera, Line = "That's all. I've said it. Now it's yours to carry too.", Effects = new[] { DialogueEffect.Set(SeraTalk3) } },
        },
    };

    /// <summary>Mira once she's joined: the healing light, her father among the Hollowed, and the half of the Ember Index she left behind. The hook for her personal quest.</summary>
    public static readonly DialogueScript MiraJoined = new()
    {
        Id = "mira-talk-1",
        With = Mira,
        Needs = new[] { MiraRecruited },
        Unless = new[] { MiraTalk1 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Mira, Direction = "flexing her hands", Line = "Sorry. Give me a moment. The light leaves your hands shaking, after. Like holding a lamp up too long.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Take all the time you need.", Next = "time", Effects = new[] { DialogueEffect.Add(MiraApproval, 2) } },
                    new() { Text = "Where did you learn to heal like that?", Next = "learn" },
                    new()
                    {
                        Text = "Do you charge for that, or is it just for us?", Next = "charge",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "time", Speaker = Mira, Line = "Thank you. Nobody's said that to me in a month. Everybody needs it now.", Next = "faces" },
            new() { Id = "learn", Speaker = Mira, Line = "The Archive taught me to read. The road taught me the rest. The light was in a book I wasn't supposed to open.", Next = "faces" },
            new() { Id = "charge", Speaker = Mira, Direction = "a tired laugh", Line = "For you, a song. A bad one. I only know the hymns from home." },
            new() { Id = "faces", Speaker = Mira, Line = "Can I tell you something strange? During the raid, I kept looking at their faces. The Hollowed. Every one of them." },
            new() { Speaker = Mira, Line = "I was looking for my father." },
            new() { Speaker = You, Line = "Your father?" },
            new() { Speaker = Mira, Line = "Tobin Ashgrove. He pressed cider in Orrin's Reach, under the Archive hill. A burn across his left hand, from a still that went up when I was six." },
            new() { Speaker = Mira, Line = "When the rot reached the Reach, I was up at the Archive. By the time I got home the village was empty. No bodies. Every door open, and supper on the tables." },
            new()
            {
                Speaker = Mira, Line = "So he's dead, or he's one of them. And I can't grieve him, because I don't know which.",
                Options = new DialogueOption[]
                {
                    new() { Text = "If we ever see him, I'll know the hand. I promise.", Next = "promise", Effects = new[] { DialogueEffect.Add(MiraApproval, 5) } },
                    new() { Text = "Maybe he got out. Some must have.", Next = "got-out", Effects = new[] { DialogueEffect.Add(MiraApproval, 2) } },
                    new() { Text = "The Hollowed have no memory. If he's one of them, he's already gone.", Next = "gone", Effects = new[] { DialogueEffect.Add(MiraApproval, -2) } },
                },
            },
            new() { Id = "promise", Speaker = Mira, Direction = "quietly", Line = "Thank you. Even knowing would be a mercy. Either way.", Next = "index" },
            new() { Id = "got-out", Speaker = Mira, Line = "Some did. I tell myself that every night. It's a very good story, and I'm an archivist. I know a good story.", Next = "index" },
            new() { Id = "gone", Speaker = Mira, Direction = "very evenly", Line = "I know what they are. I helped write down what they are." },
            new() { Id = "index", Speaker = Mira, Line = "There's something else. When I ran, I took the Archive's index of the Box. Every note my teacher and I ever made on it." },
            new() { Speaker = Mira, Line = "Half of it. The other half is still up there, in a burned library, in the dark." },
            new()
            {
                Speaker = Mira, Line = "If there's a way to bring the Hollowed back, it's in the half I left. One day I'm going back for it.",
                Effects = new[] { DialogueEffect.Set(MiraQuestHook) },
            },
            new()
            {
                Speaker = Mira, Line = "And {sb}. I asked you to watch {sb_them}. That wasn't fair of me. I'd known {sb_them} a day.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You saw something. Tell me what.", Next = "fire", Effects = new[] { DialogueEffect.Add(MiraApproval, 3) } },
                    new() { Text = "{sb} isn't your teacher.", Next = "teacher" },
                    new() { Text = "I'll watch. I always have.", Next = "always", Effects = new[] { DialogueEffect.Add(MiraApproval, 2) } },
                },
            },
            new() { Id = "fire", Speaker = Mira, Line = "A fire. Not a bad fire. The kind that keeps a whole room warm, until there's no more wood, and it starts on the furniture.", Next = "king" },
            new() { Id = "teacher", Speaker = Mira, Line = "No. I hope not. But my teacher was the best man I knew, too. That's rather the point.", Next = "king" },
            new() { Id = "always", Speaker = Mira, Line = "Then I'll stop worrying. A little. I'm out of practice at not worrying." },
            new()
            {
                Id = "king", Speaker = Mira, Conditions = BeforeCourt,
                Line = "Now. The king. Someone in that palace has to hear what I know, and I'd rather not walk in alone.",
                Effects = new[] { DialogueEffect.Set(MiraTalk1) },
            },
            new()
            {
                Speaker = Mira, Conditions = AfterCourt,
                Line = "Thank you for listening. I'm better at all this on paper.",
                Effects = new[] { DialogueEffect.Set(MiraTalk1) },
            },
        },
    };

    /// <summary>Mira after the vault: the cube on Kael's black-iron coin is her drawing, and she read the Box's lid aloud.</summary>
    public static readonly DialogueScript MiraCoin = new()
    {
        Id = "mira-talk-2",
        With = Mira,
        Needs = new[] { VaultDone },
        Unless = new[] { MiraTalk2 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Mira, Line = "You've still got Kael's coin. May I see it again?" },
            new() { Speaker = Speakers.Narrator, Line = "She turns the black coin over and over. On one face, a cube, drawn in a few clean lines." },
            new()
            {
                Speaker = Mira, Line = "I know this cube. I drew it.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You drew it?", Next = "drew" },
                    new() { Text = "Mira. Sit down first.", Next = "sit", Effects = new[] { DialogueEffect.Add(MiraApproval, 2) } },
                },
            },
            new() { Id = "sit", Speaker = Mira, Direction = "sitting", Line = "Yes. All right. Thank you." },
            new() { Id = "drew", Speaker = Mira, Line = "My first week at the Archive, I catalogued what came up from the lowest stacks. My teacher asked me to sketch it for the index. A box. Six faces, no seams." },
            new() { Speaker = Mira, Line = "He liked my drawing. He said it was honest." },
            new() { Speaker = Mira, Line = "Now it's on his money. My little drawing, paying for murders inside your king's walls." },
            new() { Speaker = Mira, Line = "I read him the lid, too. Old Emberic. Nobody up there could read it but him and me, and his eyes were going." },
            new()
            {
                Speaker = Mira, Line = "'Want, and be answered.' That's what it says. I read it out loud, by his lamp, and he smiled.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You couldn't have known what it would do.", Next = "known", Effects = new[] { DialogueEffect.Add(MiraApproval, 2) } },
                    new() { Text = "He'd have read it himself, sooner or later.", Next = "later" },
                    new() { Text = "You've carried this on your own for a long time. You don't have to.", Next = "carry", Effects = new[] { DialogueEffect.Add(MiraApproval, 5) } },
                },
            },
            new() { Id = "known", Speaker = Mira, Line = "Everyone says that. Archivists are supposed to know. It's the whole job.", Next = "buyer" },
            new() { Id = "later", Speaker = Mira, Line = "Later, yes. A season later. A season is a great many families.", Next = "buyer" },
            new() { Id = "carry", Speaker = Mira, Direction = "after a long while", Line = "...I don't know how to put it down. Nobody ever taught me that." },
            new() { Speaker = Mira, Line = "But thank you for offering to hold the other end." },
            new() { Id = "buyer", Speaker = Mira, Line = "Whoever paid Kael is close to the crown. Perfumed, he said. Finishing school." },
            new() { Speaker = Mira, Conditions = AfterCourt, Line = "I told the king all of it. Names, dates, the coin. And I watched him weigh me like a sack of grain." },
            new()
            {
                Speaker = Mira, Conditions = BeforeCourt,
                Line = "I want to tell the king all of it. And I'm frightened he'll look at me and see a refugee with a story.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Then we'll stand right behind you.", Next = "behind", Effects = new[] { DialogueEffect.Add(MiraApproval, 3) } },
                    new() { Text = "Let {sb} do the talking. {sb} is good at it.", Next = "sb-talks", Effects = new[] { DialogueEffect.Add(MiraApproval, -2) } },
                    new() { Text = "Say it plainly. The truth carries.", Next = "plainly", Effects = new[] { DialogueEffect.Add(MiraApproval, 2) } },
                },
            },
            new() { Id = "behind", Speaker = Mira, Conditions = BeforeCourt, Line = "Behind me. Not in front. I have to be the one who says it.", Next = "coin-end" },
            new() { Id = "sb-talks", Speaker = Mira, Conditions = BeforeCourt, Line = "{sb} is. That's rather my worry.", Next = "coin-end" },
            new() { Id = "plainly", Speaker = Mira, Conditions = BeforeCourt, Line = "The truth carries. I'll write it on the back of my hand." },
            new() { Id = "coin-end", Speaker = Mira, Line = "Keep the coin safe. It's evidence. And it's the ugliest thing I ever made.", Effects = new[] { DialogueEffect.Set(MiraTalk2) } },
        },
    };

    /// <summary>Mira after the court: Crane calling her country dramatic, the Soul-Bound saying the brave thing for her, and why that frightens her.</summary>
    public static readonly DialogueScript MiraCourt = new()
    {
        Id = "mira-talk-3",
        With = Mira,
        Needs = new[] { CourtDone },
        Unless = new[] { MiraTalk3 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Mira, Direction = "far too composed", Line = "'Dramatic.' He called my whole country dramatic. In front of the king." },
            new()
            {
                Speaker = Mira, Line = "I was so careful. I practised it on the stairs. Names, dates, the coin. And it took him one word.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Crane's a coward in silk. The king listened to you.", Next = "listened", Effects = new[] { DialogueEffect.Add(MiraApproval, 3) } },
                    new()
                    {
                        Text = "You were brilliant in there. I'd follow that speech anywhere.", Next = "anywhere", Flirt = true,
                        Conditions = new[] { DialogueCondition.Is(MiraFlirt), DialogueCondition.AtLeast(MiraApproval, RomanceSeed) },
                        Effects = new[] { DialogueEffect.Set(MiraFlirt2), DialogueEffect.Add(MiraApproval, 3) },
                    },
                    new() { Text = "The king's sending us to Greywatch. That's something.", Next = "something", Effects = new[] { DialogueEffect.Add(MiraApproval, 2) } },
                },
            },
            new() { Id = "listened", Speaker = Mira, Line = "He did. 'One frightened scholar.' He listened, and then he weighed me.", Next = "brave" },
            new() { Id = "anywhere", Speaker = Mira, Direction = "a surprised laugh", Line = "Anywhere is a long way. Greywatch will do, to start.", Next = "brave" },
            new() { Id = "something", Speaker = Mira, Line = "It is. It's a start. Starts are what I have." },
            new() { Id = "brave", Speaker = Mira, Line = "And {sb}. 'They're already dying. Just not your villages yet.'" },
            new() { Speaker = Mira, Line = "{sb} said it better than I did. Louder. The whole court went still." },
            new()
            {
                Speaker = Mira, Line = "I should be grateful. I am grateful. That's what frightens me.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Why does it frighten you?", Next = "why", Effects = new[] { DialogueEffect.Add(MiraApproval, 2) } },
                    new() { Text = "{sb} was right.", Next = "was-right" },
                    new()
                    {
                        Text = "Crane caught {sb} in the hall afterwards.", Next = "crane",
                        Conditions = new[] { DialogueCondition.Is(CraneContact) },
                        Effects = new[] { DialogueEffect.Set(MiraHeardCrane), DialogueEffect.Add(MiraApproval, 3) },
                    },
                },
            },
            new() { Id = "why", Speaker = Mira, Line = "Because the last time I was grateful to someone for saying the brave thing, he said a braver thing every year. And I clapped. Every time.", Next = "pass" },
            new() { Id = "was-right", Speaker = Mira, Line = "{sb} was. Being right is the most dangerous thing I know.", Next = "pass" },
            new() { Id = "crane", Speaker = Mira, Direction = "very still", Line = "...Did he. What did he say?" },
            new() { Speaker = You, Line = "That wars need different men. And 'not yet'." },
            new() { Speaker = Mira, Line = "That's the voice. That is exactly the voice that used to talk to my teacher. Not the Box. People. Nice people, at nice dinners." },
            new() { Id = "pass", Speaker = Mira, Line = "Greywatch, then. My teacher's army will come through that pass. I've read the maps he drew of it." },
            new() { Speaker = Mira, Conditions = new[] { DialogueCondition.Is(MiraQuestHook) }, Line = "If there are Hollowed there... a burn across the left hand. You'll remember?" },
            new() { Speaker = You, Conditions = new[] { DialogueCondition.Is(MiraQuestHook) }, Line = "I'll remember." },
            new() { Speaker = Mira, Line = "Thank you. I'll be the one with too many books, at the back of the caravan.", Effects = new[] { DialogueEffect.Set(MiraTalk3) } },
        },
    };

    /// <summary>Kael once he's joined: why he took the Sunheart job (Mother Gannet, Tallow Lane, Wren), and what the Soul-Bound's offer meant. The hook for his personal quest.</summary>
    public static readonly DialogueScript KaelJoined = new()
    {
        Id = "kael-talk-1",
        With = Kael,
        Needs = new[] { KaelRecruited },
        Unless = new[] { KaelTalk1 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Kael, Direction = "walking a copper over his knuckles", Line = "Ah. The one who didn't recruit me. Come to inspect the merchandise?",
                Options = new DialogueOption[]
                {
                    new() { Text = "Came to say welcome. Properly.", Next = "welcome", Effects = new[] { DialogueEffect.Add(KaelApproval, 3) } },
                    new() { Text = "Came to check you haven't robbed the inn.", Next = "robbed" },
                    new() { Text = "You robbed the king's vault. Why should I trust you?", Next = "trust", Effects = new[] { DialogueEffect.Add(KaelApproval, -2) } },
                },
            },
            new() { Id = "welcome", Speaker = Kael, Line = "Properly. Gosh. Nobody's welcomed me properly since my mother, and she had a wooden spoon.", Next = "gorgeous" },
            new() { Id = "robbed", Speaker = Kael, Line = "Only a little. A spoon. It was asking for it.", Next = "gorgeous" },
            new() { Id = "trust", Speaker = Kael, Line = "You shouldn't. Trust is for people with savings." },
            new()
            {
                Id = "gorgeous", Speaker = Kael, Conditions = new[] { DialogueCondition.Is(KaelFlirt) },
                Line = "Also, I called you gorgeous in a vault full of corpses. I'd like that struck from the record. Unless it worked.",
            },
            new() { Speaker = Kael, Line = "You want to know why I took the job. Everybody does. 'Kael, why would a nice boy rob a king?'" },
            new() { Speaker = Kael, Line = "The honest answer is money. The honester answer is whose." },
            new() { Speaker = Kael, Line = "There's a woman in the Low Ring called Mother Gannet. She owns Tallow Lane. Every door, every roof, every rat." },
            new() { Speaker = Kael, Line = "She's doubling the rents. Twelve families. Most of them have kids who run messages for me." },
            new() { Speaker = Kael, Line = "You might have met Wren in the market. Small, quick, tries everybody's belt. She's one of mine." },
            new()
            {
                Speaker = Kael, Line = "The Sunheart job would've paid all twelve rents for two years. I'd have been a saint. A very well-dressed saint.",
                Effects = new[] { DialogueEffect.Set(KaelQuestHook) },
                Options = new DialogueOption[]
                {
                    new() { Text = "You stole for them. That's not nothing.", Next = "noble", Effects = new[] { DialogueEffect.Add(KaelApproval, 5) } },
                    new() { Text = "So what now? The job's gone.", Next = "now" },
                    new() { Text = "Gannet sounds like someone who should meet {sb}.", Next = "meet", Effects = new[] { DialogueEffect.Add(KaelLoyalty, 3) } },
                },
            },
            new() { Id = "noble", Speaker = Kael, Direction = "squirming", Line = "Don't. Don't make it noble. I took a cut. A big one. I've got expensive taste in boots.", Next = "offer" },
            new() { Id = "now", Speaker = Kael, Line = "Now I find another job. Or Gannet finds the kids first. It's a race. I'm very fast.", Next = "offer" },
            new() { Id = "meet", Speaker = Kael, Direction = "delighted", Line = "Oh, I'd pay to watch that. I'd sell tickets." },
            new() { Id = "offer", Speaker = Kael, Line = "Your friend, though. {sb}. 'Come with us.' Just like that, in front of everyone." },
            new() { Speaker = Kael, Line = "People don't ask me to come with them. Usually it's 'come with us', and then a cell." },
            new()
            {
                Speaker = Kael, Line = "{sb} looked at me like I was worth something. You looked at me like you were counting your pockets.",
                Options = new DialogueOption[]
                {
                    new() { Text = "I was counting my pockets. I'm glad you're here anyway.", Next = "pockets", Effects = new[] { DialogueEffect.Add(KaelApproval, 3) } },
                    new() { Text = "{sb} sees the best in people. It's what {sb} does.", Next = "best", Effects = new[] { DialogueEffect.Add(KaelLoyalty, 3) } },
                    new() { Text = "You're worth more than what your hands can do, Kael.", Next = "worth", Effects = new[] { DialogueEffect.Add(KaelApproval, 5) } },
                },
            },
            new() { Id = "pockets", Speaker = Kael, Direction = "a real laugh", Line = "Honest. Horrible. I like it.", Next = "reformed" },
            new() { Id = "best", Speaker = Kael, Line = "Then {sb} is the first. I'll try not to disappoint the first.", Next = "reformed" },
            new() { Id = "worth", Speaker = Kael, Direction = "no joke, for a moment", Line = "...Steady on. You'll ruin my reputation." },
            new() { Id = "reformed", Speaker = Kael, Line = "Right. If anyone asks, I'm reformed. Mostly reformed. Reformed-adjacent.", Effects = new[] { DialogueEffect.Set(KaelTalk1) } },
        },
    };

    /// <summary>Kael after the court: whether knights keep thieves, the fire on the Low Quay, and his mother.</summary>
    public static readonly DialogueScript KaelKnights = new()
    {
        Id = "kael-talk-2",
        With = Kael,
        Needs = new[] { CourtDone, KaelRecruited },
        Unless = new[] { KaelTalk2 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Kael, Line = "Knights, I hear. Both of you, once you're back from the pass. The whole Flagon's buzzing with it." },
            new()
            {
                Speaker = Kael, Line = "Professional question. Does a knight keep a thief on staff? Asking for a friend. The friend is me.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You're not staff. You're one of us.", Next = "one-of-us", Effects = new[] { DialogueEffect.Add(KaelApproval, 5) } },
                    new() { Text = "{sb}'ll find you a title. {sb} looks after people.", Next = "title", Effects = new[] { DialogueEffect.Add(KaelLoyalty, 3) } },
                    new() { Text = "Depends. Will you stop stealing spoons?", Next = "spoons", Effects = new[] { DialogueEffect.Add(KaelApproval, 2) } },
                },
            },
            new() { Id = "one-of-us", Speaker = Kael, Direction = "after a moment", Line = "...One of us. You can't just say things like that to people. Somebody might believe you.", Next = "fire" },
            new() { Id = "title", Speaker = Kael, Line = "A title. Lord Pockets of the Low Ring. I'd wear it badly and often.", Next = "fire" },
            new() { Id = "spoons", Speaker = Kael, Line = "Define stealing. Define spoons. Define stop." },
            new() { Id = "fire", Speaker = Kael, Line = "Here's a thing about being Emberkin in a nice city. Every time a candle tips over, somebody looks at you." },
            new() { Speaker = Kael, Line = "When I was nine, a wool warehouse burned on the Low Quay. The Watch came straight down Tallow Lane and took my mother. Three days." },
            new() { Speaker = Kael, Line = "She came back with no job and a cough. We were out of the lane by the end of the month." },
            new()
            {
                Speaker = Kael, Line = "She never stole a thing in her life. Look where it got her. So I took up the family business. The other family business.",
                Options = new DialogueOption[]
                {
                    new() { Text = "I'm sorry. That should never have happened to her.", Next = "sorry", Effects = new[] { DialogueEffect.Add(KaelApproval, 3) } },
                    new() { Text = "Is she...?", Next = "is-she" },
                    new()
                    {
                        Text = "You're not as fine about it as you sound.",
                        Check = new DialogueCheck { Label = "INSIGHT", Ability = Ability.Wisdom },
                        OnSuccess = "not-fine", OnFailure = "wonderful",
                    },
                },
            },
            new() { Id = "sorry", Speaker = Kael, Line = "No. It shouldn't. Thank you. Don't do it again, I'll cry on your boots, and they're nice boots.", Next = "laugh" },
            new() { Id = "is-she", Speaker = Kael, Line = "Dead. Lye-lung. I was fourteen. It's fine. It's old. It's fine.", Next = "laugh" },
            new()
            {
                Id = "not-fine", Speaker = Kael, Direction = "the grin slipping", Line = "...No. I'm not. Her name was Ressa. Nobody's asked me her name in nine years.",
                Effects = new[] { DialogueEffect.Add(KaelApproval, 3) }, Next = "laugh",
            },
            new() { Id = "wonderful", Speaker = Kael, Line = "I'm wonderful. I'm always wonderful. It's my best trick." },
            new()
            {
                Id = "laugh", Speaker = Kael, Line = "Anyway. Into the king's own vault and out again with my skin on. My mother would've laughed herself sick.",
                Options = new DialogueOption[]
                {
                    new() { Text = "She'd have been proud of you.", Next = "proud", Effects = new[] { DialogueEffect.Add(KaelApproval, 2) } },
                    new()
                    {
                        Text = "For what it's worth, I'd steal you a better past if I could.", Next = "better-past", Flirt = true,
                        Conditions = new[] { DialogueCondition.Is(KaelFlirt), DialogueCondition.AtLeast(KaelApproval, RomanceSeed) },
                        Effects = new[] { DialogueEffect.Set(KaelFlirt2), DialogueEffect.Add(KaelApproval, 3) },
                    },
                    new() { Text = "Come on. Greywatch won't rob itself.", Next = "go" },
                },
            },
            new() { Id = "proud", Speaker = Kael, Line = "Proud. Of a thief. You've never met a laundress.", Next = "go" },
            new() { Id = "better-past", Speaker = Kael, Direction = "a long pause, no grin", Line = "...That's a line. That's my kind of line. Who taught you that?" },
            new() { Speaker = Kael, Line = "Careful, {name}. I'll start believing you, and then where will we be?" },
            new() { Id = "go", Speaker = Kael, Line = "Greywatch, then. I'll be the one complaining at the back.", Effects = new[] { DialogueEffect.Set(KaelTalk2) } },
        },
    };

    /// <summary>Brannock once he's joined: the crown paying his tab, Greywatch and Pim Hollis, the forty letters, and the Soul-Bound. The hook for his personal quest.</summary>
    public static readonly DialogueScript BrannockJoined = new()
    {
        Id = "brannock-talk-1",
        With = Brannock,
        Needs = new[] { BrannockRecruited },
        Unless = new[] { BrannockTalk1 },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Brannock, Direction = "checking the straps on his shield",
                Line = "If you've come to tell me I'm doing a fine thing, don't. I've been told I was doing fine things before.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Came to see if you're sober.", Next = "sober", Effects = new[] { DialogueEffect.Add(BrannockApproval, 2) } },
                    new() { Text = "Came to say thank you for coming.", Next = "thanks" },
                    new() { Text = "Came to hear about Greywatch.", Next = "greywatch" },
                },
            },
            new() { Id = "sober", Speaker = Brannock, Line = "Sober enough to hold a shield. Drunk enough to agree to this. That's the right amount.", Next = "tab" },
            new() { Id = "thanks", Speaker = Brannock, Line = "Don't thank me yet. Thank me at the other end, if there is one.", Next = "tab" },
            new() { Id = "greywatch", Speaker = Brannock, Line = "Course you did. Everybody does. Nobody wants to hear it twice. Sit, then." },
            new() { Id = "tab", Speaker = Brannock, Line = "Know who pays my tab at the Anvil? The crown. Every month a clerk comes down with a purse and doesn't look at me." },
            new() { Speaker = Brannock, Line = "Forty years of the king's money, poured down my throat. He thinks it's kindness. It's a muzzle. I drink it anyway." },
            new() { Speaker = Brannock, Line = "The last war. Forty men and me, on the inner gate at Greywatch. Six days, waiting on a relief column." },
            new() { Speaker = Brannock, Line = "It sat at Kestrel Ford for three of them. Orders from the old Lord Chancellor. 'It might be a trap.' A man in a warm room." },
            new() { Speaker = Brannock, Line = "Fourth night, my youngest asked me if they were coming. Pim Hollis. Seventeen. Ears like jug handles." },
            new() { Speaker = Brannock, Line = "I said, 'They're coming, lad. Hold.' I said it to all of them. Every night." },
            new()
            {
                Speaker = Brannock, Line = "They held, because I said so. That's what killed them. My word, with the crown's name stamped on it.",
                Options = new DialogueOption[]
                {
                    new() { Text = "You couldn't have known the relief would wait.", Next = "known", Effects = new[] { DialogueEffect.Add(BrannockApproval, 2) } },
                    new() { Text = "The ones who broke that promise were in Questeria. Not on the gate.", Next = "questeria" },
                    new() { Text = "(Say nothing. Let him finish.)", Next = "silence", Effects = new[] { DialogueEffect.Add(BrannockApproval, 5) } },
                },
            },
            new() { Id = "known", Speaker = Brannock, Line = "No. But I promised anyway. That's all a captain is: a promise other men make with your mouth.", Next = "letters" },
            new() { Id = "questeria", Speaker = Brannock, Line = "That's what the king says. He pays me to agree with him.", Next = "letters" },
            new() { Id = "silence", Speaker = Brannock, Direction = "after a while", Line = "...Most people fill the quiet. Thank you for not." },
            new() { Id = "letters", Speaker = Brannock, Line = "There's a bundle in the bottom of my pack. Forty letters. One for every family. I wrote them the winter after." },
            new() { Speaker = Brannock, Line = "Never sent one. Every year I tell myself I'll walk them round. Every year there's a tankard in the way." },
            new()
            {
                Speaker = Brannock, Line = "Kestrel Ford's on the road to the pass. Pim's sister still lives there, last I heard. Hester. She'll be old now.",
                Effects = new[] { DialogueEffect.Set(BrannockQuestHook) },
                Options = new DialogueOption[]
                {
                    new() { Text = "We'll stop at the ford. I'll walk with you.", Next = "walk", Effects = new[] { DialogueEffect.Add(BrannockApproval, 5) } },
                    new() { Text = "Forty years is a long time. Maybe let them rest.", Next = "rest", Effects = new[] { DialogueEffect.Add(BrannockApproval, -2) } },
                    new() { Text = "Then send them. Before the pass.", Next = "send" },
                },
            },
            new() { Id = "walk", Speaker = Brannock, Line = "...We'll see. I'm not promising anything. I don't do that any more.", Next = "warlord" },
            new() { Id = "rest", Speaker = Brannock, Line = "Maybe. Or maybe that's the coward's way, and you're too polite to say it.", Next = "warlord" },
            new() { Id = "send", Speaker = Brannock, Line = "Spoken like somebody who's never had to write one. ...But you're not wrong." },
            new()
            {
                Id = "warlord", Speaker = Brannock, Conditions = new[] { DialogueCondition.Is(BrannockWary) },
                Line = "Your friend. 'We'll give you enough.' Every warlord I ever fought said that, near enough word for word, to men who'd been given too little.",
            },
            new() { Speaker = Brannock, Conditions = new[] { DialogueCondition.Is(BrannockWary) }, Line = "I'm not saying {sb} is one. I'm saying I know the tune." },
            new()
            {
                Speaker = Brannock, Conditions = new[] { DialogueCondition.Not(BrannockWary) },
                Line = "Your friend's got fire in {sb_them}. I've seen fire hold a gate. I've seen it take the gate down with it.",
            },
            new()
            {
                Speaker = Brannock, Line = "So. What's {sb} to you, little one? Truly.",
                Options = new DialogueOption[]
                {
                    new() { Text = "{sb} is the most loyal person I know.", Next = "loyal", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new() { Text = "Keep an eye on {sb} with me.", Next = "eye", Effects = new[] { DialogueEffect.Add(BrannockApproval, 3) } },
                    new()
                    {
                        Text = "You're judging {sb} on one sentence.", Next = "judging",
                        Effects = new[] { DialogueEffect.Add(BrannockApproval, -2), DialogueEffect.Add(Meters.Bond, 2) },
                    },
                },
            },
            new() { Id = "loyal", Speaker = Brannock, Line = "Loyal to what, though? That's the only question that ever mattered.", Next = "dawn" },
            new() { Id = "eye", Speaker = Brannock, Line = "Two eyes. Old ones. They still work.", Next = "dawn" },
            new() { Id = "judging", Speaker = Brannock, Line = "I'm judging on two hundred years of sentences, little one. Most of them weren't {sb}'s." },
            new() { Id = "dawn", Speaker = Brannock, Line = "Dawn. East gate. I'll be the one who looks like he's been hit by a cart.", Effects = new[] { DialogueEffect.Set(BrannockTalk1) } },
        },
    };
}
