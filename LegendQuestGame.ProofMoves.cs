using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.Characters.Motion;
using LegendQuest.Components;
using LegendQuest.Particles;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest;

/// <summary>
/// The proof run's Moves part (--proof dir Moves): the fighting moves and their impacts filmed frame
/// by frame, so the motion can be looked at rather than read about.
/// </summary>
/// <remarks>
/// <para>
/// It starts straight away (it doesn't play through the menus first), puts a room up, and plays one
/// take after another: the party's looks and a rigged foe, and the three creatures drawn from sheets,
/// swinging, casting, flinching, blocking, dodging, falling and dying, with the hit-stops, flashes,
/// trails, sparks, bursts, numbers, markers and shakes the fight will use. Every take runs on a fixed
/// sixtieth of a second for each drawn frame, whatever the real clock does (saving a picture stalls
/// it), so the frames are evenly spaced; every second frame is saved as moves/(take)-(NNN).png, with
/// the take's name and the frame in the corner.
/// </para>
/// <para>
/// It also writes moves/samples.tsv, every track of a few clips sampled at every frame, so the C#
/// easing can be checked against tools/rig's to the fourth decimal place, and the effects' made
/// shapes as moves/shape-(name).png.
/// </para>
/// </remarks>
public partial class LegendQuestGame
{
    /// <summary>The screen the Moves part films.</summary>
    private MovesProof _movesProof;

    /// <summary>One frame of the Moves part: puts its screen up the first time, then steps it and names the frame to save, until its takes are done.</summary>
    private string ProofMoves()
    {
        if (_movesProof is null)
        {
            foreach (GameScreen screen in _screenManager.GetScreens()) _screenManager.RemoveScreen(screen);
            _movesProof = new MovesProof(Path.Combine(ProofDirectory, "moves"));
            _screenManager.AddScreen(_movesProof);
            return null;
        }
        string shot = _movesProof.Step();
        if (_movesProof.Done) _proofStage = ProofStage.Done;
        return shot is null ? null : Path.Combine("moves", shot);
    }

    /// <summary>
    /// The room the moves are filmed in, everyone in it, and the takes: each a few seconds of moves
    /// and effects on a schedule, stepped a sixtieth of a second a frame.
    /// </summary>
    private sealed class MovesProof : GameScreen
    {
        /// <summary>The fixed step, a frame's worth.</summary>
        private const float Step60 = 1f / 60f;

        /// <summary>Which frames are saved: every this many.</summary>
        private const int ShotEvery = 2;

        /// <summary>The room, and the spot in it the fighters stand round.</summary>
        private const string Zone = "road";

        /// <summary>How far apart two fighters stand for a melee blow, and for a ranged one, in world pixels.</summary>
        private const float Melee = 30f, Ranged = 120f;

        private readonly string _folder;
        private readonly Camera2D _camera = new(new Point(ScreenWidth, ScreenHeight));
        private readonly CombatEffects _effects = new();
        private readonly DamagePopups _popups = new();
        private readonly List<WalkerSprite> _cast = new();
        private readonly List<Take> _takes = new();
        private ContentManager _content;
        private RoomScene _scene;
        private Color _tint = Color.White;
        private SpriteFont _font;
        private Vector2 _middle;
        private int _take = -1, _frame;
        private float _clock;

        /// <summary>One take: its name, how many frames it runs, what to do on which frame, and who's in it.</summary>
        private sealed class Take
        {
            public string Name;
            public int Frames;
            public Action Setup;
            public readonly Dictionary<int, List<Action>> At = new();

            /// <summary>Does something at a time into the take.</summary>
            public Take On(float seconds, Action action)
            {
                int frame = (int)MathF.Round(seconds * 60f);
                if (!At.TryGetValue(frame, out List<Action> list)) At[frame] = list = new List<Action>();
                list.Add(action);
                return this;
            }
        }

        public MovesProof(string folder) => _folder = folder;

        /// <summary>Whether every take's been filmed.</summary>
        public bool Done { get; private set; }

