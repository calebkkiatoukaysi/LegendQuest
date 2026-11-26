using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>How a clip goes from one key to the next.</summary>
/// <remarks>
/// A clip has one for every key that doesn't say otherwise (its "ease"), and a key can name its own as
/// a third item ([t, v, "back"]), which governs the stretch from that key to the next. The maths is
/// tools/rig/clips.py's, the same to the fourth decimal place: with u how far through the stretch, 0
/// to 1, the value is v0 + (v1 - v0) * f(u).
/// </remarks>
public enum ClipEase
{
    /// <summary>A cubic through the keys that never overshoots them: at rest at a key where a channel turns back or holds, moving on through one it carries on past. Its slopes are worked out over the whole track.</summary>
    Smooth,

    /// <summary>Eased in and out of every key with a sine, stopping at each.</summary>
    Sine,

    /// <summary>Straight from each key to the next: f = u.</summary>
    Linear,

    /// <summary>Starting slow and getting faster into the next key: f = u².</summary>
    In,

    /// <summary>Starting fast and slowing into the next key: f = 1 - (1 - u)².</summary>
    Out,

    /// <summary>Fast out, past the next key and back to it (ease-out with overshoot, the standard 1.70158): the follow-through after a blow.</summary>
    Back,

    /// <summary>Stepped: the key's value held until the next key, which it jumps to.</summary>
    Hold,
}

/// <summary>Something a clip says happens at a time into it: a blow landing ("hit"), a shot let go ("release"), a foot down ("step"), the body hitting the floor ("land"), and the rest of the contract's list; with what it's about, if anything (the bone a shot leaves from, an effect's id).</summary>
/// <param name="Time">When, in seconds into the clip.</param>
/// <param name="Name">What happens.</param>
/// <param name="Arg">What it's about, or null.</param>
public readonly record struct ClipEvent(float Time, string Name, string Arg)
{
    /// <summary>The impact: the frame a blow lands. A flurry has several.</summary>
    public const string Hit = "hit";

    /// <summary>A projectile, a beam or a thrown thing leaves (Arg the bone, hand_near if none).</summary>
    public const string Release = "release";

    /// <summary>A held warning pose starts.</summary>
    public const string Telegraph = "telegraph";

    /// <summary>The earliest the clip may be cut by the next action.</summary>
    public const string Recover = "recover";

    /// <summary>The body hits the floor.</summary>
    public const string Land = "land";

    /// <summary>A foot lands.</summary>
    public const string Step = "step";

    /// <summary>A purely visual effect, Arg its id.</summary>
    public const string Fx = "fx";
}

/// <summary>A stretch of a clip when what it names (the weapon) smears and leaves a ribbon behind it: the fastest frames of a swing.</summary>
/// <param name="Start">From when, in seconds into the clip.</param>
/// <param name="End">Until when.</param>
/// <param name="What">What trails: "weapon".</param>
public readonly record struct ClipTrail(float Start, float End, string What);

