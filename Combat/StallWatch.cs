using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// Watches a simulated fight's events go by and, if it runs into the simulator's round cap, says why
/// it never ended: people being got up over and over, a stand-off, one side unable to hurt the
/// other, or just a long fight.
/// </summary>
/// <remarks>
/// For --simulate, so a fight that won't finish says what's wrong with it instead of only how often.
/// It looks at the last few rounds only, since that's where a stuck fight shows: whatever happened
/// early on, by the cap nothing's changing.
/// </remarks>
internal sealed class StallWatch
{
    /// <summary>How many rounds at the end it judges a stuck fight by.</summary>
    public const int Window = 5;

    /// <summary>The party spent its turns getting people up, and none of the foes got hurt.</summary>
    public const string Treadmill = "revive treadmill: the party kept getting people up who went straight back down, and hurt nobody";

    /// <summary>Nobody hurt anybody, though somebody was moving.</summary>
    public const string StandOffMoving = "stand-off: nobody hurt anybody, though people were moving (out of reach, or walled in)";

    /// <summary>Nobody hurt anybody, and nobody moved.</summary>
    public const string StandOffStill = "stand-off: nobody hurt anybody or moved (everyone left thinks there's nothing worth doing)";

    /// <summary>The foes went unhurt while the party was hurt.</summary>
    public const string PartyHarmless = "the party couldn't hurt the foes";

    /// <summary>The party went unhurt while the foes were hurt.</summary>
    public const string FoesHarmless = "the foes couldn't hurt the party";

    /// <summary>Both sides were still being hurt: a slow fight rather than a stuck one.</summary>
    public const string Slow = "still trading blows: a slow fight, not a stuck one";

    /// <summary>The rounds anyone on the party's side was got back up in, one entry each time.</summary>
    private readonly List<int> _revives = new();

    /// <summary>The round the events are up to.</summary>
    private int _round;

    /// <summary>The last round a foe lost hit points, the last a party member did, and the last anyone took a step.</summary>
    private int _foesHurt, _partyHurt, _moved;

    /// <summary>Takes in what a command caused.</summary>
    /// <param name="engine">The fight, to tell which side an id is on.</param>
    /// <param name="events">What happened, in order.</param>
    public void See(CombatEngine engine, IReadOnlyList<CombatEvent> events)
    {
        foreach (CombatEvent happened in events)
        {
            switch (happened.Kind)
            {
                case CombatEventKind.RoundStarted:
                    _round = happened.Amount;
                    break;
                case CombatEventKind.Moved:
                    _moved = _round;
                    break;
                case CombatEventKind.Revived:
                    _revives.Add(_round);
                    break;
                case CombatEventKind.Damaged when happened.Amount > 0 && engine.Find(happened.Target) is Combatant hurt:
                    if (hurt.Side == Side.Foes) _foesHurt = _round;
                    else _partyHurt = _round;
                    break;
            }
        }
    }

    /// <summary>Why the fight never ended, judged on its last few rounds: one of this class's reasons.</summary>
    public string Verdict()
    {
        int since = _round - Window;
        int revives = _revives.FindAll(round => round > since).Count;
        bool foesHurt = _foesHurt > since, partyHurt = _partyHurt > since;

        if (!foesHurt && revives >= Window) return Treadmill;
        if (!foesHurt && !partyHurt) return _moved > since ? StandOffMoving : StandOffStill;
        if (!foesHurt) return PartyHarmless;
        return partyHurt ? Slow : FoesHarmless;
    }
}
