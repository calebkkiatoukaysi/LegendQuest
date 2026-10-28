using LegendQuest.Characters;
using LegendQuest.Story;

namespace LegendQuest.Dialogue.Scripts;

/// <summary>
/// The Prologue, "Two Names, One Oath": the title card, who drives you, the Soul-Bound remembered,
/// the night on the Gull's Wager, the sea-wraiths, and Questeria at dawn.
/// </summary>
/// <remarks>
/// Every line is STORY.md's, scenes P-01 to P-05; the ids are the scene ids lowercased. The
/// sea-wraith fight is a card until combat exists, with the Soul-Bound's barks round it, and the
/// Soul-Link it teaches comes with combat too.
/// </remarks>
public static class Prologue
{
    private const string You = Origins.You;
    private const string SoulBound = Origins.SoulBound;

    /// <summary>The flag P-04 sets when you warn the Soul-Bound about crowns. A1-C6-04 reads it.</summary>
    public const string SbWarned = "sb-warned-p";

    /// <summary>The flag the end of the Prologue sets: you're coming into Questeria's harbour.</summary>
    public const string Done = "prologue-done";

    /// <summary>P-01: the card before anything.</summary>
    public static readonly DialogueScript TitleCard = new()
    {
        Id = "p-01",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Card, Line = "Every legend begins with a promise. Most end with one broken." },
        },
    };

    /// <summary>P-02: after you're made, the narrator names you and asks what drives you, which sets your voice.</summary>
    public static readonly DialogueScript YourVoice = new()
    {
        Id = "p-02",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = Speakers.Narrator, Line = "{name}. A name the world has not yet learned. It will." },
            new()
            {
                Speaker = Speakers.Narrator,
                Line = "What drives you?",
                Options = new DialogueOption[]
                {
                    new() { Text = "Doing what's right, even when it costs me.", Effects = new[] { DialogueEffect.Speak(Voice.Honorable) } },
                    new() { Text = "Getting the job done. Ideals don't pay for bread.", Effects = new[] { DialogueEffect.Speak(Voice.Pragmatic) } },
                    new() { Text = "A good story. And a better tavern at the end of it.", Effects = new[] { DialogueEffect.Speak(Voice.Witty) } },
                },
            },
        },
    };

    /// <summary>P-03: after the Soul-Bound's made, the memory of the pier, what you remember of them, and the oath under the red moon.</summary>
    public static readonly DialogueScript Remembered = new()
    {
        Id = "p-03",
        Nodes = new DialogueNode[]
        {
            new()
            {
                Speaker = Speakers.Narrator, Painting = "pier-memory",
                Line = "You were eight when you met. A storm, a capsized boat, and a child who dove in after you when no one else would.",
            },
            new()
            {
                Speaker = Speakers.Narrator, Painting = "pier-memory",
                Line = "What do you remember most about {sb}?",
                Options = new DialogueOption[]
                {
                    new() { Text = "{Sb_they} never let me give up.", Effects = new[] { DialogueEffect.Add(Meters.Bond, 5) } },
                    new() { Text = "{Sb_they} always wanted more than our little town could give.", Effects = new[] { DialogueEffect.Add(Meters.Ambition, 5) } },
                    new() { Text = "{Sb_they} made me laugh when there was nothing to laugh about.", Effects = new[] { DialogueEffect.Add(Meters.Bond, 5) } },
                },
            },
            new()
            {
                Speaker = Speakers.Narrator, Painting = "red-moon",
                Line = "At sixteen, under a red moon, you cut your palms and clasped hands. The Soul-Bond. Two lives, one oath: never leave the other behind.",
            },
        },
    };

    /// <summary>P-04 and P-05: on the deck at night, two days out, then the sea-wraiths, and Questeria rising out of the sea at dawn.</summary>
    public static readonly DialogueScript Voyage = new()
    {
        Id = "p-04",
        Nodes = new DialogueNode[]
        {
            new() { Speaker = SoulBound, Line = "Can't sleep either?" },
            new() { Speaker = You, Line = "Too quiet. I keep waiting for something to try to eat us." },
            new() { Speaker = SoulBound, Direction = "laughing", Line = "Give it time. Fable's supposed to be crawling with work. Monsters, bandits, lost heirlooms. Coin for days." },
            new()
            {
                Speaker = SoulBound,
                Line = "Captain says King Leon pays adventurers in gold and gratitude. Imagine that. A king who says thank you.",
                Options = new DialogueOption[]
                {
                    new() { Text = "Don't get your hopes up. Kings are kings.", Next = "kings" },
                    new() { Text = "Maybe this one's different.", Next = "different", Effects = new[] { DialogueEffect.Add(Meters.Bond, 2) } },
                    new()
                    {
                        Text = "I'll settle for gold. Gratitude doesn't fill a stomach.", Next = "gold",
                        Conditions = new[] { DialogueCondition.Speaks(Voice.Witty) }, Locked = true,
                    },
                },
            },
            new() { Id = "kings", Speaker = SoulBound, Line = "Ha. Spoken like someone who grew up where we did.", Next = "harrow" },
            new() { Id = "different", Speaker = SoulBound, Line = "Maybe. I'd like to see that, just once.", Next = "harrow" },
            new() { Id = "gold", Speaker = SoulBound, Line = "That's why you're the brains and I'm the charm." },

            new() { Id = "harrow", Speaker = SoulBound, Direction = "quieter, looking at the water", Line = "Do you remember Lord Harrow's men? When they took the nets?" },
            new() { Speaker = You, Line = "I remember." },
            new() { Speaker = SoulBound, Line = "My mother begged. On her knees, in the mud. And he didn't even look at her." },
            new() { Speaker = SoulBound, Line = "I swore then that I'd never be that small again. That no one I loved would be." },
            new()
            {
                Speaker = SoulBound, Direction = "the grin comes back",
                Line = "So here's the plan. We go to Fable. We make a name. Then a bigger name. And someday, I'm going to have a kingdom of my own. A place where nobody kneels in the mud.",
                Options = new DialogueOption[]
                {
                    new() { Text = "And I'll be right there with you.", Next = "with-you", Effects = new[] { DialogueEffect.Add(Meters.Bond, 5) } },
                    new() { Text = "A kingdom? Let's start with rent money.", Next = "rent" },
                    new() { Text = "Careful. People who want crowns usually lose something getting them.", Next = "crowns", Effects = new[] { DialogueEffect.Set(SbWarned) } },
                },
            },
            new() { Id = "with-you", Speaker = SoulBound, Line = "Never doubted it. Soul-Bound, remember?", Next = "bell" },
            new() { Id = "rent", Speaker = SoulBound, Line = "Small steps. Big dreams.", Next = "bell" },
            new() { Id = "crowns", Speaker = SoulBound, Direction = "a pause, then a smile", Line = "Then I'll just have to want it more than they did." },

            // P-05: the bell, the wraiths, and the fight as a card until combat exists.
            new() { Id = "bell", Speaker = "lookout", Direction = "from the crow's nest, over the bell", Line = "Sea-wraiths off the port bow!", Sound = Sfx.ShipBell },
            new() { Speaker = SoulBound, Direction = "drawing", Line = "Together! Just like the old days!" },
            new() { Speaker = Speakers.Card, Line = "The sea-wraiths come over the rail, and go back into the sea in pieces." },
            new() { Speaker = SoulBound, Direction = "breathing hard", Line = "I've got you. I've always got you." },

            new() { Speaker = Speakers.Narrator, Painting = "questeria-dawn", Line = "Dawn breaks. Questeria rises on its hill: white walls, and a golden spire." },
            new() { Speaker = "odelle", Painting = "questeria-dawn", Line = "There she is. Questeria, Capital of Fable. Don't let the shine fool you, it's got teeth like any city." },
            new() { Speaker = SoulBound, Painting = "questeria-dawn", Direction = "awed", Line = "Look at that. That's what a kingdom looks like." },
            new()
            {
                Speaker = Speakers.Card, Painting = "questeria-dawn", Line = "Prologue complete.",
                Effects = new[] { DialogueEffect.Set(Done), DialogueEffect.Travel("docks", "start") },
            },
        },
    };
}
