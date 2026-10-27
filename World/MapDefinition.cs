using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// One map, as read out of its .map file: what it's called, its tiles in three layers, how it's lit,
/// what plays on it, and everything placed on it.
/// </summary>
/// <remarks>
/// Data only. MapLoader builds these and checks them, Tilemap draws one, and nothing changes one
/// after it's loaded. Tile ids count from 1 and 0 is an empty cell, same as the tilemap exercise.
/// </remarks>
public sealed class MapDefinition
{
    /// <summary>What saves and other maps call it. Has to match the file name.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>The tileset every layer is drawn from.</summary>
    public required TilesetDefinition Tileset { get; init; }

    /// <summary>Whether it's dark, which means the darkness is laid over it with the lights cut out of it.</summary>
    public required bool Dark { get; init; }

    /// <summary>How much light gets everywhere on a dark map, 0 (none) to 1 (as if it weren't dark).</summary>
    public required float Ambient { get; init; }

    /// <summary>The song that plays on it.</summary>
    public required Track Music { get; init; }

    /// <summary>How many tiles across.</summary>
    public required int Width { get; init; }

    /// <summary>How many tiles down.</summary>
    public required int Height { get; init; }

    /// <summary>The floor. Every cell has a tile.</summary>
    public required int[,] Ground { get; init; }

    /// <summary>Things on the floor: fences, flowers, rubble. Mostly empty.</summary>
    public required int[,] Detail { get; init; }

    /// <summary>Things over everyone's heads: roofs, mostly. Drawn in front of the people and never solid.</summary>
    public required int[,] Overhead { get; init; }

    /// <summary>Where the party can arrive, by name. Every map has one called start.</summary>
    public required IReadOnlyDictionary<string, Vector2> Spawns { get; init; }

    /// <summary>Every prop on it.</summary>
    public required IReadOnlyList<MapProp> Props { get; init; }
}
