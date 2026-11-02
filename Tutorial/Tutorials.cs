using System.Collections.Generic;
using LegendQuest.Save;

namespace LegendQuest.Tutorial;

/// <summary>
/// The tips for one game: which have been seen, whether they're wanted, the one that's up, and the
/// ones waiting their turn.
/// </summary>
/// <remarks>
/// Draws nothing; TutorialPanel shows whatever's Current and asks for the Next. A tip counts as seen
/// the moment it comes up, not when it's put away, so quitting with one on screen doesn't bring it
/// back. Seen is kept in the save (SaveData.Tips), so it's once a game, not once a session, and a
/// new game on another slot teaches it all again. Tips that come at the same moment queue up and
/// come one at a time; one that's done before its turn (you've already talked to someone) is
/// dropped from the queue. A screen that shows tips makes one of these with its game and the
/// settings, and a TutorialPanel to show them.
/// </remarks>
public sealed class Tutorials
{
    private readonly SaveData _game;
    private readonly Settings _settings;
    private readonly List<TutorialTip> _waiting = new();

    /// <summary>Whether the proof run has put a tip up for a shot, which comes up even though tips are quiet.</summary>
    private bool _proofing;

    /// <summary>When true no tip comes up by itself, whatever the settings say. The proof run turns this on, so its shots are the same as they were and nothing waits on a press it can't make.</summary>
    public static bool Quiet { get; set; }

    /// <summary>Gets the tips ready for a game.</summary>
    /// <param name="game">The game, whose save keeps which tips have been seen.</param>
    /// <param name="settings">The settings, for TUTORIAL TIPS on the options. Read every time, so switching it takes effect at once.</param>
    public Tutorials(SaveData game, Settings settings)
    {
        _game = game;
        _settings = settings;
    }

    /// <summary>Whether tips are wanted: TUTORIAL TIPS is on and the proof run isn't going, or the proof run has put one up.</summary>
    public bool On => _proofing || (!Quiet && _settings.TutorialTips);

    /// <summary>The tip that's up, or null.</summary>
    public TutorialTip Current { get; private set; }

    /// <summary>How many tips have been seen in this game, for the tip that says where they all are.</summary>
    public int SeenCount => _game.Tips.Count;

    /// <summary>Whether a tip has been seen in this game, or what it teaches has been done.</summary>
    /// <param name="tipId">The tip's id in TutorialCatalog.</param>
    public bool Seen(string tipId) => _game.Tips.Contains(tipId);

    /// <summary>Puts a tip in line to come up, unless tips are off, it's been seen, or it's up or waiting already. Safe to call every frame the moment holds.</summary>
    /// <param name="tipId">The tip's id in TutorialCatalog.</param>
    /// <returns>True if it was put in line.</returns>
    /// <exception cref="System.ArgumentException">There's no tip with that id, which means a typo where it's called.</exception>
    public bool Show(string tipId)
    {
        TutorialTip tip = TutorialCatalog.Get(tipId);
        if (Quiet || !On || Seen(tipId) || Current == tip || _waiting.Contains(tip)) return false;

        _waiting.Add(tip);
        return true;
    }

    /// <summary>Says you've done what a tip teaches: it counts as seen, it's put away if it's up, and it's dropped if it's waiting.</summary>
    /// <remarks>Counted even with tips off, so switching them back on doesn't teach you something you've already shown you know.</remarks>
    /// <param name="tipId">The tip's id in TutorialCatalog.</param>
    /// <exception cref="System.ArgumentException">There's no tip with that id.</exception>
    public void Complete(string tipId)
    {
        TutorialTip tip = TutorialCatalog.Get(tipId);
        _game.Tips.Add(tipId);
        _waiting.Remove(tip);
        if (Current == tip) Dismiss();
    }

    /// <summary>Brings the next tip in line up, now there's room, and counts it as seen.</summary>
    /// <returns>The tip, or null if one's up already, none is waiting, or tips are off (when the line is dropped).</returns>
    public TutorialTip Next()
    {
        if (!On)
        {
            Clear();
            return null;
        }
        if (Current is not null || _waiting.Count == 0) return null;

        Current = _waiting[0];
        _waiting.RemoveAt(0);
        _game.Tips.Add(Current.Id);
        return Current;
    }

    /// <summary>Puts away the tip that's up: GOT IT, a click, or its time running out.</summary>
    public void Dismiss()
    {
        Current = null;
        _proofing = false;
    }

    /// <summary>Puts away the tip that's up and drops the line, for when tips are switched off.</summary>
    public void Clear()
    {
        _waiting.Clear();
        Dismiss();
    }

    /// <summary>For the proof run, since tips are quiet in it: puts a tip first in line whatever Quiet and the settings say, so a shot can show its card. It goes the usual ways.</summary>
    /// <param name="tipId">The tip's id in TutorialCatalog.</param>
    internal void ProofShow(string tipId)
    {
        TutorialTip tip = TutorialCatalog.Get(tipId);
        _waiting.Remove(tip);
        _waiting.Insert(0, tip);
        _proofing = true;
    }
}
