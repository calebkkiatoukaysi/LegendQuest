using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// The card a chapter opens on: the act it's in, in small gold capitals, and the chapter's name big under it,
/// on a soft dark smudge across the middle of the screen.
/// </summary>
/// <remarks>
/// The ship coming in ends on Chapter 1's, and every chapter after it opens on a conversation line
/// from Speakers.Chapter, which DialogueScreen draws with this, so they all look the same.
/// </remarks>
public class ChapterCard
{
    /// <summary>The acts, over the chapter's name, by the act meter: none before the first, and none on the Prologue's own card.</summary>
    private static readonly string[] Acts = { null, "ACT 1  ·  THE HUNGER AT THE HEART", "ACT 2  ·  THE LONG ECLIPSE", "ACT 3  ·  SOUL-BOUND" };

    /// <summary>How the Prologue's card starts, which goes without an act over it.</summary>
    private const string Prologue = "Prologue";

    /// <summary>How far above the middle of the screen the top of the card is, in screen pixels.</summary>
    private const float Rise = 80f;

    /// <summary>How tall the smudge is, and the room between the act and the chapter's name, in screen pixels.</summary>
    private const int Height = 140;
    private const float Gap = 10f;

    /// <summary>How wide the smudge is, as so many times the chapter's name in the small face: a good bit wider than the name's written, so it fades out clear of it.</summary>
    private const float Breadth = 2f;

    /// <summary>The smudge behind it, and how wide its soft round ends are, in screen pixels.</summary>
    private static readonly Color Backing = Color.Black * 0.55f;
    private const int Ends = 90;

    private SpriteFont _small;
    private SpriteFont _big;
    private Texture2D _glow;

    /// <summary>Loads the two faces and the glow the smudge is made from.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _small = content.Load<SpriteFont>("spectral-detail");
        _big = content.Load<SpriteFont>("spectral-ui");
        _glow = content.Load<Texture2D>("glow");
    }

    /// <summary>Draws the card in the middle of the screen.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in screen space, linear-sampled.</param>
    /// <param name="chapter">The chapter's name, like "Chapter 2  ·  The Rot in the Mirewood".</param>
    /// <param name="act">The act it's in, from the act meter.</param>
    /// <param name="shown">How much of it is showing, 0 to 1, with the screen's own fade in it.</param>
    public void Draw(SpriteBatch spriteBatch, string chapter, int act, float shown)
    {
        float top = LegendQuestGame.ScreenHeight / 2f - Rise;
        float width = _small.MeasureString(chapter).X * Breadth;
        var area = new Rectangle((int)((LegendQuestGame.ScreenWidth - width) / 2f), (int)top, (int)width, Height);

        Text.DrawBacking(spriteBatch, _glow, area, Ends, Backing * shown);
        string over = chapter.StartsWith(Prologue) || act < 0 || act >= Acts.Length ? null : Acts[act];
        if (over is not null) Text.DrawCentered(spriteBatch, _small, over, top, Palette.Gold * shown, Palette.Shadow * shown);
        Text.DrawCentered(spriteBatch, _big, chapter, top + _small.LineSpacing + Gap, Palette.Bone * shown, Palette.Shadow * shown);
    }
}
