using System;
using System.Collections.Generic;

namespace LegendQuest.Story;

/// <summary>
/// Every meter the story keeps: how close you are to the Soul-Bound, how far they're drifting,
/// each companion's approval, your renown, and how far the sun's gone.
/// </summary>
/// <remarks>
/// The numbers are the story's. Kept as a table like the class catalog, so a typo in a script's
/// meter shows up when the game starts. The hidden ones (Kael's and Valencia's loyalty to the
/// Soul-Bound) have no companion, so moving them never pops a toast.
/// </remarks>
public static class Meters
{
    /// <summary>Closeness to the Soul-Bound.</summary>
    public const string Bond = "bond";

    /// <summary>The Soul-Bound's quiet drift towards power.</summary>
    public const string Ambition = "ambition";

    /// <summary>Renown. People mention it.</summary>
    public const string Fame = "fame";

    /// <summary>How far the Long Eclipse has gone, 0 to 5.</summary>
    public const string EclipseStage = "eclipse-stage";

    /// <summary>The most fame there is. It isn't really capped; it just has to stop somewhere.</summary>
    private const int MostFame = 999;

    private static readonly MeterDefinition[] Table =
    {
        new(Bond, 50, 0, 100, null),
        new(Ambition, 10, 0, 100, null),
        new("affinity-sera", 20, 0, 100, "sera"),
        new("affinity-kael", 20, 0, 100, "kael"),
        new("affinity-mira", 20, 0, 100, "mira"),
        new("affinity-brannock", 20, 0, 100, "brannock"),
        new("affinity-elara", 20, 0, 100, "elara"),
        new("affinity-valencia", 10, 0, 100, "valencia"),
        new("affinity-kael-sb", 40, 0, 100, null),
        new("affinity-valencia-sb", 0, 0, 100, null),
        new(Fame, 0, 0, MostFame, null),
        new(EclipseStage, 0, 0, 5, null),
    };

    private static readonly Dictionary<string, MeterDefinition> ById = new();

    /// <summary>Indexes the table.</summary>
    /// <remarks>Throws on purpose, so a bad row shows up when the game starts.</remarks>
    static Meters()
    {
        foreach (MeterDefinition meter in Table)
        {
            if (meter.Start < meter.Min || meter.Start > meter.Max)
                throw new InvalidOperationException($"Meter {meter.Id} starts at {meter.Start}, outside {meter.Min} to {meter.Max}.");
            if (!ById.TryAdd(meter.Id, meter)) throw new InvalidOperationException($"Two meters are called {meter.Id}.");
        }
    }

    /// <summary>Whether there's a meter called this.</summary>
    /// <param name="id">What a script calls it.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One meter.</summary>
    /// <param name="id">Its id. It has to exist.</param>
    public static MeterDefinition Get(string id) => ById[id];
}
