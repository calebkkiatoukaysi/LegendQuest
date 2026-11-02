using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;
using LegendQuest.Particles;

namespace LegendQuest.Components;

/// <summary>
/// What a fight throws about on the map: sparks off a blow, motes of coloured light, the arrows and
/// bolts in flight, and the shake of a hit.
/// </summary>
/// <remarks>
/// The Black Box's TableScreen.Effects as a component of its own, since a fight on a map has more
/// going on than a table. Everything here is in world pixels and goes through the camera. The
/// particles are additive (they're light), a system for each colour asked for, made the first time
/// it's wanted. What flies is hud.png's art, pointing right with its middle at the frame's middle,
/// turned to the way it's going and animated through its frames; a flask goes up and over in an
/// arc, everything else straight. Each leaves a few motes of its colour behind it. The screen says
/// when each thing lands (it knows how long it gave it); this only draws it on the way.
/// </remarks>
public class CombatEffects
{
    /// <summary>How many bursts of one colour of sparks, and clouds of one colour of motes, can be in the air at once.</summary>
    private const int BurstsPerColour = 8;
    private const int CloudsPerColour = 10;

    /// <summary>How long each frame of something flying is up, in seconds.</summary>
    private const float FlightFrameSeconds = 0.06f;

    /// <summary>How high a lobbed flask goes at the top of its arc, as a share of how far it's going, and the least and most that comes to in world pixels.</summary>
    private const float ArcShare = 0.35f;
    private const float ArcLeast = 10f;
    private const float ArcMost = 36f;

    /// <summary>How often something flying drops a mote behind it, in seconds, and the little area a mote starts in, in world pixels.</summary>
    private const float TrailSeconds = 0.035f;
    private const int TrailArea = 3;

    /// <summary>The hud.png piece that's lobbed rather than flown straight.</summary>
    private const string Lobbed = "flask";

    private readonly ScreenShake _shake = new();
    private readonly Dictionary<Color, SparkParticleSystem> _sparks = new();
    private readonly Dictionary<Color, MoteParticleSystem> _motes = new();
    private readonly List<Flight> _flying = new();

    private ContentManager _content;
    private Texture2D _hud;

    /// <summary>One thing in the air.</summary>
    private sealed class Flight
    {
        /// <summary>What it is, by its hud.png piece.</summary>
        public string Piece;

        /// <summary>Where it left, in world pixels.</summary>
        public Vector2 From;

        /// <summary>Where it lands.</summary>
        public Vector2 To;

        /// <summary>How long it takes to get there.</summary>
        public float Seconds;

        /// <summary>How long it's been going.</summary>
        public float Clock;

        /// <summary>How high it goes at the top of its arc, or 0 for straight.</summary>
        public float Arc;

        /// <summary>The colour of the motes it leaves behind, or null for none.</summary>
        public Color? Trail;

        /// <summary>How long since it last dropped a mote.</summary>
        public float TrailClock;
    }

    /// <summary>The shake's matrix, to multiply after the camera's.</summary>
    public Matrix Shake => _shake.Transform;