        public override void Activate()
        {
            _content = new ContentManager(ScreenManager.Game.Services, "Content");
            _effects.LoadContent(_content);
            _popups.LoadContent(_content);
            _font = _content.Load<SpriteFont>("spectral-ui");

            RoomDefinition room = ZoneCatalog.Get(Zone).Rooms[0];
            _scene = new RoomScene(room);
            _scene.LoadContent(ScreenManager.Game.Services, ScreenManager.GraphicsDevice);
            _tint = room.Tint;
            _camera.LockTo(_scene.Bounds, (int)ZoneCatalog.RoomScale);
            float floor = room.Spawns.Count > 0 ? room.Spawns.Values.First().Feet.Y : ZoneCatalog.RoomHeight * 0.8f;
            float x = MathHelper.Clamp(room.Width / 2f, 220f, room.Width - 220f);
            _middle = new Vector2(x, floor);
            _camera.SnapTo(new Vector2(x, ZoneCatalog.RoomHeight / 2f));

            Directory.CreateDirectory(_folder);
            WriteSamples();
            WriteShapes();
            Plan();
        }

        public override void Unload()
        {
            _scene?.Unload();
            _content?.Unload();
        }

        /// <summary>Steps the take on a sixtieth of a second and says which frame to save, if this one.</summary>
        public string Step()
        {
            if (Done) return null;
            if (_take < 0 || _frame >= _takes[_take].Frames)
            {
                _take++;
                if (_take >= _takes.Count)
                {
                    Done = true;
                    return null;
                }
                Begin(_takes[_take]);
            }
            Take take = _takes[_take];
            if (take.At.TryGetValue(_frame, out List<Action> actions))
                foreach (Action action in actions) action();

            _clock += Step60;
            var time = new GameTime(TimeSpan.FromSeconds(_clock), TimeSpan.FromSeconds(Step60));
            foreach (WalkerSprite walker in _cast) walker.Update(Step60);
            _effects.Update(Step60);
            _popups.Update(Step60);
            _scene.Update(time);
            _frame++;
            return _frame % ShotEvery == 0 ? $"{take.Name}-{_frame / ShotEvery:000}.png" : null;
        }

        private void Begin(Take take)
        {
            _cast.Clear();
            _effects.Clear();
            _effects.Shaker.Clear();
            _popups.Clear();
            _frame = 0;
            take.Setup?.Invoke();
        }

        public override void Draw(GameTime gameTime)
        {
            if (_scene is null) return;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Matrix shake = _effects.Shake;
            Matrix world = _camera.Transform * shake;
            var time = new GameTime(TimeSpan.FromSeconds(_clock), TimeSpan.FromSeconds(Step60));

            _scene.DrawBack(time, spriteBatch, _camera, _tint, shake);
            _effects.DrawGround(world);
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
            foreach (WalkerSprite walker in _cast) walker.Draw(spriteBatch, MathHelper.Clamp(0.9f - walker.Position.Y / 4000f, 0.05f, 0.95f), _camera.Pixel, _tint);
            spriteBatch.End();
            _scene.DrawFront(spriteBatch, _camera, _tint * 0.3f, shake);
            _scene.DrawDark(spriteBatch, _camera, shake);

            if (_effects.Ribbons is RibbonBatch ribbons)
            {
                ribbons.Begin(world);
                foreach (WalkerSprite walker in _cast) walker.DrawTrail(ribbons);
                ribbons.End();
            }
            _effects.DrawParticles(spriteBatch, world);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: world);
            _effects.DrawFlying(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            _popups.Draw(spriteBatch, _camera.ToScreen, 1f);
            if (_take >= 0 && _take < _takes.Count)
            {
                string caption = $"{_takes[_take].Name}   frame {_frame}   {_frame / 60f:0.000} s";
                spriteBatch.DrawString(_font, caption, new Vector2(22f, 18f), Color.Black * 0.7f);
                spriteBatch.DrawString(_font, caption, new Vector2(20f, 16f), Color.White);
            }
            spriteBatch.End();
        }

        // ------------------------------------------------------------------ the cast

        /// <summary>A party member's figure, in what their origin wears with another weapon in the hand if asked (and the off hand empty unless that's asked for too).</summary>
        private WalkerSprite Member(string origin, string weapon = null, string kind = null, string offhand = null)
        {
            PartyMember member = Origins.Create(origin);
            if (weapon is not null)
            {
                member.Unequip(EquipmentSlot.OffHand);
                member.Equip(weapon, EquipmentSlot.MainHand);
            }
            if (offhand is not null) member.Equip(offhand, EquipmentSlot.OffHand);
            return Enter(new WalkerSprite { Art = ImportedArt.For(member), WeaponKind = kind });
        }

