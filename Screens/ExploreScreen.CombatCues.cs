using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Rules;
using LegendQuest.Tutorial;

namespace LegendQuest.Screens;

/// <summary>
/// What each thing that happens in a fight looks and sounds like as it's shown: the motion, the
/// poses, what flies, the sparks and the motes, the numbers, the shake, the sound and the log line.
/// </summary>
/// <remarks>
/// The Black Box's TableCues and TableScreen.Effects for a grid: each CombatEvent is a beat with a
/// length, and everything it sets off happens as it's shown. An attack winds up, then lunges or
/// lets fly, and its log line goes in as the blow lands, with the hit's sparks, its sound and the
/// shake right after; the number pops as the bar drains. A step is a slide from tile to tile. Every
/// length is a named constant: quick enough that a turn doesn't drag, slow enough to follow.
/// </remarks>
public partial class ExploreScreen
{
    // How long each kind of beat is shown for, in seconds.

    /// <summary>One initiative roll: quick, so the turn order fills in at a run.</summary>
    private const float InitiativeSeconds = 0.16f;

    /// <summary>A turn starting, so its banner's read before anything happens.</summary>
    private const float TurnSeconds = 0.6f;

    /// <summary>One step from a tile to the next.</summary>
    private const float StepSeconds = 0.17f;

    /// <summary>A foe lashing out at someone pulling away.</summary>
    private const float OpportunitySeconds = 0.4f;

    /// <summary>An ability used: the cast, or the drink, or the dash.</summary>
    private const float AbilitySeconds = 0.5f;

    /// <summary>An attack's wind-up before the blow, and the blow itself from close up.</summary>
    private const float WindUpSeconds = 0.22f;
    private const float StrikeSeconds = 0.14f;

    /// <summary>A hit landing, and a critical, which is let to land harder.</summary>
    private const float HitSeconds = 0.2f;
    private const float CriticalSeconds = 0.36f;

    /// <summary>A miss, a number popping up, healing, a save, a condition going on or coming off.</summary>
    private const float MissSeconds = 0.4f;
    private const float DamageSeconds = 0.32f;
    private const float HealSeconds = 0.4f;
    private const float SaveSeconds = 0.55f;
    private const float ConditionSeconds = 0.32f;
    private const float ConditionGoneSeconds = 0.15f;

    /// <summary>Going down, a death save, stopping dying, getting up, dying: each given time to land.</summary>
    private const float DownedSeconds = 0.85f;
    private const float DeathSaveSeconds = 0.85f;
    private const float StabiliseSeconds = 0.5f;
    private const float ReviveSeconds = 0.6f;
    private const float DieSeconds = 0.9f;

    /// <summary>The Soul-Link charging, someone joining, a line said, and a line in the log with nothing else to it.</summary>
    private const float ChargeSeconds = 0.75f;
    private const float JoinSeconds = 0.7f;
    private const float BarkBeatSeconds = 0.35f;
    private const float LogSeconds = 0.3f;

    // The motion.

    /// <summary>How long a pose is held: the strike, the cast, being hurt.</summary>
    private const float StrikePoseSeconds = 0.3f;
    private const float CastPoseSeconds = 0.45f;
    private const float HurtPoseSeconds = 0.35f;

    /// <summary>How far a lunge goes, a blow knocks someone back, and a miss is sidestepped, in world pixels, and how long each takes there and back.</summary>
    private const float LungeDistance = 6f;
    private const float LungeSeconds = 0.22f;
    private const float RecoilDistance = 3f;
    private const float RecoilSeconds = 0.2f;
    private const float DodgeDistance = 4f;
    private const float DodgeSeconds = 0.3f;

    /// <summary>How long a hit's red flash takes to die away.</summary>
    private const float FlashSeconds = 0.3f;

    /// <summary>How fast things fly, in world pixels a second, and the least and most time any flight takes.</summary>
    private const float ProjectileSpeed = 260f;
    private const float FlightLeast = 0.12f;
    private const float FlightMost = 0.6f;

    /// <summary>How far up their figure from their feet someone's chest is, as a share of how tall they stand: where blows land and motes rise from.</summary>
    private const float ChestShare = 0.55f;

    /// <summary>How long a fallen foe takes to fade off the map, and how faint one of the party who's fallen is drawn.</summary>
    private const float FadeOutSeconds = 0.8f;
    private const float FallenOpacity = 0.6f;

