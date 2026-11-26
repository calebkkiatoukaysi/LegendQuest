using System;

namespace LegendQuest;

/// <summary>Which look the world is drawn in.</summary>
/// <remarks>Saved in settings.json by name, so reordering here is safe but renaming one puts it back to the default.</remarks>
public enum ArtLook
{
    /// <summary>The smooth painted figures at the screen's own resolution, as the game was first drawn.</summary>
    Painted,

    /// <summary>True pixel art: the world drawn at 800 by 450 art pixels and blown up twice with hard edges, the figures posed twelve times a second.</summary>
    Pixel,
}

/// <summary>
/// The look the world is drawn in and the numbers the pixel look is built on
/// (docs/pixel-art-brief.md §1): how big the art-pixel world is, how many screen pixels each art
/// pixel covers, and how often a figure's pose changes.
/// </summary>
/// <remarks>
/// The look is the player's setting (Settings.ArtLook), read as the game starts and changed on the
/// options screen; LQ_LOOK=Painted or LQ_LOOK=Pixel in the environment overrides it, so a proof run
/// can film either look whatever the settings file says. Only how things are drawn depends on it:
/// the clips, their hit and release times and everything the fight's engine reads are the same in
/// both.
/// </remarks>
public static class ArtSettings
{
    /// <summary>How many screen pixels an art pixel is across in the pixel look.</summary>
    public const int ArtScale = 2;

    /// <summary>The world's size in art pixels in the pixel look.</summary>
    public const int ArtWidth = LegendQuestGame.ScreenWidth / ArtScale, ArtHeight = LegendQuestGame.ScreenHeight / ArtScale;

    /// <summary>How many poses a second a figure is drawn in in the pixel look, like a hand-drawn sprite's frames. Where it stands still moves every frame.</summary>
    public const float PoseRate = 12f;

    /// <summary>The look when the settings don't say.</summary>
    public const ArtLook Default = ArtLook.Pixel;

    /// <summary>The look the world is drawn in now.</summary>
    public static ArtLook Look { get; set; } = FromEnvironment() ?? Default;

    /// <summary>Whether the world is drawn as pixel art.</summary>
    public static bool Pixel => Look == ArtLook.Pixel;

    /// <summary>Takes the look from the settings, unless the environment names one.</summary>
    /// <param name="saved">The settings' look.</param>
    public static void Apply(ArtLook saved) => Look = FromEnvironment() ?? saved;

    /// <summary>The look LQ_LOOK names, or null if it isn't set or isn't a look.</summary>
    private static ArtLook? FromEnvironment() =>
        Enum.TryParse(Environment.GetEnvironmentVariable("LQ_LOOK"), true, out ArtLook look) && Enum.IsDefined(look) ? look : null;
}
