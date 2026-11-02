namespace LegendQuest;

/// <summary>
/// Every sound effect. The file is the name in kebab case under Content/Sfx, so MenuMove is
/// Sfx/menu-move.
/// </summary>
/// <remarks>Grouped by where you hear them. If you rename one, rename its file and its line in Content.mgcb too.</remarks>
public enum Sfx
{
    // Menus.

    /// <summary>Moving the cursor to another plate.</summary>
    MenuMove,

    /// <summary>Pressing a plate.</summary>
    MenuConfirm,

    /// <summary>Backing out of a menu.</summary>
    MenuBack,

    /// <summary>Changing a value with left or right (the options).</summary>
    OptionChange,

    /// <summary>Opening the pause menu.</summary>
    Pause,

    // Walking.

    /// <summary>One step on something hard. The pitch and pan change every step.</summary>
    FootstepStone,

    /// <summary>One step on grass or dirt.</summary>
    FootstepGrass,

    // Using things.

    /// <summary>A door swinging open.</summary>
    Door,

    /// <summary>Trying a door that won't open.</summary>
    DoorLocked,

    /// <summary>A chest's lid coming up.</summary>
    Chest,

    /// <summary>A lever going over.</summary>
    Lever,

    /// <summary>Picking something up: coins, for now.</summary>
    Pickup,

    /// <summary>A waypoint stone waking up the first time it's found.</summary>
    Waypoint,

    // Conversations.

    /// <summary>The d20 thrown across the panel.</summary>
    DiceRoll,

    /// <summary>A check passing.</summary>
    CheckPass,

    /// <summary>A check failing.</summary>
    CheckFail,

    /// <summary>A companion approving of what you said.</summary>
    Approve,

    /// <summary>A companion disapproving.</summary>
    Disapprove,

    // The story.

    /// <summary>One heartbeat, then two in rhythm: the Prologue's title card.</summary>
    Heartbeat,

    /// <summary>A ship's bell rung in alarm.</summary>
    ShipBell,

    /// <summary>A gloomfang howling in the trees, and another answering it.</summary>
    WolfHowl,

    /// <summary>The black crystal's hum: deep, wrong, and throbbing three times.</summary>
    CrystalPulse,

    /// <summary>War horns a long way off: the Hollowed coming.</summary>
    WarHorn,

    /// <summary>A bow loosed, and the arrow hissing away.</summary>
    Bowshot,

    /// <summary>A coin flipped and caught, or pay counted into a hand.</summary>
    Coin,

    // Fighting.

    /// <summary>A blade landing.</summary>
    SwordHit,

    /// <summary>A blow that misses: a whoosh past and nothing else.</summary>
    Miss,

    /// <summary>A critical hit: the blow harder, a boom under it and Steel's call flaring on chimes.</summary>
    Crit,

    /// <summary>A blunt blow landing: a mace, a hammer, a fist.</summary>
    Thud,

    /// <summary>A blow taken on a shield.</summary>
    Shield,

    /// <summary>A bow loosed in a fight. Shorter and tighter than Bowshot, which is for the story.</summary>
    Bow,

    /// <summary>A spell let go.</summary>
    Cast,

    /// <summary>Fire catching and roaring: a fire bolt landing, or alchemist's fire.</summary>
    Fire,

    /// <summary>Frost spreading and setting: a ray of frost landing.</summary>
    Frost,

    /// <summary>Someone healed.</summary>
    Heal,

    /// <summary>A potion drunk.</summary>
    Potion,

    /// <summary>The Soul-Link: two heartbeats falling into step and two voices meeting, as the duo strike together.</summary>
    SoulLink,

    /// <summary>Your turn starting: Steel's call run up quickly on plucked strings.</summary>
    TurnStart,

    /// <summary>Someone in the party going down, and one heartbeat after: down, not dead.</summary>
    Downed,

    /// <summary>Something dying.</summary>
    Death,

    /// <summary>The fight won: Steel's call on the trumpet over the horns.</summary>
    Victory,

    /// <summary>The party fallen: the call turned upside down, a choir and a great bell.</summary>
    GameOver,

    // Gear and shops.

    /// <summary>Putting gear on: leather, a buckle, a little mail.</summary>
    Equip,

    /// <summary>Buying something: a purse set down and a coin rung on the counter.</summary>
    Buy,
}
