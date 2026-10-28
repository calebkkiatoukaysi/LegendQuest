using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Collisions;
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
/// The ship comes into the harbour once the Prologue's over: it sails in with you and the Soul-Bound
/// on its deck, ties up, the gangplank goes down, you both walk off onto the boardwalk, and Act 1's
/// card comes up. Then the harbour's own scene takes over (the Red Gulls). It's steps on a clock,
/// not a general cutscene system; the scenes are data, in the map files. Nothing you press moves you
/// while the ship's coming in, but Escape skips it.
/// </remarks>
public partial class ExploreScreen
{
    private const string SkipHint = "ESC  ·  SKIP";
    private const string ActCard = "ACT 1  ·  THE HUNGER AT THE HEART";
    private const string ChapterCard = "Chapter 1  ·  Saltwind Welcome";
    private const string ControlsLine = "WASD or click to walk  ·  E to talk  ·  M for the map";

    /// <summary>The flag the ship coming in sets once you're off it.</summary>
    public const string Arrived = "arrived";

    /// <summary>Where what you're to do next goes, under the map's name.</summary>
    private const float ObjectiveY = 50f;

    /// <summary>How far east of where it ties up the ship starts, and how long it takes to sail in.</summary>
    private const float SailFrom = 560f;
    private const float SailSeconds = 7.5f;

    /// <summary>When, after it's in, the sail comes in and then the gangplank goes down, and when the Soul-Bound sets off down it.</summary>
    private const float FurlAt = 0.3f;
    private const float PlankAt = 0.9f;
    private const float OffAt = 1.6f;

    /// <summary>How far behind the Soul-Bound you set off, how fast you both walk off (art pixels a second), and how long the act's card is up.</summary>
    private const float FollowGap = 0.7f;
    private const float DisembarkSpeed = 34f;
    private const float CardSeconds = 4.2f;

    /// <summary>How much the ship rocks up and down, in art pixels, and how fast.</summary>
    private const float Bob = 1.2f;
    private const float BobRate = 1.8f;

    /// <summary>The black bars across the top and bottom while it plays, and how fast they come and go.</summary>
    private const int BarHeight = 70;
    private const float BarSeconds = 0.8f;

    /// <summary>The smudge behind the act's card, and how far it spreads.</summary>
    private static readonly Color CardBacking = Color.Black * 0.55f;
    private static readonly Vector2 CardSpread = new(90f, 20f);

    /// <summary>Where you and the Soul-Bound stand on the deck, from the ship's foot, and the way off: the foot of the gangplank, its top, and where each of you ends up on the boardwalk.</summary>
    private static readonly Vector2 YouOnDeck = new(-22f, -56f);
    private static readonly Vector2 SoulBoundOnDeck = new(-58f, -60f);
    private static readonly Vector2[] SoulBoundOff = { new(-40f, -62f), new(-40f, -102f), new(-56f, -112f) };
    private static readonly Vector2[] YouOff = { new(-40f, -62f), new(-40f, -100f), new(-22f, -110f) };

    private readonly InputAction _skip = new(new[] { Buttons.Start, Buttons.Back }, new[] { Keys.Escape }, true);

    /// <summary>Which part of the arrival is playing, or None when it isn't.</summary>
    private enum Arriving { None, Sailing, Docked, Disembarking, Card }

    private Arriving _arriving;
    private float _arrivalClock;
    private float _bars;
    private PropSprite _ship;
    private PropSprite _gangplank;
    private SpriteFont _cardFont;
    private bool _talking;

    /// <summary>Who's walking off the ship and the way they're going, a point at a time.</summary>
    private readonly List<(WalkerSprite Walker, Queue<Vector2> Way, float Wait)> _disembarking = new();

    /// <summary>Starts the ship coming in if this is the harbour, the Prologue's over, and you've not landed yet.</summary>
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
        _ship.Frame = 0;
        _gangplank.Hidden = true;
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

        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _arrivalClock += seconds;
        _ship.Offset = new Vector2(_ship.Offset.X, MathF.Round(MathF.Sin(_arrivalClock * BobRate) * Bob));

