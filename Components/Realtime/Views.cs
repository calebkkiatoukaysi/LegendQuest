using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;

namespace LegendQuest.Components.Realtime;

// The real-time fight's HUD draws nothing but these: plain, immutable records the screen builds
// each frame from the engine's state (Combat/Realtime's Battle and its Fighters), so the frames,
// the hotbar, the aura rows and the nameplates never see the engine and can be drawn from made-up
// numbers, as the HUD's proof stage does. Every time is in seconds and every share is 0 to 1.

/// <summary>Where someone's face comes from: the same three things CombatArt.Face reads off a FighterView.</summary>
/// <param name="Sheet">The sheet their face is cut from, or null.</param>
/// <param name="Shape">The shape of that sheet's frames.</param>
/// <param name="Art">The imported picture whose portrait is their face instead, or null.</param>
public sealed record PortraitView(Texture2D Sheet, WalkerShape Shape, ImportedFigure Art)
{
    /// <summary>Nobody's face: an empty well.</summary>
    public static readonly PortraitView None = new(null, WalkerShape.Person, null);
}

/// <summary>One aura on someone: a buff, a debuff, a condition, or damage or healing over time (11-combat.md §4.4).</summary>
/// <param name="IconId">Its icon in Combat/icons.json: an aura's ("burning") or a skill's ("warlock-hex"). An id with no icon falls back on the buff or the debuff.</param>
/// <param name="Name">What it's called, for its card.</param>
/// <param name="Stacks">How many stacks it has; 1 or less shows no count.</param>
/// <param name="Remaining">How long it has left, in seconds, or 0 or less for one that lasts (a passive, a stance).</param>
/// <param name="Total">How long it lasted when it was put on, in seconds, or 0 for one that lasts.</param>
/// <param name="Beneficial">Whether it's good news for whoever has it: its edge is green, a bad one's red.</param>
/// <param name="Text">What it does, in a line, for its card, or null.</param>
/// <param name="Source">Who put it there, for its card ("from Sera"), or null.</param>
public sealed record AuraView(string IconId, string Name, int Stacks, float Remaining, float Total, bool Beneficial, string Text = null, string Source = null)
{
    /// <summary>Whether it lasts with no timer.</summary>
    public bool Lasting => Total <= 0f || Remaining <= 0f;

    /// <summary>How much of it is left, 1 when it's fresh, or 1 for one that lasts.</summary>
    public float Share => Lasting ? 1f : MathHelper.Clamp(Remaining / Total, 0f, 1f);
}

/// <summary>Someone's cast: the bar that fills while they wind something up.</summary>
/// <param name="Name">What they're casting, as written on the bar ("OVERHEAD CHOP").</param>
/// <param name="IconId">Its icon, or null.</param>
/// <param name="Progress">How far through it is, 0 to 1.</param>
/// <param name="Left">How long it has to go, in seconds.</param>
/// <param name="Interruptible">Whether damage, moving or a Stun can stop it: one that can't is grey with a lock.</param>
/// <param name="Interrupted">Whether it's just been stopped: the bar shows INTERRUPTED in red while the screen keeps it up.</param>
public sealed record CastView(string Name, string IconId, float Progress, float Left, bool Interruptible, bool Interrupted = false);

/// <summary>A foe's telegraph over its head: the move it's winding up and how full the warning is.</summary>
/// <param name="IconId">The move's icon ("foe-hook-pull"), or null for the plain warning.</param>
/// <param name="Name">The move, for the nameplate when it's the target's.</param>
/// <param name="Progress">How full the warning is, 0 to 1: it lands when it's full.</param>
public sealed record TelegraphView(string IconId, string Name, float Progress);

