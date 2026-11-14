using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>
/// Everyone else in the painted art: a room's townsfolk and crowd, and the cast whose own recipe
/// isn't made yet, each put together from the shared parts (a face from the set, a library hair,
/// the everyday clothes of their trade, dyed) rather than drawn as the old pixel figures.
/// </summary>
/// <remarks>
/// A crowd look is worked out from who it's for (their id and name) and never changes, so the same
/// deckhand is the same person every time: a man or a woman (by what their name says, else by
/// chance), a face from the set of their age (a child for a brat, an elder for a grandmother), a
/// hairstyle, clothes (their trade's, if their name says it: CrowdClothes), and skin, hair, eye and
/// clothes colours. A child's or a Hearthling's is made small, a
/// Stoneborn's stout. The game registers who needs one as it loads (Crowd), and ImportedArt.Get
/// falls back to it (CrowdFigure), so the rooms, the scenes, the fights and the portraits all get it.
/// Prewarm puts them together with the recipes, so a room doesn't stall to make them.
/// </remarks>
public static partial class PartCatalog
{
    /// <summary>The words in a name that say who someone is: a woman, a man, a child, someone old.</summary>
    private static readonly string[] WomanWords = { "woman", "wife", "girl", "lady", "maid", "mother", "sister", "widow", "matron", "grandmother", "granny", "aunt", "daughter", "nun", "queen", "duchess", "fishwife", "laundress", "seamstress", "barmaid", "her " };
    private static readonly string[] ManWords = { "man", "boy", "lad", "father", "brother", "son", "grandfather", "uncle", "king", "duke", "lord", "monk", "friar", "fisherman", "his " };
    private static readonly string[] ChildWords = { "child", "brat", "urchin", "boy", "girl", "kid", "lad ", "orphan", "little" };
    private static readonly string[] OldWords = { "old", "elder", "grand", "granny", "widow", "aged", "veteran" };

    /// <summary>The faces of the set by age (Story/13 §4.0's order): child, then young, grown and old, for each base.</summary>
    private static readonly int[] ChildFaces = { 1 }, YoungFaces = { 2, 3, 4 }, GrownFaces = { 3, 4, 5, 6, 7, 8 }, OldFaces = { 9, 10 };

    /// <summary>The hairstyles a crowd wears, shorter for the men and longer for the women (both from the library, each on their own base).</summary>
    private static readonly int[] MenHairs = { 2, 3, 4, 5, 6, 7, 10, 13, 14, 17, 18, 19, 22, 24, 25, 26, 32, 42 };
    private static readonly int[] WomenHairs = { 15, 16, 18, 20, 21, 22, 23, 25, 27, 28, 29, 30, 31, 32, 33, 34, 35, 37, 38, 39, 40, 45 };

    /// <summary>
    /// The clothes colours a crowd is dyed in (Swatches.Cloth's numbers), the everyday ones of a
    /// harbour town: the main colour from the earthy and muted, the trim from those or a little
    /// brighter. The vivid swatches are left to the cast and the player.
    /// </summary>
    private static readonly int[] CrowdBases = { 4, 6, 7, 8, 11, 12, 14, 16, 17, 18, 19, 20, 22 };
    private static readonly int[] CrowdTrims = { 1, 4, 5, 6, 8, 9, 12, 16, 17, 18, 19, 20, 22 };

