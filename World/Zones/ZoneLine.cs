using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LegendQuest.World;

/// <summary>
/// One line of a zone file, split up: the words before its =, and what comes after, cut at each ·
/// into the line's body and its fields.
/// </summary>
/// <remarks>
/// A zone file's line is head = body · field · field. A field is a word on its own (mark) or a key
/// and the rest of the field as its value (gold 15, patrol 400 1200); a value in double quotes has
/// them taken off. The loader takes each field it knows out as it goes, then calls Done, which
/// throws if anything's left, the way the tile maps' loader caught a typo. A ; outside quotes is a
/// comment, taken off before the line gets here.
/// </remarks>
internal sealed class ZoneLine
{
    /// <summary>What separates the body and the fields: a middle dot.</summary>
    internal const char Separator = '·';

    private readonly string _file;
    private readonly Dictionary<string, string> _fields = new();

    /// <summary>Which line of the file it is, for the errors.</summary>
    public int Line { get; }

    /// <summary>Its first word, in lower case: a setting or a kind of object.</summary>
    public string Key { get; }

    /// <summary>Every word before its =, Key first.</summary>
    public IReadOnlyList<string> Head { get; }

    /// <summary>Everything after its =, trimmed, or null if it hasn't one.</summary>
    public string Body { get; }

    /// <summary>Body cut at each · outside quotes, each trimmed. Empty if there's no body.</summary>
    public IReadOnlyList<string> Segments { get; }

    /// <summary>Splits a line up.</summary>
    /// <param name="file">The file, as errors call it: Zones/docks.txt.</param>
    /// <param name="number">Which line it is.</param>
    /// <param name="text">The line, trimmed, its comment off.</param>
    public ZoneLine(string file, int number, string text)
    {
        _file = file;
        Line = number;

        int equals = OutsideQuotes(text, '=');
        string head = equals < 0 ? text : text[..equals];
        string[] words = head.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0) throw Fail("a line has to start with a key");

