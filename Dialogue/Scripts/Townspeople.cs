using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// The capital's people who'll stop and talk: the harbour's, the Lantern Market's, the temple's and
/// the palace terrace's, and the hooks for the four side quests on the board.
/// </summary>
/// <remarks>
/// Only Old Tomas and Sergeant Brisk are the story's; the rest are mine, written round its bible (the
/// Watch sent east, the Mirewood going wrong, the Princess riding out on her own). The side quests
/// (SQ-Cellar, SQ-Ledger, SQ-Tollroad and SQ-Widow) are only hooks until they're built: hearing one
/// sets a flag, and none of them touches what you're to do next, which stays the main quest's. Each
/// person has a conversation that sets a flag, then something shorter to say once it has. The hooks
/// get the quest marker over their heads until you've heard them out.
/// </remarks>
public static class Townspeople
{
    private const string You = Origins.You;
    private const string SoulBound = Origins.SoulBound;

    /// <summary>SQ-Cellar's hook: Pip's told you about the black-eyed rats in the Flagon's cellar.</summary>
    public const string CellarHeard = "sq-cellar-heard";

    /// <summary>SQ-Ledger's hook: Osric's told you about his stolen ledger.</summary>
    public const string LedgerHeard = "sq-ledger-heard";

    /// <summary>SQ-Tollroad's hook: Sergeant Brisk's told you about the highwaymen on the Mirewood road.</summary>
    public const string TollroadHeard = "sq-tollroad-heard";

    /// <summary>SQ-Widow's hook: Maud's told you about her ring in the harbour shallows.</summary>
    public const string WidowHeard = "sq-widow-heard";

    private const string TomasMet = "met-tomas";
    private const string NettyMet = "met-netty";
    private const string WrenMet = "met-wren";
    private const string LarkMet = "met-lark";
    private const string IlsaMet = "met-ilsa";
    private const string TamsinMet = "met-tamsin";
    private const string OswinMet = "met-oswin";
    private const string HesperMet = "met-hesper";