    /// <summary>
    /// What a crowd wears by their trade, as their name gives it (Story/13 §5's everyday armors): a
    /// deckhand a sailor's clothes, a porter a carrier's, a barmaid a tavern apron. Only what's painted
    /// on their own base is worn; with none of their trade's, they wear the everyday clothes.
    /// </summary>
    private static readonly (string[] Words, string[] Clothes)[] Trades =
    {
        (new[] { "sailor", "deckhand", "bosun", "cook", "rope", "dock", "fisher", "fishmonger", "gutter", "boatman", "lookout" }, new[] { "armor-sailor", "armor-fisher" }),
        (new[] { "porter", "carrier", "carter", "courier", "messenger", "runner" }, new[] { "armor-porter", "armor-courier", "armor-road" }),
        (new[] { "smith", "blacksmith", "apprentice", "farrier", "armourer" }, new[] { "armor-smith" }),
        (new[] { "baker", "innkeeper", "barmaid", "barkeep", "potboy", "tavern" }, new[] { "armor-baker", "armor-tavern", "armor-tavern-m" }),
        (new[] { "bard", "busker", "minstrel", "musician", "singer" }, new[] { "armor-busker" }),
        (new[] { "clerk", "customs", "harbourmaster", "scribe", "notary" }, new[] { "armor-clerk", "armor-clerk-f" }),
        (new[] { "merchant", "seller", "vendor", "trader", "stall", "pedlar", "shopkeeper", "customer" }, new[] { "armor-merchant", "armor-trader" }),
        (new[] { "maid", "servant", "laundress", "seamstress" }, new[] { "armor-maid" }),
        (new[] { "chapel", "worshipper", "priest", "priestess", "acolyte", "pilgrim" }, new[] { "armor-temple", "armor-pilgrim" }),
        (new[] { "noble", "lord", "lady", "courtier", "duke", "duchess" }, new[] { "armor-noble", "armor-noble-gown" }),
        (new[] { "watchman" }, new[] { "armor-watch" }),
        (new[] { "refugee", "beggar", "vagrant" }, new[] { "armor-refugee-man", "armor-refugee-woman", "armor-refugee-man-f" }),
        (new[] { "traveller", "wanderer" }, new[] { "armor-traveller", "armor-traveller-f", "armor-road" }),
        (new[] { "farmer", "farmhand", "woodcutter", "labourer", "worker", "gardener" }, new[] { "armor-villager-man", "armor-villager-woman", "armor-road" }),
    };

    /// <summary>A child's clothes, an old townsfolk's, and everyone else's (the town's everyday, the townsfolk's twice as likely).</summary>
    private static readonly string[] ChildClothes = { "armor-urchin", "armor-urchin-f", "armor-child-cove", "armor-child-cove-f" };
    private static readonly string[] OldClothes = { "armor-elder-coat", "armor-elder-woman" };
    private static readonly string[] EverydayClothes =
    {
        "armor-villager-man", "armor-villager-man", "armor-villager-woman", "armor-villager-woman", "armor-sailor", "armor-merchant", "armor-courier",
        "armor-road", "armor-tavern-m", "armor-clerk", "armor-clerk-f", "armor-trader", "armor-maid", "armor-refugee-woman", "armor-traveller", "armor-traveller-f",
    };

    /// <summary>The morph of a child, and of the small and stout peoples (Story/13 §3.2).</summary>
    private static readonly Morph ChildMorph = new(0.72f, 0.8f), SmallMorph = new(0.74f, 0.82f), StoutMorph = new(0.86f, 1.28f);

    /// <summary>Who has a crowd look, by the id ImportedArt.Get is asked for, and what it is.</summary>
    private static readonly Dictionary<string, Outfit> Crowds = new(StringComparer.Ordinal);

    /// <summary>
    /// Gives someone a crowd look if they need one: anyone who isn't a creature, whose own recipe
    /// isn't there to put together. Their id is the one ImportedArt.Get is asked for them by.
    /// </summary>
    /// <param name="id">Who.</param>
    /// <param name="name">What they're called, which says something about them.</param>
    /// <param name="ancestry">Their people, if they've a look that says.</param>
    public static void Crowd(string id, string name, Ancestry? ancestry = null)
    {
        if (id is null || _file is null || Crowds.ContainsKey(id) || _file.Recipes.ContainsKey(id) || PlayerFaces.Count == 0) return;
        if (CrowdOutfit(id, name ?? string.Empty, ancestry) is Outfit outfit) Crowds[id] = outfit;
    }

    /// <summary>Someone's crowd figure, or null if they've none.</summary>
    /// <param name="id">Who.</param>
    public static ImportedFigure CrowdFigure(string id) => id is not null && Crowds.TryGetValue(id, out Outfit outfit) ? Build(outfit) : null;

    /// <summary>The crowd looks, for Prewarm.</summary>
    private static IEnumerable<Outfit> CrowdOutfits => Crowds.Values;

