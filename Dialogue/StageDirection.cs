using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Dialogue;

/// <summary>
/// A stage cue read: what its words mean, worked out once, so whatever plays it and the start-up
/// check read them the same way.
/// </summary>
/// <remarks>
/// The forms are build plan §5.3's, as the script writes them:
/// <code>
/// place x [xN] at spawn | beside y | on y | before y | over y | in-crowd [facing left|right]
/// walk x to spawn-or-y            face x y | left | right       travel-actor x map [spawn]
/// anim x clip [partner]            hold x socket item             drop x socket
/// light x-or-item [socket-or-item] state                          sigil you|soulbound|both state [1 frame]
/// show thing [at spawn]            hide thing                     swap x look
/// echo x [clip]                    reflection x delay s           trail x colour
/// backdrop id|off                  tint id|off                    camera spawn-or-y
/// fade in|out s                    shake 0-3                      slow factor
/// sfx id                           music track|none               wait s           board on|off
/// </code>
/// A sound and a song are a member of Sfx and Track by the cue's word with its hyphens taken out
/// (wall-break is WallBreak), a tint is one of Tints, and a light's state is one of the glows
/// below. A cue only faces someone left or right. Who and where are left as words: whether they're
/// on a map is for whatever plays it, or for the start-up check that knows the script's map.
/// </remarks>
public sealed class StageDirection
{
    /// <summary>The verbs everything before has to finish for, and everything after waits for (build plan §5.2).</summary>
    public static readonly IReadOnlySet<string> Barriers = new HashSet<string> { "fade", "wait", "backdrop", "camera", "travel-actor", "slow" };

    /// <summary>The sockets something can be held in: a person's (03-art-overhaul.md §3.4) and a prop's.</summary>
    public static readonly IReadOnlySet<string> Sockets = new HashSet<string>
    {
        "hand_front", "hand_back", "arm_back", "back", "belt_front", "belt_back", "pouch", "neck", "head", "shoulder", "float",
        "top", "seat", "arm",
    };

    /// <summary>Who a sigil cue can be on: the two of you, or both.</summary>
    public static readonly IReadOnlySet<string> SigilBearers = new HashSet<string> { "you", "soulbound", Both };

    /// <summary>The id for everyone in the party.</summary>
    public const string Party = "party";

    /// <summary>The id for you and the Soul-Bound together.</summary>
    public const string Both = "both";

    /// <summary>What a cue says to take a backdrop down or put a light out.</summary>
    public const string Off = "off";

    /// <summary>What a music cue says to stop the music.</summary>
    public const string Silence = "none";

    /// <summary>The strongest shake.</summary>
    public const float MostShake = 3f;

    /// <summary>The glow each state of a light cue gives whoever it's on, or none for off. Legacy draws the glow round the person, since the item isn't drawn.</summary>
    private static readonly Dictionary<string, Color?> Glows = new()
    {
        ["on"] = new Color(255, 214, 140),
        [Off] = null,
        ["gold"] = new Color(240, 200, 104),
        ["violet"] = new Color(178, 142, 255),
        ["violet-flare"] = new Color(206, 160, 255),
        ["gold-violet"] = new Color(222, 172, 214),
        ["red-black"] = new Color(150, 36, 40),
        ["green"] = new Color(140, 220, 140),
        ["pale"] = new Color(220, 228, 255),
    };

    /// <summary>An id as a cue writes one: lower case, numbers and hyphens.</summary>
    private static readonly Regex Id = new("^[a-z0-9][a-z0-9_-]*$");

    /// <summary>How many copies a place cue makes, as it writes it: x and a number, from ×4.</summary>
    private static readonly Regex Copies = new("^x([0-9]+)$");

    private StageDirection(StageCue cue) => Cue = cue;

    /// <summary>The cue it was read from.</summary>
    public StageCue Cue { get; }

    /// <summary>What it does.</summary>
    public string Verb => Cue.Verb;

    /// <summary>Whether it's a barrier: everything before it finishes, then it, then what comes after.</summary>
    public bool IsBarrier => Barriers.Contains(Cue.Verb);

