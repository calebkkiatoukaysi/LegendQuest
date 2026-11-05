using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.Save;
using LegendQuest.StateManagement;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The story on a map: conversations, the scenes that play by themselves, what you're to do next in
/// the corner of the screen, the townsfolk strolling about, and the ship coming in.
/// </summary>
/// <remarks>
/// The ship comes into the harbour once the Prologue's over: she sails in along the pier with you and
/// the Soul-Bound on her foredeck, her sail coming in as she slows, ties up clear of the boardwalk,
/// the gangplank slides out of her rail and down onto the pier, you both walk off along the pier and
/// up the jetty to the boardwalk, and Act 1's card comes up. Then the harbour's own scene takes over
/// (the Red Gulls). It's steps on a clock, not a general cutscene system; the scenes are data, in the
/// map files. Nothing you press moves you while the ship's coming in, but Escape skips it.
/// </remarks>
public partial class ExploreScreen
{
    private const string SkipHint = "ESC  ·  SKIP";
    private const string FirstChapter = "Chapter 1  ·  Saltwind Welcome";
    private const string ControlsLine = "WASD or click to walk  ·  E to talk  ·  M for the map";

    /// <summary>The flag the ship coming in sets once you're off it.</summary>
    public const string Arrived = "arrived";

    /// <summary>Where what you're to do next goes, under the map's name.</summary>
    private const float ObjectiveY = 50f;

    /// <summary>How far east of where she ties up the ship starts, and how long she takes to sail in.</summary>
    private const float SailFrom = 560f;
    private const float SailSeconds = 8f;

    /// <summary>How fast she's still going, as a share of her top speed, when her sail starts coming in, when it's nearly up, and when it's furled.</summary>
    private const float HaulAt = 0.3f;
    private const float NearlyUpAt = 0.08f;
    private const float FurledAt = 0.01f;

    /// <summary>How long each of the two pictures of her wake is up while she's moving.</summary>
    private const float WakeSeconds = 0.22f;

    /// <summary>When, after she stops, her lines go ashore, the gate in her rail opens and the plank starts sliding out, how long each push of it takes, and when the Soul-Bound sets off down it.</summary>
    private const float TieUpAt = 0.5f;
    private const float GateAt = 0.9f;
    private const float PlankAt = 1.1f;
    private const float PlankStepSeconds = 0.14f;
    private const float OffAt = 1.9f;

    /// <summary>How far behind the Soul-Bound you set off, how fast you both walk off (art pixels a second), and how long the act's card is up.</summary>
    private const float FollowGap = 0.8f;
    private const float DisembarkSpeed = 52f;
    private const float CardSeconds = 4.2f;

    /// <summary>How much the ship rocks up and down, in art pixels, how fast, and how long she takes to settle once she's stopped.</summary>
    private const float Bob = 1.2f;
    private const float BobRate = 1.8f;
    private const float SettleSeconds = 0.8f;

    /// <summary>The ship's pictures, as tools/generate_harbour.py's SHIP_FRAMES has them: tied up, then the sail full with her wake behind her (two of those), the sail being hauled up (two), nearly up, and furled with her lines not ashore yet.</summary>
    private const int ShipMoored = 0;
    private const int ShipFullSail = 1;
    private const int ShipHauling = 3;
    private const int ShipNearlyUp = 5;
    private const int ShipFurled = 6;

    /// <summary>Her near rail's pictures, as RAIL_FRAMES has them: the plank down on the pier, the gate in the rail shut, the gate open, then the plank sliding out, a picture a push.</summary>
    private const int PlankDown = 0;
    private const int GateShut = 1;
    private const int GateOpen = 2;
    private const int PlankSliding = 3;
    private const int PlankPushes = 3;

    /// <summary>The black bars across the top and bottom while it plays, and how fast they come and go.</summary>
    private const int BarHeight = 70;
    private const float BarSeconds = 0.8f;

    /// <summary>Where the camera looks while she sails in, from the ship's foot: the middle of her deck.</summary>
    private static readonly Vector2 ShipViewOffset = new(-40f, -50f);

    /// <summary>
    /// Where you and the Soul-Bound stand on her foredeck, from the ship's foot, close enough to her
    /// near rail that it hides your feet; and the way off: along the deck to the gangway, down the plank
    /// onto the pier, along the pier to the jetty, up the jetty to the boardwalk, and a step along it.
    /// Nobody goes behind her mast, her sail or her rigging, which are all north of the deck.
    /// </summary>
    private static readonly Vector2 YouOnDeck = new(-80f, -46f);
    private static readonly Vector2 SoulBoundOnDeck = new(-56f, -37f);
    private static readonly Vector2[] SoulBoundOff = { new(-44f, -34f), new(-44f, 20f), new(-152f, 20f), new(-152f, -154f), new(-120f, -168f) };
    private static readonly Vector2[] YouOff = { new(-44f, -34f), new(-44f, 20f), new(-164f, 20f), new(-164f, -146f), new(-140f, -156f) };

