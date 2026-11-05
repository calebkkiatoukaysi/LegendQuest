using System.Collections.Generic;
using LegendQuest.Dialogue;
using LegendQuest.Save;

namespace LegendQuest.World;

/// <summary>
/// The "!" a room has for you, worked out without being in it: what floats over the badge of a door
/// into it and the chevrons of a way out to it, and what goes on its thumbnail on the zone map.
/// </summary>
/// <remarks>
/// Read off the zone file and the save, not off anyone walking about, so it can be asked of any
/// room. Red if a marked trigger there hasn't gone off or a scene there is waiting to play; gold if
/// anyone standing there has a conversation with a gold marker that would start if you talked to them
/// (the one ExploreScreen marks over their head); otherwise none. Red outranks gold, the way the story
/// outranks an errand. The save keeps a trigger or a scene that's gone off as zone/id, the same as
/// WorldState does.
/// </remarks>
public static class RoomMarkers
{
    /// <summary>The marker a room has with the save as it is.</summary>
    /// <param name="zone">The zone it's in.</param>
    /// <param name="room">The room, by its id in the zone. Both have to exist.</param>
    /// <param name="game">The save, for the flags and what's gone off.</param>
    /// <returns>Event, Quest or None.</returns>
    public static MarkerKind In(string zone, string room, SaveData game)
    {
        RoomDefinition here = ZoneCatalog.Get(zone).Room(room);
        ISet<string> flags = game.Flags;

        foreach (MapTrigger trigger in here.Triggers)
        {
            bool spent = trigger.Once && game.Triggered.Contains(zone + "/" + trigger.Id);
            if (trigger.Mark && trigger.Presence.Holds(flags) && !spent) return MarkerKind.Event;
        }
        foreach (MapScene scene in here.Scenes)
            if (scene.Presence.Holds(flags) && !game.Triggered.Contains(zone + "/" + scene.Id)) return MarkerKind.Event;

        MarkerKind marker = MarkerKind.None;
        foreach (MapNpc npc in here.Npcs)
        {
            if (!npc.Presence.Holds(flags)) continue;

            MarkerKind theirs = ScriptFor(npc, flags)?.Marker ?? MarkerKind.None;
            if (theirs == MarkerKind.Event) return MarkerKind.Event;
            if (theirs == MarkerKind.Quest) marker = MarkerKind.Quest;
        }
        return marker;
    }

    /// <summary>The conversation talking to someone would start now: the one the zone file gives them with talk=, if it can start, or else their first in DialogueCatalog that can. Null if there's neither.</summary>
    private static DialogueScript ScriptFor(MapNpc npc, ISet<string> flags)
    {
        if (DialogueCatalog.Get(npc.Talk) is DialogueScript talk && talk.CanStart(flags)) return talk;
        return DialogueCatalog.For(npc.Id, flags);
    }
}
