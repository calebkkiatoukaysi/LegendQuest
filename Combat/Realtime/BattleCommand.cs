using System.Numerics;

namespace LegendQuest.Combat.Realtime;

/// <summary>
/// Something a fighter is told to do: by the player for the leader, or by the AI for everyone else.
/// </summary>
/// <remarks>
/// Commands are the only way into a fight. Battle.Command queues one, and the next Step applies it at
/// the tick boundary, before anything else that tick, so the game and the simulator see the same
/// fight from the same commands. A Use on a target out of reach walks the fighter into reach first,
/// then uses it; one that can't be done at all (no Focus, cooling down) is refused with a Denied
/// event saying why.
/// </remarks>
/// <param name="Kind">What to do.</param>
/// <param name="ActorId">Who's to do it, by fighter id.</param>
/// <param name="TargetId">At whom, by fighter id, or null.</param>
/// <param name="SkillId">Which skill, for Use.</param>
/// <param name="Point">Where, for MoveTo.</param>
/// <param name="On">On or off, for AutoAttack.</param>
public sealed record BattleCommand(CommandKind Kind, string ActorId, string TargetId = null, string SkillId = null, Vector2 Point = default, bool On = true)
{
    /// <summary>Pick a target (a foe to fight, an ally to heal), or clear it with null. A foe targeted turns the auto attack on.</summary>
    public static BattleCommand Target(string actor, string target) => new(CommandKind.Target, actor, target);

    /// <summary>Walk to a point on the floor. Moving cancels a cast with a cast time.</summary>
    public static BattleCommand MoveTo(string actor, Vector2 point) => new(CommandKind.MoveTo, actor, Point: point);

    /// <summary>Use a skill on someone (or null for the skill's own choice: the target, the nearest foe, the most hurt ally, yourself).</summary>
    public static BattleCommand Use(string actor, string skill, string target = null) => new(CommandKind.Use, actor, target, skill);

    /// <summary>Drink the bag's best healing potion.</summary>
    public static BattleCommand Potion(string actor) => new(CommandKind.Potion, actor);

    /// <summary>Spend the Soul-Link on the Soul-Link Strike against a foe (or the current target).</summary>
    public static BattleCommand SoulLink(string actor, string target = null) => new(CommandKind.SoulLink, actor, target);

    /// <summary>Turn the auto attack on or off.</summary>
    public static BattleCommand AutoAttack(string actor, bool on) => new(CommandKind.AutoAttack, actor, On: on);

    /// <summary>Help a Downed ally up: a 2 s channel standing next to them.</summary>
    public static BattleCommand Help(string actor, string target) => new(CommandKind.Help, actor, target);

    /// <summary>Stop what you're doing: the walk, the queued skill, a cast.</summary>
    public static BattleCommand Stop(string actor) => new(CommandKind.Stop, actor);

    /// <summary>Give the fight up.</summary>
    public static BattleCommand Concede() => new(CommandKind.Concede, null);
}

/// <summary>What a BattleCommand asks for.</summary>
public enum CommandKind
{
    /// <summary>Pick a target.</summary>
    Target,

    /// <summary>Walk somewhere.</summary>
    MoveTo,

    /// <summary>Use a skill.</summary>
    Use,

    /// <summary>Drink a potion.</summary>
    Potion,

    /// <summary>The Soul-Link Strike.</summary>
    SoulLink,

    /// <summary>The auto attack on or off.</summary>
    AutoAttack,

    /// <summary>Help a Downed ally up.</summary>
    Help,

    /// <summary>Stop.</summary>
    Stop,

    /// <summary>Give up.</summary>
    Concede,
}