/// <summary>
/// Someone in the fight as the frames draw them: the player's frame, the target's, a party row, and
/// the boss's frame under its own name.
/// </summary>
/// <param name="Id">Who, by their id in the fight.</param>
/// <param name="Name">What they're called.</param>
/// <param name="Line">What they are, under the name: "LEVEL 3 BLADEMASTER", or a foe's kind.</param>
/// <param name="Portrait">Where their face comes from.</param>
/// <param name="Foe">Whether they're on the other side: red health and a foe's frame.</param>
/// <param name="Hp">Their hit points.</param>
/// <param name="MaxHp">The most they can have.</param>
/// <param name="TempHp">Temporary hit points on top, drawn as a pale shield over the end of the bar, or 0.</param>
/// <param name="Focus">Their Focus.</param>
/// <param name="MaxFocus">The most Focus they can have, 100 for everyone; 0 hides the bar (a creature).</param>
/// <param name="ArmourClass">Their armour class, written on the target's frame; 0 leaves it off.</param>
/// <param name="Auras">What's on them.</param>
/// <param name="Casting">What they're casting, or null.</param>
/// <param name="Swing">How far round their auto attack's swing is, 0 to 1, or less than 0 if they're not swinging.</param>
/// <param name="IsDown">Whether they're Downed: on the ground, rolling death saves.</param>
/// <param name="DeathSuccesses">Death saves made while Downed, 0 to 3.</param>
/// <param name="DeathFailures">Death saves failed while Downed, 0 to 3.</param>
/// <param name="IsTarget">Whether they're the leader's target.</param>
/// <param name="IsOut">Whether they've fallen for the rest of the fight (or died, for a foe).</param>
public sealed record FighterHudView(string Id, string Name, string Line, PortraitView Portrait, bool Foe,
    int Hp, int MaxHp, int TempHp, int Focus, int MaxFocus, int ArmourClass,
    IReadOnlyList<AuraView> Auras, CastView Casting, float Swing,
    bool IsDown, int DeathSuccesses, int DeathFailures, bool IsTarget, bool IsOut = false)
{
    /// <summary>Their health as a share of the most they can have.</summary>
    public float HealthShare => MaxHp > 0 ? MathHelper.Clamp((float)Hp / MaxHp, 0f, 1f) : 0f;
}

/// <summary>
/// One slot of the hotbar: keys 1 to 6 (the auto attack, three skills, the ultimate and the potion).
/// The Soul-Link sigil on 7 has its own view, SoulLinkView.
/// </summary>
/// <param name="IconId">Its icon in Combat/icons.json, or null for a slot whose skill isn't unlocked yet.</param>
/// <param name="Key">Its key, written in its top corner ("1").</param>
/// <param name="Name">What it's called, for its card.</param>
/// <param name="FocusCost">What it costs, written in its bottom corner in blue; 0 writes nothing.</param>
/// <param name="CooldownLeft">How long until it's ready again, in seconds, or 0.</param>
/// <param name="CooldownTotal">How long its cooldown is from the top, in seconds, for the sweep.</param>
/// <param name="GcdLeft">How long the global cooldown has left over it, in seconds, or 0 (always 0 for a slot off the GCD).</param>
/// <param name="GcdTotal">How long the global cooldown was from the top, in seconds.</param>
/// <param name="Usable">Whether pressing it now would do something. One that isn't is dimmed, and its card says why.</param>
/// <param name="WhyNot">Why it can't be used now, for its card ("Not enough Focus", "Out of reach"), or null.</param>
/// <param name="Affordable">Whether there's Focus enough for it: one that isn't is greyed, its cost red.</param>
/// <param name="InRange">Whether the target's in its reach: one that isn't is tinted red.</param>
/// <param name="Pressed">How lit its press flash is, 1 the moment it's pressed, fading to 0. The hotbar's own Press does the same.</param>
/// <param name="Armed">How long a reaction it's armed has left, in seconds (it glows gold while it waits), or 0.</param>
/// <param name="Count">What's left of what it uses ("3" potions), in its bottom corner in gold, or null.</param>
/// <param name="Detail">The card's line of numbers ("25 FOCUS · 12 s COOLDOWN · MELEE"), or null.</param>
/// <param name="Text">What it does, for its card, or null.</param>
/// <param name="Locked">What unlocks it ("RANK 3"), for an empty slot, or null.</param>
public sealed record SlotView(string IconId, string Key, string Name, int FocusCost,
    float CooldownLeft, float CooldownTotal, float GcdLeft, float GcdTotal,
    bool Usable, string WhyNot, bool Affordable, bool InRange, float Pressed, float Armed,
    string Count = null, string Detail = null, string Text = null, string Locked = null)
{
    /// <summary>Whether its own cooldown is running.</summary>
    public bool Cooling => CooldownLeft > 0f && CooldownTotal > 0f;

    /// <summary>Whether the global cooldown's sweep shows over it: it's running and outlasts the slot's own.</summary>
    public bool Global => GcdLeft > 0f && GcdTotal > 0f && GcdLeft > CooldownLeft;
}

