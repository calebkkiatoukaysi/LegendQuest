using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Dialogue;

namespace LegendQuest.Components;

/// <summary>
/// The pixel-art conversation portraits: a front-facing face for each speaker who has one, an
/// expression to a row, and how it moves while it's up.
/// </summary>
/// <remarks>
/// tools/pixel/portrait_kit.py converts ChatGPT's paintings (tools/source/pixel/portraits/) into
/// Content/Portraits/&lt;id&gt;.png, one 96 x 96 face per expression in Dialogue/Expressions.cs's
/// order, and portraits.json, which expressions each sheet has. The id is the speaker's (sera,
/// brannock) or a variant's (brannock-sling). A face a sheet hasn't falls back to the nearest it
/// has (Expressions.Nearest). Someone without a sheet keeps the old portrait.
///
/// The faces move without frames of their own: they breathe (down an art pixel and back every few
/// seconds), bob with the words while a line types out, and dip when the expression changes. Cold
/// doesn't move at all (Expressions.Cold). Every move is a whole art pixel down, drawn by cutting
/// that many rows off the bottom of the face, so its well never shows a gap and the pixels stay
/// square.
/// </remarks>
public sealed class PortraitKit
{
    /// <summary>A face's size in art pixels.</summary>
    public const int Grid = 96;

    /// <summary>How long a breath takes, and how much of it is drawn an art pixel down.</summary>
    private const float BreathSeconds = 3.4f;
    private const float BreathDown = 0.45f;

    /// <summary>How long the face stays up and down while the words are typing.</summary>
    private const float TalkBeat = 0.13f;

    /// <summary>How long the dip lasts when the expression changes.</summary>
    private const float DipSeconds = 0.14f;

    private static Dictionary<string, string[]> _rows;
    private static Dictionary<string, string> _aliases;

    private readonly ContentManager _content;
    private readonly Dictionary<string, Texture2D> _sheets = new(StringComparer.Ordinal);
    private float _time;
    private float _dip;
    private string _shownId;
    private string _shownFace;

    /// <summary>Gets the portraits ready to load through a content manager. Reads the list of sheets the first time.</summary>
    /// <param name="content">Where the sheets load from; they go when it unloads.</param>
    public PortraitKit(ContentManager content)
    {
        _content = content;
        if (_rows is null) (_rows, _aliases) = ReadManifest(content.RootDirectory);
    }

    /// <summary>Whether there's a pixel portrait for an id.</summary>
    /// <param name="id">A speaker's id, or a variant's.</param>
    public static bool Has(string id) => id is not null && _rows is not null && _rows.ContainsKey(Resolve(id));

    /// <summary>The portrait a speaker uses: their own, or the one portraits.json's aliases give them (refugee-mother is Hedda).</summary>
    private static string Resolve(string id) =>
        id is not null && _aliases is not null && _aliases.TryGetValue(id, out string to) ? to : id;

    /// <summary>The expressions a portrait has, in its sheet's order, or none.</summary>
    /// <param name="id">A speaker's id, or a variant's.</param>
    public static IReadOnlyList<string> FacesOf(string id) =>
        id is not null && _rows is not null && _rows.TryGetValue(Resolve(id), out string[] rows) ? rows : Array.Empty<string>();

    /// <summary>Moves the breathing, the talking and the dip on.</summary>
    /// <param name="seconds">The frame's length.</param>
    public void Update(float seconds)
    {
        _time += seconds;
        _dip = Math.Max(0f, _dip - seconds);
    }

    /// <summary>
    /// Draws someone's face at an expression into a well, if they have a portrait: whole art pixels,
    /// as many screen pixels each as fit the well.
    /// </summary>
    /// <param name="spriteBatch">A batch begun with point sampling.</param>
    /// <param name="id">Whose face.</param>
    /// <param name="expression">The line's expression, or null for neutral.</param>
    /// <param name="well">Where it goes; square.</param>
    /// <param name="talking">Whether the line's still typing out, so the face bobs with the words.</param>
    /// <param name="tint">What it's drawn with.</param>
    /// <returns>Whether there was a portrait to draw.</returns>
    public bool Draw(SpriteBatch spriteBatch, string id, string expression, Rectangle well, bool talking, Color tint)
    {
        if (!Has(id)) return false;
        id = Resolve(id);

        string face = Pick(id, expression);
        if (id != _shownId || face != _shownFace)
        {
            // A new speaker starts still; the same one changing expression dips.
            if (id == _shownId) _dip = DipSeconds;
            _shownId = id;
            _shownFace = face;
        }

        int row = Array.IndexOf(_rows[id], face);
        int scale = Math.Max(1, well.Width / Grid);
        int down = Offset(expression, talking);
        var source = new Rectangle(0, row * Grid, Grid, Grid - down);
        var at = new Rectangle(well.X + (well.Width - Grid * scale) / 2, well.Y + (well.Height - Grid * scale) / 2 + down * scale,
            Grid * scale, (Grid - down) * scale);
        spriteBatch.Draw(Sheet(id), at, source, tint);
        return true;
    }

    /// <summary>How many art pixels down the face is drawn this frame: 0 or 1.</summary>
    private int Offset(string expression, bool talking)
    {
        if (expression == Expressions.Cold) return 0;
        if (_dip > 0f) return 1;
        if (talking) return (int)(_time / TalkBeat) % 2;
        return _time % BreathSeconds / BreathSeconds < BreathDown ? 1 : 0;
    }

    /// <summary>The face a portrait shows for an expression: that one, or the nearest it has.</summary>
    private static string Pick(string id, string expression)
    {
        string[] rows = _rows[id];
        foreach (string face in Expressions.Nearest(expression))
            if (Array.IndexOf(rows, face) >= 0) return face;
        return rows[0];
    }

    /// <summary>A portrait's sheet, loaded the first time it's drawn.</summary>
    private Texture2D Sheet(string id)
    {
        if (!_sheets.TryGetValue(id, out Texture2D sheet))
            _sheets[id] = sheet = _content.Load<Texture2D>("Portraits/" + id);
        return sheet;
    }

    /// <summary>Reads which expressions each sheet has, and the aliases, from portraits.json; none if it isn't there.</summary>
    private static (Dictionary<string, string[]>, Dictionary<string, string>) ReadManifest(string root)
    {
        var rows = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            using Stream stream = TitleContainer.OpenStream(Path.Combine(root, "Portraits", "portraits.json"));
            using JsonDocument doc = JsonDocument.Parse(stream);
            foreach (JsonProperty portrait in doc.RootElement.GetProperty("portraits").EnumerateObject())
            {
                var faces = new List<string>();
                foreach (JsonElement face in portrait.Value.EnumerateArray()) faces.Add(face.GetString());
                if (faces.Count > 0) rows[portrait.Name] = faces.ToArray();
            }
            if (doc.RootElement.TryGetProperty("aliases", out JsonElement named))
                foreach (JsonProperty alias in named.EnumerateObject()) aliases[alias.Name] = alias.Value.GetString();
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
        return (rows, aliases);
    }
}
