using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The real-time HUD's icons, found by id: every class skill's ("fighter-cleave"), the bar's own
/// ("drink-potion", "soul-link-strike"), the foes' telegraphed moves ("foe-hook-pull"), and every
/// aura's ("burning", "hexed"); and the two textures the hotbar's sweeps and glints are cut from.
/// </summary>
/// <remarks>
/// <para>
/// tools/generate_combat_art.py draws the icons onto Combat/skills.png and Combat/auras.png, 24x24
/// a cell, and writes Combat/icons.json beside them: each sheet's ids in cell order. This reads that
/// index, so nothing here depends on an enum's order and a new skill only needs its icon drawn.
/// </para>
/// <para>
/// The cooldown sweep is a fan of veils made here, once, as a texture of SweepFrames frames: frame
/// k covers the last k/SweepFrames of the circle, clockwise from twelve, cut to the slot's square
/// and smoothed at its edge, so a sweep at 2.4 of 12 seconds is one frame of it drawn black over
/// the icon. A hundred and twenty frames is a step every three degrees, too fine to see at the
/// slot's size. The glint is a soft diagonal band in a strip three wells wide; sliding a well-sized
/// window along it sweeps the band across a slot.
/// </para>
/// </remarks>
public class HudIcons
{
    /// <summary>The index of the sheets, as the content folder names it.</summary>
    public const string IndexPath = "Combat/icons.json";

    /// <summary>What the index's format line says, and what this reads.</summary>
    private const string Format = "LegendQuest hud icons 1";

    /// <summary>How big a sweep's frame is (a hotbar well's size on screen), and how many frames there are.</summary>
    public const int SweepSize = 48;
    public const int SweepFrames = 120;
    private const int SweepColumns = 12;

    /// <summary>How many samples a side each sweep pixel is smoothed from.</summary>
    private const int SweepSamples = 4;

    /// <summary>How wide the glint's band is, in pixels, and how far it leans.</summary>
    private const float GlintWidth = 7f;
    private const float GlintLean = 0.6f;

    /// <summary>The icon drawn for an id with no icon of its own: a buff's or a debuff's.</summary>
    public const string BuffFallback = "buff";
    public const string DebuffFallback = "debuff";

    private readonly Dictionary<string, (Texture2D Sheet, Rectangle Source)> _icons = new(StringComparer.Ordinal);
    private readonly HashSet<string> _passives = new(StringComparer.Ordinal);
    private readonly List<string> _ids = new();

    /// <summary>An icon's size, in art pixels.</summary>
    public int Size { get; private set; } = 24;

    /// <summary>Every id there's an icon for, the sheets' order.</summary>
    public IReadOnlyList<string> Ids => _ids;

    /// <summary>The fan of veils the cooldown sweeps are cut from.</summary>
    public Texture2D Sweep { get; private set; }

    /// <summary>The strip a ready slot's glint slides along: three wells wide, the band in its middle.</summary>
    public Texture2D Glint { get; private set; }

    /// <summary>Loads the index and its sheets, and makes the sweep and the glint.</summary>
    /// <param name="content">The content manager to load with.</param>
    /// <param name="device">The graphics device, for the textures made here.</param>
    /// <exception cref="InvalidOperationException">If the index can't be read, or names a sheet too small for its icons.</exception>
    public void LoadContent(ContentManager content, GraphicsDevice device)
    {
        IconsFile file;
        using (Stream stream = TitleContainer.OpenStream(Path.Combine(content.RootDirectory, IndexPath)))
        {
            try
            {
                file = JsonSerializer.Deserialize<IconsFile>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException unreadable)
            {
                throw new InvalidOperationException($"Content/{IndexPath} isn't readable: {unreadable.Message}", unreadable);
            }
        }
        if (file?.Format != Format)
            throw new InvalidOperationException($"Content/{IndexPath} says it's \"{file?.Format}\", not \"{Format}\". Run tools/generate_combat_art.py.");

        Size = file.Size;
        foreach (IconsFile.SheetEntry entry in file.Sheets)
        {
            var sheet = content.Load<Texture2D>(entry.Asset);
            int rows = (entry.Icons.Length + file.Columns - 1) / file.Columns;
            if (sheet.Width < file.Columns * Size || sheet.Height < rows * Size)
                throw new InvalidOperationException($"{entry.Asset}.png is {sheet.Width}x{sheet.Height}, too small for its {entry.Icons.Length} icons. Run tools/generate_combat_art.py.");
            for (int i = 0; i < entry.Icons.Length; i++)
            {
                var source = new Rectangle(i % file.Columns * Size, i / file.Columns * Size, Size, Size);
                if (!_icons.TryAdd(entry.Icons[i], (sheet, source)))
                    throw new InvalidOperationException($"Content/{IndexPath} has two icons called {entry.Icons[i]}.");
                _ids.Add(entry.Icons[i]);
            }
        }
        foreach (string passive in file.Passives ?? Array.Empty<string>()) _passives.Add(passive);

        Sweep = MakeSweep(device);
        Glint = MakeGlint(device);
    }

    /// <summary>Whether there's an icon with an id.</summary>
    /// <param name="id">The id.</param>
    public bool Has(string id) => id is not null && _icons.ContainsKey(id);

    /// <summary>Whether an id is a passive skill's (its icon has a gold rim).</summary>
    /// <param name="id">The id.</param>
    public bool IsPassive(string id) => id is not null && _passives.Contains(id);