    // What's thrown about.

    /// <summary>How hard a hit, a critical, someone going down and a blast shake the screen, from 0 to 1.</summary>
    private const float HitShake = 0.22f;
    private const float CriticalShake = 0.55f;
    private const float DownedShake = 0.35f;
    private const float BlastShake = 0.3f;

    /// <summary>Straight up the screen, and how wide sparks fan off a blow and off a critical, in radians.</summary>
    private const float Upward = -MathHelper.PiOver2;
    private const float SparkSpread = 1.1f;
    private const float CriticalSpread = 1.6f;

    /// <summary>The areas motes start in, in world pixels: round someone casting, round someone healed or dying, and a column over someone light's falling on.</summary>
    private static readonly Point CastCloud = new(16, 22);
    private static readonly Point HealCloud = new(18, 26);
    private static readonly Point FallCloud = new(12, 10);

    /// <summary>How far over someone's head radiant light starts falling, in world pixels.</summary>
    private const float FallFrom = 30f;

    /// <summary>The colour a foe fades away in.</summary>
    private static readonly Color Fading = new(170, 160, 200);

    /// <summary>The colour of damage numbers over a foe, and over one of yours.</summary>
    private static readonly Color DamageOnFoe = new(255, 240, 222);
    private static readonly Color DamageOnParty = new(255, 108, 92);

    // Sound.

    /// <summary>How loud a foe's turn starting is, against yours, and something being thrown.</summary>
    private const float QuietTurn = 0.45f;
    private const float LaunchVolume = 0.7f;

    // Words.

    /// <summary>How the engine's save line says it was made: "... = 17 - SAVES.".</summary>
    private const string SavedWord = " - SAVES";

    /// <summary>What a death save has to reach to count as made: 5e's 10.</summary>
    private const int DeathSaveTarget = 10;

    /// <summary>How long a line said mid-fight stays over the speaker's head, plus a bit for every letter.</summary>
    private const float BarkSeconds = 2.4f;
    private const float BarkSecondsPerLetter = 0.05f;

    /// <summary>Whoever the last critical shown landed on, until its number's popped, so the number's the big one.</summary>
    private string _criticalOn;

