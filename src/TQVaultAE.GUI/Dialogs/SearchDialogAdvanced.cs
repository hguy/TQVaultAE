using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;
using TQVaultAE.Domain.Helpers;
using Microsoft.Extensions.Logging;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.GUI.Tooltips;
using System.Text.RegularExpressions;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using VisibilityItem = TQVaultAE.GUI.Models.Search.SearchQuery.VisibilityItem;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Class for the Search Dialog box.
/// </summary>
public partial class SearchDialogAdvanced : VaultForm
{
	private readonly IItemDatabaseService ItemDatabaseService;
	private readonly ILogger Log;
	private readonly Bitmap ButtonImageUp;
	private readonly Bitmap ButtonImageDown;
	private readonly (ScalingButton Button, FlowLayoutPanel Panel)[] NavMap;
	private readonly List<BoxItem> SelectedFilters = new();
	private readonly HashSet<BoxItem> SelectedFiltersSet = new();
	private readonly List<VirtualCheckedListBox> CachedCheckedListBoxes = new();
	private readonly SearchQueries SQueries;

	public SearchResult[] QueryResults { get; private set; } = Array.Empty<SearchResult>();
	private bool scalingCheckBoxReduceDuringSelection_LastChecked;

	/// <summary>
	/// Initializes a new instance of SearchDialog class.
	/// </summary>
	public SearchDialogAdvanced(
		MainForm instance
		, ILogger<SearchDialogAdvanced> log
		, SearchQueries sQueries
		, IItemDatabaseService itemDatabaseService
	) : base(instance.Services)
	{
		this.Owner = instance;
		this.Log = log;
		this.SQueries = sQueries;
		this.ItemDatabaseService = itemDatabaseService;

		this.InitializeComponent();

		this.MinimizeBox = false;
		this.NormalizeBox = false;
		this.MaximizeBox = true;

		#region Apply custom font

		this.ProcessAllControls(c =>
		{
			if (c is IScalingControl) c.Font = FontService.GetFont(9F);
		});

		this.applyButton.Font = FontService.GetFontLight(12F);
		this.cancelButton.Font = FontService.GetFontLight(12F);

		this.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0));

		#endregion

		#region Load localized strings

		this.Text = Resources.SearchDialogCaption;
		this.applyButton.Text = TranslationService.TranslateXTag("tagMenuButton07");
		this.cancelButton.Text = TranslationService.TranslateXTag("tagMenuButton06");
		this.scalingLabelFiltersSelected.Tag = Resources.SearchFiltersSelected;
		this.scalingLabelFiltersSelected.Text = string.Empty;
		this.scalingLabelOperator.Text = $"{Resources.SearchOperator} :";
		this.buttonExpandAll.Text = Resources.GlobalExpandAll;
		this.buttonCollapseAll.Text = Resources.GlobalCollapseAll;
		this.scalingLabelMaxVisibleElement.Text = $"{Resources.SearchVisibleElementsPerCategory} :";
		this.scalingLabelSearchTerm.Text = $"{TranslationService.TranslateXTag("xtagLobbySearchSearch")} :";
		this.scalingButtonReset.Text = TranslationService.TranslateXTag("tagSkillReset");
		this.scalingLabelQueries.Text = $"{Resources.SearchQueries} :";
		this.scalingButtonQuerySave.Text = Resources.GlobalSave;
		this.scalingButtonQueryDelete.Text = TranslationService.TranslateXTag("tagMenuButton03");
		this.scalingCheckBoxReduceDuringSelection.Text = Resources.SearchReduceCategoriesDuringSelection;

		this.scalingComboBoxOperator.Items.Clear();
		this.scalingComboBoxOperator.Items.AddRange(new[] { Resources.SearchOperatorAnd, Resources.SearchOperatorOr });

		this.scalingButtonMenuAttribute.Text
			= this.scalingLabelItemAttributes.Text
			= TranslationService.TranslateXTag("tagCAttributes");
		this.scalingButtonMenuBaseAttribute.Text
			= this.scalingLabelBaseAttributes.Text
			= Resources.GlobalBaseAttribute;
		this.scalingButtonMenuCharacters.Text
			= this.scalingLabelCharacters.Text
			= TranslationService.TranslateXTag("tagWindowName01");
		this.scalingButtonMenuPrefixAttribute.Text
			= this.scalingLabelPrefixAttributes.Text
			= Resources.GlobalPrefix;
		this.scalingButtonMenuPrefixName.Text
			= this.scalingLabelPrefixName.Text
			= Resources.GlobalPrefixName;
		this.scalingButtonMenuQuality.Text
			= this.scalingLabelQuality.Text
			= Resources.ResultsQuality;
		this.scalingButtonMenuRarity.Text
			= this.scalingLabelRarity.Text
			= Resources.GlobalRarity;
		this.scalingButtonMenuStyle.Text
			= this.scalingLabelStyle.Text
			= Resources.GlobalStyle;
		this.scalingButtonMenuSuffixAttribute.Text
			= this.scalingLabelSuffixAttributes.Text
			= Resources.GlobalSuffix;
		this.scalingButtonMenuSuffixName.Text
			= this.scalingLabelSuffixName.Text
			= Resources.GlobalSuffixName;
		this.scalingButtonMenuType.Text
			= this.scalingLabelItemType.Text
			= Resources.GlobalType;
		this.scalingButtonMenuVaults.Text
			= this.scalingLabelInVaults.Text
			= Resources.GlobalVaults;
		this.scalingButtonMenuWithCharm.Text
			= this.scalingLabelWithCharm.Text
			= Resources.SearchHavingCharm;
		this.scalingButtonMenuWithRelic.Text
			= this.scalingLabelWithRelic.Text
			= Resources.SearchHavingRelic;
		this.scalingButtonMenuOrigin.Text
			= this.scalingLabelOrigin.Text
			= Resources.GlobalOrigin;
		this.scalingButtonMenuSetItems.Text
			= this.scalingLabelSetItems.Text
			= Resources.SearchSetItem;

		scalingCheckBoxMinReq.Text = $"{Resources.GlobalMin} {Resources.GlobalRequirements} :";
		scalingCheckBoxMaxReq.Text = $"{Resources.GlobalMax} {Resources.GlobalRequirements} :";
		scalingLabelMaxLvl.Text =
		scalingLabelMinLvl.Text = TranslationService.TranslateXTag("tagMenuImport05");
		scalingLabelMaxStr.Text =
		scalingLabelMinStr.Text = TranslationService.TranslateXTag("Strength");
		scalingLabelMaxDex.Text =
		scalingLabelMinDex.Text = TranslationService.TranslateXTag("Dexterity");
		scalingLabelMaxInt.Text =
		scalingLabelMinInt.Text = TranslationService.TranslateXTag("Intelligence");
		scalingCheckBoxHavingCharm.Text = Resources.SearchHavingCharm;
		scalingCheckBoxHavingRelic.Text = Resources.SearchHavingRelic;
		scalingCheckBoxHavingPrefix.Text = Resources.SearchHavingPrefix;
		scalingCheckBoxHavingSuffix.Text = Resources.SearchHavingSuffix;
		scalingCheckBoxIsSetItem.Text = Resources.SearchSetItem;
		scalingCheckBoxHavingSkill.Text = Resources.SearchHavingSkill;

		#endregion

		// Mapping between nav button & content component
		if (NavMap is null)
		{
			NavMap = new (ScalingButton Button, FlowLayoutPanel Panel)[] {
				(this.scalingButtonMenuAttribute, this.flowLayoutPanelItemAttributes),
				(this.scalingButtonMenuBaseAttribute, this.flowLayoutPanelBaseAttributes),
				(this.scalingButtonMenuCharacters, this.flowLayoutPanelCharacters),
				(this.scalingButtonMenuPrefixAttribute, this.flowLayoutPanelPrefixAttributes),
				(this.scalingButtonMenuPrefixName, this.flowLayoutPanelPrefixName),
				(this.scalingButtonMenuRarity, this.flowLayoutPanelRarity),
				(this.scalingButtonMenuStyle, this.flowLayoutPanelStyle),
				(this.scalingButtonMenuSuffixAttribute, this.flowLayoutPanelSuffixAttributes),
				(this.scalingButtonMenuSuffixName, this.flowLayoutPanelSuffixName),
				(this.scalingButtonMenuType, this.flowLayoutPanelItemType),
				(this.scalingButtonMenuVaults, this.flowLayoutPanelInVaults),
				(this.scalingButtonMenuWithCharm, this.flowLayoutPanelWithCharm),
				(this.scalingButtonMenuWithRelic, this.flowLayoutPanelWithRelic),
				(this.scalingButtonMenuQuality, this.flowLayoutPanelQuality),
				(this.scalingButtonMenuOrigin, this.flowLayoutPanelOrigin),
				(this.scalingButtonMenuSetItems, this.flowLayoutPanelSetItems),
			};
		}

		// Keep reference of base Up & Down button image
		this.ButtonImageUp = this.scalingButtonMenuAttribute.UpBitmap;
		this.ButtonImageDown = this.scalingButtonMenuAttribute.DownBitmap;

		this.scalingComboBoxOperator.SelectedIndex = (int)SearchOperator.And;

		// Remove design time fake elements
		scalingComboBoxQueryList.Items.Clear();

		// Cache all VirtualCheckedListBox references for performance
		this.flowLayoutPanelMain.ProcessAllControls(c =>
		{
			if (c is VirtualCheckedListBox lb)
				this.CachedCheckedListBoxes.Add(lb);
		});

		// Now ClearAllCheckBoxes will work correctly
		CleanAllCheckBoxes();

		// Set numerical to default
		this.ProcessAllControls(c =>
		{
			if (c is NumericUpDown num && num != numericUpDownMaxElement) num.Value = 0;
		});

		this.VisibleChanged += this.SearchDialogVisibleChanged;
	}

	private void SetSearchBoxVisibility(bool isVisible)
		=> NavMap.ToList().ForEach(m => m.Panel.Visible = isVisible);

	#region Apply & Cancel

	/// <summary>
	/// Handler for clicking the apply button on the form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ApplyButtonClicked(object? sender, EventArgs e)
	{
		if (SelectedFilters.Count == 0)
		{
			scalingLabelProgress.Text = $"{Resources.SearchTermRequired} - {string.Format(Resources.SearchItemCountIs, this.ItemDatabaseService.ItemDatabase.Count)}";
			return;
		}
		;

		this.DialogResult = DialogResult.OK;
		this.Close();
	}

	/// <summary>
	/// Handler for clicking the cancel button on the form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CancelButtonClicked(object? sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
		this.Close();
	}

	#endregion

	#region Show & rebuild

	/// <summary>Item database the categories were built from, and its size at that time.</summary>
	private object? _builtDatabase;
	private int _builtCount = -1;

	/// <summary>Measures the time from opening the dialog to the categories being ready.</summary>
	private readonly System.Diagnostics.Stopwatch _openWatch = new();

	/// <summary>Steps run on the UI thread after the item data is built, each one a progress bar step.</summary>
	private Action[] UiBuildSteps => [
		this.PopulateCharacters,
		this.PopulateItemType,
		this.PopulateRarity,
		this.PopulateStyle,
		this.PopulateQuality,
		this.PopulateVaults,
		this.PopulateWithCharm,
		this.PopulateWithRelic,
		this.PopulateOrigin,
		this.PopulateSetItems,
		this.PopulateItemAttributes,
		this.PopulateBaseAttributes,
		this.PopulatePrefixName,
		this.PopulatePrefixAttributes,
		this.PopulateSuffixName,
		this.PopulateSuffixAttributes,
		this.AdjustCheckBoxesWidth,
		this.AdjustCheckBoxesHeight,
		() => { this.SetSearchBoxVisibility(true); this.SyncNaveButton(); },
		this.LoadPersonnalQueries,
	];

	/// <summary>
	/// The dialog is kept between openings (see <c>MainForm.OpenSearchDialog</c>), and <see cref="Form.Shown"/> is only
	/// raised the first time, so each opening is detected here.
	/// </summary>
	private void SearchDialogVisibleChanged(object? sender, EventArgs e)
	{
		if (!this.Visible)
			return;

		this._openWatch.Restart();
		this.BeginInvoke(this.PrepareForSearch);
	}

	/// <summary>
	/// Builds the categories on the first opening, and again only when the item database changed since then;
	/// otherwise the dialog is shown as it was left.
	/// </summary>
	private async void PrepareForSearch()
	{
		// Paint the dialog (VaultForm custom borders etc.) before the categories are built.
		this.Update();

		var database = this.ItemDatabaseService.ItemDatabase;
		if (ReferenceEquals(database, this._builtDatabase) && database.Count == this._builtCount)
		{
			this.Log.LogInformation("Search dialog reopened in {Elapsed} ms (items unchanged)", this._openWatch.ElapsedMilliseconds);
			this.scalingTextBoxSearchTerm.Focus();
			return;
		}

		if (this._buildingSearchData)
			return;

		if (this._builtDatabase is not null)
			this.ClearForRebuild();

		scalingLabelProgress.Text = Resources.SearchBuildingData;
		scalingLabelProgress.Visible = true;

		vaultProgressBar.Minimum = 0;
		vaultProgressBar.Value = 0;
		vaultProgressBar.Maximum = database.Count + this.UiBuildSteps.Length;
		vaultProgressBar.Visible = true;

		this._buildingSearchData = true;
		try
		{
			// Created on the UI thread, so the progress bar is updated there.
			var progress = new Progress<int>(this.vaultProgressBar.Increment);
			await Task.Run(() => this.InitItemDatabase(progress));
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Building the search data failed");
			scalingLabelProgress.Text = ex.Message;
			return;
		}
		finally
		{
			this._buildingSearchData = false;
		}

		this.SearchEngineReady();
	}

	/// <summary>True while <see cref="InitItemDatabase"/> runs.</summary>
	private bool _buildingSearchData;

	/// <summary>Empties the categories and the current selection, which refer to the previous items.</summary>
	private void ClearForRebuild()
	{
		this._Apply_SelectedFiltersDisabled = true;
		try
		{
			this.SetSearchBoxVisibility(false);
			this.CleanAllCheckBoxes();
			this.SelectedFilters.Clear();
			this.SelectedFiltersSet.Clear();
			this.QueryResults = [];
			this.scalingComboBoxQueryList.ResetText();
			this.ResetQuickFilters();
			this.ResetRequirements();
			this.TextBoxSearchTerm_UpdateText_Notrigger(string.Empty);
			this.scalingLabelFiltersSelected.Text = string.Empty;
		}
		finally
		{
			this._Apply_SelectedFiltersDisabled = false;
		}
	}

	/// <summary>Advances the progress bar and paints it now: the UI build steps keep the message loop busy.</summary>
	private void ProgressStep()
	{
		this.vaultProgressBar.Increment(1);
		this.vaultProgressBar.Update();
	}

	#endregion

	#region Load & Init

	private void SearchDialogAdvanced_Load(object? sender, EventArgs e)
	{
		// this.ItemDatabaseService.ItemDatabase is already populated during container loading via SessionContext
		// No need to rebuild it here - just lazy-load the friendly names
	}

	/// <summary>
	/// Load item data & display progress. Runs off the UI thread, so it must not change UI controls.
	/// </summary>
	/// <param name="progress">Receives the number of items loaded since the last report.</param>
	private void InitItemDatabase(IProgress<int> progress)
	{
		// Parallel lazy loading for much faster database initialization
		var items = this.ItemDatabaseService.ItemDatabase.ToList();
		var processedCount = 0;

		// Report in batches: one UI message per item floods the message loop on large vaults.
		int batch = Math.Max(1, items.Count / 200);
		Parallel.ForEach(items, item =>
		{
			item.LazyLoad();
			if (Interlocked.Increment(ref processedCount) % batch == 0)
				progress.Report(batch);
		});
		if (processedCount % batch != 0)
			progress.Report(processedCount % batch);

		// Cleanup zombies - ConcurrentBag doesn't support RemoveAll, so we filter and rebuild
		var validItems = items.Where(id => !string.IsNullOrWhiteSpace(id.ItemName)).ToList();

		this.ItemDatabaseService.ResetAllItemDatabase(validItems);
	}

	#endregion

	private void SearchEngineReady()
	{
		// Fill and size the categories, one progress step each, so the bar covers everything until the dialog is usable.
		var dataBuilt = this._openWatch.ElapsedMilliseconds;
		var stepTimes = new List<long>(this.UiBuildSteps.Length + 1);
		var stepWatch = System.Diagnostics.Stopwatch.StartNew();
		this.flowLayoutPanelMain.SuspendLayout();
		try
		{
			foreach (var step in this.UiBuildSteps)
			{
				step();
				this.ProgressStep();
				stepTimes.Add(stepWatch.ElapsedMilliseconds);
				stepWatch.Restart();
			}
		}
		finally
		{
			this.flowLayoutPanelMain.ResumeLayout(true);
			stepTimes.Add(stepWatch.ElapsedMilliseconds);
		}
		this.Log.LogInformation(
			"Search dialog build: data {DataMs} ms, UI steps [{Steps}] ms (last is ResumeLayout), list items [{Items}]"
			, dataBuilt
			, string.Join(", ", stepTimes)
			, string.Join(", ", this.CachedCheckedListBoxes.Select(lb => $"{lb.Name.Replace("scalingCheckedListBox", string.Empty)}={lb.Items.Count}")));

		this._builtDatabase = this.ItemDatabaseService.ItemDatabase;
		this._builtCount = this.ItemDatabaseService.ItemDatabase.Count;

		scalingLabelProgress.Text = $"{Resources.SearchEngineReady} - {string.Format(Resources.SearchItemCountIs, this._builtCount)}";
		this.Log.LogInformation("Search dialog ready in {Elapsed} ms ({Count} items)", this._openWatch.ElapsedMilliseconds, this._builtCount);

		// Start
		this.scalingTextBoxSearchTerm.Focus();
	}

	private void AdjustCheckBoxesWidth()
	{
		if (this.CachedCheckedListBoxes.Count == 0)
			return;

		var maxElement = (int)this.numericUpDownMaxElement.Value;
		foreach (var lb in this.CachedCheckedListBoxes)
			lb.AdjustToMaxTextWidth(maxElement);
	}

	#region Populate

	/// <summary>
	/// Label of the category that holds <paramref name="list"/>.
	/// </summary>
	private static ScalingLabel CategoryLabel(Control list)
		=> (list.Parent ?? throw new InvalidOperationException($"{list.Name} has no category."))
			.Controls.OfType<ScalingLabel>().First();

	#endregion

	private void numericUpDownMaxElement_ValueChanged(object? sender, EventArgs e)
		=> AdjustCheckBoxesHeight();

	private void buttonCollapseAll_Click(object? sender, EventArgs e)
	{
		foreach (var lb in this.CachedCheckedListBoxes)
			lb.Visible = false;
	}

	private void buttonExpandAll_Click(object? sender, EventArgs e)
	{
		foreach (var lb in this.CachedCheckedListBoxes)
			lb.Visible = true;
	}

	private void scalingButtonMenu_Click(object? sender, EventArgs e)
	{
		// Toggle
		var (button, panel) = NavMap.First(m => object.ReferenceEquals(m.Button, sender));
		panel.Visible = !panel.Visible;

		SyncNaveButton();
	}

	private void scalingCheckedListBox_SelectedValueChanged(object? sender, EventArgs e)
		=> Sync_SelectedFilters();

	private void scalingLabelFiltersSelected_MouseEnter(object? sender, EventArgs e)
	{
		var ctrl = (Control)sender!;
		SearchFiltersTooltip.ShowTooltip(this.Services, ctrl, this.SelectedFilters, (SearchOperator)scalingComboBoxOperator.SelectedIndex);
	}

	private void scalingLabelFiltersSelected_MouseLeave(object? sender, EventArgs e)
		=> SearchFiltersTooltip.HideTooltip();

	bool _Apply_SelectedFiltersDisabled = false;

	private void scalingComboBoxOperator_SelectionChangeCommitted(object? sender, EventArgs e)
	{
		if (scalingCheckBoxReduceDuringSelection.Checked)
			ResetCheckBoxesToFirstLoad();

		Apply_SelectedFilters();
	}

	private void scalingCheckBoxReduceDuringSelection_CheckedChanged(object? sender, EventArgs e)
	{
		scalingCheckBoxReduceDuringSelection_LastChecked = !scalingCheckBoxReduceDuringSelection.Checked;
		ApplyCategoriesReducer();
		scalingCheckBoxReduceDuringSelection_LastChecked = scalingCheckBoxReduceDuringSelection.Checked;
	}

	private sealed class FilterCategoriesDetails
	{
		internal bool HasFilterCategory;
		internal bool IsRegex;
		internal string Search = string.Empty;
		internal Regex? Regex;
		internal bool RegexIsValid;
		internal string SearchRaw = string.Empty;
		internal string SearchRawPrevious = string.Empty;

		internal bool HasChanged => SearchRaw != SearchRawPrevious;
		internal bool IsMatch(string attributeText)
			=> (IsRegex && RegexIsValid)
				? Regex?.IsMatch(attributeText) ?? false
				: attributeText.ContainsIgnoreCase(Search);

		internal bool AvoidDisplayAttribute(string attributeText)
			=> HasFilterCategory && !IsMatch(attributeText);
	}
	private readonly FilterCategoriesDetails FilterCategories = new FilterCategoriesDetails();

	private void scalingButtonReset_Click(object? sender, EventArgs e)
		=> ResetSelectedFilters();

	/// <summary>Empties the category filter box and its state without waiting for the typing delay.</summary>
	private void ClearFilterCategories()
	{
		var f = this.FilterCategories;
		f.SearchRawPrevious = f.SearchRaw;
		f.SearchRaw = string.Empty;
		f.HasFilterCategory = false;
		this.scalingTextBoxFilterCategories.Text = string.Empty;
	}

	internal void ResetQuickFilters()
	{
		scalingCheckBoxIsSetItem.Checked =
		scalingCheckBoxHavingSkill.Checked =
		scalingCheckBoxHavingCharm.Checked =
		scalingCheckBoxHavingRelic.Checked =
		scalingCheckBoxHavingPrefix.Checked =
		scalingCheckBoxHavingSuffix.Checked = false;
	}

	private void TextBoxSearchTerm_UpdateText_Notrigger(string newText)
	{
		this.scalingTextBoxSearchTerm.TextChanged -= scalingTextBoxSearchTerm_TextChanged;

		this.scalingTextBoxSearchTerm.Text = newText;

		this.scalingTextBoxSearchTerm.TextChanged += scalingTextBoxSearchTerm_TextChanged;
	}

	private void scalingTextBoxSearchTerm_TextChanged(object? sender, EventArgs e)
		/// Wait for the end of typing by delaying the call to <see cref="scalingTextBoxSearchTerm_TextChanged_Idled"/>
		=> this.typeAssistantSearchBox.TextChanged();

	private void scalingTextBoxSearchTerm_TextChanged_Idled(object? sender, EventArgs e)
		=> TextBoxSearchTerm_TextChanged_Logic();

	private void scalingTextBoxFilterCategories_TextChanged(object? sender, EventArgs e)
		/// Wait for the end of typing by delaying the call to <see cref="typeAssistantFilterCategories_Idled"/>
		=> typeAssistantFilterCategories.TextChanged();

	private void scalingLabelProgress_MouseEnter(object? sender, EventArgs e)
	{
		var ctrl = (Control)sender!;
		FoundResultsTooltip.ShowTooltip(this.Services, ctrl, this.QueryResults);
	}

	private void scalingLabelProgress_MouseLeave(object? sender, EventArgs e)
		=> FoundResultsTooltip.HideTooltip();

	private void scalingLabelProgress_TextChanged(object? sender, EventArgs e)
	{
		var ctrl = (Control)sender!;
		ScalingLabelProgressAdjustSizeAndPosition(ctrl);
	}

	private static void ScalingLabelProgressAdjustSizeAndPosition(Control ctrl)
	{
		// Adjust Control Size & position manualy because center align the control in it's container doesn't work with AutoSize = true
		ctrl.Size = TextRenderer.MeasureText(ctrl.Text, ctrl.Font);
		var loc = ctrl.Location;
		if (ctrl.Parent is { } parent)
			loc.X = (parent.Size.Width / 2) - (ctrl.Width / 2);
		ctrl.Location = loc;
	}

	private void scalingLabelProgressPanelAlignText_SizeChanged(object? sender, EventArgs e)
	{
		// Prevent child text truncation during resize
		var ctrl = (Control)sender!;
		var label = ctrl.Controls.OfType<ScalingLabel>().First();
		ScalingLabelProgressAdjustSizeAndPosition(label);
	}

	private void SavePersonnalQueries()
	{
		this.SQueries.Save();
	}

	private void SearchQueriesInit()
	{
		scalingComboBoxQueryList.BeginUpdate();
		scalingComboBoxQueryList.Items.Clear();
		scalingComboBoxQueryList.Items.AddRange(this.SQueries.ToArray());
		scalingComboBoxQueryList.EndUpdate();
	}

	private void scalingComboBoxQueryList_SelectedIndexChanged(object? sender, EventArgs e)
	{
		var idx = scalingComboBoxQueryList.SelectedIndex;
		Make_SelectedFilters(this.SQueries[idx]);
	}

	private void scalingCheckBoxMinMaxReq_CheckedChanged(object? sender, EventArgs e)
	{
		Apply_SelectedFilters();
	}

	private void scalingCheckBoxQuickFilters_CheckedChanged(object? sender, EventArgs e)
	{
		Apply_SelectedFilters();
	}
}

public static class SearchDialogAdvancedExtension
{
	public static (VirtualCheckedListBox, BoxTag) GetBoxTag(this Control obj)
	{
		if (obj is not VirtualCheckedListBox ctr)
			throw new ArgumentException($"{obj.Name} is not a filter list.", nameof(obj));

		var Tag = ctr.Tag as BoxTag ?? new BoxTag();
		ctr.Tag = Tag;

		return (ctr, Tag);
	}

	public static (ScalingTextBox, BoxItem) GetBoxItem(this Control obj, bool newInstance)
	{
		if (obj is not ScalingTextBox ctr)
			throw new ArgumentException($"{obj.Name} is not a search box.", nameof(obj));

		var Tag = newInstance
			? new BoxItem()
			: ctr.Tag as BoxItem ?? new BoxItem();

		ctr.Tag = Tag;

		return (ctr, Tag);
	}
}