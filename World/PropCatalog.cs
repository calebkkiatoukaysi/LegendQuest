using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// Every prop a map can place, by the id map files use.
/// </summary>
/// <remarks>Each one's picture is Content/Props/(id).png, drawn by tools/generate_world.py.</remarks>
public static class PropCatalog
{
    /// <summary>How far a sconce's torch light reaches. Four tiles, so a hall wants one every eight or so.</summary>
    private const float TorchReach = 64f;

    /// <summary>How far a campfire's light reaches. Further than a torch, and it's out in the open.</summary>
    private const float FireReach = 88f;

    private static readonly PropDefinition[] Table =
    {
        new() { Id = "barrel", Texture = "Props/barrel", Footprint = new Point(12, 6) },
        new() { Id = "crate", Texture = "Props/crate", Footprint = new Point(14, 7) },
        new() { Id = "well", Texture = "Props/well", Footprint = new Point(28, 12) },
        new() { Id = "urn", Texture = "Props/urn", Footprint = new Point(8, 4) },
        new() { Id = "sarcophagus", Texture = "Props/sarcophagus", Footprint = new Point(30, 14) },
        new() { Id = "bedroll", Texture = "Props/bedroll" },
        new() { Id = "campfire", Texture = "Props/campfire", Footprint = new Point(16, 6),
            Light = new PropLight(new Vector2(0f, -4f), new Vector2(0f, -2f), FireReach, 1f) },

        // The ones that are something you can use. Each has its shut (or up, or asleep) picture first.
        new() { Id = "chest", Texture = "Props/chest", Frames = 2, Footprint = new Point(14, 6) },
        new() { Id = "door", Texture = "Props/door", Frames = 2, Footprint = new Point(32, 6) },
        new() { Id = "gate", Texture = "Props/gate", Frames = 2, Footprint = new Point(32, 6) },
        new() { Id = "lever", Texture = "Props/lever", Frames = 2, Placement = PropPlacement.Wall },
        new() { Id = "sign", Texture = "Props/sign", Footprint = new Point(4, 3) },
        new() { Id = "waypoint", Texture = "Props/waypoint", Frames = 2, Footprint = new Point(12, 5) },

        // The flame sits on the top of the handle, 14 art pixels up from where the sconce is fixed. The
        // sconce goes on the lower row of a wall's face, so the floor starts half a tile below it.
        new()
        {
            Id = "sconce", Texture = "Props/sconce", Placement = PropPlacement.Wall,
            Light = new PropLight(new Vector2(0f, -14f), new Vector2(0f, 14f), TorchReach, 1f),
        },
    };

    private static readonly Dictionary<string, PropDefinition> ById = new();

    /// <summary>Indexes the table and checks every row.</summary>
    /// <remarks>Throws on purpose, so a bad row shows up when the game starts.</remarks>
    static PropCatalog()
    {
        foreach (PropDefinition prop in Table)
        {
            if (prop.Footprint.X < 0 || prop.Footprint.Y < 0)
                throw new InvalidOperationException($"Prop {prop.Id} has a negative footprint.");
            if (prop.Frames < 1)
                throw new InvalidOperationException($"Prop {prop.Id} has {prop.Frames} pictures. It needs at least one.");
            if (prop.Light is { } light && (light.Reach <= 0f || light.Strength <= 0f))
                throw new InvalidOperationException($"Prop {prop.Id} gives off a light that reaches nowhere.");
            if (!ById.TryAdd(prop.Id, prop))
                throw new InvalidOperationException($"Two props are called {prop.Id}.");
        }
    }

    /// <summary>Looks a prop up by id.</summary>
    /// <param name="id">What map files call it.</param>
    /// <param name="prop">The prop, if there's one called that.</param>
    /// <returns>True if there is.</returns>
    public static bool TryGet(string id, out PropDefinition prop) => ById.TryGetValue(id ?? string.Empty, out prop);
}
