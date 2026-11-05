using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// Act 1, Chapter 2, "The Rot in the Mirewood": the gloomfangs at the forest's edge and Sera, the
/// old quarry and the dead courier, the Thornmother and the black crystal, the night camp, and
/// Fennick paying out back in the city.
/// </summary>
/// <remarks>
/// A1-C2-01 to A1-C2-03 are compiled from Story/04-script-prologue-act1.md into StoryPrologueAct1;
/// this class keeps their flags by name, and the scenes 04 leaves as built: Sera talking the
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
                Speaker = "fennick", Sound = Sfx.Coin, Line = "Triple, as promised. I'd like it noted that you didn't ask what it was.",
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
