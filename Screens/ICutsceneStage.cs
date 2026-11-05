using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Dialogue;

namespace LegendQuest.Screens;

/// <summary>
/// Where a conversation's stage cues play out: whatever's under it with people and places on it.
/// CutsceneDirector says what happens and when; this does it to the world.
/// </summary>
/// <remarks>
/// Everything goes through in the script's own words (an actor id, a spawn's name, a map's) and as
/// world positions, never as tiles, so the tile maps and the side-on rooms that replace them can
/// both be one. Anything that takes time (walking, the camera going somewhere, a tint coming over)
/// is started by one call and goes on in Advance, so the director keeps the clock and can ask
/// whether it's done. A call naming nobody on the stage does nothing; the director has already
/// asked Actors and logged it.
/// <para>
/// Who an id names (build plan §5.4): you, soulbound, party (everyone in it) and both (you and the
/// Soul-Bound); a companion by their origin id while they're in the party; someone on the map by
/// their id, or by their cast id, which is everyone the map puts down as that one of the cast;
/// copies by their number (royal-guard-2). An id that's a group (a cast id several stand as, or
/// whoever a place cue made copies of) names all of them, and so does its plural (royal-guards).
/// </para>
/// </remarks>
public interface ICutsceneStage
{
    /// <summary>Everyone an actor id names who's on the stage now, one key each, in a steady order. A key names that one person again: their origin id in the party, their id on the map, or a copy's.</summary>
    /// <param name="id">The actor id, as a cue has it.</param>
    /// <returns>Their keys, or none if nobody it names is here.</returns>
    IReadOnlyList<string> Actors(string id);

    /// <summary>Where a spawn is, by its name.</summary>
    /// <param name="name">The spawn's name.</param>
    /// <param name="at">Where it is, in world pixels.</param>
    /// <returns>False if there's no spawn called that here.</returns>
    bool TryGetSpawn(string name, out Vector2 at);

    /// <summary>Where a place cue's spot is: a spawn, beside someone, on, before or over a prop or someone, or a free spot in the crowd.</summary>
    /// <param name="spot">The spot, in the cue's words.</param>
    /// <param name="at">Where it is, in world pixels.</param>
    /// <returns>False if it names a spawn, a prop or someone that isn't here.</returns>
    bool TryGetSpot(CueSpot spot, out Vector2 at);

    /// <summary>Puts someone at a spot at once, turned the way it says (or the way a spawn faces, if it doesn't). Someone who isn't here but is one of the cast or the creatures is brought on; copies are made round the spot, numbered from 1.</summary>
    /// <param name="actor">Who.</param>
    /// <param name="spot">Where.</param>
    /// <param name="facing">Which way they face after, or null to leave it to the spot.</param>
    /// <param name="copies">How many copies to make of them, or 0 for them alone.</param>
    /// <returns>False if there was nobody to put there: they aren't here and can't be brought on, or the spot isn't.</returns>
    bool Place(string actor, CueSpot spot, CueFacing? facing, int copies);

    /// <summary>Starts someone walking somewhere along the stage's own paths. A group goes to spots round it.</summary>
    /// <param name="actor">Who.</param>
    /// <param name="to">Where, in world pixels.</param>
    void Walk(string actor, Vector2 to);

    /// <summary>Whether someone's still on their way somewhere a walk sent them.</summary>
    /// <param name="key">Who, by one of Actors' keys.</param>
    bool IsWalking(string key);

    /// <summary>Turns someone left or right.</summary>
    /// <param name="actor">Who.</param>
    /// <param name="way">Which way.</param>
    void Face(string actor, CueFacing way);

    /// <summary>Turns someone to face someone else.</summary>
    /// <param name="actor">Who.</param>
    /// <param name="other">Who they face.</param>
    void FaceToward(string actor, string other);

    /// <summary>Plays a clip on someone, toward someone or something if it says.</summary>
    /// <param name="actor">Who.</param>
    /// <param name="clip">The clip's name.</param>
    /// <param name="partner">Who or what it's at, or null.</param>
    /// <returns>How long it takes, in seconds, before their next cue can go: 0 for a clip there's nothing to show for here.</returns>
    float Animate(string actor, string clip, string partner);

    /// <summary>Lets go of the poses the last line's clips struck, as the next line's cues start. Someone downed stays down.</summary>
    void EndGestures();

    /// <summary>Puts a glow round someone, or takes it off.</summary>
    /// <param name="actor">Who.</param>
    /// <param name="colour">The glow's colour, or null for none.</param>
    void Glow(string actor, Color? colour);

    /// <summary>Makes someone a memory: there, but half seen.</summary>
    /// <param name="actor">Who.</param>
    void Echo(string actor);

    /// <summary>Shows something that's hidden till it's wanted, or someone, at a place if it says.</summary>
    /// <param name="thing">Its id on the map, or an actor id.</param>
    /// <param name="at">Where, in world pixels, or null to leave it where it is.</param>
    /// <returns>False if there's nothing called that here.</returns>
    bool Show(string thing, Vector2? at);

    /// <summary>Hides something, or someone.</summary>
    /// <param name="thing">Its id on the map, or an actor id.</param>
    /// <returns>False if there's nothing called that here.</returns>
    bool Hide(string thing);

    /// <summary>Changes someone to another of their looks.</summary>
    /// <param name="actor">Who.</param>
    /// <param name="look">The look's name: one of their Cast entry's, or their own id for the usual one.</param>
    /// <returns>False if they haven't a look called that.</returns>
    bool Swap(string actor, string look);

    /// <summary>Brings a tint over everything on the stage, or takes it off back to the stage's own.</summary>
    /// <param name="colour">The tint, or null for the stage's own.</param>
    /// <param name="seconds">How long it takes to come over.</param>
    void Tint(Color? colour, float seconds);

    /// <summary>Starts the camera going to look at a point.</summary>
    /// <param name="at">The point, in world pixels.</param>
    void Camera(Vector2 at);

    /// <summary>Whether the camera's still on its way where Camera sent it.</summary>
    bool CameraMoving { get; }

    /// <summary>Shakes the stage.</summary>
    /// <param name="strength">How hard, more than 0 and up to 3.</param>
    void Shake(float strength);

    /// <summary>Shows the Soul-Link's sigil on the screen in a state, or flashes it into one for a frame.</summary>
    /// <param name="state">The state.</param>
    /// <param name="oneFrame">Whether it's only a flash, after which it goes back to how it was.</param>
    void Sigil(SigilState state, bool oneFrame);

    /// <summary>Takes someone to another map or zone mid-scene, onto a spawn there; you takes the whole party.</summary>
    /// <param name="actor">Who.</param>
    /// <param name="mapOrZone">Where to, by its id.</param>
    /// <param name="spawn">The spawn they arrive at.</param>
    void TravelActor(string actor, string mapOrZone, string spawn);

    /// <summary>Runs everything that's going on a frame on: the walking, the clips, the camera, the tint, the shake.</summary>
    /// <param name="seconds">How long the frame is, slowed if a slow cue says.</param>
    void Advance(float seconds);

    /// <summary>Puts everything that's going on at its end at once: everyone walking where they were going, the camera there, the tint over. For a skip.</summary>
    void Finish();

    /// <summary>The scene's over: lets go of every pose, glow, echo and sigil it put up. Where people were put and the tint stay.</summary>
    void Wrap();
}
