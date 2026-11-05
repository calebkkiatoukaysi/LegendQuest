using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Audio;
using LegendQuest.Dialogue;
using LegendQuest.Save;
using LegendQuest.Story;

namespace LegendQuest.Screens;

/// <summary>
/// Plays a line's stage cues before the line shows (build plan §5): who's put where and who walks
/// where, the clips, the camera, the fades, the sounds.
/// </summary>
/// <remarks>
/// <para>
/// The timing is §5.2's. Everyone on the stage has a queue of their own: a cue waits for that
/// person's last one to finish (only walking and a clip take any time), and cues on different
/// people go together, so anim soulbound strike and anim crane die play at once. A cue on nobody in
/// particular (a sound, a tint, a shake) goes the moment it comes. Fade, wait, backdrop, camera,
/// travel-actor and slow are barriers: they wait for everything before them, and everything after
/// waits for them. The line shows once nothing's left. Skip puts every cue still to come at its end
/// at once, without its sound. A cue line's {if …} is looked at as the line comes up, and a cue
/// whose conditions don't hold is left out.
/// </para>
/// <para>
/// It keeps the clock and the order, and the world's left to an ICutsceneStage, so nothing here
/// knows about tiles. The fade, the sounds and the music are its own (the dialogue screen draws
/// the fade, over the painting and under the panel), and so is the slow, which slows its clock and
/// the stage's together. Hold and drop go in the save's sockets. With no stage (the creation
/// screen's scripts, which aren't on a map) a cue on the world does nothing, quietly; the
/// fades, waits, sounds and backdrops still play. A cue that can't play (its words wrong, or
/// someone or somewhere it names not here) goes in the error log and is skipped; the start-up check
/// means that's only ever someone the flags have taken off the map.
/// </para>
/// </remarks>
public sealed class CutsceneDirector
{
    /// <summary>How long a tint takes to come over, in seconds (03b-world-art.md §5).</summary>
    private const float TintSeconds = 0.6f;

    private readonly ICutsceneStage _stage;
    private readonly SaveData _game;
    private readonly AudioManager _audio;
    private readonly string _script;

    /// <summary>The cues still to come, in order.</summary>
    private readonly Queue<StageDirection> _pending = new();

    /// <summary>The cues waiting for someone's last cue to finish, in order, with who they're on.</summary>
    private readonly List<(StageDirection Cue, IReadOnlyList<string> Keys)> _waiting = new();

    /// <summary>Everyone striking a clip's pose, and how much longer before their next cue can go.</summary>
    private readonly Dictionary<string, float> _posing = new();

    /// <summary>Everyone a walk's sent somewhere who isn't there yet, and who to face when they are, if anyone.</summary>
    private readonly HashSet<string> _walking = new();
    private readonly Dictionary<string, string> _faceAfter = new();

    /// <summary>The barrier going on, if one is, and how long it's been going.</summary>
    private StageDirection _barrier;
    private float _barrierClock;

    /// <summary>Where a fade starts from and goes to: 0 is clear and 1 black.</summary>
    private float _fadeFrom;
    private float _fadeTo;

    /// <summary>How fast the scene's clock runs: 1, or what a slow cue says.</summary>
    private float _speed = 1f;

    /// <summary>Whether it's putting everything at its end for a skip, which plays no sounds.</summary>
    private bool _skipping;

    /// <summary>Which line the cues are for, for the log.</summary>
    private string _where;

    /// <summary>Gets a conversation's cues ready to play.</summary>
    /// <param name="stage">Where they play, or null for a conversation that isn't over a map.</param>
    /// <param name="game">The game, for the cues' conditions and the sockets.</param>
    /// <param name="audio">Where the sounds and the music go.</param>
    /// <param name="script">The script's id, for the log.</param>
    public CutsceneDirector(ICutsceneStage stage, SaveData game, AudioManager audio, string script)
    {
        _stage = stage;
        _game = game;
        _audio = audio;
        _script = script;
    }

    /// <summary>Raised when a backdrop cue comes: whatever painting the line has goes up behind the scene now, not when the line shows.</summary>
    /// <remarks>The script compiler has already put the backdrop's painting on the line (Painting), if it's one there is, so that's what goes up.</remarks>
    public event Action Backdrop;

    /// <summary>How far faded to black the screen is, 0 to 1, from the fade cues.</summary>
    public float Blackout { get; private set; }

