using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components.Realtime;
using LegendQuest.Tutorial;

namespace LegendQuest.Screens;

/// <summary>
/// The fight's lessons (Story/11-combat.md §7.2), one at a time at the moment each is for: targeting,
/// the auto attack, your first skill, the cooldowns and the global cooldown, Focus and your class's
/// way of filling it, the potion, a foe's telegraph, the Soul-Link; and the allies, going down,
/// walking and the win. Each holds the fight while its card's up, with a gold pulse on what it's about.
/// </summary>
/// <remarks>
/// <para>
/// The tutorial fight at sea teaches them in that order, each when its moment comes and the last has
/// come and gone: a foe to pick while you've none; the swing once you have; the skill once your
/// first blow's landed; the cooldowns just after the skill goes off; Focus a moment later, or as soon
/// as a skill's too dear; the potion the first time you're hurt, or a while after; the telegraph on a
/// foe's first wind-up, the Soul-Link the first time the sigil fills. Any other fight brings the same
/// tips the first time their moment comes in it, once a game, like every tip. A lesson whose card
/// holds the fight (TutorialTip.HoldsFight) still lets the keys and the mouse through, so doing what
/// it teaches (clicking a foe, pressing 2) puts it away and the fight goes on; so does GOT IT.
/// </para>
/// <para>
/// The proof run keeps tips quiet, but in the tutorial fight it puts its first three lessons up
/// itself (ProofLessons), takes their shots and does what each says, as a player would
/// (ExploreScreen.ProofFights).
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long after your first skill goes off the cooldowns lesson comes, so the sweep's on the bar; and after that, how long before Focus comes if no skill's too dear by then. Milliseconds of the fight.</summary>
    private const int CooldownLessonAfterMs = 400, FocusLessonAfterMs = 2500;

    /// <summary>How hurt you have to be for the potion lesson, as a share of your hit points, and how long after Focus it comes anyway.</summary>
    private const float PotionLessonHurt = 0.6f;
    private const int PotionLessonAfterMs = 8000;

    /// <summary>How long after you've a target the skill lesson comes if no swing of yours has landed yet, and how long into a fight the tip about walking waits. Milliseconds.</summary>
    private const int SkillLessonAfterMs = 3000, MoveLessonAfterMs = 12000;

    /// <summary>The lessons the proof run puts up itself in the tutorial fight, in order.</summary>
    private static readonly string[] ProofLessons = { TutorialCatalog.CombatTarget, TutorialCatalog.CombatAttack, TutorialCatalog.CombatAbility };

    /// <summary>The gold pulse round what a lesson's about: how wide its edge is, how far its glow spreads, and how fast it breathes.</summary>
    private const int PulseEdge = 3, PulseSpread = 10;
    private const float PulseRate = 1.6f;

    /// <summary>The lessons done in this fight.</summary>
    private readonly HashSet<string> _learned = new();

    /// <summary>The last lesson put in line, so the next waits until it's come up and gone; and the fight's moment it was.</summary>
    private string _lessonAsked;
    private float _lessonAskedMs;

    /// <summary>The moments the lessons wait for: when you first had a foe targeted, whether a swing of yours has landed, when your first skill went off (-1 before), and who the lesson that's up is about (a foe winding up, a friend down).</summary>
    private float _targetedMs = -1f;
    private bool _swingLanded;
    private float _skillMs = -1f;
    private string _lessonAbout;

    /// <summary>Whether the last lesson asked for is still to come up, or is up.</summary>
    private bool LessonWaiting => _lessonAsked is string id && (!_tutorials.Seen(id) || _tutorials.Current?.Id == id);

    /// <summary>Whether the proof run's putting the tutorial fight's first lessons up itself.</summary>
    private bool ProofLessonsOn => ProofSeed is not null && _encounter is { Tutorial: true };

    /// <summary>Whether the lesson that's up holds the fight.</summary>
    private bool LessonHolds => _tutorials.Current is { HoldsFight: true };

    /// <summary>Starts the lessons for a fight: nothing done yet, your class's way of filling Focus for its lesson, the tip about fights first, and the one about allies if it has any.</summary>
    private void BeginLessons()
    {
        _learned.Clear();
        _lessonAsked = null;
        _lessonAbout = null;
        _targetedMs = -1f;
        _swingLanded = false;
        _skillMs = -1f;
        if (_battle.Leader is Fighter leader) _tutorials.Filling[TutorialCatalog.FocusSlot] = FocusWay(leader.FocusModel);
        Lesson(TutorialCatalog.CombatStart);
        if (_encounter.Guests.Count > 0 || _encounter.Supports.Count > 0) Lesson(TutorialCatalog.CombatAllies);
    }

    /// <summary>How a class fills its Focus, in a sentence for the lesson (10-classes.md §1.2).</summary>
    private static string FocusWay(FocusModel model) => model switch
    {
        FocusModel.Momentum => "Yours fills as your blows land, and more on a critical.",
        FocusModel.Opportunist => "Yours fills as you hit, crit and dodge.",
        FocusModel.Bulwark => "Yours fills as you're hit and as you block.",
        FocusModel.Wellspring => "Yours fills by itself, steadily.",
        FocusModel.Devotion => "Yours fills slowly, and with every heal you land.",
        FocusModel.Wild => "Yours fills slowly, and as your spells tick.",
        FocusModel.Pact => "Yours fills as you hit, fastest on a foe you've cursed.",
        FocusModel.Tempo => "Yours starts nearly full and fills as you hit.",
        FocusModel.Blood => "Yours fills by itself; some skills cost blood as well.",
        _ => "Each class fills it its own way.",
    };

    /// <summary>Puts a lesson in line, if it's not been seen and isn't done. In the tutorial fight under the proof run, its first lessons go up through the run's own door, since tips are quiet there.</summary>
    /// <param name="tipId">The tip, by its id in TutorialCatalog.</param>
    /// <param name="about">Who it's about, for its pulse: a foe winding up, a friend who's down; or null.</param>
    /// <returns>True if it was put in line.</returns>
    private bool Lesson(string tipId, string about = null)
    {
        if (_learned.Contains(tipId) || _tutorials.Seen(tipId)) return false;
        if (ProofLessonsOn && ProofLessons.Contains(tipId)) _tutorials.ProofShow(tipId);
        else if (!ShowTutorial(tipId)) return false;
        _lessonAsked = tipId;
        _lessonAskedMs = _shownMs;
        _lessonAbout = about;
        return true;
    }

    /// <summary>Says a lesson's been done: its tip goes if it's up and never comes if it isn't.</summary>
    /// <param name="tipId">The tip, by its id in TutorialCatalog.</param>
    private void Learn(string tipId)
    {
        if (!_learned.Add(tipId)) return;
        TutorialDone(tipId);
    }

    /// <summary>Puts in line the lesson the fight's at, once the last has come and gone, in the tutorial's order.</summary>
    private void GuideFight()
    {
        if (!(TutorialsOn || ProofLessonsOn) || LessonWaiting || _battle.Leader is not Fighter leader || leader.IsOut) return;

        if (leader.Target is not { Side: Side.Foes })
        {
            Lesson(TutorialCatalog.CombatTarget);
            return;
        }
        if (_targetedMs < 0f) _targetedMs = _shownMs;
        Learn(TutorialCatalog.CombatTarget);
        if (Lesson(TutorialCatalog.CombatAttack)) return;

        if ((_swingLanded || _shownMs - _targetedMs >= SkillLessonAfterMs) && Lesson(TutorialCatalog.CombatAbility)) return;
        if (_skillMs >= 0f && _shownMs - _skillMs >= CooldownLessonAfterMs && Lesson(TutorialCatalog.CombatCooldown)) return;

        bool afterCooldowns = _tutorials.Seen(TutorialCatalog.CombatCooldown) && _shownMs - _lessonAskedMs >= FocusLessonAfterMs;
        bool tooDear = _battle.Bar(leader).Any(slot => slot.Unlocked && slot.Skill is not null && !slot.Affordable);
        if ((afterCooldowns || tooDear && _tutorials.Seen(TutorialCatalog.CombatAbility)) && Lesson(TutorialCatalog.CombatFocus)) return;

        bool hurt = leader.Hp < leader.MaxHp * PotionLessonHurt;
        bool afterFocus = _tutorials.Seen(TutorialCatalog.CombatFocus) && _shownMs - _lessonAskedMs >= PotionLessonAfterMs;
        if ((hurt || afterFocus) && Lesson(TutorialCatalog.CombatPotion)) return;

        if (_battle.NowMs >= MoveLessonAfterMs && _tutorials.Seen(TutorialCatalog.CombatPotion)) Lesson(TutorialCatalog.CombatMove);
    }

    /// <summary>The moments in what the engine did: your first swing landing and your first skill (for the lessons that wait on them), and the lessons an event is the moment for (a foe's first wind-up, your first potion).</summary>
    private void NoticeForLessons(BattleEvent happened)
    {
        Fighter actor = FighterOf(happened.ActorId);
        bool yours = actor is not null && actor == _battle.Leader;
        switch (happened.Kind)
        {
            case BattleEventKind.Damaged or BattleEventKind.Hit when yours:
                _swingLanded = true;
                break;
            case BattleEventKind.ActionStarted when yours && _skillMs < 0f && SkillOf(happened.SkillId) is { Kind: not SkillKind.Auto }:
                _skillMs = happened.AtMs;
                break;
            case BattleEventKind.Telegraph when actor?.Side == Side.Foes && !LessonWaiting:
                Lesson(TutorialCatalog.CombatTelegraph, actor.Id);
                break;
            case BattleEventKind.PotionDrunk when yours:
                Learn(TutorialCatalog.CombatPotion);
                break;
        }
    }

    /// <summary>The gold pulse round what the lesson that's up is about: the foe to click, slot 1 and the swing, slot 2, the skills' sweeps, the Focus bar, the potion, the foe winding up, the sigil, a friend who's down. Goes in a point sampled batch over the HUD's art.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    private void DrawLessonPulse(SpriteBatch spriteBatch, float alpha)
    {
        if (_tutorials.Current is not { HoldsFight: true } tip || !_tutorialPanel.Showing || _battle?.Leader is not Fighter leader) return;
        Rectangle[] slots = Hotbar.Slots();
        var marks = new List<Rectangle>();
        switch (tip.Id)
        {
            case TutorialCatalog.CombatTarget:
                if (NearestFoe(leader) is Fighter foe && FigureOnScreen(foe.Id) is Rectangle figure) marks.Add(figure);
                break;
            case TutorialCatalog.CombatAttack:
                marks.Add(slots[0]);
                marks.Add(Hotbar.SwingArea);
                break;
            case TutorialCatalog.CombatAbility:
                marks.Add(slots[1]);
                break;
            case TutorialCatalog.CombatCooldown:
                for (int i = 1; i <= 4; i++) marks.Add(slots[i]);
                break;
            case TutorialCatalog.CombatFocus:
                marks.Add(_hud.Player.FocusArea(_hud.Kit));
                break;
            case TutorialCatalog.CombatPotion:
                marks.Add(slots[Battle.PotionKey - 1]);
                break;
            case TutorialCatalog.CombatSoulLink:
                marks.Add(slots[Hotbar.SigilIndex]);
                break;
            case TutorialCatalog.CombatTelegraph or TutorialCatalog.CombatDowned:
                if (FigureOnScreen(_lessonAbout) is Rectangle about) marks.Add(about);
                break;
        }

        HudKit kit = _hud.Kit;
        float pulse = kit.Pulse(PulseRate, 0.35f) * alpha;
        foreach (Rectangle mark in marks)
        {
            Rectangle glow = mark;
            glow.Inflate(PulseSpread * 2, PulseSpread * 2);
            spriteBatch.Draw(kit.Glow, glow, Palette.Gold * (0.45f * pulse));
            Rectangle edge = mark;
            edge.Inflate(PulseEdge + 2, PulseEdge + 2);
            kit.Edge(spriteBatch, edge, PulseEdge, Palette.Gold * pulse);
        }
    }

    /// <summary>Where someone in the fight's figure is on the screen, or null if they're not drawn.</summary>
    /// <param name="id">Their id in the fight.</param>
    private Rectangle? FigureOnScreen(string id)
    {
        if (FighterWalker(id) is not WalkerSprite walker) return null;
        Vector2 top = _camera.ToScreen(walker.Position - new Vector2(walker.Size.X / 2f, walker.Size.Y));
        Vector2 bottom = _camera.ToScreen(walker.Position + new Vector2(walker.Size.X / 2f, 0f));
        return new Rectangle((int)top.X, (int)top.Y, (int)(bottom.X - top.X), (int)(bottom.Y - top.Y));
    }
}
