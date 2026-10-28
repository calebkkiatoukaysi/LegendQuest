using System;
using System.Collections.Generic;
using LegendQuest.Story;

namespace LegendQuest.Dialogue;

/// <summary>
/// One conversation: who it's with, when it can happen, and every line of it.
/// </summary>
/// <remarks>
/// Data only, no state; DialogueRunner plays it against a game. Talking to someone on a map picks
/// their first script whose Needs are all set and whose Unless aren't. The script's scene ids are
/// the story's, lowercased (A1-C1-01 is a1-c1-01), so a line can be found from the script.
/// </remarks>
public sealed class DialogueScript
{
    /// <summary>Where a line or an option goes to end the conversation.</summary>
    public const string End = "end";

    /// <summary>The most options a question can have. The panel has room for four.</summary>
    private const int MostOptions = 4;

    /// <summary>The fewest options a question can have. One isn't a question.</summary>
    private const int FewestOptions = 2;

    /// <summary>The highest DC a check can have. Anything over 30 is impossible in 5e.</summary>
    private const int HighestDc = 30;

    /// <summary>What it's called. Triggers, scenes and other scripts start it by this.</summary>
    public required string Id { get; init; }

    /// <summary>The id of the person on a map it's with, or null if only a trigger, a scene or another script starts it.</summary>
    public string With { get; init; }

    /// <summary>Flags that have to be set for talking to them to start it.</summary>
    public IReadOnlyList<string> Needs { get; init; } = new string[0];

    /// <summary>Flags that stop talking to them starting it. A script that sets one of these only plays once.</summary>
    public IReadOnlyList<string> Unless { get; init; } = new string[0];

    /// <summary>Every line, in order. It starts on the first.</summary>
    public required IReadOnlyList<DialogueNode> Nodes { get; init; }

    /// <summary>Each node's place in Nodes, by id, built by Validate.</summary>
    private Dictionary<string, int> _index;

    /// <summary>Whether it can start with these flags set.</summary>
    /// <param name="flags">The game's flags.</param>
    public bool CanStart(ISet<string> flags)
    {
        foreach (string flag in Needs)
            if (!flags.Contains(flag)) return false;
        foreach (string flag in Unless)
            if (flags.Contains(flag)) return false;
        return true;
    }

    /// <summary>Where a node is in Nodes.</summary>
    /// <param name="id">The node's id.</param>
    /// <returns>Its place, or -1 for End.</returns>
    public int IndexOf(string id) => id == End ? -1 : _index[id];

    /// <summary>Checks the script can be played as written, and indexes it.</summary>
    /// <remarks>
    /// Throws on purpose, like the Black Box's. Every fault here is a typo that would otherwise only
    /// show up halfway through a conversation in front of a player: a Next pointing nowhere, a
    /// question with one option, a check with nowhere to go, an effect naming a meter that isn't one.
    /// </remarks>
    /// <exception cref="InvalidOperationException">If it couldn't be played as written.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id)) throw new InvalidOperationException("A script has no id.");
        if (Nodes.Count == 0) throw Fail("has no lines");

        var index = new Dictionary<string, int>();
        for (int i = 0; i < Nodes.Count; i++)
        {
            string id = Nodes[i].Id;
            if (id is null) continue;
            if (id == End) throw Fail($"line {i} is called {End}, which means the end");
            if (!index.TryAdd(id, i)) throw Fail($"has two lines called {id}");
        }
        _index = index;

        for (int i = 0; i < Nodes.Count; i++)
        {
            DialogueNode node = Nodes[i];
            string where = node.Id ?? "line " + i;
            if (string.IsNullOrWhiteSpace(node.Speaker)) throw Fail($"{where} has nobody saying it");
            if (string.IsNullOrWhiteSpace(node.Line)) throw Fail($"{where} says nothing");
            CheckTarget(where, "Next", node.Next);
            CheckAll(where, node.Conditions, node.Effects);
            if (node.Painting is not null && !Paintings.Exists(node.Painting)) throw Fail($"{where} shows a painting, {node.Painting}, that doesn't exist");

            if (node.Options.Count > 0 && (node.Options.Count < FewestOptions || node.Options.Count > MostOptions))
                throw Fail($"{where} has {node.Options.Count} options; a question has {FewestOptions} to {MostOptions}");
            if (node.Options.Count > 0 && node.Next is not null) throw Fail($"{where} has options and a Next; the options say where it goes");
            if (node.Seconds > 0f && (node.Options.Count == 0 || node.Timeout is null)) throw Fail($"{where} is on a clock but has no options or nowhere to go when it runs out");
            if (node.Seconds > 0f) CheckTarget(where, "Timeout", node.Timeout);

            for (int o = 0; o < node.Options.Count; o++)
            {
                DialogueOption option = node.Options[o];
                string what = $"{where}, option {o + 1}";
                if (string.IsNullOrWhiteSpace(option.Text)) throw Fail($"{what} says nothing");
                CheckAll(what, option.Conditions, option.Effects);
                if (option.Check is null)
                {
                    CheckTarget(what, "Next", option.Next);
                    continue;
                }

                if (option.OnSuccess is null || option.OnFailure is null) throw Fail($"{what} rolls a check without somewhere to go on a pass and a fail");
                if (option.Check.Dc < 1 || option.Check.Dc > HighestDc) throw Fail($"{what} has a DC of {option.Check.Dc}");
                if (!Enum.IsDefined(option.Check.Ability)) throw Fail($"{what} checks an ability that doesn't exist");
                CheckTarget(what, "OnSuccess", option.OnSuccess);
                CheckTarget(what, "OnFailure", option.OnFailure);
                foreach (DialogueCondition condition in option.Check.Advantage)
                    if (condition.Problem() is string problem) throw Fail($"{what}'s advantage {problem}");
            }
        }
    }

    /// <summary>Throws if a target names a line that isn't there. Null and End are fine.</summary>
    private void CheckTarget(string where, string what, string target)
    {
        if (target is not null && target != End && !_index.ContainsKey(target))
            throw Fail($"{where} has a {what} of {target}, and there's no line called that");
    }

    /// <summary>Throws if any condition or effect is badly formed.</summary>
    private void CheckAll(string where, IReadOnlyList<DialogueCondition> conditions, IReadOnlyList<DialogueEffect> effects)
    {
        foreach (DialogueCondition condition in conditions)
            if (condition.Problem() is string problem) throw Fail($"{where} has a condition that {problem}");
        foreach (DialogueEffect effect in effects)
            if (effect.Problem() is string problem) throw Fail($"{where} has an effect that {problem}");
    }

    /// <summary>The exception for a fault, with the script's id on the front.</summary>
    private InvalidOperationException Fail(string message) => new($"Dialogue {Id}: {message}.");
}
