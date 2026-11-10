using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>How a clip goes from one key to the next.</summary>
public enum ClipEase
{
    /// <summary>A cubic through the keys that never overshoots them: at rest at a key where a channel turns back or holds, moving on through one it carries on past.</summary>
    Smooth,

    /// <summary>Eased in and out of every key with a sine, stopping at each.</summary>
    Sine,

    /// <summary>Straight from each key to the next.</summary>
    Linear,
}

/// <summary>
/// One clip of the figures' animation (tools/rig/clips.py): how long it is, whether it loops, and
/// each bone's keys and how it eases between them; what it brings in front of the chest (a far arm
/// across the body), if anything; and for one that moves the figure along, how fast its planted feet
/// are left behind and when each lands.
/// </summary>
/// <param name="Name">Its name.</param>
/// <param name="Length">How long it is, in seconds.</param>
/// <param name="Loop">Whether it starts again when it ends; one that doesn't holds its last pose.</param>
/// <param name="Ease">How it goes from one key to the next.</param>
/// <param name="Tracks">Its keys, each bone's channels.</param>
/// <param name="Front">The bones it draws in front of the chest, by name; empty for none.</param>
/// <param name="Speed">How fast its planted feet are left behind, in the clips' units a second; 0 for a clip that doesn't go anywhere.</param>
/// <param name="Steps">When each foot lands, in seconds into it; empty for none.</param>
public sealed record Clip(string Name, float Length, bool Loop, ClipEase Ease, IReadOnlyList<ClipTrack> Tracks, IReadOnlyList<string> Front,
    float Speed, IReadOnlyList<float> Steps)
{
    /// <summary>Every bone's pose at a time into it, held at its ends: what it doesn't move, at rest.</summary>
    /// <param name="time">Seconds into it.</param>
    /// <param name="into">Each bone's pose, by its place in the skeleton, filled in.</param>
    public void Sample(float time, BonePose[] into)
    {
        Array.Fill(into, BonePose.Rest);
        foreach (ClipTrack track in Tracks)
        {
            float v = track.At(time, Ease);
            BonePose p = into[track.Bone];
            into[track.Bone] = track.Channel switch
            {
                ClipChannel.Rot => p with { Rot = v },
                ClipChannel.X => p with { X = v },
                ClipChannel.Y => p with { Y = v },
                ClipChannel.Sx => p with { Sx = v },
                _ => p with { Sy = v },
            };
        }
    }
}

/// <summary>What a track of a clip moves: a bone's turn, its offset across or down, or its piece's stretch.</summary>
public enum ClipChannel
{
    /// <summary>The turn, in degrees clockwise.</summary>
    Rot,

    /// <summary>The offset across, in the clips' units.</summary>
    X,

    /// <summary>The offset down, in the clips' units.</summary>
    Y,

    /// <summary>How much wider its piece is.</summary>
    Sx,

    /// <summary>How much taller its piece is.</summary>
    Sy,
}

/// <summary>One bone's keys in one channel of a clip, and each key's slope for easing smoothly through it.</summary>
/// <param name="Bone">The bone, by its place in the skeleton.</param>
/// <param name="Channel">What it moves.</param>
/// <param name="Times">Each key's time, in seconds, in order.</param>
/// <param name="Values">Each key's value.</param>
/// <param name="Slopes">Each key's slope, a second, for ClipEase.Smooth (Slope works them out).</param>
public sealed record ClipTrack(int Bone, ClipChannel Channel, float[] Times, float[] Values, float[] Slopes)
{
    /// <summary>Its value at a time: held before its first key and after its last, eased between.</summary>
    public float At(float time, ClipEase ease)
    {
        if (time <= Times[0]) return Values[0];
        int last = Times.Length - 1;
        if (time >= Times[last]) return Values[last];
        int k = 1;
        while (Times[k] < time) k++;
        float h = MathF.Max(1e-6f, Times[k] - Times[k - 1]);
        float f = (time - Times[k - 1]) / h;
        switch (ease)
        {
            case ClipEase.Linear:
                return MathHelper.Lerp(Values[k - 1], Values[k], f);
            case ClipEase.Sine:
                return MathHelper.Lerp(Values[k - 1], Values[k], 0.5f - 0.5f * MathF.Cos(MathHelper.Pi * f));
            default:
                float f2 = f * f, f3 = f2 * f;
                return (2f * f3 - 3f * f2 + 1f) * Values[k - 1] + (f3 - 2f * f2 + f) * h * Slopes[k - 1]
                    + (-2f * f3 + 3f * f2) * Values[k] + (f3 - f2) * h * Slopes[k];
        }
    }

