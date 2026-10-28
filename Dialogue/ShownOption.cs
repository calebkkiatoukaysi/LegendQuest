namespace LegendQuest.Dialogue;

/// <summary>
/// An option as the panel shows it: the option, whether it can be picked, and what it needs if it can't.
/// </summary>
/// <param name="Option">The option.</param>
/// <param name="Open">Whether it can be picked.</param>
/// <param name="Reason">What it needs, for a greyed one, or null.</param>
public sealed record ShownOption(DialogueOption Option, bool Open, string Reason);