        words[0] = words[0].ToLowerInvariant();
        Head = words;
        Key = words[0];
        Body = equals < 0 ? null : text[(equals + 1)..].Trim();
        Segments = Body is null ? Array.Empty<string>() : Split(Body);
    }

    /// <summary>Reads Segments from one on as key value fields, for the getters below to take out.</summary>
    /// <param name="first">The first segment that's a field (the ones before it are the body's).</param>
    public void TakeFields(int first)
    {
        for (int i = first; i < Segments.Count; i++)
        {
            string field = Segments[i];
            if (field.Length == 0) throw Fail($"there's an empty field (two {Separator}s together, or one at the end)");
            if (field[0] == '"') throw Fail($"{field} is a quoted value with no key in front of it");

            int space = field.IndexOfAny(new[] { ' ', '\t' });
            string key = (space < 0 ? field : field[..space]).ToLowerInvariant();
            string value = space < 0 ? null : field[(space + 1)..].Trim();
            if (value is not null && value.StartsWith('"')) value = Unquote(value, key);
            if (!_fields.TryAdd(key, value)) throw Fail($"{key} is set twice");
        }
    }

    /// <summary>Takes a field's value out, or returns null if the line hasn't got it.</summary>
    /// <param name="key">The field.</param>
    public string Text(string key)
    {
        if (!_fields.Remove(key, out string value)) return null;
        return value ?? throw Fail($"{key} needs a value");
    }

    /// <summary>Takes out a field that has to be there.</summary>
    /// <param name="key">The field.</param>
    /// <param name="kind">What the line is, for the message.</param>
    public string Required(string key, string kind) => Text(key) ?? throw Fail($"a {kind} has to set {key}");

    /// <summary>Takes out a field that's a word on its own, like mark, and says whether it was there.</summary>
    /// <param name="key">The field.</param>
    public bool Flag(string key)
    {
        if (!_fields.Remove(key, out string value)) return false;
        return value is null ? true : throw Fail($"{key} is a word on its own, with nothing after it");
    }

    /// <summary>Takes out a yes or no, or returns the fallback if it isn't set.</summary>
    /// <param name="key">The field.</param>
    /// <param name="fallback">What it is when the line doesn't set it.</param>
    public bool YesNo(string key, bool fallback) => Text(key) is string text ? ReadYesNo(text, key) : fallback;

    /// <summary>Takes out a number from 0 up, or returns the fallback if it isn't set.</summary>
    /// <param name="key">The field.</param>
    /// <param name="fallback">What it is when the line doesn't set it.</param>
    public float Number(string key, float fallback)
    {
        string text = Text(key);
        if (text is null) return fallback;
        float value = ReadNumber(text, key);
        return value >= 0f ? value : throw Fail($"{key} can't be less than 0");
    }

    /// <summary>Takes out a field that's several numbers, like patrol 400 1200, or returns null if it isn't set.</summary>
    /// <param name="key">The field.</param>
    /// <param name="count">How many numbers it has to be.</param>
    public float[] Numbers(string key, int count) => Text(key) is string text ? ReadNumbers(text, key, count) : null;

    /// <summary>Takes out when and unless, which most objects can have.</summary>
    public MapPresence Presence() => new(Text("when"), Text("unless"));

    /// <summary>Checks every field on the line got used.</summary>
    /// <param name="kind">What the line is.</param>
    /// <param name="allowed">The fields its kind has, for the message.</param>
    /// <exception cref="InvalidOperationException">If one didn't, which means it's spelled wrong or on the wrong kind.</exception>
    public void Done(string kind, string allowed)
    {
        if (_fields.Count == 0) return;
        throw Fail($"a {kind} has no field called {string.Join(", ", _fields.Keys)} (it has {allowed})");
    }

    /// <summary>Reads a yes or no.</summary>
    /// <param name="text">What the file says.</param>
    /// <param name="what">What it is, for the message.</param>
    public bool ReadYesNo(string text, string what) => text.ToLowerInvariant() switch
    {
        "yes" or "true" => true,
        "no" or "false" => false,
        _ => throw Fail($"{what} has to be yes or no, not {text}"),
    };

    /// <summary>Reads a number, any number but not infinity or NaN.</summary>
    /// <param name="text">What the file says.</param>
    /// <param name="what">What it is, for the message.</param>
    public float ReadNumber(string text, string what)
    {
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) || !float.IsFinite(value))
            throw Fail($"{what} has to be a number, not {text}");
        return value;
    }

    /// <summary>Reads several numbers between spaces.</summary>
    /// <param name="text">What the file says.</param>
    /// <param name="what">What they are, for the message.</param>
    /// <param name="count">How many there have to be.</param>
    public float[] ReadNumbers(string text, string what, int count)
    {
        string[] parts = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != count) throw Fail($"{what} has to be {count} numbers, not {text}");

        var numbers = new float[count];
        for (int i = 0; i < count; i++) numbers[i] = ReadNumber(parts[i], what);
        return numbers;
    }

    /// <summary>Reads a whole number from a lowest value up.</summary>
    /// <param name="text">What the file says.</param>
    /// <param name="what">What it is, for the message.</param>
    /// <param name="least">The lowest it can be.</param>
    public int ReadWhole(string text, string what, int least = 0)
    {
        if (!int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value))
            throw Fail($"{what} has to be a whole number, not {text}");
        return value >= least ? value : throw Fail($"{what} has to be {least} or more, not {text}");
    }

    /// <summary>Checks something is one word: an id, a name, a prop.</summary>
    /// <param name="text">What the file says.</param>
    /// <param name="what">What it is, for the message.</param>
    public string ReadWord(string text, string what)
    {
        if (string.IsNullOrEmpty(text) || text.Contains('"')) throw Fail($"{what} has to be one word, not {text}");
        foreach (char c in text)
            if (char.IsWhiteSpace(c)) throw Fail($"{what} has to be one word, not {text}");
        return text;
    }

    /// <summary>Takes the double quotes off a value that has to have them, like a door's name.</summary>
    /// <param name="text">What the file says.</param>
    /// <param name="what">What it is, for the message.</param>
    public string Unquote(string text, string what)
    {
        if (text.Length < 3 || text[0] != '"' || text[^1] != '"' || text[1..^1].Contains('"'))
            throw Fail($"{what} has to be in double quotes, like \"The Rusty Anvil\", with no \" inside, not {text}");
        return text[1..^1];
    }

    /// <summary>An error about this line.</summary>
    /// <param name="message">What's wrong with it.</param>
    public InvalidOperationException Fail(string message) => ZoneLoader.Fail(_file, Line, message);

    /// <summary>Where a character first comes outside double quotes, or -1.</summary>
    /// <param name="text">The text to look in.</param>
    /// <param name="c">The character.</param>
    internal static int OutsideQuotes(string text, char c)
    {
        bool quoted = false;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '"') quoted = !quoted;
            else if (text[i] == c && !quoted) return i;
        }
        return -1;
    }

    /// <summary>Cuts a line's body at each separator outside quotes, trimming each piece.</summary>
    private List<string> Split(string body)
    {
        var parts = new List<string>();
        var part = new StringBuilder();
        bool quoted = false;
        foreach (char c in body)
        {
            if (c == '"') quoted = !quoted;
            if (c == Separator && !quoted)
            {
                parts.Add(part.ToString().Trim());
                part.Clear();
            }
            else part.Append(c);
        }
        if (quoted) throw Fail("a \" is never closed");
        parts.Add(part.ToString().Trim());
        return parts;
    }
}