    /// <summary>Loads hud.png, which the things that fly are cut from, and keeps the content manager to load each colour's particles with.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _content = content;
        _hud = content.Load<Texture2D>(CombatIcons.HudSheet);
        CombatIcons.CheckSheet(CombatIcons.HudSheet, _hud);
    }

    /// <summary>Shakes the screen for a hit.</summary>
    /// <param name="amount">How hard, from 0 to 1.</param>
    /// <param name="focus">Where it landed, in screen pixels.</param>
    public void Kick(float amount, Vector2 focus) => _shake.Kick(amount, focus);

    /// <summary>Throws a burst of sparks.</summary>
    /// <param name="where">Where from, in world pixels.</param>
    /// <param name="colour">The colour they cool through.</param>
    /// <param name="facing">Which way the middle of the burst goes, in radians. Up the screen is -pi/2.</param>
    /// <param name="spread">How far either side of that they fan, in radians.</param>
    public void Sparks(Vector2 where, Color colour, float facing, float spread)
    {
        if (!_sparks.TryGetValue(colour, out SparkParticleSystem system))
        {
            system = new SparkParticleSystem(BurstsPerColour, colour);
            system.LoadContent(_content);
            _sparks[colour] = system;
        }
        system.Burst(where, facing, spread);
    }

    /// <summary>Lets a cloud of motes go.</summary>
    /// <param name="centre">The middle of where they start, in world pixels.</param>
    /// <param name="size">How big an area they start in, in world pixels.</param>
    /// <param name="colour">Their colour.</param>
    /// <param name="falling">Whether they drift down (light falling on someone) rather than up.</param>
    public void Motes(Vector2 centre, Point size, Color colour, bool falling)
    {
        if (!_motes.TryGetValue(colour, out MoteParticleSystem system))
        {
            system = new MoteParticleSystem(CloudsPerColour, colour);
            system.LoadContent(_content);
            _motes[colour] = system;
        }
        system.Release(new Rectangle((int)(centre.X - size.X / 2f), (int)(centre.Y - size.Y / 2f), Math.Max(1, size.X), Math.Max(1, size.Y)), falling);
    }

    /// <summary>Sends something flying.</summary>
    /// <param name="piece">What, by its hud.png piece: an arrow, a fire bolt.</param>
    /// <param name="from">Where it leaves, in world pixels.</param>
    /// <param name="to">Where it lands.</param>
    /// <param name="seconds">How long it takes.</param>
    /// <param name="trail">The colour of the motes it leaves behind, or null for none (an arrow).</param>
    public void Launch(string piece, Vector2 from, Vector2 to, float seconds, Color? trail)
    {
        float arc = piece == Lobbed ? MathHelper.Clamp(Vector2.Distance(from, to) * ArcShare, ArcLeast, ArcMost) : 0f;
        _flying.Add(new Flight { Piece = piece, From = from, To = to, Seconds = MathF.Max(seconds, float.Epsilon), Arc = arc, Trail = trail });
    }

    /// <summary>Moves the particles and what's flying on, drops the trails, and lets the shake settle.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        foreach (SparkParticleSystem system in _sparks.Values) system.Update(gameTime);
        foreach (MoteParticleSystem system in _motes.Values) system.Update(gameTime);
        _shake.Update(gameTime);

        for (int i = _flying.Count - 1; i >= 0; i--)
        {
            Flight flight = _flying[i];
            flight.Clock += seconds;
            if (flight.Clock >= flight.Seconds)
            {
                _flying.RemoveAt(i);
                continue;
            }

            if (flight.Trail is not Color trail) continue;
            flight.TrailClock += seconds;
            if (flight.TrailClock < TrailSeconds) continue;
            flight.TrailClock = 0f;
            Motes(Where(flight), new Point(TrailArea, TrailArea), trail, false);
        }
    }

    /// <summary>Draws the sparks and the motes, each system in its own additive batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to draw with, between batches.</param>
    /// <param name="world">The camera's matrix, the shake included.</param>
    public void DrawParticles(SpriteBatch spriteBatch, Matrix world)
    {
        foreach (MoteParticleSystem system in _motes.Values) system.Draw(spriteBatch, world);
        foreach (SparkParticleSystem system in _sparks.Values) system.Draw(spriteBatch, world);
    }

    /// <summary>Draws whatever's flying, turned the way it's going.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with, in world space, point-sampled, deferred.</param>
    public void DrawFlying(SpriteBatch spriteBatch)
    {
        foreach (Flight flight in _flying)
        {
            Rectangle source = CombatIcons.Piece(flight.Piece, (int)(flight.Clock / FlightFrameSeconds));
            Vector2 heading = flight.To - flight.From;
            float t = flight.Clock / flight.Seconds;
            heading.Y -= flight.Arc * (4f - 8f * t);
            float angle = MathF.Atan2(heading.Y, heading.X);

            Vector2 at = Where(flight);
            spriteBatch.Draw(_hud, new Vector2(MathF.Round(at.X), MathF.Round(at.Y)), source, Color.White, angle,
                new Vector2(source.Width / 2f, source.Height / 2f), 1f, SpriteEffects.None, 0f);
        }
    }

    /// <summary>Lets go of everything at once, when the fight's over.</summary>
    public void Clear() => _flying.Clear();

    /// <summary>Where something flying is now: along the straight line, and up its arc if it's lobbed.</summary>
    private static Vector2 Where(Flight flight)
    {
        float t = MathHelper.Clamp(flight.Clock / flight.Seconds, 0f, 1f);
        return Vector2.Lerp(flight.From, flight.To, t) - new Vector2(0f, flight.Arc * 4f * t * (1f - t));
    }
}
