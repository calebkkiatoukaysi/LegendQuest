using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.Tutorial;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The zone screen's part of the world: using the things in a room, talking to people, walking into
/// triggers and off the ends of rooms, going through doors and up and down stairs, the toasts, the
/// zone map and fast travel.
/// </summary>
/// <remarks>
/// WorldState makes the decisions about things and hands back what happened. This part only plays
/// the sound for each and puts its line up, the same way The Black Box's TableCues did for a round.
/// A way out is an edge exit (the last few pixels at either end of the floor), a stair (a strip at the
/// back or the front of the floor where it climbs or drops) or a door (a box in the painting standing
/// on the back of the floor, used with E or a click). Each goes to another room of the zone, which
/// swaps in with a fade, to a spawn in another zone, which goes through the loading screen, or back
/// out of the building to wherever you went in; while it's locked, trying it says why.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far above something's picture its name and prompt go, in screen pixels.</summary>
    private const float PromptGap = 6f;

    /// <summary>How close you have to be to talk to someone, in art pixels, feet to feet. Someone broader than a person can be talked to from further off (see ReachOf).</summary>
    private const float TalkReach = 28f;

    /// <summary>How far above a person's feet the top of their head is, in art pixels, for writing over it. A creature's is as much higher as its frame stands taller (see HeadOf).</summary>
    private const float HeadHeight = 44f;

    /// <summary>Where a building's way out goes when the save doesn't know where you went in.</summary>
    private const string QuesteriaZone = "questeria";

    /// <summary>What comes up in the next room or zone if the autosave on the way there couldn't be written.</summary>
    private const string AutosaveFailedLine = "The autosave couldn't be written";

    /// <summary>How far in front of a door, or in from the end of a room, you're put when you come back out of the building it led into, in art pixels.</summary>
    private const float BackStep = 10f;

    /// <summary>How far from the back of the floor you can be and still use a door, in art pixels: a quarter of the band.</summary>
    private const float DoorDepth = 16f;

    /// <summary>A stair's strip at the back or front of the floor: how far either side of its foot it reaches, and how deep, in art pixels.</summary>
    private const float StairHalfWidth = 14f;
    private const float StairDepth = 10f;

    /// <summary>What the prompt says for a door that opens, and one that won't.</summary>
    private const string EnterPrompt = "E  ·  ENTER";
    private const string LockedPrompt = "E  ·  LOCKED";

    private readonly InputAction _use = new(new[] { Buttons.A }, new[] { Keys.E, Keys.Space, Keys.Enter }, true);
    private readonly InputAction _openMap = new(new[] { Buttons.Y }, new[] { Keys.M }, true);

    private readonly WorldState _world;
    private readonly Toast _toast = new();

    /// <summary>Everything in the room you can use, with the picture drawn for it.</summary>
    private readonly List<(MapInteractable Thing, PropSprite Sprite)> _things = new();
    private readonly List<(MapInteractable Thing, PropSprite Sprite)> _everyThing = new();

    /// <summary>The ways out and triggers your feet are in, by id, so each one only goes off as you walk into it.</summary>
    private readonly HashSet<string> _inside = new();

    /// <summary>The nearest thing you're close enough to use, the nearest person you're close enough to talk to, the door you're at, or the place in the party of the nearest of them with something to say to you. At most one of them is set.</summary>
    private MapInteractable _near;
    private Npc _nearNpc;
    private RoomDoor _nearDoor;
    private int? _nearMember;

    /// <summary>Sets off any trigger you've just walked into, then takes you through any way out you've just walked into.</summary>
    /// <remarks>
    /// Only your feet count. The others follow you through. A trigger that does nothing but start a
    /// banter doesn't go off at all while its banter can't play, so it's still there for when it can.
    /// </remarks>
    private void CheckAreas()
    {
        BoundingRectangle feet = FeetAt(Leader.Position);

        foreach (MapTrigger trigger in _room.Triggers)
        {
            if (!trigger.Presence.Holds(_game.Flags) || !Entered(trigger.Id, trigger.Area, feet)) continue;

            bool barks = trigger.Bark is not null && !Used(trigger) && CanBark(trigger);
            if (trigger.OnlyBarks && !barks) continue;

            if (_world.Enter(trigger) is WorldEvent said) Cue(said);
            if (barks) Bark(trigger);
            Refresh();
        }

        foreach (RoomExit exit in _room.Exits)
        {
            if (!exit.Presence.Holds(_game.Flags) || !Entered(exit.Id, ExitArea(exit), feet)) continue;

            TakeExit(exit);
            return;
        }
    }

    /// <summary>Where on the floor walking into a way out takes it: the feet within EdgeReach of the floor's end (so the feet box is in a strip half its width narrower), or a stair's strip at the back of the floor (up) or the front (down).</summary>
    /// <param name="exit">The way out.</param>
    private BoundingRectangle ExitArea(RoomExit exit)
    {
        float edge = RoomExit.EdgeReach - RoomWalker.FeetSize.X / 2f;
        switch (exit.Side)
        {
            case ExitSide.West:
                return new BoundingRectangle(0f, 0f, edge, ZoneCatalog.RoomHeight);
            case ExitSide.East:
                return new BoundingRectangle(_room.Width - edge, 0f, edge, ZoneCatalog.RoomHeight);
            case ExitSide.Up:
                return new BoundingRectangle(exit.X - StairHalfWidth, _room.BackEdgeAt(exit.X) - StairDepth, StairHalfWidth * 2f, StairDepth * 2f);
            default:
                return new BoundingRectangle(exit.X - StairHalfWidth, _room.FrontEdgeAt(exit.X) - StairDepth, StairHalfWidth * 2f, StairDepth * 2f);
        }
    }

    /// <summary>Where you'd walk to take a way out: the middle of its strip, on the floor.</summary>
    /// <param name="exit">The way out.</param>
    private Vector2 ExitSpot(RoomExit exit)
    {
        BoundingRectangle area = ExitArea(exit);
        float x = area.X + area.Width / 2f;
        float y = exit.Side switch
        {
            ExitSide.Up => _room.BackEdgeAt(x) + StairDepth / 2f,
            ExitSide.Down => _room.FrontEdgeAt(x) - StairDepth / 2f,
            _ => Leader.Position.Y,
        };
        return _walk.NearestOnFloor(new Vector2(x, y));
    }

    /// <summary>Goes out of the room by a way out: says why if it's locked, otherwise back out of the building, into another room of the zone, or to another zone, leaving somewhere to come back to.</summary>
    /// <param name="exit">The way out.</param>
    private void TakeExit(RoomExit exit)
    {
        if (exit.LockedWhy is not null)
        {
            SayLocked(exit.LockedWhy);
            return;
        }

        if (exit.ToZone == RoomExit.Back)
        {
            GoBack();
            return;
        }

        if (exit.ToZone == _zone.Id)
        {
            ChangeRoom(exit.ToRoom, exit.ToSpawn);
            return;
        }

        float inward = exit.Side == ExitSide.East ? -BackStep : exit.Side == ExitSide.West ? BackStep : 0f;
        RememberWayBack(Leader.Position + new Vector2(inward, 0f));
        GoTo(exit.ToZone, exit.ToSpawn);
    }

    /// <summary>Goes through a door: says why if it's locked, otherwise back out, into another room of the zone, or into another zone, leaving somewhere in front of it to come back out to.</summary>
    /// <param name="door">The door.</param>
    private void UseDoor(RoomDoor door)
    {
        Halt();
        Leader.Face(door.Foot);
        if (door.LockedWhy is not null)
        {
            SayLocked(door.LockedWhy);
            return;
        }

        TutorialDone(TutorialCatalog.Enter);
        if (door.ToZone == RoomExit.Back)
        {
            GoBack();
            return;
        }

        if (door.ToZone == _zone.Id)
        {
            ChangeRoom(door.ToRoom, door.ToSpawn);
            return;
        }

        RememberWayBack(_walk.NearestOnFloor(door.Foot + new Vector2(0f, BackStep)));
        GoTo(door.ToZone, door.ToSpawn);
    }

    /// <summary>Says why a door or a way out won't let you through, with the locked door's sound.</summary>
    /// <param name="why">Why.</param>
    private void SayLocked(string why)
    {
        _audio.Play(Sfx.DoorLocked);
        _toast.Show(why);
    }

    /// <summary>Keeps where to come back out to when a building's way out goes back: this room, at a point.</summary>
    /// <param name="at">Where, in world pixels.</param>
    private void RememberWayBack(Vector2 at)
    {
        _game.BackZone = _zone.Id;
        _game.BackRoom = _room.Id;
        _game.BackX = at.X;
        _game.BackY = at.Y;
    }

    /// <summary>Out of a building, back where you came in, facing the way you are. If the save doesn't know where that was, it's Questeria's start.</summary>
    private void GoBack()
    {
        if (_game.BackZone is string zone && ZoneCatalog.Contains(zone) && ZoneCatalog.Get(zone).Room(_game.BackRoom ?? string.Empty) is RoomDefinition room)
        {
            Travel(zone, room.Id, new Vector2(_game.BackX, _game.BackY), Leader.Facing);
            return;
        }

        GoTo(QuesteriaZone, ZoneLoader.StartSpawn);
    }

    /// <summary>Whether the feet have just walked into an area: they're in it now, and weren't last frame.</summary>
    /// <param name="id">The way out or trigger, to remember it by.</param>
    /// <param name="area">Its rectangle.</param>
    /// <param name="feet">The feet box.</param>
    private bool Entered(string id, BoundingRectangle area, BoundingRectangle feet)
    {
        if (feet.CollidesWith(area)) return _inside.Add(id);

        _inside.Remove(id);
        return false;
    }

    /// <summary>Notes every way out and trigger your feet are already in, so arriving in one doesn't set it off; you have to walk out and back in.</summary>
    private void NoteWhereYouStand()
    {
        _inside.Clear();
        BoundingRectangle feet = FeetAt(Leader.Position);
        foreach (RoomExit exit in _room.Exits)
            if (feet.CollidesWith(ExitArea(exit))) _inside.Add(exit.Id);
        foreach (MapTrigger trigger in _room.Triggers)
            if (feet.CollidesWith(trigger.Area)) _inside.Add(trigger.Id);
    }

    /// <summary>Whether a trigger that only goes off once already has. The save keeps it as zone/id, the same as WorldState does.</summary>
    private bool Used(MapTrigger trigger) => trigger.Once && _game.Triggered.Contains(_zone.Id + "/" + trigger.Id);

    /// <summary>Finds the nearest thing you're close enough to use that there's still something to do with, the nearest person you're close enough to talk to, or the door you're at, whichever's nearest; or if there's none of them, the nearest of the party with something to say to you.</summary>
    /// <remarks>
    /// Measured from your feet to its foot, so it doesn't matter how tall it is. The party's always at
    /// your elbow, so they only get E when there's nothing else to use it on, or they'd keep taking it
    /// off the chest you're standing at.
    /// </remarks>
    private void FindNearest()
    {
        _near = null;
        _nearNpc = null;
        _nearDoor = null;
        _nearMember = null;
        float best = float.MaxValue;

        foreach ((MapInteractable thing, _) in _things)
        {
            float distance = Vector2.Distance(Leader.Position, thing.Foot);
            if (distance > thing.Reach || distance >= best || !_world.CanUse(thing)) continue;

            _near = thing;
            best = distance;
        }

        foreach (Npc npc in _npcs)
        {
            float distance = Vector2.Distance(Leader.Position, TalkSpot(npc));
            if (distance > ReachOf(npc) || distance >= best) continue;

            _near = null;
            _nearNpc = npc;
            best = distance;
        }

        foreach (RoomDoor door in Doors())
        {
            float distance = Vector2.Distance(Leader.Position, door.Foot);
            if (!AtDoor(door, Leader.Position) || distance >= best) continue;

            _near = null;
            _nearNpc = null;
            _nearDoor = door;
            best = distance;
        }

        if (_near is not null || _nearNpc is not null || _nearDoor is not null) return;

        for (int i = 0; i < _walkers.Count; i++)
        {
            float distance = Vector2.Distance(Leader.Position, _walkers[i].Position);
            if (i == _game.Leader || distance > TalkReach || distance >= best || ScriptFor(_game.Party[i]) is null) continue;

            _nearMember = i;
            best = distance;
        }
    }

    /// <summary>The room's doors that are there now: a door and its locked twin are never both there.</summary>
    private IEnumerable<RoomDoor> Doors()
    {
        foreach (RoomDoor door in _room.Doors)
            if (door.Presence.Holds(_game.Flags)) yield return door;
    }

    /// <summary>Whether feet are at a door: across under its box, and near the back of the floor where it stands.</summary>
    private static bool AtDoor(RoomDoor door, Vector2 feet) =>
        feet.X >= door.Area.Left && feet.X <= door.Area.Right && feet.Y - door.Foot.Y <= DoorDepth;

    /// <summary>Where you'd stand to use a door: at its foot, a little way onto the floor.</summary>
    private Vector2 DoorSpot(RoomDoor door) => _walk.NearestOnFloor(door.Foot + new Vector2(0f, DoorDepth / 2f));

    /// <summary>Talks to or uses whatever's nearest, the way E does.</summary>
    /// <returns>True if there was anyone or anything to.</returns>
    private bool UseNearest()
    {
        if (_nearNpc is Npc npc)
        {
            Halt();
            Talk(npc);
            return true;
        }

        if (_nearDoor is RoomDoor door)
        {
            UseDoor(door);
            return true;
        }

        if (_nearMember is int member)
        {
            Halt();
            TalkToMember(member);
            return true;
        }

        if (_near is null) return false;

        Halt();
        Use(_near);
        return true;
    }

    /// <summary>Talks to someone: you both turn to face each other, and it's a conversation if they've one for now, or their line if not. Someone waiting at camp with no conversation of their own offers to take a place in the party.</summary>
    /// <param name="npc">Who.</param>
    private void Talk(Npc npc)
    {
        TutorialDone(TutorialCatalog.Talk);
        Leader.Face(npc.Walker.Position);

        DialogueScript script = ScriptFor(npc) ?? (_campers.Contains(npc) ? CampOffer(npc.Definition.Id) : null);
        npc.TalkTo(Leader.Position, script is null);
        if (script is not null) Converse(script, null);
    }

    /// <summary>Talks to one of the party you're not controlling, if they've something to say: you turn to each other, they stop where they are, and their conversation starts.</summary>
    /// <param name="index">Their place in the party.</param>
    private void TalkToMember(int index)
    {
        if (ScriptFor(_game.Party[index]) is not DialogueScript script) return;

        TutorialDone(TutorialCatalog.Companion);
        WalkerSprite walker = _walkers[index];
        Leader.Face(walker.Position);
        walker.Face(Leader.Position);
        walker.IsWalking = false;
        Converse(script, null);
    }

    /// <summary>The conversation talking to someone in the room starts now: the one the zone gives them with talk, if it can start, or else their first in DialogueCatalog that can. Null if there's neither.</summary>
    /// <param name="npc">Who.</param>
    private DialogueScript ScriptFor(Npc npc)
    {
        if (DialogueCatalog.Get(npc.Definition.Talk) is DialogueScript talk && talk.CanStart(_game.Flags)) return talk;
        return DialogueCatalog.For(npc.Definition.Id, _game.Flags);
    }

    /// <summary>The conversation talking to one of the party starts now: their first in DialogueCatalog that's with them (by their origin id) and can start, or null if they've nothing to say.</summary>
    /// <param name="member">Who.</param>
    private DialogueScript ScriptFor(PartyMember member) => DialogueCatalog.For(member.Id, _game.Flags);

    /// <summary>How close you have to be to talk to someone: TalkReach for a person, and as much further as they're broader than one each side, so a beast the size of a cart can be talked to from beside it.</summary>
    private static float ReachOf(Npc npc) =>
        TalkReach + (npc.Walker.Shape.Footprint.X - WalkerShape.Person.Footprint.X) / 2f;

    /// <summary>Where you talk to someone from: their feet, or for someone aboard a ship, the back of the floor below where they stand on her deck.</summary>
    private Vector2 TalkSpot(Npc npc) =>
        npc.Definition.Aboard is null ? npc.Walker.Position : new Vector2(npc.Walker.Position.X, _room.BackEdgeAt(npc.Walker.Position.X));

    /// <summary>Where the top of someone's head is, in world pixels: HeadHeight up from a person's feet, and as much higher again as a creature's frame stands taller than a person's. An imported picture's is as far over its own height as HeadHeight is over a person's figure.</summary>
    private static Vector2 HeadOf(WalkerSprite walker) =>
        walker.Position - new Vector2(0f, !walker.DrawsArt
            ? HeadHeight + walker.Shape.Feet.Y - WalkerShape.Person.Feet.Y
            : walker.Size.Y + HeadHeight - WalkerShape.Person.Figure.Y);

    /// <summary>Uses something: turns you to it, cues what happened, and opens the zone map to travel if it was a waypoint.</summary>
    /// <param name="thing">What to use.</param>
    private void Use(MapInteractable thing)
    {
        TutorialUsed(thing);
        Leader.IsWalking = false;
        Leader.Face(thing.Foot);

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
            WorldEventKind.ItemsTaken => Sfx.Pickup,
            _ => null,
        };

        if (happened.Kind == WorldEventKind.TriggerTalked)
        {
            Converse(DialogueCatalog.Get(happened.Text), null);
            return;
        }

        if (sound is Sfx play) _audio.Play(play);
        Announce(happened);
    }

    /// <summary>Catches the pictures and the markers up with the save after something's been used or walked into, and if a door moved, plays it and lets the paths through again.</summary>
    private void Refresh()
    {
        RefreshMarkers();
        if (!MatchSave()) return;

        _audio.Play(Sfx.Door);
        BuildNav();
        _previewFor = null;
    }

    /// <summary>Puts every picture to match the save: open or shut, up or down.</summary>
    /// <returns>True if a door moved.</returns>
    private bool MatchSave()
    {
        bool doorMoved = false;

        foreach ((MapInteractable thing, PropSprite sprite) in _things)
        {
            int frame = _world.IsOpen(thing) ? 1 : 0;
            if (sprite.Frame == frame) continue;

            sprite.Frame = frame;
            doorMoved |= thing.Kind == InteractableKind.Door;
        }

        return doorMoved;
    }

    /// <summary>Opens the zone map over this screen.</summary>
    /// <param name="waypoint">The waypoint you're travelling from, or null to just look.</param>
    private void OpenMap(MapInteractable waypoint)
    {
        Halt();
        TutorialDone(TutorialCatalog.Map);

        var map = new ZoneMapScreen(_game, _zone.Id, _room.Id, travelling: waypoint is not null) { CanJump = !InCombat && !_talking };
        map.JumpTo += room => ChangeRoom(room, ArrivalIn(_zone.Room(room)));
        map.TravelTo += TravelTo;
        ScreenManager.AddScreen(map);
    }

    /// <summary>The spawn a jump into a room from the zone map puts you on: its first.</summary>
    private static string ArrivalIn(RoomDefinition room)
    {
        foreach (string spawn in room.Spawns.Keys) return spawn;
        throw new InvalidOperationException($"Zones/{room.ZoneId}.txt: room {room.Id} has no spawn to jump to.");
    }

    /// <summary>Fast travels to a zone's waypoint you've found: beside the stone, in its room.</summary>
    /// <param name="zone">The zone.</param>
    private void TravelTo(string zone)
    {
        foreach (RoomDefinition room in ZoneCatalog.Get(zone).Rooms)
        {
            foreach (MapInteractable thing in room.Things)
            {
                if (thing.Kind != InteractableKind.Waypoint || !_game.Waypoints.Contains(zone + "/" + thing.Id)) continue;
                Travel(zone, room.Id, thing.Beside, Leader.Facing);
                return;
            }
        }
    }

    /// <summary>Goes to a spawn in a zone: another room of this zone swaps in with a fade; another zone is a new screen through the loading screen, with the autosave written on the way. A spawn the zone hasn't got is its start.</summary>
    /// <param name="zone">The zone, by its id.</param>
    /// <param name="spawn">The spawn there, by its name.</param>
    private void GoTo(string zone, string spawn)
    {
        if (!ZoneCatalog.TryResolve(zone, spawn, out RoomDefinition room, out MapSpawn at))
        {
            spawn = ZoneLoader.StartSpawn;
            ZoneCatalog.TryResolve(zone, spawn, out room, out at);
        }

        if (zone == _zone.Id)
        {
            ChangeRoom(room.Id, spawn);
            return;
        }
        Travel(zone, room.Id, at.Feet, at.Facing);
    }

    /// <summary>Goes to another zone: the save moves there, the autosave is written, and the loading screen takes over.</summary>
    /// <remarks>
    /// Everyone's put on the spot you arrive at, and the next zone spreads the others out behind you.
    /// The autosave goes before anything fades, so it has exactly what the next zone will start from.
    /// If it can't be written the game carries on, and the next zone says so.
    /// </remarks>
    /// <param name="zone">The zone.</param>
    /// <param name="room">The room of it you arrive in.</param>
    /// <param name="feet">Where you arrive, in world pixels.</param>
    /// <param name="facing">Which way you face when you get there.</param>
    private void Travel(string zone, string room, Vector2 feet, Direction facing)
    {
        Halt();

        _game.ZoneId = zone;
        _game.RoomId = room;
        foreach (PartyMember member in _game.Party)
        {
            member.Feet = feet;
            member.Facing = facing;
        }

        var next = new ExploreScreen(_game);
        if (!SaveSystem.Write(SaveSystem.AutoSlot, _game)) next._toast.Show(AutosaveFailedLine);

        LoadingScreen.Load(ScreenManager, ZoneCatalog.Get(zone).Name.ToUpperInvariant(), next);
    }

    /// <summary>What goes over whoever or whatever you're next to: their name, the key and the word for using it, and where the middle of the top of them is on screen.</summary>
    /// <remarks>Not over someone who's already saying something, since their line goes there.</remarks>
    /// <returns>The prompt, or null if there's nothing to prompt for.</returns>
    private (string Name, string Use, Vector2 Top)? Prompting()
    {
        if (_nearNpc is Npc npc)
        {
            if (npc.Line is not null || SayingBanter(npc.Walker)) return null;
            return (npc.Definition.Name.ToUpperInvariant(), "E  ·  TALK", HeadOnScreen(npc.Walker));
        }

        if (_nearDoor is RoomDoor door)
        {
            var middle = new Vector2(door.Foot.X, door.Area.Y + door.Area.Height / 2f);
            return (door.Name.ToUpperInvariant(), door.LockedWhy is not null ? LockedPrompt : EnterPrompt, _camera.ToScreen(middle));
        }

        if (_nearMember is int member)
        {
            WalkerSprite walker = _walkers[member];
            if (SayingBanter(walker)) return null;
            return (_game.Party[member].Name.ToUpperInvariant(), "E  ·  TALK", HeadOnScreen(walker));
        }

        if (_near is not MapInteractable thing) return null;

        PropSprite sprite = null;
        foreach ((MapInteractable each, PropSprite drawn) in _things)
            if (each == thing) sprite = drawn;
        if (sprite is null) return null;
        return (thing.Name.ToUpperInvariant(), "E  ·  " + thing.Prompt, _camera.ToScreen(thing.Foot - new Vector2(0f, sprite.Size.Y)));
    }

    /// <summary>The room the prompt takes up on screen, so the lines and markers over people's heads keep out of its way.</summary>
    /// <returns>Its room, or null if there's no prompt.</returns>
    private Rectangle? PromptBox()
    {
        if (Prompting() is not { } prompt) return null;

        float width = MathF.Max(_detailFont.MeasureString(prompt.Name).X, _detailFont.MeasureString(prompt.Use).X);
        float height = _detailFont.LineSpacing * 2f;
        return new Rectangle((int)(prompt.Top.X - width / 2f), (int)(prompt.Top.Y - PromptGap - height), (int)width, (int)height);
    }

    /// <summary>Writes the name of whoever or whatever you're next to over them, and the key and the word for using it, on the same dark smudge as what people say.</summary>
    private void DrawPrompt(SpriteBatch spriteBatch, float alpha)
    {
        if (Prompting() is not { } prompt || PromptBox() is not Rectangle box) return;

        box.Inflate(0, (int)LineSpread.Y);
        Text.DrawBacking(spriteBatch, _glow, box, (int)LineSpread.X, LineBacking * alpha);

        float y = prompt.Top.Y - PromptGap - _detailFont.LineSpacing * 2f;
        Text.Draw(spriteBatch, _detailFont, prompt.Name, prompt.Top.X, y, Palette.Bone * alpha, Palette.Shadow * alpha, centred: true);
        Text.Draw(spriteBatch, _detailFont, prompt.Use, prompt.Top.X, y + _detailFont.LineSpacing,
            Palette.Torch * alpha, Palette.Shadow * alpha, centred: true);
    }
}
