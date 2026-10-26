using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.StateManagement;

namespace LegendQuest.Components;

/// <summary>
/// A clickable iron plate with light coming out of the groove round its edge. Every menu entry is one.
/// </summary>
/// <remarks>
/// The Black Box's button with the concrete swapped for iron. button.png has two bands on the same
/// grid: the plate on top, and a white mask of the light in the groove under it, which gets tinted
/// with Accent. That's how one sheet does the torch, ember and bone buttons. Every frame is
/// nine-sliced so the button can be any size without the corners stretching.
/// </remarks>
public class ButtonSprite
{
    /// <summary>The label before any light falls on it, live and greyed out, and how far it warms toward the accent when lit.</summary>
    private static readonly Color LabelCold = new(200, 190, 180);
    private static readonly Color LabelDisabled = new(98, 92, 88);
    private const float LabelLitBlend = 0.55f;

    /// <summary>The detail line: dimmer than the label, and it warms less and later.</summary>
    private const float DetailDim = 0.72f;
    private const float DetailLitBlend = 0.35f;
    private const float DetailKindle = 0.7f;

    /// <summary>Width and height of one frame in button.png.</summary>
    private const int FrameSize = 24;

    /// <summary>The fixed corner of each frame. Has to match BUTTON_CORNER in tools/generate_ui.py.</summary>
    private const int CornerSize = 8;

    /// <summary>Rows in the sheet, which is also the number of animation states.</summary>
    private const int SheetRows = 4;

    /// <summary>Distance down the sheet to the light band, which mirrors the plate band.</summary>
    private const int AccentBandOffset = FrameSize * SheetRows;

    // Rows of the sheet, in the order they are drawn.
    private const int RowIdle = 0;
    private const int RowHover = 1;
    private const int RowPress = 2;
    private const int RowDisabled = 3;

    private const int IdleFrames = 4;
    private const int HoverFrames = 6;
    private const int PressFrames = 4;

    /// <summary>The art is small and blown up by a whole number to stay crisp. 3x, same as the world.</summary>
    private const float DrawScale = 3f;

    /// <summary>Seconds per frame of the idle smoulder. Slow, so it never reads as a prompt.</summary>
    private const double IdleFrameTime = 0.17;

    /// <summary>How fast the plate kindles under the cursor, in hover-animation per second.</summary>
    private const float KindleRate = 6.5f;

    /// <summary>How fast it goes out again. Slower than it lit, so the light lingers.</summary>
    private const float CoolRate = 4.5f;

    /// <summary>How fast the plate sinks while held, in press-animation per second.</summary>
    private const float SinkRate = 14f;

    /// <summary>How fast it comes back out on release. Quick, or the button feels sticky.</summary>
    private const float RiseRate = 9f;

    /// <summary>How far the plate travels in when fully pressed, in screen pixels.</summary>
    private const float SinkDistance = 3f;

    /// <summary>Padding around the label when a button is left to size itself.</summary>
    private static readonly Point LabelPadding = new(54, 26);

    /// <summary>Gap between the label and the detail line under it, in screen pixels.</summary>
    private const float SublabelGap = 4f;

    private Texture2D _texture;
    private SpriteFont _font;
    private SpriteFont _detailFont;

    /// <summary>How far through the hover animation the plate is, 0 cold to 1 fully kindled.</summary>
    private float _kindle;

    /// <summary>How far through the press animation the plate is, 0 out to 1 fully sunk.</summary>
    private float _sink;

    private double _idleTimer;
    private bool _hovered;

    /// <summary>Whether the press that is currently down started on this button.</summary>
    private bool _armed;

    /// <summary>The line drawn across the plate.</summary>
    public string Label;

    /// <summary>An optional smaller line under the label, for things like what a save slot holds.</summary>
    public string Sublabel;

    /// <summary>Centre of the button in screen space.</summary>
    public Vector2 Center;

    /// <summary>Size in screen pixels. Left at zero, the button sizes itself to its label in LoadContent.</summary>
    public Point Size;

    /// <summary>Colour of the light coming out of the groove.</summary>
    public Color Accent;

    /// <summary>Whether the button responds to the cursor at all.</summary>
    public bool Enabled = true;

    /// <summary>Whether the keyboard is on this button. It lights up the same as a hover.</summary>
    public bool Focused;

    /// <summary>How visible the button is, 0 to 1. Screens fade it with their transition.</summary>
    public float Opacity = 1f;

    /// <summary>Raised the moment a press that started on this button is released on it.</summary>
    public event Action Clicked;

    /// <summary>Whether the button is being held down right now.</summary>
    private bool Held => _armed && _hovered;

