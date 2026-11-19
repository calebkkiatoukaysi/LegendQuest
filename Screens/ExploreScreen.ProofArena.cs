using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;

namespace LegendQuest.Screens;

/// <summary>
/// What one bout of the proof run's Arena, Boss and Classes stages films (LegendQuestGame.ProofArena):
/// where its shots go, and which of the fight's moments it waits for.
/// </summary>
internal sealed class ArenaPlan
{
    /// <summary>The start of every shot's path, folder and all: "arena/gulls".</summary>
    public required string Prefix { get; init; }

    /// <summary>Whether the bout presses the leader's skills, 2 to 5, each once as soon as it can be used, with a shot of each.</summary>
    public bool PressSkills { get; init; }

    /// <summary>Whether it films the leader's ultimate, every second frame from the moment it starts, into seq/.</summary>
    public bool FilmUltimate { get; init; }

    /// <summary>A foe move it films from its telegraph, every second frame, into seq/, or null.</summary>
    public string FilmMove { get; init; }

    /// <summary>What the bout does before its fight: none (the save starts the fight), walk at an aggressive field, or click a field's foe.</summary>
    public ArenaStart Start { get; init; }

    /// <summary>The field's foe the bout points at and clicks, by npc id, for ArenaStart.Click.</summary>
    public string ClickAt { get; init; }

    /// <summary>Whether it waits after the fight for the field to fill again, with a shot of it.</summary>
    public bool WaitForRespawn { get; init; }

    /// <summary>Whether it opens your character sheet before the fight, for a shot of the class's kit, rank and class points.</summary>
    public bool SheetShot { get; init; }
}

/// <summary>How a bout's fight starts.</summary>
internal enum ArenaStart
{
    /// <summary>The save is made mid-fight, so the room starts it as it loads (ResumeSavedFight).</summary>
    Saved,

    /// <summary>The party walks east at an aggressive field until it comes at them.</summary>
    Walk,

    /// <summary>The pretend mouse goes over a field's foe and clicks it.</summary>
    Click,
}

