using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters.Motion;
using LegendQuest.Combat;
using LegendQuest.Particles;

namespace LegendQuest.Components;

/// <summary>The school of an effect: a weapon's plain steel, or the element a spell's made of. Each has its own colour, burst, projectile and sound.</summary>
public enum Element
{
    /// <summary>Steel, wood and fists: sparks and the arc of the blow.</summary>
    Physical,

    /// <summary>Embers rising and a lick of flame.</summary>
    Fire,

    /// <summary>Splinters of ice and a cold puff.</summary>
    Frost,

    /// <summary>Forked light.</summary>
    Lightning,

    /// <summary>Rays and falling gold.</summary>
    Radiant,

    /// <summary>Dark wisps sinking, and a sickly light.</summary>
    Necrotic,

    /// <summary>A clean shockwave.</summary>
    Force,

    /// <summary>Ripples of rose light.</summary>
    Psychic,

    /// <summary>Leaves and green light.</summary>
    Nature,

    /// <summary>A heavy ring and dust off the floor.</summary>
    Thunder,

    /// <summary>Bubbles and green drops.</summary>
    Poison,

    /// <summary>Red drops and streaks.</summary>
    Blood,
}

/// <summary>What a ground marker covers.</summary>
public enum MarkerShape
{
    /// <summary>A circle round a point.</summary>
    Ring,

    /// <summary>A band from one point toward another.</summary>
    Line,

    /// <summary>A wedge out from a point.</summary>
    Cone,
}