    /// <summary>Old Tomas once the docks are quiet: his hook, and whether the Red Gulls will be back.</summary>
    public static readonly DialogueScript Tomas = new()
    {
        Id = "tomas-talk",
        With = "tomas",
        Needs = new[] { Act1Chapter1.DocksDone },
        Unless = new[] { TomasMet },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "tomas", Line = "Forty years on this harbour, and nobody's ever stood up for me. Not once. Not till you two." },
            new() { Speaker = SoulBound, Line = "Somebody should have. A long time ago." },
            new()
            {
                Speaker = "tomas", Line = "That hook bringing you any luck yet?",
                Options = new DialogueOption[]
                {
                    new() { Text = "Not yet. Give it a day.", Next = "a-day" },
                    new()
                    {
                        Text = "I caught a Red Gull with it. Does that count?", Next = "caught",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                    new() { Text = "Will the Red Gulls come back for you?", Next = "back" },
                },
            },
            new() { Id = "a-day", Speaker = "tomas", Line = "Took me thirty years to get any. You'll be quicker. You've the look.", Next = "go-on" },
            new() { Id = "caught", Speaker = "tomas", Direction = "wheezing", Line = "Ha! Biggest catch this harbour's seen all year. It counts.", Next = "go-on" },
            new() { Id = "back", Speaker = "tomas", Line = "Not this week. Next week, who knows. That's the harbour." },
            new() { Speaker = SoulBound, Direction = "quietly", Line = "Then next week they'll find us here instead." },
            new() { Id = "go-on", Speaker = "tomas", Line = "Go on up, then. The city's grand. Just remember it's got mud, same as anywhere." },
            new()
            {
                Speaker = SoulBound, Line = "We remember. Believe me.",
                Effects = new[] { DialogueEffect.Set(TomasMet) },
            },
        },
    };

    /// <summary>Old Tomas after that.</summary>
    public static readonly DialogueScript TomasAfter = new()
    {
        Id = "tomas-after",
        With = "tomas",
        Needs = new[] { TomasMet },
        Nodes = new DialogueNode[] { new() { Speaker = "tomas", Line = "Catch is good today. Nobody's taxing it, either." } },
    };

    /// <summary>Netty the net-mender on the boardwalk: what the Red Gulls have been taking, and who might be behind them.</summary>
    public static readonly DialogueScript Netty = new()
    {
        Id = "netty-talk",
        With = "netty",
        Needs = new[] { Act1Chapter1.GullsBeaten },
        Unless = new[] { NettyMet },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "netty", Direction = "not looking up from the net", Line = "Mind the hooks, love. Saw you sort the Red Gulls. Best show this harbour's had all year." },
            new() { Speaker = SoulBound, Line = "Do they come round often?" },
            new() { Speaker = "netty", Line = "Every week since the Watch went thin. A tax for the dock, a tax for the rope, a tax for breathing salt air." },
            new() { Speaker = SoulBound, Direction = "jaw tight", Line = "And nobody stops them." },
            new()
            {
                Speaker = "netty", Line = "Who'd stop them? Hale's got half a Watch, and the other half's on the road to Greywatch.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Where do the Red Gulls come from?", Next = "where" },
                    new()
                    {
                        Text = "Somebody's paying them. Thugs don't run a racket on their own.", Next = "paying",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Pragmatic) }, Locked = true,
                    },
                    new()
                    {
                        Text = "If they come back, send for us.", Next = "send",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Honorable) }, Locked = true,
                        Effects = new[] { DialogueEffect.Add(Meters.Fame, 1) },
                    },
                    new() { Text = "Good luck with the nets.", Next = "nets" },
                },
            },
            new() { Id = "where", Speaker = "netty", Line = "The Low Ring, mostly. They drink at a place with no sign and no windows. Don't go looking.", Next = "nets" },
            new() { Id = "paying", Speaker = "netty", Direction = "a sharp look", Line = "Clever. Nobody says. But somebody's buying their beer, and it isn't the fish.", Next = "nets" },
            new() { Id = "send", Speaker = "netty", Line = "Bless you. I'll shout loud enough for the palace to hear." },
            new()
            {
                Id = "nets", Speaker = "netty", Line = "Off you go. Nets don't mend themselves, and neither do harbours.",
                Effects = new[] { DialogueEffect.Set(NettyMet) },
            },
        },
    };

    /// <summary>Netty after that.</summary>
    public static readonly DialogueScript NettyAfter = new()
    {
        Id = "netty-after",
        With = "netty",
        Needs = new[] { NettyMet },
        Nodes = new DialogueNode[] { new() { Speaker = "netty", Line = "Still here, still mending. Mind the hooks." } },
    };

    /// <summary>SQ-Widow's hook: Maud, the bellringer's widow, on the jetty, and the ring she lost in the shallows.</summary>
    public static readonly DialogueScript Widow = new()
    {
        Id = "sq-widow",
        With = "maud",
        Marker = MarkerKind.Quest,
        Needs = new[] { Act1Chapter1.GullsBeaten },
        Unless = new[] { WidowHeard },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "maud", Direction = "staring at the water", Line = "Oh. I'm sorry, I'm in the way. I'm always in the way lately." },
            new() { Speaker = SoulBound, Line = "You're not. Are you all right?" },
            new() { Speaker = "maud", Line = "My Edric rang the temple bells for thirty years. He passed in the spring." },
            new() { Speaker = "maud", Line = "I come down to feed the gulls, the way he did. Yesterday my ring slipped off. Into the shallows, by the jetty posts." },
            new()
            {
                Speaker = "maud", Direction = "turning her bare finger", Line = "Thin fingers. Grief does that. It's only gold and an old promise. But it's mine.",
                Options = new DialogueOption[]
                {
                    new() { Text = "We'll keep an eye out for it.", Next = "eye-out" },
                    new()
                    {
                        Text = "There's more to it than gold, isn't there?",
                        Check = new DialogueCheck { Label = "INSIGHT", Ability = Ability.Wisdom },
                        OnSuccess = "engraved", OnFailure = "only-ring",
                    },
                    new()
                    {
                        Text = "Want me to ask the gulls if they took it?", Next = "gulls",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "eye-out", Speaker = "maud", Line = "Would you? Edric would have liked you. He liked anyone who'd stop.", Next = "promise" },
            new() { Id = "engraved", Speaker = "maud", Direction = "after a while", Line = "...It's engraved inside. 'Ring true.' A bellringer's joke. He thought it was very funny.", Next = "promise" },
            new() { Id = "only-ring", Speaker = "maud", Line = "Nothing more, dear. Just an old woman and her ring.", Next = "promise" },
            new() { Id = "gulls", Speaker = "maud", Direction = "a small laugh", Line = "He talked to them too. They never answered him either." },
            new()
            {
                Id = "promise", Speaker = SoulBound, Direction = "quietly, to {name}", Line = "We're finding that ring.",
                Effects = new[] { DialogueEffect.Set(WidowHeard) },
            },
        },
    };

    /// <summary>Maud, still waiting on the ring.</summary>
    public static readonly DialogueScript WidowWaiting = new()
    {
        Id = "sq-widow-waiting",
        With = "maud",
        Needs = new[] { WidowHeard },
        Nodes = new DialogueNode[] { new() { Speaker = "maud", Line = "Any luck? It's by the jetty posts, I'm sure of it. I come down every morning." } },
    };

    /// <summary>SQ-Cellar's hook: Pip, the Flagon's potboy, and the rats in the cellar with eyes black as ink.</summary>
    public static readonly DialogueScript Cellar = new()
    {
        Id = "sq-cellar",
        With = "pip",
        Marker = MarkerKind.Quest,
        Unless = new[] { CellarHeard },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "pip", Direction = "under a tray of empties", Line = "Don't go down the cellar. I'm not saying why. I'm just saying don't.",
                Options = new DialogueOption[]
                {
                    new() { Text = "What's down there?", Next = "rats" },
                    new() { Text = "All right. We won't.", Next = "curious" },
                    new()
                    {
                        Text = "Well, now I have to go down the cellar.", Next = "have-to",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "curious", Speaker = "pip", Line = "...You're not even curious? Everybody's curious.", Next = "rats" },
            new() { Id = "have-to", Speaker = "pip", Line = "That's what the last one said. He came back up the stairs very fast.", Next = "rats" },
            new() { Id = "rats", Speaker = "pip", Direction = "lowering his voice", Line = "Rats. Big as cats. Eyes black as ink, all the way through." },
            new() { Speaker = "pip", Line = "They don't squeak. They don't run from the lamp. They just sit there and watch it." },
            new() { Speaker = SoulBound, Line = "Lovely. Does your boss know?" },
            new()
            {
                Speaker = "pip", Line = "It's on the quest board. Bad for trade, telling customers. So I'm only telling you.",
                Options = new DialogueOption[]
                {
                    new() { Text = "We'll look into it.", Next = "boot" },
                    new()
                    {
                        Text = "You've been down there, haven't you?",
                        Check = new DialogueCheck { Label = "INSIGHT", Ability = Ability.Wisdom },
                        OnSuccess = "been", OnFailure = "not-been",
                    },
                },
            },
            new() { Id = "been", Speaker = "pip", Direction = "very quietly", Line = "...Once. One of them sat on the stairs and didn't move. I dropped the lamp and ran." },
            new() { Speaker = SoulBound, Line = "Smart. Running's underrated.", Next = "boot" },
            new() { Id = "not-been", Speaker = "pip", Line = "No! Maybe. Stop looking at me like that." },
            new()
            {
                Id = "boot", Speaker = "pip", Line = "Bring a big boot. A really big one.",
                Effects = new[] { DialogueEffect.Set(CellarHeard) },
            },
        },
    };

    /// <summary>Pip after that.</summary>
    public static readonly DialogueScript CellarAfter = new()
    {
        Id = "sq-cellar-after",
        With = "pip",
        Needs = new[] { CellarHeard },
        Nodes = new DialogueNode[] { new() { Speaker = "pip", Line = "Big boot. I mean it. And don't tell the boss I told you." } },
    };

    /// <summary>SQ-Ledger's hook: Osric Penhallow, a cloth merchant in the Lantern Market, and his stolen ledger.</summary>
    public static readonly DialogueScript Ledger = new()
    {
        Id = "sq-ledger",
        With = "osric",
        Marker = MarkerKind.Quest,
        Unless = new[] { LedgerHeard },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "osric", Direction = "wringing his gloves", Line = "You. You look capable. Are you capable? Please say yes.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Yes." },
                    new() { Text = "Depends what for.", Next = "ledger" },
                },
            },
            new() { Speaker = "osric", Line = "Marvellous. Wonderful. I have a disaster." },
            new() { Id = "ledger", Speaker = "osric", Line = "My ledger's been stolen. Every sale, every debt, every name. Fifteen years of business in one green book." },
            new() { Speaker = SoulBound, Line = "Who steals a book of sums?" },
            new()
            {
                Speaker = "osric", Line = "Somebody who wants to know who owes me. It was seen in the Low Ring, which is where things go to vanish.",
                Options = new DialogueOption[]
                {
                    new()
                    {
                        Text = "What's it worth to you?", Next = "worth",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Pragmatic) }, Locked = true,
                    },
                    new() { Text = "Who was near it last?", Next = "last" },
                    new()
                    {
                        Text = "What's really in that book, Osric?",
                        Check = new DialogueCheck { Label = "INSIGHT", Ability = Ability.Wisdom },
                        OnSuccess = "really", OnFailure = "sums",
                    },
                    new() { Text = "We'll keep an eye out.", Next = "unopened" },
                },
            },
            new() { Id = "worth", Speaker = "osric", Direction = "a pained smile", Line = "More than I'll say out loud in a market. It's on the board. Generously.", Next = "unopened" },
            new() { Id = "last", Speaker = "osric", Line = "A boy delivering silk. A red scarf round his neck. I thought nothing of it." },
            new() { Speaker = SoulBound, Line = "A red scarf. Red Gulls, maybe.", Next = "unopened" },
            new() { Id = "really", Speaker = "osric", Direction = "lowering his voice", Line = "...Some of my customers are very important, and very late paying. They'd rather stay unnamed." },
            new() { Speaker = SoulBound, Line = "Nobles who don't pay their tailor. Shocking.", Next = "unopened" },
            new() { Id = "sums", Speaker = "osric", Line = "Sums! Only sums. Dull, dull sums. Nothing anybody would kill for." },
            new()
            {
                Id = "unopened", Speaker = "osric", Line = "Green, with brass corners. If you find it, bring it back unopened. Ideally.",
                Effects = new[] { DialogueEffect.Set(LedgerHeard) },
            },
        },
    };

    /// <summary>Osric after that.</summary>
    public static readonly DialogueScript LedgerAfter = new()
    {
        Id = "sq-ledger-after",
        With = "osric",
        Needs = new[] { LedgerHeard },
        Nodes = new DialogueNode[] { new() { Speaker = "osric", Line = "Green book, brass corners. Unopened. Did I say unopened?" } },
    };

    /// <summary>SQ-Tollroad's hook: Sergeant Brisk at the city gate, the highwaymen on the Mirewood road, and nobody to send.</summary>
    public static readonly DialogueScript Tollroad = new()
    {
        Id = "sq-tollroad",
        With = "brisk",
        Marker = MarkerKind.Quest,
        Unless = new[] { TollroadHeard },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "brisk", Line = "Halt. No, don't halt, I've no time for halting. Going out the gate?" },
            new() { Speaker = SoulBound, Line = "Thinking about it." },
            new() { Speaker = "brisk", Line = "Then know this. The Mirewood road's got highwaymen on it. Tollroad bandits, the carters call them." },
            new()
            {
                Speaker = "brisk", Line = "Stop a wagon, charge a toll, take the wagon. I'd send men, but I've got two recruits and a dog. The dog's the best of us.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Where did your men go?", Next = "men" },
                    new()
                    {
                        Text = "Could we borrow the dog?", Next = "dog",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                    new()
                    {
                        Text = "Make it official. We'll deal with them for the Watch.",
                        Check = new DialogueCheck
                        {
                            Label = "PERSUADE",
                            Advantage = new[] { DialogueCondition.Speaks(Voice.Honorable), DialogueCondition.IsClass(ClassId.Paladin) },
                        },
                        OnSuccess = "friends", OnFailure = "triplicate",
                    },
                    new() { Text = "We'll keep our eyes open.", Next = "board" },
                },
            },
            new() { Id = "men", Speaker = "brisk", Line = "Greywatch. Marched east a month back, palace orders. Nobody says why." },
            new() { Speaker = SoulBound, Line = "Everybody keeps not saying why.", Next = "board" },
            new() { Id = "dog", Speaker = "brisk", Direction = "flatly", Line = "Nobody borrows the dog.", Next = "board" },
            new()
            {
                Id = "friends", Speaker = "brisk", Line = "Can't deputise you. But I'll tell the gate you're friends of the Watch.",
                Effects = new[] { DialogueEffect.Add(Meters.Fame, 1) }, Next = "board",
            },
            new() { Id = "triplicate", Speaker = "brisk", Line = "Nice try. That's forms in triplicate, and the clerk's in Greywatch too." },
            new()
            {
                Id = "board", Speaker = "brisk", Line = "It's on the quest board. The Watch pays, when the Watch has coin. Mind yourselves out there.",
                Effects = new[] { DialogueEffect.Set(TollroadHeard) },
            },
        },
    };

    /// <summary>Brisk after that.</summary>
    public static readonly DialogueScript TollroadAfter = new()
    {
        Id = "sq-tollroad-after",
        With = "brisk",
        Needs = new[] { TollroadHeard },
        Nodes = new DialogueNode[] { new() { Speaker = "brisk", Line = "Still no men. Still got the dog. Mind the road." } },
    };

    /// <summary>Wren, a Low Ring pickpocket in the market, who tries your belt.</summary>
    public static readonly DialogueScript Wren = new()
    {
        Id = "wren-pocket",
        With = "wren",
        Unless = new[] { WrenMet },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator, Line = "A small hand brushes past your belt, quick as a fish.",
                Options = new DialogueOption[]
                {
                    new()
                    {
                        Text = "(Catch the hand.)",
                        Check = new DialogueCheck
                        {
                            Label = "PERCEPTION", Ability = Ability.Wisdom,
                            Advantage = new[] { DialogueCondition.IsClass(ClassId.Rogue) },
                        },
                        OnSuccess = "caught", OnFailure = "gone",
                    },
                    new() { Text = "Hey!", Next = "hey" },
                    new()
                    {
                        Text = "Wrong pocket. The money's in the other one.", Next = "wrong",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "gone", Speaker = Speakers.Narrator, Line = "The hand's gone, and so is its owner, laughing, into the crowd." },
            new() { Speaker = SoulBound, Line = "Did they get anything?" },
            new()
            {
                Speaker = You, Line = "Just my pride.",
                Effects = new[] { DialogueEffect.Set(WrenMet) }, Next = DialogueScript.End,
            },
            new() { Id = "hey", Speaker = "wren", Line = "Wasn't me! I was never here!" },
            new()
            {
                Speaker = Speakers.Narrator, Line = "A small figure in a big coat bolts into the crowd.",
                Effects = new[] { DialogueEffect.Set(WrenMet) }, Next = DialogueScript.End,
            },
            new() { Id = "wrong", Speaker = "wren", Direction = "stopping, interested", Line = "...Is it?", Next = "name" },
            new() { Id = "caught", Speaker = "wren", Line = "Ow! All right! I wasn't taking, I was checking. For lint." },
            new() { Id = "name", Speaker = "wren", Line = "Name's Wren. Don't tell the Watch. They're busy anyway." },
            new() { Speaker = SoulBound, Direction = "a crooked grin", Line = "Low Ring?" },
            new() { Speaker = "wren", Line = "Born and raised. You talk like you're from somewhere poor too." },
            new()
            {
                Speaker = SoulBound, Line = "Poorer. So I know the rule. You don't rob people who'd have helped you.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Keep out of trouble, Wren.", Next = "never" },
                    new() { Text = "Tell us something useful and we're square.", Next = "useful" },
                },
            },
            new()
            {
                Id = "never", Speaker = "wren", Line = "Never. But thanks.",
                Effects = new[] { DialogueEffect.Set(WrenMet) }, Next = DialogueScript.End,
            },
            new()
            {
                Id = "useful", Speaker = "wren", Line = "Fennick's board pays fair. And never drink at the Rusty Anvil unless you like fights.",
                Effects = new[] { DialogueEffect.Set(WrenMet) },
            },
        },
    };

    /// <summary>Wren after that.</summary>
    public static readonly DialogueScript WrenAfter = new()
    {
        Id = "wren-after",
        With = "wren",
        Needs = new[] { WrenMet },
        Nodes = new DialogueNode[] { new() { Speaker = "wren", Line = "Not you two again. Your pockets are boring." } },
    };

    /// <summary>Lark, the busker by the fountain, who gives every new face a verse.</summary>
    public static readonly DialogueScript Lark = new()
    {
        Id = "lark-song",
        With = "lark",
        Unless = new[] { LarkMet },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "lark", Direction = "strumming", Line = "Fresh faces! Every fresh face gets a verse. That's the rule. I made the rule." },
            new()
            {
                Speaker = "lark", Line = "Name and trade, then. What do you do?",
                Options = new DialogueOption[]
                {
                    new() { Text = "{name}. I take contracts.", Next = "scan" },
                    new()
                    {
                        Text = "{name}. A legend. The world just hasn't heard yet.", Next = "legend",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                    new() { Text = "No verses, thanks.", Next = "too-late" },
                },
            },
            new() { Id = "scan", Speaker = "lark", Line = "{name} the contract-taker. Hm. Doesn't scan. I'll fix it.", Next = "deed" },
            new() { Id = "legend", Speaker = "lark", Line = "Ooh, arrogant. Audiences love arrogant.", Next = "deed" },
            new() { Id = "too-late", Speaker = "lark", Line = "Too late. I'm already rhyming." },
            new() { Id = "deed", Speaker = SoulBound, Line = "Do me next. Make it heroic." },
            new() { Speaker = "lark", Line = "Heroic needs a deed. What's your deed?" },
            new()
            {
                Speaker = SoulBound, Direction = "grinning", Line = "We beat the Red Gulls down on the docks.",
                Options = new DialogueOption[]
                {
                    new()
                    {
                        Text = "(Sing the chorus with her.)",
                        Check = new DialogueCheck
                        {
                            Label = "PERFORM",
                            Advantage = new[] { DialogueCondition.IsClass(ClassId.Bard), DialogueCondition.Speaks(Voice.Witty) },
                        },
                        OnSuccess = "sang", OnFailure = "flat",
                    },
                    new() { Text = "Just make it rhyme with Red Gulls.", Next = "rhyme" },
                },
            },
            new() { Id = "sang", Speaker = Speakers.Narrator, Line = "The two of you hit the chorus together. A small crowd claps, and coins land in the hat." },
            new()
            {
                Speaker = "lark", Line = "Half's yours. Don't tell my landlord.",
                Effects = new[] { DialogueEffect.Gold(4), DialogueEffect.Add(Meters.Fame, 1) }, Next = "verse",
            },
            new() { Id = "flat", Speaker = Speakers.Narrator, Line = "You hit a note that doesn't exist. A gull leaves." },
            new() { Speaker = "lark", Line = "Bold. Wrong, but bold.", Next = "verse" },
            new() { Id = "rhyme", Speaker = "lark", Line = "Dead gulls? Fled gulls? Wet gulls. Wet gulls it is." },
            new()
            {
                Id = "verse", Speaker = "lark", Line = "Come back when you've done something bigger. I need a second verse.",
                Effects = new[] { DialogueEffect.Set(LarkMet) },
            },
        },
    };

    /// <summary>Lark once you're off to the Mirewood.</summary>
    public static readonly DialogueScript LarkMirewood = new()
    {
        Id = "lark-mirewood",
        With = "lark",
        Needs = new[] { LarkMet, Act1Chapter1.BoardRead },
        Nodes = new DialogueNode[] { new() { Speaker = "lark", Line = "Off to the Mirewood? Wonderful. Tragic songs pay better. No offence." } },
    };

    /// <summary>Lark before that, after her verse.</summary>
    public static readonly DialogueScript LarkAfter = new()
    {
        Id = "lark-after",
        With = "lark",
        Needs = new[] { LarkMet },
        Unless = new[] { Act1Chapter1.BoardRead },
        Nodes = new DialogueNode[] { new() { Speaker = "lark", Line = "Still waiting on that second verse. Do something dramatic." } },
    };

    /// <summary>Ilsa Fenwright, an apprentice of the Arcanum Spire with an armful of scrolls for the guildhall.</summary>
    public static readonly DialogueScript Ilsa = new()
    {
        Id = "ilsa-spire",
        With = "ilsa",
        Unless = new[] { IlsaMet },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "ilsa", Direction = "juggling scrolls", Line = "Careful! These are for the guildhall. If I drop one, the Archmage will know. She always knows." },
            new() { Speaker = SoulBound, Line = "The Archmage?" },
            new()
            {
                Speaker = "ilsa", Line = "Thessaly Varn. Head of the Mages Guild. Sixty, two fingers short, and never once wrong.",
                Options = new DialogueOption[]
                {
                    new() { Text = "How did she lose the fingers?", Next = "fingers" },
                    new()
                    {
                        Text = "Never wrong? Has anybody checked?", Next = "checked",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                    new()
                    {
                        Text = "You look worried. What's in the scrolls?",
                        Check = new DialogueCheck { Label = "INSIGHT", Ability = Ability.Wisdom },
                        OnSuccess = "east", OnFailure = "dull",
                    },
                    new() { Text = "We'll let you get on.", Next = "glows" },
                },
            },
            new() { Id = "fingers", Speaker = "ilsa", Line = "A containment that went wrong. She tells it like a recipe. Very calmly. Nobody sleeps after.", Next = "glows" },
            new() { Id = "checked", Speaker = "ilsa", Line = "Somebody did, once. He counts candles in the Spire's cellars now.", Next = "glows" },
            new() { Id = "east", Speaker = "ilsa", Direction = "lowering her voice", Line = "Copies of reports from the east. Half the senior mages went to Greywatch. Nobody tells apprentices why." },
            new() { Speaker = SoulBound, Line = "Greywatch again. Everybody's going east but us.", Next = "glows" },
            new() { Id = "dull", Speaker = "ilsa", Line = "Inventory! Very dull. Very, very dull." },
            new()
            {
                Id = "glows", Speaker = "ilsa", Line = "If you're ever up at the Arcanum Spire, don't touch anything that glows. Or hums. Or looks at you.",
                Effects = new[] { DialogueEffect.Set(IlsaMet) },
            },
        },
    };

    /// <summary>Ilsa after that.</summary>
    public static readonly DialogueScript IlsaAfter = new()
    {
        Id = "ilsa-after",
        With = "ilsa",
        Needs = new[] { IlsaMet },
        Nodes = new DialogueNode[] { new() { Speaker = "ilsa", Line = "Still haven't dropped one. The day's young." } },
    };

    /// <summary>Tamsin, a ranger of the Lodge that posted the Mirewood contract, on what's wrong in there.</summary>
    public static readonly DialogueScript Tamsin = new()
    {
        Id = "tamsin-lodge",
        With = "tamsin",
        Unless = new[] { TamsinMet },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "tamsin", Direction = "arms folded", Line = "You two have the look of people who'd walk into a forest on purpose." },
            new()
            {
                Speaker = "tamsin", Line = "Rangers' Lodge. That glowing contract on Fennick's board is ours.",
                Conditions = new[] { new DialogueCondition(ConditionKind.NotFlag, Act1Chapter1.BoardRead) },
            },
            new()
            {
                Speaker = "tamsin", Line = "Rangers' Lodge. You took our contract. Good. Somebody had to.",
                Conditions = new[] { new DialogueCondition(ConditionKind.Flag, Act1Chapter1.BoardRead) },
            },
            new() { Speaker = SoulBound, Line = "What's actually wrong in there?" },
            new()
            {
                Speaker = "tamsin", Line = "Deer that don't run. Wolves that don't eat what they kill. Two woodcutters went in last week. Their axes came back. They didn't.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Why not go in yourselves?", Next = "short" },
                    new()
                    {
                        Text = "Triple pay. You're scared.", Next = "scared",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Pragmatic) }, Locked = true,
                    },
                    new()
                    {
                        Text = "We'll find your woodcutters.", Next = "find",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Honorable) }, Locked = true,
                    },
                    new() { Text = "Any advice?", Next = "advice" },
                },
            },
            new() { Id = "short", Speaker = "tamsin", Line = "We did. We're three rangers short now. Hence the board.", Next = "job" },
            new() { Id = "scared", Speaker = "tamsin", Direction = "a long look", Line = "Scared keeps rangers alive. Be scared in there. It's good advice.", Next = "job" },
            new() { Id = "find", Speaker = "tamsin", Line = "Find them, or find out what happened. Either way, I'll owe you.", Next = "job" },
            new() { Id = "advice", Speaker = "tamsin", Line = "Stay on the paths. And if the birds go quiet, so do you." },
            new()
            {
                Id = "job", Speaker = SoulBound, Line = "Mad animals, quiet forests. I love this job already.",
                Effects = new[] { DialogueEffect.Set(TamsinMet) },
            },
        },
    };

    /// <summary>Tamsin after that.</summary>
    public static readonly DialogueScript TamsinAfter = new()
    {
        Id = "tamsin-after",
        With = "tamsin",
        Needs = new[] { TamsinMet },
        Nodes = new DialogueNode[] { new() { Speaker = "tamsin", Line = "Stay on the paths. If the birds go quiet, so do you." } },
    };

    /// <summary>Guardsman Oswin on the palace terrace: the King up in the Sunspire Keep, and the Princess who keeps riding out.</summary>
    public static readonly DialogueScript Oswin = new()
    {
        Id = "oswin-terrace",
        With = "oswin",
        Unless = new[] { OswinMet },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "oswin", Direction = "at attention", Line = "Palace terrace. Look all you like. Touch nothing, climb nothing, pick no flowers." },
            new() { Speaker = SoulBound, Line = "Is the King up there?" },
            new()
            {
                Speaker = "oswin", Line = "His Majesty's up in the Sunspire Keep. He walks the terrace at dusk, some evenings. Waves at the children.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Does he really say thank you?", Next = "thanks" },
                    new() { Text = "And the Princess?", Next = "elara" },
                    new()
                    {
                        Text = "Any chance of a look inside?",
                        Check = new DialogueCheck { Label = "PERSUADE" },
                        OnSuccess = "inside", OnFailure = "no",
                    },
                    new()
                    {
                        Text = "Long shift?", Next = "shift",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Honorable) }, Locked = true,
                    },
                },
            },
            new() { Id = "thanks", Speaker = "oswin", Line = "To the guards? Every night, at the change. Unsettling, the first year." },
            new() { Speaker = SoulBound, Direction = "quietly", Line = "Huh. So it's true." },
            new() { Speaker = SoulBound, Line = "Must be nice. Being the one who gets to say it.", Next = "move" },
            new() { Id = "elara", Speaker = "oswin", Direction = "sighing", Line = "Princess Elara is wherever her guards aren't. She rode out the south gate twice this week. Alone.", Next = "move" },
            new() { Id = "inside", Speaker = "oswin", Line = "No. But stand by the blue banner at dusk and you'll see the throne room light up through the glass.", Next = "move" },
            new() { Id = "no", Speaker = "oswin", Line = "No. Absolutely not. Nice try, though.", Next = "move" },
            new() { Id = "shift", Speaker = "oswin", Line = "Double. Half the Royal Guard rode east with the supply wagons. Thank you for asking." },
            new()
            {
                Id = "move", Speaker = "oswin", Line = "Move along now. Kindly.",
                Effects = new[] { DialogueEffect.Set(OswinMet) },
            },
        },
    };

    /// <summary>Oswin after that.</summary>
    public static readonly DialogueScript OswinAfter = new()
    {
        Id = "oswin-after",
        With = "oswin",
        Needs = new[] { OswinMet },
        Nodes = new DialogueNode[] { new() { Speaker = "oswin", Line = "Touch nothing, climb nothing, pick no flowers. Good day." } },
    };

    /// <summary>Sister Hesper on the temple steps: the quiet bells, the bellringer who died, and the rangers who came in.</summary>
    public static readonly DialogueScript Hesper = new()
    {
        Id = "hesper-temple",
        With = "hesper",
        Unless = new[] { HesperMet },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "hesper", Direction = "sweeping the steps", Line = "Welcome, travellers. The temple's open to anyone with a heavy heart or muddy boots." },
            new() { Speaker = SoulBound, Line = "Which gets you in quicker?" },
            new() { Speaker = "hesper", Line = "Boots. Hearts take longer." },
            new() { Speaker = "hesper", Line = "Forgive the bells. Our bellringer, Edric, passed in the spring. The new boy rings them like he's angry at them." },
            new()
            {
                Speaker = SoulBound, Line = "Edric. His widow's down on the docks. She lost her ring in the harbour.",
                Conditions = new[] { new DialogueCondition(ConditionKind.Flag, WidowHeard) },
            },
            new()
            {
                Speaker = "hesper", Line = "Maud. Poor Maud. If you find it, bring it by. I'd like to see her face.",
                Conditions = new[] { new DialogueCondition(ConditionKind.Flag, WidowHeard) },
            },
            new()
            {
                Speaker = "hesper", Line = "Is there something I can do for you?",
                Options = new DialogueOption[]
                {
                    new() { Text = "Could we have a blessing?", Next = "blessing" },
                    new()
                    {
                        Text = "You seem tired, Sister.",
                        Check = new DialogueCheck { Label = "INSIGHT", Ability = Ability.Wisdom },
                        OnSuccess = "rangers", OnFailure = "steps",
                    },
                    new()
                    {
                        Text = "Does the angry boy take requests?", Next = "requests",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "blessing", Speaker = "hesper", Line = "For the road, then. May you come home on the feet you left on.", Next = "go-well" },
            new() { Id = "rangers", Speaker = "hesper", Line = "The rangers came in for blessings this week. All of them, at once. Rangers never come in.", Next = "go-well" },
            new() { Id = "steps", Speaker = "hesper", Line = "Only the steps, dear. They never stay swept.", Next = "go-well" },
            new() { Id = "requests", Speaker = "hesper", Direction = "smiling", Line = "He does not. I asked for gentle. He gave me thunder." },
            new()
            {
                Id = "go-well", Speaker = "hesper", Line = "Go well. The door's always open.",
                Effects = new[] { DialogueEffect.Set(HesperMet) },
            },
        },
    };

    /// <summary>Hesper after that.</summary>
    public static readonly DialogueScript HesperAfter = new()
    {
        Id = "hesper-after",
        With = "hesper",
        Needs = new[] { HesperMet },
        Nodes = new DialogueNode[] { new() { Speaker = "hesper", Line = "Muddy boots or a heavy heart. Either way, the door's open." } },
    };
}
