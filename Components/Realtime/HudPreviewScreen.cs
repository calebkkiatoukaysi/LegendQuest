using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;
using LegendQuest.StateManagement;
using LegendQuest.World;

namespace LegendQuest.Components.Realtime;

/// <summary>
/// The proof run's look at the real-time HUD, before the fight that will drive it is built: every
/// component drawn from made-up views over a real room, with a party and foes standing in it, in
/// one of a few states, and a contact sheet of every icon.
/// </summary>
/// <remarks>
/// Only the proof run opens it (LegendQuestGame.HudProof.cs, the Hud stage). It takes no input. The
/// states are set pieces: a fight under way (a cooldown mid-sweep, an armed reaction, a skill there
/// isn't Focus for, a foe winding up, the target casting), the same with a slot's card up, reaching
/// too far (slots out of reach, the global cooldown over the bar, a press, your cast, a heal's
/// target, a card over an aura), and a boss (its wide frame in its second phase,
/// casting what can't be stopped, someone down with their death saves, the sigil lit, a slot
/// catching its glint). The icon sheets are drawn twice their size with their ids under them.
/// Everything goes through CombatHud, from a HudView built each frame the way the fight screen will
/// build one, with a made-up mouse on a slot or an aura for the cards.
/// </remarks>
public class HudPreviewScreen : GameScreen
{
    /// <summary>The states it can show.</summary>
    public enum Shot
    {
        Fight,
        Card,
        Reach,
        Boss,
        Icons,
    }

    /// <summary>The room the party stands in, and where in it.</summary>
    private const string Zone = "road", Room = "farmland";
    private const float CentreX = 1700f / ZoneCatalog.RoomScale;

    /// <summary>Where everyone's feet are, in world pixels across from the middle of the screen and down the room.</summary>
    private const float FrontFeet = 196f, BackFeet = 184f;

    /// <summary>How far down a figure's box its head is: the imported figures' boxes have room over their heads.</summary>
    private const float HeadRoom = 0.12f;

    /// <summary>The contact sheet: how many icons to a row and to a page, and each cell's size.</summary>
    private const int SheetColumns = 16;
    private const int SheetRows = 9;
    private const int CellWidth = 98;
    private const int CellHeight = 92;

    /// <summary>How many auras to a row under a frame, as FighterFrame lays them out.</summary>
    private const int FrameAurasPerRow = 10;

    private readonly CombatHud _hud = new();
    private readonly Camera2D _camera = new(new Point(LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight));
    private readonly List<(string Id, WalkerSprite Walker, bool Foe)> _figures = new();

    private ContentManager _content;
    private RoomScene _scene;
    private Color _tint = Color.White;

    /// <summary>The state it's in.</summary>
    public Shot State { get; private set; }

    /// <summary>Which page of the icons' contact sheet it's on.</summary>
    public int Page { get; private set; }

    /// <summary>How many pages the icons' contact sheet takes.</summary>
    public int Pages => (_hud.Kit.Icons.Ids.Count + SheetColumns * SheetRows - 1) / (SheetColumns * SheetRows);

    /// <summary>Opens on a state.</summary>
    /// <param name="state">Which.</param>
    public HudPreviewScreen(Shot state) => State = state;

    /// <summary>Loads the HUD, the room and the people in it.</summary>
    public override void Activate()
    {
        _content ??= new ContentManager(ScreenManager.Game.Services, "Content");
        _hud.LoadContent(_content, ScreenManager.GraphicsDevice, ScreenManager.BlankTexture);

        RoomDefinition room = ZoneCatalog.Get(Zone).Room(Room);
        _scene = new RoomScene(room);
        _scene.LoadContent(ScreenManager.Game.Services, ScreenManager.GraphicsDevice);
        _tint = room.Tint;
        _camera.LockTo(_scene.Bounds, (int)ZoneCatalog.RoomScale);
        _camera.SnapTo(new Vector2(CentreX, ZoneCatalog.RoomHeight / 2f));
        Cast(State);
    }

