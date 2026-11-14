/// <summary>
/// Class for the Search Dialog box.
/// </summary>

using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models.Search;

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
			height += lb.HorizontalScrollBarHeight();
			lb.Height = height;
		}
	}

	/// <summary>The categories, in the order they are filled, and the list showing each one.</summary>
	private (SearchCategory Category, VirtualCheckedListBox List)[] CategoryLists => [
		(SearchCategory.Characters, this.scalingCheckedListBoxCharacters),
		(SearchCategory.ItemType, this.scalingCheckedListBoxItemType),
		(SearchCategory.Rarity, this.scalingCheckedListBoxRarity),
		(SearchCategory.Style, this.scalingCheckedListBoxStyle),
		(SearchCategory.Quality, this.scalingCheckedListBoxQuality),
		(SearchCategory.Vaults, this.scalingCheckedListBoxVaults),
		(SearchCategory.WithCharm, this.scalingCheckedListBoxWithCharm),
		(SearchCategory.WithRelic, this.scalingCheckedListBoxWithRelic),
		(SearchCategory.Origin, this.scalingCheckedListBoxOrigin),
		(SearchCategory.SetItems, this.scalingCheckedListBoxSetItems),
		(SearchCategory.Skills, this.scalingCheckedListBoxSkills),
		(SearchCategory.ItemAttributes, this.scalingCheckedListBoxItemAttributes),
		(SearchCategory.BaseAttributes, this.scalingCheckedListBoxBaseAttributes),
		(SearchCategory.PrefixName, this.scalingCheckedListBoxPrefixName),
		(SearchCategory.PrefixAttributes, this.scalingCheckedListBoxPrefixAttributes),
		(SearchCategory.SuffixName, this.scalingCheckedListBoxSuffixName),
		(SearchCategory.SuffixAttributes, this.scalingCheckedListBoxSuffixAttributes),
	];

	/// <summary>Items the categories are built from, taken once for all of them by <see cref="SearchEngineReady"/>.</summary>
	private SearchResult[] _categoryItems = [];

	/// <summary>
	/// Fills <paramref name="clb"/> with the values of <paramref name="category"/>.
	/// </summary>
	private void PopulateCategory(SearchCategory category, VirtualCheckedListBox clb)
	{
		var label = CategoryLabel(clb);
		var items = this.Services.SearchCategoryService.BuildCategory(category, this._categoryItems)
			.Select(entry => new BoxItem
			{
				DisplayValue = entry.DisplayValue,
				MatchingResults = entry.Results,
				CheckedList = clb,
				Category = label,
			});

		PopulateInit(clb, items);
	}

	private static void PopulateInit(VirtualCheckedListBox clb, IEnumerable<BoxItem> items)
	{
		var (_, tag) = clb.GetBoxTag();
		tag.DataSource = items.ToArray();
		clb.BeginUpdate();
		clb.Items.AddRange(tag.DataSource);
		clb.EndUpdate();
	}
}
