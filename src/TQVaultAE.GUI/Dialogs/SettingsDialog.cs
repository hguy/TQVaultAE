//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using EnumsNET;
using TQVaultAE.GUI.Infrastructure;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Class for the Settings/Configuration Dialog
/// </summary>
internal sealed partial class SettingsDialog : VaultForm, IScalingControl
{
	#region Properties & Fields

	public string BaseFont { get; private set; } = string.Empty;

	/// <summary>
	/// Indicates whether the last opened vault will be loaded at startup
	/// </summary>
	private bool loadLastVault;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableTQVaultSounds"/> setting has been changed
	/// </summary>
	public bool enableTQVaultSounds;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableItemRequirementRestriction"/> setting has been changed
	/// </summary>
	public bool enableItemRequirementRestriction;

	/// <summary>
	/// Indicates whether the title screen will be skipped on startup
	/// </summary>
	private bool skipTitle;

	/// <summary>
	/// Save path for the vault files
	/// </summary>
	public string VaultPath { get; private set; } = string.Empty;

	/// <summary>
	/// Indicates whether item copying is allowed
	/// </summary>
	private bool allowItemCopy;

	/// <summary>
	/// Indicates whether item editing is allowed
	/// </summary>
	private bool allowItemEdit;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableEpicLegendaryAffixes"/> setting has been changed
	/// </summary>
	public bool EnableEpicLegendaryAffixes;

	/// <summary>
	/// Indicates whether character editing is allowed
	/// </summary>
	private bool allowCharacterEdit;

	/// <summary>
	/// Indicates whether the last opened character will be loaded at startup
	/// </summary>
	private bool loadLastCharacter;

	/// <summary>
	/// Indicates whether the language will be auto detected
	/// </summary>
	private bool detectLanguage;

	/// <summary>
	/// Activate the alternative Tooltip display
	/// </summary>
	private bool enableDetailedTooltipView;
	private bool showInGameValues;
	private bool tooltipShowInGameRolls;
	private bool tooltipShowValueRanges;
	private bool tooltipShowGrantedSkills;
	private bool tooltipShowSetPieceCounts;

	/// <summary>
	/// Value Range (0-255) for item background color opacity
	/// </summary>
	private int itemBGColorOpacity;

	/// <summary>
	/// The language we will be using.
	/// </summary>
	private string titanQuestLanguage = string.Empty;

	/// <summary>
	/// Indicates whether the game paths will be auto detected
	/// </summary>
	private bool detectGamePath;

	/// <summary>
	/// Titan Quest game path
	/// </summary>
	private string titanQuestPath = string.Empty;

	/// <summary>
	/// Immortal Throne game path
	/// </summary>
	private string immortalThronePath = string.Empty;

	/// <summary>
	/// Indicates whether custom maps have been enabled
	/// </summary>
	private bool enableMods;

	/// <summary>
	/// Current custom map
	/// </summary>
	private string customMap = string.Empty;

	/// <summary>
	/// Indicates whether we are loading all data files on startup
	/// </summary>
	private bool loadAllFiles;

	/// <summary>
	/// Indicates whether warning messages are suppressed
	/// </summary>
	private bool suppressWarnings;

	/// <summary>
	/// Indicates whether player items will be readonly.
	/// </summary>
	private bool playerReadonly;

	/// <summary>
	/// Indicates whether the vault path has been changed
	/// </summary>
	public bool VaultPathChanged { get; private set; }

	/// <summary>
	/// Indicates whether the play list filter has been changed
	/// </summary>
	public bool PlayerFilterChanged { get; private set; }

	/// <summary>
	/// Indicates that any configuration item has been changed
	/// </summary>
	public bool ConfigurationChanged { get; private set; }

	/// <summary>
	/// Indicates that the UI setting has changed.
	/// </summary>
	public bool UISettingChanged { get; private set; }

