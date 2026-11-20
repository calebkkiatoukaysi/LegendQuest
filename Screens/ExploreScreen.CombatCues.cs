using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Audio;
using LegendQuest.Characters;
using LegendQuest.Characters.Motion;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Dialogue;
using LegendQuest.Components;
using LegendQuest.Components.Realtime;
using LegendQuest.Items;
using LegendQuest.Rules;
using LegendQuest.Tutorial;

namespace LegendQuest.Screens;

/// <summary>
/// What each thing that happens in a fight looks and sounds like, played on whoever it happens to
/// the moment it happens: the clips, the markers on the floor, what flies, the impacts, the numbers,
/// the shake, the sounds and the log's line.
/// </summary>
/// <remarks>
/// <para>
/// There's no queue of beats: every fighter's figure plays its own clips (Story/11-combat.md §8), so
/// four people can be mid-swing at once. A move starts its clip when the engine starts it
/// (ActionStarted) and the engine has timed its blow to the clip's impact, so the hit that follows
/// is played on that frame: the hit-stop on both, the white flash, the squash and the knock
/// (WalkerSprite.TakeHit), sparks where the weapon meets the body, the weapon's or the school's
/// sound with the critical's over it, the number, and a shake only for a heavy skill, a critical, a
/// kill, a big blow on you, or a boss's slam (never an auto attack). A swing's whoosh starts just
/// before its blow, by its family's lead (CombatSounds.SwingLeadMs). A foe's telegraph holds its
/// wind-up with a glint and a marker on the floor that fills until the blow and resolves on it.
/// </para>
/// <para>
/// A reaction clip (a flinch, a block, a knockdown, a death) carries a "hit" of its own, the frame
/// it's struck on: only an action's "hit" is a blow (Clip.IsAction), and the screen takes blows from
/// the engine's events, not the clips, so the clip events it listens to are the landings (dust) and
/// the effects named in the clips (fx). A flinch isn't played on someone mid-move, so a blow taken
/// while swinging doesn't cut the swing short; the flash, squash and knock still show it.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far a dodge sidesteps, in world pixels, and how long it takes there and back.</summary>
    private const float DodgeDistance = 4f;
    private const float DodgeSeconds = 0.3f;

    /// <summary>How long a block's hit-stop holds both, in frames.</summary>
    private const int BlockFrames = 3;

    /// <summary>How much of a blow, as a share of the most health someone has, is a big one on you: it shakes the screen.</summary>
    private const float BigHitShare = 0.2f;

    /// <summary>How long a fallen foe lies before it fades in motes, and how long the fade takes.</summary>
    private const float LieSeconds = 0.5f;
    private const float FadeOutSeconds = 0.6f;

    /// <summary>How faint one of the party who's fallen for the rest of the fight is drawn, on the floor.</summary>
    private const float FallenOpacity = 0.6f;

    /// <summary>How long a line said mid-fight stays over the speaker's head, plus a bit for every letter.</summary>
    private const float BarkSeconds = 2.4f;
    private const float BarkSecondsPerLetter = 0.05f;

    /// <summary>How long a support's arrow or stone takes to reach its foe, and how far over the feet the chest is, as a share of the figure.</summary>
    private const float SupportFlightSeconds = 0.18f;
    private const float ChestShare = 0.55f;

    /// <summary>How far over someone's head the numbers start, in world pixels.</summary>
    private const float NumbersRise = 4f;

    /// <summary>How loud a support's shot and a foe's buff are against the rest.</summary>
    private const float QuietVolume = 0.6f;

    /// <summary>The colour a foe fades away in, and the smoke a vanishing fighter goes in.</summary>
    private static readonly Color Fading = new(190, 170, 220);
    private static readonly Color Smoke = new(150, 150, 160);

    /// <summary>The markers on the floor still filling, by the fighter whose telegraph or cast they are, to resolve or cancel.</summary>
    private readonly Dictionary<string, int> _cueMarks = new();

    /// <summary>Whether the win's been shown: a swing or an arrow still timed to sound after it (a blow that will never land) stays quiet.</summary>
    private bool _wonCued;

    /// <summary>The keys of your bar's slots on a cooldown long enough to chime for when it ends, as of the last frame.</summary>
    private readonly HashSet<string> _cooling = new(StringComparer.Ordinal);

    /// <summary>The shortest cooldown worth a chime when it ends, in seconds: a 3 s filler would chime all fight.</summary>
    private const float ChimeLeastSeconds = 5f;

    /// <summary>A soft chime the moment one of your bar's longer cooldowns comes off, so you know it's back without looking.</summary>
    /// <param name="view">The HUD as it's about to be drawn.</param>
    private void ChimeReady(HudView view)
    {
        if (view?.Slots is null) return;
        foreach (SlotView slot in view.Slots)
        {
            if (slot.Key is null) continue;
            if (slot.Cooling)
            {
                if (slot.CooldownTotal >= ChimeLeastSeconds) _cooling.Add(slot.Key);
            }
            else if (_cooling.Remove(slot.Key) && !_wonCued) Play(SoundKeys.CooldownReady, null, QuietVolume);
        }
    }

    /// <summary>Plays one thing the engine did, on whoever it happened to, and writes its log line.</summary>
    /// <param name="happened">The event.</param>
    private void Cue(BattleEvent happened)
    {
        Fighter actor = FighterOf(happened.ActorId);
        Fighter target = FighterOf(happened.TargetId);
        WalkerSprite by = FighterWalker(happened.ActorId);
        WalkerSprite on = FighterWalker(happened.TargetId);
        SkillDefinition skill = SkillOf(happened.SkillId);
        NoticeForLessons(happened);
        ArenaSaw(happened);

        switch (happened.Kind)
        {
            case BattleEventKind.ActionStarted:
                CueAction(happened, actor, by, on, skill);
                break;

            case BattleEventKind.Telegraph:
                CueTelegraph(happened, actor, by);
                break;

            case BattleEventKind.CastStarted:
                CueCast(happened, actor, by, skill);
                break;

            case BattleEventKind.CastInterrupted:
                CueInterrupted(happened, by);
                break;

            case BattleEventKind.ProjectileLaunched:
                CueProjectile(happened, actor, by, on, skill);
                break;

            case BattleEventKind.Damaged:
                CueDamage(happened, actor, target, by, on, skill);
                break;

            case BattleEventKind.Absorbed:
                if (on is not null) _popups.Avoided(happened.TargetId, NumbersAt(on), "ABSORBED");
                break;

            case BattleEventKind.Missed:
            case BattleEventKind.Dodged:
            case BattleEventKind.Blocked:
                CueAvoided(happened, by, on);
                break;

            case BattleEventKind.Healed:
                CueHealed(happened, target, on);
                break;

            case BattleEventKind.AuraApplied:
                CueAura(happened, actor, target, on);
                break;

            case BattleEventKind.KnockedDown:
                if (on is null) break;
                on.PlayMove("knockdown", hold: true);
                Play(SoundKeys.Knockdown, on);
                break;

            case BattleEventKind.GotUp:
                if (by is not null && actor is { IsDown: false, IsDead: false }) by.PlayMove("getup");
                break;

            case BattleEventKind.Downed:
                CueDowned(target, on);
                break;

            case BattleEventKind.DeathSave:
                if (on is null || happened.Amount <= 0) break;
                bool made = happened.Flags.HasFlag(HitFlags.Save);
                Callout(on, $"DEATH SAVE  {happened.Amount}", made ? Palette.Gold : Palette.Ember);
                break;

            case BattleEventKind.Revived:
                if (by is not null && by != on && by.Move is { Name: "channel" }) by.StopMove();
                CueRevived(happened, target, on);
                break;

            case BattleEventKind.Died:
                CueDied(target, on);
                break;

            case BattleEventKind.FellBack:
                CueFellBack(actor, by);
                break;

            case BattleEventKind.Joined:
                CueJoined(happened, actor);
                break;

            case BattleEventKind.Vanished:
                if (by is null) break;
                _effects.Motes(ChestOf(by), new Point(20, 26), Smoke, false);
                FadeTo(by, 0f, 0.2f);
                break;

            case BattleEventKind.SupportActed:
                CueSupport(happened);
                break;

            case BattleEventKind.PhaseChanged:
                CuePhase(happened, actor, by);
                break;

            case BattleEventKind.Bark:
                CueBark(happened, actor, by);
                break;

            case BattleEventKind.SoulLinkCharged:
                CueCharge(happened, by);
                break;

            case BattleEventKind.SoulLinkStrike:
                Play(SoundKeys.SoulLink, on);
                if (on is not null) _effects.Motes(ChestOf(on) - new Vector2(0f, 24f), new Point(14, 12), Palette.Gold, true);
                if (by is not null) _effects.Glint(HeadOf(by), Palette.Gold);
                break;

            case BattleEventKind.FightWon:
                CueVictory();
                break;

            case BattleEventKind.FightLost:
                _log.Add(happened.Text ?? "THE FIGHT IS LOST.", Palette.Ember);
                Play(SoundKeys.Defeat, null);
                break;

            case BattleEventKind.Armed:
                if (by is not null) _effects.Glint(HeadOf(by) + new Vector2(0f, 8f), Palette.Gold);
                break;

            case BattleEventKind.ReactionFired:
                if (by is not null && skill is not null) Callout(by, skill.Name.ToUpperInvariant(), Palette.Gold);
                break;

            case BattleEventKind.Taunted:
                if (on is not null) Callout(on, "TAUNTED", Palette.Ember);
                break;

            case BattleEventKind.Denied:
                if (actor is not null && actor == _battle.Leader && !_proofAutoplay) Refuse(happened.Text ?? "Not now.");
                break;

            case BattleEventKind.Transformed:
                if (by is null) break;
                _effects.Motes(ChestOf(by), new Point(22, 30), CombatStyle.Nature, false);
                by.WeaponKind = WeaponKinds.Clip(actor?.Form?.Weapon ?? actor?.WeaponKind ?? WeaponKind.Unarmed);
                break;

            case BattleEventKind.HelpStarted:
                if (by is null) break;
                by.PlayMove("channel");
                Callout(by, "HELPING", Palette.Gold);
                break;

            case BattleEventKind.PotionDrunk:
                if (by is null) break;
                if (by.Move is not { IsAction: true }) by.PlayMove("drink-potion");
                Play(SoundKeys.Potion, by);
                break;

            case BattleEventKind.Stole:
                if (by is not null) _effects.Glint(ChestOf(by), Palette.Gold);
                break;
        }

        if (happened.Text is not null && Logged(happened.Kind)) Say(happened.Text, actor);
    }

    // The moves.

    /// <summary>A move starting: its clip on the mover, facing whoever it's aimed at; the swing's whoosh timed to peak on the blow; for a spell the school's cast and the glow in the hand until it leaves.</summary>
    private void CueAction(BattleEvent happened, Fighter actor, WalkerSprite by, WalkerSprite on, SkillDefinition skill)
    {
        if (by is null || actor is null) return;
        if (_cueMarks.Remove(actor.Id, out int telegraphed)) AtShown(happened.ImpactMs, () => _effects.Resolve(telegraphed));
        if (on is not null && on != by) by.Face(on.Position);

        Clip clip = by.PlayMove(happened.Motion ?? "strike-one-hand", restart: true);
        if (clip is not null) _moves[actor.Id] = (clip, happened.AtMs);

        string school = SchoolOf(skill);
        bool potion = happened.SkillId == Battle.Potion.Id;
        bool weapon = !potion && (skill is null || skill.Weapon || skill.Id == SkillCatalog.FoeAttack || happened.Motion?.StartsWith("auto-", StringComparison.Ordinal) == true);
        if (school is not null && !weapon)
        {
            Play(CombatSounds.Cast(school), by);
            float charge = MathF.Max(0.05f, (happened.ImpactMs - happened.AtMs) / 1000f);
            Element element = CombatEffects.ElementOf(school);
            WalkerSprite caster = by;
            _effects.ChargeIn(() => caster.BonePosition("hand_near"), element, charge);
            return;
        }

        // A heal, a buff or the potion swings nothing: their own cues have their sounds.
        if (!weapon && !Strikes(skill)) return;
        string family = FamilyOf(actor);
        if (family is "bow" or "crossbow") return;
        int lead = CombatSounds.SwingLeadMs(family);
        string swing = CombatSounds.Swing(family);
        WalkerSprite swinger = by;
        AtShown(happened.ImpactMs - lead, () => { if (!_wonCued) Play(swing, swinger); });
    }

    /// <summary>A foe winding up a big move: its held coil (the clip names it), a glint, the warning sound, and the marker on the floor filling until the blow lands, if it's aimed at a piece of the floor.</summary>
    private void CueTelegraph(BattleEvent happened, Fighter actor, WalkerSprite by)
    {
        if (by is null || actor is null) return;
        if (happened.Motion is string coil) by.PlayMove(coil, restart: true, hold: true);
        _effects.Glint(HeadOf(by) + new Vector2(actor.Facing < 0 ? -6f : 6f, 6f));
        Play(SoundKeys.Telegraph, by);
        if (happened.Area is BattleArea area && MarkFor(area, happened.DurationMs / 1000f) is int mark) _cueMarks[actor.Id] = mark;
    }

    /// <summary>A cast with a cast time starting: the caster's channel, the school's cast sound, the glow in the hand, and a marker for one aimed at the floor.</summary>
    private void CueCast(BattleEvent happened, Fighter actor, WalkerSprite by, SkillDefinition skill)
    {
        if (by is null || actor is null) return;
        by.PlayMove("channel", restart: true);
        string school = SchoolOf(skill) ?? "force";
        Play(CombatSounds.Cast(school), by);
        WalkerSprite caster = by;
        _effects.ChargeIn(() => caster.BonePosition("hand_near"), CombatEffects.ElementOf(school), happened.DurationMs / 1000f);
        if (happened.Area is BattleArea { Shape: not AreaKind.Room } area && skill?.Area is not (AreaShape.Single or null)
            && MarkFor(area, happened.DurationMs / 1000f) is int mark)
            _cueMarks[actor.Id] = mark;
    }

    /// <summary>A cast or a wind-up stopped: back to the ready stance, its marker gone, and the word over them.</summary>
    private void CueInterrupted(BattleEvent happened, WalkerSprite by)
    {
        if (happened.ActorId is string id && _cueMarks.Remove(id, out int mark)) _effects.Cancel(mark);
        if (by is null) return;
        if (happened.SkillId != "help") Callout(by, "INTERRUPTED", Palette.Ember);
        by.StopMove();
    }

    /// <summary>The marker on the floor for a piece of it a blow will land on, filling over the wind-up: a ring, a line or a cone, in the warning's colour. Null for the whole room.</summary>
    private int? MarkFor(BattleArea area, float seconds)
    {
        Vector2 at = ToScreen(area.Center), to = ToScreen(area.To);
        return area.Shape switch
        {
            AreaKind.Ring => _effects.Mark(MarkerShape.Ring, at, at, area.Radius, seconds),
            AreaKind.Line => _effects.Mark(MarkerShape.Line, at, to, Vector2.Distance(at, to), seconds, width: MathF.Max(8f, area.Width)),
            AreaKind.Cone => _effects.Mark(MarkerShape.Cone, at, to, area.Radius, seconds, halfAngle: MathHelper.ToRadians(area.Width / 2f)),
            _ => null,
        };
    }

    /// <summary>Something leaving the hand: an arrow or a bolt, a bolt of a school, or a beam of lightning, timed to land on the tick the engine lands it; a bow's shot sounds as it goes.</summary>
    private void CueProjectile(BattleEvent happened, Fighter actor, WalkerSprite by, WalkerSprite on, SkillDefinition skill)
    {
        float seconds = MathF.Max(0.02f, (happened.ImpactMs - happened.AtMs) / 1000f);
        Vector2 from = by?.BonePosition("hand_near") ?? ToScreen(happened.Position) - new Vector2(0f, 20f);
        // An area's bolt (a Fireball) flies to the middle of where the engine aims it, not at one of its victims.
        bool area = AreaBolt(skill) && happened.Area is not null;
        Vector2 to = on is not null && !area ? ChestOf(on) : ToScreen(happened.Area?.To ?? happened.Position) - new Vector2(0f, 12f);
        DamageType type = happened.DamageType;

        if (type is DamageType.Piercing or DamageType.Slashing or DamageType.Bludgeoning)
        {
            _effects.Launch(CombatStyle.Arrow, from, to, seconds, null);
            string family = actor is null ? "bow" : FamilyOf(actor);
            Play(CombatSounds.Swing(family is "bow" or "crossbow" ? family : "bow"), by);
            return;
        }

        Element element = CombatEffects.ElementOf(type);
        if (area)
        {
            // It goes off as a whole: a burst the size of the area where it lands, and the area's kick.
            _effects.Projectile(element, from, to, seconds, 0f, bursts: false);
            AtShown(happened.ImpactMs, () =>
            {
                _effects.Burst(to, element, AreaBurstSize);
                _effects.Shaker.Kick(ShakeCause.AreaSpell, false, _camera.ToScreen(to));
            });
            return;
        }
        if (element == Element.Lightning) _effects.BeamFrom(element, from, to, MathF.Max(seconds, 0.12f));
        else _effects.Projectile(element, from, to, seconds, skill?.Id.Contains("blasting") == true ? 26f : 0f, bursts: true);
    }

    /// <summary>How much bigger than a bolt's own an area bolt's burst is (the Moves proof stages Fireball at it).</summary>
    private const float AreaBurstSize = 2.2f;

    /// <summary>Whether a skill's bolt goes off over an area (a Fireball), rather than on the one it's thrown at.</summary>
    private static bool AreaBolt(SkillDefinition skill) =>
        skill?.Projectile is not null && skill.Area is AreaShape.AllFoes or AreaShape.Ring or AreaShape.Row or AreaShape.Line or AreaShape.Neighbours or AreaShape.Front;

    // The blows.

    /// <summary>
    /// Damage landing, on the tick the engine landed it (the clip's impact frame): the hit-stop,
    /// flash, squash and knock, sparks at the contact point (in the damage's colour for a foe's chill
    /// or fiery touch) or the school's burst, the sound with the critical's over it, the number, the
    /// flinch (unless they're mid-move), and the shake its cause earns. Only a skill's spell washes
    /// its target in the school's colour; an auto attack's never does, or a foe whose every touch is
    /// cold would leave you pale the whole fight. A tick of something over time is only its small
    /// tinted number.
    /// </summary>
    private void CueDamage(BattleEvent happened, Fighter actor, Fighter target, WalkerSprite by, WalkerSprite on, SkillDefinition skill)
    {
        if (on is null || target is null || happened.Amount <= 0 && !happened.Flags.HasFlag(HitFlags.Kill)) return;

        HitFlags flags = happened.Flags;
        bool taken = target.Side == Side.Party;
        Vector2 numbers = NumbersAt(on);
        Element element = CombatEffects.ElementOf(happened.DamageType);

        if (flags.HasFlag(HitFlags.Tick))
        {
            _popups.Tick(target.Id, numbers, happened.Amount, element == Element.Physical ? DamagePopups.TakenColour : CombatEffects.ColourOf(element));
            on.FlashTint(CombatEffects.ColourOf(element == Element.Physical ? Element.Blood : element), 8);
            if (target == _battle.Leader) Play(SoundKeys.AuraTick, on);
            return;
        }

        // A blow from a support has no clip to land on: it lands with its arrow.
        if (actor is null && _supportShot.Remove(target.Id, out float lands) && lands > _shownMs)
        {
            AtShown(lands, () => Land(happened, null, target, null, on, skill, numbers, taken));
            return;
        }
        Land(happened, actor, target, by, on, skill, numbers, taken);
    }

    /// <summary>One blow landing on someone: everything CueDamage says, at once.</summary>
    private void Land(BattleEvent happened, Fighter actor, Fighter target, WalkerSprite by, WalkerSprite on, SkillDefinition skill, Vector2 numbers, bool taken)
    {
        HitFlags flags = happened.Flags;
        bool critical = flags.HasFlag(HitFlags.Crit), kill = flags.HasFlag(HitFlags.Kill), auto = flags.HasFlag(HitFlags.Auto);
        bool heavy = !auto && (flags.HasFlag(HitFlags.Big) || skill is not null && skill.Id != SkillCatalog.FoeAttack);
        // A class whose magic has its own sound (SchoolOf: nature, blood) lands with it, whatever it hurts with.
        string own = SchoolOf(skill) is string mine && mine is "nature" or "blood" ? mine : null;
        string school = own ?? CombatSounds.SchoolOf(happened.DamageType);
        bool magic = school is not null && (skill is null || !skill.Weapon);
        bool spell = magic && !auto;
        Element element = own is not null ? CombatEffects.ElementOf(own) : school is not null ? CombatEffects.ElementOf(happened.DamageType) : Element.Physical;

        on.TakeHit(by, happened.Amount, Math.Max(1, target.MaxHp), critical, kill, heavy, spell ? CombatEffects.ColourOf(element) : null);

        Vector2 toward = by is null ? new Vector2(target.Facing < 0 ? 1f : -1f, 0f) : new Vector2(MathF.Sign(on.Position.X - by.Position.X), 0f);
        if (toward.X == 0f) toward = Vector2.UnitX;
        if (magic && skill?.Projectile is not null && !AreaBolt(skill))
        {
            // Its bolt bursts where it lands (CueProjectile); an area's bursts in the middle, and on each it catches here.
        }
        else if (spell)
        {
            _effects.Burst(ChestOf(on), element, heavy ? 1.4f : 1f, -toward);
        }
        else
        {
            Vector2 chest = on.BonePosition("torso");
            Vector2 tip = by?.WeaponTip ?? chest;
            float near = on.Position.X - toward.X * on.Size.X * 0.22f;
            var contact = new Vector2(near, MathHelper.Clamp(tip.Y, on.Position.Y - on.Size.Y * 0.85f, on.Position.Y - on.Size.Y * 0.25f));
            Vector2 swing = by?.SwingDirection ?? Vector2.Zero;
            if (swing.LengthSquared() < 1e-3f) swing = toward;
            _effects.Impact(by is null ? chest : contact, swing, element, critical, heavy, BlowOf(actor, happened.DamageType));
        }

        Play(magic ? CombatSounds.Impact(school) : CombatSounds.Hit(actor is null ? "bow" : FamilyOf(actor)), on);
        if (critical) Play(SoundKeys.Crit, on);

        _popups.Damage(target.Id, numbers, happened.Amount, critical, taken);

        if (!kill && on.Move is not { IsAction: true } && !target.IsDown && !Prone(target))
            on.PlayMove(critical || heavy ? "hurt-big" : "hurt");

        if (Shake(happened, actor, target, auto, heavy, spell) is ShakeCause cause)
            _effects.Shaker.Kick(cause, critical, _camera.ToScreen(ChestOf(on)), heavy || critical || kill ? toward : Vector2.Zero);
    }

    /// <summary>What shakes the screen for a blow, or null: never an auto attack; a kill, a boss's heavy blow, a big blow on you, a heavy skill, an area spell, a spell, or a critical on its own.</summary>
    private ShakeCause? Shake(BattleEvent happened, Fighter actor, Fighter target, bool auto, bool heavy, bool spell)
    {
        if (auto) return null;
        HitFlags flags = happened.Flags;
        if (flags.HasFlag(HitFlags.Kill)) return ShakeCause.Kill;
        if (actor is not null && _battle.IsBoss(actor) && flags.HasFlag(HitFlags.Big)) return ShakeCause.BossSlam;
        if (target == _battle.Leader && happened.Amount >= target.MaxHp * BigHitShare) return ShakeCause.PlayerBigHit;
        if (flags.HasFlag(HitFlags.Area) && spell) return ShakeCause.AreaSpell;
        if (heavy) return spell ? ShakeCause.Spell : ShakeCause.Heavy;
        if (flags.HasFlag(HitFlags.Crit)) return ShakeCause.Light;
        return null;
    }

    /// <summary>A blow that didn't land: the dodge or the block on the one it was aimed at (unless they're mid-move), a sidestep or a clash, the word, and its sound.</summary>
    private void CueAvoided(BattleEvent happened, WalkerSprite by, WalkerSprite on)
    {
        if (on is null) return;
        Vector2 numbers = NumbersAt(on);
        bool free = on.Move is not { IsAction: true };
        switch (happened.Kind)
        {
            case BattleEventKind.Dodged:
                if (free) on.PlayMove("dodge");
                Vector2 away = by is null ? Vector2.UnitX : on.Position - by.Position;
                if (away.LengthSquared() > 0f) away = Vector2.Normalize(away);
                on.Nudge(new Vector2(away.X, 0.4f) * DodgeDistance, DodgeSeconds);
                _popups.Avoided(happened.TargetId, numbers, "DODGE");
                Play(SoundKeys.Dodge, on);
                break;

            case BattleEventKind.Blocked:
                if (free) on.PlayMove("block");
                on.Freeze(BlockFrames, HitStopRole.Victim, 1f);
                by?.Freeze(BlockFrames, HitStopRole.Attacker);
                _effects.Impact(on.BonePosition("hand_far"), by is null ? Vector2.UnitX : new Vector2(MathF.Sign(on.Position.X - by.Position.X), 0f), Element.Physical, blow: "blunt");
                _popups.Avoided(happened.TargetId, numbers, "BLOCK");
                Play(SoundKeys.Block, on);
                break;

            default:
                bool saved = happened.Flags.HasFlag(HitFlags.Save);
                _popups.Avoided(happened.TargetId, numbers, saved ? "RESIST" : "MISS");
                if (!saved) Play(SoundKeys.Miss, on);
                break;
        }
    }

    /// <summary>Healing landing: the green number (a tick's smaller), the glow, a green wash, and the sound.</summary>
    private void CueHealed(BattleEvent happened, Fighter target, WalkerSprite on)
    {
        if (on is null || target is null || happened.Amount <= 0) return;
        Vector2 numbers = NumbersAt(on);
        if (happened.Flags.HasFlag(HitFlags.Tick))
        {
            _popups.Tick(target.Id, numbers, happened.Amount, DamagePopups.HealColour, heal: true);
            return;
        }
        _popups.Heal(target.Id, numbers, happened.Amount);
        _effects.HealGlow(ChestOf(on));
        on.FlashTint(DamagePopups.HealColour, 10);
        Play(SoundKeys.Heal, on);
    }

    /// <summary>An aura going on: the HUD shows it; on the figure only a small glow, gold for a blessing and the school's for a curse, and a sound when it's yours or on you.</summary>
    private void CueAura(BattleEvent happened, Fighter actor, Fighter target, WalkerSprite on)
    {
        if (on is null || target is null) return;
        Aura aura = null;
        foreach (Aura each in target.Auras)
            if (each.Id == happened.AuraId) aura = each;
        bool good = aura?.Beneficial ?? target.Side == actor?.Side;
        _effects.Motes(ChestOf(on), new Point(12, 18), good ? CombatStyle.Blessing : CombatStyle.Threat, false);
        if (actor == _battle.Leader || target == _battle.Leader) Play(good ? SoundKeys.Buff : SoundKeys.Debuff, on, QuietVolume);
    }

    // Life and death.

    /// <summary>One of the party going down: they fall and stay on the floor rolling their saves, with the word, the sound, and a shake if it's you.</summary>
    private void CueDowned(Fighter target, WalkerSprite on)
    {
        if (on is null || target is null) return;
        Clip collapse = on.PlayMove("collapse", hold: true);
        Play(SoundKeys.Downed, on);
        FallsAt(on, collapse);
        Callout(on, _encounter.Nonlethal ? "KNOCKED OUT" : "DOWN!", Palette.Ember);
        if (target == _battle.Leader) _effects.Shaker.Kick(ShakeCause.Downed, false, _camera.ToScreen(on.Position));
        // The lesson's about helping a friend up, so it pulses on them; not while it's you on the floor.
        if (target.Side == Side.Party && target != _battle.Leader && !_encounter.Nonlethal) Lesson(TutorialCatalog.CombatDowned, target.Id);
    }

    /// <summary>The thud of a body meeting the floor, on the clip's land, if it has one.</summary>
    private void FallsAt(WalkerSprite on, Clip fall)
    {
        if (fall?.FirstEvent("land") is not float land) return;
        WalkerSprite body = on;
        AtShown(_shownMs + land * 1000f, () => Play(SoundKeys.BodyFall, body));
    }

    /// <summary>Someone back on their feet: the get-up, the glow and the word, and a note to say so after a fight that's over.</summary>
    private void CueRevived(BattleEvent happened, Fighter target, WalkerSprite on)
    {
        if (on is null || target is null) return;
        on.PlayMove("revive");
        on.Opacity = 1f;
        _effects.HealGlow(ChestOf(on), 1.3f);
        Play(SoundKeys.Revive, on);
        Callout(on, "BACK UP", DamagePopups.HealColour);
    }

    /// <summary>Someone dying: a foe's death clip, a held beat on the floor, then a fade in motes; one of the party who's fallen for the rest of the fight stays on the floor, faint.</summary>
    private void CueDied(Fighter target, WalkerSprite on)
    {
        if (on is null || target is null) return;
        Play(SoundKeys.Death, on);
        if (target.Side == Side.Party)
        {
            if (on.Move is null || on.Move.Name != "collapse") on.PlayMove("collapse", hold: true);
            FadeTo(on, FallenOpacity, FadeOutSeconds);
            Callout(on, "FALLEN", Palette.Ember);
            return;
        }

        Clip die = on.PlayMove("die", hold: true);
        FallsAt(on, die);
        float lies = (die?.Length ?? 0.8f) + LieSeconds;
        WalkerSprite gone = on;
        AtShown(_shownMs + lies * 1000f, () =>
        {
            _effects.DeathMotes(gone.Position - new Vector2(0f, gone.Size.Y * 0.2f), gone.Size * new Vector2(1f, 0.5f), Fading);
            FadeTo(gone, 0f, FadeOutSeconds);
        });
    }

    /// <summary>A guest beaten down, or a foe who yields, stepping out of the fight: the word over them and a fade, unhurt for the story.</summary>
    private void CueFellBack(Fighter actor, WalkerSprite by)
    {
        if (by is null || actor is null) return;
        Callout(by, actor.Side == Side.Foes ? "YIELDS" : "FALLS BACK", Palette.Bone);
        if (by.Move is not { IsAction: true }) by.PlayMove("hurt-big");
        FadeTo(by, actor.Side == Side.Foes ? 0.85f : 0f, FadeOutSeconds);
    }

    /// <summary>Someone joining partway through (a wave, a summon, a guest, a foe out of the smoke): their figure, faded in or walking in from where the engine has them, motes, and the word.</summary>
    private void CueJoined(BattleEvent happened, Fighter actor)
    {
        if (actor is null) return;
        WalkerSprite walker = FigureForJoiner(actor);
        walker.Position = ToScreen(actor.Position);
        _before[actor.Id] = actor.Position;
        if (walker.Opacity < 1f) ArriveIn(walker);
        if (actor.Side == Side.Foes && !actor.Vanished) Callout(walker, "JOINS THE FIGHT", CombatStyle.FoeLine);
        else if (actor.Side == Side.Party) Callout(walker, "JOINS THE FIGHT", Palette.Gold);
    }

    /// <summary>A boss changing phase: its name across the screen, a white flash and a squash, the boss's own sound and a slam of a shake.</summary>
    private void CuePhase(BattleEvent happened, Fighter actor, WalkerSprite by)
    {
        if (actor is null) return;
        string phase = happened.Text?.ToUpperInvariant() ?? $"PHASE {happened.Amount}";
        _banner.Show(actor.Name.ToUpperInvariant(), phase, Palette.Ember);
        Play(SoundKeys.BossPhase, by);
        if (by is null) return;
        by.FlashWhite(6, 10);
        by.Squash(-0.12f, 14);
        _effects.Burst(ChestOf(by), Element.Necrotic, 1.6f);
        _effects.Shaker.Kick(ShakeCause.BossSlam, false, _camera.ToScreen(ChestOf(by)));
    }

    /// <summary>A line shouted mid-fight: a line in the barks' lane under the frames at the top (PlaceBarks), and the log; one said by nobody (a wave's) goes under the banner. A line waits while a banner's up (a boss's phase and its roar come together), so the two don't come at once.</summary>
    private void CueBark(BattleEvent happened, Fighter actor, WalkerSprite by)
    {
        if (happened.Text is null) return;
        string line = TextTokens.Fill(happened.Text, _game);
        if (by is null || actor is null)
        {
            _banner.Show(string.Empty, line.ToUpperInvariant(), Palette.Torch);
            return;
        }
        _barks.RemoveAll(bark => bark.Walker == by);
        _barks.Add((by, line, -_banner.Left, BarkSeconds + line.Length * BarkSecondsPerLetter));
    }

    /// <summary>The Soul-Link charging: gold round the two of you, a soft chime, the word, and the tip about it the first time.</summary>
    private void CueCharge(BattleEvent happened, WalkerSprite by)
    {
        foreach (string duo in new[] { Origins.You, Origins.SoulBound })
            if (FighterWalker(duo) is WalkerSprite walker) _effects.Motes(ChestOf(walker), new Point(16, 22), Palette.Gold, false);
        bool full = happened.Amount >= Battle.SoulLinkCharges;
        Play(full ? SoundKeys.CooldownReady : SoundKeys.Buff, by, QuietVolume);
        if (by is not null) Callout(by, full ? "SOUL-LINK READY" : "SOUL-LINK", Palette.Gold);
        if (full) Lesson(TutorialCatalog.CombatSoulLink);
    }

    /// <summary>The fight's won: everyone of the party still standing strikes their victory pose, and the last blow's accent.</summary>
    private void CueVictory()
    {
        _wonCued = true;
        Play(SoundKeys.Victory, null);
        foreach (Fighter fighter in _battle.Fighters)
        {
            if (fighter.Side != Side.Party || !fighter.Present || fighter.HasLeft || !_fighters.TryGetValue(fighter.Id, out WalkerSprite walker)) continue;
            if (walker.Move is { Name: "collapse" }) walker.PlayMove("revive");
            else walker.PlayMove("victory-" + WeaponKinds.Clip(fighter.Form?.Weapon ?? fighter.WeaponKind), hold: true);
        }
    }

    // Helpers.

    /// <summary>Whether a kind of event writes its line in the log: not the walking, the threat or the Focus, which would drown everything else.</summary>
    private static bool Logged(BattleEventKind kind) => kind is not (BattleEventKind.Moved or BattleEventKind.ThreatTargetChanged or BattleEventKind.FocusChanged
        or BattleEventKind.AuraTicked or BattleEventKind.AuraRefreshed or BattleEventKind.Denied or BattleEventKind.FightLost);

    /// <summary>A skill by its id, or null for none or an id the catalog hasn't got (a help, a support).</summary>
    private static SkillDefinition SkillOf(string id) => id is not null && SkillCatalog.Exists(id) ? SkillCatalog.Get(id) : null;

    /// <summary>The spell school a skill's cast and impact sound like (CombatSounds.Schools), or null for a weapon's blow, a heal or a buff.</summary>
    /// <remarks>The damage type picks it, but a few classes' magic has its own sound whatever it hurts with: a Wildcaller's thorns and a Hivekeeper's bees are nature, a Hemomancer's spells blood.</remarks>
    private static string SchoolOf(SkillDefinition skill)
    {
        if (skill is null || skill.Weapon) return null;
        string found = null;
        if (skill.Projectile is DamageType flies) found = CombatSounds.SchoolOf(flies);
        foreach (Effect effect in skill.Effects)
        {
            if (found is not null) break;
            if (effect.Kind == EffectType.Damage && !effect.Weapon && effect.Type is DamageType type) found = CombatSounds.SchoolOf(type);
        }
        bool hurts = skill.Effects.Any(effect => effect.Kind == EffectType.Damage && !effect.Weapon);
        return skill.Class switch
        {
            ClassId.Druid or ClassId.Hivekeeper when hurts && found is null => "nature",
            ClassId.Hemomancer when hurts && found is null or "necrotic" => "blood",
            _ => found,
        };
    }

    /// <summary>Whether a move that isn't a weapon's strikes someone (a Shield Bash, a kick, a thrown thing), so swings; a heal, a buff or the potion doesn't.</summary>
    private static bool Strikes(SkillDefinition skill) => skill is not null && skill.Effects.Any(effect => effect.Kind is EffectType.Damage or EffectType.Throw);

    /// <summary>The weapon family someone's blows sound like: a beast's claws or teeth, a party member's weapon in hand, or their weapon kind's.</summary>
    private static string FamilyOf(Fighter fighter)
    {
        if (fighter.Form is not null) return "claw";
        if (fighter.Enemy is EnemyDefinition enemy && enemy.Beast)
            return enemy.Attacks.Count > 0 && enemy.Attacks[0].DamageType == DamageType.Piercing ? "bite" : "claw";
        if (fighter.Member?.WornIn(EquipmentSlot.MainHand) is ItemDefinition weapon) return CombatSounds.FamilyOfWeapon(weapon.Base ?? weapon.Id);
        return CombatSounds.FamilyOfKind(WeaponKinds.Clip(fighter.WeaponKind));
    }

    /// <summary>How a weapon's blow lands, for its sparks: a cut, a thrust or a blow.</summary>
    private static string BlowOf(Fighter actor, DamageType type) => type switch
    {
        DamageType.Piercing => "thrust",
        DamageType.Bludgeoning => "blunt",
        _ => actor?.WeaponKind is WeaponKind.Polearm or WeaponKind.Dagger ? "thrust" : "slash",
    };

    /// <summary>Whether someone's on the ground (knocked down), so a flinch would stand them up.</summary>
    private bool Prone(Fighter fighter) => _battle.Has(fighter, ConditionId.Prone);

    /// <summary>Plays a combat sound, panned by where it happens on the screen.</summary>
    private void Play(string key, WalkerSprite where, float volume = 1f)
    {
        float pan = where is null ? 0f : MathHelper.Clamp((_camera.ToScreen(where.Position).X / LegendQuestGame.ScreenWidth - 0.5f) * 1.2f, -0.8f, 0.8f);
        _audio.PlayCombat(key, pan, volume);
    }

    /// <summary>Where someone's numbers and words start: a little over their head.</summary>
    private static Vector2 NumbersAt(WalkerSprite walker) => HeadOf(walker) - new Vector2(0f, NumbersRise);

    /// <summary>Pops up a callout over someone: over their nameplate, kept off everyone else's, and waiting while the banner's up (DamagePopups.Word).</summary>
    private void Callout(WalkerSprite walker, string text, Color colour)
    {
        string id = null;
        foreach ((string each, WalkerSprite figure) in _fighters)
            if (figure == walker) id = each;
        _popups.Word(NumbersAt(walker), text, colour, id);
    }

    /// <summary>Where someone's chest is: where blows land and motes rise from.</summary>
    private static Vector2 ChestOf(WalkerSprite walker) => walker.Position - new Vector2(0f, walker.Size.Y * ChestShare);

    /// <summary>Writes a line in the log, in the colour of whoever did it: bone for yours, soft red for a foe's, gold for the fight's own.</summary>
    private void Say(string line, Fighter actor) => _log.Add(line, LineColour(actor));

    /// <summary>The colour a line's written in, by whose it is.</summary>
    private static Color LineColour(Fighter actor) => actor is null ? Palette.Gold : actor.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Bone;

    /// <summary>The combat sounds the cues play by name (CombatSounds has the families and the schools).</summary>
    private static class SoundKeys
    {
        public const string Crit = "crit", Block = "block", Dodge = "dodge", Miss = "miss", Heal = "heal", Buff = "buff", Debuff = "debuff";
        public const string AuraTick = "aura-tick", Telegraph = "telegraph", BossPhase = "boss-phase", Knockdown = "knockdown", Death = "death";
        public const string Downed = "downed", Revive = "revive", Potion = "potion", SoulLink = "soul-link", Press = "ui-press", Denied = "ui-denied";
        public const string CooldownReady = "cooldown-ready", Victory = "victory", Defeat = "defeat", LevelUp = "level-up", BodyFall = "body-fall";
    }
}