    /// <summary>
    /// Each key's slope for easing smoothly through it (tools/rig/clips.py's slopes(), the same): the
    /// slopes of the stretches either side blended by their weighted harmonic mean, so the curve never
    /// overshoots a key; none where the channel turns back or holds, nor at a clip's ends, unless it
    /// loops, when its first and last keys (the same pose) share the slope across the join.
    /// </summary>
    public static float[] Slope(float[] times, float[] values, bool loop)
    {
        int n = times.Length;
        var m = new float[n];
        if (n < 3) return m;
        float H(int i) => MathF.Max(1e-6f, times[i + 1] - times[i]);
        float D(int i) => (values[i + 1] - values[i]) / H(i);
        static float Blend(float d0, float d1, float h0, float h1) =>
            d0 * d1 <= 0f ? 0f : 3f * (h0 + h1) / ((2f * h1 + h0) / d0 + (h1 + 2f * h0) / d1);
        for (int i = 1; i < n - 1; i++) m[i] = Blend(D(i - 1), D(i), H(i - 1), H(i));
        if (loop) m[0] = m[n - 1] = Blend(D(n - 2), D(0), H(n - 2), H(0));
        return m;
    }
}

/// <summary>
/// Every clip the figures play, from Content/Characters/Parts/clips.json (tools/rig/clips.py writes
/// it, tools/import_parts.py copies it there), by name or by any of its other names, and which clip
/// each fighting move plays for each kind of weapon.
/// </summary>
public static class ClipLibrary
{
    private static readonly Dictionary<string, Clip> Clips = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Dictionary<string, string>> Combat = new(StringComparer.Ordinal);

    /// <summary>Reads the clips for a skeleton, dropping any bone it hasn't (a hair's own bone, say). Nothing's there if the file isn't.</summary>
    /// <param name="path">Where clips.json is.</param>
    /// <param name="skeleton">The skeleton they play on.</param>
    public static void Load(string path, Skeleton skeleton)
    {
        Clips.Clear();
        Aliases.Clear();
        Combat.Clear();
        if (!File.Exists(path) || skeleton is null) return;

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement rig = doc.RootElement.GetProperty("rigs").GetProperty("humanoid");
        foreach (JsonProperty clip in rig.GetProperty("clips").EnumerateObject())
            Clips[clip.Name] = Read(clip.Name, clip.Value, skeleton);
        if (rig.TryGetProperty("aliases", out JsonElement aliases))
            foreach (JsonProperty alias in aliases.EnumerateObject())
                Aliases[alias.Name] = alias.Value.GetString();
        if (rig.TryGetProperty("combat", out JsonElement combat))
        {
            foreach (JsonProperty move in combat.EnumerateObject())
            {
                var byClass = new Dictionary<string, string>(StringComparer.Ordinal);
                if (move.Value.ValueKind == JsonValueKind.String) byClass[""] = move.Value.GetString();
                else if (move.Value.ValueKind == JsonValueKind.Object)
                    foreach (JsonProperty kind in move.Value.EnumerateObject())
                        if (kind.Value.ValueKind == JsonValueKind.String) byClass[kind.Name] = kind.Value.GetString();
                Combat[move.Name] = byClass;
            }
        }
    }

    /// <summary>How many clips there are.</summary>
    public static int Count => Clips.Count;

    /// <summary>A clip by its name or another name for it.</summary>
    public static bool TryGet(string name, out Clip clip)
    {
        clip = null;
        if (name is null) return false;
        return Clips.TryGetValue(name, out clip) || Aliases.TryGetValue(name, out string real) && Clips.TryGetValue(real, out clip);
    }

    /// <summary>A clip by its name or another name for it, the one worked out for a base's own proportions ("jog@base-female") if there is one.</summary>
    /// <param name="name">The clip's name, or another name for it.</param>
    /// <param name="onBase">The base the figure's built on, or null.</param>
    /// <param name="clip">The clip.</param>
    public static bool TryGet(string name, string onBase, out Clip clip)
    {
        if (!TryGet(name, out clip)) return false;
        if (onBase is not null && Clips.TryGetValue(clip.Name + "@" + onBase, out Clip own)) clip = own;
        return true;
    }

    /// <summary>A clip by name, or null.</summary>
    public static Clip Get(string name) => TryGet(name, out Clip clip) ? clip : null;

    /// <summary>A clip by name, the base's own if there is one, or null.</summary>
    public static Clip Get(string name, string onBase) => TryGet(name, onBase, out Clip clip) ? clip : null;

    /// <summary>The clip a fighting move (windup, strike, cast, ...) plays for a kind of weapon (one-hand, two-hand, polearm, dagger, unarmed, hook, bow, crossbow), or its only one, or null.</summary>
    public static Clip ForMove(string move, string weapon)
    {
        if (!Combat.TryGetValue(move, out Dictionary<string, string> byClass)) return null;
        if (weapon is not null && byClass.TryGetValue(weapon, out string clip) || byClass.TryGetValue("", out clip)) return Get(clip);
        return byClass.Count > 0 ? Get(byClass.Values.First()) : null;
    }

