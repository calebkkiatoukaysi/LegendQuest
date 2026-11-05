namespace LegendQuest;

/// <summary>
/// Something that happened in a fight: what CombatEngine hands back for the screen to animate, sound
/// and write in the log, in the order it happened.
/// </summary>
/// <remarks>New ones go on the end, after Log.</remarks>
public enum CombatEventKind
{
    /// <summary>Someone rolled initiative. The amount is their total.</summary>
    InitiativeRolled,

    /// <summary>Someone's turn started.</summary>
    TurnStarted,

    /// <summary>Someone Shifted, or was pulled or pushed, from one slot to another, From to To.</summary>
    Shifted,

    /// <summary>Someone left the front row and a foe in theirs took its swing. The attack follows.</summary>
    OpportunityAttack,

    /// <summary>An attack roll. Its text is the whole line, damage and all; the roll is the d20s.</summary>
    AttackRolled,

    /// <summary>The attack hit.</summary>
    Hit,

    /// <summary>The attack missed.</summary>
    Missed,

    /// <summary>The attack was a natural 20 (or a melee hit on someone Downed): the dice doubled.</summary>
    Critical,

    /// <summary>Someone lost hit points. The amount is how many.</summary>
    Damaged,

    /// <summary>Someone got hit points back. The amount is how many.</summary>
    Healed,

    /// <summary>A saving throw. Its text is the whole line; the roll is the d20s.</summary>
    SaveRolled,

    /// <summary>A condition went on someone. The amount is which, as a ConditionId's number.</summary>
    ConditionAdded,

    /// <summary>A condition came off someone. The amount is which, as a ConditionId's number.</summary>
    ConditionRemoved,

    /// <summary>A party member dropped to 0 hit points.</summary>
    Downed,

    /// <summary>A Downed party member rolled a death save. The amount is the d20.</summary>
    DeathSave,

    /// <summary>A Downed party member stopped dying.</summary>
    Stabilised,

    /// <summary>A Downed party member got back up.</summary>
    Revived,

    /// <summary>Someone died: a foe at 0 hit points, or a party member who failed three death saves.</summary>
    Died,

    /// <summary>The Soul-Link meter gained a charge. The amount is the charge now.</summary>
    SoulLinkCharged,

    /// <summary>The Soul-Link Strike was spent. The strike's own attack line follows.</summary>
    SoulLinkStrike,

    /// <summary>A new round started. The amount is its number.</summary>
    RoundStarted,

    /// <summary>The fight's won. The amount is the XP each party member got.</summary>
    Victory,

    /// <summary>The fight's lost.</summary>
    Defeat,

    /// <summary>A line for the log with nothing to animate.</summary>
    Log,

    /// <summary>Someone used an ability: the cue for the wind-up or the cast. What it did follows.</summary>
    AbilityUsed,

    /// <summary>Someone joined the fight partway through: a wave of foes arriving. They roll initiative next.</summary>
    Joined,

    /// <summary>Someone said something mid-fight, in a bubble. The text is the line, which may hold the same tokens as a conversation's.</summary>
    Bark,

    /// <summary>Someone's Focus went up or down. The amount is the change.</summary>
    FocusChanged,

    /// <summary>A support used its ability at the end of the round. The actor is the support's id, not a Combatant's; what it did follows.</summary>
    SupportActed,

    /// <summary>A guest at 0 hit points fell back out of the fight, unhurt for the story.</summary>
    FellBack,

    /// <summary>A round's last turn ended, before the supports act. The amount is its number.</summary>
    RoundEnded,

    /// <summary>The fight was given to the party ("Win this fight"). The victory follows.</summary>
    Conceded,

    /// <summary>The fight's paused for a conversation in the middle of it. The text is the conversation's id.</summary>
    Interlude,
}