	/// <summary>
	/// Indicates that the settings have been loaded
	/// </summary>
	private bool settingsLoaded;

	/// <summary>
	/// Indicates that the ItemBGColorOpacity setting has been changed
	/// </summary>
	public bool ItemBGColorOpacityChanged { get; private set; }

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableItemRequirementRestriction"/> setting has been changed
	/// </summary>
	public bool EnableItemRequirementRestrictionChanged { get; private set; }

	/// <summary>
	/// Indicates that the allow character edit setting has been changed.
	/// </summary>
	public bool EnableCharacterEditChanged { get; private set; }

	/// <summary>
	/// Enale the hot reload feature
	/// </summary>
	private bool enableHotReload;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableHotReload"/> setting has been changed
	/// </summary>
	public bool EnableHotReloadChanged { get; private set; }

	/// <summary>
	/// Indicates that the language setting has been changed
	/// </summary>
	public bool LanguageChanged { get; private set; }

	/// <summary>
	/// Indicates that the game language has been changed
	/// </summary>
	public bool GamePathChanged { get; private set; }

	/// <summary>
	/// Indicates that the custom map selection has changed
	/// </summary>
	public bool CustomMapsChanged { get; private set; }

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableTQVaultSounds"/> setting has been changed
	/// </summary>
	public bool EnableTQVaultSoundsChanged { get; private set; }

	/// <summary>
	/// Disable auto stacking last value
	/// </summary>
	private bool disableAutoStacking;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.DisableAutoStacking"/> setting has been changed
	/// </summary>
	public bool DisableAutoStackingChanged { get; private set; }

	/// <summary>
	/// AllowCheats last value
	/// </summary>
	private bool allowCheats;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.AllowCheats"/> setting has been changed
	/// </summary>
	public bool AllowCheatsChanged { get; private set; }

	/// <summary>
	/// enableGitBackup last value
	/// </summary>
	private bool enableGitBackup;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.GitBackupEnabled"/> setting has been changed
	/// </summary>
	public bool GitBackupEnabledChanged { get; private set; }

	/// <summary>
	/// gitBackupRepository last value
	/// </summary>
	private string gitBackupRepository = string.Empty;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.GitBackupRepository"/> setting has been changed
	/// </summary>
	public bool GitBackupRepositoryChanged { get; private set; }

	/// <summary>
	/// disableLegacyBackup last value
	/// </summary>
	private bool disableLegacyBackup;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.DisableLegacyBackup"/> setting has been changed
	/// </summary>
	public bool DisableLegacyBackupChanged { get; private set; }

	/// <summary>
	/// enableBackupPlayerSaves last value
	/// </summary>
	private bool enableBackupPlayerSaves;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.GitBackupPlayerSavesEnabled"/> setting has been changed
	/// </summary>
	public bool EnableBackupPlayerSavesChanged { get; private set; }

	/// <summary>
	/// enableOriginalTQSupport last value
	/// </summary>
	private bool enableOriginalTQSupport;
	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableOriginalTQSupport"/> setting has been changed
	/// </summary>
	public bool EnableOriginalTQSupportChanged { get; private set; }

	/// <summary>
	/// PasteBin API Key
	/// </summary>
	private string pasteBinApiKey = string.Empty;

	/// <summary>
	/// PasteBin paste expiration
	/// </summary>
	private string pasteBinExpiration = string.Empty;

	/// <summary>
	/// Indicates that the PasteBin settings have been changed
	/// </summary>
	public bool PasteBinSettingsChanged { get; private set; }

	private static readonly Dictionary<string, string> PasteBinExpirationOptions = new()
	{
		["10M"] = "10 Minutes",
		["1H"] = "1 Hour",
		["1D"] = "1 Day",
		["1W"] = "1 Week",
		["2W"] = "2 Weeks",
		["1M"] = "1 Month",
		["6M"] = "6 Months",
		["1Y"] = "1 Year",
		["N"] = "Never"
	};

