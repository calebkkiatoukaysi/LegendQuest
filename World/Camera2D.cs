using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// The map camera. It eases after whoever you're controlling, never shows past the edge of the map,
/// zooms in whole-number steps, and gives SpriteBatch.Begin the matrix for it.
/// </summary>
/// <remarks>
/// The Black Box's Camera2D, which was Matrix.CreateTranslation from the parallax tutorial following
/// the player. The difference is that the world is in art pixels now and the camera does the scaling,
/// so zoom is just the scale: 2x, 3x or 4x, never anything in between, so every art pixel stays a
/// square of whole screen pixels. The offset is still rounded to whole screen pixels, because the
/// tiles shimmer otherwise.
/// </remarks>
public class Camera2D
{
    /// <summary>How fast the camera catches up to its target. Higher is tighter.</summary>
    private const float Stiffness = 7f;

    /// <summary>The furthest out the wheel goes. Shows more of the map.</summary>
    private const int MinZoom = 2;

    /// <summary>The furthest in the wheel goes, for looking closely.</summary>
    private const int MaxZoom = 4;

    /// <summary>Where the zoom starts. 3x is the size all the art is drawn for.</summary>
    private const int DefaultZoom = 3;

    /// <summary>How big the view is, in screen pixels.</summary>
    public Point ViewSize { get; }

    /// <summary>The map in world pixels (art pixels). The view never goes outside it.</summary>
    public Rectangle Bounds { get; set; }

    /// <summary>The world point at the middle of the view.</summary>
    public Vector2 Centre { get; private set; }

    /// <summary>How many screen pixels one art pixel covers.</summary>
    public int Zoom { get; private set; } = DefaultZoom;

    /// <summary>How big one screen pixel is in world pixels. Things snap to this so they move a screen pixel at a time.</summary>
    public float Pixel => 1f / Zoom;

    /// <summary>The top left of the view in screen pixels, rounded so the whole world sits on whole pixels.</summary>
    private Point Offset => new(
        (int)MathF.Round(Centre.X * Zoom - ViewSize.X / 2f),
        (int)MathF.Round(Centre.Y * Zoom - ViewSize.Y / 2f));

    /// <summary>The part of the world on screen right now, in world pixels, rounded out to whole pixels.</summary>
    public Rectangle View
    {
        get
        {
            Point offset = Offset;
            return new Rectangle(offset.X / Zoom, offset.Y / Zoom, ViewSize.X / Zoom + 2, ViewSize.Y / Zoom + 2);
        }
    }

    /// <summary>The world to screen matrix for SpriteBatch.Begin: scale up, then scroll.</summary>
    public Matrix Transform
    {
        get
        {
            Point offset = Offset;
            return Matrix.CreateScale(Zoom, Zoom, 1f) * Matrix.CreateTranslation(-offset.X, -offset.Y, 0f);
        }
    }

    /// <summary>Creates a camera for a view of a given size.</summary>
    /// <param name="viewSize">The size of the window, in screen pixels.</param>
    public Camera2D(Point viewSize)
    {
        ViewSize = viewSize;
    }

    /// <summary>Jumps the camera straight to a point with no easing. Used on the first frame.</summary>
    /// <param name="target">The world point to look at.</param>
    public void SnapTo(Vector2 target) => Centre = Clamp(target);

    /// <summary>Eases the camera toward a point.</summary>
    /// <remarks>Exponential easing, so it moves the same at any frame rate.</remarks>
    /// <param name="target">The world point to follow.</param>
    /// <param name="gameTime">The frame's timing.</param>
    public void Follow(Vector2 target, GameTime gameTime)
    {
        float t = 1f - MathF.Exp(-Stiffness * (float)gameTime.ElapsedGameTime.TotalSeconds);
        Centre = Clamp(Vector2.Lerp(Centre, target, t));
    }

    /// <summary>Zooms in or out by whole steps, staying between MinZoom and MaxZoom.</summary>
    /// <param name="steps">How many steps in (positive) or out (negative).</param>
    /// <returns>True if the zoom changed.</returns>
    public bool ZoomBy(int steps)
    {
        int zoom = Math.Clamp(Zoom + steps, MinZoom, MaxZoom);
        if (zoom == Zoom) return false;

        Zoom = zoom;

        // A different zoom shows a different amount of map, so the edge might be in view now.
        Centre = Clamp(Centre);
        return true;
    }

    /// <summary>Keeps the view inside the map.</summary>
    private Vector2 Clamp(Vector2 centre)
    {
        float halfW = ViewSize.X / 2f / Zoom, halfH = ViewSize.Y / 2f / Zoom;

        // If the map is smaller than the view on an axis, just centre it.
        float x = Bounds.Width <= halfW * 2f ? Bounds.Center.X : Math.Clamp(centre.X, Bounds.Left + halfW, Bounds.Right - halfW);
        float y = Bounds.Height <= halfH * 2f ? Bounds.Center.Y : Math.Clamp(centre.Y, Bounds.Top + halfH, Bounds.Bottom - halfH);
        return new Vector2(x, y);
    }
}
