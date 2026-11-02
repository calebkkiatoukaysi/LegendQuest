using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// Act 1, Chapter 4, "The Sunheart": the royal messenger, Princess Elara in the Flagon's back booth,
/// Kael in the Old Crown Vault, the audience with King Leon, Chancellor Crane in the great hall, and
/// Brannock Ironvein at the Rusty Anvil.
/// </summary>
/// <remarks>
/// STORY.md's A1-C4-01 to A1-C4-05, and what it leaves to fill: the messenger who finds you after
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

    /// <summary>Kael's approval.</summary>
    private const string KaelAffinity = "affinity-kael";

    /// <summary>Elara's approval.</summary>
    private const string ElaraAffinity = "affinity-elara";

    /// <summary>Brannock's approval.</summary>
    private const string BrannockAffinity = "affinity-brannock";

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

    /// <summary>A1-C4-01: the hooded woman in the back booth, the stolen Sunheart, and the way down into the vault.</summary>
    public static readonly DialogueScript Summons = new()
    {
        Id = "a1-c4-01",
        With = "elara",
        Needs = new[] { Act1Chapter3.ChapterDone },
        Unless = new[] { SummonsDone },
        Marker = MarkerKind.Event,
        Nodes = new DialogueNode[]
        {
            // The chapter's card, if the messenger didn't get to you first.
            new() { Speaker = Speakers.Chapter, Line = "Chapter 4  ·  The Sunheart", Conditions = new[] { DialogueCondition.Not(MessengerMet) } },
            new() { Speaker = Speakers.Narrator, Line = "In the back booth, a woman sits alone in a plain travelling cloak with the hood up, and a cup in front of her she hasn't touched." },
            new() { Speaker = Speakers.Narrator, Line = "Under the hood: a sharp face, and eyes that have already counted every way out of the room." },
            new() { Speaker = SoulBound, Direction = "barely a whisper", Line = "...Is that the..." },
            new() { Speaker = "elara", Line = "Sit. Don't bow, people are watching. You're the ones from Thornbridge." },
            new()
            {
                Speaker = "elara", Direction = "a glance across the table", Line = "And {sb}. They're singing your name at the east gate. Don't let it go to your head. Songs are cheap.",
                Conditions = new[] { DialogueCondition.Is(Act1Chapter3.CrowdLoved) },
            },
            new() { Speaker = "elara", Line = "Three nights ago, someone broke into the Old Crown Vault beneath the city and took the Sunheart: the gem that powers Questeria's old ward-walls." },
            new() { Speaker = "elara", Line = "Without it, the city is just stone. My father doesn't want a panic. I want it back before anyone outside this booth knows it's gone." },
            new() { Speaker = SoulBound, Line = "And why not send your own guards?" },
            new()
            {
                Speaker = "elara", Line = "Because I don't know which of them to trust.",
                Options = new DialogueOption[]
                {
                    new() { Text = "We'll get it back.", Next = "back" },
                    new() { Text = "Why us?", Next = "why" },
                    new()
                    {
                        Text = "What's it worth to you?", Next = "worth",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Pragmatic) }, Locked = true,
                    },
                    new()
                    {
                        Text = "Do princesses usually drink at the Flagon?", Next = "drink",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "back", Speaker = "elara", Line = "Good. I like people who don't make speeches.", Next = "mira" },
            new() { Id = "why", Speaker = "elara", Line = "Because you walked out to Thornbridge when nobody was paying you to. That's rarer in this city than you'd think.", Next = "mira" },
            new() { Id = "worth", Speaker = "elara", Line = "Gold, and a princess who owes you a favour. Only one of those is easy to come by.", Next = "mira" },
            new() { Id = "drink", Speaker = "elara", Line = "This one does. The ale's terrible, and nobody looks twice at a woman in a hood." },

            new()
            {
                Id = "mira", Speaker = "mira", Line = "Your Highness. When this is done, I need to speak to the King. About Doomward, and the Box.",
                Conditions = new[] { DialogueCondition.With("mira") },
            },
            new()
            {
                Speaker = "elara", Direction = "a long look at her", Line = "Bring me the Sunheart and I'll put you in front of him myself.",
                Conditions = new[] { DialogueCondition.With("mira") },
            },
            new() { Speaker = "elara", Line = "There's an iron door in the terrace wall below the Sunspire. It's unlocked, and the guard on it is one of mine." },
            new()
            {
                Speaker = "elara", Line = "Go down. Find my gem. Come back up the same way. Quietly.",
                Effects = new[] { DialogueEffect.Set(SummonsDone), DialogueEffect.Objective("Go down into the Old Crown Vault, below the Sunspire") },
            },
            new() { Speaker = Speakers.Narrator, Line = "She leaves a coin on the table for the drink she didn't touch, and then she's gone." },
        },
    };

    /// <summary>A1-C4-02 up to the duel: Kael on the relic shelf, and the Sunheart he won't hand over.</summary>
    /// <remarks>It's with Kael as well as on the trigger before him, so walking up and talking to him starts it too, until he's called the truce.</remarks>
    public static readonly DialogueScript Vault = new()
    {
        Id = "a1-c4-02",
        With = "kael",
        Unless = new[] { KaelTruce, VaultDone },
        Marker = MarkerKind.Event,
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator,
                Line = "Deep in the vault, someone's halfway up a shelf of relics: an Emberkin in a dark cloak, horns swept back, tail flicking for balance. A gold light glows through the pouch at his hip.",
            },
            new() { Speaker = "kael", Line = "Ah. Company. I'd say make yourselves at home, but it's technically the king's home, and I'm technically robbing it." },
            new() { Speaker = SoulBound, Line = "Hand it over." },
            new() { Speaker = "kael", Line = "Love to. Can't. Someone's paying me very well to keep it. And I'm very fond of being paid." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "He drops off the shelf and lands like a cat, with a knife in each hand.",
                Effects = new[] { DialogueEffect.Fight("a1-c4-02-kael") },
            },
        },
    };

    /// <summary>A1-C4-02 once Kael's had enough of the duel: the sappers breaking in, and the truce.</summary>
    /// <remarks>EncounterCatalog's a1-c4-02-kael plays it when Kael gives up, which has set KaelTruce and brought the sappers up through the floor. It ends on the fight with them.</remarks>
    public static readonly DialogueScript Truce = new()
    {
        Id = "a1-c4-02-kael-after",
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator,
                Line = "Kael's out of smoke and out of room, his back to the shelves, a knife still up. Then the floor behind him cracks.",
            },
            new() { Speaker = Speakers.Narrator, Line = "Stone bursts upward. Grey shapes climb out of a fresh tunnel with picks and hooded lamps, and there are more behind them." },
            new()
            {
                Speaker = "kael", Direction = "staring", Line = "...Those are not my employers. Truce?",
                Options = new DialogueOption[]
                {
                    new() { Text = "Truce. Fight now, argue later.", Effects = new[] { DialogueEffect.Add(KaelAffinity, 5) } },
                    new()
                    {
                        Text = "Only if you stop being so distracting.", Next = "flirt", Flirt = true,
                        Effects = new[] { DialogueEffect.Add(KaelAffinity, 3), DialogueEffect.Set(KaelFlirt) },
                    },
                    new() { Text = "(Let {sb} answer.)", Next = "sb" },
                },
            },
            new() { Speaker = "kael", Line = "Fight now. Argue later. My two favourite things.", Next = "sappers" },
            new() { Id = "flirt", Speaker = "kael", Line = "No promises, gorgeous.", Next = "sappers" },
            new() { Id = "sb", Speaker = SoulBound, Line = "Fine. But I'm watching you, thief." },
            new() { Speaker = "kael", Direction = "admiring", Line = "Please do. I've only got good sides." },

            // Their chief, up last out of the hole.
            new()
            {
                Id = "sappers", Speaker = Speakers.Narrator,
                Line = "Something bigger hauls itself up out of the hole behind them, a lantern in one fist and a pick in the other.",
                Effects = new[] { DialogueEffect.Fight("a1-c4-02-sappers") },
            },
        },
    };

    /// <summary>The rest of A1-C4-02, once the sappers are beaten: the Sunheart handed over, the black-iron coin, and Kael joining.</summary>
    /// <remarks>EncounterCatalog's a1-c4-02-sappers plays it when the fight's won, which has set SappersBeaten.</remarks>
    public static readonly DialogueScript KaelJoins = new()
    {
        Id = "a1-c4-02-sappers-after",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Narrator, Line = "In the quiet after, Kael looks at the hole in the floor for a long moment. Then he unhooks the pouch from his belt." },
            new()
            {
                Speaker = "kael", Line = "Here. Before I change my mind. It's been warm against my hip for three days. I'll miss it.",
                Effects = new[] { DialogueEffect.Give("sunheart") },
            },
            new() { Speaker = "kael", Line = "Here's the thing. My buyer paid half up front. In these." },
            new() { Speaker = Speakers.Narrator, Sound = Sfx.Coin, Line = "He flips a coin and catches it: black metal, stamped with a cube." },
            new() { Speaker = "kael", Line = "Doomward coin. Someone in your pretty city is working for the other side." },
            new() { Speaker = "mira", Line = "Black-iron marks. Maleficio's agents pay in them. Whoever hired you is close to the crown." },
            new()
            {
                Speaker = "kael", Line = "Never saw a face. Hooded, perfumed, spoke like he'd been to finishing school. Here. You'll want this more than I do.",
                Effects = new[] { DialogueEffect.Set(SpySeed), DialogueEffect.Give("black-iron-coin") },
            },
            new() { Speaker = SoulBound, Direction = "to Kael", Line = "You're good. Wasted on pockets. Come with us." },
            new() { Speaker = "kael", Line = "Is that an offer of employment or friendship? Because I'm bad at one of them." },
            new() { Speaker = SoulBound, Line = "Both. You'll learn." },
            new() { Speaker = "sera", Direction = "flatly", Line = "He'll rob us blind.", Conditions = new[] { DialogueCondition.With("sera") } },
            new() { Speaker = "kael", Line = "Only a little. Friends' rates.", Conditions = new[] { DialogueCondition.With("sera") } },
            new()
            {
                Speaker = "kael", Direction = "grinning", Line = "I'll keep a seat warm at the Flagon. Five's a crowd, down a hole like this.",
                Effects = new[]
                {
                    DialogueEffect.Recruit("kael"), DialogueEffect.Set(KaelRecruited), DialogueEffect.Set(VaultDone),
                    DialogueEffect.Objective("Take the Sunheart to the Sunspire Keep"),
                },
            },
        },
    };

    /// <summary>A1-C4-03: the audience with King Leon, the Sunheart handed back, Mira's warning, the Soul-Bound out of turn, and the King's decision.</summary>
    public static readonly DialogueScript Audience = new()
    {
        Id = "a1-c4-03",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Narrator, Line = "The throne room of the Sunspire: gold light through tall windows, and the Aurelian sun on every banner." },
            new()
            {
                Speaker = Speakers.Narrator,
                Line = "King Leon sits the throne, grey-bearded and very still. Elara stands at his right, the travelling cloak gone. At his left, a man in plum silk with a ring on every finger.",
            },
            new() { Speaker = "leon", Line = "So these are the heroes of Saltwind, Mirewood and Thornbridge. And now, I'm told, the Vault. My daughter keeps secrets poorly when she's proud of someone." },
            new() { Speaker = "elara", Line = "Father." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "You hold out the Sunheart. Elara takes it in both hands, and for a moment the whole room seems warmer.",
                Effects = new[] { DialogueEffect.Take("sunheart") },
            },
            new() { Speaker = "leon", Line = "Fable thanks you. I thank you." },
            new() { Speaker = Speakers.Narrator, Line = "He stands, and bows his head to you. The court murmurs." },
            new() { Speaker = SoulBound, Direction = "whispering to you, moved", Line = "...He actually said it." },
            new() { Speaker = "mira", Direction = "stepping forward", Line = "Your Majesty. I am Mira Ashgrove of the Ember Archive. Maleficio's army is real." },
            new() { Speaker = "mira", Line = "The Hollowed raided Thornbridge. His agents are inside your walls. He will come through Greywatch Pass, and soon." },
            new() { Speaker = "crane", Line = "A refugee's fears, Majesty. Doomward has always been dramatic." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "Beside you, Kael's nose wrinkles at something on the air. He says nothing.",
                Conditions = new[] { DialogueCondition.With("kael") },
            },
            new() { Speaker = "leon", Line = "I fought the last war, Vesper. I know how they begin: with men in warm rooms calling the danger dramatic." },
            new() { Speaker = "leon", Direction = "a beat", Line = "But I also know how they end. I will not empty Fable's villages of their sons on the word of one frightened scholar." },
            new() { Speaker = SoulBound, Direction = "stepping forward, out of turn", Line = "With respect, Your Majesty, they're already dying. Just not your villages yet." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "Silence. Along the walls, the guards shift their grip on their spears.",
                Options = new DialogueOption[]
                {
                    new()
                    {
                        Text = "Forgive my friend, Majesty. We've seen too much this week.", Next = "forgive",
                        Effects = new[] { DialogueEffect.Add(Meters.Bond, -2) },
                    },
                    new()
                    {
                        Text = "{sb}'s right. Waiting costs lives.", Next = "backed",
                        Effects = new[] { DialogueEffect.Add(Meters.Bond, 5), DialogueEffect.Set(BackedSbCourt) },
                    },
                    new() { Text = "(Say nothing.)", Next = "nothing", Effects = new[] { DialogueEffect.Add(ElaraAffinity, 5) } },
                },
            },
            new() { Id = "forgive", Speaker = "leon", Line = "No need. I'd rather be told the truth rudely than lies politely." },
            new() { Speaker = SoulBound, Direction = "muttering", Line = "I didn't need you to apologise for me.", Next = "decision" },
            new() { Id = "backed", Speaker = "leon", Direction = "a long pause", Line = "Then prove it to me.", Next = "decision" },
            new() { Id = "nothing", Speaker = "elara", Direction = "stepping in", Line = "Father, let them go to Greywatch. Let them see." },

            new() { Id = "decision", Speaker = "leon", Line = "Here is my decision. A supply caravan leaves for Greywatch Pass at dawn. You will escort it, and bring me Commander Aldric's own report." },
            new() { Speaker = "leon", Line = "If the threat is what you say, I will call the banners. And when you return, you will kneel, and rise as Knights of Fable." },
            new() { Speaker = SoulBound, Direction = "eyes bright", Line = "Knights." },
            new() { Speaker = "leon", Line = "One more thing. Brannock Ironvein. Once my best captain, now my most expensive drunk. He knows that pass better than anyone alive. Take him." },
            new()
            {
                Speaker = "elara", Line = "He drinks at the Rusty Anvil, on the docks. If he says no, tell him he still owes me a horse.",
                Effects = new[] { DialogueEffect.Set(AudienceDone), DialogueEffect.Objective("Find Brannock Ironvein at the Rusty Anvil, on the docks") },
            },
        },
    };

    /// <summary>A1-C4-04: the Soul-Bound falls behind in the great hall, and the Lord Chancellor is waiting.</summary>
    public static readonly DialogueScript Crane = new()
    {
        Id = "a1-c4-04",
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator,
                Line = "On the way out through the great hall, {sb} falls a step behind. When you turn, the Lord Chancellor has drifted into {sb_their} path: plum silk, quiet rings, and a smell of rose oil.",
            },
            new() { Speaker = Speakers.Narrator, Line = "You're too far off to step in. Close enough to hear." },
            new() { Speaker = "crane", Line = "A remarkable speech. The king has been surrounded by cautious men so long, he's forgotten what conviction sounds like." },
            new() { Speaker = SoulBound, Line = "He seems like a good man." },
            new() { Speaker = "crane", Line = "Oh, the best of men. Good men make lovely kings in peacetime." },
            new() { Speaker = "crane", Direction = "smiling", Line = "Wars need different men. Remember that, {sb}. Knight of Fable." },
            new() { Speaker = SoulBound, Line = "I'm not a knight yet." },
            new()
            {
                Speaker = "crane", Line = "Not yet.",
                Effects = new[] { DialogueEffect.Add(Meters.Ambition, 5), DialogueEffect.Set(CraneContact) },
            },
            new()
            {
                Speaker = Speakers.Narrator, Line = "He inclines his head and drifts away down the hall, rings catching the light. {sb} watches him go, then comes back to you.",
                Options = new DialogueOption[]
                {
                    new() { Text = "What did he want?", Next = "want" },
                    new() { Text = "Stay away from that man.", Next = "away" },
                    new() { Text = "(Say nothing.)", Next = "nothing" },
                },
            },
            new() { Id = "want", Speaker = SoulBound, Line = "To tell me I'm wonderful. I didn't argue.", Next = "go" },
            new() { Id = "away", Speaker = SoulBound, Direction = "laughing", Line = "Relax. I know a courtier when I smell one." },
            new() { Speaker = SoulBound, Direction = "quieter", Line = "...He's not wrong, though. About wars.", Next = "go" },
            new() { Id = "nothing", Speaker = SoulBound, Direction = "grinning", Line = "Knights, {name}. Us. Can you imagine?" },
            new()
            {
                Id = "go", Speaker = SoulBound, Line = "Come on. Brannock Ironvein isn't going to drink himself sober.",
                Effects = new[] { DialogueEffect.Set(CraneDone) },
            },
        },
    };

    /// <summary>A1-C4-05: Brannock Ironvein at the Rusty Anvil, Greywatch, and the forty men.</summary>
    public static readonly DialogueScript Brannock = new()
    {
        Id = "a1-c4-05",
        With = "brannock",
        Needs = new[] { AudienceDone },
        Unless = new[] { BrannockRecruited },
        Marker = MarkerKind.Event,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Narrator, Line = "In the Anvil's darkest corner, a huge Stoneborn slumps over a tankard. His beard is braided with tarnished royal clasps." },
            new() { Speaker = "brannock", Line = "If the king sent you, tell him I'm busy. If you're buying, sit." },
            new() { Speaker = SoulBound, Line = "Greywatch Pass. Dawn. The king says you know it." },
            new() { Speaker = Speakers.Narrator, Line = "The tankard stops halfway to his mouth." },
            new() { Speaker = "brannock", Direction = "very still", Line = "...Greywatch." },
            new()
            {
                Speaker = "brannock", Line = "Lost forty men there in the last war, holding a gate everyone said would fall. It didn't. They did.",
                Options = new DialogueOption[]
                {
                    new()
                    {
                        Text = "Then help us make sure it doesn't happen again.", Next = "again",
                        Effects = new[] { DialogueEffect.Add(BrannockAffinity, 5) },
                    },
                    new()
                    {
                        Text = "Stay here and drink, then. We'll go without you.", Next = "without",
                        Effects = new[] { DialogueEffect.Add(BrannockAffinity, 3) },
                    },
                    new() { Text = "(Let {sb} answer.)", Next = "sb" },
                },
            },
            new() { Id = "again", Speaker = "brannock", Direction = "looking at you a long while", Line = "...Nobody makes sure of anything at Greywatch. You stand in the gap and you hope." },
            new() { Speaker = "brannock", Line = "But I'd sooner hope up there than down here.", Next = "stands" },
            new() { Id = "without", Speaker = "brannock", Direction = "bristling", Line = "Like hell you will.", Next = "stands" },
            new() { Id = "sb", Speaker = SoulBound, Line = "You lost forty men because someone above you wouldn't give you enough. Come with us. We'll give you enough." },
            new()
            {
                Speaker = "brannock", Direction = "a long, hard look", Line = "...Careful, youngling. That's how every warlord I ever fought started talking.",
                Effects = new[] { DialogueEffect.Set(BrannockWary) },
            },

            new() { Id = "stands", Speaker = Speakers.Narrator, Line = "He drains the tankard, sets it down very gently, and stands. Standing, he fills the corner." },
            new() { Speaker = "brannock", Line = "Dawn, then. I'll want my armour back out of hock. And somebody to settle my tab." },
            new() { Speaker = "anvil-barkeep", Direction = "from behind the bar", Line = "Eleven months, Brannock." },
            new() { Speaker = "brannock", Line = "...The king's paying." },
            new()
            {
                Speaker = "brannock", Line = "I'll sleep it off at the Flagon. The Anvil knows me too well.",
                Effects = new[]
                {
                    DialogueEffect.Recruit("brannock"), DialogueEffect.Set(BrannockRecruited), DialogueEffect.Set(ChapterDone),
                    DialogueEffect.Objective("The caravan for Greywatch leaves at dawn: meet it at the east gate"),
                },
            },
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
