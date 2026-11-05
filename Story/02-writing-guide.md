# Writing guide

These are the rules every line in Legend Quest follows, including lines written after this. `COMPANIONS.md` covers who the six companions are and what they want. This file covers how everyone talks, and the habits that make game dialogue read as machine-written. If a line breaks a rule here, rewrite it. Don't argue with the rule unless the line is genuinely better for breaking it, and if it is, note why in a comment next to it.

`story/tools/lint_dialogue.py` (Part 6 of the build plan) catches the mechanical rules. The rest is judgement.

---

## 1. The rules

**One job per line.** A line tells you something, shows who someone is, or moves the scene along. It can do two of those. It never does none.

**Say what you want, not what you feel.** People in this game ask for things, refuse things, change the subject, make jokes and give orders. They almost never describe their own emotions. "I'm scared" is weaker than "Can we not stand next to it?"

**Nobody names the theme.** The game is about what you do when nobody with power comes. No one ever says that. Nobody says "power corrupts", "we're not so different", "the real enemy", or "I'm doing this for the greater good". The Voice can say {sb}'s thoughts back to them; that's its job, and it's still specific ("How many more Aldrics?"), never abstract.

**Answer sideways.** A reply doesn't repeat the question. "Are you all right?" / "Give me a minute." Not "Yes, I'm all right."

**Specific beats general.** Herring, not fish. Tallow Lane, not the slums. "Two woodcutters not home", not "people missing". A name, a number or a thing in every second or third line.

**Short.** One or two sentences on screen. A line over 170 characters is split into two nodes. A speech over four nodes from one speaker gets interrupted by someone, or by an action.

**Humour comes from people, not quips.** Kael prices things. Sera understates. Brannock takes things literally. {sb} teases you. Mira jokes at her own expense. Nobody does a generic one-liner that any of them could say.

**Earn the big lines.** A line that sounds like a trailer ("Never leave the other behind") can appear only as a deliberate callback (section 3). Everything else is plain speech.

**Villains are brief.** No antagonist speaks more than three nodes in a row. Maleficio asks questions. Severin grieves. Crane flatters. Vorlaith hardly speaks at all.

**Write every variant in full.** If a flag changes a line, write the other line too. No "[reference the crystal choice here]".

## 2. Habits that read as machine-written

These are the tells. `lint_dialogue.py` flags most of them.

