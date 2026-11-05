using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Items;
using LegendQuest.StateManagement;
using LegendQuest.Tutorial;

namespace LegendQuest.Screens;

/// <summary>
/// Your turn in a fight: what the mouse is over, what a click would do there, and doing it, by the
/// mouse or the keys.
/// </summary>
/// <remarks>
/// <para>
/// With nothing picked, a click on a foe (or its plate) is your Auto on it, on a friend who's Downed
/// helps them up, and on a friend's slot or an empty one of yours that you can Shift to, Shifts
/// there. 1 to 6 pick the primary buttons (Auto, the three skills, the ultimate, the potion) and 7, 8,
/// 9 and 0 the shared strip; one used on yourself happens at once, any other waits for a target from
/// its Targets, which the mouse picks, or Tab goes through and Enter uses. Clicking the sigil picks
/// the Soul-Link Strike. Right click or Escape puts a pick back. Space, End or END TURN ends the turn,
/// and WASD or the arrows Shift a slot (W back, S front, A and D along the row). What a click would do
/// is worked out as a TurnPlan before it's made, so the plates, the rings and the card show exactly
/// that.
/// </para>
/// <para>
/// The screen's pieces (the hotbar, the turn order, the budget, the sigil, the frame) are in front of
/// the room, so the mouse finds them first. L shows and hides the log, the wheel scrolls it when the
/// mouse is on it, and Escape with nothing picked is the pause menu. While it isn't your turn,
/// pointing still works and says what everything is; only doing is held back.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long the line saying why something can't be done stays up.</summary>
    private const float RefusalSeconds = 1.8f;

    /// <summary>How many primary buttons there are: Auto, three skills, the ultimate and the potion.</summary>
    private const int PrimaryButtons = 6;

    /// <summary>How many skills the primary buttons have room for.</summary>
    private const int SkillButtons = 3;

    /// <summary>How many buttons the shared strip has room for.</summary>
    private const int StripButtons = 4;

    /// <summary>The hotbar's keys, left to right: the six primary buttons, then the strip.</summary>
    private static readonly Keys[] HotbarKeys =
    {
        Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0,
    };

    /// <summary>What each key's written as on its button.</summary>
    private static readonly string[] HotbarKeyNames = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

    private readonly InputAction _toggleLog = new(Array.Empty<Buttons>(), new[] { Keys.L }, true);
    private readonly InputAction _endTurn = new(new[] { Buttons.Y }, new[] { Keys.Space, Keys.End }, true);
    private readonly InputAction _nextTarget = new(new[] { Buttons.RightShoulder }, new[] { Keys.Tab }, true);
    private readonly InputAction _confirm = new(new[] { Buttons.A }, new[] { Keys.Enter }, true);
    private readonly InputAction _cursorUp = new(new[] { Buttons.DPadUp }, new[] { Keys.W, Keys.Up }, true);
    private readonly InputAction _cursorDown = new(new[] { Buttons.DPadDown }, new[] { Keys.S, Keys.Down }, true);
    private readonly InputAction _cursorLeft = new(new[] { Buttons.DPadLeft }, new[] { Keys.A, Keys.Left }, true);
    private readonly InputAction _cursorRight = new(new[] { Buttons.DPadRight }, new[] { Keys.D, Keys.Right }, true);

    /// <summary>The hotbar's buttons for whoever's turn it is (or whoever of yours went last), rebuilt each frame: the six primary ones, then the strip.</summary>
    private readonly List<HotbarEntry> _hotbarEntries = new();

    /// <summary>The ability picked and waiting for a target, or null.</summary>
    private AbilityDefinition _selected;

    /// <summary>The target Tab's on, for the keys, until the mouse moves; or null.</summary>
    private Combatant _keyTarget;

    /// <summary>Whether the right button was down last frame, so a click is only the frame it goes down.</summary>
    private bool _rightWasDown;

    /// <summary>What the mouse is over: a hotbar button, END TURN, a portrait or a support on the turn order, a part of the budget, the sigil, an icon in the frame, someone in the room, or a plate.</summary>
    private int _hoverSlot = -1;
    private bool _hoverEndTurn;
    private int _hoverPortrait = -1;
    private int _hoverSupport = -1;
    private BudgetPart? _hoverBudget;
    private bool _hoverSigil;
    private int _hoverIcon = -1;
    private Combatant _hoverFighter;
    private Slot? _hoverPlate;

    /// <summary>What a click would do now, and what it was worked out for, so it's only worked out again when something changes.</summary>
    private TurnPlan _plan;
    private object _planFor;

    /// <summary>Why the last thing tried couldn't be done, and how much longer that's shown.</summary>
    private string _refusal;
    private float _refusalClock;

    /// <summary>Whoever of yours had the last turn, whose hotbar shows (dark) while it's someone else's.</summary>
    private Combatant _lastOfYours;

    /// <summary>Works out what the mouse is over and what a click would do, and on your turn does it.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    /// <returns>True if a fight's on, so nothing else should look at the input.</returns>
    private bool HandleCombatInput(GameTime gameTime, InputState input)
    {
        if (!InCombat) return false;

        _mouse = input.MousePosition;
        bool rightDown = input.CurrentMouseState.RightButton == ButtonState.Pressed;
        bool rightClick = rightDown && !_rightWasDown;
        _rightWasDown = rightDown;
        bool click = input.IsNewLeftClick;
        _refusalClock = MathF.Max(0f, _refusalClock - (float)gameTime.ElapsedGameTime.TotalSeconds);

        if (HandleTutorialInput(input)) return true;

        if (_result.Showing)
        {
            if (_result.Ready && (click || _confirm.Occurred(input) || _endTurn.Occurred(input))) DismissResult();
            return true;
        }

        if (_toggleLog.Occurred(input)) _log.Shown = !_log.Shown;
        if (_pause.Occurred(input))
        {
            if (_selected is not null) Cancel();
            else OpenCombatPause();
            return true;
        }

        if (input.WheelTurn != 0 && _log.Bounds.Contains(_mouse)) _log.Scroll(input.WheelTurn);
        if (input.MouseMoved) _keyTarget = null;

        BuildHotbar();
        _initiative.SetSupports(ActingSupports());
        FindHover();
        if (AwaitingPlayer) _plan = CurrentPlan();
        else
        {
            _plan = null;
            _planFor = null;
        }
        _cursor.Shape = CursorFor(_plan);
        DescribeHover();

        if (!AwaitingPlayer) return true;

        for (int i = 0; i < HotbarKeys.Length && i < _hotbarEntries.Count; i++)
            if (input.IsNewKeyPress(HotbarKeys[i])) PressSlot(i);

        if (_endTurn.Occurred(input)) EndYourTurn();
        else if (_nextTarget.Occurred(input)) NextTarget();
        else if (_confirm.Occurred(input) && _keyTarget is not null && _plan is not null) Carry(_plan);
        else if (ShiftFrom(input) is Slot step) ShiftTo(step);
        else if (rightClick) Cancel();
        else if (click) Click();
        return true;
    }

    /// <summary>Puts back whatever was picked.</summary>
    private void Cancel()
    {
        _selected = null;
        _keyTarget = null;
        _plan = null;
        _planFor = null;
    }

    /// <summary>Says why something can't be done, over the hotbar, with a dull knock.</summary>
    /// <param name="reason">Why, in a few words.</param>
    private void Refuse(string reason)
    {
        _refusal = reason;
        _refusalClock = RefusalSeconds;
        _audio.Play(Sfx.MenuBack);
    }

    /// <summary>Whose hotbar shows: whoever's turn it is if they're yours, or whoever of yours went last.</summary>
    private Combatant HotbarOwner
    {
        get
        {
            if (_combat.Current is { Side: Side.Party, Member: not null, Guest: false } current) _lastOfYours = current;
            return _lastOfYours;
        }
    }

    /// <summary>Lays out the hotbar: the six primary buttons in their places (Auto, the skills, the ultimate, the potion, each empty if the kit has nothing there), then the shared strip.</summary>
    private void BuildHotbar()
    {
        _hotbarEntries.Clear();
        if (HotbarOwner is not Combatant owner) return;

        IReadOnlyList<AbilityDefinition> abilities = _combat.Abilities(owner);
        List<AbilityDefinition> skills = abilities.Where(each => each.Bar == BarSlot.Skill).Take(SkillButtons).ToList();
        var primary = new AbilityDefinition[PrimaryButtons];
        primary[0] = abilities.FirstOrDefault(each => each.Bar == BarSlot.Auto);
        for (int i = 0; i < skills.Count; i++) primary[1 + i] = skills[i];
        primary[1 + SkillButtons] = abilities.FirstOrDefault(each => each.Bar == BarSlot.Ultimate);
        primary[2 + SkillButtons] = abilities.FirstOrDefault(each => each.Bar == BarSlot.Potion);

        for (int i = 0; i < primary.Length; i++) _hotbarEntries.Add(Entry(owner, primary[i], HotbarKeyNames[i], true));
        foreach (AbilityDefinition shared in abilities.Where(each => each.Bar is BarSlot.Shared or BarSlot.Bag).Take(StripButtons))
            _hotbarEntries.Add(Entry(owner, shared, HotbarKeyNames[_hotbarEntries.Count], false));
    }

    /// <summary>One hotbar button: its icon (a bow for an Attack with a bow in hand), whether it can be used, its Focus cost, its cooldown, and what's left in the bag.</summary>
    private HotbarEntry Entry(Combatant owner, AbilityDefinition ability, string key, bool primary)
    {
        if (ability is null) return new HotbarEntry(null, Rectangle.Empty, key, primary, false, false, 0, 0, null, false);

        bool usable = _combat.Unusable(owner, ability) is null;
        Rectangle icon = ability.Id == AbilityId.Attack && owner.MainHand.Ranged ? CombatIcons.Ability("ranged-attack") : CombatIcons.Ability(ability.Id);
        return new HotbarEntry(ability, icon, key, primary, usable, ability == _selected, ability.Focus, _combat.CooldownLeft(owner, ability),
            CountFor(owner, ability), Guiding(ability));
    }

    /// <summary>What's left in the bag of what a button uses up, for its corner; null for anything that isn't from the bag.</summary>
    private string CountFor(Combatant owner, AbilityDefinition ability)
    {
        if (owner.Member is null) return null;
        if (ability.Item is not null) return (_combat.Bag?.Count(ability.Item) ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (ability.Kind == AbilityKind.Potion) return HealingPotions().ToString(System.Globalization.CultureInfo.InvariantCulture);
        return null;
    }

    /// <summary>How many healing potions of every kind are in the bag.</summary>
    private int HealingPotions()
    {
        int count = 0;
        foreach (ItemDefinition item in ItemCatalog.All)
            if (item.Kind == ItemKind.Consumable && !item.Heals.IsNone) count += _combat.Bag?.Count(item.Id) ?? 0;
        return count;
    }

    /// <summary>Someone's Auto: the kit's auto attack, or Attack if it hasn't one.</summary>
    private AbilityDefinition AutoOf(Combatant who) =>
        _combat.Abilities(who).FirstOrDefault(each => each.Bar == BarSlot.Auto) ?? AbilityCatalog.Get(AbilityId.Attack);

    /// <summary>Works out what the mouse is over, the screen's pieces before the room, and in the room someone's figure before a plate.</summary>
    private void FindHover()
    {
        _hoverSlot = -1;
        _hoverEndTurn = false;
        _hoverPortrait = -1;
        _hoverSupport = -1;
        _hoverBudget = null;
        _hoverSigil = false;
        _hoverIcon = -1;
        _hoverFighter = null;
        _hoverPlate = null;

        if (!new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight).Contains(_mouse) || PointingAtTutorial()) return;

        if (CombatHotbar.At(_mouse, _hotbarEntries) is int slot)
        {
            _hoverSlot = slot;
            return;
        }
        if (_hotbar.OnEndTurn(_mouse, _hotbarEntries))
        {
            _hoverEndTurn = true;
            return;
        }

        IReadOnlyList<Combatant> order = _shown.Order;
        if (_initiative.At(_mouse, order.Count, CurrentIndex(order)) is int portrait)
        {
            _hoverPortrait = portrait;
            _hoverFighter = order[portrait];
            return;
        }
        if (_initiative.SupportAt(_mouse, order.Count) is int support)
        {
            _hoverSupport = support;
            return;
        }
        if (_budgetBar.At(_mouse) is BudgetPart part)
        {
            _hoverBudget = part;
            return;
        }
        if (_combat.SoulLink.Active && SoulLinkSigil.Bounds.Contains(_mouse))
        {
            _hoverSigil = true;
            return;
        }
        if (Fighter(_shown.Turn) is Combatant current && UnitFrame.Bounds.Contains(_mouse))
        {
            Rectangle[] icons = _unitFrame.Icons(_shown.Conditions(current).Count, Passives(current).Count);
            _hoverIcon = Array.FindIndex(icons, icon => icon.Contains(_mouse));
            return;
        }
        if (_log.Bounds.Contains(_mouse) || CombatHotbar.Bar.Contains(_mouse)) return;

        if (_keyTarget is not null)
        {
            _hoverFighter = _keyTarget;
            return;
        }

        Vector2 world = _camera.ToWorld(_mouse.ToVector2());
        _hoverFighter = FighterAt(world);
        if (_hoverFighter is not null || _stage.SlotAt(world) is not Slot plate) return;

        if (_shown.At(plate) is Combatant standing && !_shown.HasDied(standing)) _hoverFighter = standing;
        else _hoverPlate = plate;
    }

    /// <summary>Whoever in the fight the mouse is over in the room, the one in front if two overlap, or null. The dead foes and the guests who fell back have gone.</summary>
    private Combatant FighterAt(Vector2 world)
    {
        Combatant found = null;
        WalkerSprite front = null;
        foreach (Combatant each in _combat.Combatants)
        {
            if (each.Side == Side.Foes && _shown.HasDied(each) || _shown.HasFallenBack(each) || FighterWalker(each.Id) is not WalkerSprite walker || walker.Opacity <= 0f) continue;
            if (!Contains(walker.Figure, world) || front is not null && walker.Position.Y <= front.Position.Y) continue;
            found = each;
            front = walker;
        }
        return found;
    }

    /// <summary>Where whoever's turn it is sits on the shown turn order, or -1.</summary>
    private int CurrentIndex(IReadOnlyList<Combatant> order)
    {
        for (int i = 0; i < order.Count; i++)
            if (order[i].Id == _shown.Turn) return i;
        return -1;
    }

    /// <summary>What a click would do for what the mouse is over, worked out again only when that, the pick or the turn has changed.</summary>
    private TurnPlan CurrentPlan()
    {
        Combatant me = _combat.Current;
        Slot here = _shown.SlotsOf(me).FirstOrDefault();
        var key = (me.Id, here, me.Budget.Action, me.Budget.BonusAction, me.Budget.Shift, me.Focus, _selected?.Id, _hoverFighter?.Id, _hoverPlate, _shown.Round);
        if (Equals(key, _planFor)) return _plan;
        _planFor = key;

        if (_hoverFighter is Combatant fighter) return PlanFor(me, fighter);
        return _hoverPlate is Slot plate ? PlanAt(me, plate) : null;
    }

    /// <summary>What a click on someone would do: the picked ability on them; or with nothing picked, your Auto on a foe, Help for a friend who's down, or a Shift into a friend's slot if you can swap with them.</summary>
    private TurnPlan PlanFor(Combatant me, Combatant fighter)
    {
        if (_selected is AbilityDefinition picked) return Aim(me, picked, fighter);
        if (fighter == me || _shown.HasDied(fighter)) return null;
        if (fighter.Side != me.Side) return Aim(me, AutoOf(me), fighter);
        if (_shown.IsDown(fighter)) return Aim(me, AbilityCatalog.Get(AbilityId.Help), fighter);
        IReadOnlyList<Slot> theirs = _shown.SlotsOf(fighter);
        return theirs.Count > 0 && _combat.Shifts(me).Contains(theirs[0]) ? PlanShift(me, theirs[0]) : null;
    }

    /// <summary>What a click on an empty plate would do: with an ability picked, nothing (it wants someone); with nothing picked, a Shift there if it's on your side.</summary>
    private TurnPlan PlanAt(Combatant me, Slot plate)
    {
        if (_selected is not null) return new TurnPlan(_selected, null, null, IsForFriends(_selected) ? "Pick an ally." : "Pick a foe.");
        if (plate.Side != me.Side || _shown.SlotsOf(me).Contains(plate)) return null;
        return PlanShift(me, plate);
    }

    /// <summary>An ability on someone, with why it can't be done if it can't.</summary>
    private TurnPlan Aim(Combatant me, AbilityDefinition ability, Combatant target) => new(ability, target, null, _combat.WhyNot(me, ability, target));

    /// <summary>A Shift to a slot, with why it can't be done if it can't.</summary>
    private TurnPlan PlanShift(Combatant me, Slot to) => new(null, null, to, _combat.WhyNotShift(me, to));

    /// <summary>Whether an ability's aimed at your own side.</summary>
    private static bool IsForFriends(AbilityDefinition ability) =>
        ability.Shape is TargetShape.Self or TargetShape.Ally or TargetShape.AllyNextTo or TargetShape.AllAllies or TargetShape.YourRow or TargetShape.YourBackRow;

    /// <summary>The cursor for what a click would do: the boot for a Shift, the hand for an ability, the cross for something that can't be done.</summary>
    private static CursorShape CursorFor(TurnPlan plan) => plan switch
    {
        null => CursorShape.Default,
        { Problem: not null } => CursorShape.Invalid,
        { Ability: not null } => CursorShape.Use,
        _ => CursorShape.Move,
    };

    /// <summary>A click: a hotbar button, END TURN, the sigil, or whatever's planned in the room.</summary>
    private void Click()
    {
        if (_hoverSlot >= 0) PressSlot(_hoverSlot);
        else if (_hoverEndTurn) EndYourTurn();
        else if (_hoverSigil) PickSoulLink();
        else if (_hoverBudget is null && _hoverIcon < 0 && _hoverSupport < 0 && _plan is not null) Carry(_plan);
    }

    /// <summary>A button pressed: an empty one or one that can't be used says why, one used on yourself happens now, and any other's picked (or put back, if it was picked already) and waits for a target, with the first good one ready for Enter.</summary>
    /// <param name="index">Its place on the hotbar.</param>
    private void PressSlot(int index)
    {
        HotbarEntry entry = _hotbarEntries[index];
        Combatant me = _combat.Current;
        if (entry.Ability is null)
        {
            Refuse("Nothing there yet.");
            return;
        }
        if (!entry.Usable)
        {
            Refuse(_combat.Unusable(me, entry.Ability) ?? "Not now.");
            return;
        }
        Pick(me, entry.Ability);
    }

    /// <summary>The sigil clicked: the Soul-Link Strike picked, or why it can't be.</summary>
    private void PickSoulLink()
    {
        Combatant me = _combat.Current;
        AbilityDefinition strike = _combat.Abilities(me).FirstOrDefault(each => each.Id == AbilityId.SoulLinkStrike);
        if (strike is null)
        {
            Refuse("The bond can't be called on now.");
            return;
        }
        if (_combat.Unusable(me, strike) is string why)
        {
            Refuse(why);
            return;
        }
        Pick(me, strike);
    }

    /// <summary>Picks an ability: one used on yourself happens now; any other waits for a target, the first good one ready for Enter.</summary>
    private void Pick(Combatant me, AbilityDefinition ability)
    {
        _audio.Play(Sfx.MenuMove);
        if (ability.Shape == TargetShape.Self)
        {
            Cancel();
            IReadOnlyList<CombatEvent> used = _combat.Use(ability, me);
            Play(used);
            if (used.Count > 0) LearnFrom(ability);
            return;
        }

        _selected = _selected == ability ? null : ability;
        _keyTarget = _selected is null ? null : _combat.Targets(me, _selected).FirstOrDefault(each => _combat.WhyNot(me, _selected, each) is null);
        _planFor = null;
    }

    /// <summary>Carries out what was planned: the Shift, or the ability.</summary>
    /// <param name="plan">The plan.</param>
    private void Carry(TurnPlan plan)
    {
        if (plan.Problem is not null)
        {
            Refuse(plan.Problem);
            return;
        }

        if (plan.Shift is Slot to)
        {
            Play(_combat.Shift(to));
            Learn(TutorialCatalog.CombatShift);
        }
        else if (plan.Ability is AbilityDefinition ability)
        {
            IReadOnlyList<CombatEvent> used = _combat.Use(ability, plan.Target);
            Play(used);
            if (used.Count > 0) LearnFrom(ability);
        }
        Cancel();
    }

    /// <summary>Ends your turn, with whatever's left of it.</summary>
    private void EndYourTurn()
    {
        Cancel();
        Learn(TutorialCatalog.CombatEndTurn);
        Play(_combat.EndTurn());
    }

    /// <summary>Tab: on to the next of whoever the picked ability (or your Auto) could be aimed at.</summary>
    private void NextTarget()
    {
        Combatant me = _combat.Current;
        IReadOnlyList<Combatant> targets = _combat.Targets(me, _selected ?? AutoOf(me));
        if (targets.Count == 0) return;

        int at = _keyTarget is null ? -1 : IndexIn(targets, _keyTarget);
        _keyTarget = targets[(at + 1) % targets.Count];
        _planFor = null;
    }

    /// <summary>Where someone is in a list, by reference, or -1.</summary>
    private static int IndexIn(IReadOnlyList<Combatant> list, Combatant who)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i] == who) return i;
        return -1;
    }

    /// <summary>The slot WASD or the arrows Shift to from where you stand: W the back row, S the front, A and D along the row; or null if none was pressed or it's off the side.</summary>
    private Slot? ShiftFrom(InputState input)
    {
        IReadOnlyList<Slot> mine = _shown.SlotsOf(_combat.Current);
        if (mine.Count == 0) return null;
        Slot here = mine[0];

        Slot? to = null;
        if (_cursorUp.Occurred(input)) to = here with { Row = Row.Back };
        else if (_cursorDown.Occurred(input)) to = here with { Row = Row.Front };
        else if (_cursorLeft.Occurred(input)) to = here with { Lane = here.Lane - 1 };
        else if (_cursorRight.Occurred(input)) to = here with { Lane = here.Lane + 1 };

        if (to is not Slot slot) return null;
        if (slot == here || slot.Lane < 0 || slot.Lane >= Slot.LanesOf(slot.Side))
        {
            Refuse("There's no slot that way.");
            return null;
        }
        return slot;
    }

    /// <summary>Shifts whoever's turn it is to a slot, if they can.</summary>
    private void ShiftTo(Slot slot)
    {
        if (_combat.WhyNotShift(_combat.Current, slot) is string why)
        {
            Refuse(why);
            return;
        }

        Cancel();
        Play(_combat.Shift(slot));
        Learn(TutorialCatalog.CombatShift);
    }

    /// <summary>Someone's always-on abilities, for their frame: Sneak Attack, the Defence style, Pack Tactics.</summary>
    private List<AbilityDefinition> Passives(Combatant who)
    {
        var passives = new List<AbilityDefinition>();
        foreach (AbilityId id in _kits.Kit(who))
        {
            AbilityDefinition ability = AbilityCatalog.Get(id);
            if (ability.Kind == AbilityKind.Passive || ability.Bar == BarSlot.Passive) passives.Add(ability);
        }
        return passives;
    }
}
