using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ItemActionRules"/>: the actions of the sack context menu.
/// </summary>
public class ItemActionRulesTests
{
	private const ItemActions GroupActions = ItemActions.Delete | ItemActions.MoveTo;

	private static Item Sword(bool socketed1 = false, bool socketed2 = false) => new()
	{
		BaseItemId = RecordId.Create(@"records\xpack\items\weapons\swords\u_e_01.dbr"),
		relicID = socketed1 ? RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId() : RecordId.Empty,
		relic2ID = socketed2 ? RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId() : RecordId.Empty,
	};

	private static Item Potions(int count)
	{
		var potion = new Item { BaseItemId = RecordId.Create(@"records\item\oneshot\potionhealth01.dbr") };
		potion.Number = count;
		return potion;
	}

	private static ItemActions For(Item? item, int selectionCount = 0, bool itemIsTheSelection = false
		, bool readOnly = false, bool allowEdit = false, bool allowCopy = false)
		=> ItemActionRules.Available(new ItemActionContext(item, selectionCount, itemIsTheSelection, readOnly, allowEdit, allowCopy));

	[Fact]
	public void Available_NothingUnderCursorNorSelected_OffersNothing()
		=> For(null).Should().Be(ItemActions.None);

	[Fact]
	public void Available_SingleItem_OffersItsActionsAndTheEnabledOptions()
	{
		For(Sword()).Should().Be(GroupActions | ItemActions.Properties);
		For(Sword(), allowCopy: true).Should().Be(GroupActions | ItemActions.Properties | ItemActions.Copy | ItemActions.Duplicate);
		For(Sword(), allowEdit: true).Should().Be(GroupActions | ItemActions.Properties | ItemActions.Edit);
	}

	[Fact]
	public void Available_ReadOnlySack_OnlyLooksAndCopies()
		=> For(Sword(socketed1: true), readOnly: true, allowEdit: true, allowCopy: true)
			.Should().Be(ItemActions.Properties | ItemActions.Copy | ItemActions.Duplicate);

	[Fact]
	public void Available_SeveralSelected_OffersOnlyTheGroupActions()
		=> For(Sword(), selectionCount: 3, allowEdit: true, allowCopy: true)
			.Should().Be(GroupActions | ItemActions.ClearSelection);

	[Fact]
	public void Available_SelectionOfTheItemAlone_ActsOnTheItem()
		=> For(Sword(), selectionCount: 1, itemIsTheSelection: true)
			.Should().Be(GroupActions | ItemActions.Properties | ItemActions.ClearSelection);

	[Fact]
	public void Available_SelectionWithoutItemUnderCursor_OffersTheGroupActions()
		=> For(null, selectionCount: 2).Should().Be(GroupActions | ItemActions.ClearSelection);

	[Theory]
	[InlineData(true, false, ItemActions.RemoveRelic)]
	[InlineData(false, true, ItemActions.RemoveRelic2)]
	[InlineData(true, true, ItemActions.RemoveRelic | ItemActions.RemoveRelic2)]
	public void Available_SocketedItem_RemovesEachFilledSlotWhenEditing(bool slot1, bool slot2, ItemActions expected)
	{
		const ItemActions relics = ItemActions.RemoveRelic | ItemActions.RemoveRelic2;

		(For(Sword(slot1, slot2), allowEdit: true) & relics).Should().Be(expected);
		(For(Sword(slot1, slot2)) & relics).Should().Be(ItemActions.None, "removing a relic edits the item");
	}

	[Theory]
	[InlineData(2, true)]
	[InlineData(1, false)]
	public void Available_Stack_SplitsOnlyAStackOfSeveral(int count, bool canSplit)
		=> For(Potions(count)).HasFlag(ItemActions.Split).Should().Be(canSplit);
}