    private readonly InputAction _skip = new(new[] { Buttons.Start, Buttons.Back }, new[] { Keys.Escape }, true);

    /// <summary>Chapter 1's card, which the arrival ends on.</summary>
    private readonly ChapterCard _chapterCard = new();

    /// <summary>Which part of the arrival is playing, or None when it isn't.</summary>
    private enum Arriving { None, Sailing, Docked, Disembarking, Card }

    private Arriving _arriving;
    private float _arrivalClock;
    private float _bars;
    private PropSprite _ship;
    private PropSprite _gangplank;
    private bool _talking;

    /// <summary>Who's walking off the ship and the way they're going, a point at a time.</summary>
    private readonly List<(WalkerSprite Walker, Queue<Vector2> Way, float Wait)> _disembarking = new();

    /// <summary>Starts the ship coming in if this is the harbour, the Prologue's over, and you've not landed yet.</summary>
    /// <remarks>Otherwise she's left as the map has her: tied up, with her plank down on the pier.</remarks>
    private void BeginArrival()
    {
        if (_map.Id != "docks" || !_game.Flags.Contains(Prologue.Done) || _game.Flags.Contains(Arrived)) return;

        foreach (PropSprite prop in _everyProp)
        {
            if (prop.Definition.Id == "ship") _ship = prop;
            if (prop.Definition.Id == "gangplank") _gangplank = prop;
        }
        if (_ship is null || _gangplank is null || _walkers.Count < 2) return;

        _arriving = Arriving.Sailing;
        _arrivalClock = 0f;
        _bars = 1f;
        _ship.Frame = ShipFullSail;
        _gangplank.Frame = GateShut;
        MoveShip(new Vector2(SailFrom, 0f));
        PlaceOnDeck();
        _camera.SnapTo(ShipView());
    }

    /// <summary>Esc skips the ship coming in. Nothing else does anything while it plays.</summary>
    /// <returns>True if it's playing, so nothing else should look at the input.</returns>
    private bool HandleArrivalInput(InputState input)
    {
        if (_arriving == Arriving.None) return false;

        if (_skip.Occurred(input)) EndArrival();
        return true;
    }

