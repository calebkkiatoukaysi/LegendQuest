using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Items;
using LegendQuest.Story;
using LegendQuest.Tutorial;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// A fight on the map you're on: starting one, everyone taking their places, and how it ends, with
/// the loot, the experience and the conversation after, or the conversation for a fight you were
/// allowed to lose, or game over.
/// </summary>
/// <remarks>
/// <para>
/// There's no arena: the fight is on this map, and its foes are the map's own people where they're
/// standing, or turn up on their tiles. A conversation ends with DialogueEffect.Fight and the fight
/// starts when it's over (OnDialogue notes it), the way a shop or a journey waits. The game's
/// autosaved first, so a lost fight can be fought again. Everyone steps onto the middle of their
/// tile, the nearest free one (the engine picks it), the battle's song comes in, and the dice for
/// initiative fill the turn order in across the top.
/// </para>
/// <para>
/// The fight itself is in partials of its own: ExploreScreen.CombatPlayback runs it and plays each
/// thing that happens in order, ExploreScreen.CombatCues is what each one looks and sounds like,
/// ExploreScreen.CombatInput is your turn, ExploreScreen.CombatDraw draws it,
/// ExploreScreen.CombatTutorial teaches it, and ExploreScreen.CombatProof drives it for the proof run.
/// Nobody in the party dies for good: someone whose death saves run out has fallen for the rest of
/// the fight, and everyone's back up on 1 HP when it's won, or when a fight that's allowed to be lost
/// is. It's only lost when every one of the party is down or fallen at once. A foe who dies leaves
/// the map for the rest of the visit; its presence flag keeps it gone after that.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far below the middle of their tile someone's feet go, in world pixels, so they stand in it rather than on its top edge.</summary>
    private static readonly Vector2 StandOffset = new(0f, 4f);

    /// <summary>How long everyone takes to step onto their tile when the fight starts, and how long the fight's name is up before the dice roll.</summary>
    private const float TakePlacesSeconds = 0.35f;
    private const float IntroSeconds = 1.1f;

    /// <summary>How long someone who turns up takes to fade in.</summary>
    private const float ArriveSeconds = 0.5f;

    /// <summary>What the pause menu says in a fight, instead of its usual line.</summary>
    private const string NoSaving = "NO SAVING MID-FIGHT: THE AUTOSAVE IS FROM JUST BEFORE IT.";

    /// <summary>What goes under the fight's name as it starts.</summary>
    private const string RollLine = "ROLL FOR INITIATIVE";

    /// <summary>The panel's headings at the end.</summary>
    private const string VictoryHeading = "VICTORY";
    private const string DefeatHeading = "DEFEAT";

    /// <summary>The fight's sheets, and everything on the screen that's only there in a fight.</summary>
    private readonly CombatArt _combatArt = new();
    private readonly CombatEffects _effects = new();
    private readonly DamagePopups _popups = new();
    private readonly TurnBanner _banner = new();
    private readonly CombatLog _log = new();
    private readonly CombatCard _card = new();
    private readonly InitiativeBar _initiative = new();
    private readonly CombatHotbar _hotbar = new();
    private readonly BudgetBar _budgetBar = new();
    private readonly SoulLinkSigil _sigil = new();
    private readonly UnitFrame _unitFrame = new();
    private readonly CombatResult _result = new();

    /// <summary>Everyone fighting's figure, by their id in the fight: the party's own walkers, the map's people who turned, and anyone who turned up.</summary>
    private readonly Dictionary<string, WalkerSprite> _fighters = new();

    /// <summary>The figures of those who turned up for the fight (a foe on a tile, a guest from nowhere), which aren't anyone on the map, so the fight draws and runs them itself.</summary>
    private readonly List<WalkerSprite> _spawned = new();

    /// <summary>The map's people who died in a fight, kept off the map for the rest of this visit (RefreshPresence leaves them out).</summary>
    private readonly HashSet<Npc> _fallen = new();

    /// <summary>The names of those of the party who got up when it was over, for a line each once the panel's gone.</summary>
    private readonly List<string> _backUp = new();

    /// <summary>The fight, as the engine has it, and as the screen has shown it so far; null when there's no fight.</summary>
    private CombatEngine _combat;
    private ShownFight _shown;
    private EncounterDefinition _encounter;

    /// <summary>The encounter a conversation that's up has asked to start when it's over (OnDialogue notes it), or null.</summary>
    private string _fightAfterTalk;

    /// <summary>Who you were controlling before the fight, to go back to after it.</summary>
    private int _leaderBeforeFight;

    /// <summary>Whether the fight's sheets and components have been loaded on this screen.</summary>
    private bool _combatLoaded;

    /// <summary>Whether the end of the fight has been dealt with: the panel's up, or game over.</summary>
    private bool _fightEnded;

    /// <summary>Whether a fight's on.</summary>
    private bool InCombat => _combat is not null;

    /// <summary>Starts the fight the save was made in, if it was made as one started: loading it is the fight again.</summary>
    /// <returns>True if one started.</returns>
    private bool ResumeSavedFight()
    {
        if (InCombat || _game.Fight is not string id) return false;
        StartFight(EncounterCatalog.Get(id));
        return true;
    }

    /// <summary>Starts the fight the conversation that's just ended asked for, if it asked for one.</summary>
    private void StartAwaitedFight()
    {
        if (_fightAfterTalk is not string id) return;
        _fightAfterTalk = null;
        StartFight(EncounterCatalog.Get(id));
    }

    /// <summary>
    /// Starts a fight on this map: the autosave, everyone into the fight and onto their tiles, the song,
    /// the fight's name across the screen, and the initiative dice after it.
    /// </summary>
    /// <param name="encounter">The fight.</param>
    private void StartFight(EncounterDefinition encounter)
    {
        if (InCombat) return;

        LoadCombat();
        Halt();
        _banter = null;
        _banterNext = null;
        foreach (Npc npc in _npcs) npc.Arrive();
        foreach (WalkerSprite walker in _walkers) walker.IsWalking = false;
        _toast.Clear();
        _game.Fight = encounter.Id;
        if (!Autosave()) _toast.Show(AutosaveFailedLine);

        _encounter = encounter;
        _leaderBeforeFight = _game.Leader;
        _fighters.Clear();
        _spawned.Clear();
        _backUp.Clear();
        _fightEnded = false;
        _lastOfYours = null;
        _reachFor = null;
        Cancel();

        var setups = new List<CombatantSetup>();
        Point gather = Tilemap.CellOf(Leader.Position);

        for (int i = 0; i < _game.Party.Count; i++)
        {
            PartyMember member = _game.Party[i];
            if (encounter.SitsOut.Contains(member.Id)) continue;
            setups.Add(CombatantSetup.ForMember(member, Tilemap.CellOf(_walkers[i].Position), _walkers[i].Facing));
            _fighters[member.Id] = _walkers[i];
        }

        for (int i = 0; i < encounter.Foes.Count; i++)
        {
            EncounterFoe foe = encounter.Foes[i];
            if (foe.Npc is string npcId)
            {
                Npc npc = Turning(npcId);
                setups.Add(CombatantSetup.ForFoe(foe, npcId, Tilemap.CellOf(npc.Walker.Position), npc.Walker.Facing));
                _fighters[npcId] = npc.Walker;
                continue;
            }

            string id = $"{foe.Enemy}-{i + 1}";
            setups.Add(CombatantSetup.ForFoe(foe, id, foe.Cell ?? gather, Direction.Down));
            _fighters[id] = Spawn(EnemyWalker(EnemyCatalog.Get(foe.Enemy)));
        }

        // Guests start beside the party, wherever their figure was standing.
        foreach (string guest in encounter.Guests)
        {
            setups.Add(CombatantSetup.ForGuest(guest, gather, Leader.Facing));
            Npc standing = _npcs.Find(npc => npc.Definition.Id == guest || npc.Definition.Cast == guest);
            WalkerSprite walker = standing?.Walker ?? Spawn(GuestWalker(guest));
            walker.Opacity = 0f;
            _fighters[guest] = walker;
        }

        var fighting = new HashSet<WalkerSprite>(_fighters.Values);
        _combat = new CombatEngine(encounter, setups, new MapCombatGrid(_tilemap, FightFootprints(fighting)), _random) { Bag = _game.Bag };
        _shown = new ShownFight(_combat);
        TakePlaces();

        _audio.PlaySong(encounter.Music);
        _log.Clear();
        _popups.Clear();
        _effects.Clear();
        _initiative.Clear();
        _result.Hide();
        _log.Add($"{encounter.Name.ToUpperInvariant()}: the fight begins.", Palette.Gold);
        _banner.Show(encounter.Name.ToUpperInvariant(), RollLine, encounter.Boss ? Palette.Ember : Palette.Gold);
        _focus = Centre(_fighters.Values);

        BeginPlayback(IntroSeconds);
        Play(_combat.Begin());
        BeginLessons();
    }

    /// <summary>The map npc a foe line names, there or not: the fight brings them out whatever the flags say, since the encounter needs them.</summary>
    /// <remarks>Throws if the map has nobody of that id at all, which is a mistake in EncounterCatalog or the map file.</remarks>
    private Npc Turning(string npcId)
    {
        Npc npc = _everyNpc.Find(each => each.Definition.Id == npcId)
            ?? throw new InvalidOperationException($"Encounter {_encounter.Id} has npc {npcId}, but {_map.Id}.map has nobody called that.");
        if (!_npcs.Contains(npc)) _npcs.Add(npc);
        return npc;
    }

    /// <summary>Gets a figure that isn't anyone on the map ready to fight: its shadow loaded, faded out until it arrives, and drawn by the fight.</summary>
    private WalkerSprite Spawn(WalkerSprite walker)
    {
        walker.LoadContent(_content);
        walker.Opacity = 0f;
        _spawned.Add(walker);
        return walker;
    }

    /// <summary>A figure for a foe from its stat block: its creature's sheet, or its cast member's.</summary>
    private WalkerSprite EnemyWalker(EnemyDefinition enemy)
    {
        if (enemy.Creature is not null)
        {
            Creature creature = Creatures.Get(enemy.Creature);
            return new WalkerSprite { Shape = creature.Shape, Sheet = CreatureSheet(creature) };
        }

        CastMember cast = Cast.Get(enemy.Cast);
        Texture2D sheet = cast.Sheet is not null ? _content.Load<Texture2D>(Townsfolk.Texture(cast.Sheet)) : SheetFor(cast.Look);
        return new WalkerSprite { Sheet = sheet };
    }

    /// <summary>A figure for a guest who isn't standing on the map: a companion as their origin dresses them, or a stat block's.</summary>
    private WalkerSprite GuestWalker(string guest)
    {
        if (!Origins.Exists(guest)) return EnemyWalker(EnemyCatalog.Get(guest));

        PartyMember member = Origins.Create(guest);
        return new WalkerSprite { Sheet = SheetFor(member.Look, Paperdoll.GearOf(member)) };
    }

    /// <summary>What the fight's ground is built from: the props, the shut doors, and everyone standing about who isn't fighting (the party's sat-out members among them).</summary>
    /// <param name="fighting">Everyone fighting's figure, left out, since the engine says where they stand.</param>
    private IEnumerable<BoundingRectangle> FightFootprints(HashSet<WalkerSprite> fighting)
    {
        foreach (PropSprite prop in _props)
            foreach (BoundingRectangle footprint in prop.Footprints) yield return footprint;
        foreach ((MapInteractable thing, PropSprite sprite) in _things)
        {
            if (thing.Kind == InteractableKind.Door && _world.IsOpen(thing)) continue;
            foreach (BoundingRectangle footprint in sprite.Footprints) yield return footprint;
        }
        foreach (Npc npc in _npcs)
            if (!fighting.Contains(npc.Walker)) yield return npc.Footprint;
        foreach (WalkerSprite walker in _walkers)
            if (!fighting.Contains(walker)) yield return FeetAt(walker.Position);
    }

    /// <summary>Everyone onto the tile the engine put them on: those already here step onto it, those turning up fade in on it.</summary>
    private void TakePlaces()
    {
        foreach (Combatant each in _combat.Combatants)
        {
            if (!_fighters.TryGetValue(each.Id, out WalkerSprite walker)) continue;

            walker.Facing = each.Facing;
            if (walker.Opacity <= 0f)
            {
                walker.Position = FeetOn(each.Cell);
                FadeTo(walker, 1f, ArriveSeconds);
                _effects.Motes(walker.Position - new Vector2(0f, walker.Shape.Figure.Y / 2f), new Point((int)walker.Shape.Figure.X, (int)walker.Shape.Figure.Y), CombatStyle.Blessing, false);
            }
            else
            {
                Glide(walker, FeetOn(each.Cell), TakePlacesSeconds);
            }
        }
    }

    /// <summary>Where someone's feet go to stand on a tile.</summary>
    /// <param name="cell">The tile.</param>
    private static Vector2 FeetOn(Point cell) => NavGrid.Middle(cell) + StandOffset;

    /// <summary>The middle of a crowd of figures, for the camera to start on.</summary>
    private static Vector2 Centre(IEnumerable<WalkerSprite> walkers)
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;
        foreach (WalkerSprite walker in walkers)
        {
            sum += walker.Position;
            count++;
        }
        return count == 0 ? sum : sum / count;
    }

    /// <summary>The figure of someone in the fight, by their id, or null.</summary>
    /// <param name="id">Their id in the fight.</param>
    private WalkerSprite FighterWalker(string id) => id is null ? null : _fighters.GetValueOrDefault(id);

    /// <summary>The fight's over and everything's been shown: on to the panel, game over, or for a fight to lose, its panel too.</summary>
    private void EndOfFight()
    {
        _fightEnded = true;
        Cancel();

        if (_combat.Outcome == CombatOutcome.Victory)
        {
            var lines = new List<(string Text, Color Colour)>();
            int share = _victoryXp;
            if (share > 0) lines.Add(($"+{share} XP EACH", Palette.Gold));

            (int gold, List<string> loot) = RollLoot();
            if (gold > 0) lines.Add(($"+{gold} GOLD", Palette.Gold));
            if (loot.Count > 0) lines.Add(("INTO THE BAG", Palette.Note));

            _audio.Play(Sfx.Victory);
            _result.Show(VictoryHeading, Palette.Gold, lines, loot);
            Lesson(TutorialCatalog.CombatVictory);
            return;
        }

        if (_encounter.Lost is null)
        {
            ScreenManager.AddScreen(new GameOverScreen());
            return;
        }

        _audio.Play(Sfx.GameOver);
        _result.Show(DefeatHeading, Palette.Ember, new[] { ("THE FIGHT IS LOST, BUT NOT THE STORY.", Palette.Bone) }, Array.Empty<string>());
    }

    /// <summary>Rolls what every foe who died was carrying, and the fight's own treasure, and puts it in the gold and the bag.</summary>
    /// <returns>The gold, and every item, one entry each.</returns>
    private (int Gold, List<string> Loot) RollLoot()
    {
        int gold = 0;
        var loot = new List<string>();

        void Take(string table)
        {
            LootResult found = LootTableCatalog.Get(table).Roll(_random);
            gold += found.Gold;
            loot.AddRange(found.Items);
        }

        foreach (Combatant each in _combat.Combatants)
            if (each.Side == Side.Foes && each.IsDead && each.Enemy?.LootTable is string drops) Take(drops);
        if (_encounter.LootTable is string treasure) Take(treasure);

        Inventory bag = _game.Bag;
        foreach (string id in loot) bag.Add(id);
        _game.Gold += gold;
        return (gold, loot);
    }

    /// <summary>The panel's been read: the fight's packed away, and the conversation after it plays (the one for having lost it, if it was lost).</summary>
    private void DismissResult()
    {
        bool won = _combat.Outcome == CombatOutcome.Victory;
        _result.Hide();
        if (won && _encounter.Victory is string flag) _game.Flags.Add(flag);

        // Nobody dies for good: anyone down or fallen gets up on 1 HP.
        foreach (Combatant each in _combat.Combatants)
        {
            if (each.Member is null || each.Guest || each.Member.Hp > 0) continue;
            each.Member.Hp = 1;
            if (!_backUp.Contains(each.Name)) _backUp.Add(each.Name);
        }

        string then = won ? _encounter.Then : _encounter.Lost;
        EndFight();
        foreach (string name in _backUp) _toast.Show($"{name} is back on their feet");
        Converse(DialogueCatalog.Get(then), null);
    }

    /// <summary>Packs the fight away: the dead off the map, everyone standing again where they are, the party following you again, and the map's own song.</summary>
    private void EndFight()
    {
        _game.Fight = null;
        foreach (Combatant each in _combat.Combatants)
        {
            if (each.Side != Side.Foes || !each.IsDead) continue;
            Npc npc = _npcs.Find(standing => standing.Walker == FighterWalker(each.Id));
            if (npc is not null) _fallen.Add(npc);
        }

        foreach (WalkerSprite walker in _fighters.Values)
        {
            walker.Release();
            walker.IsWalking = false;
            if (!_spawned.Contains(walker)) walker.Opacity = 1f;
        }

        _combat = null;
        _shown = null;
        _fighters.Clear();
        _spawned.Clear();
        _glides.Clear();
        _fades.Clear();
        _barks.Clear();
        _effects.Clear();
        _popups.Clear();
        _banner.Clear();
        _game.Leader = Math.Clamp(_leaderBeforeFight, 0, _walkers.Count - 1);

        _audio.PlaySong(_map.Music);
        RefreshPresence();
        BuildNav();
        SeedTrail();
    }

    /// <summary>Opens the pause menu in a fight: no INVENTORY (what's worn can't change mid-fight), and a line that says why there's no saving.</summary>
    private void OpenCombatPause()
    {
        Halt();
        ScreenManager.AddScreen(new PauseMenuScreen(null, NoSaving));
    }

    /// <summary>Loads the fight's sheets and components the first time a fight starts on this screen.</summary>
    private void LoadCombat()
    {
        if (_combatLoaded) return;
        _combatLoaded = true;

        Texture2D pixel = ScreenManager.BlankTexture;
        _combatArt.LoadContent(_content);
        _effects.LoadContent(_content);
        _popups.LoadContent(_content);
        _banner.LoadContent(_content, pixel);
        _log.LoadContent(_content, pixel);
        _card.LoadContent(_content, pixel);
        _initiative.LoadContent(_content, _combatArt, pixel);
        _hotbar.LoadContent(_content, _combatArt, pixel);
        _budgetBar.LoadContent(_content, _combatArt);
        _sigil.LoadContent(_content, _combatArt);
        _unitFrame.LoadContent(_content, _combatArt, pixel);
        _result.LoadContent(_content, pixel);
    }
}