	private sealed record PasteBinExpirationComboBoxItem(string Key, string Label)
	{
		public override string ToString() => this.Label;
	}

	private readonly ILogger<SettingsDialog> Log;

	#endregion

	/// <summary>
	/// Initializes a new instance of the SettingsDialog class.
	/// </summary>
	public SettingsDialog(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;
		this.Log = instance.Services.Logger<SettingsDialog>();

		this.InitializeComponent();

		// The dialog follows its inner table: the font, a translation, a group shown or a monitor of another DPI change it.
		this.bufferedTableLayoutPanelSkeleton.SizeChanged += (_, _) => this.FitToContent();
		this.FitToContent();

		foreach (var kvp in PasteBinExpirationOptions)
		{
			this.pasteBinExpirationComboBox.Items.Add(new PasteBinExpirationComboBoxItem(kvp.Key, kvp.Value));
		}

		this.SuspendLayout();

		//this.ShowResizeBorders = false;
		// As in the other dialogs: the zoom reset of this button belongs to the main window, here it reset the
		// zoom of the whole application without resizing the main window.
		this.NormalizeBox = false;
		//this.DrawCustomBorder = true;
		//this.ResizeCustomAllowed = false;
		//this.ScaleOnResize = false;

		this.linkLabelTQOriginalSupport.LinkColor = TQColor.Aqua.Color();

		#region Apply custom font

		var font12 = FontService.GetFontLight(12F);
		var font1125 = FontService.GetFontLight(11.25F);

		this.cancelButton.Font =
		this.okayButton.Font =
		this.resetButton.Font =
		this.vaultPathBrowseButton.Font =
		this.titanQuestPathBrowseButton.Font =
		this.immortalThronePathBrowseButton.Font = font12;

		this.hotReloadCheckBox.Font =
		this.scalingCheckBoxEnableSounds.Font =
		this.characterEditCheckBox.Font =
		this.allowItemEditCheckBox.Font =
		this.allowItemCopyCheckBox.Font =
		this.skipTitleCheckBox.Font =
		this.loadLastCharacterCheckBox.Font =
		this.loadLastVaultCheckBox.Font =
		this.vaultPathTextBox.Font =
		this.vaultPathLabel.Font =
		this.enableCustomMapsCheckBox.Font =
		this.loadAllFilesCheckBox.Font =
		this.suppressWarningsCheckBox.Font =
		this.playerReadonlyCheckbox.Font =
		this.languageComboBox.Font =
		this.languageLabel.Font =
		this.detectLanguageCheckBox.Font =
		this.titanQuestPathTextBox.Font =
		this.titanQuestPathLabel.Font =
		this.immortalThronePathLabel.Font =
		this.immortalThronePathTextBox.Font =
		this.detectGamePathsCheckBox.Font =
		this.customMapLabel.Font =
		this.mapListComboBox.Font =
		this.baseFontLabel.Font =
		this.baseFontComboBox.Font =
		this.scalingCheckBoxTooltipInGameRolls.Font =
		this.scalingCheckBoxTooltipValueRanges.Font =
		this.scalingCheckBoxTooltipGrantedSkills.Font =
		this.scalingCheckBoxTooltipSetPieceCounts.Font =
		this.ShowInGameValuesCheckBox.Font =
		this.EnableItemRequirementRestrictionCheckBox.Font =
		this.ItemBGColorOpacityLabel.Font =
		this.scalingLabelCSVDelim.Font =
		this.scalingComboBoxCSVDelim.Font =
		this.scalingCheckBoxEnableEpicLegendaryAffixes.Font =
		this.scalingCheckBoxDisableAutoStacking.Font = font1125;

		this.checkGroupBoxGitBackup.ProcessAllControls((ctr) => ctr.Font = font1125);
		this.checkGroupBoxOriginalTQSupport.ProcessAllControls((ctr) => ctr.Font = font1125);

		this.Font = font1125;

		#endregion

		this.vaultPathLabel.Text = Resources.SettingsLabel1;
		this.languageLabel.Text = Resources.SettingsLabel2;
		this.titanQuestPathLabel.Text = Resources.SettingsLabel3;
		this.immortalThronePathLabel.Text = Resources.SettingsLabel4;
		this.customMapLabel.Text = Resources.SettingsLabel5;
		this.detectGamePathsCheckBox.Text = Resources.SettingsDetectGamePath;
		this.detectLanguageCheckBox.Text = Resources.SettingsDetectLanguage;
		this.enableCustomMapsCheckBox.Text = Resources.SettingsEnableMod;
		this.toolTip.SetToolTip(this.enableCustomMapsCheckBox, Resources.SettingsEnableModTT);
		this.skipTitleCheckBox.Text = Resources.SettingsSkipTitle;
		this.toolTip.SetToolTip(this.skipTitleCheckBox, Resources.SettingsSkipTitleTT);
		this.allowItemCopyCheckBox.Text = Resources.SettingsAllowCopy;
		this.toolTip.SetToolTip(this.allowItemCopyCheckBox, Resources.SettingsAllowCopyTT);
		this.allowItemEditCheckBox.Text = Resources.SettingsAllowEdit;
		this.toolTip.SetToolTip(this.allowItemEditCheckBox, Resources.SettingsAllowEdit);
		this.characterEditCheckBox.Text = Resources.SettingsAllowEditCE;
		this.toolTip.SetToolTip(this.characterEditCheckBox, Resources.SettingsAllowEditCE);
		this.loadLastCharacterCheckBox.Text = Resources.SettingsLoadChar;
		this.toolTip.SetToolTip(this.loadLastCharacterCheckBox, Resources.SettingsLoadCharTT);
		this.loadLastVaultCheckBox.Text = Resources.SettingsLoadVault;
		this.toolTip.SetToolTip(this.loadLastVaultCheckBox, Resources.SettingsLoadVaultTT);
		this.loadAllFilesCheckBox.Text = Resources.SettingsPreLoad;
		this.toolTip.SetToolTip(this.loadAllFilesCheckBox, Resources.SettingsPreLoadTT);
		this.suppressWarningsCheckBox.Text = Resources.SettingsNoWarning;
		this.toolTip.SetToolTip(this.suppressWarningsCheckBox, Resources.SettingsNoWarningTT);
		this.playerReadonlyCheckbox.Text = Resources.SettingsPlayerReadonly;
		this.toolTip.SetToolTip(this.playerReadonlyCheckbox, Resources.SettingsPlayerReadonlyTT);
		this.resetButton.Text = Resources.SettingsReset;
		this.toolTip.SetToolTip(this.resetButton, Resources.SettingsResetTT);
		this.checkGroupBoxTooltipEnhanced.Text = Resources.SettingTooltipEnhanced;
		this.toolTip.SetToolTip(this.checkGroupBoxTooltipEnhanced, Resources.SettingTooltipEnhancedTT);
		this.scalingCheckBoxTooltipInGameRolls.Text = Resources.SettingTooltipShowInGameRolls;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipInGameRolls, Resources.SettingTooltipShowInGameRollsTT);
		this.scalingCheckBoxTooltipValueRanges.Text = Resources.SettingTooltipShowValueRanges;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipValueRanges, Resources.SettingTooltipShowValueRangesTT);
		this.scalingCheckBoxTooltipGrantedSkills.Text = Resources.SettingTooltipShowGrantedSkills;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipGrantedSkills, Resources.SettingTooltipShowGrantedSkillsTT);
		this.scalingCheckBoxTooltipSetPieceCounts.Text = Resources.SettingTooltipShowSetPieceCounts;
		this.toolTip.SetToolTip(this.scalingCheckBoxTooltipSetPieceCounts, Resources.SettingTooltipShowSetPieceCountsTT);
		this.ShowInGameValuesCheckBox.Text = Resources.SettingShowInGameValues;
		this.toolTip.SetToolTip(this.ShowInGameValuesCheckBox, Resources.SettingShowInGameValuesTT);
		this.ItemBGColorOpacityLabel.Text = Resources.SettingsItemBGColorOpacityLabel;
		this.toolTip.SetToolTip(this.ItemBGColorOpacityLabel, Resources.SettingsItemBGColorOpacityLabelTT);
		this.EnableItemRequirementRestrictionCheckBox.Text = Resources.SettingsEnableItemRequirementRestriction;
		this.toolTip.SetToolTip(this.EnableItemRequirementRestrictionCheckBox, Resources.SettingsEnableItemRequirementRestrictionTT);
		this.hotReloadCheckBox.Text = Resources.SettingsEnableHotReload;
		this.toolTip.SetToolTip(this.hotReloadCheckBox, Resources.SettingsEnableHotReloadTT);

		this.scalingLabelCSVDelim.Text = Resources.SettingsCsvDelimiterLabel;

		this.scalingCheckBoxEnableEpicLegendaryAffixes.Text = Resources.SettingsEnableEpicLegendaryAffixes;
		this.toolTip.SetToolTip(this.scalingCheckBoxEnableEpicLegendaryAffixes, Resources.SettingsEnableEpicLegendaryAffixesTT);

		this.scalingCheckBoxDisableAutoStacking.Text = Resources.SettingsDisableAutoStacking;
		this.toolTip.SetToolTip(this.scalingCheckBoxDisableAutoStacking, Resources.SettingsDisableAutoStackingTT);

		this.checkGroupBoxAllowCheats.Text = Resources.SettingsAllowCheats;
		this.scalingCheckBoxDisableLegacyBackup.Text = Resources.SettingsDisableLegacyBackup;
		this.toolTip.SetToolTip(this.scalingCheckBoxDisableLegacyBackup, Resources.SettingsDisableLegacyBackupTT);
		this.scalingLabelGitRepository.Text = Resources.SettingsGitRepositoryUrl;

		this.scalingCheckBoxBackupPlayerSaves.Text = Resources.SettingsBackupPlayerSaves;
		this.toolTip.SetToolTip(this.scalingCheckBoxBackupPlayerSaves, Resources.SettingsBackupPlayerSavesTT);

		this.checkGroupBoxOriginalTQSupport.Text = Resources.SettingsOriginalTQSupport;
		this.toolTip.SetToolTip(this.checkGroupBoxOriginalTQSupport, Resources.SettingsOriginalTQSupportTT);
		this.linkLabelTQOriginalSupport.Text = Resources.SettingsHowToPlay;

		this.cancelButton.Text = Resources.GlobalCancel;
		this.okayButton.Text = Resources.GlobalOK;
		this.Text = Resources.SettingsTitle;

		this.mapListComboBox.Items.Clear();

		var maps = GamePathResolver.GetCustomMapList();

		if (maps is { Length: > 0 })
			this.mapListComboBox.Items.AddRange(maps);

		this.ResumeLayout(false);
		this.PerformLayout();
	}

	/// <summary>
	/// Override of ScaleControl which supports font scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		base.ScaleControl(factor, specified);
	}

	/// <summary>
	/// Handler for loading this dialog box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SettingsDialogLoad(object? sender, EventArgs e)
	{
		this.settingsLoaded = false;

		this.LoadSettings();
		this.UpdateDialogSettings();
	}

	/// <summary>
	/// Loads the settings from the config file
	/// </summary>
	private void LoadSettings()
	{
		this.BaseFont = USettings.BaseFont;

		this.skipTitle = USettings.SkipTitle;
		this.VaultPath = USettings.VaultPath;
		this.allowItemCopy = USettings.AllowItemCopy;
		this.allowItemEdit = USettings.AllowItemEdit;
		this.allowCharacterEdit = USettings.AllowCharacterEdit;
		this.loadLastCharacter = USettings.LoadLastCharacter;
		this.loadLastVault = USettings.LoadLastVault;
		this.detectLanguage = USettings.AutoDetectLanguage;
		this.enableDetailedTooltipView = USettings.EnableDetailedTooltipView;
		this.showInGameValues = USettings.ShowInGameValues;
		this.tooltipShowInGameRolls = USettings.TooltipShowInGameRolls;
		this.tooltipShowValueRanges = USettings.TooltipShowValueRanges;
		this.tooltipShowGrantedSkills = USettings.TooltipShowGrantedSkills;
		this.tooltipShowSetPieceCounts = USettings.TooltipShowSetPieceCounts;
		this.itemBGColorOpacity = USettings.ItemBGColorOpacity;
		this.enableItemRequirementRestriction = USettings.EnableItemRequirementRestriction;
		this.enableHotReload = USettings.EnableHotReload;
		this.enableTQVaultSounds = USettings.EnableTQVaultSounds;
		this.EnableEpicLegendaryAffixes = USettings.EnableEpicLegendaryAffixes;
		this.disableAutoStacking = USettings.DisableAutoStacking;
		this.allowCheats = USettings.AllowCheats;
		this.enableGitBackup = USettings.GitBackupEnabled;
		this.disableLegacyBackup = USettings.DisableLegacyBackup;
		this.gitBackupRepository = USettings.GitBackupRepository;
		this.enableBackupPlayerSaves = USettings.GitBackupPlayerSavesEnabled;
		this.enableOriginalTQSupport = USettings.EnableOriginalTQSupport;
		this.pasteBinApiKey = USettings.PasteBinApiKey ?? string.Empty;
		this.pasteBinExpiration = USettings.PasteBinExpiration ?? UserSettings.DefaultPasteBinExpiration;

		// Force English since there was some issue with getting the proper language setting.
		var gl = Database.GameLanguage;
		this.titanQuestLanguage = gl == null ? "English" : gl;

		this.detectGamePath = USettings.AutoDetectGamePath;
		this.titanQuestPath = GamePathResolver.GamePathTQ;
		this.immortalThronePath = GamePathResolver.GamePathTQIT;
		this.enableMods = USettings.ModEnabled;
		this.customMap = USettings.CustomMap;
		this.loadAllFiles = USettings.LoadAllFiles;
		this.suppressWarnings = USettings.SuppressWarnings;
		this.playerReadonly = USettings.PlayerReadonly;

		this.settingsLoaded = true;
		this.ConfigurationChanged = false;
		this.VaultPathChanged = false;
		this.PlayerFilterChanged = false;
		this.LanguageChanged = false;
		this.GamePathChanged = false;
		this.UISettingChanged = false;
	}

	/// <summary>
	/// Updates the dialog settings display
	/// </summary>
	private void UpdateDialogSettings()
	{
		// Check to see that we can update things
		if (!this.settingsLoaded)
			return;

		this.SuspendLayout();

		// Build language combo box
		this.languageComboBox.Items.Clear();

		// Read the languages from the config file
		ComboBoxItem[] languages = USettings.GameLanguages.Split(',').Select(iso =>
		{
			CultureInfo ci = new CultureInfo(iso.ToUpperInvariant(), true);
			return new ComboBoxItem() { Value = ci.EnglishName, DisplayName = ci.DisplayName };// to keep EnglishName as baseline value
		}).OrderBy(cb => cb.DisplayName).ToArray();

		// Reading failed so we default to English
		if (languages.Length == 0) languages = new ComboBoxItem[] { new ComboBoxItem() { Value = "English", DisplayName = "English" } };

		this.languageComboBox.Items.AddRange(languages);

		this.vaultPathTextBox.Text = this.VaultPath;
		this.skipTitleCheckBox.Checked = this.skipTitle;
		this.allowItemEditCheckBox.Checked = this.allowItemEdit;
		this.allowItemCopyCheckBox.Checked = this.allowItemCopy;
		this.characterEditCheckBox.Checked = this.allowCharacterEdit;
		this.loadLastCharacterCheckBox.Checked = this.loadLastCharacter;
		this.loadLastVaultCheckBox.Checked = this.loadLastVault;
		this.detectLanguageCheckBox.Checked = this.detectLanguage;
		this.detectGamePathsCheckBox.Checked = this.detectGamePath;
		this.immortalThronePathTextBox.Text = this.immortalThronePath;
		this.immortalThronePathTextBox.Enabled = !this.detectGamePath;
		this.immortalThronePathBrowseButton.Enabled = !this.detectGamePath;
		this.titanQuestPathTextBox.Text = this.titanQuestPath;
		this.titanQuestPathTextBox.Enabled = !this.detectGamePath;
		this.titanQuestPathBrowseButton.Enabled = !this.detectGamePath;
		this.loadAllFilesCheckBox.Checked = this.loadAllFiles;
		this.suppressWarningsCheckBox.Checked = this.suppressWarnings;
		this.playerReadonlyCheckbox.Checked = this.playerReadonly;
		this.checkGroupBoxTooltipEnhanced.Checked = this.enableDetailedTooltipView;
		this.scalingCheckBoxTooltipInGameRolls.Checked = this.tooltipShowInGameRolls;
		this.scalingCheckBoxTooltipValueRanges.Checked = this.tooltipShowValueRanges;
		this.scalingCheckBoxTooltipGrantedSkills.Checked = this.tooltipShowGrantedSkills;
		this.scalingCheckBoxTooltipSetPieceCounts.Checked = this.tooltipShowSetPieceCounts;
		this.ShowInGameValuesCheckBox.Checked = this.showInGameValues;
		this.ItemBGColorOpacityTrackBar.Value = this.itemBGColorOpacity;
		this.EnableItemRequirementRestrictionCheckBox.Checked = this.enableItemRequirementRestriction;
		this.hotReloadCheckBox.Checked = this.enableHotReload;
		this.scalingCheckBoxEnableSounds.Checked = this.enableTQVaultSounds;
		this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked = this.EnableEpicLegendaryAffixes;
		this.scalingCheckBoxDisableAutoStacking.Checked = this.disableAutoStacking;
		this.checkGroupBoxAllowCheats.Checked = this.allowCheats;
		this.checkGroupBoxGitBackup.Checked = this.enableGitBackup;
		this.scalingCheckBoxDisableLegacyBackup.Checked = this.disableLegacyBackup;
		this.scalingTextBoxGitRepository.Text = this.gitBackupRepository;
		this.scalingCheckBoxBackupPlayerSaves.Checked = this.enableBackupPlayerSaves;
		this.checkGroupBoxOriginalTQSupport.Checked = this.enableOriginalTQSupport;

		this.pasteBinApiKeyTextBox.Text = this.pasteBinApiKey;

		PasteBinExpirationOptions.TryGetValue(this.pasteBinExpiration, out var expLabel);
		if (expLabel != null)
		{
			this.pasteBinExpirationComboBox.SelectedItem = this.pasteBinExpirationComboBox.Items
				.OfType<PasteBinExpirationComboBoxItem>()
				.FirstOrDefault(i => i.Key == this.pasteBinExpiration);
		}

		this.enableCustomMapsCheckBox.Checked = this.enableMods;

		var lst = this.mapListComboBox.Items.Cast<GamePathEntry>();
		var found = lst.Where(m => m.Path == this.customMap).FirstOrDefault();
		this.mapListComboBox.SelectedItem = found;

		this.mapListComboBox.Enabled = this.enableMods;

		this.languageComboBox.SelectedItem = languages.FirstOrDefault(cb => cb.Value.Equals(this.titanQuestLanguage, StringComparison.OrdinalIgnoreCase));

		this.languageComboBox.Enabled = !this.detectLanguage;

		// Build Font combo box
		this.baseFontComboBox.Items.Clear();
		var listItem = Enums.GetMembers<FontFamilyList>()
			.Select(m => new ComboBoxItem()
			{
				Value = m.AsString(EnumFormat.Name) ?? string.Empty,
				DisplayName = m.AsString(EnumFormat.Description, EnumFormat.Name) ?? string.Empty
			}).ToArray();
		this.baseFontComboBox.Items.AddRange(listItem);
		this.baseFontComboBox.SelectedItem = listItem.Where(i => i.Value == this.BaseFont).FirstOrDefault() ?? listItem.First();

		// Build CsvDelim combo box
		this.scalingComboBoxCSVDelim.Items.Clear();
		var listItemDelim = Enums.GetMembers<CsvDelimiter>()
			.Select(m =>
			{
				var name = m.AsString(EnumFormat.Name);
				// Each delimiter has its character as description.
				var desc = m.AsString(EnumFormat.Description)!;
				return new ComboBoxItem<CsvDelimiter, char>()
				{
					ComboValue = m.Value,
					Value = desc.First(),
					DisplayName = $"{name} ({desc})"
				};
			}).ToArray();
		this.scalingComboBoxCSVDelim.Items.AddRange(listItemDelim);
		this.scalingComboBoxCSVDelim.SelectedItem = listItemDelim
			.Where(i => i.ComboValue.ToString() == USettings.CSVDelimiter)
			.FirstOrDefault() ?? listItemDelim.First();

		this.ResumeLayout(false);
		this.PerformLayout();
	}

	private void ItemBGColorOpacityTrackBar_Scroll(object? sender, EventArgs e)
	{
		this.itemBGColorOpacity = this.ItemBGColorOpacityTrackBar.Value;
		this.ConfigurationChanged = this.ItemBGColorOpacityChanged = true;
	}

	/// <summary>
	/// On a monitor of another DPI, Windows gives the window its suggested size after the layout was scaled, over the
	/// fit made while the table changed: fit again once it is applied.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		this.FitToContent();
	}

	/// <summary>
	/// Adjusts the size to the inner table, so a taller column does not push the buttons out. Not Form.AutoSize: the
	/// caption buttons of the custom border sit on the right edge and would keep the form from shrinking.
	/// </summary>
	private void FitToContent()
	{
		this.Width = this.bufferedTableLayoutPanelSkeleton.Location.X + this.bufferedTableLayoutPanelSkeleton.Width + this.Padding.Right + 5;
		this.Height = this.bufferedTableLayoutPanelSkeleton.Location.Y + this.bufferedTableLayoutPanelSkeleton.Height + this.Padding.Bottom + 5;
	}

	private void scalingTextBoxGitRepository_TextChanged(object? sender, EventArgs e)
	{
		// Changed
		if (this.gitBackupRepository != scalingTextBoxGitRepository.Text)
		{
			this.gitBackupRepository = scalingTextBoxGitRepository.Text;
			this.ConfigurationChanged = this.UISettingChanged = this.GitBackupRepositoryChanged = true;
		}
	}

	private void linkLabelTQOriginalSupport_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
	{
		ProcessStartInfo psInfo = new ProcessStartInfo
		{
			FileName = this.USettings.TQOriginalHowtoUrl,
			UseShellExecute = true
		};
		Process.Start(psInfo);
	}

	private void PasteBinApiKeyTextBoxTextChanged(object? sender, EventArgs e)
	{
		string newValue = this.pasteBinApiKeyTextBox.Text ?? string.Empty;
		if (newValue != this.pasteBinApiKey)
		{
			this.pasteBinApiKey = newValue;
			this.ConfigurationChanged = this.PasteBinSettingsChanged = true;
		}
	}
}