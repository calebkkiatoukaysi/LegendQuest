using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Collisions;
using LegendQuest.Components;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's part of the world: using the things on the map, walking into triggers and
/// exits, the toasts, the map screen and fast travel.
/// </summary>
/// <remarks>
/// WorldState makes the decisions and hands back what happened. This part only plays the sound for
/// each thing and puts its line up, the same way The Black Box's TableCues did for a round.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far above something's picture its name and prompt go, in screen pixels.</summary>
    private const float PromptGap = 6f;

    /// <summary>What comes up on the next map if the autosave on the way there couldn't be written.</summary>
    private const string AutosaveFailedLine = "The autosave couldn't be written";

    private readonly InputAction _use = new(new[] { Buttons.A }, new[] { Keys.E, Keys.Space, Keys.Enter }, true);
    private readonly InputAction _openMap = new(new[] { Buttons.Y }, new[] { Keys.M }, true);

    private readonly WorldState _world;
    private readonly Discovery _seen;
    private readonly Toast _toast = new();

    /// <summary>Everything on the map you can use, with the picture drawn for it.</summary>
    private readonly List<(MapInteractable Thing, PropSprite Sprite)> _things = new();

    /// <summary>The exits and triggers your feet are in, by id, so each one only goes off as you walk into it.</summary>
    private readonly HashSet<string> _inside = new();

    /// <summary>The nearest thing you're close enough to use, or null.</summary>
    private MapInteractable _near;

    /// <summary>Puts you where the save says, or on the map's start if that's no good, and notes any exit or trigger you're already standing in.</summary>
    /// <remarks>
    /// An old save, or one I've edited by hand, can say somewhere off the map or inside a wall. That
    /// isn't worth an error, so it starts you at the start instead. Arriving in a trigger doesn't set
    /// it off; you have to walk out and back in.
    /// </remarks>
    private void Arrive()
    {
        var saved = new Vector2(_game.X, _game.Y);
        MapSpawn start = _map.Spawns[MapLoader.StartSpawn];
        bool fits = _tilemap.Bounds.Contains(saved) && !Blocked(FeetAt(saved));

        _walker.Position = fits ? saved : start.Feet;
        _walker.Facing = fits ? _game.Facing : start.Facing;

        BoundingRectangle feet = FeetAt(_walker.Position);
        foreach (MapExit exit in _map.Exits)
            if (feet.CollidesWith(exit.Area)) _inside.Add(exit.Id);
        foreach (MapTrigger trigger in _map.Triggers)
            if (feet.CollidesWith(trigger.Area)) _inside.Add(trigger.Id);
    }

    /// <summary>Whether feet there would be standing in something solid.</summary>
    private bool Blocked(BoundingRectangle feet)
    {
        foreach (BoundingRectangle solid in Solids())
            if (feet.CollidesWith(solid)) return true;
        return false;
    }

    /// <summary>Sets off any trigger you've just walked into, then takes you off the map if you've walked into an exit.</summary>
    private void CheckAreas()
    {
        BoundingRectangle feet = FeetAt(_walker.Position);

        foreach (MapTrigger trigger in _map.Triggers)
        {
            if (!Entered(trigger.Id, trigger.Area, feet)) continue;

            if (_world.Enter(trigger) is WorldEvent said) Cue(said);
            Refresh();
        }

        foreach (MapExit exit in _map.Exits)
        {
            if (!Entered(exit.Id, exit.Area, feet)) continue;

            MapDefinition to = MapCatalog.Get(exit.ToMap);
            MapSpawn spawn = to.Spawns[exit.ToSpawn];
            Leave(to, spawn.Feet, spawn.Facing);
            return;
        }
    }

    /// <summary>Whether the feet have just walked into an area: they're in it now, and weren't last frame.</summary>
    /// <param name="id">The exit or trigger, to remember it by.</param>
    /// <param name="area">Its rectangle.</param>
    /// <param name="feet">The feet box.</param>
    private bool Entered(string id, BoundingRectangle area, BoundingRectangle feet)
    {
        if (feet.CollidesWith(area)) return _inside.Add(id);

        _inside.Remove(id);
        return false;
    }

    /// <summary>The nearest thing you're close enough to use that there's still something to do with.</summary>
    /// <remarks>Measured from the feet to its foot, so it doesn't matter how tall it is.</remarks>
    /// <returns>The thing, or null if there's nothing in reach.</returns>
    private MapInteractable Nearest()
    {
        MapInteractable nearest = null;
        float best = float.MaxValue;

        foreach ((MapInteractable thing, _) in _things)
        {
            float distance = Vector2.Distance(_walker.Position, thing.Foot);
            if (distance > thing.Reach || distance >= best || !_world.CanUse(thing)) continue;

            nearest = thing;
            best = distance;
        }

        return nearest;
    }

    /// <summary>Uses something: turns you to it, cues what happened, and opens the map to travel if it was a waypoint.</summary>
    /// <param name="thing">What to use.</param>
    private void Use(MapInteractable thing)
    {
        _walker.IsWalking = false;
        Face(thing.Foot);

        bool travel = false;
        foreach (WorldEvent happened in _world.Use(thing))
        {
            Cue(happened);
            travel |= happened.Kind is WorldEventKind.WaypointFound or WorldEventKind.WaypointUsed;
        }

        Refresh();
        if (travel) OpenMap(thing);
    }

    /// <summary>Plays the sound for something that happened, and puts its line up.</summary>
    /// <remarks>A door has no sound here. Refresh plays it when it sees one move, since a lever across the room can move one too.</remarks>
    /// <param name="happened">What happened.</param>
    private void Cue(WorldEvent happened)
    {
        Sfx? sound = happened.Kind switch
        {
            WorldEventKind.ChestOpened => Sfx.Chest,
            WorldEventKind.GoldTaken => Sfx.Pickup,
            WorldEventKind.DoorLocked or WorldEventKind.DoorStuck => Sfx.DoorLocked,
            WorldEventKind.LeverPulled => Sfx.Lever,
            WorldEventKind.WaypointFound => Sfx.Waypoint,
            _ => null,
        };

        if (sound is Sfx play) _audio.Play(play);
        if (happened.Text is not null) _toast.Show(happened.Text);
    }

    /// <summary>Catches the pictures up with the save after something's been used, and if a door moved, plays it and lets the light and sight through again.</summary>
    private void Refresh()
    {
        if (!MatchSave()) return;

        _audio.Play(Sfx.Door);
        _lightMap?.Rebuild(ScreenManager.GraphicsDevice);
        _seen.LookAgain();
    }

    /// <summary>Puts every picture to match the save (open or shut, up or down), and opens or shuts the cells under any door that moved.</summary>
    /// <returns>True if a door moved.</returns>
    private bool MatchSave()
    {
        bool doorMoved = false;

        foreach ((MapInteractable thing, PropSprite sprite) in _things)
        {
            int frame = _world.IsOpen(thing) ? 1 : 0;
            if (sprite.Frame == frame) continue;

            sprite.Frame = frame;
            if (thing.Kind != InteractableKind.Door) continue;

            _tilemap.Shut(CellsUnder(sprite), frame == 0);
            doorMoved = true;
        }

        return doorMoved;
    }

    /// <summary>The cells a door's picture covers, which it shuts off from light and sight while it's shut.</summary>
    /// <remarks>The whole picture, not just the footprint, so its face takes its light from the floor in front like a wall does.</remarks>
    private static Rectangle CellsUnder(PropSprite door)
    {
        Point size = door.Size;
        int left = (int)MathF.Floor((door.Foot.X - size.X / 2f) / Tilemap.TileSize);
        int right = (int)MathF.Ceiling((door.Foot.X + size.X / 2f) / Tilemap.TileSize);
        int top = (int)MathF.Floor((door.Foot.Y - size.Y) / Tilemap.TileSize);
        int bottom = (int)MathF.Ceiling(door.Foot.Y / Tilemap.TileSize);
        return new Rectangle(left, top, right - left, bottom - top);
    }

    /// <summary>Turns you to face a point, whichever of the four ways is closest.</summary>
    private void Face(Vector2 point)
    {
        Vector2 to = point - _walker.Position;
        _walker.Facing = MathF.Abs(to.X) > MathF.Abs(to.Y)
            ? to.X < 0f ? Direction.Left : Direction.Right
            : to.Y < 0f ? Direction.Up : Direction.Down;
    }

    /// <summary>Opens the map over this screen.</summary>
    /// <param name="waypoint">The waypoint you're travelling from, or null to just look.</param>
    private void OpenMap(MapInteractable waypoint)
    {
        _walker.IsWalking = false;

        var map = new MapScreen(_map, _tilemap, _seen, _world, _walker.Position, travelling: waypoint is not null);
        map.Travel += to => Leave(to, _world.FoundWaypoint(to).Beside, Direction.Down);
        ScreenManager.AddScreen(map);
    }

    /// <summary>Goes to another map: the save moves there, the autosave is written, and the loading screen takes over.</summary>
    /// <remarks>
    /// The autosave goes before anything fades, so it has exactly what the next map will start from.
    /// If it can't be written the game carries on, and the next map says so.
    /// </remarks>
    /// <param name="to">The map.</param>
    /// <param name="feet">Where you arrive on it.</param>
    /// <param name="facing">Which way you face when you get there.</param>
    private void Leave(MapDefinition to, Vector2 feet, Direction facing)
    {
        _walker.IsWalking = false;

        _game.MapId = to.Id;
        _game.X = feet.X;
        _game.Y = feet.Y;
        _game.Facing = facing;

        var next = new ExploreScreen(_game);
        if (!SaveSystem.Write(SaveSystem.AutoSlot, _game)) next._toast.Show(AutosaveFailedLine);

        LoadingScreen.Load(ScreenManager, to.Name.ToUpperInvariant(), next);
    }

    /// <summary>Writes the name of the thing you're near over it, and the key and the word for using it.</summary>
    private void DrawPrompt(SpriteBatch spriteBatch, float alpha)
    {
        if (_near is null) return;

        PropSprite sprite = null;
        foreach ((MapInteractable thing, PropSprite drawn) in _things)
            if (thing == _near) sprite = drawn;

        Vector2 top = Vector2.Transform(_near.Foot - new Vector2(0f, sprite.Size.Y), _camera.Transform);
        float y = top.Y - PromptGap - _detailFont.LineSpacing * 2f;

        Text.Draw(spriteBatch, _detailFont, _near.Name.ToUpperInvariant(), top.X, y,
            Palette.Bone * alpha, Palette.Shadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, "E  ·  " + _near.Prompt, top.X, y + _detailFont.LineSpacing,
            Palette.Torch * alpha, Palette.Shadow * alpha, centred: true);
    }
}