        /// <summary>A rigged foe's figure, from its parts.</summary>
        private WalkerSprite Foe(string id, string kind = null) => Enter(new WalkerSprite { Art = ImportedArt.Get(id), WeaponKind = kind });

        /// <summary>A creature drawn from its sheet.</summary>
        private WalkerSprite Beast(string id)
        {
            Creature creature = Creatures.Get(id);
            return Enter(new WalkerSprite { Shape = creature.Shape, Sheet = _content.Load<Texture2D>(creature.Texture) });
        }

        private WalkerSprite Enter(WalkerSprite walker)
        {
            walker.LoadContent(_content);
            walker.InFight = true;
            _cast.Add(walker);
            return walker;
        }

        /// <summary>Two fighters facing each other across the middle, a distance apart.</summary>
        private void Face(WalkerSprite attacker, WalkerSprite target, float apart, float lift = 0f)
        {
            attacker.Position = _middle + new Vector2(-apart / 2f, lift);
            target.Position = _middle + new Vector2(apart / 2f, lift);
            attacker.Facing = Direction.Right;
            target.Facing = Direction.Left;
        }

        // ------------------------------------------------------------------ what happens

        /// <summary>A blow landing from one on another, staged the way the fight will: the hit-stop on both, the flash, squash and knock, sparks along the swing and the arc of the blow, the number, a shake for the big ones, and the flinch (or the death).</summary>
        private void Land(WalkerSprite by, WalkerSprite on, int damage, int most, bool critical = false, bool kill = false, bool heavy = false,
            Element element = Element.Physical, string blow = "slash")
        {
            Color? tint = element == Element.Physical ? null : CombatEffects.ColourOf(element);
            on.TakeHit(by, damage, most, critical, kill, heavy, tint);
            Vector2 chest = on.BonePosition("torso");
            Vector2 toward = by is null ? Vector2.UnitX : new Vector2(MathF.Sign(on.Position.X - by.Position.X), 0f);
            Vector2 tip = by?.WeaponTip ?? chest;
            float near = on.Position.X - toward.X * on.Size.X * 0.22f;
            var contact = new Vector2(near, MathHelper.Clamp(tip.Y, on.Position.Y - on.Size.Y * 0.85f, on.Position.Y - on.Size.Y * 0.25f));
            Vector2 swing = by?.SwingDirection ?? Vector2.Zero;
            if (swing.LengthSquared() < 1e-3f) swing = toward;
            _effects.Impact(element == Element.Physical ? contact : chest, swing, element, critical, heavy, blow);
            _popups.Damage("target", on.BonePosition("head") - new Vector2(0f, 10f), damage, critical, taken: false);
            ShakeCause cause = kill ? ShakeCause.Kill : heavy ? ShakeCause.Heavy : element != Element.Physical ? ShakeCause.Spell : ShakeCause.Light;
            _effects.Shaker.Kick(cause, critical, _camera.ToScreen(chest), heavy || critical || kill ? toward : Vector2.Zero);
            if (kill) on.PlayMove("die", hold: true);
            else on.PlayMove(critical || heavy ? "hurt-big" : "hurt");
        }

        /// <summary>Wires a figure's clip events: a blow on an action's "hit" (a flinch's own "hit" is the frame it's struck, not a blow), a shot on "release", dust on "land", motes on a creature's fade.</summary>
        private void Wire(WalkerSprite walker, Action onHit = null, Action onRelease = null)
        {
            walker.ClipEvent += ev =>
            {
                switch (ev.Name)
                {
                    case ClipEvent.Hit when walker.Move is { IsAction: true }:
                        onHit?.Invoke();
                        break;
                    case ClipEvent.Release:
                        onRelease?.Invoke();
                        break;
                    case ClipEvent.Land:
                        _effects.Dust(walker.Position, walker.Size.X / 30f);
                        _effects.Shaker.Kick(ShakeCause.Land, false, _camera.ToScreen(walker.Position));
                        break;
                    case ClipEvent.Fx when ev.Arg == "death-motes":
                        _effects.DeathMotes(walker.Position - new Vector2(0f, walker.Size.Y * 0.3f), walker.Size * new Vector2(1f, 0.6f), new Color(190, 170, 220));
                        break;
                }
            };
        }

