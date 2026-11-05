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
/// A map being walked around: the tiles, the props, the things you can use, the people, the party,
/// and the dark if it's a dark map.
/// </summary>
/// <remarks>
/// The Black Box's lobby screen, reading its room out of a map file instead of having it in code.
/// Top down, the map is bigger than the window so the camera follows and stops at the edge, and
/// everyone on the floor is sorted by how far down the map their feet are. It's in partials: this
/// one walks and draws, ExploreScreen.Party has the party and the followers, ExploreScreen.Pointer
/// has the mouse, ExploreScreen.World has using things, talking, triggers and the ways off the map,
/// ExploreScreen.Banter has the banter and the lines over people's heads, ExploreScreen.Markers
/// has the "!"s, ExploreScreen.Camp has whoever's waiting at camp, ExploreScreen.Tutorial has
/// the tutorial's tips, and ExploreScreen.Stage is the map as a conversation's stage, where its cues
/// play. A fight on the map is ExploreScreen.Combat and the partials named after it, which take over
/// the input, the running and the drawing while it's on.
/// </remarks>
public partial class ExploreScreen : GameScreen
{
    /// <summary>How fast you walk, in art pixels per second.</summary>
    /// <remarks>About three and a half tiles a second. The Black Box's 230 screen pixels was nearly five, which looks like running at this size.</remarks>
    private const float WalkSpeed = 56f;

    /// <summary>The size of the feet box in art pixels. This is what actually collides.</summary>
    /// <remarks>Narrower than a tile, so a one-tile gap between two walls can be walked through without lining up perfectly.</remarks>
    private static readonly Vector2 FeetSize = new(10f, 4f);

    /// <summary>How far off a wall the feet get left. The tutorial's rectangles count touching as colliding, so it can't be exactly 0.</summary>
    private const float CollisionGap = 0.01f;

    /// <summary>The smallest push that counts as walking, squared. Under it the stick's resting slop doesn't move anyone.</summary>
    private const float MinimumPush = 0.04f;

