using System.Collections.Generic;
using LegendQuest.Combat;
using LegendQuest.Tutorial;

namespace LegendQuest.Screens;

/// <summary>
/// The fight's tutorial: the combat tips at the moments they're for, and in the tutorial fight, the
/// thing each one's about picked out on the screen until you've done it.
/// </summary>
/// <remarks>
/// The tips come a lesson at a time on your turns, in the order a turn goes: A FIGHT as it starts
/// (it holds everything until it's read) and the allies, if the fight has any; then on your turn the
/// rows, the Shift while it's unspent, attacking, Focus once a skill that costs it is on the bar,
/// skills from your second turn, the bonus action once the action's spent, and ending the turn; the
/// allies again the first time a support acts; the Soul-Link the first time it charges; Downed the
/// first time one of yours goes down; and the fight's won. Each is once a game (11-combat §6.4: the
/// first fight that uses each idea), and doing what a tip teaches before it comes means it never
/// does. In an encounter marked Tutorial (the wraiths at sea) each lesson's thing pulses gold (the
/// chevrons for the Shift, the Auto and the foes' plates for attacking, a skill's button, END TURN)
/// until you've done it, so the first fight walks you through one turn.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>The lessons asked for in this fight and not yet done, whose things pulse in the tutorial fight.</summary>
    private readonly HashSet<string> _open = new();

    /// <summary>The lessons done in this fight.</summary>
    private readonly HashSet<string> _learned = new();

    /// <summary>The last lesson put in line, so the next waits until it's come up and gone.</summary>
    private string _lessonAsked;

    /// <summary>Whether this is the tutorial fight with tips on, so the lessons' things pulse.</summary>
    private bool _guided;

    /// <summary>How many of your turns have started this fight.</summary>
    private int _yourTurns;

    /// <summary>Whether the last lesson asked for is still to come up, or is up.</summary>
    private bool LessonWaiting => _lessonAsked is string id && (!_tutorials.Seen(id) || _tutorials.Current?.Id == id);

    /// <summary>Starts the lessons for a fight: nothing done yet, the tip about fights first, and the one about allies if it has any.</summary>
    private void BeginLessons()
    {
        _open.Clear();
        _learned.Clear();
        _lessonAsked = null;
        _yourTurns = 0;
        _guided = _encounter.Tutorial && TutorialsOn;
        Lesson(TutorialCatalog.CombatStart);
        if (_encounter.Guests.Count > 0 || _encounter.Supports.Count > 0) Lesson(TutorialCatalog.CombatAllies);
    }

    /// <summary>Puts a combat tip in line, if it's not been seen, and marks its lesson open.</summary>
    /// <param name="tipId">The tip, by its id in TutorialCatalog.</param>
    /// <returns>True if it was put in line.</returns>
    private bool Lesson(string tipId)
    {
        if (!ShowTutorial(tipId)) return false;
        _lessonAsked = tipId;
        _open.Add(tipId);
        return true;
    }

    /// <summary>Notes a turn starting, to count yours.</summary>
    /// <param name="actor">Whose turn it is.</param>
    private void LessonTurnStarted(Combatant actor)
    {
        if (actor.Side == Side.Party && actor.Member is not null && !actor.Guest) _yourTurns++;
    }

    /// <summary>Puts in line the lesson your turn's at, if the last one's come and gone: the rows, the Shift while it's there to use, attacking while the action's there, Focus and skills from your second turn, the bonus action once the action's spent, and ending the turn once there's nothing much left.</summary>
    private void GuideTurn()
    {
        if (!TutorialsOn || LessonWaiting) return;

        Combatant me = _combat.Current;
        TurnBudget budget = me.Budget;

        if (budget.Action && Lesson(TutorialCatalog.CombatRows)) return;
        if (budget.Action && budget.Shift && _combat.Shifts(me).Count > 0 && Lesson(TutorialCatalog.CombatShift)) return;
        if (budget.Action && Lesson(TutorialCatalog.CombatAttack)) return;
        if (budget.Action && _yourTurns >= 2 && HasFocusSkill(me) && Lesson(TutorialCatalog.CombatFocus)) return;
        if (budget.Action && _yourTurns >= 2 && HasOwn(me) && Lesson(TutorialCatalog.CombatAbility)) return;
        if (!budget.Action && budget.BonusAction && HasBonus(me) && Lesson(TutorialCatalog.CombatBonus)) return;
        if (!budget.Action) Lesson(TutorialCatalog.CombatEndTurn);
    }

    /// <summary>Says a lesson's been done: it's closed, and its tip goes if it's up and never comes if it isn't.</summary>
    /// <param name="tipId">The tip, by its id in TutorialCatalog.</param>
    private void Learn(string tipId)
    {
        _learned.Add(tipId);
        _open.Remove(tipId);
        TutorialDone(tipId);
    }

    /// <summary>Says which lessons using an ability has done: attacking, the Soul-Link, a skill, Focus, a bonus action.</summary>
    /// <param name="ability">What was used.</param>
    private void LearnFrom(AbilityDefinition ability)
    {
        if (ability.Bar == BarSlot.Auto || ability.Id == AbilityId.Attack) Learn(TutorialCatalog.CombatAttack);
        else if (ability.Id == AbilityId.SoulLinkStrike) Learn(TutorialCatalog.CombatSoulLink);
        else if (IsSkill(ability)) Learn(TutorialCatalog.CombatAbility);
        if (ability.Focus > 0) Learn(TutorialCatalog.CombatFocus);
        if (ability.Cost == ActionCost.BonusAction) Learn(TutorialCatalog.CombatBonus);
    }

    /// <summary>Whether a lesson's open and its thing should pulse.</summary>
    private bool Guided(string tipId) => _guided && _open.Contains(tipId);

    /// <summary>Whether a hotbar button pulses: the Auto for attacking, a skill for skills, one that costs Focus for Focus, a bonus action's for the bonus action, the Soul-Link Strike for the Soul-Link.</summary>
    /// <param name="ability">The button's ability.</param>
    private bool Guiding(AbilityDefinition ability) =>
        Guided(TutorialCatalog.CombatAttack) && ability.Bar == BarSlot.Auto
        || Guided(TutorialCatalog.CombatAbility) && IsSkill(ability)
        || Guided(TutorialCatalog.CombatFocus) && ability.Focus > 0
        || Guided(TutorialCatalog.CombatBonus) && ability.Cost == ActionCost.BonusAction
        || Guided(TutorialCatalog.CombatSoulLink) && ability.Id == AbilityId.SoulLinkStrike;

    /// <summary>Whether END TURN pulses.</summary>
    private bool GuidingEndTurn => Guided(TutorialCatalog.CombatEndTurn);

    /// <summary>Whether the chevrons over the slots you can Shift to pulse.</summary>
    private bool GuidingMove => Guided(TutorialCatalog.CombatShift);

    /// <summary>Whether the foes' plates pulse, for the lesson on attacking.</summary>
    private bool GuidingAttack => Guided(TutorialCatalog.CombatAttack);

    /// <summary>Whether an ability's one of the class's skills or its ultimate.</summary>
    private static bool IsSkill(AbilityDefinition ability) => ability.Bar is BarSlot.Skill or BarSlot.Ultimate;

    /// <summary>Whether someone has a usable skill on the hotbar.</summary>
    private bool HasOwn(Combatant me)
    {
        foreach (AbilityDefinition ability in _combat.Abilities(me))
            if (IsSkill(ability) && _combat.Unusable(me, ability) is null) return true;
        return false;
    }

    /// <summary>Whether someone has anything on the hotbar that costs Focus.</summary>
    private bool HasFocusSkill(Combatant me)
    {
        foreach (AbilityDefinition ability in _combat.Abilities(me))
            if (ability.Focus > 0) return true;
        return false;
    }

    /// <summary>Whether someone has a usable bonus action on the hotbar.</summary>
    private bool HasBonus(Combatant me)
    {
        foreach (AbilityDefinition ability in _combat.Abilities(me))
            if (ability.Cost == ActionCost.BonusAction && _combat.Unusable(me, ability) is null) return true;
        return false;
    }
}
