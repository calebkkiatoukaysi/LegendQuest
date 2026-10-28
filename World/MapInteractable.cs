using Microsoft.Xna.Framework;

namespace LegendQuest.World;

/// <summary>
/// Something on a map you walk up to and use: a chest, a door, a lever, a sign or a waypoint stone.
/// </summary>
/// <remarks>
/// Data only, like the rest of a MapDefinition. Whether it's open, pulled or found lives in the
/// save, and WorldState is what reads and changes that. Only the settings its kind uses are set;
/// the loader refuses the rest.
/// </remarks>
public sealed class MapInteractable
{
    /// <summary>How close your feet have to be to its foot to use it, in art pixels. A tile and a half.</summary>
    private const float UseReach = 24f;

    /// <summary>How close you have to be to a door. Further, since a door's two tiles wide and its foot is in the middle.</summary>
    private const float DoorReach = 30f;

    /// <summary>How close you have to be to read a sign. You can read one from further off than you can open a chest.</summary>
    private const float ReadReach = 36f;

    /// <summary>Where you stand by a waypoint when you arrive at it: a tile to its right, and a little in front so you're drawn over it.</summary>
    private static readonly Vector2 BesideOffset = new(Tilemap.TileSize, 2f);

    /// <summary>What kind of thing it is.</summary>
    public required InteractableKind Kind { get; init; }

    /// <summary>What the map file calls it. Saves keep track of it by this and the map's id.</summary>
    public required string Id { get; init; }

    /// <summary>The middle of its bottom edge, in world pixels, same as a prop's.</summary>
    public required Vector2 Foot { get; init; }

    /// <summary>What it looks like. Anything but a sign has two pictures: shut and open, or up and down.</summary>
    public required PropDefinition Look { get; init; }

    /// <summary>When it's there.</summary>
    public MapPresence Presence { get; init; }

    /// <summary>What it's called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>A chest: how much gold is in it.</summary>
    /// <remarks>Gold only until there are items. Then a chest gets a list of them too.</remarks>
    public int Gold { get; init; }

    /// <summary>A door: whether it's locked. It stays that way until there are keys and checks to open it with.</summary>
    public bool Locked { get; init; }

    /// <summary>A lever: the flag it sets and clears. A door: the flag that opens it, which means it won't open by hand.</summary>
    public string Flag { get; init; }

    /// <summary>A sign: what it says.</summary>
    public string Text { get; init; }

    /// <summary>How close your feet have to be to its foot to use it, in art pixels.</summary>
    public float Reach => Kind switch
    {
        InteractableKind.Sign => ReadReach,
        InteractableKind.Door => DoorReach,
        _ => UseReach,
    };

    /// <summary>The word for using it, shown next to its name when you're close enough.</summary>
    public string Prompt => Kind switch
    {
        InteractableKind.Lever => "PULL",
        InteractableKind.Sign => "READ",
        InteractableKind.Waypoint => "TRAVEL",
        _ => "OPEN",
    };

    /// <summary>Where fast travel puts you: beside it, not in front, where you'd be standing in the way of it.</summary>
    public Vector2 Beside => Foot + BesideOffset;
}