    /// <summary>
    /// Checks the HUD's sums that a picture can't show: that each slot's middle hits that slot and the
    /// gaps between hit none, that the aura rows and the party's rows hit what they draw, that the
    /// timers write "12", "12s", "2.5" and "2m", that every font a timer's drawn in carries its
    /// marks, and that the sweep's frames run from none to the whole circle.
    /// </summary>
    /// <returns>What's wrong, a line each; none if everything's right.</returns>
    public List<string> Check()
    {
        var problems = new List<string>();
        void Expect<T>(string what, T got, T wanted)
        {
            if (!EqualityComparer<T>.Default.Equals(got, wanted)) problems.Add($"{what}: got {got}, wanted {wanted}");
        }

        Rectangle[] slots = Hotbar.Slots();
        for (int i = 0; i < slots.Length; i++)
        {
            Expect($"Hotbar.At(slot {i + 1}'s middle)", Hotbar.At(slots[i].Center), i);
            Expect($"Hotbar.At(slot {i + 1}'s top left)", Hotbar.At(slots[i].Location), i);
            Expect($"Hotbar.At(just past slot {i + 1})", Hotbar.At(new Point(slots[i].Right + 1, slots[i].Center.Y)), -1);
        }
        Expect("Hotbar.At(the middle of the screen)", Hotbar.At(new Point(800, 450)), -1);

        Rectangle[] auras = AuraRow.Layout(12, new Point(100, 100), 5, true);
        for (int i = 0; i < auras.Length; i++) Expect($"AuraRow.At(aura {i})", AuraRow.At(auras[i].Center, 12, new Point(100, 100), 5, true), i);
        Rectangle[] leftward = AuraRow.Layout(4, new Point(500, 100), 10, true, rightToLeft: true);
        Expect("AuraRow right to left, the first's right edge", leftward[0].Right, 500);
        Expect("AuraRow right to left, At(the third)", AuraRow.At(leftward[2].Center, 4, new Point(500, 100), 10, true, true), 2);

        Rectangle[] rows = _hud.Party.Rows(3);
        for (int i = 0; i < rows.Length; i++) Expect($"PartyFrames.At(row {i})", _hud.Party.At(rows[i].Center, 3), i);

        // The whole HUD's hit-testing, on the fight's own view: a slot, the sigil, your third aura, a party row, the sky.
        HudView view = View();
        Expect("CombatHud.At(slot 3)", _hud.At(slots[2].Center, view), new CombatHud.Spot(CombatHud.Part.Slot, 2, null));
        Expect("CombatHud.At(the sigil)", _hud.At(slots[Hotbar.SigilIndex].Center, view), new CombatHud.Spot(CombatHud.Part.Sigil, Hotbar.SigilIndex, null));
        Point third = AuraRow.Layout(view.Player.Auras.Count, _hud.Player.AurasAt, FrameAurasPerRow, true)[2].Center;
        Expect("CombatHud.At(your third aura)", _hud.At(third, view).Aura, view.Player.Auras[2]);
        Expect("CombatHud.At(the second party row)", _hud.At(_hud.Party.Rows(view.Party.Count)[1].Center, view), new CombatHud.Spot(CombatHud.Part.PartyRow, 1, null));
        Expect("CombatHud.At(the sky)", _hud.At(new Point(800, 300), view), CombatHud.Spot.Nothing);

        Expect("Seconds(12.0)", HudKit.Seconds(12f), "12");
        Expect("Seconds(11.2)", HudKit.Seconds(11.2f), "12");
        Expect("Seconds(12.0, unit)", HudKit.Seconds(12f, unit: true), "12s");
        Expect("Seconds(2.46)", HudKit.Seconds(2.46f), "2.5");
        Expect("Seconds(0.04)", HudKit.Seconds(0.04f), "0.1");
        Expect("Seconds(75)", HudKit.Seconds(75f), "2m");

        foreach ((string name, SpriteFont font) in new[] { ("spectral-timer", _hud.Kit.Timer), ("spectral-small", _hud.Kit.Small), ("spectral-number", _content.Load<SpriteFont>("spectral-number")) })
            foreach (char mark in "0123456789.sm%")
                if (!font.Characters.Contains(mark)) problems.Add($"{name} has no '{mark}'");

        Expect("SweepFrame(0)", HudIcons.SweepFrame(0f), Rectangle.Empty);
        Expect("SweepFrame(1) is the last frame", HudIcons.SweepFrame(1f).Location,
            new Point((HudIcons.SweepFrames - 1) % 12 * HudIcons.SweepSize, (HudIcons.SweepFrames - 1) / 12 * HudIcons.SweepSize));
        Expect("SweepFrame(0.001) is the first frame", HudIcons.SweepFrame(0.001f).Location, Point.Zero);

        foreach (string id in new[] { "fighter-strike", "hourwarden-stolen-minute", "drink-potion", "soul-link-strike", "telegraph", "burning", "buff", "debuff" })
            if (!_hud.Kit.Icons.Has(id)) problems.Add($"No icon called {id}");
        Expect("fighter-fighting-style is a passive", _hud.Kit.Icons.IsPassive("fighter-fighting-style"), true);
        Expect("fighter-cleave is a passive", _hud.Kit.Icons.IsPassive("fighter-cleave"), false);
        return problems;
    }

