namespace LegendQuest.World;

/// <summary>
/// A room's tile strip: the ground under its floor band (and an interior's back wall), as tile ids
/// in its tileset, with whatever lies on top of them.
/// </summary>
/// <remarks>Indexed [x, y] like a map's layers, so Tilemap can draw it the same way. Ids count from 1, and 0 in Detail is nothing.</remarks>
/// <param name="Tileset">The tileset the ids are in.</param>
/// <param name="Ground">The ground tiles, every cell one.</param>
/// <param name="Detail">What lies on the ground, 0 where nothing does.</param>
public sealed record TileLayers(TilesetDefinition Tileset, int[,] Ground, int[,] Detail);
