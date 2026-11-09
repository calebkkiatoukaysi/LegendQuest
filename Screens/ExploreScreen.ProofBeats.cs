using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Dialogue;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The proof run's beats: a visit to a zone written as what happens in order (walk into this, talk
/// to them, read it through answering these, go out that way) rather than on a clock, and the
/// helpers that find what a beat names in the zone.
/// </summary>
/// <remarks>
/// <para>
/// A visit is mostly conversations, walks and room changes, and how long each takes depends on its
/// lines, the answers picked and the fades, so every visit is a queue of beats, each finishing when
/// what it waits on has happened. Everything still goes in the way a player's would: triggers and
/// ways out go off through the same check that walking into them does, talking and using are the E
/// key's code (unless something nearer would take the E, when it's the same call E would have made),
/// walking is the movement keys' code, and the mouse is the pretend one the screen reads like the
/// real one.
/// </para>
/// <para>
/// People, triggers, doors, ways out, things and spawns are found by their ids in the zone file, so
/// moving one doesn't break the run. The helpers are zone-scoped: if what a beat names is in another
/// room of the zone, it changes room first, through the same fade a player gets, and waits for it.
/// The run stands you in a trigger or a way out at the first clear spot from the side you're on, so
/// a prop inside it doesn't stop it, beside someone to talk to them, at a door's foot to go through
/// it, and in reach of a thing to use it.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>The longest a visit to a zone can run before the run gives up on it and moves on: a little under two minutes, well past the longest. Every visit ends sooner, by leaving the zone.</summary>
    internal const int ProofVisitEnd = Settled + 6000;

    /// <summary>How many frames in a row there has to be no conversation and no dice up before a reading beat counts it over. Long enough for a scene that starts straight after another to get going.</summary>
    private const int ProofQuietFrames = 40;

    /// <summary>How finely an area's searched for a clear spot to stand in, in art pixels.</summary>
    private const float FootholdStep = 2f;

    /// <summary>How far to one side of someone you stand to talk to them, in art pixels: well inside TalkReach, and clear of their feet.</summary>
    private const float TalkSide = 12f;

    /// <summary>The spots to try round a thing's foot for using it, nearest the front first, in art pixels.</summary>
    private static readonly Vector2[] UseOffsets = { new(0f, 8f), new(-14f, 4f), new(14f, 4f), new(0f, 14f), new(-18f, 10f), new(18f, 10f) };

    /// <summary>How long a walking beat can get nowhere before it stops, in frames.</summary>
    private const int ProofStuckFrames = 20;

    /// <summary>How long a waiting beat waits for what it waits on before it gives up and lets the run carry on, in frames.</summary>
    private const int ProofWaitFrames = 600;

    /// <summary>What a reading beat holds for as a shot when it means the first question, or the dice landing, rather than a line by someone.</summary>
    private const string Asked = "?";
    private const string Rolled = "#";

    /// <summary>What a beat did on a frame: whether it's finished, and the shot to take on that frame, if any.</summary>
    private readonly record struct ProofTick(bool Done, string Shot = null);

    /// <summary>One beat of a visit, called once a frame until it says it's done.</summary>
    private delegate ProofTick ProofBeat();

    /// <summary>The beats still to play on this visit, built the first frame they're needed.</summary>
    private Queue<ProofBeat> _proofBeats;

    /// <summary>How many of this visit's beats are done, so a fight lost partway through it can pick the visit up again after the reload instead of starting it over in the wrong room.</summary>
    internal static int ProofBeatsDone;

    /// <summary>How many beats the next visit skips as already done: set when a lost fight's save is loaded, spent when the visit's beats are built.</summary>
    internal static int ProofResumeAt;

    /// <summary>Whether the run's holding Alt down.</summary>
    private bool _proofAlt;

    /// <summary>Plays a visit's beats in order, a frame at a time.</summary>
    /// <param name="frame">Which drawn frame of the visit this is.</param>
    /// <param name="beats">Builds the visit's beats, the first time they're needed.</param>
    /// <returns>The file the frame should be written to, or null.</returns>
    private string Beats(int frame, Func<IEnumerable<ProofBeat>> beats)
    {
        if (frame < Settled) return null;

        if (_proofBeats is null)
        {
            _proofBeats = new Queue<ProofBeat>(beats());
            ProofBeatsDone = 0;

            // Back from a lost fight: what came before it has happened, and the save it loaded is
            // from just before the fight, so the visit carries on from the beat that was playing.
            for (; ProofResumeAt > 0 && _proofBeats.Count > 0; ProofResumeAt--, ProofBeatsDone++) _proofBeats.Dequeue();
            ProofResumeAt = 0;
        }

        while (_proofBeats.Count > 0)
        {
            ProofTick tick = _proofBeats.Peek()();
            if (tick.Done)
            {
                _proofBeats.Dequeue();
                ProofBeatsDone++;
            }
            if (tick.Shot is not null) return tick.Shot;
            if (!tick.Done) return null;
        }
        return null;
    }

    /// <summary>Takes a shot of the screen as it is.</summary>
    private static ProofBeat Shot(string name) => () => new ProofTick(true, name);

    /// <summary>Does something, once.</summary>
    private static ProofBeat Do(Action action) => () =>
    {
        action();
        return new ProofTick(true);
    };

    /// <summary>Lets some frames go by.</summary>
    private static ProofBeat Wait(int frames)
    {
        int left = frames;
        return () => new ProofTick(--left <= 0);
    }

    /// <summary>Waits until something's so, or ProofWaitFrames, whichever's first.</summary>
    private static ProofBeat WaitFor(Func<bool> condition)
    {
        int left = ProofWaitFrames;
        return () => new ProofTick(condition() || --left <= 0);
    }

    /// <summary>Sets the answers to the next questions, in order. Any question after them gets its first option.</summary>
    private ProofBeat Answer(params int[] choices) => Do(() => ProofChoices(choices));

    /// <summary>Reads whatever conversations come up through to the end, answering the questions with the answers set, and holding on the first line each named speaker says for a shot.</summary>
    /// <param name="holds">Who to hold on (a speaker id, Asked for the first question, Rolled for the dice landing) and the shot to take.</param>
    private ProofBeat Read(params (string Speaker, string Shot)[] holds) => ReadUntil(() => true, holds);

    /// <summary>Reads conversations the way Read does, but only counts it over once something's so as well: for a scene that a fight comes in the middle of, or that never ends but by leaving.</summary>
    /// <param name="over">What has to be so for the reading to be over, as well as nothing being up.</param>
    /// <param name="holds">Who to hold on, as for Read.</param>
    private ProofBeat ReadUntil(Func<bool> over, params (string Speaker, string Shot)[] holds)
    {
        int quiet = 0;
        return () =>
        {
            DialogueScreen talk = ProofDialogue();
            DiceRollOverlay dice = ProofDice();
            if (talk is null && dice is null)
                return new ProofTick(++quiet >= ProofQuietFrames && over());

            quiet = 0;
            _proofMove = Vector2.Zero;
            foreach ((string speaker, string shot) in holds)
            {
                if (speaker == Rolled)
                {
                    if (dice is not null && dice.ProofLanded && ProofOnce(shot)) return new ProofTick(false, shot);
                    continue;
                }
                if (talk is null) continue;

                // A hold, once it's started, carries on to its shot whatever the line's become:
                // finishing a line that's a question turns it into the question, and nothing reads
                // on while a hold's waiting.
                if (speaker == Asked)
                {
                    if (_proofHolding is null && talk.ProofAsking && ProofOnce(shot)) return new ProofTick(false, shot);
                    continue;
                }

                bool says = _proofHolding == shot || talk.ProofLine is DialogueNode line && line.Speaker == speaker;
                if (says && ProofHold(talk, shot)) return new ProofTick(false, shot);
            }

            ProofTalk();
            return new ProofTick(false);
        };
    }

    /// <summary>Stops walking, wherever the party's got to.</summary>
    private ProofBeat Stop() => Do(() => _proofMove = Vector2.Zero);

    /// <summary>Walks along the room with the movement keys, as far as it says (left if it's less than nothing), and stops there, or where something's in the way, or when a conversation starts.</summary>
    /// <param name="dx">How far, in art pixels.</param>
    private ProofBeat Walk(float dx)
    {
        float? from = null;
        float best = 0f;
        int stuck = 0;
        return () =>
        {
            from ??= Leader.Position.X;
            float gone = MathF.Abs(Leader.Position.X - from.Value);
            stuck = gone > best + 0.01f ? 0 : stuck + 1;
            best = MathF.Max(best, gone);

            bool done = gone >= MathF.Abs(dx) || stuck > ProofStuckFrames || ProofDialogue() is not null || _roomChange is not null;
            _proofMove = done ? Vector2.Zero : new Vector2(MathF.Sign(dx), 0f);
            return new ProofTick(done);
        };
    }

    /// <summary>Holds Alt down, or lets it go.</summary>
    private ProofBeat ShowAll(bool on) => Do(() => _proofAlt = on);

    /// <summary>Changes room, if what an id names is in another room of the zone, and waits till the new room's in.</summary>
    /// <param name="id">Anyone, any trigger, door, way out, thing or spawn in the zone, by id. Anyone the zone doesn't put down (someone waiting at camp) is taken to be in this room.</param>
    private IEnumerable<ProofBeat> To(string id)
    {
        if (RoomOf(id) is string room) yield return InRoom(room);
    }

    /// <summary>Changes to a room of the zone, arriving on its first spawn, unless you're in it; then waits till it's faded in.</summary>
    private ProofBeat InRoom(string room)
    {
        bool asked = false;
        int left = ProofWaitFrames;
        return () =>
        {
            if (!asked && _room.Id != room && _roomChange is null)
            {
                ChangeRoom(room, ArrivalIn(_zone.Room(room)));
                asked = true;
            }
            return new ProofTick(_room.Id == room && _roomChange is null && _roomFade <= 0f || --left <= 0);
        };
    }

    /// <summary>Which room of the zone an id is in, or null if it's not one the zone file puts down.</summary>
    private string RoomOf(string id)
    {
        foreach (RoomDefinition room in _zone.Rooms)
        {
            if (room.Spawns.ContainsKey(id)) return room.Id;
            foreach (MapNpc npc in room.Npcs) if (npc.Id == id) return room.Id;
            foreach (MapTrigger trigger in room.Triggers) if (trigger.Id == id) return room.Id;
            foreach (MapInteractable thing in room.Things) if (thing.Id == id) return room.Id;
            foreach (RoomExit exit in room.Exits) if (exit.Id == id) return room.Id;
            foreach (RoomDoor door in room.Doors) if (door.Id == id) return room.Id;
        }
        return null;
    }

    /// <summary>Goes to a spawn of the zone: into its room, and onto it, facing the way it faces.</summary>
    private IEnumerable<ProofBeat> GoToSpawn(string spawn)
    {
        foreach (ProofBeat beat in To(spawn)) yield return beat;
        yield return Do(() =>
        {
            MapSpawn at = _room.Spawns[spawn];
            ProofPlace(at.Feet, at.Facing);
        });
    }

    /// <summary>Walks up to someone in the zone and talks to them: stood beside them, facing them, then E.</summary>
    /// <param name="id">Their id in the zone file, or their origin id if they're waiting at camp.</param>
    private IEnumerable<ProofBeat> TalkTo(string id)
    {
        foreach (ProofBeat beat in To(id)) yield return beat;
        yield return Do(() =>
        {
            Vector2 feet = ProofNpc(id).Walker.Position;
            Vector2 spot = _walk.Fits(feet - new Vector2(TalkSide, 0f)) ? feet - new Vector2(TalkSide, 0f) : feet + new Vector2(TalkSide, 0f);
            ProofPlace(spot, spot.X < feet.X ? Direction.Right : Direction.Left);
        });
        yield return Wait(2);
        yield return Do(() =>
        {
            Npc npc = ProofNpc(id);
            if (_nearNpc == npc) ProofUse();
            else
            {
                Halt();
                Talk(npc);
            }
        });
    }

    /// <summary>Walks up to one of the party and talks to them: you beside them, facing them, then E.</summary>
    /// <param name="id">Their origin id.</param>
    private IEnumerable<ProofBeat> TalkToFollower(string id)
    {
        yield return Do(() =>
        {
            Vector2 feet = _walkers[_game.Party.FindIndex(member => member.Id == id)].Position;
            Vector2 spot = _walk.Fits(feet - new Vector2(TalkSide, 0f)) ? feet - new Vector2(TalkSide, 0f) : feet + new Vector2(TalkSide, 0f);
            ProofPlaceAlone(spot, spot.X < feet.X ? Direction.Right : Direction.Left);
        });
        yield return Wait(2);
        yield return Do(() =>
        {
            int index = _game.Party.FindIndex(member => member.Id == id);
            if (_nearMember == index) ProofUse();
            else
            {
                Halt();
                TalkToMember(index);
            }
        });
    }

    /// <summary>Steps into a trigger in the zone, the way it's crossed on foot from the side you're on. Being in it is what sets it off, on the next frame; arriving in the room already standing in it (on a spawn inside it) counts as having walked out of it first.</summary>
    /// <param name="id">The trigger's id in the zone file.</param>
    private IEnumerable<ProofBeat> Into(string id)
    {
        foreach (ProofBeat beat in To(id)) yield return beat;
        yield return Do(() =>
        {
            _inside.Remove(id);
            StandIn(Trigger(id).Area);
        });
    }

    /// <summary>Goes out of the room by a way out or a door in the zone: stood in the way out's strip, which takes it on the next frame, or at the door's foot, then E. Then waits for the room it goes to, if it's one of this zone's; a locked one only says why.</summary>
    /// <param name="id">The way out's or the door's id in the zone file.</param>
    private IEnumerable<ProofBeat> Out(string id)
    {
        foreach (ProofBeat beat in To(id)) yield return beat;
        if (DoorInZone(id))
        {
            foreach (ProofBeat beat in AtDoor(id)) yield return beat;
            yield return Wait(2);
            yield return Do(() =>
            {
                RoomDoor door = FindDoor(id);
                if (_nearDoor == door) ProofUse();
                else UseDoor(door);
            });
        }
        else
        {
            yield return Do(() =>
            {
                _inside.Remove(id);
                StandIn(ExitArea(FindExit(id)));
            });
            yield return Wait(2);
        }
        yield return WaitFor(() => IsExiting || _roomChange is null && _roomFade <= 0f);
    }

    /// <summary>Whether an id is a door's in any room of the zone, rather than a way out's. Asked as the beats are built, before any room's changed.</summary>
    private bool DoorInZone(string id)
    {
        foreach (RoomDefinition room in _zone.Rooms)
            foreach (RoomDoor door in room.Doors)
                if (door.Id == id) return true;
        return false;
    }

    /// <summary>Stands at the foot of a door in the zone, facing it, without going through.</summary>
    /// <param name="id">The door's id in the zone file.</param>
    private IEnumerable<ProofBeat> AtDoor(string id)
    {
        foreach (ProofBeat beat in To(id)) yield return beat;
        yield return Do(() =>
        {
            RoomDoor door = FindDoor(id);
            ProofPlace(DoorSpot(door), Leader.Position.X < door.Foot.X ? Direction.Right : Direction.Left);
        });
    }

    /// <summary>Walks up to a thing in the zone and uses it: in reach of it, facing it, then E.</summary>
    /// <param name="id">The thing's id in the zone file.</param>
    private IEnumerable<ProofBeat> UseThing(string id)
    {
        foreach (ProofBeat beat in NextTo(id)) yield return beat;
        yield return Wait(2);
        yield return Do(() =>
        {
            MapInteractable thing = FindThing(id);
            if (_near == thing) ProofUse();
            else
            {
                Halt();
                Use(thing);
            }
        });
    }

    /// <summary>Stands in reach of a thing in the zone, facing it, without using it: the first spot round its foot that's clear and in reach.</summary>
    /// <param name="id">The thing's id in the zone file.</param>
    private IEnumerable<ProofBeat> NextTo(string id)
    {
        foreach (ProofBeat beat in To(id)) yield return beat;
        yield return Do(() =>
        {
            MapInteractable thing = FindThing(id);
            Vector2 spot = UseSpot(thing) + UseOffsets[0];
            foreach (Vector2 offset in UseOffsets)
            {
                Vector2 tried = _walk.NearestOnFloor(UseSpot(thing) + offset);
                if (!_walk.Fits(tried) || Vector2.Distance(tried, thing.Foot) > thing.Reach - ReachMargin) continue;
                spot = tried;
                break;
            }
            ProofPlace(spot, spot.X <= thing.Foot.X ? Direction.Right : Direction.Left);
        });
    }

    /// <summary>Keeps the pretend mouse on someone, a door, a way out's chevrons or a thing in this room, by id.</summary>
    /// <param name="id">Its id in the zone file, or the origin id of someone waiting at camp.</param>
    private ProofBeat Hover(string id) => Do(() => ProofAimAt(PointOf(id)));

    /// <summary>Opens the zone map, and waits for it to come in.</summary>
    private IEnumerable<ProofBeat> ZoneMap()
    {
        yield return Do(() => OpenMap(null));
        yield return WaitFor(() => ProofMap() is not null);
        yield return Wait(ProofScreenComesIn);
    }

    /// <summary>Jumps to a room from the zone map that's up, and waits till it's faded in.</summary>
    private IEnumerable<ProofBeat> Jump(string room)
    {
        yield return Do(() => ProofMap().ProofJump(room));
        yield return InRoom(room);
    }

    /// <summary>Closes the zone map that's up.</summary>
    private ProofBeat CloseMap() => Do(() => ProofMap()?.ExitScreen());

    /// <summary>Walks up to a waypoint in the zone, uses it, and fast travels from the zone map's world that opens.</summary>
    /// <param name="waypoint">The waypoint's id in the zone file.</param>
    /// <param name="zone">The zone to travel to.</param>
    private IEnumerable<ProofBeat> TravelFrom(string waypoint, string zone)
    {
        foreach (ProofBeat beat in UseThing(waypoint)) yield return beat;
        yield return WaitFor(() => ProofMap() is not null);
        yield return Wait(ProofScreenComesIn);
        yield return Do(() => ProofMap().ProofTravel(zone));
    }

    /// <summary>Goes straight to a spawn in another zone, the way the run skips a walk that would only be walking.</summary>
    private ProofBeat Teleport(string zone, string spawn) => Do(() => GoTo(zone, spawn));

    /// <summary>Stands the party in an area at the first clear spot in it from the side you're on, looking along it a step at a time, from the middle of the floor's depth outwards. The middle of the area on the floor if there's nowhere clear.</summary>
    private void StandIn(BoundingRectangle area)
    {
        bool fromLeft = Leader.Position.X <= area.X + area.Width / 2f;
        Vector2 spot = _walk.NearestOnFloor(new Vector2(area.X + area.Width / 2f, area.Y + area.Height / 2f));
        bool found = false;
        for (float into = FootholdStep; into < area.Width && !found; into += FootholdStep)
        {
            float x = fromLeft ? area.X + into : area.Right - into;
            float back = MathF.Max(area.Top, _room.BackEdgeAt(x)), front = MathF.Min(area.Bottom, _room.FrontEdgeAt(x));
            float middle = (back + front) / 2f;
            for (float off = 0f; off <= (front - back) / 2f && !found; off += FootholdStep)
            {
                foreach (float side in new[] { off, -off })
                {
                    var tried = new Vector2(x, middle + side);
                    if (!_walk.Fits(tried)) continue;
                    spot = tried;
                    found = true;
                    break;
                }
            }
        }

        ProofPlace(spot, fromLeft ? Direction.Right : Direction.Left);
        _proofMove = Vector2.Zero;
    }

    /// <summary>Where the pretend mouse goes to point at something in this room, by id: someone's chest, a door's middle, a way out's chevrons, or the middle of a thing's picture.</summary>
    private Vector2 PointOf(string id)
    {
        if (_npcs.Find(npc => npc.Definition.Id == id) is Npc npc) return npc.Walker.Position - new Vector2(0f, npc.Walker.Size.Y / 2f);
        if (FindDoor(id) is RoomDoor door) return new Vector2(door.Foot.X, door.Area.Y + door.Area.Height / 2f);
        if (FindThing(id) is MapInteractable thing) return thing.Foot - new Vector2(0f, ProofThingHeight);

        RoomExit exit = FindExit(id);
        return exit.Side switch
        {
            ExitSide.West => new Vector2(ProofChevronIn, _room.BackEdgeAt(ProofChevronIn) + ProofChevronUp),
            ExitSide.East => new Vector2(_room.Width - ProofChevronIn, _room.BackEdgeAt(_room.Width - ProofChevronIn) + ProofChevronUp),
            _ => new Vector2(exit.X, _room.BackEdgeAt(exit.X) - ProofChevronUp),
        };
    }

    /// <summary>Where the pretend mouse points at a way out's chevrons: this far in from the end, and this far onto (or, for a stair, over) the back of the floor, in art pixels.</summary>
    private const float ProofChevronIn = 16f;
    private const float ProofChevronUp = 12f;

    /// <summary>How far up a thing's picture the pretend mouse points, in art pixels.</summary>
    private const float ProofThingHeight = 10f;

    /// <summary>How long a screen opened over the room gets to come in before anything's done with it or it's shot, in frames.</summary>
    private const int ProofScreenComesIn = 24;

    /// <summary>A trigger in this room, by id.</summary>
    /// <exception cref="InvalidOperationException">If there isn't one, which means a beat names a trigger the zone file doesn't have.</exception>
    private MapTrigger Trigger(string id) =>
        _room.Triggers.FirstOrDefault(trigger => trigger.Id == id) ?? throw Missing("trigger", id);

    /// <summary>A way out of this room, by id.</summary>
    private RoomExit FindExit(string id) =>
        _room.Exits.FirstOrDefault(exit => exit.Id == id) ?? throw Missing("way out", id);

    /// <summary>A door in this room, by id, or null if it hasn't one by that id.</summary>
    private RoomDoor FindDoor(string id) => _room.Doors.FirstOrDefault(door => door.Id == id);

    /// <summary>A thing in this room, by id, or null if it hasn't one by that id.</summary>
    private MapInteractable FindThing(string id) => _room.Things.FirstOrDefault(thing => thing.Id == id);

    /// <summary>Why the run can't go on: a beat names something that isn't where it should be.</summary>
    private InvalidOperationException Missing(string what, string id) =>
        new($"The proof run looked for the {what} {id} in {_zone.Id}/{_room.Id}, and it isn't there.");
}
