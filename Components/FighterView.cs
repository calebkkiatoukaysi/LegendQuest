using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using LegendQuest.Characters;

namespace LegendQuest.Components;

/// <summary>
/// Someone in a fight the way the screen's showing them: what the turn order, the frame of whoever's
/// turn it is and the bars over their heads draw.
/// </summary>
/// <remarks>
/// Made fresh each frame by the explore screen from what it's shown so far (ShownFight), so the
/// components draw the fight as played, not as the engine already knows it ends.
/// </remarks>
/// <param name="Id">Who, by their id in the fight.</param>
/// <param name="Name">What they're called.</param>
/// <param name="Sheet">The sheet their face is cut from.</param>
/// <param name="Shape">The shape of that sheet's frames.</param>
/// <param name="Foe">Whether they're on the other side.</param>
/// <param name="Hp">Their hit points as shown.</param>
/// <param name="MaxHp">The most they can have.</param>
/// <param name="ArmourClass">Their armour class.</param>
/// <param name="Focus">Their Focus as shown, 0 to Combatant.MaxFocus.</param>
/// <param name="Conditions">What's on them as shown.</param>
/// <param name="Down">Whether they're Downed (or knocked out, in a fight to yield).</param>
/// <param name="Fallen">Whether they've died, or for one of the party, fallen and out of the fight.</param>
/// <param name="DeathSuccesses">Death saves made while Downed.</param>
/// <param name="DeathFailures">Death saves failed while Downed.</param>
public sealed record FighterView(string Id, string Name, Texture2D Sheet, WalkerShape Shape, bool Foe, int Hp, int MaxHp, int ArmourClass, int Focus,
    IReadOnlyList<ConditionId> Conditions, bool Down, bool Fallen, int DeathSuccesses, int DeathFailures);