    /// <summary>Who it's on: an actor id (build plan §5.4), or for light, swap, show and hide perhaps a thing instead. Null for the verbs that aren't on anyone.</summary>
    public string Actor { get; private set; }

    /// <summary>Where a place cue puts them.</summary>
    public CueSpot Spot { get; private set; }

    /// <summary>Where a walk or a camera goes, who a face turns to, or the spawn a show puts it at, as written: a spawn's name or someone's id.</summary>
    public string Target { get; private set; }

    /// <summary>Which way a place or a face cue turns them, or null to leave it (or to face Target).</summary>
    public CueFacing? Facing { get; private set; }

    /// <summary>How many copies a place cue makes, or 0 for just the one.</summary>
    public int Count { get; private set; }

    /// <summary>The clip an anim or an echo plays, or null.</summary>
    public string Clip { get; private set; }

    /// <summary>Who or what an anim plays toward, or null.</summary>
    public string Partner { get; private set; }

    /// <summary>The socket a hold fills or a drop empties, or the socket or item a light picks out (null if it names none).</summary>
    public string Socket { get; private set; }

    /// <summary>What a hold puts in the socket.</summary>
    public string Item { get; private set; }

    /// <summary>A light's glow, or null for off.</summary>
    public Color? Glow { get; private set; }

    /// <summary>What a sigil cue does to the sigil.</summary>
    public SigilState Sigil { get; private set; }

    /// <summary>Whether a sigil cue is only a flash, a frame long.</summary>
    public bool OneFrame { get; private set; }

    /// <summary>The number in it: seconds for a fade or a wait, how hard a shake is, how slow a slow is, how far behind a reflection is.</summary>
    public float Number { get; private set; }

    /// <summary>Whether a fade is in from black, rather than out to it.</summary>
    public bool FadesIn { get; private set; }

    /// <summary>The sound an sfx cue plays.</summary>
    public Sfx Sound { get; private set; }

    /// <summary>The song a music cue plays, or null to stop the music.</summary>
    public Track? Song { get; private set; }

    /// <summary>A tint cue's colour, or null to go back to the map's own.</summary>
    public Color? Tint { get; private set; }

    /// <summary>The map a travel-actor goes to.</summary>
    public string Map { get; private set; }

    /// <summary>The spawn a travel-actor arrives at: start if it doesn't say.</summary>
    public string Spawn { get; private set; }

    /// <summary>The look a swap changes them to.</summary>
    public string Look { get; private set; }

    /// <summary>Reads a cue.</summary>
    /// <param name="cue">The cue.</param>
    /// <param name="direction">What it says, if it's well formed.</param>
    /// <param name="problem">What's wrong with its words, if they aren't.</param>
    /// <returns>Whether it's well formed.</returns>
    public static bool TryRead(StageCue cue, out StageDirection direction, out string problem)
    {
        direction = null;
        problem = cue.Problem();
        if (problem is not null) return false;

        var read = new StageDirection(cue);
        problem = read.Fill(cue.Args);
        if (problem is not null) return false;

        direction = read;
        return true;
    }