    /// <summary>Starts showing an event: the screen's picture of the fight catches up with it, and everything it sets off starts.</summary>
    /// <param name="happened">The event.</param>
    private void StartBeat(CombatEvent happened)
    {
        _beat = happened;
        _beatClock = 0f;
        _beatLength = 0f;
        _beatStrike = null;
        _beatLine = null;
        _shown.Show(happened);

        Combatant actor = Fighter(happened.Actor), target = Fighter(happened.Target);
        WalkerSprite by = FighterWalker(happened.Actor), on = FighterWalker(happened.Target);
        AbilityDefinition ability = happened.Ability is AbilityId id ? AbilityCatalog.Get(id) : null;

        switch (happened.Kind)
        {
            case CombatEventKind.InitiativeRolled:
                if (by is not null) _popups.Word(HeadOf(by), happened.Amount.ToString(CultureInfo.InvariantCulture), Palette.Gold);
                if (_shown.Order.Count == 1) _audio.Play(Sfx.DiceRoll);
                _log.Add(happened.Text, Palette.Note);
                _beatLength = InitiativeSeconds;
                break;

            case CombatEventKind.RoundStarted:
                _log.Add($"ROUND {happened.Amount}", Palette.Gold);
                break;

            case CombatEventKind.TurnStarted:
                CueTurn(actor, by);
                break;

            case CombatEventKind.Moved:
                if (by is null) break;
                Glide(by, FeetOn(happened.To), StepSeconds);
                _focusWalker = by;
                _beatLength = StepSeconds;
                break;

            case CombatEventKind.OpportunityAttack:
                if (by is not null && on is not null)
                {
                    by.Face(on.Position);
                    _popups.Word(HeadOf(by), "OPPORTUNITY ATTACK", Palette.Ember);
                }
                Say(happened.Text, actor);
                _beatLength = OpportunitySeconds;
                break;

            case CombatEventKind.AbilityUsed:
                CueAbility(happened, actor, by, on, ability);
                break;

            case CombatEventKind.AttackRolled:
                CueAttack(happened, actor, by, on, ability);
                break;

            case CombatEventKind.Hit:
            case CombatEventKind.Critical:
                CueImpact(happened, actor, by, on, ability);
                break;

            case CombatEventKind.Missed:
                if (on is not null)
                {
                    _popups.Word(HeadOf(on), "MISS", Palette.Note);
                    if (by is not null)
                    {
                        Vector2 toward = Toward(by, on);
                        on.Nudge(new Vector2(-toward.Y, toward.X) * DodgeDistance, DodgeSeconds);
                    }
                }
                _audio.Play(Sfx.Miss);
                _beatLength = MissSeconds;
                break;

            case CombatEventKind.Damaged:
                CueDamage(happened, actor, target, on);
                break;

            case CombatEventKind.Healed:
                if (on is null || happened.Amount <= 0) break;
                _popups.Number(HeadOf(on), happened.Amount, true, false, CombatStyle.Healing);
                _effects.Motes(ChestOf(on), HealCloud, CombatStyle.Healing, false);
                _beatLength = HealSeconds;
                break;

            case CombatEventKind.SaveRolled:
                CueSave(happened, actor, on, ability);
                break;

            case CombatEventKind.ConditionAdded:
                if (happened.Text is null || on is null) break;
                var condition = (ConditionId)happened.Amount;
                _popups.Word(HeadOf(on), ConditionInfo.Name(condition).ToUpperInvariant(), CombatStyle.Helps(condition) ? Palette.Gold : Palette.Ember);
                Say(happened.Text, actor);
                _beatLength = ConditionSeconds;
                break;

            case CombatEventKind.ConditionRemoved:
                if (happened.Text is null) break;
                Say(happened.Text, actor);
                _beatLength = ConditionGoneSeconds;
                break;

            case CombatEventKind.Downed:
            case CombatEventKind.DeathSave:
            case CombatEventKind.Stabilised:
            case CombatEventKind.Revived:
            case CombatEventKind.Died:
                CueLife(happened, target, on);
                break;

            case CombatEventKind.SoulLinkCharged:
                CueCharge(happened, by);
                break;

            case CombatEventKind.SoulLinkStrike:
                CueTogether(happened, actor, by, on);
                break;

            case CombatEventKind.Victory:
                _victoryXp = happened.Amount;
                _log.Add(happened.Text, Palette.Gold);
                break;

            case CombatEventKind.Defeat:
                _log.Add(happened.Text, Palette.Ember);
                break;

            case CombatEventKind.Joined:
                CueJoined(happened, actor);
                break;

            case CombatEventKind.Bark:
                if (by is null || actor is null || happened.Text is null) break;
                string line = TextTokens.Fill(happened.Text, _game);
                _barks.Add((by, line, 0f, BarkSeconds + line.Length * BarkSecondsPerLetter));
                _log.Add($"{actor.Name.ToUpperInvariant()}: \"{line}\"", Palette.Torch);
                _beatLength = BarkBeatSeconds;
                break;

            default:
                if (happened.Text is null) break;
                Say(happened.Text, actor);
                if (happened.Ability == AbilityId.Protection && by is not null)
                {
                    _audio.Play(Sfx.Shield);
                    _popups.Word(HeadOf(by), "SHIELD UP", Palette.Gold);
                }
                _beatLength = LogSeconds;
                break;
        }
    }

    /// <summary>A turn starting: its banner, its sound, the camera to them, and for one of yours, the hotbar becoming theirs.</summary>
    private void CueTurn(Combatant actor, WalkerSprite by)
    {
        if (actor is null) return;

        _aiWait = 0f;
        _focusWalker = by;
        Cancel();

        string name = actor.Name.ToUpperInvariant();
        bool yours = actor.Side == Side.Party && actor.Member is not null && !actor.Guest;
        if (yours && _game.Party.IndexOf(actor.Member) is int index and >= 0) _game.Leader = index;

        if (yours && !_proofAutoplay)
        {
            _banner.Show("YOUR TURN", $"{name}  ·  {WhatTheyAre(actor)}", Palette.Gold);
            _audio.Play(Sfx.TurnStart);
        }
        else
        {
            Color colour = actor.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Bone;
            _banner.Show($"{name}'S TURN", $"ROUND {_shown.Round}", colour);
            _audio.Play(Sfx.TurnStart, QuietTurn);
        }

        _log.Add($"{name}'S TURN", LineColour(actor));
        _beatLength = TurnSeconds;
        LessonTurnStarted(actor);
    }

