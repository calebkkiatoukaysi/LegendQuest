using System;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using LegendQuest.Rules;

namespace LegendQuest.Characters;

/// <summary>
/// Someone in the party: who they are, what they are, how hurt they are, how they look, and where
/// they're standing.
/// </summary>
/// <remarks>
/// Plain data that goes in the save as it is. The sprite that walks them round a map is separate,
/// and gets made from this when the map loads. Equipment, resources, approval and conditions get
/// their fields when the systems that own them exist.
/// </remarks>
public class PartyMember
{
    /// <summary>Who they are, for the game rather than the screen: you, or one of the companions in Origins.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>What they're called on screen.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Their class.</summary>
    public ClassId Class { get; set; }

    /// <summary>Their level. Everyone's level 1 in the skeleton; levelling up comes later.</summary>
    public int Level { get; set; } = 1;

    /// <summary>Their six ability scores.</summary>
    public AbilityScores Scores { get; set; } = new();

    /// <summary>The hit points they have now.</summary>
    public int Hp { get; set; }

    /// <summary>How they look.</summary>
    public CharacterLook Look { get; set; } = new();

    /// <summary>Which pronouns the lines use for them. Only the Soul-Bound's are ever read.</summary>
    public Pronouns Pronouns { get; set; }

    /// <summary>How far across the map their feet are, in world pixels.</summary>
    public float X { get; set; }

    /// <summary>How far down the map their feet are, in world pixels.</summary>
    public float Y { get; set; }

    /// <summary>Which way they're facing.</summary>
    public Direction Facing { get; set; }

    /// <summary>The most hit points they can have.</summary>
    /// <remarks>5e at level 1: the class's whole hit die plus the Constitution modifier, and never less than 1.</remarks>
    [JsonIgnore]
    public int MaxHp => Math.Max(1, ClassCatalog.Get(Class).HitDie + Scores.Modifier(Ability.Constitution));

    /// <summary>X and Y together.</summary>
    [JsonIgnore]
    public Vector2 Feet
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }
}