/// <summary>
/// One clip of the figures' animation (tools/rig/clips.py): how long it is, whether it loops, and
/// each bone's keys and how it eases between them; what it brings in front of the chest (a far arm
/// across the body), if anything; and for one that moves the figure along, how fast its planted feet
/// are left behind and when each lands. A fighting move also says when its blow lands and whatever
/// else happens in it (Events), how quickly it blends in (Blend), and when its weapon smears
/// (Trails).
/// </summary>
/// <param name="Name">Its name.</param>
/// <param name="Length">How long it is, in seconds.</param>
/// <param name="Loop">Whether it starts again when it ends; one that doesn't holds its last pose.</param>
/// <param name="Ease">How it goes from one key to the next, where a key doesn't say.</param>
/// <param name="Tracks">Its keys, each bone's channels.</param>
/// <param name="Front">The bones it draws in front of the chest, by name; empty for none.</param>
/// <param name="Speed">How fast its planted feet are left behind, in the clips' units a second; 0 for a clip that doesn't go anywhere.</param>
/// <param name="Steps">When each foot lands, in seconds into it; empty for none.</param>
/// <param name="Swaps">The pieces it draws another picture of, and from when: a bone's piece swapped for its "bone.part" (RigComposer's variants); empty for none.</param>
public sealed record Clip(string Name, float Length, bool Loop, ClipEase Ease, IReadOnlyList<ClipTrack> Tracks, IReadOnlyList<string> Front,
    float Speed, IReadOnlyList<float> Steps, IReadOnlyList<ClipSwap> Swaps)
{
    /// <summary>How long a clip takes to blend in from whatever was showing when it doesn't say, in seconds.</summary>
    public const float DefaultBlend = 0.12f;

    /// <summary>Everything that happens in it, in time order (the steps among them); empty for none.</summary>
    public IReadOnlyList<ClipEvent> Events { get; init; } = Array.Empty<ClipEvent>();

    /// <summary>How long it takes to blend in from whatever was showing, in seconds: 0.12 unless it says (a strike or a flinch 0.03 to 0.05, so it snaps).</summary>
    public float Blend { get; init; } = DefaultBlend;

    /// <summary>When its weapon smears and trails a ribbon; empty for never.</summary>
    public IReadOnlyList<ClipTrail> Trails { get; init; } = Array.Empty<ClipTrail>();

    /// <summary>Windows in which bones draw immediately before the head, preserving their order.</summary>
    public IReadOnlyList<ClipBehind> Behind { get; init; } = Array.Empty<ClipBehind>();

    /// <summary>Specific layers drawn immediately before the head, such as a cloak swept forward.</summary>
    public IReadOnlyList<string> BeforeHead { get; init; } = Array.Empty<string>();

    /// <summary>Whether a bone goes behind the head at this clip time.</summary>
    public bool BehindHead(string bone, float time)
    {
        if (Loop && Length > 0f) time %= Length;
        foreach (ClipBehind window in Behind)
            if (time >= window.Start && time < window.End && window.Bones.Contains(bone)) return true;
        return false;
    }

    /// <summary>
    /// Whether it's an action of the figure's own (an auto attack, a skill, a foe's move, a telegraph,
    /// a strike): its "hit" events are its blows landing. A reaction (hurt, block, knockdown, die)
    /// marks its own contact frame with "hit" too, the frame it's struck, which isn't a blow of its own.
    /// </summary>
    public bool IsAction => Starts("auto-") || Starts("skill-") || Starts("foe-") || Starts("telegraph") || Starts("strike") || Starts("soul-link-strike");

    private bool Starts(string prefix) => Name.StartsWith(prefix, StringComparison.Ordinal);

    /// <summary>The time of its first event of a name ("hit", "release"), or null if it hasn't one.</summary>
    /// <param name="name">The event's name.</param>
    public float? FirstEvent(string name)
    {
        for (int i = 0; i < Events.Count; i++)
            if (Events[i].Name == name) return Events[i].Time;
        return null;
    }

    /// <summary>The times of every event of a name, in order: each blow of a flurry.</summary>
    /// <param name="name">The event's name.</param>
    public IEnumerable<float> EventTimes(string name)
    {
        for (int i = 0; i < Events.Count; i++)
            if (Events[i].Name == name) yield return Events[i].Time;
    }

    /// <summary>Whether its weapon is trailing at a time into it (inside one of its Trails).</summary>
    /// <param name="time">Seconds into it.</param>
    public bool Trailing(float time)
    {
        for (int i = 0; i < Trails.Count; i++)
            if (time >= Trails[i].Start && time <= Trails[i].End) return true;
        return false;
    }

    /// <summary>
    /// Every event that happens between two times into it, in order: after <paramref name="from"/>,
    /// up to and including <paramref name="to"/>. A clip that loops goes round: from later to earlier
    /// means it went past its end and started again, and the events either side of the join count.
    /// </summary>
    /// <param name="from">Where it was, in seconds into it (less than nothing to include an event at 0).</param>
    /// <param name="to">Where it is now.</param>
    /// <param name="each">What to do with each.</param>
    public void Crossed(float from, float to, Action<ClipEvent> each)
    {
        if (Events.Count == 0 || from == to) return;
        bool wrapped = Loop && to < from;
        for (int i = 0; i < Events.Count; i++)
        {
            float t = Events[i].Time;
            if (wrapped ? t > from || t <= to : t > from && t <= to) each(Events[i]);
        }
    }

    /// <summary>The other picture a bone's piece is drawn as at a time into it (its last swap by then), or null for its own.</summary>
    /// <param name="bone">The bone.</param>
    /// <param name="time">Seconds into it.</param>
    public string PartOf(string bone, float time)
    {
        string part = null;
        for (int i = 0; i < Swaps.Count; i++)
            if (Swaps[i].Bone == bone && Swaps[i].Time <= time) part = Swaps[i].Part;
        return part;
    }

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

/// <summary>What a track of a clip moves: a bone's turn, its offset across or down, or its stretch.</summary>
public enum ClipChannel
{
    /// <summary>The turn, in degrees clockwise.</summary>
    Rot,

    /// <summary>The offset across, in the clips' units.</summary>
    X,

    /// <summary>The offset down, in the clips' units.</summary>
    Y,

    /// <summary>How much wider its piece is, and how much further across its children's joints are.</summary>
    Sx,

    /// <summary>How much taller its piece is, and how much further down its children's joints are.</summary>
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
    /// <summary>The overshoot of ClipEase.Back: the standard constant, which carries about a tenth past the key.</summary>
    public const double BackOvershoot = 1.70158;

    /// <summary>Each key's own ease, for the stretch from it to the next: null where a key doesn't name one (the clip's goes), or null altogether.</summary>
    public ClipEase?[] Eases { get; init; }

    /// <summary>
    /// The keys' times and values as clips.json gives them, to double precision, and the slopes
    /// worked out from those; or null to go by Times, Values and Slopes. A fast stretch turns a
    /// hundred degrees in a few hundredths of a second, so a key time rounded to a float can move it
    /// by most of the ten-thousandth tools/rig/clips.py and this have to agree to.
    /// </summary>
    public double[] ExactTimes { get; init; }

    /// <inheritdoc cref="ExactTimes"/>
    public double[] ExactValues { get; init; }

    /// <inheritdoc cref="ExactTimes"/>
    public double[] ExactSlopes { get; init; }

    /// <summary>Its value at a time: held before its first key and after its last, eased between, each stretch as its first key says or else as the clip does.</summary>
    /// <param name="time">Seconds into the clip.</param>
    /// <param name="ease">The clip's ease, for a key that doesn't name its own.</param>
    public float At(float time, ClipEase ease)
    {
        if (ExactTimes is not null) return At(time, ease, ExactTimes, ExactValues, ExactSlopes);
        int n = Times.Length;
        var times = new double[n];
        var values = new double[n];
        var slopes = new double[n];
        for (int i = 0; i < n; i++)
        {
            times[i] = Times[i];
            values[i] = Values[i];
            slopes[i] = Slopes[i];
        }
        return At(time, ease, times, values, slopes);
    }

    private float At(float time, ClipEase ease, double[] times, double[] values, double[] slopes)
    {
        if (time <= times[0]) return (float)values[0];
        int last = times.Length - 1;
        if (time >= times[last]) return (float)values[last];
        int k = 1;
        while (times[k] < time) k++;
        double h = Math.Max(1e-6, times[k] - times[k - 1]);
        double u = (time - times[k - 1]) / h;
        double v0 = values[k - 1], v1 = values[k];
        switch (Eases?[k - 1] ?? ease)
        {
            case ClipEase.Linear:
                return (float)(v0 + (v1 - v0) * u);
            case ClipEase.Sine:
                return (float)(v0 + (v1 - v0) * (0.5 - 0.5 * Math.Cos(Math.PI * u)));
            case ClipEase.In:
                return (float)(v0 + (v1 - v0) * u * u);
            case ClipEase.Out:
                return (float)(v0 + (v1 - v0) * (1.0 - (1.0 - u) * (1.0 - u)));
            case ClipEase.Back:
            {
                double w = u - 1.0;
                return (float)(v0 + (v1 - v0) * (1.0 + (BackOvershoot + 1.0) * w * w * w + BackOvershoot * w * w));
            }
            case ClipEase.Hold:
                return (float)v0;
            default:
                double u2 = u * u, u3 = u2 * u;
                return (float)((2.0 * u3 - 3.0 * u2 + 1.0) * v0 + (u3 - 2.0 * u2 + u) * h * slopes[k - 1]
                    + (-2.0 * u3 + 3.0 * u2) * v1 + (u3 - u2) * h * slopes[k]);
        }
    }

    /// <summary>
    /// Each key's slope for easing smoothly through it (tools/rig/clips.py's slopes(), the same): the
    /// slopes of the stretches either side blended by their weighted harmonic mean, so the curve never
    /// overshoots a key; none where the channel turns back or holds, nor at a clip's ends, unless it
    /// loops, when its first and last keys (the same pose) share the slope across the join. They're
    /// worked out over the whole track whatever each key's ease, so a smooth stretch beside a stepped
    /// one curves as it would among smooth ones.
    /// </summary>
    public static float[] Slope(float[] times, float[] values, bool loop)
    {
        double[] m = Slope(Array.ConvertAll(times, t => (double)t), Array.ConvertAll(values, v => (double)v), loop);
        return Array.ConvertAll(m, v => (float)v);
    }

    /// <summary>Slope, to double precision.</summary>
    public static double[] Slope(double[] times, double[] values, bool loop)
    {
        int n = times.Length;
        var m = new double[n];
        if (n < 3) return m;
        double H(int i) => Math.Max(1e-6, times[i + 1] - times[i]);
        double D(int i) => (values[i + 1] - values[i]) / H(i);
        static double Blend(double d0, double d1, double h0, double h1) =>
            d0 * d1 <= 0.0 ? 0.0 : 3.0 * (h0 + h1) / ((2.0 * h1 + h0) / d0 + (h1 + 2.0 * h0) / d1);
        for (int i = 1; i < n - 1; i++) m[i] = Blend(D(i - 1), D(i), H(i - 1), H(i));
        if (loop) m[0] = m[n - 1] = Blend(D(n - 2), D(0), H(n - 2), H(0));
        return m;
    }

    /// <summary>An ease by the name clips.json gives it, or null for one it doesn't know (the clip's then goes).</summary>
    /// <param name="name">"smooth", "sine", "linear", "in", "out", "back" or "hold".</param>
    public static ClipEase? EaseNamed(string name) => name switch
    {
        "smooth" => ClipEase.Smooth,
        "sine" => ClipEase.Sine,
        "linear" => ClipEase.Linear,
        "in" => ClipEase.In,
        "out" => ClipEase.Out,
        "back" => ClipEase.Back,
        "hold" => ClipEase.Hold,
        _ => null,
    };
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

    /// <summary>Every clip's own name (not its other names), in no particular order.</summary>
    public static IEnumerable<string> Names => Clips.Keys;

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

    /// <summary>The clip a fighting move (windup, strike, cast, ...) plays for a kind of weapon (one-hand, two-hand, polearm, dagger, unarmed, hook, staff, bow, crossbow), or its only one, or null.</summary>
    public static Clip ForMove(string move, string weapon)
    {
        if (!Combat.TryGetValue(move, out Dictionary<string, string> byClass)) return null;
        if (weapon is not null && byClass.TryGetValue(weapon, out string clip) || byClass.TryGetValue("", out clip)) return Get(clip);
        return byClass.Count > 0 ? Get(byClass.Values.First()) : null;
    }

    /// <summary>The name of the auto attack a kind of weapon swings for the nth blow of a run: auto-one-hand-1, -2, -3, then round again.</summary>
    /// <param name="weapon">The kind of weapon (WalkerSprite.WeaponKind); null for unarmed.</param>
    /// <param name="swing">Which blow of the run, from 0.</param>
    public static string AutoName(string weapon, int swing) => $"auto-{weapon ?? "unarmed"}-{(swing % 3 + 3) % 3 + 1}";

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
                    var keys = channel.Value.EnumerateArray().Select(k => (T: k[0].GetDouble(), V: k[1].GetDouble(),
                        E: k.GetArrayLength() >= 3 && k[2].ValueKind == JsonValueKind.String ? ClipTrack.EaseNamed(k[2].GetString()) : null)).ToArray();
                    if (keys.Length == 0) continue;
                    double[] exactTimes = keys.Select(k => k.T).ToArray(), exactValues = keys.Select(k => k.V).ToArray();
                    double[] exactSlopes = ClipTrack.Slope(exactTimes, exactValues, loop);
                    float[] times = Array.ConvertAll(exactTimes, t => (float)t), values = Array.ConvertAll(exactValues, v => (float)v);
                    ClipEase?[] eases = keys.Any(k => k.E is not null) ? keys.Select(k => k.E).ToArray() : null;
                    tracks.Add(new ClipTrack(index, which.Value, times, values, Array.ConvertAll(exactSlopes, m => (float)m))
                    {
                        Eases = eases, ExactTimes = exactTimes, ExactValues = exactValues, ExactSlopes = exactSlopes,
                    });
                }
            }
        }
        float length = json.TryGetProperty("length", out JsonElement l) ? l.GetSingle() : 1f;
        ClipEase ease = json.TryGetProperty("ease", out JsonElement e) && e.ValueKind == JsonValueKind.String
            ? ClipTrack.EaseNamed(e.GetString()) ?? ClipEase.Smooth
            : ClipEase.Smooth;
        string[] front = json.TryGetProperty("front", out JsonElement f) && f.ValueKind == JsonValueKind.Array ? f.EnumerateArray().Select(b => b.GetString()).ToArray() : Array.Empty<string>();
        float speed = json.TryGetProperty("speed", out JsonElement sp) && sp.ValueKind == JsonValueKind.Number ? sp.GetSingle() : 0f;

        // Events: [t, name] or [t, name, arg]. Steps are the "step" ones, as they always were.
        var events = new List<ClipEvent>();
        if (json.TryGetProperty("events", out JsonElement evs) && evs.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement ev in evs.EnumerateArray())
            {
                if (ev.ValueKind != JsonValueKind.Array || ev.GetArrayLength() < 2 || ev[0].ValueKind != JsonValueKind.Number || ev[1].ValueKind != JsonValueKind.String) continue;
                string arg = ev.GetArrayLength() < 3 ? null : ev[2].ValueKind switch
                {
                    JsonValueKind.String => ev[2].GetString(),
                    JsonValueKind.Number => ev[2].GetRawText(),
                    _ => null,
                };
                events.Add(new ClipEvent(ev[0].GetSingle(), ev[1].GetString(), arg));
            }
        }
        events.Sort((a, b) => a.Time.CompareTo(b.Time));
        var steps = new List<float>();
        foreach (ClipEvent ev in events)
            if (ev.Name == ClipEvent.Step) steps.Add(ev.Time);

        var swaps = new List<ClipSwap>();
        if (json.TryGetProperty("swaps", out JsonElement sw) && sw.ValueKind == JsonValueKind.Array)
            foreach (JsonElement s in sw.EnumerateArray())
                if (s.GetArrayLength() >= 3) swaps.Add(new ClipSwap(s[0].GetSingle(), s[1].GetString(), s[2].GetString()));

        float blend = json.TryGetProperty("blend", out JsonElement bl) && bl.ValueKind == JsonValueKind.Number ? MathF.Max(0f, bl.GetSingle()) : Clip.DefaultBlend;
        var trails = new List<ClipTrail>();
        if (json.TryGetProperty("trail", out JsonElement tr) && tr.ValueKind == JsonValueKind.Array)
            foreach (JsonElement t in tr.EnumerateArray())
                if (t.ValueKind == JsonValueKind.Array && t.GetArrayLength() >= 2 && t[0].ValueKind == JsonValueKind.Number && t[1].ValueKind == JsonValueKind.Number)
                    trails.Add(new ClipTrail(t[0].GetSingle(), t[1].GetSingle(), t.GetArrayLength() >= 3 && t[2].ValueKind == JsonValueKind.String ? t[2].GetString() : "weapon"));

        var behind = new List<ClipBehind>();
        if (json.TryGetProperty("behind", out JsonElement bh))
            foreach (JsonElement window in bh.EnumerateArray())
            {
                float start = window[0].GetSingle(), end = window[1].GetSingle();
                string[] windowBones = window[2].EnumerateArray().Select(b => b.GetString()).ToArray();
                if (!float.IsFinite(start) || !float.IsFinite(end) || start < 0f || end <= start || end > length || windowBones.Any(b => skeleton.IndexOf(b) < 0))
                    throw new InvalidOperationException($"Clip {name} has an invalid behind window.");
                behind.Add(new ClipBehind(start, end, windowBones));
            }
        string[] beforeHead = json.TryGetProperty("layer", out JsonElement layer)
            ? layer.EnumerateObject().Where(p => p.Value.ValueKind == JsonValueKind.String && p.Value.GetString() == "before-head").Select(p => p.Name).ToArray()
            : Array.Empty<string>();
        return new Clip(name, MathF.Max(0.01f, length), loop, ease, tracks, front, speed, steps, swaps)
            { Events = events, Blend = blend, Trails = trails, Behind = behind, BeforeHead = beforeHead };
    }
}