/// <summary>
/// The fight's part of the proof run's Arena, Boss and Classes stages: playing one bout in this room,
/// and taking the shots that show the system (the thick of it with skills and numbers, the bar's
/// cooldowns, a telegraph's marker filling, a boss's phase changing, a downed friend helped up, the
/// panel at the end) and the frame runs of a foe's big move and an ultimate.
/// </summary>
/// <remarks>
/// The party's AI drives everyone, the leader included (Battle.LeaderAutoplay), on the run's lockstep
/// clock, so each shot is the frame the moment comes on. A shot is due a set time after the event
/// it's of (a telegraph's marker part-filled, a phase's banner up); one comes once a bout. The
/// class bouts press the leader's skills through the same code keys 2 to 5 use (UseSlot), each as
/// soon as it can go off. Nothing here changes the fight: it only looks, and presses what a player
/// could.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long the room gets to settle before a field bout starts walking or pointing, in frames.</summary>
    private const int ArenaSettle = 60;

    /// <summary>How long a bout's shot of the character sheet takes, opening and putting it away, in frames.</summary>
    private const int ArenaSheetFrames = 70;

    /// <summary>How long the party walks back from a field it's beaten, in frames, out of reach of it filling again.</summary>
    private const int ArenaStepBack = 150;

    /// <summary>How long the pretend mouse rests on a field's foe before the shot of it and the click, in frames.</summary>
    private const int ArenaHover = 30;

    /// <summary>How far into a fight its busy shot can be taken from, and the latest, in the fight's milliseconds; and how busy it has to be.</summary>
    private const int ArenaBusyFrom = 4000, ArenaBusyLatest = 25000, ArenaBusyCount = 5;

    /// <summary>How long a frame run is, in frames, and how often it saves one.</summary>
    private const int ArenaFilmFrames = 216, ArenaFilmEvery = 2;

    /// <summary>How long after a pressed skill goes off its shot is taken, in the fight's milliseconds: about when a skill's clip lands its blow.</summary>
    private const float ArenaSkillShotMs = 420f;

    /// <summary>The longest a bout presses for a skill that won't go, in the fight's milliseconds, before it gives up on it.</summary>
    private const int ArenaPressPatience = 15000;

    /// <summary>The bout being filmed, or null when the stage isn't one of the Arena's.</summary>
    private ArenaPlan _arena;

    /// <summary>The bout's clock in drawn frames, and its moment in the fight before its fight started.</summary>
    private int _arenaClock;

    /// <summary>Which moments have had their shot.</summary>
    private readonly HashSet<string> _arenaTaken = new();

    /// <summary>Shots waiting for their moment: when in the fight's shown time, and the shot.</summary>
    private readonly List<(float AtMs, string Shot)> _arenaDue = new();

    /// <summary>The run of frames being filmed: its name, how far in it is, or null.</summary>
    private (string Name, int Frame)? _arenaFilm;

    /// <summary>Whether this bout's fight has started, and the panel's clock once it's up.</summary>
    private bool _arenaFought;
    private int _arenaEnding;

    /// <summary>The skill slots still to press (2 to 5), the one pressed and waiting to go off, and since when.</summary>
    private readonly List<int> _arenaPresses = new();
    private int _arenaPressed;
    private string _arenaPressedSkill;
    private float _arenaPressedAt;

    /// <summary>How many phases the boss has changed in this bout.</summary>
    private int _arenaPhases;

    /// <summary>Whether the bout's done: its panel shot taken (and, for a field, its room filled again).</summary>
    internal bool ArenaDone { get; private set; }

    /// <summary>What the bout's fight was, by encounter id, for the run's notes.</summary>
    internal string ArenaFight => _encounter?.Id;

    /// <summary>One frame of a bout: whatever comes before its fight, the fight's moments, and after.</summary>
    /// <param name="plan">The bout.</param>
    /// <returns>The shot to take this frame, as a path in the run's folder, or null.</returns>
    internal string ArenaStep(ArenaPlan plan)
    {
        if (_arena != plan)
        {
            _arena = plan;
            _arenaClock = 0;
            _arenaTaken.Clear();
            _arenaDue.Clear();
            _arenaFilm = null;
            _arenaFought = false;
            _arenaEnding = 0;
            _arenaPhases = 0;
            _arenaPresses.Clear();
            if (plan.PressSkills) _arenaPresses.AddRange(new[] { 2, 3, 4, 5 });
            _arenaPressed = 0;
            ArenaDone = false;
            ProofFieldAggro = plan.Start == ArenaStart.Walk;
        }
        _arenaClock++;

        // The pretend mouse, as the story's visits have it: a click is the button down for a frame.
        if (_proofAim is Vector2 aim)
        {
            Vector2 screen = _camera.ToScreen(aim);
            ScreenManager.Input.ProofMouse(new Point((int)screen.X, (int)screen.Y), _proofPress);
        }
        _proofPress = false;

        if (InCombat) return ArenaFightStep(plan);
        if (!_arenaFought) return ArenaBefore(plan);
        return ArenaAfter(plan);
    }

    /// <summary>Before the fight: a field bout walks at its foes or points at one and clicks; a saved one waits for the room to start it.</summary>
    private string ArenaBefore(ArenaPlan plan)
    {
        if (plan.Start == ArenaStart.Saved || _arenaClock < ArenaSettle) return null;
        if (_arenaClock == ArenaSettle) return plan.Prefix + "-before.png";

        // The sheet, opened the way C opens it, and put away again.
        int sheet = plan.SheetShot ? ArenaSheetFrames : 0;
        if (plan.SheetShot && _arenaClock == ArenaSettle + 1) ScreenManager.AddScreen(new InventoryScreen(_game, InventoryPage.Character));
        if (plan.SheetShot && _arenaClock == ArenaSettle + ProofScreenSettles) return plan.Prefix + "-sheet.png";
        if (plan.SheetShot && _arenaClock == ArenaSettle + ProofScreenSettles + 1)
            foreach (StateManagement.GameScreen screen in ScreenManager.GetScreens())
                if (screen is InventoryScreen) screen.ExitScreen();
        if (_arenaClock < ArenaSettle + sheet) return null;

        if (plan.Start == ArenaStart.Walk)
        {
            _proofMove = new Vector2(1f, 0f);
            return null;
        }

        Npc foe = _npcs.Find(npc => npc.Definition.Id == plan.ClickAt);
        if (foe is null) return null;
        ProofAimAt(foe.Walker.Position - new Vector2(0f, foe.Walker.Size.Y / 2f));
        if (_arenaClock == ArenaSettle + sheet + ArenaHover) return plan.Prefix + "-pointing.png";
        if (_arenaClock == ArenaSettle + sheet + ArenaHover + 1) ProofClick();
        return null;
    }

    /// <summary>After a field fight's panel's put away: the party steps back from where the field stands, a shot of the room cleared, and then one of it filled again.</summary>
    private string ArenaAfter(ArenaPlan plan)
    {
        if (!plan.WaitForRespawn)
        {
            ArenaDone = true;
            return null;
        }

        if (_arenaTaken.Add("cleared"))
        {
            _arenaClock = 0;
            return null;
        }
        _proofMove = _arenaClock < ArenaStepBack ? new Vector2(-1f, 0f) : null;
        if (_arenaClock == ArenaStepBack + 10) return plan.Prefix + "-cleared.png";
        if (_arenaClock > ArenaStepBack + 10 && _fieldCleared.Count == 0 && _arenaTaken.Add("respawned"))
        {
            ProofNote($"{plan.Prefix}: the field filled again after {_arenaClock / 60f:F0} s of frames.");
            _arenaClock = 0;
            return null;
        }
        if (_arenaTaken.Contains("respawned") && _arenaClock == 20)
        {
            ArenaDone = true;
            return plan.Prefix + "-respawned.png";
        }
        return null;
    }

    /// <summary>One frame of the fight: the AI on, the frame runs, the shots that are due, the busy shot, the presses, and the panel's shot at the end.</summary>
    private string ArenaFightStep(ArenaPlan plan)
    {
        if (!_arenaFought)
        {
            _arenaFought = true;
            _proofMove = null;
            ProofLetGo();
            ProofAutoplay(true, 1f);
            _arenaDue.Add((700f, plan.Prefix + "-start.png"));
        }

        if (ProofFightOver)
        {
            if (!_result.Ready) return null;
            _arenaEnding++;
            if (_arenaEnding == ProofFightEnding) return $"{plan.Prefix}-{(FightWon ? "won" : "lost")}.png";
            if (_arenaEnding < ProofFightEnding + ProofFightDismiss) return null;
            if (plan.WaitForRespawn) ProofContinue();
            else ArenaDone = true;
            return null;
        }

        string shot = null;
        for (int i = 0; i < _arenaDue.Count; i++)
        {
            if (_arenaDue[i].AtMs > _shownMs) continue;
            shot = _arenaDue[i].Shot;
            _arenaDue.RemoveAt(i);
            break;
        }

        if (_arenaFilm is (string name, int frame))
        {
            _arenaFilm = frame + 1 >= ArenaFilmFrames ? null : (name, frame + 1);
            if (shot is null && frame % ArenaFilmEvery == 0) shot = $"seq/{name}-{frame / ArenaFilmEvery:000}.png";
        }

        if (shot is null && !_arenaTaken.Contains("fight") && _shownMs >= ArenaBusyFrom && (ProofBusy() >= ArenaBusyCount || _shownMs >= ArenaBusyLatest))
        {
            _arenaTaken.Add("fight");
            shot = plan.Prefix + "-fight.png";
        }

        ArenaPress();
        return shot;
    }

    /// <summary>Presses one of the leader's skills that's still to go, as key 2 to 5 would: the first of them that can be used now, with a foe targeted; gives up on one pressed that didn't go off after a while.</summary>
    private void ArenaPress()
    {
        if (_arenaPresses.Count == 0 && _arenaPressed == 0 || _battle.Leader is not Fighter leader || !leader.IsStanding) return;
        if (_arenaPressed != 0)
        {
            if (_shownMs - _arenaPressedAt < ArenaPressPatience) return;
            ProofNote($"{_arena.Prefix}: slot {_arenaPressed} was pressed and never went off.");
            _arenaPressed = 0;
        }
        if (_arenaPresses.Count == 0 || leader.Target is not { Side: Side.Foes }) return;

        IReadOnlyList<BarSlot> bar = _battle.Bar(leader);
        foreach (int key in _arenaPresses)
        {
            BarSlot slot = bar.FirstOrDefault(each => each.Key == key);
            if (slot is null || !slot.Unlocked)
            {
                ProofNote($"{_arena.Prefix}: slot {key} is empty or locked.");
                _arenaPresses.Remove(key);
                return;
            }
            if (!slot.Usable || slot.GcdLeftMs > 0) continue;

            _arenaPresses.Remove(key);
            _arenaPressed = key;
            _arenaPressedSkill = slot.Skill.Id;
            _arenaPressedAt = _shownMs;
            UseSlot(leader, key - 1);
            return;
        }
    }

    /// <summary>Notes the events a bout's shots are of, and sets each shot for the moment it shows best.</summary>
    /// <param name="happened">What the engine did, as it's shown.</param>
    private void ArenaSaw(BattleEvent happened)
    {
        if (_arena is null) return;
        string prefix = _arena.Prefix;
        Fighter actor = FighterOf(happened.ActorId);
        bool leader = actor is not null && actor == _battle.Leader;
        SkillDefinition skill = happened.SkillId is null ? null : SkillOf(happened.SkillId);

        switch (happened.Kind)
        {
            case BattleEventKind.Telegraph:
                if (happened.Area is not null) Due("telegraph", happened.AtMs + happened.DurationMs * 0.6f, prefix + "-telegraph.png");
                if (_arena.FilmMove is string move && happened.SkillId == move && _arenaFilm is null && _arenaTaken.Add("film-" + move))
                    _arenaFilm = (Short(prefix) + "-" + move.Replace("foe-", ""), 0);
                break;

            case BattleEventKind.PhaseChanged:
                _arenaPhases++;
                Due("phase-" + _arenaPhases, happened.AtMs + 600f, $"{prefix}-phase-{_arenaPhases + 1}.png");
                break;

            case BattleEventKind.Downed when actor?.Side == Side.Party || FighterOf(happened.TargetId)?.Side == Side.Party:
                Due("downed", happened.AtMs + 500f, prefix + "-downed.png");
                break;

            case BattleEventKind.HelpStarted:
                Due("help", happened.AtMs + 900f, prefix + "-help.png");
                break;

            case BattleEventKind.SoulLinkStrike:
                Due("soul-link", happened.AtMs + 250f, prefix + "-soul-link.png");
                break;

            case BattleEventKind.SkillUsed when leader && _arenaPressed != 0 && happened.SkillId == _arenaPressedSkill:
                // Its shot a moment on, when its clip's at its blow (or its cast's under way).
                Due("skill-" + _arenaPressed, happened.AtMs + ArenaSkillShotMs, $"{prefix}-skill-{_arenaPressed}.png");
                _arenaPressed = 0;
                break;

            case BattleEventKind.ActionStarted when leader && skill is not null && skill.Kind != SkillKind.Auto:
                Due("cooldowns", happened.AtMs + 350f, prefix + "-cooldowns.png");
                if (skill.Ultimate && _arena.FilmUltimate && _arenaFilm is null && _arenaTaken.Add("film-ultimate"))
                    _arenaFilm = (Short(prefix) + "-ultimate-" + skill.Id, 0);
                break;
        }
    }

    /// <summary>Sets a shot for a moment of the fight, once a bout.</summary>
    private void Due(string moment, float atMs, string shot)
    {
        if (_arenaTaken.Add(moment)) _arenaDue.Add((atMs, shot));
    }

    /// <summary>A prefix's last part, for a frame run's name: "boss/thornmother" is "thornmother".</summary>
    private static string Short(string prefix) => prefix[(prefix.LastIndexOf('/') + 1)..];
}
