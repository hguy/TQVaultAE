using System.Globalization;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Logs;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	private IReadOnlyCollection<string>? _watchedPlayerFiles = null;
	private IEnumerable<string> WatchedPlayerFiles
	{
		get
		{
			if (_watchedPlayerFiles is null)
				_watchedPlayerFiles = new List<string> { this.GamePathResolver.PlayerSaveFileName, this.GamePathResolver.PlayerStashFileNameB }.AsReadOnly();

			return _watchedPlayerFiles;
		}
	}

	/// <summary>
	/// Handler for changing the Character drop down selection.
	/// </summary>
	/// <param name="sender">Sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CharacterComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		// Hmm. We can load a character now!
		var selected = this.comboBoxCharacter.SelectedItem;
		var selectedSave = selected as PlayerSave;
		var selectedText = selected?.ToString() ?? string.Empty;

		// See if they actually changed their selection and ignore "No TQ characters detected"
		if (selectedText.Equals(Resources.MainFormSelectCharacter, StringComparison.Ordinal) || selectedText.Equals(Resources.MainFormNoCharacters, StringComparison.Ordinal)
			|| selectedText.Equals(Resources.MainFormNoCustomChars, StringComparison.Ordinal))
		{
			// no char selected
			this.ClearPlayer();
		}
		else if (selectedSave is not null)
		{
			this.LoadPlayerAndStashes(selectedSave);
		}
		this.Refresh();
	}

	/// <summary>
	/// Gets a list of available player files and populates the drop down list.
	/// </summary>
	private void GetPlayerList()
	{
		// Initialize the character combo-box
		this.comboBoxCharacter.Items.Clear();

		var characters = this.playerService.GetPlayerSaveList();

		// Init FileWatcher
		if (base.USettings.EnableHotReload)
		{
			foreach (var ps in characters)
			{
				ps.PlayerSaveWatcher = CreateFileWatcher(ps.Folder, this.GamePathResolver.PlayerSaveFileName);
				ps.PlayerStashWatcher = CreateFileWatcher(ps.Folder, this.GamePathResolver.PlayerStashFileNameB);
			}
		}

		if (characters.Length == 0)
			this.comboBoxCharacter.Items.Add(Resources.MainFormNoCharacters);
		else
		{
			this.comboBoxCharacter.Items.Add(Resources.MainFormSelectCharacter);

			foreach (var chara in characters)
				this.comboBoxCharacter.Items.Add(chara);
		}

		this.comboBoxCharacter.SelectedIndex = 0;
	}

	/// <summary>
	/// Coalesces the change events of the game save files (player, player stash, caravan and relic stash).
	/// </summary>
	private FileReloadDebouncer? _fileReloadDebouncer;
	private FileReloadDebouncer FileReloadDebouncer => this._fileReloadDebouncer ??= new FileReloadDebouncer(this.Log);

	// Raised on the UI thread (SynchronizingObject = this)
	private void PlayerSaveFile_Changed(object? sender, FileSystemEventArgs e)
	{
		if (e.ChangeType != WatcherChangeTypes.Changed
			|| !this.WatchedPlayerFiles.Any(f => f.Equals(e.Name, StringComparison.OrdinalIgnoreCase))
		) return;

		var fw = (FileSystemWatcher)sender!;
		var folder = fw.Path;
		var fileName = e.Name;

		this.FileReloadDebouncer.Schedule(e.FullPath, () =>
		{
			// retrieve PlayerSave
			var playerSave = this.comboBoxCharacter.Items.OfType<PlayerSave>().FirstOrDefault(ps => ps.Folder == folder);
			if (playerSave is null) return;

			// Reload player file
			PlayerLoadResult? playerResult = null;
			if (string.Equals(fileName, this.GamePathResolver.PlayerSaveFileName, StringComparison.OrdinalIgnoreCase))
				playerResult = this.LoadPlayer(playerSave, true);

			StashLoadResult? stashResult = null;
			if (string.Equals(fileName, this.GamePathResolver.PlayerStashFileNameB, StringComparison.OrdinalIgnoreCase))
				stashResult = this.LoadPlayerStash(playerSave, true);

			// Refresh if is current displayed character
			if (this.comboBoxCharacter.SelectedItem == playerSave)
			{
				if (playerResult is not null)
				{
					this.playerPanel.Player = playerResult.Player;
					this.stashPanel.Player = playerResult.Player;
				}

				if (stashResult is not null)
					this.stashPanel.Stash = stashResult.Stash;
			}
		});
	}


	private FileSystemWatcher CreateFileWatcher(string folder, string filename)
	{
		// the game write save file when inventory is opened and again when it's closed. We can't distinguish between both events.
		var fw = new FileSystemWatcher();
		fw.BeginInit();
		fw.Path = folder;
		fw.Filter = filename;
		fw.IncludeSubdirectories = false;
		fw.NotifyFilter = NotifyFilters.Attributes;// Attributes triggers after file update
		fw.SynchronizingObject = this;
		fw.Changed += new FileSystemEventHandler(this.PlayerSaveFile_Changed);
		fw.EnableRaisingEvents = true;
		fw.EndInit();
		return fw;
	}

	/// <summary>
	/// Creates the player panel
	/// </summary>
	private void CreatePlayerPanel()
	{
		this.playerPanel = new PlayerPanel(this.DragInfo, 4, new Size(12, 5), new Size(8, 5), this.Services);

		this.playerPanel.DrawAsGroupBox = false;

		this.playerPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
		this.playerPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
		this.playerPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
		this.playerPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
		this.playerPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
		this.playerPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);

		this.flowLayoutPanelRightPanels.Controls.Add(this.playerPanel);
		this.playerPanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
		this.playerPanel.Margin = new Padding(0);
	}



	/// <summary>
	/// Clears out the selected player
	/// Changed by VillageIdiot to a separate function.
	/// </summary>
	private void ClearPlayer()
	{
		if (this.playerPanel.Player != null)
		{
			this.playerPanel.Player = null;
			this.stashPanel.Player = null;
			this.stashPanel.CurrentBag = BagIdConstants.BAGID_EQUIPMENTPANEL;

			if (this.stashPanel.Stash != null)
				this.stashPanel.Stash = null;
		}
	}

	/// <summary>
	/// Loads a player using the drop down list.
	/// Assumes designators are appended to character name.
	/// Changed by VillageIdiot to a separate function.
	/// </summary>
	/// <param name="selectedSave">Player string from the drop down list.</param>
	/// <param name="fromFileWatcher">When <code>true</code> called from <see cref="FileSystemWatcher.Changed"/></param>
	/// <returns></returns>
	private (PlayerLoadResult PlayerResult, StashLoadResult? StashResult) LoadPlayerAndStashes(PlayerSave selectedSave, bool fromFileWatcher = false)
	{
		var result = LoadPlayer(selectedSave, fromFileWatcher);

		var resultStash = this.LoadPlayerStash(selectedSave, fromFileWatcher);

		if (selectedSave.IsImmortalThrone)
		{
			if (this.stashPanel.RelicVaultStash == null)
				LoadRelicVaultStash();

			if (this.stashPanel.TransferStash == null)
				LoadTransferStash();
		}
		else // Titan Quest original
		{
			this.stashPanel.RelicVaultStash = null;
			this.stashPanel.TransferStash = null;
		}

		return (result, resultStash);
	}

	private PlayerLoadResult LoadPlayer(PlayerSave selectedSave, bool fromFileWatcher)
	{
		// Use management service
		var resultDTO = this.playerService.LoadPlayer(selectedSave, fromFileWatcher);

		// Get the player
		try
		{
			if (resultDTO.Player?.ArgumentException != null)
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, resultDTO.PlayerFile, resultDTO.Player.ArgumentException.Message);
				this.Services.MessageBoxes.Inform(msg, Resources.GlobalError, MessageBoxIcon.Error);
			}

			if (!fromFileWatcher)
			{
				this.playerPanel.Player = resultDTO.Player;
				this.stashPanel.Player = resultDTO.Player;
				this.stashPanel.CurrentBag = BagIdConstants.BAGID_EQUIPMENTPANEL;
			}
			this.comboBoxCharacter.RefreshItem(selectedSave);
		}
		catch (IOException exception)
		{
			string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, resultDTO.PlayerFile, exception.ToString());
			this.Services.MessageBoxes.Inform(msg, Resources.MainFormPlayerReadError, MessageBoxIcon.Error);
			Log.LogError(exception, "{Message}", msg);
			this.playerPanel.Player = null;
			this.comboBoxCharacter.SelectedIndex = 0;
		}

		return resultDTO;
	}

	private StashLoadResult? LoadPlayerStash(PlayerSave selectedSave, bool fromFileWatcher = false)
	{
		// Get the player's stash - use management service

		// Only if it's IT, TQ doesn't have one
		if (!selectedSave.IsImmortalThrone)
		{
			this.stashPanel.Stash = null;
			return null;
		}

		var resultStash = this.stashService.LoadPlayerStash(selectedSave, fromFileWatcher);
		try
		{
			// Throw a message if the stash is not present.
			if (resultStash.Stash is { StashFound: false })
			{
				var msg = string.Concat(Resources.StashNotFoundMsg, "\n\nCharacter : ", selectedSave);
				this.Services.MessageBoxes.Inform(msg, Resources.StashNotFound, MessageBoxIcon.Information);
			}

			if (resultStash.Stash?.ArgumentException != null)
			{
				string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, resultStash.StashFile, resultStash.Stash.ArgumentException.Message);
				this.Services.MessageBoxes.Inform(msg, Resources.GlobalError, MessageBoxIcon.Error);
			}

			if (!fromFileWatcher) this.stashPanel.Stash = resultStash.Stash;
		}
		catch (IOException exception)
		{
			string msg = string.Concat(Resources.MainFormReadError, resultStash.StashFile, exception.ToString());
			this.Services.MessageBoxes.Inform(msg, Resources.MainFormStashReadError, MessageBoxIcon.Error);
			Log.LogError(exception, "{Message}", msg);
			this.stashPanel.Stash = null;
		}

		return resultStash;
	}

	/// <summary>
	/// Attempts to save all modified player files
	/// </summary>
	/// <returns>True if there were any modified player files.</returns>
	private bool SaveAllModifiedPlayers()
	{
	retry:
		bool saved = false;
		PlayerCollection? playerIfError = null;
		try
		{
			saved = this.playerService.SaveAllModifiedPlayers(ref playerIfError);
		}
		catch (IOException exception)
		{
			string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, playerIfError?.PlayerName);
			Log.LogError(exception, "{Message}", title);
			switch (this.Services.MessageBoxes.Ask(Log.FormatException(exception), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation))
			{
				case DialogResult.Abort:
					// rethrow the exception
					throw;
				case DialogResult.Retry:
					goto retry;
					//default: break; // DialogResult.Ignore
			}
		}
		return saved;
	}

	/// <summary>
	/// Attempt to duplicate a character
	/// </summary>
	private void DuplicateCharacter()
	{
#if DEBUG
		if (Automation.AutomationHost.IsSaveBlocked("duplicate character"))
			return;
#endif
		var ps = this.comboBoxCharacter.SelectedItem as PlayerSave;
		if (ps is not null)
		{

		askAgain:
			// Ask for new name
			var newname = InputDialog.Show(Resources.DuplicateCharacter_NewNameRequired, Resources.DuplicateCharacter_ModalTitle, "NewName").Trim();
			if (newname == string.Empty) return;// Cancel button

			// validate new name
			char[] invalidChars = this.PathIO.GetInvalidPathChars().Concat(this.PathIO.GetInvalidFileNameChars()).ToArray();
			if (string.IsNullOrWhiteSpace(newname) || newname.IndexOfAny(invalidChars) > -1)
			{
				this.Services.MessageBoxes.Inform(Resources.DuplicateCharacter_NewNameMustBeValid, Resources.DuplicateCharacter_ModalTitle, MessageBoxIcon.Error);
				goto askAgain;
			}

			var alreadyUsed = this.comboBoxCharacter.Items.OfType<PlayerSave>().Any(ps => newname.Equals(ps.Name, StringComparison.OrdinalIgnoreCase));
			if (alreadyUsed)
			{
				this.Services.MessageBoxes.Inform(Resources.DuplicateCharacter_NewNameAlreadyUsed, Resources.DuplicateCharacter_ModalTitle, MessageBoxIcon.Error);
				goto askAgain;
			}

			string? newFolder = null;
			try
			{
				// Duplicate File
				newFolder = this.GameFileService.DuplicateCharacterFiles(ps.Folder, newname);
				this.playerService.AlterNameInPlayerFileSave(newname, newFolder);
			}
			catch (Exception ex)
			{
				// Display Error
				Log.LogError(ex, "{Message}", Resources.DuplicateCharacter_Error);
				this.Services.MessageBoxes.ShowException(ex, Resources.DuplicateCharacter_Error, MessageBoxIcon.Error);
				// Rollback IO
				try
				{
					if (newFolder is not null) DirectoryIO.Delete(newFolder, true);
				}
				catch (Exception exx)
				{
					// log & mute
					Log.LogError(exx, "{Message}", Resources.DuplicateCharacter_Error);
				}
				return;
			}

			System.Windows.Forms.Application.Restart();
			return;
		}

		// Player must be selected
		this.Services.MessageBoxes.Inform(Resources.DuplicateCharacter_SourceRequired, Resources.DuplicateCharacter_ModalTitle, MessageBoxIcon.Warning);
	}
}