/// <summary>A clip drawing a bone's piece as another picture of it from a time on.</summary>
/// <param name="Time">From when, in seconds into the clip.</param>
/// <param name="Bone">The bone whose piece it is.</param>
/// <param name="Part">Which other picture: the piece "bone.part".</param>
public readonly record struct ClipSwap(float Time, string Bone, string Part);

/// <summary>A time window that draws its bones before the head.</summary>
public sealed record ClipBehind(float Start, float End, IReadOnlyList<string> Bones);

/// <summary>
/// A figure's clip playing: how far into it, how fast, and the pose it's blending from, so going
/// from one clip to another (walking to standing, a wind-up to its strike) eases over the new clip's
/// Blend rather than jumping.
/// </summary>
public sealed class ClipPlayer
{
    /// <summary>How long a change of clip takes to blend when the clip doesn't say, in seconds.</summary>
    public const float Blend = Clip.DefaultBlend;

    private readonly BonePose[] _from, _sampled;
    private float _time, _speed = 1f, _blend = 1f, _blendLength = Blend;

    /// <summary>How long the clip has been playing, in seconds of the figure's own clock: what the blend is measured by once it's over.</summary>
    private float _since;

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

    /// <summary>How fast it's playing, against its own time.</summary>
    public float Speed => _speed;

