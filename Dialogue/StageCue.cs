using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendQuest.Dialogue;

/// <summary>
/// Something that happens on the stage around a line, before it shows: someone placed or walked
/// somewhere, a clip played, a sound, the camera, a fade. A script's cue lines, one per cue.
/// </summary>
/// <remarks>
/// Data only, the way the script writes it: the verb, then the rest of its words in order, so
/// "place soulbound at bow facing right" is Place("soulbound", "at", "bow", "facing", "right") and
/// the words keep their meaning in one place, whatever plays them (build plan §5.3): StageDirection
/// reads them. CutsceneDirector plays a line's cues before the line shows, and the game checks every
/// one against the map its script happens on when it starts.
/// </remarks>
/// <param name="Verb">What it does, as the script writes it: place, walk, fade.</param>
/// <param name="Args">The words after the verb, in order. "×4" is written x4.</param>
/// <param name="Conditions">What has to hold for it to play, from an {if …} at the start of its cue line. Empty for always.</param>
public sealed record StageCue(string Verb, IReadOnlyList<string> Args, IReadOnlyList<DialogueCondition> Conditions)
{
    /// <summary>Every verb a cue can have.</summary>
    public static readonly IReadOnlySet<string> KnownVerbs = new HashSet<string>
    {
        "place", "walk", "face", "travel-actor", "anim", "hold", "drop", "light", "sigil", "show", "hide", "swap", "echo",
        "reflection", "trail", "backdrop", "tint", "camera", "fade", "shake", "slow", "sfx", "music", "wait", "board",
    };

    /// <summary>A cue of any verb, played always. The factories below are this with the verb filled in.</summary>
    /// <param name="verb">What it does.</param>
    /// <param name="args">The words after the verb, in order.</param>
    public static StageCue Of(string verb, params string[] args) => new(verb, args, Array.Empty<DialogueCondition>());

    /// <summary>Puts an actor on the stage: at a spawn (facing left or right), beside an actor, on, before or over a prop or an actor, in the crowd, or four of them (x4) round the spot.</summary>
    public static StageCue Place(params string[] args) => Of("place", args);

    /// <summary>Walks an actor, or the party, to a spawn or an actor. Their next cue waits till they get there.</summary>
    public static StageCue Walk(params string[] args) => Of("walk", args);

    /// <summary>Turns an actor to face another actor, or left or right.</summary>
    public static StageCue Face(params string[] args) => Of("face", args);

    /// <summary>Takes one actor to another map mid-scene, onto a spawn if one's given; you takes the party. Everything before it finishes first.</summary>
    public static StageCue TravelActor(params string[] args) => Of("travel-actor", args);

    /// <summary>Plays a clip on an actor, at a partner or a target if one's given. Their next cue waits for it.</summary>
    public static StageCue Anim(params string[] args) => Of("anim", args);

    /// <summary>Puts an item in one of an actor's sockets: a hand, the belt, the neck.</summary>
    public static StageCue Hold(params string[] args) => Of("hold", args);

    /// <summary>Empties one of an actor's sockets.</summary>
    public static StageCue Drop(params string[] args) => Of("drop", args);

    /// <summary>Sets how an item or an actor is lit, in one of their sockets if one's given: on, off, or a state of its own.</summary>
    public static StageCue Light(params string[] args) => Of("light", args);

    /// <summary>Sets the Soul-Link's sigil for you, the Soul-Bound or both, for one frame if it says so.</summary>
    public static StageCue Sigil(params string[] args) => Of("sigil", args);

    /// <summary>Shows a map object that's hidden till it's wanted, at a spawn if one's given.</summary>
    public static StageCue Show(params string[] args) => Of("show", args);

    /// <summary>Hides a map object.</summary>
    public static StageCue Hide(params string[] args) => Of("hide", args);

    /// <summary>Changes an actor to another of their looks.</summary>
    public static StageCue Swap(params string[] args) => Of("swap", args);

    /// <summary>Places an actor as a memory, half there, playing a clip if one's given.</summary>
    public static StageCue Echo(params string[] args) => Of("echo", args);

    /// <summary>Gives an actor a reflection that follows them a delay behind.</summary>
    public static StageCue Reflection(params string[] args) => Of("reflection", args);

    /// <summary>Leaves a trail of a colour behind an actor.</summary>
    public static StageCue Trail(params string[] args) => Of("trail", args);

    /// <summary>Puts a backdrop up behind the scene, or takes it down (off). Everything before it finishes first.</summary>
    public static StageCue Backdrop(params string[] args) => Of("backdrop", args);

    /// <summary>Tints the scene, fading over 0.6 s, or clears it (off).</summary>
    public static StageCue Tint(params string[] args) => Of("tint", args);

    /// <summary>Moves the camera to an actor or a spawn. Everything before it finishes first.</summary>
    public static StageCue Camera(params string[] args) => Of("camera", args);

    /// <summary>Fades the screen out or in over a number of seconds. Everything before it finishes first.</summary>
    public static StageCue Fade(params string[] args) => Of("fade", args);

    /// <summary>Shakes the screen, from 1 to 3.</summary>
    public static StageCue Shake(params string[] args) => Of("shake", args);

    /// <summary>Slows time by a factor; 1 puts it back. Everything before it finishes first.</summary>
    public static StageCue Slow(params string[] args) => Of("slow", args);

    /// <summary>Plays a sound.</summary>
    public static StageCue Sfx(params string[] args) => Of("sfx", args);

    /// <summary>Changes the music to a track, or stops it (none).</summary>
    public static StageCue Music(params string[] args) => Of("music", args);

    /// <summary>Waits a number of seconds. Everything before it finishes first.</summary>
    public static StageCue Wait(params string[] args) => Of("wait", args);

    /// <summary>Puts the quest board up (on) or takes it down (off).</summary>
    public static StageCue Board(params string[] args) => Of("board", args);

    /// <summary>The same cue, played only if all of these hold as well as any it had already: what an {if …} at the start of a cue line does.</summary>
    /// <param name="conditions">What has to hold.</param>
    public StageCue When(params DialogueCondition[] conditions) => this with { Conditions = Conditions.Concat(conditions).ToArray() };

    /// <summary>What's wrong with it, or null if it's well formed.</summary>
    /// <remarks>Only that it has a verb and words. StageDirection reads the words, and whether the actors, spawns and props it names are on the map is for DialogueCatalog's check and whatever plays it to say.</remarks>
    public string Problem()
    {
        if (!KnownVerbs.Contains(Verb ?? string.Empty)) return $"says {Verb}, which isn't a cue's verb";
        if (Args is null || Args.Count == 0) return $"says {Verb} with nothing after it";
        foreach (string arg in Args)
            if (string.IsNullOrWhiteSpace(arg)) return $"says {Verb} with a blank word in it";
        foreach (DialogueCondition condition in Conditions ?? Array.Empty<DialogueCondition>())
            if (condition.Problem() is string problem) return $"only plays on a condition that {problem}";
        return null;
    }
}