    /// <summary>Whether any cue is still to come or still going. The line waits for this to be false.</summary>
    public bool Playing => _pending.Count > 0 || _waiting.Count > 0 || _barrier is not null || _posing.Count > 0 || _walking.Count > 0;

    /// <summary>Starts a line's cues, or the closing's, after letting go of the poses the last line struck.</summary>
    /// <param name="cues">The cues, in order.</param>
    /// <param name="where">Which line they're for, for the log: its id, or where it is in the script.</param>
    public void Play(IReadOnlyList<StageCue> cues, string where)
    {
        _where = where;
        _stage?.EndGestures();
        foreach (StageCue cue in cues)
        {
            if (!StageDirection.TryRead(cue, out StageDirection direction, out string problem)) Log(cue, problem);
            else if (cue.Conditions.All(condition => condition.Holds(_game))) _pending.Enqueue(direction);
        }
        Pump();
    }

    /// <summary>Runs the clock a frame on: the stage, the fade, the poses, the walking, then whatever can start.</summary>
    /// <param name="seconds">How long the frame was.</param>
    public void Update(float seconds)
    {
        float time = seconds * _speed;
        _stage?.Advance(time);

        foreach (string key in _posing.Keys.ToList())
        {
            _posing[key] -= time;
            if (_posing[key] <= 0f) _posing.Remove(key);
        }
        foreach (string key in _walking.ToList())
            if (!_stage.IsWalking(key)) Arrived(key);

        if (_barrier is not null)
        {
            _barrierClock += time;
            if (_barrier.Verb == "fade")
                Blackout = MathHelper.Lerp(_fadeFrom, _fadeTo, _barrier.Number <= 0f ? 1f : MathHelper.Clamp(_barrierClock / _barrier.Number, 0f, 1f));
        }
        Pump();
    }

    /// <summary>Puts every cue still to come or still going at its end, now: everyone where they were going, the fade done, the camera there. Sounds still to come don't play.</summary>
    public void Skip()
    {
        if (!Playing) return;
        _skipping = true;
        Settle();
        Pump();
        _skipping = false;
    }

    /// <summary>The conversation's over: the stage lets go of what the scene put up for itself, and time goes back to its own speed.</summary>
    public void End()
    {
        _stage?.Wrap();
        _speed = 1f;
    }

    /// <summary>Starts whatever can start: waiting cues whose people are free, then the next cues in order, up to a barrier that has to wait or one that's going.</summary>
    private void Pump()
    {
        while (true)
        {
            if (StartWaiting()) continue;

            if (_barrier is not null)
            {
                if (!_skipping && !BarrierDone()) return;
                if (_barrier.Verb == "fade") Blackout = _fadeTo;
                _barrier = null;
            }

            if (_pending.Count == 0) return;

            StageDirection next = _pending.Peek();
            if (next.IsBarrier)
            {
                if (Busy()) return;
                _pending.Dequeue();
                _barrier = next;
                _barrierClock = 0f;
                Start(next, Keys(next));
                continue;
            }

            _pending.Dequeue();
            IReadOnlyList<string> keys = Keys(next);
            if (keys.Any(key => Busy(key) || _waiting.Exists(waiting => waiting.Keys.Contains(key)))) _waiting.Add((next, keys));
            else Start(next, keys);
        }
    }

    /// <summary>Starts the first waiting cue whose people are free and that no earlier waiting cue is ahead of.</summary>
    /// <returns>True if one started.</returns>
    private bool StartWaiting()
    {
        for (int i = 0; i < _waiting.Count; i++)
        {
            (StageDirection cue, IReadOnlyList<string> keys) = _waiting[i];
            if (keys.Any(Busy) || _waiting.Take(i).Any(earlier => earlier.Keys.Intersect(keys).Any())) continue;

            _waiting.RemoveAt(i);
            Start(cue, keys);
            return true;
        }
        return false;
    }

    /// <summary>Whether the barrier that's going is done: a fade or a wait when its seconds are up, the camera when it's there, the rest at once.</summary>
    private bool BarrierDone() => _barrier.Verb switch
    {
        "fade" or "wait" => _barrierClock >= _barrier.Number,
        "camera" => _stage is null || !_stage.CameraMoving,
        _ => true,
    };

    /// <summary>Whether anyone's still busy with a cue, or a cue's waiting on them.</summary>
    private bool Busy() => _waiting.Count > 0 || _posing.Count > 0 || _walking.Count > 0;

