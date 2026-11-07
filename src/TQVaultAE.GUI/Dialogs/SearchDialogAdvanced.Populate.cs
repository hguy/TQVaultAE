/// <summary>
/// Class for the Search Dialog box.
/// </summary>

using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.Presentation;
using VisibilityItem = TQVaultAE.GUI.Models.Search.SearchQuery.VisibilityItem;

namespace TQVaultAE.GUI.Dialogs;

public partial class SearchDialogAdvanced
{
	private void AdjustCheckBoxesHeight()
	{
		var maxRow = (int)this.numericUpDownMaxElement.Value;
		foreach (var lb in this.CachedCheckedListBoxes)
		{
			// Adjust to current UI setting
			int height = 0, currrow = 0;
			foreach (var line in lb.Items)
			{
				if (currrow == maxRow) break;
				height += lb.GetItemHeight(currrow);
				currrow++;
			}
			height += SystemInformation.HorizontalScrollBarHeight;
			lb.Height = height;
		}
	}

	private void PopulateSetItems()
	{
		var clb = scalingCheckedListBoxSetItems;
		var setitems =
			from id in this.ItemDatabaseService.ItemDatabase
			let itm = id.LoadedFriendlyNames.Item
			let set = id.LoadedFriendlyNames.ItemSet
			where set is not null
			let setName = set.Translations[set.setName].RemoveAllTQTags()
			orderby setName
			group id by setName into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};

