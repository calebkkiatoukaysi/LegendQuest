using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Characters;

/// <summary>
/// The parts for the menus, put together off the main thread so changing a look never stalls a
/// frame: the creation screen's big figure (LargeWhenReady) and its grids' thumbnails
/// (ThumbnailWhenReady), and the stand-in clothes for someone whose own aren't painted yet.
/// </summary>
/// <remarks>
/// Putting a figure together (Composed) is pure work on the parts' pixels, so it runs on the thread
/// pool; only the textures are made on the main thread, when it's done. A screen asks every frame:
/// until the figure is ready it gets null (or the one it had) and keeps drawing what it had, so a
/// new hairstyle fades in a moment later instead of the game stopping while it's made. At most
/// MostAtOnce are put together at a time, and the thumbnails kept are bounded (KeptThumbnails),
/// each only a portrait's few pixels, not a figure's pictures.
/// </remarks>
public static partial class PartCatalog
{
    /// <summary>The clothes someone wears in the rooms till their own armor is painted, by base: the first of these painted on it.</summary>
    private static readonly string[] StandIns = { "armor-traveller", "armor-traveller-f" };

    /// <summary>
    /// Whether the menus' figures are put together at once, on the main thread, instead of off it:
    /// the proof run's, so each shot shows what it set up rather than what was ready by then.
    /// </summary>
    public static bool Synchronous { get; set; }

    /// <summary>How many figures are put together at once off the main thread.</summary>
    private const int MostAtOnce = 2;

    /// <summary>How many thumbnails are kept, the least recently drawn let go first.</summary>
    private const int KeptThumbnails = 240;

    /// <summary>The bones a portrait shows: the head and what's round it.</summary>
    private static readonly HashSet<string> PortraitBones = new(StringComparer.Ordinal) { "head", "torso", "arm_near_up", "arm_far_up" };

    /// <summary>What's being put together off the main thread, by what it's for.</summary>
    private static readonly Dictionary<string, Task<(RigComposer.Result Result, bool HasBody, float RigPx, string Base)?>> Pending = new(StringComparer.Ordinal);

    /// <summary>What was asked for and turned out to make nothing (an unpainted face), so it isn't asked again.</summary>
    private static readonly HashSet<string> MakesNothing = new(StringComparer.Ordinal);

    /// <summary>The thumbnails, by outfit and size, and the order they were last drawn in (oldest first).</summary>
    private static readonly Dictionary<string, Texture2D> Thumbnails = new(StringComparer.Ordinal);
    private static readonly LinkedList<string> ThumbnailOrder = new();

    /// <summary>The stand-in armor on a base: the traveller's clothes painted for it, or null if none is.</summary>
    private static PartImage StandIn(string onBase)
    {
        foreach (string id in StandIns)
            if (Part(id) is PartImage armor && armor.Base == onBase) return armor;
        return null;
    }

    /// <summary>Whether a part shows in a portrait: one of the head's and shoulders' pieces, or anything on the head.</summary>
    private static bool InPortrait(PartImage part) =>
        part.Bone is string bone ? PortraitBones.Contains(bone) : part.Slot is PartSlot.Head or PartSlot.Face or PartSlot.Facial or PartSlot.Helm or PartSlot.Marks;

    /// <summary>
    /// The outfit someone with a face picked from the set wears before they're dressed: their own
    /// recipe's, or the player's on that face's base, with their own piece on the head. What ForMember
    /// puts together, for a thumbnail.
    /// </summary>
    /// <param name="id">Who (a party member's id, or null).</param>
    /// <param name="face">The face.</param>
    public static Outfit OutfitForFace(string id, string face) =>
        Pick(id, face) is (string name, PartsFile.RecipeEntry recipe) ? Headed(Outfit.Of(name, recipe, face) with { Helm = OwnHead(id, recipe, face) }) : null;

    /// <summary>How many are being put together now.</summary>
    private static int Running => Pending.Values.Count(task => !task.IsCompleted);

    /// <summary>
    /// The creation screen's big figure for an outfit (Large), if it's been put together: otherwise
    /// null, having started putting it together off the main thread if it wasn't already. The caller
    /// keeps drawing what it had till this gives it the new one. Null for good if the outfit's face
    /// isn't painted (MakesNothing says which).
    /// </summary>
    /// <param name="outfit">The outfit.</param>
    /// <param name="height">How tall a standard adult is in it, in pixels at a room's scale.</param>
    public static ImportedFigure LargeWhenReady(Outfit outfit, int height)
    {
        if (_device is null || outfit is null) return null;
        string key = outfit.Key + "@" + height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        foreach ((string k, ImportedFigure kept) in Larges)
            if (k == key) return kept;
        if (MakesNothing.Contains(key) || !Finished(key, false, outfit, out var made)) return null;
        if (made is not (RigComposer.Result result, bool hasBody, float rigPx, string onBase))
        {
            MakesNothing.Add(key);
            return null;
        }
        return KeepLarge(key, outfit, height, result, hasBody, rigPx, onBase);
    }

