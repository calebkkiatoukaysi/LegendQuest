using System.Text;
using LegendQuest.Characters;
using LegendQuest.Rules;
using LegendQuest.Save;

namespace LegendQuest.Dialogue;

/// <summary>
/// Fills the tokens in a line: your name, the Soul-Bound's name and pronouns, your peoples and
/// classes, and whoever you're courting.
/// </summary>
/// <remarks>
/// A token with a capital first letter ({Sb_they}) comes out capitalised, for the start of a
/// sentence. An unknown token is left as it is, so a typo shows on screen instead of vanishing.
/// </remarks>
public static class TextTokens
{
    /// <summary>What's said about someone whose name isn't known yet.</summary>
    private const string Nobody = "someone";

    /// <summary>A line with every token filled in.</summary>
    /// <param name="text">The line.</param>
    /// <param name="game">The game the names come from.</param>
    public static string Fill(string text, SaveData game)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0) return text;

        var filled = new StringBuilder(text.Length + 16);
        int at = 0;
        while (at < text.Length)
        {
            int open = text.IndexOf('{', at);
            int close = open < 0 ? -1 : text.IndexOf('}', open);
            if (close < 0)
            {
                filled.Append(text, at, text.Length - at);
                break;
            }

            filled.Append(text, at, open - at);
            string token = text.Substring(open + 1, close - open - 1);
            string value = Value(token.ToLowerInvariant(), game);
            if (value is null) filled.Append(text, open, close - open + 1);
            else filled.Append(char.IsUpper(token[0]) ? Capitalise(value) : value);
            at = close + 1;
        }
        return filled.ToString();
    }

    /// <summary>What a token stands for, or null if it's not a token.</summary>
    private static string Value(string token, SaveData game)
    {
        PartyMember you = game.You;
        PartyMember soulBound = game.Member(Origins.SoulBound);
        Pronouns pronouns = soulBound?.Pronouns ?? Pronouns.They;

        return token switch
        {
            "name" => you.Name,
            "sb" => soulBound?.Name ?? Nobody,
            "sb_they" => pronouns switch { Pronouns.He => "he", Pronouns.She => "she", _ => "they" },
            "sb_them" => pronouns switch { Pronouns.He => "him", Pronouns.She => "her", _ => "them" },
            "sb_their" => pronouns switch { Pronouns.He => "his", Pronouns.She => "her", _ => "their" },
            "race" => you.Look.Ancestry.ToString(),
            "sb_race" => soulBound?.Look.Ancestry.ToString() ?? Nobody,
            "class" => ClassCatalog.Get(you.Class).Name,
            "sb_class" => soulBound is null ? Nobody : ClassCatalog.Get(soulBound.Class).Name,
            "romance" => game.Romance is null ? Nobody : Origins.NameOf(game.Romance),
            _ => null,
        };
    }

    /// <summary>A word with its first letter capitalised.</summary>
    private static string Capitalise(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];
}