/// <summary>The Soul-Link sigil on slot 7 (11-combat.md §5.3).</summary>
/// <param name="Charge">Its charges, 0 to Full.</param>
/// <param name="Full">How many charges light it, 2.</param>
/// <param name="Broken">Whether the bond's broken or the two of you aren't both in the fight: grey, and dead.</param>
/// <param name="Strained">Whether one of the two of you is down: violet.</param>
/// <param name="Pressed">How lit its press flash is, 0 to 1.</param>
/// <param name="Key">Its key, "7".</param>
/// <param name="Text">What it does, for its card, or null.</param>
public sealed record SoulLinkView(int Charge, int Full, bool Broken, bool Strained, float Pressed, string Key = "7", string Text = null)
{
    /// <summary>Whether it's lit and can be spent.</summary>
    public bool Ready => !Broken && !Strained && Charge >= Full;
}

/// <summary>A boss as its wide frame draws it: who it is, and how far through its phases.</summary>
/// <param name="Fighter">The boss itself.</param>
/// <param name="Title">What it's called after its name, smaller ("Mother of the Quarry"), or null.</param>
/// <param name="Phase">Which phase it's in, from 1.</param>
/// <param name="Thresholds">The health shares where each phase after the first starts, highest first (0.66, 0.33): ticks on the bar and a pip each.</param>
/// <param name="PhaseName">The phase's name, after the pips ("ENRAGED"), or null.</param>
public sealed record BossView(FighterHudView Fighter, string Title, int Phase, IReadOnlyList<float> Thresholds, string PhaseName = null)
{
    /// <summary>How many phases it has.</summary>
    public int Phases => (Thresholds?.Count ?? 0) + 1;
}

/// <summary>Someone's nameplate over their head, in screen space.</summary>
/// <param name="Id">Who.</param>
/// <param name="Name">What they're called.</param>
/// <param name="Head">The top of their head on the screen, in screen pixels: the plate sits above it.</param>
/// <param name="Feet">Where their feet are on the screen, for the target's ring.</param>
/// <param name="Width">How wide they are on the screen, for the target's ring.</param>
/// <param name="HealthShare">Their health, 0 to 1.</param>
/// <param name="Foe">Whether they're a foe: a red bar and a soft red name.</param>
/// <param name="IsTarget">Whether they're the leader's target: brighter, bracketed, with the marker over them.</param>
/// <param name="Telegraph">What they're winding up, or null.</param>
/// <param name="IsDown">Whether they're Downed: the bar greys and says DOWN.</param>
/// <param name="Numbers">Their hit points as written while Alt's held ("22 / 30"), or null.</param>
/// <param name="Hostile">Whether a foe not yet in the fight can be clicked to start it: the name is a brighter red.</param>
public sealed record NameplateView(string Id, string Name, Vector2 Head, Vector2 Feet, float Width, float HealthShare, bool Foe,
    bool IsTarget, TelegraphView Telegraph, bool IsDown = false, string Numbers = null, bool Hostile = false);

/// <summary>Everything the real-time HUD shows in a frame, for CombatHud to draw in one go.</summary>
/// <param name="Player">You: your frame, and your cast and swing over the bar.</param>
/// <param name="Party">Everyone else on your side, a row each under your frame, in order.</param>
/// <param name="Target">Your target's frame, or null for none. With a boss up it sits under the boss's frame.</param>
/// <param name="Boss">The boss's wide frame, or null for none.</param>
/// <param name="Slots">The hotbar's six slots, keys 1 to 6.</param>
/// <param name="Sigil">The Soul-Link sigil on 7, or null to leave the slot empty.</param>
/// <param name="Plates">Everyone's nameplate over their head, back to front; the one with IsTarget gets the ring and the marker.</param>
public sealed record HudView(FighterHudView Player, IReadOnlyList<FighterHudView> Party, FighterHudView Target, BossView Boss,
    IReadOnlyList<SlotView> Slots, SoulLinkView Sigil, IReadOnlyList<NameplateView> Plates);
