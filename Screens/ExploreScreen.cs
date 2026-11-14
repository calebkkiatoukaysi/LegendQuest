using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Audio;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Components;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// A zone being walked: one of its side-on rooms at a time, its painting, its props, the things you
/// can use, the people, the party, and the dark if it's a dark room.
/// </summary>
/// <remarks>
/// The Black Box's lobby screen, reading its rooms out of a zone file instead of having one in code.
/// A room is the screen's height and anything up to three screens wide, so the camera slides along it
/// and stops at the ends, and everyone on its floor band is sorted by how far down the band their feet
/// are. The rooms of a zone swap inside the one screen, with a quick fade and an autosave; another
/// zone is another screen, through the loading screen. It's in partials: this one walks, draws and
/// swaps rooms, ExploreScreen.Party has the party and the followers, ExploreScreen.Pointer has the
/// mouse, ExploreScreen.World has using things, talking, triggers, doors and the ways out,
/// ExploreScreen.Banter has the banter and the lines over people's heads, ExploreScreen.Markers has
/// the "!"s, ExploreScreen.Camp has whoever's waiting at camp, ExploreScreen.Tutorial has the
/// tutorial's tips, ExploreScreen.Story has the conversations and the ship coming in, and
/// ExploreScreen.Stage is the room as a conversation's stage, where its cues play. A fight is
/// ExploreScreen.Combat and the partials named after it, which take over the input, the running and
/// the drawing while it's on.
/// </remarks>
public partial class ExploreScreen : GameScreen
{
    /// <summary>How fast you walk, in art pixels per second.</summary>
    /// <remarks>About three and a half tiles a second. The Black Box's 230 screen pixels was nearly five, which looks like running at this size.</remarks>
    private const float WalkSpeed = 56f;

    /// <summary>The smallest push that counts as walking, squared. Under it the stick's resting slop doesn't move anyone.</summary>
    private const float MinimumPush = 0.04f;

    /// <summary>How hard someone has to be pushed across the room to turn that way. Walking deeper or nearer with a little sideways in it keeps the way they face.</summary>
    private const float TurnPush = 0.2f;

