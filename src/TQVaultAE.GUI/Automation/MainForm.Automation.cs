//-----------------------------------------------------------------------
// <copyright file="MainForm.Automation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Config;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI;

/// <summary>
/// Debug-only read access to the main form state for the automation endpoint.
/// </summary>
public partial class MainForm
{
	internal ItemDragInfo AutomationDragInfo => this.DragInfo;

	internal UserSettings AutomationSettings => this.USettings;

	internal string? AutomationSelectedVault => this.vaultListComboBox?.SelectedItem?.ToString();

	internal string? AutomationSelectedSecondaryVault
		=> this.showSecondaryVault ? this.secondaryVaultListComboBox?.SelectedItem?.ToString() : null;

	internal string? AutomationSelectedCharacter => this.comboBoxCharacter?.SelectedItem?.ToString();

	internal IReadOnlyList<string> AutomationCharacters
		=> this.comboBoxCharacter?.Items.Select(i => i?.ToString() ?? string.Empty).ToList() ?? [];

	internal IReadOnlyList<string> AutomationVaults
		=> this.vaultListComboBox?.Items.Cast<object>().Select(i => i?.ToString() ?? string.Empty).ToList() ?? [];

	/// <summary>
	/// Selects a character like the user would in the drop down, which loads the player and its stashes.
	/// </summary>
	/// <returns>false when no character has that name.</returns>
	internal bool AutomationSelectCharacter(string name)
	{
		var item = this.comboBoxCharacter.Items.FirstOrDefault(i => string.Equals(i?.ToString(), name, StringComparison.OrdinalIgnoreCase));
		if (item is null)
			return false;

		this.comboBoxCharacter.SelectedItem = item;
		return true;
	}

	/// <summary>
	/// Selects a vault in the main vault drop down, which loads it.
	/// </summary>
	/// <returns>false when no vault has that name.</returns>
	internal bool AutomationSelectVault(string name)
	{
		var item = this.vaultListComboBox.Items.Cast<object>().FirstOrDefault(i => string.Equals(i?.ToString(), name, StringComparison.OrdinalIgnoreCase));
		if (item is null)
			return false;

		this.vaultListComboBox.SelectedItem = item;
		return true;
	}

	internal string? AutomationCurrentPlayerFile => this.userContext?.CurrentPlayer?.PlayerFile;

	internal bool AutomationLoadingComplete => this.loadingComplete;

	internal VaultPanel? AutomationVaultPanel => this.vaultPanel;

	internal VaultPanel? AutomationSecondaryVaultPanel => this.secondaryVaultPanel;

	internal PlayerPanel? AutomationPlayerPanel => this.playerPanel;

	internal StashPanel? AutomationStashPanel => this.stashPanel;

	internal ForgePanel? AutomationForgePanel => this.forgePanel;

	internal bool AutomationForgeShown => this.forgePanel?.Visible == true;

	/// <summary>
	/// Shows or hides the forge like the Forge button does, which also hides or restores the other panels.
	/// </summary>
	internal void AutomationShowForge(bool show)
	{
		if (show != this.AutomationForgeShown)
			this.scalingButtonForge_Click(this, EventArgs.Empty);
	}
}