    /// <summary>How far the blend into the clip has got, 0 to 1, smoothed: 1 once it's the clip alone.</summary>
    public float Blended => _blend * _blend * (3f - 2f * _blend);

    /// <summary>Plays a clip from its start, blending from the pose showing over the clip's own Blend; the clip that's playing just changes speed, unless it's to start again.</summary>
    /// <param name="clip">The clip.</param>
    /// <param name="speed">How fast, against its own time.</param>
    /// <param name="restart">Whether to start it again if it's the one playing: a second blow of the same swing, a second flinch.</param>
    /// <returns>Whether it started from the beginning.</returns>
    public bool Play(Clip clip, float speed = 1f, bool restart = false)
    {
        _speed = speed;
        if (clip == Clip && !restart) return false;
        Array.Copy(Pose, _from, Pose.Length);
        Clip = clip;
        _time = 0f;
        _since = 0f;
        _blendLength = clip?.Blend ?? Blend;
        _blend = _blendLength <= 0f ? 1f : 0f;
        Refresh();
        return true;
    }

    /// <summary>Moves the clip on.</summary>
    /// <param name="seconds">How long since the last update: 0 while the figure's held in a hit-stop.</param>
    public void Update(float seconds)
    {
        if (Clip is null) return;
        _time += seconds * _speed;
        if (Clip.Loop) _time %= Clip.Length;
        else _time = MathF.Min(_time, Clip.Length);
        if (_blendLength > 0f) _blend = MathF.Min(1f, _blend + seconds / _blendLength);
        _since += seconds;
        Refresh();
    }

