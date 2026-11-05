using System.Collections.Generic;
using LegendQuest.Dialogue;

namespace LegendQuest.Screens;

/// <summary>
/// The proof run through Act 1's chapters 2 to 4, a visit to a zone at a time: the Mirewood and the
/// quarry, Thornbridge, the Gilded Flagon, the Old Crown Vault, Sunspire Keep and the Rusty Anvil.
/// </summary>
/// <remarks>
/// Each visit is beats (see ExploreScreen.ProofBeats.cs), and each beat finds its trigger, door, way
/// out or person by id anywhere in the zone, changing room first if it has to. The answers it gives
/// are picked to see as much as it can in one go: Sera flirted with, the crystal purified, the gate
/// talked open on the dice, Mira helped first, the crowd let loose on, the Soul-Bound backed at court,
/// and Brannock left to the Soul-Bound. Between scenes it walks the way a player would, except where
/// that would only be walking: then it uses a waypoint, or goes straight to the next door.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>One of the chapters' visits: its beats, by the stage it is.</summary>
    private string ChapterStep(int frame, ProofStage stage) => stage switch
    {
        ProofStage.RoadAgain => Beats(frame, () => Seq(Shot("road-arrived.png"), Out("mirewood"))),
        ProofStage.Mirewood => Beats(frame, MirewoodBeats),
        ProofStage.Quarry => Beats(frame, QuarryBeats),
        ProofStage.MirewoodCamp => Beats(frame, CampBeats),
        ProofStage.QuesteriaPaid => Beats(frame, PaidBeats),
        ProofStage.Thornbridge => Beats(frame, ThornbridgeBeats),
        ProofStage.QuesteriaMessenger => Beats(frame, MessengerBeats),
        ProofStage.FlagonElara => Beats(frame, ElaraBeats),
        ProofStage.QuesteriaVault => Beats(frame, () => Seq(AtDoor("vault"), Wait(20), Shot("questeria-vault-stair.png"), Out("vault"))),
        ProofStage.Vault => Beats(frame, VaultBeats),
        ProofStage.QuesteriaKeep => Beats(frame, () => Seq(AtDoor("keep"), Wait(20), Shot("questeria-keep-door.png"), Out("keep"))),
        ProofStage.Keep => Beats(frame, KeepBeats),
        ProofStage.QuesteriaDocks => Beats(frame, () => Out("docks")),
        ProofStage.DocksAnvil => Beats(frame, () => Seq(AtDoor("anvil"), Wait(20), Shot("docks-anvil.png"), Out("anvil"))),
        ProofStage.Anvil => Beats(frame, AnvilBeats),
        ProofStage.FlagonCamp => Beats(frame, FlagonCampBeats),
        _ => Beats(frame, () => Seq(Shot("road-again.png"), Out("barrow"))),
    };

    /// <summary>The Mirewood's edge: into the clearing where the gloomfangs are, the chapter's card, Sera's arrow, a flirt, and Sera with you; then along to the quarry's mouth.</summary>
    private IEnumerable<ProofBeat> MirewoodBeats() => Seq(
        Shot("mirewood.png"),
        Answer(1),
        Into("a1-c2-01"),
        Read((Speakers.Chapter, "mirewood-card.png"), (Speakers.Card, "mirewood-wolves.png"), ("sera", "mirewood-sera.png"), (Asked, "mirewood-choice.png")),
        Stop(),
        Shot("mirewood-joined.png"),
        Out("quarry"));

    /// <summary>The quarry: Sera parting both walls of roots, the courier's letter, then the den, the Thornmother, and the crystal purified.</summary>
    private IEnumerable<ProofBeat> QuarryBeats() => Seq(
        Shot("quarry.png"),
        Into("roots-1-sera"),
        Read(("sera", "quarry-roots.png")),
        Stop(),
        Shot("quarry-roots-parted.png"),
        Into("roots-2-sera"),
        Read(),
        Into("courier"),
        Read(("you", "quarry-letter.png")),
        Stop(),
        Shot("quarry-den.png"),
        Answer(0),
        Into("den"),
        Read((Speakers.Card, "quarry-thornmother.png"), (Asked, "quarry-choice.png")));

    /// <summary>The camp that night, then a word with Sera, then home from the camp's stone.</summary>
    private IEnumerable<ProofBeat> CampBeats() => Seq(
        Read(("soulbound", "mirewood-camp.png")),
        Shot("mirewood-camp-after.png"),
        TalkToFollower("sera"),
        Read(("sera", "mirewood-sera-talk.png")),
        TravelFrom("stone", "questeria"));

    /// <summary>Fennick pays, and points you at the east gate; then out of it.</summary>
    private IEnumerable<ProofBeat> PaidBeats() => Seq(
        TalkTo("fennick"),
        Read(("fennick", "questeria-paid.png")),
        Out("thornbridge"));

    /// <summary>
    /// Thornbridge: the gate (talked open, on the dice), the camp, Mira and her three errands, the
    /// horns, the raid at the river, the crowd, and Mira joining; then back in through the gate.
    /// </summary>
    private IEnumerable<ProofBeat> ThornbridgeBeats() => Seq(
        Answer(1),
        Read((Speakers.Chapter, "thornbridge-card.png"), (Asked, "thornbridge-gate.png"), ("hedda", "thornbridge-mother.png")),
        Shot("thornbridge.png"),
        TalkTo("mira"),
        Answer(1),
        Read(("mira", "thornbridge-mira.png")),
        TalkTo("sick-1"),
        Read(),
        TalkTo("sick-2"),
        Read(),
        TalkTo("sick-3"),
        Read(),
        TalkTo("mira"),
        Read(),
        Stop(),
        Shot("thornbridge-hollowed.png"),
        Answer(1),
        Into("raid"),
        Read((Speakers.Card, "thornbridge-raid.png"), (Asked, "thornbridge-crowd.png")),
        Stop(),
        Shot("thornbridge-after.png"),
        Out("questeria"));

    /// <summary>The royal messenger at the Flagon's door, with Chapter 4's card, then in.</summary>
    private IEnumerable<ProofBeat> MessengerBeats() => Seq(
        TalkTo("messenger"),
        Read((Speakers.Chapter, "questeria-chapter-4.png"), ("royal-messenger", "questeria-messenger.png")),
        Out("flagon"));

    /// <summary>The hooded woman in the booth, and what she wants; then out.</summary>
    private IEnumerable<ProofBeat> ElaraBeats() => Seq(
        TalkTo("elara"),
        Read(("elara", "flagon-elara.png")),
        Out("door"));

    /// <summary>The vault: in from the stair, the plates in the halls, Kael up his shelf in the strongroom, the truce, the sappers, and Kael off to wait at the Flagon; then back up the stair.</summary>
    private IEnumerable<ProofBeat> VaultBeats() => Seq(
        Shot("vault.png"),
        Into("plates"),
        Wait(40),
        Stop(),
        Shot("vault-plates.png"),
        Answer(0),
        Into("kael-shelf"),
        Read(("kael", "vault-kael.png"), (Asked, "vault-truce.png"), (Speakers.Card, "vault-sappers.png")),
        Stop(),
        Shot("vault-after.png"),
        Out("stair-up"));

    /// <summary>The keep: the audience, the Soul-Bound backed, then Crane in the great hall on the way out.</summary>
    private IEnumerable<ProofBeat> KeepBeats() => Seq(
        Answer(1),
        Read(("leon", "keep-leon.png"), (Asked, "keep-choice.png")),
        Shot("keep.png"),
        Into("hall-crane"),
        Read(("crane", "keep-crane.png")),
        Stop(),
        Shot("keep-hall.png"),
        Out("door"));

    /// <summary>The Rusty Anvil: Brannock, left to the Soul-Bound; then, with nothing between here and the Flagon but walking, straight there.</summary>
    private IEnumerable<ProofBeat> AnvilBeats() => Seq(
        Shot("anvil.png"),
        TalkTo("brannock"),
        Answer(2),
        Read(("brannock", "anvil-brannock.png"), (Asked, "anvil-choice.png")),
        Stop(),
        Shot("anvil-after.png"),
        Teleport("gilded-flagon", "start"));

    /// <summary>The Flagon with Kael and Brannock waiting at its tables: Kael's two conversations, since he joined too late in the chapter to have had either, then Kael taking Sera's place, a look in on the screens over the room, and off to the road.</summary>
    private IEnumerable<ProofBeat> FlagonCampBeats() => Seq(
        Shot("flagon-waiting.png"),
        TalkTo("kael"),
        Read(("kael", "flagon-kael.png")),
        TalkTo("kael"),
        Read(),
        TalkTo("kael"),
        Answer(0),
        Read((Asked, "flagon-swap.png")),
        Stop(),
        Shot("flagon-swapped.png"),
        LookIns(),
        Teleport("road", "start"));
}