    /// <summary>Runs the ship coming in a frame on.</summary>
    /// <returns>True if it's playing, so the party and the camera are its to move.</returns>
    private bool UpdateArrival(GameTime gameTime)
    {
        if (_arriving == Arriving.None)
        {
            _bars = MathF.Max(0f, _bars - (float)gameTime.ElapsedGameTime.TotalSeconds / BarSeconds);
            return false;
        }

        _arrivalClock += (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (_arriving)
        {
            case Arriving.Sailing:
                // She comes in fast and slows to a stop (an ease out), and how fast she's still
                // going says how much of her sail's up and how much wake she's leaving.
                float left = 1f - MathF.Min(1f, _arrivalClock / SailSeconds);
                MoveShip(new Vector2(MathF.Round(SailFrom * left * left * left), Rock(_arrivalClock, 1f)));
                _ship.Frame = SailingFrame(left * left);
                PlaceOnDeck();
                if (left <= 0f) Next(Arriving.Docked);
                break;

            case Arriving.Docked:
                MoveShip(new Vector2(0f, Rock(SailSeconds + _arrivalClock, MathF.Max(0f, 1f - _arrivalClock / SettleSeconds))));
                PlaceOnDeck();
                TieUp();
                if (_arrivalClock >= OffAt) StartDisembarking();
                break;

            case Arriving.Disembarking:
                if (Disembark(gameTime))
                {
                    _walkers[1].Face(_walkers[0].Position);
                    _walkers[0].Face(_walkers[1].Position);
                    Next(Arriving.Card);
                }
                break;

            case Arriving.Card:
                if (_arrivalClock >= CardSeconds) EndArrival();
                break;
        }

        foreach (WalkerSprite walker in _walkers) walker.Update(gameTime);
        _camera.Follow(_arriving == Arriving.Sailing ? ShipView() : (_walkers[0].Position + _walkers[1].Position) / 2f, gameTime);
        return true;
    }

    /// <summary>Puts the arrival on to its next part, and starts that part's clock.</summary>
    private void Next(Arriving step)
    {
        _arriving = step;
        _arrivalClock = 0f;
    }

    /// <summary>How far up or down the swell has the ship, in whole art pixels, at a moment of the arrival.</summary>
    /// <param name="seconds">How long since she started sailing in. It carries on from sailing into tying up, so she doesn't jump.</param>
    /// <param name="size">How much of the rocking's left: all of it while she's sailing, dying away as she settles.</param>
    private static float Rock(float seconds, float size) => MathF.Round(MathF.Sin(seconds * BobRate) * Bob * size);

    /// <summary>Which picture of the ship goes with how fast she's going, as a share of her top speed: the sail full and her wake churning while she's making way, then the sail coming in as she slows, and furled by the time she stops.</summary>
    private int SailingFrame(float speed)
    {
        int wake = (int)(_arrivalClock / WakeSeconds) % 2;
        if (speed > HaulAt) return ShipFullSail + wake;
        if (speed > NearlyUpAt) return ShipHauling + wake;
        return speed > FurledAt ? ShipNearlyUp : ShipFurled;
    }

    /// <summary>Ties her up a step at a time once she's stopped: her lines ashore with the bell, the gate in her rail open, then the plank sliding out a push at a time and down onto the pier.</summary>
    private void TieUp()
    {
        if (_arrivalClock >= TieUpAt && _ship.Frame != ShipMoored)
        {
            _ship.Frame = ShipMoored;
            _audio.Play(Sfx.ShipBell);
        }

        int push = (int)((_arrivalClock - PlankAt) / PlankStepSeconds);
        int frame = _arrivalClock < GateAt ? GateShut
            : _arrivalClock < PlankAt ? GateOpen
            : push < PlankPushes ? PlankSliding + push
            : PlankDown;
        if (frame == _gangplank.Frame) return;

        _gangplank.Frame = frame;
        if (frame == GateOpen) _audio.Play(Sfx.Lever);
        if (frame == PlankDown) _audio.Play(Sfx.Door);
    }

    /// <summary>Moves the ship off where she ties up, her near rail with her, and her crew on her deck.</summary>
    private void MoveShip(Vector2 offset)
    {
        _ship.Offset = offset;
        _gangplank.Offset = offset;
        foreach (Npc npc in _everyNpc)
            if (npc.Definition.Aboard == _ship.Definition.Id) npc.Walker.Position = npc.Definition.Feet + offset;
    }

    /// <summary>The middle of the ship's deck, which the camera keeps on while she sails in.</summary>
    private Vector2 ShipView() => _ship.Foot + _ship.Offset + ShipViewOffset;

    /// <summary>Stands you and the Soul-Bound on her foredeck, wherever she's got to: you looking up at the city, the Soul-Bound out ahead.</summary>
    private void PlaceOnDeck()
    {
        Vector2 deck = _ship.Foot + _ship.Offset;
        _walkers[0].Position = deck + YouOnDeck;
        _walkers[1].Position = deck + SoulBoundOnDeck;
        _walkers[0].Facing = Direction.Up;
        _walkers[1].Facing = Direction.Left;
    }

    /// <summary>Sets the Soul-Bound off down the gangplank, and you after them.</summary>
    private void StartDisembarking()
    {
        Next(Arriving.Disembarking);
        MoveShip(Vector2.Zero);
        _disembarking.Clear();
        _disembarking.Add((_walkers[1], Way(SoulBoundOff), 0f));
        _disembarking.Add((_walkers[0], Way(YouOff), FollowGap));
    }

    /// <summary>A way off the ship, from the ship's foot.</summary>
    private Queue<Vector2> Way(Vector2[] points)
    {
        var way = new Queue<Vector2>();
        foreach (Vector2 point in points) way.Enqueue(_ship.Foot + point);
        return way;
    }

    /// <summary>Walks each of you a step along your way off, straight there, the gangplank and all.</summary>
    /// <returns>True once you're both on the boardwalk.</returns>
    private bool Disembark(GameTime gameTime)
    {
        bool done = true;
        foreach ((WalkerSprite walker, Queue<Vector2> way, float wait) in _disembarking)
        {
            if (_arrivalClock < wait || way.Count == 0)
            {
                walker.IsWalking = false;
                done &= way.Count == 0;
                continue;
            }

            Vector2 to = way.Peek(), gap = to - walker.Position;
            float step = DisembarkSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            walker.Face(to);
            walker.IsWalking = true;
            if (gap.Length() <= step)
            {
                walker.Position = to;
                way.Dequeue();
            }
            else
            {
                walker.Position += Vector2.Normalize(gap) * step;
            }
            done = false;
        }
        return done;
    }

    /// <summary>Ends the ship coming in, whether it played out or got skipped: tied up with her plank down, you both on the boardwalk looking along it at the trouble, and the harbour's scene next.</summary>
    /// <remarks>The controls go up as a toast only with the tutorial's tips off. With them on, the tips teach the same things one at a time.</remarks>
    private void EndArrival()
    {
        MoveShip(Vector2.Zero);
        _ship.Frame = ShipMoored;
        _gangplank.Frame = PlankDown;
        _walkers[1].Position = _ship.Foot + SoulBoundOff[^1];
        _walkers[0].Position = _ship.Foot + YouOff[^1];
        foreach (WalkerSprite walker in _walkers) walker.IsWalking = false;
        _walkers[0].Facing = Direction.Right;
        _walkers[1].Facing = Direction.Right;
        _arriving = Arriving.None;

        _game.Flags.Add(Arrived);
        SelectLeader(0);
        SeedTrail();
        RefreshPresence();
        if (!TutorialsOn) _toast.Show(ControlsLine);
        if (!Autosave()) _toast.Show(AutosaveFailedLine);
    }

    /// <summary>Writes the autosave with everyone where they're stood.</summary>
    private bool Autosave()
    {
        Leader.IsWalking = false;
        for (int i = 0; i < _walkers.Count; i++)
        {
            _game.Party[i].Feet = _walkers[i].Position;
            _game.Party[i].Facing = _walkers[i].Facing;
        }
        return SaveSystem.Write(SaveSystem.AutoSlot, _game);
    }

    /// <summary>Plays the first of the map's scenes that can play and hasn't, if nothing else is going on.</summary>
    /// <returns>True if one started.</returns>
    private bool PlayScenes()
    {
        if (_talking) return false;

        foreach (MapScene scene in _map.Scenes)
        {
            string key = _map.Id + "/" + scene.Id;
            if (_game.Triggered.Contains(key) || !scene.Presence.Holds(_game.Flags)) continue;

            _game.Triggered.Add(key);
            Converse(DialogueCatalog.Get(scene.Talk), null);
            return true;
        }
        return false;
    }

    /// <summary>Starts a conversation over the map, with the map as the stage its cues play on. When it's over, whatever's to happen next does, and if it (or a travel-actor cue) sends the party somewhere, it goes.</summary>
    /// <param name="script">What gets said.</param>
    /// <param name="then">What happens after, or null.</param>
    private void Converse(DialogueScript script, Action then)
    {
        if (script is null || _talking) return;

        Halt();
        _talking = true;
        var talk = new DialogueScreen(script, _game, Speaker, _random, this);
        talk.Happened += OnDialogue;
        talk.Finished += travel =>
        {
            _talking = false;
            travel ??= _cueTravel;
            _cueTravel = null;
            then?.Invoke();
            OpenAwaitedShop();
            StartAwaitedFight();
            if (travel is null) return;

            string[] where = travel.Split(' ');
            MapDefinition to = MapCatalog.Get(where[0]);
            MapSpawn spawn = to.Spawns[where[1]];
            Leave(to, spawn.Feet, spawn.Facing);
        };
        ScreenManager.AddScreen(talk);
    }

    /// <summary>Catches the map up with something a conversation did: people coming and going with the flags, and someone joining the party, going to wait at camp, or swapping places with someone waiting.</summary>
    private void OnDialogue(DialogueEvent happened)
    {
        switch (happened.Kind)
        {
            case DialogueEventKind.Flag:
                RefreshPresence();
                break;

            case DialogueEventKind.Recruited:
                CatchUpParty();
                break;

            case DialogueEventKind.Shop:
                _shopAfterTalk = happened.Id;
                break;

            case DialogueEventKind.Fight:
                _fightAfterTalk = happened.Id;
                break;
        }
    }

    /// <summary>Who a speaker id in a conversation is: their name and the sheet their face comes off.</summary>
    /// <remarks>
    /// The party first, then whoever's on the map (whoever's waiting at camp too), then the story's
    /// cast wherever they are. A creature has no face to give: its sheet isn't a person's, so the
    /// portrait's crop would come off the wrong part of it, and its frame is left empty. A voice is
    /// whoever it's the voice of, found the same way, with no name: it's heard, not said by anyone
    /// there.
    /// </remarks>
    private (string Name, Texture2D Sheet) Speaker(string id)
    {
        if (Speakers.VoiceOf(id) is string whose) return (string.Empty, Speaker(whose).Sheet);

        for (int i = 0; i < _game.Party.Count; i++)
            if (_game.Party[i].Id == id) return (_game.Party[i].Name, _walkers[i].Sheet);
        foreach (Npc npc in _everyNpc)
        {
            if (npc.Definition.Id == id || npc.Definition.Cast == id)
                return (npc.Definition.Name, npc.Definition.Creature is null ? npc.Walker.Sheet : null);
        }
        if (Cast.Get(id) is CastMember cast)
        {
            Texture2D sheet = cast.Look is not null ? SheetFor(cast.Look)
                : cast.Sheet is not null ? _content.Load<Texture2D>(Townsfolk.Texture(cast.Sheet)) : null;
            return (cast.Name, sheet);
        }
        return (id, null);
    }

    /// <summary>Walks every townsperson who's strolling somewhere a step nearer it. They stop and rest if they get there, or if anything's in the way.</summary>
    private void MoveTownsfolk(GameTime gameTime)
    {
        float step = Npc.StrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (Npc npc in _npcs)
        {
            if (npc.Heading is not Vector2 to) continue;

            Vector2 at = npc.Walker.Position, gap = to - at;
            if (gap.Length() <= step)
            {
                npc.Arrive();
                continue;
            }

            Vector2 next = at + Vector2.Normalize(gap) * step;
            if (Crowded(npc, next))
            {
                npc.Arrive();
                continue;
            }

            npc.Walker.Position = next;
            npc.Walker.IsWalking = true;
            npc.Walker.Face(to);
        }
    }

    /// <summary>Whether a townsperson's feet (or a creature's) would bump into anything at a point: a wall, a prop, someone else, or the party.</summary>
    private bool Crowded(Npc mover, Vector2 at)
    {
        if (!_tilemap.Bounds.Contains(at)) return true;

        BoundingRectangle feet = mover.FootprintAt(at);
        foreach (BoundingRectangle solid in _tilemap.Walls)
            if (feet.CollidesWith(solid)) return true;
        foreach (PropSprite prop in AllProps())
            foreach (BoundingRectangle footprint in prop.Footprints)
                if (feet.CollidesWith(footprint)) return true;
        foreach (Npc other in _npcs)
            if (other != mover && feet.CollidesWith(other.Footprint)) return true;
        foreach (WalkerSprite walker in _walkers)
            if (feet.CollidesWith(Grown(FeetAt(walker.Position), 2f))) return true;
        return false;
    }

    /// <summary>The black bars, the skip hint and the act's card while the ship comes in.</summary>
    private void DrawArrival(SpriteBatch spriteBatch, float alpha)
    {
        if (_bars <= 0f) return;

        int height = (int)MathF.Round(BarHeight * _bars);
        spriteBatch.Draw(ScreenManager.BlankTexture, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, height), null,
            Color.Black * alpha, 0f, Vector2.Zero, SpriteEffects.None, Layers.Veil);
        spriteBatch.Draw(ScreenManager.BlankTexture, new Rectangle(0, LegendQuestGame.ScreenHeight - height, LegendQuestGame.ScreenWidth, height), null,
            Color.Black * alpha, 0f, Vector2.Zero, SpriteEffects.None, Layers.Veil);

        if (_arriving is Arriving.Sailing or Arriving.Docked or Arriving.Disembarking)
            Text.Draw(spriteBatch, _detailFont, SkipHint, LegendQuestGame.ScreenWidth - Margin, LegendQuestGame.ScreenHeight - height / 2f - _detailFont.LineSpacing / 2f,
                Palette.Note * alpha, Palette.Shadow * alpha, rightAligned: true);

        if (_arriving != Arriving.Card) return;

        float t = _arrivalClock / CardSeconds;
        float shown = MathHelper.Clamp(MathF.Min(t * 4f, (1f - t) * 3f), 0f, 1f) * alpha;
        _chapterCard.Draw(spriteBatch, FirstChapter, _game.Meter(Meters.Act), shown);
    }

    /// <summary>What you're to do next, under the map's name, on its smudge.</summary>
    private void DrawObjective(SpriteBatch spriteBatch, float alpha)
    {
        if (string.IsNullOrEmpty(_game.Objective) || _arriving != Arriving.None) return;

        string text = _game.Objective.ToUpperInvariant();
        Vector2 size = _detailFont.MeasureString(text);
        var area = new Rectangle((int)Margin, (int)(ObjectiveY - HeadingSpread.Y), (int)size.X, (int)(size.Y + HeadingSpread.Y * 2f));
        Text.DrawBacking(spriteBatch, _glow, area, (int)HeadingSpread.X, HeadingBacking * alpha);
        Text.Draw(spriteBatch, _detailFont, text, Margin, ObjectiveY, Palette.Torch * alpha, Palette.Shadow * alpha);
    }
}
