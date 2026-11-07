/// <summary>
/// Class for the Search Dialog box.
/// </summary>

using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.Presentation;
using VisibilityItem = TQVaultAE.GUI.Models.Search.SearchQuery.VisibilityItem;

namespace TQVaultAE.GUI.Dialogs;

public partial class SearchDialogAdvanced
{
	private void TextBoxSearchTerm_TextChanged_Logic()
	{
		MakeSearchTermBoxItem();

		/// Wrapped into an Invoke() because i'm currently in the thread of the
		/// <see cref="typeAssistantSearchBox"> and i need  <see cref="Sync_SelectedFilters"/> to be executed from the main thread to avoid concurrent access exception
		/// Check IsHandleCreated and !IsDisposed to avoid InvalidOperationException when the form is initializing or disposing
		if (this.IsHandleCreated && !this.IsDisposed)
			this.Invoke(Sync_SelectedFilters);
	}

	private BoxItem MakeSearchTermBoxItem(string? searchTerm = null)
	{
		// Init a special BoxItem for the search term
		var txt = searchTerm is null
			? scalingTextBoxSearchTerm.Text.Trim()
			: searchTerm.Trim();

		var (_, searchTermBoxItem) = scalingTextBoxSearchTerm.GetBoxItem(true);// true : i need to make a new intance here to make the SearchQuery remember search term (must not share same object reference)

		searchTermBoxItem.Category = scalingLabelSearchTerm;
		searchTermBoxItem.DisplayValue = txt;

		if (string.IsNullOrWhiteSpace(txt))
			searchTermBoxItem.MatchingResults = [];
		else
		{
			// Item fulltext search using service method
			var (isRegex, _, regex, regexIsValid) = StringHelper.IsTQVaultSearchRegEx(txt);

			searchTermBoxItem.MatchingResults = this.ItemDatabaseService.FullTextSearch(txt, isRegex && regexIsValid).ToList();
		}

		// When this method is used as a BoxItem factory, i don't want the textbox to keep the reference.
		if (searchTerm != null)
			scalingTextBoxSearchTerm.Tag = null;

		return searchTermBoxItem;
	}

	private void LoadPersonnalQueries()
	{
		if (!this.SQueries.Any())
			return;

		// Try to retrieve actual instantiated BoxItems related to saved data.
		var matrix = (
			from query in this.SQueries
			from boxi in query.CheckedItems
			select new
			{
				// Source
				query,
				query.QueryName,
				boxiSave = boxi,
				// Join
				boxi.DisplayValue,
				boxi.CategoryName,
				boxi.CheckedListName,
				found = new List<BoxItem>() // Late binding placeholder because anonymous types are immutable
			}
		).ToList();

		// Retrieve CheckBoxes - use cached list for performance
		foreach (var lb in this.CachedCheckedListBoxes)
		{
			var (_, tag) = lb.GetBoxTag();

			(
				// Align Saved & Live BoxItems
				from ds in tag.DataSource
				join m in matrix on new { ds.CategoryName, ds.CheckedListName, ds.DisplayValue } equals new { m.CategoryName, m.CheckedListName, m.DisplayValue }
				select new { boxiLive = ds, matrix = m }
			).ToList().ForEach(r => r.matrix.found.Add(r.boxiLive));// Bind
		}

		// Make Search terms
		(
			from m in matrix
			where m.CategoryName == scalingLabelSearchTerm.Name // Unique to the search term
			select new { boxiLive = MakeSearchTermBoxItem(m.DisplayValue), matrix = m }
		).ToList().ForEach(r => r.matrix.found.Add(r.boxiLive));// Bind

		// Renew list
		(
			from m in matrix
			where m.found.Count != 0 // Saved boxitems may not be retrieved if you have lost the items that carry the corresponding properties.
			group m by m.QueryName into grp // Should have only one query per group
			select grp
		).ToList().ForEach(grp =>
		{
			var originalQueryInstance = grp.First().query;
			//originalQueryInstance.QueryName = grp.Key;
			originalQueryInstance.CheckedItems = grp.SelectMany(i => i.found).ToArray();
		});

		SearchQueriesInit();
	}

