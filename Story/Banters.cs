using System;
using System.Collections.Generic;
using LegendQuest.Characters;
using LegendQuest.Dialogue.Scripts;

namespace LegendQuest.Story;

/// <summary>
/// Every banter in the game, by id: the party talking about where they've got to, and the
/// townsfolk talking to each other.
/// </summary>
/// <remarks>
/// Mine, not STORY.md's, written round its bible: the Watch half gone east, ash on the wind from
/// Doomward, the Mirewood going wrong, the Princess riding out on her own. The Soul-Bound does most
/// of the talking, and you only get a word or two in, since how you sound is the player's to pick.
/// A line from someone on a map names them by their id there, so a banter between two townsfolk
/// only plays where they both stand. Kept as a table like the meters, so a map naming a banter that
/// isn't here, or a banter with a blank line, is found when the game starts.
/// </remarks>
public static class Banters
{
    private const string You = Origins.You;
    private const string SoulBound = Origins.SoulBound;

    private static readonly Banter[] Table =
    {
        // The Gull's Wager, at night, before the bell.
        new()
        {
            Id = "deck-night",
            Unless = new[] { Prologue.Done },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Listen. Nothing but rope creaking and somebody snoring."),
                new(You, "Two more days of this."),
                new(SoulBound, "Two more days, then a whole new kingdom. I can't sit still."),
            },
        },
        new()
        {
            Id = "deck-sailors",
            Unless = new[] { Prologue.Done },
            Lines = new BanterLine[]
            {
                new("sailor-a", "Wraith weather, this. Flat sea, no moon."),
                new("sailor-b", "Don't say wraith. Captain fines you for saying wraith."),
                new("sailor-a", "Captain fines me for breathing."),
                new("sailor-b", "Then breathe quieter."),
            },
        },