    /// <summary>The area the button covers, in screen pixels.</summary>
    public Rectangle Bounds => new(
        (int)MathF.Round(Center.X - Size.X / 2f),
        (int)MathF.Round(Center.Y - Size.Y / 2f),
        Size.X,
        Size.Y);

    /// <summary>Creates a button. Leave size unset to have it fit its own label.</summary>
    /// <param name="label">The line drawn across the plate.</param>
    /// <param name="center">Centre of the button in screen space.</param>
    /// <param name="accent">Colour of the light in the groove.</param>
    /// <param name="size">Size in screen pixels, or default to size to the label.</param>
    public ButtonSprite(string label, Vector2 center, Color accent, Point size = default)
    {
        Label = label;
        Center = center;
        Accent = accent;
        Size = size;
    }

    /// <summary>Loads the button sheet and the fonts.</summary>
    /// <param name="content">The ContentManager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _texture = content.Load<Texture2D>("button");
        _font = content.Load<SpriteFont>("spectral-ui");
        _detailFont = content.Load<SpriteFont>("spectral-detail");

        if (Size == Point.Zero)
        {
            Vector2 measured = MeasureText();
            Size = new Point(
                (int)MathF.Round(measured.X) + LabelPadding.X,
                (int)MathF.Round(measured.Y) + LabelPadding.Y);
        }

