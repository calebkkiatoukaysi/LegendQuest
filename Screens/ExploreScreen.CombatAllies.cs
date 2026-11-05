using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Story;
using LegendQuest.Tutorial;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// A fight's allies on the screen: the supports' figures behind your side with a banner over each,
/// their entries at the end of the turn order, and what their strikes look like; and the guests in
/// the rows falling back when they're beaten.
/// </summary>
/// <remarks>
/// 11-combat §4. A support is drawn as whoever the encounter says (EncounterSupport.Figure): one of
/// the party sitting the fight out, someone standing in the room, or a figure made for it (an
/// origin, the cast, an enemy, a creature, a townsfolk sheet, or someone a zone puts down elsewhere).
/// Each stands at FightStage.SupportStand on the band's back edge, those in the room walking there
/// and the rest fading in. Its banner says what it does, lit while it acts at the end of this round
/// and dim before its first round, after its last, and once a script's stood it down; one holding
/// out for something (Sera at the crystal) has a ring beside it that fills as the rounds go. When it
/// acts, its figure winds up and strikes, an arrow flies if it shoots, and its name pops over it; the
/// damage or the condition follows as its own events. Nobody can hit a support, so nothing is drawn
/// under one.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far over a support's head its banner sits, in screen pixels.</summary>
    private const int SupportBannerRise = 14;

    /// <summary>Each support's figure, by the support's id.</summary>
    private readonly Dictionary<string, WalkerSprite> _supportFigures = new();

    /// <summary>Where each figure that was standing in the room before it was a support stood, to go back to.</summary>
    private readonly Dictionary<WalkerSprite, Vector2> _supportHomes = new();

    /// <summary>Puts each support's figure at its place behind your side: one standing in the room walks there, any other fades in on it.</summary>
    private void PlaceSupports()
    {
        _supportFigures.Clear();
        _supportHomes.Clear();

        IReadOnlyList<SupportState> supports = _combat.Supports;
        for (int i = 0; i < supports.Count; i++)
        {
            if (SupportFigure(supports[i].Support.Figure) is not WalkerSprite walker) continue;

            Vector2 stand = _stage.SupportStand(i, supports.Count);
            _supportFigures[supports[i].Id] = walker;
            if (walker.Opacity <= 0f)
            {
                walker.Position = stand;
                walker.Facing = Direction.Right;
                FadeTo(walker, 1f, ArriveSeconds);
                continue;
            }

            if (!_benched.ContainsKey(walker)) _supportHomes[walker] = walker.Position;
            WalkTo(walker, stand, Direction.Right, TakePlacesLeast, TakePlacesMost);
        }
    }

    /// <summary>The supports go: those who were standing in the room back where they stood, and the rest with the fight's other figures.</summary>
    private void PackSupportsAway()
    {
        foreach ((WalkerSprite walker, Vector2 home) in _supportHomes)
        {
            walker.Position = home;
            walker.IsWalking = false;
            walker.Release();
        }
        _supportHomes.Clear();
        _supportFigures.Clear();
    }

    /// <summary>The figure a support is drawn as: one of the party sitting it out, someone in the room who isn't fighting, or one made for it; null if there's nobody to draw.</summary>
    /// <param name="figure">EncounterSupport.Figure.</param>
    private WalkerSprite SupportFigure(string figure)
    {
        var fighting = new HashSet<WalkerSprite>(_fighters.Values);
        int member = _game.Party.FindIndex(each => each.Id == figure);
        if (member >= 0 && !fighting.Contains(_walkers[member])) return _walkers[member];

        Npc standing = _npcs.Find(npc => (npc.Definition.Id == figure || npc.Definition.Cast == figure) && !fighting.Contains(npc.Walker));
        if (standing is not null) return standing.Walker;

        return MadeFigure(figure) is WalkerSprite made ? Spawn(made) : null;
    }

    /// <summary>A figure made for a support who isn't anyone in the room: an origin as they'd be dressed, one of the cast, an enemy, a creature, a townsfolk sheet, or someone a zone puts down elsewhere.</summary>
    private WalkerSprite MadeFigure(string figure)
    {
        if (Origins.Exists(figure)) return GuestWalker(figure);
        if (Cast.Get(figure) is CastMember cast && (cast.Look is not null || cast.Sheet is not null))
            return new WalkerSprite { Sheet = cast.Sheet is not null ? _content.Load<Texture2D>(Townsfolk.Texture(cast.Sheet)) : SheetFor(cast.Look) };
        if (EnemyCatalog.Exists(figure)) return EnemyWalker(EnemyCatalog.Get(figure));
        if (Creatures.Get(figure) is Creature creature) return new WalkerSprite { Shape = creature.Shape, Sheet = CreatureSheet(creature) };
        if (Townsfolk.Contains(figure)) return new WalkerSprite { Sheet = _content.Load<Texture2D>(Townsfolk.Texture(figure)) };

        foreach (ZoneDefinition zone in ZoneCatalog.All)
            foreach (RoomDefinition room in zone.Rooms)
                foreach (MapNpc npc in room.Npcs)
                    if (npc.Id == figure || npc.Cast == figure)
                        return new WalkerSprite { Shape = npc.Creature is null ? WalkerShape.Person : Creatures.Get(npc.Creature).Shape, Sheet = NpcSheet(npc) };
        return null;
    }

    /// <summary>Every support as the screen shows it, in the order they act.</summary>
    private List<SupportView> SupportViews()
    {
        var views = new List<SupportView>();
        foreach (SupportState state in _combat.Supports)
        {
            EncounterSupport support = state.Support;
            string name = support.Name.ToUpperInvariant();
            string line = support.Ability?.Text.ToUpperInvariant() ?? name;
            float? progress = support.Progress && _encounter.EndsAfterRounds > 0
                ? MathHelper.Clamp((float)_shown.Round / _encounter.EndsAfterRounds, 0f, 1f)
                : null;
            views.Add(new SupportView(support.Id, name, line, SupportIcon(support.Ability), state.Ready && !state.Off, progress));
        }
        return views;
    }

    /// <summary>The supports that act, for the end of the turn order: the ones with nothing to do (a figure the story needs there) aren't on it.</summary>
    private List<SupportView> ActingSupports()
    {
        List<SupportView> views = SupportViews();
        views.RemoveAll(view => FindSupport(view.Id)?.Support.Ability is null);
        return views;
    }

    /// <summary>A support in the fight by its id, or null.</summary>
    private SupportState FindSupport(string id)
    {
        foreach (SupportState state in _combat.Supports)
            if (state.Id == id) return state;
        return null;
    }

    /// <summary>The icon on a support's banner: a net for one that holds a foe, a bow for one that shoots, the spell for fire or light, a blade for anything else; none for one with nothing to do.</summary>
    private static Rectangle? SupportIcon(SupportAbility ability)
    {
        if (ability is null) return null;
        if (ability.Condition == ConditionId.Restrained) return CombatIcons.Ability(AbilityId.Entangle);
        if (ability.Condition is not null && ability.Damage.IsNone) return CombatIcons.Ability(AbilityId.Help);
        return ability.Type switch
        {
            DamageType.Piercing => CombatIcons.Ability("ranged-attack"),
            DamageType.Fire => CombatIcons.Ability(AbilityId.FireBolt),
            DamageType.Radiant => CombatIcons.Ability(AbilityId.SacredFlame),
            _ => CombatIcons.Ability(AbilityId.Attack),
        };
    }

    /// <summary>Where a support's banner goes over its figure, on screen.</summary>
    private Rectangle SupportBannerOver(WalkerSprite walker, SupportView view)
    {
        Vector2 head = HeadOnScreen(walker);
        int width = _supportBanner.Width(view.Line);
        int extra = view.Progress is null ? 0 : SupportBanner.RingSize;
        return new Rectangle((int)(head.X - (width + extra) / 2f), (int)(head.Y - SupportBannerRise - SupportBanner.Height), width, SupportBanner.Height);
    }

    /// <summary>Draws the banners over the supports' figures. Goes in the point sampled screen batch.</summary>
    private void DrawSupportBannersArt(SpriteBatch spriteBatch, IReadOnlyList<SupportView> views, float alpha)
    {
        foreach (SupportView view in views)
        {
            if (!_supportFigures.TryGetValue(view.Id, out WalkerSprite walker) || walker.Opacity <= 0f) continue;
            _supportBanner.DrawArt(spriteBatch, SupportBannerOver(walker, view), view.Icon, view.Lit, view.Progress, walker.Opacity * alpha);
        }
    }

    /// <summary>Writes the lines on the banners over the supports' figures. Goes in the linear sampled screen batch.</summary>
    private void DrawSupportBannersWords(SpriteBatch spriteBatch, IReadOnlyList<SupportView> views, float alpha)
    {
        foreach (SupportView view in views)
        {
            if (!_supportFigures.TryGetValue(view.Id, out WalkerSprite walker) || walker.Opacity <= 0f) continue;
            _supportBanner.DrawWords(spriteBatch, SupportBannerOver(walker, view), view.Line, view.Lit, walker.Opacity * alpha);
        }
    }

    /// <summary>A support acting at the end of the round: its figure winds up and strikes, an arrow flies to the foe if it shoots, and its name pops over it. What it did follows.</summary>
    private void CueSupport(CombatEvent happened)
    {
        SupportState state = FindSupport(happened.Actor);
        string name = state?.Support.Name.ToUpperInvariant() ?? happened.Actor?.ToUpperInvariant();
        _proofSupportShown = true;
        _log.Add(happened.Text ?? $"{name}: {state?.Support.Ability?.Text}", Palette.Gold);
        _beatLength = WindUpSeconds + StrikeSeconds;
        if (happened.Actor is null || !_supportFigures.TryGetValue(happened.Actor, out WalkerSprite by)) return;

        WalkerSprite on = FighterWalker(happened.Target);
        if (on is not null) by.Face(on.Position);
        by.Play(WalkerPose.WindUp, WindUpSeconds);
        _popups.Word(HeadOf(by), name, Palette.Gold);
        Lesson(TutorialCatalog.CombatAllies);

        bool shoots = state?.Support.Ability is SupportAbility ability && !ability.Damage.IsNone && Shoots(ability.Text);
        DamageType kind = state?.Support.Ability?.Type ?? DamageType.Bludgeoning;
        _beatStrikeAt = WindUpSeconds;
        if (!shoots || on is null)
        {
            _beatStrike = () =>
            {
                by.Play(WalkerPose.Strike, StrikePoseSeconds);
                if (on is not null && happened.Amount > 0) _effects.Sparks(ChestOf(on), CombatStyle.DamageColour(kind), Upward, SparkSpread);
            };
            return;
        }

        Vector2 from = ChestOf(by), to = ChestOf(on);
        float flight = FlightSeconds(from, to);
        _beatStrike = () =>
        {
            by.Play(WalkerPose.Strike, StrikePoseSeconds);
            _effects.Launch(CombatStyle.Arrow, from, to, flight, null);
            _audio.Play(Sfx.Bow, LaunchVolume);
        };
        _beatLength = WindUpSeconds + flight;
    }

    /// <summary>Whether a support's ability is something shot from where it stands, so an arrow flies: its line names an arrow, a bow, a bolt or a crossbow.</summary>
    private static bool Shoots(string text)
    {
        if (text is null) return false;
        foreach (string word in ShotWords)
            if (text.Contains(word, System.StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>The words in a support's line that mean it shoots.</summary>
    private static readonly string[] ShotWords = { "arrow", "bow", "bolt", "crossbow" };

    /// <summary>A guest beaten down to nothing falling back out of the fight: they fade from their slot, unhurt for the story.</summary>
    private void CueFellBack(CombatEvent happened)
    {
        Combatant guest = Fighter(happened.Target ?? happened.Actor);
        WalkerSprite walker = FighterWalker(guest?.Id);
        string name = guest?.Name.ToUpperInvariant() ?? string.Empty;
        _log.Add(happened.Text ?? $"{name} FALLS BACK OUT OF THE FIGHT.", Palette.Bone);
        _beatLength = DieSeconds;
        if (walker is null) return;

        _popups.Word(HeadOf(walker), "FALLS BACK", Palette.Bone);
        walker.Play(WalkerPose.Hurt, HurtPoseSeconds);
        FadeTo(walker, 0f, FadeOutSeconds);
    }
}
