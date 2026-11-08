using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>Actions offered on the items of a sack, for example by its context menu.</summary>
[Flags]
public enum ItemActions
{
	None = 0,

	/// <summary>Delete the item under the cursor or the selection.</summary>
	Delete = 1 << 0,

	/// <summary>Take the relic or charm out of the first slot.</summary>
	RemoveRelic = 1 << 1,

	/// <summary>Take the relic or charm out of the second slot.</summary>
	RemoveRelic2 = 1 << 2,

	/// <summary>Split a stack in two.</summary>
	Split = 1 << 3,

	/// <summary>Copy the item to the drag cursor.</summary>
	Copy = 1 << 4,

	/// <summary>Put a copy of the item in the sack.</summary>
	Duplicate = 1 << 5,

	/// <summary>Move the item or the selection to another sack.</summary>
	MoveTo = 1 << 6,

	/// <summary>Edit the item: seed, affixes, completion bonuses, set pieces.</summary>
	Edit = 1 << 7,

	/// <summary>Show the item properties.</summary>
	Properties = 1 << 8,

	/// <summary>Clear the selection.</summary>
	ClearSelection = 1 << 9,

	/// <summary>Choose the item seed in a dialog. Part of <see cref="Edit"/>.</summary>
	Seed = 1 << 10,

	/// <summary>Give the item a new random seed. Part of <see cref="Edit"/>.</summary>
	ForceSeed = 1 << 11,

	/// <summary>Complete a relic or charm and roll its bonus. Part of <see cref="Edit"/>.</summary>
	CompleteRelic = 1 << 12,

	/// <summary>Craft the artifact of a formula. Part of <see cref="Edit"/>.</summary>
	CraftArtifact = 1 << 13,
}

/// <summary>
/// What the user points at and may do when asking for the actions of a sack.
/// </summary>
/// <param name="Item">Item under the cursor, or <see langword="null"/>.</param>
/// <param name="SelectionCount">Number of selected items.</param>
/// <param name="ItemIsTheSelection"><see langword="true"/> when the selection is <paramref name="Item"/> alone.</param>
/// <param name="ReadOnly">The sack cannot change, as the equipment of a read-only player.</param>
/// <param name="AllowItemEdit">The user enabled item editing.</param>
/// <param name="AllowItemCopy">The user enabled item copying.</param>
public readonly record struct ItemActionContext(
	Item? Item
	, int SelectionCount
	, bool ItemIsTheSelection
	, bool ReadOnly
	, bool AllowItemEdit
	, bool AllowItemCopy
);

/// <summary>
/// Which <see cref="ItemActions"/> apply: the decisions of the sack context menu, apart from its drawing.
/// </summary>
public static class ItemActionRules
{
	/// <summary>The actions available in <paramref name="context"/>.</summary>
	public static ItemActions Available(in ItemActionContext context)
	{
		var item = context.Item;
		bool hasSelection = context.SelectionCount > 0;

		// Something to act on: the item under the cursor, the selection, or both.
		bool anyTarget = item is not null || hasSelection;

		// The item alone is the target: no selection, or a selection of that item only.
		bool singleItem = item is not null && (!hasSelection || context.ItemIsTheSelection);

		var actions = ItemActions.None;

		if (anyTarget && !context.ReadOnly)
			actions |= ItemActions.Delete | ItemActions.MoveTo;

		if (singleItem)
		{
			actions |= ItemActions.Properties;

			if (context.AllowItemCopy)
				actions |= ItemActions.Copy | ItemActions.Duplicate;

			if (!context.ReadOnly)
			{
				if (item!.DoesStack && item.Number > 1)
					actions |= ItemActions.Split;

				if (context.AllowItemEdit)
				{
					actions |= ItemActions.Edit;

					if (item.HasSocketedSlot1)
						actions |= ItemActions.RemoveRelic;

					if (item.HasSocketedSlot2)
						actions |= ItemActions.RemoveRelic2;
				}
			}
		}

		if (hasSelection && !context.ReadOnly)
			actions |= ItemActions.ClearSelection;

		return actions;
	}

	/// <summary>
	/// Indicates whether <paramref name="item"/> can be equipped by a player of that game:
	/// a Titan Quest original player only takes the items of the base game.
	/// </summary>
	/// <param name="item">Item to check.</param>
	/// <param name="immortalThronePlayer"><see langword="true"/> for an Immortal Throne (or later) player.</param>
	/// <returns><see langword="true"/> when the item suits the player.</returns>
	public static bool IsSuitableFor(Item item, bool immortalThronePlayer)
		=> immortalThronePlayer || item.GameDlc == GameDlc.TitanQuest;

	/// <summary>
	/// The entries of <see cref="ItemActions.Edit"/> that do not need a catalog: the seed for any item, the completion of
	/// an incomplete relic or charm, the artifact of a formula.
	/// </summary>
	/// <param name="item">The item being edited.</param>
	public static ItemActions EditActions(Item item)
	{
		var actions = ItemActions.Seed | ItemActions.ForceSeed;

		if (item.IsRelicOrCharm && !item.IsRelicComplete)
			actions |= ItemActions.CompleteRelic;

		if (item.IsFormulae)
			actions |= ItemActions.CraftArtifact;

		return actions;
	}
}
