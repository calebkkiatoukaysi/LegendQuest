using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Dialogue;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The proof run through Act 1's chapters 2 to 4, a visit to a map at a time: the Mirewood and the
/// quarry, Thornbridge, the Gilded Flagon, the Old Crown Vault, Sunspire Keep and the Rusty Anvil.
/// </summary>
/// <remarks>
/// Each visit is beats (see ExploreScreen.ProofBeats.cs). The answers it gives are picked to see as
/// much as it can in one go: Sera flirted with, the crystal purified, the gate talked open on the
/// dice, Mira helped first, the crowd let loose on, the Soul-Bound backed at court, and Brannock
/// left to the Soul-Bound. Between scenes it walks the way a player would, except where that would
/// only be walking: then it uses a waypoint, or goes straight to the next door, as the first
/// maps' schedules do.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>One of the chapters' visits: its beats, by the stage it is.</summary>
    private string ChapterStep(int frame, ProofStage stage) => stage switch
    {
        ProofStage.RoadAgain => Beats(frame, RoadAgainBeats),
        ProofStage.Mirewood => Beats(frame, MirewoodBeats),
        ProofStage.Quarry => Beats(frame, QuarryBeats),
        ProofStage.MirewoodCamp => Beats(frame, CampBeats),
        ProofStage.QuesteriaPaid => Beats(frame, PaidBeats),
        ProofStage.Thornbridge => Beats(frame, ThornbridgeBeats),
        ProofStage.QuesteriaMessenger => Beats(frame, MessengerBeats),
        ProofStage.FlagonElara => Beats(frame, ElaraBeats),
        ProofStage.QuesteriaVault => Beats(frame, () => Walk("questeria-vault-stair.png", Out("vault", Direction.Up))),
        ProofStage.Vault => Beats(frame, VaultBeats),
        ProofStage.QuesteriaKeep => Beats(frame, () => Walk("questeria-keep-door.png", Out("keep", Direction.Up))),
        ProofStage.Keep => Beats(frame, KeepBeats),
        ProofStage.QuesteriaDocks => Beats(frame, () => Walk(null, Out("docks", Direction.Left))),
        ProofStage.DocksAnvil => Beats(frame, () => Walk("docks-anvil.png", Out("anvil", Direction.Up))),
        ProofStage.Anvil => Beats(frame, AnvilBeats),
        ProofStage.FlagonCamp => Beats(frame, FlagonCampBeats),
        _ => Beats(frame, () => Walk("road-again.png", Out("barrow", Direction.Up))),
    };

    /// <summary>A visit that's only on the way somewhere: a shot of where it is, if one's wanted, and out the way given.</summary>
    private static IEnumerable<ProofBeat> Walk(string shot, ProofBeat way)
    {
        if (shot is not null) yield return Shot(shot);
        yield return way;
    }

    /// <summary>The road again, off the waypoint: east past the sawn trunk, and into the Mirewood.</summary>
    private IEnumerable<ProofBeat> RoadAgainBeats()
    {
        yield return Shot("road-arrived.png");
        yield return Out("mirewood", Direction.Right);
    }

    /// <summary>The Mirewood's edge: into the clearing where the gloomfangs are, the chapter's card, Sera's arrow, a flirt, and Sera with you; then up the trail to the quarry.</summary>
    private IEnumerable<ProofBeat> MirewoodBeats()
    {
        yield return Shot("mirewood.png");
        yield return Answer(1);
        yield return Into("a1-c2-01", Direction.Right);
        yield return Read((Speakers.Chapter, "mirewood-card.png"), (Speakers.Card, "mirewood-wolves.png"), ("sera", "mirewood-sera.png"), (Asked, "mirewood-choice.png"));
        yield return Stop();
        yield return Shot("mirewood-joined.png");
        yield return Out("quarry", Direction.Up);
    }

    /// <summary>The quarry: Sera parting both walls of roots, the courier's letter, then the den, the Thornmother, and the crystal purified.</summary>
    private IEnumerable<ProofBeat> QuarryBeats()
    {
        yield return Shot("quarry.png");
        yield return Into("roots-1-sera", Direction.Up);
        yield return Read(("sera", "quarry-roots.png"));
        yield return Stop();
        yield return Shot("quarry-roots-parted.png");
        yield return Into("roots-2-sera", Direction.Up);
        yield return Read();
        yield return Into("courier", Direction.Up);
        yield return Read((Asked, "quarry-letter.png"));
        yield return Stop();
        yield return Shot("quarry-den.png");
        yield return Answer(0);
        yield return Into("den", Direction.Right);
        yield return Read((Speakers.Card, "quarry-thornmother.png"), (Asked, "quarry-choice.png"));
    }

    /// <summary>The camp that night, then a word with Sera, then home from the camp's stone.</summary>
    private IEnumerable<ProofBeat> CampBeats()
    {
        yield return Answer(1);
        yield return Read(("soulbound", "mirewood-camp.png"));
        yield return Shot("mirewood-camp-after.png");
        foreach (ProofBeat beat in TalkToFollower("sera")) yield return beat;
        yield return Read(("sera", "mirewood-sera-talk.png"));
        foreach (ProofBeat beat in TravelFrom("stone", "questeria")) yield return beat;
    }

    /// <summary>Fennick pays, and points you at the east gate; then out of it.</summary>
    private IEnumerable<ProofBeat> PaidBeats()
    {
        foreach (ProofBeat beat in TalkTo("fennick")) yield return beat;
        yield return Read(("fennick", "questeria-paid.png"));
        yield return Out("thornbridge", Direction.Right);
    }

    /// <summary>
    /// Thornbridge: the gate (talked open, on the dice), the camp, Mira and her three errands, the
    /// horns, the raid, the crowd, and Mira joining; then back in through the gate.
    /// </summary>
    private IEnumerable<ProofBeat> ThornbridgeBeats()
    {
        yield return Answer(1);
        yield return Read((Speakers.Chapter, "thornbridge-card.png"), (Asked, "thornbridge-gate.png"), ("refugee-mother", "thornbridge-mother.png"));
        yield return Shot("thornbridge.png");
        foreach (ProofBeat beat in TalkTo("mira")) yield return beat;
        yield return Answer(1);
        yield return Read(("mira", "thornbridge-mira.png"));
        foreach (string sick in new[] { "sick-1", "sick-2", "sick-3" })
        {
            foreach (ProofBeat beat in TalkTo(sick)) yield return beat;
            yield return Read();
        }
        foreach (ProofBeat beat in TalkTo("mira")) yield return beat;
        yield return Read();
        yield return Stop();
        yield return Shot("thornbridge-hollowed.png");
        yield return Answer(1, 0);
        yield return Into("raid", Direction.Right);
        yield return Read((Speakers.Card, "thornbridge-raid.png"), (Asked, "thornbridge-crowd.png"));
        yield return Stop();
        yield return Shot("thornbridge-after.png");
        yield return Out("questeria", Direction.Up);
    }

    /// <summary>The royal messenger at the Flagon's door, with Chapter 4's card, then in.</summary>
    private IEnumerable<ProofBeat> MessengerBeats()
    {
        foreach (ProofBeat beat in TalkTo("messenger")) yield return beat;
        yield return Read((Speakers.Chapter, "questeria-chapter-4.png"), ("royal-messenger", "questeria-messenger.png"));
        yield return Out("flagon", Direction.Up);
    }

    /// <summary>The hooded woman in the back booth, and what she wants; then out.</summary>
    private IEnumerable<ProofBeat> ElaraBeats()
    {
        foreach (ProofBeat beat in TalkTo("elara")) yield return beat;
        yield return Read(("elara", "flagon-elara.png"));
        yield return Out("door", Direction.Down);
    }

    /// <summary>The vault: down the stair, the plates, Kael up his shelf, the truce, the sappers, and Kael off to wait at the Flagon; then back up.</summary>
    private IEnumerable<ProofBeat> VaultBeats()
    {
        yield return Shot("vault.png");
        yield return Into("plates", Direction.Up);
        yield return Wait(40);
        yield return Stop();
        yield return Shot("vault-plates.png");
        yield return Answer(0);
        yield return Into("kael-shelf", Direction.Right);
        yield return Read(("kael", "vault-kael.png"), (Asked, "vault-truce.png"), (Speakers.Card, "vault-sappers.png"));
        yield return Stop();
        yield return Shot("vault-after.png");
        yield return Out("stair-up", Direction.Down);
    }

    /// <summary>The keep: the audience, the Soul-Bound backed, then Crane in the great hall on the way out.</summary>
    private IEnumerable<ProofBeat> KeepBeats()
    {
        yield return Answer(1);
        yield return Read(("leon", "keep-leon.png"), (Asked, "keep-choice.png"));
        yield return Shot("keep.png");
        yield return Answer(0);
        yield return Into("hall-crane", Direction.Down);
        yield return Read(("crane", "keep-crane.png"));
        yield return Stop();
        yield return Shot("keep-hall.png");
        yield return Out("door", Direction.Down);
    }

    /// <summary>The Rusty Anvil: Brannock, left to the Soul-Bound; then, with nothing between here and the Flagon but walking, straight there.</summary>
    private IEnumerable<ProofBeat> AnvilBeats()
    {
        yield return Shot("anvil.png");
        foreach (ProofBeat beat in TalkTo("brannock")) yield return beat;
        yield return Answer(2);
        yield return Read(("brannock", "anvil-brannock.png"), (Asked, "anvil-choice.png"));
        yield return Stop();
        yield return Shot("anvil-after.png");
        yield return Teleport("gilded-flagon", "start");
    }

    /// <summary>The Flagon with Kael and Brannock waiting at its tables: Kael's two conversations, since he joined too late in the chapter to have had either, then Kael taking Sera's place, a look in on the screens over the map, and off to the road.</summary>
    private IEnumerable<ProofBeat> FlagonCampBeats()
    {
        yield return Shot("flagon-waiting.png");
        foreach (ProofBeat beat in TalkTo("kael")) yield return beat;
        yield return Read(("kael", "flagon-kael.png"));
        foreach (ProofBeat beat in TalkTo("kael")) yield return beat;
        yield return Read();
        foreach (ProofBeat beat in TalkTo("kael")) yield return beat;
        yield return Answer(0);
        yield return Read((Asked, "flagon-swap.png"));
        yield return Stop();
        yield return Shot("flagon-swapped.png");
        foreach (ProofBeat beat in LookIns()) yield return beat;
        yield return Teleport("road", "start");
    }

    /// <summary>Walks up behind one of the party and talks to them: you a tile above them, facing them, then E.</summary>
    /// <param name="id">Their origin id.</param>
    private IEnumerable<ProofBeat> TalkToFollower(string id)
    {
        yield return Do(() =>
        {
            Vector2 feet = _walkers[_game.Party.FindIndex(member => member.Id == id)].Position / Tilemap.TileSize;
            ProofPlaceAlone(feet.X, feet.Y - 1f, Direction.Down);
        });
        yield return Wait(2);
        yield return Do(ProofUse);
    }

    /// <summary>Walks up to a waypoint, uses it, and fast travels from the map that opens.</summary>
    /// <param name="waypoint">The waypoint's id on this map.</param>
    /// <param name="to">The map to travel to.</param>
    private IEnumerable<ProofBeat> TravelFrom(string waypoint, string to)
    {
        yield return Do(() =>
        {
            Vector2 foot = _map.Interactables.First(thing => thing.Id == waypoint).Foot / Tilemap.TileSize;
            ProofPlace(foot.X, foot.Y + 1f, Direction.Up);
        });
        yield return Wait(2);
        yield return Do(ProofUse);
        yield return Wait(25);
        yield return Do(() => ProofMap().ProofTravel(to));
    }

    /// <summary>Goes straight to a spawn on another map, the way the first maps' schedules skip a walk that would only be walking.</summary>
    private ProofBeat Teleport(string map, string spawn) => Do(() =>
    {
        MapDefinition to = MapCatalog.Get(map);
        MapSpawn at = to.Spawns[spawn];
        Leave(to, at.Feet, at.Facing);
    });
}
