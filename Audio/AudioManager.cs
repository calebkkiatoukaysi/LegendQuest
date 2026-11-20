using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using LegendQuest.Save;

namespace LegendQuest.Audio;

/// <summary>
/// Every song and sound effect in the game, and the volume settings.
/// </summary>
/// <remarks>
/// Same calls as the audio tutorial (loading a Song and SoundEffects, MediaPlayer with IsRepeating,
/// SoundEffect.Play), all in one place like The Black Box's. It's a game service like the
/// achievement service from the services tutorial, so any screen can grab it. MediaPlayer can only
/// play one song at a time, so switching songs fades the old one out and the new one in.
///
/// A fight goes BeginBattle, then PlayCombat for every swing, blow and spell, then EndBattle. The
/// battle song comes in fast under the engage stinger, the map's song is remembered where it had
/// got to, and after the victory or defeat jingle (which plays once) the map's song comes back from
/// there. MediaPlayer.Play(song, position) seeks the song's stream on DesktopGL, so the songs can
/// stay Songs, streamed, rather than whole sound effects held in memory. The stinger and the two
/// jingles are short sound effects instead, so they start on the frame they're asked for and can
/// play over a song fading out.
/// </remarks>
public class AudioManager : GameComponent
{
    /// <summary>How long a song takes to fade out or in, in seconds.</summary>
    private const float FadeSeconds = 0.6f;

    /// <summary>How fast the map's song gets out of the way when a fight starts, in seconds. The battle song starts once it's gone, this far into the stinger.</summary>
    private const float BattleFadeOutSeconds = 0.4f;

    /// <summary>How fast the battle song comes in under the stinger, in seconds.</summary>
    private const float BattleFadeInSeconds = 0.3f;

    /// <summary>How fast the battle song goes when the fight ends, before the jingle, in seconds.</summary>
    private const float JingleFadeOutSeconds = 0.3f;

    /// <summary>How slowly the map's song comes back after the jingle, in seconds.</summary>
    private const float ResumeFadeInSeconds = 1.5f;

    /// <summary>A remembered place this close to the end of its song starts the song over instead, in seconds.</summary>
    private const double ResumeNearEndSeconds = 2.0;

    /// <summary>How far the battle song dips under the engage stinger, as a level (1 is no dip).</summary>
    private const float StingerDuck = 0.5f;

    /// <summary>How far the music dips under a big blow, as a level, and how long it stays down before it comes back up, in seconds.</summary>
    private const float ImpactDuck = 0.65f;
    private const float ImpactDuckHold = 0.2f;

    /// <summary>How long a duck takes to go all the way down, and to come all the way back up, in seconds.</summary>
    private const float DuckAttackSeconds = 0.04f;
    private const float DuckReleaseSeconds = 0.5f;

    /// <summary>The voice limit: at most VoicesPerKey of one combat sound, and VoicesInAll of all of them, in any VoiceWindow seconds.</summary>
    private const int VoicesPerKey = 2;
    private const int VoicesInAll = 10;
    private const float VoiceWindow = 0.04f;

    /// <summary>The folder under Content where the sound effects are.</summary>
    private const string SfxFolder = "Sfx/";

    /// <summary>The engage stinger under Content. It's a sound effect so it can play over the battle song.</summary>
    private const string EngageName = "Music/engage";

    /// <summary>The jingles that end a fight, by track, and the one that's playing.</summary>
    private readonly Dictionary<Track, SoundEffect> _jingles = new();
    private SoundEffectInstance _jingle;

    private readonly Dictionary<Track, Song> _songs = new();
    private readonly Dictionary<Sfx, SoundEffect> _effects = new();

    /// <summary>The takes of every combat sound that loaded, by key.</summary>
    private readonly Dictionary<string, SoundEffect[]> _combat = new();

    /// <summary>Which take each combat key plays next.</summary>
    private readonly Dictionary<string, int> _nextTake = new();

    /// <summary>When each combat key last played, oldest first, for the voice limit (seconds on _clock).</summary>
    private readonly Dictionary<string, Queue<float>> _recent = new();
    private readonly Queue<float> _recentAll = new();

    /// <summary>For the pitch nudges and the first take of each key. Seeded, so a proof run's log comes out the same every time.</summary>
    private readonly Random _random = new(1717);

    /// <summary>The engage stinger, or null if its file isn't there.</summary>
    private SoundEffect _engage;

    /// <summary>The song that's playing, and the one waiting for it to finish fading out.</summary>
    private Track? _current;
    private Track? _next;