    /// <summary>Whether someone's still busy with a cue: walking, or striking a pose.</summary>
    private bool Busy(string key) => _posing.ContainsKey(key) || _walking.Contains(key);

    /// <summary>Who a cue's on, one key each, for their queues: none for a cue on nobody, or with no stage. A light on an item is on whoever's holding it.</summary>
    private IReadOnlyList<string> Keys(StageDirection cue)
    {
        if (_stage is null || cue.Actor is null || cue.Verb == "sigil") return Array.Empty<string>();

        IReadOnlyList<string> keys = _stage.Actors(cue.Actor);
        if (keys.Count == 0 && cue.Verb == "light" && Holder(cue.Actor) is string holder) return new[] { holder };
        return keys;
    }

    /// <summary>Does what a cue does. During a skip, whatever it starts is put at its end straight away.</summary>
    private void Start(StageDirection cue, IReadOnlyList<string> keys)
    {
        switch (cue.Verb)
        {
            case "fade":
                _fadeFrom = cue.FadesIn ? 1f : Blackout;
                _fadeTo = cue.FadesIn ? 0f : 1f;
                Blackout = _fadeFrom;
                break;

            case "slow":
                _speed = cue.Number;
                break;

            case "backdrop":
                Backdrop?.Invoke();
                break;

            case "sfx":
                if (!_skipping) _audio.Play(cue.Sound);
                break;

            case "music":
                if (cue.Song is Track song) _audio.PlaySong(song);
                else _audio.StopSong();
                break;

            case "wait":
            case "board":
            case "reflection":
            case "trail":
                // A wait is all clock; the board's on the line already (DialogueNode.Board); Legacy draws no reflection and no trail.
                break;

            case "hold":
            case "drop":
                Hold(cue, keys);
                break;

            default:
                if (_stage is not null) OnStage(cue, keys);
                break;
        }

        if (_skipping) Settle();
    }

    /// <summary>Does a cue that's something on the stage.</summary>
    private void OnStage(StageDirection cue, IReadOnlyList<string> keys)
    {
        switch (cue.Verb)
        {
            case "place":
                if (!_stage.Place(cue.Actor, cue.Spot, cue.Facing, cue.Count))
                    Log(cue.Cue, $"puts {cue.Actor} where they can't go: they aren't here and can't be brought on, or {cue.Spot.Name ?? "the crowd"} isn't here");
                break;

            case "walk":
                Walk(cue, keys);
                break;

            case "face":
                if (keys.Count == 0) Missing(cue, cue.Actor);
                else if (cue.Facing is CueFacing way) _stage.Face(cue.Actor, way);
                else if (_stage.Actors(cue.Target).Count > 0) _stage.FaceToward(cue.Actor, cue.Target);
                else Missing(cue, cue.Target);
                break;

            case "travel-actor":
                if (keys.Count == 0) Missing(cue, cue.Actor);
                else _stage.TravelActor(cue.Actor, cue.Map, cue.Spawn);
                break;

            case "anim":
                if (keys.Count == 0)
                {
                    Missing(cue, cue.Actor);
                    break;
                }
                Pose(keys, _stage.Animate(cue.Actor, cue.Clip, cue.Partner));
                break;

            case "light":
                // On someone, or on an item, which Legacy doesn't draw: whoever's holding it glows, and nobody if nobody is.
                foreach (string key in keys) _stage.Glow(key, cue.Glow);
                if (keys.Count == 0 && !KeyItems.Exists(cue.Actor)) Missing(cue, cue.Actor);
                break;

            case "sigil":
                _stage.Sigil(cue.Sigil, cue.OneFrame);
                break;

            case "show":
                Vector2? at = null;
                if (cue.Target is not null)
                {
                    if (!_stage.TryGetSpawn(cue.Target, out Vector2 spawn))
                    {
                        Missing(cue, cue.Target);
                        break;
                    }
                    at = spawn;
                }
                if (!_stage.Show(cue.Actor, at)) Missing(cue, cue.Actor);
                break;

            case "hide":
                if (!_stage.Hide(cue.Actor)) Missing(cue, cue.Actor);
                break;

            case "swap":
                // An item's other pictures (Brannock's shield with more names) aren't drawn in Legacy.
                if (KeyItems.Exists(cue.Actor) || Holder(cue.Actor) is not null) break;
                if (keys.Count == 0) Missing(cue, cue.Actor);
                else if (!_stage.Swap(cue.Actor, cue.Look)) Missing(cue, cue.Look);
                break;

            case "echo":
                if (keys.Count == 0 && !_stage.Place(cue.Actor, CueSpot.Crowd, null, 0))
                {
                    Missing(cue, cue.Actor);
                    break;
                }
                _stage.Echo(cue.Actor);
                if (cue.Clip is not null) Pose(_stage.Actors(cue.Actor), _stage.Animate(cue.Actor, cue.Clip, null));
                break;

            case "tint":
                _stage.Tint(cue.Tint, _skipping ? 0f : TintSeconds);
                break;

            case "camera":
                if (Where(cue.Target) is Vector2 look) _stage.Camera(look);
                else Missing(cue, cue.Target);
                break;

            case "shake":
                _stage.Shake(cue.Number);
                break;
        }
    }

