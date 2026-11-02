using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Dialogue;
using LegendQuest.Story;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The map screen's talk that isn't a conversation: the banter a trigger starts, said a line at a
/// time over people's heads while you keep walking, the townsfolk saying things by themselves, and
/// laying out every line over anyone's head so none of them sits on another.
/// </summary>
/// <remarks>
/// One banter plays at a time. Another that starts while it's going waits for it, and if one's
/// already waiting, the new one's dropped; three at once was too much to read anyway. A line whose
/// speaker isn't here (a companion who isn't in the party, someone who's gone home) is skipped.
/// A conversation or a menu over the map holds the line that's up, and it starts again after, so
/// nothing gets said while you couldn't read it. If the conversation changed the flags so the banter
/// can't play any more (the Red Gulls beaten while the two of you were still talking about them),
/// the rest of it would be wrong, so it stops there. The party's roaming banters need no trigger:
/// once you've been walking a while with nothing said, the first one whose flags are right and whose
/// speakers are all in the party plays, once a game.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long a banter line stays up, plus a bit for every letter, how long it takes to come and go, and the pause before the next one.</summary>
    private const float BanterLineSeconds = 2.2f;
    private const float BanterSecondsPerLetter = 0.05f;
    private const float BanterFadeInSeconds = 0.15f;
    private const float BanterFadeOutSeconds = 0.4f;
    private const float BanterGapSeconds = 0.35f;

    /// <summary>How long before a trigger that goes off more than once can start its banter again, in seconds of play.</summary>
    private const float BarkCooldownSeconds = 45f;

    /// <summary>How long you have to walk with nothing said before the party strikes up a banter of its own, in seconds.</summary>
    private const float RoamQuietSeconds = 35f;

    /// <summary>What goes in front of a roaming banter's id in the save's list of things that have happened, so it plays once a game.</summary>
    private const string RoamedPrefix = "party/";

    /// <summary>How long you've been walking with nothing said, in seconds.</summary>
    private float _roamQuiet;

    /// <summary>How near you someone has to be to say something by themselves, in art pixels, and how many can be at it at once.</summary>
    private const float ChatterReach = 5f * Tilemap.TileSize;
    private const int MostChattering = 2;

    /// <summary>The widest a line over someone's head gets before it wraps, in screen pixels.</summary>
    private const float BubbleWidth = 380f;

    /// <summary>The room left between two things over people's heads when one moves up out of the other's way, in screen pixels.</summary>
    private const int BubbleGap = 2;

    /// <summary>How far the smudge behind what someone's saying reaches past it, across and up and down.</summary>
    private static readonly Vector2 LineSpread = new(14f, 10f);
    private static readonly Color LineBacking = Color.Black * 0.85f;

    /// <summary>The banter playing, which of its lines is up, how long it's been up and how long it stays, and what it says with its tokens filled.</summary>
    private Banter _banter;
    private int _banterLine;
    private float _banterClock;
    private float _banterLength;
    private string _banterText;

    /// <summary>The banter waiting for the one playing to finish, or null.</summary>
    private Banter _banterNext;

    /// <summary>When each trigger that goes off more than once last started its banter, in seconds of play, for the cooldown.</summary>
    private readonly Dictionary<string, double> _barked = new();

    /// <summary>Whether a trigger's banter can start now: its flags are right, and if the trigger goes off more than once, it's been long enough since it last started it.</summary>
    /// <param name="trigger">A trigger with bark= set.</param>
    private bool CanBark(MapTrigger trigger)
    {
        if (!Banters.Get(trigger.Bark).CanPlay(_game.Flags)) return false;
        return trigger.Once || !_barked.TryGetValue(trigger.Id, out double last) || _game.PlaytimeSeconds - last >= BarkCooldownSeconds;
    }

    /// <summary>Starts a trigger's banter, or sets it waiting if one's playing. If one's waiting already, this one's dropped.</summary>
    /// <param name="trigger">A trigger with bark= set, whose banter can play.</param>
    private void Bark(MapTrigger trigger)
    {
        if (_banter is not null && _banterNext is not null) return;

        if (!trigger.Once) _barked[trigger.Id] = _game.PlaytimeSeconds;
        Banter banter = Banters.Get(trigger.Bark);
        if (_banter is not null)
        {
            _banterNext = banter;
            return;
        }

        _banter = banter;
        _banterLine = -1;
        NextBanterLine();
    }

    /// <summary>Counts the quiet while you walk, and once it's been long enough, starts the first of the party's roaming banters that can play, hasn't, and has everyone it needs.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    private void Roam(GameTime gameTime)
    {
        if (_banter is not null || _talking)
        {
            _roamQuiet = 0f;
            return;
        }

        if (!Leader.IsWalking) return;
        _roamQuiet += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_roamQuiet < RoamQuietSeconds) return;

        _roamQuiet = 0f;
        foreach (Banter banter in Banters.Roaming)
        {
            if (_game.Triggered.Contains(RoamedPrefix + banter.Id) || !banter.CanPlay(_game.Flags) || !AllInParty(banter)) continue;

            _game.Triggered.Add(RoamedPrefix + banter.Id);
            _banter = banter;
            _banterLine = -1;
            NextBanterLine();
            return;
        }
    }

    /// <summary>Whether everyone with a line in a banter is in the party, so none of it gets skipped.</summary>
    private bool AllInParty(Banter banter)
    {
        foreach (BanterLine line in banter.Lines)
            if (!_game.InParty(line.Speaker)) return false;
        return true;
    }

    /// <summary>Runs the banter's clock, and moves it on to its next line once the one that's up has had its time and the pause after it.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    private void UpdateBanter(GameTime gameTime)
    {
        if (_banter is null) return;

        if (!_banter.CanPlay(_game.Flags))
        {
            _banterLine = _banter.Lines.Count;
            NextBanterLine();
            return;
        }

        _banterClock += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_banterClock >= _banterLength + BanterGapSeconds) NextBanterLine();
    }

    /// <summary>Puts the banter line that's up back to its start, while a conversation or a menu is over the map, so it's all there to read after.</summary>
    private void HoldBanter()
    {
        if (_banterClock < _banterLength) _banterClock = 0f;
    }

    /// <summary>Moves the banter on to its next line whose speaker's here, and on to the banter waiting once it runs out, if that one can still play.</summary>
    private void NextBanterLine()
    {
        while (_banter is not null)
        {
            while (_banter.CanPlay(_game.Flags) && ++_banterLine < _banter.Lines.Count)
            {
                BanterLine line = _banter.Lines[_banterLine];
                if (FindSpeaker(line.Speaker) is null) continue;

                _banterText = TextTokens.Fill(line.Line, _game);
                _banterLength = BanterLineSeconds + _banterText.Length * BanterSecondsPerLetter;
                _banterClock = 0f;
                return;
            }

            _banter = _banterNext;
            _banterNext = null;
            _banterLine = -1;
        }
    }

    /// <summary>Who's saying the banter's line right now, and whether they're in the party.</summary>
    /// <returns>Them, or null between lines, when there's no banter, or if they've gone since the line started.</returns>
    private (WalkerSprite Walker, bool Party)? BanterSpeaking()
    {
        if (_banter is null || _banterClock >= _banterLength) return null;
        return FindSpeaker(_banter.Lines[_banterLine].Speaker);
    }

    /// <summary>Someone a banter line can come from: one of the party by their id (you, the Soul-Bound, a companion), or someone on the map by theirs.</summary>
    /// <param name="id">The line's speaker.</param>
    /// <returns>Their figure and whether they're in the party, or null if they aren't here.</returns>
    private (WalkerSprite Walker, bool Party)? FindSpeaker(string id)
    {
        for (int i = 0; i < _game.Party.Count; i++)
            if (_game.Party[i].Id == id) return (_walkers[i], true);
        foreach (Npc npc in _npcs)
            if (npc.Definition.Id == id) return (npc.Walker, false);
        return null;
    }

    /// <summary>Whether someone's saying the banter's line right now.</summary>
    private bool SayingBanter(WalkerSprite walker) => BanterSpeaking() is { } speaking && speaking.Walker == walker;

    /// <summary>How much of the banter's line is showing, 0 to 1, so it fades in and out.</summary>
    private float BanterAlpha() =>
        MathHelper.Clamp(MathF.Min(_banterClock / BanterFadeInSeconds, (_banterLength - _banterClock) / BanterFadeOutSeconds), 0f, 1f);

    /// <summary>Lets anyone near you who's kept quiet long enough say something by themselves, so long as there aren't two at it already and nobody's in a conversation.</summary>
    /// <remarks>Not whoever you're next to, since their name and the key to talk go over their head, and not anyone who's in the middle of a banter.</remarks>
    private void Chatter()
    {
        int chattering = 0;
        foreach (Npc npc in _npcs)
            if (npc.Chattering) chattering++;

        foreach (Npc npc in _npcs)
        {
            if (!npc.ReadyToChatter) continue;

            bool near = Vector2.Distance(Leader.Position, npc.Walker.Position) <= ChatterReach;
            if (!near || _talking || chattering >= MostChattering || npc == _nearNpc || SayingBanter(npc.Walker))
            {
                npc.WaitToChatter();
                continue;
            }

            npc.Chatter();
            chattering++;
        }
    }

    /// <summary>Works out where everything over people's heads goes this frame, so none of it sits on anything else: the prompt stays where it is, the lines people are saying go over that, and the markers over them.</summary>
    /// <param name="bubbles">Filled with each line's room on screen, its rows, its colour and how much of it is showing.</param>
    /// <param name="marks">Filled with each marker on the screen: its room, which marker, and where its bob's up to.</param>
    private void LayOutOverheads(List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)> bubbles,
        List<(Rectangle Box, MarkerKind Kind, float Phase)> marks)
    {
        var taken = new List<Rectangle>();
        if (PromptBox() is Rectangle prompt) taken.Add(prompt);
        PlaceBubbles(taken, bubbles);
        PlaceMarkers(taken, marks);
    }

    /// <summary>Works out where each line over someone's head goes: what the townsfolk are saying and the banter's line, the lowest on the screen first, each moved up out of the way of anything already there.</summary>
    /// <param name="taken">What's already over people's heads, on screen. Each line's room is added to it.</param>
    /// <param name="bubbles">Filled with each line's room on screen, its rows, its colour and how much of it is showing.</param>
    private void PlaceBubbles(List<Rectangle> taken, List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)> bubbles)
    {
        var said = new List<(Vector2 Head, string Text, Color Colour, float Shown)>();
        foreach (Npc npc in _npcs)
            if (npc.Line is string line) said.Add((HeadOnScreen(npc.Walker), TextTokens.Fill(line, _game), Palette.Bone, npc.LineAlpha));
        if (BanterSpeaking() is { } speaking)
            said.Add((HeadOnScreen(speaking.Walker), _banterText, speaking.Party ? Palette.Torch : Palette.Bone, BanterAlpha()));

        // Whoever's lowest on the screen is in front, so they keep their place and the ones behind make way.
        said.Sort((a, b) => b.Head.Y.CompareTo(a.Head.Y));

        foreach ((Vector2 head, string text, Color colour, float shown) in said)
        {
            if (!OnScreen(head)) continue;

            List<string> rows = Text.Wrap(text, _detailFont, BubbleWidth);
            float width = 0f;
            foreach (string row in rows) width = MathF.Max(width, _detailFont.MeasureString(row).X);
            float height = rows.Count * _detailFont.LineSpacing;

            var box = new Rectangle((int)(head.X - width / 2f - LineSpread.X), (int)(head.Y - PromptGap - height - LineSpread.Y),
                (int)(width + LineSpread.X * 2f), (int)(height + LineSpread.Y * 2f));
            // Kept off the very edge of the screen, the same distance in as the heading.
            box.X = Math.Clamp(box.X, (int)Margin, LegendQuestGame.ScreenWidth - (int)Margin - box.Width);
            // One that's had to move up so far it'd be under the heading's band isn't said at all: it
            // couldn't be read there, and it's only ever chatter that gets pushed that far.
            Rectangle placed = MakeRoom(box, taken);
            if (placed.Top < BandSolid) continue;
            bubbles.Add((placed, rows, colour, shown));
        }
    }

    /// <summary>Moves a box up the screen past anything already there that it would sit on, then marks its room taken.</summary>
    /// <param name="box">Where it would go, in screen pixels.</param>
    /// <param name="taken">What's already there.</param>
    /// <returns>Where it goes.</returns>
    private static Rectangle MakeRoom(Rectangle box, List<Rectangle> taken)
    {
        // Every move puts it over one of the others, so it can't take more moves than there are of them.
        for (int moves = 0; moves < taken.Count; moves++)
        {
            Rectangle? under = null;
            foreach (Rectangle other in taken)
                if (other.Intersects(box)) under = other;
            if (under is not Rectangle hit) break;

            box.Y = hit.Top - BubbleGap - box.Height;
        }

        taken.Add(box);
        return box;
    }

    /// <summary>Writes each line over someone's head on its dark smudge, fading in and out. Party lines are in torchlight, the colour a speaker's name is in a conversation.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, linear-sampled, back to front.</param>
    /// <param name="bubbles">Where each goes, from LayOutOverheads.</param>
    /// <param name="alpha">How faded in the screen is.</param>
    private void DrawBubbles(SpriteBatch spriteBatch, List<(Rectangle Box, List<string> Rows, Color Colour, float Shown)> bubbles, float alpha)
    {
        foreach ((Rectangle box, List<string> rows, Color colour, float shown) in bubbles)
        {
            float fade = shown * alpha;
            var area = new Rectangle(box.X + (int)LineSpread.X, box.Y, box.Width - (int)LineSpread.X * 2, box.Height);
            Text.DrawBacking(spriteBatch, _glow, area, (int)LineSpread.X, LineBacking * fade);

            for (int i = 0; i < rows.Count; i++)
                Text.Draw(spriteBatch, _detailFont, rows[i], box.Center.X, box.Y + LineSpread.Y + i * _detailFont.LineSpacing,
                    colour * fade, Palette.Shadow * fade, centred: true);
        }
    }

    /// <summary>Where the top of someone's head is on screen.</summary>
    private Vector2 HeadOnScreen(WalkerSprite walker) => _camera.ToScreen(HeadOf(walker));

    /// <summary>Whether a point is on the screen.</summary>
    private static bool OnScreen(Vector2 point) =>
        point.X >= 0f && point.Y >= 0f && point.X < LegendQuestGame.ScreenWidth && point.Y < LegendQuestGame.ScreenHeight;
}
