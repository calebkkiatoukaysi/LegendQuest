namespace LegendQuest;

/// <summary>
/// What a sigil cue does to the Soul-Link's sigil, the art spec's states of it (03-art-overhaul.md
/// §4.4): sigil you on, sigil both gold, sigil soulbound flicker-violet.
/// </summary>
/// <remarks>
/// The cue writes them in kebab case; each is its name here with the hyphens taken out, so
/// flicker-gold is FlickerGold. The two flickers are a flash that goes back to whatever was showing.
/// </remarks>
public enum SigilState
{
    /// <summary>The mark there, as it is the rest of the time.</summary>
    On,

    /// <summary>Not there at all.</summary>
    Off,

    /// <summary>Gold, as when the bond's charged.</summary>
    Gold,

    /// <summary>Gold with a light round it, as when it's full.</summary>
    Glow,

    /// <summary>Violet, as when one of the two of you is down.</summary>
    Violet,

    /// <summary>Grey: broken.</summary>
    Grey,

    /// <summary>Cold: the grey of broken, gone colder.</summary>
    Cold,

    /// <summary>A flash of gold.</summary>
    FlickerGold,

    /// <summary>A flash of violet.</summary>
    FlickerViolet,
}
