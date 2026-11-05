namespace LegendQuest.Dialogue;

/// <summary>
/// Which way a cue turns someone: left or right, the two ways a side-on figure faces.
/// </summary>
/// <remarks>
/// A tile map's figures can face up and down too, and turning toward someone can end up either; a
/// cue never asks for those, so a scene reads the same whatever it's played on.
/// </remarks>
public enum CueFacing
{
    /// <summary>Facing left.</summary>
    Left,

    /// <summary>Facing right.</summary>
    Right,
}
