using System;
using System.Text.Json.Serialization;

namespace LegendQuest.Save;

/// <summary>
/// The settings that aren't part of a game: the three volumes, full screen or not, how fast text types out, whether the tutorial's tips come up, how hard the fights are, and how much a fight shakes, flashes and shows its numbers.
/// </summary>
/// <remarks>Not in the save slots on purpose. Turning the music down shouldn't depend on which game you loaded, and the difficulty's changeable any time outside a fight (build plan §10).</remarks>
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

    /// <summary>Whether the tutorial's tips come up as you play. On to start with; a file from before there were tips doesn't have it, and gets it on.</summary>
    public bool TutorialTips { get; set; } = true;

    /// <summary>How hard the fights are, by name. Balanced to start with; a file from before there was a choice doesn't have it, and gets Balanced.</summary>
    public string Difficulty { get; set; } = nameof(LegendQuest.Difficulty.Balanced);

    /// <summary>How hard the screen shakes in a fight, by name (ShakeStrength): Full to start with; Reduced is a third as much, Off none. A file from before there was a choice doesn't have it, and gets Full.</summary>
    public string ScreenShake { get; set; } = nameof(LegendQuest.Components.ShakeStrength.Full);

    /// <summary>Whether someone hit in a fight flashes white. On to start with; off for anyone the flashing bothers.</summary>
    public bool HitFlashes { get; set; } = true;

    /// <summary>Whether the damage and healing numbers pop up over people in a fight. On to start with.</summary>
    public bool DamageNumbers { get; set; } = true;

    /// <summary>ScreenShake as the enum, or Full if the file has something it doesn't recognise. Setting it sets ScreenShake by name.</summary>
    [JsonIgnore]
    public LegendQuest.Components.ShakeStrength Shake
    {
        get => Enum.TryParse(ScreenShake, out LegendQuest.Components.ShakeStrength shake) && Enum.IsDefined(shake) ? shake : LegendQuest.Components.ShakeStrength.Full;
        set => ScreenShake = value.ToString();
    }

    /// <summary>TextSpeed as the enum, or Normal if the file has something it doesn't recognise.</summary>
    [JsonIgnore]
    public TextSpeed Typing =>
        Enum.TryParse(TextSpeed, out TextSpeed speed) && Enum.IsDefined(speed) ? speed : LegendQuest.TextSpeed.Normal;

    /// <summary>Difficulty as the enum, or Balanced if the file has something it doesn't recognise. Setting it sets Difficulty by name.</summary>
    [JsonIgnore]
    public Difficulty Mode
    {
        get => Enum.TryParse(Difficulty, out Difficulty mode) && Enum.IsDefined(mode) ? mode : LegendQuest.Difficulty.Balanced;
        set => Difficulty = value.ToString();
    }

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