    /// <summary>How long the map takes to fade in and out.</summary>
    private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.8);

    /// <summary>Footsteps: how loud, how much the pitch varies, and how far they pan left and right.</summary>
    private const float StepVolume = 0.8f;
    private const float StepPitch = 0.1f;
    private const float StepPan = 0.12f;

    /// <summary>The flickering light over each torch on a dark map: how far it reaches (art pixels) and its colour at full strength.</summary>
    private const float GlowRadius = 40f;
    private static readonly Color GlowColor = new Color(255, 150, 70) * 0.16f;

    /// <summary>Where the heading text goes.</summary>
    private const float HeadingY = 14f;
    private const float Margin = 24f;
    private const string Hint = "TAB  ·  SWITCH      M  ·  MAP      WHEEL  ·  ZOOM      ESC  ·  PAUSE";

    /// <summary>The smudge behind the heading, and how far it spreads round the text, so it reads on Questeria's paving as well as in the barrow.</summary>
    private static readonly Color HeadingBacking = Color.Black * 0.8f;
    private static readonly Vector2 HeadingSpread = new(28f, 12f);

    /// <summary>The soft dark band across the top of the screen that the heading, the keys and the objective sit on, so they read over busy roofs: how dark it is, how far down it stays that dark (past the objective), and how far under that it takes to fade to nothing. Screen pixels.</summary>
    private const float BandDarkness = 0.6f;
    private const int BandSolid = 82;
    private const int BandFade = 72;

    /// <summary>How far down from where they'd be the toasts go on a map, in screen pixels, so they're under the band and never on the objective.</summary>
    private const float ToastDrop = 44f;

    private readonly InputAction _pause = new(new[] { Buttons.Start, Buttons.Back }, new[] { Keys.Escape }, true);
    private readonly InputAction _walkUp = new(new[] { Buttons.DPadUp }, new[] { Keys.W, Keys.Up }, false);
    private readonly InputAction _walkDown = new(new[] { Buttons.DPadDown }, new[] { Keys.S, Keys.Down }, false);
    private readonly InputAction _walkLeft = new(new[] { Buttons.DPadLeft }, new[] { Keys.A, Keys.Left }, false);
    private readonly InputAction _walkRight = new(new[] { Buttons.DPadRight }, new[] { Keys.D, Keys.Right }, false);

    private readonly SaveData _game;
    private readonly MapDefinition _map;
    private readonly Tilemap _tilemap;
    private readonly NavGrid _nav;
    private readonly LightMap _lightMap;
    private readonly List<PropSprite> _props = new();
    private readonly List<PropSprite> _everyProp = new();
    private readonly List<TorchSprite> _torches = new();
    private readonly List<Npc> _npcs = new();
    private readonly List<Npc> _everyNpc = new();
    private readonly Camera2D _camera = new(new Point(LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight));
    private readonly Paperdoll _paperdoll = new();
    private readonly Random _random = ProofSeed is int seed ? new Random(seed) : new Random();

    /// <summary>The seed every map's dice start from under the proof run, so its fights go the same way every time, or null to roll freely.</summary>
    internal static int? ProofSeed { get; set; }

    /// <summary>The party's baked sheets, one per look, so two who look alike share one.</summary>
    private readonly Dictionary<string, Texture2D> _sheets = new();

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _detailFont;
    private Texture2D _glow;
    private Texture2D _band;
    private bool _stepLeft;

    /// <summary>Puts the game on the map its save says it's on.</summary>
    /// <param name="game">The game being played.</param>
    public ExploreScreen(SaveData game)
    {
        _game = game;
        _map = MapCatalog.Get(game.MapId);
        _tilemap = new Tilemap(_map);
        _nav = new NavGrid(_tilemap);
        _world = new WorldState(game, _map);
        _seen = new Discovery(_map, _tilemap, game);
        SetUpStage();

        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        var lights = new List<(Vector2 Pool, float Reach, float Strength)>();
        foreach (MapProp placed in _map.Props)
        {
            _everyProp.Add(new PropSprite(placed));
            if (placed.Prop.Light is not { } light) continue;

            _torches.Add(new TorchSprite(placed.Foot + light.Flame, 1f));
            lights.Add((placed.Foot + light.Pool, light.Reach, light.Strength));
        }
        foreach (MapLight light in _map.Lights) lights.Add((light.Pool, light.Reach, light.Strength));

        foreach (MapInteractable thing in _map.Interactables)
            _everyThing.Add((thing, new PropSprite(new MapProp(thing.Look, thing.Foot, thing.Presence))));
        foreach (MapNpc npc in _map.Npcs) _everyNpc.Add(new Npc(npc));
        foreach (PartyMember member in game.Party)
        {
            _walkers.Add(new WalkerSprite());
            _members.Add(member);
        }
        SeatCampers();
        RefreshPresence();

        if (_map.Dark) _lightMap = new LightMap(_tilemap, _map.Ambient, lights);
    }

    /// <summary>Loads the map and everyone on it, puts the party where the save says, and starts the map's song.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();
        _cursor = ScreenManager.Game.Services.GetService<CursorSprite>();

        _tilemap.LoadContent(_content);
        foreach (PropSprite prop in _everyProp) prop.LoadContent(_content);
        foreach ((_, PropSprite sprite) in _everyThing) sprite.LoadContent(_content);
        foreach (TorchSprite torch in _torches) torch.LoadContent(_content);
        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        _glow = _content.Load<Texture2D>("glow");
        _marks = _content.Load<Texture2D>("map-marks");
        _markers = _content.Load<Texture2D>("markers");
        _band ??= BuildBand();
        _toast.LoadContent(_content);
        _tooltip.LoadContent(_content);
        _strip.LoadContent(_content, ScreenManager.GraphicsDevice);
        LoadTutorial();

        // The townsfolk and the creatures come dressed. The party, whoever's waiting at camp and the
        // story's cast are baked off the paperdoll, each to how they look, unless one of the cast is
        // drawn from a townsfolk sheet.
        _paperdoll.LoadContent(_content);
        foreach (Npc npc in _everyNpc)
        {
            npc.Walker.LoadContent(_content);
            npc.Walker.Sheet = NpcSheet(npc.Definition);
        }
        for (int i = 0; i < _walkers.Count; i++) DressWalker(_walkers[i], _game.Party[i]);

        // Every door starts shut, then the ones the save says are open open. The dark gets built
        // after that, since a shut door stops the light.
        foreach ((MapInteractable thing, PropSprite sprite) in _everyThing)
            if (thing.Kind == InteractableKind.Door) _tilemap.Shut(CellsUnder(sprite), true);
        MatchSave();
        BuildNav();
        _lightMap?.LoadContent(ScreenManager.GraphicsDevice);

        Arrive();
        _camera.Bounds = _tilemap.Bounds;
        _camera.SnapTo(Leader.Position);
        _chapterCard.LoadContent(_content);
        BeginArrival();

        _audio.PlaySong(_map.Music);
    }

    /// <summary>Unloads the map, and disposes everything the content manager didn't make: the dark, the band across the top, the baked sheets and the strip's pixel.</summary>
    public override void Unload()
    {
        _lightMap?.Unload();
        _band?.Dispose();
        foreach (Texture2D sheet in _sheets.Values) sheet.Dispose();
        _strip.Unload();
        _content?.Unload();
    }

    /// <summary>The pause menu, a tip's card, the map, switching who you control, using things, zooming, the mouse, and walking.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
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

        if (input.WheelTurn != 0) _camera.ZoomBy(input.WheelTurn);

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

    /// <summary>Runs everything that moves on its own (the party, the people, the torches, the camera, the clock, and a scene's shake or tint still settling), checks what you've walked into, then runs the banter and lets the townsfolk talk.</summary>
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
        SettleStage(gameTime);
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

        _seen.LookAround(Leader.Position);
        FindNearest();
        CheckAreas();
        UpdateBanter(gameTime);
        Roam(gameTime);
        Chatter();
    }

    /// <summary>Draws the map and everyone on it in the map's tint (or a scene's), the dark over them, the light over that, the markers over everything, then what's on the screen itself.</summary>
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

        // 1. The map: the three layers of tiles, and everything on the floor sorted by how far down the
        //    map it is, all in the map's tint. Not the flames, which are the light, not what it falls on.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        _tilemap.Draw(gameTime, spriteBatch, _camera.View, tint);
        foreach (PropSprite prop in AllProps()) prop.Draw(spriteBatch, prop.Layer(_tilemap.Bounds.Height), tint);
        foreach (TorchSprite torch in _torches) torch.Draw(spriteBatch, Layers.WallFlame);
        foreach (Npc npc in _npcs) npc.Walker.Draw(spriteBatch, OnFloor(npc.Walker.Position), pixel, tint);
        foreach (WalkerSprite walker in _walkers)
            if (!_hiddenWalkers.Contains(walker)) walker.Draw(spriteBatch, OnFloor(walker.Position), pixel, tint);
        spriteBatch.End();

        // 2. On a dark map, the dark with the light cut out of it (linear, so the edges are soft).
        if (_lightMap is not null)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, transformMatrix: world);
            _lightMap.Draw(spriteBatch);
            spriteBatch.End();
        }

        // 3. Light, additive so it brightens what's under it: the flicker over each torch, the glow
        //    round anyone a scene's lit, and the pool under whatever the mouse is on, which shows
        //    even in the dark.
        spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp, transformMatrix: world);
        if (_lightMap is not null)
            foreach (TorchSprite torch in _torches) torch.DrawGlow(spriteBatch, GlowRadius, GlowColor);
        DrawGlows(spriteBatch);
        DrawHighlight(spriteBatch);
        spriteBatch.End();

        // 4. The path you'd walk if you clicked, over everything on the map, so it shows in the dark too.
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        DrawPathPreview(spriteBatch);
        spriteBatch.End();

        // 5. The screen. First the soft dark band across the top that the heading sits on. While the
        //    arrival plays it's the black bars and the card instead.
        float alpha = TransitionAlpha;
        bool playing = _arriving != Arriving.None;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        if (!playing) spriteBatch.Draw(_band, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, _band.Height), Color.White * alpha);
        spriteBatch.End();

        // 6. What's over people's heads, laid out so none of it sits on anything else, and none of it
        //    while a conversation's up. The markers and the arrows to the ones off the screen go
        //    first, point sampled at the map's zoom, over the roofs and the dark.
        bool overheads = !playing && !_talking;
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

        // 7. The party's cards and the sigil on their corner if a scene's put it up (point sampled),
        //    then all the writing.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
        if (!playing) _strip.DrawCards(spriteBatch, _game.Party, PartySheets(), _game.Leader, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        DrawSigil(spriteBatch, alpha);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        if (!playing)
        {
            DrawHeading(spriteBatch, _map.Name.ToUpperInvariant(), Margin, false, alpha);
            DrawHeading(spriteBatch, Hint, LegendQuestGame.ScreenWidth - Margin, true, alpha);
            DrawObjective(spriteBatch, alpha);
            _strip.DrawNames(spriteBatch, _game.Party, _game.Leader, alpha);
            DrawBubbles(spriteBatch, bubbles, alpha);
            if (overheads) DrawPrompt(spriteBatch, alpha);
            DrawTooltip(spriteBatch, alpha);
        }
        DrawArrival(spriteBatch, alpha);
        spriteBatch.End();

        // 8. The toasts, in a batch of their own moved down under the band, so a long one (the
        //    controls after the arrival) never sits on the objective.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp,
            transformMatrix: Matrix.CreateTranslation(0f, ToastDrop, 0f));
        _toast.Draw(spriteBatch, alpha);
        spriteBatch.End();

        // 9. A tutorial tip's card, over everything on the map and under any conversation over it.
        DrawTutorial(spriteBatch, alpha);

        ScreenManager.FadeBackBufferToBlack(TransitionPosition);
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

    /// <summary>Gets a party member's walker ready to walk: its shadow, their sheet baked with what they're wearing, and their footsteps.</summary>
    private void DressWalker(WalkerSprite walker, PartyMember member)
    {
        walker.LoadContent(_content);
        walker.Sheet = SheetFor(member.Look, Paperdoll.GearOf(member));
        walker.Stepped += () => Footstep(walker);
    }

    /// <summary>Someone on the map's sheet: a creature's own, the one baked for their look if they're one of a crowd (or waiting at camp), their townsfolk sheet, or their cast member's, baked if they're a look.</summary>
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

    /// <summary>Works out who and what's on the map with the flags as they are now (and anything a scene's shown or hidden), and who's marked. Builds the paths again if anything came or went.</summary>
    /// <returns>True if anything came or went.</returns>
    private bool RefreshPresence()
    {
        List<PropSprite> props = _everyProp.FindAll(prop => prop.Presence.Holds(_game.Flags));
        List<Npc> npcs = _everyNpc.FindAll(npc => Present(npc.Definition.Id, npc.Definition.Presence) && !_fallen.Contains(npc));
        var things = _everyThing.FindAll(each => Present(each.Thing.Id, each.Thing.Presence));
        bool changed = !props.SequenceEqual(_props) || !npcs.SequenceEqual(_npcs) || !things.SequenceEqual(_things);

        _props.Clear();
        _props.AddRange(props);
        _npcs.Clear();
        _npcs.AddRange(npcs);
        _things.Clear();
        _things.AddRange(things);

        if (changed && _content is not null) BuildNav();
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

    /// <summary>Where someone standing at a point sorts.</summary>
    private float OnFloor(Vector2 feet) => Layers.OnFloor(feet.Y, _tilemap.Bounds.Height);

    /// <summary>Every prop on the map, the ones you can use as well, for drawing.</summary>
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

    /// <summary>Moves someone one axis at a time, so walking into a wall at an angle slides along it instead of stopping dead.</summary>
    /// <remarks>Exactly The Black Box's LobbyScreen.Walk, for anyone now, not just you.</remarks>
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
        Vector2 at = walker.Position;

        at.X += step.X;
        at.X = ResolveX(at, step.X, avoid);
        at.Y += step.Y;
        at.Y = ResolveY(at, step.Y, avoid);

        walker.Position = at;
        walker.IsWalking = true;

        // Face whichever way you're pushing the most. On a perfect diagonal keep the old facing.
        if (MathF.Abs(push.X) > MathF.Abs(push.Y) + 0.01f) walker.Facing = push.X < 0 ? Direction.Left : Direction.Right;
        else if (MathF.Abs(push.Y) > MathF.Abs(push.X) + 0.01f) walker.Facing = push.Y < 0 ? Direction.Up : Direction.Down;
    }

    /// <summary>Pushes the feet back out of anything they walked into sideways, right up against its edge.</summary>
    private float ResolveX(Vector2 at, float moved, BoundingRectangle? avoid)
    {
        foreach (BoundingRectangle solid in Solids(avoid))
        {
            BoundingRectangle feet = FeetAt(at);
            if (!feet.CollidesWith(solid)) continue;

            at.X = moved > 0f ? solid.Left - FeetSize.X / 2f - CollisionGap : solid.Right + FeetSize.X / 2f + CollisionGap;
        }
        return at.X;
    }

    /// <summary>Same thing for walking up or down.</summary>
    private float ResolveY(Vector2 at, float moved, BoundingRectangle? avoid)
    {
        foreach (BoundingRectangle solid in Solids(avoid))
        {
            BoundingRectangle feet = FeetAt(at);
            if (!feet.CollidesWith(solid)) continue;

            at.Y = moved > 0f ? solid.Top - CollisionGap : solid.Bottom + FeetSize.Y + CollisionGap;
        }
        return at.Y;
    }

    /// <summary>Everything that can't be walked through: the walls, the props, the people, anything you use that isn't an open door, and one more box if there is one.</summary>
    private IEnumerable<BoundingRectangle> Solids(BoundingRectangle? avoid = null)
    {
        foreach (BoundingRectangle wall in _tilemap.Walls) yield return wall;
        foreach (BoundingRectangle footprint in Footprints()) yield return footprint;
        if (avoid is BoundingRectangle extra) yield return extra;
    }

    /// <summary>Everything standing on the map that can't be walked through: the props, the people, and anything you use that isn't an open door. The NavGrid is built from these.</summary>
    /// <param name="standingOnly">Whether to leave out the people who wander.</param>
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
            if (!standingOnly || npc.Definition.Wander == 0f) yield return npc.Footprint;
    }

    /// <summary>Works out again where the pathfinder can go. When the map loads, and when a door opens or shuts.</summary>
    /// <remarks>Not round the townsfolk who wander: they'd be gone from where it was worked out by the time you got there.</remarks>
    private void BuildNav() => _nav.Build(Footprints(standingOnly: true));

    /// <summary>The feet box at a position.</summary>
    private static BoundingRectangle FeetAt(Vector2 at) =>
        new(at.X - FeetSize.X / 2f, at.Y - FeetSize.Y, FeetSize.X, FeetSize.Y);

    /// <summary>Plays a footstep for whatever's underfoot, a little different each time and alternating left and right.</summary>
    /// <remarks>Only yours. Four sets of feet at once was a lot, and it's yours you want to hear change from grass to stone.</remarks>
    /// <param name="walker">Whose foot it was.</param>
    private void Footstep(WalkerSprite walker)
    {
        if (walker != Leader) return;

        _stepLeft = !_stepLeft;
        Sfx sound = _tilemap.SurfaceAt(walker.Position) == Surface.Grass ? Sfx.FootstepGrass : Sfx.FootstepStone;
        float pitch = ((float)_random.NextDouble() * 2f - 1f) * StepPitch;
        _audio.Play(sound, StepVolume, pitch, _stepLeft ? -StepPan : StepPan);
    }
}
