using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Collisions;
using LegendQuest.Components;
using LegendQuest.Dialogue;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen as a conversation's stage: the stage cues done to the tile map and everyone on it,
/// as a CutsceneDirector asks.
/// </summary>
/// <remarks>
/// <para>
/// Who an actor id names is ICutsceneStage's rule, looked for in that order: the party (party,
/// both, you, soulbound, a companion by their origin id), then whoever's on the map by their id,
/// then a group, which is everyone standing as that one of the cast and every copy numbered after
/// it (guard-1, guard-2), and then the same for the id without an s on the end, so royal-guards is
/// every royal guard. Someone a place or an echo names who isn't here, but is one of the cast or the
/// creatures, is brought on, dressed off their Cast entry or their creature's sheet, and stays for as
/// long as the map's up, like anyone else put down on it.
/// </para>
/// <para>
/// A spawn's spot is exactly where the map has it, turned the way it faces unless the cue says. The
/// other spots look for somewhere the feet fit, nearest first: beside is a party's spacing to one
/// side of whoever it is, before a tile in front of them (down the map), over a tile behind them,
/// on right where they stand, and the crowd the nearest free spot to you. A group or copies are
/// spread a spacing apart round the spot. Walking goes along the A* paths the mouse uses, straight
/// through anyone in the way, at a walk; a spot no path reaches is jumped to. A clip strikes the
/// Legacy pose LegacyClips gives it, held through its line; downed is held till a clip gets them
/// up. A glow is the glow texture over them, light added; an echo is them at half strength; a tint
/// fades the map's own tint to the cue's; a shake is ScreenShake over the world; the sigil goes up on
/// the corner of the party's cards. Show and hide go over what the flags say for as long as the
/// map's up. A travel-actor to another map can't happen with the scene still on this one, so the
/// party (you) leaves when the conversation's over, the way a Travel effect goes, and anyone else is
/// gone from here for the rest of the scene.
/// </para>
/// </remarks>
public partial class ExploreScreen : ICutsceneStage
{
    /// <summary>How near the camera has to be to where a camera cue sent it to be there, in world pixels.</summary>
    private const float CameraNear = 1f;

    /// <summary>How many tiles round a spot to look for somewhere free to stand.</summary>
    private const int SpotSearch = 120;

    /// <summary>How strong an echo is drawn: build plan §5.3's 50%.</summary>
    private const float EchoOpacity = 0.5f;

    /// <summary>A glow: how much wider and taller than the figure, how strong at the bottom and top of its breath, and how fast it breathes, in radians a second.</summary>
    private const float GlowWidth = 2.6f;
    private const float GlowHeight = 1.5f;
    private const float GlowLeast = 0.45f;
    private const float GlowMost = 0.65f;
    private const float GlowRate = 3f;

    /// <summary>Who's walking by a cue, along which path, and which point of it they're heading for.</summary>
    private readonly Dictionary<WalkerSprite, (List<Vector2> Path, int Step)> _cueWalks = new();

    /// <summary>Who's striking a clip's pose for their line, and who's down till a clip gets them up.</summary>
    private readonly HashSet<WalkerSprite> _gestures = new();
    private readonly HashSet<WalkerSprite> _downed = new();

    /// <summary>Who's glowing and in what, and who's an echo.</summary>
    private readonly Dictionary<WalkerSprite, Color> _glows = new();
    private readonly HashSet<WalkerSprite> _echoes = new();

    /// <summary>Everyone the cues have brought on, and the ones an echo brought on, who go when the scene does.</summary>
    private readonly List<Npc> _brought = new();
    private readonly List<Npc> _remembered = new();

    /// <summary>What show and hide have put over the flags, by id on the map, and the party's figures hidden for the scene.</summary>
    private readonly HashSet<string> _cueShown = new();
    private readonly HashSet<string> _cueHidden = new();
    private readonly HashSet<WalkerSprite> _hiddenWalkers = new();

    /// <summary>The shake over the world a shake cue starts.</summary>
    private readonly ScreenShake _cueShake = new();

    /// <summary>Where a camera cue's sent the camera, or null to follow you as usual.</summary>
    private Vector2? _cueCamera;

    /// <summary>The tint the map's going from and to, how far it's got and how long it takes; and the glows' clock.</summary>
    private Color _tintFrom;
    private Color _tintTo;
    private float _tintClock;
    private float _tintLength;
    private float _glowClock;

