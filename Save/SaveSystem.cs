using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.Items;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Save;

/// <summary>
/// Reads, writes and erases the save slots: three numbered ones, the quicksave and the autosave.
/// </summary>
/// <remarks>
/// The Black Box's SaveSystem with two more slots. One slot is one JSON file in local app data,
/// written with System.Text.Json since that ships with .NET. Not next to the exe, because bin/ gets
/// wiped by a clean build. Nothing in here throws: a bad file shows up as an unreadable slot the
/// player can erase, not a crash on the title screen.
/// </remarks>
public static class SaveSystem
{
    /// <summary>How many numbered slots there are. They come first, from 0.</summary>
    public const int NumberedSlots = 3;

    /// <summary>The slot F5 writes to.</summary>
    public const int QuickSlot = NumberedSlots;

    /// <summary>The slot the game writes to on its own, between maps and before a fight.</summary>
    public const int AutoSlot = NumberedSlots + 1;

    /// <summary>Every slot, the quicksave and autosave included.</summary>
    public const int SlotCount = NumberedSlots + 2;

    /// <summary>The shape of save file this build writes.</summary>
    /// <remarks>Bump it when an existing field changes meaning or goes away, and add a step to Upgrade. A new field with a default does not need a bump.</remarks>
    public const int CurrentVersion = 6;

    /// <summary>Written to disk indented, so a save can be read and edited while the game is built.</summary>
    /// <remarks>Enums go in by name, so reordering one is safe and renaming one breaks saves. A number where a name should be counts as unreadable.</remarks>
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    /// <summary>Why the last read, write or erase failed, or null if it did not.</summary>
    public static string LastError { get; private set; }

    /// <summary>Where the slots live.</summary>
    /// <remarks>Settable so the proof run can point it at a scratch folder and never touch a real slot.</remarks>
    public static string Folder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LegendQuest", "Saves");

    /// <summary>Reads every slot, in order.</summary>
    /// <returns>One SaveSlot per slot, empty and unreadable ones included.</returns>
    public static SaveSlot[] ReadAll()
    {
        var slots = new SaveSlot[SlotCount];
        for (int i = 0; i < slots.Length; i++) slots[i] = Read(i);
        return slots;
    }

    /// <summary>The slot that was written to last, which is what CONTINUE loads.</summary>
    /// <returns>The newest readable slot, or null if no slot has a game in it.</returns>
    public static SaveSlot Latest()
    {
        SaveSlot latest = null;
        foreach (SaveSlot slot in ReadAll())
        {
            if (slot.HasGame && (latest is null || slot.Data.SavedUtc > latest.Data.SavedUtc)) latest = slot;
        }
        return latest;
    }