    /// <summary>The pose at another time into the clip playing, with the blend as it was or will be then: for sampling a swing between frames (a ribbon's points).</summary>
    /// <param name="time">Seconds into the clip, near Time.</param>
    /// <param name="into">Each bone's pose, filled in.</param>
    public void PoseAt(float time, BonePose[] into)
    {
        if (Clip is null)
        {
            Array.Fill(into, BonePose.Rest);
            return;
        }
        Clip.Sample(time, into);
        if (_blendLength <= 0f || _speed <= 0f) return;
        float b = MathHelper.Clamp(_blend - (_time - time) / _speed / _blendLength, 0f, 1f);
        float t = b * b * (3f - 2f * b);
        if (t >= 1f) return;
        for (int i = 0; i < into.Length; i++) into[i] = BonePose.Lerp(_from[i], into[i], t);
    }

    /// <summary>How far the blend into the clip had got at an earlier time into it, 0 to 1 before smoothing: 1 once it was the clip alone. Unlike PoseAt's, it's right long after the blend is over.</summary>
    /// <param name="time">Seconds into the clip, at or before Time.</param>
    private float BlendAt(float time) =>
        Clip is null || _blendLength <= 0f || _speed <= 0f ? 1f : MathHelper.Clamp((_since - (_time - time) / _speed) / _blendLength, 0f, 1f);

