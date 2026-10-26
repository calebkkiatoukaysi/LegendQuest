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

    /// <summary>Offset drop shadow sitting just behind a line of text.</summary>
    public const float TextShadow = 0.12f;

    /// <summary>Foreground text.</summary>
    public const float Text = 0.10f;
}
