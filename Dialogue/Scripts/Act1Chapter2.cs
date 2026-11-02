using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// Act 1, Chapter 2, "The Rot in the Mirewood": the gloomfangs at the forest's edge and Sera, the
/// old quarry and the dead courier, the Thornmother and the black crystal, the night camp, and
/// Fennick paying out back in the city.
/// </summary>
/// <remarks>
/// STORY.md's A1-C2-01 to A1-C2-03, and what it leaves for the game to fill: Sera talking the
/// quarry's root walls open, the courier's letter as a scene of its own, Fennick's pay with the hook
/// to the east gate (A1-C2-04), and the Lodge's ranger and the busker once you're back. Each fight
/// splits its scene in two: the conversation before it ends by starting it, and winning it plays the
/// rest (EncounterCatalog's Then). The gloomfangs, then Sera; the Thornmother, then the crystal; and
/// if you hold the den while Sera purifies it, the guard wave, then camp. The flags change the maps
/// as they go: the wolves leave the clearing once they're beaten, the roots stand open once Sera's
/// parted them, the Thornmother's gone once she's down, and the crystal ends shattered or shining
/// white.
/// </remarks>
public static class Act1Chapter2
{
    private const string You = Origins.You;
    private const string SoulBound = Origins.SoulBound;

    /// <summary>Sera's approval.</summary>
    private const string SeraAffinity = "affinity-sera";

    /// <summary>What the Rangers' contract pays: triple, the way Fennick said.</summary>
    private const int ContractPay = 90;

    /// <summary>Tamsin's heard how the Mirewood went.</summary>
    private const string TamsinThanked = "tamsin-thanked";

    /// <summary>Lark's sung the Mirewood's verse.</summary>
    private const string LarkSecondVerse = "lark-second-verse";

    /// <summary>The gloomfangs at the forest's edge are beaten: the wolves leave the clearing.</summary>
    public const string GloomfangsBeaten = "gloomfangs-beaten";

    /// <summary>The flag A1-C2-01 sets when you flirt with Sera. Her romance reads it.</summary>
    public const string SeraFlirt = "sera-flirt-1";

    /// <summary>Sera's joined: she leaves the clearing's tree line.</summary>
    public const string SeraRecruited = "sera-recruited";

    /// <summary>The forest's edge is over: Sera's with you and the quarry's next.</summary>
    public const string EdgeDone = "a1-c2-01";

    /// <summary>Sera's parted the quarry's first root wall: its door stands open.</summary>
    public const string FirstRootsParted = "quarry-roots-1";

    /// <summary>Sera's parted the quarry's second root wall: its door stands open.</summary>
    public const string SecondRootsParted = "quarry-roots-2";

    /// <summary>The dead courier's letter is read: the den at the bottom of the quarry is next.</summary>
    public const string LetterRead = "courier-letter-read";

    /// <summary>The Thornmother's beaten: she leaves her den.</summary>
    public const string ThornmotherBeaten = "thornmother-beaten";

    /// <summary>Sera's drawing the rot out of the crystal: it shines white.</summary>
    public const string CrystalPurified = "crystal-purified";

    /// <summary>The crystal's shattered: its stump stays.</summary>
    public const string CrystalShattered = "crystal-shattered";

    /// <summary>You and the Soul-Bound held the den while Sera purified the crystal.</summary>
    public const string CrystalGuarded = "crystal-guarded";

    /// <summary>The crystal's been dealt with one way or another: the black one's gone.</summary>
    public const string CrystalDealtWith = "crystal-dealt-with";

    /// <summary>The flag A1-C2-02 sets when you side with Sera: the forest heals fully, and its merchants remember it.</summary>
    public const string SeraPurified = "sera-purified";

    /// <summary>SQ-Stream's hook: the crystal was shattered, and the stream below the quarry runs black.</summary>
    public const string StreamQuest = "sq-stream";

    /// <summary>The quarry's done: the party's taken to the camp in the Mirewood.</summary>
    public const string QuarryDone = "a1-c2-02";

    /// <summary>The flag the night camp sets: Sera watched the Soul-Bound across the fire and said nothing. Act 3's tracker reads it.</summary>
    public const string SeraNoticedSb = "sera-noticed-sb";

