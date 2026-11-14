using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LegendQuest.Characters;

/// <summary>
/// Everyone's figure put together before it's needed, so walking into a room or starting a
/// conversation never stalls a frame while a figure is made.
/// </summary>
/// <remarks>
/// Prewarm starts it as the game loads: every recipe's figure is put together off the main thread,
/// MostWarming at a time, and each frame (Update) the finished ones are made into figures on the
/// main thread (their textures), as many as fit in WarmBudgetMs, and kept with the rest (Built). By the
/// time a player is through the title and the creation screen they're all made. A figure asked for
/// before its turn is made at once, as before, or, if it's being put together already, waited for. The
/// proof run makes them all at once as it starts (Synchronous), in parallel, so its shots and timings
/// don't depend on how fast it got through the menus.
/// </remarks>
public static partial class PartCatalog
{
    /// <summary>How many recipes are put together at once in the background.</summary>
    private const int MostWarming = 2;

    /// <summary>How long a frame may spend making finished figures' textures.</summary>
    private const double WarmBudgetMs = 3.0;

    /// <summary>The recipes still to put together, and those being put together now, by their outfit's Key.</summary>
    private static readonly Queue<Outfit> ToWarm = new();
    private static readonly Dictionary<string, (Outfit Outfit, Task<(RigComposer.Result Result, bool HasBody, float RigPx, string Base)?> Task)> Warmed = new(StringComparer.Ordinal);

    /// <summary>Starts putting every recipe's figure together in the background (all at once in the proof run).</summary>
    public static void Prewarm()
    {
        if (_file is null || _device is null) return;
        var outfits = new List<Outfit>();
        foreach ((string id, PartsFile.RecipeEntry recipe) in _file.Recipes)
        {
            Outfit outfit = Headed(Outfit.Of(id, recipe));
            if (!Built.ContainsKey(outfit.Key)) outfits.Add(outfit);
        }
        foreach (Outfit outfit in CrowdOutfits)
            if (!Built.ContainsKey(outfit.Key)) outfits.Add(outfit);
        if (Synchronous)
        {
            var made = new (RigComposer.Result Result, bool HasBody, float RigPx, string Base)?[outfits.Count];
            Parallel.For(0, outfits.Count, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Math.Min(4, Environment.ProcessorCount)) },
                i => made[i] = Composed(outfits[i]));
            for (int i = 0; i < outfits.Count; i++)
                if (!Built.ContainsKey(outfits[i].Key)) Keep(outfits[i], made[i]);
            return;
        }
        foreach (Outfit outfit in outfits) ToWarm.Enqueue(outfit);
    }

    /// <summary>Moves the background work on: makes finished figures (within WarmBudgetMs) and starts the next.</summary>
    public static void Update()
    {
        if (Warmed.Count == 0 && ToWarm.Count == 0) return;
        var clock = Stopwatch.StartNew();
        foreach (string key in Warmed.Keys.ToList())
        {
            if (clock.Elapsed.TotalMilliseconds > WarmBudgetMs) break;
            (Outfit outfit, var task) = Warmed[key];
            if (!task.IsCompleted) continue;
            Warmed.Remove(key);
            if (task.IsFaulted) throw task.Exception!.GetBaseException();
            if (!Built.ContainsKey(key)) Keep(outfit, task.Result);
        }
        while (Warmed.Count < MostWarming && ToWarm.Count > 0)
        {
            Outfit next = ToWarm.Dequeue();
            if (Built.ContainsKey(next.Key) || Warmed.ContainsKey(next.Key)) continue;
            Warmed[next.Key] = (next, Task.Run(() => Composed(next)));
        }
    }

    /// <summary>The background work putting an outfit together, if it's underway, so Build can wait for it rather than do it again.</summary>
    private static Task<(RigComposer.Result Result, bool HasBody, float RigPx, string Base)?> Warming(string key)
    {
        if (!Warmed.TryGetValue(key, out var warming)) return null;
        Warmed.Remove(key);
        return warming.Task;
    }
}