    /// <summary>An ability used: the cast or the strike, the light round the user (and the one it's for), its sound, and for a charge thrown into a crowd, the charge flying to where it lands.</summary>
    private void CueAbility(CombatEvent happened, Combatant actor, WalkerSprite by, WalkerSprite on, AbilityDefinition ability)
    {
        Say(happened.Text, actor);
        _beatLength = AbilitySeconds;
        if (by is null || ability is null) return;

        _focusWalker = by;
        if (on is not null && on != by) by.Face(on.Position);
        by.Play(ability.Spell ? WalkerPose.Cast : WalkerPose.Strike, CastPoseSeconds);

        Color light = CombatStyle.LightOf(ability);
        bool glows = ability.Spell || ability.Kind is AbilityKind.Heal or AbilityKind.Potion or AbilityKind.Condition;
        if (glows) _effects.Motes(ChestOf(by), CastCloud, light, false);
        if (on is not null && on != by && ability.Kind is AbilityKind.Heal or AbilityKind.Condition or AbilityKind.Help)
            _effects.Motes(ChestOf(on), HealCloud, light, false);

        Sfx? sound = ability.Kind switch
        {
            AbilityKind.Heal => Sfx.Heal,
            AbilityKind.Potion => Sfx.Potion,
            _ when ability.Spell => Sfx.Cast,
            _ when ability.Conditions.Count > 0 && ability.Target == TargetKind.Self => Sfx.Shield,
            _ => null,
        };
        if (sound is Sfx play) _audio.Play(play);
        if (ability.Kind is AbilityKind.Dash or AbilityKind.Help) _popups.Word(HeadOf(by), ability.Name.ToUpperInvariant(), Palette.Gold);

        string flying = CombatStyle.Projectile(ability.Id);
        if (flying is null || ability.Kind != AbilityKind.Save) return;

        Vector2 from = ChestOf(by), to = LandingOf(ability.Id) ?? on?.Position ?? by.Position;
        float flight = FlightSeconds(from, to);
        _beatStrikeAt = WindUpSeconds;
        _beatStrike = () =>
        {
            _effects.Launch(flying, from, to, flight, light);
            _audio.Play(Sfx.Miss, LaunchVolume);
        };
        _beatLength = WindUpSeconds + flight;
    }

    /// <summary>An attack: the wind-up, then the lunge or what flies, its log line going in as it lands.</summary>
    private void CueAttack(CombatEvent happened, Combatant actor, WalkerSprite by, WalkerSprite on, AbilityDefinition ability)
    {
        _criticalOn = null;
        _beatLine = happened.Text;
        _beatLineColour = LineColour(actor);
        _beatLength = WindUpSeconds;
        if (by is null || on is null) return;

        by.Face(on.Position);
        _focusWalker = null;
        _focus = (by.Position + on.Position) / 2f;

        bool spell = ability?.Spell == true;
        Color light = ability is null ? Palette.Gold : CombatStyle.LightOf(ability);
        by.Play(spell ? WalkerPose.Cast : WalkerPose.WindUp, WindUpSeconds);
        if (spell) _effects.Motes(ChestOf(by), CastCloud, light, false);

        string flying = Flying(happened, actor, ability);
        _beatStrikeAt = WindUpSeconds;
        if (flying is null)
        {
            _beatStrike = () =>
            {
                by.Play(WalkerPose.Strike, StrikePoseSeconds);
                by.Nudge(Toward(by, on) * LungeDistance, LungeSeconds);
            };
            _beatLength = WindUpSeconds + StrikeSeconds;
            return;
        }

        Vector2 from = ChestOf(by), to = ChestOf(on);
        float flight = FlightSeconds(from, to);
        bool arrow = flying == CombatStyle.Arrow;
        _beatStrike = () =>
        {
            by.Play(spell ? WalkerPose.Cast : WalkerPose.Strike, StrikePoseSeconds);
            _effects.Launch(flying, from, to, flight, arrow ? null : light);
            _audio.Play(arrow ? Sfx.Bow : Sfx.Cast, LaunchVolume);
        };
        _beatLength = WindUpSeconds + flight;
    }