| Don't | Because | Instead |
| --- | --- | --- |
| "It's not X, it's Y." / "Not X. Y." | The single most recognisable machine pattern | Say Y |
| Three short fragments in a row ("Cold. Tired. Alone.") | Same | One plain sentence. Allowed once per act, for a real peak |
| An ellipsis at the start of a line ("...Yeah.") | Overused to fake hesitation | Start the line. At most one ellipsis per scene, anywhere |
| Stage directions "a beat", "after a pause", "something shifts in", "for a long moment" | Filler | Cut it, or show it in the narration once |
| Em dash for drama ("I'm fine — really") | Same | Comma or full stop. Em dash only for an interruption: the line ends in "—" and the next speaker cuts in |
| Exclamation marks in conversation | Shouting on paper | Only in fights, barks and real shouts |
| Therapy words: "valid", "process", "space", "boundaries", "I hear you", "journey", "trauma" | Wrong century | People in Fable say what happened |
| Stock phrases: "testament to", "weight of", "the air thick with", "palpable", "a dance of", "whispers of", "tapestry", "echoes of" (unless an actual echo), "unspoken", "in that moment" | Generic | Name the thing |
| "We've got this", "You're not alone", "I'm proud of you", "Let that sink in", "Here's the thing" | Modern pep talk | (Leon's dying line uses "proud" on purpose. Nobody else does) |
| A character explaining their own arc: "I realise now that I was afraid of..." | Nobody talks like that | Show the change by a choice they make |
| Narration that tells you what someone feels ("{sb} lowers the blade, conflicted") | Telling | "{sb} lowers the blade." |
| Every character speaking in the same rhythm | Voice collapse | Check the voice sheet. Cover the names: you should still know who's talking |

## 3. Callbacks

Some lines come back on purpose. A callback is marked `[CALLBACK name]` in the script so it isn't "fixed" by a later edit. No phrase appears more than four times in the game.

| Name | Line | Where |
| --- | --- | --- |
| `oath` | "Never leave the other behind." | P-03, A1-C5-02, A3-C1-04, A3-C5-04 |
| `sleep` | "Can't sleep either?" | P-04, A3-C3-07 |
| `mud` | "...where nobody kneels in the mud." | P-04, A3-C1-04, A3-C2-04 |
| `lovely-kings` | "Good men make lovely kings in peacetime." | A1-C4-04 (Crane), A1-C6-05 (the Voice), A2-C6-07 (the Voice, turned) |
| `rudely` | "I'd rather be told the truth rudely than lies politely." | A1-C4-03 (Leon), A3-C1-03 ({sb}, mocking), A3-C4-01 (Elara) |
| `herring` | The carved herring | P-04 (given), A3 birthday parcel, A3-C2-04 (left on the table), A3-C5-04 |
| `proud` | "Tell Elara I was proud of her." | A3-C2-03 (Leon, dying), A3-C2-05 (you, passing it on). The only two uses of "proud" in the game |
| `small` | "small", meaning powerless | {sb} only, once per chapter at most, and the Voice. Nobody else uses the word that way |

## 4. Pronouns and titles

{sb}'s pronouns are the player's choice. Never write a verb that has to agree with `{sb_they}`: "{sb_they} is", "{sb_they}'s" and "{sb_they} was" all break for one of the three. Use the name, a modal ("{sb_they}'ll", "{sb_they} can") or rewrite. `{sb_them}` and `{sb_their}` are always safe.

The player's own lines never mention {sb}'s pronouns. Use the name.

Fable's knights are all "Sir", whatever their pronouns. It's an old title and nobody in the kingdom thinks twice about it, so "Sir {sb}" is always right.

## 5. Your lines (the player)

- Short: twelve words or fewer, nearly always.
- No feelings, no names for yourself, no speeches.
- The four kinds of option are usually: the kind answer, the hard answer, the joke, and saying nothing. They don't always all appear, and they're never labelled.
- Voice-locked options are marked `[voice:witty]` and so on. **Honorable** is plain and direct, and owns mistakes. **Pragmatic** is short and transactional, and asks about cost. **Witty** deflects with a joke and lands one true thing in it.
- "(Say nothing.)" is a real choice and often the strongest one.

## 6. The narrator

Present tense. Plain. One clause of physical action, rarely two. Never interprets. Never uses "seems". Never more than two narrator lines in a row. The narrator is used for what the camera can't show clearly in a top-down game, and for nothing else.

## 7. Voice sheets

The companions' sheets are in `COMPANIONS.md` (Voice, Never says, Sample lines). These are short reminders, plus everyone else.

### Companions (summary of COMPANIONS.md)

| Who | Rhythm | Notices | Signature habits | Never |
| --- | --- | --- | --- | --- |
| **{sb}** | Quick, warm, sentences that run on when excited | Who owns things | Teases you; cove words (tide, nets, gutting, the landing); "we" in Act 1, "I" in Act 2, "them" in Act 3. When lying, adds one detail too many | "peasant", "obey", "my people" (before Act 2's end), cruelty for fun |
| **Sera** | Flat statements, few words | Animals, weather, tracks | Counts; understates; swears by weather ("Rot and rain") | "I feel", "magical", "destiny", gods |
| **Mira** | Complete, careful sentences; long when nervous, short when angry | Dates, sources, who wrote what | Corrects gently; jokes at herself; calls Maleficio "my teacher" | Crude words, "monster" for a person, "I'm sure" when she isn't |
| **Kael** | Fast, talks through silences, undercuts himself | Prices, exits, pockets | Calls {sb} "chief"; nicknames; when serious, very short sentences | "I need", "help me", "home" (until Act 3) |
| **Brannock** | Short, gruff, military | Chokepoints, sightlines, who's tired | Calls {sb} "youngling" and you "little one"; counts the dead; "Stone and anvil" | "glory", promises (Act 1), "orders" as an excuse |
| **Valencia** | Dry and elegant; a long honest sentence she regrets | Hands, light, who's lying | Convent politeness with an edge; calls the sun "the old enemy"; drops contractions when angry | "monster" about herself, "hunger" |

### The court and the city

- **King Leon.** Slow, courteous, plain. Thanks people and means it. Admits mistakes in full sentences. Tired, and lets it show only to Elara. Uses first names for old friends (Vesper, Brannock, Aldric). *"I fought the last war. I know how wars start."*
- **Princess Elara.** Quick, interrupts, decides out loud. Funny when she forgets to be careful. Swears in private ("Hells"). Says "thank you" to guards, never "please" to courtiers. *"I've got guards. I don't know which of them took it."*
- **Lord Chancellor Vesper Crane.** Long, smooth sentences; compliments that cost you something. Never threatens directly. Calls everyone by their title, a little too often. *"Good men make lovely kings in peacetime."*
- **Captain Roderic Hale.** A working man, tired and decent. Talks about shifts, men and boots. Dislikes orders he can't explain. *"Mine come from people who don't explain them."*
- **Archmage Thessaly Varn.** Clipped, procedural, numbers and rules. No pleasantries. Dry humour so dry it's easy to miss. *"I've buried three apprentices who thought the rule was for other people."*
- **Old Tomas.** Fifty years a fisherman. Talks in weather and tides. Grateful, and embarrassed by it.
- **Questmaster Fennick.** A clerk who loves his board. Pedantic, kind, slightly too pleased with his columns.
- **Sergeant Brisk.** Not a bad man; the shape of the job. Repeats what he's been told because he can't afford to think about it.
- **Captain Odelle Marsh.** Salt and cheek. Treats passengers as cargo with opinions.
- **Lord Harrow of Heronsgate.** Bored, polite, never looks at the person he's ruining. Talks about the weather.

### Doomward

- **Maleficio.** A teacher. Asks questions, never raises his voice, uses "child" for Mira. Reasonable to the end. Never gloats; he's disappointed in you instead. *"Is that so terrible?"*
- **Varkhul.** Fragments, as if remembering each word. Present tense for the past. Courtesy from a man who used to have it.
- **Brenna of Ashfall.** Few words, hard ones. Doomward plainness: no titles, no thanks. Softens only about her father, and only once.
- **Commander Aldric Voss.** Old soldier, gallows humour, fond of Brannock. Says "lad" to everyone, including the king.

### Morvaine and the Order

- **Lord Severin Morvaine.** Slow, elegant, grieving. Speaks about the dead in the present tense. Calls Valencia "my dawn". Polite to his enemies because rudeness would mean they matter.
- **Lady Isadora** (memory). Warm, practical, a mother giving instructions while she still can.
- **Sir Corwin Halbeck.** Liturgy and certainty. Quotes his Order's rule. When certainty breaks, he goes very quiet and very plain.
- **Mother Superior Anselle.** Kind, knows more than she says, answers questions with questions.
- **Sister Orla.** The abbey's beekeeper. Practical, unembarrassed, talks about bees.
- **Thrall-Knights (Vesk, Vreya, Mordain).** Showy, mocking, enjoy their master's favour. Each has one verbal habit: Vesk is formal, Vreya sing-song, Mordain whispers.

### The things that aren't people

- **The Voice.** Always in the listener's own voice, and in their own words: it borrows phrases the listener has said earlier in the game. Second person. Never more than two sentences. It agrees; it never orders. In the script: `VOICE(sb):` means the Voice speaking to {sb}. On screen it uses the listener's portrait, tinted violet, with no name.
- **Vorlaith, the Unmade King.** Speaks four times in Act 3. Calm, ancient, faintly amused. Uses other people's words back at them. Never explains how anything works.

## 8. Before and after

These are taken from the old draft, to show the rules applied.

| Before | After | Why |
| --- | --- | --- |
| "Every crown is built on someone's bones. I used to think Fable was different." | "They light the festival lanterns in that same square, don't they?" | Says the theme; the after asks a question and makes the player think it |
| "...Yeah. Of course." [a pause too long] | "No. Course not." [not looking at you] | Leading ellipsis; banned direction |
| "Rude. Accurate, but rude." | "I'll remember that next time you're hungry." | A stock joke shape; the after is {sb}'s teasing |
| "He never once thought he was the villain." | "Every step made sense when he took it. That's the part I can't get past." | A movie line; the after is specific and Mira's |
| "He may kneel or he may burn. I am, as ever, flexible." | "He can kneel or he can burn. I'm not particular." | "As ever, flexible" is a quip; the after is drier and colder |
| "Your friend has a fire in them." | "{sb} runs hot. I knew someone else who ran hot like that." | Pronoun safety, and it points at Maleficio without naming him |
| "Thank you, little sovereign. You held the door… so… well." | "You held the door so well." | Ellipses for menace; the plain line is scarier |
