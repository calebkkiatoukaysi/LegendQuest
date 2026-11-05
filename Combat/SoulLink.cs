using System.Collections.Generic;
using LegendQuest.Characters;

namespace LegendQuest.Combat;

/// <summary>
/// The duo's meter: it charges when you and the Soul-Bound both hit the same foe between your
/// turns, and once it's full either of you can spend it on the Soul-Link Strike.
/// </summary>
/// <remarks>
/// The story's P-05. "Between your turns" is from the end of the hitter's last turn: if the other one
/// hit that foe since then, the bond charges, whichever order initiative puts the two of you in.
/// One charge a turn at most, so a strike that hits twice doesn't fill it twice. The barks are the
/// Soul-Bound's lines from P-05, which the engine hands to the screen as Bark events.
/// </remarks>
public sealed class SoulLink
{
    /// <summary>How many charges fill it. Two, so the tutorial fight shows it filling and getting spent.</summary>
    public const int Full = 2;

    /// <summary>What the Soul-Bound says the first time the two of you strike as one in a fight.</summary>
    public const string StrikeBark = "Together! Just like the old days!";

    /// <summary>What the Soul-Bound says the first time you're hurt badly in a fight.</summary>
    public const string LowHealthBark = "I've got you. I've always got you.";

    /// <summary>The share of your hit points at or under which you count as badly hurt.</summary>
    public const float LowHealth = 1f / 3f;

    /// <summary>For each of the duo, when they last hit each foe, by the engine's clock.</summary>
    private readonly Dictionary<string, Dictionary<string, int>> _hits = new()
    {
        [Origins.You] = new Dictionary<string, int>(),
        [Origins.SoulBound] = new Dictionary<string, int>(),
    };

    /// <summary>For each of the duo, when their last turn ended, by the engine's clock.</summary>
    private readonly Dictionary<string, int> _turnEnded = new() { [Origins.You] = -1, [Origins.SoulBound] = -1 };

    /// <summary>The number of the turn it last charged on.</summary>
    private int _chargedTurn = -1;

    /// <summary>Sets it up empty.</summary>
    /// <param name="active">Whether both of the duo are in the fight.</param>
    internal SoulLink(bool active) => Active = active;

    /// <summary>Whether both of you are in this fight, so it can charge at all.</summary>
    public bool Active { get; }

    /// <summary>How many charges it has, from 0 to Full.</summary>
    public int Charge { get; private set; }

    /// <summary>Whether it's full and the strike can be used.</summary>
    public bool IsFull => Charge >= Full;

    /// <summary>Whether the strike's been made this fight, so its bark only plays once.</summary>
    internal bool Struck { get; set; }

    /// <summary>Whether you've been badly hurt this fight, so its bark only plays once.</summary>
    internal bool Hurt { get; set; }

    /// <summary>Notes one of the duo hitting a foe, and charges if the other hit it since this one's last turn.</summary>
    /// <param name="hitter">You or the Soul-Bound, by origin id.</param>
    /// <param name="foe">The foe's id.</param>
    /// <param name="clock">The engine's clock now.</param>
    /// <param name="turn">The number of the turn it's happening on.</param>
    /// <returns>Whether it charged.</returns>
    internal bool NoteHit(string hitter, string foe, int clock, int turn)
    {
        if (!Active || !_hits.ContainsKey(hitter)) return false;
        _hits[hitter][foe] = clock;
        if (IsFull || _chargedTurn == turn) return false;

        string partner = hitter == Origins.You ? Origins.SoulBound : Origins.You;
        if (!_hits[partner].TryGetValue(foe, out int theirs) || theirs <= _turnEnded[hitter]) return false;

        Charge++;
        _chargedTurn = turn;
        return true;
    }

    /// <summary>Notes one of the duo's turns ending.</summary>
    /// <param name="who">Whose turn it was, by id. Anyone else's is ignored.</param>
    /// <param name="clock">The engine's clock now.</param>
    internal void TurnEnded(string who, int clock)
    {
        if (_turnEnded.ContainsKey(who)) _turnEnded[who] = clock;
    }

    /// <summary>Spends the charge on a strike: back to empty, and every hit so far forgotten.</summary>
    internal void Spend()
    {
        Charge = 0;
        foreach (Dictionary<string, int> hits in _hits.Values) hits.Clear();
    }
}
