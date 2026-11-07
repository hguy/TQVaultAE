//-----------------------------------------------------------------------
// <copyright file="VaultPanel.ContextMenu.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Represents a TQ Vault control that displays a frame around a group of TQ Vault panels with an optional caption.
/// </summary>

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

public partial class VaultPanel
{
	private (char charDelimiter, CsvDelimiter csvDelimiter) GetCSVDelimiter()
	{
		var delimStr = this.USettings.CSVDelimiter;

		char delim = ',';

		if (!EnumsNET.Enums.TryParse<CsvDelimiter>(delimStr, out var delimEnum))
			delimEnum = CsvDelimiter.Comma;

		delim = (EnumsNET.Enums.AsString(delimEnum, EnumsNET.EnumFormat.Description) ?? ",").First();

		return (delim, delimEnum);
	}

	private void ExportVaultCSVToClipboard(object? sender, EventArgs e)
	{
		var (delim, _) = GetCSVDelimiter();

		var csvRows = (this.ShownVault.Sacks ?? []).SelectMany((sack, idx)
				=> sack.Select(itm => (itm, bagid: idx + 1, vaultname: this.ShownVault.PlayerName))
			)
			.Select((vt, idx) =>
			{
				var fnr = this.ItemProvider.GetFriendlyNames(vt.itm, FriendlyNamesExtraScopes.Requirements);
				return new CsvRow(vt.vaultname, vt.bagid, fnr, idx, delim).ToString();
			}).ToList();

		csvRows.Insert(0, CsvRow.GetCSVHeader(delim));

		var export = string.Join(Environment.NewLine, csvRows);

		Clipboard.SetText(export, TextDataFormat.UnicodeText);

		this.UIService.NotifyUser(string.Format(Resources.CSVExportedToClipboardNotification, DateTime.Now));
	}

	private void ExportBagCSVToClipboard(object? sender, EventArgs e)
	{
		var (delim, _) = GetCSVDelimiter();

		var csvRows = this.BagSackPanel.ShownSack.Select(itm => (itm, bagid: this.BagSackPanel.CurrentSack + 1, vaultname: this.ShownVault.PlayerName))
			.Select((vt, idx) =>
			{
				var fnr = this.ItemProvider.GetFriendlyNames(vt.itm, FriendlyNamesExtraScopes.Requirements);
				return new CsvRow(vt.vaultname, vt.bagid, fnr, idx, delim).ToString();
			}).ToList();

		csvRows.Insert(0, CsvRow.GetCSVHeader(delim));

		var export = string.Join(Environment.NewLine, csvRows);

		Clipboard.SetText(export, TextDataFormat.UnicodeText);

		this.UIService.NotifyUser(string.Format(Resources.CSVExportedToClipboardNotification, DateTime.Now));
	}

	private void DisableTooltipClicked(object? sender, EventArgs e)
	{
		ToolStripMenuItem? item = (ToolStripMenuItem)sender!;

		if (item != null)
		{
			if (item.Text == Resources.PlayerPanelMenuEnableAllTooltip)
			{
				this.ShownVault.DisabledTooltipBagId.Clear();
			}

			if (item.Text == Resources.PlayerPanelMenuDisableAllTooltip)
			{
				var all = this.BagButtons.Select(b => b.ButtonNumber).ToArray();
				this.ShownVault.DisabledTooltipBagId.Clear();
				this.ShownVault.DisabledTooltipBagId.AddRange(all);
			}

			if (item.Text == Resources.PlayerPanelMenuEnableTooltip)
			{
				this.ShownVault.DisabledTooltipBagId.Remove(this.CurrentBag);
			}

			if (item.Text == Resources.PlayerPanelMenuDisableTooltip)
			{
				this.ShownVault.DisabledTooltipBagId.Add(this.CurrentBag);
			}
		}
	}

	private void AddExportTabSubMenu()
	{
		var hasApiKey = this.ExchangeService?.HasPasteBinApiKey == true;

		var clipboardItem = new ToolStripMenuItem(
			Resources.PlayerPanelMenuExportTabClipboard, null,
			this.ExportTabToClipboardClicked)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
		};