    /// <summary>Lets go of the room and the content.</summary>
    public override void Unload()
    {
        _scene?.Unload();
        _content?.Unload();
        base.Unload();
    }

    /// <summary>Changes the state, and who's standing in the room for it.</summary>
    /// <param name="state">Which state.</param>
    /// <param name="page">For the icons, which page.</param>
    public void Show(Shot state, int page = 0)
    {
        State = state;
        Page = page;
        Cast(state);
        _hud.Reset();
        if (state is Shot.Fight or Shot.Card)
        {
            _hud.Target.ShowHit("footman-1", 0.82f, 0.62f);
            _hud.Player.ShowHit("you", 0.86f, 0.72f);
        }
        if (state == Shot.Boss) _hud.Boss.ShowHit("thornmother", 0.58f, 0.52f);
    }

    /// <summary>Catches the boss state's slot coming off its cooldown half way through its glint, for the frame before its picture.</summary>
    public void CatchGlint() => _hud.Hotbar.Ready(1, 0.45f);

    /// <summary>Runs the room, the figures and the HUD's clocks.</summary>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, false);
        _scene?.Update(gameTime);
        foreach ((_, WalkerSprite walker, _) in _figures) walker.Update(gameTime);
        if (State != Shot.Icons) _hud.Update(gameTime, View());
    }

    /// <summary>Draws the room and its people, then the HUD in the state it's in, or a page of the icons.</summary>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        ScreenManager.GraphicsDevice.Clear(Color.Black);
        if (State == Shot.Icons)
        {
            DrawIcons(spriteBatch);
            return;
        }

        _scene.DrawBack(gameTime, spriteBatch, _camera, _tint);

        HudView view = View();
        Point? mouse = Mouse(view);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _hud.DrawRing(spriteBatch, view, 0.9f);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: _camera.Transform);
        foreach ((_, WalkerSprite walker, _) in _figures)
            walker.Draw(spriteBatch, _scene.Depth(walker.Position), _camera.Pixel, _tint);
        spriteBatch.End();
        _scene.DrawFront(spriteBatch, _camera, _tint * 0.3f);

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        _hud.DrawArt(spriteBatch, view, mouse, 1f);
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        _hud.DrawWords(spriteBatch, view, mouse, 1f);
        spriteBatch.End();
    }

    /// <summary>Everything the HUD shows in the state it's in, as the fight screen will build it from the battle.</summary>
    private HudView View()
    {
        FighterHudView you = You();
        if (YourCast() is { } cast) you = you with { Casting = cast };
        return new HudView(you, Party(), Target(), Boss(), Slots(), Sigil(), Plates());
    }

    /// <summary>Where the made-up mouse is: on slot 2 for its card, on your fourth aura while reaching, or nowhere.</summary>
    private Point? Mouse(HudView view) => State switch
    {
        Shot.Card => Hotbar.Slots()[1].Center,
        Shot.Reach => AuraRow.Layout(view.Player.Auras.Count, _hud.Player.AurasAt, FrameAurasPerRow, true)[3].Center,
        _ => null,
    };

    /// <summary>A page of every icon, twice its size, with its id under it: skills, the bar's own and the foes' moves, then auras.</summary>
    private void DrawIcons(SpriteBatch spriteBatch)
    {
        HudKit kit = _hud.Kit;
        IReadOnlyList<string> ids = kit.Icons.Ids;
        int perPage = SheetColumns * SheetRows;
        int first = Page * perPage, last = Math.Min(ids.Count, first + perPage);
        int left = (LegendQuestGame.ScreenWidth - SheetColumns * CellWidth) / 2;
        const int top = 40;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        spriteBatch.Draw(kit.Pixel, new Rectangle(0, 0, LegendQuestGame.ScreenWidth, LegendQuestGame.ScreenHeight), new Color(22, 20, 30));
        for (int i = first; i < last; i++)
        {
            int cell = i - first;
            var at = new Vector2(left + cell % SheetColumns * CellWidth + (CellWidth - 48) / 2, top + cell / SheetColumns * CellHeight);
            kit.Icons.Draw(spriteBatch, ids[i], at, 2f, Color.White);
        }
        spriteBatch.End();

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
        Text.Draw(spriteBatch, kit.Words, $"HUD ICONS · PAGE {Page + 1} OF {Pages} · {ids.Count} ICONS · GOLD RIM = PASSIVE", LegendQuestGame.ScreenWidth / 2f, 6,
            Palette.Heading, Palette.Shadow, centred: true);
        for (int i = first; i < last; i++)
        {
            int cell = i - first;
            float x = left + cell % SheetColumns * CellWidth + CellWidth / 2f;
            float y = top + cell / SheetColumns * CellHeight + 50;
            // Two lines: the class (or the first word), then the rest.
            string id = ids[i];
            int dash = id.IndexOf('-');
            if (dash > 0 && id[..dash] is "lodge" or "ember" or "gilded") dash = id.IndexOf('-', dash + 1);
            string line1 = dash > 0 ? id[..dash] : id, line2 = dash > 0 ? id[(dash + 1)..] : null;
            Text.Draw(spriteBatch, kit.Small, line1, x, y, Palette.Note, Palette.Shadow, centred: true, width: CellWidth - 4);
            if (line2 is not null)
                Text.Draw(spriteBatch, kit.Small, line2, x, y + kit.Small.LineSpacing - 3, Palette.Bone, Palette.Shadow, centred: true, width: CellWidth - 4);
        }
        spriteBatch.End();
    }

    // ----------------------------------------------------------------------------------------- //
    // Who's in the room
    // ----------------------------------------------------------------------------------------- //

    /// <summary>Puts the people for a state in the room: the party on the left, the foes (or the boss) on the right.</summary>
    private void Cast(Shot state)
    {
        _figures.Clear();
        if (state == Shot.Icons) return;
        Stand("you", "kael", -95f, FrontFeet, false);
        Stand("sera", "sera", -140f, BackFeet, false);
        Stand("mira", "mira", -165f, FrontFeet + 6f, false, down: state == Shot.Boss);
        Stand("brannock", "brannock", -58f, BackFeet - 4f, false);
        if (state == Shot.Boss)
        {
            Stand("thornmother", null, 95f, FrontFeet + 2f, true, Creatures.Get("thornmother"));
            Stand("whelp", null, 5f, BackFeet + 6f, true, Creatures.Get("gloomfang"));
        }
        else
        {
            Creature footman = Creatures.Get("hollowed-footman");
            Stand("footman-1", "hollowed-footman", 20f, FrontFeet, true, footman);
            Stand("footman-2", "hollowed-footman", 80f, BackFeet, true, footman);
            Stand("hexer", "hollowed-hexer", 140f, FrontFeet - 2f, true, Creatures.Get("hollowed-hexer"));
        }
    }

    /// <summary>Stands someone in the room: their imported figure if they've one, else a creature's sheet.</summary>
    private void Stand(string id, string figure, float across, float feet, bool foe, Creature creature = null, bool down = false)
    {
        var walker = new WalkerSprite { Art = ImportedArt.Get(figure), Facing = foe ? Direction.Left : Direction.Right };
        if (creature is not null && walker.Art is not { HasBody: true })
            walker = new WalkerSprite { Shape = creature.Shape, Sheet = _content.Load<Texture2D>(creature.Texture), Art = walker.Art, Facing = foe ? Direction.Left : Direction.Right };
        walker.LoadContent(_content);
        walker.Position = new Vector2(CentreX + across, feet);
        if (down && walker.PlayClip("downed") <= 0f) walker.Play(WalkerPose.Downed);
        else if (walker.PlayClip("ready") <= 0f) walker.Release();
        _figures.Add((id, walker, foe));
    }

    /// <summary>Everyone's nameplate, from where they stand, back to front.</summary>
    private List<NameplateView> Plates()
    {
        var plates = new List<NameplateView>();
        foreach ((string id, WalkerSprite walker, bool foe) in _figures.OrderBy(f => f.Walker.Position.Y))
        {
            if (id == "you") continue;
            var box = walker.Figure;
            Vector2 head = _camera.ToScreen(new Vector2(walker.Position.X, box.Top + box.Height * HeadRoom));
            Vector2 feet = _camera.ToScreen(walker.Position);
            float width = box.Width * _camera.Zoom;
            (string name, float health, TelegraphView warn, bool target, bool isDown) = id switch
            {
                "footman-1" => ("Hollowed Footman", 0.62f, null, true, false),
                "footman-2" => ("Hollowed Footman", 1f, null, false, false),
                "hexer" => ("Hollowed Hexer", 0.9f, new TelegraphView("foe-ashen-bolt", "Ashen Bolt", 0.6f), false, false),
                "thornmother" => ("The Thornmother", 0.52f, new TelegraphView("foe-thorn-burst", "Thorn Burst", 0.35f), false, false),
                "whelp" => ("Gloomfang Whelp", 0.45f, new TelegraphView("foe-bite", "Bite", 0.8f), true, false),
                "sera" => ("Sera", State == Shot.Reach ? 0.38f : 0.8f, null, State == Shot.Reach, false),
                "mira" => ("Mira", State == Shot.Boss ? 0f : 0.95f, null, false, State == Shot.Boss),
                _ => ("Brannock", 0.7f, null, false, false),
            };
            if (State == Shot.Reach && foe) target = false;
            plates.Add(new NameplateView(id, name, head, feet, width, health, foe, target, warn, isDown,
                Numbers: !target ? null : id == "whelp" ? "9 / 20" : foe ? "18 / 30" : "11 / 29"));
        }
        return plates;
    }

    // ----------------------------------------------------------------------------------------- //
    // The made-up views
    // ----------------------------------------------------------------------------------------- //

    /// <summary>Someone's face: their imported figure, else their creature's sheet.</summary>
    private PortraitView Face(string id)
    {
        foreach ((string who, WalkerSprite walker, _) in _figures)
            if (who == id) return new PortraitView(walker.Sheet, walker.Shape, walker.Art);
        ImportedFigure art = ImportedArt.Get(id);
        return art is not null ? new PortraitView(null, WalkerShape.Person, art) : PortraitView.None;
    }

    /// <summary>You: a Swordhand in a fight, or an Arcanist reaching too far.</summary>
    private FighterHudView You()
    {
        bool arcanist = State == Shot.Reach;
        var auras = new List<AuraView>
        {
            new("blessed", "Blessed", 1, 7.4f, 9f, true, "+1d4 to attacks and saves.", "Mira"),
            new("inspired", "Inspired", 1, 13f, 18f, true, "+1d6 to your next attack, save or check.", "Sera"),
            new(arcanist ? "wizard-empowered" : "fighter-fighting-style", arcanist ? "Empowered" : "Fighting Style", 1, 0f, 0f, true,
                arcanist ? "Once a turn, add Int to one damage roll." : "+2 to hit with melee."),
            new("burning", "Burning", 2, 2.1f, 6f, false, "Fire damage every 3 s.", "Hollowed Hexer"),
            new("shielded", "Ward", 1, 24f, 30f, true, "Temporary hit points: hits come off these first."),
        };
        return new FighterHudView("you", "Kael", arcanist ? "LEVEL 3 ARCANIST" : "LEVEL 3 SWORDHAND", Face("you"), false,
            State == Shot.Boss ? 19 : 29, 40, State == Shot.Reach ? 0 : 6, arcanist ? 18 : 45, 100, 15, auras, null, Swing(), false, 0, 0, false);
    }

    /// <summary>The rest of the party, one row each.</summary>
    private List<FighterHudView> Party()
    {
        bool boss = State == Shot.Boss;
        return new List<FighterHudView>
        {
            new("sera", "Sera", "WILDCALLER", Face("sera"), false, State == Shot.Reach ? 11 : 23, 29, 0, 62, 100, 13,
                new List<AuraView>
                {
                    new("regenerating", "Honey Salve", 1, 5f, 9f, true, "Heals 2 at the start of each tick."),
                    new("poisoned", "Poisoned", 3, 4.2f, 9f, false, "Disadvantage on attacks; 1d4 every 3 s."),
                },
                null, -1f, false, 0, 0, State == Shot.Reach),
            new("mira", "Mira", "LIGHTBEARER", Face("mira"), false, boss ? 0 : 27, 28, 0, boss ? 0 : 38, 100, 14,
                boss ? new List<AuraView> { new("downed", "Downed", 1, 0f, 0f, false, "On the ground, rolling a death save every 3 s.") }
                    : new List<AuraView> { new("concentration", "Concentration", 1, 0f, 0f, true, "Holding Bless.") },
                null, -1f, boss, 2, 1, false),
            new("brannock", "Brannock", "IRONVEIN BULWARK", Face("brannock"), false, 31, 44, 9, 71, 100, 18,
                new List<AuraView>
                {
                    new("bulwark-stone-skin", "Stone Skin", 1, 8f, 12f, true, "Resistance to weapons."),
                    new("taunted", "Hold", 1, 2.4f, 6f, true, "Foes can't target the back row."),
                },
                null, -1f, false, 0, 0, false),
        };
    }

    /// <summary>The target: a footman casting, Sera for a heal, or a whelp under the boss.</summary>
    private FighterHudView Target()
    {
        if (State == Shot.Reach) return Party()[0];
        if (State == Shot.Boss)
            return new FighterHudView("whelp", "Gloomfang Whelp", "BEAST · PACK HUNTER", Face("whelp"), true, 9, 20, 0, 0, 0, 12,
                new List<AuraView> { new("marked", "Hunter's Mark", 1, 9f, 12f, false, "Your hits do +1d6.", "Sera") },
                new CastView("Bite", "foe-bite", 0.8f, 0.2f, true), -1f, false, 0, 0, true);
        return new FighterHudView("footman-1", "Hollowed Footman", "LEVEL 3 · UNDEAD SOLDIER", Face("footman-1"), true, 18, 30, 0, 0, 0, 13,
            new List<AuraView>
            {
                new("hexed", "Hex", 1, 6.2f, 18f, false, "Your hits on it do +1d6 necrotic.", "Kael"),
                new("bleeding", "Bleeding", 3, 4.1f, 9f, false, "Damage every 3 s.", "Kael"),
                new("slowed", "Slowed", 1, 1.6f, 6f, false, "Haste −50%, half speed."),
            },
            new CastView("Overhead Chop", "foe-cleave", 0.62f, 0.5f, true), -1f, false, 0, 0, true);
    }

    /// <summary>The boss, in the boss state.</summary>
    private BossView Boss()
    {
        if (State != Shot.Boss) return null;
        var mother = new FighterHudView("thornmother", "The Thornmother", null, Face("thornmother"), true, 312, 600, 0, 0, 0, 14,
            new List<AuraView>
            {
                new("hexed", "Hex", 1, 11f, 18f, false, "Your hits on it do +1d6 necrotic.", "Kael"),
                new("burning", "Burning", 4, 5f, 9f, false, "Fire damage every 3 s; stacks."),
                new("hasted", "Enraged", 1, 0f, 0f, true, "Attacks 25% faster."),
            },
            new CastView("Thorn Burst", "foe-thorn-burst", 0.35f, 1.2f, false), -1f, false, 0, 0, false);
        return new BossView(mother, "Mother of the Quarry", 2, new[] { 0.66f, 0.33f }, "PHASE 2");
    }

    /// <summary>Your swing, a little different in each state.</summary>
    private float Swing() => State switch { Shot.Fight or Shot.Card => 0.42f, Shot.Reach => -1f, _ => 0.85f };

    /// <summary>Your cast, while reaching: a Fire Bolt half done.</summary>
    private CastView YourCast() => State == Shot.Reach ? new CastView("Fire Bolt", "wizard-fire-bolt", 0.55f, 0.6f, true) : null;

    /// <summary>The Soul-Link sigil.</summary>
    private SoulLinkView Sigil() => State switch
    {
        Shot.Boss => new SoulLinkView(2, 2, false, false, 0f, Text: "You and Sera strike together: both weapons and 1d8 radiant, a sure hit."),
        Shot.Reach => new SoulLinkView(0, 2, true, false, 0f),
        _ => new SoulLinkView(1, 2, false, false, 0f),
    };

    /// <summary>The bar's six slots for the state.</summary>
    private IReadOnlyList<SlotView> Slots()
    {
        if (State == Shot.Reach)
        {
            const float gcd = 0.9f, gcdTotal = 1.5f;
            return new[]
            {
                new SlotView("wizard-fire-bolt", "1", "Fire Bolt", 0, 0f, 0f, 0f, 0f, true, null, true, true, 0f, 0f,
                    Detail: "AUTO ATTACK · LONG REACH", Text: "1d10 fire. Builds 5 Focus a second."),
                new SlotView("wizard-magic-missile", "2", "Magic Missile", 20, 0f, 0f, gcd, gcdTotal, false, "Sera isn't a foe.", true, false, 0f, 0f),
                new SlotView("wizard-burning-hands", "3", "Burning Hands", 25, 0f, 0f, gcd, gcdTotal, false, "Out of reach: walk closer.", true, false, 0.8f, 0f),
                new SlotView("wizard-shield", "4", "Shield", 15, 3.2f, 12f, 0f, 0f, false, "Cooling down.", true, true, 0f, 0f),
                new SlotView("wizard-fireball", "5", "Fireball", 60, 31f, 45f, gcd, gcdTotal, false, "Not enough Focus.", false, false, 0f, 0f),
                new SlotView("drink-potion", "6", "Healing Potion", 0, 0f, 0f, 0f, 0f, true, null, true, true, 0f, 0f, Count: "3"),
            };
        }
        if (State == Shot.Boss)
        {
            return new[]
            {
                new SlotView("fighter-strike", "1", "Strike", 0, 0f, 0f, 0f, 0f, true, null, true, true, 0f, 0f),
                new SlotView("fighter-cleave", "2", "Cleave", 20, 0f, 8f, 0f, 0f, true, null, true, true, 0f, 0f),
                new SlotView("fighter-second-wind", "3", "Second Wind", 20, 1.4f, 16f, 0f, 0f, false, "Cooling down.", true, true, 0f, 0f),
                new SlotView("fighter-riposte", "4", "Riposte", 20, 0f, 0f, 0f, 0f, true, null, true, true, 0f, 0f),
                new SlotView(null, "5", null, 0, 0f, 0f, 0f, 0f, false, null, true, true, 0f, 0f, Locked: "RANK 5"),
                new SlotView("drink-potion", "6", "Healing Potion", 0, 22f, 30f, 0f, 0f, false, "Cooling down.", true, true, 0f, 0f, Count: "2"),
            };
        }
        return new[]
        {
            new SlotView("fighter-strike", "1", "Strike", 0, 0f, 0f, 0f, 0f, true, null, true, true, 0f, 0f),
            new SlotView("fighter-cleave", "2", "Cleave", 20, 7.6f, 12f, 0.6f, 1.5f, false, "Cooling down.", true, true, 0f, 0f,
                Detail: "20 FOCUS · 12 s COOLDOWN · ACTION · MELEE", Text: "Weapon dice to your target and every foe within 36 px of it."),
            new SlotView("fighter-second-wind", "3", "Second Wind", 20, 0f, 0f, 0f, 0f, true, null, true, true, 0f, 0f),
            new SlotView("fighter-riposte", "4", "Riposte", 20, 0f, 12f, 0f, 0f, true, null, true, true, 0f, 3.1f),
            new SlotView("fighter-action-surge", "5", "Action Surge", 60, 0f, 0f, 0f, 0f, false, "Not enough Focus.", false, true, 0f, 0f),
            new SlotView("drink-potion", "6", "Healing Potion", 0, 0f, 0f, 0f, 0f, true, null, true, true, 0f, 0f, Count: "3"),
        };
    }
}
