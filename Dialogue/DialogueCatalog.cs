using System;
using System.Collections.Generic;
using LegendQuest.Characters;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.Dialogue.Scripts.Generated;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Dialogue;

/// <summary>
/// Every conversation in the game, by id, and who on a map has which.
/// </summary>
/// <remarks>
/// Checked all at once when the game starts, after the maps, so a typo in any script stops the
/// game right there instead of halfway through a scene, and a script that sends you to a map or a
/// spawn that isn't there, or has a line said by nobody the game knows, is caught too. Talking to
/// someone plays their first script in this list that can start, so a later one for the same person
/// goes after the earlier ones. The list is the hand-written table, in its order, then every
/// generated script the table doesn't already hold, in the story files' order; one compiled to
/// replace a hand-written script goes in the table at that script's place, so the order holds.
/// </remarks>
public static class DialogueCatalog
{
    private static readonly DialogueScript[] Table =
    {
        StoryPrologueAct1.P_01,
        StoryPrologueAct1.P_02,
        StoryPrologueAct1.P_03,
        StoryPrologueAct1.P_04,
        StoryPrologueAct1.P_05_Wraiths_After,
        StoryPrologueAct1.A1_C1_01,
        StoryPrologueAct1.A1_C1_01_Gulls_After,
        StoryPrologueAct1.A1_C1_02,

        // The later chapters' lines for people who already had some, ahead of their old ones so
        // they win while they can start: Fennick's pay, Tamsin and Lark once you're back from the
        // Mirewood, and Brisk out at Thornbridge.
        Act1Chapter2.FennickPays,
        Act1Chapter2.FennickAfterPay,
        Act1Chapter2.TamsinBack,
        Act1Chapter2.TamsinAfterThanks,
        Act1Chapter2.LarkVerse,
        Act1Chapter2.LarkAfterVerse,
        Act1Chapter3.BriskCamp,
        Act1Chapter3.BriskAfterRaid,

        Act1Chapter1.FennickAfter,
        Scripts.Questeria.SoulBoundCity,
        Scripts.Questeria.InnkeeperWork,
        Scripts.Questeria.InnkeeperWaiting,
        Scripts.Questeria.Rosewater,
        Scripts.Questeria.InnkeeperPaid,
        Scripts.Questeria.InnkeeperAfter,
        Townspeople.Tomas,
        Townspeople.TomasAfter,
        Townspeople.Netty,
        Townspeople.NettyAfter,
        Townspeople.Widow,
        Townspeople.WidowWaiting,
        Townspeople.Cellar,
        Townspeople.CellarAfter,
        Townspeople.Ledger,
        Townspeople.LedgerAfter,
        Townspeople.Tollroad,
        Townspeople.TollroadAfter,
        Townspeople.Wren,
        Townspeople.WrenAfter,
        Townspeople.Lark,
        Townspeople.LarkMirewood,
        Townspeople.LarkAfter,
        Townspeople.Ilsa,
        Townspeople.IlsaAfter,
        Townspeople.Tamsin,
        Townspeople.TamsinAfter,
        Townspeople.Oswin,
        Townspeople.OswinAfter,
        Townspeople.Hesper,
        Townspeople.HesperAfter,
        Shops.TraderHello,
        Shops.TraderAgain,
        Shops.SmithHello,
        Shops.SmithAgain,
        Shops.ChandlerHello,
        Shops.ChandlerAgain,
        Shops.ApothecaryHello,
        Shops.ApothecaryAgain,

        StoryPrologueAct1.A1_C2_01,
        StoryPrologueAct1.A1_C2_01_Gloomfangs_After,
        Act1Chapter2.FirstRoots,
        Act1Chapter2.SecondRoots,
        StoryPrologueAct1.A1_C2_02_Letter,
        Act1Chapter2.Thornmother,
        StoryPrologueAct1.A1_C2_02_Thornmother_After,
        Act1Chapter2.Guarded,
        StoryPrologueAct1.A1_C2_03,
        StoryPrologueAct1.A1_C3_01,
        Act1Chapter3.MotherCamp,
        Act1Chapter3.MotherAfter,
        Act1Chapter3.ChildCamp,
        Act1Chapter3.ChildAfter,
        StoryPrologueAct1.A1_C3_02,
        Act1Chapter3.MiraWaiting,
        Act1Chapter3.MiraStory,
        Act1Chapter3.MiraReady,
        Act1Chapter3.SupplyOne,
        Act1Chapter3.SickOneAfter,
        Act1Chapter3.SupplyTwo,
        Act1Chapter3.SickTwoAfter,
        Act1Chapter3.SupplyThree,
        Act1Chapter3.SickThreeAfter,
        Act1Chapter3.Raid,
        StoryPrologueAct1.A1_C3_02_Raid_After,
        Act1Chapter4.Messenger,
        Act1Chapter4.MessengerWaiting,
        StoryPrologueAct1.A1_C4_01,
        StoryPrologueAct1.A1_C4_02,
        StoryPrologueAct1.A1_C4_02_Kael_After,
        StoryPrologueAct1.A1_C4_02_Sappers_After,
        StoryPrologueAct1.A1_C4_03,
        StoryPrologueAct1.A1_C4_04,
        Act1Chapter4.LeonAfter,
        Act1Chapter4.ElaraAfter,
        Act1Chapter4.CraneAfter,
        Act1Chapter4.KeepGuard,
        Act1Chapter4.ElaraGuard,
        StoryPrologueAct1.A1_C4_05,
        Act1Chapter4.Barkeep,
        Act1Chapter4.BarkeepAfter,

        Companions.SoulBoundMirewood,
        Companions.SoulBoundThornbridge,
        Companions.SoulBoundCourt,
        Companions.SeraJoined,
        Companions.SeraCrystal,
        Companions.SeraThornbridge,
        Companions.MiraJoined,
        Companions.MiraCoin,
        Companions.MiraCourt,
        Companions.KaelJoined,
        Companions.KaelKnights,
        Companions.BrannockJoined,
    };