    /// <summary>A hit or a critical landing (a dart flying in first, for Magic Missile): sparks the damage's colour, its sound, the target hurt and knocked back, and the shake.</summary>
    private void CueImpact(CombatEvent happened, Combatant actor, WalkerSprite by, WalkerSprite on, AbilityDefinition ability)
    {
        bool critical = happened.Kind == CombatEventKind.Critical;
        if (critical) _criticalOn = happened.Target;
        if (happened.Text is not null) _beatLine = happened.Text;
        _beatLineColour = LineColour(actor);
        _beatLength = critical ? CriticalSeconds : HitSeconds;
        if (on is null) return;

        DamageType kind = happened.DamageType ?? DamageType.Bludgeoning;
        string flying = ability is { Kind: AbilityKind.AutoHit } && by is not null ? CombatStyle.Projectile(ability.Id) : null;
        if (flying is null)
        {
            Land(by, on, kind, critical);
            return;
        }

        Vector2 from = ChestOf(by), to = ChestOf(on);
        float flight = FlightSeconds(from, to);
        _effects.Launch(flying, from, to, flight, CombatStyle.DamageColour(kind));
        _audio.Play(Sfx.Cast, LaunchVolume);
        _focus = (by.Position + on.Position) / 2f;
        _focusWalker = null;
        _beatStrikeAt = flight;
        _beatStrike = () => Land(by, on, kind, critical);
        _beatLength = flight + HitSeconds;
    }

    /// <summary>A blow landing on someone.</summary>
    /// <param name="by">Who struck it, or null.</param>
    /// <param name="on">Who it landed on.</param>
    /// <param name="kind">The kind of damage.</param>
    /// <param name="critical">Whether it was a critical.</param>
    private void Land(WalkerSprite by, WalkerSprite on, DamageType kind, bool critical)
    {
        Vector2 chest = ChestOf(on);
        Vector2 away = by is null ? -Vector2.UnitY : Toward(by, on);
        float facing = by is null ? Upward : MathF.Atan2(away.Y, away.X);

        _effects.Sparks(chest, CombatStyle.DamageColour(kind), facing, SparkSpread);
        _audio.Play(CombatStyle.DamageSound(kind));
        if (critical)
        {
            _effects.Sparks(chest, Palette.Gold, Upward, CriticalSpread);
            _audio.Play(Sfx.Crit);
            _popups.Word(HeadOf(on), "CRITICAL!", Palette.Gold);
        }

        if (on.Pose != WalkerPose.Downed) on.Play(WalkerPose.Hurt, HurtPoseSeconds);
        on.Flash(FlashSeconds);
        if (by is not null) on.Nudge(away * RecoilDistance, RecoilSeconds);
        _effects.Kick(critical ? CriticalShake : HitShake, _camera.ToScreen(chest));
    }

    /// <summary>Damage shown: the number over them as their bar drains, or for someone already down, the death save it costs them.</summary>
    private void CueDamage(CombatEvent happened, Combatant actor, Combatant target, WalkerSprite on)
    {
        _beatLength = DamageSeconds;
        if (on is null || target is null) return;

        if (happened.Text is not null)
        {
            _popups.Word(HeadOf(on), "DEATH SAVE FAILED", Palette.Ember);
            on.Flash(FlashSeconds);
            Say(happened.Text, actor);
            return;
        }

        bool critical = _criticalOn == happened.Target;
        _criticalOn = null;
        _popups.Number(HeadOf(on), happened.Amount, false, critical, target.Side == Side.Party ? DamageOnParty : DamageOnFoe);
    }

    /// <summary>A saving throw shown: the spell's light on whoever made it or didn't (radiant light falling, a burst of fire, the vines), and SAVED or FAILED over them.</summary>
    private void CueSave(CombatEvent happened, Combatant actor, WalkerSprite on, AbilityDefinition ability)
    {
        Say(happened.Text, actor);
        _beatLength = SaveSeconds;
        if (on is null) return;

        bool saved = happened.Text?.Contains(SavedWord, StringComparison.Ordinal) == true;
        Color light = ability is null ? Palette.Gold : CombatStyle.LightOf(ability);
        Vector2 chest = ChestOf(on);
        if (ability?.Id == AbilityId.SacredFlame)
        {
            _effects.Motes(chest - new Vector2(0f, FallFrom), FallCloud, light, true);
        }
        else if (ability is { Kind: AbilityKind.Save } && !ability.Damage.IsNone)
        {
            _effects.Sparks(chest, light, Upward, CriticalSpread);
            _effects.Kick(BlastShake, _camera.ToScreen(chest));
        }
        else if (ability is not null)
        {
            _effects.Motes(chest, CastCloud, light, false);
        }

        _popups.Word(HeadOf(on), saved ? "SAVED" : "FAILED", saved ? Palette.Bone : Palette.Ember);
        _focusWalker = on;
    }

