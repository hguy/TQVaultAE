//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.EventHandlers.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for the Settings/Configuration Dialog
/// </summary>

using System.Globalization;
using System.Text.RegularExpressions;
using EnumsNET;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

internal partial class SettingsDialog
{
	/// <summary>
	/// Handler for clicking the vault path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultPathBrowseButtonClick(object? sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseVault;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
		this.folderBrowserDialog.SelectedPath = this.VaultPath;
		this.folderBrowserDialog.ShowDialog();
		if (!this.folderBrowserDialog.SelectedPath.Trim().Equals(this.VaultPath.Trim(), StringComparison.OrdinalIgnoreCase))
		{
			this.VaultPath = this.folderBrowserDialog.SelectedPath.Trim();
			this.vaultPathTextBox.Text = this.VaultPath;
			this.vaultPathTextBox.Invalidate();
			this.ConfigurationChanged = true;
			this.VaultPathChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking the reset button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ResetButtonClick(object? sender, EventArgs e)
	{
		if (this.ConfigurationChanged)
		{
			this.LoadSettings();
			this.UpdateDialogSettings();
			this.Invalidate();
		}
	}

	/// <summary>
	/// Handler for clicking the skip title check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SkipTitleCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.skipTitleCheckBox.Checked)
		{
			if (!this.skipTitle)
			{
				this.ConfigurationChanged = this.skipTitle = true;
			}
		}
		else
		{
			if (this.skipTitle)
			{
				this.skipTitle = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the OK button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OkayButtonClick(object? sender, EventArgs e)
	{
		// Validation
		if (enableGitBackup)
		{
			if (string.IsNullOrEmpty(gitBackupRepository) || !GitUrlRegex().IsMatch(gitBackupRepository))
			{
				this.UIService.ShowError("You must enter a valid Git Url.", Buttons: Application.Contracts.Services.ShowMessageButtons.OK);
				this.DialogResult = DialogResult.None;
				return;
			}
		}

		if (this.ConfigurationChanged)
		{
			USettings.SkipTitle = this.skipTitle;
			USettings.VaultPath = this.VaultPath;
			USettings.AllowItemCopy = this.allowItemCopy;
			USettings.AllowItemEdit = this.allowItemEdit;
			USettings.AllowCharacterEdit = this.allowCharacterEdit;
			USettings.LoadLastCharacter = this.loadLastCharacter;
			USettings.LoadLastVault = this.loadLastVault;
			USettings.AutoDetectLanguage = this.detectLanguage;
			USettings.TQLanguage = this.titanQuestLanguage;
			USettings.AutoDetectGamePath = this.detectGamePath;
			USettings.TQITPath = this.immortalThronePath;
			USettings.TQPath = this.titanQuestPath;
			USettings.ModEnabled = this.enableMods;
			USettings.CustomMap = this.customMap;
			USettings.LoadAllFiles = this.loadAllFiles;
			USettings.SuppressWarnings = this.suppressWarnings;
			USettings.PlayerReadonly = this.playerReadonly;
			USettings.BaseFont = this.BaseFont;
			USettings.EnableDetailedTooltipView = this.enableDetailedTooltipView;
			USettings.ShowInGameValues = this.showInGameValues;
			USettings.TooltipShowInGameRolls = this.tooltipShowInGameRolls;
			USettings.TooltipShowValueRanges = this.tooltipShowValueRanges;
			USettings.TooltipShowGrantedSkills = this.tooltipShowGrantedSkills;
			USettings.TooltipShowSetPieceCounts = this.tooltipShowSetPieceCounts;
			USettings.ItemBGColorOpacity = this.itemBGColorOpacity;
			USettings.EnableItemRequirementRestriction = this.enableItemRequirementRestriction;
			USettings.EnableHotReload = this.enableHotReload;
			USettings.EnableTQVaultSounds = this.enableTQVaultSounds;
			USettings.DisableAutoStacking = this.disableAutoStacking;
			USettings.AllowCheats = this.allowCheats;
			USettings.GitBackupEnabled = this.enableGitBackup;
			USettings.DisableLegacyBackup = this.disableLegacyBackup;
			USettings.GitBackupRepository = this.gitBackupRepository;
			USettings.GitBackupPlayerSavesEnabled = this.enableBackupPlayerSaves;
			USettings.EnableOriginalTQSupport = this.enableOriginalTQSupport;

			USettings.PasteBinApiKey = this.pasteBinApiKey;
			USettings.PasteBinExpiration = this.pasteBinExpiration;

			USettings.EnableEpicLegendaryAffixes =
				this.scalingCheckBoxEnableEpicLegendaryAffixes.Enabled && this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked;

			if (this.scalingComboBoxCSVDelim.SelectedItem is ComboBoxItem<CsvDelimiter, char> delim)
				USettings.CSVDelimiter = delim.ComboValue.ToString();
		}

		this.DialogResult = DialogResult.OK;
	}

	/// <summary>
	/// Handler for clicking the allow item editing check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AllowItemEditCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.allowItemEditCheckBox.Checked)
		{
			if (!this.allowItemEdit)
			{
				this.ConfigurationChanged = this.allowItemEdit = true;
			}

			this.scalingCheckBoxEnableEpicLegendaryAffixes.Enabled = true;
		}
		else
		{
			if (this.allowItemEdit)
			{
				this.allowItemEdit = false;
				this.ConfigurationChanged = true;
			}

			this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked =
			this.scalingCheckBoxEnableEpicLegendaryAffixes.Enabled = false;
		}

	}

	/// <summary>
	/// Handler for clicking the allow item copy check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AllowItemCopyCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.allowItemCopyCheckBox.Checked)
		{
			if (!this.allowItemCopy)
			{
				this.ConfigurationChanged = this.allowItemCopy = true;
			}
		}
		else
		{
			if (this.allowItemCopy)
			{
				this.allowItemCopy = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the load last character check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadLastCharacterCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.loadLastCharacterCheckBox.Checked)
		{
			if (!this.loadLastCharacter)
			{
				this.ConfigurationChanged = this.loadLastCharacter = true;
			}
		}
		else
		{
			if (this.loadLastCharacter)
			{
				this.loadLastCharacter = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the load last vault check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadLastVaultCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.loadLastVaultCheckBox.Checked)
		{
			if (!this.loadLastVault)
			{
				this.ConfigurationChanged = this.loadLastVault = true;
			}
		}
		else
		{
			if (this.loadLastVault)
			{
				this.loadLastVault = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the detect language check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void DetectLanguageCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.detectLanguageCheckBox.Checked)
		{
			if (!this.detectLanguage)
			{
				this.languageComboBox.Enabled = false;
				this.ConfigurationChanged = this.detectLanguage = true;

				// Force TQVault to restart to autodetect the language
				this.LanguageChanged = true;
			}
		}
		else
		{
			if (this.detectLanguage)
			{
				this.detectLanguage = false;
				this.ConfigurationChanged = this.languageComboBox.Enabled = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the detect game paths check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void DetectGamePathsCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.detectGamePathsCheckBox.Checked)
		{
			if (!this.detectGamePath)
			{
				this.titanQuestPathTextBox.Enabled = this.immortalThronePathTextBox.Enabled
					= this.titanQuestPathBrowseButton.Enabled = this.immortalThronePathBrowseButton.Enabled = false;

				// Force TQVault to restart to autodetect the game path
				this.GamePathChanged = this.ConfigurationChanged = this.detectGamePath = true;
			}
		}
		else
		{
			if (this.detectGamePath)
			{
				this.detectGamePath = false;
				this.ConfigurationChanged = this.immortalThronePathTextBox.Enabled = this.titanQuestPathTextBox.Enabled
					= this.titanQuestPathBrowseButton.Enabled = this.immortalThronePathBrowseButton.Enabled = true;
			}
		}
	}

	/// <summary>
	/// Handler for changing the language selection
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LanguageComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		// There was some problem getting the game language so we ignore changing it.
		var gl = Database.GameLanguage;
		if (gl == null)
			return;

		this.titanQuestLanguage = (this.languageComboBox.SelectedItem as ComboBoxItem)?.Value ?? string.Empty;
		if (!this.titanQuestLanguage.Equals(gl, StringComparison.OrdinalIgnoreCase))
			this.LanguageChanged = true;

		this.ConfigurationChanged = true;
	}

	/// <summary>
	/// Handler for leaving the vault text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultPathTextBoxLeave(object? sender, EventArgs e)
	{
		if (!this.VaultPath.Equals(this.vaultPathTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))
		{
			this.VaultPath = this.vaultPathTextBox.Text.Trim();
			this.vaultPathTextBox.Invalidate();
			this.ConfigurationChanged = this.VaultPathChanged = true;
		}
	}

	/// <summary>
	/// Handler for leaving the Titan Quest game path text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void TitanQuestPathTextBoxLeave(object? sender, EventArgs e)
	{
		if (!this.titanQuestPath.Equals(this.titanQuestPathTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))
		{
			this.titanQuestPath = this.titanQuestPathTextBox.Text.Trim();
			this.titanQuestPathTextBox.Invalidate();
			this.ConfigurationChanged = this.GamePathChanged = true;
		}
	}

	/// <summary>
	/// Handler for leaving the Immortal Throne game path text box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ImmortalThronePathTextBoxLeave(object? sender, EventArgs e)
	{
		if (!this.immortalThronePath.Equals(this.immortalThronePathTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))
		{
			this.immortalThronePath = this.immortalThronePathTextBox.Text.Trim();
			this.immortalThronePathTextBox.Invalidate();
			this.ConfigurationChanged = this.GamePathChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking the Titan Quest game path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void TitanQuestPathBrowseButtonClick(object? sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseTQ;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
		this.folderBrowserDialog.SelectedPath = this.titanQuestPath;
		this.folderBrowserDialog.ShowDialog();
		if (!this.folderBrowserDialog.SelectedPath.Trim().Equals(this.titanQuestPath.Trim(), StringComparison.OrdinalIgnoreCase))
		{
			this.titanQuestPath = this.folderBrowserDialog.SelectedPath.Trim();
			this.titanQuestPathTextBox.Text = this.titanQuestPath;
			this.titanQuestPathTextBox.Invalidate();
			this.ConfigurationChanged = this.GamePathChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking the Immortal Throne game path browse button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ImmortalThronePathBrowseButtonClick(object? sender, EventArgs e)
	{
		this.folderBrowserDialog.Description = Resources.SettingsBrowseIT;
		this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
		this.folderBrowserDialog.SelectedPath = this.immortalThronePath;
		this.folderBrowserDialog.ShowDialog();
		if (!this.folderBrowserDialog.SelectedPath.Trim().Equals(this.immortalThronePath.Trim(), StringComparison.OrdinalIgnoreCase))
		{
			this.immortalThronePath = this.folderBrowserDialog.SelectedPath.Trim();
			this.immortalThronePathTextBox.Text = this.immortalThronePath;
			this.immortalThronePathTextBox.Invalidate();
			this.ConfigurationChanged = this.GamePathChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking enable custom maps
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void EnableCustomMapsCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.enableCustomMapsCheckBox.Checked)
		{
			if (!this.enableMods)
			{
				this.mapListComboBox.Enabled = this.CustomMapsChanged = this.enableMods = this.ConfigurationChanged = true;
			}
		}
		else
		{
			if (this.enableMods)
			{
				this.enableMods = this.mapListComboBox.Enabled = false;
				this.ConfigurationChanged = this.CustomMapsChanged = true;
				this.customMap = string.Empty;// Reset value
			}
		}
	}

	/// <summary>
	/// Handler for changing the selected custom map
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MapListComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		if (!this.settingsLoaded)
			return;

		var custommap = (this.mapListComboBox.SelectedItem as GamePathEntry)?.Path ?? string.Empty;
		if (custommap != USettings.CustomMap)
		{
			this.customMap = custommap;
			this.ConfigurationChanged = this.CustomMapsChanged = true;
		}
	}

	/// <summary>
	/// Handler for clicking the load all files check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void LoadAllFilesCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.loadAllFilesCheckBox.Checked)
		{
			if (!this.loadAllFiles)
			{
				this.loadAllFiles = this.ConfigurationChanged = true;
			}
		}
		else
		{
			if (this.loadAllFiles)
			{
				this.loadAllFiles = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	/// <summary>
	/// Handler for clicking the suppress warnings check box
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SuppressWarningsCheckBoxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.suppressWarningsCheckBox.Checked)
		{
			if (!this.suppressWarnings)
			{
				this.suppressWarnings = this.ConfigurationChanged = true;
			}
		}
		else
		{
			if (this.suppressWarnings)
			{
				this.suppressWarnings = false;
				this.ConfigurationChanged = true;
			}
		}
	}

	private void PlayerReadonlyCheckboxCheckedChanged(object? sender, EventArgs e)
	{
		if (this.playerReadonlyCheckbox.Checked)
		{
			if (!this.playerReadonly)
			{
				this.playerReadonly = this.ConfigurationChanged = true;
			}
		}
		else
		{
			if (this.playerReadonly)
			{
				this.playerReadonly = false;
				this.ConfigurationChanged = true;
			}
		}

	}

	private void CharacterEditCheckBox_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.characterEditCheckBox.Checked)
		{
			if (!this.allowCharacterEdit)
			{
				this.allowCharacterEdit = this.ConfigurationChanged = this.EnableCharacterEditChanged = true;
			}
		}
		else
		{
			if (this.allowCharacterEdit)
			{
				this.allowCharacterEdit = false;
				this.ConfigurationChanged = this.EnableCharacterEditChanged = true;
			}
		}
	}

	private void FontComboBoxBase_SelectedIndexChanged(object? sender, EventArgs e)
	{
		var font = this.baseFontComboBox.SelectedItem as ComboBoxItem;
		if (font == null)
			return;

		if (font.Value != USettings.BaseFont)
		{
			this.BaseFont = font.Value;
			this.ConfigurationChanged = this.UISettingChanged = true;// Force restart
		}
	}

	private void checkGroupBoxTooltipEnhanced_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.checkGroupBoxTooltipEnhanced.Checked != this.enableDetailedTooltipView)
		{
			this.enableDetailedTooltipView = this.checkGroupBoxTooltipEnhanced.Checked;
			this.ConfigurationChanged = true;
		}
	}

	private void scalingCheckBoxTooltipInGameRolls_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.scalingCheckBoxTooltipInGameRolls.Checked != this.tooltipShowInGameRolls)
		{
			this.tooltipShowInGameRolls = this.scalingCheckBoxTooltipInGameRolls.Checked;
			this.ConfigurationChanged = true;
		}
	}

	private void scalingCheckBoxTooltipValueRanges_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.scalingCheckBoxTooltipValueRanges.Checked != this.tooltipShowValueRanges)
		{
			this.tooltipShowValueRanges = this.scalingCheckBoxTooltipValueRanges.Checked;
			this.ConfigurationChanged = true;
		}
	}

	private void scalingCheckBoxTooltipGrantedSkills_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.scalingCheckBoxTooltipGrantedSkills.Checked != this.tooltipShowGrantedSkills)
		{
			this.tooltipShowGrantedSkills = this.scalingCheckBoxTooltipGrantedSkills.Checked;
			this.ConfigurationChanged = true;
		}
	}

	private void scalingCheckBoxTooltipSetPieceCounts_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.scalingCheckBoxTooltipSetPieceCounts.Checked != this.tooltipShowSetPieceCounts)
		{
			this.tooltipShowSetPieceCounts = this.scalingCheckBoxTooltipSetPieceCounts.Checked;
			this.ConfigurationChanged = true;
		}
	}

	private void ShowInGameValuesCheckBox_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.ShowInGameValuesCheckBox.Checked != this.showInGameValues)
		{
			this.showInGameValues = this.ShowInGameValuesCheckBox.Checked;
			this.ConfigurationChanged = true;
		}
	}

	private void EnableItemRequirementRestrictionCheckBox_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.EnableItemRequirementRestrictionCheckBox.Checked)
		{
			if (!this.enableItemRequirementRestriction)
				this.enableItemRequirementRestriction = this.ConfigurationChanged = this.EnableItemRequirementRestrictionChanged = true;
		}
		else
		{
			if (this.enableItemRequirementRestriction)
			{
				this.enableItemRequirementRestriction = false;
				this.ConfigurationChanged = this.EnableItemRequirementRestrictionChanged = true;
			}
		}
	}

	private void hotReloadCheckBox_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.hotReloadCheckBox.Checked && !this.enableHotReload)
		{
			this.enableHotReload = this.ConfigurationChanged = this.EnableHotReloadChanged = this.UISettingChanged = true;// Force restart
			return;
		}

		if (!this.hotReloadCheckBox.Checked && this.enableHotReload)
		{
			this.enableHotReload = false;
			this.ConfigurationChanged = this.EnableHotReloadChanged = this.UISettingChanged = true;// Force restart
		}
	}

	private void scalingCheckBoxEnableEpicLegendaryAffixes_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked && !this.EnableEpicLegendaryAffixes)
		{
			this.EnableEpicLegendaryAffixes = this.ConfigurationChanged = true;
			return;
		}

		if (!this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked && this.EnableEpicLegendaryAffixes)
		{
			this.EnableEpicLegendaryAffixes = false;
			this.ConfigurationChanged = true;
		}
	}

	private void scalingCheckBoxEnableSounds_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.scalingCheckBoxEnableSounds.Checked)
		{
			if (!this.enableTQVaultSounds)
				this.enableTQVaultSounds = this.ConfigurationChanged = this.EnableTQVaultSoundsChanged = true;
		}
		else
		{
			if (this.enableTQVaultSounds)
			{
				this.enableTQVaultSounds = false;
				this.ConfigurationChanged = this.EnableTQVaultSoundsChanged = true;
			}
		}
	}

	private void scalingComboBoxCSVDelim_SelectedIndexChanged(object? sender, EventArgs e)
	{
		this.ConfigurationChanged = true;
	}

	private void scalingCheckBoxDisableAutoStacking_CheckedChanged(object? sender, EventArgs e)
	{

		if (this.scalingCheckBoxDisableAutoStacking.Checked)
		{
			if (!this.disableAutoStacking)
				this.disableAutoStacking = this.ConfigurationChanged = this.DisableAutoStackingChanged = true;
			return;
		}

		if (this.disableAutoStacking)
		{
			this.disableAutoStacking = false;
			this.ConfigurationChanged = this.DisableAutoStackingChanged = true;
		}
	}

	private void checkGroupBoxCheats_CheckedChanged(object? sender, EventArgs e)
	{

		if (this.checkGroupBoxAllowCheats.Checked)
		{
			if (!this.allowCheats)
				this.allowCheats = this.ConfigurationChanged = this.AllowCheatsChanged = true;

			return;
		}

		// Unchecked
		if (this.allowCheats)
		{
			this.allowCheats = false;
			this.ConfigurationChanged = this.AllowCheatsChanged = true;
		}

		// Adjust content
		this.characterEditCheckBox.Checked =
		this.allowItemEditCheckBox.Checked =
		this.allowItemCopyCheckBox.Checked = false;
		//this.scalingCheckBoxEnableEpicLegendaryAffixes.Checked = false;// Triggered by allowItemEditCheckBox
	}

	private void checkGroupBoxGitBackup_CheckedChanged(object? sender, EventArgs e)
	{
		if (checkGroupBoxGitBackup.Checked)
		{
			if (!this.enableGitBackup)
				this.enableGitBackup = this.ConfigurationChanged = this.UISettingChanged = this.GitBackupEnabledChanged = true;
			return;
		}

		// Unchecked
		if (this.enableGitBackup)
		{
			this.enableGitBackup = false;
			this.ConfigurationChanged = this.UISettingChanged = this.GitBackupEnabledChanged = true;
		}

		this.scalingCheckBoxDisableLegacyBackup.Checked = false;// Restore legacy backup
	}

	private void scalingCheckBoxDisableLegacyBackup_CheckedChanged(object? sender, EventArgs e)
	{

		if (scalingCheckBoxDisableLegacyBackup.Checked)
		{
			if (!this.disableLegacyBackup)
				this.disableLegacyBackup = this.ConfigurationChanged = this.UISettingChanged = this.DisableLegacyBackupChanged = true;
			return;
		}

		// Unchecked
		if (this.disableLegacyBackup)
		{
			this.disableLegacyBackup = false;
			this.ConfigurationChanged = this.UISettingChanged = this.DisableLegacyBackupChanged = true;
		}
	}

	private void scalingCheckBoxBackupPlayerSaves_CheckedChanged(object? sender, EventArgs e)
	{
		if (scalingCheckBoxBackupPlayerSaves.Checked)
		{
			if (!this.enableBackupPlayerSaves)
				this.enableBackupPlayerSaves = this.ConfigurationChanged = this.UISettingChanged = this.EnableBackupPlayerSavesChanged = true;
			return;
		}

		// Unchecked
		if (this.enableBackupPlayerSaves)
		{
			this.enableBackupPlayerSaves = false;
			this.ConfigurationChanged = this.UISettingChanged = this.EnableBackupPlayerSavesChanged = true;
		}
	}

	private void checkGroupBoxOriginalTQSupport_CheckedChanged(object? sender, EventArgs e)
	{
		if (checkGroupBoxOriginalTQSupport.Checked)
		{
			if (!this.enableOriginalTQSupport)
				this.enableOriginalTQSupport = this.ConfigurationChanged = this.UISettingChanged = this.EnableOriginalTQSupportChanged = true;
			return;
		}

		// Unchecked
		if (this.enableOriginalTQSupport)
		{
			this.enableOriginalTQSupport = false;
			this.ConfigurationChanged = this.UISettingChanged = this.EnableOriginalTQSupportChanged = true;
		}
	}

	private void PasteBinExpirationComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		if (this.pasteBinExpirationComboBox.SelectedItem is PasteBinExpirationComboBoxItem item)
		{
			string apiValue = item.Key;
			if (apiValue != this.pasteBinExpiration)
			{
				this.pasteBinExpiration = apiValue;
				this.ConfigurationChanged = this.PasteBinSettingsChanged = true;
			}
		}
	}
}
