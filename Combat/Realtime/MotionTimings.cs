using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// When each blow lands: the times of every clip's hit and release events, and its length, read once
/// from Content/Characters/Parts/clips.json.
/// </summary>
/// <remarks>
/// The contract's timing source of truth (contract §2.4): the engine never decides on its own when a
/// blow lands if a clip exists for it, so retiming a clip retimes the fight, in the game and in the
/// simulator alike (both read the same file from the build's Content folder). It is a plain JSON read
/// with no MonoGame types, so the simulator needs no window. A clip is looked up by name in every
/// rig's clips and then its aliases. When a clip or its event is missing, Fallback's times stand in,
/// and Missing lists it for --validate.
/// </remarks>
public static class MotionTimings
{
    /// <summary>Where an auto attack's blow lands when its clip has no hit: 0.30 s in.</summary>
    public const int AutoFallbackMs = 300;

    /// <summary>Where a melee skill's blow lands without a clip: 0.45 s in.</summary>
    public const int MeleeFallbackMs = 450;

    /// <summary>Where a cast or a shot is let go without a clip: 0.40 s in.</summary>
    public const int ReleaseFallbackMs = 400;

    /// <summary>Where an ultimate lands without a clip: 0.70 s in.</summary>
    public const int UltimateFallbackMs = 700;

    /// <summary>How long a fallback clip lasts after its last blow, so recovery has somewhere to be.</summary>
    private const int FallbackRecoveryMs = 250;

    private static readonly object Gate = new();
    private static Dictionary<string, MotionTiming> _clips;
    private static Dictionary<string, string> _aliases;
    private static string _loadedFrom;

    /// <summary>Where clips.json was read from, or null if it couldn't be found (every timing is then a fallback).</summary>
    public static string LoadedFrom
    {
        get
        {
            Ensure();
            return _loadedFrom;
        }
    }