    /// <summary>Whether the pose at an earlier time into the clip playing is the clip's alone, the blend from the pose before over by then.</summary>
    /// <param name="time">Seconds into the clip, at or before Time.</param>
    public bool SettledAt(float time) => BlendAt(time) >= 1f;

    /// <summary>The pose at an earlier time into the clip playing, with the blend as it was then: for a figure drawn in steps (the pixel look's twelve poses a second).</summary>
    /// <param name="time">Seconds into the clip, at or before Time.</param>
    /// <param name="into">Each bone's pose, filled in.</param>
    public void PoseStepped(float time, BonePose[] into)
    {
        if (Clip is null)
        {
            Array.Fill(into, BonePose.Rest);
            return;
        }
        Clip.Sample(time, into);
        float b = BlendAt(time), t = b * b * (3f - 2f * b);
        if (t >= 1f) return;
        for (int i = 0; i < into.Length; i++) into[i] = BonePose.Lerp(_from[i], into[i], t);
    }

    private void Refresh()
    {
        if (Clip is null)
        {
            Array.Fill(Pose, BonePose.Rest);
            return;
        }
        Clip.Sample(_time, _sampled);
        float t = Blended;
        for (int i = 0; i < Pose.Length; i++) Pose[i] = t >= 1f ? _sampled[i] : BonePose.Lerp(_from[i], _sampled[i], t);
    }
}