    /// <summary>The night camp's over: Fennick owes you.</summary>
    public const string CampDone = "a1-c2-03";

    /// <summary>Fennick's paid for the Mirewood and pointed you at the east gate.</summary>
    public const string Paid = "a1-c2-04";

    /// <summary>A1-C2-01 up to the fight: into the Mirewood, and the gloomfangs in the clearing.</summary>
    public static readonly DialogueScript Edge = new()
    {
        Id = "a1-c2-01",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Chapter, Line = "Chapter 2  ·  The Rot in the Mirewood" },
            new() { Speaker = Speakers.Narrator, Line = "The Mirewood is green at its edge, but the birds have already stopped singing. Further in, black runs through the bark of the trees like veins." },
            new() { Speaker = SoulBound, Direction = "sniffing the air", Line = "Smell that? Like a burnt-out forge." },
            new() { Speaker = You, Line = "Smells like triple pay.", Conditions = new[] { DialogueCondition.Speaks(Voice.Pragmatic) } },
            new() { Speaker = You, Line = "Smells like people in trouble.", Conditions = new[] { DialogueCondition.Speaks(Voice.Honorable) } },
            new() { Speaker = You, Line = "Smells like your cooking.", Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) } },
            new()
            {
                Speaker = SoulBound, Direction = "laughing", Line = "Everything smells like money to you. It's why I keep you around.",
                Conditions = new[] { DialogueCondition.Speaks(Voice.Pragmatic) },
            },
            new() { Speaker = SoulBound, Direction = "the grin going", Line = "Yeah. It does.", Conditions = new[] { DialogueCondition.Speaks(Voice.Honorable) } },
            new() { Speaker = SoulBound, Line = "Rude. Accurate, but rude.", Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) } },

            new() { Speaker = Speakers.Narrator, Sound = Sfx.WolfHowl, Line = "A howl, close. Four wolves come out of the trees with their heads low, and black smoke leaks from their eyes like tears." },
            new()
            {
                Speaker = SoulBound, Direction = "drawing", Line = "Nice doggies. Nice, horrible doggies.",
                Effects = new[] { DialogueEffect.Fight("a1-c2-01-gloomfangs") },
            },
        },
    };

    /// <summary>The rest of A1-C2-01, once the gloomfangs are beaten: the one that gets up again, Sera's arrow, and Sera joining.</summary>
    /// <remarks>EncounterCatalog's a1-c2-01-gloomfangs plays it when the fight's won, which has set GloomfangsBeaten.</remarks>
    public static readonly DialogueScript SeraJoins = new()
    {
        Id = "a1-c2-01-gloomfangs-after",
        Nodes = new DialogueNode[]
        {
            // The rot won't let the last of them lie, which is how Sera gets the last word.
            new()
            {
                Speaker = Speakers.Narrator,
                Line = "The last gloomfang won't stay down. It drags itself up out of the bracken with the smoke pouring from its eyes, and goes for {sb}'s throat.",
            },
            new() { Speaker = Speakers.Narrator, Sound = Sfx.Bowshot, Line = "An arrow out of the dark takes it in mid-leap. It's dead before it lands at {sb}'s feet." },
            new() { Speaker = Speakers.Narrator, Line = "A woman steps out of the trees with her bow still drawn: a Sylvan in green and brown leathers, a braid over one shoulder." },
            new() { Speaker = "sera", Line = "You're loud. Both of you. The whole forest heard you coming." },
            new() { Speaker = SoulBound, Direction = "grinning", Line = "And yet you waited until we'd done the hard part." },
            new()
            {
                Speaker = "sera", Line = "I waited to see if you'd make it worse. You didn't. Barely.",
                Options = new DialogueOption[]
                {
                    new() { Text = "We're here from the Rangers' Lodge. We want to help.", Next = "lodge", Effects = new[] { DialogueEffect.Add(SeraAffinity, 5) } },
                    new()
                    {
                        Text = "Nice shot. You do this often, or just for us?", Next = "flirt", Flirt = true,
                        Effects = new[] { DialogueEffect.Add(SeraAffinity, 3), DialogueEffect.Set(SeraFlirt) },
                    },
                    new() { Text = "We had it handled.", Next = "handled" },
                },
            },
            new() { Id = "lodge", Speaker = "sera", Line = "Tamsin's lot. About time they sent someone who'd actually come in.", Next = "warden" },
            new() { Id = "flirt", Speaker = "sera", Line = "Only for people worth the arrow. Jury's still out.", Next = "warden" },
            new() { Id = "handled", Speaker = "sera", Direction = "a look at the dead wolf", Line = "Clearly." },

            new() { Id = "warden", Speaker = "sera", Direction = "lowering the bow at last", Line = "Sera Vantis, warden of this stretch of wood. The wolves aren't mad. They're poisoned." },
            new() { Speaker = "sera", Line = "Something in the old quarry is bleeding black into the roots. Every creature that drinks from the stream turns." },
            new() { Speaker = Speakers.Narrator, Line = "She crouches by the last wolf and closes its eyes with two fingers. The smoke stops." },
            new() { Speaker = SoulBound, Line = "So we find it and smash it." },
            new() { Speaker = "sera", Line = "So we find it and understand it. Then we decide." },
            new() { Speaker = SoulBound, Direction = "to you, half joking", Line = "I like her. She's wrong, but I like her." },
            new() { Speaker = "sera", Direction = "without looking round", Line = "I can hear you." },
            new()
            {
                Speaker = "sera", Line = "The quarry's east, where the rot's worst. Keep up, and keep quiet.",
                Effects = new[]
                {
                    DialogueEffect.Recruit("sera"), DialogueEffect.Set(SeraRecruited), DialogueEffect.Set(EdgeDone),
                    DialogueEffect.Objective("Find the old quarry deep in the Mirewood"),
                },
            },
        },
    };

    /// <summary>The quarry's first root wall, and Sera talking it open.</summary>
    public static readonly DialogueScript FirstRoots = new()
    {
        Id = "a1-c2-02-roots-1",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Narrator, Line = "A wall of roots chokes the way down, thick as a man's waist and black at the heart. They creak as you come closer." },
            new() { Speaker = SoulBound, Line = "Stand back. I'll cut us a door." },
            new() { Speaker = "sera", Line = "Cut them and they'll bleed on you. Stand back yourself." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "Sera lays her palm on the roots and talks to them, very low, the way you'd talk to a frightened horse. They shiver. Then, slowly, they draw apart.",
                Effects = new[] { DialogueEffect.Set(FirstRootsParted) },
            },
            new() { Speaker = SoulBound, Line = "...Can you teach me that?" },
            new() { Speaker = "sera", Line = "No." },
        },
    };

    /// <summary>The quarry's second root wall, rotted right through, and what parting it costs Sera.</summary>
    public static readonly DialogueScript SecondRoots = new()
    {
        Id = "a1-c2-02-roots-2",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Narrator, Line = "More roots, and worse: the black has gone right through these, and they're moving when nothing's touching them." },
            new()
            {
                Speaker = "sera", Direction = "kneeling", Line = "These ones are hurting. Give me a moment.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Take all the time you need.", Next = "time" },
                    new() { Text = "Anything we can do?", Next = "do" },
                },
            },
            new() { Id = "time", Speaker = "sera", Direction = "a glance up, surprised", Line = "...Thank you.", Next = "parted" },
            new() { Id = "do", Speaker = "sera", Line = "Stand where they can see you. And don't be frightening." },
            new() { Speaker = SoulBound, Line = "I'm never frightening." },
            new() { Speaker = "sera", Direction = "not looking up", Line = "Mm." },
            new()
            {
                Id = "parted", Speaker = Speakers.Narrator, Line = "It takes longer this time. When the roots finally part, Sera's hand comes away shaking, with black under the nails.",
                Effects = new[] { DialogueEffect.Set(SecondRootsParted) },
            },
            new() { Speaker = SoulBound, Line = "You all right?" },
            new() { Speaker = "sera", Line = "No. Keep moving." },
        },
    };

    /// <summary>A1-C2-02, at the bottom of the quarry: the dead Doomward courier, and the letter in his satchel.</summary>
    public static readonly DialogueScript Letter = new()
    {
        Id = "a1-c2-02-letter",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Narrator, Sound = Sfx.CrystalPulse, Line = "Somewhere ahead, something pulses, slow as a heartbeat. You feel it in your teeth." },
            new() { Speaker = Speakers.Narrator, Line = "A man in charred robes sits against the rock as if he stopped for a rest and never got up. There's a satchel at his side." },
            new() { Speaker = "sera", Line = "Doomward robes. This far west?" },
            new() { Speaker = Speakers.Narrator, Line = "In the satchel: hard bread, a knife, and a letter under a black seal pressed with a cube." },
            new()
            {
                Speaker = You, Direction = "reading", Line = "'...the Master's shards seed the land ahead of the army. Let the rot soften Fable before we arrive...'",
                Effects = new[] { DialogueEffect.Give("courier-letter"), DialogueEffect.Set(LetterRead) },
            },
            new()
            {
                Speaker = SoulBound, Direction = "low", Line = "An army. Coming here.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Then somebody has to tell the King.", Next = "king" },
                    new() { Text = "The crystal first. Armies later.", Next = "first" },
                    new()
                    {
                        Text = "Well. That's ruined my afternoon.", Next = "afternoon",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "king", Speaker = SoulBound, Line = "Would a king listen to two sellswords off a boat?" },
            new() { Speaker = SoulBound, Direction = "folding the letter away", Line = "...Then we'd better be worth listening to.", Next = "awake" },
            new() { Id = "first", Speaker = "sera", Line = "Finally. Someone sensible.", Next = "awake" },
            new() { Id = "afternoon", Speaker = SoulBound, Line = "Your afternoon? I was going to retire on this contract." },
            new()
            {
                Id = "awake", Speaker = Speakers.Narrator, Sound = Sfx.CrystalPulse,
                Line = "The pulse comes again from the den at the quarry's end, deeper now. Something very big shifts its weight in the dark.",
            },
            new()
            {
                Speaker = "sera", Direction = "nocking an arrow", Line = "Whatever's guarding it is awake.",
                Effects = new[] { DialogueEffect.Objective("Go into the den at the bottom of the quarry") },
            },
        },
    };

    /// <summary>A1-C2-02 up to the fight: the den, the crystal, and what's become of the bear that guards it.</summary>
    public static readonly DialogueScript Thornmother = new()
    {
        Id = "a1-c2-02",
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator, Sound = Sfx.CrystalPulse,
                Line = "In the den, a spike of black crystal juts out of the rock, taller than a man, pulsing like a heart. Curled round its foot is a bear. Or what's left of one.",
            },
            new() { Speaker = Speakers.Narrator, Line = "She stands. She's twice your height, with thorns grown out through her hide and crystal light where her eyes should be." },
            new() { Speaker = "sera", Direction = "very quietly", Line = "...Oh, no. Not you." },
            new() { Speaker = "sera", Line = "She's denned in this quarry ten springs running. I've watched her raise every cub." },
            new() { Speaker = SoulBound, Direction = "gently", Line = "Sera..." },
            new()
            {
                Speaker = "sera", Direction = "drawing", Line = "I know. Make it quick. She'd want it quick.",
                Effects = new[] { DialogueEffect.Fight("a1-c2-02-thornmother") },
            },
        },
    };

    /// <summary>The rest of A1-C2-02, once the Thornmother's beaten: the woodcutters, the party's first argument, and out to camp, or the den to hold while Sera purifies the crystal.</summary>
    /// <remarks>EncounterCatalog's a1-c2-02-thornmother plays it when the fight's won, which has set ThornmotherBeaten. Holding the den is a fight of its own, and Guarded is the rest of that way.</remarks>
    public static readonly DialogueScript Crystal = new()
    {
        Id = "a1-c2-02-thornmother-after",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Narrator, Line = "The Thornmother goes down like a falling tree, and the thorns in her hide crumble to black dust." },
            new() { Speaker = Speakers.Narrator, Line = "Sera kneels and rests a hand on the bear's muzzle until the crystal light in its eyes goes out." },
            new()
            {
                Speaker = Speakers.Narrator, Sound = Sfx.CrystalPulse,
                Line = "Behind them, the crystal pulses on. Among the bones at its foot lie two pairs of boots, and a woodcutter's axe-belt with no axe in it.",
            },
            new() { Speaker = SoulBound, Direction = "quietly", Line = "The missing woodcutters." },
            new() { Speaker = "sera", Direction = "getting up", Line = "If we shatter it, the corruption spills into the stream. We need to draw it out slowly. Purify it. That takes a day, maybe two." },
            new() { Speaker = SoulBound, Line = "And in two days how many more animals turn? How many more woodcutters? We smash it now." },
            new() { Speaker = "sera", Line = "Smash it and the rot goes into the water. Every farm downriver drinks it." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "They both turn to you.",
                Options = new DialogueOption[]
                {
                    new()
                    {
                        Text = "Sera's right. We purify it.", Next = "purify",
                        Effects = new[] { DialogueEffect.Add(SeraAffinity, 10), DialogueEffect.Set(CrystalPurified), DialogueEffect.Set(SeraPurified) },
                    },
                    new()
                    {
                        Text = "{sb}'s right. Smash it.", Next = "shatter",
                        Effects = new[]
                        {
                            DialogueEffect.Add(SeraAffinity, -5), DialogueEffect.Add(Meters.Bond, 5),
                            DialogueEffect.Set(CrystalShattered), DialogueEffect.Set(StreamQuest),
                        },
                    },
                    new()
                    {
                        Text = "Sera purifies it. {sb} and I hold the quarry.", Next = "guard",
                        Effects = new[] { DialogueEffect.Add(SeraAffinity, 5), DialogueEffect.Add(Meters.Bond, 2) },
                    },
                },
            },

            new() { Id = "purify", Speaker = SoulBound, Line = "Fine. Your forest, your call." },
            new()
            {
                Speaker = Speakers.Narrator, Line = "{sb} turns and walks back up the quarry alone, and doesn't look round.",
                Effects = new[] { DialogueEffect.Add(Meters.Ambition, 3) },
            },
            new() { Speaker = Speakers.Narrator, Line = "Sera lays both hands on the crystal's roots. Slowly, very slowly, the black starts to drain out of it, and the stone underneath turns white." },
            new() { Speaker = "sera", Line = "Two days. Less, now there's nothing feeding it.", Next = "camp" },

            new() { Id = "shatter", Speaker = Speakers.Narrator, Line = "{sb} doesn't need telling twice. One blow, two, and on the third the crystal bursts with a scream that isn't quite a sound." },
            new() { Speaker = SoulBound, Line = "See? Done. Sometimes the fast way is the right way." },
            new() { Speaker = Speakers.Narrator, Line = "Black sludge wells out of the broken stump, finds the stream, and goes with it." },
            new() { Speaker = "sera", Direction = "watching it go", Line = "...That's in the water now. All the way to the river farms." },
            new() { Speaker = SoulBound, Line = "Then we'll clean the stream as well. One thing at a time.", Next = "camp" },

            // The compromise: the guard wave. Sera starts, and the quarry answers.
            new() { Id = "guard", Speaker = "sera", Line = "Then hold it. Everything left in this wood will come for the light the moment I start." },
            new()
            {
                Speaker = Speakers.Narrator, Sound = Sfx.WolfHowl, Line = "She lays both hands on the crystal's roots. Out in the passage, something howls, and something else answers it.",
                Effects = new[] { DialogueEffect.Fight("a1-c2-02-guard-wave") }, Next = DialogueScript.End,
            },

            NightFalls("camp"),
        },
    };

    /// <summary>The compromise's end, once the den's held: the crystal shining, the two of you back to back, and out to camp.</summary>
    /// <remarks>EncounterCatalog's a1-c2-02-guard-wave plays it when the fight's won, which has set CrystalGuarded. The crystal only turns white once the den's been held.</remarks>
    public static readonly DialogueScript Guarded = new()
    {
        Id = "a1-c2-02-guard-wave-after",
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator, Line = "The last of them goes down at the den's mouth. Behind you, the crystal has stopped screaming, and started to shine.",
                Effects = new[] { DialogueEffect.Set(CrystalPurified) },
            },
            new() { Speaker = SoulBound, Direction = "breathing hard, grinning", Line = "Back to back. Just like home." },
            NightFalls(null),
        },
    };

    /// <summary>A1-C2-03: the night camp, the Soul-Bound by the fire, and Sera watching from the dark.</summary>
    public static readonly DialogueScript Camp = new()
    {
        Id = "a1-c2-03",
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator, Line = "Night. A small fire in a ring of stones. Sera has taken the first watch up on the rocks, out of the light.",
                Effects = new[] { DialogueEffect.Rest() },
            },
            new()
            {
                Speaker = Speakers.Narrator, Line = "{sb} came back to the fire an hour after dark, said nothing about the quarry, and handed you half an apple.",
                Conditions = new[] { DialogueCondition.Is(SeraPurified) },
            },
            new()
            {
                Speaker = SoulBound, Line = "Wolves, a bear made of thorns, and a rock that had it coming. Good day's work.",
                Conditions = new[] { DialogueCondition.Is(CrystalShattered) },
            },
            new()
            {
                Speaker = SoulBound, Direction = "rolling a sore shoulder", Line = "My arms are going to fall off. Worth it, though.",
                Conditions = new[] { DialogueCondition.Is(CrystalGuarded) },
            },
            new()
            {
                Speaker = SoulBound, Direction = "poking the fire", Line = "Remember the first night we ever slept rough? Under Harrow's hay barge, hiding from his men?",
                Options = new DialogueOption[]
                {
                    new() { Text = "You snored the whole night.", Next = "snored" },
                    new() { Text = "You kept the rats off me till morning.", Next = "rats" },
                },
            },
            new() { Id = "snored", Speaker = SoulBound, Line = "I did not. That was the barge. Barges snore.", Next = "army" },
            new() { Id = "rats", Speaker = SoulBound, Line = "And you kept the cold off me. Fair trade. Always has been." },
            new() { Id = "army", Speaker = SoulBound, Direction = "the smile fading", Line = "An army, {name}. Marching on a kingdom that doesn't even know it. Someone should be ready." },
            new() { Speaker = You, Line = "Someone like us?" },
            new() { Speaker = SoulBound, Direction = "smiling at the flames", Line = "Who else?" },
            new()
            {
                Speaker = Speakers.Narrator, Line = "Up on the rocks, Sera isn't watching the trees. She's watching {sb}. She doesn't say a word.",
                Effects = new[] { DialogueEffect.Set(SeraNoticedSb) },
            },
            new()
            {
                Speaker = SoulBound, Direction = "lying back", Line = "Get some sleep. I'll wake you for second watch. Then it's back to Questeria, and Fennick owes us triple.",
                Effects = new[] { DialogueEffect.Set(CampDone), DialogueEffect.Objective("Go back to Questeria and collect from Questmaster Fennick") },
            },
        },
    };

    /// <summary>A1-C2-04: Fennick pays the Rangers' contract, hears about the letter, and points you at the east gate.</summary>
    /// <remarks>Ahead of Act1Chapter1.FennickAfter in the catalogue, so it wins.</remarks>
    public static readonly DialogueScript FennickPays = new()
    {
        Id = "a1-c2-04",
        With = "fennick",
        Needs = new[] { CampDone },
        Unless = new[] { Paid },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "fennick", Line = "There they are! A Lodge runner came by at first light. Said the Mirewood's birds are singing again, and rangers get very emotional about birds." },
            new()
            {
                Speaker = "fennick", Line = "Said the rot's draining out of the trees like a fever breaking. Two days, they reckon, and it's gone.",
                Conditions = new[] { DialogueCondition.Is(CrystalPurified) },
            },
            new()
            {
                Speaker = "fennick", Line = "Said something about you two holding the old quarry against half the forest. Rangers exaggerate. Do rangers exaggerate?",
                Conditions = new[] { DialogueCondition.Is(CrystalGuarded) },
            },
            new()
            {
                Speaker = "fennick", Line = "Also said the stream below the quarry's running black, and the river farms drink from it. They'll be posting that one.",
                Conditions = new[] { DialogueCondition.Is(CrystalShattered) },
            },
            new() { Speaker = SoulBound, Line = "Then we'll be taking it.", Conditions = new[] { DialogueCondition.Is(CrystalShattered) } },
            new()
            {
                Speaker = "fennick", Sound = Sfx.Coin, Line = "Triple, as promised. The Lodge made me count it twice.",
                Effects = new[] { DialogueEffect.Gold(ContractPay), DialogueEffect.Add(Meters.Fame, 2) },
            },
            new()
            {
                Speaker = "sera", Direction = "dry", Line = "Triple pay, for a forest that's stood a thousand years. You city people.",
                Conditions = new[] { DialogueCondition.With("sera") },
            },
            new() { Speaker = SoulBound, Line = "We found something else in there, Fennick. A Doomward courier, dead, with a letter on him. There's an army coming." },
            new() { Speaker = "fennick", Direction = "the smile going", Line = "...I'm a questmaster. I hang paper on a board. That's a letter for the palace." },
            new() { Speaker = "fennick", Line = "And the palace doesn't open its doors to two sellswords off the morning ship. Not yet." },
            new() { Speaker = SoulBound, Line = "Not yet?" },
            new() { Speaker = "fennick", Line = "Make yourselves a name, and doors open. Speaking of which. Something's wrong at the east gate." },
            new() { Speaker = "fennick", Line = "The Watch has shut it on a crowd off the Doomward road, and Sergeant Brisk's been sent to hold it with every spear he's got. Both of them. And the dog." },
            new() { Speaker = "fennick", Line = "Nobody's posted it, because nobody's paying. But I've a feeling it's where you'll want to be." },
            new()
            {
                Speaker = SoulBound, Line = "Nobody's paying. Right. Then we'll go for free.",
                Effects = new[] { DialogueEffect.Set(Paid), DialogueEffect.Objective("See what's wrong at the east gate") },
            },
        },
    };

    /// <summary>Fennick once he's paid: the east gate, then the talk of the market as the story goes on.</summary>
    public static readonly DialogueScript FennickAfterPay = new()
    {
        Id = "a1-c2-04-after",
        With = "fennick",
        Needs = new[] { Paid },
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = "fennick", Line = "The east gate. Out past the market and along the wall. Brisk could use a friendly face.",
                Conditions = new[] { DialogueCondition.Not(Act1Chapter3.ChapterDone) },
            },
            new()
            {
                Speaker = "fennick", Line = "Thornbridge! The whole market's talking. Half of them say you're heroes, and the other half say you're trouble.",
                Conditions = new[] { DialogueCondition.Is(Act1Chapter3.ChapterDone), DialogueCondition.Not(Act1Chapter4.AudienceDone) },
            },
            new()
            {
                Speaker = "fennick", Line = "An audience with the King, I hear. Don't forget the little people. Or the board. Mostly the board.",
                Conditions = new[] { DialogueCondition.Is(Act1Chapter4.AudienceDone) },
            },
        },
    };

    /// <summary>Ranger Tamsin once you're back from the Mirewood, and what became of the Lodge's woodcutters.</summary>
    /// <remarks>Ahead of Townspeople's Tamsin in the catalogue, so it wins.</remarks>
    public static readonly DialogueScript TamsinBack = new()
    {
        Id = "tamsin-mirewood",
        With = "tamsin",
        Needs = new[] { CampDone },
        Unless = new[] { TamsinThanked },
        Marker = MarkerKind.Quest,
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "tamsin", Direction = "unfolding her arms", Line = "You came back. The Lodge says the birds came back too. I didn't believe either until now." },
            new() { Speaker = SoulBound, Direction = "gently", Line = "We found your woodcutters. What was left of them. I'm sorry." },
            new() { Speaker = "tamsin", Direction = "after a long breath", Line = "...Better found than wondered about. Their families will want to know. I'll tell them myself." },
            new() { Speaker = "tamsin", Line = "And you've brought Sera Vantis out of the trees. Nobody does that.", Conditions = new[] { DialogueCondition.With("sera") } },
            new() { Speaker = "sera", Line = "Tamsin. You look like a city.", Conditions = new[] { DialogueCondition.With("sera") } },
            new() { Speaker = "tamsin", Line = "And you look like you slept in a bush.", Conditions = new[] { DialogueCondition.With("sera") } },
            new() { Speaker = "sera", Line = "I did.", Conditions = new[] { DialogueCondition.With("sera") } },
            new()
            {
                Speaker = "tamsin", Line = "The Lodge owes you. So do I. If you're ever out on the road and need a ranger, shout.",
                Effects = new[] { DialogueEffect.Set(TamsinThanked) },
            },
        },
    };

    /// <summary>Tamsin after that.</summary>
    public static readonly DialogueScript TamsinAfterThanks = new()
    {
        Id = "tamsin-mirewood-after",
        With = "tamsin",
        Needs = new[] { TamsinThanked },
        Nodes = new DialogueNode[] { new() { Speaker = "tamsin", Line = "Shout if you need a ranger. I'll come running. Quietly." } },
    };

    /// <summary>Lark once you're back from the Mirewood: the second verse she asked for.</summary>
    /// <remarks>Ahead of Townspeople's Lark in the catalogue, so she stops asking whether you're off to the Mirewood.</remarks>
    public static readonly DialogueScript LarkVerse = new()
    {
        Id = "lark-mirewood-verse",
        With = "lark",
        Needs = new[] { CampDone },
        Unless = new[] { LarkSecondVerse },
        Nodes = new DialogueNode[]
        {
            new() { Speaker = "lark", Direction = "stopping mid-strum", Line = "Wait. Wait! You're the Mirewood pair. You went in and came back out, which almost nobody does in songs." },
            new()
            {
                Speaker = "lark", Line = "Second verse. Give me something true and I'll make it rhyme.",
                Options = new DialogueOption[]
                {
                    new() { Text = "There was a bear, grown through with thorns.", Next = "bear" },
                    new() { Text = "We met a ranger who shoots first and talks later.", Next = "ranger" },
                    new()
                    {
                        Text = "I was magnificent. Write that down.", Next = "magnificent",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "bear", Speaker = "lark", Line = "A bear grown through with thorns. Oh, that's horrible. That's good. That's horribly good.", Next = "sing" },
            new() { Id = "ranger", Speaker = "lark", Line = "A ranger with a temper. Crowds love a ranger with a temper." },
            new()
            {
                Speaker = "sera", Direction = "flatly", Line = "I talked first. You weren't listening.",
                Conditions = new[] { DialogueCondition.With("sera") }, Next = "sing",
            },
            new() { Id = "magnificent", Speaker = "lark", Line = "'Magnificent' rhymes with nothing. I'll make something up. That's the job." },
            new()
            {
                Id = "sing", Speaker = Speakers.Narrator,
                Line = "She tries it on the fountain crowd: wolves with smoke for eyes, the quarry's black heart, and two strangers off the morning ship who wouldn't turn back.",
            },
            new()
            {
                Speaker = Speakers.Narrator, Line = "The crowd claps. Somebody whistles. A copper lands in the hat, then a few more.",
                Effects = new[] { DialogueEffect.Add(Meters.Fame, 1) },
            },
            new() { Speaker = SoulBound, Direction = "grinning", Line = "We're in a song, {name}. An actual song." },
            new()
            {
                Speaker = "lark", Line = "Do something bigger and you'll get a chorus.",
                Effects = new[] { DialogueEffect.Set(LarkSecondVerse) },
            },
        },
    };

    /// <summary>Lark after the Mirewood's verse.</summary>
    public static readonly DialogueScript LarkAfterVerse = new()
    {
        Id = "lark-mirewood-after",
        With = "lark",
        Needs = new[] { LarkSecondVerse },
        Nodes = new DialogueNode[] { new() { Speaker = "lark", Line = "The Mirewood verse is my best seller. Go and do something else dangerous. I need a chorus." } },
    };

    /// <summary>The line every way through the quarry ends on: the crystal's dealt with, and out to camp in the Mirewood.</summary>
    /// <param name="id">What the line's called, for the ways that go to it, or null if nothing does.</param>
    private static DialogueNode NightFalls(string id) => new()
    {
        Id = id, Speaker = Speakers.Narrator, Line = "Night's coming on by the time you climb out of the quarry. You make camp where the trees are still green.",
        Effects = new[] { DialogueEffect.Set(CrystalDealtWith), DialogueEffect.Set(QuarryDone), DialogueEffect.Objective("Rest at the camp in the Mirewood"), DialogueEffect.Travel("mirewood", "camp") },
    };
}
