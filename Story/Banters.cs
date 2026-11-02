using System;
using System.Collections.Generic;
using System.Linq;
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
/// A companion's line is only said if they're in the party, so theirs are asides that stand on
/// their own (COMPANIONS.md has who notices what). A line from someone on a map names them by their
/// id there, so a banter between two townsfolk only plays where they both stand. Kept as a table
/// like the meters, so a map naming a banter that isn't here, or a banter with a blank line, is
/// found when the game starts.
/// </remarks>
public static class Banters
{
    private const string You = Origins.You;
    private const string SoulBound = Origins.SoulBound;
    private const string Sera = "sera";
    private const string Mira = "mira";
    private const string Kael = "kael";
    private const string Brannock = "brannock";

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

        // The old road, through the meadows at noon, and the barrow. Sera only walks it on the way back.
        new()
        {
            Id = "road-out",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Out of the gate and straight into all this green. Fable doesn't do anything by halves."),
                new(You, "It's so bright."),
                new(SoulBound, "Blue sky, warm sun, birds going mad. Something's bound to go wrong."),
            },
        },
        new()
        {
            Id = "meadow-out",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Wildflowers up to your knees. Back home the only flowers grew on the cliffs, where the goats couldn't get them."),
                new(You, "Smells like honey."),
                new(SoulBound, "Smells like somebody's very nice farm. Look, a cow. I'm in love with a cow."),
                new(Sera, "Clover and bees. This is what the edge of my wood should smell like."),
            },
        },
        new()
        {
            Id = "meadow-stream",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Water so clear you can count the stones. I could drink the whole stream."),
                new(You, "Don't."),
                new(SoulBound, "I said I could. I didn't say I would."),
                new(Sera, "It runs east, into the Mirewood. Whatever happens to this water happens to my trees."),
            },
        },
        new()
        {
            Id = "meadow-farm",
            Lines = new BanterLine[]
            {
                new(SoulBound, "A windmill. A real one. Look at it go round."),
                new(SoulBound, "Imagine owning all this. Your own field, your own mill, nobody's reeve at the gate."),
                new(You, "You'd be bored in a week."),
                new(SoulBound, "Two days. But they'd be a very good two days."),
                new(Sera, "The farmer leaves the field edges wild for the hares. A good farmer."),
            },
        },
        new()
        {
            Id = "meadow-mirewood-edge",
            Needs = new[] { Act1Chapter1.BoardRead },
            Lines = new BanterLine[]
            {
                new(SoulBound, "There it is. The Mirewood. Green on this side, and on that side... less green."),
                new(You, "The birds have gone quiet."),
                new(SoulBound, "The rangers have a saying. If the birds go quiet, so do you."),
                new(SoulBound, "This is me being quiet. Very quiet. Listen to how quiet I'm being."),
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

        // The Mirewood: its green edge before Sera, and the rot further in with her.
        new()
        {
            Id = "mirewood-edge",
            Unless = new[] { Companions.SeraRecruited },
            Lines = new BanterLine[]
            {
                new(SoulBound, "It's lovely in here. Moss, ferns, the light coming down green."),
                new(SoulBound, "So why does it feel like the trees are holding their breath?"),
                new(You, "Something's wrong further in."),
                new(SoulBound, "Course it is. Nobody pays triple for lovely."),
            },
        },
        new()
        {
            Id = "mirewood-veins",
            Needs = new[] { Companions.SeraRecruited },
            Lines = new BanterLine[]
            {
                new(SoulBound, "That tree's got black running through the bark. Like veins."),
                new(Sera, "Don't touch it. The rot goes into skin as easily as wood."),
                new(SoulBound, "I wasn't going to touch it."),
                new(You, "You were reaching."),
                new(Sera, "You were reaching."),
            },
        },
        new()
        {
            Id = "mirewood-tar",
            Needs = new[] { Companions.SeraRecruited },
            Lines = new BanterLine[]
            {
                new(Sera, "Tar. There was a spring here last summer. Deer drank from it."),
                new(SoulBound, "Can we burn it out?"),
                new(Sera, "You can burn anything. Then you've got ash, with tar under the ash."),
                new(SoulBound, "Right. Not burning it. Noted."),
            },
        },
        new()
        {
            Id = "mirewood-ranger-sign",
            Needs = new[] { Companions.SeraRecruited },
            Lines = new BanterLine[]
            {
                new(SoulBound, "A sign. 'Lodge path. Wardens only.' Somebody's carved a face on it."),
                new(Sera, "That's my face."),
                new(SoulBound, "It's very stern."),
                new(Sera, "It's very accurate."),
            },
        },
        new()
        {
            Id = "mirewood-quarry-mouth",
            Needs = new[] { Companions.SeraRecruited },
            Unless = new[] { Companions.CrystalDealtWith },
            Lines = new BanterLine[]
            {
                new(Sera, "The quarry. The black's coming up out of it like smoke out of a chimney."),
                new(SoulBound, "Then that's where the fire is."),
                new(Sera, "Or the chimney. Look before you swing, {sb}."),
                new(You, "Together, then. Carefully."),
            },
        },

        // The old quarry, on the way down to the crystal.
        new()
        {
            Id = "quarry-down",
            Unless = new[] { Companions.CrystalDealtWith },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Down we go. Into the dark, under the rocks, towards the evil glow."),
                new(You, "You're enjoying this."),
                new(SoulBound, "A bit. Is that wrong? That's a bit wrong."),
                new(Sera, "Breathe through your sleeve. The air down here has been sitting with the rot."),
            },
        },
        new()
        {
            Id = "quarry-roots",
            Unless = new[] { Companions.CrystalDealtWith },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Roots. Through solid stone. As thick as my arm."),
                new(Sera, "They're frightened. Roots grow towards water. These are growing away from something."),
                new(SoulBound, "I hate how much sense that makes."),
            },
        },
        new()
        {
            Id = "quarry-crystal-hum",
            Unless = new[] { Companions.CrystalDealtWith },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Do you hear that? A hum. Like something breathing through its teeth."),
                new(Sera, "I've heard it since the quarry mouth. Every animal in the wood has."),
                new(SoulBound, "It's almost... I don't know. Like it's asking for something."),
                new(Sera, "Don't listen to it. Things that ask in the dark aren't asking."),
                new(You, "{sb}? Stay with me."),
                new(SoulBound, "Always. Come on."),
            },
        },

        // Thornbridge: the camp outside the east gate, before the raid and after it.
        new()
        {
            Id = "thornbridge-camp",
            Needs = new[] { Companions.EastGate },
            Unless = new[] { Companions.RaidBeaten },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Tents all the way to the river. Hundreds of them. And a wall with a gate in it, shut."),
                new(Sera, "Smoke, wet wool, too many people on too little ground. Sickness follows that smell."),
                new(SoulBound, "Look. Somebody's going tent to tent with bandages. Doing the Watch's job for them."),
                new(You, "Then let's help."),
            },
        },
        new()
        {
            Id = "thornbridge-river",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Kids fishing with string and bent pins. We used to do that."),
                new(You, "We caught nothing."),
                new(SoulBound, "We caught a boot once. Best day of my life."),
                new(Sera, "The water's clean this far up. Someone should tell the camp to drink above the washing."),
                new(Mira, "I've told them to wash below the drinking. Three times. Washing is a very strong habit."),
            },
        },
        new()
        {
            Id = "thornbridge-after-raid",
            Needs = new[] { Companions.RaidBeaten },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Ash on everything. On the tents, on the kids. I'll be tasting it for a week."),
                new(Mira, "They came out of the trees in rows. In step. I knew some of those steps."),
                new(Sera, "You looked at every face. I watched you do it."),
                new(Mira, "I'm looking for one face. It's a habit. A bad one."),
                new(Sera, "It isn't a bad habit. It's a count. Wardens keep them too."),
            },
        },

        // Questeria: the east gate before Thornbridge, and the palace door with the Sunheart.
        new()
        {
            Id = "city-east-gate",
            Needs = new[] { Companions.FennickPaid },
            Unless = new[] { Companions.EastGate },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Smoke over the wall. Cookfires. Lots of them."),
                new(Sera, "Hundreds. Too many for the ground they're on. I can smell sickness from here."),
                new(SoulBound, "And the gate's shut on them. Course it is."),
                new(You, "Let's see who shut it."),
            },
        },
        new()
        {
            Id = "city-keep-door",
            Needs = new[] { Companions.VaultDone },
            Unless = new[] { Companions.CourtDone },
            Lines = new BanterLine[]
            {
                new(SoulBound, "The Sunspire Keep. The front door this time, with the king's own gem in our pockets."),
                new(You, "Try not to drop it."),
                new(Mira, "I've rehearsed what to say to him all the way up the hill. I'm going to forget every word."),
                new(Sera, "Too many walls. Too many doors. I'll stand by a window."),
                new(Kael, "If anyone asks, I've never seen that gem in my life, and I love the king dearly."),
            },
        },

        // The Old Crown Vault, under the city.
        new()
        {
            Id = "vault-down",
            Needs = new[] { Companions.Summoned },
            Lines = new BanterLine[]
            {
                new(SoulBound, "The Old Crown Vault. Under the whole city. Every step down, the air gets older."),
                new(Mira, "These stairs are three hundred years old. The masons marked every tenth step. Look, there."),
                new(Sera, "Something's been through here, and not long ago. The dust's been moved, and not by feet."),
                new(SoulBound, "Not by feet. Lovely. Keep going."),
            },
        },
        new()
        {
            Id = "vault-traps",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Holes in the walls at knee height. Neat little rows of them."),
                new(Mira, "Dart traps. The old kings didn't trust their own guards."),
                new(SoulBound, "Smart kings."),
                new(Sera, "Step where the dust is thickest. Nothing's been set off there in a long time."),
            },
        },
        new()
        {
            Id = "vault-gold",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Gold. Heaps of it, lying on the floor like straw."),
                new(You, "Don't."),
                new(SoulBound, "I'm looking. Looking's free. Tomas nearly lost a week's catch to a tax that wasn't, and this is just sitting here in the dark."),
                new(Mira, "Royal hoards are rarely spent. They're kept to be counted."),
                new(SoulBound, "Then somebody ought to count it out. To the people who need it."),
                new(Kael, "I've been saying that for years. Nobody listens. They chase me."),
            },
        },
        new()
        {
            Id = "vault-constructs",
            Lines = new BanterLine[]
            {
                new(SoulBound, "Statues. Big ones. With hands like anvils."),
                new(Mira, "Vault constructs. Old crown magic. They sleep until something wakes them."),
                new(SoulBound, "What wakes them?"),
                new(Mira, "Usually people saying 'what wakes them' out loud."),
                new(Sera, "Quieter. Both of you."),
            },
        },

        // Sunspire Keep.
        new()
        {
            Id = "keep-hall",
            Needs = new[] { Companions.VaultDone },
            Lines = new BanterLine[]
            {
                new(SoulBound, "Look at this hall. You could fit the whole cove in here. Boats and all."),
                new(Sera, "Stone trees. Somebody's carved the pillars like an oak wood. Badly."),
                new(Mira, "The oak was the Aurelian sign before the sun was. The first kings went to war under a tree."),
                new(SoulBound, "I'd have kept the tree."),
            },
        },
        new()
        {
            Id = "keep-windows",
            Needs = new[] { Companions.VaultDone },
            Lines = new BanterLine[]
            {
                new(SoulBound, "You can see the whole city from up here. The docks, the market, the Low Ring roofs."),
                new(SoulBound, "Every one of them somebody's. And from up here they all look like toys."),
                new(Mira, "That's the trouble with high windows. Everything below looks small enough to arrange."),
                new(Sera, "And that green smudge at the edge of everything. Mine."),
                new(You, "{sb}? You've gone quiet."),
                new(SoulBound, "Just looking. It's a lovely view."),
            },
        },

        // The Rusty Anvil on the docks, where Brannock drinks in the corner.
        new()
        {
            Id = "anvil-in",
            Needs = new[] { Companions.CourtDone },
            Unless = new[] { Companions.BrannockRecruited },
            Lines = new BanterLine[]
            {
                new(SoulBound, "The Rusty Anvil. Sawdust, low beams, and every eye in the place on us."),
                new("barkeep", "Door. You're letting the daylight in."),
                new(Brannock, "Shut it. Some of us drink in the dark on purpose."),
                new(Sera, "There's a bear in the corner. A drunk one, with braids."),
                new(Mira, "That's him. That has to be him."),
                new(Kael, "I may owe money in here. Don't say my name."),
                new(SoulBound, "Captain Ironvein, I presume. Let's go and ruin his afternoon."),
            },
        },

        // Two companions talking, for wherever both of them are likely to be about. Each needs a
        // trigger placed where both are, or it plays as half a conversation.
        new()
        {
            Id = "party-sera-mira",
            Roaming = true,
            Needs = new[] { Companions.MiraRecruited },
            Lines = new BanterLine[]
            {
                new(Sera, "You count things. Pages, steps, people. I've watched you do it."),
                new(Mira, "Habit. An archivist is just a counter with better handwriting."),
                new(Sera, "And a warden's a counter with worse. Here's one for you. How many times did {sb} look up at the Sunspire today?"),
                new(Mira, "...Nine."),
                new(Sera, "Eleven. You missed two."),
                new(Mira, "I didn't miss them. I hoped I had."),
            },
        },
        new()
        {
            Id = "party-kael-sb",
            Roaming = true,
            Needs = new[] { Companions.KaelRecruited },
            Lines = new BanterLine[]
            {
                new(Kael, "Chief. Question. How do you walk into a room like it already owes you money?"),
                new(SoulBound, "Easy. You decide it does, and then you act surprised when it pays."),
                new(Kael, "I walk into rooms like they're about to throw me out. Through a window, generally."),
                new(SoulBound, "Then walk in with me. Nobody throws you out of a room I'm in."),
                new(Kael, "...See, that. That thing you just did. Do it again some time."),
            },
        },
        new()
        {
            Id = "party-brannock-kael",
            Roaming = true,
            Needs = new[] { Companions.BrannockRecruited, Companions.KaelRecruited },
            Lines = new BanterLine[]
            {
                new(Brannock, "Thief. Your hand's been in my pack twice since the docks."),
                new(Kael, "Three times. You missed one. I tell you as a professional courtesy."),
                new(Brannock, "What were you after?"),
                new(Kael, "Nothing. I wanted to know what a man like you carries. Letters, it turns out. A lot of letters."),
                new(Brannock, "...Stay out of the letters, lad."),
                new(Kael, "I didn't read them. I'm a thief, not a monster."),
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

    /// <summary>The party's own banters, the ones no trigger plays, in the order they're tried.</summary>
    public static IEnumerable<Banter> Roaming => Table.Where(banter => banter.Roaming);

    /// <summary>Whether there's a banter called this.</summary>
    /// <param name="id">What a map's trigger calls it.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);
}
