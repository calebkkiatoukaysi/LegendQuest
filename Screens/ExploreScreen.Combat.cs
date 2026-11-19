using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components;
using LegendQuest.Components.Realtime;
using LegendQuest.Dialogue;
using LegendQuest.Items;
using LegendQuest.Rules;
using LegendQuest.Story;
using LegendQuest.Tutorial;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// A fight in the room you're in: starting one where everyone stands, and how it ends, with the
/// loot, the experience, the class points and the levels and the conversation after, or the
/// conversation for a fight you were allowed to lose, or game over with what the difficulty offers.
/// </summary>
/// <remarks>
/// <para>
/// There's no arena and nobody walks to a slot (Story/11-combat.md §2.1): a conversation ends with
/// DialogueEffect.Fight and the fight starts when it's over, in place. The game's autosaved first, so
/// a lost fight can be fought again. BattleRoster says who's in it; everyone standing in the room
/// fights from where their feet are (the party, the room's people who turn, the guests who are
/// here), and the foes and guests who aren't here turn up at the encounter's places on the floor
/// (BattleFloor builds the arena from the room's band and fight range). The Battle is made with the
/// difficulty, the story's flags, the bag, the room's dark and sky, you as the leader, and a seed
/// (the proof run's own, so its fights replay). While it's on the room's ways out, doors, triggers
/// and scenes wait; anyone standing about who isn't in it steps back to the far edge of the floor to
/// watch a fight to yield, or leaves by the nearest edge of the room for one that can kill, and both
/// come back after. Whoever of the party sits it out steps back and watches.
/// </para>
/// <para>
/// The fight itself is in partials of its own: ExploreScreen.CombatLoop steps the engine and places
/// everyone, ExploreScreen.CombatCues is what each thing that happens looks and sounds like on the
/// one it happens to, ExploreScreen.CombatInput is the mouse and the keys, ExploreScreen.CombatHudView
/// turns the fight into the HUD's views, ExploreScreen.CombatDraw draws it, ExploreScreen.CombatCards
/// says what someone is, ExploreScreen.CombatAllies has the supports and the guests,
/// ExploreScreen.CombatTutorial teaches it, and ExploreScreen.CombatProof drives it for the proof run.
/// Nobody in the party dies for good: everyone's back up on 1 HP when it's won, or when a fight that's
/// allowed to be lost is. A foe who dies leaves the room for the rest of the visit; its presence flag
/// keeps it gone after.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>The most anyone standing about takes to step back out of the fight's way or leave the room, and to come back once it's over.</summary>
    private const float StandBackLeast = 0.35f;
    private const float StandBackMost = 1.6f;

    /// <summary>How far onto the floor from its back edge they stand to watch, in world pixels.</summary>
    private const float StandBackInset = 1f;

    /// <summary>How far past the room's edge someone leaving a lethal fight walks, in world pixels, so they're off the floor.</summary>
    private const float EdgeOut = 24f;

    /// <summary>How long someone who turns up takes to fade in.</summary>
    private const float ArriveSeconds = 0.5f;

    /// <summary>What the pause menu says in a fight, instead of its usual line.</summary>
    private const string NoSaving = "NO SAVING MID-FIGHT: THE AUTOSAVE IS FROM JUST BEFORE IT.";

    /// <summary>The panel's headings at the end.</summary>
    private const string VictoryHeading = "VICTORY";
    private const string DefeatHeading = "DEFEAT";

    /// <summary>The meter that darkens the world in the second act, and the stage from which the sun's failing counts as dark for the skills that care (Story/05-script-act2.md).</summary>
    private const string EclipseMeter = "eclipse-stage";
    private const int EclipseDark = 3;

    /// <summary>The fight's sheets, and everything on the screen that's only there in a fight.</summary>
    private readonly CombatArt _combatArt = new();
    private readonly CombatEffects _effects = new();
    private readonly DamagePopups _popups = new();
    private readonly TurnBanner _banner = new();
    private readonly CombatLog _log = new();
    private readonly CombatCard _card = new();
    private readonly CombatHud _hud = new();
    private readonly SoulLinkSigil _sigil = new();
    private readonly SupportBanner _supportBanner = new();
    private readonly CombatResult _result = new();

    /// <summary>Everyone fighting's figure, by their id in the fight: the party's own walkers, the room's people who turned, and anyone who turned up.</summary>
    private readonly Dictionary<string, WalkerSprite> _fighters = new();

    /// <summary>The figures of those who turned up for the fight (a foe, a guest or a support from nowhere), which aren't anyone in the room, so the fight draws and runs them itself.</summary>
    private readonly List<WalkerSprite> _spawned = new();

    /// <summary>The room's people who died in a fight, as room/id, kept out of their room for the rest of this visit to the zone (RefreshPresence leaves them out).</summary>
    private readonly HashSet<string> _fallen = new();

    /// <summary>Everyone in the room who stepped back out of the fight's way or left the room, and where they were standing before, to go back to.</summary>
    private readonly Dictionary<Npc, Vector2> _standingBack = new();

    /// <summary>Whoever of the party's sitting the fight out, and where they were standing before.</summary>
    private readonly Dictionary<WalkerSprite, Vector2> _benched = new();

    /// <summary>Which way each figure turns once it's got where it was walking out of the fight's way.</summary>
    private readonly Dictionary<WalkerSprite, Direction> _restFacing = new();

    /// <summary>The names of those of the party who got up when it was over, for a line each once the panel's gone.</summary>
    private readonly List<string> _backUp = new();

    /// <summary>The fight, or null when there's none.</summary>
    private Battle _battle;
    private EncounterDefinition _encounter;

    /// <summary>The difficulty this fight's fought on: the setting (Balanced under the proof run), or Story if it's being fought again on Story.</summary>
    private Difficulty _fightDifficulty;

    /// <summary>The encounter a conversation that's up has asked to start when it's over (OnDialogue notes it), or null.</summary>
    private string _fightAfterTalk;

    /// <summary>Who you were controlling before the fight, to go back to after it.</summary>
    private int _leaderBeforeFight;

    /// <summary>Whether the fight's sheets and components have been loaded on this screen.</summary>
    private bool _combatLoaded;

    /// <summary>Whether the end of the fight has been dealt with: the panel's up, or game over.</summary>
    private bool _fightEnded;

    /// <summary>Whether the fight was handed to the party from the game-over screen (WIN THIS FIGHT), so it ends as won whatever the engine says.</summary>
    private bool _handedWin;

    /// <summary>Whether a fight's on.</summary>
    private bool InCombat => _battle is not null;

    /// <summary>Whether the fight's won, or was handed to the party.</summary>
    private bool FightWon => _handedWin || _battle?.Outcome == FightOutcome.Won;

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
    /// Starts a fight in this room, where everyone stands: the autosave, who's in it and where, the
    /// arena from the room's floor, the Battle, the supports to their places and everyone else out of
    /// the way, the battle's music, and the fight's name across the screen.
    /// </summary>
    /// <param name="encounter">The fight.</param>
    private void StartFight(EncounterDefinition encounter)
    {
        if (InCombat) return;

        LoadCombat();
        Halt();
        StopPath();
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
        _handedWin = false;

        _fightDifficulty = RetryState.Start(encounter.Id, ProofSeed is null ? _audio.Settings.Mode : Difficulty.Balanced);
        bool Flag(string flag) => _game.Flags.Contains(flag);
        List<CombatantSetup> setups = BattleRoster.Build(encounter, _game.Party, Flag);

        // You lead, as the player's own character; if you're sitting it out, whoever of the party's first.
        string leaderId = setups.Any(each => each.Id == Origins.You && each.Member is not null && !each.Guest)
            ? Origins.You
            : setups.FirstOrDefault(each => each.Side == Side.Party && !each.Guest)?.Id;
        int leaderIndex = _game.Party.FindIndex(each => each.Id == leaderId);
        if (leaderIndex >= 0) _game.Leader = leaderIndex;

        // Everyone standing in the room fights from where their feet are; the rest turn up.
        for (int i = 0; i < setups.Count; i++)
        {
            WalkerSprite walker = FigureFor(setups[i]);
            _fighters[setups[i].Id] = walker;
            if (!_spawned.Contains(walker)) setups[i] = setups[i] with { At = ToEngine(walker.Position) };
        }
        for (int i = 0; i < _game.Party.Count; i++)
            if (!_fighters.ContainsKey(_game.Party[i].Id)) _benched[_walkers[i]] = _walkers[i].Position;

        BattleArena arena = BattleFloor.For(_room, Leader.Position, FoesStanding(setups));
        var rules = new BattleRules(_fightDifficulty, Flag, leaderId, _game.Bag)
        {
            Dark = _room.Dark || _game.Meter(EclipseMeter) >= EclipseDark,
            Outdoors = _room.Kind == RoomKind.Street,
        };
        ulong seed = ProofSeed is int proof ? (ulong)proof : (ulong)_random.NextInt64();
        _battle = new Battle(encounter, setups, rules, seed, arena);

        foreach (Fighter fighter in _battle.Fighters)
        {
            if (!_fighters.TryGetValue(fighter.Id, out WalkerSprite walker)) continue;
            Ready(walker, fighter);
            if (!fighter.Present) continue;
            walker.Position = ToScreen(fighter.Position);
            if (_spawned.Contains(walker)) ArriveIn(walker);
        }

        PlaceSupports();
        ClearTheFloor();
        ApplyFightSettings();

        _audio.BeginBattle(encounter.Music);
        _log.Clear();
        _popups.Clear();
        _effects.Clear();
        _hud.Reset();
        _result.Hide();
        _log.Add($"{encounter.Name.ToUpperInvariant()}: the fight begins.", Palette.Gold);
        if (_fightDifficulty != Difficulty.Balanced) _log.Add($"ON {_fightDifficulty.ToString().ToUpperInvariant()}.", Palette.Note);
        _banner.Show(encounter.Name.ToUpperInvariant(), BannerLine(encounter), encounter.Boss ? Palette.Ember : Palette.Gold);

        BeginLoop();
        BeginLessons();
    }

    /// <summary>What goes under the fight's name as it starts: what ends it, if it isn't beating every foe.</summary>
    private static string BannerLine(EncounterDefinition encounter)
    {
        if (encounter.SurviveSeconds > 0) return $"HOLD FOR {encounter.SurviveSeconds} SECONDS";
        if (encounter.YieldsAt > 0f || encounter.Nonlethal) return "TO YIELD, NOT TO THE DEATH";
        return encounter.Boss ? "A FIGHT FOR YOUR LIVES" : "TO ARMS";
    }

    /// <summary>Where the foes standing in the room stand, on average, or null if none is.</summary>
    private Vector2? FoesStanding(IReadOnlyList<CombatantSetup> setups)
    {
        Vector2 sum = Vector2.Zero;
        int count = 0;
        foreach (CombatantSetup setup in setups)
        {
            if (setup.Side != Side.Foes || setup.At is not System.Numerics.Vector2 at) continue;
            sum += ToScreen(at);
            count++;
        }
        return count == 0 ? null : sum / count;
    }

    /// <summary>The settings a fight's drawn by, taken when it starts and again every frame (the options can be changed from the pause menu mid-fight): how hard the screen shakes, whether blows flash, and whether the numbers come up.</summary>
    private void ApplyFightSettings()
    {
        WalkerSprite.Flashes = _audio.Settings.HitFlashes;
        _effects.Shaker.Strength = _audio.Settings.Shake;
        _popups.Shown = _audio.Settings.DamageNumbers;
    }

    /// <summary>A figure made ready for the fight: the ready stance, and the weapon the engine says it fights with.</summary>
    private static void Ready(WalkerSprite walker, Fighter fighter)
    {
        walker.InFight = true;
        walker.WeaponKind = WeaponKinds.Clip(fighter.Form?.Weapon ?? fighter.WeaponKind);
        walker.Facing = fighter.Facing < 0 ? Direction.Left : Direction.Right;
        walker.Release();
    }

    /// <summary>A figure who's turned up fading in where it stands, in a cloud of motes.</summary>
    private void ArriveIn(WalkerSprite walker)
    {
        walker.Opacity = 0f;
        FadeTo(walker, 1f, ArriveSeconds);
        _effects.Motes(walker.Position - new Vector2(0f, walker.Size.Y / 2f), new Point((int)walker.Size.X, (int)walker.Size.Y), CombatStyle.Blessing, false);
    }

    /// <summary>A point in the room as the engine has it.</summary>
    private static System.Numerics.Vector2 ToEngine(Vector2 point) => new(point.X, point.Y);

    /// <summary>A point in the engine as the room has it.</summary>
    private static Vector2 ToScreen(System.Numerics.Vector2 point) => new(point.X, point.Y);

    /// <summary>The figure for someone in the fight: a party member's walker, the room's npc who turns, a guest standing here, or one made for a foe or a guest who turns up.</summary>
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
        return standing?.Walker ?? Spawn(GuestWalker(setup.Id));
    }

    /// <summary>The figure for someone who joins partway through: theirs if they're in the fight or standing in the room, or one made for them.</summary>
    private WalkerSprite FigureForJoiner(Fighter fighter)
    {
        if (_fighters.TryGetValue(fighter.Id, out WalkerSprite known)) return known;
        WalkerSprite walker;
        if (fighter.Side == Side.Foes && fighter.Enemy is EnemyDefinition enemy) walker = Spawn(EnemyWalker(enemy));
        else if (fighter.Kind == FighterKind.Summon && fighter.Enemy is EnemyDefinition summoned) walker = Spawn(SummonWalker(summoned));
        else
        {
            Npc standing = _npcs.Find(npc => npc.Definition.Id == fighter.Id || npc.Definition.Cast == fighter.Id);
            walker = standing?.Walker ?? Spawn(GuestWalker(fighter.Id));
        }
        _fighters[fighter.Id] = walker;
        Ready(walker, fighter);
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

    /// <summary>A figure for a foe from its stat block: its creature's sheet, or its cast member's, and its imported picture if it has one.</summary>
    private WalkerSprite EnemyWalker(EnemyDefinition enemy)
    {
        if (enemy.Creature is not null)
        {
            Creature creature = Creatures.Get(enemy.Creature);
            return new WalkerSprite { Shape = creature.Shape, Sheet = CreatureSheet(creature), Art = ImportedArt.Get(enemy.Id) };
        }

        CastMember cast = Cast.Get(enemy.Cast);
        Texture2D sheet = cast.Sheet is not null ? _content.Load<Texture2D>(Townsfolk.Texture(cast.Sheet)) : SheetFor(cast.Look);
        return new WalkerSprite { Sheet = sheet, Art = ImportedArt.Get(enemy.Id) };
    }

    /// <summary>A figure for someone a skill calls in on the party's side (the Watch, the Order's knights): their cast's look, else their stat block's.</summary>
    private WalkerSprite SummonWalker(EnemyDefinition enemy)
    {
        string look = enemy.Id switch
        {
            "watchman" => "watch",
            "gilded-order-knight" => "royal-guard",
            _ => null,
        };
        if (look is not null && MadeFigure(look) is WalkerSprite made) return made;
        return EnemyWalker(enemy);
    }

    /// <summary>A figure for a guest who isn't standing in the room: a companion as their origin dresses them, or a stat block's, and their imported picture if they've one.</summary>
    private WalkerSprite GuestWalker(string guest)
    {
        if (!Origins.Exists(guest)) return EnemyWalker(EnemyCatalog.Get(guest));

        PartyMember member = Origins.Create(guest);
        return new WalkerSprite { Sheet = SheetFor(member.Look, Paperdoll.GearOf(member)), Art = ImportedArt.For(member) };
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
    private void ClearTheFloor()
    {
        var fighting = new HashSet<WalkerSprite>(_fighters.Values);
        fighting.UnionWith(_supportFigures.Values);
        float middle = Leader.Position.X;

        foreach (Npc npc in _npcs)
        {
            if (fighting.Contains(npc.Walker) || npc.Definition.Aboard is not null) continue;
            _standingBack[npc] = npc.Walker.Position;
            Vector2 to = _encounter.Nonlethal ? StandBack(npc.Walker.Position) : OutOfTheRoom(npc.Walker.Position);
            Direction watching = npc.Walker.Position.X < middle ? Direction.Right : Direction.Left;
            WalkTo(npc.Walker, to, watching, StandBackLeast, StandBackMost);
        }

        foreach ((WalkerSprite walker, Vector2 home) in _benched)
        {
            if (fighting.Contains(walker)) continue;
            WalkTo(walker, StandBack(home), Direction.Right, StandBackLeast, StandBackMost);
        }
    }

    /// <summary>Everyone who got out of the way comes back to where they were standing, while the panel at the end of the fight is up.</summary>
    private void ComeBack()
    {
        foreach ((Npc npc, Vector2 home) in _standingBack) WalkTo(npc.Walker, home, npc.Walker.Facing, StandBackLeast, StandBackMost);
        foreach ((WalkerSprite walker, Vector2 home) in _benched) WalkTo(walker, home, walker.Facing, StandBackLeast, StandBackMost);
    }

    /// <summary>Where someone standing at a point stands back to: the far edge of the floor behind them.</summary>
    /// <param name="feet">Where they were standing, in world pixels.</param>
    private Vector2 StandBack(Vector2 feet) => new(feet.X, _room.BackEdgeAt(feet.X) + StandBackInset);

    /// <summary>Where someone standing at a point leaves the room by: just past its nearer end, at the same depth.</summary>
    /// <param name="feet">Where they were standing, in world pixels.</param>
    private Vector2 OutOfTheRoom(Vector2 feet) => new(feet.X < _room.Width / 2f ? -EdgeOut : _room.Width + EdgeOut, feet.Y);

    /// <summary>The figure of someone in the fight, by their id, or null.</summary>
    /// <param name="id">Their id in the fight.</param>
    private WalkerSprite FighterWalker(string id) => id is null ? null : _fighters.GetValueOrDefault(id);

    /// <summary>Someone in the fight by their id, or null.</summary>
    private Fighter FighterOf(string id) => _battle?.Find(id);

    /// <summary>Whether someone in the room died in a fight earlier in this visit to the zone, so they stay out of it.</summary>
    /// <param name="npc">Who.</param>
    private bool Fallen(Npc npc) => _fallen.Contains(FallenKey(npc));

    /// <summary>What _fallen knows someone by: their room and their id, since a room's people are made again every time it's entered.</summary>
    private string FallenKey(Npc npc) => $"{_room.Id}/{npc.Definition.Id}";

    /// <summary>The fight's over and everything's been shown: on to the panel, game over with what the difficulty offers, or for a fight to lose, its panel too. Anyone who got out of the way comes back while it's up.</summary>
    private void EndOfFight()
    {
        _fightEnded = true;
        ComeBack();

        if (FightWon)
        {
            RetryState.Won(_encounter.Id);
            _battle.ApplyToParty();
            var lines = new List<(string Text, Color Colour)>();
            AwardExperience(lines);

            (int gold, List<string> loot) = RollLoot();
            if (gold > 0) lines.Add(($"+{gold} GOLD", Palette.Gold));
            if (loot.Count > 0) lines.Add(("INTO THE BAG", Palette.Note));

            _audio.EndBattle(true);
            _result.Show(VictoryHeading, Palette.Gold, lines, loot);
            Lesson(TutorialCatalog.CombatVictory);
            return;
        }

        _battle.ApplyToParty();
        if (_encounter.Lost is null)
        {
            RetryState.Lost(_encounter.Id);
            ScreenManager.AddScreen(new GameOverScreen(_encounter.Id, RetryState.Options(_encounter.Id, _fightDifficulty), ConcedeFight));
            return;
        }

        _audio.EndBattle(false);
        _result.Show(DefeatHeading, Palette.Ember, new[] { ("THE FIGHT IS LOST, BUT NOT THE STORY.", Palette.Bone) }, Array.Empty<string>());
    }

    /// <summary>WIN THIS FIGHT, from the game-over screen: the fight's handed to the party, and the win plays out.</summary>
    private void ConcedeFight()
    {
        if (!InCombat) return;
        _handedWin = true;
        _fightEnded = false;
        _endClock = 0f;
    }

    /// <summary>Gives the whole party and whoever's waiting at camp the fight's experience, each at their own level (Leveling.AwardFight), and the class points to everyone who fought (ClassRanks.AwardFight); a line on the panel for each, and for everyone it takes up a level or a rank.</summary>
    /// <param name="lines">The panel's lines, to add to.</param>
    private void AwardExperience(List<(string Text, Color Colour)> lines)
    {
        int xp = LevelTable.FightXp(_encounter, _game.Party[0].Level);
        IReadOnlyList<string> levelled = Leveling.AwardFight(_encounter, _game.Party.Concat(_game.Camp));

        var fought = new List<PartyMember>();
        foreach (Fighter fighter in _battle.Fighters)
            if (fighter.Kind == FighterKind.Member && fighter.Member is PartyMember member) fought.Add(member);
        int points = ClassRanks.Award(_encounter.Kind);
        IReadOnlyList<string> ranked = ClassRanks.AwardFight(_encounter.Kind, fought);

        if (xp > 0) lines.Add(($"+{xp} XP", Palette.Gold));
        if (points > 0) lines.Add(($"+{points} CLASS POINTS", Palette.Gold));
        foreach (string line in levelled) lines.Add((line.ToUpperInvariant(), Palette.Heading));
        foreach (string line in ranked) lines.Add((line.ToUpperInvariant(), Palette.Heading));
    }

    /// <summary>Rolls what every foe who died was carrying and the fight's own treasure, adds the gold lifted mid-fight and the party's share of the takings, and puts it in the gold and the bag.</summary>
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

        foreach (Fighter fighter in _battle.Fighters)
            if (fighter.Side == Side.Foes && fighter.IsDead && fighter.Enemy?.LootTable is string drops) Take(drops);
        if (_encounter.LootTable is string treasure) Take(treasure);

        // Pick Pocket and its like: a share of a foe's purse, whether or not it fell.
        foreach ((string foe, int share) in _battle.Stolen)
            if (_battle.Find(foe)?.Enemy?.LootTable is string purse)
                gold += LootTableCatalog.Get(purse).Roll(_random).Gold * share / 100;
        gold += gold * _battle.GoldBonusPct / 100;

        Inventory bag = _game.Bag;
        foreach (string id in loot) bag.Add(id);
        _game.Gold += gold;
        return (gold, loot);
    }

    /// <summary>The panel's been read: the fight's packed away, the difficulty's healing after a story fight that was won, and the conversation after it plays (the one for having lost it, if it was lost).</summary>
    private void DismissResult()
    {
        bool won = FightWon;
        _result.Hide();
        if (won && _encounter.Victory is string flag) _game.Flags.Add(flag);

        // Nobody dies for good: anyone down or fallen gets up on 1 HP, then a won story fight heals
        // what the difficulty says.
        foreach (Fighter fighter in _battle.Fighters)
        {
            if (fighter.Kind != FighterKind.Member || fighter.Member is not PartyMember member) continue;
            if (member.Hp <= 0)
            {
                member.Hp = 1;
                if (!_backUp.Contains(fighter.Name)) _backUp.Add(fighter.Name);
            }
            if (won && _encounter.Kind != EncounterKind.Random)
                member.Hp += DifficultyRules.HealAfterStoryFight(_fightDifficulty, member.Hp, member.MaxHp);
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
        foreach (Fighter fighter in _battle.Fighters)
        {
            if (fighter.Side != Side.Foes || !fighter.IsDead) continue;
            Npc npc = _npcs.Find(standing => standing.Walker == FighterWalker(fighter.Id));
            if (npc is not null) _fallen.Add(FallenKey(npc));
        }

        foreach (WalkerSprite walker in _fighters.Values)
        {
            walker.Release();
            walker.InFight = false;
            walker.WeaponKind = null;
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

        _battle = null;
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
        EndLoop();
        _game.Leader = Math.Clamp(_leaderBeforeFight, 0, _walkers.Count - 1);

        _audio.PlaySong(_zone.Music);
        ClearedField();
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

    /// <summary>Loads the fight's sheets and components the first time a fight (or a scene's sigil) needs them on this screen.</summary>
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
        _hud.LoadContent(_content, ScreenManager.GraphicsDevice, pixel);
        _supportBanner.LoadContent(_content, _combatArt);
        _sigil.LoadContent(_content, _combatArt);
        _result.LoadContent(_content, pixel);
    }
}