        /// <summary>A figure from parts fading out where it lies, in motes, the way a dead foe goes.</summary>
        private void FadeOut(Take take, WalkerSprite walker, float from, float seconds)
        {
            take.On(from, () => _effects.DeathMotes(walker.Position - new Vector2(0f, 6f), new Vector2(walker.Size.Y, 12f), new Color(190, 170, 220)));
            for (int i = 0; i <= (int)(seconds * 60f); i++)
            {
                float share = i / (seconds * 60f);
                take.On(from + i / 60f, () => walker.Opacity = 1f - share);
            }
        }

        // ------------------------------------------------------------------ the takes

        private Take NewTake(string name, float seconds)
        {
            var take = new Take { Name = name, Frames = (int)MathF.Round(seconds * 60f) };
            _takes.Add(take);
            return take;
        }

        /// <summary>Every take, in order.</summary>
        private void Plan()
        {
            // Each weapon kind's three auto attacks, one after the next, on a foe that flinches. The
            // companions are drawn as their own pictures whatever they hold, so the weapons are shown
            // on the Wanderer, whose look is put together from what's in his hands.
            const string you = Origins.You;
            var kinds = new (string Kind, string Who, string Weapon, string Off, string Blow)[]
            {
                ("one-hand", you, "longsword", "wooden-shield", "slash"),
                ("two-hand", you, "greatsword", null, "slash"),
                ("polearm", you, "spear", null, "thrust"),
                ("dagger", you, "dagger", "dagger", "thrust"),
                ("unarmed", you, "none", null, "blunt"),
                ("staff", you, "quarterstaff", null, "blunt"),
                ("hook", you, "handaxe", "handaxe", "slash"),
            };
            foreach ((string kind, string who, string weapon, string off, string blow) in kinds) AutoTake(kind, who, weapon, off, blow);
            RangedTake("bow", you, "longbow", "arrow");
            RangedTake("crossbow", you, "light-crossbow", "arrow");

            SkillTakes();
            CritTake();
            BlockTake();
            DodgeTake();
            KnockdownTake();
            DeathTake();
            foreach (string beast in new[] { "sea-wraith", "gloomfang", "thornmother" }) BeastTake(beast);
            MarkerTakes();
            SchoolsTake();
            NumbersTake();
        }

        private void AutoTake(string kind, string who, string weapon, string off, string blow)
        {
            Take take = NewTake("auto-" + kind, 2.6f);
            take.Setup = () =>
            {
                WalkerSprite by = weapon == "none" ? MemberUnarmed(who, kind) : Member(who, weapon, kind, off);
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, Melee);
                Wire(by, onHit: () => Land(by, on, 6, 30, blow: blow));
                Wire(on);
                take.At.Clear();
                take.On(0.15f, () => by.PlayMove(ClipLibrary.AutoName(kind, 0)));
                take.On(0.95f, () => by.PlayMove(ClipLibrary.AutoName(kind, 1)));
                take.On(1.75f, () => by.PlayMove(ClipLibrary.AutoName(kind, 2)));
            };
        }

        /// <summary>A party member with nothing in their hands.</summary>
        private WalkerSprite MemberUnarmed(string origin, string kind)
        {
            PartyMember member = Origins.Create(origin);
            member.Unequip(EquipmentSlot.MainHand);
            member.Unequip(EquipmentSlot.OffHand);
            return Enter(new WalkerSprite { Art = ImportedArt.For(member), WeaponKind = kind });
        }

