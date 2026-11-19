using System.Globalization;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components;

namespace LegendQuest.Screens;

/// <summary>
/// The card for whoever the mouse is on in the room, in a fight: who they are, their health and armour,
/// what they're winding up, who they're after, and what a click on them does.
/// </summary>
/// <remarks>
/// The HUD draws its own cards (a slot's, the sigil's, an aura's: CombatHud.DrawWords); this one is
/// for the people in the room, drawn by the mouse. Every number says what it is, and the last line,
/// in torchlight, is what a click does.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>Draws the card for whoever the mouse is on in the room, if anyone. Goes in the linear sampled screen batch.</summary>
    private void DrawFighterCard(SpriteBatch spriteBatch, float alpha)
    {
        if (_hoverFighter is not Fighter fighter || _proofAutoplay && _proofAim is null) return;
        Fighter leader = _battle.Leader;

        _card.Clear();
        _card.Line(fighter.Name.ToUpperInvariant(), fighter.Side == Side.Foes ? CombatStyle.FoeLine : Palette.Gold);
        _card.Line(LineOf(fighter), Palette.Note);
        _card.Line(string.Format(CultureInfo.InvariantCulture, "{0} / {1} HP  ·  AC {2}", fighter.Hp, fighter.MaxHp, fighter.BaseAc), Palette.Bone);
        if (fighter.TempHp > 0) _card.Line($"+{fighter.TempHp} SHIELD", Palette.Bone);

        if (fighter.Casting is CastView cast)
            _card.Line((cast.Telegraph ? "WINDING UP " : "CASTING ") + cast.Name.ToUpperInvariant(), CombatStyle.Threat);
        if (fighter.IsDown) _card.Line($"DOWN  ·  SAVES {fighter.DeathSuccesses} MADE, {fighter.DeathFailures} FAILED", Palette.Ember);
        if (fighter.Side == Side.Foes && fighter.Target is Fighter after) _card.Line("AFTER " + (after == leader ? "YOU" : after.Name.ToUpperInvariant()), Palette.Note);
        if (fighter.Auras.Count > 0) _card.Line(fighter.Auras.Count == 1 ? "1 AURA ON THEM" : $"{fighter.Auras.Count} AURAS ON THEM", Palette.Note);

        if (leader is not null && fighter != leader)
        {
            _card.Gap();
            string does = fighter.Side == Side.Foes ? "CLICK  ·  TARGET AND ATTACK"
                : fighter.IsDown ? "CLICK  ·  TARGET, AND HELP THEM UP"
                : "CLICK  ·  TARGET, FOR HEALS AND HELP";
            _card.Line(does, Palette.Torch);
        }
        _card.DrawByMouse(spriteBatch, _mouse, alpha);
    }
}