        // Saltwind Docks: the Red Gulls ahead, then after them.
        new()
        {
            Id = "docks-gulls-ahead",
            Unless = new[] { Act1Chapter1.GullsBeaten },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Red bandanas, one old man, and a crowd looking the other way."),
                new(SoulBound, "I know this one. I grew up watching this one."),
            },
        },
        new()
        {
            Id = "docks-boardwalk",
            Needs = new[] { Act1Chapter1.GullsBeaten },
            Unless = new[] { Act1Chapter1.DockCruelty },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Fish, tar and wet rope. Smells like home, honestly."),
                new(You, "Home smelled worse."),
                new(SoulBound, "Home smelled of Harrow's men. This is just fish."),
            },
        },
        new()
        {
            Id = "docks-boardwalk-grell",
            Needs = new[] { Act1Chapter1.DockCruelty },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Think Grell's dried off yet?"),
                new(You, "You didn't have to kick him in."),
                new(SoulBound, "No. But it felt like paying back a very old debt."),
                new(SoulBound, "...Too far? Tell me if it's too far."),
            },
        },
        new()
        {
            Id = "docks-fight-mess",
            Needs = new[] { Act1Chapter1.GullsBeaten },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Splinters, a red bandana, and a tooth. Not mine."),
                new(You, "Grell's?"),
                new(SoulBound, "He can come back for it. I'd like to see him try."),
            },
        },
        new()
        {
            Id = "docks-fish",
            Needs = new[] { Act1Chapter1.GullsBeaten },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Eels the length of my arm. Who eats those?"),
                new(You, "Rich people."),
                new(SoulBound, "Then I'll learn to love eels."),
            },
        },
        new()
        {
            Id = "docks-gulls",
            Needs = new[] { Act1Chapter1.GullsBeaten },
            Lines = new BanterLine[]
            {
                new(SoulBound, "That gull's been following me since we got off the ship."),
                new(You, "It wants your breakfast."),
                new(SoulBound, "Well, it can't have it. I'm a friend of the people, not the gulls."),
            },
        },
        new()
        {
            Id = "docks-warehouses",
            Needs = new[] { Act1Chapter1.GullsBeaten },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Crates from every coast in the world, stacked to the roof."),
                new(SoulBound, "All that money going past, and Tomas couldn't keep his catch."),
                new(You, "He can now."),
                new(SoulBound, "He can now. Somebody should make that the rule."),
            },
        },
        new()
        {
            Id = "docks-up-to-city",
            Needs = new[] { Act1Chapter1.DocksDone },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Up that street's the capital. The actual capital."),
                new(SoulBound, "Stand up straight. We're about to be important."),
                new(You, "We're about to be lost."),
                new(SoulBound, "Important people get lost all the time. They call it touring."),
            },
        },
        new()
        {
            Id = "docks-fishwives",
            Needs = new[] { Act1Chapter1.GullsBeaten },
            Lines = new BanterLine[]
            {
                new("fishmonger-a", "Saw the Red Gulls go down like wet sacks, I did."),
                new("fishmonger-b", "About time somebody did it."),
                new("fishmonger-a", "Watch never did."),
                new("fishmonger-b", "Watch is off to Greywatch, love. Half of it, anyhow."),
            },
        },
        new()
        {
            Id = "docks-sailors-ash",
            Lines = new BanterLine[]
            {
                new("sailor-c", "Came in past the Ashen Strait. Grey sky the whole way."),
                new("sailor-d", "Grey how? Storm grey?"),
                new("sailor-c", "Ash grey. Ash on the rigging, ash in the water barrels."),
                new("sailor-d", "Don't talk about the east on a dock. It's bad luck."),
            },
        },

        // Questeria: the plaza, the market and the board, before the contract's taken and after.
        new()
        {
            Id = "city-fountain",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Look at this fountain. Somebody paid a fortune for wet stone."),
                new(You, "It's beautiful."),
                new(SoulBound, "It is. Back home the well was a bucket and a prayer."),
            },
        },
        new()
        {
            Id = "city-waypoint",
            Lines = new BanterLine[]
            {
                new(SoulBound, "The crystal hums. Can you feel it in your teeth?"),
                new(You, "I can now."),
                new(SoulBound, "Touch it once and it'll bring us back. Magic for the lost."),
            },
        },
        new()
        {
            Id = "city-market",
            Lines = new BanterLine[]
            {
                new(SoulBound, "The Lantern Market. Bet it glows like a festival at night."),
                new(SoulBound, "Smell that? Bread, spice, and other people's money."),
                new(You, "Hands where I can see them."),
                new(SoulBound, "I was only looking. Looking's free."),
            },
        },
        new()
        {
            Id = "city-board",
            Unless = new[] { Act1Chapter1.BoardRead },
            Lines = new BanterLine[]
            {
                new(SoulBound, "There's the board. Look at all that work."),
                new(SoulBound, "Monsters, bandits, lost rings. We could eat for a year."),
            },
        },
        new()
        {
            Id = "city-board-taken",
            Needs = new[] { Act1Chapter1.BoardRead },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Triple pay. I keep saying it in my head. Triple."),
                new(You, "Don't spend it yet."),
                new(SoulBound, "Too late. I've spent it twice."),
            },
        },
        new()
        {
            Id = "city-terrace",
            Lines = new BanterLine[]
            {
                new(SoulBound, "There. Up the hill, behind the palace. The Sunspire Keep."),
                new(SoulBound, "King Leon's up there somewhere, saying thank you to people."),
                new(You, "You don't believe it."),
                new(SoulBound, "I'd like to. That's not the same thing."),
            },
        },
        new()
        {
            Id = "city-terrace-crown",
            Needs = new[] { Act1Chapter1.BoardRead },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Imagine waking up in that keep. Every window full of city."),
                new(SoulBound, "Every person down here, yours to look after."),
                new(You, "Yours?"),
                new(SoulBound, "Figure of speech. Mostly."),
            },
        },
        new()
        {
            Id = "city-temple",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Listen. Bells, somewhere inside. Bit out of tune."),
                new(You, "Going in?"),
                new(SoulBound, "Temples always want something. A coin, or a confession."),
            },
        },
        new()
        {
            Id = "city-guildhall",
            Lines = new BanterLine[]
            {
                new(SoulBound, "The guildhall. Merchants, masons and mages under one roof."),
                new(SoulBound, "The Mages Guild proper's up at the Arcanum Spire. Somewhere tall."),
                new(You, "Naturally."),
            },
        },
        new()
        {
            Id = "city-canal",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Canals in the streets. Boats where the carts should be."),
                new(You, "Don't fall in."),
                new(SoulBound, "Last time I went in the water, it was after you. Remember?"),
                new(You, "I remember."),
            },
        },
        new()
        {
            Id = "city-houses-west",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Flower boxes on every window. Back home we grew turnips in ours."),
                new(You, "We didn't have windows."),
                new(SoulBound, "Exactly my point."),
            },
        },
        new()
        {
            Id = "city-houses-south",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Imagine owning one. A door that locks. A roof that doesn't leak."),
                new(You, "Someday."),
                new(SoulBound, "Someday. I'm holding you to that."),
            },
        },
        new()
        {
            Id = "city-gate",
            Unless = new[] { Act1Chapter1.BoardRead },
            Lines = new BanterLine[]
            {
                new(SoulBound, "The gate's open. The old road's out there, and whatever's past it."),
                new(You, "No contract, no reason to go."),
                new(SoulBound, "Fair. The board first, then glory."),
            },
        },
        new()
        {
            Id = "city-gate-mirewood",
            Needs = new[] { Act1Chapter1.BoardRead },
            Lines = new BanterLine[]
            {
                new(SoulBound, "The Mirewood's down that road. Mad animals, missing woodcutters."),
                new(SoulBound, "Let's go be the reason somebody gets to come home."),
            },
        },
        new()
        {
            Id = "city-vendors",
            Lines = new BanterLine[]
            {
                new("gossip-a", "Heard the Princess rode out the south gate again. No escort."),
                new("gossip-b", "Elara? She'd outride the escort anyway."),
                new("gossip-a", "Her father'll go grey."),
                new("gossip-b", "He's already grey. That's what kings are for."),
            },
        },
        new()
        {
            Id = "city-watch",
            Lines = new BanterLine[]
            {
                new("watch-a", "Another double shift."),
                new("watch-b", "Blame Greywatch. Half the barracks marched east."),
                new("watch-a", "For what, though?"),
                new("watch-b", "Captain says rumours. He says it like it's a swear word."),
            },
        },
        new()
        {
            Id = "city-kids",
            Lines = new BanterLine[]
            {
                new("kid-a", "I'm the Hollowed and you're the Watch. Grrr."),
                new("kid-b", "No! You were the Hollowed yesterday!"),
                new("kid-a", "Fine. I'll be Archmage Varn."),
                new("kid-b", "You've got all your fingers. That's cheating."),
            },
        },
        new()
        {
            Id = "city-old-couple",
            Lines = new BanterLine[]
            {
                new("old-a", "Forty years of peace, Edda. Forty."),
                new("old-b", "And you've grumbled through every one of them."),
                new("old-a", "I'm only saying it's been nice."),
                new("old-b", "You're saying it like it's ending."),
            },
        },
        new()
        {
            Id = "city-busker",
            Lines = new BanterLine[]
            {
                new("lark", "Ohh, the lanterns of the harbour, where the gulls are never..."),
                new("heckler", "Never what? You've sung that line six times!"),
                new("lark", "Then it's a chorus."),
                new("heckler", "It's a threat."),
            },
        },
        new()
        {
            Id = "city-acolytes",
            Lines = new BanterLine[]
            {
                new("acolyte-a", "The new boy rang noon twice today."),
                new("acolyte-b", "Says the first one was practice."),
                new("acolyte-a", "Old Edric never practised."),
                new("acolyte-b", "Old Edric never needed to."),
            },
        },

        // Indoors: the Flagon, the chandler's, a warehouse, and somebody's cottage.
        new()
        {
            Id = "flagon-inside",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Now this is a tavern. Warm fire, sticky floor, good noise."),
                new(You, "It's perfect."),
                new(SoulBound, "First round's on you. So's the second."),
            },
        },
        new()
        {
            Id = "flagon-regulars",
            Lines = new BanterLine[]
            {
                new("patron-1", "Hear the rats under the floor again last night?"),
                new("patron-2", "Those weren't rats. Rats don't stare."),
                new("patron-1", "How'd you know they were staring?"),
                new("patron-2", "I could feel it through the floorboards."),
            },
        },
        new()
        {
            Id = "shop-inside",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Rope, wax and lamp oil. Smells like a ship's belly in here."),
                new(SoulBound, "Who needs a candle that big?"),
                new(You, "Somebody scared of the dark."),
                new(SoulBound, "Fair. Buy two."),
            },
        },
        new()
        {
            Id = "warehouse-inside",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Crates to the rafters. Bet half of these paid the Red Gulls' tax."),
                new(You, "Don't open anything."),
                new(SoulBound, "I'm reading the labels. With my eyes. Very loudly."),
            },
        },
        new()
        {
            Id = "cottage-snoop",
            Lines = new BanterLine[]
            {
                new(SoulBound, "We just walked into somebody's house."),
                new(You, "The door was open."),
                new(SoulBound, "That's what every burglar says."),
                new(SoulBound, "Nice bed, though. Don't touch the bed."),
            },
        },
        new()
        {
            Id = "cottage-hearth",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Fire's still going. Somebody's only stepped out."),
                new(SoulBound, "If they come back, we're inspectors. Hearth inspectors."),
                new(You, "It's a good hearth."),
                new(SoulBound, "See? You're a natural."),
            },
        },

        // The old road and the barrow.
        new()
        {
            Id = "road-out",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Out of the gate and straight into trees. Fable doesn't waste time."),
                new(You, "It's getting dark."),
                new(SoulBound, "It's dusk. Dark's later. Dark's worse."),
            },
        },
        new()
        {
            Id = "road-camp",
            Unless = new[] { Townspeople.TollroadHeard },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Fire's still warm. Whoever camps here left in a hurry."),
                new(You, "Or they're close."),
                new(SoulBound, "Thanks. I hate that."),
            },
        },
        new()
        {
            Id = "road-camp-bandits",
            Needs = new[] { Townspeople.TollroadHeard },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Bedrolls, a stash, and a good view of the road. Brisk's bandits?"),
                new(You, "Could be."),
                new(SoulBound, "Then they owe the road some coin back. With interest."),
            },
        },
        new()
        {
            Id = "road-fork",
            Lines = new BanterLine[]
            {
                new(SoulBound, "West to the city, north to a barrow."),
                new(SoulBound, "Nobody ever puts up a sign saying north to a picnic."),
            },
        },
        new()
        {
            Id = "barrow-mouth",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Steps going down into a hill. Every story like this ends badly."),
                new(You, "We're not in a story."),
                new(SoulBound, "Not yet."),
            },
        },
        new()
        {
            Id = "barrow-inside",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Cold in here. Not cave cold. Kept cold."),
                new(You, "Stay close."),
                new(SoulBound, "Always."),
            },
        },
        new()
        {
            Id = "barrow-hall",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Whoever's in there was important once. Big door, small comfort."),
                new(You, "It's locked."),
                new(SoulBound, "The important ones always are."),
            },
        },
    };

    private static readonly Dictionary<string, Banter> ById = new();

    /// <summary>Indexes the table.</summary>
    /// <remarks>Throws on purpose, so a typo shows up when the game starts: a banter with no id or no lines, a line nobody says or that says nothing, or two banters with the same id.</remarks>
    static Banters()
    {
        foreach (Banter banter in Table)
        {
            if (string.IsNullOrWhiteSpace(banter.Id)) throw new InvalidOperationException("A banter has no id.");
            if (banter.Lines.Count == 0) throw new InvalidOperationException($"Banter {banter.Id} has no lines.");
            foreach (BanterLine line in banter.Lines)
                if (string.IsNullOrWhiteSpace(line.Speaker) || string.IsNullOrWhiteSpace(line.Line))
                    throw new InvalidOperationException($"Banter {banter.Id} has a line with nobody saying it, or nothing said.");
            if (!ById.TryAdd(banter.Id, banter)) throw new InvalidOperationException($"Two banters are called {banter.Id}.");
        }
    }

    /// <summary>One banter, or null if there's none called that.</summary>
    /// <param name="id">Its id.</param>
    public static Banter Get(string id) => ById.GetValueOrDefault(id ?? string.Empty);

    /// <summary>Whether there's a banter called this.</summary>
    /// <param name="id">What a map's trigger calls it.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);
}