    /// <summary>Reads one slot.</summary>
    /// <param name="slot">Which slot, from 0 to SlotCount - 1.</param>
    /// <returns>What the slot turned out to be. Never null, never throws.</returns>
    public static SaveSlot Read(int slot)
    {
        string path = PathFor(slot);

        try
        {
            if (!File.Exists(path)) return new SaveSlot { Index = slot, State = SaveSlotState.Empty };

            SaveData data = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(path), Options);

            // A file holding just "null" is valid JSON and deserialises to null. Catch it here.
            if (data is null) return Unreadable(slot, "the file is empty");

            // A save from a newer build has fields this one would quietly drop if it wrote the slot back.
            if (data.Version > CurrentVersion) return Unreadable(slot, "it was saved by a newer version of the game");

            // Loading it would mean walking onto a map that isn't there.
            if (!MapCatalog.Contains(data.MapId)) return Unreadable(slot, "it's on a map called " + data.MapId + ", which this build doesn't have");

            data = Upgrade(data);
            Mend(data);
            return new SaveSlot { Index = slot, State = SaveSlotState.Occupied, Data = data };
        }
        catch (JsonException e)
        {
            return Unreadable(slot, "the file is not readable as a save: " + e.Message);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return Unreadable(slot, e.Message);
        }
    }

    /// <summary>Writes one slot, stamping it with the current time and version on the way out.</summary>
    /// <remarks>
    /// Written to a .tmp first and then swapped in, so if the game dies mid-write the slot still
    /// has the old save rather than half a new one. File.Replace keeps the old one as a .bak too.
    /// </remarks>
    /// <param name="slot">Which slot, from 0 to SlotCount - 1.</param>
    /// <param name="data">The game to write.</param>
    /// <returns>True if the slot now holds this game.</returns>
    public static bool Write(int slot, SaveData data)
    {
        if (data is null) return Fail("there is nothing to save");

        string path = PathFor(slot);
        string temporary = path + ".tmp";

        data.Version = CurrentVersion;
        data.SavedUtc = DateTime.UtcNow;
        if (data.CreatedUtc == default) data.CreatedUtc = data.SavedUtc;

        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(temporary, JsonSerializer.Serialize(data, Options));

            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);

            LastError = null;
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            // Don't leave a half-written temp file lying around looking like a save.
            TryDelete(temporary);
            return Fail(e.Message);
        }
    }

    /// <summary>Erases one slot, and the backup behind it.</summary>
    /// <param name="slot">Which slot, from 0 to SlotCount - 1.</param>
    /// <returns>True if the slot is now empty, which includes it having been empty already.</returns>
    public static bool Delete(int slot)
    {
        string path = PathFor(slot);

        try
        {
            TryDelete(path + ".tmp");
            TryDelete(path + ".bak");
            TryDelete(path);

            LastError = null;
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return Fail(e.Message);
        }
    }

    /// <summary>The file one slot lives in.</summary>
    /// <param name="slot">Which slot, from 0 to SlotCount - 1.</param>
    internal static string PathFor(int slot)
    {
        if (slot < 0 || slot >= SlotCount)
            throw new ArgumentOutOfRangeException(nameof(slot), slot, "There are only " + SlotCount + " slots.");

        string name = slot switch
        {
            QuickSlot => "quicksave",
            AutoSlot => "autosave",
            _ => "slot" + slot,
        };
        return Path.Combine(Folder, name + ".json");
    }

    /// <summary>Brings a save written by an older build up to CurrentVersion.</summary>
    /// <remarks>
    /// One step per version, each moving the file up one and falling through to the next, like The
    /// Black Box's. Version 1 had one X, Y and Facing for you, before there was a party; version 2
    /// gives you the starting party and puts its leader there. Version 2's party was you, Valen and
    /// the four placeholders; version 3 has the Soul-Bound where Valen was, keeping his name and
    /// he and him, and the placeholders gone, since the story recruits the companions. A version 2 game started on the
    /// harbour, so the Prologue's behind it: without that flag the ship would never come in. Version 4
    /// is when there were items: everyone in the party and at camp gets dressed in their kit (a
    /// companion's own, or their class's), and the bag gets the healing potions a new game starts
    /// with. Version 5 is when the act became a meter: a game that hasn't seen Act 1's last card is
    /// in Act 1, and one that has is in Act 2. Version 6 is when the sockets went in: the herring and
    /// Tomas's hook, if they've been given, go where the voyage and the docks put them, your neck and
    /// your belt. The old fields only survive the read because of SaveData.Extra.
    /// </remarks>
    /// <param name="data">The save as it was read off disk.</param>
    /// <returns>The same game, in the shape this build expects.</returns>
    private static SaveData Upgrade(SaveData data)
    {
        if (data.Version < 2)
        {
            data.Party = new List<PartyMember> { Origins.Create(Origins.You) };
            data.Leader = 0;
            if (data.Extra is not null)
            {
                if (data.Extra.Remove("X", out JsonElement x) && x.TryGetSingle(out float feetX)) data.LeaderMember.X = feetX;
                if (data.Extra.Remove("Y", out JsonElement y) && y.TryGetSingle(out float feetY)) data.LeaderMember.Y = feetY;
                if (data.Extra.Remove("Facing", out JsonElement facing) && facing.ValueKind == JsonValueKind.String
                    && Enum.TryParse(facing.GetString(), out Direction way))
                    data.LeaderMember.Facing = way;
            }
        }

        if (data.Version < 3)
        {
            var kept = new List<PartyMember>();
            PartyMember you = null, soulBound = null;

            // Mend hasn't been at it yet, so a party or a flag list nulled by hand is still null here.
            data.Party ??= new List<PartyMember>();
            data.Flags ??= new HashSet<string>();
            foreach (PartyMember member in data.Party)
            {
                if (member?.Id == Origins.You) you = member;
                else if (member?.Id == OldValen) soulBound = member;
            }

            you ??= data.Party.Find(member => member is not null) ?? Origins.Create(Origins.You);
            you.Id = Origins.You;
            kept.Add(you);

            PartyMember fresh = Origins.Create(Origins.SoulBound);
            fresh.Pronouns = Pronouns.He;
            if (soulBound is not null)
            {
                fresh.Name = soulBound.Name;
                fresh.Look = soulBound.Look;
                fresh.Class = soulBound.Class;
                fresh.Scores = soulBound.Scores;
                fresh.Feet = soulBound.Feet;
            }
            else
            {
                fresh.Name = UpgradedSoulBoundName;
                fresh.Feet = you.Feet;
            }
            kept.Add(fresh);

            data.Party = kept;
            data.Leader = 0;
            data.Flags.Add(Prologue.Done);
        }

        if (data.Version < 4)
        {
            // Mend hasn't been at these either, so a list nulled by hand, or someone in it, can still be null.
            data.Party ??= new List<PartyMember>();
            data.Camp ??= new List<PartyMember>();
            data.Inventory ??= new List<ItemStack>();
            foreach (PartyMember member in data.Party.Concat(data.Camp))
            {
                if (member is null) continue;
                member.Scores ??= new();
                member.Equipment ??= new Dictionary<EquipmentSlot, string>();
                member.PutOn(Origins.KitFor(member.Id, member.Class));
            }
            data.Bag.Add(ItemCatalog.HealingPotion, SaveData.StartingPotions);
        }

        if (data.Version < 5)
        {
            // Mend hasn't been at these either.
            data.Flags ??= new HashSet<string>();
            data.Meters ??= new Dictionary<string, int>();
            data.SetMeter(Meters.Act, data.Flags.Contains(Act1Done) ? 2 : 1);
        }

        if (data.Version < 6)
        {
            // Mend hasn't been at these either.
            data.Items ??= new List<string>();
            data.Sockets ??= new Dictionary<string, Dictionary<string, string>>();
            foreach ((string item, string socket) in WornFromTheStart)
            {
                if (!data.Items.Contains(item)) continue;
                if (!data.Sockets.TryGetValue(Origins.You, out Dictionary<string, string> yours))
                    data.Sockets[Origins.You] = yours = new Dictionary<string, string>();
                yours[socket] = item;
            }
        }

        data.Version = CurrentVersion;
        return data;
    }

    /// <summary>The id Valen had before the Soul-Bound took his place.</summary>
    private const string OldValen = "valen";

    /// <summary>What a version 2 save that never had Valen calls its Soul-Bound, since the Soul-Bound has no default name.</summary>
    private const string UpgradedSoulBoundName = "Valen";

    /// <summary>The flag Act 1's last card sets, which says a version 4 save is in Act 2.</summary>
    private const string Act1Done = "act1-done";

    /// <summary>The key items a scene puts on you as it hands them over, and the socket each goes in: the voyage's herring round your neck, and Tomas's hook on your belt. A version 5 save that has them gets them there.</summary>
    private static readonly (string Item, string Socket)[] WornFromTheStart = { ("herring-charm", "neck"), ("tomas-hook", "belt_front") };

    /// <summary>Puts back anything a hand-edited file left out or set to null, so nothing that reads the save has to check.</summary>
    /// <remarks>
    /// A save with nobody in the party gets the starting party, standing nowhere, which puts them on
    /// the map's start, and one with more than Origins.MostInParty loses the ones past it. Anything
    /// in the bag or worn that this build hasn't got is dropped, the bag is put back into stacks the
    /// way Inventory keeps it, and something worn in a slot it doesn't go in goes back in the bag.
    /// Hit points out of range get pulled back in, after the equipment, since what's worn can add to
    /// the most there can be. Whoever's away is mended the same as the party and the camp. A socket
    /// with nothing in it is dropped, and so is anyone left holding nothing.
    /// </remarks>
    /// <param name="data">The save, upgraded.</param>
    private static void Mend(SaveData data)
    {
        data.Flags ??= new();
        data.Tips ??= new();
        data.Opened ??= new();
        data.Triggered ??= new();
        data.Waypoints ??= new();
        data.Seen ??= new();
        data.Sockets ??= new();
        foreach (Dictionary<string, string> held in data.Sockets.Values)
        {
            if (held is null) continue;
            foreach (string socket in new List<string>(held.Keys))
                if (string.IsNullOrEmpty(held[socket])) held.Remove(socket);
        }
        foreach (string who in new List<string>(data.Sockets.Keys))
            if (data.Sockets[who] is not { Count: > 0 }) data.Sockets.Remove(who);

        data.Camp ??= new();
        data.Away ??= new();
        if (data.Fight is not null && !EncounterCatalog.Exists(data.Fight)) data.Fight = null;
        data.Items ??= new();
        data.Meters ??= new();
        data.Camp.RemoveAll(member => member is null);
        data.Away.RemoveAll(member => member is null);
        data.Items.RemoveAll(item => !KeyItems.Exists(item));
        foreach (string meter in new List<string>(data.Meters.Keys))
        {
            if (Meters.Exists(meter)) data.SetMeter(meter, data.Meters[meter]);
            else data.Meters.Remove(meter);
        }
        MendBag(data);

        data.Party ??= new();
        data.Party.RemoveAll(member => member is null);
        if (data.Party.Count > Origins.MostInParty) data.Party.RemoveRange(Origins.MostInParty, data.Party.Count - Origins.MostInParty);
        if (data.Party.Count == 0) data.Party = new List<PartyMember> { Origins.Create(Origins.You), Origins.Create(Origins.SoulBound) };
        if (data.Leader < 0 || data.Leader >= data.Party.Count) data.Leader = 0;

        foreach (PartyMember member in data.Party.Concat(data.Camp).Concat(data.Away))
        {
            member.Name ??= string.Empty;
            member.Scores ??= new();
            member.Look ??= new();
            member.Xp = Math.Max(0, member.Xp);
            MendEquipment(member, data.Bag);
            member.Spent ??= new();
            foreach (string resource in new List<string>(member.Spent.Keys))
                if (member.Spent[resource] <= 0) member.Spent.Remove(resource);
            member.Hp = Math.Clamp(member.Hp, 0, member.MaxHp);
        }
    }

    /// <summary>Drops anything in the bag that isn't an item or isn't there at all, and puts the rest back into stacks the way Inventory keeps them.</summary>
    /// <param name="data">The save.</param>
    private static void MendBag(SaveData data)
    {
        List<ItemStack> read = data.Inventory ?? new List<ItemStack>();
        data.Inventory = new List<ItemStack>();

        Inventory bag = data.Bag;
        foreach (ItemStack stack in read)
            if (stack is not null && ItemCatalog.Exists(stack.Id) && stack.Count > 0) bag.Add(stack.Id, stack.Count);
    }

    /// <summary>Drops anything worn that isn't an item, and moves anything worn where it doesn't go into the bag.</summary>
    /// <param name="member">Who.</param>
    /// <param name="bag">The party's bag.</param>
    private static void MendEquipment(PartyMember member, Inventory bag)
    {
        member.Equipment ??= new();
        foreach ((EquipmentSlot slot, string id) in new List<KeyValuePair<EquipmentSlot, string>>(member.Equipment))
        {
            if (ItemCatalog.Exists(id) && ItemCatalog.Get(id).Fits(slot)) continue;

            member.Equipment.Remove(slot);
            if (ItemCatalog.Exists(id)) bag.Add(id);
        }
    }

    /// <summary>Deletes a file if it is there, and says nothing if it is not.</summary>
    /// <param name="path">The file to remove.</param>
    private static void TryDelete(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>Records why an operation failed, and reports the failure to the caller.</summary>
    /// <param name="reason">What went wrong.</param>
    private static bool Fail(string reason)
    {
        LastError = reason;
        return false;
    }

    /// <summary>Records why a slot could not be read, and describes it as unreadable.</summary>
    /// <param name="slot">Which slot could not be read.</param>
    /// <param name="reason">What went wrong.</param>
    private static SaveSlot Unreadable(int slot, string reason)
    {
        LastError = reason;
        return new SaveSlot { Index = slot, State = SaveSlotState.Unreadable };
    }
}