		var fileItem = new ToolStripMenuItem(
			Resources.PlayerPanelMenuExportTabFile, null,
			this.ExportTabToFileClicked)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
		};

		var pasteBinItem = new ToolStripMenuItem(
			Resources.PlayerPanelMenuExportTabPasteBin, null,
			this.ExportTabToPasteBinClicked)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
			Enabled = hasApiKey
		};

		var exportTabMenu = new ToolStripMenuItem(
			Resources.PlayerPanelMenuExportTab, null,
			clipboardItem, fileItem, pasteBinItem)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		};

		this.contextMenu.Items.Add(exportTabMenu);
	}

	private void ExportTabToClipboardClicked(object? sender, EventArgs e)
	{
		var exchangeService = this.ExchangeService;
		if (exchangeService != null && this.BagSackPanel?.Sack != null)
		{
			var json = exchangeService.SerializeSackCollection(this.BagSackPanel.Sack, this.CurrentBag);
			Clipboard.SetText(json);
		}
	}

	private void ExportTabToFileClicked(object? sender, EventArgs e)
	{
		var exchangeService = this.ExchangeService;
		if (exchangeService != null && this.BagSackPanel?.Sack != null)
		{
			var json = exchangeService.SerializeSackCollection(this.BagSackPanel.Sack, this.CurrentBag);
			using var dialog = new SaveFileDialog
			{
				Filter = "JSON files (*.json)|*.json",
				DefaultExt = "json",
				FileName = "tab_export.json"
			};

			if (dialog.ShowDialog() == DialogResult.OK)
				File.WriteAllText(dialog.FileName, json);
		}
	}

	private async void ExportTabToPasteBinClicked(object? sender, EventArgs e)
	{
		var exchangeService = this.ExchangeService;
		if (exchangeService != null && this.BagSackPanel?.Sack != null)
		{
			try
			{
				var json = exchangeService.SerializeSackCollection(this.BagSackPanel.Sack, this.CurrentBag);
				var tabName = this.BagSackPanel.Sack.BagButtonIconInfo?.Label;
				var pasteName = string.IsNullOrWhiteSpace(tabName)
					? $"{this.ShownVault.PlayerName}/Tab {this.CurrentBag + 1}"
					: $"{this.ShownVault.PlayerName}/{tabName}";
				var url = await exchangeService.ExportToPasteBinAsync(json, pasteName);
				Clipboard.SetText(url);
				this.UIService.NotifyUser($"Tab exported to PasteBin: {url}");
			}
			catch (Exception ex)
			{
				this.UIService.ShowError($"Failed to export to PasteBin: {ex.Message}");
			}
		}
	}

	/// <summary>
	/// Gets the index from the menu string using the delimiter in the resources since the strings are now regionalized.
	/// Helper for the context menu.
	/// </summary>
	/// <param name="selectedItem">string of the item selected in the menu</param>
	/// <returns>numeric index of the item selected.  -1 if it fails.</returns>
	private static int GetDestinationSackIndex(string? selectedItem)
	{
		if (string.IsNullOrEmpty(selectedItem))
		{
			return -1;
		}

		int hashSign = Resources.GlobalMenuBag.IndexOf(Resources.GlobalMenuBagDelimiter, StringComparison.Ordinal) + 1;

		if (hashSign == -1)
		{
			return -1;
		}

		return Convert.ToInt32(selectedItem.Substring(hashSign), CultureInfo.InvariantCulture) - 1;
	}

	/// <summary>
	/// Creates a context menu submenu with choices for each bag in the panel
	/// except for the currently selected bag.
	/// </summary>
	/// <param name="menuText">Text label for the sub menu</param>
	/// <param name="menuCallback">handler for the submenu</param>
	private void AddSubMenu(string menuText, EventHandler menuCallback)
	{
		var menuChoices = new ToolStripItem[this.BagButtons.Count - 1];
		int menuIndex = 0;
		for (int i = 0; i < this.BagButtons.Count; i++)
		{
			if (i != this.CurrentBag)
			{
				int val = i + 1;
				menuChoices[menuIndex] = new ToolStripMenuItem(string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, val)
					, null, menuCallback, string.Format(CultureInfo.CurrentCulture, Resources.GlobalMenuBag, val))
				{
					BackColor = this.contextMenu.BackColor,
					Font = this.contextMenu.Font,
					ForeColor = this.contextMenu.ForeColor,
					Tag = new MenuItemTagBagIndex(i),
				};
				menuIndex++;
			}
		}

		var subMenu = new ToolStripMenuItem(menuText, null, menuChoices)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
			DisplayStyle = ToolStripItemDisplayStyle.Text
		};

		this.contextMenu.Items.Add(subMenu);
	}

	private void AddMenuItem(string menuText, EventHandler menuCallback)
	{
		var subMenu = new ToolStripMenuItem(menuText, null, menuCallback)
		{
			BackColor = this.contextMenu.BackColor,
			Font = this.contextMenu.Font,
			ForeColor = this.contextMenu.ForeColor,
			DisplayStyle = ToolStripItemDisplayStyle.Text,
		};

		this.contextMenu.Items.Add(subMenu);
	}

	/// <summary>
	/// Handler for clicking on the context menu
	/// </summary>
	/// <param name="sender">semder object</param>
	/// <param name="e">ToolStripItemClickedEventArgs data</param>
	private void ContextMenuItemClicked(object? sender, ToolStripItemClickedEventArgs e)
	{
		string? selectedItem = e.ClickedItem?.Text;
		if (selectedItem == Resources.PlayerPanelMenuEmpty)
		{
			if (this.USettings.SuppressWarnings || MessageBox.Show(
				Resources.PlayerPanelEmptyMsg,
				Resources.PlayerPanelEmpty,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1,
				PlayerPanel.RightToLeftOptions) == DialogResult.Yes)
			{
				this.BagSackPanel.ShownSack.EmptySack();
				this.BagSackPanel.Refresh();
				BagButtonTooltip.InvalidateCache(this.BagSackPanel.Sack);
			}
		}

		if (selectedItem == Resources.PlayerPanelChangeIcon)
		{
			var button = this.BagButtons[this.CurrentBag];

			var bbs = this.Services.Forms.Create<BagButtonSettings>();
			bbs.CurrentBagButton = button;

			if (bbs.ShowDialog() == DialogResult.OK)
			{
				button.Refresh();
			}
		}

		if (selectedItem == Resources.PlayerPanelCopyIcon)
		{
			var button = this.BagButtons[this.CurrentBag];
			var sourceInfo = button.Sack?.BagButtonIconInfo;

			// Create a new instance (deep copy) to avoid shared reference issues
			if (sourceInfo is not null)
			{
				this.userContext.IconInfoCopy = new BagButtonIconInfo
				{
					DisplayMode = sourceInfo.DisplayMode,
					Label = sourceInfo.Label,
					OnStr = sourceInfo.OnStr,
					OffStr = sourceInfo.OffStr,
					OverStr = sourceInfo.OverStr
				};
			}
			else
			{
				this.userContext.IconInfoCopy = null;
			}

			if (this.FindForm() is MainForm frm)
				frm.NotificationText.Text = Resources.GlobalCopied;
		}

		if (selectedItem == Resources.PlayerPanelPasteIcon)
		{
			var button = this.BagButtons[this.CurrentBag];

			// Create a new instance to avoid shared reference issues
			var sourceInfo = this.userContext.IconInfoCopy;
			if (sourceInfo is not null)
			{
				if (button.Sack is null)
					return;

				button.Sack.BagButtonIconInfo = new BagButtonIconInfo
				{
					DisplayMode = sourceInfo.DisplayMode,
					Label = sourceInfo.Label,
					OnStr = sourceInfo.OnStr,
					OffStr = sourceInfo.OffStr,
					OverStr = sourceInfo.OverStr
				};
			}
			else
			{
				if (button.Sack is not null)
					button.Sack.BagButtonIconInfo = null;
			}

			if (button.Sack is not null)
				button.Sack.IsModified = true;
			if (this.FindForm() is MainForm frm)
				frm.NotificationText.Text = Resources.GlobalPasted;
			button.ApplyIconInfo(button.Sack);
			button.Refresh();
		}

	}

	/// <summary>
	/// Handler for clicking on Copy Bag from the context menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CopyBagClicked(object? sender, EventArgs e)
	{
		ToolStripMenuItem? item = (ToolStripMenuItem)sender!;
		if (item != null)
		{
			int destinationIndex = GetDestinationSackIndex(item.Name);

			if (destinationIndex > this.ShownPlayer.NumberOfSacks)
			{
				return;
			}

			if (!(this.ShownPlayer.GetSack(destinationIndex + this.BagPanelOffset)?.IsEmpty ?? true))
			{
				if (this.USettings.SuppressWarnings || MessageBox.Show(
					Resources.PlayerOverwriteSackMsg,
					Resources.PlayerOverwriteSack,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions) != DialogResult.Yes)
				{
					return;
				}
			}

			if (this.ShownPlayer.CopySack(this.CurrentBag, destinationIndex))
			{
				if (this.CurrentBag != destinationIndex)
				{
					// turn off the current bag and turn on the new bag
					this.BagButtons[this.CurrentBag].IsOn = false;
					this.BagButtons[destinationIndex].IsOn = true;
					this.CurrentBag = destinationIndex;
					this.BagSackPanel.Sack = this.ShownPlayer.GetSack(this.CurrentBag + this.BagPanelOffset);
					this.BagSackPanel.CurrentSack = this.CurrentBag + this.BagPanelOffset;
				}
			}
		}
	}

	/// <summary>
	/// Handler for clicking move bag from the context menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MoveBagClicked(object? sender, EventArgs e)
	{
		ToolStripMenuItem? item = (ToolStripMenuItem)sender!;
		if (item != null)
		{
			int destinationIndex = GetDestinationSackIndex(item.Name);
			if (this.ShownPlayer.MoveSack(this.CurrentBag, destinationIndex))
			{
				if (this.CurrentBag != destinationIndex)
				{
					// turn off the current bag and turn on the new bag
					this.BagButtons[this.CurrentBag].IsOn = false;
					this.BagButtons[destinationIndex].IsOn = true;
					this.CurrentBag = destinationIndex;
					this.BagSackPanel.Sack = this.ShownPlayer.GetSack(this.CurrentBag + this.BagPanelOffset);
					this.BagSackPanel.CurrentSack = this.CurrentBag + this.BagPanelOffset;
				}

				ApplyCustomButtonIcons();
				Refresh();
			}
		}
	}

	/// <summary>
	/// Handler for clicking Merge Bag on the context menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MergeBagClicked(object? sender, EventArgs e)
	{
		ToolStripMenuItem? item = (ToolStripMenuItem)sender!;
		if (item != null)
		{
			int destinationIndex = GetDestinationSackIndex(item.Name);

			if (destinationIndex < 0 || destinationIndex > this.ShownPlayer.NumberOfSacks || this.CurrentBag == destinationIndex)
			{
				return;
			}

			SackPanel dstSackPanel = this.BagSackPanel;
			if (dstSackPanel.MergeSack(destinationIndex))
			{
				// turn off the current bag and turn on the new bag
				this.BagButtons[this.CurrentBag].IsOn = false;
				this.BagButtons[destinationIndex].IsOn = true;
				this.CurrentBag = destinationIndex;
				this.BagSackPanel.Sack = this.ShownPlayer.GetSack(this.CurrentBag + this.BagPanelOffset);
				this.BagSackPanel.CurrentSack = this.CurrentBag + this.BagPanelOffset;
			}
		}
	}
}