    /// <summary>Where in its song _next starts, if not the top.</summary>
    private TimeSpan? _nextAt;

    /// <summary>How faded in the current song is, 0 is silent and 1 is full.</summary>
    private float _fade;

    /// <summary>Whether the current song is fading out.</summary>
    private bool _fadingOut;

    /// <summary>How long the fade out and the next fade in take, in seconds. A fight's switches are quicker than a map's.</summary>
    private float _fadeOutSeconds = FadeSeconds;
    private float _fadeInSeconds = FadeSeconds;

    /// <summary>Seconds since the game started, for the voice limit and the ducks.</summary>
    private float _clock;

    /// <summary>Whether a fight is on (between BeginBattle and EndBattle).</summary>
    private bool _inBattle;

    /// <summary>The map's song when the fight started and where it had got to, to bring it back from there afterwards.</summary>
    private Track? _zoneTrack;
    private TimeSpan _zoneAt;

    /// <summary>The song to bring back once the jingle's over, and where from.</summary>
    private Track? _afterJingle;
    private TimeSpan _afterJingleAt;

    /// <summary>Seconds left of the jingle (or of the old sound effect standing in for it), after which the map's song comes back.</summary>
    private float _jingleLeft;

    /// <summary>How long the current song has been playing, in seconds, and where in it it started.</summary>
    /// <remarks>
    /// Together they say where the song has got to. MediaPlayer.PlayPosition can't be used for that
    /// on DesktopGL: it reports how far the stream has been read, which runs a second or two ahead
    /// of what's heard, a buffer at a time (measured: 2.8 s one second after starting).
    /// </remarks>
    private float _playingFor;
    private TimeSpan _startedFrom;

    /// <summary>The music's duck: the level it's at, the level it's heading for, and when it comes back up (on _clock).</summary>
    private float _duck = 1f;
    private float _duckTarget = 1f;
    private float _duckUntil;

    /// <summary>The volumes and the text speed. These get saved between sessions.</summary>
    public Settings Settings { get; }

    /// <summary>When true nothing plays, but every sound still gets written to Log.</summary>
    /// <remarks>For the proof run, so it doesn't blast music while it runs. Songs still run at no volume, and a jingle still takes its length, so the timing is real.</remarks>
    public bool Silent { get; set; }

    /// <summary>Every sound asked for while Silent is on, in order, so the proof run can check them.</summary>
    public List<string> Log { get; } = new();

    /// <summary>Whether a fight's music is on: from BeginBattle until EndBattle.</summary>
    public bool InBattle => _inBattle;

    /// <summary>The combat keys with no takes loaded, which PlayCombat skips. Empty when every file's there.</summary>
    public List<string> MissingCombatSounds { get; } = new();

    /// <summary>Creates the manager with the settings.</summary>
    /// <param name="game">The game it belongs to.</param>
    /// <param name="settings">The volumes.</param>
    public AudioManager(Game game, Settings settings) : base(game)
    {
        Settings = settings;
    }

    /// <summary>Loads every song and sound effect.</summary>
    /// <remarks>
    /// All up front. They're small, and loading a sound in the middle of the game would hitch the
    /// frame. The songs the maps use and every Sfx must be there (a missing one throws on the first
    /// frame, which is me forgetting a file, not something a player can do). The jingles, the
    /// stinger and the combat takes are skipped if they're missing, so a half-built Content folder
    /// still starts: EndBattle falls back to the old sound effects and PlayCombat to silence.
    /// </remarks>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        foreach (Track track in Enum.GetValues<Track>())
        {
            if (!IsJingle(track))
                _songs[track] = content.Load<Song>(SongName(track));
            else if (TryLoad<SoundEffect>(content, JingleName(track)) is SoundEffect jingle)
                _jingles[track] = jingle;
        }

        foreach (Sfx sfx in Enum.GetValues<Sfx>())
            _effects[sfx] = content.Load<SoundEffect>(SfxFolder + Kebab(sfx.ToString()));

        _engage = TryLoad<SoundEffect>(content, EngageName);

        foreach (string key in CombatSounds.Keys)
        {
            var takes = new List<SoundEffect>(CombatSounds.Takes);
            for (int take = 1; take <= CombatSounds.Takes; take++)
                if (TryLoad<SoundEffect>(content, $"{CombatSounds.Folder}{key}-{take}") is SoundEffect effect)
                    takes.Add(effect);

            if (takes.Count == 0)
            {
                MissingCombatSounds.Add(key);
                continue;
            }
            _combat[key] = takes.ToArray();
            _nextTake[key] = _random.Next(takes.Count);
        }