        // Never smaller than two corners, or the nine-slice would have to crop the art.
        int minimum = (int)MathF.Round(CornerSize * 2 * DrawScale);
        Size = new Point(Math.Max(Size.X, minimum), Math.Max(Size.Y, minimum));
    }

    /// <summary>Puts the button back in its cold, unpressed state.</summary>
    /// <remarks>Called when a menu gets focus back, so a press that started while it was covered can't finish on it.</remarks>
    public void Reset()
    {
        _kindle = 0f;
        _sink = 0f;
        _hovered = false;
        _armed = false;
    }

    /// <summary>Presses the button from the keyboard. The plate sinks and pops back up, same as a click.</summary>
    public void Activate()
    {
        if (!Enabled) return;

        _sink = 1f;
        Clicked?.Invoke();
    }

    /// <summary>Advances the animation and works out whether the button was clicked this frame.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    /// <param name="input">The input this frame, for the mouse.</param>
    public void Update(GameTime gameTime, InputState input)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _hovered = Enabled && Bounds.Contains(input.MousePosition);

        if (!Enabled)
        {
            // Drop everything in progress so a button disabled mid-press doesn't come back holding a click.
            _armed = false;
            _kindle = 0f;
            _sink = 0f;
        }
        else
        {
            // A click has to start and finish on the same plate, like any normal button.
            if (input.IsNewLeftClick && _hovered) _armed = true;

            if (input.IsLeftReleased)
            {
                if (_armed && _hovered) Clicked?.Invoke();
                _armed = false;
            }

            bool lit = _hovered || Focused;
            _kindle = Approach(_kindle, lit ? 1f : 0f, (lit ? KindleRate : CoolRate) * elapsed);
            _sink = Approach(_sink, Held ? 1f : 0f, (Held ? SinkRate : RiseRate) * elapsed);
        }

        _idleTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_idleTimer >= IdleFrameTime * IdleFrames) _idleTimer -= IdleFrameTime * IdleFrames;
    }

    /// <summary>Draws the plate, the light in its groove, and the label across it.</summary>
    /// <param name="gameTime">The game time.</param>
    /// <param name="spriteBatch">The SpriteBatch to render with. Point-sampled.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        Point frame = CurrentFrame();
        Rectangle plate = Bounds;

        // A held plate moves with its light and label, so the whole thing sinks in.
        if (_sink > 0f)
        {
            int travel = (int)MathF.Round(_sink * SinkDistance);
            plate.Offset(travel, travel);
        }

        DrawPlate(spriteBatch, plate, frame, Color.White * Opacity, Layers.ButtonPlate);
        DrawPlate(spriteBatch, plate, new Point(frame.X, frame.Y + AccentBandOffset), Accent * Opacity, Layers.ButtonAccent);
        DrawLabel(spriteBatch, plate);
    }

    /// <summary>Picks the frame to draw, as its top-left corner in the sheet's plate band.</summary>
    /// <remarks>The hover row is a ramp, not a loop, so running _kindle back down plays it in reverse. No un-hover animation needed.</remarks>
    private Point CurrentFrame()
    {
        if (!Enabled) return new Point(0, RowDisabled * FrameSize);

        if (_sink > 0f)
            return new Point(Index(_sink, PressFrames) * FrameSize, RowPress * FrameSize);

        if (_kindle > 0f)
            return new Point(Index(_kindle, HoverFrames) * FrameSize, RowHover * FrameSize);

        int idle = (int)(_idleTimer / IdleFrameTime) % IdleFrames;
        return new Point(idle * FrameSize, RowIdle * FrameSize);
    }

    /// <summary>Draws one frame of the sheet over the whole plate, nine-sliced.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="destination">The area to cover, in screen pixels.</param>
    /// <param name="origin">Top-left of the frame in the sheet.</param>
    /// <param name="tint">Colour multiplied into the frame.</param>
    /// <param name="layerDepth">Sort key for the batch. See Layers.</param>
    private void DrawPlate(SpriteBatch spriteBatch, Rectangle destination, Point origin, Color tint, float layerDepth) =>
        NineSlice.Draw(spriteBatch, _texture, destination, origin, FrameSize, CornerSize, DrawScale, tint, layerDepth);

    /// <summary>Measures the label, plus the detail line if there is one.</summary>
    private Vector2 MeasureText()
    {
        Vector2 size = _font.MeasureString(Label ?? string.Empty);
        if (string.IsNullOrEmpty(Sublabel)) return size;

        Vector2 detail = _detailFont.MeasureString(Sublabel);
        return new Vector2(MathF.Max(size.X, detail.X), size.Y + SublabelGap + detail.Y);
    }

    /// <summary>Draws the label, and the detail line under it if there is one, centred on the plate.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="plate">The plate, already offset by any press.</param>
    private void DrawLabel(SpriteBatch spriteBatch, Rectangle plate)
    {
        if (string.IsNullOrEmpty(Label)) return;

        // The label warms toward the accent as the plate lights up.
        Color cold = Enabled ? LabelCold : LabelDisabled;
        Color lit = Color.Lerp(cold, Color.Lerp(Accent, Color.White, LabelLitBlend), _kindle);

        // Centre the whole block, so a detail line pushes the label up instead of hanging off it.
        float fit = LabelFit;
        Vector2 label = _font.MeasureString(Label) * fit;
        float height = string.IsNullOrEmpty(Sublabel) ? label.Y : label.Y + SublabelGap + _detailFont.MeasureString(Sublabel).Y;
        float top = MathF.Round(plate.Y + (plate.Height - height) / 2f);

        DrawLine(spriteBatch, _font, Label, plate, top, lit, fit, Opacity);

        if (string.IsNullOrEmpty(Sublabel)) return;

        // Dimmer than the label so it doesn't compete with it.
        Color detail = Color.Lerp(cold * DetailDim, Color.Lerp(Accent, Color.White, DetailLitBlend), _kindle * DetailKindle);
        DrawLine(spriteBatch, _detailFont, Sublabel, plate, top + label.Y + SublabelGap, detail, 1f, Opacity);
    }

    /// <summary>How far a label may be shrunk to fit a fixed-size plate, so a long one never runs off the edges.</summary>
    /// <remarks>The margin is the corner art, which a label shouldn't sit over.</remarks>
    private float LabelFit
    {
        get
        {
            float room = Size.X - CornerSize * 2 * DrawScale;
            float width = _font.MeasureString(Label ?? string.Empty).X;
            return width > room ? room / width : 1f;
        }
    }

    /// <summary>Draws one line of text centred across the plate, with a shadow.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="font">The SpriteFont to measure and render with.</param>
    /// <param name="text">The line to draw.</param>
    /// <param name="plate">The plate to centre across.</param>
    /// <param name="y">Top of the line, in screen pixels.</param>
    /// <param name="color">Colour of the text.</param>
    /// <param name="scale">How far the line is shrunk to fit. 1 is full size.</param>
    /// <param name="opacity">How visible it is, 0 to 1.</param>
    private static void DrawLine(SpriteBatch spriteBatch, SpriteFont font, string text, Rectangle plate, float y, Color color,
        float scale, float opacity)
    {
        Vector2 size = font.MeasureString(text) * scale;
        var position = new Vector2(MathF.Round(plate.X + (plate.Width - size.X) / 2f), MathF.Round(y));

        spriteBatch.DrawString(font, text, position + Palette.ShadowOffset, Palette.LabelShadow * opacity,
            0f, Vector2.Zero, scale, SpriteEffects.None, Layers.ButtonLabelShadow);
        spriteBatch.DrawString(font, text, position, color * opacity,
            0f, Vector2.Zero, scale, SpriteEffects.None, Layers.ButtonLabel);
    }

    /// <summary>Moves value toward target without overshooting.</summary>
    private static float Approach(float value, float target, float step)
    {
        if (value < target) return MathF.Min(target, value + step);
        return MathF.Max(target, value - step);
    }

    /// <summary>Maps a 0..1 animation position onto a frame index, holding the last frame instead of wrapping.</summary>
    private static int Index(float progress, int frames) =>
        Math.Clamp((int)(progress * frames), 0, frames - 1);
}