    /// <summary>Every script, in the order talking to someone looks through them: Table, then the generated scripts it doesn't hold.</summary>
    private static readonly IReadOnlyList<DialogueScript> Registered = Register();

    private static readonly Dictionary<string, DialogueScript> ById = new();

    /// <summary>Indexes every script by id, so the maps can check the ones they name while they're read.</summary>
    /// <remarks>Throws on purpose if two have the same id, or if one's missing: a field read before the class that holds it had set it.</remarks>
    static DialogueCatalog()
    {
        foreach (DialogueScript script in Registered)
        {
            if (script is null) throw new InvalidOperationException("A script in the catalog is null: it was read before its class had made it.");
            if (!ById.TryAdd(script.Id, script)) throw new InvalidOperationException($"Two scripts are called {script.Id}.");
        }
    }

    /// <summary>Checks every script can be played as written, and that everyone who says a line is someone.</summary>
    /// <remarks>Throws on purpose. Call it after MapCatalog.LoadAll, since it checks where a Travel goes and who's on the maps.</remarks>
    public static void ValidateAll()
    {
        HashSet<string> onMaps = PeopleOnMaps();
        foreach (DialogueScript script in Registered)
        {
            script.Validate();
            CheckTravel(script);
            CheckSpeakers(script, onMaps);
        }
    }

    /// <summary>A script by id.</summary>
    /// <param name="id">Its id.</param>
    /// <returns>The script, or null if there's none called that.</returns>
    public static DialogueScript Get(string id) => ById.GetValueOrDefault(id ?? string.Empty);

    /// <summary>What someone on a map has to say with these flags set: their first script that can start, or null.</summary>
    /// <param name="npcId">Who, by their id on the map.</param>
    /// <param name="flags">The game's flags.</param>
    public static DialogueScript For(string npcId, ISet<string> flags)
    {
        foreach (DialogueScript script in Registered)
            if (script.With == npcId && script.CanStart(flags)) return script;
        return null;
    }

    /// <summary>The table, then every generated script that isn't in it already, each in its own order.</summary>
    /// <remarks>By reference, not by id: a generated script slotted into the table is the same one, and a hand-written one with a generated one's id is two scripts called the same, which the index throws on.</remarks>
    private static DialogueScript[] Register()
    {
        var registered = new List<DialogueScript>(Table);
        var slotted = new HashSet<DialogueScript>(Table, ReferenceEqualityComparer.Instance);
        foreach (DialogueScript script in GeneratedScripts.All)
            if (!slotted.Contains(script)) registered.Add(script);
        return registered.ToArray();
    }

    /// <summary>The id of everyone a map puts down, and the cast id of any of them who's one of the cast.</summary>
    private static HashSet<string> PeopleOnMaps()
    {
        var people = new HashSet<string>();
        foreach (MapDefinition map in MapCatalog.All)
        {
            foreach (MapNpc npc in map.Npcs)
            {
                people.Add(npc.Id);
                if (npc.Cast is not null) people.Add(npc.Cast);
            }
        }
        return people;
    }

    /// <summary>Throws if a line's said by nobody the game knows: not an origin, the cast, one of Speakers, someone on a map, or the voice of an origin or one of the cast.</summary>
    /// <remarks>Without it a typo in a speaker plays as a blank face with the id for a name.</remarks>
    private static void CheckSpeakers(DialogueScript script, HashSet<string> onMaps)
    {
        for (int i = 0; i < script.Nodes.Count; i++)
        {
            string speaker = script.Nodes[i].Speaker;
            bool known = Speakers.VoiceOf(speaker) is string whose
                ? Origins.Exists(whose) || Cast.Get(whose) is not null
                : Speakers.Exists(speaker) || Origins.Exists(speaker) || Cast.Get(speaker) is not null || onMaps.Contains(speaker);
            if (known) continue;

            string where = script.Nodes[i].Id ?? "line " + i;
            throw new InvalidOperationException($"Dialogue {script.Id}: {where} is said by {speaker}, who isn't an origin, one of the cast, one of Speakers, anyone on a map, or the voice of an origin or one of the cast.");
        }
    }

    /// <summary>Throws if a script travels somewhere that isn't a map, or onto a spawn the map doesn't have.</summary>
    private static void CheckTravel(DialogueScript script)
    {
        var effects = new List<DialogueEffect>();
        foreach (DialogueNode node in script.Nodes)
        {
            effects.AddRange(node.Effects);
            foreach (DialogueOption option in node.Options) effects.AddRange(option.Effects);
        }

        foreach (DialogueEffect effect in effects)
        {
            if (effect.Kind != EffectKind.Travel) continue;

            string[] where = effect.Id.Split(' ');
            if (!MapCatalog.Contains(where[0]))
                throw new InvalidOperationException($"Dialogue {script.Id}: travels to {where[0]}, which isn't a map.");
            if (!MapCatalog.Get(where[0]).Spawns.ContainsKey(where[1]))
                throw new InvalidOperationException($"Dialogue {script.Id}: travels to {where[0]}'s spawn {where[1]}, which it hasn't got.");
        }
    }
}
