using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Services;
using TQVaultAE.Application;
using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	// Uses vaultService from MainForm.cs

	/// <summary>
	/// Creates the vault panel
	/// </summary>
	/// <param name="numBags">Number of bags in the vault panel.</param>
	private void CreateVaultPanel(int numBags)
	{
		this.vaultPanel = new VaultPanel(this.DragInfo, numBags, new Size(18, 20), 1, AutoMoveLocation.Vault, this.Services);

		this.vaultPanel.DrawAsGroupBox = false;

		this.vaultPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.vaultPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.vaultPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
		this.vaultPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.vaultPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.vaultPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

		this.tableLayoutPanelMain.Controls.Add(this.vaultPanel, 0, 1);
		this.vaultPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		this.vaultPanel.Margin = new Padding(0);
	}

	/// <summary>
	/// Creates the secondary vault panel.  Player panel needs to be created before this.
	/// </summary>
	/// <param name="numBags">Number of bags in the secondary vault panel.</param>
	private void CreateSecondaryVaultPanel(int numBags)
	{
		this.secondaryVaultPanel = new VaultPanel(this.DragInfo, numBags, new Size(18, 20), 1, AutoMoveLocation.SecondaryVault, this.Services);
		this.secondaryVaultPanel.DrawAsGroupBox = false;

		this.secondaryVaultPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.secondaryVaultPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.secondaryVaultPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
		this.secondaryVaultPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.secondaryVaultPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.secondaryVaultPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

		this.flowLayoutPanelRightPanels.Controls.Add(this.secondaryVaultPanel);
		this.secondaryVaultPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		this.secondaryVaultPanel.Margin = new Padding(0);
	}


	/// <summary>
	/// Gets a list of all available vault files and populates the drop down list.
	/// </summary>
	/// <param name="loadVault">Indicates whether the list will also load the last vault selected.</param>
	private void GetVaultList(bool loadVault)
	{
		string[] vaults = GamePathResolver.GetVaultList();

		// Added by VillageIdiot
		// See if the Vault path was set during GetVaultList and update the key accordingly
		if (GamePathResolver.VaultFolderChanged)
			this.vaultService.UpdateVaultPath(GamePathResolver.TQVaultSaveFolder);

		// There was something already selected so we will save it.
		string? currentVault = (this.vaultListComboBox.Items.Count > 0) ? this.vaultListComboBox.SelectedItemText() : VaultService.MAINVAULT;

		// Added by VillageIdiot
		// Clear the list before creating since this function can be called multiple times.
		this.vaultListComboBox.Items.Clear();

		this.vaultListComboBox.Items.Add(Resources.MainFormMaintainVault);

		// Add Main Vault first
		if (this.secondaryVaultListComboBox.SelectedItem == null || this.secondaryVaultListComboBox.SelectedItemText() != VaultService.MAINVAULT)
			this.vaultListComboBox.Items.Add(VaultService.MAINVAULT);

		if (vaults.Length > 0)
		{
			// now add everything EXCEPT for main vault
			foreach (string vault in vaults)
			{
				if (!vault.Equals(VaultService.MAINVAULT, StringComparison.Ordinal))
				{
					// Skip the vault open in the secondary panel; the ones after it stay in the list.
					if (this.secondaryVaultListComboBox.SelectedItem != null && vault.Equals(this.secondaryVaultListComboBox.SelectedItemText(), StringComparison.Ordinal) && this.showSecondaryVault)
						continue;

					this.vaultListComboBox.Items.Add(vault);
				}
			}
		}

		// See if we should load the last loaded vault
		if (base.USettings.LoadLastVault)
		{
			currentVault = base.USettings.LastVaultName;

			// Make sure there is something in the config file to load else load the Main Vault
			// We do not want to create new here.
			if (string.IsNullOrEmpty(currentVault) || !FileIO.Exists(GamePathResolver.GetVaultFile(currentVault)))
				currentVault = VaultService.MAINVAULT;
		}

		if (loadVault)
		{
			this.vaultListComboBox.SelectedItem = currentVault;

			// Finally load Vault
			this.LoadVault(currentVault, false);
		}
	}

	/// <summary>
	/// Reads the list from the main vault combo box.
	/// To support adding another vault panel to the screen.
	/// </summary>
	private void GetSecondaryVaultList()
	{

		// There was something already selected so we will save it.
		string? currentVault = (this.secondaryVaultListComboBox.Items.Count > 0) ? this.secondaryVaultListComboBox.SelectedItemText() : Resources.MainFormSelectVault;

		if (currentVault == this.vaultListComboBox.SelectedItemText())
			// Clear the selection if it is already loaded on the main panel.
			currentVault = Resources.MainFormSelectVault;

		// Clear the list before creating since this function can be called multiple times.
		this.secondaryVaultListComboBox.Items.Clear();
		this.secondaryVaultListComboBox.Items.Add(Resources.MainFormSelectVault);

		if (this.vaultListComboBox.Items.Count > 1)
		{
			// Now add everything EXCEPT for the selected vault in the other panel.
			for (int i = 1; i < this.vaultListComboBox.Items.Count; ++i)
			{ // Skip over the maintenance selection.
				if (i != this.vaultListComboBox.SelectedIndex)
					// Skip over the selected item.
					this.secondaryVaultListComboBox.Items.Add(this.vaultListComboBox.Items[i] ?? string.Empty);
			}
		}

		this.secondaryVaultListComboBox.SelectedItem = currentVault;

		// Finally load Vault
		this.LoadVault(currentVault, true);
	}

	/// <summary>
	/// Loads a vault file
	/// </summary>
	/// <param name="vaultName">Name of the vault</param>
	/// <param name="secondaryVault">flag indicating whether this selection is for the secondary panel</param>
	private void LoadVault(string vaultName, bool secondaryVault)
	{
		PlayerCollection? vault = null;
		if (secondaryVault && vaultName == Resources.MainFormSelectVault)
		{
			if (this.secondaryVaultPanel.Player != null)
				this.secondaryVaultPanel.Player = null;
		}
		else
		{
			// Use service
			var result = this.vaultService.LoadVault(vaultName);

			if (result.ArgumentException != null)
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.Filename, result.ArgumentException.Message);
				this.Services.MessageBoxes.Inform(msg, Resources.GlobalError, MessageBoxIcon.Error);
			}

			vault = result.Vault;
		}

		// Now assign the vault to the vaultpanel
		if (secondaryVault)
			this.secondaryVaultPanel.Player = vault;
		else
			this.vaultPanel.Player = vault;
	}

	/// <summary>
	/// Method for the maintain vault files dialog
	/// </summary>
	private void MaintainVaultFilesDialog()
	{
		try
		{
			this.SaveAllModifiedFiles();

			var dlg = this.Services.Forms.Create<VaultMaintenanceDialog>();
			using (ZoomScope.Enter())
				dlg.Scale(new SizeF(UIService.Zoom, UIService.Zoom));

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				string? newName = dlg.Target;
				string? oldName = dlg.Source;
				bool handled = false;

				// Create a new vault?
				if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.New && newName != null)
				{
					// Add the name to the list
					this.vaultListComboBox.Items.Add(newName);

					// Select it
					this.vaultListComboBox.SelectedItem = newName;

					// Load it
					this.LoadVault(newName, false);
					handled = true;
				}
				else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Copy && newName != null && oldName != null)
				{
					string oldFilename = GamePathResolver.GetVaultFile(oldName);
					string newFilename = GamePathResolver.GetVaultFile(newName);

					// Make sure we save all modifications first.
					this.SaveAllModifiedFiles();

					// Make sure the vault file to copy exists and the new name does not.
					if (FileIO.Exists(oldFilename) && !FileIO.Exists(newFilename))
					{
						FileIO.Copy(oldFilename, newFilename);

						// Add the new name to the list
						this.vaultListComboBox.Items.Add(newName);

						// Select the new name
						this.vaultListComboBox.SelectedItem = newName;

						// Load the new file.
						this.LoadVault(newName, false);
						handled = true;
					}
				}
				else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Delete && oldName != null)
				{
					string filename = GamePathResolver.GetVaultFile(oldName);

					// Make sure we save all modifications first.
					this.SaveAllModifiedFiles();

					// Make sure the vault file to delete exists.
					if (FileIO.Exists(filename))
					{
						FileIO.Delete(filename);
					}

					// Remove the file from the cache.
					userContext.Vaults.TryRemove(filename, out var remVault);

					// Remove the deleted file from the list.
					this.vaultListComboBox.Items.Remove(oldName);

					// Select the Main Vault since we know it's still there.
					this.vaultListComboBox.SelectedIndex = 1;

					handled = true;
				}
				else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Rename && newName != null && oldName != null)
				{
					string oldFilename = GamePathResolver.GetVaultFile(oldName);
					string newFilename = GamePathResolver.GetVaultFile(newName);

					// Make sure we save all modifications first.
					this.SaveAllModifiedFiles();

					// Make sure the vault file to rename exists and the new name does not.
					if (FileIO.Exists(oldFilename) && !FileIO.Exists(newFilename))
					{
						FileIO.Move(oldFilename, newFilename);

						// Remove the old vault from the cache.
						userContext.Vaults.TryRemove(oldFilename, out var remOldVault);

						// Get rid of the old name from the list
						this.vaultListComboBox.Items.Remove(oldName);

						// If we renamed something to main vault we need to remove it,
						// since the list always contains Main Vault.
						if (newName == VaultService.MAINVAULT)
						{
							userContext.Vaults.TryRemove(newFilename, out var remMainVault);
							this.vaultListComboBox.Items.Remove(newName);
						}

						// Add the new name to the list
						this.vaultListComboBox.Items.Add(newName);

						// Select the new name
						this.vaultListComboBox.SelectedItem = newName;

						// Load the new file.
						this.LoadVault(newName, false);
						handled = true;
					}
				}

				if ((newName == null && oldName == null) || !handled)
				{
					// put the vault back to what it was
					if (this.vaultPanel.Player != null)
					{
						this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
					}
				}
			}
			else
			{
				// put the vault back to what it was
				if (this.vaultPanel.Player != null)
				{
					this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
				}
			}
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "Vault maintenance failed (shown vault {Vault})", this.vaultPanel.Player?.PlayerName);
			this.Services.MessageBoxes.ShowException(exception, icon: MessageBoxIcon.None);
		}
	}


	/// <summary>
	/// Attempts to save all modified vault files
	/// </summary>
	private bool SaveAllModifiedVaults()
	{
		PlayerCollection? vaultOnError = null;
		return this.SaveWithRetry(() => this.vaultService.SaveAllModifiedVaults(ref vaultOnError) > 0, () => vaultOnError?.PlayerName);
	}

	/// <summary>
	/// Handler for changing the vault list drop down selection
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultListComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		try
		{
			if (this.vaultListComboBox.SelectedIndex == 0)
			{
				this.MaintainVaultFilesDialog();
			}
			else
			{
				string? vaultName = this.vaultListComboBox.SelectedItemText();
				if (this.vaultPanel.Player == null || !vaultName.Equals(this.vaultPanel.Player.PlayerName, StringComparison.Ordinal))
				{
					this.LoadVault(vaultName, false);
				}
			}

			if (this.showSecondaryVault)
			{
				this.GetSecondaryVaultList();
			}
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "Failed to load the vault {Vault}", this.vaultListComboBox.SelectedItemText());
			this.Services.MessageBoxes.ShowException(exception, icon: MessageBoxIcon.None);

			// put the vault back to what it was
			if (this.vaultPanel.Player != null)
			{
				this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
			}
		}
	}


	/// <summary>
	/// Handler for changing the secondary vault list drop down selection.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SecondaryVaultListComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		try
		{
			if (this.secondaryVaultListComboBox.SelectedIndex == 0)
			{
				// Clear the vault panel.
				if (this.secondaryVaultPanel.Player != null)
				{
					this.secondaryVaultPanel.Player = null;
				}
			}
			else
			{
				string? vaultName = this.secondaryVaultListComboBox.SelectedItemText();
				if (this.secondaryVaultPanel.Player == null || !vaultName.Equals(this.secondaryVaultPanel.Player.PlayerName, StringComparison.Ordinal))
				{
					this.LoadVault(vaultName, true);
				}
			}
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "Failed to load the secondary vault {Vault}", this.secondaryVaultListComboBox.SelectedItemText());
			this.Services.MessageBoxes.ShowException(exception, icon: MessageBoxIcon.None);

			// put the vault back to what it was
			if (this.secondaryVaultPanel.Player != null)
			{
				this.secondaryVaultListComboBox.SelectedItem = this.secondaryVaultPanel.Player.PlayerName;
			}
		}
	}
}
