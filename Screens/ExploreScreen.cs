using System;
using System.Collections.Generic;
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
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// A map being walked around: the tiles, the props, the things you can use, whoever you are, and the
/// dark if it's a dark map.
/// </summary>
/// <remarks>
/// The Black Box's lobby screen, reading its room out of a map file instead of having it in code.
/// Top down, the map is bigger than the window so the camera follows and stops at the edge, and
/// everyone on the floor is sorted by how far down the map their feet are. Using things, triggers
/// and the ways off the map are in ExploreScreen.World. The party, the talking and the fighting get
/// partials of their own later.
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
    private const string Hint = "M  ·  MAP      WHEEL  ·  ZOOM      ESC  ·  PAUSE";

    private readonly InputAction _pause = new(new[] { Buttons.Start, Buttons.Back }, new[] { Keys.Escape }, true);
    private readonly InputAction _walkUp = new(new[] { Buttons.DPadUp }, new[] { Keys.W, Keys.Up }, false);
    private readonly InputAction _walkDown = new(new[] { Buttons.DPadDown }, new[] { Keys.S, Keys.Down }, false);
    private readonly InputAction _walkLeft = new(new[] { Buttons.DPadLeft }, new[] { Keys.A, Keys.Left }, false);
    private readonly InputAction _walkRight = new(new[] { Buttons.DPadRight }, new[] { Keys.D, Keys.Right }, false);

    private readonly SaveData _game;
    private readonly MapDefinition _map;
    private readonly Tilemap _tilemap;
    private readonly LightMap _lightMap;
    private readonly List<PropSprite> _props = new();
    private readonly List<TorchSprite> _torches = new();
    private readonly Camera2D _camera = new(new Point(LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight));
    private readonly WalkerSprite _walker = new();
    private readonly Random _random = new();

    private ContentManager _content;
    private AudioManager _audio;
    private SpriteFont _detailFont;
    private bool _stepLeft;

    /// <summary>Puts the game on the map its save says it's on.</summary>
    /// <param name="game">The game being played.</param>
    public ExploreScreen(SaveData game)
    {
        _game = game;
        _map = MapCatalog.Get(game.MapId);
        _tilemap = new Tilemap(_map);
        _world = new WorldState(game, _map);
        _seen = new Discovery(_map, _tilemap, game);

        TransitionOnTime = FadeTime;
        TransitionOffTime = FadeTime;

        var lights = new List<(Vector2 Pool, float Reach, float Strength)>();
        foreach (MapProp placed in _map.Props)
        {
            _props.Add(new PropSprite(placed));
            if (placed.Prop.Light is not { } light) continue;

            _torches.Add(new TorchSprite(placed.Foot + light.Flame, 1f));
            lights.Add((placed.Foot + light.Pool, light.Reach, light.Strength));
        }
        foreach (MapLight light in _map.Lights) lights.Add((light.Pool, light.Reach, light.Strength));

        foreach (MapInteractable thing in _map.Interactables)
            _things.Add((thing, new PropSprite(new MapProp(thing.Look, thing.Foot))));

        if (_map.Dark) _lightMap = new LightMap(_tilemap, _map.Ambient, lights);
    }

    /// <summary>Loads the map and everything on it, puts you where the save says, and starts the map's song.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _audio = ScreenManager.Game.Services.GetService<AudioManager>();

        _tilemap.LoadContent(_content);
        foreach (PropSprite prop in _props) prop.LoadContent(_content);
        foreach ((_, PropSprite sprite) in _things) sprite.LoadContent(_content);
        foreach (TorchSprite torch in _torches) torch.LoadContent(_content);
        _detailFont = _content.Load<SpriteFont>("spectral-detail");
        _toast.LoadContent(_content);

        // Every door starts shut, then the ones the save says are open open. The dark gets built
        // after that, since a shut door stops the light.
        foreach ((MapInteractable thing, PropSprite sprite) in _things)
            if (thing.Kind == InteractableKind.Door) _tilemap.Shut(CellsUnder(sprite), true);
        MatchSave();
        _lightMap?.LoadContent(ScreenManager.GraphicsDevice);

        // Just the body for now. The paperdoll puts clothes, hair and a face on it later.
        _walker.LoadContent(_content);
        _walker.Sheet = _content.Load<Texture2D>("Paperdoll/body-lean");
        _walker.Stepped += Footstep;
        Arrive();

        _camera.Bounds = _tilemap.Bounds;
        _camera.SnapTo(_walker.Position);

        _audio.PlaySong(_map.Music);
    }

    /// <summary>Unloads the map and disposes the darkness, since the content manager didn't make it.</summary>
    public override void Unload()
    {
        _lightMap?.Unload();
        _content?.Unload();
    }

    /// <summary>Walking, using things, zooming, the map and the pause menu.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame.</param>
    public override void HandleInput(GameTime gameTime, InputState input)
    {
        if (_pause.Occurred(input))
        {
            _walker.IsWalking = false;
            ScreenManager.AddScreen(new PauseMenuScreen());
            return;
        }

        if (_openMap.Occurred(input))
        {
            OpenMap(null);
            return;
        }

        if (_use.Occurred(input) && _near is not null)
        {
            Use(_near);
            return;
        }

        if (input.WheelTurn != 0) _camera.ZoomBy(input.WheelTurn);

        Walk(gameTime, ReadMovement(input));
    }

    /// <summary>Runs everything that moves on its own (you, the torches, the camera and the clock), then checks what you've walked into.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="otherScreenHasFocus">Whether a screen above has the input.</param>
    /// <param name="coveredByOtherScreen">Whether a non-popup screen is on top.</param>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        // Paused, so nothing moves, not even the clock.
        if (!IsActive)
        {
            _walker.IsWalking = false;
            return;
        }

        _game.PlaytimeSeconds += gameTime.ElapsedGameTime.TotalSeconds;

        _walker.Update(gameTime);
        foreach (TorchSprite torch in _torches) torch.Update(gameTime);
        _toast.Update(gameTime);
        _camera.Follow(_walker.Position, gameTime);

        _seen.LookAround(_walker.Position);
        _near = Nearest();
        CheckAreas();
    }

    /// <summary>Draws the map and everyone on it, the dark over them, the torchlight over that, then the writing.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        Matrix world = _camera.Transform;

        // 1. The map: the three layers of tiles, and everything on the floor sorted by how far down the map it is.
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
        _tilemap.Draw(gameTime, spriteBatch, _camera.View, Color.White);
        foreach (PropSprite prop in _props) prop.Draw(spriteBatch, LayerOf(prop));
        foreach ((_, PropSprite sprite) in _things) sprite.Draw(spriteBatch, LayerOf(sprite));
        foreach (TorchSprite torch in _torches) torch.Draw(spriteBatch, Layers.WallFlame);
        _walker.Draw(spriteBatch, Layers.OnFloor(_walker.Position.Y, _tilemap.Bounds.Height), _camera.Pixel);
        spriteBatch.End();

        // 2. On a dark map, the dark with the light cut out of it (linear, so the edges are soft), then the
        //    flicker over each torch, additive so it brightens what's under it.
        if (_lightMap is not null)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, transformMatrix: world);
            _lightMap.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp, transformMatrix: world);
            foreach (TorchSprite torch in _torches) torch.DrawGlow(spriteBatch, GlowRadius, GlowColor);
            spriteBatch.End();
        }

        // 3. The writing: the heading, what you can use, and the toast.
        float alpha = TransitionAlpha;
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp);
        Text.Draw(spriteBatch, _detailFont, _map.Name.ToUpperInvariant(), Margin, HeadingY, Palette.Note * alpha, Palette.Shadow * alpha);
        Text.Draw(spriteBatch, _detailFont, Hint, LegendQuestGame.ScreenWidth - Margin, HeadingY,
            Palette.Note * alpha, Palette.Shadow * alpha, rightAligned: true);
        DrawPrompt(spriteBatch, alpha);
        _toast.Draw(spriteBatch, alpha);
        spriteBatch.End();

        ScreenManager.FadeBackBufferToBlack(TransitionPosition);
    }

    /// <summary>Where a prop sorts: behind everyone if it's on a wall, otherwise by how far down the map its foot is.</summary>
    private float LayerOf(PropSprite prop) =>
        prop.Definition.Placement == PropPlacement.Wall ? Layers.WorldWall : Layers.OnFloor(prop.Foot.Y, _tilemap.Bounds.Height);

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

    /// <summary>Moves you one axis at a time, so walking into a wall at an angle slides along it instead of stopping dead.</summary>
    /// <remarks>Exactly The Black Box's LobbyScreen.Walk.</remarks>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="push">Which way, from the input.</param>
    private void Walk(GameTime gameTime, Vector2 push)
    {
        if (push.LengthSquared() < 0.04f)
        {
            _walker.IsWalking = false;
            return;
        }

        // So diagonals aren't faster.
        if (push.LengthSquared() > 1f) push.Normalize();

        Vector2 step = push * WalkSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2 at = _walker.Position;

        at.X += step.X;
        at.X = ResolveX(at, step.X);
        at.Y += step.Y;
        at.Y = ResolveY(at, step.Y);

        _walker.Position = at;
        _walker.IsWalking = true;

        // Face whichever way you're pushing the most. On a perfect diagonal keep the old facing.
        if (MathF.Abs(push.X) > MathF.Abs(push.Y) + 0.01f) _walker.Facing = push.X < 0 ? Direction.Left : Direction.Right;
        else if (MathF.Abs(push.Y) > MathF.Abs(push.X) + 0.01f) _walker.Facing = push.Y < 0 ? Direction.Up : Direction.Down;
    }

    /// <summary>Pushes the feet back out of anything they walked into sideways, right up against its edge.</summary>
    private float ResolveX(Vector2 at, float moved)
    {
        foreach (BoundingRectangle solid in Solids())
        {
            BoundingRectangle feet = FeetAt(at);
            if (!feet.CollidesWith(solid)) continue;

            at.X = moved > 0f ? solid.Left - FeetSize.X / 2f - CollisionGap : solid.Right + FeetSize.X / 2f + CollisionGap;
        }
        return at.X;
    }

    /// <summary>Same thing for walking up or down.</summary>
    private float ResolveY(Vector2 at, float moved)
    {
        foreach (BoundingRectangle solid in Solids())
        {
            BoundingRectangle feet = FeetAt(at);
            if (!feet.CollidesWith(solid)) continue;

            at.Y = moved > 0f ? solid.Top - CollisionGap : solid.Bottom + FeetSize.Y + CollisionGap;
        }
        return at.Y;
    }

    /// <summary>Everything you can't walk through: the walls from the solid tiles, the props, and anything you use that isn't an open door.</summary>
    private IEnumerable<BoundingRectangle> Solids()
    {
        foreach (BoundingRectangle wall in _tilemap.Walls) yield return wall;
        foreach (PropSprite prop in _props)
            if (prop.Footprint is BoundingRectangle footprint) yield return footprint;
        foreach ((MapInteractable thing, PropSprite sprite) in _things)
        {
            if (thing.Kind == InteractableKind.Door && _world.IsOpen(thing)) continue;
            if (sprite.Footprint is BoundingRectangle footprint) yield return footprint;
        }
    }

    /// <summary>The feet box at a position.</summary>
    private static BoundingRectangle FeetAt(Vector2 at) =>
        new(at.X - FeetSize.X / 2f, at.Y - FeetSize.Y, FeetSize.X, FeetSize.Y);

    /// <summary>Plays a footstep for whatever's underfoot, a little different each time and alternating left and right.</summary>
    private void Footstep()
    {
        _stepLeft = !_stepLeft;
        Sfx sound = _tilemap.SurfaceAt(_walker.Position) == Surface.Grass ? Sfx.FootstepGrass : Sfx.FootstepStone;
        float pitch = ((float)_random.NextDouble() * 2f - 1f) * StepPitch;
        _audio.Play(sound, StepVolume, pitch, _stepLeft ? -StepPan : StepPan);
    }
}
