using System;
using System.Text.Json.Serialization;

namespace LegendQuest.Save;

/// <summary>
/// The settings that aren't part of a game: the three volumes, full screen or not, and how fast text types out.
/// </summary>
/// <remarks>Not in the save slots on purpose. Turning the music down shouldn't depend on which game you loaded.</remarks>
public class Settings
{
    /// <summary>Everything. Gets multiplied into the other two.</summary>
    public float MasterVolume { get; set; } = 0.8f;

    /// <summary>The songs.</summary>
    public float MusicVolume { get; set; } = 0.7f;

    /// <summary>Every sound effect.</summary>
    public float SfxVolume { get; set; } = 0.9f;

    /// <summary>Whether the game takes up the whole screen. F11 switches it too.</summary>
    public bool FullScreen { get; set; }

    /// <summary>How fast dialogue types out, by name.</summary>
    public string TextSpeed { get; set; } = nameof(LegendQuest.TextSpeed.Normal);

    /// <summary>TextSpeed as the enum, or Normal if the file has something it doesn't recognise.</summary>
    [JsonIgnore]
    public TextSpeed Typing =>
        Enum.TryParse(TextSpeed, out TextSpeed speed) && Enum.IsDefined(speed) ? speed : LegendQuest.TextSpeed.Normal;

    /// <summary>Clamps every volume to 0 to 1, in case the file got edited by hand.</summary>
    /// <returns>This, so it can be chained.</returns>
    public Settings Clamped()
    {
        MasterVolume = Math.Clamp(MasterVolume, 0f, 1f);
        MusicVolume = Math.Clamp(MusicVolume, 0f, 1f);
        SfxVolume = Math.Clamp(SfxVolume, 0f, 1f);
        return this;
    }
}