    /// <summary>Whether the sigil's up on the corner of the cards for the scene.</summary>
    private bool _sigilUp;

    /// <summary>Whether a cue's put any of the party somewhere, so the followers' trail starts again from where they are once it's over.</summary>
    private bool _partyStaged;

    /// <summary>Where a travel-actor sent the party, as "map spawn", for when the conversation's over; or null.</summary>
    private string _cueTravel;

    /// <summary>The colour of the light on the map now: its own tint, or a tint cue's, or on the way between them.</summary>
    private Color WorldTint => Color.Lerp(_tintFrom, _tintTo, _tintLength <= 0f ? 1f : MathHelper.Clamp(_tintClock / _tintLength, 0f, 1f));

    /// <inheritdoc/>
    public IReadOnlyList<string> Actors(string id)
    {
        var keys = new List<string>();
        foreach ((string key, _, _) in Figures(id)) keys.Add(key);
        return keys;
    }

    /// <inheritdoc/>
    public bool TryGetSpawn(string name, out Vector2 at)
    {
        bool found = _map.Spawns.TryGetValue(name ?? string.Empty, out MapSpawn spawn);
        at = found ? spawn.Feet : Vector2.Zero;
        return found;
    }

    /// <inheritdoc/>
    public bool TryGetSpot(CueSpot spot, out Vector2 at)
    {
        at = Vector2.Zero;
        switch (spot.Kind)
        {
            case CueSpotKind.Spawn:
                return TryGetSpawn(spot.Name, out at);

            case CueSpotKind.Crowd:
                at = FreeNear(Leader.Position, _walkers.ConvertAll(walker => walker.Position));
                return true;

            case CueSpotKind.Beside:
                if (Figures(spot.Name) is not [var beside, ..]) return false;
                Vector2 by = beside.Walker.Position;
                foreach (float side in new[] { Spacing, -Spacing })
                {
                    if (!Standable(by + new Vector2(side, 0f))) continue;
                    at = by + new Vector2(side, 0f);
                    return true;
                }
                at = FreeNear(by, new List<Vector2> { by });
                return true;

            default:
                if (!TryGetFoot(spot.Name, out Vector2 foot)) return false;
                float ahead = spot.Kind switch { CueSpotKind.Before => Tilemap.TileSize, CueSpotKind.Over => -Tilemap.TileSize, _ => 0f };
                at = ahead == 0f ? foot : FreeNear(foot + new Vector2(0f, ahead), new List<Vector2>());
                return true;
        }
    }

    /// <inheritdoc/>
    public bool Place(string actor, CueSpot spot, CueFacing? facing, int copies)
    {
        if (copies > 0) return PlaceCopies(actor, spot, facing, copies);

        var figures = Figures(actor);
        if (figures.Count == 0 && Bring(actor, actor) is not null) figures = Figures(actor);
        if (figures.Count == 0 || !TryGetSpot(spot, out Vector2 at)) return false;

        Direction? way = facing is CueFacing turned ? Turned(turned)
            : spot.Kind == CueSpotKind.Spawn ? _map.Spawns[spot.Name].Facing
            : spot.Kind == CueSpotKind.Beside ? Figures(spot.Name)[0].Walker.Facing
            : null;
        List<Vector2> spots = Round(at, figures.Count);
        for (int i = 0; i < figures.Count; i++) Put(figures[i].Walker, spots[i], way);
        BuildNav();
        return true;
    }

    /// <inheritdoc/>
    public void Walk(string actor, Vector2 to)
    {
        var figures = Figures(actor);
        List<Vector2> spots = Round(to, figures.Count);
        for (int i = 0; i < figures.Count; i++)
        {
            WalkerSprite walker = figures[i].Walker;
            List<Vector2> path = Pathfinder.Find(_nav, walker.Position, spots[i], 0f);
            if (path is not null && path[^1] != spots[i]) path.Add(spots[i]);
            path ??= Pathfinder.Find(_nav, walker.Position, spots[i], Tilemap.TileSize);

            if (path is null)
            {
                Put(walker, spots[i], null);
                continue;
            }
            _cueWalks[walker] = (path, 0);
            if (_walkers.Contains(walker)) _partyStaged = true;
        }
    }

    /// <inheritdoc/>
    public bool IsWalking(string key) => Figures(key) is [var figure, ..] && _cueWalks.ContainsKey(figure.Walker);