    /// <summary>How long the zone takes to fade in and out.</summary>
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.8);

    /// <summary>How long a room takes to fade out when you leave it for another in the zone, and the next to fade in, in seconds.</summary>
    private const float RoomFadeSeconds = 0.25f;

    /// <summary>Footsteps: how loud, how much the pitch varies, and how far they pan left and right.</summary>
    private const float StepVolume = 0.8f;
    private const float StepPitch = 0.1f;
    private const float StepPan = 0.12f;

    /// <summary>The flickering light over each torch in a dark room: how far it reaches (art pixels) and its colour at full strength.</summary>
    private const float GlowRadius = 40f;
    private static readonly Color GlowColor = new Color(255, 150, 70) * 0.16f;

    /// <summary>Where the heading text goes.</summary>
    private const float HeadingY = 14f;
    private const float Margin = 24f;
    private const string Hint = "TAB  ·  SWITCH      ALT  ·  SHOW ALL      M  ·  ZONE MAP      ESC  ·  PAUSE";

    /// <summary>What goes between the zone's name and the room's in the heading.</summary>
    private const string HeadingJoin = "  ·  ";

    /// <summary>The smudge behind the heading, and how far it spreads round the text, so it reads on a bright sky as well as in the barrow.</summary>
    private static readonly Color HeadingBacking = Color.Black * 0.8f;
    private static readonly Vector2 HeadingSpread = new(28f, 12f);

    /// <summary>The soft dark band across the top of the screen that the heading, the keys and the objective sit on, so they read over a busy sky: how dark it is, how far down it stays that dark (past the objective), and how far under that it takes to fade to nothing. Screen pixels.</summary>
    private const float BandDarkness = 0.6f;
    private const int BandSolid = 82;
    private const int BandFade = 72;

    /// <summary>How far down from where they'd be the toasts go in a room, in screen pixels, so they're under the band and never on the objective.</summary>
    private const float ToastDrop = 44f;

    private readonly InputAction _pause = new(new[] { Buttons.Start, Buttons.Back }, new[] { Keys.Escape }, true);
    private readonly InputAction _walkUp = new(new[] { Buttons.DPadUp }, new[] { Keys.W, Keys.Up }, false);
    private readonly InputAction _walkDown = new(new[] { Buttons.DPadDown }, new[] { Keys.S, Keys.Down }, false);
    private readonly InputAction _walkLeft = new(new[] { Buttons.DPadLeft }, new[] { Keys.A, Keys.Left }, false);
    private readonly InputAction _walkRight = new(new[] { Buttons.DPadRight }, new[] { Keys.D, Keys.Right }, false);
    private readonly InputAction _showEverything = new(new[] { Buttons.LeftShoulder }, new[] { Keys.LeftAlt, Keys.RightAlt }, false);

    private readonly SaveData _game;
    private readonly ZoneDefinition _zone;
    private readonly List<PropSprite> _props = new();
    private readonly List<PropSprite> _everyProp = new();
    private readonly List<TorchSprite> _torches = new();
    private readonly List<Npc> _npcs = new();
    private readonly List<Npc> _everyNpc = new();
    private readonly Camera2D _camera = new(new Point(LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight));
    private readonly Paperdoll _paperdoll = new();
    private readonly Random _random = ProofSeed is int seed ? new Random(seed) : new Random();

    /// <summary>The ids the room file gives its props, for the ones it names, so a scene can show or hide them.</summary>
    private readonly Dictionary<PropSprite, string> _propIds = new();

    /// <summary>The seed every zone's dice start from under the proof run, so its fights go the same way every time, or null to roll freely.</summary>
    internal static int? ProofSeed { get; set; }

    /// <summary>The party's baked sheets, one per look, so two who look alike share one.</summary>
    private readonly Dictionary<string, Texture2D> _sheets = new();

    /// <summary>The room you're in, the painting of it, and walking in it.</summary>
    private RoomDefinition _room;
    private RoomScene _scene;
    private RoomWalker _walk;

    /// <summary>The room you're on your way into and the spawn you'll be on there while the room you're leaving fades out, or null.</summary>
    private (string Room, string Spawn)? _roomChange;

    /// <summary>How dark the fade between two rooms is, 0 (none) to 1 (black), and whether it's coming back in.</summary>
    private float _roomFade;
    private bool _roomFadingIn;

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _detailFont;
    private Texture2D _glow;
    private Texture2D _band;
    private bool _stepLeft;

    /// <summary>Whether Alt's held, which shows every badge, name, label and glint at once.</summary>
    private bool _showAll;

    /// <summary>Puts the game in the zone and room its save says it's in.</summary>
    /// <param name="game">The game being played.</param>
    public ExploreScreen(SaveData game)
    {
        _game = game;
        _zone = ZoneCatalog.Get(game.ZoneId);
        _room = _zone.Room(game.RoomId ?? string.Empty) ?? _zone.RoomOfSpawn(ZoneLoader.StartSpawn);
        _world = new WorldState(game, _zone.Id);

        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        foreach (PartyMember member in game.Party)
        {
            _walkers.Add(new WalkerSprite());
            _members.Add(member);
        }
    }

    /// <summary>Which zone this is, so the proof run and the zone map can tell one zone's screen from the last one's.</summary>
    internal string ZoneId => _zone.Id;

    /// <summary>Which room of the zone you're in.</summary>
    internal string RoomId => _room.Id;

    /// <summary>Loads the room and everyone in it, puts the party where the save says, and starts the zone's song.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();
        _cursor = ScreenManager.Game.Services.GetService<CursorSprite>();

        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        _glow = _content.Load<Texture2D>("glow");
        _marks = _content.Load<Texture2D>("map-marks");
        _markers = _content.Load<Texture2D>("markers");
        _band ??= BuildBand();
        _toast.LoadContent(_content);
        _tooltip.LoadContent(_content);
        _strip.LoadContent(_content, ScreenManager.GraphicsDevice);
        _indicators.LoadContent(_content);
        LoadTutorial();

        // The party is baked off the paperdoll, each to how they look and what they're wearing.
        _paperdoll.LoadContent(_content);
        for (int i = 0; i < _walkers.Count; i++) DressWalker(_walkers[i], _game.Party[i]);

        EnterRoom();
        Arrive();
        _camera.SnapTo(Leader.Position);
        _chapterCard.LoadContent(_content);
        BeginArrival();

        _audio.PlaySong(_zone.Music);
    }

    /// <summary>Unloads the room and the zone, and disposes everything the content manager didn't make: the band across the top, the baked sheets and the strip's pixel.</summary>
    public override void Unload()
    {
        _scene?.Unload();
        _band?.Dispose();
        foreach (Texture2D sheet in _sheets.Values) sheet.Dispose();
        _strip.Unload();
        _content?.Unload();
    }

    /// <summary>The pause menu, a tip's card, the zone map, switching who you control, using things, the mouse, and walking. Nothing while a room's fading.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        _showAll = _proofAlt || _showEverything.Occurred(input);
        if (_roomChange is not null || _roomFade > 0f) return;
        if (HandleArrivalInput(input)) return;
        if (HandleCombatInput(gameTime, input)) return;

        if (_pause.Occurred(input))
        {
            Halt();
            ScreenManager.AddScreen(new PauseMenuScreen(() => OpenInventory(InventoryPage.Bag)));
            return;
        }

        if (HandleTutorialInput(input)) return;
        if (HandleInventoryInput(input)) return;

        if (_openMap.Occurred(input))
        {
            OpenMap(null);
            return;
        }

        if (HandleSwitching(input)) return;

        if (_use.Occurred(input) && UseNearest()) return;

        HandlePointer(input);

        // The keys always win over a path the mouse set going.
        Vector2 push = ReadMovement(input);
        if (push.LengthSquared() >= MinimumPush)
        {
            StopPath();
            Walk(Leader, push, WalkSpeed, gameTime, null);
        }
        else if (!FollowPath(gameTime))
        {
            Leader.IsWalking = false;
        }
    }

    /// <summary>Runs everything that moves on its own (the party, the people, the torches, the room's life, the camera, the clock, and a scene's shake or tint still settling), checks what you've walked into, then runs the banter and lets the townsfolk talk. While a room's fading, only the fade.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        UpdateTutorial(gameTime);

        // Paused, so nothing moves, not even the clock, and the banter's line waits to be read.
        if (!IsActive)
        {
            foreach (WalkerSprite walker in _walkers) walker.IsWalking = false;
            HoldBanter();
            return;
        }

        _game.PlaytimeSeconds += gameTime.ElapsedGameTime.TotalSeconds;
        if (UpdateRoomChange(gameTime)) return;

        SettleStage(gameTime);
        _scene.Update(gameTime);
        if (UpdateCombat(gameTime)) return;

        MoveTownsfolk(gameTime);
        foreach (Npc npc in _npcs) npc.Update(gameTime);
        foreach (TorchSprite torch in _torches) torch.Update(gameTime);
        _toast.Update(gameTime);
        if (UpdateArrival(gameTime)) return;
        if (ResumeSavedFight()) return;
        if (PlayScenes()) return;

        MoveFollowers(gameTime);
        foreach (WalkerSprite walker in _walkers) walker.Update(gameTime);
        _camera.Follow(Leader.Position, gameTime);

        FindNearest();
        CheckAreas();
        UpdateBanter(gameTime);
        Roam(gameTime);
        Chatter();
    }

    /// <summary>Draws the room behind everyone, everyone on its floor sorted by depth in its tint (or a scene's), the room in front of them, the dark over that, the light over the dark, then what's over people's heads and on the screen itself.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        if (InCombat)
        {
            DrawCombat(gameTime);
            return;
        }

        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        Matrix world = _camera.Transform * _cueShake.Transform;
        float pixel = _camera.Pixel;
        Color tint = WorldTint;

        // 1. The room behind everyone: the sky, the far layer, the tile strip and the mid layer.
        _scene.DrawBack(gameTime, spriteBatch, _camera, tint, _cueShake.Transform);

        // 2. Everything on the floor sorted by how far down the band it stands, all in the room's tint.
        //    Not the flames, which are the light, not what it falls on.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        foreach (PropSprite prop in AllProps()) prop.Draw(spriteBatch, prop.Layer(ZoneCatalog.RoomHeight), tint);
        foreach (TorchSprite torch in _torches) torch.Draw(spriteBatch, Layers.WallFlame);
        foreach (Npc npc in _npcs) npc.Walker.Draw(spriteBatch, OnFloor(npc.Walker.Position), pixel, tint);
        foreach (WalkerSprite walker in _walkers)
            if (!_hiddenWalkers.Contains(walker)) walker.Draw(spriteBatch, OnFloor(walker.Position), pixel, tint);
        spriteBatch.End();

        // 3. The room in front of everyone, then in a dark room the dark over all of it.
        _scene.DrawFront(spriteBatch, _camera, tint, _cueShake.Transform);
        _scene.DrawDark(spriteBatch, _camera, _cueShake.Transform);

        // 4. Light, additive so it brightens what's under it: the flicker over each torch in a dark
        //    room, the glow round anyone a scene's lit, and the pool under whatever the mouse is on,
        //    which shows even in the dark.
        spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp, transformMatrix: world);
        if (_room.Dark)
            foreach (TorchSprite torch in _torches) torch.DrawGlow(spriteBatch, GlowRadius, GlowColor);
        DrawGlows(spriteBatch);
        DrawHighlight(spriteBatch);
        spriteBatch.End();

        // 5. The path you'd walk if you clicked, over everything in the room, so it shows in the dark too.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        DrawPathPreview(spriteBatch);
        spriteBatch.End();

        // 6. The indicators: the badges over the doors, the chevrons at the ends and on the stairs,
        //    the names over heads and the glints, none of them while the arrival or a conversation's
        //    up. Then the soft dark band across the top that the heading sits on; while the arrival
        //    plays it's the black bars and the card instead.
        float alpha = TransitionAlpha;
        bool playing = _arriving != Arriving.None;
        bool overheads = !playing && !_talking;
        IndicatorFrame indicators = Indicators();
        if (overheads) _indicators.Draw(spriteBatch, _camera, indicators, gameTime, alpha);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        if (!playing) spriteBatch.Draw(_band, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, _band.Height), Color.White * alpha);
        spriteBatch.End();

        // 7. What's over people's heads, laid out so none of it sits on anything else, and none of it
        //    while a conversation's up. The markers and the arrows to the ones off the screen go
        //    first, point sampled at the room's zoom, over the dark.
        var bubbles = new List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)>();
        var marks = new List<(Rectangle Box, MarkerKind Kind, float Phase)>();
        if (overheads) LayOutOverheads(bubbles, marks);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        if (overheads)
        {
            DrawMarkers(spriteBatch, marks, gameTime, alpha);
            DrawArrows(spriteBatch, gameTime, alpha);
        }
        spriteBatch.End();

        // 8. The party's cards and the sigil on their corner if a scene's put it up (point sampled),
        //    then all the writing.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        if (!playing) _strip.DrawCards(spriteBatch, _game.Party, _walkers, _game.Leader, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawSigil(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        if (!playing)
        {
            DrawHeading(spriteBatch, (_zone.Name + HeadingJoin + _room.Name).ToUpperInvariant(), Margin, false, alpha);
            DrawHeading(spriteBatch, Hint, LegendQuestGame.ScreenWidth - Margin, true, alpha);
            DrawObjective(spriteBatch, alpha);
            _strip.DrawNames(spriteBatch, _game.Party, _game.Leader, alpha);
            DrawBubbles(spriteBatch, bubbles, alpha);
            if (overheads) DrawPrompt(spriteBatch, alpha);
            DrawTooltip(spriteBatch, alpha);
        }
        DrawArrival(spriteBatch, alpha);
        spriteBatch.End();

        // 9. The toasts, in a batch of their own moved down under the band, so a long one (the
        //    controls after the arrival) never sits on the objective.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp,
            transformMatrix: Matrix.CreateTranslation(0f, ToastDrop, 0f));
        _toast.Draw(spriteBatch, alpha);
        spriteBatch.End();

        // 10. A tutorial tip's card, over everything in the room and under any conversation over it.
        DrawTutorial(spriteBatch, alpha);

        ScreenManager.FadeBackBufferToBlack(MathF.Max(TransitionPosition, _roomFade));
    }

    /// <summary>Puts together the room you're in: its painting, walking in it, its props and things and people, whoever's waiting at camp there, what's open, and the camera on it.</summary>
    /// <remarks>On activating, and again every time you change rooms; the room before it is let go of first. The party's figures carry over: Arrive or ChangeRoom puts them.</remarks>
    private void EnterRoom()
    {
        // What was near you was in the room you've left; the next update finds what's near you here.
        _near = null;
        _nearNpc = null;
        _nearDoor = null;
        _nearMember = null;

        var clock = System.Diagnostics.Stopwatch.StartNew();
        _scene?.Unload();
        _scene = new RoomScene(_room);
        _scene.LoadContent(ScreenManager.Game.Services, ScreenManager.GraphicsDevice);
        LegendQuestGame.Note("room pictures (" + _room.Id + ")", clock.Elapsed.TotalMilliseconds);
        _walk = new RoomWalker(_room);
        _camera.LockTo(_scene.Bounds, (int)ZoneCatalog.RoomScale);

        _game.RoomId = _room.Id;
        _game.Visited.Add(_zone.Id + "/" + _room.Id);
        _tintFrom = _room.Tint;
        _tintTo = _room.Tint;
        _tintLength = 0f;

        _everyProp.Clear();
        _propIds.Clear();
        _torches.Clear();
        foreach (MapProp placed in _room.Props)
        {
            var prop = new PropSprite(placed);
            prop.LoadContent(_content);
            _everyProp.Add(prop);
            if (placed.Id is not null) _propIds[prop] = placed.Id;
            if (placed.Prop.Light is not { } light) continue;

            var torch = new TorchSprite(placed.Foot + light.Flame, 1f);
            torch.LoadContent(_content);
            _torches.Add(torch);
        }

        _everyThing.Clear();
        foreach (MapInteractable thing in _room.Things)
        {
            var sprite = new PropSprite(new MapProp(thing.Look, thing.Foot, thing.Presence));
            sprite.LoadContent(_content);
            _everyThing.Add((thing, sprite));
        }

        // The townsfolk and the creatures come dressed; the story's cast is baked off the paperdoll,
        // each to how they look, unless one of them is drawn from a townsfolk sheet.
        _everyNpc.Clear();
        _brought.Clear();
        _remembered.Clear();
        _campers.Clear();
        clock.Restart();
        foreach (MapNpc placed in _room.Npcs) _everyNpc.Add(Dressed(new Npc(placed, _walk)));
        LegendQuestGame.Note("people dressed (" + _room.Npcs.Count + ")", clock.Elapsed.TotalMilliseconds);
        clock.Restart();
        SeatCampers();

        _props.Clear();
        _npcs.Clear();
        _things.Clear();
        _inside.Clear();
        _previewFor = null;
        RefreshPresence();
        MatchSave();
        BuildNav();
        Board();
        LegendQuestGame.Note("campers, presence, paths", clock.Elapsed.TotalMilliseconds);
    }

    /// <summary>Starts the fade out of this room into another of the zone: you come in on a spawn there, the others behind you, and the game's autosaved once you're in.</summary>
    /// <remarks>Contract for the other partials and the proof run: nothing moves and nothing's read from the input until the new room has faded in.</remarks>
    /// <param name="room">The room, by its id in the zone.</param>
    /// <param name="spawn">The spawn there you arrive on.</param>
    private void ChangeRoom(string room, string spawn)
    {
        Halt();
        _roomChange = (room, spawn);
        _roomFadingIn = false;
    }

    /// <summary>Runs the fade between two rooms: out of this one, the swap, then into the next.</summary>
    /// <returns>True while it's fading, so nothing else moves.</returns>
    private bool UpdateRoomChange(GameTime gameTime)
    {
        if (_roomChange is null && _roomFade <= 0f) return false;

        float step = (float)gameTime.ElapsedGameTime.TotalSeconds / RoomFadeSeconds;
        if (_roomFadingIn)
        {
            _roomFade = MathF.Max(0f, _roomFade - step);
            if (_roomFade <= 0f) _roomFadingIn = false;
            return _roomFade > 0f;
        }

        _roomFade = MathF.Min(1f, _roomFade + step);
        if (_roomFade < 1f || _roomChange is not (string room, string spawn)) return true;

        _roomChange = null;
        _roomFadingIn = true;
        SwapRoom(room, spawn);
        return true;
    }

    /// <summary>Swaps this room for another of the zone at once, under the fade: you on a spawn there, the others behind you, and the autosave written.</summary>
    /// <param name="room">The room, by its id in the zone.</param>
    /// <param name="spawn">The spawn there you arrive on.</param>
    private void SwapRoom(string room, string spawn)
    {
        _room = _zone.Room(room);
        MapSpawn at = _room.Spawns[spawn];
        EnterRoom();

        Leader.Position = at.Feet;
        Leader.Facing = at.Facing;
        foreach (WalkerSprite follower in Followers()) follower.Facing = at.Facing;
        GatherBehind(Followers());
        SeedTrail();
        NoteWhereYouStand();
        _camera.SnapTo(Leader.Position);

        if (!Autosave()) _toast.Show(AutosaveFailedLine);
    }

    /// <summary>One end of the heading, on its smudge.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled, back to front.</param>
    /// <param name="text">What it says.</param>
    /// <param name="x">Where it starts, or where it ends if it's right-aligned.</param>
    /// <param name="rightAligned">Whether x is its right edge.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    private void DrawHeading(SpriteBatch spriteBatch, string text, float x, bool rightAligned, float alpha)
    {
        Vector2 size = _detailFont.MeasureString(text);
        float left = rightAligned ? x - size.X : x;
        var area = new Rectangle((int)left, (int)(HeadingY - HeadingSpread.Y), (int)size.X, (int)(size.Y + HeadingSpread.Y * 2f));
        Text.DrawBacking(spriteBatch, _glow, area, (int)HeadingSpread.X, HeadingBacking * alpha);
        Text.Draw(spriteBatch, _detailFont, text, x, HeadingY, Palette.Bone * alpha, Palette.Shadow * alpha, rightAligned: rightAligned);
    }

    /// <summary>Builds the band across the top: one column of pixels, dark down past the objective and then easing off to nothing, which gets stretched across the screen.</summary>
    private Texture2D BuildBand()
    {
        var texels = new Color[BandSolid + BandFade];
        for (int y = 0; y < texels.Length; y++)
        {
            float t = MathHelper.Clamp((y - BandSolid) / (float)BandFade, 0f, 1f);
            texels[y] = Color.Black * (BandDarkness * (1f - t * t * (3f - 2f * t)));
        }

        var band = new Texture2D(ScreenManager.GraphicsDevice, 1, texels.Length);
        band.SetData(texels);
        return band;
    }

    /// <summary>Gets a party member's walker ready to walk: its shadow, their sheet baked with what they're wearing, their imported picture if they've one, and their footsteps.</summary>
    private void DressWalker(WalkerSprite walker, PartyMember member)
    {
        walker.LoadContent(_content);
        walker.Sheet = SheetFor(member.Look, Paperdoll.GearOf(member));
        walker.Art = ImportedArt.For(member);
        walker.Stepped += () => Footstep(walker);
    }

    /// <summary>Someone in the room with their shadow loaded, their sheet on, and their imported picture if they've one.</summary>
    /// <param name="npc">Who.</param>
    /// <returns>The same npc.</returns>
    private Npc Dressed(Npc npc)
    {
        npc.Walker.LoadContent(_content);
        npc.Walker.Sheet = NpcSheet(npc.Definition);
        npc.Walker.Art = ArtFor(npc.Definition);
        return npc;
    }

    /// <summary>The imported picture someone in the room is drawn as, or null: their creature's, their cast member's, their own for someone baked from a look (a party member waiting at camp), or their townsfolk sheet's.</summary>
    private static ImportedFigure ArtFor(MapNpc npc) =>
        ImportedArt.Get(npc.Creature ?? npc.Cast ?? (npc.Look is not null ? npc.Id : npc.Sheet));

    /// <summary>Someone in the room's sheet: a creature's own, the one baked for their look if they're one of a crowd (or waiting at camp), their townsfolk sheet, or their cast member's, baked if they're a look.</summary>
    private Texture2D NpcSheet(MapNpc npc)
    {
        if (npc.Creature is not null) return CreatureSheet(Creatures.Get(npc.Creature));
        if (npc.Look is not null) return SheetFor(npc.Look);
        if (npc.Cast is null) return _content.Load<Texture2D>(Townsfolk.Texture(npc.Sheet));

        CastMember cast = Cast.Get(npc.Cast);
        return cast.Sheet is not null ? _content.Load<Texture2D>(Townsfolk.Texture(cast.Sheet)) : SheetFor(cast.Look);
    }

    /// <summary>A creature's sheet, checked for room for every frame its walker cuts from it.</summary>
    /// <remarks>Throws if there isn't, which means tools/generate_creatures.py and Creatures don't agree on how big its frames are.</remarks>
    private Texture2D CreatureSheet(Creature creature)
    {
        var sheet = _content.Load<Texture2D>(creature.Texture);
        if (!WalkerSprite.Fits(sheet, creature.Shape))
            throw new InvalidOperationException($"{creature.Texture}.png is {sheet.Width} by {sheet.Height}, which hasn't room for a {creature.Id}'s {creature.Shape.Frame.X} by {creature.Shape.Frame.Y} frames. Run tools/generate_creatures.py.");
        return sheet;
    }

    /// <summary>Works out who and what's in the room with the flags as they are now (and anything a scene's shown or hidden), and who's marked. Builds the paths again if anything came or went.</summary>
    /// <returns>True if anything came or went.</returns>
    private bool RefreshPresence()
    {
        List<PropSprite> props = _everyProp.FindAll(prop => _propIds.TryGetValue(prop, out string id) ? Present(id, prop.Presence) : prop.Presence.Holds(_game.Flags));
        List<Npc> npcs = _everyNpc.FindAll(npc => Present(npc.Definition.Id, npc.Definition.Presence) && !Fallen(npc));
        var things = _everyThing.FindAll(each => Present(each.Thing.Id, each.Thing.Presence));
        bool changed = !props.SequenceEqual(_props) || !npcs.SequenceEqual(_npcs) || !things.SequenceEqual(_things);

        _props.Clear();
        _props.AddRange(props);
        _npcs.Clear();
        _npcs.AddRange(npcs);
        _things.Clear();
        _things.AddRange(things);

        if (changed) BuildNav();
        RefreshMarkers();
        return changed;
    }

    /// <summary>The sheet for a look and the gear they're wearing (none for the townsfolk and the cast), baked the first time it's asked for and kept after that.</summary>
    private Texture2D SheetFor(CharacterLook look, IReadOnlyList<string> gear = null)
    {
        string key = Paperdoll.Key(look, gear);
        if (!_sheets.TryGetValue(key, out Texture2D sheet))
        {
            sheet = _paperdoll.Compose(ScreenManager.GraphicsDevice, look, gear);
            _sheets[key] = sheet;
        }
        return sheet;
    }

    /// <summary>Where someone standing at a point sorts: the room's depth for their feet.</summary>
    private float OnFloor(Vector2 feet) => _scene.Depth(feet);

    /// <summary>Every prop in the room, the ones you can use as well, for drawing.</summary>
    private IEnumerable<PropSprite> AllProps()
    {
        foreach (PropSprite prop in _props) yield return prop;
        foreach ((_, PropSprite sprite) in _things) yield return sprite;
    }

    /// <summary>Which way you're pushing, from WASD, the arrows, the d-pad or the stick.</summary>
    private Vector2 ReadMovement(InputState input)
    {
        if (_proofMove is Vector2 scripted) return scripted;

        Vector2 push = Vector2.Zero;
        if (_walkUp.Occurred(input)) push.Y -= 1f;
        if (_walkDown.Occurred(input)) push.Y += 1f;
        if (_walkLeft.Occurred(input)) push.X -= 1f;
        if (_walkRight.Occurred(input)) push.X += 1f;

        Vector2 stick = input.CurrentGamePadStates[0].ThumbSticks.Left;
        push += new Vector2(stick.X, -stick.Y);

        return push;
    }

    /// <summary>Moves someone along the floor, one axis at a time, so walking into something at an angle slides along it instead of stopping dead, and turns them the way they're going across the room.</summary>
    /// <remarks>The Black Box's LobbyScreen.Walk, for anyone now, with the room's RoomWalker doing the moving.</remarks>
    /// <param name="walker">Who's walking.</param>
    /// <param name="push">Which way, and how hard (up to 1).</param>
    /// <param name="speed">How fast at full push, in art pixels a second.</param>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="avoid">One more box they can't walk into, or null. Followers keep out of your feet with it.</param>
    private void Walk(WalkerSprite walker, Vector2 push, float speed, GameTime gameTime, BoundingRectangle? avoid)
    {
        if (push.LengthSquared() < MinimumPush)
        {
            walker.IsWalking = false;
            return;
        }

        // So diagonals aren't faster.
        if (push.LengthSquared() > 1f) push.Normalize();

        Vector2 step = push * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        walker.Position = _walk.Move(walker.Position, step, avoid);
        walker.IsWalking = true;
        if (MathF.Abs(push.X) >= TurnPush) walker.Facing = push.X < 0f ? Direction.Left : Direction.Right;
    }

    /// <summary>Everything standing in the room that can't be walked through: the props, the people, and anything you use that isn't an open door. The paths go round these.</summary>
    /// <param name="standingOnly">Whether to leave out the people who walk about, who'd be gone from where the paths were worked out by the time you got there.</param>
    private IEnumerable<BoundingRectangle> Footprints(bool standingOnly = false)
    {
        foreach (PropSprite prop in _props)
            foreach (BoundingRectangle footprint in prop.Footprints) yield return footprint;
        foreach ((MapInteractable thing, PropSprite sprite) in _things)
        {
            if (thing.Kind == InteractableKind.Door && _world.IsOpen(thing)) continue;
            foreach (BoundingRectangle footprint in sprite.Footprints) yield return footprint;
        }
        foreach (Npc npc in _npcs)
            if (!standingOnly || !npc.Moves) yield return npc.Footprint;
    }

    /// <summary>Tells the room's walking what's standing in it. When the room's put together, and when anyone comes or goes or a door opens or shuts.</summary>
    /// <remarks>Not the people who walk about: you walk through them as they pass, the way the townsfolk of AQW do, and they stop short of you.</remarks>
    private void BuildNav() => _walk.Rebuild(Footprints(standingOnly: true));

    /// <summary>The feet box at a position.</summary>
    private static BoundingRectangle FeetAt(Vector2 at) => RoomWalker.FeetAt(at);

    /// <summary>Plays a footstep for whatever's underfoot, a little different each time and alternating left and right.</summary>
    /// <remarks>Only yours. Four sets of feet at once was a lot, and it's yours you want to hear change from grass to stone.</remarks>
    /// <param name="walker">Whose foot it was.</param>
    private void Footstep(WalkerSprite walker)
    {
        if (walker != Leader) return;

        _stepLeft = !_stepLeft;
        Sfx sound = _scene.SurfaceAt(walker.Position) == Surface.Grass ? Sfx.FootstepGrass : Sfx.FootstepStone;
        float pitch = ((float)_random.NextDouble() * 2f - 1f) * StepPitch;
        _audio.Play(sound, StepVolume, pitch, _stepLeft ? -StepPan : StepPan);
    }
}
