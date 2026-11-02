using System;
using System.Collections.Generic;
using System.Globalization;
using LegendQuest.Characters;
using LegendQuest.Dialogue;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's camp: whoever's waiting for a place in the party, standing at the map's camp
/// places, and taking the place of someone in the party when you ask them to.
/// </summary>
/// <remarks>
/// They're people on the map like anyone else (an Npc each, dressed off their own look), so they
/// stand, look round, face you, and get a marker the same way. Talking to one plays their own
/// conversation if they've one that can start; if not, they offer to take the place of one of the
/// companions with you, or to come along if there's room. You and the Soul-Bound are never offered:
/// the story's about the two of you. The offer is a conversation built there and then, so the swap
/// goes through DialogueRunner like anything else a conversation does, and the map catches up when
/// it hears.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>What someone waiting says when there's no room for them unless someone else waits instead, and when there's room.</summary>
    private const string OfferLine = "Need me out there? Say whose place I'm taking.";
    private const string RoomLine = "There's room for one more out there. Say the word.";

    /// <summary>The options under it: to take someone's place (with their name in it), to come along, and not now.</summary>
    private const string TakePlace = "Take {0}'s place";
    private const string ComeAlong = "Come with us";
    private const string NotNow = "Not now";

    /// <summary>What they say once they're in, so the toasts saying who's in and who's waiting are up long enough to read, and the line's id.</summary>
    private const string JoinedLine = "Right behind you.";
    private const string Joined = "joined";

    /// <summary>What an offer's called: this, then who's making it. It's never in DialogueCatalog, so it can't clash with anything there.</summary>
    private const string OfferId = "camp-";

    /// <summary>Whoever's waiting at camp who has a place to stand on this map, in the camp's order.</summary>
    private readonly List<Npc> _campers = new();

    /// <summary>Stands whoever's waiting at camp at this map's camp places, the first of them at the first, as many as there are places for. Anyone already standing there is put away first.</summary>
    /// <remarks>Before the map's loaded they're dressed with everyone else in Activate; after, they're dressed here.</remarks>
    private void SeatCampers()
    {
        foreach (Npc camper in _campers) _everyNpc.Remove(camper);
        _campers.Clear();

        int seated = Math.Min(_game.Camp.Count, _map.Camp.Count);
        for (int i = 0; i < seated; i++)
        {
            PartyMember waiting = _game.Camp[i];
            MapCamp place = _map.Camp[i];
            var camper = new Npc(new MapNpc(waiting.Id, waiting.Name, place.Feet, place.Facing, Sheet: null, Cast: null,
                Look: waiting.Look, Creature: null, Say: Array.Empty<string>(), Talk: null, Wander: 0f, Presence: MapPresence.Always));

            if (_content is not null)
            {
                camper.Walker.LoadContent(_content);
                camper.Walker.Sheet = NpcSheet(camper.Definition);
            }

            _campers.Add(camper);
            _everyNpc.Add(camper);
        }
    }

    /// <summary>What someone waiting at camp says when they've no conversation of their own to have: they'll come along if there's room, take the place of any companion with you, or wait.</summary>
    /// <remarks>
    /// With the party full there are at least two companions to offer, and with room there's coming
    /// along, so it's never fewer than two options with not now; and you're never offered, so it's
    /// never more than four.
    /// </remarks>
    /// <param name="waiting">Who, by their origin id.</param>
    private DialogueScript CampOffer(string waiting)
    {
        bool room = _game.Party.Count < Origins.PartySize;
        var options = new List<DialogueOption>();

        if (room) options.Add(new DialogueOption { Text = ComeAlong, Effects = new[] { DialogueEffect.Recruit(waiting) }, Next = Joined });
        foreach (PartyMember member in _game.Party)
        {
            if (member == _game.You || member.Id == Origins.SoulBound) continue;

            options.Add(new DialogueOption
            {
                Text = string.Format(CultureInfo.InvariantCulture, TakePlace, member.Name),
                Effects = new[] { new DialogueEffect(EffectKind.Swap, waiting + " " + member.Id) },
                Next = Joined,
            });
        }
        options.Add(new DialogueOption { Text = NotNow, Next = DialogueScript.End });

        var offer = new DialogueScript
        {
            Id = OfferId + waiting,
            With = waiting,
            Nodes = new[]
            {
                new DialogueNode { Speaker = waiting, Line = room ? RoomLine : OfferLine, Options = options },
                new DialogueNode { Id = Joined, Speaker = waiting, Line = JoinedLine, Next = DialogueScript.End },
            },
        };

        // Indexes its lines, which the runner needs, and checks it's sound like every other script.
        offer.Validate();
        return offer;
    }
}