    /// <summary>Reads clips.json from a path, replacing whatever was read before. Ensure does this from the build's Content folder on first use.</summary>
    /// <param name="path">Where clips.json is.</param>
    public static void Load(string path)
    {
        var clips = new Dictionary<string, MotionTiming>(StringComparer.Ordinal);
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        if (File.Exists(path))
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.TryGetProperty("rigs", out JsonElement rigs) && rigs.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty rig in rigs.EnumerateObject())
                {
                    if (rig.Value.TryGetProperty("clips", out JsonElement table) && table.ValueKind == JsonValueKind.Object)
                        foreach (JsonProperty clip in table.EnumerateObject())
                            clips.TryAdd(clip.Name, Read(clip.Value));
                    if (rig.Value.TryGetProperty("aliases", out JsonElement names) && names.ValueKind == JsonValueKind.Object)
                        foreach (JsonProperty alias in names.EnumerateObject())
                            if (alias.Value.ValueKind == JsonValueKind.String) aliases.TryAdd(alias.Name, alias.Value.GetString());
                }
            }
        }

        lock (Gate)
        {
            _clips = clips;
            _aliases = aliases;
            _loadedFrom = File.Exists(path) ? path : null;
        }
    }

    /// <summary>Whether a clip of this name exists, as itself or as an alias.</summary>
    /// <param name="clip">The clip's name.</param>
    public static bool Exists(string clip) => Find(clip) is not null;

    /// <summary>A clip's timing, or a fallback of the given kind when there's no such clip or it has no blow in it.</summary>
    /// <param name="clip">The clip's name.</param>
    /// <param name="kind">What sort of move it is, for the fallback's times.</param>
    public static MotionTiming Of(string clip, MotionKind kind)
    {
        MotionTiming found = Find(clip);
        if (found is not null && (found.HitsMs.Count > 0 || found.ReleasesMs.Count > 0 || kind == MotionKind.Reaction)) return found;
        return Fallback(clip, kind, found?.LengthMs ?? 0);
    }

    /// <summary>The fallback timing for a kind of move: one blow at the contract's time, and a little after it.</summary>
    /// <param name="clip">The clip's name, kept on the timing.</param>
    /// <param name="kind">What sort of move it is.</param>
    /// <param name="length">The clip's real length if it exists without events, or 0.</param>
    public static MotionTiming Fallback(string clip, MotionKind kind, int length = 0)
    {
        int at = kind switch
        {
            MotionKind.Auto => AutoFallbackMs,
            MotionKind.Melee => MeleeFallbackMs,
            MotionKind.Release => ReleaseFallbackMs,
            MotionKind.Ultimate => UltimateFallbackMs,
            _ => 0,
        };
        var none = Array.Empty<int>();
        bool releases = kind == MotionKind.Release;
        return new MotionTiming(clip, Math.Max(length, at + FallbackRecoveryMs), releases ? none : new[] { at }, releases ? new[] { at } : none, 0, false);
    }

    /// <summary>Every clip name in a list that clips.json doesn't have, for --validate's warnings.</summary>
    /// <param name="names">The names to look for.</param>
    public static IReadOnlyList<string> Missing(IEnumerable<string> names)
    {
        var missing = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string name in names)
            if (name is not null && seen.Add(name) && !Exists(name)) missing.Add(name);
        return missing;
    }

    /// <summary>Reads clips.json from the build's Content folder the first time a timing's asked for.</summary>
    private static void Ensure()
    {
        if (_clips is not null) return;
        lock (Gate)
        {
            if (_clips is not null) return;
        }
        Load(Path.Combine(AppContext.BaseDirectory, "Content", "Characters", "Parts", "clips.json"));
    }

    /// <summary>A clip by name, following one alias, or null.</summary>
    private static MotionTiming Find(string clip)
    {
        if (string.IsNullOrEmpty(clip)) return null;
        Ensure();
        if (_clips.TryGetValue(clip, out MotionTiming timing)) return timing;
        if (_aliases.TryGetValue(clip, out string target) && _clips.TryGetValue(target, out timing)) return timing with { Name = clip };
        return null;
    }

    /// <summary>One clip's length and events, out of its JSON.</summary>
    private static MotionTiming Read(JsonElement clip)
    {
        double length = clip.TryGetProperty("length", out JsonElement l) && l.ValueKind == JsonValueKind.Number ? l.GetDouble() : 0.0;
        var hits = new List<int>();
        var releases = new List<int>();
        int recover = 0;
        if (clip.TryGetProperty("events", out JsonElement events) && events.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement e in events.EnumerateArray())
            {
                if (e.ValueKind != JsonValueKind.Array || e.GetArrayLength() < 2) continue;
                if (e[0].ValueKind != JsonValueKind.Number || e[1].ValueKind != JsonValueKind.String) continue;
                int ms = (int)Math.Round(e[0].GetDouble() * 1000.0);
                switch (e[1].GetString())
                {
                    case "hit": hits.Add(ms); break;
                    case "release": releases.Add(ms); break;
                    case "recover": recover = ms; break;
                }
            }
        }
        hits.Sort();
        releases.Sort();
        return new MotionTiming(null, (int)Math.Round(length * 1000.0), hits, releases, recover, true);
    }
}

/// <summary>A clip's timing: its length and when its blows land, in milliseconds from its start.</summary>
/// <param name="Name">The clip's name.</param>
/// <param name="LengthMs">How long it plays.</param>
/// <param name="HitsMs">When each blow lands, in order.</param>
/// <param name="ReleasesMs">When each thing thrown, shot or cast leaves the hand, in order.</param>
/// <param name="RecoverMs">The earliest it may be cut by the next action, or 0 for "after its last blow".</param>
/// <param name="FromFile">Whether it came out of clips.json rather than a fallback.</param>
public sealed record MotionTiming(string Name, int LengthMs, IReadOnlyList<int> HitsMs, IReadOnlyList<int> ReleasesMs, int RecoverMs, bool FromFile)
{
    /// <summary>Every moment something lands or leaves, in order: the hits and releases together.</summary>
    public IReadOnlyList<int> Moments
    {
        get
        {
            var all = new List<int>(HitsMs);
            all.AddRange(ReleasesMs);
            all.Sort();
            return all;
        }
    }
}

/// <summary>What sort of move a clip is, for its fallback timing.</summary>
public enum MotionKind
{
    /// <summary>An auto attack's swing.</summary>
    Auto,

    /// <summary>A melee skill.</summary>
    Melee,

    /// <summary>A cast, a shot or a throw: something leaves the hand.</summary>
    Release,

    /// <summary>An ultimate.</summary>
    Ultimate,

    /// <summary>A reaction (a block, a flinch): no blow.</summary>
    Reaction,
}
