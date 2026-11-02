using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters;

/// <summary>
/// The creatures a map can put down with npc ... creature=(id): the beasts, the Hollowed and the
/// things that guard the vault, each with its name and the shape of its frames.
/// </summary>
/// <remarks>
/// Each one's sheet is Content/Creatures/(id).png, drawn by tools/generate_creatures.py and laid out
/// like everyone else's (ten columns, four rows, one for each way it faces), but in a frame of its own
/// size, with its feet in the middle of the bottom, two pixels up. The Hollowed were people, so they
/// have a person's shape. For now they stand, look round and stroll like the townsfolk; they fight
/// when there's fighting. Sizes are STORY.md's.
/// </remarks>
public static class Creatures
{
    /// <summary>How far up from the bottom of its frame a creature's feet are, in art pixels: the same as a person's, so the shadow has room under them.</summary>
    private const float FeetLift = 2f;

    private static readonly Creature[] Table =
    {
        // A Mirewood wolf with black smoke leaking from its eyes: long and low, side on.
        new("gloomfang", "Gloomfang Wolf",
            Standing(48, 32, figure: new(34f, 26f), shadow: new(30f, 6f), footprint: new(18f, 5f))),

        // The boss of the old quarry: a giant she-bear gone wrong, thorns through her fur and black
        // crystal growing out of her. She takes up most of her den.
        new("thornmother", "The Thornmother",
            Standing(96, 80, figure: new(76f, 72f), shadow: new(70f, 12f), footprint: new(52f, 12f))),

        // The Hollowed: Doomward's dead, ash-grey and empty-eyed, in a person's frame.
        new("hollowed-footman", "Hollowed Footman", WalkerShape.Person),
        new("hollowed-hexer", "Hollowed Hexer", WalkerShape.Person),
        new("hollowed-sapper", "Hollowed Sapper", WalkerShape.Person),

        // The vault's guardians, stone and gold, dormant by its doors.
        new("vault-construct", "Vault Construct",
            Standing(48, 64, figure: new(32f, 58f), shadow: new(30f, 7f), footprint: new(22f, 7f))),
    };

    private static readonly Dictionary<string, Creature> ById = new();

    /// <summary>Indexes the table and checks every row.</summary>
    /// <remarks>Throws on purpose, so a bad row shows up when the game starts.</remarks>
    static Creatures()
    {
        foreach (Creature creature in Table)
        {
            if (string.IsNullOrWhiteSpace(creature.Name))
                throw new InvalidOperationException($"Creature {creature.Id} has no name.");
            if (creature.Shape.Problem() is string problem)
                throw new InvalidOperationException($"Creature {creature.Id} {problem}.");
            if (!ById.TryAdd(creature.Id, creature))
                throw new InvalidOperationException($"Two creatures are called {creature.Id}.");
        }
    }

    /// <summary>Every creature's id, in the table's order, for the error message when a map names one that isn't.</summary>
    public static IEnumerable<string> Ids
    {
        get
        {
            foreach (Creature creature in Table) yield return creature.Id;
        }
    }

    /// <summary>A creature by id, or null if there's none called that.</summary>
    /// <param name="id">What the map file says.</param>
    public static Creature Get(string id) => ById.GetValueOrDefault(id ?? string.Empty);

    /// <summary>The shape of a creature's frames, with its feet in the middle of the bottom, FeetLift up.</summary>
    /// <param name="width">How wide a frame is, in art pixels.</param>
    /// <param name="height">How tall a frame is, in art pixels.</param>
    /// <param name="figure">The box round it for pointing at: how wide, and how tall up from its feet.</param>
    /// <param name="shadow">The shadow under it: how wide and how tall.</param>
    /// <param name="footprint">The floor it blocks: how wide, and how deep up from its feet.</param>
    private static WalkerShape Standing(int width, int height, Vector2 figure, Vector2 shadow, Vector2 footprint) =>
        new(new Point(width, height), new Vector2(width / 2f, height - FeetLift), figure, shadow, footprint);
}