        MediaPlayer.IsRepeating = true;
    }

    /// <summary>Starts a song, fading out whatever's playing first. Asking for the song that's already playing does nothing.</summary>
    /// <remarks>
    /// Asking for a map's song while a fight's music is still on ends the fight's music the quiet way
    /// (no jingle), and if it's the song the fight interrupted it comes back from where it was. Asking
    /// for it while the jingle plays leaves the jingle to finish, since that song is coming back anyway.
    /// Victory or Defeat plays that jingle once, with nothing after it.
    /// </remarks>
    /// <param name="track">The song to play.</param>
    public void PlaySong(Track track)
    {
        if (_afterJingle == track) return;

        _afterJingle = null;
        StopJingle();
        if (IsJingle(track))
        {
            StopSong();
            PlayJingle(track);
            return;
        }

        TimeSpan? at = null;
        if (_inBattle && !IsBattleSong(track))
        {
            _inBattle = false;
            if (_zoneTrack == track) at = _zoneAt;
            _zoneTrack = null;
        }

        SwitchTo(track, at, FadeSeconds, FadeSeconds);
    }

    /// <summary>Fades the song out and plays nothing after it, for a scene that wants quiet. The next PlaySong starts again.</summary>
    public void StopSong()
    {
        _next = null;
        _nextAt = null;
        _afterJingle = null;
        StopJingle();
        if (_current is null) return;

        if (Silent) Log.Add("song none");
        _fadeOutSeconds = FadeSeconds;
        _fadingOut = true;
    }

    /// <summary>A fight starting: the engage stinger, and the battle song in fast under it. The map's song is remembered where it had got to, for EndBattle.</summary>
    /// <remarks>A second BeginBattle before EndBattle (a retry, a boss's own song) keeps the map song the first one remembered, and leaves the battle song running if it's the same one.</remarks>
    /// <param name="battleTrack">The fight's song: the encounter's Music, Battle or Boss.</param>
    public void BeginBattle(Track battleTrack)
    {
        if (Silent) Log.Add("battle begin " + battleTrack);

        if (!_inBattle)
        {
            RememberZone();
            _inBattle = true;
        }
        _afterJingle = null;
        StopJingle();

        PlayStinger();
        SwitchTo(battleTrack, null, BattleFadeOutSeconds, BattleFadeInSeconds);
        if (_engage is not null) Duck(StingerDuck, (float)_engage.Duration.TotalSeconds - 0.3f);
    }

    /// <summary>A fight over: the battle song goes, the victory or defeat jingle plays once, then the map's song comes back from where it was when the fight started.</summary>
    /// <remarks>Without the jingle's file it plays the turn-based fight's Victory or GameOver sound effect instead.</remarks>
    /// <param name="won">Whether the party won.</param>
    public void EndBattle(bool won)
    {
        if (Silent) Log.Add("battle end " + (won ? "won" : "lost"));

        Track? zone = _zoneTrack;
        TimeSpan zoneAt = _zoneAt;
        if (!_inBattle && _afterJingle is Track waiting)
        {
            // A second end (a lost fight won after all): the map's song still comes back after this jingle.
            zone = waiting;
            zoneAt = _afterJingleAt;
        }
        else if (!_inBattle)
        {
            zone = _current is Track now && !IsBattleSong(now) && !IsJingle(now) ? now : null;
            zoneAt = Position();
        }
        _inBattle = false;
        _zoneTrack = null;

        _next = null;
        _nextAt = null;
        if (_current is not null)
        {
            _fadeOutSeconds = JingleFadeOutSeconds;
            _fadingOut = true;
        }

        PlayJingle(won ? Track.Victory : Track.Defeat);
        _afterJingle = zone;
        _afterJingleAt = zoneAt;
    }

    /// <summary>Plays one combat sound by its key (README "Music and sound" lists them), turning through its takes with a small random nudge to the pitch.</summary>
    /// <remarks>
    /// The voice limit drops it if the same key has already played twice in the last 40 ms (a
    /// multi-hit skill, or three foes struck by one sweep), or ten combat sounds have. The big ones
    /// (CombatSounds.Ducks) dip the music for a moment so they land. An unknown key does nothing.
    /// </remarks>
    /// <param name="soundKey">The key, like hit-axe or cast-fire.</param>
    /// <param name="pan">Left or right, -1 to 1: where on screen it happened.</param>
    /// <param name="volume">How loud, before the settings, 0 to 1.</param>
    /// <returns>Whether it played (or, when Silent, would have).</returns>
    public bool PlayCombat(string soundKey, float pan = 0f, float volume = 1f)
    {
        if (!CombatSounds.IsKey(soundKey))
        {
            if (Silent) Log.Add($"combat {soundKey} (no such sound)");
            return false;
        }

        if (!TakeVoice(soundKey))
        {
            if (Silent) Log.Add($"combat {soundKey} (voice limit)");
            return false;
        }

        if (CombatSounds.Ducks(soundKey)) Duck(ImpactDuck, ImpactDuckHold);

        int take = 0;
        if (_combat.TryGetValue(soundKey, out SoundEffect[] takes))
        {
            take = _nextTake[soundKey];
            _nextTake[soundKey] = (take + 1) % takes.Length;
        }
        float spread = CombatSounds.PitchSpread(soundKey);
        float pitch = spread > 0f ? ((float)_random.NextDouble() * 2f - 1f) * spread : 0f;

        if (Silent)
        {
            Log.Add($"combat {soundKey} take {take + 1} pitch {pitch:+0.00;-0.00} pan {pan:+0.00;-0.00}" + (takes is null ? " (no file)" : ""));
            return takes is not null;
        }

        float level = Math.Clamp(volume * Settings.MasterVolume * Settings.SfxVolume, 0f, 1f);
        if (takes is null || level <= 0f) return false;

        return takes[take].Play(level, Math.Clamp(pitch, -1f, 1f), Math.Clamp(pan, -1f, 1f));
    }

    /// <summary>Dips the music for a moment, under something that has to land.</summary>
    /// <param name="level">How far down, as a level: 0.5 is half (about 6 dB down), 1 no dip.</param>
    /// <param name="hold">How long it stays down before it comes back up, in seconds.</param>
    public void Duck(float level, float hold)
    {
        _duckTarget = MathF.Min(_duckTarget, Math.Clamp(level, 0f, 1f));
        _duckUntil = MathF.Max(_duckUntil, _clock + MathF.Max(0f, hold));
    }

    /// <summary>Plays one sound effect.</summary>
    /// <param name="sfx">Which one.</param>
    /// <param name="volume">How loud, before the settings, 0 to 1.</param>
    /// <param name="pitch">Up or down, -1 to 1 (one octave either way).</param>
    /// <param name="pan">Left or right, -1 to 1.</param>
    public void Play(Sfx sfx, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        if (Silent)
        {
            Log.Add("sfx " + sfx);
            return;
        }

        float level = Math.Clamp(volume * Settings.MasterVolume * Settings.SfxVolume, 0f, 1f);
        if (!_effects.TryGetValue(sfx, out SoundEffect effect) || level <= 0f) return;

        effect.Play(level, Math.Clamp(pitch, -1f, 1f), Math.Clamp(pan, -1f, 1f));
    }

    /// <summary>Runs the fades and the ducks, and hands back to the map's song when a jingle ends.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public override void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _clock += seconds;
        if (_current is not null) _playingFor += seconds;

        if (_fadingOut)
        {
            _fade = MathF.Max(0f, _fade - seconds / _fadeOutSeconds);

            if (_fade <= 0f)
            {
                _fadingOut = false;
                MediaPlayer.Stop();
                _current = null;

                if (_next is Track next)
                {
                    TimeSpan? at = _nextAt;
                    _next = null;
                    _nextAt = null;
                    Start(next, at);
                }
            }
        }
        else if (_current is not null)
        {
            _fade = MathF.Min(1f, _fade + seconds / _fadeInSeconds);
        }

        if (_jingleLeft > 0f)
        {
            if (_jingle is not null) _jingle.Volume = JingleLevel();
            _jingleLeft -= seconds;
            if (_jingleLeft <= 0f) ResumeAfterJingle();
        }

        if (_clock >= _duckUntil) _duckTarget = 1f;
        if (_duck > _duckTarget) _duck = MathF.Max(_duckTarget, _duck - seconds / DuckAttackSeconds);
        else _duck = MathF.Min(_duckTarget, _duck + seconds / DuckReleaseSeconds);

        MediaPlayer.Volume = Silent ? 0f : Settings.MasterVolume * Settings.MusicVolume * _fade * _duck;

        base.Update(gameTime);
    }

    /// <summary>Moves to a song: nothing if it's already on, straight in if nothing is, otherwise once the current one has faded out.</summary>
    /// <param name="track">The song.</param>
    /// <param name="at">Where in it to start, or null for the top.</param>
    /// <param name="fadeOut">How long the current song takes to go, in seconds.</param>
    /// <param name="fadeIn">How long the new one takes to come in, in seconds.</param>
    private void SwitchTo(Track track, TimeSpan? at, float fadeOut, float fadeIn)
    {
        _fadeInSeconds = fadeIn;

        // Already playing this one (even if it was fading out), so just bring it back up instead of restarting it.
        if (_current == track)
        {
            _next = null;
            _nextAt = null;
            _fadingOut = false;
            return;
        }

        if (_next == track) return;

        if (Silent) Log.Add("song " + track + (at is TimeSpan from ? $" from {from.TotalSeconds:0.0} s" : ""));

        if (_current is null)
        {
            Start(track, at);
            return;
        }

        _next = track;
        _nextAt = at;
        _fadeOutSeconds = fadeOut;
        _fadingOut = true;
    }

    /// <summary>Starts a song at 0 volume, from the top or from a remembered place. Update fades it in.</summary>
    /// <param name="track">The song.</param>
    /// <param name="at">Where to start, or null for the top.</param>
    private void Start(Track track, TimeSpan? at = null)
    {
        if (!_songs.TryGetValue(track, out Song song)) return;

        _current = track;
        _fade = 0f;
        _playingFor = 0f;
        MediaPlayer.Volume = 0f;

        TimeSpan from = at ?? TimeSpan.Zero;
        if (from > TimeSpan.Zero && song.Duration > TimeSpan.Zero)
        {
            from = TimeSpan.FromTicks(from.Ticks % song.Duration.Ticks);
            if ((song.Duration - from).TotalSeconds < ResumeNearEndSeconds) from = TimeSpan.Zero;
        }

        _startedFrom = from;
        if (from > TimeSpan.Zero) MediaPlayer.Play(song, from);
        else MediaPlayer.Play(song);
    }

    /// <summary>Plays a jingle once, over whatever song is fading out, at the music's volume. The turn-based fight's Victory or GameOver sound effect stands in if its file isn't there.</summary>
    /// <param name="track">Victory or Defeat.</param>
    private void PlayJingle(Track track)
    {
        StopJingle();
        if (Silent) Log.Add("jingle " + track + (_jingles.ContainsKey(track) ? "" : " (no file)"));

        if (!_jingles.TryGetValue(track, out SoundEffect jingle))
        {
            Sfx fallback = track == Track.Victory ? Sfx.Victory : Sfx.GameOver;
            Play(fallback);
            _jingleLeft = _effects.TryGetValue(fallback, out SoundEffect effect) ? MathF.Max(0.1f, (float)effect.Duration.TotalSeconds) : 0.1f;
            return;
        }

        _jingleLeft = MathF.Max(0.1f, (float)jingle.Duration.TotalSeconds);
        if (Silent) return;
        _jingle = jingle.CreateInstance();
        _jingle.IsLooped = false;
        _jingle.Volume = JingleLevel();
        _jingle.Play();
    }

    /// <summary>Cuts a jingle off, for something that wants different music before it's done.</summary>
    private void StopJingle()
    {
        _jingleLeft = 0f;
        if (_jingle is null) return;
        _jingle.Stop();
        _jingle.Dispose();
        _jingle = null;
    }

    /// <summary>How loud a jingle plays: the music's volume, without the fade or the duck.</summary>
    private float JingleLevel() => Math.Clamp(Settings.MasterVolume * Settings.MusicVolume, 0f, 1f);

    /// <summary>The jingle's done: the map's song comes back, slowly, from where it was.</summary>
    private void ResumeAfterJingle()
    {
        _jingleLeft = 0f;
        if (_jingle is not null)
        {
            _jingle.Dispose();
            _jingle = null;
        }
        if (_afterJingle is not Track zone) return;

        _afterJingle = null;
        if (Silent) Log.Add("jingle over");
        SwitchTo(zone, _afterJingleAt, FadeSeconds, ResumeFadeInSeconds);
    }

    /// <summary>Notes the map's song and where it's got to, as a fight starts. A song still waiting to come in counts, from where it was going to start.</summary>
    private void RememberZone()
    {
        if (_afterJingle is Track waiting)
        {
            // Straight into another fight while the last one's jingle played: the same map song is still the one to come back to.
            _zoneTrack = waiting;
            _zoneAt = _afterJingleAt;
        }
        else if (_next is Track queued && !IsBattleSong(queued) && !IsJingle(queued))
        {
            _zoneTrack = queued;
            _zoneAt = _nextAt ?? TimeSpan.Zero;
        }
        else if (_current is Track now && !IsBattleSong(now) && !IsJingle(now))
        {
            _zoneTrack = now;
            _zoneAt = Position();
        }
        else
        {
            _zoneTrack = null;
            _zoneAt = TimeSpan.Zero;
        }

        if (Silent && _zoneTrack is Track zone) Log.Add($"remember {zone} at {_zoneAt.TotalSeconds:0.0} s");
    }

    /// <summary>Plays the engage stinger, at the music's volume since it's part of the score.</summary>
    private void PlayStinger()
    {
        if (Silent)
        {
            Log.Add(_engage is null ? "stinger engage (no file)" : "stinger engage");
            return;
        }
        if (_engage is null) return;

        float level = Math.Clamp(Settings.MasterVolume * Settings.MusicVolume, 0f, 1f);
        if (level > 0f) _engage.Play(level, 0f, 0f);
    }

    /// <summary>Takes a voice for a combat sound if the limit allows it, and counts it.</summary>
    /// <param name="key">The sound's key.</param>
    private bool TakeVoice(string key)
    {
        if (!_recent.TryGetValue(key, out Queue<float> times)) _recent[key] = times = new Queue<float>();
        float since = _clock - VoiceWindow;
        while (times.Count > 0 && times.Peek() <= since) times.Dequeue();
        while (_recentAll.Count > 0 && _recentAll.Peek() <= since) _recentAll.Dequeue();

        if (times.Count >= VoicesPerKey || _recentAll.Count >= VoicesInAll) return false;

        times.Enqueue(_clock);
        _recentAll.Enqueue(_clock);
        return true;
    }

    /// <summary>Where the current song has got to: where it started, plus how long it's played, round its length if it's gone round.</summary>
    private TimeSpan Position()
    {
        if (_current is not Track track || !_songs.TryGetValue(track, out Song song)) return TimeSpan.Zero;

        TimeSpan at = _startedFrom + TimeSpan.FromSeconds(_playingFor);
        return song.Duration > TimeSpan.Zero ? TimeSpan.FromTicks(at.Ticks % song.Duration.Ticks) : at;
    }

    /// <summary>Whether a song is one of the fight songs, which a fight never comes back to.</summary>
    /// <param name="track">The song.</param>
    private static bool IsBattleSong(Track track) => track is Track.Battle or Track.Boss;

    /// <summary>Whether a song is a jingle that plays once.</summary>
    /// <param name="track">The song.</param>
    private static bool IsJingle(Track track) => track is Track.Victory or Track.Defeat;

    /// <summary>Loads a piece of content, or gives null if its file isn't there.</summary>
    /// <param name="content">The content manager.</param>
    /// <param name="name">The asset's name.</param>
    private static T TryLoad<T>(ContentManager content, string name) where T : class
    {
        try
        {
            return content.Load<T>(name);
        }
        catch (ContentLoadException)
        {
            return null;
        }
    }

    /// <summary>The content name for a song. The files are named after the song, not the screen, so they're spelled out here.</summary>
    /// <remarks>Throws for a track with no file, which happens in LoadContent on the first frame. That's me forgetting a line here, not something a player can do.</remarks>
    /// <param name="track">The song.</param>
    private static string SongName(Track track) => track switch
    {
        Track.Title => "Music/embers",
        Track.Exploring => "Music/hearthsmoke",
        Track.Underground => "Music/cold-stone",
        Track.Sea => "Music/salt-and-lanterns",
        Track.Meadow => "Music/meadowlark",
        Track.Court => "Music/sunspire",
        Track.Battle => "Music/steel",
        Track.Boss => "Music/the-hunger",
        _ => throw new ArgumentOutOfRangeException(nameof(track), track, "There's no song file for this track."),
    };

    /// <summary>The content name for a jingle. They're sound effects, beside the songs.</summary>
    /// <param name="track">Victory or Defeat.</param>
    private static string JingleName(Track track) => track == Track.Victory ? "Music/victory" : "Music/defeat";

    /// <summary>Turns an enum name into its file name, like MenuMove to menu-move.</summary>
    /// <param name="name">The PascalCase name.</param>
    private static string Kebab(string name)
    {
        var kebab = new StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0) kebab.Append('-');
            kebab.Append(char.ToLowerInvariant(name[i]));
        }
        return kebab.ToString();
    }
}
