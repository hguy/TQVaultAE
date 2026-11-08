using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>
/// Items to send to another panel or bag, in the order they are moved.
/// </summary>
/// <param name="Items">The items, the largest first.</param>
/// <param name="Destination">The destination panel.</param>
/// <param name="DestIndex">The destination bag, or -1 to let the panel find one.</param>
public sealed record AutoMovePlan(IReadOnlyList<Item> Items, AutoMoveLocation Destination, int DestIndex);

/// <summary>
/// Plans the moves of the "Move to" menu, of the quick move keys and of a bag merge.
/// </summary>
/// <remarks>
/// The panels move the items one by one, each through a drag; an item that cannot be placed stays in the drag and
/// stops the moves after it. Moving the largest items first leaves the small ones for the gaps.
/// </remarks>
public static class AutoMovePlanner
{
	/// <summary>
	/// Orders items for a move: the largest first (height counts more than width), then by item group.
	/// </summary>
	/// <param name="items">The items; <see langword="null"/> entries are skipped.</param>
	public static IReadOnlyList<Item> Order(IEnumerable<Item?> items)
		=> items
			.OfType<Item>()
			.OrderByDescending(item => (((item.Height * 3) + item.Width) * 100) + item.ItemGroup)
			.ToList();

	/// <summary>
	/// Plans the move of the selection, or of the item under the cursor when nothing is selected.
	/// </summary>
	/// <param name="selection">The selected items, or <see langword="null"/> when there is no selection.</param>
	/// <param name="focusedItem">The item under the cursor.</param>
	/// <param name="destination">The destination panel.</param>
	/// <param name="destIndex">The destination bag, or -1.</param>
	public static AutoMovePlan Plan(IEnumerable<Item?>? selection, Item? focusedItem, AutoMoveLocation destination, int destIndex = -1)
	{
		IReadOnlyList<Item> items = selection is not null
			? Order(selection)
			: focusedItem is not null ? [focusedItem] : [];

		return new AutoMovePlan(items, destination, destIndex);
	}

	/// <summary>
	/// Where a quick move key sends the items of a panel.
	/// </summary>
	/// <param name="requested">The panel of the key.</param>
	/// <param name="here">The panel of the items.</param>
	/// <param name="secondaryVaultShown">The secondary vault replaces the player and stash panels.</param>
	/// <returns>The destination, or <see cref="AutoMoveLocation.NotSet"/> when the key moves nothing.</returns>
	public static AutoMoveLocation QuickMoveDestination(AutoMoveLocation requested, AutoMoveLocation here, bool secondaryVaultShown)
	{
		if (requested == here)
			return AutoMoveLocation.NotSet;

		// The player key reaches the secondary vault shown in place of the player.
		var destination = secondaryVaultShown && requested == AutoMoveLocation.Player
			? AutoMoveLocation.SecondaryVault
			: requested;

		if (secondaryVaultShown && destination is AutoMoveLocation.Stash or AutoMoveLocation.Player)
			return AutoMoveLocation.NotSet;

		return destination;
	}
}
