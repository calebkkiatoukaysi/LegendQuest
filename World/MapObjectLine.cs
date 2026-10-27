using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LegendQuest.World;

/// <summary>
/// One line of a map's [objects], split up: the kind, the id, x and y, and its key=value settings.
/// </summary>
/// <remarks>
/// The loader takes each setting it knows out as it goes, then calls Done, which throws if anything
/// is left over. That's how a typo like gold=12 on a door gets caught. A value with spaces in it goes
/// in double quotes, like text="THE OLD ROAD".
/// </remarks>
internal sealed class MapObjectLine
{
    private readonly string _mapId;
    private readonly Dictionary<string, string> _settings = new();

    /// <summary>Which line of the file it's on, for the error messages.</summary>
    public int Line { get; }

    /// <summary>What kind of object it is, in lower case.</summary>
    public string Kind { get; }

    /// <summary>Its id (for a prop, which prop it is).</summary>
    public string Id { get; }

    /// <summary>How far across, in tiles.</summary>
    public float X { get; }

    /// <summary>How far down, in tiles.</summary>
    public float Y { get; }

    /// <summary>Splits a line up and reads x and y.</summary>
    /// <param name="mapId">The map, for the error messages.</param>
    /// <param name="line">Which line of the file it is.</param>
    /// <param name="text">The line, trimmed and with any comment taken off.</param>
    /// <exception cref="InvalidOperationException">If the line isn't kind id x y, or a setting isn't key=value.</exception>
    public MapObjectLine(string mapId, int line, string text)
    {
        _mapId = mapId;
        Line = line;

        List<string> parts = Split(text);
        if (parts.Count < 4) throw Fail("an object line has to be kind id x y, then any settings");

        Kind = parts[0].ToLowerInvariant();
        Id = parts[1];

        if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
            || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            throw Fail($"{parts[2]} {parts[3]} isn't a place on the map (x and y are in tiles)");
        X = x;
        Y = y;

        for (int i = 4; i < parts.Count; i++)
        {
            int equals = parts[i].IndexOf('=');
            if (equals <= 0) throw Fail($"{parts[i]} isn't a setting; they're key=value");

            string key = parts[i][..equals].ToLowerInvariant();
            if (!_settings.TryAdd(key, parts[i][(equals + 1)..])) throw Fail($"{key} is set twice");
        }
    }

    /// <summary>Takes a setting out, or returns null if the line doesn't have it.</summary>
    /// <param name="key">The setting.</param>
    public string Text(string key)
    {
        if (!_settings.Remove(key, out string value)) return null;
        if (value.Length == 0) throw Fail($"{key} is set to nothing");
        return value;
    }

    /// <summary>Takes a setting out that has to be there.</summary>
    /// <param name="key">The setting.</param>
    public string Required(string key) => Text(key) ?? throw Fail($"a {Kind} has to set {key}");

    /// <summary>Takes out a whole number from 1 up, or returns the fallback if it isn't set.</summary>
    /// <param name="key">The setting.</param>
    /// <param name="fallback">What it is when the line doesn't set it.</param>
    public int Count(string key, int fallback)
    {
        string text = Text(key);
        if (text is null) return fallback;
        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value) || value < 1)
            throw Fail($"{key} has to be a whole number from 1 up, not {text}");
        return value;
    }

    /// <summary>Takes out a number from 0 up, or returns the fallback if it isn't set.</summary>
    /// <param name="key">The setting.</param>
    /// <param name="fallback">What it is when the line doesn't set it.</param>
    public float Number(string key, float fallback)
    {
        string text = Text(key);
        if (text is null) return fallback;
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) || value < 0f)
            throw Fail($"{key} has to be a number from 0 up, not {text}");
        return value;
    }

    /// <summary>Takes out a yes or no, or returns the fallback if it isn't set.</summary>
    /// <param name="key">The setting.</param>
    /// <param name="fallback">What it is when the line doesn't set it.</param>
    public bool YesNo(string key, bool fallback) => Text(key)?.ToLowerInvariant() switch
    {
        null => fallback,
        "yes" or "true" => true,
        "no" or "false" => false,
        string other => throw Fail($"{key} has to be yes or no, not {other}"),
    };

    /// <summary>Takes out a direction (down, left, right or up), or returns the fallback if it isn't set.</summary>
    /// <param name="key">The setting.</param>
    /// <param name="fallback">What it is when the line doesn't set it.</param>
    public Direction Facing(string key, Direction fallback)
    {
        string text = Text(key);
        if (text is null) return fallback;
        if (!Enum.TryParse(text, ignoreCase: true, out Direction facing) || !Enum.IsDefined(facing))
            throw Fail($"{key} has to be down, left, right or up, not {text}");
        return facing;
    }

    /// <summary>Checks every setting on the line got used.</summary>
    /// <param name="allowed">The settings this kind has, for the message.</param>
    /// <exception cref="InvalidOperationException">If one didn't, which means it's spelled wrong or on the wrong kind.</exception>
    public void Done(string allowed)
    {
        if (_settings.Count == 0) return;

        string extra = string.Join(", ", _settings.Keys);
        throw Fail(allowed.Length == 0
            ? $"a {Kind} has nothing to set, but this line sets {extra}"
            : $"a {Kind} has no setting called {extra} (it has {allowed})");
    }

    /// <summary>An error about this line.</summary>
    /// <param name="message">What's wrong with it.</param>
    public InvalidOperationException Fail(string message) => MapLoader.Fail(_mapId, Line, message);

    /// <summary>Splits on spaces, except inside double quotes, which come off.</summary>
    private List<string> Split(string text)
    {
        var parts = new List<string>();
        var part = new StringBuilder();
        bool quoted = false;

        foreach (char c in text)
        {
            if (c == '"') quoted = !quoted;
            else if (c == ' ' && !quoted)
            {
                if (part.Length > 0) parts.Add(part.ToString());
                part.Clear();
            }
            else part.Append(c);
        }

        if (quoted) throw Fail("a \" is never closed");
        if (part.Length > 0) parts.Add(part.ToString());
        return parts;
    }
}