    /// <summary>Going down, a death save, stopping dying, getting up, and dying, each with its pose, its sound and its word.</summary>
    private void CueLife(CombatEvent happened, Combatant target, WalkerSprite on)
    {
        Say(happened.Text, target);
        if (on is null || target is null) return;
        _focusWalker = on;

        switch (happened.Kind)
        {
            case CombatEventKind.Downed:
                on.Play(WalkerPose.Downed);
                _audio.Play(Sfx.Downed);
                _popups.Word(HeadOf(on), _encounter.Nonlethal ? "KNOCKED OUT" : "DOWN!", Palette.Ember);
                _effects.Kick(DownedShake, _camera.ToScreen(on.Position));
                if (target.Side == Side.Party && !_encounter.Nonlethal) Lesson(TutorialCatalog.CombatDowned);
                _beatLength = DownedSeconds;
                break;

            case CombatEventKind.DeathSave:
                bool made = happened.Amount >= DeathSaveTarget;
                _popups.Word(HeadOf(on), $"DEATH SAVE  {happened.Amount}", made ? Palette.Gold : Palette.Ember);
                _audio.Play(Sfx.DiceRoll);
                _beatLength = DeathSaveSeconds;
                break;

            case CombatEventKind.Stabilised:
                _popups.Word(HeadOf(on), "STABLE", Palette.Gold);
                _beatLength = StabiliseSeconds;
                break;

            case CombatEventKind.Revived:
                on.Release();
                _effects.Motes(ChestOf(on), HealCloud, CombatStyle.Healing, false);
                _audio.Play(Sfx.Heal);
                _popups.Word(HeadOf(on), "BACK UP", CombatStyle.Healing);
                if (target.Member is not null && !target.Guest && happened.Actor == happened.Target && VictoryAhead() && !_backUp.Contains(target.Name))
                    _backUp.Add(target.Name);
                _beatLength = ReviveSeconds;
                break;

            case CombatEventKind.Died:
                _audio.Play(Sfx.Death);
                on.Play(WalkerPose.Downed);
                if (target.Side == Side.Foes)
                {
                    FadeTo(on, 0f, FadeOutSeconds);
                    _effects.Motes(ChestOf(on), HealCloud, Fading, false);
                }
                else
                {
                    FadeTo(on, FallenOpacity, FadeOutSeconds);
                    _popups.Word(HeadOf(on), "FALLEN", Palette.Ember);
                }
                _beatLength = DieSeconds;
                break;
        }
    }

    /// <summary>The Soul-Link charging: the sigil's flicker, its chime, gold round the two of you, and the tip about it the first time.</summary>
    private void CueCharge(CombatEvent happened, WalkerSprite by)
    {
        _sigil.Flicker();
        _audio.Play(Sfx.SoulLink);
        _log.Add(happened.Text, Palette.Gold);
        foreach (string duo in new[] { Origins.You, Origins.SoulBound })
            if (FighterWalker(duo) is WalkerSprite walker) _effects.Motes(ChestOf(walker), HealCloud, Palette.Gold, false);

        if (by is not null) _popups.Word(HeadOf(by), happened.Amount >= SoulLink.Full ? "SOUL-LINK READY" : "SOUL-LINK", Palette.Gold);
        Lesson(TutorialCatalog.CombatSoulLink);
        _beatLength = ChargeSeconds;
    }

    /// <summary>The Soul-Link Strike: the two of you winding up together, then lunging as one in a fall of gold. The hit that follows lands it.</summary>
    private void CueTogether(CombatEvent happened, Combatant actor, WalkerSprite by, WalkerSprite on)
    {
        _beatLine = happened.Text;
        _beatLineColour = Palette.Gold;
        _beatLength = WindUpSeconds + StrikeSeconds;
        if (by is null || on is null) return;

        WalkerSprite partner = FighterWalker(_combat.Partner(actor)?.Id);
        WalkerSprite[] strikers = partner is null ? new[] { by } : new[] { by, partner };
        foreach (WalkerSprite striker in strikers)
        {
            striker.Face(on.Position);
            striker.Play(WalkerPose.WindUp, WindUpSeconds);
        }

        _effects.Motes(ChestOf(on) - new Vector2(0f, FallFrom), FallCloud, Palette.Gold, true);
        _audio.Play(Sfx.SoulLink);
        _focusWalker = null;
        _focus = (by.Position + on.Position) / 2f;
        _beatStrikeAt = WindUpSeconds;
        _beatStrike = () =>
        {
            foreach (WalkerSprite striker in strikers)
            {
                striker.Play(WalkerPose.Strike, StrikePoseSeconds);
                striker.Nudge(Toward(striker, on) * LungeDistance, LungeSeconds);
            }
        };
    }

