using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>
/// The slots a character is put together from, in the order they're drawn: back to front.
/// </summary>
/// <remarks>
/// Body and Head are the base's own: its body from the jaw down and its bald head, which is drawn
/// only when there's no face. The rest are the parts a recipe names (Story/13-art-prompts.md §7).
/// The armor covers the whole body below the jaw, so it's drawn over the base's; the face over the
/// armor, since hair and beards fall over it; the weapon last, in the near hand.
/// </remarks>
public enum PartSlot
{
    Cape,
    Body,
    Armor,
    Head,
    Face,
    Helm,
    Weapon,
    Offhand,
}

/// <summary>
/// The sockets every base has: the points on its body the parts hang from, measured off the base by
/// tools/import_parts.py and kept in Content/Characters/Parts/parts.json.
/// </summary>
/// <remarks>
/// <para>
/// Every part is painted in place on a base, so its pivot (its socket, in its own pixels) is where
/// that socket was on the base. Drawn with its pivot on the socket, a part lands on the pixel it was
/// painted on. A morph moves the sockets, not the art: the body stretches away from the root by the
/// morph's height and width, and every other socket goes with the body, so a head sits on a broad
/// neck at its own size and a weapon stays in a hand that's moved.
/// </para>
/// <para>
/// The table here and the importer's SLOT_SOCKET have to agree, and PartCatalog.LoadAll checks that
/// they do.
/// </para>
/// </remarks>
/// <summary>
/// The pieces a figure moves in, back to front: the cape, the body (with the armor), the head (with
/// the face and the helm), and what's in each hand. Each turns about its socket (Sockets.OfBone)
/// on top of the whole figure's lean and breath.
/// </summary>
public enum FigureBone
{
    /// <summary>The cape, swaying from the back.</summary>
    Back,

    /// <summary>The body and the armor, from the root: the whole figure's lean is this.</summary>
    Body,

    /// <summary>The head, the face and the helm, nodding on the neck.</summary>
    Head,

    /// <summary>What's in the near hand, swung from it.</summary>
    Near,

    /// <summary>What's in the far hand, braced on it.</summary>
    Far,
}

public static class Sockets
{
    /// <summary>The ground between the feet. The body scales about it, and the figure stands on it.</summary>
    public const string Root = "root";

    /// <summary>The middle of the jaw seam. The head (face, helm, or the bald head) sits on it.</summary>
    public const string Neck = "neck";

    /// <summary>The top of the head.</summary>
    public const string Crown = "crown";

    /// <summary>The middle of the chest, for what lands on someone: a heal, a hit.</summary>
    public const string Chest = "chest";

    /// <summary>The back at the shoulder line, where a cape hangs from.</summary>
    public const string Back = "back";

    /// <summary>The near hand, the lit one on the left, which holds the weapon.</summary>
    public const string NearHand = "hand.near";

    /// <summary>The far hand, which holds whatever's in the other.</summary>
    public const string FarHand = "hand.far";

    /// <summary>Every socket, in the order the importer writes them.</summary>
    public static readonly string[] All = { Root, Neck, Crown, Chest, Back, NearHand, FarHand };

    /// <summary>The socket a slot's part hangs from.</summary>
    /// <param name="slot">The slot.</param>
    public static string Of(PartSlot slot) => slot switch
    {
        PartSlot.Cape => Back,
        PartSlot.Body or PartSlot.Armor => Root,
        PartSlot.Head or PartSlot.Face or PartSlot.Helm => Neck,
        PartSlot.Weapon => NearHand,
        PartSlot.Offhand => FarHand,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, "a slot with no socket"),
    };

    /// <summary>A slot's name as parts.json writes it.</summary>
    /// <param name="slot">The slot.</param>
    public static string Name(PartSlot slot) => slot.ToString().ToLowerInvariant();

    /// <summary>The piece a slot's part moves with.</summary>
    /// <param name="slot">The slot.</param>
    public static FigureBone BoneOf(PartSlot slot) => slot switch
    {
        PartSlot.Cape => FigureBone.Back,
        PartSlot.Body or PartSlot.Armor => FigureBone.Body,
        PartSlot.Head or PartSlot.Face or PartSlot.Helm => FigureBone.Head,
        PartSlot.Weapon => FigureBone.Near,
        _ => FigureBone.Far,
    };

    /// <summary>The socket a piece turns about.</summary>
    /// <param name="bone">The piece.</param>
    public static string OfBone(FigureBone bone) => bone switch
    {
        FigureBone.Back => Back,
        FigureBone.Body => Root,
        FigureBone.Head => Neck,
        FigureBone.Near => NearHand,
        _ => FarHand,
    };

    /// <summary>The slot a name in parts.json is, if it's one.</summary>
    /// <param name="name">The name.</param>
    /// <param name="slot">The slot.</param>
    public static bool TryParse(string name, out PartSlot slot)
    {
        foreach (PartSlot each in Enum.GetValues<PartSlot>())
        {
            if (Name(each) == name)
            {
                slot = each;
                return true;
            }
        }
        slot = default;
        return false;
    }
}

/// <summary>
/// How much taller and broader than the base someone stands: Story/13-art-prompts.md §7's
/// "h × w". The body stretches by it; a head and a weapon ride on their sockets at sizes of their own.
/// </summary>
/// <param name="Height">How much taller, 1 being the base's height.</param>
/// <param name="Width">How much broader, 1 being the base's build.</param>
public readonly record struct Morph(float Height, float Width)
{
    /// <summary>The base as it is.</summary>
    public static readonly Morph None = new(1f, 1f);

    /// <summary>The least and most a morph can be, either way.</summary>
    public const float Least = 0.3f, Most = 2.5f;

    /// <summary>How big the head's drawn: a short body keeps more of its head than it loses in height (a child's head is big for them); a tall or broad one keeps it as it is.</summary>
    public float HeadScale => MathF.Sqrt(MathF.Min(1f, Height));

    /// <summary>Where a socket goes on the morphed body: away from the root by the morph.</summary>
    /// <param name="socket">The socket on the base.</param>
    /// <param name="root">The base's root.</param>
    public Vector2 Move(Vector2 socket, Vector2 root) => root + (socket - root) * new Vector2(Width, Height);

    /// <summary>How a slot's part is scaled: the head's at its own size, a weapon with the height, the rest with the body.</summary>
    /// <param name="slot">The slot.</param>
    public Vector2 ScaleOf(PartSlot slot) => slot switch
    {
        PartSlot.Head or PartSlot.Face or PartSlot.Helm => new Vector2(HeadScale),
        PartSlot.Weapon or PartSlot.Offhand => new Vector2(Height),
        _ => new Vector2(Width, Height),
    };
}