    /// <summary>Draws an icon by id, or the fallback's if there's none with that id.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, point-sampled.</param>
    /// <param name="id">Which icon.</param>
    /// <param name="at">Its top left, in screen pixels.</param>
    /// <param name="scale">How much it's blown up: 1 in an aura row, 2 on the hotbar.</param>
    /// <param name="colour">What to multiply it by.</param>
    /// <param name="fallback">The icon to draw if there's none called id.</param>
    public void Draw(SpriteBatch spriteBatch, string id, Vector2 at, float scale, Color colour, string fallback = BuffFallback)
    {
        if (!_icons.TryGetValue(id ?? string.Empty, out (Texture2D Sheet, Rectangle Source) icon) && !_icons.TryGetValue(fallback ?? string.Empty, out icon))
            return;
        spriteBatch.Draw(icon.Sheet, at, icon.Source, colour, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    /// <summary>The frame of the sweep that veils a share of a well, clockwise from twelve to the end.</summary>
    /// <param name="remaining">How much of the circle is still veiled, 0 to 1.</param>
    /// <returns>Its place on Sweep, or empty for none.</returns>
    public static Rectangle SweepFrame(float remaining)
    {
        int frame = (int)MathF.Ceiling(MathHelper.Clamp(remaining, 0f, 1f) * SweepFrames);
        if (frame <= 0) return Rectangle.Empty;
        int index = frame - 1;
        return new Rectangle(index % SweepColumns * SweepSize, index / SweepColumns * SweepSize, SweepSize, SweepSize);
    }

    /// <summary>Draws the sweep over a well: black, the share still to go veiled, and a pale hand along its leading edge.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="pixel">One white pixel.</param>
    /// <param name="well">The well, in screen pixels.</param>
    /// <param name="remaining">How much of the circle is still veiled, 0 to 1.</param>
    /// <param name="veil">How dark the veil is.</param>
    /// <param name="hand">How bright the hand along its edge is, or 0 for none.</param>
    public void DrawSweep(SpriteBatch spriteBatch, Texture2D pixel, Rectangle well, float remaining, float veil, float hand)
    {
        Rectangle frame = SweepFrame(remaining);
        if (frame.IsEmpty) return;
        spriteBatch.Draw(Sweep, well, frame, Color.Black * veil);
        if (hand <= 0f || remaining >= 1f) return;

        // The hand: from the middle out to the square's edge, along the veil's leading edge.
        float angle = (1f - remaining) * MathHelper.TwoPi - MathHelper.PiOver2;
        var centre = new Vector2(well.Center.X, well.Center.Y);
        var way = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        float reach = well.Width / 2f / MathF.Max(MathF.Abs(way.X), MathF.Abs(way.Y));
        spriteBatch.Draw(pixel, centre, null, Color.White * hand, angle, new Vector2(0f, 0.5f), new Vector2(reach, 2f), SpriteEffects.None, 0f);
    }

    /// <summary>The fan of veils: frame k covers the last k/SweepFrames of the circle, clockwise from twelve.</summary>
    private static Texture2D MakeSweep(GraphicsDevice device)
    {
        int rows = (SweepFrames + SweepColumns - 1) / SweepColumns;
        int width = SweepColumns * SweepSize, height = rows * SweepSize;
        var pixels = new Color[width * height];

        // Each pixel's samples' angles, clockwise from twelve as a share of the circle, once.
        int samples = SweepSamples * SweepSamples;
        var turns = new float[SweepSize * SweepSize * samples];
        float half = SweepSize / 2f;
        for (int y = 0; y < SweepSize; y++)
            for (int x = 0; x < SweepSize; x++)
                for (int s = 0; s < samples; s++)
                {
                    float sx = x + (s % SweepSamples + 0.5f) / SweepSamples - half;
                    float sy = y + (s / SweepSamples + 0.5f) / SweepSamples - half;
                    float turn = MathF.Atan2(sx, -sy) / MathHelper.TwoPi;
                    turns[(y * SweepSize + x) * samples + s] = turn < 0f ? turn + 1f : turn;
                }

        for (int frame = 0; frame < SweepFrames; frame++)
        {
            float from = 1f - (frame + 1f) / SweepFrames;
            int ox = frame % SweepColumns * SweepSize, oy = frame / SweepColumns * SweepSize;
            for (int y = 0; y < SweepSize; y++)
                for (int x = 0; x < SweepSize; x++)
                {
                    int covered = 0;
                    for (int s = 0; s < samples; s++)
                        if (turns[(y * SweepSize + x) * samples + s] >= from) covered++;
                    byte a = (byte)(255 * covered / samples);
                    pixels[(oy + y) * width + ox + x] = new Color(a, a, a, a);
                }
        }

        var texture = new Texture2D(device, width, height);
        texture.SetData(pixels);
        return texture;
    }

    /// <summary>The glint's strip: three wells wide, a soft white band leaning across its middle.</summary>
    private static Texture2D MakeGlint(GraphicsDevice device)
    {
        int width = SweepSize * 3, height = SweepSize;
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float across = x - width / 2f + (y - height / 2f) * GlintLean;
                float a = MathF.Exp(-(across * across) / (GlintWidth * GlintWidth));
                byte v = (byte)(255 * a);
                pixels[y * width + x] = new Color(v, v, v, v);
            }
        var texture = new Texture2D(device, width, height);
        texture.SetData(pixels);
        return texture;
    }

    /// <summary>Combat/icons.json as written by tools/generate_combat_art.py.</summary>
    private sealed class IconsFile
    {
        public string Format { get; set; }
        public int Size { get; set; }
        public int Columns { get; set; }
        public SheetEntry[] Sheets { get; set; }
        public string[] Passives { get; set; }

        public sealed class SheetEntry
        {
            public string Asset { get; set; }
            public string[] Icons { get; set; }
        }
    }
}