    /// <summary>Whether an outfit that LargeWhenReady was asked for can't be made (its face isn't painted).</summary>
    /// <param name="outfit">The outfit.</param>
    /// <param name="height">The height it was asked for at.</param>
    public static bool CantMake(Outfit outfit, int height) =>
        outfit is null || MakesNothing.Contains(outfit.Key + "@" + height.ToString(System.Globalization.CultureInfo.InvariantCulture));

    /// <summary>
    /// A portrait of an outfit for a grid, `size` big, if it's been put together (just the head and
    /// shoulders); otherwise null, having started it off the main thread. Kept, the oldest let go once
    /// there are more than KeptThumbnails.
    /// </summary>
    /// <param name="outfit">The outfit.</param>
    /// <param name="size">The thumbnail's size.</param>
    public static Texture2D ThumbnailWhenReady(Outfit outfit, Point size)
    {
        if (_device is null || outfit is null) return null;
        string key = "thumb|" + outfit.Key + "@" + size.X.ToString(System.Globalization.CultureInfo.InvariantCulture) + "x" + size.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (Thumbnails.TryGetValue(key, out Texture2D thumbnail))
        {
            ThumbnailOrder.Remove(key);
            ThumbnailOrder.AddLast(key);
            return thumbnail;
        }
        if (MakesNothing.Contains(key) || !Finished(key, true, outfit, out var made)) return null;
        if (made is not (RigComposer.Result result, _, _, _))
        {
            MakesNothing.Add(key);
            return null;
        }
        thumbnail = ImportedFigure.PortraitOf(_device, result.Pixels, result.Width, result.Height, result.Head, size);
        Thumbnails[key] = thumbnail;
        ThumbnailOrder.AddLast(key);
        while (ThumbnailOrder.Count > KeptThumbnails)
        {
            string oldest = ThumbnailOrder.First.Value;
            ThumbnailOrder.RemoveFirst();
            Thumbnails[oldest].Dispose();
            Thumbnails.Remove(oldest);
        }
        return thumbnail;
    }

    /// <summary>
    /// Whether what `key` is for has been put together, giving it if so; otherwise starts it off the
    /// main thread (if fewer than MostAtOnce are running) and says no.
    /// </summary>
    private static bool Finished(string key, bool headOnly, Outfit outfit, out (RigComposer.Result Result, bool HasBody, float RigPx, string Base)? made)
    {
        made = null;
        if (Pending.TryGetValue(key, out var task))
        {
            if (!task.IsCompleted) return false;
            Pending.Remove(key);
            if (task.IsFaulted) throw task.Exception!.GetBaseException();
            made = task.Result;
            return true;
        }
        if (Synchronous)
        {
            made = Composed(outfit, headOnly);
            return true;
        }
        if (Running < MostAtOnce) Pending[key] = Task.Run(() => Composed(outfit, headOnly));
        return false;
    }

    /// <summary>
    /// The inventory's picture of an outfit (Picture), if it's been put together; otherwise null,
    /// having started it off the main thread. The caller keeps drawing the last one till then.
    /// </summary>
    /// <param name="outfit">The outfit.</param>
    /// <param name="height">How tall a standard adult is in it, in pixels.</param>
    public static Standing PictureWhenReady(Outfit outfit, int height)
    {
        if (_device is null || outfit is null) return null;
        string key = "picture|" + outfit.Key + "@" + height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string kept = outfit.Key + "@" + height.ToString(System.Globalization.CultureInfo.InvariantCulture);
        foreach ((string k, Standing picture) in Pictures)
            if (k == kept) return picture;
        if (MakesNothing.Contains(key) || !Finished(key, false, outfit, out var made)) return null;
        if (made is not (RigComposer.Result result, bool hasBody, _, _))
        {
            MakesNothing.Add(key);
            return null;
        }
        return KeepPicture(kept, height, result, hasBody);
    }

    /// <summary>A Large figure made from its parts put together, kept with the last few.</summary>
    private static ImportedFigure KeepLarge(string key, Outfit outfit, int height, RigComposer.Result result, bool hasBody, float rigPx, string onBase)
    {
        var info = new ImportedArtInfo(outfit.Name, "standard", "person", new[] { result.Width, result.Height },
            new[] { result.Root.X, result.Root.Y }, new[] { result.Head.X, result.Head.Y, result.Head.Width, result.Head.Height }, SourceCanvas);
        var figure = new ImportedFigure(_device, info, result.Pixels, _file.StandardHeight * ImportedArt.RoomHeight / height, hasBody, result.Layers, result.Hand, result.Joints, rigPx, onBase);
        Larges.Add((key, figure));
        if (Larges.Count > KeptLarges)
        {
            Larges[0].Figure.Dispose();
            Larges.RemoveAt(0);
        }
        return figure;
    }
}
