namespace LegendQuest;

/// <summary>
/// Layer depths for SpriteBatch in BackToFront mode. 1 is the back, 0 is the front.
/// </summary>
/// <remarks>
/// Depth only sorts within one Begin/End. Anything additive (the torch light, the embers) needs
/// its own batch, so the batch order sorts first and these sort inside each batch.
/// </remarks>
public static class Layers
{
    /// <summary>The gate behind the title screen. Behind everything.</summary>
    public const float TitleScene = 1.00f;

    /// <summary>A torch's flame, over the wall its sconce is on.</summary>
    public const float Flame = 0.90f;

    /// <summary>The dimming pane a form is laid over. Back of the form's batch.</summary>
    public const float Veil = 0.60f;

    /// <summary>The iron slab a form is built on, over the veil.</summary>
    public const float PanelPlate = 0.50f;

    /// <summary>The iron plate of a button.</summary>
    public const float ButtonPlate = 0.18f;

    /// <summary>The light coming out of a button's groove, over its own plate.</summary>
    public const float ButtonAccent = 0.17f;

    /// <summary>Offset drop shadow behind a button's label.</summary>
    public const float ButtonLabelShadow = 0.16f;

    /// <summary>A button's label.</summary>
    public const float ButtonLabel = 0.15f;

    /// <summary>A soft dark smudge behind a line of text, so it reads over anything.</summary>
    public const float TextBacking = 0.14f;

    /// <summary>Offset drop shadow sitting just behind a line of text.</summary>
    public const float TextShadow = 0.12f;

    /// <summary>Foreground text.</summary>
    public const float Text = 0.10f;

    // A map. It's one world space batch of its own, sorted by how far down the map things are.

    /// <summary>The ground layer of a map. Behind everything.</summary>
    public const float GroundTile = 1.00f;

    /// <summary>The detail layer, over the ground: fences, flowers, rubble.</summary>
    public const float DetailTile = 0.98f;

    /// <summary>Things on walls, like the torches. Behind everyone on the floor.</summary>
    public const float WorldWall = 0.96f;

    /// <summary>A flame in a wall sconce, over the sconce and still behind everyone on the floor.</summary>
    public const float WallFlame = 0.95f;

    /// <summary>The back and front of the range that people and props get sorted into.</summary>
    private const float WorldFar = 0.90f;
    private const float WorldNear = 0.10f;

    /// <summary>Overhead tiles: roofs and canopies, in front of everyone.</summary>
    public const float Overhead = 0.04f;

    /// <summary>The layer for something standing on the floor. The further down the map, the more in front it is.</summary>
    /// <param name="footY">How far down the map its feet are, in world pixels.</param>
    /// <param name="mapHeight">How tall the map is, in world pixels.</param>
    public static float OnFloor(float footY, float mapHeight) =>
        WorldFar - (WorldFar - WorldNear) * System.Math.Clamp(footY / mapHeight, 0f, 1f);
}
