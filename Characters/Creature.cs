namespace LegendQuest.Characters;

/// <summary>
/// One kind of creature: what it's called on screen, and the shape of the frames on its sheet.
/// </summary>
/// <param name="Id">What maps call it with creature=, and its sheet's name.</param>
/// <param name="Name">What it's called on screen, unless the map gives the one it puts down a name= of its own.</param>
/// <param name="Shape">How big its frames are, where its feet are in them, and the rest.</param>
public sealed record Creature(string Id, string Name, WalkerShape Shape)
{
    /// <summary>Its sheet, by content name.</summary>
    public string Texture => "Creatures/" + Id;
}
