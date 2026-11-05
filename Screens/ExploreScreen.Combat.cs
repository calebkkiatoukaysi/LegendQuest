using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Items;
using LegendQuest.Rules;
using LegendQuest.Story;
using LegendQuest.Tutorial;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// A fight in the room you're in: starting one, everyone walking to their slots in the rows, and how
/// it ends, with the loot, the experience and the levels and the conversation after, or the
/// conversation for a fight you were allowed to lose, or game over with what the difficulty offers.
/// </summary>
/// <remarks>
/// <para>
/// There's no arena: the room is the place you fight, the way an AQW map is (Story/11-combat.md §1).
/// The slots are laid out on its floor by FightStage, your side on the left and the foes on the
/// right, and the camera holds on the middle of them. A conversation ends with DialogueEffect.Fight
/// and the fight starts when it's over (OnDialogue notes it), the way a shop or a journey waits. The
/// game's autosaved first, so a lost fight can be fought again. FightRoster lines the party, the
/// guests and the foes up (Build and Foes); the party walks to its slots and the foes standing in the room to theirs, those who
/// turn up fade in on theirs, the supports go to their places behind your side (ExploreScreen
/// .CombatAllies), the battle's song comes in, and the dice for initiative fill the turn order in. In
/// a fight to yield, anyone standing about who isn't in it steps back to the far edge of the floor to
/// watch; in a fight that can kill they leave by the nearest edge of the room, and both come back
/// after. Whoever of the party sits it out steps back and watches.
/// </para>
/// <para>
/// The fight itself is in partials of its own: ExploreScreen.CombatPlayback runs it and plays each
/// thing that happens in order, ExploreScreen.CombatCues is what each one looks and sounds like,
/// ExploreScreen.CombatInput is your turn, ExploreScreen.CombatDraw draws it, ExploreScreen.CombatCards
/// says what everything is, ExploreScreen.CombatTutorial teaches it, and ExploreScreen.CombatProof
/// drives it for the proof run. Nobody in the party dies for good: someone whose death saves run out
/// has fallen for the rest of the fight, and everyone's back up on 1 HP when it's won, or when a fight
/// that's allowed to be lost is. It's only lost when every one of the party is down or fallen at once.
/// A foe who dies leaves the room for the rest of the visit; its presence flag keeps it gone after.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>The least and most anyone takes to walk to their slot as the fight starts: walking pace in between.</summary>
    private const float TakePlacesLeast = 0.35f;
    private const float TakePlacesMost = 1.6f;

    /// <summary>How long the fight's name is up before the dice roll, at least.</summary>
    private const float IntroSeconds = 1.1f;

    /// <summary>The most anyone standing about takes to step back out of the fight's way or leave the room, and to come back once it's over.</summary>
    private const float StandBackMost = 1.6f;

    /// <summary>How far onto the floor from its back edge they stand to watch, in world pixels.</summary>
    private const float StandBackInset = 1f;

    /// <summary>How far past the room's edge someone leaving a lethal fight walks, in world pixels, so they're off the floor.</summary>
    private const float EdgeOut = 24f;

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
    private readonly SupportBanner _supportBanner = new();
    private readonly CombatResult _result = new();

    /// <summary>Everyone fighting's figure, by their id in the fight: the party's own walkers, the room's people who turned, and anyone who turned up.</summary>
    private readonly Dictionary<string, WalkerSprite> _fighters = new();

    /// <summary>The figures of those who turned up for the fight (a foe in its slot, a guest or a support from nowhere), which aren't anyone in the room, so the fight draws and runs them itself.</summary>
    private readonly List<WalkerSprite> _spawned = new();

    /// <summary>The room's people who died in a fight, as room/id, kept out of their room for the rest of this visit to the zone (RefreshPresence leaves them out).</summary>
    private readonly HashSet<string> _fallen = new();

    /// <summary>Everyone in the room who stepped back out of the fight's way or left the room, and where they were standing before, to go back to.</summary>
    private readonly Dictionary<Npc, Vector2> _standingBack = new();

    /// <summary>Whoever of the party's sitting the fight out, and where they were standing before.</summary>
    private readonly Dictionary<WalkerSprite, Vector2> _benched = new();

    /// <summary>Which way each figure in the fight turns once it's got where it was walking: the party right, the foes left.</summary>
    private readonly Dictionary<WalkerSprite, Direction> _restFacing = new();

    /// <summary>The foes' moves seen this session, as enemy/ability, so their intent shows over them next time (11-combat §6.4).</summary>
    private readonly HashSet<string> _seenMoves = new();

    /// <summary>Where the fight's slots stand on the room's floor.</summary>
    private FightStage _stage;

    /// <summary>The names of those of the party who got up when it was over, for a line each once the panel's gone.</summary>
    private readonly List<string> _backUp = new();

    /// <summary>The fight, as the engine has it, and as the screen has shown it so far; null when there's no fight.</summary>
    private CombatEngine _combat;
    private ShownFight _shown;
    private EncounterDefinition _encounter;

    /// <summary>The difficulty this fight's fought on: the setting, Story if it's being fought again on Story, Balanced under the proof run.</summary>
    private Difficulty _fightDifficulty;

    /// <summary>The kits the fight's abilities come from.</summary>
    private readonly IKitSource _kits = AbilityCatalog.Interim;

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
    /// Starts a fight in this room: the autosave, the line-up, the slots laid out on the floor,
    /// everyone walking to theirs (and everyone else out of the way), the supports to their places,
    /// the song, the fight's name across the screen, and the initiative dice after it.
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
        _standingBack.Clear();
        _benched.Clear();
        _restFacing.Clear();
        _backUp.Clear();
        _fightEnded = false;
        _lastOfYours = null;
        Cancel();

        _fightDifficulty = ProofSeed is null ? RetryState.Start(encounter.Id, _audio.Settings.Mode) : Difficulty.Balanced;
        var rules = new FightRules(_fightDifficulty, flag => _game.Flags.Contains(flag), _kits);
        FightLineup lineup = FightRoster.Build(encounter, _game.Party, flag => _game.Flags.Contains(flag), _game.Meter);
        var setups = new List<CombatantSetup>(lineup.Setups);
        setups.AddRange(FightRoster.Foes(encounter));
        Vector2 leader = Leader.Position;
        float centre = StageCentre(encounter, leader);

        foreach (CombatantSetup setup in setups) _fighters[setup.Id] = FigureFor(setup);
        for (int i = 0; i < _game.Party.Count; i++)
            if (lineup.SittingOut.Contains(_game.Party[i])) _benched[_walkers[i]] = _walkers[i].Position;

        _combat = new CombatEngine(encounter, setups, _random, rules) { Bag = _game.Bag };
        _shown = new ShownFight(_combat);
        _stage = new FightStage(_room, centre);

        float walk = TakePlaces();
        PlaceSupports();
        walk = MathF.Max(walk, ClearTheFloor());

        _audio.PlaySong(encounter.Music);
        _log.Clear();
        _popups.Clear();
        _effects.Clear();
        _initiative.Clear();
        _result.Hide();
        _log.Add($"{encounter.Name.ToUpperInvariant()}: the fight begins.", Palette.Gold);
        if (_fightDifficulty != Difficulty.Balanced) _log.Add($"ON {_fightDifficulty.ToString().ToUpperInvariant()}.", Palette.Note);
        _banner.Show(encounter.Name.ToUpperInvariant(), RollLine, encounter.Boss ? Palette.Ember : Palette.Gold);

        BeginPlayback(MathF.Max(IntroSeconds, walk));
        Play(_combat.Begin());
        BeginLessons();
    }

    /// <summary>Where the screen wants the fight's middle: midway between the leader and the foes standing in the room, or with nobody standing, where it puts the leader in your middle lane.</summary>
    private float StageCentre(EncounterDefinition encounter, Vector2 leader)
    {
        float sum = 0f;
        int count = 0;
        foreach (EncounterFoe foe in encounter.Foes)
        {
            if (foe.Npc is not string id || _everyNpc.Find(each => each.Definition.Id == id) is not Npc npc) continue;
            sum += npc.Walker.Position.X;
            count++;
        }
        return count == 0 ? leader.X + FightStage.FrontGap / 2f + FightStage.LaneGap : (leader.X + sum / count) / 2f;
    }

    /// <summary>The figure for someone in the line-up: a party member's walker, the room's npc who turns, or one made for a foe or a guest who turns up.</summary>
    private WalkerSprite FigureFor(CombatantSetup setup)
    {
        int member = _game.Party.FindIndex(each => each.Id == setup.Id);
        if (member >= 0 && setup.Side == Side.Party && !setup.Guest) return _walkers[member];

        if (setup.Side == Side.Foes)
        {
            EncounterFoe foe = FoeOf(setup.Id);
            if (foe?.Npc is string npcId) return Turning(npcId).Walker;
            return Spawn(EnemyWalker(EnemyCatalog.Get(foe?.Enemy ?? setup.Enemy.Id)));
        }

        Npc standing = _npcs.Find(npc => npc.Definition.Id == setup.Id || npc.Definition.Cast == setup.Id);
        if (standing is not null) return standing.Walker;
        WalkerSprite walker = Spawn(GuestWalker(setup.Id));
        return walker;
    }

    /// <summary>The encounter's line for a foe, by its id in the fight, or null.</summary>
    private EncounterFoe FoeOf(string id)
    {
        for (int i = 0; i < _encounter.Foes.Count; i++)
            if (FightRoster.FoeId(_encounter.Foes[i], i) == id) return _encounter.Foes[i];
        return null;
    }

    /// <summary>The room's npc a foe line names, there or not: the fight brings them out whatever the flags say, since the encounter needs them.</summary>
    /// <remarks>Throws if the room has nobody of that id at all, which is a mistake in EncounterCatalog or the zone file (ZoneCatalog checks every encounter's npcs are in one room).</remarks>
    private Npc Turning(string npcId)
    {
        Npc npc = _everyNpc.Find(each => each.Definition.Id == npcId)
            ?? throw new InvalidOperationException($"Encounter {_encounter.Id} has npc {npcId}, but room {_room.Id} of Zones/{_zone.Id}.txt has nobody called that.");
        if (!_npcs.Contains(npc)) _npcs.Add(npc);
        return npc;
    }

    /// <summary>Gets a figure that isn't anyone in the room ready to fight: its shadow loaded, faded out until it arrives, and drawn by the fight.</summary>
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

    /// <summary>A figure for a guest who isn't standing in the room: a companion as their origin dresses them, or a stat block's.</summary>
    private WalkerSprite GuestWalker(string guest)
    {
        if (!Origins.Exists(guest)) return EnemyWalker(EnemyCatalog.Get(guest));

        PartyMember member = Origins.Create(guest);
        return new WalkerSprite { Sheet = SheetFor(member.Look, Paperdoll.GearOf(member)) };
    }

    /// <summary>Everyone to the slot the engine put them in: those in the room walk there at walking pace, those turning up fade in on it. Those who join later wait.</summary>
    /// <returns>How long the longest walk takes.</returns>
    private float TakePlaces()
    {
        float longest = 0f;
        foreach (Combatant each in _combat.Combatants)
        {
            if (!_fighters.TryGetValue(each.Id, out WalkerSprite walker) || FeetOf(each) is not Vector2 feet) continue;

            Direction facing = each.Side == Side.Party ? Direction.Right : Direction.Left;
            if (walker.Opacity <= 0f)
            {
                walker.Position = feet;
                walker.Facing = facing;
                FadeTo(walker, 1f, ArriveSeconds);
                _effects.Motes(walker.Position - new Vector2(0f, walker.Shape.Figure.Y / 2f), new Point((int)walker.Shape.Figure.X, (int)walker.Shape.Figure.Y), CombatStyle.Blessing, false);
                continue;
            }
            longest = MathF.Max(longest, WalkTo(walker, feet, facing, TakePlacesLeast, TakePlacesMost));
        }
        return longest;
    }

    /// <summary>Walks a figure somewhere at walking pace, kept between two lengths of time, turning it a way when it gets there.</summary>
    /// <returns>How long it takes.</returns>
    private float WalkTo(WalkerSprite walker, Vector2 to, Direction facing, float least, float most)
    {
        float seconds = Math.Clamp(Vector2.Distance(walker.Position, to) / WalkSpeed, least, most);
        Glide(walker, to, seconds);
        _restFacing[walker] = facing;
        return seconds;
    }

    /// <summary>Everyone in the room who isn't in the fight gets out of its way: back to the far edge of the floor to watch a fight to yield, or out by the nearest edge of the room for one that can kill. Whoever of the party sits it out steps back either way.</summary>
    /// <returns>How long the longest of it takes.</returns>
    private float ClearTheFloor()
    {
        var fighting = new HashSet<WalkerSprite>(_fighters.Values);
        fighting.UnionWith(_supportFigures.Values);
        float longest = 0f;

        foreach (Npc npc in _npcs)
        {
            if (fighting.Contains(npc.Walker) || npc.Definition.Aboard is not null) continue;
            _standingBack[npc] = npc.Walker.Position;
            Vector2 to = _encounter.Nonlethal ? StandBack(npc.Walker.Position) : OutOfTheRoom(npc.Walker.Position);
            Direction watching = npc.Walker.Position.X < _stage.Centre.X ? Direction.Right : Direction.Left;
            longest = MathF.Max(longest, WalkTo(npc.Walker, to, watching, TakePlacesLeast, StandBackMost));
        }

        foreach ((WalkerSprite walker, Vector2 home) in _benched)
        {
            if (fighting.Contains(walker)) continue;
            longest = MathF.Max(longest, WalkTo(walker, StandBack(home), Direction.Right, TakePlacesLeast, StandBackMost));
        }
        return longest;
    }

    /// <summary>Everyone who got out of the way comes back to where they were standing, while the panel at the end of the fight is up.</summary>
    private void ComeBack()
    {
        foreach ((Npc npc, Vector2 home) in _standingBack) WalkTo(npc.Walker, home, npc.Walker.Facing, TakePlacesLeast, StandBackMost);
        foreach ((WalkerSprite walker, Vector2 home) in _benched) WalkTo(walker, home, walker.Facing, TakePlacesLeast, StandBackMost);
    }

    /// <summary>Where someone standing at a point stands back to: the far edge of the floor behind them.</summary>
    /// <param name="feet">Where they were standing, in world pixels.</param>
    private Vector2 StandBack(Vector2 feet) => new(feet.X, _room.BackEdgeAt(feet.X) + StandBackInset);

    /// <summary>Where someone standing at a point leaves the room by: just past its nearer end, at the same depth.</summary>
    /// <param name="feet">Where they were standing, in world pixels.</param>
    private Vector2 OutOfTheRoom(Vector2 feet) => new(feet.X < _room.Width / 2f ? -EdgeOut : _room.Width + EdgeOut, feet.Y);

    /// <summary>Where someone in the fight stands, as shown: on their slot, or between their two; null if they've left the rows.</summary>
    /// <param name="who">Who.</param>
    private Vector2? FeetOf(Combatant who)
    {
        IReadOnlyList<Slot> slots = _shown.SlotsOf(who);
        if (slots.Count == 0) return null;
        Vector2 sum = Vector2.Zero;
        foreach (Slot slot in slots) sum += _stage.FeetAt(slot);
        return sum / slots.Count;
    }

    /// <summary>The figure of someone in the fight, by their id, or null.</summary>
    /// <param name="id">Their id in the fight.</param>
    private WalkerSprite FighterWalker(string id) => id is null ? null : _fighters.GetValueOrDefault(id);

    /// <summary>Whether someone in the room died in a fight earlier in this visit to the zone, so they stay out of it.</summary>
    /// <param name="npc">Who.</param>
    private bool Fallen(Npc npc) => _fallen.Contains(FallenKey(npc));

    /// <summary>What _fallen knows someone by: their room and their id, since a room's people are made again every time it's entered.</summary>
    private string FallenKey(Npc npc) => $"{_room.Id}/{npc.Definition.Id}";

    /// <summary>The fight's over and everything's been shown: on to the panel, game over with what the difficulty offers, or for a fight to lose, its panel too. Anyone who got out of the way comes back while it's up.</summary>
    private void EndOfFight()
    {
        _fightEnded = true;
        Cancel();
        ComeBack();

        if (_combat.Outcome == CombatOutcome.Victory)
        {
            RetryState.Won(_encounter.Id);
            var lines = new List<(string Text, Color Colour)>();
            AwardExperience(lines);

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
            RetryState.Lost(_encounter.Id);
            ScreenManager.AddScreen(new GameOverScreen(_encounter.Id, RetryState.Options(_encounter.Id, _fightDifficulty), ConcedeFight));
            return;
        }

        _audio.Play(Sfx.GameOver);
        _result.Show(DefeatHeading, Palette.Ember, new[] { ("THE FIGHT IS LOST, BUT NOT THE STORY.", Palette.Bone) }, Array.Empty<string>());
    }

    /// <summary>WIN THIS FIGHT, from the game-over screen: the fight's handed to the party, and the win plays out.</summary>
    private void ConcedeFight()
    {
        if (!InCombat) return;
        _fightEnded = false;
        Play(_combat.Concede());
    }

    /// <summary>Gives the whole party and whoever's waiting at camp the fight's experience, each at their own level (Leveling.AwardFight), so nobody falls behind the levels the story's fights are tuned for; and a line on the panel for everyone it takes up a level.</summary>
    /// <param name="lines">The panel's lines, to add to.</param>
    private void AwardExperience(List<(string Text, Color Colour)> lines)
    {
        int xp = LevelTable.FightXp(_encounter, _game.Party[0].Level);
        IReadOnlyList<string> levelled = Leveling.AwardFight(_encounter, _game.Party.Concat(_game.Camp));

        if (xp > 0) lines.Add(($"+{xp} XP", Palette.Gold));
        foreach (string line in levelled) lines.Add((line.ToUpperInvariant(), Palette.Heading));
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

    /// <summary>The panel's been read: the fight's packed away, the difficulty's healing after a story fight that was won, and the conversation after it plays (the one for having lost it, if it was lost).</summary>
    private void DismissResult()
    {
        bool won = _combat.Outcome == CombatOutcome.Victory;
        _result.Hide();
        if (won && _encounter.Victory is string flag) _game.Flags.Add(flag);

        // Nobody dies for good: anyone down or fallen gets up on 1 HP, then a won story fight heals
        // what the difficulty says.
        foreach (Combatant each in _combat.Combatants)
        {
            if (each.Member is null || each.Guest) continue;
            if (each.Member.Hp <= 0)
            {
                each.Member.Hp = 1;
                if (!_backUp.Contains(each.Name)) _backUp.Add(each.Name);
            }
            if (won && _encounter.Kind != EncounterKind.Random)
                each.Member.Hp += DifficultyRules.HealAfterStoryFight(_fightDifficulty, each.Member.Hp, each.Member.MaxHp);
        }

        string then = won ? _encounter.Then : _encounter.Lost;
        EndFight();
        foreach (string name in _backUp) _toast.Show($"{name} is back on their feet");
        Converse(DialogueCatalog.Get(then), null);
    }

    /// <summary>Packs the fight away: the dead out of the room, the supports gone or back where they were, everyone standing again where they are (and anyone who got out of the way where they stood before), the party following you again, and the zone's own song.</summary>
    private void EndFight()
    {
        _game.Fight = null;
        foreach (Combatant each in _combat.Combatants)
        {
            if (each.Side != Side.Foes || !each.IsDead) continue;
            Npc npc = _npcs.Find(standing => standing.Walker == FighterWalker(each.Id));
            if (npc is not null) _fallen.Add(FallenKey(npc));
        }

        foreach (WalkerSprite walker in _fighters.Values)
        {
            walker.Release();
            walker.IsWalking = false;
            if (!_spawned.Contains(walker)) walker.Opacity = 1f;
        }
        foreach ((Npc npc, Vector2 home) in _standingBack)
        {
            npc.Walker.Position = home;
            npc.Walker.IsWalking = false;
        }
        foreach ((WalkerSprite walker, Vector2 home) in _benched)
        {
            walker.Position = home;
            walker.IsWalking = false;
        }
        PackSupportsAway();

        _combat = null;
        _shown = null;
        _stage = null;
        _fighters.Clear();
        _spawned.Clear();
        _standingBack.Clear();
        _benched.Clear();
        _restFacing.Clear();
        _glides.Clear();
        _fades.Clear();
        _barks.Clear();
        _effects.Clear();
        _popups.Clear();
        _banner.Clear();
        _game.Leader = Math.Clamp(_leaderBeforeFight, 0, _walkers.Count - 1);

        _audio.PlaySong(_zone.Music);
        RefreshPresence();
        BuildNav();
        SeedTrail();
    }

    /// <summary>Opens the pause menu in a fight: no INVENTORY (what's worn can't change mid-fight), a line that says why there's no saving, and the difficulty held where it is.</summary>
    private void OpenCombatPause()
    {
        Halt();
        ScreenManager.AddScreen(new PauseMenuScreen(null, NoSaving, lockDifficulty: true));
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
        _supportBanner.LoadContent(_content, _combatArt);
        _initiative.LoadContent(_content, _combatArt, pixel, _supportBanner);
        _hotbar.LoadContent(_content, _combatArt, pixel);
        _budgetBar.LoadContent(_content, _combatArt);
        _sigil.LoadContent(_content, _combatArt);
        _unitFrame.LoadContent(_content, _combatArt, pixel);
        _result.LoadContent(_content, pixel);
    }
}