    /// <inheritdoc/>
    public void Face(string actor, CueFacing way)
    {
        foreach ((_, WalkerSprite walker, _) in Figures(actor)) walker.Facing = Turned(way);
    }

    /// <inheritdoc/>
    public void FaceToward(string actor, string other)
    {
        if (Figures(other) is not [var them, ..]) return;
        foreach ((_, WalkerSprite walker, _) in Figures(actor))
            if (walker != them.Walker) walker.Face(them.Walker.Position);
    }

    /// <inheritdoc/>
    public float Animate(string actor, string clip, string partner)
    {
        if (!LegacyClips.TryGet(clip, out WalkerPose pose)) return 0f;

        Vector2? toward = partner is not null && Figures(partner) is [var them, ..] ? them.Walker.Position : null;
        foreach ((_, WalkerSprite walker, _) in Figures(actor))
        {
            _gestures.Remove(walker);
            _downed.Remove(walker);
            if (pose == WalkerPose.None)
            {
                walker.Release();
                continue;
            }

            if (toward is Vector2 at && at != walker.Position) walker.Face(at);
            walker.Play(pose);
            if (pose == WalkerPose.Downed) _downed.Add(walker);
            else _gestures.Add(walker);
        }
        return pose == WalkerPose.None ? 0f : LegacyClips.Seconds;
    }

    /// <inheritdoc/>
    public void EndGestures()
    {
        foreach (WalkerSprite walker in _gestures) walker.Release();
        _gestures.Clear();
    }

    /// <inheritdoc/>
    public void Glow(string actor, Color? colour)
    {
        foreach ((_, WalkerSprite walker, _) in Figures(actor))
        {
            if (colour is Color glow) _glows[walker] = glow;
            else _glows.Remove(walker);
        }
    }

    /// <inheritdoc/>
    public void Echo(string actor)
    {
        foreach ((_, WalkerSprite walker, Npc npc) in Figures(actor))
        {
            walker.Opacity = EchoOpacity;
            _echoes.Add(walker);
            if (npc is not null && _brought.Contains(npc) && !_remembered.Contains(npc)) _remembered.Add(npc);
        }
    }

    /// <inheritdoc/>
    public bool Show(string thing, Vector2? at)
    {
        if (OnMap(thing))
        {
            _cueHidden.Remove(thing);
            _cueShown.Add(thing);
            RefreshPresence();
        }
        else if (Figures(thing).Count == 0)
        {
            return false;
        }

        foreach ((_, WalkerSprite walker, _) in Figures(thing))
        {
            _hiddenWalkers.Remove(walker);
            if (at is Vector2 spot) Put(walker, spot, null);
        }
        return true;
    }

    /// <inheritdoc/>
    public bool Hide(string thing)
    {
        var figures = Figures(thing);
        if (OnMap(thing))
        {
            _cueShown.Remove(thing);
            _cueHidden.Add(thing);
            RefreshPresence();
            return true;
        }

        foreach ((_, WalkerSprite walker, Npc npc) in figures)
        {
            if (npc is null) _hiddenWalkers.Add(walker);
            else _cueHidden.Add(npc.Definition.Id);
        }
        RefreshPresence();
        return figures.Count > 0;
    }

    /// <inheritdoc/>
    public bool Swap(string actor, string look)
    {
        var figures = Figures(actor);
        foreach ((string key, WalkerSprite walker, Npc npc) in figures)
        {
            string castId = npc?.Definition.Cast ?? key;
            if (Cast.Get(castId)?.LookCalled(look) is not CharacterLook changed) return false;
            walker.Sheet = SheetFor(changed);
        }
        return figures.Count > 0;
    }

    /// <inheritdoc/>
    public void Tint(Color? colour, float seconds)
    {
        _tintFrom = WorldTint;
        _tintTo = colour ?? _map.Tint;
        _tintClock = 0f;
        _tintLength = seconds;
    }

    /// <inheritdoc/>
    public void Camera(Vector2 at) => _cueCamera = at;

    /// <inheritdoc/>
    public bool CameraMoving => _cueCamera is Vector2 at && !_camera.IsNear(at, CameraNear);

    /// <inheritdoc/>
    public void Shake(float strength) =>
        _cueShake.Kick(strength / StageDirection.MostShake, new Vector2(LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight) / 2f);

    /// <inheritdoc/>
    public void Sigil(SigilState state, bool oneFrame)
    {
        LoadCombat();
        _sigil.Mark(state, oneFrame);
        _sigilUp = true;
    }

