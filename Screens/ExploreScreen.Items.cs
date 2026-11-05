using System;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.StateManagement;
using LegendQuest.Tutorial;
using LegendQuest.World;

namespace LegendQuest.Screens;

/// <summary>
/// The zone screen's part of the party's things: I and C for the inventory and the character sheet,
/// a shopkeeper's shop once the talking's done, the line that says what's gone in the bag, and
/// dressing the party again when what they're wearing changes.
/// </summary>
/// <remarks>
/// The inventory and the shop are popups over the room, like the zone map, so the room stays drawn
/// and paused under them. The inventory says whenever anyone's gear changes, and the figures are
/// baked again then and there, so the party behind the veil is already wearing it when it closes.
/// A conversation that opens a shop only says which; the shop opens when the conversation's over,
/// the same way a conversation's travel waits for it to end.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>What goes in front of the names of whatever's come out of a chest.</summary>
    private const string IntoTheBag = "Into the bag  ·  ";

    private readonly InputAction _openBag = new(Array.Empty<Buttons>(), new[] { Keys.I }, true);
    private readonly InputAction _openSheet = new(Array.Empty<Buttons>(), new[] { Keys.C }, true);

    /// <summary>The shop the conversation that's up has asked to open when it's over (OnDialogue notes it), or null.</summary>
    private string _shopAfterTalk;

    /// <summary>I for the bag, C for the character sheet.</summary>
    /// <param name="input">The input this frame.</param>
    /// <returns>True if one of them was opened.</returns>
    private bool HandleInventoryInput(InputState input)
    {
        if (_openBag.Occurred(input)) OpenInventory(InventoryPage.Bag);
        else if (_openSheet.Occurred(input)) OpenInventory(InventoryPage.Character);
        else return false;
        return true;
    }

    /// <summary>Opens the inventory over the room, which is the tip about opening it done.</summary>
    /// <param name="page">The page to open on.</param>
    private void OpenInventory(InventoryPage page)
    {
        Halt();
        TutorialDone(TutorialCatalog.InventoryOpen);

        var inventory = new InventoryScreen(_game, page);
        inventory.Changed += Redress;
        ScreenManager.AddScreen(inventory);
    }

    /// <summary>Dresses every figure in the party again in what its owner's wearing now.</summary>
    private void Redress()
    {
        for (int i = 0; i < _walkers.Count; i++)
            _walkers[i].Sheet = SheetFor(_members[i].Look, Paperdoll.GearOf(_members[i]));
    }

    /// <summary>Opens the shop the conversation that's just ended asked for, if it asked for one. Buying anything there is the moment for the tip about the bag.</summary>
    private void OpenAwaitedShop()
    {
        if (_shopAfterTalk is not string id) return;
        _shopAfterTalk = null;

        Halt();
        var shop = new ShopScreen(ShopCatalog.Get(id), _game);
        shop.Closed += bought =>
        {
            if (bought) ShowTutorial(TutorialCatalog.InventoryOpen);
        };
        ScreenManager.AddScreen(shop);
    }

    /// <summary>Puts up the line for something that happened in the room. Things out of a chest say they've gone in the bag, and the first time it happens the tip about loot comes (or, after that, the one about opening the bag, if it's still to come).</summary>
    /// <param name="happened">What happened.</param>
    private void Announce(WorldEvent happened)
    {
        if (happened.Text is null) return;
        if (happened.Kind != WorldEventKind.ItemsTaken)
        {
            _toast.Show(happened.Text);
            return;
        }

        _toast.Show(IntoTheBag + happened.Text);
        if (!ShowTutorial(TutorialCatalog.Loot)) ShowTutorial(TutorialCatalog.InventoryOpen);
    }
}
