using System;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// Keys for testing, in debug builds only: F9 puts whoever you're controlling in the next class (all
/// twenty-five, the advanced ones too, in ClassId's order), dressed in its kit, and F10 gives them the
/// class points for its next rank, so a class's skills can be tried in a fight without playing to them.
/// </summary>
/// <remarks>
/// A release build has neither: HandleDebugInput is then a method that does nothing. What they do is
/// saved like anything else, so they're for a scratch game, not one being played.
/// </remarks>
public partial class ExploreScreen
{
#if DEBUG
    private readonly InputAction _debugClass = new(Array.Empty<Buttons>(), new[] { Keys.F9 }, true);
    private readonly InputAction _debugRank = new(Array.Empty<Buttons>(), new[] { Keys.F10 }, true);

    /// <summary>F9 and F10: the next class, or the next rank in this one, for whoever you're controlling, with a line to say so.</summary>
    /// <param name="input">The input this frame.</param>
    /// <returns>True if one was pressed.</returns>
    private bool HandleDebugInput(InputState input)
    {
        PartyMember member = _game.LeaderMember;
        if (_debugClass.Occurred(input))
        {
            ClassId next = (ClassId)(((int)member.Class + 1) % Enum.GetValues<ClassId>().Length);
            member.Class = next;
            member.Learn(next);
            member.Equipment.Clear();
            member.PutOn(Origins.KitFor(member.Id, next));
            member.Hp = member.MaxHp;
            _walkers[_game.Leader].Sheet = SheetFor(member.Look, Paperdoll.GearOf(member));
            _toast.Show($"DEBUG: {member.Name} is a {Rules.ClassCatalog.Get(next).Name}, rank {member.Rank}");
            return true;
        }
        if (_debugRank.Occurred(input) && member.Rank < Combat.ClassRanks.MaxRank)
        {
            member.AddClassPoints(Combat.ClassRanks.PointsFor(member.Rank + 1) - member.PointsIn(member.Class));
            _toast.Show($"DEBUG: {member.Name} is {Rules.ClassCatalog.Get(member.Class).Name} rank {member.Rank}");
            return true;
        }
        return false;
    }
#else
    /// <summary>Nothing, in a release build.</summary>
    private bool HandleDebugInput(InputState input) => false;
#endif
}