        switch (_arriving)
        {
            case Arriving.Sailing:
                float t = MathF.Min(1f, _arrivalClock / SailSeconds);
                float eased = 1f - (1f - t) * (1f - t) * (1f - t);
                _ship.Offset = new Vector2(MathF.Round(SailFrom * (1f - eased)), _ship.Offset.Y);
                PlaceOnDeck();
                if (t >= 1f) Next(Arriving.Docked);
                break;

            case Arriving.Docked:
                _ship.Offset = new Vector2(0f, _ship.Offset.Y);
                PlaceOnDeck();
                if (_arrivalClock >= FurlAt && _ship.Frame == 0)
                {
                    _ship.Frame = 1;
                    _audio.Play(Sfx.OptionChange);
                }
                if (_arrivalClock >= PlankAt && _gangplank.Hidden)
                {
                    _gangplank.Hidden = false;
                    _audio.Play(Sfx.Door);
                }
                if (_arrivalClock >= OffAt) StartDisembarking();
                break;

            case Arriving.Disembarking:
                _ship.Offset = Vector2.Zero;
                if (Disembark(gameTime))
                {
                    _walkers[1].Face(_walkers[0].Position);
                    _walkers[0].Face(_walkers[1].Position);
                    Next(Arriving.Card);
                }
                break;

            case Arriving.Card:
                _ship.Offset = Vector2.Zero;
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

    /// <summary>The middle of the ship's deck, which the camera keeps on while it sails in.</summary>
    private Vector2 ShipView() => _ship.Foot + _ship.Offset + new Vector2(-30f, -70f);

    /// <summary>Stands you and the Soul-Bound on the deck, wherever the ship's got to.</summary>
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

    /// <summary>Ends the ship coming in, whether it played out or got skipped: tied up, you both on the boardwalk, and the harbour's scene next.</summary>
    private void EndArrival()
    {
        _ship.Offset = Vector2.Zero;
        _ship.Frame = 1;
        _gangplank.Hidden = false;
        _walkers[1].Position = _ship.Foot + SoulBoundOff[^1];
        _walkers[0].Position = _ship.Foot + YouOff[^1];
        foreach (WalkerSprite walker in _walkers) walker.IsWalking = false;
        _walkers[0].Facing = Direction.Up;
        _walkers[1].Facing = Direction.Up;
        _arriving = Arriving.None;

        _game.Flags.Add(Arrived);
        SelectLeader(0);
        SeedTrail();
        RefreshPresence();
        _toast.Show(ControlsLine);
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

    /// <summary>Starts a conversation over the map. When it's over, whatever's to happen next does, and if it sends the party somewhere, it goes.</summary>
    /// <param name="script">What gets said.</param>
    /// <param name="then">What happens after, or null.</param>
    private void Converse(DialogueScript script, Action then)
    {
        if (script is null || _talking) return;

        Halt();
        _talking = true;
        var talk = new DialogueScreen(script, _game, Speaker, _random);
        talk.Happened += OnDialogue;
        talk.Finished += travel =>
        {
            _talking = false;
            then?.Invoke();
            if (travel is null) return;

            string[] where = travel.Split(' ');
            MapDefinition to = MapCatalog.Get(where[0]);
            MapSpawn spawn = to.Spawns[where[1]];
            Leave(to, spawn.Feet, spawn.Facing);
        };
        ScreenManager.AddScreen(talk);
    }

    /// <summary>Catches the map up with something a conversation did: people coming and going with the flags, and someone joining the party.</summary>
    private void OnDialogue(DialogueEvent happened)
    {
        switch (happened.Kind)
        {
            case DialogueEventKind.Flag:
                RefreshPresence();
                break;

            case DialogueEventKind.Recruited when happened.Amount == 1:
                var walker = new WalkerSprite { Position = Leader.Position, Facing = Leader.Facing };
                DressWalker(walker, _game.Member(happened.Id));
                _walkers.Add(walker);
                GatherBehind(new List<WalkerSprite> { walker });
                SeedTrail();
                break;
        }
    }

    /// <summary>Who a speaker id in a conversation is: their name and the sheet their face comes off.</summary>
    /// <remarks>The party first, then whoever's on the map, then the story's cast wherever they are.</remarks>
    private (string Name, Texture2D Sheet) Speaker(string id)
    {
        for (int i = 0; i < _game.Party.Count; i++)
            if (_game.Party[i].Id == id) return (_game.Party[i].Name, _walkers[i].Sheet);
        foreach (Npc npc in _everyNpc)
            if (npc.Definition.Id == id || npc.Definition.Cast == id) return (npc.Definition.Name, npc.Walker.Sheet);
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

    /// <summary>Whether a townsperson's feet would bump into anything at a point: a wall, a prop, someone else, or the party.</summary>
    private bool Crowded(Npc mover, Vector2 at)
    {
        if (!_tilemap.Bounds.Contains(at)) return true;

        BoundingRectangle feet = FeetAt(at);
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
        float y = LegendQuestGame.ScreenHeight / 2f - 80f;
        Vector2 act = _detailFont.MeasureString(ChapterCard);
        var area = new Rectangle((int)((LegendQuestGame.ScreenWidth - act.X * 2f) / 2f), (int)y, (int)(act.X * 2f), 140);
        Text.DrawBacking(spriteBatch, _glow, area, (int)CardSpread.X, CardBacking * shown);
        Text.DrawCentered(spriteBatch, _detailFont, ActCard, y, Palette.Gold * shown, Palette.Shadow * shown);
        Text.DrawCentered(spriteBatch, _cardFont, ChapterCard, y + _detailFont.LineSpacing + 10f, Palette.Bone * shown, Palette.Shadow * shown);
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