	private void scalingButtonQuerySave_Click(object? sender, EventArgs e)
	{
		var input = scalingComboBoxQueryList.Text.Trim();
		bool overwrite = false;
		SearchQuery? foundIt = null;

		#region Validation

		// You must have text
		if (string.IsNullOrWhiteSpace(input))
		{
			this.Services.MessageBoxes.Inform(Resources.SearchQueryNameMustBeSet, Resources.GlobalInputWarning, MessageBoxIcon.Error);
			return;
		}

		// You must have filters
		if (!HaveFilters())
		{
			this.Services.MessageBoxes.Inform(Resources.SearchTermRequired, Resources.GlobalInputWarning, MessageBoxIcon.Error);
			return;
		}

		// Name conflict
		foundIt = this.SQueries.FirstOrDefault(q => q.QueryName.Equals(input, StringComparison.OrdinalIgnoreCase));
		if (foundIt != null)
		{
			if (!this.Services.MessageBoxes.Confirm(Resources.SearchQueryNameAlreadyExist, Resources.GlobalInputWarning, MessageBoxIcon.Warning))
				return;

			overwrite = true;
		}

		#endregion

		if (overwrite && foundIt is not null)
		{
			UpdateQuery(input, foundIt);
			SavePersonnalQueries();
			scalingComboBoxQueryList.Refresh();
			return;
		}

		// Add scenario
		var newList = new IEnumerable<SearchQuery>[] {
			this.SQueries
			, new[] {
				UpdateQuery(input, new SearchQuery())
			}
		}
		.SelectMany(s => s)
		.OrderBy(s => s.QueryName)
		.ToList();

		this.SQueries.Clear();
		this.SQueries.AddRange(newList);

		SearchQueriesInit();

		SavePersonnalQueries();

		SearchQuery UpdateQuery(string input, SearchQuery foundIt)
		{
			foundIt.QueryName = input;
			foundIt.CheckedItems = this.SelectedFilters.ToArray();// i need a clone here so ToArray() do the job

			// Visible elements / category
			foundIt.MaxElement = this.numericUpDownMaxElement.Value;
			// Reduce categories
			foundIt.Reduce = this.scalingCheckBoxReduceDuringSelection.Checked;
			// Logical operator
			foundIt.Logic = (SearchOperator)this.scalingComboBoxOperator.SelectedIndex;
			// Add Category filter
			foundIt.Filter = this.scalingTextBoxFilterCategories.Text;
			// Add Display elements
			foundIt.Visible = NavMap.Select(m => new VisibilityItem(m.Button.Name, m.Panel.Visible)).ToList();
			// Requierements
			foundIt.MinRequirement = scalingCheckBoxMinReq.Checked;
			foundIt.MaxRequirement = scalingCheckBoxMaxReq.Checked;
			foundIt.MaxLvl = (int)numericUpDownMaxLvl.Value;
			foundIt.MaxStr = (int)numericUpDownMaxStr.Value;
			foundIt.MaxDex = (int)numericUpDownMaxDex.Value;
			foundIt.MaxInt = (int)numericUpDownMaxInt.Value;
			foundIt.MinLvl = (int)numericUpDownMinLvl.Value;
			foundIt.MinStr = (int)numericUpDownMinStr.Value;
			foundIt.MinDex = (int)numericUpDownMinDex.Value;
			foundIt.MinInt = (int)numericUpDownMinInt.Value;
			foundIt.HavingPrefix = this.scalingCheckBoxHavingPrefix.Checked;
			foundIt.HavingSuffix = this.scalingCheckBoxHavingSuffix.Checked;
			foundIt.HavingRelic = this.scalingCheckBoxHavingRelic.Checked;
			foundIt.HavingCharm = this.scalingCheckBoxHavingCharm.Checked;
			foundIt.IsSetItem = this.scalingCheckBoxIsSetItem.Checked;
			foundIt.HavingSkill = this.scalingCheckBoxHavingSkill.Checked;
			return foundIt;
		}

		bool HaveFilters()
		{
			return this.SelectedFilters.Count != 0
				|| (this.scalingCheckBoxMinReq.Checked
					&& (
						this.numericUpDownMinLvl.Value != 0
						|| this.numericUpDownMinStr.Value != 0
						|| this.numericUpDownMinDex.Value != 0
						|| this.numericUpDownMinInt.Value != 0
					)
				)
				|| (this.scalingCheckBoxMaxReq.Checked
					&& (
						this.numericUpDownMaxLvl.Value != 0
						|| this.numericUpDownMaxStr.Value != 0
						|| this.numericUpDownMaxDex.Value != 0
						|| this.numericUpDownMaxInt.Value != 0
					)
				)
				|| this.scalingCheckBoxHavingCharm.Checked
				|| this.scalingCheckBoxHavingRelic.Checked
				|| this.scalingCheckBoxHavingPrefix.Checked
				|| this.scalingCheckBoxHavingSuffix.Checked
				|| this.scalingCheckBoxIsSetItem.Checked
				|| this.scalingCheckBoxHavingSkill.Checked
				;
		}
	}

	private void scalingButtonQueryDelete_Click(object? sender, EventArgs e)
	{
		var idx = scalingComboBoxQueryList.SelectedIndex;
		if (idx == -1) return;

		var item = scalingComboBoxQueryList.SelectedItem;

		if (!this.Services.MessageBoxes.Confirm(string.Format(Resources.GlobalDeleteConfirm, item), Resources.GlobalInputWarning))
			return;


		scalingComboBoxQueryList.Items.RemoveAt(idx);
		this.SQueries.RemoveAt(idx);

		SavePersonnalQueries();
	}
}
