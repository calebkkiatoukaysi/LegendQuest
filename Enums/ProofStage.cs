namespace LegendQuest;

/// <summary>
/// Which part of the proof run is playing. It goes through the screens in the same order a player would.
/// </summary>
/// <remarks>Each from Voyage on is one visit to a zone, which ends when the party leaves it.</remarks>
public enum ProofStage
{
    /// <summary>The title, the options and the save slots.</summary>
    Menus,

    /// <summary>A new game: the Prologue's card, you made, your voice, the Soul-Bound made, the memory and the oath.</summary>
    Creation,

    /// <summary>The deck of the Gull's Wager at night: the walk to the bow, the Prologue's voyage, the sea-wraiths, and Questeria at dawn.</summary>
    Voyage,

    /// <summary>The harbourside: the ship sailing in, the walk off it, Act 1's card, the Red Gulls and Captain Hale, then through the fish market up to the city.</summary>
    Docks,

    /// <summary>Questeria room by room: the gate plaza, the festival square (the fountain, the elder, the city gate, Alt, the stair, the waypoint), and the Lantern Market (Fennick's board, the badges, the zone map), then into the Gilded Flagon.</summary>
    Questeria,

    /// <summary>Inside the Gilded Flagon: the innkeeper's errand, and out through the door.</summary>
    Flagon,

    /// <summary>Outside the Flagon's door again, then across to the festival square and out through the city gate.</summary>
    QuesteriaOut,

    /// <summary>The old road: the farmland's trigger, then at the fork the chest, the sign, the zone map, the waypoint, and fast travel back to Questeria.</summary>
    Road,

    /// <summary>Back in Questeria off the waypoint, and straight back to the road by it.</summary>
    QuesteriaAgain,

    /// <summary>On the road again by its waypoint, past where the woodcutters sawed through the fallen tree, and east into the Mirewood.</summary>
    RoadAgain,

    /// <summary>Chapter 2 begins: the Mirewood's edge, the gloomfangs, Sera, and on to the quarry's mouth.</summary>
    Mirewood,

    /// <summary>The Old Quarry: the roots Sera parts, the courier and his letter, the Thornmother, and the crystal.</summary>
    Quarry,

    /// <summary>The night camp in the Mirewood, a word with Sera, and fast travel home from the camp's stone.</summary>
    MirewoodCamp,

    /// <summary>Fennick pays out, then east out of the city to Thornbridge.</summary>
    QuesteriaPaid,

    /// <summary>Chapter 3: the east gate, the camp, Mira and her errands, the Hollowed raid, and back into the city.</summary>
    Thornbridge,

    /// <summary>The royal messenger outside the Gilded Flagon, then in.</summary>
    QuesteriaMessenger,

    /// <summary>Chapter 4: Elara in her booth, and out again.</summary>
    FlagonElara,

    /// <summary>Up to the Sunspire Steps, and down the stair under the terrace into the vault.</summary>
    QuesteriaVault,

    /// <summary>The Old Crown Vault: the plates in the halls, Kael on his shelf in the strongroom, the sappers, and back up the stair.</summary>
    Vault,

    /// <summary>In through the keep's door on the Sunspire Steps.</summary>
    QuesteriaKeep,

    /// <summary>Sunspire Keep: the audience with King Leon, Crane in the great hall, and out.</summary>
    Keep,

    /// <summary>Back down through the city and out of the gate plaza to the harbour.</summary>
    QuesteriaDocks,

    /// <summary>The fish market, along to the harbourside, and in at the Rusty Anvil's door.</summary>
    DocksAnvil,

    /// <summary>The Rusty Anvil: Brannock at his corner table.</summary>
    Anvil,

    /// <summary>The Gilded Flagon with Kael and Brannock waiting at its tables: a word with one, a swap, and off to the road.</summary>
    FlagonCamp,

    /// <summary>The road once more, and in at the barrow's gate at the fork.</summary>
    RoadToBarrow,

    /// <summary>The barrow: the dark steps, the cold, the hall's locked door, the lever and its door, the side room's chest, the zone map, a torch, then out through the pause menu.</summary>
    Barrow,

    /// <summary>Back on the title after QUIT TO TITLE, and the slots with the autosave in them.</summary>
    TitleAgain,

    /// <summary>Writing the audio log and quitting.</summary>
    Done,
}