        private void RangedTake(string kind, string who, string weapon, string piece)
        {
            Take take = NewTake("auto-" + kind, 2.4f);
            take.Setup = () =>
            {
                WalkerSprite by = Member(who, weapon, kind);
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, Ranged);
                const float flight = 0.22f;
                Wire(by, onRelease: () =>
                {
                    Vector2 from = by.BonePosition("hand_far"), to = on.BonePosition("torso");
                    _effects.Launch(piece, from, to, flight, null);
                    int at = _frame + (int)(flight * 60f);
                    take.On(at / 60f, () => Land(by, on, 5, 30, blow: "thrust"));
                });
                Wire(on);
                take.At.Clear();
                take.On(0.15f, () => by.PlayMove(ClipLibrary.AutoName(kind, 0)));
                take.On(1.25f, () => by.PlayMove(ClipLibrary.AutoName(kind, 1)));
            };
        }

        private void SkillTakes()
        {
            // A heavy melee skill: a big blow with a shake and the kick.
            Take cleave = NewTake("skill-fighter-cleave", 2f);
            cleave.Setup = () =>
            {
                WalkerSprite by = Member(Origins.You, "greatsword", "two-hand");
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, Melee);
                Wire(by, onHit: () => Land(by, on, 14, 30, heavy: true));
                Wire(on);
                cleave.At.Clear();
                cleave.On(0.15f, () => by.PlayMove("skill-fighter-cleave"));
            };

            // A bolt of fire: the charge in the hand, the release, the flight and the burst.
            Take bolt = NewTake("skill-wizard-fire-bolt", 2f);
            bolt.Setup = () =>
            {
                WalkerSprite by = Member("mira", "quarterstaff", "staff");
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, Ranged);
                Clip clip = null;
                Wire(by, onRelease: () =>
                {
                    _effects.Projectile(Element.Fire, by.BonePosition("hand_near"), on.BonePosition("torso"), 0.3f);
                    bolt.On((_frame + 18) / 60f, () => Land(by, on, 9, 30, element: Element.Fire));
                });
                Wire(on);
                bolt.At.Clear();
                bolt.On(0.15f, () =>
                {
                    clip = by.PlayMove("skill-wizard-fire-bolt");
                    float charge = clip?.FirstEvent(ClipEvent.Release) ?? 0.3f;
                    _effects.ChargeIn(() => by.BonePosition("hand_near"), Element.Fire, charge);
                });
            };

            // Three darts of force, one after another: a flurry's several blows.
            Take missiles = NewTake("skill-wizard-magic-missile", 2f);
            missiles.Setup = () =>
            {
                WalkerSprite by = Member("mira", "quarterstaff", "staff");
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, Ranged);
                Wire(by);
                Wire(on);
                missiles.At.Clear();
                missiles.On(0.15f, () => by.PlayMove("skill-wizard-magic-missile"));
                for (int i = 0; i < 3; i++)
                {
                    float at = 0.45f + 0.12f * i;
                    float lift = (i - 1) * 14f;
                    missiles.On(at, () => _effects.Projectile(Element.Force, by.BonePosition("hand_near"), on.BonePosition("torso"), 0.25f, lift, bursts: false));
                    missiles.On(at + 0.25f, () => Land(by, on, 3, 30, element: Element.Force));
                }
            };

            // Healing on an ally: the glow, green numbers.
            Take heal = NewTake("skill-cleric-cure-wounds", 1.6f);
            heal.Setup = () =>
            {
                WalkerSprite by = Member("mira");
                WalkerSprite on = Member("brannock");
                Face(by, on, 50f);
                on.Facing = Direction.Right;
                Wire(by, onHit: Healed, onRelease: Healed);
                Wire(on);
                heal.At.Clear();
                heal.On(0.15f, () => by.PlayMove("skill-cleric-cure-wounds"));

                void Healed()
                {
                    _effects.HealGlow(on.BonePosition("torso"));
                    on.FlashTint(new Color(110, 240, 130), 10);
                    _popups.Heal("ally", on.BonePosition("head") - new Vector2(0f, 10f), 11);
                }
            };

            // The ultimate: the ball of fire with its ring on the floor first.
            Take fireball = NewTake("skill-wizard-fireball", 3f);
            fireball.Setup = () =>
            {
                WalkerSprite by = Member("mira", "quarterstaff", "staff");
                WalkerSprite a = Foe("hollowed-footman", "one-hand"), b = Foe("red-gull-thug", "one-hand");
                Face(by, a, 110f);
                b.Position = a.Position + new Vector2(26f, 6f);
                b.Facing = Direction.Left;
                Wire(by);
                Wire(a);
                Wire(b);
                fireball.At.Clear();
                Vector2 centre = a.Position + new Vector2(13f, 3f);
                int ring = 0;
                fireball.On(0.1f, () => ring = _effects.Mark(MarkerShape.Ring, centre, centre, 34f, 1.2f, colour: new Color(255, 140, 40)));
                fireball.On(0.1f, () => by.PlayMove("skill-wizard-fireball"));
                fireball.On(0.1f, () => _effects.ChargeIn(() => by.BonePosition("hand_near"), Element.Fire, 1.0f));
                fireball.On(1.0f, () => _effects.Projectile(Element.Fire, by.BonePosition("hand_near"), centre - new Vector2(0f, 20f), 0.3f, 18f, bursts: false));
                fireball.On(1.3f, () =>
                {
                    _effects.Resolve(ring);
                    _effects.Burst(centre - new Vector2(0f, 14f), Element.Fire, 2.2f);
                    _effects.Shaker.Kick(ShakeCause.AreaSpell, false, _camera.ToScreen(centre));
                    Land(by, a, 12, 30, heavy: true, element: Element.Fire);
                    Land(by, b, 12, 30, heavy: true, element: Element.Fire);
                });
            };
        }

        private void CritTake()
        {
            Take take = NewTake("crit", 2f);
            take.Setup = () =>
            {
                WalkerSprite by = Member("brannock", null, "one-hand");
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, Melee);
                Wire(by, onHit: () => Land(by, on, 13, 30, critical: true));
                Wire(on);
                take.At.Clear();
                take.On(0.15f, () => by.PlayMove(ClipLibrary.AutoName("one-hand", 1)));
            };
        }

        private void BlockTake()
        {
            Take take = NewTake("block", 1.6f);
            take.Setup = () =>
            {
                WalkerSprite by = Foe("hollowed-footman", "one-hand");
                WalkerSprite on = Member("brannock", null, "one-hand", "wooden-shield");
                Face(on, by, Melee);
                Wire(by, onHit: () =>
                {
                    on.PlayMove("block");
                    on.Freeze(3, HitStopRole.Victim, 1f);
                    by.Freeze(3, HitStopRole.Attacker);
                    on.Knockback(new Vector2(-2f, 0f));
                    _effects.Impact(on.BonePosition("hand_far"), new Vector2(-1f, 0f), Element.Physical, blow: "blunt");
                    _popups.Avoided("you", on.BonePosition("head") - new Vector2(0f, 10f), "BLOCK");
                });
                Wire(on);
                take.At.Clear();
                take.On(0.15f, () => by.PlayMove(ClipLibrary.AutoName("one-hand", 0)));
            };
        }

        private void DodgeTake()
        {
            Take take = NewTake("dodge", 1.6f);
            take.Setup = () =>
            {
                WalkerSprite by = Foe("hollowed-footman", "one-hand");
                WalkerSprite on = Member("kael", null, "dagger");
                Face(on, by, Melee);
                Wire(by);
                Wire(on);
                take.At.Clear();
                Clip swing = null;
                take.On(0.15f, () =>
                {
                    swing = by.PlayMove(ClipLibrary.AutoName("one-hand", 0));
                    // The dodge starts a little before the blow lands, so it's clear of it on the frame.
                    float hit = swing?.FirstEvent(ClipEvent.Hit) ?? 0.3f;
                    take.On(0.15f + MathF.Max(0f, hit - 0.12f), () =>
                    {
                        on.PlayMove("dodge");
                        _popups.Avoided("you", on.BonePosition("head") - new Vector2(0f, 10f), "DODGE");
                    });
                });
            };
        }

        private void KnockdownTake()
        {
            Take take = NewTake("knockdown", 3.2f);
            take.Setup = () =>
            {
                WalkerSprite by = Member(Origins.You, "greatsword", "two-hand");
                WalkerSprite on = Foe("red-gull-thug", "one-hand");
                Face(by, on, Melee);
                Wire(by, onHit: () =>
                {
                    Land(by, on, 10, 30, heavy: true, blow: "blunt");
                    on.PlayMove("knockdown", hold: true);
                });
                Wire(on);
                take.At.Clear();
                take.On(0.15f, () => by.PlayMove("skill-fighter-cleave"));
                take.On(2.1f, () => on.PlayMove("getup"));
            };
        }

        private void DeathTake()
        {
            Take take = NewTake("death", 3.4f);
            take.Setup = () =>
            {
                WalkerSprite by = Member("brannock", null, "one-hand");
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, Melee);
                Wire(by, onHit: () => Land(by, on, 30, 30, kill: true));
                Wire(on);
                take.At.Clear();
                take.On(0.15f, () => by.PlayMove(ClipLibrary.AutoName("one-hand", 2)));
                FadeOut(take, on, 2.3f, 0.6f);
            };
        }

        private void BeastTake(string id)
        {
            Take take = NewTake("beast-" + id, 6.4f);
            take.Setup = () =>
            {
                WalkerSprite beast = Beast(id);
                WalkerSprite hero = Member("brannock", null, "one-hand");
                float apart = id == "thornmother" ? 60f : 38f;
                Face(hero, beast, apart);
                Wire(beast, onHit: () =>
                {
                    hero.TakeHit(beast, 7, 40, false, false, id == "thornmother");
                    hero.PlayMove("hurt");
                    _effects.Impact(hero.BonePosition("torso"), new Vector2(-1f, 0f), Element.Physical, false, id == "thornmother", "blunt");
                    _popups.Damage("you", hero.BonePosition("head") - new Vector2(0f, 10f), 7, false, taken: true);
                    _effects.Shaker.Kick(ShakeCause.PlayerHit, false, _camera.ToScreen(hero.Position), new Vector2(-1f, 0f));
                });
                Wire(hero, onHit: () => Land(hero, beast, id == "thornmother" ? 8 : 30, 30, kill: id != "thornmother"));
                take.At.Clear();
                take.On(0.1f, () =>
                {
                    beast.PlayMove("telegraph-claw");
                    _effects.Glint(beast.BonePosition("head") + new Vector2(-6f, 0f));
                });
                take.On(0.9f, () => beast.PlayMove("auto-claw-1"));
                take.On(2.0f, () => beast.PlayMove("hurt"));
                take.On(2.0f, () => Land(hero, beast, 6, 30));
                take.On(2.9f, () => hero.PlayMove(ClipLibrary.AutoName("one-hand", 2)));
                if (id == "thornmother") take.On(4.0f, () => Land(hero, beast, 30, 30, kill: true, heavy: true));
            };
        }

        private void MarkerTakes()
        {
            foreach (MarkerShape shape in new[] { MarkerShape.Ring, MarkerShape.Line, MarkerShape.Cone })
            {
                Take take = NewTake("marker-" + shape.ToString().ToLowerInvariant(), 2.6f);
                take.Setup = () =>
                {
                    WalkerSprite foe = Foe("hollowed-sapper", "polearm");
                    WalkerSprite hero = Member("brannock", null, "one-hand");
                    Face(hero, foe, 110f);
                    Wire(foe);
                    Wire(hero);
                    take.At.Clear();
                    int id = 0;
                    take.On(0.1f, () =>
                    {
                        foe.PlayMove("telegraph-polearm");
                        Vector2 from = shape == MarkerShape.Ring ? hero.Position : foe.Position;
                        id = _effects.Mark(shape, from, hero.Position, shape == MarkerShape.Ring ? 30f : 120f, 1.5f, width: 26f, halfAngle: 0.45f);
                    });
                    take.On(1.6f, () =>
                    {
                        _effects.Resolve(id);
                        foe.PlayMove("auto-polearm-1");
                        _effects.Burst(hero.BonePosition("torso"), Element.Thunder, 1.2f);
                        _effects.Shaker.Kick(ShakeCause.BossSlam, false, _camera.ToScreen(hero.Position));
                        hero.TakeHit(foe, 12, 40, false, false, true);
                        hero.PlayMove("hurt-big");
                        _popups.Damage("you", hero.BonePosition("head") - new Vector2(0f, 10f), 12, false, taken: true);
                    });
                };
            }
        }

        private void SchoolsTake()
        {
            Element[] schools = Enum.GetValues<Element>().Where(e => e != Element.Physical).ToArray();
            Take take = NewTake("schools", 0.5f * schools.Length + 1.2f);
            take.Setup = () =>
            {
                WalkerSprite by = Member("mira", "quarterstaff", "staff");
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, 140f);
                Wire(by);
                Wire(on);
                take.At.Clear();
                for (int i = 0; i < schools.Length; i++)
                {
                    Element school = schools[i];
                    float at = 0.1f + 0.5f * i;
                    take.On(at, () =>
                    {
                        by.PlayMove("cast");
                        if (school == Element.Lightning) _effects.BeamFrom(Element.Lightning, by.BonePosition("hand_near"), on.BonePosition("torso"), 0.25f);
                        else _effects.Projectile(school, by.BonePosition("hand_near"), on.BonePosition("torso"), 0.3f, school == Element.Poison ? 20f : 0f, bursts: true);
                    });
                    take.On(at + 0.3f, () =>
                    {
                        on.TakeHit(by, 4, 30, false, false, false, CombatEffects.ColourOf(school));
                        on.PlayMove("hurt");
                        _popups.Tick("target", on.BonePosition("head") - new Vector2(0f, 10f), 4, CombatEffects.ColourOf(school));
                    });
                }
            };
        }

        private void NumbersTake()
        {
            Take take = NewTake("numbers", 2.2f);
            take.Setup = () =>
            {
                WalkerSprite by = Member("kael", null, "dagger");
                WalkerSprite on = Foe("hollowed-footman", "one-hand");
                Face(by, on, Melee);
                Wire(by);
                Wire(on);
                take.At.Clear();
                for (int i = 0; i < 10; i++)
                {
                    int n = i;
                    take.On(0.1f + 0.09f * i, () =>
                    {
                        bool crit = n == 4;
                        _popups.Damage("target", on.BonePosition("head") - new Vector2(0f, 10f), 2 + n, crit, false);
                        if (n % 3 == 0) _popups.Avoided("target", on.BonePosition("head") - new Vector2(0f, 10f), n == 3 ? "MISS" : "DODGE");
                    });
                }
                take.On(1.2f, () => _popups.Heal("you", by.BonePosition("head") - new Vector2(0f, 10f), 15));
                take.On(1.3f, () => _popups.Damage("you", by.BonePosition("head") - new Vector2(0f, 10f), 9, false, taken: true));
            };
        }

        // ------------------------------------------------------------------ checks

        /// <summary>Writes every track of a handful of clips (those with per-key easing first) sampled every frame, for checking against tools/rig's sampler.</summary>
        private void WriteSamples()
        {
            var text = new StringBuilder("clip\tbone\tchannel\tt\tvalue\n");
            Skeleton skeleton = Skeleton.Current;
            if (skeleton is null) return;
            var names = new List<string>();
            foreach (string name in ClipLibrary.Names.OrderBy(n => n, StringComparer.Ordinal))
            {
                Clip clip = ClipLibrary.Get(name);
                bool keyed = clip.Tracks.Any(t => t.Eases is not null);
                if (keyed && names.Count < 40) names.Add(name);
            }
            foreach (string name in new[] { "idle", "ready", "strike-one-hand", "jog", "die" })
                if (ClipLibrary.Get(name) is not null && !names.Contains(name)) names.Add(name);
            foreach (string name in names)
            {
                Clip clip = ClipLibrary.Get(name);
                foreach (ClipTrack track in clip.Tracks)
                {
                    string channel = track.Channel.ToString().ToLowerInvariant();
                    for (int f = 0; f <= (int)(clip.Length * 60f) + 1; f++)
                    {
                        // The time as the game samples it (a float), written out in full so a check
                        // samples at the same instant: on a fast stretch the float's rounding alone
                        // moves the value by more than a ten-thousandth.
                        float t = f / 60f;
                        text.Append(name).Append('\t').Append(skeleton.Names[track.Bone]).Append('\t').Append(channel).Append('\t')
                            .Append(((double)t).ToString("R", CultureInfo.InvariantCulture)).Append('\t')
                            .Append(track.At(t, clip.Ease).ToString("R", CultureInfo.InvariantCulture)).Append('\n');
                    }
                }
            }
            File.WriteAllText(Path.Combine(_folder, "samples.tsv"), text.ToString());
        }

        /// <summary>Writes each of the effects' made shapes, to look at.</summary>
        private void WriteShapes()
        {
            using var textures = new EffectTextures(ScreenManager.GraphicsDevice);
            foreach (EffectShape shape in Enum.GetValues<EffectShape>())
            {
                using FileStream file = File.Create(Path.Combine(_folder, "shape-" + shape.ToString().ToLowerInvariant() + ".png"));
                textures[shape].SaveAsPng(file, textures[shape].Width, textures[shape].Height);
            }
        }
    }
}
