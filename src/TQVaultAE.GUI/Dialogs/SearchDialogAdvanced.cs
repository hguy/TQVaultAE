using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;
using TQVaultAE.Domain.Helpers;
using Microsoft.Extensions.Logging;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.GUI.Tooltips;
using System.Text.RegularExpressions;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Models;
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

	private void CleanAllCheckBoxes()
	{
		this.flowLayoutPanelMain.SuspendLayout();
		try
		{
			foreach (var lb in this.CachedCheckedListBoxes)
			{
				lb.BeginUpdate();
				lb.Items.Clear();
				lb.EndUpdate();
			}
		}
		finally
		{
			this.flowLayoutPanelMain.ResumeLayout(true);
		}
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

	#endregion

	private void numericUpDownMaxElement_ValueChanged(object? sender, EventArgs e)
		=> AdjustCheckBoxesHeight();

	private void scalingCheckedListBox_MouseMove(object? sender, MouseEventArgs e)
	{
		var ctr = (Control)sender!;
		var (lstBox, tag) = ctr.GetBoxTag();

		var focusedIdx = lstBox.IndexFromPoint(e.Location);

		if (tag.LastTooltipIndex != focusedIdx)
		{
			tag.LastTooltipIndex = focusedIdx;
			if (tag.LastTooltipIndex > -1)
			{
				var item = lstBox.Items[focusedIdx] as BoxItem;
				if (item is not null && item.MatchingResults is not null)
					toolTip.SetToolTip(lstBox, string.Format(Resources.SearchMatchingItemsTT, item.MatchingResults.Count()));
				else
					toolTip.SetToolTip(lstBox, string.Empty);
			}
		}

		lstBox.Tag = tag;
	}

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

	private void SyncNaveButton()
	{
		/// Push invisible categories at the end of <see cref="flowLayoutPanelMain"/> so nav buttons define categories order in flowpanel
		var trail = NavMap.Where(map => !map.Panel.Visible).Select(map => map.Panel).ToList();
		// Remove
		trail.ForEach(c => this.flowLayoutPanelMain.Controls.Remove(c));
		// Then Put it back at the end
		this.flowLayoutPanelMain.Controls.AddRange(trail.ToArray());

		SyncNavButtonImage();
	}

	private void SyncNavButtonImage()
	{
		foreach (var map in NavMap)
		{
			// Invert Up & Down each time you click to make it behave like a toggle
			if (map.Panel.Visible)
			{
				map.Button.UpBitmap = this.ButtonImageDown;
				map.Button.OverBitmap = this.ButtonImageUp;
				map.Button.Image = map.Button.DownBitmap;
			}
			else
			{
				map.Button.UpBitmap = this.ButtonImageUp;
				map.Button.OverBitmap = this.ButtonImageDown;
				map.Button.Image = map.Button.UpBitmap;
			}
		}
	}
	private void scalingCheckedListBox_SelectedValueChanged(object? sender, EventArgs e)
		=> Sync_SelectedFilters();

	private void Sync_SelectedFilters()
	{
		this.SelectedFilters.Clear();
		this.SelectedFiltersSet.Clear();

		// Add the SearchTerm on top 
		var (_, searchTermBoxItem) = this.scalingTextBoxSearchTerm.GetBoxItem(false);
		// if i get something to filter with
		if (!string.IsNullOrWhiteSpace(searchTermBoxItem.DisplayValue))
		{
			this.SelectedFilters.Add(searchTermBoxItem);
			this.SelectedFiltersSet.Add(searchTermBoxItem);
		}

		// Use cached list for performance
		// Use OfType instead of Cast to handle any non-BoxItem objects gracefully
		foreach (var lb in this.CachedCheckedListBoxes)
			this.SelectedFilters.AddRange(lb.CheckedItems.OfType<BoxItem>());

		// Populate HashSet for O(1) lookups
		foreach (var filter in this.SelectedFilters)
			this.SelectedFiltersSet.Add(filter);

		this.Apply_SelectedFilters();
	}

	private void scalingLabelCategory_MouseClick(object? sender, MouseEventArgs e)
	{
		var label = (ScalingLabel)sender!;
		if (label.Parent is not { } flowpanel)
			return;

		if (e.Button == MouseButtons.Left)
		{
			// Individual Expand/Collapse
			flowpanel.ProcessAllControls(c =>
			{
				if (c is VirtualCheckedListBox lb)
					lb.Visible = !lb.Visible;
			});
		}
		// Uncheck
		if (e.Button == MouseButtons.Right)
		{
			UncheckCategories(flowpanel);
			Sync_SelectedFilters();
		}
	}

	private void UncheckCategories(Control flowpanel)
	{
		// Optimize for main flowLayoutPanel - use cached list
		if (flowpanel == flowLayoutPanelMain)
		{
			this.flowLayoutPanelMain.SuspendLayout();
			try
			{
				foreach (var lb in this.CachedCheckedListBoxes)
				{
					lb.BeginUpdate();
					foreach (var idx in lb.CheckedIndices.Cast<int>().ToList()) lb.SetItemChecked(idx, false);
					lb.EndUpdate();
				}
			}
			finally
			{
				this.flowLayoutPanelMain.ResumeLayout(true);
			}
		}
		else
		{
			// Individual category panel - traverse
			flowpanel.ProcessAllControls(c =>
			{
				if (c is VirtualCheckedListBox lb)
				{
					lb.BeginUpdate();
					foreach (var idx in lb.CheckedIndices.Cast<int>().ToList()) lb.SetItemChecked(idx, false);
					lb.EndUpdate();
				}
			});
		}
	}

	private void scalingLabelFiltersSelected_MouseEnter(object? sender, EventArgs e)
	{
		var ctrl = (Control)sender!;
		SearchFiltersTooltip.ShowTooltip(this.Services, ctrl, this.SelectedFilters, (SearchOperator)scalingComboBoxOperator.SelectedIndex);
	}

	private void scalingLabelFiltersSelected_MouseLeave(object? sender, EventArgs e)
		=> SearchFiltersTooltip.HideTooltip();

	bool _Apply_SelectedFiltersDisabled = false;

	private void Apply_SelectedFilters()
	{
		if (_Apply_SelectedFiltersDisabled)
			return;

		int MaxLvl = (int)numericUpDownMaxLvl.Value,
			MaxDex = (int)numericUpDownMaxDex.Value,
			MaxInt = (int)numericUpDownMaxInt.Value,
			MaxStr = (int)numericUpDownMaxStr.Value,
			MinLvl = (int)numericUpDownMinLvl.Value,
			MinDex = (int)numericUpDownMinDex.Value,
			MinInt = (int)numericUpDownMinInt.Value,
			MinStr = (int)numericUpDownMinStr.Value;
		bool MaxRequierement = scalingCheckBoxMaxReq.Checked, MinRequierement = scalingCheckBoxMinReq.Checked;

		this.scalingLabelFiltersSelected.Text = string.Format(this.scalingLabelFiltersSelected.Tag?.ToString() ?? "{0}", SelectedFilters.Count);

		// Combine selected filter results based on operator
		IReadOnlyList<SearchResult> initialResults;
		if (this.scalingComboBoxOperator.SelectedIndex == (int)SearchOperator.And)
		{
			// AND operator => item must exist in every filter
			var firstFilter = SelectedFilters.FirstOrDefault();
			if (firstFilter == null)
			{
				initialResults = [];
			}
			else
			{
				var resultSet = new HashSet<SearchResult>(firstFilter.MatchingResults);
				foreach (var filter in SelectedFilters.Skip(1))
				{
					resultSet.IntersectWith(filter.MatchingResults);
				}
				initialResults = resultSet.ToList();
			}
		}
		else
		{
			// OR Operator => Accumulate & Distinct
			initialResults = SelectedFilters
				.SelectMany(f => f.MatchingResults)
				.Distinct()
				.ToList();
		}

		// Apply quick filters and requirements via service
		this.QueryResults = this.ItemDatabaseService.ExecuteAdvancedSearch(
			new AdvancedSearchRequest
			{
				InitialResults = initialResults,
				HasPrefix = this.scalingCheckBoxHavingPrefix.Checked,
				HasSuffix = this.scalingCheckBoxHavingSuffix.Checked,
				HasRelic = this.scalingCheckBoxHavingRelic.Checked,
				HasCharm = this.scalingCheckBoxHavingCharm.Checked,
				IsSetItem = this.scalingCheckBoxIsSetItem.Checked,
				MinLevel = MinRequierement ? MinLvl : 0,
				MaxLevel = MaxRequierement ? MaxLvl : 0,
				MinStrength = MinRequierement ? MinStr : 0,
				MaxStrength = MaxRequierement ? MaxStr : 0,
				MinDexterity = MinRequierement ? MinDex : 0,
				MaxDexterity = MaxRequierement ? MaxDex : 0,
				MinIntelligence = MinRequierement ? MinInt : 0,
				MaxIntelligence = MaxRequierement ? MaxInt : 0
			}
		).ToArray();

		scalingLabelProgress.Text = $"{string.Format(Resources.SearchItemCountIs, QueryResults.Length)}";

		ApplyCategoriesReducer();
	}

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

	private void ApplyCategoriesReducer()
	{
		// Category Reduced Display
		if (scalingCheckBoxReduceDuringSelection.Checked)
		{
			ReduceCheckBoxesToQueryResult();
			return;
		}

		if (
			this.FilterCategories.HasChanged
			|| (
				// Category Full Display
				!scalingCheckBoxReduceDuringSelection.Checked
				/// But comes from <see cref="scalingCheckBoxReduceDuringSelection_CheckedChanged"/> meaning "from a Category Reduced Display"
				&& scalingCheckBoxReduceDuringSelection.Checked != scalingCheckBoxReduceDuringSelection_LastChecked
			)
		)
		{
			ResetCheckBoxesToFirstLoad();// Restore Full Display
		}
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

	private void ResetCheckBoxesToFirstLoad()
	{
		// Reset to FirstLoad
		CleanAllCheckBoxes();

		this.flowLayoutPanelMain.SuspendLayout();
		try
		{
			foreach (var lb in this.CachedCheckedListBoxes)
			{
				var (_, tag) = lb.GetBoxTag();
				lb.BeginUpdate();
				foreach (var item in tag.DataSource)
				{
					// Use HashSet for O(1) lookup instead of O(n) List.Contains
					var isChecked = this.SelectedFiltersSet.Contains(item);

					if (!isChecked // Do not hide already checked
						/// Should i filter out this check box based on <see cref="FilterCategories"/> ?
						&& this.FilterCategories.AvoidDisplayAttribute(item.DisplayValue)
					) continue;

					lb.Items.Add(item, isChecked);
				}
				lb.EndUpdate();
			}
		}
		finally
		{
			this.flowLayoutPanelMain.ResumeLayout(true);
		}
	}

	private void ReduceCheckBoxesToQueryResult()
	{
		// Nothing selected: there is nothing to reduce to, show every category entry.
		if (this.SelectedFilters.Count == 0)
		{
			ResetCheckBoxesToFirstLoad();
			return;
		}

		if (this.QueryResults.Length != 0)
		{
			CleanAllCheckBoxes();

			this.flowLayoutPanelMain.SuspendLayout();
			try
			{
				// Reset to QueryResult.
				foreach (var lb in this.CachedCheckedListBoxes)
				{
					var (_, tag) = lb.GetBoxTag();

					var DSvsQR =
						// Does boxitems match queryresult ?
						from boxitem in tag.DataSource
						from boxitemresult in boxitem.MatchingResults
						join QR in this.QueryResults on boxitemresult equals QR
						// distinct boxitems
						group boxitem by boxitem into grp
						select grp;

					lb.BeginUpdate();
					foreach (var item in DSvsQR)
					{
						// Use HashSet for O(1) lookup instead of O(n) List.Contains
						var isChecked = this.SelectedFiltersSet.Contains(item.Key);

						if (!isChecked // Do not hide already checked
							/// Should i filter out this check box based on <see cref="FilterCategories"/> ?
							&& this.FilterCategories.AvoidDisplayAttribute(item.Key.DisplayValue)
						) continue;

						lb.Items.Add(item.Key, isChecked);
					}
					lb.EndUpdate();
				}
			}
			finally
			{
				this.flowLayoutPanelMain.ResumeLayout(true);
			}
		}
	}

	private void scalingButtonReset_Click(object? sender, EventArgs e)
		=> ResetSelectedFilters();

	/// <summary>
	/// Clears the selection, the search term and the category filter, and shows every category entry again
	/// (also when the categories were reduced to the previous results).
	/// </summary>
	private void ResetSelectedFilters()
	{
		this._Apply_SelectedFiltersDisabled = true;
		try
		{
			this.scalingComboBoxQueryList.ResetText();
			ResetQuickFilters();
			ResetRequirements();
			UncheckCategories(flowLayoutPanelMain);
			TextBoxSearchTerm_UpdateText_Notrigger(string.Empty);
			ClearFilterCategories();
		}
		finally
		{
			this._Apply_SelectedFiltersDisabled = false;
		}

		// Recompute the (now empty) selection; with nothing selected the categories return to their full content.
		TextBoxSearchTerm_TextChanged_Logic();
		ResetCheckBoxesToFirstLoad();
	}

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
		scalingCheckBoxHavingCharm.Checked =
		scalingCheckBoxHavingRelic.Checked =
		scalingCheckBoxHavingPrefix.Checked =
		scalingCheckBoxHavingSuffix.Checked = false;
	}

	internal void ResetRequirements()
	{
		scalingCheckBoxMaxReq.Checked =
		scalingCheckBoxMinReq.Checked = false;
		numericUpDownMaxDex.Value =
		numericUpDownMaxInt.Value =
		numericUpDownMaxLvl.Value =
		numericUpDownMaxStr.Value =
		numericUpDownMinDex.Value =
		numericUpDownMinInt.Value =
		numericUpDownMinLvl.Value =
		numericUpDownMinStr.Value = 0;
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

	private void typeAssistantFilterCategories_Idled(object? sender, EventArgs e)
	{
		var f = FilterCategories;

		f.SearchRawPrevious = f.SearchRaw;
		f.SearchRaw = this.scalingTextBoxFilterCategories.Text;
		f.HasFilterCategory = !string.IsNullOrWhiteSpace(f.SearchRaw);

		if (f.HasFilterCategory)
			(f.IsRegex, f.Search, f.Regex, f.RegexIsValid)
				= StringHelper.IsTQVaultSearchRegEx(f.SearchRaw);

		/// Wrapped into an Invoke() because i'm currently in the thread of the
		/// <see cref="typeAssistantFilterCategories"> and i need  <see cref="ApplyCategoriesReducer"/> to be executed from the main thread to avoid concurrent access exception
		/// Check IsHandleCreated and !IsDisposed to avoid InvalidOperationException when the form is initializing or disposing
		if (this.IsHandleCreated && !this.IsDisposed)
			this.Invoke(ApplyCategoriesReducer);
	}

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
		DialogResult? overrideIt = null;
		SearchQuery? foundIt = null;

		#region Validation

		// You must have text
		if (string.IsNullOrWhiteSpace(input))
		{
			MessageBox.Show(
				Resources.SearchQueryNameMustBeSet
				, Resources.GlobalInputWarning
				, MessageBoxButtons.OK
				, MessageBoxIcon.Error
				, MessageBoxDefaultButton.Button1
				, RightToLeftOptions
			);
			return;
		}

		// You must have filters
		if (!HaveFilters())
		{
			MessageBox.Show(
				Resources.SearchTermRequired
				, Resources.GlobalInputWarning
				, MessageBoxButtons.OK
				, MessageBoxIcon.Error
				, MessageBoxDefaultButton.Button1
				, RightToLeftOptions
			);
			return;
		}

		// Name conflict
		foundIt = this.SQueries.FirstOrDefault(q => q.QueryName.Equals(input, StringComparison.OrdinalIgnoreCase));
		if (foundIt != null)
		{
			overrideIt = MessageBox.Show(
				Resources.SearchQueryNameAlreadyExist
				, Resources.GlobalInputWarning
				, MessageBoxButtons.YesNo
				, MessageBoxIcon.Warning
				, MessageBoxDefaultButton.Button1
				, RightToLeftOptions
			);

			if (overrideIt == DialogResult.No)
				return;
		}

		#endregion

		if (overrideIt == DialogResult.Yes && foundIt is not null)
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
				;
		}
	}

	private void SearchQueriesInit()
	{
		scalingComboBoxQueryList.BeginUpdate();
		scalingComboBoxQueryList.Items.Clear();
		scalingComboBoxQueryList.Items.AddRange(this.SQueries.ToArray());
		scalingComboBoxQueryList.EndUpdate();
	}

	private void scalingButtonQueryDelete_Click(object? sender, EventArgs e)
	{
		var idx = scalingComboBoxQueryList.SelectedIndex;
		if (idx == -1) return;

		var item = scalingComboBoxQueryList.SelectedItem;

		var deleteIt = MessageBox.Show(
			string.Format(Resources.GlobalDeleteConfirm, item)
			, Resources.GlobalInputWarning
			, MessageBoxButtons.YesNo
			, MessageBoxIcon.Question
			, MessageBoxDefaultButton.Button1
			, RightToLeftOptions
		);

		if (deleteIt == DialogResult.No)
			return;


		scalingComboBoxQueryList.Items.RemoveAt(idx);
		this.SQueries.RemoveAt(idx);

		SavePersonnalQueries();
	}

	private void scalingComboBoxQueryList_SelectedIndexChanged(object? sender, EventArgs e)
	{
		var idx = scalingComboBoxQueryList.SelectedIndex;
		Make_SelectedFilters(this.SQueries[idx]);
	}

	private void Make_SelectedFilters(SearchQuery searchQuery)
	{
		// Make _SelectedFilters from saved query
		this.SelectedFilters.Clear();
		this.SelectedFiltersSet.Clear();

		this.SelectedFilters.AddRange(searchQuery.CheckedItems);

		// Populate HashSet for O(1) lookups
		foreach (var filter in this.SelectedFilters)
			this.SelectedFiltersSet.Add(filter);

		ResetCheckBoxesToFirstLoad();

		// Restore SearchTerm ?
		var term = searchQuery.CheckedItems.FirstOrDefault(i => i.CheckedList is null);

		/// Avoid trigger of <see cref="scalingTextBoxSearchTerm_TextChanged"/>
		if (term is null)
			TextBoxSearchTerm_UpdateText_Notrigger(string.Empty);
		else
		{
			TextBoxSearchTerm_UpdateText_Notrigger(term.DisplayValue);
			this.scalingTextBoxSearchTerm.Tag = term;
		}

		MakeSearchTermBoxItem();

		_Apply_SelectedFiltersDisabled = true;// Prevent filter before ui is fully init

		(// Find category visibility differences and apply
			from map in this.NavMap
			join vis in searchQuery.Visible on map.Button.Name equals vis.Name
			where map.Panel.Visible != vis.Visible
			select map
		).ToList().ForEach(map => map.Button.PerformClick());

		// Requirements
		this.numericUpDownMaxStr.Value = searchQuery.MaxStr;
		this.numericUpDownMaxDex.Value = searchQuery.MaxDex;
		this.numericUpDownMaxInt.Value = searchQuery.MaxInt;
		this.numericUpDownMinStr.Value = searchQuery.MinStr;
		this.numericUpDownMinDex.Value = searchQuery.MinDex;
		this.numericUpDownMinInt.Value = searchQuery.MinInt;
		this.numericUpDownMaxLvl.Value = searchQuery.MaxLvl;
		this.numericUpDownMinLvl.Value = searchQuery.MinLvl;
		this.scalingCheckBoxMinReq.Checked = searchQuery.MinRequirement;
		this.scalingCheckBoxMaxReq.Checked = searchQuery.MaxRequirement;

		// Quick Filters
		this.scalingCheckBoxHavingPrefix.Checked = searchQuery.HavingPrefix;
		this.scalingCheckBoxHavingSuffix.Checked = searchQuery.HavingSuffix;
		this.scalingCheckBoxHavingRelic.Checked = searchQuery.HavingRelic;
		this.scalingCheckBoxHavingCharm.Checked = searchQuery.HavingCharm;
		this.scalingCheckBoxIsSetItem.Checked = searchQuery.IsSetItem;

		// UI Behaviors
		this.numericUpDownMaxElement.Value = searchQuery.MaxElement;
		this.scalingComboBoxOperator.SelectedIndex = (int)searchQuery.Logic;
		this.scalingTextBoxFilterCategories.Text = searchQuery.Filter;
		this.scalingCheckBoxReduceDuringSelection.Checked = searchQuery.Reduce;

		_Apply_SelectedFiltersDisabled = false;

		this.Apply_SelectedFilters();
	}

	private void scalingCheckBoxMinMaxReq_CheckedChanged(object? sender, EventArgs e)
	{
		Apply_SelectedFilters();
	}

	private void numericUpDownMinMax_ValueChanged(object? sender, EventArgs e)
	{
		var num = (NumericUpDown)sender!;

		// Avoid absurd range
		if (num == numericUpDownMinLvl && numericUpDownMaxLvl.Value < num.Value)
			numericUpDownMaxLvl.Value = num.Value;

		else if (num == numericUpDownMaxLvl && numericUpDownMinLvl.Value > num.Value)
			numericUpDownMinLvl.Value = num.Value;

		else if (num == numericUpDownMinStr && numericUpDownMaxStr.Value < num.Value)
			numericUpDownMaxStr.Value = num.Value;

		else if (num == numericUpDownMaxStr && numericUpDownMinStr.Value > num.Value)
			numericUpDownMinStr.Value = num.Value;

		else if (num == numericUpDownMinDex && numericUpDownMaxDex.Value < num.Value)
			numericUpDownMaxDex.Value = num.Value;

		else if (num == numericUpDownMaxDex && numericUpDownMinDex.Value > num.Value)
			numericUpDownMinDex.Value = num.Value;

		else if (num == numericUpDownMinInt && numericUpDownMaxInt.Value < num.Value)
			numericUpDownMaxInt.Value = num.Value;

		else if (num == numericUpDownMaxInt && numericUpDownMinInt.Value > num.Value)
			numericUpDownMinInt.Value = num.Value;

		if (// No need to filter if corresponding checkbox is uncheck
			(num.Name.Contains("Min") && !this.scalingCheckBoxMinReq.Checked)
			|| (num.Name.Contains("Max") && !this.scalingCheckBoxMaxReq.Checked)
		) return;

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