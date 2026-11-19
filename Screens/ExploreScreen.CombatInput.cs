using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components.Realtime;
using LegendQuest.StateManagement;
using LegendQuest.Tutorial;

namespace LegendQuest.Screens;

/// <summary>
/// The fight's mouse and keys (Story/11-combat.md §2.3, §7.1): targeting, walking, the bar, the
/// Soul-Link, the log and the pause.
/// </summary>
/// <remarks>
/// <para>
/// Everything you do is a BattleCommand for the leader, so the engine owns where you are and what
/// you're doing: a click on a foe targets it, which turns your auto attack on and walks you into its
/// reach; a click on one of your side targets them, for heals and buffs; a click on the floor walks
/// you there; WASD and the arrows walk you while they're held. 1 to 6 or a click on a slot use the
/// bar: 1 turns the auto attack on and off, 2 to 5 use the skill on your target (one out of reach
/// walks you into reach and then goes off, as the engine has it), 6 drinks the bag's best healing
/// potion; 7 or the sigil is the Soul-Link Strike. Tab goes to the next foe, nearest first; T takes
/// your target's target; Escape clears your target, and with none it's the pause menu; L shows the
/// log, the wheel scrolls it; Alt shows everyone's numbers.
/// </para>
/// <para>
/// The HUD is in front of the room, so the mouse finds it first: a slot, the sigil, a party row, an
/// aura; then the log; then someone in the room, the one in front if two overlap; then the floor. The
/// HUD draws the cards for the slots and the auras; the screen's own card says who someone in the
/// room is. While the proof run has the AI playing for you, pointing still works and does nothing.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long the line saying why something can't be done stays up.</summary>
    private const float RefusalSeconds = 1.8f;

    /// <summary>How far ahead of you the keys walk you each frame they're held, in world pixels: far enough to keep going until the next frame's.</summary>
    private const float KeyStep = 14f;

    /// <summary>The bar's keys, 1 to 7.</summary>
    private static readonly Keys[] BarKeys = { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7 };

    private readonly InputAction _toggleLog = new(Array.Empty<Buttons>(), new[] { Keys.L }, true);
    private readonly InputAction _nextFoe = new(new[] { Buttons.RightShoulder }, new[] { Keys.Tab }, true);
    private readonly InputAction _assist = new(Array.Empty<Buttons>(), new[] { Keys.T }, true);
    private readonly InputAction _dismiss = new(new[] { Buttons.A }, new[] { Keys.Enter, Keys.Space }, true);

    /// <summary>What the mouse is on: a part of the HUD, someone in the room, or the floor (a point), or nothing.</summary>
    private CombatHud.Spot _hoverHud = CombatHud.Spot.Nothing;
    private Fighter _hoverFighter;
    private Vector2? _hoverFloor;

    /// <summary>Which way the keys were walking you last frame, to stop you when they're let go.</summary>
    private Vector2 _held;

    /// <summary>Why the last thing tried couldn't be done, and how much longer that's shown.</summary>
    private string _refusal;
    private float _refusalClock;

    /// <summary>The leader, while they're yours to drive: not while the proof run has the AI playing for you.</summary>
    private Fighter Driven => _proofAutoplay || _battle.LeaderAutoplay ? null : _battle.Leader;

    /// <summary>Works out what the mouse is over and does whatever the keys and the clicks ask for.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    /// <returns>True if a fight's on, so nothing else should look at the input.</returns>
    private bool HandleCombatInput(GameTime gameTime, InputState input)
    {
        if (!InCombat) return false;

        _mouse = input.MousePosition;
        bool click = input.IsNewLeftClick;
        _refusalClock = MathF.Max(0f, _refusalClock - (float)gameTime.ElapsedGameTime.TotalSeconds);

        if (HandleTutorialInput(input)) return true;

        if (_result.Showing)
        {
            if (_result.Ready && (click || _dismiss.Occurred(input))) DismissResult();
            return true;
        }

        if (_toggleLog.Occurred(input)) _log.Shown = !_log.Shown;
        if (input.WheelTurn != 0 && _log.Shown && _log.Bounds.Contains(_mouse)) _log.Scroll(input.WheelTurn);

        Fighter leader = Driven;
        if (_pause.Occurred(input))
        {
            if (leader?.Target is not null)
            {
                _battle.Command(BattleCommand.Target(leader.Id, null));
                _battle.Command(BattleCommand.Stop(leader.Id));
            }
            else OpenCombatPause();
            return true;
        }

        FindHover();
        _cursor.Shape = CursorFor(leader);
        if (leader is null || _fightEnded || _battle.Outcome != FightOutcome.Ongoing) return true;

        for (int i = 0; i < BarKeys.Length; i++)
            if (input.IsNewKeyPress(BarKeys[i])) UseSlot(leader, i);

        if (_nextFoe.Occurred(input)) NextFoe(leader);
        if (_assist.Occurred(input)) Assist(leader);

        Vector2 push = ReadMovement(input);
        if (push.LengthSquared() >= MinimumPush)
        {
            push.Normalize();
            _battle.Command(BattleCommand.MoveTo(leader.Id, ToEngine(ToScreen(leader.Position) + push * KeyStep)));
            _held = push;
            Learn(TutorialCatalog.CombatMove);
        }
        else if (_held != Vector2.Zero)
        {
            _held = Vector2.Zero;
            _battle.Command(BattleCommand.MoveTo(leader.Id, leader.Position));
        }

        if (click) Click(leader);
        return true;
    }

    /// <summary>Works out what the mouse is on: the HUD first, then the log, then someone in the room, then the floor.</summary>
    private void FindHover()
    {
        _hoverHud = CombatHud.Spot.Nothing;
        _hoverFighter = null;
        _hoverFloor = null;
        if (!new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight).Contains(_mouse) || PointingAtTutorial() || _hudView is null) return;

        _hoverHud = _hud.At(_mouse, _hudView);
        if (_hoverHud.On != CombatHud.Part.None) return;
        if (_log.Shown && _log.Bounds.Contains(_mouse)) return;
        if (_hud.Player.Bounds.Contains(_mouse) || Hotbar.Panel.Contains(_mouse)) return;

        Vector2 world = _camera.ToWorld(_mouse.ToVector2());
        _hoverFighter = FighterAt(world);
        if (_hoverFighter is null && _room.OnFloor(world)) _hoverFloor = world;
    }

    /// <summary>Whoever in the fight the mouse is over in the room, the one in front if two overlap, or null. The dead, the gone and the hidden can't be pointed at.</summary>
    private Fighter FighterAt(Vector2 world)
    {
        Fighter found = null;
        WalkerSprite front = null;
        foreach (Fighter fighter in _battle.Fighters)
        {
            if (!fighter.Present || fighter.IsDead || fighter.HasLeft || fighter.Vanished || !_fighters.TryGetValue(fighter.Id, out WalkerSprite walker) || walker.Opacity <= 0f) continue;
            if (!Contains(walker.Figure, world) || front is not null && walker.Position.Y <= front.Position.Y) continue;
            found = fighter;
            front = walker;
        }
        return found;
    }

    /// <summary>The cursor for what a click would do: the hand on a foe or a friend, the boot on the floor.</summary>
    private CursorShape CursorFor(Fighter leader)
    {
        if (leader is null || _hoverHud.On != CombatHud.Part.None) return CursorShape.Default;
        if (_hoverFighter is not null) return _hoverFighter == leader ? CursorShape.Default : CursorShape.Use;
        return _hoverFloor is not null ? CursorShape.Move : CursorShape.Default;
    }

    /// <summary>A click: a slot, the sigil, a party row (target them), someone in the room (target them, and walk into reach of a foe), or the floor (walk there).</summary>
    private void Click(Fighter leader)
    {
        switch (_hoverHud.On)
        {
            case CombatHud.Part.Slot:
                UseSlot(leader, _hoverHud.Index);
                return;
            case CombatHud.Part.Sigil:
                UseSlot(leader, Battle.SoulLinkKey - 1);
                return;
            case CombatHud.Part.PartyRow:
                if (_hoverHud.Index < _hudView.Party.Count) TargetOf(leader, FighterOf(_hudView.Party[_hoverHud.Index].Id));
                return;
            case CombatHud.Part.Aura:
                return;
        }

        if (_hoverFighter is Fighter fighter)
        {
            if (fighter != leader) TargetOf(leader, fighter);
            return;
        }
        if (_hoverFloor is Vector2 floor) _battle.Command(BattleCommand.MoveTo(leader.Id, ToEngine(floor)));
    }

    /// <summary>Targets someone: a foe turns your auto attack on and walks you into its reach; one of your side is there for your heals and buffs.</summary>
    private void TargetOf(Fighter leader, Fighter fighter)
    {
        if (fighter is null || fighter.IsDead || fighter.HasLeft) return;
        _battle.Command(BattleCommand.Target(leader.Id, fighter.Id));
        if (fighter.Side == Side.Foes)
        {
            _battle.Command(BattleCommand.AutoAttack(leader.Id, true));
            Learn(TutorialCatalog.CombatTarget);
        }
        else if (fighter.IsDown)
        {
            _battle.Command(BattleCommand.Help(leader.Id, fighter.Id));
            Learn(TutorialCatalog.CombatDowned);
        }
    }

    /// <summary>One of the bar's slots pressed, 0 for key 1: the auto attack on or off, a skill on your target (or its own choice), the potion, or the Soul-Link; one that can't be used says why.</summary>
    /// <param name="leader">You.</param>
    /// <param name="index">The slot, 0 to 6.</param>
    private void UseSlot(Fighter leader, int index)
    {
        int key = index + 1;
        Combat.Realtime.BarSlot slot = _battle.Bar(leader).FirstOrDefault(each => each.Key == key);
        if (index < Hotbar.SlotCount) _hud.Hotbar.Press(index);
        if (slot is null)
        {
            Refuse("Nothing there.");
            return;
        }

        if (key == 1)
        {
            bool on = !leader.AutoAttackOn;
            if (on && (leader.Target is null || leader.Target.Side != Side.Foes) && NearestFoe(leader) is Fighter nearest)
                _battle.Command(BattleCommand.Target(leader.Id, nearest.Id));
            _battle.Command(BattleCommand.AutoAttack(leader.Id, on));
            Play(SoundKeys.Press, null);
            if (on) Learn(TutorialCatalog.CombatAttack);
            return;
        }

        if (!slot.Unlocked || !slot.Usable)
        {
            Refuse(slot.WhyNot ?? "Not now.");
            return;
        }

        Play(SoundKeys.Press, null);
        if (key == Battle.PotionKey)
        {
            _battle.Command(BattleCommand.Potion(leader.Id));
            Learn(TutorialCatalog.CombatPotion);
            return;
        }
        if (key == Battle.SoulLinkKey)
        {
            _battle.Command(BattleCommand.SoulLink(leader.Id, leader.Target?.Side == Side.Foes ? leader.Target.Id : null));
            Learn(TutorialCatalog.CombatSoulLink);
            return;
        }

        SkillDefinition skill = slot.Skill;
        Fighter aimed = leader.Target;
        bool fits = aimed is not null && (skill.Target == SkillTarget.Foe ? aimed.Side == Side.Foes : skill.Target == SkillTarget.Ally && aimed.Side == Side.Party);
        _battle.Command(BattleCommand.Use(leader.Id, skill.Id, fits ? aimed.Id : null));
        Learn(TutorialCatalog.CombatAbility);
    }

    /// <summary>Tab: the next foe, nearest first, round to the nearest again after the furthest.</summary>
    private void NextFoe(Fighter leader)
    {
        List<Fighter> foes = _battle.Fighters
            .Where(each => each.Side == Side.Foes && each.IsStanding && !each.Vanished)
            .OrderBy(each => System.Numerics.Vector2.Distance(each.Position, leader.Position))
            .ToList();
        if (foes.Count == 0) return;
        int at = leader.Target is null ? -1 : foes.IndexOf(leader.Target);
        TargetOf(leader, foes[(at + 1) % foes.Count]);
    }

    /// <summary>T: your target's own target, an ally's foe or a foe's victim.</summary>
    private void Assist(Fighter leader)
    {
        if (leader.Target?.Target is Fighter theirs && theirs != leader) TargetOf(leader, theirs);
        else Refuse("Your target isn't aiming at anyone.");
    }

    /// <summary>The nearest foe still standing, or null.</summary>
    private Fighter NearestFoe(Fighter leader) => _battle.Fighters
        .Where(each => each.Side == Side.Foes && each.IsStanding && !each.Vanished)
        .OrderBy(each => System.Numerics.Vector2.Distance(each.Position, leader.Position))
        .FirstOrDefault();

    /// <summary>Says why something can't be done, over the bar, with the bar's dull knock.</summary>
    /// <param name="reason">Why, in a few words.</param>
    private void Refuse(string reason)
    {
        _refusal = reason;
        _refusalClock = RefusalSeconds;
        Play(SoundKeys.Denied, null);
    }
}