    /// <summary>Works its words out, by verb.</summary>
    /// <returns>What's wrong with them, or null.</returns>
    private string Fill(IReadOnlyList<string> args)
    {
        switch (Verb)
        {
            case "place":
                return Place(args);

            case "walk":
                if (args.Count != 3 || args[1] != "to") return "isn't walk, who, to, then a spawn or someone";
                Actor = args[0];
                Target = args[2];
                return Ids(Actor, Target);

            case "face":
                if (args.Count != 2) return "isn't face, who, then someone, left or right";
                Actor = args[0];
                if (ReadFacing(args[1]) is CueFacing way) Facing = way;
                else Target = args[1];
                return Ids(Actor, Target);

            case "travel-actor":
                if (args.Count is < 2 or > 3) return "isn't travel-actor, who, a map, and a spawn there if not its start";
                Actor = args[0];
                Map = args[1];
                Spawn = args.Count == 3 ? args[2] : MapLoader.StartSpawn;
                return Ids(Actor, Map, Spawn);

            case "anim":
                if (args.Count is < 2 or > 3) return "isn't anim, who, a clip, and who or what it's at if anyone";
                Actor = args[0];
                Clip = args[1];
                Partner = args.Count == 3 ? args[2] : null;
                return Ids(Actor, Clip, Partner);

            case "hold":
                if (args.Count != 3) return "isn't hold, who, a socket, then the item";
                Actor = args[0];
                Socket = args[1];
                Item = args[2];
                return Sockets.Contains(Socket) ? Ids(Actor, Item) : $"holds it in {Socket}, which isn't a socket";

            case "drop":
                if (args.Count != 2) return "isn't drop, who, then a socket";
                Actor = args[0];
                Socket = args[1];
                return Sockets.Contains(Socket) ? Ids(Actor) : $"empties {Socket}, which isn't a socket";

            case "light":
                if (args.Count is < 2 or > 3) return "isn't light, who or what, the socket or item if it's someone's, then a state";
                if (!Glows.TryGetValue(args[^1], out Color? glow)) return $"lights it {args[^1]}, which isn't a light's state ({string.Join(", ", Glows.Keys)})";
                Actor = args[0];
                Socket = args.Count == 3 ? args[1] : null;
                Glow = glow;
                return Ids(Actor, Socket);

            case "sigil":
                if (args.Count is not (2 or 4) || args.Count == 4 && (args[2] != "1" || args[3] != "frame"))
                    return "isn't sigil, you, soulbound or both, a state, and 1 frame if it's only a flash";
                if (!SigilBearers.Contains(args[0])) return $"is on {args[0]}, and a sigil's only on you, soulbound or both";
                if (!TryEnum(args[1], out SigilState state)) return $"sets the sigil {args[1]}, which isn't one of its states";
                Actor = args[0];
                Sigil = state;
                OneFrame = args.Count == 4;
                return null;

            case "show":
                if (args.Count != 1 && (args.Count != 3 || args[1] != "at")) return "isn't show, what, and at a spawn if it's to go somewhere";
                Actor = args[0];
                Target = args.Count == 3 ? args[2] : null;
                return Ids(Actor, Target);

            case "hide":
                if (args.Count != 1) return "isn't hide, then what";
                Actor = args[0];
                return Ids(Actor);

            case "swap":
                if (args.Count != 2) return "isn't swap, who, then the look";
                Actor = args[0];
                Look = args[1];
                return Ids(Actor, Look);

            case "echo":
                if (args.Count is < 1 or > 2) return "isn't echo, who, and a clip if they're doing something";
                Actor = args[0];
                Clip = args.Count == 2 ? args[1] : null;
                return Ids(Actor, Clip);

            case "reflection":
                if (args.Count != 3 || args[1] != "delay" || !TryNumber(args[2], out float delay))
                    return "isn't reflection, who, delay, then seconds";
                Actor = args[0];
                Number = delay;
                return Ids(Actor);

            case "trail":
                if (args.Count != 2) return "isn't trail, who, then a colour";
                Actor = args[0];
                return Ids(Actor, args[1]);

            case "backdrop":
                return args.Count == 1 ? Ids(args[0]) : "isn't backdrop, then a backdrop's id or off";

            case "tint":
                if (args.Count != 1) return "isn't tint, then a tint's id or off";
                if (args[0] == Tints.Off) return null;
                if (!Tints.Exists(args[0])) return $"tints it {args[0]}, which isn't one of Tints";
                Tint = Tints.Get(args[0]);
                return null;

            case "camera":
                if (args.Count != 1) return "isn't camera, then a spawn or someone";
                Target = args[0];
                return Ids(Target);

            case "fade":
                if (args.Count != 2 || args[0] is not ("in" or "out") || !TryNumber(args[1], out float fade))
                    return "isn't fade, in or out, then seconds";
                FadesIn = args[0] == "in";
                Number = fade;
                return null;

            case "shake":
                if (args.Count != 1 || !TryNumber(args[0], out float shake) || shake <= 0f || shake > MostShake)
                    return $"isn't shake, then how hard, more than 0 and up to {MostShake}";
                Number = shake;
                return null;

            case "slow":
                if (args.Count != 1 || !TryNumber(args[0], out float slow) || slow <= 0f)
                    return "isn't slow, then how slow, more than 0 (1 puts it back)";
                Number = slow;
                return null;

            case "sfx":
                if (args.Count != 1) return "isn't sfx, then the sound";
                if (!TryEnum(args[0], out Sfx sound)) return $"plays {args[0]}, which isn't one of Sfx";
                Sound = sound;
                return null;

            case "music":
                if (args.Count != 1) return "isn't music, then a song or none";
                if (args[0] == Silence) return null;
                if (!TryEnum(args[0], out Track song)) return $"plays {args[0]}, which isn't one of Track";
                Song = song;
                return null;

            case "wait":
                if (args.Count != 1 || !TryNumber(args[0], out float wait)) return "isn't wait, then seconds";
                Number = wait;
                return null;

            case "board":
                return args.Count == 1 && (args[0] is "on" or Off) ? null : "isn't board, then on or off";

            default:
                return $"says {Verb}, which isn't a cue's verb";
        }
    }

