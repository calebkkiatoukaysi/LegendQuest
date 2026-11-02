namespace LegendQuest;

/// <summary>
/// Every ability in a fight: the actions anyone can take, each class's level-1 kit, and what the
/// enemies can do. AbilityCatalog has what each one does.
/// </summary>
/// <remarks>
/// In hotbar order: the common actions first, then each class's kit in ClassId's order, then the
/// enemies' own. Content/Combat's ability icons are laid out in this order too, so new ones go on
/// the end. The names are the SRD 5.1's, or mine where it hasn't one.
/// </remarks>
public enum AbilityId
{
    // Anyone.

    /// <summary>One attack with the weapon in the main hand, or a foe's attacks.</summary>
    Attack,

    /// <summary>A bonus-action attack with a light weapon in the off hand, after attacking with a light one in the main hand.</summary>
    OffHandAttack,

    /// <summary>Spends the action on as much movement again.</summary>
    Dash,

    /// <summary>Moving doesn't provoke opportunity attacks for the rest of the turn.</summary>
    Disengage,

    /// <summary>Attacks against you have disadvantage until your next turn.</summary>
    Dodge,

    /// <summary>Gets a Downed ally back up with 1 HP, or gives an ally advantage on their next attack.</summary>
    Help,

    /// <summary>A healing potion out of the party's bag, as a bonus action.</summary>
    DrinkPotion,

    /// <summary>A flask of alchemist's fire out of the party's bag, thrown.</summary>
    ThrowAlchemistsFire,

    /// <summary>You and the Soul-Bound strike one foe together: a sure hit with both weapons and radiant light.</summary>
    SoulLinkStrike,

    // Fighter.

    /// <summary>A bonus-action breather that heals 1d10 + level, once a short rest.</summary>
    SecondWind,

    /// <summary>The Defence fighting style: +1 AC while wearing armour.</summary>
    Defence,

    // Rogue.

    /// <summary>An extra 1d6 once a turn with a finesse or ranged weapon, with advantage or an ally beside the target.</summary>
    SneakAttack,

    /// <summary>Cunning Action: Dash as a bonus action.</summary>
    CunningDash,

    /// <summary>Cunning Action: Disengage as a bonus action.</summary>
    CunningDisengage,

    // Cleric.

    /// <summary>Radiant fire on a foe who fails a Dexterity save. A cantrip.</summary>
    SacredFlame,

    /// <summary>A bonus-action heal at range.</summary>
    HealingWord,

    /// <summary>A bigger heal, by touch.</summary>
    CureWounds,

    // Wizard.

    /// <summary>A mote of fire thrown at a foe. A cantrip.</summary>
    FireBolt,

    /// <summary>A beam of cold that slows its target. A cantrip.</summary>
    RayOfFrost,

    /// <summary>Three darts of force that never miss.</summary>
    MagicMissile,

    // Paladin.

    /// <summary>A pool of healing by touch, spent a hit point at a time.</summary>
    LayOnHands,

    /// <summary>The Protection fighting style: the reaction that turns an attack on someone beside you to disadvantage, with a shield up.</summary>
    Protection,

    // Druid (and Healing Word).

    /// <summary>A flame in the palm, thrown. A cantrip.</summary>
    ProduceFlame,

    /// <summary>Grasping weeds and vines that restrain everyone in a patch of ground.</summary>
    Entangle,

    // Warlock.

    /// <summary>A crackling beam of force. A cantrip.</summary>
    EldritchBlast,

    /// <summary>A curse: an extra 1d6 necrotic on every hit against its target.</summary>
    Hex,

    // Bard (and Healing Word).

    /// <summary>An insult laced with magic: psychic damage and disadvantage on the target's next attack. A cantrip.</summary>
    ViciousMockery,

    /// <summary>A d6 given to an ally to add to an attack or a save.</summary>
    BardicInspiration,

    // Foes.

    /// <summary>The Attack action makes every one of the foe's attacks, not just one.</summary>
    Multiattack,

    /// <summary>Advantage on an attack when another of the foe's side is beside the target.</summary>
    PackTactics,

    /// <summary>A melee hit knocks the target Prone unless they make a Strength save.</summary>
    Trip,

    /// <summary>A bonus-action cloud of smoke: Dodging and Disengaged until the foe's next turn.</summary>
    SmokeBomb,

    /// <summary>A lit charge of blasting powder thrown into a crowd: fire on everyone near where it lands.</summary>
    BlastingCharge,

    /// <summary>Thorns burst out of the Thornmother's hide into everyone beside her.</summary>
    ThornBurst,

    // Anyone, again: the hotbar puts it beside Drink Potion.

    /// <summary>An elixir of focus out of the party's bag, as a bonus action: a spent spell slot back.</summary>
    DrinkElixir,
}