    /// <summary>Sends someone walking: to a spawn, or to beside someone, and then to face them.</summary>
    private void Walk(StageDirection cue, IReadOnlyList<string> keys)
    {
        if (keys.Count == 0)
        {
            Missing(cue, cue.Actor);
            return;
        }

        bool toSomeone = !_stage.TryGetSpawn(cue.Target, out Vector2 to);
        if (toSomeone && (_stage.Actors(cue.Target).Count == 0 || !_stage.TryGetSpot(new CueSpot(CueSpotKind.Beside, cue.Target), out to)))
        {
            Missing(cue, cue.Target);
            return;
        }

        _stage.Walk(cue.Actor, to);
        foreach (string key in keys)
        {
            _walking.Add(key);
            if (toSomeone) _faceAfter[key] = cue.Target;
        }
    }

    /// <summary>Keeps everyone a clip's on busy for as long as it takes.</summary>
    private void Pose(IReadOnlyList<string> keys, float seconds)
    {
        if (seconds <= 0f || _skipping) return;
        foreach (string key in keys) _posing[key] = seconds;
    }

    /// <summary>Puts an item in a socket, or empties one, for everyone the cue's on: in the save, so it's there whatever draws them. Nothing's drawn for it in Legacy.</summary>
    private void Hold(StageDirection cue, IReadOnlyList<string> keys)
    {
        if (_stage is null) return;
        if (keys.Count == 0)
        {
            Missing(cue, cue.Actor);
            return;
        }

        foreach (string key in keys)
        {
            if (!_game.Sockets.TryGetValue(key, out Dictionary<string, string> held)) _game.Sockets[key] = held = new Dictionary<string, string>();
            if (cue.Verb == "hold") held[cue.Socket] = cue.Item;
            else held.Remove(cue.Socket);
            if (held.Count == 0) _game.Sockets.Remove(key);
        }
    }

    /// <summary>Whoever on the stage is holding an item in one of their sockets, or null.</summary>
    private string Holder(string item)
    {
        foreach ((string key, Dictionary<string, string> held) in _game.Sockets)
            if (held.ContainsValue(item) && _stage.Actors(key).Count > 0) return key;
        return null;
    }

    /// <summary>Where a camera cue looks: a spawn, or whoever it names.</summary>
    private Vector2? Where(string target)
    {
        if (_stage.TryGetSpawn(target, out Vector2 spawn)) return spawn;
        if (_stage.Actors(target).Count > 0 && _stage.TryGetSpot(new CueSpot(CueSpotKind.On, target), out Vector2 at)) return at;
        return null;
    }

    /// <summary>Someone a walk sent somewhere has got there: they turn to whoever they walked up to.</summary>
    private void Arrived(string key)
    {
        _walking.Remove(key);
        if (_faceAfter.Remove(key, out string other) && _stage.Actors(other).Count > 0) _stage.FaceToward(key, other);
    }

    /// <summary>Puts everything that's going at its end: the stage's walking and camera, and the poses.</summary>
    private void Settle()
    {
        _stage?.Finish();
        foreach (string key in _walking.ToList()) Arrived(key);
        _posing.Clear();
    }

    /// <summary>Logs a cue that can't play because someone or something it names isn't here.</summary>
    private void Missing(StageDirection cue, string id) => Log(cue.Cue, $"names {id}, who or which isn't here");

    /// <summary>Writes a cue that can't play to the error log, the same way the start-up check words it.</summary>
    private void Log(StageCue cue, string problem) =>
        Console.Error.WriteLine($"Dialogue {_script}: {_where} has a cue, {cue.Verb} {string.Join(' ', cue.Args)}, that {problem}.");
}