    /// <summary>A crowd look, the same every time for the same id and name; null if there's no base to put it on.</summary>
    private static Outfit CrowdOutfit(string id, string name, Ancestry? ancestry)
    {
        var dice = new Random(StableHash(id));
        string lower = " " + name.ToLowerInvariant() + " ";
        bool woman = Has(lower, WomanWords) ? true : Has(lower, ManWords) ? false : dice.Next(2) == 0;
        bool child = Has(lower, ChildWords);
        bool old = !child && Has(lower, OldWords);

        string prefix = woman ? "face-f" : "face-m";
        int[] ages = child ? ChildFaces : old ? OldFaces : dice.Next(5) == 0 ? YoungFaces : GrownFaces;
        string face = Painted(prefix, ages, dice) ?? Painted(prefix, GrownFaces, dice);
        if (face is null || FaceBase(face) is not string onBase) return null;

        int[] hairs = woman ? WomenHairs : MenHairs;
        string hair = null;
        for (int tries = 0; tries < hairs.Length && hair is null; tries++)
        {
            string pick = "hair-" + hairs[dice.Next(hairs.Length)].ToString("00", System.Globalization.CultureInfo.InvariantCulture);
            if (Has(pick)) hair = pick;
        }
        // An old man's hair now and then goes, or thins.
        if (old && !woman && dice.Next(3) == 0) hair = Has("hair-11") ? "hair-11" : hair;

        Morph morph = child ? ChildMorph : ancestry switch
        {
            Ancestry.Hearthling => SmallMorph,
            Ancestry.Stoneborn => StoutMorph,
            _ => new Morph(0.96f + (float)dice.NextDouble() * 0.08f, 0.94f + (float)dice.NextDouble() * 0.12f),
        };
        // Grey for the old; otherwise any hair colour but the bright ones.
        int hairTint = old ? 9 + dice.Next(2) : 1 + dice.Next(7);
        var tints = new Tints(
            Swatches.Pick(Swatches.Skin, 1 + dice.Next(10)),
            Swatches.Pick(Swatches.Hair, hairTint),
            Swatches.Pick(Swatches.Eyes, 1 + dice.Next(9)),
            Swatches.Pick(Swatches.Cloth, CrowdBases[dice.Next(CrowdBases.Length)]),
            Swatches.Pick(Swatches.Cloth, CrowdTrims[dice.Next(CrowdTrims.Length)]));
        string armor = CrowdClothes(lower, onBase, child, old, dice);
        return new Outfit(id, face, armor, null, hair, null, null, morph, tints) { Base = onBase };
    }

    /// <summary>
    /// What a crowd wears: a child's clothes, else their trade's (Trades), else an old townsfolk's,
    /// else the everyday; each painted on their base, chosen by the dice. With nothing painted, the
    /// stand-in (null with none of that either).
    /// </summary>
    private static string CrowdClothes(string name, string onBase, bool child, bool old, Random dice)
    {
        var pick = new List<string>();
        void Add(string[] clothes)
        {
            foreach (string id in clothes)
                if (Part(id) is PartImage armor && armor.Base == onBase) pick.Add(id);
        }

        if (child) Add(ChildClothes);
        foreach ((string[] words, string[] clothes) in Trades)
            if (pick.Count == 0 && Has(name, words)) Add(clothes);
        if (pick.Count == 0 && old) Add(OldClothes);
        if (pick.Count == 0) Add(EverydayClothes);
        return pick.Count > 0 ? pick[dice.Next(pick.Count)] : StandIn(onBase)?.Id;
    }

    /// <summary>A face of the set with one of these numbers, painted, chosen by the dice; null if none is.</summary>
    private static string Painted(string prefix, int[] numbers, Random dice)
    {
        int start = dice.Next(numbers.Length);
        for (int k = 0; k < numbers.Length; k++)
        {
            string face = prefix + numbers[(start + k) % numbers.Length].ToString("00", System.Globalization.CultureInfo.InvariantCulture);
            if (Has(face)) return face;
        }
        return null;
    }

    private static bool Has(string text, string[] words)
    {
        foreach (string word in words)
            if (text.Contains(" " + word, StringComparison.Ordinal)) return true;
        return false;
    }

    /// <summary>A hash of a string that's the same in every run (string.GetHashCode isn't).</summary>
    private static int StableHash(string text)
    {
        unchecked
        {
            int hash = (int)2166136261;
            foreach (char c in text) hash = (hash ^ c) * 16777619;
            return hash & int.MaxValue;
        }
    }
}