/// <summary>
/// What a fight throws about on the map: sparks off a blow and the arc it cuts, a spell's burst in
/// its school's colours, the bolts and arrows in flight with their trails, beams, the markers that
/// fill on the floor before a big attack lands, dust off a body landing, motes off one dying, and the
/// shake of a hit.
/// </summary>
/// <remarks>
/// <para>
/// The Black Box's TableScreen.Effects as a component of its own, since a fight on a map has more
/// going on than a table. Everything here is in world pixels and goes through the camera. The old
/// sparks and motes (a particle system a colour, made the first time it's wanted) are kept for the
/// turn-based screen's calls. What flies from hud.png is that sheet's art, pointing right with its
/// middle at the frame's middle, turned to the way it's going and animated through its frames; a
/// flask goes up and over in an arc, everything else straight.
/// </para>
/// <para>
/// The real-time fight's effects are drawn from shapes made in code (Particles/EffectTextures), so
/// they're all LegendQuest's own: one pool of particles (Particles/EffectParticles) for every school's
/// burst, and bands of light (Characters/Motion/RibbonBatch) for trails, beams, forked lightning and
/// the ground markers. Impact puts sparks out along the swing, the arc of the blow (a crescent for a
/// blade, a flare and a ring for a blunt blow, a streak for a thrust) and, for a spell, its burst.
/// Projectile flies a school's bolt with a ribbon behind it and bursts where it lands. Mark puts a
/// marker on the floor that fills from its middle over its wind-up and flashes as it resolves (the
/// moment the blow lands); stepping out of it before then is the point. Draw them in this order: the
/// markers (DrawGround) after the room's floor and before the people; then DrawParticles and
/// DrawFlying after the people.
/// </para>
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

    /// <summary>A bolt's ribbon: how many of its last places it keeps, and how wide it is at the head, in world pixels.</summary>
    private const int BoltTrail = 10;
    private const float BoltWidth = 5f;

    /// <summary>A ground marker's flash as it resolves, and its fade after, in seconds.</summary>
    private const float MarkerFlash = 0.06f;
    private const float MarkerFade = 0.3f;

    /// <summary>How deep the floor is drawn against how wide, for a ring on it: 1 draws it as the engine measures it, a true circle in world pixels.</summary>
    public static float FloorDepth { get; set; } = 1f;

    /// <summary>The warning colour of a foe's ground marker.</summary>
    public static readonly Color Warning = new(255, 84, 40);

    private readonly ScreenShake _shake = new();
    private readonly Dictionary<Color, SparkParticleSystem> _sparks = new();
    private readonly Dictionary<Color, MoteParticleSystem> _motes = new();
    private readonly List<Flight> _flying = new();
    private readonly EffectParticles _particles = new();
    private readonly List<Bolt> _bolts = new();
    private readonly List<Beam> _beams = new();
    private readonly List<Fork> _forks = new();
    private readonly List<Marker> _markers = new();
    private readonly List<Charge> _charges = new();
    private readonly Random _random = new(5);
    private int _nextMarker = 1;

    private ContentManager _content;
    private Texture2D _hud;
    private EffectTextures _textures;
    private RibbonBatch _ribbons;

    /// <summary>One thing from hud.png in the air.</summary>
    private sealed class Flight
    {
        public string Piece;
        public Vector2 From, To;
        public float Seconds, Clock, Arc;
        public Color? Trail;
        public float TrailClock;
    }

    /// <summary>A school's bolt in the air: where from and to, how long, how far along, its arc, the places it's been, whether it bursts when it lands, and its emitter's clock.</summary>
    private sealed class Bolt
    {
        public Element Element;
        public Vector2 From, To;
        public float Seconds, Clock, Arc, Emit;
        public bool Bursts;
        public readonly List<Vector2> Trail = new();
    }

    /// <summary>A beam: from, to, how long and how far through.</summary>
    private sealed class Beam
    {
        public Element Element;
        public Vector2 From, To;
        public float Seconds, Clock, Emit;
    }

    /// <summary>A fork of lightning: its points, its colour, how long it lasts and how long it's been.</summary>
    private sealed class Fork
    {
        public Vector2[] Points;
        public Color Colour;
        public float Life, Clock, Width;
    }

    /// <summary>A charge gathering in a hand: where the hand is, its colour, how long it grows and how long it's been.</summary>
    private sealed class Charge
    {
        public Func<Vector2> Where;
        public Element Element;
        public float Seconds, Clock, Emit;
    }

    /// <summary>A ground marker: its shape and where, its colour, how long it fills and how far it's got, and when it resolved.</summary>
    private sealed class Marker
    {
        public int Id;
        public MarkerShape Shape;
        public Vector2 At, Toward;
        public float Size, Width, HalfAngle;
        public Color Colour;
        public float Seconds, Clock;
        public float Resolved = -1f;
    }

    /// <summary>The shake's matrix, to multiply after the camera's.</summary>
    public Matrix Shake => _shake.Transform;

    /// <summary>The shake itself, for its strength (the options' setting) and the kicks by cause.</summary>
    public ScreenShake Shaker => _shake;

    /// <summary>Loads hud.png, which the things that fly are cut from, keeps the content manager to load each colour's particles with, and makes the effects' own shapes.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _content = content;
        _hud = content.Load<Texture2D>(CombatIcons.HudSheet);
        CombatIcons.CheckSheet(CombatIcons.HudSheet, _hud);
        if (content.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) is IGraphicsDeviceService graphics)
        {
            _textures ??= new EffectTextures(graphics.GraphicsDevice);
            _ribbons ??= new RibbonBatch(graphics.GraphicsDevice);
        }
    }

    /// <summary>The ribbon batch the effects draw with, for the figures' weapon trails too (WalkerSprite.DrawTrail).</summary>
    public RibbonBatch Ribbons => _ribbons;

    /// <summary>An element's colour: what its bursts, bolts and tint are.</summary>
    public static Color ColourOf(Element element) => element switch
    {
        Element.Fire => new Color(255, 128, 36),
        Element.Frost => new Color(140, 214, 255),
        Element.Lightning => new Color(200, 196, 255),
        Element.Radiant => new Color(255, 222, 120),
        Element.Necrotic => new Color(150, 220, 150),
        Element.Force => new Color(150, 170, 255),
        Element.Psychic => new Color(255, 120, 210),
        Element.Nature => new Color(120, 220, 90),
        Element.Thunder => new Color(180, 196, 230),
        Element.Poison => new Color(160, 230, 70),
        Element.Blood => new Color(220, 30, 40),
        _ => new Color(255, 214, 150),
    };

    /// <summary>The element a kind of damage looks like: steel for slashing, piercing and bludgeoning, frost for cold, nature for acid, else its own.</summary>
    public static Element ElementOf(DamageType kind) => kind switch
    {
        DamageType.Fire => Element.Fire,
        DamageType.Cold => Element.Frost,
        DamageType.Lightning => Element.Lightning,
        DamageType.Thunder => Element.Thunder,
        DamageType.Radiant => Element.Radiant,
        DamageType.Necrotic => Element.Necrotic,
        DamageType.Poison => Element.Poison,
        DamageType.Psychic => Element.Psychic,
        DamageType.Force => Element.Force,
        DamageType.Acid => Element.Nature,
        _ => Element.Physical,
    };

    /// <summary>The element a school's name means ("fire", "frost", "cold", "lightning", "radiant", "necrotic", "force", "psychic", "nature", "thunder", "poison", "blood"), or Physical for anything else.</summary>
    public static Element ElementOf(string school) => school?.ToLowerInvariant() switch
    {
        "fire" => Element.Fire,
        "frost" or "cold" or "ice" => Element.Frost,
        "lightning" => Element.Lightning,
        "radiant" => Element.Radiant,
        "necrotic" => Element.Necrotic,
        "force" => Element.Force,
        "psychic" => Element.Psychic,
        "nature" or "acid" => Element.Nature,
        "thunder" => Element.Thunder,
        "poison" => Element.Poison,
        "blood" => Element.Blood,
        _ => Element.Physical,
    };

    /// <summary>Shakes the screen for a hit.</summary>
    /// <param name="amount">How hard, from 0 to 1.</param>
    /// <param name="focus">Where it landed, in screen pixels.</param>
    public void Kick(float amount, Vector2 focus) => _shake.Kick(amount, focus);

    /// <summary>Throws a burst of sparks, the old way: one colour cooling.</summary>
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

    /// <summary>Sends something from hud.png flying (an arrow, a fire bolt, a flask).</summary>
    /// <param name="piece">What, by its hud.png piece.</param>
    /// <param name="from">Where it leaves, in world pixels.</param>
    /// <param name="to">Where it lands.</param>
    /// <param name="seconds">How long it takes.</param>
    /// <param name="trail">The colour of the motes it leaves behind, or null for none (an arrow).</param>
    public void Launch(string piece, Vector2 from, Vector2 to, float seconds, Color? trail)
    {
        float arc = piece == Lobbed ? MathHelper.Clamp(Vector2.Distance(from, to) * ArcShare, ArcLeast, ArcMost) : 0f;
        _flying.Add(new Flight { Piece = piece, From = from, To = to, Seconds = MathF.Max(seconds, float.Epsilon), Arc = arc, Trail = trail });
    }

    /// <summary>
    /// A blow landing: hot streaks out along the swing (and gold ones too on a critical), the arc of
    /// the blow (a crescent along the swing for a blade, a flare and a ring for a blunt blow, a streak
    /// for a thrust), and for a spell its school's burst.
    /// </summary>
    /// <param name="where">The contact point, in world pixels: the weapon's tip, or the target's chest.</param>
    /// <param name="swing">Which way the blow was going (WalkerSprite.SwingDirection, or from the attacker to the target).</param>
    /// <param name="element">Its school: Physical for a weapon.</param>
    /// <param name="critical">Whether it was a critical: more of everything, and gold.</param>
    /// <param name="heavy">Whether it was a heavy skill: bigger.</param>
    /// <param name="blow">For a weapon, how it lands: "slash", "blunt" or "thrust".</param>
    public void Impact(Vector2 where, Vector2 swing, Element element, bool critical = false, bool heavy = false, string blow = "slash")
    {
        if (_textures is null) return;
        Vector2 dir = swing.LengthSquared() > 1e-4f ? Vector2.Normalize(swing) : Vector2.UnitX;
        float angle = MathF.Atan2(dir.Y, dir.X);
        float big = (heavy ? 1.35f : 1f) * (critical ? 1.25f : 1f);
        Color colour = ColourOf(element);

        // The white of the impact itself.
        Pop(EffectShape.Star, where, 0f, 10f * big, 26f * big, Color.White, 0.1f, angle + MathHelper.PiOver4 * 0.5f);
        if (element == Element.Physical)
        {
            switch (blow)
            {
                case "blunt":
                    Pop(EffectShape.Ring, where, 0f, 6f * big, 30f * big, new Color(255, 236, 200), 0.16f);
                    break;
                case "thrust":
                    Pop(EffectShape.Streak, where - dir * 6f, angle, 30f * big, 44f * big, new Color(255, 240, 210), 0.1f);
                    break;
                default:
                    Pop(EffectShape.Slash, where - dir * 2f, angle, 30f * big, 42f * big, new Color(255, 246, 228), 0.12f);
                    break;
            }
        }

        // Streaks out along the swing, fanned, white hot cooling to the colour.
        Color hot = element == Element.Physical ? new Color(255, 200, 120) : colour;
        int count = (critical ? 16 : 10) + (heavy ? 4 : 0);
        for (int i = 0; i < count; i++)
        {
            float a = angle + _particles.Range(-0.7f, 0.7f);
            _particles.Add(new EffectParticle
            {
                Position = where,
                Velocity = new Vector2(MathF.Cos(a), MathF.Sin(a)) * _particles.Range(70f, 190f) * big,
                Gravity = new Vector2(0f, 220f),
                Drag = 4f,
                Life = _particles.Range(0.16f, 0.34f),
                Size0 = 2.2f, Size1 = 1f,
                Colour0 = Color.White, Colour1 = hot, Strength = 1f,
                Shape = EffectShape.Streak, Stretch = 0.035f,
            });
        }
        if (critical)
            for (int i = 0; i < 8; i++)
            {
                float a = _particles.Range(0f, MathHelper.TwoPi);
                _particles.Add(new EffectParticle
                {
                    Position = where,
                    Velocity = new Vector2(MathF.Cos(a), MathF.Sin(a)) * _particles.Range(40f, 90f),
                    Gravity = new Vector2(0f, 60f), Drag = 3f,
                    Life = _particles.Range(0.35f, 0.6f),
                    Size0 = 4f, Size1 = 2f,
                    Colour0 = new Color(255, 236, 150), Colour1 = new Color(255, 180, 40), Strength = 1f,
                    Shape = EffectShape.Star, Spin = _particles.Range(-6f, 6f),
                });
            }
        if (element != Element.Physical) Burst(where, element, big * 0.8f, dir);
    }

    /// <summary>A school's burst at a point: what a spell looks like landing.</summary>
    /// <param name="where">The middle, in world pixels.</param>
    /// <param name="element">The school.</param>
    /// <param name="size">How big, 1 for a single target.</param>
    /// <param name="toward">Which way it was going, for the bursts that follow through; zero for none.</param>
    public void Burst(Vector2 where, Element element, float size = 1f, Vector2 toward = default)
    {
        if (_textures is null) return;
        Color c = ColourOf(element);
        float s = size;
        Pop(EffectShape.Dot, where, 0f, 26f * s, 40f * s, c, 0.18f);
        switch (element)
        {
            case Element.Fire:
                for (int i = 0; i < 18; i++) Spray(where, EffectShape.Dot, 30f, 110f * s, new Vector2(0f, -140f), 2.5f, 3f, 1f, 0.3f, 0.7f, new Color(255, 230, 140), new Color(220, 50, 10));
                for (int i = 0; i < 6; i++) Spray(where, EffectShape.Puff, 10f, 40f * s, new Vector2(0f, -90f), 1.5f, 12f * s, 22f * s, 0.25f, 0.45f, new Color(255, 170, 60), new Color(160, 30, 10));
                Pop(EffectShape.Star, where, 0f, 20f * s, 40f * s, new Color(255, 220, 140), 0.12f);
                break;
            case Element.Frost:
                for (int i = 0; i < 12; i++) Spray(where, EffectShape.Shard, 60f, 170f * s, new Vector2(0f, 200f), 2.5f, 7f, 4f, 0.35f, 0.6f, Color.White, c, spin: true);
                for (int i = 0; i < 10; i++) Spray(where, EffectShape.Dot, 5f, 30f, new Vector2(0f, 20f), 1f, 2.5f, 1.5f, 0.6f, 1.1f, Color.White, c);
                Pop(EffectShape.Ring, where, 0f, 10f * s, 44f * s, new Color(200, 240, 255), 0.22f);
                break;
            case Element.Lightning:
                for (int i = 0; i < 5; i++)
                {
                    float a = _particles.Range(0f, MathHelper.TwoPi);
                    ForkLine(where, where + new Vector2(MathF.Cos(a), MathF.Sin(a)) * _particles.Range(18f, 34f) * s, c, 0.12f, 1.6f);
                }
                for (int i = 0; i < 10; i++) Spray(where, EffectShape.Streak, 90f, 200f * s, Vector2.Zero, 5f, 2f, 1f, 0.12f, 0.22f, Color.White, c, stretch: 0.03f);
                Pop(EffectShape.Star, where, 0.4f, 24f * s, 50f * s, Color.White, 0.08f);
                break;
            case Element.Radiant:
                for (int i = 0; i < 10; i++)
                {
                    float a = i / 10f * MathHelper.TwoPi + _particles.Range(-0.1f, 0.1f);
                    Pop(EffectShape.Streak, where + new Vector2(MathF.Cos(a), MathF.Sin(a)) * 14f * s, a, 10f * s, 34f * s, new Color(255, 246, 200), 0.24f);
                }
                for (int i = 0; i < 14; i++) Spray(where - new Vector2(0f, 16f), EffectShape.Dot, 5f, 25f, new Vector2(0f, 40f), 1f, 3f, 1.5f, 0.6f, 1.0f, Color.White, c);
                Pop(EffectShape.Star, where, 0f, 30f * s, 56f * s, new Color(255, 250, 220), 0.14f);
                break;
            case Element.Necrotic:
                for (int i = 0; i < 10; i++) Spray(where, EffectShape.Puff, 15f, 45f * s, new Vector2(0f, 50f), 2f, 10f * s, 20f * s, 0.45f, 0.8f, new Color(60, 20, 70), new Color(10, 0, 20), matter: true, strength: 0.75f);
                for (int i = 0; i < 12; i++) Spray(where, EffectShape.Dot, 20f, 70f * s, new Vector2(0f, 30f), 2f, 3f, 1.5f, 0.4f, 0.8f, new Color(200, 255, 200), c);
                Pop(EffectShape.Ring, where, 0f, 34f * s, 8f * s, c, 0.25f);
                break;
            case Element.Force:
                Pop(EffectShape.Ring, where, 0f, 8f * s, 56f * s, new Color(210, 220, 255), 0.2f);
                Pop(EffectShape.Ring, where, 0f, 4f * s, 36f * s, c, 0.16f);
                for (int i = 0; i < 8; i++) Spray(where, EffectShape.Streak, 120f, 200f * s, Vector2.Zero, 6f, 2f, 1f, 0.12f, 0.2f, Color.White, c, stretch: 0.03f);
                break;
            case Element.Psychic:
                for (int i = 0; i < 3; i++) Pop(EffectShape.Ring, where, 0f, 6f * s, (30f + 14f * i) * s, c, 0.3f + 0.06f * i, delay: 0.06f * i);
                for (int i = 0; i < 10; i++) Spray(where, EffectShape.Dot, 20f, 60f * s, new Vector2(0f, -20f), 2f, 3f, 1f, 0.4f, 0.7f, Color.White, c);
                break;
            case Element.Nature:
                for (int i = 0; i < 12; i++) Spray(where, EffectShape.Leaf, 50f, 130f * s, new Vector2(0f, 120f), 3f, 7f, 6f, 0.5f, 0.9f, new Color(170, 240, 110), new Color(60, 140, 40), spin: true, matter: true);
                for (int i = 0; i < 10; i++) Spray(where, EffectShape.Dot, 10f, 40f, new Vector2(0f, -30f), 1f, 3f, 1.5f, 0.5f, 0.9f, Color.White, c);
                break;
            case Element.Thunder:
                Pop(EffectShape.Ring, where, 0f, 10f * s, 70f * s, new Color(220, 228, 255), 0.26f);
                for (int i = 0; i < 8; i++) Spray(where + new Vector2(0f, 14f), EffectShape.Puff, 40f, 90f * s, new Vector2(0f, -20f), 3f, 8f, 18f, 0.4f, 0.7f, new Color(170, 160, 150), new Color(120, 110, 100), matter: true, strength: 0.6f);
                break;
            case Element.Poison:
                for (int i = 0; i < 12; i++) Spray(where, EffectShape.Ring, 15f, 50f * s, new Vector2(0f, -50f), 1.5f, 3f, 6f, 0.5f, 0.9f, new Color(220, 255, 160), c);
                for (int i = 0; i < 10; i++) Spray(where, EffectShape.Dot, 60f, 140f * s, new Vector2(0f, 300f), 1.5f, 3f, 2f, 0.3f, 0.5f, c, new Color(80, 140, 20), matter: true);
                break;
            case Element.Blood:
                for (int i = 0; i < 16; i++) Spray(where, EffectShape.Dot, 60f, 170f * s, new Vector2(0f, 380f), 1.5f, 3f, 2f, 0.3f, 0.55f, new Color(200, 20, 30), new Color(90, 0, 10), matter: true);
                for (int i = 0; i < 6; i++) Spray(where, EffectShape.Streak, 90f, 160f * s, Vector2.Zero, 6f, 3f, 1f, 0.12f, 0.2f, new Color(255, 120, 120), c, stretch: 0.03f);
                break;
            default:
                for (int i = 0; i < 10; i++) Spray(where, EffectShape.Streak, 60f, 150f * s, new Vector2(0f, 200f), 4f, 2f, 1f, 0.15f, 0.3f, Color.White, c, stretch: 0.035f);
                break;
        }
    }

    /// <summary>Sends a school's bolt flying from one point to another, its ribbon behind it and its motes falling off it; it bursts where it lands (unless told not to).</summary>
    /// <param name="element">The school.</param>
    /// <param name="from">Where it leaves, in world pixels: the caster's hand (WalkerSprite.BonePosition("hand_near")).</param>
    /// <param name="to">Where it lands: the target's chest.</param>
    /// <param name="seconds">How long it takes; the blow's damage lands with it.</param>
    /// <param name="arc">How high it arcs at the middle, in world pixels: 0 for straight, more for a lob.</param>
    /// <param name="bursts">Whether it bursts in its school's way when it lands.</param>
    public void Projectile(Element element, Vector2 from, Vector2 to, float seconds, float arc = 0f, bool bursts = true)
    {
        if (element == Element.Lightning)
        {
            // Lightning doesn't fly: it forks across at once, and the burst comes as it lands.
            ForkLine(from, to, ColourOf(element), MathF.Max(0.12f, seconds), 2.4f);
            if (bursts) Burst(to, element);
            return;
        }
        _bolts.Add(new Bolt { Element = element, From = from, To = to, Seconds = MathF.Max(seconds, 0.02f), Arc = arc, Bursts = bursts });
    }

    /// <summary>A beam from one point to another for a time: a core of white in a band of the school's colour, flickering, with sparks off its end.</summary>
    public void BeamFrom(Element element, Vector2 from, Vector2 to, float seconds) =>
        _beams.Add(new Beam { Element = element, From = from, To = to, Seconds = MathF.Max(seconds, 0.05f) });

    /// <summary>A charge gathering in a hand through a wind-up: a glow that grows, and specks drawn in to it.</summary>
    /// <param name="where">Where the hand is, asked each frame (WalkerSprite.BonePosition("hand_near")).</param>
    /// <param name="element">The school.</param>
    /// <param name="seconds">How long it gathers before it goes.</param>
    public void ChargeIn(Func<Vector2> where, Element element, float seconds) =>
        _charges.Add(new Charge { Where = where, Element = element, Seconds = MathF.Max(seconds, 0.05f) });

    /// <summary>A glint: a four-pointed flare that pops and goes in a fifth of a second, on a weapon at the start of a telegraph.</summary>
    public void Glint(Vector2 where, Color? colour = null)
    {
        if (_textures is null) return;
        Pop(EffectShape.Star, where, 0f, 4f, 22f, colour ?? new Color(255, 250, 230), 0.2f, spinning: 4f);
        Pop(EffectShape.Dot, where, 0f, 6f, 12f, colour ?? new Color(255, 240, 200), 0.16f);
    }

    /// <summary>Dust kicked up where a body lands (a knockdown, a death), spreading low either side of it.</summary>
    /// <param name="feet">Where on the floor, in world pixels.</param>
    /// <param name="size">How big, 1 for a person.</param>
    public void Dust(Vector2 feet, float size = 1f)
    {
        if (_textures is null) return;
        for (int i = 0; i < 12; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            _particles.Add(new EffectParticle
            {
                Position = feet + new Vector2(_particles.Range(-8f, 8f) * size, _particles.Range(-2f, 1f)),
                Velocity = new Vector2(side * _particles.Range(20f, 70f) * size, _particles.Range(-22f, -6f)),
                Gravity = new Vector2(0f, 12f), Drag = 3.5f,
                Life = _particles.Range(0.4f, 0.75f),
                Size0 = 7f * size, Size1 = 16f * size,
                Colour0 = new Color(196, 182, 160), Colour1 = new Color(150, 138, 120), Strength = 0.7f,
                Shape = EffectShape.Puff, Rotation = _particles.Range(0f, 6f), Matter = true,
            });
        }
    }

    /// <summary>A body fading away in motes: many small lights rising and swaying off the area it covered.</summary>
    /// <param name="centre">The middle of the body, in world pixels.</param>
    /// <param name="size">How big the body is, in world pixels.</param>
    /// <param name="colour">Their colour: a pale grey-violet for a foe.</param>
    public void DeathMotes(Vector2 centre, Vector2 size, Color colour)
    {
        if (_textures is null) return;
        for (int i = 0; i < 34; i++)
        {
            _particles.Add(new EffectParticle
            {
                Position = centre + new Vector2(_particles.Range(-0.45f, 0.45f) * size.X, _particles.Range(-0.45f, 0.45f) * size.Y),
                Velocity = new Vector2(_particles.Range(-10f, 10f), _particles.Range(-40f, -12f)),
                Gravity = new Vector2(0f, -14f), Drag = 0.6f,
                Life = _particles.Range(0.7f, 1.4f),
                Size0 = _particles.Range(2f, 3.5f), Size1 = 1f,
                Colour0 = Color.White, Colour1 = colour, Strength = 0.9f,
                Shape = EffectShape.Dot, Delay = _particles.Range(0f, 0.25f),
            });
        }
    }

    /// <summary>Healing rising off someone: green motes and a soft swell of light.</summary>
    public void HealGlow(Vector2 centre, float size = 1f)
    {
        if (_textures is null) return;
        var green = new Color(110, 240, 130);
        Pop(EffectShape.Dot, centre, 0f, 20f * size, 36f * size, green, 0.3f);
        for (int i = 0; i < 16; i++)
            _particles.Add(new EffectParticle
            {
                Position = centre + new Vector2(_particles.Range(-10f, 10f) * size, _particles.Range(-6f, 14f) * size),
                Velocity = new Vector2(_particles.Range(-6f, 6f), _particles.Range(-36f, -14f)),
                Gravity = new Vector2(0f, -10f), Drag = 0.5f,
                Life = _particles.Range(0.6f, 1.0f),
                Size0 = 3f, Size1 = 1.5f,
                Colour0 = Color.White, Colour1 = green, Strength = 0.9f,
                Shape = i % 4 == 0 ? EffectShape.Star : EffectShape.Dot, Delay = _particles.Range(0f, 0.15f),
            });
    }

    /// <summary>Puts a ground marker down: a ring round a point, a band toward another, or a wedge, that fills from its middle over its wind-up. It resolves (flashes) when it's full, or when Resolve says.</summary>
    /// <param name="shape">Its shape.</param>
    /// <param name="at">Its middle (a ring) or where it starts (a line, a cone), on the floor, in world pixels.</param>
    /// <param name="toward">For a line or a cone, a point it goes toward; ignored for a ring.</param>
    /// <param name="size">A ring's radius, a line's length, a cone's reach, in world pixels.</param>
    /// <param name="seconds">How long it takes to fill.</param>
    /// <param name="width">A line's width, in world pixels.</param>
    /// <param name="halfAngle">A cone's half angle, in radians.</param>
    /// <param name="colour">Its colour: the warning orange-red unless it's given one.</param>
    /// <returns>Its id, for Resolve or Cancel.</returns>
    public int Mark(MarkerShape shape, Vector2 at, Vector2 toward, float size, float seconds, float width = 24f, float halfAngle = MathHelper.Pi / 3f, Color? colour = null)
    {
        int id = _nextMarker++;
        _markers.Add(new Marker
        {
            Id = id, Shape = shape, At = at, Toward = toward, Size = size, Width = width, HalfAngle = halfAngle,
            Colour = colour ?? Warning, Seconds = MathF.Max(0.05f, seconds),
        });
        return id;
    }

    /// <summary>Resolves a marker now (the blow's landed): it flashes and fades.</summary>
    public void Resolve(int id)
    {
        foreach (Marker marker in _markers)
            if (marker.Id == id && marker.Resolved < 0f) marker.Resolved = 0f;
    }

    /// <summary>Takes a marker away without it resolving (the wind-up was interrupted).</summary>
    public void Cancel(int id) => _markers.RemoveAll(marker => marker.Id == id);

    /// <summary>Moves the particles and what's flying on, drops the trails, and lets the shake settle.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        foreach (SparkParticleSystem system in _sparks.Values) system.Update(gameTime);
        foreach (MoteParticleSystem system in _motes.Values) system.Update(gameTime);
        Update((float)gameTime.ElapsedGameTime.TotalSeconds, oldSystems: false);
    }

    /// <summary>Moves everything on by a number of seconds (the proof run's fixed step).</summary>
    /// <param name="seconds">Seconds since the last update.</param>
    public void Update(float seconds) => Update(seconds, oldSystems: true);

    private void Update(float seconds, bool oldSystems)
    {
        if (oldSystems)
        {
            var time = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(seconds));
            foreach (SparkParticleSystem system in _sparks.Values) system.Update(time);
            foreach (MoteParticleSystem system in _motes.Values) system.Update(time);
        }
        _shake.Update(seconds);
        _particles.Update(seconds);

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

        for (int i = _bolts.Count - 1; i >= 0; i--)
        {
            Bolt bolt = _bolts[i];
            bolt.Clock += seconds;
            Vector2 head = Where(bolt);
            bolt.Trail.Add(head);
            if (bolt.Trail.Count > BoltTrail) bolt.Trail.RemoveAt(0);
            bolt.Emit += seconds;
            while (bolt.Emit >= 1f / 60f)
            {
                bolt.Emit -= 1f / 60f;
                Shed(head, bolt.Element);
            }
            if (bolt.Clock < bolt.Seconds) continue;
            if (bolt.Bursts) Burst(bolt.To, bolt.Element, 1f, bolt.To - bolt.From);
            _bolts.RemoveAt(i);
        }

        for (int i = _beams.Count - 1; i >= 0; i--)
        {
            Beam beam = _beams[i];
            beam.Clock += seconds;
            beam.Emit += seconds;
            while (beam.Emit >= 1f / 30f)
            {
                beam.Emit -= 1f / 30f;
                Spray(beam.To, EffectShape.Streak, 40f, 110f, new Vector2(0f, 120f), 4f, 2f, 1f, 0.15f, 0.25f, Color.White, ColourOf(beam.Element), stretch: 0.03f);
            }
            if (beam.Clock >= beam.Seconds) _beams.RemoveAt(i);
        }

        for (int i = _forks.Count - 1; i >= 0; i--)
        {
            _forks[i].Clock += seconds;
            if (_forks[i].Clock >= _forks[i].Life) _forks.RemoveAt(i);
        }

        for (int i = _charges.Count - 1; i >= 0; i--)
        {
            Charge charge = _charges[i];
            charge.Clock += seconds;
            charge.Emit += seconds;
            Vector2 at = charge.Where();
            while (charge.Emit >= 1f / 40f)
            {
                charge.Emit -= 1f / 40f;
                float a = _particles.Range(0f, MathHelper.TwoPi), r = _particles.Range(12f, 20f);
                var off = new Vector2(MathF.Cos(a), MathF.Sin(a)) * r;
                _particles.Add(new EffectParticle
                {
                    Position = at + off, Velocity = -off / 0.2f, Life = 0.2f, Size0 = 2.5f, Size1 = 1f,
                    Colour0 = ColourOf(charge.Element), Colour1 = Color.White, Strength = 0.9f, Shape = EffectShape.Dot,
                });
            }
            if (charge.Clock >= charge.Seconds) _charges.RemoveAt(i);
        }

        for (int i = _markers.Count - 1; i >= 0; i--)
        {
            Marker marker = _markers[i];
            marker.Clock += seconds;
            if (marker.Resolved < 0f && marker.Clock >= marker.Seconds) marker.Resolved = 0f;
            else if (marker.Resolved >= 0f) marker.Resolved += seconds;
            if (marker.Resolved >= MarkerFlash + MarkerFade) _markers.RemoveAt(i);
        }
    }

    /// <summary>Draws the ground markers, laid on the floor. Outside any batch, after the room's floor and before the people.</summary>
    /// <param name="world">The camera's matrix, the shake included.</param>
    public void DrawGround(Matrix world)
    {
        if (_ribbons is null || _markers.Count == 0) return;
        _ribbons.Begin(world, BlendState.AlphaBlend);
        foreach (Marker marker in _markers) DrawMarker(marker);
        _ribbons.End();
    }

    /// <summary>Draws the sparks, the bursts, the bolts, the beams and the motes: the matter laid over, then the light added. Outside any batch, after the people.</summary>
    /// <param name="spriteBatch">The SpriteBatch to draw with, between batches.</param>
    /// <param name="world">The camera's matrix, the shake included.</param>
    public void DrawParticles(SpriteBatch spriteBatch, Matrix world)
    {
        foreach (MoteParticleSystem system in _motes.Values) system.Draw(spriteBatch, world);
        foreach (SparkParticleSystem system in _sparks.Values) system.Draw(spriteBatch, world);
        if (_textures is null) return;

        _particles.Draw(spriteBatch, _textures, world, matter: true);
        if (_bolts.Count + _beams.Count + _forks.Count > 0)
        {
            _ribbons.Begin(world);
            foreach (Bolt bolt in _bolts) DrawBoltTrail(bolt);
            foreach (Beam beam in _beams) DrawBeam(beam);
            foreach (Fork fork in _forks) DrawFork(fork);
            _ribbons.End();
        }
        _particles.Draw(spriteBatch, _textures, world, matter: false);

        if (_bolts.Count + _charges.Count + _beams.Count == 0) return;
        spriteBatch.Begin(SpriteSortMode.Deferred, Blends.PremultipliedAdditive, SamplerState.LinearClamp, transformMatrix: world);
        foreach (Bolt bolt in _bolts) DrawBoltHead(spriteBatch, bolt);
        foreach (Charge charge in _charges)
        {
            float grow = MathHelper.Clamp(charge.Clock / charge.Seconds, 0f, 1f);
            Glow(spriteBatch, EffectShape.Dot, charge.Where(), 6f + 14f * grow, ColourOf(charge.Element) * (0.5f + 0.5f * grow), 0f);
            Glow(spriteBatch, EffectShape.Star, charge.Where(), 4f + 10f * grow, Color.White * (0.4f + 0.4f * grow), charge.Clock * 3f);
        }
        foreach (Beam beam in _beams)
        {
            Glow(spriteBatch, EffectShape.Dot, beam.From, 16f, ColourOf(beam.Element), 0f);
            Glow(spriteBatch, EffectShape.Star, beam.To, 22f + 6f * MathF.Sin(beam.Clock * 50f), Color.White, beam.Clock * 2f);
        }
        spriteBatch.End();
    }

    /// <summary>Draws whatever from hud.png is flying, turned the way it's going.</summary>
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
    public void Clear()
    {
        _flying.Clear();
        _bolts.Clear();
        _beams.Clear();
        _forks.Clear();
        _charges.Clear();
        _markers.Clear();
        _particles.Clear();
    }

    /// <summary>Where something from hud.png is now: along the straight line, and up its arc if it's lobbed.</summary>
    private static Vector2 Where(Flight flight)
    {
        float t = MathHelper.Clamp(flight.Clock / flight.Seconds, 0f, 1f);
        return Vector2.Lerp(flight.From, flight.To, t) - new Vector2(0f, flight.Arc * 4f * t * (1f - t));
    }

    /// <summary>Where a bolt is now.</summary>
    private static Vector2 Where(Bolt bolt)
    {
        float t = MathHelper.Clamp(bolt.Clock / bolt.Seconds, 0f, 1f);
        return Vector2.Lerp(bolt.From, bolt.To, t) - new Vector2(0f, bolt.Arc * 4f * t * (1f - t));
    }

    /// <summary>A sprite that grows and fades where it is: an impact's flare, a ring going out, a ray.</summary>
    private void Pop(EffectShape shape, Vector2 where, float angle, float from, float to, Color colour, float life, float spinning = 0f, float delay = 0f) =>
        _particles.Add(new EffectParticle
        {
            Position = where, Life = life, Size0 = from, Size1 = to, Colour0 = colour, Colour1 = colour, Strength = 1f,
            Shape = shape, Rotation = angle, Spin = spinning, Delay = delay,
        });

    /// <summary>One particle thrown out of a point in any direction at a speed between two, pulled by gravity, with drag, growing or shrinking, cooling from one colour to another.</summary>
    private void Spray(Vector2 where, EffectShape shape, float least, float most, Vector2 gravity, float drag, float size0, float size1, float lifeLeast, float lifeMost,
        Color colour0, Color colour1, bool spin = false, bool matter = false, float strength = 1f, float stretch = 0f)
    {
        float a = _particles.Range(0f, MathHelper.TwoPi);
        _particles.Add(new EffectParticle
        {
            Position = where,
            Velocity = new Vector2(MathF.Cos(a), MathF.Sin(a)) * _particles.Range(least, most),
            Gravity = gravity, Drag = drag,
            Life = _particles.Range(lifeLeast, lifeMost),
            Size0 = size0, Size1 = size1,
            Colour0 = colour0, Colour1 = colour1, Strength = strength,
            Shape = shape, Rotation = _particles.Range(0f, MathHelper.TwoPi), Spin = spin ? _particles.Range(-10f, 10f) : 0f,
            Matter = matter, Stretch = stretch,
        });
    }

    /// <summary>What a bolt sheds as it flies: embers, frost, wisps, leaves, drops.</summary>
    private void Shed(Vector2 at, Element element)
    {
        Color c = ColourOf(element);
        switch (element)
        {
            case Element.Fire:
                Spray(at, EffectShape.Dot, 5f, 25f, new Vector2(0f, -80f), 2f, 3f, 1f, 0.2f, 0.4f, new Color(255, 230, 150), new Color(220, 50, 10));
                break;
            case Element.Necrotic:
                Spray(at, EffectShape.Puff, 2f, 10f, new Vector2(0f, 20f), 2f, 6f, 10f, 0.25f, 0.4f, new Color(50, 20, 60), Color.Black, matter: true, strength: 0.6f);
                break;
            case Element.Nature:
                if (_random.Next(3) == 0) Spray(at, EffectShape.Leaf, 5f, 20f, new Vector2(0f, 60f), 2f, 5f, 4f, 0.3f, 0.5f, new Color(170, 240, 110), new Color(60, 140, 40), spin: true, matter: true);
                break;
            case Element.Blood:
            case Element.Poison:
                Spray(at, EffectShape.Dot, 5f, 15f, new Vector2(0f, 200f), 1f, 2.5f, 1.5f, 0.2f, 0.35f, c, c * 0.6f, matter: true);
                break;
            default:
                Spray(at, EffectShape.Dot, 3f, 18f, Vector2.Zero, 2f, 2.5f, 1f, 0.15f, 0.3f, Color.White, c);
                break;
        }
    }

    /// <summary>A fork of lightning between two points: a jagged line, flickering, with a branch or two.</summary>
    private void ForkLine(Vector2 from, Vector2 to, Color colour, float life, float width)
    {
        const int Segments = 9;
        var points = new Vector2[Segments + 1];
        Vector2 along = to - from;
        Vector2 across = along.LengthSquared() > 0f ? Vector2.Normalize(new Vector2(-along.Y, along.X)) : Vector2.UnitY;
        float jag = MathF.Min(10f, along.Length() * 0.12f);
        for (int i = 0; i <= Segments; i++)
        {
            float t = i / (float)Segments;
            float off = i == 0 || i == Segments ? 0f : _particles.Range(-jag, jag);
            points[i] = from + along * t + across * off;
        }
        _forks.Add(new Fork { Points = points, Colour = colour, Life = life, Width = width });
    }

    private void DrawBoltTrail(Bolt bolt)
    {
        Color c = ColourOf(bolt.Element);
        List<Vector2> trail = bolt.Trail;
        for (int i = 1; i < trail.Count; i++)
        {
            float t0 = (i - 1) / (float)trail.Count, t1 = i / (float)trail.Count;
            Vector2 d = trail[i] - trail[i - 1];
            if (d.LengthSquared() < 1e-4f) continue;
            Vector2 n = Vector2.Normalize(new Vector2(-d.Y, d.X));
            float w0 = BoltWidth * t0 / 2f, w1 = BoltWidth * t1 / 2f;
            _ribbons.Quad(trail[i - 1] + n * w0, trail[i] + n * w1, trail[i] - n * w1, trail[i - 1] - n * w0, c * (t0 * 0.9f), c * (t1 * 0.9f), c * (t1 * 0.9f), c * (t0 * 0.9f));
        }
    }

    private void DrawBoltHead(SpriteBatch spriteBatch, Bolt bolt)
    {
        Vector2 at = Where(bolt);
        Color c = ColourOf(bolt.Element);
        Glow(spriteBatch, EffectShape.Dot, at, 16f, c, 0f);
        Glow(spriteBatch, bolt.Element == Element.Frost ? EffectShape.Shard : EffectShape.Star, at, 10f, Color.White,
            bolt.Element == Element.Frost ? MathF.Atan2(bolt.To.Y - bolt.From.Y, bolt.To.X - bolt.From.X) : bolt.Clock * 8f);
    }

    private void DrawBeam(Beam beam)
    {
        Color c = ColourOf(beam.Element);
        float u = beam.Clock / beam.Seconds;
        float swell = MathF.Min(1f, MathF.Min(u * 8f, (1f - u) * 6f));
        float flicker = 0.85f + 0.15f * MathF.Sin(beam.Clock * 60f);
        _ribbons.Band(beam.From, beam.To, 14f * swell * flicker, c * 0.8f, c * 0.8f);
        _ribbons.Band(beam.From, beam.To, 5f * swell, Color.White, Color.White);
    }

    private void DrawFork(Fork fork)
    {
        float u = fork.Clock / fork.Life;
        // On, off and on again: lightning flickers.
        float on = u < 0.3f || u > 0.45f && u < 0.75f ? 1f - u * 0.6f : 0.15f;
        for (int i = 1; i < fork.Points.Length; i++)
        {
            _ribbons.Band(fork.Points[i - 1], fork.Points[i], fork.Width * 3f, fork.Colour * (0.6f * on), fork.Colour * (0.6f * on));
            _ribbons.Band(fork.Points[i - 1], fork.Points[i], fork.Width, Color.White * on, Color.White * on);
        }
    }

    private void DrawMarker(Marker marker)
    {
        float fill = MathHelper.Clamp(marker.Clock / marker.Seconds, 0f, 1f);
        float flash = 0f, fade = 1f;
        if (marker.Resolved >= 0f)
        {
            flash = marker.Resolved < MarkerFlash ? 1f : 0f;
            fade = marker.Resolved < MarkerFlash ? 1f : 1f - (marker.Resolved - MarkerFlash) / MarkerFade;
            fill = 1f;
        }
        // Laid over the floor rather than added to it, so it reads on grass and sand alike: a faint
        // wash of its colour, the part filled so far stronger, and a bright rim; white as it resolves.
        Color edge = Color.Lerp(marker.Colour, Color.White, 0.25f + 0.75f * flash) * (0.9f * fade);
        Color inside = Color.Lerp(marker.Colour, Color.White, flash) * ((0.16f + 0.2f * flash) * fade);
        Color filled = Color.Lerp(marker.Colour, Color.White, flash) * ((0.34f + 0.16f * flash) * fade);
        var depth = new Vector2(1f, FloorDepth);
        // The rim pulses while it's filling, faster as it nears full.
        float pulse = marker.Resolved >= 0f ? 1f : 0.75f + 0.25f * MathF.Sin(marker.Clock * (8f + 16f * fill));
        edge *= pulse;

        switch (marker.Shape)
        {
            case MarkerShape.Ring:
            {
                Vector2 r = new Vector2(marker.Size) * depth;
                _ribbons.Disc(marker.At, r, inside * 0.5f, inside, 0f, 40);
                _ribbons.Disc(marker.At, r * fill, filled, filled * 0.7f, 0f, 40);
                _ribbons.SoftRing(marker.At, r, edge);
                break;
            }
            case MarkerShape.Line:
            {
                Vector2 dir = marker.Toward - marker.At;
                dir = dir.LengthSquared() > 1e-4f ? Vector2.Normalize(dir) : Vector2.UnitX;
                Vector2 end = marker.At + dir * marker.Size;
                Vector2 side = new Vector2(-dir.Y, dir.X) * depth * (marker.Width / 2f);
                _ribbons.Quad(marker.At + side, end + side, end - side, marker.At - side, inside, inside, inside, inside);
                Vector2 reach = marker.At + dir * marker.Size * fill;
                _ribbons.Quad(marker.At + side, reach + side, reach - side, marker.At - side, filled, filled, filled, filled);
                Vector2 rim = Vector2.Normalize(side) * 1.5f;
                _ribbons.Quad(marker.At + side, end + side, end + side - rim, marker.At + side - rim, edge, edge, edge, edge);
                _ribbons.Quad(marker.At - side + rim, end - side + rim, end - side, marker.At - side, edge, edge, edge, edge);
                _ribbons.Quad(end + side, end - side, end - side - dir * 1.5f, end + side - dir * 1.5f, edge, edge, edge, edge);
                break;
            }
            case MarkerShape.Cone:
            {
                Vector2 dir = marker.Toward - marker.At;
                float angle = dir.LengthSquared() > 1e-4f ? MathF.Atan2(dir.Y, dir.X) : 0f;
                Vector2 r = new Vector2(marker.Size) * depth;
                float from = angle - marker.HalfAngle, sweep = marker.HalfAngle * 2f;
                _ribbons.Disc(marker.At, r, inside * 0.5f, inside, 0f, 24, from, sweep);
                _ribbons.Disc(marker.At, r * fill, filled, filled * 0.7f, 0f, 24, from, sweep);
                _ribbons.Disc(marker.At, r, edge, edge, 0.94f, 24, from, sweep);
                foreach (float side in new[] { from, from + sweep })
                {
                    Vector2 tip = marker.At + new Vector2(MathF.Cos(side), MathF.Sin(side)) * r;
                    _ribbons.Band(marker.At, tip, 3f, edge, edge);
                }
                break;
            }
        }
    }

    private void Glow(SpriteBatch spriteBatch, EffectShape shape, Vector2 at, float size, Color colour, float rotation)
    {
        Texture2D texture = _textures[shape];
        spriteBatch.Draw(texture, at, null, colour, rotation, new Vector2(texture.Width, texture.Height) / 2f, size / texture.Width, SpriteEffects.None, 0f);
    }
}