    /// <summary>A place cue: who, how many copies, where, and which way they face.</summary>
    /// <returns>What's wrong with it, or null.</returns>
    private string Place(IReadOnlyList<string> args)
    {
        const string Form = "isn't place, who, xN for copies, then at a spawn, beside someone, on, before or over something, or in-crowd, then facing left or right if it says";
        int at = 1;
        Actor = args[0];
        if (args.Count > at && Copies.Match(args[at]) is { Success: true } copies)
        {
            Count = int.Parse(copies.Groups[1].Value, CultureInfo.InvariantCulture);
            if (Count < 2) return "makes fewer than two copies; one is placed without an xN";
            at++;
        }

        if (args.Count > at && args[at] == "in-crowd")
        {
            Spot = CueSpot.Crowd;
            at++;
        }
        else if (args.Count > at + 1 && ReadSpot(args[at]) is CueSpotKind kind)
        {
            Spot = new CueSpot(kind, args[at + 1]);
            at += 2;
        }
        else
        {
            return Form;
        }

        if (args.Count == at) return Ids(Actor, Spot.Name);
        if (args.Count != at + 2 || args[at] != "facing" || ReadFacing(args[at + 1]) is not CueFacing way) return Form;
        Facing = way;
        return Ids(Actor, Spot.Name);
    }

    /// <summary>The kind of place the word before a place cue's spot means, or null if it isn't one.</summary>
    private static CueSpotKind? ReadSpot(string word) => word switch
    {
        "at" => CueSpotKind.Spawn,
        "beside" => CueSpotKind.Beside,
        "on" => CueSpotKind.On,
        "before" => CueSpotKind.Before,
        "over" => CueSpotKind.Over,
        _ => null,
    };

    /// <summary>Left or right, or null for any other word.</summary>
    private static CueFacing? ReadFacing(string word) => word switch
    {
        "left" => CueFacing.Left,
        "right" => CueFacing.Right,
        _ => null,
    };

    /// <summary>What's wrong with words that should be ids (nulls are skipped), or null.</summary>
    private static string Ids(params string[] words)
    {
        foreach (string word in words)
            if (word is not null && !Id.IsMatch(word)) return $"has {word} where an id goes";
        return null;
    }

    /// <summary>A number as a cue writes one, 0.6 or 2: no sign, so never less than nothing.</summary>
    private static bool TryNumber(string word, out float number) =>
        float.TryParse(word, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out number);

    /// <summary>An enum member by a cue's word for it: wall-break is WallBreak. Only a name: a number isn't one.</summary>
    private static bool TryEnum<T>(string word, out T value) where T : struct, Enum
    {
        value = default;
        if (!Id.IsMatch(word) || char.IsDigit(word[0])) return false;

        string name = string.Concat(Array.ConvertAll(word.Split('-'), part => part.Length == 0 ? part : char.ToUpperInvariant(part[0]) + part[1..]));
        return Enum.TryParse(name, out value) && Enum.IsDefined(value);
    }
}