    /// <inheritdoc/>
    public void TravelActor(string actor, string mapOrZone, string spawn)
    {
        if (mapOrZone == _map.Id)
        {
            Place(actor, CueSpot.At(spawn), null, 0);
            return;
        }

        if (actor is Origins.You or StageDirection.Party)
        {
            _cueTravel = mapOrZone + " " + spawn;
            _hiddenWalkers.UnionWith(_walkers);
            return;
        }
        Hide(actor);
    }

    /// <inheritdoc/>
    public void Advance(float seconds)
    {
        var time = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));
        _glowClock += seconds;
        _tintClock += seconds;
        _cueShake.Update(time);
        if (_sigilUp) _sigil.Update(time);
        if (_cueCamera is Vector2 look) _camera.Follow(look, time);

        bool moved = false;
        foreach (WalkerSprite walker in new List<WalkerSprite>(_cueWalks.Keys))
        {
            (List<Vector2> path, int step) = _cueWalks[walker];
            float left = WalkSpeed * seconds;
            while (step < path.Count && left > 0f)
            {
                Vector2 gap = path[step] - walker.Position;
                walker.Face(path[step]);
                if (gap.Length() <= left)
                {
                    walker.Position = path[step];
                    left -= gap.Length();
                    step++;
                }
                else
                {
                    walker.Position += Vector2.Normalize(gap) * left;
                    left = 0f;
                }
            }

            walker.IsWalking = step < path.Count;
            walker.Update(time);
            if (walker.IsWalking)
            {
                _cueWalks[walker] = (path, step);
                continue;
            }
            _cueWalks.Remove(walker);
            moved = true;
        }
        if (moved) BuildNav();
    }

    /// <inheritdoc/>
    public void Finish()
    {
        foreach ((WalkerSprite walker, (List<Vector2> path, _)) in _cueWalks)
        {
            walker.Face(path[^1]);
            walker.Position = path[^1];
            walker.IsWalking = false;
        }
        if (_cueWalks.Count > 0) BuildNav();
        _cueWalks.Clear();

        if (_cueCamera is Vector2 look) _camera.SnapTo(look);
        _tintClock = _tintLength;
    }

    /// <inheritdoc/>
    public void Wrap()
    {
        Finish();
        foreach (WalkerSprite walker in _gestures) walker.Release();
        foreach (WalkerSprite walker in _downed) walker.Release();
        _gestures.Clear();
        _downed.Clear();
        _glows.Clear();

        foreach (WalkerSprite walker in _echoes) walker.Opacity = 1f;
        _echoes.Clear();
        foreach (Npc memory in _remembered)
        {
            _everyNpc.Remove(memory);
            _brought.Remove(memory);
        }
        _remembered.Clear();

        _hiddenWalkers.Clear();
        _cueCamera = null;
        _sigilUp = false;
        _sigil.Unmark();
        RefreshPresence();

        if (!_partyStaged) return;
        _partyStaged = false;
        SeedTrail();
    }

    /// <summary>Lets a shake or a tint a scene started settle on the map's own clock, once the scene that started it is over.</summary>
    private void SettleStage(GameTime gameTime)
    {
        _cueShake.Update(gameTime);
        _tintClock += (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    /// <summary>Starts the map's light as its own tint. Called once, as the map's put together.</summary>
    private void SetUpStage()
    {
        _tintFrom = _map.Tint;
        _tintTo = _map.Tint;
    }

    /// <summary>Whether something's on the map by its id, there now or not: someone it puts down or a cue's brought on, or one of the things on it. What show and hide go over the flags for.</summary>
    private bool OnMap(string id) => _everyNpc.Exists(npc => npc.Definition.Id == id) || _everyThing.Exists(each => each.Thing.Id == id);

    /// <summary>Whether something on the map's there now: what show and hide said, or else what the flags say.</summary>
    private bool Present(string id, MapPresence presence) => !_cueHidden.Contains(id) && (_cueShown.Contains(id) || presence.Holds(_game.Flags));

    /// <summary>Everyone an actor id names who's on the stage now, each with their key, their figure, and who they are if they're on the map rather than in the party.</summary>
    private List<(string Key, WalkerSprite Walker, Npc Npc)> Figures(string id)
    {
        var found = new List<(string Key, WalkerSprite Walker, Npc Npc)>();
        if (id is null) return found;

        if (id == StageDirection.Party || id == StageDirection.Both)
        {
            for (int i = 0; i < _walkers.Count; i++)
                if (id == StageDirection.Party || _members[i].Id is Origins.You or Origins.SoulBound) found.Add((_members[i].Id, _walkers[i], null));
            return found;
        }

        int place = _members.FindIndex(member => member.Id == id);
        if (place >= 0)
        {
            found.Add((id, _walkers[place], null));
            return found;
        }

        if (_npcs.Find(npc => npc.Definition.Id == id) is Npc one)
        {
            found.Add((id, one.Walker, one));
            return found;
        }

        AddGroup(id, found);
        if (found.Count == 0 && id.EndsWith('s')) AddGroup(id[..^1], found);
        return found;
    }

    /// <summary>Adds everyone on the map who's in a group: standing as that one of the cast, or numbered after it.</summary>
    private void AddGroup(string id, List<(string Key, WalkerSprite Walker, Npc Npc)> found)
    {
        foreach (Npc npc in _npcs)
        {
            string name = npc.Definition.Id;
            bool numbered = name.Length > id.Length + 1 && name.StartsWith(id + "-", StringComparison.Ordinal)
                && int.TryParse(name[(id.Length + 1)..], out _);
            if (npc.Definition.Cast == id || numbered) found.Add((name, npc.Walker, npc));
        }
    }

    /// <summary>Makes copies of someone round a spot, numbered from 1 after their id, bringing on any that aren't here yet.</summary>
    private bool PlaceCopies(string actor, CueSpot spot, CueFacing? facing, int copies)
    {
        if (!TryGetSpot(spot, out Vector2 at)) return false;

        List<Vector2> spots = Round(at, copies);
        Direction? way = facing is CueFacing turned ? Turned(turned) : spot.Kind == CueSpotKind.Spawn ? _map.Spawns[spot.Name].Facing : null;
        for (int i = 0; i < copies; i++)
        {
            string id = actor + "-" + (i + 1);
            WalkerSprite walker = _npcs.Find(npc => npc.Definition.Id == id)?.Walker ?? Bring(id, actor)?.Walker;
            if (walker is null) return false;
            Put(walker, spots[i], way);
        }
        BuildNav();
        return true;
    }

    /// <summary>Brings someone on who isn't here: one of the cast, dressed off their entry, or one of the creatures. They stand where you are till they're put somewhere.</summary>
    /// <param name="id">What they'll be called on the stage.</param>
    /// <param name="who">Who they are: a cast id or a creature's.</param>
    /// <returns>Them, or null if who is neither.</returns>
    private Npc Bring(string id, string who)
    {
        CastMember cast = Cast.Get(who) is CastMember member && (member.Look is not null || member.Sheet is not null) ? member : null;
        Creature creature = cast is null ? Creatures.Get(who) : null;
        if (cast is null && creature is null) return null;

        var definition = new MapNpc(id, cast?.Name ?? creature.Name, Leader.Position, Direction.Down, Sheet: null, Cast: cast?.Id, Look: null,
            Creature: creature?.Id, Say: Array.Empty<string>(), Talk: null, Wander: 0f, Presence: MapPresence.Always);
        var npc = new Npc(definition);
        npc.Walker.LoadContent(_content);
        npc.Walker.Sheet = NpcSheet(definition);
        _everyNpc.Add(npc);
        _brought.Add(npc);
        RefreshPresence();
        return npc;
    }

    /// <summary>Puts someone at a spot, turned a way if there is one, and stops them where they are.</summary>
    private void Put(WalkerSprite walker, Vector2 at, Direction? way)
    {
        _cueWalks.Remove(walker);
        walker.Position = at;
        walker.IsWalking = false;
        if (way is Direction facing) walker.Facing = facing;
        if (_walkers.Contains(walker)) _partyStaged = true;
    }

    /// <summary>Where a prop, a thing on the map or someone stands: someone first, then the thing by its id, then the nearest prop to you of that kind.</summary>
    private bool TryGetFoot(string id, out Vector2 foot)
    {
        foot = Vector2.Zero;
        if (Figures(id) is [var someone, ..])
        {
            foot = someone.Walker.Position;
            return true;
        }
        foreach ((MapInteractable thing, _) in _things)
        {
            if (thing.Id != id) continue;
            foot = thing.Foot;
            return true;
        }

        bool found = false;
        foreach (PropSprite prop in _props)
        {
            if (prop.Definition.Id != id || found && Vector2.Distance(prop.Foot, Leader.Position) >= Vector2.Distance(foot, Leader.Position)) continue;
            foot = prop.Foot;
            found = true;
        }
        return found;
    }

    /// <summary>Spots for so many people round a point: the point itself for the first, then the nearest free spots a party's spacing apart.</summary>
    private List<Vector2> Round(Vector2 at, int count)
    {
        var spots = new List<Vector2> { at };
        while (spots.Count < count) spots.Add(FreeNear(at, spots));
        return spots;
    }

    /// <summary>The nearest spot to a point where feet fit, a party's spacing from every spot that's taken, looking out a tile at a time and never through a wall; the point itself if there's nowhere.</summary>
    /// <param name="near">The point, in world pixels.</param>
    /// <param name="taken">Spots that are spoken for.</param>
    private Vector2 FreeNear(Vector2 near, List<Vector2> taken)
    {
        bool Free(Vector2 spot) => Standable(spot) && taken.TrueForAll(other => Vector2.Distance(other, spot) >= Spacing);
        if (Free(near)) return near;

        Point start = Tilemap.CellOf(near);
        var seen = new HashSet<Point> { start };
        var queue = new Queue<Point>();
        queue.Enqueue(start);
        for (int looked = 0; queue.Count > 0 && looked < SpotSearch; looked++)
        {
            Point cell = queue.Dequeue();
            if (cell != start && Free(NavGrid.Middle(cell))) return NavGrid.Middle(cell);

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    var next = new Point(cell.X + dx, cell.Y + dy);
                    if (seen.Add(next) && _tilemap.Bounds.Contains(NavGrid.Middle(next)) && !_tilemap.IsSolid(next.X, next.Y)) queue.Enqueue(next);
                }
            }
        }
        return near;
    }

    /// <summary>Whether someone's feet would fit at a point: on the map, clear of the walls, the props, the shut doors, everyone on the map and the party.</summary>
    private bool Standable(Vector2 feet)
    {
        if (!_tilemap.Bounds.Contains(feet)) return false;

        BoundingRectangle box = FeetAt(feet);
        foreach (BoundingRectangle wall in _tilemap.Walls)
            if (box.CollidesWith(wall)) return false;
        foreach (PropSprite prop in _props)
            foreach (BoundingRectangle footprint in prop.Footprints)
                if (box.CollidesWith(footprint)) return false;
        foreach ((MapInteractable thing, PropSprite sprite) in _things)
        {
            if (thing.Kind == InteractableKind.Door && _world.IsOpen(thing)) continue;
            foreach (BoundingRectangle footprint in sprite.Footprints)
                if (box.CollidesWith(footprint)) return false;
        }
        foreach (Npc npc in _npcs)
            if (box.CollidesWith(npc.Footprint)) return false;
        foreach (WalkerSprite walker in _walkers)
            if (box.CollidesWith(Grown(FeetAt(walker.Position), 2f))) return false;
        return true;
    }

    /// <summary>A cue's facing as a sheet's row.</summary>
    private static Direction Turned(CueFacing way) => way == CueFacing.Left ? Direction.Left : Direction.Right;

    /// <summary>The glows round whoever a light cue's lit, breathing. Goes in the additive batch, in world space.</summary>
    private void DrawGlows(SpriteBatch spriteBatch)
    {
        float breathe = MathHelper.Lerp(GlowLeast, GlowMost, 0.5f + 0.5f * MathF.Sin(_glowClock * GlowRate));
        foreach ((WalkerSprite walker, Color colour) in _glows)
        {
            if (_hiddenWalkers.Contains(walker)) continue;
            Vector2 figure = walker.Shape.Figure;
            var size = new Vector2(figure.X * GlowWidth, figure.Y * GlowHeight);
            Vector2 middle = walker.Position - new Vector2(0f, figure.Y / 2f);
            var area = new Rectangle((int)(middle.X - size.X / 2f), (int)(middle.Y - size.Y / 2f), (int)size.X, (int)size.Y);
            spriteBatch.Draw(_glow, area, colour * (breathe * walker.Opacity));
        }
    }

    /// <summary>The sigil on the corner of the party's cards, while a scene has it up. Goes in a point sampled batch on the screen.</summary>
    private void DrawSigil(SpriteBatch spriteBatch, float alpha)
    {
        if (_sigilUp && _sigil.Marked) _sigil.DrawMark(spriteBatch, PartyStrip.Corner(SoulLinkSigil.Size), alpha);
    }
}
