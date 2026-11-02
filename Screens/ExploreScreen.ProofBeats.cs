using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Collisions;
using LegendQuest.Dialogue;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The proof run's beats: the later chapters' visits to a map, written as what happens in order
/// (walk into this, talk to them, read it through answering these, go out that way) rather than on a
/// clock.
/// </summary>
/// <remarks>
/// The first maps' schedules are by frame, which suits a walk round a map that's always the same.
/// A chapter is mostly conversations, and how long one takes depends on its lines and the answers
/// picked, so each of its visits is a queue of beats instead, each finishing when what it waits on
/// has happened. Everything still goes in the way a player's would: triggers and exits go off
/// through the same check that walking into them does, and talking is the E key's code. People and
/// places are found by their ids in the map file, so moving one on the map doesn't break the run,
/// and the run stands you in a trigger or an exit at the first clear spot from the side it's
/// crossed from, so a prop inside it doesn't either.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>The longest a chapter's visit to a map can run before the run gives up on it and moves on: a little under two minutes, well past the longest conversation.</summary>
    internal const int ChapterProofEnd = Settled + 6000;

    /// <summary>How many frames in a row there has to be no conversation and no dice up before a reading beat counts it over. Long enough for a scene that starts straight after another to get going.</summary>
    private const int ProofQuietFrames = 40;

    /// <summary>How finely an area's searched for a clear spot to stand in, in art pixels.</summary>
    private const float FootholdStep = 2f;

    /// <summary>What a reading beat holds for as a shot when it means the first question, rather than a line by someone.</summary>
    private const string Asked = "?";

    /// <summary>What a beat did on a frame: whether it's finished, and the shot to take on that frame, if any.</summary>
    private readonly record struct ProofTick(bool Done, string Shot = null);

    /// <summary>One beat of a visit, called once a frame until it says it's done.</summary>
    private delegate ProofTick ProofBeat();

    /// <summary>The beats still to play on this visit, built the first frame they're needed.</summary>
    private Queue<ProofBeat> _proofBeats;

    /// <summary>Plays a visit's beats in order, a frame at a time.</summary>
    /// <param name="frame">Which drawn frame of the visit this is.</param>
    /// <param name="beats">Builds the visit's beats, the first time they're needed.</param>
    /// <returns>The file the frame should be written to, or null.</returns>
    private string Beats(int frame, Func<IEnumerable<ProofBeat>> beats)
    {
        if (frame < Settled) return null;

        _proofBeats ??= new Queue<ProofBeat>(beats());
        while (_proofBeats.Count > 0)
        {
            ProofTick tick = _proofBeats.Peek()();
            if (tick.Done) _proofBeats.Dequeue();
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

    /// <summary>Sets the answers to the next questions, in order. Any question after them gets its first option.</summary>
    private ProofBeat Answer(params int[] choices) => Do(() => ProofChoices(choices));

    /// <summary>Reads whatever conversations come up through to the end, answering the questions with the answers set, and holding on the first line each named speaker says for a shot.</summary>
    /// <param name="holds">Who to hold on (a speaker id, or Asked for the first question) and the shot to take.</param>
    private ProofBeat Read(params (string Speaker, string Shot)[] holds)
    {
        int quiet = 0;
        return () =>
        {
            DialogueScreen talk = ProofDialogue();
            if (talk is null && ProofDice() is null)
                return new ProofTick(++quiet >= ProofQuietFrames);

            quiet = 0;
            if (talk is not null)
            {
                foreach ((string speaker, string shot) in holds)
                {
                    // A hold, once it's started, carries on to its shot whatever the line's become:
                    // finishing a line that's a question turns it into the question, and nothing
                    // reads on while a hold's waiting.
                    if (speaker == Asked)
                    {
                        if (_proofHolding is null && talk.ProofAsking && ProofOnce(shot)) return new ProofTick(false, shot);
                        continue;
                    }

                    bool says = _proofHolding == shot || talk.ProofLine is DialogueNode line && line.Speaker == speaker;
                    if (says && ProofHold(talk, shot)) return new ProofTick(false, shot);
                }
            }

            ProofTalk();
            return new ProofTick(false);
        };
    }

    /// <summary>Walks up to someone on the map and talks to them: stood a tile in front of them, facing them, then E.</summary>
    /// <param name="id">Their id in the map file.</param>
    private IEnumerable<ProofBeat> TalkTo(string id)
    {
        yield return Do(() =>
        {
            Vector2 feet = ProofNpc(id) / Tilemap.TileSize;
            ProofPlace(feet.X, feet.Y + 1f, Direction.Up);
        });
        yield return Wait(2);
        yield return Do(ProofUse);
    }

    /// <summary>Steps into a trigger, the way it's crossed on foot.</summary>
    /// <param name="id">The trigger's id in the map file.</param>
    /// <param name="way">Which way it's crossed: you stand at the first clear spot in it from the other side.</param>
    private ProofBeat Into(string id, Direction way) => Do(() => StandIn(_map.Triggers.First(trigger => trigger.Id == id).Area, way));

    /// <summary>Steps out through an exit or in through a door.</summary>
    /// <param name="id">The exit's or entrance's id in the map file.</param>
    /// <param name="way">Which way it's gone through.</param>
    private ProofBeat Out(string id, Direction way) => Do(() => StandIn(_map.Exits.First(exit => exit.Id == id).Area, way));

    /// <summary>Stops walking, wherever the party's got to.</summary>
    private ProofBeat Stop() => Do(() => _proofMove = Vector2.Zero);

    /// <summary>Stands the party in an area, facing the way it's crossed, at the first clear spot in it from the side it's crossed from. Being in it is what sets it off, on the next frame.</summary>
    private void StandIn(BoundingRectangle area, Direction way)
    {
        Vector2 at = FootholdIn(area, way) / Tilemap.TileSize;
        ProofPlace(at.X, at.Y, way);
        _proofMove = Vector2.Zero;
    }

    /// <summary>
    /// The first spot in an area where your feet touch nothing solid, looking in from the side it's
    /// crossed from: a row at a time across it, from the middle of each row outwards. The middle of
    /// the area if there's nowhere clear, which only a map with a trigger nobody could reach has.
    /// </summary>
    private Vector2 FootholdIn(BoundingRectangle area, Direction way)
    {
        Vector2 ahead = Ahead(way);
        bool across = ahead.X != 0f;
        float depth = across ? area.Width : area.Height;
        float breadth = across ? area.Height : area.Width;
        Vector2 middle = new(area.X + area.Width / 2f, area.Y + area.Height / 2f);

        for (float into = FootholdStep; into < depth; into += FootholdStep)
        {
            for (float off = 0f; off <= breadth / 2f; off += FootholdStep)
            {
                foreach (float side in new[] { off, -off })
                {
                    float along = (ahead.X + ahead.Y) > 0f ? into : depth - into;
                    Vector2 spot = across
                        ? new Vector2(area.X + along, middle.Y + side)
                        : new Vector2(middle.X + side, area.Y + along);
                    if (Clear(spot)) return spot;
                }
            }
        }
        return middle;
    }

    /// <summary>Whether your feet could stand here: on the map, and touching nothing solid.</summary>
    private bool Clear(Vector2 feet)
    {
        if (!_tilemap.Bounds.Contains(feet)) return false;
        BoundingRectangle box = FeetAt(feet);
        foreach (BoundingRectangle solid in Solids())
            if (box.CollidesWith(solid)) return false;
        return true;
    }
}