    /// <summary>Someone joining partway through: their figure fading in on their tile in a cloud of motes.</summary>
    private void CueJoined(CombatEvent happened, Combatant actor)
    {
        Say(happened.Text, actor);
        if (actor?.Enemy is null) return;

        WalkerSprite walker = Spawn(EnemyWalker(actor.Enemy));
        walker.Position = FeetOn(happened.From);
        walker.Facing = actor.Facing;
        _fighters[actor.Id] = walker;
        FadeTo(walker, 1f, ArriveSeconds);
        _effects.Motes(ChestOf(walker), HealCloud, Fading, false);
        _popups.Word(HeadOf(walker), "JOINS THE FIGHT", CombatStyle.FoeLine);
        _focusWalker = walker;
        _beatLength = JoinSeconds;
    }

    /// <summary>Someone in the fight by their id, or null.</summary>
    private Combatant Fighter(string id) => id is null ? null : _combat.Find(id);

    /// <summary>Writes a line in the log, in the colour of whoever did it: bone for yours, soft red for a foe's, gold for the fight's own.</summary>
    private void Say(string line, Combatant actor) => _log.Add(line, LineColour(actor));

    /// <summary>The colour a line's written in, by whose it is.</summary>
    private static Color LineColour(Combatant actor) => actor is null ? Palette.Gold : actor.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Bone;

    /// <summary>What someone is, for their turn's banner and their frame: "LEVEL 1 BLADEMASTER", or a guest's or a foe's kind.</summary>
    private static string WhatTheyAre(Combatant who) => who.Member is not null
        ? $"LEVEL {who.Member.Level} {ClassCatalog.Get(who.Member.Class).Name.ToUpperInvariant()}"
        : who.Side == Side.Foes ? "FOE" : "ALLY";

    /// <summary>Where someone's chest is: where blows land and motes rise from.</summary>
    private static Vector2 ChestOf(WalkerSprite walker) => walker.Position - new Vector2(0f, walker.Shape.Figure.Y * ChestShare);

    /// <summary>The way from one figure to another, as a unit, or nothing if they're on the same spot.</summary>
    private static Vector2 Toward(WalkerSprite from, WalkerSprite to)
    {
        Vector2 way = to.Position - from.Position;
        return way.LengthSquared() > 0f ? Vector2.Normalize(way) : Vector2.Zero;
    }

    /// <summary>How long something takes to fly between two points.</summary>
    private static float FlightSeconds(Vector2 from, Vector2 to) => MathHelper.Clamp(Vector2.Distance(from, to) / ProjectileSpeed, FlightLeast, FlightMost);

    /// <summary>What flies for an attack: the ability's own (a fire bolt, a flask), or an arrow for a weapon attack from further off than the tiles beside the attacker with a ranged weapon; null for a blow from close up.</summary>
    private static string Flying(CombatEvent happened, Combatant actor, AbilityDefinition ability)
    {
        if (ability is not null && CombatStyle.Projectile(ability.Id) is string piece) return piece;
        if (actor is null || Measure.Steps(happened.From, happened.To) <= 1) return null;
        foreach (AttackProfile attack in actor.Attacks)
            if (attack.Ranged) return CombatStyle.Arrow;
        return null;
    }

    /// <summary>Where a charge thrown into a crowd lands: on the first one it catches, from the save that follows it.</summary>
    private Vector2? LandingOf(AbilityId ability)
    {
        foreach (CombatEvent waiting in _beats)
            if (waiting.Kind == CombatEventKind.SaveRolled && waiting.Ability == ability && FighterWalker(waiting.Target) is WalkerSprite caught) return caught.Position;
        return null;
    }

    /// <summary>Whether the fight's won further down the events still to show, so whoever gets up now got up because it's over.</summary>
    private bool VictoryAhead()
    {
        foreach (CombatEvent waiting in _beats)
            if (waiting.Kind == CombatEventKind.Victory) return true;
        return false;
    }
}
