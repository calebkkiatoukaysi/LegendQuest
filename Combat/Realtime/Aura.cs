namespace LegendQuest.Combat.Realtime;

/// <summary>
/// An aura on someone in a fight: what it is, who put it there, how long it has left, its stacks,
/// and what's left of a shield or its uses.
/// </summary>
/// <remarks>The engine changes it; the screen only reads it, for the aura rows (icon, timer, stacks, border by Beneficial).</remarks>
public sealed class Aura
{
    /// <summary>Puts an aura together; Battle.ApplyAura is what puts it on someone.</summary>
    internal Aura(AuraDefinition definition, Fighter source, SkillDefinition skill, int startMs, int dc)
    {
        Definition = definition;
        Source = source;
        Skill = skill;
        StartMs = startMs;
        Dc = dc;
        Stacks = 1;
        Charges = definition.Charges;
    }

    /// <summary>What it is.</summary>
    public AuraDefinition Definition { get; }

    /// <summary>Its id.</summary>
    public string Id => Definition.Id;

    /// <summary>What it's called.</summary>
    public string Name => Definition.Name;

    /// <summary>Whether it helps its bearer.</summary>
    public bool Beneficial => Definition.Beneficial;

    /// <summary>Who put it there, or null (a support, the room).</summary>
    public Fighter Source { get; internal set; }

    /// <summary>Whoever put it there's id, or null.</summary>
    public string SourceId => Source?.Id;

    /// <summary>The skill that put it there, or null.</summary>
    public SkillDefinition Skill { get; }

    /// <summary>When it went on (or was last refreshed), in fight time.</summary>
    public int StartMs { get; internal set; }

    /// <summary>When it runs out, in fight time, or 0 if it doesn't.</summary>
    public int EndsAtMs { get; internal set; }

    /// <summary>How long it lasts in all, from its last refresh; 0 if it doesn't run out.</summary>
    public int TotalMs => EndsAtMs == 0 ? 0 : EndsAtMs - StartMs;

    /// <summary>How long it has left at a moment.</summary>
    /// <param name="now">The fight's time.</param>
    public int RemainingMs(int now) => EndsAtMs == 0 ? 0 : System.Math.Max(0, EndsAtMs - now);

    /// <summary>How many stacks it has.</summary>
    public int Stacks { get; internal set; }

    /// <summary>A shield's temporary hit points left.</summary>
    public int Amount { get; internal set; }

    /// <summary>Uses left before it's gone (Mirror Image's images).</summary>
    public int Charges { get; internal set; }

    /// <summary>The DC its saves are made against.</summary>
    public int Dc { get; }

    /// <summary>When it next ticks, in fight time.</summary>
    internal int NextTickMs { get; set; }

    /// <summary>Whether it's one of its source's concentration auras.</summary>
    internal bool Concentration { get; set; }

    /// <summary>Whether it's come off, so a loop over the auras can skip it before they're swept.</summary>
    internal bool Gone { get; set; }
}