		PopulateInit(clb, setitems);
	}

	private void PopulateOrigin()
	{
		var clb = scalingCheckedListBoxOrigin;

		var originList = new ItemOrigin[] {
			new ItemOrigin{
				Value = GameDlc.TitanQuest,
				DisplayName = TranslationService.Translate(GameDlc.TitanQuest)
			},
			new ItemOrigin{
				Value = GameDlc.ImmortalThrone,
				DisplayName = TranslationService.Translate(GameDlc.ImmortalThrone)
			},
			new ItemOrigin{
				Value = GameDlc.Ragnarok,
				DisplayName = TranslationService.Translate(GameDlc.Ragnarok)
			},
			new ItemOrigin{
				Value = GameDlc.Atlantis,
				DisplayName = TranslationService.Translate(GameDlc.Atlantis)
			},
			new ItemOrigin{
				Value = GameDlc.EternalEmbers,
				DisplayName = TranslationService.Translate(GameDlc.EternalEmbers)
			},
		};

		var originItems =
			from id in this.ItemDatabaseService.ItemDatabase
			let itm = id.LoadedFriendlyNames.Item
			from org in originList
			where itm.GameDlc == org.Value
			orderby org.Value
			group id by org.DisplayName into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};

		PopulateInit(clb, originItems);
	}

	private void PopulateWithRelic()
	{
		var clb = scalingCheckedListBoxWithRelic;
		var WithRelic =
			from id in this.ItemDatabaseService.ItemDatabase
			let itm = id.LoadedFriendlyNames.Item
			where itm.HasRelicOrCharmSlot1 && !itm.IsRelic1Charm || itm.HasRelicOrCharmSlot2 && !itm.IsRelic2Charm
			from reldesc in new[] { id.LoadedFriendlyNames.RelicInfo1Description, id.LoadedFriendlyNames.RelicInfo2Description }
			where !string.IsNullOrWhiteSpace(reldesc)
			let attClean = reldesc.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, WithRelic);
	}

	private static void PopulateInit(VirtualCheckedListBox clb, IEnumerable<BoxItem> items)
	{
		var (_, tag) = clb.GetBoxTag();
		tag.DataSource = items.ToArray();
		clb.BeginUpdate();
		clb.Items.AddRange(tag.DataSource);
		clb.EndUpdate();
	}

	private void PopulateWithCharm()
	{
		var clb = scalingCheckedListBoxWithCharm;
		var WithCharm =
			from id in this.ItemDatabaseService.ItemDatabase
			let itm = id.LoadedFriendlyNames.Item
			where itm.HasRelicOrCharmSlot1 && itm.IsRelic1Charm || itm.HasRelicOrCharmSlot2 && itm.IsRelic2Charm
			from reldesc in new[] { id.LoadedFriendlyNames.RelicInfo1Description, id.LoadedFriendlyNames.RelicInfo2Description }
			where !string.IsNullOrWhiteSpace(reldesc)
			let attClean = reldesc.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, WithCharm);
	}

	private void PopulateVaults()
	{
		var clb = scalingCheckedListBoxVaults;
		var Vaults =
			from id in this.ItemDatabaseService.ItemDatabase.Where(i => i.SackType == SackType.Vault)
			let att = id.ContainerName
			orderby att
			group id by att into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, Vaults);
	}

	private void PopulateQuality()
	{
		var clb = scalingCheckedListBoxQuality;
		var Quality =
			from id in this.ItemDatabaseService.ItemDatabase
			let att = id.LoadedFriendlyNames.BaseItemInfoQuality
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, Quality);
	}

	private void PopulateSuffixName()
	{
		var clb = scalingCheckedListBoxSuffixName;
		var SuffixName =
			from id in this.ItemDatabaseService.ItemDatabase
			let att = id.LoadedFriendlyNames.SuffixInfoDescription
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, SuffixName);
	}

	private void PopulateSuffixAttributes()
	{
		var clb = scalingCheckedListBoxSuffixAttributes;
		var SuffixAttributes =
			from id in this.ItemDatabaseService.ItemDatabase
			from att in id.LoadedFriendlyNames.SuffixAttributes
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, SuffixAttributes);
	}

	private void PopulateStyle()
	{
		var clb = scalingCheckedListBoxStyle;
		var Style =
			from id in this.ItemDatabaseService.ItemDatabase
			let att = id.LoadedFriendlyNames.BaseItemInfoStyle
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, Style);
	}

	private void PopulateRarity()
	{
		var equipmentOnly = new[] {
			ItemStyle.Broken
			, ItemStyle.Mundane
			, ItemStyle.Common
			, ItemStyle.Rare
			, ItemStyle.Epic
			, ItemStyle.Legendary
		};
		var clb = scalingCheckedListBoxRarity;
		var Rarity =
			from id in this.ItemDatabaseService.ItemDatabase
			where equipmentOnly.Contains(id.ItemStyle)
			let att = id.LoadedFriendlyNames.BaseItemRarity
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, Rarity);
	}

	private void PopulatePrefixName()
	{
		var clb = scalingCheckedListBoxPrefixName;
		var PrefixName =
			from id in this.ItemDatabaseService.ItemDatabase
			let att = id.LoadedFriendlyNames.PrefixInfoDescription
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.TQCleanup().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, PrefixName);
	}

	private void PopulatePrefixAttributes()
	{
		var clb = scalingCheckedListBoxPrefixAttributes;
		var PrefixAttributes =
			from id in this.ItemDatabaseService.ItemDatabase
			from att in id.LoadedFriendlyNames.PrefixAttributes
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, PrefixAttributes);
	}

	private void PopulateCharacters()
	{
		var clb = scalingCheckedListBoxCharacters;
		var Players =
			from id in this.ItemDatabaseService.ItemDatabase
			where id.SackType == SackType.Player || id.SackType == SackType.Equipment
			let att = id.ContainerName
			orderby att
			group id by att into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, Players);
	}

	private void PopulateItemType()
	{
		var clb = scalingCheckedListBoxItemType;
		var ItemType =
			from id in this.ItemDatabaseService.ItemDatabase
			let att = id.LoadedFriendlyNames.BaseItemInfoClass ?? id.LoadedFriendlyNames.Item.ItemClass
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, ItemType);
	}

	private void PopulateItemAttributes()
	{
		var clb = scalingCheckedListBoxItemAttributes;
		var ItemAttributes =
			from id in this.ItemDatabaseService.ItemDatabase
			from att in id.LoadedFriendlyNames.AttributesAll
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, ItemAttributes);
	}

	private void PopulateBaseAttributes()
	{
		var clb = scalingCheckedListBoxBaseAttributes;
		var BaseAttributes =
			from id in this.ItemDatabaseService.ItemDatabase
			from att in id.LoadedFriendlyNames.BaseAttributes
			where !string.IsNullOrWhiteSpace(att)
			let attClean = att.RemoveAllTQTags().Trim()
			orderby attClean
			group id by attClean into grp
			select new BoxItem
			{
				DisplayValue = grp.Key,
				MatchingResults = grp,
				CheckedList = clb,
				Category = CategoryLabel(clb),
			};
		PopulateInit(clb, BaseAttributes);
	}
}
