using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// Act 1, Chapter 3, "Ashes at Thornbridge": the refugees shut out at the east gate and Sergeant
/// Brisk's orders, Mira Ashgrove at the healer's station, her remedies for the sick, the Hollowed
/// raid, and Mira joining.
/// </summary>
/// <remarks>
/// STORY.md's A1-C3-01 and A1-C3-02, and what it leaves to fill: the three sick refugees Mira's
/// remedies go to, and what Brisk, Hedda and her daughter say when you talk to them again. The raid
/// is a card until combat exists. Mira tells her story about Maleficio either straight away or once
/// the remedies are handed out, so both of her scripts end on the same lines (Maleficio); the last of
/// them sounds the horns and sets mira-supplies-done, which brings the Hollowed out of the treeline,
/// and walking up to them starts the raid.
/// </remarks>
public static class Act1Chapter3
{
    private const string SoulBound = Origins.SoulBound;

    /// <summary>Mira's approval.</summary>
    private const string MiraAffinity = "affinity-mira";

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

    /// <summary>A1-C3-01: the camp outside the east gate, Brisk's orders, and the Soul-Bound's first quiet anger at a crown.</summary>
    public static readonly DialogueScript Gate = new()
    {
        Id = "a1-c3-01",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Chapter, Line = "Chapter 3  ·  Ashes at Thornbridge" },
            new()
            {
                Speaker = Speakers.Narrator,
                Line = "Outside the east gate, a camp has swollen overnight across the meadow by the bridge: sacking tents, cookfires, hundreds of people who've walked a very long way on very little.",
            },
            new() { Speaker = Speakers.Narrator, Line = "Behind you, the gate swings shut again, and the Watch's spears come down across it." },
            new() { Speaker = "brisk", Direction = "to the crowd, not for the first time", Line = "Orders from the Chancellor. No Doomward folk inside the walls. Could be sickness. Could be spies." },
            new() { Speaker = "refugee-mother", Direction = "holding a child", Line = "We walked for three weeks. Please. She hasn't eaten in two days." },
            new() { Speaker = SoulBound, Direction = "stepping up to the sergeant", Line = "Look at them. Do they look like spies to you?" },
            new() { Speaker = "brisk", Line = "Orders are orders, friend." },
            new()
            {
                Speaker = SoulBound, Direction = "very quietly", Line = "Orders. From a man in a warm room who's never missed a meal.",
                Options = new DialogueOption[]
                {
                    new() { Text = "We'll bring food out to the camp ourselves.", Next = "food", Effects = new[] { DialogueEffect.Add(Meters.Fame, 1) } },
                    new()
                    {
                        Text = "Let them in, Sergeant. We'll take responsibility.",
                        Check = new DialogueCheck { Label = "PERSUADE" },
                        OnSuccess = "let-in", OnFailure = "shove",
                    },
                    new()
                    {
                        Text = "The sergeant's right. We don't know who's out there.", Next = "right",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Pragmatic) }, Locked = true,
                        Effects = new[] { DialogueEffect.Add(Meters.Bond, -5), DialogueEffect.Add(Meters.Ambition, 2) },
                    },
                },
            },
            new() { Id = "food", Speaker = SoulBound, Direction = "after a long moment", Line = "...Fine. But someone should answer for this.", Next = "bread" },

            new()
            {
                Id = "let-in", Speaker = "brisk", Direction = "a long breath out", Line = "...The sick. Only the sick, and only as far as the temple. And it's on your heads, not mine.",
                Effects = new[] { DialogueEffect.Add(Meters.Fame, 2), DialogueEffect.Add(Meters.Bond, 3), DialogueEffect.Set(BriskPersuaded) },
            },
            new() { Speaker = Speakers.Narrator, Line = "The spears lift. The worst of the sick are carried through on doors and blankets, and the gate shuts behind them.", Next = "bread" },

            new() { Id = "shove", Speaker = "brisk", Line = "I said no. Step back, friend." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "{sb} shoves him, hard. Brisk staggers into the gatepost, and a young guard has his sword half out before anyone's breathed.",
                Effects = new[] { DialogueEffect.Add(Meters.Ambition, 3), DialogueEffect.Set(BriskShoved) },
            },
            new() { Speaker = "watch", Direction = "his voice cracking", Line = "Hands off the sergeant!" },
            new() { Speaker = Speakers.Narrator, Line = "You step between them with your hands open, and keep talking, low and steady, until the sword goes back where it came from." },
            new() { Speaker = "brisk", Direction = "straightening his helm", Line = "...We'll call that a stumble. Once.", Next = "bread" },

            new() { Id = "right", Speaker = SoulBound, Direction = "staring at you", Line = "Who are you right now?" },

            new() { Id = "bread", Speaker = Speakers.Narrator, Line = "{sb} crouches in front of the woman and her daughter, and gives the girl the bread out of {sb_their} pack." },
            new() { Speaker = "refugee-mother", Line = "Bless you. Bless you." },
            new()
            {
                Speaker = "refugee-mother", Line = "There's a healer down by the river. One of ours. She hasn't slept since we got here. If you want to help, help her.",
                Effects = new[] { DialogueEffect.Set(GateDone), DialogueEffect.Objective("Find the healer down by the river") },
            },
        },
    };

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

    /// <summary>A1-C3-02: meeting Mira at the healer's station, and either her story or her remedies.</summary>
    public static readonly DialogueScript Mira = new()
    {
        Id = "a1-c3-02",
        With = "mira",
        Needs = new[] { GateDone },
        Unless = new[] { Supplies, SuppliesDone, MiraRecruited },
        Marker = MarkerKind.Event,
        Nodes = Join(
            new DialogueNode[]
            {
                new() { Speaker = Speakers.Narrator, Line = "Down by the river, a young woman in faded scholar's robes kneels by a cot. There's a faint gold glow on her hands." },
                new() { Speaker = "mira", Direction = "to the man on the cot", Line = "Hold still. This will feel warm. Then it will feel better." },
                new() { Speaker = Speakers.Narrator, Line = "The man lets out a long breath, and the pain goes out of his face." },
                new() { Speaker = "mira", Direction = "noticing you", Line = "You're not Watch. And you're not running, which makes you either brave or new." },
                new() { Speaker = SoulBound, Line = "Little of both." },
                new()
                {
                    Speaker = "mira", Line = "Mira Ashgrove. I was an archivist in Doomward, at the Ember Archive. Before.",
                    Options = new DialogueOption[]
                    {
                        new() { Text = "Before what?", Next = "before" },
                        new()
                        {
                            Text = "You look exhausted. Let us help first, talk later.", Next = "help",
                            Effects = new[] { DialogueEffect.Add(MiraAffinity, 5) },
                        },
                        new()
                        {
                            Text = "An archivist. So you're brilliant and kind. Unfair to the rest of us.", Next = "flirt", Flirt = true,
                            Effects = new[] { DialogueEffect.Add(MiraAffinity, 3), DialogueEffect.Set(MiraFlirt) },
                        },
                    },
                },
                new() { Id = "help", Speaker = "mira", Direction = "looking at you properly for the first time", Line = "...Yes. Thank you. I haven't sat down since dawn." },
                new()
                {
                    Speaker = "mira",
                    Line = "There are three by the river I haven't reached: an old farmer with a cough, a boy with a fever, and a glassblower with burned hands. This for the cough, willowbark for the fever, salve for the burns. Tell them Mira sent you.",
                    Effects = new[] { DialogueEffect.Set(Supplies), DialogueEffect.Objective("Take Mira's remedies to the three sick refugees by the river") },
                    Next = DialogueScript.End,
                },
                new() { Id = "flirt", Speaker = "mira", Direction = "a tired laugh", Line = "Flattery won't feed anyone. But... thank you." },
            },
            Maleficio()),
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

    /// <summary>A1-C3-02's raid: the Hollowed at the treeline, the Watch too late, the Soul-Bound and Brisk, and Mira joining.</summary>
    public static readonly DialogueScript Raid = new()
    {
        Id = "a1-c3-02-raid",
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator, Sound = Sfx.WarHorn,
                Line = "The horns again, close now. Footmen come across the ford with axes, and behind them a Hexer in rags, chanting, black light gathering in her hands.",
            },
            new() { Speaker = "mira", Direction = "stepping up beside you", Line = "I'm not hiding behind the carts. Not again." },

            // The defence of Thornbridge, as a card until combat exists, with the Soul-Bound's bark in the thick of it.
            new() { Speaker = SoulBound, Direction = "mid-fight", Line = "Where's the Watch?! Where's the king's army?!" },
            new()
            {
                Speaker = Speakers.Card,
                Line = "The Hollowed are beaten. The last footman goes down in the shallows with Mira's gold light still burning on his chest, and the river runs grey with ash for a long way downstream.",
                Effects = new[] { DialogueEffect.Set(RaidBeaten) },
            },
            new() { Speaker = Speakers.Narrator, Line = "Only then does the gate open. Sergeant Brisk and his Watch come out at a jog with their spears up, to a camp that doesn't need them any more." },
            new() { Speaker = SoulBound, Direction = "to Brisk, shaking with rage", Line = "You stood behind your wall and watched." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "It's the second time today {sb}'s squared up to him. Nobody draws a sword this time.",
                Conditions = new[] { DialogueCondition.Is(BriskShoved) },
            },
            new() { Speaker = "brisk", Line = "I... we had orders..." },
            new()
            {
                Speaker = SoulBound, Line = "Say 'orders' one more time.",
                Options = new DialogueOption[]
                {
                    new() { Text = "(Put a hand on {sb}'s shoulder.)", Next = "hand", Effects = new[] { DialogueEffect.Add(Meters.Bond, 5) } },
                    new()
                    {
                        Text = "Let them hear it. They earned it.", Next = "crowd",
                        Effects = new[] { DialogueEffect.Add(Meters.Ambition, 5), DialogueEffect.Set(CrowdLoved), DialogueEffect.Add(Meters.Fame, 2) },
                    },
                },
            },
            new() { Id = "hand", Speaker = Speakers.Narrator, Line = "Under your hand, {sb} is shaking. A breath. Another. Then {sb} steps back." },
            new() { Speaker = SoulBound, Line = "I'm okay. I'm okay.", Next = "watch-it" },

            new() { Id = "crowd", Speaker = Speakers.Narrator, Line = "{sb} climbs up onto a cart, where the whole camp can see." },
            new() { Speaker = SoulBound, Line = "No one is coming to save you. So we will." },
            new() { Speaker = Speakers.Narrator, Line = "For a moment, nothing. Then somebody shouts {sb}'s name, and then everybody is." },

            new()
            {
                Id = "watch-it", Speaker = "mira", Direction = "to you, privately",
                Line = "Your friend has a fire in them. I've seen that fire before. Watch it for me?",
                Options = new DialogueOption[]
                {
                    new() { Text = "I always have.", Next = "always" },
                    new() { Text = "{sb} is nothing like him.", Next = "nothing-like" },
                    new() { Text = "I will.", Next = "will" },
                },
            },
            new() { Id = "always", Speaker = "mira", Line = "Then keep doing it. Especially when it's easy not to.", Next = "king" },
            new() { Id = "nothing-like", Speaker = "mira", Direction = "gently", Line = "No. Neither was he, once.", Next = "king" },
            new() { Id = "will", Speaker = "mira", Line = "Thank you." },

            new() { Id = "king", Speaker = "mira", Line = "I need to see your king. What I know about the Box, somebody in that palace has to hear. Before it's too late. Will you take me?" },
            new() { Speaker = SoulBound, Direction = "coming back over", Line = "We'll get you in front of him. Somehow." },
            new() { Speaker = SoulBound, Line = "...How does anyone get in front of a king?" },
            new() { Speaker = "mira", Line = "After today? You won't have to look. Go somewhere you can be found." },
            new()
            {
                Speaker = SoulBound, Line = "The Gilded Flagon, then. Everybody ends up at the Flagon.",
                Effects = new[]
                {
                    DialogueEffect.Recruit("mira"), DialogueEffect.Set(MiraRecruited), DialogueEffect.Set(ChapterDone),
                    DialogueEffect.Objective("Go back into the city; the Gilded Flagon's a good place to be found"),
                },
            },
        },
    };

    /// <summary>The heart of A1-C3-02, shared by both ways you hear it: Mira on Maleficio, the Soul-Bound's slip, and the horns.</summary>
    /// <remarks>Starts on the line called before, which the meeting's "Before what?" goes to. Its last line brings the Hollowed out.</remarks>
    private static DialogueNode[] Maleficio() => new DialogueNode[]
    {
        new() { Id = "before", Speaker = "mira", Line = "Before Maleficio opened the Box. He was my teacher. He wanted to end the famines in the east. He was kind." },
        new() { Speaker = "mira", Line = "I watched him become something else. One little hunger at a time." },
        new() { Speaker = "mira", Line = "First he said he only needed enough power to fix the harvest. Then enough to fix the council. Then enough to fix the world." },
        new() { Speaker = "mira", Direction = "quietly", Line = "He never once thought he was the villain." },
        new() { Speaker = Speakers.Narrator, Line = "{sb} has gone very still, listening to every word." },
        new() { Speaker = SoulBound, Line = "Maybe he wasn't wrong about the council." },
        new() { Speaker = "mira", Direction = "sharply", Line = "He burned the council. With their families in the hall." },
        new() { Speaker = SoulBound, Direction = "backing off, hands up", Line = "I meant... I didn't mean that. Sorry." },
        new() { Speaker = Speakers.Narrator, Sound = Sfx.WarHorn, Line = "Horns. Low and long, from the trees across the river." },
        new() { Speaker = "mira", Direction = "going white", Line = "No. Not here. Not this far west." },
        new() { Speaker = Speakers.Narrator, Line = "Grey figures walk out of the treeline in a line. Not running. Ash-grey skin, and nothing behind the eyes." },
        new() { Speaker = "mira", Line = "Hollowed. They were people once. They don't remember it." },
        new()
        {
            Speaker = SoulBound, Direction = "drawing", Line = "Everyone behind the carts! {name}, the treeline. Now!",
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
