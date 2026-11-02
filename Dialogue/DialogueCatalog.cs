using System;
using System.Collections.Generic;
using LegendQuest.Dialogue.Scripts;
using LegendQuest.World;

namespace LegendQuest.Dialogue;

/// <summary>
/// Every conversation in the game, by id, and who on a map has which.
/// </summary>
/// <remarks>
/// Checked all at once when the game starts, after the maps, so a typo in any script stops the
/// game right there instead of halfway through a scene, and a script that sends you to a map or a
/// spawn that isn't there is caught too. Talking to someone plays their first script in this list
/// that can start, so a later one for the same person goes after the earlier ones.
/// </remarks>
public static class DialogueCatalog
{
    private static readonly DialogueScript[] Table =
    {
        Prologue.TitleCard,
        Prologue.YourVoice,
        Prologue.Remembered,
        Prologue.Voyage,
        Prologue.Dawn,
        Act1Chapter1.Docks,
        Act1Chapter1.Grell,
        Act1Chapter1.LanternMarket,

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

        Act1Chapter2.Edge,
        Act1Chapter2.SeraJoins,
        Act1Chapter2.FirstRoots,
        Act1Chapter2.SecondRoots,
        Act1Chapter2.Letter,
        Act1Chapter2.Thornmother,
        Act1Chapter2.Crystal,
        Act1Chapter2.Guarded,
        Act1Chapter2.Camp,
        Act1Chapter3.Gate,
        Act1Chapter3.MotherCamp,
        Act1Chapter3.MotherAfter,
        Act1Chapter3.ChildCamp,
        Act1Chapter3.ChildAfter,
        Act1Chapter3.Mira,
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
        Act1Chapter3.Aftermath,
        Act1Chapter4.Messenger,
        Act1Chapter4.MessengerWaiting,
        Act1Chapter4.Summons,
        Act1Chapter4.Vault,
        Act1Chapter4.Truce,
        Act1Chapter4.KaelJoins,
        Act1Chapter4.Audience,
        Act1Chapter4.Crane,
        Act1Chapter4.LeonAfter,
        Act1Chapter4.ElaraAfter,
        Act1Chapter4.CraneAfter,
        Act1Chapter4.KeepGuard,
        Act1Chapter4.ElaraGuard,
        Act1Chapter4.Brannock,
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

    private static readonly Dictionary<string, DialogueScript> ById = new();

    /// <summary>Indexes every script by id, so the maps can check the ones they name while they're read.</summary>
    /// <remarks>Throws on purpose if two have the same id.</remarks>
    static DialogueCatalog()
    {
        foreach (DialogueScript script in Table)
            if (!ById.TryAdd(script.Id, script)) throw new InvalidOperationException($"Two scripts are called {script.Id}.");
    }

    /// <summary>Checks every script can be played as written.</summary>
    /// <remarks>Throws on purpose. Call it after MapCatalog.LoadAll, since it checks where a Travel goes.</remarks>
    public static void ValidateAll()
    {
        foreach (DialogueScript script in Table)
        {
            script.Validate();
            CheckTravel(script);
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
        foreach (DialogueScript script in Table)
            if (script.With == npcId && script.CanStart(flags)) return script;
        return null;
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
