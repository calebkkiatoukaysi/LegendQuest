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

    /// <summary>Checks every script can be played as written, that everyone who says a line is someone, and that every cue can play where its script happens.</summary>
    /// <remarks>
    /// Throws on purpose. Call it after ZoneCatalog.LoadAll, since it checks where a Travel goes and
    /// who's in the zones (which have every spawn and everyone their old maps had). A cue that can't
    /// play (its words wrong, a spawn its zone hasn't got, someone who can't be there) throws in a
    /// debug build, with every such cue in the message, and in a release build goes in the error log
    /// instead, and CutsceneDirector skips it when it comes.
    /// </remarks>
    public static void ValidateAll()
    {
        HashSet<string> onMaps = PeopleInZones();
        var faults = new List<string>();
        foreach (DialogueScript script in Registered)
        {
            script.Validate();
            CheckTravel(script);
            CheckSpeakers(script, onMaps);
            CheckStaging(script, faults);
        }
        Report(faults);
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

    /// <summary>The id of everyone a zone puts down, and the cast id of any of them who's one of the cast.</summary>
    private static HashSet<string> PeopleInZones()
    {
        var people = new HashSet<string>();
        foreach (ZoneDefinition zone in ZoneCatalog.All)
        {
            foreach (RoomDefinition room in zone.Rooms)
            {
                foreach (MapNpc npc in room.Npcs)
                {
                    people.Add(npc.Id);
                    if (npc.Cast is not null) people.Add(npc.Cast);
                }
            }
        }
        return people;
    }

    /// <summary>Throws if a line's said by nobody the game knows: not an origin, the cast, one of Speakers, someone in a zone, or the voice of an origin or one of the cast.</summary>
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

    /// <summary>Notes every cue in a script that can't play: its words, and if the script's in a zone, the spawns and the people they name.</summary>
    /// <remarks>A script that isn't in a zone (the creation screen's) only has its words checked. There's nobody on its stage, so what a cue does to the people on one does nothing there.</remarks>
    private static void CheckStaging(DialogueScript script, List<string> faults)
    {
        Staging stage = null;
        if (script.Map is not null)
        {
            if (!ZoneCatalog.Contains(script.Map))
            {
                faults.Add($"Dialogue {script.Id}: happens in {script.Map}, which isn't a zone.");
                return;
            }
            stage = new Staging(ZoneCatalog.Get(script.Map));
        }

        void Check(string where, IReadOnlyList<StageCue> cues)
        {
            foreach (StageCue cue in cues)
            {
                string problem = StageDirection.TryRead(cue, out StageDirection direction, out string wrong) ? stage?.Problem(direction) : wrong;
                if (problem is not null) faults.Add($"Dialogue {script.Id}: {where} has a cue, {cue.Verb} {string.Join(' ', cue.Args)}, that {problem}.");
            }
        }

        for (int i = 0; i < script.Nodes.Count; i++) Check(script.Nodes[i].Id ?? "line " + i, script.Nodes[i].Cues);
        Check("the closing", script.Closing);
    }

    /// <summary>Throws with every cue that can't play, in a debug build; writes each to the error log in a release one.</summary>
    private static void Report(List<string> faults)
    {
        if (faults.Count == 0) return;
#if DEBUG
        throw new InvalidOperationException(string.Join(Environment.NewLine, faults));
#else
        foreach (string fault in faults) Console.Error.WriteLine(fault);
#endif
    }

    /// <summary>Throws if a script travels somewhere that isn't a zone, or onto a spawn it hasn't got.</summary>
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
            if (!ZoneCatalog.Contains(where[0]))
                throw new InvalidOperationException($"Dialogue {script.Id}: travels to {where[0]}, which isn't a zone.");
            if (!ZoneCatalog.TryResolve(where[0], where[1], out _, out _))
                throw new InvalidOperationException($"Dialogue {script.Id}: travels to {where[0]}'s spawn {where[1]}, which it hasn't got.");
        }
    }

    /// <summary>
    /// Who and what can be in a script's zone, for checking its cues one after another: everyone the
    /// zone's rooms put down, by their id and their cast id, their props and the things in them, and
    /// whoever the cues so far have brought on.
    /// </summary>
    /// <remarks>
    /// Someone can be there if they're the party or both of you, one of the origins (you, the
    /// Soul-Bound, or a companion who might be in the party), anyone the zone puts down whether
    /// they're there yet or not, or someone an earlier place or echo cue brought on: one of the cast
    /// or the creatures, or their copies (place guard x4 brings on guard-1 to guard-4). A group's
    /// name is everyone with that id or cast id and its copies, and so is its plural (royal-guards is
    /// every royal-guard). It goes in the script's order, not the order it's played in, so a cue on
    /// one branch sees whoever the cues above it on any branch brought on.
    /// </remarks>
    private sealed class Staging
    {
        private readonly ZoneDefinition _zone;
        private readonly HashSet<string> _people = new();
        private readonly HashSet<string> _things = new();
        private readonly HashSet<string> _brought = new();
        private readonly HashSet<string> _groups = new();
        private readonly HashSet<string> _held = new();

        /// <summary>Starts from what the zone's rooms put down.</summary>
        public Staging(ZoneDefinition zone)
        {
            _zone = zone;
            foreach (RoomDefinition room in zone.Rooms)
            {
                foreach (MapNpc npc in room.Npcs)
                {
                    _people.Add(npc.Id);
                    if (npc.Cast is not null) _people.Add(npc.Cast);
                }
                foreach (MapProp prop in room.Props)
                {
                    _things.Add(prop.Prop.Id);
                    if (prop.Id is not null) _things.Add(prop.Id);
                }
                foreach (MapInteractable thing in room.Things) _things.Add(thing.Id);
            }
        }

        /// <summary>Why a cue can't play in the zone, or null if it can. Whoever a place or an echo brings on, and whatever a hold puts in someone's hand, are kept for the cues after it.</summary>
        public string Problem(StageDirection cue)
        {
            switch (cue.Verb)
            {
                case "place":
                    if (!Bring(cue.Actor, cue.Count)) return Nobody(cue.Actor);
                    return cue.Spot.Kind switch
                    {
                        CueSpotKind.Spawn => Spawn(_zone.Id, cue.Spot.Name),
                        CueSpotKind.Beside => Someone(cue.Spot.Name),
                        CueSpotKind.Crowd => null,
                        _ => IsActor(cue.Spot.Name) || _things.Contains(cue.Spot.Name) ? null : Nothing(cue.Spot.Name),
                    };

                case "walk":
                case "camera":
                    if (cue.Actor is not null && !IsActor(cue.Actor)) return Nobody(cue.Actor);
                    return ZoneCatalog.TryResolve(_zone.Id, cue.Target, out _, out _) || IsActor(cue.Target) ? null
                        : $"goes to {cue.Target}, which isn't a spawn in {_zone.Id} or anyone who can be there";

                case "face":
                    return Someone(cue.Actor) ?? (cue.Target is null ? null : Someone(cue.Target));

                case "travel-actor":
                    if (Someone(cue.Actor) is string missing) return missing;
                    return ZoneCatalog.Contains(cue.Map) ? Spawn(cue.Map, cue.Spawn) : $"goes to {cue.Map}, which isn't a zone";

                case "anim":
                    return Someone(cue.Actor) ?? (cue.Partner is null || IsActor(cue.Partner) || _things.Contains(cue.Partner) ? null : Nothing(cue.Partner));

                case "hold":
                    _held.Add(cue.Item);
                    return Someone(cue.Actor);

                case "light":
                    return IsActor(cue.Actor) || KeyItems.Exists(cue.Actor) || _held.Contains(cue.Actor) ? null
                        : $"lights {cue.Actor}, who isn't anyone who can be in {_zone.Id}, a key item, or anything a cue's put in someone's hand";

                case "show":
                case "hide":
                    if (!IsActor(cue.Actor) && !_things.Contains(cue.Actor)) return Nothing(cue.Actor);
                    return cue.Target is null ? null : Spawn(_zone.Id, cue.Target);

                case "swap":
                    return Swap(cue);

                case "echo":
                    return Bring(cue.Actor, 0) ? null : Nobody(cue.Actor);

                case "drop":
                case "reflection":
                case "trail":
                    return Someone(cue.Actor);

                default:
                    return null;
            }
        }

        /// <summary>Whether someone with this id can be in the zone: the party, both of you, an origin, anyone it puts down, anyone brought on, a group, or a group's plural.</summary>
        private bool IsActor(string id) =>
            id is StageDirection.Party or StageDirection.Both || Origins.Exists(id) || _people.Contains(id) || _brought.Contains(id)
            || _groups.Contains(id) || id.EndsWith('s') && (_people.Contains(id[..^1]) || _groups.Contains(id[..^1]));

        /// <summary>Makes sure someone's on: anyone who can be there already is, and one of the cast or the creatures is brought on. Copies, numbered from 1, are brought on either way.</summary>
        /// <returns>False if they can't be there and aren't someone who can be brought on.</returns>
        private bool Bring(string id, int copies)
        {
            bool drawn = Cast.Get(id) is CastMember cast && (cast.Look is not null || cast.Sheet is not null) || Creatures.Get(id) is not null;
            if (!drawn && !IsActor(id)) return false;

            if (copies == 0)
            {
                _brought.Add(id);
                return true;
            }
            _groups.Add(id);
            for (int i = 1; i <= copies; i++) _brought.Add(id + "-" + i);
            return true;
        }

        /// <summary>What's wrong with a swap: whoever it's on has to be one of the cast with that look (by their own id or the one the map gives them), or else it's an item, whose other pictures Legacy doesn't draw.</summary>
        private string Swap(StageDirection cue)
        {
            if (KeyItems.Exists(cue.Actor) || _held.Contains(cue.Actor)) return null;
            if (!IsActor(cue.Actor)) return Nobody(cue.Actor);

            string castId = Cast.Get(cue.Actor) is not null ? cue.Actor : null;
            foreach (RoomDefinition room in _zone.Rooms)
            {
                foreach (MapNpc npc in room.Npcs)
                    if (castId is null && npc.Id == cue.Actor) castId = npc.Cast;
            }
            return Cast.Get(castId)?.LookCalled(cue.Look) is not null ? null : $"changes {cue.Actor} to a look, {cue.Look}, that they haven't got in the cast";
        }

        /// <summary>Null if someone can be on the map, or why not.</summary>
        private string Someone(string id) => IsActor(id) ? null : Nobody(id);

        /// <summary>Why a cue naming someone who can't be there can't play.</summary>
        private string Nobody(string id) =>
            $"names {id}, who can't be in {_zone.Id}: not the party, an origin, anyone the zone puts down, or one of the cast or the creatures a cue's brought on";

        /// <summary>Why a cue naming something that isn't there can't play.</summary>
        private string Nothing(string id) => $"names {id}, which isn't anyone who can be in {_zone.Id} or a prop or a thing in it";

        /// <summary>Null if a zone has a spawn by that name, or why not.</summary>
        private static string Spawn(string zone, string spawn) =>
            ZoneCatalog.TryResolve(zone, spawn, out _, out _) ? null : $"names the spawn {spawn}, which {zone} hasn't got";
    }
}
