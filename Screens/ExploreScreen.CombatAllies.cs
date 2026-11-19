using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components;
using LegendQuest.Story;
using LegendQuest.Tutorial;
using LegendQuest.World;
using SupportView = LegendQuest.Combat.Realtime.SupportView;

namespace LegendQuest.Screens;

/// <summary>
/// A fight's supports on the screen: their figures behind the party with a banner over each and its
/// ring filling to the next time it acts, and what their shots and nets look like.
/// </summary>
/// <remarks>
/// Story/11-combat.md §5.4. A support is drawn as whoever the encounter says (EncounterSupport.Figure):
/// one of the party sitting the fight out, someone standing in the room, or a figure made for it (an
/// origin, the cast, an enemy, a creature, a townsfolk sheet, or someone a zone puts down elsewhere).
/// Each stands on the band's back edge behind the party (BattleArena.SupportSpot, spread along the
/// edge), those in the room walking there and the rest fading in. Its banner says what it does, lit
/// for a moment when it acts and dim once a script's stood it down; its ring fills to its next turn,
/// or for one holding out for something (Sera at the crystal) to the end of the hold. When it acts,
/// its figure swings or shoots, an arrow flies if it shoots, and its name pops over it; the blow
/// lands with the arrow. Nobody can hit a support, so nothing is drawn under one.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far over a support's head its banner sits, in screen pixels.</summary>
    private const int SupportBannerRise = 14;

    /// <summary>How far apart along the back edge two supports stand, in world pixels.</summary>
    private const float SupportSpacing = 22f;

    /// <summary>How long a support's banner stays lit after it acts.</summary>
    private const int SupportLitMs = 700;

    /// <summary>Each support's figure, by the support's id.</summary>
    private readonly Dictionary<string, WalkerSprite> _supportFigures = new();

    /// <summary>Where each figure that was standing in the room before it was a support stood, to go back to.</summary>
    private readonly Dictionary<WalkerSprite, Vector2> _supportHomes = new();

    /// <summary>When a support's shot lands on each foe it's flying at, by the foe's id, for its blow to land with it.</summary>
    private readonly Dictionary<string, float> _supportShot = new();

    /// <summary>A support's banner as it's drawn: who, what it says, its icon, whether it's lit, and its ring.</summary>
    private sealed record SupportBannerView(string Id, string Line, Rectangle? Icon, bool Lit, float? Progress);

    /// <summary>Puts each support's figure at its place behind the party: one standing in the room walks there, any other fades in on it.</summary>
    private void PlaceSupports()
    {
        _supportFigures.Clear();
        _supportHomes.Clear();
        _supportShot.Clear();

        IReadOnlyList<SupportView> supports = _battle.Supports;
        Vector2 spot = ToScreen(_battle.Arena.SupportSpot);
        float ahead = Leader.Position.X >= spot.X ? -1f : 1f;
        for (int i = 0; i < supports.Count; i++)
        {
            if (SupportFigure(supports[i].Figure) is not WalkerSprite walker) continue;

            float x = Math.Clamp(spot.X + ahead * i * SupportSpacing, _battle.Arena.Left, _battle.Arena.Right);
            var stand = new Vector2(x, _room.BackEdgeAt(x) + BattleFloor.Inset);
            Direction facing = Leader.Position.X >= x ? Direction.Right : Direction.Left;
            _supportFigures[supports[i].Id] = walker;
            if (walker.Opacity <= 0f)
            {
                walker.Position = stand;
                walker.Facing = facing;
                ArriveIn(walker);
                continue;
            }

            if (!_benched.ContainsKey(walker)) _supportHomes[walker] = walker.Position;
            _benched.Remove(walker);
            WalkTo(walker, stand, facing, StandBackLeast, StandBackMost);
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
        foreach (WalkerSprite walker in _supportFigures.Values) walker.InFight = false;
        _supportHomes.Clear();
        _supportFigures.Clear();
        _supportShot.Clear();
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

    /// <summary>A figure made for someone who isn't anyone in the room: an origin as they'd be dressed, one of the cast, an enemy, a creature, a townsfolk sheet, or someone a zone puts down elsewhere, each with their imported picture if they've one.</summary>
    private WalkerSprite MadeFigure(string figure)
    {
        if (Origins.Exists(figure)) return GuestWalker(figure);
        if (Cast.Get(figure) is CastMember cast && (cast.Look is not null || cast.Sheet is not null))
            return new WalkerSprite { Sheet = cast.Sheet is not null ? _content.Load<Texture2D>(Townsfolk.Texture(cast.Sheet)) : SheetFor(cast.Look), Art = ImportedArt.Get(figure) };
        if (EnemyCatalog.Exists(figure)) return EnemyWalker(EnemyCatalog.Get(figure));
        if (Creatures.Get(figure) is Creature creature) return new WalkerSprite { Shape = creature.Shape, Sheet = CreatureSheet(creature), Art = ImportedArt.Get(figure) };
        if (Townsfolk.Contains(figure)) return new WalkerSprite { Sheet = _content.Load<Texture2D>(Townsfolk.Texture(figure)), Art = ImportedArt.Get(figure) };

        foreach (ZoneDefinition zone in ZoneCatalog.All)
            foreach (RoomDefinition room in zone.Rooms)
                foreach (MapNpc npc in room.Npcs)
                    if (npc.Id == figure || npc.Cast == figure)
                        return new WalkerSprite { Shape = npc.Creature is null ? WalkerShape.Person : Creatures.Get(npc.Creature).Shape, Sheet = NpcSheet(npc), Art = ArtFor(npc) };
        return null;
    }

    /// <summary>Every support's banner as it's drawn: lit for a moment as it acts, its ring filling to its next turn (or to the end of the hold, for one holding out).</summary>
    /// <remarks>Its line is who they are, and only while it's lit what they did: a long line up the whole fight sat across the middle of it.</remarks>
    private List<SupportBannerView> SupportBanners()
    {
        var views = new List<SupportBannerView>();
        foreach (SupportView support in _battle.Supports)
        {
            EncounterSupport definition = SupportOf(support.Id);
            bool lit = !support.Off && support.LastMs >= 0 && _battle.NowMs - support.LastMs < SupportLitMs;
            string line = (lit ? definition?.Ability?.Text ?? support.Name : support.Name).ToUpperInvariant();
            float? progress = null;
            if (support.Progress) progress = (float)_battle.SurviveShare;
            else if (!support.Off && definition?.Ability is not null && support.EveryMs > 0)
                progress = MathHelper.Clamp(1f - (support.NextMs - _battle.NowMs) / (float)support.EveryMs, 0f, 1f);
            views.Add(new SupportBannerView(support.Id, line, SupportIcon(definition?.Ability), lit, progress));
        }
        return views;
    }

    /// <summary>A support of the encounter by its id, or null.</summary>
    private EncounterSupport SupportOf(string id)
    {
        foreach (EncounterSupport support in _encounter.Supports)
            if (support.Id == id) return support;
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

    /// <summary>Where a support's banner goes over its figure, on screen, kept inside the screen's edges.</summary>
    private Rectangle SupportBannerOver(WalkerSprite walker, SupportBannerView view)
    {
        Vector2 head = HeadOnScreen(walker);
        int width = _supportBanner.Width(view.Line);
        int extra = view.Progress is null ? 0 : SupportBanner.RingSize;
        int x = Math.Clamp((int)(head.X - (width + extra) / 2f), (int)Margin, Math.Max((int)Margin, LegendQuestGame.ScreenWidth - (int)Margin - width - extra));
        return new Rectangle(x, (int)(head.Y - SupportBannerRise - SupportBanner.Height), width, SupportBanner.Height);
    }

    /// <summary>Draws the banners over the supports' figures. Goes in the point sampled screen batch.</summary>
    private void DrawSupportBannersArt(SpriteBatch spriteBatch, IReadOnlyList<SupportBannerView> views, float alpha)
    {
        foreach (SupportBannerView view in views)
        {
            if (!_supportFigures.TryGetValue(view.Id, out WalkerSprite walker) || walker.Opacity <= 0f) continue;
            _supportBanner.DrawArt(spriteBatch, SupportBannerOver(walker, view), view.Icon, view.Lit, view.Progress, walker.Opacity * alpha);
        }
    }

    /// <summary>Writes the lines on the banners over the supports' figures. Goes in the linear sampled screen batch.</summary>
    private void DrawSupportBannersWords(SpriteBatch spriteBatch, IReadOnlyList<SupportBannerView> views, float alpha)
    {
        foreach (SupportBannerView view in views)
        {
            if (!_supportFigures.TryGetValue(view.Id, out WalkerSprite walker) || walker.Opacity <= 0f) continue;
            _supportBanner.DrawWords(spriteBatch, SupportBannerOver(walker, view), view.Line, view.Lit, walker.Opacity * alpha);
        }
    }

    /// <summary>A support acting: its figure swings or shoots at the one it's aimed at, an arrow flies if it shoots (the blow lands with it), a net or a blessing falls if that's what it does, and its name pops over it.</summary>
    private void CueSupport(BattleEvent happened)
    {
        EncounterSupport support = SupportOf(happened.ActorId);
        WalkerSprite on = FighterWalker(happened.TargetId);
        Lesson(TutorialCatalog.CombatAllies);
        if (support is null || !_supportFigures.TryGetValue(support.Id, out WalkerSprite by)) return;

        if (on is not null) by.Face(on.Position);
        _popups.Word(NumbersAt(by), support.Name.ToUpperInvariant(), Palette.Gold);
        SupportAbility ability = support.Ability;
        bool shoots = ability is not null && !ability.Damage.IsNone && Shoots(ability.Text);
        if (!shoots || on is null)
        {
            by.PlayMove(on is null ? "cast" : "auto-one-hand-1");
            if (on is not null && ability?.Condition is not null) _effects.Motes(ChestOf(on) - new Vector2(0f, 18f), new Point(18, 10), Palette.Bone, true);
            return;
        }

        by.PlayMove("auto-bow-1");
        Vector2 from = by.BonePosition("hand_near"), to = ChestOf(on);
        _effects.Launch(CombatStyle.Arrow, from, to, SupportFlightSeconds, null);
        Play("swing-bow", by, QuietVolume);
        if (happened.TargetId is string foe) _supportShot[foe] = _shownMs + SupportFlightSeconds * 1000f;
    }

    /// <summary>Whether a support's ability is something shot from where it stands, so an arrow flies: its line names an arrow, a bow, a bolt or a crossbow.</summary>
    private static bool Shoots(string text)
    {
        if (text is null) return false;
        foreach (string word in ShotWords)
            if (text.Contains(word, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>The words in a support's line that mean it shoots.</summary>
    private static readonly string[] ShotWords = { "arrow", "bow", "bolt", "crossbow" };
}
