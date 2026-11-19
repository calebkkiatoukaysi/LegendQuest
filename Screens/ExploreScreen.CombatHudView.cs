using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components.Realtime;
using LegendQuest.Items;
using LegendQuest.Rules;
using CastView = LegendQuest.Components.Realtime.CastView;

namespace LegendQuest.Screens;

/// <summary>
/// The fight as the HUD shows it: a HudView built from the engine's state every frame (Story/11-combat.md
/// §1): your frame, the party's rows, your target's frame with its cast bar and auras, the boss's wide
/// frame while one's in the fight, the hotbar from your bar with its sweeps, and a nameplate over
/// everyone else.
/// </summary>
/// <remarks>
/// The HUD's views (Components/Realtime/Views) never see the engine: every time here is the engine's
/// milliseconds as seconds, and every share 0 to 1. Your swing is drawn while your auto attack is on
/// and there's a foe to swing at. A foe's nameplate carries its telegraph while it winds up or casts,
/// filling to the moment its blow lands. The hotbar's slots that your rank hasn't opened show the
/// rank that opens them; the potion slot counts the healing potions in the bag.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How far down a figure's box its head is, as a share of the box: an imported figure's box has room over its head.</summary>
    private const float HeadRoom = 0.12f;

    /// <summary>The HUD as it was built this frame.</summary>
    private HudView _hudView;

    /// <summary>The engine's time as seconds.</summary>
    private static float Seconds(int ms) => ms / 1000f;

    /// <summary>Builds everything the HUD shows this frame from the fight.</summary>
    private HudView BuildHudView()
    {
        Fighter leader = _battle.Leader;
        FighterHudView player = leader is null ? null : HudOf(leader, isPlayer: true);

        var party = new List<FighterHudView>();
        foreach (Fighter fighter in _battle.Fighters)
            if (fighter.Side == Side.Party && fighter != leader && fighter.Present && !fighter.HasLeft) party.Add(HudOf(fighter));

        Fighter aimed = leader?.Target is { Present: true, HasLeft: false } target && !(target.IsDead && target.Side == Side.Foes) ? target : null;
        BossView boss = null;
        foreach (Fighter fighter in _battle.Fighters)
        {
            if (!fighter.Present || fighter.IsDead || fighter.HasLeft || !_battle.IsBoss(fighter)) continue;
            PhaseView phases = _battle.Phase(fighter);
            var thresholds = new List<float>();
            for (int i = 1; i < phases.Shares.Count; i++) thresholds.Add((float)phases.Shares[i]);
            boss = new BossView(HudOf(fighter), null, phases.Current, thresholds, phases.Name?.ToUpperInvariant());
            break;
        }

        var slots = new List<SlotView>();
        SoulLinkView sigil = null;
        if (leader is not null)
        {
            IReadOnlyList<Combat.Realtime.BarSlot> bar = _battle.Bar(leader);
            for (int key = 1; key <= Hotbar.SlotCount; key++)
            {
                Combat.Realtime.BarSlot slot = bar.FirstOrDefault(each => each.Key == key);
                slots.Add(slot is null ? EmptySlot(key) : SlotOf(leader, slot));
            }
            if (bar.FirstOrDefault(each => each.Key == Battle.SoulLinkKey) is Combat.Realtime.BarSlot link)
                sigil = new SoulLinkView(link.Charges, Battle.SoulLinkCharges, !link.Unlocked, DuoDown(), 0f,
                    Text: "When you and the Soul-Bound both land a hit on the same foe within 3 seconds, the bond charges. Twice charged, the two of you strike it as one: both weapons and 1d8 radiant, a sure hit.");
        }

        return new HudView(player, party, aimed is null ? null : HudOf(aimed), boss, slots, sigil, Plates(leader, aimed));
    }

    /// <summary>Someone as the frames show them.</summary>
    /// <param name="fighter">Who.</param>
    /// <param name="isPlayer">Whether it's your own frame, which draws your swing.</param>
    private FighterHudView HudOf(Fighter fighter, bool isPlayer = false)
    {
        WalkerSprite walker = FighterWalker(fighter.Id);
        var portrait = walker is null ? PortraitView.None : new PortraitView(walker.Sheet, walker.Shape, walker.Art);
        bool foe = fighter.Side == Side.Foes;

        var auras = new List<AuraView>();
        foreach (Aura aura in fighter.Auras)
        {
            if (aura.TotalMs > 0 && _battle.Remaining(aura) <= 0) continue;
            string icon = aura.Definition.Condition is ConditionId condition ? condition.ToString().ToLowerInvariant() : aura.Id;
            auras.Add(new AuraView(icon, aura.Name, aura.Stacks, Seconds(_battle.Remaining(aura)), Seconds(aura.TotalMs), aura.Beneficial,
                aura.Skill?.Description, aura.Source is Fighter source && source != fighter ? source.Name : null));
        }

        float swing = -1f;
        if (isPlayer && fighter.AutoAttackOn && fighter.Target is not null && fighter.SwingTotalMs > 0)
            swing = MathHelper.Clamp(1f - fighter.SwingLeftMs / (float)fighter.SwingTotalMs, 0f, 1f);

        return new FighterHudView(fighter.Id, fighter.Name, LineOf(fighter), portrait, foe,
            fighter.Hp, fighter.MaxHp, fighter.TempHp, fighter.Focus, foe ? 0 : Fighter.MaxFocus, fighter.BaseAc,
            auras, CastOf(fighter), swing, fighter.IsDown, fighter.DeathSuccesses, fighter.DeathFailures,
            fighter == _battle.Leader?.Target, fighter.IsDead || fighter.HasLeft);
    }

    /// <summary>What someone is, under their name: "LEVEL 3 BLADEMASTER" for one of the party, a guest's class, or a foe's kind.</summary>
    private static string LineOf(Fighter fighter)
    {
        if (fighter.Member is PartyMember member)
        {
            string cls = ClassCatalog.Get(member.Class).Name.ToUpperInvariant();
            return fighter.Kind == FighterKind.Member ? $"LEVEL {member.Level} {cls}  ·  RANK {fighter.Rank}" : cls;
        }
        if (fighter.Side == Side.Party) return "ALLY";
        if (fighter.Enemy is EnemyDefinition enemy) return enemy.Beast ? "BEAST" : enemy.Undead ? "UNDEAD" : "FOE";
        return "FOE";
    }

    /// <summary>What someone's casting or winding up, for a cast bar, or null.</summary>
    private CastView CastOf(Fighter fighter)
    {
        if (fighter.Casting is not Combat.Realtime.CastView cast) return null;
        float length = Math.Max(1, cast.ImpactMs - cast.StartMs);
        float through = MathHelper.Clamp((_battle.NowMs - cast.StartMs) / length, 0f, 1f);
        return new CastView(cast.Name.ToUpperInvariant(), cast.SkillId, through, Seconds(Math.Max(0, cast.ImpactMs - _battle.NowMs)), cast.Interruptible);
    }

    /// <summary>One slot of your bar as the hotbar draws it.</summary>
    private SlotView SlotOf(Fighter leader, Combat.Realtime.BarSlot slot)
    {
        SkillDefinition skill = slot.Skill;
        string key = slot.Key.ToString(CultureInfo.InvariantCulture);
        if (!slot.Unlocked)
            return new SlotView(null, key, skill.Name, 0, 0f, 0f, 0f, 0f, false, slot.WhyNot, true, true, 0f, 0f,
                Text: skill.Description, Locked: $"RANK {slot.UnlockRank}");

        bool potion = slot.Key == Battle.PotionKey;
        string icon = potion ? "drink-potion" : skill.Id;
        if (slot.Key == 1 && leader.Auto?.Id is string auto) icon = WeaponKinds.Shoots(leader.WeaponKind) && skill.Weapon ? "auto-ranged" : auto;
        return new SlotView(icon, key, skill.Name, slot.FocusCost, Seconds(slot.CooldownLeftMs), Seconds(slot.CooldownTotalMs),
            Seconds(slot.GcdLeftMs), Seconds(slot.GcdTotalMs), slot.Usable, slot.WhyNot, slot.Affordable, slot.InReach, 0f, 0f,
            Count: potion ? HealingPotions().ToString(CultureInfo.InvariantCulture) : null,
            Detail: DetailOf(slot), Text: skill.Description);
    }

    /// <summary>An empty slot: nothing in the kit goes there.</summary>
    private static SlotView EmptySlot(int key) =>
        new(null, key.ToString(CultureInfo.InvariantCulture), "Empty", 0, 0f, 0f, 0f, 0f, false, "Nothing in the kit goes here", true, true, 0f, 0f, Locked: "—");

    /// <summary>A slot card's line of numbers: the Focus, the cooldown and the reach.</summary>
    private static string DetailOf(Combat.Realtime.BarSlot slot)
    {
        var parts = new List<string>();
        if (slot.FocusCost > 0) parts.Add($"{slot.FocusCost} FOCUS");
        if (slot.CooldownTotalMs > 0) parts.Add($"{Seconds(slot.CooldownTotalMs).ToString("0.#", CultureInfo.InvariantCulture)} s COOLDOWN");
        if (slot.Skill.CastMs > 0) parts.Add($"{Seconds(slot.Skill.CastMs).ToString("0.#", CultureInfo.InvariantCulture)} s CAST");
        parts.Add(slot.Skill.Range.ToString().ToUpperInvariant());
        return string.Join("  ·  ", parts);
    }

    /// <summary>How many healing potions of every kind are in the bag.</summary>
    private int HealingPotions()
    {
        int count = 0;
        foreach (ItemDefinition item in ItemCatalog.All)
            if (item.Kind == ItemKind.Consumable && !item.Heals.IsNone) count += _game.Bag.Count(item.Id);
        return count;
    }

    /// <summary>Whether you or the Soul-Bound is down, which strains the bond.</summary>
    private bool DuoDown()
    {
        foreach (string duo in new[] { Origins.You, Origins.SoulBound })
            if (FighterOf(duo) is { IsDown: true } or { IsDead: true }) return true;
        return false;
    }

    /// <summary>Everyone's nameplate but yours, back to front: their name, health, telegraph, and the ring and marker for your target.</summary>
    private List<NameplateView> Plates(Fighter leader, Fighter aimed)
    {
        var plates = new List<NameplateView>();
        var order = new List<(Fighter Fighter, WalkerSprite Walker)>();
        foreach (Fighter fighter in _battle.Fighters)
        {
            if (fighter == leader || !fighter.Present || fighter.HasLeft || fighter.IsDead || fighter.Vanished) continue;
            if (!_fighters.TryGetValue(fighter.Id, out WalkerSprite walker) || walker.Opacity <= 0.05f) continue;
            order.Add((fighter, walker));
        }
        order.Sort((a, b) => a.Walker.Position.Y.CompareTo(b.Walker.Position.Y));

        foreach ((Fighter fighter, WalkerSprite walker) in order)
        {
            var box = walker.Figure;
            Vector2 head = _camera.ToScreen(new Vector2(walker.Position.X, box.Top + box.Height * (walker.DrawsArt ? HeadRoom : 0f)));
            Vector2 feet = _camera.ToScreen(walker.Position);
            float width = walker.ShadowSize.X * _camera.Zoom;
            TelegraphView warn = null;
            if (fighter.Side == Side.Foes && fighter.Casting is Combat.Realtime.CastView cast)
            {
                float length = Math.Max(1, cast.ImpactMs - cast.StartMs);
                warn = new TelegraphView(cast.SkillId, cast.Name, MathHelper.Clamp((_battle.NowMs - cast.StartMs) / length, 0f, 1f));
            }
            string numbers = _showAll || fighter == aimed ? $"{fighter.Hp} / {fighter.MaxHp}" : null;
            plates.Add(new NameplateView(fighter.Id, fighter.Name, head, feet, width, (float)fighter.HpShare, fighter.Side == Side.Foes,
                fighter == aimed, warn, fighter.IsDown, numbers));
        }
        return Unheaped(plates);
    }

    /// <summary>How far apart two nameplates are kept, up and down, in screen pixels.</summary>
    private const int PlateGap = 2;

    /// <summary>
    /// The plates lifted clear of each other: from the front of the floor back, each one that would
    /// sit on a plate nearer the front goes up over it, so a crowd's names stack rather than run into
    /// each other. The ring under the target stays at its feet.
    /// </summary>
    /// <param name="plates">The plates, back to front.</param>
    private List<NameplateView> Unheaped(List<NameplateView> plates)
    {
        var placed = new List<Rectangle>();
        for (int i = plates.Count - 1; i >= 0; i--)
        {
            NameplateView plate = plates[i];
            for (int tries = 0; tries < plates.Count; tries++)
            {
                Rectangle box = PlateBox(plate);
                int clash = placed.FindIndex(other => other.Intersects(box));
                if (clash < 0) break;
                plate = plate with { Head = plate.Head - new Vector2(0f, box.Bottom - placed[clash].Top + PlateGap) };
            }
            plates[i] = plate;
            placed.Add(PlateBox(plate));
        }
        return plates;
    }

    /// <summary>What a nameplate covers on the screen: as wide as its name (or its bar), from the top of its telegraph or name down to its head.</summary>
    private Rectangle PlateBox(NameplateView plate)
    {
        float wide = MathF.Max(_hud.Kit.Small.MeasureString(plate.Name).X, PlateBarRoom) + PlateGap * 2;
        float top = Nameplates.Top(_hud.Kit, plate);
        return new Rectangle((int)(plate.Head.X - wide / 2f), (int)top, (int)wide, (int)MathF.Max(1f, plate.Head.Y - top));
    }

    /// <summary>The widest a nameplate's bar is, in screen pixels, for a short name's box.</summary>
    private const float PlateBarRoom = 80f;
}