    private static Clip Read(string name, JsonElement json, Skeleton skeleton)
    {
        var tracks = new List<ClipTrack>();
        bool loop = json.TryGetProperty("loop", out JsonElement lp) && lp.ValueKind == JsonValueKind.True;
        if (json.TryGetProperty("bones", out JsonElement bones))
        {
            foreach (JsonProperty bone in bones.EnumerateObject())
            {
                int index = skeleton.IndexOf(bone.Name);
                if (index < 0) continue;
                foreach (JsonProperty channel in bone.Value.EnumerateObject())
                {
                    ClipChannel? which = channel.Name switch
                    {
                        "rot" => ClipChannel.Rot,
                        "x" => ClipChannel.X,
                        "y" => ClipChannel.Y,
                        "sx" => ClipChannel.Sx,
                        "sy" => ClipChannel.Sy,
                        _ => null,
                    };
                    if (which is null) continue;
                    var keys = channel.Value.EnumerateArray().Select(k => (T: k[0].GetSingle(), V: k[1].GetSingle())).ToArray();
                    if (keys.Length == 0) continue;
                    float[] times = keys.Select(k => k.T).ToArray(), values = keys.Select(k => k.V).ToArray();
                    tracks.Add(new ClipTrack(index, which.Value, times, values, ClipTrack.Slope(times, values, loop)));
                }
            }
        }
        float length = json.TryGetProperty("length", out JsonElement l) ? l.GetSingle() : 1f;
        ClipEase ease = json.TryGetProperty("ease", out JsonElement e) ? e.GetString() switch
        {
            "linear" => ClipEase.Linear,
            "sine" => ClipEase.Sine,
            _ => ClipEase.Smooth,
        } : ClipEase.Smooth;
        string[] front = json.TryGetProperty("front", out JsonElement f) && f.ValueKind == JsonValueKind.Array ? f.EnumerateArray().Select(b => b.GetString()).ToArray() : Array.Empty<string>();
        float speed = json.TryGetProperty("speed", out JsonElement sp) && sp.ValueKind == JsonValueKind.Number ? sp.GetSingle() : 0f;
        var steps = new List<float>();
        if (json.TryGetProperty("events", out JsonElement events) && events.ValueKind == JsonValueKind.Array)
            foreach (JsonElement ev in events.EnumerateArray())
                if (ev.GetArrayLength() >= 2 && ev[1].GetString() == "step") steps.Add(ev[0].GetSingle());
        return new Clip(name, MathF.Max(0.01f, length), loop, ease, tracks, front, speed, steps);
    }
}

/// <summary>
/// A figure's clip playing: how far into it, how fast, and the pose it's blending from, so going
/// from one clip to another (walking to standing, a wind-up to its strike) eases over a moment
/// rather than jumping.
/// </summary>
public sealed class ClipPlayer
{
    /// <summary>How long a change of clip takes to blend, in seconds.</summary>
    public const float Blend = 0.12f;

    private readonly BonePose[] _from, _sampled;
    private float _time, _speed = 1f, _blend = 1f, _blendLength = Blend;

    /// <summary>A player for a skeleton's bones, at rest.</summary>
    public ClipPlayer(Skeleton skeleton)
    {
        Pose = new BonePose[skeleton.Count];
        _from = new BonePose[skeleton.Count];
        _sampled = new BonePose[skeleton.Count];
        Array.Fill(Pose, BonePose.Rest);
    }

    /// <summary>The clip playing, or null for the rest pose.</summary>
    public Clip Clip { get; private set; }

    /// <summary>Every bone's pose now.</summary>
    public BonePose[] Pose { get; }

    /// <summary>Whether a clip that doesn't loop has come to its end (it holds its last pose).</summary>
    public bool Finished => Clip is { Loop: false } && _time >= Clip.Length;

    /// <summary>Seconds into the clip.</summary>
    public float Time => _time;

    /// <summary>Plays a clip from its start, blending from the pose showing; the clip that's playing just changes speed.</summary>
    /// <param name="clip">The clip.</param>
    /// <param name="speed">How fast, against its own time.</param>
    /// <param name="restart">Whether to start it again if it's the one playing.</param>
    public void Play(Clip clip, float speed = 1f, bool restart = false)
    {
        _speed = speed;
        if (clip == Clip && !restart) return;
        Array.Copy(Pose, _from, Pose.Length);
        Clip = clip;
        _time = 0f;
        _blend = 0f;
        _blendLength = Blend;
        Refresh();
    }

    /// <summary>Moves the clip on.</summary>
    /// <param name="seconds">How long since the last update.</param>
    public void Update(float seconds)
    {
        if (Clip is null) return;
        _time += seconds * _speed;
        if (Clip.Loop) _time %= Clip.Length;
        else _time = MathF.Min(_time, Clip.Length);
        _blend = MathF.Min(1f, _blend + seconds / _blendLength);
        Refresh();
    }

    private void Refresh()
    {
        if (Clip is null)
        {
            Array.Fill(Pose, BonePose.Rest);
            return;
        }
        Clip.Sample(_time, _sampled);
        float t = _blend * _blend * (3f - 2f * _blend);
        for (int i = 0; i < Pose.Length; i++) Pose[i] = t >= 1f ? _sampled[i] : BonePose.Lerp(_from[i], _sampled[i], t);
    }
}
