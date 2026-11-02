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
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// Your turn in a fight: what the mouse is over, what a click would do there, and doing it, by the
/// mouse or the keys.
/// </summary>
/// <remarks>
/// <para>
/// With nothing picked, a click on the ground walks there (as far as the turn's movement goes), on a
/// foe attacks it (walking into reach first if it's in range of a walk), and on a friend who's
/// Downed helps them up. 1 to 0 (then - and =) or a click picks a slot on the hotbar: one that's
/// used on yourself happens at once; any other waits for a target, which the mouse picks, or Tab
/// goes through and Enter uses. Right click or Escape puts it back. Space, End or END TURN ends the
/// turn, and WASD or the arrows step a tile. What a click would do is worked out as a TurnPlan before
/// it's made, so the path, the rings and the card show exactly that.
/// </para>
/// <para>
/// The screen's pieces (the hotbar, the turn order, the budget, the sigil, the frame) are in front
/// of the map, so the mouse finds them first. L shows and hides the log, the wheel scrolls it when
/// the mouse is on it and zooms otherwise, and Escape with nothing picked is the pause menu. While
/// it isn't your turn, pointing still works and says what everything is; only doing is held back.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long the line saying why something can't be done stays up.</summary>
    private const float RefusalSeconds = 1.8f;

    /// <summary>The hotbar's keys, left to right, as CombatHotbar writes them.</summary>
    private static readonly Keys[] HotbarKeys =
    {
        Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0, Keys.OemMinus, Keys.OemPlus,
    };

    private readonly InputAction _toggleLog = new(Array.Empty<Buttons>(), new[] { Keys.L }, true);
    private readonly InputAction _endTurn = new(new[] { Buttons.Y }, new[] { Keys.Space, Keys.End }, true);
    private readonly InputAction _nextTarget = new(new[] { Buttons.RightShoulder }, new[] { Keys.Tab }, true);
    private readonly InputAction _confirm = new(new[] { Buttons.A }, new[] { Keys.Enter }, true);
    private readonly InputAction _cursorUp = new(new[] { Buttons.DPadUp }, new[] { Keys.W, Keys.Up }, true);
    private readonly InputAction _cursorDown = new(new[] { Buttons.DPadDown }, new[] { Keys.S, Keys.Down }, true);
    private readonly InputAction _cursorLeft = new(new[] { Buttons.DPadLeft }, new[] { Keys.A, Keys.Left }, true);
    private readonly InputAction _cursorRight = new(new[] { Buttons.DPadRight }, new[] { Keys.D, Keys.Right }, true);

    /// <summary>The hotbar's slots for whoever's turn it is (or whoever of yours went last), rebuilt each frame.</summary>
    private readonly List<HotbarEntry> _hotbarEntries = new();

    /// <summary>The ability picked and waiting for a target, or null.</summary>
    private AbilityDefinition _selected;

    /// <summary>The target Tab's on, for the keys, until the mouse moves; or null.</summary>
    private Combatant _keyTarget;

    /// <summary>Whether the right button was down last frame, so a click is only the frame it goes down.</summary>
    private bool _rightWasDown;

    /// <summary>What the mouse is over: a hotbar slot, END TURN, a portrait, a part of the budget, the sigil, an icon in the frame, someone on the map, or a tile.</summary>
    private int _hoverSlot = -1;
    private bool _hoverEndTurn;
    private int _hoverPortrait = -1;
    private BudgetPart? _hoverBudget;
    private bool _hoverSigil;
    private int _hoverIcon = -1;
    private Combatant _hoverFighter;
    private Point? _hoverCell;

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

        if (input.WheelTurn != 0)
        {
            if (_log.Bounds.Contains(_mouse)) _log.Scroll(input.WheelTurn);
            else _camera.ZoomBy(input.WheelTurn);
        }
        if (input.MouseMoved) _keyTarget = null;

        BuildHotbar();
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
        else if (StepFrom(input) is Point step) StepTo(step);
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

    /// <summary>Lays out the hotbar's slots: the fighting ones (the weapon and the class's own), the Soul-Link, moving about, and the bag.</summary>
    private void BuildHotbar()
    {
        _hotbarEntries.Clear();
        if (HotbarOwner is not Combatant owner) return;

        foreach (AbilityDefinition ability in _combat.Abilities(owner).OrderBy(HotbarGroup))
        {
            bool usable = _combat.Unusable(owner, ability) is null;
            Rectangle icon = ability.Id == AbilityId.Attack && owner.MainHand.Ranged ? CombatIcons.Ability("ranged-attack") : CombatIcons.Ability(ability.Id);
            _hotbarEntries.Add(new HotbarEntry(ability, icon, HotbarGroup(ability), usable, ability == _selected, CountFor(owner, ability), Guiding(ability)));
        }
    }

    /// <summary>Which group of the hotbar an ability's in: 0 the fighting ones, 1 the Soul-Link, 2 moving about and helping, 3 what comes out of the bag.</summary>
    private static int HotbarGroup(AbilityDefinition ability)
    {
        if (ability.Id == AbilityId.SoulLinkStrike) return 1;
        if (ability.Item is not null || ability.Kind == AbilityKind.Potion) return 3;
        if (AbilityCatalog.IsCommon(ability.Id) && ability.Id is not (AbilityId.Attack or AbilityId.OffHandAttack)) return 2;
        return 0;
    }

    /// <summary>What's left of what a slot spends, for its corner: spell slots or uses, or how many are in the bag; null for something free.</summary>
    private string CountFor(Combatant owner, AbilityDefinition ability)
    {
        if (owner.Member is null) return null;
        if (ability.Item is not null) return (_combat.Bag?.Count(ability.Item) ?? 0).ToString();
        if (ability.Kind == AbilityKind.Potion) return HealingPotions().ToString();
        return ability.Resource is null ? null : ResourceCatalog.Left(owner.Member, ability.Resource).ToString();
    }

    /// <summary>How many healing potions of every kind are in the bag.</summary>
    private int HealingPotions()
    {
        int count = 0;
        foreach (ItemDefinition item in ItemCatalog.All)
            if (item.Kind == ItemKind.Consumable && !item.Heals.IsNone) count += _combat.Bag?.Count(item.Id) ?? 0;
        return count;
    }

    /// <summary>Works out what the mouse is over, the screen's pieces before the map.</summary>
    private void FindHover()
    {
        _hoverSlot = -1;
        _hoverEndTurn = false;
        _hoverPortrait = -1;
        _hoverBudget = null;
        _hoverSigil = false;
        _hoverIcon = -1;
        _hoverFighter = null;
        _hoverCell = null;

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
        if (_log.Bounds.Contains(_mouse)) return;

        if (_keyTarget is not null)
        {
            _hoverFighter = _keyTarget;
            return;
        }

        Vector2 world = _camera.ToWorld(_mouse.ToVector2());
        _hoverFighter = FighterAt(world);
        if (_hoverFighter is null) _hoverCell = Tilemap.CellOf(world);
    }

    /// <summary>Whoever in the fight the mouse is over on the map, the one in front if two overlap, or null. The dead foes have gone.</summary>
    private Combatant FighterAt(Vector2 world)
    {
        Combatant found = null;
        WalkerSprite front = null;
        foreach (Combatant each in _combat.Combatants)
        {
            if (each.Side == Side.Foes && _shown.HasDied(each) || FighterWalker(each.Id) is not WalkerSprite walker || walker.Opacity <= 0f) continue;
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

    /// <summary>What a click would do for what the mouse is over, worked out again only when that, the pick or the turn's budget has changed.</summary>
    private TurnPlan CurrentPlan()
    {
        Combatant me = _combat.Current;
        var key = (me.Id, me.Cell, me.Budget.MovementLeft, me.Budget.Action, me.Budget.BonusAction, _selected?.Id, _hoverFighter?.Id, _hoverFighter?.Cell, _hoverCell);
        if (Equals(key, _planFor)) return _plan;
        _planFor = key;

        if (_hoverFighter is Combatant fighter) return PlanFor(me, fighter);
        return _hoverCell is Point cell ? PlanAt(me, cell) : null;
    }

    /// <summary>What a click on someone would do: the picked ability on them, or with nothing picked, an attack on a foe or Help for a friend who's down.</summary>
    private TurnPlan PlanFor(Combatant me, Combatant fighter)
    {
        AbilityDefinition ability = _selected ?? DefaultAbility(me, fighter);
        if (ability is null) return null;
        if (ability.Target is TargetKind.Area or TargetKind.Point) return PlanAt(me, fighter.Cell);
        return PlanUse(me, ability, fighter, fighter.Cell);
    }

    /// <summary>What a click with nothing picked does to someone: attack a foe, help up a friend who's down, or nothing.</summary>
    private AbilityDefinition DefaultAbility(Combatant me, Combatant fighter)
    {
        if (fighter == me || _shown.HasDied(fighter)) return null;
        if (fighter.Side != me.Side) return AbilityCatalog.Get(AbilityId.Attack);
        return _shown.IsDown(fighter) ? AbilityCatalog.Get(AbilityId.Help) : null;
    }

    /// <summary>What a click on a tile would do: the picked area ability there, a walk there with nothing picked, or nothing for an ability that wants someone.</summary>
    private TurnPlan PlanAt(Combatant me, Point cell)
    {
        if (_selected is { Target: TargetKind.Area or TargetKind.Point }) return PlanUse(me, _selected, null, cell);
        if (_selected is not null) return new TurnPlan(_selected, null, cell, Array.Empty<Point>(), 0, _selected.Target == TargetKind.Ally ? "Pick an ally." : "Pick a foe.");
        if (cell == me.Cell) return null;

        IReadOnlyList<Point> path = _combat.PathTo(cell);
        if (path.Count == 0) return new TurnPlan(null, null, cell, path, 0, "There's no way there.");
        int reachable = Reachable(me, path);
        return new TurnPlan(null, null, cell, path, reachable, reachable == 0 ? "No movement left for that." : null);
    }

    /// <summary>Using an ability on someone or at a tile: now if it can be, or after a walk into reach if that's all that's stopping it and the turn's movement will do it.</summary>
    private TurnPlan PlanUse(Combatant me, AbilityDefinition ability, Combatant target, Point cell)
    {
        if (_combat.WhyNot(me, ability, target, cell) is not string why) return new TurnPlan(ability, target, cell, Array.Empty<Point>(), 0, null);
        if ((_combat.Unusable(me, ability) ?? _combat.TargetFault(me, ability, target, cell)) is string fault)
            return new TurnPlan(ability, target, cell, Array.Empty<Point>(), 0, fault);

        Point aim = (ability.Target is TargetKind.Enemy or TargetKind.Ally) ? target.Cell : cell;
        List<Point> path = WalkIntoReach(me, ability, aim);
        return path is null
            ? new TurnPlan(ability, target, cell, Array.Empty<Point>(), 0, why)
            : new TurnPlan(ability, target, cell, path, path.Count, null);
    }

    /// <summary>The shortest walk this turn to somewhere free an ability reaches a tile from, the one that provokes fewest opportunity attacks of those as short; or null if there's none.</summary>
    private List<Point> WalkIntoReach(Combatant me, AbilityDefinition ability, Point aim)
    {
        Dictionary<Point, Point> came = _combat.Flood(me, me.Budget.MovementLeft);
        List<Point> best = null;
        int bestThreats = 0;
        foreach (Point tile in came.Keys)
        {
            if (tile == me.Cell || _combat.At(tile, me) is not null || _combat.ReachFrom(me, ability, tile, aim) is not null) continue;

            List<Point> path = CombatEngine.PathIn(came, tile);
            int threats = _combat.ThreatsAlong(me, me.Cell, path).Count;
            if (best is not null && (path.Count > best.Count || path.Count == best.Count && threats >= bestThreats)) continue;
            best = path;
            bestThreats = threats;
        }
        return best;
    }

    /// <summary>How many of a path's tiles this turn's movement covers, ending on a free one, the way the engine walks it.</summary>
    private int Reachable(Combatant me, IReadOnlyList<Point> path)
    {
        int last = 0;
        for (int i = 0; i < path.Count; i++)
        {
            if (!me.Budget.CanWalk((i + 1) * Measure.TileMetres)) break;
            if (_combat.At(path[i], me) is null) last = i + 1;
        }
        return last;
    }

    /// <summary>The cursor for what a click would do: the boot for a walk, the hand for an ability, the cross for something that can't be done.</summary>
    private static CursorShape CursorFor(TurnPlan plan) => plan switch
    {
        null => CursorShape.Default,
        { Problem: not null } => CursorShape.Invalid,
        { Ability: not null } => CursorShape.Use,
        _ => CursorShape.Move,
    };

    /// <summary>A click: a hotbar slot, END TURN, or whatever's planned on the map.</summary>
    private void Click()
    {
        if (_hoverSlot >= 0) PressSlot(_hoverSlot);
        else if (_hoverEndTurn) EndYourTurn();
        else if (_hoverBudget is null && !_hoverSigil && _hoverIcon < 0 && _plan is not null) Carry(_plan);
    }

    /// <summary>A slot pressed: one that can't be used says why, one used on yourself happens now, and any other's picked (or put back, if it was picked already) and waits for a target, with the nearest good one ready for Enter.</summary>
    /// <param name="index">Its place on the hotbar.</param>
    private void PressSlot(int index)
    {
        HotbarEntry entry = _hotbarEntries[index];
        Combatant me = _combat.Current;
        if (!entry.Usable)
        {
            Refuse(_combat.Unusable(me, entry.Ability) ?? "Not now.");
            return;
        }

        _audio.Play(Sfx.MenuMove);
        if (entry.Ability.Target == TargetKind.Self)
        {
            Cancel();
            IReadOnlyList<CombatEvent> used = _combat.Use(entry.Ability, me, me.Cell);
            Play(used);
            if (used.Count > 0) LearnFrom(entry.Ability);
            return;
        }

        _selected = _selected == entry.Ability ? null : entry.Ability;
        _keyTarget = _selected is null ? null : Targets(_selected).FirstOrDefault(each => _combat.WhyNot(me, _selected, each, each.Cell) is null);
        _planFor = null;
    }

    /// <summary>Carries out what was planned: the walk, then the ability.</summary>
    /// <param name="plan">The plan.</param>
    private void Carry(TurnPlan plan)
    {
        if (plan.Problem is not null)
        {
            Refuse(plan.Problem);
            return;
        }

        if (plan.Path.Count > 0)
        {
            Play(_combat.Move(plan.Path));
            Learn(TutorialCatalog.CombatMove);
        }

        if (plan.Ability is AbilityDefinition ability)
        {
            IReadOnlyList<CombatEvent> used = _combat.Use(ability, plan.Target, plan.Cell);
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

    /// <summary>Tab: on to the next of whoever the picked ability (or an attack) could be aimed at, nearest first.</summary>
    private void NextTarget()
    {
        List<Combatant> targets = Targets(_selected);
        if (targets.Count == 0) return;

        int at = _keyTarget is null ? -1 : targets.IndexOf(_keyTarget);
        _keyTarget = targets[(at + 1) % targets.Count];
        _planFor = null;
    }

    /// <summary>Everyone an ability could be aimed at, nearest first: foes for one aimed at a foe (or with nothing picked), friends for one aimed at a friend.</summary>
    private List<Combatant> Targets(AbilityDefinition ability)
    {
        Combatant me = _combat.Current;
        bool friends = ability?.Target == TargetKind.Ally;
        var targets = new List<Combatant>();
        foreach (Combatant each in _combat.Combatants)
        {
            if (_shown.HasDied(each) || friends != (each.Side == me.Side)) continue;
            if (friends && ability.NotSelf && each == me) continue;
            targets.Add(each);
        }
        targets.Sort((a, b) => Measure.Steps(me.Cell, a.Cell).CompareTo(Measure.Steps(me.Cell, b.Cell)));
        return targets;
    }

    /// <summary>The tile WASD or the arrows step to, or null if none was pressed.</summary>
    private Point? StepFrom(InputState input)
    {
        Point at = _combat.Current.Cell;
        if (_cursorUp.Occurred(input)) return new Point(at.X, at.Y - 1);
        if (_cursorDown.Occurred(input)) return new Point(at.X, at.Y + 1);
        if (_cursorLeft.Occurred(input)) return new Point(at.X - 1, at.Y);
        if (_cursorRight.Occurred(input)) return new Point(at.X + 1, at.Y);
        return null;
    }

    /// <summary>Steps whoever's turn it is onto the tile next to them, if they can.</summary>
    private void StepTo(Point cell)
    {
        IReadOnlyList<Point> path = _combat.PathTo(cell);
        if (path.Count != 1 || Reachable(_combat.Current, path) != 1)
        {
            Refuse(path.Count == 1 ? "No movement left." : "Can't step there.");
            return;
        }

        Cancel();
        Play(_combat.Move(path));
        Learn(TutorialCatalog.CombatMove);
    }

    /// <summary>Someone's always-on abilities, for their frame: Sneak Attack, the Defence style, Pack Tactics.</summary>
    private static List<AbilityDefinition> Passives(Combatant who)
    {
        var passives = new List<AbilityDefinition>();
        IEnumerable<AbilityId> kit = who.Member is not null ? AbilityCatalog.Kit(who.Member.Class) : who.Enemy?.Abilities ?? Enumerable.Empty<AbilityId>();
        foreach (AbilityId id in kit)
            if (AbilityCatalog.Get(id).Kind == AbilityKind.Passive) passives.Add(AbilityCatalog.Get(id));
        return passives;
    }
}
